<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlPairwiseBar" Codebehind="ctrlPairwise.ascx.vb" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<script type="text/javascript" language="javascript" src='<% =sRootPath %>Scripts/sliderbars.js'></script>
<script type="text/javascript" language="javascript" src='<% =sRootPath %>Scripts/twinslider.js'></script>
<script type="text/javascript"><!--

    var img_path        = '<% =ImagePath %>';
    
    var img_blank       = img_path + 'blank.gif';

    var img_infodoc1	= new Image; img_infodoc1.src   = img_path + '<% =ImageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src   = img_path + '<% =ImageInfodocEmpty %>';
    var img_WRTinfodoc1	= new Image; img_WRTinfodoc1.src = img_path + '<% =ImageWRTInfodoc %>';
    var img_WRTinfodoc2	= new Image; img_WRTinfodoc2.src = img_path + '<% =ImageWRTInfodocEmpty %>';

    var img_point	    = new Image; img_point.src	    = img_path + 'bars/point.gif';
    var img_position0	= new Image; img_position0.src  = img_path + 'bars/position0.gif'; 
    var img_positive	= new Image; img_positive.src   = img_path + 'bars/position1.gif'; 
    var img_negative	= new Image; img_negative.src   = img_path + 'bars/position2.gif';

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

    var value_undef = <% =pwUndefinedValue %>;

    var sliders = new Array();
    slider_on_change = "isGPWChanged(val, adv, slider_drag_end)";

    var gpw_id = 1;
    var gpw_autoadvance = false;

    slider_gpw_real_max = <% =GetSliderMax %>;
    slider_gpw_real_over_max = <% =GetSliderMaxReal %>;

    function isUndefined()
    {
        return (theForm.pwValue) && (theForm.pwValue.value=="" || Math.abs(theForm.pwValue.value)>= Math.abs(value_undef));
    }

    function CheckUndefButton()
    {
        var btn = theForm.<% =btnUndefined.ClientID %>;
        if ((btn))
        {
            btn.style.display = (btn.disabled || (isUndefined()) ? "none" : "block");
        }
    }
    
    function ChangeImage(img, val)
    {
        if ((img)) img.src=val.src;
    }

    function isChanged()
    {
         CheckUndefButton();
         <% if ChangeFlagID<>"" Then %>if (theForm.<% =ChangeFlagID %>) theForm.<% =ChangeFlagID %>.value=1;<% end if %>
//         var m = $get("<% =lblMessage.ClientID %>");
//         if ((m)) m.style.display="none";
         $("#<% =lblMessage.ClientID %>").hide();
    }

    function isGPWChanged(val, adv, autoadvance)
    {
         if ((autoadvance)) gpw_autoadvance = true;
         SavePWValue(val, adv);
         CheckUndefButton();
         <% if ChangeFlagID<>"" Then %>if (theForm.<% =ChangeFlagID %>) theForm.<% =ChangeFlagID %>.value=1;<% end if %>
         <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>    // D0255
    }

    var show_pw_warning = 1;

    function SetPW_onChangeCall()
    {
         isChanged();
         <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>    // D0255
    }


    function SavePWValue(value, adv)
    {
         if (theForm.pwValue) theForm.pwValue.value = value; 
         if (theForm.pwAdv) theForm.pwAdv.value = adv; 
    }

    function SetPW(val, adv)
    {
         UpdatePW(val, adv);
         SavePWValue(val, adv);
         <% if ShowPWExtremeWarning Then %>if (val==9 && show_pw_warning)
         {
            dxDialog("<div style='max-width:550px;'><% =JS_SafeString(msgPWExtreme) %></div>", 'document.cookie = "<% =_COOKIE_NOVICE_PWEXTREME %>=1;path=\;expires=Thu, 31-Dec-2037 23:59:59 GMT;"; show_pw_warning = 0; SetPW_onChangeCall();');
        } else {
            SetPW_onChangeCall();
        }<% Else %>SetPW_onChangeCall();<% End if %>
         return false;
    }
    
    function PWChange(diff)
    {
        if (theForm.pwValue)
        {
            var val = theForm.pwValue.value*1;
            var adv = theForm.pwAdv.value*1;
            if (isUndefined())
            {
              val = 1; 
              adv = 0;
            }
            else
            {
              if (adv==0) adv = diff;
              val+=(-adv*diff);
              if (val==1) { adv=0; }
              if (val<=0) { val = 1; adv=-adv; }
            }
            if (Math.abs(val)<=9) SetPW(val, adv);
        }
        return false;
    }

    function UpdatePW(val, adv)
    {
         var idx = -adv*(val-1);
         if (Math.abs(val)>9) idx = value_undef;
         for (var i=-8; i<=8; i++)
          {
                var dst = img_point;
                if (!i && idx>=-8) {
                    dst = img_position0;
                }
                else
                {
                    if (i<0 && i>=idx-0.99 && idx>=-8) dst = img_negative; else if (i>0 && i<=idx+0.99) dst = img_positive;
                }
//                if (i>2 && i<6) alert(dst.src);
                SetBar('pw'+i, dst.src, Math.abs(Math.abs(i)-val)); 
          }
         if (theForm.<% =btnUndefined.ClientID %>) theForm.<% =btnUndefined.ClientID %>.disabled = (idx<-8);
    }

    function SetBar(name, img_src, delta)
    {
        if (document.images[name]) {
            document.images[name].src = img_src;
            document.images[name].style.width = (img_src!=img_point.src && delta>0.01 && delta<1 ? Math.round(14*delta) + "px" : "14px");
        }
    }

    function InitGPW(val, adv)
    {
        var d = $get("Slider");
        if ((d)) {
            d.innerHTML = TwinSliderInit(gpw_id, val*1, adv*1, 0, 0, 0);
            setTimeout('var s = eval("theForm.gpw_slider" + gpw_id + "_b"); if ((s)) { s.focus(); s.blur(); }', 100);
        }
    }

    function GPWHotkeys(event)
    {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event) {
          var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
          if (code == 13) { gpw_autoadvance = true; if ((theForm.pwValue) && (theForm.pwAdv)) isGPWChanged(theForm.pwValue.value, theForm.pwAdv.value); return false; }
        }
        return true;
    }
    
    theForm.onkeyup = GPWHotkeys;
    
    function SetEditFocus(is_a)
    {
        var fld = (is_a ? TwinSliderGetEditA(gpw_id) : TwinSliderGetEditB(gpw_id));
        if ((fld)) fld.focus();
    }

    function CheckGPWForm()
    {
        var fld_a = TwinSliderGetEditA(gpw_id);
        var fld_b = TwinSliderGetEditB(gpw_id);
        if ((fld_a) && (fld_b) && (fld_a.value!="" || fld_b.value!=""))
        {
            var a = TwinSliderCheckEditField(fld_a);
            if (fld_a.value!="" && !a.valid) { dxDialog("<% = JS_SafeString(MsgWrongNumberPart) %>", true, "SetEditFocus(1);"); return false; }
            var b = TwinSliderCheckEditField(fld_b);
            if (fld_b.value!="" && !b.valid) { dxDialog("<% = JS_SafeString(MsgWrongNumberPart) %>", true, "SetEditFocus(0);"); return false; }
            <% If GPWMode<>GraphicalPairwiseMode.gpwmInfinite AndAlso GPWModeStrict Then %>if (a.valid && b.valid) {
                var c = (a.value>b.value ? a.value/(b.value+0.000000001) : b.value/(a.value+0.00000001));
                if (c><% =GetSliderMaxReal %>) { dxDialog("<% = JS_SafeString(MsgWrongNumber) %>", true, "SetEditFocus(" + (b.value==1 ? 1 : 0) + ");"); return false; }
            }<% End if %>
        }
        if (isUndefined() && !(ConfirmMissingJudgment())) return false;
        return true;
    }

    function CheckPWForm()
    {
        if (isUndefined() && !ConfirmMissingJudgment()) return false;
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

    function InitPW() {
        for (var i=0; i<9; i++) {
            setTimeout('$("#ID' + i + '").show().animate({height:' + (7*(i+1)) + '}, 100);' + (i!=0 ? '$("#ID-' + i + '").show().animate({height:' + (7*(i+1)) + '}, 100);' : ''), 150+i*30);
        }
    }
    
<% if CanEditInfodocs Then %> 
        // D0995 ===
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
                      case "goal": { n="<% =imgCaptionInfodoc.ClientID %>"; t = "<% =tooltipGoal.ClientID %>"; break; }
                      case "left": { n="<% =ImageLeft.ClientID %>"; t = "<% =tooltipLeft.ClientID %>"; break; }
                      case "right": { n="<% =ImageRight.ClientID %>"; t = "<% =tooltipRight.ClientID %>"; break; }
                      case "left_wrt": { n="<% =ImageLeftWRT.ClientID %>"; t = "<% =tooltipLeftWRT.ClientID %>"; break; }
                      case "right_wrt": { n="<% =ImageRightWRT.ClientID %>"; t = "<% =tooltipRightWRT.ClientID %>"; break; }
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
                    setTimeout("var f = $get('" + callback[0] + "'); f.src= \"" + src + "\";" , 350);
                  }   
                  else
                  {
                    if (callback[0]=="left_wrt" || callback[0]=="right_wrt")
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
                          txt = "<iframe style='border:0px; padding:2px;' frameborder='0' allowtransparency='true' src='" + src + "' class='frm_loading' onload='InfodocFrameLoaded(this);'></iframe>";
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
        // D0995 ==
        
    function InitRFrame(name) {
        var o = document.getElementById(name);
        if ((o)) o.style.overflow = "hidden";
    }
   
    document.cookie = "pw_node_save=;pw_goal_save=;pw_wrt_save=;";
        
<% End If %>  
//-->    
</script>
<% If ShowFramedInfodocs Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%>
<table border="0" cellpadding="0" cellspacing="0" style="width:99%"><tr><td align="center" valign="top"><table border="0" cellpadding="2" cellspacing="0" style="margin-bottom:1ex;" class="zoom800">
    <tr style="height:1em">
        <td colspan="3" align="center" valign="top" class="text"><% If Not ShowFramedInfodocs Then%><h4 style="padding:0px"><% End If%><asp:Image runat="server" ID="imgCaptionInfodoc" SkinID="InfoIcon15" Visible="false" EnableViewState="false" /><telerik:RadToolTip runat="server" ID="tooltipGoal" Visible="false" TargetControlID="imgCaptionInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomCenter" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler" EnableViewState="false"/><% If Not ShowFramedInfodocs Then%><asp:Label ID="lblCaption" runat="server" EnableViewState="false"/></h4><% End If%>
        <% If ShowFramedInfodocs Then %><p align="center" style="padding:0px; margin:0px"><table border="0" cellpadding="0" cellspacing="0"><tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocGoal" EnableViewState="false"/></td></tr></table></p><% End if %>
        </td>
    </tr>
    <tr style="height:50px">
        <td style="width:35%" class="pw_object object_left aslink" <% If PWType = PairwiseType.ptGraphical Then %>onmousedown='slider_is_pressed=1; TwinSliderArrowClick(1,-1,1);' onmouseup='slider_is_pressed=0;' onmouseout='slider_is_pressed=0;'<% Else %>onclick='PWChange(-1);'<% End if %>><asp:Image runat="server" ID="ImageLeft" SkinID="InfoIcon15" Visible="false" EnableViewState="false"/><telerik:RadToolTip runat="server" ID="tooltipLeft" Visible="false" TargetControlID="ImageLeft" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler" EnableViewState="false"/>&nbsp;<asp:Literal ID="lblNode1" runat="server" EnableViewState="false"/>&nbsp;<asp:Image runat="server" ID="ImageLeftWRT" SkinID="InfoReadme" Visible="false" EnableViewState="false"/><telerik:RadToolTip runat="server" ID="tooltipLeftWRT" Visible="false" TargetControlID="ImageLeftWRT" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler" EnableViewState="false"/><% = GetKnownLikelihood(KnownLikelihoodA)%><% = GetNodeComment(LeftNodeComment)%></td>
        <td class='pw_scale' valign="<% =IIF(PWType = PairwiseType.ptGraphical, "middle", "bottom") %>" style="text-align:center; white-space:nowrap; vertical-align:bottom; line-height:1.0" align="center"><input type="hidden" name="pwValue" value="<% =GetPWValue() %>" /><input type="hidden" name="pwAdv" value="<% = Data.Advantage %>" /><% = GetPWBars() %><asp:Panel runat="server" ID="pnlGPW" Visible="false" EnableViewState="false">
        <div style="width:460px; padding-top:16px;" id="Slider"><div style="text-align:center;" class='text small gray'>Loading…</div></div></asp:Panel></td>
        <td style="width:35%" class="pw_object object_right aslink" <% If PWType = PairwiseType.ptGraphical Then %>onmousedown='slider_is_pressed=1; TwinSliderArrowClick(1,+1,1);' onmouseup='slider_is_pressed=0;' onmouseout='slider_is_pressed=0;'<% Else %>onclick='PWChange(+1);'<% End if %>><asp:Image runat="server" ID="ImageRight" SkinID="InfoIcon15" Visible="false" EnableViewState="false"/><telerik:RadToolTip runat="server" ID="tooltipRight" Visible="false" TargetControlID="ImageRight" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler" EnableViewState="false"/>&nbsp;<asp:Literal ID="lblNode2" runat="server" EnableViewState="false"/>&nbsp;<asp:Image runat="server" ID="ImageRightWRT" SkinID="InfoReadme" Visible="false" EnableViewState="false"/><telerik:RadToolTip runat="server" ID="tooltipRightWRT" Visible="false" TargetControlID="ImageRightWRT" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler" EnableViewState="false"/><% = GetKnownLikelihood(KnownLikelihoodB)%><% = GetNodeComment(RightNodeComment)%></td>
    </tr>
    <tr style="height:1px">
        <td align="center" colspan="3" valign="top"><asp:PlaceHolder ID="phScale" runat="server" EnableViewState="false"></asp:PlaceHolder></td>
    </tr>
    <tr>
        <% If ShowFramedInfodocs Then %><td valign='top' style='white-space:nowrap' class='text'><img src='<% =ImagePath %>blank.gif' width=252 height=1 alt='' border=0><br><EC:FramedInfodoc runat="server" ID="frmInfodocLeft" EnableViewState="false"/>
          <% If ShowWRTInfodocs Then %><div style='clear:both'><EC:FramedInfodoc runat="server" ID="frmInfodocWRTLeft" EnableViewState="false"/></div><% End if %>
        </td><% End If%>
        <td <% = iif(ShowFramedInfodocs, "", "colspan=3") %> align="center" valign="top" style="padding-top:3px">
        <asp:Button runat="server" ID="btnUndefined" AccessKey="E" CssClass="button" UseSubmitBehavior="false" Width="10em"/>
        <asp:Label runat="server" ID="lblMessage" Visible="false"/>
        <asp:Panel runat="server" ID="pnlComment" Width="280" Visible="false"><% =CreateCommentArea("PWComment", "isChanged();")%></asp:Panel>
        </td>
        <% If ShowFramedInfodocs Then %><td valign='top' style='white-space:nowrap' class='text'><img src='<% =ImagePath %>blank.gif' width=252 height=1 alt='' border=0><br><EC:FramedInfodoc runat="server" ID="frmInfodocRight" EnableViewState="false"/>
          <% If ShowWRTInfodocs Then %><div style='clear:both'><EC:FramedInfodoc runat="server" ID="frmInfodocWRTRight" EnableViewState="false"/></div><% End if %>
        </td><% End If%>
    </tr>
</table></td></tr></table>
<asp:Label runat="server" ID="mapVerbalPW" Enabled="false"  EnableViewState="false"/>
<script type="text/javascript" language="javascript">
    CheckUndefButton();
</script>