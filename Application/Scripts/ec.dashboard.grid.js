/* javascript plug-ins for Dashboard Grid by SL */
var ecDashboardGridOptions = {
    mode: "view", // "view" "edit"
    width: 100, // width in pixels
    height: 100, // height in pixels
    cols: 8,
    rows: 8,
    backgroundColor: "#e0e0e0",
    cellColor: "#000000",
    selectedColor: "#ffffaa",
    itemColor: "#aaffaa",
    minSelectedCols: 1,
    minSelectedRows: 1,
    selectedCols: 1,
    selectedRows: 1,
    visibleRows: -1,
    cellMargin: 2, // margin between cells in pixels
    margin: 2, // widget margin in pixels
    items: [], // [[2,2],[5,2],[3,4],[5,3],[3,3]], // ignored in edit mode
    showSizes: true,
    onCellClick: null, // function (col, row) { alert(col + "x" + row) },
    _updating: false,
    _hover: false,
};

(function ($) {
    $.widget("expertchoice.ecDashboardGrid", {
        options: ecDashboardGridOptions,
        _create: function () {
            var opt = this.options;
            opt.divCanvas = this.element[0];
            opt.divCanvas.innerHTML = "";
            opt.divCanvas.style.shapeRendering = "crispEdges";
            $(opt.divCanvas).addClass("widget-created");
            if ((is_ie)) {
                if (SVG.supported) {
                    this.redraw()
                } else {
                    $(opt.divCanvas).html("<h6>Error: Your browser is not supported</h6>");
                }
            } else {
                this.redraw()
            }

        },
        _destroy: function () {

        },
        _isViewMode: function () {
            return (this.options.mode != "edit");
        },
        _isEditMode: function () {
            return (this.options.mode == "edit");
        },
        resize: function (width, height) {
            var opt = this.options;
            var w = width;
            var h = height;
            opt.width = w;
            opt.height = h;
            $("#" + opt.divCanvas.id).empty();
            if ((is_ie)) {
                opt.context = SVG(opt.divCanvas.id).size(w, h);
            } else {
                opt.context = SVG().addTo('#' + opt.divCanvas.id).size(w, h);
            }
            this._draw();
        },
        redraw: function () {
            var opt = this.options;
            this.resize(opt.width, opt.height);
        },
        beginUpdate: function () {
            this.options._updating = true;
        },
        endUpdate: function () {
            this.options._updating = false;
            this.redraw();
        },
        _drawSelectedArea: function (col, row, w, h) {
            var opt = this.options;
            var ctx = opt.context;
            var resizingShift = 0.5;
            var x = opt.initMargin + col * (opt.cellWidth + 1) - resizingShift;
            var y = opt.initMargin + row * (opt.cellHeight + 1) - resizingShift;
            var width = w * (opt.cellWidth + 1) - opt.cellMargin;
            var height = h * (opt.cellHeight + 1) - opt.cellMargin;
            opt.selectedArea = ctx.rect(width, height).move(x, y).fill(opt.selectedColor);
            this._showSelectedArea();
            opt.selectedAreaXY = { x: col, y: row };
        },
        _drawItem: function (col, row, w, h) {
            var opt = this.options;
            var ctx = opt.context;
            if (w > opt.cols) w = opt.cols;
            if (h > opt.rows) h = opt.rows;
            var x = opt.initMargin + col * (opt.cellWidth + 1);
            var y = opt.initMargin + row * (opt.cellHeight + 1);
            var width = w * (opt.cellWidth + 1) - 1 - opt.cellMargin;
            var height = h * (opt.cellHeight + 1) - 1 - opt.cellMargin;
            var rect = ctx.rect(width, height).move(x, y).fill(opt.itemColor).stroke({ color: opt.cellColor });
            if (opt.showSizes) {
                if (w >= 2 && h >= 2) {
                    var fs = opt.cellWidth;
                    if (w * fs > 32 || fs > 14) fs = 14;
                    ctx.text(h + "x" + w).font({ size: fs }).center((x + width / 2).toFixed(2), (y + height / 2).toFixed(2));
                };
            }
            opt.rectItems.push(rect);
            for (var j = 0; j < w; j++) {
                for (var i = 0; i < h; i++) {
                    opt.filledCells.push((col + j) + "x" + (row + i));
                };
            };
        },
        _getFirstEmptyAreaXY: function (w, h) {
            var opt = this.options;
            for (var j = 0; j < opt.rows; j++) {
                for (var i = 0; i < opt.cols; i++) {
                    if (this._isEmptyArea(i, j, w, h)) {
                        return { x: i, y: j };
                    };
                };
            };
            return null;
        },
        _isEmptyArea: function (x, y, w, h) {
            var opt = this.options;
            for (var j = 0; j < w; j++) {
                for (var i = 0; i < h; i++) {
                    if (((x + j) >= opt.cols) || ((y + i) >= opt.rows) || (opt.filledCells.indexOf((x + j) + "x" + (y + i)) >= 0)) {
                        return false;
                    };
                };
            };
            return true;
        },
        _resizeSelectedArea: function (w, h) {
            var opt = this.options;
            var width = w * (opt.cellWidth + 1) - opt.cellMargin;
            var height = h * (opt.cellHeight + 1) - opt.cellMargin;
            opt.selectedArea.animate(50, ">").size(width, height);
            this._showSelectedArea();
        },
        _showSelectedArea: function () {
            if (this._isEditMode()) {
                var opt = this.options;
                if ((opt.selectedArea)) opt.selectedArea.attr({ "fill-opacity": (opt._hover ? 0.5 : 0) });
            }
        },
        _draw: function () {
            if (this.options._updating) return this;
            var opt = this.options;
            var ctx = opt.context;
            var scope = this;
            var w = opt.width;
            var h = opt.height;
            var r = ctx.rect(w, h).move(0, 0).fill(opt.backgroundColor).id("background");
            r.mouseenter(function () {
                opt._hover = true;
                scope._showSelectedArea();
            });
            r.mouseleave(function () {
                opt._hover = false;
                scope._showSelectedArea();
            });
            opt.cellWidth = (w - opt.margin * 2) / opt.cols - 1;
            opt.cellHeight = (h - opt.margin * 2) / opt.rows - 1;
            opt.initMargin = 0.5 + opt.margin + opt.cellMargin / 2;
            opt.cells = [];
            for (var j = 0; j < opt.rows; j++) {
                for (var i = 0; i < opt.cols; i++) {
                    var cell = ctx.rect((opt.cellWidth - opt.cellMargin), (opt.cellHeight - opt.cellMargin));
                    cell.move(opt.initMargin + i * (opt.cellWidth + 1), opt.initMargin + j * (opt.cellHeight + 1));
                    cell.fill(opt.backgroundColor).stroke({ color: opt.cellColor });
                    opt.cells.push(cell);
                };
            };
            if (this._isEditMode()) {
                opt.rectItems = [];
                opt.filledCells = [];
                var posSel = this._getFirstEmptyAreaXY(opt.minSelectedCols, opt.minSelectedRows);
                this._drawItem(posSel.x, posSel.y, opt.selectedCols, opt.selectedRows);
                this._drawInvisibleArea();
                this._drawSelectedArea(posSel.x, posSel.y, opt.selectedCols, opt.selectedRows);
                
                for (var j = 0; j < opt.rows; j++) {
                    for (var i = 0; i < opt.cols; i++) {
                        var cell = ctx.rect(opt.cellWidth + 1, opt.cellHeight + 1);
                        cell.move(opt.initMargin + i * (opt.cellWidth + 1), opt.initMargin + j * (opt.cellHeight + 1));
                        cell.fill("transparent");
                        cell.data("idx", { row: j, col: i });
                        cell.mouseenter(function () {
                            opt._hover = true;
                            var idx = this.data("idx");
                            var row = idx.row;
                            var col = idx.col;
                            if (col <= opt.selectedAreaXY.x + opt.minSelectedCols - 1) { col = opt.minSelectedCols - 1 } else { col -= opt.selectedAreaXY.x };
                            if (row <= opt.selectedAreaXY.y + opt.minSelectedRows - 1) { row = opt.minSelectedRows - 1 } else { row -= opt.selectedAreaXY.y };
                            scope._resizeSelectedArea(col + 1, row + 1);
                        });
                        cell.mouseleave(function () {
                            opt._hover = false;
                            scope._showSelectedArea();
                        });
                        cell.click(function () {
                            var idx = this.data("idx");
                            var row = idx.row;
                            var col = idx.col;
                            if (col <= opt.selectedAreaXY.x + opt.minSelectedCols - 1) { col = opt.minSelectedCols - 1 } else { col -= opt.selectedAreaXY.x };
                            if (row <= opt.selectedAreaXY.y + opt.minSelectedRows - 1) { row = opt.minSelectedRows - 1 } else { row -= opt.selectedAreaXY.y };
                            var selCols = col + 1;
                            var selRows = row + 1;
                            opt.selectedCols = selCols;
                            opt.selectedRows = selRows;
                            opt._hover = false;
                            scope.redraw();
                            if (isFunction(opt.onCellClick)) {
                                opt.onCellClick(selCols, selRows);
                            };
                        });
                    };
                };
            };
            if (this._isViewMode()) {
                opt.rectItems = [];
                opt.filledCells = [];
                for (var i = 0; i < opt.items.length; i++) {
                    var item = opt.items[i];
                    var w = item[0];
                    var h = item[1];
                    if (w < 1) w = opt.cols;
                    if (w > opt.cols) w = opt.cols;
                    if (h < 1) h = 1;
                    if (h > opt.rows) h = opt.rows;
                    var pos = this._getFirstEmptyAreaXY(w, h);
                    if (pos !== null) {
                        this._drawItem(pos.x, pos.y, item[0], item[1]);
                    };
                };
                this._drawInvisibleArea();

            };
        },
        _drawInvisibleArea: function() {
            var opt = this.options;
            var ctx = opt.context;
            var row = 0;
            if (opt.visibleRows > -1) {
                h = opt.rows - opt.visibleRows;
                row = opt.visibleRows;
            } else { h = 0 };
            if (h > 0) {
                var x = 0;
                var y = opt.initMargin + row * (opt.cellHeight + 1);
                var height = h * (opt.cellHeight + 1) - 1;
                var r = ctx.rect(opt.width, height).move(0, y).fill(opt.backgroundColor).id("invisibleArea").attr({ "fill-opacity": 0.5 });
            }
        },
        _setOption: function (key, value) {
            var opt = this.options;
            this._super(key, value);
            //this.redraw();
        },
        _setOptions: function (options) {
            this._super(options);
        },
    });
})(jQuery);

function isFunction(functionToCheck) {
    return functionToCheck && {}.toString.call(functionToCheck) === '[object Function]';
}