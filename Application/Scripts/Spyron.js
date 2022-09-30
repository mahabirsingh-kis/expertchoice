// Spyron scripts, ver 171011
// (C) SL, AD // ExpertChoice Inc. 2017

var msg_unsaved = "You could have unsaved changes on this page. Do you really want to leave without saving?";

var use_plain = 0;
var parse_html = 0;
var s_manual = false;
var alert_on_exit = true;
var plain_fld = null;

function onClose() {
    var o = window.opener;
    if ((o)) o.onUpdateSurvey();
}

function onRichEditorRefresh(empty, infodoc, callback_param) {
    window.focus();
    var frm_infodoc = $get("frmRich");
    if ((frm_infodoc)) {
        setTimeout("var f = $get('frmRich'); f.src= \"" + frm_infodoc.src + "&" + Math.round(10000 * Math.random()) + "\";", 200);
        parse_html = 1;
        theForm.EditorMode.value = 1;
        theForm.isChanged.value = 1;
    }
}

function SwitchEditor(rich, is_init) {
    if (!is_init) {
        if (rich) {
            var p = $get("divPlain");
            if ((p)) p.style.display = (rich ? "none" : "block");
            var r = $get("divRich");
            if ((r)) r.style.display = (rich ? "block" : "none");
            var f = $get("frmRich");
            if ((f) && (plain_fld)) f.contentWindow.document.body.innerHTML = replaceString("\n", "<br>", replaceString("<", "&lt;", replaceString(">", "&gt;", plain_fld.value)));
            use_plain = 1;
        }
        else {
            if (confirm('Are you sure want to convert rich content to plain text?')) {
                var p = $get("divPlain");
                if ((p)) p.style.display = (rich ? "none" : "block");
                var r = $get("divRich");
                if ((r)) r.style.display = (rich ? "block" : "none");
                parseHTML();
            } else {
                return false;
            }
        }
        document.cookie = "survey_rich_editor=" + (rich ? "1" : "0");
    }
    theForm.EditorMode.value = rich;
    theForm.isChanged.value = 1;
}

function parseHTML() {
    var f = $get("frmRich");
    if (parse_html && (f) && (plain_fld)) {
        var re = /<\S[^><]*>/g;
        var txt = f.contentWindow.document.body.innerHTML.replace(re, "");
        plain_fld.value = replaceString("&lt;", "<", replaceString("&gt;", ">", txt));
        plain_fld.focus();
        parse_html = 0;
    }
}

function CheckExitEvent() {
    if ((alert_on_exit) && isModifiedForm(theForm)) {
        var res = confirm(msg_unsaved);
        if (res) alert_on_exit = false;
        return res;
    }
    return true;
}

function isModifiedForm(form) {
    if ((theForm.isChanged) && (theForm.isChanged.value != "0")) return true;
    for (var i = 0; i < form.elements.length; i++) {
        var e = form.elements[i];
        if (typeof (e.defaultValue) !== 'undefined' && e.type!="checkbox" && e.value != e.defaultValue) return true;
        if (typeof (e.defaultChecked) !== 'undefined' && e.checked != e.defaultChecked) return true;
        if (typeof (e.defaultSelected) !== 'undefined' && e.selected != e.defaultSelected) return true;
        if (e.id!="" && $("#" + e.id).length && $("#" + e.id).data("defaultValue") != null && e.value != $("#" + e.id).data("defaultValue")) return true;
    }
    return (false);
}

function initSurveyForm() {
    $("input[id$='CancelButton']").on('click', function () { alert_on_exit = false; });
    $("input[id$='FinishButton']").on('click', function () { alert_on_exit = false; });
    $("select").each(function () { $("#" + this.id).data("defaultValue", this.value); } )
}

window.onunload = onClose;
window.onbeforeunload = function () { if ((alert_on_exit) && isModifiedForm(theForm)) return msg_unsaved; };
$(document).ready(function () { initSurveyForm() });
