<%@ Page Language="vb" CodeBehind="List.aspx.vb" Inherits="ProjectsListPage" %>

<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="PageContent" runat="server">
    <script type="text/javascript">

        can_create = <% =Bool2JS(App.CanUserCreateNewProject) %>;
        var prj_last_dt = (new Date()).getTime();
        var can_manage_all = <% =Bool2JS(CanManageAllModels) %>;
        var can_modify = <% =Bool2JS(CanEditAtLeastOneModel) %>;
        var db_version = "<% = JS_SafeString(GetCurrentDBVersion.GetVersionString) %>";
        var columns_ver = "191203";
        var show_splash = false;    // temporary false due to old links
        var show_popups = false;
        var PrjDataSource = new DevExpress.data.CustomStore({});
        var projects_loaded = false;
        var force_reload = false;
        var projects = [];
        var master_projects = [];
        var template_projects = [];
        var wipeout_project = null;
    <% If App.isRiskEnabled Then %>var model_types = [{ "value": <% =CInt(ProjectType.ptRegular) %>, "text": "<% =JS_SafeString(ResString("optIsRiskModel")) %>", "comment": "<% =JS_SafeString(ResString("hintRiskModel")) %>" },
            { "value": <% =CInt(ProjectType.ptOpportunities) %>, "text": "<% =JS_SafeString(ResString("optIsOpportunityProject")) %>", "comment": "<% =JS_SafeString(ResString("hintOpportunityModel")) %>" },
        { "value": <% =CInt(ProjectType.ptMixed) %>, "text": "<% =JS_SafeString(ResString("optIsMixedModel")) %>", "comment": "<% =JS_SafeString(ResString("hintMixedModel")) %>" }<% If _OPT_MY_RISK_REWARD_ALLOWED Then %>,
        { "value": <% =CInt(ProjectType.ptMyRiskReward) %>, "text": "<% =JS_SafeString(ResString("optIsMyRiskRewardModel")) %>", "comment": "<% =JS_SafeString(ResString("hintIsMyRiskRewardModel")) %>" }<% End if %>
    ];<% End if %>

        var max_prj_multi = 100;
        var opt_use_last_used = true;

    <% if CanCreateNewModels Then %>var saveOptions = {
            Src_ID: 0,
            Save_Users: true,
            Hide_ObjName: false,
            Hide_AltName: false,
            Hide_UserEmail: false,
            Hide_UserName: false,
            Save_Snapshots: true,
            SaveAs: false,
            DBVer: <% = GetCurrentDBVersion.MinorVersion %>
    };

        var rr_mode = <% =Bool2JS(App.isRiskEnabled AndAlso App.Options.RiskionRiskRewardMode) %>;

        var option_sets_by_type = [];   // must be lower case
    <% if App.isRiskEnabled Then %>option_sets_by_type[<% =CInt(ProjectType.ptOpportunities) %>] = "opportunity model"; option_sets_by_type[<% =CInt(ProjectType.ptMyRiskReward) %>] = "riskreward";<% End if %>
    <% End if %>

        var grid_auto_collapse = <% = Bool2JS(GetCookie("prj_lst_autohide", If(App.isMobileBrowser, "1", "0")) = "1") %>;
        var grid_filtering = <% = Bool2JS(GetCookie("prj_filtering", "0") = "1") %>;
        var grid_autopager = <% = Bool2JS(GetCookie("prj_autopage", "0") = "1") %>;
        var grid_vscrolling = <% = Bool2JS(GetCookie("prj_vscroll", "0") = "1") %>;

        var grid_defaults = { "autocollapse": <% =Bool2JS(App.isMobileBrowser) %>, "filtering": false, "autopager": false, "vscrolling": false };

        var first_load = true;
        var grid_w_old = 0;
        var grid_h_old = 0;

        var grid_row_height = -1;
        var grid_pg_size = -1;

        //var last_row_opened = -1;

        var grid = null;
        var toolbar = null;

        var do_flash_advanced = false;

    <% If CanCreateNewModels Then %>var use_wkg_wording = <% =Bool2JS(Str2Bool(GetCookie("wkg_wording", "1"))) %>;
        var use_nodesets = <% =Bool2JS(Str2Bool(GetCookie("use_nodesets", ""))) %>;
        var projects_cnt = <% =App.ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxProjectsTotal) %>;
        var projects_max = <% =App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxProjectsTotal) %>;
        var lifetime_cnt = <% =App.ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxLifetimeProjects) %>;
        var lifetime_max = <% =App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxLifetimeProjects) %>;<% End if %>

    <%--var eval_url = "<% =JS_SafeString(PageURL(_PGID_EVALUATION)) %>";
    var eval_url = "<% =JS_SafeString(If(App.isEvalURL_EvalSite, PageURL(_PGID_SERVICEPAGE, String.Format("{0}={1}", _PARAM_ACTION, "eval_anytime")), PageURL(_PGID_EVALUATION))) %>";--%>
        var eval_url = "<% =JS_SafeString(PageURL(_PGID_EVALUATION,, True)) %>";

        function projectStatus() {
            switch (pgID) {
                case <% =_PGID_PROJECTSLIST_ARCHIVED %>:
                return "<% = ecProjectStatus.psArchived %>";
            case <% =_PGID_PROJECTSLIST_TEMPLATES %>:
                return "<% = ecProjectStatus.psTemplate %>";
            case <% =_PGID_PROJECTSLIST_MASTERPROJECTS %>:
                return "<% = ecProjectStatus.psMasterProject %>";
        }
        return "<% = ecProjectStatus.psActive %>";
        }

        function projectStatusInt() {
            switch (pgID) {
                case <% =_PGID_PROJECTSLIST_ARCHIVED %>:
                return "<% = CInt(ecProjectStatus.psArchived) %>";
            case <% =_PGID_PROJECTSLIST_TEMPLATES %>:
                return "<% = CInt(ecProjectStatus.psTemplate) %>";
            case <% =_PGID_PROJECTSLIST_MASTERPROJECTS %>:
                return "<% = CInt(ecProjectStatus.psMasterProject) %>";
        }
        return "<% = CInt(ecProjectStatus.psActive) %>";
        }

        function is_deleted() {
            return (pgID == <% =_PGID_PROJECTSLIST_DELETED %>);
        }

        function saveLastVisited(pgid) {
            var pg = pageByID(nav_json, pgid);
            if ((pg) && typeof pg.pguid != "undefined") {
                var _p = curPrjID;
                curPrjID = -1;
                var h = getWorkflowPagesHistory();
                h["p_" + pg.pguid] = pgid;
                localStorage.setItem(sess_workflow_history(), JSON.stringify(h));
                curPrjID = _p;
            }
        }

        function onSetPage(pgid) {
            var res = false;
            if (can_modify) {
                switch (pgid % _pgid_max_mod) {
                    case <% =_PGID_PROJECTSLIST %>:
                case <% =_PGID_PROJECTSLIST_ARCHIVED %>:
                case <% =_PGID_PROJECTSLIST_TEMPLATES %>:
                case <% =_PGID_PROJECTSLIST_MASTERPROJECTS %>:
                case <% =_PGID_PROJECTSLIST_DELETED %>:
                        pgID = pgid;
                        initToolbar();
                        initGrid();
                        //showLoadingPanel();
                        //if (hasGrid()) {
                        //    ////grid.beginUpdate();
                        //    //grid.option("columns", getColumns());
                        //    //showLoadingPanel();
                        //    //grid.filter(getFilter());
                        //    ////grid.endUpdate();
                        //    initGrid();
                        //} else {
                        //    initGrid();
                        //}
                        saveLastVisited(pgid);
                        res = true;
                        break;
                }
            }
            return res;
        }

        function resizeGrid() {
            var d = ($("body").hasClass("fullscreen") ? 8 : (grid_autopager ? 2 : 2));
            $("#grid").height(160).width(240);
            $("#toolbar").hide();
            var td = $("#trGrid");
            var w = $("#grid").width(Math.round(td.width() - d)).width();
            if (($("#toolbar")) && ($("#toolbar").length)) $("#toolbar").show().dxToolbar("instance").repaint();
            updateToolbarButtons();
            var h = $("#grid").height(Math.round(td.height() - d - 2)).height();
            //$("#grid").css("max-width", w);
            //$("#grid").show();
            if ((grid_w_old != w || grid_h_old != h)) {
                grid_w_old = w;
                grid_h_old = h;
                onSetAutoPager();
                return true;
            };
            return false;
        }

        function checkResize(w_o, h_o, real_resize) {
            var w = $(window).width();
            var h = $(window).height();
            if (!w || !h || !w_o || !h_o || (w == w_o && h == h_o)) {
                if (resizeGrid() && (real_resize) && hasGrid()) {
                    grid.beginUpdate();
                    if (grid_w_old > 100) grid.option("width", grid_w_old);
                    if (grid_h_old > 100) grid.option("height", grid_h_old);
                    grid.endUpdate();
                }
            }
        }

        function onResize(force_redraw, real_resize) {
            var w = $(window).width();
            var h = $(window).height();
            if (force_redraw) {
                grid_w_old = 0;
                grid_h_old = 0;
            }
            if (real_resize) {
                setTimeout(function () {
                    checkResize(w, h, real_resize);
                }, 150);
            } else {
                checkResize(force_redraw ? 0 : w, force_redraw ? 0 : h, real_resize);
            }
        }

        function onRefresh(data) {
            if ((data)) {
                if (hasGrid()) grid.clearSelection();
                force_reload = true;
                reloadProjectsList();
            } else {
                DevExpress.ui.notify(resString("msgCantPerformAction"), "error");
            }
        }

<% If CanEditAtLeastOneModel Then %>
        function onDelete(ids) {
            if ((ids) && (ids.length)) {
                var result = DevExpress.ui.dialog.confirm(((ids) && ids.length > 1 ? resString((is_deleted() ? "msgSureDeleteProjectSeleted" : "msgConfirmDeleteProjects")) : resString((is_deleted() ? "msgSureDeleteProject" : "msgConfirmDeleteProject"))), resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    if (dialogResult) {
                        if (is_deleted()) {
                            wipeoutProjects(ids);
                        } else {
                            deleteProjects(ids, true);
                        }
                    }
                });
            }
        }

        function onUnDelete(ids) {
            if ((ids) && (ids.length)) {
                var result = DevExpress.ui.dialog.confirm(((ids) && ids.length > 1 ? resString("msgConfirmUnDeleteProjects") : resString("msgConfirmUnDeleteProject")), resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    if (dialogResult) deleteProjects(ids, false);
                });
            }
        }

        function onArchive(ids) {
            if ((ids)) {
                var status = (pgID == <% =_PGID_PROJECTSLIST_ARCHIVED %> ? "<% =CInt(ecProjectStatus.psActive) %>" : "<% =CInt(ecProjectStatus.psArchived) %>");
            var args = { "ids": ids.join(), "archived": status };
            callAPI("project/?<% =_PARAM_ACTION %>=Set_Archived", args, function (data) {
                    onIterateProjectsList(data, status, function (prj) {
                        prj.ProjectStatus = status;
                        pushProjectData(prj);
                    });
                });
            }
        }

        function onDownload(ids) {
            if ((ids)) {
                var result = DevExpress.ui.dialog.confirm(resString("confSaveSnapshots"), resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    onDownloadFiles(ids, (dialogResult), "ahps");
                });
            }
        }

        function onDownloadFiles(ids, snapshots, fmt) {
            if ((ids)) {
                doDownload("<% =JS_SafeString(PageURL(_PGID_PROJECT_DOWNLOAD, _PARAM_ACTION + "=download")) %>" + ((ids) && (ids.length > 1) ? "&mode=multi&list=" + ids.slice(0, max_prj_multi).join(",") : "&id=" + ids[0]) + "&type=" + fmt + "&snapshots=" + (snapshots ? "yes" : "no"), function () { onDownloaded(snapshots, fmt); });
            showLoadingPanel("<% =JS_SafeString(ResString("msgPrepareDownload")) %>" + (ids.length > 1 ? " (models: " + ids.length + ")" : ""));
            }
        }

        function onDownloaded(snapshots, fmt) {
            if (hasGrid()) {
                var selected = getSelected();
                var s = selected.slice(max_prj_multi);
                grid.selectRows(s, false);
                if ((s) && (s.length)) onDownloadFiles(s, snapshots, fmt);
            }
        }<% End If %>

    <% If CanCreateNewModels Then %>
        function restoreMaster() {
            $(".dx-dialog.dx-popup").each(function () {
                if ($(this).data("dxPopup")) $(this).dxPopup("instance").hide();
            });
            var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgRestoreMasterProjects")) %>", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
        result.done(function (dialogResult) {
            if (dialogResult) callAPI("project/?<% =_PARAM_ACTION %>=restore_defaults", {}, onRestoreMaster);
        });
        }

        function onRestoreMaster(data) {
            if (isValidReply(data) && (data.Result == _res_Success) && (typeof data.Data != "undefined") && (data.Data)) {
                force_reload = true;
                reloadProjectsList();
                DevExpress.ui.notify("<% =JS_SafeString(ResString("msgRefreshProjects")) %>");
            }
        }

        function uploadProject(on_close) {
            maxUploadSize = <% =GetMaxUploadSize %>;
        uploadDialog("<% =JS_SafeString(ResString("mnuProjectUpload")) %>", can_create, _API_URL + "project/?<% =_PARAM_ACTION %>=upload", onUploadProject, null, false, null, on_close);
        }

        function onUploadProject(data) {
            if (isValidReply(data) && (data.Result == _res_Success) && (typeof data.Data != "undefined") && (data.Data != null) && (data.Data.length) && (data.Data[0] == "replace")) do_ping = false;
            onOpenProject(data);
        }

        function checkSaveAsOptions(form) {
            //var form = $("#save_options").dxForm("instance");
            if ((form)) {
                form.getEditor("Hide_UserEmail").option("disabled", !saveOptions.Save_Users);
                form.getEditor("Hide_UserName").option("disabled", !saveOptions.Save_Users);
                form.getEditor("Save_Snapshots").option("disabled", !saveOptions.Save_Users || saveOptions.Hide_UserEmail || saveOptions.Hide_UserName || saveOptions.Hide_ObjName || saveOptions.Hide_AltName);
            }
        }

        function onSaveAsWithConfirm(id, orig_name, def_name) {

            $("#popupContainer").dxPopup({
                maxWidth: 600,
                height: "auto",
                title: "<% =JS_SafeString(ResString("titleConfirmation")) %>",
            animation: {
                //show: { "duration": 0 },
                hide: { "duration": 0 }
            },
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    text: "<% =JS_SafeString(ResString("btnContinue")) %>",
                    elementAttr: { "class": "button_enter" },
                    onClick: function () {
                        onSaveAs(id, orig_name, def_name);
                        return false;
                    }
                },
                toolbar: "bottom",
                location: 'after'
            }, {
                    widget: 'dxButton',
                    options: {
                        //icon: "far fa-times-circle", 
                        text: "<% =JS_SafeString(ResString("btnCancel")) %>",
                        elementAttr: { "class": "button_esc" },
                        onClick: function () {
                            closePopup();
                            return false;
                        }
                    },
                    toolbar: "bottom",
                    location: 'after'
                }],
            contentTemplate: function () {
                return $("<div>" + resString("msgAddModelWhenLifetime") + "</div>");
            }
        });
            $("#popupContainer").dxPopup("show");
        }

        function onSaveAs(id, orig_name, def_name) {
            saveOptions.Src_ID = id;
            $("#popupContainer").dxPopup({
                width: 450,
                height: "auto",
                title: replaceString("{0}", htmlUnescape(ShortString(orig_name, 30)), "<% =JS_SafeString(ResString("titleProjectSaveAs")) %>"),
            animation: {
                show: { "duration": 0 },
                hide: { "duration": 0 }
            },
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    text: "<% = JS_SafeString(ResString("btnSave")) %>",
                    elementAttr: { "class": "button_enter" },
                    onClick: function () {
                        var name = $('#inpName').find('input').val();
                        if (name == "") {
                            DevExpress.ui.notify("<% =JS_SafeString(ResString("msgPrjNameEmpty")) %>", "error");
                            setTimeout(function () { $('#inpName').find('input'); }, 500);
                        } else {
                            //var p = $("#save_options").dxForm("instance").option("formData");
                            var args = {}; //{"name": encodeURIComponent(replaceString('"', '\"', name))};
                            var form = $("#save_options").dxForm("instance");
                            for (var prop in saveOptions) {
                                if (saveOptions.hasOwnProperty(prop) && (typeof form.getEditor(prop) == "undefined" || !(form.getEditor(prop).option("disabled")))) {
                                    if (prop != "DBVer" || saveOptions["SaveAs"]) args[prop] = saveOptions[prop];
                                }
                            }
                            if (typeof projects_cnt != "undefined") projects_cnt += 1;
                            if (typeof lifetime_cnt != "undefined") lifetime_cnt += 1;
                            showProjectsCounter(getSelected().length);
                            callAPI("project/?<% =_PARAM_ACTION %>=copy&name=" + encodeURIComponent(name), args, onOpenProject);
                        }
                        return false;
                    }
                },
                toolbar: "bottom",
                location: 'after'
            }, {
                    widget: 'dxButton',
                    options: {
                        //icon: "far fa-times-circle", 
                        text: "<% = JS_SafeString(ResString("btnClose")) %>",
                        elementAttr: { "class": "button_esc" },
                        onClick: function () { closePopup(); return false; }
                    },
                    toolbar: "bottom",
                    location: 'after'
                }],
            contentTemplate: function () {
                var dlg = "<div class='option_title'><b><% = JS_SafeString(ResString("lblProjectName")) %></b>:<div id='inpName'></div></div><div id='save_accordion' class='text' style='margin-top:6px;'><div id='save_options'></div></div>";
                return $(dlg);
            }
        });
        $("#popupContainer").dxPopup("show");

        var db_versions = [];
        for (var i =<% =GetCurrentDBVersion.MinorVersion%>; i > 34; i--) {
            db_versions.push({ "id": i, "title": ("1.1." + i) });
        }

        var frm = $("#save_options").dxForm({
            formData: saveOptions,
            items: [{
                dataField: "Save_Users",
                editorType: "dxCheckBox",
                label: { text: " " },
                editorOptions: { text: "<% =JS_SafeString(ResString("lblCopyParticipants")) %> " }
            }, {
                    dataField: "Hide_UserEmail",
                    editorType: "dxCheckBox",
                    label: { text: " " },
                    editorOptions: { text: "<% =JS_SafeString(ResString("lblCamouflageEmails")) %> " }
                }, {
                    dataField: "Hide_UserName",
                    editorType: "dxCheckBox",
                    label: { text: " " },
                    editorOptions: { text: "<% =JS_SafeString(ResString("lblCamouflageNames")) %> " }
                }, {
                    dataField: "Hide_ObjName",
                    editorType: "dxCheckBox",
                    label: { text: " " },
                    editorOptions: { text: "<% =JS_SafeString(ResString("lblCamouflageObjs")) %> " }
                }, {
                    dataField: "Hide_AltName",
                    editorType: "dxCheckBox",
                    label: { text: " " },
                    editorOptions: { text: "<% =JS_SafeString(ResString("lblCamouflageAlts")) %> " }
                }, {
                    dataField: "Save_Snapshots",
                    editorType: "dxCheckBox",
                    label: { text: " " },
                    editorOptions: { text: "<% =JS_SafeString(ResString("lblCopySnapshots")) %> " }
                }, {
                    dataField: "SaveAs",
                    editorType: "dxCheckBox",
                    label: { text: " " },
                    template: function (data, itemElement) {
                        itemElement.append("<div class='table' style='width:auto'><div class='tr'><div id='divSaveAsVersion' class='td option' style='padding-right:1ex'></div><div id='divVersion' class='td'></div></div></div>");
                        $("#divVersion").dxSelectBox({
                            dataSource: new DevExpress.data.ArrayStore({
                                data: db_versions,
                                key: "id"
                            }),
                            displayExpr: "title",
                            valueExpr: "id",
                            value: saveOptions.DBVer,
                            onValueChanged: function (e) {
                                saveOptions.DBVer = e.value;
                            }
                        });
                        $("#divSaveAsVersion").dxCheckBox({
                            value: saveOptions.SaveAs,
                            text: "<% =JS_SafeString(ResString("lblSaveAsVersion")) %>: ",
                        onContentReady: function (e) {
                            $("#divVersion").dxSelectBox("instance").option("disabled", !saveOptions.SaveAs);
                        },
                        onValueChanged: function (e) {
                            saveOptions.SaveAs = e.value;
                            if (saveOptions.SaveAs) $("#divVersion").dxSelectBox("instance").focus();
                            $("#divVersion").dxSelectBox("instance").option("disabled", !e.value);
                        }
                    });
                    }
                }],
            onFieldDataChanged: function (e) {
                checkSaveAsOptions(e.component);
            },
            readOnly: false,
            showColonAfterLabel: false,
            labelLocation: "left"
        }).dxForm("instance");

<%--        $("#save_accordion").dxAccordion({
            dataSource: ["<% = JS_SafeString(ResString("lblCopyOptions")) %>"],
            animationDuration: 300,
            collapsible: true,
            multiple: false,
            selectedIndex: 0
        })//.dxAccordion("instance").collapseItem(0);--%>

            $("#popupContainer").dxPopup("hide");
            $("#inpName").dxTextBox({
                value: (typeof def_name != "undefined" ? def_name : "")
            });
            $("#popupContainer").dxPopup("show");

            setTimeout(function () { $('#inpName').find('input').select().focus(); }, 500);
            checkSaveAsOptions(frm);
        }

        function getLastID(name) {
            if (opt_use_last_used) {
                var id = localStorage.getItem(name +"_<% =App.ActiveWorkgroup.ID %>");
                if (typeof id != " undefined" && (id)) return id * 1; else return "";
            } else {
                return -1;
            }
        }

        function setLastID(name, val) {
            if (opt_use_last_used) localStorage.setItem(name +"_<% =App.ActiveWorkgroup.ID %>", val);
        }

        var orig_mp = -1;
        function createProject(res_title, res_select, prj_list, last_used, def_name, def_src_id, on_close, is_tpl_master) {
            var from_name = (last_used == "" && prj_list == master_projects ? "master_last" : "");
            if (last_used == "" && prj_list == template_projects) from_name = "tpl_last";
            if (last_used == "" && from_name != "") last_used = getLastID(from_name);
            if ((prj_list) && (last_used) && (last_used > 0)) {
                var has_in_list = false;
                for (var i = 0; i < prj_list.length; i++) {
                    if (prj_list[i].ID == last_used) {
                        has_in_list = true;
                        break;
                    }
                }
                if (!has_in_list) last_used = "";
            }
            if (opt_use_last_used && prj_list.length && (typeof last_used == "undefined" || last_used == "" || last_used <= 0)) last_used = prj_list[0].ID;
            var is_create_new = (prj_list == master_projects);
            if (is_create_new) orig_mp = last_used;
            $("#popupContainer").dxPopup({
                width: dlgMaxWidth(480),
                height: "auto",
                title: res_title,
                animation: {
                    show: { "duration": 0 },
                    hide: { "duration": 0 }
                },
                toolbarItems: [{
                    widget: 'dxButton',
                    options: {
                        text: "<% = JS_SafeString(ResString("btnCreate")) %>",
                    elementAttr: { "class": "button_enter" },
                    onClick: function () {
                        var name = $('#inpName').find('input').val();
                        if (name == "") {
                            DevExpress.ui.notify("<% =JS_SafeString(ResString("msgPrjNameEmpty")) %>", "error");
                            setTimeout(function () { $('#inpName').find('input').focus(); }, 500);
                        } else {

                            var tf = "";
                            var can_pass = true;
                            if (isRiskion && is_create_new) {
                                tf = $('#inpTimeframe').find('input').val();
                                if (tf == "") {
                                    DevExpress.ui.notify("<% =JS_SafeString(ResString("msgTimeframeEmpty")) %>", "error");
                                    setTimeout(function () { $('#inpTimeframe').find('input').focus(); }, 500);
                                    can_pass = false;
                                }
                            }

                            if (can_pass) {
                                var divSrc = $("#inpSrcPrj");
                                if ((prj_list.length) && (divSrc) && divSrc.is(":visible") && divSrc.data("dxSelectBox")) {
                                    var sel = divSrc.dxSelectBox("option", "value");
                                    if (sel == "" || sel <= 0 || sel == null) {
                                        DevExpress.ui.notify("'" + res_select + "' " + resString("lblIsNotSpecified"), "error");
                                        setTimeout(function () {
                                            divSrc.dxSelectBox("instance").focus();
                                            divSrc.addClass("blink_red");
                                            setTimeout(function () {
                                                divSrc.dxSelectBox("instance").open();
                                                divSrc.removeClass("blink_red");
                                            }, 2000);
                                        }, 150);
                                        return false;
                                    }
                                }

                                var args = { "name": name };
                                if ((prj_list.length) || (def_src_id > 0)) {
                                    args["src_id"] = ((def_src_id > 0) ? def_src_id : $("#inpSrcPrj").dxSelectBox("option", "value"));
                                }
                                var t = projectStatusInt();
                                if ((prj_list == template_projects || prj_list == [] || !(prj_list.length)) && t != <% = CInt(ecProjectStatus.psActive) %>) t = <% = CInt(ecProjectStatus.psActive) %>;
                                if (is_tpl_master && ($("#rbProjectType").length)) t = $("#rbProjectType").dxRadioGroup("option", "value");
                                args["status"] = t;
                                if (isRiskion && is_create_new && ($("#lblModelType").length)) {
                                    args["model_type"] = $("#lblModelType").dxRadioGroup("option", "value");
                                }
                                if (is_create_new) {
                                    if ($("#cbUseNodesets").length) {
                                        args["use_nodesets"] = use_nodesets;
                                    } else {
                                        args["use_nodesets"] = false;
                                    }
                                    if ($("#cbUseWkgWording").length) {
                                        use_wkg_wording;
                                    } else {
                                        args["wkg_wording"] = false;
                                    }
                                    if (isRiskion) args["timeframe"] = tf;
                                    if (($("#shortDesc")) && ($("#shortDesc").length)) args["description"] = $("#shortDesc").dxTextArea("instance").option("value");
                                }
                                callAPI("project/?<% =_PARAM_ACTION %>=create", args, onOpenProject);
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
                        //icon: "far fa-times-circle", 
                        text: "<% = JS_SafeString(ResString("btnClose")) %>",
                        elementAttr: { "class": "button_esc" },
                        onClick: function () {
                            closePopup();
                            if (typeof on_close == "function") on_close();
                            return false;
                        }
                    },
                    toolbar: "bottom",
                    location: 'after'
                }],
            contentTemplate: function () {
                var dlg = "<div class='option_title'><span id='lblPrjName'><b>" + (is_tpl_master ? "" : "<% = JS_SafeString(ResString("lblProjectName")) %>") + "</b>*</span>:<div id='inpName'></div></div>";
                if (is_create_new) dlg += "<div style='margin-top:1ex'><% =JS_SafeString(ResString("lblShortDesc")) %>:<div id='shortDesc'></div></div>";
                if (isRiskion && is_create_new) dlg += "<div style='margin-top:1ex'><b><% =JS_SafeString(ResString("lblPrjTimeframe")) %></b>*:<div id='inpTimeframe'></div></div>";
                if (is_tpl_master) dlg += "<div id='rbProjectType' style='margin-top:1ex'></div>";
                if (isRiskion && is_create_new) dlg += "<div style='margin-top:1ex;' id='lblPrjType'><% = JS_SafeString(ResString("tblProjectType")) %>:</div><div id='lblModelType' style='padding-left:1em'></div>";
                if (prj_list.length) dlg += "<div style='margin-top:1ex' id='lblPrjSrc'>" + res_select + ":<div id='inpSrcPrj'></div></div>";
                <% If App.NodeSets_GetList(If(App.isRiskEnabled, ecNodeSetHierarchy.hidLikelihood, ecNodeSetHierarchy.hidObjectives), True, Nothing).Count > 0 Then %>if (is_create_new) dlg += "<div><div style='margin-top:1ex' id='cbUseNodesets'></div></div>";<% End If %>
                if (is_create_new) dlg += "<div style='margin-top:1ex' id='cbUseWkgWording'></div>";
                return $(dlg);
            }
        });
        $("#popupContainer").dxPopup("show");
        $("#inpName").dxTextBox({
            value: (typeof def_name != "undefined" ? def_name : "")
            //value: "<% =JS_SafeString(ResString("mnuProjectCreate")) %>"
        });
        if (is_tpl_master) {
            updatePrjNameWording(true);
            $("#rbProjectType").dxRadioGroup({
                dataSource: [{ "value":"<% =CInt(ecProjectStatus.psTemplate) %>", "text": "<% = JS_SafeString(ResString("lblCreateTemplate")) %>" },
                    { "value":"<% =CInt(ecProjectStatus.psMasterProject) %>", "text": "<% = JS_SafeString(ResString("lblCreateMasterProject")) %>" }],
                displayExpr: "text",
                valueExpr: "value",
                value: "<% =CInt(ecProjectStatus.psTemplate) %>",
                onValueChanged: function (e) {
                    var is_tpl = e.value == "<% =CInt(ecProjectStatus.psTemplate) %>";
                    updatePrjNameWording(is_tpl);
                    $('#inpName').find('input').val(is_tpl ? def_name : "").focus().select();
                }
            });
        }
        if (prj_list.length) {
            $("#inpSrcPrj").dxSelectBox({
                dataSource: new DevExpress.data.ArrayStore({
                    data: prj_list,
                    key: "ID"
                }),
                onValueChanged: function (e) {
                    if (from_name != "") {
                        setLastID(from_name, e.value);
                        if (opt_use_last_used && !e.component.option("disabled")) orig_mp = e.value;
                    }
                },
                displayExpr: "Name",
                valueExpr: "ID",
                value: last_used
            });
        }
        if (isRiskion && is_create_new) {

            $("#inpTimeframe").dxTextBox({});

            var mt = getLastID("mt");
            var has_mt = false
            for (var i = 0; i < model_types.length; i++) {
                if (model_types[i].value == mt) has_mt = true;
            }
            if (typeof mt == "undefined" || mt == "" || mt <= 0 || !has_mt) mt = model_types[0].value;
            if (rr_mode) mt = <% =CInt(ProjectType.ptMyRiskReward) %>;
            $("#lblModelType").dxRadioGroup({
                items: model_types,
                value: mt,
                valueExpr: "value",
                displayExpr: "text",
                //layout: "horizontal",
                itemTemplate: function (itemData, _, itemElement) {
                    if (typeof itemData.comment != "undefined" && itemData.comment != "") itemElement.html(itemData.text + "<div class='text small' style='padding-bottom:4px; opacity:0.8;'>" + itemData.comment + "</div>");
                },
                onValueChanged: function (e) {
                    setLastID("mt", e.value);
                    if (!is_tpl_master) checkProjectTypeAndOptionSets(e.value, prj_list);
                }
            });
            $("#lblModelType .dx-radio-value-container").css("vertical-align", "top");
        }
        if (is_create_new && $("#shortDesc").length) {
            $("#shortDesc").dxTextArea({
                value: "",
                height: 64,
            });
        }
        if (is_create_new && $("#cbUseNodesets").length) {
            $("#cbUseNodesets").dxCheckBox({
                value: use_nodesets,
                text: "<% =JS_SafeString(ResString("cmnuObjectivesAddFromDataset")) %>",
                onValueChanged: function (e) {
                    use_nodesets = e.value;
                    document.cookie = 'use_nodesets=' + ((use_nodesets) ? "1" : "0");
                }
            });
        }
        if (is_create_new && $("#cbUseWkgWording").length) {
            $("#cbUseWkgWording").dxCheckBox({
                value: use_wkg_wording,
                text: "<% =JS_SafeString(ResString("lblUserWorkgroupWording")) %>",
                onValueChanged: function (e) {
                    use_wkg_wording = e.value;
                    document.cookie = 'wkg_wording=' + ((use_wkg_wording) ? "1" : "0");
                }
            });
            }
            if (is_create_new && !is_tpl_master) {
                checkProjectTypeAndOptionSets(mt, prj_list);
                if (rr_mode) {  // hide ability to choose type and default option set when started via /rr page (RiskReward mode);
                    $("#lblPrjSrc").hide();
                    $("#lblPrjType").hide();
                    $("#lblModelType").hide();
                }
            }
            $("#popupContainer").dxPopup("hide");
            $("#popupContainer").dxPopup("show");
            setTimeout(function () {
                $('#inpName').find('input').focus().select();
            }, 500);
        }

        function checkProjectTypeAndOptionSets(type, prj_list) {
            if (typeof option_sets_by_type != "undefined" && option_sets_by_type.length && prj_list.length) {
                var mp = $("#inpSrcPrj").dxSelectBox("instance");
                var master = "";
                if (typeof option_sets_by_type[type] != "undefined") master = option_sets_by_type[type];
                var has_mp = false;
                if (master != "") {
                    for (var i = 0; i < prj_list.length; i++) {
                        var n = prj_list[i].Name.toLowerCase();
                        if (n.indexOf(master) >= 0) {
                            if (!mp.option("disabled")) {
                                if (opt_use_last_used) orig_mp = prj_list[i].ID;
                                mp.option("disabled", true);
                                $("#lblPrjSrc").prop("disabled", "disabled").addClass("disabled");
                            }
                            mp.option("value", prj_list[i].ID);
                            has_mp = true;
                            break;
                        }
                    }
                }
                if (master == "" || !has_mp) {
                    if (mp.option("disabled")) {
                        mp.option("disabled", false);
                        if (opt_use_last_used && orig_mp > 0) mp.option("value", orig_mp); else mp.option("value", orig_mp);
                        $("#lblPrjSrc").prop("disabled", "").removeClass("disabled");
                    }
                }
            }
        }

        function updatePrjNameWording(is_tpl) {
            $("#lblPrjName").html(is_tpl ? "<% = JS_SafeString(ResString("lblTemplateName")) %>" : "<% = JS_SafeString(ResString("lblMasterProjectName")) %>");
    }<% End If %>

        function showOnlineUsers(prjid) {
            callAPI("pm/user/?<% =_PARAM_ACTION %>=list_online", { "id": prjid }, onOnlineUsers);
        }

        function onOnlineUsers(res) {
            if (isValidReply(res)) {
                if (res.Result == _res_Success && typeof res.Data != "undefined" && (res.Data != null)) {
                    var lst = "";
                    for (var i = 0; i < res.Data.length; i++) {
                        lst += "<div>&#149; " + res.Data[i].UserEmail + "</div>";
                    }
                    DevExpress.ui.dialog.alert(resString("lblOnlineUsersList") + "<div style='max-height:10em; overflow-y:auto; padding-left:1em'>" + lst + "</div>", resString("lblOnlineUsers"));
                    return true;
                }
            }
            showResMessage(err, true);
        }

        function joinTTSession(prjid) {
            open_tt_popup = true;
            openProject(prjid);
        }

        function resumeTTSession(prjid) {
            setTimeout(function () { showTTWindow(prjid); }, 500);
            onProjectPage("<% = JS_SafeString(PageURL(_PGID_TEAMTIME_STATUS)) %>", prjid);
        }

        function stopTTSession(prjid) {
            var result = DevExpress.ui.dialog.confirm(resString("msgStopTTMeeting"), resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    callAPI("project/?<% =_PARAM_ACTION %>=TeamTime_Stop", { "id": prjid }, onStopTTSessions);
            }
        });
        }

        function onStopTTSessions(res) {
            if (isValidReply(res)) {
                if (res.Result == _res_Success && typeof projects != "undefined") {
                    var p = getItemByID(projects, res.ObjectID)
                    if ((p)) {
                        p.TeamTimeStatus = -1;
                        p.LockStatus = <% =CInt(ECLockStatus.lsUnLocked) %>;
                        p.ComplexStatus = getComplexStatus(p);
                        if (res.Data != "undefined" && res.Data != null && typeof res.LockStatus != "undefined") p = res.Data;
                        pushProjectData(p);
                    }
                    return true;
                }
            }
            showResMessage(res, typeof res == "undefined" || res.Result == _res_error);
        }

        function showProjectsCounter(sel_count) {
            var info = $(".dx-datagrid-pager").find(".dx-info");
            if ((info) && (info.length)) {
                var prj = info.find("#spanProjectsCount");
                if (!(prj) || !(prj.length)) {
                    var txt = info.html();
                    var idx = txt.indexOf(".");
                    if (idx < 0) {
                        idx = 0;
                        txt = ". " + txt;
                    }
                    if ((idx)) txt = txt.substr(idx);
                    info.html("<span id='spanProjectsCount'></span><span class='nowide800'>" + txt + "</span>");
                    prj = info.find("#spanProjectsCount");
                }
                if ((prj) || (prj.length)) {
                    var rows = (hasGrid() ? grid.totalCount() : 0);
                    var msg = "";
                    if (sel_count > 0) {
                        msg = replaceString("{0}", sel_count, replaceString("{1}", rows, "<% = JS_SafeString(ResString("lblProjectsSelected")) %>"));
                } else {
                    msg = (rows > 0 ? "<% = JS_SafeString(ResString("lblProjects")) %>: " + rows : "");
                    <% If CanCreateNewModels Then %>if (pgID == <% =_PGID_PROJECTSLIST %>) {
                        var left1 = -1;
                        var left2 = -1;
                        if (projects_max > 0) left1 = projects_max - projects_cnt;
                        if (lifetime_max > 0) left2 = lifetime_max - lifetime_cnt;
                        var left = (left2 > left1 ? left2 : left1);
                        if (left > 0 && left < 10000) msg += "<span class='nowide700'>" + (msg == "" ? "" : " (") + "<% = JS_SafeString(ResString("lblProjectsRemaining")) %> " + left + (msg == "" ? "" : ")") + "</span>";
                    }<% End If %>
                }
                prj.html(msg);
            }
            var ps = $(".dx-page-sizes");
            if ((ps) && (ps.length)) {
                var lbl = $("#lblPageSize");
                if (!(lbl) || !(lbl.length)) {
                    $("<span style='opacity: 0.6;' class='nowide900' id='lblPageSize'><% =JS_SafeString(ResString("lblRowsPerPage")) %>:&nbsp;</span>").prependTo(ps);
                }
                if ((grid)) {
                    var s = grid.option("paging.pageSize");
                    $(".dx-page-sizes .dx-page-size").each(function () {
                        if ($(this).text() == s) $(this).addClass("dx-selection");
                    });
                }
                //dx-selection
            }
        }
        var lbl = $("#lblHasFilter");
        if ((lbl) && (lbl.length) && (grid)) {
            var msg = "";
            var c = 0;
            if (getSearchValue() != "") { c++; msg += "<nobr><i class='fas fa-search' style='color:#0058a3;'></i>&nbsp;<% = JS_SafeString(ResString("lblSearchApplied")) %> <span class='gray'>[<a href='' onclick='resetSearch(); return false;' class='actions dashed'><% = JS_SafeString(ResString("btnReset")) %></a>]</span></nobr>"; }
            if (hasFilter()) { c++; msg += (msg == "" ? "" : " &nbsp; ") + "<nobr><i class='fas fa-filter' style='color:#0058a3;'></i>&nbsp;<% = JS_SafeString(ResString("lblFilterApplied")) %> <span class='gray'>[<a href='' onclick='resetFiltering(); return false;' class='actions dashed'><% = JS_SafeString(ResString("btnReset")) %></a>]</span></nobr>"; }
                if (msg != "") msg = "<div class='warning nowide" + (c > 1 ? "1200" : "1000") + "' style='border:none; border-radius:3px; padding:3px 8px'>" + msg + "</div>";
                lbl.html(msg);
                lbl.width(msg == "" ? 0 : c * 180 + 40);
            }

            $(".dx-group-panel-message").removeClass("nowide800").addClass("nowide800");
        }

        function getSearchValue() {
            var res = "";
            if ((grid)) {
                var tbSearch = $("input", ".dx-searchbox");
                //var flt = grid.getCombinedFilter(false);
                if ((tbSearch) && (tbSearch.length)) res = tbSearch.prop("value");
            }
            return res;
        }

        function resetSearch() {
            if ((grid)) {
                grid.searchByText("");
                onSearchEdit();
            }
        }

        function hasFilter() {
            var has_flt = false;
            if ((grid)) {
                for (var i = 0; i < grid.columnCount(); i++) {
                    var flt = (grid.columnOption(i).filterValue);
                    if (((flt) && typeof flt != "undefined" && flt != "") || (grid.columnOption(i).filterValues != null)) {
                        has_flt = true;
                        break;
                    }
                }
            }
            return has_flt;
        }

        function resetFiltering() {
            if ((grid)) {
                var s = getSearchValue();
                grid.clearFilter("row");
                grid.clearFilter("header");
                grid.clearFilter("filterValue");
                grid.filter(getFilter());
                if (s != "") grid.searchByText(s);
                showProjectsCounter(getSelected().length);
            }
        }

        function getSelected() {
            var lst = [];
            if (hasGrid()) {
                //var lst = grid.getSelectedRowKeys();
                var st = projectStatusInt();
                var del = is_deleted();
                var selected = grid.getSelectedRowsData();
                for (var i = 0; i < selected.length; i++) {
                    if (selected[i].CanModify && (del || selected[i].ProjectStatus == st) && selected[i].isMarkedAsDeleted == del) lst.push(selected[i].ID);
                }
            }
            return lst;
        }

        function initToolbar() {<% If CanEditAtLeastOneModel Then %>
            var is_regular = (pgID == <% =_PGID_PROJECTSLIST %> || pgID == <% =_PGID_PROJECTSLIST_REVIEW %>);
        var is_tpl = (pgID == <% =_PGID_PROJECTSLIST_TEMPLATES %>);
        var is_master = (pgID == <% =_PGID_PROJECTSLIST_MASTERPROJECTS %>);
        var has_sel = (hasGrid() ? getSelected().length > 0 : false);

        var prj_tabs = [];
        var prj_ids = [<% =GetTabIDs %>];
        for (var i = 0; i < prj_ids.length; i++) {
            var p = pageByID(nav_json, prj_ids[i]);
            if ((p)) {
                prj_tabs.push({ "id": p.pgID, "icon": p.icon, "text": p.text });
            }
        }

        $("#toolbar").dxToolbar({
            //disabled: !(toolbar), //AD: cause totally locked
            items: [{
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxTabs',
                orientation: 'vertical',
                visible: (prj_tabs.length > 1),
                options: {
                    items: prj_tabs,
                    keyExpr: "id",
                    selectedItemKeys: [pgID],
                    onItemClick: function (e) {
                        //                            if (!onSetPage(e.itemData.id)) onOpenPage(e.itemData.id);
                        openPage(pageByID(nav_json, e.itemData.id));
                    }
                }
            }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: (<% =Bool2JS(CanCreateNewModels) %> && (is_regular || is_tpl)),
                options: {
                    disabled: !can_create,
                    text: resString((rr_mode ? "titleNewRRProject" : "titleProjectCreate")),
                    icon: 'doc',
                    elementAttr: { id: 'btnCreate' },
                    onClick: function () {
                        createProject((is_tpl ? "<% = JS_SafeString(ResString("titleCreateTemplate")) %>" : "<% = JS_SafeString(ResString(If(App.Options.RiskionRiskRewardMode, "titleNewRRProject", "titleProjectCreate"))) %>"), "<% = JS_SafeString(ResString("lblBasedOn")) %>", master_projects, "", "", -1, undefined, false);
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: (<% =Bool2JS(CanCreateNewModels) %> && (is_regular || is_tpl)),
                options: {
                    disabled: (!(template_projects.length) || !can_create),
                    text: resString("btnCreateFromTemplate"),
                    icon: 'box',
                    elementAttr: { id: 'btnCreateFromTemplate' },
                    onClick: function () {
                        createProject("<% = JS_SafeString(ResString("btnCreateFromTemplate")) %>", "<% = JS_SafeString(ResString("lblProjectTemplate")) %>", template_projects, "", "", -1, undefined, false);
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: (<% =Bool2JS(App.CanUserDoAction(ecActionType.at_alUploadModel, App.ActiveUserWorkgroup, App.ActiveWorkgroup)) %> && (is_regular || pgID == <% =_PGID_PROJECTSLIST_TEMPLATES %>)),
                    options: {
                        disabled: !can_create,
                        text: resString("titleProjectUpload"),
                        icon: 'upload',
                        elementAttr: { id: 'btnUpload' },
                        onClick: function () {
                            uploadProject();
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: is_master,
                    options: {
                        text: resString("btnRestoreMasterProjects"),
                        icon: 'fas fa-magic',
                        elementAttr: { id: 'btnRestoreMaster' },
                        onClick: function () {
                            restoreMaster();
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxDropDownButton',
                    visible: !isRiskion,
                    options: {
                        disabled: !has_sel,
                        icon: "download",
                        text: resString("btnDownload"),
                        elementAttr: { id: 'btnDownload' },
                        displayExpr: "text",
                        useSelectMode: false,
                        wrapItemText: false,
                        dropDownOptions: {
                            width: 250,
                        },
                        items: [],
                        onButtonClick: function (e) {
                            var selected = getSelected();
                            var single = (selected.length == 1);
                            var items = [{
                                text: resString(single ? "lblDownloadAsAHPS" : "lblDownloadAsZipAHPS"),
                                icon: "fas fa-arrow-alt-circle-down",
                                onClick: function () {
                                    if (single) {
                                        onDownload(selected);
                                    } else {
                                        onDownloadFiles(selected, false, "ahps");
                                    }
                                }
                            }, {
                                text: resString(single ? "lblDownloadAsAHPZ" : "lblDownloadAsZipAHP"),
                                icon: "far fa-arrow-alt-circle-down",
                                onClick: function () {
                                    if (single) {
                                        onDownloadFiles(selected, false, "ahp&zip=yes&ext=ahpz");
                                    } else {
                                        onDownloadFiles(selected, false, "ahp&zip=yes&ext=ahpz");
                                    }
                                }
                            }];
                            e.component.option('items', items);
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: isRiskion,
                    options: {
                        disabled: !has_sel,
                        icon: "download",
                        text: resString("btnDownload"),
                        elementAttr: { id: 'btnDownloadRiskion' },
                        onClick: function () {
                            var selected = getSelected();
                            var single = (selected.length == 1);
                            if (single) {
                                onDownload(selected);
                            } else {
                                onDownloadFiles(selected, false, "ahps");
                            }
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    options: {
                        disabled: !has_sel,
                        text: resString("btnDelete"),
                        icon: 'trash',
                        elementAttr: { id: 'btnDelete' },
                        onClick: function () {
                            onDelete(getSelected());
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: is_deleted(),
                    options: {
                        disabled: !has_sel,
                        text: resString("mnuProjectUnDelete"),
                        icon: 'fa fa-undo',
                        elementAttr: { id: 'btnUnDelete' },
                        onClick: function () {
                            onUnDelete(getSelected());
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: (!is_deleted() && !is_master),
                    options: {
                        disabled: !has_sel,
                        text: resString(pgID == <% =_PGID_PROJECTSLIST_ARCHIVED %> ? "lblDoActiveProject" : "btnArchive"),
                        icon: 'fa fa-archive',
                        elementAttr: { id: 'btnArchive' },
                        onClick: function () {
                            onArchive(getSelected());
                        }
                    }
                }]
        });
        toolbar = $("#toolbar").dxToolbar("instance");
        <% End If %>
        }

        function updateGridButtons() {
            if (hasGrid()) {
                $("#divGridOptions").dxButtonGroup("option", "selectedItemKeys", getSelectedGridOptions());
            }
        }

        function updateToolbarButtons() {
            if (hasGrid() && (toolbar)) {
                var sel = getSelected().length;
                var has_sel = (sel > 0);
            <% If CanEditAtLeastOneModel Then %>
            if ($("#btnDownload").data("dxDropDownButton")) $("#btnDownload").dxDropDownButton("instance").option("disabled", !has_sel);
            if ($("#btnDownloadRiskion").data("dxButton")) $("#btnDownloadRiskion").dxButton("instance").option("disabled", !has_sel);
            if ($("#btnArchive").data("dxButton")) $("#btnArchive").dxButton("instance").option("disabled", !has_sel);
            if ($("#btnDelete").data("dxButton")) $("#btnDelete").dxButton("instance").option("disabled", !has_sel);
            if ($("#btnUnDelete").data("dxButton")) $("#btnUnDelete").dxButton("instance").option("disabled", !has_sel);<% End If %>
            <% If CanCreateNewModels Then %>
            if ($("#btnCreate").data("dxButton")) $("#btnCreate").dxButton("instance").option("disabled", !projects_loaded || !can_create);
            if ($("#btnCreateFromTemplate").data("dxButton")) $("#btnCreateFromTemplate").dxButton("instance").option("disabled", (!projects_loaded || !can_create || template_projects.length == 0));
            if ($("#btnUpload").data("dxButton")) $("#btnUpload").dxButton("instance").option("disabled", !projects_loaded || !can_create);<% End If %>
                updateGridButtons();
                setTimeout(function () {
                    showProjectsCounter(sel);
                }, 50);
            }
        }

        function getProjectType(ptype) {
            var res = "";
            switch (ptype) {
                case <% =CInt(ProjectType.ptOpportunities) %>:
                res = "<% =JS_SafeString(ResString("lblOpportunityModel")) %>";
                break;
            case <% =CInt(ProjectType.ptMixed) %>:
                res = "<% =JS_SafeString(ResString("lblMixedModel")) %>";
                break;
            case <% =CInt(ProjectType.ptMyRiskReward) %>:
                res = "<% =JS_SafeString(ResString("lblMyRiskRewardModel")) %>";
                break;
            case <% =CInt(ProjectType.ptRiskAssociated) %>:
                res = "<% =JS_SafeString(ResString("lblAssociatedRiskModel")) %>";
                break;
            case <% =CInt(ProjectType.ptRegular) %>:
            default:
                res = "<% =JS_SafeString(ResString(If(App.isRiskEnabled, "lblRiskModel", "lblRegularModel"))) %>";
            }
            return res;
        }

        function getProjectNameTitle() {
            var n = "lblProjectName";
            switch (projectStatusInt() * 1) {
                case <% = CInt(ecProjectStatus.psArchived) %>:
                n = "lblArchiveName";
                break;
            case <% = CInt(ecProjectStatus.psTemplate) %>:
                n = "lblTemplateName";
                break;
            case <% = CInt(ecProjectStatus.psMasterProject) %>:
                    n = "lblMasterProjectName";
                    break;
            }
            return resString(n);
        }

        function showDateTime(element, info) {
            if (typeof info.value != "undefined" && info.value != null) {
                element.html(new Date(info.value).toLocaleString("en-US"));
            }
        };

        function getProjectNameDiv(data, div) {
            var st = "";
            if (curPrjID == data.ID) st = "font-weight:bold;";
            if (db_version != data.Version) st = "color:#9999cc;";
            var has_link = false;
            var canOpen = data.CanOpen;
            var is_new_ver = (db_version < data.Version);
            if (is_new_ver) {
                //canOpen = false;
                st = "color:#cc9999c;";
            }
            div.prop("title", "#" + data.ID + "<br><% = JS_SafeString(ResString("lblVersionPlain")) %>: " + data.Version + (db_version == data.Version ? "" : " (" + (db_version > data.Version ? "<% =JS_SafeString(ResString("lblExpected")) %>" : "<% =JS_SafeString(ResString("lblExpectedIsNewer")) %>") + " " + db_version + ")") + (false && data.CanModify ? "<br><br><i><% = JS_SafeString(ResString("lblDblClickEditPrjName")) %></i>" : ""));
        if (canOpen) {
            if (!data.isMarkedAsDeleted && (data.CanModify || data.isOnline || data.CanView) && !is_new_ver) {
                var s = $("<a href='' onclick='openProject(" + data.ID + "); return false;' class='actions'" + (st != "" ? " style='" + st + "'" : "") + " id='lblProjectName" + data.ID + "'>" + htmlEscape(data.Name) + "</a>").appendTo(div);
                //element.prop("id", "tdProjectName" + data.ID);
                has_link = true;
            }
            $("<div class='dx-button-small' style='position: absolute; right:0px; top:-3px;' id='btnPrjExtra" + data.ID + "'></div>").dxButton({
                icon: "spindown",
                visible: (data.CanModify || (data.CanView && data.CanEvaluate) || curPrjID == data.ID),
                //visible: false, //data.CanModify, 
                showEvent: 'dxclick',
                onClick: function () {
                    onShowSubmenu($("#context-menu"), data.ID, getContextMenuItems(data), "btnPrjExtra" + data.ID);
                    //}, 
                    //elementAttr: { 
                    //    "id":"btnPrjExtra" + data.ID 
                }
            }).appendTo(div);
            if (curPrjID == data.ID) {
                $("<div class='dx-button-small' style='position: absolute; right:25px; top:-3px;' id='btnPrjClose" + data.ID + "'></div>").dxButton({
                    icon: "far fa-eye-slash",
                    visible: true,
                    showEvent: 'dxclick',
                    hint: "<% = JS_SafeString(ResString("btnCloseProject")) %>",
                    onClick: function () {
                        closeProject(data.ID);
                    }
                }).appendTo(div);
                }
            }
            if (!has_link) {
                $("<span class='gray'>" + htmlEscape(data.Name) + "</span>").appendTo(div);
            }
            return div;
        }

        function getColumns() {
            //if (hasGrid()) grid.hideColumnChooser();
            return [
                <%If App.ActiveWorkgroup IsNot Nothing AndAlso isStarredAvailable() Then%>, {
                dataField: "Starred",
                caption: String.fromCharCode(9733),
                dataType: "boolean",
                alignment: "center",
                width: 55,
                minWidth: 50,
                visible: !is_deleted(),
                visibleIndex: 1,
                //cssClass: "on_advanced",
                fixed: true,
                fixedPosition: "left",
                allowResizing: false,
                allowHeaderFiltering: true,
                allowFiltering: false,
                allowSearch: false,
                allowEditing: false,
                showInColumnChooser: !is_deleted(),
                headerFilter: {
                    dataSource: [{
                        text: String.fromCharCode(9734),    //"<i class='ion ion-md-star-outline'></i>",
                        value: false
                    }, {
                        text: String.fromCharCode(9733),    //"<i class='ion ion-md-star'></i>",
                        value: true
                    }]
                },
                cellTemplate: function (element, info) {
                    element.append('<i class="starred-mark fa-star ' + (info.data.Starred ? "fas project-starred" : "far") + '" id="starred' + info.data.ID + '" onclick="onToggleStarred(' + info.data.ID + ', this, false); return false;"></i>');
                }
            },<% End If %>
            //{
            //    dataField: "ID",
            //    dataType: "number",
            //    caption: "#",
            //    visibleIndex: 2,
            //    width: 70,
            //    fixed: true,
            //    fixedPosition: "left",
            //    allowHiding: true,
            //    allowSearch: true,
            //    allowEditing: false,
            //    showInColumnChooser: true,
            //    hidingPriority: (!grid_auto_collapse ? null : 7),
            //    visible: false,
            //    allowHeaderFiltering: true,
            //    allowFiltering: true,
            //}, 
            {
                dataField: "Name",
                caption: getProjectNameTitle(),
                visibleIndex: 3,
                allowGrouping: false,
                fixed: true,
                fixedPosition: "left",
                //allowResizing: true,
                allowHiding: false,
                allowSearch: true,
                showInColumnChooser: false,
                allowHeaderFiltering: true,
                allowFiltering: true,
                allowEditing: false,
                minWidth: 200,<% if CanEditAtLeastOneModel Then %>
                    //width:400,<% End If %>
                cellTemplate: function (element, info) {
                    var data = info.data;
                    var div = $("<div style='position:relative; white-space:nowrap;' id='prjTitle" + data.ID + "'></div>").appendTo(element);
                    getProjectNameDiv(data, div);
                }
            }, {
                dataField: "TeamTimeStatus",
                caption: resString("tblAction"),
                visibleIndex: 4,
                visible: false, // !is_deleted(),
                fixed: true,
                fixedPosition: "left",
                alignment: "center",
                hidingPriority: (!grid_auto_collapse ? null : 90),
                minWidth: 150,
                width: 150,
                alignment: "center",
                allowGrouping: false,
                showInColumnChooser: false,
                //allowResizing: false,
                allowHeaderFiltering: false,
                allowFiltering: false,
                allowSorting: false,
                allowExporting: false,
                allowSearch: false,
                allowEditing: false,
                cellTemplate: function (container, options) {
                    var d = options.data;
                    var name = resString("btnView");
                    var active = d.CanOpen;
                    if (curPrjID == d.ID) {
                        name = resString("btnClose");
                    } else {
                        var is_master = d.ProjectStatus == "<% =ecProjectStatus.psMasterProject %>";
                        if (d.CanOpen || is_master || d.TeamTimeStatus >= 0) {
                            if (d.CanModify) {
                                name = resString(is_master ? "btnEdit" : "btnOpen");
                            } else {
                                name = resString(d.isOnline ? "btnEvaluate" : "lblOffline");
                                if (!d.isOnline) active = false;
                            }
                        }
                    }

                    $("<div></div>").dxButton({
                        text: name,
                        width: "100px",
                        disabled: !active,
                        onClick: function () {
                            if (curPrjID == d.ID) closeProject(d.ID); else openProject(d.ID);
                        }
                    }).appendTo(container);
                    $("<div></div>").dxButton({
                        icon: "spindown",
                        disabled: !active,
                        visible: d.CanModify,
                        showEvent: 'dxclick',
                        onClick: function () {
                            onShowSubmenu($("#context-menu"), d.ID, getContextMenuItems(d), "btnPrjExtra" + d.ID);
                        },
                        elementAttr: {
                            "style": "margin-left:4px",
                            "id": "btnPrjExtra" + d.ID
                        }
                    }).appendTo(container);
                }
            }, {
                dataField: (is_deleted() ? "ModifyDate" : "LastAccessDate"),
                caption: resString(is_deleted() ? "tblDeletedAt" : "tblLastAccess"),
                width: 160,
                minWidth: 140,
                allowSearch: false,
                allowEditing: false,
                hidingPriority: (!grid_auto_collapse ? null : 50),
                sortIndex: 0,
                sortOrder: 'desc',
                visible: <% =Bool2JS(CanEditAtLeastOneModel) %>,
                dataType: "date",
                cellTemplate: showDateTime,
            }, {
                dataField: "isOnline",
                caption: resString("tblProjectStatus"),
                width: 100,
                minWidth: 75,
                alignment: "center",
                allowHeaderFiltering: true,
                allowFiltering: false,
                allowSearch: false,
                allowEditing: false,
                //allowResizing: false,
                visible: pgID == <% = CInt(_PGID_PROJECTSLIST) %>,
                    showInColumnChooser: pgID == <% = CInt(_PGID_PROJECTSLIST) %>,
                    hidingPriority: (!grid_auto_collapse ? null : 45),
                    headerFilter: {
                        dataSource: [{
                            text: resString("lblOffline"),
                            value: false
                        }, {
                            text: resString("lblOnline"),
                            value: true
                        }]
                    },
                    cellTemplate: function (container, options) {
                        var d = options.data;
                        <% If CanEditAtLeastOneModel Then %>$('<label class="ec_switch small_element" style="position:relative; top:3px;" title="' + resString(d.isOnline ? "lblOnline" : "lblOffline") + '"><input id="cbOnline' + d.ID + '" type="checkbox"' + (d.isOnline ? ' checked="checked"' : '') + ' onclick="setProjectOnline(' + d.ID + ', this.checked, onRefresh);"' + (d.CanModify && d.TeamTimeStatus <= 0 ? '' : ' disabled="disabled"') + '><span class="ec_slider small green"></span></label>').appendTo(container);<% Else %>if (d.isOnline) $('<i class="icon-light fas fa-check"></i>').appendTo(container); <% End if %>
                }
                //}, {
                //    dataField: "HasJudgments",
                //    caption: "Has Judgments",
                //    width: 90,
                //    hidingPriority: (!grid_auto_collapse ? null : 30),
            }, {
                dataField: "ComplexStatus",
                caption: "<% = JS_SafeString(ResString("lblStatus")) %>",
                    alignment: "center",
                    hidingPriority: (!grid_auto_collapse ? null : 40),
                    minWidth: 100,
                    width: 155,
                    visible: pgID == <% = Cint(_PGID_PROJECTSLIST) %>,
                    showInColumnChooser: pgID == <% = CInt(_PGID_PROJECTSLIST) %>,
                    allowGrouping: false,
                    allowHeaderFiltering: false,
                    allowFiltering: false,
                    allowSorting: false,
                    allowExporting: true,
                    allowSearch: false,
                    allowEditing: false,
                    cellTemplate: function (element, info) {
                        var s = "";
                        //if ((info.data.LockStatus) && (info.data.LockExpires) > Date.now()) {
                        if ((info.data.LockStatus != "") && (info.data.LockStatus !="<% =CInt(ECLockStatus.lsUnLocked)  %>")) {
                            var pm = (info.data.LockedBy == "" ? "<% =JS_SafeString(ResString("lblProjectAccessTTAnother")) %>" : "<a href='mailto:" + info.data.LockedBy + "' class='actions'>" + info.data.LockedBy + "</a>");
                            var e = "";
                            switch (info.data.LockStatus) {
                                case <% =CInt(ECLockStatus.lsLockForModify) %>:
                                    if (info.data.LockedUserID != <% =App.ActiveUser.UserID %>) {
                                        s = replaceString("{0}", pm, "<% =JS_SafeString(ResString("lblProjectAccessLocked")) %>");
                                    } else {
                                        s = "<% =JS_SafeString(ResString("lblProjectAccessLockedByMe")) %>";
                                        if (info.data.CanModify) s += "<div><a href='' onclick='setLockProject(" + info.data.ID +", 0); return false;' class='dashed actions'><% =JS_SafeString(ResString("mnuProjectUnlock")) %></a></div>";
                                    }
                                    break;
                                case <% =(ECLockStatus.lsLockForTeamTime) %>:
                                    if (info.data.LockedUserID != <% =App.ActiveUser.UserID %>) {
                                        s = replaceString("{0}", pm, "<% =JS_SafeString(ResString("lblProjectAccessTTOther")) %>");
                                        if (info.data.CanEvaluate) e += "<nobr><a href='' onclick='joinTTSession(" + info.data.ID +"); return false;' class='dashed actions'><% =JS_SafeString(ResString("btnJoin")) %></a></nobr>";
                                    } else {
                                        s = "<% =JS_SafeString(ResString("lblProjectAccessTTMy")) %>";
                                        if (info.data.CanEvaluate) e += "<nobr><a href='' onclick='resumeTTSession(" + info.data.ID +"); return false;' class='dashed actions'><% =JS_SafeString(ResString("btnContinue")) %></a></nobr>";
                                    }
                                    if (info.data.CanModify) e += (e == "" ? "" : "&nbsp;") + " <nobr><a href='' onclick='stopTTSession(" + info.data.ID +"); return false;' class='dashed actions'><% =JS_SafeString(ResString("btnTTStopSession")) %></a></nobr>";
                                    if (e != "") s += "<div>" + e + "</div>";
                                    break;
                                case <% =CInt(ECLockStatus.lsLockForSystem) %>:
                                    s = "<% =JS_SafeString(ResString("lblProjectAccessLockedSystem")) %>";
                                    break;
                                case <% =CInt(ECLockStatus.lsLockForAntigua) %>:
                                    if (info.data.LockedUserID != <% =App.ActiveUser.UserID %>) {
                                        s = replaceString("{0}", pm, "<% =JS_SafeString(ResString("lblProjectAccessLockedAntiguaOther")) %>");
                                    } else {
                                        s = "<% =JS_SafeString(ResString("lblProjectAccessLockedAntigua")) %>";
                                    }
                                    break;
                            }
                        }
                        if (s != "") {
                            s = "<div class='small text' style='margin-top:3px; white-space: normal'>" + s + "</div>";
                        } else {
                            if (info.data.isMarkedAsDeleted) {
                                s = "<span>" + resString("lblMarkedAsDeleted") + "</span>";
                            } else {
                                if (info.data.ProjectStatus == "<% =ecProjectStatus.psActive %>") {
                                    s = "<span>" + resString(info.data.isOnline && info.data.CanOpen ? "lblProjectAccessOnline" : (info.data.CanModify ? "lblProjectAccessOffline" : "lblProjectAccessDisabled")) + "</span>";
                                } else {
                                    var n = "";
                                    switch (info.data.ProjectStatus + "") {
                                        case "<% =ecProjectStatus.psArchived %>":
                                            n = "<% =ecProjectStatus.psArchived.ToString %>";
                                            break;
                                        case "<% =ecProjectStatus.psTemplate %>":
                                            n = "<% =ecProjectStatus.psTemplate.ToString %>";
                                            break;
                                        case "<% =ecProjectStatus.psMasterProject %>":
                                            n = "<% =ecProjectStatus.psMasterProject.ToString %>";
                                            break;
                                    }
                                    if (n != "") s = "<span>" + resString("lbl_" + n) + "</span>";
                                }
                            }
                        }
                        if (info.data.OnlineUsers > 0 && info.data.CanModify) {
                            s += "<div class='small text' style='margin-top:3px; white-space: normal'><nobr><% = JS_SafeString(ResString("lblOnlineUsers")) %>: <a href='' onclick='showOnlineUsers(" + info.data.ID + "); return false;' class='dashed actions'><span id='online" + info.data.ID + "'>" + info.data.OnlineUsers + "</span></nobr></a></div>";
                        }
                        if (s != "") $(s).appendTo(element);
                    }
                }<% If CanEditAtLeastOneModel Then %>, {
                dataField: "ProjectType",
                caption: resString("tblProjectType"),
                alignment: "center",
                visible: isAdvancedMode,
                cssClass: "on_advanced",
                width: 130,
                allowHeaderFiltering: true,
                allowFiltering: false,
                allowSearch: false,
                allowEditing: false,
                customizeText: function (cellInfo) {
                    return getProjectType(cellInfo.value);
                },
                cellTemplate: function (element, info) {
                    var s = "<span>" + getProjectType(info.data.ProjectType) + "</span>";
                    $(s).appendTo(element);
                },
                hidingPriority: (!grid_auto_collapse ? null : 35)
            }, {
                dataField: "UserRole",
                caption: resString("tblYourRole"),
                alignment: "center",
                width: 140,
                visible: false && !is_deleted(),
                allowHeaderFiltering: true,
                allowFiltering: false,
                allowSearch: false,
                allowEditing: false,
                showInColumnChooser: pgID == <% = CInt(_PGID_PROJECTSLIST) %>,
                hidingPriority: (!grid_auto_collapse ? null : 20)
            }, {
                dataField: "HasSurveys",
                caption: resString("tblHasSurvey"),
                alignment: "center",
                width: 120,
                visible: false,
                allowHeaderFiltering: true,
                allowFiltering: false,
                allowSearch: false,
                allowEditing: false,
                hidingPriority: (!grid_auto_collapse ? null : 10),
                headerFilter: {
                    dataSource: [{
                        text: resString("lblNoSurveys"),
                        value: ["HasSurveys", "=", ""]
                    }, {
                        text: resString("lblSurveyBeginning"),
                        value: ["HasSurveys", "contains", "LW"]
                    }, {
                        text: resString("lblSurveyBeginningOnly"),
                        value: ["HasSurveys", "=", "LW"]
                    }, {
                        text: resString("lblSurveyEnd"),
                        value: ["HasSurveys", "contains", "LT"]
                    }, {
                        text: resString("lblSurveyEndOnly"),
                        value: ["HasSurveys", "=", "LT"]
                    }, {
                        text: resString("lblAllSurveys"),
                        value: [["HasSurveys", "contains", "LT"], "and", ["HasSurveys", "contains", "LW"]]
                    }]
                },
                encodeHtml: false,
                customizeText: function (cellInfo) {
                    return (typeof cellInfo.value == "undefined" || cellInfo.value == "" ? "" : (cellInfo.value.indexOf("LW") >= 0 ? resString("lblSurveyBeginning") + "<br>" : "") + (cellInfo.value.indexOf("LT") >= 0 ? resString("lblSurveyEnd") : ""));
                },
            }, {
                dataField: "OwnerName",
                caption: resString("tblProjectOwner"),
                width: 145,
                alignment: "center",
                encodeHtml: false,
                allowEditing: false,
                visible: (isAdvancedMode && pgID == <% = CInt(_PGID_PROJECTSLIST) %>),
                    cssClass: "on_advanced",
                    showInColumnChooser: (pgID != <% = CInt(_PGID_PROJECTSLIST_TEMPLATES) %> && pgID != <% = CInt(_PGID_PROJECTSLIST_MASTERPROJECTS) %>),
                hidingPriority: (!grid_auto_collapse ? null : 25)
            }, {
                dataField: "CreationDate",
                caption: resString("lblCreated"),
                width: 160,
                minWidth: 120,
                visible: false,
                allowSearch: false,
                hidingPriority: (!grid_auto_collapse ? null : 5),
                dataType: "date",
                cellTemplate: showDateTime,
            }, {
                dataField: "ModifyDate",
                caption: resString("lblLastModified"),
                width: 160,
                minWidth: 120,
                visible: false,
                showInColumnChooser: !is_deleted(),
                allowSearch: false,
                allowEditing: false,
                hidingPriority: (!grid_auto_collapse ? null : 15),
                dataType: "date",
                cellTemplate: showDateTime,
            }<% End If %>];
        }

        function getFilter() {
            return (is_deleted() ? ['isMarkedAsDeleted', '=', true] : [['isMarkedAsDeleted', '=', false], "and", ['ProjectStatus', '=', projectStatusInt()]]);  // <% If Not CanEditAtLeastOneModel Then %>, "and", ['isOnline', '=', true]<% End If %>
        }

        function getSessName() {
            return "PrjList_<% =Bool2Num(CanEditAtLeastOneModel) %>_" + (isAdvancedMode ? "a" : "b") + "_" + (is_deleted() ? "del" : projectStatusInt());
        }

        function hasGrid() {
            return ((grid) && $("#grid").html() != "");
        }

        function updateGridPagination() {
            if ((projects) && (grid)) {
                //var pc = grid.pageCount();
                //var ps = grid.option("paging.pageSize");
                //var max = 10;
                //if (grid_row_height>10) {
                //    max = Math.floor($("#grid").height()/grid_row_height - 3.3 - ((grid.option("filterRow.visible")) ? 0.85 : 0));
                //}
                //var cnt = grid.getVisibleRows().length; //projects.length;
                //var show = (projects.length>5 && !grid_vscrolling && !grid_autopager && (cnt>=max || (ps>max)));
                //grid.option("pager.showPageSizeSelector", show);
                //grid.option("pager.visible", show || ((can_create && (projects_max>0 || lifetime_max>0)) || pc>1));

                grid.option("pager.showPageSizeSelector", !grid_vscrolling && !grid_autopager);
            }
        }

        function initGrid() {
            if (hasGrid()) {
                $("#grid").dxDataGrid("dispose");
                $("#grid").html("");
            }
            var grid_columns = getColumns();
            $("#grid").dxDataGrid({
                dataSource: {
                    key: "ID",
                    store: PrjDataSource,
                    reshapeOnPush: true,
                    pushAggregationTimeout: 30,
                    filter: getFilter()
                },
                repaintChangesOnly: false,
                highlightChanges: true,
                //focusedRowEnabled: true,
                //focusedRowKey: curPrjID,
                stateStoring: {
                    enabled: true,
                    type: "localStorage",   //"sessionStorage",
                    //savingTimeout: 500,
                    storageKey: getSessName()
                },
                width: "100%",
                height: "99%",
                rowAlternationEnabled: true,
                columns: grid_columns,
                columnResizingMode: "widget",
                remoteOperations: {
                    sorting: false,
                    filtering: false,   // AD: doesn't work on server side
                    paging: false
                },
                pager: {
                    allowedPageSizes: [10, 15, 20, 30, 50, 100],
                    showInfo: true,
                //infoText: "<% = JS_SafeString(ResString("lblProjects")) %>: {2}. <% = JS_SafeString(ResString("lblPage")) %> #{0} <% = JS_SafeString(ResString("lblOf")) %> {1}",
                infoText: "<% = JS_SafeString(ResString("lblPage")) %> #{0} <% = JS_SafeString(ResString("lblOf")) %> {1}",
                showNavigationButtons: true,
                showPageSizeSelector: !grid_vscrolling && !grid_autopager,
                visible: !grid_vscrolling
            },
            paging: {
                enabled: true,
                pageSize: (grid_vscrolling ? 20 : 50),
            },
            scrolling: {
                showScrollbar: "always",
                mode: (grid_vscrolling ? "virtual" : "standard"),
                useNative: true
            },
            preloadEnabled: true,
            //rowRenderingMode: 'virtual',
            //columnRenderingMode: 'virtual',
            filterRow: {
                visible: isAdvancedMode && grid_filtering,
                applyFilter: "auto"
            },
            hoverStateEnabled: true,
            columnHidingEnabled: isAdvancedMode && grid_auto_collapse,
            columnAutoWidth: true,
            allowColumnResizing: true,
            allowColumnReordering: true,
            searchPanel: {
                visible: true,
                width: 240,
                searchVisibleColumnsOnly: true,
                placeholder: resString("btnDoSearch") + "..."
            },
            headerFilter: {
                allowSearch: true,
                visible: true
            },
            showBorders: false,
            //cacheEnabled: true,
            "export": {
                enabled: isAdvancedMode && <% =Bool2JS(CanCreateNewModels) %>,
                allowExportSelectedData: true,
                excelFilterEnabled: true,
                fileName: "<% =JS_SafeString(String.Format("{1} - {0}", App.ActiveWorkgroup.Name, PageMenuItem(CurrentPageID))) %>"
            },
            grouping: {
                //expandMode: "rowClick",
                contextMenuEnabled: isAdvancedMode
            },
            groupPanel: {
                //visible: (isAdvancedMode ? "auto" : false)
                visible: "auto"
            },
            columnFixing: {
                enabled: true
            },
            selection: {
                mode: (can_modify ? "multiple" : "none"),
                allowSelectAll: can_modify,
                showCheckBoxesMode: "always"
            },
            columnChooser: {
                enabled: !is_deleted(),
                mode: "select", //"dragAndDrop"
                title: resString("lblColumnChooser"),
                height: 400,
                width: 250,
                emptyPanelText: resString("lblColumnChooserPlace")
            },
            //editing: {
            //    mode: 'cell', 
            //    allowUpdating: true,
            //    allowAdding: false,
            //    allowDeleting: false,
            //    selectTextOnEditStart: true,
            //    startEditAction: "dblClick",
            //    useIcons: true
            //},
            //onCellDblClick: function (e) {
            //    if ((e.data) && e.row.rowType == "data" && e.column.dataField === "Name" && (e.data.CanModify)) {
            //        e.component.editCell(e.rowIndex, "Name");
            //    }
            //},
            //onEditorPreparing: function (e) {
            //},
            noDataText: function () {
                if (hasFilter()) {  // have an extra filtering at header
                    return resString("errNoSearch");
                } else {
                    var s = getSearchValue();
                    if (s != "") {
                        return resString("errNoSearch") + " (" + htmlEscape(s) + ")";
                    } else {
                        if (pgID == _pgid_projectslist) {
                            setTimeout(function () {
                                var d = $(".dx-datagrid-nodata");
                                if ((d) && (d.length)) {
                                    d.css("z-index", 10);
                                    d.parent().find(".dx-scrollable-wrapper").hide();
                                    d.html("<div class='tdRoundBox'><h6 style='margin:1em'><% = JS_SafeString(ResString("lblNoProjects")) %></h6><% If CanEditAtLeastOneModel Then %><div id='btnStartProject'></div><br><br><div id='btnDownloadSample'></div></div><% End if %>");
                                    <% If CanEditAtLeastOneModel Then %>
                                    $("#btnStartProject").dxButton({
                                        text: "<% =JS_SafeString(ResString("btnCreatingProject")) %>",
                                        icon: "far fa-hand-point-right",
                                        type: "success",
                                        width: "250px",
                                        onClick: function (e) {
                                            startCreatingProject();
                                            return false;
                                        }
                                    }).focus();
                                    $("#btnDownloadSample").dxButton({
                                        text: "<% =JS_SafeString(ResString("btnDownloadSamples")) %>",
                                        icon: "fas fa-download",
                                        type: "normal",
                                        width: "250px",
                                        visible: (!isRiskion && pageByID(nav_json, <% =_PGID_RESOURCE_CENTER %>)),
                                        onClick: function (e) {
                                            onResourceCenter("tab=project");
                                            return false;
                                        }
                                    });<% End If %>
                                }
                            }, 100);
                        }
                        return resString("msgNoProjectsInGrid");
                    }
                }
            },
            onContentReady: function (e) {
                e.component.option("loadPanel.enabled", false);
                var g = $("#grid");
                //onResize(true);
                if (grid_row_height <= 0) {
                    var tr = $("#trDefRow");
                    if ((tr) && (tr.length) && (g) && (g.length)) {
                        if (g.height() > 15 && g.height() > tr.height()) {
                            grid_row_height = Math.ceil(tr.height() + 2);
                            onSetAutoPager();
                        }
                    }
                }
                hideLoadingPanel();
                //if (!(first_load)) { 
                //    setTimeout(function () {
                //        onResize(true);
                //    }, 2000);
                //}
                hideBackgroundMessage();
                //setTimeout(function () { updateToolbarButtons(); }, 100);
                if ((do_flash_advanced) && typeof flashAdvancedOptions == "function") {
                    setTimeout(function () {
                        flashAdvancedOptions(true);
                    }, 2000);
                    setTimeout(function () {
                        flashAdvancedOptions(false);
                    }, 6100);
                    do_flash_advanced = false;
                }
            },
            onInitialized: function (e) {
                setTimeout(function () { onResize(true); }, 300);
                if (first_load) {
                    setTimeout(function () { initSearchInput(); }, 200);
                    first_load = false;
                }
            },
            onSelectionChanged: function (e) {
                //var disabledKeys = [];
                //var selected = grid.getSelectedRowsData();
                //for (var i=0; i<selected.length; i++) {
                //    if (!selected[i].CanModify) disabledKeys.push(selected[i].ID);
                //}
                //if (disabledKeys.length > 0) e.component.deselectRows(disabledKeys);    
                updateToolbarButtons();
                //e.component.collapseAll(-1);
                //if (e.currentSelectedRowKeys.length == 1) e.component.expandRow(e.currentSelectedRowKeys[0]);
            },
            onRowPrepared: function (e) {
                if ((grid_row_height <= 0) && (e.rowType == "data") && (e.rowIndex == 1)) {
                    var o = e.rowElement;
                    if ((o)) o.prop("id", "trDefRow");
                }
                if (curPrjID > 0 && (e.rowType == "data") && (e.data) && (e.data.ID == curPrjID)) {
                    var o = e.rowElement;
                    if ((o)) o.addClass("dx-row-cur-prj");
                }
            },
            onCellPrepared: function (e) {
                if (e.rowType === "data" && e.column.command === 'select' && e.data) {
                    if (!e.data.CanModify) {
                        var instance = e.cellElement.find('.dx-select-checkbox').dxCheckBox("instance");
                        instance.option("visible", false);
                        e.cellElement.off();
                    }
                }
            },
            //onOptionChanged: function (e) {
            //    var sel = getSelected().length;
            //    showProjectsCounter(sel); 
            //    setTimeout(function() { 
            //        showProjectsCounter(sel); 
            //    }, 150);
            //},
            //onCellPrepared: function (e) {
            //    if ((e.data) && (e.rowType == "data") && (e.columnIndex == 1)) {
            //        var btn = e.cellElement.find(".dx-button-small");
            //        if ((btn) && (btn.length)) {
            //            btns.push(btn);
            //            //btn.hide();
            //        }
            //    }
            //},
            //onCellHoverChanged:  function (e) {
            //    if ((e.data)) {
            //        var d = e.data;
            //        var btn = e.cellElement.find(".dx-button-small");
            //        if ((btn) && (btn.length)) {
            //            if (e.eventType=="mouseover") btn.show(); else btn.hide();
            //        }
            //    }
            //},
            //loadPanel: {
            //    enabled: true,
            //},
            loadPanel: main_loadPanel_options,
            onContextMenuPreparing: onGridCellContextMenu,
            onToolbarPreparing: onCreateGridToolbar
        });

            grid = $("#grid").dxDataGrid('instance');
            updateGridPagination();
        }

        function onSetAutoPager() {
            updateGridPagination();
            var g = $("#grid");
            if (grid_autopager && !grid_vscrolling && (g) && (g.length) && grid_row_height > 10 && hasGrid()) {
                var cnt = Math.floor(g.height() / grid_row_height - 3.3 - ((grid.option("filterRow.visible")) ? 0.85 : 0));
                if (cnt < 5) cnt = 5;
                var c = grid.option("paging.pageSize");
                if (c != cnt) {
                    setTimeout(function () { grid.option("paging.pageSize", cnt); }, 100);
                }
            }
        }

        function startCreatingProject() {
            $("#popupContainer").dxPopup({
                contentTemplate: function () {
                    return $("<div style='text-align:center; line-height:38px;'><div id='btnCreateProjectDlg' class='button_enter'></div><br><div id='btnProjectFromTplDlg' style='margin:6px'></div><br><div id='btnUploadProjectDlg'></div><br><div id='btnCancelDlg' style='margin-top:1em' class='button_esc'></div></div>");
                },
                width: 325,
                height: 225,
                showTitle: true,
                title: "<% =JS_SafeString(ResString("lblCreateProject")) %>",
            dragEnabled: true,
            shading: true,
            closeOnOutsideClick: true,
            resizeEnabled: false,
            //position: { my: 'center', at: 'center', of: window },
            showCloseButton: true,
        });
        $("#popupContainer").dxPopup("show");
        $("#btnCreateProjectDlg").dxButton({
            text: "<% =JS_SafeString(ResString("btnCreateNewProject")) %>",
            icon: "far fa-hand-point-right",
            type: "success",
            width: "250px",
            disabled: !can_create,
            onClick: function (e) {
                createProject("<% = JS_SafeString(ResString("titleProjectCreate")) %>", "<% = JS_SafeString(ResString("lblBasedOn")) %>", master_projects, "", "", -1, startCreatingProject, false);
                return false;
            }
        }).focus();
        $("#btnProjectFromTplDlg").dxButton({
            text: "<% =JS_SafeString(ResString("btnProjectFromTpl")) %>",
            icon: "fas fa-file",
            type: "normal",
            width: "250px",
            disabled: !can_create || !template_projects.length,
            onClick: function (e) {
                createProject("<% = JS_SafeString(ResString("btnCreateFromTemplate")) %>", "<% = JS_SafeString(ResString("lblProjectTemplate")) %>", template_projects, "", "", -1, startCreatingProject, false);
                return false;
            }
        });
        $("#btnUploadProjectDlg").dxButton({
            text: "<% =JS_SafeString(ResString("btnUploadExisting")) %>",
            icon: "fas fa-upload",
            type: "normal",
            width: "250px",
            disabled: !can_create,
            onClick: function (e) {
                closePopup();
                uploadProject(startCreatingProject);
                return false;
            }
        });
        $("#btnCancelDlg").dxButton({
            text: "<% =JS_SafeString(ResString("btnCancel")) %>",
            icon: "",
            type: "default",
            width: "100px",
            onClick: function (e) {
                closePopup();
                return false;
            }
        });
        }

        function getSelectedGridOptions() {
            var opt = [];
            if (grid_autopager) opt.push("autopgsize");
            if (grid_vscrolling) opt.push("infscroll");
            if (grid_filtering) opt.push("filters");
            if (grid_auto_collapse) opt.push("autohide");
            return opt;
        }

        function onCreateGridToolbar(e) {
            var toolbarItems = e.toolbarOptions.items;

            $.each(toolbarItems, function (_, item) {
                //"groupPanel"
                if (item.name == "searchPanel") {
                }
                if (item.name == "exportButton") {
                    item.visible = isAdvancedMode;
                    item.showText = "inMenu";
                    item.text = resString("btnExport");
                    item.hint = item.text;
                    template: "content";
                }
                if (item.name == "columnChooserButton") {
                    item.showText = "inMenu";
                    //item.text = resString("lblColumnChooser");
                    item.hint = item.text;
                    item.locateInMenu = "auto";
                    //item.showText = "always";
                }
            });

            var idx = 1; // (is_deleted() || <% =Bool2JS(CanCreateNewModels) %> ? 2 : 3);

        var btnOptions = [
            {
                icon: "fas fa-filter",
                id: "filters",
                hint: resString("lblGridFiltering")
            },
            {
                icon: "fa fa-ellipsis-h",
                id: "autohide",
                hint: resString("lblAutoHideCols")
            },
            {
                icon: "fa fa-window-restore",
                id: "autopgsize",
                hint: resString("lblAutoPageSize")
            },
            {
                icon: "fa fa-arrows-alt-v",
                id: "infscroll",
                hint: resString("lblInfiniteScrolling")
            }
        ];

        var btnSelected = getSelectedGridOptions();

        toolbarItems.splice(idx, 0, {
            widget: 'dxButtonGroup',
            options: {
                items: btnOptions,
                elementAttr: { 'id': 'divGridOptions', 'class': 'on_advanced' },
                keyExpr: "id",
                //stylingMode: "text",
                selectionMode: "multiple",
                selectedItemKeys: btnSelected,
                onItemClick: function (e) {
                    switch (e.itemData.id) {
                        case "filters":
                            onSwitchFilteringColumns(!grid_filtering, true);
                            break;
                        case "autohide":
                            onSwitchAutoHideColumns(!grid_auto_collapse, true);
                            break;
                        case "autopgsize":
                            onSwitchAutoPager(!grid_autopager, true);;
                            break;
                        case "infscroll":
                            onSwitchScrollingMode(!grid_vscrolling, true);
                            break;
                    }
                    setTimeout(function () {
                        updateGridButtons();
                    }, 250);
                }
            },
            visible: isAdvancedMode,
            locateInMenu: "auto",
            showText: "inMenu",
            location: 'after'
        });
        toolbarItems.splice(idx + 1, 0, {
            widget: 'dxButton',
            options: {
                icon: 'fa fa-eraser',
                elementAttr: { "class": "on_advanced" },
                text: resString("btnReset"),
                hint: resString("btnEditTaskReset"),
                onClick: function () {
                    onResetSettings();
                    setTimeout(function () { initGrid(); }, 350);
                }
            },
            locateInMenu: "auto",
            showText: "inMenu",
            location: 'after',
            visible: isAdvancedMode
        });

        var pg = pageByID(nav_json, <% =_PGID_ADMIN_PRJ_LOOKUP %>);
        if ((pg)) {
            toolbarItems.push({
                widget: 'dxButton',
                options: {
                    icon: 'fa fa-binoculars',
                    text: "<%=JS_SafeString(PageMenuItem(_PGID_ADMIN_PRJ_LOOKUP)) %>",
                    hint: "<%=JS_SafeString(PageTitle(_PGID_ADMIN_PRJ_LOOKUP)) %>",
                    onClick: function () {
                        var pg = pageByID(nav_json, <% =_PGID_ADMIN_PRJ_LOOKUP %>);
                        if ((pg)) onMenuItemClick(pg);
                    }
                },
                visible: <% =Bool2JS(CanCreateNewModels AndAlso App.UserWorkgroups.Count > 1) %>,
                locateInMenu: "auto",
                showText: "inMenu",
                location: 'after'
            });

            toolbarItems.push({
                widget: 'dxButton',
                options: {
                    icon: 'fa fa-chart-bar',
                    elementAttr: { "class": "on_advanced" },
                    text: "<%=JS_SafeString(PageMenuItem(_PGID_ADMIN_PRJ_STAT)) %>",
                    hint: "<%=JS_SafeString(PageTitle(_PGID_ADMIN_PRJ_STAT)) %>",
                    onClick: function () {
                        var pg = pageByID(nav_json, <% =_PGID_ADMIN_PRJ_STAT %>);
                        if ((pg)) onMenuItemClick(pg);
                    }
                },
                visible: isAdvancedMode && can_manage_all,
                locateInMenu: "auto",
                showText: "inMenu",
                location: 'after'
            });
            }

            toolbarItems.push({
                widget: 'dxButton',
                options: {
                    icon: 'fa fa-sync',
                    elementAttr: { id: 'btnGridReload' },
                    text: resString("btnRefresh"),
                    hint: resString("btnRefresh"),
                    onClick: function () {
                        if (hasGrid()) {
                            //showLoadingPanel();
                            force_reload = true;
                            reloadProjectsList();
                        }
                    }
                },
                locateInMenu: "auto",
                showText: "inMenu",
                location: 'after'
            });

            toolbarItems.push({
                template: function () {
                    return $("<div id='lblHasFilter'></div>");
                },
                location: 'center',
                locateInMenu: 'Never'
            });


            setTimeout(function () {
                updateGridButtons();
                $(".dx-datagrid-export-button").addClass("on_advanced");
            }, 150);
        }

        function initSearchInput() {
            var tbSearch = $("input", ".dx-searchbox");
            if ((tbSearch) && (tbSearch.length)) {
                // reset search box in case of new session started
                var oldSID = localStorage.getItem(sess_session_id);
                if (oldSID != curSID) {
                    localStorage.setItem(sess_session_id, curSID);
                    if ((tbSearch.value != "") && hasGrid()) grid.searchByText("");
                }
                tbSearch.on("keyup", onSearchEdit).parent().parent().find(".dx-clear-button-area").on("click", onSearchEdit);
                tbSearch.select();
                //tbSearch.focus(); 
            }
        }

        var search_old = "";
        function onSearchEdit() {
            setTimeout(function () {
                var e = getSearchValue();
                if (search_old != e) {
                    showProjectsCounter(getSelected().length);
                    search_old = e;
                }
            }, 150);
        }

        function onSwitchAutoHideColumns(val, do_init) {
            grid_auto_collapse = val;
            document.cookie = "prj_lst_autohide=" + (grid_auto_collapse ? 1 : 0);
            if (do_init) {
                if (hasGrid()) grid.dispose();
                initGrid();
            }
        }

        function onSwitchFilteringColumns(val, do_init) {
            if (do_init) showLoadingPanel();
            grid_filtering = val;
            document.cookie = "prj_filtering=" + (grid_filtering ? 1 : 0);
            if (do_init) {
                if (hasGrid()) grid.option("filterRow.visible", grid_filtering);
                setTimeout(function () { hideLoadingPanel(); }, 500);
            }
        }

        function onSwitchAutoPager(val, do_init) {
            grid_autopager = val;
            document.cookie = "prj_autopage=" + (grid_autopager ? 1 : 0);
            if (hasGrid() && (do_init)) {
                if (grid_autopager) grid_pg_size = grid.option("paging.pageSize"); else grid.option("paging.pageSize", grid_pg_size);
                grid.option("pager.showPageSizeSelector", (!grid_vscrolling && !grid_autopager));
                onSetAutoPager();
            }
        }

        function onSwitchScrollingMode(val, do_init) {
            grid_vscrolling = val;
            document.cookie = "prj_vscroll=" + (grid_vscrolling ? 1 : 0);
            if (hasGrid() && (do_init)) {
                if (hasGrid()) grid.dispose();
                initGrid();
            }
        }

        function resetGridSettings() {
            showLoadingPanel();
            localStorage.removeItem(getSessName());
            if (hasGrid()) {
                grid.state({});
                grid.dispose();
            }
            hideLoadingPanel();
        }

        function onResetSettings() {
            if (hasGrid()) {
                onSwitchAutoHideColumns(grid_defaults.autocollapse, false);
                onSwitchFilteringColumns(grid_defaults.filtering, false);
                onSwitchAutoPager(grid_defaults.autopager, false);
                onSwitchScrollingMode(grid_defaults.vscrolling, false);
                setTimeout(function () {
                    resetGridSettings();
                }, 250);
            }
        }

        function onGridCellContextMenu(e) {
            if ((e.row) && (e.row.rowType === "data") && hasGrid() && (e.row.data)) {
                var d = e.row.data;
                if ((d.CanOpen) && (d.CanModify || d.isOnline)) {
                    e.items = getContextMenuItems(d);
                }
            }
        }

        function getContextMenuItems(data) {
            var is_regular = data.ProjectStatus == "<% =ecProjectStatus.psActive %>";       //(pgID == <% =_PGID_PROJECTSLIST %> || pgID == <% =_PGID_PROJECTSLIST_REVIEW %>);
        var is_template = data.ProjectStatus == "<% =ecProjectStatus.psTemplate %>";    //(pgID == <% =_PGID_PROJECTSLIST_TEMPLATES %>);
        var is_master = data.ProjectStatus == "<% =ecProjectStatus.psMasterProject %>";//(pgID == <% =_PGID_PROJECTSLIST_MASTERPROJECTS %>);
        var is_archived = data.ProjectStatus == "<% =ecProjectStatus.psArchived %>";    //(pgID == <% =_PGID_PROJECTSLIST_ARCHIVED %>);
        var is_deleted = data.isMarkedAsDeleted;
        var c_open = data.CanOpen;
        var c_eval = data.CanEvaluate;
        var c_view = data.CanView;
        var c_modify = data.CanModify;
        var is_new_ver = (db_version < data.Version);
        return [
            {
                text: (curPrjID == data.ID ? "<% =JS_SafeString(ResString("btnCloseProject")) %>" : "<% =JS_SafeString(ResString("btnOpenProject")) %>"),
                icon: (curPrjID == data.ID ? "far fa-eye-slash" : "far fa-eye"),
                visible: ((c_modify || curPrjID == data.ID) && !is_deleted && !is_new_ver),
                onItemClick: function () {
                    if (curPrjID == data.ID) closeProject(data.ID); else openProject(data.ID);
                }
            }<% If CanEditAtLeastOneModel Then %>, {
                text: "<% = JS_SafeString(ResString("titleProjectInfo")) %>",
                icon: "fa fa-file-invoice",
                visible: (!is_deleted && (c_view || c_modify) && !is_new_ver),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_PROJECT_PROPERTIES)) %>", data.ID);
                }
            }, {
                text: resString((is_template || is_master) ? "mnuProjectCreateFrom" : "mnuProjectCopy"),
                icon: "fa fa-copy",
                disabled: !can_create,
                visible: (!is_deleted && (c_modify || can_manage_all) && !is_new_ver),
                onItemClick: function () {
                    //var d = new Date();
                    //var dt = d.toLocaleDateString() + " " + d.toLocaleTimeString();
                    //var dt = new Date().toJSON().replace(/\u200E/g, "").replace(/T/g, " ").slice(0,19); // strip 8206 chars (left-to-right);
                    //var dt = new Date().toLocaleString().replace(/\u200E/g, "").replace(/T/g, " ").slice(0,19); // strip 8206 chars (left-to-right);
                    var dt = new Date(new Date().toString().split('GMT')[0] + ' UTC').toISOString().split('.')[0].replace('T', ' ');
                    if (is_template || is_master) createProject("<% =JS_SafeString(ResString("mnuProjectCreateFrom")) %>", "", [], "", (is_template ? data.Name : ""), data.ID, undefined, false); else <% =If(App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxLifetimeProjects) > 0, "onSaveAsWithConfirm", "onSaveAs") %>(data.ID, data.Name, data.Name + " (" + dt + ")", false);
                }
            }, {
                text: resString(is_regular ? "btnArchive" : "lblDoActiveProject"),
                icon: "fa fa-archive",
                visible: (c_modify && !is_deleted && !is_template && !is_master),
                onItemClick: function () {
                    onArchive([data.ID]);
                }
            }, {
                text: "<% =JS_SafeString(PageMenuItem(_PGID_PROJECT_DOWNLOAD)) %>",
                icon: "download",
                visible: (c_modify && !is_deleted),
                onItemClick: function () {
                    onDownload([data.ID]);
                }
            }, {
                text: "<% =JS_SafeString(ResString("mnuProjectUnDelete")) %>",
                icon: "fa fa-undo",
                disabled: !is_deleted,
                visible: (c_modify && is_deleted),
                onItemClick: function () {
                    onUnDelete([data.ID]);
                }
            }, {
                text: "<% =JS_SafeString(PageMenuItem(_PGID_PROJECT_DELETE)) %>",
                icon: "trash",
                visible: (c_modify),
                onItemClick: function () {
                    onDelete([data.ID]);
                }
            }<% End If %>, {
                text: "<% =JS_SafeString(PageMenuItem(_PGID_EVALUATION)) %>",
                icon: "fas fa-clipboard-list",
                visible: (c_eval && !is_deleted && is_regular && !isRiskion && !is_new_ver),
                onItemClick: function () {
                <% if App.isEvalURL_EvalSite Then %>url_on_open_blank = true;<% End if %>
                    onProjectPage(eval_url, data.ID);
                }
            }<% if CanEditAtLeastOneModel Then %>, {
                text: "<% =JS_SafeString(PageMenuItem(_PGID_MEASURE_EVAL_PROGRESS)) %>",
                icon: "fa fa-tasks",
                visible: (c_modify && !is_deleted && is_regular && !isRiskion && !is_new_ver),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_MEASURE_EVAL_PROGRESS)) %>", data.ID);
                }
            }, {
                text: "<% =JS_SafeString(PageMenuItem(_PGID_PROJECT_USERS)) %>",
                icon: "fa fa-user-friends",
                visible: (c_modify && !is_deleted && is_regular && !is_new_ver),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_PROJECT_USERS)) %>", data.ID);
                }
            }, {
                text: "<% =JS_SafeString(ResString("lblCopyEvalLink")) %>",
                icon: "fa fa-external-link-alt",
                visible: (c_eval && !is_deleted && is_regular && !is_new_ver), //  && !isRiskion
                onItemClick: function () {
                    checkProjectOnlineAndCopyLink(data.ID, data.isOnline, "<% =JS_SafeString(ApplicationURL(False, False)) %>/?passcode=" + data.AccessCode);
                }
            }, {
                text: "<% =JS_SafeString(ResString("lblGetLink")) %>",
                icon: "fa fa-link",
                visible: (c_modify && !is_deleted && is_regular && !isRiskion && !is_new_ver),
                onItemClick: function () {
                    getProjectLinkDialog(data.ID, data.Name, data.AccessCode, data.MeetingID, data.CS_MeetingID, data.isOnline, "<% =JS_SafeString(ApplicationURL(False, False)) %>", "", "<% =JS_SafeString(ApplicationURL(False, False)) %>", false);
                }
            }, {
                text: "<% =JS_SafeString(ResString("mnuAllocate")) %>",
                icon: "fa fa-balance-scale",
                visible: (c_modify && !is_deleted && is_regular && !isRiskion && !is_new_ver),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_RA_BASE)) %>", data.ID);
                }
            }<% End If %>, {
                text: "<% =JS_SafeString(ResString("lblResults")) %>",
                icon: "fa fa-chart-pie",
                visible: (c_view && !is_deleted && is_regular && !isRiskion && !is_new_ver),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_ANALYSIS_GRIDS_ALTS)) %>", data.ID);
                }
            }, {
                text: "<% =JS_SafeString(ResString("titleDashboard")) %>",
                icon: "fa fa-tachometer-alt",
                visible: (c_modify && !is_deleted && is_regular && !isRiskion && !is_new_ver),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_ANALYSIS_DASHBOARD)) %>", data.ID);
                }
            }<% if CanEditAtLeastOneModel Then %>, {
                text: "<% =JS_SafeString(ResString("lblLikelihood")) %>",
                icon: "fa fa-sitemap ",
                visible: (c_modify && !is_deleted && is_regular && isRiskion && !is_new_ver),
                items: [{
                    text: "<% =JS_SafeString(PageMenuItem(_PGID_EVALUATION)) %>",
                icon: "fas fa-clipboard-list",
                visible: (c_eval && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    <% if App.isEvalURL_EvalSite Then %>url_on_open_blank = true;<% End if %>
                    onProjectPage(eval_url, data.ID, data.AccessCode);
                }
            }<% if CanEditAtLeastOneModel Then %>, {
                    text: "<% =JS_SafeString(PageMenuItem(_PGID_MEASURE_EVAL_PROGRESS)) %>",
                icon: "fa fa-tasks",
                visible: (c_modify && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_MEASURE_EVAL_PROGRESS)) %>", data.ID, data.AccessCode);
                }
            }<%--<% If App.isEvalURL_EvalSite OrElse App.Options.EvalSiteURL <> "" Then %>, {
                text: "<% =JS_SafeString(ResString("lblCopyEvalLink")) %>",
                icon: "fa fa-external-link-alt",
                visible: (c_eval && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    checkProjectOnlineAndCopyLink(data.ID, data.isOnline, "<% =JS_SafeString(ApplicationURL(False, False)) %>/?passcode=" + data.AccessCode);
                }
            }<% End If %>--%>, {
                    text: "<% =JS_SafeString(ResString("lblGetLink")) %>",
                icon: "fa fa-link",
                visible: (c_modify && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    getProjectLinkDialog(data.ID, data.Name, data.AccessCode, data.MeetingID, data.CS_MeetingID, data.isOnline, "<% =JS_SafeString(ApplicationURL(False, False)) %>", " (<% =JS_SafeString(ResString("lblLikelihood")) %>)", "<% =JS_SafeString(ApplicationURL(False, False)) %>", false);
                }
            }<% End If %>, {
                    text: "<% =JS_SafeString(ResString("mnuEvaluateInvite")) %>",
                icon: "fa fa-envelope",
                visible: (c_modify && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_EVALUATE_INVITE)) %>", data.ID, data.AccessCode);
                    }
                }, {
                    text: "<% =JS_SafeString(ResString("lblResults")) %>",
                icon: "fa fa-chart-pie",
                visible: (c_view && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_ANALYSIS_OVERALL_ALTS)) %>", data.ID, data.AccessCode);
                        }
                    }]
            }, {
                text: "<% =JS_SafeString(ResString("lblImpact")) %>",
                icon: "fa fa-sitemap ",
                visible: (c_modify && !is_deleted && is_regular && isRiskion && !is_new_ver),
                items: [{
                    text: "<% =JS_SafeString(PageMenuItem(_PGID_EVALUATION)) %>",
                icon: "fas fa-clipboard-list",
                visible: (c_eval && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    <% if App.isEvalURL_EvalSite Then %>url_on_open_blank = true;<% End if %>
                    onProjectPage(eval_url, data.ID, data.AccessCodeImpact);
                }
            }<% if CanEditAtLeastOneModel Then %>, {
                    text: "<% =JS_SafeString(PageMenuItem(_PGID_MEASURE_EVAL_PROGRESS)) %>",
                icon: "fa fa-tasks",
                visible: (c_modify && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_MEASURE_EVAL_PROGRESS)) %>", data.ID, data.AccessCodeImpact);
                }
            }<%--<% If App.isEvalURL_EvalSite OrElse App.Options.EvalSiteURL <> "" Then %>, {
                text: "<% =JS_SafeString(ResString("lblCopyEvalLink")) %>",
                icon: "fa fa-external-link-alt",
                visible: (c_eval && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    checkProjectOnlineAndCopyLink(data.ID, data.isOnline, "<% =JS_SafeString(ApplicationURL(False, False)) %>/?passcode=" + data.AccessCodeImpact);
                }
            }<% End If %>--%>, {
                    text: "<% =JS_SafeString(ResString("lblGetLink")) %>",
                icon: "fa fa-link",
                visible: (c_modify && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    getProjectLinkDialog(data.ID, data.Name, data.AccessCodeImpact, data.MeetingIDImpact, data.CS_MeetingIDImpact, data.isOnline, "<% =JS_SafeString(ApplicationURL(False, False)) %>", " (<% =JS_SafeString(ResString("lblImpact")) %>)", "", "<% =JS_SafeString(ApplicationURL(False, False)) %>", false);
                }
            }<% End If %>, {
                    text: "<% =JS_SafeString(ResString("mnuEvaluateInvite")) %>",
                icon: "fa fa-envelope",
                visible: (c_modify && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_EVALUATE_INVITE)) %>", data.ID, data.AccessCodeImpact);
                    }
                }, {
                    text: "<% =JS_SafeString(ResString("lblResults")) %>",
                icon: "fa fa-chart-pie",
                visible: (c_view && !is_deleted && is_regular && isRiskion),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_ANALYSIS_OVERALL_ALTS)) %>", data.ID, data.AccessCodeImpact);
                        }
                    }]
            }<% End If %><% if CanEditAtLeastOneModel Then %>, {
                text: "<% =JS_SafeString(ResString("mnuEvaluateInvite")) %>",
                icon: "fa fa-envelope",
                visible: (c_modify && !is_deleted && is_regular && !isRiskion && !is_new_ver),
                onItemClick: function () {
                    onProjectPage("<% = JS_SafeString(PageURL(_PGID_EVALUATE_INVITE)) %>", data.ID);
                }
            }, {
                text: resString((is_template || is_master) ? "mnuProjectCopy" : "lblCreateTplMaster"),
                icon: "fa fa-paste",
                disabled: <% =Bool2JS(Not CanCreateNewModels) %>,
                visible: ((c_modify || can_manage_all) && !is_deleted && !is_new_ver),
                onItemClick: function () {
                    createProject((is_regular || is_archived ? "<% =JS_SafeString(ResString("lblCreateTplMaster")) %>" : "<% =JS_SafeString(ResString("mnuProjectCopy")) %>"), "", [], "", data.Name, data.ID, undefined, true);
                }
            }, {
                text: "<% =JS_SafeString(PageMenuItem(_PGID_PROJECT_SNAPSHOTS)) %>",
                icon: "ion ion-md-reverse-camera",
                visible: (c_modify && !is_deleted && !is_archived),
                onItemClick: function () {
                    onShowSnapshots(data.ID);
                }
            }, {
                text: "<% =JS_SafeString(PageMenuItem(_PGID_PROJECT_LOGS)) %>",
                icon: "fa fa-calendar-alt",
                visible: (c_modify && !is_deleted),
                onItemClick: function () {
                //onProjectPage("<% = JS_SafeString(PageURL(_PGID_PROJECT_LOGS)) %>", data.ID);
                //loadURL("<% = JS_SafeString(PageURL(_PGID_PROJECT_LOGS, "prj_id=")) %>" + data.ID);
                onShowIFrame("<% = JS_SafeString(PageURL(_PGID_PROJECT_LOGS, "temptheme=sl&prj_id=")) %>" + data.ID, "<% =JS_SafeString(PageMenuItem(_PGID_PROJECT_LOGS)) %>: " + ShortString(data.Name, 65, false));
                }
            }<% end If %>];
        }

        function onProjectPage(url, id, passcode) {
            url += (url.indexOf("?") > 0 ? "&" : "?") + "project=" + id;
            var extra = {};
            if ((isRiskion) && typeof passcode != "undefined" && passcode != "") {
                url = urlWithParams("<% =_PARAM_PASSCODE %>=" + passcode, url);
                extra = { "passcode": passcode };
            }
            url_on_open = url;
            openProject(id, extra);
        }

        function scanProjectsList() {
            if ((projects)) {
                master_projects = [];
                template_projects = [];
                wipeout_projects = [];
                var wipeout_date = "<% =Now.AddDays(-App.WipeoutProjectsTimeout(App.ActiveUser.UserID)).ToString("s") %>";
            for (var i = 0; i < projects.length; i++) {
                var p = projects[i];
                if (!p.isMarkedAsDeleted) {
                    if (p.ProjectStatus == <% =CInt(ecProjectStatus.psMasterProject) %>) master_projects.push(p);
                    if (p.ProjectStatus == <% =CInt(ecProjectStatus.psTemplate) %>) template_projects.push(p);
                    } else {
                        if (typeof p.ModifyDate != "undefined" && p.ModifyDate != "" && p.ModifyDate < wipeout_date) wipeout_projects.push(p);
                    }
                }
            }
        }

        function initDataSource() {
            PrjDataSource = new DevExpress.data.CustomStore({
                //loadMode: "raw",
                //cacheRawData: false,
                key: "ID",
                load: function (loadOptions) {
                    var btn_refresh = $("#btnGridReload");
                    if ((btn_refresh) && (btn_refresh.data())) {
                        btn_refresh.dxButton("option", "disabled", true);
                    }
                    if (force_reload || typeof projects == "undefined" || projects == null || !(projects.length)) {
                        if (force_reload) showLoadingPanel();
                        var deferred = $.Deferred(),
                            args = {};
                        if (loadOptions.sort) {
                            args.orderby = loadOptions.sort[0].selector;
                            if (loadOptions.sort[0].desc)
                                args.orderby += " desc";
                        }
                        args.skip = loadOptions.skip || 0;
                        args.take = loadOptions.take || 12;
                        if (force_reload) args.reload = true;
                        prj_last_dt = (new Date()).getTime();
                        callAPI("project/?<% =_PARAM_ACTION %>=list", args, function (res) {
                        showBackgroundMessage("Processing data...");
                        //hideLoadingPanel();
                        if (isValidReply(res)) {
                            var msg = res.Message;
                            if (res.Result == _res_Success) {
                                //deferred.resolve(data, { totalCount: data.length });
                                deferred.resolve(res.Data);
                                if (typeof res.Tag != "undefined" && res.Tag != null && typeof res.Tag.CanCreateNew != "undefined") {
                                    can_create = (res.Tag.CanCreateNew);
                                    projects_cnt = res.Tag.License_TotalValue;
                                    projects_max = res.Tag.License_TotalLimit;
                                    lifetime_cnt = res.Tag.License_LifetimeValue;
                                    lifetime_max = res.Tag.License_LifetimeLimit;
                                }
                                projects = res.Data;
                                scanProjectsList();
                                updateGridPagination();
                                projects_loaded = true;

                                var btn_refresh = $("#btnGridReload");
                                if ((btn_refresh) && (btn_refresh.data())) {
                                    btn_refresh.dxButton("option", "disabled", false);
                                }
                                var t = $("#toolbar");
                                if ((t) && (t.length) && (toolbar)) toolbar.option("disabled", false);

                                updateToolbarButtons();
                            } else {
                                if (res.Message == "") msg = "Unable to load models list";
                            }
                            if (msg != "") {
                                DevExpress.ui.dialog.alert(msg, resString("lblError"));
                            }
                        }
                    }, "Loading models list...");
                    force_reload = false;
                    return deferred.promise();
                } else {
                    return projects;
                }
            },
            update: function (key, values) {
                if ((key)) {
                    if (typeof values["Name"] != "undefined" && values["Name"] != "") setTimeout(function () {
                        $("#lblProjectName" + key).html(values["Name"]);
                    }, 50);
                    values["ID"] = key;
                    callAPI("project/?<% =_PARAM_ACTION %>=update", values, function (res) {
                        onEditName(res, key, values);
                        var idx = grid.getRowIndexByKey(key);
                        if ((idx >= 0)) {
                            setTimeout(function () {
                                grid.repaintRows([idx]);
                            }, 300);
                        }
                    }, true);
                }
            },
            onLoading: function (loadOptions) {
            },
            onLoaded: function (request) {
                hideLoadingPanel();
                updateToolbarButtons();
                //showBackgroundMessage("Update list...", "ion ion-ios-cog", false);
            },
            errorHandler: function (error) {
                DevExpress.ui.notify("<% JS_SafeString(ResString("msgLoadProjectsError")) %>: " + error.message, "error");
            },
            onPush: function (changes) {
                if ((changes)) {
                    for (var i = 0; i < changes.length; i++) {
                        var d = changes[i];
                        if (d.type == "update" && d.key == curPrjID) {
                            updateActiveProjectUIAttributes(d.data);
                        }
                    }
                }
            }
        });
        }

        function onEditName(res, key, values) {
            showLoadingPanel();
            var prj = null;
            if (isValidReply(res) && res.Result == _res_Success && typeof res.Data != "undefined") prj = res.Data
            setTimeout(function () {
                updateToolbarButtons();
                $("#grid").dxDataGrid("option", "focusedColumnIndex", -1);
                $("#grid").dxDataGrid("option", "focusedRowIndex", -1);
                $("#grid").blur();
                hideLoadingPanel();
            }, 500);
            for (var i = 0; i < projects.length; i++) {
                var p = projects[i];
                if ((p.ID == key)) {
                    if ((prj != null)) {
                        projects[i] = prj;
                        if (p.ID == curPrjID) curPrjData = prj;
                        pushProjectData(projects[i]);
                    } else {
                        if (typeof values["Name"] != "undefined" && values["Name"] != "") {
                            p.Name = values["Name"];
                            if (p.ID == curPrjID) curPrjData.Name = p.Name;
                            pushProjectData(p);
                        }
                    }
                    break;
                }
            }
        }

        function showPMInstruction() {
            var show_cb = <% =Bool2JS(App.isCommercialUseEnabled AndAlso HasActiveProjects()) %>;
        var msg = "<iframe frameborder='0' scrolling='yes' src='../" + (isRiskion ? "PMgettingStartedRisk.html" : "PMgettingStarted.html") + "' style='border:0px solid #f0f0f0; width:100%; height:100%;'></iframe>";
        if (show_cb) msg += "<div style='padding-top:1ex'><div id='cbShowSplash'></div></div>";

        $("#popupContainer").dxPopup({
            width: dlgMaxWidth(600),
            height: dlgMaxHeight(450 - (show_cb ? 0 : 30)),
            contentTemplate: function () {
                return $(msg);
            },
            showTitle: true,
            title: "<% =JS_SafeString(ApplicationName()) %>",
            dragEnabled: true,
            shading: true,
            closeOnOutsideClick: true,
            resizeEnabled: true,
            showCloseButton: true,
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    text: "<% =JS_SafeString(ResString("btnOK")) %>",
                    elementAttr: { "class": "button_enter button_esc" },
                    onClick: function (e) {
                        closePopup();
                        showPopupsOnLoad(2);
                    }
                },
                toolbar: "bottom",
                location: 'after'
            }]
        });
        $("#popupContainer").dxPopup("show");
        if (show_cb) {
            var title = "<% =JS_SafeString(String.Format(ResString("lblDontShowPMSplash"), PageMenuItem(_PGID_ACCOUNT_EDIT))) %>";
            $("#cbShowSplash").dxCheckBox({
                value: false,
                text: title,
                onValueChanged: function (e) {
                    callAPI("account/?<% =_PARAM_ACTION %>=option", { "name": "show_splash", "value": !(e.value) }, hideLoadingPanel, "<% =JS_SafeString(ResString("msgSaving")) %>");
                }
            });
            $("#cbShowSplash").find(".dx-checkbox-text").html(title);
        }
        show_splash = false;
        document.cookie = "show_splash" + (isRiskion ? "_risk" : "") + "<% =If(App.isCommercialUseEnabled, "", "_" + App.ActiveWorkgroup.ID.ToString) %>=0;";
        }


        function showPopupsOnLoad(idx) {
            show_popups = true;
            const max_idx = 5;
            var dlg = null;
            var do_next = (typeof idx != "undefined" && idx <= max_idx && idx >= 0);
            switch (idx) {
                case 0:<% If Message <> "" Then %>dlg = DevExpress.ui.dialog.alert("<div style='max-width:" + dlgMaxWidth(650) + "px; max-height:" + dlgMaxHeight(450) + "px; overflow: auto;'><% =JS_SafeString(Message) %></div>", "<% =JS_SafeString(ResString(If(isError, "titleAlert", "titleInformation"))) %>");<% End if %>
                break;
            case 1:<% If CanShowPMInstruction() Then %>
                if (show_splash) {
                    showPMInstruction();
                    do_next = false;
                }<% End If %>
                break;
            case 2:<% Dim sToday As String = Date.Now().ToString("yyyy-MM-dd")
        Dim sLastShow As String = GetCookie(_COOKIE_LAST_NEWS, "")
        If Not App.isRiskEnabled AndAlso OPT_SHOW_NEWS AndAlso sToday > sLastShow AndAlso CanCreateNewModels Then %>dlg = onWhatsNew(false);
                do_next = false;<% End If %>
                break;
            case 3:<% Dim sTodayIssues As String = Date.Now().ToString("yyyy-MM-dd")
        Dim sLastIssues As String = GetCookie(_COOKIE_LAST_ISSUES, "")
        If OPT_SHOW_KNOWN_ISSUES AndAlso (sLastIssues = "" OrElse sTodayIssues > sLastIssues) AndAlso CanCreateNewModels Then %>if (show_issues) {
                dlg = onKnownIssues(true);
                do_next = false;
            }<% End If %>
                break;
            case 4:<%--<% Dim sTodayWipeout As String = Date.Now().AddDays(1).Date.Ticks.ToString
    Dim sLastWipeout As String = GetCookie(_COOKIE_WIPEOUT + App.ActiveWorkgroup.ID.ToString, "")
    If True OrElse App.WipeoutProjectsTimeout(App.ActiveUser.UserID) > 0 AndAlso CanCreateNewModels AndAlso (sLastWipeout = "" OrElse sTodayWipeout > sLastWipeout) Then %>doCheckWipeout();
                do_next = false;<% End If %>--%>
                break;
            case 5:<% if Not App.isRiskEnabled AndAlso _OPT_SHOW_RELEASE_NOTES AndAlso CanEditAtLeastOneModel AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsEnabled Then %>
                var rn = "<% = JS_SafeString(GetCookie("ReleaseNotes", "")) %>";
                var rr = "<% = JS_SafeString(GetCookie("ReleaseRemind", "")) %>";
                if (rn != "<% = JS_SafeString(ResString("ReleaseNotesCookie")) %>" && rr == "") {
                    onShowReleaseNotes();
                    do_next = false;
                }<% End If %>
                    break;
            }

            if (do_next) {
                if ((dlg)) {
                    dlg.done(function (result) {
                        if (idx < max_idx) showPopupsOnLoad(idx + 1);
                    });
                } else {
                    if (idx < max_idx) showPopupsOnLoad(idx + 1);
                }
            }
            if (idx >= max_idx) show_popups = false;
            //if (idx>=max_idx && !(projects.length)) showLoadingPanel();
        }

        var wipeout_tries = 10;
        function doCheckWipeout() {
            if (wipeout_tries > 0) {
                if (typeof wipeout_projects == "undefined" || wipeout_projects == null) {
                    wipeout_tries--;
                    setTimeout(function () {
                        doCheckWipeout();
                    }, 1000);
                } else {
                    document.cookie = "<% =JS_SafeString(_COOKIE_WIPEOUT + App.ActiveWorkgroup.ID.ToString) %>=<% =Date.Now().AddDays(1).Date.Ticks.ToString %>;path=/;expires=Mon, 31-Dec-2029 23:59:59 GMT;";
                wipeout_tries = 0;
                if (wipeout_projects.length) {
                    var msg = "<% =JS_SafeString(ResString("msgProjectsWipeout")) %>".format(wipeout_projects.length, <% =App.WipeoutProjectsTimeout(App.ActiveUser.UserID) %>);
                        var result = DevExpress.ui.dialog.confirm(msg, resString("titleConfirmation"));
                        result.done(function (dialogResult) {
                            if (dialogResult) {
                                var lst = [];
                                for (var i = 0; i < wipeout_projects.length; i++) {
                                    lst.push(wipeout_projects[i].ID);
                                }
                                wipeoutProjects(lst, function () {
                                    showLoadingPanel();
                                    wipeout_projects = [];
                                    force_reload = true;
                                    setTimeout(function () {
                                        reloadProjectsList();
                                        showPopupsOnLoad(5);
                                    }, 750);
                                });
                            } else {
                                showPopupsOnLoad(5);
                            }
                        });
                    }
                }
            } else {
                showPopupsOnLoad(5);
            }
        }

        function onSwitchAdvancedMode() {
            $("#grid").html();
            grid = null;
            showLoadingPanel();
            do_flash_advanced = true;
            //toolbar = null;
            initToolbar();
            //var _state = window.localStorage[getSessName()];
            initGrid();
            //if ((grid)) { 
            //    grid.state(_state);
            //}
        }

        function onGetPingParams() {
            var ms = (new Date()).getTime() - prj_last_dt;
            var s = Math.ceil(ms / 1000);
            return (projects_loaded ? "check=projects&last=" + s : "");
        }

        function onPingRecieved(data) {
            if (typeof data != "undefined" && (data)) {
                if (typeof data.projects != "undefined" && (data.projects.length) && (projects) && (projects_loaded)) {
                    var lst = data.projects;
                    if (lst.length > 0) {
                        for (var i = 0; i < projects.length; i++) {
                            if (projects[i].OnlineUsers > 0) {
                                projects[i].OnlineUsers = 0;
                                projects[i].ComplexStatus = getComplexStatus(projects[i]);
                                pushProjectData(projects[i]);
                            }
                        }
                    }
                    for (var i = 0; i < lst.length; i++) {
                        if (typeof lst[i].ID != "undefined") {
                            var p = getItemByID(projects, lst[i].ID);
                            if ((p)) {
                                pushProjectData(lst[i]);
                            } else {
                                projects.push(lst[i]);
                                PrjDataSource.push([{ type: "insert", data: lst[i], key: lst[i].ID }]);
                            }
                        }
                    }
                    showProjectsCounter(getSelected().length);
                }
            }
            prj_last_dt = (new Date()).getTime();
        }

        function beforeInitNavigation(lst) {
            if ((lst) && (lst.length)) {
                for (var i = 0; i < lst.length; i++) {
                    var m = lst[i];
                    if (typeof m.extra != "undefined" && m.extra.indexOf("projectslist") >= 0) {
                        m.selected = true;
                    }
                    if (typeof m.items != "undefined" && (m.items.length)) beforeInitNavigation(m.items);
                }
            }
        }

        resize_custom = function (force_redraw) {
            onResize(force_redraw, true);
        };

        initDataSource();
        $(document).ready(function () {
            //showLoadingPanel();
            initToolbar();
            initGrid();
            saveLastVisited(pgID);
            var ver = localStorage.getItem("prj_list_col_ver");
            if (ver != columns_ver) {
                if ((ver) && ver > "") onResetSettings();
                localStorage.setItem("prj_list_col_ver", columns_ver);
            }
            ping_params = onGetPingParams;
            ping_on_reply = onPingRecieved;
            showPopupsOnLoad(0);
        });

    </script>
    <%--<div class="table" id="tblPageMain">
        <% If CanEditAtLeastOneModel Then %><div class="tr" style="height:2em;">
            <div class="td">
                <div id="toolbar" class="dxToolbar"></div>
            </div>
        </div><% End If %>
        <div class="tr whole" id="trGrid">
            <div class="td tdCentered"><div id="grid"></div></div>
        </div>
    </div>--%>

    <div class="main_wrapper">
        <div class="container">
            <div class="tab-content" id="myTabContent">
                <div class="tab-pane fade show active" id="models" role="tabpanel" aria-labelledby="models-tab">
                    <div class="manage-model-page-wrapper collect-input-page-wrapper">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="breadcrumb-wrapper">
                                    <nav aria-label="breadcrumb">
                                        <ol class="breadcrumb">
                                            <li class="breadcrumb-item">
                                                <a href="collect_input_dashboard.php">Home</a>
                                            </li>
                                            <li class="breadcrumb-item active" aria-current="page">Manage Models</li>
                                        </ol>
                                    </nav>
                                </div>
                            </div>
                            <!-- Manage Model page  -->
                            <div class="col-md-12">
                                <div class="header-content-wrapper d-flex jus-con-sb align-center-info">
                                    <div class="head-title-wrapper">
                                        <h2 class="head-title-info col-blue">Manage Models </h2>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="manage-model-page-main-wrapper">
                                    <div class="manage-model-page-main-info">


                                        <div class="manage-model-page-table-wrapper table-wb-shadow-info">
                                            <div class="manage-model-page-table-info">
                                                <div class="pages-table-head-info d-flex">
                                                    <div class="search-and-columns-info cus-wid-100 d-flex jus-con-sb">
                                                        <div class="table-header-ctas-wrapper d-flex align-items-center">
                                                            <div class="form-group a-link-wrapper">
                                                                <input type="checkbox" id="html1allza" class="checkbox">
                                                                <label for="html1allza" class="checkbox_label"></label>
                                                            </div>
                                                            <div class="deleteall-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/../Images/img/icon/delete-btn.svg" alt="delete-all">
                                                                </a>
                                                            </div>
                                                            <div class="copyall-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/../Images/img/icon/Copy-square-icon.svg" alt="copy-all">
                                                                </a>
                                                            </div>
                                                            <div class="cloud-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/img/icon/colud-database-icon.svg" alt="cloud-data">
                                                                </a>
                                                            </div>
                                                            <div class="add-new-model-info a-link-wrapper">
                                                                <a href="#" class="btn prm_green_btn" data-bs-toggle="modal" data-bs-target="#add_new_modal">
                                                                    <img src="../Images/img/Add_white.svg" class="me-2">
                                                                    Add Model</a>
                                                            </div>
                                                            <!--
                                        <div class="sort-data-info a-link-wrapper">
                                            <a href="#" class="btn a-link-info">
                                                <img src="../Images/img/icon/star-yellow-icon.svg" class="me-1"  alt="sort-data"> Favorites
                                            </a>
                                        </div>
-->

                                                        </div>
                                                        <div class="table_setting align-center-info">
                                                            <div class="cta-export-info">
                                                                <a href="#" class="cta-link-info remove-all-info">
                                                                    <img src="../Images/img/icon/refresh-rectangle.svg" alt="refresh-link">
                                                                </a>
                                                            </div>
                                                            <div class="cta-export-info">
                                                                <a href="#" class="cta-link-info remove-all-info">
                                                                    <img src="../Images/img/icon/export-rectangle.svg" alt="export-link">
                                                                </a>
                                                            </div>
                                                            <div class="btn-group mr-10" role="group" aria-label="Button group with nested dropdown">
                                                                <div class="btn-group" role="group">
                                                                    <button id="btnGroupDrop1c" type="button" class="btn choose-column-btn-info dropdown-toggle">
                                                                        <img src="./../Images/img/icon/Columns.svg" class="me-2">
                                                                        Choose columns
                                                                    <img src="../Images/img/icon/down-arrow-bordered-icon.svg" class="down-arrow-icon" alt="down-arrow">
                                                                    </button>
                                                                    <div class="dropdown-menu-choose-options-info">
                                                                        <form action="" method="post">
                                                                            <div class="row">
                                                                                <div class="col-12 col-md-6">
                                                                                    <ul class="dropdown-menu-info dropdown-menu-info-one main-dropdown-menu-info">
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkid2ab" class="checkbox">
                                                                                                <label for="checkid2ab" class="checkbox_label">ID</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprojects2ab" class="checkbox">
                                                                                                <label for="checkprojects2ab" class="checkbox_label">Projects</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="Checkfunded2ab" class="checkbox">
                                                                                                <label for="Checkfunded2ab" class="checkbox_label">Funded</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkfundedprojects2ab" class="checkbox">
                                                                                                <label for="checkfundedprojects2ab" class="checkbox_label">Finish data of funded project</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkbenefit2ab" class="checkbox">
                                                                                                <label for="checkbenefit2ab" class="checkbox_label">Benefit</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkexpbenefits2ab" class="checkbox">
                                                                                                <label for="checkexpbenefits2ab" class="checkbox_label">Expected benefit</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkrisk2ab" class="checkbox">
                                                                                                <label for="checkrisk2ab" class="checkbox_label">Risk</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprosuccess2ab" class="checkbox">
                                                                                                <label for="checkprosuccess2ab" class="checkbox_label">Probability of success</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprofailure2ab" class="checkbox">
                                                                                                <label for="checkprofailure2ab" class="checkbox_label">Probability of Failure</label>
                                                                                            </div>
                                                                                        </li>
                                                                                    </ul>
                                                                                </div>
                                                                                <div class="col-12 col-md-6">
                                                                                    <ul class="dropdown-menu-info dropdown-menu-info-two main-dropdown-menu-info">
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkcost2ab" class="checkbox">
                                                                                                <label for="checkcost2ab" class="checkbox_label">Cost</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkgroupinfo2ab" class="checkbox">
                                                                                                <label for="checkgroupinfo2ab" class="checkbox_label">Group</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="Checkpartialinfo2ab" class="checkbox">
                                                                                                <label for="Checkpartialinfo2ab" class="checkbox_label">Partial</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmininfo2ab" class="checkbox">
                                                                                                <label for="checkmininfo2ab" class="checkbox_label">Min%</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmustinfo2ab" class="checkbox">
                                                                                                <label for="checkmustinfo2ab" class="checkbox_label">Must</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmustnot2ab" class="checkbox">
                                                                                                <label for="checkmustnot2ab" class="checkbox_label">Must not</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkcustomeconstraints2ab" class="checkbox">
                                                                                                <label for="checkcustomeconstraints2ab" class="checkbox_label">[Custom Constraints]</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkaltattributes2ab" class="checkbox">
                                                                                                <label for="checkaltattributes2ab" class="checkbox_label">[Alt. Attributes]</label>
                                                                                            </div>
                                                                                        </li>
                                                                                    </ul>
                                                                                </div>
                                                                                <div class="col-12 col-md-12">
                                                                                    <div class="row align-items-center dropdown-show-hide-all-wrapper">
                                                                                        <div class="col-12 col-md-6">
                                                                                            <ul class="dropdown-menu-info dropdown-show-hide-all-info d-flex">
                                                                                                <li>
                                                                                                    <div class="form-group">
                                                                                                        <input type="checkbox" id="checkshowall2ab" class="checkbox">
                                                                                                        <label for="checkshowall2ab" class="checkbox_label">Show all</label>
                                                                                                    </div>
                                                                                                </li>
                                                                                                <li>
                                                                                                    <div class="form-group">
                                                                                                        <input type="checkbox" id="checkhideall2ab" class="checkbox">
                                                                                                        <label for="checkhideall2ab" class="checkbox_label">Hide all</label>
                                                                                                    </div>
                                                                                                </li>
                                                                                            </ul>
                                                                                        </div>
                                                                                        <div class="col-12 col-md-6">
                                                                                            <div class="applyall-cta-info text-end">
                                                                                                <button type="submit" class="btn prm_green_btn">Apply</button>
                                                                                            </div>
                                                                                        </div>
                                                                                    </div>
                                                                                </div>

                                                                            </div>
                                                                        </form>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="input-group search_div">
                                                                <span class="input-group-text" id="basic-addon1z">
                                                                    <img src="./../Images/img/icon/Search.svg"></span>
                                                                <input type="text" class="form-control" placeholder="Search" aria-label="Username" aria-describedby="basic-addon1">
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="manage-model-page-content-info">
                                                    <div class="table-responsive participant_table">
                                                        <table class="table table-striped-info">
                                                            <thead>
                                                                <tr>
                                                                    <th class="text_blue" style="width: 4%;">
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="emailcheck1a" class="checkbox">
                                                                            <label for="emailcheck1a" class="checkbox_label"></label>
                                                                        </div>
                                                                    </th>
                                                                    <th class="text_blue" style="width: 4%;">
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info th-star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="unfilled-star-icon switch-star-wrap d-none">
                                                                                <img src="../Images/img/icon/star-yellow-icon.svg" alt="filled-star-icon" class="filled-star-icon switch-star-wrap">
                                                                            </a>
                                                                        </div>
                                                                    </th>
                                                                    <th class="text_blue">Model name
                                                                    <div class="filter-icons-info d-none"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                    </th>

                                                                    <th class="text_blue">Last Access
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Online
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Status
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Model type
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Your role
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Has survey
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Creater
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                                <tr>
                                                                    <td>
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="html1" class="checkbox">
                                                                            <label for="html1" class="checkbox_label"></label>
                                                                        </div>
                                                                    </td>
                                                                    <td>
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="unfilled-star-icon td-switch-star-wrap switch-star-wrap d-none">
                                                                                <img src="../Images/img/icon/star-yellow-icon.svg" alt="filled-star-icon" class="filled-star-icon td-switch-star-wrap switch-star-wrap">
                                                                            </a>
                                                                        </div>
                                                                    </td>
                                                                    <td>Perception of Relative Importance of Select Department
                                                                    <div class="vertical-dots-wrapper" id="verticaldotlinkone1">
                                                                        <div class="icon-info">
                                                                            <a href="#" class="vertical-dot-link-info">
                                                                                <img src="../Images/img/icon/vertical-dots-icon.svg">
                                                                            </a>
                                                                        </div>

                                                                    </div>
                                                                        <div class="vertical-table-data-info" id="openverticaldotlinkone1">
                                                                            <ul class="vertical-table-data-ul">
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/eye-closed-icon.svg" alt="close-model"></div>
                                                                                        Close Model
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-details-icon.svg" alt="model-details-icon"></div>
                                                                                        Model Details
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/save-as-icon.svg" alt="save-as-icon"></div>
                                                                                        Save as
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/archive-icon.svg" alt="archives-icon"></div>
                                                                                        Archives
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/download-icon.svg" alt="download-icon"></div>
                                                                                        Download
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/delete-model-icon.svg" alt="delete-model-icon"></div>
                                                                                        Delete
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-details-icon.svg" alt="collect-my-icon"></div>
                                                                                        Collect my input
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-logs-icon.svg" alt="delete-model-icon"></div>
                                                                                        Evaluation Status
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/Add participants.svg" alt="delete-model-icon"></div>
                                                                                        Participants
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/Export.svg" alt="export-model-icon"></div>
                                                                                        Get model link and copy to clipboard
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/get-link-icon.svg" alt="getlink-model-icon"></div>
                                                                                        Get links
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/law-icon.svg" alt="allocate-model-icon"></div>
                                                                                        Allocate
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/pie-chart-icon.svg" alt="delete-model-icon"></div>
                                                                                        Results
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/dashboard-icons.svg" alt="dashboard-model-icon"></div>
                                                                                        Dashboard
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/mail-icon.svg" alt="send-invitaion-model-icon"></div>
                                                                                        Send Invitation
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/create-temp-icon.svg" alt="create-temp-icon"></div>
                                                                                        Create Template or Default Option Set
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/photo-camera-icon.svg" alt="snap-model-icon"></div>
                                                                                        Model snapshots
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-logs-icon.svg" alt="model-logs-icon"></div>
                                                                                        Model Logs
                                                                                    </a>
                                                                                </li>
                                                                            </ul>
                                                                        </div>
                                                                    </td>

                                                                    <td>4/5/2022, 2:23:02 AM</td>
                                                                    <td>
                                                                        <div class="manage-model-toggle-wrapper">
                                                                            <div class="toggle-on-off-btn-info">
                                                                                <label class="toggle-switch-info">
                                                                                    <input type="checkbox" id="togglebtn1a" checked="">
                                                                                    <span class="toggle-slider-info"></span>
                                                                                </label>
                                                                            </div>
                                                                        </div>
                                                                    </td>
                                                                    <td>Available</td>
                                                                    <td>Regular</td>
                                                                    <td>Project Manager</td>
                                                                    <td>Beginning</td>
                                                                    <td>Beginning</td>

                                                                </tr>
                                                                <tr>
                                                                    <td>
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="html2a" class="checkbox">
                                                                            <label for="html2a" class="checkbox_label"></label>
                                                                        </div>
                                                                    </td>
                                                                    <td>
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="unfilled-star-icon td-switch-star-wrap switch-star-wrap d-none">
                                                                                <img src="../Images/img/icon/star-yellow-icon.svg" alt="filled-star-icon" class="filled-star-icon td-switch-star-wrap switch-star-wrap">
                                                                            </a>
                                                                        </div>
                                                                    </td>
                                                                    <td>Perception of Relative Importance of Select Department
                                                                    <div class="vertical-dots-wrapper" id="verticaldotlinktwo">
                                                                        <div class="icon-info">
                                                                            <a href="#" class="vertical-dot-link-info">
                                                                                <img src="../Images/img/icon/vertical-dots-icon.svg">
                                                                            </a>
                                                                        </div>

                                                                    </div>


                                                                    </td>

                                                                    <td>4/5/2022, 2:23:02 AM</td>
                                                                    <td>
                                                                        <div class="manage-model-toggle-wrapper">
                                                                            <div class="toggle-on-off-btn-info">
                                                                                <label class="toggle-switch-info">
                                                                                    <input type="checkbox" id="togglebtn2a" checked="">
                                                                                    <span class="toggle-slider-info"></span>
                                                                                </label>
                                                                            </div>
                                                                        </div>
                                                                    </td>
                                                                    <td>Available</td>
                                                                    <td>Regular</td>
                                                                    <td>Project Manager</td>
                                                                    <td>Beginning</td>
                                                                    <td>Beginning</td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="pagination-bottom-wrapper">
                                                <div class="pagination-bottom-info d-flex">
                                                    <div class="pagi-left">
                                                        <ul class="pagi-ul d-flex">
                                                            <li class="pagi-li"><a href="#" class="pagi-link active">10</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">25</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">50</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">100</a></li>
                                                        </ul>
                                                    </div>
                                                    <div class="pagi-right">
                                                        <ul class="pagi-ul d-flex">
                                                            <li>
                                                                <div class="pagination-data-wrap">
                                                                    <ul class="pagi-data-ul">
                                                                        <li class="pagi-prev-data-info">Page <span class="pagi-prev-data">1</span></li>
                                                                        <li class="pagi-next-data-info">of <span class="pagi-next-data">3</span><span class="pagi-remaining-data"> (124 items)</span></li>
                                                                    </ul>
                                                                </div>
                                                            </li>
                                                            <li class="pagi-li pagi-prev-info"><a href="#" class="pagi-link">
                                                                <img src="../Images/img/icon/left-arrow-icon.svg" alt="left-arrow">
                                                            </a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link active">1</a></li>
                                                            <li class="pagi-li pagi-next-info"><a href="#" class="pagi-link">
                                                                <img src="../Images/img/icon/right-arrow-icon.svg" alt="right-arrow"></a></li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- End -->
                        </div>
                    </div>
                </div>

                <div class="tab-pane fade" id="archives" role="tabpanel" aria-labelledby="archives-tab">
                    <div class="container">
                        <div class="manage-model-archives-wrapper collect-input-page-wrapper">
                            <div class="row">
                                <div class="col-md-12">
                                    <div class="breadcrumb-wrapper">
                                        <nav aria-label="breadcrumb">
                                            <ol class="breadcrumb">
                                                <li class="breadcrumb-item">
                                                    <a href="collect_input_dashboard.php">Home</a>
                                                </li>
                                                <li class="breadcrumb-item active" aria-current="page">Manage Models Archives</li>
                                            </ol>
                                        </nav>
                                    </div>
                                </div>
                                <!-- Manage Model Archives  -->
                                <div class="col-md-12">
                                    <div class="header-content-wrapper d-flex jus-con-sb align-center-info">
                                        <div class="head-title-wrapper">
                                            <h2 class="head-title-info col-blue">Manage Models Archives</h2>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-12">
                                    <div class="manage-model-archives-main-wrapper">
                                        <div class="manage-model-archives-main-info">


                                            <div class="manage-model-archives-table-wrapper table-wb-shadow-info">
                                                <div class="manage-model-archives-table-info">
                                                    <div class="pages-table-head-info d-flex">
                                                        <div class="search-and-columns-info cus-wid-100 d-flex jus-con-sb">
                                                            <div class="table-header-ctas-wrapper d-flex align-items-center">
                                                                <div class="form-group a-link-wrapper">
                                                                    <input type="checkbox" id="html1all" class="checkbox">
                                                                    <label for="html1all" class="checkbox_label"></label>
                                                                </div>
                                                                <div class="deleteall-data-info a-link-wrapper">
                                                                    <a href="#" class="a-link-info">
                                                                        <img src="../Images/img/icon/delete-btn.svg" alt="delete-all">
                                                                    </a>
                                                                </div>
                                                                <div class="copyall-data-info a-link-wrapper">
                                                                    <a href="#" class="a-link-info">
                                                                        <img src="../Images/img/icon/Copy-square-icon.svg" alt="copy-all">
                                                                    </a>
                                                                </div>
                                                                <div class="cloud-data-info a-link-wrapper">
                                                                    <a href="#" class="a-link-info">
                                                                        <img src="../Images/img/icon/colud-database-icon.svg" alt="cloud-data">
                                                                    </a>
                                                                </div>
                                                            </div>
                                                            <div class="table_setting align-center-info">
                                                                <div class="cta-export-info">
                                                                    <a href="#" class="cta-link-info remove-all-info">
                                                                        <img src="../Images/img/icon/refresh-rectangle.svg" alt="refresh-link">
                                                                    </a>
                                                                </div>
                                                                <div class="cta-export-info">
                                                                    <a href="#" class="cta-link-info remove-all-info">
                                                                        <img src="../Images/img/icon/export-rectangle.svg" alt="export-link">
                                                                    </a>
                                                                </div>
                                                                <div class="btn-group mr-10" role="group" aria-label="Button group with nested dropdown">
                                                                    <div class="btn-group" role="group">
                                                                        <button id="btnGroupDrop1c" type="button" class="btn choose-column-btn-info dropdown-toggle">
                                                                            <img src="./../Images/img/icon/Columns.svg" class="me-2">
                                                                            Choose columns
                                                                        <img src="../Images/img/icon/down-arrow-bordered-icon.svg" class="down-arrow-icon" alt="down-arrow">
                                                                        </button>
                                                                        <div class="dropdown-menu-choose-options-info">
                                                                            <form action="" method="post">
                                                                                <div class="row">
                                                                                    <div class="col-12 col-md-6">
                                                                                        <ul class="dropdown-menu-info dropdown-menu-info-one main-dropdown-menu-info">
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkid2a" class="checkbox">
                                                                                                    <label for="checkid2a" class="checkbox_label">ID</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkprojects2a" class="checkbox">
                                                                                                    <label for="checkprojects2a" class="checkbox_label">Projects</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="Checkfunded2a" class="checkbox">
                                                                                                    <label for="Checkfunded2a" class="checkbox_label">Funded</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkfundedprojects2a" class="checkbox">
                                                                                                    <label for="checkfundedprojects2a" class="checkbox_label">Finish data of funded project</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkbenefit2a" class="checkbox">
                                                                                                    <label for="checkbenefit2a" class="checkbox_label">Benefit</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkexpbenefits2a" class="checkbox">
                                                                                                    <label for="checkexpbenefits2a" class="checkbox_label">Expected benefit</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkrisk2a" class="checkbox">
                                                                                                    <label for="checkrisk2a" class="checkbox_label">Risk</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkprosuccess2a" class="checkbox">
                                                                                                    <label for="checkprosuccess2a" class="checkbox_label">Probability of success</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkprofailure2a" class="checkbox">
                                                                                                    <label for="checkprofailure2a" class="checkbox_label">Probability of Failure</label>
                                                                                                </div>
                                                                                            </li>
                                                                                        </ul>
                                                                                    </div>
                                                                                    <div class="col-12 col-md-6">
                                                                                        <ul class="dropdown-menu-info dropdown-menu-info-two main-dropdown-menu-info">
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkcost2a" class="checkbox">
                                                                                                    <label for="checkcost2a" class="checkbox_label">Cost</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkgroupinfo2a" class="checkbox">
                                                                                                    <label for="checkgroupinfo2a" class="checkbox_label">Group</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="Checkpartialinfo2a" class="checkbox">
                                                                                                    <label for="Checkpartialinfo2a" class="checkbox_label">Partial</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkmininfo2a" class="checkbox">
                                                                                                    <label for="checkmininfo2a" class="checkbox_label">Min%</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkmustinfo2a" class="checkbox">
                                                                                                    <label for="checkmustinfo2a" class="checkbox_label">Must</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkmustnot2a" class="checkbox">
                                                                                                    <label for="checkmustnot2a" class="checkbox_label">Must not</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkcustomeconstraints2a" class="checkbox">
                                                                                                    <label for="checkcustomeconstraints2a" class="checkbox_label">[Custom Constraints]</label>
                                                                                                </div>
                                                                                            </li>
                                                                                            <li>
                                                                                                <div class="form-group">
                                                                                                    <input type="checkbox" id="checkaltattributes2a" class="checkbox">
                                                                                                    <label for="checkaltattributes2a" class="checkbox_label">[Alt. Attributes]</label>
                                                                                                </div>
                                                                                            </li>
                                                                                        </ul>
                                                                                    </div>
                                                                                    <div class="col-12 col-md-12">
                                                                                        <div class="row align-items-center dropdown-show-hide-all-wrapper">
                                                                                            <div class="col-12 col-md-6">
                                                                                                <ul class="dropdown-menu-info dropdown-show-hide-all-info d-flex">
                                                                                                    <li>
                                                                                                        <div class="form-group">
                                                                                                            <input type="checkbox" id="checkshowall2a" class="checkbox">
                                                                                                            <label for="checkshowall2a" class="checkbox_label">Show all</label>
                                                                                                        </div>
                                                                                                    </li>
                                                                                                    <li>
                                                                                                        <div class="form-group">
                                                                                                            <input type="checkbox" id="checkhideall2a" class="checkbox">
                                                                                                            <label for="checkhideall2a" class="checkbox_label">Hide all</label>
                                                                                                        </div>
                                                                                                    </li>
                                                                                                </ul>
                                                                                            </div>
                                                                                            <div class="col-12 col-md-6">
                                                                                                <div class="applyall-cta-info text-end">
                                                                                                    <button type="submit" class="btn prm_green_btn">Apply</button>
                                                                                                </div>
                                                                                            </div>
                                                                                        </div>
                                                                                    </div>

                                                                                </div>
                                                                            </form>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                                <div class="input-group search_div">
                                                                    <span class="input-group-text" id="basic-addon1z">
                                                                        <img src="./../Images/img/icon/Search.svg"></span>
                                                                    <input type="text" class="form-control" placeholder="Search" aria-label="Username" aria-describedby="basic-addon1">
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="manage-model-archives-content-info">
                                                        <div class="table-responsive participant_table">
                                                            <table class="table table-striped-info">
                                                                <thead>
                                                                    <tr>
                                                                        <th class="text_blue" style="width: 4%;">
                                                                            <div class="form-group">
                                                                                <input type="checkbox" id="emailcheck1a" class="checkbox">
                                                                                <label for="emailcheck1a" class="checkbox_label"></label>
                                                                            </div>
                                                                        </th>
                                                                        <th class="text_blue" style="width: 4%;">
                                                                            <div class="stars-icon-info">
                                                                                <a href="#" class="star-link-info">
                                                                                    <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="bordered-star-icon d-none">
                                                                                    <img src="../Images/img/icon/star-yellow-icon.svg" alt="filled-star-icon" class="filled-star-icon">
                                                                                </a>
                                                                            </div>
                                                                        </th>
                                                                        <th class="text_blue" style="width: 72%;">Model name
                                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                            <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                        </th>

                                                                        <th class="text_blue" style="width: 20%;">Last Access
                                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                            <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                        </th>
                                                                    </tr>
                                                                </thead>
                                                                <tbody>
                                                                    <tr>
                                                                        <td>
                                                                            <div class="form-group">
                                                                                <input type="checkbox" id="html1" class="checkbox">
                                                                                <label for="html1" class="checkbox_label"></label>
                                                                            </div>
                                                                        </td>
                                                                        <td>
                                                                            <div class="stars-icon-info">
                                                                                <a href="#" class="star-link-info">
                                                                                    <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="bordered-star-icon">
                                                                                </a>
                                                                            </div>
                                                                        </td>
                                                                        <td>Perception of Relative Importance of Select Department
                                                                        <div class="vertical-dots-wrapper" id="verticaldotlinkone">
                                                                            <div class="icon-info">
                                                                                <a href="#" class="vertical-dot-link-info">
                                                                                    <img src="../Images/img/icon/vertical-dots-icon.svg">
                                                                                </a>
                                                                            </div>

                                                                        </div>
                                                                            <div class="vertical-table-data-info" id="openverticaldotlinkone">
                                                                                <ul class="vertical-table-data-ul">
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/open-model-icon.svg" alt="open-model"></div>
                                                                                            Open Model
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/model-details-icon.svg" alt="model-details-icon"></div>
                                                                                            Model Details
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/save-as-icon.svg" alt="save-as-icon"></div>
                                                                                            Save as
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/activate-model-icon.svg" alt="activate-model-icon"></div>
                                                                                            Activate model
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/download-icon.svg" alt="download-icon"></div>
                                                                                            Download
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/delete-model-icon.svg" alt="delete-model-icon"></div>
                                                                                            Delete
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/create-temp-icon.svg" alt="create-temp-icon"></div>
                                                                                            Create Template or Default Option Set
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/model-logs-icon.svg" alt="model-logs-icon"></div>
                                                                                            Model Logs
                                                                                        </a>
                                                                                    </li>
                                                                                </ul>
                                                                            </div>
                                                                        </td>

                                                                        <td>4/5/2022, 2:23:02 AM</td>

                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            <div class="form-group">
                                                                                <input type="checkbox" id="html2a" class="checkbox">
                                                                                <label for="html2a" class="checkbox_label"></label>
                                                                            </div>
                                                                        </td>
                                                                        <td>
                                                                            <div class="stars-icon-info">
                                                                                <a href="#" class="star-link-info">
                                                                                    <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="bordered-star-icon">
                                                                                </a>
                                                                            </div>
                                                                        </td>
                                                                        <td>Perception of Relative Importance of Select Department
                                                                        <div class="vertical-dots-wrapper" id="verticaldotlinktwo">
                                                                            <div class="icon-info">
                                                                                <a href="#" class="vertical-dot-link-info">
                                                                                    <img src="../Images/img/icon/vertical-dots-icon.svg">
                                                                                </a>
                                                                            </div>

                                                                        </div>
                                                                            <div class="vertical-table-data-info">
                                                                                <ul class="vertical-table-data-ul">
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/open-model-icon.svg" alt="open-model"></div>
                                                                                            Open Model
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/model-details-icon.svg" alt="model-details-icon"></div>
                                                                                            Model Details
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/save-as-icon.svg" alt="save-as-icon"></div>
                                                                                            Save as
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/activate-model-icon.svg" alt="activate-model-icon"></div>
                                                                                            Activate model
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/download-icon.svg" alt="download-icon"></div>
                                                                                            Download
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/delete-model-icon.svg" alt="delete-model-icon"></div>
                                                                                            Delete
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/create-temp-icon.svg" alt="create-temp-icon"></div>
                                                                                            Create Template or Default Option Set
                                                                                        </a>
                                                                                    </li>
                                                                                    <li class="vertical-li-info">
                                                                                        <a href="#" class="link-info">
                                                                                            <div class="icon-info">
                                                                                                <img src="../Images/img/icon/model-logs-icon.svg" alt="model-logs-icon"></div>
                                                                                            Model Logs
                                                                                        </a>
                                                                                    </li>
                                                                                </ul>
                                                                            </div>

                                                                        </td>

                                                                        <td>4/5/2022, 2:23:02 AM</td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="pagination-bottom-wrapper">
                                                    <div class="pagination-bottom-info d-flex">
                                                        <div class="pagi-left">
                                                            <ul class="pagi-ul d-flex">
                                                                <li class="pagi-li"><a href="#" class="pagi-link active">10</a></li>
                                                                <li class="pagi-li"><a href="#" class="pagi-link">25</a></li>
                                                                <li class="pagi-li"><a href="#" class="pagi-link">50</a></li>
                                                                <li class="pagi-li"><a href="#" class="pagi-link">100</a></li>
                                                            </ul>
                                                        </div>
                                                        <div class="pagi-right">
                                                            <ul class="pagi-ul d-flex">
                                                                <li>
                                                                    <div class="pagination-data-wrap">
                                                                        <ul class="pagi-data-ul">
                                                                            <li class="pagi-prev-data-info">Page <span class="pagi-prev-data">1</span></li>
                                                                            <li class="pagi-next-data-info">of <span class="pagi-next-data">3</span><span class="pagi-remaining-data"> (124 items)</span></li>
                                                                        </ul>
                                                                    </div>
                                                                </li>
                                                                <li class="pagi-li pagi-prev-info"><a href="#" class="pagi-link">
                                                                    <img src="../Images/img/icon/left-arrow-icon.svg" alt="left-arrow">
                                                                </a></li>
                                                                <li class="pagi-li"><a href="#" class="pagi-link active">1</a></li>
                                                                <li class="pagi-li pagi-next-info"><a href="#" class="pagi-link">
                                                                    <img src="../Images/img/icon/right-arrow-icon.svg" alt="right-arrow"></a></li>
                                                            </ul>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <!-- End -->
                            </div>
                        </div>
                    </div>
                </div>

                <div class="tab-pane fade" id="templates" role="tabpanel" aria-labelledby="templates-tab">
                    <div class="manage-model-template-wrapper collect-input-page-wrapper">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="breadcrumb-wrapper">
                                    <nav aria-label="breadcrumb">
                                        <ol class="breadcrumb">
                                            <li class="breadcrumb-item">
                                                <a href="collect_input_dashboard.php">Home</a>
                                            </li>
                                            <li class="breadcrumb-item active" aria-current="page">Manage Models Templates</li>
                                        </ol>
                                    </nav>
                                </div>
                            </div>
                            <!-- Manage Model page  -->
                            <div class="col-md-12">
                                <div class="header-content-wrapper d-flex jus-con-sb align-center-info">
                                    <div class="head-title-wrapper">
                                        <h2 class="head-title-info col-blue">Manage Models Templates</h2>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="manage-model-template-main-wrapper">
                                    <div class="manage-model-template-main-info">


                                        <div class="manage-model-template-table-wrapper table-wb-shadow-info">
                                            <div class="manage-model-template-table-info">
                                                <div class="pages-table-head-info d-flex">
                                                    <div class="search-and-columns-info cus-wid-100 d-flex jus-con-sb">
                                                        <div class="table-header-ctas-wrapper d-flex align-items-center">
                                                            <div class="form-group a-link-wrapper">
                                                                <input type="checkbox" id="html1allza" class="checkbox">
                                                                <label for="html1allza" class="checkbox_label"></label>
                                                            </div>
                                                            <div class="deleteall-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/img/icon/delete-btn.svg" alt="delete-all">
                                                                </a>
                                                            </div>
                                                            <div class="copyall-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/img/icon/Copy-square-icon.svg" alt="copy-all">
                                                                </a>
                                                            </div>
                                                            <div class="cloud-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/img/icon/colud-database-icon.svg" alt="cloud-data">
                                                                </a>
                                                            </div>
                                                            <div class="add-new-model-info a-link-wrapper">
                                                                <a href="#" class="btn prm_green_btn" data-bs-toggle="modal" data-bs-target="#create_new_template">
                                                                    <img src="../Images/img/Add_white.svg" class="me-2">
                                                                    Add Template</a>
                                                            </div>

                                                        </div>
                                                        <div class="table_setting align-center-info">
                                                            <div class="cta-export-info">
                                                                <a href="#" class="cta-link-info remove-all-info">
                                                                    <img src="../Images/img/icon/refresh-rectangle.svg" alt="refresh-link">
                                                                </a>
                                                            </div>
                                                            <div class="cta-export-info">
                                                                <a href="#" class="cta-link-info remove-all-info">
                                                                    <img src="../Images/img/icon/export-rectangle.svg" alt="export-link">
                                                                </a>
                                                            </div>
                                                            <div class="btn-group mr-10" role="group" aria-label="Button group with nested dropdown">
                                                                <div class="btn-group" role="group">
                                                                    <button id="btnGroupDrop1c" type="button" class="btn choose-column-btn-info dropdown-toggle">
                                                                        <img src="./../Images/img/icon/Columns.svg" class="me-2">
                                                                        Choose columns
                                                            <img src="../Images/img/icon/down-arrow-bordered-icon.svg" class="down-arrow-icon" alt="down-arrow">
                                                                    </button>
                                                                    <div class="dropdown-menu-choose-options-info">
                                                                        <form action="" method="post">
                                                                            <div class="row">
                                                                                <div class="col-12 col-md-6">
                                                                                    <ul class="dropdown-menu-info dropdown-menu-info-one main-dropdown-menu-info">
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkid2abc" class="checkbox">
                                                                                                <label for="checkid2abc" class="checkbox_label">ID</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprojects2abc" class="checkbox">
                                                                                                <label for="checkprojects2abc" class="checkbox_label">Projects</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="Checkfunded2abc" class="checkbox">
                                                                                                <label for="Checkfunded2abc" class="checkbox_label">Funded</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkfundedprojects2abc" class="checkbox">
                                                                                                <label for="checkfundedprojects2abc" class="checkbox_label">Finish data of funded project</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkbenefit2abc" class="checkbox">
                                                                                                <label for="checkbenefit2abc" class="checkbox_label">Benefit</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkexpbenefits2abc" class="checkbox">
                                                                                                <label for="checkexpbenefits2abc" class="checkbox_label">Expected benefit</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkrisk2abc" class="checkbox">
                                                                                                <label for="checkrisk2abc" class="checkbox_label">Risk</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprosuccess2abc" class="checkbox">
                                                                                                <label for="checkprosuccess2abc" class="checkbox_label">Probability of success</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprofailure2abc" class="checkbox">
                                                                                                <label for="checkprofailure2abc" class="checkbox_label">Probability of Failure</label>
                                                                                            </div>
                                                                                        </li>
                                                                                    </ul>
                                                                                </div>
                                                                                <div class="col-12 col-md-6">
                                                                                    <ul class="dropdown-menu-info dropdown-menu-info-two main-dropdown-menu-info">
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkcost2abc" class="checkbox">
                                                                                                <label for="checkcost2abc" class="checkbox_label">Cost</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkgroupinfo2abc" class="checkbox">
                                                                                                <label for="checkgroupinfo2abc" class="checkbox_label">Group</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="Checkpartialinfo2abc" class="checkbox">
                                                                                                <label for="Checkpartialinfo2abc" class="checkbox_label">Partial</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmininfo2abc" class="checkbox">
                                                                                                <label for="checkmininfo2abc" class="checkbox_label">Min%</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmustinfo2abc" class="checkbox">
                                                                                                <label for="checkmustinfo2abc" class="checkbox_label">Must</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmustnot2abc" class="checkbox">
                                                                                                <label for="checkmustnot2abc" class="checkbox_label">Must not</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkcustomeconstraints2abc" class="checkbox">
                                                                                                <label for="checkcustomeconstraints2abc" class="checkbox_label">[Custom Constraints]</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkaltattributes2abc" class="checkbox">
                                                                                                <label for="checkaltattributes2abc" class="checkbox_label">[Alt. Attributes]</label>
                                                                                            </div>
                                                                                        </li>
                                                                                    </ul>
                                                                                </div>
                                                                                <div class="col-12 col-md-12">
                                                                                    <div class="row align-items-center dropdown-show-hide-all-wrapper">
                                                                                        <div class="col-12 col-md-6">
                                                                                            <ul class="dropdown-menu-info dropdown-show-hide-all-info d-flex">
                                                                                                <li>
                                                                                                    <div class="form-group">
                                                                                                        <input type="checkbox" id="checkshowall2abc" class="checkbox">
                                                                                                        <label for="checkshowall2abc" class="checkbox_label">Show all</label>
                                                                                                    </div>
                                                                                                </li>
                                                                                                <li>
                                                                                                    <div class="form-group">
                                                                                                        <input type="checkbox" id="checkhideall2abc" class="checkbox">
                                                                                                        <label for="checkhideall2abc" class="checkbox_label">Hide all</label>
                                                                                                    </div>
                                                                                                </li>
                                                                                            </ul>
                                                                                        </div>
                                                                                        <div class="col-12 col-md-6">
                                                                                            <div class="applyall-cta-info text-end">
                                                                                                <button type="submit" class="btn prm_green_btn">Apply</button>
                                                                                            </div>
                                                                                        </div>
                                                                                    </div>
                                                                                </div>

                                                                            </div>
                                                                        </form>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="input-group search_div">
                                                                <span class="input-group-text" id="basic-addon1za">
                                                                    <img src="./../Images/img/icon/Search.svg"></span>
                                                                <input type="text" class="form-control" placeholder="Search" aria-label="Username" aria-describedby="basic-addon1">
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="manage-model-template-content-info">
                                                    <div class="table-responsive participant_table">
                                                        <table class="table table-striped-info">
                                                            <thead>
                                                                <tr>
                                                                    <th class="text_blue" style="width: 4%;">
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="emailcheck1a" class="checkbox">
                                                                            <label for="emailcheck1a" class="checkbox_label"></label>
                                                                        </div>
                                                                    </th>
                                                                    <th class="text_blue" style="width: 4%;">
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="bordered-star-icon d-none">
                                                                                <img src="../Images/img/icon/star-yellow-icon.svg" alt="filled-star-icon" class="filled-star-icon">
                                                                            </a>
                                                                        </div>
                                                                    </th>
                                                                    <th class="text_blue">Template name
                                                            <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>

                                                                    <th class="text_blue">Last Access
                                                            <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Model type
                                                            <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Has survey
                                                            <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Creater
                                                            <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Last modified
                                                            <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                                <tr>
                                                                    <td>
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="html1" class="checkbox">
                                                                            <label for="html1" class="checkbox_label"></label>
                                                                        </div>
                                                                    </td>
                                                                    <td>
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="bordered-star-icon">
                                                                            </a>
                                                                        </div>
                                                                    </td>
                                                                    <td>Perception of Relative Importance of Select Department
                                                            <div class="vertical-dots-wrapper" id="verticaldotlinktwo2">
                                                                <div class="icon-info">
                                                                    <a href="#" class="vertical-dot-link-info">
                                                                        <img src="../Images/img/icon/vertical-dots-icon.svg">
                                                                    </a>
                                                                </div>

                                                            </div>
                                                                        <div class="vertical-table-data-info" id="openverticaldotlinktwo2">
                                                                            <ul class="vertical-table-data-ul">
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/open-model-icon.svg" alt="open-model"></div>
                                                                                        Open Model
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-details-icon.svg" alt="model-details-icon"></div>
                                                                                        Model Details
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/save-as-icon.svg" alt="save-as-icon"></div>
                                                                                        Save as
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/activate-model-icon.svg" alt="activate-model-icon"></div>
                                                                                        Activate model
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/download-icon.svg" alt="download-icon"></div>
                                                                                        Download
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/delete-model-icon.svg" alt="delete-model-icon"></div>
                                                                                        Delete
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/create-temp-icon.svg" alt="create-temp-icon"></div>
                                                                                        Create Template or Default Option Set
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-logs-icon.svg" alt="model-logs-icon"></div>
                                                                                        Model Logs
                                                                                    </a>
                                                                                </li>
                                                                            </ul>
                                                                        </div>
                                                                    </td>

                                                                    <td>4/5/2022, 2:23:02 AM</td>
                                                                    <td>Regular</td>
                                                                    <td>Beginning</td>
                                                                    <td>Beginning</td>
                                                                    <td>4/5/2022, 2:23:02 AM</td>

                                                                </tr>
                                                                <tr>
                                                                    <td>
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="html2a" class="checkbox">
                                                                            <label for="html2a" class="checkbox_label"></label>
                                                                        </div>
                                                                    </td>
                                                                    <td>
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="bordered-star-icon">
                                                                            </a>
                                                                        </div>
                                                                    </td>
                                                                    <td>Perception of Relative Importance of Select Department
                                                            <div class="vertical-dots-wrapper" id="verticaldotlinktwo">
                                                                <div class="icon-info">
                                                                    <a href="#" class="vertical-dot-link-info">
                                                                        <img src="../Images/img/icon/vertical-dots-icon.svg">
                                                                    </a>
                                                                </div>

                                                            </div>
                                                                        <div class="vertical-table-data-info">
                                                                            <ul class="vertical-table-data-ul">
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/open-model-icon.svg" alt="open-model"></div>
                                                                                        Open Model
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-details-icon.svg" alt="model-details-icon"></div>
                                                                                        Model Details
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/save-as-icon.svg" alt="save-as-icon"></div>
                                                                                        Save as
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/activate-model-icon.svg" alt="activate-model-icon"></div>
                                                                                        Activate model
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/download-icon.svg" alt="download-icon"></div>
                                                                                        Download
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/delete-model-icon.svg" alt="delete-model-icon"></div>
                                                                                        Delete
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/create-temp-icon.svg" alt="create-temp-icon"></div>
                                                                                        Create Template or Default Option Set
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-logs-icon.svg" alt="model-logs-icon"></div>
                                                                                        Model Logs
                                                                                    </a>
                                                                                </li>
                                                                            </ul>
                                                                        </div>

                                                                    </td>

                                                                    <td>4/5/2022, 2:23:02 AM</td>
                                                                    <td>Regular</td>
                                                                    <td>Beginning</td>
                                                                    <td>Beginning</td>
                                                                    <td>4/5/2022, 2:23:02 AM</td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="pagination-bottom-wrapper">
                                                <div class="pagination-bottom-info d-flex">
                                                    <div class="pagi-left">
                                                        <ul class="pagi-ul d-flex">
                                                            <li class="pagi-li"><a href="#" class="pagi-link active">10</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">25</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">50</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">100</a></li>
                                                        </ul>
                                                    </div>
                                                    <div class="pagi-right">
                                                        <ul class="pagi-ul d-flex">
                                                            <li>
                                                                <div class="pagination-data-wrap">
                                                                    <ul class="pagi-data-ul">
                                                                        <li class="pagi-prev-data-info">Page <span class="pagi-prev-data">1 </span></li>
                                                                        <li class="pagi-next-data-info">of <span class="pagi-next-data">3 </span><span class="pagi-remaining-data">(124 items)</span></li>
                                                                    </ul>
                                                                </div>
                                                            </li>
                                                            <li class="pagi-li pagi-prev-info"><a href="#" class="pagi-link">
                                                                <img src="../Images/img/icon/left-arrow-icon.svg" alt="left-arrow">
                                                            </a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link active">1</a></li>
                                                            <li class="pagi-li pagi-next-info"><a href="#" class="pagi-link">
                                                                <img src="../Images/img/icon/right-arrow-icon.svg" alt="right-arrow"></a></li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- End -->
                        </div>
                    </div>
                </div>

                <div class="tab-pane fade" id="favorites" role="tabpanel" aria-labelledby="favorites-tab">
                    <div class="manage-model-page-wrapper collect-input-page-wrapper">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="breadcrumb-wrapper">
                                    <nav aria-label="breadcrumb">
                                        <ol class="breadcrumb">
                                            <li class="breadcrumb-item">
                                                <a href="collect_input_dashboard.php">Home</a>
                                            </li>
                                            <li class="breadcrumb-item active" aria-current="page">Manage Models</li>
                                        </ol>
                                    </nav>
                                </div>
                            </div>
                            <!-- Manage Model page  -->
                            <div class="col-md-12">
                                <div class="header-content-wrapper d-flex jus-con-sb align-center-info">
                                    <div class="head-title-wrapper">
                                        <h2 class="head-title-info col-blue">Manage Models </h2>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="manage-model-page-main-wrapper">
                                    <div class="manage-model-page-main-info">


                                        <div class="manage-model-page-table-wrapper table-wb-shadow-info">
                                            <div class="manage-model-page-table-info">
                                                <div class="pages-table-head-info d-flex">
                                                    <div class="search-and-columns-info cus-wid-100 d-flex jus-con-sb">
                                                        <div class="table-header-ctas-wrapper d-flex align-items-center">
                                                            <div class="form-group a-link-wrapper">
                                                                <input type="checkbox" id="html1allza" class="checkbox">
                                                                <label for="html1allza" class="checkbox_label"></label>
                                                            </div>
                                                            <div class="deleteall-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/img/icon/delete-btn.svg" alt="delete-all">
                                                                </a>
                                                            </div>
                                                            <div class="copyall-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/img/icon/Copy-square-icon.svg" alt="copy-all">
                                                                </a>
                                                            </div>
                                                            <div class="cloud-data-info a-link-wrapper">
                                                                <a href="#" class="a-link-info">
                                                                    <img src="../Images/img/icon/colud-database-icon.svg" alt="cloud-data">
                                                                </a>
                                                            </div>
                                                            <div class="add-new-model-info a-link-wrapper">
                                                                <a href="#" class="btn prm_green_btn" data-bs-toggle="modal" data-bs-target="#add_new_modal">
                                                                    <img src="../Images/img/Add_white.svg" class="me-2">
                                                                    Add Model</a>
                                                            </div>
                                                            <!--
                                        <div class="sort-data-info a-link-wrapper">
                                            <a href="#" class="btn a-link-info">
                                                <img src="../Images/img/icon/star-yellow-icon.svg" class="me-1"  alt="sort-data"> Favorites
                                            </a>
                                        </div>
-->

                                                        </div>
                                                        <div class="table_setting align-center-info">
                                                            <div class="cta-export-info">
                                                                <a href="#" class="cta-link-info remove-all-info">
                                                                    <img src="../Images/img/icon/refresh-rectangle.svg" alt="refresh-link">
                                                                </a>
                                                            </div>
                                                            <div class="cta-export-info">
                                                                <a href="#" class="cta-link-info remove-all-info">
                                                                    <img src="../Images/img/icon/export-rectangle.svg" alt="export-link">
                                                                </a>
                                                            </div>
                                                            <div class="btn-group mr-10" role="group" aria-label="Button group with nested dropdown">
                                                                <div class="btn-group" role="group">
                                                                    <button id="btnGroupDrop1c" type="button" class="btn choose-column-btn-info dropdown-toggle">
                                                                        <img src="./../Images/img/icon/Columns.svg" class="me-2">
                                                                        Choose columns
                                                                    <img src="../Images/img/icon/down-arrow-bordered-icon.svg" class="down-arrow-icon" alt="down-arrow">
                                                                    </button>
                                                                    <div class="dropdown-menu-choose-options-info">
                                                                        <form action="" method="post">
                                                                            <div class="row">
                                                                                <div class="col-12 col-md-6">
                                                                                    <ul class="dropdown-menu-info dropdown-menu-info-one main-dropdown-menu-info">
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkid2ab" class="checkbox">
                                                                                                <label for="checkid2ab" class="checkbox_label">ID</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprojects2ab" class="checkbox">
                                                                                                <label for="checkprojects2ab" class="checkbox_label">Projects</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="Checkfunded2ab" class="checkbox">
                                                                                                <label for="Checkfunded2ab" class="checkbox_label">Funded</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkfundedprojects2ab" class="checkbox">
                                                                                                <label for="checkfundedprojects2ab" class="checkbox_label">Finish data of funded project</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkbenefit2ab" class="checkbox">
                                                                                                <label for="checkbenefit2ab" class="checkbox_label">Benefit</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkexpbenefits2ab" class="checkbox">
                                                                                                <label for="checkexpbenefits2ab" class="checkbox_label">Expected benefit</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkrisk2ab" class="checkbox">
                                                                                                <label for="checkrisk2ab" class="checkbox_label">Risk</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprosuccess2ab" class="checkbox">
                                                                                                <label for="checkprosuccess2ab" class="checkbox_label">Probability of success</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkprofailure2ab" class="checkbox">
                                                                                                <label for="checkprofailure2ab" class="checkbox_label">Probability of Failure</label>
                                                                                            </div>
                                                                                        </li>
                                                                                    </ul>
                                                                                </div>
                                                                                <div class="col-12 col-md-6">
                                                                                    <ul class="dropdown-menu-info dropdown-menu-info-two main-dropdown-menu-info">
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkcost2ab" class="checkbox">
                                                                                                <label for="checkcost2ab" class="checkbox_label">Cost</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkgroupinfo2ab" class="checkbox">
                                                                                                <label for="checkgroupinfo2ab" class="checkbox_label">Group</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="Checkpartialinfo2ab" class="checkbox">
                                                                                                <label for="Checkpartialinfo2ab" class="checkbox_label">Partial</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmininfo2ab" class="checkbox">
                                                                                                <label for="checkmininfo2ab" class="checkbox_label">Min%</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmustinfo2ab" class="checkbox">
                                                                                                <label for="checkmustinfo2ab" class="checkbox_label">Must</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkmustnot2ab" class="checkbox">
                                                                                                <label for="checkmustnot2ab" class="checkbox_label">Must not</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkcustomeconstraints2ab" class="checkbox">
                                                                                                <label for="checkcustomeconstraints2ab" class="checkbox_label">[Custom Constraints]</label>
                                                                                            </div>
                                                                                        </li>
                                                                                        <li>
                                                                                            <div class="form-group">
                                                                                                <input type="checkbox" id="checkaltattributes2ab" class="checkbox">
                                                                                                <label for="checkaltattributes2ab" class="checkbox_label">[Alt. Attributes]</label>
                                                                                            </div>
                                                                                        </li>
                                                                                    </ul>
                                                                                </div>
                                                                                <div class="col-12 col-md-12">
                                                                                    <div class="row align-items-center dropdown-show-hide-all-wrapper">
                                                                                        <div class="col-12 col-md-6">
                                                                                            <ul class="dropdown-menu-info dropdown-show-hide-all-info d-flex">
                                                                                                <li>
                                                                                                    <div class="form-group">
                                                                                                        <input type="checkbox" id="checkshowall2ab" class="checkbox">
                                                                                                        <label for="checkshowall2ab" class="checkbox_label">Show all</label>
                                                                                                    </div>
                                                                                                </li>
                                                                                                <li>
                                                                                                    <div class="form-group">
                                                                                                        <input type="checkbox" id="checkhideall2ab" class="checkbox">
                                                                                                        <label for="checkhideall2ab" class="checkbox_label">Hide all</label>
                                                                                                    </div>
                                                                                                </li>
                                                                                            </ul>
                                                                                        </div>
                                                                                        <div class="col-12 col-md-6">
                                                                                            <div class="applyall-cta-info text-end">
                                                                                                <button type="submit" class="btn prm_green_btn">Apply</button>
                                                                                            </div>
                                                                                        </div>
                                                                                    </div>
                                                                                </div>

                                                                            </div>
                                                                        </form>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="input-group search_div">
                                                                <span class="input-group-text" id="basic-addon1z">
                                                                    <img src="./../Images/img/icon/Search.svg"></span>
                                                                <input type="text" class="form-control" placeholder="Search" aria-label="Username" aria-describedby="basic-addon1">
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="manage-model-page-content-info">
                                                    <div class="table-responsive participant_table">
                                                        <table class="table table-striped-info">
                                                            <thead>
                                                                <tr>
                                                                    <th class="text_blue" style="width: 4%;">
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="emailcheck1a" class="checkbox">
                                                                            <label for="emailcheck1a" class="checkbox_label"></label>
                                                                        </div>
                                                                    </th>
                                                                    <th class="text_blue" style="width: 4%;">
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info th-star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="unfilled-star-icon switch-star-wrap d-none">
                                                                                <img src="../Images/img/icon/star-yellow-icon.svg" alt="filled-star-icon" class="filled-star-icon switch-star-wrap">
                                                                            </a>
                                                                        </div>
                                                                    </th>
                                                                    <th class="text_blue">Model name
                                                                    <div class="filter-icons-info d-none"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                    </th>

                                                                    <th class="text_blue">Last Access
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Online
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Status
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Model type
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Your role
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Has survey
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                    <th class="text_blue">Creater
                                                                    <div class="filter-icons-info"><a href="#" class="filter-link">
                                                                        <img src="../Images/img/icon/filter-icon.svg" alt="filter-icon"></a></div>
                                                                    </th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                                <tr>
                                                                    <td>
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="html1" class="checkbox">
                                                                            <label for="html1" class="checkbox_label"></label>
                                                                        </div>
                                                                    </td>
                                                                    <td>
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="unfilled-star-icon td-switch-star-wrap switch-star-wrap d-none">
                                                                                <img src="../Images/img/icon/star-yellow-icon.svg" alt="filled-star-icon" class="filled-star-icon td-switch-star-wrap switch-star-wrap">
                                                                            </a>
                                                                        </div>
                                                                    </td>
                                                                    <td>Perception of Relative Importance of Select Department
                                                                    <div class="vertical-dots-wrapper" id="verticaldotlinkone1">
                                                                        <div class="icon-info">
                                                                            <a href="#" class="vertical-dot-link-info">
                                                                                <img src="../Images/img/icon/vertical-dots-icon.svg">
                                                                            </a>
                                                                        </div>

                                                                    </div>
                                                                        <div class="vertical-table-data-info" id="openverticaldotlinkone1">
                                                                            <ul class="vertical-table-data-ul">
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/eye-closed-icon.svg" alt="close-model"></div>
                                                                                        Close Model
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-details-icon.svg" alt="model-details-icon"></div>
                                                                                        Model Details
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/save-as-icon.svg" alt="save-as-icon"></div>
                                                                                        Save as
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/archive-icon.svg" alt="archives-icon"></div>
                                                                                        Archives
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/download-icon.svg" alt="download-icon"></div>
                                                                                        Download
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/delete-model-icon.svg" alt="delete-model-icon"></div>
                                                                                        Delete
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-details-icon.svg" alt="collect-my-icon"></div>
                                                                                        Collect my input
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-logs-icon.svg" alt="delete-model-icon"></div>
                                                                                        Evaluation Status
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/Add participants.svg" alt="delete-model-icon"></div>
                                                                                        Participants
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/Export.svg" alt="export-model-icon"></div>
                                                                                        Get model link and copy to clipboard
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/get-link-icon.svg" alt="getlink-model-icon"></div>
                                                                                        Get links
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/law-icon.svg" alt="allocate-model-icon"></div>
                                                                                        Allocate
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/pie-chart-icon.svg" alt="delete-model-icon"></div>
                                                                                        Results
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/dashboard-icons.svg" alt="dashboard-model-icon"></div>
                                                                                        Dashboard
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/mail-icon.svg" alt="send-invitaion-model-icon"></div>
                                                                                        Send Invitation
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/create-temp-icon.svg" alt="create-temp-icon"></div>
                                                                                        Create Template or Default Option Set
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/photo-camera-icon.svg" alt="snap-model-icon"></div>
                                                                                        Model snapshots
                                                                                    </a>
                                                                                </li>
                                                                                <li class="vertical-li-info">
                                                                                    <a href="#" class="link-info">
                                                                                        <div class="icon-info">
                                                                                            <img src="../Images/img/icon/model-logs-icon.svg" alt="model-logs-icon"></div>
                                                                                        Model Logs
                                                                                    </a>
                                                                                </li>
                                                                            </ul>
                                                                        </div>
                                                                    </td>

                                                                    <td>4/5/2022, 2:23:02 AM</td>
                                                                    <td>
                                                                        <div class="manage-model-toggle-wrapper">
                                                                            <div class="toggle-on-off-btn-info">
                                                                                <label class="toggle-switch-info">
                                                                                    <input type="checkbox" id="togglebtn1a" checked="">
                                                                                    <span class="toggle-slider-info"></span>
                                                                                </label>
                                                                            </div>
                                                                        </div>
                                                                    </td>
                                                                    <td>Available</td>
                                                                    <td>Regular</td>
                                                                    <td>Project Manager</td>
                                                                    <td>Beginning</td>
                                                                    <td>Beginning</td>

                                                                </tr>
                                                                <tr>
                                                                    <td>
                                                                        <div class="form-group">
                                                                            <input type="checkbox" id="html2a" class="checkbox">
                                                                            <label for="html2a" class="checkbox_label"></label>
                                                                        </div>
                                                                    </td>
                                                                    <td>
                                                                        <div class="stars-icon-info">
                                                                            <a href="#" class="star-link-info">
                                                                                <img src="../Images/img/icon/bordered-star-icon.svg" alt="bordered-star-icon" class="unfilled-star-icon td-switch-star-wrap switch-star-wrap d-none">
                                                                                <img src="../Images/img/icon/star-yellow-icon.svg" alt="filled-star-icon" class="filled-star-icon td-switch-star-wrap switch-star-wrap">
                                                                            </a>
                                                                        </div>
                                                                    </td>
                                                                    <td>Perception of Relative Importance of Select Department
                                                                    <div class="vertical-dots-wrapper" id="verticaldotlinktwo">
                                                                        <div class="icon-info">
                                                                            <a href="#" class="vertical-dot-link-info">
                                                                                <img src="../Images/img/icon/vertical-dots-icon.svg">
                                                                            </a>
                                                                        </div>

                                                                    </div>


                                                                    </td>

                                                                    <td>4/5/2022, 2:23:02 AM</td>
                                                                    <td>
                                                                        <div class="manage-model-toggle-wrapper">
                                                                            <div class="toggle-on-off-btn-info">
                                                                                <label class="toggle-switch-info">
                                                                                    <input type="checkbox" id="togglebtn2a" checked="">
                                                                                    <span class="toggle-slider-info"></span>
                                                                                </label>
                                                                            </div>
                                                                        </div>
                                                                    </td>
                                                                    <td>Available</td>
                                                                    <td>Regular</td>
                                                                    <td>Project Manager</td>
                                                                    <td>Beginning</td>
                                                                    <td>Beginning</td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="pagination-bottom-wrapper">
                                                <div class="pagination-bottom-info d-flex">
                                                    <div class="pagi-left">
                                                        <ul class="pagi-ul d-flex">
                                                            <li class="pagi-li"><a href="#" class="pagi-link active">10</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">25</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">50</a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link">100</a></li>
                                                        </ul>
                                                    </div>
                                                    <div class="pagi-right">
                                                        <ul class="pagi-ul d-flex">
                                                            <li>
                                                                <div class="pagination-data-wrap">
                                                                    <ul class="pagi-data-ul">
                                                                        <li class="pagi-prev-data-info">Page <span class="pagi-prev-data">1</span></li>
                                                                        <li class="pagi-next-data-info">of <span class="pagi-next-data">3</span><span class="pagi-remaining-data"> (124 items)</span></li>
                                                                    </ul>
                                                                </div>
                                                            </li>
                                                            <li class="pagi-li pagi-prev-info"><a href="#" class="pagi-link">
                                                                <img src="../Images/img/icon/left-arrow-icon.svg" alt="left-arrow">
                                                            </a></li>
                                                            <li class="pagi-li"><a href="#" class="pagi-link active">1</a></li>
                                                            <li class="pagi-li pagi-next-info"><a href="#" class="pagi-link">
                                                                <img src="../Images/img/icon/right-arrow-icon.svg" alt="right-arrow"></a></li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- End -->
                        </div>
                    </div>
                </div>

                <div class="tab-pane fade" id="lookup" role="tabpanel" aria-labelledby="lookup-tab">
                    <div class="select-teamtime-participant-wrapper collect-input-page-wrapper">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="breadcrumb-wrapper">
                                    <nav aria-label="breadcrumb">
                                        <ol class="breadcrumb">
                                            <li class="breadcrumb-item">
                                                <a href="collect_input_dashboard.php">Home</a>
                                            </li>
                                            <li class="breadcrumb-item active" aria-current="page">Manage Models Lookup</li>
                                        </ol>
                                    </nav>
                                </div>
                            </div>
                            <!-- Select Participant  -->
                            <div class="col-md-12">
                                <div class="header-content-wrapper d-flex jus-con-sb align-center-info">
                                    <div class="head-title-wrapper">
                                        <h2 class="head-title-info col-blue">Model Lookup [IT Portfolio Optimizatio…] – Comparion®</h2>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="select-participants-wrapper">
                                    <div class="select-participants-info">


                                        <div class="select-participants-table-wrapper table-wb-shadow-info">
                                            <div class="select-participants-table-info">
                                                <div class="select-participants-table-head-info d-flex">
                                                    <div class="search-and-columns-info cus-wid-100 d-flex jus-con-sb justify-content-end align-items-center">
                                                        <div class="col-5"></div>
                                                        <div class="col-8 d-flex align-items-center justify-content-end">
                                                            <div class="input-group search_div mw-100 w-50">
                                                                <span class="input-group-text" id="basic-addon1z">
                                                                    <img src="./../Images/img/icon/Search.svg"></span>
                                                                <input type="text" class="form-control" placeholder="Search model by name, description or access code" aria-label="Username" aria-describedby="basic-addon1">
                                                            </div>
                                                            <div class="checkbox-group d-flex">
                                                                <div class="form-group ms-2">
                                                                    <input type="checkbox" id="html8" class="checkbox">
                                                                    <label for="html8" class="checkbox_label">Group by workgroup</label>
                                                                </div>
                                                                <div class="form-group ms-2">
                                                                    <input type="checkbox" id="html9" class="checkbox">
                                                                    <label for="html9" class="checkbox_label">Show by pages</label>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="select-participants-content-info">
                                                <div class="table-responsive participant_table">
                                                    <table class="table table-striped-info">
                                                        <thead>
                                                            <tr>
                                                                <th class="text_blue ">Model name
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue">Workgroup
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue ">Access Code
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue ">Status
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue">On-line
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue">Created
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue">Visited
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue">Modifid
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            <tr>
                                                                <td>IT Portfolio Optimization ZB (1) </td>
                                                                <td>Model list re-write sandbox.</td>
                                                                <td>2049-7620</td>
                                                                <td>Active </td>
                                                                <td>Yes </td>
                                                                <td>2022-07-01 13:00:49</td>
                                                                <td>2022-07-05 06:02:57</td>
                                                                <td>22022-07-01 13:01:56</td>
                                                            </tr>
                                                            <tr>
                                                                <td>IT Portfolio Optimization ZB (1) </td>
                                                                <td>Model list re-write sandbox.</td>
                                                                <td>2049-7620</td>
                                                                <td>Active </td>
                                                                <td>Yes </td>
                                                                <td>2022-07-01 13:00:49</td>
                                                                <td>2022-07-05 06:02:57</td>
                                                                <td>22022-07-01 13:01:56</td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="pagination-bottom-wrapper">
                                            <div class="pagination-bottom-info d-flex">
                                                <div class="pagi-left">
                                                    <ul class="pagi-ul d-flex">
                                                        <li class="pagi-li"><a href="#" class="pagi-link active">10</a></li>
                                                        <li class="pagi-li"><a href="#" class="pagi-link">25</a></li>
                                                        <li class="pagi-li"><a href="#" class="pagi-link">50</a></li>
                                                        <li class="pagi-li"><a href="#" class="pagi-link">100</a></li>
                                                    </ul>
                                                </div>
                                                <div class="pagi-right">
                                                    <ul class="pagi-ul d-flex">
                                                        <li>
                                                            <div class="pagination-data-wrap">
                                                                <ul class="pagi-data-ul">
                                                                    <li class="pagi-prev-data-info">Page <span class="pagi-prev-data">1</span></li>
                                                                    <li class="pagi-next-data-info">of <span class="pagi-next-data">3</span><span class="pagi-remaining-data"> (124 items)</span></li>
                                                                </ul>
                                                            </div>
                                                        </li>
                                                        <li class="pagi-li pagi-prev-info"><a href="#" class="pagi-link">
                                                            <img src="../Images/img/icon/left-arrow-icon.svg" alt="left-arrow">
                                                        </a></li>
                                                        <li class="pagi-li"><a href="#" class="pagi-link active">1</a></li>
                                                        <li class="pagi-li pagi-next-info"><a href="#" class="pagi-link">
                                                            <img src="../Images/img/icon/right-arrow-icon.svg" alt="right-arrow"></a></li>
                                                    </ul>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- End -->
                    </div>
                </div>

                <div class="tab-pane fade" id="models-access" role="tabpanel" aria-labelledby="models-access-tab">
                    <div class="select-teamtime-participant-wrapper collect-input-page-wrapper">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="breadcrumb-wrapper">
                                    <nav aria-label="breadcrumb">
                                        <ol class="breadcrumb">
                                            <li class="breadcrumb-item">
                                                <a href="collect_input_dashboard.php">Home</a>
                                            </li>
                                            <li class="breadcrumb-item active" aria-current="page">Manage Models Access Log</li>
                                        </ol>
                                    </nav>
                                </div>
                            </div>
                            <!-- Select Participant  -->
                            <div class="col-md-12">
                                <div class="header-content-wrapper d-flex jus-con-sb align-center-info">
                                    <div class="head-title-wrapper">
                                        <h2 class="head-title-info col-blue">Models Access Log [All Methods ZB] - Comparion®</h2>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="select-participants-wrapper">
                                    <div class="select-participants-info">


                                        <div class="select-participants-table-wrapper table-wb-shadow-info">
                                            <div class="select-participants-table-info">
                                                <div class="select-participants-table-head-info d-flex">
                                                    <div class="search-and-columns-info cus-wid-100 d-flex jus-con-sb justify-content-end align-items-center">
                                                        <div class="col-4">
                                                            <div class="d-flex align-items-center">
                                                                <label class="me-2">Show per page</label>
                                                                <select class="form-select bg-white w-auto">
                                                                    <option>Select all</option>
                                                                    <option>6</option>
                                                                </select>
                                                            </div>
                                                        </div>
                                                        <div class="col-8 d-flex align-items-center justify-content-end">
                                                            <div class="input-group search_div mw-100 w-50">
                                                                <span class="input-group-text" id="basic-addon1z">
                                                                    <img src="./../Images/img/icon/Search.svg"></span>
                                                                <input type="text" class="form-control" placeholder="Search model by name, description or access code" aria-label="Username" aria-describedby="basic-addon1">
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="select-participants-content-info">
                                                <div class="table-responsive participant_table statistic-modal_table">
                                                    <table class="table table-striped-info text-nowrap">
                                                        <thead>
                                                            <tr>
                                                                <th class="text_blue text-start">Model name
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue">Access Code
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue ">Model
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue bg-gray">Snapshots
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue border-right">Size
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue text-center bg-gray border-top border-left">Manual
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue  text-center bg-gray border-top">Auto
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue text-center bg-gray border-top border-right">Total
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue">Status
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                                <th class="text_blue">Created
                                                        <div class="filter-icons-info"><a href="#" class="filter-link">
                                                            <img src="../Images/img/shorting_icon.svg" alt="filter-icon"></a></div>
                                                                </th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            <tr>
                                                                <td class="text-start col-blue">All Methods ZB</td>
                                                                <td>5225-9926</td>
                                                                <td>671.0Kb</td>
                                                                <td>10.6Mb	</td>
                                                                <td>11.3Mb	</td>
                                                                <td class="text-center border-left">1</td>
                                                                <td class="text-center">49</td>
                                                                <td class="text-center col-blue border-right">50</td>
                                                                <td>Active</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start col-blue">IT Portfolio Optimization ZB (1)</td>
                                                                <td>2049-7620</td>
                                                                <td>326.6Kb</td>
                                                                <td>305.6Kb</td>
                                                                <td>632.2Kb</td>
                                                                <td class="text-center border-left">0</td>
                                                                <td class="text-center">6</td>
                                                                <td class="text-center col-blue border-right">6</td>
                                                                <td>Archived</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start col-blue">Perception of Relative Importance of Select Department Chairperson Skills - December 2019 (1)</td>
                                                                <td>2049-7620</td>
                                                                <td>326.6Kb</td>
                                                                <td>305.6Kb</td>
                                                                <td>632.2Kb</td>
                                                                <td class="text-center border-left">0</td>
                                                                <td class="text-center">6</td>
                                                                <td class="text-center col-blue border-right">6</td>
                                                                <td>Archived</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start">MultiOmniTech Vendor Source Selection - RB (v 1.1.46)</td>
                                                                <td>5225-9926</td>
                                                                <td>671.0Kb</td>
                                                                <td>10.6Mb	</td>
                                                                <td>11.3Mb	</td>
                                                                <td class="text-center border-left">1</td>
                                                                <td class="text-center">49</td>
                                                                <td class="text-center col-blue border-right">50</td>
                                                                <td>Marked as deleted</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start col-blue">MultiOmniTech Vendor Source Selection (1)<img src="../Images/img/icon/info-close.svg" class="ms-1" /></td>
                                                                <td>7387-2902</td>
                                                                <td>159.4Kb</td>
                                                                <td>3.0Mb</td>
                                                                <td>3.1Mb</td>
                                                                <td class="text-center border-left">1</td>
                                                                <td class="text-center">49</td>
                                                                <td class="text-center col-blue border-right">50</td>
                                                                <td>Archived</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start col-blue">All Methods ZB</td>
                                                                <td>5225-9926</td>
                                                                <td>671.0Kb</td>
                                                                <td>10.6Mb	</td>
                                                                <td>11.3Mb	</td>
                                                                <td class="text-center border-left">1</td>
                                                                <td class="text-center">49</td>
                                                                <td class="text-center col-blue border-right">50</td>
                                                                <td>Active</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start col-blue">IT Portfolio Optimization ZB (1)</td>
                                                                <td>2049-7620</td>
                                                                <td>326.6Kb</td>
                                                                <td>305.6Kb</td>
                                                                <td>632.2Kb</td>
                                                                <td class="text-center border-left">0</td>
                                                                <td class="text-center">6</td>
                                                                <td class="text-center col-blue border-right">6</td>
                                                                <td>Archived</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start col-blue">Perception of Relative Importance of Select Department Chairperson Skills - December 2019 (1)</td>
                                                                <td>2049-7620</td>
                                                                <td>326.6Kb</td>
                                                                <td>305.6Kb</td>
                                                                <td>632.2Kb</td>
                                                                <td class="text-center border-left">0</td>
                                                                <td class="text-center">6</td>
                                                                <td class="text-center col-blue border-right">6</td>
                                                                <td>Archived</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start">MultiOmniTech Vendor Source Selection - RB (v 1.1.46)</td>
                                                                <td>5225-9926</td>
                                                                <td>671.0Kb</td>
                                                                <td>10.6Mb	</td>
                                                                <td>11.3Mb	</td>
                                                                <td class="text-center border-left">1</td>
                                                                <td class="text-center">49</td>
                                                                <td class="text-center col-blue border-right">50</td>
                                                                <td>Marked as deleted</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="text-start col-blue">MultiOmniTech Vendor Source Selection (1)<img src="../Images/img/icon/info-close.svg" class="ms-1" /></td>
                                                                <td>7387-2902</td>
                                                                <td>159.4Kb</td>
                                                                <td>3.0Mb</td>
                                                                <td>3.1Mb</td>
                                                                <td class="text-center border-left">1</td>
                                                                <td class="text-center">49</td>
                                                                <td class="text-center col-blue border-right">50</td>
                                                                <td>Archived</td>
                                                                <td class="text-nowrap">2022-06-1</td>
                                                            </tr>

                                                        </tbody>
                                                    </table>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="pagination-bottom-wrapper model-statistic-footer-wrap">
                                            <div class="pagination-bottom-info d-flex">
                                                <div class="pagi-left">
                                                    <ul class="pagi-ul d-flex">
                                                        <li class="pagi-li"><a href="#" class="pagi-link active">10</a></li>
                                                        <li class="pagi-li"><a href="#" class="pagi-link">25</a></li>
                                                        <li class="pagi-li"><a href="#" class="pagi-link">50</a></li>
                                                        <li class="pagi-li"><a href="#" class="pagi-link">100</a></li>
                                                    </ul>
                                                </div>
                                                <div class="pagi-right">
                                                    <ul class="pagi-ul d-flex">
                                                        <li class="pagi-li pagi-prev-info"><a href="#" class="pagi-link">
                                                            <img src="../Images/img/icon/left-arrow-icon.svg" alt="left-arrow">
                                                        </a></li>
                                                        <li class="pagi-li"><a href="#" class="pagi-link active">1</a></li>
                                                        <li class="pagi-li pagi-next-info"><a href="#" class="pagi-link">
                                                            <img src="../Images/img/icon/right-arrow-icon.svg" alt="right-arrow"></a></li>
                                                    </ul>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- End -->
                    </div>
                </div>

            </div>
        </div>

    </div>
</asp:Content>
