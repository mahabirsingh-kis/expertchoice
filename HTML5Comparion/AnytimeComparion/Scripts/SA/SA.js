/* javascript plug-ins for Sensitivity Analysis by SL */

(function ($) {
    $.widget("sa.sa", {

        // Default options
        options: {
            viewMode: 'DSA',
            isMultiView: false,
            normalizationMode: 'unnormalized',
            DSABarHeight: 40,
            PSABarHeight: 64,
            isMobile: false,
            PSAAllowScroll: true,
            PSAObjExpand: true,
            backgroundColor: '#fff',
            DSAInitBarColor: '#777777',
            DSAObjBarColor: '#6baed6',
            DSAAltBarColor: 'rgb(100,250,100)',
            DSATitleFont: '16px Arial',
            DSAFontSize: 16,
            DSAFontFace: 'Arial',
            buttonSize: 32,
            DSASorting: false,
            valDigits: 2,
            altFilterValue: -1,
            PSAShowLines: true,
            PSALineup: true,
            GSAShowLegend: false,
            GSAmarginRight: 40,
            objs: [{ id: 1, idx: 1, name: 'Obj1', value: 0.3, initValue: 0.3, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.5}], gradientMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}], gradientInitMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}], values2D: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}] },
                   { id: 2, idx: 2, name: 'Obj2', value: 0.5, initValue: 0.5, gradientMaxValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.5}], gradientMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7}], gradientInitMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7}], values2D: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}] },
                   { id: 3, idx: 3, name: 'Obj3', value: 0.2, initValue: 0.2, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.3}], gradientMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1}], gradientInitMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1}], values2D: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}]}],
            alts: [{ id: 0, idx: 1, name: 'Alt1', value: 0.7, initValue: 0.7, color: '#95c5f0', visible: 1 },
                   { id: 1, idx: 2, name: 'Alt2', value: 0.3, initValue: 0.3, color: '#fa7000', visible: 1 }],
            objMinNormValues: [],
            objMaxNormValues: [],
            MaxAltValue: 1,
            MinAltValue: 0,
            MaxAltValueNormalized: 1,
            MinAltValueNormalized: 0,
            minNormAltValue: 0,
            maxNormAltValue: 1,
            objGradientSum: [],

            canvas: null,
            context: null,
            canvasRect: null,

            DSAHalfBarHeight: 32,
            canvasHalfWidth: 0,
            AltScrollShift: 0,
            ObjScrollShift: 0,
            isMouseDown: false,
            mouseDownPosition: null,
            mouseDownObjScrollShift: 0,
            mouseDownAltScrollShift: 0,
            isUpdateFinished: true,
            SelectedObjectiveIndex: 0,
            SelectedObjectiveIndexY: 0,
            HTHAlt1Index: 0,
            HTHAlt2Index: 0,
            objBarsAreaHeight: 100,
            altBarsAreaHeight: 100,
            isMouseOverObjectives: true,
            isShowCustomColors: true,
            IsObjValuesChanged: false,
            maxObjIndex: 0,
            mouseOverElement: {},
            interactiveMap: []
        },

        // Constructor function
        _create: function () {
            this.options.canvas = this.element[0];
            this.options.context = this.options.canvas.getContext('2d');
            this.options.canvasRect = this.options.canvas.getBoundingClientRect();
            this.options.DSAHalfBarHeight = this.options.DSABarHeight / 2;
            this.options.canvasHalfWidth = this.options.canvas.width / 2;
            this._updateMinMaxAltBarValue();
            if ((this.options.DSASorting) || (this.options.GSAShowLegend)) { this.options.alts.sort(SortByValue) };

            this.options.objBarsAreaHeight = this.options.DSABarHeight * (this.options.objs.length) + 8;
            this.options.altBarsAreaHeight = this.options.DSABarHeight * (this.options.alts.length) + 8;

            switch (this.options.viewMode) {
                case 'GSA':
                    this.options.altBarsAreaHeight = 24 * (this.options.alts.length) + 22;
                    break;
                case 'PSA':
                    if (this.options.canvas.width <= 300) { this.options.isMobile };
                    if (this.options.isMobile) { this.options.PSABarHeight = 52 } else { this.options.PSABarHeight = 64 };
                    this.options.altBarsAreaHeight = 24 * (this.options.alts.length) + 22;
                    this.options.objBarsAreaHeight = this.options.PSABarHeight * (this.options.objs.length) + 8;
                    break;
                case '2D':
                    if (this.options.objs.length > 1) { this.options.SelectedObjectiveIndex = 1 };
                    break;
            };
            if (this.options.alts.length > 1) { this.options.HTHAlt2Index = 1 };

            this.redrawSA();
            this._on(this.options.canvas, {
                "mousedown": function (event) {
                    var pos = getRelativePosition(this.options.canvas, event.clientX, event.clientY);
                    this.options.mouseDownPosition = pos;
                    if (this.options.viewMode == 'PSA') {this.options.mouseDownObjScrollShift = this.options.ObjScrollShift};
                    this._onMouseDown(pos);
                    return false;
                },
                "touchstart": function (event) {
                    var pos = getRelativePosition(this.options.canvas, event.originalEvent.touches[0].pageX, event.originalEvent.touches[0].pageY);
                    this._onMouseMove(pos);
                    this.options.mouseDownPosition = pos;
                    this._onMouseDown(pos, this);
                    return false;
                },
                "touchmove": function (event) {
                    var pos = getRelativePosition(this.options.canvas, event.originalEvent.touches[0].pageX, event.originalEvent.touches[0].pageY);
                    this._onMouseMove(pos, this);
                    return false;
                }
            });
            this._on($(document), {
                //this._on(this.options.canvas, {
                "mousemove": function (event) {
                    var pos = getRelativePosition(this.options.canvas, event.clientX, event.clientY);
                    this._onMouseMove(pos);
                    return false;
                },
                "mouseup": function (event) {
                    this._onMouseUp();
                    return false;
                },
                //"mouseout": function (event) {
                //    this.options.mouseOverElement = {};
                //    if (this.options.viewMode === 'DSA') {
                //        if (this.options.canvas.height < this.options.objBarsAreaHeight) this._drawScrollbar(true);
                //        if (this.options.canvas.height < this.options.altBarsAreaHeight) this._drawScrollbar(false);
                //    };
                //    this._onMouseUp();
                //    return false;
                //},

                "touchend": function (event) {
                    this._onMouseUp();
                    return false;
                },
                "wheel": function (event) {
                    var Shift = event.originalEvent.deltaY / Math.abs(event.originalEvent.deltaY);

                    if (this.options.viewMode === 'GSA') {
                        if (this.options.isMouseOverObjectives) {
                            this.options.SelectedObjectiveIndex += Shift;
                            if (this.options.SelectedObjectiveIndex < 0) { this.options.SelectedObjectiveIndex = 0 };
                            if (this.options.SelectedObjectiveIndex >= this.options.objs.length) { this.options.SelectedObjectiveIndex = this.options.objs.length - 1 };
                            this.redrawSA();
                            if (this.options.isMultiView) this.syncSACharts(false);
                            $('#selSubobjectives').val(this.options.SelectedObjectiveIndex);
                        } else {
                            if (this.options.mouseOverElement.key === 'legend') {
                                var canvasHeight = this.options.canvas.height;
                                var areaHeight = this.options.altBarsAreaHeight;
                                var scrollShift = this.options.AltScrollShift;
                                var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                                var NewAltScrollShift = this.options.AltScrollShift + 24 * Shift;
                                if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };

                                var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;

                                if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                                    NewAltScrollShift = (areaHeight - canvasHeight);
                                };
                                this.options.AltScrollShift = NewAltScrollShift;
                                this.redrawSA();
                            };
                        };

                    };
                    if (this.options.viewMode === 'DSA') {
                        var canvasHeight = this.options.canvas.height;
                        if (this.options.isMouseOverObjectives) {
                            if (this.options.canvas.height < this.options.objBarsAreaHeight) {
                                var areaHeight = this.options.objBarsAreaHeight;
                                var scrollShift = this.options.ObjScrollShift;
                                var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                                var NewObjScrollShift = this.options.ObjScrollShift + this.options.DSABarHeight * Shift;
                                if (NewObjScrollShift < 0) { NewObjScrollShift = 0 };

                                var sliderTop = NewObjScrollShift * canvasHeight / areaHeight;

                                if ((sliderTop + sliderHeight) > canvasHeight) {
                                    NewObjScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                                };
                                this.options.ObjScrollShift = NewObjScrollShift;
                                this._drawObjectives();
                            };
                        } else {
                            if (this.options.canvas.height < this.options.altBarsAreaHeight) {
                                var areaHeight = this.options.altBarsAreaHeight;
                                var scrollShift = this.options.AltScrollShift;
                                var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                                var NewAltScrollShift = this.options.AltScrollShift + this.options.DSABarHeight * Shift;
                                if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };

                                var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;

                                if ((sliderTop + sliderHeight) > canvasHeight) {
                                    NewAltScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                                };
                                this.options.AltScrollShift = NewAltScrollShift;
                                this._drawAlternatives();
                            };
                        };
                    };
                    if (this.options.viewMode === 'PSA') {
                        if ((this.options.mouseOverElement.key === 'objX') || (this.options.mouseOverElement == '') || (this.options.mouseOverElement == {})) {
                            if ((!this.options.PSAObjExpand) || (this.options.PSAAllowScroll)) {
                                var scrollShift = this.options.ObjScrollShift;
                                var areaWidth = (this.options.objs.length + 2) * this.options.PSABarHeight + this.options.PSABarHeight / 2 - scrollShift;
                                var canvasWidth = this.options.canvasHalfWidth;
                                var NewObjScrollShift = this.options.ObjScrollShift + this.options.PSABarHeight * Shift;
                                //var NewObjScrollShift = this.options.ObjScrollShift + 5*Shift;
                                if (NewObjScrollShift < 0) { NewObjScrollShift = 0 };

                                var sliderLeft = NewObjScrollShift * canvasWidth / areaWidth;
                                var sliderWidth = canvasWidth * canvasWidth / areaWidth;
                                if ((sliderWidth + sliderLeft) > canvasWidth) {
                                    NewObjScrollShift = (canvasWidth - sliderWidth) * areaWidth / canvasWidth + 2;
                                };
                                this.options.ObjScrollShift = NewObjScrollShift;
                                this.redrawSA();
                            };
                        };

                        if (this.options.mouseOverElement.key === 'legend') {
                            var canvasHeight = this.options.canvas.height;
                            var areaHeight = this.options.altBarsAreaHeight;
                            var scrollShift = this.options.AltScrollShift;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewAltScrollShift = this.options.AltScrollShift + 24 * Shift;
                            if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };

                            var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;

                            if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                                NewAltScrollShift = (areaHeight - canvasHeight);
                            };
                            this.options.AltScrollShift = NewAltScrollShift;
                            this.redrawSA();
                        };
                    };
                    return false;
                }
            });
        },
        _onMouseMove: function (pos) {
            if ((this.options.viewMode === 'DSA') || (this.options.viewMode === 'PSA')) {
                this.options.isMouseOverObjectives = (pos.x < this.options.canvasHalfWidth - 10);
                var oldMousePos = this.options.mouseOverElement;
                if (!this.options.isMouseDown) {
                    var ieKey = this._getInteractiveMapElement(pos.x, pos.y);
                    this.options.mouseOverElement = ieKey;
                };
                if (this.options.viewMode === 'DSA') {
                    if (this.options.isMouseDown) {
                        if (this.options.mouseOverElement.key === 'objscroll') {
                            var canvasHeight = this.options.canvas.height;
                            var areaHeight = this.options.objBarsAreaHeight;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewObjScrollShift = pos.y * (areaHeight / canvasHeight) - this.options.mouseDownPosition.y;
                            if (NewObjScrollShift < 0) { NewObjScrollShift = 0 };
                            var sliderTop = NewObjScrollShift * canvasHeight / areaHeight;
                            if ((sliderTop + sliderHeight) > canvasHeight) {
                                NewObjScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                            };
                            this.options.ObjScrollShift = NewObjScrollShift;
                        };
                        if (this.options.mouseOverElement.key === 'altscroll') {
                            var canvasHeight = this.options.canvas.height;
                            var areaHeight = this.options.altBarsAreaHeight;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewAltScrollShift = pos.y * (areaHeight / canvasHeight) - this.options.mouseDownPosition.y;
                            if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };
                            var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;
                            if ((sliderTop + sliderHeight) > canvasHeight) {
                                NewAltScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                            };
                            this.options.AltScrollShift = NewAltScrollShift;
                        };
                    };
                    if (oldMousePos != this.options.mouseOverElement) {
                        if (this.options.canvas.height < this.options.objBarsAreaHeight) {
                            this._drawScrollbar(true);
                        };
                        if (this.options.canvas.height < this.options.altBarsAreaHeight) this._drawScrollbar(false);
                    };
                    if (this.options.isMouseDown) {
                        if (this.options.canvas.height < this.options.objBarsAreaHeight) { this._drawObjectives() };
                        if (this.options.canvas.height < this.options.altBarsAreaHeight) { this._drawAlternatives() };

                    };
                };
                if (this.options.viewMode === 'PSA') {
                    if ((this.options.mouseOverElement.key === 'legend') && (this.options.isMouseDown)) {
                        var canvasHeight = this.options.canvas.height;
                        var areaHeight = this.options.altBarsAreaHeight;
                        var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                        var NewAltScrollShift = pos.y * (areaHeight / canvasHeight) - this.options.mouseDownPosition.y;
                        if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };
                        if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                            NewAltScrollShift = (areaHeight - canvasHeight);
                        };
                        this.options.AltScrollShift = NewAltScrollShift;
                        this.redrawSA();
                        this.options.isUpdateFinished = false;
                    } else {
                        this.options.isMouseOverObjectives = true;
                        if ((this.options.isMouseDown) && (this.options.canvas.width / 2 < this.options.objs.length * this.options.PSABarHeight) && ((this.options.mouseOverElement == '') || (this.options.mouseOverElement.key === 'hscroll') || (this.options.mouseOverElement.key === 'hscrollbar'))) {
                            var canvasHeight = this.options.canvas.width / 2;
                            var areaHeight = this.options.objs.length * this.options.PSABarHeight + 8;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewObjScrollShift = this.options.mouseDownObjScrollShift + this.options.mouseDownPosition.x - pos.x;
                            if (this.options.mouseOverElement.key === 'hscroll') {
                                NewObjScrollShift = pos.x * (areaHeight / canvasHeight) - this.options.mouseDownPosition.x;
                            }
                            if (this.options.mouseOverElement.key === 'hscrollbar') {
                                NewObjScrollShift = this.options.mouseDownObjScrollShift - (this.options.mouseDownPosition.x - pos.x) * (areaHeight / canvasHeight);
                            }
                            if (NewObjScrollShift < 0) { NewObjScrollShift = 0 };
                            var sliderTop = NewObjScrollShift * canvasHeight / areaHeight;
                            if ((sliderTop + sliderHeight) > canvasHeight) {
                                NewObjScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                            };
                            this.options.ObjScrollShift = NewObjScrollShift;
                            this.redrawSA();
                        };
                    };
                    if (oldMousePos != this.options.mouseOverElement) {
                        this._drawHorizontalScrollbar();
                    };
                };
                if (!this.options.isMouseDown) {
                    var OldSelectedObjIndex = this.options.SelectedObjectiveIndex;
                    var NewSelectedObjIndex = this.GetMouseOverObjectiveIndex(pos.x, pos.y);
                    if (isNaN(NewSelectedObjIndex)) { NewSelectedObjIndex = 0 };
                    if (this.options.viewMode === 'PSA') {
                        if (NewSelectedObjIndex == -1) { NewSelectedObjIndex = OldSelectedObjIndex };
                    };
                    this.options.SelectedObjectiveIndex = NewSelectedObjIndex;
                    if (OldSelectedObjIndex != this.options.SelectedObjectiveIndex) {
                        this.redrawSA();
                        if (this.options.isMultiView) this.syncSACharts(false);
                    };
                };
            };
            if (this.options.viewMode === 'GSA') {
                if ((this.options.mouseOverElement.key === 'legend') && (this.options.isMouseDown)) {
                    var canvasHeight = this.options.canvas.height;
                    var areaHeight = this.options.altBarsAreaHeight;
                    var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                    var NewAltScrollShift = pos.y * (areaHeight / canvasHeight) - this.options.mouseDownPosition.y;
                    if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };
                    if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                        NewAltScrollShift = (areaHeight - canvasHeight);
                    };
                    this.options.AltScrollShift = NewAltScrollShift;
                    this.redrawSA();
                };
                if (!this.options.isMouseDown) {
                    var prevMouseOver = this.options.mouseOverElement;
                    var ieElement = this._getInteractiveMapElement(pos.x, pos.y);
                    this.options.isMouseOverObjectives = (ieElement.key === 'objX');
                    this.options.mouseOverElement = {};
                    if (!this.options.isMouseOverObjectives) {
                        this.options.mouseOverElement = ieElement;
                    };
                    if ((prevMouseOver != this.options.mouseOverElement) && (ieElement.key != 'legend')) { this.redrawSA() };
                };
            };

            if (this.options.viewMode === '2D') {
                var prevMouseOver = this.options.mouseOverElement;
                var ieElement = this._getInteractiveMapElement(pos.x, pos.y);
                this.options.mouseOverElement = ieElement;
                if (prevMouseOver != this.options.mouseOverElement) { this.redrawSA() };
            };

            if (this.options.viewMode === 'HTH') {
                var prevMouseOver = this.options.mouseOverElement;
                var ieKey = this._getInteractiveMapElement(pos.x, pos.y);
                this.options.isMouseOverObjectives = (ieKey.key === 'objX');
                this.options.mouseOverElement = ieKey;
                if (prevMouseOver != this.options.mouseOverElement) { this.redrawSA() };
            };

            if (this.options.isUpdateFinished) {
                this.options.isUpdateFinished = false;
                this.updateContent(pos.x, pos.y);
            };
        },
        _onMouseDown: function (pos) {
            this.options.isMouseDown = true;
            this.options.isUpdateFinished = false;
            if ((this.options.viewMode === 'DSA') || (this.options.viewMode === 'PSA')) {
                if (this.options.isMouseOverObjectives) {
                    this.options.SelectedObjectiveIndex = this.GetMouseOverObjectiveIndex(pos.x, pos.y);
                    if (isNaN(this.options.SelectedObjectiveIndex))
                    {
                        this.options.SelectedObjectiveIndex = 0;
                        if (this.options.viewMode === 'PSA') { this._onMouseMove(pos) };
                    } else
                    { this.updateContent(pos.x, pos.y) };
                };
                if (this.options.viewMode === 'DSA') {
                    this.options.mouseDownObjScrollShift = this.options.ObjScrollShift;
                    this.options.mouseDownAltScrollShift = this.options.AltScrollShift;
                    if (this.options.canvas.height < this.options.objBarsAreaHeight) this._drawScrollbar(true);
                    if (this.options.canvas.height < this.options.altBarsAreaHeight) this._drawScrollbar(false);
                    this._onMouseMove(pos);
                };
            };
            if (this.options.viewMode === 'GSA') {
                if (this.options.isMouseOverObjectives) {
                    this.updateContent(pos.x, pos.y);
                } else {
                    if ((this.options.mouseOverElement.key === 'prev') && (this.options.SelectedObjectiveIndex > 0)) { this.options.SelectedObjectiveIndex--; $('#selSubobjectives').val(this.options.SelectedObjectiveIndex); };
                    if ((this.options.mouseOverElement.key === 'next') && (this.options.SelectedObjectiveIndex < this.options.objs.length - 1)) { this.options.SelectedObjectiveIndex++; $('#selSubobjectives').val(this.options.SelectedObjectiveIndex); };
                    this.redrawSA();
                    if (this.options.isMultiView) this.syncSACharts(false);
                };
            };
            if (this.options.viewMode === '2D') {
                if ((this.options.mouseOverElement.key === 'prev') && (this.options.SelectedObjectiveIndex > 0)) { this.options.SelectedObjectiveIndex-- };
                if ((this.options.mouseOverElement.key === 'next') && (this.options.SelectedObjectiveIndex < this.options.objs.length - 1)) { this.options.SelectedObjectiveIndex++ };
                if ((this.options.mouseOverElement.key === 'prevY') && (this.options.SelectedObjectiveIndexY > 0)) { this.options.SelectedObjectiveIndexY-- };
                if ((this.options.mouseOverElement.key === 'nextY') && (this.options.SelectedObjectiveIndexY < this.options.objs.length - 1)) { this.options.SelectedObjectiveIndexY++ };
                this.redrawSA();
            };
            if (this.options.viewMode === 'HTH') {
                if ((this.options.mouseOverElement.key === 'nextAlt1') && (this.options.HTHAlt1Index < this.options.alts.length - 1)) { this.options.HTHAlt1Index++ };
                if ((this.options.mouseOverElement.key === 'prevAlt1') && (this.options.HTHAlt1Index > 0)) { this.options.HTHAlt1Index-- };
                if ((this.options.mouseOverElement.key === 'nextAlt2') && (this.options.HTHAlt2Index < this.options.alts.length - 1)) { this.options.HTHAlt2Index++ };
                if ((this.options.mouseOverElement.key === 'prevAlt2') && (this.options.HTHAlt2Index > 0)) { this.options.HTHAlt2Index-- };
                this.redrawSA();
            };
        },
        _onMouseUp: function () {
            this.options.isMouseDown = false;
            if (this.options.viewMode === 'DSA') {
                if (this.options.canvas.height < this.options.objBarsAreaHeight) this._drawScrollbar(true);
                if (this.options.canvas.height < this.options.altBarsAreaHeight) this._drawScrollbar(false);
            };
            if (this.options.IsObjValuesChanged) { this._dsaUpdateCurrentValues() };
        },
        _updateMinMaxAltBarValue: function () {
            var aMaxVal = 0;
            var aMinVal = 1;
            var aMaxValNorm = 0;
            var aMinValNorm = 1;
            this.options.objGradientSum = [];
            for (var i = 0; i < this.options.objs.length; i++) {
                var xMaxCoef = 0;
                var obj = this.options.objs[i];

                for (var k = 0; k < this.options.alts.length; k++) {
                    var altID = this.options.alts[k].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                    xMaxCoef += maxVal;
                };

                if (xMaxCoef == 0) { xMaxCoef = 1 };
                this.options.objGradientSum.push(xMaxCoef);
                for (var j = 0; j < this.options.alts.length; j++) {
                    var altID = this.options.alts[j].id;
                    var minVal = this.getGradientValueByAltID(obj.gradientMinValues, altID);
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);

                    if (aMaxVal < minVal) { aMaxVal = minVal };
                    if (aMaxVal < maxVal) { aMaxVal = maxVal };
                    if (aMinVal > minVal) { aMinVal = minVal };
                    if (aMinVal > maxVal) { aMinVal = maxVal };

                    var minValNorm = minVal / xMaxCoef;
                    var maxValNorm = maxVal / xMaxCoef;
                    if (aMaxValNorm < minValNorm) { aMaxValNorm = minValNorm };
                    if (aMaxValNorm < maxValNorm) { aMaxValNorm = maxValNorm };
                    if (aMinValNorm > minValNorm) { aMinValNorm = minValNorm };
                    if (aMinValNorm > maxValNorm) { aMinValNorm = maxValNorm };
                };
            };
            if (aMaxVal > 0) { this.options.MaxAltValue = aMaxVal };
            if (aMinVal < 1) { this.options.MinAltValue = aMinVal };
            if (aMaxValNorm > 0) { this.options.MaxAltValueNormalized = aMaxValNorm };
            if (aMinValNorm < 1) { this.options.MinAltValueNormalized = aMinValNorm };
        },

        _getMaxScaleValue: function () {
            var aMaxVal = 0;
            for (var i = 0; i < this.options.objs.length; i++) {
                var obj = this.options.objs[i];
                var xMaxCoef = 0;
                if (this.options.normalizationMode == 'normAll') {
                    for (var k = 0; k < this.options.alts.length; k++) {
                        var altID = this.options.alts[k].id;
                        var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                        xMaxCoef += maxVal;
                    };
                };
                if (xMaxCoef == 0) { xMaxCoef = 1 };
                for (var j = 0; j < this.options.alts.length; j++) {
                    var altID = this.options.alts[j].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID) / xMaxCoef;
                    if (aMaxVal < maxVal) { aMaxVal = maxVal };
                };
            };
            if (aMaxVal > 0) { return aMaxVal };
            return 1;
        },
        _getMaxObjScaleValue: function (obj) {
            var aMaxVal = 0;
            var xMaxCoef = 0;
            if (this.options.normalizationMode == 'normAll') {
                for (var k = 0; k < this.options.alts.length; k++) {
                    var altID = this.options.alts[k].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                    xMaxCoef += maxVal;
                };
            };
            if (xMaxCoef == 0) { xMaxCoef = 1 };
            for (var j = 0; j < this.options.alts.length; j++) {
                var altID = this.options.alts[j].id;
                var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID) / xMaxCoef;
                if (aMaxVal < maxVal) { aMaxVal = maxVal };
            };
            if (aMaxVal > 0) { return aMaxVal };
            return 1;
        },

        _getInteractiveMapElement: function (x, y) {
            for (var i = this.options.interactiveMap.length - 1; i >= 0; i--) {
                var intElement = this.options.interactiveMap[i];
                var xe = intElement.rect[0];
                var ye = intElement.rect[1];
                var we = intElement.rect[2];
                var he = intElement.rect[3];
                if ((x > xe) && (y > ye) && (x < xe + we) && (y < ye + he)) {
                    return intElement;
                }
            };
            return '';
        },

        _getInteractiveMapRect: function (key) {
            for (var i = 0; i < this.options.interactiveMap.length; i++) {
                var intElement = this.options.interactiveMap[i];
                if (intElement.key == key) {
                    return intElement.rect;
                }
            };
            return [0, 0, 2, 2];
        },

        _drawDSABar: function (index, barX, barY, barWidth, barName, barValue, barInitValue, barColor, isSelected, barMaxValue, barMinValue, valueNormCoef) {
            var ctx = this.options.context;
            ctx.save();
            ctx.beginPath();
            ctx.rect(barX, barY, barWidth + 1, this.options.DSABarHeight - 16);
            ctx.lineWidth = 2;
            ctx.strokeStyle = '#eee';
            ctx.fillStyle = 'rgba(255,255,255,0.9)';
            if (isSelected) {
                ctx.strokeStyle = '#bbb';
                ctx.fillStyle = 'rgba(220,220,220,0.7)';
            };
            ctx.fill();
            ctx.stroke();
            ctx.restore();

            var normCoef = (barMaxValue - barMinValue);
            ctx.save();
            ctx.beginPath();
            ctx.rect(barX + 1, barY + 1, (barWidth - 1) * ((barValue - barMinValue) / normCoef), this.options.DSABarHeight - 18);
            ctx.fillStyle = barColor;
            ctx.fill();
            ctx.beginPath();
            ctx.font = this.options.DSATitleFont;
            ctx.font = getFont(ctx, barName, this.options.DSAFontSize, 10, barWidth - 100, this.options.DSAFontFace);
            ctx.fillStyle = 'rgba(255,255,255,0.5)';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';
            var w = ctx.measureText(barName).width + 3;
            ctx.shadowBlur = 8;
            ctx.shadowColor = 'rgba(255,255,255,0.8)';
            ctx.rect(barX + 3, barY + 4, w, this.options.DSABarHeight - 24);
            ctx.fill();
            ctx.fillStyle = 'black';
            ctx.beginPath();
            ctx.fillText(barName, barX + 4, barY + 4);
            //rectText(context, barName, DSATitleFont, barX, titleY + 2, canvasHalfWidth - valueMaxWidth, 22, getTextHeight(DSATitleFont), 3, false);
            ctx.textAlign = 'right';
            ctx.font = this.options.DSATitleFont;
            ctx.textBaseline = 'top';

            ctx.beginPath();
            var barNormValue = barValue / valueNormCoef;
            var sbarValue = (barNormValue * 100).toFixed(this.options.valDigits) + ' %';
            w = ctx.measureText(sbarValue).width + 3;
            ctx.fillStyle = 'rgba(255,255,255,0.5)';
            ctx.rect(barX + barWidth - 4 - w, barY + 4, w, this.options.DSABarHeight - 24);
            ctx.fill();
            ctx.fillStyle = 'black';
            ctx.beginPath();
            ctx.fillText(sbarValue, barX + barWidth - 4, barY + 4);
            ctx.restore();

            var x = barX + (barWidth - 1) * ((barInitValue - barMinValue) / normCoef) + 1;
            ctx.fillStyle = this.options.DSAInitBarColor;
            this._drawArrow(ctx, x, barY, 'down');
        },

        _drawPSABar: function (index, barName, barValue, barInitValue, barColor, isSelected, scrollShift, expandWidth, intMap) {
            var PSAHalfBarHeight = this.options.PSABarHeight / 2;
            var barX = index * expandWidth - scrollShift;
            if (!this.options.isMobile) { barX += PSAHalfBarHeight } else { barX += 4 };
            if (barX < this.options.canvasHalfWidth - PSAHalfBarHeight) {
                var barY = 10;
                var titleY = barX - 6;
                var barWidth = this.options.canvas.height - 30;
                var ctx = this.options.context;
                ctx.save();
                iex = barX;
                iey = barY + 1;
                iew = PSAHalfBarHeight - 4;
                ieh = barWidth + 1;
                intEl = { key: 'obj' + index, rect: [iex, iey, iew, ieh], hint: '' };
                intMap.push(intEl);
                ctx.beginPath();
                ctx.rect(iex, iey, iew, ieh);
                ctx.lineWidth = 2;
                ctx.strokeStyle = '#eee';
                ctx.fillStyle = 'rgba(255,255,255,0.7)';
                if (isSelected) {
                    ctx.strokeStyle = '#bbb';
                    ctx.fillStyle = 'rgba(220,220,220,0.7)';
                };
                ctx.fill();
                ctx.stroke();
                ctx.restore();

                ctx.beginPath();
                ctx.rect(barX + 1, barY + barWidth + 1, PSAHalfBarHeight - 6, -(barWidth - 1) * barValue);
                ctx.fillStyle = barColor;
                ctx.fill();

                ctx.fillStyle = 'black';
                ctx.save();
                ctx.rotate(-Math.PI / 2);
                ctx.beginPath();
                ctx.font = this.options.DSATitleFont;
                //ctx.font = getFont(ctx, barName, this.options.DSAFontSize, 12, barWidth - 60, this.options.DSAFontFace);
                ctx.textAlign = 'left';
                ctx.textBaseline = 'top';

                var w = ctx.measureText(barName).width + 3;
                ctx.shadowBlur = 8;
                ctx.shadowColor = 'rgba(255,255,255,0.8)';
                ctx.fillStyle = 'rgba(255,255,255,0.5)';
                ctx.rect(-barWidth - 6, barX + 4, w + 10, PSAHalfBarHeight - 12);
                ctx.fill();

                ctx.beginPath();
                ctx.shadowBlur = 4;
                ctx.fillStyle = 'black';
                //ctx.shadowColor = "#fff";
                var maxWidth = barWidth - ctx.measureText('9999.00 %').width;
                var truncName = getTruncatedString(ctx, barName, maxWidth);
                ctx.fillText(truncName, -barWidth - 4, barX + 4);
                ctx.restore();

                ctx.save();
                ctx.rotate(-Math.PI / 2);
                ctx.textAlign = 'right';
                //if (!this.options.isMobile) { ctx.font = this.options.DSATitleFont };
                ctx.font = this.options.DSATitleFont;
                ctx.textBaseline = 'top';
                ctx.shadowBlur = 4;
                ctx.shadowColor = "#fff";
                ctx.fillText((barValue * 100).toFixed(this.options.valDigits) + ' %', -16, barX + 4);
                ctx.restore();

                var x = (barWidth - 1) * barInitValue - barY - 1;
                ctx.fillStyle = this.options.DSAInitBarColor;
                this._drawArrow(ctx, PSAHalfBarHeight + barX - 2, barWidth - x, 'left');
            };
        },

        _drawDSAMinMaxMarks: function (index, barX, barWidth, minValue, maxValue, scrollShift, barMaxValue, barMinValue) {
            var barY = index * this.options.DSABarHeight + this.options.DSAHalfBarHeight + 4 - scrollShift;
            var barY2 = barY + this.options.DSABarHeight - 20;
            var ctx = this.options.context;
            var normCoef = (barMaxValue - barMinValue);

            var x = barX + (barWidth - 1) * ((minValue - barMinValue) / normCoef) + 1;
            ctx.fillStyle = 'rgba(0,0,255,0.6)';
            this._drawArrow(ctx, x, barY2, 'up');

            x = barX + (barWidth - 1) * ((maxValue - barMinValue) / normCoef) + 1;
            ctx.fillStyle = 'rgba(255,0,0,0.6)';
            this._drawArrow(ctx, x, barY2, 'up');
        },

        _drawObjectives: function () {
            this.options.context.fillStyle = this.options.backgroundColor;
            this.options.context.fillRect(0, 0, this.options.canvasHalfWidth, this.options.canvas.height);
            for (var i = 0; i < this.options.objs.length; i++) {
                var objColor = this.options.DSAObjBarColor;
                var obj = this.options.objs[i];
                var barY = i * this.options.DSABarHeight + this.options.DSAHalfBarHeight - this.options.ObjScrollShift;
                if (barY > this.options.canvas.height) { break };
                if (barY >= 0) {
                    this._drawDSABar(i, 10, barY, this.options.canvasHalfWidth - 30, obj.name, obj.value, obj.initValue, objColor, this.options.SelectedObjectiveIndex === i, 1, 0, 1);
                };
            };
            if (this.options.canvas.height < this.options.objBarsAreaHeight) {
                this._drawScrollbar(true);
            };
        },

        _getAltNormCoef: function () {
            var normCoef = 0;
            switch (this.options.normalizationMode) {
                case 'normMax100':
                    for (var i = 0; i < this.options.alts.length; i++) {
                        var alt = this.options.alts[i];
                        if (normCoef < alt.value) { normCoef = alt.value }
                    };
                    break;
                case 'normAll':
                    for (var i = 0; i < this.options.alts.length; i++) {
                        var alt = this.options.alts[i];
                        normCoef += alt.value;
                    };
                    break;
                default:
                    return 1;
            };

            if (normCoef == 0) { return 1 } else { return normCoef };
        },

        _drawAlternatives: function () {
            this.options.context.fillStyle = this.options.backgroundColor;
            this.options.context.fillRect(this.options.canvasHalfWidth, 0, this.options.canvasHalfWidth, this.options.canvas.height);
            var altNormCoef = this._getAltNormCoef();
            var altsCountFilter = this.options.alts.length;
            if ((this.options.altFilterValue > -1) && (this.options.altFilterValue < this.options.alts.length)) {
                altsCountFilter = this.options.altFilterValue
            };
            var idx = 0;
            for (var i = 0; i < altsCountFilter; i++) {
                var altColor = this.options.DSAAltBarColor;
                var alt = this.options.alts[i];
                if (alt.visible == 1) {
                    if (this.options.isShowCustomColors) { altColor = alt.color };
                    var barY = idx * this.options.DSABarHeight + this.options.DSAHalfBarHeight - this.options.AltScrollShift;
                    if (barY > this.options.canvas.height) { break };
                    if (barY >= 0) {
                        this._drawDSABar(idx, this.options.canvasHalfWidth + 10, barY, this.options.canvasHalfWidth - 30, alt.name, alt.value, alt.initValue, altColor, false, this.options.MaxAltValue, this.options.MinAltValue, altNormCoef);
                        if ((this.options.SelectedObjectiveIndex < this.options.objs.length) && (this.options.SelectedObjectiveIndex >= 0)) {
                            var altID = alt.id;
                            var obj = this.options.objs[this.options.SelectedObjectiveIndex];
                            var minVal = this.getGradientValueByAltID(obj.gradientMinValues, altID);
                            var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                            this._drawDSAMinMaxMarks(idx, this.options.canvasHalfWidth + 10, this.options.canvasHalfWidth - 30, minVal, maxVal, this.options.AltScrollShift, this.options.MaxAltValue, this.options.MinAltValue)
                        };
                    };
                    idx++;
                };
            };

            if (this.options.canvas.height < this.options.altBarsAreaHeight) {
                this._drawScrollbar(false);
            };
        },

        _drawScrollbar: function (isObjectives) {
            var intMap = this.options.interactiveMap;
            var iex, iey, iew, ieh;
            var rightBound = this.options.canvas.width;
            var canvasHeight = this.options.canvas.height;
            var areaHeight = this.options.altBarsAreaHeight;
            var scrollShift = this.options.AltScrollShift;
            var ctx = this.options.context;
            var scrollColor = '#aaa';
            if (isObjectives) {
                rightBound = this.options.canvasHalfWidth;
                areaHeight = this.options.objBarsAreaHeight;
                scrollShift = this.options.ObjScrollShift;
                if (this.options.mouseOverElement.key === 'objscroll') {
                    if (this.options.isMouseDown) {
                        scrollColor = '#555'
                    } else {
                        scrollColor = '#777'
                    };
                };
            } else {
                if (this.options.mouseOverElement.key === 'altscroll') {
                    if (this.options.isMouseDown) {
                        scrollColor = '#555'
                    } else {
                        scrollColor = '#777'
                    };
                };
            };

            ctx.beginPath();
            ctx.fillStyle = '#fff';
            ctx.fillRect(rightBound - 15, 0, 16, canvasHeight);

            var sliderTop = scrollShift * canvasHeight / areaHeight;
            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
            ctx.beginPath();
            ctx.rect(rightBound - 13, 0, 12, canvasHeight);
            ctx.fillStyle = '#eee';
            ctx.fill();

            iex = rightBound - 15;
            iey = 0;
            iew = 16;
            ieh = canvasHeight;
            var elementName = 'altscroll';
            if (isObjectives) { elementName = 'objscroll' };
            intEl = { key: elementName, rect: [iex, iey, iew, ieh], hint: '' };
            intMap.push(intEl);
            iex = rightBound - 13;
            iey = sliderTop + 2;
            iew = 12;
            ieh = sliderHeight - 4;
            ctx.beginPath();
            ctx.rect(iex, iey, iew, ieh);
            ctx.fillStyle = scrollColor;
            ctx.fill();

            this.options.interactiveMap = intMap;
        },

        _drawHorizontalScrollbar: function () {
            var intMap = this.options.interactiveMap;
            var iex, iey, iew, ieh;
            var canvasHeight = this.options.canvas.height;
            var canvasWidth = this.options.canvas.width / 2;
            var scrollShift = this.options.ObjScrollShift;
            var areaWidth = this.options.objs.length * this.options.PSABarHeight + 8;
            if (canvasWidth < areaWidth) {
                var ctx = this.options.context;
                var sliderLeft = scrollShift * canvasWidth / areaWidth;
                var sliderWidth = canvasWidth * canvasWidth / areaWidth;
                iex = 3;
                iey = canvasHeight - 16;
                iew = canvasWidth;
                ieh = 14;
                intEl = { key: 'hscroll', rect: [iex, iey, iew, ieh], hint: '' };
                intMap.push(intEl);
                ctx.beginPath();
                ctx.rect(iex, iey, iew, ieh);
                ctx.fillStyle = '#fff';
                ctx.fill();
                ctx.beginPath();
                iex = sliderLeft + 3;
                iey = canvasHeight - 15;
                iew = sliderWidth;
                ieh = 12;
                intEl = { key: 'hscrollbar', rect: [iex, iey, iew, ieh], hint: '' };
                intMap.push(intEl);
                ctx.rect(sliderLeft + 3, canvasHeight - 15, sliderWidth, 12);
                ctx.fillStyle = '#999';
                if (this.options.mouseOverElement.key === 'hscrollbar') { ctx.fillStyle = '#555'; };
                if (this.options.mouseOverElement.key === 'hscroll') { ctx.fillStyle = '#777'; };
                ctx.fill();
            };
            this.options.interactiveMap = intMap;
        },

        _setOption: function (key, value) {
            this._super(key, value);
            if (key === 'objs') {
                this.options.SelectedObjectiveIndex = 0;
                this.options.SelectedObjectiveIndexY = 0;
                if ((this.options.viewMode == '2D') && (this.options.objs.length > 1)) { this.options.SelectedObjectiveIndex = 1 };
            };
            if (key === 'alts') {
                if ((this.options.DSASorting) || (this.options.GSAShowLegend)) { this.options.alts.sort(SortByValue) };
            };
        },

        _setOptions: function (options) {
            this._super(options);
        },

        redrawSA: function () {
            if ((this.options.objs.length > 0) && (this.options.alts.length > 0)) {
                switch (this.options.viewMode) {
                    case 'DSA':
                        this.options.interactiveMap = [];
                        this._drawObjectives();
                        this._drawAlternatives();
                        break;
                    case 'GSA':
                        this.redrawGSA();
                        break;
                    case 'PSA':
                        this.redrawPSA();
                        break;
                    case '2D':
                        this.redraw2D();
                        break;
                    case 'HTH':
                        this.redrawHTH();
                        break;
                };
            } else { this.showWarning() };
        },

        resetSA: function () {
            this._updateMinMaxAltBarValue();
            for (var i = 0; i < this.options.alts.length; i++) {
                var alt = this.options.alts[i];
                alt.value = alt.initValue;
            };
            for (var i = 0; i < this.options.objs.length; i++) {
                var obj = this.options.objs[i];
                obj.value = obj.initValue;
            };
            for (var i = 0; i < this.options.objs.length; i++) {
                for (var j = 0; j < this.options.alts.length; j++) {
                    var obj = this.options.objs[i];
                    obj.gradientMinValues[j] = obj.gradientInitMinValues[j];
                };
            };
            if (this.options.viewMode === 'DSA') {
                if (!this.options.DSASorting) { this.options.alts.sort(SortByIndex) } else { this.options.alts.sort(SortByValue) };
            };
            this.redrawSA();
        },

        _drawArrow: function (ctx, x, y, orientation) {
            ctx.beginPath();
            switch (orientation) {
                case 'left':
                    ctx.moveTo(x + 8, y - 3);
                    ctx.lineTo(x, y);
                    ctx.lineTo(x + 8, y + 3);
                    break;
                case 'right':
                    ctx.moveTo(x - 8, y - 3);
                    ctx.lineTo(x, y);
                    ctx.lineTo(x - 8, y + 3);
                    break;
                case 'up':
                    ctx.moveTo(x - 3, y + 8);
                    ctx.lineTo(x, y);
                    ctx.lineTo(x + 3, y + 8);
                    break;
                case 'down':
                    ctx.moveTo(x - 3, y - 8);
                    ctx.lineTo(x, y);
                    ctx.lineTo(x + 3, y - 8);
                    break;
            };
            ctx.fill();
        },

        redrawPSA: function () {
            var intMap = [];
            var iex, iey, iew, ieh;
            if (this.options.isMobile) { this.options.PSABarHeight = 52 } else { this.options.PSABarHeight = 64 };
            var ctx = this.options.context;
            ctx.fillStyle = this.options.backgroundColor;
            ctx.fillRect(0, 0, this.options.canvas.width, this.options.canvas.height);
            var barWidth = this.options.canvas.height - 31;
            var minAltVal = this.options.MinAltValue;
            var maxAltVal = this.options.MaxAltValue;
            var normCoef = (maxAltVal - minAltVal);

            var minAltValNorm = this.options.MinAltValueNormalized;
            var maxAltValNorm = this.options.MaxAltValueNormalized;
            var normCoefNorm = (maxAltValNorm - minAltValNorm);

            intEl = { key: 'legend', rect: [this.options.canvasHalfWidth, 0, this.options.canvasHalfWidth, this.options.canvas.height], hint: '' };
            intMap.push(intEl);

            var x, y, w, h;
            var yStep = (barWidth - 1) / 10;
            var yValStep = 0.01;
            var scaleMaxVal = maxAltVal;
            if (this.options.normalizationMode == 'normAll') { scaleMaxVal = maxAltValNorm };
            yStep = (barWidth - 1) * yValStep / scaleMaxVal;
            ctx.lineWidth = 0.5;
            ctx.strokeStyle = 'black';
            ctx.fillStyle = 'black';
            //if (this.options.isMobile) { ctx.font = '12px Arial' } else { ctx.font = '12px Arial' };
            ctx.font = '12px Arial'
            var textWidth = ctx.measureText((100.0).toFixed(this.options.valDigits) + ' %').width + 10;
            var PSAHalfBarHeight = this.options.PSABarHeight / 2;
            x = this.options.canvasHalfWidth;
            var marginTop = 12;
            ctx.textAlign = 'left';
            ctx.textBaseline = 'bottom';
            var prevY = (barWidth - 1) * 2;
            for (var i = 0; i <= 100; i++) {
                y = (barWidth - 1) - yStep * i + marginTop;
                if (y > 12) {
                    if ((prevY - y) > 24) {
                        ctx.beginPath();
                        ctx.moveTo(x, y);
                        ctx.lineTo(x + 5, y);
                        ctx.stroke();
                        ctx.beginPath();
                        ctx.fillText((((yValStep * i) * 100).toFixed(0)) + ' %', x + 10, y + 5);
                        prevY = y;
                    }
                } else { break };
            };
            y = marginTop;
            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 5, y);
            ctx.stroke();
            ctx.beginPath();
            ctx.fillText((scaleMaxVal * 100).toFixed(this.options.valDigits) + ' %', x + 10, y + 5);

            ctx.moveTo(x + 5, 12);
            ctx.lineTo(x + 5, barWidth + 12);
            ctx.stroke();
            var altNormCoef = this._getAltNormCoef();
            var altsCountFilter = this.options.alts.length;
            if ((this.options.altFilterValue > -1) && (this.options.altFilterValue < this.options.alts.length)) {
                altsCountFilter = this.options.altFilterValue
            };
            this.options.alts.sort(SortByValue);
            var isExpand = ((this.options.objs.length * this.options.PSABarHeight) < this.options.canvasHalfWidth);
            var expandWidth = this.options.PSABarHeight;
            this.options.PSAObjExpand = isExpand;
            if (isExpand) { expandWidth = this.options.canvasHalfWidth / this.options.objs.length };
            for (var j = 0; j < altsCountFilter; j++) {
                var alt = this.options.alts[j];
                if (alt.visible == 1) {
                    var obj = this.options.objs[0];
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                    var normChartCoef = 1;
                    if (this.options.normalizationMode == 'normAll') { normChartCoef = (1 / this.options.objGradientSum[0] / maxAltValNorm) };
                    y = (barWidth - 1) * maxVal * normChartCoef - 11;
                    x = -this.options.ObjScrollShift - 2;
                    if (!this.options.isMobile) { x += PSAHalfBarHeight } else { x += 4 };
                    ctx.fillStyle = alt.color;
                    if (this.options.PSAShowLines) {
                        ctx.beginPath();
                        ctx.moveTo(x, barWidth - y);
                        ctx.lineTo(x + PSAHalfBarHeight, barWidth - y);
                        ctx.strokeStyle = alt.color;
                        ctx.lineWidth = 2;
                    } else {
                        this._drawArrow(ctx, x, barWidth - y, 'right');
                    };
                    for (var i = 1; i < this.options.objs.length; i++) {
                        obj = this.options.objs[i];
                        maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                        normChartCoef = 1;
                        if (this.options.normalizationMode == 'normAll') { normChartCoef = (1 / this.options.objGradientSum[i] / maxAltValNorm) };
                        y = (barWidth - 1) * maxVal * normChartCoef - 11;
                        x = i * expandWidth - this.options.ObjScrollShift - 2;
                        if (!this.options.isMobile) { x += PSAHalfBarHeight } else { x += 4 };
                        if (x < this.options.canvasHalfWidth - PSAHalfBarHeight - 2) {
                            if (this.options.PSAShowLines) {
                                ctx.lineTo(x, barWidth - y);
                                ctx.lineTo(x + PSAHalfBarHeight, barWidth - y);
                            } else {
                                this._drawArrow(ctx, x, barWidth - y, 'right');
                            };
                            this.options.PSAAllowScroll = false;
                        } else { this.options.PSAAllowScroll = true };
                    };
                    normChartCoef = 1;
                    if (this.options.normalizationMode == 'normAll') { normChartCoef = (1 / altNormCoef / maxAltValNorm) };
                    y = barWidth - ((barWidth - 1) * alt.value * normChartCoef - 11);
                    x = this.options.canvasHalfWidth + 4;
                    if (this.options.PSAShowLines) {
                        ctx.lineTo(x - 10, y);
                        ctx.lineTo(x, y);
                        ctx.stroke();
                    } else {
                        this._drawArrow(ctx, x, y, 'right');
                    };
                    x = x + textWidth;

                    w = this.options.canvas.width - x - 24;
                    ctx.beginPath();
                    ctx.moveTo(x - 20, y);
                    ctx.lineTo(x - 15, y);
                    if (!this.options.PSALineup) {
                        var yLegendStep = barWidth / (altsCountFilter - 1);
                        if (yLegendStep < 24) {
                            yLegendStep = 24;
                        } else { this.options.AltScrollShift = 0 };
                        y = 12 + j * yLegendStep - this.options.AltScrollShift;
                        ctx.lineTo(x, y);
                        ctx.lineTo(x + 5, y);
                    } else { ctx.lineTo(x, y); };
                    
                    ctx.strokeStyle = alt.color;
                    ctx.lineWidth = 1;
                    ctx.stroke();

                    if ((y < this.options.canvas.height) && (y > 0)) {
                        if (!this.options.PSALineup) {
                            this._drawArrow(ctx, x + 10, y, 'right');
                            this._drawAltLegendLabel(ctx, alt, x + 11, y, w, 22, altNormCoef);
                        } else {
                            this._drawAltLegendLabel(ctx, alt, x - 15, y, w + 35, 22, altNormCoef);
                        };

                    };
                };
            };

            for (var i = 0; i < this.options.objs.length; i++) {
                var objColor = this.options.DSAObjBarColor;
                var obj = this.options.objs[i];
                this._drawPSABar(i, obj.name, obj.value, obj.initValue, objColor, this.options.SelectedObjectiveIndex === i, this.options.ObjScrollShift, expandWidth, intMap);
            };

            this.options.interactiveMap = intMap;
            this._drawHorizontalScrollbar();
            if (!this.options.PSALineup) { this._drawScrollbar(false) };
            
        },

        _drawAltLegendLabel: function (ctx, alt, x, y, w, h, normCoef) {
            var h2 = h / 2;
            ctx.save();
            ctx.font = '12px Arial';
            var altValueTextWidth = ctx.measureText((this.options.MaxAltValue * 100).toFixed(this.options.valDigits) + ' %').width + 10;
            ctx.beginPath();
            ctx.fillStyle = '#fff';
            ctx.rect(x, y - h2, w, h);
            ctx.fill();
            ctx.beginPath();
            ctx.fillStyle = alt.color;
            ctx.rect(x, y - h2, altValueTextWidth, h);
            ctx.fill();

            ctx.beginPath();
            ctx.fillStyle = "#000";
            ctx.textBaseline = 'middle';
            ctx.font = getFont(ctx, alt.name, this.options.DSAFontSize, 11, w - altValueTextWidth, this.options.DSAFontFace);
            var truncName = getTruncatedString(ctx, alt.name, w - altValueTextWidth);
            ctx.textAlign = 'left';
            ctx.fillText(truncName, x + 5 + altValueTextWidth, y);

            ctx.fillStyle = "#fff";
            ctx.textAlign = 'right';
            ctx.shadowBlur = 4;
            ctx.shadowColor = "#000";
            ctx.font = '12px Arial';
            //if (!this.options.isMobile) { ctx.font = this.options.DSATitleFont };
            ctx.fillText((alt.value * 100 / normCoef).toFixed(this.options.valDigits) + ' %', x + altValueTextWidth - 5, y);
            ctx.restore();
        },

        _calcualteNormalizedValues: function () {
            var obj = this.options.objs[this.options.SelectedObjectiveIndex];
            var minValues = [];
            var maxValues = [];
            for (var i = 0; i < this.options.alts.length; i++) {
                var alt = this.options.alts[i];
                minValues.push(this.getGradientValueByAltID(obj.gradientMinValues, alt.id));
                maxValues.push(this.getGradientValueByAltID(obj.gradientMaxValues, alt.id));
            };
            var minSum = 0;
            var maxSum = 0;
            for (var i = 0; i < this.options.alts.length; i++) {
                minSum += minValues[i];
                maxSum += maxValues[i];
            };
            this.options.objMinNormValues = [];
            this.options.objMaxNormValues = [];
            for (var i = 0; i < this.options.alts.length; i++) {
                this.options.objMinNormValues.push(minValues[i] / minSum);
                this.options.objMaxNormValues.push(maxValues[i] / maxSum);
            };
            this.options.minNormAltValue = 1;
            this.options.maxNormAltValue = 0;
            for (var i = 0; i < this.options.alts.length; i++) {
                if (this.options.minNormAltValue > this.options.objMinNormValues[i]) { this.options.minNormAltValue = this.options.objMinNormValues[i] };
                if (this.options.maxNormAltValue < this.options.objMinNormValues[i]) { this.options.maxNormAltValue = this.options.objMinNormValues[i] };
                if (this.options.minNormAltValue > this.options.objMaxNormValues[i]) { this.options.minNormAltValue = this.options.objMaxNormValues[i] };
                if (this.options.maxNormAltValue < this.options.objMaxNormValues[i]) { this.options.maxNormAltValue = this.options.objMaxNormValues[i] };
            };
        },

        redrawGSA: function () {
            var marginLeft = 50;
            var marginTop = 20;
            var marginBottom = 60;
            var btnSize = this.options.buttonSize;
            var intMap = [];
            var iex, iey, iew, ieh, btnMode;

            if (this.options.GSAShowLegend) { this.options.GSAmarginRight = 300 } else { this.options.GSAmarginRight = 40 };
            var marginRight = this.options.GSAmarginRight;

            this.options.context.font = '12px Arial';
            marginLeft = this.options.context.measureText((1000.0).toFixed(this.options.valDigits) + ' %').width + 10;

            var MaxAltValue = this.options.MaxAltValue;
            var MinAltValue = this.options.MinAltValue;
            if (this.options.normalizationMode == 'normAll') {
                this._calcualteNormalizedValues();
                MaxAltValue = this.options.maxNormAltValue;
                MinAltValue = this.options.minNormAltValue;
            };
            var normCoef = (MaxAltValue - MinAltValue);
            var ctx = this.options.context;
            ctx.beginPath();
            ctx.fillStyle = this.options.backgroundColor;
            ctx.fillRect(0, 0, this.options.canvas.width, this.options.canvas.height);

            ctx.beginPath();
            ctx.fillStyle = '#ffffff';
            var chartHeight = this.options.canvas.height - (marginTop + marginBottom);
            var chartWidth = this.options.canvas.width - (marginLeft + marginRight);
            ctx.fillRect(marginLeft, marginTop, chartWidth, chartHeight);
            var xTicks = 10;
            var yTicks = 10;
            if (chartWidth < 300) { xTicks = 5 };
            if (chartHeight < 200) { yTicks = 5 };
            var yStep = chartHeight / yTicks;
            var xStep = chartWidth / xTicks;
            var yValStep = normCoef / yTicks;
            var xValStep = 1 / xTicks;
            ctx.lineWidth = 0.5;
            ctx.strokeStyle = 'rgba(0,0,0,0.5)';
            ctx.fillStyle = 'black';

            intEl = { key: 'objX', rect: [marginLeft, marginTop, chartWidth, chartHeight], hint: '' };
            intMap.push(intEl);
            intEl = { key: 'legend', rect: [this.options.canvas.width - marginRight, 0, marginRight, this.options.canvas.height], hint: '' };
            intMap.push(intEl);

            for (var i = 0; i <= xTicks; i++) {
                var x = xStep * i + marginLeft;
                ctx.beginPath();
                ctx.moveTo(x, marginTop);
                ctx.lineTo(x, this.options.canvas.height - marginBottom + 5);
                ctx.stroke();
                ctx.beginPath();
                ctx.textAlign = 'center';
                ctx.textBaseline = 'top';
                ctx.fillText((i * xValStep * 100).toFixed(0) + ' %', x - 10, this.options.canvas.height - marginBottom + 10);
            };
            for (var i = 0; i <= yTicks; i++) {
                var y = yStep * i + marginTop;
                ctx.beginPath();
                ctx.moveTo(marginLeft - 5, y);
                ctx.lineTo(this.options.canvas.width - marginRight, y);
                ctx.stroke();
                ctx.beginPath();
                ctx.textAlign = 'right';
                ctx.textBaseline = 'bottom';
                ctx.fillText((((MaxAltValue - yValStep * i) * 100).toFixed(this.options.valDigits)) + ' %', marginLeft - 10, y + 5);
            };

            var objValue = this.options.objs[this.options.SelectedObjectiveIndex].value;
            var selectedObjName = this.options.objs[this.options.SelectedObjectiveIndex].name + ' = ' + (objValue * 100).toFixed(this.options.valDigits) + ' %';
            ctx.beginPath();
            ctx.textAlign = 'center';
            ctx.font = getFont(ctx, selectedObjName, this.options.DSAFontSize, 10, this.options.canvas.width - 74, this.options.DSAFontFace);
            ctx.fillStyle = 'blue';
            ctx.textBaseline = 'top';
            ctx.fillText(selectedObjName, this.options.canvasHalfWidth, this.options.canvas.height - 34);
            var xcenter = this.options.canvasHalfWidth - (this.options.objs.length * 10 / 2);
            for (var i = 0; i < this.options.objs.length; i++) {
                ctx.beginPath();
                ctx.arc(i * 10 + xcenter, this.options.canvas.height - 8, 3, 0, 2 * Math.PI, false);
                if (i === this.options.SelectedObjectiveIndex) { ctx.fillStyle = 'blue' } else { ctx.fillStyle = '#eee' };
                ctx.fill();
            };
            var xbtm = this.options.canvas.height;
            var xw = this.options.canvas.width;
            if (this.options.SelectedObjectiveIndex < this.options.objs.length - 1) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'next') {
                    btnMode = 'over';
                };
                iex = this.options.canvas.width - 30 - marginRight;
                iey = xbtm - btnSize - 5;
                intEl = { key: 'next', rect: [iex, iey, btnSize, btnSize], hint: 'Next Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 0);
            };

            if (this.options.SelectedObjectiveIndex !== 0) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'prev') {
                    btnMode = 'over';
                };
                iex = this.options.canvas.width - 65 - marginRight;
                iey = xbtm - btnSize - 5;
                intEl = { key: 'prev', rect: [iex, iey, btnSize, btnSize], hint: 'Previous Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 180);
            };

            var x1 = marginLeft;
            var x2 = marginLeft + chartWidth;
            var altsCountFilter = this.options.alts.length;
            if ((this.options.altFilterValue > -1) && (this.options.altFilterValue < this.options.alts.length)) {
                altsCountFilter = this.options.altFilterValue
            };
            var altNormCoef = this._getAltNormCoef();
            //if (this.options.normalizationMode = 'normAll') {normCoef = altNormCoef}

            var obj = this.options.objs[this.options.SelectedObjectiveIndex];

            for (var i = 0; i < altsCountFilter; i++) {
                var alt = this.options.alts[i];
                if (alt.visible == 1) {
                    var gradientMinValue = this.getGradientValueByAltID(obj.gradientMinValues, alt.id);
                    var gradientMaxValue = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                    if (this.options.normalizationMode == 'normAll') {
                        gradientMinValue = this.options.objMinNormValues[i];
                        gradientMaxValue = this.options.objMaxNormValues[i];
                    };
                    ctx.beginPath();
                    ctx.lineWidth = 2;
                    var y1 = marginTop + chartHeight - ((gradientMinValue - MinAltValue) / normCoef * chartHeight);
                    var y2 = marginTop + chartHeight - ((gradientMaxValue - MinAltValue) / normCoef * chartHeight);
                    ctx.moveTo(x1, y1);
                    ctx.lineTo(x2, y2);
                    ctx.strokeStyle = alt.color;
                    ctx.stroke();
                };
            };

            var objInitValue = this.options.objs[this.options.SelectedObjectiveIndex].initValue;
            var xInitVal = marginLeft + objInitValue * chartWidth;
            ctx.beginPath();
            ctx.lineWidth = 3;
            ctx.strokeStyle = 'grey';
            ctx.moveTo(xInitVal, marginTop);
            ctx.lineTo(xInitVal, marginTop + chartHeight);
            ctx.stroke();

            var xVal = marginLeft + objValue * chartWidth;
            ctx.beginPath();
            ctx.lineWidth = 3;
            ctx.strokeStyle = 'blue';
            ctx.moveTo(xVal, marginTop);
            ctx.lineTo(xVal, marginTop + chartHeight);
            ctx.stroke();

            if (this.options.GSAShowLegend) {
                var idx = 0;
                for (var i = 0; i < altsCountFilter; i++) {
                    var alt = this.options.alts[i];
                    if (alt.visible == 1) {
                        var yTop = idx * 24 + 22 - this.options.AltScrollShift;
                        if (yTop > 0) { this._drawAltLegendLabel(ctx, alt, this.options.canvas.width - marginRight + 10, yTop, 265, 22, altNormCoef); }
                        if ((yTop) > (this.options.canvas.height - marginTop)) { break };
                        idx++;
                    };
                }
                this._drawScrollbar(false);
            };
            this.options.interactiveMap = intMap;
        },

        _get2DValue: function (objID, altID) {
            for (var j = 0; j < this.options.objs[objID].values2D.length; j++) {
                var altItem = this.options.objs[objID].values2D[j];
                if (altItem.altID == altID) {
                    return altItem.val;
                };
            };
            return 0;
        },

        redraw2D: function () {
            var intMap = [];
            var iex, iey, iew, ieh, btnMode;
            var marginLeft = 50;
            var marginTop = 20;
            var marginBottom = 70;
            var objXColor = '#aaaaff';
            var btnSize = this.options.buttonSize;
            var marginRight = this.options.GSAmarginRight;

            if (this.options.GSAShowLegend) { this.options.GSAmarginRight = 240 } else { this.options.GSAmarginRight = 40 };
            this.options.context.font = '12px Arial';
            marginLeft = this.options.context.measureText((1000.0).toFixed(this.options.valDigits) + ' %').width + 50;

            var normCoefY = (this.options.MaxAltValue - this.options.MinAltValue);
            var ctx = this.options.context;
            ctx.beginPath();
            ctx.fillStyle = this.options.backgroundColor;
            ctx.fillRect(0, 0, this.options.canvas.width, this.options.canvas.height);

            ctx.beginPath();
            ctx.fillStyle = '#ffffff';
            var chartHeight = this.options.canvas.height - (marginTop + marginBottom);
            var chartWidth = this.options.canvas.width - (marginLeft + marginRight);
            ctx.fillRect(marginLeft, marginTop, chartWidth, chartHeight);
            var xTicks = 10;
            var yTicks = 10;
            if (chartWidth < 300) { xTicks = 5 };
            if (chartHeight < 200) { yTicks = 5 };
            var yStep = chartHeight / yTicks;
            var xStep = chartWidth / xTicks;
            var yValStep = normCoefY / yTicks;
            var xValStep = normCoefY / xTicks;
            ctx.lineWidth = 0.5;
            ctx.strokeStyle = 'rgba(0,0,0,0.5)';
            ctx.fillStyle = 'black';

            for (var i = 0; i <= xTicks; i++) {
                var x = xStep * i + marginLeft;
                ctx.beginPath();
                ctx.moveTo(x, marginTop);
                ctx.lineTo(x, this.options.canvas.height - marginBottom + 5);
                ctx.stroke();
                ctx.beginPath();
                ctx.textAlign = 'center';
                ctx.textBaseline = 'top';
                ctx.fillText(((-this.options.MinAltValue + xValStep * i) * 100).toFixed(this.options.valDigits) + ' %', x + 5, this.options.canvas.height - marginBottom + 10);
            };
            for (var i = 0; i <= yTicks; i++) {
                var y = yStep * i + marginTop;
                ctx.beginPath();
                ctx.moveTo(marginLeft - 5, y);
                ctx.lineTo(this.options.canvas.width - marginRight, y);
                ctx.stroke();
                ctx.beginPath();
                ctx.textAlign = 'right';
                ctx.textBaseline = 'bottom';
                ctx.fillText((((this.options.MaxAltValue - yValStep * i) * 100).toFixed(this.options.valDigits)) + ' %', marginLeft - 10, y + 5);
            };

            var objValue = this.options.objs[this.options.SelectedObjectiveIndex].value;
            var objYValue = this.options.objs[this.options.SelectedObjectiveIndexY].value;
            var selectedObjName = this.options.objs[this.options.SelectedObjectiveIndex].name + ' = ' + (objValue * 100).toFixed(this.options.valDigits) + ' %';
            var selectedObjYName = this.options.objs[this.options.SelectedObjectiveIndexY].name + ' = ' + (objYValue * 100).toFixed(this.options.valDigits) + ' %';
            ctx.beginPath();
            ctx.textAlign = 'center';
            ctx.font = getFont(ctx, selectedObjName, this.options.DSAFontSize, 10, this.options.canvas.width - 74, this.options.DSAFontFace);
            ctx.fillStyle = 'blue';
            ctx.textBaseline = 'top';
            ctx.fillText(selectedObjName, this.options.canvasHalfWidth, this.options.canvas.height - 34);
            //iex = marginLeft;
            //iey = this.options.canvas.height - this.options.DSABarHeight - 5;
            //iew = chartWidth;
            //ieh = this.options.DSABarHeight;
            //intEl = { key: 'objX', rect: [iex, iey, iew, ieh] };
            //intMap.push(intEl);
            //this._drawDSABar(this.options.SelectedObjectiveIndex, iex, iey, iew, this.options.objs[this.options.SelectedObjectiveIndex].name, objValue, this.options.objs[this.options.SelectedObjectiveIndex].initValue, objXColor, false, 1, 0, 1);

            ctx.save();
            ctx.rotate(-Math.PI / 2);
            ctx.beginPath();
            ctx.fillText(selectedObjYName, -this.options.canvas.height / 2, 10);
            ctx.restore();

            var xcenter = this.options.canvasHalfWidth - (this.options.objs.length * 10 / 2);
            for (var i = 0; i < this.options.objs.length; i++) {
                ctx.beginPath();
                ctx.arc(i * 10 + xcenter, this.options.canvas.height - 10, 4, 0, 2 * Math.PI, false);
                if (i === this.options.SelectedObjectiveIndex) { ctx.fillStyle = 'blue' } else { ctx.fillStyle = '#eee' };
                ctx.fill();
            };

            var ycenter = this.options.canvas.height / 2 + (this.options.objs.length * 10 / 2);
            for (var i = 0; i < this.options.objs.length; i++) {
                ctx.beginPath();
                ctx.arc(40, ycenter - i * 10, 4, 0, 2 * Math.PI, false);
                if (i === this.options.SelectedObjectiveIndexY) { ctx.fillStyle = 'blue' } else { ctx.fillStyle = '#eee' };
                ctx.fill();
            };

            var xbtm = this.options.canvas.height;
            var xw = this.options.canvas.width;
            iey = xbtm - btnSize - 5;
            if (this.options.SelectedObjectiveIndex < this.options.objs.length - 1) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'next') {
                    btnMode = 'over';
                };
                iex = xw - btnSize - 5;
                intEl = { key: 'next', rect: [iex, iey, btnSize, btnSize], hint: 'Next X Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 0);
            };

            if (this.options.SelectedObjectiveIndex !== 0) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'prev') {
                    btnMode = 'over';
                };
                iex = xw - btnSize * 2 - 10;
                intEl = { key: 'prev', rect: [iex, iey, btnSize, btnSize], hint: 'Previous X Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 180);
            };

            iex = 5;
            if (this.options.SelectedObjectiveIndexY < this.options.objs.length - 1) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'nextY') {
                    btnMode = 'over';
                };
                iey = 5;
                intEl = { key: 'nextY', rect: [iex, iey, btnSize, btnSize], hint: 'Next Y Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, -90);
            };

            if (this.options.SelectedObjectiveIndexY !== 0) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'prevY') {
                    btnMode = 'over';
                };
                iey = btnSize + 10;
                intEl = { key: 'prevY', rect: [iex, iey, btnSize, btnSize], hint: 'Previous Y Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 90);
            };

            ctx.textBaseline = 'middle';
            var altsCountFilter = this.options.alts.length;
            if ((this.options.altFilterValue > -1) && (this.options.altFilterValue < this.options.alts.length)) {
                altsCountFilter = this.options.altFilterValue
            };
            for (var i = 0; i < altsCountFilter; i++) {
                var alt = this.options.alts[i];
                if (alt.visible == 1) {
                    var xVal = this._get2DValue(this.options.SelectedObjectiveIndex, alt.id);
                    var yVal = this._get2DValue(this.options.SelectedObjectiveIndexY, alt.id);
                    var ValString = alt.name + ' X: ' + (xVal * 100).toFixed(this.options.valDigits) + ' % Y: ' + (yVal * 100).toFixed(this.options.valDigits) + ' %';
                    ctx.beginPath();
                    var x1 = marginLeft + ((xVal - this.options.MinAltValue) / normCoefY * chartWidth);
                    var y1 = marginTop + chartHeight - ((yVal - this.options.MinAltValue) / normCoefY * chartHeight);
                    var radius = 10;
                    iex = x1 - radius;
                    iey = y1 - radius;
                    iew = radius * 2;
                    var ieKey = 'alt:' + i;
                    intEl = { key: ieKey, rect: [iex, iey, iew, iew], hint: ValString };
                    intMap.push(intEl);
                    ctx.arc(x1, y1, 10, 0, 2 * Math.PI, false);
                    ctx.fillStyle = alt.color;
                    ctx.fill();
                };
            };

            for (var i = 0; i < altsCountFilter; i++) {
                var ieKey = 'alt:' + i;
                ctx.beginPath();
                var ie = this.options.mouseOverElement;
                if (ie.key == ieKey) {
                    ctx.save();
                    ctx.shadowBlur = 4;
                    ctx.shadowColor = "#fff";
                    var iex = ie.rect[0] + 10;
                    var iey = ie.rect[1] + ie.rect[3] / 2;
                    var iew = ie.rect[2];
                    var ieh = ie.rect[3];
                    var tooltipWidth = ctx.measureText(ie.hint).width;
                    if ((tooltipWidth + iex + iew) > (marginLeft + chartWidth)) {
                        ctx.textAlign = 'right';
                        ctx.fillStyle = '#ffffdd';
                        ctx.fillRect(iex - iew - tooltipWidth, iey - 10, tooltipWidth, 20);
                        ctx.fillStyle = '#000';
                        ctx.fillText(ie.hint, iex - 20, iey);
                    } else {
                        ctx.fillStyle = '#ffffdd';
                        ctx.fillRect(iex + 20, iey - 10, tooltipWidth, 20);
                        ctx.textAlign = 'left';
                        ctx.fillStyle = '#000';
                        ctx.fillText(ie.hint, iex + 20, iey);
                    };
                    ctx.restore();
                };
            };

            this.options.interactiveMap = intMap;
        },

        redrawHTH: function () {
            var intMap = [];
            var iex, iey, iew, ieh, btnMode;
            var btnSize = this.options.buttonSize;

            var ctx = this.options.context;
            ctx.beginPath();
            ctx.fillStyle = this.options.backgroundColor;
            ctx.fillRect(0, 0, this.options.canvas.width, this.options.canvas.height);

            ctx.font = this.options.DSATitleFont;
            var alt1 = this.options.alts[this.options.HTHAlt1Index];
            var alt2 = this.options.alts[this.options.HTHAlt2Index];
            ctx.beginPath();
            ctx.fillStyle = '#000';
            ctx.textBaseline = 'middle';
            ctx.textAlign = 'center';
            ctx.fillText('< >', this.options.canvasHalfWidth, btnSize + 5);
            ctx.textAlign = 'right';
            ctx.fillText(alt1.name, this.options.canvasHalfWidth - btnSize * 2, btnSize + 5);
            ctx.textAlign = 'left';
            ctx.fillText(alt2.name, this.options.canvasHalfWidth + btnSize * 2, btnSize + 5);

            if (this.options.HTHAlt1Index < this.options.alts.length - 1) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'nextAlt1') {
                    btnMode = 'over';
                };
                iex = 5;
                iey = 0;
                intEl = { key: 'nextAlt1', rect: [iex, iey, btnSize, btnSize], hint: 'Next Alternative' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, -90);
            };
            if (this.options.HTHAlt1Index > 0) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'prevAlt1') {
                    btnMode = 'over';
                };
                iex = 5;
                iey = btnSize;
                intEl = { key: 'prevAlt1', rect: [iex, iey, btnSize, btnSize], hint: 'Previous Alternative' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 90);
            };
            if (this.options.HTHAlt2Index < this.options.alts.length - 1) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'nextAlt2') {
                    btnMode = 'over';
                };
                iex = this.options.canvas.width - btnSize - 5;
                iey = 0;
                intEl = { key: 'nextAlt2', rect: [iex, iey, btnSize, btnSize], hint: 'Next Alternative' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, -90);
            };
            if (this.options.HTHAlt2Index > 0) {
                btnMode = 'normal';
                if (this.options.mouseOverElement.key === 'prevAlt2') {
                    btnMode = 'over';
                };
                iex = this.options.canvas.width - btnSize - 5; ;
                iey = btnSize;
                intEl = { key: 'prevAlt2', rect: [iex, iey, btnSize, btnSize], hint: 'Previous Alternative' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 90);
            };

            ctx.beginPath();
            ctx.arc(this.options.canvasHalfWidth - btnSize - 5, btnSize + 5, btnSize / 2, 0, 2 * Math.PI, false);
            ctx.fillStyle = alt1.color;
            ctx.fill();

            ctx.beginPath();
            ctx.arc(this.options.canvasHalfWidth + btnSize + 5, btnSize + 5, btnSize / 2, 0, 2 * Math.PI, false);
            ctx.fillStyle = alt2.color;
            ctx.fill();
            var overallVal = 0;
            for (var i = 0; i < this.options.objs.length; i++) {
                var obj = this.options.objs[i];
                var alt1Val = this._get2DValue(i, alt1.id);
                var alt2Val = this._get2DValue(i, alt2.id);
                var altDiffVal = alt1Val - alt2Val;
                var altBarWidth = -altDiffVal / this.options.MaxAltValue * (this.options.canvasHalfWidth - 15);
                ctx.fillStyle = '#eee';
                ctx.beginPath();
                ctx.fillRect(5, btnSize * 2 + i * this.options.DSABarHeight, this.options.canvas.width - 9, this.options.DSABarHeight - 8);
                if (altDiffVal < 0) {
                    ctx.fillStyle = alt2.color;
                } else { ctx.fillStyle = alt1.color; };

                ctx.fillRect(this.options.canvasHalfWidth, btnSize * 2 + i * this.options.DSABarHeight, altBarWidth, this.options.DSABarHeight - 10);
                overallVal += altDiffVal;

                ctx.fillStyle = '#000';
                ctx.beginPath();
                ctx.textAlign = 'center';
                ctx.fillText(obj.name, this.options.canvasHalfWidth, btnSize * 2.5 + i * this.options.DSABarHeight);

                if (altDiffVal > 0) {
                    ctx.textAlign = 'left';
                    ctx.fillText((altDiffVal * 100).toFixed(this.options.valDigits) + '%', 10, btnSize * 2.5 + i * this.options.DSABarHeight);
                } else {
                    ctx.textAlign = 'right';
                    ctx.fillText((Math.abs(altDiffVal) * 100).toFixed(this.options.valDigits) + '%', this.options.canvas.width - 10, btnSize * 2.5 + i * this.options.DSABarHeight);
                };

            };
            ctx.textAlign = 'center';
            ctx.fillStyle = '#eee';
            ctx.beginPath();
            var altBarWidth = -overallVal / this.options.MaxAltValue * (this.options.canvasHalfWidth - 15);
            ctx.fillRect(5, btnSize * 2 + this.options.objs.length * this.options.DSABarHeight, this.options.canvas.width - 10, this.options.DSABarHeight - 8);
            var overallTop = btnSize * 2.5 + this.options.objs.length * this.options.DSABarHeight;
            var overallBarTop = btnSize * 2 + this.options.objs.length * this.options.DSABarHeight + 1;
            if (overallVal < 0) {
                ctx.fillStyle = alt2.color;
            } else { ctx.fillStyle = alt1.color; };
            ctx.fillRect(this.options.canvasHalfWidth, overallBarTop, altBarWidth, this.options.DSABarHeight - 10);
            ctx.fillStyle = '#000';
            ctx.beginPath();

            ctx.fillText('OVERALL', this.options.canvasHalfWidth, overallTop);

            if (overallVal > 0) {
                ctx.textAlign = 'left';
                ctx.fillText((overallVal * 100).toFixed(this.options.valDigits) + '%', 10, overallTop);
            } else {
                ctx.textAlign = 'right';
                ctx.fillText((Math.abs(overallVal) * 100).toFixed(this.options.valDigits) + '%', this.options.canvas.width - 10, overallTop);
            };

            this.options.interactiveMap = intMap;
        },
        showWarning: function () {
            var ctx = this.options.context;
            ctx.beginPath();
            ctx.fillStyle = this.options.backgroundColor;
            ctx.fillRect(0, 0, this.options.canvas.width, this.options.canvas.height);

            ctx.font = this.options.DSATitleFont;
            ctx.beginPath();
            ctx.fillStyle = '#000';
            ctx.textBaseline = 'middle';
            ctx.textAlign = 'center';
            var warningMessage = 'Not enough data to show sensitivity results.';
            var y = this.options.canvas.height / 2 - 20;
            ctx.fillText(warningMessage, this.options.canvasHalfWidth, y);

            if (this.options.objs.length == 0) {
                y += 20;
                warningMessage = 'No objectives to show in this hierarchy.';
                ctx.fillText(warningMessage, this.options.canvasHalfWidth, y);
            };
            if (this.options.alts.length == 0) {
                y += 20;
                warningMessage = 'No alternatives to show in this project.';
                ctx.fillText(warningMessage, this.options.canvasHalfWidth, y);
            };
        },

        GradientMinValues: function (value) {
            for (var i = 0; i < value.length; i++) {
                var objItem = value[i];
                var objID = objItem[0];
                var obj = this.getObjByID(objID);
                obj.gradientMinValues = objItem[1];
            };
            this._updateMinMaxAltBarValue();
            this.redrawSA();
            this.options.IsObjValuesChanged = false;
        },

        getGradientValueByAltID: function (gradientValues, altID) {
            return $.grep(gradientValues, function (e) { return e.altID === altID; })[0].val;
        },

        GetMouseOverObjectiveIndex: function (x, y) {
            var retVal = -1
            if (this.options.viewMode === 'DSA') {
                if (x < this.options.canvasHalfWidth - 5) {
                    retVal = Math.round((y + 5 + this.options.ObjScrollShift) / this.options.DSABarHeight) - 1
                } else {
                    retVal = this.options.SelectedObjectiveIndex
                };
                if (retVal < 0) { retVal = 0 };
            };
            if (this.options.viewMode === 'PSA') {
                //retVal = Math.round((x + 20 + this.options.ObjScrollShift) / this.options.PSABarHeight) - 1
                if ((this.options.mouseOverElement != '') && (this.options.mouseOverElement != {})) {
                    retVal = Number(this.options.mouseOverElement.key.replace('obj', ''));
                };
            };
            if (retVal >= this.options.objs.length) { retVal = this.options.objs.length - 1 };
            return retVal;
        },

        setSelectedObjectiveIndex: function (ObjIndex) {
            this.options.SelectedObjectiveIndex = ObjIndex;
        },

        syncSACharts: function (isValueSync, ObjID, NewObjValue) {
            var dsaIDs = [];
            var className = this.options.canvas.className;
            $('canvas.' + className).each(function () { dsaIDs.push(this.getAttribute("id")) });
            for (var i = 0; i < dsaIDs.length; i++) {
                if (dsaIDs[i] != this.element[0].id) {
                    if (isValueSync) {
                        $('#' + dsaIDs[i]).sa("setObjLocalPriority", ObjID, NewObjValue);
                        $('#' + dsaIDs[i]).sa("updateAltPriorities", ObjID);
                    };
                    $('#' + dsaIDs[i]).sa("setSelectedObjectiveIndex", this.options.SelectedObjectiveIndex);
                    $('#' + dsaIDs[i]).sa("redrawSA");
                };
            };
        },

        updateContent: function (x, y) {
            if (this.options.isMouseDown) {
                if ((this.options.SelectedObjectiveIndex < this.options.objs.length) && (this.options.SelectedObjectiveIndex >= 0)) {
                    var NewObjValue = 1;
                    if (this.options.viewMode === 'DSA') {
                        NewObjValue = ((x - 10) / (this.options.canvasHalfWidth - 30));
                    };
                    if (this.options.viewMode === 'GSA') {
                        var objXRect = this._getInteractiveMapRect('objX');
                        NewObjValue = ((x - objXRect[0]) / (objXRect[2]));
                    };
                    if (this.options.viewMode === '2D') {

                    };
                    if (this.options.viewMode === 'PSA') {
                        NewObjValue = ((this.options.canvas.height - 20 - y) / (this.options.canvas.height - 40));
                    };
                    var aObj = this.options.objs[this.options.SelectedObjectiveIndex];
                    if (NewObjValue != aObj.value) { this.options.IsObjValuesChanged = true };
                    this.setObjLocalPriority(aObj.id, NewObjValue);
                    this.updateAltPriorities(aObj.id);
                    if (this.options.isMultiView) this.syncSACharts(true, aObj.id, NewObjValue);
                };
                this.redrawSA();
            };
            this.options.isUpdateFinished = true;
        },

        resizeCanvas: function (width, height) {
            this.options.canvas.width = width;
            this.options.canvas.height = height;
            if (width < 400) { this.options.isMobile = true } else { this.options.isMobile = false };
            this.options.context = this.options.canvas.getContext('2d');
            this.options.canvasRect = this.options.canvas.getBoundingClientRect();
            this.options.canvasHalfWidth = this.options.canvas.width / 2;
            this.redrawSA();
        },

        _dsaUpdateCurrentValues: function () {
            var values = "";
            for (var i = 0; i < this.options.objs.length; i++) {
                values += (values == "" ? "" : ",") + this.options.objs[i].value;
            };
            var objids = "";
            for (var i = 0; i < this.options.objs.length; i++) {
                objids += (objids == "" ? "" : ",") + this.options.objs[i].id;
            };
            sendCommand("action=" + ACTION_DSA_UPDATE_VALUES + "&values=" + values + "&objIds=" + objids + "&id=" + this.options.canvas.id, false);
        },

        getObjByID: function (ObjID) {
            return $.grep(this.options.objs, function (e) { return e.id === ObjID; })[0];
        },

        getAltByID: function (AltID) {
            return $.grep(this.options.alts, function (e) { return e.id === AltID; })[0];
        },

        setObjLocalPriority: function (ObjID, ObjValue) {
            if (ObjValue < 0) { ObjValue = 0 };
            if (ObjValue > 1) { ObjValue = 1 };
            var obj = this.getObjByID(ObjID);
            obj.value = ObjValue;
            total = 0;
            coef = 1;
            for (var i = 0; i < this.options.objs.length; i++) {
                var aObj = this.options.objs[i];
                if (aObj.id != ObjID) {
                    total += aObj.value;
                };
            };
            if (total <= 0) {
                coef = 0;
            } else {
                coef = (1 - ObjValue) / total;
            };
            for (var i = 0; i < this.options.objs.length; i++) {
                var aObj = this.options.objs[i];
                if (aObj.id != ObjID) {
                    aObj.value *= coef;
                    if (aObj.value <= 0.000001) {
                        aObj.value = 0.00001 * aObj.initValue;
                    };
                };
            };
        },

        updateAltPriorities: function (ObjID) {
            for (var i = 0; i < this.options.alts.length; i++) {
                var alt = this.options.alts[i];
                var obj = this.getObjByID(ObjID);
                var aMinValue = this.getGradientValueByAltID(obj.gradientMinValues, alt.id);
                var aMaxValue = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id); ;
                alt.value = aMinValue + (aMaxValue - aMinValue) * obj.value;
            };
            if (this.options.viewMode === 'DSA') { this.sortAlternatives() };
            if ((this.options.viewMode === 'GSA') && this.options.GSAShowLegend) { this.options.alts.sort(SortByValue) };
        },

        sortAlternatives: function () {
            if (this.options.DSASorting) { this.options.alts.sort(SortByValue) };
        }

    });
})(jQuery);

function SortByValue(a, b) {
    return ((a.value < b.value) ? 1 : ((a.value > b.value) ? -1 : 0));
}

function SortByIndex(a, b) {
    return ((a.idx > b.idx) ? 1 : ((a.idx < b.idx) ? -1 : 0));
}

function getRelativePosition(canvas, absX, absY) {
    var rect = canvas.getBoundingClientRect();
    return {
        x: absX - rect.left,
        y: absY - rect.top
    };
}

//    function vbarAltDragHandler(event) {
//        if (isUpdateFinished){
//            isUpdateFinished = false;
//            var altBarsAreaHeight = DSABarHeight * (AltNames.length + 1);
//            AltScrollShift = Math.round(parseInt(event.target.style.top) * altBarsAreaHeight / canvas.height);
//            context.fillStyle = "#ffffff";
//            context.fillRect(canvasHalfWidth, 0, canvasHalfWidth, canvas.height);
//            drawAlternatives(context, AltNames, AltValues, AltInitValues, AltScrollShift, GradientMinValues, GradientMaxValues, SelectedObjectiveIndex, AltScrollShift);
//            isUpdateFinished = true;
//        };
//    }

//    function vbarObjDragHandler(event) {
//        if (isUpdateFinished){
//            isUpdateFinished = false;
//            var objBarsAreaHeight = DSABarHeight * (ObjNames.length + 1);
//            ObjScrollShift = Math.round(parseInt(event.target.style.top) * objBarsAreaHeight / canvas.height);
//            context.fillStyle = "#ffffff";
//            context.fillRect(0, 0, canvasHalfWidth, canvas.height);
//            drawObjectives(context, ObjNames, ObjValues, ObjInitValues, SelectedObjectiveIndex, ObjScrollShift);
//            isUpdateFinished = true;
//        };
//    }

//$(".bar").draggable({
//    containment: "parent"
//});