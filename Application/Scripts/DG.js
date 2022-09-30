/* javascript plug-ins for Datagrid based widgets by SL */
var chArrowUp = '&#x25B2;';
var chArrowDown = '&#x25BC;';
(function ($) {
    $.widget("dg.datagrid", {
        options: {
            columns: [{ id: '0', idx: 1, title: 'Column 1', nodeid:0},
                      { id: '1', idx: 2, title: 'Column 2', nodeid:1}],
            rows: [{ id: '0', idx: 1, title: 'Row 1', nodeid: 0, attrvals: [1, '2'], show: 1, cont: [1, 1] },
                   { id: '0', idx: 2, title: 'Row 2', nodeid: 1, attrvals: [3, '4'], show: 1, cont: [0, 0] }],
            viewMode: 'roles',
            gridContent: [],
            wrtInfodocs: [],
            nodeInfodocs: [],
            hierarchyNodes: [],
            pageSize: 15,
            currentPage: 1,
            searchbyname: true,
            showAttributes: '', //A1201 - string of attribute guids to show, delimited with "," (SelectedColumns)
            attributes: [{ guid: 'guid0', name: 'Name0', type: 'string', search: true }, { guid: 'guid1', name: 'Name1', type: 'int', search: true }],
            indexColumn: '', //A1202  '' - hidden, 'id' - ID column, 'index' - Index column
            contextMenu: true, //A1203 - "View Objectives/Sources", "Delete", etc. context menu
            hid: -1, //A1203 - active hierarchy ID, -1 if not a Risk project
            table: null,
            header: null,
            body: null,
            hierarchyHeaderHTML: '',
            imgPath: '',
            maxAltID: 0,
            sortMode: {columnName: 'Name', mode:'none'},
            _last_checked_cell: [-1, -1, false] // A1200 - [i, j, chk (0:1)]
        },
        _create: function () {
            this.options.table = this.element[0];
            this.redraw();
        },
        redrawRows:function() {
            var imgPath = this.options.imgPath;
            var id = this.options.table.id;
            $(this.options.body).empty();

            var maxAltIndex = this.options.rows.length + 1;
            var maxAltID = this.options.maxAltID;

            var dgRows = '';
            var pageItems = 0;
            var shownItems = 0;
            var skipRows = 0;
            var totalRows = 0;
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                if (row.show == 1) {
                    totalRows += 1;
                };
            };
            if (totalRows < 30) { this.options.pageSize = 30 } else { this.options.pageSize = 15};
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                if (row.show == 1) {
                    if (skipRows < (this.options.pageSize * (this.options.currentPage - 1))) {
                        skipRows += 1;
                    };
                };
            };
            var totalPages = Math.ceil(totalRows / this.options.pageSize);
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                if (row.show == 1) {
                    if ((pageItems >= skipRows) && (pageItems < (this.options.pageSize * this.options.currentPage))) {
                        var sRow = '';
                        var rowControl = '<input type="checkbox" class="input_selectRow*ind*" id="selectRow' + row.nodeid + '" *rowcheck* >';
                        switch (this.options.viewMode) {
                            case 'roles':
                                rowControl = '<div class="dg_roles_div_in"></div>';
                                break;
                            case 'contributions':

                                break;
                            case 'infodocs':
                                var isInfo = this.isNodeInfodoc(row.id);
                                var imgUrl = 'icon_newdocument.png';
                                if (isInfo) {
                                    imgUrl = 'icon_document.png';
                                };
                                rowControl = '<img id="altInfodoc' + row.id + '" class="input_altInfodoc" style="cursor:pointer" src="' + imgPath + imgUrl + '">';
                                break;
                            default:

                        };
                        var rowContent = [];
                        var rowAllChecked = true;
                        var rowAllUnchecked = true;
                        var sIDColumnValue = "";
                        switch (this.options.indexColumn) {
                            case "id":
                                sIDColumnValue = '<td class="dg_fixed_column">' + padWithZeros(row.nodeid + 1, maxAltID) + '</td>';
                                //sIDColumnValue = '<td class="dg_fixed_column">' + (row.nodeid + 1) + '</td>';
                                break;
                            case "index":
                                sIDColumnValue = '<td class="dg_fixed_column">' + padWithZeros(i + 1, maxAltIndex) + '</td>';
                                //sIDColumnValue = '<td class="dg_fixed_column">' + (i + 1) + '</td>';
                                break;
                        }
                        //A1203 === Context Menu for the Alt Title
                        var context_menu = (this.options.contextMenu ? "<img align='right' style='vertical-align:top;cursor:hand;margin-right:3px;' src='" + imgPath + "menu_dots.png' alt='' onclick='showEventMenu(this," + row.nodeid + ",&apos;" + row.title + "&apos;,&apos;" + imgPath + "&apos;," + this.options.hid + ");' >" : "");
                        //A1203 ==
                        var rowAttrs = '';
                        //A1201 - add visible attributes columns values
                        for (var k = 0; k < this.options.attributes.length; k++) {
                            var attr = this.options.attributes[k];
                            if (this.options.showAttributes.indexOf(attr.guid) != -1) {
                                rowAttrs += '<td align="left" class="text dg_cell dg_' + attr.type + '">' + row.attrvals[k] + '</td>';
                            }
                        };

                        sRow += '<tr id="dgRow' + i + '">' + sIDColumnValue + '<td class="dg_fixed_column" style="text-align:center;">' + rowControl + '</td><td class="dg_fixed_column" title="' + row.title + '"><div id="lblName' + row.nodeid + '"><span style="cursor:pointer" onclick="switchAltNameEditor(' + row.nodeid + ',1);">' + htmlEscape(row.title) + '</span>' + context_menu + '</div><input type="text" id="tbName' + row.nodeid + '" style="width:99%; display:none;" value="' + row.title + '" onblur="switchAltNameEditor(' + row.nodeid + ',0);"></td>' + rowAttrs;

                        for (var j = 0; j < this.options.columns.length; j++) {
                            var col = this.options.columns[j];
                            var cellControl = '';
                            switch (this.options.viewMode) {
                                case 'roles':
                                    cellControl = '<div class="dg_roles_div_out"><div class="dg_roles_div_in" onclick="roleClicked(this);"  id="cell' + i + '-' + j + '" title="' + col.title + ' / ' + row.title + '"></div></div>';
                                    break;
                                case 'contributions':
                                    var isContributed = (row.cont[j] == 1);
                                    var checkedString = '';
                                    if (isContributed) {
                                        checkedString = 'checked = "checked"';
                                        rowContent.push(1);
                                        rowAllUnchecked = false;
                                    } else {
                                        rowContent.push(0);
                                        rowAllChecked = false;
                                    };
                                    cellControl = '<input class="input_cbCell" id="cbCell' + row.nodeid + 'x' + j + '" type="checkbox" ' + checkedString + ' title="' + col.title + ' / ' + row.title + '" i="' + i + '" j="' + j + '" >'; //A1200
                                    break;
                                case 'infodocs':
                                    var isInfo = this.isWRTInfodoc(i, j);
                                    var imgUrl = 'icon_newdocument.png';
                                    if (isInfo) {
                                        imgUrl = 'icon_document.png';
                                    };
                                    cellControl = '<img class="input_wrtInfodoc" id="wrtInfodoc' + row.id + '_' + col.id + '" style="cursor:pointer" src="' + imgPath + imgUrl + '" title="' + col.title + ' / ' + row.title + '">';
                                    break;
                                default:

                            };
                            sRow += '<td align="center" class="dg_cell">' + cellControl + '</td>';
                        };
                        sRow = sRow.replace('*rowcheck*', (rowAllChecked ? 'checked="checked"' : ''));
                        sRow = sRow.replace('*ind*', (!rowAllChecked) && (!rowAllUnchecked) ? ' input_ind' : '');
                        this.options.gridContent.push(rowContent);
                        sRow += '</tr>';
                        dgRows += sRow;
                        shownItems += 1;
                    };
                    pageItems += 1;
                };
            };
            if (this.options.pageSize < totalRows) {
                var prevDisabledStr = "disabled='disabled'";
                if (this.options.currentPage > 1) {
                    prevDisabledStr = "";
                };
                var nextDisabledStr = "disabled='disabled'";
                if (this.options.currentPage < totalPages) {
                    nextDisabledStr = "";
                };
                var pagesLinksStr = "";
                for (var i = 1; i <= totalPages; i++) {
                    var disabled = 
                    pagesLinksStr += "<input type='button' value='" + i + "' " + (i == this.options.currentPage ? "disabled" : "class='dg_pgbtn'") + "> ";
                };
                var showMoreRowsString = "<tr id='trMoreRows'><td colspan='" + (this.options.columns.length + 3) + "' class='dg_cell'><br><input id='btnPrevPage' type='button' class='button' value='Prev Page' " + prevDisabledStr + " ><input id='btnNextPage' type='button' class='button' value='Next Page' " + nextDisabledStr + " > (" + this.options.currentPage + "/" + totalPages + " Page)  "+pagesLinksStr+"</td></tr>";
                dgRows += showMoreRowsString;
            };
            $(dgRows).appendTo(this.options.body);

            $('#btnPrevPage').on('click', function () {
                showPrevPage();
            });

            $('#btnNextPage').on('click', function () {
                showNextPage();
            });

            $('.dg_pgbtn').on('click', function () {
                showPage(this.value);
            });

            if (this.options.viewMode == 'contributions') {
                $('.input_selectRow').on('click', function () {
                    setRowContributions(this);
                });
                $('.input_ind').prop("indeterminate", true);
                $('.input_cbCell').on('click', function (event) {
                    setCellContribution(event, this);
                });
            };

            if (this.options.viewMode == 'infodocs') {
                $('.input_altInfodoc').on('click', function () {
                    var altID = this.id.replace('altInfodoc', '');
                    OpenRichEditor('?type=2&field=infodoc&guid=' + altID + '&callback=altInfodoc' + altID);
                });
                $('.input_wrtInfodoc').on('click', function () {
                    var pairID = this.id.replace('wrtInfodoc', '');
                    var altID = pairID.split('_')[0];
                    var objID = pairID.split('_')[1];
                    OpenRichEditor('?type=3&field=infodoc&guid=' + altID + '&pguid=' + objID + '&callback=wrtInfodoc' + pairID);
                });
            };

            var allCellsChecked = true;
            var allCellsUnchecked = true;
            for (var j = 0; j < this.options.columns.length; j++) {
                var colAllChecked = true;
                var colAllUnchecked = true;
                for (var i = 0; i < this.options.rows.length; i++) {
                    if (row.show == 1) {
                        if (typeof this.options.gridContent[i] !== 'undefined') {
                            if (this.options.gridContent[i][j] == 1) { colAllUnchecked = false; allCellsUnchecked = false };
                            if (this.options.gridContent[i][j] == 0) { colAllChecked = false; allCellsChecked = false };
                        };
                    };
                };
                if (colAllChecked) { $('#selectCol' + j).prop('checked', true) };
                if ((!colAllChecked) && (!colAllUnchecked)) { $('#selectCol' + j).prop("indeterminate", true) };
            };
            if (allCellsChecked) { $('#selectAllcb').prop('checked', true) };
            if ((!allCellsChecked) && (!allCellsUnchecked)) { $('#selectAllcb').prop("indeterminate", true) };
        },
        redraw: function () {
            var imgPath = this.options.imgPath;
            var id = this.options.table.id;
            //$(this.options.table).floatThead('destroy');
            $(this.options.table).empty();
            var sIDColumnHeader = '';            
            switch (this.options.indexColumn) {
                case "id":
                    sIDColumnHeader = '<th class="dg_th">ID</th>';
                    break;
                case "index":
                    sIDColumnHeader = '<th class="dg_th">Index</th>';
                    break;
            }            
            this.options.header = $('<thead style="background: white;">' + this.options.hierarchyHeaderHTML + '<tr id="headerCheckRow">' + sIDColumnHeader + '</tr></thead>', {});
            $(this.options.table).append(this.options.header);
            if (this.options.viewMode == 'contributions') {
                    $('<th class="dg_th" title="Check / Uncheck All"><input type="checkbox" id="selectAllcb"></th>').appendTo('#headerCheckRow');
                    $('#selectAllcb').on('click', function () {
                            setAllContributions(this.checked);
                        });
                } else {
                $('<th class="dg_th"></th>').appendTo('#headerCheckRow');
            };
            $('<th class="dg_th sortthstr" style="cursor:pointer;" id="' + id + '_sortChr">Name</div></th>').appendTo('#headerCheckRow');
            $('.sortthstr').on('click', function () {
                $('#' + id).datagrid('sort');
            });

            var j = 0;
            for (var i = 0; i < this.options.hierarchyNodes.length; i++) {
                var obj = this.options.hierarchyNodes[i];
                $('<div>' + htmlEscape(obj.title) + '</div>').appendTo('#objColumn' + obj.id);
                if (this.options.viewMode == 'infodocs') {
                    var isInfo = this.isNodeInfodoc(obj.id);
                    var imgUrl = 'icon_newdocument.png';
                    if (isInfo) {
                        imgUrl = 'icon_document.png';
                    };
                    $('<img id="objInfodoc' + obj.id + '" style="cursor:pointer" src="' + imgPath + imgUrl + '" >').appendTo('#objColumn' + obj.id);
                };
                if ((obj.isTerminal)&&(this.options.viewMode == 'contributions')) {
                    $('<input type="checkbox" id="selectCol' + j + '">').appendTo('#objColumn' + obj.id);
                    j++;
                };
            };

            //A1201 - add visible attributes columns headers
            var visible_attributes_count = 0;
            for (var k = 0; k < this.options.attributes.length; k++) {
                var attr = this.options.attributes[k];
                if (this.options.showAttributes.indexOf(attr.guid) != -1) {
                    visible_attributes_count += 1;
                    $('<th class="text dg_th">' + htmlEscape(attr.name) + '</th>').appendTo('#headerCheckRow');
                }
            };

            $("th.thHeader0").attr("colspan", (this.options.indexColumn == '' ? 2 : 3) + visible_attributes_count);
            var maxAltIndex = this.options.rows.length + 1;
            var maxAltID = 0;
            $.each(this.options.rows, function (index, value) { var v = value.nodeid * 1; if (maxAltID < v) maxAltID = v; });
            this.options.maxAltID = maxAltID;

            for (var i = 0; i < this.options.columns.length; i++) {
                var col = this.options.columns[i];
                //var colControl = '';
                //switch (this.options.viewMode) {
                //    case 'roles':
                //        colControl = '<div class="dg_roles_div_in"></div>';
                //        $('<th class="dg_th" title="' + col.title + '">' + colControl + '</th>').appendTo('#headerCheckRow');
                //        break;
                //    case 'contributions':
                //        //colControl = '<input type="checkbox" id="selectCol' + i + '">';
                //        break;
                //    case 'infodocs':
                //        var isInfo = this.isNodeInfodoc(col.id);
                //        var imgUrl = 'icon_newdocument.png';
                //        if (isInfo) {
                //            imgUrl = 'icon_document.png';
                //        };
                //        colControl = '<img id="objInfodoc' + col.id + '" style="cursor:pointer" src="' + imgPath + imgUrl + '" >';
                //        $('<th class="dg_th" title="' + col.title + '">' + colControl + '</th>').appendTo('#headerCheckRow');
                //        break;
                //    default:
                //};
                
                if (this.options.viewMode == 'contributions') {
                    $('#selectCol' + i).on('click', function () {
                        setColumnContributions(this);
                    });
                };
            };

            if (this.options.viewMode == 'infodocs') {
                for (var i = 0; i < this.options.hierarchyNodes.length; i++) {
                    var obj = this.options.hierarchyNodes[i];
                    $('#objInfodoc' + obj.id).on('click', function () {
                        var objID = this.id.replace('objInfodoc', '');
                        OpenRichEditor('?type=1&field=infodoc&guid=' + objID + '&callback=objInfodoc' + objID);
                    });
                };
            };

            this.options.body = $('<tbody id="' + id + '_tbody"></tbody>', {});
            $(this.options.table).append(this.options.body);
            $("#input_Search").on("keypress", function (e) {
                if (e.keyCode == 13) {
                    var searchText = $(this).val().toLowerCase();
                    $('#' + id).datagrid('filterRows', searchText);
                    return false;
                };
            });
            $("#input_Search").on("keyup", function (e) {
                    var searchText = $(this).val().toLowerCase();
                    setTimeout('var curtext = $("#input_Search").val(); if (curtext == "' + searchText + '"){$("#'+id+'").datagrid("filterRows", curtext);};', 400);
                       // $('#' + id).datagrid('filterRows', searchText);
                    return false;
            });
            this.redrawRows();
            //$(this.options.table).floatThead();
        },
        _setOption: function (key, value) {
            this._super(key, value);
        },
        _setOptions: function (options) {
            this._super(options);
        },
        resize: function () {

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
        isNodeInfodoc: function (nodeGuid) {
            for (var i = 0; i < this.options.nodeInfodocs.length; i++) {
                var item = this.options.nodeInfodocs[i];
                if (item.nodeGuid == nodeGuid) {
                    return true;
                };
            };
            return false;
        },
        setColumnContributions: function (sender) {
            var colID = Number(sender.id.replace('selectCol', ''));
            //sendCommand('action=setcol&col_id=' + colID + '&val=' + sender.checked);
            var nodeIDs = '';
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                if (row.show == 1) {
                    $('#cbCell' + row.nodeid + 'x' + colID).prop('checked', sender.checked);
                    this._setClientContributions(row.nodeid, colID, sender.checked);
                    nodeIDs += (nodeIDs == '' ? '' : ';') + row.nodeid + ',' + colID;
                };
            };
            sendCommand('action=setrange&nodeids=' + nodeIDs + '&val=' + sender.checked);
            this.updateRowsContributions();
            this.updateAllCheckbox();
        },
        _getRowIndexByNodeID: function (nodeID) {
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                if (row.nodeid == nodeID) { return i };
            };
            return -1;
        },
        _setClientContributions: function (altID, colID, checked) {
            var i = this._getRowIndexByNodeID(altID);
            var row = this.options.rows[i].cont[colID] = (checked ? 1 : 0);
        },
        setRowContributions: function (sender) {
            var nodeID = Number(sender.id.replace('selectRow', ''));
            sendCommand('action=setrow&row_id=' + nodeID + '&val=' + sender.checked);
            for (var i = 0; i < this.options.columns.length; i++) {
                $('#cbCell' + nodeID + 'x' + i).prop('checked', sender.checked);
                this._setClientContributions(nodeID, i, sender.checked);
            };
            this.updateHeaderContributions();
            this.updateAllCheckbox();
        },
        setCellContribution: function (e, sender) {
            var cur_i = sender.getAttribute("i") * 1;
            var cur_j = sender.getAttribute("j") * 1;
            var chk = sender.checked; //this.options._last_checked_cell[2];

            if ((e.shiftKey) && e.shiftKey && this.options._last_checked_cell[0] != -1) {
                var _cur_checked_cell = [cur_i, cur_j, false];
                var i0 = Math.min(cur_i, this.options._last_checked_cell[0]);
                var j0 = Math.min(cur_j, this.options._last_checked_cell[1]);
                var i1 = Math.max(cur_i, this.options._last_checked_cell[0]);
                var j1 = Math.max(cur_j, this.options._last_checked_cell[1]);
                var nodeIDs = '';
                for (var i = i0; i <= i1; i++) {
                    for (var j = j0; j <= j1; j++) {
                        var row = this.options.rows[i];
                        var cb = document.getElementById('cbCell' + row.nodeid + 'x' + j);
                        if ((cb)) cb.checked = chk;
                        this._setClientContributions(row.nodeid, j, chk);
                        nodeIDs += (nodeIDs == '' ? '' : ';') + row.nodeid + ',' + j;
                    }
                }
                //sendCommand('action=setrange&i0=' + i0 + '&j0=' + j0 + '&i1=' + i1 + '&j1=' + j1 + '&val=' + chk);
                sendCommand('action=setrange&nodeids=' + nodeIDs + '&val=' + chk);
            } else {
                var row = this.options.rows[cur_i];
                this._setClientContributions(row.nodeid, cur_j, chk);
                sendCommand('action=setcell&row_id=' + row.nodeid + '&col_id=' + cur_j + '&val=' + chk);
            }
            this.updateHeaderContributions();
            this.updateRowsContributions();
            this.updateAllCheckbox();
            this.options._last_checked_cell = [cur_i, cur_j, sender.checked];
            // A1200 ==
        },
        setAllContributions: function (isChecked) {
            //sendCommand('action=setall&val=' + isChecked);
            $('#selectAllcb').prop('checked', isChecked);
            $('#selectAllcb').prop("indeterminate", false);
            var nodeIDs = '';
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                if (row.show == 1) {
                    $('#selectRow' + row.nodeid).prop('checked', isChecked);
                    $('#selectRow' + row.nodeid).prop("indeterminate", false);
                    for (var j = 0; j < this.options.columns.length; j++) {
                        $('#cbCell' + row.nodeid + 'x' + j).prop('checked', isChecked);
                        this._setClientContributions(row.nodeid, j, isChecked);
                        nodeIDs += (nodeIDs == '' ? '' : ';') + row.nodeid + ',' + j;
                    };
                };
            };
            sendCommand('action=setrange&nodeids=' + nodeIDs + '&val=' + isChecked);
            for (var j = 0; j < this.options.columns.length; j++) {
                $('#selectCol' + j).prop('checked', isChecked);
                $('#selectCol' + j).prop("indeterminate", false);
            };
        },
        showNextPage: function(){
            $('table#dataGridTable tr#trMoreRows').remove();
            this.options.currentPage += 1;
            this.redrawRows();
        },
        showPrevPage: function () {
            $('table#dataGridTable tr#trMoreRows').remove();
            this.options.currentPage -= 1;
            this.redrawRows();
        },
        showPage: function (pageID) {
            $('table#dataGridTable tr#trMoreRows').remove();
            this.options.currentPage = Number(pageID);
            this.redrawRows();
        },
        updateHeaderContributions: function () {
            for (var j = 0; j < this.options.columns.length; j++) {
                var allContributions = true;
                var noneContributions = true;
                for (var i = 0; i < this.options.rows.length; i++) {
                    var row = this.options.rows[i];
                    if ($('#cbCell' + row.nodeid + 'x' + j).prop('checked')) {
                        noneContributions = false;
                    } else {
                        allContributions = false;
                    };
                };
                var checkBox = $('#selectCol' + j);
                checkBox.prop("indeterminate", false);
                if (allContributions) { checkBox.prop('checked', true) };
                if (noneContributions) { checkBox.prop('checked', false) };
                if ((!allContributions) && (!noneContributions)) { checkBox.prop("indeterminate", true) };
            };
        },
        updateRowsContributions: function () {
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                var allContributions = true;
                var noneContributions = true;
                for (var j = 0; j < this.options.columns.length; j++) {
                    if ($('#cbCell' + row.nodeid + 'x' + j).prop('checked')) {
                        noneContributions = false;
                    } else {
                        allContributions = false;
                    };
                };
                var checkBox = $('#selectRow' + row.nodeid);
                checkBox.prop("indeterminate", false);
                if (allContributions) { checkBox.prop('checked', true) };
                if (noneContributions) { checkBox.prop('checked', false) };
                if ((!allContributions) && (!noneContributions)) { checkBox.prop('indeterminate', true) };
            };
        },
        updateAllCheckbox: function () {
            var allContributions = true;
            var noneContributions = true;
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                if (row.show == 1) {
                    var checkBox = $('#selectRow' + row.nodeid);
                    if (checkBox.prop('checked')) {
                        noneContributions = false;
                    } else {
                        allContributions = false;
                    };
                    if (checkBox.prop('indeterminate')) {
                        noneContributions = false;
                        allContributions = false;
                    };
                };
            };
            for (var j = 0; j < this.options.columns.length; j++) {
                var checkBox = $('#selectCol' + j);
                if (checkBox.prop('checked')) {
                    noneContributions = false;
                } else {
                    allContributions = false;
                };
                if (checkBox.prop('indeterminate')) {
                    noneContributions = false;
                    allContributions = false;
                };
            };
            var checkBox = $('#selectAllcb');
            checkBox.prop('indeterminate', false);
            if (allContributions) { checkBox.prop('checked', true) };
            if (noneContributions) { checkBox.prop('checked', false) };
            if ((!allContributions) && (!noneContributions)) { checkBox.prop('indeterminate', true) };
        },
        updateInfodoc: function (infodocID, isEmpty) {
            var imgUrl = 'icon_document.png';
            if (isEmpty == '1') {
                imgUrl = 'icon_newdocument.png';
            };
            $('#' + infodocID).prop('src', this.options.imgPath + imgUrl);
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
            var id = this.options.table.id;
            $('.checkattr').each(function () {
                var guid = $(this).attr("guid");
                var ischecked = $(this).is(':checked');
                $('#' + id).datagrid('setsearchfields', guid, ischecked);
            });
            for (var i = 0; i < this.options.rows.length; i++) {
                var row = this.options.rows[i];
                var showrow = 0;
                if (this.options.searchbyname) {
                    if (row.title.toLowerCase().indexOf(searchText) >= 0) { showrow = 1 };
                };
                for (var k = 0; k < this.options.attributes.length; k++) {
                    var attr = this.options.attributes[k];
                    if (attr.search) {                      
                        if (row.attrvals[k].toLowerCase().indexOf(searchText) == 0) { showrow = 1 };
                    };
                };
                row.show = showrow;
            };
            this.options.currentPage = 1;
            this.redrawRows();
            this.updateAllCheckbox();
        },
        deleteEvent: function (event_id) {
            sendCommand("action=delete_event&event_id=" + event_id);
            var i = 0;
            while (i < this.options.rows.length) {
                var row = this.options.rows[i];
                if (row.nodeid == event_id) {
                    this.options.rows.splice(i, 1);
                } else {
                    i++;
                }
            }
            this.redrawRows();
        },
        updateEventName: function (event_id, val) {
            if (val != "") {
                for (var i = 0; i < this.options.rows.length; i++) {
                    var row = this.options.rows[i];
                    if (row.nodeid == event_id) row.title = val;
                };                
                sendCommand("action=update_event_name&event_id=" + encodeURIComponent(event_id) + "&val=" + encodeURIComponent(val));
                this.redrawRows();
            }
        }

    });
})(jQuery);

function roleClicked(sender){
    alert($(sender).attr("id"));
}

function setColumnContributions(sender) {
    $("#dataGridTable").datagrid("setColumnContributions", sender);
}

function setRowContributions(sender) {
    $("#dataGridTable").datagrid("setRowContributions", sender);
}

function setCellContribution(event, sender) {
    $("#dataGridTable").datagrid("setCellContribution", event, sender);
}

function setAllContributions(isChecked) {
    $("#dataGridTable").datagrid("setAllContributions", isChecked);
}

function showNextPage() {
    $("#dataGridTable").datagrid("showNextPage");
}

function showPrevPage() {
    $("#dataGridTable").datagrid("showPrevPage");
}

function showPage(pageID) {
    $("#dataGridTable").datagrid("showPage", pageID);
}

//A1203 ===
var last_menu_event_id = "";

function showEventMenu(sender, event_id, event_name, imgPath, hid) {    
    is_context_menu_open = false;
    $("div.context-menu").hide().remove();
    var title = decodeURIComponent(ShortString(event_name, 30));
    var sMenu = "<div class='context-menu' style='width:84px;'><nobr>";
    //sMenu += "<a href='' class='context-menu-item' title='" + event_name + "' onclick='onContextMenuViewObjectivesClick(" + '"' + event_id + '"' + "," + '"' + event_name + '"' + "); return false;' style='text-align:left;'><div><nobr><img align='left' style='vertical-align:middle;' src='" + imgPath + "view_tree.gif' alt='' >&nbsp;View " + (hid == -1 ? "Contributions" : (hid == 0 ? "Objectives" : "Sources")) + " of&nbsp;\"" + title + "\"</nobr></div></a>";
    //sMenu += "<a href='' class='context-menu-item' title='" + event_name + "' onclick='onContextMenuRenameEventClick(" + '"' + event_id + '"' + "," + '"' + event_name + '"' + "); return false;' style='text-align:left;'><div><nobr><img align='left' style='vertical-align:middle;' src='" + imgPath + "edit_small.gif' alt='' >&nbsp;Edit&nbsp;\"" + title + "\"</nobr></div></a>";
    //sMenu += "<a href='' class='context-menu-item' title='" + event_name + "' onclick='confrimDeleteEvent(" + '"' + event_id + '"' + "," + '"' + event_name + '"' + "); return false;' style='text-align:left;'><div><nobr>&nbsp;<img align='left' style='vertical-align:middle;margin-left:3px;' src='" + imgPath + "delete_tiny.gif' alt='' >&nbsp;Delete&nbsp;\"" + title + "\"</nobr></div></a>";
    sMenu += "<a href='' class='context-menu-item' title='Delete&nbsp;\"" + title + "\"' onclick='confrimDeleteEvent(" + '"' + event_id + '"' + "," + '"' + event_name + '"' + "); return false;'><img style='vertical-align:middle;margin:3px;' src='" + imgPath + "delete_tiny.gif' alt='' ></a>&nbsp;";
    sMenu += "<a href='' class='context-menu-item' title='Edit&nbsp;\"" + title + "\"' onclick='onContextMenuRenameEventClick(" + '"' + event_id + '"' + "," + '"' + event_name + '"' + "); return false;'><img style='vertical-align:middle;' src='" + imgPath + "edit_small.gif' alt='' ></a>&nbsp;";
    sMenu += "<a href='' class='context-menu-item' title='View " + (hid == -1 ? "Contributions" : (hid == 0 ? "Objectives" : "Sources")) + " of&nbsp;\"" + title + "\"' onclick='onContextMenuViewObjectivesClick(" + '"' + event_id + '"' + "," + '"' + event_name + '"' + "); return false;'><img style='vertical-align:middle;' src='" + imgPath + "view_tree.gif' alt='' ></a><span style='color:#d0d0d0;'>&nbsp;|&nbsp;</span>";
    sMenu += "<img style='vertical-align:middle;cursor:hand;' src='" + imgPath + "menu_dots.png' alt='' onclick='is_context_menu_open = false; $('div.context-menu').hide().remove();' >";
    sMenu += "</nobr></div>";
    //var x = event.clientX;
    //var y = event.clientY + $(window).scrollTop();
    var rect = sender.getBoundingClientRect();
    var x = rect.left - 74;
    var y = rect.top - 8;
    var s = $(sMenu).appendTo("#divMain").css({ top: y + "px", left: x + "px" });
    $("div.context-menu").fadeIn(500);
    setTimeout('canCloseMenu()', 200);
}

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

function deleteEvent(event_id) {
    $("#dataGridTable").datagrid("deleteEvent", event_id);
    
}
//A1203 ==

function SortByTitleDesc(a, b) {
    return ((a.title.toLowerCase() < b.title.toLowerCase()) ? 1 : ((a.title > b.title) ? -1 : 0));
}

function SortByTitleAsc(a, b) {
    return ((a.title.toLowerCase() > b.title.toLowerCase()) ? 1 : ((a.title < b.title) ? -1 : 0));
}

function SortByIdxAsc(a, b) {
    return ((a.idx > b.idx) ? 1 : ((a.idx < b.idx) ? -1 : 0));
}