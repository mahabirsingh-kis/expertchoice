<%@ Page Language="VB" Inherits="CombinedReportPage" title="Combined Report" Codebehind="CombinedReport.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    var options_xls = [
        { "name": "IncludeModelDescription", "title": "Model Description", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeModelDescription) %>, "disabled": false},
        { "name": "IncludeObjectives", "title": "Objectives", "type": "bool","value": <% = Bool2JS(ReportOptions.IncludeObjectives) %>, "disabled": false},
        { "name": "IncludeObjectivesDescriptions", "title": "Objectives Descriptions", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeObjectivesDescriptions) %>, "disabled": false},
        { "name": "IncludeAlternatives", "title": "Alternatives", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeAlternatives) %>, "disabled": false},
        { "name": "IncludeAlternativesDescriptions", "title": "Alternatives Descriptions", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeAlternativesDescriptions) %>, "disabled": false},
        { "name": "IncludeContributions", "title": "Contributions", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeContributions) %>, "disabled": false}, 
        { "name": "IncludeParticipants", "title": "Participants List", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeParticipants) %>, "disabled": false},
        { "name": "IncludeMeasurementMethods", "title": "Measurement Methods", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeMeasurementMethods) %>, "disabled": false},
        { "name": "IncludeSurveyResults", "title": "Survey Results", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeSurveyResults) %>, "disabled": false},
        { "name": "IncludeInconsistency", "title": "Inconsistency", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeInconsistency) %>, "disabled": false},
        { "name": "IncludeConsensus", "title": "Consensus View", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeConsensus) %>, "disabled": false},
        <% If App.isRAAvailable Then %>{ "name": "IncludeResourceAligner", "title": "Resource Aligner", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeResourceAligner) %>, "disabled": false},<% End If %>
    ];

    var formOptions = null;
    var divOptions = "#divReportOptions";
    var options_doc = [
        { "name": "ReportTitle", "title": "Report Title", "type": "string", "value": "<% = JS_SafeString(App.ActiveProject.ProjectName) %>", "disabled": false, "onChange": null, "validate": checkEmpty},
        { "type": "space" },
        //{ "name": "ReportFontName", "title": "Base Font Name", "type": "string", "value": "<% = JS_SafeString(ReportOptions.ReportFontName) %>", "disabled": false, "onChange": null, "validate": checkEmpty},
        { "name": "ReportFontName", "title": "Base Font Name", "type": "list", "value": "<% = JS_SafeString(ReportOptions.ReportFontName) %>", "values": ["Arial", "Courier", "Sans Serif", "Tahoma", "Times", "Verdana"], "disabled": false},
        { "name": "ReportFontSize", "title": "Base Font Size", "type": "int", "value": <% = JS_SafeNumber(ReportOptions.ReportFontSize) %>, "disabled": false, "onChange": null},
        { "name": "ReportTitleFontSize", "title": "Title Font Size", "type": "int", "value": <% = JS_SafeNumber(ReportOptions.ReportTitleFontSize) %>, "disabled": false, "onChange": null},
        { "type": "space" },
       // { "name": "ReportIncludeParts", "title": "Include Parts", "type": "list", "value": "<% = JS_SafeString(ReportOptions.ReportIncludePartName) %>", "values": ["Model Description", "Hierarchy", "Goal Description", "Nodes Descriptions", "Data Grid", "Objectives", "Alternatives", "Survey Results", "Inconsistency",  "Overall Results", "Objjectives/Alternatives Priorities",  "Judgments Of Objectives","Judgments Of Alternatives", "Objectives and Alternatives", "Contributions", "Judgments Overview","Objectives Priorities", "Alternatives Priorities", "Evaluation Progress"], "disabled": false},
       //mon1=== Include in report:
        { "name": "IncludeInReport", "title": "Include in Report", "type": "string", "disabled": false},
        { "name": "IncludeModelDescription", "title": "Model Description", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeModelDescription) %>, "disabled": false},
        { "name": "IncludeHierarchyImage", "title": "Hierarchy (image)", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeHierarchyImage) %>, "disabled": false},
        { "name": "IncludeHierarchyEditable", "title": "Hierarchy (editable)", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeHierarchyEditable) %>, "disabled": false},
        { "name": "IncludeGoalDescription", "title": "Goal Description", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeGoalDescription) %>, "disabled": false},
        { "name": "IncludeObjectivesDescriptions", "title": "Objectives Descriptions", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeObjectivesDescriptions) %>, "disabled": false},
        { "name": "IncludeAlternativesDescriptions", "title": "Alternatives Descriptions", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeAlternativesDescriptions) %>, "disabled": false},
        { "name": "IncludeDataGrid", "title": "Data Grid", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeDataGrid) %>, "disabled": false},
        { "name": "IncludeObjectives", "title": "Objectives", "type": "bool","value": <% = Bool2JS(ReportOptions.IncludeObjectives) %>, "disabled": false},
        { "name": "IncludeAlternatives", "title": "Alternatives", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeAlternatives) %>, "disabled": false},
        { "name": "IncludeSurveyResults", "title": "Survey Results", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeSurveyResults) %>, "disabled": false},
        { "name": "IncludeInconsistency", "title": "Inconsistency", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeInconsistency) %>, "disabled": false},
        { "name": "IncludeOverallResults", "title": "Overall Results", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeOverallResults) %>, "disabled": false},
        { "name": "IncludeObjectivesAlternativesPriorities", "title": "Objectives/Alternatives Priorities", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeObjectivesAlternativesPriorities) %>, "disabled": false},
        { "name": "IncludeJudgmentsOfObjectives", "title": "Judgments Of Objectives", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeJudgmentsOfObjectives) %>, "disabled": false},
        { "name": "IncludeJudgmentsOfAlternatives", "title": "Judgments Of Alternatives", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeJudgmentsOfAlternatives) %>, "disabled": false}, 
        { "name": "IncludeObjectivesAndAlternatives", "title": "Objectives and Alternatives", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeObjectivesAndAlternatives) %>, "disabled": false},
        { "name": "IncludeContributions", "title": "Contributions", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeContributions) %>, "disabled": false}, 
        { "name": "IncludeJudgmentsOverview", "title": "Judgments Overview", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeJudgmentsOverview) %>, "disabled": false}, 
        { "name": "IncludeObjectivesPriorities", "title": "Objectives Priorities", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeObjectivesPriorities) %>, "disabled": false}, 
        { "name": "IncludeAlternativesPriorities", "title": "Alternatives Priorities", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeAlternativesPriorities) %>, "disabled": false}, 
        { "name": "IncludeEvaluationProgress", "title": "EvaluationProgress", "type": "bool", "value": <% = Bool2JS(ReportOptions.IncludeOverallResults) %>, "disabled": false}, //mon1==
];

    function download(fmt, extra) {
        doDownload(_API_URL + "pm/dashboard/?<% =_PARAM_ACTION %>=download_" + fmt + (typeof extra != "undefined" && extra != "" ? "&" + extra : ""));
        //setTimeout(function () { showLoadingPanel("Prepare document..."); }, 300);
    }

    function downloadWithOptions(fmt, options) {
        var p = "";
        if ((options)) p = getOptionValues(options, true);
        download(fmt, p);
        return true;
    }

    function checkEmpty(val, option) {
        if (typeof val != "undefined" && val != "") {
            return ""
        } else {
            return ((typeof option != "undefined" && typeof option.title !="") ? "'" + option.title + "' " : "") + "value is empty!";
        }
    }

    function getOption(option) {
        if ((option) && typeof option.type != "undefined" && (option.type == "space" || typeof option.name != "undefined")) {
            var title= (typeof option.title != "undefined" && option.title != "" ? option.title : option.name);

            switch (option.type) {

                case "space":
                    return { itemType: "empty" }

                case "string":
                    return {
                        dataField: option.name,
                        showClearButton: true,
                        width: "100%",
                        label: {
                            text: title + ":"
                        },
                        editorOptions: {
                            width: "100%",
                            disabled: (typeof option.disabled != "undefined" && (option.disabled)),
                            elementAttr: {"id": "opt_" + option.name}
                        }
                    }

                case "int":
                    return {
                        dataField: option.name,
                        showClearButton: true,
                        label: {
                            text: title + ":"
                        },
                        editorOptions: {
                            width: "100px",
                            disabled: (typeof option.disabled != "undefined" && (option.disabled)),
                            elementAttr: {"id": "opt_" + option.name}
                        }
                    }

                case "bool":
                    return {
                        dataField: option.name,
                        label: {
                            text: " ",
                            visible: true,
                        },
                        editorOptions: {
                            text: title,
                            disabled: (typeof option.disabled != "undefined" && (option.disabled)),
                            elementAttr: {"id": "opt_" + option.name}
                        }
                    }

                case "list":
                    return {
                        dataField: option.name,
                        editorType: "dxSelectBox",
                        label: {
                            text: title + ":"
                        },
                        editorOptions: { 
                            items: option.values,
                            value: option.value,
                            disabled: (typeof option.disabled != "undefined" && (option.disabled)),
                            width: "100%",
                            elementAttr: {"id": "opt_" + option.name}
                        },
                    }
            }
            
        }

        return null;
    }

    function getOptionsList(options, container) {
        if ((options) && (container)) {
            var form_items = [];
            var options_list = {};
            for (var i=0; i<options.length; i++) {
                var option = options[i];
                var elem = getOption(option);
                if ((elem)) {
                    form_items.push(elem);
                    if (typeof option.type != "undefined" && option.type != "space" && typeof option.name != "undefined") options_list["" + option.name] = option.value;
                }
            }

            if (form_items.length) {
                formOptions = container.dxForm({
                    formData: options_list,
                    validationGroup: "userData",
                    items: form_items,
                    //onEditorEnterKey: function(e) {
                    //},
                    readOnly: false,
                    showColonAfterLabel: false,
                    showValidationSummary: false,
                    labelLocation: "left",
                    onFieldDataChanged: function(e) {
                        for (var i=0; i<options_xls.length; i++) {
                            if (options[i].name == e.dataField) { 
                                options[i].value = e.value;
                                break;
                            }
                        }
                        if (!is_manual) checkSelectAll($("#divSelAllOptions"), options_xls, $("#btnDownload"));
                    }
                }).dxForm("instance");

                return true;
            } else {
                container.html("<p align=center>No options</p>");
            }
        }
        return false;
    }

    function getOptionInput(option) {
        if ((option)  && typeof option.name != "undefined") {
            var elem = $("#opt_" + option.name);
            if ((elem) && (elem.length==1)) {
                switch (option.type) {
                    case "string":
                        return elem.dxTextBox("instance");
                    case "int":
                        return elem.dxNumberBox("instance");
                    case "bool":
                        return elem.dxCheckBox("instance");
                    case "list":
                        return elem.dxSelectBox("instance");
                }
            }
        }
        return null;
    }

    function getOptionValue(option) {
        if ((option)  && typeof option.name != "undefined") {
            var inp = getOptionInput(option);
            if (inp != null) {
                switch (option.type) {
                    case "string":
                    case "int":
                    case "bool":
                    case "list":
                        return inp.option("value");
                }
            }
        }
        return null;
    }

    function validateOptions(options, silent) {
        if ((options)) {
            for (var i=0; i<options.length; i++) {
                var option = options[i];
                if ((typeof option.disabled == "undefined" || !(option.disabled)) && typeof option.validate == "function") {
                    var val = getOptionValue(option);
                    if (val!=null) {
                        var res = option.validate(val, option);
                        if (res != "" && res !== true) {
                            if (typeof silent == "undefined" || !(silent)) {
                                var a = DevExpress.ui.dialog.alert("<div style='max-width:700px;'>" + res + "</div>", resString("titleError"));
                                a.done(function () {
                                    var inp = getOptionInput(option);
                                    if (inp!=null) setTimeout(function () { inp.focus(); }, 300);
                                    return true;
                                });
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    function getOptionValues(options, do_update) {
        if ((options)) {
            var vals = "";
            for (var i=0; i<options.length; i++) {
                var option = options[i];
                var val = getOptionValue(option);
                if (val != null && (typeof option.disabled == "undefined" || !(option.disabled))) {
                    vals += (vals == "" ? "" : "&") + encodeURI(option.name) + "=" + encodeURI(val);
                    if ((do_update)) options[i].value = val;
                }
            }
            return vals;
        }
        return "";
    }

    function showOptionsDlg(title, options, on_export) {
        var maxH = dlgMaxHeight(650);
        var container = $("<div id='divReportOptions' style='padding-right:1ex; max-height:" + (maxH - 70) + "px; overflow-y: scroll;'></div>"); 
        if ((options) && (container) && (container.length>0)) {
            var has_options = getOptionsList(options, container);
            $("#popupContainer").dxPopup({
                minWidth: 320,
                width: dlgMaxWidth(450),
                maxWidth: dlgMaxWidth(650),
                minHeight: 200,
                //height: "auto",
                //maxHeight: maxH,
                contentTemplate: function () {
                    return container;
                },
                showTitle: true,
                title: title,
                dragEnabled: true,
                shading: true,
                disabled: (typeof on_export != "function"),
                closeOnOutsideClick: false,
                resizeEnabled: true,
                showCloseButton: false,
                toolbarItems: [{
                    widget: 'dxButton',
                    disabled: !has_options,
                    options: {
                        text: resString("btnExport"),
                        type: 'default',
                        elementAttr: {"id": "btnExport", "class": "button_enter"},
                        onClick: function (e) {
                            if ((options) && validateOptions(options, false)) {
                                on_export(options);
                                setTimeout( function () { closePopup(); }, 300);
                            }
                        }
                    },
                    toolbar: "bottom",
                    location: 'after'
                }, {
                    widget: 'dxButton',
                    options: {
                        text: resString("btnCancel"),
                        onClick: function (e) {
                            closePopup();
                        }
                    },
                    toolbar: "bottom",
                    location: 'after'
                }]
            });
            $("#popupContainer").dxPopup("show");
            $("#btnExport").dxButton("focus");
        }
    }

    <%--    function onUploadSuccess(fn, res, fmt) {
        //$("#frmGenerator").hide();
        _loadPanelPersist = false;
        var extra = "";
        if (typeof res != "undefined" && res.URL != "") {
            $("#imgPreview").show().prop("src", res.URL).width(128);
            extra = "&img=" + encodeURI(res.URL);
        }
        hideLoadingPanel();
        //doDownload(_API_URL + "pm/report/?<% =_PARAM_ACTION %>=download&format=" + encodeURI(fmt) + "&img=" + encodeURI(res.URL));
        CreatePopup(_API_URL + "pm/report/?<% =_PARAM_ACTION %>=download&format=" + encodeURI(fmt) + "&img=" + encodeURI(res.URL), "report", ",");
    }--%>

    var is_manual = false;
    function checkSelectAll(ctrl, options, btn) {
        if ((options) && (ctrl) && (ctrl.data("dxCheckBox"))) {
            var sel = 0;
            var all = 0;
            for (var i=0; i<options.length; i++) {
                var option = options[i];
                if (option.type == "bool") {
                    all++;
                    if ((option.value)) sel++;
                }
            }
            is_manual = true;
            ctrl.dxCheckBox("instance").option("value", sel == 0 ? false : (sel == all ? true : undefined));
            is_manual = false;
            if ((btn) && (btn.data("dxButton"))) {
                btn.dxButton("instance").option("disabled", (sel==0));
            }
        }
    }

    $(document).ready(function () {

        var container = $("#divPagesList"); 
        if ((options_xls) && (container) && (container.length>0)) {
            var has_options = getOptionsList(options_xls, container);
        }

        $("#divSelAllOptions").dxCheckBox({
            value: false,
            onValueChanged: function(e) {
                if (e.value != "undefined" && !is_manual) {
                    var form = container.dxForm("instance");
                    var data = form.option("formData");
                    for (let par in data) {
                        form.getEditor(par).option("value", e.value);
                    }
                }
            }
        });

        $("#btnDownload").dxButton({
            "text": "Download report (.xlsx)",
            "icon": "fas fa-file-download",
            "type": "default",
            "onClick": function (e) {
                if ((options_xls) && validateOptions(options_xls, false)) {
                    return downloadWithOptions("gembox_xlsx", options_xls);
                }
                //download("gembox_xlsx");
            }
        });

        checkSelectAll($("#divSelAllOptions"), options_xls, $("#btnDownload"));

<%--       $("#btnDownloadDOCX").dxButton({
            "text": "Word...",
            "width": "160",
            "icon": "far fa-file-word",
            "onClick": function (e) {
                //download("gembox_docx");
                showOptionsDlg("DOC export options", options_doc, function (options) {
                    return downloadWithOptions("gembox_docx", options);
                });
            }
        });

        $("#btnDownloadXLSX").dxButton({
            "text": "Excel",
            "width": "160",
            "icon": "far fa-file-excel",
            "type": "default",
            "onClick": function (e) {
                download("gembox_xlsx");
            }
        });

        $("#btnDownloadPPTX").dxButton({
            "text": "PowerPoint",
            "width": "160",
            "icon": "far fa-file-powerpoint",
            "onClick": function (e) {
                download("gembox_pptx");
            }
        });

        $("#btnDownloadPDF").dxButton({
            "text": "PDF",
            "width": "160",
            "icon": "far fa-file-pdf",
            "onClick": function (e) {
                download("gembox_pdf");
            }
        });

        $("#btnUpload").dxButton({
            "text": "Test inline upload",
            "width": "160",
            "icon": "upload",
            "onClick": function (e) {
                uploadBinaryData(str2Blob("Just a test for upload a dynamic file from client to server.", "text/plain"), "text.txt", "text/plain", function (res) {
                    if (res.Result == _res_Success) {
                        if (res.URL!="" && res.URL[0]=="/") CreatePopup(res.URL, "upload", ",", true); else DevExpress.ui.dialog.alert("Uploaded " + res.Data + " bytes to temp file " + res.URL, "Info");
                    } else {
                        showResMessage(res, true);
                    }
                }, false, "docmedia");
            }
        });

        $("#btnGenerate").dxButton({
            "text": "Generate Report",
            "width": 160,
            "height": 24,
            "icon": "image",
            "type": "default",
            "onClick": function (e) {
                var frm = document.getElementById("frmGenerator");
                frm_div = $("#frmGenerator");
                if ((frm)) {
                    //frm_div.show();
                    showLoadingPanel("Create image...", true);
                    var fmt = $("#selFormat").dxSelectBox("option", "value");
                    frm.src = "<% = JS_SafeString(PageURL(_PGID_EXPORT_CHARTS_ALTS, _PARAM_ACTION + "=upload&filename=alts_chart.png&temptheme=sl&format=")) %>" + fmt;
                    $("#imgPreview").show();
                }
            }
        });

        $("#selFormat").dxSelectBox({
            items: ["pdf", "docx", "rtf"],
            value: "pdf",
            width: 100,
            height: 24,
            showClearButton: false
        });--%>

    });

</script>

<h4 style="margin:1em"><% =PageTitle(CurrentPageID) %></h4>

<div style="display:inline-block; margin:0px auto; padding:1ex 1em; border-radius:6px; background:#fafafa; border:1px solid #f0f0f0;">
    <table border="0" cellpadding="4" cellspacing="1">
        <tr><td align="left"><div style="padding:1ex"><b>Select Reports to include:</b></div><div style="padding:4px; margin-left:8px; border-bottom:1px solid #f0f0f0"><div id="divSelAllOptions"></div>&nbsp;Select/Deselect All</div><div id="divPagesList" style="white-space:nowrap; min-width:200px"></div></td></tr>
        <tr><td align="center"><div id="btnDownload" style="margin:1em 0px; padding:3px 8px"></div></td></tr>
    </table>
</div>

<%--<center>
    <h5 style="margin-top:3em; margin-bottom:1em">Download report:</h5>
    <div id="btnDownloadDOCX"></div><br />
    <div id="btnDownloadXLSX" style="margin:1em 0px"></div><br />
    <div id="btnDownloadPPTX"></div><br />
    <div id="btnDownloadPDF" style="margin:1em 0px"></div>
</center>--%>

</asp:Content>