/* javascript plug-ins for Resource Aligner by SL, DA */

(function ($) {
    $.widget("ra.timeline", {
        options: {
            projs: [{ id: '0', idx: 1, name: 'Project1', periodStart: 0, periodDuration: 1, periodMin: 0, periodMax: 1, color: '#95c5f0', isFunded: true, resources: [{ name: 'Cost', values: [10, 20, 30], totalValue: 60 }, { name: 'Res1', values: [20, 30, 40], totalValue: 90 }], hasinfodoc: false },
                    { id: '1', idx: 2, name: 'Project2', periodStart: 0, periodDuration: 1, periodMin: 0, periodMax: 1, color: '#fa7000', isFunded: true, resources: [{ name: 'Cost', values: [10, 20, 30], totalValue: 60 }, { name: 'Res1', values: [20, 30, 40], totalValue: 90 }], hasinfodoc: false }],
            periodsCount: 1,
            portfolioResources: [{ resID: '0', periodID: 0, minVal: 5, maxVal: 10 }, { resID: '1', periodID: 0, minVal: 5, maxVal: 10 }],
            solvedResults: [],
            resources: [{ id: '0', name: 'Cost' }, { id: '1', name: 'Res1' }],
            imgPath: "",
            imgInfodoc: "",
            imgNoInfodoc: "",
            viewMode: 'edit',
            distribMode: 0,
            selectedResourceID: '0',
            resID: 0,
            valDigits: 2,
            hideCents: false,
            periodNamingMode: 1,
            periodNamePrefix: 'Period #',
            firstPeriod: 1,
            periodStep: 1,
            startDate: '',
            rowHeight: 29,
            cellMargin: 8,
            background: '#fff',
            font: '13px Arial',
            useColors: false,
            autoSolve: true,
            fundedOnly: false,
            tableAlign: false,
            fixedWidth: "",
            div: null,
            mouseDownPosition: null,
            mouseOverElement: { key: '', rect: [0, 0, 0, 0], hint: '' },
            mouseOverProjectID: '',
            interactiveMap: [],
            projectsColumnWidth: 100,
            periodColumnWidth: 100,
            isMouseDown: false,
            mouseDragShift: 0,
            totalRow: [0, 0, 0],
            fundedRow: [0, 0, 0],
            pageSize: 20,
            currentPage: 1,
            titleProject: "Projects"
        },
        _create: function () {
            var opt = this.options;
            opt.div = this.element[0];
            $(opt.div).empty();
            for (var i = 0; i < opt.resources.length; i++) {
                var res = opt.resources[i];
                if (res.id == opt.selectedResourceID) { opt.resID = i };
            };
            if (opt.viewMode === 'results') {
                opt.hideCents = (opt.valDigits == 0);
                this.updateSolvedResults();
                this.redrawResults();
            } else {
                if (opt.periodsCount == 0) { enableEditButtons(false); };
                this.redraw();
            };
        },
        _setOption: function (key, value) {
            this._super(key, value);
            if (key == 'selectedResourceID') {
                for (var i = 0; i < this.options.resources.length; i++) {
                    var res = this.options.resources[i];
                    if (res.id == value) { this.options.resID = i };
                };
            };
        },
        _setOptions: function (options) {
            this._super(options);
        },
        resize: function () {
            if (this.options.viewMode == 'results') {
                this.redrawResults();
            } else {
                this.redraw();
            };
        },
        _getPeriodName: function (i) {
            var opt = this.options;
            var periodVal = opt.firstPeriod + i * opt.periodStep;
            var periodName = 'Period ' + periodVal;
            if (opt.periodNamingMode != 5) {
                var monthNames = ["January", "February", "March", "April", "May", "June",
                                  "July", "August", "September", "October", "November", "December"];
                var today = new Date();
                if (opt.startDate != '') {
                    today = new Date(opt.startDate);
                };
                switch (opt.periodNamingMode) {
                    case 0:
                        today.setDate(today.getDate() + i * opt.periodStep);
                        break;
                    case 1:
                        today.setDate(today.getDate() + i * opt.periodStep * 7);
                        break;
                    case 2:
                        today.setMonth(today.getMonth() + i * opt.periodStep);
                        break;
                    case 3:
                        today.setMonth(today.getMonth() + i * opt.periodStep * 3);
                        break;
                    case 4:
                        today.setFullYear(today.getFullYear() + i * opt.periodStep);
                        break;
                    default:

                };
                var dd = today.getDate();
                var mm = today.getMonth() + 1;
                var yyyy = today.getFullYear();
                if (dd < 10) {
                    dd = '0' + dd
                };
                if (mm < 10) {
                    mm = '0' + mm
                };
                switch (opt.periodNamingMode) {
                    case 0:
                        periodName = mm + '/' + dd + '/' + yyyy;
                        break;
                    case 1:
                        periodName = mm + '/' + dd + '/' + yyyy + ' (Week ' + (i + 1) + ')';
                        break;
                    case 2:
                        periodName = monthNames[today.getMonth()] + ' ' + yyyy;
                        break;
                    case 3:
                        periodName = monthNames[today.getMonth()] + ' ' + yyyy + ' (Q' + (i + 1) + ')';
                        break;
                    case 4:
                        periodName = yyyy;
                        break;
                    default:
                };
            };
            return periodName;
        },
        createTimeperiodTable: function () {
            var opt = this.options;
            opt.div = this.element[0];
            $(opt.div).empty();
            var resName = '<select id="resSelect" onchange="changeResource(this.value);"></select>';//this._getResourceNameByID(opt.selectedResourceID);
            //for (var i = 0; i < opt.resources.length; i++) {
            //    var res = opt.resources[i];
            //    if (res.id === resID) { return res.name };
            //};
            var newTable = $('<table class="dg_table" style="table-layout: fixed;"' + (opt.tableAlign ? ' align="center"' : '') + '><thead><tr id="tHeaderRow">' + (opt.index_visible ? '<th class="ra_th" style="width:30px;vertical-align:middle;">ID</th>' : '') + '<th class="ra_th" style="' + ((opt.fixedWidth) && (opt.fixedWidth != "") ? ';width:' + opt.fixedWidth + 'px' : 'min-width:100px') + ';vertical-align:middle;">' + opt.titleProject + '</th><th class="ra_th" style="width:90px;vertical-align:middle;">' + resName + '</th></tr></thead><tfoot id="tFoot"></tfoot><tbody id="tBody"></tbody></table>', {}); //A1143sss
            $(opt.div).append(newTable);
            var sel = document.getElementById('resSelect');
            for (var i = 0; i < opt.resources.length; i++) {
                var res = opt.resources[i];
                var optHTML = document.createElement('option');
                optHTML.innerHTML = res.name;
                optHTML.value = res.id;
                sel.appendChild(optHTML);
            };
            var btnCopyPaste = (opt.viewMode == 'edit' ? '<span class="ratp_copypaste"><a href="" class="actions" onclick="copyColumn({0});return false;">Copy</a> <a href="" class="actions" onclick="pasteColumn({0}); return false;">Paste</a></span>' : '');
            $("#resSelect").val(opt.selectedResourceID);
            if (opt.viewMode == 'edit') {
                $('<th class="ra_th" style="width:40px;font-size:10px">Earliest<br>' + replaceString('{0}', 0, btnCopyPaste) + '</th><th class="ra_th" style="width:40px;font-size:10px">Latest<br>' + replaceString('{0}', 1, btnCopyPaste) + '</th><th class="ra_th" style="width:40px;font-size:10px">Duration<br>' + replaceString('{0}', 2, btnCopyPaste) + '</th><th class="ra_th" style="width:40px;font-size:10px">Must for specific Periods</th>').appendTo('#tHeaderRow'); // D4479
            };
            for (var i = 0; i < opt.periodsCount; i++) {
                var periodName = this._getPeriodName(i);
                var cpButton = ((opt.viewMode == 'results') ? ' ' + replaceString('{0}', i, btnCopyPaste) : '');
                var periodIdx = '';
                switch (opt.periodNamingMode) {
                  case 1:
                  case 3:
                  case 5:
                    break;
                  default:
		    periodIdx = ((i < opt.periodsCount) || (opt.viewMode == 'results') ? ' (' + (i + 1) + ')' + ((i < opt.periodsCount-1) ? '<br>' : '') + cpButton : '');
                }
                $('<th id="thp' + i + '" class="ra_th" style="width:90px; overflow: hidden; min-width:90px">' + periodName + periodIdx + '</th>').appendTo('#tHeaderRow');
            };
            if (opt.viewMode == 'edit') {
                $('<th class="ra_th" style="vertical-align:bottom;color:#aaa;width:90px;">' + this._getPeriodName(opt.periodsCount) + '<br><input id="btnAddPeriod" type="button" style="width:40px" value="+" title="Add Period"></th>').appendTo('#tHeaderRow');
                if (opt.periodsCount > 0) {
                    $('<br><input id="btnRemovePeriod" type="button" style="width:40px" value="-" title="Remove Period">').appendTo('#thp' + (opt.periodsCount-1));
                    $("#btnRemovePeriod").click(function () {
                        $("#RATimeline").timeline("removePeriod");
                    });
                };
                $("#btnAddPeriod").click(function () {
                    $("#RATimeline").timeline("addPeriod");
                });
            };
        },
        addTotalRows: function () {
            var opt = this.options;
            var resName = this._getResourceNameByID(opt.selectedResourceID);
            var totalCost = 0;
            for (var i = 0; i < opt.projs.length; i++) {
                var proj = opt.projs[i];
                var totalVal = proj.resources[opt.resID].totalValue;
                if (totalVal > 0){totalCost += totalVal};
            };
            this._calculateTotalRow();
            var sumFundedCost = 0;
            var fundedString = '';
            var totalString = '';
            var minString = '';
            var maxString = '';
            var tpSync = (localStorage.getItem('tpSync' + curPrjID) == 'true');
            for (var p = 0; p < opt.periodsCount; p++) {
                var pFunded = opt.fundedRow[p];
                var pTotal = opt.totalRow[p];
                var resource = this.getPortfolioResource(opt.selectedResourceID, p);
                var valmin = resource.minVal;
                var sValmin = '';
                if (valmin > -2147483648) { sValmin =  number2locale(valmin, false, opt.hideCents)}; // number2locale(valmin, true)

                var valmax = resource.maxVal;
                var sValmax = '';
                if (valmax > -2147483648) { sValmax = number2locale(valmax, false, opt.hideCents) };

                sumFundedCost += pFunded;
                minString += '<td class="ra_td"><input type="text" class="inpminmaxclass' + (p > 0 ? ' syncmin' : '') + '" style="text-align: right; width: 80px;" id="resInputMin' + p + '_' + opt.selectedResourceID + '" value="' + sValmin + '" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false"></td>';
                maxString += '<td class="ra_td"><input type="text" class="inpminmaxclass' + (p > 0 ? ' syncmax' : '') + '" style="text-align: right; width: 80px;" id="resInputMax' + p + '_' + opt.selectedResourceID + '" value="' + sValmax + '" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false"></td>';
                fundedString += '<td class="ra_th" style="text-align: right;padding-right: 13px;">' + number2locale(pFunded, false, opt.hideCents) + '</td>';
                totalString += '<td class="ra_th" style="text-align: right;padding-right: 13px;">' + number2locale(pTotal, false, opt.hideCents) + '</td>';
            };
            var spancols = 3;
            if (!opt.index_visible) { spancols = 2 };
            var totalRowsStr = '';
            var syncMinMaxCheckbox = '<label id="lblcbSync" title="Check to use same value for each time period"><input id="cbSyncMinMax" class="checkbox checkbox_tiny" onclick="return onSyncClick(this.name, this.checked, \'' + opt.selectedResourceID + '\', ' + opt.periodsCount + ');" ' + (tpSync ? 'checked' : '') + ' type="checkbox"> Sync</label>';
            totalRowsStr += '<tr><td class="ra_th" style="text-align: right" colspan="' + (spancols - 1) + '"><input type="button" onclick="return onSyncMinClick(\'' + opt.selectedResourceID + '\', ' + opt.periodsCount + ');" class="button ratp_btncopy" value="Copy to All"></td><td class="ra_th" style="text-align: right">Min ' + resName + '</td>' + minString + '</tr>';
            totalRowsStr += '<tr><td class="ra_th" style="text-align: right" colspan="' + (spancols - 1) + '"><input type="button" onclick="return onSyncMaxClick(\'' + opt.selectedResourceID + '\', ' + opt.periodsCount + ');" class="button ratp_btncopy" value="Copy to All"></td><td class="ra_th" style="text-align: right">Max ' + resName + '</td>' + maxString + '</tr>';
            totalRowsStr += '<tr><td class="ra_th" style="text-align: right" colspan="' + (spancols - 1) + '">Funded ' + resName + '</td><td style="text-align: right;" class="ra_th"><b>' + number2locale(sumFundedCost, false, opt.hideCents) + '</b></td>' + fundedString + '</tr>';
            totalRowsStr += '<tr><td class="ra_th" style="text-align: right" colspan="' + (spancols - 1) + '">Total ' + resName + '</td><td style="text-align: right;" class="ra_th">' + number2locale(totalCost, false, opt.hideCents) + '</td>' + totalString + '</tr>';
            if ((opt.projs.length > opt.pageSize) && (opt.pageSize > 0)) {
                var totalPages = Math.ceil(opt.projs.length / opt.pageSize);
                /*
                var pagesStr = '<b>Page</b>: ';
                for (var i = 1; i <= totalPages; i++) {
                    if (i === opt.currentPage) {
                        pagesStr += '<b>' + i + '</b> ';
                    } else {
                        pagesStr += '<a class="action" onclick="setTPPage(' + i +'); return false;" href="">'+ i +'</a> ';
                    };
                    if (i < totalPages) { pagesStr += '&middot; ' };
                };
                */
                if (opt.currentPage < 1) opt.currentPage = 1;
                if (opt.currentPage > totalPages) opt.currentPage = totalPages;
                pagesStr = getPagesList(totalPages, opt.currentPage, "<b>Page</b>: %%pages%%", "setTPPage(%%pg%%);");
                totalRowsStr += '<tr><td class="text" style="text-align: center; padding-top: 4px;" colspan="' + (spancols + opt.periodsCount) + '">' + pagesStr + '</td></tr>';
            };
            $(totalRowsStr).appendTo('#tFoot');
            $(".syncmin").prop("disabled", tpSync);
            $(".syncmax").prop("disabled", tpSync);
            $(".inpminmaxclass").focus(function () { $(this).select(); });
            $(".inpminmaxclass").change(function () {
                var newVal = $(this).val();
                if (newVal == "") { newVal = "-2147483648" };
                var idName = $(this).attr("id");
                var p = 0;
                if (idName.indexOf('Min') >= 0) {
                    p = str2cost(idName.replace("resInputMin", "").split('_')[0]);
                    var resID = idName.replace("resInputMin", "").split('_')[1];
                    var isSync = ($("#cbSyncMinMax").is(":checked") && p == 0);
                    if (isSync) {
                        $(".syncmin").val(newVal);
                        for (var tp = 0; tp < opt.periodsCount; tp++) {
                            $("#RATimeline").timeline("setMinValue", resID, tp, newVal, false);
                        };
                        sendCommand('action=tpsetallminval&res_id=' + resID + '&val=' + newVal);
                    } else {
                        $("#RATimeline").timeline("setMinValue", resID, p, newVal, true);
                    };
                };
                if (idName.indexOf('Max') >= 0) {
                    p = str2cost(idName.replace("resInputMax", "").split('_')[0]);
                    var resID = idName.replace("resInputMax", "").split('_')[1];
                    var isSync = ($("#cbSyncMinMax").is(":checked") && p == 0);
                    if (isSync) {
                        $(".syncmax").val(newVal);
                        for (var tp = 0; tp < opt.periodsCount; tp++) {
                            $("#RATimeline").timeline("setMaxValue", resID, tp, newVal, false);
                        };
                        sendCommand('action=tpsetallmaxval&res_id=' + resID + '&val=' + newVal);
                    } else {
                        $("#RATimeline").timeline("setMaxValue", resID, p, newVal, true);
                    };
                };
            });
            $(".inpminmaxclass").on("keypress", function (e) {
                if (e.keyCode == 13) { this.blur(); };
            });
        },
        pastecolumn: function (pdata, periodID) {
            var opt = this.options;
            var resultsstring = '';
            var sValues = '';
            var sTotal = '';
            var opt = this.options;
            var pdataarray = pdata.split('\n');
            for (var i = 0; i < pdataarray.length; i++) {
                var proj = opt.projs[i];
                var strNewVal = pdataarray[i];
                var c = str2cost(strNewVal);
                if (typeof c !== "undefined" && typeof (proj) !== "undefined") {
                    if ((periodID - proj.periodStart < proj.resources[opt.resID].values.length) && (proj.resources[opt.resID].values[periodID - proj.periodStart] !== c)) {
                        this.distributeResourceValues(c, proj.id, periodID - proj.periodStart);
                        sValues += proj.id + ';';
                        var resVals = '';
                        for (var j = 0; j < proj.resources[opt.resID].values.length; j++) {
                            if (resVals !== '') { resVals += ';' };
                            resVals += proj.resources[opt.resID].values[j];
                        };
                        sValues += resVals + '*';
                        if (opt.distribMode == 2) {
                            if (sTotal !== '') { sTotal += ';' };
                            sTotal += proj.resources[opt.resID].totalValue;
                        };
                    };
                };
            };
            var sCommand = 'action=setresourcegrid&res_id=' + opt.selectedResourceID;
            var sData = 'values=' + sValues;
            if (opt.distribMode == 2) {
                sData += '&total=' + sTotal;
            };
            sendCommand(sCommand, true, sData);
            this.redrawResults();
        },
        pasterow: function (pdata, projectIdx) {
            var opt = this.options;
            var sValues = '';
            var pdataarray = pdata.split('\t');
            var proj = this._getProjectByIdx(projectIdx);
            if (proj !== null) {
                for (var i = 0; i < pdataarray.length; i++) {
                    var strNewVal = pdataarray[i];
                    var c = str2cost(strNewVal);
                    var periodID = i;
                    var inpRes = $("#resInput" + proj.id + '_' + (periodID - proj.periodStart));
                    if (c !== "undefined" && typeof c !== "undefined" && typeof inpRes !== "undefined" && inpRes.length > 0) {
                        inpRes.val(c);
                        proj.resources[opt.resID].values[periodID - proj.periodStart] = Number(strNewVal);
                    };
                };
                var totalVal = 0;
                for (var i = 0; i < proj.resources[opt.resID].values.length; i++) {
                    var val = proj.resources[opt.resID].values[i];
                    if (sValues !== '') { sValues += 'x'; };
                    sValues += val;
                    totalVal += val;
                };
                var totalRes = $("#totalInput" + proj.id);
                if (typeof totalRes !== "undefined") {
                    totalRes.val(totalVal);
                };
                proj.resources[opt.resID].totalValue = totalVal;
                sendCommand('action=setresource&res_id=' + opt.selectedResourceID + '&project_id=' + proj.id + '&values=' + sValues + '&total=' + totalVal + '&dm=2');
                this.redrawResults();
            };
        },
        redrawResults: function () {
            var opt = this.options;
            this.createTimeperiodTable();
            var totalCost = 0;
            var totalRows = 0;
            for (var i = 0; i < opt.projs.length; i++) {
                var proj = opt.projs[i];
                var projTotalCost = proj.resources[opt.resID].totalValue;
                if (projTotalCost > 0) { totalCost += projTotalCost };
                if ((opt.pageSize === 0) || ((i >= ((opt.currentPage - 1) * opt.pageSize)) && (i < opt.currentPage * opt.pageSize))) {
                    var resourcesString = '';
                    if (opt.periodsCount > 0) {
                        var projColor = (opt.useColors ? proj.color : '#80b0f0');
                        var pID = 0;
                        for (var s = 0; s < proj.periodStart; s++) {
                            var tdColor = '#fff';
                            if ((s >= proj.periodMin) && (s < proj.periodStart)) {
                                tdColor = '#eee';
                            };
                            resourcesString += '<td style="background-color:' + tdColor + ';border-right: none;"></td>';
                            pID++;
                        };

                        for (var p = 0; p < proj.periodDuration; p++) {
                            var aVal = proj.resources[opt.resID].values[p];
                            var cellText = '';
                            if (aVal > -2147483648) { cellText = number2locale(aVal, false, opt.hideCents) };
                            var valBackground = '#fff';
                            var txtColor = '#000';
                            projColor = (opt.useColors ? proj.color : '#80b0f0');
                            if (!proj.isFunded) {
                                projColor = '#aaa';
                                //valBackground = '#ddd';
                                txtColor = '#888';
                            };
                            var isDisabled = (((opt.distribMode == 1) && (p == proj.periodDuration - 1)) || (proj.periodDuration == 1 && opt.distribMode !== 2));
                            //if (isDisabled) { txtColor = '#888'; };
                            var inputField = '<input ' + (isDisabled ? 'disabled title="The final period for each project cannot be edited because it is automatically calculated as the remaining undistributed balance"' : 'class="inpresclass"') + ' type="text" style="text-align: right; width: 80px; border:none; color:' + txtColor + '; background-color:#fff" id="resInput' + proj.id + '_' + p + '" value="' + cellText + '" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false" tabindex="' + ((i + 1) * 1000 + pID) + '">';
                            inputField += "<div id='context-menu"+ proj.id + '_' + p +"'></div>"
                            //inputField += '<ul class="custom-menu" id="ctx{0}_{1}">'.f(proj.id, p); 
                            //for (var r = 0; r < proj.resources.length; r++) {
                            //    var itemRes = proj.resources[r];
                            //    if (itemRes.values[p] > -2147483648) { inputField += '<li data-action="{0}" style="background-color:{3};">{1}: {2}</li>'.f(r, itemRes.name, number2locale(itemRes.values[p], false, opt.hideCents), (r == opt.resID ? "lightgrey" : "white")) };
                            //};  
                            //inputField += '</ul>';
                            resourcesString += '<td style="padding:4px;background-color:' + projColor + ';" class="ra_td">' + inputField + '</td>';
                            pID++;
                        };


                        projColor = (opt.useColors ? proj.color : '#80b0f0');
                        for (var s = proj.periodStart + proj.periodDuration; s < (proj.periodMax + proj.periodDuration) ; s++) {
                            var tdColor = '#eee';
                            resourcesString += '<td style="background-color:' + tdColor + ';"></td>';
                            pID++;
                        };
                        for (var s = proj.periodMax + proj.periodDuration; s < (opt.periodsCount) ; s++) {
                            resourcesString += '<td style="background-color:#fff;"></td>';
                        };
                    };
                    var inputField = '<input class="inptotalclass" type="text" style="text-align: right; width: 80px;" id="totalInput' + proj.id + '" value="' + (projTotalCost < 0 ? '' : number2locale(projTotalCost, false, opt.hideCents)) + '" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false">';
                    var fundedBackground = '';
                    var fontBold = '';  //(proj.isFunded ? 'font-weight: bold;' : ''); D4341
                    if (proj.isFunded) {
                        fundedBackground = 'background-color: #d1e6b5;'
                    };
                    var imgInfodoc = opt.imgPath + opt.imgInfodoc;
                    var imgNoInfodoc = opt.imgPath + opt.imgNoInfodoc; 
                    var btnCopyPaste = "<a href='' class='actions' title='Copy Row' onclick='copyRow({0});return false;' onmouseenter='highlightRow(\"{1}\", true);' onmouseout='highlightRow(\"{1}\", false);'><i class='fas fa-copy'></i></a> <a href='' title='Paste Row' class='actions' onclick='pasteRow({0}); return false;' onmouseenter='highlightRow(\"{1}\", true);' onmouseout='highlightRow(\"{1}\", false);'><i class='fas fa-paste'></i></a> ".f(proj.idx, proj.id);
                    var infodocLink = btnCopyPaste + "<a href='' onclick='OpenRichEditor(\"?type={2}&field=infodoc&guid={1}&callback={1}\"); return false;' style='margin-right:4px'><img src='{0}' id='img_name_{1}' width=12 height=12 title=''></a>".f((proj.hasinfodoc ? imgInfodoc : imgNoInfodoc), proj.id, 2);
                    var altNameHTML = "{0}<span id='name{2}' title='{5}' style='cursor:pointer' onclick='switchPrjNameEditor(\"{2}\",1);'> {1}</span><input type='text' id='tbName{2}' style='width:90%; min-width:{5}ex; display:none;' value='{3}' onblur='switchPrjNameEditor(\"{2}\",0);'><input type='hidden' id='altID{2}' value='{4}'>".f(infodocLink, ShortString(proj.name, 65), proj.id, proj.name, proj.id, "Edit Project Name", proj.name.length);
                    if (proj.isFunded || !opt.fundedOnly) { $('<tr>' + (opt.index_visible ? '<td class="ra_td" style="text-align: right;' + fundedBackground + '">' + proj.idx + '</td>' : '') + '<td class="ra_td" style="white-space: nowrap;text-align: left;' + fontBold + fundedBackground + '" title="' + htmlEscape(proj.name) + '">' + altNameHTML + '</td><td style="paddin: 4px;' + fundedBackground + '" class="ra_td">' + inputField + '</td>' + resourcesString + '</tr>').appendTo('#tBody'); }; //A1143 + A1207 + A1208
                };
            };
            for (var i = 0; i < opt.projs.length; i++) {
                var proj = opt.projs[i];
                if ((opt.pageSize === 0) || ((i >= ((opt.currentPage - 1) * opt.pageSize)) && (i < opt.currentPage * opt.pageSize))) {
                    if (opt.periodsCount > 0) {
                        for (var p = 0; p < proj.periodDuration; p++) {
                            var ctxItems = [];
                            for (var r = 0; r < proj.resources.length; r++) {
                                var itemRes = proj.resources[r];
                                if (itemRes.values[p] > -2147483648) {
                                    var ctxItem = { text: '{0}: {1}'.f(itemRes.name, number2locale(itemRes.values[p], false, opt.hideCents)), res: r };
                                    ctxItems.push(ctxItem);
                                };
                            };
                            $("#context-menu"+ proj.id + '_' + p).dxContextMenu({
                                dataSource: ctxItems,
                                target: "#resInput" + proj.id + '_' + p,
                                itemTemplate: function (itemData, itemIndex, itemElement) {
                                    var background = (itemData.res == opt.resID ? "background-color:lightgrey;" : "");
                                    var template = $('<div style="'+background+'"></div>');
                                    template.append(itemData.text);
                                    return template;
                                },
                                onItemClick: function (e) {
                                    //if (!e.itemData.items) {
                                    //    DevExpress.ui.notify("The \"" + e.itemData.text + "\" item was clicked", "success", 1500);
                                    //}
                                }
                            });
                        };
                    };
                };
            };
            var scope = this;
            $(".inpresclass").focus(function () {
                $(this).select();
            });
            $(".inpresclass").change(function () {
                var newVal = $(this).val();
                var name = $(this).attr("id").replace("resInput", "");
                var projID = name.split("_")[0];
                var periodID = str2int(name.split("_")[1]);
                if (newVal == "") { $(this).val(number2locale(Number(0.0), false, opt.hideCents)) } else { $(this).val(number2locale(str2cost(newVal), false, opt.hideCents)) };
                if (last_focus_tp == $(this).attr("id")) {
                    last_focus_tp = 'resInput' + projID + '_' + (periodID + 1);
                };
                $("#RATimeline").timeline("setProjectResourceValue", str2cost(newVal), projID, periodID);
            });
            $(".inpresclass").on("keypress", function (e) {
                var name = $(this).attr("id").replace("resInput", "");
                var projID = name.split("_")[0];
                var periodID = str2int(name.split("_")[1]);
                var tabindex = str2int($(this).attr("tabindex"));
                last_focus_tp = $(this).attr("id");
                if (e.keyCode == 13) {
                    this.blur();
                    var proj = scope._getProjectByID(projID);
                    var prjIndex = opt.projs.indexOf(proj);
                    do {
                        tabindex += 1000;
                        prjIndex++;
                        var nextInput = $("input[tabindex=" + tabindex + "]");
                        var newID = nextInput.attr("id");
                    } while ((prjIndex <= opt.projs.length) && (typeof newID == "undefined"));
                    if (typeof newID != "undefined") {
                        last_focus_tp = newID;
                    };
                    scope.redrawResults();
                };
            });
            $(".inptotalclass").focus(function () { 
                $(this).select(); 
                last_focus_tp = $(this).attr("id");
            });
            $(".inptotalclass").change(function () {
                var newVal = $(this).val();
                var name = $(this).attr("id").replace("totalInput", "");
                var projID = name;
                if (newVal == "") { $(this).val(number2locale(Number(0.0), false, opt.hideCents)) } else { $(this).val(number2locale(str2cost(newVal), false, opt.hideCents)) };
                $("#RATimeline").timeline("setTotalProjectResourceValue", str2cost(newVal), projID);
            });
            $(".inptotalclass").on("keypress", function (e) {
                if (e.keyCode == 13) { this.blur();};
            });
            this.addTotalRows();
            if ((typeof last_focus_tp != "undefined") && (last_focus_tp != '')) {
                var focusedInput = $("#" + last_focus_tp);
                if ((focusedInput) && (focusedInput.length)) {
                    focusedInput.focus().select();
                };
            };
        },
        setMinValue: function (resID, pID, val, sendToServer) {
            var nVal = str2cost(val);
            var resource = this.getPortfolioResource(resID, pID);
            if ((resource.maxVal != -2147483648) && (nVal > resource.maxVal)) { nVal = resource.maxVal };
            if ((nVal != -2147483648) && (nVal < 0)) { nVal = 0 };
            resource.minVal = nVal;
            if (sendToServer) sendCommand('action=tpsetminval&res_id=' + resID + '&period_id=' + pID + '&val=' + nVal);
        },
        setMaxValue: function (resID, pID, val, sendToServer) {
            var nVal = str2cost(val);
            var resource = this.getPortfolioResource(resID, pID);
            if ((nVal != -2147483648) && (nVal < resource.minVal)) { nVal = resource.minVal };
            if ((nVal != -2147483648) && (nVal < 0)) { nVal = 0 };
            resource.maxVal = nVal;
            if (sendToServer) sendCommand('action=tpsetmaxval&res_id=' + resID + '&period_id=' + pID + '&val=' + nVal);
        },
        getPortfolioResource: function (resID, pID) {
            for (var i = 0; i < this.options.portfolioResources.length; i++) {
                var pres = this.options.portfolioResources[i];
                if ((pres.resID == resID) && (pres.periodID == pID)) {
                    return pres;
                };
            };
            return null;
        },
        copyMinRow: function (resID) {
            for (var i = 0; i < this.options.portfolioResources.length; i++) {
                var pres = this.options.portfolioResources[i];
                if (pres.resID != resID) {
                    var resVal = this.getPortfolioResource(resID, pres.periodID).minVal;
                    pres.minVal = resVal;
                };
            };
        },
        copyMaxRow: function (resID) {
            for (var i = 0; i < this.options.portfolioResources.length; i++) {
                var pres = this.options.portfolioResources[i];
                if (pres.resID != resID) {
                    var resVal = this.getPortfolioResource(resID, pres.periodID).maxVal;
                    pres.maxVal = resVal;
                };
            };
        },
        updateSolvedResults: function () {
            for (var i = 0; i < this.options.projs.length; i++) {
                var proj = this.options.projs[i];
                var res = this._getSolvedResultsByID(proj.id);
                if (res == null) {
                    proj.isFunded = false;
                    proj.periodStart = proj.periodMin;
                } else {
                    proj.isFunded = true;
                    proj.periodStart = res.start;
                };
            };
        },
        setInfeasible: function (){
            for (var i = 0; i < this.options.projs.length; i++) {
                var proj = this.options.projs[i];
                proj.isFunded = false;
                proj.periodStart = proj.periodMin;
            };
        },
        _onMouseUp: function (pos) {
            this.options.mouseDownPosition = pos;
            this.options.isMouseDown = false;
            this.redraw();
        },
        addPeriod: function () {
            if (this.options.periodsCount == 0) { enableEditButtons(true); };
            this.options.periodsCount += 1;
            sendCommand('action=add_period');
            this.redraw();
        },
        removePeriod: function () {
            this.options.periodsCount -= 1;
            for (var i = 0; i < this.options.projs.length; i++) {
                var proj = this.options.projs[i];
                if ((proj.periodMax + proj.periodDuration)>this.options.periodsCount) {
                    if (proj.periodMax > proj.periodMin){
                        proj.periodMax -= 1;
                    }else{
                        if (proj.periodMin > 0) {
                            proj.periodMin -= 1;
                            proj.periodMax -= 1;
                        } else {
                            if (proj.periodDuration > 1) {
                                proj.periodDuration -= 1;
                            };
                        };
                    };
                };
            };
            if (this.options.periodsCount == 0) { enableEditButtons(false); };
            sendCommand('action=remove_period');
            this.redraw();
        },
        _getInteractiveMapElement: function (pos) {
            for (var i = this.options.interactiveMap.length - 1; i >= 0; i--) {
                var intElement = this.options.interactiveMap[i];
                if (intElement.key.indexOf(this.options.mouseOverProjectID) > -1) {
                    var xe = intElement.rect[0];
                    var ye = intElement.rect[1];
                    var we = intElement.rect[2];
                    var he = intElement.rect[3];
                    if ((pos.x > xe) && (pos.y > ye) && (pos.x < xe + we) && (pos.y < ye + he)) {
                        return intElement;
                    };
                };
            };
            return { key: '', rect: [0, 0, 0, 0], hint: '' };
        },
        _getInteractiveMapElementByKey: function (key) {
            for (var i = this.options.interactiveMap.length - 1; i >= 0; i--) {
                var intElement = this.options.interactiveMap[i];
                if (intElement.key == key) {
                    return intElement;
                }
            };
            return { key: '', rect: [0, 0, 0, 0], hint: '' };
        },
        _getResourceNameByID: function (resID) {
            for (var i = 0; i < this.options.resources.length; i++) {
                var res = this.options.resources[i];
                if (res.id === resID){return res.name};
            };
            return '';
        },
        expandProjects: function () {
            var maxPeriod = this.options.periodsCount;
            for (var i = 0; i < this.options.projs.length; i++) {
                var proj = this.options.projs[i];
                proj.periodMax = maxPeriod - proj.periodDuration;
            };
            this.resize();
        },
        contractProjects: function () {
            for (var i = 0; i < this.options.projs.length; i++) {
                var proj = this.options.projs[i];
                proj.periodMax = proj.periodMin;
            };
            this.resize();
        },
        trimTimeline: function () {
            var maxPeriod = 1;
            for (var i = 0; i < this.options.projs.length; i++) {
                var proj = this.options.projs[i];
                if (maxPeriod < proj.periodMax + proj.periodDuration) { maxPeriod = proj.periodMax + proj.periodDuration };
            };
            this.options.periodsCount = maxPeriod;
            this.resize();
        },
        setPediodMin: function (projID, val) {
            var proj = this._getProjectByID(projID);
            var val = val - 1;
            if (val < 0) { val = 0 };
            var deltaMin = val - proj.periodMin;
            proj.periodMin = val;
            proj.periodMax += deltaMin;
            if (proj.periodMax < proj.periodMin) { proj.periodMax = proj.periodMin };
            if ((proj.periodMax + proj.periodDuration) > this.options.periodsCount) { this.options.periodsCount = proj.periodMax + proj.periodDuration };
            this.redraw();
            sendCommand('action=tpsetminmaxdur&project_id=' + proj.id + '&min=' + proj.periodMin + '&max=' + proj.periodMax + '&dur=' + proj.periodDuration + '&pcount=' + this.options.periodsCount);
        },
        setPediodMax: function (projID, val) {
            var proj = this._getProjectByID(projID);
            var val = val - 1;
            if (val < 0) { val = 0 };
            proj.periodMax = val;
            if (proj.periodMax < proj.periodMin) { proj.periodDuration = 1; proj.periodMin = proj.periodMax };
            if ((proj.periodMax + proj.periodDuration) > this.options.periodsCount) { this.options.periodsCount = proj.periodMax + proj.periodDuration };
            this.redraw();
            sendCommand('action=tpsetminmaxdur&project_id=' + proj.id + '&min=' + proj.periodMin + '&max=' + proj.periodMax + '&dur=' + proj.periodDuration + '&pcount=' + this.options.periodsCount);
        },
        setPediodDur: function (projID, val) {
            var proj = this._getProjectByID(projID);
            if (val < 1) { val = 1 };
            proj.periodDuration = val;
            if ((proj.periodMax + proj.periodDuration) > this.options.periodsCount) { this.options.periodsCount = proj.periodMax + proj.periodDuration };
            this.redraw();
            sendCommand('action=tpsetminmaxdur&project_id=' + proj.id + '&min=' + proj.periodMin + '&max=' + proj.periodMax + '&dur=' + proj.periodDuration + '&pcount=' + this.options.periodsCount);
        },
        setProjHasMust: function (projID, val) {
            var proj = this._getProjectByID(projID);
            if (val) {
                if (proj.mustPeriod < 0) { proj.mustPeriod = 0 };
                proj.hasMust = true;
            } else {
                proj.hasMust = false;
            };
            this.redraw();
            sendCommand('action=tpsetmustperiod&project_id=' + proj.id + '&val=' + proj.mustPeriod + '&hasmust=' + (proj.hasMust ? '1' : '0'));
        },
        _redrawProjectCanvasBar: function (prjCanvas, mode, proj) {
            var opt = this.options;
            var intMap = opt.interactiveMap;
            var iex, iey, iew, ieh, intEl;
            var cellMargin = opt.cellMargin;
            var mouseOverElement = opt.mouseOverElement;
            var ctx = prjCanvas.getContext('2d');
            //ctx.fillStyle = (proj.isFunded ? '#ffff80' : '#fff');
            ctx.fillStyle = '#fff';
            ctx.fillRect(0, 0, prjCanvas.width, prjCanvas.height);
            var periodColumnWidth = prjCanvas.width / opt.periodsCount;
            ctx.save();
            ctx.globalAlpha = 0.4;
            ctx.beginPath();
            ctx.fillStyle = '#aaa';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            for (i = 0; i < opt.periodsCount; i++) {
                var periodName = this._getPeriodName(i);
                ctx.fillText(periodName, i * periodColumnWidth + periodColumnWidth / 2, prjCanvas.height / 2);
            };
            ctx.globalAlpha = 0.7;
            
            var maxLeft = (opt.periodsCount - proj.periodDuration - (proj.periodMax - proj.periodMin)) * periodColumnWidth;
            var minLeft = 0;
            var rectLeft = proj.periodMin * periodColumnWidth;
            var projMustShift = 0;
            if (proj.mustPeriod >= 0) {
                projMustShift = proj.mustPeriod * periodColumnWidth;
            };
            var rectWidth = proj.periodDuration * periodColumnWidth;
            var rectHeight = prjCanvas.height;
            var rectTop = 0;
            var arcLeft = proj.periodMax * periodColumnWidth + cellMargin;
            var boundRectWidth = ((proj.periodMax + proj.periodDuration) - proj.periodMin) * periodColumnWidth - cellMargin;
                        if (this.options.viewMode != 'edit') {
                        };
            if (mouseOverElement.key.indexOf(proj.id) > -1) {
                if ((mouseOverElement.key.indexOf('mode:drag') > -1) || (mouseOverElement.key.indexOf('mode:mustdrag') > -1) || (mouseOverElement.key.indexOf('mode:resizeR') > -1)) { ctx.globalAlpha = 1; };
                if (opt.isMouseDown) {

                    if (mouseOverElement.key.indexOf('mode:resizeR') > -1) {
                        var newRectWidth = rectWidth + opt.mouseDragShift;
                        if (rectLeft + newRectWidth > minLeft + (proj.periodMax + proj.periodDuration) * periodColumnWidth - cellMargin * 2) {
                            newRectWidth = minLeft + (proj.periodMax + proj.periodDuration) * periodColumnWidth - rectLeft - cellMargin * 2;
                        };
                        if (newRectWidth < periodColumnWidth - cellMargin * 2) { newRectWidth = periodColumnWidth - cellMargin * 2 };
                        rectWidth = newRectWidth;
                        if (rectWidth > proj.periodDuration * periodColumnWidth + periodColumnWidth / 2) {
                            proj.periodDuration += 1;
                            proj.periodMax -= 1;
                            if (proj.resources[opt.resID].values.length < proj.periodDuration) { proj.resources[opt.resID].values.push(0) };
                            sendCommand('action=dur_inc&project_id=' + proj.id);
                            opt.mouseDownPosition.x += periodColumnWidth;
                        };
                        if (rectWidth < proj.periodDuration * periodColumnWidth - periodColumnWidth / 2) {
                            proj.periodDuration -= 1;
                            proj.periodMax += 1;
                            sendCommand('action=dur_dec&project_id=' + proj.id);
                            opt.mouseDownPosition.x -= periodColumnWidth;
                        };
                    };

                    if (mouseOverElement.key.indexOf('mode:moveMax') > -1) {
                        var newBoundRectWidth = boundRectWidth + opt.mouseDragShift;
                        if (rectLeft + newBoundRectWidth > minLeft + opt.periodsCount * periodColumnWidth - cellMargin * 2) {
                            newBoundRectWidth = minLeft + opt.periodsCount * periodColumnWidth - rectLeft - cellMargin * 2;
                        };
                        if (newBoundRectWidth < (proj.periodDuration * periodColumnWidth - cellMargin * 2)) { newBoundRectWidth = proj.periodDuration * periodColumnWidth - cellMargin * 2 };
                        boundRectWidth = newBoundRectWidth;
                        var currentBoundWidth = (proj.periodMax - proj.periodMin + proj.periodDuration) * periodColumnWidth - cellMargin * 2;
                        if (boundRectWidth > currentBoundWidth + periodColumnWidth / 2) {
                            proj.periodMax += 1;
                            sendCommand('action=set_max&project_id=' + proj.id + '&maxval=' + proj.periodMax)
                            opt.mouseDownPosition.x += periodColumnWidth;
                        };
                        if (boundRectWidth < currentBoundWidth - periodColumnWidth / 2) {
                            proj.periodMax -= 1;
                            sendCommand('action=set_max&project_id=' + proj.id + '&maxval=' + proj.periodMax)
                            opt.mouseDownPosition.x -= periodColumnWidth;
                        };

                    };

                    if (mouseOverElement.key.indexOf('mode:drag') > -1) {
                        var newRectLeft = rectLeft + opt.mouseDragShift;
                        if (newRectLeft < minLeft) { newRectLeft = minLeft };
                        if (newRectLeft > maxLeft) { newRectLeft = maxLeft };
                        rectLeft = newRectLeft;
                        if (rectLeft > minLeft + proj.periodMin * periodColumnWidth + periodColumnWidth / 2) {
                            proj.periodMin += 1;
                            proj.periodMax += 1;
                            sendCommand('action=minmax_inc&project_id=' + proj.id);
                            if (proj.periodMax > opt.periodsCount) { proj.periodMax = opt.periodsCount };
                            opt.mouseDownPosition.x += periodColumnWidth;
                        };
                        if (rectLeft < minLeft + proj.periodMin * periodColumnWidth - periodColumnWidth / 2) {
                            proj.periodMin -= 1;
                            proj.periodMax -= 1;
                            sendCommand('action=minmax_dec&project_id=' + proj.id);
                            opt.mouseDownPosition.x -= periodColumnWidth;
                        };
                    };

                    if (mouseOverElement.key.indexOf('mode:mustdrag') > -1) {
                        var newMustShift = projMustShift + opt.mouseDragShift;
                        if (newMustShift < 0) { newMustShift = 0 };
                        if (newMustShift / periodColumnWidth + proj.periodMin > proj.periodMax) { newMustShift = projMustShift };
                        projMustShift = newMustShift;
                        if (projMustShift > proj.mustPeriod * periodColumnWidth + periodColumnWidth / 2) {
                            proj.mustPeriod += 1;
                            if ((proj.mustPeriod + proj.periodMin) > proj.periodMax) {
                                proj.mustPeriod -= 1;
                            } else {
                                sendCommand('action=tpsetmustperiod&project_id=' + proj.id + '&val=' + proj.mustPeriod + '&hasmust=' + (proj.hasMust ? '1' : '0'));
                                opt.mouseDownPosition.x += periodColumnWidth;
                            };
                        };
                        if (projMustShift < proj.mustPeriod * periodColumnWidth - periodColumnWidth / 2) {
                            proj.mustPeriod -= 1;
                            if (proj.mustPeriod < 0) {
                                proj.mustPeriod = 0;
                            } else {
                                sendCommand('action=tpsetmustperiod&project_id=' + proj.id + '&val=' + proj.mustPeriod + '&hasmust=' + (proj.hasMust ? '1' : '0'));
                                opt.mouseDownPosition.x -= periodColumnWidth;
                            };
                        };
                    };

                    $('#inpE_' + proj.id).val(proj.periodMin + 1);
                    $('#inpL_' + proj.id).val(proj.periodMax + 1);
                    $('#inpD_' + proj.id).val(proj.periodDuration);

                    if (proj.periodMin > proj.periodStart) { proj.periodStart = proj.periodMin };
                    if (proj.periodMax < proj.periodStart) { proj.periodStart = proj.periodMax };
                };

            };


            ctx.beginPath();
            var projCol = (opt.useColors ? proj.color : '#80b0f0');
            if (proj.hasMust) {
                projCol = ColorLuminance(projCol, -0.5);
            } else { projMustShift = 0 };
            ctx.fillStyle = projCol;
            ctx.rect(rectLeft + cellMargin - 1 + projMustShift, rectTop + 4, rectWidth - cellMargin * 2, rectHeight - 8);
            ctx.fill();

            ctx.beginPath();
            ctx.lineWidth = 1.5;
            ctx.lineJoin = 'round';
            ctx.rect(rectLeft + 1, rectTop + 1, boundRectWidth + 4, rectHeight - 2);
            ctx.strokeStyle = projCol;
            ctx.stroke();
            ctx.fillStyle = '#fff';
            if (mouseOverElement.key.indexOf('mode:moveMax') > -1) {
                ctx.fillStyle = projCol;
            };

            if (!proj.hasMust) {
                ctx.beginPath();
                iex = rectLeft + boundRectWidth + 3;
                iey = rectTop + 4;
                iew = 4;
                ieh = rectHeight - 8;
                ctx.rect(iex, iey, iew, ieh);
                intEl = { key: proj.id + 'mode:moveMax', rect: [iex - 4, 0, iew + 8, rectHeight], hint: 'Change Project Latest Period' };
                var existedIntEl = this._getInteractiveMapElementByKey(intEl.key);
                if (existedIntEl.key === intEl.key) {
                    existedIntEl.rect = intEl.rect;
                } else {
                    intMap.push(intEl);
                };
                ctx.fill();
                ctx.stroke();
            };

            var addShift = 0;
            ctx.strokeStyle = '#fff';
            if ((!proj.hasMust) && (proj.periodMax + proj.periodDuration) > (proj.periodMin + proj.periodDuration)) {
                iex = rectLeft + rectWidth - rectHeight / 2 - cellMargin - 2;
                iey = rectTop + rectHeight / 2;
                iew = rectHeight - 12;
                this._drawArrow(ctx, 'right', iex, iey, iew);
                intEl = { key: proj.id + 'mode:resizeR', rect: [iex - iew / 2, 0, iew * 2, iew * 2], hint: 'Resize Project Duration' };
                var existedIntEl = this._getInteractiveMapElementByKey(intEl.key);
                if (existedIntEl.key === intEl.key) {
                    existedIntEl.rect = intEl.rect;
                } else {
                    intMap.push(intEl);
                };
                addShift = rectHeight / 2;
            };
            if ((!proj.hasMust) && proj.periodDuration > 1) {
                iex = rectLeft + rectWidth - rectHeight / 2 - cellMargin - addShift;
                iey = rectTop + rectHeight / 2;
                iew = rectHeight - 12;
                this._drawArrow(ctx, 'left', iex, iey, iew);
                intEl = { key: proj.id + 'mode:resizeR', rect: [iex - iew / 2, 0, iew * 2, iew * 2], hint: 'Resize Project Duration' };
                var existedIntEl = this._getInteractiveMapElementByKey(intEl.key);
                if (existedIntEl.key === intEl.key) {
                    existedIntEl.rect = intEl.rect;
                } else {
                    intMap.push(intEl);
                };
            };
            if ((proj.hasMust) || !((proj.periodMin == 0) && (proj.periodMax + proj.periodDuration == opt.periodsCount))) {
                iex = rectLeft + projMustShift + rectWidth / 2;
                iey = rectTop + rectHeight / 2;
                iew = rectHeight - 12;
                this._drawDragButton(ctx, 'horizontal', iex, iey, iew);
                if (!proj.hasMust) {
                    intEl = { key: proj.id + 'mode:drag', rect: [iex - iew, iey - iew, iew * 2, iew * 2], hint: 'Move Project' };
                } else {
                    intEl = { key: proj.id + 'mode:mustdrag', rect: [iex - iew, iey - iew, iew * 2, iew * 2], hint: 'Move Must Project Period' };
                };
                
                var existedIntEl = this._getInteractiveMapElementByKey(intEl.key);
                if (existedIntEl.key === intEl.key) {
                    existedIntEl.rect = intEl.rect;
                } else {
                    intMap.push(intEl);
                };
            };
            ctx.globalAlpha = 0.4;
            ctx.strokeStyle = '#80b0f0';
            ctx.lineWidth = 1;
            ctx.beginPath();
            for (i = 0; i < opt.periodsCount; i++) {
                ctx.moveTo(i * periodColumnWidth, 4);
                ctx.lineTo(i * periodColumnWidth, prjCanvas.height - 4);
                ctx.stroke();
            };
            ctx.restore();
            opt.interactiveMap = intMap;
            if (opt.periodsCount > 0) { $('#btnRemovePeriod').show(); } else { $('#btnRemovePeriod').hide(); };
        },
        onMouseMove: function (pos) {
            var opt = this.options;
            var newIntElement = this._getInteractiveMapElement(pos);
            if (opt.mouseOverElement.key !== newIntElement.key) {
                opt.mouseOverElement = newIntElement;
                var projID = opt.mouseOverProjectID;
                if (projID !== '') {
                    var proj = this._getProjectByID(projID);
                    var prjCanvas = $('#prjCanvas_' + proj.id)[0];
                    this._redrawProjectCanvasBar(prjCanvas, 'default', proj);
                };
            }; 
            if (opt.isMouseDown) {
                opt.mouseDragShift = pos.x - opt.mouseDownPosition.x;
                var projID = opt.mouseOverProjectID;
                var proj = this._getProjectByID(projID);
                var prjCanvas = $('#prjCanvas_' + proj.id)[0];
                this._redrawProjectCanvasBar(prjCanvas, 'drag', proj);
            };
        },
        prjMouseMove: function (projID, pos) {
            var opt = this.options;
            var newIntElement = this._getInteractiveMapElement(pos);
            var projID = opt.mouseOverProjectID;
            if (!opt.isMouseDown) {
                if (opt.mouseOverElement.key !== newIntElement.key) {
                    opt.mouseOverElement = newIntElement;
                    if (projID !== '') {
                        var proj = this._getProjectByID(projID);
                        var prjCanvas = $('#prjCanvas_' + proj.id)[0];
                        this._redrawProjectCanvasBar(prjCanvas, 'coord: ' + number2locale(pos.x, false, opt.hideCents), proj);
                    };
                };
            } else {
                if (projID !== '') {
                    opt.mouseDragShift = pos.x - opt.mouseDownPosition.x;
                    var proj = this._getProjectByID(projID);
                    var prjCanvas = $('#prjCanvas_' + proj.id)[0];
                    this._redrawProjectCanvasBar(prjCanvas, 'coord: ' + number2locale(pos.x, false, opt.hideCents), proj);
                };
            };

        },
        prjMouseDown: function (projID, pos) {
            var opt = this.options;
            opt.mouseOverProjectID = projID;
            opt.mouseDownPosition = pos;
            opt.isMouseDown = true;
            opt.mouseOverElement = this._getInteractiveMapElement(pos);
        },
        prjMouseUp: function (pos) {
            var projID = this.options.mouseOverProjectID;
            this.options.isMouseDown = false;
            if (projID !== '') {
                var proj = this._getProjectByID(projID);
                var prjCanvas = $('#prjCanvas_' + proj.id)[0];
                this.options.mouseDownPosition = pos;
                this._redrawProjectCanvasBar(prjCanvas, 'default', proj);
            };
            
        },
        prjMouseEnter: function (projID) {
                this.options.isMouseDown = false;
                this.options.mouseOverProjectID = projID;
                var proj = this._getProjectByID(projID);
                var prjCanvas = $('#prjCanvas_' + proj.id)[0];
                this._redrawProjectCanvasBar(prjCanvas, 'default', proj);
        },
        prjMouseLeave: function (projID) {
                this.options.mouseOverProjectID = '';
                this.options.isMouseDown = false;
                this.options.mouseOverElement = this._getInteractiveMapElement({x:-1,y:-1});
                var proj = this._getProjectByID(projID);
                var prjCanvas = $('#prjCanvas_' + proj.id)[0];
                this._redrawProjectCanvasBar(prjCanvas, 'default', proj);
        },
        redraw: function () {
            var opt = this.options;
            opt.interactiveMap = [];
            this.createTimeperiodTable();
            var fundedBackground = '';
            var fontBold = '';
            for (var i = 0; i < opt.projs.length; i++) {
                var proj = opt.projs[i];
                fundedBackground = (proj.isFunded ? 'background-color:#d1e6b5;' : '');
                fontBold = '';  // (proj.isFunded ? 'font-weight: bold;' : ''); D4341
                var hasMust = (proj.hasMust ? 'checked="checked"' : '');
                var resourcesString = '<td id="tdProj_' + proj.id + '" colspan="' + opt.periodsCount + '" class="ra_td"><canvas height="24px" width="'+(opt.periodsCount*90)+'px" class="prjcanvasclass" id="prjCanvas_' + proj.id + '" ></td>';
                var inputPeriodParamsString = '<td class="ra_td" style="text-align: center;' + fundedBackground + '"><input type="text" class="inpperiodclass" style="text-align: right; width: 40px;" id="inpE_' + proj.id + '" value="' + (opt.periodsCount > 0 ? (proj.periodMin + 1) : '') + '" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false"></td>';
                inputPeriodParamsString += '<td class="ra_td" style="text-align: center;' + fundedBackground + '"><input type="text" class="inpperiodclass" style="text-align: right; width: 40px;" id="inpL_' + proj.id + '" value="' + (opt.periodsCount > 0 ? (proj.periodMax + 1) : '') + '" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false"></td>';
                inputPeriodParamsString += '<td class="ra_td" style="text-align: center;' + fundedBackground + '"><input type="text" class="inpperiodclass" style="text-align: right; width: 40px;" id="inpD_' + proj.id + '" value="' + (opt.periodsCount > 0 ? (proj.periodDuration) : '') + '" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false"></td>';
                inputPeriodParamsString += '<td class="ra_td" style="text-align: center;' + fundedBackground + '"><input type="checkbox" class="hasmustclass" style="width: 40px;" id="inpMust_' + proj.id + '" ' + hasMust + '></td>';
                var totalVal = proj.resources[opt.resID].totalValue;
                var totalValStr = (totalVal > -2147483648 ? number2locale(totalVal, false, opt.hideCents) : '');
                var imgInfodoc = opt.imgPath + opt.imgInfodoc;
                var imgNoInfodoc = opt.imgPath + opt.imgNoInfodoc;
                var infodocLink = "<a href='' onclick='OpenRichEditor(\"?type={2}&field=infodoc&guid={1}&callback={1}\"); return false;' style='margin-right:4px'><img src='{0}' id='img_name_{1}' width=12 height=12 title=''></a>".f((proj.hasinfodoc ? imgInfodoc : imgNoInfodoc), proj.id, 2);
                var altNameHTML = "{0}<span id='name{2}' title='{5}' style='cursor:pointer' onclick='switchPrjNameEditor(\"{2}\",1);'>{1}</span><input type='text' id='tbName{2}' style='width:90%; min-width:{5}ex; display:none;' value='{3}' onblur='switchPrjNameEditor(\"{2}\",0);'><input type='hidden' id='altID{2}' value='{4}'>".f(infodocLink, ShortString(proj.name, 65), proj.id, proj.name, proj.id, "Edit Project Name", proj.name.length);
                $('<tr>' + (opt.index_visible ? '<td class="ra_td" style="text-align: right;' + fundedBackground + '">' + proj.idx + '</td>' : '') + '<td class="ra_td" style="white-space: nowrap;text-align: left;' + fontBold + fundedBackground + '" title="' + htmlEscape(proj.name) + '">' + altNameHTML + '</td><td style="text-align: right;' + fundedBackground + '" class="ra_td">' + totalValStr + '</td>' + inputPeriodParamsString + resourcesString + '</tr>').appendTo('#tBody'); // A1208
                var prjCanvas = $('#prjCanvas_' + proj.id)[0];
                var prjtd = $('#tdProj_' + proj.id);
                fitToContainer(prjCanvas);
                this._redrawProjectCanvasBar(prjCanvas, 'default', proj);
            };
            $("#RATimeline").on("mousemove", function (event) {
                return false;
            });
            $("#RATimeline").on("mouseup", function (event) {
                $("#RATimeline").timeline("prjMouseUp", { x: -1, y: -1 });
                return false;
            });
            $(".prjcanvasclass").on("mousemove", function (event) {
                var pos = getRelativePosition(event.target, event.clientX, event.clientY);
                var idName = $(this).attr("id");
                var projID = idName.replace('prjCanvas_', '');
                $("#RATimeline").timeline("prjMouseMove", projID, pos);
                return false;
            });
            $(".prjcanvasclass").on("mousedown", function (event) {
                var pos = getRelativePosition(event.target, event.clientX, event.clientY);
                var idName = $(this).attr("id");
                var projID = idName.replace('prjCanvas_', '');
                $("#RATimeline").timeline("prjMouseDown", projID, pos);
                return false;
            });
            $(".prjcanvasclass").on("mouseenter", function () {
                var idName = $(this).attr("id");
                var projID = idName.replace('prjCanvas_', '');
                $("#RATimeline").timeline("prjMouseEnter", projID);
            });
            $(".prjcanvasclass").on("mouseleave", function () {
                var idName = $(this).attr("id");
                var projID = idName.replace('prjCanvas_', '');
                $("#RATimeline").timeline("prjMouseLeave", projID);
            });
            $(".prjcanvasclass").on("mouseup", function (event) {
                var pos = getRelativePosition(event.target, event.clientX, event.clientY);
                $("#RATimeline").timeline("prjMouseUp", pos);
            });
            $(".inpperiodclass").focus(function () { $(this).select(); });
            $(".inpperiodclass").on("keypress", function (e) {
                if (e.keyCode == 13) { this.blur() };
            });
            $(".inpperiodclass").change(function () {
                var newVal = $(this).val();
                var idName = $(this).attr("id");
                var prjID = '';
                var newIntVal = 1;
                if (Math.floor(newVal) == newVal && $.isNumeric(newVal)) {
                    newIntVal = str2int(newVal);
                    if (newIntVal < 1) { newIntVal = 1 };
                    $(this).val(newIntVal);
                    if (idName.indexOf('inpE') >= 0) {
                        prjID = idName.replace('inpE_', '');
                        $("#RATimeline").timeline("setPediodMin", prjID, newIntVal);
                    };
                    if (idName.indexOf('inpL') >= 0) {
                        prjID = idName.replace('inpL_', '');
                        $("#RATimeline").timeline("setPediodMax", prjID, newIntVal);
                    };
                    if (idName.indexOf('inpD') >= 0) {
                        prjID = idName.replace('inpD_', '');
                        $("#RATimeline").timeline("setPediodDur", prjID, newIntVal);
                    };
                };
            });
            $(".hasmustclass").click(function () {
                var idName = $(this).attr("id");
                var isChecked = false;
                if ($(this).is(':checked')) {
                    isChecked = true;
                };
                if (idName.indexOf('inpMust_') >= 0) {
                    prjID = idName.replace('inpMust_', '');
                    $("#RATimeline").timeline("setProjHasMust", prjID, isChecked);
                };
            });
        },
        _getSolvedResultsByID: function (id) {
            var opt = this.options;
            for (var i = 0; i < opt.solvedResults.length; i++) {
                var res = opt.solvedResults[i];
                if (res.projID == id) { return res };
            };
            return null;
        },
        _getProjectByID: function (id) {
            for (var i = 0; i < this.options.projs.length; i++) {
                var proj = this.options.projs[i];
                if (proj.id == id){return proj};
            };
            return null;
        },
        _getProjectByIdx: function (idx) {
            for (var i = 0; i < this.options.projs.length; i++) {
                var proj = this.options.projs[i];
                if (proj.idx == idx) { return proj };
            };
            return null;
        },
        setTotalProjectResourceValue: function (val, projID) {
            var opt = this.options;
            var proj = this._getProjectByID(projID)
            if (proj != null) {
                var totalVal = 0;
                for (var i = 0; i < proj.periodDuration; i++) {
                    totalVal += proj.resources[opt.resID].values[i];
                };
                proj.resources[opt.resID].totalValue = val;
                if (totalVal !== proj.resources[opt.resID].totalValue) {
                    var pVal = proj.resources[opt.resID].totalValue / proj.periodDuration;
                    for (var i = 0; i < proj.periodDuration; i++) {
                        proj.resources[opt.resID].values[i] = pVal;
                    };
                };
                sendCommand('action=settotalresource&res_id=' + opt.selectedResourceID + '&project_id=' + proj.id + '&value=' + val);
                this.redrawResults();
            };
        },
        distributeResourceValues: function (val, projID, periodID) {
            var opt = this.options;
            var proj = this._getProjectByID(projID);
            if (proj != null) {
                var res = proj.resources[opt.resID];
                if (opt.distribMode == 0) {
                    if (val > proj.resources[opt.resID].totalValue) { val = proj.resources[opt.resID].totalValue };
                    var resSum = 0.0;
                    for (var i = 0; i < res.values.length; i++) {
                        if (i !== periodID) {
                            resSum += res.values[i];
                        };
                    };
                    var distribVal = proj.resources[opt.resID].totalValue - val;
                    res.values[periodID] = val;
                    if ((distribVal >= 0) && (res.values.length > 1)) {
                        var sValues = '';
                        for (var i = 0; i < res.values.length; i++) {
                            if (i !== periodID) {
                                res.values[i] = (distribVal / (res.values.length - 1));
                            };
                        };
                    };
                };
                if (opt.distribMode == 1) {
                    var resSum = 0.0;
                    for (var i = 0; i < res.values.length; i++) {
                        if (i < periodID) {
                            resSum += res.values[i];
                        };
                    };
                    if ((val + resSum) > proj.resources[opt.resID].totalValue) { val = proj.resources[opt.resID].totalValue - resSum };
                    if (((val + resSum) < proj.resources[opt.resID].totalValue) && (periodID == res.values.length - 1)) { val = proj.resources[opt.resID].totalValue - resSum };
                    var distribVal = proj.resources[opt.resID].totalValue - (val + resSum);
                    res.values[periodID] = val;
                    if ((distribVal >= 0) && (res.values.length > 1)) {
                        var sValues = '';
                        for (var i = 0; i < res.values.length; i++) {
                            if (i > periodID) {
                                res.values[i] = (distribVal / (res.values.length - periodID - 1));
                            };
                        };
                    };
                };
                if (opt.distribMode == 2) {
                    var resSum = 0.0;
                    res.values[periodID] = val;
                    for (var i = 0; i < res.values.length; i++) {
                        resSum += res.values[i];
                    };
                    proj.resources[opt.resID].totalValue = resSum;
                };
            };
        },
        setProjectResourceValue: function (val, projID, periodID) {
            var opt = this.options;
            var proj = this._getProjectByID(projID);
            if (proj != null) {
                var res = proj.resources[opt.resID];
                var oldResVal = res.values[periodID];
                this.distributeResourceValues(val, projID, periodID);
                if ((res.values.length > 1)) {
                    var sValues = '';
                    for (var i = 0; i < res.values.length; i++) {
                        if (sValues !== '') { sValues += 'x'; };
                        sValues += res.values[i];
                    };
                    sendCommand('action=setresource&res_id=' + opt.selectedResourceID + '&project_id=' + proj.id + '&values=' + sValues + '&total=' + proj.resources[opt.resID].totalValue + '&dm=' + opt.distribMode);
                };
            };
            if (res.values.length == 1) {
                if (val != proj.resources[opt.resID].totalValue) { res.values[periodID] = proj.resources[opt.resID].totalValue };
            };
            this.redrawResults();
        },
        setTPPage: function (pageID) {
            this.options.currentPage = Number(pageID);
            if (this.options.viewMode === 'results') {
                this.redrawResults();
            } else {
                this.redraw();
            };
            this._trigger('setPage');
        },
        _calculateTotalRow: function () {
            var opt = this.options;
            opt.totalRow = [];
            opt.fundedRow = [];
            for (var p = 0; p < opt.periodsCount; p++) {
                var total = 0;
                var funded = 0;
                for (var i = 0; i < opt.projs.length; i++) {
                    var proj = opt.projs[i];
                    if ((proj.periodStart <= p)&&((proj.periodStart + proj.periodDuration) > p)) {
                        var ridx = p - proj.periodStart;
                        var aVal = proj.resources[opt.resID].values[ridx];
                        if (aVal > -2147483648) {
                            if (proj.isFunded) { funded += aVal };
                            total += aVal;
                        };
                    };
                };
                opt.fundedRow.push(funded);
                opt.totalRow.push(total);
            };
        },
        _drawArrow: function (ctx, direction, x, y, size) {
            var halfSize = size / 2;
            ctx.lineWidth = 3;
            ctx.beginPath();
            switch (direction) {
                case 'right':
                    ctx.moveTo(x, y - halfSize);
                    ctx.lineTo(x + halfSize, y);
                    ctx.lineTo(x, y + halfSize);
                    break;
                case 'left':
                    ctx.moveTo(x, y - halfSize);
                    ctx.lineTo(x - halfSize, y);
                    ctx.lineTo(x, y + halfSize);
                    break;
                case 'up':
                    ctx.moveTo(x - halfSize, y);
                    ctx.lineTo(x, y - halfSize);
                    ctx.lineTo(x + halfSize, y);
                    break;
                case 'down':
                    ctx.moveTo(x - halfSize, y);
                    ctx.lineTo(x, y + halfSize);
                    ctx.lineTo(x + halfSize, y);
                    break;
            };
            ctx.stroke();
        },
        _drawDragButton: function (ctx, orientation, x, y, size) {
            var halfSize = size / 2;
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.moveTo(x - 4, y - halfSize);
            ctx.lineTo(x - 4, y + halfSize);
            ctx.stroke();
            ctx.beginPath();
            ctx.moveTo(x, y - halfSize);
            ctx.lineTo(x, y + halfSize);
            ctx.stroke();
            ctx.beginPath();
            ctx.moveTo(x + 4, y - halfSize);
            ctx.lineTo(x + 4, y + halfSize);
            ctx.stroke();
        }
    });
})(jQuery);

function getRelativePosition(canvas, absX, absY) {
    var rect = canvas.getBoundingClientRect();
    return {
        x: absX - rect.left,
        y: absY - rect.top
    };
}

function isNumeric( obj ) {
    // parseFloat NaNs numeric-cast false positives (null|true|false|"")
    // ...but misinterprets leading-number strings, particularly hex literals ("0x...")
    // subtraction forces infinities to NaN
    // adding 1 corrects loss of precision from parseFloat (#15100)
    return !jQuery.isArray( obj ) && (obj - parseFloat( obj ) + 1) >= 0;
}

function fitToContainer(canvas) {
    canvas.style.width = '100%';
    canvas.style.height = '100%';
    canvas.width = canvas.offsetWidth;
    canvas.height = canvas.offsetHeight;
}

function ColorLuminance(hex, lum) {
    // validate hex string
    hex = String(hex).replace(/[^0-9a-f]/gi, '');
    if (hex.length < 6) {
        hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
    }
    lum = lum || 0;
    // convert to decimal and change luminosity
    var rgb = "#", c, i;
    for (i = 0; i < 3; i++) {
        c = parseInt(hex.substr(i * 2, 2), 16);
        c = Math.round(Math.min(Math.max(0, c + (c * lum)), 255)).toString(16);
        rgb += ("00" + c).substr(c.length);
    }
    return rgb;
}

function changeResource(id) {
    var sCommand = 'action=changeresourceid&res_id=' + id;
    sendCommand(sCommand, true);
    $("#RATimeline").timeline("option", "selectedResourceID", id);
    $("#RATimeline").timeline("resize");
}

function setTPPage(id) {
    $("#RATimeline").timeline("setTPPage", id);
}

var last_focus_tp = '';

function copyMinRow(resID) {
    $("#RATimeline").timeline("copyMinRow", resID);
    sendCommand('action=tpcopyminrow&res_id=' + resID);
}

function copyMaxRow(resID) {
    $("#RATimeline").timeline("copyMaxRow", resID);
    sendCommand('action=tpcopymaxrow&res_id=' + resID);
}

var prj_name = "";

function switchPrjNameEditor(id, vis) {
    if (vis == 1) {
        if (prj_name == "" || prj_name == id || (prj_name !== "" && prj_name != id && switchPrjNameEditor(prj_name, 0))) {
            var w = $("#name"+id).width() + 80;
            $("#name"+id).hide();
            $("#tbName"+id).width(w-2).show().select().focus();
            prj_name = id;
            alt_value = $("#tbName"+id).val();
            on_hit_enter = "switchPrjNameEditor('" + id + "', 0);";
        }
    } else {
        var val = $("#tbName"+id).val();
        if (val=="") {
            dxDialog("Name is empty", true,  'setTimeout(\'$("#tbName'+id+'").focus();\', 50);', '');
            return false;
        }
        if (vis==0 && val!="" && alt_value!="" && val!=alt_value) saveTPAltName(id, val);
        $("#tbName"+id).hide();
        $("#name"+id).show().focus();
        prj_name = "";
        alt_value = "";
        on_hit_enter = "";
    }
    return true;
}

function highlightRow(prjID, isEnter) {
    $(".dg_table").find("input[id^='resInput" + prjID + "']").css("background-color", isEnter ? "#D1E6B5" : "white");
}

function onSyncClick(name, value, resID, periodsCount) {
    localStorage.setItem('tpSync' + curPrjID, value);
    $(".syncmin").prop("disabled", value);
    $(".syncmax").prop("disabled", value);
    if (value) {
        var min = $('#resInputMin0_' + resID).val();
        var max = $('#resInputMax0_' + resID).val();
        $(".syncmin").val(min);
        $(".syncmax").val(max);
        for (var tp = 0; tp < periodsCount; tp++) {
            $("#RATimeline").timeline("setMinValue", resID, tp, min, false);
            $("#RATimeline").timeline("setMaxValue", resID, tp, max, false);
        };
        sendCommand('action=tpsetallminmaxval&res_id=' + resID + '&minval=' + min + '&maxval=' + max);
    };
}

function onSyncMinClick(resID, periodsCount) {
    var min = $('#resInputMin0_' + resID).val();
    $(".syncmin").val(min);
    for (var tp = 0; tp < periodsCount; tp++) {
        $("#RATimeline").timeline("setMinValue", resID, tp, min, false);
    };
    sendCommand('action=tpsetallminmaxval&res_id=' + resID + '&minval=' + min);
}

function onSyncMaxClick(resID, periodsCount) {
    var max = $('#resInputMax0_' + resID).val();
    $(".syncmax").val(max);
    for (var tp = 0; tp < periodsCount; tp++) {
        $("#RATimeline").timeline("setMaxValue", resID, tp, max, false);
    };
    sendCommand('action=tpsetallminmaxval&res_id=' + resID + '&maxval=' + max);
}

function copyColumn(colID) {
    var data = '';
    for (i = 0; i < projects.length; i++) {
        var prj = projects[i];
        var pmin = prj.periodMin;
        var pmax = prj.periodMax;
        var pdur = prj.periodDuration;
        var value = 1;
        switch (colID) {
            case 0:
                value = pmin + 1;
                break;
            case 1:
                value = pmax + 1;
                break;
            case 2:
                value = pdur;
                break;
        };
        data += (data == "" ? "" : "\r\n") + value;
    };
    copyDataToClipboard(data);
}