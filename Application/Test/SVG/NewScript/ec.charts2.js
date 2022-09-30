/* javascript plug-ins for advanced animated SVG charts by SL */
const ecChartsDefaultOptions = {
    animateOnResize: true,
    backgroundColor: '#ffffff', //'#ffffaa',
    dataSource: null,
    debugMode: false,
    expr: {
        id: 'ID',
        pid: 'ParentNodeID',
        name: 'Name',
        color: 'Color',
    },
    height: 200,
    rootValue: 1,
    shadeColorsLevel: 1,
    shadeColorsRate: 0.4,
    width: 300,
};

var debugStrokeStyle = { width: 0.5, color: 'red' };

var ecChartsDefaultOptionsList = ['animateOnResize', 'backgroundColor'];

(function ($) {
    $.widget('expertchoice.ecCharts2', {
        options: ecChartsDefaultOptions,
        _create: function () {
            var opt = this.options;
            opt.divCanvas = this.element[0];
            opt.divCanvas.innerHTML = '';
            $(opt.divCanvas).addClass('widget-created');
            this.redraw();
        },
        _destroy: function () {
            if ((this.options) && (this.options.divCanvas)) $(this.options.divCanvas).removeClass('widget-created').html('');
        },
        getChangedOptions: function () {
            var opt = this.options;
            var opts = ecChartsDefaultOptions;
            var curOptions = {};
            for (var i = 0; i < ecChartsDefaultOptionsList.length; i++) {
                var prop = ecChartsDefaultOptionsList[i];
                if (Object.prototype.hasOwnProperty.call(opts, prop)) {
                    if (opts[prop] !== opt[prop]) {
                        curOptions[prop] = opt[prop];
                    }
                }
            }
            return curOptions;
        },
        resize: function (width, height) {
            var opt = this.options;
            var w, h, w2, h2, caw, cah, cacx, cacy, tw, th, tcx, tcy, tm, tr;
            w = Math.round(width);
            h = Math.round(height);
            w2 = w/2;
            h2 = h/2;
            caw = w;
            cah = h;
            cacx = w2;
            cacy = h2;
            opt.width = w;
            opt.height = h;
            opt.context.addClass('scroll').css('height', h + 'px').css('width', w + 'px');
            opt.svg.background.size(w, h).center(w2, h2);
            if (opt.debugMode) {
                dg = opt.svg.grpDebug;
                dg.lineCenterV.plot(w2, 0, w2, h);
                dg.lineCenterH.plot(0, h2, w, h2);
                //dg.txtSize.text(w + 'x' + h);
                //var bb = dg.txtSize.bbox();
                //dg.txtSize.x(bb.height).y(bb.height);
            };
            this._resizeHierarchy(opt.dataSource.objs[opt.rootValue], {x:0,y:0,w:w,h:h});
        },
        redraw: function () {
            var opt = this.options;
            $('#' + opt.divCanvas.id).empty();
            opt.width = opt.divCanvas.parentNode.offsetWidth;
            opt.height = opt.divCanvas.parentNode.clientHeight;
            $('<div id="' + opt.divCanvas.id + '_main"></div>').appendTo( '#' + opt.divCanvas.id );
            opt.context = SVG().addTo('#' + opt.divCanvas.id +'_main').size(opt.width, opt.height);
            opt.svg = {};
            this._draw();
        },
        _draw: function () {
            var opt, e, ctx, w, h, y;
            opt = this.options;
            e = opt.expr;
            ctx = opt.context;
            w = opt.width;
            h = opt.height;
            opt.svg.background = ctx.rect().fill(opt.backgroundColor);
            if (opt.debugMode) {
                opt.svg.background.stroke(debugStrokeStyle);
                dg = ctx.group();
                opt.svg.grpDebug = dg;                
                dg.lineCenterV = ctx.line().stroke(debugStrokeStyle);
                dg.lineCenterH = ctx.line().stroke(debugStrokeStyle);
                //dg.txtSize = ctx.text('');
                opt.svg.chartArea = ctx.rect().fill('transparent').stroke(debugStrokeStyle);
            };
            opt.svg.grpObjs = ctx.group();
            this._drawHierarchy(opt.dataSource.objs[opt.rootValue], {x:0,y:0,w:w,h:h});
            this.resize(w, h);
        },

        _setOption: function (key, value) {
            var opt = this.options;
            this._super(key, value);
            if (key == 'dataSource') {

            }
        },
        _setOptions: function (options) {
            this._super(options);
        },
        _getChildren(node) {
            var opt, e, retVal;
            opt = this.options;
            e = opt.expr;
            retVal = [];
            for (const [id, obj] of Object.entries(opt.dataSource.objs)) { 
                if (obj[e.pid] == node[e.id]) {
                    retVal.push(obj)
                }
            }
            return retVal;
        },
        ellipsisText: function (txt, maxWidth) {
            var text = txt.text();
            if (txt.bbox().width < maxWidth) { return txt };
            var words = text.split(" ");
            var result = text;
            while ((txt.text(result).bbox().width > maxWidth) && (words.length > 1)) {
                words.pop();
                result = words.join(" ");
            };
            while ((txt.text(result + "\u2026").bbox().width > maxWidth) && result.length > 0) {
                result = result.substring(0, result.length - 2);
            }
            return txt;
        },
        _drawHierarchy(node) {
            if ((node)) {
                var opt, e, children, txtTitle, tooltip, grpNode, color, pNode;
                opt = this.options;
                e = opt.expr;
                tooltip = node[e.name];
                pNode = opt.dataSource.objs[node[e.pid]];
                grpNode = opt.svg.grpObjs.group();
                //if ((pNode)) {grpNode = pNode.grpNode.group()} else {grpNode = opt.svg.grpObjs.group();};
                node.grpNode = grpNode;
                color = (node.Level > opt.shadeColorsLevel ? colorBrightness(pNode[e.color], opt.shadeColorsRate) : node[e.color]);
                node[e.color] = color;
                node.rectNode = grpNode.rect().fill(color).radius(5).stroke(opt.backgroundColor);
                grpNode.add(SVG('<title>' + tooltip + '</title>', true));
                txtTitle = grpNode.text(node[e.name]).fill(getContrastColor(color));
                node.txtTitle = txtTitle;
                if (node.Level > 0) {
                    grpNode.mouseenter(function () {
                        this.animate(100, "<").transform({ scale: 1.02 });
                    });
                    grpNode.mouseleave(function () {
                        this.animate(100, "<").transform({ scale: 1 });
                    });
                };
                children = this._getChildren(node);
                if ((children) && (children.length > 0)) {
                    for (var i = 0; i < children.length; i++) {
                        var child = children[i];                    
                        this._drawHierarchy(child);
                    };
                };
            }
        },
        _resizeHierarchy(node, area) {
            if ((node) && (area)) {
                var opt, e, children, bb, bbh, bbw, txtTitle;
                opt = this.options;
                e = opt.expr;
                if (opt.animateOnResize) {
                    node.rectNode.animate().size(area.w, area.h).x(area.x).y(area.y);
                } else {
                    node.rectNode.size(area.w, area.h).x(area.x).y(area.y);
                }
                txtTitle = node.txtTitle;
                txtTitle.text(node[e.name]);
                bb = txtTitle.bbox();
                bbh = bb.height;
                bbw = bb.width;
                if (area.h < bbh * 2 || area.w < bbh * 3) {
                    txtTitle.x(area.x + bbh / 2).y(area.y + bbh / 2);
                    txtTitle.clear();
                } else {
                    if (opt.animateOnResize) {
                        txtTitle.animate().x(area.x + bbh / 2).y(area.y + bbh / 2);
                    } else {
                        txtTitle.x(area.x + bbh / 2).y(area.y + bbh / 2);
                    }
                    this.ellipsisText(txtTitle, Math.floor(area.w - bbh));
                }
                children = this._getChildren(node);
                if ((children) && (children.length > 0)) {
                    for (var i = 0; i < children.length; i++) {
                        var child, cArea, cW, cH;
                        child = children[i];
                        if (area.w > area.h) {
                            cW = (area.w - bbh / 2) / children.length;
                            cH = area.h - bbh * 2.5;
                            cArea = {x: area.x + bbh / 2 + i * cW, y: area.y + bbh * 2, w: cW - bbh / 2, h: cH};
                        } else {
                            cW = area.w - bbh;
                            cH = (area.h - bbh * 2) / children.length;
                            cArea = {x: area.x + bbh / 2, y: area.y + bbh * 2 + i * cH, w: cW, h: cH - bbh / 2};
                        }                      
                        this._resizeHierarchy(child, cArea);
                    };
                };
            }
        }
    });

})(jQuery);

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}

function getContrastColor(hexColor) {
    var aColor = hexToRgb(hexColor);
    if ((aColor.r + aColor.g + aColor.b) > 384) {
        return "#000000"
    } else {
        return "#ffffff"
    };
}

function colorBrightness(Clr, f) { //-1 < F < 1  
    var r = parseInt(Clr.charAt(1) + Clr.charAt(2), 16);
    var g = parseInt(Clr.charAt(3) + Clr.charAt(4), 16);
    var b = parseInt(Clr.charAt(5) + Clr.charAt(6), 16);

    if (f < 0) {
        f = f + 1;
        r = Math.floor(r * f);
        g = Math.floor(g * f);
        b = Math.floor(b * f);
    } else {
        r = Math.floor((255 - r) * f + r);
        g = Math.floor((255 - g) * f + g);
        b = Math.floor((255 - b) * f + b);
    }

    if (r > 255) r = 255;
    if (g > 255) g = 255;
    if (b > 255) b = 255;

    var hex = "#" + r.toString(16) + g.toString(16) + b.toString(16);
    return hex
}