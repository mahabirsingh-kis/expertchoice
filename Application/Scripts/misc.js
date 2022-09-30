/* Miscellaneous javascript routines | (C) DA/AD v.220324 */

var is_firefox = navigator.userAgent.toLowerCase().indexOf('firefox') > -1;
var is_chrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
var is_edge = (document.documentMode>11 || /Edge/.test(navigator.userAgent) || /Edg/.test(navigator.userAgent));
var is_ie = (navigator.userAgent.indexOf('MSIE') !== -1 || navigator.appVersion.indexOf('Trident/') > 0);
var is_canvas = !!window.HTMLCanvasElement;
var is_svg3 = true;

var _ajax_xhr;
var _ajax_active = false;
var _ajax_ok_code = "";
var _ajax_error_code = "";
var _ajax_show_error = true;
var _ajax_error_msg = "Unable to complete this request due to an error.";
var _ajax_error_403 = "User is not authorized.";
var _ajax_error_404 = "Page not found.";
var _ajax_error_408 = "Request timeout.";
var _ajax_error_500 = "Internal error occured. Please contact with Customer Support.";
var _ajax_error_503 = "Request is not allowed.";
var _ajax_error_sess = "Your session has been changed or expired.";
var _ajax_error_pid = "Looks like you opened another project in the other browser tab.";
var _ajax_error_no_prj = "Looks like you closed this project in another browser tab.";
var _ajax_reload_msg = "Please click &quot;OK&quot; to reload the page.";

var _ajax_timeout = 1000 * 600;
var _ajax_last_dt = new Date();

var _sess_idx_sid = 0;
var _sess_idx_uid = 1;
var _sess_idx_pid = 2;

var TABKEY   = 9;
var ENTERKEY = 13;

var KEYCODE_TAB = 9;
var KEYCODE_ENTER = 13;
var KEYCODE_LEFT = 37;
var KEYCODE_UP = 38;
var KEYCODE_RIGHT = 39;
var KEYCODE_DOWN = 40;
var KEYCODE_ESCAPE = 27;
var KEYCODE_PERIOD = 190; // "." key

var CHAR_DELTA = "&#8710;";
var CHAR_UNDEFINED_ATTR = "- undefined -";

var glyph_opacity_normal = '0.75';
var glyph_opacity_hover = '1.0';
var glyph_opacity_disabled = '0.4';

var guid_empty = '00000000-0000-0000-0000-000000000000';

var UNDEFINED_INTEGER_VALUE = -2147483648;

var CurrencyThousandSeparator = "";

var text = ("textContent" in document) ? "textContent" : "innerText"; //for FF
    /* usage: in PreRender handler assign innerHtml and then in js code
    var d = $get("<% =divData.ClientID %>");
    if ((d)) data = eval(cd[text]);
    */

function getCookieDomain(custom_url) {
    var url = ((typeof custom_url != "undefined" && custom_url != "") ? custom_url : document.location.hostname);
    if (url.toLowerCase().indexOf("localhost") >= 0) {
        url = "";
    } else {
        var p = url.split(".");
        if (p.length < 2) {
            url = "";
        } else {
            url = "";
            //var c = (p.length>3 ? 3 : 2);
            for (var i = 1; i <= 2; i++) {
                url += "." + p[p.length - i];
            }
        }
    }
    return url;
}

function setCookieCrossDomain(name, val) {
    document.cookie = name + "=" + val + ";path=/;domain=" + getCookieDomain() +";expires=Mon, 31-Dec-2029 23:59:59 GMT;";
}

function getWindowHeight() {
    return window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight
}

/* numbers */
function validFloat(s) {
    s = replaceString(((CurrencyThousandSeparator)) ? CurrencyThousandSeparator : " ", "", s + "");
    return !isNaN(parseFloat(replaceString(",", ".", s + "")));
}

function str2double(s) {
    s = replaceString(CurrencyThousandSeparator, "", s + "");
    return parseFloat(replaceString(",", ".", s + ""));
}

function str2bool(s) {
    switch (s.toLowerCase().trim()) {
        case "true":
        case "yes":
        case "on":
        case "1":
            return true;
        default:
            return false;
    }
    //var v = (s + "").toLowerCase().trim();
    //return (v == "true" || v == "yes" ||  v == "on" || v == "1");
}

function clearString(str) { // use for clean memory after big string using
    return JSON.parse(JSON.stringify(str));
}

function clearString(str) { // use for clean memory after big string using
    return JSON.parse(JSON.stringify(str));
}

function validInteger(s) {
    return !isNaN(parseInt(s, 10));
}

function trim(s) {
    return s.replace(/^\s+|\s+$/gm, '');
}

function str2int(s) {
    return parseInt(s, 10);
}

function float2int(value) {
    return parseInt(value, 10);
}

function num2prc(value, decimals, show_udefined_as_tilda_zero) {
    return value == "" || value == UNDEFINED_INTEGER_VALUE ? (typeof show_udefined_as_tilda_zero == "undefined" || show_udefined_as_tilda_zero == false ? "" : "~0%") : (value * 100).toFixed(typeof decimals == "undefined" ? 2 : decimals) + "%";
}

function str2cost(val) {
    var s = val;
    if ((s.length > 5 && s[s.length - 5] == ",") || (s.length > 3 && s[s.length - 3] == ",") || (s.length > 2 && s[s.length - 2] == ",")) s = replaceString(",", ".", s);
    s = replaceString(",", ".", replaceString(CurrencyThousandSeparator, "", s)); //A0922
    //A0907 === remove thousands separators and currency signs
    s = s.replace(/[^\-0-9.]/g, "");
    var last = s.lastIndexOf('.');
    if ((last > 0) && (last < s.length)) s = replaceString(".", "", s.substring(0, last)) + s.substring(last);
    //A0907 ==        
    if (s == "0.") s = "0";
    if (s == "1.") s = "1";
    if (s[0] == ".") s = "0" + s;
    if (s[0] == "-" && s[1]==".") s = "-0" + s.substring(1);
    var v = s * 1;
    if (s == v && s != "") {
        //            edit.value = v;
        return v;
    }
    else {
        return "undefined";
    };
}

function ip2num(ip) {
    var d = ip.split('.');
    return ((((((+d[0]) * 256) + (+d[1])) * 256) + (+d[2])) * 256) + (+d[3]);
}

function num2ip(val) {
    var d = val % 256;
    for (var i = 3; i > 0; i--) {
        val = Math.floor(val / 256);
        d = val % 256 + '.' + d;
    }
    return d;
}

function id2hid(id) {
    switch (id) {
        case 0:
        case "0":
            return "likelihood";
        case 2:
        case "2":
            return "impact";
    }
    return "";
}

function hid2id(hid) {
    switch (hid) {
        case "likelihood":
            return 0;
        case "impact":
            return 2;
    }
    return -1;
}

function list2params(lst) {
    var res = [];
    $.each(lst, function (idx, val) {
        res.push(idx + "=" + encodeURIComponent(val));
    });
    return res.join("&")
}

function params2list(url) {
    var params = {};
    var pairs = url.substring(url.indexOf('?') + 1).split('&');
    for (var i=0; i<pairs.length; i++) {
        if (pairs[i] && pairs[i].indexOf("=")>0) {
            var pair = pairs[i].split('=');
            params[pair[0]] = decodeURIComponent(pair[1]);
        }
    }
    return params;
}

function joinlists(base_list, pty_list) {
    var lst = {};
    for (var id in base_list) {
        lst[id] = base_list[id];
    }
    for (var id in pty_list) {
        lst[id] = pty_list[id];
    }
    return lst;
}

function GetCurrencyDecimalSeparator() {
    return (1.1).toLocaleString('en-US').substring(1, 2); //A1376
}

var CurrencyDecimalSeparator = GetCurrencyDecimalSeparator();

function number2locale(n, skipZeroCents, roundCents) {
    n = (n * 1);
    var retVal = Number(roundTo(n, 2)).toLocaleString('en-US'); //A1376
    var retVal_len = retVal.length;
    var s = retVal.indexOf(CurrencyDecimalSeparator);
    if (s !== -1 && s + 3 > retVal_len) retVal += "0";
    if ((typeof skipZeroCents) == "undefined" || !skipZeroCents) {
        if (s === -1) retVal += CurrencyDecimalSeparator + "00";
    } else {
        if (retVal.indexOf(CurrencyDecimalSeparator + "00", retVal_len - 3) !== -1) {
            retVal = retVal.substring(0, retVal_len - 3);
        }
    }
    if (typeof roundCents !== "undefined" && roundCents && retVal.indexOf(CurrencyDecimalSeparator) !== -1) {
        s = retVal.indexOf(CurrencyDecimalSeparator);
        retVal = retVal.substring(0, s);
    }
    return retVal;
}

var _OPT_MaxDecimalPlaces = 14;
var _OPT_EPS = Math.pow(10, -_OPT_MaxDecimalPlaces - 1);

function roundTo(n, digits) {
    digits = digits || 0;
    return Math.round(n * Math.pow(10, digits)) / Math.pow(10, digits);
}

function jsDouble2String(Value, MinDecimalPlaces, tPercentSign) {
    if (isNaN(Value)) Value = 0;

    var dp = MinDecimalPlaces;
    if (Value != 0) {
        while ((dp <= _OPT_MaxDecimalPlaces) && (Math.abs(roundTo(Value, dp)) <= _OPT_EPS)) {
            dp += 1;
        }
    }

    if (dp > _OPT_MaxDecimalPlaces) dp = MinDecimalPlaces;

    var sValue = roundTo(Value, dp) + (tPercentSign ? "%" : "");
    return sValue;
}

function jsDouble2Fixed(Value, DecimalPlaces, tPercentSign) {
    if (isNaN(Value)) Value = 0;

    var sValue = Value.toFixed(DecimalPlaces) + (tPercentSign ? "%" : "");
    return sValue;
}

// show $cost with extra options: show very tiny values (~$0), show Millions(M), Billions(B)
function showCost(cost, skipZeroCents, skipNonZeroCents, allowK, allowMB) {
    //if (cost < -1073741824) return "";  // "undefined"
    if (cost < -2147483648) return "";  // "undefined"

    allowK = typeof allowK == "undefined" ? false : allowK;
    allowMB = typeof allowMB == "undefined" ? false : allowMB;

    if ((typeof skipNonZeroCents) != 'undefined') {
        if (skipNonZeroCents) {
            skipZeroCents = true;
            cost = Math.round(cost);
        }
    }

    var is_negative = cost < 0;
    cost = Math.abs(cost);

    var c = "";
    var d = 1;
    if (allowK && cost > 1000) { c = "K"; d = 1000; }
    if (allowMB && cost > 1000000) { c = "M"; d = 1000000; }
    if (allowMB && cost > 1000000000) { c = "B"; d = 1000000000; }
    return (cost < 0.01 && cost >= 0.0001 ? "~" : "") + "$" + (is_negative ? "-" : "") + number2locale((d > 1 ? cost / d : cost), skipZeroCents, false) + c; // A1219 //, d > 1 && cost > 10000
}

/* strings */
function ShortString(str, maxlen, is_encoding) {
    //str = replaceString('"', '&quot;', replaceString("'", "&#39;", str));
    if (typeof str != "undefined") {
        if (typeof is_encoding == "undefined" || is_encoding) {
            return str.length > maxlen ? htmlEscape(str.substr(0, maxlen)) + "&hellip;" : htmlEscape(str);
        } else {
            return str.length > maxlen ? str.substr(0, maxlen) + "…" : str;
        }
    }
}

function stripHTMLTags(testString) {    
    return $("<div>" + testString + "</div>").text().trim();
}

function html2text(html) {
    var text = html.replace(/\n/gi, "");
    text = text.replace(/<style([\s\S]*?)<\/style>/gi, "");
    text = text.replace(/<script([\s\S]*?)<\/script>/gi, "");
    text = text.replace(/<iframe([\s\S]*?)<\/iframe>/gi, "");
    text = text.replace(/<a.*?href="(.*?)[\?\"].*?>(.*?)<\/a.*?>/gi, " $2");
    //text = text.replace(/<a.*?href="(.*?)[\?\"].*?>(.*?)<\/a.*?>/gi, " $2 $1 ");
    text = text.replace(/<\/div>/gi, "\n\n");
    text = text.replace(/<\/li>/gi, "\n");
    text = text.replace(/<li.*?>/gi, "  *  ");
    text = text.replace(/<\/ul>/gi, "\n\n");
    text = text.replace(/<\/p>/gi, "\n\n");
    text = text.replace(/<br\s*[\/]?>/gi, "\n");
    text = text.replace(/<[^>]+>/gi, "");
    text = text.replace(/^\s*/gim, "");
    text = text.replace(/ ,/gi, ",");
    //text = text.replace(/ +/gi, " ");
    text = text.replace(/\n+/gi, "\n\n");
    return stripHTMLTags(text);
}

String.prototype.format = String.prototype.f = function () {
    var s = this,
        i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
}

function str2Blob(binStr, mime) {
    var len = binStr.length;
    var arr = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        arr[i] = binStr.charCodeAt(i);
    }
    return new Blob([arr], { type: mime || 'image/png' });
}

if (!HTMLCanvasElement.prototype.toBlob) {
    Object.defineProperty(HTMLCanvasElement.prototype, 'toBlob', {
        value: function (callback, type, quality) {
            var dataURL = this.toDataURL(type, quality).split(',')[1];
            setTimeout(function () {
                callback(str2Blob(atob(dataURL), type));
            });
        }
    });
}

function checkGlyphButtons() {
    $(".btn_glyph22:enabled").css('opacity', glyph_opacity_normal);
    $(".btn_glyph22:disabled").css('opacity', glyph_opacity_disabled);
    $(".btn_glyph22").hover(function () { $(this).addClass('btn_glyph22_hover').css('opacity', glyph_opacity_hover); }, function () { $(this).removeClass('btn_glyph22_hover').css('opacity', glyph_opacity_normal); });
}

function disableGlyphButton(btn, is_disabled) {
    if ((btn) && !(btn instanceof jQuery) && btn.id != "") { btn = $("#" + btn.id); }
    if ((btn) && (btn instanceof jQuery)) {
        if ((is_disabled)) btn.prop('disabled', true); else btn.prop('disabled', false);
        //btn.attr('disabled', ((is_disabled) ? 'disabled' : ''));
        btn.css('opacity', ((is_disabled) ? glyph_opacity_disabled : glyph_opacity_normal));
//        alert(is_disabled + " = " + btn.css('opacity') + " - " + btn.attr('disabled'));
    }
}

function htmlEscape(str) {
    return String(str)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
}

function htmlUnescape(str) {
    return String(str)
            .replace(/&amp;/g, '&')
            .replace(/&quot;/g, '"')
            .replace(/&#39;/g, '\'')
            .replace(/&lt;/g, '<')
            .replace(/&gt;/g, '>;')
            .replace(/&hellip;/g, '\u2026');
}

function jsEscape(str) {
    return replaceString('\"', '"', replaceString("\\'", "'", str));
}

function htmlStrip(html) { // remove HTML tags from string
    var div = document.createElement("div");
    div.innerHTML = html;
    return div.textContent || div.innerText || "";
}

function IsAction(sAction, cmd) {
    return (cmd == "action=" + sAction) || (cmd.indexOf("action=" + sAction + "&") == 0);
}

function isAction(sAction, cmd) {
    return IsAction(sAction, cmd);
}

function padWithZeros(value, maxValue) {
    var digitPlaces = function (b) {
        var N = 0;
        do {
            b = Math.floor(b / 10);
            N += 1;
        } while (b > 0)
        return N;
    };

    var N = digitPlaces(maxValue);
    var P = digitPlaces(value);
    var retVal = value + "";

    for (var i = P; i < N; i++) {
        retVal = "0" + retVal;
    }
    return retVal;
}


var BKG_FUNDED = "#d1e6b5";
var BKG_FUNDED_HOVER = "#bad991";
var BKG_DEFAULT_HOVER = "#f0f5ff";
var BKG_DEFAULT_ALT_HOVER = "#eaf0f5";
var BKG_SUMMARY = 'rgb(240, 245, 255)';
var COLOR_FUNDED_STATE = "#009900";
var COLOR_SELECTION_BORDER = "#ff9933";
var TEXT_DISABLED = "#999999";

/* Grid View */
function RowHover(r, hover, alt, summary) { //no hover if summary row
    if ((r)) {
        if (((typeof summary) == "undefined") || (summary == 0)) {
            var bg = (hover == 1 ? (alt ? BKG_DEFAULT_ALT_HOVER : BKG_DEFAULT_HOVER) : (alt ? "#fafafa" : "#ffffff"));
            for (var i = 0; i < r.cells.length; i++) {
                var cell = r.cells[i];
                if (cell.className != "funded_cell") { cell.style.background = bg; } else {cell.style.background = (hover == 1 ? BKG_FUNDED_HOVER : BKG_FUNDED); }
            }
        }
    }
}

/* Colors */
var ColorPalette = ["#2c75ff", "#fa7000", "#9d27a8", "#478430", "#6c3b2a", "#9e2373", "#f24961", "#663d2e", "#9600fa",
                    "#ffbde6", "#00c49f", "#7280c4", "#009180", "#e33000", "#80bdff", "#a10040", "#0affe3", "#00523c", 
                    "#919100", "#5c00f7", "#a15f00", "#cce6ff", "#00465c", "#adff69", "#f24ba0", "#0dff87", "#ff8c47", 
                    "#349400", "#b3b300", "#a10067", "#ba544a", "#edc2d1", "#00e8c3", "#3f0073", "#5ec1f7", "#6e00b8", 
                    "#f5f5c4", "#e33000", "#52ba00", "#ff943b", "#0079db", "#f0e6c0", "#ffb517", "#cf0076", "#e8cfc9"];


var ColorPickerPalette = ["#5451FF", "#00967F", "#DB0000", "#356383", "#068200", "#898200", "#DB7C00", "#DB008A", "#9d27a8", "#555555",
                          "#837CFF", "#00C1A7", "#FF0000", "#367AAB", "#09B500", "#C1B853", "#FF8C00", "#FF0CA6", "#B24FFF", "#888888",
                          "#A6A3FF", "#0BE2C2", "#FF5656", "#4791C5", "#0CFF00", "#FFFF00", "#FFA33A", "#FF3DBB", "#C277FF", "#cccccc",
                          "#B6B5FF", "#23FFE1", "#FFA8A8", "#68A6D0", "#70FF68", "#FFFF56", "#FFB260", "#FF6DCC", "#D39EFF", "#eeeeee",
                          "#C6D4FF", "#9EFFEE", "#FFE7E7", "#8ABBDB", "#A0FF6A", "#FFFFA6", "#FFC993", "#FFA8E2", "#E3AEFF", "#ffffff"];

//var DistinctColorPalette = ["#50BFE6", "#ED0A3F", "#FF6037", "#FFCC33", "#FFFF66", "#66FF66", "#50BFE6", "#FF6EFF", "#299617", "#2243B6", "#A83731", "#AF6E4D", "#58427C", "#0081AB", "#757575", "#CCFF00"];
var DistinctColorPalette = ['#e6194b', '#3cb44b', '#ffe119', '#4363d8', '#f58231', '#911eb4', '#663d2e', '#bcf60c', '#800000', '#aaffc3', '#808000', '#009999', '#444444'];

var jqPlotDefaultColors = ["#4bb2c5", "#EAA228", "#c5b47f", "#579575", "#839557", "#958c12", "#953579", "#4b5de4", "#d8b83f", "#ff5800", "#0085cc", "#c747a3", "#cddf54", "#FBD178", "#26B4E3", "#bd70c7"];

var ec_palettes = [
["Default", "#fafafa", ["#95c5f0\n#fa7000\n#9d27a8\n#e3e112\n#00687d\n#407000\n#f24961\n#663d2e\n#9600fa\n#ffbde6\n#00c49f\n#7280c4\n#009180\n#e33000\n#80bdff\n#a10040\n#0affe3\n#00523c\n#919100\n#5c00f7\n#a15f00\n#cce6ff\n#00465c\n#adff69\n#f24ba0\n#0dff87\n#ff8c47\n#349400\n#b3b300\n#a10067\n#ba544a\n#edc2d1\n#00e8c3\n#3f0073\n#5ec1f7\n#6e00b8\n#f5f5c4\n#e33000\n#52ba00\n#ff943b\n#0079db\n#f0e6c0\n#ffb517\n#cf0076\n#e8cfc9"]],
["Colorblind 1", "#f4cccc", ["#d54a4a\n#44cc7c\n#554bb3\n#a6bc3a\n#577ceb\n#c1a830\n#887adf\n#76c661\n#903d99\n#80a237\n#d871d2\n#428129\n#4b2c79\n#d6902c\n#468ae0\n#b4501f\n#49d4ba\n#c54530\n#41ad79\n#913078\n#78bb73\n#a7306a\n#346e2e\n#d978be\n#a8bd69\n#4e5da7\n#c5ac4f\n#b585d6\n#747022\n#66a1e5\n#c37f34\n#de72a1\n#d09f5f\n#ce4462\n#8d531d\n#983a50\n#df7d52\n#842721\n#da6b6e\n#d2745b"]],
["Colorblind 2", "#f4cccc", ["#983a50\n#ce4462\n#da6b6e\n#d54a4a\n#842721\n#c54530\n#d2745b\n#df7d52\n#b4501f\n#8d531d\n#c37f34\n#d6902c\n#d09f5f\n#c5ac4f\n#c1a830\n#747022\n#a6bc3a\n#a8bd69\n#80a237\n#428129\n#76c661\n#346e2e\n#78bb73\n#44cc7c\n#41ad79\n#49d4ba\n#66a1e5\n#468ae0\n#577ceb\n#4e5da7\n#887adf\n#554bb3\n#4b2c79\n#b585d6\n#903d99\n#d871d2\n#d978be\n#913078\n#de72a1\n#a7306a"]],
["Colorblind 3", "#f4cccc", ["#66a1e5\n#d09f5f\n#983a50\n#747022\n#49d4ba\n#346e2e\n#8d531d\n#4e5da7\n#a8bd69\n#d2745b\n#78bb73\n#41ad79\n#842721\n#da6b6e\n#de72a1\n#b585d6\n#c5ac4f\n#4b2c79\n#468ae0\n#d978be\n#df7d52\n#913078\n#a7306a\n#c37f34\n#428129\n#80a237\n#887adf\n#ce4462\n#903d99\n#b4501f\n#76c661\n#c1a830\n#44cc7c\n#d54a4a\n#d871d2\n#d6902c\n#554bb3\n#577ceb\n#c54530\n#a6bc3a"]],
["Colorblind 4", "#f4cccc", ["#4b2c79\n#842721\n#913078\n#554bb3\n#983a50\n#a7306a\n#903d99\n#8d531d\n#346e2e\n#4e5da7\n#747022\n#b4501f\n#c54530\n#428129\n#ce4462\n#d54a4a\n#577ceb\n#887adf\n#468ae0\n#da6b6e\n#d2745b\n#c37f34\n#de72a1\n#80a237\n#df7d52\n#d871d2\n#b585d6\n#d978be\n#41ad79\n#66a1e5\n#d6902c\n#d09f5f\n#c1a830\n#78bb73\n#c5ac4f\n#a6bc3a\n#76c661\n#a8bd69\n#44cc7c\n#49d4ba"]],
["Intense 1", "#cfe2f3;", ["#922b60\n#53d131\n#9d40e1\n#49b745\n#d63fdc\n#adbb2f\n#502cbb\n#78b044\n#6c61e4\n#d5a22d\n#443590\n#54ba78\n#c952cf\n#4f752f\n#d83fab\n#9aa14a\n#812a94\n#dd7b2a\n#5478d3\n#e1462a\n#4ea7ba\n#d14155\n#63af92\n#e34c87\n#385530\n#a671d7\n#c4944a\n#633871\n#b2a174\n#d175bc\n#7d5928\n#789ed4\n#a54325\n#3d5682\n#e28e71\n#b08ecd\n#6a2b24\n#d594aa\n#6d3b55\n#af6265"]],
["Intense 2", "#cfe2f3;", ["#d14155\n#af6265\n#6a2b24\n#e1462a\n#a54325\n#e28e71\n#dd7b2a\n#7d5928\n#c4944a\n#d5a22d\n#b2a174\n#adbb2f\n#9aa14a\n#78b044\n#4f752f\n#53d131\n#385530\n#49b745\n#54ba78\n#63af92\n#4ea7ba\n#789ed4\n#3d5682\n#5478d3\n#6c61e4\n#443590\n#502cbb\n#b08ecd\n#a671d7\n#9d40e1\n#633871\n#812a94\n#c952cf\n#d63fdc\n#d175bc\n#d83fab\n#6d3b55\n#922b60\n#d594aa\n#e34c87"]],
["Intense 3", "#cfe2f3;", ["#b2a174\n#385530\n#6d3b55\n#4ea7ba\n#d594aa\n#3d5682\n#63af92\n#789ed4\n#6a2b24\n#af6265\n#7d5928\n#b08ecd\n#633871\n#e28e71\n#4f752f\n#9aa14a\n#c4944a\n#922b60\n#d175bc\n#54ba78\n#5478d3\n#a54325\n#443590\n#a671d7\n#78b044\n#d14155\n#e34c87\n#d5a22d\n#812a94\n#dd7b2a\n#adbb2f\n#49b745\n#d83fab\n#6c61e4\n#c952cf\n#e1462a\n#502cbb\n#53d131\n#d63fdc\n#9d40e1"]],
["Intense 4", "#cfe2f3;", ["#6a2b24\n#443590\n#633871\n#6d3b55\n#502cbb\n#385530\n#812a94\n#922b60\n#3d5682\n#7d5928\n#a54325\n#4f752f\n#9d40e1\n#6c61e4\n#d14155\n#af6265\n#5478d3\n#e1462a\n#d83fab\n#c952cf\n#d63fdc\n#e34c87\n#a671d7\n#dd7b2a\n#d175bc\n#4ea7ba\n#9aa14a\n#b08ecd\n#789ed4\n#c4944a\n#63af92\n#78b044\n#49b745\n#b2a174\n#e28e71\n#d594aa\n#54ba78\n#d5a22d\n#adbb2f\n#53d131"]],
["Fancy 1", "#fffae0", ["#b1c8eb\n#fde096\n#74aff3\n#d8f3af\n#b9b5f3\n#afc987\n#e6afda\n#7bf7e1\n#f2a3b6\n#70d3c1\n#f5b391\n#67c6f2\n#f2e2a9\n#88aee1\n#d5dfa0\n#9cc2f7\n#9ab474\n#cab8e5\n#9fe0b2\n#ebb9c7\n#55cdd8\n#e6a79c\n#91edf0\n#e5bb96\n#8bdcee\n#d0c28a\n#90cfeb\n#b4b67a\n#91b6d6\n#cfe6b8\n#86cbd1\n#eadab6\n#9beed6\n#c3bc96\n#b6eee2\n#8ac294\n#94cabf\n#c1edcf\n#80b6aa\n#98c3a6"]],
["Fancy 2", "#fffae0", ["#f2a3b6\n#e6a79c\n#f5b391\n#e5bb96\n#fde096\n#eadab6\n#f2e2a9\n#d0c28a\n#c3bc96\n#b4b67a\n#d5dfa0\n#afc987\n#9ab474\n#d8f3af\n#cfe6b8\n#8ac294\n#9fe0b2\n#c1edcf\n#98c3a6\n#9beed6\n#80b6aa\n#b6eee2\n#7bf7e1\n#70d3c1\n#94cabf\n#91edf0\n#86cbd1\n#55cdd8\n#8bdcee\n#90cfeb\n#67c6f2\n#91b6d6\n#74aff3\n#b1c8eb\n#88aee1\n#9cc2f7\n#b9b5f3\n#cab8e5\n#e6afda\n#ebb9c7"]],
["Fancy 3", "#fffae0", ["#eadab6\n#94cabf\n#b1c8eb\n#ebb9c7\n#b6eee2\n#80b6aa\n#c3bc96\n#91b6d6\n#c1edcf\n#98c3a6\n#86cbd1\n#90cfeb\n#cab8e5\n#cfe6b8\n#8bdcee\n#e6a79c\n#e5bb96\n#91edf0\n#88aee1\n#f2e2a9\n#d0c28a\n#9cc2f7\n#e6afda\n#9beed6\n#f2a3b6\n#b4b67a\n#8ac294\n#d5dfa0\n#f5b391\n#70d3c1\n#b9b5f3\n#55cdd8\n#9fe0b2\n#67c6f2\n#9ab474\n#d8f3af\n#afc987\n#74aff3\n#fde096\n#7bf7e1"]],
["Fancy 4", "#fffae0", ["#9ab474\n#74aff3\n#88aee1\n#80b6aa\n#91b6d6\n#b4b67a\n#8ac294\n#e6a79c\n#f2a3b6\n#98c3a6\n#67c6f2\n#c3bc96\n#b9b5f3\n#55cdd8\n#94cabf\n#9cc2f7\n#86cbd1\n#cab8e5\n#e6afda\n#afc987\n#f5b391\n#d0c28a\n#70d3c1\n#e5bb96\n#ebb9c7\n#90cfeb\n#b1c8eb\n#8bdcee\n#9fe0b2\n#d5dfa0\n#eadab6\n#91edf0\n#9beed6\n#cfe6b8\n#f2e2a9\n#7bf7e1\n#fde096\n#c1edcf\n#b6eee2\n#d8f3af"]]
];

var AlternativeUniformColor = "#7d9b3c";
var ObjectiveUniformColor = "#060cc3";

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? [parseInt(result[1], 16),
                     parseInt(result[2], 16),
                     parseInt(result[3], 16)] : null;
}

function pickHex(hexcolor1, hexcolor2, weight) {
    var p = weight;
    var w = p * 2 - 1;
    var w1 = (w / 1 + 1) / 2;
    var w2 = 1 - w1;
    var color1 = hexToRgb(hexcolor1);
    var color2 = hexToRgb(hexcolor2);
    var rgb = [Math.round(color1[0] * w1 + color2[0] * w2),
        Math.round(color1[1] * w1 + color2[1] * w2),
        Math.round(color1[2] * w1 + color2[2] * w2)];
    return 'rgb(' + rgb.join() + ')';
}

function heatMapColor(colorLow, colorMid, colorHigh, riskLow, riskHigh, riskValue, mode) {
    if (mode == 0) {
        if (riskValue <= riskLow) {
            var w = (riskLow - riskValue) / riskLow;
            return pickHex(colorMid, colorLow, 1 - w);
        }
        if (riskValue >= riskHigh) {
            var w = (riskValue - riskHigh) / (100 - riskHigh);
            return pickHex(colorMid, colorHigh, 1 - w);
        }
    }
    if (mode == 1) {
        var low = riskLow + (riskHigh - riskLow) / 3;
        if (riskValue <= (low)) {
            var w = (low - riskValue) / low;
            return pickHex(colorMid, colorLow, 1 - w);
        }
        var high = riskHigh - (riskHigh - riskLow) / 3;
        if (riskValue >= high) {
            var w = (riskValue - high) / (100 - high);
            return pickHex(colorMid, colorHigh, 1 - w);
        }
    }
    if (mode == 2) {
        var low = riskLow + (riskHigh - riskLow) / 3;
        var midlow = riskLow / 2;
        if (riskValue < midlow) return colorLow;
        if (riskValue <= (low) && (riskValue >= midlow)) {
            var w = (low - riskValue) / (low - midlow);
            return pickHex(colorMid, colorLow, 1 - w);
        }
        var high = riskHigh - (riskHigh - riskLow) / 3;
        var midhigh = high + (100 - riskHigh) / 2;
        if (riskValue > midhigh) { return colorHigh; }
        if (riskValue >= high) {
            var w = (riskValue - high) / (midhigh - high);
            return pickHex(colorMid, colorHigh, 1 - w);
        }
    }
    if (mode == 3) {
        var gradientArea = (riskHigh - riskLow) / 4;
        var low = riskLow + gradientArea;
        var midlow = riskLow - gradientArea;
        if (riskValue < midlow) { return colorLow; }
        if (riskValue <= (low) && (riskValue >= midlow)) {
            var w = (low - riskValue) / (low - midlow);
            return pickHex(colorMid, colorLow, 1 - w);
        }
        var high = riskHigh - gradientArea;
        var midhigh = high + gradientArea * 3;
        if (riskValue > midhigh) { return colorHigh; }
        if (riskValue >= high) {
            var w = (riskValue - high) / (midhigh - high);
            return pickHex(colorMid, colorHigh, 1 - w);
        }
    }
    if (mode == 4) { //A1433
        var gradientAreaLow = riskLow / 2;
        var low = riskLow;
        var midlow = riskLow - gradientAreaLow;
        if (riskValue < midlow) return colorLow;
        if (riskValue <= low) {
            var w = (low - riskValue) / (low - midlow);
            return pickHex(colorMid, colorLow, 1 - w);
        }
        var rm = riskHigh - riskLow;
        var rh = 100 - riskHigh;
        var gradientAreaHigh = (rm > rh ? rh : rm) / 2;
        var high = riskHigh;
        var midhigh = high + gradientAreaHigh;
        if (riskValue > midhigh) return colorHigh;
        if (riskValue >= high) {
            var w = (riskValue - high) / (midhigh - high);
            return pickHex(colorMid, colorHigh, 1 - w);
        }
    }
    return colorMid;
}

function averageColor(color1, color2) {
    var avg = function (a, b) { return (a + b) / 2 },
        t16 = function (c) { return parseInt(('' + c).replace('#', ''), 16) },
        hex = function (c) { return (c >> 0).toString(16) },
        hex1 = t16(color1),
        hex2 = t16(color2),
        r = function (hex) { return hex >> 16 & 0xFF },
        g = function (hex) { return hex >> 8 & 0xFF },
        b = function (hex) { return hex & 0xFF },
        res = '#' + hex(avg(r(hex1), r(hex2))) + hex(avg(g(hex1), g(hex2))) + hex(avg(b(hex1), b(hex2)));
    return res;
}

function colorBrightness(hex, percent) {
    hex = hex.replace(/^\s*#|\s*$/g, '');

    if (hex.length == 3) {
        hex = hex.replace(/(.)/g, '$1$1');
    }

    var r = parseInt(hex.substr(0, 2), 16),
        g = parseInt(hex.substr(2, 2), 16),
        b = parseInt(hex.substr(4, 2), 16);

    return '#' +
       ((0 | (1 << 8) + r + (256 - r) * percent / 100).toString(16)).substr(1) +
       ((0 | (1 << 8) + g + (256 - g) * percent / 100).toString(16)).substr(1) +
       ((0 | (1 << 8) + b + (256 - b) * percent / 100).toString(16)).substr(1);
}

function rgbColorToHex(colorval) {
    var parts = colorval.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/);
    delete (parts[0]);
    for (var i = 1; i <= 3; ++i) {
        parts[i] = parseInt(parts[i]).toString(16);
        if (parts[i].length == 1) parts[i] = '0' + parts[i];
    }
    color = '#' + parts.join('');

    return color;
}

function contrastColor(brkgColor) {
    var rgb = brkgColor.indexOf("#") > -1 ? hexToRgb(brkgColor) : brkgColor.replace(/[^\d,]/g, '').split(','); // hex color string or rgb color string
    if (rgb[0] + rgb[1] + rgb[2] > 400) {
        return "#000000";
    } else {
        return "#FFFFFF";
    }
}

/* HTML 5 Canvas functions */
function canvasTextWidth(context, text) {
    var lineMetrics = context.measureText(text);
    return lineMetrics.width;
}

function wrapText(context, text, x, y, maxWidth, lineHeight, align_center) {
    var words = text.split(' ');
    var line = '';
    var num_lines = 1;

    for (var n = 0; n < words.length; n++) {
        var testLine = line + (line == "" ? "" : " ") + words[n];
        if (canvasTextWidth(context, testLine) > maxWidth && n > 0) {
            context.fillText(line, (align_center ? x + ((maxWidth - canvasTextWidth(context, line)) / 2) : x), y);
            line = words[n];
            num_lines += 1;
            y += lineHeight;
            num_lines += 1;
        }
        else {
            line = testLine;
        }
    }

    context.fillText(line, (align_center ? x + ((maxWidth - canvasTextWidth(context, line)) / 2) : x), y);

    return num_lines;
}

// render the text inside a rectangular area aligned to center vertically and horizontally, font = "14px serif"
function rectText(context, text, font, x, y, width, height, fontHeight, margin, align_center) {
    var was_trimmed = false;

    var fitTextWidth = function (line, width, forceEllipsis) {
        var retVal = line;        
        if (canvasTextWidth(context, line) > width) {
            do {
                line = line.slice(0, -1); // removing last chars
                testWidth = canvasTextWidth(context, line + "…");
            } while (line.length > 1 && testWidth > width - margin);
            was_trimmed = true;
            retVal = line + "…";
        }
        if (forceEllipsis && retVal.length > 0 && retVal.charAt(retVal.length - 1) != "…") {
            was_trimmed = true;
            retVal += "…";
        }
        return retVal;
    };

    var words = text.split(' ');
    var line = '';
    var lineHeight = fontHeight; // top margin = 2 and bottom margin = 2
    var margin2 = margin * 2;
    var maxWidth = width - margin2;
    var maxLines = float2int(height / lineHeight);
    maxLines = (maxLines <= 0 ? 1 : maxLines);

    if ((typeof align_center) == 'undefined') align_center = true;
    
    var printLines = [];
    context.font = font;
    context.textBaseline = "top";
    context.textAline = "left";

    var words_len = words.length;
    for (var n = 0; n < words_len; n++) {
        var testLine = line + (line == "" ? "" : " ") + words[n];
        var testWidth = context.measureText(testLine).width;
        if (testWidth > maxWidth) {
            if (n == 0) { // long first word                        
                line = fitTextWidth(testLine, maxWidth, true);
                n = words_len; // exit for
            } else {
                var lineWidth = context.measureText(line).width;
                if (lineWidth < maxWidth) {
                    if (printLines.length < maxLines) {
                        printLines.push({ line: line, lineWidth: lineWidth });
                        line = words[n];
                    } else {
                        var l = printLines[printLines.length - 1];
                        //if ((l)) {
                            l.line = fitTextWidth(l.line, maxWidth, true);
                            l.lineWidth = context.measureText(l.line).width;
                        //}
                        line = "";
                        n = words_len; // exit for                
                    }
                } else {

                }
            }
        } else {
            line = testLine;
        }
    }

    if (line != "") {
        line = fitTextWidth(line, maxWidth, false);
        printLines.push({ line: line, lineWidth: context.measureText(line).width });
    }
    
    if (printLines.length > maxLines) {
        printLines.splice(maxLines, printLines.length - maxLines);
        var l = printLines[printLines.length-1]
        l.line = fitTextWidth(l.line, maxWidth, true);
        l.lineWidth = context.measureText(l.line).width;
    }

    y += margin + 2;
    y += ((height - margin2) - lineHeight * printLines.length) / 2;
    for (var i = 0; i < printLines.length; i++) {
        context.fillText(printLines[i].line, (align_center ? x - 1 + ((width - printLines[i].lineWidth) / 2) : x), y);
        y += lineHeight;
    }

    //return printLines.length > 0 && printLines[printLines.length - 1].line.charAt(printLines[printLines.length - 1].line.length - 1) == "…";
    return was_trimmed;
}

//get height of the text line for context.fillText()
function getTextHeight(font) {
    var el = document.createElement("span");
    el.appendChild(document.createTextNode("Height"));
    document.body.appendChild(el);
    el.style.cssText = "font: " + font + "; white-space: nowrap; display: inline;";
    var height = el.offsetHeight;
    document.body.removeChild(el);
    return height <= 0 ? 14 : height;
}
jQuery.expr[":"].Contains = jQuery.expr.createPseudo(function(arg) {
    return function( elem ) {
        return jQuery(elem).text().toUpperCase().indexOf(arg.toUpperCase()) >= 0;
    };
});

function createSimpleGradient(ctx, COLOR_0, COLOR_1, x0, y0, x1, y1) {
    var gradientBrush = ctx.createLinearGradient(x0, y0, x1, y1);
    gradientBrush.addColorStop(0, COLOR_0);
    gradientBrush.addColorStop(1, COLOR_1);
    return gradientBrush
}

// use as onclick = focusOnClick(this); or onmousedown event;
function focusOnClick(obj) {
    if ((obj)) {
        if (!$(obj).is(":focus")) obj.select();
    }
    return true;
}

function resetPageSelection() {
    if (window.getSelection) {
        if (window.getSelection().empty) {  // Chrome
            window.getSelection().empty();
        } else if (window.getSelection().removeAllRanges) {  // Firefox
            window.getSelection().removeAllRanges();
        }
    } else if (document.selection) {  // IE
        document.selection.empty();
    }
}

function htmlGraphBar(dValue, MaxValue, sBeforeText, sAfterText, ImagesPath, BWidth, BHeight, sColor) {
    var tMargin = 0;
    var FillWidth = -1;
    if (MaxValue == 0) { dValue = 0 } else { dValue = dValue / (MaxValue + 0.00000001) };
    if ((dValue >= 0) && (dValue <= 100)) FillWidth = Math.round((BWidth - tMargin) * dValue) - 1;
    if (FillWidth < 0) FillWidth = 0;
    if (FillWidth > BWidth - tMargin) FillWidth = BWidth - tMargin;
    var sLH = Math.floor(100 * BHeight / 9) + "";
    var sBG = (FillWidth > 0) && (FillWidth <= BWidth - tMargin) ? " background: url("+ImagesPath+"prg_bg_white.gif) repeat-y " + FillWidth + "px" : "";
    if (FillWidth > 0) sBG += (sBG = "" ? " background:" : "") + " " + sColor;
    var sBar = "<div style='display:inline-block; font-size:6px; line-height:" + sLH + "%; width:100%; height:100%;" + sBG + "'></div>";
    return sBeforeText + "<div class='progress' style='display:inline-block;height:" + BHeight + "px;width:" + BWidth + "px;padding:1px;'>" + sBar + "</div>" + sAfterText;
}

function htmlGraphBarWithValue(dValue, MaxValue, sValueText, ImagesPath, BWidth, BHeight, sColor, sGradient, sClickHandler, isCrossHatched) {
    var tMargin = 0;
    var sFill = "";
    var FillWidth = -1;
    if (MaxValue < 0) return "&nbsp;";
    if (MaxValue == 0) { dValue = 0 } else { dValue = dValue / (MaxValue + 0.00000001) };
    if (Math.abs(dValue <= 100)) FillWidth = Math.round((BWidth - tMargin) * Math.abs(dValue)) - 1;
    if (FillWidth < 0) FillWidth = 0;
    if (FillWidth > BWidth - tMargin) FillWidth = BWidth - tMargin;
    var sLH = Math.floor(8 * BHeight) + "";
    var sBG = (FillWidth > 0) && (FillWidth <= BWidth - tMargin) ? " background: url("+ImagesPath+"prg_bg_white.gif) repeat-y " + FillWidth + "px" : "";
    if (FillWidth > 0) sBG += (sBG = "" ? " background:" : "") + " " + sColor;
    var sTextCol = "#000000";
    if (FillWidth > 0 && Math.abs(dValue) >= 0.65) sTextCol = contrastColor(sColor); // use contrast color if bar width > 65%
    //if ((typeof sGradient) != 'undefined') sBG = " background: " + sGradient + " no-repeat; background-size:" + FillWidth + "px " + BHeight + "px; ";
    var sBar = "<div class='bar_value' style='display:block; font-size:10px; line-height:" + sLH + "%; width:100%; height:100%;" + sBG + "' title='" + sValueText + "'><font style='color:" + sTextCol + "; vertical-align: middle; display: inline-block; width: 100%; height: 100%; " + (typeof isCrossHatched !== "undefined" && isCrossHatched ? "background:repeating-linear-gradient(45deg,transparent,white 1px,white 1px,transparent 3px)" : "") + "'>" + sValueText + "</font></div>";
    return "<div class='progress' style='display:block;height:" + BHeight + "px;width:" + BWidth + "px;padding:1px;" + ((typeof sClickHandler) != 'undefined' ? "cursor: pointer;" : "") + "' " + ((typeof sClickHandler) != 'undefined' ? "onclick='" + sClickHandler + "' oncontextmenu='" + sClickHandler  + "'" : "") + ">" + sBar + "</div>";
}

// Context menu
is_context_menu_open = false;

function canCloseMenu() {
    is_context_menu_open = true;
}

// Use %%pages%% for pages list template in "tpl", use "%%pg%% as template for page number in "onclick"
function getPagesList(total, current, tpl, onclick) {
    var lst = "";
    var max_pages = 15;
    if (total > 1) {
        for (var i = 1; i <= total; i++) {
            var n = i;
            if (total > max_pages) {
                n = "<option value='" + i + "'" + (i == current ? " selected" : "") + ">" + n + "</option>";
                lst += n;
            } else {
                if (i == current) {
                    n = "<span style='color:#000000'><b>" + n + "</b></span>";
                } else {
                    n = "<a href='' class='actions' onclick='" + replaceString("%%pg%%", i, onclick) + "return false;'>" + n + "</a>";
                }
                lst += (lst == "" ? "" : " &middot; ") + n;
            }
        }
        if (total > max_pages) {
            lst = "<select id='pgSelect' onchange='" + replaceString("%%pg%%", "this.value", onclick) + "'>" + lst + "</select>";
        }
        if (lst != "")  lst = (tpl=="" ? lst : replaceString("%%pages%%", lst, tpl));
    }
    return lst;
}

//$('#thetext,#thearea').insertAtCaret("Текст для вставки");
jQuery.fn.extend({
    insertAtCaret: function(myValue){
        return this.each(function(i) {
            if (document.selection) {
                // Для браузеров типа Internet Explorer
                this.focus();
                var sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            }
            else if (this.selectionStart || this.selectionStart == '0') {
                // Для браузеров типа Firefox и других Webkit-ов
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos)+myValue+this.value.substring(endPos,this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        })
    }
});


function _ajaxSend(params, url) {
    _ajax_active = true;
    if (jQuery.fn.jquery >= "3") {//A1327
        _ajax_xhr = new window.XMLHttpRequest();
    }
    var _ajax_request = $.ajax({
        type: "POST",
        data: "ajax=yes&" + params,
        dataType: "text",
        async: true,
        cache: false,
        url: (url=="undefined" ? "" : url),
        success: function (data) { if (typeof _ajaxOnEvent != "undefined") _ajaxOnEvent(data, _ajax_ok_code); },
        error: function (jqXHR, textStatus) {
            if (typeof _ajaxOnEvent != "undefined") _ajaxOnEvent([jqXHR, textStatus], _ajax_error_code); if (_ajax_show_error) DevExpress.ui.dialog.alert(_ajax_error_msg, "Error");
        },
        timeout: _ajax_timeout,
        statusCode: {
            404: function() {
                _ajaxShowMsg(_ajax_error_404);
            },
            401: function () {
                _ajaxShowMsg(_ajax_error_403);
            },
            403: function () {
                _ajaxShowMsg(_ajax_error_403);
            },
            408: function () {
                _ajaxShowMsg(_ajax_error_408);
            },
            500: function () {
                _ajaxShowMsg(_ajax_error_500);
            },
            503: function() {
                _ajaxShowMsg(_ajax_error_503);
            }
        }
    });
    if (jQuery.fn.jquery < "3") {//A1327
        _ajax_xhr = _ajax_request;
    }
}

function _ajaxCancel() { //A1327
    if (_ajax_xhr) {
        _ajax_show_error = false;
        _ajax_xhr.abort();
        _ajax_show_error = true;
    }
}

function _ajaxShowMsg(msg) {
    var result = DevExpress.ui.dialog.alert(msg + "<br><br>" + _ajax_reload_msg, "Alert");
    result.done(function (dialogResult) {
        _ajaxPageReload();
    });
}

function _ajaxPageReload() {
    if ((window.opener) && (typeof window.opener.reloadPage) != "undefined") { window.opener.reloadPage(); return true; }
    if ((window.parent) && (typeof window.parent.reloadPage) != "undefined") { window.parent.reloadPage(); return true; }
    document.location.reload(true);
}

function _ajaxOnEvent(data, cmd) {
    _ajax_active = false;
    if ((typeof cmd == 'string') && (cmd != "")) eval(cmd);
    if  (typeof cmd == 'function') cmd(data);
    //-A1228 _ajax_ok_code = "";
    //-A1228 _ajax_error_code = "";
}


// jQuery disable autocomplete for inputs
function disableAutocomplete(elements) {
    $(elements).prop({ "autocomplete": "off", "autocorrect": "off", "autocapitalize": "off", "spellcheck": false });
}

// javascript helper functions
function insertAfter(newNode, referenceNode) {
    referenceNode.parentNode.insertBefore(newNode, referenceNode.nextSibling);
}

// clear combobox (<select>)
function clearCombobox(cb) {
    while (cb.firstChild) { cb.removeChild(cb.firstChild); }
}

function guid_plain(guid) {
    return replaceString("{", "", replaceString("}", "", replaceString("-", "", guid))).toLowerCase();
}

function guid_equal(a, b) {
    return (guid_plain(a) == guid_plain(b));
}

function object_equal(a, b) {
    return JSON.stringify(a) === JSON.stringify(b)
}

function $get(el_name) {
    return document.getElementById(el_name);
}

function dxConfirm(msg, yes_code, no_code, title) {
    var result = DevExpress.ui.dialog.confirm(msg, (title) ? title : resString("titleConfirmation"));
    result.done(function (dialogResult) {
        if (dialogResult && (yes_code)) {
            if ((typeof yes_code == 'string') && (yes_code != "")) eval(yes_code);
            if (typeof yes_code == 'function') yes_code();
        }
        if (!dialogResult && (no_code)) {
            if ((typeof no_code == 'string') && (no_code != "")) eval(no_code);
            if (typeof no_code == 'function') no_code();
        }
    });
}

var dxDialog_opened = false;

function dxDialog(msg, yes_code, no_code, title, btn_ok_text, btn_no_text) {
    var $popupContainer = $("<div>").appendTo($("body"));

    var myDialog = $popupContainer.dxPopup({
        animation: {
            show: { "duration": 0 },
            hide: { "duration": 0 }
        },
        contentTemplate: function () {
            return $("<div>" + msg + "</div><div style='text-align: center;'><div id='btnOKDlg' style='margin-top:1em; min-width: 100px;'></div><div id='btnCancelDlg' style='margin-top:1em; margin-left: 4px; min-width: 100px;'></div></div>");
        },
        width: "auto",
        height: "auto",
        showTitle: true,
        title: (title && title !== "undefined" && typeof title !== "undefined") ? title : "Information",
        dragEnabled: true,
        shading: false,
        closeOnOutsideClick: true,
        resizeEnabled: false,
        showCloseButton: true,
        onHiding: function (e) {
            dxDialog_opened = false;
        },
        onHidden: function (e) {
            $popupContainer.dxPopup("instance").dispose();
            $popupContainer.empty().remove();
            dxDialog_opened = false;
        },
        visible: true
    });

    dxDialog_opened = true;

    $("#btnOKDlg").dxButton(
        {
            elementAttr: { id: "jDialog_btnOK", class: "button_enter" },
            text: (btn_ok_text && btn_ok_text != "undefined" && typeof btn_ok_text != "undefined") ? btn_ok_text : resString("btnOK"),
            type: "normal",
            onClick: function () {
                var dialogResult = true;
                if (dialogResult && (yes_code)) {
                    if ((typeof yes_code == 'string') && (yes_code != "")) eval(yes_code);
                    if (typeof yes_code == 'function') yes_code();
                }
                $popupContainer.empty().remove();
                dxDialog_opened = false;
            }
        });
    $("#btnCancelDlg").dxButton(
        {
            elementAttr: { id: "jDialog_btnCancel", class: "button_esc" },
            text: (btn_no_text) ? btn_no_text : resString("btnCancel"),
            type: "normal",
            onClick: function () {
                var dialogResult = false;
                if (!dialogResult && (no_code)) {
                    if ((typeof no_code == 'string') && (no_code != "")) eval(no_code);
                    if (typeof no_code == 'function') no_code();
                }
                $popupContainer.dxPopup("instance").dispose();
                $popupContainer.empty().remove();
                dxDialog_opened = false;
            },
            visible: (no_code) && no_code != "undefined" && typeof no_code != "undefined"
        });
    return myDialog;
}

// btnOK can be boolean (true for 'jDialog_btn', false for 'jDialog_btnCancel') and can be string (need to specify the full btn name in that case)
function dxDialogBtnDisable(btnOK, disable) {
    var btn = $("#" + (typeof btnOK == "string" ? btnOK : "jDialog_btn" + ((btnOK) ? "OK" : "Cancel")));
    try {
        if ((btn) && (btn.length)) btn.dxButton("instance").option("disabled", disable);
    }
    catch (e) {
        btn.button((disable) ? "disable" : "enable");
    }
}

function dxDialogBtnFocus(btnOK, disable) {
    var btn = $("#" + (typeof btnOK == "string" ? btnOK : "jDialog_btn" + ((btnOK) ? "OK" : "Cancel")));
    if ((btn) && (btn.length)) btn.dxButton("instance").focus();
}

function applyStyleDxButtonEC() {
    $(".dx-button-mode-contained.dx-button-default").addClass("dx-ec-button-default");
    $(".dx-button-mode-contained.dx-button-default.dx-state-hover").addClass("dx-ec-button-default-hover");
    $(".dx-button-mode-contained.dx-button-default.dx-state-focused").addClass("dx-ec-button-default-focused");
    $(".dx-button-mode-contained.dx-button-default.dx-state-active").addClass("dx-ec-button-default-active");
}

function dxTextBoxFocus(tb_name) {
    setTimeout(function () {
        var tb = $("#" + tb_name).dxTextBox('instance');
        tb.focus();
        tb.element().find("input").select();
    }, 100);
}

function dxTextAreaFocus(tb_name) {
    setTimeout(function () {
        var tb = $("#" + tb_name).dxTextArea('instance');
        tb.focus();
        tb.element().find("input").select();
    }, 100);
}


function createECSwitch(id, title, checked, onclick, left_aligned) {
    var cb = $("<div id='div" + id + "'><label class='ec_switch small_element ec_switch_" + (typeof left_aligned == "undefined" || (left_aligned) ? "left" : "right") + "'><input id=" + id + " type='checkbox'" + (typeof onclick == "string" ? " onclick='" + onclick + "'" : "") + (checked ? " checked='checked'" : "") + "><span class='ec_slider small'></span></label>" + title + "<div style='clear:both'></div>");
    if (typeof onclick == "function") cb.find("input").change(onclick);
    return cb;
}

function ResetRadToolbar() {
    $(".rtbMiddle, .rtbOuter").css({ "background": "transparent", "border": "0" });
}

// use with "dx-treelist-withlines" class applied to the treelist DOM element
function getDxTreeListNodeConnectingLinesOnCellPrepared(e) {
    if (e.rowType === "data" && e.columnIndex === 0) {
        var currentNode = e.row.node,
            origItem = e.row.node,
            parent = currentNode.parent,            
            $emptySpaceElements = e.cellElement.find(".dx-treelist-empty-space"),
            i = 0;

        while ((parent) && ((typeof parent.level !== "undefined" && parent.level >= 0) || currentNode.hasChildren)) {
            var $emptyEl = $emptySpaceElements.eq($emptySpaceElements.length - 1 - i);
            var children = currentNode.parent.children;
            isLastChild = children[children.length - 1].key === currentNode.key;
            if (isLastChild) {
                if (currentNode.key == origItem.key) {
                    $emptyEl.addClass("dx-line")
                            .addClass("dx-line-middle")
                            .addClass("dx-line-last"); // "L"
                } else {
                    $emptyEl.addClass("dx-line-middle"); // "-"
                }
            } else {
                if (currentNode.key == origItem.key) {
                    $emptyEl.addClass("dx-line")
                            .addClass("dx-line-middle"); // "+"
                } else {
                    $emptyEl.addClass("dx-line"); // "|"
                }
            }
            currentNode = parent;
            parent = parent.parent;
            i += 1;
        }
    }
}

function loadSVGLib() {
    var lib = document.createElement("script");
    is_svg3 = !(is_ie);
    lib.src = _url_scripts + (is_svg3 ? "svg312.min.js" : "svg.min.js");
    document.head.appendChild(lib);
}

// === temp ====
function ec2020() {
    DevExpress.ui.dialog.alert("<div style='max-width:500px'><h5>Expert Choice Comparion 2020 Coming Soon!</h5><p>On October 22nd, Expert Choice will release Comparion 2020 (v.6.0), a major release that includes expanded browser support, updated interface, and other enhancements. You will automatically be directed to the new release when you log into Comparion.</p>On the same day, October 22nd at 12 pm EDT, Expert Choice is hosting a live demonstration of our new Comparion 2020 release. Want a first hand tour? <a href='https://pages.expertchoice.com/comparion-2020' class='dashed' target=_blank>Register here</a> to attend our live launch demonstration webinar.</p>If you need a little time to adjust to the new interface, we will provide a version 5.7 site. This site will be available through December 31st and the model data will be the same as on the Comparion 2020 site and will be kept in sync.</p></div>", "See the New Edition");
    document.cookie = "EC2020=1;path=/;expires=Mon, 31-Dec-2029 23:59:59 GMT;";
}
