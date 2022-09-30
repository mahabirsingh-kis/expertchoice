/* Hierarchical Pie (Donut) Chart by DA */

var CHART_GAP   = 50;
var CHART_FONT  = "14px serif";
var LABEL_COLOR = "#212121";

var levelRadius     = 50; //LevelRadius
var START_ANGLE = -90.0;

var centerX;
var centerY;

var IDX_HPIE_LABEL     = 0;
var IDX_HPIE_VALUE     = 1;
var IDX_HPIE_NODE_ID   = 2;
var IDX_HPIE_PARENT_ID = 3;
var IDX_HPIE_TITLE     = 4;
var IDX_HPIE_SEGMENTS  = 5;

var maxLevel = 1;

var ColorID = 0;
var pieColors = [];
var show_labels = false;
var hasNonZeroData = false;

var ctx;
var data; // [[label, value, nodeID, parentNodeID, Title, Segments], [...] ...], where Segments is {}

function hPiePerformLayout(context, width, height, arr, colorPalette, showLabels) {
    //console.log("start hPie layout");
    data = arr;
    ctx = context;

    ctx.strokeStyle = LABEL_COLOR;
    ctx.fillStyle   = LABEL_COLOR;
    ctx.lineWidth   = "2";
    ctx.font = CHART_FONT;

    centerX = Math.floor(width  / 2);
    centerY = Math.floor(height / 2);

    hasNonZeroData = false;
    pieColors = colorPalette;
    show_labels = showLabels;

    ColorID = 0;
    maxLevel = 1;    
        
    generateSegments(0, 1, "");

    levelRadius = Math.floor(((height / 2) - CHART_GAP - 6) / maxLevel);
        
    var goalTitle = data[0][IDX_HPIE_LABEL];
    wrapText(ctx, goalTitle, centerX - CHART_GAP * Math.SQRT2 / 2, centerY - CHART_GAP * Math.SQRT2 / 2, CHART_GAP * 2, 10, true);

    if (levelRadius <= 0) levelRadius = 30;
    ctx.clearRect(0, 0, width, height);

    var MaxAngle = 269.999;

    var res_segments = [];
    RecourseLayoutSegments(0, MaxAngle, START_ANGLE, 360, res_segments);


    return res_segments;
}

function hasChildren(node) {
    var data_len = data.length;
    for (var i=0;i<data_len;i++) {
        if (data[i][IDX_HPIE_PARENT_ID] == node[IDX_HPIE_NODE_ID]) return true;
    }
    return false
}

function getChildren(node) {
    var retVal = [];
    var data_len = data.length;
    for (var i=0;i<data_len;i++) {
        if (data[i][IDX_HPIE_PARENT_ID] == node[IDX_HPIE_NODE_ID]) retVal.push(data[i]);
    }
    return retVal
}

function getNodeIndexById(id) {
    var data_len = data.length;
    for (var i = 0; i < data_len; i++) {
        if (data[i][IDX_HPIE_NODE_ID] == id) return i;
    }
    return -1;
}

function generateSegments(id, level, segmentColor) {
    data[id].push([]);
    var children = getChildren(data[id]);    
    if ((children) && (children.length > 0)) {                
        var SumPercentage = 0;
        for (var i=0;i<children.length;i++) {
            var child = children[i];
            var segment = {nodeId:child[IDX_HPIE_NODE_ID]};
            segment.level = level;
            segment.node_id = child[IDX_HPIE_NODE_ID];
            if (level > maxLevel) maxLevel = level;

            if (segmentColor == "") {
                segment.color = pieColors[ColorID % pieColors.length];
                ColorID += 1;
            } else {
                segment.color = segmentColor
            }

            segment.origColor = segment.color;
            segment.percentage = data[id][IDX_HPIE_VALUE];
            segment.tooltip = data[id][IDX_HPIE_TITLE];
            segment.label = (show_labels ? child[IDX_HPIE_LABEL] : ""); 

            SumPercentage += segment.percentage
            data[id][IDX_HPIE_SEGMENTS].push(segment);
            generateSegments(getNodeIndexById(child[IDX_HPIE_NODE_ID]), level + 1, ColorBrightness(segment.color, 0.4));            
        }
        //normalize segments percentage, sum = 100%
        if (SumPercentage > 0) {
            for (var k = 0; k < data[id][IDX_HPIE_SEGMENTS].length; k++) {
                data[id][IDX_HPIE_SEGMENTS][k].percentage = 100 * (segment.percentage / SumPercentage);
            }
        }
    }
    //alert(node[IDX_HPIE_TITLE] + " : " + node[IDX_HPIE_SEGMENTS].length);
}

function ColorBrightness(Clr, f) { //-1 < F < 1
    //console.log(Clr);   
    var r = parseInt(Clr.charAt(1) + Clr.charAt(2), 16);
    var g = parseInt(Clr.charAt(3) + Clr.charAt(4), 16);
    var b = parseInt(Clr.charAt(5) + Clr.charAt(6), 16);
    //console.log(r+":"+g+":"+b);
    
    if (f < 0) {
        f = f + 1;
        r = r * f;
        g = g * f;
        b = b * f;
    } else {
        r = Math.floor((255 - r) * f + r);
        g = Math.floor((255 - g) * f + g);
        b = Math.floor((255 - b) * f + b);
    }

    if (r > 255) r = 255;
    if (g > 255) g = 255;
    if (b > 255) b = 255;

    var hex = "#" + r.toString(16) + g.toString(16) + b.toString(16);
    //console.log(hex);
    return hex
}

function RecourseLayoutSegments(id, MaxAngle, StartAngle, TotalAngle, res_segments) {
    var node = data[id];
    var children = getChildren(node);
    //alert("id="+id+", segments:"+node[IDX_HPIE_SEGMENTS]);
    if ((node[IDX_HPIE_SEGMENTS])) {
        for (var i=0; i<node[IDX_HPIE_SEGMENTS].length; i++) {
            var segment = node[IDX_HPIE_SEGMENTS][i];
            if ((segment.percentage > 0) && (!hasNonZeroData)) hasNonZeroData = true;

            var endAngle = Math.min(StartAngle + TotalAngle * segment.percentage / 100, MaxAngle);
                                    
            //alert("var endAngle = Math.min("+StartAngle +" + "+ TotalAngle +" * "+ segment.percentage + "/ 100, MaxAngle");            
            LayoutSegment(StartAngle, endAngle, segment)

            res_segments.push(segment);

            RecourseLayoutSegments(getNodeIndexById(segment.nodeId), endAngle, StartAngle, endAngle - StartAngle, res_segments)

            StartAngle = endAngle
        }
    }
}

function GetCircumferencePoint(angle, radius) {
    var angleRad = (Math.PI / 180.0) * angle;
    var _x = centerX + radius * Math.cos(angleRad);
    var _y = centerY + radius * Math.sin(angleRad);
    return {x:_x, y:_y}
}

function LayoutSegment(startAngle, endAngle, segment) {
    
    if (!(segment)) return false;

    segment.startAngle = startAngle;
    segment.endAngle   = endAngle;

    var pieRadius = CHART_GAP + levelRadius * segment.level
    var gapRadius = pieRadius - levelRadius

    var A = GetCircumferencePoint(startAngle, pieRadius);
    var B = GetCircumferencePoint(startAngle, gapRadius);
    var C = GetCircumferencePoint(endAngle, gapRadius);
    var D = GetCircumferencePoint(endAngle, pieRadius);

    var isReflexAngle = Math.abs(endAngle - startAngle) > 180;

    ctx.fillStyle = segment.color;
    ctx.strokeStyle = "#ccc";

    var legend_cell = $("#boxLegendColor" + segment.node_id + ":first");
    if ((legend_cell)) legend_cell.css("background-color", segment.color);

    var startAngleR = startAngle * Math.PI / 180;
    var endAngleR   =   endAngle * Math.PI / 180;

    ctx.beginPath();
    ctx.moveTo(A.x, A.y);
    ctx.lineTo(B.x, B.y);
    ctx.arc(centerX, centerY, gapRadius, startAngleR, endAngleR, false); // C
    ctx.lineTo(D.x, D.y);
    ctx.arc(centerX, centerY, pieRadius, endAngleR, startAngleR, true);

    ctx.stroke();
    ctx.fill();
    ctx.closePath();

    if (show_labels) {
        segment.midAngle = startAngle + ((endAngle - startAngle) / 2.0);
        segment.midAngleR = startAngleR + ((endAngleR - startAngleR) / 2.0);
        
        var inRadius  = pieRadius - levelRadius / 2;
        var pos = GetCircumferencePoint(segment.midAngle, inRadius);
                
        segment.IsFlippedLabel = (segment.midAngle < -90) || (segment.midAngle > 90);
        segment.metrics = ctx.measureText(segment.label);
        
        //console.log(Math.floor(pos.x - segment.metrics.width / 2.0) + "  -----  " + Math.floor(pos.y - 8));

        //Rotate(segment);
        ctx.fillStyle = LABEL_COLOR;
        ctx.fillText(segment.label, Math.floor(pos.x - segment.metrics.width / 2.0), Math.floor(pos.y + 4));
    }
}

function Rotate(segment) {    
    if (segment.level > 1) {
        //ctx.save();        
        //ctx.translate(segment.metrics.width / 2, segment.metrics.height / 2);
        ctx.rotate( ((segment.level > 1) && (segment.IsFlippedLabel) ? segment.midAngle + 2 * Math.PI : segment.midAngle) );
    } else { return 0 }
}