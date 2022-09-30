<%@ Page Language="VB" Inherits="MyRiskRewardPage" title="MyRiskReward" Codebehind="MyRiskReward.aspx.vb" %>
<asp:Content ID="ContentScrMRR" ContentPlaceHolderID="head_JSFiles" Runat="Server">
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    showLoadingPanel();
    
    var OPT_ALLOW_MULTIPLE_CONTRIBUTIONS = true;
    
    var isReadOnly = <% = Bool2JS(IsReadOnly) %>;
    var eventsDisplayMode = "<% = PM.Parameters.MyRiskReward_ShowEventsMode %>";
    var showDescriptions = <% = Bool2JS(PM.Parameters.MyRiskReward_ShowDescriptions) %>;
    var lblRisk = "<% = JS_SafeString(ParseString("%%Risk%% %%Alternative%%")) %>";
    var lblReward = "<% = JS_SafeString(ParseString("%%Opportunity%% %%Alternative%%")) %>";
    var scenarios = [];
    var lastScenarioID, lastEventID;
    var selectedScenario;
    var phase = 0;

    var OPT_PLEASE_WAIT_DELAY = 3000;

    var onTextBoxFocusIn = function (e) { 
        setTimeout(function (){ $(e.element).find("input").select(); }, 10);
    };
    var onTextAreaFocusIn = function (e) { 
        setTimeout(function (){ $(e.element).find("textarea").select(); }, 10);
    };

    /* Toolbar */
    var toolbarItems = [{ location: 'before', locateInMenu: 'never', widget: 'dxButton', visible: !isReadOnly,
            options: { icon: "far fa-file-alt", text: "<% = ParseString("Add %%Objective(l)%%")%>", showText: true, elementAttr: {id: 'btn_add_scenario'},
                onClick: function (e) {
                    dxDialog("<div id='tbNodeName' style='width: 400px;'></div>", function () { 
                        var text = "";
                        if ($("#tbNodeName").data("dxTextBox")) {
                            text = $("#tbNodeName").dxTextBox("instance").option("value").trim();
                        }
                        if (text != "") {
                            callAPI("pm/hierarchy/?action=add_scenario", { name: text}, function (res) {
                                if (isValidReply(res) && res.Result == _res_Success) {
                                    scenarios.push(res.Data);                                    
                                    setPhaseView(phase);
                                    if (phase == 1) {
                                        $("#scenariosSelectBox").dxSelectBox("instance").option("value", scenarios[scenarios.length - 1].id);
                                    }
                                    DevExpress.ui.notify("<% = ParseString("%%Objective(l)%%") %> added");
                                }
                            }, false, OPT_PLEASE_WAIT_DELAY);
                        }
                        $("#tbNodeName").dxTextBox("instance").dispose();
                    }, function () {$("#tbNodeName").dxTextBox("instance").dispose();}, "<% = ParseString("Enter new %%objective(l)%% name:")%>");
                    $("#tbNodeName").dxTextBox({
                        value: "",
                        placeholder: "<% = ParseString("Type new %%objective(l)%% name here…")%>"
                    });
                    dxTextBoxFocus('tbNodeName');
                }
            }
        },
        { location: 'before', locateInMenu: 'auto', widget: 'dxSelectBox', visible: true,
        options: { searchEnabled: false, valueExpr: "value", value: eventsDisplayMode, displayExpr: "text", disabled: false, width: 200, elementAttr: {id: 'btn_events_display'},
            items: [{"value": "all", "text": "<% = ResString("optEventDisplayAll") %>"}, {"value": "risks", "text": "<% = ResString("optEventDisplayRisks") %>"}, {"value": "rewards", "text": "<% = ResString("optEventDisplayRewards") %>"} ],
            onSelectionChanged: function (e) { 
                if (e.selectedItem.value != eventsDisplayMode) {
                    eventsDisplayMode = e.selectedItem.value;
                    $(".wizard-event-type-risk").parent().toggle(eventsDisplayMode == "all" || eventsDisplayMode == "risks"); 
                    $(".wizard-event-type-reward").parent().toggle(eventsDisplayMode == "all" || eventsDisplayMode == "rewards");                    
                    callAPI("pm/hierarchy/?action=save_wizard_option", {"id": "eventsDisplayMode", "value": eventsDisplayMode}, undefined, false, OPT_PLEASE_WAIT_DELAY);
                }
            }
        }
    }, { location: 'before', locateInMenu: 'auto', widget: 'dxCheckBox', visible: true,
    options: { text: "<% = ResString("btnShowDescriptions") %>", showText: true, value: showDescriptions, disabled: false,
            onValueChanged: function (e) {
                $(".wizard-scenario-description").toggle(e.value);
                $(".wizard-event-description").toggle(e.value);
                $(".wizard-objective-description").toggle(e.value);
                $(".wizard-structure-event-description").toggle(e.value);
                showDescriptions = e.value;
                callAPI("pm/hierarchy/?action=save_wizard_option", {"id": "showDescriptions", "value": showDescriptions}, undefined, false, OPT_PLEASE_WAIT_DELAY);
            }
        }
    } ];    

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: toolbarItems,
            //visible: isAdvancedMode
        });
    }
    /* Toolbar */

    function initWizard() {        
        var wizardContainerParent = $("#wizardScenarios").parent();
        $("#wizardScenarios").empty().remove();
        $("<div>").prop("id", "wizardScenarios").addClass("wizard-container phase1").appendTo(wizardContainerParent);

        /* init wizard scenarios */
        var wizardScenarios = $("#wizardScenarios");
        scenarios.forEach(function(scenario) {
            renderScenarioList(wizardScenarios, scenario);
        });
    
        wizardScenarios.addClass("wizard-scrollable-board").dxScrollView({
            direction: "horizontal",
            showScrollbar: "always",
            height: 400
        });
        
        wizardScenarios.addClass("wizard-sortable-lists").dxSortable({
            allowReordering: !isReadOnly,
            data: scenarios,
            filter: ".wizard-scenario",
            group: "scenariosGroup",
            itemOrientation: "horizontal",
            handle: ".wizard-scenario", //".wizard-scenario-title",
            moveItemOnDrop: true,
            onDragStart: function (e) {
                $("#wizardPageTitle" + phase).hide();
                $(".wizard-scenario-delete-area").show();
            },
            onDragEnd: function (e) {
                $("#wizardPageTitle" + phase).show();
                $(".wizard-scenario-delete-area").hide();
            },
            onReorder: function (e) {
                if (e.toIndex !== -1 && e.toIndex !== e.fromIndex) {
                    var params = { "fromID": scenarios[e.fromIndex].id, "toID": scenarios[e.toIndex].id, "hid": <% = CInt(ECHierarchyID.hidLikelihood) %>, "move_action" : e.fromIndex > e.toIndex ? <% = CInt(NodeMoveAction.nmaBeforeNode) %> : <% = CInt(NodeMoveAction.nmaAfterNode) %> };
                    scenarios.splice(e.toIndex, 0, scenarios.splice(e.fromIndex, 1)[0]);
                    callAPI("pm/hierarchy/?action=move_node", params, function (res) {
                        if (isValidReply(res) && res.Result == _res_Success) {
                            DevExpress.ui.notify("<% = ParseString("%%Objectives(l)%%") %> reordered");
                        }
                    }, false, OPT_PLEASE_WAIT_DELAY);
                }
            }
        });

        $("#divDeleteScenario").empty();
        
        $("<div>").dxSortable({
            allowReordering: false,
            dropFeedbackMode: "push",
            group: "scenariosGroup",
            filter: "",
            moveItemOnDrop: false,
            onAdd: function (e) {
                dxConfirm(replaceString("{0}", htmlEscape(e.fromData[e.fromIndex].name), resString("msgSureDeleteCommon")), function () {
                    var scenario_id = e.fromData[e.fromIndex].id;
                    scenarios.splice(e.fromIndex, 1);
                    $(e.itemElement).empty().remove();
                    resizePage();
                    callAPI("pm/hierarchy/?action=delete_scenario", {"id": scenario_id}, function (res) {
                        if (isValidReply(res) && res.Result == _res_Success) {
                            refreshAllAvailableEventsDataSources();
                            toggleSidebarMenuEnabled();
                            DevExpress.ui.notify("<% = ParseString("%%Objective(l)%%") %> deleted");
                        }
                    }, false, OPT_PLEASE_WAIT_DELAY);
                });                    
            }
        }).html('<div><i class="fas fa-times"></i>&nbsp;<% = ResString("btnDelete") %></div>').appendTo($("#divDeleteScenario").hide());
                
        resizePage();
    }

    function renderStructureWizard() {
        var wizardContainerParent = $("#wizardStructure").parent();
        $("#wizardStructure").empty().remove();
        $("<div>").prop("id", "wizardStructure").addClass("wizard-container phase2").appendTo(wizardContainerParent);

        /* init wizard structure */
        var wizardStructure = $("#wizardStructure").empty();
        
        $("#wizardStructure").css({"width": "99%", "height": "100%", /*"background-color": "#a1a1a0",*/ "padding": 0, "margin": 0, "white-space": "nowrap"});
        //var $divSources = $("<div>").css({"width": "20%", "height": "100%", "border" : "1px dashed blue", /*"background-color": "#f5f0f0",*/ "padding": 0, "margin": 0, "display": "inline-block", "white-space": "normal", "vertical-align": "top"}).text("<% = ParseString("All %%Objectives(l)%%") %>").appendTo(wizardStructure);
        var $divStructureContent = $("<div>").css({/*"border" : "1px dashed blue", "background-color": "#f5f5f5",*/ "padding": 0, "margin": 0, "display": "inline-block", "white-space": "normal", "vertical-align": "top"}).appendTo(wizardStructure);
        //var $divObjectives = $("<div>").css({"width": "20%", "height": "100%", "border" : "1px dashed blue", /*"background-color": "#f0f0f5",*/ "padding": 0, "margin": 0, "display": "inline-block", "white-space": "normal", "vertical-align": "top"}).text("<% = ParseString("All %%Objectives(i)%%") %>").appendTo(wizardStructure);

        var $divScenariosSwitch = $("<div>").addClass("wizard-structure-scenarios-switch").css({"width": "100%", "height": "auto", /*"background-color": "#e4a0a0",*/ "padding": "5px", "display": "inline-block", "white-space": "normal", "vertical-align": "top", "text-align" : "center"}).html("<span style='font-size:medium; font-size: medium; vertical-align: top; margin-top: 3px; margin-right: 3px; display: inline-block;'><% = SafeFormString(ParseString("%%Objective(l)%%:")) %></span>").appendTo($divStructureContent);
        
        var $divEventsContent = $("<div>").addClass("wizard-structure-scenario-events-content").css({"width": "100%", "height": "100%", /*"background-color": "#a1e1f4",*/ "padding": "5px", "display": "inline-block", "white-space": "normal", "vertical-align": "top"}).text("Events").appendTo($divStructureContent);

        selectedScenario = scenarios.length > 0 ? scenarios[0] : null;

        var $scenariosSelectBox = $("<div>").prop("id", "scenariosSelectBox").css({"display": "inline-block", "min-width" : "400px"}).dxSelectBox({ 
                dataSource: scenarios,
                disabled: false,
                displayExpr: "name", //function (item) {  return item.name + (showDescriptions ? " - " + item.description : "");},
                //itemTemplate: function (itemData, itemIndex, itemElement) {
                //    return $("<div></div>").append(
                //        $("<span></span>").text(itemData.name),
                //        $("<p></p>").text(itemData.description).toggle(showDescriptions)
                //    );
                //},
                searchEnabled: false,
                value: (selectedScenario) ? selectedScenario.id : undefined,
                valueExpr: "id",
                onValueChanged: function (e) {
                    selectedScenario = scenarios.filter(function(item) { return item.id == e.value; });
                    renderStructureScenario($divEventsContent, selectedScenario[0]);
                    resizePage();
                }
            }).appendTo($divScenariosSwitch);        

        renderStructureScenario($divEventsContent, selectedScenario);
    }

    function renderStructureScenario($divEventsContent, scenario) {
        $divEventsContent.empty();
        $("#titleSelectedScenario").remove();
        
        if ((scenario)) {                                    
            <%--$("<div></div>").prop("id", "titleSelectedScenario").addClass("wizard-scenario-events-title").text(scenario.name + " <% = ParseString("%%Alternatives%%") %>").appendTo($divEventsContent);--%>

            var $scroll = $("<div>").appendTo($divEventsContent);
            var $items = $("<div>").appendTo($scroll);
        
            scenario.eventsStructureContainer = $items;

            scenario.events.forEach(function(alt) {
                renderStructureEvent($items, alt, scenario);
            });

            if (!(scenario.events) || scenario.events.length == 0) {
                $items.html("<h6 style='margin:2em 10em'><nobr><% = ResString("msgRiskResultsNoData") %></nobr><br><br><span class='note'><% = ParseString("You can <u><a href='' onclick='setPhaseView(0); return false;'>switch back to phase 1 view</a></u> to add %%alternatives%% to this %%objective(l)%%") %></span></h6>");
            }

            $scroll.addClass("wizard-scrollable-objectives-list").dxScrollView({
                direction: "vertical",
                showScrollbar: "always",
                //width: "95%",
                //height: 500
            });
        }
    }

    function renderScenarioList($container, scenario) {
        var $list = $("<div>").addClass("wizard-scenario").appendTo($container);
        scenario.containerElement = $list;
        renderScenarioTitle($list, scenario);        
        renderEvents($list, scenario);
    }

    function renderScenarioTitle($container, scenario) {
        var $div = $("<div>")
            .addClass("wizard-scenario-title")
            .addClass("dx-theme-text-color")
            .appendTo($container);
        $("<div>").addClass("wizard-scenario-name").appendTo($div).dxTextBox({
            value : scenario.name,
            onEnterKey: function (e) { updateNodeName(scenario, e.component.option("text"), <% = CInt(ECHierarchyID.hidLikelihood) %>); },
            onFocusOut: function (e) { updateNodeName(scenario, e.component.option("text"), <% = CInt(ECHierarchyID.hidLikelihood) %>); },
            onFocusIn: onTextBoxFocusIn,
            readOnly: isReadOnly,
            stylingMode: "underlined"
        });
        $("<div>").addClass("wizard-scenario-description").appendTo($div).dxTextArea({ 
            onEnterKey: function (e) { updateNodeDescription(scenario, e.component.option("text"), <% = CInt(ECHierarchyID.hidImpact) %>); },
            onFocusOut: function (e) { updateNodeDescription(scenario, e.component.option("text"), <% = CInt(ECHierarchyID.hidImpact) %>); },
            onFocusIn: onTextAreaFocusIn,
            minHeight: "70px",
            placeholder: "<% = ParseString("Type %%objective(l)%% description here…")  %>",
            readOnly: isReadOnly,
            stylingMode: "underlined",
            value : scenario.description 
        }).toggle(showDescriptions);
        $("<div></div>").addClass("wizard-scenario-events-title").text(scenario.name + " <% = ParseString("%%Alternatives%%") %>").appendTo($div);
        if (!isReadOnly) {
            if (OPT_ALLOW_MULTIPLE_CONTRIBUTIONS) {
                var events_ds = getAvailableEventsDataSource(scenarios, scenario);
                var $addContainer = $("<div>").addClass("wizard-event-add-area").appendTo($div);
                $("<div>").addClass("wizard-event-add-area-tag-box").dxTagBox({
                    scenario: scenario,
                    acceptCustomValue: true,
                    applyValueMode: "useButtons",
                    dataSource: events_ds,
                    displayExpr: "name",
                    groupComponent: null,
                    grouped: true,
                    groupRender: null,
                    groupTemplate: "group",
                    hideSelectedItems: false,
                    multiline: true,
                    noDataText: "No data to display",
                    onChange: null,
                    onClosed: null,
                    onContentReady: null,
                    onCustomItemCreating: function(e) {    
                        var selectedItems = $.find(".dx-list-select-checkbox.dx-checkbox-checked");
                        if (selectedItems.length > 0) return;
                        is_cb_init = true;
                        var curText;
                        if ((e.text) && (typeof e.text !== "undefined") && (e.text) && (e.text !== "") && (e.text.trim() !== "")) curText = e.text;
                        if (typeof curText == "undefined") curText = e.component.option("text");
                        if (typeof curText !== "undefined") curText = curText.trim();
                        if (!e.customItem && (curText) && (curText !== "")) {
                            e.component.beginUpdate();
                            e.component.option("text", "");
                            e.component.reset();
                            e.component.endUpdate();
                            e.component.close();
                            e.customItem = curText;
                            callAPI("pm/hierarchy/?action=add_new_event_to_scenario", {"id": scenario.id, "name": curText, "type" : <% = CInt(EventType.Opportunity) %>}, function (res) {
                                if (isValidReply(res) && res.Result == _res_Success && typeof res.Data == "object") {
                                    scenario.events.push(res.Data);
                                    jqHighlight(renderEvent(scenario.itemsContainer, res.Data, scenario));
                                    refreshAllAvailableEventsDataSources();
                                    toggleSidebarMenuEnabled();
                                    DevExpress.ui.notify("<% = ParseString("%%Alternative%% added to the %%objective(l)%%") %>");
                                }
                            }, false, OPT_PLEASE_WAIT_DELAY);
                        } 
                        setTimeout(function () {is_cb_init = false;}, 50);
                    },
                    onEnterKey: null,
                    onFocusOut: function(e) { 
                        var selectedItems = $.find(".dx-list-select-checkbox.dx-checkbox-checked");
                        if (!is_cb_init && selectedItems.length == 0) {
                            var curText = e.component.option("text");
                            if (typeof curText !== "undefined" && typeof curText !== "object" && curText + "" !== "") {
                                e.component.option("onCustomItemCreating")(e); 
                            }
                        }
                    },
                    onSelectionChanged: function (e) {
                        var selectedItems = e.component.option("selectedItems");
                        if (!is_cb_init) {
                            is_cb_init = true;
                            e.component.close();
                            if (selectedItems.length > 0) {
                                var selIDs = "";
                                selectedItems.map(function(item) { return item.id; }).forEach(function(itemId) { selIDs += (selIDs == "" ? "" : ",") + itemId; } );
                                callAPI("pm/hierarchy/?action=add_existing_events_to_scenario", {"id": scenario.id, "event_ids": selIDs, "hid": <% = CInt(ECHierarchyID.hidLikelihood) %>}, function (res) {
                                    if (isValidReply(res) && res.Result == _res_Success) {
                                        res.Data.forEach(function(item) { 
                                            scenario.events.push(item); 
                                            jqHighlight(renderEvent(scenario.itemsContainer, item, scenario));
                                        } );
                                        refreshAllAvailableEventsDataSources();
                                        toggleSidebarMenuEnabled();
                                        DevExpress.ui.notify("<% = ParseString("%%Alternative%%") %> type changed");
                                    }
                                }, false, OPT_PLEASE_WAIT_DELAY);
                                e.component.reset();
                            } else {
                                //see if custom field has text
                                //var curText = e.component.option("text");
                                //if ((curText) && typeof curText !== "undefined" && typeof curText !== "object" && curText.length && curText.trim() !== "") {
                                //    e.component.option("onCustomItemCreating")(e);
                                //}
                            }
                            setTimeout(function () {is_cb_init = false;}, 50);
                        }
                    },
                    onValueChanged: null,
                    openOnFieldClick: true,
                    placeholder: "<% = ParseString("Type new %%alternative%% name here…")  %>",
                    readOnly: isReadOnly,
                    searchEnabled: true,
                    searchMode: "contains",
                    searchTimeout: 50,
                    selectAllMode: "page",
                    showClearButton: false,
                    showDataBeforeSearch: true,
                    showDropDownButton: true,
                    showSelectionControls: true,
                    stylingMode: "underlined",                
                    valueExpr: "id",
                    visible: true,
                    width: "100%",
                    wrapItemText: false
                }).appendTo($addContainer);
            } else {
                var $addContainer = $("<div>").addClass("wizard-event-add-area").appendTo($div);
                $("<div>").addClass("wizard-event-add-area-tag-box").dxTextBox({
                    scenario: scenario,
                    onFocusOut: function(e) {
                        var curText = e.component.option("text");
                        if (!isReadOnly && (curText) && typeof curText !== "undefined" && curText !== "" && (curText + "").trim() !== "") curText = (curText + "").trim();
                        if (curText !== "") {
                            e.component.beginUpdate();
                            e.component.option("value", "");
                            e.component.option("disabled", true);
                            e.component.endUpdate();
                            callAPI("pm/hierarchy/?action=add_new_event_to_scenario", {"id": scenario.id, "name": curText, "type" : <% = CInt(EventType.Opportunity) %>}, function (res) {
                                if (isValidReply(res) && res.Result == _res_Success && typeof res.Data == "object") {
                                    scenario.events.push(res.Data);
                                    jqHighlight(renderEvent(scenario.itemsContainer, res.Data, scenario));
                                    refreshAllAvailableEventsDataSources();
                                    toggleSidebarMenuEnabled();
                                    e.component.option("disabled", false);
                                    if (!($("input").is(':focus') || $("textarea").is(':focus'))) e.component.focus();
                                    DevExpress.ui.notify("<% = ParseString("%%Opportunity%% %%alternative%% added to the %%objective(l)%%") %>");
                                }
                            }, false, OPT_PLEASE_WAIT_DELAY);
                        } 
                    },
                    onEnterKey: function(e) { e.component.blur(); },
                    placeholder: "<% = ParseString("Type new %%opportunity%% name here…")  %>",
                    readOnly: isReadOnly,
                    visible: true,
                    width: "50%"
                }).appendTo($addContainer).find(".dx-placeholder").addClass("color-opportunity");
                $("<div>").addClass("wizard-event-add-area-tag-box").dxTextBox({
                    scenario: scenario,
                    onFocusOut: function(e) {
                        var curText = e.component.option("text");
                        if (!isReadOnly && (curText) && typeof curText !== "undefined" && curText !== "" && (curText + "").trim() !== "") curText = (curText + "").trim();
                        if (curText !== "") {
                            e.component.beginUpdate();
                            e.component.option("value", "");
                            e.component.option("disabled", true);
                            e.component.endUpdate();
                            callAPI("pm/hierarchy/?action=add_new_event_to_scenario", {"id": scenario.id, "name": curText, "type" : <% = CInt(EventType.Risk) %>}, function (res) {
                                if (isValidReply(res) && res.Result == _res_Success && typeof res.Data == "object") {
                                    scenario.events.push(res.Data);
                                    jqHighlight(renderEvent(scenario.itemsContainer, res.Data, scenario));
                                    refreshAllAvailableEventsDataSources();
                                    toggleSidebarMenuEnabled();
                                    e.component.option("disabled", false);
                                    if (!($("input").is(':focus') || $("textarea").is(':focus'))) e.component.focus();
                                    DevExpress.ui.notify("<% = ParseString("%%Risk%% %%alternative%% added to the %%objective(l)%%") %>");
                                }
                            }, false, OPT_PLEASE_WAIT_DELAY);
                        } 
                    },
                    onEnterKey: function(e) { e.component.blur(); },
                    placeholder: "<% = ParseString("Type new %%risk%% name here…")  %>",
                    readOnly: isReadOnly,
                    visible: true,
                    width: "50%"
                }).appendTo($addContainer).css("margin-left", "3px").find(".dx-placeholder").addClass("color-risk");
            }
            $("<div>").addClass("wizard-event-delete-area").dxSortable({                
                allowReordering: false,
                dropFeedbackMode: "push",
                group: "tasksGroup",
                filter: "",
                moveItemOnDrop: false,
                onAdd: function (e) {
                    dxConfirm(replaceString("{0}", htmlEscape(e.fromData[e.fromIndex].name), resString("msgSureDeleteCommon")), function () {
                        var event_id = e.fromData[e.fromIndex].id;
                        scenario.events.splice(e.fromIndex, 1);
                        $(e.itemElement).empty().remove();
                        callAPI("pm/hierarchy/?action=delete_event_from_scenario", {"id": scenario.id, "event_id": event_id, "hid": <% = CInt(ECHierarchyID.hidLikelihood) %>}, function (res) {
                            if (isValidReply(res) && res.Result == _res_Success) {
                                refreshAllAvailableEventsDataSources();
                                toggleSidebarMenuEnabled();
                                DevExpress.ui.notify("<% = ParseString("%%Alternative%%") %> deleted");
                            }
                        }, false, OPT_PLEASE_WAIT_DELAY);
                    });                    
                }
            }).html('<h5><i class="fas fa-times"></i>&nbsp;<% = ResString("btnDelete") %></h5>').appendTo($div).hide();
        }
    }

    function renderEvents($container, scenario) {
        var $scroll = $("<div>").appendTo($container);
        var $items = $("<div>").appendTo($scroll);
        
        scenario.itemsContainer = $items;

        scenario.events.forEach(function(alt) {
            renderEvent($items, alt, scenario);
        });

        $scroll.addClass("wizard-scrollable-list").dxScrollView({
            direction: "vertical",
            showScrollbar: "always"
        });

        $items.addClass("wizard-sortable-cards").dxSortable({
            allowReordering: !isReadOnly,
            group: "tasksGroup",
            data: scenario.events,
            moveItemOnDrop: true,
            onAdd: function (e) {
                //dxDialog("", function () {
                    // proceed to "onAdd" event handler - "move"
                    var params = { "fromID": e.fromData[e.fromIndex].id, "toID": e.toData.length && e.toIndex >= 0 ? (e.toIndex < e.toData.length ? e.toData[e.toIndex].id : e.toData[e.toIndex - 1].id) : -1, "fromScenarioID": lastScenarioID, "toScenarioID": scenario.id, "move_action" : e.toIndex < e.toData.length ? <% = CInt(NodeMoveAction.nmaBeforeNode) %> : <% = CInt(NodeMoveAction.nmaAfterNode) %>};
                    scenario.events.splice(e.toIndex, 0, e.fromData.splice(e.fromIndex, 1)[0]);
                    callAPI("pm/hierarchy/?action=move_event_to_scenario", params, function (res) {
                        if (isValidReply(res) && res.Result == _res_Success) {
                            //todo refresh other scenarios events lists (may be issues with order)
                            DevExpress.ui.notify("<% = ParseString("%%Alternative%% moved between %%objectives(l)%%") %>");
                        }
                    }, false, OPT_PLEASE_WAIT_DELAY);                
                <%--}, function () {
                    // "copy"
                    e.cancel = true;
                    var params = { "fromID": e.fromData[e.fromIndex].id, "toID": e.toData.length && e.toIndex >= 0 ? (e.toIndex < e.toData.length ? e.toData[e.toIndex].id : e.toData[e.toIndex - 1].id) : -1, "fromScenarioID": lastScenarioID, "toScenarioID": scenario.id, "move_action" : e.toIndex < e.toData.length ? <% = CInt(NodeMoveAction.nmaBeforeNode) %> : <% = CInt(NodeMoveAction.nmaAfterNode) %>};
                    callAPI("pm/hierarchy/?action=copy_event_to_scenario", params, function (res) {
                        if (isValidReply(res) && res.Result == _res_Success) {
                            scenario.events.splice(e.toIndex, 0, res.Data);
                            var element = renderEvent(scenario.itemsContainer, res.Data, scenario);
                            $(e.toData.containerElement).after(element);
                            if (e.toIndex == 0) {
                                scenario.itemsContainer.children().eq(e.toIndex).before(element);
                            } else {
                                scenario.itemsContainer.children().eq(e.toIndex).after(element);
                            }
                            DevExpress.ui.notify("<% = ParseString("%%Alternative%% copied to %%objective(l)%%") %>");
                        }
                    }, false, OPT_PLEASE_WAIT_DELAY);
                }, "Confirmation", "Move " + htmlEscape(ShortString(e.fromData[e.fromIndex].name, 20)) + " to this <% = ParseString("%%Objective(l)%%")%>", "Copy " + htmlEscape(ShortString(e.fromData[e.fromIndex].name, 20)) + " to this <% = ParseString("%%Objective(l)%%")%>");--%>
            },
            onReorder: function (e) {
                var params = { "fromID": e.fromData[e.fromIndex].id, "toID": e.fromData[e.toIndex].id, "hid": -1, "move_action" : e.fromIndex > e.toIndex ? <% = CInt(NodeMoveAction.nmaBeforeNode) %> : <% = CInt(NodeMoveAction.nmaAfterNode) %>};
                scenario.events.splice(e.toIndex, 0, scenario.events.splice(e.fromIndex, 1)[0]);
                //if (e.toIndex == 0) {
                //    scenario.itemsContainer.children().eq(e.toIndex).before(scenario.itemsContainer.children().eq(e.fromIndex));
                //} else {
                //    scenario.itemsContainer.children().eq(e.toIndex).after(scenario.itemsContainer.children().eq(e.fromIndex));
                //}
                callAPI("pm/hierarchy/?action=move_node", params, function (res) {
                    if (isValidReply(res) && res.Result == _res_Success) {
                        DevExpress.ui.notify("<% = ParseString("%%Alternatives%%") %> reordered");
                    }
                }, false, OPT_PLEASE_WAIT_DELAY);
            },
            onDragStart: function (e) {
                lastScenarioID = scenario.id;
                $container.find(".wizard-event-add-area").hide();
                $container.find(".wizard-event-delete-area").show();
            },
            onDragEnd: function (e) {
                $container.find(".wizard-event-add-area").show();
                $container.find(".wizard-event-delete-area").hide();
            }
        });
    }

    function renderEvent($container, alt, scenario) {
        var $item = $("<div>")
            .addClass("wizard-event")
            .addClass("dx-card")
            .addClass("dx-theme-text-color")
            .addClass("dx-theme-background-color")
            .appendTo($container);
        
        alt.containerElement = $item;
        
        $("<div>").addClass("wizard-event-name").appendTo($item).dxTextBox({
            onEnterKey: function (e) { updateNodeName(alt, e.component.option("text"), -1, scenario); },
            onFocusOut: function (e) { updateNodeName(alt, e.component.option("text"), -1, scenario); },
            onFocusIn: onTextBoxFocusIn,
            readOnly: isReadOnly,
            stylingMode: "underlined",
            value : alt.name 
        });
        $("<div>").addClass("wizard-event-description").appendTo($item).dxTextArea({             
            onEnterKey: function (e) { updateNodeDescription(alt, e.component.option("text"), -1); },
            onFocusOut: function (e) { updateNodeDescription(alt, e.component.option("text"), -1); },
            onFocusIn: onTextAreaFocusIn,
            placeholder: "<% = ParseString("Type %%alternative%% description here…")  %>",
            readOnly: isReadOnly,
            stylingMode: "underlined",
            value : alt.description 
        }).toggle(showDescriptions);
        $("<div>").addClass(alt.type == 0 ? "side-marker wizard-event-type-risk" : "side-marker wizard-event-type-reward").appendTo($item);
        $("<div>").addClass(alt.type == 0 ? "wizard-event-type-risk-switch" : "wizard-event-type-reward-switch").text(alt.type == 0 ? lblRisk : lblReward)
            .on("click", function () {
                var newType = alt.type == 0 ? 1 : 0;
                onEventTypeSwitch(this, alt, newType);                
            })
            .appendTo($item);        
        return $item.toggle(eventsDisplayMode == "all" || (alt.type == 0 ? eventsDisplayMode == "risks" : eventsDisplayMode == "rewards"));
    }

    function onEventTypeSwitch(element, alt, newType) {
        // update left side marker
        var marker = $(element).parent().find(".side-marker");
        marker.removeClass("wizard-event-type-risk").removeClass("wizard-event-type-reward");
        marker.addClass(newType == 0 ? "wizard-event-type-risk" : "wizard-event-type-reward");
        // update switch control
        $(element).removeClass("wizard-event-type-risk-switch").removeClass("wizard-event-type-reward-switch");
        $(element).addClass(newType == 0 ? "wizard-event-type-risk-switch" : "wizard-event-type-reward-switch").text(newType == 0 ? lblRisk : lblReward);
        // update text labels for phase 2 view
        $(element).parent().find(".risk-related-text").toggle(newType == 0);
        $(element).parent().find(".reward-related-text").toggle(newType == 1);

        updateEventType(alt, newType);
    }

    function renderObjective($container, alt, objective) {
        var $item = $("<div>")
            .addClass("wizard-objective")
            //.addClass("dx-card")
            .addClass("dx-theme-text-color")
            .addClass("dx-theme-background-color")
            .appendTo($container);
        
        objective.containerElement = $item;


        $("<i>").addClass("fas fa-grip-vertical wizard-objective-drag-handle").appendTo($item);

        $("<div>").addClass("wizard-objective-name").appendTo($item).dxTextBox({
            onEnterKey: function (e) { updateNodeName(objective, e.component.option("text"), <% = CInt(ECHierarchyID.hidImpact) %>, null, alt); },
            onFocusOut: function (e) { updateNodeName(objective, e.component.option("text"), <% = CInt(ECHierarchyID.hidImpact) %>, null, alt); },
            onFocusIn: onTextBoxFocusIn,
            readOnly: isReadOnly,
            stylingMode: "underlined",
            value : objective.name 
        });
        $("<div>").addClass("wizard-objective-description").appendTo($item).dxTextArea({             
            onEnterKey: function (e) { updateNodeDescription(objective, e.component.option("text"), <% = CInt(ECHierarchyID.hidImpact) %>); },
            onFocusOut: function (e) { updateNodeDescription(objective, e.component.option("text"), <% = CInt(ECHierarchyID.hidImpact) %>); },
            onFocusIn: onTextAreaFocusIn,
            placeholder: "<% = ParseString("Type %%objective(i)%% description here…")  %>",
            readOnly: isReadOnly,
            stylingMode: "underlined",
            value : objective.description 
        }).toggle(showDescriptions);       
        return $item;
    }

    function getAvailableEventsDataSource(scenarios_list, selected_scenario) {        
        var retVal = [{key: "<% = ParseString("%%Opportunity%% %%Alternatives%%") %>", items: []}, {key: "<% = ParseString("%%Risk%% %%Alternatives%%") %>", items: []}];
        var selected_scenario_event_ids = $.map(selected_scenario.events, function(n, i) { return n.id; });
        for (var i = 0; i < scenarios_list.length; i++) {
            if (scenarios_list[i].id !== selected_scenario.id) {
                for (var j = 0; j < scenarios_list[i].events.length; j++) {
                    var e = scenarios_list[i].events[j];
                    if (selected_scenario_event_ids.indexOf(e.id) == -1) {
                        delete e.key;
                        retVal[e.type == 0 ? 1 : 0].items.push(e);
                    }
                }
            }
        }
        return retVal;
    }

    var is_cb_init = false;

    function refreshAllAvailableEventsDataSources() {
        if (OPT_ALLOW_MULTIPLE_CONTRIBUTIONS) {
            is_cb_init = true;
            $(".wizard-event-add-area-tag-box").each(function () {
                if ($(this).data("dxTagBox")) {
                    var cb = $(this).dxTagBox("instance");
                    cb.beginUpdate();
                    cb.reset();
                    cb.option("selectedItems", []);
                    cb.option("dataSource", getAvailableEventsDataSource(scenarios, cb.option("scenario")));
                    cb.endUpdate();
                }
            });
            is_cb_init = false;
        }        
    }

    function getAvailableObjectivesDataSource(scenarios_list, selected_alt) {
        var lblEventName = (selected_alt.type == 0 ? lblRisk : lblReward) + " " + selected_alt.name;
        var retVal = [{key: "<% = ParseString("Candidate %%Objectives(i)%% For ") %>" + lblEventName, items: []}];
        var selected_alt_objective_ids = $.map(selected_alt.objectives, function(n, i) { return n.id; });
        for (var i = 0; i < scenarios_list.length; i++) {
            //if (scenarios_list[i].id !== selected_scenario.id) {
            for (var j = 0; j < scenarios_list[i].events.length; j++) {
                if (scenarios_list[i].events[j].id !== selected_alt.id) {
                    var e = scenarios_list[i].events[j];
                    e.objectives.forEach(function (item) {
                        if (selected_alt_objective_ids.indexOf(item.id) == -1) {
                            selected_alt_objective_ids.push(item.id);
                            delete item.key;
                            retVal[0].items.push(item);
                        }
                    });
                    
                }
            }
            //}
        }
        return retVal;
    }

    function refreshAllAvailableObjectivesDataSources() {
        is_cb_init = true;
        $(".wizard-structure-event-add-area-tag-box").each(function () {
            if ($(this).data("dxTagBox")) {
                var cb = $(this).dxTagBox("instance");
                cb.beginUpdate();
                cb.reset();
                cb.option("selectedItems", []);
                cb.option("dataSource", getAvailableObjectivesDataSource(scenarios, cb.option("alt")));
                cb.endUpdate();
            }
        });
        is_cb_init = false;
    }

    var is_editing = false;

    function updateNodeName(alt, value, hid, scenario, parentAlt) {
        if (is_editing) return;
        is_editing = true;
        if (value.trim() !== "" && value.trim() !== alt.name) {
            alt.name = value.trim();
            callAPI("pm/hierarchy/?action=update_node_name", {"id": alt.id, "name": value.trim(), "hid": hid}, function (res) {
                if (isValidReply(res) && res.Result == _res_Success) {
                    refreshAllAvailableEventsDataSources();
                    switch (hid) {
                        case <% = CInt(ECHierarchyID.hidLikelihood) %>:
                            DevExpress.ui.notify("<% = ParseString("%%Objective(l)%%") %> name changed");
                            break;
                        case <% = CInt(ECHierarchyID.hidImpact) %>:
                            DevExpress.ui.notify("<% = ParseString("%%Objective(l)%%") %> name changed");
                            break;
                        default:
                            DevExpress.ui.notify("<% = ParseString("%%Alternative%%") %> name changed");
                    }
                }
            }, false, OPT_PLEASE_WAIT_DELAY);
        }
        if (value == "" || value.trim() == "") {
            switch (hid) {
                case -1: // delete event from scenario
                    if (scenario.events.indexOf(alt) == -1) return;
                    dxConfirm(replaceString("{0}", "<% = ParseString("%%alternative%%") %> " + htmlEscape(alt.name), resString("msgSureDeleteCommon")), function () {
                        scenario.events.splice(scenario.events.indexOf(alt), 1);
                        $(alt.containerElement).empty().remove();
                        callAPI("pm/hierarchy/?action=delete_event_from_scenario", {"id": scenario.id, "event_id": alt.id, "hid": <% = CInt(ECHierarchyID.hidLikelihood) %>}, function (res) {
                            if (isValidReply(res) && res.Result == _res_Success) {
                                refreshAllAvailableEventsDataSources();
                                toggleSidebarMenuEnabled();
                                DevExpress.ui.notify("<% = ParseString("%%Alternative%%") %> deleted");
                            }
                        }, false, OPT_PLEASE_WAIT_DELAY);
                    }, function () {
                        $(alt.containerElement).find(".wizard-event-name").dxTextBox("instance").option("value", alt.name);
                    });
                    break;
                case <% = CInt(ECHierarchyID.hidLikelihood) %>: // delete scenario
                    var scenario = alt;
                    if (scenarios.indexOf(scenario) == -1) return;
                    dxConfirm(replaceString("{0}", "<% = ParseString("%%objective(l)%%") %> " + htmlEscape(scenario.name), resString("msgSureDeleteCommon")), function () {
                        scenarios.splice(scenarios.indexOf(scenario), 1);
                        $(scenario.containerElement).empty().remove();
                        resizePage();
                        callAPI("pm/hierarchy/?action=delete_scenario", {"id": scenario.id}, function (res) {
                            if (isValidReply(res) && res.Result == _res_Success) {
                                refreshAllAvailableEventsDataSources();
                                toggleSidebarMenuEnabled();
                                DevExpress.ui.notify("<% = ParseString("%%objective(l)%%") %> deleted");
                            }
                        }, false, OPT_PLEASE_WAIT_DELAY);
                    }, function () {
                        $(scenario.containerElement).find(".wizard-scenario-name").dxTextBox("instance").option("value", scenario.name);
                    });
                    break;
                case <% = CInt(ECHierarchyID.hidImpact) %>: // remove objective contribution to event
                    var objective = alt;
                    if (parentAlt.objectives.indexOf(objective) == -1) return;
                    dxConfirm(replaceString("{0}", "<% = ParseString("%%objective(i)%%") %> " + htmlEscape(objective.name), resString("msgSureDeleteCommon")), function () {
                        parentAlt.objectives.splice(parentAlt.objectives.indexOf(objective), 1);
                        $(objective.containerElement).empty().remove();
                        callAPI("pm/hierarchy/?action=delete_objective_from_event", {"alt_id": parentAlt.id, "obj_id": objective.id, "hid": <% = CInt(ECHierarchyID.hidLikelihood) %>}, function (res) {
                            if (isValidReply(res) && res.Result == _res_Success) {
                                refreshAllAvailableEventsDataSources();
                                toggleSidebarMenuEnabled();
                                DevExpress.ui.notify("<% = ParseString("%%Objective(i)%%") %> deleted");
                            }
                        }, false, OPT_PLEASE_WAIT_DELAY);                    
                    }, function () {
                        $(objective.containerElement).find(".wizard-objective-name").dxTextBox("instance").option("value", objective.name);
                    });
                    break;            
            }
        }
        is_editing = false;
    }

    function updateNodeDescription(alt, value, hid) {
        if (value !== alt.description) {
            alt.description = value;
            callAPI("pm/hierarchy/?action=update_node_description", {"id": alt.id, "description": value, "hid": hid}, function (res) {
                if (isValidReply(res) && res.Result == _res_Success) {
                    switch (hid) {
                        case <% = CInt(ECHierarchyID.hidLikelihood) %>:
                            DevExpress.ui.notify("<% = ParseString("%%Objective(l)%%") %> description changed");
                            break;
                        case <% = CInt(ECHierarchyID.hidImpact) %>:
                            DevExpress.ui.notify("<% = ParseString("%%Objective(l)%%") %> description changed");
                            break;
                        default:
                            DevExpress.ui.notify("<% = ParseString("%%Alternative%%") %> description changed");
                    }
                }
            }, false, OPT_PLEASE_WAIT_DELAY);
        }
    }

    function updateEventType(alt, value) {
        if (alt.type !== value) {
            alt.type = value;
            callAPI("pm/hierarchy/?action=update_node_type", {"id": alt.id, "type": value, "hid": <% = -1 %>}, function (res) {
                if (isValidReply(res) && res.Result == _res_Success) {
                    refreshAllAvailableEventsDataSources();
                    DevExpress.ui.notify("<% = ParseString("%%Alternative%%") %> type changed");
                }
            }, false, OPT_PLEASE_WAIT_DELAY);
        }
    }

    /* Structure Events  */
    function renderStructureEvent($container, alt, scenario) {
        var $item = $("<div>")
            .addClass("wizard-structure-event")
            .appendTo($container);
        
        var $structureEventTitle = $("<div>").addClass("wizard-structure-event-title").appendTo($item);
        $("<div>").addClass("wizard-structure-event-name risk-related-text").html("<b>" + lblRisk + ": </b>" + htmlEscape(alt.name)).appendTo($structureEventTitle).toggle(alt.type == 0);
        $("<div>").addClass("wizard-structure-event-name reward-related-text").html("<b>" + lblReward + ": </b>" + htmlEscape(alt.name)).appendTo($structureEventTitle).toggle(alt.type == 1);
        $("<div>").addClass("wizard-structure-event-description").text(alt.description).appendTo($structureEventTitle).toggle(showDescriptions);
        $("<div>").addClass(alt.type == 0 ? "side-marker wizard-event-type-risk" : "side-marker wizard-event-type-reward").appendTo($item);
        $("<div>").addClass(alt.type == 0 ? "wizard-event-type-risk-switch" : "wizard-event-type-reward-switch").text(alt.type == 0 ? lblRisk : lblReward)
            .on("click", function () {
                var newType = alt.type == 0 ? 1 : 0;
                onEventTypeSwitch(this, alt, newType);                
            })
            .appendTo($item);

        $("<span>").addClass("note risk-related-text").text("<% = ParseString("This %%risk%% %%alternative%% will result in losses to the following %%objectives(i)%%:")  %>").appendTo($item).toggle(alt.type == 0);
        $("<span>").addClass("note reward-related-text").text("<% = ParseString("This %%opportunity%% %%alternative%% will result in gains to the following %%objectives(i)%%:")  %>").appendTo($item).toggle(alt.type == 1);

        if (!isReadOnly) {
            var objectives_ds = getAvailableObjectivesDataSource(scenarios, alt);
            var $addContainer = $("<div>").addClass("wizard-structure-event-add-area").appendTo($item);
            $("<div>").addClass("wizard-structure-event-add-area-tag-box").dxTagBox({
                alt: alt,
                acceptCustomValue: true,
                applyValueMode: "useButtons",
                dataSource: objectives_ds,
                displayExpr: "name",
                groupComponent: null,
                grouped: true,
                groupRender: null,
                groupTemplate: "group",
                hideSelectedItems: false,
                multiline: true,
                noDataText: "No data to display",
                onChange: null,
                onClosed: null,
                onContentReady: null,
                onCustomItemCreating: function(e) {
                    var selectedItems = $.find(".dx-list-select-checkbox.dx-checkbox-checked");
                    if (selectedItems.length > 0) return;
                    is_cb_init = true;
                    var curText;
                    if ((e.text) && (typeof e.text !== "undefined") && (e.text) && (e.text !== "") && (e.text.trim() !== "")) curText = e.text;
                    if (typeof curText == "undefined") curText = e.component.option("text");
                    if (typeof curText !== "undefined") curText = curText.trim();
                    if (!e.customItem && (curText) && (curText !== "")) {
                        e.component.beginUpdate();
                        e.component.option("text", "");
                        e.component.reset();
                        e.component.endUpdate();
                        e.component.close();
                        e.customItem = curText;
                        callAPI("pm/hierarchy/?action=add_new_objective_to_event", {"id": alt.id, "name": curText, "hid": <% = CInt(ECHierarchyID.hidImpact) %>}, function (res) {
                            if (isValidReply(res) && res.Result == _res_Success && typeof res.Data == "object") {
                                alt.objectives.push(res.Data);
                                renderObjective(alt.objectivesContainer, alt, res.Data);
                                alt.scrollView.scrollTo(alt.scrollView.scrollHeight());
                                refreshAllAvailableObjectivesDataSources();
                                toggleSidebarMenuEnabled();
                                DevExpress.ui.notify("<% = ParseString("%%Objective(i)%% added to the %%alternative%%") %>");
                            }
                        }, false, OPT_PLEASE_WAIT_DELAY);
                    } 
                    setTimeout(function () {is_cb_init = false;}, 50);
                },
                onEnterKey: null,
                onFocusOut: function(e) { 
                    var selectedItems = $.find(".dx-list-select-checkbox.dx-checkbox-checked");
                    if (!is_cb_init && selectedItems.length == 0) {
                        var curText = e.component.option("text");
                        if (typeof curText !== "undefined" && typeof curText !== "object" && curText + "" !== "") {
                            e.component.option("onCustomItemCreating")(e); 
                        }
                    }
                },
                onSelectionChanged: function (e) {
                    var selectedItems = e.component.option("selectedItems");
                    if (!is_cb_init) {
                        is_cb_init = true;
                        e.component.close();
                        if (selectedItems.length > 0) {
                            var selIDs = "";
                            selectedItems.map(function(item) { return item.id; }).forEach(function(itemId) { selIDs += (selIDs == "" ? "" : ",") + itemId; } );
                            callAPI("pm/hierarchy/?action=add_existing_objectives_to_event", {"id": alt.id, "obj_ids": selIDs, "hid": <% = CInt(ECHierarchyID.hidImpact) %>}, function (res) {
                                if (isValidReply(res) && res.Result == _res_Success) {
                                    res.Data.forEach(function(item) { 
                                        alt.objectives.push(item); 
                                        renderObjective(alt.objectivesContainer, alt, item);
                                    });
                                    alt.scrollView.scrollTo(alt.scrollView.scrollHeight());
                                    refreshAllAvailableObjectivesDataSources();
                                    toggleSidebarMenuEnabled();
                                    DevExpress.ui.notify("<% = ParseString("Existing %%objective(i)%% added to %%alternative%%") %>");
                                }
                            }, false, OPT_PLEASE_WAIT_DELAY);
                            e.component.reset();
                        } else {
                            //see if custom field has text
                            //var curText = e.component.option("text");
                            //if ((curText) && typeof curText !== "undefined" && typeof curText !== "object" && curText.length && curText.trim() !== "") {
                            //    e.component.option("onCustomItemCreating")(e);
                            //}
                        }
                        setTimeout(function () {is_cb_init = false;}, 50);
                    }
                },
                onValueChanged: null,
                openOnFieldClick: true,
                placeholder: "<% = ParseString("Type new %%objective(i)%% name here…")  %>",
                readOnly: isReadOnly,
                searchEnabled: true,
                searchMode: "contains",
                searchTimeout: 50,
                selectAllMode: "page",
                showClearButton: false,
                showDataBeforeSearch: true,
                showDropDownButton: true,
                showSelectionControls: true,
                stylingMode: "underlined",                
                valueExpr: "id",
                visible: true,
                width: "100%",
                wrapItemText: false
            }).appendTo($addContainer);
        }

        $("<div>").addClass("wizard-structure-event-delete-area").dxSortable({                
            allowReordering: false,
            dropFeedbackMode: "push",
            group: "tasksStructureGroup" + alt.id,
            filter: "",
            moveItemOnDrop: false,
            onAdd: function (e) {
                dxConfirm(replaceString("{0}", htmlEscape(e.fromData[e.fromIndex].name), resString("msgSureDeleteCommon")), function () {
                    var obj_id = e.fromData[e.fromIndex].id;
                    e.fromData.splice(e.fromIndex, 1);
                    $(e.itemElement).empty().remove();
                    callAPI("pm/hierarchy/?action=delete_objective_from_event", {"alt_id": alt.id, "obj_id": obj_id, "hid": <% = CInt(ECHierarchyID.hidLikelihood) %>}, function (res) {
                        if (isValidReply(res) && res.Result == _res_Success) {
                            refreshAllAvailableEventsDataSources();
                            toggleSidebarMenuEnabled();
                            DevExpress.ui.notify("<% = ParseString("%%Objective(i)%%") %> deleted");
                        }
                    }, false, OPT_PLEASE_WAIT_DELAY);
                });                    
            }
        }).html('<h5><i class="fas fa-times"></i>&nbsp;<% = ResString("btnDelete") %></h5>').appendTo($item).hide();

        renderStrucureObjectives($item, alt);

        return $item.toggle(eventsDisplayMode == "all" || (alt.type == 0 ? eventsDisplayMode == "risks" : eventsDisplayMode == "rewards"));
    }

    function renderStrucureObjectives($container, alt) {
        var $scroll = $("<div>").appendTo($container);
        var $items = $("<div>").appendTo($scroll);
        
        alt.objectivesContainer = $items;

        alt.objectives.forEach(function(obj) {
            renderObjective($items, alt, obj);
        });

        $scroll.addClass("wizard-scrollable-event-objectives-list").dxScrollView({
            direction: "vertical",
            showScrollbar: "always",
            height: 200
        });

        alt.scrollView = $scroll.dxScrollView("instance");

        $items.addClass("wizard-sortable-objectives").dxSortable({
            allowReordering: !isReadOnly,
            group: "tasksStructureGroup" + alt.id,
            data: alt.objectives,
            moveItemOnDrop: true,
            onAdd: function (e) {
                return; //todo
                var params = { "fromID": e.fromData[e.fromIndex].id, "toID": e.toData.length && e.toIndex >= 0 ? (e.toIndex < e.toData.length ? e.toData[e.toIndex].id : e.toData[e.toIndex - 1].id) : -1, "fromAltID": lastEventID, "toAltID": scenario.id, "move_action" : e.toIndex < e.toData.length ? <% = CInt(NodeMoveAction.nmaBeforeNode) %> : <% = CInt(NodeMoveAction.nmaAfterNode) %>};
                scenario.events.splice(e.toIndex, 0, e.fromData.splice(e.fromIndex, 1)[0]);
                callAPI("pm/hierarchy/?action=move_objective_to_alternative", params, function (res) {
                    if (isValidReply(res) && res.Result == _res_Success) {
                        DevExpress.ui.notify("<% = ParseString("%%objectives(i)%% moved between %%alternatives%%") %>");
                    }
                }, false, OPT_PLEASE_WAIT_DELAY);
            },
            onReorder: function (e) {
                var params = { "fromID": e.fromData[e.fromIndex].id, "toID": e.fromData[e.toIndex].id, "hid": <% = (ECHierarchyID.hidImpact)%>, "move_action" : e.fromIndex > e.toIndex ? <% = CInt(NodeMoveAction.nmaBeforeNode) %> : <% = CInt(NodeMoveAction.nmaAfterNode) %>};
                alt.objectives.splice(e.toIndex, 0, alt.objectives.splice(e.fromIndex, 1)[0]);
                callAPI("pm/hierarchy/?action=move_node", params, function (res) {
                    if (isValidReply(res) && res.Result == _res_Success) {
                        DevExpress.ui.notify("<% = ParseString("%%Objectives(i)%%") %> reordered");
                    }
                }, false, OPT_PLEASE_WAIT_DELAY);
            },
            onDragStart: function (e) {
                lastEventID = alt.id;
                $container.find(".wizard-structure-event-add-area").hide();
                $container.find(".wizard-structure-event-delete-area").show();
            },
            onDragEnd: function (e) {
                $container.find(".wizard-structure-event-add-area").show();
                $container.find(".wizard-structure-event-delete-area").hide();
            }
        });
    }
    /* end Structure Events  */

    function toggleSidebarMenuEnabled() {
        var events_count = 0;
        $(scenarios).each(function() {
            events_count += this.events.length;
        });
        menu_setOption(menu_option_alts, events_count > 0);
    }

    function loadScenarios() {
        callAPI("pm/hierarchy/?action=get_scenarios", {}, function (data) {
            if (typeof data == "object") {
                scenarios = data.scenarios;                
                setPhaseView(0);                
            }
            DevExpress.ui.notify("<% = ParseString("%%Objectives(l)%%") %> loaded");
        }, false, OPT_PLEASE_WAIT_DELAY);
    }

    function jqHighlight(element, duration) {
        duration = duration || 800;
        var el = $(element).effect("highlight", {}, duration);
        if (el.length > 0) el[0].scrollIntoView();
    }

    function setPhaseView(value) {
        phase = value;
        
        $("#wizardPageTitle" + (phase == 0 ? 1 : 0)).hide();
        $("#wizardPageTitle" + phase).show();
        $(".phase1").toggle(value == 0);
        $(".phase2").toggle(value > 0);
        
        if (value == 0) {
            initWizard();
        } else {
            renderStructureWizard();
        }
        
        resizePage();
        hideLoadingPanel();
    }

    onSwitchAdvancedMode = function (value) { 
        //$("#toolbar").dxToolbar("instance").option("visible", value);
        setTimeout(function () { resizePage(); }, 200);
    };

    function resizePage(force_redraw) {        
        $("#wizardScenarios").height(100);
        $(".wizard-scrollable-list").height(50);

        var w = $("#wizardScenarios").parent().innerWidth();
        var h = $("#wizardScenarios").parent().innerHeight();

        var scrollableListMinWidth = 360;
        var scrollableListWidth = scrollableListMinWidth;

        
        if (typeof scenarios !== "undefined" && scenarios.length) {
            var n = scenarios.length;

            $(".wizard-scrollable-list").width(scrollableListMinWidth);

            var scrollableListWidth = Math.max(Math.floor((w / n) - (n*0) - 30), scrollableListMinWidth);
            
            $(".wizard-scrollable-list").width(scrollableListWidth);
        }

        $(".wizard-header-content").each(function () { h -= Math.ceil($(this).outerHeight()); });
        $("#wizardScenarios").height(Math.floor(h - 1));
        
        var scrollListHeight = $("#wizardScenarios").innerHeight() - $(".wizard-scenario-title").outerHeight();
        $(".wizard-scrollable-list").height(scrollListHeight);
        $(".wizard-sortable-cards").css("min-height", scrollListHeight);


        /* resize phase 2 screen */
        $("#wizardStructure").height(100);
        $(".wizard-scrollable-objectives-list").height(50);

        w = $("#wizardStructure").parent().innerWidth();
        h = $("#wizardStructure").parent().innerHeight();

        scrollableListWidth = scrollableListMinWidth;
       
        $(".wizard-scrollable-objectives-list").width(scrollableListMinWidth);

        scrollableListWidth = Math.max(Math.floor(w - 50), scrollableListMinWidth);
        
        $(".wizard-scrollable-objectives-list").width(scrollableListWidth - 20);

        $(".wizard-header-content").each(function () { h -= Math.ceil($(this).outerHeight()); });
        $("#wizardStructure").height(Math.floor(h - 1));
        
        scrollListHeight = $("#wizardStructure").innerHeight() - $(".wizard-structure-scenarios-switch").outerHeight() - $(".wizard-scenario-events-title").outerHeight() - 35;
        $(".wizard-scrollable-objectives-list").height("auto");
    }

    resize_custom = resizePage;

    $(document).ready(function () {        
        initToolbar(); 
        loadScenarios();
    });

</script>

    <div id="toolbar" class="dxToolbar wizard-header-content"></div>
    <h5 style="margin: 0.5em 0px 0px 0px" id="wizardPageTitle0" class="wizard-header-content"><% = SafeFormString(ParseString("%%Objectives(l)%%")) %> for <% = SafeFormString(App.ActiveProject.ProjectName) %></h5>
    <h5 style="margin: 0.5em 0px 0px 0px" id="wizardPageTitle1" class="wizard-header-content"><% = SafeFormString(ParseString("%%Objectives(i)%%")) %> for <% = SafeFormString(App.ActiveProject.ProjectName) %></h5>
    <h5 style="margin: 0.5em 0px 0px 0px" id="divDeleteScenario" class="wizard-scenario-delete-area"></h5>
    <h6 class="wizard-phase-text wizard-header-content phase1"><% = SafeFormString("PHASE 1: " + ParseString("Define %%objectives(l)%% and identify %%risk%% and %%opportunity%% %%alternatives%% for each %%objectives(l)%%")) %></h6>
    <h6 class="wizard-phase-text phase2"><% = SafeFormString("PHASE 2: " + ParseString("For each %%risk%% and %%opportunity%% %%alternative%%, identify one or more %%objectives(i)%% to which the %%risk%% or %%opportunity%% %%alternative%% will have consequences")) %></h6>
    <a class="phase2" href="" onclick="setPhaseView(0); return false;" style="margin-left: 10px;"><i class="fas fa-angle-double-left"></i> Phase 1</a>&nbsp;
    <a class="phase1" href="" onclick="setPhaseView(1); return false;" style="float: right; margin-right: 10px;">Phase 2 <i class="fas fa-angle-double-right"></i></a>
    <div id="wizardScenarios" class="wizard-container phase1"></div>
    <div id="wizardStructure" class="wizard-container phase2"></div>

</asp:Content>