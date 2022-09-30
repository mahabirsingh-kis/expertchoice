/* Service function for work with dataTables;  ver 171019 // AD, DA */


function dataTablesHighlighSearch(table) {
    table.on('draw', function () {
        if ((table) && table.rows({ filter: 'applied' }).data().length) {
            var body = $(table.table().body());
            body.unhighlight();
            body.highlight(table.search());
        }
    });
}

function dataTablesGrouping(table, tableID, columnIdx) {
    $("#" + tableID + " tbody").on("click", "tr.group", function () {
        var currentOrder = table.order()[0];
        if ((currentOrder) && currentOrder[0] === columnIdx && currentOrder[1] === 'asc') {
            table.order([columnIdx, 'desc']).draw();
        }
        else {
            table.order([columnIdx, 'asc']).draw();
        }
    });
}

function dataTablesDetailRow(tableID, funcDetails, customStyle) {
    $('#' + tableID + ' tbody').on('click', 'td.' + ((customStyle === "undefined") ? 'details-control' : customStyle), function () {
        var tr = $(this).closest('tr');
        var row = table.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        }
        else {
            // Open this row
            row.child(funcDetails.apply(row, [row.data()])).show();
            tr.addClass('shown');
        }
    });

}

function dataTablesResize(tableID, parentID, offset) {
    var tdMain = $("#" + parentID);
    var tbl_c = $("#" + tableID).parent();  // class='dataTables_scrollBody'

    if ((tdMain) && (tbl_c)) {

        var tbl_h = tbl_c.prev(); 	            // class='dataTables_scrollHead'
        var tbl_f = tbl_h.children(); 	        // class='dataTables_scrollHeadInner'

        if ((tbl_h) && (tbl_f)) {

            var hs = $("#" + tableID + "_filter").height(); // when filter is displayed
            var hh = tbl_h.height();
            var hi = $("#" + tableID + "_info").height();	// when info is displayed

            tbl_c.css("max-height", 10);
            tbl_f.width(10);

            if (!(hs) || hs < 1) hs = 1;
            if (!(hh) || hh < 1) hh = 1;
            if (!(hi) || hi < 1) hi = 1;

            var h = tdMain.innerHeight();
            h = h - hs - hh - hi;
            var thh = h - ((typeof offset == "undefined") ? 0 : offset * 1);
            tbl_c.css("max-height", thh);
            tbl_c.css("height", thh);
            tbl_f.width(tdMain.outerWidth() - 32);
        }
    }
    // auto-size column headers
    $('#' + tableID).DataTable().draw();
}

function dataTablesDestroy(table, tbl_id) { // A1121
    if ((table) && (table != null) && ((typeof table.destroy) != 'undefined')) {
        // need to clear custom buttons, otherwise table.destroy() method throws an error
        if (typeof table.buttons != 'undefined' && table.buttons().length > 0) {
            var buttons = [];
            $.each(table.buttons()[0].inst.s.buttons,
                function () {
                    buttons.push(this);
                });
            $.each(buttons,
                function () {
                    table.buttons()[0].inst.remove(this.node);
                });
        }

        // clear and destroy
        table.destroy();
        table.clear();
        table = null;
        if (typeof tbl_id != 'undefined') { $("#" + tbl_id).empty(); };
    }
}

function dataTablesColumnVisibility(table_id, column_index, is_visibile) {
    var dt = $('#' + table_id).DataTable();
    dt.column(column_index).visible(is_visibile);
}

function dataTablesRedrawRow(datatable, rowid, newdata) {
    //var rowdata = table.row(rowid).data();
    datatable.row(rowid).data(newdata).invalidate();
}

function dataTablesRedrawRows(datatable, data) {
    datatable.clear().draw();
    datatable.rows.add(data);
    datatable.columns.adjust().draw(); // draw the table and adjust column sizes
}

function dataTablesClearSearch(datatable) {
    datatable.search('').columns().search('').draw();
}

function dataTablesInit(datatable) {
    $("div.dataTables_filter").addClass("text").find("input").addClass("input").prop({ "autocomplete": "off", "autocorrect": "off", "autocapitalize": "off", "spellcheck": false });
    $("div.dataTables_paginate").addClass("text").css({ "margin-right": "30px", "margin-top": "-3px" });
    //datatable.fnAdjustColumnSizing();
}

function dataTablesRefreshHeaders(table_id) {
    // trick to auto-size column headers
    setTimeout("$('#" + table_id + "').DataTable().draw();", 200);
}

function dataTablesDisableAlternateRowColor(datatable) {
    //$(datatable).find("tr.even").css("background-color", "#ffffff");
    //$(datatable).find("tr.odd").css("background-color", "#ffffff");
    //$(datatable).find("td.sorting_1").css("background-color", "#ffffff");
    $("tr.even").css("background-color", "#ffffff");
    $("tr.odd").css("background-color", "#ffffff");
    $("td.sorting_1").css("background-color", "#ffffff");
}

var th_event = null;
function dataTablesPreventSort(col_idx) {
    var th = $("th:eq(" + col_idx + ")");
    if ((th) && (th.length)) {
        $.each($._data(th.get(0), "events"), function (i, event) {
            if (i == "click") {
                $.each(event, function (j, h) {
                    th_event = h.handler;
                });
            }
        });
        th.unbind("click.DT");
        setTimeout("dataTablesRestoreEvent(" + col_idx + ");", 50);
    }
}

function dataTablesRestoreEvent(col_idx) {
    if ((th_event != null)) $("th:eq(" + col_idx + ")").bind("click.DT", th_event);
    th_event = null;
}