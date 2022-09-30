<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlUtilityCurve" Codebehind="ctrlUtilityCurve.ascx.vb" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%--<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>--%>
<style type='text/css'>
     .ui-slider .ui-slider-handle 
     {
         width:11px;
         height:14px;
         background:url(<% =ImagePath%>slider_handler_top.gif) center no-repeat;
         border:0px;
     }
     .ui-slider-horizontal
     {
         margin-top:4px;
         height:5px;
         background:#e0e0e0;
         padding:0px;
     }
     .ui-slider-horizontal .ui-slider-handle 
     {
         top:-8px;
         margin-left:-4px;
     }
      
</style>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.jqplot.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasOverlay.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasTextRenderer.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasAxisLabelRenderer.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.ui.touch-punch.min.js"></script>
<!--[if lt IE 9]><script language="javascript" type="text/javascript" src="../../js/excanvas.min.js"></script><![endif]-->
<script type="text/javascript"><!--

    var img_path        = '<% =ImagePath %>';
    
    var img_infodoc1	= new Image; img_infodoc1.src    = img_path + '<% =imageInfodoc %>';
    var img_infodoc2	= new Image; img_infodoc2.src    = img_path + '<% =imageInfodocEmpty %>';
    var img_WRTinfodoc1	= new Image; img_WRTinfodoc1.src = img_path + '<% =imageWRTInfodoc %>';
    var img_WRTinfodoc2	= new Image; img_WRTinfodoc2.src = img_path + '<% =imageWRTInfodocEmpty %>';
    var img_delete      = new Image; img_delete.src      = img_path + 'delete_tiny.gif';
    var img_delete_     = new Image; img_delete_.src     = img_path + 'nodelete_tiny.gif';

    function HasUndefined() {
        var val = theForm.UCValue.value;
        return (val=="" || val==<% =UNDEFINED_SINGLE_VALUE %>);
    }

    function isChanged()
    {
        <% if changeFlagID<>"" Then %>if (theForm.<% =ChangeFlagID %>) theForm.<% =ChangeFlagID %>.value=1;<% end if %>
        <% if onChangeAction<>"" Then %><% = onChangeAction %>;<% end if %>
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
    
    function SetFlashVar(name, value)
    {
        var v = $get("RUC");
        if ((v)) v.SetVariable(name, value); 
    }
    
    function InitFlash()
    {
        setTimeout('theForm.SliderValue.focus();', 500);
        <% =CustomCodeOnInit %>
    }

    function SubmitForm()
    {
        if (typeof(NextStep)=="function") { isChanged(); NextStep(); }
    }

<% if CanEditInfodocs Then %> 
        function onRichEditorRefresh(empty, infodoc, callback)
        {
            if ((callback) && (callback[0]) && callback[0]!="")
            {
                var framed = <% =IIf(ShowFramedInfodocs, "1", "0") %>;
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
                  //if (src == "" && (callback[1]) && callback[1]!="") src = callback[1];                  if (src == "" && (callback[1]) && callback[1]!="") src = callback[1];
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

    document.cookie = "uc_node_save=;uc_alt_save=;uc_wrt_save=;uc_scale_save=;";

<% End If %>    


    var StepsX = [<% = StepsX %>];
    var StepsY = [<% = StepsY %>];
    var XMin = <% = XMin %>;
    var XMax = <% = XMax %>;
    var UCtype = '<% = UCtype %>';
    var Decreasing = <% = Decreasing %>;
    var Curvature = <% = Curvature %>;
    var XValue = <% = JS_SafeNumber(iIf(XValueString = "", UNDEFINED_SINGLE_VALUE, XValueString)) %>;
    var PWL = '<% = PWLString %>';
    var is_manual = false;
    var optSmooth = true;
    var optShowMarker = false;
    var plot1 = null;

    if (UCtype == 'Step') {
        optShowMarker = true;
        optSmooth = false;
    };

    function PrepareValue(value)
    {
        value = value.replace(",", ".");
        if (value=="0.") value="0";
        if (value=="1.") value="1";
        return value;
    }

    function CheckValue()
    {
        var obj = theForm.SliderValue;
        var value = ((obj) ? obj.value.replace(",", ".") : '');
        if (value=="") return true; // D0317
        if (value=="0.") value="0";
        if (value=="1.") value="1";
        if ((value*1)!=value) { dxDialog('<% =JS_SafeString(msgWrongNumber) %>', true, ((obj) ? "theForm.SliderValue.focus();" : "")); return false; }
        return true;
    }

    var UCFunction = function (x) {
        if (UCtype == "Step") {
            if (x < StepsX[0]){return Number(StepsY[0])};
            if (x >= StepsX[StepsX.length-1]){return Number(StepsY[StepsY.length-1])};
            for (var i = 0; i < StepsX.length-1; i++) {
                if ((StepsX[i] <= x)&&(StepsX[i+1] > x)){
                    if (PWL == "1"){
                        var k = (StepsY[i+1] - StepsY[i]) / (StepsX[i+1] - StepsX[i]);
                        var b = StepsY[i] - k * StepsX[i];
                        return (k * x + b);
                    }else{
                        return StepsY[i] };
                };
            };
        } else {
            if (XMin >= XMax){return 0};
            var n = new Number(0.0);
            if (!Decreasing){
                if (x < XMin){return 0};
                if (x > XMax){return 1};
                n = (XMax - XMin) * Curvature;
                if (n == 0){return (x - XMin) / (XMax - XMin)}
                else	{return (1 - Math.exp(-(x - XMin) / n)) / (1 - Math.exp(-(XMax - XMin) / n))};
            }
            else
            {
                if (x < XMin){return 1};
                if (x > XMax){return 0};
                n = (XMax - XMin) * Curvature;
                if (n == 0){return (XMax - x) / (XMax - XMin)}
                else {return (1 - Math.exp(-(XMax - x) / n)) / (1 - Math.exp(-(XMax - XMin) / n))};
            };
        };
    };

    function drawUCChart() {
        var sineRenderer = function () {
            var data = [[]];
            for (var i = 0; i < 10; i += 0.4) {
                data[0].push([i, Math.sin(i)]);
            }
            return data;
        };
        var UCRenderer = function () {
            var data = [[]];  	
	        if (UCtype == "Step"){
                var Shift = (XMax-XMin)/100;
                    data[0].push([XMin, StepsY[0] * 100]);
                    for (var i = 0; i < StepsX.length-1; i++) {
                        data[0].push([StepsX[i], StepsY[i] * 100]);
                    };
                    data[0].push([XMax, StepsY[StepsY.length-1] * 100]);
                }else{
                var Shift = (XMax-XMin)/10;	
	            for (var i = 0; i < 11; i++) {
		            data[0].push([XMin+i*Shift, UCFunction(XMin+i*Shift) * 100]);
	            };
            };


            return data;
        };

        // we have an empty data array here, but use the "dataRenderer"
        // option to tell the plot to get data from our renderer.
        plot1 = $.jqplot('ChartRUC', [], {
            dataRenderer: UCRenderer,
            seriesDefaults: {
                showMarker: optShowMarker,
                rendererOptions: {
                    smooth: optSmooth
                }
            },
            axes: {
                xaxis: {
                    min: XMin,
                    max: XMax,
                    label:'Data'
                },
                yaxis: {
                    min: 0,
                    max: 100,
                    label: "<%=YAxisCaption%>",
                    labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                    tickOptions: {
                        suffix: ' %',
                        formatString: "%#.1f"
                    }
                }
            },
            canvasOverlay: {
                show: true,
                objects: [
                    {verticalLine: {
                        name: 'xValLine',
                        x: XValue,
                        lineWidth: 3,
                        yOffset: 0,
                        lineCap: 'butt',
                        color: 'rgb(89, 198, 154)',
                        shadow: false
                    }}
                ]
            }
        });
    };

    function onChangeValue() {
        var tbVal = theForm.SliderValue;
        if ((plot1) && (tbVal) && (theForm.UCValue) && theForm.UCValue.value!=tbVal.value)
        {
//                if (CheckValue()) {

                var show_line = true;
                var val = tbVal.value*1;
                var undef = (tbVal.value=="");

                // get chart and line
                var co = plot1.plugins.canvasOverlay;
                var line = co.get('xValLine');

                // get real value and save form data
                theForm.UCValue.value = (undef ? "" : val);
                // changed
                isChanged();

                // check for limits
                var s_val = val;
                if (val<XMin) { s_val = XMin; show_line = false; }
                if (val>XMax) { s_val = XMax; show_line = false; }
                if (undef) { show_line = false; s_val = XMin; }

                // draw line
                line.options.x = (show_line ? s_val : XMin-1); 
                co.draw(plot1);

//                // get slider and set line
                $("#divSlider").slider("option", "value", s_val);

                var im = $get("imgErase");
                if ((im)) im.src = (undef ? img_delete_.src : img_delete.src);

//                }
        }
    }

    function onChangeText() {
        is_manual = true;
        onChangeValue();
        is_manual = false;
    }

    function onSliderChange(event, ui) {
        if (!is_manual) {
            var tbVal = theForm.SliderValue;
            if ((ui) && (tbVal)) {
                tbVal.value = ui.value;
                onChangeValue();
            }
        }
    }

    function sliderInit() {
        $('#divSlider').slider({
            slide: function(event, ui) { onSliderChange(event, ui); },
            min: XMin,
            max: XMax,
            step: (XMax-XMin)/500,
            value: XValue,
            animate: "medium"
        });
        var im = $get("imgErase");
        var tbVal = theForm.SliderValue;
        if ((im) && (tbVal)) im.src = ((tbVal.value=="") ? img_delete_.src : img_delete.src);
    }

    function onResizeChart() {
        var td = $get("tdContent");
        var c = $get("ChartRUC");
        if ((td) && (c)) {
            c.style.height = 1;
            $('#ChartRUC').empty();
            var pnl = $get("tdInfodcos");
            if ((pnl)) pnl.style.display='none';
            var h_all = td.clientHeight;
            var h_chart = h_all - 38;
            if (h_chart<200) h_chart = 200;
            if (h_chart>450) h_chart = 450;
            var w_chart = Math.round(h_chart/3*4);
            c.style.height = h_chart;
            c.style.width = w_chart;
            if ((plot1)) drawUCChart();
            var s = $get("divSlider");
            if ((s)) s.style.width = (w_chart - 90);
            if ((pnl)) pnl.style.display='block';
        }
    }

    function eraseValue() {
        var tbVal = theForm.SliderValue;
        if ((tbVal)) {
            tbVal.value = '';
            onChangeText();
        }
    }

    $(document).ready(function() { setTimeout("sliderInit(); drawUCChart();", 150); setTimeout("onResizeChart(); theForm.SliderValue.focus(); if (!(plot1)) drawUCChart();", 500); } );
    window.onresize = onResizeChart;

//-->    
</script>
<% If ShowFramedInfodocs Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%>

<table border="0" cellpadding="2" cellspacing="2" style="height:99%" align="center">
<tr>
    <td style="height:1em"><h4 style="margin:3px; padding:0px;"><asp:Image runat="server" ID="imgCaptionInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipGoal" Visible="false" TargetControlID="imgCaptionInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/><asp:Image runat="server" ID="imgAltInfodoc" SkinID="InfoIcon15" Visible="false" /><telerik:RadToolTip runat="server" ID="tooltipAltInfodoc" Visible="false" TargetControlID="imgAltInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomRight" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/><asp:Label id="lblCaption" runat="server"></asp:Label><asp:Image runat="server" ID="imgWRTInfodoc" SkinID="InfoReadme" Visible="false"/><telerik:RadToolTip runat="server" ID="tooltipWRTInfodoc" Visible="false" TargetControlID="imgWRTInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/></h4></td>
    <td rowspan="2" align="left" class="text" style="padding-left:1em" valign="top"><asp:Panel runat="server" ID="pnlComment" Width="300" Visible="false" HorizontalAlign="Left">
        <div class="text" style="text-align:left; margin:1ex 0px 4px 0px; text-align:left"><% =CreateCommentArea("UCComment", "isChanged();")%></div></asp:Panel>
        <br />
    <%  If (ShowFramedInfodocs AndAlso (AlternativeInfodocURL <> "" OrElse CaptionInfodocURL <> "")) Then%>
    <table border="0" cellpadding="0" cellspacing="0" id='tdInfodcos'>
    <tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocGoal"/></td></tr>
    <tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocNode"/></td></tr>
    <tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocWRT"/></td></tr>
    <tr><td><EC:FramedInfodoc runat="server" ID="frmInfodocScale"/></td></tr>
    </table>
    <% End If%>
    </td>
</tr>
<tr valign="top" align="center">
<td id="tdContent"><asp:label runat="server" ID="lblMessage" Visible="false" CssClass="text error" />
<div runat="server" id="pnlRUC" style="text-align:center" visible="false">
<div>
<div id="ChartRUC" style="width:450px;height:350px"></div>
<div id="divSlider" style="margin-left:45px; margin-right:10px;"></div>
</div>
<div style='text-align:center; margin-top:5px;'>
    <asp:Image runat="server" ID="imgScaleInfodoc" SkinID="InfoIcon15" Visible="false" />
    <input type='text' class='as_number' id="SliderValue" name="SliderValue" value="<% =XValueString %>" onkeyup='onChangeText();' onmousedown='focusOnClick(this);' style='width:6em; margin-right:6px'/><a href='' onclick='eraseValue(); return false;'><img src='<% =ImagePath %>nodelete_tiny.gif' id='imgErase' width='10' height='10' title='Erase judgment' border='0'></a>
</div>
<input type='hidden' name='UCValue' value='<% =XValue %>' /><telerik:RadToolTip runat="server" ID="tooltipScaleInfodoc" Visible="false" TargetControlID="imgScaleInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler"/>
</div>
</td>
</tr>
</table>