/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

SA_Slider 1.73.170817

Originally from:
http://www.arantius.com/article/lightweight+javascript+slider+control

Copyright (c) 2006 Anthony Lieuallen, http://www.arantius.com/

Enhanced version by AD // Expert Choice Inc., 2012-15

/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

var slider_palette = ["#f08070", "#60e070", "#7090ff", "#ccff60", "#ffcc80",
  	                  "#aa80ff", "#f0a0aa", "#70d0b0", "#80b0d0", "#aaf0d0",
  	                  "#e0d040", "#c06060", "#4455d0", "#aa6699", "#5588aa",
  	                  "#40d0e0", "#cccccc", "#ffd0e0", "#f0ffa0", "#909090"];

var slider_handler_width = 9;
var slider_prefix = 'slider';

var slider_drag_end = false;

var slider_on_change_code = "";

var slider_on_move_code = "";
var slider_on_hover_code = "";

var _img_slider_images_path = "images/";
var _img_slider_position = new Image(); //_img_slider_position.src = _img_slider_images_path + "slider_position.gif";
var _img_slider_handler = new Image();  //_img_slider_handler.src = _img_slider_images_path + "slider_handler.gif";
var _img_slider_handler2 = new Image(); //_img_slider_handler2.src = _img_slider_images_path + "slider_handler2.gif";
var _img_slider_blank = new Image();    //_img_slider_blank.src = _img_slider_images_path + "blank.gif";

var activeSlider = -1;
var el_activeSlider = null;

var _is_ie = ((document.attachEvent));

function getSliderHandlerWidth(slider) {
    return (slider.handler ? slider_handler_width : 0);
}

function sliderGetWidth(slider) {
    return ((slider) && (slider.scrollWidth) ? slider.scrollWidth : slider.realWidth);
}

function getBarWidth(slider, value) {
    var x;
    if (value <= 0) {
        x = 0;
    }
    else {
        var p = (value - slider.min) / (slider.max - slider.min);
        var x = (sliderGetWidth(slider) - getSliderHandlerWidth(slider) / 2 - (slider.is_left || slider.is_right ? 1 : 1)) * p;
    }
    return Math.round(x);
}

function drawSliderByVal(slider) {
    if ((slider)) {
        if (slider.val * 1 != slider.val) slider.val = slider.min - 1;
        var x = getBarWidth(slider, slider.val);
        if ((slider.children)) {
            if (slider.children.length >= 4) {
                slider.children[4].style.left = Math.round(x - getSliderHandlerWidth(slider) / 2) + "px";
                var st = (slider.masked != 0 || !slider.handler || (slider.val * 1) < (slider.min * 1) ? "none" : "block");
                if (slider.children[4].style.display != st) slider.children[4].style.display = st;
            }
            if (slider.children.length >= 3) {
                var middle = (slider.max - slider.min) / 2;
                var x_middle = getBarWidth(slider, middle);
                if (slider.is_gpw || (slider.is_left)) {
                    if ((slider.is_left)) { middle = slider.max; x_middle = x_middle * 2 + 1; }
                    var c = null;
                    var img = null;
                    var bp = null;
                    var l = 0;
                    var w = x_middle;
                    var v = slider.val;
                    if (slider.val < slider.min) {
                        v = middle;
                        l = x_middle;
                        w = 0;
                    }
                    else {
                        if (slider.val < middle) {
                            c = slider.color_left;
                            img = "slider_blue.gif";
                            bp = "top left";
                            l = getBarWidth(slider, v);
                            w = x_middle - l;
                            if ((slider.is_left)) l -= 1;
                        }
                        else {
                            c = slider.color_right;
                            img = "slider_red.gif";
                            bp = "top left";
                            w = getBarWidth(slider, v) - x_middle + 1;
                            l = x_middle - 1;
                        }
                    }
                    if ((slider.masked != 0)) {
                        c = "#cccccc";
                        if (Math.abs(slider.val - middle) < 6) {
                            l = x_middle - 2;
                            w = 3;
                        }
                        if (slider.val < slider.min) {
                            l = x_middle;
                            w = 0;
                        }
                        else {
                            if (slider.masked < 0) {
                                if (slider.val < middle / 2+2) {
                                    l = 0;
                                    w = x_middle;
                                } else {
                                    l = 0;
                                    w = 0;
                                }
                            }
                            else {
                                l = 0;
                                w = x_middle * 2 - 2;
                            }
                        }
                    }
                    if ((c) && slider.children[3].style.backgroundColor != c) slider.children[3].style.backgroundColor = c;
                    if ((img) && !slider.readonly && slider.masked == 0) {
                        if (slider.children[3].style.backgroundImage != "url(" + _img_slider_images_path + img + ")" || slider.children[3].style.backgroundPosition != bp) {
                            slider.children[3].style.backgroundImage = "url(" + _img_slider_images_path + img + ")";
                            slider.children[3].style.backgroundPosition = bp;
                        }
                    }
                    var x_max = getBarWidth(slider, (slider.max - slider.min));
                    if ((l + w) > x_max) w = x_max - l;
                    if (w < 0) w = 0;
                    if (l < 0) l = 0;
                    l = Math.round(l) + "px";
                    w = Math.round(w) + "px";
                    if (slider.children[3].style.left != l) slider.children[3].style.left = l;
                    if (slider.children[3].style.width != w) slider.children[3].style.width = w;
                } else {
                    if (x < 0) x = 0;
                    var l = 0;
                    var c = slider.color;
                    if ((slider.masked != 0)) {
                        c = "#cccccc";
                        x = (slider.val < slider.min ? 0 : sliderGetWidth(slider));
                        if ((slider.is_right) && slider.val<middle-2 && slider.masked<0) {
                            x = 0;
                        }
                    }

                    if ((slider.children[3]) && (slider.children[3].style)) {
                        if (c != "" && slider.children[3].style.backgroundColor != c) slider.children[3].style.backgroundColor = c;

                        if ((slider.is_right)) {
                            img = "slider_red.gif";
                            bp = "top right";
                            if ((img) && !slider.readonly && slider.masked == 0) {
                                if (slider.children[3].style.backgroundImage != "url(" + _img_slider_images_path + img + ")" || slider.children[3].style.backgroundPosition != bp) {
                                    slider.children[3].style.backgroundImage = "url(" + _img_slider_images_path + img + ")";
                                    slider.children[3].style.backgroundPosition = bp;
                                }
                            }
                        }

                        l = Math.round(l) + "px";
                        x = Math.round(x) + "px";
                        if (slider.children[3].style.left != l) slider.children[3].style.left = l;
                        if (slider.children[3].style.width != x) slider.children[3].style.width = x;
                    }
                }
            }
        }
    }
}

function setSliderByClientX(slider, clientX) {
    if ((slider)) {
//        var p = (clientX - slider.offsetLeft - Math.round(getSliderHandlerWidth(slider) / 2)) / (sliderGetWidth(slider) - getSliderHandlerWidth(slider));
        var p = clientX / (sliderGetWidth(slider) - getSliderHandlerWidth(slider));
        slider.val = (slider.max - slider.min) * p + slider.min;
        if (slider.val > slider.max) slider.val = slider.max;
        if (slider.val < slider.min) slider.val = slider.min;
        var val = Math.round(slider.val);
        if (Math.abs(slider.val - clientX) < 1.5) val = clientX;
        drawSliderByVal(slider);
        if (slider.num>=0 && (slider.onchange)) slider.onchange(val, slider.num);
    }
}

function slider_findPosX(obj) {
    var curleft = 0;
    if ((obj) && (obj.offsetParent)) {
        while (obj.offsetParent) {
            curleft += obj.offsetLeft
            obj = obj.offsetParent;
        }
    }
    else if ((obj) && (obj.x))
        curleft += obj.x;
    return curleft;
}

function slider_get_coord(e, el) {
    var posx = 0;
    if (!e) var e = window.event;
    if (e.pageX || e.pageY) { posx = e.pageX; } else if (e.clientX || e.clientY) { posx = e.clientX + document.body.scrollLeft; }
    return (posx - slider_findPosX(el)+3);  // dirty fix for shift for "pointer" finger :)
}

function sliderClick(e) {
    var el = sliderFromEvent(e);
    if (!el) return;
    var v = slider_get_coord(e, el);
    var v_ = v;
    if (v_ < el.min) v_ = el.min;
    if (v_ > el.max) v_ = el.max;
    if (el.val < el.min && el.initial_min >= 0 && el.initial_max >= 0 && el.initial_value >= 0 && v_ >= el.initial_min && v_ <= el.initial_max) {
        v = el.initial_value; 
    }
    setSliderByClientX(el, v);
}

function sliderMouseHover(e) {
    var el = ((el_activeSlider) && el_activeSlider.num == activeSlider ? el_activeSlider : sliderFromEvent(e));
    if (!el) return;
    var pos = slider_get_coord(e, el);
    if (slider_on_hover_code != "") eval(slider_on_hover_code);
}

function sliderMouseMove(e) {
    sliderMouseHover(e);
    if (activeSlider < 0) return;
    var el = ((el_activeSlider) && el_activeSlider.num == activeSlider ? el_activeSlider : sliderFromEvent(e));
    if (!el) return;

    var pos = slider_get_coord(e, el);
    if (slider_on_move_code != "") eval(slider_on_move_code);

    setSliderByClientX(el, pos);
    stopEvent(e);
}

function sliderMouseUp(e) {
    if (activeSlider != -1) {
        slider_drag_end = true;
        activeSlider = -1;
        el_activeSlider = null;
        if (slider_on_change_code != "") eval(slider_on_change_code);
        stopEvent(e);
    }
}

function sliderMouseDown(e) {
    slider_drag_end = false;
    sliderClick(e);
    var el = sliderFromEvent(e);
    if (!el) return;
    activeSlider = el.num;
    el_activeSlider = el;
    stopEvent(e);
}

function sliderFromEvent(e) {
    if (!e && window.event) e = window.event;
    if (!e) return false;

    var el;
    if (e.target) el = e.target;
    if (e.srcElement) el = e.srcElement;

    if (!el.id || !el.id.match(eval('/' + slider_prefix + '\\d+/'))) el = el.parentNode;
    if (!el) return false;
    if (!el.id || !el.id.match(eval('/' + slider_prefix + '\\d+/'))) return false;

    return el;
}

//borrowed from prototype: http://prototype.conio.net/
function stopEvent(event) {
    if (event.preventDefault) {
        event.preventDefault();
        event.stopPropagation();
    } else {
        event.returnValue = false;
        event.cancelBubble = true;
    }
}

//add event function from http://www.dynarch.com/projects/calendar/
function addAnEvent(el, evname, func) {
    if (el.attachEvent) { // IE
        el.attachEvent("on" + evname, func);
    } else if (el.addEventListener) { // Gecko / W3C
        el.addEventListener(evname, func, true);
    } else {
        el["on" + evname] = func;
    }
}

function sliderInitByName(div_slider_name, slider) {
    var div_slider = document.getElementById(div_slider_name);
    if ((div_slider)) sliderInit(div_slider, slider);
}

function sliderInit(div_slider, slider) {
    if (!(div_slider) || !(slider)) return false;

    var is_left = ((slider.is_left));
    var is_right = ((slider.is_right));

    div_slider.innerHTML = '<div class="left" style="height:14px;' + (is_right ? 'width:0px;' : '') + '"></div><div class="right" style="height:14px;' + (is_left ? 'width:0px;' : '') + '"></div><img src="' + _img_slider_position.src + '" width="5" height="14" border="0" title="" class="sa_slider_original">' +
                           '<img src="' + _img_slider_blank.src + '" width="1" height="4" border="0" title="" class="sa_slider_value">' +
                           '<img src="' + _img_slider_handler.src + '" width="' + slider_handler_width + '" height="14" border="0" title="" ' + (slider.readonly ? '' : 'onmouseover="this.src=_img_slider_handler2.src" onmouseout="this.src=_img_slider_handler.src"') + '>';

    div_slider.min = slider.min;
    div_slider.max = slider.max;
    div_slider.realWidth = ((typeof slider.realWidth != "undefined") ? slider.realWidth : 200);
    div_slider.is_left = is_left;
    div_slider.is_right = is_right;
    div_slider.val = slider.val;
    div_slider.val_old = ((typeof slider.val_old != "undefined") ? slider.val_old : slider.val);
    div_slider.handler = slider.handler;
    div_slider.readonly = slider.readonly;
    div_slider.color_left = slider.color_left;
    div_slider.color_right = slider.color_right;
    div_slider.masked = slider.masked;
    div_slider.is_gpw = !(typeof(slider.color_left) == "undefined" || typeof(slider.color_right) == "undefined");
    if ((slider.orig_val) != null) div_slider.orig_val = slider.orig_val;
    div_slider.num = slider.num;
    div_slider.initial_min = ((slider.initial_min) != null ? slider.initial_min : -1);
    div_slider.initial_max = ((slider.initial_max) != null ? slider.initial_max : -1);
    div_slider.initial_value = ((slider.initial_value) != null ? slider.initial_value : -1);

    var pos = div_slider.getElementsByTagName('img')[0];
    if ((slider.orig_val) != null) {
        if ((pos)) { var xp = Math.round(getBarWidth(div_slider, slider.orig_val) - (slider.is_right ? 3 : 2)); if (xp < -3) xp = -3; pos.style.left = xp + "px"; }
    }
    else {
        if ((pos)) pos.style.display = "none";
    }

    if (div_slider.children.length >= 3 && !div_slider.is_gpw) {
        div_slider.children[3].style.backgroundColor = slider.color;
        div_slider.children[3].style.backgroundColor
    }

    drawSliderByVal(div_slider);

    div_slider.onchange = slider.onchange;
    if ((slider.onchange)) div_slider.onchange(div_slider.val, slider.num);
    if ((slider.readonly) != 1) {
        div_slider.style.cursor = 'hand';
        addAnEvent(div_slider, 'mousedown', sliderMouseDown);
        addAnEvent(div_slider, 'mouseup', sliderMouseUp);
        addAnEvent(div_slider, 'mousemove', sliderMouseMove);
    }
    if ((slider.handler) == 0 || (slider.value < slider.min) || (slider.masked != 0)) {
        if (div_slider.children.length >= 4) { div_slider.children[4].style.display = "none"; }
    }

}

//// init
//addAnEvent(document, 'mousemove', sliderMouseMove);
//addAnEvent(document, 'mouseup', sliderMouseUp);