// Sidebar accordion, workflow navigation menus for Comparion project | (C) AD v.220504

var _pgid_max_mod = 100000;
var _pgid_projectslist = 20001;
var _pgid_projects_active = 20001;
var _pgid_projects_archived = 20003;
var _pgid_projects_templates = 20005;
var _pgid_projects_master = 20006;

var _pgid_structure_alternatives = 20102;

var position_tab = "tab";
var position_workflow = "workflow";
var position_usermenu = "usermenu";
var sess_workflow_history_prefix = "worfklow_pages_";
var sess_tab_history_prefix = "tab_pages_";
var sess_session_id = "SID";

var menu_option_alts = "alts";
var menu_option_noAIP = "no_aip";
var menu_option_no_wrap = "no_wrap";
var menu_option_csdata = "csdata";

var opt_store_last_visited = true;
var opt_ignore_last_visted = ["30010", "50110"];
var opt_show_workflow_landing = true;
var opt_show_pgtitle_basic = false;
var opt_show_context_usermenu = true;
var opt_show_workflow_when_single = true;

var menu_switch_min_width = 800;
var menu_switch_auto = true;
var menu_switch_ignore = false;
var menu_switch_speed = 350;
var menu_switch_hover = false;

var menu_last = null;
var menu_active_tab = null;
var menu_active_workflow = null;
var menu_active_item = null;

var wkg_json = [];
var wkg_id = -1;
var nav_json = [];
var nav_layout = "";
var menu_last = [];
var showSidebar = false;

var use_aip = false;
var has_alts = false;
var has_csdata = false;
var opt_ask_disable_AIP = true;

var COLOR_EXTA_ICON_YELLOW = "#f4ab5e";

var _url_infopage = "/Info.aspx";
var _url_landingpage = "/Project/Landing.aspx";

//var swipe_start_x = 0;
//var swipe_start_y = 0;

(function ($) {
    $.widget("expertchoice.sidebarMenu", {

        options: {
            speedSubmenu: 200,
            speedPanel: 200,
            showDelay: 0,
            hideDelay: 0,
            singleOpen: true,
            clickEffect: true,
            setClicked: true,
            expandAll: false,
            //autoCollapse: true,
            //collapseMinWidth: 800,
            onClick: function () { },
            onExpand: function () { },
            onCollapse: function () { }
        },

        _create: function () {
        },

        _init: function () {
            this.init();
        },

        _setOption: function (key, value) {
            /*switch( key ) {
              case "speed":
                break;
            }*/
            this._super("_setOption", key, value);
        },

        _setOptions: function (options) {
            this._super(options);
        },

        destroy: function () {
            this.element.html("");
        },

        init: function () {
            this.is_init = true;
            this._initSubmenu();
            if ((this.element)) this.element.data("object", this);
            this._initSubmenuIndicators();
            this._initOnClick();
            if (this.options.clickEffect) this._initClickEffect();
            if (this.options.expandAll) this.expandAll();
            this.onActive();
            this.is_init = false;
        },

        onActive: function () {
            var act = $(this.element).find(".active");
            if ((act) && (act.length)) {
                var is_init = this.is_init;
                this.is_init = true;
                act.parents(".submenu").css('display', 'block');
                if (act.children.length) {
                    var o = $(act).data("object");
                    if ((o)) o._expandNode($(act), null); else act.find("ul").addClass("active");
                }
                this.is_init = is_init;
            }
        },

        expandAll: function () {
            var act = $(this.element).find(".submenu");
            if ((act) && (act.length)) {
                var is_init = this.is_init;
                this.is_init = true;
                act.css('display', 'block');
                if (act.children.length) {
                    var o = $(act).data("object");
                    if ((o)) o._expandNode($(act), null);// else act.find("ul").addClass("active");
                }
                $(".submenu-indicator").hide();
                this.is_init = is_init;
            }
        },

        _initSubmenu: function () {
            if (!this.options.expandAll) {
                $(this.element).children("ul").find("li").find("a").bind("click touch", function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    if ($(this).hasClass("menu-group")) return false;
                    var o = $(this).data("object");
                    if (((o) && o.is_init) || $(this).closest("ul").hasClass("submenu") || !$(this).parents().hasClass("collapsed")) {
                        if (!$(this).hasClass("menu-group") && $(this).children(".submenu").length > 0) {
                            o._expandNode($(this), e);
                        }
                    }
                })
            }
        },

        _expandNode: function (obj, e) {
            if (this.options.expandAll) return false;

            var o = obj.data("object");

            var showDelay = ((o) && !o.is_init ? o.options.showDelay : 0);
            var hideDelay = ((o) && !o.is_init ? o.options.hideDelay : 0);
            var speed = ((o) ? (o.is_init ? 0 : o.options.speedSubmenu) : 200);
            var singleOpen = ((o) ? o.options.singleOpen : true);

            if (obj.children(".submenu").css("display") == "none") {
                obj.children(".submenu").delay(showDelay).slideDown(speed);
                obj.children(".submenu").siblings("a").addClass("submenu-indicator-minus");
                if (singleOpen) {
                    obj.siblings().children(".submenu").slideUp(speed);
                    obj.siblings().children(".submenu").siblings("a").removeClass("submenu-indicator-minus")
                }
                if ((o) && (!o.is_init) && (typeof o.options.onExpand == "function")) {
                    o.options.onExpand(o, obj, e);
                }
                return false;
            } else {
                obj.children(".submenu").delay(hideDelay).slideUp(speed);
                if ((o) && (!o.is_init) && (typeof o.options.onCollapse == "function")) {
                    o.options.onCollapse(o, obj, e);
                }
            }
            if (obj.children(".submenu").siblings("a").hasClass("submenu-indicator-minus")) {
                obj.children(".submenu").siblings("a").removeClass("submenu-indicator-minus")
            }
        },

        _initSubmenuIndicators: function () {
            if (!this.options.expandAll) {
                if ($(this.element).find(".submenu").length > 0) {
                    $(this.element).find(".submenu").siblings("a").append("<span class='submenu-indicator'>+</span>");
                }
                //} else {
                //    if ($(this.element).find(".submenu").length > 0) {
                //        $(this.element).find(".submenu").siblings("a").parent().append("<span class='sidebar-menu-label' style='background: #fff5c0; padding-top: 2px; font-size: 80%;'><i class='ion ion-help'></i></span>");
                //    }
            }
        },

        _initOnClick: function () {
            $(this.element).find("ul li").data("object", this).click(function (e) {
                if ($(this).hasClass("menu-group") || $(this).hasClass("disabled")) return false;
                var o = $(this).data("object");
                var do_click = true;
                if ((o) && (!o.is_init) && (typeof o.options.onClick == "function")) {
                    do_click = !o.options.onClick(o, $(this), e);
                }

                var url = $(this).children("a").attr("href");
                var can_open = (typeof url != "undefined" && url != "" && !$(this).hasClass("disabled") && !$(this).hasClass("missing"));

                if (!do_click || can_open) {
                    var setClicked = ((o) ? o.options.setClicked : true);
                    var clickEffect = ((o) ? o.options.clickEffect : true);
                    if (setClicked) {
                        if (!($(this).children(".submenu").length) || ($(this).parent().parent().hasClass("sidebar-menu"))) {
                            $("li.active").removeClass("active");
                            $(this).addClass("active");
                        }
                        if ($(this).parents().hasClass("collapsed")) {
                            $(".subactive").removeClass("subactive");
                            $(".active").closest(".sidebar-menu>ul>li").addClass("subactive");
                        }
                    }
                }
                if (do_click && can_open) {
                    loadURL(url);
                }
            });

        },

        _initClickEffect: function () {
            var ink, d, x, y;
            $(this.element).find("ul").first().find("a").bind("click touch",
            function (e) {
                $(".menu-ink").remove();
                if ($(this).children(".menu-ink").length === 0) {
                    $(this).prepend("<span class='menu-ink'></span>")
                }
                ink = $(this).find(".menu-ink");
                ink.removeClass("animate-menu-ink");
                if (!ink.height() && !ink.width()) {
                    d = Math.max($(this).outerWidth(), $(this).outerHeight());
                    ink.css({
                        height: d,
                        width: d
                    })
                }
                x = e.pageX - $(this).offset().left - ink.width() / 2;
                y = e.pageY - $(this).offset().top - ink.height() / 2;
                ink.css({
                    top: y + 'px',
                    left: x + 'px'
                }).addClass("animate-menu-ink");
            })
        },

        collapse: function () {
            if (!this.options.expandAll) {
                this.is_init = true;
                this.element.find("ul.submenu").filter(function () {
                    return $(this).css('display') != 'none';
                }).css("display", "");
                this._old_width = this.element.width();
                this.element.addClass("collapsed", Math.round(0.75 * this.options.speedPanel)).animate({ width: 50 }, this.options.speedPanel);
                this.element.find(".active").closest(".sidebar-menu>ul>li").addClass("subactive");
                this.is_init = false;
                this.resize();
            }
        },

        expand: function () {
            if (!this.options.expandAll) {
                this.is_init = true;
                this.element.removeClass("collapsed", Math.round(0.75 * this.options.speedPanel)).animate({ width: ((this._old_width) ? this._old_width : 260) }, this.options.speedPanel).css("width", "");
                this.element.find(".subactive").removeClass("subactive");
                if (this.options.expandAll) this.expandAll();
                this.onActive();
                this.is_init = false;
                this.resize();
            }
        },

        toggle: function () {
            if (!this.options.expandAll) {
                if (this.isCollapsed()) this.expand(); else this.collapse();
            }
        },

        isCollapsed: function () {
            return (this.element.hasClass("collapsed"));
        },

        //refresh: function () {
        //},

        resize: function () {
            this._trigger("onResize");
        },


    });

})(jQuery);

function prepareSidebarItems(obj, items, is_submenu) {
    var ul = "";
    if ((items)) {
        if (items.length) {
            ul = $("<ul></ul>");
            if (is_submenu) ul.prop("class", "submenu");
            var margin = false;
            var added = 0;
            for (var i = 0; i < items.length; i++) {
                var el = items[i];
                var li = $("<li></li>");
                if (typeof el.selected == "undefined" && el.pgID == pgID) {
                    el.selected = true;
                }
                if ((el.selected)) {
                    li.prop("class", "active");
                    if (el.help != "" && help_uri == "") updateHelpPage(el.help);
                }
                if ((el.position == position_workflow)) li.addClass("workflow");
                if (margin) {
                    li.addClass("menu-margin");
                    margin = false;
                }
                var extras = "";
                var target = "";
                var onclick = "";
                var hidden = false;
                var show_warning = false;
                if (typeof el.extra != "undefined" && (typeof el.items == "undefined" || !el.items.length)) {
                    if (el.extra.indexOf("hotkey") >= 0) extras += addLabelIcon("ion ion-ios-pricetag", (el.hotkey != "" ? "Shortcut: " + el.hotkey : "Shortcut available"), "#cccccc", 2);
                    if ((show_draft)) {
                        if (el.extra.indexOf("html5") >= 0) extras += addLabelIcon("ion ion-logo-html5", "HTML5 version", "#99cc99", 2);
                        if (el.extra.indexOf("draft") >= 0) extras += addLabelIcon("", "Draft (beta) version", "#99cccc", 0, "&beta;");
                        if (el.extra.indexOf("missing") >= 0) extras += addLabelIcon("fa fa-ban", "Page does not exist", "#dd9999", 1);
                        if (el.extra.indexOf("gecko") >= 0) extras += addLabelIcon("fa fa-mobile-alt", "Responsive version (Gecko project)", "#9999dd", 1);
                    }
                    if (el.extra.indexOf("help") >= 0) extras += addLabelIcon("fa fa-question", "Help page", COLOR_EXTA_ICON_YELLOW, 1);
                    if (el.extra.indexOf("hide") >= 0) hidden = true;
                    if (el.extra.indexOf("eval") >= 0) {
                        extras = addLabelIcon("ion ion-ios-open", "Responsive version", "#dd99dd", 1);
                        target = "_blank";
                    }
                    if (el.extra.indexOf(menu_option_alts) >= 0 && typeof has_alts != "undefined") {
                        el.disabled = !(has_alts);
                        if (!(has_alts)) {
                            show_warning = true;
                            onclick = "msgNoAlts(); return false;";
                        }
                    }
                    if (!show_warning && el.extra.indexOf(menu_option_noAIP) >= 0 && typeof use_aip != "undefined") {
                        el.disabled = (use_aip);
                        if ((use_aip)) {
                            show_warning = true;
                            onclick = "msgUseAIP(); return false;";
                        }
                    }
                    if (el.extra.indexOf(menu_option_csdata) >= 0 && typeof has_csdata != "undefined") {
                        if ((has_csdata)) {
                            extras += addLabelIcon("fas fa-check", resString("lblHasStructuringData"), COLOR_EXTA_ICON_YELLOW, 1);
                        }
                    }
                    if (el.extra.indexOf("new") >= 0 && el.pgID) {
                        if (el.selected) localStorage.setItem("new" + el.pgID, el.pgID);
                        var c = localStorage.getItem("new" + el.pgID);
                        if (typeof c == "undefined" || c == null || c == "") {
                            extras = "<span class='sidebar-menu-label menu_new' style='background:transparent;' title='" + resString("lblNewFeature") + "'><i class='fas fa-certificate'></i><span>N</span></span>";
                        }
                    }
                    if (el.extra.indexOf("highlight") !== -1) {
                        li.addClass("menu-highlight");
                    }
                    el.nowrap = el.extra.indexOf(menu_option_no_wrap) >= 0;
                }
                if (show_warning) extras = "<span style='cursor:pointer'>" + addLabelIcon("", "Why this is disabled?' onclick='" + onclick, "#f1b561", 1, "<b>?</b>") + "</span>";


                var h = (el.title == "" ? "" : el.title);
                li.html("<a href='" + (el.url != "" ? el.url : "") + "'" + (onclick != "" ? " onclick='" + onclick + "'" : "") + " title='" + ((h == "") ? "" : htmlEscape(h)) + (typeof el.hotkey != "undefined" && el.hotkey != "" ? " (" + el.hotkey + ")" : "") + "'" + (target == "" ? "" : " target=" + target) + ((el.url == "" && onclick == "") || el.disabled == "true" || el.disabled == true ? " style='cursor:default;'" : "") + (typeof el.nowrap != "undefined" && el.nowrap ? " class='sidebar-menu-nowrap' " : "") + "><i class='" + (el.icon == "" ? (is_submenu ? "fa fa-genderless" : "fa fa-angle-right") : el.icon) + "'></i>" + el.text + ((show_draft) && typeof el.desc != "undefined" && el.desc != "" ? "<span class='nav_desc'>&nbsp;" + el.desc + "</span>" : "") + "</a>" + extras);
                li.data(el);
                if ((el.disabled == "true") || (el.disabled == true)) {
                    li.prop("disabled", "disabled");
                    li.addClass("disabled");
                }
                if ((el.extra) && el.extra.indexOf("missing") >= 0) {
                    li.addClass("missing");
                }
                el.object = li;
                if ((el.items) && (el.items.length)) prepareSidebarItems(li, el.items, true);
                if (!hidden) {
                    if (el.text == "-" && el.pgID <= 0) {
                        margin = true;
                        ul.append($("<li class='menu-group'></li>"));
                    } else {
                        ul.append(li);
                    }
                    added += 1;
                }
            }
            if (added) obj.append(ul);
        }
    }
    return ul;
}

function msgNoAlts() {
    //DevExpress.ui.dialog.alert(resString("msgNoAlternatives"), resString("titleInformation"));
    var pg_alts;
    if ((nav_json)) pg_alts = pageByID(nav_json, _pgid_structure_alternatives);
    var conf_options = {
        "title": resString("titleInformation"),
        "messageHtml": resString("msgNoAlternatives"),
        "buttons": [{
            text: resString("btnAddAlts"),
            "visible": pgID != _pgid_structure_alternatives && ((pg_alts)),
            onClick: function () {
                onOpenPage(_pgid_structure_alternatives);
            }
        }, {
            text: resString("btnIGotIt"),
            onClick: function () {
            }
        }]
    };
    var dlg = DevExpress.ui.dialog.custom(conf_options);
    dlg.show();
}

function msgUseAIP() {
    if (opt_ask_disable_AIP) {
        var result = DevExpress.ui.dialog.confirm(resString("msgWarningSensytivitiesAIP") + "<p>" + resString("confDisableAIP"), resString("titleConfirmation"));
        result.done(function (dialogResult) {
            if (dialogResult) {
                callAPI("pm/params/?action=set_pipe_option", { "name": "CombinedMode", "value": 0 }, menu_onDisableAIP);  // CombinedCalculationsMode.cmAIJ
            }
        });
    } else {
        DevExpress.ui.dialog.alert(resString("msgWarningSensytivitiesAIP"), resString("titleInformation"));
    }
    return false;
}

function menu_onDisableAIP(res) {
    if (isValidReply(res) && res.Result == _res_Success) {
        menu_setOption(menu_option_noAIP, (res.Data != 0));
        if (!use_aip) reloadPage();
    }
}

function addLabelIcon(icon, hint, color, offset, text) {
    return "<span class='sidebar-menu-label' title='" + hint + "' style='background:" + color + "; padding-top:" + (2 + offset) + "px;" + (icon.indexOf("ion") >= 0 ? "font-size:75%;" : "") + "'>" + (icon == "" ? text : "<i class='" + icon + "'></i>") + "</span>";
}

function sess_workflow_history() {
    return sess_workflow_history_prefix + "_" + curSID + (typeof curPrjID == "undefined" || curPrjID <= 0 ? "" : "_" + curPrjID);
}

function sess_tab_history() {
    return sess_tab_history_prefix + "_" + curSID + (typeof curPrjID == "undefined" || curPrjID <= 0 ? "" : "_" + curPrjID);
}

function initWorkflowMenu(items, uid) {
    var tabs = [];
    var sel = -1;
    if (typeof (uid) == "undefined") uid = "";
    if ((items) && (items.length)) {
        var hasLeft = false;
        var pgID_ = pgID % _pgid_max_mod;
        for (var i = 0; i < items.length; i++) {
            var e = items[i];
            if (e.text != "" && e.position == "workflow") {
                tabs.push(e);
                if (e.pgID == pgID || e.uid == uid || (e.pgID % _pgid_max_mod) == pgID_) sel = tabs.length - 1;
                if ((!isRiskion || typeof e.hid == "undefined" || e.hid == (curPrjHID == 0 ? "likelihood" : "impact")) && pageByIDReal(e.items, pgID_)) {
                    hasLeft = initSideBarMenu(e.items);
                    if (hasLeft) sel = tabs.length - 1;
                } else {
                    if (pageByUID(e.items, uid)) {
                        hasLeft = initSideBarMenu(e.items);
                        if (hasLeft) sel = tabs.length - 1;
                    }
                }
            }
        }
        if (!hasLeft) {
            var p = pageByIDReal(items, pgID_);
            if (!(p) && uid != "") p = pageByUID(items, uid);
            if ((p)) {
                if (typeof p.items != "undefined") {
                    var lst = ((p.position) && p.position == position_workflow ? p.items : [p]);
                    hasLeft = initSideBarMenu(lst);
                } else {
                    if ((p.pguid)) {
                        var par = pageByUID(items, p.pguid);
                        if ((par)) {
                            hasLeft = initSideBarMenu([par]);
                            if (typeof par.position != "unedfined" && par.position == position_usermenu) ignoreAdvancedSwitch = true;
                        }
                    }
                }
            }
        }

        //if ((opt_store_last_visited) && (opt_ignore_last_visted.indexOf(pgID * 1) == -1 && opt_ignore_last_visted.indexOf(pgID + "") == -1) && JSON && items) {
        if ((opt_ignore_last_visted.indexOf(pgID * 1) == -1 && opt_ignore_last_visted.indexOf(pgID + "") == -1) && JSON && items) {
            var wrkfl = pageWorkflowByUID(items, pgID, true);
            if (wrkfl && wrkfl.uid != pgID && (wrkfl.uid)) {
                var pgid = "p_" + wrkfl.uid;
                var h = getWorkflowPagesHistory();
                h[pgid] = pgID;
                localStorage.setItem(sess_workflow_history(), JSON.stringify(h));
            }
        }

    }
    if (sel < 0) {
        var par = pageWorkflowByID(items, pgID); // pageParentByID(nav_json, pgID, "undefined");
        if ((par)) {
            for (var i = 0; i < tabs.length; i++) {
                if (tabs[i].uid == par.uid) {
                    sel = i;
                    break;
                }
            }
        }
    }
    if (tabs.length && (opt_show_workflow_when_single || tabs.length > 1)) {
        var menu = $("#workflow");
        menu.html("");

        var sel_item = (sel >= 0 ? tabs[sel] : null);
        if ((sel_item == null)) {
            for (var i = 0; i < tabs.length; i++) {
                if (tabs[i].position == "workflow") {
                    if (tabs[i].pgID == pgID || tabs[i].uid == uid || (pageByID(tabs[i].items, pgID)) || (pageByIDReal(tabs[i].items, pgID_)) || (typeof tabs[i].selected != "undefined" && (tabs[i].selected))) {
                        sel_item = tabs[i];
                        break;
                    }
                }
            }
        }
        if (typeof pgID != "undefined" && (!(sel_item) || sel_item.uid == pgID) && pgID != _pgid_projectslist) {
            sel_item = pageByID(items, _pgid_projectslist);
        }

        var top_url = getWorkflowURL(sel_item, false);
        $("<div class='project_actions' id='divProjectActions'></div>").appendTo(menu);
        if ((sel_item) && top_url != "") {
            $("<a href='" + top_url + "' onclick='showLoadingPanel();' title='Navigate to top [" + sel_item.text + "]'><div id='divGoTop'><i class='fas fa-arrow-alt-circle-up'></i></div></a>").appendTo(menu);
            if (window.history.length > 1) $("<a href='' onclick='window.history.go(-1); showLoadingPanel(); return false;' title='Navigate to previous page'><div id='divGoBack'><i class='fas fa-arrow-alt-circle-left'></i></div></a>").appendTo(menu);
        }
        var pg = pageByID(items, pgID);
        if (!(pg)) pg = pageByUID(items, pgID);
        if ((opt_show_pgtitle_basic) && top_url != "" && (pg) && pg.position != "workflow") $("<div class='nowide800' id='divCurPageName' title='" + (pg.title == "" ? pg.text : pg.title) + "'>" + pg.text + "</div>").appendTo(menu);
        $("#divGoTop").toggle(!isAdvancedMode);
        $("#divGoBack").toggle(!isAdvancedMode);
        if (opt_show_pgtitle_basic) $("#divCurPageName").toggle(!isAdvancedMode);
        if (!isRiskion || tabs.length > 1) {
            for (var i = 0; i < tabs.length; i++) {
                var e = tabs[i];
                if (e.position == "workflow") {
                    var dis = (e.disabled == true || (typeof e.extra != "undefined" && e.extra.indexOf("missing") >= 0));
                    var isSelected = (e.pgID == pgID || e.uid == uid || ((!isRiskion || typeof e.hid == "undefined" || e.hid == (curPrjHID == 0 ? "likelihood" : "impact")) && pageByIDReal(e.items, pgID)) || (typeof tabs[i].selected != "undefined" && (tabs[i].selected)));
                    var selectedClass = (isSelected ? " selected" : "");
                    if (isSelected) {
                        menu.find("span.arrow:last").addClass("pre_selected");
                        menu_active_workflow = e;
                    };
                    var url = getWorkflowURL(e, (isSelected));
                    //$("<li><a href='" + url + "' class='" + ((i & 1) ? "odd" : "") + selectedClass + "' onclick='onWorkflowClick(pageByUID(nav_json, " + e.uid + "), nav_json); return false;'>" + (e.icon != "" ? "<i class='dx-icon " + e.icon + (e.position == "compact" ? ' plist' : '') + " 'title='" + e.text + "'></i>&nbsp;" : "") + (e.position != "compact" ? "<span class='workflow-item-label'>" + e.text + "</span>" : '') + "<span class='arrow" + (i == tabs.length - 1 ? " last " : "") + selectedClass + "'></span></a></li>").appendTo(menu);
                    var t = (typeof e.title != "undefined" && e.title != "" ? e.title : e.text);
                    if ((dis)) {
                        $("<li><span class='" + ((i & 1) ? "odd" : "") + selectedClass + " disabled' disabled='disabled' title='" + t + ((typeof e.extra != "undefined" && e.extra.indexOf("missing") > 0) ? " [ Not implemented ]" : "") + "'><span class='fas fa-angle-double-right workflow-chevron'></span>" + (e.icon != "" ? "<i class='dx-icon " + e.icon + (e.position == "compact" ? ' plist' : '') + "'></i>&nbsp;" : "") + (e.position != "compact" ? "<span class='workflow-item-label'>" + e.text + "</span>" : '') + "<span class='arrow" + (i == tabs.length - 1 ? " last " : "") + selectedClass + "'></span></span></li>").appendTo(menu);
                    } else {
                        var sNew = "";
                        if (typeof e.extra !== "undefined" && e.extra.indexOf("new") >= 0) {
                            if (e.pgID) {
                                if (isSelected) localStorage.setItem("new" + e.pgID, e.pgID);
                                var c = localStorage.getItem("new" + e.pgID);
                                if (typeof c == "undefined" || c == null || c == "") {
                                    sNew = "<span class='menu_new' title='" + resString("lblNewFeature") + "'><i class='fas fa-certificate blink_new'></i><span>N</span></span>";
                                }
                            }
                        }
                        var sClass = "";
                        if (typeof e.extra !== "undefined" && e.extra.indexOf("highlight") !== -1) {
                            sClass += "menu-highlight";
                        }
                        var lnk = $("<li class='" + sClass + "'><!--span class='btn_caret'></span--><a href='" + (dis ? "" : url) + "' class='" + ((i & 1) ? "odd" : "") + selectedClass + (dis ? " disabled" : "") + " link-handler' data-uid='" + e.uid + "' title='" + t + "'><span class='fas fa-angle-double-right workflow-chevron'></span>" + (e.icon != "" ? "<i class='dx-icon " + e.icon + (e.position == "compact" ? ' plist' : '') + "'></i>&nbsp;" : "") + (e.position != "compact" ? "<span class='workflow-item-label'>" + e.text + "</span>" : '') + "<span class='arrow" + (i == tabs.length - 1 ? " last " : "") + selectedClass + "'></span>" + sNew + "</a></li>").appendTo(menu);
                        lnk.find(".link-handler").data("pg", e);
                    }
                };
            }
        }
        $(".link-handler").on("click", function () {
            var pg = $(this).data("pg");
            if ((pg) && typeof pg != "undefined" && typeof pg.pgID != "undefined") {
                onWorkflowClick(pg, nav_json);
            } else {
                var uid = this.getAttribute("data-uid") * 1;
                onWorkflowClick(pageByUID(nav_json, uid), nav_json);
            }
            return false;
        });
        $("#trWorkflow").show();

        $("#workflow a").hover(
            function () {
                $(this).addClass("hover").children().addClass("hover");
                $(this).parent().prev().find("span.arrow:first").addClass("pre_hover");
            },
            function () {
                $(this).removeClass("hover").children().removeClass("hover");
                $(this).parent().prev().find("span.arrow:first").removeClass("pre_hover");
            }
        );
    } else {
        if (!opt_show_workflow_when_single && tabs.length == 1) hasLeft = false;
    }

    var p = pageByID(items, pgID);
    if (!(p)) p = pageByIDReal(items, pgID_);
    if (!(p) && uid != "") p = pageByUID(items, uid);
    if (hasLeft && (p) && typeof p.extra != "undefined" && p.extra.indexOf("hidemenu") >= 0) {
        $("#trTopMenu").hide();
        $("#trWorkflow").hide();
        $(".mainContentContainer").removeClass("divMainContent_padding");
        hasLeft = false;
    }

    if ((p)) {
        if (typeof p.extra != "undefined" && p.extra.indexOf("collapsemenu") >= 0 && !menu_switch_hover) {
            toggleSideBarMenu(true, 0, false);
        }
    }
    if (!hasLeft) {
        $(".tdMenu").hide();
        if ($("#divTopMenu").length) $("#divTopMenu").addClass("border");
    }
    return hasLeft;
}

function getWorkflowURL(e, set_help_uri) {
    var url = "";
    if ((e)) {
        var url = e.url;
        if ((set_help_uri) && help_uri == "" && typeof e.help != "undefined" && e.help != "") help_uri = updateHelpPage(e.help);
        if (e.uid != "" && url == "" && help_uri != "" && help_uri.indexOf(".") > 0 && (typeof e.position == "undefined" || e.position == position_workflow)) url = _url_infopage + "?navpgid=" + e.uid;
        if (e.uid != "" && url == "" && (e.position == position_workflow || e.position == tab)) url = _url_landingpage + "?navpgid=" + e.uid;
        if (url == "" && typeof e.items != "undefined" && e.items.length) {
            if (e.items[0].url != "") {
                url = e.items[0].url;
            } else {
                //if (typeof e.items[0].items != "undefined" && e.items[0].items.length) url = e.items[0].items[0].url;
            }
        }
    }
    return url;
}

function initSideBarMenu(menu) {
    if ((menu) && (menu.length)) {
        menu_last = menu;
        var has_menu = false;
        if ($("#divMenu") && $("#divMenu").length) {
            $("#divMenu").html("");
            $(".mainContentContainer").addClass("divMainContent_padding");
            has_menu = true;
        }
        //$(".sidebar-menu-content").css("overflow", "hidden");
        //$("#divMenu").show();
        prepareSidebarItems($("#divMenu"), menu, false);
        $("#divMenu").sidebarMenu({
            speed: 200,
            speedCollapse: 0,
            singleOpen: false,
            expandAll: true,
            onClick: function (data, item, e) {
                e.stopPropagation();
                e.preventDefault();
                return onMenuItemClick($(item).data());
            }
        });
        $(".tdMenu").show();
        $(".sidebar-menu-content").dxScrollView({
            //showScrollbar: "always",
            direction: "vertical",
        });
        updateAutoCollapseIcon();
        if (has_menu && menu.length == 1 && (typeof menu[0].items == "undefined" || (menu[0].items.length <= 1))) toggleSideBarMenu(true, 0, false);

        //// swipe for toggle sidebar menu
        //DevExpress.events.on($(".onswipe"), "dxswipestart", "", function (event) {
        //    swipe_start_x = event.clientX;
        //    swipe_start_y = event.clientY;
        //});
        //DevExpress.events.on($(".onswipe"), "dxswipeend", "", function (event) {
        //    var dif_x = (event.clientX - swipe_start_x);
        //    var dif_y = (event.clientY - swipe_start_y);
        //    if (Math.abs(dif_x) > 50 && Math.abs(dif_x) > 2 * Math.abs(dif_y))
        //    {
        //        toggleSideBarMenu(true, (dif_x > 0 ? 1 : 0), true);
        //    }
        //});
        if (has_menu) menu_active_item = pageSelected(menu);
        return has_menu;
        //if (typeof (onMPResize) == "function") onMPResize();
    } else {
        //$("#divMenu").hide();
        $(".tdMenu").hide();
        return false;
    }
}

function pageByIDReal(lst, pgid) {
    var p = null;
    if ((lst) && (lst.length)) {
        pgid = (pgid % _pgid_max_mod);
        for (var i = 0; i < lst.length; i++) {
            var pg = lst[i];
            if ((pg.pgID % _pgid_max_mod) == pgid) return pg;
            if ((pg.items) && (pg.items.length)) {
                p = pageByIDReal(pg.items, pgid);
                if (p != null) break;
            }
        }
    }
    return p;
}

function pageByID(lst, pgid) {
    var p = null;
    if ((lst) && (lst.length)) {
        for (var i = 0; i < lst.length; i++) {
            var pg = lst[i];
            if (pg.pgID == pgid) return pg;
            if ((pg.items) && (pg.items.length)) {
                p = pageByID(pg.items, pgid);
                if (p != null) break;
            }
        }
    }
    return p;
}

function pageByUID(lst, uid) {
    var p = null;
    if ((lst) && (lst.length)) {
        for (var i = 0; i < lst.length; i++) {
            var pg = lst[i];
            if (pg.uid == uid) return pg;
            if ((pg.items) && (pg.items.length)) {
                p = pageByUID(pg.items, uid);
                if (p != null) break;
            }
        }
    }
    return p;
}

function pageSelected(lst) {
    var p = null;
    if ((lst) && (lst.length)) {
        for (var i = 0; i < lst.length; i++) {
            var pg = lst[i];
            if (typeof pg.selected != "undefined" && (pg.selected)) return pg;
            if ((pg.items) && (pg.items.length)) {
                p = pageSelected(pg.items);
                if (p != null) break;
            }
        }
    }
    return p;
}

//function pageParentByID(lst, pgid, parent) {
//        for (var i = 0; i < lst.length; i++) {
//            var pg = lst[i];
//            if (pg.pgID == pgid) return parent;
//            if ((pg.items) && (pg.items.length)) {
//                p = pageParentByID(pg.items, pgid, pg);
//                if (p != null) break;
//            }
//        }
//    }
//    return p;
//}

//function pageParentByUID(lst, uid, parent) {
//    var p = null;
//    if ((lst) && (lst.length)) {
//        for (var i = 0; i < lst.length; i++) {
//            var pg = lst[i];
//            if (pg.pgID == uid) return parent;
//            if ((pg.items) && (pg.items.length)) {
//                p = pageParentByID(pg.items, uid, pg);
//                if (p != null) break;
//            }
//        }
//    }
//    return p;
//}

function pageWorkflowByID(lst, id) {
    var pg = pageByID(lst, id);
    return ((pg) ? pageWorkflowByPage(lst, pg, false) : null);
}

function pageWorkflowByUID(lst, uid) {
    var pg = pageByUID(lst, uid);
    return ((pg) ? pageWorkflowByPage(lst, pg, false) : null);
}

function pageWorkflowByPage(lst, pg, allow_root) {
    if ((pg) && (lst) && (lst.length)) {
        var p = pageByUID(lst, pg.pguid);
        if ((p)) {
            if (p.position == position_workflow || (p.pguid == "" && allow_root)) {
                return p;
            } else {
                return pageWorkflowByPage(lst, p, allow_root);
            }
        }
    }
    return null;
}

function onOpenPage(pgid) {
    var pg = (typeof (pgid) == "integer" || typeof (pgid) == "number" ? pageByID(nav_json, pgid) || pageByUID(nav_json, pgid) : pgid);
    if ((pg)) {
        if ((pg.object) && (pg.object.length)) {
            pg.object.click();
            return true
        }
        return onMenuItemClick(pg);
    } else {
        return false;
    }
}

function navOpenPage(pgid, keep_hid) {
    var pg = pageByID(nav_json, pgid);
    if ((typeof keep_hid != "undefined") && (keep_hid) && (typeof menu_active_tab != "undefined") && ((menu_active_tab)) && (typeof menu_active_tab.hid != "undefined") && (typeof menu_active_tab.items != "undefined") && (menu_active_tab.items.length)) {
        var pg_ = pageByIDReal(menu_active_tab.items, pgid);
        if ((pg_)) pg = pg_;
    }
    return !(onOpenPage(pg));
}

function onWorkflowClick(e, pglist) {
    if ((opt_store_last_visited && isAdvancedMode) && (e) && typeof e.uid != "undefined" && typeof (e.position) != "undefined" && pglist) {
        //if ((e) && typeof e.uid != "undefined" && typeof (e.position) != "undefined" && pglist) {
        var h = getWorkflowPagesHistory();
        var uid = h["p_" + e.uid];
        if ((uid) && (opt_ignore_last_visted.indexOf(uid * 1) == -1 && opt_ignore_last_visted.indexOf(uid + "") == -1)) {
            var tmp = pageByUID(pglist, uid);
            if ((tmp)) e = tmp;
        }
        if ((opt_store_last_visited && isAdvancedMode) && typeof e.position != "undefined" && e.position == position_workflow) {
            var name = "landing_" + e.uid + "_" + curPrjID;
            var visited = (document.cookie.indexOf(name) >= 0);
            if (visited) {
                if (typeof e.items != "undefined" && e.items.length) {
                    var e_ = e.items[0];
                    if (e_.url == "" && typeof e_.items != "undefined" && e_.items.length) e_ = e_.items[0];
                    if ((e_) && (e_.url != "")) e = e_;
                }
            }
        }
    }
    return openPage(e);
}

function getWorkflowPagesHistory() {
    var oldSID = localStorage.getItem(sess_session_id);
    if (oldSID != curSID) {
        localStorage.setItem(sess_session_id, curSID);
        localStorage.removeItem(sess_workflow_history());
    }
    var hs = localStorage.getItem(sess_workflow_history());
    if (typeof hs == "undefined" || hs == "" || !(hs)) hs = "{}";
    try {
        var h = (JSON && JSON.parse(hs) || $.parseJSON(hs));
    }
    catch (e) {
        h = {}
    }
    return h;
}

//function getTabPagesHistory() {
//    var oldSID = localStorage.getItem(sess_session_id);
//    if (oldSID != curSID) {
//        localStorage.setItem(sess_session_id, curSID);
//        localStorage.removeItem(sess_tab_history());
//    }
//    var hs = localStorage.getItem(sess_tab_history());
//    if (typeof hs == "undefined" || hs == "" || !(hs)) hs = "{}";
//    try {
//        var h = (JSON && JSON.parse(hs) || $.parseJSON(hs));
//    }
//    catch (e) {
//        h = {}
//    }
//    return h;
//}

function updateHelpPage(url, allow_postpone) {
    var is_new = (help_uri != url && url != "");
    help_uri = url;
    if (!ko_widget2 && typeof __ko16 != "undefined") {

        if (is_new) {
            sessionStorage.removeItem('_ko16_activeArt');
            sessionStorage.removeItem('_ko16_activeFetchArt');
            __ko16.showContainer('._ko16_article_list_cntr');
            if ((document.querySelector('._ko16_widget_btm_cntr'))) document.querySelector('._ko16_widget_btm_cntr').style.display = 'block';
            if ((document.querySelector('._ko16_widget_art_ext_cntr'))) document.querySelector('._ko16_widget_art_ext_cntr').style.display = 'none';
            __ko16.updateDimensions();
            if ((document.querySelector('._ko16_widget_back'))) document.querySelector('._ko16_widget_back').style.display = 'none';
        }

        if (typeof url != "undefined" && url != "") {
            __ko16.updatePageLoc(url);
        }

        setTimeout(function () {
            if ($('#_ko16_sug_article_list li:has("a")').length == 1) {
                $('#_ko16_sug_article_list li a')[0].click();
            }
        }, 1000);

    }
    if (ko_widget2 && typeof _ko19 != "undefined" && typeof _ko19.updateRecommended == "function") {

        if (typeof url != "undefined" && url != "") {
            //_ko19.loadKnowledge();
            _ko19.updateRecommended(url);
            //_ko19.openArticle(url);
        }

    }
 
    if (typeof help_uri == "undefined") help_uri = "";
    return help_uri;
}

function onMenuItemClick(e) {
    if ((e) && (e.disabled == "true")) return false;
    if ((e) && (e.extra) && e.extra.indexOf("missing") >= 0) {
        notImplemented();
        return false;
    }
    var doURL = true;
    var url = ((e) ? e.url : "");
    if (typeof url == "undefined") return false;
    if ((e) && typeof e.help != "undefined") help_uri = updateHelpPage(e.help);
    if (typeof help_uri == "undefined") help_uri = "";
    //if ((e) && !(e.items) && typeof onLeavePage == "function") {
    if ((e) && typeof onLeavePage == "function") {
        if (!onLeavePage(e)) {
            doURL = false;
        }
    }
    if (typeof e.js != "undefined" && (e.js != "")) {
        doURL = (eval(e.js) != false);
    }
    //if (doURL && (e) && typeof onSetPage == "function" && (Math.trunc(pgID / _pgid_max_mod) ==  Math.trunc(e.pgID / _pgid_max_mod))) {  // must be the same hierarchy
    if (doURL && (e) && typeof onSetPage == "function") {
        if (!isRiskion || (typeof e.hid == "undefined") || (typeof e.hid != "undefined" && (e.hid == "" || e.hid == id2hid(curPrjHID)))) {
            if (onSetPage(e.pgID, e)) {
                if (last_cmenu_opened != "") {
                    if (($("#context-menu").data("dxContextMenu"))) { //A1535
                        var obj = $("#context-menu");
                        if ((obj) && (obj.length)) {
                            var cm = obj.dxContextMenu("instance");
                            cm.hide();
                        }
                    }
                }
                pgID = e.pgID;
                doURL = false;
                document.title = ((e.title != "") ? e.title : e.text);
                if (e.uid != "" && url == "" && typeof e.position != "undefined" && e.position == position_usermenu && typeof e.items != "undefined" && e.items.length == 1) {
                    return onMenuItemClick(e.items[0]);
                }
                if (e.uid != "" && url == "" && help_uri != "" && help_uri.indexOf(".") > 0 && (typeof e.position == "undefined" || e.position == position_workflow)) url = _url_infopage + "?navpgpg=" + e.uid;  // just load a static content/help page
                if (url != "" && (history.replaceState)) {
                    history.replaceState({}, document.title, url);  //pushState
                }
                if (typeof initWireframeLink == "function") {
                    initWireframeLink();
                }
                $("#divCurPageName").html(e.text).prop("title", (e.title == "" ? e.text : e.title));
            }
        }
    }
    if (doURL && url != "") {
        if ((e) && (e.extra) && e.extra.indexOf("eval") >= 0) {
            wnd_gecko = CreatePopup(url, "gecko", ",", true);
            if ((wnd_gecko)) setTimeout(function () { wnd_gecko.focus(); }, 1000);
        } else {
            loadURL(url);
        }
        doURL = false;
    }
    if (doURL && url == "" && typeof help_uri != "undefined" && help_uri != "" && help_uri.indexOf(".") > 0 && (typeof e.position == "undefined" || e.position == position_workflow)) {
        if ((e) && e.uid != "") {
            loadURL(_url_infopage + "?navpgid=" + e.uid);
        } else {
            onShowHelp(help_uri);
        }
        doURL = false;
    }
    if (!doURL && (e)) menu_active_item = e;
    if (doURL && url == "" && help_uri == "" && (e) && !(e.items)) {
        notImplemented();
        doURL = false;
    }
    return !doURL;
}

function menu_setOption(name, value) {
    if ((window.parent) && (typeof window.parent.menu_setOption == "function") && (window.parent.document != document)) {
        window.parent.menu_setOption(name, value);
    } else {
        if (menu_last && (typeof isAdvancedMode == "undefined" || (isAdvancedMode))) {
            switch (name) {
                case menu_option_alts: {
                    if (has_alts != value) {
                        has_alts = value;
                        initSideBarMenu(menu_last);
                    }
                    break;
                }
                case menu_option_noAIP: {
                    if (use_aip != value) {
                        use_aip = value;
                        initSideBarMenu(menu_last);
                    }
                    break;
                }
                case menu_option_csdata: {
                    if (has_csdata != value) {
                        has_csdata = value;
                        initSideBarMenu(menu_last);
                    }
                    break;
                }
            }
        }
    }
}

function openPage(pg) {
    if ((pg)) {

        // show landing for tab/workflow when no links and have childs
        if (opt_show_workflow_landing && typeof pg.position != "undefined" && (pg.position == position_workflow || pg.position == position_tab) && typeof pg.items != "undefined" && pg.items.length > 1) {
            //if (pg.url == "" && (typeof pg.js == "undefined" || pg.js == "") && (typeof pg.help == "undefined" || pg.help == "")) {
            if (pg.url == "" && (typeof pg.js == "undefined" || pg.js == "")) {
                if ((typeof pg.id != "undefined" && (pg.id)) || (typeof pg.uid != "undefined" && (pg.uid))) {
                    var _pgid = (typeof pg.id != "undefined" && (pg.id) ? pg.id : pg.uid) * 1;
                    var extra = "";
                    if ((isRiskion) && (nav_json) && typeof curPrjData != "undefined") {
                        var tab = pg;
                        var hid_old = ((tab) && (typeof tab.hid != "undefined") ? tab.hid : "");

                        var has_mirror = false;
                        if (pg.position == position_workflow && hid_old != "") {
                            if ((pg.items) && (!(menu_active_tab) || menu_active_tab.pgID != pg.pgID)) {
                                var p = pageByIDReal(pg.items, pgID);
                                if (p) {
                                    pg = p;
                                    has_mirror = true;
                                }
                            }
                            pg = checkMirror(pg, has_mirror);
                        }

                        if (!has_mirror && pg.position == position_workflow && typeof pg.pguid != "undefined") tab = pageByUID(nav_json, pg.pguid);
                        if (hid_old != "" && (tab) && (typeof tab.hid == "undefined")) tab.hid = hid_old;
                        if ((tab) && (typeof tab.hid != "undefined")) {
                            switch (tab.hid) {
                                case "impact":
                                case "1":
                                    extra = "passcode=" + curPrjData.AccessCodeImpact;
                                    break;
                                case "likelihood":
                                case "0":
                                    extra = "passcode=" + curPrjData.AccessCode;
                                    break;
                            }
                        }
                        if ((tab) && pg.position == position_workflow && (opt_ignore_last_visted.indexOf(_pgid * 1) == -1 && opt_ignore_last_visted.indexOf(_pgid + "") == -1)) {
                            if (tab.uid != _pgid && (tab.uid)) {
                                var pgid = "p_" + tab.uid;
                                var h = getWorkflowPagesHistory();
                                h[pgid] = _pgid;
                                localStorage.setItem(sess_workflow_history(), JSON.stringify(h));
                            }
                        }
                    }
                    if (!(pg) || !has_mirror) {
                        loadURL(_url_landingpage + "?navpgid=" + _pgid + (extra == "" ? "" : "&" + extra));
                        return true;
                    }
                }
            }
        }

        if (pg.url != "" || (typeof pg.js != "undefined" && pg.js != "")) {
            return onMenuItemClick(pg);
        } else {
            if (typeof pg.help != "undefined" && pg.help != "") {
                onMenuItemClick(pg);
            } else {
                if ((pg.items) && (pg.items.length)) {
                    return openPage(pg.items[0]);
                }
            }
        }
    }
    return false;
}

function initTopLineMenu(nav_json) {
    var tabs = [];
    var items = nav_json;
    var sel = -1;
    var hasLeft = false;
    if ((items) && (items.length)) {
        for (var i = 0; i < items.length; i++) {
            var e = items[i];
            if (e.text != "" && (e.position == "tab")) {
                //tabs.push({ "id": e.pgID, "icon": e.icon, "text": e.text, "uid": e.uid, "pg": e, "position": e.position });
                var dis = (e.disabled == true || (typeof e.extra != "undefined" && e.extra.indexOf("missing") >= 0));
                var s = (typeof e.selected != "undefined" && (e.selected));
                tabs.push({ "id": e.pgID, "text": e.text, "uid": e.uid, "pg": e, "position": e.position, "disabled": (dis), "selected": s });  // "icon": "fas fa-caret-right", 
                if (s) sel = tabs.length - 1;
                //if (e.pgID == pgID || e.uid == pgID) sel = tabs.length - 1;
                //if (pageByID(e.items, pgID)) {
                //    sel = tabs.length - 1;
                //    tabs[sel].icon = "fas fa-caret-down";
                //    hasLeft = initWorkflowMenu(e.items);
                //    if (!hasLeft && $("#trWorkflow").html() != "") hasLeft = true;
                //};
            }
        }
    };
    if (sel < 0) {

        var lst = nav_json;
        if (isRiskion && curPrjID > 0) {
            var tab = null;
            var hid = id2hid(curPrjHID);
            for (var i = 0; i < items.length; i++) {
                var e = items[i];
                if (e.text != "" && (e.position == "tab")) {
                    if (typeof e.extra != "undefined" && e.extra.indexOf(hid) >= 0) {
                        tab = e;
                        break;
                    }
                }
            }
            if ((tab)) {
                var id = (pgID % _pgid_max_mod);
                var pg = pageByIDReal(tab.items, id);
                if ((pg)) lst = tab.items;
            }
        }

        var sel_tab = null;

        var pg = pageByID(lst, pgID);
        var is_tab = ((pg) && typeof pg.position != "undefined" && pg.position == position_tab);
        if (!(pg)) pg = pageByIDReal(lst, (pgID % _pgid_max_mod));
        if (!(pg)) pg = pageByUID(lst, pgID);
        var lvl = 0;
        while ((pg) && pg.pguid != "" && lvl < 3) {
            var p_pg = pageByID(nav_json, pg.pguid);
            if (!(p_pg)) p_pg = pageByUID(nav_json, pg.pguid);
            if ((p_pg)) {
                pg = p_pg;
            } else {
                break;
            }
            lvl += 1;
        }
        if ((pg)) {
            sel_tab = pg;
            if (!is_tab) {
                hasLeft = initWorkflowMenu((typeof pg.items != "undefined" && (pg.items)) ? pg.items : [pg]);
                if (!hasLeft && $("#workflow").length && $("#workflow").html() != "") hasLeft = true;
            }
        }
        if (!hasLeft && (pg) && typeof pg.items != "undefined" && (pg.items)) {
            hasLeft = initSideBarMenu(pg.items);
        }

        if ((sel_tab)) {
            menu_active_tab = sel_tab;
            for (var i = 0; i < tabs.length; i++) {
                if (tabs[i].uid == sel_tab.uid) {
                    sel = i;
                    //tabs[sel].icon = "fas fa-caret-down";
                    break;
                }
            }
        }
    }

    if (!hasLeft) {
        $("#trWorkflow").hide();
        $(".tdMenu").hide();
        $(".mainContentContainer").removeClass("divMainContent_padding");
    }
    if ((sel)) {
        //if ((opt_store_last_visited) && (opt_ignore_last_visted.indexOf(pgID * 1) == -1 && opt_ignore_last_visted.indexOf(pgID + "") == -1) && JSON && nav_json) {
        if ((opt_ignore_last_visted.indexOf(pgID * 1) == -1 && opt_ignore_last_visted.indexOf(pgID + "") == -1) && JSON && nav_json) {
            var wrkfl = pageWorkflowByUID(nav_json, pgID, true);
            if (!(wrkfl)) wrkfl = pageWorkflowByID(nav_json, pgID, true);
            if ((wrkfl) && typeof wrkfl.pguid != "undefined") {
                var tab = pageByUID(items, wrkfl.pguid);
                if (tab && tab.uid != pgID && (tab.uid) && tab.position == position_tab) {
                    var pgid = "p_" + tab.uid;
                    var h = getWorkflowPagesHistory();
                    h[pgid] = pgID;
                    localStorage.setItem(sess_workflow_history(), JSON.stringify(h));
                }
            }
        }
    }

    if (tabs.length) {
        $("#trTopMenu").show();
        //setTimeout(function () {
        //    $(".tdHeaderNoNav").removeClass("tdHeaderNoNav");
        //}, 30);
        $("#divTopMenu").dxTabs({
            dataSource: tabs,
            selectedIndex: sel,
            //width: "100%",
            //scrollByContent: true,
            showNavButtons: true,
            onItemClick: function (e) {
                var p = e.itemData;
                var pg = p.pg;
                if (!(pg)) pg = pageByUID(nav_json, p.uid);
                if (!(pg) && (p.id)) pg = pageByID(nav_json, p.id);
                onTabClick(pg);
            },
            itemTemplate: function (itemData, itemIndex, element) {
                //if (typeof itemData.icon != "undefined" && itemData.icon != "") element.append($('<i class="dx-icon fas fa-caret-down"></i>'));
                element.append($('<span class="dx-tab-text">' + itemData.text + '</span></div>'));
                if (itemData.text.length > 5) element.prop("title", itemData.text);
            }
        });
        if ((!($(".tdMenu").length) || !$(".tdMenu").is(":visible")) && $("#divTopMenu").length) $("#divTopMenu").addClass("border");
    }
    return hasLeft;
}

function onTabClick(pg) {
    var has_mirror = false;
    if (typeof pgID != "undefined" && (pgID > 0) && (pg) && typeof pg.position != "undefined" && pg.position == "tab" && (pg.items) && (!(menu_active_tab) || menu_active_tab.pgID != pg.pgID)) {
        var p = pageByIDReal(pg.items, pgID);
        if (p) {
            pg = p;
            has_mirror = true;
        }
    }
    pg = checkMirror(pg, has_mirror);
    openPage(pg);
}

function checkMirror(pg, has_mirror) {
    //if (!has_mirror && (pg) && (opt_store_last_visited && isAdvancedMode) && typeof pg.uid != "undefined" && typeof (pg.position) != "undefined" && (pg.items)) {
    if (!has_mirror && (pg) && typeof pg.uid != "undefined" && typeof (pg.position) != "undefined" && (pg.items)) {
        var h = getWorkflowPagesHistory();
        var uid = h["p_" + pg.uid];
        if ((uid) && (opt_ignore_last_visted.indexOf(uid * 1) == -1 && opt_ignore_last_visted.indexOf(uid + "") == -1)) {
            var tmp = pageByUID(pg.items, uid);
            if (!(tmp)) tmp = pageByID(pg.items, uid);
            if ((tmp)) {
                if ((opt_store_last_visited && isAdvancedMode)) {
                    pg = tmp;
                } else {
                    if (typeof tmp.position == "undefined" || tmp.position != position_workflow && (nav_json)) {
                        tmp = pageWorkflowByPage(nav_json, tmp, false);
                    }
                    if ((tmp) && typeof tmp.position == "undefined" || tmp.position == position_workflow && typeof tmp.items != "undefined" && tmp.items.length > 1) {
                        pg = tmp;
                    }
                }
            }
        }
        if (isRiskion && (pg) && pg.url == "" && typeof pg.position != "undefined" && pg.position == position_tab && typeof pg.items != "undefined" && pg.items.length) {
            pg = pg.items[0];
        }
        if ((opt_store_last_visited && isAdvancedMode) && typeof pg.position != "undefined" && pg.position == position_tab) {
            var name = "landing_" + pg.uid + "_" + curPrjID;
            var visited = (document.cookie.indexOf(name) >= 0);
            if (visited) {
                if (typeof pg.items != "undefined" && pg.items.length) {
                    var e_ = pg.items[0];
                    if (e_.url == "" && typeof e_.items != "undefined" && e_.items.length) e_ = e_.items[0];
                    if ((e_) && (typeof e_.url == "undefined" || e_.url == "") && typeof e_.items != "undefined" && e_.items.length) e_ = e_.items[0];
                    if ((e_) && (e_.url != "")) pg = e_;
                }
            }
        }
    }

    return pg;
}

function onUserMenuClick(pg) {
    if ((pg)) {
        var open_page = !(pg.disabled);
        if (open_page && typeof pg.extra != "undefined" && pg.extra.indexOf("projectslist") >= 0 && typeof pg.items != "undefined" && pg.items.length) return onUserMenuClick(pg.items[0]);
        if (open_page) {
            if (typeof pg.js != "undefined" && (pg.js != "")) {
                open_page = (eval(pg.js) != false);
            }
        }
        if (open_page) {
            if (!onMenuItemClick(pg)) {
                if ((pg.items) && (pg.items.length)) {
                    if (isAdvancedMode) {
                        for (var i = 0; i < pg.items.length; i++) {
                            var c = pg.items[i];
                            if (!(c.disabled)) {
                                if (onMenuItemClick(c)) break;
                            }
                        }
                    } else {
                        loadURL(_url_landingpage + "?navpgid=" + (typeof pg.id != "undefined" && (pg.id) ? pg.id : pg.uid));
                        open_page = false;
                    }
                }
            }
            //document.location.href = pg.URL;
        }
        return open_page;
    }
}


function initUserMenu() {
    //initSideBarMenu();
    var items = [];
    for (var i = 0; i < nav_json.length; i++) {
        var b = nav_json[i];
        if (b.position == "usermenu") {
            var subitems = [];
            if (opt_show_context_usermenu) {
                if (typeof b.items != "undefined" && (b.items.length > 1)) {
                    for (var j = 0; j < b.items.length; j++) {
                        var c = b.items[j];
                        if (typeof c.disabled == "undefined" || !(c.disabled)) {
                            subitems.push({
                                text: c.text,
                                icon: c.icon,
                                title: c.title,
                                disabled: c.disabled,
                                data: c,
                                onItemClick: function (e) {
                                    if (typeof e.itemData != "undefined" && typeof e.itemData.data != "undefined") onUserMenuClick(e.itemData.data);
                                }
                            });
                        }
                    }
                }
            }
            items.push({
                text: b.text,
                icon: b.icon,
                title: b.title,
                disabled: b.disabled,
                items: subitems,
                data: b,
                onItemClick: function (e) {
                    if (typeof e.itemData != "undefined" && typeof e.itemData.data != "undefined") onUserMenuClick(e.itemData.data);
                }
            });
        }
    }
    var has_usermenu = ((items.length));
    if (has_usermenu) {
        var icon = ((typeof sess_user_anon != "undefined" && sess_user_anon == true) || sess_username == resString("defAnonymousName") ? "fa fa-user-secret" : "fa fa-user");
        if (opt_show_context_usermenu) {
            $("#divUserMenu").dxButton({
                type: "normal",
                icon: icon,
                text: sess_username,
                hint: sess_username + (sess_username != sess_useremail && sess_useremail != "" ? ", " + sess_useremail : ""),
                template: function (e) {
                    return templateButtonSubmenu(sess_username, icon);
                },
                onClick: function (e) {
                    onShowSubmenu($("#context-menu"), "dl_menu", items, "divUserMenu");
                }
            });
        } else {
            $("#divUserMenu").dxDropDownButton({
                type: "normal",
                icon: icon,
                text: sess_username,
                hint: sess_username + (sess_username != sess_useremail ? ", " + sess_useremail : ""),
                displayExpr: "text",
                useSelectMode: false,
                wrapItemText: false,
                items: items,
                dropDownOptions: {
                    width: 165,
                },
            });
        }
    } else {
        $("#divUserMenu").dxButton({
            type: "normal",
            icon: "fa fa-user-circle",
            text: sess_username,
            hint: sess_useremail + (sess_username != sess_useremail ? ", " + sess_username : ""),
            disabled: true,
        });
        $("#divUserMenu").find(".dx-button-content").html("<span class='dx-button-text'><div class='button-submenu-caption' id='divSessUserName'>" + sess_username + "</div></span>");
    }
}


// == for mpDesktop.master sidebar menu interaction and page resize ==

//function shiftMenu() {
//    $(".tdMenu").css({ "left": "0px" });
//    $(".sidebar-menu-content").parent().hide();
//    $(".sidebar-menu-panel").css("min-width", "1px;");
//}

function isNarrowScreen() {
    return ($(window).width() <= menu_switch_min_width);
}

var sidebar_focused = false;
function autoToggleSideBarMenu(vis) {
    if (sidebar_focused != vis && !menu_switch_ignore) {
        sidebar_focused = vis;
        if (menu_switch_hover) {
            setTimeout(function () {
                if (menu_switch_hover && sidebar_focused == vis) {
                    overlayToggleSideBarMenu(vis)
                }
            }, (vis ? 50 : 500));
        }
    }
}

function overlayToggleSideBarMenu(vis) {
    if ($(".tdMenu").is(":visible")) {
        var panel_visible = !$(".tdMenu").hasClass("hidden");
        if (vis != panel_visible && !menu_switch_ignore) {
            menu_switch_ignore = true;
            setTimeout(function () { menu_switch_ignore = false; }, menu_switch_speed + 30);
            if (vis) $("#tdMenu").focus();
            toggleSideBarMenu(false, (vis ? 1 : 0), true);
        }
    }
}

function doToggleSidebarMenu() {
    if (menu_switch_hover) {
        if ($(".tdMenu").is(":visible")) {
            var panel_visible = !$(".tdMenu").hasClass("hidden");
            overlayToggleSideBarMenu(!panel_visible);
        }
    } else {
        menu_switch_auto = false;
        toggleSideBarMenu(true, -1);
    }
}

function toggleAutoHideMenu() {
    if (!isNarrowScreen()) {
        menu_switch_hover = !menu_switch_hover;
        updateAutoCollapseIcon();
        onMPResize(true, 30);
        document.cookie = "menu_autohide=" + (menu_switch_hover ? 1 : 0) + ";path=/;expires=Mon, 31-Dec-2029 23:59:59 GMT;";
    }
}

function updateAutoCollapseIcon() {
    $("#iconAutoHideMenu").removeAttr("class").addClass(menu_switch_hover ? "fas fa-thumbtack unpinned" : "fas fas fa-thumbtack pinned");
    if (isNarrowScreen()) $(".sidebar-menu-pinner").addClass("no-pinner"); else $("#sidebar-menu-pinner").removeClass("no-pinner");
}

// vis could be undefined or -1 for just toggle and 0 for hide, 1 for show
function toggleSideBarMenu(from_user, vis, animate) {
    if ($(".tdMenu").is(":visible")) {
        var panel_visible = !$(".tdMenu").hasClass("hidden");
        var do_visible = (vis == 1 || (typeof vis == "undefined" || vis == -1) && !panel_visible);

        if (panel_visible != do_visible) {
            //$(".tdMenu").toggleClass("hidden");
            if (from_user) menu_switch_auto = false;
            var speed = (typeof animate == "undefined" || animate ? menu_switch_speed : 0);

            setTimeout(function () {
                $(".sidebar-menu-toggler i").toggleClass("fa-chevron-left fa-chevron-right");
            }, speed + 10);

            if (do_visible) {
                $(".sidebar-menu-content").parent().show();
                var on_show = function () {
                    $(".sidebar-menu-panel").css("min-width", "");
                    $(".tdMenu").css({ "left": "0px" }).removeClass("hidden");
                    //$(".sidebar-menu-option").fadeIn();
                    if (from_user) {
                        onMPResize(true, 50);
                    } else {
                        if ($(".sidebar-menu-content").width() <= 0) {
                            fixSidebarWidth();
                        }
                    }
                    //if (sidebar_width !== -1) onSidebarResize(sidebar_width, false);
                    intiSidebarResizable();
                }
                if (speed) {
                    $(".tdMenu").css({ "left": -(document.getElementById("divMenu").clientWidth) + "px" }).animate({ "left": "0px" }, speed, "swing", on_show);
                } else {
                    on_show();
                }
            } else {
                var on_hide = function () {
                    $(".sidebar-menu-panel").css("min-width", "1px;");
                    $(".sidebar-menu-content").parent().hide();
                    //$(".sidebar-menu-option").fadeOut();
                    $(".tdMenu").css({ "left": "0px", "width" : ""}).addClass("hidden");
                    $(".sidebar-menu-content").css({"width": ""});
                }
                if (speed) {
                    $(".tdMenu").css({ "left": "0px" }).animate({ "left": -(document.getElementById("divMenu").clientWidth) + "px" }, speed, "swing", on_hide);
                } else {
                    on_hide();
                }
                if (from_user) setTimeout(function () {
                    onMPResize(true, 0);
                }, speed + 30);
            }
        }
    }
}

function onMPResize(force_redraw, resize_delay) {
    resize_delay = resize_delay || 50;

    if ($(".tdMenu").is(":visible")) {
        var panel_visible = !$(".tdMenu").hasClass("hidden");
        var w = $(window).width();

        if ((menu_switch_auto) && !(menu_switch_hover) && !(menu_switch_ignore)) {
            var do_collapse = (w <= menu_switch_min_width && panel_visible);
            var do_expand = (w > menu_switch_min_width && !panel_visible);
            if (do_collapse || do_expand) {
                menu_switch_ignore = true;
                toggleSideBarMenu(false, do_expand, true);
                panel_visible = !panel_visible;
                setTimeout(function () { menu_switch_ignore = false; }, menu_switch_speed + 30);
            }
        }

        $(".sidebar-menu-content").width(140);
        if (panel_visible && !menu_switch_hover && (!menu_switch_ignore && w >= menu_switch_min_width)) {
            setTimeout(function () { $("#divMainContent").css("padding-left", document.getElementById("divMenu").clientWidth * 1); }, 25);
        } else {
            setTimeout(function () { $("#divMainContent").css("padding-left", 0); }, 25);
        }
        //if (!(menu_switch_hover) || w > 0)
        var mw = $(".tdMenu").width() - $("#tdMenuToggler").width() - 1;
        $(".sidebar-menu-content").width(mw);
        if (panel_visible && mw <= 0) {
            setTimeout(fixSidebarWidth, menu_switch_speed + 10);
        }

        var has_nopinner = $(".sidebar-menu-pinner").hasClass("no-pinner");
        if (isNarrowScreen() != has_nopinner) {
            $(".sidebar-menu-pinner").toggleClass("no-pinner");
        }
    }

    if ($("#divTopMenu").length && $("#divTopMenu").is(":visible")) {
        setTimeout(function () {
            var w = $("#divTopMenu").hide().parent().width();
            w = (w > 100 && w < 1200 ? Math.floor(w) + "px" : "");
            $("#divTopMenu").show();
            $("#divTopMenu").dxTabs("instance").option("width", w);
            $("#divTopMenu").dxTabs("instance").option("showNavButtons", (w != ""));
        }, 350);
    }

    if ($(document.body).hasClass('fullscreen')) {
        setTimeout(function () { $("#divMainContent").css("padding-left", 0); }, 25);
    }
    var snapshots = $("#frmSnapshots");
    if ((snapshots) && (snapshots.length)) {
        setTimeout(function () {
            $("#frmSnapshots").width($("#MainContent").width() - 90).height($("#MainContent").height() - 120);
        }, 40);
    }

    resize_popups_();

    if ((typeof resize_ignore == "undefined" || !resize_ignore) && typeof resize_custom == "function") {
        if (resize_delay > 0) { setTimeout(function () { resize_custom_(force_redraw); }, resize_delay); } else { resize_custom_(force_redraw); }
    }
}

function resize_popups_() {
    if ($("#popupContainer").is(":visible") && $("#popupContainer").data("dxPopup")) {
        $("#popupContainer").dxPopup("option", "position", { my: 'center', at: 'center', of: window });
        var w_now = $("#popupContainer").dxPopup("option", "width");
        var h_now = $("#popupContainer").dxPopup("option", "height");
        var w_orig = 800;
        var h_orig = 600;
        if (typeof w_now != "undefined" && w_now > 0) {
            if (typeof $("#popupContainer").data("w") == "undefined") {
                $("#popupContainer").data("w", w_now);
                w_orig = w_now;
            } else {
                w_orig = $("#popupContainer").data("w") * 1;
            }
        }
        if (typeof h_now != "undefined" && h_now > 0) {
            if (typeof $("#popupContainer").data("h") == "undefined") {
                $("#popupContainer").data("h", h_now);
                h_orig = h_now;
            } else {
                h_orig = $("#popupContainer").data("h") * 1;
            }
        }

        if (w_orig * 1 < 100) w_orig = 800;
        if (h_orig * 1 < 500) w_orig = 600;

        var w = dlgMaxWidth(w_orig);
        var h = dlgMaxHeight(h_orig);
        if (w < 120) w = 120;
        if (h < 80) h = 80;

        if (w_now > 0 && Math.abs(w - w_now) > 16) $("#popupContainer").dxPopup("option", "width", w);
        if (h_now > 0 && Math.abs(h - h_now) > 16) $("#popupContainer").dxPopup("option", "height", h);
    }
}

function resize_custom_(force_redraw) {
    var mc = $("#divMainContentPlaceholder");
    var chld = $("#divMainContentPlaceholder:visible");
    var w = -1;
    var h = -1;
    if ((mc) && (chld)) {
        var o = mc.css("overflow");
        mc.css("overflow", "hidden");
        w = mc.width();
        h = mc.height();
        mc.css("overflow", o);
    }
    resize_custom(force_redraw, w, h);
}

function getStyle(oElm, strCssRule) {
    var strValue = "";
    if (document.defaultView && document.defaultView.getComputedStyle) {
        strValue = document.defaultView.getComputedStyle(oElm, "").getPropertyValue(strCssRule);
    }
    else if (oElm.currentStyle) {
        strCssRule = strCssRule.replace(/\-(\w)/g, function (strMatch, p1) {
            return p1.toUpperCase();
        });
        strValue = oElm.currentStyle[strCssRule];
    }
    return strValue;
}

function fixSidebarWidth() {
    var w = $(".tdMenu").width() - $("#tdMenuToggler").width() - 1;
    $(".sidebar-menu-content").width(w);
}


