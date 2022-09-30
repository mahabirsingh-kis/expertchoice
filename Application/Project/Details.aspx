<%@ Page Language="VB" Inherits="ProjectDetailsPage" title="Project Details" Codebehind="Details.aspx.vb" %>
<asp:Content ID="Main" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    var formats = [<% =GetLinks() %>];
    var project_types = [
        {"ID": <%= ProjectType.ptRegular %>,        "text" : "<% =JS_SafeString(ResString("lblRiskModel")) %>"}, 
        {"ID": <%= ProjectType.ptOpportunities %>,  "text": "<% =JS_SafeString(ResString("lblOpportunityModel")) %>"}, 
        {"ID": <%= ProjectType.ptMixed %>,          "text" : "<% =JS_SafeString(ResString("lblMixedModel")) %>"}<% If _OPT_MY_RISK_REWARD_ALLOWED OrElse PRJ.isMyRiskRewardModel Then %>, 
        {"ID": <% =ProjectType.ptMyRiskReward %>,     "text" :  "<% =JS_SafeString(ResString("lblMyRiskRewardModel")) %>"}<% End If %>
    ];

    var isProjectAttributesVisible = <% =Bool2JS(CurrentPageID = _PGID_PROJECT_PROPERTIES)%>;

    function onSetPage(pgid) {
        var res = false;
        switch (pgid % _pgid_max_mod) {
            case <% =_PGID_PROJECT_DESCRIPTION %>:
            case <% =_PGID_PROJECT_PROPERTIES %>:
                $("#tabs").dxTabPanel("instance").option("selectedIndex", (pgid == <% =_PGID_PROJECT_DESCRIPTION %> ? 0 :1));
                res = true;
                break;
        }
        return res;
    }

    function onGetReport(addReportItem) {
        if (typeof addReportItem == "function") {
            addReportItem({"name": "<% =JS_SafeString(ResString("titleDescription")) %>", "type": "<% =JS_SafeString(ecReportItemType.ModelDescription.ToString) %>", "edit": document.location.href});
        }
    }

    function getTypeByID(id) {
        for (var i=0; i<project_types.length; i++) {
            if (project_types[i].ID == id) return project_types[i].text;
        }
        return "";
    }

    var formData = {
        id: <%=PRJ.ID%>,
        name: "<%=JS_SafeString(PRJ.ProjectName)%>",
        description: "", <%--"<%=JS_SafeString(PRJ.Comment)%>"--%>
        passcode_likelihood: "<%=JS_SafeString(PRJ.PasscodeLikelihood)%>",
        passcode_impact: "<%=JS_SafeString(PRJ.PasscodeImpact)%>",
        db_version: "<%=PRJ.DBVersion.GetVersionString()%>",
        is_public: <%=Bool2JS(PRJ.isPublic)%>,
        is_online: <%=Bool2JS(PRJ.isOnline)%>,
        project_type: <%=CInt(PRJ.RiskionProjectType)%>,
        date_created: "<%=PRJ.Created%>",
        date_last_modified: "<%=PRJ.LastModify%>",
        date_last_visited: "<%=PRJ.LastVisited%>",
        statistic: "<%=GetProjectStatistic()%>",
        begins_on: "<%=PM.PipeParameters.StartDate%>",
        ends_on: "<%=PM.PipeParameters.EndDate%>",
        timeframe: "<%=JS_SafeString(PM.Parameters.TimeFrame) %>",
        assumptions: "<%=JS_SafeString(PM.Parameters.Assumptions) %>",
    };

    function onChangeActiveProject(prj) {
        if ((prj) && (prj.ID == formData.id)) {
            formData.name = prj.Name;
            formData.passcode_likelihood = prj.AccessCode;
            formData.passcode_impact = prj.AccessCodeImpact;
            formData.is_public = prj.isPublic;
            formData.is_online = prj.isOnline;
            //initFormDetails();
        }
    }

    function DoDownload() {
        var v = getValue();
        var url = "";
        if (v.indexOf("survey") > 0) {
            url = "<% =SurveyLink %>&" + v;
        } else {
            url = "<% =DownloadLink %>&" + v;
            var s = "no";
            if (($("#cbSaveSnapshots").length) && $("#cbSaveSnapshots").dxCheckBox("instance").option("value")) s = "yes";
            if (isAHPS()) url+="&snapshots=" + s;
        }
	    doDownload(url);
    }

    function getValue() {
        return $("#cbFormat").dxSelectBox("instance").option("value");
    }

    function isAHPS() {
        var v = getValue();
        return (v.indexOf("ahps") > 0)
    }

    function checkOptions() {
        if ($("#cbSaveSnapshots").length) {
            $("#cbSaveSnapshots").dxCheckBox("instance").option("disabled", !isAHPS());
        }
    }

    function getHelpText(project_type) {
        return project_type == <% = CInt(ProjectType.ptOpportunities) %> ? resString("hintOpportunityModel") : (project_type == <% = CInt(ProjectType.ptMixed) %> ? resString("hintMixedModel") : (project_type == <% = CInt(ProjectType.ptMyRiskReward) %> ? resString("hintIsMyRiskRewardModel") : resString("hintRiskModel")));
    }

    function initFormDetails() {
        if (($("#formProjectDetails").data("dxForm"))) {
            $("#formProjectDetails").dxForm("instance").dispose();
        }

        var offset = 12;

        var formObj = $("#formProjectDetails").dxForm({
            colCount: 1,
            formData: formData,
            items: [{
                dataField: "name",
                //visible: isProjectAttributesVisible,
                label: {alignment: "right", text: "<%=JS_SafeString(ResString("lblAppPrjName"))%>"},
                isRequired: true,
                editorOptions: {
                    onValueChanged: function (e) {
                        saveChanges();
                    }
                }
            }, {
                dataField: "description",
                label: {alignment: "right", text: "<% =JS_SafeString(ResString("lblProjectComment")) %>"},
                template: function (data, itemElement) {
                    var frmInfodoc = "<div id='btnInfodocContainer' style='position:absolute; right:" + (isProjectAttributesVisible ? 32 : 40) + "px; bottom:" + (isProjectAttributesVisible ? 24 : <% =If(PM.IsRiskProject, 64, 24) %>) + "px; z-index:88;'><div id='btnEditInfodoc'></div></div><div id='btnEditDescr'></div><iframe src='' id='frmInfodoc' style='width:100%; height:" + (isProjectAttributesVisible ? "<%=JS_SafeNumber(PM.Parameters.Project_DescriptionHeight)%>px" : "100%") + "; border:1px solid #e0e0e0; border-radius: 4px; background:#ffffff; margin-bottom:8px;' frameborder='0'></iframe>";
                    var btn_vis = false;
                    if (isProjectAttributesVisible) {
                        itemElement.append("<div id='divDescriptionResizeable'>").dxResizable({
                            handles: "bottom",
                            onResize: function (e) {
                                $("#frmInfodoc").height(e.height-offset);
                            },
                            onResizeEnd: function (e) {
                                callAjax("action=description_height&value=" + (e.height - offset), syncReceived);
                            }
                        });
                    } else {
                        itemElement.append("<div id='divDescriptionResizeable'>");
                    }
                    $("#divDescriptionResizeable").append(frmInfodoc);
                    loadDescription();
                }
<%--            }, {
                label: {alignment: "right", text: ""},
                template: function (data, itemElement) {
                    if (isProjectAttributesVisible) {
                        var btn3 = $("<div id='divBtnCopyLinkLikelihood' style='float:left'>").dxButton({
                            icon: "fas fa-clipboard",
                            text: "<% = JS_SafeString(ResString("lblCopyEvalLink")) %>",
                            onClick: function(e) {
                                checkProjectOnlineAndCopyLink(<% =App.ProjectID %>, (typeof curPrjData != "undefined" && typeof curPrjData.isOnline != "undefined" ?  curPrjData.isOnline : <% =Bool2JS(PRJ.isOnline) %>), "<% =JS_SafeString(ApplicationURL(False, False)) %>/?passcode=" + data.component.option("formData")["passcode_likelihood"]);
                            }
                        });         
                        $("#divDescriptionResizeable").append(btn3);
                    }
                }--%>
            }, {
                visible: isProjectAttributesVisible,
                dataField: "assumptions",
                label: {alignment: "right", text: "<%=JS_SafeString(ResString("lblPrjAssumptions"))%>"},
                editorOptions: {
                    onValueChanged: function (e) {
                        saveChanges();
                    }
                }
            }, <%If PM.IsRiskProject Then%>{
                //visible: isProjectAttributesVisible,
                dataField: "timeframe",
                label: {alignment: "right", text: "<% =JS_SafeString(ResString("lblPrjTimeframe")) %>"},
                //editorType: "dxDateBox",
                editorOptions: {
                    //elementAttr: {"class": "to_advanced"},
                    //width:250,
                    onValueChanged: function (e) {
                        if (((e.value) || (e.previousValue)) && !object_equal(e.value, e.previousValue)) { 
                            saveChanges();
                        }
                    }
                }
            }, {
                visible: isProjectAttributesVisible,
                dataField: "passcode_likelihood",
                label: {alignment: "right", text: "<%= JS_SafeString(ResString("lblPasscodeLikelihood")) %>"},
                isRequired: true,
                editorOptions: {
                    onValueChanged: function (e) {
                        saveChanges();
                    }
                },
                template: function (data, itemElement) {
                    var tb = $("<div id='divPasscodeLikelihood' style='float: left;'>").dxTextBox({
                        value: data.component.option('formData')[data.dataField],
                        width: 130,
                        onValueChanged: function(e) {
                            data.component.updateData(data.dataField, e.value);
                            saveChanges();
                        }
                    });
                    itemElement.append(tb);
                    var btn1 = $("<div id='divBtnCreateCodeLikelihood' style='margin: 0px 2px 0px 2px;'>").dxButton({
                        icon: "fa fa-sync-alt",
                        hint: "<% =JS_SafeString(String.Format(ResString("lblCreateNewPasscode"), Capitalize(ParseString(_TPL_RISK_LIKELIHOOD)))) %>",
                        onClick: function(e) {
                            var result = DevExpress.ui.dialog.confirm("<% = JS_SafeString(ResString("confNewPasscode")) %>", "<% = JS_SafeString(ResString("titleConfirmation")) %>");
                            result.done(function (dialogResult) {
                                if (dialogResult) {
                                    callAjax("action=get_passcode&for=1", syncReceived);
                                }
                            });
                        }
                    });
                    itemElement.append(btn1);
                    var btn2 = $("<div id='divBtnGetLinkLikelihood' style='margin: 0px 2px 0px 0px;'>").dxButton({
                        icon: "fas fa-link",
                        text: "<% = JS_SafeString(ResString("lblGetLinkLikelihood")) %>",
                        width: 230,
                        disabled: <%=Bool2JS(PRJ.isMarkedAsDeleted OrElse PRJ.ProjectStatus <> ecProjectStatus.psActive)%>,
                        visible: <%=Bool2JS(PRJ.ProjectStatus = ecProjectStatus.psActive)%>,
                        onClick: function(e) {
                            //callAjax("action=get_likelihood_link", syncReceived);
                            getProjectLinkDialog(curPrjID, formData.name, formData.passcode_likelihood, "<%=clsMeetingID.AsString(PRJ.MeetingIDLikelihood)%>", "<%=clsMeetingID.AsString(PRJ.MeetingIDLikelihood, clsMeetingID.ecMeetingIDType.Antigua)%>", formData.is_online, "<% =JS_SafeString(ApplicationURL(False, False)) %>", "", "<% =JS_SafeString(ApplicationURL(False, False)) %>", false);
                            //onGetLink(formData.passcode_likelihood, "<%=clsMeetingID.AsString(PRJ.MeetingIDLikelihood)%>", "<%=clsMeetingID.AsString(PRJ.MeetingIDLikelihood, clsMeetingID.ecMeetingIDType.Antigua)%>", formData.name);
                        }
                    });         
                    itemElement.append(btn2);
                }
            }, {
                visible: isProjectAttributesVisible,
                dataField: "passcode_impact",
                label: {alignment: "right", text: "<%= JS_SafeString(ResString("lblPasscodeImpact")) %>"},
                isRequired: true,
                editorOptions: {
                    onValueChanged: function (e) {
                        saveChanges();
                    }
                },
                template: function (data, itemElement) {
                    var tb = $("<div id='divPasscodeImpact' style='float: left;'>").dxTextBox({
                        value: data.component.option('formData')[data.dataField],
                        width: 130,
                        onValueChanged: function(e) {
                            data.component.updateData(data.dataField, e.value.trim());
                            saveChanges();
                        }
                    });
                    itemElement.append(tb);
                    var btn1 = $("<div id='divBtnCreateCodeImpact' style='margin: 0px 2px 0px 2px;'>").dxButton({
                        icon: "fa fa-sync-alt",
                        hint: "<% =JS_SafeString(String.Format(ResString("lblCreateNewPasscode"), Capitalize(ParseString(_TPL_RISK_IMPACT)))) %>",
                        onClick: function(e) {
                            var result = DevExpress.ui.dialog.confirm("<% = JS_SafeString(ResString("confNewPasscode")) %>", "<% = JS_SafeString(ResString("titleConfirmation")) %>");
                            result.done(function (dialogResult) {
                                if (dialogResult) {
                                    callAjax("action=get_passcode&for=2", syncReceived);
                                }
                            });
                        }
                    });
                    itemElement.append(btn1);
                    var btn2 = $("<div id='divBtnGetLinkImpact' style='margin: 0px 2px 0px 0px;'>").dxButton({
                        icon: "fas fa-link",
                        text: "<% = JS_SafeString(ResString("lblGetLinkImpact")) %>",
                        disabled: <%=Bool2JS(PRJ.isMarkedAsDeleted OrElse PRJ.ProjectStatus <> ecProjectStatus.psActive)%>,
                        visible: <%=Bool2JS(PRJ.ProjectStatus = ecProjectStatus.psActive)%>,
                        width: 230,
                        onClick: function(e) {
                            getProjectLinkDialog(curPrjID, formData.name, formData.passcode_impact, "<%=clsMeetingID.AsString(PRJ.MeetingIDImpact)%>", "<%=clsMeetingID.AsString(PRJ.MeetingIDImpact, clsMeetingID.ecMeetingIDType.Antigua)%>", formData.is_online, "<% =JS_SafeString(ApplicationURL(False, False)) %>", "", "<% =JS_SafeString(ApplicationURL(False, False)) %>", false);
                        }
                    });         
                    itemElement.append(btn2);
                }
            }, <%Else%>{
                visible: isProjectAttributesVisible,
                dataField: "passcode_likelihood",
                label: {alignment: "right", text: "<% = JS_SafeString(ResString("lblPasscode")) %>"},
                isRequired: true,
                editorOptions: {
                    onValueChanged: function (e) {
                        saveChanges();
                    }
                },
                template: function (data, itemElement) {
                    var tb = $("<div id='divPasscode' style='float: left;'>").dxTextBox({
                        value: data.component.option('formData')[data.dataField],
                        width: 130,
                        onValueChanged: function(e) {
                            data.component.updateData(data.dataField, e.value.trim());
                            saveChanges();
                        }
                    });
                    itemElement.append(tb);
                    var btn1 = $("<div id='divBtnCreateCode' style='margin: 0px 2px 0px 2px;'>").dxButton({
                        icon: "fa fa-sync-alt",
                        hint: "<% =JS_SafeString(String.Format(ResString("lblCreateNewPasscode"), "")) %>",
                        onClick: function(e) {
                            var result = DevExpress.ui.dialog.confirm("<% = JS_SafeString(ResString("confNewPasscode")) %>", "<% = JS_SafeString(ResString("titleConfirmation")) %>");
                            result.done(function (dialogResult) {
                                if (dialogResult) {
                                    callAjax("action=get_passcode&for=0", syncReceived);
                                }
                            });
                        }
                    });
                    itemElement.append(btn1);
                    var btn2 = $("<div id='divBtnGetLink' style='margin: 0px 2px 0px 0px;'>").dxButton({
                        icon: "fas fa-link",
                        text: "<% =JS_SafeString(ResString("lblGetLink")) %>",
                        disabled: <%=Bool2JS(Not (Not PRJ.isMarkedAsDeleted AndAlso PRJ.ProjectStatus = ecProjectStatus.psActive))%>,
                        visible: <%=Bool2JS(PRJ.ProjectStatus = ecProjectStatus.psActive)%>,
                        onClick: function(e) {
                            getProjectLinkDialog(curPrjID, formData.name, formData.passcode_likelihood, "<%=clsMeetingID.AsString(PRJ.MeetingIDLikelihood)%>", "<%=clsMeetingID.AsString(PRJ.MeetingIDLikelihood, clsMeetingID.ecMeetingIDType.Antigua)%>", formData.is_online, "<% =JS_SafeString(ApplicationURL(False, False)) %>", "", "<% =JS_SafeString(ApplicationURL(False, False)) %>", false);
                        }
                    });         
                    itemElement.append(btn2);
                }
            }, <%End If%> {
                visible: isProjectAttributesVisible,
                dataField: "is_public",
                label: {alignment: "right", text: " ", showColon: false},
                disabled: <%=Bool2JS(IsReadOnly)%>,
                editorOptions: {
                    text: "<% = JS_SafeString(ResString("optAccessCode")) %>",
                    onValueChanged: function (e) {
                        saveChanges();
                    }
                }
            }, {
                visible: false, //isProjectAttributesVisible,
                dataField: "is_online",
                label: {alignment: "right", text: "<% = JS_SafeString(ResString("mnuProjectOnlineStatus")) %>"},
                editorOptions: {
                    readOnly: true
                },
                template: function (data, itemElement) {
                    itemElement.append("<span>" + (formData.is_online ? "<% = JS_SafeString(ResString("optProjectOnline")) %>" : "<% = JS_SafeString(ResString("optProjectOffline")) %>") + "</span>");
                }                
                <%--helpText: "<%=If(PRJ.isOnline, "Online (evaluators will be able to input judgments)", "Offline (evaluators will not be able to input judgments)")%>"--%>
            }, {
                visible: isProjectAttributesVisible && isAdvancedMode,
                dataField: "begins_on",
                disabled: <%=Bool2JS(IsReadOnly)%>,
                label: {alignment: "right", text: "<% =JS_SafeString(ResString("lblTimelineStart")) %>"},
                editorType: "dxDateBox",
                editorOptions: {
                    elementAttr: {"class": "to_advanced"},
                    width:250,
                    onValueChanged: function (e) {
                        if (((e.value) || (e.previousValue)) && !object_equal(e.value, e.previousValue)) { 
                            saveChanges();
                        }
                    }
                }
            }, {
                visible: isProjectAttributesVisible && isAdvancedMode,
                dataField: "ends_on",
                disabled: <%=Bool2JS(IsReadOnly)%>,
                editorType: "dxDateBox",
                label: {alignment: "right", text: "<% =JS_SafeString(ResString("lblTimelineEnd")) %>"},
                editorOptions: {
                    elementAttr: {"class": "to_advanced"},
                    width:250,
                    onValueChanged: function (e) {
                        if (((e.value) || (e.previousValue)) && !object_equal(e.value, e.previousValue)) { 
                            saveChanges();
                        }
                    }
                }
            }, <%If PM.IsRiskProject Then%>{
                visible: isProjectAttributesVisible,
                dataField: "project_type",
                label: {alignment: "right", text: "<% =JS_SafeString(ResString("tblProjectType")) %>"},
                disabled: <%=Bool2JS(Not (Not IsReadOnly OrElse PRJ.ProjectStatus = ecProjectStatus.psTemplate OrElse PRJ.ProjectStatus = ecProjectStatus.psMasterProject))%>,
                elementAttr: { id: "frmItemProjectType" },
                editorType: "dxSelectBox",
                editorOptions: { 
                    items: project_types,
                    displayExpr: "text",
                    valueExpr: "ID",
                    value: formData.project_type,
                    width: 250,
                    onValueChanged: function (e) {
                        var mt = $("#spanModelTypeName");
                        if ((mt) && (mt.length)) mt.text(getTypeByID(e.value));
                        //var frm = $("#formProjectDetails").dxForm("instance");
                        //frm.itemOption("project_type", "helpText", getModelTypeHint(e.value));
                        setTimeout(initFormDetails, 300);
                        saveChanges(function () {
                            callAPI("account/?<% = _PARAM_ACTION %>=allowed_pages", {}, onLoadMenus, "Update resources...");
                        });
                    }
                },
                helpText: getModelTypeHint(formData.project_type)
            }, <%End If%>{
                visible: isProjectAttributesVisible,
                //colSpan: 2,
                dataField: "statistic",
                label: {alignment: "right", text: "<%=JS_SafeString(ResString("titlePrjStatistic"))%>"},
                editorType: "dxTextArea",
                editorOptions: {
                    height: 70,
                    readOnly: true
                },
                template: function (data, itemElement) {
                    itemElement.append("<div class='dx-field-item-custom-text'>" + data.component.option('formData')[data.dataField] + "</div>");
                }
            }, {
                visible: isProjectAttributesVisible && isAdvancedMode,
                dataField: "db_version",
                label: {alignment: "right", text: "DB version"},
                editorOptions: { readOnly: true },
                template: function (data, itemElement) {
                    itemElement.append("<div class='to_advanced dx-field-item-custom-text'>" + data.component.option('formData')[data.dataField] + "</div>");
                }
            }, {
                visible: isProjectAttributesVisible && isAdvancedMode,
                dataField: "date_created",
                label: {alignment: "right", text: "<% = JS_SafeString(ResString("lblCreated")) %>"},
                editorOptions: { readOnly: true },
                template: function (data, itemElement) {
                    itemElement.append("<div class='to_advanced dx-field-item-custom-text'>" + data.component.option('formData')[data.dataField] + "</div>");
                }
            }, {
                visible: isProjectAttributesVisible && isAdvancedMode,
                dataField: "date_last_modified",
                label: {alignment: "right", text: "<% = JS_SafeString(ResString("lblLastModified")) %>"},
                editorOptions: { readOnly: true },
                template: function (data, itemElement) {
                    itemElement.append("<div class='to_advanced dx-field-item-custom-text'>" + data.component.option('formData')[data.dataField] + "</div>");
                }
            }
            ]
        });

        $("#btnEditInfodoc").dxButton({
            hint: "Edit description",
            icon: "far fa-edit",
            type: "default",
            height: 40,
            width: 40,
            elementAttr: { "class": "dx-btn-float" },
            visible : <%=Bool2JS(Not IsReadOnly AndAlso CanUserModifyActiveProject) %>,
            onClick: function() {
                OpenRichEditor("?project=<%=PRJ.ID%>&type=7&callback=comment");            
            }
        });

        $(".dx-item-content").css("padding", "2px");
        $(".to_advanced").closest(".dx-box-item-content").addClass("on_advanced");

        onResize();
    }

    function getModelTypeHint(t) {
        return (t == <%=CInt(ProjectType.ptOpportunities)%> ? resString("hintOpportunityModel") : (t == <%=CInt(ProjectType.ptMixed)%> ? resString("hintMixedModel") : (t == <%=CInt(ProjectType.ptMyRiskReward)%> ? resString("hintIsMyRiskRewardModel") : resString("hintRiskModel"))));
    }

    function onLoadMenus(res) {
        if (isValidReply(res)) {
            if (res.Result == _res_Success && res.Data !=" undefined") {
                var nav_json_ = parseReply(res.Data);
                if ((nav_json_)) {
                    nav_json = nav_json_;
                    initNavigation();
                }
            }
        }
    }

    function onResizeInfodoc() {
        if (!isProjectAttributesVisible) {
            var f = $("#frmInfodoc");
            var m = $("#tabs");
            if ((f) && (f.length) && (m) && (m.length)) {
                f.height(32);
                var offset = f.prop("offsetTop")*1;
                if (offset<40) offset = 40;
                var h = m.height() - offset - <% =If(App.isRiskEnabled, 86, 48) %>;
                if (h > 30) f.height(h);
            }
        }
    }

    function loadDescription() {
        var src = "<%=PageURL(_PGID_EVALUATE_INFODOC)%>?project=<%=PRJ.ID%>&type=<% = CInt(reObjectType.Description) %>&r=" + (Math.floor(Math.random() * 999999))
        var f = document.getElementById("frmInfodoc");
        if ((f)) {
            if (src == "") {
                f.src = "";
            } else {
                //showLoadingPanel();
                initFrameLoader(f);
                setTimeout(function () { document.getElementById("frmInfodoc").src = src }, 30);
                //f.onload = hideLoadingPanel;
            }
        }
    }

    function onRichEditorRefresh(empty, infodoc, callback_param) {        
        loadDescription();
    }

    function saveChanges(extra_func) {
        var formObj = $("#formProjectDetails").dxForm("instance");
        var data = formObj.option("formData");        
        if (data.name.trim() != "" && data.passcode_likelihood.trim() != "" && data.passcode_impact.trim() != "") {
            callAjax("action=save&data=" + encodeURIComponent(JSON.stringify(data).replace(/</g, '&lt;').replace(/>/g, '&gt;')), function (params) { 
                syncReceived(params, extra_func) 
            });
        }
    }

    /* Callback */
    function syncReceived(params, extra_func) {
        if ((params)) {
            var rd = eval(params);
            if (rd[0] == "save") {    
                var formObj = $("#formProjectDetails").dxForm("instance");
                formData = formObj.option("formData");        
                formData.is_online = rd[1];
                formData.date_last_modified = rd[2];
                formData.project_type = rd[3];
                //initFormDetails();
                DevExpress.ui.notify("Changes Saved", "success");
            }
            if (rd[0] == "get_passcode") {
                var formObj = $("#formProjectDetails").dxForm("instance");
                formData = formObj.option("formData");        
                var sPasscode = rd[1] + "";
                var sFor = rd[2] + "";
                switch (sFor) {
                    case "0":
                    case "1":
                        formData.passcode_likelihood = sPasscode;
                        break;
                    case "2":
                        formData.passcode_impact = sPasscode;
                        break;
                }
                callAjax("action=save&data=" + encodeURIComponent(JSON.stringify(formData)), syncReceived);
                initFormDetails();
            }
            if (rd[0] == "set_prj_online") {
                var formObj = $("#formProjectDetails").dxForm("instance");
                formData = formObj.option("formData");        
                formData.is_online = rd[1];
                formData.date_last_modified = rd[2];
                initFormDetails();
            }
            if (rd[0] == "get_link" || rd[0] == "get_likelihood_link" || rd[0] == "get_impact_link") {
                var link = rd[1];
                var formObj = $("#formProjectDetails").dxForm("instance");
                formData = formObj.option("formData");
                formData.is_online = rd[2] == "1";
                var can_set_online = rd[3] == "1";
                formData.date_last_modified = rd[4];
                if (!formData.is_online && can_set_online) {                    
                    checkProjectOnlineAndCopyLink(<%=PRJ.ID%>, formData.is_online, link);
                    //dxConfirm(resString("msgSetProjectStatusOnline"), "callAjax('action=set_prj_online', syncReceived);");
                } else {
                    copyDataToClipboard(link);
                }
                //copyDataToClipboard(link);
                initFormDetails();
            }
        }
        displayLoadingPanel(false);
        if (typeof extra_func == "function") extra_func();
    }

    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }

    function initDownloads() {
        $("#cbFormat").dxSelectBox({
            dataSource: formats,
            displayExpr: "text",
            valueExpr: "value",
            value: formats[0].value,
            width: "100%",
            onItemClick: function (e) {
                checkOptions();
            }
        });
        $("#btnDownload").dxButton({
            text: "<%=JS_SafeString(ResString("btnDownload"))%>",
            icon: "download",
            width: "115px",
            type: "default",
            onClick: function () {
                setTimeout("DoDownload();", 150);
            }
        });
        $("#btnDownload").css("vertical-align", "top").css("padding-bottom", "2px");
        if ($("#cbSaveSnapshots").length) {
            $("#cbSaveSnapshots").dxCheckBox({
                text: "<% =JS_SafeString(ResString("lblSaveSnapshots"))%>",
                disabled: true,
                value: true,
            });
            checkOptions();
        }
    }

    function onResize(force, w, h) {
        if (typeof h == "undefined" || h<50) {
            var m = $("#tabs").parent();
            if ((m) && (m.length)) {
                $("#tabs").hide();
                h = m.height();
                $("#tabs").show();
            }
        }
        if (h>50) $("#tabs").height(h-32);
        onFormNarrow("#formProjectDetails", 800);
        onResizeInfodoc();
    }

    function onSwitchAdvancedMode() {
        initFormDetails();
    }

    function initTabs() {
        $("#tabs").dxTabPanel({
            items: [{ id:0, title: "<% = JS_SafeString(ResString("mnuDescription")) %>", icon: "far fa-file-alt", template: $("#tab0") },
                    { id:1, title: "<% = JS_SafeString(ResString("mnuProjectInfo")) %>", icon: "far fa-edit", template: $("#tab1") }],
            focusStateEnabled: false,
            scrollByContent: true, 
            selectedIndex: 0,  // leave as 0 for any tab
            onSelectionChanged: function(e) {
                if (e.addedItems.length > 0) {
                    var idx = e.addedItems[0].id;
                    var pg = -1;
                    if (idx == 0) pg = <% =_PGID_PROJECT_DESCRIPTION %>;
                    if (idx == 1) pg = <% =_PGID_PROJECT_PROPERTIES %>;
                    if (typeof navOpenPage == "function" && (pg>0)) {
                        navOpenPage(pg, true);
                    }
                    $("#divForm").detach().appendTo($("#tab" + idx));
                    isProjectAttributesVisible = (idx>0);
                    initFormDetails();
                    $("#divDownload").toggle(isProjectAttributesVisible);
                    if (isProjectAttributesVisible) {
                        initDownloads();
                    }
                }
            }
        });
    }

    function navigatePage(pgid, hid) {
        //cancelPing(true);
        //showLoadingPanel("", true);
    }

    resize_custom = onResize;
    ping_active = false;

    $(document).ready(function () {
        initTabs();
        initFormDetails();
        if (<% =Bool2JS(CurrentPageID = _PGID_PROJECT_PROPERTIES) %>) {
             $("#tabs").dxTabPanel("instance").option("selectedIndex", 1);
        }
        //setTimeout(function () { showLoadingPanel(); }, 30);
        document.addEventListener('scroll', function (event) {
            //if (event.target.id === 'tab1') {
            //    alignFloatButton();
            //}
        }, true);
        cancelPing(true);
    });
    
</script>

<div id="tabs" class="ec_tabs" style="margin: 1em 1em 1em 1ex;">
    <div id="tab0" class="whole" style="overflow:auto; padding:1ex;">
        <div id="divForm">
            <div id="formProjectDetails"></div>
            <% If App.CanUserDoProjectAction(ecActionType.at_mlDownloadModel, App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup) Then  %><div id="divDownload" style="text-align:center; display:none; border-top:1px solid #e0e0e0;"><% If App.HasActiveProject Then%>
            <table border="0" align="center" style="min-width:280px; max-width: 700px;">
            <tr class="text">
                <td colspan="2"><div style="padding:1em; margin:1em 0px; border:1px solid #f0f0f0; background:#f5faff; border-radius:6px; text-align:center;">
                    <h6><% = ResString("lblDownloadFormat")%></h6>
                    <div style="vertical-align:top;">
                        <div style="padding:1px; display:inline-block; min-width:240px;"><div id="cbFormat"></div></div>
                        <div style="padding:1px; display:inline-block; float:right;"><div id="btnDownload"></div></div>
                    </div>
                    <% If App.isSnapshotsAvailable AndAlso _SNAPSHOT_USE_IN_AHPS Then%><div style="text-align:left; padding-top:4px;"><div id="cbSaveSnapshots"></div></div><% End If%>
                </div></td>
            </tr>
            </table>
            <% End If%>
            <asp:Label runat="server" ID="lblError" Font-Bold="true" CssClass="text error" Text=""/>
            </div><% End If %>
        </div>
    </div>
    <div id="tab1" class="whole" style="overflow:auto; padding: 1ex;"></div>
</div>

</asp:Content>