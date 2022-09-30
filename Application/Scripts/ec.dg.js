/* Expertchoice jquery datagrid widget created by Sergey Lysikov */

var chArrowUp = '&#x25B2;';
var chArrowDown = '&#x25BC;';
(function ($) {
    $.widget("expertchoice.datagrid", {
        options: {
            columns: [{ id: '0', idx: 1, title: 'Column 1', nodeid: 0, mt:'PW'},
                      { id: '1', idx: 2, title: 'Column 2', nodeid: 1, mt:'PW'}],
            rows: [{ id: '0', idx: 1, title: 'Row 1', nodeid: 0, attrvals: [1, '2'], show: 1, cont: [1, 1], dg:[] },
                   { id: '0', idx: 2, title: 'Row 2', nodeid: 1, attrvals: [3, '4'], show: 1, cont: [0, 0], dg:[] }],
            viewMode: 'roles',
            rolesMode: 'alts',
            showContributions: true,
            showNoSources: false,
            use_datamapping: false, // D4463
            readonly: false,        // D4470
            infodocReadOnly: false,
            hasMapKey: false,
            gridContent: [],
            wrtInfodocs: [],
            nodeInfodocs: [],
            nodeDataMap:[],
            affectedCols: [],
            affectedRows: [],
            terminalNodesCount: 0,
            maxNodesLevel: 0,
            minMarginTop: 0,
            rowsTitle: 'Alternatives',
            attributes: [{ guid: 'guid0', name: 'Name0', type: 'string', search: true, width: 50 }, { guid: 'guid1', name: 'Name1', type: 'int', search: true, width: 50 }],
            canvas: null,
            context: null,
            canvasRect: null,
            interactiveMap: [],
            redrawCount: 0,
            fontSize: 13,
            splitterWidth: 4,
            scrollBarWidth: 8,
            colors: ['#666666', '#888888', '#999999', '#aaaaaa', '#eeeeee', '#88bacd', '#f5f5f5'],
            evenStyle: '#fefefe',
            oddStyle: '#fafafa',
            selStyle: '#ddebf8',
            warningStyle: '#ffcccc',
            isMouseDown: false,
            mouseDownPosition: null,
            mouseOverElement: {},
            marginTop: 100,
            marginTopOld: 100,
            marginLeft: 100,
            marginLeftOld:100,
            maxRowWidth: 24,
            maxColWidth: 24,
            hscroll: 0,
            vscroll: 0,
            hscrollOld: 0,
            vscrollOld: 0,
            realRowAreaHeight: 100,
            realColAreaWidth: 100,
            width: 500,
            height: 500,
            checkedUsers: [], //A1362
            isForGroups: false, //A1520
            valDigits: 4,
            showRolesStats: false,
            showRolesMode: 0,
            showMissedRoles: false,
            dgMinColumnWidth: 26,
            dgColAltNameGuid: 'b6ff0096-989a-457c-9ea7-4e72c58f65da',
            dgColAltDMGuid: '00000000-0000-0000-0000-000000000000',
            showAttributes: null,
            hiddenAttributes: null,
            pointerMoveTarget: null,
            datamappings: [] //'AS/25743a
        },
        _create: function () {
            var opt = this.options;
            opt.canvas = this.element[0];
            opt.context = opt.canvas.getContext('2d');
            opt.canvasRect = opt.canvas.getBoundingClientRect();
            this.resize(opt.width, opt.height);
            this._calculateMargins();
            if (opt.viewMode == 'datagrid') { this._calculateDGColumnsWidth() };
            if (opt.viewMode == 'contributions' || opt.viewMode == 'roles') { this.updateObjCheckboxState() };
            if (opt.viewMode == 'roles') { this.updateRoles(false) };
            this.updateAttributesColumnWidth();
            this.redraw();
            this._on(this.options.canvas, {
                "pointerdown": function (event) {
                    if (this.options.readonly) return false;    // D4470
                    this._onMouseDown(event);
                    this.options.pointerMoveTarget = event.target;
                    var scope = this;
                    event.target.onpointermove = function (e) {
                        var opt = scope.options;
                        var pos = getRelativePosition(opt.canvas, e.clientX, e.clientY);
                        var oldIE = opt.mouseOverElement;
                        var needredraw = false;
                        if ((opt.mouseOverElement.key == 'topSplitter') && (opt.isMouseDown)) {
                            var newmarginTop = opt.marginTopOld + pos.y - opt.mouseDownPosition.y;
                            if ((newmarginTop != opt.marginTop) && (newmarginTop > opt.minMarginTop + opt.fontSize * 2)) {
                                var maxMarginTop = opt.maxColWidth + opt.fontSize * 3 + opt.minMarginTop;
                                if (newmarginTop < (maxMarginTop)) {
                                    opt.marginTop = newmarginTop;
                                } else {
                                    opt.marginTop = maxMarginTop;
                                };
                                scope.setNewVerticalScrollPosition(opt.vscroll);
                                needredraw = true;
                            };
                        };
                        if ((opt.mouseOverElement.key == 'leftSplitter') && (opt.isMouseDown)) {
                            var newmarginLeft = opt.marginLeftOld + pos.x - opt.mouseDownPosition.x;
                            if ((newmarginLeft != opt.marginLeft) && (newmarginLeft > opt.fontSize * 2) && (newmarginLeft < (opt.canvas.width / 4 * 3))) {
                                if (newmarginLeft < (opt.maxRowWidth + opt.fontSize * 3)) {
                                    opt.marginLeft = newmarginLeft;
                                } else {
                                    opt.marginLeft = opt.maxRowWidth + opt.fontSize * 3;
                                };
                                scope.setNewHorizontalScrollPosition(opt.hscroll);
                                needredraw = true;
                            };
                        };
                        if ((opt.mouseOverElement.key == 'vertScrollbar') && (opt.isMouseDown)) {
                            var newvscroll = opt.vscrollOld + (pos.y - opt.mouseDownPosition.y);
                            scope.setNewVerticalScrollPosition(newvscroll);
                            needredraw = true;
                        };
                        if ((opt.mouseOverElement.key == 'horScrollbar') && (opt.isMouseDown)) {
                            var newhscroll = opt.hscrollOld + pos.x - opt.mouseDownPosition.x;
                            scope.setNewHorizontalScrollPosition(newhscroll);
                            needredraw = true;
                        };
                        if (oldIE.key != opt.mouseOverElement.key) {
                            needredraw = true;
                        };
                        if (needredraw) {
                            scope.redraw();
                        };
                    };
                    event.target.setPointerCapture(event.pointerId);
                    //document.body.style.cursor = 'pointer';
                    return false;
                },
                "touchstart": function (event) {
                    if (this.options.readonly) return false;    // D4470
                    //var pos = getRelativePosition(this.options.canvas, event.originalEvent.touches[0].pageX, event.originalEvent.touches[0].pageY);
                    //this._onMouseMove(pos);
                    //this.options.mouseDownPosition = pos;
                    //this._onMouseDown(pos, this);
                    //return false;
                },
                "touchmove": function (event) {
                    if (this.options.readonly) return false;    // D4470
                    //var pos = getRelativePosition(this.options.canvas, event.originalEvent.touches[0].pageX, event.originalEvent.touches[0].pageY);
                    //this._onMouseMove(pos, this);
                    //return false;
                },
                "mousemove": function (event) {
                    if (this.options.readonly) return false;    // D4470
                    //var pos = getRelativePosition(this.options.canvas, event.clientX, event.clientY);
                    this._onMouseMove(event);
                    return false;
                },
                "pointerup": function (event) {
                    if (this.options.readonly) return false;    // D4470
                    this._onMouseUp();
                    event.target.onpointermove = null;
                    this.options.pointerMoveTarget.onpointermove = null;
                    event.target.releasePointerCapture(event.pointerId);
                    return false;
                },
                "mouseout": function (event) {
                    if (this.options.readonly) return false;    // D4470
                    //this._onMouseUp();
                    //return false;
                    document.body.style.cursor = 'default';
                    this.redraw();
                },
                "touchend": function (event) {
                    if (this.options.readonly) return false;    // D4470
                    //this._onMouseUp();
                    //return false;
                },
                "wheel": function (event) {
                    if (this.options.readonly) return false;    // D4470
                    var Shift = event.originalEvent.deltaY / Math.abs(event.originalEvent.deltaY);
                    var newvscroll = opt.vscroll + Shift * opt.fontSize;
                    if (isNaN(newvscroll)) { newvscroll = opt.vscroll };
                    this.setNewVerticalScrollPosition(newvscroll);
                    this.redraw();
                    return false;
                }
            });
        },
        setNewVerticalScrollPosition: function (newvscroll) {
            var opt = this.options;
            if (newvscroll < 0) { newvscroll = 0 };
            var rowsAreaHeight = opt.canvas.height - opt.marginTop - opt.splitterWidth - opt.scrollBarWidth;
            if (opt.marginTop + opt.realRowAreaHeight - newvscroll * (opt.realRowAreaHeight / rowsAreaHeight) < opt.canvas.height) {
                newvscroll = (opt.realRowAreaHeight - opt.canvas.height + opt.marginTop) / (opt.realRowAreaHeight / rowsAreaHeight);
            };
            opt.vscroll = newvscroll;
            if (opt.marginTop + opt.realRowAreaHeight - opt.vscroll < opt.canvas.height) {
                opt.vscroll = opt.realRowAreaHeight - opt.canvas.height + opt.marginTop;
            };
            if (opt.vscroll < 0) { opt.vscroll = 0 };
        },
        setNewHorizontalScrollPosition: function (newhscroll) {
            var opt = this.options;
            if (newhscroll < 0) { newhscroll = 0 };
            var columnsAreaWidth = opt.canvas.width - opt.marginLeft - opt.scrollBarWidth - opt.splitterWidth;
            if (opt.marginLeft + opt.realColAreaWidth - newhscroll * (opt.realColAreaWidth / columnsAreaWidth) < opt.canvas.width) {
                newhscroll = (opt.realColAreaWidth - opt.canvas.width + opt.marginLeft) / (opt.realColAreaWidth / columnsAreaWidth);
            };
            opt.hscroll = newhscroll;
            if (opt.marginLeft + opt.realColAreaWidth - opt.hscroll < opt.canvas.width) {
                opt.hscroll = opt.realColAreaWidth - opt.canvas.width + opt.marginLeft;
            };
            if (opt.hscroll < 0) { opt.hscroll = 0 };
        },
        _calculateDGColumnsWidth: function () {
            var opt = this.options;
            var ctx = opt.context;
            var vi = 0;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                col.minwidth = opt.fontSize * 2;
                if (col.isTerminal == 1) {
                    for (var j = 0; j < opt.rows.length; j++) {
                        var row = opt.rows[j];
                        var val = row.dg[vi];
                        var width = ctx.measureText(val).width + opt.fontSize;
                        if (width > col.minwidth) { col.minwidth  = width};
                        if (width > opt.dgMinColumnWidth) { opt.dgMinColumnWidth = width };
                    };
                    vi++;
                };
            };
        },
        _calculateMargins: function () {
            var opt = this.options;
            var ctx = opt.context;
            opt.maxRowWidth = ctx.measureText(opt.rowsTitle).width;
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                var w = ctx.measureText(row.title).width;
                if (opt.maxRowWidth < w) { opt.maxRowWidth = w };
            };
            //if (opt.showRolesStats) {
            //    opt.maxRowWidth += opt.fontSize * 2
            //};
            if (opt.viewMode == 'datagrid') { opt.maxRowWidth -= opt.fontSize * 2};
            opt.terminalNodesCount = 0;
            opt.maxNodesLevel = 0;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.isTerminal == 1) {
                    var w = ctx.measureText(col.title).width;
                    if (opt.maxColWidth < w) { opt.maxColWidth = w };
                    opt.terminalNodesCount += 1;
                } else {
                    if (col.level > opt.maxNodesLevel) {
                        opt.maxNodesLevel = col.level;
                    };
                };
            };
            opt.minMarginTop = (opt.maxNodesLevel + 1) * opt.fontSize * 2;
            opt.marginLeft = opt.maxRowWidth + opt.fontSize * 3;
            if (opt.marginLeft > opt.canvas.width / 3) opt.marginLeft = opt.canvas.width / 3;
            opt.marginLeftOld = opt.marginLeft;
            opt.marginTop = opt.fontSize * 8 + opt.minMarginTop;
            opt.marginTopOld = opt.marginTop;
        },
        _drawSplitter: function (orientation, position, isSelected) {
            var opt = this.options;
            var ctx = opt.context;
            var splitterWidth = opt.splitterWidth;
            ctx.beginPath();
            if (isSelected) {
                ctx.fillStyle = opt.colors[4];
            } else {
                ctx.fillStyle = '#ffffff';
            };

            if (orientation == 1) {
                if (isSelected) document.body.style.cursor = 'col-resize';
                ctx.fillRect(position, 0, splitterWidth, opt.canvas.height);
                ctx.beginPath();
                ctx.fillStyle = opt.colors[2];
                var dw = opt.splitterWidth - 1;
                var dc = opt.marginTop / 2; //(opt.marginTop + opt.rows.length * opt.fontSize * 2 < opt.canvas.height ? opt.marginTop + (opt.rows.length * opt.fontSize * 2) / 2 : opt.canvas.height / 2);
                position += 1;
                ctx.fillRect(position, dc - dw * 2, dw, dw);
                ctx.fillRect(position, dc, dw, dw);
                ctx.fillRect(position, dc + dw * 2, dw, dw);
            } else {
                if (isSelected) document.body.style.cursor = 'row-resize';
                ctx.fillRect(0, position, opt.canvas.width, splitterWidth);
                ctx.beginPath();
                ctx.fillStyle = opt.colors[2];
                var dw = opt.splitterWidth - 1;
                var dc = opt.marginLeft / 2; //opt.canvas.width / 2;
                position += 1;
                ctx.fillRect(dc - dw * 2, position, dw, dw);
                ctx.fillRect(dc, position, dw, dw);
                ctx.fillRect(dc + dw * 2, position, dw, dw);
            };
        },
        _drawArrow: function (ctx, x, y, w, h, orientation) {
            ctx.beginPath();
            switch (orientation) {
                case 0:
                    ctx.moveTo(x, y + h);
                    ctx.lineTo(x + w / 2, y);
                    ctx.lineTo(x + w, y + h);
                    break;
                case 1:
                    ctx.moveTo(x, y);
                    ctx.lineTo(x + w, y);
                    ctx.lineTo(x + w / 2, y + h);
                    break;
                case 2:
                    ctx.moveTo(x + w, y);
                    ctx.lineTo(x + w, y + h);
                    ctx.lineTo(x, y + h / 2);
                    break;
                case 3:
                    ctx.moveTo(x, y);
                    ctx.lineTo(x, y + h);
                    ctx.lineTo(x + w, y + h / 2);
                    break;
            };
            ctx.fill();
        },
        _drawScrollBar: function (orientation, extSize, intSize, intPosition, isSelected) { 
            var opt = this.options;
            var ctx = opt.context;
            var iex, iey, iew, ieh;
            ctx.beginPath();
            ctx.strokeStyle = opt.colors[2];
            var backFill = opt.colors[4];
            var frontFill = opt.colors[3];
            if (isSelected) {
                frontFill = opt.colors[1];
            };
            if (orientation == 1) {
                iex = opt.canvas.width - opt.scrollBarWidth;
                iey = opt.marginTop + opt.splitterWidth;
                iew = opt.scrollBarWidth;
                ieh = extSize;
                ctx.moveTo(iex, iey);
                ctx.lineTo(iex, iey + ieh);
                ctx.stroke();
                ctx.fillStyle = '#ffffff';
                ctx.fillRect(iex, iey, iew, ieh + iew);
                ctx.fillStyle = opt.colors[0];
                this._drawArrow(ctx, iex, iey, iew, iew / 2, 0);
                this._drawArrow(ctx, iex, iey + ieh - iew / 2, iew, iew / 2, 1);
                ctx.fillStyle = frontFill;
                roundRect(ctx, iex, iey + intPosition + iew / 2 + 1, iew, ieh * intSize, iew / 2, true);
            } else {
                iex = opt.marginLeft + opt.splitterWidth;
                iey = opt.canvas.height - opt.scrollBarWidth;
                iew = extSize;
                ieh = opt.scrollBarWidth;
                ctx.moveTo(iex, iey);
                ctx.lineTo(iex + iew, iey);
                ctx.stroke();
                ctx.fillStyle = '#ffffff';
                ctx.fillRect(iex, iey, iew + ieh, ieh);
                ctx.beginPath();
                ctx.fillStyle = opt.colors[0];
                this._drawArrow(ctx, iex, iey, ieh / 2, ieh, 2);
                this._drawArrow(ctx, iex + iew - ieh / 2, iey, ieh / 2, ieh, 3);
                ctx.fillStyle = frontFill;
                roundRect(ctx, iex + intPosition + ieh / 2 + 1, iey, iew * intSize, ieh, ieh / 2, true);
            };
        },
        _getInteractiveMapElement: function (x, y) {
            for (var i = this.options.interactiveMap.length-1; i >= 0 ; i--) {
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
        _onMouseDown: function (event) {
            if (this.options.readonly) return false;    // D4470
            var opt = this.options;
            opt.isMouseDown = true;
            var pos = getRelativePosition(opt.canvas, event.clientX, event.clientY);
            opt.mouseDownPosition = pos;
            switch (opt.mouseOverElement.key) {
                case 'topSplitter':
                    opt.marginTopOld = opt.marginTop;
                    break;
                case 'leftSplitter':
                    opt.marginLeftOld = opt.marginLeft;
                    break;
                case 'vertScrollbar':
                    opt.vscrollOld = opt.vscroll;
                    break;
                case 'horScrollbar':
                    opt.hscrollOld = opt.hscroll;
                    break;
            };
            if (event.which == 1) {
                if (opt.mouseOverElement == '') {
                    var ieKey = this._getInteractiveMapElement(pos.x, pos.y);
                    opt.mouseOverElement = ieKey;
                };

                for (var i = 0; i < opt.rows.length; i++) {
                    var row = opt.rows[i];
                    var keyRowName = 'row' + row.nodeid;
                    if (opt.mouseOverElement.key == keyRowName) {
                        switch (opt.viewMode) {
                            case 'contributions':
                                var newCont = (this.getRowCheckboxState(row.idx) == 1 ? 0 : 1);
                                for (var j = 0; j < opt.columns.length; j++) {
                                    row.cont[j] = newCont;
                                };
                                this.updateObjCheckboxState();
                                this.redraw();
                                this.setRowContributions(row.nodeid, newCont);
                                break;
                            case 'infodocs':
                                opt.isMouseDown = false;
                                if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                                this.redraw();
                                showInfodoc('?type=2&field=infodoc&guid=' + row.id + '&callback=altInfodoc' + row.id, opt.infodocReadOnly);
                                break;
                            case 'roles':
                                opt.isMouseDown = false;
                                if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                                if (opt.showRolesMode == 0 && !opt.showRolesStats) {
                                    var arole = this.getRowRoleState(i).r;
                                    var nrole = 2;
                                    if (arole == 0) { nrole = 1 };
                                    if (arole == 1) { nrole = 2 };
                                    if (arole == 2) { nrole = 0 };
                                    if (arole == 3) { nrole = 1 };
                                    //row.role = nrole;
                                    var rowids = [row.nodeid];
                                    var colids = [];
                                    var terminalNodes = this.getTerminalNodes();
                                    for (var j = 0; j < terminalNodes.length; j++) {
                                        if (row.cont[j] == 1) {
                                            row.roles[j] = nrole;
                                            colids.push(terminalNodes[j].nodeid);
                                        };
                                    };
                                    this.setRoleRange(rowids, colids, nrole);
                                    this.updateRoles(false);
                                    this.updateObjCheckboxState();
                                    this.redraw();
                                };
                                break;
                        };
                    };
                    var vj = 0;
                    for (var j = 0; j < opt.columns.length; j++) {
                        var col = opt.columns[j];
                        if (col.isTerminal == 1) {
                            var keyName = 'cell' + col.nodeid + '-' + row.nodeid;
                            if (opt.mouseOverElement.key == keyName) {
                                switch (opt.viewMode) {
                                    case 'contributions':
                                        var cont = opt.rows[i].cont[vj];
                                        cont = (cont == 1 ? 0 : 1);
                                        opt.rows[i].cont[vj] = cont;
                                        this.updateObjCheckboxState();
                                        this.redraw();
                                        this.setCellContribution(row.id, col.id, cont);
                                        break;
                                    case 'infodocs':
                                        opt.isMouseDown = false;
                                        if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                                        this.redraw();
                                        showInfodoc('?type=3&field=infodoc&guid=' + row.id + '&pguid=' + col.id + '&callback=wrtInfodoc' + row.id + '_' + col.id, opt.infodocReadOnly);
                                        break;
                                    case 'roles':
                                        opt.isMouseDown = false;
                                        if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                                        if (opt.showRolesMode == 0 && !opt.showRolesStats) {
                                            var arole = opt.rows[i].roles[vj];
                                            var nrole = 2;
                                            if (arole !== -1) {
                                                if (arole == 0) { nrole = 1 };
                                                if (arole == 1) { nrole = 2 };
                                                if (arole == 2) { nrole = 0 };
                                                opt.rows[i].roles[vj] = nrole;
                                                this.setCellRoles(row.nodeid, col.nodeid, nrole);
                                                this.updateRoles(false);
                                                this.redraw();
                                            };
                                        };               
                                        break;
                                };

                            };
                            vj += 1;
                        };
                    };
                };
                for (var j = 0; j < opt.columns.length; j++) {
                    var col = opt.columns[j];           
                    var keyColName = 'col' + col.nodeid;
                    var keyObjName = 'obj' + col.nodeid;
                    if ((opt.mouseOverElement.key == keyColName) || (opt.mouseOverElement.key == keyObjName)) {
                        switch (opt.viewMode) {
                            case 'contributions':
                                var newCont = (col.checkstate == 1 ? 0 : 1);
                                if (col.isTerminal == 1) {
                                    col.checkstate = newCont;
                                    this.setColumnContributions(col.nodeid, newCont);
                                } else {
                                    this.setObjectiveContributionsRecursive(col, newCont);
                                };
                                sendCommand('action=setcol&col_id=' + col.nodeid + '&val=' + newCont, true);
                                this.updateObjCheckboxState();
                                this.redraw();
                                break;
                            case 'infodocs':
                                opt.isMouseDown = false;
                                if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                                this.redraw();
                                showInfodoc('?type=1&field=infodoc&guid=' + col.id + '&callback=objInfodoc' + col.id, opt.infodocReadOnly);
                                break;
                            case 'roles':
                                opt.isMouseDown = false;
                                if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                                if (opt.showRolesMode == 0 && !opt.showRolesStats) {
                                    var arole = col.role;
                                    var nrole = 2;
                                    if (arole == 0) { nrole = 1 };
                                    if (arole == 1) { nrole = 2 };
                                    if (arole == 2) { nrole = 0 };
                                    if (arole == 3) { nrole = 2 };

                                    col.role = nrole;
                                    col.frole = (nrole == 2 ? col.grole : nrole);
                                    if (col.isTerminal == 1) {
                                        this.setColumnRoles(col.nodeid, nrole);
                                    } else {
                                        this.setObjRoles(col.nodeid, nrole);
                                    };
                                    this.updateRoles(false);
                                    this.redraw();
                                };
                                break;
                        };
                        break;
                    };
                };
                if (opt.viewMode == 'datagrid' && opt.use_datamapping) {    // D4463
                    //-A1478 if (opt.hiddenAttributes !== "") {
                        for (var j = 0; j < opt.attributes.length; j++) {
                            var attr = opt.attributes[j];
                            if (this.isAttributeColumnVisible(attr.guid)) { //A1478
                                var keyAttr = 'attr' + attr.guid;
                                if (opt.mouseOverElement.key == keyAttr) {
                                    showDMOptions(attr);
                                    opt.isMouseDown = false;
                                    if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                                };
                            };

                        };
                    //-A1478 };

                    for (var j = 0; j < opt.columns.length; j++) {
                        var col = opt.columns[j];
                        if (opt.mouseOverElement.key == 'col' + col.idx) {
                            showDMOptions(col);
                            opt.isMouseDown = false;
                            if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                        };
                    };

                    if (opt.mouseOverElement.key == 'attr' + opt.dgColAltNameGuid) {
                        var attr = { guid: opt.dgColAltNameGuid, dm: opt.dgColAltDMGuid };
                        showDMOptions(attr);
                        opt.isMouseDown = false;
                        if (this.options.pointerMoveTarget) this.options.pointerMoveTarget.onpointermove = null;
                    };
                };
            };
        },
        setObjectiveContributionsRecursive: function (obj, contribution) {
            obj.checkstate = contribution;
            if (obj.isTerminal == 0) {
                var children = this.getNodeChildren(obj.nodeid);
                for (var c = 0; c < children.length; c++) {
                    var chld = children[c];
                    this.setObjectiveContributionsRecursive(chld, contribution);
                };
            } else {
                this.setColumnContributions(obj.nodeid, contribution);
            };
        },
        isAttributeColumnVisible: function (attrGuid) {
            var opt = this.options;
            return ((opt.showAttributes != null) && opt.showAttributes.indexOf(attrGuid) >= 0) || ((opt.hiddenAttributes != null) && opt.hiddenAttributes.indexOf(attrGuid) < 0);
        },
        setAttrDM: function (attrGuid, attrDMGuid) {
            var opt = this.options;
            for (var i = 0; i < opt.attributes.length; i++) {
                var attr = opt.attributes[i];
                if (attr.guid == attrGuid) {
                    attr.dm = attrDMGuid;
                    this.redraw();
                    return true;
                };
            };
            for (var i = 0; i < opt.columns.length; i++) { //'AS/24195===
                var col = opt.columns[i];
                if (col.guid == attrGuid) {
                    col.dm = attrDMGuid;
                    this.redraw();
                    return true;
                };
            }; //'AS/24195==
            return false;
        },
        _onMouseUp: function () {
            if (this.options.readonly) return false;    // D4470
            //if (document.releaseCapture) document.releaseCapture();
            this.options.isMouseDown = false;
            this.options.mouseOverElement = '';
            this.redraw();
        },
        _onMouseMove: function (e) {
            if (this.options.readonly) return false;    // D4470
            var opt = this.options;
            var pos = getRelativePosition(opt.canvas, e.clientX, e.clientY);
            var oldIE = opt.mouseOverElement;
            var needredraw = false;
            if (!opt.isMouseDown) {
                var ieKey = this._getInteractiveMapElement(pos.x, pos.y);
                opt.mouseOverElement = ieKey;
                if (ieKey == '') {
                    document.body.style.cursor = 'default';
                } else {
                    if (((ieKey.key.indexOf('col') >= 0) || (ieKey.key.indexOf('row') >= 0) || (ieKey.key.indexOf('cell') >= 0) || (ieKey.key.indexOf('obj') >= 0) || (ieKey.key.indexOf('attr') >= 0)) && (opt.viewMode !== 'datagrid')) document.body.style.cursor = 'pointer';
                    if ((ieKey.key.indexOf('attr') >= 0) && (opt.viewMode == 'datagrid')) document.body.style.cursor = 'pointer';
                    if ((ieKey.key.indexOf('col') >= 0) && (opt.viewMode == 'datagrid') && (opt.use_datamapping)) document.body.style.cursor = 'pointer';
                };
            } else {

            };
            if (oldIE.key != opt.mouseOverElement.key) {
                needredraw = true;
            };
            if (needredraw) {
                this.redraw();
            };
        },
        drawCheckbox: function (ctx, x, y, size, state) {
            var opt = this.options;
            ctx.beginPath();
            ctx.lineWidth = 0.5;
            ctx.strokeStyle = opt.colors[3];
            ctx.fillStyle = '#ffffff';
            ctx.rect(x - size / 2, y - size / 2, size, size);
            ctx.fill();
            ctx.stroke();
            ctx.fillStyle = opt.colors[5];
            ctx.beginPath();
            var nx = x - size / 2 + size / 6;
            var ny = y - size / 2 + size / 6;
            var nw = size - size / 3;
            if (state == 1) {
                ctx.fillRect(nx, ny, nw, nw);
            };
            if (state == 2) {
                ctx.moveTo(nx, ny);
                ctx.lineTo(nx + nw, ny + nw);
                ctx.lineTo(nx, ny + nw);
                ctx.fill();
            };
        },
        drawRolebox: function (ctx, x, y, size, role, grouprole, stat, showRoleMode, frole) {
            //showRoleMode:
            //0 - role + grouprole
            //1 - final role
            //2 - stats

            //roles:
            //-1 - no contributions
            //0 - restricted
            //1 - allowed
            //2 - "undefined"
            //3 - mixed
            if (role !== -1) {
                var opt = this.options;
                ctx.save();
                ctx.font = (opt.fontSize - 2) + 'px Arial';
                ctx.beginPath();
                if (showRoleMode == 2) {
                    ctx.fillStyle = '#000000';
                    //if (stat.indexOf('/0') >= 0) { ctx.fillStyle = '#990000'; };
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    var statVal = (stat.indexOf('-1') >= 0 ? 'N/A' : stat);
                    ctx.fillText(statVal, x, y);
                } else {
                    ctx.fillStyle = '#ffffff';
                    var nx = x - size / 2 + size / 6 - 1;
                    var ny = y - size / 2 + size / 6 - 1;
                    var nw = size - size / 3 + 2;
                    var nx2 = nx - 3;
                    var ny2 = ny - 3;
                    var nw2 = nw + 6;
                    ctx.rect(nx2, ny2, nw2, nw2);
                    ctx.fill();
                    if (opt.isForGroups || showRoleMode == 1) { nx = nx2; ny = ny2; nw = nw2 };
                    if (!opt.isForGroups) {
                        switch (grouprole) {
                            case 0:
                                ctx.strokeStyle = '#ff8888';
                                ctx.fillStyle = '#ff8888';
                                break;
                            case 1:
                                ctx.strokeStyle = '#88ff88';
                                ctx.fillStyle = '#88ff88';
                                break;
                            case 2:
                                ctx.strokeStyle = '#888888';
                                ctx.fillStyle = '#888888';
                                break;
                            case 3:
                                ctx.strokeStyle = '#ffff00';
                                ctx.fillStyle = '#ffff00';
                                break;
                            default:
                        };
                        ctx.fill();
                    };
                    ctx.beginPath();
                    var viewRole = role;
                    if (showRoleMode == 1) { viewRole = frole };
                    switch (viewRole) {
                        case 0:
                            ctx.fillStyle = '#bb0000';
                            ctx.fillRect(nx, ny, nw, nw);
                            break;
                        case 1:
                            ctx.fillStyle = '#00bb00';
                            ctx.fillRect(nx, ny, nw, nw);
                            break;
                        case 2:
                            if (opt.isForGroups) {
                                ctx.strokeStyle = '#888888';
                                ctx.fillStyle = '#888888';
                                ctx.fillRect(nx, ny, nw, nw);
                            };
                            break;
                        case 3:
                            ctx.fillStyle = '#eeee00';
                            ctx.fillRect(nx, ny, nw, nw);
                            break;
                        default:
                    };
                    
                };
                ctx.restore();
            };
        },
        drawInfodoc: function (ctx, x, y, size, state) {
            var opt = this.options;
            ctx.save();
            ctx.beginPath();
            ctx.lineWidth = 1;
            ctx.strokeStyle = opt.colors[3];
            //ctx.fillStyle = '#ffffff';
            var dx = x - size / 2;
            var dy = y - size / 2;
            var dw = size * 0.9;
            var dh = size;
            var d4 = dw / 4;
            if (state == 1) { ctx.fillStyle = "#539ddd" } else { ctx.fillStyle = opt.colors[3] };
            //ctx.arc(x, y, size / 2 + 1, 0, Math.PI * 2, true);
            ctx.textAlign = "center";
            //ctx.font = 'Bold 14px Courier New';
            //ctx.fillText('i', x, y);
            ctx.font = '14px FontAwesome';
            ctx.fillText("\uf05a", x, y);
            //if (state == 1) {
            //    ctx.fill();
            //    ctx.fillStyle = "#ffffff";
            //} else {
            //    ctx.stroke();
            //    ctx.fillStyle = opt.colors[3];
            //};

            //ctx.moveTo(dx, dy);
            //ctx.lineTo(dx + dw - d4, dy);
            //ctx.lineTo(dx + dw, dy + d4);
            //ctx.lineTo(dx + dw, dy + dh);
            //ctx.lineTo(dx, dy + dh);
            //ctx.lineTo(dx, dy);
            //ctx.fill();
            //ctx.stroke();
            //if (state == 1) {
            //    ctx.beginPath();
            //    ctx.lineWidth = 2;
            //    dy += d4 / 4;
            //    for (var i = 0; i < 3; i++) {
            //        ctx.moveTo(dx + d4, dy + d4);
            //        ctx.lineTo(dx + dw - d4, dy + d4);
            //        dy += d4;
            //    };
            //    ctx.stroke();
            //};
            ctx.restore();
        },
        drawDataMapping: function (ctx, x, y, size, state) {
            var opt = this.options;
            if (opt.use_datamapping && (state > -1)) {  // D4463
                ctx.save();
                ctx.beginPath();
                ctx.fillStyle = opt.colors[5];
                if (state == 0) {
                    ctx.fillStyle = opt.selStyle;
                };
                ctx.font = '10px Arial';
               // ctx.font = 'bold ' + (opt.fontSize + 4) + 'px Calibri';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText('DM', x, y);
                ctx.restore();
            }
        },
        drawMeasurementType: function (ctx, x, y, size, mt) {
            var opt = this.options;
            ctx.save();
            ctx.beginPath();
            ctx.fillStyle = opt.colors[5];
            ctx.font = '10px Arial';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'middle';
            ctx.fillText(mt, x, y);
            ctx.restore();
        },
        getRowCheckboxState: function (rowIdx) {
            var opt = this.options;
            var row = opt.rows[rowIdx];
            var isAllChecked = true;
            var isAllUnchecked = true;
            var startIdx = 0;
            if (opt.showNoSources) { startIdx = 1 };
            for (var j = startIdx; j < row.cont.length; j++) {
                if (row.cont[j] == 1) {
                    isAllUnchecked = false;
                } else {
                    isAllChecked = false;
                };
                if ((!isAllChecked) && (!isAllUnchecked)) { return 2};
            };
            if (isAllChecked) return 1;
            if (isAllUnchecked) return 0;
        },
        getRowRoleState: function (rowIdx) {
            var opt = this.options;
            var row = opt.rows[rowIdx];
            var role = 2;
            var grole = 2;
            var frole = 2;
            var isAllAllowed = true;
            var isAllRestricted = true;
            var isAllUndefined = true;
            for (var j = 0; j < row.roles.length; j++) {
                if (row.cont[j] == 1) {
                    if (row.roles[j] == 0) { isAllAllowed = false; isAllUndefined = false; };
                    if (row.roles[j] == 1) { isAllRestricted = false; isAllUndefined = false; };
                    if (row.roles[j] == 2) { isAllRestricted = false; isAllAllowed = false; };
                    if (row.roles[j] == 3) { isAllAllowed = false; isAllUndefined = false; isAllRestricted = false; };
                }
            };
            if (isAllAllowed) { role = 1 }
            else {
                if (isAllRestricted) { role = 0 } else {
                    if (isAllUndefined) { role = 2 } else {
                        role = 3
                    }
                }
            };

            isAllAllowed = true;
            isAllRestricted = true;
            isAllUndefined = true;
            for (var j = 0; j < row.froles.length; j++) {
                if (row.cont[j] == 1) {
                    if (row.froles[j] == 0) { isAllAllowed = false; isAllUndefined = false; };
                    if (row.froles[j] == 1) { isAllRestricted = false; isAllUndefined = false; };
                    if (row.froles[j] == 2) { isAllRestricted = false; isAllAllowed = false; };
                    if (row.froles[j] == 3) { isAllAllowed = false; isAllUndefined = false; isAllRestricted = false; };
                };
            };
            if (isAllAllowed) { frole = 1 }
            else {
                if (isAllRestricted) { frole = 0 } else {
                    if (isAllUndefined) { frole = 2 } else {
                        frole = 3
                    }
                }
            };

            var isAllAllowed = true;
            var isAllRestricted = true;
            for (var j = 0; j < row.groles.length; j++) {
                if (row.cont[j] == 1) {
                    if (row.groles[j] == 0) { isAllAllowed = false; };
                    if (row.groles[j] == 1) { isAllRestricted = false; };
                    if (row.groles[j] == 3) { isAllAllowed = false; isAllRestricted = false; };
                };
            };
            if (isAllAllowed) { grole = 1 } else {
                if (isAllRestricted) { grole = 0 } else {
                    grole = 3
                }
            };
            return {r:role,g:grole,f:frole}
        },
        getColCheckboxState: function (colIdx) {
            var opt = this.options;
            var isAllChecked = true;
            var isAllUnchecked = true;
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                if (row.cont[colIdx] == 1) {
                    isAllUnchecked = false;
                } else {
                    isAllChecked = false;
                };
                if ((!isAllChecked) && (!isAllUnchecked)) { return 2 };
            };
            if (isAllChecked) return 1;
            if (isAllUnchecked) return 0;
        },
        updateObjCheckboxState: function () {
            var opt = this.options;
            var terminalNodes = this.getTerminalNodes();
            for (var j = 0; j < terminalNodes.length; j++) {
                var tnode = terminalNodes[j];
                var isAllChecked = true;
                var isAllUnchecked = true;
                for (var i = 0; i < opt.rows.length; i++) {
                    var row = opt.rows[i];
                    if (row.cont[j] == 1) {
                        isAllUnchecked = false;
                    } else {
                        isAllChecked = false;
                    };
                };
                var obj = this.getNodeByID(tnode.nodeid);
                obj.checkstate = 2;
                if (isAllChecked) obj.checkstate = 1;
                if (isAllUnchecked) obj.checkstate = 0;
            };
            for (var j = opt.columns.length - 1; j >= 0; j--) {
                var obj = opt.columns[j];
                if (obj.isTerminal == 0) {
                    var isAllChecked = true;
                    var isAllUnchecked = true;
                    var children = this.getNodeChildren(obj.nodeid);
                    for (var i = 0; i < children.length; i++) {
                        var ch = children[i];
                        if (ch.checkstate == 1) isAllUnchecked = false;
                        if (ch.checkstate == 0) isAllChecked = false;
                        if (ch.checkstate == 2) { isAllChecked = false; isAllUnchecked = false; };
                    };
                    obj.checkstate = 2;
                    if (isAllChecked) obj.checkstate = 1;
                    if (isAllUnchecked) obj.checkstate = 0;
                };
            };
        },
        setCellRoleStat: function(colID,rowID,newStat) {
            var opt = this.options;
            var row = this._getRowByNodeID(rowID);
            var nidx = this._getColIndexByNodeID(colID);
            row.stat[nidx] = newStat;
        },
        updateRoles: function (skipStatUpdate) {
            var opt = this.options;
            var checkedUsersCount = opt.checkedUsers.length;
            var terminalNodes = this.getTerminalNodes();
            for (var j = 0; j < terminalNodes.length; j++) {
                var tnode = terminalNodes[j];
                var isAllAllowed = true;
                var isAllRestricted = true;
                var isAllUndefined = true;
                var isGroupAllAllowed = true;
                var isGroupAllRestricted = true;
                var isFinalAllAllowed = true;
                var isFinalAllRestricted = true;
                for (var i = 0; i < opt.rows.length; i++) {
                    var row = opt.rows[i];
                    if (row.cont[j] == 1) {
                        if (row.roles[j] == 0) {
                            isAllAllowed = false;
                            isAllUndefined = false;
                            row.froles[j] = 0;
                        };
                        if (row.groles[j] == 0) {
                            isGroupAllAllowed = false;
                        };
                        if (row.roles[j] == 1) {
                            isAllRestricted = false;
                            isAllUndefined = false;
                            row.froles[j] = 1;
                        };
                        if (row.groles[j] == 1) {
                            isGroupAllRestricted = false;
                        };
                        if (row.roles[j] == 2) {
                            isAllRestricted = false;
                            isAllAllowed = false;
                            row.froles[j] = row.groles[j];
                        };
                        if (row.roles[j] == 3) {
                            isAllRestricted = false;
                            isAllAllowed = false;
                            isAllUndefined = false;
                        };
                        if (row.groles[j] == 3) {
                            isGroupAllAllowed = false;
                            isGroupAllRestricted = false;
                        };
                        if (row.froles[j] == 3) {
                            isFinalAllRestricted = false;
                            isFinalAllAllowed = false;
                        };
                        if (row.froles[j] == 0) {
                            isFinalAllAllowed = false;
                        };
                        if (row.froles[j] == 1) {
                            isFinalAllRestricted = false;
                        };
                    };
                };
                var obj = this.getNodeByID(tnode.nodeid);
                obj.role = 3;
                if (isGroupAllAllowed) { obj.grole = 1 } else {
                    if (isGroupAllRestricted) { obj.grole = 0 } else {
                        obj.grole = 3;
                    };      
                };
                if (isAllAllowed) obj.role = 1;
                if (isAllRestricted) obj.role = 0;
                if (isAllUndefined) obj.role = 2;
                if (obj.role == 2) { obj.frole = obj.grole } else { obj.frole = obj.role };
                if (isFinalAllAllowed) obj.frole = 1;
                if (isFinalAllRestricted) obj.frole = 0;
                if (!isFinalAllAllowed && !isFinalAllRestricted) obj.frole = 3;
            };
        },
        getNodeChildren: function (nodeID) {
            var children = [];
            var opt = this.options;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.parentnodeid == nodeID) { children.push(col) };
            };
            return children;
        },
        getTerminalNodes: function () {
            var nodes = [];
            var opt = this.options;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.isTerminal == 1) nodes.push(col);
            };
            return nodes;
        },
        getNodeByID: function (nodeID) {
            var opt = this.options;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.nodeid == nodeID) { return col };
            };
            return null;
        },
        drawMultilineText: function (ctx, text, x, y, w, h) {
            var opt = this.options;
            var words = text.split(' ');
            var lines = [];
            var aline = '';
            for (var i = 0; i < words.length; i++) {
                if (ctx.measureText(aline + (aline == '' ? '' : ' ') + words[i]).width < w) {
                    aline += (aline == '' ? '' : ' ') + words[i];
                } else {
                    lines.push(aline);
                    aline = words[i];
                };
            };
            lines.push(aline);
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i];
                ctx.fillText(line, x, y + i * opt.fontSize - opt.fontSize * (lines.length - 1));
            };
        },
        drawHint: function (ctx, text, rect) {
            var x = rect[0] + rect[2];
            var y = rect[1];
            var opt = this.options;
            var maxW = 0;
            var margins = 15;
            var lineH = opt.fontSize * 1.5;
            var lines = text.split('//');
            var maxH = lineH * lines.length + opt.fontSize / 2;
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i];
                var linew = ctx.measureText(line).width;
                if (maxW < linew) { maxW = linew };
            };
            if (x + maxW + margins > opt.canvas.width) {
                x = rect[0] - (maxW + margins);
            };
            if (y + maxH > opt.canvas.height) {
                y = opt.canvas.height - maxH;
            };
            ctx.save();
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
                ctx.fillText(line, x + margins / 2, y + opt.fontSize / 2 + i * lineH);
            };
            ctx.restore();
        },
        getAttributesColumnsWidth: function() {
            var opt = this.options;
            var result = 0;
            //-A1478 if (opt.hiddenAttributes !== "") {
                for (var i = 0; i < opt.attributes.length; i++) {
                    if (this.isAttributeColumnVisible(opt.attributes[i].guid)) result += opt.attributes[i].width;
                };
            //-A1478};
            return result;
        },
        getDGColumnsWidth: function () {
            var opt = this.options;
            var result = 0;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.isTerminal == 1) {
                    result += opt.columns[i].width;
                };
            };
            return result;
        },
        updateAttributesColumnWidth: function () {
            var opt = this.options;
            for (var i = 0; i < opt.attributes.length; i++) {
                opt.attributes[i].width = this.calculateAttributeColWidth(i);
            };
        },
        calculateAttributeColWidth: function (attrID) {
            var result = 50;
            var opt = this.options;
            var ctx = opt.context;
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                var valW = ctx.measureText(row.attrvals[attrID]).width + opt.fontSize / 2;
                if (result < valW) result = valW;
            };
            if (result > 150) result = 150;
            return result;
        },
        updateDGColumnWidth: function () {
            var opt = this.options;
            var vi = 0;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.isTerminal == 1) {
                    opt.columns[i].width = this.calculateDGColWidth(vi);
                };
            };
        },
        calculateDGColWidth: function (colID) {
            var opt = this.options;
            var result = opt.fontSize * 2;
            var ctx = opt.context;
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                var valW = ctx.measureText(row.dg[colID]).width + opt.fontSize;
                if (result < valW) result = valW;
            };
            if (result > 150) result = 150;
            return result;
        },
        drawCell: function (ctx, intMap, x, y, w, h, value, valueType, keyName, hint, isEven, isSelected, isWarning) {
            var opt = this.options;
            var fontSize = opt.fontSize;
            var fontSize2 = fontSize * 2;
            var iex, iey, iew, ieh, intEl;
            ctx.fillStyle = (isEven ? opt.evenStyle : opt.oddStyle);
            if (isSelected) { ctx.fillStyle = opt.selStyle };
            //if (isWarning && (opt.viewMode == "roles") && (!opt.isForGroups)) { ctx.fillStyle = opt.warningStyle };


            ctx.beginPath();
            ctx.rect(x + 1, y, w - 2, h);
            ctx.fill();

            if ((valueType == 'infodocs') && (isSelected)) {
                this.drawInfodoc(ctx, x + w / 2, y + h / 2, fontSize, 1);
            };
            if ((opt.viewMode !== 'infodocs') || !(opt.infodocReadOnly && !value)) {
                if (!(opt.viewMode == 'contributions' && !opt.showContributions)) {
                    if (opt.viewMode == 'roles' && opt.showRolesStats) { } else {
                        intEl = { key: keyName, rect: [x, y + h / 2 - fontSize, w, fontSize2], hint: hint };
                        intMap.push(intEl);
                    }
                }
            };
            ctx.save();
            ctx.beginPath();
            ctx.rect(x + fontSize / 4, y + fontSize / 4, w - fontSize / 2, h - fontSize / 2);
            ctx.clip();
            ctx.beginPath();
            ctx.fillStyle = '#000000';
            if (typeof value !== 'undefined') {
                switch (valueType) {
                    case 'avtLong':
                        ctx.textAlign = 'right';
                        ctx.fillText(value, x + w - fontSize / 4, y + h / 2);
                        break;
                    case 'avtDouble':
                        ctx.textAlign = 'right';
                        //var txtValue = (typeof (value) == 'string' ? value : value.toFixed(opt.valDigits));
                        var txtValue = value;
                        ctx.fillText(txtValue, x + w - fontSize / 4, y + h / 2);
                        break;
                    case 'avtBoolean':
                        ctx.textAlign = 'center';
                        ctx.fillText(value, x + w / 2, y + h / 2);
                        break;
                    case 'infodocs':
                        this.drawInfodoc(ctx, x + w / 2, y + fontSize, fontSize, value);      
                        break;
                    case 'contributions':
                        if (opt.showContributions) {
                            this.drawCheckbox(ctx, x + w / 2, y + fontSize, fontSize, value);
                        };
                        break;
                    case 'roles':
                        var nx = x + w / 2;
                        //if (opt.showRolesStats) {
                        //    nx = x + fontSize;
                        //};
                        var roleMode = opt.showRolesMode;
                        if (opt.showRolesStats) { roleMode = 2};
                        this.drawRolebox(ctx, nx, y + fontSize, fontSize * 1.5, value[0], value[1], value[2], roleMode, value[3]);
                        break;
                    case 'datagrid':
                        ctx.fillStyle = '#000000';
                        ctx.textAlign = 'right';
                        var dgval = value;
                        if (typeof dgval !== 'undefined') {
                            var dgvalstr = '';
                            switch (typeof dgval) {
                                case 'number':
                                    dgvalstr = dgval.toString();
                                    break;
                                case 'string':
                                    dgvalstr = dgval;
                                    break;
                            };
                            ctx.fillText(dgvalstr, x + w - fontSize / 2, y + fontSize);
                        };
                        break;
                    default:
                        ctx.textAlign = 'left';
                        ctx.fillText(value, x + fontSize / 4, y + h / 2);
                };
            };
            ctx.restore();
        },
        drawColumnHeader: function (ctx, intMap, x, y, w, h, text, isMouseOver) {

        },
        calculateVisualIndicesForColumns: function () {
            var opt = this.options;
            //calculate levelshifts for each node in hierarchy which level less than hierarchy depth of tree
            var skippedLevelShift = [];
            for (var j = 0; j <= opt.maxNodesLevel; j++) {
                var levelShift = 0;
                for (var i = 0; i < opt.columns.length; i++) {
                    var col = opt.columns[i];
                    if (col.level == j) {
                        if (col.isTerminal == 0 || col.level <= opt.maxNodesLevel) {
                            while (skippedLevelShift.indexOf(levelShift) >= 0) {
                                levelShift++
                            };
                            col.levelShift = levelShift;
                            if (col.isTerminal == 1) { skippedLevelShift.push(levelShift) };
                        };
                        levelShift += col.terminalnodescount;
                    }
                }
            }

            //add skipped visual index for terminal nodes which level less than hierarchy depth of tree
            var skippedVi = [];
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.isTerminal == 1 && col.level <= opt.maxNodesLevel) {
                    skippedVi.push(col.levelShift);
                };
            };

            vi = 0;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.isTerminal == 1) {
                    if (skippedVi.indexOf(vi) >= 0) { vi++ }; //skip visual index for terminal nodes with higher levels
                    var visualIndex = vi;
                    if (col.level <= opt.maxNodesLevel) { visualIndex = col.levelShift; vi-- }; //use node levelshif as visual index and don't increment vi index
                    col.visualIndex = visualIndex;
                    vi += 1;
                };
            };
        },
        redraw: function () {
            var opt = this.options;
            opt.redrawCount += 1;
            var intMap = []; 
            var iex, iey, iew, ieh, intEl;
            var ctx = opt.context;
            var fontSize = opt.fontSize;
            var fontSize2 = fontSize * 2;
            ctx.beginPath();
            ctx.fillStyle = '#ffffff';
            ctx.fillRect(0, 0, opt.canvas.width, opt.canvas.height);
            ctx.textBaseline = 'middle';
            ctx.fillStyle = '#000000';
            if (opt.viewMode == "roles" && opt.checkedUsers.length == 0) {
                ctx.textAlign = 'center';
                var warningMessage = 'No Items Selected';
                ctx.fillText(warningMessage, opt.canvas.width / 2, opt.canvas.height / 2);
                return;
            };
            var marginLeft = opt.marginLeft;
            var marginTop = opt.marginTop;
            var gridX = marginLeft + opt.splitterWidth;
            var gridY = marginTop + opt.splitterWidth;
            var gridW = opt.canvas.width - gridX;
            var gridH = opt.canvas.height - gridY;
            var needShowVerticalScrollbar = false;
            var needShowHorizontalScrollbar = false;

            var rowsAreaHeight = opt.canvas.height - marginTop - opt.splitterWidth - opt.scrollBarWidth;
            var rowHeight = fontSize2;
            opt.realRowAreaHeight = rowsAreaHeight;
            if (rowsAreaHeight / opt.rows.length < fontSize2) {
                needShowVerticalScrollbar = true;
                opt.realRowAreaHeight = rowHeight * opt.rows.length + opt.scrollBarWidth * 2;
            };

            this.calculateVisualIndicesForColumns();
            var mtop = marginTop + opt.splitterWidth - opt.vscroll * (opt.realRowAreaHeight / rowsAreaHeight);
            var vi = 0;
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                if (!opt.showMissedRoles || this.isWarningRow(row)) {
                    var y = mtop + rowHeight * vi;
                    if ((y > marginTop + opt.splitterWidth - rowHeight) && (y < opt.canvas.height)) {            
                        ctx.beginPath();
                        if (vi % 2 === 0)
                        { ctx.fillStyle = opt.evenStyle }
                        else
                        { ctx.fillStyle = opt.oddStyle };
                        if (this.isWarningRow(row)) {ctx.fillStyle = opt.warningStyle };
                        ctx.fillRect(0, y, marginLeft + fontSize, rowHeight);
                        ctx.beginPath();
                        ctx.fillStyle = '#000000';
                        var lmargins = fontSize2;
                        if (opt.viewMode == 'datagrid') { lmargins = fontSize / 2};
                        //if (opt.showRolesStats) { lmargins = fontSize2 * 2 };
                        ctx.fillText(row.title, lmargins, y + rowHeight / 2);
                        var keyName = 'row' + row.nodeid;
                        iex = fontSize;
                        iey = y + rowHeight / 2;
                        if (opt.mouseOverElement.key == keyName) {
                            ctx.fillStyle = opt.selStyle;
                            ctx.beginPath();
                            ctx.fillRect(0, y, fontSize2, rowHeight);
                            if (opt.viewMode == 'infodocs') {
                                this.drawInfodoc(ctx, iex, iey, iex, 1);
                            };
                        };

                        switch (opt.viewMode) {
                            case 'infodocs':
                                if (!(opt.infodocReadOnly && !this.isNodeInfodoc(row.id))) {
                                    intEl = { key: keyName, rect: [0, y, fontSize2, rowHeight], hint: row.title };
                                    intMap.push(intEl);
                                };
                                this.drawInfodoc(ctx, iex, iey, iex, (this.isNodeInfodoc(row.id) ? 1 : 0));
                                break;
                            case 'contributions':
                                if (opt.showContributions) {
                                    intEl = { key: keyName, rect: [0, y, fontSize2, rowHeight], hint: row.title };
                                    intMap.push(intEl);
                                    this.drawCheckbox(ctx, iex, iey, iex, this.getRowCheckboxState(i));
                                }
                                break;
                            case 'roles':
                                if (opt.rolesMode !== 'objs') {

                                    var roleMode = opt.showRolesMode;
                                    if (opt.showRolesStats) { roleMode = 2 };
                                    var rowRole = this.getRowRoleState(i);
                                    if (this.getRowCheckboxState(i) !== 0) {
                                        if (opt.viewMode == 'roles' && opt.showRolesStats) { } else {
                                            intEl = { key: keyName, rect: [0, y, fontSize2, rowHeight], hint: row.title };
                                            intMap.push(intEl);
                                        };
                                        this.drawRolebox(ctx, iex, iey, fontSize * 1.5, rowRole.r, rowRole.g, '', roleMode, rowRole.f);
                                    }           
                                };
                                break;
                        };
                    };
                    vi += 1;
                };
            };

            var columnsAreaWidth = opt.canvas.width - marginLeft - opt.scrollBarWidth - opt.splitterWidth;
            var columnWidth = (columnsAreaWidth - this.getAttributesColumnsWidth()) / opt.terminalNodesCount;
            opt.realColAreaWidth = columnsAreaWidth;
            var minColumnWidth = fontSize2;
            //if ((opt.showRolesStats)&&(opt.viewMode == 'roles')) { minColumnWidth = fontSize2 * 2 };
            if (opt.viewMode == 'datagrid') { minColumnWidth = opt.dgMinColumnWidth };
            if (columnWidth < minColumnWidth) {
                columnWidth = minColumnWidth;
                needShowHorizontalScrollbar = true;
                opt.realColAreaWidth = columnWidth * opt.terminalNodesCount + this.getAttributesColumnsWidth() + opt.scrollBarWidth * 2;
            };
            //draw grid content
            ctx.beginPath();
            ctx.fillStyle = '#ffffff';
            ctx.fillRect(gridX, gridY, gridW, gridH);
            ctx.save();
            ctx.rect(gridX, gridY, gridW, gridH);
            ctx.clip();
            var vi = 0;
            var hscroll = opt.hscroll * (opt.realColAreaWidth / columnsAreaWidth);
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                if (!opt.showMissedRoles || this.isWarningRow(row)) {
                    var y = gridY + rowHeight * vi - opt.vscroll * (opt.realRowAreaHeight / rowsAreaHeight);
                    if ((y > gridY - rowHeight) && (y < opt.canvas.height)) {
                        var vj = 0;
                        var attrsWidth = 0;
                        //-A1478 if (opt.hiddenAttributes !== "") {
                        for (var j = 0; j < opt.attributes.length; j++) {
                            var attr = opt.attributes[j];
                            if (this.isAttributeColumnVisible(attr.guid)) { //A1478
                                var x = gridX + attrsWidth - hscroll;
                                attrsWidth += attr.width;
                                if ((x > marginLeft - attr.width) && (x < opt.canvas.width)) {
                                    var isEven = (vi % 2 === 0);
                                    var keyName = 'attr' + attr.guid + '*' + row.nodeid;
                                    var isCellSelected = false;
                                    if (opt.mouseOverElement.key == keyName) {
                                        isCellSelected = true;
                                    };
                                    var keyRowName = 'row' + row.nodeid;
                                    if (opt.mouseOverElement.key == keyRowName) {
                                        isCellSelected = true;
                                    };
                                    var hint = row.attrvals[j] + '//' + attr.name + '//' + row.title;
                                    var value = row.attrvals[j];
                                    var valueType = attr.type;
                                    this.drawCell(ctx, intMap, x, y, attr.width, rowHeight, value, valueType, keyName, hint, isEven, isCellSelected, this.isWarningRow(row));
                                };
                                vj += 1;
                            };
                        };
                        //-A1478};

                        vj = 0;
                        for (var j = 0; j < opt.columns.length; j++) {
                            var col = opt.columns[j];
                            var x = gridX + attrsWidth + columnWidth * col.visualIndex - hscroll;
                            if (col.isTerminal == 1) {
                                if ((x > marginLeft - columnWidth) && (x < opt.canvas.width)) {
                                    var isEven = (vi % 2 === 0);

                                    var keyName = 'cell' + col.nodeid + '-' + row.nodeid;
                                    var keyRowName = 'row' + row.nodeid;
                                    var keyColName = 'col' + col.nodeid;

                                    var hint = row.title + '//' + col.title;
                                    var isCellSelected = false;
                                    var value = '';

                                    var cont = row.cont[vj];
                                    var role = row.roles[vj];
                                    var grole = row.groles[vj];
                                    var stat = row.stat[vj];
                                    var frole = row.froles[vj];
                                    if (((opt.mouseOverElement.key == keyName) || (opt.mouseOverElement.key == keyRowName) || (opt.mouseOverElement.key == keyColName)) && (opt.viewMode !== 'infodocs')) {
                                        if ((opt.viewMode !== 'roles') || (cont == 1)) {
                                            isCellSelected = true;
                                        };
                                    };
                                    if ((opt.viewMode == 'infodocs') && (opt.mouseOverElement.key == keyName)) {
                                        isCellSelected = true;
                                    };
                                    switch (opt.viewMode) {
                                        case 'infodocs':
                                            if (cont == 1) {
                                                value = this.isWRTInfodoc(row.nodeid, col.nodeid);
                                                valueType = 'infodocs';
                                            } else {
                                                value = '';
                                                valueType = 'avtString';
                                                keyName = '';
                                                hint = '';
                                            };
                                            break;
                                        case 'contributions':
                                            value = cont;
                                            valueType = 'contributions';
                                            break;
                                        case 'roles':
                                            if (cont == 1) {
                                                value = [role, grole, stat, frole];
                                                valueType = 'roles';
                                            } else {
                                                value = '';
                                                valueType = 'avtString';
                                                keyName = '';
                                                hint = '';
                                            };
                                            break;
                                        case 'datagrid':
                                            var dgval = row.dg[vj];
                                            value = dgval;
                                            valueType = 'datagrid';
                                            if (typeof value !== 'undefined') {
                                                hint = value + '//' + hint;
                                            };
                                            break;
                                    };
                                    var isHighLight = false;
                                    //if (stat !== undefined) {
                                    //    isHighLight = (stat.indexOf('/0') > -1);
                                    //};
                                    if (opt.viewMode !== "roles" || opt.rolesMode !== 'objs') {
                                        this.drawCell(ctx, intMap, x, y, columnWidth, rowHeight, value, valueType, keyName, hint, isEven, isCellSelected, isHighLight);
                                    };
                                };
                                vj += 1;
                            };
                        };
                    };
                    vi += 1;
                };
            };
            ctx.restore();
            //draw attributes columns headers
            ctx.beginPath();
            ctx.fillStyle = '#ffffff';
            ctx.fillRect(0, 0, opt.canvas.width, opt.marginTop);
            ctx.save();
            ctx.translate(marginLeft + opt.splitterWidth, marginTop);
            ctx.rotate(-Math.PI / 2);
            ctx.rect(0, 0, marginTop, opt.canvas.width - marginLeft);
            ctx.clip();
            var vi = 0;
            var attrsWidth = 0;
            //-A1478 if (opt.hiddenAttributes !== "") {
                for (var i = 0; i < opt.attributes.length; i++) {
                    var attr = opt.attributes[i];
                    if (this.isAttributeColumnVisible(attr.guid)) { //A1478
                        var y = attrsWidth - hscroll;
                        attrsWidth += attr.width;
                        var keyName = 'attr' + attr.guid;
                        if ((opt.viewMode == 'datagrid') && (attr.dm !== '0') && (opt.hasMapKey)) {
                            iex = marginLeft + opt.splitterWidth + y;
                            iey = rowHeight;
                            ieh = marginTop - rowHeight;
                            iew = attr.width;
                            intEl = { key: keyName, rect: [iex, iey, iew, ieh], hint: attr.name };
                            intMap.push(intEl);
                        };
                        ctx.beginPath();
                        ctx.fillStyle = opt.colors[6];
                        if (opt.mouseOverElement.key == keyName) {
                            ctx.fillStyle = opt.selStyle;
                        };
                        ctx.strokeStyle = opt.colors[3];
                        ctx.rect(1, y + 1, marginTop - rowHeight - 2, attr.width - 2);
                        ctx.fill();
                        //ctx.stroke();
                        ctx.beginPath();
                        ctx.fillStyle = '#000000';
                        if (attr.width > fontSize * 5) {
                            ctx.save();
                            ctx.translate(fontSize / 4, fontSize / 2 + y);
                            ctx.rotate(Math.PI / 2);
                            ctx.textBaseline = 'bottom';
                            var mbottom = -fontSize - fontSize / 2;
                            this.drawMultilineText(ctx, attr.name, 0, mbottom, attr.width - fontSize2, 10);
                            ctx.restore();
                        } else {
                            ctx.fillText(attr.name, fontSize * 2, attr.width / 2 + y);
                        };

                        if ((opt.viewMode == 'datagrid') && (attr.dm !== '0')) {
                            if (opt.dgColAltDMGuid !== guid_empty) { //'AS/24186 enclosed
                                var state = 1;
                                if (guid_equal(attr.dm, guid_empty) && (opt.mouseOverElement.key !== keyName)) {
                                    state = 0;
                                };
                                ctx.save();
                                ctx.translate(fontSize / 4, fontSize / 2 + y);
                                ctx.rotate(Math.PI / 2);
                                this.drawDataMapping(ctx, attr.width / 2 - fontSize / 2, -fontSize / 2, fontSize, state);
                                ctx.restore();
                            };
                        };
                        vi += 1;
                    };
                };
            //-A1478 };

            vi = 0;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.isTerminal == 1) {
                    var y = attrsWidth + columnWidth * col.visualIndex - hscroll;

                    var keyName = 'col' + col.nodeid;
                    iex = marginLeft + opt.splitterWidth + y;
                    iey = col.level * rowHeight;
                    iew = columnWidth;
                    ieh = marginTop - col.level * rowHeight;
                    if ((opt.viewMode !== 'infodocs') || !(opt.infodocReadOnly && !this.isNodeInfodoc(col.id))) {
                        if (!(opt.viewMode == 'contributions' && !opt.showContributions)) {
                            if (opt.viewMode == 'roles' && (opt.showRolesStats || opt.rolesMode == 'objs')) { } else {
                                if (col.checkstate !== 0) {
                                    intEl = { key: keyName, rect: [iex, iey, iew, ieh], hint: col.title };
                                    intMap.push(intEl);
                                }
                            }
                        }
                    };
                    if (opt.viewMode == 'contributions' || opt.viewMode == 'infodocs') {
                        intEl = { key: keyName, rect: [iex, iey, iew, ieh], hint: col.title };
                        intMap.push(intEl);
                    };
                    if (opt.mouseOverElement.key == keyName) {
                        ctx.fillStyle = opt.selStyle;
                    } else {
                        ctx.fillStyle = opt.colors[6];
                    };

                    ctx.beginPath();   
                    ctx.strokeStyle = opt.colors[3];
                    ctx.save();
                    ctx.rect(1, y + 1, marginTop - col.level * rowHeight - 2, columnWidth - 2);
                    ctx.clip();
                    ctx.fill();
                    //ctx.stroke();
                    ctx.beginPath();
                    ctx.fillStyle = '#000000';
                    if (columnWidth > fontSize * 5) {
                        ctx.save();
                        ctx.translate(fontSize / 2, fontSize / 2 + y);
                        ctx.rotate(Math.PI / 2);
                        //ctx.textAlign = 'center';
                        ctx.textBaseline = 'bottom';
                        var mbottom = -fontSize - fontSize / 2;
                        this.drawMultilineText(ctx, col.title, 0, mbottom, columnWidth - fontSize, 10);
                        if (opt.viewMode == 'datagrid') {
                            this.drawMeasurementType(ctx, 0, mbottom + 10, fontSize, col.mt);
                            //if (col.mt !== 'PW') { //'AS/24186 removed 
                            if (opt.dgColAltDMGuid !== guid_empty) { //'AS/24186 enclosed
                                var state = 1;
                                if (guid_equal(col.dm, guid_empty) && opt.mouseOverElement.key !== keyName) {
                                    state = 0;
                                };
                                this.drawDataMapping(ctx, columnWidth / 2, mbottom + 10, fontSize, state);
                            };
                            //};
                        };
                        ctx.restore();
                    } else {
                        ctx.fillText(col.title, fontSize * 2 - (opt.viewMode == 'roles' && opt.rolesMode == 'objs' ? fontSize * 1.5 : 0), columnWidth / 2 + y);
                    };
                    ctx.restore();
                    ctx.save();
                    ctx.translate(0, y);
                    ctx.rotate(Math.PI / 2);
                    if (opt.mouseOverElement.key == keyName) {
                        //ctx.fillStyle = selStyle;
                        //ctx.beginPath();
                        //ctx.fillRect(0, -ieh, iew, ieh);
                        if (opt.viewMode == 'infodocs') {
                            this.drawInfodoc(ctx, columnWidth / 2, -fontSize, fontSize, 1);
                        };
                    };
                    switch (opt.viewMode) {
                        case 'infodocs':
                            this.drawInfodoc(ctx, columnWidth / 2, -fontSize, fontSize, (this.isNodeInfodoc(col.id) ? 1 : 0));
                            break;
                        case 'contributions':
                            if (opt.showContributions) {
                                this.drawCheckbox(ctx, columnWidth / 2, -fontSize, fontSize, col.checkstate);
                            };
                            break;
                        case 'roles':
                            var lMargin = columnWidth / 2;
                            //if (opt.showRolesStats) { lMargin = fontSize };
                            var roleMode = opt.showRolesMode;
                            if (opt.showRolesStats) { roleMode = 2 };
                            if (opt.rolesMode !== 'objs') {
                                if (roleMode !== 2 && col.checkstate !== 0) {
                                    this.drawRolebox(ctx, lMargin, -fontSize, fontSize * 1.5, col.role, col.grole, col.stat, roleMode, col.frole);
                                }
                            };
                            break;
                    };
                    ctx.restore();
                    vi += 1;
                };
            };
            ctx.restore();
            ctx.save();
            ctx.beginPath();
            ctx.rect(0, 0, marginLeft, marginTop);
            ctx.clip();
            ctx.beginPath();
            var keyName = 'attr' + opt.dgColAltNameGuid;
            if (opt.viewMode == 'datagrid') {
                iex = 0;
                iey = 0;
                iew = marginLeft;
                ieh = marginTop;
                intEl = { key: keyName, rect: [iex, iey, iew, ieh], hint: 'Import ' + opt.rowsTitle };
                intMap.push(intEl);
            };
            ctx.fillStyle = opt.colors[6];
            if (opt.mouseOverElement.key == keyName) {
                ctx.fillStyle = opt.selStyle;
            };
            ctx.rect(1, 0, marginLeft+1, marginTop - 2);
            ctx.fill();
            ctx.beginPath();
            ctx.fillStyle = '#000000';
            ctx.fillText(opt.rowsTitle, fontSize / 2, marginTop - rowHeight);
            if (opt.viewMode == 'datagrid') {
                var state = 1;
                if (guid_equal(opt.dgColAltDMGuid, guid_empty) && (opt.mouseOverElement.key !== keyName)) {
                    state = 0;
                };
                this.drawDataMapping(ctx, marginLeft / 2, marginTop - fontSize, fontSize, state);
            };
            ctx.restore();
            ctx.save();
            var mleft = this.getAttributesColumnsWidth() + opt.marginLeft + opt.splitterWidth - hscroll;
            ctx.rect(opt.marginLeft + opt.splitterWidth, 0, opt.canvas.width, opt.marginTop + opt.splitterWidth);
            ctx.clip();

            //A1478 ===
            var visibleAttributeExists = false;
            if (opt.attributes.length > 0) {
                for (var j = 0; j < opt.attributes.length; j++) {
                    var attr = opt.attributes[j];
                    if (this.isAttributeColumnVisible(attr.guid)) {
                        visibleAttributeExists = true;
                    }
                }
            }
            //A1478 ==

            //-A1478 if (opt.attributes.length > 0 && opt.hiddenAttributes !== "") {
            if (opt.attributes.length > 0 && visibleAttributeExists) { //A1478
                ctx.save();
                ctx.beginPath();
                ctx.fillStyle = opt.colors[6];
                iex = opt.marginLeft + opt.splitterWidth - hscroll;
                ctx.rect(iex + 1, 1, this.getAttributesColumnsWidth() - 2, fontSize2 - 2);
                ctx.fill();
                ctx.clip();
                ctx.fillStyle = '#000000';
                ctx.fillText('Attributes', iex + fontSize / 2, fontSize);
                ctx.restore();
            };

            for (var j = 0; j <= opt.maxNodesLevel; j++) {
                for (var i = 0; i < opt.columns.length; i++) {
                    var col = opt.columns[i];
                    if (col.level == j) {
                        if (col.isTerminal == 0) {
                            ctx.save();
                            //ctx.strokeStyle = opt.colors[3];
                            ctx.beginPath();
                            ctx.fillStyle = opt.colors[6];
                            ctx.rect(mleft + col.levelShift * columnWidth + 1, j * fontSize2 + 1, col.terminalnodescount * columnWidth - 2, fontSize2 - 2);
                            ctx.fill();
                            //ctx.stroke();
                            ctx.clip();
                            iex = mleft + col.levelShift * columnWidth;
                            iey = j * fontSize2;
                            var keyName = 'obj' + col.nodeid;
                            if (opt.mouseOverElement.key == keyName) {
                                ctx.fillStyle = opt.selStyle;
                                ctx.beginPath();
                                ctx.fillRect(iex, iey, fontSize2, fontSize2);
                                if (opt.viewMode == 'infodocs') {
                                    this.drawInfodoc(ctx, iex + fontSize, iey + fontSize, fontSize, 1);
                                };
                            };
                            var addMargin = 0;
                            var addStat = '';
                            if ((opt.viewMode == 'roles') && (opt.showRolesStats)) {
                                addMargin = -fontSize * 1.5;
                                addStat = col.stat + '  ';
                            };
                            if ((opt.viewMode == 'roles') && (opt.rolesMode == "alts")) {
                                addMargin = -fontSize * 1.5;
                            };
                            ctx.fillStyle = '#000000';
                            ctx.fillText(addStat + col.title, iex + fontSize2 + addMargin, iey + fontSize);
                            switch (opt.viewMode) {
                                case 'infodocs':
                                    this.drawInfodoc(ctx, iex + fontSize, iey + fontSize, fontSize, (this.isNodeInfodoc(col.id) ? 1 : 0));
                                    break;
                                case 'contributions':
                                    if (opt.showContributions) {
                                        this.drawCheckbox(ctx, iex + fontSize, iey + fontSize, fontSize, col.checkstate);
                                    };
                                    break;
                                case 'roles':
                                    var roleMode = opt.showRolesMode;
                                    if (opt.showRolesStats) { roleMode = 2 };
                                    if (opt.rolesMode == 'objs') {
                                        this.drawRolebox(ctx, iex + fontSize, iey + fontSize, fontSize * 1.5, col.role, col.grole, '', roleMode, col.frole);
                                    };
                                    break;
                            };
                            if ((opt.viewMode !== 'infodocs') || !(opt.infodocReadOnly && !this.isNodeInfodoc(col.id))) {
                                if (opt.viewMode == 'roles' && (opt.showRolesStats || opt.rolesMode == 'alts')) { } else {
                                    intEl = { key: keyName, rect: [iex, iey, fontSize2, fontSize2], hint: col.title };
                                    intMap.push(intEl);
                                }
                            };
                            ctx.restore();
                        };
                    };
                };
            };
            ctx.restore();
            ctx.beginPath(); 
            var isSelected = false;
            isSelected = (opt.mouseOverElement.key == 'topSplitter');
            this._drawSplitter(0, marginTop, isSelected);
            isSelected = (opt.mouseOverElement.key == 'leftSplitter');
            this._drawSplitter(1, marginLeft, isSelected);
            intEl = { key: 'topSplitter', rect: [0, marginTop, opt.canvas.width, opt.splitterWidth], hint: '' };
            intMap.push(intEl);
            intEl = { key: 'leftSplitter', rect: [marginLeft, 0, opt.splitterWidth, opt.canvas.height], hint: '' };
            intMap.push(intEl);
            if (needShowVerticalScrollbar) {
                iex = opt.canvas.width - opt.scrollBarWidth;
                iey = gridY;
                iew = opt.scrollBarWidth;
                ieh = gridH - opt.scrollBarWidth;
                intEl = { key: 'vertScrollbar', rect: [iex, iey, iew, ieh], hint: '' };
                intMap.push(intEl);
                var isSelected = false;
                isSelected = (opt.mouseOverElement.key == 'vertScrollbar');
                this._drawScrollBar(1, ieh, ieh / opt.realRowAreaHeight, opt.vscroll, isSelected);
            };
            if (needShowHorizontalScrollbar) {
                iex = gridX;
                iey = opt.canvas.height - opt.scrollBarWidth;
                iew = gridW - opt.scrollBarWidth;
                ieh = opt.scrollBarWidth; 
                intEl = { key: 'horScrollbar', rect: [iex, iey, iew, ieh], hint: '' };
                intMap.push(intEl);
                var isSelected = false;
                isSelected = (opt.mouseOverElement.key == 'horScrollbar');
                this._drawScrollBar(0, iew, iew / opt.realColAreaWidth, opt.hscroll, isSelected);
            };
            //ctx.fillText(opt.redrawCount + String.fromCharCode(opt.redrawCount + 32), 10, 10);
            var mEl = opt.mouseOverElement;
            if ((mEl !== '') && (typeof mEl.hint != 'undefined') && (mEl.hint !== '')) {

                if (opt.viewMode == 'datagrid' && opt.use_datamapping) { // 'AS/25743a===
 
                     for (var k = 0; k < opt.attributes.length; k++) {
                       var attr = opt.attributes[k];
                       if (this.isAttributeColumnVisible(attr.guid)) { 
                            var keyAttr = 'attr' + attr.guid;
                            if (opt.mouseOverElement.key == keyAttr) {
                               for (var j = 0; j < opt.datamappings.length; j++) {
                                    var mapdata = opt.datamappings[j];
                                    if (typeof mapdata != 'undefined') {  //'AS/25743b
                                        if (attr.dm == opt.datamappings[j].guid) {
                                            mEl.hint = ''
                                            mEl.hint = 'Mapped to: //' + '   ' + mapdata.db_name + '//' + '   ' + mapdata.db_table + '//' + '   ' + mapdata.db_field;
                                        };
                                    };
                                };
                            };
                        };
                    };

                    //// for (var j = 0; j < opt.columns.length; j++) {
                    //for (var j = 0; j < opt.datamappings.length; j++) {
                    //    var col = opt.columns[j];
                    //    if (opt.mouseOverElement.key == 'col' + col.idx) {
                    //        var mapdata = opt.datamappings[j];
                    //        mEl.hint = ''
                    //        mEl.hint = mapdata.db_name + '//' + mapdata.db_table + '//' + mapdata.db_field;
                    //    };
                    //};

                    if (opt.mouseOverElement.key == 'attr' + opt.dgColAltNameGuid) {
                        var attr = { guid: opt.dgColAltNameGuid, dm: opt.dgColAltDMGuid };
                        var mapdata = opt.datamappings[0];
                        if (typeof mapdata != 'undefined') {  //'AS/25743b
                            mEl.hint = ''
                            mEl.hint = 'Mapped to: //' + '   ' + mapdata.db_name + '//' + '   ' + mapdata.db_table + '//' + '   ' + mapdata.db_field;
                        };
                    };
                }; //'AS/25743a==

                this.drawHint(opt.context, mEl.hint, mEl.rect);
            };
            
            opt.interactiveMap = intMap;
        },
        _setOption: function (key, value) { 
            this._super(key, value);
            if (key == 'showRolesStats') {
                this._calculateMargins();
                this.updateAttributesColumnWidth();
                this.redraw();
            };
        },
        _setOptions: function (options) {
            this._super(options);
        },
        resize: function (w, h) {
            var opt = this.options;
            opt.width = w;
            opt.height = h;
            opt.canvas.width = opt.width;
            opt.canvas.height = opt.height;
            opt.context.font = opt.fontSize + 'px Arial';
        },
        isWRTInfodoc: function (altID, objID) {
            for (var i = 0; i < this.options.wrtInfodocs.length; i++) {
                var item = this.options.wrtInfodocs[i];
                if ((item.aid == altID) && (item.oid == objID)) {
                    return true;
                };
            };
            return false;
        },
        getWRTInfodocIndex: function (altID, objID) {
            for (var i = 0; i < this.options.wrtInfodocs.length; i++) {
                var item = this.options.wrtInfodocs[i];
                if ((item.aid == altID) && (item.oid == objID)) {
                    return i;
                };
            };
            return -1;
        },
        isNodeInfodoc: function (nodeGuid) {
            for (var i = 0; i < this.options.nodeInfodocs.length; i++) {
                var item = this.options.nodeInfodocs[i];
                if (item.nodeGuid == nodeGuid) {
                    return true;
                };
            };
            return false;
        },
        isNodeDataMapped: function (nodeGuid) {
            for (var i = 0; i < this.options.nodeDataMap.length; i++) {
                var item = this.options.nodeDataMap[i];
                if (item.nodeGuid == nodeGuid) {
                    return true;
                };
            };
            return false;
        },
        isWarningRow: function (row) {
            //if (this.options.viewMode !== "roles") return false;
            //for (var i = 0; i < row.stat.length; i++) {
            //    var st = row.stat[i];
            //    if (st.indexOf('/0') !== -1) {
            //        return true;
            //    };
            //};
            return false;
            //return row.warning;
        },
        getNodeInfodocIndex: function (nodeGuid) {
            for (var i = 0; i < this.options.nodeInfodocs.length; i++) {
                var item = this.options.nodeInfodocs[i];
                if (item.nodeGuid == nodeGuid) {
                    return i;
                };
            };
            return -1;
        },
        setColumnContributions: function (ColID, chk) {
            var opt = this.options;
            var tnodes = this.getTerminalNodes();
            var nidx = 0;
            for (var i = 0; i < tnodes.length; i++) {
                var node = tnodes[i];
                if (node.nodeid == ColID) {
                    nidx = i;
                    break;
                };
            };
            for (var i = 0; i < opt.rows.length; i++) {
                opt.rows[i].cont[nidx] = chk;
            };
        },
        _getColIndexByNodeID: function(ColID) {
            var tnodes = this.getTerminalNodes();
            for (var i = 0; i < tnodes.length; i++) {
                var node = tnodes[i];
                if (node.nodeid == ColID) {
                    return i;
                };
            };
            return -1;
        },
        setColumnRoles: function (ColID, role) {
            var opt = this.options;
            var tnodes = this.getTerminalNodes();
            var nidx = this._getColIndexByNodeID(ColID);
            var rowsids = [];
            var colids = [ColID];
            for (var i = 0; i < opt.rows.length; i++) {
                opt.rows[i].roles[nidx] = role;
                rowsids.push(opt.rows[i].nodeid);
            };
            this.setRoleRange(rowsids, colids, role);
        },
        _getRowIndexByNodeID: function (nodeID) {
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                if (row.nodeid == nodeID) { return i };
            };
            return -1;
        },
        setRowContributions: function (RowID, chk) {
            sendCommand('action=setrow&row_id=' + RowID + '&val=' + chk, true);
        },
        setRoleRange: function (rowNodeIDs, colNodeIDs, role) {
            sendCommand('action=setrolerange&rowids=' + rowNodeIDs + '&colids=' + colNodeIDs + '&val=' + role + '&chkusers=' + this.options.checkedUsers, true);
        },
        setObjRoles: function (nodeid, role) {
            sendCommand('action=setobjrole&objid=' + nodeid + '&val=' + role + '&chkusers=' + this.options.checkedUsers, true);
        },
        setCellContribution: function (rowNodeID, ColNodeID, chk) {
            sendCommand('action=setcell&row_id=' + rowNodeID + '&col_id=' + ColNodeID + '&val=' + chk, true);
        },
        setCellRoles: function (rowNodeID, colNodeID, role) {
            sendCommand('action=setcellrole&rowid=' + rowNodeID + '&colid=' + colNodeID + '&val=' + role + '&chkusers=' + this.options.checkedUsers, true);
        },
        setAllContributions: function (isChecked) {
            var opt = this.options;
            var newCont = (isChecked ? 1 : 0);
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                for (var j = 0; j < opt.columns.length; j++) {
                    row.cont[j] = newCont;
                };
            };
            this.updateObjCheckboxState();
            this.redraw();
            sendCommand('action=setall&val=' + isChecked, true);
        },
        setAllRoles: function (role) {
            var opt = this.options;
            for (var j = 0; j < opt.columns.length; j++) {
                var col = opt.columns[j];
                if (col.role !== -1 && opt.rolesMode !== 'alts') {
                    col.role = role;
                    col.frole = (role == 2 ? col.grole : role);
                };
                for (var i = 0; i < opt.rows.length; i++) {
                    var row = opt.rows[i];
                    if (row.roles[j] !== -1 && opt.rolesMode !== 'objs') {
                        row.roles[j] = role;
                        row.froles[j] = (role == 2 ? row.groles[j] : role);
                    };
                };
            };
            this.updateRoles(false);
            this.redraw();
        },
        _getRowByGuid: function(rowGuid) {
            var opt = this.options;
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                if (row.id == rowGuid) {
                    return row;
                };
            };
            return null;
        },
        _getRowByNodeID: function (rowID) {
            var opt = this.options;
            for (var i = 0; i < opt.rows.length; i++) {
                var row = opt.rows[i];
                if (row.nodeid == rowID) {
                    return row;
                };
            };
            return null;
        },
        _getColByGuid: function(colGuid) {
            var opt = this.options;
            for (var i = 0; i < opt.columns.length; i++) {
                var col = opt.columns[i];
                if (col.id == colGuid) {
                    return col;
                };
            };
            return null;
        },
        updateInfodoc: function (infodocID, isEmpty) {
            var opt = this.options;
            if (infodocID.indexOf('wrtInfodoc') >= 0) {
                var pairGuid = infodocID.replace('wrtInfodoc', '');
                var altID = pairGuid.split('_')[0];
                var objID = pairGuid.split('_')[1];
                var row = this._getRowByGuid(altID);
                var col = this._getColByGuid(objID);
                var wrtIndex = this.getWRTInfodocIndex(row.nodeid, col.nodeid);
                if ((wrtIndex > -1) && (isEmpty == true)) {
                    opt.wrtInfodocs.splice(wrtIndex, 1);
                    this.redraw();
                };
                if ((wrtIndex == -1) && (isEmpty == false)) {
                    opt.wrtInfodocs.push({ aid: row.nodeid, oid: col.nodeid });
                    this.redraw();
                };
            };
            
            if ((infodocID.indexOf('altInfodoc') >= 0) || (infodocID.indexOf('objInfodoc') >= 0)) {
                var altID;
                if (infodocID.indexOf('altInfodoc') >= 0) {
                    altID = infodocID.replace('altInfodoc', '');
                } else {
                    altID = infodocID.replace('objInfodoc', '');
                };

                var row = this._getRowByGuid(altID);
                var rowIndex = this.getNodeInfodocIndex(altID);
                if ((rowIndex > -1) && (isEmpty == true)) {
                    opt.nodeInfodocs.splice(rowIndex, 1);
                    this.redraw();
                };
                if ((rowIndex == -1) && (isEmpty == false)) {
                    opt.nodeInfodocs.push({ nodeGuid: altID });
                    this.redraw();
                };
            };
        },
        sort: function () {
            var id = this.options.table.id;
            var mode = this.options.sortMode.mode;
            var sortName = this.options.sortMode.columnName;
            var headerCell = $('#' + id + '_sortChr');
            var chArrow = '';
            switch (mode) {
                case 'asc':
                    this.options.sortMode.mode = 'desc';
                    this.options.rows.sort(SortByTitleDesc);
                    chArrow = chArrowUp;
                    break;
                case 'desc':
                    this.options.sortMode.mode = 'none';
                    this.options.rows.sort(SortByIdxAsc);
                    chArrow = '';
                    break;
                default:
                    this.options.sortMode.mode = 'asc';
                    this.options.rows.sort(SortByTitleAsc);
                    chArrow = chArrowDown;
            };
            headerCell.html(sortName + ' ' + chArrow);
            this.redrawRows();
        },
        setsearchfields: function (guid, ischecked) {
            if (guid == '0') { this.options.searchbyname = ischecked };
            for (var k = 0; k < this.options.attributes.length; k++) {
                var attr = this.options.attributes[k];
                if (guid == attr.guid) {
                    attr.search = ischecked;
                };
            };
        },
        filterRows: function (searchText) {
            //var id = this.options.table.id;
            //$('.checkattr').each(function () {
            //    var guid = $(this).attr("guid");
            //    var ischecked = $(this).is(':checked');
            //    $('#' + id).datagrid('setsearchfields', guid, ischecked);
            //});
            //for (var i = 0; i < this.options.rows.length; i++) {
            //    var row = this.options.rows[i];
            //    var showrow = 0;
            //    if (this.options.searchbyname) {
            //        if (row.title.toLowerCase().indexOf(searchText) >= 0) { showrow = 1 };
            //    };
            //    for (var k = 0; k < this.options.attributes.length; k++) {
            //        var attr = this.options.attributes[k];
            //        if (attr.search) {                      
            //            if (row.attrvals[k].toLowerCase().indexOf(searchText) == 0) { showrow = 1 };
            //        };
            //    };
            //    row.show = showrow;
            //};
            //this.options.currentPage = 1;
            //this.redrawRows();
            //this.updateAllCheckbox();
        },
        deleteEvent: function (event_id) {
            //sendCommand("action=delete_event&event_id=" + event_id);
            //var i = 0;
            //while (i < this.options.rows.length) {
            //    var row = this.options.rows[i];
            //    if (row.nodeid == event_id) {
            //        this.options.rows.splice(i, 1);
            //    } else {
            //        i++;
            //    }
            //}
            //this.redrawRows();
        },
        updateEventName: function (event_id, val) {
            //if (val != "") {
            //    for (var i = 0; i < this.options.rows.length; i++) {
            //        var row = this.options.rows[i];
            //        if (row.nodeid == event_id) row.title = val;
            //    };                
            //    sendCommand("action=update_event_name&event_id=" + encodeURIComponent(event_id) + "&val=" + encodeURIComponent(val));
            //    this.redrawRows();
            //}
        },
        setreadonly: function (val) {   // D4470
            this.options.readonly = val;
            if ((val)) document.body.style.cursor = "default"; else this.redraw();
        }

    });
})(jQuery);

function onContextMenuViewObjectivesClick(event_id, event_name) {
    $("div.context-menu").hide(200); is_context_menu_open = false;
    showContributedNodes(event_id, event_name);
}

function onContextMenuRenameEventClick(event_id, event_name) {        
    $("div.context-menu").hide(200); is_context_menu_open = false;
    switchAltNameEditor(event_id, 1);
}

var alt_name = -1;
var alt_value = "";

function switchAltNameEditor(id, vis) {
    if (vis == 1) {
        if (alt_name < 0 || alt_name == id || (alt_name >= 0 && alt_name != id && switchAltNameEditor(alt_name, 0))) {
            $("#lblName" + id).hide();
            $("#tbName" + id).show().focus();
            alt_name = id;
            alt_value = $("#tbName" + id).val();
            on_hit_enter = "switchAltNameEditor(" + id + ", 0);";
        }
    } else {
        var val = $("#tbName" + id).val();
        if (val == "") {
            dxDialog("Name can't be empty!", true,  'setTimeout(\'$("#tbName' + id + '").focus();\', 50);', '');
            $(".ui-dialog").css("z-index", 9999);
            return false;
        }
        if (vis == 0 && val != "" && alt_value != "" && val != alt_value) $("#dataGridTable").datagrid("updateEventName", id, val);
        $("#tbName" + id).hide();
        $("#lblName" + id).show();
        alt_name = -1;
        alt_value = "";
        on_hit_enter = "";
    }
    return true;
}

function confrimDeleteEvent(event_id, event_name) {
    $("div.context-menu").hide(200); is_context_menu_open = false;
    dxDialog("Are you sure you want to delete \"" + htmlEscape(event_name) + "\"?", false, "deleteEvent(\"" + event_id + "\");", ";", "Confirmation", 350, -1);
    $(".ui-dialog").css("z-index", 9999);
}

function SortByTitleDesc(a, b) {
    return ((a.title.toLowerCase() < b.title.toLowerCase()) ? 1 : ((a.title > b.title) ? -1 : 0));
}

function SortByTitleAsc(a, b) {
    return ((a.title.toLowerCase() > b.title.toLowerCase()) ? 1 : ((a.title < b.title) ? -1 : 0));
}

function SortByIdxAsc(a, b) {
    return ((a.idx > b.idx) ? 1 : ((a.idx < b.idx) ? -1 : 0));
}

function getRelativePosition(canvas, absX, absY) {
    var rect = canvas.getBoundingClientRect();
    return {
        x: absX - rect.left,
        y: absY - rect.top
    };
}

function roundRect(ctx, x, y, width, height, radius, fill, stroke) {
    if (typeof stroke == "undefined") {
        stroke = true;
    }
    if (typeof radius === "undefined") {
        radius = 5;
    }
    ctx.beginPath();
    ctx.moveTo(x + radius, y);
    ctx.lineTo(x + width - radius, y);
    ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
    ctx.lineTo(x + width, y + height - radius);
    ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
    ctx.lineTo(x + radius, y + height);
    ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
    ctx.lineTo(x, y + radius);
    ctx.quadraticCurveTo(x, y, x + radius, y);
    ctx.closePath();
    if (stroke) {
        ctx.stroke();
    }
    if (fill) {
        ctx.fill();
    }
}

function showInfodoc(cmd, readonly) {
    if (readonly) {
        OpenInfodoc(cmd);
    } else
    {
        OpenRichEditor(cmd);
    };
    
}