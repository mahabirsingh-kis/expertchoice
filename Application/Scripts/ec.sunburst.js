/* javascript plug-ins for Sunburst Chart (Hierarchical Pie) by DA and SL */

(function ($) {
    $.widget("expertchoice.ecChart", {
        options: {
            areaMode: true,
            canvas: null,
            canvasRect: null,
            center: { x: 0, y: 0 },
            colorPalette: null,
            context: null,
            dataSource: [{ name: "Main Goal", id: 1, pid: -1, prty: 1, color: "#cc0000" },
                         { name: "Objective 1", id: 2, pid: 1, prty: 0.1, color: "#00cc00" },
                         { name: "Objective 2", id: 3, pid: 1, prty: 0.1, color: "#0000cc" },
                         { name: "Objective 3", id: 4, pid: 1, prty: 0.2, color: "#aacc00" },
                         { name: "Objective 3-1", id: 8, pid: 4, prty: 0.06, color: "#ffeecc" },
                         { name: "Objective 3-2", id: 9, pid: 4, prty: 0.04, color: "#eeffcc" },
                         { name: "Objective 3-3", id: 10, pid: 4, prty: 0.04, color: "#eeffcc" },
                         { name: "Objective 4", id: 5, pid: 1, prty: 0.6, color: "#00ccaa" },
                         { name: "Objective 4-1", id: 6, pid: 5, prty: 0.06, color: "#00eecc" },
                         { name: "Objective 4-2", id: 7, pid: 5, prty: 0.04, color: "#ee00cc" },
            ],
            //dataSource: [{ "name" : "Goal: Optimize IT Portfolio To Improve Performance", "id" : 1, "pid" : -1, "prty": 1, "prc" : 100, "color": "#2c75ff" },{ "name" : "Leverage Knowledge", "id" : 9, "pid" : 1, "prty": 0.2353936, "prc" : 23.54, "color": "#FF0000" },{ "name" : "Vendor/Partner Access", "id" : 37, "pid" : 9, "prty": 0.06217778, "prc" : 6.22, "color": "#9d27a8" },{ "name" : "Customer Access/Service", "id" : 38, "pid" : 9, "prty": 0.06966268, "prc" : 6.97, "color": "#478430" },{ "name" : "Internal Access", "id" : 39, "pid" : 9, "prty": 0.1035532, "prc" : 10.36, "color": "#e33000" },{ "name" : "Improve Organizational Efficiency", "id" : 11, "pid" : 1, "prty": 0.2545224, "prc" : 25.45, "color": "#E46C0A" },{ "name" : "Improve Service Efficiencies", "id" : 12, "pid" : 11, "prty": 0.06135145, "prc" : 6.14, "color": "#a10040" },{ "name" : "Leverage Purchasing Power", "id" : 29, "pid" : 11, "prty": 0.04441281, "prc" : 4.44, "color": "#0affe3" },{ "name" : "Improve Time to Market", "id" : 30, "pid" : 11, "prty": 0.06628368, "prc" : 6.63, "color": "#00523c" },{ "name" : "Manage Resources", "id" : 40, "pid" : 11, "prty": 0.08247445, "prc" : 8.25, "color": "#ffbde6" },{ "name" : "Maintain Serviceability", "id" : 16, "pid" : 1, "prty": 0.110408, "prc" : 11.04, "color": "#4F6228" },{ "name" : "Scaleability", "id" : 52, "pid" : 16, "prty": 0.04498999, "prc" : 4.5, "color": "#7280c4" },{ "name" : "Responsiveness", "id" : 53, "pid" : 16, "prty": 0.0328638, "prc" : 3.29, "color": "#009180" },{ "name" : "Resources", "id" : 54, "pid" : 16, "prty": 0.03255419, "prc" : 3.26, "color": "#6c3b2a" },{ "name" : "Minimize Risks", "id" : 21, "pid" : 1, "prty": 0.20219, "prc" : 20.22, "color": "#7030A0" },{ "name" : "Leverage Proven Technology", "id" : 33, "pid" : 21, "prty": 0.1002445, "prc" : 10.02, "color": "#f24961" },{ "name" : "Ensure Readiness", "id" : 65, "pid" : 21, "prty": 0.1019455, "prc" : 10.19, "color": "#663d2e" },{ "name" : "Vendor", "id" : 24, "pid" : 65, "prty": 0.03738382, "prc" : 3.74, "color": "#9600fa" },{ "name" : "Business", "id" : 23, "pid" : 65, "prty": 0.06456167, "prc" : 6.46, "color": "#919100" },{ "name" : "Financials", "id" : 62, "pid" : 1, "prty": 0.197486, "prc" : 19.75, "color": "#DB008A" },{ "name" : "Maximize NPV", "id" : 63, "pid" : 62, "prty": 0.06818487, "prc" : 6.82, "color": "#a15f00" },{ "name" : "Maximize ROI", "id" : 64, "pid" : 62, "prty": 0.1293011, "prc" : 12.93, "color": "#cce6ff" }],
            //decimals: 2,
            drawSVG: false,
            divCanvas: null,
            fontSize: 12,
            hMaxLevel: 1,
            initDataSource: [],
            interactiveMap: [],
            isMouseDown: false,
            isMouseOverSegment: false,
            levelRadius: 80,
            margin: 10,
            mouseDownPosition: null,
            mouseOverElement: {},
            showLegend: true,
            showLabels: true,
            svg: null,
            selectedNode: null,
        },
        _create: function () {
            var opt = this.options;
            opt.divCanvas = this.element[0];
            opt.divCanvas.innerHTML = "";
            $(opt.divCanvas).addClass("widget-created");
            opt.canvas = $("<canvas></canvas>").appendTo(opt.divCanvas)[0];
            this._saveInitDataSource();
            var rootNode = opt.dataSource[0];
            rootNode.percentage = 100;
            rootNode.sAngle = 0;
            rootNode.segmentAngle = 360;
            rootNode.level = 0;
            opt.selectedNode = rootNode;
            this._initHierarchy(rootNode, 1);
            if ((opt.canvas) && (opt.canvas.getContext)) {
                var w = opt.divCanvas.clientWidth;
                var h = opt.divCanvas.clientHeight;
                opt.canvas.width = w;
                opt.canvas.height = h;
                if (opt.drawSVG) {
                    opt.svg = null;
                    opt.context = new C2S(w, h);
                } else {
                    opt.context = opt.canvas.getContext("2d");
                };
                this._resetCenterPoint();
                this._resetLevelRadius();
                this.redraw();
                if (animation.enabled && !opt.drawSVG) this._startAnimating();
                var scope = this;
                $(opt.divCanvas).mousemove(function (event) {
                    var opt = scope.options;
                    var clientCoords = "( " + event.clientX + ", " + event.clientY + " )";
                    var pos = getRelativePosition(opt.divCanvas, event.clientX, event.clientY);
                    var ar = getAngleRadiusFromPoint(opt.center.x, opt.center.y, pos.x, pos.y);
                    //$("#coords").html(" | canvasCoords: " + pos.x + ", " + pos.y + " clientCoords: " + clientCoords + " | a:" + Math.round(ar.a) + " r:" + Math.round(ar.r) + " mo:" + opt.mouseOverElement.name);
                    var oldMouseOverElement = opt.mouseOverElement;
                    var rootNode = opt.selectedNode;
                    opt.mousePosition = { x: pos.x, y: pos.y };
                    if (scope._isPointInSegment(pos.x, pos.y, 0, 360, 0, opt.levelRadius)) {
                        opt.mouseOverElement = rootNode;
                    } else {
                        opt.isMouseOverSegment = false;
                        scope._findMouseOverElement(rootNode, pos.x, pos.y);
                        if (!opt.isMouseOverSegment) opt.mouseOverElement = null;
                    };
                    if (opt.mouseOverElement !== oldMouseOverElement) {
                        if (opt.mouseOverElement == null) {
                            opt.svg = null;
                            scope.redraw();
                        } else {
                            if (animation.enabled && !opt.drawSVG) {
                                animation.startValue = 0;
                                animation.endValue = 100;
                                animation.mode = "selectSegment";
                            } else {
                                animation.startValue = 100;
                                opt.svg = null;
                                scope.redraw();
                            };
                        };
                    };
                    //scope._drawSegment(scope.options.center.x, scope.options.center.y, ar.a - 1, ar.a + 1, ar.r - 5, ar.r + 5, "#000000", "A", true);
                });
                $(opt.divCanvas).click(function (event) {
                    var opt = scope.options;
                    if (opt.mouseOverElement !== null) {
                        var mouseHoverNode = opt.mouseOverElement;
                        if (mouseHoverNode.id == opt.selectedNode.id) {
                            scope._restoreInitDataSource();
                            var parentNode = scope._getParent(opt.selectedNode);
                            if (parentNode !== null) {
                                mouseHoverNode = parentNode;
                                opt.mouseOverElement = mouseHoverNode;
                            }
                        };

                        if (scope._getChildren(mouseHoverNode).length > 0) {
                            //animation.startValue = mouseHoverNode.percentage;
                            //animation.endValue = 100;
                            mouseHoverNode.sAngle = 0;
                            //animation.startValue = animation.endValue;
                            opt.mouseOverElement.percentage = 100;
                            opt.mouseOverElement.segmentAngle = 360;
                            opt.hMaxLevel = 1;
                            opt.selectedNode = opt.mouseOverElement;
                            scope._initHierarchy(opt.mouseOverElement, opt.mouseOverElement.level + 1);
                            scope.resize(opt.canvas.width, opt.canvas.height);
                            //animation.mode = "drilldownSegment";
                        }
                    }
                });
                opt.canvas.addEventListener("wheel", function (event) {
                    //   initAngle += event.deltaY;
                    //   //scope.options.levelRadius += event.deltaY;
                    //   //if (scope.options.levelRadius < 8) scope.options.levelRadius = 8;
                    //   scope.redraw();
                    //   return false;
                });
            } else {
                $(opt.divCanvas).html("<h6>Error: Your browser is not supported</h6>");
            }
        },
        _destroy: function () {

        },
        _onMouseMove: function (pos) {
            document.body.style.cursor = 'default';
            var opt = this.options;
            var oldMousePos = opt.mouseOverElement;
            if (!opt.isMouseDown) {
                var ieKey = this._getInteractiveMapElement(pos.x, pos.y);
                opt.mouseOverElement = ieKey;
                if (oldMousePos != opt.mouseOverElement) {

                };
            };
        },
        _onMouseDown: function (pos) {
            var opt = this.options;
            opt.isMouseDown = true;
            opt.isUpdateFinished = false;
        },
        _onMouseUp: function () {
            if (document.releaseCapture) document.releaseCapture();
            document.body.style.cursor = 'default';
            var opt = this.options;
            opt.isMouseDown = false;
        },
        _getInteractiveMapElement: function (x, y) {
            for (var i = this.options.interactiveMap.length - 1; i >= 0; i--) {
                var intElement = this.options.interactiveMap[i];
                var xe = intElement.rect[0];
                var ye = intElement.rect[1];
                var we = intElement.rect[2];
                var he = intElement.rect[3];
                if ((x > xe) && (y > ye) && (x < xe + we) && (y < ye + he)) {
                    //var ctx = this.options.context;
                    //this.drawHint(ctx, [intElement.key, x, y], 10, 10);
                    return intElement;
                }
            };
            return '';
        },
        _saveInitDataSource: function () {
            // var opt = this.options;
            // opt.initDataSource = [];
            // for (var i=0;i<opt.dataSource.length;i++){
            //     opt.initDataSource.push(opt.dataSource[i]);
            // };
        },
        _restoreInitDataSource: function () {
            // var opt = this.options;
            // opt.dataSource = [];
            // for (var i=0;i<opt.initDataSource.length;i++){
            //     opt.dataSource.push(opt.initDataSource[i]);
            // };
        },
        _resetLevelRadius: function () {
            var opt = this.options;
            var maxRadius = Math.min(opt.center.x - opt.margin, opt.center.y - opt.margin);
            if (opt.areaMode) {
                opt.levelRadius = Math.sqrt(Math.pow(maxRadius, 2) / (opt.hMaxLevel + 1 - opt.selectedNode.level))
            } else {
                opt.levelRadius = (maxRadius) / (opt.hMaxLevel + 1 - opt.selectedNode.level)
            };
        },
        _resetCenterPoint: function () {
            var opt = this.options;
            var w = opt.canvas.width;
            var h = opt.canvas.height;
            if (w > h) { opt.center.x = (opt.showLegend ? w / 3 : w / 2); opt.center.y = h / 2 }
            else { opt.center.x = w / 2; opt.center.y = (opt.showLegend ? h / 3 : h / 2) };
        },
        export: function (filename, width, height, format) {
            var opt = this.options;
            var oldW = opt.canvas.width;
            var oldH = opt.canvas.height;
            var oldFontSize = opt.fontSize;
            opt.fontSize = Math.round((width > height ? height / 1000 * 14 : width / 1000 * 14));
            opt.mouseOverElement = null;
            switch (format) {
                case "PNG":
                    this.resize(width, height);
                    var img = opt.canvas.toDataURL("image/png");
                    download(img, filename + ".png", "image/png");
                    break;
                case "SVG":
                    opt.canvas.width = width;
                    opt.canvas.height = height;
                    this._resetCenterPoint();
                    this._resetLevelRadius();
                    opt.context = new C2S(width, height);
                    var ctx = opt.context;
                    this.redraw();
                    var img = ctx.getSerializedSvg(true);
                    download(img, filename + ".svg", "image/svg");
                    break;
                case "PDF":
                    this.resize(width, height);
                    var img = opt.canvas.toDataURL("image/png");

                    var doc = new jsPDF((width > height ? "l" : "p"), "mm", "a4");
                    var margin = 15;
                    var imgWidth = Math.round(doc.internal.pageSize.width) - margin * 2;
                    var imgHeight = Math.round(imgWidth / width * height);
                    doc.text(margin, margin, filename);
                    doc.addImage(img, "PNG", margin, margin * 2, imgWidth, imgHeight, "", "FAST");
                    // doc.addSVG(img, margin, margin, imgWidth, imgHeight);
                    // doc.addPage();
                    // doc.text(20, 20, "Page 2");
                    doc.save(filename + ".pdf");
                    break;
            };
            opt.fontSize = oldFontSize;
            this.resize(oldW, oldH);
        },
        resize: function (width, height) {
            var opt = this.options;
            opt.canvas.width = width;
            opt.canvas.height = height;
            this._resetCenterPoint();
            this._resetLevelRadius();
            if (opt.drawSVG) {
                opt.svg = null;
                opt.context = new C2S(width, height);
            } else {
                opt.context = opt.canvas.getContext("2d");
            };
            this.redraw();
        },
        redraw: function () {
            var opt = this.options;
            var ctx = opt.context;
            var w = opt.canvas.width;
            var h = opt.canvas.height;
            var rootNode = opt.selectedNode;
            var animateProgress = (animation.startValue / 100);
            //ctx.clearRect(0, 0, w, h);
            ctx.save();
            ctx.beginPath();
            ctx.fillStyle = "#ffffff";
            ctx.rect(0, 0, w, h);
            ctx.fill();
            /* Main Border */
            //ctx.rect(1, 1, w - 2, h - 2);
            //ctx.lineWidth = 1;
            //ctx.strokeStyle = "#000";
            //ctx.stroke();

            //ctx.textAlign = 'left';
            //ctx.font = "14px Arial";  
            //opt.mouseOverElement = opt.dataSource[2];
            //ctx.beginPath();
            //ctx.fillText(opt.canvas.width + "x" + opt.canvas.height, 10, 10);
            ctx.restore();
            for (var i = 0; i < opt.dataSource.length; i++) {
                opt.dataSource[i].showInLegend = false;
            };
            opt.selectedSegment = { selected: false };
            this._drawHierarchy(rootNode);
            var sel = opt.selectedSegment;
            if (sel.selected) { this._drawSegment(opt.center.x, opt.center.y, sel.startAngle - 1 * animateProgress, sel.endAngle + 1 * animateProgress, sel.r1 + 4 * animateProgress, sel.r2 + 8 * animateProgress, sel.color, sel.text, true) };
            //ctx.restore();
            var rootRadius = opt.levelRadius;
            var fontSize = opt.fontSize;
            if (opt.mouseOverElement == rootNode) {
                rootRadius += 8 * animateProgress;
                fontSize += animateProgress;
            };
            this._drawRoot(rootRadius, rootNode, opt.mouseOverElement == rootNode, fontSize);
            if (opt.showLegend) this._drawLegend();
            if (opt.drawSVG) {
                if (opt.svg == null) opt.svg = ctx.getSerializedSvg(true);
                $(opt.divCanvas).html(opt.svg);
            };
        },
        _drawRoot: function (radius, rootNode, isSelected, fontSize) {
            var opt = this.options;
            ctx = opt.context;
            ctx.save();
            ctx.beginPath();
            ctx.fillStyle = rootNode.color;
            ctx.strokeStyle = "#ffffff";
            ctx.lineWidth = 2;
            if (isSelected) {
                ctx.shadowColor = '#ffffff';
                ctx.shadowBlur = 15;
            };
            ctx.arc(opt.center.x, opt.center.y, radius, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
            ctx.restore();
            ctx.save();
            ctx.font = fontSize + "px Arial";
            ctx.fillStyle = getContrastColor(rootNode.color);
            ctx.textAlign = 'center';
            ctx.textBaseline = "middle";
            //var priorityText = (rootNode.prty * 100).toFixed(opt.decimals) + "% ";
            var priorityText = (rootNode.prc) + "% ";
            var textWidth = radius * 2 - fontSize * 2;
            var maxWidth = textWidth * 4;
            var truncText = getTruncatedString(ctx, priorityText + rootNode.name, maxWidth);
            drawMultilineText(ctx, truncText, opt.center.x, opt.center.y, textWidth, fontSize);
            //ctx.fillText(, opt.center.x, opt.center.y - fontSize * 2);
            if (rootNode.level > 0) {
                ctx.beginPath();
                ctx.font = fontSize + "px FontAwesome";
                ctx.fillText("\uf0e2", opt.center.x, opt.center.y + fontSize * 2);
            };

            ctx.restore();
        },
        _drawSegment: function (x, y, startAngle, endAngle, radius1, radius2, color, label, isSelected) {
            var opt = this.options;
            var ctx = opt.context;
            ctx.save();
            ctx.beginPath();
            ctx.fillStyle = color;
            ctx.strokeStyle = "#ffffff";
            ctx.lineWidth = 2;
            var A = getRadPoint(x, y, startAngle, radius1);
            var B = getRadPoint(x, y, startAngle, radius2);
            //var C = getRadPoint(x,y,endAngle, radius2);
            var D = getRadPoint(x, y, endAngle, radius1);
            var startAngleRad = getRadAngle(startAngle);
            var endAngleRad = getRadAngle(endAngle);
            ctx.moveTo(A.x, A.y);
            ctx.lineTo(B.x, B.y);
            ctx.arc(x, y, radius2, startAngleRad, endAngleRad, false);
            //ctx.lineTo(C.x, C.y);
            ctx.lineTo(D.x, D.y);
            //ctx.lineTo(A.x, A.y);
            ctx.arc(x, y, radius1, endAngleRad, startAngleRad, true);
            if (isSelected) {
                ctx.shadowColor = '#ffffff';
                ctx.shadowBlur = 15;
            };
            ctx.fill();
            ctx.stroke();
            ctx.restore();
            var segmentHeight = Math.sqrt(Math.pow(Math.abs(D.x - A.x), 2) + Math.pow(Math.abs(D.y - A.y), 2));
            if ((segmentHeight > opt.fontSize) && ((radius2 - radius1) > ctx.measureText(label.split(" ")[0]).width)) {
                this._drawSegmentLabel(label, x, y, startAngle, endAngle, radius1, radius2, getContrastColor(color), segmentHeight);
            }
        },
        _drawSegmentLabel: function (text, x, y, startAngle, endAngle, radius1, radius2, color, height) {
            var opt = this.options;
            var ctx = opt.context;
            var rotateText = false;
            var rotateAngle = (startAngle + (endAngle - startAngle) / 2);
            if ((rotateAngle + initAngle) % 360 > 180) {
                rotateText = true;
            };
            if (rotateText) { rotateAngle -= 180 };
            var rotateAngleRad = getRadAngle(rotateAngle);
            ctx.save();
            ctx.translate(x, y);
            ctx.rotate(rotateAngleRad);
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillStyle = color;
            var fontSize = opt.fontSize;
            ctx.font = fontSize + "px Arial";
            var lineWidth = radius2 - radius1 - fontSize;
            var maxWidth = (height / fontSize) * lineWidth;
            var truncText = getTruncatedString(ctx, text, maxWidth);
            drawMultilineText(ctx, truncText, (radius1 + (radius2 - radius1) / 2) * (rotateText ? -1 : 1), 0, lineWidth, fontSize);
            ctx.restore();
        },
        _getChildren: function (node) {
            var opt = this.options;
            var retVal = [];
            for (var i = 0; i < opt.dataSource.length; i++) {
                if (opt.dataSource[i].pid == node.id) retVal.push(opt.dataSource[i]);
            }
            return retVal
        },
        _getParent: function (node) {
            var opt = this.options;
            var retVal = null;
            for (var i = 0; i < opt.dataSource.length; i++) {
                if (opt.dataSource[i].id == node.pid) retVal = opt.dataSource[i];
            }
            return retVal;
        },
        _initHierarchy: function (node, level) {
            var opt = this.options;
            var children = this._getChildren(node);
            if ((children) && (children.length > 0)) {
                var sumPrty = 0;
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    child.level = level;
                    //child.position = i;
                    if (level > 1) child.color = ColorBrightness(node.color, 0.5);
                    if (level > opt.hMaxLevel) opt.hMaxLevel = level;
                    sumPrty += child.prty;
                    this._initHierarchy(child, level + 1);
                }
                //normalize segments percentage, sum = 100%
                if (sumPrty > 0) {
                    for (var i = 0; i < children.length; i++) {
                        var child = children[i];
                        child.percentage = child.prty / sumPrty * 100;
                    }
                }
            }

        },
        _drawHierarchy: function (node) {
            var opt = this.options;
            node.showInLegend = true;
            var children = this._getChildren(node);
            if ((children) && (children.length > 0)) {
                var sAngle = node.sAngle;
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    if (child.percentage > 0) {
                        //var text = (child.prty * 100).toFixed(opt.decimals) + "%";
                        var text = child.prc + "%";
                        if (opt.showLabels) {
                            text += " " + child.name;
                        }
                        var nodeAngle = node.segmentAngle * (child.percentage / 100);
                        child.sAngle = sAngle;
                        child.segmentAngle = nodeAngle;
                        var radius1 = this._getLevelRadius(child.level);
                        var radius2 = this._getLevelRadius(child.level + 1);
                        if (opt.mouseOverElement == child) {
                            opt.selectedSegment = { selected: true, startAngle: sAngle, endAngle: sAngle + nodeAngle, r1: radius1, r2: radius2, color: child.color, text: text };
                        } else {
                            this._drawSegment(opt.center.x, opt.center.y, sAngle, sAngle + nodeAngle, radius1, radius2, child.color, text, false)
                        };
                        sAngle += nodeAngle;
                    };
                    this._drawHierarchy(child);
                };
            };
        },
        _findMouseOverElement: function (node, x, y) {
            var opt = this.options;
            var children = this._getChildren(node);
            if ((children) && (children.length > 0)) {
                var sAngle = node.sAngle;
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    var nodeAngle = node.segmentAngle * (child.percentage / 100);
                    child.sAngle = sAngle;
                    child.segmentAngle = nodeAngle;
                    var radius1 = this._getLevelRadius(child.level);
                    var radius2 = this._getLevelRadius(child.level + 1);
                    if (this._isPointInSegment(x, y, sAngle, sAngle + nodeAngle, radius1, radius2)) {
                        opt.mouseOverElement = child;
                        opt.isMouseOverSegment = true;
                        return true;
                    };
                    sAngle += nodeAngle;
                    this._findMouseOverElement(child, x, y);
                };
            };
        },
        _isPointInSegment: function (x, y, startAngle, endAngle, r1, r2) {
            var opt = this.options;
            var cx = opt.center.x;
            var cy = opt.center.y;
            var A = getRadPoint(cx, cy, startAngle, r1);
            var B = getRadPoint(cx, cy, startAngle, r2);
            var C = getRadPoint(cx, cy, endAngle, r2);
            var D = getRadPoint(cx, cy, endAngle, r1);
            var startAngleRad = getRadAngle(startAngle);
            var endAngleRad = getRadAngle(endAngle);
            var ar = getAngleRadiusFromPoint(cx, cy, x, y);
            if (ar.a > startAngle && ar.a < endAngle && ar.r > r1 && ar.r < r2) {
                return true;
            } else {
                return false;
            };
        },
        _drawLegend: function () {
            var opt = this.options;
            var ctx = opt.context;
            var fontSize = opt.fontSize;
            ctx.save();
            ctx.textAlign = "left";
            //ctx.textBaseline = "top";
            var nodey = 1;
            var vsp = fontSize / 4;
            for (var i = 0; i < opt.dataSource.length; i++) {
                var node = opt.dataSource[i];
                if (node.showInLegend) {
                    ctx.beginPath();
                    var isSelected = opt.mouseOverElement == node;
                    if (isSelected) {
                        ctx.font = "bold " + fontSize + "px Arial";
                    } else {
                        ctx.font = fontSize + "px Arial";
                    };
                    ctx.fillStyle = node.color;
                    var x, y;
                    if (opt.canvas.width > opt.canvas.height) {
                        x = opt.canvas.width / 3 * 2 + fontSize * node.level;
                        y = nodey * (fontSize + vsp) + fontSize;
                    } else {
                        x = fontSize * node.level + fontSize;
                        y = opt.canvas.height / 3 * 2 + nodey * (fontSize + vsp) + fontSize;
                        if (y + fontSize > opt.canvas.height) {
                            y -= opt.canvas.height / 3 - fontSize;
                            x += opt.canvas.width / 2;
                        };
                    };
                    ctx.rect(x, y, fontSize, fontSize);
                    ctx.fill();
                    if (isSelected) ctx.stroke();
                    ctx.beginPath();
                    ctx.fillStyle = "#000000";
                    var maxWidth;
                    if (opt.canvas.width > opt.canvas.height) {
                        maxWidth = opt.canvas.width - x - fontSize * 2;
                    } else {
                        maxWidth = opt.canvas.width / 2 - fontSize * 2 - fontSize * node.level;
                    };
                    var truncName = getTruncatedString(ctx, node.name, maxWidth);
                    ctx.fillText(truncName, x + fontSize + vsp, y + fontSize - vsp / 2);
                    nodey += 1;
                };
            };
            ctx.restore();
        },
        drawHint: function (ctx, text) {
            var opt = this.options;
            var margins = 15;
            ctx.save();
            ctx.font = (opt.fontSize + 2) + "px Arial";
            var x = opt.mousePosition.x + margins;
            var y = opt.mousePosition.y;
            var maxW = 0;
            var lineH = (opt.fontSize + 2) * 1.5;
            var lines = text.split('//');
            var maxH = lineH * lines.length + (opt.fontSize + 2) / 2;
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i];
                var linew = ctx.measureText(line).width;
                if (maxW < linew) { maxW = linew };
            };
            if (x + maxW + margins > opt.canvas.width) {
                x = opt.mousePosition.x - (maxW + margins);
            };
            if (y + maxH > opt.canvas.height) {
                y = opt.canvas.height - maxH;
            };
            ctx.fillStyle = '#FCFBDE';
            ctx.beginPath();
            ctx.rect(x, y, maxW + margins, maxH);
            ctx.save();
            ctx.shadowBlur = 4;
            ctx.shadowColor = "#444444";
            ctx.shadowOffsetX = 2;
            ctx.shadowOffsetY = 2;
            ctx.fill();
            ctx.restore();
            ctx.fillStyle = '#333333';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i];
                ctx.fillText(line, x + margins / 2, y + (opt.fontSize + 2) / 2 + i * lineH);
            };
            ctx.restore();
        },
        _getLevelRadius: function (level) {
            var opt = this.options;
            if (opt.areaMode) {
                var S = Math.PI * Math.pow(opt.levelRadius, 2);
                return Math.sqrt((level - opt.selectedNode.level) * S / Math.PI);
            } else {
                return opt.levelRadius * (level - opt.selectedNode.level);
            }
        },
        _setOption: function (key, value) {
            var opt = this.options;
            this._super(key, value);
            if (key == "showLegend") {
                this._resetCenterPoint();
                this._resetLevelRadius();
            }
            if (key == "dataSource") {
                var rootNode = opt.dataSource[0];
                rootNode.percentage = 100;
                rootNode.sAngle = 0;
                rootNode.segmentAngle = 360;
                rootNode.level = 0;
                opt.selectedNode = rootNode;
                opt.mouseOverElement = {};
                this._initHierarchy(rootNode, 1);
                this._resetCenterPoint();
                this._resetLevelRadius();
            }
        },

        _setOptions: function (options) {
            this._super(options);
        },
        _startAnimating: function () {
            var opt = this.options;
            animation.fpsInterval = 1000 / animation.fps;
            animation.then = Date.now();
            animation.startTime = animation.then;
            animation.wid = opt.divCanvas.id;
            animate();
        },
        animate: function () {
            var opt = this.options;

            if (animation.mode == "rotate") {
                initAngle = initAngle + Math.abs(animation.endValue - initAngle) / 4 * (animation.endValue > initAngle ? 1 : -1);
                if (Math.abs(animation.endValue - initAngle) < 1) {
                    initAngle = animation.endValue;
                    animation.mode = "none";
                };
                this.redraw();
            }

            if (animation.mode == "selectSegment") {
                animation.startValue += Math.abs(animation.endValue - animation.startValue) / 4 * (animation.endValue > animation.startValue ? 1 : -1);
                if (Math.abs(animation.endValue - animation.startValue) < 1) {
                    animation.startValue = animation.endValue;
                    animation.mode = "none";
                    if (opt.mouseOverElement !== null) {
                        //var hint = opt.mouseOverElement.name + "//" + Math.round(opt.mouseOverElement.prty * 10000) / 100 + "%";
                        var hint = opt.mouseOverElement.name + "//" + (opt.mouseOverElement.prc) + "%";
                        setTimeout(function () { drawHint(opt.context, hint); }, 500);
                    }
                };
                this.redraw();
            }

            if (animation.mode == "drilldownSegment") {
                animation.startValue += Math.abs(animation.endValue - animation.startValue) / 4 * (animation.endValue > animation.startValue ? 1 : -1);
                //opt.mouseOverElement.percentage = animation.startValue;   

                if (Math.abs(animation.endValue - animation.startValue) < 1) {
                    animate.mode = "none";
                    animation.startValue = animation.endValue;
                    opt.mouseOverElement.percentage = animation.startValue;
                    opt.mouseOverElement.segmentAngle = 360;
                    opt.hMaxLevel = 1;
                    opt.selectedNode = opt.mouseOverElement;
                    this._initHierarchy(opt.mouseOverElement, opt.mouseOverElement.level + 1);
                    this.resize(opt.canvas.width, opt.canvas.height);
                };
                this.redraw();
            }
        }
    });
})(jQuery);

function getRadPoint(x, y, angle, radius) {
    var angleRad = getRadAngle(angle);
    var _x = x + radius * Math.cos(angleRad);
    var _y = y + radius * Math.sin(angleRad);
    return { x: _x, y: _y };
}

function getAngleRadiusFromPoint(x, y, pointx, pointy) {
    var angle = Math.atan((pointy - y) / (pointx - x)) * 180 / Math.PI - initAngle + 90;
    if (pointx - x < 0) angle += 180;
    if (angle < 0) angle += 360;
    var radius = Math.sqrt(Math.pow(pointx - x, 2) + Math.pow(pointy - y, 2));
    return { a: angle, r: radius };
}

var initAngle = 0;

function getRadAngle(angle) {
    return (Math.PI / 180.0) * (angle + initAngle - 90);
}

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
var animation = {
    enabled: true,
    fps: 30,
    fpsInterval: 0,
    startTime: null,
    now: null,
    then: null,
    elapsed: null,
    mode: "none",
    startValue: 0,
    endValue: 100,
    wid: ""
};

function animate() {
    // request another frame
    requestAnimationFrame(animate);
    // calc elapsed time since last loop
    animation.now = Date.now();
    animation.elapsed = animation.now - animation.then;
    // if enough time has elapsed, draw the next frame
    if (animation.elapsed > animation.fpsInterval) {
        // Get ready for next frame by setting then=now, but also adjust for your
        // specified fpsInterval not being a multiple of RAF's interval (16.7ms)
        animation.then = animation.now - (animation.elapsed % animation.fpsInterval);
        //drawing code here
        if (animation.mode !== "none") {
            $("#" + animation.wid).ecChart("animate");
        }
    }
}

function drawHint(ctx, text) {
    $("#" + animation.wid).ecChart("drawHint", ctx, text);
}

function getRelativePosition(canvas, absX, absY) {
    var rect = canvas.getBoundingClientRect();
    return {
        x: absX - rect.left,
        y: absY - rect.top
    };
}

function drawMultilineText(ctx, text, x, y, w, lineHeight) {
    var lines = splitTextToLines(ctx, text, 4, w);
    ctx.beginPath();
    lineY = y - (lines.length * lineHeight) / 2 + lineHeight / 2;
    for (var i = 0; i < lines.length; i++) {
        ctx.fillText(lines[i], x, lineY + i * lineHeight, w);
    };
}

function splitTextToLines(ctx, text, maxLines, maxWidth) {
    var words = text.split(" ");
    var lines = [];
    var line = "";
    for (var i = 0; i < words.length; i++) {
        var word = words[i];
        if (ctx.measureText(line + word).width > maxWidth) {
            lines.push(line);
            line = word;
        } else {
            line += " " + word;
        };
    };
    if (line !== "") lines.push(line);
    return lines;
}

function ColorBrightness(Clr, f) { //-1 < F < 1  
    var r = parseInt(Clr.charAt(1) + Clr.charAt(2), 16);
    var g = parseInt(Clr.charAt(3) + Clr.charAt(4), 16);
    var b = parseInt(Clr.charAt(5) + Clr.charAt(6), 16);

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
    return hex
}