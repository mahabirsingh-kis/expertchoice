<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlDirectData" Codebehind="ctrlDirectData.ascx.vb" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<script type="text/javascript" language="javascript" src='<% =sRootPath %>Scripts/sliderbars.js'></script>
<script type="text/javascript"><!--

    var img_path        = '<% =ImagePath %>';
    
    var img_infodoc1	= new Image; img_infodoc1.src   = img_path + '<% =ImageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src   = img_path + '<% =ImageInfodocEmpty %>';
    var img_WRTinfodoc1	= new Image; img_WRTinfodoc1.src = img_path + '<% =ImageWRTInfodoc %>';
    var img_WRTinfodoc2	= new Image; img_WRTinfodoc2.src = img_path + '<% =ImageWRTInfodocEmpty %>';
    var img_delete      = new Image; img_delete.src = img_path + 'delete_tiny.gif';
    var img_delete_     = new Image; img_delete_.src = img_path + 'nodelete_tiny.gif';
    
    _img_slider_images_path = img_path;
    _img_slider_position.src = _img_slider_images_path + "slider_position.gif";
    _img_slider_handler.src = _img_slider_images_path + "slider_handler.gif";
    _img_slider_handler2.src = _img_slider_images_path + "slider_handler2.gif";
    _img_slider_blank.src = _img_slider_images_path + 'blank.gif';


    var value_orig      = '<% =TrimEndZeroes(ValueStr) %>';
    var is_focused = 0;

    var slider = null;
    var slider_id = "1";

    var slider_direct_max = 1000;
    var slider_is_pressed = 0;
    var slider_is_manual = 0;

    function isChanged()
    {
        updateImg();
        <% if changeFlagID<>"" Then %>if (theForm.<% =changeFlagID %>) theForm.<% =changeFlagID %>.value=1;<% end if %>
    }

    function PrepareValue(value)
    {
        value = value.replace(",", ".");
        if (value=="0.") value="0";
        if (value=="1.") value="1";
        return value;
    }

    function updateImg() {
        var val = (theForm.DDValue) ? theForm.DDValue.value : "";
        var img = $get("img_erase");
        if ((img)) img.src = (val!="" ? img_delete.src : img_delete_.src);
    }

    function SetDDValue()
    {
        if ((theForm.DDValue) && (theForm.DDValue.value != value_orig || theForm.DDValue.value == ""))    // D1010
        {
            var value = PrepareValue(theForm.DDValue.value);
            var valid = 1;
            if (value!="") valid = ((value*1) == value) && ((value*1)>=0) && ((value*1)<=1);
            if ((theForm.DDValue.style)) theForm.DDValue.style.color = (valid ? "#000000" : "#990000");
           
            var s = document.getElementById(slider_prefix + slider_id);
            if ((s))
            {
                slider_is_manual = 1;
                s.val = (valid && value!=="" ? Math.round(value * slider_direct_max) : -1);
                drawSliderByVal(s);
                onChangeSliderDirect(s.val, s.num);
                slider_is_manual = 0;
            }
            if (theForm.DDValue.value != value_orig) isChanged();
        }    
        <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>    // D0255
        return true;
    }
    
    function SetFocus(val)
    {
        is_focused = val;
    }
    
    function CheckValue()
    {
        var obj = theForm.DDValue;
        var value = ((obj) ? obj.value.replace(",", ".") : '');
        if (value=="") return true; // D0317
        if (value=="0.") value="0";
        if (value=="1.") value="1";
        if ((value*1)!=value) { dxDialog('<% =JS_SafeString(msgWrongNumber) %>', true, ((obj) ? "theForm.DDValue.focus();" : "")); return false; }
        value = value*1;
        if (value<0 || value>1) { dxDialog('<% =JS_SafeString(msgWrongNumberRange) %>', true, ((obj) ? "theForm.DDValue.focus();" : "")); return false; }
        return true;
    }
    
    function CheckFormOnSubmit()
    {
        if (!CheckValue()) return false;
        var obj = theForm.DDValue;
        if ((obj) && (obj.value=="") && !ConfirmMissingJudgment()) return false;
<%--        if (is_focused) 
        { 
            theForm.DDValue.blur();
            <% if NextButtonClientID<>"" Then %>var btn = theForm.<% =NextButtonClientID %>;
            if ((btn) && (!btn.disabled)) btn.focus();<% End If %>
            setTimeout("theForm.<% =NextButtonClientID %>.click();", 250);
            return false;
        }    --%>
        return true;
    }
        
    function InfodocFrameLoaded(frm)
    {
        if ((frm) && (frm.style)) { frm.style.backgroundImage='none'; };
    }

    function OnToolTipShowHandler(sender, args)
    {
        if ((sender) && (sender.get_text()=="")) args.set_cancel(true);
    }

    function InitSlider()
    {
        slider_is_manual = 1;
        
        var val = theForm.DDValue.value;
        if (val!="") val = val*1;
            
        slider = new Object();
        slider.num = slider_id;
        slider.min = 0;
        slider.max = slider_direct_max;
        slider.realWidth = <% =SliderWidth %>;
        slider.masked = 0;
        slider.readonly = 0;
        slider.handler = 1;

        slider.val = (val=="" ? -1 : Math.round(val*slider_direct_max));
        slider.onchange = onChangeSliderDirect;
        slider.color = "#0058a3";
            
        sliderInitByName(slider_prefix + slider.num, slider);

        updateImg();

        slider_is_manual = 0;
    }

    function onChangeSliderDirect(val, slider_id) {
        var s = document.getElementById(slider_prefix + slider_id);
        if ((s) && !slider_is_manual) {
          var e = theForm.DDValue;
          if ((e) && !slider_is_manual) {
            if (s.val<s.min) 
            {
                e.value = "";
            }
            else
            {
                var value = s.val/slider_direct_max;
                e.value = ShowDecNum(value);
            }
            isChanged();
            <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>    // D6708
//            CheckUndefined(slider_id);
          }
       }
    }

    function ShowDecNum(val)
    {
        if (val!=="") return Math.round(<% =_precCoeff %>*val)/<% =_precCoeff %>; else return "";
    }
       
<% if CanEditInfodocs Then %> 
        function onRichEditorRefresh(empty, infodoc, callback)
        {
            if ((callback) && (callback[0]) && callback[0]!="")
            {
                var framed = <% =iif(ShowFramedInfodocs, "1", "0") %>;
                var n = callback[0];
                var t;
                if (!framed) 
                {
                    switch (callback[0])
                    {
                      case "node": { n="<% =imgCaptionInfodoc.ClientID %>"; t = "<% =tooltipGoal.ClientID %>"; break; }
                      case "alt": { n="<% =imgAltInfodoc.ClientID %>"; t = "<% =tooltipAltInfodoc.ClientID %>"; break; }
                      case "wrt": { n="<% =imgWRTInfodoc.ClientID %>"; t = "<% =tooltipWRTInfodoc.ClientID %>"; break; }
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
                    setTimeout("var f = $get('" + callback[0] + "'); f.src= \"" + src + "\";" , 200);
                  }   
                  else
                  {
                    if (callback[0]=="wrt")
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

    document.cookie = "di_node_save=;di_alt_save=;di_wrt_save=;";

<% End If %>    
//-->    
</script>
<% If ShowFramedInfodocs Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%>

<h4 style="margin-top:1ex" runat="server" id="tdCaption"><asp:Image runat="server" ID="imgCaptionInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipGoal" Visible="false" TargetControlID="imgCaptionInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/><asp:Image runat="server" ID="imgAltInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipAltInfodoc" Visible="false" TargetControlID="imgAltInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>&nbsp;<asp:Label id="lblCaption" runat="server"></asp:Label>&nbsp;<asp:Image runat="server" ID="imgWRTInfodoc" SkinID="InfoReadme" Visible="false"/><telerik:RadToolTip runat="server" ID="tooltipWRTInfodoc" Visible="false" TargetControlID="imgWRTInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/></h4>
<table border="0" cellpadding="0" cellspacing="0" style="margin:1ex auto 0px auto;">
<tr><td style='height:14px;' align="center" valign="top">
<table border="0" cellpadding="0" cellspacing="2">
<tr><td colspan=2><asp:Label id="lblPrompt" CssClass="text" runat="server" Font-Bold="true"/></td></tr>
<tr valign='middle'><td align="right" style="padding-right:1ex"><input type="text" name="DDValue" size="4" maxlength="8" class="input" style='width:4em; text-align:right;' onchange='SetFocus(1); SetDDValue();' onfocus="SetFocus(1);" onkeyup='SetFocus(1); SetDDValue();' onblur='SetFocus(0); SetDDValue();' value='<% =ValueStr %>'/></td>
    <td align="left" width="160px;"><div class='sa_slider' style='width:<% =SliderWidth %>px; height:14px; display:inline-block;' id='slider1'></div><img src='' id='img_erase' width=9 height=9 alt='Erase judgment' style='margin:0px 0px 3px 6px; cursor:pointer' onclick='if ((theForm.DDValue)) { theForm.DDValue.value = ""; SetDDValue();  } return false;'/></td></tr>
</table>
<asp:Panel runat="server" ID="pnlComment" Width="300" Visible="false" HorizontalAlign="Center" Style="margin-top:1em">
<div class="text" style="margin:1em 0px 4px 0px; text-align:left" id="DDCommentDiv"><% =CreateCommentArea("DDComment", "isChanged();")%></div>
</asp:Panel></td>
<%  If (ShowFramedInfodocs AndAlso (AlternativeInfodocURL <> "" Or CaptionInfodocURL <> "")) Then%>
<td valign='top' align='left' class='text' <% =iif(ShowComment, "rowspan=2", "") %> style='padding-left:1em; width:350px'>
<table border='0' cellpadding='0' cellspacing='0'>
<tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocGoal"/></td></tr>
<tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocNode"/></td></tr>
<tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocWRT"/></td></tr>
</table></td> 
<% End If%>
</tr></table>