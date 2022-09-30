<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlRating" Codebehind="ctrlRating.ascx.vb" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<script type="text/javascript"><!--

    var img_path        = '<% =ImagePath %>';
    
    var img_infodoc1	= new Image; img_infodoc1.src   = img_path + '<% =ImageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src   = img_path + '<% =ImageInfodocEmpty %>';
    var img_WRTinfodoc1	= new Image; img_WRTinfodoc1.src = img_path + '<% =ImageWRTInfodoc %>';
    var img_WRTinfodoc2	= new Image; img_WRTinfodoc2.src = img_path + '<% =ImageWRTInfodocEmpty %>';

    var img_int_0	= new Image; img_int_0.src   = img_path + 'info12_dis.png';
    var img_int_1	= new Image; img_int_1.src   = img_path + 'info12.png';

    function isChanged()
    {
         <% if ChangeFlagID<>"" Then %>if (theForm.<% =ChangeFlagID %>) theForm.<% =ChangeFlagID %>.value=1;<% end if %>
    }

    function SetRating(ID)
    {
         if (theForm.RatingValue) theForm.RatingValue.value = ID;
         isChanged();
         <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>
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
    
    function CheckForm()
    {
         if ((theForm.RatingValue) && (theForm.RatingValue.value==='' || theForm.RatingValue.value==-1) && !ConfirmMissingJudgment()) return false;
         return true;
    }

    function CheckNumber(val)
    {
        var is_number = false;
        var n = val.replace(",", ".");
        if (n[0] == ".") n = "0" + n;
        if (n == "0.") n = "0";
        if ((n * 1) == n) 
        {
            n = n * 1;
            is_number = (n >= 0 && n <= 1);
        }
        return {valid: is_number, value: n};
    }

    function onKeyUpDirect(show_warning)
    {
        var inp = theForm.DirectRating;
        var v = CheckNumber(inp.value);
        if (v.valid)
        {
            inp.style.backgroundColor = "";
            if (inp.value!="") SetRating(-2);
        }
        else
        {
            inp.style.backgroundColor = "#ffe0e0";
            if (show_warning) ShowWarning();
        }
    }
       
    function ShowWarning()
    {
        dxDialog('<% =JS_SafeString(msgDirectRatingValue) %>', true, "theForm.DirectRating.focus();");
        return false;
    }

    function SaveDescription() {
        var radToolTip = $find("<%= ttEditDescription.ClientID %>");
        var c_fld = eval("theForm.Intensity" + theForm.fldID.value);
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
                d.innerHTML = (desc=="" ? "" : " &nbsp;&middot;&nbsp;" + (desc.length>100 ? desc.substr(0,100) + "&#133;" : desc));
                d.title = desc;
            }

            isChanged();
            show_comment = 0;
        }
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
                    case "scale": { n="<% =imgScaleInfodoc.ClientID %>"; t = "<% =tooltipScaleInfodoc.ClientID %>"; break; }
                }  
            }
            var f = $get(n);
            if ((f))
            {
                var src = callback[1];
                if (framed && src == "" && location.href!=f.src) src = f.src;
                //if (src == "" && (callback[1]) && callback[1]!="") src = callback[1];                if (src == "" && (callback[1]) && callback[1]!="") src = callback[1];
                if (src!="") src = src + "&=" + Math.random();
                if (empty==1) src = "";
                if (framed)
                {
                if ((f.contentWindow)) f.contentWindow.document.body.innerHTML = "<div style='<% =String.Format(FrameLoadingStyle, ImagePath)%>'>&nbsp</div>";
                setTimeout("var f = $get('" + callback[0] + "'); f.src= \"" + src + "\";" , 200);
                }   
                else
                {
                if (callback[0]=="scale")
                {
                    f.src = (empty==1 ? img_infodoc2.src : img_infodoc1.src);
                }
                else
                {
                    if (callback[0]=="wrt")
                    {
                        f.src = (empty==1 ? img_WRTinfodoc2.src : img_WRTinfodoc1.src);
                    }
                    else
                    {   
                        f.src = (empty==1 ? img_infodoc2.src : img_infodoc1.src);
                    }
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

    function SetDesc() {
        for (var i=0; i<50; i++) {
            var d = eval("theForm.Intensity" + i);
            if ((d) && d.value!="") $get('Int' + i).title = d.value;
        }
    }

 function EditIntensityDesc(id, top)
    {
        var int_d = eval("theForm.Intensity" + id);
        if ((int_d))
        {
            var desc = int_d.value;
            var radToolTip = $find("<%= ttEditDescription.ClientID %>");
            if ((radToolTip)) {
                show_comment = 1;
                radToolTip.set_targetControlID("<% =iif(ShowFramedInfodocs, "imgDesc", "Int") %>" + id);
                radToolTip.set_position(top ? 11 : 31);
                theForm.tbDescription.value = desc;
                theForm.curDescID.value = id;
                theForm.fldID.value = id;
                radToolTip.show();
                setTimeout('theForm.tbDescription.focus();', 100);
            }
        }
        return false;
    }

    document.cookie = "r_node_save=;r_alt_save=;r_wrt_save=;scale_save=;";

<% End If %>

    function onResizeRatings() {
        var h_old = $("#divRatings").height();
        $("#divRatings").height(10);
        var h = $("#tdRatings").parent().height();
        if (h>60) {
            h -=2;
        } else {
            h = h_old;
        }
        $("#divRatings").height(h);
    }

    function onMouseUpResize() {
        setTimeout('onResizeRatings();', 5);
    }

    window.onresize = onResizeRatings;
    window.onmouseup = onMouseUpResize;

//-->    
</script>
<% If ShowFramedInfodocs Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%>
<table border="0" cellpadding="0" cellspacing="0" style="height:99%; margin: 0px auto;">
<tr style="height:1em; padding-top:1ex;" runat="server" id="trCaption">
 <td colspan="2"><h4><asp:Image runat="server" ID="imgCaptionInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipGoal" Visible="false" TargetControlID="imgCaptionInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight"  OffsetX="-9" OffsetY="-5" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/><asp:Image runat="server" ID="imgAltInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipAltInfodoc" Visible="false" TargetControlID="imgAltInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" OffsetX="-9" OffsetY="-5" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>&nbsp;<asp:Label ID="lblCaption" runat="server"/>&nbsp;<asp:Image runat="server" ID="imgWRTInfodoc" SkinID="InfoReadme" Visible="false"/><telerik:RadToolTip runat="server" ID="tooltipWRTInfodoc" Visible="false" TargetControlID="imgWRTInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/></h4></td>
</tr>

<tr class="text" valign='top'>
  <td width="40%" align="right"><% If (ShowFramedInfodocs AndAlso (AlternativeInfodocURL <> "" OrElse CaptionInfodocURL <> "" OrElse WRTInfodocURL <> "")) Then%>
    <table border='0' cellpadding='6' cellspacing='0'>
     <% If CaptionInfodocURL <> "" Then%><tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocGoal"/></td></tr><% End If%>
     <% If AlternativeInfodocURL <> "" Then%><tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocNode"/></td></tr><% End If%>
     <% If WRTInfodocURL <> "" Then%><tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocWRT"/></td></tr><% End If%>
    </table>
  <% Else%>&nbsp;<% End If%></td>
  <td style="white-space:nowrap;"><table class="whole" border="0" cellpadding="6" cellspacing="0">
  <%  If ShowFramedInfodocs AndAlso ScaleInfodocURL <> "" Then%><tr style="height:1em"><td class="text"><EC:FramedInfodoc runat="server" ID="frmInfodocScale"/></td></tr><% End If%>
  <tr><td class="text" valign="top" id="tdRatings"><div id="divRatings" style="height:100%;overflow:auto; text-align:left"><asp:Image runat="server" ID="imgScaleInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipScaleInfodoc" Visible="false" TargetControlID="imgScaleInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" OffsetX="-9" OffsetY="-5" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/><asp:RadioButtonList ID="rbListRatings" runat="server" CssClass="text"/></div></td></tr>
  <%  If ShowComment Then%><tr style="height:1em"><td><div style="width:300px" id="CommentDiv"><% =CreateCommentArea("RatingComment", "isChanged();")%></div></td></tr><% End If %>
  </table></td>
</tr>
</table><input type="hidden" name="RatingValue" value="<% =GetValue %>" />
<telerik:radtooltip runat="server" id="ttEditDescription" Skin="WebBlue" SkinID="tooltipInfo" RelativeTo="Element" Position="BottomLeft" OffsetX="-5" ShowEvent="FromCode" HideEvent="ManualClose" CssClass="panel" IsClientID="true" >
<div class="text" style="font-weight:bold;"><%=lblIntensityDescriptionInput%>:</div>
<textarea name="tbDescription" rows="8" cols="45" class="textarea" style="margin:1ex 0px"></textarea>
<div style="text-align:right"><input type='button' id='btnSaveDesc' value='Save' onclick='SaveDescription();' class='button button_small'/><input type='hidden' id='curDescID' value='-1'/><input type='hidden' id='fldID' value='-1'/></div>
</telerik:radtooltip>
<% = Intensities %>