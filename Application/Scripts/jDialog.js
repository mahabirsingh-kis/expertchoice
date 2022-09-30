/* Client-side code for jQuery.dialog() by AD // Expert Choice Inc., ver 200619 */

var jDialog_error = "Error";
var jDialog_info = "Information";
var jDialog_object = "alert_content";
var jDialog_icon = "alert_icon";
var jDialog_def_width = 450;
var jDialog_def_height = "auto";    // 205;
var jDialog_ID = "#alert_div";
var jDialog_btn_OK = "OK";
var jDialog_btn_Cancel = "Cancel";
var jDialog_btn_Extra = "Custom";

var jDialog_queue = [];
var jDialog_opened = false;
var jDialog_show_hidden =true;
var jDialog_show_icon = true;
var jDialog_on_show = "";

var jDialog_body_overflow = "";

//var jDialog_first_run = true;

function jDialog(msg, is_error, on_close, on_cancel, title, w, h, btn_yes, btn_no, btn_extra, on_extra) { //A1303
    var body_wrong = ($("body").width() < 100);

    if (jDialog_opened || (body_wrong && jDialog_queue.length==0)) {
        jDialog_queue.push([msg, is_error, on_close, on_cancel, title, w, h, btn_yes, btn_no]);
        if (jDialog_opened) return false;
    }

    if ((body_wrong)) {
        setTimeout('jDialog_next()', 500);
        return false;
    }

    var a = document.getElementById(jDialog_object);
    if ((a)) {
        a.innerHTML = msg;
        var img = document.getElementById(jDialog_icon);
        if ((img)) { img.className = "ui-icon ui-icon-" + (is_error ? "alert" : "info"); img.style.display = (jDialog_show_icon ? "block" : "none"); }
        var btns = {}
        btns.Ok = {id: 'jDialog_btnOK', text: ((btn_yes) ? btn_yes : jDialog_btn_OK), click: function () { setTimeout("$('" + jDialog_ID + "').dialog('close');", 30); if ((on_close)) eval(on_close); }};
        if ((on_cancel) && (on_cancel != ""))
            btns.cancel = { id: 'jDialog_btnCancel', text: ((btn_no) ? btn_no : jDialog_btn_Cancel), click: function () { setTimeout("$('" + jDialog_ID + "').dialog('close');", 30); if ((on_cancel)) eval(on_cancel); } }
        if ((on_extra) && (on_extra != "")) //A1303
            btns.extra = { id: 'jDialog_btnExtra', text: ((btn_extra) ? btn_extra : jDialog_btn_Extra), click: function () { setTimeout("$('" + jDialog_ID + "').dialog('close');", 30); if ((on_extra)) eval(on_extra); } }; //A1303
        $(jDialog_ID).dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "no-close",
            closeOnEscape: (on_cancel) && (on_cancel != ""),
            bgiframe: true,
            zIndex: 255,
            title: (typeof title == "undefined" || title == "" ? (is_error ? jDialog_error : jDialog_info) : title),
            width: (typeof w == "undefined" || w<0 ? jDialog_def_width : w),
            height: (typeof h == "undefined" || h<0 ? jDialog_def_height : h),
            show: { duration: 250 },
            hide: { effect: "puff", duration: 250 },
            open: function () { jDialog_opened = true; jDialog_body_overflow = $("body").css("overflow"); $("body").css("overflow", "hidden"); setTimeout('jDialog_HideSliders(true);', 55); jDialog_HideSLResults(true); if (jDialog_on_show != "") { eval(jDialog_on_show); jDialog_on_show=""; } },
            close: function () { jDialog_opened = false; $("body").css("overflow", (jDialog_body_overflow=="" ? "auto" : jDialog_body_overflow)); if (jDialog_queue.length < 1 && (jDialog_show_hidden)) { jDialog_HideSliders(false); jDialog_HideSLResults(false); }  jDialog_next(); },
            //open: function () { $('.ui-dialog').css("top", "37%").css("left", "31%"); },    // fix position for IE on jQuery 1.8+
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: btns
        });
        //if (jDialog_first_run) { setTimeout('jDialog_checkOnOpen();', 500); jDialog_first_run = false; }
    }
    return false;
}

function jDialog_checkOnOpen() {
}

function jDialog_next() {
    if (jDialog_queue.length > 0) {
        var p = jDialog_queue.shift();
        if ((p)) jDialog(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8]);
    }
}

function jDialog_HideSliders(hide) {
    try {
        var _sliders = $(".RadSlider");
        if ((_sliders)) {
            for (var i = 0; i < _sliders.length; i++) {
                var s = _sliders.get(i);
                if ((s) && (s.style)) s.style.display = (hide ? 'none' : '');
            }
        }
        _sliders = $(".sa_slider");
        if ((_sliders)) {
            for (var i = 0; i < _sliders.length; i++) {
                var s = _sliders.get(i);
                if ((s) && (s.style)) s.style.display = (hide ? 'none' : '');
            }
        }
        _sliders = $(".sa_slider_dummy");
        if ((_sliders)) {
            for (var i = 0; i < _sliders.length; i++) {
                var s = _sliders.get(i);
                if ((s) && (s.style)) s.style.display = (hide ? 'block' : 'none');
            }
        }
    }
    catch (l) {}
}

function jDialog_HideSLResults(hide) {
    var sl = $(".silverlightControlHost");
    if ((sl) && (sl.style)) sl.style.display = (hide ? "none" : "block");
}


/*
$( window ).resize(function() {
    if (($(jDialog_ID))) $(jDialog_ID).dialog("option", "position", $(jDialog_ID).dialog("option", "position"));
}); 
*/