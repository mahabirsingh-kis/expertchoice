<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlMultiRatings" Codebehind="ctrlMultiRatings.ascx.vb" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<script type="text/javascript"><!--

    var img_path        = '<% =ImagePath %>';
    
    var img_infodoc1	= new Image; img_infodoc1.src    = img_path + '<% =ImageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src    = img_path + '<% =ImageInfodocEmpty %>';
    var img_WRTinfodoc1	= new Image; img_WRTinfodoc1.src = img_path + '<% =ImageWRTInfodoc %>';
    var img_WRTinfodoc2	= new Image; img_WRTinfodoc2.src = img_path + '<% =ImageWRTInfodocEmpty %>';

    var img_int_0	= new Image; img_int_0.src   = img_path + 'info12_dis.png';
    var img_int_1	= new Image; img_int_1.src   = img_path + 'info12.png';

    var img_note_empty  = "<% =ImageCommentEmpty %>";
    var img_note_exists = "<% =ImageCommentExists %>";

    var fill = 'fill1';
    var maxvalue = 1;
    var height = 6;
    var width = <% =BAR_WIDTH %>;
    var padding = 1;
    var max_desc_len = 65;
    var imgname = img_path + "blank.gif";
    
    var ComboBoxes      = [<%= _ComboBoxesList %>];
    var RatingLabels    = [<%= _LabelsList %>];
    var Ratings         = [<%= _RatingsList %>];
    var TooltipsInfodoc = [<% =_TooltipsInfodocList %>];
    var TooltipsWRT     = [<% =_TooltipsWRTList %>];
    var ImagesInfodoc   = [<% =_ImagesList %>];
    var ImagesWRTInfodoc= [<% =_ImagesWRTList %>];
    var ImagesComments  = [<% =_ImagesCommentsList %>];
    var InfodocURLs     = [<% =_InfodocURLsList %>];
    var WRTInfodocURLs  = [<% =_WRTInfodocURLsList %>];
    var InfodocTitles   = [<% =_InfodocTitlesList %>];
    var InfodocWRTTitles= [<% =_InfodocTitlesWRTList %>];
    var InfodocEditURLs     = [<% =_InfodocEditURLsList %>];
    var WRTInfodocEditURLs  = [<% =_WRTInfodocEditURLsList %>];
    
    var focus_active_id = -1;
    var focused_combo = 0;
    var focused_direct = 0;

    var bar2animate = [<% =Bars2Animate %>];

    var _is_manual = false;

    var show_comment = false;
    var first_load = true;
    var show_direct = <% =Bool2JS(ShowDirectValue) %>;
    var show_prty = <% =Bool2JS(ShowPrtyValues) %>;
    var hook_keypress = true;

    var prec = <% =Precision %>;

    function HasUndefined()
    {
        var fUndef = false;
        if ((theForm.RatingsCount))
            for (var i=0; i<theForm.RatingsCount.value; i++)
                if (eval("theForm.Rating" + i + ".value")<0 && (eval("theForm.RatingDirect" + i ) && eval("theForm.RatingDirect" + i + ".value")<0)) fUndef = true;
        return fUndef;
    }

    function SetBarByID(id, v, disp)
    {
        var b = $get("bar" + id);
        if ((b))
        {
            fill = 'fill1';
//            /*var p = Math.pow(10, prec);
//            if (p==0) p = 1;
//            b.innerHTML = "<nobr>" + (v<0 ? "" : GetBar(v)) + "&nbsp;<span style='margin-left:1ex; width:" + (6+prec) + "ex; text-align:right'>" + (v>=0 ? Math.round(v*100*p)/p + "%" : "&nbsp;") + "</span></nobr>"; */
//            var p = (v+"").length-2;
//            if (p<prec) p =prec;
//            p = Math.pow(10, p);
//            if (p==0) p = 1;
////            var val = replaceString(".", CurrencyDecimalSeparator, "" + Math.round(v*100*p)/p);
//            if (val.indexOf(CurrencyDecimalSeparator)<0) val += CurrencyDecimalSeparator;
//            for (var i=0; i<prec; i++) {
//                if (val.length-val.indexOf(CurrencyDecimalSeparator)<=prec) val += "0";
//            }
            
            var old = 0;
            var bo = eval('theForm.bar_old_' + id);
            if ((bo)) old = bo.value*1;
            val_ = (v*100).toFixed(8);
            if (v>0.001) {
                val_ = roundTo(v*100, prec);
            } else {
                var p_idx = val_.indexOf(".");
                if (p_idx<=0) p_idx = val_.indexOf(",");
                while (val_.length>p_idx+prec+1 && val_.substr(val_.length-1,1)=="0") val_=val_.substr(0,val_.length-1);
            }
            var sVal = (typeof disp!="undefined" ? disp : val_ + "%");
            if ((usd_show) && usd_cost>0 && (1*v>=0)) sVal = showCost(usd_cost*v);
            var sv = Math.round(old * width);
            if (sv>width) sv=width;
            b.innerHTML = (v*1<0 ? "&nbsp;" : "<table border=0 cellspacing=0 cellpadding=0 width='100%'><tr valign=middle><td style='width:" + width + "px; padding-right:4px; background:!important; border:0px;'>" +  GetBar(v, 10, sv) + "</td><td align=right class='text small' style='background:!important; border:0px;'><nobr>" + sVal + "</nobr></td></tr></table><input type='hidden' name='bar_old_" + id + "' value='" + (v*1) + "'>");
        }
    }
    
    function onShowComment(sender, eventArgs)
    {
        var c = eval("theForm.Comment" + focus_active_id);
        if ((c)) { c.focus(); show_comment = true; }
    }

    function switchUSD(chk) {
        usd_show = chk;
        if (focus_active_id>=0) LoadRatings(focus_active_id);
        for (var i=0; i<Ratings.length; i++) {
            var r = eval("theForm.Rating" + i);
            var d = eval("theForm.RatingDirect" + i);
            var dv = -1;
            if ((d)) {
                var dv = CheckComboNumber(d.value);
            }
            if (dv!=-1 && (dv.valid)) {
                SetBarByID(i, 1*dv.value);
            } else {
                if ((r) && r.value>=0) {
                    var cb = $find(ComboBoxes[i]);
                    if ((cb))
                    {
                        items = cb.get_items();
                        for (j=0; j<items.get_count(); j++) {
                            var r_ = items.getItem(j);
                            if (r_.get_value()*1==r.value*1) {
                                var v = eval(r_.get_attributes().getAttribute("data"));
                                val = v[1]*1;
                                SetBarByID(i, val);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

   function onChangedRating(sender,eventArgs)
   {        
        var item = eventArgs.get_item();
        var cid = item.get_comboBox().get_id();
       var rid = -1;
        for (var i=0; i<ComboBoxes.length; i++)
          if (ComboBoxes[i] == cid) rid = i;
        if (rid==-1) return false;
        var v_orig = eval(item.get_attributes().getAttribute("data"));
        var v = v_orig[1]*1;
        var inp = sender.get_inputDomElement();
        if ((inp)) inp.style.backgroundColor = "";
        SetBarByID(rid, v_orig[1], v_orig[2]);
        SetRating(Ratings[rid], item.get_value(), -1);
        LoadRatings(rid);
        return true;
    }

    function CheckComboNumber(val)
    {
        var is_number = false;
        var n = (val+"").replace(",", ".");
        if (n[0] == ".") n = "0" + n;
        if (n == "0.") n = "0";
        if ((n * 1) == n) 
        {
            n = n * 1;
            is_number = (n >= 0 && n <= 1);
        }
        return {valid: is_number, value: n};
    }
    
    function CheckComboEdit(sender)
    {
        var val = sender.get_text();
        if (!_is_manual && !sender.findItemByText(val))
        {
            if (val == "")
            {
                var not_rated = sender.findItemByValue(-1);
//                if (show_warnings && (not_rated)) not_rated.select();
            }
            else
            {
                var v = CheckComboNumber(val);
                var inp = sender.get_inputDomElement();
                var rid = -1;
                for (var i=0; i<ComboBoxes.length; i++) if (ComboBoxes[i] == sender.get_id()) rid = i;
                if (!v.valid)
                {
                    if ((inp)) inp.style.backgroundColor = "#ffe0e0";
                    return false;
                }
                else
                {
                    _is_manual = true;
                    if ((inp)) inp.style.backgroundColor = "";
//                    if (show_warnings)
//                    {
//                        sender.clearSelection();
//                        sender.set_text(v.value+"");
//                    }
                    if (rid!=-1)
                    {
                        SetBarByID(rid, v.value);
//                        if (show_warnings) SetRating(rid, -1, v.value);
                    }
                    if (rid == focus_active_id)
                    {
                        SetDirectValue(sender, rid);
                        theForm.DirectRating.value = val;
                    }
                    _is_manual = false;
                }
            }
        }
        return true;
    }

    function SetDirectValue(sender, id)
    {
        if ((sender))
        {
            var r = theForm.rating_value.item(sender.get_items().get_count());
            if ((r) && (!r.checked || id<0))
            {
                r.checked=true;
                if (Math.abs(id)!=focus_active_id) LoadRatings(id);
                return true;
            }
        }
       return false;
    }

    function onClientFocused(sender, eventArgs) 
    {
        focused_combo = 1;
    }
    
    function onClientBlur(sender, eventArgs) 
    {
        focused_combo = 0;
        return onClientTextChange(sender, eventArgs);
    }

    function onClientTextChange(sender, eventArgs) 
    {
        if (!CheckComboEdit(sender)) eventArgs.returnValue = false;
    }

    function onClientKeyPress(e)
    {
        if (1*focus_active_id>=0)
        {
            var sender = $find(ComboBoxes[1*focus_active_id]);
            if ((sender)) 
            {
                var val = sender.get_text();
                if (!_is_manual)
                {
                    if (val>"" && (val=="." || val==",")) { val = "0" + val; sender.set_text(val); }
                    var inp = sender.get_inputDomElement();
                    var v = CheckComboNumber(val);
                    var idx = sender.get_value();
                    var rid = -1;
                    if ((v.valid) || (idx>=-1))
                    {
                        for (var i=0; i<ComboBoxes.length; i++) if (ComboBoxes[i] == sender.get_id()) rid = i;
                        _is_manual = true;
                        if ((inp)) inp.style.backgroundColor = "";
                        if (rid!=-1)
                        {
                            var id = "";
                            if ((!v.valid))
                            {
                                var txt = sender.get_text();
                                var val = "";
                                items = sender.get_items();
                                for (i=0; i<=items.get_count(); i++)
                                {
                                    var r = items.getItem(i);
                                    if ((r) && (r.get_text() == txt))
                                    {
                                        id = r.get_value();
                                        var v = eval(r.get_attributes().getAttribute("data"));
                                        val = v[1]*1;
                                    }
                                }
                                if (id!="" && val!="")
                                {
                                    sender.set_value(id);
                                    v.value = 1*val;
                                }
                            }
                            if (id!="")
                            {
                                SetBarByID(rid, v.value);
                                SetRating(rid, id, -1);
                            }
                            else
                            {
                                SetBarByID(rid, v.value);
                                SetDirectValue(sender, rid);
                                SetRating(rid, -1, v.value);
                            }
                        }
                        _is_manual = false;
                    }
                    else
                    {
                        if ((inp)) inp.style.backgroundColor = "#ffe0e0";
                    }

                    if ((theForm.DirectRating))
                    {
                        if (rid>=0 && idx!="")
                        {
                            theForm.DirectRating.value = "";
                        }
                        else
                        {
                            theForm.DirectRating.value = val;
                            theForm.DirectRating.style.backgroundColor = inp.style.backgroundColor;
                        }
                    }
                }
            }
        }
    }

    function ShowWarning(id, is_combo)
    {
        var a = DevExpress.ui.dialog.alert('<% =JS_SafeString(msgDirectRatingValue) %>', "Warning");
        a.done( function() { 
            onShowWarning(id, is_combo); 
        } );
        return false;
    }

    function onShowWarning(id, is_combo)
    {
        SetFocus(id, 1, 1*focus_active_id!=i, 0);
        if (is_combo) SetComboFocus(); else SetDirectFocus();
        return false;
    }

    function Changed()
    {
        <% if changeFlagID<>"" Then %>if (theForm.<% =ChangeFlagID %>) theForm.<% =ChangeFlagID %>.value=1;<% end if %>
        <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>
        if (!HasUndefined()) setTimeout("FlashNextButton();", 1000);
    }
    
    function SetRating(ID, val, direct)
    {
        var r = eval("theForm.Rating" + ID);
        if ((r)) r.value = val;
        r = eval("theForm.RatingDirect" + ID);
        if ((r)) r.value = direct;
        Changed();
        return false;
    }
    
    function CheckFormOnSubmit()
    {
        if (HasUndefined() && !ConfirmMissingJudgment()) return false;
        for (var i=0; i<ComboBoxes.length; i++)
        {
            var combo = $find(ComboBoxes[i]);
            var c = eval("theForm.Rating" + i);
            if ((c) && (combo) && !(combo.get_highlightedItem()) && !(combo.findItemByText(combo.get_text())) && (combo.get_items().get_count()))
            {
                var inp = combo.get_inputDomElement();
                if ((inp))
                {
                    vc = CheckComboNumber(inp.value);
                    if (!vc.valid) { SetFocus(i, 1, 1*focus_active_id!=i, 1); ShowWarning(i, true); return false }
                }
                var d = eval("theForm.RatingDirect" + i);
                if ((d))
                {
                    vd = CheckComboNumber(d.value);
                    if (!vd.valid) { SetFocus(i, 1, 1*focus_active_id!=i, 2); ShowWarning(i, false); return false }
                }
            }
        }
        fixComboboxes();
        return true;
    }

    function fixComboboxes() {
        for (var i=0; i<ComboBoxes.length; i++)
        {
            var combo = $find(ComboBoxes[i]);
            if ((combo)) combo._disposeChildElements = false;
        }
    }
    
    function ApplyComment(id, tooltip, imgid)
    {
        var tb = eval("theForm.Comment" + id);
        if ((tb)) 
        {
            comment = tb.value;
            var img = $get(imgid);
            if ((img))
            {
                img.src = (comment=="" ? img_note_empty  : img_note_exists);
//                img.tooltip = comment; //replaceString("<", "&lt;", replaceString(">", "&gt;", comment));
                img.title = comment;
            }
            Changed();
        }
        SwitchTooltip(tooltip);
        return false;
    }
    
    function SwitchTooltip(id)
    {
        var t = $find(id);
        if ((t))
        {
            if (t.isVisible) { t.hide(); show_comment = false; } else t.show();
        }
    }
    
    function OnToolTipShowHandler(sender, args)
    {
        if ((sender) && (sender.get_text()=="")) args.set_cancel(true);
    }
    
    function InfodocFrameLoaded(frm)
    {
        if ((frm) && (frm.style)) { frm.style.backgroundImage='none'; };
    }

    function InitCombos(id, cnt)
    {
        if (ComboBoxes.length<=0) {
            $("#divRatings").hide();
            $("#imgList").hide();
            return false;
        }
        var test = $find(ComboBoxes[0]);
        if ((test))
        {
            for (var i=0; i<ComboBoxes.length; i++)
            {
                var combo = $find(ComboBoxes[i]);
                if ((combo)) { var inp = combo.get_inputDomElement(); inp.onkeyup = onClientKeyPress; }
                var c = eval("theForm.Comment" + i);
                if (ImagesComments.length>0) {
                    var im = $get(ImagesComments[i]);
                    if ((c) && (im)) im.title = (c.value == "" ? "[Edit comment]" : c.value);
                }
            }
            if (id<0) id=0;
            SetFocus(id, 1, true, 0);
            if ((typeof is_mobile!="undefined" && (is_mobile)) || (do_flash)) {
                var r = $get("divRatings");
                if ((r)) r.scrollIntoView(false);
            }
        }
        else
        {
            if ((cnt<10)) setTimeout("InitCombos(" + id + ", " + (cnt+1) + ");", 200);
        }
    }

    function SetFocus(id, focus, load_ratings, focus_el)    // focus_el : 0 - none, 1 - combo, 2 - direct
    {
       var do_scroll = (focus == 1 && (typeof is_mobile == "undefined" || !(is_mobile)));
       if (focus==4) focus=1;
       if (1*focus_active_id == id && focus!=0) return false;
       if ((focus==1) && (1*focus_active_id>=0)) { SetFocus(focus_active_id, 0, false ); }
       var r = $get("row" + id);
       if ((r))
       {
            var clr = (focus == 1 ? "#ffffaa" : (focus == 0 || focus==-1 ? (r.className.indexOf("_alt")>0 ? "#f5f5f5" : "#ffffff")  : (focus==2 ? (r.className.indexOf("_alt")>0 ? "#e5f0f5" : "#f0f5fa"): "#ffffc0")));
            for (var i=0; i<r.cells.length; i++) r.cells[i].style.background = clr;
            if (focus==1)
            {
               focus_active_id = id*1;
               if (typeof load_ratings != "undefined" && (load_ratings)) LoadRatings(id);
               if (focus_el == 1) SetComboFocus();
               if (focus_el == 2) SetDirectFocus();
               if (do_scroll && id>1)
               {
                    if (id<ComboBoxes.length-2)
                    {
                        var scr = $get("row" + (id+1));
                        if ((scr)) scr.scrollIntoView(false);
                    }  else
                    //if (ComboBoxes.length>2)
                    {
                        var scr = $get("row" + (ComboBoxes.length-1));
                        if ((scr)) scr.scrollIntoView(false);
                    }    
               }
            }
            if (focus == 0)
            {
                var rat = $get("divRatings");
                if ((rat)) rat.innerHTML = "&nbsp;";
            }
            var framed = <% =iif(ShowFramedInfodocs, "1", "0") %>;
            if (framed && (focus==1))
            {
                LoadInfodocFrame("alt", InfodocTitles[id], InfodocURLs[id], InfodocEditURLs[id]);
                LoadInfodocFrame("wrt", InfodocWRTTitles[id], WRTInfodocURLs[id], WRTInfodocEditURLs[id]);
            }
       }
       return false;
    }
    
    function LoadInfodocFrame(name, title, url, editurl)
    {
        var f = $get(name);
        if ((f))
        {
            if ((f.contentWindow) && (f.contentWindow.document.body)) f.contentWindow.document.body.innerHTML = "<div style='<% =String.Format(FrameLoadingStyle, ImagePath)%>'>&nbsp</div>";
            setTimeout("var f = $get('" + name + "'); if ((f)) f.src= \"" + url + (url=="" ? "" : "&p=" + Math.round(10000*Math.random())) + "\";" , 200);
            var t = $get("lbl_" + name + "<% =_HIDDEN %>");
            if ((t)) t.innerHTML = title;
            t = $get("lbl_" + name + "<% =_VISIBLE %>");
            if ((t)) t.innerHTML = title;
            var fnc = new Function("onclick", <% If CanEditInfodocs Then %>(editurl=="" ? (url="" ? "" : "open_infodoc('" + url + "'); return false;") : editurl)<% Else %>(url="" ? "" : "open_infodoc('" + url + "'); return false;")<%End if %>);
            var l = $get("lnk_" + name + "<% =_HIDDEN %>");
            if ((l)) l.onclick=fnc;
            l = $get("lnk_" + name + "<% =_VISIBLE %>");
            if ((l)) l.onclick =fnc;
        }
    }

    var old_rid = -1;
    var do_flash = <% =Bool2JS(FlashRatingsTitle) %>;

    function flashWord(id) {
        if (do_flash) {
            $("#" + id).addClass("blink_opacity");
            setTimeout(function () {
                $("#" + id).removeClass("blink_opacity");
            }, 5100);
            do_flash = false;
        }
    }

    function LoadRatings(id) {
        var r = $get("divRatings");
        if ((r))
        {
            //var sd = $("#divScaleDesc").has(":visible");
            //if ((sd)) sd.hide();
            var obj_act = null;
            r.innerHTML = "…";
            var p = $("#spanPath" + id);
            var path = "";
            if ((p) && (p.length) && typeof  p.attr("path")!="undefined") path = p.attr("path");
            var cb = $find(ComboBoxes[id]); 
            if ((cb))
            {
                var t = InfodocTitles[id];
                <% If RatingsTitle <> "" Then %>if (t!="") {
                    t = "<% =JS_SafeString(GetRatingsTitleForFlash) %> of<br> " + t;
                    setTimeout(function () { flashWord("divFlash"); }, 1500);
                }<% End If %>
                if (path!="") { t = "<div style='text-align:left; font-weight:normal; padding:2px 1ex; color:#006688; height:<% =12*GetPathMaxHeight+5 %>px;' class='text'>" + path + "</div><h6 style='margin-top:4px'>" + t + "</h6>"; }
                if (t!="") {
                    t = "<tr class='tbl_row'><td colspan=" + (show_prty ? 2 : 1) + " style='min-height:" + (path == "" ? "2.8" : "3.8") + "em; background: #ffffaa;' valign='middle'><h6 style='padding:6px 4px;'" + (old_rid != id ? " class='blink_twice'" : "") + ">" + t + "</h6><span id='spanScaleHdr'></span></td></tr>";
                }
                var sel = cb.get_value();
                items = cb.get_items();
                var cnt = items.get_count();
                var tbl = "<table border=0 class='text tbl' cellspacing=1 cellpadding=" + (cnt<5 ? 2 : (cnt<10 ? 1 : 0)) + " style='margin-right:1em'>" + t + "<tr class='tbl_hdr'><td><% =tblIntensity %></td>" + (show_prty ? "<td><% =tblPriority %></td>" : "") + "</tr>\n";
                for (i=0; i<=(show_direct ? cnt : cnt-1); i++)
                {
                    var name = "";
                    var name_extra = "";
                    var prty = "";
                    var val = "";
                    var act = false;
                    var extra = (i==items.get_count());
                    if (extra)
                    {
                        act = (sel=="");
                        val = -2;
                        name = "<% =JS_SafeString(lblDirectValue) %>";
                        var direct = "";
                        var dir = eval("theForm.RatingDirect" + id);
                        //                        if ((dir) && dir.value!="" && dir.value!=-1) direct = cb.get_text();
                        if ((dir) && dir.value!="" && (cb.get_value()<-1 || cb.get_value()=="")) direct = dir.value;
                        if ((usd_show) && direct!="" && usd_cost>0) {
                            d = CheckComboNumber(direct);
                            if (d.valid) {
                                direct = usd_cost * d.value;
                                if ((direct+"").replace(",",".").indexOf(".")>0) direct = Math.round(direct*100)/100;
                            }
                        } 
                        prty = "<input type='text' class='input' style='width:100%' name='DirectRating' value='" + direct + "' autocomplete='off' onblur='onKeyUpDirect(" + id + "); focused_direct=0;' onfocus='focused_direct=1;' onkeyup='onKeyUpDirect(" + id + ");' onmousedown='focusOnClick(this);'>";
                        if (obj_act == null) obj_act = "DirectRating";
                    }
                    else
                    {
                        var item = items.getItem(i);
                        var data = item.get_attributes().getAttribute("data");
                        if ((data))
                        {
                            data = eval(data);
                            act = (item.get_value() == sel);
                            val = item.get_value();
                            name = data[0];
                            var c_fld = eval("theForm.Intensity_" + data[3]);
                            if ((c_fld))
                            {
                                var com = c_fld.value;
                                if ((com)<% If CanEditInfodocs Then%> || (val>=0)<% End If%>) {
                                if (!(com)) com = "";
                                com = replaceString("<", "&lt;", replaceString(">", "&gt;", com));
                                <% If CanEditInfodocs AndAlso ShowFramedInfodocs Then %>name_extra = "&nbsp;<a href='' onclick='EditIntensityDesc(\"" + id + "\", " + i + ", " + (i>4 ? 1 : 0) + "); return false;'><img id='imgDesc" + i + "' src='<% =ImagePath %>edit_tiny.gif' width=10 height=10 border=0 title='<% =SafeFormString(lblIntensityDescriptionInput) %>' style='position: absolute; display:none'/></a>";<% End if %>
                                name_extra = "<% If ShowFramedInfodocs Then%><span class='text small' style='color:#336666; font-weight:normal;'><span id='IntRow" + i + "' style='max-width:250px;'" + (com.length>max_desc_len ? "title='" + replaceString("'", "&#39;", com) + "'" : "") + ">" + (com=="" ? "" : " &middot;&nbsp;" + (com.length>max_desc_len ? com.substr(0,max_desc_len).trim() + "&#133;" : com)) + "</span>" + name_extra + "</span><% Else%><img src='" + ((com!="") ? img_int_1.src: img_int_0.src) + "' width=12 height=12 border=0 title='" + com + "' id='Int" + i + "'/><% End If%>";
                                <% If CanEditInfodocs AndAlso Not ShowFramedInfodocs Then%>name_extra = "<a href='' onclick='EditIntensityDesc(\"" + id + "\", " + i + ", " + (i>4 ? 1 : 0) + "); return false;'>" + name_extra + "</a>";<% End If%>
                                name_extra = "&nbsp;&nbsp;" + name_extra;
                            }
                       }
                       fill = (act ? "fill1" : "fill_edit");
                       var sVal = data[2];
                       if ((usd_show) && 1*data[1]>=0 && usd_cost>0) {
                           sVal =  showCost(usd_cost * data[1]);
                       }
                       prty = (data[1]>=0 ? "<table border=0 cellspacing=0 cellpadding=0 width='99%'><tr valign=middle><td align=right class='text small'><nobr>" + sVal + "</nobr></td><td style='width:" + width + "px; padding-left:4px;'>" +  GetBar(data[1], (first_load ? 10+75*i : 0), 0) + "</td></tr></table>" : "&nbsp;");
                       if ((act)) obj_act = "int_" + data[3];
                   }
               }
               if (name!="")
               {
                   if (cnt>0) {
                       var onclick = " onclick='" + (extra ? "InitDirect(" + id + "); " : "SetRatingValue(" + id + "," + val + ");") +"'";
                       tbl += "<tr class='tbl_row" + (act ? "_sel' style='color:#000099; font-weight:bold" : "") + "'><td<% If CanEditInfodocs AndAlso ShowFramedInfodocs Then %> onmouseover='$(\"#imgDesc" + i + "\").show();' onmouseout='$(\"#imgDesc" + i + "\").hide();'<% End If%> style='min-width:8em'><span style='max-width:12em; padding-left:1px;'><input type='radio' name='rating_value' id='int_" + data[3] + "' " + (act ? "checked " : "") + "value='" + val + "'" + onclick + "><span" + onclick +" class='aslink'>" + name + "</span></span>" + name_extra + "</td>" + (show_prty ? "<td class='small' style='text-align:right; padding-left:3px;padding-right:3px; width:120px;'" + (extra ? "" : onclick) + "><nobr>" + prty + "</nobr></td>" : "") + "</tr>\n";
                   } else {
                       tbl += "<tr class='tbl_row'><td colspan='" + (show_prty ? 2 : 1) + "'><div style='padding:2em; text-align:center; font-weight: 4000;'>No data</div></td></tr>\n";
                   }
               }
           }
           tbl += "</table>";
           var _t = $("#_item_" + id);
           if ((_t) && (_t.length) && _t.html()!="" && typeof setObjectName == "function") setObjectName(_t.html());
           old_rid = id;

           var frm = null;
           var scale_desc = cb.get_attributes().getAttribute("data");
           if ((scale_desc))
           {
               scale_desc = eval(scale_desc);
               var d = "";
                frm = $get("divScaleDesc");
                <% If ShowFramedInfodocs AndAlso tooltipScaleDesc.Visible Then%>
                if ((frm)) frm.style.display = "none";<% Else%>
                <% If tooltipScaleDesc.Visible Then %>if ((<% =iif(CanEditInfodocs, "true", "scale_desc[1]") %>)) d = "<img src='<% =ImagePath %>" + ((scale_desc[1]) ? "info15.png" : "info15_dis.png") + "' id='imgScaleDesc' width=15 height=15 border=0 title='' style='float:left; margin-right:2px;'/>";<% End If%>
                <% End If%>
                <% If ScaleInfodocEditURL <> "" AndAlso CanEditInfodocs Then%>if (d!="") d = "<a href='' onclick=\"" + replaceString("%%guid%%", scale_desc[0], "<% =IIf(CanEditInfodocs, ScaleInfodocEditURL, String.Format("open_infodoc('{0}'); return false;", JS_SafeString(ScaleInfodocURL))) %>") +"\">" + d + "</a>";<% End If%>
                tbl = d + tbl;
            }

            r.innerHTML = tbl;

            <% If ShowFramedInfodocs Then%>
            if ((frm))
            {
                var d = <% =iif(CanEditInfodocs, "1", "((scale_desc) && (scale_desc[1]))") %>;
                if ((d)) {
                    frm.style.display = "block";
                    var url = replaceString("%%guid%%", scale_desc[0], "<% =ScaleInfodocURL %>")+"&r=" + Math.round(10000*Math.random());
                    var editurl = replaceString("%%guid%%", scale_desc[0], "<% =ScaleInfodocEditURL %>");
                    LoadInfodocFrame("scale", "<% = JS_SafeString(lblInfodocTitleScale)%>", url, editurl);
<%--                    var f = $get("scale");
                    if ((f)) f.src = replaceString("%%guid%%", scale_desc[0], "<% =ScaleInfodocURL %>")+"&r=" + Math.round(10000*Math.random());--%>
                }
            }<% Else%>
            var ti = $find("<% =tooltipScaleDesc.ClientID %>");
            if ((scale_desc) && (ti)) { 
                ti.set_targetControlID(""); 
                ti.set_content("");
                if ((scale_desc[1])) {
                    txt = "<iframe style='border:0px; padding:2px; width:99%' frameborder='0' allowtransparency='true' src='" + replaceString("%%guid%%", scale_desc[0], "<% =ScaleInfodocURL %>") + "&r=" + Math.round(10000*Math.random()) + "' class='frm_loading' onload='InfodocFrameLoaded(this);'></iframe>";
                        ti.set_content(txt);
                    }
                    ti.set_targetControlID("imgScaleDesc"); 
                } <% End If%>
                if ((obj_act) && (typeof is_mobile == "undefined" || !(is_mobile))) {
                    //                    var o = eval(obj_act);
                    var o = document.getElementById(obj_act);
                    if ((o)) o.scrollIntoView(false);
                }
                //if ((sd)) sd.width(r.clientWidth).show();
            }
        }
        first_load = false; 
        return false;
    }

    function InitDirect(id)
    {
        var val = theForm.DirectRating.value;
        var combo = $find(ComboBoxes[id]);
        if ((combo))
        {
            combo.clearSelection();
            combo.set_text(theForm.DirectRating.value);
        }
        SetRating(id, -1, val);
        SetDirectValue(combo, -id);
    }

    function onKeyUpDirect(id)
    {
        if (!hook_keypress) return false;
        var d = theForm.DirectRating;
        var c = $find(ComboBoxes[id]);
        if ((d) && (c))
        {
            if (SetDirectValue(c, id)) return false;

            _is_manual = 1;
            var e = c.get_inputDomElement();
            var val = d.value;
            if ((usd_show) && usd_cost>0) {
                var v = (val+"").replace(",", ".");
                if (v[0] == ".") v = "0" + v;
                if ((v*1) == v) val = (v / usd_cost).toFixed(4);
            }
            var v = CheckComboNumber(val);
            if ((v.valid))
            {
                d.style.backgroundColor = "";
                if (val!="")
                {
                    SetRating(id, -1, v.value);
                    SetBarByID(id, v.value);
                }
            }
            else
            {
                d.style.backgroundColor = "#ffe0e0";
            }
            if ((e))
            {
                if (val!="")
                {
                    e.value = val;
                    var l = $get(RatingLabels[id]);
                    if ((l))
                    {
                        l.innerHTML = val;
                        l.style.backgroundColor = d.style.backgroundColor;
                    }
                }
                e.style.backgroundColor = d.style.backgroundColor;
            }
            _is_manual = 0;
        }
    }

    function SetRatingValue(id, val)
    {
        id = id*1;
        var cb = $find(ComboBoxes[id]);
        if ((cb))
        {
            var item = cb.findItemByValue(val+"");
            if ((item))
            {
                cb.trackChanges();
                item.select();
                cb.commitChanges();
                var l = $get(RatingLabels[id]);
                if ((l)) l.innerHTML = ShortString(cb.get_text(), 85, false);
            }
            cb.hideDropDown();
            if (id<(ComboBoxes.length-1))
            {
                //setTimeout("SetFocus(" + (id) + ", -1, false, 0);", 30);
                //setTimeout('SetFocus(' + (1+id) + ', 1, true, 0);', 50);
                setTimeout("SetFocus(" + (1+id) + ", 3, false, 0);", 100);
                setTimeout("SetFocus(" + (1+id) + ", -1, false, 0);", 250); 
                setTimeout("SetFocus(" + (1+id) + ", 3, false, 0);", 350);
                setTimeout("SetFocus(" + (1+id) + ", -1, false, 0);", 500);
                setTimeout("SetFocus(" + (1+id) + ", 3, false, 0);", 600);
                setTimeout("SetFocus(" + (1+id) + ", 1, true, 0);", 750);
            }   
        }
    }
    
    function GetBar(value, delay, start_value)
    {
        var fillWidth  = Math.round(value * width);
        if (fillWidth<1) fillWidth = 1; else if (fillWidth>width) fillWidth = width;
        var style= (value<0.0001 ? "" : fill);
        var img_id = Math.round(9999*Math.random());
        var w = (delay>0 && fillWidth>1 ? start_value : fillWidth);
        //var sFill = "<div id='div" + img_id + "' class='" + style + "' style='width:" + w +"px;margin:0px;padding:0px;'><img src='" + imgname + "' height='" + (height-padding) + "' width='" + w + "' id='bar" + img_id + "' border='0' title='" + value + "'/></div>";
        var sFill = '<span class="' + style + '" style="float: left;"><img height="' + (height+2*padding) + '" id="bar' + img_id + '" style="width:' + (w) + 'px; display: inline-block;" src="' + imgname + '" border="0"></span>';
        var sBar = "<div class='progress' style='height:" + (height+2*padding) + "px;width:" + (width+0*padding) + "px;padding:" + padding + "px;'>" + sFill + "</div>";
        if (delay>0 && fillWidth>0) setTimeout('$("#bar' + img_id + '").animate({width: ' + fillWidth + '}, 350); $("#div' + img_id + '").animate({width: ' + fillWidth + '}, 350);', delay);
        return sBar;
    }

    function onClientKeyPressing(sender, eventArgs)
    {
        var code = eventArgs.get_domEvent().keyCode;
        if (eventArgs.get_domEvent().ctrlKey || code==38 || code==40 || code==17 || code==18)
        {
           eventArgs.returnValue = false;
           return false;
        }
        if (code == 13 || code == 9)
        {
            sender.hideDropDown();
        }
    }

    function SetComboFocus()
    {
        <% If Not _OPT_SHOW_COMBOBOXES Then %>SetDirectFocus(); return false;<% End if %>
        var c = $find(ComboBoxes[focus_active_id]);
        if ((c))
        {
            var e = c.get_inputDomElement();
            if ((e)) { e.focus(); e.select(); }
        }
    }

    function SetDirectFocus()
    {
        var d = theForm.DirectRating;
        if ((d)) { d.focus(); d.select(); }
    }

    function RatingHotkeys(event)
    {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event && (theForm.DirectRating) && (!show_comment) && (1*focus_active_id>=0))
        {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            var dir = theForm.DirectRating.value;
            if (((code==40 || code==38) && event.ctrlKey) || code==13)
            {
                var f_c = focused_combo;
                var f_d = focused_direct;
//                var v = CheckComboNumber(dir);
//                if (!v.valid) { ShowWarning(focus_active_id, false); return false; }
                var ofs = (code == 38 ? -1 : +1);
                if (ofs+1*focus_active_id>=0 && ofs+1*focus_active_id<=Ratings.length-1)
                { 
                    var cb = $find(ComboBoxes[1*focus_active_id]);
                    if ((cb)) CheckComboEdit(cb);
                    SetFocus(1*focus_active_id+ofs, 1, true, 1*f_c + 2*f_d);
//                    if ((f_d)) setTimeout("SetDirectFocus();", 30);
                    return false;
                }
            }
            if (code==17 || code==18 || event.ctrlKey) return false;
            
            if (code==48 || code==49 || code==188 || code==96 || code==97 || code==110 || code==190)
            {
              var d = theForm.DirectRating;
              if ((d) && !(focused_combo) && (d.value=="") && (hook_keypress))
              {
                var c = "";
                if (code==48 || code==96) c = "0";
                if (code==49 || code==97) c = "1";
                if (code==188) c = "0,";
                if (code==190 || code==110) c = "0.";
                if (c!="")
                {
                  if (!focused_direct) SetDirectFocus();
                  d.value = c;
                  InitDirect(focus_active_id);
                  onKeyUpDirect(focus_active_id);
                  return false;
                }
              }
            }
        }
        //return true;
    }

    function InitCursor(focus_id) {
        $("tblRatingsMain").hide();
        var del = 20;
        if (bar2animate.length>0) {
            del += 100+(bar2animate.length*50);
            if (del>700) del=700;
            for (var i=0; i< bar2animate.length; i++) {
                setTimeout('$("#' + bar2animate[i][0] + '").animate({width: ' + bar2animate[i][1] + '}, 500);', 30+50*i);
            }
        }
        setTimeout('InitCombos(' + focus_id + ', 0);' + ((usd_show) ? 'switchUSD(true); $("tblRatingsMain").show();' : '') , del);
    }

    document.onkeydown = RatingHotkeys;
    
<% if CanEditInfodocs Then %> 
    function onRichEditorRefresh(empty, infodoc, callback)
    {
        if ((callback) && (callback[0]) && callback[0]!="")
        {
            var framed = <% =iif(ShowFramedInfodocs, "1", "0") %>;
            var n = callback[0];
            var t;
            var id;
            var wrt;
            if (callback[0]=="node") 
            { 
                if (!framed)
                {
                    n="<% =imgCaptionInfodoc.ClientID %>"; 
                    t="<% =tooltipGoal.ClientID %>"; 
                }
            }
            if (callback[0].substr(0,3)=="alt")
            {
                id = 1* callback[0].substr(3);
                if (!framed)
                {
                    n = ImagesInfodoc[id];
                    t = TooltipsInfodoc[id];
                }    
                else
                {
                n = "alt";
                callback[0] = n;
                }
                wrt = false;
            }
            if (callback[0].substr(0,3)=="wrt")
            {
                id = 1* callback[0].substr(3);
                if (!framed)
                {
                n = ImagesWRTInfodoc[id];
                t = TooltipsWRT[id];
                }  
                else
                {
                n = "wrt";
                callback[0] = n;
                }
                wrt = true;
            }
            if (callback[0].substr(0,5)=="scale")
            {
                if (!framed)
                {
                n = "imgScaleDesc";
                t = "<% =tooltipScaleDesc.ClientID %>";
                }  
                else
                {
                n = "scale";
                callback[0] = n;
                }
            }
            var f = $get(n);
            if ((f))
            {
                var src = callback[1];
                if (framed && src == "" && location.href!=f.src) src = f.src;
                //if (src == "" && (callback[1]) && callback[1]!="") src = callback[1];                
                if (src!="") src = src + "&=" + Math.random();
                if (empty==1) src = "";
                if (framed)
                {
                    if ((f.contentWindow)) f.contentWindow.document.body.innerHTML = "<div style='<% =String.Format(FrameLoadingStyle, ImagePath)%>'>&nbsp</div>";
                    setTimeout("var f = $get('" + callback[0] + "'); if ((f)) f.src= \"" + src + "\";" , 200);
                }   
                else
                {
                    if (callback[0].substr(0,3)=="wrt")
                    {
                        f.src = (empty==1 ? img_WRTinfodoc2 .src : img_WRTinfodoc1.src);
                    }
                    else
                    {   
                        f.src = (empty==1 ? img_infodoc2.src : img_infodoc1.src);
                    }
                    if ((t))
                    {
                        var tooltip = $find(t);
                        if ((tooltip))
                        {
                        var vis = tooltip.isVisible();
                        tooltip.hide();
                        if (src=="") 
                        {
                            txt = "";
                        }
                        else 
                        {
                            txt = "<iframe style='border:0px; padding:2px; width:99%' frameborder='0' allowtransparency='true' src='" + src + "' class='frm_loading' onload='InfodocFrameLoaded(this);'></iframe>";
                            if (vis) tooltip.show();
                        }  
                        tooltip.set_content(txt);
                        }  
                    }
                }    
            }
        }
        window.focus();
    }

    function SaveDescription() {
        var radToolTip = $find("<%= ttEditDescription.ClientID %>");
        var c_fld = eval("theForm.Intensity_" + theForm.fldID.value);
        if ((c_fld) && (radToolTip) && (c_fld.value != desc)) {
            radToolTip.hide();

            var desc = theForm.tbDescription.value;
            var idx = theForm.curDescID.value;

            c_fld.value = desc;

            desc = replaceString("<", "&lt;", replaceString(">", "&gt;", desc));

            var img = $get("Int" + idx);
            if ((img)) { img.title = desc; img.src = (desc=="" ? img_int_0.src : img_int_1.src); }
            var d = $get("IntRow" + idx);
            if ((d)) {
                d.innerHTML = (desc=="" ? "" : " &middot;&nbsp;" + (desc.length>100 ? desc.substr(0,100) + "&#133;" : desc));
                d.title = desc;
            }

            <% if changeFlagID<>"" Then %>if (theForm.<% =changeFlagID %>) theForm.<% =changeFlagID %>.value=1;<% end if %>
            show_comment = 0;
        }
    }

    function EditIntensityDesc(cb_id, idx, top)
    {
        var cb = $find(ComboBoxes[cb_id]);
        if ((cb))
        {
            var items = cb.get_items();
            if ((items))
            {
                var item = items.getItem(idx);
                if ((item))
                {
                    var data = item.get_attributes().getAttribute("data");
                    if ((data))
                    {
                        var c_fld = eval("theForm.Intensity_" + eval(data)[3]);
                        if ((c_fld))
                        {
                            var desc = c_fld.value;
                            var radToolTip = $find("<%= ttEditDescription.ClientID %>");
                            if ((radToolTip)) {
                                show_comment = 1;
                                radToolTip.set_targetControlID("<% =iif(ShowFramedInfodocs, "imgDesc", "Int") %>" + idx);
                                radToolTip.set_position(top ? 11 : 31);
                                theForm.tbDescription.value = desc;
                                theForm.curDescID.value = idx;
                                theForm.fldID.value = eval(data)[3];
                                radToolTip.show();
                                setTimeout('theForm.tbDescription.focus();', 100);
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    document.cookie = "mr_save=;scale_save=;";

<% End If %>

    function onResizeRatings() {
      $("#divList").height(80);
      $("#divRatings").height(80);
      var h = Math.round($("#tdList").parent().height()-8);
      if (h>80) {
          $("#divList").height(h);
          $("#divRatings").height(h);
      }
  }
    
  function onMouseUpResize() {  // on collapse/expand infodocs
      setTimeout('onResizeRatings();', 5);
  }

  window.addEventListener("beforeunload", function (e) {
      fixComboboxes();
  });

  resize_custom = onResizeRatings;
  window.onmouseup = onMouseUpResize;

//-->    
</script><% If ShowFramedInfodocs AndAlso frmInfodocGoal IsNot Nothing Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%><table border="0" cellpadding="0" cellspacing="2" id="tblRatingsMain" class="anytimeMainTable">
<tr style="height:1ex;"><td colspan="2" align="center"><% If ShowFramedInfodocs AndAlso frmInfodocWRT IsNot Nothing Then%><table border="0" cellpadding="2" cellspacing="0"><tr valign="top">
<td runat="server" id="tdInfodocLeft"></td><td runat="server" id="tdInfodocCenter"></td><td><EC:FramedInfodoc runat="server" ID="frmInfodocWRT"/></td><td style="width:1%;"><div id='divScaleDesc' style='display:none;'><EC:FramedInfodoc runat="server" ID="frmInfodocScale" Visible="false"/></div></td>
</tr></table><% Else %>
<div runat="server" id="trCaption" style="text-align:center; padding:1ex 0px 0.5em 0px" class="text"><h4><asp:Image runat="server" ID="imgCaptionInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipGoal" Visible="false"  TargetControlID="imgCaptionInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>&nbsp;<asp:Label ID="lblCaption" runat="server"></asp:Label></h4></div>
<% End If %></td></tr>
<tr valign="top"><td id="tdList" align="right" width="60%"><%--<img src='<% =ImagePath %>blank.gif' width='1' height='140' style='float:left'>--%><div id="divList" style='height:99%; overflow-y:auto; text-align:center; padding:0px 2px 0px 2px;'><%--<asp:Panel runat="server" ID="pnlRatings" HorizontalAlign="Center" Wrap="false" ScrollBars="None" CssClass="whole">--%>
<input type="hidden" name="RatingsCount" value="<%=GetRatingsCount%>"/><asp:Repeater ID="rptAlts" runat="server" OnItemDataBound="rptAlts_ItemDataBound">
<HeaderTemplate>
    <table border="0" cellpadding="0" cellspacing="0" class="whole"><tr><td valign="top" align="right"><table border="0" cellpadding="3" cellspacing="0">
</HeaderTemplate>
<FooterTemplate>
    </table></td></tr></table>
</FooterTemplate>
<ItemTemplate>
<tr valign="middle" class="text tbl_row<%#iif(Cint(DataBinder.Eval(Container, "ItemIndex")) mod 2 = 1, "", "_alt") %>" id="row<%#DataBinder.Eval(Container, "ItemIndex")%>" onmouseover="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', 2, false, 0);" onmouseout="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', -1, false, 0);" onclick="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', 4, true, 1*focused_combo + 2*focused_direct);" >
 <td style="width:1px"><nobr><asp:Image runat="server" ID="imgInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipInfodoc" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/></nobr></td>
 <td style="padding:4px 3px;"><%#GetNodePath(Container.DataItem)%><% If ShowIndex Then %><%#DataBinder.Eval(Container.DataItem, "Idx")%>.&nbsp;<% End If %><b><span id='_item_<%#DataBinder.Eval(Container, "ItemIndex")%>'><%#DataBinder.Eval(Container.DataItem, "Title")%></span></b><span style="display:none; margin-top:6px;" id="infodoc_<%#DataBinder.Eval(Container.DataItem, "ID")%>"></span></td>
 <td style="width:<% =ComboWidth*7 %>px; padding-left:1ex; text-align:left"><asp:Image runat="server" ID="imgWRT" SkinID="InfoReadme" Visible="false" /><asp:Label runat="server" ID="lblRatingValue" Visible="false" /> <telerik:RadComboBox ID="ddListRatings" runat="server" ExpandAnimation-Duration="100" CollapseAnimation-Duration="100" DataTextField="Name" DataValueField="ID" OnPreRender="ddListRatings_PreRender" Width="130px" AllowCustomText="true" IsCaseSensitive="false" MarkFirstMatch="false" CloseDropDownOnBlur="true" EnableTextSelection="false" ShowDropDownOnTextboxClick="false" ChangeTextOnKeyBoardNavigation="false" OnClientTextChange="onClientTextChange" OnClientFocus="onClientFocused" OnClientBlur="onClientBlur" OnClientKeyPressing="onClientKeyPressing" /><input type="hidden" name="Rating<%#DataBinder.Eval(Container.DataItem, "ID")%>" value="<%#DataBinder.Eval(Container.DataItem, "RatingID")%>"/><input type="hidden" name="RatingDirect<%#DataBinder.Eval(Container.DataItem, "ID")%>" value="<%#DataBinder.Eval(Container.DataItem, "DirectData")%>"/><telerik:RadToolTip runat="server" ID="tooltipWRT" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/><span style="display:none; margin-top:2px;" id="infodoc_wrt_<%#DataBinder.Eval(Container.DataItem, "ID")%>"></span></td>
 <td style="white-space:nowrap; width:<% =BAR_WIDTH + (Precision+3) * 6 + 20 %>px;<% =iif(ShowPrtyValues, "", "display:none;") %>" class='text small' align='right' id="bar<%#DataBinder.Eval(Container.DataItem, "ID")%>"><asp:Label runat="server" ID="Bar" /></td>
 <td style="<% =If(ShowComment, "", "display:none;") %>"><asp:Image runat="server" id="imgComment" Width="16" SkinID="NoteDisabledIcon" CssClass="aslink" Visible="false"/><telerik:radtooltip runat="server" id="ttEditComment" Skin="WebBlue" SkinID="tooltipInfo" RelativeTo="Element" Position="BottomLeft" OffsetX="-8" OffsetY="-7" ShowEvent="onClick" HideEvent="ManualClose" CssClass="panel" Visible="false" OnClientShow="onShowComment">
<div class="panel" style="width:99%">
<div class="text" style="font-weight:bold; white-space:normal; max-width:400px;" id="divCommentTitle"><%=lblCommentTitle%> for <%#DataBinder.Eval(Container.DataItem, "Title")%>:</div>
<textarea name="Comment<%#DataBinder.Eval(Container.DataItem, "ID")%>" rows="8" cols="45" class="textarea" style="width:97%; margin:1ex 0px"><%#DataBinder.Eval(Container.DataItem, "Comment")%></textarea>
<div style="text-align:right"><asp:Button runat="server" ID="btnSave" UseSubmitBehavior="false" CssClass="button button_small"/><asp:Button runat="server" ID="btnCancel" Text="Cancel" OnClientClick="return false;" UseSubmitBehavior="false" CssClass="button button_small"/></div>
</div></telerik:radtooltip></td>
</tr>
</ItemTemplate>
</asp:Repeater><% =IIf(rptAlts Is Nothing OrElse rptAlts.Items.Count = 0, "<div style='padding-top:7em;'><h4>No data to display</h4><p align=center class='text gray'>Please check the measurement scale</p></div>", "")%>
<%--</asp:Panel>--%></div><img src='<% =ImagePath %>blank.gif' width='180' height='1' border='0' title=''/></td>
<td align="left" class="text" style="padding-left:1em; max-width:40%;"><div id="divRatings" style="height:99%; overflow-y:auto"><div class='small gray' style='text-align:center; padding-top:2em'><img src='<% =ImagePath %>devex_loading.gif' width=16 height=16 border=0 title='' style='margin:5em'/></div></div><img src='<% =ImagePath %>blank.gif' width='180' height='1' border='0' title='' id="imgList"/></td></tr>
</table>
<telerik:RadToolTip runat="server" ID="tooltipScaleDesc" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" OffsetX="-8" OffsetY="-6" ShowEvent="OnMouseOver" IsClientID="true" OnClientBeforeShow="OnToolTipShowHandler"/>
<telerik:radtooltip runat="server" id="ttEditDescription" Skin="WebBlue" SkinID="tooltipInfo" RelativeTo="Element" Position="BottomLeft" OffsetX="-5" ShowEvent="FromCode" HideEvent="ManualClose" CssClass="panel" IsClientID="true" >
<div class="text" style="font-weight:bold;"><%=lblIntensityDescriptionInput%>:</div>
<textarea name="tbDescription" rows="8" cols="45" class="textarea" style="margin:1ex 0px"></textarea>
<div style="text-align:right"><input type='button' id='btnSaveDesc' value='<% = CaptionSaveComment %>' onclick='SaveDescription();' class='button button_small'/><input type='hidden' id='curDescID' value='-1'/><input type='hidden' id='fldID' value='-1'/></div>
</telerik:radtooltip>
<% =ScalesComment %>