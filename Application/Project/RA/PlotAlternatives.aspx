<%@ Page Language="VB" Inherits="RAPlotAlternatives" title="Resource Aligner Plot Alternatives" Codebehind="PlotAlternatives.aspx.vb" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI"   Assembly="Telerik.Web.UI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.jqplot.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.bubbleRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.cursor.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasAxisLabelRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasTextRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.highlighter.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables_only.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables.extra.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/dataTables.buttons.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/buttons.html5.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/buttons.print.min.js"></script>
<script language="javascript" type="text/javascript">

    var bubble_chart = null;
    var table = null;

    var data_all = [[<% =GetData(0) %>],
                    [<% =GetData(1) %>],
                    [<% =GetData(2) %>],
                    [<% =GetData(3) %>]];

    var chart_id = <% =ChartID %>;
    var chart_data = data_all[chart_id];

    var bubble_mult = <% =JS_SafeNumber(BubbleCoeff) %>;

    var set_spins = true;

    var show_labels = false;
    var spin_manual = false;
    var mh = 400;   // maxHeight for dialogs;

    var IDX_CHART_NAMES = 0;        //A0902
    var IDX_CHART_DATA  = 1;        //A0902
    var IDX_CHART_CAT_DATA  = 2;    //A0902

    var IDX_ORIG_RADIUS = 4;        // D3309
    var IDX_ALT_ID = 5;             //A0902
    var IDX_CATEGORIES = 6;         //A0902
    var IDX_FUNDED_STATE_CHAR = 7;  //A0891 - F, +, -, %
    var IDX_FUNDED_STATE = 8;       //A0891 - 0 - became funded, 1 - became not funded, 2 - both funded, 3- both not funded, 4 - percent changed
    var IDX_FUNDED_VALUE = 9;       // % funded

    var IDX_BUBBLE_X = 0;           // A0902
    var IDX_BUBBLE_Y = 1;           // A0902
    var IDX_BUBBLE_RADIUS = 2;      // A0902
    var IDX_BUBBLE_EXTRA = 3;       // A0902

    var IDX_COLOR_LEGEND = 0;                //A0902
    var IDX_COLOR_LEGEND_COLOR    = 0;
    var IDX_COLOR_LEGEND_CATEGORY_NAME = 1;
    var IDX_COLOR_LEGEND_CATEGORY_GUID = 2;
    var IDX_COLOR_LEGEND_PROJECTS = 3;
    var IDX_COLOR_LEGEND_PROJECTS_PRC = 4;
    var IDX_COLOR_LEGEND_COSTS        = 5;
    var IDX_COLOR_LEGEND_COSTS_PRC    = 6;
    var IDX_COLOR_LEGEND_COSTS_SORT   = 7;
    var IDX_COLOR_LEGEND_BENEFITS     = 8;
    var IDX_COLOR_LEGEND_BENEFITS_PRC = 9;

    var IDX_SELECTED_CATEGORY_TYPE  = 1;     //A0902
    var IDX_SELECTED_CATEGORY_ITEMS = 2;     //A0902

    var COLOR_BECAME_FUNDED = "#66cc66";        //"#ffff66";
    var COLOR_BECAME_NOT_FUNDED = "#990000";    //"#ff66cc";
    var COLOR_BOTH_FUNDED = "#ffff00";          //"#ccff00";
    var COLOR_BOTH_NOT_FUNDED = "#ffffff";
    var COLOR_FUNDED_PERCENT_CHANGED = " #ffccff"; //"#ffffcc";
    var COLOR_NEGATIVE_RADIUS = "#ffffff";      // D3309
    
    var ATTR_TYPE_ENUM = 4;         // A0902
    var ATTR_TYPE_MULTI_ENUM = 5;   // A0902

    var ShowColorLegend = "<% =ColorBubblesByCategory AndAlso AttributesList.Count > 0 %>" == "True";
    var FundedFilteringMode = <%=Cint(FundedFilteringMode) %>;
    var is_context_menu_open = false;       //A0902
    var right_click_alt_id = "";            //A0902

    var saved_spins = [[0,0,0,0],[0,0,0,0],[0,0,0,0],[0,0,0,0]]; //A0902 + D4533

    function createChart() {
        var has_data = ((chart_data) && (chart_data.length>=2) && chart_data[IDX_CHART_DATA].length>0);
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var btn = toolbar.findItemByText("<% = JS_SafeString(ResString("lblRAPlotAltsViewArea")) %>"); //A0888
            if ((btn)) btn.set_enabled(has_data);
        }
        
        var plot_title = (chart_data[IDX_CHART_NAMES].length>1 ? chart_data[IDX_CHART_NAMES][0] : chart_data[IDX_CHART_NAMES]);
        $get("PlotTitle").innerHTML = htmlEscape(plot_title);
        
        if (!has_data)
        {
            $("#divColorLegend").hide();
            $("#bubble_chart").html("<table border=0 class='whole'><tr><td valign=middle align=center><h6><%=ResString("msgRiskResultsNoData")%></h6></td></tr></table>");
            return false;
        }
        if ((set_spins))
        {
            var cur_spins = saved_spins[chart_data[IDX_CHART_NAMES][4]];
            if ((cur_spins[0]==0) && (cur_spins[1]==0) && (cur_spins[2]==0) && (cur_spins[3]==0)) {
                for (var j=0; j<=3; j++) saved_spins[chart_data[IDX_CHART_NAMES][4]][j] = chart_data[IDX_CHART_NAMES][j+5];
            }
            spin_manual = true;
            var xi = $find("<% =SpinIDs(0) %>"); if ((xi)) xi.set_value(chart_data[IDX_CHART_NAMES][5]);
            var xa = $find("<% =SpinIDs(1) %>"); if ((xa)) xa.set_value(chart_data[IDX_CHART_NAMES][6]);
            var yi = $find("<% =SpinIDs(2) %>"); if ((yi)) yi.set_value(chart_data[IDX_CHART_NAMES][7]);
            var ya = $find("<% =SpinIDs(3) %>"); if ((ya)) ya.set_value(chart_data[IDX_CHART_NAMES][8]);
            spin_manual = false;            
        }

        var pos_data = [];
        var neg_data = [];
        for (var i=0; i<chart_data[IDX_CHART_DATA].length; i++) {
            if (chart_data[IDX_CHART_DATA][i][IDX_ORIG_RADIUS]<0) neg_data.push(chart_data[IDX_CHART_DATA][i]); else pos_data.push(chart_data[IDX_CHART_DATA][i]);
        }

        bubble_chart = $.jqplot('bubble_chart', [pos_data,neg_data], {  // D3309
            // Only animate if we're not using excanvas (not in IE 7 or IE 8).
            animate: !$.jqplot.use_excanvas, //A0902
            captureRightClick: true, //A0902 - if 'false' - need to disable the 'cursor' plugin
            highlighter: { show: false }, //A0902
            //title: {                
                //text: plot_title,
                //fontSize: '13pt',
                //textColor: '#006699',
                //show: false
            //},
            seriesDefaults: {
                renderer: $.jqplot.BubbleRenderer,
                rendererOptions: {
//                    bubbleAlpha: 0.75,
//                    highlightAlpha: 0.95,
//                    showLabels: (show_labels || (chart_data[IDX_CHART_NAMES][4]!=0 && (chart_data[IDX_CHART_NAMES][9])) ? true : false),
                    showLabels: (show_labels ? true : false),
                    varyBubbleColors: true,
                    autoscaleBubbles: false,    //((chart_data[IDX_CHART_NAMES][9]==1) ? false : true),
                    bubbleGradients: false
                    /*,
                    autoscaleMultiplier: bubble_mult*/
                },
//                shadow: true,
                shadowAlpha: 0.05
            },
            grid: {
                background: '#ffffff'
            },
            series:[    // D3309
                {rendererOptions: { bubbleAlpha: 0.70, highlightAlpha: 0.97}, shadow: true},
                {rendererOptions: { bubbleAlpha: 0.25, highlightAlpha: 0.35}, shadow: false}
            ],
            cursor: {
                show: true,
                tooltipLocation: 'sw',
                showTooltip: false,
                zoom: true,
                clickReset: true,
                dblClickReset: true,
                showVerticalLine: false,
                showHorizontalLine: false
            },
            axes:{
                xaxis:{
                    min: chart_data[IDX_CHART_NAMES][5],
                    max: chart_data[IDX_CHART_NAMES][6],
                    label:chart_data[IDX_CHART_NAMES][1],
                    labelRenderer: $.jqplot.CanvasAxisLabelRenderer/*,
                    labelOptions:{
                        textColor: "#333333"
                    }*/
                },
                yaxis:{
                    min: chart_data[IDX_CHART_NAMES][7],
                    max: chart_data[IDX_CHART_NAMES][8],
                    label:chart_data[IDX_CHART_NAMES][2],
                    labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                    tickRenderer: $.jqplot.CanvasAxisTickRenderer,
                        tickOptions: {
                            formatString: "%'.2f"
                        } /*
                    labelOptions:{
                        textColor: "#333333"
                    }*/
                }
            }/*,
            legend:{
                show: true,
                placement: 'outsideGrid',
                labels: names
            }*/
        });

        $('#bubble_chart').unbind('jqplotDataHighlight');
        $('#bubble_chart').bind('jqplotDataHighlight',
        function (ev, seriesIndex, pointIndex, data, radius) {
            var chart_left = 0; //$('#bubble_chart').offset().left;
            var chart_top = 0; //$('#bubble_chart').offset().top;
            x = bubble_chart.axes.xaxis.u2p(data[IDX_BUBBLE_X]);  // convert x axis unita to pixels
            y = bubble_chart.axes.yaxis.u2p(data[IDX_BUBBLE_Y]);  // convert y axis units to pixels
            var color = 'rgb(80%,80%,80%)';
            var lbls = ["","x","y","r"];
            if ((chart_data) && (chart_data[IDX_CHART_NAMES].length>1)) lbls = chart_data[IDX_CHART_NAMES];
            var pos_x = chart_left + x + radius; // + 5;
            var w = $('#bubble_chart').width();
            if ((w) && w>100 && pos_x>(w-radius-100)) { pos_x = chart_left + x - radius - 100; if (pos_x<20) pos_x = 20; }
            var pos_y = chart_top + y + 5;
            var h = $('#bubble_chart').height();
            if ((h) && h>100 && pos_y>(h-80)) { pos_y = chart_top + y - 80; if (pos_y<20) pos_y = 20; }
            $('#bubble_tooltip').css({ left: pos_x, top: pos_y });
            //$('#bubble_tooltip').html('<h6>' + htmlEscape(data[IDX_BUBBLE_EXTRA].label) + '</h6>' + lbls[1] + ': <b>' + (chart_id != 2 ? data[3].cost : data[0]) + '</b><br>' + lbls[2] + ': <b>' + data[1] + '</b>' + ((chart_data[IDX_CHART_NAMES][4]) && data[2]!='0.00001' ? '<br>' + lbls[3] + ': <b>' + (chart_data[IDX_CHART_NAMES][4] == 1 ? Math.round(1000*(data[2]-<% =JS_SafeNumber(OPT_RISK_OFFSET) %>))/1000 : (chart_id == 2 ? data[3].cost : data[2])) + '</b>' : '')); //A0907
            $('#bubble_tooltip').html('<h6>' + htmlEscape(data[IDX_BUBBLE_EXTRA].label) + '</h6>' + lbls[1] + ': <b>' + (chart_id != 2 ? data[IDX_BUBBLE_EXTRA].cost : data[0]) + '</b><br>' + lbls[2] + ': <b>' + data[1] + '</b>' + ((chart_data[IDX_CHART_NAMES][4] && chart_data[IDX_CHART_NAMES][4]!=3) ? '<br>' + lbls[3] + ': <b>' + (chart_data[IDX_CHART_NAMES][4] == 1 || chart_data[IDX_CHART_NAMES][4] == 2 ? data[IDX_BUBBLE_EXTRA].risk : (chart_id == 2 ? (data[IDX_ORIG_RADIUS]<0? "<span style='color:#cc0000'>" + data[IDX_BUBBLE_EXTRA].cost + "</span>" : data[IDX_BUBBLE_EXTRA].cost) : data[IDX_BUBBLE_EXTRA].risk)) + '</b>' : '')); //A0907 + A0946
            $('#bubble_tooltip').show();
        });

        // Bind a function to the unhighlight event to clean up after highlighting.
        $('#bubble_chart').unbind('jqplotDataUnhighlight');
        $('#bubble_chart').bind('jqplotDataUnhighlight',
        function (ev, seriesIndex, pointIndex, data) {
            $('#bubble_tooltip').empty();
            $('#bubble_tooltip').hide();
        });

        //$.jqplot.postDrawHooks.push(function() {
        //    $(".jqplot-highlight-canvas").css('z-index', '3');
        //    $(".jqplot-event-canvas").css('z-index', '4');
        //});
        
        // highlight Funded (F), became funded (+), stopped being funded (-)
        highlightFundedDatapoints(bubble_chart); //A0891
        
        //$('#bubble_chart').bind('jqplotDataClick', //A0902 - disable the cursor plugin in order to capture left-clicks
        //    function (ev, seriesIndex, pointIndex, data, radius) {
        //        alert('point: '+pointIndex+', data: '+data);
        //    }
        //);
        $('#bubble_chart').unbind('jqplotDataRightClick');
        $('#bubble_chart').bind('jqplotDataRightClick', 
            //function (ev, seriesIndex, pointIndex, data, radius) {
            function (ev, seriesIndex, pointIndex, data) {                
                hideContextMenu();
                
                //hide the tooltip
                $('#bubble_tooltip').empty();
                $('#bubble_tooltip').hide();
                
                //show the context menu
                var x = bubble_chart.axes.xaxis.u2p(data[IDX_BUBBLE_X]);  // convert x axis unita to pixels
                var y = bubble_chart.axes.yaxis.u2p(data[IDX_BUBBLE_Y]); 
                $("div.context-menu").css({top: y + 3 + "px", left: x + 3 + "px"});
                var w = $("div.context-menu").width();
                var pw = $("#bubble_chart").width(); 
                if ((pw) && (x+w+10>pw) && (x-w-10>0)) $("div.context-menu").css({left: (x-w-10) + "px"});

                $("#menuRowColor").hide();
                $("#menuRowCategory").hide();
                //$("#menuRowButtons").hide();
                $("#cbBubbleCategory").hide();
                $("#divMultiCategories").hide();

                if (!ShowColorLegend) {// If Color Bubbles By Category is turned ON - don't allow to change colors                                    
                    //init color picker
                    $("#colorbox").dxColorBox("instance").option("disabled", true);
                    $("#colorbox").dxColorBox("instance").option("value", data[IDX_BUBBLE_EXTRA].color);
                    $("#colorbox").dxColorBox("instance").option("disabled", false);
                    $("#colorbox").dxColorBox("instance").option("readOnly", false);
                    $("#menuRowColor").show();                
                } else { // If Color Bubbles By Category is turned ON - allow to change categories
                    var cb = $get("cbCategories");
                    $get("lblMenuCategory").innerHTML = "";
                    $("#lblMenuCategory").css({color:"#000"});
                    if ((cb) && (cb.selectedIndex >=0)) $get("lblMenuCategory").innerHTML = ShortString(cb.options[cb.selectedIndex].innerHTML, 25) + ":";
                    if ((cb) && (cb.selectedIndex >=0) && (chart_data[IDX_CHART_CAT_DATA].length>IDX_SELECTED_CATEGORY_TYPE)) {
                        switch (chart_data[IDX_CHART_CAT_DATA][IDX_SELECTED_CATEGORY_TYPE]*1) {
                            case ATTR_TYPE_ENUM:
                                $("#cbBubbleCategory").show();
                                var cbb = $get("cbBubbleCategory");
                                if ((cbb)) {
                                    cbb.disabled = false;
                                    if (data[IDX_CATEGORIES] !="") {
                                        cbb.value = data[IDX_CATEGORIES];
                                    } else {
                                        //cbb.selectedIndex = cbb.options.length - 1;
                                        cbb.selectedIndex = 0;
                                    }
                                }
                                if (cbb.options.length == 1) { //No Category item only
                                    cbb.selectedIndex = 0;
                                    cbb.disabled = true;
                                    $("#lblMenuCategory").css({color:"#a0a0a0"});
                                }
                                break;
                            case ATTR_TYPE_MULTI_ENUM:
                                $("#divMultiCategories").show();
                                var div_multi = $get("divMultiCategories");
                                //$("#menuRowButtons").show();
                                var sVal = "";
                                var vals = data[IDX_CATEGORIES].split(";");
                                if (chart_data[IDX_CHART_CAT_DATA].length>IDX_SELECTED_CATEGORY_ITEMS) {                                    
                                    var v = chart_data[IDX_CHART_CAT_DATA][IDX_SELECTED_CATEGORY_ITEMS];
                                    var items_len = v.length;
                                    for (j=0; j<items_len; j++) {
                                        var is_selected = false;
                                        for (k=0; k<vals.length; k++) {
                                            if (vals[k] == v[j][0]) is_selected = true;                            
                                        }
                                        sVal += "<li style='margin:0; padding:0;' title='" + htmlEscape(v[j][1]) +"'><label for='chk" + j + "_" + k + "'><input type='checkbox' class='checkbox cat_item_chk' id='chk" + j + "_" + k + "' value='" + v[j][0] + "'" + (is_selected ? " checked" : "") + ">" + htmlEscape(v[j][1]) + "</label></li>";
                                    }
                                    sVal = "<ul id='flt_val_" + i + "' style='height:12ex; width:175px; overflow-x:hidden; overflow-y:auto; border:1px solid #999999; text-align:left; margin:0px; padding:0px;'>" + sVal + "</ul>";
                                    if (items_len == 0) { /*$("#menuRowButtons").hide();*/ $("#lblMenuCategory").css({color:"#a0a0a0"});}
                                }
                                div_multi.innerHTML = sVal;
                                break;
                        }
                    } else {
                        var cbb = $get("cbBubbleCategory");
                        if ((cbb)) { cbb.disabled = true; $("#lblMenuCategory").css({color:"#a0a0a0"});}
                    }
                    $("#colorbox").dxColorBox("instance").option("readOnly", true);
                    $("#menuRowCategory").show();
                }
                $get("lblMenuAltName").innerHTML = data[IDX_BUBBLE_EXTRA].label;
                
                is_context_menu_open = true;                
                
                $("div.context-menu").fadeIn(100);
                $("div.context-menu").css({overflow:"visible"});
                right_click_alt_id = data[IDX_ALT_ID];
            }
        );
        
        $(document).unbind("click");
        //$(document).bind("click", function(event) { hideContextMenu(); }); 
    }

    function drawChart() {
        $("#divToolbar").width(100);
        $("#divColorLegend").height(100);
        $("#bubble_chart").height(100);
        $("#bubble_chart").width(100);

        var w_t = $("#tdToolbar").outerWidth()-2;
        var l_w = $("#tdColorLegend").outerWidth()+2;
        var c_w = w_t - l_w;
        if (c_w > 0) {
            $("#divToolbar").width(w_t-20);
            $("#tdChart").width(c_w - 28);
            $("#bubble_chart").height($("#tdChart").innerHeight()-22);
            $("#bubble_chart").width(c_w-30);
            if ((bubble_chart)) {
                //try {
                setTimeout("bubble_chart.replot({ resetAxes: false });",50);
                //} catch (err) { }            
                highlightFundedDatapoints(bubble_chart);
            } else {
                $('#bubble_chart').empty();
                setTimeout("createChart();", 50);
            }
        }

        var d = $("#divColorLegend");
        if ((d)) { 
            var h_pnl = $("#<%=pnlView.ClientID %>").innerHeight(); 
            var h_ch = $("#bubble_chart").outerHeight(); 
            d.height((h_ch-h_pnl-40)>100?h_ch-h_pnl-40:100); 
            var t = $("#divTableColorLegend");
            var d_h = d.height()- $("#divColorLegendDetails").outerHeight() - $("#lblCategories").outerHeight() - 10;
            if ((t) && (d_h > 0)) t.height(d_h);
        }
    }

    function resetChart() {
        $('#bubble_chart').empty();
        bubble_chart = null;
    }

    function checkButtons()
    {
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var inc = toolbar.findItemByValue("i");
            var dec = toolbar.findItemByValue("d");
            if ((inc)) inc.set_enabled(bubble_mult<2);
            if ((dec)) dec.set_enabled(bubble_mult>0.2);
            var res = toolbar.findItemByValue("r"); // D3309
            if ((res)) res.set_enabled(bubble_mult!=1);
        }
    }

    function getToolbarButton(btn_id) {
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) return toolbar.findItemByValue(btn_id);
        return "undefined";
    }

    function onClickToolbar(sender, args) {
        var button = args.get_item();
//        args.set_cancel(true);    
        if ((button))
        {
            var btn_id = button.get_value();
            switch (btn_id+"") {
                case "0":
                case "1":
                case "2":
                case "3":
                    chart_id = btn_id*1;
                    chart_data = data_all[chart_id];
//                    var l = getToolbarButton("l");
//                    if ((l)) l.set_enabled(chart_data[0][4]!=0 && (chart_data[0][9]));
                    break;
                case "i":
                    bubble_mult*=1.1;
                    break;
                case "d":
                    bubble_mult/=1.1;
                    break;
                case "r":
                    bubble_mult=1.0;
                    sendCommand("action=reset_funded_before"); //A0905
                    break;
                case "l":
                    show_labels = button.get_checked();
                    sendCommand("labels=" + (show_labels ? 1 : 0));
            }

            if (btn_id == "i" || btn_id == "d" || btn_id == "r") {
                InitFixBubbles();
                checkButtons();
                sendCommand("action=bubble&coeff=" + bubble_mult, true);
            }

            resetChart();
            drawChart();
        }
    }

    function InitFixBubbles() {
        for (var i=0; i<chart_data[1].length; i++)
        {
//            data_all[0][1][i][2] = <% =OPT_BUBBLE_SIZE %>*Math.pow(bubble_mult, 2);
            chart_data[1][i][2] = Math.ceil(Math.abs(chart_data[1][i][IDX_ORIG_RADIUS]*Math.pow(bubble_mult, 2)));  // D3309
        }
    }

    function onEditArea(sender, eventArgs)
    {
       var v = eventArgs.get_newValue();
       if (!spin_manual && v!="") {
           var cid = sender._clientID; 
            var idx = -1;
            if (cid == "<% =SpinIDs(0) %>") idx = 5;
            if (cid == "<% =SpinIDs(1) %>") idx = 6;
            if (cid == "<% =SpinIDs(2) %>") idx = 7;
            if (cid == "<% =SpinIDs(3) %>") idx = 8;
            if (idx>0) {
                set_spins = false;
                chart_data[IDX_CHART_NAMES][idx] = v*1;
                resetChart();
                drawChart();
                set_spins = true;
            }
        }
    }
    
    function resetAxis() {
        if (chart_data[IDX_CHART_NAMES].length>9) {
            for (var j=0; j<=3; j++) chart_data[IDX_CHART_NAMES][j+5] = saved_spins[chart_data[IDX_CHART_NAMES][4]][j];
            resetChart();
            spin_manual = true;
            var xi = $find("<% =SpinIDs(0) %>"); if ((xi)) xi.set_value(chart_data[IDX_CHART_NAMES][5]);
            var xa = $find("<% =SpinIDs(1) %>"); if ((xa)) xa.set_value(chart_data[IDX_CHART_NAMES][6]);
            var yi = $find("<% =SpinIDs(2) %>"); if ((yi)) yi.set_value(chart_data[IDX_CHART_NAMES][7]);
            var ya = $find("<% =SpinIDs(3) %>"); if ((ya)) ya.set_value(chart_data[IDX_CHART_NAMES][8]);
            spin_manual = false;
            sendCommand("action=axis_reset&mode=" + chart_id, true);
            drawChart();
        }
    }

    function InitPage() {
        $("button").button();
        InitFixBubbles();
        initColorLegend();      //A0888
        hideLoadingPanel();    //A0888  
        updateSolverMsgVisibility(); //A0890
        //A0891 ===
        $("#divLegend0").css("background-color",COLOR_BECAME_FUNDED);
        $("#divLegend1").css("background-color",COLOR_BECAME_NOT_FUNDED);
        $("#divLegend2").css("background-color",COLOR_BOTH_FUNDED);
        $("#divLegend4").css("background-color",COLOR_FUNDED_PERCENT_CHANGED);
        //A0891 ==
        <%If Not App.ActiveProject.IsRisk Then%>
        disableGlyphButton($("#btnSaveAltsColors"), !$get("cbColorBubbles").checked); //A0900
        <%End If%>
        updateCategoriesTitle(); //A0902
    }

    function onViewAreaClose() {
        sendCommand("action=axis&mode=" + chart_id + "&area=" + chart_data[IDX_CHART_NAMES][5] + ";" + chart_data[IDX_CHART_NAMES][6] + ";" + chart_data[IDX_CHART_NAMES][7] + ";" + chart_data[IDX_CHART_NAMES][8], true);
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var btn =  toolbar.findItemByText("View Area");
            if ((btn)) btn.hideDropDown();
//            resetChart();
//            drawChart();
        }
    }

    function onSetScenario(id) {
        theForm.disabled = true;
        document.body.style.cursor = "wait";
        document.location.href='<% =PageURL(CurrentPageID) %>?page_action=scenario<% =GetTempThemeURI(true) %>&sid='+ id + "&chart=" + chart_id; //A0888
        return false;
    }

    // A0888 ===
    function onSetChart(id) {
        chart_id = id*1;
        sendCommand("action=set_chart&id=" + chart_id);
    }

    function onSetColoringByCategory(checked) {
        var cb = $("#cbCategories");
        if ((cb)) cb.prop('disabled', !checked);
        disableGlyphButton($("#btnSaveAltsColors"), !checked); //A0900
        updateCategoriesTitle(); //A0902
        sendCommand("action=color_by_category&chk=" + (checked ? "1" : "0"));
    }

    function onSetCategory(cat_guid) {
        updateCategoriesTitle(); //A0902
        sendCommand("action=select_category&cat_guid="+ cat_guid);
    }

    function updateCategoriesTitle() { //A0902
        var cb = $get("cbCategories");
        var lbl = $get("lblCategories");        
        var filter_name = "";

        var radios = document.getElementsByName("AltsFilter");
        for (i = 0; i < radios.length; i++) {
            if(radios[i].checked) {
                filter_name = " - " + radios[i].nextSibling.innerHTML;
            }
        }

        if ((lbl)) lbl.innerHTML = "";
        if ((cb) && (cb.selectedIndex >=0)) lbl.innerHTML = cb.options[cb.selectedIndex].innerHTML + filter_name;
    }

    function onSetFilteringMode(value) {
        FundedFilteringMode = value;        
        updateCategoriesTitle();
        sendCommand("action=funded_filter&filter_id="+ value);
    }

    function sendCommand(params, no_loader) {
        params += "&chart=" + chart_id;
        cmd = params;
        if (!(no_loader)) {
            showLoadingPanel();
            //hideContextMenu(); //A0902
        }

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function IsAction(sAction) {
        return cmd.indexOf("action=" + sAction) == 0;
    }

    function syncReceived(params) {
        if ((params) && (IsAction("funded_filter") || IsAction("set_chart") || IsAction("axis_reset") || IsAction("color_by_category") || IsAction("select_category") || IsAction("set_color") || IsAction("set_category") || IsAction("set_multi_category") || IsAction("reset_funded_before"))) {    
            data_all[chart_id] = eval(params);
            chart_data = data_all[chart_id];
            InitFixBubbles();
            initColorLegend();
            updateSolverMsgVisibility(); //A0890
            resetChart();
            drawChart();
        }
        hideLoadingPanel();
        cmd = "";
    }

    function initColorLegend() {
        var d = $("#divColorLegend");
        if ((d)) (ShowColorLegend ? d.show() : d.hide());
        var t = $("#divColorLegend");
        if ((t)) { (ShowColorLegend ? t.show() : t.hide()); t.width(ShowColorLegend ? <%=COL_LEGEND_WIDTH %> : 1); }
        
        tableLegend();
        //datatablesLegend();
        
        var mnu_cb = $get("cbBubbleCategory");
        if ((mnu_cb)) {
            while (mnu_cb.firstChild) {
               mnu_cb.removeChild(mnu_cb.firstChild);
            }
            var categories = chart_data[IDX_CHART_CAT_DATA][IDX_SELECTED_CATEGORY_ITEMS];
            $.each(categories, function(index, val) {
               var opt = document.createElement("option");
               opt.value= val[0]; //category GUID
               opt.innerHTML = htmlEscape(val[1]);
               mnu_cb.appendChild(opt);
            });
        }
    }

    function syncError() {
        hideLoadingPanel();
        DevExpress.ui.notify("<% =ResString("ErrorMsg_ServiceOperation") %>", "error");
    }

    function tableLegend() {
        $("#TableColorLegend").empty();        
        var legends = chart_data[IDX_CHART_CAT_DATA][IDX_COLOR_LEGEND];
        
        // legend table header
        $('#TableColorLegend').append('<tr valign="top"><th colspan="2" class="text small"><%=ResString("lblRACategory")%></th><th class="text small"><span style="float:right;"><%=ResString("tblAltCount")%>&nbsp;</span></th></tr>');
        
        // legend table data
        var total = 0;
        $.each(legends, function(index, val) {                       
            $('#TableColorLegend').append('<tr onmouseover="LegendRowHover(this,1);" onmouseout="LegendRowHover(this,0);" valign="top"><td style="border:0px;vertical-align:middle; padding:2px 3px;"><img src="../../images/ra/legend-15.png" width="15" height="15" title="" border="0" style="background:'+val[IDX_COLOR_LEGEND_COLOR]+'"></td><td style="word-wrap:break-word; border:0px;" title="'+val[IDX_COLOR_LEGEND_CATEGORY_NAME]+'" id="legend_row_'+index+'" class="text small" valign="center">'+htmlEscape(val[IDX_COLOR_LEGEND_CATEGORY_NAME])+'</td><td class="int_cell"><small>'+val[IDX_COLOR_LEGEND_PROJECTS]+'</small></td></tr>');
            total += val[IDX_COLOR_LEGEND_PROJECTS]*1;
        });

        $("#divColorLegendDetails").html("");
        if (legends.length > 0) {
            $("#divColorLegendDetails").html("<span class='text' style='float:right;'><small><b><%=ResString("tblRowTotal")%>&nbsp;"+total+"</b></small></span><br><input type='button' class='text ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' onclick='dlgColorLegendDetails(); return false;' style='float:right; margin-right:0px; padding:3px 6px;' value='&nbsp;&nbsp;<%=ResString("btnDetails")%>&nbsp;&nbsp;'>");
        }
    }

    function dlgColorLegendDetails() {
        dlg_details = $("#divDetailsDialog").dialog({
            autoOpen: true,
            width: 650,
            minWidth: 550,
            maxWidth: 950,
            minHeight: 250,
            maxHeight: mh,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: false,
            bgiframe: true,
            title: "<%=ResString("btnDetails")%>",
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons:  { Close: function() { dlg_details.dialog( "close" ); }},
              open: function() { 
                  $("body").css("overflow", "hidden");
                  var lbl = $get("lblCategories"); 
                  var lbl2 = $get("lblCategoriesDetails"); 
                  if ((lbl) && (lbl2)) lbl2.innerHTML = lbl.innerHTML; 
                  datatablesLegend();
                  $(".ui-widget-content").css({border:0});
                  $(".ui-dialog .ui-dialog-buttonpane").css({marginTop:0});
              },
              close: function() {
                  $("body").css("overflow", "auto");
                  dataTablesDestroy(table);
                  dlg_details = null;                  
              }
        });
        $.ui.dialog.prototype._focusTabbable = $.noop; // force the dialog not to auto-focus elements because it causes Copy function not working
    }

    function datatablesLegend() {
        var show_footer = false;

        var columns = [];
        var dataset = [];        
            
        var legends = chart_data[IDX_CHART_CAT_DATA][IDX_COLOR_LEGEND];
        var legends_len = legends.length;
        
        //init columns headers        
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><%=ResString("tblNo_")%>&nbsp;&nbsp;</span>", "class" : "td_center" });
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><%=ResString("lblRACategory")%></span>", "class" : "td_left", "iDataSort" : 2 });
        columns.push({ "title" : "sort_by_category", "bVisible" : false });
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><%=ResString("tblAltCount")%>&nbsp;&nbsp;&nbsp;</span>", "class" : "td_center", "width": 70 });
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><%=ResString("tblAltCount")%>,&nbsp;&#37;&nbsp;&nbsp;&nbsp;</span>", "class" : "td_right", "sortable" : true, "bVisible" : false });
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><%=ResString("tblCost")%>&nbsp;&nbsp;&nbsp;</span>", "class" : "td_right", "iDataSort" : 7, "width": 70 });
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><%=ResString("tblCost")%>,&nbsp;&#37;&nbsp;&nbsp;&nbsp;</span>", "class" : "td_right", "sortable" : true, "width": 35});
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><%=ResString("tblCost")%>&nbsp;&nbsp;&nbsp;</span>", "class" : "td_right", "width": 70, "bVisible" : false });
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><nobr><%=BenefitName(False,True)%>&nbsp;&nbsp;&nbsp;</nobr></span>", "class" : "td_right", "width": 70 });
        columns.push({ "title" : "<span class='hdr_cell' style='font-weight:bold;font-size:12px;'><nobr><%=BenefitName(False,True)%>,&nbsp;&#37;&nbsp;&nbsp;&nbsp;</nobr></span>", "class" : "td_right", "sortable" : true, "width": 35 });

        //init dataset
        for (var i = 0; i < legends_len; i++) {
            var val = legends[i];
            var row = [];
            row.push(i + 1); // No.
            row.push("<img src='../../images/ra/legend-15.png' width='15' height='15' title='' border='0' style='background:"+val[IDX_COLOR_LEGEND_COLOR]+"; vertical-align:middle;'><span style='word-wrap:break-word;' title='"+val[IDX_COLOR_LEGEND_CATEGORY_NAME]+"' class='text'>&nbsp;"+htmlEscape(val[IDX_COLOR_LEGEND_CATEGORY_NAME])+"</span>");
            row.push((i == legends_len - 1 ? "⁮⁮" : "") + htmlEscape(val[IDX_COLOR_LEGEND_CATEGORY_NAME])); // for the "No category" item adding an invisible char for sorting
            row.push(val[IDX_COLOR_LEGEND_PROJECTS]);
            row.push(val[IDX_COLOR_LEGEND_PROJECTS_PRC]);
            row.push(val[IDX_COLOR_LEGEND_COSTS]);
            row.push(val[IDX_COLOR_LEGEND_COSTS_PRC]);
            row.push(val[IDX_COLOR_LEGEND_COSTS_SORT]);
            row.push(val[IDX_COLOR_LEGEND_BENEFITS]);
            row.push(val[IDX_COLOR_LEGEND_BENEFITS_PRC]);
            dataset.push(row);
        }

        var btn_styles = "ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only";

        table = $('#TableColorLegendDetails').DataTable( {
            data: dataset,
            dom: 'Brti',            
            columns: columns,
            buttons: [
                { extend: "copyHtml5",  exportOptions : { columns: [0,1,3,7,6,8,9]}, className : btn_styles, customize : function (data, config) {return $get("lblCategoriesDetails").innerHTML + "\r\n" + data} }, 
                //{ extend: "csvHtml5",   exportOptions : { columns: [0,1,3,5,6,8,9]}, className : btn_styles },
                //{ extend: "excelHtml5", exportOptions : { columns: [0,1,3,7,6,8,9]}, className : btn_styles },
                { extend: "print",      exportOptions : { columns: [0,1,3,5,6,8,9]}, className : btn_styles } 
            ],
            footer: true,
            ordering:  true,
            paging:    false,            
            stateSave: false,
            searching: false,
            info:      false
        });

        // footer
        var lbl = $get("lblCategories");        
        if (show_footer && (lbl)) {
            var column = table.column(0);
            $(column.footer()).html('<i>*&nbsp;' + lbl.innerHTML + '</i>');
        }

        //$("a.dt-button").css({ height:"15px", padding:"4px 8px", fontSize: "12px" });
        $("a.dt-button").css({ padding:"4px 8px", width:"5em", margin:"5px 4px" });
        $("div.dt-buttons").css({"float":"none"});        

        // put filter name in the header
        //var tHead = document.getElementById('TableColorLegendDetails').tHead;
        //var tr = document.createElement('tr');
        //var th = document.createElement('th');
        //var lbl = $get("lblCategories");        
        //if ((lbl)) th.innerHTML = "<h6>"+lbl.innerHTML+"</h6>";
        //th.colSpan = tHead.children[0].cells.length;
        //tr.appendChild(th);
        //tHead.insertBefore(tr, tHead.children[0]);
        ////tHead.appendChild(tr);
    }

    function LegendRowHover(r, hover) {
        if (r.style.background != "#ffff99") r.style.background = (hover == 1 ? "#f0f0fa" : "#ffffff");
    }

    function onViewMenuOkClick() {
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var btn =  toolbar.findItemByText("View");
            if ((btn)) btn.hideDropDown();
        }
    }
    // A0888 ==

    function updateSolverMsgVisibility() { //A0890
        var m = $("#divSolverState");
        if ((m)) (FundedFilteringMode > 0 ? m.show() : m.hide()); 
    }
    
    function highlightFundedDatapoints(plot) { //A0891
        // "F" - funded, "+" - became funded, "-" - stopped being funded, "%" - funded % changed
        var drawingCanvas = $(".jqplot-highlight-canvas")[0];
        if (!(drawingCanvas)) return false;

        var ctx = drawingCanvas.getContext('2d');
        ctx.fillStyle = COLOR_FUNDED_STATE;
        ctx.font = "bold 18px serif";        
        
        ctx.strokeStyle ="#ccc";

        for (var s=0; s<plot.series.length; s++) {
            var data = plot.series[s].data;       
            var data_len = data.length;
            for (var i=0;i<data_len;i++) {
                var x = plot.axes.xaxis.series_u2p(data[i][0]);
                var y = plot.axes.yaxis.series_u2p(data[i][1]);
                var radius = plot.series[s].gridData[i][2];

                if (s==1 && radius>3) { // D3309
                    ctx.lineWidth = (radius<10 ? 1 : (radius>30 ? 3 : 2));
                    ctx.beginPath();
                    ctx.strokeStyle = COLOR_NEGATIVE_RADIUS;
                    ctx.arc(x,y,radius-ctx.lineWidth-1,0,Math.PI*2,false);
                    ctx.closePath();
                    ctx.stroke();
                    ctx.lineWidth = (radius<10 ? 1 : 2);
                    ctx.beginPath();
                    ctx.strokeStyle = COLOR_NEGATIVE_RADIUS;
                    ctx.arc(x,y,radius,0,Math.PI*2,false);
                    ctx.closePath();
                    ctx.stroke();
                }

                if ((data[i].length>IDX_FUNDED_STATE) && (data[i][IDX_FUNDED_STATE] != <% =FundedStates.fsBothNotFunded %>)) { //if the marker data exists then show it
                    switch (data[i][IDX_FUNDED_STATE]*1) {
                        case <% =FundedStates.fsBecameFunded %>:
                            ctx.strokeStyle = COLOR_BECAME_FUNDED;
                            break;
                        case <% =FundedStates.fsBecameNotFunded %>:
                            ctx.strokeStyle = COLOR_BECAME_NOT_FUNDED;
                            break;
                        case <% =FundedStates.fsBothFunded %>:
                            ctx.strokeStyle = COLOR_BOTH_FUNDED;
                            break;
                        case <% =FundedStates.fsBothNotFunded %>:
                            //ctx.strokeStyle = COLOR_BOTH_NOT_FUNDED;
                            break;
                        case <% =FundedStates.fsFundedPercentChanged %>:
                            ctx.strokeStyle = COLOR_FUNDED_PERCENT_CHANGED;
                            break;
                    }

                    ctx.lineWidth=Math.round(1+bubble_mult);
                    ctx.beginPath();
                    ctx.arc(x,y,radius,0,Math.PI*2,false);
                    ctx.closePath();
                    ctx.stroke();
                    //ctx.fillText(data[i][IDX_FUNDED_STATE_CHAR], x, y);
                }
            }
        }
    }

    function onSaveAltsColors() { //A0901
        var cb = $get("cbCategories");
        if ((cb)) dxConfirm(replaceString('{0}', cb.options[cb.selectedIndex].innerHTML, '<%=ResString("msgSureSaveAltsColors") %>'), "sendCommand('action=set_alts_color_by_category');", ";");
    }

    function onColorPickerChange(e) {
        if (!e.component.option("disabled")) {
            var oc = e.previousValue;
            var nc = e.value;
            if (oc != nc) {
                sendCommand("action=set_color&alt_id="+right_click_alt_id+"&color="+nc);
            }            
            //$("div.context-menu").hide(200); is_context_menu_open = false; right_click_alt_id = "";
        }
    }

    function onMenuCategoryChange(id) {
        sendCommand("action=set_category&alt_id="+right_click_alt_id+"&cat_id="+id);
    }

    function hideContextMenu() {
        $("div.context-menu").hide(); 
        is_context_menu_open = false; 
        right_click_alt_id = "";
    }

    function onMenuOkClick() {
        return;
        var ids = "";
        $('input.cat_item_chk:checked').each(function() { ids += (ids==""?"":";") + $(this).val(); });
        sendCommand("action=set_multi_category&alt_id="+right_click_alt_id+"&cat_ids="+ids);
    }

    function InitColorPicker() {
        $("#colorbox").dxColorBox({
            acceptCustomValue: true,
            onValueChanged: onColorPickerChange,
            readOnly: false,
            visible: true
        });        
    }

    resize_custom = drawChart;

    $(document).ready(function () { 
        InitPage(); 
        InitColorPicker();
        ResetRadToolbar();
        setTimeout('drawChart();', 150);
    });

    resize_custom = function (force_redraw) { drawChart(); };

</script>
<div style="display:none;" id="bubble_tooltip" class='bubble_tooltip text'></div>

<table cellpadding="0" cellspacing="1" border="0" class="whole" style='width:99%'>
<tr style="height:3em" valign="bottom" align="center">
    <td class='text' colspan='2' id="tdToolbar"><div id='divToolbar' style='overflow:hidden'><telerik:RadToolBar ID="RadToolBarMain" runat="server" CssClass="dxToolbar" Skin="Default" Width="100%" OnClientButtonClicked="onClickToolbar" AutoPostBack="false" EnableViewState="false">
    <Items>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Enabled="false">
            <ItemTemplate><span class='toolbar-label'>&nbsp;<%=ResString("lblScenario") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbOptScenario" Enabled="false">
            <ItemTemplate><% =GetScenarios()%></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton IsSeparator="true" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Enabled="false" ImageUrl="~/App_Themes/EC09/images/chart-20.png">
            <ItemTemplate><span class='toolbar-label'>&nbsp;<%=ResString("lblChart") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbOptChart" Enabled="false">
            <ItemTemplate><% =GetCharts()%></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton IsSeparator="true" />
        <%--<telerik:RadToolBarDropDown runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/filter20.png" Text="View">
            <Buttons>
                <telerik:RadToolBarButton runat="server" CheckOnClick="false" Checked="false" HoveredCssClass="toolbarcustom">
                    <ItemTemplate><div class='toolbarcustom' style="padding:1ex 1em;margin-left:-2px;">
                    <table cellpadding='0' cellspacing='0' class='text' style='margin-left:20px;'><tr>
                    <td valign='top' style='padding-right:12px'>
                        <nobr><input type='radio' class='radio' name='AltsFilter' id='rbAll' value='0' <%=IIf(Cint(FundedFilteringMode) = 0," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbAll'>All</label></nobr><br />
                        <nobr><input type='radio' class='radio' name='AltsFilter' id='rbFunded' value='1' <%=IIf(Cint(FundedFilteringMode) = 1," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbFunded'>Funded</label></nobr><br />
                        <nobr><input type='radio' class='radio' name='AltsFilter' id='rbPartial' value='2' <%=IIf(Cint(FundedFilteringMode) = 2," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbPartial'>Partially funded</label></nobr><br />
                        <nobr><input type='radio' class='radio' name='AltsFilter' id='rbFully' value='3' <%=IIf(Cint(FundedFilteringMode) = 3," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbFully'>Fully funded</label></nobr><br />
                        <nobr><input type='radio' class='radio' name='AltsFilter' id='rbNotFunded' value='4' <%=IIf(Cint(FundedFilteringMode) = 4," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbNotFunded'>Not funded</label></nobr>
                    </td>
                    <td valign='top' style='padding-left:32px; border-left:1px solid #f0f0f0;'>
                        <nobr><label style='margin-left:-20px;'><input type="checkbox" class='checkbox' id='cbColorBubbles' <%=IIf(AttributesList.Count=0, " disabled='disabled' ","") %>  <% =IIf(ColorBubblesByCategory AndAlso AttributesList.Count > 0, " checked='checked' ","") %> onclick='ShowColorLegend = this.checked; onSetColoringByCategory(this.checked);'>Color Bubbles by Category<br />and View Color Legend</label></nobr><br />
                        <nobr><br />Category:<br /><% =GetCategories()%></nobr>
                    </td></tr>
                    <tr><td colspan="2" valign="bottom" style='text-align:right;'>
                        <input type='button' class='button' id='btnViewOk' value='OK' onclick='onViewMenuOkClick(); return false;' />
                    </td></tr>                    
                    </table></div>
                    </ItemTemplate>
                </telerik:RadToolBarButton>
            </Buttons>
        </telerik:RadToolBarDropDown>--%>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/labels_20.png" AllowSelfUnCheck="True" CheckOnClick="True" Checked="False" Text="Show Labels" Value="l"/>
        <telerik:RadToolBarDropDown runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/rulers-20.png"  Text="View Area">
            <Buttons >
                <telerik:RadToolBarButton runat="server" CheckOnClick="false" Checked="false">
                    <ItemTemplate><div class='toolbarcustom' style="padding:1ex 1em 1ex 12px;margin-left:-2px;"><table border='0' cellspacing='0' cellpadding='2'>
                    <tr class='text' valign="middle">
                        <td style='width:50px'><nobr><b>X</b>&nbsp;&nbsp;&nbsp;&nbsp;<% =ResString("lblRAMin")%>:</nobr></td>
                        <td><telerik:RadNumericTextBox runat="server" id="ntbXMin" Type="Number" Width="70" IncrementSettings-Step="0.1" CssClass="as_number" ShowSpinButtons="True" ClientEvents-OnValueChanged="onEditArea" OnInit="ntbXMin_Init"/></td>
                        <td style="padding-left:1ex"><% =ResString("lblRAMax")%>:</td>
                        <td><telerik:RadNumericTextBox runat="server" id="ntbXMax" Type="Number" Width="70" IncrementSettings-Step="0.1" CssClass="as_number" ShowSpinButtons="True" ClientEvents-OnValueChanged="onEditArea" OnInit="ntbXMin_Init"/></td>
                    </tr>
                    <tr class='text' valign="middle">
                        <td style='width:50px'><nobr><b>Y</b>&nbsp;&nbsp;&nbsp;&nbsp;<% =ResString("lblRAMin")%>:</nobr></td>
                        <td><telerik:RadNumericTextBox runat="server" id="ntbYMin" Type="Number" Width="70" IncrementSettings-Step="0.1" CssClass="as_number" ShowSpinButtons="True" ClientEvents-OnValueChanged="onEditArea" OnInit="ntbXMin_Init"/></td>
                        <td style="padding-left:1ex"><% =ResString("lblRAMax")%>:</td>
                        <td><telerik:RadNumericTextBox runat="server" id="ntbYMax" Type="Number" Width="70" IncrementSettings-Step="0.1" CssClass="as_number" ShowSpinButtons="True" ClientEvents-OnValueChanged="onEditArea" OnInit="ntbXMin_Init"/></td>
                    </tr>
                    <tr>
                        <td colspan="4" align="center" style='padding-left:24px; padding-top:6px;'><input type='button' class='button' id='btnApplyAxis' value='<% =ResString("btnApply")%>' onclick='onViewAreaClose(); return false;' />&nbsp;<input type='button' class='button' id='btnResetAxis' value='<% =ResString("btnReset")%>' onclick='resetAxis(); return false;' /></td>
                    </tr>
                    </table></div></ItemTemplate>
                </telerik:RadToolBarButton>
            </Buttons>
        </telerik:RadToolBarDropDown>
        <%--<telerik:RadToolBarButton runat="server" EnableViewState="false"  ImageUrl="~/App_Themes/EC09/images/reload-20.png" Text="Refresh" Value="reset" />--%>
        <telerik:RadToolBarButton IsSeparator="true" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Enabled="false">
            <ItemTemplate><span class='toolbar-label'>&nbsp;<%=ResString("lblBubbleSize") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/bubble_i-20.png" Value="i" Text="Increase" ToolTip="Increase bubble size" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/bubble_d-20.png" Value="d" Text="Decrease" ToolTip="Decrease bubble size" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/bubble_r-20.png" Value="r" Text="Reset" ToolTip="Reset bubble size to default" />
    </Items>
</telerik:RadToolBar></div>
<h5 style="padding-top:1ex; padding-bottom:0px;" title="<% =SafeFormString(RA.Scenarios.ActiveScenario.Description) %>"><% = String.Format(ResString("lblRAPlotAltsTitle"), SafeFormString(ShortString(RA.Scenarios.ActiveScenario.Name, 65)))%></h5>
<h6 id='PlotTitle' style="padding-bottom:0px;"></h6></td>
</tr>
<tr valign="middle" align="center" style="height:99%">
    <td id='tdChart' class='text' style='width:95%' valign='top'>
        <div id='divSolverState'><% =SolverStateHTML(RA.Solver)%></div>
        <div id="bubble_chart" class='whole' style="overflow:hidden;"><p style="margin-top:6em;">Please wait...</p></div>
    </td>
    <td id="tdColorLegend" style="width:<% =COL_LEGEND_WIDTH+10 %>px; padding-top:4px; padding-bottom:4px; margin-right:10px; overflow:hidden; padding-right:4px;" valign="top" align="left">
        <fieldset class="legend" style="padding-bottom:3px; padding-top:0px; width:auto;" runat="server" id="pnlView">
        <legend class="text legend_title">&nbsp;<% =ResString("lblRAViewCategories")%>:&nbsp;</legend>
        <table cellpadding='0' cellspacing='0' class='text' style='margin-left:0px;'><tr>
        <td valign='top' style='padding-bottom:12px;'>
            <nobr><input type='radio' class='radio' name='AltsFilter' id='rbAll' value='0' <%=IIf(Cint(FundedFilteringMode) = 0," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbAll'><% =ResString("lblRAViewCatAll")%></label></nobr><br />
            <nobr><input type='radio' class='radio' name='AltsFilter' id='rbFunded' value='1' <%=IIf(Cint(FundedFilteringMode) = 1," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbFunded'><% =ResString("lblRAViewCatFunded")%></label></nobr><br />
            <nobr><input type='radio' class='radio' name='AltsFilter' id='rbPartial' value='2' <%=IIf(Cint(FundedFilteringMode) = 2," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbPartial'><% =ResString("lblRAViewCatPartially")%></label></nobr><br />
            <nobr><input type='radio' class='radio' name='AltsFilter' id='rbFully' value='3' <%=IIf(Cint(FundedFilteringMode) = 3," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbFully'><% =ResString("lblRAViewCatFully")%></label></nobr><br />
            <nobr><input type='radio' class='radio' name='AltsFilter' id='rbNotFunded' value='4' <%=IIf(Cint(FundedFilteringMode) = 4," checked='checked' ","") %> onclick='onSetFilteringMode(this.value);'><label for='rbNotFunded'><% =ResString("lblRAViewCatNotFunded")%></label></nobr>
        </td></tr>
        <%If Not App.ActiveProject.IsRisk Then%>
        <tr><td valign='top' style='padding:5px 0px;border-top:1px solid #f0f0f0;'>
            <div style='margin-bottom:4px'><nobr><label><input type="checkbox" class='checkbox' id='cbColorBubbles' <%=IIf(AttributesList.Count=0, " disabled='disabled' ","") %>  <% =IIf(ColorBubblesByCategory AndAlso AttributesList.Count > 0, " checked='checked' ","") %> onclick='ShowColorLegend = this.checked; onSetColoringByCategory(this.checked);'><% =ResString("lblRAPlotAltsColorByCat")%></label></nobr></div>
            <nobr><% =ResString("lblRAAttributes")%>:<br /><% =GetCategories()%><input type='button' class='btn_glyph22 btn_glyph22_palette' id='btnSaveAltsColors' onclick="onSaveAltsColors(); return false;" title="<%=ResString("titleSaveAltsColors") %>" style='margin-left:2px; vertical-align:top;margin-top:1px;height:20px;width:20px;' /></nobr>
        </td></tr><%End If%>
        </table></fieldset>
        <div id="divColorLegend" style="width:<%=COL_LEGEND_WIDTH%>px; height:100%; overflow:auto;border:1px solid #6699cc; text-align:left; padding:4px; margin-top:1ex;"><h6 id='lblCategories'></h6><div id="divTableColorLegend" style="overflow:auto;"><table id="TableColorLegend" class='text grid' cellspacing="0" cellpadding="1" style='width:100%; border-collapse:collapse;'></table></div><div id="divColorLegendDetails" style="float:right; margin: 5px;"></div></div>
        <!--<div id="divColorLegend" style="width:< %=COL_LEGEND_WIDTH% >px; height:100%; overflow:auto;border-right:1px solid #f0f0f0; border-bottom:1px solid #f0f0f0; background:#fafafa; text-align:left; padding:4px; margin-top:1ex;"><h6 id='lblCategories'></h6><table id="TableColorLegend" class='text grid' style='border-collapse: collapse;' cellspacing='0'></table></div>-->
    </td>
</tr>
<% If ShowChanges Then%>
<tr>
    <td class='text' id="tdChangesLegend" colspan='2'>
        <center>
        <fieldset class="legend" style="width: auto; padding: 5px 15px 5px 15px; text-align: left;">
        <legend class="text legend_title">&nbsp;<%="Changes in scenario """ + ShortString(SafeFormString(RA.Scenarios.ActiveScenario.Name), 50) + """ compared to scenario """ + ShortString(SafeFormString(BeforeScenarioName), 50) + """:"%> &nbsp;</legend>
        <table cellpadding="0" cellspacing="1" border="0" style='width:100%'>
            <tr>
                <td style='padding-left:6px;'><div id='divLegend0' style='width:20px;height:4px;line-height:0;font-size:0;'></div></td><td style='padding-left:2px;'><small><% =ResString("lblRALegendBecameFunded")%></small></td>
                <td style='padding-left:6px;'><div id='divLegend1' style='width:20px;height:4px;line-height:0;font-size:0;'></div></td><td style='padding-left:2px;'><small><% =ResString("lblRALegendStopFunded")%></small></td>
                <td style='padding-left:6px;'><div id='divLegend2' style='width:20px;height:4px;line-height:0;font-size:0;'></div></td><td style='padding-left:2px;'><small><% =ResString("lblRALegendFundedBoth")%></small></td>
                <td style='padding-left:6px;'><div id='divLegend4' style='width:20px;height:4px;line-height:0;font-size:0;'></div></td><td style='padding-left:2px;'><small><% =ResString("lblRALegendFundedDiff")%></small></td>
            </tr>
        </table>
        </fieldset>
        </center>
    </td>
</tr>
<%End If%>
</table>
<div class='context-menu text' style='display:none; padding:5px;overflow:visible;background-color:#f5f5f5;' <%--onclick='if (event.stopPropagation) {event.stopPropagation();} else {event.cancelBubble = true;}'--%>>
    <table border='0' cellpadding='0' cellspacing='0' class='text'>
    <tr><td colspan='2' align="left" style='padding:4px 4px;'><h6 id='lblMenuAltName' style='text-align:left'></h6></td></tr>
    <tr id='menuRowColor'>
        <td style='width:75px;'><span style='float:left;text-align:left;margin-top:2px;margin-left:10px;'>&nbsp;<%=ResString("lblColor")%>:&nbsp;</span></td>
        <td style='width:125px;'><div id='colorbox'></div></td>
    </tr>
    <tr id='menuRowCategory'>
        <td style='width:75px;'><span style='float:left;display:block;text-align:left;margin-top:2px;white-space:normal;width:75px;' id='lblMenuCategory'></span></td>
        <td align='left'><select id='cbBubbleCategory' size='1' style='width:200px;margin-top:2px;' onclick='return false;' onchange='onMenuCategoryChange(this.value);'></select><div id='divMultiCategories' style='width:125px;padding:2px 2px;'></div></td>
    </tr>
    <tr id='menuRowButtons'>
        <td>&nbsp;</td>
        <td align='left'>
            <input type='button' class='button' id='btnMenuOK' value='<%=ResString("btnOK") %>' onclick='onMenuOkClick(); hideContextMenu(); return false;' style='margin-left:2px;' />
            <%--<input type='button' class='button' id='btnMenuCancel' value='<%=ResString("btnCancel") %>' onclick='hideContextMenu(); return false;' />&nbsp;--%>
        </td>        
    </tr>
    </table>
</div>

<div id="divDetailsDialog" style="display:none; text-align:center">
    <h6 id='lblCategoriesDetails'></h6>
    <table id="TableColorLegendDetails" class='text grid' cellspacing="0" cellpadding="1" style='width:100%; border-collapse:collapse;'><!--<tfoot><th colspan='10' style='text-align:left;'></th></tfoot>--></table>
</div>

</asp:Content>