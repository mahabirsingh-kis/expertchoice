/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

Twin_Slider 1.48.190722

Copyright (C) AD // Expert Choice Inc., 2012-19

/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        var slider_gpw_real_max = 99;
        var slider_gpw_real_over_max = 999;
        var slider_gpw_max = 200;
        var slider_real_width = 200;
        var slider_gpw_undef = slider_gpw_max / 2;
        var slider_gpw_twin = 1000000000;
        var slider_is_pressed = 0;
        var slider_is_manual = 0;
        var slider_prefix = 'gpw_slider';
        var slider_drag_end = false;

        //var slider_right_color = "#f0a0a0";
        //var slider_left_color = "#99a0f0";

        //var slider_right_color = "#a5d490";
        //var slider_left_color = "#549ddd";

        var slider_right_color = "#6aa84f";
        var slider_left_color = "#0058a3";

        slider_on_change_code = "var el = sliderFromEvent(e);  if ((el)) TwinSliderCheckAutoadvance(el.num);";

        //slider_on_move_code = "TwinSliderOnMove(e, el, pos);"
        slider_on_hover_code = "TwinSliderOnMove(e, el, pos);"

        var slider_on_change = null;

        var img_swap = new Image();

        function TwinSliderFormatValue(value) {
            var s = "";
            if ((value || value == 0) && (value != "")) {
                value = value * 1;
                var k = 2;  //3
                if (value >= 10) k = 1; //2
                if (value >= 100) k = 1;
                if (value >= 1000) k = 0;
                var s = value.toFixed(k);
            }
            return s;
        }

        function TwinSliderValue2Pixels(val, adv) {
            var pix = value_undef;
            if (val >= 1) {
                if (val > (slider_gpw_real_max <99 ? 99 :  slider_gpw_real_max)) {
                    pix = slider_gpw_max;
                }
                else {
                    pix = slider_gpw_max / (val + 1) * val;
                }
                if (adv > 0) { pix = slider_gpw_max - pix; }
                if (adv == 0) { pix = slider_gpw_max / 2; }
            }
            return Math.round(pix);
        }

        function TwinSliderPixels2Value(pix) {
            var v = value_undef;
            if (pix >= 0 && pix <= slider_gpw_max) {
                if (pix == 0) pix = 1;
                var a = pix / slider_gpw_max;
                var b = (1 - a);
                v = (a < b ? b / a : a / b);
                if (v <= 1.05) v = 1;
                if (v > slider_gpw_real_max) v = slider_gpw_real_over_max+0.0000001;
                if (a > 0.5) v = -v;
            }
            return v;
        }

        function TwinSliderIsChanged(slider_id, val, adv) {
            var changed = false;
            var v = eval("theForm." + slider_prefix + slider_id + "_val");
            if ((v) && v.value != val) { v.value = val; changed = true; }
            var a = eval("theForm." + slider_prefix + slider_id + "_adv");
            if ((a) && a.value != adv) { a.value = adv; changed = true; }
            //            if ((changed) &&  slider_on_change != null && slider_on_change != "") eval(slider_on_change);
            if (slider_id >= slider_gpw_twin) slider_id -= slider_gpw_twin;
            if (slider_on_change != null && slider_on_change != "") eval(slider_on_change);
        }

        function TwinSliderReset(slider_id) {
            TwinSliderSetByValue(slider_id, value_undef, 0);
        }

        function TwinSliderSwap(slider_id) {
            var a = TwinSliderGetEditA(slider_id);
            var b = TwinSliderGetEditB(slider_id);
            if ((a) && (b)) {
                var a_ = a.value;
                var b_ = b.value;
                slider_is_manual = 1;
                a.value = b_;
                b.value = a_;
                slider_is_manual = 0;
                TwinSliderOnChangeEdit(slider_id, 0);
            }
            return false;
        }
    
        function TwinSliderUpdateEditsByValue(slider_id, val_a, val_b) {
            var f_a = TwinSliderGetEditA(slider_id);
            var f_b = TwinSliderGetEditB(slider_id);
            if ((f_a) && (f_b)) {
                f_a.value = TwinSliderFormatValue(val_a);
                f_b.value = TwinSliderFormatValue(val_b);
                if ((f_a.style) && (f_a.style.color != "")) f_a.style.color = "";
                if ((f_b.style) && (f_b.style.color != "")) f_b.style.color = "";
            }
        }

        function TwinSliderUpdateEdits(slider_id, val, adv) {
            slider_drag_end = false;
            TwinSliderUpdateEditsByValue(slider_id, (val <0 ? "" : (adv > 0 ? val : "1")), (val <0 ? "" : (adv < 0 ? val : "1")));
        }

        function TwinSliderSetByValue(slider_id, val, adv) {
            var slider = document.getElementById(slider_prefix + slider_id);
            if ((slider)) {
//                if (slider_gpw_real_max < 99 && val > slider_gpw_max) val = slider_gpw_max;
                if (!slider_is_manual) TwinSliderUpdateEdits(slider_id, val, adv);
                slider.val = TwinSliderValue2Pixels(val, adv);
                slider_is_manual = 1;
                drawSliderByVal(slider);
                TwinSliderOnDrag(slider.val, slider_id);
                slider_is_manual = 0;
                TwinSliderIsChanged(slider_id, val, adv);
            }
        }

        function TwinSliderGetValue(slider_id) {
            if ((slider_id >= slider_gpw_twin)) slider_id -= slider_gpw_twin;
            var v = eval("theForm." + slider_prefix + slider_id + "_val");
            var a = eval("theForm." + slider_prefix + slider_id + "_adv");
            if ((v) && (a)) return { value: v.value, adv: a.value }
            return null;
        }

        function TwinSliderPair2Value(value, adv) {
            return (value < 0 ? value_undef : -adv * (value - 1));
        }

        function TwinSliderValue2Pair(val) {
            return (val<=value_undef ? { value: -1, adv: 0 } : { value: (Math.abs(val)+1) , adv: (val<0 ? 1 : -1)});
        }

        function TwinSliderCheckEditValue(value) {
            var is_number = false;
            var n = value.replace(",", ".");
            if (n[0] == ".") n = "0" + n;
            if (n == "0.") n = "0";
            if ((n * 1) == n) {
                n = n * 1;
                is_number = (n>0);
            }
            return { valid: is_number, value: n };
        }

        function TwinSliderCheckEditField(fld) {
            if ((fld)) {
                var val = TwinSliderCheckEditValue(fld.value);
                var s = (fld.value=="" || (val.valid) ? "" : "#cc0000");
                if ((fld.style) && (fld.style.color != s)) fld.style.color = s;
                return val;
            }
            return { valid: false, value: "" };
        }

        function TwinSliderGetEditA(slider_id) {
            return eval("theForm." + slider_prefix + slider_id + "_a");
        }

        function TwinSliderGetEditB(slider_id) {
            return eval("theForm." + slider_prefix + slider_id + "_b");
        }

        function TwinSliderOnChangeEdit(slider_id, focused) {
            var f_a = TwinSliderGetEditA(slider_id);
            var f_b = TwinSliderGetEditB(slider_id);
            if ((f_a) && (f_b)) {
                var val_a = TwinSliderCheckEditField(f_a);
                var val_b = TwinSliderCheckEditField(f_b);
                if (f_a.value == "" && val_b.valid && focused==2) { f_a.value = TwinSliderFormatValue(1); val_a = TwinSliderCheckEditField(f_a); }
                if (f_b.value == "" && val_a.valid && focused==1) { f_b.value = TwinSliderFormatValue(1); val_b = TwinSliderCheckEditField(f_b); }
                var val = value_undef;
                var adv = 0;
                if (val_a.valid && val_b.valid && f_a.value != "" && f_b.value != "") {
                    var a = val_a.value;
                    var b = val_b.value;
                    if (a > b) { val = a / b; adv = 1; } else {val = b / a; adv = -1; }
                }
                if (!slider_is_manual) {
                    slider_is_manual = 1;
                    TwinSliderSetByValue(slider_id, val, adv);
                    slider_is_manual = 0;
                }
            }
        }

        function TwinSliderSetArrow(img, diff, hover) {
            if ((img)) img.src = (hover ? (diff < 0 ? img_decrease_ : img_increase_) : (diff < 0 ? img_decrease : img_increase)).src;
        }

        function TwinSliderArrowClick(slider_id, dir, first) {
            var fld = eval(dir >= 0 ? TwinSliderGetEditB(slider_id) : TwinSliderGetEditA(slider_id));
            var fld_other = eval(dir >= 0 ? TwinSliderGetEditA(slider_id) : TwinSliderGetEditB(slider_id));
            if ((fld)) {
                var diff = 1;
                if (fld.value == "" && fld_other.value == "") { fld.value = "1"; fld_other.value = "1"; diff = 0; }
                if (fld.value != "" && fld_other.value != "" && Math.round(10 * fld.value) < Math.round(10 * fld_other.value)) { fld = fld_other; diff = -diff; }
                var val = 1 * fld.value;
                if (val >= slider_gpw_real_max && diff > 0) {
                    slider_is_pressed = 0;
                }
                else {
                    if (val == slider_gpw_real_max && diff < 0) val = slider_gpw_real_max;
                    if (val >= 1 && val < 2) diff = diff / 100;
                    if (val >= 2 && val < 5) diff = diff / 20;
                    if (val >= 5 && val < 10) diff = diff / 10;
                    if (val >= 20) diff = 10 * diff;
                    var c = (val < 10 ? 100 : 1);
                    fld.value = (fld.value == "" ? 1 : ((Math.round(c * val) + c * diff) / c).toFixed((val + diff) < 10 ? 2 : 1));
                    if (1 * fld.value > slider_gpw_real_max) { fld.value = slider_gpw_real_max; slider_is_pressed = 0; }
                    if (1 * fld.value == 1) fld.value = "1";

                    var x = value_undef;
                    var adv = 0;
                    var f_a = TwinSliderGetEditA(slider_id);
                    var f_b = TwinSliderGetEditB(slider_id);
                    if ((f_a) && (f_b)) {
                        var a = f_a.value * 1;
                        var b = f_b.value * 1;
                        if (a != 0 && b != 0) {
                            if (a < b) { x = b / a; adv = -1; } else { x = a / b; adv = 1; }
                            if (a == b) { x = 1; adv = 0; }
                        }
                        TwinSliderSetByValue(slider_id, x, adv);
                    }
                }
            }
            if (slider_is_pressed) setTimeout("if (slider_is_pressed) TwinSliderArrowClick(" + slider_id + "," + dir + ",0);", (first ? 350 : 150));
            return false;
        }

        function TwinSliderOnDrag(pixels, slider_id) {
            var id2 = (slider_id >= slider_gpw_twin ? slider_id - slider_gpw_twin : slider_gpw_twin + slider_id);
            var slider = document.getElementById(slider_prefix + id2);
            if ((slider)) { if (!(slider_is_manual) && (slider.readonly)) return false; slider.val = pixels; drawSliderByVal(slider); }
            var slider_id = (id2 < slider_id ? id2 : slider_id);

            var s = document.getElementById(slider_prefix + slider_id);
            var e_img = document.getElementById(slider_prefix + slider_id + "_erase");
            if ((e_img) && (s) && !s.readonly) e_img.src = (s.val < s.min ? img_delete_ : img_delete).src;
            var s_img = document.getElementById(slider_prefix + slider_id + "_swap");
            if ((s_img) && (s) && !s.readonly) s_img.src = (s.val < s.min ? img_blank : img_swap.src);

            if (!slider_is_manual) {
                var is_over = 0;
                var v = TwinSliderPixels2Value(pixels);
                if (Math.abs(v) > slider_gpw_real_over_max && slider_gpw_real_over_max<=99)
                {
                    is_over = 1;
                    if (v<0) v = -slider_gpw_real_max; else v = slider_gpw_real_max;
                }
                var val = Math.abs(v);
                if (val >= Math.abs(value_undef+1)) val = value_undef;  // hard fix for most left position (pix=0);
                var adv = (Math.abs(v) <= 1 ? 0 : (v < 0 ? -1 : 1));
                TwinSliderUpdateEdits(slider_id, val, adv);
                TwinSliderIsChanged(slider_id, val, adv);
                if (is_over) {
                    is_over = 1;
                    slider_is_manual = 1;
                    TwinSliderSetByValue(slider_id, val, adv)
                    slider_is_manual = 0;
                }
            }
        }

        function TwinSliderFlashEdits(slider_id, highlight) {
            if (slider_id >= slider_gpw_twin) slider_id -= slider_gpw_twin;
            var a = TwinSliderGetEditA(slider_id);
            var b = TwinSliderGetEditB(slider_id);
            if ((a) && (b)) {
                var c = (highlight ? "#fde910" : "")
                a.style.backgroundColor = c;
                b.style.backgroundColor = c;
            }
        }
        
        function TwinSliderCheckAutoadvance(slider_id) {
            var v = TwinSliderGetValue(slider_id);
            if ((v)) {
                slider_drag_end = true;
                setTimeout("if ((slider_drag_end)) TwinSliderIsChanged(" + slider_id + ", " + v.value + ", " + v.adv + ");", 250);
                TwinSliderFlashEdits(slider_id, true);
                setTimeout("TwinSliderFlashEdits('" + slider_id + "', false);", 500);
            }
        }

        function TwinSliderCreate(slider, slider_id, slider_id_b, slider_width, onChangeFunction, onResetFunction, sCustomRow, val, adv) {
            //var hint = " onmouseover='TwinSliderShowHint(\"" + slider_id + "\",1);' onmouseout='TwinSliderShowHint(\"" + slider_id + "\",0);'";
            //var hint = " onmouseout='TwinSliderShowHint(\"" + slider_id + "\",0);'";
            var hint = "";  // hide due to an issue 17599
            return "<!--div id='" + slider_prefix + slider_id + "_hint' class='sa_slider_hint'>&nbsp;</div-->" + 
             "<table border=0 cellspacing=1 cellpadding=0 style='border:0px; padding:0px; margin:0px;'><tr valign='top' style='height:24px;'>" +
             "<td align='right' style='padding-right:4px; border:0px; background:url(" + _img_slider_images_path + "blank.gif);'><img src='" + (!slider.readonly ? img_decrease.src : img_blank) + "' width=17 height=17 " + (!slider.readonly ? "style='cursor:pointer;' onmousedown='slider_is_pressed=1; slider_drag_end=false; " + onChangeFunction + "(" + slider_id + ",-1,1);' onmouseup='slider_is_pressed=0;' onmouseover='TwinSliderSetArrow(this,-1,1);' onmouseout='TwinSliderSetArrow(this,-1,0); slider_is_pressed=0; slider_drag_end=false;'" : "") + " alt='Decrease value'></td>" +
             "<td style='background:url(" + _img_slider_images_path + (slider_gpw_real_max == 9 ? "gpw_scale_l9" : "gpw_scale_l") + ".gif) no-repeat left bottom; padding:0px; margin:0px; border:0px;'" + hint + "><div class='sa_slider' style='width:" + slider_width + "px; height:14px;' id='" + slider_prefix + slider_id + "' " + (onResetFunction == "" ? "" : "ondblclick='" + onResetFunction + "(" + slider_id + ")'") + "></div><div class='sa_slider_dummy' style='width:" + slider_width + "px; height:14px;'></div></td>" +
             "<td style='background:url(" + _img_slider_images_path + (slider_gpw_real_max == 9 ? "gpw_scale_r9" : "gpw_scale_r") + ".gif) no-repeat right bottom; padding:0px; margin:0px; border:0px;'" + hint + "><div class='sa_slider' style='width:" + slider_width + "px; height:14px;' id='" + slider_prefix + slider_id_b + "' " + (onResetFunction == "" ? "" : "ondblclick='" + onResetFunction + "(" + slider_id + ")'") + "></div><div class='sa_slider_dummy' style='width:" + slider_width + "px; height:14px;'></div></td>" +
             "<td align='left' style='padding-left:4px; border:0px; background:url(" + _img_slider_images_path + "blank.gif);'><img src='" + (!slider.readonly ? img_increase.src : img_blank) + "' width=17 height=17 " + (!slider.readonly ? "style='cursor:pointer;' onmousedown='slider_is_pressed=1; slider_drag_end=false; " + onChangeFunction + "(" + slider_id + ",+1,1);' onmouseup='slider_is_pressed=0;' onmouseover='TwinSliderSetArrow(this,+1,1);' onmouseout='TwinSliderSetArrow(this,+1,0); slider_is_pressed=0; slider_drag_end=false;'" : "") + " alt='Increase value'></td>" +
             "</tr>" + sCustomRow + "</table><input type='hidden' name='" + slider_prefix + slider_id + "_val' value='" + val + "'><input type='hidden' name='" + slider_prefix + slider_id + "_adv' value='" + adv + "'>";
        }

        function TwinSliderShowHint(slider_id, vis) {
            var o = $get(slider_prefix + slider_id + "_hint");
            //if ((o) && (o.style)) { o.style.display = (vis ? "block" : "none"); }
            if ((o) && (o.css)) { o.css("visibility", (vis ? "visible" : "hidden")); }
        }

        function TwinSliderOnMove(e, el, pos) {
            var is_right = (el.num >= slider_gpw_twin);
            slider_id = (is_right ? el.num - slider_gpw_twin : el.num);
            var o = $get(slider_prefix + slider_id + "_hint");
            if ((o)) {
                //var v = TwinSliderPixels2Value(pos - (is_right && !_is_ie ? 1 : 0));
                var v = TwinSliderPixels2Value(pos);
                if (Math.abs(v) > slider_gpw_real_max) v = (v < 0 ? -slider_gpw_real_over_max : slider_gpw_real_over_max);
                var d = Math.abs(v)
//                d = d.toFixed(d < 5 ? 3 : (d<10 ? 2 : (d >98 ? 0 : 1)));
                d = d.toFixed(d < 10 ? 2 : (d > 98 ? 0 : 1));
                if (d <= 1.005) d = "1";
                o.innerHTML = (v < 0 ? "1:" + d : d + ":1");
//                o.style.left = (_is_ie ? e.x + el.offsetLeft - Math.round(o.clientWidth / 2) : e.clientX - Math.round(o.clientWidth / 2));
                o.style.left = slider_get_coord(e, el) + el.offsetLeft - Math.round(o.clientWidth / 2);
                o.style.display = "block";
            }
        }

        function TwinSliderCreateObject(slider_id, onTwinSliderOnDrag) {
            var slider = new Object();
            slider.num = slider_id;
            slider.min = 0;
            slider.max = slider_gpw_max;
            slider.realWidth = slider_real_width;
            slider.onchange = onTwinSliderOnDrag;
            return slider;
        }

        function TwinSliderInit(slider_id, value, adv, masked, is_readonly, handlers) {
            var pix = TwinSliderValue2Pixels(value, adv);

            var slider_left = TwinSliderCreateObject(slider_id, TwinSliderOnDrag);
            slider_left.masked = masked;
            slider_left.val = pix;
            slider_left.is_left = 1;
            slider_left.color = slider_left_color;
            slider_left.readonly = is_readonly;
            slider_left.handler = handlers;
            slider_left.initial_min = slider_gpw_max / 2;
            slider_left.initial_max = slider_gpw_max;
            slider_left.initial_value = slider_left.initial_min;
            sliders[slider_id] = slider_left;

            var slider_id2 = slider_gpw_twin + slider_id * 1;
            var slider_right = TwinSliderCreateObject(slider_id2, TwinSliderOnDrag);
            slider_right.masked = masked;
            slider_right.val = pix;
            slider_right.is_right = 1;
            slider_right.color = slider_right_color;
            slider_right.readonly = is_readonly;
            slider_right.handler = handlers;
            slider_right.orig_val = 0;
            slider_right.initial_max = slider_gpw_max / 2;
            slider_right.initial_min = 0;
            slider_right.initial_value = slider_right.initial_max;
            sliders[slider_id2] = slider_right;

            setTimeout("slider_is_manual = 1; sliderInitByName('" + slider_prefix + slider_left.num + "', sliders[" + slider_left.num + "]); slider_is_manual = 0; ", 30);
            setTimeout("slider_is_manual = 1; sliderInitByName('" + slider_prefix + slider_right.num + "', sliders[" + slider_right.num + "]); slider_is_manual = 0; ", 45);

            var sExtraRow = "<tr align=center><td colspan=4 align='center' style='padding-right:8px; border:0px; background:url(" + _img_slider_images_path + "blank.gif);'><div id='" + slider_prefix + slider_id + "_values' style='display:" + (masked != 0 ? "none" : "block") + "'><img src='" + (is_readonly ? img_blank : _img_slider_images_path + "swap.gif") + "' width=15 height=12 title='Swap' border=0 id='" + slider_prefix + slider_id + "_swap' style='margin-right:4px; cursor:pointer' " + (is_readonly ? "" : " onclick='TwinSliderSwap(" + slider_id + ");'") + "><input type='text' name='" + slider_prefix + slider_id + "_a' value='" + TwinSliderFormatValue(value < 1 ? "" : (adv > 0 ? value : "1")) + "' size=5 maxlength=12 style='text-align:right' onkeyup='TwinSliderOnChangeEdit(" + slider_id + ",1);'" + (!is_readonly ? "" : " disabled=1") + "  onousedown='focusOnClick(this);'>:<input type='text' name='" + slider_prefix + slider_id + "_b' value='" + TwinSliderFormatValue(value < 1 ? "" : (adv < 0 ? value : "1")) + "' size=5 maxlength=12 onkeyup='TwinSliderOnChangeEdit(" + slider_id + ",2);'" + (!is_readonly ? "" : " disabled=1") + " onousedown='focusOnClick(this);'><img src='" + (!is_readonly ? img_delete_.src : img_blank) + "' id='" + slider_prefix + slider_id + "_erase' width=9 height=9 alt='Erase judgment' style='margin-left:3px; cursor:pointer' " + (!is_readonly ? "onclick='TwinSliderReset(" + slider_id + "); return false;'" : "") + "></div></td></tr>";

            return TwinSliderCreate(slider_left, slider_left.num, slider_right.num, slider_real_width, "TwinSliderArrowClick", "", sExtraRow, value, adv);
        }
