/* javascript plug-ins for Sensitivity Analysis by SL */

(function ($) {
    $.widget("expertchoice.sa", {

        // Default options
        options: {
            viewMode: 'DSA',
            readOnly: false,
            showRadar: false,
            isMultiView: false,
            normalizationMode: 'unnormalized',
            showComponents: false,
            DSAComponentsData: [],
            DSABarHeight: 35,
            PSABarHeight: 54,
            isMobile: false,
            PSAAllowScroll: true,
            PSAObjExpand: true,
            backgroundColor: '#fff',
            DSAInitBarColor: '#777777',
            DSAObjBarColor: '#6baed6',
            DSAObjBarColorTransp: 'rgba(107,174,214,0.8)',
            DSAObjBarColorSelected: 'rgba(137,204,234,0.9)',
            DSAAltBarColor: 'rgb(100,250,100)',
            DSATitleFont: '12px Arial',
            DSAFontSize: 12,
            DSAFontFace: 'Arial',
            buttonSize: 32,
            //DSASorting: false,
            valDigits: 2,
            altFilterValue: -1,
            PSAShowLines: true,
            PSALineup: true,
            GSAShowLegend: true,
            GSAmarginLeft: 50,
            GSAmarginRight: 40,
            GSAmarginTop: 10,
            GSAmarginDown: 40,
            ASAmaxBallSize: 30,
            ASAChartWidth: 300,
            ASAChartHeight: 100,
            ASAPageObjs: [],
            ASAPageSize: 15,
            ASACurrentPage: 1,
            ASAVisibleObjs: 0,
            ASASortBy: 0,
            SAAltsSortBy: 0,
            ASAFont: '12px Arial',
            objs: [{ id: 1, idx: 1, name: 'Obj1', value: 0.3, initValue: 0.3, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.5 }], gradientMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8 }], gradientInitMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8 }], values2D: [{ altID: 0, val: 0.3, valNorm: 0.3 }, { altID: 1, val: 0.8, valNorm: 0.7 }], color: '#95c5f0' },
                   { id: 2, idx: 2, name: 'Obj2', value: 0.5, initValue: 0.5, gradientMaxValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.5 }], gradientMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7 }], gradientInitMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7 }], values2D: [{ altID: 0, val: 0.3, valNorm: 0.3 }, { altID: 1, val: 0.8, valNorm: 0.7 }], color: '#fa7000' },
                   { id: 3, idx: 3, name: 'Obj3', value: 0.2, initValue: 0.2, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.3 }], gradientMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1 }], gradientInitMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1 }], values2D: [{ altID: 0, val: 0.3, valNorm: 0.3 }, { altID: 1, val: 0.8, valNorm: 0.7 }], color: '#95c5f0' }],
            alts: [{ id: 0, idx: 1, name: 'Alt1', value: 0.7, initValue: 0.7, color: '#95c5f0', visible: 1 },
                   { id: 1, idx: 2, name: 'Alt2', value: 0.3, initValue: 0.3, color: '#fa7000', visible: 1 }],
            ASAdata: {
                objectives: [{ id: 1, name: 'Obj1', prty: 0.3, visible: 1 }, { id: 2, name: 'Obj2', prty: 0.4, visible: 1 }, { id: 3, name: 'Objective 3', prty: 0.2, visible: 1 }, { id: 4, name: 'Objective 4', prty: 0.7, visible: 1 }],
                alternatives: [{ id: 0, idx: 1, name: 'Alt1', initVals: [0.3, 0.4, 0.3, 0.76], newVals: [], oldVals: [], value: 0.6, initValue: 0.6, color: '#95c5f0', visible: 1 }, { id: 1, idx: 2, name: 'Alt2', initVals: [0.6, 0.7, 0.1, 0.4], newVals: [], oldVals: [], value: 0.4, initValue: 0.6, color: '#fa7000', visible: 1 }],
                maxAltValue: 1
            },
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
            m_canvas_legend: null,
            m_canvas_background: null,
            m_canvas_initalts: null,
            m_canvas_alts: null,
            context: null,
            canvasRect: null,

            DSAHalfBarHeight: 32,
            canvasHalfWidth: 0,
            AltScrollShift: 0,
            ObjScrollShift: 0,
            isMouseDown: false,
            isObjSelected: false,
            mouseDownPosition: null,
            mouseDownObjScrollShift: 0,
            mouseDownAltScrollShift: 0,
            isUpdateFinished: true,
            SelectedObjectiveIndex: 0,
            SelectedObjectiveIndexY: 0,
            SelectedAlternativeIndex: 0,
            HTHAlt1Index: 0,
            HTHAlt2Index: 0,
            visibleAlts: [],
            objBarsAreaHeight: 100,
            altBarsAreaHeight: 100,
            isMouseOverObjectives: true,
            isShowCustomColors: true,
            IsObjValuesChanged: false,
            maxObjIndex: 0,
            mouseOverElement: {},
            interactiveMap: [],
            objInitSum: 1,
            legendPosition: 0,
            showMarkers: false,
            legendLabelHeight: 18,
            markersType: 1,
            titleAlternatives: "Alternatives",
            titleObjectives: "Objectives"
        },
        _initSA: function () {
            var opt = this.options;
            if (opt.canvas.width < 200) opt.canvas.width = 200;
            opt.DSAHalfBarHeight = opt.DSABarHeight / 2;
            opt.canvasHalfWidth = opt.canvas.width / 2;
            switch (opt.viewMode) {
                case 'DSA':
                    var altsCount = ((opt.altFilterValue > 0) ? opt.altFilterValue : opt.alts.length);
                    opt.altBarsAreaHeight = opt.DSABarHeight * altsCount + opt.legendLabelHeight;
                    opt.objBarsAreaHeight = opt.DSABarHeight * opt.objs.length + opt.legendLabelHeight;
                    this._applySAAltsFilter();
                    break;
                case 'GSA':
                    var altsCount = ((opt.altFilterValue > 0) ? opt.altFilterValue : opt.alts.length);
                    opt.altBarsAreaHeight = opt.DSABarHeight * altsCount + opt.legendLabelHeight;
                    this._applySAAltsFilter();
                    break;
                case 'ASA':
                    opt.altBarsAreaHeight = 24 * (opt.ASAdata.alternatives.length) + opt.legendLabelHeight;
                    for (var i = 0; i < opt.ASAdata.alternatives.length; i++) {
                        var alt = opt.ASAdata.alternatives[i];
                        alt.newVals = alt.initVals.slice(0);
                        alt.oldVals = alt.initVals.slice(0);
                    };
                    this._calculateASAValues(true);
                    opt.ASAdata.alternatives.sort(SortByValue);
                    this._applyAltsFilter();
                    if (opt.ASASortBy == 0) opt.ASAdata.objectives.sort(SortByIndex);
                    if (opt.ASASortBy == 1) opt.ASAdata.objectives.sort(SortByPriority);
                    if (opt.ASASortBy == 2) opt.ASAdata.objectives.sort(SortByName);
                    break;
                case 'PSA':
                    if (opt.canvas.width <= 300) { opt.isMobile };
                    if (opt.isMobile) { opt.PSABarHeight = 52 } else { opt.PSABarHeight = 64 };
                    opt.altBarsAreaHeight = (24 * (opt.alts.length) + opt.legendLabelHeight) * (opt.PSALineup ? 2 : 1);
                    opt.objBarsAreaHeight = opt.PSABarHeight * (opt.objs.length) + 8;
                    this._applySAAltsFilter();
                    break;
                case 'HTH':
                    this._applySAAltsFilter();
                    break;
                case '2D':
                    var altsCount = ((opt.altFilterValue > 0) ? opt.altFilterValue : opt.alts.length);
                    opt.altBarsAreaHeight = opt.DSABarHeight * altsCount + opt.legendLabelHeight;
                    if (opt.objs.length > 1) { opt.SelectedObjectiveIndex = 1 };
                    this._applySAAltsFilter();
                    break;
                default:
                    opt.objBarsAreaHeight = opt.DSABarHeight * (opt.objs.length) + 8;
                    opt.altBarsAreaHeight = opt.DSABarHeight * (opt.alts.length) + 8;
            };
        },
        applyAltsSorting: function() {
            var opt = this.options;
            if (opt.viewMode == 'DSA' || opt.viewMode == 'GSA' || opt.viewMode == 'PSA') {
                if (opt.SAAltsSortBy == 0) { opt.alts.sort(SortByIndex) };
                if (opt.SAAltsSortBy == 1) { opt.alts.sort(SortByValue) };
                if (opt.SAAltsSortBy == 2) { opt.alts.sort(SortByName) };
            };
        },
        applyObjsSorting: function () {
            var opt = this.options;
            if (opt.ASASortBy == 0) { opt.objs.sort(SortByIndex) };
            if (opt.ASASortBy == 1) { opt.objs.sort(SortByValue) };
            if (opt.ASASortBy == 2) { opt.objs.sort(SortByName) };
        },
        // Constructor function
        _create: function () {
            var opt = this.options;
            opt.canvas = this.element[0];
            opt.context = opt.canvas.getContext('2d');
            opt.canvasRect = opt.canvas.getBoundingClientRect();
            this._updateMinMaxAltBarValue();
            //if ((opt.SAAltsSortBy == 1) || (opt.GSAShowLegend) || (opt.altFilterValue > -1)) { opt.alts.sort(SortByValue) };
            this.applyAltsSorting();

            opt.objInitSum = 0;
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                opt.objInitSum += obj.initValue;
            };
            if (opt.objInitSum == 0) { opt.objInitSum = 1 };

            this._initSA();
            if (opt.viewMode == 'HTH') {
                if (opt.visibleAlts.length > 1) {
                    opt.HTHAlt1Index = opt.visibleAlts[0].id;
                    opt.HTHAlt2Index = opt.visibleAlts[1].id;
                } else {
                    if (opt.visibleAlts.length > 0) {
                        opt.HTHAlt1Index = opt.visibleAlts[0].id;
                        opt.HTHAlt2Index = opt.visibleAlts[0].id;
                    }
                };
            };
            this.redrawSA();
            this._on(opt.canvas, {
                "mousedown": function (event) {
                    if (event.which == 1) {
                        var pos = getRelativePosition(opt.canvas, event.clientX, event.clientY);
                        opt.mouseDownPosition = pos;
                        if (opt.viewMode == 'PSA') { opt.mouseDownObjScrollShift = opt.ObjScrollShift };
                        this._onMouseDown(pos);
                        if (event.target.setCapture) event.target.setCapture();
                    };
                    if (event.which == 2) {
                        switch (this.options.viewMode) {
                            case 'DSA':
                                this.options.viewMode = 'PSA';
                                break;
                            case 'PSA':
                                this.options.viewMode = 'GSA';
                                break;
                            case 'GSA':
                                this.options.viewMode = 'DSA';
                                break;
                            default:
                        };
                        this._updateMinMaxAltBarValue();
                        this._initSA();
                        this.redrawSA();
                    };
                    return false;
                },
                "touchstart": function (event) {
                    var pos = getRelativePosition(opt.canvas, event.originalEvent.touches[0].pageX, event.originalEvent.touches[0].pageY);
                    this._onMouseMove(pos);
                    opt.mouseDownPosition = pos;
                    this._onMouseDown(pos, this);
                    return false;
                },
                "touchmove": function (event) {
                    var pos = getRelativePosition(opt.canvas, event.originalEvent.touches[0].pageX, event.originalEvent.touches[0].pageY);
                    this._onMouseMove(pos, this);
                    return false;
                },
                "mousemove": function (event) {
                    var pos = getRelativePosition(opt.canvas, event.clientX, event.clientY);
                    this._onMouseMove(pos);
                    return false;
                },
                "mouseup": function (event) {
                    this._onMouseUp();
                    return false;
                },
                "mouseout": function (event) {
                    //this._onMouseUp();
                    //return false;
                },
                "touchend": function (event) {
                    this._onMouseUp();
                    return false;
                },
                "wheel": function (event) {
                    var Shift = event.originalEvent.deltaY / Math.abs(event.originalEvent.deltaY);
                    if (isNaN(Shift)) Shift = 0;
                    if (opt.viewMode === 'GSA' || opt.viewMode === '2D') {
                        if (opt.isMouseOverObjectives) {
                            opt.SelectedObjectiveIndex += Shift;
                            if (opt.SelectedObjectiveIndex < 0) { opt.SelectedObjectiveIndex = 0 };
                            if (opt.SelectedObjectiveIndex >= opt.objs.length) { opt.SelectedObjectiveIndex = opt.objs.length - 1 };
                            this.redrawSA();
                            if (opt.isMultiView) this.syncSACharts(false);
                            $('#selSubobjectives').val(opt.SelectedObjectiveIndex);
                        } else {
                            if (opt.mouseOverElement.key === 'legend') {
                                var canvasHeight = opt.canvas.height;
                                var areaHeight = opt.altBarsAreaHeight;
                                var scrollShift = opt.AltScrollShift;
                                var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                                var NewAltScrollShift = opt.AltScrollShift + 24 * Shift;
                                if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };

                                var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;

                                if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                                    NewAltScrollShift = (areaHeight - canvasHeight);
                                };
                                opt.AltScrollShift = NewAltScrollShift;
                                this.redrawSA();
                            };
                        };

                    };
                    if (opt.viewMode === 'DSA') {
                        var canvasHeight = opt.canvas.height;
                        if (opt.isMouseOverObjectives) {
                            if (opt.canvas.height < opt.objBarsAreaHeight) {
                                var areaHeight = opt.objBarsAreaHeight;
                                var scrollShift = opt.ObjScrollShift;
                                var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                                var NewObjScrollShift = opt.ObjScrollShift + opt.DSABarHeight * Shift;
                                if (NewObjScrollShift < 0) { NewObjScrollShift = 0 };

                                var sliderTop = NewObjScrollShift * canvasHeight / areaHeight;

                                if ((sliderTop + sliderHeight) > canvasHeight) {
                                    NewObjScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                                };
                                opt.ObjScrollShift = NewObjScrollShift;
                                this._drawObjectives();
                            };
                        } else {
                            if (opt.canvas.height < opt.altBarsAreaHeight) {
                                var areaHeight = opt.altBarsAreaHeight;
                                var scrollShift = opt.AltScrollShift;
                                var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                                var NewAltScrollShift = opt.AltScrollShift + opt.DSABarHeight * Shift;
                                if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };

                                var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;

                                if ((sliderTop + sliderHeight) > canvasHeight) {
                                    NewAltScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                                };
                                opt.AltScrollShift = NewAltScrollShift;
                                this._drawAlternatives();
                            };
                        };
                    };
                    if (opt.viewMode === 'PSA') {
                        if ((opt.mouseOverElement.key === 'objX') || (opt.mouseOverElement == '') || (opt.mouseOverElement == {})) {
                            if ((!opt.PSAObjExpand) || (opt.PSAAllowScroll)) {
                                var scrollShift = opt.ObjScrollShift;
                                var areaWidth = (opt.objs.length + 2) * opt.PSABarHeight + opt.PSABarHeight / 2 - scrollShift;
                                var canvasWidth = opt.canvasHalfWidth;
                                var NewObjScrollShift = opt.ObjScrollShift + opt.PSABarHeight * Shift;
                                //var NewObjScrollShift = opt.ObjScrollShift + 5*Shift;
                                if (NewObjScrollShift < 0) { NewObjScrollShift = 0 };

                                var sliderLeft = NewObjScrollShift * canvasWidth / areaWidth;
                                var sliderWidth = canvasWidth * canvasWidth / areaWidth;
                                if ((sliderWidth + sliderLeft) > canvasWidth) {
                                    NewObjScrollShift = (canvasWidth - sliderWidth) * areaWidth / canvasWidth + 2;
                                };
                                opt.ObjScrollShift = NewObjScrollShift;
                                this.redrawSA();
                            };
                        };

                        if (opt.mouseOverElement.key === 'legend') {
                            var canvasHeight = opt.canvas.height;
                            var areaHeight = opt.altBarsAreaHeight;
                            var scrollShift = opt.AltScrollShift;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewAltScrollShift = opt.AltScrollShift + 24 * Shift;
                            if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };

                            var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;

                            if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                                NewAltScrollShift = (areaHeight - canvasHeight);
                            };
                            opt.AltScrollShift = NewAltScrollShift;
                            this.redrawSA();
                        };
                    };
                    if (opt.viewMode === 'ASA') {
                        if (opt.mouseOverElement != "" && opt.mouseOverElement.key.indexOf('legend') >= 0) {
                            var canvasHeight = opt.canvas.height;
                            var areaHeight = opt.altBarsAreaHeight;
                            var scrollShift = opt.AltScrollShift;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewAltScrollShift = opt.AltScrollShift + 24 * Shift;
                            if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };

                            var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;

                            if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                                NewAltScrollShift = (areaHeight - canvasHeight);
                            };
                            opt.AltScrollShift = NewAltScrollShift;
                            opt.m_canvas_legend = null;
                            this.redrawSA();
                        };
                    };
                    return false;
                }
            });
        },
        _getDSAComponentCoef: function (altID, objID) {
            var opt = this.options;
            for (var i = 0; i < opt.DSAComponentsData.length; i++) {
                var comp = opt.DSAComponentsData[i];
                if ((comp.altID == altID) && (comp.objID == objID)) {
                    return comp.comp
                }
            }
            return 1;
        },
        _onMouseMove: function (pos) {
            document.body.style.cursor = 'default';
            var opt = this.options;
            if (opt.viewMode === 'ASA') {
                if (!opt.isMouseDown) {
                    var oldIE = opt.mouseOverElement;
                    var ieKey = this._getInteractiveMapElement(pos.x, pos.y);
                    opt.mouseOverElement = ieKey;
                    if ((typeof opt.mouseOverElement.key !== 'undefined') && (!opt.readOnly)) {
                        if ((opt.mouseOverElement.key.indexOf('alt:') >= 0) || (opt.mouseOverElement.key.indexOf('_legend') >= 0)) {
                            document.body.style.cursor = 'hand';
                        };
                    };
                    if (ieKey !== '') {
                        var OldSelectedObjIndex = opt.SelectedObjectiveIndex;
                        var NewSelectedObjIndex = OldSelectedObjIndex;
                        if (opt.mouseOverElement.key.indexOf('obj') >= 0) {
                            NewSelectedObjIndex = Number(opt.mouseOverElement.key.replace('obj', ''));
                        };
                        if (opt.mouseOverElement.key.indexOf('alt') >= 0) {
                            var pairID = opt.mouseOverElement.key.replace('alt', '');
                            NewSelectedObjIndex = Number(pairID.split('_')[0]);
                            opt.SelectedAlternativeIndex = Number(pairID.split('_')[1]);
                        };
                        if (isNaN(NewSelectedObjIndex)) { NewSelectedObjIndex = 0 };
                        if (NewSelectedObjIndex == -1) { NewSelectedObjIndex = OldSelectedObjIndex };
                        opt.SelectedObjectiveIndex = NewSelectedObjIndex;
                        if (oldIE.key != opt.mouseOverElement.key) {
                            opt.m_canvas_alts = null;
                            opt.m_canvas_legend = null;
                            this.redrawSA();
                        };
                    };
                } else {
                    if (opt.mouseOverElement.key === 'altscroll') {
                        var canvasHeight = opt.canvas.height;
                        var areaHeight = opt.altBarsAreaHeight;
                        var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                        var NewAltScrollShift = pos.y * (areaHeight / canvasHeight) - opt.mouseDownPosition.y;
                        if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };
                        var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;
                        if ((sliderTop + sliderHeight) > canvasHeight) {
                            NewAltScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                        };
                        opt.AltScrollShift = NewAltScrollShift;
                        opt.m_canvas_legend = null;
                        this.redrawSA();
                    };
                    if ((opt.mouseOverElement.key.indexOf('alt') >= 0) && (opt.mouseOverElement.key !== 'altscroll')) {
                        var pairIDs = opt.mouseOverElement.key.replace('alt', '').split('_');
                        var objID = Number(pairIDs[0]);
                        var altID = Number(pairIDs[1]);
                        var altASA = this.getASAAltByID(altID);
                        var valCoef = opt.ASAChartHeight / opt.ASAdata.maxAltValue;
                        altASA.newVals[objID] = altASA.oldVals[objID] - (pos.y - opt.mouseDownPosition.y) / valCoef;
                        if (altASA.newVals[objID] < 0) { altASA.newVals[objID] = 0 };
                        if (altASA.newVals[objID] > opt.ASAdata.maxAltValue) { altASA.newVals[objID] = opt.ASAdata.maxAltValue };
                        this._calculateASAValues(false);
                        opt.ASAdata.alternatives.sort(SortByValue);
                        opt.m_canvas_legend = null;
                        //opt.m_canvas_alts = null;
                        this.redrawSA();
                    };
                };
            };
            if ((opt.viewMode === 'DSA') || (opt.viewMode === 'PSA')) {
                opt.isMouseOverObjectives = (pos.x < opt.canvasHalfWidth - 10);
                var oldMousePos = opt.mouseOverElement;
                if (!opt.isMouseDown) {
                    var ieKey = this._getInteractiveMapElement(pos.x, pos.y);
                    opt.mouseOverElement = ieKey;
                    if (oldMousePos != opt.mouseOverElement) {
                        this.redrawSA();
                    };
                };
                if ((typeof opt.mouseOverElement.key !== 'undefined') && (!opt.readOnly)) {
                    if (opt.mouseOverElement.key.indexOf('alt:') >= 0) {
                        document.body.style.cursor = 'hand';
                    };
                };
                if (opt.viewMode === 'DSA') {
                    if (opt.isMouseDown) {
                        if (opt.mouseOverElement.key === 'objscroll') {
                            var canvasHeight = opt.canvas.height;
                            var areaHeight = opt.objBarsAreaHeight;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewObjScrollShift = pos.y * (areaHeight / canvasHeight) - opt.mouseDownPosition.y;
                            if (NewObjScrollShift < 0) { NewObjScrollShift = 0 };
                            var sliderTop = NewObjScrollShift * canvasHeight / areaHeight;
                            if ((sliderTop + sliderHeight) > canvasHeight) {
                                NewObjScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                            };
                            opt.ObjScrollShift = NewObjScrollShift;
                        };
                        if (opt.mouseOverElement.key === 'altscroll') {
                            var canvasHeight = opt.canvas.height;
                            var areaHeight = opt.altBarsAreaHeight;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewAltScrollShift = pos.y * (areaHeight / canvasHeight) - opt.mouseDownPosition.y;
                            if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };
                            var sliderTop = NewAltScrollShift * canvasHeight / areaHeight;
                            if ((sliderTop + sliderHeight) > canvasHeight) {
                                NewAltScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                            };
                            opt.AltScrollShift = NewAltScrollShift;
                        };
                        if ((opt.SelectedObjectiveIndex >= 0)&&(opt.isMouseOverObjectives)&&(opt.isObjSelected)) {
                            this.updateContent(pos.x, pos.y);
                        };
                    };
                    if (oldMousePos != opt.mouseOverElement) {
                        if (opt.canvas.height < opt.objBarsAreaHeight) {
                            this._drawScrollbar(true);
                        };
                        if (opt.canvas.height < opt.altBarsAreaHeight) this._drawScrollbar(false);
                    };
                    if (opt.isMouseDown) {
                        if (opt.canvas.height < opt.objBarsAreaHeight) { this._drawObjectives() };
                        if (opt.canvas.height < opt.altBarsAreaHeight) { this._drawAlternatives() };
                    };
                };
                if ((opt.viewMode === 'PSA') && (opt.showRadar)) {
                    if ((opt.isMouseDown) && (opt.SelectedObjectiveIndex >= 0) && (opt.isMouseOverObjectives) && (opt.isObjSelected)) {
                        this.updateContent(pos.x, pos.y);
                    };
                };
                if ((opt.viewMode === 'PSA' || opt.viewMode === '2D') && (!opt.showRadar)) {
                    if (((opt.mouseOverElement.key === 'legend') || (opt.mouseOverElement.key === 'altscroll')) && (opt.isMouseDown)) {
                        var canvasHeight = opt.canvas.height;
                        var areaHeight = opt.altBarsAreaHeight;
                        var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                        var NewAltScrollShift = pos.y * (areaHeight / canvasHeight) - opt.mouseDownPosition.y;
                        if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };
                        if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                            NewAltScrollShift = (areaHeight - canvasHeight);
                        };
                        opt.AltScrollShift = NewAltScrollShift;
                        this.redrawSA();
                        opt.isUpdateFinished = false;
                    } else {
                        opt.isMouseOverObjectives = true;
                        if ((opt.isMouseDown) && (opt.canvas.width / 2 < opt.objs.length * opt.PSABarHeight) && ((opt.mouseOverElement == '') || (opt.mouseOverElement.key === 'hscroll') || (opt.mouseOverElement.key === 'hscrollbar'))) {
                            var canvasHeight = opt.canvas.width / 2;
                            var areaHeight = opt.objs.length * opt.PSABarHeight + 8;
                            var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                            var NewObjScrollShift = opt.mouseDownObjScrollShift + opt.mouseDownPosition.x - pos.x;
                            if (opt.mouseOverElement.key === 'hscroll') {
                                NewObjScrollShift = pos.x * (areaHeight / canvasHeight) - opt.mouseDownPosition.x;
                            }
                            if (opt.mouseOverElement.key === 'hscrollbar') {
                                NewObjScrollShift = opt.mouseDownObjScrollShift - (opt.mouseDownPosition.x - pos.x) * (areaHeight / canvasHeight);
                            }
                            if (NewObjScrollShift < 0) { NewObjScrollShift = 0 };
                            var sliderTop = NewObjScrollShift * canvasHeight / areaHeight;
                            if ((sliderTop + sliderHeight) > canvasHeight) {
                                NewObjScrollShift = (canvasHeight - sliderHeight) * areaHeight / canvasHeight;
                            };
                            opt.ObjScrollShift = NewObjScrollShift;
                            this.redrawSA();
                        };
                        if ((opt.isMouseDown) && (opt.SelectedObjectiveIndex >= 0) && (opt.isMouseOverObjectives) && (opt.isObjSelected)) {
                            this.updateContent(pos.x, pos.y);
                        };
                    };
                    if (oldMousePos != opt.mouseOverElement) {
                        this._drawHorizontalScrollbar();
                    };
                };
                //if (!opt.isMouseDown) {
                //    var OldSelectedObjIndex = opt.SelectedObjectiveIndex;
                //    var NewSelectedObjIndex = this.GetMouseOverObjectiveIndex(pos.x, pos.y);
                //    if (isNaN(NewSelectedObjIndex)) { NewSelectedObjIndex = 0 };
                //    if (opt.viewMode === 'PSA') {
                //        if (NewSelectedObjIndex == -1) { NewSelectedObjIndex = OldSelectedObjIndex };
                //    };
                //    opt.SelectedObjectiveIndex = NewSelectedObjIndex;
                //    if (OldSelectedObjIndex != opt.SelectedObjectiveIndex) {
                //        this.redrawSA();
                //        if (opt.isMultiView) this.syncSACharts(false);
                //    };
                //};
            };
            if (opt.viewMode === 'GSA') {
                if ((opt.mouseOverElement.key === 'legend') && (opt.isMouseDown)) {
                    var canvasHeight = opt.canvas.height;
                    var areaHeight = opt.altBarsAreaHeight;
                    var sliderHeight = canvasHeight * canvasHeight / areaHeight;
                    var NewAltScrollShift = pos.y * (areaHeight / canvasHeight) - opt.mouseDownPosition.y;
                    if (NewAltScrollShift < 0) { NewAltScrollShift = 0 };
                    if ((areaHeight - NewAltScrollShift) < (canvasHeight)) {
                        NewAltScrollShift = (areaHeight - canvasHeight);
                    };
                    opt.AltScrollShift = NewAltScrollShift;
                    this.redrawSA();
                };
                if (!opt.isMouseDown) {
                    var prevMouseOver = opt.mouseOverElement;
                    var ieElement = this._getInteractiveMapElement(pos.x, pos.y);
                    opt.isMouseOverObjectives = (ieElement.key === 'objX');
                    opt.mouseOverElement = {};
                    if (!opt.isMouseOverObjectives) {
                        opt.mouseOverElement = ieElement;
                        document.body.style.cursor = 'default';
                    } else {
                        if (!opt.readOnly) document.body.style.cursor = 'col-resize';
                    };
                    if ((prevMouseOver != opt.mouseOverElement) && (ieElement.key != 'legend')) { this.redrawSA() };
                    if ((typeof opt.mouseOverElement.key !== 'undefined') && (!opt.readOnly)) {
                        if (opt.mouseOverElement.key.indexOf('alt:') >= 0) {
                            document.body.style.cursor = 'hand';
                        };
                    };
                };
            };

            if (opt.viewMode === '2D') {
                var prevMouseOver = opt.mouseOverElement;
                var ieElement = this._getInteractiveMapElement(pos.x, pos.y);
                opt.mouseOverElement = ieElement;
                if (prevMouseOver != opt.mouseOverElement) { this.redrawSA() };
            };

            if (opt.viewMode === 'HTH') {
                var prevMouseOver = opt.mouseOverElement;
                var ieKey = this._getInteractiveMapElement(pos.x, pos.y);
                opt.isMouseOverObjectives = (ieKey.key === 'objX');
                opt.mouseOverElement = ieKey;
                if (prevMouseOver != opt.mouseOverElement) { this.redrawSA() };
                if ((typeof opt.mouseOverElement.key !== 'undefined') && (!opt.readOnly)) {
                    if (opt.mouseOverElement.key.indexOf('alt:') >= 0) {
                        document.body.style.cursor = 'hand';
                    };
                };
            };

            if (opt.isUpdateFinished) {
                opt.isUpdateFinished = false;
                this.updateContent(pos.x, pos.y);
            };
        },
        _onMouseDown: function (pos) {
            var opt = this.options;
            if (!opt.readOnly) {
                opt.isMouseDown = true;
                opt.isUpdateFinished = false;
                if (opt.viewMode === 'ASA') {
                    if (typeof opt.mouseOverElement.key !== "undefined") {
                        if (opt.mouseOverElement.key.indexOf("alt") >= 0) {
                            opt.m_canvas_alts = null;
                            this.redrawSA();
                            if (opt.mouseOverElement.key.indexOf('_legend') >= 0) {
                                var altid = opt.mouseOverElement.key.replace('_legend', '').replace('alt' + opt.ASAdata.objectives.length + '_', '');
                                openColorPicker(pos.x, pos.y, altid, opt.canvas);
                            };
                        };
                    };
                };
                if ((opt.viewMode === 'DSA') || (opt.viewMode === 'PSA')) {
                    var needMouseMove = false;
                    if (opt.isMouseOverObjectives) {
                        var newSelIndex = this.GetMouseOverObjectiveIndex(pos.x, pos.y);
                        if (!isNaN(newSelIndex) && newSelIndex >= 0 && newSelIndex < opt.objs.length) {
                            needMouseMove = (opt.SelectedObjectiveIndex == newSelIndex);
                            opt.SelectedObjectiveIndex = newSelIndex;
                            this.redrawSA();
                            if (opt.isMultiView) this.syncSACharts(false);
                        };
                    };
                    if (opt.viewMode === 'DSA') {
                        opt.mouseDownObjScrollShift = opt.ObjScrollShift;
                        opt.mouseDownAltScrollShift = opt.AltScrollShift;
                        if (opt.canvas.height < opt.objBarsAreaHeight) this._drawScrollbar(true);
                        if (opt.canvas.height < opt.altBarsAreaHeight) this._drawScrollbar(false);
                    };
                    if ((typeof opt.mouseOverElement.key !== 'undefined') && (opt.mouseOverElement.key.indexOf('alt:') >= 0)) openColorPicker(pos.x, pos.y, opt.mouseOverElement.key.replace('alt:', ''), opt.canvas);
                    if (needMouseMove) { opt.isObjSelected = true; this._onMouseMove(pos); } else { opt.isObjSelected = false; }
                };
                if (opt.viewMode === 'GSA') {
                    if ((opt.isMouseOverObjectives) && (!opt.readOnly)) {
                        document.body.style.cursor = 'col-resize';
                        this.updateContent(pos.x, pos.y);
                    } else {
                        document.body.style.cursor = 'default';
                        if ((opt.mouseOverElement.key === 'prev') && (opt.SelectedObjectiveIndex > 0)) { opt.SelectedObjectiveIndex--; $('#selSubobjectives').val(opt.SelectedObjectiveIndex); };
                        if ((opt.mouseOverElement.key === 'next') && (opt.SelectedObjectiveIndex < opt.objs.length - 1)) { opt.SelectedObjectiveIndex++; $('#selSubobjectives').val(opt.SelectedObjectiveIndex); };
                        this.redrawSA();
                        if ((typeof opt.mouseOverElement.key !== 'undefined') && (opt.mouseOverElement.key.indexOf('alt:') >= 0)) openColorPicker(pos.x, pos.y, opt.mouseOverElement.key.replace('alt:', ''), opt.canvas);
                        if (opt.isMultiView) this.syncSACharts(false);
                    };
                };
                if (opt.viewMode === '2D') {
                    if (typeof opt.mouseOverElement.key !== "undefined") {
                        if ((opt.mouseOverElement.key === 'prev') && (opt.SelectedObjectiveIndex > 0)) { opt.SelectedObjectiveIndex-- };
                        if ((opt.mouseOverElement.key === 'next') && (opt.SelectedObjectiveIndex < opt.objs.length - 1)) { opt.SelectedObjectiveIndex++ };
                        if ((opt.mouseOverElement.key === 'prevY') && (opt.SelectedObjectiveIndexY > 0)) { opt.SelectedObjectiveIndexY-- };
                        if ((opt.mouseOverElement.key === 'nextY') && (opt.SelectedObjectiveIndexY < opt.objs.length - 1)) { opt.SelectedObjectiveIndexY++ };
                        this.redrawSA();
                        if (opt.mouseOverElement.key.indexOf('alt:') >= 0) {
                            var altidx = opt.mouseOverElement.key.replace('alt:', '');
                            var altid = opt.alts[Number(altidx)].id;
                            openColorPicker(pos.x, pos.y, altid, opt.canvas);
                        };
                    };
                };
                if (opt.viewMode === 'HTH') {
                    var alt1Idx = opt.visibleAlts.indexOf(this.getAltByID(opt.HTHAlt1Index));
                    var alt2Idx = opt.visibleAlts.indexOf(this.getAltByID(opt.HTHAlt2Index));
                    if ((opt.mouseOverElement.key === 'nextAlt1') && (alt1Idx < opt.visibleAlts.length - 1)) { alt1Idx++ };
                    if ((opt.mouseOverElement.key === 'prevAlt1') && (alt1Idx > 0)) { alt1Idx-- };
                    if ((opt.mouseOverElement.key === 'nextAlt2') && (alt2Idx < opt.visibleAlts.length - 1)) { alt2Idx++ };
                    if ((opt.mouseOverElement.key === 'prevAlt2') && (alt2Idx > 0)) { alt2Idx-- };
                    if (alt1Idx > -1) opt.HTHAlt1Index = opt.visibleAlts[alt1Idx].id;
                    if (alt2Idx > -1) opt.HTHAlt2Index = opt.visibleAlts[alt2Idx].id;
                    this.redrawSA();
                    if ((typeof opt.mouseOverElement.key !== 'undefined') && (opt.mouseOverElement.key.indexOf('alt:') >= 0)) openColorPicker(pos.x, pos.y, opt.mouseOverElement.key.replace('alt:', ''), opt.canvas);
                };
            };
        },
        _onMouseUp: function () {
            if (document.releaseCapture) document.releaseCapture();
            document.body.style.cursor = 'default';
            var opt = this.options;
            opt.isMouseDown = false;
            if (opt.viewMode === 'DSA') {
                if (opt.canvas.height < opt.objBarsAreaHeight) this._drawScrollbar(true);
                if (opt.canvas.height < opt.altBarsAreaHeight) this._drawScrollbar(false);
            };
            if (opt.viewMode === 'ASA') {
                if (opt.mouseOverElement !== '') {
                    if ((opt.mouseOverElement.key.indexOf('alt') >= 0) && (opt.mouseOverElement.key !== 'altscroll')) {
                        var pairIDs = opt.mouseOverElement.key.replace('alt', '').split('_');
                        var objID = Number(pairIDs[0]);
                        var altID = Number(pairIDs[1]);
                        var altASA = this.getASAAltByID(altID);
                        altASA.oldVals[objID] = altASA.newVals[objID];
                    };
                };
            };
            if (opt.IsObjValuesChanged) { this._dsaUpdateCurrentValues() };
        },
        _updateMinMaxAltBarValue: function () {
            var opt = this.options;
            var aMaxVal = 0;
            var aMinVal = 1;
            var aMaxValNorm = 0;
            var aMinValNorm = 1;
            opt.objGradientSum = [];
            for (var i = 0; i < opt.objs.length; i++) {
                var xMaxCoef = 0;
                var obj = opt.objs[i];

                for (var k = 0; k < opt.alts.length; k++) {
                    var altID = opt.alts[k].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                    xMaxCoef += maxVal;
                };

                if (xMaxCoef == 0) { xMaxCoef = 1 };
                opt.objGradientSum.push(xMaxCoef);
                for (var j = 0; j < opt.alts.length; j++) {
                    var altID = opt.alts[j].id;
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
            if (aMaxVal > 1) aMaxVal = 1;
            if (aMaxValNorm > 1) aMaxValNorm = 1;
            if (aMaxVal > 0) { opt.MaxAltValue = aMaxVal };
            if (aMinVal < 1) { opt.MinAltValue = aMinVal };
            if (aMaxValNorm > 0) { opt.MaxAltValueNormalized = aMaxValNorm };
            if (aMinValNorm < 1) { opt.MinAltValueNormalized = aMinValNorm };
        },

        _getMaxScaleValue: function () {
            var opt = this.options;
            var aMaxVal = 0;
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                var xMaxCoef = 0;
                if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) {
                    for (var k = 0; k < opt.alts.length; k++) {
                        var altID = opt.alts[k].id;
                        var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                        xMaxCoef += maxVal;
                    };
                };
                if (xMaxCoef == 0) { xMaxCoef = 1 };
                for (var j = 0; j < opt.alts.length; j++) {
                    var altID = opt.alts[j].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID) / xMaxCoef;
                    if (aMaxVal < maxVal) { aMaxVal = maxVal };
                };
            };
            if (aMaxVal > 0) { return aMaxVal };
            return 1;
        },
        _getMaxObjScaleValue: function (obj) {
            var opt = this.options;
            var aMaxVal = 0;
            var xMaxCoef = 0;
            if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) {
                for (var k = 0; k < opt.alts.length; k++) {
                    var altID = opt.alts[k].id;
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                    xMaxCoef += maxVal;
                };
            };
            if (xMaxCoef == 0) { xMaxCoef = 1 };
            for (var j = 0; j < opt.alts.length; j++) {
                var altID = opt.alts[j].id;
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
                    //var ctx = this.options.context;
                    //this.drawHint(ctx, [intElement.key, x, y], 10, 10);
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
        _drawDSABar2: function (index, barX, barY, barWidth, barName, barValue, barInitValue, barColor, isSelected, barMaxValue, barMinValue, valueNormCoef, components) {
            var opt = this.options;
            var ctx = opt.context;
            var bh = opt.DSABarHeight / 2;
            ctx.save();
            ctx.beginPath();
            ctx.rect(barX, barY + opt.DSABarHeight / 2, barWidth + 1, bh);
            ctx.lineWidth = 1;
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
            if (components.length == 0) {
                ctx.beginPath();
                ctx.rect(barX + 1, barY + opt.DSABarHeight / 2 + 1, (barWidth - 1) * ((barValue - barMinValue) / normCoef), bh - 2);
                ctx.fillStyle = barColor;
                ctx.fill();
            } else {
                var xshift = barX + 1;
                var yshift = barY + opt.DSABarHeight / 2 + 1;
                var compHeight = bh - 2;
                for (var i = 0; i < components.length; i++) {
                    var comp = components[i];
                    ctx.beginPath();
                    var compWidth = (barWidth - 1) * ((barValue - barMinValue) / normCoef) * comp.value;
                    ctx.rect(xshift, yshift, compWidth, compHeight);
                    ctx.fillStyle = comp.color;
                    ctx.fill();
                    xshift += compWidth;
                };
            };

            ctx.beginPath();
            ctx.font = opt.DSATitleFont;
            ctx.fillStyle = 'rgba(255,255,255,0.7)';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';
            var truncName = getTruncatedString(ctx, barName, barWidth - 50);
            ctx.fillStyle = 'black';
            ctx.beginPath();
            ctx.fillText(truncName, barX + 4, barY + 4);
            ctx.textAlign = 'right';
            ctx.font = opt.DSATitleFont;
            ctx.textBaseline = 'top';

            ctx.beginPath();
            var barNormValue = barValue / valueNormCoef;
            var sbarValue = (barNormValue * 100).toFixed(opt.valDigits) + '%';

            ctx.fillStyle = 'black';
            ctx.beginPath();
            ctx.fillText(sbarValue, barX + barWidth - 4, barY + 4);
            ctx.restore();

            if (opt.showMarkers) {
                var x = barX + (barWidth - 1) * ((barInitValue - barMinValue) / normCoef) + 1;
                ctx.strokeStyle = opt.DSAInitBarColor;
                ctx.lineWidth = 1.5;
                ctx.moveTo(x, barY + opt.DSABarHeight / 2);
                ctx.lineTo(x, barY + opt.DSABarHeight);
                ctx.stroke();
            };

        },
        _drawDSABar: function (index, barX, barY, barWidth, barName, barValue, barInitValue, barColor, isSelected, barMaxValue, barMinValue, valueNormCoef) {
            var opt = this.options;
            var ctx = opt.context;
            ctx.save();
            ctx.beginPath();
            ctx.rect(barX, barY, barWidth + 1, opt.DSABarHeight - opt.DSABarHeight / 2);
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
            ctx.rect(barX + 1, barY + 1, (barWidth - 1) * ((barValue - barMinValue) / normCoef), opt.DSABarHeight - 18);
            ctx.fillStyle = barColor;
            ctx.fill();
            ctx.beginPath();
            ctx.font = opt.DSATitleFont;
            //ctx.font = getFont(ctx, barName, opt.DSAFontSize, 10, barWidth - 100, opt.DSAFontFace);
            ctx.fillStyle = 'rgba(255,255,255,0.7)';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';
            var truncName = getTruncatedString(ctx, barName, barWidth - 100);
            var w = ctx.measureText(truncName).width + 3;
            ctx.shadowBlur = 8;
            ctx.shadowColor = 'rgba(255,255,255,0.5)';
            ctx.rect(barX + 3, barY + 4, w, opt.DSABarHeight - opt.DSABarHeight / 2 - 6);
            ctx.fill();
            ctx.fillStyle = 'black';
            ctx.beginPath();
            ctx.fillText(truncName, barX + 4, barY + 4);
            //rectText(context, barName, DSATitleFont, barX, titleY + 2, canvasHalfWidth - valueMaxWidth, 22, getTextHeight(DSATitleFont), 3, false);
            ctx.textAlign = 'right';
            ctx.font = opt.DSATitleFont;
            ctx.textBaseline = 'top';

            ctx.beginPath();
            var barNormValue = barValue / valueNormCoef;
            var sbarValue = (barNormValue * 100).toFixed(opt.valDigits) + '%';
            w = ctx.measureText(sbarValue).width + 3;
            ctx.fillStyle = 'rgba(255,255,255,0.5)';
            ctx.rect(barX + barWidth - 4 - w, barY + 4, w, opt.DSABarHeight - opt.DSABarHeight / 2 - 6);
            ctx.fill();
            ctx.fillStyle = 'black';
            ctx.beginPath();
            ctx.fillText(sbarValue, barX + barWidth - 4, barY + 4);
            ctx.restore();

            if (opt.showMarkers) {
                var x = barX + (barWidth - 1) * ((barInitValue - barMinValue) / normCoef) + 1;
                ctx.fillStyle = opt.DSAInitBarColor;
                this._drawArrow(ctx, x, barY, 'down', opt.markersType);
            };

        },

        _drawPSABar: function (index, barName, barValue, barInitValue, barColor, isSelected, scrollShift, expandWidth, intMap) {
            var opt = this.options;
            var PSAHalfBarHeight = opt.PSABarHeight / 2;
            var barX = index * expandWidth - scrollShift;
            if (!opt.isMobile) { barX += PSAHalfBarHeight } else { barX += 4 };
            if (barX < opt.canvasHalfWidth - PSAHalfBarHeight) {
                var barY = 20;
                var titleY = barX - 6;
                var barWidth = opt.canvas.height - 30;
                var ctx = opt.context;
                ctx.save();
                iex = barX;
                iey = barY + 1;
                iew = PSAHalfBarHeight * 2 - 4;
                ieh = barWidth + 1;
                var x1, y1, x2, y2;
                x1 = iex - expandWidth / 2;
                y1 = iey;
                x2 = x1 + iew + expandWidth;
                y2 = y1 + ieh;
                intEl = { key: 'obj' + index, rect: [x1, y1, iew + expandWidth, ieh], element: ['obj', index], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: barName };
                intMap.push(intEl);
                ctx.beginPath();
                ctx.rect(iex, iey, iew / 2, ieh);
                ctx.lineWidth = 0.7;
                ctx.strokeStyle = '#ddd';
                ctx.fillStyle = 'rgba(255,255,255,0.4)';
                if (isSelected) {
                    ctx.strokeStyle = '#aaa';
                    ctx.fillStyle = 'rgba(255,255,255,0.4)';
                    if ((opt.isMouseOverObjectives) && (!opt.readOnly)) document.body.style.cursor = 'hand';
                };
                ctx.fill();
                ctx.stroke();
                ctx.restore();

                ctx.beginPath();
                ctx.rect(barX + 1, barY + barWidth + 1, PSAHalfBarHeight - 4, -(barWidth - 1) * barValue);
                ctx.fillStyle = barColor;
                ctx.fill();

                ctx.fillStyle = 'black';
                ctx.save();
                ctx.rotate(-Math.PI / 2);
                ctx.beginPath();
                ctx.font = opt.DSATitleFont;
                //ctx.font = getFont(ctx, barName, opt.DSAFontSize, 12, barWidth - 60, opt.DSAFontFace);
                ctx.textAlign = 'left';
                ctx.textBaseline = 'top';

                var w = barWidth; //ctx.measureText(barName).width + 3;
                var maxWidth = barWidth;// - ctx.measureText((barValue * 100).toFixed(opt.valDigits) + '%%%').width - 4;
                var truncName = getTruncatedString(ctx, (barValue * 100).toFixed(opt.valDigits) + " " + barName, maxWidth);
                var labelWidth = ctx.measureText(truncName).width + 4;
                //ctx.shadowBlur = 8;
                //ctx.shadowColor = 'rgba(255,255,255,0.8)';
                ctx.fillStyle = 'rgba(255,255,255,0.5)';
                ctx.rect(-barWidth - 6, barX - 15, labelWidth, 13);
                //ctx.fill();
                //ctx.rect(-(barWidth - maxWidth), barX - 20, (barWidth - maxWidth), PSAHalfBarHeight - 13);
                ctx.fill();

                ctx.beginPath();
                ctx.shadowBlur = 4;
                ctx.fillStyle = 'black';
                //ctx.shadowColor = "#fff";

                ctx.fillText(truncName, -barWidth - 4, barX - 16);
                ctx.restore();

                //ctx.save();
                //ctx.rotate(-Math.PI / 2);
                //ctx.textAlign = 'right';
                ////if (!opt.isMobile) { ctx.font = opt.DSATitleFont };
                //ctx.font = opt.DSATitleFont;
                //ctx.textBaseline = 'top';
                //ctx.shadowBlur = 4;
                //ctx.shadowColor = "#fff";
                //ctx.fillText((barValue * 100).toFixed(opt.valDigits) + '%', -16, barX - 16);
                //ctx.restore();

                var x = (barWidth - 1) * barInitValue - barY - 1;
                ctx.save();
                ctx.strokeStyle = opt.DSAInitBarColor;
                ctx.lineWidth = 2;
                ctx.moveTo(barX - 2, barWidth - x);
                ctx.lineTo(PSAHalfBarHeight + barX, barWidth - x);
                ctx.stroke();
                ctx.restore();

                //this._drawArrow(ctx, PSAHalfBarHeight/2 + barX - 2, barWidth - x, 'left');
            };
        },

        _drawDSAMinMaxMarks: function (index, barX, barWidth, minValue, maxValue, scrollShift, barMaxValue, barMinValue) {
            var opt = this.options;
            var barY = index * opt.DSABarHeight - scrollShift;
            var barY2 = barY + opt.DSABarHeight;
            var ctx = opt.context;
            var normCoef = (barMaxValue - barMinValue);

            var x = barX + (barWidth - 1) * ((minValue - barMinValue) / normCoef) + 1;
            ctx.fillStyle = 'rgba(0,0,255,0.6)';
            this._drawArrow(ctx, x, barY2, 'up', opt.markersType);

            x = barX + (barWidth - 1) * ((maxValue - barMinValue) / normCoef) + 1;
            ctx.fillStyle = 'rgba(255,0,0,0.6)';
            this._drawArrow(ctx, x, barY2, 'up', opt.markersType);
        },

        _drawObjectives: function () {
            var opt = this.options;
            var ctx = opt.context;
            var intMap = opt.interactiveMap;
            var iex, iey, iew, ieh;
            ctx.fillStyle = opt.backgroundColor;
            ctx.fillRect(0, 0, opt.canvasHalfWidth, opt.canvas.height);

            ctx.beginPath();
            ctx.font = opt.DSATitleFont;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'top';
            ctx.fillStyle = "black";
            ctx.fillText(opt.titleObjectives, (opt.canvasHalfWidth - 30) / 2 + 10, 4);

            var NormCoef = opt.objInitSum;
            for (var i = 0; i < opt.objs.length; i++) {
                var objColor = opt.DSAObjBarColor;
                var obj = opt.objs[i];
                //if (opt.showComponents) {
                    objColor = obj.color;
                //};
                var barY = i * opt.DSABarHeight + opt.DSAHalfBarHeight - opt.ObjScrollShift;
                if (barY > opt.canvas.height) { break };
                if (barY >= 0) {
                    var elementName = 'obj:' + obj.id;
                    iex = 10;
                    iey = barY;
                    iew = opt.canvasHalfWidth - 30;
                    ieh = opt.DSABarHeight;
                    var x1, y1, x2, y2;
                    x1 = iex;
                    x2 = iex + iew;
                    y1 = iey;
                    y2 = iey + ieh;
                    var intEl = { key: elementName, rect: [x1, y1, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: obj.name + ' ' + obj.value };
                    intMap.push(intEl);
                    if ((opt.SelectedAlternativeIndex == i)&&(!opt.readOnly)) { document.body.style.cursor = 'hand' };
                    this._drawDSABar2(i, iex, iey, iew, obj.name, obj.value, obj.initValue, objColor, opt.SelectedObjectiveIndex === i, NormCoef, 0, 1, []);
                };
            };
            if (opt.canvas.height < opt.objBarsAreaHeight) {
                this._drawScrollbar(true);
            };
            opt.interactiveMap = intMap;
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
                case 'normSelected':
                    for (var i = 0; i < this.options.alts.length; i++) {
                        var alt = this.options.alts[i];
                        if (alt.visible == 1) { normCoef += alt.value };
                    };
                    break;
                default:
                    return 1;
            };

            if (normCoef == 0) { return 1 } else { return normCoef };
        },

        _drawAlternatives: function () {
            var opt = this.options;
            var intMap = opt.interactiveMap;
            var iex, iey, iew, ieh;
            var ctx = opt.context;
            ctx.fillStyle = opt.backgroundColor;
            ctx.fillRect(opt.canvasHalfWidth, 0, opt.canvasHalfWidth, opt.canvas.height);
            var altNormCoef = this._getAltNormCoef();
            var altsCountFilter = opt.alts.length;
            var idx = 0;
            ctx.beginPath();
            ctx.font = opt.DSATitleFont;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'top';
            ctx.fillStyle = "black";
            ctx.fillText(opt.titleAlternatives, opt.canvasHalfWidth  + (opt.canvasHalfWidth - 30) / 2 + 10, 4);

            for (var i = 0; i < altsCountFilter; i++) {
                var altColor = opt.DSAAltBarColor;
                var alt = opt.alts[i];
                if (alt.visible == 1) {
                    if (opt.isShowCustomColors) {
                        //altColor = ColorPalette[alt.idx % ColorPalette.length]
                        altColor = alt.color
                    };
                    var barY = idx * opt.DSABarHeight + opt.DSAHalfBarHeight - opt.AltScrollShift;
                    if (barY > opt.canvas.height) { break };
                    if (barY >= 0) {
                        var elementName = 'alt:' + alt.id;
                        iex = opt.canvasHalfWidth + 10;
                        iey = barY;
                        iew = opt.canvasHalfWidth - 30;
                        ieh = opt.DSABarHeight;
                        var x1, y1, x2, y2;
                        x1 = iex;
                        x2 = iex + iew;
                        y1 = iey;
                        y2 = iey + ieh;
                        var intEl = { key: elementName, rect: [x1, y1, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: alt.name + ' ' + alt.value };
                        intMap.push(intEl);
                        var components = [];
                        if (opt.showComponents) {
                            var componentsSum = 0;
                            for (var j = 0; j < opt.objs.length; j++) {
                                var obj = opt.objs[j];
                                var compCoef = this._getDSAComponentCoef(alt.id, obj.id);
                                var acomp = { name: obj.name, color: obj.color, value: obj.value * compCoef / alt.value };
                                components.push(acomp);
                                componentsSum += compCoef * obj.value;
                            };
                        };
                        this._drawDSABar2(idx, iex, iey, iew, alt.name, alt.value, alt.initValue, altColor, false, opt.MaxAltValue, 0, altNormCoef, components);
                        if ((opt.SelectedObjectiveIndex < opt.objs.length) && (opt.SelectedObjectiveIndex >= 0)) {
                            var altID = alt.id;
                            var obj = opt.objs[opt.SelectedObjectiveIndex];
                            var minVal = this.getGradientValueByAltID(obj.gradientMinValues, altID);
                            var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, altID);
                            if (opt.showMarkers) {
                                this._drawDSAMinMaxMarks(idx, opt.canvasHalfWidth + 10, opt.canvasHalfWidth - 30, minVal, maxVal, opt.AltScrollShift, opt.MaxAltValue, 0)
                            };
                        };
                    };
                    idx++;
                };
            };
            this._drawScrollbar(false);
            //if (opt.canvas.height < opt.altBarsAreaHeight) {
            //    this._drawScrollbar(false);
            //};
            opt.interactiveMap = intMap;
        },

        _drawScrollbar: function (isObjectives) {
            var opt = this.options;
            var intMap = opt.interactiveMap;
            var iex, iey, iew, ieh;
            var rightBound = opt.canvas.width;
            var canvasHeight = opt.canvas.height;
            var areaHeight = opt.altBarsAreaHeight;
            var scrollShift = opt.AltScrollShift;
            if (canvasHeight < areaHeight) {
                var ctx = opt.context;
                var scrollColor = '#aaa';
                if (isObjectives) {
                    rightBound = opt.canvasHalfWidth;
                    areaHeight = opt.objBarsAreaHeight;
                    scrollShift = opt.ObjScrollShift;
                    if (opt.mouseOverElement.key === 'objscroll') {
                        if (opt.isMouseDown) {
                            scrollColor = '#555'
                        } else {
                            scrollColor = '#777'
                        };
                    };
                } else {
                    if (opt.mouseOverElement.key === 'altscroll') {
                        if (opt.isMouseDown) {
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
                var x1, y1, x2, y2;
                x1 = iex;
                x2 = iex + iew;
                y1 = iey;
                y2 = iey + ieh;
                intEl = { key: elementName, rect: [x1, y1, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
                intMap.push(intEl);
                iex = rightBound - 13;
                iey = sliderTop + 2;
                iew = 12;
                ieh = sliderHeight - 4;
                ctx.beginPath();
                ctx.rect(iex, iey, iew, ieh);
                ctx.fillStyle = scrollColor;
                ctx.fill();

                opt.interactiveMap = intMap;
            };
        },

        _drawHorizontalScrollbar: function () {
            var opt = this.options;
            var intMap = opt.interactiveMap;
            var iex, iey, iew, ieh;
            var canvasHeight = opt.canvas.height;
            var canvasWidth = opt.canvas.width / 2;
            var scrollShift = opt.ObjScrollShift;
            var areaWidth = opt.objs.length * opt.PSABarHeight + 8;
            if (canvasWidth < areaWidth) {
                var ctx = opt.context;
                var sliderLeft = scrollShift * canvasWidth / areaWidth;
                var sliderWidth = canvasWidth * canvasWidth / areaWidth;
                iex = 3;
                iey = canvasHeight - 16;
                iew = canvasWidth;
                ieh = 14;
                var x1, y1, x2, y2;
                x1 = iex;
                x2 = iex + iew;
                y1 = iey;
                y2 = iey + ieh;
                var elementName = 'hscroll';
                intEl = { key: elementName, rect: [iex, iey, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
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
                x1 = iex;
                x2 = iex + iew;
                y1 = iey;
                y2 = iey + ieh;
                elementName = 'hscrollbar';
                intEl = { key: elementName, rect: [iex, iey, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
                intMap.push(intEl);
                ctx.rect(sliderLeft + 3, canvasHeight - 15, sliderWidth, 12);
                ctx.fillStyle = '#999';
                if (opt.mouseOverElement.key === 'hscrollbar') { ctx.fillStyle = '#555'; };
                if (opt.mouseOverElement.key === 'hscroll') { ctx.fillStyle = '#777'; };
                ctx.fill();
            };
            opt.interactiveMap = intMap;
        },

        _setOption: function (key, value) {
            var opt = this.options;
            this._super(key, value);
            if (key === 'objs') {
                if (opt.SelectedObjectiveIndex > opt.objs.length - 1) opt.SelectedObjectiveIndex = 0;
                opt.SelectedObjectiveIndexY = 0;
                if ((opt.viewMode === '2D') && (opt.objs.length > 1)) { opt.SelectedObjectiveIndex = 1 };
                if (opt.viewMode === 'DSA') { this._initSA() };
                this.applyObjsSorting();
            };
            if (key === 'alts') {
                if ((opt.viewMode === 'HTH') || (opt.viewMode === 'GSA') || (opt.viewMode === 'PSA') || (opt.viewMode === 'DSA') || (opt.viewMode === '2D')) {
                    this._applySAAltsFilter();
                    if (opt.viewMode === 'DSA') { this._initSA() };
                };
                //if ((opt.SAAltsSortBy == 1) || (opt.GSAShowLegend)) { opt.alts.sort(SortByValue) };
                this.applyAltsSorting();
            };
            if (key === 'ASAdata') {
                if ((opt.viewMode === 'ASA') && (typeof opt.ASAdata.alternatives !== 'undefined')) {
                    for (var i = 0; i < opt.ASAdata.alternatives.length; i++) {
                        var alt = opt.ASAdata.alternatives[i];
                        alt.newVals = alt.initVals.slice(0);
                        alt.oldVals = alt.initVals.slice(0);
                    };
                    this._calculateASAValues(true);
                    opt.ASAdata.alternatives.sort(SortByValue);
                    this.refreshASA();
                };
            };
            if (key === 'altFilterValue') {
                if ((opt.viewMode === 'ASA') && (typeof opt.ASAdata.alternatives !== 'undefined')) {
                    this._applyAltsFilter();
                };
                if ((opt.viewMode === 'HTH') || (opt.viewMode === 'GSA') || (opt.viewMode === 'PSA') || (opt.viewMode === 'DSA') || (opt.viewMode === 'DSA') || (opt.viewMode === '2D')) {
                    this._applySAAltsFilter();
                    if (opt.viewMode === 'DSA') { this._initSA() };
                };
            };
            if (key === 'ASASortBy') {
                this.applyObjsSorting();
                this.redrawSA();
            };
        },

        _setOptions: function (options) {
            this._super(options);
        },
        _applySAAltsFilter: function () {
            var opt = this.options;
            opt.visibleAlts = [];
            if (opt.altFilterValue >= -1) {
                if (opt.SAAltsSortBy == 0) { opt.alts.sort(SortByIndex) };
                if (opt.SAAltsSortBy == 1) { opt.alts.sort(SortByValue) };
                if (opt.SAAltsSortBy == 2) { opt.alts.sort(SortByName) };
                for (var i = 0; i < opt.alts.length; i++) {
                    var alt = opt.alts[i];
                    if ((i < opt.altFilterValue) || (opt.altFilterValue == -1)) {
                        alt.visible = 1;
                        opt.visibleAlts.push(alt);
                    } else {
                        alt.visible = 0
                    };
                };
                this.applyAltsSorting();
                this.applyObjsSorting();
            };
            if (opt.altFilterValue == -4 || opt.altFilterValue == -5 || opt.altFilterValue == -3) {
                for (var i = 0; i < opt.alts.length; i++) {
                    var alt = opt.alts[i];
                    if (alt.visible == 1) {
                        opt.visibleAlts.push(alt);
                    };
                };
            };
        },
        _applyAltsFilter: function () {
            if (this.options.altFilterValue >= -1) {
                for (var i = 0; i < this.options.ASAdata.alternatives.length; i++) {
                    var alt = this.options.ASAdata.alternatives[i];
                    if ((i < this.options.altFilterValue) || (this.options.altFilterValue == -1)) {
                        alt.visible = 1
                    } else {
                        alt.visible = 0
                    };
                };
            };
        },
        redrawSA: function () {
            if (((this.options.objs.length > 0) && (this.options.alts.length > 0)) || (this.options.viewMode == 'ASA')) {
                switch (this.options.viewMode) {
                    case 'DSA':
                        this.options.interactiveMap = [];
                        //if (this.options.DSASorting) { this.options.alts.sort(SortByValue) };
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
                    case 'ASA':
                        this.redrawASA();
                        break;
                };
            } else { this.showWarning() };
        },

        resetSA: function () {
            var opt = this.options;
            this._updateMinMaxAltBarValue();
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                alt.value = alt.initValue;
            };
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                obj.value = obj.initValue;
            };
            for (var i = 0; i < opt.objs.length; i++) {
                for (var j = 0; j < opt.alts.length; j++) {
                    var obj = opt.objs[i];
                    obj.gradientMinValues[j] = obj.gradientInitMinValues[j];
                };
            };
            if (opt.viewMode === 'DSA' || opt.viewMode === 'GSA' || opt.viewMode === 'PSA') {
                this.applyAltsSorting();
            };
            if (opt.viewMode === 'ASA') {
                for (var i = 0; i < opt.ASAdata.alternatives.length; i++) {
                    var alt = opt.ASAdata.alternatives[i];
                    alt.newVals = alt.initVals.slice(0);
                    alt.oldVals = alt.initVals.slice(0);
                };
                this._calculateASAValues(true);
                opt.ASAdata.alternatives.sort(SortByValue);
                opt.m_canvas_legend = null;
                opt.m_canvas_alts = null;
            };

            this.redrawSA();
        },

        _drawArrow: function (ctx, x, y, orientation, markersType) {
            var opt = this.options;
            ctx.save();
            ctx.beginPath();
            if (markersType == 0) {
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
            };
            if (markersType == 1) {
                var r = opt.DSABarHeight / 12;
                var cy = y - r - 1;
                switch (orientation) {
                    case 'left':
                        //cy += r;
                        //ctx.arc(x, cy, r, 0, 2 * Math.PI, false);
                        ctx.rect(x - r * 2 + 1, y - 1, r * 4, 2);
                        break;
                    case 'right':
                        //cy += r;
                        //ctx.arc(x + r / 2, cy, r, 0, 2 * Math.PI, false);
                        ctx.rect(x - r * 6 + 1, y - 1, r * 12, 2);
                        break;
                    case 'up':
                        //ctx.arc(x, cy, r, 0, 2 * Math.PI, false);
                        ctx.rect(x - 1, y, 2, r * 6);
                        break;
                    case 'down':
                        //ctx.arc(x, cy, r, 0, 2 * Math.PI, false);
                        ctx.rect(x - r * 2 + 1, y - 1, 2, r * 4);
                        break;
                };
            };
            ctx.fill();
            ctx.restore();
        },

        redrawPSA: function () {
            var intMap = [];
            var iex, iey, iew, ieh;
            var opt = this.options;
            //if (opt.isMobile) { opt.PSABarHeight = 52 } else { opt.PSABarHeight = 64 * 1.5 };
            opt.PSABarHeight = 52;
            var ctx = opt.context;
            ctx.fillStyle = opt.backgroundColor;
            ctx.fillRect(0, 0, opt.canvas.width, opt.canvas.height);

            ctx.beginPath();
            ctx.font = opt.DSATitleFont;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'top';
            ctx.fillStyle = "black";

            var barWidth = opt.canvas.height - 31;
            var minAltVal = opt.MinAltValue;
            var maxAltVal = opt.MaxAltValue;
            var normCoef = (maxAltVal - minAltVal);

            this._calcualteNormalizedValues();
            this._calculateOverallMinMaxValues();
            var maxAltValNorm = opt.maxNormAltValue;
            var minAltValNorm = opt.minNormAltValue;
            var normCoefNorm = (maxAltValNorm - minAltValNorm);

            var altsCountFilter = opt.alts.length;

            ctx.font = opt.DSATitleFont;
            var maxAltTextWidth = 10;
            var percentStr = (100.0).toFixed(opt.valDigits) + '%';
            for (var j = 0; j < altsCountFilter; j++) {
                var alt = opt.alts[j];
                if (alt.visible == 1) {
                    var altTextWidth = ctx.measureText(alt.name + percentStr + percentStr).width + 50;
                    if (altTextWidth > maxAltTextWidth) { maxAltTextWidth = altTextWidth };
                };
            };

            if (opt.canvas.width - opt.canvasHalfWidth > maxAltTextWidth) { opt.canvasHalfWidth = opt.canvas.width - maxAltTextWidth };

            iex = opt.canvasHalfWidth;
            iey = 0;
            iew = opt.canvasHalfWidth;
            ieh = opt.canvas.height;
            var x1, y1, x2, y2;
            x1 = iex;
            x2 = iex + iew;
            y1 = iey;
            y2 = iey + ieh;
            var elementName = 'legend';
            intEl = { key: elementName, rect: [iex, iey, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
            intMap.push(intEl);

            var x, y, w, h;
            var yStep = (barWidth - 1) / 10;
            var yValStep = 0.05;
            var scaleMaxVal = maxAltVal;
            if ((opt.normalizationMode == 'normAll')||(opt.normalizationMode == 'normSelected')) { scaleMaxVal = maxAltValNorm };
            yStep = (barWidth - 1) * yValStep / scaleMaxVal;
            ctx.lineWidth = 0.5;
            ctx.strokeStyle = 'black';
            ctx.fillStyle = 'black';
            ctx.font = opt.DSATitleFont;
            var textWidth = ctx.measureText(percentStr).width + 10;
            var PSAHalfBarHeight = opt.PSABarHeight / 2;
            x = opt.canvasHalfWidth;
            var marginTop = 22;
            ctx.textAlign = 'left';
            ctx.textBaseline = 'bottom';
            var prevY = (barWidth - 1) * 2;
            for (var i = 0; i <= 100; i++) {
                y = (barWidth - 1) - yStep * i + marginTop;
                if (y > 12) {
                    if ((prevY - y) > 48) {
                        ctx.beginPath();
                        ctx.moveTo(x + 5, y);
                        ctx.lineTo(x + 10, y);
                        ctx.stroke();
                        ctx.beginPath();
                        ctx.fillText((((yValStep * i) * 100).toFixed(0)) + '%', x + 10, y + 5);
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
            //ctx.fillText((scaleMaxVal * 100).toFixed(opt.valDigits) + '%', x + 10, y + 5);

            ctx.moveTo(x + 5, marginTop);
            ctx.lineTo(x + 5, barWidth + marginTop);
            ctx.stroke();
            var altNormCoef = this._getAltNormCoef();
            var maxy = y;
            //opt.alts.sort(SortByValue);
            this.applyAltsSorting();
            var isExpand = ((opt.objs.length * opt.PSABarHeight) < opt.canvasHalfWidth);
            var expandWidth = opt.PSABarHeight;
            opt.PSAObjExpand = isExpand;
            if (isExpand) { expandWidth = opt.canvasHalfWidth / opt.objs.length };
            if (opt.showRadar) {
                var l = barWidth / 2;
                var astep = this._getAngleStep();
                ctx.save();
                ctx.translate(0, marginTop);
                for (var i = 0; i < opt.objs.length; i++) {
                    obj = opt.objs[i];
                    maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                    if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) { normChartCoef = (1 / opt.objGradientSum[i] / maxAltValNorm) };
                    y = l - Math.cos(astep * i) * l;
                    x = opt.canvasHalfWidth / 2 + Math.sin(astep * i) * l;
                    ctx.lineWidth = 0.5;
                    ctx.strokeStyle = 'black';
                    ctx.beginPath();
                    ctx.moveTo(opt.canvasHalfWidth / 2, l);
                    ctx.lineTo(x, y);
                    ctx.stroke();

                    //var xmin = opt.canvasHalfWidth / 2 + Math.sin(astep * i - astep / 2) * l;
                    //var xmax = opt.canvasHalfWidth / 2 + Math.sin(astep * i + astep / 2) * l;
                    //var ymin = l - Math.cos(astep * i - astep / 2) * l;
                    //var ymax = l - Math.cos(astep * i + astep / 2) * l;

                    //ctx.strokeStyle = 'red';
                    //ctx.beginPath();
                    //ctx.moveTo(opt.canvasHalfWidth / 2, l);
                    //ctx.lineTo(xmin, ymin);
                    //ctx.lineTo(xmax, ymax);
                    //ctx.lineTo(opt.canvasHalfWidth / 2, l);
                    //ctx.stroke();

                    if (opt.SelectedObjectiveIndex === i) { ctx.strokeStyle = opt.DSAObjBarColorSelected; ctx.lineWidth = 12; } else { ctx.strokeStyle = opt.DSAObjBarColorTransp; ctx.lineWidth = 10; };
                    ctx.beginPath();
                    ctx.moveTo(opt.canvasHalfWidth / 2 + Math.sin(astep * i) * 10, l - Math.cos(astep * i) * 10);
                    ctx.lineTo(opt.canvasHalfWidth / 2 + Math.sin(astep * i) * (l * obj.value + 10), l - Math.cos(astep * i) * (l * obj.value + 10));
                    ctx.stroke();

                    var numTxt = ' (' + (obj.value * 100).toFixed(opt.valDigits) + '%)';
                    var txt = obj.name;
                    var numTxtW = ctx.measureText(numTxt).width;

                    var minW1 = x * 2 - numTxtW;
                    var minW2 = (opt.canvasHalfWidth - x) * 2 - numTxtW;
                    var minW = (minW2 < minW1 ? minW2 : minW1);
                    var truncName = getTruncatedString(ctx, txt, minW) + numTxt;
                    ctx.beginPath();
                    ctx.textAlign = 'center';
                    ctx.beginPath();
                    ctx.fillStyle = 'rgba(255,255,255,0.8)';
                    minW = ctx.measureText(truncName).width + 10;
                    if ((astep * i > Math.PI / 2) && (astep * i < Math.PI + Math.PI / 2)) {
                        ctx.textBaseline = 'top';
                        ctx.fillRect(x - minW / 2, y - 2, minW, 18);
                    } else {
                        ctx.textBaseline = 'bottom';
                        ctx.fillRect(x - minW / 2, y - 15, minW, 18);
                    };

                    ctx.fillStyle = '#000';
                    ctx.fillText(truncName, x, y);
                };
                ctx.restore();
            } else {
                for (var i = 0; i < opt.objs.length; i++) {
                    var objColor = opt.DSAObjBarColor;
                    var obj = opt.objs[i];
                    objColor = obj.color;
                    this._drawPSABar(i, obj.name, obj.value, obj.initValue, objColor, opt.SelectedObjectiveIndex === i, opt.ObjScrollShift, expandWidth, intMap);
                };
                this._drawHorizontalScrollbar();
            };
            for (var j = 0; j < altsCountFilter; j++) {
                var alt = opt.alts[j];
                if (alt.visible == 1) {
                    var obj = opt.objs[0];
                    var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                    var normChartCoef = 1 / scaleMaxVal;
                    if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) { normChartCoef = (1 / opt.objGradientSum[0] / maxAltValNorm); normChartCoef = 1; };
                    y = (barWidth - 1) * maxVal * normChartCoef - marginTop + 1;
                    x = -opt.ObjScrollShift - 2;
                    var x0, y0, x1, y1;
                    if (opt.showRadar) {
                        ctx.save();
                        ctx.translate(0, marginTop);
                        ctx.beginPath();
                        //ctx.strokeStyle = ColorPalette[alt.idx % ColorPalette.length];
                        ctx.strokeStyle = alt.color;
                        ctx.fillStyle = ctx.strokeStyle;
                        ctx.lineWidth = 1;
                        var l = barWidth / 2;
                        var astep = this._getAngleStep();
                        ctx.beginPath();
                        if (opt.PSAShowLines) {
                            ctx.moveTo(opt.canvasHalfWidth / 2, l - y / 2 - 5);
                        } else {
                            this._drawArrow(ctx, opt.canvasHalfWidth / 2 - 4, l - y / 2 - 5, 'right', opt.markersType);
                        };
                        for (var i = 1; i < opt.objs.length; i++) {
                            obj = opt.objs[i];
                            maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                            if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) { normChartCoef = (1 / opt.objGradientSum[i] / maxAltValNorm) };
                            var ya = l - Math.cos(astep * i) * l * maxVal * normChartCoef;
                            var xa = opt.canvasHalfWidth / 2 + Math.sin(astep * i) * l * maxVal * normChartCoef;
                            if (opt.PSAShowLines) {
                                ctx.lineTo(xa, ya);
                            } else {
                                ctx.save();
                                ctx.translate(xa, ya);
                                ctx.rotate(astep  * i);
                                this._drawArrow(ctx, -4, 0, 'right', opt.markersType);
                                ctx.restore();
                            };

                        };
                        if (opt.PSAShowLines) {
                            ctx.lineTo(opt.canvasHalfWidth / 2, l - y / 2 - 5);
                            ctx.stroke();
                        };

                        ctx.restore();
                    } else {
                        ctx.beginPath();
                        if (!opt.isMobile) { x += PSAHalfBarHeight } else { x += 4 };
                        //ctx.fillStyle = ColorPalette[alt.idx % ColorPalette.length];
                        ctx.fillStyle = alt.color;
                        if (opt.PSAShowLines) {
                            x0 = x + PSAHalfBarHeight / 2 + 1;
                            y0 = barWidth - y + 2;
                            x1 = x0;
                            y1 = y0;
                            //ctx.beginPath();
                            //ctx.moveTo(x, barWidth - y);
                            //ctx.lineTo(x + PSAHalfBarHeight, barWidth - y);         
                            //ctx.strokeStyle = ColorPalette[alt.idx % ColorPalette.length];
                        } else {
                            this._drawArrow(ctx, x + PSAHalfBarHeight / 2, barWidth - y, 'right', opt.markersType);
                        };

                        for (var i = 1; i < opt.objs.length; i++) {
                            obj = opt.objs[i];
                            maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                            //normChartCoef = 1;
                            if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) { normChartCoef = (1 / opt.objGradientSum[i] / maxAltValNorm); normChartCoef = 1; };
                            y = (barWidth - 1) * maxVal * normChartCoef - marginTop + 1;
                            x = i * expandWidth - opt.ObjScrollShift - 2;
                            if (!opt.isMobile) { x += PSAHalfBarHeight } else { x += 4 };
                            if (x < opt.canvasHalfWidth - PSAHalfBarHeight - 2) {
                                if (opt.PSAShowLines) {
                                    //ctx.lineTo(x, barWidth - y);
                                    //ctx.lineTo(x + PSAHalfBarHeight, barWidth - y);
                                    x1 = x + PSAHalfBarHeight / 2 + 1;
                                    y1 = barWidth - y;
                                    //var grad = ctx.createLinearGradient(x0, y0, x1, y1);
                                    //grad.addColorStop(0, alt.color);
                                    //grad.addColorStop(0.3, alt.color);
                                    //grad.addColorStop(0.4, opt.backgroundColor);
                                    //grad.addColorStop(0.5, opt.backgroundColor);
                                    //grad.addColorStop(0.7, alt.color);
                                    //grad.addColorStop(1, alt.color);

                                    //ctx.strokeStyle = grad;
                                    ctx.strokeStyle = alt.color;
                                    ctx.setLineDash([5, 3]);
                                    ctx.lineWidth = 1;
                                    ctx.beginPath();
                                    ctx.moveTo(x0, y0);
                                    ctx.lineTo(x1, y1);
                                    ctx.stroke();
                                    ctx.setLineDash([1, 0]);
                                    x0 = x1;
                                    y0 = y1;
                                } else {
                                    this._drawArrow(ctx, x + PSAHalfBarHeight / 2, barWidth - y, 'right', opt.markersType);
                                };
                                opt.PSAAllowScroll = false;
                            } else { opt.PSAAllowScroll = true };
                        };
                    };

                    //normChartCoef = 1;
                    if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) { normChartCoef = (1 / altNormCoef / maxAltValNorm) };
                    y = barWidth - ((barWidth - 1) * alt.value * normChartCoef - marginTop + 1);
                    x = opt.canvasHalfWidth + 4;
                    if (!opt.showRadar) {
                        ctx.setLineDash([5, 3]);
                        if (opt.PSAShowLines) {
                            ctx.lineTo(x - 10, y);
                            ctx.lineTo(x, y);
                            ctx.stroke();
                        } else {
                            this._drawArrow(ctx, x, y, 'right', opt.markersType);
                        };
                    };
                    //x = x + textWidth;
                    x = opt.canvasHalfWidth + textWidth;
                    w = opt.canvas.width - x - 24;
                    //ctx.strokeStyle = ColorPalette[alt.idx % ColorPalette.length];
                    ctx.strokeStyle = alt.color;
                    ctx.fillStyle = ctx.strokeStyle;
                    ctx.lineWidth = 1;
                    ctx.beginPath();
                    ctx.moveTo(opt.canvasHalfWidth, y);
                    ctx.lineTo(opt.canvasHalfWidth + 5, y);
                    ctx.stroke();
                    ctx.beginPath();
                    ctx.moveTo(x - 20, y);
                    ctx.lineTo(x - 15, y);
                    var valy = y;
                    if (!opt.PSALineup) {
                        var yLegendStep = barWidth / (altsCountFilter - 1);
                        if (yLegendStep < 24) {
                            yLegendStep = 24;
                        } else {
                            //opt.AltScrollShift = 0
                        };
                        y = 12 + j * yLegendStep;
                        ctx.lineTo(x, y - opt.AltScrollShift);
                        ctx.lineTo(x + 5, y - opt.AltScrollShift);
                    };

                    ctx.stroke();
                    ctx.setLineDash([1, 0]);
                    if ((y - opt.AltScrollShift < opt.canvas.height) && (y - opt.AltScrollShift > 0)) {
                        if (!opt.PSALineup) {
                            this._drawArrow(ctx, x + 10, y - opt.AltScrollShift, 'right', 0);
                            this._drawAltLegendLabel(ctx, alt, x + 11, y - opt.AltScrollShift, w, opt.legendLabelHeight, altNormCoef, true);
                        } else {
                            //this._drawAltLegendLabel(ctx, alt, x - 15, y, w + 35, 22, altNormCoef);
                            if (y < maxy) y = maxy;
                            ctx.beginPath();
                            ctx.moveTo(x - 15, valy);
                            ctx.lineTo(x, y - opt.AltScrollShift);
                            ctx.lineTo(x + 10, y - opt.AltScrollShift);
                            ctx.stroke();
                            this._drawArrow(ctx, x + 10, y - opt.AltScrollShift, 'right', 0);
                            this._drawAltLegendLabel(ctx, alt, x + 11, y - opt.AltScrollShift, w, opt.legendLabelHeight, altNormCoef, true);
                            maxy = y + opt.legendLabelHeight;
                        };
                        var elementName = 'alt:' + alt.id;
                        iex = x + 10;
                        iey = y - 10;
                        iew = 20;
                        ieh = 20;
                        var x1, y1, x2, y2;
                        x1 = iex;
                        x2 = iex + iew;
                        y1 = iey;
                        y2 = iey + ieh;
                        var intEl = { key: elementName, rect: [x1, y1, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
                        intMap.push(intEl);
                    };
                    
                };
            };
            
            ctx.fillText(opt.titleObjectives, opt.canvasHalfWidth / 2 + 30, 6);
            ctx.fillText(opt.titleAlternatives, opt.canvasHalfWidth + 150, 6);

            if (!opt.PSALineup || true) {
                this._drawScrollbar(false);
            };

            if (!opt.showRadar) {
                var r = PSAHalfBarHeight / 4;
                for (var j = 0; j < altsCountFilter; j++) {
                    var alt = opt.alts[j];
                    if (alt.visible == 1) {
                        var obj = opt.objs[0];
                        var maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                        var normChartCoef = 1 / scaleMaxVal;
                        if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) { normChartCoef = (1 / opt.objGradientSum[0] / maxAltValNorm); normChartCoef = 1; };
                        y = (barWidth - 1) * maxVal * normChartCoef - marginTop + 1;
                        x = -opt.ObjScrollShift - 2;
                        ctx.beginPath();
                        if (!opt.isMobile) { x += PSAHalfBarHeight } else { x += 4 };
                        ctx.fillStyle = alt.color;
                        if (opt.PSAShowLines) {
                            ctx.beginPath();
                            ctx.fillStyle = alt.color;
                            ctx.arc(x + PSAHalfBarHeight / 2 + 1, barWidth - y, r, 0, 2 * Math.PI, false);
                            ctx.fill();
                        };

                        for (var i = 1; i < opt.objs.length; i++) {
                            obj = opt.objs[i];
                            maxVal = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                            if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) { normChartCoef = (1 / opt.objGradientSum[i] / maxAltValNorm); normChartCoef = 1; };
                            y = (barWidth - 1) * maxVal * normChartCoef - marginTop + 1;
                            x = i * expandWidth - opt.ObjScrollShift - 2;
                            if (!opt.isMobile) { x += PSAHalfBarHeight } else { x += 4 };
                            if (x < opt.canvasHalfWidth - PSAHalfBarHeight - 2) {
                                if (opt.PSAShowLines) {
                                    ctx.beginPath();
                                    ctx.fillStyle = alt.color;
                                    ctx.arc(x + PSAHalfBarHeight / 2 + 1, barWidth - y, r, 0, 2 * Math.PI, false);
                                    ctx.fill();
                                };
                            };
                        };
                    };

                };
            };

            opt.interactiveMap = intMap;
        },

        _drawAltLegendLabel: function (ctx, alt, x, y, w, h, normCoef, showValue) {
            var opt = this.options;
            var h2 = h / 2;
            ctx.save();
            var intMap = opt.interactiveMap;
            var iex, iey, iew, ieh, x1, y1, x2, y2, elementName;

            ctx.font = '12px Arial';
            
            if ((typeof opt.mouseOverElement.key != "undefined") && (opt.mouseOverElement.key.indexOf('_' + alt.id + '_') != -1)) {
                ctx.font = 'bold 12px Arial';
            };
            var altValueTextWidth = h2;
            ctx.beginPath();
            //ctx.fillStyle = ColorPalette[alt.idx % ColorPalette.length];
            var labelShift = 0;
            if (opt.viewMode !== "PSA") {
                labelShift = 5;
                ctx.fillStyle = alt.color;
                ctx.arc(x + 5, y, h2 - 2, 0, 2 * Math.PI, false);
                ctx.fill();
            };
            if (opt.viewMode === 'ASA') {
                var altKey = 'alt' + opt.ASAdata.objectives.length + '_' + alt.id + '_' + 'legend';
                x1 = opt.GSAmarginLeft + opt.ASAChartWidth + x;
                y1 = y - h2;
                x2 = x1 + altValueTextWidth;
                y2 = y1 + h;
                elementName = 'altLegend';
                intEl = { key: altKey, rect: [x1, y1, altValueTextWidth, h], element: [elementName, opt.ASAdata.objectives.length, alt.id], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
                intMap.push(intEl);
            };


            ctx.beginPath();
            ctx.fillStyle = "#000";
            ctx.textBaseline = 'middle';
            var aname = (showValue ? (alt.value * 100 / normCoef).toFixed(opt.valDigits) + '%  ' : '') + alt.name;
            var truncName = getTruncatedString(ctx, aname, w - altValueTextWidth);
            ctx.textAlign = 'left';
            ctx.fillText(aname, x + labelShift + altValueTextWidth, y);

            ctx.textAlign = 'right';
            opt.interactiveMap = intMap;
        },

        _calcualteNormalizedValues: function () {
            var opt = this.options;
            opt.minNormAltValue = 1;
            opt.maxNormAltValue = 0;
            var obj = opt.objs[opt.SelectedObjectiveIndex];
            var minValues = [];
            var maxValues = [];
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                if (((opt.normalizationMode == 'normSelected') && (alt.visible == 1)) || (opt.normalizationMode !== 'normSelected')) {
                    if ((obj) && (obj.gradientMinValues)) minValues.push(this.getGradientValueByAltID(obj.gradientMinValues, alt.id));
                    if ((obj) && (obj.gradientMaxValues)) maxValues.push(this.getGradientValueByAltID(obj.gradientMaxValues, alt.id));
                };
            };
            var minSum = 0;
            var maxSum = 0;
            for (var i = 0; i < minValues.length; i++) {
                minSum += minValues[i];
                maxSum += maxValues[i];
            };
            opt.objMinNormValues = [];
            opt.objMaxNormValues = [];
            for (var i = 0; i < minValues.length; i++) {
                opt.objMinNormValues.push(minValues[i] / minSum);
                opt.objMaxNormValues.push(maxValues[i] / maxSum);
            };

            for (var i = 0; i < opt.objMinNormValues.length; i++) {
                if (opt.minNormAltValue > opt.objMinNormValues[i]) { opt.minNormAltValue = opt.objMinNormValues[i] };
                if (opt.maxNormAltValue < opt.objMinNormValues[i]) { opt.maxNormAltValue = opt.objMinNormValues[i] };
                if (opt.minNormAltValue > opt.objMaxNormValues[i]) { opt.minNormAltValue = opt.objMaxNormValues[i] };
                if (opt.maxNormAltValue < opt.objMaxNormValues[i]) { opt.maxNormAltValue = opt.objMaxNormValues[i] };
            };
        },
        _calculateOverallMinMaxValues: function () {
            var opt = this.options;
            opt.minNormAltValue = 1;
            opt.maxNormAltValue = 0;
            for (var j = 0; j < opt.objs.length; j++) {
                var obj = opt.objs[j];
                var minValues = [];
                var maxValues = [];
                for (var i = 0; i < opt.alts.length; i++) {
                    var alt = opt.alts[i];
                    if (((opt.normalizationMode == 'normSelected') && (alt.visible == 1)) || (opt.normalizationMode !== 'normSelected')) {
                        minValues.push(this.getGradientValueByAltID(obj.gradientMinValues, alt.id));
                        maxValues.push(this.getGradientValueByAltID(obj.gradientMaxValues, alt.id));
                    };
                };
                var minSum = 0;
                var maxSum = 0;
                for (var i = 0; i < minValues.length; i++) {
                    minSum += minValues[i];
                    maxSum += maxValues[i];
                };
                var objMinNormValues = [];
                var objMaxNormValues = [];
                for (var i = 0; i < minValues.length; i++) {
                    objMinNormValues.push(minValues[i] / minSum);
                    objMaxNormValues.push(maxValues[i] / maxSum);
                };

                for (var i = 0; i < objMinNormValues.length; i++) {
                    if (opt.minNormAltValue > objMinNormValues[i]) { opt.minNormAltValue = objMinNormValues[i] };
                    if (opt.maxNormAltValue < objMinNormValues[i]) { opt.maxNormAltValue = objMinNormValues[i] };
                    if (opt.minNormAltValue > objMaxNormValues[i]) { opt.minNormAltValue = objMaxNormValues[i] };
                    if (opt.maxNormAltValue < objMaxNormValues[i]) { opt.maxNormAltValue = objMaxNormValues[i] };
                };
            };
        },
        redrawGSA: function () {
            var opt = this.options;
            var marginLeft = 10;
            var marginTop = 10;
            var marginBottom = 60;
            var btnSize = opt.buttonSize;
            var intMap = [];
            var iex, iey, iew, ieh, btnMode, x1, y1, x2, y2, elementName;
            opt.GSAmarginRight = 10;
            var id = opt.canvas.id;
            var optionsList = '';
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                optionsList += "<option value='" + i + "'>" + obj.name + "</option>";
            };
            $('#' + id + 'Options').html("<select id='" + id + "objx' class='select' style='width: 100px'>" + optionsList + "</select>");
            $('#' + id + 'objx').val(opt.SelectedObjectiveIndex);
            var scope = this;
            $('#' + id + 'objx').change(function () {
                scope.options.SelectedObjectiveIndex = parseInt($('#' + id + 'objx').val());
                scope.redrawSA();
                if (opt.isMultiView) scope.syncSACharts(false);
            });
            var ctx = opt.context;
            if (opt.canvas.width <= opt.canvas.height) {
                opt.legendPosition = 1;
            };
            ctx.font = opt.DSATitleFont;
            var maxPrtyText = (1000.0).toFixed(opt.valDigits) + '%';
            var altsCountFilter = opt.alts.length;
            if (opt.GSAShowLegend) {
                var maxWidth = 10;
                if (opt.legendPosition == 0) {
                    for (var i = 0; i < altsCountFilter; i++) {
                        var alt = opt.alts[i];
                        if (alt.visible == 1) {
                            var altWidth = ctx.measureText(alt.name + maxPrtyText).width + 15;
                            if ((maxWidth < altWidth) && (altWidth < opt.canvas.width / 2)) {
                                maxWidth = altWidth
                            };
                        };
                    };
                    opt.GSAmarginRight = maxWidth;
                } else {
                    marginBottom = opt.canvas.height / 3;
                };
            };
            var marginRight = opt.GSAmarginRight;

            //marginLeft = ctx.measureText(maxPrtyText).width;

            var MaxAltValue = opt.MaxAltValue;
            var MinAltValue = opt.MinAltValue;
            if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) {
                this._calcualteNormalizedValues();
                this._calculateOverallMinMaxValues();
                MaxAltValue = opt.maxNormAltValue;
                MinAltValue = opt.minNormAltValue;
            };
            var normCoef = (MaxAltValue - MinAltValue);
            ctx.beginPath();
            ctx.fillStyle = opt.backgroundColor;
            ctx.fillRect(0, 0, opt.canvas.width, opt.canvas.height);

            ctx.beginPath();
            ctx.fillStyle = '#ffffff';
            var chartHeight = opt.canvas.height - (marginTop + marginBottom);
            var yTicks = 10;
            if (chartHeight < 200) { yTicks = 5 };
            var yStep = chartHeight / yTicks;
            var yValStep = normCoef / yTicks;
            
            for (var i = 0; i <= yTicks; i++) {
                var margin = ctx.measureText(numberToStr((MaxAltValue - yValStep * i) * 100, opt.valDigits) + '%').width;
                if (marginLeft < margin) { marginLeft = margin};
            };

            marginLeft += 10;

            var chartWidth = opt.canvas.width - (marginLeft + marginRight);
            ctx.fillRect(marginLeft, marginTop, chartWidth, chartHeight);
            var xTicks = 10;      
            if (chartWidth < 300) { xTicks = 5 };
            var xStep = chartWidth / xTicks;           
            var xValStep = 1 / xTicks;
            ctx.lineWidth = 0.5;
            ctx.strokeStyle = 'rgba(0,0,0,0.5)';
            ctx.fillStyle = 'black';
            elementName = 'objX';
            x1 = marginLeft;
            y1 = marginTop;
            x2 = x1 + chartWidth;
            y2 = y1 + chartHeight;
            intEl = { key: elementName, rect: [x1, y1, chartWidth, chartHeight], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
            intMap.push(intEl);
            elementName = 'legend';
            x1 = opt.canvas.width - marginRight;
            y1 = 0;
            x2 = x1 + marginRight;
            y2 = y1 + opt.canvas.height;
            intEl = { key: elementName, rect: [x1, y1, marginRight, opt.canvas.height], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
            intMap.push(intEl);

            for (var i = 0; i <= xTicks; i++) {
                var x = xStep * i + marginLeft;
                ctx.beginPath();
                ctx.moveTo(x, marginTop);
                ctx.lineTo(x, opt.canvas.height - marginBottom + 5);
                ctx.stroke();
                ctx.beginPath();
                ctx.textAlign = 'center';
                ctx.textBaseline = 'top';
                ctx.fillText((i * xValStep * 100).toFixed() + '%', x - 10, opt.canvas.height - marginBottom + 10);
            };
            for (var i = 0; i <= yTicks; i++) {
                var y = yStep * i + marginTop;
                ctx.beginPath();
                ctx.moveTo(marginLeft - 5, y);
                ctx.lineTo(opt.canvas.width - marginRight, y);
                ctx.stroke();
                ctx.beginPath();
                ctx.textAlign = 'right';
                ctx.textBaseline = 'bottom';
                ctx.fillText(numberToStr((MaxAltValue - yValStep * i) * 100, opt.valDigits) + '%', marginLeft - 7, y + 5);
            };

            if ((typeof opt.SelectedObjectiveIndex == 'undefined') || (opt.SelectedObjectiveIndex >= opt.objs.length)) opt.SelectedObjectiveIndex = opt.objs.length - 1;
            var objValue = opt.objs[opt.SelectedObjectiveIndex].value;
            var selectedObjName = opt.objs[opt.SelectedObjectiveIndex].name + ' = ' + (objValue * 100).toFixed(opt.valDigits) + '%';
            ctx.beginPath();
            ctx.textAlign = 'center';
            ctx.fillStyle = 'blue';
            ctx.textBaseline = 'top';
            ctx.fillText(selectedObjName, marginLeft + (chartWidth / 2) - btnSize, marginTop + chartHeight + 40);
            var xcenter = marginLeft + (chartWidth / 2) - (opt.objs.length * 10 / 2) - btnSize;
            for (var i = 0; i < opt.objs.length; i++) {
                ctx.beginPath();
                ctx.arc(i * 10 + xcenter, marginTop + chartHeight + 32, 3, 0, 2 * Math.PI, false);
                if (i === opt.SelectedObjectiveIndex) { ctx.fillStyle = 'blue' } else { ctx.fillStyle = '#eee' };
                ctx.fill();
            };
            var xbtm = marginTop + chartHeight + btnSize + 30;
            var xw = opt.canvas.width;
            if (opt.SelectedObjectiveIndex < opt.objs.length - 1 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'next') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iex = opt.canvas.width - 30 - marginRight;
                iey = xbtm - btnSize - 5;
                x1 = iex;
                y1 = iey;
                x2 = x1 + btnSize;
                y2 = y1 + btnSize;
                elementName = 'next';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Next Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 0);
            };

            if (opt.SelectedObjectiveIndex !== 0 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'prev') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iex = opt.canvas.width - 65 - marginRight;
                iey = xbtm - btnSize - 5;
                x1 = iex;
                y1 = iey;
                x2 = x1 + btnSize;
                y2 = y1 + btnSize;
                elementName = 'prev';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Previous Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 180);
            };

            var x1 = marginLeft;
            var x2 = marginLeft + chartWidth;
            var altNormCoef = this._getAltNormCoef();
            //if (opt.normalizationMode = 'normAll') {normCoef = altNormCoef}

            var obj = opt.objs[opt.SelectedObjectiveIndex];
            var vi = 0;
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                if (alt.visible == 1) {
                    var gradientMinValue = this.getGradientValueByAltID(obj.gradientMinValues, alt.id);
                    var gradientMaxValue = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);
                    if (opt.normalizationMode == 'normSelected') {
                        gradientMinValue = opt.objMinNormValues[vi];
                        gradientMaxValue = opt.objMaxNormValues[vi];
                    };
                    if (opt.normalizationMode == 'normAll') {
                        gradientMinValue = opt.objMinNormValues[i];
                        gradientMaxValue = opt.objMaxNormValues[i];
                    };
                    ctx.beginPath();
                    ctx.lineWidth = 2;
                    var y1 = marginTop + chartHeight - ((gradientMinValue - MinAltValue) / normCoef * chartHeight);
                    var y2 = marginTop + chartHeight - ((gradientMaxValue - MinAltValue) / normCoef * chartHeight);
                    ctx.moveTo(x1, y1);
                    ctx.lineTo(x2, y2);
                    //ctx.strokeStyle = ColorPalette[alt.idx % ColorPalette.length];
                    ctx.strokeStyle = alt.color;
                    ctx.stroke();
                    vi++;
                };
            };

            var objInitValue = opt.objs[opt.SelectedObjectiveIndex].initValue;
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

            if (opt.GSAShowLegend) {
                var idx = 0;
                var xLeft = 0;
                var xColumn = 0;
                var maxLabelWidth = 10;
                var labelH = 15;
                for (var i = 0; i < altsCountFilter; i++) {
                    var alt = opt.alts[i];
                    if (alt.visible == 1) {
                        if (opt.legendPosition == 0) {
                            var yTop = idx * (labelH+2) + labelH - opt.AltScrollShift;
                            if (yTop > 0) {
                                iex = opt.canvas.width - marginRight + 10;
                                iey = yTop;
                                iew = labelH;
                                ieh = labelH;
                                this._drawAltLegendLabel(ctx, alt, iex, yTop, 265, labelH, altNormCoef, true);
                            };
                            if ((yTop) > (opt.canvas.height - marginTop)) { break };
                        } else {
                            var yTop = marginTop + chartHeight + idx * (labelH+2) + labelH * 4;
                            if (yTop > marginTop + chartHeight) {
                                iex = 4 + xLeft;
                                iey = yTop;
                                iew = labelH;
                                ieh = labelH;
                                this._drawAltLegendLabel(ctx, alt, iex, yTop, chartWidth, labelH, altNormCoef, true);
                            };
                            var mlWidth = ctx.measureText(alt.name + '0000' + maxPrtyText).width;
                            if (maxLabelWidth < mlWidth) { maxLabelWidth = mlWidth };
                            if ((yTop) > (opt.canvas.height - labelH*1.5)) { idx = -1; xLeft += maxLabelWidth; maxLabelWidth = 10; };
                        };
                        var elementName = 'alt:' + alt.id;
                        var x1, y1, x2, y2;
                        iex = iex - 7;
                        iey = iey - 7;
                        x1 = iex;
                        x2 = iex + iew;
                        y1 = iey;
                        y2 = iey + ieh;
                        var intEl = { key: elementName, rect: [x1, y1, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
                        intMap.push(intEl);
                        idx++;
                    };
                }
                if (opt.legendPosition == 0) {
                    this._drawScrollbar(false)
                };
            };
            opt.interactiveMap = intMap;
        },
        _calculateASAValues: function (isInitValues) {
            var opt = this.options;
            for (var j = 0; j < opt.ASAdata.alternatives.length; j++) {
                var altASA = opt.ASAdata.alternatives[j];
                var altVal = 0;
                for (var i = 0; i < opt.ASAdata.objectives.length; i++) {
                    var obj = opt.ASAdata.objectives[i];
                    altVal += altASA.newVals[obj.idx] * obj.prty;
                };
                altASA.value = altVal;
                if (isInitValues) { altASA.initValue = altVal };
            };
        },

        _getNextVisibleObjIndex: function (idx) {
            for (var i = idx; i < this.options.ASAdata.objectives.length; i++) {
                var obj = this.options.ASAdata.objectives[i];
                if ((obj.visible === 1)&&(this.options.ASAPageObjs.indexOf(obj) >= 0)) { return i; };
            };
            return -1;
        },

        _drawASAAltLine: function (ctx, altIdx, objIdx, isNewVals, bw) {
            var opt = this.options;
            var maxBallSize = opt.ASAmaxBallSize;
            var marginTop = opt.GSAmarginTop;
            var altASA = opt.ASAdata.alternatives[altIdx];
            var chartHeight = opt.ASAChartHeight;
            var marginRight = opt.GSAmarginRight;
            var marginLeft = opt.GSAmarginLeft;
            var chartWidth = opt.canvas.width - (marginLeft + marginRight);
            if (chartWidth < 50) chartWidth = 50;
            var ieh = bw;
            if (ieh > maxBallSize) { ieh = maxBallSize };
            var iew = ieh / 2;
            var altMaxVal = opt.ASAdata.maxAltValue;

            var idx = 0;
            for (var i = 0; i < opt.ASAdata.objectives.length; i++) {
                var obj = opt.ASAdata.objectives[i];
                if (!obj || ((obj.visible === 1) && (opt.ASAPageObjs.indexOf(obj) >= 0))) {
                    ctx.fillStyle = altASA.color;
                    iex = marginLeft + idx * (bw * 2) + bw - iew;
                    var altValue = altASA.newVals[(!obj ? i : obj.idx)];
                    if (!isNewVals) { altValue = altASA.initVals[(!obj ? i : obj.idx)]; };
                    iey = marginTop + chartHeight - chartHeight * altValue / altMaxVal;
                    var nextVisIdx = this._getNextVisibleObjIndex(i + 1);
                    var r = 0.7;
                    if (nextVisIdx > i) {
                        ctx.beginPath();
                        ctx.strokeStyle = altASA.color;
                        ctx.lineWidth = 2;
                        iex1 = iex + (bw * 2);
                        var altValue1 = altASA.newVals[opt.ASAdata.objectives[nextVisIdx].idx];
                        if (!isNewVals) { altValue1 = altASA.initVals[opt.ASAdata.objectives[nextVisIdx].idx]; };
                        iey1 = marginTop + chartHeight - chartHeight * altValue1 / altMaxVal;
                        ctx.moveTo(iex + iew, iey);
                        ctx.lineTo(iex1 + iew, iey1);
                        ctx.stroke();
                    } else {
                        if (i == objIdx) {
                            r = 0.9
                        };
                        iex1 = iex + (bw * 2);
                        altValue = altASA.value;
                        if (!isNewVals) { altValue = altASA.initValue };
                        iey1 = marginTop + chartHeight - chartHeight * altValue / altMaxVal;
                        ctx.beginPath();
                        ctx.strokeStyle = altASA.color;
                        ctx.lineWidth = 2;
                        ctx.moveTo(iex + iew, iey);
                        ctx.lineTo(iex1 + iew, iey1);
                        ctx.stroke();
                        ctx.beginPath();
                        ctx.lineWidth = 2;
                        ctx.strokeStyle = '#eee';
                        ctx.arc(iex1 + iew, iey1, iew * r, 0, 2 * Math.PI, false);
                        ctx.fill();
                        ctx.stroke();
                    };

                    ctx.lineWidth = 2;
                    ctx.strokeStyle = '#eee';
                    if (i == objIdx) {
                        r = 0.9
                    };
                    ctx.beginPath();
                    ctx.arc(iex + iew, iey, iew * r, 0, 2 * Math.PI, false);
                    ctx.fill();
                    ctx.stroke();
                    idx++;
                };
            };
        },

        _updateASAMargins: function (ctx) {
            var opt = this.options;
            var MaxAltValue = opt.ASAdata.maxAltValue;
            var MinAltValue = opt.MinAltValue;
            var normCoef = (MaxAltValue - MinAltValue);
            var marginLeft = opt.GSAmarginLeft;
            var marginRight = opt.GSAmarginRight;
            var marginTop = opt.GSAmarginTop;
            var marginBottom = opt.GSAmarginDown;
            var chartHeight, chartWidth, bw, bw2;
            var maxPrtyText = '';
            if (normCoef == 1) { maxPrtyText = '100 %' } else { maxPrtyText = (100.0).toFixed(opt.valDigits) + '%' };
            marginLeft = ctx.measureText(maxPrtyText).width + 10;
            opt.GSAmarginLeft = marginLeft;
            opt.GSAmarginRight = opt.canvas.width / 2;
            var altsCountFilter = opt.ASAdata.alternatives.length;
            if ((opt.altFilterValue > -1) && (opt.altFilterValue < opt.ASAdata.alternatives.length)) {
                altsCountFilter = opt.altFilterValue
            };
            var maxAltTextWidth = 0;
            maxPrtyText = (100.0).toFixed(opt.valDigits) + '%';
            for (var j = 0; j < altsCountFilter; j++) {
                var altASA = opt.ASAdata.alternatives[j];
                var measureAltText = ctx.measureText(maxPrtyText + altASA.name).width + 35;
                if (maxAltTextWidth < measureAltText) { maxAltTextWidth = measureAltText };
            };
            if (maxAltTextWidth < opt.GSAmarginRight) { opt.GSAmarginRight = maxAltTextWidth };
            marginRight = opt.GSAmarginRight;
            chartHeight = opt.canvas.height - (marginTop + marginBottom);
            chartWidth = opt.canvas.width - (marginLeft + marginRight);
            opt.ASAChartWidth = chartWidth;
            opt.ASAChartHeight = chartHeight;
        },

        redrawASA: function () {
            var opt = this.options;
            var pagesize = opt.ASAPageSize;
            var currentpage = opt.ASACurrentPage;
            // init variables for chart
            var marginLeft, marginRight, marginTop, marginBottom;
            var maxBallSize = opt.ASAmaxBallSize;
            var chartHeight, chartWidth, bw, bw2;
            var intMap = [];
            var iex, iey, iew, ieh, btnMode, x1, x2, y1, y2, elementName;
            var altsCountFilter = opt.ASAdata.alternatives.length;
            //if ((opt.altFilterValue > -1) && (opt.altFilterValue < opt.ASAdata.alternatives.length)) {
            //    altsCountFilter = opt.altFilterValue
            //};

            var MaxAltValue = opt.ASAdata.maxAltValue;
            var MinAltValue = opt.MinAltValue;
            var normCoef = (MaxAltValue - MinAltValue);

            // calculate max objectives value
            var maxObj = 0;
            opt.ASAVisibleObjs = 0;
            opt.ASAPageObjs = [];
            for (var i = 0; i < opt.ASAdata.objectives.length; i++) {
                var obj = opt.ASAdata.objectives[i];
                if (obj.prty > maxObj) { maxObj = obj.prty };
                if (obj.visible === 1) { opt.ASAVisibleObjs++ };
            };
            if ((opt.ASAVisibleObjs < pagesize) && (opt.ASAVisibleObjs > 0)) { pagesize = opt.ASAVisibleObjs };
            opt.ASAVisibleObjs = 0;
            for (var i = 0; i < opt.ASAdata.objectives.length; i++) {
                var obj = opt.ASAdata.objectives[i];
                if (obj.visible === 1) { opt.ASAVisibleObjs++ };
                if ((obj.visible === 1) && (opt.ASAPageObjs.length < pagesize) && (opt.ASAVisibleObjs > ((currentpage - 1) * pagesize)) && (opt.ASAVisibleObjs <= (currentpage * pagesize))) { opt.ASAPageObjs.push(obj) };
            };
            // set margins and adjust font size based on content
            var ctx = opt.context;
            ctx.font = opt.ASAFont;
            this._updateASAMargins(ctx);
            chartWidth = opt.ASAChartWidth;
            bw = chartWidth / (pagesize + 1) / 2;
            if (bw < 10) {
                opt.ASAFont = '12px Arial';
                ctx.font = opt.ASAFont;
                this._updateASAMargins(ctx);
                chartWidth = opt.ASAChartWidth;
                bw = chartWidth / (pagesize + 1) / 2;
            };

            chartHeight = opt.ASAChartHeight;
            marginLeft = opt.GSAmarginLeft;
            marginRight = opt.GSAmarginLeft;
            marginTop = opt.GSAmarginTop;
            marginBottom = opt.GSAmarginDown;
            bw2 = bw / 2;

            // draw background (objectives bars and chart grid)
            if (opt.m_canvas_background == null) {
                opt.m_canvas_background = document.createElement('canvas');
                opt.m_canvas_background.width = opt.canvas.width;
                opt.m_canvas_background.height = opt.canvas.height;
                ctx = opt.m_canvas_background.getContext('2d');
                ctx.font = opt.ASAFont;

                ctx.beginPath();
                ctx.fillStyle = opt.backgroundColor;
                ctx.fillRect(0, 0, opt.canvas.width, opt.canvas.height);

                ctx.beginPath();
                ctx.fillStyle = '#ffffff';

                var yTicks = 10;
                if (chartHeight < 200) { yTicks = 5 };
                var yStep = chartHeight / yTicks;
                var yValStep = normCoef / yTicks;
                ctx.lineWidth = 0.5;
                ctx.strokeStyle = 'rgba(0,0,0,0.5)';
                ctx.fillStyle = 'black';

                for (var i = 0; i <= yTicks; i++) {
                    var y = yStep * i + marginTop;
                    ctx.beginPath();
                    ctx.moveTo(marginLeft - 5, y);
                    ctx.lineTo(opt.canvas.width - marginRight, y);
                    ctx.stroke();
                    ctx.beginPath();
                    ctx.textAlign = 'right';
                    ctx.textBaseline = 'bottom';
                    if (normCoef == 1) {
                        ctx.fillText((((MaxAltValue - yValStep * i) * 100).toFixed(0)) + '%', marginLeft - 10, y + 5);
                    } else {
                        ctx.fillText((((MaxAltValue - yValStep * i) * 100).toFixed(opt.valDigits)) + '%', marginLeft - 10, y + 5);
                    };
                    
                };

                var idx = 0;
                var allObjsDone = false;
                for (var i = 0; i <= opt.ASAdata.objectives.length; i++) {
                    var obj = opt.ASAdata.objectives[i];
                    if (!obj || ((obj.visible === 1) && (opt.ASAPageObjs.indexOf(obj) >= 0))) {
                        ctx.beginPath();
                        var title = 'Overall';
                        if (allObjsDone || !obj) {
                            ctx.fillStyle = '#333333';
                            ieh = chartHeight;
                        } else {
                            ctx.fillStyle = opt.DSAObjBarColor;
                            ieh = chartHeight * obj.prty / maxObj;
                            title = (obj.prty * 100).toFixed(opt.valDigits) + '%  ' + obj.name;
                        };
                        iex = marginLeft + idx * (bw * 2) + bw2;
                        iey = marginTop + chartHeight - ieh;
                        iew = bw;
                        ctx.fillRect(iex, iey, iew, ieh);
                        ctx.fillStyle = '#333333';
                        ctx.save();
                        ctx.beginPath();
                        ctx.rect(iex, iey, iew, ieh);
                        ctx.lineWidth = 2;
                        ctx.strokeStyle = '#eee';
                        ctx.fillStyle = 'rgba(255,255,255,0.7)';
                        ctx.fill();
                        ctx.stroke();
                        ctx.restore();
                        ctx.save();
                        ctx.rotate(-Math.PI / 2);
                        ctx.textAlign = 'left';
                        ctx.textBaseline = 'top';
                        ctx.shadowBlur = 4;
                        ctx.shadowColor = "#fff";
                        ctx.fillText(title, -chartHeight - marginTop, iex - 15);
                        ctx.restore();
                        idx++;
                    };
                    if (!obj || ((obj.visible === 1) && (opt.ASAPageObjs.indexOf(obj) == opt.ASAPageObjs.length - 1))) allObjsDone = true;
                };
            };
            ctx = opt.context;
            ctx.drawImage(opt.m_canvas_background, 0, 0);

            // draw selected objective with objective hint
            chartHeight = opt.ASAChartHeight;
            marginRight = opt.GSAmarginRight;
            chartWidth = opt.canvas.width - (marginLeft + marginRight);
            bw = chartWidth / (pagesize + 1) / 2;
            bw2 = bw / 2;
            iex = opt.canvas.width - marginRight;
            iey = 0;
            iew = marginRight;
            ieh = opt.canvas.height;
            elementName = 'legend';
            x1 = iex;
            y1 = iey;
            x2 = x1 + iew;
            y2 = y1 + ieh;
            intEl = { key: elementName, rect: [iex, iey, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
            intMap.push(intEl);
            var idx = 0;
            allObjsDone = false;
            for (var i = 0; i <= opt.ASAdata.objectives.length; i++) {
                var obj = opt.ASAdata.objectives[i];
                if (!obj || ((obj.visible === 1) && (opt.ASAPageObjs.indexOf(obj) >= 0))) {
                    var title = 'Overall';
                    if (allObjsDone || !obj) {
                        ieh = chartHeight;
                    } else {
                        ieh = chartHeight * obj.prty / maxObj;
                        title = (obj.prty * 100).toFixed(opt.valDigits) + '%  ' + obj.name;
                    };
                    iex = marginLeft + idx * (bw * 2) + bw2;
                    iey = marginTop + chartHeight - ieh;
                    iew = bw;

                    x1 = iex - bw;
                    y1 = marginTop;
                    x2 = x1 + iew + bw;
                    y2 = y1 + chartHeight;
                    elementName = 'obj';
                    intEl = { key: elementName + i, rect: [x1, y1, iew + bw, chartHeight], element: [elementName, i], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
                    intMap.push(intEl);

                    if (opt.SelectedObjectiveIndex === i) {
                        ctx.save();
                        ctx.beginPath();
                        ctx.rect(iex, iey, iew, ieh);
                        ctx.lineWidth = 2;
                        ctx.strokeStyle = '#bbb';
                        ctx.fillStyle = 'rgba(220,220,220,0.7)';
                        ctx.fill();
                        ctx.stroke();
                        ctx.restore();

                        ctx.save();
                        ctx.beginPath();
                        ctx.fillStyle = '#eeeeee';
                        ctx.moveTo(iex + bw2 - 10, marginTop + chartHeight + maxBallSize - 5);
                        ctx.lineTo(iex + bw2, marginTop + chartHeight + maxBallSize / 2 - 5);
                        ctx.lineTo(iex + bw2 + 10, marginTop + chartHeight + maxBallSize - 5);
                        ctx.fill();
                        ctx.beginPath();
                        ctx.rect(marginLeft, marginTop + chartHeight + maxBallSize - 10, chartWidth, 24);
                        ctx.fill();
                        ctx.fillStyle = '#444444';
                        ctx.textAlign = 'left';
                        ctx.textBaseline = 'top';
                        ctx.fillText(title, marginLeft + 12, marginTop + chartHeight + maxBallSize - 5);
                        ctx.restore();
                    };
                    idx++;
                };
                if (!obj || ((obj.visible === 1) && (opt.ASAPageObjs.indexOf(obj) == opt.ASAPageObjs.length - 1))) { allObjsDone = true };
            };

            ieh = bw;
            if (ieh > maxBallSize) { ieh = maxBallSize };
            iew = ieh / 2;
            var altMaxVal = opt.ASAdata.maxAltValue;

            var hintlines = [];
            var hintx, hinty;

            // draw alts init values with alpha = 0.2
            if (opt.m_canvas_initalts == null) {
                opt.m_canvas_initalts = document.createElement('canvas');
                opt.m_canvas_initalts.width = opt.canvas.width;
                opt.m_canvas_initalts.height = opt.canvas.height;
                ctx = opt.m_canvas_initalts.getContext('2d');
                ctx.font = opt.ASAFont;

                for (var j = 0; j < altsCountFilter; j++) {
                    var altASA = opt.ASAdata.alternatives[j];
                    if (altASA.visible == 1) {
                        this._drawASAAltLine(ctx, j, -1, false, bw);
                    };
                };
            };
            ctx = opt.context;
            ctx.save();
            ctx.globalAlpha = 0.2;
            ctx.drawImage(opt.m_canvas_initalts, 0, 0);
            ctx.restore();

            // draw alts current values
            //if (opt.m_canvas_alts == null) {
                opt.m_canvas_alts = document.createElement('canvas');
                opt.m_canvas_alts.width = opt.canvas.width;
                opt.m_canvas_alts.height = opt.canvas.height;
                ctx = opt.m_canvas_alts.getContext('2d');
                ctx.font = opt.ASAFont;

                for (var j = 0; j < altsCountFilter; j++) {
                    var altASA = opt.ASAdata.alternatives[j];
                    if (altASA.visible == 1) {
                        var aKey = '_' + altASA.id + '_';
                        var mOverElement = opt.mouseOverElement.key;
                        if ((typeof mOverElement == "undefined") || (typeof mOverElement !== "undefined") && (mOverElement.indexOf(aKey) < 0)) {
                            this._drawASAAltLine(ctx, j, -1, true, bw);
                        };
                    };
                };
            //};

            ctx = opt.context;
            ctx.save();
            ctx.drawImage(opt.m_canvas_alts, 0, 0);

            // draw selected alternative line with selected alternative node
            for (var j = 0; j < altsCountFilter; j++) {
                var altASA = opt.ASAdata.alternatives[j];
                if (altASA.visible == 1) {
                    //var altKey = '_' + altASA.id + '_';
                    //var mOverElement = opt.mouseOverElement.key;
                    //if ((typeof mOverElement !== "undefined") && (mOverElement.indexOf('alt') >= 0)) {
                    //    if (mOverElement.indexOf(altKey) >= 0) {
                    //        this._drawASAAltLine(ctx, j, opt.SelectedObjectiveIndex, true, bw);
                    //    };
                    //};
                    if (opt.SelectedAlternativeIndex == altASA.id) {
                        this._drawASAAltLine(ctx, j, opt.SelectedObjectiveIndex, true, bw);
                    };

                    var idx = 0;
                    allObjsDone = false;
                    for (var i = 0; i <= opt.ASAdata.objectives.length; i++) {
                        var obj = opt.ASAdata.objectives[i];
                        if (!obj || ((obj.visible === 1) && (opt.ASAPageObjs.indexOf(obj) >= 0))) {
                            var aKey = 'alt' + (!obj ? i : obj.idx) + '_' + altASA.id + '_';
                            elementName = 'alt';
                            ctx.fillStyle = altASA.color;
                            iex = marginLeft + idx * (bw * 2) + bw - iew;
                            iey = marginTop + chartHeight - chartHeight * altASA.newVals[(!obj ? i : obj.idx)] / altMaxVal;
                            //var nextVisIdx = this._getNextVisibleObjIndex(i + 1);
                            var r = 0.4;
                            if (!allObjsDone) {
                                x1 = iex;
                                y1 = iey - iew;
                                x2 = x1 + ieh;
                                y2 = y1 + ieh;
                                intEl = { key: aKey, rect: [iex, iey - iew, ieh, ieh], element: [elementName, (!obj ? i : obj.idx), altASA.id], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
                                intMap.push(intEl);                    
                                if (aKey == opt.mouseOverElement.key) {
                                    ctx.lineWidth = 2;
                                    ctx.strokeStyle = '#eee';
                                    r = 0.9;
                                    hintlines.push(altASA.name);
                                    var delta = (altASA.newVals[(!obj ? i : obj.idx)] - altASA.initVals[(!obj ? i : obj.idx)]);
                                    var plus = '';
                                    if (delta > 0) { plus = '+' };
                                    hintlines.push((altASA.newVals[(!obj ? i : obj.idx)] * 100).toFixed(opt.valDigits) + '% Delta = ' + plus + (delta * 100).toFixed(opt.valDigits) + '%');
                                    hintx = iex + iew + 10;
                                    hinty = iey;
                                    ctx.beginPath();
                                    ctx.arc(iex + iew, iey, iew * r, 0, 2 * Math.PI, false);
                                    ctx.fill();
                                    ctx.stroke();
                                };
                            } else {
                                iey1 = marginTop + chartHeight - chartHeight * altASA.value / altMaxVal;
                                if (aKey == opt.mouseOverElement.key) {
                                    r = 0.9;
                                    hintlines.push(altASA.name);
                                    var delta = (altASA.value - altASA.initValue);
                                    var plus = '';
                                    if (delta > 0) { plus = '+' };
                                    hintlines.push((altASA.value * 100).toFixed(opt.valDigits) + '% Delta = ' + plus + (delta * 100).toFixed(opt.valDigits) + '%');
                                    hintx = iex + iew + 10;
                                    hinty = iey1;
                                    ctx.beginPath();
                                    ctx.lineWidth = 2;
                                    ctx.strokeStyle = '#eee';
                                    ctx.arc(iex + iew, iey1, iew * r, 0, 2 * Math.PI, false);
                                    ctx.fill();
                                    ctx.stroke();
                                };
                                x1 = iex;
                                y1 = iey1 - iew;
                                x2 = x1 + ieh;
                                y2 = y1 + ieh;
                                intEl = { key: aKey, rect: [iex, iey1 - iew, ieh, ieh], element: [elementName, i, altASA.id], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
                                intMap.push(intEl);
                            };
                            idx++;
                        };
                        if (!obj || ((obj.visible === 1) && (opt.ASAPageObjs.indexOf(obj) == opt.ASAPageObjs.length - 1))) allObjsDone = true;
                    };
                };
            };
            ctx.restore();
            ctx.lineWidth = 0.5;

            opt.interactiveMap = intMap;
            // draw alts legend
            if (opt.m_canvas_legend == null) {
                opt.m_canvas_legend = document.createElement('canvas');
                opt.m_canvas_legend.width = marginRight;
                opt.m_canvas_legend.height = opt.canvas.height;
                var m_context = opt.m_canvas_legend.getContext('2d');
                m_context.font = opt.ASAFont;
                this.drawASAlegend(m_context);
            };
            ctx.drawImage(opt.m_canvas_legend, marginLeft + chartWidth, 0);
            intMap = opt.interactiveMap;
            // draw hint box
            if (hintlines.length > 0) {
                this.drawHint(ctx, hintlines, hintx, hinty);
            };
            this._drawScrollbar(false);
            opt.interactiveMap = intMap;
        },
        drawHint: function (ctx, lines, x, y) {
            var maxW = 0;
            var lineH = 16;
            var maxH = lineH * lines.length;
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i];
                var linew = ctx.measureText(line).width;
                if (maxW < linew) { maxW = linew };
            };
            ctx.save();
            ctx.fillStyle = '#FCFBDE';
            ctx.beginPath();
            ctx.rect(x, y, maxW + 20, maxH + 20);
            ctx.fill();
            ctx.stroke();
            ctx.fillStyle = '#333333';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i];
                ctx.fillText(line, x + 10, y + 10 + i * lineH);
            };
            ctx.restore();
        },
        drawASAlegend: function (ctx) {
            var opt = this.options;
            if ((opt.GSAShowLegend)||(opt.viewMode == 'ASA')) {
                ctx.beginPath();
                ctx.fillStyle = opt.backgroundColor;
                //ctx.fillRect(opt.canvas.width - marginRight, 0, marginRight, opt.canvas.height);
                ctx.fillRect(0, 0, opt.m_canvas_legend.width, opt.m_canvas_legend.height);
                var marginRight = opt.GSAmarginRight;
                var marginTop = 20;
                var altsCountFilter = opt.ASAdata.alternatives.length;
                //if ((opt.altFilterValue > -1) && (opt.altFilterValue < opt.ASAdata.alternatives.length)) {
                //    altsCountFilter = opt.altFilterValue
                //};
                var idx = 0;
                for (var i = 0; i < altsCountFilter; i++) {
                    var alt = opt.ASAdata.alternatives[i];
                    if (alt.visible == 1) {
                        var yTop = idx * 24 + opt.legendLabelHeight - opt.AltScrollShift;
                        if (yTop > 0) { this._drawAltLegendLabel(ctx, alt, 10, yTop, marginRight, opt.legendLabelHeight, 1, true); }
                        if ((yTop) > (opt.canvas.height - marginTop)) { break };
                        idx++;
                    };
                }
                this._drawScrollbar(false);
            };
        },
        drawLegend: function (ctx) {
            var opt = this.options;
            if (opt.GSAShowLegend) {
                ctx.beginPath();
                ctx.fillStyle = opt.backgroundColor;
                var marginRight = opt.GSAmarginRight;
                var marginTop = 20;
                var altsCountFilter = opt.alts.length;
                var idx = 0;
                for (var i = 0; i < altsCountFilter; i++) {
                    var alt = opt.alts[i];
                    if (alt.visible == 1) {
                        var yTop = idx * 24 + opt.legendLabelHeight - opt.AltScrollShift;
                        if (yTop > 0) { this._drawAltLegendLabel(ctx, alt, opt.canvas.width - marginRight + 20, yTop, marginRight, opt.legendLabelHeight, 1, false); }
                        if ((yTop) > (opt.canvas.height - marginTop)) { break };
                        idx++;
                    };
                }
                //this._drawScrollbar(false);
            };
        },
        _get2DValue: function (objID, altID) {
            var opt = this.options;
            for (var j = 0; j < opt.objs[objID].values2D.length; j++) {
                var altItem = opt.objs[objID].values2D[j];
                if (altItem.altID == altID) {
                    if ((opt.normalizationMode !== 'normAll') && (opt.normalizationMode !== 'normSelected')) { return altItem.val } else { return altItem.valNorm };
                };
            };
            return 0;
        },

        redraw2D: function () {
            var opt = this.options;
            var intMap = [];
            var iex, iey, iew, ieh, btnMode, x1, y1, x2, y2, elementName;
            var marginLeft = 50;
            var marginTop = 20;
            var marginBottom = 70;
            var objXColor = '#aaaaff';
            var btnSize = opt.buttonSize;
            var marginRight = opt.GSAmarginRight;

            var id = opt.canvas.id;
            var optionsList = '';
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                optionsList += "<option value='" + i + "'>" + obj.name + "</option>";
            };
            $('#' + id + 'Options').html("<table style='width:100%;'><tr><td class='text'>Y-Axis: <select id='" + id + "yaxis' class='select' style='width:100px;'>" + optionsList + "</select></td><td class='text'>X-Axis: <select id='" + id + "xaxis' class='select' style='width:100px;'>" + optionsList + "</select></td></tr></table>");

            $('#' + id + 'xaxis').val(opt.SelectedObjectiveIndex);
            $('#' + id + 'yaxis').val(opt.SelectedObjectiveIndexY);
            var scope = this;
            $('#' + id + 'xaxis').change(function () {
                scope.options.SelectedObjectiveIndex = parseInt($('#' + id + 'xaxis').val());
                scope.redrawSA();
            });
            $('#' + id + 'yaxis').change(function () {
                scope.options.SelectedObjectiveIndexY = parseInt($('#' + id + 'yaxis').val());
                scope.redrawSA();
            });

            //if (opt.GSAShowLegend) { opt.GSAmarginRight = 240 } else { opt.GSAmarginRight = 40 };
            var ctx = opt.context;
            var altsCountFilter = opt.alts.length;
            if (opt.GSAShowLegend) {
                var maxWidth = 10;
                if (opt.legendPosition == 0) {
                    for (var i = 0; i < altsCountFilter; i++) {
                        var alt = opt.alts[i];
                        if (alt.visible == 1) {
                            var altWidth = ctx.measureText(alt.name).width + 50;
                            if ((maxWidth < altWidth) && (altWidth < opt.canvas.width / 2)) {
                                maxWidth = altWidth
                            };
                        };
                    };
                    opt.GSAmarginRight = maxWidth;              
                } else {
                    marginBottom = opt.canvas.height / 3;
                };
            } else {
                opt.GSAmarginRight = 40;
            };
            marginRight = opt.GSAmarginRight;
            opt.context.font = opt.DSATitleFont;
            marginLeft = opt.context.measureText((1000.0).toFixed(opt.valDigits) + '%').width + 50;
            var maxAlt = opt.MaxAltValue;
            var minAlt = opt.MinAltValue;
            if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) {
                this._calcualteNormalizedValues();
                this._calculateOverallMinMaxValues();
                maxAlt = opt.maxNormAltValue;
                minAlt = opt.minNormAltValue;
            };
            var normCoefY = (maxAlt - minAlt);
            ctx.beginPath();
            ctx.fillStyle = opt.backgroundColor;
            ctx.fillRect(0, 0, opt.canvas.width, opt.canvas.height);

            ctx.beginPath();
            ctx.fillStyle = '#ffffff';
            var chartHeight = opt.canvas.height - (marginTop + marginBottom);
            var chartWidth = opt.canvas.width - (marginLeft + marginRight);
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
                ctx.lineTo(x, opt.canvas.height - marginBottom + 5);
                ctx.stroke();
                ctx.beginPath();
                ctx.textAlign = 'center';
                ctx.textBaseline = 'top';
                ctx.fillText(((minAlt + xValStep * i) * 100).toFixed(opt.valDigits) + '%', x-10, opt.canvas.height - marginBottom + 10);
            };
            for (var i = 0; i <= yTicks; i++) {
                var y = yStep * i + marginTop;
                ctx.beginPath();
                ctx.moveTo(marginLeft - 5, y);
                ctx.lineTo(opt.canvas.width - marginRight, y);
                ctx.stroke();
                ctx.beginPath();
                ctx.textAlign = 'right';
                ctx.textBaseline = 'bottom';
                ctx.fillText((((maxAlt - yValStep * i) * 100).toFixed(opt.valDigits)) + '%', marginLeft - 10, y + 5);
            };

            var objValue = opt.objs[opt.SelectedObjectiveIndex].value;
            var objYValue = opt.objs[opt.SelectedObjectiveIndexY].value;
            var selectedObjName = opt.objs[opt.SelectedObjectiveIndex].name + ' = ' + (objValue * 100).toFixed(opt.valDigits) + '%';
            var selectedObjYName = opt.objs[opt.SelectedObjectiveIndexY].name + ' = ' + (objYValue * 100).toFixed(opt.valDigits) + '%';
            ctx.beginPath();
            ctx.textAlign = 'center';
            ctx.font = getFont(ctx, selectedObjName, opt.DSAFontSize, 10, opt.canvas.width - 74, opt.DSAFontFace);
            ctx.fillStyle = 'blue';
            ctx.textBaseline = 'top';
            ctx.fillText(selectedObjName, opt.canvasHalfWidth, opt.canvas.height - 34);
            //iex = marginLeft;
            //iey = opt.canvas.height - opt.DSABarHeight - 5;
            //iew = chartWidth;
            //ieh = opt.DSABarHeight;
            //intEl = { key: 'objX', rect: [iex, iey, iew, ieh] };
            //intMap.push(intEl);
            //this._drawDSABar(opt.SelectedObjectiveIndex, iex, iey, iew, opt.objs[opt.SelectedObjectiveIndex].name, objValue, opt.objs[opt.SelectedObjectiveIndex].initValue, objXColor, false, 1, 0, 1);

            ctx.save();
            ctx.rotate(-Math.PI / 2);
            ctx.beginPath();
            ctx.fillText(selectedObjYName, -opt.canvas.height / 2, 10);
            ctx.restore();

            var xcenter = opt.canvasHalfWidth - (opt.objs.length * 10 / 2);
            for (var i = 0; i < opt.objs.length; i++) {
                ctx.beginPath();
                ctx.arc(i * 10 + xcenter, opt.canvas.height - 10, 4, 0, 2 * Math.PI, false);
                if (i === opt.SelectedObjectiveIndex) { ctx.fillStyle = 'blue' } else { ctx.fillStyle = '#eee' };
                ctx.fill();
            };

            var ycenter = opt.canvas.height / 2 + (opt.objs.length * 10 / 2);
            for (var i = 0; i < opt.objs.length; i++) {
                ctx.beginPath();
                ctx.arc(40, ycenter - i * 10, 4, 0, 2 * Math.PI, false);
                if (i === opt.SelectedObjectiveIndexY) { ctx.fillStyle = 'blue' } else { ctx.fillStyle = '#eee' };
                ctx.fill();
            };

            var xbtm = opt.canvas.height;
            var xw = opt.canvas.width - marginRight - 10;
            iey = xbtm - btnSize - 5;
            if (opt.SelectedObjectiveIndex < opt.objs.length - 1 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'next') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iex = xw - btnSize - 5;
                x1 = iex;
                x2 = iex + btnSize;
                y1 = iey;
                y2 = iey + btnSize;
                elementName = 'next';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Next X Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 0);
            };

            if (opt.SelectedObjectiveIndex !== 0 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'prev') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iex = xw - btnSize * 2 - 10;
                x1 = iex;
                x2 = iex + btnSize;
                y1 = iey;
                y2 = iey + btnSize;
                elementName = 'prev';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Previous X Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 180);
            };

            iex = 5;
            if (opt.SelectedObjectiveIndexY < opt.objs.length - 1 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'nextY') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iey = 5;
                x1 = iex;
                x2 = iex + btnSize;
                y1 = iey;
                y2 = iey + btnSize;
                elementName = 'nextY';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Next Y Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, -90);
            };

            if (opt.SelectedObjectiveIndexY !== 0 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'prevY') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iey = btnSize + 10;
                x1 = iex;
                x2 = iex + btnSize;
                y1 = iey;
                y2 = iey + btnSize;
                elementName = 'prevY';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Previous Y Objective' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 90);
            };

            ctx.textBaseline = 'middle';
            var altsCountFilter = opt.alts.length;
            if ((opt.altFilterValue > -1) && (opt.altFilterValue < opt.alts.length)) {
                altsCountFilter = opt.altFilterValue
            };

            var sumNormCoefX = 1;
            var sumNormCoefY = 1;
            if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) {
                sumNormCoefX = 0;
                sumNormCoefY = 0;
                for (var i = 0; i < opt.alts.length; i++) {
                    var alt = opt.alts[i];
                    if ((alt.visible == 1) || (opt.normalizationMode !== 'normSelected')) {
                        var xVal = this._get2DValue(opt.SelectedObjectiveIndex, alt.id);
                        var yVal = this._get2DValue(opt.SelectedObjectiveIndexY, alt.id);
                        sumNormCoefX += xVal;
                        sumNormCoefY += yVal;
                    };
                };
                if (sumNormCoefX == 0) { sumNormCoefX = 1 };
                if (sumNormCoefY == 0) { sumNormCoefY = 1 };
            };

            for (var i = 0; i < altsCountFilter; i++) {
                var alt = opt.alts[i];
                if (alt.visible == 1) {
                    var xVal = this._get2DValue(opt.SelectedObjectiveIndex, alt.id);
                    var yVal = this._get2DValue(opt.SelectedObjectiveIndexY, alt.id);
                    if ((opt.normalizationMode == 'normAll') || (opt.normalizationMode == 'normSelected')) {
                        xVal = xVal / sumNormCoefX;
                        yVal = yVal / sumNormCoefY;
                    };
                    var ValString = alt.name + ' X: ' + (xVal * 100).toFixed(opt.valDigits) + '% Y: ' + (yVal * 100).toFixed(opt.valDigits) + '%';
                    ctx.beginPath();
                    var x1 = marginLeft + ((xVal - minAlt) / normCoefY * chartWidth);
                    var y1 = marginTop + chartHeight - ((yVal - minAlt) / normCoefY * chartHeight);
                    var radius = 10;
                    iex = x1 - radius;
                    iey = y1 - radius;
                    iew = radius * 2;
                    var ieKey = 'alt:' + i;
                    x1 = iex;
                    x2 = iex + iew;
                    y1 = iey;
                    y2 = iey + iew;
                    elementName = 'alt:';
                    intEl = { key: ieKey, rect: [iex, iey, iew, iew], element: [elementName, i], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: ValString };
                    intMap.push(intEl);
                    ctx.arc(x1 + radius, y1 + radius, 10, 0, 2 * Math.PI, false);
                    //ctx.fillStyle = ColorPalette[alt.idx % ColorPalette.length];
                    ctx.fillStyle = alt.color;
                    ctx.fill();
                };
            };

            for (var i = 0; i < altsCountFilter; i++) {
                var ieKey = 'alt:' + i;
                ctx.beginPath();
                var ie = opt.mouseOverElement;
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
            this.drawLegend(ctx);
            opt.interactiveMap = intMap;
        },

        redrawHTH: function () {
            var opt = this.options;
            var intMap = [];
            var iex, iey, iew, ieh, btnMode, x1, y1, x2, y2;
            var btnSize = opt.buttonSize;
            var id = opt.canvas.id;
            var optionsList = '';
            for (var i = 0; i < opt.visibleAlts.length; i++) {
                var alt = opt.visibleAlts[i];
                optionsList += "<option value='" + alt.id + "'>" + alt.name + "</option>";
            };
            $('#' + id + 'Options').html("<table style='width:100%;'><tr><td style='text-align:right;width:50%;'><select id='" + id + "Alt1' class='select' style='width:100px;'>" + optionsList + "</select></td><td><select id='" + id + "Alt2' class='select' style='width:100px;'>" + optionsList + "</select></td></tr></table>");
            var ctx = opt.context;
            ctx.beginPath();
            ctx.fillStyle = opt.backgroundColor;
            ctx.fillRect(0, 0, opt.canvas.width, opt.canvas.height);

            ctx.font = opt.DSATitleFont;
            var alt1 = this.getAltByID(opt.HTHAlt1Index);
            var alt2 = this.getAltByID(opt.HTHAlt2Index);

            //if (!(alt1) && !(alt2)) return false;
            if (opt.visibleAlts.indexOf(alt1) == -1 && opt.visibleAlts.length > 0) { alt1 = opt.visibleAlts[0] };
            if (opt.visibleAlts.indexOf(alt2) == -1 && opt.visibleAlts.length > 0) { alt2 = opt.visibleAlts[0] };
            if ((alt1)) $('#' + id + 'Alt1').val(alt1.id);
            if ((alt2)) $('#' + id + 'Alt2').val(alt2.id);
            var scope = this;
            $('#' + id + 'Alt1').change(function () {
                scope.options.HTHAlt1Index = $('#' + id + 'Alt1').val();
                scope.redrawSA();
            });
            $('#' + id + 'Alt2').change(function () {
                scope.options.HTHAlt2Index = $('#' + id + 'Alt2').val();
                scope.redrawSA();
            });
            ctx.beginPath();
            ctx.fillStyle = '#000';
            ctx.textBaseline = 'middle';
            ctx.textAlign = 'center';
            ctx.fillText('< >', opt.canvasHalfWidth, btnSize + 5);
            ctx.textAlign = 'right';
            if ((alt1)) ctx.fillText(alt1.name, opt.canvasHalfWidth - btnSize * 2, btnSize + 5);
            ctx.textAlign = 'left';
            if ((alt2)) ctx.fillText(alt2.name, opt.canvasHalfWidth + btnSize * 2, btnSize + 5);

            var alt1Idx = opt.visibleAlts.indexOf(this.getAltByID(opt.HTHAlt1Index));
            var alt2Idx = opt.visibleAlts.indexOf(this.getAltByID(opt.HTHAlt2Index));
            if (alt1Idx < opt.visibleAlts.length - 1 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'nextAlt1') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iex = 5;
                iey = 0;
                x1 = iex;
                x2 = iex + btnSize;
                y1 = iey;
                y2 = iey + btnSize;
                elementName = 'nextAlt1';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Next Alternative' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, -90);
            };
            if (alt1Idx > 0 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'prevAlt1') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iex = 5;
                iey = btnSize;
                x1 = iex;
                x2 = iex + btnSize;
                y1 = iey;
                y2 = iey + btnSize;
                elementName = 'prevAlt1';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Previous Alternative' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 90);
            };
            if (alt2Idx < opt.visibleAlts.length - 1 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'nextAlt2') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iex = opt.canvas.width - btnSize - 5;
                iey = 0;
                x1 = iex;
                x2 = iex + btnSize;
                y1 = iey;
                y2 = iey + btnSize;
                elementName = 'nextAlt2';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Next Alternative' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, -90);
            };
            if (alt2Idx > 0 && !opt.hideButtons) {
                btnMode = 'normal';
                if ((opt.mouseOverElement.key === 'prevAlt2') && (!opt.readOnly)) {
                    btnMode = 'over';
                    document.body.style.cursor = 'hand';
                };
                iex = opt.canvas.width - btnSize - 5;;
                iey = btnSize;
                x1 = iex;
                x2 = iex + btnSize;
                y1 = iey;
                y2 = iey + btnSize;
                elementName = 'prevAlt2';
                intEl = { key: elementName, rect: [iex, iey, btnSize, btnSize], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: 'Previous Alternative' };
                intMap.push(intEl);
                drawButton(ctx, iex, iey, btnSize, 'nextArrow', btnMode, 90);
            };

            ctx.beginPath();
            ctx.arc(opt.canvasHalfWidth - btnSize - 5, btnSize + 5, btnSize / 2, 0, 2 * Math.PI, false);
            var elementName = 'alt:';
            if ((alt1)) {
                ctx.fillStyle = alt1.color;
                ctx.fill();
                elementName += alt1.id;
            }
            iex = opt.canvasHalfWidth - btnSize * 1.5 - 5;
            iey = btnSize / 2 + 5;
            iew = btnSize;
            ieh = btnSize;
            var x1, y1, x2, y2;
            x1 = iex;
            x2 = iex + iew;
            y1 = iey;
            y2 = iey + ieh;
            var intEl = { key: elementName, rect: [x1, y1, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
            intMap.push(intEl);

            ctx.beginPath();
            ctx.arc(opt.canvasHalfWidth + btnSize + 5, btnSize + 5, btnSize / 2, 0, 2 * Math.PI, false);
            var elementName = 'alt:';
            if ((alt2)) {
                ctx.fillStyle = alt2.color;
                ctx.fill();
                elementName += alt2.id;
            }
            iex = opt.canvasHalfWidth + btnSize / 2 + 5;
            iey = btnSize / 2 + 5;
            iew = btnSize;
            ieh = btnSize;
            var x1, y1, x2, y2;
            x1 = iex;
            x2 = iex + iew;
            y1 = iey;
            y2 = iey + ieh;
            var intEl = { key: elementName, rect: [x1, y1, iew, ieh], element: [elementName], area: [x1, y1, x2, y1, x2, y2, x1, y2], hint: '' };
            intMap.push(intEl);
            var total1 = 0;
            var total2 = 0;
            var difTotal = 0;
            var maxAbsVal = 0;
            var overallVal = 0;
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                var alt1Val = this._get2DValue(i, (alt1) ? alt1.id : -1);
                var alt2Val = this._get2DValue(i, (alt2) ? alt2.id : -1);
                var altDiffVal = (alt1Val - alt2Val) * obj.value;
                overallVal += altDiffVal;
                if (maxAbsVal < Math.abs(altDiffVal)) { maxAbsVal = Math.abs(altDiffVal) };
            };
            if (Math.abs(overallVal) > maxAbsVal) { difTotal = Math.abs(overallVal) } else { difTotal = maxAbsVal};
            if (difTotal == 0) { difTotal = 1 };
            overallVal = 0;
            for (var i = 0; i < opt.objs.length; i++) {
                var obj = opt.objs[i];
                var alt1Val = this._get2DValue(i, (alt1) ? alt1.id : -1);
                var alt2Val = this._get2DValue(i, (alt2) ? alt2.id : -1);
                var altDiffVal = (alt1Val - alt2Val) * obj.value;
                var altBarWidth = -altDiffVal / difTotal * (opt.canvasHalfWidth - 15);
                ctx.fillStyle = '#eee';
                ctx.beginPath();
                ctx.fillRect(5, btnSize * 2.2 + i * opt.DSABarHeight * 0.8 + 8, opt.canvas.width - 9, opt.DSABarHeight / 2 - 6);
                if (altDiffVal < 0) {
                    if ((alt2)) ctx.fillStyle = alt2.color;
                } else {
                    if ((alt1)) ctx.fillStyle = alt1.color;
                };

                ctx.fillRect(opt.canvasHalfWidth, btnSize * 2.2 + i * opt.DSABarHeight * 0.8 + 8, altBarWidth, opt.DSABarHeight / 2 - 6);
                overallVal += altDiffVal;

                ctx.fillStyle = '#000';
                ctx.beginPath();
                ctx.textAlign = 'center';
                ctx.fillText(obj.name, opt.canvasHalfWidth, btnSize * 2.2 + i * opt.DSABarHeight * 0.8);

                if (altDiffVal > 0) {
                    ctx.textAlign = 'left';
                    ctx.fillText((altDiffVal * 100).toFixed(opt.valDigits) + '%', 10, btnSize * 2.6 + i * opt.DSABarHeight * 0.8);
                } else {
                    ctx.textAlign = 'right';
                    ctx.fillText((Math.abs(altDiffVal) * 100).toFixed(opt.valDigits) + '%', opt.canvas.width - 10, btnSize * 2.6 + i * opt.DSABarHeight * 0.8);
                };
            };
            ctx.textAlign = 'center';
            ctx.fillStyle = '#eee';
            ctx.beginPath();
            var altBarWidth = -overallVal / difTotal * (opt.canvasHalfWidth - 15);
            ctx.fillRect(5, btnSize * 2.2 + opt.objs.length * opt.DSABarHeight * 0.8 + 8, opt.canvas.width - 10, opt.DSABarHeight / 2 - 6);
            var overallTop = btnSize * 2.2 + opt.objs.length * opt.DSABarHeight * 0.8 + 2;
            var overallBarTop = btnSize * 2.2 + opt.objs.length * opt.DSABarHeight * 0.8 + 8;
            if (overallVal < 0) {
                if ((alt2)) ctx.fillStyle = alt2.color;
            } else {
                if ((alt1)) ctx.fillStyle = alt1.color;
            };
            ctx.fillRect(opt.canvasHalfWidth, overallBarTop, altBarWidth, opt.DSABarHeight / 2 - 6);
            ctx.fillStyle = '#000';
            ctx.beginPath();

            ctx.fillText('OVERALL', opt.canvasHalfWidth, overallTop);

            if (overallVal > 0) {
                ctx.textAlign = 'left';
                ctx.fillText((overallVal * 100).toFixed(opt.valDigits) + '%', 10, overallTop + btnSize * 0.4);
            } else {
                ctx.textAlign = 'right';
                ctx.fillText((Math.abs(overallVal) * 100).toFixed(opt.valDigits) + '%', opt.canvas.width - 10, overallTop + btnSize * 0.4);
            };

            opt.interactiveMap = intMap;
        },
        showWarning: function () {
            var opt = this.options;
            var ctx = opt.context;
            ctx.beginPath();
            ctx.fillStyle = opt.backgroundColor;
            ctx.fillRect(0, 0, opt.canvas.width, opt.canvas.height);

            ctx.font = opt.DSATitleFont;
            ctx.beginPath();
            ctx.fillStyle = '#000';
            ctx.textBaseline = 'middle';
            ctx.textAlign = 'center';
            var warningMessage = 'Not enough data to show sensitivity results.';
            var y = opt.canvas.height / 2 - 20;
            ctx.fillText(warningMessage, opt.canvasHalfWidth, y);

            if (opt.objs.length == 0) {
                y += 20;
                warningMessage = 'No objectives to show in this hierarchy.';
                ctx.fillText(warningMessage, opt.canvasHalfWidth, y);
            };
            if (opt.alts.length == 0) {
                y += 20;
                warningMessage = 'No alternatives to show in this project.';
                ctx.fillText(warningMessage, opt.canvasHalfWidth, y);
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
            //for (var i = 0; i < gradientValues.length; i++) {
            //    if (gradientValues[i].altID == altID) {
            //        return gradientValues[i].val;
            //    };
            //};
            //return null;
            if (gradientValues !== "undefined") {
                return $.grep(gradientValues, function (e) { return e.altID === altID; })[0].val;
            } else {
                return 0;
            };
        },
        _getAngleStep: function () {
            var opt = this.options;
            var astep = Math.PI * 2 / opt.objs.length;
            if (opt.objs.length <= 2) { astep = Math.PI / 2 };
            return astep;
        },
        GetMouseOverObjectiveIndex: function (x, y) {
            var opt = this.options;
            var retVal = -1
            if (opt.viewMode === 'DSA') {
                if (x < opt.canvasHalfWidth - 5) {
                    retVal = Math.round((y + 5 + opt.ObjScrollShift) / opt.DSABarHeight) - 1
                } else {
                    retVal = opt.SelectedObjectiveIndex
                };
                if (retVal < 0) { retVal = 0 };
            };
            if ((opt.viewMode === 'PSA') || (opt.viewMode === 'ASA')) {
                //retVal = Math.round((x + 20 + opt.ObjScrollShift) / opt.PSABarHeight) - 1
                if ((opt.mouseOverElement != '') && (opt.mouseOverElement != {})) {
                    retVal = Number(opt.mouseOverElement.key.replace('obj', ''));
                };
                if (opt.showRadar) {
                    var astep = this._getAngleStep();
                    var l = opt.canvas.height / 2;
                    for (var i = -1; i < opt.objs.length + 1; i++) {
                        var deltaX = x - opt.canvasHalfWidth / 2;
                        var deltaY = y - l;
                        var deg = Math.atan2(deltaY, deltaX);
                        if ((deg > (astep * i - astep / 2 - Math.PI / 2)) && (deg < (astep * i + astep / 2 - Math.PI / 2))) { retVal = i; break; };
                        //var xmin = opt.canvasHalfWidth / 2 + Math.sin(astep * i - astep / 2) * l;
                        //var xmax = opt.canvasHalfWidth / 2 + Math.sin(astep * i + astep / 2) * l;
                        //var ymin = l - Math.cos(astep * i - astep / 2) * l;
                        //var ymax = l - Math.cos(astep * i + astep / 2) * l;
                        //if ((x > xmin) && (x < xmax) && (y > ymin) && (y < ymax)) { retVal = i; break; }; 
                    };
                    if (retVal < 0) { retVal = opt.objs.length - 1 };
                };
            };
            if ((opt.viewMode !== 'DSA') && (retVal >= opt.objs.length)) { retVal = opt.objs.length - 1 };
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
            var opt = this.options;
            if (opt.isMouseDown) {
                if ((opt.SelectedObjectiveIndex < opt.objs.length) && (opt.SelectedObjectiveIndex >= 0)) {
                    var NewObjValue = 1;
                    if (opt.viewMode === 'DSA') {
                        NewObjValue = ((x - 10) / (opt.canvasHalfWidth - 30)) * opt.objInitSum;
                    };
                    if (opt.viewMode === 'GSA') {
                        var objXRect = this._getInteractiveMapRect('objX');
                        NewObjValue = ((x - objXRect[0]) / (objXRect[2]));
                    };
                    if (opt.viewMode === 'ASA') {
                        var objXRect = this._getInteractiveMapRect('objX');
                        NewObjValue = ((x - objXRect[0]) / (objXRect[2]));
                    };
                    if (opt.viewMode === '2D') {

                    };
                    if (opt.viewMode === 'PSA') {
                        if (!opt.showRadar) {
                            NewObjValue = ((opt.canvas.height - 9 - y) / (opt.canvas.height - 31));
                        } else {
                            var w = opt.canvasHalfWidth / 2;
                            var l = opt.canvas.height / 2;
                            NewObjValue = Math.sqrt(Math.pow(Math.abs(y - l), 2) + Math.pow(Math.abs(x - w), 2)) / l;
                        };
                        
                    };
                    var aObj = opt.objs[opt.SelectedObjectiveIndex];
                    if (NewObjValue != aObj.value) { opt.IsObjValuesChanged = true };
                    this.setObjLocalPriority(aObj.id, NewObjValue);
                    this.updateAltPriorities(aObj.id);
                    if (opt.isMultiView) this.syncSACharts(true, aObj.id, NewObjValue);
                };
                this.redrawSA();
            };
            opt.isUpdateFinished = true;
        },
        refreshASA: function () {
            var opt = this.options;
            opt.m_canvas_alts = null;
            opt.m_canvas_background = null;
            opt.m_canvas_initalts = null;
            opt.m_canvas_legend = null;
            if (opt.ASASortBy == 0) opt.ASAdata.objectives.sort(SortByIndex);
            if (opt.ASASortBy == 1) opt.ASAdata.objectives.sort(SortByPriority);
            if (opt.ASASortBy == 2) opt.ASAdata.objectives.sort(SortByName);
            this.redrawSA();
        },
        resizeCanvas: function (width, height) {
            var opt = this.options;
            if (width < 200) width = 200;
            if (height < 100) height = 100;
            opt.canvas.width = width;
            opt.canvas.height = height;
            if (width < 400) { opt.isMobile = true } else { opt.isMobile = false };
            opt.context = opt.canvas.getContext('2d');
            opt.canvasRect = opt.canvas.getBoundingClientRect();
            opt.canvasHalfWidth = opt.canvas.width / 2;
            if (opt.viewMode === 'ASA') {
                opt.m_canvas_alts = null;
                opt.m_canvas_background = null;
                opt.m_canvas_initalts = null;
                opt.m_canvas_legend = null;
            };
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
            var opt = this.options;
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                if (alt.id == AltID) { return alt };
            };
            return null;
        },
        getASAAltByID: function (AltID) {
            return $.grep(this.options.ASAdata.alternatives, function (e) { return e.id === AltID; })[0];
        },
        setAltColor: function (AltID, color) {
            var alt = this.getAltByID(AltID);
            if ((alt)) alt.color = color;
            this.redrawSA();
        },
        setObjLocalPriority: function (ObjID, ObjValue) {
            var objSum = this.options.objInitSum;
            if (ObjValue < 0) { ObjValue = 0 };
            if (ObjValue > objSum) { ObjValue = objSum };
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
                coef = (objSum - ObjValue) / total;
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
            var opt = this.options;
            for (var i = 0; i < opt.alts.length; i++) {
                var alt = opt.alts[i];
                var obj = this.getObjByID(ObjID);
                var aMinValue = this.getGradientValueByAltID(obj.gradientMinValues, alt.id);
                var aMaxValue = this.getGradientValueByAltID(obj.gradientMaxValues, alt.id);;
                alt.value = aMinValue + (aMaxValue - aMinValue) * obj.value;
            };
            if (opt.viewMode === 'DSA') { this.sortAlternatives() };
            if ((opt.viewMode === 'GSA') && opt.GSAShowLegend) { this.sortAlternatives() };
            if ((opt.viewMode === 'ASA') && opt.GSAShowLegend) { opt.alts.sort(SortByValue) };
        },

        sortAlternatives: function () {
            //if (this.options.SAAltsSortBy == 1) { this.options.alts.sort(SortByValue) };
            this.applyAltsSorting();
        }

    });
})(jQuery);

function numberToStr(n, decimals) {
    var n1 = Number(n.toFixed(decimals));
    var n2 = Number(n.toFixed());
    if (n1 == n2) {
        return (n).toFixed()
    } else {
        return n.toFixed(decimals)
    };
}

function SortByValue(a, b) {
    return ((a.value < b.value) ? 1 : ((a.value > b.value) ? -1 : 0));
}

function SortByIndex(a, b) {
    return ((a.idx > b.idx) ? 1 : ((a.idx < b.idx) ? -1 : 0));
}

function SortByName(a, b) {
    return ((a.name > b.name) ? 1 : ((a.name < b.name) ? -1 : 0));
}

function SortByPriority(a, b) {
    return ((a.prty < b.prty) ? 1 : ((a.prty > b.prty) ? -1 : 0));
}

function getRelativePosition(canvas, absX, absY) {
    var rect = canvas.getBoundingClientRect();
    return {
        x: absX - rect.left,
        y: absY - rect.top
    };
}

function getAbsolutePosition(canvas, absX, absY) {
    var rect = canvas.getBoundingClientRect();
    return {
        x: absX + rect.left,
        y: absY + rect.top
    };
}

function openColorPicker(x, y, key, canvas) {
    if ((typeof key !== "undefined")&&(typeof ecColorPicker === "function")) {
        //var pos = getAbsolutePosition(canvas, x, y);
        ecColorPicker(event, null, true, key);
    }
}