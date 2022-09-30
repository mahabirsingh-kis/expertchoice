/* javascript plug-ins for advanced animated SVG charts by SL */
const ecChartsDefaultOptions = {
    animateOnResize: true,
    autoFontSize: false,
    backgroundColor: '#ffffff', //'#ffffaa',
    chartType: 'blocks', // 'blocks', 'sunburst'
    dataSource: null,
    decimals: 2,
    debugMode: false,
    expr: {
        id: 'ID',
        pid: 'ParentNodeID',
        name: 'Name',
        color: 'Color',
    },
    fontSize: 14,
    height: 200,
    itemsArrange: 'auto', // 'auto' 'horizontal' 'vertical'
    maxFontSize: 20,
    minFontSize: 10,
    rootValue: 1,
    shadeColorsLevel: 1,
    shadeColorsRate: 0.3,
    sizeMode: 'percent', // 'percent', 'equal', 'count'
    width: 300,
};

var debugStrokeStyle = { width: 0.5, color: 'red' };

var ecChartsDefaultOptionsList = ['animateOnResize', 'backgroundColor', 'decimals'];

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
            var w, h, w2, h2;
            w = Math.round(width);
            h = Math.round(height);
            w2 = w/2;
            h2 = h/2;
            opt.width = w;
            opt.height = h;
            opt.context.addClass('scroll').css('height', h + 'px').css('width', w + 'px');
            opt.svg.background.size(w, h).center(w2, h2);
            if (opt.debugMode) {
                dg = opt.svg.grpDebug;
                dg.lineCenterV.plot(w2, 0, w2, h);
                dg.lineCenterH.plot(0, h2, w, h2);
                dg.txtSize.text(w + 'x' + h);
                var bb = dg.txtSize.bbox();
                dg.txtSize.x(bb.height).y(bb.height);
            };
            this._resizeHierarchy(opt.dataSource.objs[opt.rootValue], {x:0,y:0,w:w,h:h}, -1);
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
            var opt, e, ctx, w, h;
            opt = this.options;
            e = opt.expr;
            ctx = opt.context;
            w = opt.width;
            h = opt.height;
            opt.svg.background = ctx.rect().fill(opt.backgroundColor);
            opt.svg.grpObjs = ctx.group();
            opt.initRootValue = opt.rootValue;
            this._drawHierarchy(opt.dataSource.objs[opt.rootValue], 0, -1);
            if (opt.debugMode) {
                opt.svg.background.stroke(debugStrokeStyle);
                dg = ctx.group();
                opt.svg.grpDebug = dg;                
                dg.lineCenterV = ctx.line().stroke(debugStrokeStyle);
                dg.lineCenterH = ctx.line().stroke(debugStrokeStyle);
                dg.txtSize = ctx.text('');
            };
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
        _getTotalChildren(node, tChildren) {
            var opt, e, children;
            opt = this.options;
            e = opt.expr;
            children = this._getChildren(node);
            if ((children) && (children.length > 0)) {
                for (var i = 0; i < children.length; i++) {
                    var child = children[i]; 
                    tChildren.push(child);
                    this._getTotalChildren(child, tChildren);
                }
            }
        },
        ellipsisText: function (txt, maxWidth) {
            var text = txt.text();
            if (txt.bbox().width < maxWidth) { return txt };
            var result = text;
            while ((txt.text(result + "\u2026").bbox().width > maxWidth) && result.length > 0) {
                result = result.substring(0, result.length - 1);
            }
            return txt;
        },
        _getAutoFontSize() {
            var opt, afs, totalChildrenCount;
            opt = this.options;
            totalChildrenCount = opt.dataSource.objs[opt.rootValue].totalChildrenCount;
            afs = opt.height / (totalChildrenCount * 3);
            if (afs < opt.minFontSize) afs = opt.minFontSize;
            if (afs > opt.maxFontSize) afs = opt.maxFontSize;
            return afs;
        },
        _drawHierarchy(node, idx, uid) {
            var opt, e, children, txtTitle, tooltip, grpNode, color, pNode, ups;
            opt = this.options;
            e = opt.expr;
            if ((node)) {
                if (uid in opt.dataSource.objPriorities) {
                    if(node[e.id] in opt.dataSource.objPriorities[uid]) {
                        ups = opt.dataSource.objPriorities[uid][node[e.id]];
                    }
                }
                if (typeof ups == 'undefined') {
                    ups = {prty:1, uprty:1, lprty:1, gprty:1}
                }
                tooltip = roundedValue(ups.prty * 100, opt.decimals) + '% ' + node[e.name];
                totalChildren = [];
                this._getTotalChildren(node, totalChildren);
                node.totalChildrenCount = totalChildren.length;
                pNode = opt.dataSource.objs[node[e.pid]];
                grpNode = opt.svg.grpObjs.group();
                node.grpNode = grpNode;
                color = (node.Level > opt.shadeColorsLevel ? colorBrightness(pNode[e.color], (idx + 1) * opt.shadeColorsRate / this._getChildren(pNode).length) : node[e.color]);
                node.selectedColor = colorBrightness(color, 0.5);
                node[e.color] = color;
                node.rectNode = grpNode.rect().fill(color).radius(5).stroke(opt.backgroundColor);
                txtTitle = grpNode.text(tooltip).fill(getContrastColor(color));
                if (opt.autoFontSize) {
                    txtTitle.font({ size: this._getAutoFontSize() })
                } else {
                    txtTitle.font({ size: opt.fontSize })
                }
                bb = txtTitle.bbox();
                node.txtTitle = txtTitle;

                grpNode.mouseenter(function () {
                    $(opt.divCanvas).prop("title", tooltip);
                    node.rectNode.animate(100, "<").fill(node.selectedColor).animate().stroke('#0000aa');
                });

                grpNode.mouseleave(function () {
                    $(opt.divCanvas).prop("title", "");
                    node.rectNode.animate(500, "<").fill(node[e.color]).animate().stroke(opt.backgroundColor);
                });
                var scope = this;
                children = this._getChildren(node);
                grpNode.click(function () {
                    scope._moveNodesToFront(node);
                    if (opt.rootValue == node[e.id]) {
                        opt.rootValue = opt.initRootValue;
                    } else {
                        opt.rootValue = node[e.id];
                    };
                    scope.resize(opt.width, opt.height);
                });

                
                if ((children) && (children.length > 0)) {
                    for (var i = 0; i < children.length; i++) {
                        var child = children[i];                    
                        this._drawHierarchy(child, i, uid);
                    };
                };
            }
        },
        _moveNodesToFront(node) {
            var opt, e, children;
            opt = this.options;
            e = opt.expr;
            node.grpNode.front();
            children = this._getChildren(node);
            if ((children) && (children.length > 0)) {
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    this._moveNodesToFront(child);
                };
            }
        },
        _getNodeSize(parentSize, sum, percent, totalCount, count, childrenCount) {
            var opt, retVal;
            opt = this.options;
            retVal = 1;
            switch(opt.sizeMode) {
                case 'percent':
                    retVal = parentSize / sum * percent;
                    break;
                case 'count':
                    retVal = parentSize / totalCount * (count + 1);
                    break;
                default:
                    retVal = parentSize / childrenCount;
            };  
            return retVal;
        },
        _resizeHierarchy(node, area, uid) {
            var opt, e, children, bb, bbh, bbw, txtTitle, ups;
            opt = this.options;
            e = opt.expr;
            if ((node) && (area)) {
                var rw, rh;
                rw = (area.w < 2 ? 2 : area.w);
                rh = (area.h < 2 ? 2 : area.h);
                if (opt.animateOnResize) {
                    node.rectNode.animate().size(rw, rh).x(area.x).y(area.y);
                } else {
                    node.rectNode.size(rw, rh).x(area.x).y(area.y);
                }
                if (uid in opt.dataSource.objPriorities) {
                    if(node[e.id] in opt.dataSource.objPriorities[uid]) {
                        ups = opt.dataSource.objPriorities[uid][node[e.id]];
                    }
                }
                if (typeof ups == 'undefined') {
                    ups = {prty:1, uprty:1, lprty:1, gprty:1}
                }
                txtTitle = node.txtTitle;
                txtTitle.text(roundedValue(ups.prty * 100, opt.decimals) + '% ' + node[e.name]);
                if (opt.autoFontSize) {
                    txtTitle.font({ size: this._getAutoFontSize() })
                } else {
                    txtTitle.font({ size: opt.fontSize })
                }
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
                    var childShift = 0;
                    var sumPrty = 0;
                    for (var i = 0; i < children.length; i++) {
                        var child, cArea, cW, cH;
                        child = children[i];
                        if (uid in opt.dataSource.objPriorities) {
                            if(child[e.id] in opt.dataSource.objPriorities[uid]) {
                                ups = opt.dataSource.objPriorities[uid][child[e.id]];
                            }
                        }
                        if (typeof ups == 'undefined') {
                            ups = {prty:1, uprty:1, lprty:1, gprty:1}
                        }
                        sumPrty += ups.prty;
                    };
                    for (var i = 0; i < children.length; i++) {
                        var child, cArea, cW, cH;
                        child = children[i];
                        if (uid in opt.dataSource.objPriorities) {
                            if(child[e.id] in opt.dataSource.objPriorities[uid]) {
                                ups = opt.dataSource.objPriorities[uid][child[e.id]];
                            }
                        }
                        if (typeof ups == 'undefined') {
                            ups = {prty:1, uprty:1, lprty:1, gprty:1}
                        }
                        if ((opt.itemsArrange == 'auto' && area.w > area.h)||(opt.itemsArrange == 'horizontal')) {
                            cW = this._getNodeSize(area.w - bbh / 2, sumPrty, ups.prty, node.totalChildrenCount, child.totalChildrenCount, children.length);                           
                            cH = area.h - bbh * 2.5;
                            cArea = {x: area.x + bbh / 2 + childShift, y: area.y + bbh * 2, w: cW - bbh / 2, h: cH};
                            childShift += cW;
                        } else {
                            cW = area.w - bbh;
                            cH = this._getNodeSize(area.h - bbh * 2, sumPrty, ups.prty, node.totalChildrenCount, child.totalChildrenCount, children.length);
                            cArea = {x: area.x + bbh / 2, y: area.y + bbh * 2 + childShift, w: cW, h: cH - bbh / 2};
                            childShift += cH;
                        }                      
                        this._resizeHierarchy(child, cArea, uid);
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

function roundedValue(val, decimals) {
    if (decimals == 0) {
        return Math.round(val);
    } else {
        var p = Math.pow(10, decimals);
        var rVal = Math.round(val * p) / p;
        var sVal = rVal + "";
        var idx = sVal.indexOf(".");
        if (idx == -1) {
            sVal += ".";
            idx = sVal.length - 1;
        };
        var trail = "";
        var zCount = decimals - (sVal.length - idx - 1);
        for (var i = 0; i < zCount; i++) {
            trail += "0";
        };
        retVal = sVal + trail;
        return retVal;
    }
}