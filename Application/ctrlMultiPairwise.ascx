<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlMultiPairwise" Codebehind="ctrlMultiPairwise.ascx.vb" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<script type="text/javascript" language="javascript" src='<% =sRootPath %>Scripts/sliderbars.js'></script>
<script type="text/javascript" language="javascript" src='<% =sRootPath %>Scripts/twinslider.js'></script>
<script type="text/javascript"><!--

    var img_path        = '<% =ImagePath %>';

    var img_blank       = img_path + 'blank.gif';
    
    var img_infodoc1	= new Image; img_infodoc1.src    = img_path + '<% =ImageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src    = img_path + '<% =ImageInfodocEmpty %>';
    var img_WRTinfodoc1	= new Image; img_WRTinfodoc1.src = img_path + '<% =ImageWRTInfodoc %>';
    var img_WRTinfodoc2	= new Image; img_WRTinfodoc2.src = img_path + '<% =ImageWRTInfodocEmpty %>';

    var img_Scale       = new Image; img_Scale.src = "<% =String.Format("{0}PWScale.aspx?type={1}", _URL_EVALUATE, clsPairwiseData.Scale_Tiny) %>";
    var img_ScaleHelp   = new Image; img_ScaleHelp.src = img_path + "help16.gif";

    _img_slider_images_path = img_path;
    _img_slider_position.src = _img_slider_images_path + "slider_position.gif";
    _img_slider_handler.src = _img_slider_images_path + "slider_handler.gif";
    _img_slider_handler2.src = _img_slider_images_path + "slider_handler2.gif";
    _img_slider_blank.src = _img_slider_images_path + "blank.gif";

    var _img_slider_blue = new Image();    _img_slider_blue.src = _img_slider_images_path + "slider_blue.gif";
    var _img_slider_red = new Image();    _img_slider_red.src = _img_slider_images_path + "slider_red.gif";

    var img_decrease = new Image; img_decrease.src = img_path + 'decrease.gif';
    var img_decrease_ = new Image; img_decrease_.src = img_path + 'decrease_.gif';
    var img_increase = new Image; img_increase.src = img_path + 'increase.gif';
    var img_increase_ = new Image; img_increase_.src = img_path + 'increase_.gif';
    var img_delete = new Image; img_delete.src = img_path + 'delete_tiny.gif';
    var img_delete_ = new Image; img_delete_.src = img_path + 'nodelete_tiny.gif';
    var img_swap = new Image; img_swap.src = img_path + 'swap.gif';

    var img_note_empty  = "<% =ImageCommentEmpty %>";
    var img_note_exists = "<% =ImageCommentExists %>";

    var pw_count        = <% =Data.Count %>;
    
    var TooltipsInfodoc = [<% =_TooltipsInfodocList %>];
    var TooltipsWRT     = [<% =_TooltipsWRTList %>];
    var ImagesInfodoc   = [<% =_ImagesList %>];
    var ImagesWRTInfodoc= [<% =_ImagesWRTList %>];
    var ImagesComments  = [<% =_ImagesCommentsList %>];
    var LInfodocURLs     = [<% =_LeftInfodocURLsList %>];
    var LWRTInfodocURLs  = [<% =_LeftWRTInfodocURLsList %>];
    var LInfodocTitles   = [<% =_LeftInfodocTitlesList %>];
    var LInfodocWRTTitles= [<% =_LeftInfodocTitlesWRTList %>];
    var LInfodocEditURLs     = [<% =_LeftInfodocEditURLsList %>];
    var LWRTInfodocEditURLs  = [<% =_LeftWRTInfodocEditURLsList %>];
    var RInfodocURLs     = [<% =_RightInfodocURLsList %>];
    var RWRTInfodocURLs  = [<% =_RightWRTInfodocURLsList %>];
    var RInfodocTitles   = [<% =_RightInfodocTitlesList %>];
    var RInfodocWRTTitles= [<% =_RightInfodocTitlesWRTList %>];
    var RInfodocEditURLs     = [<% =_RightInfodocEditURLsList %>];
    var RWRTInfodocEditURLs  = [<% =_RightWRTInfodocEditURLsList %>];
    
    var hints = [<% =GetHints() %>];
    
    var fill_style = '<% =_BarStyle %>';
    var height = <% =_BarHeight %>+2;
    var width = <% =_BarWidth %>-3;
    var padding = 2;
    var imgname = '<% =BlankImage %>';
    var pending_last = "";
    
    var steps = 8;
    var cell_width = 14;
    var cell_height = 14;
    
    var gpwSlideEnd = 0;
    
    var value_undef = <% =pwUndefinedValue %>;

    slider_gpw_real_max = <% =GetSliderMax %>;
    slider_gpw_real_over_max = <% =GetSliderMaxReal %>;

    var sliders = new Array();
    slider_on_change = "isGPWChanged(slider_id, val, adv, slider_drag_end);";

    var focus_active_id = -1;

    function GetPairwise(id, value)
    {
        var pw = "";
        var style = fill_style;
        if (value>=-steps) value=-value;
        for (var i=-steps; i<=steps; i++)
        {
            var cell_class = (i == 0 ? (Math.abs(value) <= steps ? (value == 0 ? "equal_dark" : "equal") : "undef_mpw") : "empty");
            var hint = hints[i+steps]; // due to have telerik problem hints[i+steps];
            var j = Math.floor(value) + (i<0 ? 0 : 1);
            if (value<0 && i<0 && Math.abs(value)<=steps) if (value<=i) cell_class = "left";
            if (value>0 && i>0 && Math.abs(value)<=steps) if (value>=i) cell_class = "right";
            var cell = "<img src='"+ imgname + "' width=" + cell_width + " height=" + cell_height + " class=\"no-tooltip\" border=0 alt=\"" + hint + "\" title=\"" + hint + "\">";
            cell = "<a href='' onclick='SetPairwise(\"" + id + "\"," + -i +");return false;'>" + cell + "</a>";
            cell = "<div class='" + style + "_" + cell_class + "' style='width:" + cell_width + "px;'>" + cell + "</div>";
            pw += "<td style='padding:0px;margin:0px;border:0px;text-align:" + (i<0 ? "right" : "left")+ ";width:" + cell_width + "px;'>" + cell + "</td>";
        }
        var undef = (value < -steps);
        var r = "<img src='" + img_path + (undef ? "nodelete_tiny.gif" : "delete_tiny.gif") +"' width=10 height=10 border=0 class=\"no-tooltip\" title='Reset'>";
        r = "<a href='' onclick='SetPairwise(\"" + id + "\",value_undef); return false;'>" + r + "</a>";
        pw = "<table border=0 cellspacing=1 cellpadding=0 class='" + style + "_tbl' align='center'><tr style='font-size:" + (cell_height-2) + "px;'>" + pw + "</tr></table>";
        pw = "<table border=0 cellspacing=0 cellpadding=0 align='center'><tr class='text small'><td style='padding:2px 3px'>"+pw+"</td><td width=16>"+r+"</td></table>";
        return pw;         
    }
    
    function SetPairwise(id, value)
    {
        var pw = $get("PW"+id);
        var v =eval("theForm.value" + id);
        var a =eval("theForm.adv" + id);
        if ((pw) && (v) &&(a))
        {
            pw.innerHTML = GetPairwise(id, value);
            v.value = Math.abs(value)+1;
            a.value = (value<0 ? -1 : 1);
            var w =eval("theForm.warning" + id);
            if ((w)) w.value = "0";
            Changed();
            if (value >= -steps) {
                SetFocusAfterFlash(id, getNextIndex(id));
                //SetFocusAfterFlash(id, (id < pw_count - 1 ? 1 * id + 1 : id));
            } else {
                SetFocus(id, 1);
            }
        }    
        return false;
    }

    function isUndefined(id)
    {
        var v = eval("theForm.value" + id);
        return ((v)) && (v.value=="" || Math.abs(v.value)>= <% =Math.Abs(pwUndefinedValue) %>);
    }

    function HasUndefined()
    {
        for (var i=0; i<pw_count; i++) { if (isUndefined(i)) return true; }
        return false;
    }

    function ShowWarning(vis)
    {
        //if ((vis)) $("#<% =lblMessage.ClientID %>").fadeIn(); else $("#<% =lblMessage.ClientID %>").fadeOut();    ' -D4614
    }

    function Changed()
    {
        <% if changeFlagID<>"" Then %>if (theForm.<% =changeFlagID %>) theForm.<% =changeFlagID %>.value=1;<% end if %>
        <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>
        ShowWarning(0);
        if (!HasUndefined()) setTimeout("FlashNextButton();", 1000);
    }
   
    function ApplyComment(id, tooltip, imgid)
    {
        var tb = eval("theForm.Comment" + id);
        if (tb) 
        {
            comment = tb.value;
            var img = $get(imgid);
            if (img)
            {
                img.src = (comment=="" ? img_note_empty  : img_note_exists);
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
        if (t)
        {
            if (t.isVisible) t.hide(); else t.show();
        }
    }
    
    function OnToolTipShowHandler(sender, args)
    {
        if ((sender) && (sender.get_text()=="")) args.set_cancel(true);
    }
    
    function InfodocFrameLoaded(frm)
    {
        if ((frm) && (frm.style)) { frm.style.backgroundImage='none'; };
//        if ((frm) && (frm.style)) { frm.className=''; };
    }

    function HideAllExceptOne(id)
    {
        for (var i=0; i<pw_count; i++)
            if (i!=id)
            {
                var tr = $get("row" + i);
                if ((tr)) tr.style.display = "none";
            }
    }

    function getFirstMissing(start) {
        for (var i = start; i < pw_count; i++) {
            if (isUndefined(i)) return i;
        }
        return -1;
    }

    function getNextIndex(start) {
        var idx = getFirstMissing(start);
        if (idx < 0) idx = getFirstMissing(0);
        if (idx < 0) {
            id = (start < (pw_count - 1) ? start + 1 : pw_count - 1);
        }
        return idx;
    }

    function InitSliders()
    {
        for (var i=0; i<pw_count; i++)
        {
            var val = eval("theForm.value" + i);
            var adv = eval("theForm.adv" + i);
            var d = $get("Slider" + i);
            if ((val) && (adv) && (d)) d.innerHTML = TwinSliderInit(i, 1*val.value, 1*adv.value, 0, 0, 0);
        }
        $('input[name^=' + slider_prefix + ']').on('keypress', function (e) { onGPWEnterKey(e); });
        InitList();
    }

    function onGPWEnterKey(e) {
        if ((e)) {
            var keyCode = e.keyCode || e.which;
            if (keyCode === 13) { 
                e.preventDefault();
                if (focus_active_id >= 0) {
                    SetFocusAfterFlash(focus_active_id, getNextIndex(focus_active_id));
                    var n = e.target.name;
                    if (typeof n!="undefined" && (n)) {
                        var f = (n.match("_a$") ? "_a" : "_b");
                        var fld = eval("theForm." + slider_prefix + id + f);
                        if ((fld)) fld.focus();
                    }
                }
                return false;
            }
        }
    }

    function isGPWChanged(id, val, adv, drag_end)
    {
        SavePWValue(id, val, adv);
        Changed();
        if (!(drag_end) && focus_active_id!=id) SetFocus(id, 1);
        if ((drag_end) && !isUndefined(id)) {
            SetFocusAfterFlash(id, getNextIndex(id));
            //SetFocusAfterFlash(id, (id < pw_count - 1 ? id + 1 : id));
        }
    }
    
    function SavePWValue(id, value, adv)
    {
        var v = eval("theForm.value" + id);
        var a = eval("theForm.adv" + id);
        if ((v) && (a)) { v.value = value; a.value = adv; }
    }

    function Reset(id)
    {
        UpdateRatio(id, value_undef);
        SetGPWValue(id, value_undef);
        <% if changeFlagID<>"" Then %>if (theForm.<% =changeFlagID %>) theForm.<% =changeFlagID %>.value=1;<% end if %>
    }
    
    function CheckUndefined()
    {
        if (HasUndefined() && !ConfirmMissingJudgment()) return false;
        return true;
    }

    function SetEditFocus(gpw_id, is_a)
    {
        var fld = (is_a ? TwinSliderGetEditA(gpw_id) : TwinSliderGetEditB(gpw_id));
        if ((fld)) fld.focus();
    }

    function CheckGPWForm()
    {
        var has_undef = false;
        for (i=0; i<pw_count; i++)
        {
            var gpw_id = i;
            var fld_a = TwinSliderGetEditA(gpw_id);
            var fld_b = TwinSliderGetEditB(gpw_id);
            if ((fld_a) && (fld_b) && (fld_a.value!="" || fld_b.value!=""))
            {
                var a = TwinSliderCheckEditField(fld_a);
                if (fld_a.value!="" && !a.valid) { dxDialog("<% = JS_SafeString(MsgWrongNumberPart) %>", true, "SetEditFocus('" + gpw_id + "', 1);"); return false; }
                var b = TwinSliderCheckEditField(fld_b);
                if (fld_b.value!="" && !b.valid) { dxDialog("<% = JS_SafeString(MsgWrongNumberPart) %>", true, "SetEditFocus('" + gpw_id + "', 0);"); return false; }
                <% If GPWMode<>GraphicalPairwiseMode.gpwmInfinite AndAlso GPWModeStrict Then %>if (a.valid && b.valid) {
                    var c = (a.value>b.value ? a.value/(b.value+0.000000001) : b.value/(a.value+0.00000001));
                    if (c><% =GetSliderMaxReal %>) { dxDialog("<% = JS_SafeString(MsgWrongNumber) %>", true, "SetEditFocus('" + gpw_id + "', " + (b.value==1 ? 1 : 0) + ");"); return false; }
                }<% End if %>
            }
            if (isUndefined(gpw_id)) has_undef = true;
            //if (isUndefined(gpw_id) && !(ConfirmMissingJudgment())) return false;
        }
        if ((has_undef) && !(ConfirmMissingJudgment())) return false;
        return true;
    }

    function ChangeImage(img, val)
    {
        if ((img)) img.src=val.src;
    }

    function SwitchHelpTooltip(ctrl_id, pos)
    {
        var t = $find("<% =tooltipLegend.ClientID %>");
        if ((t))
        {
           t.set_targetControlID("<% =IIf(ShowFloatLegend, "", "ctrl_id")%>");
           t.set_position(pos);
           t.show();
        }
    }

    function SetFocusAfterFlash(act_id, id)
    {
        if (id>=0)
        {
            SetFocus(act_id, 0);
            var s = "SetFocus(" + act_id +",";
            setTimeout(s+"0);", 50);
            setTimeout(s+"4);", 100);
            setTimeout(s+"0);", 250);
            setTimeout(s+"4);", 400);
            setTimeout(s+"0);", 550);
            setTimeout("SetFocus(" + id +",1);", 600);
        }
        else
        {
            SetFocus(act_id, 1);
        }
    }

    function SetFocus(id, focus, full_task)
    {
        var do_scroll = (focus == 1);
        if (focus_active_id == id && focus!=0 && focus!=4) return false;
        if ((focus==1) && (focus_active_id>=0)) { SetFocus(focus_active_id, 0); }
        var r = $get("row" + id);
        if ((r))
        {
            var clr = (focus == 1 ? "#ffffaa" : (focus == 0 || focus==-1 ? "#ffffff" : (focus==2 ? "#f5f5fa" : "#ffff66")));
            for (var i=0; i<r.cells.length; i++) r.cells[i].style.background = clr;
            if (focus==1) focus_active_id = id;
            var framed = <% =iif(ShowFramedInfodocs, "1", "0") %>;
            if (framed && (focus==1))
            {
                <% If Not isSecondNodeMode Then %>LoadInfodocFrame("nodeleft", LInfodocTitles[id], LInfodocURLs[id], LInfodocEditURLs[id]);
                LoadInfodocFrame("noderight", RInfodocTitles[id], RInfodocURLs[id], RInfodocEditURLs[id]);
                <% if ShowWRTInfodocs Then %>LoadInfodocFrame("wrtleft", LInfodocWRTTitles[id], LWRTInfodocURLs[id], LWRTInfodocEditURLs[id]);
                LoadInfodocFrame("wrtright", RInfodocWRTTitles[id], RWRTInfodocURLs[id], RWRTInfodocEditURLs[id]);<% End If %><% End If %>
                var w =eval("theForm.warning" + id);
                if ((w)) ShowWarning((w.value=="1"));
            }

            if (focus == 1) {
                var dl = $("#pw_l" + id);
                var dr = $("#pw_r" + id);
                if ((dl) && (dl.length) && (dr) && (dr.length)) {
                    var vs = dl.text() + " versus " + dr.text();
                    if (typeof full_task != "undefined" || (full_task)) setObjectName(vs); else setObjectName(vs, vs);
                }
            }

            var sc = $get("imgScale" + id );
            var sc_help = $get("imgScaleHelp" + id);
            if ((sc) && (sc_help))
            {
                if (focus==0 && focus_active_id == id) { sc.src = img_blank; sc_help.src = img_blank; focus_active_id = -1;  }
                if (focus==1) { sc.src = img_Scale.src;  <% If Not ShowFloatLegend Then %>sc_help.src = img_ScaleHelp.src;<% End If %> }
                }

                if (focus==1 && do_scroll)
                {
                    var d = $get("row" + id);
                    var dlst = $get("divList");
                    if ((d) && (dlst))
                    {
                        if ((d.offsetTop + 2*d.clientHeight)>(dlst.clientHeight + dlst.scrollTop))
                        {
                            var n = d.offsetTop+(2*d.clientHeight)-dlst.clientHeight;
                            var k=6;
                            for (var i=0; i<k; i++) setTimeout("$get('divList').scrollTop=" + Math.round(dlst.scrollTop+(i+1)*(n-dlst.scrollTop)/k)+";", i*25+10);
                            //                dlst.scrollTop=d.offsetTop+(2*d.clientHeight)-dlst.clientHeight;
                        }
                        if  (id>0 && d.offsetTop>0 && dlst.scrollTop>0 && (d.offsetTop - d.clientHeight)<dlst.scrollTop)
                        {
                            var scr = $get("row" + (id-1));
                            if ((scr)) scr.scrollIntoView();
                        }
                    }
                }
            }
            return false;
        }

    function SetPW(val)
    {
        if (focus_active_id>=0) SetPairwise(focus_active_id, val-1);
        return false;
    }
        
    function InitComments()
    {
        for (var i=0; i<ImagesComments.length; i++)
        {
                var c = eval("theForm.Comment" + i);
                var im = $get(ImagesComments[i]);
                if ((c) && (im)) im.title = (c.value == "" ? "[Edit comment]" : c.value);
        }
    }

    function LoadInfodocFrame(name, title, url, editurl)
    {
        var f = $get(name);
        if ((f))
        {
            if ((f.contentWindow) && (f.contentWindow.document.body)) f.contentWindow.document.body.innerHTML = "<div style='<% =String.Format(FrameLoadingStyle, ImagePath)%>'>&nbsp</div>";
            if (url!="") url = url + "&r2=" + Math.random();
            setTimeout("var f = $get('" + name + "'); f.src= \"" + url + "\";" , 200);
            var t = $get("lbl_" + name + "<% =_HIDDEN %>");
            if ((t)) t.innerHTML = title;
            t = $get("lbl_" + name + "<% =_VISIBLE %>");
            if ((t)) t.innerHTML = title;
            var fnc = new Function("onclick", (editurl=="" ? (url="" ? "" : "open_infodoc('" + url + "'); return false;") : editurl));
            var l = $get("lnk_" + name + "<% =_HIDDEN %>");
            if ((l)) l.onclick=fnc;
            l = $get("lnk_" + name + "<% =_VISIBLE %>");
            if ((l)) l.onclick =fnc;
        }
    }

    function showFloatLegend() {
        var l = $("#<% =divLegend.ClientID %>").detach().appendTo("#divLegendFlow");
        $("#divLegendFlow").show();
        alignFloatLegend();
    }
    
    function alignFloatLegend() {
        //var l = $("#divList").width();
        //var td = $("#pMain").width();
        //$("#tblLegend").css("max-width", "");
        //var w = $("#tblLegend").width()+24;
        //var p = ((w>100 && td>0 && l>0 && (td-l-2*w)>32) ? w+8 : 0);
        //$("#divList").css("margin-left", p);
        //$("#tblLegend").css("max-width", (td-l-w-p)>100 ? "" : 230);
    }

        <% if CanEditInfodocs Then %> 

        //function editText(title, val, callback) {
        //    var v = prompt(title, val);
        //    onRichEditorRefresh(val=="", "undefined", callback);
        //}

        function onRichEditorRefresh(empty, infodoc, callback)
        {
            if ((callback) && (callback[0]) && callback[0]!="")
            {
                var framed = <% =iif(ShowFramedInfodocs, "1", "0") %>;
                var n = callback[0];
                var t = null;
                var id;

                if (framed) 
                {
                    var l = -1;
                    if (callback[0].substr(0,8)=="nodeleft")
                    {
                      l=8;
                      wrt = false;
                    }
                    if (callback[0].substr(0,9)=="noderight")
                    {
                      l=9;
                      wrt = false;
                    }
                    if (callback[0].substr(0,7)=="wrtleft")
                    {
                      l=7;
                      wrt = true;
                    }
                    if (callback[0].substr(0,8)=="wrtright")
                    {
                      l=8;
                      wrt = true;
                    }
                    if (l>0) {
                        n = callback[0].substr(0, l);
                        id = 1* callback[0].substr(l);
                        callback[0] = n;
//                        switch (callback[0].substr(0,5)) {
//                            case "nodel":
//                                LInfodocURLs[id] = callback[1];
//                                break;
//                            case "noder":
//                                RInfodocURLs[id] = callback[1];
//                                break;
//                            case "wrtle":
//                                LWRTInfodocURLs[id] = callback[1];
//                                break;
//                            case "wrtri":
//                                RWRTInfodocURLs[id] = callback[1];
//                                break;
//                        }
                    }
                }
                else
                {
                    if (callback[0]=="node") { n="<% =imgCaptionInfodoc.ClientID %>"; t = "<% =tooltipGoal.ClientID %>"; }
                    if (callback[0].substr(0,3)=="alt")
                    {
                      id = 1* callback[0].substr(3);
                      n = ImagesInfodoc[id];
                      t = TooltipsInfodoc[id];
                    }
                    if (callback[0].substr(0,3)=="wrt")
                    {
                      id = 1* callback[0].substr(3);
                      n = ImagesWRTInfodoc[id];
                      t = TooltipsWRT[id];
                    }
                }
                var f = null;
                if ((n)) f = $get(n);
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
                        setTimeout("var f = $get('" + callback[0] + "'); f.src= \"" + src + "\";" , 200);
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

    document.cookie = "mp_save=;mpwrt_save=;";

<% End If %>

  var list_loaded = false;
  function InitList()
  {
      if (!list_loaded) {
          var r_a = $("#tblList tr:first td:first");
          var r_b = $("#tblList tr:first td:last");
          if ((r_a) && (r_a.length) && (r_b) && (r_b.length)) {
              var wa = r_a.width();
              var wb = r_b.width();
              if (wa > 0 && wb > 0) {
                  var w = Math.round((wa + wb + 16) / 2);
                  r_a.width(Math.round(w));
                  r_b.width(Math.round(w));
                  list_loaded = true;
              }
          }
          $("#divLoading").hide();
      }
      if ($("#tblList").length) {
          if ($("#divList").css("overflow-y") != "auto") {
              //$("#tblList").width(w+16);
              setTimeout('$("#divList").css("overflow-y", "auto");', 30);
          }
          if ($("#divList").length>0) {
              $("#divList").height(60);
              var p = replaceString("px", "", $("#divList").css("margin-left"));
              p = 32 + (p*1);
              //var w = $("#tblList").width();
              $("#divList").height(Math.round($get("tdScrollableList").clientHeight)-24);
              //$("#divList").width(1200).width(w+p);
          }
      }
  }

  function ResizeList()
  {
    setTimeout('InitList();', 50);
    setTimeout('InitList();', 300);
    <% If ShowFloatLegend Then%> setTimeout('alignFloatLegend();', 500); <% End If%>
  }

  window.onresize=ResizeList;
//-->    
</script>
<% If ShowFramedInfodocs Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%>
<% If PWType = PairwiseType.ptVerbal Then%>
<telerik:radtooltip runat="server" id="tooltipLegend" HideEvent="LeaveTargetAndToolTip" Skin="WebBlue" SkinID="tooltipInfo" RelativeTo="Element" OffsetY="-8" OffsetX="-6" Position="TopLeft" EnableViewState="false" IsClientID="true" CssClass="panel" Title="<span style='margin:5px 0px 0px 1ex'></span>">
  <div style="text-align:center; margin: 4px" runat="server" id="divLegend" visible="false"></div>
</telerik:radtooltip><% =PWImageMap%>
<% End if %>

<div align='center' id="pMain" class="whole"><table border="0" cellpadding="0" cellspacing="0" style="width:100%; height:97%; padding:0px; margin:0px;"><tr><td style="height:1%" colspan="2" align="center">
    <% If ShowFramedInfodocs Then %>
        <div style="text-align:center; display:inline-block;margin:0px auto;"><table border="0" cellpadding="2" cellspacing="0">
            <tr valign="top">
                <td><EC:FramedInfodoc runat="server" ID="frmInfodocNodeLeft"/></td><td<% If ShowWRTInfodocs Then %> rowspan="2"<% End If %>><EC:FramedInfodoc runat="server" ID="frmInfodocGoal"/></td><td><EC:FramedInfodoc runat="server" ID="frmInfodocNodeRight"/></td>
            </tr>
            <% If ShowWRTInfodocs Then%><tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocWRTLeft"/></td><td><EC:FramedInfodoc runat="server" ID="frmInfodocWRTRight"/></td></tr><% End If%>
        </table></div>
    <% Else %>
        <div style="text-align:center; padding:0.5em 0px" class="text"><h5 style="padding:0px; margin:3px;"><asp:Image runat="server" ID="imgCaptionInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipGoal" Visible="false" TargetControlID="imgCaptionInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>&nbsp;<asp:Label ID="lblCaption" runat="server"/></h5></div>
    <% End If%><% If PWType = PairwiseType.ptVerbal Then%><asp:Label runat="server" ID="lblMessage"/><% End If %>
    </td></tr>
    
    <tr valign="top"><td id='tdScrollableList' align="<% =IIf(ShowFloatLegend AndAlso PWType <> PairwiseType.ptGraphical, "right", "center")%>"><div style='height:98%; border:0px dashed gray; text-align:<% =IIf(ShowFloatLegend AndAlso PWType <> PairwiseType.ptGraphical, "right", "center")%>; padding:0px; min-height:100px; overflow-y:auto; display:inline-block;' id='divList'><p id="divLoading" style="height:2em; padding-bottom:1em;" class="text gray">Loading...</p>
        <asp:Repeater ID="rptPW" runat="server" OnItemDataBound="rptPW_ItemDataBound">

        <HeaderTemplate>
            <table border="0" cellpadding="4" cellspacing="0" id="tblList" style="margin:4px">
        </HeaderTemplate>
        
        <FooterTemplate>
            </table>
        </FooterTemplate>
        
        <ItemTemplate>
          <tr class="text tbl_row" valign="middle" id="row<%#DataBinder.Eval(Container, "ItemIndex")%>" onmouseover="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', 2);" onmouseout="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', -1);" onclick="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', 1);">
            <td style="padding:3px 1ex" valign="middle"><input type="hidden" name="value<%#DataBinder.Eval(Container.DataItem, "ID")%>" value="<%#JS_SafeNumber(GetInitValue(CType(DataBinder.GetDataItem(Container), clsPairwiseLine)))%>"><input type="hidden" name="adv<%#DataBinder.Eval(Container.DataItem, "ID")%>" value="<%#JS_SafeNumber(CType(DataBinder.GetDataItem(Container), clsPairwiseLine).Advantage)%>"><input type="hidden" name="warning<%#DataBinder.Eval(Container.DataItem, "ID")%>" value="<%#isWarning(CType(DataBinder.GetDataItem(Container), clsPairwiseLine))%>"><input type="hidden" name="id_left<%#DataBinder.Eval(Container.DataItem, "ID")%>" value="<%#DataBinder.Eval(Container.DataItem, "NodeID_Left")%>" /><input type="hidden" name="id_right<%#DataBinder.Eval(Container.DataItem, "ID")%>" value="<%#DataBinder.Eval(Container.DataItem, "NodeID_Right")%>" /><div class='object_left' style='padding:1<%#iif(CStr(DataBinder.Eval(Container.DataItem, "LeftNodeComment"))<>"" OrElse CDbl(DataBinder.Eval(Container.DataItem, "KnownLikelihoodA"))>0, "ex", "em")%> 1ex;'><%#GetNodeComment(CStr(DataBinder.Eval(Container.DataItem, "ScenarioNameLeft")))%><asp:Image runat="server" ID="imgLeftInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipLeftInfodoc" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>&nbsp;<b><span id="pw_l<%#DataBinder.Eval(Container, "ItemIndex")%>"><%#DataBinder.Eval(Container.DataItem, "LeftNode")%></span></b>&nbsp;<asp:Image runat="server" ID="imgLeftWRT" SkinID="InfoReadme" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipLeftWRT" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/><%#GetKnownLikelihood(CDbl(DataBinder.Eval(Container.DataItem, "KnownLikelihoodA")))%><%#GetNodeComment(CStr(DataBinder.Eval(Container.DataItem, "LeftNodeComment")))%></div></td>
            <td runat="server" id="tdComment"><asp:Image runat="server" id="imgComment" SkinID="NoteDisabledIcon" CssClass="aslink" Visible="false"/><telerik:radtooltip runat="server" id="ttEditComment" Skin="WebBlue" SkinID="tooltipInfo" RelativeTo="Element" Position="BottomRight" OffsetX="-8" OffsetY="-7" ShowEvent="onClick" HideEvent="ManualClose" CssClass="panel" Visible="false">
                <div class="text" style="font-weight:bold;"><%=lblCommentTitle%>:</div>
                <textarea name="Comment<%#DataBinder.Eval(Container.DataItem, "ID")%>" rows="7" cols="40" class="textarea" style="margin:1ex 0px"><%#DataBinder.Eval(Container.DataItem, "Comment")%></textarea>
                <div style="text-align:right"><asp:Button runat="server" ID="btnSave" UseSubmitBehavior="false" CssClass="button button_small"/>&nbsp;<asp:Button runat="server" ID="btnClose" UseSubmitBehavior="false" CssClass="button button_small"/></div>
                </telerik:radtooltip></div>
            </td>
            <td align="center" runat="server" id="tdContentGPW" visible="false" style="padding:2px; white-space:nowrap;"><div style="width:460px; padding-top:12px;" id="Slider<%#DataBinder.Eval(Container.DataItem, "ID")%>"><div style="text-align:center;" class='text small gray'>Loading…</div></div></td></td>
            <td align="center" runat="server" id="tdContentVPW" visible="false" style="padding:0px 6px 4px 6px; white-space:nowrap" class="text">Loading…</td>
            <td style="padding:3px 1ex" valign="middle"><div class='object_right' style='padding:1<%#iif(CStr(DataBinder.Eval(Container.DataItem, "RightNodeComment"))<>"" OrElse CDbl(DataBinder.Eval(Container.DataItem, "KnownLikelihoodB"))>0, "ex", "em")%> 1ex;'><%#GetNodeComment(CStr(DataBinder.Eval(Container.DataItem, "ScenarioNameRight")))%><asp:Image runat="server" ID="imgRightInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipRightInfodoc" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>&nbsp;<b><span id="pw_r<%#DataBinder.Eval(Container, "ItemIndex")%>"><%#DataBinder.Eval(Container.DataItem, "RightNode")%></span></b>&nbsp;<asp:Image runat="server" ID="imgRightWRT" SkinID="InfoReadme" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipRightWRT" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/><%#GetKnownLikelihood(CDbl(DataBinder.Eval(Container.DataItem, "KnownLikelihoodB")))%><%#GetNodeComment(CStr(DataBinder.Eval(Container.DataItem, "RightNodeComment")))%></div></td>
         </tr>
        </ItemTemplate>
        
        </asp:Repeater>
    </div></td><td><div id="divLegendFlow" style="padding-left:12px; display:none;"></div></td></tr>
</table></div>