/* javascript plug-ins for Charts by SL */
const ecChartDefOptions = {
    adjustFontSize: 0,
    AlternativesFilterValue: -1,
    allowDrillDown: true,
    areaMode: false,
    autoFontSize: false,
    backgroundColor: "#ffffff",
    blends: [],
    canvas: null,
    chartsPerPage: 1,
    chartType: "altColumns", // objSunburst | objPie | objDoughnut | objColumns | objStacked | altPie | altDoughnut | altColumns | altStacked
    center: { x: 0, y: 0 },
    cols: 1,
    context: null,
    debugMode: false,
    decimals: 2,
    dataSource: null,
    divCanvas: null,
    doughnutMode: true,
    exportMode: false,
    fixSegmentSize: false,
    fontSize: 14,
    fontFamily: "Arial",
    groupByUsers: true,
    height: 200,
    hMaxLevel: 1,
    hierarchyReviewMode: false,
    isIE: false,
    initAngle: 90,
    isRisk: false,
    isRotated: true,
    legendPosition: "auto", // "right" | "bottom" | "auto"
    levelRadius: 80,
    margin: 15,
    maxCols: 0,
    maxFontSize: 30,
    maxRows: 0,
    maxRadius: 100,
    maxWidth: 100,
    maxHeight: 100,
    minFontSize: 9,
    multiSelectUsers: false,
    normalizationMode: "none", // "none" | "normAll" | "norm100"
    onNodeSelected: null, //function(id) {alert(id)},
    rotateLabels: false,
    rotateAngle: 0,
    rows: 1,
    selectedPage: 0,
    showAlternatives: true,
    showAllObjsInLegend: false,
    showExpectedValue: false,
    showSelectedNodePanel: true,
    showComponents: false,
    showGrid: false,
    showLegend: true,
    showLabels: true,
    showLocal: false,
    singleRow: true,
    sortBy: "none", // "none", "name", "value"
    selectedNodeID: -1,
    userIDs: [],
    width: 300,
    WRTNodeID: 1,
    zoomRatio: 1,
};

var ecChartDefOptionsList = ["areaMode", "autoFontSize", "chartsPerPage", "chartType", "decimals", "groupByUsers", "isRotated", "legendPosition", 
                            "normalizationMode", "rotateAngle", "selectedPage", "showComponents", "showLegend", "showLabels", "showLocal",
                            "singleRow", "sortBy", "selectedNodeID", "userIDs", "WRTNodeID", "zoomRatio"];

(function ($) {
    $.widget("expertchoice.ecChart", {
        options: ecChartDefOptions,
        _create: function () {
            var opt = this.options;
            opt.divCanvas = this.element[0];
            opt.divCanvas.innerHTML = "";
            $(opt.divCanvas).addClass("widget-created");
            opt.isIE = navigator.sayswho.indexOf("IE") > -1;
            if ((is_ie)) {
                if (SVG.supported) {
                    this._initChartType();
                    this._initChartsPerPage();
                    this._initSingleRow();
                    opt.initAngle = (opt.isRotated ? 90 : 0);
                    this._initAltsPID();
                    this._setSelectedNodeByID(opt.selectedNodeID);
                    if (!opt.exportMode) this.redraw();
                } else {
                    $(opt.divCanvas).html("<h6>Error: Your browser is not supported</h6>");
                }
            } else {
                this._initChartType();
                this._initChartsPerPage();
                this._initSingleRow();
                opt.initAngle = (opt.isRotated ? 90 : 0);
                this._initAltsPID();
                this._setSelectedNodeByID(opt.selectedNodeID);
                if (!opt.exportMode) this.redraw();
            }
        },
        _destroy: function () {
            if ((this.options) && (this.options.divCanvas)) $(this.options.divCanvas).removeClass("widget-created").html("");
        },
        getChangedOptions: function () {
            var opt = this.options;
            var opts = ecChartDefOptions;
            var curOptions = {};
            for (var i = 0; i < ecChartDefOptionsList.length; i++) {
                var prop = ecChartDefOptionsList[i];
                if (Object.prototype.hasOwnProperty.call(opts, prop)) {
                    if (opts[prop] !== opt[prop]) {
                        curOptions[prop] = opt[prop];
                    }
                }
            }
            return curOptions;
        },
        _getChartAreaSize: function () {
            var opt = this.options;
            return {
                width: Math.floor(opt.showLegend && (opt.legendPosition == "right" || (opt.legendPosition == "auto" && opt.width > opt.height)) ? opt.width - opt.legendWidth : opt.width), // - this.fontSize(false) * (this.chartView() == "columns" ? 3 : 0),
                height: Math.floor((opt.showLegend && (opt.legendPosition == "bottom" || (opt.legendPosition == "auto" && opt.width <= opt.height)) ? opt.height - opt.legendHeight - this.fontSize(false) : opt.height))// - this.fontSize(false)) //* (this.chartView() == "columns" ? 2 : 1)
            }
        },
        _resetChartPosition: function (idx, isSquare, chartsCount) {
            var opt = this.options;
            var areaSize = this._getChartAreaSize();
            var w = areaSize.width;
            var h = areaSize.height;
            var rows = 1;
            var cols = 1;
            var maxLW = w;
            var maxLH = h;
            while (cols * rows < chartsCount) {
                maxLW = w / (cols + 1);
                maxLH = h / (rows + 1);
                if (maxLW > maxLH) {
                    if (opt.maxCols == 0 || opt.maxCols > cols) {
                        cols += 1;
                        while (cols * rows >= chartsCount + cols) {
                            rows -= 1;
                        }
                    } else {
                        rows += 1;
                        while (cols * rows >= chartsCount + rows) {
                            cols -= 1;
                        }
                    };
                } else {
                    if (opt.maxRows == 0 || opt.maxRows > rows) {
                        rows += 1;
                        while (cols * rows >= chartsCount + rows) {
                            cols -= 1;
                        }
                    } else {
                        cols += 1;
                        while (cols * rows >= chartsCount + cols) {
                            rows -= 1;
                        }
                    }

                }

            };
            maxLW = w / cols;
            maxLH = h / rows;
            opt.maxWidth = maxLW;
            opt.maxHeight = maxLH;
            opt.maxRadius = Math.min(opt.maxWidth / 2, opt.maxHeight / 2);
            opt.cols = cols;
            opt.rows = rows;

            var maxWidth = cols * opt.maxWidth;
            var widthShift = 0
            if (maxWidth < w) {
                widthShift = (w - maxWidth) / 2;
            };

            var maxHeight = rows * opt.maxHeight;
            var heightShift = 0;
            if (maxHeight < h) {
                heightShift = (h - maxHeight) / 2;
            };

            opt.center.x = Math.floor(opt.maxWidth / 2 + widthShift / cols + (opt.maxWidth + widthShift / cols) * (idx % cols));// + this.fontSize(false) * (this.chartView() == "columns" ? 3 : 1);
            opt.center.y = Math.floor(opt.maxHeight / 2 + heightShift / rows + (opt.maxHeight + heightShift / rows) * (Math.floor(idx / cols))); // + this.fontSize(false);

            var lr = (opt.hMaxLevel + 1 - opt.selectedNode.level);
            if (lr == 0) lr = 1;
            if (isNaN(opt.zoomRatio)) opt.zoomRatio = 1;
            if (opt.areaMode) {
                opt.levelRadius = Math.sqrt(Math.pow(opt.maxRadius - this.fontSize(false) * 4, 2) / lr) * opt.zoomRatio
            } else {
                opt.levelRadius = (opt.maxRadius - this.fontSize(false) * 4) / lr * opt.zoomRatio
            };
        },
        chartView: function () {
            var opt = this.options;
            var retVal = "columns";
            switch (opt.chartType) {
                case "objSunburst":
                    retVal = "sunburst";
                    break;
                case "objPie":
                    retVal = "pie";
                    break;
                case "objDoughnut":
                    retVal = "pie";
                    break;
                case "objColumns":
                    retVal = "columns";
                    break;
                case "objStacked":
                    retVal = "stacked";
                    break;
                case "altPie":
                    retVal = "pie";
                    break;
                case "altDoughnut":
                    retVal = "pie";
                    break;
                case "altColumns":
                    retVal = "columns";
                    break;
                case "altStacked":
                    retVal = "stacked";
                    break;
            };
            return retVal;
        },
        rotateAngle: function() {
            var opt = this.options;
            opt.rotateAngle = opt.rotateAngle % 360;
            if (opt.rotateAngle < 0) opt.rotateAngle = 360 + opt.rotateAngle;
            return opt.rotateAngle;
        },
        fontSize: function (isChart) {
            var opt = this.options;
            var retVal = opt.fontSize + opt.adjustFontSize;
            //if (opt.autoFontSize) {
            //    //retVal = (isChart ? this._getAutoFontSize(opt.maxRadius * 2, opt.maxRadius * 2) : this._getAutoFontSize(opt.width, opt.height));
            //    retVal = this._getAutoFontSize(opt.width, opt.height);
            //    retVal += opt.adjustFontSize;
            //};
            //if (retVal < this.minFontSize()) retVal = this.minFontSize();
            if (retVal < this.minFontSize()) retval = this.minFontSize();
            return retVal;
        },
        minFontSize: function () {
            var opt = this.options;
            //return (opt.exportMode ? this._getAutoFontSize(opt.divCanvas.width, opt.divCanvas.height) : opt.minFontSize);
            //var mf = this._getAutoFontSize(opt.width, opt.height) * 0.8;
            //if (mf < opt.minFontSize) mf = opt.minFontSize;
            //return mf
            //return (opt.exportMode ? opt.minFontSize * 3 : opt.minFontSize);
            return opt.minFontSize;
        },
        groupByUsers: function () {
            var opt = this.options;
            var gbu = true; 
            if (opt.chartType == "objColumns" || opt.chartType == "altColumns") gbu = opt.groupByUsers;
            return gbu;
        },
        _getAutoFontSize: function (width, height) {
            return (width > height ? height / 35 : width / 35);
        },
        export: function (filename, width, height, format, multiPage, on_upload) {
            var opt = this.options;
            var oldW = opt.divCanvas.clientWidth;
            var oldH = opt.divCanvas.clientHeight;
            var oldFontSize = opt.fontSize;
            var oldExportMode = opt.exportMode;
            opt.exportMode = true;
            opt.fontSize = width / oldW * oldFontSize;
            height = width / oldW * oldH;
            switch (format) {
                case "PNG":
                    this.resize(width, height);
                    var svgString = $("#" + opt.divCanvas.id).html();
                    showLoadingPanel();
                    //DevExpress.viz.exportFromMarkup(svgString, {
                    //    fileName: filename,
                    //    format: 'png',
                    //    height: height,
                    //    width: width,
                    //    margin: 10,
                    //    svgToCanvas(svg, canvas) {
                    //        const deferred = $.Deferred();

                    //        canvas.getContext('2d')
                    //            .drawSvg(new XMLSerializer().serializeToString(svg), 0, 0, null, null, {
                    //                renderCallback: deferred.resolve,
                    //            });
                    //        return deferred.promise();
                    //    },
                    //});
                    svgToPng(svgString, 10, "white").then(function (img) {
                        download(img, filename + ".png", "image/png");
                    })
                    break;
                case "SVG":
                    this.resize(width, height);
                    var svgString = $("#" + opt.divCanvas.id).html();
                    DevExpress.viz.exportFromMarkup(svgString, {
                        fileName: filename,
                        format: 'SVG',
                        height: height,
                        width: width
                    });
                    //download(svgString, filename + ".svg", "image/svg");
                    break;
                case "PDF":
                    //var pages = Math.ceil(usersCount / opt.chartsPerPage);
                    var doc = new jsPDF((width > height ? "l" : "p"), "mm", "a4");
                    var margin = 5;
                    this.resize(width, height);
                    if (multiPage) {
                        var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
                        var children = this._getChildren(rootNode);
                        var totalCharts = (this.groupByUsers() ? opt.dataSource.priorities.length : children.length);
                        var totalPages = Math.ceil(totalCharts / opt.chartsPerPage);
                        //updatePager(totalCharts, opt.selectedPage, totalPages, opt.chartsPerPage);

                        //var usersCount = opt.dataSource.priorities.length;
                        //var oldUserIDs = opt.userIDs;
                        var oldSelectedPage = opt.selectedPage;
                        //for (var i = 0; i < totalPages; i++) {
                        var scope = this;
                        function getPage(i) {
                            opt.selectedPage = i;
                            //var uid = opt.dataSource.priorities[i].uid;
                            //opt.userIDs = [uid];
                            scope.redraw();
                            var svgString = $("#" + opt.divCanvas.id).html();
                            //var img = this._createPNGImage(svgString, width, height);
                            var imgWidth = Math.round(doc.internal.pageSize.width) - margin * 2.5;
                            var imgHeight = Math.round(imgWidth / oldW * oldH);
                            //doc.text(margin, margin, filename);
                            //doc.addImage(img, "PNG", margin, margin * 2, imgWidth, imgHeight, i, "FAST");
                            //doc.addSVG(svgString, margin, margin, imgWidth, imgHeight);    
                            svgToPng(svgString, 10, "white").then(function (img) {
                                doc.addImage(img, "PNG", margin, margin * 2, imgWidth, imgHeight, "img" + i, "FAST");
                                //download(img, filename + ".png", "image/png");
                                if (i < totalPages - 1) {
                                    doc.addPage();
                                    getPage(i + 1);
                                } else {
                                    doc.save(filename + ".pdf");
                                };
                            });
                            //if (i < totalPages - 1) doc.addPage();
                            //// doc.text(20, 20, "Page 2");
                        };
                        getPage(0);
                        //opt.userIDs = oldUserIDs;
                        opt.selectedPage = oldSelectedPage;
                    } else {
                        var svgString = $("#" + opt.divCanvas.id).html();
                        //var img = this._createPNGImage(svgString, width, height);
                        var imgWidth = Math.round(doc.internal.pageSize.width) - margin * 2.5;
                        var imgHeight = Math.round(imgWidth / oldW * oldH);
                        svgToPng(svgString, 10, "white").then(function (img) {
                            doc.addImage(img, "PNG", margin, margin * 2, imgWidth, imgHeight, "img", "FAST");
                            //download(img, filename + ".png", "image/png");
                            doc.save(filename + ".pdf");
                        })
                        
                    };
                    
                    break;
            };
            opt.fontSize = oldFontSize;
            opt.exportMode = oldExportMode;
            if (!opt.exportMode) this.resize(oldW, oldH);
        },
        _createPNGImage: function (svgString, width, height) {
            var tempCanvas = document.createElement('canvas');
            tempCanvas.height = height;
            tempCanvas.width = width;
            var ctx = tempCanvas.getContext("2d");
            ctx.fillRect(0, 0, 0, 0);
            ctx.clearRect(0, 0, width, height);
            ctx.drawSvg(svgString, 0, 0, width, height);
            var img = tempCanvas.toDataURL();
            return img;
        },
        resize: function (width, height) {
            var opt = this.options;
            var w = width;
            var h = height;
            opt.width = w;
            opt.height = h;
            $("#" + opt.divCanvas.id).empty();
            if ((is_ie)) {
                opt.context = SVG(opt.divCanvas.id).size(w, h);//.attr("alignment-baseline", "before-edge"); //.attr("alignment-baseline", "hanging")
            } else {
                opt.context = SVG().addTo('#' + opt.divCanvas.id).size(w, h);
            }
            $(opt.divCanvas).prop("title", "");
            this._draw();
        },
        redraw: function () {
            var opt = this.options;
            this.resize(opt.width, opt.height);
        },
        _getTotalCharts: function() {
            var opt = this.options;
            var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
            var children = this._getChildren(rootNode);
            var totalCharts = opt.chartsPerPage; //(this.groupByUsers() ? opt.userIDs.length : children.length);
            if (this.groupByUsers() && opt.userIDs.length < opt.chartsPerPage) { totalCharts = opt.userIDs.length };
            if (!this.groupByUsers() && children.length < opt.chartsPerPage) { totalCharts = children.length };
            return totalCharts;
        },
        _draw: function () {
            var opt = this.options;
            var ctx = opt.context;
            var w = opt.width;
            var h = opt.height;
            opt.blends = [];
            var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
            var r = ctx.rect(w - 4, h - 4).move(2, 2).fill(opt.backgroundColor).id(opt.divCanvas.id + "background");
            if (opt.debugMode) r.stroke({ width: 4, color: "green" });
            for (var i = 0; i < opt.dataSource.objs.length; i++) {
                opt.dataSource.objs[i].showInLegend = false;
            };
            opt.userIDs.sort(SortById);
            //remove selected users without priorities
            //var updatedUserIDs = [];
            //for (var i = 0; i < opt.dataSource.priorities.length; i++) {
            //    var uid = opt.dataSource.priorities[i].uid;
            //    updatedUserIDs.push(uid);
            //};
            //opt.userIDs = updatedUserIDs;

            //if (opt.userIDs.length == 0 && opt.dataSource.priorities.length > 0) {
            //    opt.userIDs.push(opt.dataSource.priorities[0].uid);
            //};

            if (opt.userIDs.length > 0) {
                if (this._showComponents() && opt.showAlternatives) {
                    for (var i = 0; i < opt.userIDs.length; i++) {
                        var userID = opt.userIDs[i];
                        var p = this._getUserPriorities(userID);
                        var uComps = (p == null ? [] : p.comps);
                        for (var c = 0; c < uComps.length; c++) {
                            var cItem = uComps[c];
                            var oid = cItem.oid;
                            var obj = this._getObjectiveByID(oid);
                            if (obj !== null) obj.showInLegend = true;
                        };
                    };
                };
                for (var i = 0; i < opt.dataSource.alts.length; i++) {
                    opt.dataSource.alts[i].showInLegend = true;
                };
                var maxVal = 0;
                var children = this._getChildren(rootNode);
                for (var i = 0; i < opt.userIDs.length; i++) {
                    var userID = opt.userIDs[i];
                    for (var n = 0; n < children.length; n++) {
                        var node = children[n];
                        if (!node.isCategory) {
                            var prty = this._getUsersNodePriority(userID, node.id);
                            if (prty == null) prty = 0;
                            if (maxVal < prty) { maxVal = prty };
                        }
                    }
                };
                switch (this.chartView()) {
                    case "sunburst":
                        this._initVisibleNodes(rootNode);
                        break;
                    case "columns":
                    case "stacked":
                        var cnodes = this._getChildren(rootNode);
                        for (var i = 0; i < cnodes.length; i++) {
                            var cnode = cnodes[i];
                            if (this._showComponents()) {
                                var ccnodes = this._getChildren(cnode);
                                if (ccnodes.length == 0) {
                                    cnode.showInLegend = true;
                                } else {
                                    for (var n = 0; n < ccnodes.length; n++) {
                                        ccnodes[n].showInLegend = true;
                                    }
                                }
                            } else {
                                cnode.showInLegend = true;
                            }
                        };
                        break;
                    case "pie":
                        var cnodes = this._getChildren(rootNode);
                        for (var i = 0; i < cnodes.length; i++) {
                            var cnode = cnodes[i];
                            cnode.showInLegend = true;
                        };
                        break;
                };
                this._initHierarchy(rootNode, rootNode.level + 1, opt.userIDs[0]);
                if (opt.showLegend) {
                    var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
                    this._sortNodes(opt.sortBy, nodes);
                    this._drawLegend();
                };
                if (opt.dataSource.priorities.length > 0) this._updatePagerPanel();
                if (this.chartView() == "stacked") {
                    this._resetChartPosition(0, false, 1);
                    this._drawStackedChart();
                } else {
                    if (this.chartView() == "columns") {
                        var totalCharts = this._getTotalCharts();
                        for (var i = 0; i < totalCharts; i++) {
                            var idx = i + opt.selectedPage * opt.chartsPerPage;
                            var uid = opt.userIDs[(this.groupByUsers() ? idx : 0)];
                            opt.hMaxLevel = 1;
                            this._initHierarchy(rootNode, rootNode.level + 1, uid);
                            //var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
                            //this._sortNodes(opt.sortBy, nodes);
                            this._resetChartPosition(i, false, totalCharts);
                            this._drawColumnsChart(i, maxVal);
                        }
                    } else {
                        var totalCharts = this._getTotalCharts();
                        for (var i = 0; i < totalCharts; i++) {
                            var idx = i + opt.selectedPage * opt.chartsPerPage;
                            if (idx < opt.userIDs.length) {
                                var uid = opt.userIDs[idx];
                                opt.hMaxLevel = 1;
                                this._initHierarchy(rootNode, rootNode.level + 1, uid);
                                var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
                                this._sortNodes(opt.sortBy, nodes);
                                switch (this.chartView()) {
                                    case "sunburst":
                                        this._resetChartPosition(i, true, totalCharts);
                                        this._drawSunburstChart(uid);
                                        break;
                                    case "pie":
                                        this._resetChartPosition(i, true, totalCharts);
                                        this._drawPieChart(uid);
                                        break;
                                }
                            }
                        };
                    }
                };
                //if (!opt.exportMode && opt.showSelectedNodePanel && !opt.showAlternatives) { this._drawSelectedNodePanel() };
            } else {
                ctx.text("No Data Available").font({ size: this.fontSize(true), family: opt.fontFamily }).center(opt.width / 2, opt.height / 2);
            };
        },
        _drawColumnsChart: function (idx, maxVal) {
            var opt = this.options;
            var ctx = opt.context;
            var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
            var childrenAll = this._getChildren(rootNode);
            var children = [];
            for (var i = 0; i < childrenAll.length; i++) {
                var node = childrenAll[i];
                if (opt.showAlternatives || !node.isCategory) {
                    children.push(node);
                }
            };

            this._sortNodes(opt.sortBy, children);
            var totalIdx = idx + opt.chartsPerPage * opt.selectedPage;

            if (this.groupByUsers() && opt.userIDs.length <= totalIdx) return false;
            if (!this.groupByUsers() && children.length <= totalIdx) return false;

            var mainchart = ctx.group().id(opt.divCanvas.id + "columnschart" + idx);

            var fs = this.fontSize(true);
            var r = mainchart.rect(opt.maxWidth, opt.maxHeight).center(opt.center.x, opt.center.y).fill(opt.backgroundColor);
            if (opt.debugMode) r.stroke({ width: 1, color: "red" });
            var x = opt.center.x - opt.maxWidth / 2;
            var y = opt.center.y - opt.maxHeight / 2;

            var chartMax = (Math.floor(maxVal * 100) + 1) / 100;
            var totalColumns = (this.groupByUsers() ? children.length : opt.userIDs.length);
            var totalCharts = this._getTotalCharts();

            if (opt.autoFontSize) fs = (opt.isRotated ? (opt.maxHeight) / (totalColumns + 2) * 0.6 : (opt.maxWidth) / (totalColumns * (3 + opt.decimals)));
            if (fs > opt.maxFontSize) fs = opt.maxFontSize;
            var labelFS = fs;
            var showLabelOnBar = true;
            if (labelFS < this.minFontSize()) {
                labelFS = this.minFontSize();
            };
            var chartHeight = opt.maxHeight;
            var chartWidth = opt.maxWidth;

            var width = (opt.isRotated ? (opt.maxHeight - labelFS * 2) / (totalColumns) : (opt.maxWidth - labelFS) / (totalColumns));
            if (this._showComponents()) showLabelOnBar = false;
            var rectWidth = width * (opt.showLabels && !showLabelOnBar ? 0.65 : 0.8);
            var isMinLabelFS = labelFS < (this._showComponents() ? width - rectWidth : rectWidth);
            var exportShift = 0;
            if (opt.exportMode) exportShift = 0;

            var shiftWidthLabels = 0;
            var shiftHeightLabels = 0;
            if (opt.isRotated && opt.showLabels && !isMinLabelFS) { // shift chart for labels
                shiftWidthLabels = opt.maxWidth / 3;
                x += shiftWidthLabels;
                chartWidth -= shiftWidthLabels;
                labelFS = width * 0.9;
                if (labelFS > fs) { labelFS = fs };
                width = (opt.isRotated ? (opt.maxHeight - labelFS * 2) / (totalColumns) : (opt.maxWidth - labelFS) / (totalColumns));
                rectWidth = width * (opt.showLabels && !showLabelOnBar ? 0.65 : 0.8);
                //if (rectWidth < prcFont) prcFont = rectWidth;
            };
            if (!opt.isRotated && opt.showLabels && !isMinLabelFS) {
                shiftHeightLabels = opt.maxHeight / 3;
                chartHeight -= shiftHeightLabels;
                labelFS = width * 0.9;
                if (labelFS > fs) { labelFS = fs };
                width = (opt.isRotated ? (opt.maxHeight - labelFS * 2) / (totalColumns) : (opt.maxWidth - labelFS) / (totalColumns));
                rectWidth = width * (opt.showLabels && !showLabelOnBar ? 0.65 : 0.8);
                //if (rectWidth < prcFont) prcFont = rectWidth;
            };
            
            var expVal = -1;
            var sExp = "";
            if (this.groupByUsers() && opt.showExpectedValue) {
                var uid = this._getUserByID(opt.userIDs[totalIdx]).id;
                var userPriorities = this._getUserPriorities(uid);
                expVal = userPriorities.expval;
                sExp = " - Expected value: " + expVal;
            };
            var chartTitle = "";
            if (this.groupByUsers()) {
                var usr = this._getUserByID(opt.userIDs[totalIdx]);
                if (usr !== null) {
                    chartTitle = usr.name + sExp;
                }
            } else {
                chartTitle = children[totalIdx].name;
            };
            var txt = mainchart.text(chartTitle).font({ size: labelFS, family: opt.fontFamily }).fill("#000000"); //, weight: "bold"
            //this.ellipsisText(txt, Math.floor((opt.isRotated ? chartHeight : chartWidth)));
            if (opt.showGrid) {
                txt.center((opt.isRotated ? x + fs - shiftWidthLabels : opt.center.x), (opt.isRotated ? opt.center.y : opt.center.y + opt.maxHeight / 2 - fs)).rotate((opt.isRotated ? -90 : 0));
            } else {
                //if (opt.isRotated)
                //{ txt.move(x + fs, y + labelFS / 2); y += labelFS * 2; } else
                //{ txt.move(x + fs, y + opt.maxHeight - labelFS * 1.5) };
                if (opt.isRotated) {
                    txt.move(x, y + totalColumns * width);
                } else {
                    txt.move(x + fs, y + opt.maxHeight - fs * 1.5);
                }
                
                chartHeight -= labelFS * 3;
                this.ellipsisText(txt, Math.floor((opt.isRotated ? opt.maxWidth : opt.maxHeight)));
            };
            if (opt.showGrid) {
                if (opt.isRotated) {
                    var showGridValues = (!opt.singleRow || idx == totalCharts - 1);
                    var sp = 0;
                    var ticks = chartWidth / (fs * 4);
                    var step = Math.round(chartMax / ticks * 100) / 100;
                    if (step < 0.01) { step = 0.01 };
                    if (this._showComponents()) chartMax += step;
                    while (sp < chartMax) {
                        var tickX = x + sp * chartWidth / chartMax;
                        mainchart.path(["M", tickX, y, "V", y + opt.maxHeight].join(" ")).stroke({ width: 1, color: "#555555" }).opacity(0.5);
                        if (showGridValues) {
                            mainchart.text((sp == 0 ? "%" : Math.round(sp * 100) + "")).font({ size: fs, family: opt.fontFamily, anchor: "start" }).center(tickX, y + opt.maxHeight + fs).opacity(0.5)
                        };
                        sp += step;
                    };
                    sp = chartMax;
                    tickX = x + sp * chartWidth / chartMax;
                    mainchart.path(["M", tickX, y, "V", y + opt.maxHeight].join(" ")).stroke({ width: 1, color: "#555555" }).opacity(0.5);
                    if (showGridValues) mainchart.text(Math.round(sp * 100) + "").font({ size: fs, family: opt.fontFamily, anchor: "start" }).center(tickX, y + opt.maxHeight + fs).opacity(0.5);
                    y += width / 2;
                } else {
                    var showGridValues = (idx % opt.cols == 0);
                    var sp = 0;
                    var ticks = chartHeight / (fs * 4);
                    var step = Math.round(chartMax / ticks * 100) / 100;
                    if (step < 0.01) { step = 0.01 };
                    if (this._showComponents()) chartMax += step;
                    while (sp < chartMax) {
                        var tickY = y + chartHeight - sp * chartHeight / chartMax;
                        mainchart.path(["M", x, tickY, "H", x + opt.maxWidth].join(" ")).stroke({ width: 1, color: "#555555" }).opacity(0.5);
                        if (showGridValues) {
                            mainchart.text((sp == 0 ? "%" : Math.round(sp * 100) + "")).font({ size: fs, family: opt.fontFamily, anchor: "middle" }).center(x - fs / 2, tickY).opacity(0.5)
                        };
                        sp += step;
                    };
                    sp = chartMax;
                    tickY = y + chartHeight - sp * chartHeight / chartMax;
                    mainchart.path(["M", x, tickY, "H", x + opt.maxWidth].join(" ")).stroke({ width: 1, color: "#555555" }).opacity(0.5);
                    if (showGridValues) mainchart.text(Math.round(sp * 100) + "").font({ size: fs, family: opt.fontFamily, anchor: "middle" }).center(x - fs / 2, tickY).opacity(0.5);
                    x += width / 2;
                }
            }

            rootNode.showInLegend = true;
            var maxPrtyTxt = mainchart.text(roundedValue(1000, opt.decimals) + "%").font({ size: labelFS, family: opt.fontFamily });
            var maxPrtyWidth = maxPrtyTxt.bbox().width + exportShift * (opt.decimals + 4) * 0.5;
            maxPrtyTxt.clear();
            var noData = false;
            for (var i = 0; i < totalColumns; i++) {
                var node = children[(this.groupByUsers() ? i : totalIdx)];
                var userID = opt.userIDs[(this.groupByUsers() ? totalIdx : i)];
                var user = this._getUserByID(userID);
                var prty = this._getUsersNodePriority(userID, node.id);
                var gcol = mainchart.group().id(opt.divCanvas.id + "col" + node.id + "_" + userID);
                if (prty == null) {
                    noData = true;
                    break;
                };
                var prtyString = roundedValue(prty * 100, opt.decimals) + "%";
                
                var componentsHint = "";
                //var text = (opt.showLabels ? (showLabelOnBar || shiftHeightLabels > 0 ? prtyString + " " : "") + (this.groupByUsers() ? node.name : user.name) : "");
                var text = (opt.showLabels ? (this.groupByUsers() ? node.name : user.name) : "");
                if (this.groupByUsers() && opt.isRisk) {
                    switch (node.event_type) {
                        case 0:
                            text = "Risk - " + text;
                            break;
                        case 1:
                            text = "Opportunity - " + text;
                            break;
                        case 3:
                            text = "Multi Level - " + text;
                            break;
                    }
                }

                var blend;
                node.showInLegend = true;

                if (opt.isRotated) {
                    var height = (chartWidth - maxPrtyWidth) / chartMax * prty;
                    if (height <= 0) height = 1;
                    var rectY = y + i * width + (showLabelOnBar ? 0 : (!isMinLabelFS ? 0 : labelFS));
                    var uComps = [];
                    var uPrty = this._getUserPriorities(user.id);
                    if (uPrty != null) uComps = uPrty.comps;
                    if (this._showComponents() && (opt.showAlternatives && uComps.length > 0 || !opt.showAlternatives)) { // && uComps.length > 0
                        var nodeChildren = this._getChildren(node);
                        if (nodeChildren.length == 0) {
                            if (opt.showAlternatives) {
                                // Draw Components for Alternatives
                                var cWidthSum = 0;

                                var compVals = [];
                                var compIds = [];
                                var cSum = 0;
                                for (var c = 0; c < uComps.length; c++) {
                                    var cItem = uComps[c];
                                    if (cItem.aid == node.id) {
                                        compVals.push(cItem.val);
                                        compIds.push(cItem.oid);
                                        cSum += cItem.val;
                                    }
                                };
                                for (var c = 0; c < compVals.length; c++) {
                                    var cVal = compVals[c];
                                    var cWidth = height * cVal / cSum;
                                    var cNode = this._getObjectiveByID(compIds[c]);
                                    var cx = x + cWidthSum;
                                    gcol.rect(cWidth - 1, rectWidth).move(cx, rectY).fill((node.event_type == 1 ? this._getPatternFill(cNode.color) : cNode.color));
                                    var nprty = cVal / cSum * prty;
                                    var nprtyString = roundedValue(nprty * 100, opt.decimals) + "";
                                    componentsHint += (componentsHint == "" ? "" : ", ") + nprtyString + "% " + cNode.name;
                                    if (cWidth >= labelFS * 2) {
                                        gcol.text(nprtyString).font({ size: labelFS, family: opt.fontFamily }).center(cx + cWidth / 2, rectY + rectWidth / 2).fill(getContrastColor(cNode.color));
                                    };
                                    var infoObject = { "oid": node.id, "cid": cNode.id, "uid": user.id, "group": "o" + cNode.id, "hint": user.name + " : " + nprtyString + "% " + cNode.name };
                                    this.setupBlendRect(gcol, cWidth, rectWidth, cx, rectY, opt.divCanvas.id + "blend" + node.id + "_" + cNode.id + "_ " + user.id, infoObject);
                                    cWidthSum += cWidth;
                                };
                                componentsHint = " (" + componentsHint + ")";
                            } else {
                                gcol.rect(height, rectWidth).move(x, rectY).fill((node.event_type == 1 ? this._getPatternFill(node.color) : node.color));
                            }
                        } else {
                            var cWidthSum = 0;
                            for (var c = 0; c < nodeChildren.length; c++) {
                                var nChild = nodeChildren[c];
                                var cWidth = height * nChild.percentage / 100;
                                var cx = x + cWidthSum;
                                gcol.rect(cWidth, rectWidth).move(cx, rectY).fill((node.event_type == 1 ? this._getPatternFill(nChild.color) : nChild.color));
                                var nprty = this._getUsersNodePriority(userID, nChild.id);
                                if (nprty == null) break;
                                var nprtyString = roundedValue(nprty * 100, opt.decimals) + "";
                                //if (rectWidth >= labelFS) {
                                gcol.text(nprtyString).font({ size: labelFS, family: opt.fontFamily }).center(cx + cWidth / 2, rectY + rectWidth / 2).fill(getContrastColor(nChild.color));
                                //};
                                var infoObject = { "oid": node.id, "cid": nChild.id, "uid": userID, "group": "o" + nChild.id, "hint": user.name + " : " + nprtyString + "% " + nChild.name };
                                this.setupBlendRect(gcol, cWidth, rectWidth, cx, rectY, opt.divCanvas.id + "blend" + node.id + "_" + nChild.id + "_ " + userID, infoObject);
                                cWidthSum += cWidth;
                            }
                        }
                    } else {
                        gcol.rect(height, rectWidth).move(x, rectY).fill((this.groupByUsers() ? (node.event_type == 1 ? this._getPatternFill(node.color) : node.color) : user.color));
                    };
                    var labelWidth = 0;
                    if (opt.showLabels) {
                        if (isMinLabelFS) {
                            var ccolor = getContrastColor((this.groupByUsers() ? node.color : user.color));
                            var txtX = x + labelFS / 2;
                            var txtY = y + i * width + (showLabelOnBar ? rectWidth / 2 : labelFS / 2 - 2) + exportShift * rectWidth - labelFS / 2;
                            //var txt = gcol.text(text).font({ size: labelFS }).move(0, labelFS / 4 + (showLabelOnBar ? 0 : labelFS * 0.2) - exportShift * labelFS * 2).fill((showLabelOnBar ? ccolor : "#000000"));
                            var txt = gcol.text(text).font({ size: labelFS, family: opt.fontFamily }).move(txtX, txtY).fill((showLabelOnBar ? ccolor : "#000000"));
                            labelWidth = txt.bbox().width;
                            var showLabelOutside = (height < chartWidth / 2)&&((labelWidth + labelFS * 1.5) > height);
                            if (showLabelOnBar) {
                                if (showLabelOutside) {
                                    txt.clear();
                                } else {
                                    this.ellipsisText(txt, Math.floor(height - labelFS));
                                }
                            };
                        } else {
                            var txt = gcol.text(text).font({ size: (shiftHeightLabels > 0 ? this.minFontSize() : labelFS), family: opt.fontFamily }).move(x - labelFS / 2, y + i * width + rectWidth / 2 - labelFS / 2 - exportShift - exportShift * 10).fill("#000000");
                            this.ellipsisText(txt, Math.floor(shiftWidthLabels));
                            labelWidth = txt.bbox().width;
                            txt.dx(-labelWidth);
                        }
                    };
                    if (true) { //if (prcFont >= this.minFontSize() && (!showLabelOnBar || !opt.showLabels)) {
                        var prtyText = prtyString;
                        if (showLabelOutside && showLabelOnBar) {
                            prtyText += " - " + text;
                        };
                        txt = gcol.text(prtyText).font({ size: labelFS, weight: "normal", family: opt.fontFamily }).fill(getContrastColor(node.color)).leading(0.9).opacity(0.8);
                        var txtWidth = txt.bbox().width;
                        var txtHeight = txt.bbox().height;
                        var tx = x + height - txtWidth / 2 + exportShift / 6 + txtWidth + labelFS / 2;
                        var ty = rectY + rectWidth / 2 - exportShift / 2;
                        txt.center(tx, ty);
                        txt.fill("#000000");
                        this.ellipsisText(txt, Math.floor(chartWidth - height - labelFS));
                        if (opt.debugMode) gcol.circle(4, 4).center(x + height, rectY + rectWidth / 2).fill("red");
                    }
                    if (!this._showComponents()) {
                        var infoObject = { "oid": node.id, "cid": 0, "uid": userID, "group": "o" + (this.groupByUsers() ? node.id : user.id), "hint": user.name + " : " + prtyString + " " + node.name };
                        this.setupBlendRect(gcol, height, rectWidth, x, rectY, opt.divCanvas.id + "blend" + node.id + "_" + userID, infoObject);
                    };
                } else {
                    var height = chartHeight / chartMax * prty;
                    if (height <= 0) height = 1;

                    var rectX = Math.floor(x + i * width) + labelFS;
                    var rectY = Math.floor(y + opt.maxHeight - height - shiftHeightLabels - labelFS * 2);
                    var p = this._getUserPriorities(user.id);
                    var uComps = (p !== null ? p.comps : []);
                    if (this._showComponents() && uComps.length > 0) {
                        var nodeChildren = this._getChildren(node);
                        if (nodeChildren.length == 0) {
                            if (opt.showAlternatives) {
                                // draw Alternatives Components
                                var cWidthSum = 0;
                                var compVals = [];
                                var compIds = [];
                                var cSum = 0;
                                for (var c = 0; c < uComps.length; c++) {
                                    var cItem = uComps[c];
                                    if (cItem.aid == node.id) {
                                        compVals.push(cItem.val);
                                        compIds.push(cItem.oid);
                                        cSum += cItem.val;
                                    }
                                };
                                for (var c = 0; c < compVals.length; c++) {
                                    var cVal = compVals[c];
                                    var cWidth = height * cVal / cSum;
                                    var cNode = this._getObjectiveByID(compIds[c]);
                                    var cy = opt.center.y + opt.maxHeight / 2 - cWidthSum - cWidth - shiftHeightLabels - labelFS * 2;
                                    gcol.rect(rectWidth, cWidth - 1).move(rectX, cy).fill((node.event_type == 1 ? this._getPatternFill(cNode.color) : cNode.color));
                                    var nprty = cVal / cSum * prty;
                                    var nprtyString = roundedValue(nprty * 100, opt.decimals) + "";
                                    componentsHint += (componentsHint == "" ? "" : ", ") + nprtyString + "% " + cNode.name;
                                    //if (cWidth >= labelFS * 6) {
                                    var txt = gcol.text(nprtyString).font({ size: labelFS, family: opt.fontFamily }).center(rectX + rectWidth / 2, cy + cWidth / 2).fill(getContrastColor(cNode.color));
                                        var txtWidth = txt.bbox().width * 1.2;
                                        //if (txtWidth > rectWidth) txt.rotate(-90);
                                    //}
                                    var infoObject = { "oid": node.id, "cid": cNode.id, "uid": user.id, "group": "o" + cNode.id, "hint": user.name + " : " + nprtyString + "% " + cNode.name };
                                    this.setupBlendRect(gcol, rectWidth, cWidth - 1, rectX, cy, opt.divCanvas.id + "blend" + node.id + "_" + cNode.id + "_ " + user.id, infoObject);
                                    cWidthSum += cWidth;
                                };
                                componentsHint = " (" + componentsHint + ")";
                            } else {
                                gcol.rect(rectWidth, height).move(rectX, rectY).fill((node.event_type == 1 ? this._getPatternFill(node.color) : node.color));
                            }
                        } else {
                            var cWidthSum = 0;
                            for (var c = 0; c < nodeChildren.length; c++) {
                                var nChild = nodeChildren[c];
                                var cWidth = height * nChild.percentage / 100;
                                var cy = opt.center.y + opt.maxHeight / 2 - cWidthSum - cWidth - shiftHeightLabels - labelFS * 2;
                                gcol.rect(rectWidth, cWidth).move(rectX, cy).fill((node.event_type == 1 ? this._getPatternFill(nChild.color) : nChild.color));
                                if (rectWidth >= labelFS) {
                                    var nprty = this._getUsersNodePriority(userID, nChild.id);
                                    if (nprty == null) break;
                                    var nprtyString = roundedValue(nprty * 100, opt.decimals) + "";
                                    var txt = gcol.text(nprtyString).font({ size: labelFS, family: opt.fontFamily }).center(rectX + rectWidth / 2, cy + cWidth / 2).fill(getContrastColor(nChild.color));
                                    var txtWidth = txt.bbox().width * 1.2;
                                    if (txtWidth > rectWidth) txt.rotate(-90);
                                }
                                var infoObject = { "oid": node.id, "cid": nChild.id, "uid": userID, "group": "o" + nChild.id, "hint": user.name + " : " + nprtyString + "% " + nChild.name };
                                this.setupBlendRect(gcol, width + 1, cWidth, rectX, cy, opt.divCanvas.id + "blend" + node.id + "_" + nChild.id + "_ " + userID, infoObject);
                                cWidthSum += cWidth;
                            }
                        }
                    } else {
                        gcol.rect(rectWidth, height).move(rectX, rectY).fill((this.groupByUsers() ? (node.event_type == 1 ? this._getPatternFill(node.color) : node.color) : user.color));
                    };
                    var txt;
                    if (opt.showLabels && isMinLabelFS) {
                        var ccolor = getContrastColor((this.groupByUsers() ? node.color : user.color));
                        txt = gcol.text(text).font({ size: labelFS, family: opt.fontFamily }).move(0, labelFS / 2 - exportShift * 10).fill((showLabelOnBar ? ccolor : "#000000"));
                        var txtX = (is_ie ? (showLabelOnBar ? rectX + rectWidth / 2 - labelFS : rectX - labelFS * 1.5) : (showLabelOnBar ? rectX + rectWidth / 2 + labelFS * 0.5 - 5 : rectX - 5)); //rectX - labelFS * 1.7 + (showLabelOnBar ? rectWidth / 2 + labelFS * 0.7 : labelFS * 0.2) + exportShift;
                        var txtY = y + opt.maxHeight - fs * 2 - labelFS / 2;
                        var textVertSize = (showLabelOnBar ? txtY - height + labelFS : txtY - opt.maxHeight);
                        var p = ["M", txtX, txtY, "V", textVertSize].join(" ");
                        if (textVertSize < txtY) { txt.path(p) } else { txt.clear(); };
                        
                        //if (ccolor !== "#000000" && showLabelOnBar) {
                        //    txt = gcol.text(text).font({ size: labelFS }).move(0, labelFS / 2).fill(ccolor);
                        //    p = ["M", txtX, txtY, "V", textVertSize].join(" ");
                        //    txt.path(p);
                        //};
                    } else {
                        txt = gcol.text(text).font({ size: (shiftHeightLabels > 0 ? this.minFontSize() : labelFS), family: opt.fontFamily }).move(0, labelFS / 2).fill("#000000");
                        var p = ["M", rectX - labelFS * 1.7 + rectWidth / 2 + labelFS * 0.7 + exportShift, y + opt.maxHeight - fs * 2, "V", y + chartHeight].join(" ");
                        this.ellipsisText(txt, Math.floor(shiftHeightLabels));
                        txt.path(p);
                    };

                    if (true) { // (prcFont >= this.minFontSize() && (!showLabelOnBar || !opt.showLabels)) {
                        var tx = rectX + rectWidth / 2;
                        var ty = rectY;
                        txt = gcol.text(prtyString).font({ size: labelFS, weight: "normal", family: opt.fontFamily }).fill("#000000").leading(0.9).opacity(0.8).center(tx - exportShift, ty - labelFS / 2 - exportShift / 2);
                        if (opt.debugMode) gcol.circle(4, 4).center(tx, ty).fill("red");
                        var txtWidth = txt.bbox().width * 1.2;
                        //if (txtWidth > rectWidth) txt.scale(rectWidth / txtWidth);

                        var txtHeight = txt.bbox().height;
                        //if (true) { // (txtHeight * 1.5 > height || this._showComponents()) {
                        //    txt.center(tx, ty - txtHeight * 1.2).fill("#000000");
                        //} else {
                        //    txt.center(tx, ty + txtHeight * 1.2);
                        //};
                        //if (txtWidth > rectWidth) txt.rotate(-90);
                    };
                    if (!this._showComponents()) {
                        var infoObject = { "oid": node.id, "cid": 0, "uid": userID, "group": "o" + (this.groupByUsers() ? node.id : user.id), "hint": user.name + " : " + prtyString + " " + node.name };
                        this.setupBlendRect(gcol, rectWidth, height, rectX, rectY, opt.divCanvas.id + "blend" + node.id + "_" + userID, infoObject);
                    };
                };
                // if (!this._showComponents()) {
                //     var nodeHint = user.name + " : " + prtyString + "% " + node.name + componentsHint;
                //     blend.data("node", { value: node, uid: userID, hint: nodeHint });
                //     if (!opt.exportMode) {
                //         if (this._getChildren(node).length > 0) {
                //             gcol.attr({ cursor: "pointer" });
                //         } else {
                //             gcol.attr({ cursor: "default" });
                //         };
                //         var scope = this;
                //         blend.click(function () {
                //             var nodeObject = this.data("node").value;
                //             var opt = scope.options;
                //             if (scope._getChildren(nodeObject).length > 0) {
                //                 var selNode = scope._getNodeByID(nodeObject.id);
                //                 scope._setSelectedNodeByID(selNode.id);
                //                 scope.redraw();
                //                 if (isFunction(opt.onNodeSelected)) {
                //                     opt.onNodeSelected(opt.selectedNode.id);
                //                 }
                //             }
                //         });
                //         setupBlendRect(blend);
                //     }
                // }
            };
            if (noData) {
                var x = opt.center.x;
                var y = opt.center.y;
                gcol.text("No Data Available").font({ size: labelFS, family: opt.fontFamily }).center(x, y);
            };
        },
        _showComponents: function () {
            var opt = this.options;
            if (this.chartView() == "columns") {
                return opt.showComponents;
            } else {
                return false;
            }
        },
        setupBlendRect: function (parentSVGElement, w, h, x, y, id, infoObject) {
            var scope = this;
            var opt = this.options;
            blend = parentSVGElement.rect(w, h).move(x, y).fill(opt.backgroundColor).opacity(0).id(id);
            if (this.options.debugMode) { blend.stroke({ width: 1, color: "red" }) }
            blend.data("info", { value: infoObject });
            blend.mouseenter(function () {
                var obj = this.data("info");
                scope._hoverColumn(obj.value, true);
            });
            blend.mouseleave(function () {
                var obj = this.data("info");
                var opt = scope.options;
                $(opt.divCanvas).prop("title", "");
                scope._hoverColumn(obj.value, false);
            });
            this.options.blends.push(blend);
        },
        _drawStackedChart: function () {
            var opt = this.options;
            var ctx = opt.context;
            var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
            var mainchart = ctx.group().id(opt.divCanvas.id + "stackedchart");
            var fs = this.fontSize(true) * 1.2;
            var x = opt.center.x - opt.maxWidth / 2;
            var y = opt.center.y - opt.maxHeight / 2;
            var chartHeight = opt.maxHeight - fs * 2;
            var chartWidth = opt.maxWidth - fs * 2;
            var minWidth = 60;
            var r = mainchart.rect(opt.maxWidth, opt.maxHeight).center(opt.center.x, opt.center.y).fill(opt.backgroundColor);
            if (opt.debugMode) r.stroke({ width: 1, color: "red" });

            var childrenAll = this._getChildren(rootNode);
            this._sortNodes(opt.sortBy, childrenAll);
            var children = [];
            for (var i = 0; i < childrenAll.length; i++) {
                var node = childrenAll[i];
                if (opt.showAlternatives || !node.isCategory) {
                    children.push(node);
                }
            };

            for (var u = 0; u < opt.chartsPerPage; u++) {
                var idx = u + opt.chartsPerPage * opt.selectedPage;
                if (idx < opt.userIDs.length) {
                    var userID = opt.userIDs[idx];
                    var user = this._getUserByID(userID);

                    var width = opt.maxWidth / opt.chartsPerPage;
                    var rectWidth = width * 0.8;
                    var rectX = x + u * width + width * 0.2 / 2;
                    var labelFS = width * 0.15;
                    if (labelFS > fs) labelFS = fs;
                    var gcol = mainchart.group().id(opt.divCanvas.id + "col" + userID);
                    var txt = gcol.text(user.name).font({ size: fs, family: opt.fontFamily });
                    this.ellipsisText(txt, Math.floor(rectWidth));
                    txt.center(rectX + rectWidth / 2, opt.center.y + opt.maxHeight / 2 - fs);
                    txt.data("uname", user.name);
                    txt.mouseenter(function () {
                        $(opt.divCanvas).prop("title", this.data("uname"));
                    });
                    txt.mouseleave(function () {
                        $(opt.divCanvas).prop("title", "");
                    });
                    // var p = ["M", rectX - labelFS * (opt.exportMode ? 1.2 : 1.7), y + opt.maxHeight - fs * 2 - labelFS / 2, "V", y + fs].join(" ");
                    // txt.path(p)
                    //var rootNodePrty = this._getUsersNodePriority(userID, rootNode.id);
                    var totalPrty = 0;
                    for (var c = children.length - 1; c >= 0; c--) {
                        var node = children[c];
                        var prty = this._getUsersNodePriority(userID, node.id);
                        if (prty !== null) totalPrty += prty;
                    };
                    if (totalPrty == 0) totalPrty = 1;
                    var totalHeight = 0;
                    for (var c = children.length - 1; c >= 0; c--) {
                        var node = children[c];
                        var prty = this._getUsersNodePriority(userID, node.id);
                        if (prty == null) break;
                        var height = chartHeight / totalPrty * prty;
                        if (height <= 0) height = 1;
                        var rectY = opt.center.y + opt.maxHeight / 2 - totalHeight - height - fs * 2;
                        var itemGroup = gcol.group().id(opt.divCanvas.id + "itm" + userID + "_" + node.id);
                        var rect = itemGroup.rect(rectWidth, height).move(rectX, rectY).fill((node.event_type == 1 ? this._getPatternFill(node.color) : node.color));
                        var prtyString = roundedValue(prty * 100, opt.decimals) + "";
                        itemGroup.data("item", user.name + " : " + prtyString + "% " + node.name);
                        if (opt.showLabels) {
                            prtyString += " " + node.name;
                        };
                        if (height > fs) {
                            txt = itemGroup.text(prtyString).font({ size: fs, family: opt.fontFamily }).fill(getContrastColor(node.color));
                            this.ellipsisText(txt, Math.floor(rectWidth));
                            txt.center(rectX + rectWidth / 2, rectY + height / 2);
                        };
                        itemGroup.mouseenter(function () {
                            $(opt.divCanvas).prop("title", this.data("item"));
                        });
                        itemGroup.mouseleave(function () {
                            $(opt.divCanvas).prop("title", "");
                        });
                        totalHeight += height;
                    }
                }
            };
        },
        _drawSunburstChart: function (userID) {
            var opt = this.options;
            var ctx = opt.context;
            opt.leftSideLabels = [];
            opt.rightSideLabels = [];
            var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
            var mainchart = ctx.group().id(opt.divCanvas.id + "sunburstchart" + userID);
            if (opt.debugMode) mainchart.rect(opt.maxRadius * 2, opt.maxRadius * 2).center(opt.center.x, opt.center.y).stroke({ color: "red" }).fill("transparent");
            for (var i = opt.hMaxLevel + 1; i >= 0; i--) {
                mainchart.group().id(opt.divCanvas.id + "level" + i + "_" + userID);
            };
            var fs = this.fontSize(true);
            var txt = ctx.text(this._getUserByID(userID).name).font({ size: fs * 1.2, family: opt.fontFamily });
            this.ellipsisText(txt, Math.floor(opt.maxRadius * 2)).center(opt.center.x, opt.center.y + opt.maxHeight / 2 - fs);
            this._drawHierarchy(rootNode, userID, false);
            this._drawHierarchy(rootNode, userID, true);
            this._drawRoot(rootNode, userID);
            var clipRect = ctx.rect(opt.maxWidth - fs / 2, opt.maxHeight - fs * 2).center(opt.center.x, opt.center.y - fs);
            mainchart.clipWith(clipRect);
        },
        _drawPieChart: function (userID) {
            var opt = this.options;
            var ctx = opt.context;
            opt.leftSideLabels = [];
            opt.rightSideLabels = [];
            var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
            var mainchart = ctx.group().id(opt.divCanvas.id + "sunburstchart" + userID);
            if (opt.debugMode) mainchart.rect(opt.maxRadius * 2, opt.maxRadius * 2).center(opt.center.x, opt.center.y).stroke({ color: "red" }).fill("transparent");
            for (var i = opt.hMaxLevel; i >= 0; i--) {
                mainchart.group().id(opt.divCanvas.id + "level" + i + "_" + userID);
            };
            var fs = this.fontSize(true);
            var rootLevelGroup = svgGet(opt.divCanvas.id + "level" + rootNode.level + "_" + userID);
            if (rootLevelGroup) {
                var txt = rootLevelGroup.text(this._getUserByID(userID).name).font({ size: fs * 1.2, family: opt.fontFamily });
                this.ellipsisText(txt, Math.floor(opt.maxRadius * 2)).center(opt.center.x, opt.center.y + opt.maxRadius - fs);
            };
            this._drawPieHierarchy(rootNode, userID);
        },
        _drawRoot: function (rootNode, userID) {
            var opt = this.options;
            var fs = this.fontSize(true);
            var radius = opt.levelRadius;
            var rootLevelGroup = svgGet(opt.divCanvas.id + "level" + rootNode.level + "_" + userID);
            var cy = opt.center.y - fs;
            if (rootLevelGroup && radius > 5) {
                var root = rootLevelGroup.group().id(opt.divCanvas.id + "s" + rootNode.id + "_" + userID);
                root.circle(radius * 2 - fs * 2).center(opt.center.x, cy).fill(rootNode.color).stroke({ width: Math.floor(fs / 6) + 1, color: "#fff" });
                var p = this._getUsersNodePriority(userID, rootNode.id);
                var labeltext = '';
                if (p !== null) {
                    labeltext = roundedValue(p * 100, opt.decimals) + "% ";
                };
                 
                var txtWidth = radius * 2 - fs * 4;
                if (opt.showLabels) labeltext += rootNode.name;

                var txt = root.text(labeltext).font({ size: fs, family: opt.fontFamily });
                var maxLines = Math.floor((radius - fs) / fs);
                var lines = this.getMultilineRadialText(txt, function (i) { return txtWidth }, maxLines);
                txt.remove();
                //lines.unshift(this._getUserByID(userID).name);
                var txtX = opt.center.x - txtWidth / 2;
                var txtY = cy - (fs * lines.length / 2) - fs / 2;
                if (!(is_ie)) txtY += fs * 1.4;
                for (var i = 0; i < lines.length; i++) {
                    var t = root.text(lines[i]).font({ size: fs, family: opt.fontFamily }).move(0, -fs * 2);
                    var tw = t.node.getComputedTextLength();
                    var path = [];
                    path.push("M " + Math.floor(txtX + (txtWidth - tw) / 2));
                    path.push(Math.floor(txtY + i * fs));
                    path.push("H " + Math.floor(txtX + tw + txtWidth - (txtWidth - tw) / 2));
                    path = path.join(" ");
                    t.path(path).fill(getContrastColor(rootNode.color)).font({ size: fs, family: opt.fontFamily }); //.attr("alignment-baseline", "hanging");
                };

                if (rootNode.level > 0 && opt.allowDrillDown) {
                    var btnBack = root.path("M 257.5 445.1 l -22.2 22.2 c -9.4 9.4 -24.6 9.4 -33.9 0 L 7 273 c -9.4 -9.4 -9.4 -24.6 0 -33.9 L 201.4 44.7 c 9.4 -9.4 24.6 -9.4 33.9 0 l 22.2 22.2 c 9.5 9.5 9.3 25 -0.4 34.3 L 136.6 216 H 424 c 13.3 0 24 10.7 24 24 v 32 c 0 13.3 -10.7 24 -24 24 H 136.6 l 120.5 114.8 c 9.8 9.3 10 24.8 0.4 34.3 Z").fill(getContrastColor(rootNode.color)).size(fs, fs).center(opt.center.x, cy + radius - fs * 2);
                    root.attr({ cursor: "pointer" });
                };

                root.data("node", { value: rootNode, uid: userID });
                root.data("hMove", { value: { x: 0, y: 0 } });
                if (!opt.exportMode) {
                    var scope = this;
                    root.mouseenter(function () {
                        var obj = this.data("node");
                        scope._hoverSegment(obj, true, false);
                    });
                    root.mouseleave(function () {
                        var obj = this.data("node");
                        scope._hoverSegment(obj, false, false);
                    });
                    root.click(function () {
                        var opt = scope.options;
                        if (opt.allowDrillDown) {
                            var parentNode = scope._getParent(opt.selectedNode);
                            if (parentNode !== null) {
                                scope._setSelectedNodeByID(parentNode.id);
                                scope.redraw();
                                if (isFunction(opt.onNodeSelected)) {
                                    opt.onNodeSelected(opt.selectedNode.id);
                                }
                            };
                        }
                    });
                }
            };
        },
        _hoverSegment: function (obj, isHover, fromLegend) {
            var opt = this.options;
            for (var i = 0; i < opt.userIDs.length; i++) {
                var userID = opt.userIDs[i];
                var node = svgGet(opt.divCanvas.id + "s" + obj.value.id + "_" + userID);
                if (!node) return;
                var M = node.data("hMove").value;
                if (opt.easterEggMode) node.animate().transform({ rotation: 360 });
                if (isHover) {
                    var p = this._getUsersNodePriority(obj.uid, obj.value.id);
                    if (!fromLegend) $(opt.divCanvas).prop("title", (p !== null ? roundedValue(p * 100, opt.decimals) + "% " : "") + obj.value.name);
                    node.animate(100, "<").transform({ scale: 1.02 }).opacity(0.9);
                    if (opt.selectedNode.id == obj.value.id) {

                    } else {
                        if ((is_ie)) node.animate(100, "<").opacity(0.9).dmove(M.x, M.y);
                    };
                } else {
                    $(opt.divCanvas).prop("title", "");
                    node.animate(200, "<>").transform({ scale: 1 }).opacity(1);
                    if (opt.selectedNode.id == obj.value.id) {
                        
                    } else {
                        if ((is_ie)) node.animate(300, "<>").opacity(1).move(0, 0);
                    };
                };
            };
            var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
            var objID = obj.value.id;
            for (var n = 0; n < nodes.length; n++) {
                var node = nodes[n];
                if (node.showInLegend) {
                    if (isHover) {
                        if (node.id !== objID) {
                            var lbl = svgGet(opt.divCanvas.id + "lblend" + node.id);
                            if (lbl) {
                                if ((is_ie)) { lbl.stop() } else { lbl.timeline().stop() };
                                lbl.animate(100, "<").opacity(0.7);
                            }
                        };
                    } else {
                        var lbl = svgGet(opt.divCanvas.id + "lblend" + node.id);
                        if (lbl) {
                            if ((is_ie)) { lbl.stop() } else { lbl.timeline().stop() };
                            lbl.animate(200, "<>").opacity(0);
                        }
                    }
                }
            }
        },
        _hoverColumn: function (infoObject, isHover) {
            var opt = this.options;
            var hint = "";
            if (isHover) {
                hint = infoObject.hint;
            };
            $(opt.divCanvas).prop("title", hint);
            for (var i = 0; i < opt.blends.length; i++) {
                var blend = opt.blends[i];
                var info = blend.data("info").value;
                if (isHover) {
                    if (info.group !== infoObject.group) {
                        if ((is_ie)) { blend.stop() } else { blend.timeline().stop() };
                        blend.animate(150, "<").opacity(0.5);
                    }
                } else {
                    if ((is_ie)) { blend.stop() } else { blend.timeline().stop() };
                    blend.animate(150, "<>").opacity(0);
                }
            }
        },
        _drawSegment: function (x, y, startAngle, endAngle, radius1, radius2, node, userID, color) {
            var opt = this.options;
            var ctx = opt.context;

            var fs = this.fontSize(true);
            var A = this._getRadPoint(x, y, startAngle, (this.chartView() == "pie" && !opt.doughnutMode ? 1 : radius1));
            var B = this._getRadPoint(x, y, startAngle, radius2);
            var C = this._getRadPoint(x, y, endAngle, radius2);
            var D = this._getRadPoint(x, y, endAngle, (this.chartView() == "pie" && !opt.doughnutMode ? 1 : radius1));

            var segmentAngle = endAngle - startAngle;
            var middleAngle = startAngle + segmentAngle / 2;
            var middleRadius = radius1 + (radius2 - radius1) / 2;

            var BC = this._getRadPoint(x, y, middleAngle, radius2);
            var DA = this._getRadPoint(x, y, middleAngle, (this.chartView() == "pie" && !opt.doughnutMode ? 1 : radius1));
            var M = this._getRadPoint(x, y, middleAngle, middleRadius);
            var M2 = this._getRadPoint(x, y, middleAngle, middleRadius - (radius2 - radius1) / 10);
            var levelGroup = svgGet(opt.divCanvas.id + "level" + node.level + "_" + userID);
            var segmentGroup = levelGroup.group().id(opt.divCanvas.id + "s" + node.id + "_" + userID);
            //var M3 = this._getRadPoint(x, y, middleAngle, ((this._getChildren(node).length > 0 ? opt.maxRadius - this.fontSize(true) * 3 : radius2) + this.minFontSize()));
            var maxRad = (this.chartView() == "sunburst" ? (opt.maxRadius - this.fontSize(false) * 4) * opt.zoomRatio : opt.maxRadius);
            var M3 = this._getRadPoint(x, y, middleAngle, maxRad);
            var M4 = this._getRadPoint(x, y, middleAngle, radius2);

            var path = [
                "M", Math.floor(B.x), Math.floor(B.y),
                "A", Math.floor(radius2), Math.floor(radius2), 0, 0, 1, Math.floor(BC.x), Math.floor(BC.y),
                "A", Math.floor(radius2), Math.floor(radius2), 0, 0, 1, Math.floor(C.x), Math.floor(C.y),
                "L", Math.floor(D.x), Math.floor(D.y),
                "A", Math.floor(radius1), Math.floor(radius1), 0, 0, 0, Math.floor(DA.x), Math.floor(DA.y),
                "A", Math.floor(radius1), Math.floor(radius1), 0, 0, 0, Math.floor(A.x), Math.floor(A.y),
                "z"
            ].join(" ");
            var segment = segmentGroup.path(path);
            segment.fill((node.event_type == 1 ? this._getPatternFill(color) : color));
            var strokeColor = "#ffffff";
            segment.stroke({ color: strokeColor, width: Math.floor(fs / 6) + 1, linecap: 'round', linejoin: 'round' });

            var segmentHeight = radius2 - radius1; //Math.sqrt(Math.pow(Math.abs(A.x - B.x), 2) + Math.pow(Math.abs(A.y - B.y), 2));

            var doRotate = opt.rotateLabels || !opt.showLabels;
            var r1Angle = segmentAngle - 0.5;
            var r1Length = Math.PI * (radius1 + fs) / 180 * Math.abs(r1Angle);
            var r1DegreeLength = r1Length / Math.abs(r1Angle);
            var p = this._getUsersNodePriority(userID, node.id);
            if (p == null) p = 0;
            var text = (node.isCategory ? " " : roundedValue(p * 100, opt.decimals) + "% ");
            if (opt.showLabels) {
                text += node.name;
            };
            if (opt.debugMode) {
                segmentGroup.circle(4, 4).center(M.x, M.y).fill("red");
                //segmentGroup.text(text).font({ size: this.minFontSize() }).fill(getContrastColor(color)).center(M.x, M.y);
            };
            if (((segmentHeight > fs * 4 + fs / 2) && (segmentAngle * r1DegreeLength > fs * 4)) || (!opt.showLabels && (Math.PI * (middleRadius) / 180 * Math.abs(segmentAngle) > fs))) { //

                var txt = ctx.text(text).font({ size: fs, family: opt.fontFamily });
                var maxLines = (doRotate ?
                    Math.floor((r1Length - fs) / fs) :
                    Math.floor((radius2 - radius1 - fs * 2) / fs) + 1);
                var angle = middleAngle + opt.initAngle + this.rotateAngle();
                var textDirection = (doRotate ?
                    (angle % 360 > 180) && (angle % 360 < 360) :
                    (angle % 360 > 90) && (angle % 360 < 270)
                    );
                var lines = [];
                if (!opt.showLabels) {
                    lines.push(text);
                } else {
                    if (doRotate) {
                        lines = this.getMultilineRadialText(txt, function (i) { return radius2 - radius1 - fs }, maxLines)
                    } else {
                        lines = (textDirection ?
                            this.getMultilineRadialText(txt, function (i) { return Math.PI * (radius1 + i * fs) / 180 * Math.abs(segmentAngle - 1) }, maxLines) :
                            this.getMultilineRadialText(txt, function (i) { return Math.PI * (radius1 - i * fs) / 180 * Math.abs(segmentAngle - 1) }, maxLines)
                        );
                    };
                };
                txt.remove();
                var exportShift = (opt.exportMode || opt.isIE ? fs * 1.5 : fs / 4);
                var textVerticalShift = (lines.length - 1) * fs / 2;
                for (var i = 0; i < lines.length; i++) {
                    var txt = segmentGroup.text(lines[i]).font({ size: fs, family: opt.fontFamily, anchor: (opt.showLabels ? "middle" : "start") }).fill(getContrastColor(color)); //.move(0, -fs * 2) , anchor: (opt.showLabels ? "middle" : "start")
                    if (opt.showLabels) {
                        if ((is_ie)) {                 
                            var radius3 = middleRadius - 4 - i * fs + textVerticalShift;
                            var radius4 = middleRadius + 4 + i * fs - textVerticalShift;
                        } else {
                            var radius3 = middleRadius - 4 - i * fs + textVerticalShift;
                            var radius4 = middleRadius + 4 + i * fs - textVerticalShift;
                        }

                        var P1 = this._getRadPoint(x, y, startAngle + 1.5, radius3);
                        var P12 = this._getRadPoint(x, y, middleAngle, radius3);
                        var P2 = this._getRadPoint(x, y, endAngle - 1.5, radius3);
                        var RP1 = this._getRadPoint(x, y, endAngle - 1.5, radius4);
                        var RP12 = this._getRadPoint(x, y, middleAngle, radius4);
                        var RP2 = this._getRadPoint(x, y, startAngle + 1.5, radius4);
                        var rotA = this._getRadPoint(x, y, startAngle + i * fs / r1DegreeLength, radius1 + fs);
                        var rotB = this._getRadPoint(x, y, startAngle + i * fs / r1DegreeLength, radius2 - fs);
                        var rotA1 = this._getRadPoint(x, y, endAngle - i * fs / r1DegreeLength, radius2 - fs);
                        var rotB1 = this._getRadPoint(x, y, endAngle - i * fs / r1DegreeLength, radius1 + fs);
                        var path = "";
                        if (doRotate) {
                            path = (textDirection ? [
                                "M", Math.floor(rotA1.x), Math.floor(rotA1.y),
                                "L", Math.floor(rotB1.x), Math.floor(rotB1.y)
                            ].join(" ") :
                            [
                                "M", Math.floor(rotA.x), Math.floor(rotA.y),
                                "L", Math.floor(rotB.x), Math.floor(rotB.y)
                            ].join(" ")
                            );
                        } else {
                            path = (textDirection ? [
                                "M", Math.floor(RP1.x), Math.floor(RP1.y),
                                "A", Math.floor(radius4), Math.floor(radius4), 0, 0, 0, Math.floor(RP12.x), Math.floor(RP12.y),
                                "A", Math.floor(radius4), Math.floor(radius4), 0, 0, 0, Math.floor(RP2.x), Math.floor(RP2.y)
                            ].join(" ") : [
                                    "M", Math.floor(P1.x), Math.floor(P1.y),
                                    "A", Math.floor(radius3), Math.floor(radius3), 0, 0, 1, Math.floor(P12.x), Math.floor(P12.y),
                                    "A", Math.floor(radius3), Math.floor(radius3), 0, 0, 1, Math.floor(P2.x), Math.floor(P2.y)
                            ].join(" "));
                        };
                        if ((is_ie)) {
                            txt.path(path).textPath().attr('startOffset', '50%');
                        } else {
                            txt.path(path);
                            txt.textPath().attr('startOffset', '50%');
                        };
                        //txt.path(path).textPath().attr('startOffset', '50%'); //.attr('startOffset', '50%'); //.attr("alignment-baseline", "hanging")
                        //segmentGroup.path(path).stroke({width: 1, color: "#000"});
                        //var bbw = txt.bbox().width;
                        //txt.center(bbw, 0);
                    } else {
                        txt.center(M.x, M.y).rotate((middleAngle + opt.initAngle + this.rotateAngle()) % 360 < 180 ? middleAngle + opt.initAngle + this.rotateAngle() - 90 : middleAngle + opt.initAngle + this.rotateAngle() + 90);
                    };
                };
                //console.log(svgGet(txt.id()).node.childNodes[0].childNodes[1].getComputedTextLength() + " " + maxWidth +" "+ node.name);
            } else {
                //var cPoint = segmentGroup.circle(fs / 2).center(M.x, M.y);
                if (opt.chartsPerPage == 1 || opt.userIDs.length == 1) {
                    var isLeftSide = M.x < opt.center.x;
                    var M5y = this._checkLabelsOverlap(isLeftSide, M3.y);
                    var M5x = (isLeftSide ? opt.center.x - maxRad : opt.center.x + maxRad);
                    var maxTextWidth = Math.floor((isLeftSide ? M3.x - fs * 2 : opt.center.x * 2 - M3.x - fs * 2));
                    if (M5y < opt.height && M5y > 0 && (maxTextWidth > fs * 4) && M4.y < opt.height && M3.y < opt.height && M3.y > 0) {
                        segmentGroup.rect(fs, fs).move((isLeftSide ? (M3.x - fs * 2) : M3.x + fs), M5y - fs).fill(color);
                        var txt = segmentGroup.text(text).font({ 'size': fs, family: opt.fontFamily }).fill('#000000');
                        this.ellipsisText(txt, maxTextWidth);
                        var bb = txt.bbox();
                        txt.move((isLeftSide ? (M3.x - bb.width - fs * 2.5) : M3.x + fs * 2.5), M5y - fs);
                        segmentGroup.path(["M", M.x, M.y, "L", M3.x, M3.y, "L", (isLeftSide ? (M3.x - fs) : M3.x + fs), M5y].join(" ")).stroke({ color: "#555555", width: 0.5 }).fill("transparent");
                        segmentGroup.circle(4).center(M.x, M.y).fill("#555555");
                    }
                }
            };
            //var cPoint = segmentGroup.circle(4).move(M.x,M.y);
            // var cPoint2 = segmentGroup.circle(4).move(M2.x,M2.y);
            segmentGroup.data("hMove", { value: { x: Math.floor(M.x - M2.x), y: Math.floor(M.y - M2.y) } });
            segmentGroup.data("node", { value: node, uid: userID });

            if (!opt.exportMode) {
                var scope = this;
                if (this._getChildren(node).length > 0 && scope.options.allowDrillDown) {
                    segmentGroup.attr({ cursor: "pointer" });
                };
                segmentGroup.mouseleave(function () {
                    var obj = this.data("node");
                    scope._hoverSegment(obj, false, false);
                });
                segmentGroup.click(function () {
                    var node = this;
                    var nodeObject = node.data("node").value;
                    var opt = scope.options;
                    if (opt.allowDrillDown) {
                        if (scope._getChildren(nodeObject).length > 0) {
                            var selNode = scope._getNodeByID(nodeObject.id);
                            scope._setSelectedNodeByID(selNode.id);
                            scope.redraw();
                            if (isFunction(opt.onNodeSelected)) {
                                opt.onNodeSelected(opt.selectedNode.id);
                            }
                        }
                    }
                });
                segmentGroup.mouseenter(function () {
                    var obj = this.data("node");
                    scope._hoverSegment(obj, true, false);
                });
            }
        },
        _isLabelSpaceFree: function (items, y, fs, isLeftSide) {
            if (isLeftSide) {
                for (var i = items.length - 1; i >= 0; i--) {
                    var item = items[i];
                    if (Math.abs(item - y) <= fs) {
                        return false;
                    }
                };
            } else {
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    if (Math.abs(item - y) <= fs) {
                        return false;
                    }
                };
            }
            return true;
        },
        _checkLabelsOverlap: function (isLeftSide, y) {
            var opt = this.options;
            var fs = this.fontSize(true);
            var newY1 = y * 1;
            var newY2 = newY1;

            while (!this._isLabelSpaceFree(opt.leftSideLabels, newY1, fs, isLeftSide)) {
                newY1 = newY1 + 1;
            };
            while (!this._isLabelSpaceFree(opt.leftSideLabels, newY2, fs, isLeftSide)) {
                newY2 = newY2 - 1;
            };

            var newY = (Math.abs(y - newY1) < Math.abs(y - newY2) ? newY1 : newY2);
            opt.leftSideLabels.push(newY);
            return newY;
        }, 
        _getRootNode: function () {
            var opt = this.options;
            var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
            for (var i = 0; i < nodes.length; i++) {
                var node = nodes[i];
                if (typeof node.pid == "undefined" || node.pid === -1) return node;
            };
        },
        _getChildren: function (node) {
            var opt = this.options;
            var retVal = [];
            var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
            for (var i = 0; i < nodes.length; i++) {
                if ((nodes[i].pid == node.id)&&(nodes[i].visible || !opt.showAlternatives)) retVal.push(nodes[i]);
            }
            return retVal
        },
        _getParent: function (node) {
            var opt = this.options;
            var retVal = null;
            var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
            for (var i = 0; i < nodes.length; i++) {
                if (nodes[i].id == node.pid) retVal = nodes[i];
            }
            return retVal;
        },
        _getNodeByID: function (id) {
            var opt = this.options;
            var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
            for (var i = 0; i < nodes.length; i++) {
                if (nodes[i].id == id) return nodes[i];
            };
            return null;
        },
        getObjectiveByID: function (id) {
            return this._getObjectiveByID(id);
        },
        _getObjectiveByID: function (id) {
            var opt = this.options;
            var nodes = opt.dataSource.objs;
            for (var i = 0; i < nodes.length; i++) {
                if (nodes[i].id == id) return nodes[i];
            };
            return null;
        },
        _getUserByID: function (uid) {
            var opt = this.options;
            for (var i = 0; i < opt.dataSource.users.length; i++) {
                if (opt.dataSource.users[i].id == uid) return opt.dataSource.users[i];
            };
            return null;
        },
        _getUserPriorities: function (uid) {
            var opt = this.options;
            for (var i = 0; i < opt.dataSource.priorities.length; i++) {
                if (opt.dataSource.priorities[i].uid == uid) return opt.dataSource.priorities[i];
            };
            return null;
        },
        _getUsersNodePriority: function (uid, nid) {
            var opt = this.options;
            var userPriorities = this._getUserPriorities(uid);
            if (userPriorities == null) return null;
            var maxAltPrty = 0;
            var resVal = null;
            if ((userPriorities)) {
                var nodes = (opt.showAlternatives ? userPriorities.alts : userPriorities.objs);
                if (opt.showAlternatives && opt.normalizationMode == "norm100") {
                    for (var i = 0; i < nodes.length; i++) {
                        if (nodes[i].uprty > maxAltPrty) {
                            maxAltPrty = nodes[i].uprty;
                        };
                    };
                };
                if (opt.showAlternatives && opt.normalizationMode == "normSelected") {
                    for (var i = 0; i < nodes.length; i++) {
                        var alt = this._getNodeByID(nodes[i].aid);
                        if (alt.visible) {
                            maxAltPrty += nodes[i].prty;
                        };
                    };
                };
                if (maxAltPrty == 0) maxAltPrty = 1;
                for (var i = 0; i < nodes.length; i++) {
                    if (opt.showAlternatives) {
                        if (typeof nodes[i].aid !== "undefined") {
                            if (nodes[i].aid == nid) {
                                if (opt.normalizationMode == "normAll") resVal = nodes[i].prty;
                                if (opt.normalizationMode == "none") resVal = nodes[i].uprty;
                                if (opt.normalizationMode == "norm100") resVal = nodes[i].uprty / maxAltPrty;
                                if (opt.normalizationMode == "normSelected") resVal = nodes[i].prty / maxAltPrty;
                            };
                        } else { resVal = 1 }
                    } else {
                        //if (userPriorities.objs[i].oid == nid) return (opt.isNormalized ? userPriorities.objs[i].prty : userPriorities.objs[i].uprty);
                        if (nodes[i].oid == nid) resVal = (opt.showLocal ? nodes[i].lprty : (opt.normalizationMode == "none" ? nodes[i].uprty : nodes[i].prty));
                    };
                };
            }
            return resVal;
        },
        _sortNodes: function (sortBy, nodes) {
            switch (sortBy) {
                case "none":
                    nodes.sort(SortByIndex);
                    break;
                case "name":
                    nodes.sort(SortByName);
                    break;
                case "value":
                    nodes.sort(SortByPercentage);
                    break;
            }
        },
        _initHierarchy: function (node, level, userID) {
            var opt = this.options;
            node.expanded = true;
            var children = this._getChildren(node);
            //node.disabled = (children.length == 0 && !opt.showAlternatives);
            if ((children) && (children.length > 0)) {
                var sumPrty = 0;
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    child.level = level;
                    //if (level > 1) child.color = ColorBrightness(node.color, 0.5);
                    if (level > opt.hMaxLevel) opt.hMaxLevel = level;
                    var p = this._getUsersNodePriority(userID, child.id);
                    if (p == null) p = 1;
                    sumPrty += p;
                    this._initHierarchy(child, level + 1, userID);
                }
                //normalize segments percentage, sum = 100%
                if (sumPrty > 0) {
                    for (var i = 0; i < children.length; i++) {
                        var child = children[i];
                        var p = this._getUsersNodePriority(userID, child.id);
                        if (p == null) p = 1;
                        if (opt.hierarchyReviewMode) p = sumPrty / children.length;
                        child.percentage = p / sumPrty * 100;
                    }
                }
            }
        },
        _initVisibleNodes: function (node) {
            var opt = this.options;
            node.showInLegend = true;
            var children = this._getChildren(node);
            if ((children) && (children.length > 0)) {
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    child.hColor = child.color;
                    //if (this.chartView() == "sunburst") {
                    //    child.hColor = (child.level <= 1 ? child.color : ColorBrightness((typeof node.hColor == "undefined" ? node.color : node.hColor), 0.3));
                    //}
                    this._initVisibleNodes(child);
                }
            }
        },
        _drawHierarchy: function (node, userID, drawTerminal) {
            var opt = this.options;
            var fs = this.fontSize(true);
            var children = this._getChildren(node);
            if ((children) && (children.length > 0)) {
                var sAngle = node.sAngle;
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    if (child.percentage > 0) {
                        var itemPercent = (opt.fixSegmentSize ? (100 / children.length) : child.percentage);
                        var nodeAngle = node.segmentAngle * (itemPercent / 100);
                        child.sAngle = sAngle;
                        child.segmentAngle = nodeAngle;
                        var radius1 = this._getLevelRadius(child.level) - fs;
                        var radius2 = this._getLevelRadius(child.level + 1) - fs;
                        var isTerminal = (this._getChildren(child).length == 0);
                        if ((drawTerminal && isTerminal) || (!drawTerminal && !isTerminal)) {
                            this._drawSegment(opt.center.x, opt.center.y - fs, sAngle, sAngle + nodeAngle, radius1, radius2, child, userID, (child.level == 1 ? child.color : child.hColor));
                        };
                        sAngle += nodeAngle;
                    };
                    this._drawHierarchy(child, userID, drawTerminal);
                };
            };
        },
        _drawPieHierarchy: function (node, userID) {
            var opt = this.options;
            var children = this._getChildren(node);
            var fs = this.fontSize(true);
            if ((children) && (children.length > 0)) {
                var sAngle = node.sAngle;
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    if (child.percentage > 0) {
                        var nodeAngle = node.segmentAngle * (child.percentage / 100);
                        child.sAngle = sAngle;
                        child.segmentAngle = nodeAngle;
                        var radius1 = opt.maxRadius - opt.maxRadius / 3 - fs;
                        var radius2 = opt.maxRadius - fs;
                        this._drawSegment(opt.center.x, opt.center.y - fs, sAngle, sAngle + nodeAngle, radius1, radius2, child, userID, child.color);
                        sAngle += nodeAngle;
                    };
                };
            };
        },
        _getPatternFill: function (color) {
            var opt = this.options;
            var ctx = opt.context;
            var pattern = ctx.pattern(5, 5, function (add) {
                add.rect(5, 5).move(0,0).fill(color);
                //add.polygon('0.5,0.5 5.5,5.5 2.5,5.5 0.5,3.5').fill(color);
                add.line(1, 0, 5, 4).stroke({ width: 1, color: opt.backgroundColor });
                add.line(0, 4, 1, 5).stroke({ width: 1, color: opt.backgroundColor });
            });
            return pattern;
        },
        _drawLegend: function () {
            var opt = this.options;
            var ctx = opt.context.group().id(opt.divCanvas.id + "legend");
            var fs = this.fontSize(false);
            var w = opt.width - fs * 2;
            var h = opt.height;
            var legendMaxWidth = w / 3 - fs;
            var legendMaxHeight = h / 3;
            var nodey = (opt.exportMode ? 2 : 1);
            var vsp = fs / 4;
            var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
            if (!opt.showAlternatives) this._sortNodes("none", nodes);
            var items = (opt.showAlternatives && !this._showComponents() ? opt.dataSource.alts : opt.dataSource.objs);
            var nodes = [];
            for (var i = 0; i < items.length; i++) {
                var itm = items[i];
                if ((opt.showAlternatives && !this._showComponents() && itm.visible) || !opt.showAlternatives || (opt.showAlternatives && this._showComponents())) {
                    nodes.push(itm);
                };
            };
            
            if (opt.legendPosition == "right" || (opt.legendPosition == "auto" && opt.width > opt.height)) {    
                if (!this._showComponents() && !this.groupByUsers()) { nodes = opt.dataSource.users };
                for (var i = 0; i < nodes.length; i++) {
                    var node = nodes[i];
                    if (((!opt.showAlternatives || typeof node.pid !== "undefined") && (opt.showAllObjsInLegend || node.showInLegend)) || (!this._showComponents() && !this.groupByUsers() && opt.userIDs.indexOf(node.id) >= 0)) {
                        var x, y;
                        var tab = (opt.showAllObjsInLegend || this.chartView() == "sunburst" ? node.level - 1 : 0);
                        x = fs * tab + fs * 1.5;
                        y = nodey * (fs + vsp);
                        var maxWidth = Math.min(w - x - fs * 3, legendMaxWidth - x - fs * 3);
                        var isNotTerminal = (this._getChildren(node).length > 0);
                        var g = ctx.group().id(opt.divCanvas.id + "lg" + node.id).attr({ cursor: "pointer" }).opacity((node.showInLegend || (!this._showComponents() && !this.groupByUsers()) ? 1 : 0.4));
                        var hColor = (this.chartView() == "sunburst" ? (node.level >= 1 ? node.hColor : node.color) : node.color);
                        if (node.event_type == 1) {                      
                            hColor = this._getPatternFill(hColor);
                        };
                        if (typeof hColor == "undefined") hColor = node.color;
                        var legendMarker = g.rect(fs, fs).center(Math.floor(x), Math.floor(y - (opt.exportMode ? fs : 1))).fill(hColor).stroke({ color: "#ffffff" }).id(opt.divCanvas.id + "lm" + node.id);
                        //var lNum = g.text(node.idx + "").center(x, y).font({ size: this.minFontSize() });
                        var txt = g.text(node.name).font({ size: fs, family: opt.fontFamily }).move(Math.floor(x + fs), Math.floor(y - vsp * 2.5 - (opt.exportMode ? fs : 0))).id(opt.divCanvas.id + "lt" + node.id);
                        this.ellipsisText(txt, Math.floor(maxWidth));
                        if (!opt.exportMode) {
                            var scope = this;
                            g.data("node", { value: node });
                            if (node.showInLegend) {
                                g.mouseenter(function () {
                                    var obj = this.data("node");
                                    if (scope.chartView() == "sunburst" || scope.chartView() == "pie") scope._hoverSegment(obj, true, true);
                                });
                                g.mouseleave(function () {
                                    var obj = this.data("node");
                                    if (scope.chartView() == "sunburst" || scope.chartView() == "pie") scope._hoverSegment(obj, false, true);
                                });
                            }
                            if (true) {
                                g.click(function () {
                                    var opt = scope.options;
                                    var anode = this.data("node").value;
                                    var id = anode.id;
                                    var pid = anode.pid;
                                    var color = anode.color;
                                    ecColorPicker((opt.showAlternatives ? -1 : 0), true, id, color);
                                    //if (opt.selectedNodeID == id) {
                                    //    id = pid;
                                    //};
                                    //scope._setSelectedNodeByID(id);
                                    //scope.redraw();
                                    //if (isFunction(opt.onNodeSelected)) {
                                    //    opt.onNodeSelected(id);
                                    //}
                                });
                            };

                            var bw = txt.bbox().width + fs * 2;
                            var bh = fs + vsp;
                            var bx = Math.floor(x - fs / 2 - vsp);
                            var by = Math.floor(y - bh / 2);
                            var bid = opt.divCanvas.id + "lblend" + node.id;
                            if (this.chartView() == "columns") {
                                var infoObject = { "oid": node.id, "cid": 0, "uid": 0, "group": "o" + node.id, "hint": node.name };
                                this.setupBlendRect(g, bw, bh, bx, by, bid, infoObject);
                            } else {
                                g.rect(bw, bh).move(bx, by).fill(opt.backgroundColor).opacity(0).id(bid);
                            };
                        };
                        nodey += 1;
                    }
                };
                var lh = ctx.bbox().height;
                var lw = ctx.bbox().width + fs;
                if (lw > legendMaxWidth) lw = legendMaxWidth;
                //ctx.move((legendMaxWidth - lw - fs), 0);
                ctx.move(opt.width - lw, 0);
            } else {
                //if (this._showComponents()) this._sortNodes("none");
                var x = fs;
                var y = 0;
                if (!this._showComponents() && !this.groupByUsers()) { nodes = opt.dataSource.users };
                for (var i = 0; i < nodes.length; i++) {
                    var node = nodes[i];
                    if ((node.showInLegend && (node.pid !== -1 || !opt.showAlternatives) && typeof node.pid !== "undefined") || (!this._showComponents() && !this.groupByUsers() && opt.userIDs.indexOf(node.id) >= 0)) {
                        var maxWidth = 100;
                        var isNotTerminal = (this._getChildren(node).length > 0);
                        var g = ctx.group().id(opt.divCanvas.id + "lg" + node.id).attr({ cursor: "pointer" }).opacity((node.showInLegend || (!this._showComponents() && !this.groupByUsers()) ? 1 : 0.4));
                        var lm = g.rect(fs, fs).center(Math.floor(x), Math.floor(y)).fill((node.event_type == 1 ? this._getPatternFill(node.color) : node.color)).stroke({ color: "#ffffff" }).id(opt.divCanvas.id + "lm" + node.id);
                        var txt = g.text(node.name).font({ size: fs, family: opt.fontFamily }).id(opt.divCanvas.id + "lt" + node.id);
                        txt.move(Math.floor(x + fs), Math.floor(y - vsp * 2));
                        var bw = txt.bbox().width + fs * 3;
                        if (x + fs + bw > opt.width) {
                            x = fs; y += fs * 1.2;
                            txt.move(Math.floor(x + fs), Math.floor(y - vsp * 2));
                            lm.center(Math.floor(x), Math.floor(y));
                        };

                        if (!opt.exportMode) {
                            var scope = this;
                            g.data("node", { value: node });
                            g.mouseenter(function () {
                                var obj = this.data("node");
                                if (scope.chartView() == "sunburst" || scope.chartView() == "pie") scope._hoverSegment(obj, true, true);
                            });
                            g.mouseleave(function () {
                                var obj = this.data("node");
                                if (scope.chartView() == "sunburst" || scope.chartView() == "pie") scope._hoverSegment(obj, false, true);
                            });
                            if (true) {
                                g.click(function () {
                                    var opt = scope.options;
                                    var anode = this.data("node").value;
                                    var id = anode.id;
                                    var pid = anode.pid;
                                    var color = anode.color;
                                    ecColorPicker((opt.showAlternatives ? -1 : 0), true, id, color);
                                    //var opt = scope.options;
                                    //var id = this.data("node").value.id;
                                    //var pid = this.data("node").value.pid;
                                    //if (opt.selectedNodeID == id) {
                                    //    id = pid;
                                    //};
                                    //scope._setSelectedNodeByID(id);
                                    //scope.redraw();
                                    //if (isFunction(opt.onNodeSelected)) {
                                    //    opt.onNodeSelected(id);
                                    //}
                                });
                            };
                            var bh = g.bbox().height;
                            var bx = Math.floor(x - fs / 2 - vsp);
                            var by = Math.floor(y - fs / 2);
                            var bid = opt.divCanvas.id + "lblend" + node.id;
                            if (this.chartView() == "columns") {
                                var infoObject = { "oid": node.id, "cid": 0, "uid": 0, "group": "o" + node.id, "hint": node.name };
                                this.setupBlendRect(g, bw, bh, bx, by, bid, infoObject);
                            } else {
                                g.rect(bw, bh).move(bx, by).fill(opt.backgroundColor).opacity(0).id(bid);
                            };
                        };
                        x += bw;
                    };
                }
                var lh = ctx.bbox().height;
                var lw = ctx.bbox().width;
                //ctx.move((w - lw) / 2, legendMaxHeight - lh - fs * 0.5);
                //ctx.move(0, legendMaxHeight - lh - fs * 0.5);
                ctx.y(h - (legendMaxHeight < lh ? legendMaxHeight : lh) - fs);
            };
            opt.legendWidth = Math.min(ctx.bbox().width + fs, legendMaxWidth);
            opt.legendHeight = Math.min(ctx.bbox().height, legendMaxHeight);
            //if (opt.legendHeight == 0) opt.legendHeight = fs;
            //if (opt.legendWidth == 0) opt.legendWidth = fs;
            //if (opt.legendPosition == "right" || (opt.legendPosition == "auto" && opt.width > opt.height)) {
            //    if (opt.legendWidth > opt.width / 2) opt.legendWidth = opt.width / 2;
            //    ctx.move(opt.width / 2 + fs * 2, 0);
            //};
        },
        _updatePagerPanel: function () {
            var opt = this.options;
            var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
            var children = this._getChildren(rootNode);
            var totalCharts = (this.groupByUsers() ? opt.dataSource.priorities.length : children.length);
            var totalPages = Math.ceil(totalCharts / opt.chartsPerPage);
            if ((!opt.exportMode) && (typeof updatePager == "function")) {
                updatePager(totalCharts, opt.selectedPage, totalPages, opt.chartsPerPage)
            };
        },
        getTotalPages: function () {
            var opt = this.options;
            var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
            var children = this._getChildren(rootNode);
            var totalCharts = (this.groupByUsers() ? opt.dataSource.priorities.length : children.length);
            var totalPages = Math.ceil(totalCharts / opt.chartsPerPage);
            return totalPages;
        },
        _drawSelectedNodePanel: function () {
            var opt = this.options;
            var ctx = opt.context.group().id(opt.divCanvas.id + "selNodePanel");
            var fontSize = this.fontSize(false); //this.minFontSize() + 2;
            var w = opt.width;
            var rh = fontSize * 2;

            ctx.rect(w, rh).center(Math.floor(w / 2), rh / 2).fill("#dddddd").id(opt.divCanvas.id + "selNodeBkg");
            var txt = ctx.text(opt.selectedNode.name).font({ size: fontSize, family: opt.fontFamily, anchor: "start" }).center(w / 2, rh / 2);
            var allowMoveUp = (opt.selectedNode.pid !== -1 && opt.WRTNodeID !== opt.selectedNode.id);
            ctx.style("cursor", (allowMoveUp ? "pointer" : "default"));
            var scope = this;
            if (allowMoveUp) {
                ctx.text(function (d) { return '\uf0e2'; }).font({ family: "Font Awesome 5 Free", size: fontSize }).center(w / 2 - txt.bbox().width / 2 - fontSize, rh / 2);
                ctx.click(function () {
                    var opt = scope.options;
                    var pid = opt.selectedNode.pid;
                    scope._setSelectedNodeByID(pid);
                    scope.redraw();
                    if (isFunction(opt.onNodeSelected)) {
                        opt.onNodeSelected(pid);
                    }
                });
            }
        },
        ellipsisText: function (txt, maxWidth) {
            var text = txt.text();
            if (txt.bbox().width < maxWidth) { return txt };
            var words = text.split(" ");
            var result = text;
            while ((txt.text(result).bbox().width > maxWidth + this.fontSize(false)) && (words.length > 1)) {
                words.pop();
                result = words.join(" ");
            };
            while ((txt.text(result + "\u2026").bbox().width > maxWidth) && result.length > 0) {
                result = result.substring(0, result.length - 2);
            }
            return txt;
        },
        getMultilineRadialText: function (txt, maxWidthFunc, maxLines) {
            var text = txt.text();
            var lines = [];
            //var textIsFit = false;
            //if (txt.bbox().width < maxWidthFunc(0)) {
            //    textIsFit = true;
            //};
            var words = text.split(" ");
            lines.push(words[0]);

            var line = "";
            var addEllipsis = false;
            for (var i = 1; i < words.length; i++) {
                if (txt.text(line + " " + words[i]).node.getComputedTextLength() < maxWidthFunc(lines.length)) {
                    line += (line == "" ? "" : " ") + words[i];
                } else {
                    if (line == "") {
                        line = words[i];
                        if (lines.length < maxLines) { lines.push(line) } else { addEllipsis = true };
                        line = "";
                    } else {
                        if (lines.length < maxLines) { lines.push(line) } else { addEllipsis = true };
                        line = words[i];
                    };
                };
            };
            if (line !== "") {
                if (lines.length < maxLines) { lines.push(line) } else { addEllipsis = true };
            };
            if (addEllipsis) lines[lines.length - 1] += "\u2026";
            return lines;
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
            if (key == "dataSource") {
                this._initAltsPID();
                var rootNode = this._resetRootNode();
                this._setSelectedNodeByID(opt.selectedNodeID);
            }
            if (key == "sortBy") {
                var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
                this._sortNodes(opt.sortBy, nodes);
            }
            if (key == "showAlternatives") {
                this._initShowAlternatives();
            }
            if (key == "selectedNodeID") {
                var rootNode = this._getRootNode();
                rootNode.level = 0;
                this._setSelectedNodeByID(opt.selectedNodeID)
            }
            if (key == "isRotated") {
                if (opt.isRotated) {
                    opt.initAngle = 90
                } else {
                    opt.initAngle = 0
                }
                if (opt.singleRow) {
                    if (opt.isRotated) {
                        opt.maxCols = 1;
                        opt.maxRows = 0;
                    } else {
                        opt.maxCols = 0;
                        opt.maxRows = 1;
                    }
                } else {
                    opt.maxCols = 0;
                    opt.maxRows = 0;
                }
            }
            if (key == "multiSelectUsers") {
                if (!opt.multiSelectUsers) {
                    var uid = opt.userIDs[0];
                    opt.userIDs = [];
                    opt.userIDs.push(uid);
                    opt.chartsPerPage = 1;
                }
            }
            if (key == "groupByUsers") {
                opt.selectedPage = 0;
                var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
                var children = this._getChildren(rootNode);
                var totalCharts = (this.groupByUsers() ? opt.userIDs.length : children.length);
                if (totalCharts <= 10) {
                    opt.chartsPerPage = totalCharts;
                } else {
                    opt.chartsPerPage = 10;
                };
                this._initChartsPerPage();
            }
            if (key == "chartsPerPage") {
                this._initChartsPerPage();
            }
            if (key == "singleRow") {
                this._initSingleRow();
            }
            if (key == "chartType") {
                this._initChartType();
                var rootNode = (opt.showAlternatives ? this._getRootNode() : opt.selectedNode);
                var children = this._getChildren(rootNode);
                var totalCharts = (this.groupByUsers() ? opt.userIDs.length : children.length);
                if (totalCharts <= 10) {
                    opt.chartsPerPage = totalCharts;
                } else {
                    opt.chartsPerPage = 10;
                };
                this._initChartsPerPage();
            }
        },
        _resetRootNode: function () {
            var rootNode = this._getRootNode();
            rootNode.percentage = 100;
            rootNode.sAngle = 0;
            rootNode.segmentAngle = 360;
            rootNode.level = 0;
            return rootNode;
        },
        _initChartType: function () {
            var opt = this.options;
            var oldShowAlts = opt.showAlternatives;
            switch (opt.chartType) {
                case "objSunburst":
                    opt.showAlternatives = false;
                    break;
                case "objPie":
                    opt.doughnutMode = false;
                    opt.showAlternatives = false;
                    break;
                case "objDoughnut":
                    opt.doughnutMode = true;
                    opt.showAlternatives = false;
                    break;
                case "objColumns":
                    opt.showAlternatives = false;
                    break;
                case "objStacked":
                    opt.showAlternatives = false;
                    break;
                case "altPie":
                    opt.doughnutMode = false;
                    opt.showAlternatives = true;
                    break;
                case "altDoughnut":
                    opt.doughnutMode = true;
                    opt.showAlternatives = true;
                    break;
                case "altColumns":
                    opt.showAlternatives = true;
                    break;
                case "altStacked":
                    opt.showAlternatives = true;
                    break;
            };
            if (oldShowAlts !== opt.showAlternatives) {
                this._initShowAlternatives();
            }
        },
        _initChartsPerPage: function () {
            var opt = this.options;
            opt.selectedPage = 0;
            opt.multiSelectUsers = (opt.chartsPerPage > 1);
            if (this.groupByUsers()) {
                //while (opt.chartsPerPage < opt.userIDs.length) {
                //    opt.userIDs.splice(0, 1);
                //};
                while (opt.chartsPerPage > opt.userIDs.length && opt.userIDs.length < opt.dataSource.priorities.length) {
                    for (var i = 0; i < opt.dataSource.priorities.length; i++) {
                        var uID = opt.dataSource.priorities[i].uid;
                        if (opt.userIDs.indexOf(uID) == -1) {
                            opt.userIDs.push(uID);
                        };
                    };
                };
            } else {
                for (var i = 0; i < opt.dataSource.priorities.length; i++) {
                    var uID = opt.dataSource.priorities[i].uid;
                    if (opt.userIDs.indexOf(uID) == -1) {
                        opt.userIDs.push(uID);
                    };
                }
            };
        },
        _initAltsPID: function () {
            var opt = this.options;
            for (var i = 1; i < opt.dataSource.alts.length; i++) {
                var alt = opt.dataSource.alts[i];
                alt.pid = -2;
            };
            for (var i = 0; i < opt.dataSource.objs.length; i++) {
                var node = opt.dataSource.objs[i];
                node.expanded = true;
            };
            var rootNode = this._resetRootNode();
            if (opt.selectedNodeID == -1) opt.selectedNodeID = rootNode.id;
            this._initHierarchy(rootNode, 1, opt.userIDs[0]);
        },
        _initShowAlternatives: function () {
            var opt = this.options;
            opt.selectedPage = 0;
            var rootNode = this._resetRootNode();
            if(opt.chartType.indexOf("alt") >= 0) opt.selectedNodeID = rootNode.id;
            this._initHierarchy(rootNode, 1, opt.userIDs[0]);
            var nodes = (opt.showAlternatives ? opt.dataSource.alts : opt.dataSource.objs);
            this._sortNodes(opt.sortBy, nodes);
            this._setSelectedNodeByID(opt.selectedNodeID);
        },
        _initSingleRow: function () {
            var opt = this.options;
            if (opt.singleRow) {
                if (opt.isRotated) {
                    opt.maxCols = 1;
                    opt.maxRows = 0;
                } else {
                    opt.maxCols = 0;
                    opt.maxRows = 1;
                }
            } else {
                opt.maxCols = 0;
                opt.maxRows = 0;
            }
        },
        _setSelectedNodeByID: function (nodeID) {
            var opt = this.options;
            opt.selectedNodeID = nodeID;
            opt.selectedNode = this._getNodeByID(opt.selectedNodeID);
            if (opt.selectedNode == null) {
                opt.selectedNode = this._getRootNode();
            };
            opt.selectedNode.percentage = 100;
            opt.selectedNode.segmentAngle = 360;
            if (!opt.selectedNode.sAngle) opt.selectedNode.sAngle = 0;
            //opt.selectedNode.sAngle = 0;
        },
        _setOptions: function (options) {
            this._super(options);
        },
        _getRadPoint: function (x, y, angle, radius) {
            var angleRad = this._getRadAngle(angle);
            var _x = x + radius * Math.cos(angleRad);
            var _y = y + radius * Math.sin(angleRad);
            return { x: _x, y: _y };
        },
        _getRadAngle: function (angle) {
            var opt = this.options;
            return (Math.PI / 180.0) * (angle + opt.initAngle + this.rotateAngle() - 90);
        },
    });
})(jQuery);

function svgGet(id) {
    return (((is_ie)) ? SVG.get(id) : SVG('#' + id))
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

function isFunction(functionToCheck) {
    return functionToCheck && {}.toString.call(functionToCheck) === '[object Function]';
}

function SortById(a, b) {
    return (a > b ? 1 : (a < b ? -1 : 0));
}

function SortByIndex(a, b) {
    return ((a.idx > b.idx) ? 1 : ((a.idx < b.idx) ? -1 : 0));
}

function SortByName(a, b) {
    return ((a.name.toLowerCase() > b.name.toLowerCase()) ? 1 : ((a.name.toLowerCase() < b.name.toLowerCase()) ? -1 : 0));
}

function SortByPercentage(a, b) {
    return ((a.percentage < b.percentage) ? 1 : ((a.percentage > b.percentage) ? -1 : 0));
}

navigator.sayswho = (function () {
    var ua = navigator.userAgent, tem,
    M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
    if (/trident/i.test(M[1])) {
        tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
        return 'IE ' + (tem[1] || '');
    }
    if (M[1] === 'Chrome') {
        tem = ua.match(/\b(OPR|Edge)\/(\d+)/);
        if (tem != null) return tem.slice(1).join(' ').replace('OPR', 'Opera');
    }
    M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
    if ((tem = ua.match(/version\/(\d+)/i)) != null) M.splice(1, 1, tem[1]);
    return M.join(' ');
})();

function convertSynthesizeDataToOldFormat(newData, GridWRTNodeID) {
    var oldData = { "objs": [], "alts": [], "users": [], "priorities": [], "wrt": GridWRTNodeID };
    // objs
    for (let id in newData.objs) {
        var nObj = newData.objs[id];
        var obj = { "idx": nObj.idx, "id": nObj.ID, "key_id": [nObj.ID, nObj.ParentNodeID], "name": nObj.Name, "color": nObj.Color, "isTerminal": nObj.isTerminal, "isCategory": nObj.isCategory, "isChildNode": false };
        //if (nObj.ParentNodeID != -1) {
            obj["int_pid"] = nObj.ParentNodeID;
            obj["pid"] = nObj.ParentNodeID;
        //};
        oldData.objs.push(obj);
    };

    // alts
    var alt = { "idx": 0, "id": -2, "name": "Alternatives", "color": "#ffffff", "visible": 1, "event_type": 0 };
    oldData.alts.push(alt);
    for (let id in newData.alts) {
        var nAlt = newData.alts[id];
        alt = { "idx": nAlt.idx, "id": nAlt.ID, "name": nAlt.Name, "color": nAlt.Color, "visible": nAlt.visible, "event_type": nAlt.eventType };
        oldData.alts.push(alt);
    };

    // users
    //{""idx"":{0},""id"":{1},""name"":""{2}"",""email"":""{3}"",""hasdata"":{4},""checked"":{5},""color"":""{6}"", ""group"":{7}}}", globalIdx, usr.UserID, JS_SafeString(usr.UserName), JS_SafeString(usr.UserEMail), Bool2JS(HasData), Bool2JS(IsChecked), sColor, Bool2JS(False))
    idx = 0;
    for (let id in newData.users) {
        var nUser = newData.users[id];
        var usr = { "idx": idx, "id": nUser.id, "name": nUser.name, "email": nUser.email, "hasdata": nUser.hasData, "group": nUser.isGroup, "color": nUser.color };
        oldData.users.push(usr);
        idx++;
    };

    // priorities
    for (let uid in newData.users) {
        var nUser = newData.users[uid];
        var nPrty = { "uid": uid, "objs": [], "alts": [], "comps": [] };

        // {""oid"":{0},""prty"":{1},""uprty"":{2},""lprty"":{3}}
        for (let oid in newData.objPriorities[uid]) {
            var nObjPrty = newData.objPriorities[uid][oid];
            var oPrty = { "oid": oid, "prty": nObjPrty.prty, "uprty": nObjPrty.uprty, "lprty": nObjPrty.lprty };
            nPrty.objs.push(oPrty);
        };

        //{""aid"":{0},""prty"":{1},""uprty"":{2}}
        for (let aid in newData.altPriorities[uid]) {
            var nAltPrty = newData.altPriorities[uid][aid];
            var aPrty = { "aid": aid, "prty": nAltPrty.prty, "uprty": nAltPrty.uprty };
            nPrty.alts.push(aPrty);
            for (var i = 0; i < nAltPrty.comps.length; i++) {
                var cmp = nAltPrty.comps[i];
                nPrty.comps.push({ "aid": aid, "oid": cmp.nodeid, "val": cmp.val });
            }
        };
        oldData.priorities.push(nPrty);
    };
    oldData.prioirities
    return oldData
}
function svgToPng(svgText, margin, fill) {
    // convert an svg text to png using the browser
    return new Promise(function (resolve, reject) {
        try {
            // can use the domUrl function from the browser
            var domUrl = window.URL || window.webkitURL || window;
            if (!domUrl) {
                throw new Error("(browser doesnt support this)")
            }

            // figure out the height and width from svg text
            var match = svgText.match(/height=\"(\d+)/m);
            var height = match && match[1] ? parseInt(match[1], 10) : 200;
            var match = svgText.match(/width=\"(\d+)/m);
            var width = match && match[1] ? parseInt(match[1], 10) : 200;
            margin = margin || 0;

            // it needs a namespace
            if (!svgText.match(/xmlns=\"/mi)) {
                svgText = svgText.replace('<svg ', '<svg xmlns="http://www.w3.org/2000/svg" ');
            }

            // create a canvas element to pass through
            var canvas = document.createElement("canvas");
            canvas.width = width + margin * 2;
            canvas.height = height + margin * 2;
            var ctx = canvas.getContext("2d");


            // make a blob from the svg
            var svg = new Blob([svgText], {
                type: "image/svg+xml;charset=utf-8"
            });

            // create a dom object for that image
            var url = domUrl.createObjectURL(svg);

            // create a new image to hold it the converted type
            var img = new Image;

            // when the image is loaded we can get it as base64 url
            img.onload = function () {
                // draw it to the canvas
                ctx.drawImage(this, margin, margin);

                // if it needs some styling, we need a new canvas
                if (fill) {
                    var styled = document.createElement("canvas");
                    styled.width = canvas.width;
                    styled.height = canvas.height;
                    var styledCtx = styled.getContext("2d");
                    styledCtx.save();
                    styledCtx.fillStyle = fill;
                    styledCtx.fillRect(0, 0, canvas.width, canvas.height);
                    styledCtx.strokeRect(0, 0, canvas.width, canvas.height);
                    styledCtx.restore();
                    styledCtx.drawImage(canvas, 0, 0);
                    canvas = styled;
                }
                // we don't need the original any more
                domUrl.revokeObjectURL(url);
                // now we can resolve the promise, passing the base64 url
                resolve(canvas.toDataURL());
            };

            // load the image
            img.src = url;

        } catch (err) {
            reject('failed to convert svg to png ' + err);
        }
    });
};