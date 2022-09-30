<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlMultiDirectInputs" Codebehind="ctrlMultiDirectInputs.ascx.vb" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<script type="text/javascript" language="javascript" src='<% =sRootPath %>Scripts/sliderbars.js'></script>
<script type="text/javascript"><!--

    var img_path        = '<% =ImagePath %>';
    
    var img_infodoc1	= new Image; img_infodoc1.src    = img_path + '<% =ImageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src    = img_path + '<% =ImageInfodocEmpty %>';
    var img_WRTinfodoc1	= new Image; img_WRTinfodoc1.src = img_path + '<% =ImageWRTInfodoc %>';
    var img_WRTinfodoc2	= new Image; img_WRTinfodoc2.src = img_path + '<% =ImageWRTInfodocEmpty %>';

    var img_delete = new Image; img_delete.src = img_path + 'delete_tiny.gif';
    var img_delete_ = new Image; img_delete_.src = img_path + 'nodelete_tiny.gif';

    _img_slider_images_path = img_path;
    _img_slider_position.src = _img_slider_images_path + "slider_position.gif";
    _img_slider_handler.src = _img_slider_images_path + "slider_handler.gif";
    _img_slider_handler2.src = _img_slider_images_path + "slider_handler2.gif";
    _img_slider_blank.src = _img_slider_images_path + 'blank.gif';
    
    var img_note_empty = "<% =ImageCommentEmpty %>";
    var img_note_exists = "<% =ImageCommentExists %>";
    
    var sliders = new Array();

    var ids             = [<% =SliderIDs %>];
    var inputs          = [<% =InputIDs %>];
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
    var do_refresh = 1;

    var slider_direct_max = 1000;
    var slider_is_pressed = 0;
    var slider_is_manual = 0;

    var show_comment = false;

    function isUndefined(idx)
    {
        return (eval("theForm.DI" + idx + ".value") == "");
    }

    function HasUndefined()
    {
        if (theForm.DICount)
            for (var i=0; i<theForm.DICount.value; i++)
                if (isUndefined(i)) return true;
        return false;
    }

    function CheckUndefined(idx)
    {
        var i = $get("delete" + idx);
        if ((i)) i.src = (!isUndefined(idx)) ? img_delete.src : img_delete_.src;
    }


    function Reset(idx)
    {
        if (focus!=idx) SetFocus(idx, 1);
        var obj = eval("theForm.DI" + idx);
        if ((obj))
        {
            obj.value = "";
            onValueChange("DI" + idx);
        }
    }

    function PrepareValue(value)
    {
        value = value.replace(",", ".");
        if (value=="0.") value="0";
        if (value=="1.") value="1";
        return value;
    }
    
    function CheckValue(idx)
    {
        var obj = eval("theForm.DI" + idx);
        var value = ((obj) ? obj.value : '');
        value = PrepareValue(value);
        if (value=="") return true; // D0317
        if ((value*1)!=value) { dxDialog('<% =String.Format(JS_SafeString(msgWrongNumber), " ""' + value +'"" ") %>', true, ((obj) ? "theForm.DI" + idx + ".focus();" : "")); return false; }
        value = value*1;
        if (value<0 || value>1) { dxDialog('<% =JS_SafeString(msgWrongNumberRange) %>', true, ((obj) ? "theForm.DI" + idx + ".focus();" : "")); return false; }
        return true;
    }

    function Changed()
    {
        <% if changeFlagID<>"" Then %>if (theForm.<% =changeFlagID %>) theForm.<% =changeFlagID %>.value=1;<% end if %>
        <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>
        if (!HasUndefined()) setTimeout("FlashNextButton();", 1000);
    }

    function onShowComment(sender, eventArgs)
    {
        var c = eval("theForm.Comment" + focus_active_id);
        if ((c)) { c.focus(); show_comment = true; }
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
            if (t.isVisible) { t.hide(); show_comment = false; } else t.show();
        }
    }
    
    function CheckDIForm()
    {
        if (HasUndefined() && !ConfirmMissingJudgment()) return false;
        if (theForm.DICount)
            for (var i=0; i<theForm.DICount.value; i++)
                if (!CheckValue(i)) return false;
        return true;    
    }
    
    function GetIdx(arr, id)
    {
        for (var i=0; i<arr.length; i++)
          if (arr[i]==id) return i;
        return -1;  
    }
  
    function onValueChange(input_id)
    {
        if (input_id!="")
        {
            var input = eval("theForm." + input_id);
            if ((input))
            {
                var value = PrepareValue(input.value);
                var valid = 1;
                if (value!="") valid = ((value*1) == value) && ((value*1)>=0) && ((value*1)<=1);
                if ((input.style)) input.style.color = (valid ? "#000000" : "#990000");
                var idx = GetIdx(inputs, input_id);
                if (idx>=0)
                {
                    if (focus!=idx) SetFocus(idx, 1);

                    var slider = document.getElementById(slider_prefix + idx);
                    if ((slider))
                    {
                        slider_is_manual = 1;
                        slider.val = (valid && value!=="" ? Math.round(value * slider_direct_max) : -1);
                        drawSliderByVal(slider);
                        onChangeSliderDirect(slider.val, slider.num);
                        slider_is_manual = 0;
                    }
                }
                CheckUndefined(idx);
            }
        }
        Changed();
    }

    function InitComments()
    {
        if (ImagesComments.count>0) {
            for (var i=0; i<ids.length; i++)
            {
                    var c = eval("theForm.Comment" + i);
                    var im = $get(ImagesComments[i]);
                    if ((c) && (im)) im.title = (c.value == "" ? "[Edit comment]" : c.value);
            }
        }
    }

    function InitSliders()
    {
        slider_is_manual = 1;
        for (var i=0; i<ids.length; i++)
        {
            var slider_id = ids[i];

            var val = eval("theForm.DI" + slider_id + ".value");
            if (val!="") val = val*1;
            
            var slider_direct = new Object();
            slider_direct.num = slider_id;
            slider_direct.min = 0;
            slider_direct.max = slider_direct_max;
            slider_direct.realWidth = <% =SliderWidth %>;
            slider_direct.masked = 0;
            slider_direct.readonly = 0;
            slider_direct.handler = 1;

            slider_direct.val = (val=="" ? -1 : Math.round(val*slider_direct_max));
            slider_direct.onchange = onChangeSliderDirect;
            slider_direct.color = "#0058a3";
            
//            tbl.rows[r].cells[2].innerHTML = SyncCreateSliderWithDrags(slider_direct, slider_id, 150, "SliderDirectChange", "ResetSliderDirect", "");

            sliders[slider_id] = slider_direct;
            sliderInitByName(slider_prefix + slider_direct.num, sliders[slider_id]);
        }
        slider_is_manual = 0;
    }

    function ShowDecNum(val)
    {
        if (val!=="") return Math.round(<% =_precCoeff/10 %>*val)/<% =_precCoeff/10 %>; else return "";
    }

    function onChangeSliderDirect(val, slider_id) {
        var s = document.getElementById(slider_prefix + slider_id);
        if ((s) && !slider_is_manual) {
          var e = eval("theForm.DI" +  slider_id);
          if ((e) && !slider_is_manual) {
            if (s.val<s.min) 
            {
                e.value = "";
            }
            else {
                var value = s.val / slider_direct_max;
                e.value = ShowDecNum(value);
            }
            Changed();
            CheckUndefined(slider_id);
          }
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
    
    function SetFocus(id, focus)
    {
       id = id*1;
       focus = focus*1;
       var do_scroll = (focus == 1);
       if (focus==4) focus=1;
       if (focus_active_id == id && focus!=0) return false;
       if ((focus==1) && (focus_active_id>=0)) { SetFocus(focus_active_id, 0); }
       var r = $get("row" + id);
       if ((r))
       {
            var clr = (focus == 1 ? "#ffffaa" : (focus == 0 || focus==-1 ? (r.className.indexOf("_alt")>0 ? "#fffaf0" : "#ffffff")  : (focus==2 ? "#f5f5ff" : "#ffffe5")));
            for (var i=0; i<r.cells.length; i++) if ((r.cells[i]) && (r.cells[i].style)) r.cells[i].style.background = clr;
            if (focus==1) focus_active_id = id;
            var framed = <% =iif(ShowFramedInfodocs, "1", "0") %>;
            if (framed && (focus==1))
            {
                LoadInfodocFrame("alt", InfodocTitles[id], InfodocURLs[id], InfodocEditURLs[id]);
                LoadInfodocFrame("wrt", InfodocWRTTitles[id], WRTInfodocURLs[id], WRTInfodocEditURLs[id]);
                var obj = eval("theForm.DI" + id);
                if ((obj)) obj.focus();
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
            setTimeout("var f = $get('" + name + "'); f.src= \"" + url + (url=="" ? "" : "&p=" + Math.round(10000*Math.random())) + "\";" , 200);
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

    function RatingHotkeys(event)
    {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event)
        {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            var d = eval("theForm.DI" + focus_active_id);
            if ((((code==40 || code==38 || code==13) && event.ctrlKey) || (code==13)) && (d))
            {
                if (!CheckValue(focus_active_id)) return false;
                var ofs = (code == 38 ? -1 : +1);
                if (ofs+1*focus_active_id>=0 && ofs+1*focus_active_id<=ids.length-1)
                { 
                    setTimeout("SetFocus(" + (ofs+1*focus_active_id) + ", 1); theForm.DI" + (ofs+1*focus_active_id) + ".focus();", 75); 
                    return false;
                }
            }
            if ((!show_comment) && (code==48 || code==49 || code==188 || code==96 || code==97 || code==110 || code==190))
            {
              if ((d) && (d.value==""))
              {
                var c = "";
                if (code==48 || code==96) c = "0";
                if (code==49 || code==97) c = "1";
                if (code==188) c = "0,";
                if (code==190 || code==110) c = "0.";
                d.focus();
                d.value = c;
                onValueChange("DI" + focus_active_id);
                return false;
              }
            }
        }
        return true;
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
                
                var f = $get(n);
                if ((f))
                {
                    var src = callback[1];
	            if (framed && src == "" && location.href!=f.src) src = f.src;
                  //if (src == "" && (callback[1]) && callback[1]!="") src = callback[1];                    
                    if (src!="") src = src + "&p=" + Math.random();
                    if (empty==1) src = "";
                    
                    if (framed)
                    {
                        if ((f.contentWindow)) f.contentWindow.document.body.innerHTML = "<div style='<% =String.Format(FrameLoadingStyle, ImagePath)%>'>&nbsp</div>";
                        setTimeout("var f = $get('" + callback[0] + "'); f.src= \"" + src + (src=="" ? "" : "&p=" + Math.round(10000*Math.random())) + "\";" , 200);
                    }   
                    else
                    {
                        if (callback[0].substr(0,3)=="wrt")
                        {
                           f.src = (empty==1 ? img_WRTinfodoc2.src : img_WRTinfodoc1.src);
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

    document.cookie = "md_save=;";

<% End If %>    
<% If Request.Browser.Browser.ToLower.Contains("firefox") Then %>
  function InitList(lst_id, parent_id)
  {
    var td = $get(parent_id);
    var lst = $get(lst_id);
    if ((td) && (lst)) { var old_h = lst.style.height; lst.style.height="1px"; var h = td.clientHeight+"px"; if (old_h!=h) { old_h = h; /* alert(h); */ } lst.style.height = old_h; }
  }

  function ResizeList()
  {
    setTimeout('InitList("divList", "tdScrollableList");', 20);
  }

  window.onresize=ResizeList;
  window.onclick=ResizeList;
<% End if %>
//-->    
</script>
<% If ShowFramedInfodocs AndAlso frmInfodocGoal IsNot Nothing Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%>
<table border="0" cellpadding="0" cellspacing="0" class="anytimeMainTable"><tr><td align="center">
<% If ShowFramedInfodocs AndAlso frmInfodocGoal IsNot Nothing Then %><table border="0" cellpadding="2" cellspacing="0"><tr valign="top">
<td runat="server" id="tdLeft"></td><td runat="server" id="tdCenter"></td><td><EC:FramedInfodoc runat="server" ID="frmInfodocWRT"/></td>
</tr></table><% Else %><div style="text-align:center; padding:0.5em 0px" class="text"><h5><asp:Image runat="server" ID="imgCaptionInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipGoal" Visible="false" TargetControlID="imgCaptionInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>&nbsp;<asp:Label ID="lblCaption" runat="server"></asp:Label></h5></div><% End If%>
</td></tr>
<tr><td valign="top" align="center" style="height:99%" id="tdScrollableList"><img src='<% =ImagePath %>blank.gif' width=1 height=120 style='float:left'><div style='height:100%; overflow:auto; text-align:center; padding-right:2px;' id='divList'>
<input type="hidden" name="DICount" value="<%=GetDICount%>"/><asp:Repeater ID="rptDI" runat="server" OnItemDataBound="rptDI_ItemDataBound">
<HeaderTemplate>
  <table border="0" cellpadding="2" cellspacing="0" style="margin:0px auto; width:<% =600-CInt(iif(ShowSlider, 0, SliderWidth)) %>px;font-size:1pt">
</HeaderTemplate>
<FooterTemplate>
  </table>
</FooterTemplate>
<ItemTemplate>
  <tr class="text tbl_row<%#iif(Cint(DataBinder.Eval(Container, "ItemIndex")) mod 2 = 1, "", "_alt") %>" valign="middle" align="left" id="row<%#DataBinder.Eval(Container, "ItemIndex")%>" onmouseover_="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', 2);" onmouseout_="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', -1);" onclick="SetFocus('<%#DataBinder.Eval(Container, "ItemIndex")%>', 4);">
    <td style="padding:3px 0px">&nbsp;<asp:Image runat="server" id="imgComment" SkinID="NoteDisabledIcon" CssClass="aslink" Visible="false"/><telerik:radtooltip runat="server" id="ttEditComment" Skin="WebBlue" SkinID="tooltipInfo" RelativeTo="Element" Position="BottomRight" OffsetX="-8" OffsetY="-7" ShowEvent="onClick" HideEvent="ManualClose" CssClass="panel" Visible="false" OnClientShow="onShowComment">
<div class="text" style="font-weight:bold;"><%=lblCommentTitle%>:</div>
<textarea name="Comment<%#DataBinder.Eval(Container.DataItem, "ID")%>" rows="8" cols="45" class="textarea" style="margin:1ex 0px"><%#DataBinder.Eval(Container.DataItem, "Comment")%></textarea>
<div style="text-align:right"><asp:Button runat="server" ID="btnSave" UseSubmitBehavior="false" CssClass="button button_small"/></div>
</telerik:radtooltip>&nbsp;<asp:Image runat="server" ID="imgInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipInfodoc" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>&nbsp;<b><%#DataBinder.Eval(Container.DataItem, "Title")%></b></td>
    <td style="width:4em; padding:4px 0px; white-space:nowrap"><input type="text" class="input" style="width:5em; text-align:right; padding-right:3px" name="DI<%#DataBinder.Eval(Container.DataItem, "ID")%>" value="<%#GetDDValue(CSng(DataBinder.Eval(Container.DataItem, "DirectData")))%>" onkeyup="onValueChange('DI<%#DataBinder.Eval(Container.DataItem, "ID")%>');" tabindex="<%#CInt(DataBinder.Eval(Container, "ItemIndex"))+100 %>" onmousedown='focusOnClick(this);'/></td>
    <td runat="server" id="tdSlider" style="padding:4px 0px; white-space:nowrap; margin-left:4px;" align='center'><div class='sa_slider' style='width:<% =SliderWidth %>px; height:14px;' id='slider<%#DataBinder.Eval(Container.DataItem, "ID")%>'></div></td>
    <td style="width:16px; white-space:nowrap; padding-left:4px" class="small"><nobr><a href='' onclick='Reset("<%#DataBinder.Eval(Container.DataItem, "ID")%>"); return false;'><img src='<% =ImagePath %><%#iif(GetDDValue(CSng(DataBinder.Eval(Container.DataItem, "DirectData")))="", "nodelete_tiny.gif", "delete_tiny.gif")%>' width='10' height='10' border='0' title='Reset' id='delete<%#DataBinder.Eval(Container.DataItem, "ID")%>' style='margin-right:1ex;'></a><asp:Image runat="server" ID="imgWRT" SkinID="InfoReadme" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipWRT" Visible="false" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/></nobr></td>
  </tr>
</ItemTemplate>
</asp:Repeater><% =IIf(rptDI Is Nothing OrElse rptDI.Items.Count = 0, "<h4 style='padding:5em;'>No data to display</h4>", "")%>
</div></td></tr>
</table>