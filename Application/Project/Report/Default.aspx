<%@ Page Language="vb" CodeBehind="Default.aspx.vb" Inherits="ReportGeneratorPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="PageContent" runat="server">
<style type="text/css">
    :not(.dx-list-select-decorator-enabled).dx-list .dx-list-item.dx-list-item-selected {
        background-color: #539ddd;
        color: #ffffff;
    }
</style>
<script type="text/javascript">

    var cur_fmt = _rep_fmt_pdf;

    var reports = <% = ReportWebAPI.ReportsListJSON(App.ActiveProject.ProjectManager.Reports, ecReportCategory.Report, True) %>;
    var report_id = <% =CheckVar("report_id", -1) %>;
    if (report_id<0) report_id = getCurReportID();

    var ctrlList = null;
    var ctrlItems = null;

    function onResize(force, w, h) {
        $("#divList").height(60);
        $("#divItemsScroller").height(40);
        var hr = Math.round($("#tdList").prop("clientHeight") - $("#divList").position().top - $("#divAddNewReport").height()*1 - 48);
        var hi = Math.round($("#tdList").prop("clientHeight") - $("#divItemsScroller").position().top - 12);
        $("#divList").height(hr);
        $("#divItemsScroller").height(hi);
    }

    function isSupported(type) {
        if (typeof type == "undefined" || !(type)) return false; else return (_rep_types_allowed.indexOf(type)>=0);
    }

    function curReportID() {
        return report_id;
    }

    function curReport() {
        var id = curReportID();
        if (typeof id == "undefined" || typeof reports[id] == "undefined") return null; else return reports[id];
    }

    function curReportType() {
        var rep = curReport();
        if ((rep)) return rep.ReportType; else return "undefined";
    }

    function setCurReportID(id) {
        report_id = id;
        if ((ctrlList)) {
            var sel = ctrlList.option("selectedItemKeys");
            var el = $("#report_" + id);
            if ((el) && (el.length)) {
                var item = el.parent().parent();
                if ((sel) || sel.length!=1 || sel[0]!=id) ctrlList.unselectAll();
                ctrlList.selectItem(item);
                ctrlList.scrollToItem(item);
                $("#divList").find('[id^="_repMenu"]').hide();
                $("#_repMenu" + report_id).show();
                localStorage.setItem(rep_sess_report_id, report_id);
                initToolbar();
                showReportItems();
            }
        }

        return curReportID();
    }
    
    function initToolbar() {
        var cur_type  =curReportType();
        var supported = isSupported(cur_type);
        var has_report = supported && (curReportID() >= 0);
        $("#toolbar").dxToolbar({
            items: [{
                location: 'before',
                locateInMenu: 'auto',
                showText: "always",
                widget: 'dxButton',
                options: {
                    disabled: !has_report || true,
                    text: "New Item",
                    hint: "New Report Item",
                    icon: 'far fa-plus-square',
                    elementAttr: { id: 'btnAddItem' },
                    onClick: function () {
                        //addReport();
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButton',
                options: {
                    disabled: !has_report || (Object.keys(curReport().Items).length<1),
                    text: "Download report",
                    hint: "Generate and download Report",
                    icon: 'fa fa-download',
                    type: "",
                    elementAttr: { id: 'btnDownload' },
                    onClick: function () {
                        doDownload(_API_URL + "pm/report/?<% =_PARAM_ACTION %>=download&format=" + encodeURI(cur_fmt) + "&id=" + curReportID());
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'never',
                text: 'Format:',
                disabled: !has_report,
                visible: (cur_type == _rep_type_doc)
            }, {
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButtonGroup',
                visible: (cur_type == _rep_type_doc),
                options: {
                    items: [
                        { "text_": "DOCX", "hint":"DOCX", "icon": "far fa-file-word", "value": _rep_fmt_docx },
                        { "text_": "PDF", "hint":"PDF", "icon": "far fa-file-pdf", "value": _rep_fmt_pdf },
                        { "text_": "RTF", "hint":"RTF", "icon": "far fa-file-alt", "value": _rep_fmt_rtf }
                    ],
                    disabled: !has_report,
                    selectionMode: "single",
                    keyExpr: "value",
                    selectedItemKeys: [cur_fmt],
                    elementAttr: { id: 'btnFormats', class: 'blue_selected' },
                    onItemClick: function (e) {
                        cur_fmt = e.itemData.value;
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'never',
                text: ' ',
                visible: true
            }, {
                location: 'after',
                locateInMenu: 'auto',
                showText: "inMenu",
                widget: 'dxButton',
                options: {
                    disabled: false,
                    text: "Export Reports",
                    hint: "Export Reports List",
                    icon: 'fa fa-file-download',
                    elementAttr: { id: 'btnExportReports' },
                    onClick: function () {
                        doExport();
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'auto',
                showText: "inMenu",
                widget: 'dxButton',
                options: {
                    disabled: false,
                    text: "Refresh List",
                    hint: "Refresh List",
                    icon: 'fa fa-redo-alt',
                    elementAttr: { id: 'btnRelodReports' },
                    onClick: function () {
                        loadReports();
                    }
                }
            }]
        });
    }

    function initUIElements() {
        //$("#divListScroller").dxScrollView({
        //    scrollByContent: true,
        //    scrollByThumb: true,
        //    direction: "vertical",
        //    showScrollbar: "always",
        //});

        $("#divItemsScroller").dxScrollView({
            scrollByContent: true,
            scrollByThumb: true,
            direction: "vertical",
            showScrollbar: "always",
        });
    }

    function initReports() {
        if ((reports) && reports != null) {
            showReportsList();
        } else {
            loadReports();
        }
    }

    function loadReports(first_load) {
        showLoadingPanel();
        callAPI("pm/report/?<% =_PARAM_ACTION %>=list", {"category": "<% =ecReportCategory.Report.ToString() %>"}, function (res) {
            onLoadReports(res);
        });
    }

    function onLoadReports(res) {
        var loaded = false;
        if (isValidReply(res)) {
            var data = res.Data;
            if (typeof data != "undefined" && data != "") {
                var rep = parseReply(data);
                if ((rep)) {
                    reports = rep;
                    showReportsList();
                    loaded = true;
                }
            }
        }
        hideLoadingPanel();
        if (!loaded) showErrorMessage("Unable to load reports list", true);
    }

    function showReportsList() {

        if (report_id*1<=0) report_id = -1;
        var has_dest = false;
        var def_id = -1;
        var reportsList = [];
        for (var i=0; i<_rep_types_allowed.length; i++) {
            var t = _rep_types_allowed[i];
            var items = scanReports(reports, t);
            if ((items) && (items.length)) {
                reportsList.push({"key": _rep_types[t], "items": items});
                if (typeof items[report_id] != "undefined") {
                    has_dest = true;
                    //sel_grp = i;
                    //sel_idx = items.indexOf(items[report_id]);
                }
                if (def_id<0) def_id = items[0].ID;
            }
        }

        var has_reports = (reportsList.length>0);
        if (!has_dest) {
            if (def_id>0) report_id = def_id; else report_id = -1;
        }

        if (has_reports) {
            ctrlList = $("#divList").show().dxList({
                dataSource: reportsList,
                keyExpr: "ID",
                displayExpr: "Name",
                grouped: true,
                collapsibleGroups: true,
                selectionMode: "single",
                noDataText: "",
                //selectedItemKeys: [report_id],
                scrollByContent: true,
                scrollByThumb: true,
                useNativeScrolling: false,
                showScrollbar: "always",
                allowItemDeleting: true,
                itemDeleteMode: "swipe",
                groupTemplate: function(data) {
                    return $("<div style='color: #333399; padding:2px 0px;'><i class='" + _rep_icons[data.key] + "'></i> " + _rep_names[data.key] + "</div>");
                },
                itemTemplate: function (data) {
                    return $("<div style='text-align:center;' title='" + htmlEscape(data.Name) + " [" + _rep_name[data.ReportType] + ", #" + data.ID + "]' id='report_" + data.ID + "' onmouseover='if (report_id!="+ data.ID +") $(\"#_repMenu" + data.ID + "\").show();'' onmouseout='if (report_id!="+ data.ID +") $(\"#_repMenu" + data.ID + "\").hide();'><div style='width:42px; position:absolute; right:0; display:none;' id='_repMenu" + data.ID + "'><div id='repMenu" + data.ID + "' data-report='" + data.ID + "'></div></div><i class='" + _rep_icons[data.ReportType] + " fa-2x' style='margin-top:12px;'></i><div style='margin-bottom:6px;' class='text-overflow'>" + htmlEscape(data.Name) + "</div></div>");
                },
                onItemClick: function (e) {
                    setCurReportID(e.itemData.ID);
                },
                //onSelectionChanged: function (e) {
                //    $('[id^="_repMenu"]').hide();
                //    $("#_repMenu" + report_id).show();
                //    //for (var i=0; i<e.addedItems.length; i++) {
                //    //    $("#_repMenu" + e.addedItems[i].ID).show();
                //    //}
                //    //for (var i=0; i<e.removedItems.length; i++) {
                //    //    $("#_repMenu" + e.removedItems[i].ID).hide();
                //    //}
                //},
                onItemDeleting: function(e) {
                    var d = $.Deferred();
                    DevExpress.ui.dialog.confirm("Do you really want to delete this Report?")
                        .done(function(value) { 
                            d.resolve(!value);
                        })
                        .fail(d.reject);
                    e.cancel = d.promise();
                },
                onItemDeleted: function (e) {
                    doDeleteReport(e.itemData.ID);
                },
                onItemContextMenu: function (e) {
                    if (e.itemData) {
                        var id = e.itemData.ID;
                        event.preventDefault();
                        setCurReportID(id);
                        $("#repMenu" + id).dxDropDownButton("instance").toggle(true);
                    }
                },
                onContentReady: function (e) {
                    for (var j=0; j<reportsList.length; j++) {
                        for (var i=0; i<reportsList[j].items.length; i++) {
                            var b = $("#repMenu" + reportsList[j].items[i].ID);
                            if ((b) && (b.length)) {
                                b.dxDropDownButton({
                                    icon: "overflow",
                                    hint: "Manage report",
                                    showArrowIcon: false,
                                    stylingMode: "text",
                                    displayExpr: "text",
                                    keyExpr: "value",
                                    items: [
                                        { "text": "Edit", "hint": "Edit Report Properties", "icon": "fa fa-pencil-alt", "value": "edit" },
                                        { "text": "Clone", "hint": "Clone Report", "icon": "far fa-copy", "value": "clone" },
                                        { "text": "Delete", "hint": "Delete Report", "icon": "far fa-trash-alt", "value": "delete" }
                                    ],
                                    dropDownOptions: {
                                        width: 80
                                    },
                                    onItemClick: function(e) {
                                        var id = e.element.data("report")*1;
                                        if (id<=0) id = report_id;
                                        switch (e.itemData.value) {
                                            case "edit":
                                                editReport(id);
                                                break;
                                            case "clone":
                                                cloneReport(id);
                                                break;
                                            case "delete":
                                                deleteReport(id);
                                                break;
                                        }  
                                    }
                                });
                            }
                        }
                    }
                }
            }).dxList("instance");
        } else {
            $("#divList").hide();
        }

        setCurReportID(report_id);
    }

    function showReportItems() {
        var has_items = false;
        var rep = curReport();
        if (typeof rep != "undefined" && (rep)) {
            $("#divReportTitle").html("<h5>" + _rep_name[curReportType()] + " &quot;" + htmlEscape(rep.Name) + "&quot;</h5>" + (rep.Comment != "" ? "<div style='padding:6px 4px; margin:0px 1ex 1em 1ex; border-radius:4px; background: #f5faff; text-align:left; max-width:600px'><b>Description</b>: " + htmlEscape(rep.Comment) + "</div>" : ""));
            var items = [];
            for (let id in rep.Items) {
                items.push(rep.Items[id]);
            }
            if (items.length) {
                has_items = true;
                //if ($("#divItems").data("dxList")) $("#divItems").dxList("dispose");
                $("#divItems").html("");
                //$("#divItemsScroller").show();
                ctrlItems = $("#divItems").dxList({
                    items: items,
                    repaintChangesOnly: true,
                    keyExpr: "ID",
                    displayExpr: "Name",
                    itemDragging: {
                        allowReordering: true,
                        dragDirection: "vertical",
                        onReorder: function (e) {

                        },
                        onDragStart: function (e) {
                            //e.itemData = tasks[e.fromIndex];
                        },
                        onAdd: function (e) {
                        },
                        onRemove: function (e) {
                        }
                    }
                }).dxList("instance");
            }
        } else {
            //$("#divItemsScroller").hide();
        }
        if (!has_items) $("#divItems").html("<div class='gray' style='margin:3em; text-align:center'>No report items</div>");
        setTimeout(onResize(), 3500);
    }

    function doExport() {
        doDownload(_API_URL + "pm/report/?<% =_PARAM_ACTION %>=export");
    }

    function doAddReport(name, comment) {
        callAPI("pm/report/?<% = _PARAM_ACTION %>=add_report", {"type": _rep_type_doc, "name": name, "comment": comment }, onAddReport);
    }

    function doCloneReport(name, comment, src_id) {
        callAPI("pm/report/?<% = _PARAM_ACTION %>=clone_report", {"id": src_id, "name": name, "comment": comment }, onAddReport);
    }

    function doEditReport(name, comment, id) {
        callAPI("pm/report/?<% = _PARAM_ACTION %>=edit_report", {"id": id, "name": name, "comment": comment }, onAddReport);
    }

    function doDeleteReport(id) {
        callAPI("pm/report/?<% = _PARAM_ACTION %>=delete_report", {"id": id}, onDeleteReport);
    }

    function onAddReport(res) {
        if (isValidReply(res)) {
            if ((reports) && res.ObjectID>0 && typeof res.Data != "undefined") {
                var id = res.ObjectID;
                try {
                    var rep = parseReply(res.Data);
                    if ((rep)) {
                        reports[id] = rep;
                        showReportsList();
                        setCurReportID(id);
                    }
                }
                catch (e) {
                    loadReports(false);
                }
            }
        } else {
            showResMessage(res, true);
        }
    }

    function onDeleteReport(res) {
        if (isValidReply(res)) {
            if ((reports) && res.ObjectID>0) {
                delete reports[res.ObjectID];
                setCurReportID(-1);
                showReportsList();
            }
        } else {
            showResMessage(res, true);
        }
    }

    function addReport() {
        showReportProperties(-1, "", "", "Add Report", doAddReport);
    }

    function editReport(id) {
        var rep = (reports) && reports[id];
        if ((rep)) {
            showReportProperties(id, rep.Name, rep.Comment, "Edit Report", doEditReport);
        }
    }

    function cloneReport(id) {
        var rep = (reports) && reports[id];
        if ((rep)) {
            showReportProperties(id, rep.Name, rep.Comment, "Clone Report", doCloneReport);
        }
    }

    function deleteReport(id) {
        var rep = (reports) && reports[id];
        if ((rep)) {
            var result = DevExpress.ui.dialog.confirm("Do you want to delete report '" + rep.Name + "'?", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
            result.done(function (dialogResult) {
                if (dialogResult) {
                    doDeleteReport(id);
                }
            });
        }
    }

    function showReportProperties(id, name, comment, title, onSave) {
        var report_options = {"name": name, "comment": comment};

        $("#popupContainer").dxPopup({
            width: dlgMaxWidth(500),
            height: "auto",
            title: title,
            //animation: {
            //    show: { "duration": 0 },
            //    hide: { "duration": 0 }
            //},
            toolbarItems: [{
                widget: 'dxButton', 
                options: { 
                    text: "<% = JS_SafeString(ResString("btnSave")) %>",
                    elementAttr: { "class": "button_enter" },
                    onClick: function() {
                        var form = $("#report_frm").dxForm("instance");
                        var name = report_options.name;
                        if (name == "") {
                            DevExpress.ui.notify("<% =JS_SafeString(ResString("msgEmptyTitle")) %>", "error"); 
                            setTimeout(function () {
                                form.getEditor("name").focus();
                            }, 500);
                        } else {
                            if (typeof onSave == "function") {
                                onSave(report_options.name, report_options.comment, id);
                                closePopup();
                            }
                        }
                        return false; 
                    } 
                },
                toolbar: "bottom",
                location: 'after'
            }, {
                widget: 'dxButton', 
                options: { 
                    text: "<% = JS_SafeString(ResString("btnCancel")) %>",
                    elementAttr: { "class": "button_esc" },
                    onClick: function() { closePopup(); return false; } 
                },
                toolbar: "bottom",
                location: 'after'
            }],
            contentTemplate: function () {
                return $("<div id='report_frm'></div>");
            }
        });
        $("#popupContainer").dxPopup("show");

        var frm = $("#report_frm").dxForm({
            formData: report_options,
            items: [{
                dataField: "name",
                editorType: "dxTextBox",
                label: { text: "<% =JS_SafeString(ResString("lblTitle")) %>" }
            }, {
                dataField: "comment",
                editorType: "dxTextArea",
                label: { text: "<% =JS_SafeString(ResString("tblComment")) %>" },
                editorOptions: {}
            }],
            readOnly: false,
            showColonAfterLabel: true,
            labelLocation: "left"
        }).dxForm("instance");
        onFormNarrow("#report_frm", 600);

        setTimeout(function () {
            frm.getEditor("name").focus();
        }, 550);
    }

    resize_custom = onResize;

    $(document).ready(function () {
        initUIElements();
        initReports();
    });

</script>
<table border="0" cellpadding="0" cellspacing="0" class="whole">
    <tr style="height:24px;">
        <td colspan="2">
            <div id="toolbar" class="dxToolbar"></div></td>
    </tr>
    <tr valign="top">
        <td id="tdList" style="width:200px; border-right:1px solid #e0e0e0; padding-bottom:8px;"><div id="divList" style="padding-bottom:6px;"></div><div style="position:absolute; z-index:1000; height:16px; width:200px; margin-top:-20px; background: url(<% =ThemePath + _FILE_IMAGES %>shade_bottom.png) repeat-x;"></div><a href="" onclick="addReport(); return false"><div id="divAddNewReport" style="margin-right:1ex; border:2px dashed #e0e0e0; border-radius:4px; padding:10px 1ex 8px 1ex; text-align:center;"><span class="fa-stack" style="font-size:1.1rem;"><i class="fas fa-plus fa-stack-1x"></i><i class="far fa-circle fa-stack-2x"></i></span><div style="margin-top:1ex; font-weight:bold;"><% =ResString("lblAddReport") %></div></div></a></td>
        <td align="center" style="padding:0px 1ex 0px 1em;" id="tdItems"><div id="divReportTitle" style="text-align:center"></div><div id="divItemsScroller" style="border:1px solid #d0d0d0; max-width:600px; min-width:200px; margin:0px auto;"><div style="text-align:left; padding:1ex;"><div id="divItems"></div></div></div></td>
    </tr>
</table>
</asp:Content>
