<%@ Page Language="VB" Inherits="StructuringPage" Title="TeamTime Structuring" CodeBehind="Structuring.aspx.vb" %>

<asp:Content ID="CSContent" ContentPlaceHolderID="PageContent" runat="Server">
    <script language="javascript" type="text/javascript">

        var WAS_OWNER_DISCONNECTED = false;

        var $user;
        var isAnonymous = <% = Bool2JS(Not App.isAuthorized OrElse PRJ Is Nothing OrElse Not App.CanActiveUserModifyProject(PRJ.ID)) %>;
        var isReadOnly = <% = Bool2JS(App.IsActiveProjectReadOnly) %> && !isAnonymous;
        var isDebug = <%=Bool2JS(IsDebugModeOn)%>;
        var isCheckedAlternativesCopiedToWhiteboard = false;
        var isRisk = <%=Bool2JS(App.isRiskEnabled)%>;
        var isAllowedSwitchDrawingModeOnCtrlKey = false;
        
        var lblObjective = '<%=JS_SafeString(ParseString("%%objective%%"))%>';
        var lblObjectiveL = '<%=JS_SafeString(ResString("lblObjectiveL"))%>';
        var lblObjectiveI = '<%=JS_SafeString(ResString("lblObjectiveI"))%>';
        var lblEvent = '<%=JS_SafeString(ResString("lblEvent"))%>';

        var goalName = "<%=JS_SafeString(PM.ActiveObjectives.Nodes(0).NodeName)%>";

        var ajaxMethod = _method_POST; // don't use method GET because of caching
        var please_wait_delay = 2000; //ms
    
        var mmAlts = 0, mmSources = 1, mmImpacts = 2; //mmProsCons = 2, mmImpacts = 3, mmSourcesContribution = 4, mmConsequencesContribution = 5, mmAltsSources = 6, mmAltsConsequences = 7;
        var meetingTreeMode = <%=PM.Parameters.CS_TreeMode%>;
        var meetingBoardMode = <%=PM.Parameters.CS_BoardMode%>;
        
        var smSynchronous = 0, smAsynchronous = 1;
        var meetingSynchMode = <%=PM.Parameters.CS_MeetingSynchMode%>;

        var msInActive   = <%=Structuring.MeetingState.InActive%>, msActive = <%=Structuring.MeetingState.Active%>, msStopped = <%=Structuring.MeetingState.Stopped%>, msPaused = <%=Structuring.MeetingState.Paused%>, msBooted = <%=Structuring.MeetingState.Booted%>, msOwnerDisconnected = <%=Structuring.MeetingState.OwnerDisconnected%>;
        var meetingState = <%=PM.Parameters.CS_MeetingState%>;

        var meetingOwner = <%=PM.Parameters.CS_MeetingOwner%>;

        var _DEFAULT_ALT_TITLE = "<%=JS_SafeString(If(PM.Parameters.CS_DefaultAlternativeTitle <> "", PM.Parameters.CS_DefaultAlternativeTitle, ParseString("New %%Alternative%%")))%>";
        var _DEFAULT_SOURCE_TITLE = "<%=JS_SafeString(If(PM.Parameters.CS_DefaultSourceTitle <> "", PM.Parameters.CS_DefaultSourceTitle, If(PM.IsRiskProject, ParseString("New %%Objective(l)%%"), ParseString("New %%Objective%%"))))%>";
        var _DEFAULT_OBJ_TITLE = "<%=JS_SafeString(If(PM.Parameters.CS_DefaultObjectiveTitle <> "", PM.Parameters.CS_DefaultObjectiveTitle, If(PM.IsRiskProject, ParseString("New %%Objective(i)%%"), ParseString("New %%Objective%%"))))%>";

        var _DEFAULT_COLOR_ALT = "<%=PM.Parameters.CS_DefaultAlternativeColor%>";
        var _DEFAULT_COLOR_SOURCE = "<%=PM.Parameters.CS_DefaultSourceColor%>";
        var _DEFAULT_COLOR_OBJ = "<%=PM.Parameters.CS_DefaultObjectiveColor%>";
    
        var _DEFAULT_ITEM_WIDTH  = "<%=JS_SafeString(PM.Parameters.CS_ItemWidth)%>";
        var _DEFAULT_ITEM_HEIGHT = "<%=JS_SafeString(PM.Parameters.CS_ItemHeight)%>";

        var _MIN_ITEM_WIDTH = 100;
        var _MIN_ITEM_HEIGHT = 35;
        var _MAX_ITEM_SIZE = 1000;

        var _DEFAULT_PROS_CONS_PANE_WIDTH  = 300;
        var _DEFAULT_PROS_CONS_PANE_HEIGHT = 150;

        var whiteboardWidth  = <%=WhiteboardWidth%>;
        var whiteboardHeight = <%=WhiteboardHeight%>;

        var sessWbZoom = localStorage.getItem("CS_WbZoom<%=App.Antigua_MeetingID%>");
        var whiteboardZoom = !(sessWbZoom) || typeof sessWbZoom == "undefined" ? 100 : str2int(sessWbZoom);

        var sessWbMode = localStorage.getItem("CS_WbMode<%=App.Antigua_MeetingID%>");
        var whiteboardMode = (!(sessWbMode) || typeof sessWbMode == "undefined") ? "normal" : sessWbMode; // "normal" or "drawing"
        var whiteboardDrawingData = [<%=PM.Parameters.CS_MeetingWhiteboardDrawingData%>];
         
        var sessSplitterWidth = localStorage.getItem("CS_SplitterWidth<%=App.Antigua_MeetingID%>");
        var splitterWidth = !(sessSplitterWidth) || typeof sessSplitterWidth == "undefined" ? "30%" : str2int(sessSplitterWidth);        

        var alternatives = [<%=ServiceConnection.GetAlternativesJSON()%>];
        var sources   = [<%=ServiceConnection.GetObjectivesJSON(ECHierarchyID.hidLikelihood)%>];
        var impacts   = [<%=If(PM.IsRiskProject, ServiceConnection.GetObjectivesJSON(ECHierarchyID.hidImpact), "")%>];
        <%--var objectives   = sources;
        <%If ServiceConnection.CurrentMeeting.Info.ProjectManager.IsRiskProject AndAlso ServiceConnection.CurrentMeeting.Info.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact Then%>
        objectives = impacts;
        <%End If%>--%>
        var board_nodes  = [<% = ServiceConnection.GetWhiteboardNodesJSON(True) %>];
        var location_hid = meetingBoardMode == mmImpacts ? <%=GUILocation.BoardImpact%> : <%=GUILocation.Board%>;

        var is_editing = false;

        function objectives() {
            if (meetingTreeMode == mmSources) return sources;
            if (meetingTreeMode == mmImpacts) return impacts;
            return [];
            <%--if (location_hid == <%=CInt(GUILocation.Board)%>) return sources;
            if (location_hid == <%=CInt(GUILocation.BoardImpact)%>) return impacts;--%>
        }

        var pros_cons_items = [];
        var pros_cons_datasource = [];
        var is_all_pros_cons_shown = false;

        var cur_draggableItem, colorDlg;
        var isManuallyChanged = true;
        var selNodesIDs = "";

        var timeStamp = "<%=DateTime.Now.Ticks%>";
        var cmdOwner  =  <%=CmdOwner%>;
        var UserName  =  "<%=JS_SafeString(UserName)%>";
        var UserEmail =  "<%=JS_SafeString(UserEmail)%>";

        var isColorCoding = <%=Bool2JS(PM.Parameters.CS_ColorCodingByUser)%>;
        var isMeetingLocked = <%=Bool2JS(PM.Parameters.CS_MeetingLockedByPM)%>;

        var lastZIndex = 5;    
        var OPT_USER_LIST_HIGHLIGHT_TIMOUT = 3500;
        var OPT_CHAT_MESSAGE_HIGHLIGHT_TIMOUT = 10000;
        var OPT_CHAT_MESSAGE_MAX_SIZE = 500;
        var OPT_CHAT_MESSAGE_SHOW_TIMESTAMP = true;   
        
        var confirm_shown = false;

        function isProsCons() {
            return !isRisk && meetingTreeMode == mmSources && meetingBoardMode == mmAlts;
        }

        /* Toolbar */
        var toolbarItems = [
            {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: isAnonymous,
                options: {
                    icon: "fas fa-sync", text: "Refresh", hint: "Refresh",
                    elementAttr: {id: 'btn_refresh_anon'},
                    onClick: function (e) {
                        reloadPage();
                    }
                }
            }, {
                location: 'before', locateInMenu: 'never',  template: '<span class="" id="lblMeetingStatusTitle"></div>', visible: isAnonymous 
            }, {   
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: true,
                options: {
                    icon: "fas fa-play", text: "Start", hint: "Start",
                    template: function() {
                        return $('<i class="dx-icon fas fa-play"></i><span class="dx-button-text nowide1200">Start</span>');
                    },
                    disabled: true,
                    elementAttr: {id: 'btn_start'},
                    onClick: function (e) {
                        setMeetingState(msActive);
                        e.component.option("disabled", true);
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: true,
                options: {
                    icon: "fas fa-pause", text: "Pause", hint: "Pause",
                    template: function() {
                        return $('<i class="dx-icon fas fa-pause"></i><span class="dx-button-text nowide1200">Pause</span>');
                    },
                    disabled: false,
                    elementAttr: {id: 'btn_pause'},
                    onClick: function (e) {
                        setMeetingState(msPaused);
                        e.component.option("disabled", true);
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: true,
                options: {
                    icon: "fas fa-stop", text: "Stop", hint: "Stop",
                    template: function() {
                        return $('<i class="dx-icon fas fa-stop cs-ban-sign"></i><span class="dx-button-text nowide1200">Stop</span>');
                    },
                    disabled: false,
                    elementAttr: {id: 'btn_stop'},
                    onClick: function (e) {
                        stopMeeting();
                        e.component.option("disabled", true);                        
                    }
                }
            }, { 
                location: 'before', locateInMenu: 'never', visible: !isAnonymous, template: '<div style="width: 6px; height: 1px;"></div>' 
            }, {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: !isAnonymous,
                options: {
                    icon: "fas fa-sync", text: "Refresh", hint: "Refresh",
                    template: function() {
                        return $('<i class="dx-icon fas fa-sync"></i><span class="dx-button-text nowide1200">Refresh</span>');
                    },
                    elementAttr: {id: 'btn_refresh'},
                    onClick: function (e) {
                        reloadPage();
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: !isAnonymous,
                options: {
                    icon: isMeetingLocked ? "fas fa-lock" : "fas fa-unlock", 
                    text: isMeetingLocked ? "Unlock" : "Lock", 
                    hint: isMeetingLocked ? "Unlock the meeting" : "Lock the meeting",
                    elementAttr: {id: 'btn_lock'},
                    onClick: function (e) {
                        var params = "&is_locked=" + !isMeetingLocked;
                        csAction(<%=Command.SetMeetingLock%>, params);
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'auto',                
                widget: 'dxDropDownButton',
                options: {
                    disabled: isReadOnly, //|| alternatives.length == 0,
                    icon: "fas fa-exchange-alt",
                    text: "Copy <%=ParseString("%%Alternatives%%")%>",                    
                    elementAttr: { id: 'btnCopyAllAlts' },
                    displayExpr: "text",
                    useSelectMode: false,
                    wrapItemText: false,
                    <%--template: function() {
                        return $('<i class="dx-icon fas fa-exchange-alt"></i><span class="dx-button-text nowide1200">Copy <%=ParseString("%%Alternatives%%")%></span>');
                    },--%>
                    dropDownOptions: {
                        width: 510,
                    },
                    items: [],
                    onButtonClick: function (e) {
                        var items = [
                            {
                                text: resString("optCopyAlternativesToBoardTile"),
                                icon: "fas fa-grip-horizontal",
                                
                                disabled: isReadOnly,
                                onClick: function () {
                                    csAction(<%=Command.CopyAllAltsToBoard%>, "&mode=<%=CInt(AntiguaCopyToBoardEventArgs.CopyModes.cmTile)%>&w=" + _DEFAULT_ITEM_WIDTH + "&h=" + _DEFAULT_ITEM_HEIGHT);
                                }
                            }, {
                                text: resString("optCopyAlternativesToBoardList"),
                                icon: "fas fa-grip-vertical",
                                disabled: isReadOnly,
                                onClick: function () {
                                    csAction(<%=Command.CopyAllAltsToBoard%>, "&mode=<%=CInt(AntiguaCopyToBoardEventArgs.CopyModes.cmList)%>&w=" + _DEFAULT_ITEM_WIDTH + "&h=" + _DEFAULT_ITEM_HEIGHT);
                                }
                            }];
                        e.component.option('items', items);
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'auto',
                visible: true,
                template: function() {
                    return $('<div class="cs-toolbar-users cell_clickable nowide1200" onclick="toggleOnlineParticipantsView(); return false;">Participants:</div><div id="lblOnlineUsers" class="cs-toolbar-online" onclick="toggleOnlineParticipantsView(); return false;"><i class="fas fa-sync fa-spin" style="line-height:21px; font-size:0.9em"></i></div>');
                },
            },
            {
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButton',
                visible: !isAnonymous && (meetingOwner == cmdOwner),
                options: {
                    icon: "fas fa-users", hint: "<% = ParseString("%%Model%% Participants…") %>",
                    elementAttr: {id: 'btn_manage_users'},
                    width: 30,
                    onClick: function () {
                        showManageParticipantsWindow();
                    }
                }
            },
            {
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButton',
                visible: true,
                options: {
                    icon: "fas fa-envelope", text: "Invite", hint: "<%=ResString("titleAntiguaInvite")%>",
                    disabled: false,
                    elementAttr: {id: 'btn_invite'},
                    onClick: function (e) {
                        showInvitationDialog();
                    }
                }
            },
            {
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButton',
                visible: true,
                options: {
                    icon: "fas fa-link", text: "", hint: "<% =JS_SafeString(ResString("lblGetLink")) %>",
                    disabled: false,
                    elementAttr: {id: 'btn_link'},
                    onClick: function (e) {
                        showProjectLinkDialog();
                    }
                }
            },
            {
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButton',
                visible: true,
                options: {
                    icon: "fas fa-comments", text: "Chat", hint: "Messages",
                    disabled: false,
                    elementAttr: {id: 'btn_messages'},
                    onClick: function (e) {
                        if (toggleOnlineParticipantsView()) {
                            $("#divNewMessage").dxTextBox("instance").focus();
                        }
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                visible: false,
                height: "22px",
                options: {
                    searchEnabled: true,
                    valueExpr: "Value",
                    value: whiteboardZoom,
                    displayExpr: "Text",
                    disabled: false,
                    width: 120,
                    elementAttr: {id: 'btn_zoom'},
                    onSelectionChanged: function (e) { 
                        if (e.selectedItem.Value * 1 != whiteboardZoom) {
                            whiteboardZoom = e.selectedItem.Value * 1;
                            setZoom(e.selectedItem.Value);
                            localStorage.setItem("CS_WbZoom<%=App.Antigua_MeetingID%>", whiteboardZoom);
                        }
                    },
                    //html: '<i class="fas fa-search-plus"></i>',
                    items: [{"Value": 130, "Text": "130%"}, {"Value": 120, "Text": "120%"}, {"Value": 110, "Text": "110%"}, {"Value": 100, "Text": "100%"}, {"Value": 90, "Text": "90%"}, {"Value": 80, "Text": "80%"}, {"Value": 70, "Text": "70%"}, {"Value": 60, "Text": "60%"}, {"Value": 50, "Text": "50%"}, {"Value": 40, "Text": "40%"}, {"Value": 30, "Text": "30%"}, {"Value": -1, "Text": "Fit Page Width"}]
                }
            }, {
                location: 'after',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: !isAnonymous,
                options: {
                    beginGroup: true,
                    icon: "fas fa-cog", text: "Preferences", hint: "Preferences",
                    template: function() {
                        return $('<i class="dx-icon fas fa-cog"></i><span class="dx-button-text nowide1200">Preferences</span>');
                    },
                    disabled: isAnonymous,
                    elementAttr: {id: 'btn_settings'},
                    onClick: function (e) {
                        settingsDialog();
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: false && !isAnonymous,
                options: {
                    icon: "fas fa-question", text: "Help", hint: "Help",
                    disabled: false,
                    elementAttr: {id: 'btn_help'},
                    onClick: function (e) {
                        callAjax("action=get_help_url", onGetHelpUrl, ajaxMethod, false, "", true, please_wait_delay);
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                template: '<i id="imgLoading" class="fas fa-spinner fa-spin" style="display: none;"></i>'
            }
        ];

    function onGetHelpUrl(data) {
        onShowHelp(data);
    }

    function setZoom(value) {
        if (value < 0) { // Fit Page
            var w = $("#divWhiteboardContainer").width();
            var h = $("#divWhiteboardContainer").height();
            
            // approach 0 - always fit (both width and height)
            //var ratio0 = (w / whiteboardWidth) * 100;
            //var ratio1 = (h / whiteboardHeight) * 100;
            //value = Math.min(ratio0, ratio1);

            // approach 1 - fit page width
            value = (w / whiteboardWidth) * 100;
        }
        //$(".cs-whiteboard, .draggable-item").css({ "zoom": value + "%", "-moz-transform": "scale(" + (value/100) + ")", "-webkit-transform": "scale(" + (value/100) + ")"});
        $(".cs-whiteboard, .draggable-item").css({ "transform-origin": "0 0", "transform": "scale(" + (value/100) + ")" });
    }

    function updateToolbar() {     
        $("#toolbar").dxToolbar("instance").beginUpdate();
        if ($("#btn_switch_view").data("dxButtonGroup")) {
            var btn_switch_view = $("#btn_switch_view").dxButtonGroup("instance");
            isManuallyChanged = false;            
            btn_switch_view.option("selectedItemKeys", [meetingBoardMode]);
            btn_switch_view.option("disabled", meetingState !== msActive);
            if (isAnonymous) {
                var items = btn_switch_view.option("items");
                var sel_items = btn_switch_view.option("selectedItemKeys");
                for (var i = 0; i < items.length; i++) {
                    items[i].disabled = ($.inArray(items[i].ID, sel_items) == -1) && isModeSwitchDisabled();
                }
                btn_switch_view.repaint();
            }            
            isManuallyChanged = true;

            if (btn_switch_view.option("focusedElement") && btn_switch_view.option("focusedElement").length) {
                var lblStatus = btn_switch_view.option("focusedElement")[0].textContent + "<span class='nowide1000'><% = ActiveProjectDescription %></span>";
                $("#lblMeetingStatusTitle").html(lblStatus).prop("title", lblStatus);
            }
        }

        var lbl = document.getElementById("lblTitle0");
        if ((lbl)) lbl.innerHTML = isProsCons() ? "<%=JS_SafeString(String.Format(ResString("lblTTPCHelpTitle"), PRJ.ProjectName))%>" : "<%=JS_SafeString(String.Format(ResString("lblTTHelpTitle"), PRJ.ProjectName))%>";

        lbl = document.getElementById("lblTitle1");
        if ((lbl)) lbl.innerHTML = replaceString('{0}', meetingBoardMode == mmAlts ? lblEvent : (isRisk ? (meetingBoardMode == mmImpacts ? lblObjectiveI : lblObjectiveL) : lblObjective), '<%=JS_SafeString(ResString("lblTTHelpLabel"))%>');
        
        lbl.style.display = isProsCons() ? "none" : "";
        
        var d = $("#btn_all_proscons");
        if ((d) && (d.length)) {
            if (!isRisk && (meetingState == msActive && meetingBoardMode == mmAlts)) {
                d.show();
            } else {
                d.hide();
            }
        }

        if ($("#btn_lock").data("dxButton")) {
            var btn_lock = $("#btn_lock").dxButton("instance");
            btn_lock.option("visible", meetingOwner == cmdOwner);
            btn_lock.option("icon", isMeetingLocked ? "fas fa-lock" : "fas fa-unlock");
            //btn_lock.option("icon", isMeetingLocked ? "fas fa-lock-open" : "fas fa-user-lock");
            $("#btn_lock").find("i").removeClass("warning-button");
            if (isMeetingLocked) $("#btn_lock").find("i").addClass("warning-button");
            btn_lock.option("hint", isMeetingLocked ? "Unlock the meeting" : "Lock the meeting");
            btn_lock.option("text", isMeetingLocked ? "Unlock" : "Lock");
            btn_lock.option("disabled", meetingState != msActive);
        }

        if ($("#btnCopyAllAlts").data("dxDropDownButton")) {
            var btn_copy_alts = $("#btnCopyAllAlts").dxDropDownButton("instance");
            btn_copy_alts.option("visible", !isAnonymous);
            btn_copy_alts.option("disabled", !meetingBoardMode == mmAlts || meetingState != msActive);
        }

        if ($("#btn_zoom").data("dxSelectBox")) {
            var btn_zoom = $("#btn_zoom").dxSelectBox("instance");
            btn_zoom.option("disabled", meetingState != msActive);
        }

        $("#toolbar").dxToolbar("instance").endUpdate();

        var div_tree_tabs = $("#divTreeTabs").dxButtonGroup("instance");
        div_tree_tabs.option("selectedItemKeys", [meetingTreeMode]);
        if (isAnonymous) {
            var items =  div_tree_tabs.option("items");
            var sel_items = div_tree_tabs.option("selectedItemKeys");
            for (var i = 0; i < items.length; i++) {
                items[i].disabled = ($.inArray(items[i].ID, sel_items) == -1) && isModeSwitchDisabled();
            }
            div_tree_tabs.repaint();
        }

        updateGoalInfodocIcon();
    }

    function updateGoalInfodocIcon() {
        if ($("#btnEditGoalInfodoc").length > 0) {
            $("#btnEditGoalInfodoc").attr("class", "fas fa-info-circle " + (!sources[0].hasinfodoc ? "ec-icon-disabled" : "ec-icon"));
        }
    }

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: toolbarItems
        });

        $("#toolbar").css("margin-bottom", 0);

        DevExpress.config({
            floatingActionButtonConfig: {
                position: {
                    of: "#tdContent",
                    my: "right top",
                    at: "right top",
                    offset: "-25 15"
                }
            }
        });
    }

    function toggleAllProsCons(e) {
        is_all_pros_cons_shown = !is_all_pros_cons_shown;
        if (!isAnonymous) {
            var params = "&mode=all&is_open=" + is_all_pros_cons_shown;
            csAction(<%=Command.SwitchProsCons%>, params);
        }
        SwitchProsConsForAll(is_all_pros_cons_shown);
    }

    function hideAllInfodocs() {
        $("div.draggable-item-tooltip-div").each(function () {
            if ($(this).data("dxTooltip")) {
                var tt = $(this).dxTooltip("instance");
                tt.hide();
            }
        });
    }

    function initTreeTabs() {
        $("#divTreeTabs").dxButtonGroup({   
            disabled:  meetingState !== msActive,
            focusStateEnabled: false,
            keyExpr: "ID",
            selectedItemKeys: [meetingTreeMode],
            onItemClick: function (e) { 
                var newValue = e.itemData.ID * 1;
                if (isManuallyChanged && meetingTreeMode !== newValue) {
                    meetingTreeMode = newValue;
                    if (!isAnonymous && meetingSynchMode == smSynchronous) {
                        csAction(<%=Command.SetMeetingMode%>, "&tree_mode=" + meetingTreeMode + "&board_mode=" + meetingBoardMode);
                    } else {
                        setMeetingMode();
                    }
                }
            },
            selectionMode: "single",
            items: [{
                "ID": mmAlts,
                "hint": "<%=ParseString("%%Alternatives%%")%>", 
                "icon": "fas fa-th-list",
                template: function() {
                    return $('<i class="dx-icon fas fa-th-list"></i><span class="dx-button-text panel-nowide400"> <%=ParseString("%%Alternatives%%")%></span>');
                },
            }<% If Not PM.IsRiskProject Then %>, {
                "ID": mmSources, 
                "hint": "<%=ParseString("%%Objectives%%")%>", 
                "icon": "fas fa-sitemap",
                template: function() {
                    return $('<i class="dx-icon fas fa-sitemap"></i><span class="dx-button-text panel-nowide400"> <%=ParseString("%%Objectives%%")%></span>');
                },
            }<% End If %><% If PM.IsRiskProject Then %>, {
                "ID": mmSources, 
                "hint": "<%=ParseString("%%Objectives(l)%%")%>", 
                "icon": "fas fa-sitemap",
                template: function() {
                    return $('<i class="dx-icon fas fa-sitemap icon-likelihood"></i><span class="dx-button-text panel-nowide400"> <%=ParseString("%%Objectives(l)%%")%></span>');
                },
            }, {
                "ID": mmImpacts, 
                "hint": "<%=ParseString("%%Objectives(i)%%")%>", 
                "icon": "fas fa-sitemap",
                template: function() {
                    return $('<i class="dx-icon fas fa-sitemap icon-impact"></i><span class="dx-button-text panel-nowide400"> <%=ParseString("%%Objectives(i)%%")%></span>');
                },
            }<% End If %>]
        });
        
    }

    function isModeSwitchDisabled() {
        return isAnonymous && meetingSynchMode == smSynchronous;
    }

    function initWhiteboardTabs() {
        $("#btn_switch_view").dxButtonGroup({   
            disabled:  meetingState !== msActive,
            focusStateEnabled: false,
            keyExpr: "ID",
            selectedItemKeys: [meetingBoardMode],
            onItemClick: function (e) { 
                var newValue = e.itemData.ID * 1;
                if (isManuallyChanged && meetingBoardMode !== newValue) {
                    meetingBoardMode = newValue;
                    if (!isAnonymous && meetingSynchMode == smSynchronous) {
                        csAction(<%=Command.SetMeetingMode%>, "&tree_mode=" + meetingTreeMode + "&board_mode=" + meetingBoardMode);
                    } else {
                        setMeetingMode();
                    }
                }
                e.event.stopPropagation();
                e.event.preventDefault();
            },
            selectionMode: "single",
            items: [{
                    "ID": mmAlts, 
                    "hint": "<%=ResString("optTTVisualBrainstorming")%>", 
                    "icon": "fas fa-th-list",
                    template: function() {
                        return $('<i class="dx-icon fas fa-th-list"></i><span class="dx-button-text panel-nowide700"> <%=ResString("optTTVisualBrainstorming")%></span>');
                    },
                    //"icon": "icon icon-ec-font-alternatives"
                }, <% If PM.IsRiskProject Then %>{
                    "ID": mmSources, 
                    "hint": "<%=ResString("optTTVisualStructuringL")%>", 
                    "icon": "fas fa-sitemap",
                    template: function() {
                        return $('<i class="dx-icon fas fa-sitemap icon-likelihood"></i><span class="dx-button-text panel-nowide700"> <%=ResString("optTTVisualStructuringL")%></span>');
                    },
                    //"icon": "icon icon-ec-font-objectives"
                }, {
                    "ID": mmImpacts, 
                    "hint": "<%=ResString("optTTVisualStructuringI")%>", 
                    "icon": "fas fa-sitemap",
                    template: function() {
                        return $('<i class="dx-icon fas fa-sitemap icon-impact"></i><span class="dx-button-text panel-nowide700"> <%=ResString("optTTVisualStructuringI")%></span>');
                    },
                    //"icon": "icon icon-ec-font-objectives"
                }<% Else %>{
                    "ID": mmSources, 
                    "hint": "<%=ResString("optTTVisualStructuring")%>", 
                    "icon": "fas fa-sitemap",
                    template: function() {
                        return $('<i class="dx-icon fas fa-sitemap"></i><span class="dx-button-text panel-nowide700"> <%=ResString("optTTVisualStructuring")%></span>');
                    },
                    //"icon": "icon icon-ec-font-objectives"
                }<% End If %>]
        });
    }
    /* end Toolbar */

    /* Settings Dialog */
    var popupSettings = null, 
        popupOptions = {
            animation: null,
            width: 610,
            height: "auto",
            contentTemplate: function() {
                return  $("<div class='dx-fieldset'></div>").append(
                    $("<div class='dx-fieldset-header'>Draggable Items</div>"),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Size")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='rbItemSize'></div>"))),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div class='dx-field-label' style='width: 70px; text-align: right;'>Width:</div>"), $("<div id='tbItemCustomWidth' style='display: inline-block; float: left; margin: 3px 0px;'></div>"), $("<div class='dx-field-label' style='width: 70px; text-align: right;'>Height: </div>"), $("<div id='tbItemCustomHeight' style='display: inline-block; float: left; margin: 3px 0px;'></div>"))),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Default %%Alternative%% Title")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='tbDefaultAltTitle'></div>"))),
                    <% If PM.IsRiskProject Then %>
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Default %%Objective(l)%% Title")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='tbDefaultSourceTitle'></div>"))),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Default %%Objective(i)%% Title")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='tbDefaultObjTitle'></div>"))),
                    <% Else %>
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Default %%Objective%% Title")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='tbDefaultObjTitle'></div>"))),
                    <% End If %>
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Default %%Alternative%% Color")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='colorAlts'></div>"))),
                    <% If PM.IsRiskProject Then %>
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Default %%Objective(l)%% Color")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='colorSources'></div>"))),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Default %%Objective(i)%% Color")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='colorObjs'></div>"))),
                    <% Else %>
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Default %%Objective%% Color")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='colorObjs'></div>"))),
                    <% End If %>
                    $("<br>"),
                    $("<div class='dx-fieldset-header'>Options</div>"),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'>Meeting Mode Synchronization Mode</div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='cbSynchMode'></div>"), $("<div class='dx-field-item-help-text'>There are two modes - synchronous screen mode where everyone's screens are kept in sync and asynchronous screen mode where each participant can individually select what to see on their computer</div>"))),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'>Drawing Stays On Screen</div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='cbDrawingStays' style='float: none;'></div>"))),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'>Color Coding By Participant</div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='cbColorCoding' style='float: none;'></div>"))),

                    $("<div id='btnSettingsClose' style='margin-top: 10px; margin-bottom: 10px; float: right;'></div>")
                );
            },
            showTitle: true,
            title: "Preferences",
            dragEnabled: true,
            closeOnOutsideClick: true
        };

        var itemSizes = [{"text" : "Small", "value" : 0, "w" : 120, "h" : 35}, {"text" : "Normal", "value" : 1, "w" : 200, "h" : 50}, {"text" : "Large", "value" : 2, "w" : 300, "h" : 120}, {"text" : "Custom", "value" : 3, "w" : _DEFAULT_ITEM_WIDTH, "h" : _DEFAULT_ITEM_HEIGHT}];

        function settingsDialog() {
            $("#divOnlineUsers").hide();

            if (popupSettings) $(".popupSettings").remove();
            var $popupContainer = $("<div></div>").addClass("popupSettings").appendTo($("#popupSettings"));
            popupSettings = $popupContainer.dxPopup(popupOptions).dxPopup("instance");
            popupSettings.show();

            var radioValue = -1;
            for (var i = 0; i < itemSizes.length; i++) {
                if (_DEFAULT_ITEM_WIDTH == itemSizes[i]["w"] && _DEFAULT_ITEM_HEIGHT == itemSizes[i]["h"]) {
                    radioValue = itemSizes[i]["value"];
                    break;
                }
            }
            if (radioValue < 0) { radioValue = 3; itemSizes[itemSizes.length - 1]["w"] = _DEFAULT_ITEM_WIDTH; itemSizes[itemSizes.length - 1]["h"] = _DEFAULT_ITEM_HEIGHT; }

            $("#rbItemSize").dxRadioGroup({ 
                items: itemSizes,
                layout: "horizontal",
                displayExpr: "text",
                valueExpr: "value",
                value: radioValue,
                onValueChanged: function (e) {
                    if (e.value * 1 != radioValue) {
                        radioValue = e.value * 1;
                        
                        isManuallyChanged = false;
                        
                        var tbH = $("#tbItemCustomHeight").dxNumberBox("instance");
                        var tbW = $("#tbItemCustomWidth").dxNumberBox("instance");
                                               
                        var w = itemSizes[radioValue]["w"];
                        var h = itemSizes[radioValue]["h"];

                        tbH.option("value", h);
                        tbW.option("value", w);

                        tbH.option("disabled", radioValue != 3);
                        tbW.option("disabled", radioValue != 3);

                        isManuallyChanged = true;

                        csAction(<%=Command.Setting%>, "&name=item_size&width=" + w + "&height=" + h);
                    }
                }
            });

            $("#tbItemCustomHeight").dxNumberBox({
                value: _DEFAULT_ITEM_HEIGHT,
                min: _MIN_ITEM_HEIGHT,
                max: _MAX_ITEM_SIZE,
                width: 65,
                disabled: radioValue != 3,
                showSpinButtons: true,
                onValueChanged: function (e) {
                    if (isManuallyChanged && e.value * 1 != _DEFAULT_ITEM_HEIGHT) {
                        itemSizes[3]["h"] = e.value * 1;

                        var w = itemSizes[3]["w"];
                        var h = itemSizes[3]["h"];

                        csAction(<%=Command.Setting%>, "&name=item_size&width=" + w + "&height=" + h);
                    }
                }
            });

            $("#tbItemCustomWidth").dxNumberBox({
                value: _DEFAULT_ITEM_WIDTH,
                min: _MIN_ITEM_WIDTH,
                max: _MAX_ITEM_SIZE,
                width: 65,
                disabled: radioValue != 3,
                showSpinButtons: true,
                onValueChanged: function (e) {
                    if (isManuallyChanged && e.value * 1 != _DEFAULT_ITEM_WIDTH) {
                        itemSizes[3]["w"] = e.value * 1;

                        var w = itemSizes[3]["w"];
                        var h = itemSizes[3]["h"];

                        csAction(<%=Command.Setting%>, "&name=item_size&width=" + w + "&height=" + h);
                    }
                }
            });

            $("#tbDefaultAltTitle").dxTextBox({ 
                value: _DEFAULT_ALT_TITLE,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=default_alt_title&value=" + encodeURIComponent(e.value.trim()));
                }
            });

            <% If PM.IsRiskProject Then %>
            $("#tbDefaultSourceTitle").dxTextBox({ 
                value: _DEFAULT_SOURCE_TITLE,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=default_source_title&value=" + encodeURIComponent(e.value.trim()));
                }
            });
            $("#tbDefaultObjTitle").dxTextBox({ 
                value: _DEFAULT_OBJ_TITLE,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=default_obj_title&value=" + encodeURIComponent(e.value.trim()));
                }
            });
            <% Else %>
            $("#tbDefaultObjTitle").dxTextBox({ 
                value: _DEFAULT_OBJ_TITLE,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=default_obj_title&value=" + encodeURIComponent(e.value.trim()));
                }
            });
            <% End If %>

            $("#colorAlts").dxColorBox({ 
                value: _DEFAULT_COLOR_ALT,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=default_alt_color&value=" + e.value);
                }
            });

            <% If PM.IsRiskProject Then %>
            $("#colorSources").dxColorBox({ 
                value: _DEFAULT_COLOR_SOURCE,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=default_source_color&value=" + e.value);
                }
            });
            $("#colorObjs").dxColorBox({ 
                value: _DEFAULT_COLOR_OBJ,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=default_obj_color&value=" + e.value);
                }
            });
            <% Else %>
            $("#colorObjs").dxColorBox({ 
                value: _DEFAULT_COLOR_OBJ,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=default_obj_color&value=" + e.value);
                }
            });
            <% End If %>

            $("#btnSettingsClose").dxButton({
                text: "Close",
                icon: "fas fa-times",
                onClick: function() {
                    popupSettings.hide();
                }
            });

            $("#cbDrawingStays").dxSelectBox({ 
                width: "100%",
                dataSource: [{ "id" : 0, "text" : "15 seconds" }, { "id" : 1, "text" : "Forever" }],
                disabled: meetingOwner != cmdOwner,
                displayExpr: "text",
                searchEnabled: false,
                value: drawingLifeTime == 15000 ? 0 : 1,
                valueExpr: "id",
                onValueChanged: function (e) {
                    drawingLifeTime = e.value == 0 ? 15000 : <% = Integer.MaxValue%>;
                    csAction(<%=Command.Setting%>, "&name=drawing_stay&value=" + drawingLifeTime);
                }
            });            

            $("#cbColorCoding").dxCheckBox({ 
                value: isColorCoding,
                disabled: meetingOwner != cmdOwner,
                onValueChanged: function (e) {
                    csAction(<%=Command.Setting%>, "&name=color_coding&value=" + e.value);
                }
            });

            $("#cbSynchMode").dxSelectBox({
                acceptCustomValue: false,
                disabled: false,
                width: "100%",
                dataSource: [{ "id" : smSynchronous, "text" : "Synchronous" }, { "id" : smAsynchronous, "text" : "Asynchronous" }],
                displayExpr: "text",
                searchEnabled: false,
                value: meetingSynchMode,
                valueExpr: "id",
                onValueChanged: function(e) {
                    meetingSynchMode = e.value;
                    csAction(<%=Command.Setting%>, "&name=synch_mode&value=" + e.value, function () {
                        if (meetingSynchMode == smSynchronous) {
                            csAction(<%=Command.SetMeetingMode%>, "&tree_mode=" + meetingTreeMode + "&board_mode=" + meetingBoardMode);
                        }
                    });
                }
            });            
        }
        /* end Settings Dialog */

        /* Debug Window */
        var popupDebug = null, 
            debugPopupOptions = {
                animation: null,
                width: 450,
                height: 150,
                contentTemplate: function() {
                    return  $("<div style='padding:1ex'></div>").append(
                        $("<textarea id='tbDebugMessages' style='width: 100%; height: 100%; border: 0px; font-family: Courier New, monospace; font-size:8pt; line-height:1.2;' readonly></textarea>")
                    );
                },
                showTitle: true,
                title: "Debug Messages",
                dragEnabled: true,
                shading: false,
                closeOnOutsideClick: false,
                resizeEnabled: true,
                position: { my: 'right bottom', at: 'right bottom', of: window },
                showCloseButton: true
            };

        function debugDialog() {
            if (popupDebug) $(".popupDebug").remove();
            var $popupContainer = $("<div></div>").addClass("popupDebug").appendTo($("#popupDebug"));
            popupDebug = $popupContainer.dxPopup(debugPopupOptions).dxPopup("instance");
            popupDebug.show();

            $(".popupDebug").find(".dx-popup-content").css("padding", "2px").parent().parent().css("z-index", "1499");

            $("#tbDebugMessages").parent().css("height", "100%");
        }

        function debug(text) {
            if (isDebug) {
                var tb = document.getElementById("tbDebugMessages");
                if ((tb)) {
                    tb.value += new Date().toLocaleTimeString() + " " + text + "\n";
                    tb.scrollTop = tb.scrollHeight;
                }                
            }
        }
        /* end Debug Window*/

        /* Invitation Link Dialog */
        var popupLink = null, 
            popupLinkOptions = {
                animation: null,
                width: 600,
                height: 470,
                contentTemplate: function() {
                    return  $("<div></div>").append(
                        $("<p id='tbInfo' class='text10' align='justify' style='margin-top: 0px;'></p>"),
                        $("<span class='text10'><%=ResString("lblInvitationText")%>:</span>"),
                        $("<div id='tbInvitation'></div>"),
                        $("<br>"),
                        $("<span class='text10'><%=ResString("lblInvitationLink")%>:</span>"),
                        $("<div id='tbLink'></div>"),
                        $("<br>"),
                        $("<div id='btnInvitationClose' style='margin-top: 10px; margin-bottom: 10px; float: right;'></div>"),
                        $("<div id='btnProjectLinks' style='margin-top: 10px; margin-bottom: 10px; margin-right: 4px; float: right;'></div>"),
                        $("<div id='btnCopyLink' style='margin-top: 10px; margin-bottom: 10px; margin-right: 4px; float: right;'></div>"),
                        $("<div id='btnCopyInvitation' style='margin-top: 10px; margin-bottom: 10px; margin-right: 4px; float: right;'></div>")
                    );
                },
                showTitle: true,
                title: "<%=ResString("titleAntiguaInvite")%>",
                dragEnabled: true,
                closeOnOutsideClick: true
            };

        function showProjectLinkDialog() {
            getProjectLinkDialog(curPrjID, "<%=JS_SafeString(PRJ.ProjectName)%>", "<%=JS_SafeString(PRJ.Passcode)%>", "<%=clsMeetingID.AsString(PRJ.MeetingIDLikelihood)%>", "<%=clsMeetingID.AsString(PRJ.MeetingIDLikelihood, clsMeetingID.ecMeetingIDType.Antigua)%>", <% =If(App.HasActiveProject, "((curPrjData) && (curPrjData.isOnline))", "true") %>, "<% =JS_SafeString(ApplicationURL(False, False)) %>", "", "<% =JS_SafeString(ApplicationURL(False, False)) %>");
        }

        function showInvitationDialog() {
            $("#divOnlineUsers").hide();

            if (popupLink) $(".popupLink").remove();
            var $popupContainer = $("<div></div>").addClass("popupLink").appendTo($("#popupSettings"));
            popupLink = $popupContainer.dxPopup(popupLinkOptions).dxPopup("instance");
            popupLink.show();

            $("#tbInfo").html("<%=JS_SafeString(ResString("antiguaInvitationText"))%>");
            $("#tbInfo").parent().parent().parent().parent().css("z-index", "1499");

            $("#tbInvitation").dxTextArea({
                height: 170,
                value: "<% =JS_SafeString(InvitationText)%>",
                readOnly: true
            });

            $("#tbLink").dxTextBox({
                value: "<%=JS_SafeString(MeetingURL)%>",
                readOnly: true
            });

            $("#btnCopyLink").dxButton({
                text: "Copy Link",
                icon: "far fa-copy",
                onClick: function() {
                    if ($("#tbLink").data("dxTextBox")) {
                        var tbLink = $("#tbLink").dxTextBox("instance");
                        copyDataToClipboard(tbLink.option("value"));
                    }
                }
            });

            $("#btnCopyInvitation").dxButton({
                text: "Copy Invitation",
                icon: "fas fa-envelope-open-text",
                onClick: function() {
                    if ($("#tbInvitation").data("dxTextArea")) {
                        var tbInvitation = $("#tbInvitation").dxTextArea("instance");
                        copyDataToClipboard(tbInvitation.option("value"));
                    }
                }
            });

            $("#btnProjectLinks").dxButton({
                icon: "fas fa-link", text: "<% =JS_SafeString(ResString("lblGetLink")) %>",
                disabled: false,
                elementAttr: {id: 'btn_link_in_dlg'},
                onClick: function (e) {
                    showProjectLinkDialog();
                }
            });

            $("#btnInvitationClose").dxButton({
                text: "Close",
                icon: "fas fa-times",
                onClick: function() {
                    popupLink.hide();
                }
            });
        }

        function showManageParticipantsWindow() {
            onShowIFrame("<% = JS_SafeNumber(PageURL(_PGID_PROJECT_USERS, "temptheme=sl&is_widget=1"))%>", resString("btnManageModelParticipants"), undefined, undefined, {}, [{
                widget: 'dxButton',
                options: {
                    text: resString("btnClose"),
                    onClick: function () {
                        closePopup();
                        //repaintUserList(); //todo
                        return false;
                    }
                },
                toolbar: "bottom",
                location: 'center'
            }]);
        }
        /* end Invitation Link Dialog */

        /* User List */
        var usersList = <% = GetUsersList() %>;

        function hashByString(s) {
            var hash = 0, i, chr;
            for (i = 0; i < s.length; i++) {
                chr = s.charCodeAt(i);
                hash += chr;
            }
            return hash;
        }

        function getUserColorByName(name) {
            //var palette = DistinctColorPalette;
            //return palette[hashByString(name) % palette.length];
            var stringToColour = function(str) {
                var hash = 0;
                for (var i = 0; i < str.length; i++) {
                    hash = str.charCodeAt(i) + ((hash << 5) - hash);
                }
                hash*=100;
                var colour = '#';
                for (var i = 0; i < 3; i++) {
                    var value = (hash >> (i * 8)) & 0xFF;
                    colour += ('00' + value.toString(16)).substr(-2);
                }
                return colour;
            }
            return stringToColour(name);
        }

        function highlightDraggableItemsByUser(author, is_highlight) {
            // highlight/unhighlight all draggable items from this user
            $("#divWhiteboard").find(".draggable-item[data-author=\"" + author + "\"]").css({ "border" : is_highlight ? "2px solid " + getUserColorByName(author) : "" });
        }

        var is_man = false;

        function initUserList() {
            usersList.sort(sortUsersByNameFunc);
        
            $("#divUserList").dxList({
                dataSource: usersList,
                focusStateEnabled: false,
                hoverStateEnabled: false,                
                itemTemplate: function(data, _, element) {
                    data.color = getUserColorByName(data.name + " (" + data.email + ")");
                    element.on("mouseenter", function () {
                        // highlight all draggable items from this user
                        highlightDraggableItemsByUser(data.name + " (" + data.email + ")", true);
                    });
                    element.on("mouseout", function () {
                        // do not fire if element's child
                        var e = event.toElement || event.relatedTarget;
                        if (e.parentNode == this || e == this) {
                            return;
                        }
                        // unhighlight all draggable items from this user
                        highlightDraggableItemsByUser(data.name + " (" + data.email + ")", false);
                    });
                    element.append(
                        $("<i></i>").addClass("fas fa-user-circle").css({ "color": data.color, "margin-right" : "8px" }),
                        $("<span id='ul" + data.id + "'>").text(data.name == "" ? data.email : data.name + " (" + data.email + ")") <%--,
                        $("<div></div>").css({ "float" : "right" }).dxCheckBox({
                            disabled: isAnonymous || isReadOnly || !data.can_change_pm,
                            visible: !isAnonymous,
                            value: data.is_pm,
                            text: data.is_pm ? "<% = ResString("lblProjectOwner")%>" : "",
                            onValueChanged: function (e) {
                                if (is_man) return;
                                var f = function () {
                                    csAction(<%=Command.Setting%>, "&name=make_pm&email=" + data.email + "&value=" + e.value, isAnonymous, function (response) {
                                        var res = parseReply(response);
                                        if (isValidReply(res) && res.Result == _res_Success) {
                                            data.is_pm = e.value;
                                            data.can_change_pm = res.Tag;
                                            e.component.option("disabled", !data.can_change_pm);
                                            csAction(<%=Command.Setting%>, "&name=save_user_list&value=" + encodeURIComponent(JSON.stringify(usersList)));
                                            DevExpress.ui.notify(resString("msgSaved"), "success");
                                        } else {
                                            is_man = true;
                                            e.component.option("isValid", false);
                                            e.component.option("value", e.previousValue);
                                            is_man = false;
                                        }
                                    });
                                }
                                if (!data.can_be_pm && e.value) {
                                    dxConfirm("<%=ResString("msgAskSetUserPM")%>", f, function() {
                                        is_man = true;
                                        e.component.option("value", e.previousValue);
                                        is_man = false;
                                    });
                                } else {
                                    f();
                                }
                                
                            }
                        }) --%>
                    )
                },
                searchEnabled: false
            });        

            $("#btnManageParticipants").dxButton({
                visible: !isAnonymous && (meetingOwner == cmdOwner),
                icon: "fas fa-users",
                text: "<% = ParseString("%%Model%% Participants…") %>",
                onClick: function () {
                    showManageParticipantsWindow();
                }
            });
        }

        function repaintUserList() {
            usersList.sort(sortUsersByNameFunc);

            if (!$("#divUserList").data("dxList")) return false;
            var list = $("#divUserList").dxList("instance");
            list.option("dataSource", usersList);
            list.repaint();
            if ($("#btnManageParticipants").data("dxButton")) {
                $("#btnManageParticipants").dxButton("instance").option({visible: !isAnonymous && (meetingOwner == cmdOwner)});
            }
        }

        function toggleOnlineParticipantsView() {
            return $("#divOnlineUsers").toggle().is(":visible"); 
        }

        function highlightUser(id, isConnected, is_delete) {
            var lbl = document.getElementById("ul" + id);
            if ((lbl)) {
                lbl.className = isConnected ? "user_connected" : "user_disconnected";
        
                if (isConnected) {
                    setTimeout(function () { lbl.className = ""; }, OPT_USER_LIST_HIGHLIGHT_TIMOUT);
                } else {
                    setTimeout(function () { repaintUserList(); }, OPT_USER_LIST_HIGHLIGHT_TIMOUT);
                }
            }

            var total = document.getElementById("lblOnlineUsers");
            var cnt = usersList.length - ((is_delete) ? 1 : 0);
            total.innerHTML = cnt;
            total.title = cnt + " participant(s)";

            total.className = cnt <= 1 ? "cs-toolbar-online cs-toolbar-single-mode-color" : "cs-toolbar-online";

            if (meetingState == msActive) {
                var usr = getUserById(id);
                if ((usr)) {
                    DevExpress.ui.notify((usr.name.trim() != "" ? usr.name : usr.email)+ (isConnected ? " connected" : " disconnected"), isConnected ? "info" : "warning", OPT_USER_LIST_HIGHLIGHT_TIMOUT);
                }
            }
        }

        function getUserById(id) {
            for (var i = 0; i < usersList.length; i++) {
                if (usersList[i]["id"] == id) return usersList[i];
            }
            return false;
        }

        function deleteUser(id) {
            var idx = -1;
            for (var i = 0; i < usersList.length; i++) {
                if (usersList[i]["id"] == id) {
                    idx = i;
                    break;
                }
            }
            if (idx >= 0) usersList.splice(idx, 1);
            return false;
        }

        function sortUsersByNameFunc(a, b){
            var aName = a.name.toLowerCase();
            var bName = b.name.toLowerCase(); 
            return ((aName < bName) ? -1 : ((aName > bName) ? 1 : 0));
        }
    
        function getUserIndexByName(name) {
            for (var i = 0; i < usersList.length; i++) {
                if (usersList[i]["name"] == name) return i;
            }
            return -1;
        }
        /* end User List */

        /* Messages */
        var messagesList = [];

        function initMessages() {       
            $("#divMessagesList").dxList({
                dataSource: messagesList,
                focusStateEnabled: false,
                hoverStateEnabled: false,
                bounceEnabled: false,
                itemTemplate: function(data, _, element) {
                    element.append(
                        $("<div style='white-space:normal'>").append(
                            $("<span style='white-space:normal'>").text(data[0]), $("<span style='white-space:normal'>").text(": " + data[2]), $("<div style='white-space:normal' class='chat_timestamp'>").text(OPT_CHAT_MESSAGE_SHOW_TIMESTAMP ? " " + data[1] : "")
                        )
                    )
                },
                searchEnabled: false
            });
            
            $("#divNewMessage").dxTextBox({
                placeholder: "Type a message…",
                stylingMode: "filled",
                onEnterKey: function (e) {
                    sendBroadcastMessage();
                }
            });

            $("#divSendMessage").dxButton({
                text: "",
                width: 30,
                icon: "fas fa-bullhorn",
                onClick: function(e) {
                    sendBroadcastMessage();
                }
            });
        }

        function postBroadcastMessage(username, ts, msg) { // name, timestamp, message
            messagesList.push([username, ts, msg]);
            if ($("#divMessagesList").data("dxList")) {
                var list = $("#divMessagesList").dxList("instance");
                list.reload();
                list.scrollToItem(messagesList.length - 1);
            }
            DevExpress.ui.notify(username + " " + ts + ": " + msg, "info", OPT_CHAT_MESSAGE_HIGHLIGHT_TIMOUT);
        }

        function sendBroadcastMessage() {
            if ($("#divNewMessage").data("dxTextBox")) {
                var tbNewMessage = $("#divNewMessage").dxTextBox("instance");
                var msg = ShortString(tbNewMessage.option("text").trim(), OPT_CHAT_MESSAGE_MAX_SIZE, false);
                if (msg != "") {                                        
                    tbNewMessage.reset();
                    csAction(<%=Command.ChatMessage%>, "&username=" + encodeURIComponent(UserName) + "&ts=" + encodeURIComponent(getTimestamp()) + "&msg=" + encodeURIComponent(msg));
                }
            }
        }

        function getTimestamp() {
            var now = new Date(); 
            var h = now.getHours();
            var m = now.getMinutes();
            var a = h < 12 ? "AM" : "PM";
            if (h >= 12) h -= 12;
            if (h.toString().length == 1) h = '0' + h;
            if (m.toString().length == 1) m = '0' + m;
            return h + ':' + m + " " + a;
        }
        /* end Messages */
   
        /* TreeView */
        function initTree() {
            var nodes = meetingTreeMode == mmAlts ? alternatives : objectives();
            var nodes_len = nodes.length;

            var h = document.getElementById("divTree").clientHeight;        
            var w = document.getElementById("divTree").clientWidth;        

            var ulTree = document.getElementById("ulTree");
        
            $(".draggable-list-item").remove();
            
            for (var i = 0; i < nodes_len; i++) {
                if (meetingTreeMode == mmAlts) {
                    draggableItemCreateOnList(nodes[i], ulTree, true, true, "draggable-list-item", false, false, 0);
                } else {
                    draggableItemCreateOnList(nodes[i], ulTree, i > 0 && nodes[i]["isterminal"], i > 0, "draggable-list-item", false, false, nodes[i]["level"]);
                }
            }

            $("#btnAddToList").prop("title", meetingTreeMode == mmAlts ? "<%=ResString("btnAddAltsCS")%>" : "<%=ResString("btnAddObjsCS")%>");
        }

        function initTreeContextMenu() {
            if ($("#divTreeMenu").data("dxContextMenu")) {
                $("#divTreeMenu").dxContextMenu("instance").dispose();
            }

            $("#divTreeMenu").dxContextMenu({
                items: [
                    { text: meetingTreeMode == mmAlts ? "<%=ResString("btnAddAltsCS")%>" : "<%=ResString("btnAddObjsCS")%>", icon: "fas fa-plus", onClick: function () { onAddNewItemToList(); }, disabled: false },
                    <% If App.isAuthorized Then %>
                    { text: "Clear All", icon: "fas fa-trash-alt", onClick: function () { btnClearListClick(); }, disabled: false }
                    <% End If %>
                ],
                onShowing: function (e) { if (!canDoAction()) e.cancel = true; },
                target: "#ulTreeContainer"
            });
        }

        function initTreeSplitter() {
            $("#divTreeSplitter").dxResizable({
                handles: 'right',
                width: splitterWidth, //"50%",
                minWidth: 200,
                maxWidth: "100%",
                onResize: function (e) {
                    resizePage();
                },
                onResizeEnd: function (e) {
                    splitterWidth = e.width;
                    localStorage.setItem("CS_SplitterWidth<%=App.Antigua_MeetingID%>", splitterWidth);
                }
            }).find(".dx-resizable-handle").addClass("splitter_v");
        }

        function getNodesGuidsListToDelete(mode) {
            var retVal = "";
            switch (mode) {
                case "all_alts":
                case "all_objs":
                    for (var i = 0; i < board_nodes.length; i++) {
                        if (mode == "all_alts" && board_nodes[i]["isalt"] || mode == "all_objs" && !board_nodes[i]["isalt"] && board_nodes[i]["location"] == location_hid) { retVal += (retVal == "" ? "" : ",") + board_nodes[i]["nodeguid"]; }
                    }
                    break;
                case "all_alts_list":
                    for (var i = 0; i < alternatives.length; i++) {
                        retVal += (retVal == "" ? "" : ",") + alternatives[i]["nodeguid"];
                    }
                    break;
                case "all_objs_tree":
                    for (var i = 1; i < objectives().length; i++) {
                        retVal += (retVal == "" ? "" : ",") + objectives()[i]["nodeguid"];
                    }
                    break;
            }
            return retVal;
        }

        function btnClearListClick() {
            if (!canDoAction()) return;
            dxConfirm(resString("msgConfirmDeleteAllItems"), function () {
                var params = meetingTreeMode == mmAlts ? "&guids=" + getNodesGuidsListToDelete("all_alts_list") : "&guids=" + getNodesGuidsListToDelete("all_objs_tree");
                csAction(<%=Command.DeleteNodes%>, params + "&whiteboard_items=false");
                clearList();
            });
        }

        function clearList() {
            if (meetingTreeMode == mmAlts) {
                alternatives = [];
                
                $("#ulTree").empty();
            } else {
                objectives().splice(1, objectives().length - 1); // deleting all objectives except for the goal
                
                $("#ulTree").empty();
                initTree();
            }
        }

        function getNodeByGuid(guid) {
            var list = alternatives;
            var list_len = list.length;
            for (var j = 0; j < list_len; j++) {
                if (list[j].nodeguid == guid) {
                    return list[j];
                    break;
                }
            }
            list = sources;
            list_len = list.length;
            for (var j = 0; j < list_len; j++) {
                if (list[j].nodeguid == guid) {
                    return list[j];
                    break;
                }
            }
            list = impacts;
            list_len = list.length;
            for (var j = 0; j < list_len; j++) {
                if (list[j].nodeguid == guid) {
                    return list[j];
                    break;
                }
            }
            return;
        }

        function getNodeChildren(list, guid) {
            var retVal = [];            
            var list_len = list.length;
            for (var j = 0; j < list_len; j++) {
                if (list[j].parentguids.indexOf(guid) != -1) {
                    retVal.push(list[j]);
                }
            }
            return retVal;
        }

        function getNodeIndex(list, guid) {
            var retVal = -1;
            var list_len = list.length;
            for (var j = 0; j < list_len; j++) {
                retVal += 1;
                if (list[j].nodeguid == guid) {
                    return retVal;
                    break;
                }
            }
            return retVal;
        }

        function getTreeElementByGuid(guid) {
            var div = $("#ulTree").find("[data-guid='" + guid + "']");
            if ((div) && div.length > 0) {
                return div[0];
            } else {
                return null;
            }
        }

        function deleteTreeElementByGuid(guid) {
            var ulTree = document.getElementById("ulTree");
            var tree_element = getTreeElementByGuid(guid);                                                                                    
            if ((tree_element)) {                                            
                ulTree.removeChild(tree_element);
            }
        }

        function insertElementAt(parent, source_element, target_idx) {
            if (target_idx < 0) target_idx = 0;
            if (target_idx >= parent.childNodes.length) {
                parent.appendChild(source_element);
            } else {
                parent.insertBefore(source_element, parent.childNodes[target_idx]);
            }
        }

        function treeItemInitContextMenu(div, node, can_delete) {
            var menu_div = document.createElement("div");
            div.appendChild(menu_div);

            $(menu_div).dxContextMenu({
                items: [
                    { text: meetingTreeMode == mmAlts ? "<%=ResString("btnAddAltBelowCS")%>" : "<%=ResString("btnAddObjsCS")%>", icon: "far fa-plus-square", disabled: isReadOnly,
                        onClick: function(e) {
                            onAddNewItemToList(node);
                        }
                    },
                    { text: "Edit", icon: "fas fa-pencil-alt", disabled: isReadOnly,
                        onClick: function(e) {
                            treeItemEdit(node);
                        }
                    },
                    { text: "Delete", icon: "far fa-trash-alt", disabled: !can_delete || isReadOnly,
                        onClick: function(e) {
                            if (!canDoAction()) return;                            
                            dxConfirm(replaceString("{0}", node.name, resString("msgSureDeleteCommon")), function () {
                                var params = "&guids=" + node.nodeguid;
                                deleteWhiteboardNode(node.nodeguid);
                                csAction(<%=Command.DeleteNodes%>, params + "&whiteboard_items=false");
                            });                            
                        }    
                    },
                    { text: "Edit Information Document", icon: "far fa-file-alt", 
                        onClick: function(e) {
                            editInfodoc(node.nodeguid, meetingTreeMode == mmAlts ? <% = Cint(reObjectType.Alternative) %> : <% = Cint(reObjectType.Node) %>);
                        }
                    }, { text: "<%=ParseString("%%Objectives%%")%>", icon: "icon ec-hierarchy", visible: meetingTreeMode == mmAlts && !isAnonymous, 
                        onClick: function(e) {
                            var params = "&guid=" + node.nodeguid;
                            if (canDoAction()) callAjax("<%=_PARAM_ACTION %>=get_alt_contributions" + params, onGetAltsContributionsCallback, ajaxMethod, false, "", true, please_wait_delay);
                        }
                    }, { text: "<%=ParseString("%%Alternatives%%")%>", icon: "icon ec-alts", visible: node.isterminal && (meetingTreeMode == mmSources || meetingTreeMode == mmImpacts) && !isAnonymous, 
                        onClick: function(e) {
                            var params = "&guid=" + node.nodeguid;
                            if (canDoAction()) callAjax("<%=_PARAM_ACTION %>=get_obj_contributions" + params, onGetAltsContributionsCallback, ajaxMethod, false, "", true, please_wait_delay);
                        }
                    }
                ],
                onShowing: function (e) { 
                    if (!canDoAction()) e.cancel = true; 
                    $(".custom_tooltip").tooltip().hide();  // D6557
                },
                target: div
            });
        }

        function treeItemEdit(node) {
            if (!canDoAction()) return;

            if ($("#tbNodeName").data("dxTextBox")) {
                $("#tbNodeName").dxTextBox("instance").dispose();
            }
            $("#tbNodeName").remove();
            dxDialog("<div id='tbNodeName' style='width: 400px;'></div>", function () { onSaveNodeName(node.nodeguid, node.name); }, ";", "Enter new name:");
            $("#tbNodeName").dxTextBox({
                value: node.name,
                placeholder: "Type new name here..."
            });

            dxTextBoxFocus('tbNodeName');
        }

        function onSaveNodeName(guid, old_name) {
            var text = "";
            if ($("#tbNodeName").data("dxTextBox")) {
                text = $("#tbNodeName").dxTextBox("instance").option("value").trim();
            }
            if (text != "" && text != old_name) {
                var params = "&guid=" + guid + "&name=" + encodeURIComponent(text);
                csAction(<%=Command.RenameItem%>, params);
            }
        }

        var addNewObjectiveBelowNode;

        function onAddNewItemToList(parentNode) {
            addNewObjectiveBelowNode = parentNode;

            if (!canDoAction()) return;
            var title = meetingTreeMode == mmAlts ? "<%=ResString("btnAddAltsCS")%>" : "<%=ResString("btnAddObjsCS")%>";
            if (isRisk) {
                if (meetingTreeMode == mmSources) title = "<%=ResString("btnAddObjsCSL")%>";
                if (meetingTreeMode == mmImpacts) title = "<%=ResString("btnAddObjsCSI")%>";
            }
            $("#btnAddToList").prop("title", title);
            if ($("#tbNodeName").data("dxTextBox")) {
                $("#tbNodeName").dxTextBox("instance").dispose();
            }
            $("#tbNodeName").remove();
            dxDialog("<div id='tbNodeName' style='width: 400px;'></div>", function () { onCreateNodeToList(); }, ";", title);
            $("#tbNodeName").dxTextBox({
                value: "",
                placeholder: "Type new name here..."
            });

            dxTextBoxFocus('tbNodeName');
        }
        
        function onCreateNodeToList() {
            var text = "";
            if ($("#tbNodeName").data("dxTextBox")) {
                text = $("#tbNodeName").dxTextBox("instance").option("value").trim();
            }
            if (text != "") {
                var params = "&name=" + encodeURIComponent(text) + "&is_alt=" + (meetingTreeMode == mmAlts) + "&parent_node_guid=" + (typeof addNewObjectiveBelowNode !== "undefined" && addNewObjectiveBelowNode != null && addNewObjectiveBelowNode != "" ? addNewObjectiveBelowNode.nodeguid : "");
                csAction(<%=Command.AddNode%>, params);
                addNewObjectiveBelowNode = null;
            }
        }
        /* end TreeView */                

        /* Information Documents, Infodocs */
        var tmp_cur_infodoc_guid;
        function editInfodoc(guid, type) {
            if (isReadOnly) return;
            tmp_cur_infodoc_guid = guid;
            if (guid != "") {
                OpenRichEditor('?project=<%=PRJ.ID%>&type=' + type + '&field=infodoc&guid=' + guid + ((type*1)==1 || (type*1)==2 ? "&reset=1" : ""));
            }
        }

        function onRichEditorRefresh(empty, infodoc, callback_param) {
            if (tmp_cur_infodoc_guid == sources[0].nodeguid) {
                sources[0].hasinfodoc = !(empty == 1 || empty == true);
                updateGoalInfodocIcon();
            }
            var el = $(".draggable-item-infodoc-icon[data-guid='" + tmp_cur_infodoc_guid + "']");
            if (el.length) {
                var node = getWhiteboardNodeByGuid(tmp_cur_infodoc_guid);
                if ((node)) node.hasinfodoc = !(empty == 1 || empty == true);
                el.attr("class", "fas fa-info-circle draggable-item-infodoc-icon " + (empty == 1 || empty == true ? "toolbar-label-disabled" : "ec-icon"));
            }
            //window.focus();
            //var f = document.getElementById("frmInfodoc");
            //if ((f) && (theForm.selInfodocIDs)) {
            //    var node = getNodeByGUID(theForm.selInfodocIDs.value);
            //    if ((node)) LoadInfodoc(node[IDX_ID], node[IDX_GUID], node[IDX_IS_ALT] == 1);
            //    var alt = getAlternativeById(theForm.selInfodocIDs.value);
            //    if ((alt)) LoadInfodoc(alt["iid"], alt["guid"], true);
            //    $("#imgTreeInfodoc" + replaceString("-", "", theForm.selInfodocIDs.value)).removeClass("toolbar-label").removeClass("toolbar-label-disabled").addClass(empty == 1 || empty == true ? "toolbar-label-disabled" : "toolbar-label");

            //}
        }
        /* end Infodocs */

        /* Draggable elements */    
        var isDragging = false;
        var x0 = 0, y0 = 0;

        function initWhiteboardDrop() {
            var board = document.getElementById("divWhiteboard");

            $("#divWhiteboard").droppable({
                over: function( event, ui ) {
                    if (ui.draggable[0].className.indexOf("draggable-list-item") != -1 && meetingTreeMode !== meetingBoardMode) {
                        //show the ban icon
                        $(".cs-whiteboard-ban-sign-parent").show();
                    } else {
                        //hide the ban icon
                        $(".cs-whiteboard-ban-sign-parent").hide();
                    }
                },
                out: function( event, ui ) {
                    //hide the ban icon
                    $(".cs-whiteboard-ban-sign-parent").hide();
                },
                drop: function(e, ui) {
                    //hide the ban icon
                    $(".cs-whiteboard-ban-sign-parent").hide();

                    e.stopPropagation();
                    isDragging = false;
                    if (!canDoAction()) return;

                    var brd = e.target.getBoundingClientRect(); // whiteboard
                    var x1 = ui.position.left - brd.left;
                    var y1 = ui.position.top - brd.top;

                    var box_guid = ui.draggable[0].getAttribute("data-guid");
                    var is_list_item = ui.draggable[0].className.indexOf("draggable-list-item") != -1;
                    if (getWhiteboardElementByGuid(box_guid) !== null) {
                        // move item on the whiteboard
                        if (!is_list_item && (box_guid) && (Math.abs(x0 - x1) >= 2 || Math.abs(y0 - y1) >= 2)) {
                            debug("Item moved on whiteboard!");
                            var div = ui.draggable[0];
                            performMoveNodeOnBoard(div, x1, y1);
                            div.style.zIndex = lastZIndex;
                            lastZIndex += 1;
                            csAction(<% = Command.MoveNodesOnBoard %>, "&guid=" + box_guid + "&x=" + x1 + "&y=" + y1);
                        }
                        if (is_list_item) {
                            if (meetingBoardMode == mmAlts) DevExpress.ui.notify(resString("msgAlternativeAlreadyExistsOnBoard"));
                        }
                    } else {
                        // drag from list to the whiteboard
                        if (is_list_item && meetingTreeMode == meetingBoardMode) {
                            var node_guid = ui.draggable[0].getAttribute("data-guid");
                            if ((node_guid)) {
                                var movenode = getNodeByGuid(node_guid);
                                if ((movenode)) {
                                    csAction(movenode.isalt ? <%=Command.CopyAltToBoard%> : <%=Command.MoveFromTreeToBoard%>, "&guid=" + node_guid + "&x=" + (x1) + "&y=" + (y1) + "&w=" + _DEFAULT_ITEM_WIDTH + "&h=" + _DEFAULT_ITEM_HEIGHT);
                                }
                            }
                        }
                    }
                }
            });
        }

        function performMoveNodeOnBoard(div, x, y) {
            div.setAttribute("data-x", x);
            div.setAttribute("data-y", y);
            div.style.left = x + "px";
            div.style.top = y + "px";
        }

        function checkProsConsDrop(isListItem) {
            if (!confirm_shown && !isRisk && (!isAnonymous || meetingSynchMode == smAsynchronous) && isListItem && !isProsCons()) {                                
                confirm_shown = true;
                dxConfirm(resString("msgProsConsDragNotAllowed"), function () {
                    meetingTreeMode = mmSources;
                    if (!isAnonymous && meetingSynchMode == smSynchronous) {
                        csAction(<%=Command.SetMeetingMode%>, "&tree_mode=" + meetingTreeMode + "&board_mode=" + meetingBoardMode);
                    } else {
                        setMeetingMode();
                    }
                    confirm_shown = false;
                }, function (e) { confirm_shown = false; } );
                return false; //cancel drag
            }
        }

        function initTreeDrop() {
            var ulTree = document.getElementById("ulTree");

            $("#ulTree").droppable({
                classes: {
                    "ui-droppable": "",
                    "ui-droppable-hover": "",
                    "ui-droppable-active": "highlight-list-drop"
                },
                //accept: meetingMode == mmProsCons ? ".draggable-item, .draggable-list-item-small" : ".draggable-item",
                accept: ".draggable-item, .draggable-list-item-small",
                greedy: true,
                over: function( event, ui ) {
                    if ((ui.draggable[0].className.indexOf("draggable-list-item-small") != -1 && meetingTreeMode !== mmSources) || (ui.draggable[0].className.indexOf("draggable-item") != -1 && meetingTreeMode !== meetingBoardMode)) {
                        //show the ban icon
                        if ($(".cs-tree-ban-sign-parent").is(":hidden")) {
                            $(".cs-tree-ban-sign-parent").show(0);
                            checkProsConsDrop(ui.draggable[0].className.indexOf("draggable-list-item-small") != -1);
                        }
                    }
                    //else {
                    //    //hide the ban icon
                    //    $(".cs-tree-ban-sign-parent").hide();
                    //}
                },
                out: function( event, ui ) {
                    //hide the ban icon
                    $(".cs-tree-ban-sign-parent").hide();
                },
                drop: function(e, ui) {
                    //hide the ban icon
                    $(".cs-tree-ban-sign-parent").hide();

                    e.stopPropagation();
                    if (!canDoAction()) return;
                    var source_guid = ui.draggable[0].getAttribute("data-guid");
                    debug("Dropped id=" + source_guid);
                    if (ui.draggable[0].className.indexOf("draggable-list-item-small") != -1) { // dropped a pro or con item to the hierarchy
                        if (isProsCons()) {
                            var alt_guid = ui.draggable[0].getAttribute("alt-guid");
                            dropProsConsToTree(alt_guid, source_guid, "", -1);
                        } else {
                            if (!confirm_shown) DevExpress.ui.notify("Not allowed", "warning", OPT_CHAT_MESSAGE_HIGHLIGHT_TIMOUT);
                        }
                    } else {
                        if ((source_guid)) {
                            if (getWhiteboardElementByGuid(source_guid) !== null) {
                                if (meetingTreeMode == meetingBoardMode) {
                                    if (meetingTreeMode == mmAlts) {
                                        if ((getNodeByGuid(source_guid))) {
                                            DevExpress.ui.notify(resString("msgAlternativeAlreadyExistsInList"));
                                        } else {
                                            // copy an alternative from whiteboard to the alternatives list
                                            csAction(<%=Command.MoveAltToList%>, "&pos=-1&source_guid=" + source_guid);
                                        }
                                    }
                                    if (meetingBoardMode == mmSources || meetingBoardMode == mmImpacts) {
                                        var node = getWhiteboardNodeByGuid(source_guid);
                                        if (!(getNodeByGuid(source_guid)) && (node) && (!node["isalt"])) {
                                            // move an objective from whiteboard to the hierarchy (as the Goal node last child)
                                            csAction(<%=Command.MoveFromBoardToTree%>, "&drop_action=-1&source_guid=" + source_guid);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        function initListItemDrop(treeItem, can_drop_inside) {
            treeItem.droppable({
                classes: {
                    "ui-droppable": "",
                    "ui-droppable-hover": "highlight-list-item-hover",
                    "ui-droppable-active": "highlight-list-item-drop"
                },
                //accept: meetingMode == mmProsCons ? ".draggable-item, .draggable-list-item, .draggable-list-item-small" : ".draggable-item, .draggable-list-item",
                accept: ".draggable-item, .draggable-list-item, .draggable-list-item-small",
                greedy: true,
                over: function(e, ui) {
                    //if ((ui.draggable[0].className.indexOf("draggable-list-item-small") != -1 && !isProsCons()) || (ui.draggable[0].className.indexOf("draggable-item") != -1 && meetingTreeMode !== meetingBoardMode)) {
                    if (ui.draggable[0].className.indexOf("draggable-item") !== -1 && meetingTreeMode !== meetingBoardMode) {
                        //show the ban icon
                        $(".cs-tree-ban-sign-parent").show(0);
                    } else {
                        ////hide the ban icon
                        //$(".cs-tree-ban-sign-parent").hide();
                        if (ui.draggable[0].className.indexOf("draggable-list-item-small") != -1 && meetingTreeMode !== mmSources) {
                            checkProsConsDrop(true);
                        } else {
                            var isDropTargetAlternative = this.getAttribute("data-isalternative") == "1";
                            ui.draggable.data("current-droppable", $(this)); // remember current drop target
                            dropIndicator($(e.target), ui.draggable, !isDropTargetAlternative);
                        }
                    }
                },
                out: function( event, ui ) {
                    //hide the ban icon
                    $(".cs-tree-ban-sign-parent").hide();
                },
                drop: function(e, ui) {
                    //hide the ban icon
                    $(".cs-tree-ban-sign-parent").hide();

                    ui.draggable.removeData("current-droppable");
                    dropIndicatorClear();
                    e.stopPropagation();
                    debug("Dropped on item!");
                    if (!canDoAction()) return;
                    var source_guid = ui.draggable[0].getAttribute("data-guid");
                    var target_guid = this.getAttribute("data-guid");
                    debug("Dropped on tree view node id=" + source_guid);
                    if (ui.draggable[0].className.indexOf("draggable-list-item-small") != -1) { // dropped a pro or con item on the hierarchy node                        
                        if (isProsCons()) {
                            var alt_guid = ui.draggable[0].getAttribute("alt-guid");
                            dropProsConsToTree(alt_guid, source_guid, target_guid, lastDropPosition);
                        } else {
                            if (!confirm_shown) DevExpress.ui.notify("Not allowed", "warning", OPT_CHAT_MESSAGE_HIGHLIGHT_TIMOUT);
                        }
                    } else {
                        if ((source_guid)) {
                            var is_list_item = ui.draggable[0].className.indexOf("draggable-list-item") != -1;
                            if (is_list_item && (getNodeByGuid(source_guid))) {                                                        
                                var params = "&source_guid=" + source_guid + "&target_guid=" + target_guid + "&pos=" + lastDropPosition;
                                csAction(meetingTreeMode == mmAlts ? <%=Command.ReorderInAlts%> : <%=Command.ReorderInTree%>, params);
                            }
                            var node = getWhiteboardNodeByGuid(source_guid);
                            if (!is_list_item && (node)) {
                                if (alternatives.filter(function(item) {item.guid == source_guid}).length > 0) {
                                    if (meetingTreeMode == mmAlts) DevExpress.ui.notify(resString("msgAlternativeAlreadyExistsInList"));
                                } else {
                                    if ((node["isalt"] && meetingTreeMode == mmAlts) || (!node["isalt"] && (meetingTreeMode == mmSources || meetingTreeMode == mmImpacts))) {
                                        var index = getNodeIndex(node["isalt"] ? alternatives : objectives(), target_guid);
                                        if (lastDropPosition == nmaAfterNode) index += 1;
                                        var params = "&source_guid=" + source_guid + "&target_guid=" + target_guid + "&pos=" + index + "&drop_action=" + lastDropPosition;
                                        csAction(meetingTreeMode == mmAlts ? <%=Command.MoveAltToList%> : <%=Command.MoveFromBoardToTree%>, params);
                                    }
                                }
                            }
                        }
                    }
                },
                out: function (e, ui) {
                    ui.draggable.removeData("current-droppable");
                    $(e.target).removeClass("highlight-list-item-hover-drop-before highlight-list-item-hover-drop-after");
                }
            });
        }

        function dropProsConsToTree(alt_guid, source_guid, target_guid, pos) {
            var old_name = getProsConsName(source_guid);
            if ($("#tbNodeName").data("dxTextBox")) {
                $("#tbNodeName").dxTextBox("instance").dispose();
            }
            $("#tbNodeName").remove();
            dxDialog("<div id='tbNodeName' style='width: 400px;'></div>", function () {
                var text = $("#tbNodeName").dxTextBox("instance").option("value").trim();
                if (text != "") {
                    var params = "&alt_guid=" + alt_guid + "&source_guid=" + source_guid + "&target_guid=" + target_guid + "&pos=" + pos + "&name=" + encodeURIComponent(text);
                    csAction(<%=Command.CopyFromProsConsToTree%>, params);
                }
            }, function () {}, resString("lblEnterObjName"));
            
            $("#tbNodeName").dxTextBox({
                value: old_name,
                placeholder: "Type new name here..."
            });

            dxTextBoxFocus('tbNodeName');
        }

        function dropIndicatorClear() {
            $("#ulTree").find(".draggable-list-item").removeClass("highlight-list-item-hover-drop-before highlight-list-item-hover-drop-into highlight-list-item-hover-drop-after");
        }

        var nmaBeforeNode = 1, nmaAsChildOfNode = 2, nmaAfterNode = 4;
        var lastDropPosition;

        function dropIndicator(dropTarget, dragSource, can_drop_inside) {
            lastDropPosition = nmaBeforeNode;

            var mouseLeft = (window.event.pageX - dropTarget.offset().left) + dropTarget.parent().scrollLeft();
            var mouseTop = (window.event.pageY - dropTarget.offset().top) + dropTarget.parent().scrollTop();

            var itemHeight = dropTarget[0].clientHeight;

            dropIndicatorClear();

            //debug("Item H:" + itemHeight + ", top:" + ui.position.top + ", offset top: " + ui.offset.top + ", mouse offset inside: " + mouseTop);                    

            if (can_drop_inside) {
                var h1_4 = itemHeight / 4;
                if (mouseTop < h1_4) {
                    dropTarget.addClass("highlight-list-item-hover-drop-before");
                } else {
                    if (mouseTop > h1_4 && mouseTop < itemHeight - h1_4) { 
                        dropTarget.addClass("highlight-list-item-hover-drop-into"); lastDropPosition = nmaAsChildOfNode; 
                    } else {
                        if (mouseTop > itemHeight - h1_4) { dropTarget.addClass("highlight-list-item-hover-drop-after"); lastDropPosition = nmaAfterNode; }
                    }
                }
            } else {
                var h1_2 = itemHeight / 2;
                if (mouseTop < h1_2) { 
                    dropTarget.addClass("highlight-list-item-hover-drop-before");
                } else {
                    if (mouseTop > h1_2) { dropTarget.addClass("highlight-list-item-hover-drop-after"); lastDropPosition = nmaAfterNode; }
                }
            }

            return lastDropPosition;
        }

        function removeTemporaryItem() {
            $("#divWhiteboard").find(".draggable-item[data-temp-id]").remove();
        }

        var locationBoard = 0, locationList = 1, locationTree = 2;

        function draggableItemInitDrag(draggableItem, location, set_edit_mode) {

            if (!set_edit_mode && !isReadOnly) {
                $(draggableItem).draggable({
                    //appendTo: $("#divWhiteboardContainer"),
                    appendTo: "body",
                    //containment: 'parent',
                    //classes: {
                    //    "ui-draggable": "draggable-list-item-terminal-node"
                    //},
                    cancel: "input,textarea,button,select,option,.content-pro,.content-con,.dx-resizable-resizing",
                    handle: location == locationList || location == locationTree ? undefined : ".draggable-item-inner-content",
                    //helper: location == locationList || location == locationTree ? 'original' : 'clone',
                    helper: 'clone',
                    //delay: 300,
                    distance: 5,
                    opacity: 0.8,
                    scroll: true,
                    //refreshPositions: true,
                    start: function(e, ui) {     
                        isDragging = true;
                    
                        // unselect all selected boxes
                        $('#divWhiteboard .ui-selected').removeClass('ui-selected');
                    
                        x0 = ui.position.left;
                        y0 = ui.position.top;
                
                        ui.helper[0].style.zIndex = lastZIndex;
                        //$(draggableItem).css("min-height", "");
                        //draggableItem.style.zIndex = lastZIndex;                    
                        lastZIndex += 1;
                    },
                    drag: function (e, ui) {
                        var $droppable = $(this).data("current-droppable");
                        if (($droppable)) {
                            var isDropTargetAlternative = $droppable[0].getAttribute("data-isalternative") == "1";
                            dropIndicator($droppable, this, !isDropTargetAlternative);
                        }
                    },
                    stop:function(e, ui) {                
                        setTimeout(function() {isDragging = false}, 600);
                    }
                });

                //if (false && location == locationBoard) draggableItem.onclick = setBoxProsConsMode;
                if (location == locationBoard) draggableItem.ondblclick = function (event) {
                    if (!is_editing && canDoAction() && !isReadOnly) {
                        setBoxEditMode();
                        event.stopPropagation();
                        event.preventDefault();
                    }
                }
                if (location == locationBoard) draggableItemInitContextMenu();
            }

            var tempFlag = false;

            function setBoxEditMode(set_edit_mode) {
                if (!canDoAction() || isReadOnly) return;

                //draggableItem.style.padding = "1px";
                //draggableItem.style.cursor = "default";

                var w = $(draggableItem).width();
                var h = $(draggableItem).height();

                $(draggableItem).css("min-width", w);
                $(draggableItem).css("min-height", h);

                if (!is_mobile) $(draggableItem).find(".draggable-item-toolbar-div").hide();
                is_editing = true;

                var edit = document.createElement("div");
                edit.className = "draggable-item-input-box";

                var oldName = draggableItem.childNodes[0].innerText;
                draggableItem.childNodes[0].innerText = "";
                draggableItem.childNodes[0].appendChild(edit);

                $(draggableItem).find(".draggable-item-user-icon,.draggable-item-infodoc-icon").hide();

                $(edit).dxTextArea({
                    placeholder: "Type a text here...",
                    value: oldName,
                    minWidth: w + "px",
                    minHeight: h + "px",
                    onEnterKey: function (e) {
                        var commitChanges = true;
                        if (event) {
                            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
                            switch (code) {
                                case 13:                        
                                    if (event.shiftKey || event.ctrlKey || event.metaKey) {
                                        commitChanges = false;
                                    }
                            }
                        }
                        if (commitChanges) e.component.blur();
                    },
                    onKeyDown: function (e) {
                        if (e.event.originalEvent.keyCode == KEYCODE_ESCAPE) {
                            e.component.blur();
                        }
                    },
                    onFocusOut: function (e) {
                        setTimeout(function () { is_editing = false; }, 200);

                        if (tempFlag) return;

                        $(draggableItem).find(".draggable-item-user-icon,.draggable-item-infodoc-icon").show();

                        var text = e.component.option("value").trim();
                        
                        //draggableItem.style.padding = "10px";
                        //draggableItem.style.cursor = "move";
                        
                        var tempId = draggableItem.getAttribute("data-temp-id");
                        if (draggableItem.getAttribute("data-temp-id") != "" && draggableItem.getAttribute("data-temp-id") != null) {
                            if (text == "" || text == _DEFAULT_ALT_TITLE || text == _DEFAULT_SOURCE_TITLE || text == _DEFAULT_OBJ_TITLE) {
                                removeTemporaryItem();
                            } else {
                                var x = draggableItem.getAttribute("data-x");
                                var y = draggableItem.getAttribute("data-y");
                                var w = draggableItem.getAttribute("data-width");
                                var h = draggableItem.getAttribute("data-height");
                                var color = draggableItem.getAttribute("data-color");
                                draggableItem.removeAttribute("data-temp-id");
                                draggableItem.setAttribute("data-temp-id-created", tempId);
                                var params = "&x=" + x + "&y=" + y + "&w=" + w + "&h=" + h + "&title=" + encodeURIComponent(text) + "&color=" + color + "&temp_id=" + tempId + "&board_mode=" + meetingBoardMode + "&is_alt=" + (meetingBoardMode == mmAlts);
                                csAction(<%=Command.CreateNewNode%>, params);
                                draggableItem.childNodes[0].innerHTML = "<span class='draggable-item-content-text'>" + text + "</span>";
                                draggableItem.title = draggableItem.childNodes[0].innerText;
                            }
                        } else {
                            if (text.trim() !== oldName.trim()) {
                                var params = "&guid=" + draggableItem.getAttribute("data-guid") + "&name=" + encodeURIComponent(text);
                                csAction(<%=Command.RenameItem%>, params);
                                draggableItem.childNodes[0].innerHTML = "<span class='draggable-item-content-text'>" + text + "</span>";
                            } else {
                                draggableItem.childNodes[0].innerHTML = "<span class='draggable-item-content-text'>" + oldName + "</span>";
                            }
                            draggableItem.title = draggableItem.childNodes[0].innerText;
                        }                        
                        e.component.dispose();
                    }
                });
                
                tempFlag = true;
                var current_edit = $(edit).dxTextArea('instance');
                current_edit.focus();
                var ta = current_edit.element().find("textarea");
                if (typeof set_edit_mode !== "undefined" && set_edit_mode) {
                    ta.select(); // select all text
                } else {
                    if (ta.length) {
                        ta = ta[0];
                        ta.setSelectionRange(ta.value.length, ta.value.length); // put cursor at the end of the text
                    }
                }
                setTimeout(function () {tempFlag = false;}, 100);
            }

            <%--function setBoxProsConsMode() {
                if (!canDoAction() || !isProsCons()) return;

                var guid = draggableItem.getAttribute("data-guid");

                if (!isAnonymous) {
                    var params = "&guid=" + guid + "&mode=oneatatime&is_open=true";
                    csAction(<%=Command.SwitchProsCons%>, params);
                }

                SwitchProsConsForAll(false);
                SwitchProsCons(guid, true);
            }--%>


            function draggableItemInitContextMenu() {
                var draggableItemMenuItems = [
                        { text: "Edit", icon: "fas fa-pencil-alt", disabled: false,
                            onClick: function(e) {
                                setBoxEditMode();
                            }
                        },
                        { text: "Delete", icon: "far fa-trash-alt", 
                            onClick: function(e) {
                                deleteDraggableItem(draggableItem);
                            }    
                        },
                        //{
                        //    text: "Clipboard",
                        //    icon: "far fa-clipboard",
                        //    disabled: true,
                        //    items: [
                        //        { text: "Copy", icon: "far fa-copy" },
                        //        { text: "Paste", icon: "fas fa-paste" }
                        //    ]
                        //},
                        { text: "Move to <%=ParseString("%%Objectives%%")%>", icon: "fas fa-exchange-alt", visible: meetingBoardMode == mmAlts, 
                            onClick: function(e) {
                                var params = "&guid=" + draggableItem.getAttribute("data-guid");
                                if (canDoAction()) csAction(<%=Command.SendToObjectives%>, params);
                            }
                        },
                        { text: "Move to <%=ParseString("%%Alternatives%%")%>", icon: "fas fa-exchange-alt", visible: meetingBoardMode == mmSources || meetingBoardMode == mmImpacts, 
                            onClick: function(e) {
                                var params = "&guid=" + draggableItem.getAttribute("data-guid");
                                if (canDoAction()) csAction(<%=Command.SendToAlternatives%>, params);
                            }
                        },
                        { text: "Edit Information Document", icon: "far fa-file-alt", 
                            onClick: function(e) {
                                if (!canDoAction()) return;
                                editDraggableItemInfodoc(draggableItem);
                            }
                        },
                        { text: "Toggle Pros and Cons", icon: "fas fa-columns", visible: !isRisk && meetingBoardMode == mmAlts,
                            onClick: function(e) {                                
                                toggleDraggableItemProsCons(null, draggableItem);
                            }
                        },
                        { text: "Change Color of Selected Item", icon: "fas fa-palette", disabled: false,
                            onClick: function(e) {
                                ecColorPicker(false);
                            }
                        }
                ];

                var draggableItemToolbarItems = [
                        {
                            widget: "dxButton",
                            options: { hint: "Edit", icon: "fas fa-pencil-alt", disabled: false,
                                elementAttr: {style : "pointer-events: all;"},
                                onClick: function(e) {
                                    setBoxEditMode();
                                }
                            }
                        },
                        {
                            widget: "dxButton",
                            options: { hint: "Delete", icon: "far fa-trash-alt", 
                                elementAttr: {style : "pointer-events: all;"},
                                onClick: function(e) {
                                    deleteDraggableItem(draggableItem);
                                }    
                            }
                        },
                        {
                            widget: "dxButton",
                            options: { hint: "Edit Information Document", icon: "far fa-file-alt", 
                                elementAttr: {style : "pointer-events: all;"},
                                onClick: function(e) {
                                    if (!canDoAction()) return;
                                    editDraggableItemInfodoc(draggableItem);
                                }
                            }
                        },                        
                        {
                            widget: "dxButton",
                            options: { hint: "Change Color of Selected Item", icon: "fas fa-palette", disabled: false,
                                elementAttr: {style : "pointer-events: all;"},
                                onClick: function(e) {
                                    ecColorPicker(false);
                                }
                            }
                        },
                        {
                            widget: "dxButton",
                            options: { hint: "Toggle Pros and Cons", icon: "fas fa-columns", visible: !isRisk && meetingBoardMode == mmAlts,
                                elementAttr: {style : "pointer-events: all;"},
                                onClick: function(e) {                                
                                    toggleDraggableItemProsCons(e.event, draggableItem);
                                }
                            }
                        }
                ];

                var menu_div = document.createElement("div");
                draggableItem.appendChild(menu_div);

                draggableItem.toolbarItems = draggableItemToolbarItems;

                $(menu_div).dxContextMenu({
                    items: draggableItemMenuItems,
                    onShowing: function (e) { 
                        if (!canDoAction()) e.cancel = true; 
                        $(".custom_tooltip").tooltip().hide();  // D6557
                        if ((e.component.option("target"))) e.component.option("target").style.zIndex = lastZIndex;
                        lastZIndex += 1;
                    },
                    target: draggableItem
                });

                // init item toolbar menu
                /* draggable item toolbar */  //todo;
                //return;
                //var itemToolbarDiv = document.createElement("div");                

                //itemToolbarDiv.className = "cs-draggable-item-toolbar";
                //itemToolbarDiv.innerHTML += '<i class="fas fa-pencil-alt cs-draggable-item-toolbar-button" onclick="return false; setBoxEditMode();"></i>';
                //itemToolbarDiv.innerHTML += '<i class="far fa-trash-alt cs-draggable-item-toolbar-button" onclick="return false; deleteDraggableItem();"></i>';
                //itemToolbarDiv.innerHTML += '<i class="far fa-file-alt cs-draggable-item-toolbar-button" onclick="return false; editDraggableItemInfodoc();"></i>';
                //itemToolbarDiv.innerHTML += '<i class="fas fa-columns cs-draggable-item-toolbar-button" onclick="return false; toggleDraggableItemProsCons();"></i>';
                //itemToolbarDiv.innerHTML += '<i class="fas fa-palette cs-draggable-item-toolbar-button" onclick="return false; ecColorPicker(false);"></i>';
                
                //draggableItem.appendChild(itemToolbarDiv);
            }

            function ecColorPicker(has_reset_all_option) {
                cur_draggableItem = draggableItem;

                if (popupColors) $(".popupColors").remove();
                var $popupContainer = $("<div></div>").addClass("popupColors").appendTo($("#popupColors"));
                popupColors = $popupContainer.dxPopup(popupColorsOptions).dxPopup("instance");
                popupColors.show();

                $("#div_colorpicker").dxColorBox({ 
                    value: cur_draggableItem.style.backgroundColor,
                    disabled: isReadOnly,
                    onValueChanged: function (e) {
                        ecColorApply(e.value);
                    }
                });
                
                $("#btnCloseColorPicker").dxButton({
                    icon: "fas fa-times",
                    text: "<% = ResString("btnClose") %>",
                    onClick: function (e) {
                        popupColors.hide();
                    }
                });

                $("#btnResetAll").toggle(has_reset_all_option);
            }

            if (set_edit_mode) {
                setBoxEditMode(set_edit_mode);
            }
        }

        /* Debug Window */
        var popupColors = null, 
            popupColorsOptions = {
                animation: null,
                width: 450,
                height: 150,
                contentTemplate: function() {
                    return  $("<div style='padding: 1ex; text-align: center;'></div>").append(
                        $("<div id='div_colorpicker' style='width: 260px; padding: 10px; border: 0;'></div>"),
                        $("<a href='' id='btnResetAll' class='actions text' style='margin-top: 4px; display: none;' onclick='nodesColorsReset(); return false;'><%=ResString("btnResetAll")%></a>"),
                        $("<br></br>"),
                        $("<div id='btnCloseColorPicker'></div>")
                        );
                },
                showTitle: true,
                title: "Change Color of Selected Item",
                dragEnabled: true,
                shading: false,
                closeOnOutsideClick: false,
                resizeEnabled: true,                
                showCloseButton: true
            };

        function ecColorApply(newValue) {
            if ((colorDlg)) colorDlg.hide();

            cur_draggableItem.style.backgroundColor = newValue;
            csAction(<%=Command.SetNodesColor%>, "&guid=" + cur_draggableItem.getAttribute("data-guid") + "&color=" + newValue);
        }

        function toggleDraggableItemProsCons(event, draggableItem) {
            if (!canDoAction()) return;
            var guid = draggableItem.getAttribute("data-guid");
            var node = getWhiteboardNodeByGuid(guid);
            if ((node)) {
                node.is_pc_open = node.is_pc_open ? 0 : 1;
                if (!isAnonymous) {
                    var params = "&guid=" + guid + "&mode=single&is_open=" + (node.is_pc_open > 0);
                    csAction(<%=Command.SwitchProsCons%>, params);
                }
                ToggleProsCons(node);
            }
            if ((event)) {
                event.stopPropagation();
                event.preventDefault();
            }
        }

        function editDraggableItemInfodoc(draggableItem) {
            var guid = draggableItem.getAttribute("data-guid");
            var node = getWhiteboardNodeByGuid(guid);
            if ((node)) {
                editInfodoc(node.nodeguid, <% = Cint(reObjectType.AntiguaInfodoc) %>);
            }
        }

        function deleteDraggableItem(draggableItem) {
            if (!canDoAction()) return;
            dxConfirm(replaceString("{0}", draggableItem.childNodes[0].innerText, resString("msgSureDeleteCommon")), function () { doDeleteDraggableItem(draggableItem); });
        }

        function doDeleteDraggableItem(draggableItem) {
            var params = "&guids=" + draggableItem.getAttribute("data-guid");
            deleteWhiteboardNode(draggableItem.getAttribute("data-guid"));
            csAction(<%=Command.DeleteNodes%>, params + "&whiteboard_items=true");
        }

        function onSetAltsContributionsCallback(data) {
            var res = JSON.parse(data);
            if (res) {
                if (res.Result == _res_Success) {
                    DevExpress.ui.notify(resString("msgSaved"), "success");
                }
            }
        }

        function onGetAltsContributionsCallback(data) {
            // show popup with hierarchy
            var res = JSON.parse(data);
            if (res) {
                if (res.Result == _res_Success) {
                    var objs = res.Data;

                    $("#popupContainer").dxPopup({
                        animation: null,
                        contentTemplate: function() {
                            return $("<div id='divScroll'><div id='divHierarchy' style='text-align:left; min-height: 100px; overflow: auto;'></div></div>");
                        },
                        width: "80%",
                        height: "80%",
                        showTitle: true,
                        title: "Contributions",
                        dragEnabled: true,
                        shading: true,
                        closeOnOutsideClick: true,
                        resizeEnabled: false,
                        showCloseButton: true,
                        toolbarItems: [
                            {
                                toolbar: 'bottom',
                                location: 'after',
                                locateInMenu: 'auto',
                                widget: 'dxButton',
                                visible: true,
                                options: {
                                    text: "<% =JS_SafeString(ResString("btnSave")) %>",
                                    icon: "fas fa-save",
                                    elementAttr: {id: 'btn_save_changes'},
                                    disabled: true,
                                    onClick: function () {
                                        var nodes = $("#divHierarchy").dxTreeView("instance").getNodes();
                                        selNodesIDs = "";
                                        getSelectedNodesIDs(nodes);
                                        var params = "&guid=" + res.Tag + "&ids=" + selNodesIDs;
                                        if (canDoAction()) callAjax("<%=_PARAM_ACTION %>=" + (res.Message == "get_alt_contributions" ? "set_alt_contributions" : "set_obj_contributions") + params, onSetAltsContributionsCallback, ajaxMethod, false, "", true, please_wait_delay);
                                        closePopup();
                                    }
                                }
                            }, {
                                toolbar: 'bottom',
                                location: 'after',
                                locateInMenu: 'auto',
                                widget: 'dxButton',
                                visible: true,
                                options: {
                                    text: "<% =JS_SafeString(ResString("btnCancel")) %>",
                                    icon: "fas fa-ban",
                                    disabled: false,
                                    onClick: function () {
                                        closePopup();
                                    }
                                }
                            }
                        ]
                    });

                    $("#popupContainer").dxPopup("show");

                    $("#divHierarchy").dxTreeView({
                        height: "80%",
                        dataSource: objs,
                        dataStructure: "plain",
                        displayExpr: "text",
                        keyExpr: "id",
                        parentIdExpr: "pid",
                        rootValue: -1,
                        showCheckBoxesMode: "selectAll",
                        selectedExpr: "selected",
                        selectionMode: "multiple",
                        selectByClick: true,
                        onItemRendered: function (e) {
                            if ((e.node.children) && e.node.children.length > 0) {
                                e.itemElement.parent().find(".dx-checkbox").hide();
                            }
                        },                        
                        selectNodesRecursive: false,
                        onSelectionChanged: function (e) {
                            var btn = $("#btn_save_changes").dxButton("instance");
                            if ((btn)) btn.option("disabled", false);
                        }
                    });

                    $("#divScroll").dxScrollView({
                        height: "100%",
                        width: "100%",
                        direction: "both"
                    });
                }
            }
        }

        function getSelectedNodesIDs(nodes) {
            if ((nodes) && (nodes.length > 0)) {
                for (var i = 0; i < nodes.length; i++) {
                    var node = nodes[i];
                    if ((node) && (!(node.children) || node.children.length == 0) && node.selected) {
                        selNodesIDs += (selNodesIDs == "" ? "" : ",") + node.itemData.guid;
                    }
                    getSelectedNodesIDs(node.children)
                }
            }
        }

        function draggableItemCreateOnWhiteboard(isCreatedOnClick, e) {
            if (isDragging) return false;
            var evt = e || window.event;
            if ((typeof e == "undefined") || evt.button === 0) {        
                var board = document.getElementById("divWhiteboard");
                var div = document.createElement("div");
                var itemContentDiv = document.createElement("div");
                var itemTitle = meetingBoardMode == mmAlts ? _DEFAULT_ALT_TITLE : (isRisk && meetingBoardMode == mmSources ? _DEFAULT_SOURCE_TITLE : _DEFAULT_OBJ_TITLE);
                itemContentDiv.innerHTML = "<span class='draggable-item-content-text'>" + itemTitle + "</span>";
                itemContentDiv.className = "draggable-item-inner-content";
                //itemContentDiv.style.width = "100%";
                //itemContentDiv.style.height = "100%";
                //itemContentDiv.style.backgroundColor = "red";
                div.appendChild(itemContentDiv);                
                div.title = itemTitle;
                div.className = "draggable-item";
                div.style.position = "absolute";
                var itemColor = meetingBoardMode == mmAlts ? _DEFAULT_COLOR_ALT : (isRisk && meetingBoardMode == mmSources ? _DEFAULT_COLOR_SOURCE : _DEFAULT_COLOR_OBJ);
                if (isCreatedOnClick && (evt) && (evt.clientX) && (evt.clientY)) {
                    var tempId = cmdOwner + "_" + lastZIndex;

                    var pos = getOffsetCoords("divWhiteboard", evt.clientX, evt.clientY)
                    div.style.left = pos.x + "px";
                    div.style.top = pos.y + "px";
                
                    div.setAttribute("data-x", pos.x);
                    div.setAttribute("data-y", pos.y);
                    div.setAttribute("data-width", _DEFAULT_ITEM_WIDTH);
                    div.setAttribute("data-height", _DEFAULT_ITEM_HEIGHT);
                    div.setAttribute("data-temp-id", tempId);
                    div.setAttribute("data-color", itemColor);
                    div.setAttribute("data-author", UserName + " (" + UserEmail + ")");

                    <%--csAction(<%=Command.CreateNewNode%>, "&x=" + pos.x + "&y=" + pos.y + "&title=" + encodeURIComponent(itemTitle) + "&color=" + itemColor + "&temp_id=" + tempId + "&is_alt=" + meetingBoardMode == mmAlts);--%>
                    if (isColorCoding) div.appendChild(getBoxColorMarker(UserName + " (" + UserEmail + ")", resString("lblCreatorName") + ": " + UserName + " (" + UserEmail + ")" + "<br>Last modified by: " + UserName + " (" + UserEmail + ")"));
                }

                board.appendChild(div);
                div.style.backgroundColor = itemColor;
                div.style.zIndex = lastZIndex;
                div.style.width = _DEFAULT_ITEM_WIDTH + "px";
                div.style.height = _DEFAULT_ITEM_HEIGHT + "px";
                itemContentDiv = div.style.height;
                lastZIndex += 1;

                $(div).click(function (event) {
                    this.style.zIndex = lastZIndex;
                    lastZIndex += 1;

                    event.stopPropagation();
                    event.preventDefault();
                });
                
                draggableItemInitDrag(div, locationBoard, isCreatedOnClick);
                                
                return div;
            }
        }

        function getBoxColorMarker(s, title) {
            var marker = document.createElement("i");
            marker.title = title;
            marker.className = "fas fa-user-circle draggable-item-user-icon";
            marker.style.color = getUserColorByName(s);
            marker.style.position = "absolute";
            marker.style.right = "3px";
            marker.style.top = "4px";
            marker.style.cursor = "pointer";
            marker.onclick = toggleOnlineParticipantsView;
            marker.onmouseenter = function (e) {
                // highlight all draggable items from this user
                highlightDraggableItemsByUser(s, true);
            }
            marker.onmouseout = function (e) {
                // do not fire if element's child
                var e = event.toElement || event.relatedTarget;
                if (((e.parentNode) && e.parentNode == this) || e == this) {
                    return;
                }
                // unhighlight all draggable items from this user
                highlightDraggableItemsByUser(s, false);
            }
            return marker;
        }

        function getPCMovedMarker(visible) {
            var marker = document.createElement("i");
            marker.className = "fas fa-check";
            marker.style.position = "absolute";
            marker.style.right = "14px";
            marker.style.top = "4px";
            marker.style.display = visible ? "inline" : "none";
            return marker;
        }

        function getBoxProsConsPane(node) {
            node.itemProsConsDiv = document.createElement("div");
            node.itemProsConsDiv.className = "item_pros_cons_pane";            
            node.itemProsConsDiv.style.display = !isRisk && node.is_pc_open > 0 ? "" : "none";

            node.headerPro = document.createElement("div");
            node.headerPro.innerHTML = "Pros";
            node.headerPro.className = "cs-pros-header-pane";    
            node.headerPro.style.borderRight = "1px solid #ddd";
            node.headerPro.onclick = function (event) { 
                event = event || window.event;
                event.stopPropagation();
                event.preventDefault();
                addProsConsDialog(node, true); 
                return false;
            }
            node.itemProsConsDiv.appendChild(node.headerPro);

            node.headerCon = document.createElement("div");
            node.headerCon.innerHTML = "Cons";
            node.headerCon.className = "cs-cons-header-pane";            
            node.headerCon.onclick = function onClickToAddACon(event) { 
                event = event || window.event;
                event.stopPropagation();
                event.preventDefault();
                addProsConsDialog(node, false); 
                return false;
            }
            node.itemProsConsDiv.appendChild(node.headerCon);

            node.contentPro = document.createElement("div");
            node.contentPro.className = "content-pro";
            node.contentPro.style.verticalAlign = "top";            
            node.contentPro.style.overflowX = "hidden";
            node.contentPro.style.overflowY = "auto";
            node.contentPro.style.display = "inline-block";
            node.contentPro.style.cursor = "pointer";
            node.contentPro.style.borderRight = "1px solid #ddd";
            node.contentPro.onclick = function (event) { 
                event = event || window.event;
                event.stopPropagation();
                event.preventDefault();
                addProsConsDialog(node, true); 
                return false;
            }
            node.itemProsConsDiv.appendChild(node.contentPro);
            prosconsInitContextMenu(node.contentPro, true, !isReadOnly, null, node);

            node.contentCon = document.createElement("div");
            node.contentCon.className = "content-con";
            node.contentCon.style.verticalAlign = "top";
            node.contentCon.style.overflowX = "hidden";
            node.contentCon.style.overflowY = "auto";
            node.contentCon.style.display = "inline-block";
            node.contentCon.style.cursor = "pointer";
            node.contentCon.onclick = function (event) { 
                event = event || window.event;
                event.stopPropagation();
                event.preventDefault();
                addProsConsDialog(node, false); 
                return false;
            }
            node.itemProsConsDiv.appendChild(node.contentCon);
            prosconsInitContextMenu(node.contentCon, false, !isReadOnly, null, node);

            if ((node.pros)) {
                for (var i = 0; i < node.pros.length; i++){
                    draggableItemCreateOnList(node.pros[i], node.itemProsConsDiv, false, true, "draggable-list-item-small", true, true, 0, node);
                }
            }

            if ((node.cons)) {
                for (var i = 0; i < node.cons.length; i++){
                    draggableItemCreateOnList(node.cons[i], node.itemProsConsDiv, false, true, "draggable-list-item-small", true, false, 0, node);
                }
            }
            
            resizeBoxProsConsPane(node);

            return node.itemProsConsDiv;
        }    
        
        function resizeBoxProsConsPane(node) {
            var _INIT_PROS_CONS_PANE_WIDTH = 350; //300 x 150
            _DEFAULT_PROS_CONS_PANE_WIDTH = node.attributes.w*1 + 90;
            _DEFAULT_PROS_CONS_PANE_HEIGHT = node.attributes.h*2;

            var fullPCheight = _DEFAULT_PROS_CONS_PANE_HEIGHT + 2;
            fullPCheight = fullPCheight < 110 ? 110 : fullPCheight;

            if ((node.itemProsConsDiv)) {
                node.itemProsConsDiv.style.width = (_DEFAULT_PROS_CONS_PANE_WIDTH + 2) + "px";
                node.itemProsConsDiv.style.height = fullPCheight + "px";
                node.itemProsConsDiv.style.left = (node.attributes.w / 2 - _DEFAULT_PROS_CONS_PANE_WIDTH / 2) + "px";
            
                node.headerPro.style.width = Math.floor(_DEFAULT_PROS_CONS_PANE_WIDTH / 2) + "px";
                node.headerCon.style.width = Math.floor(_DEFAULT_PROS_CONS_PANE_WIDTH / 2) + "px";
                node.contentPro.style.width = Math.floor(_DEFAULT_PROS_CONS_PANE_WIDTH / 2) + "px";            
                node.contentPro.style.height = fullPCheight - 28 + "px";
                node.contentCon.style.width = Math.floor(_DEFAULT_PROS_CONS_PANE_WIDTH / 2) + "px";
                node.contentCon.style.height = fullPCheight - 28 + "px";

                node.divToolbar.style.display = is_mobile ? "" : "none"; //node.attributes.w <= 200 || node.attributes.h <= 50 ? "none" : "";
            }
        }

        function draggableItemCreateOnList(node, parent, can_drag, can_delete, class_name, is_proscons, is_pro, level, pros_cons_parent_node) {            
            var itemTitle = meetingBoardMode == mmAlts ? _DEFAULT_ALT_TITLE : (isRisk && meetingBoardMode == mmSources ? _DEFAULT_SOURCE_TITLE : _DEFAULT_OBJ_TITLE);
            
            var div = document.createElement("div");

            var itemContentDiv = document.createElement("div");
            itemContentDiv.innerText = itemTitle;
            itemContentDiv.className = "cs-text-overflow-ellipsis-line";
            div.appendChild(itemContentDiv);
            
            div.childNodes[0].innerText = node.name;
            //div.childNodes[0].innerHTML += (node.pc_moved ? pc_moved_marker : '');
            div.appendChild(getPCMovedMarker(node.pc_moved));
            div.style.paddingRight = "22px";
            div.title = htmlEscape(div.childNodes[0].innerText);
            div.className = class_name;
            div.setAttribute("data-guid", node.nodeguid);
            div.setAttribute("data-isalternative", node.isalt ? "1" : "0");
        
            //var el_h = 25;
            //div.style.height = el_h + "px";
            if (!can_drag) div.style.cursor = "default";
            if (typeof level != "undefined" && (level)) div.style.left = level * 20 + "px";

            var itemColor = meetingTreeMode == mmAlts ? _DEFAULT_COLOR_ALT : (meetingTreeMode == mmSources ? _DEFAULT_COLOR_SOURCE : _DEFAULT_COLOR_OBJ);
            div.style.backgroundColor = itemColor;
            
            if (is_proscons) {
                parent.childNodes[is_pro ? 2 : 3].appendChild(div); // insert into pros or cons panel
            } else {
                parent.appendChild(div);
            }

            if (can_drag && !isReadOnly) {
                draggableItemInitDrag(div, locationList, false);                
            }

            if (!is_proscons) {
                initListItemDrop($(div), meetingTreeMode != mmAlts);
            }

            if ((is_proscons)) {
                div.setAttribute("alt-guid", pros_cons_parent_node.nodeguid);                
                if (isColorCoding) div.appendChild(getBoxColorMarker(node.author, resString("lblCreatorName") + ": " + node.author + "<br>Last modified by: " + node.lastmodifiedby));
                //$(div).css("border-left", "3px solid " + getUserColorByName(node.author));
                div.title += "<br><br><small>" + resString("lblCreatorName") + ": " + node.author + "<br>Last modified by: " + node.lastmodifiedby + "</small>";                
                prosconsInitContextMenu(div, is_pro, !isReadOnly, node, pros_cons_parent_node);
                prosconsInitDrag(div, is_pro, !isReadOnly, node, pros_cons_parent_node);
            } else {
                treeItemInitContextMenu(div, node, can_delete);
                div.ondblclick = function (event) {
                    event = event || window.event;
                    treeItemEdit(node);
                    event.stopPropagation();
                    event.preventDefault();
                };
            }
            
            return div;
        }
    
        function whiteboardClick(e) {
            //if anything selected then cancel selection
            if ($('#divWhiteboard .ui-selected').length > 0) {
                $('#divWhiteboard .ui-selected').removeClass('ui-selected');
            }
            if ($('.draggable-item-input-box').data("dxTextArea")) {
                $('.draggable-item-input-box').dxTextArea("instance").blur();
            }
            //event.stopPropagation();
            //event.preventDefault();
        }

        function whiteboardDblClick(e) {
            //if anything selected then cancel selection
            if ($('#divWhiteboard .ui-selected').length > 0) {
                $('#divWhiteboard .ui-selected').removeClass('ui-selected');
            } else {
                if (!canDoAction() || isReadOnly || is_editing) return;
                draggableItemCreateOnWhiteboard(true, e);
                //event.stopPropagation();
                //event.preventDefault();
            }
        }
        /* end Draggable elements */

        /* Pros and Cons */
        function scanProsCons() {
            is_all_pros_cons_shown = false;
            board_alts_count = 0;
            board_alts_pc_open_count = 0;

            pros_cons_items = [];
            pros_cons_datasource = [];

            for (var i = 0; i < board_nodes.length; i++) {
                if (board_nodes[i].isalt) {
                    board_alts_count += 1;
                    board_alts_pc_open_count += board_nodes[i].is_pc_open;
                    if ((board_nodes[i].pros) && board_nodes[i].pros.length > 0 || (board_nodes[i].cons) && board_nodes[i].cons.length > 0) {
                        var itemP = { altname: board_nodes[i].name, items: [], ispro: true};
                        if ((board_nodes[i].pros)) {
                            for (var j = 0; j < board_nodes[i].pros.length; j++) {
                                itemP.items.push({title:board_nodes[i].pros[j].name, nodeguid: board_nodes[i].pros[j].nodeguid, pc_moved: board_nodes[i].pros[j].pc_moved, ispro: true});
                                //sPros += (sPros == "" ? "" : "\r\n") + board_nodes[i].pros[j].name;
                                <%--pros_cons_datasource.push({"<%=ParseString("%%Alternative%%")%>Name" : board_nodes[i].name, "Pros" : board_nodes[i].pros[j].name, "Cons" : ""});--%>
                            }
                        }
                        pros_cons_items.push(itemP);
                        var itemC = { altname: board_nodes[i].name, items: [], ispro: false};
                        if ((board_nodes[i].cons)) {
                            for (var j = 0; j < board_nodes[i].cons.length; j++) {
                                itemC.items.push({title:board_nodes[i].cons[j].name, nodeguid: board_nodes[i].cons[j].nodeguid, pc_moved: board_nodes[i].cons[j].pc_moved, ispro: false});
                                //sCons += (sCons == "" ? "" : "\r\n") + board_nodes[i].cons[j].name;
                                <%--pros_cons_datasource.push({"<%=ParseString("%%Alternative%%")%>Name" : board_nodes[i].name, "Pros" : "", "Cons" : board_nodes[i].cons[j].name});--%>
                            }
                        }
                        pros_cons_items.push(itemC);

                        var p_len = (board_nodes[i].pros) ? board_nodes[i].pros.length : 0;
                        var c_len = (board_nodes[i].cons) ? board_nodes[i].cons.length : 0;
                        var len = Math.max(p_len, c_len);

                        for (var j = 0; j < len; j++) {
                            pros_cons_datasource.push({"<%=ParseString("%%Alternative%%")%>Name" : board_nodes[i].name, "Pros" : (board_nodes[i].pros && board_nodes[i].pros.length > j) ? board_nodes[i].pros[j].name : "", "Cons" : (board_nodes[i].cons && board_nodes[i].cons.length > j) ? board_nodes[i].cons[j].name : ""});
                        }

                    }
                }
            }

            is_all_pros_cons_shown = board_alts_count == board_alts_pc_open_count;
            return false;
        }

        function initProsCons() {
            scanProsCons();
            if ($("#divProsConsTable").data("dxDataGrid")) $("#divProsConsTable").dxDataGrid("instance").dispose();
            
            $("#divProsConsTable").dxDataGrid({
                columns: [
                    {
                        dataField: "<%=ParseString("%%Alternative%%")%>Name",
                        groupIndex: 0,
                        "encodeHtml" : false
                    },
                    "Pros",
                    "Cons"
                ],
                dataSource: pros_cons_datasource,
                height: "100%",
                grouped: true,
                grouping: {
                    autoExpandAll: true,
                },
                groupPanel: { visible: false },
                collapsibleGroups: true,
                //groupTemplate: function(data) {
                //    return $("<div class='" + (data.ispro ? "cs-pros-header-overall" : "cs-cons-header-overall") + "' title='" + data.altname + "'>" + (data.ispro ? "Pros of" : "Cons of") + ": " + data.altname + "</div>");    
                //},
                itemTemplate: function(data, index) {
                    //return $("<span class='" + (data.ispro ? "cs-pros-list-item" : "cs-cons-list-item") + "' title='" + data.title + "'>" + data.title + "</span>");    
                    var retVal = $("<span title='" + data.title + "'>" + data.title + "</span>");
                    retVal.append(getPCMovedMarker(data.pc_moved));
                    return retVal;
                }
            });

            resizeProsCons();
        }

        function refreshProsConsList() {
            scanProsCons();
            var list = $("#divProsConsTable").data("dxDataGrid") ? $("#divProsConsTable").dxDataGrid("instance") : null;
            
            if ((list)) {
                list.option("dataSource", pros_cons_datasource);
                list.repaint();
            }

            resizeProsCons();
        }

        function ToggleProsCons(node) {
            var div = getWhiteboardElementByGuid(node.nodeguid);
            if ((div)) {
                //div.childNodes[div.childNodes.length - 1].style.display = node.is_pc_open > 0 ? "" : "none";
                if (node.is_pc_open > 0) {
                    $(div).find(".item_pros_cons_pane").show();
                    $(div).css("z-index", lastZIndex);
                    lastZIndex += 1;
                } else {
                    $(div).find(".item_pros_cons_pane").hide();
                }
            }
        }

        function SwitchProsCons(node_id, value) {
            var div = getWhiteboardElementByGuid(node_id);
            if ((div)) {
                div.childNodes[div.childNodes.length - 1].style.display = (value) ? "" : "none";
            }
        }

        function SwitchProsConsForAll(is_open) {
            for (var i = 0; i < board_nodes.length; i++) {
                if (board_nodes[i].isalt) {
                    board_nodes[i].is_pc_open = is_open ? 1 : 0;
                    ToggleProsCons(board_nodes[i]);                    
                }
            }
        }

        function onProsConsPaneClick(e) {
            if (canDoAction()) {
                e.stopPropagation();
                e.preventDefault();
                return;
            }
        }

        function getProsConsName(guid) {
            for (var i = 0; i < pros_cons_items.length; i++) {
                for (var j = 0; j < pros_cons_items[i].items.length; j++) {
                    var item = pros_cons_items[i].items[j];
                    if (item.nodeguid == guid) return item.title;
                }
            }
            return "";
        }

        function prosconsInitContextMenu(parent_div, is_pro, can_delete, node, pros_cons_parent_node) {
            var menu_div = document.createElement("div");
            parent_div.appendChild(menu_div);
            menu_div.parent_div = parent_div;
            $(menu_div).dxContextMenu({
                items: [
                    { text: "Add" + (is_pro ? " a Pro" : " a Con"), icon: "fas fa-plus", disabled: isReadOnly,
                        onClick: function(e) {
                            addProsConsDialog(pros_cons_parent_node, is_pro);
                        }
                    },
                    { text: "Edit " + (((node)) ? "\"" + ShortString(node.name, 30, false) + "\"" : ""), icon: "fas fa-edit", visible: can_delete, disabled: !can_delete || isReadOnly,
                        onClick: function(e) {
                            editProsConsName(node, is_pro, pros_cons_parent_node["nodeguid"]);                            
                        }    
                    },
                    { text: "Paste", icon: "fas fa-paste", disabled: true, //isReadOnly,
                        onClick: function(e) {
                            notImplemented();
                        }    
                    },
                    { text: "Delete " + (((node)) ? "\"" + ShortString(node.name, 30, false) + "\"" : ""), icon: "fas fa-trash-alt", visible: can_delete, disabled: !can_delete || isReadOnly,
                    onClick: function(e) {
                            dxConfirm(replaceString("{0}", node.name, resString("msgSureDeleteCommon")), function () {
                                var params = "&node_guid=" + pros_cons_parent_node["nodeguid"] + "&item_guid=" + node["nodeguid"] + "&is_pro=" + is_pro + "&all=false";
                                csAction(<%=Command.DeleteProOrCon%>, params);
                            });                            
                        }    
                    },
                    { text: "Clear", icon: "fas fa-trash-alt", disabled: isReadOnly,
                    onClick: function(e) {
                            dxConfirm(resString("msgConfirmDeleteAllItems"), function () {
                                var params = "&node_guid=" + pros_cons_parent_node["nodeguid"] + "&is_pro=" + is_pro + "&all=true";
                                csAction(<%=Command.DeleteProOrCon%>, params);
                            });
                        }    
                    }
                ],
                onShowing: function (e) { 
                    if (!canDoAction()) e.cancel = true; 
                    $(".custom_tooltip").tooltip().hide();  // D6557
                },
                onHiding: function (e) { 
                    if (!canDoAction()) e.cancel = true; 
                },
                target: parent_div
            });
        }

        function prosconsInitDrag(div, is_pro, can_delete, node, pros_cons_parent_node) {
            if (isReadOnly) return;
            $(div).draggable({
                //appendTo: $("#divWhiteboardContainer"),
                appendTo: "body",
                //containment: 'parent',
                helper: 'clone',
                distance: 5,
                opacity: 0.8,
                scroll: true,
                //refreshPositions: true,
                start: function(e, ui) {              
                    isDragging = true;
                    
                    // unselect all selected boxes
                    $('#divWhiteboard .ui-selected').removeClass('ui-selected');
                    
                    x0 = ui.position.left;
                    y0 = ui.position.top;
                
                    //ui.helper[0].style.zIndex = lastZIndex;
                    //lastZIndex += 1;
                },
                drag: function (e, ui) {
                    var $droppable = $(this).data("current-droppable");
                    if (($droppable)) {
                        var isDropTargetAlternative = $droppable[0].getAttribute("data-isalternative") == "1";
                        dropIndicator($droppable, this, !isDropTargetAlternative);
                    }
                },
                stop: function(e, ui) {
                    setTimeout(function() {isDragging = false}, 600);
                }
            });

            div.onclick = function (event) {
                event = event || window.event;
                editProsConsName(node, is_pro, pros_cons_parent_node["nodeguid"]);
                event.stopPropagation();
                event.preventDefault();
            }
        }

        function editProsConsName(node, is_pro, pros_cons_parent_node_guid) {
            if (!canDoAction()) return;

            if ($("#tbNodeName").data("dxTextBox")) {
                $("#tbNodeName").dxTextBox("instance").dispose();
            }
            $("#tbNodeName").remove();
            dxDialog("<div id='tbNodeName' style='width: 400px;'></div>", function () { onSaveProsConsName(node.nodeguid, node.name, is_pro, pros_cons_parent_node_guid); }, ";", "Enter new name:");
            $("#tbNodeName").dxTextBox({
                value: node.name,
                placeholder: "Type new name here..."
            });

            dxTextBoxFocus('tbNodeName');
        }

        function onSaveProsConsName(guid, old_name, is_pro, pros_cons_parent_node_guid) {
            var text = "";
            if ($("#tbNodeName").data("dxTextBox")) {
                text = $("#tbNodeName").dxTextBox("instance").option("value").trim();
            }
            if (text != "" && text != old_name) {
                var params = "&item_guid=" + guid + "&name=" + encodeURIComponent(text) + "&is_pro=" + is_pro + "&node_guid=" + pros_cons_parent_node_guid;
                csAction(<%=Command.RenameProsConsItem%>, params);
            }
        }

        function addProsConsDialog(pros_cons_parent_node, is_pro) {
            if (!canDoAction()) return;

            if ($("#tbNodeName").data("dxTextBox")) {
                $("#tbNodeName").dxTextBox("instance").dispose();
            }
            $("#tbNodeName").remove();
            dxDialog("<div id='tbNodeName' style='width: 400px;'></div>", function () { onAddProsConsItem(pros_cons_parent_node, is_pro); }, ";", (is_pro ? "Enter pros for " : "Enter cons for ") + pros_cons_parent_node.name);
            $("#tbNodeName").dxTextArea({
                height: 150,
                value: "",
                text: "",
                placeholder: is_pro ? resString("msgEnterProsHint") : resString("msgEnterConsHint")
            });

            dxTextAreaFocus('tbNodeName');
        }

        function onAddProsConsItem(pros_cons_parent_node, is_pro) {
            var text = "";
            if ($("#tbNodeName").data("dxTextArea")) {
                text = $("#tbNodeName").dxTextArea("instance").option("value").trim();
            }
            if (text != "") {
                var params = "&guid=" + pros_cons_parent_node.nodeguid + "&name=" + encodeURIComponent(text) + "&is_pro=" + is_pro + "&lastmodifiedby=" + UserName + " (" + UserEmail + ")";
                csAction(<%=Command.CreateNewItemToProsCons%>, params, function () { 
                    //addProsConsDialog(pros_cons_parent_node, is_pro); 
                });
            }
        }
        /* end Pros and Cons */

        function setMeetingState(newState) {
            meetingState = newState;
            csAction(<%=Command.SetMeetingState%>, "&value=" + meetingState);
        }

        function initMeetingState() {
            var lbl = document.getElementById("lblWaitingMsg");
            if ((lbl)) {
                if (meetingState == msInActive) lbl.innerHTML = resString(!isAnonymous ? "CSWaitingText_PM_Inactive" : "CSWaitingText_Inactive");
                if (meetingState == msBooted)   lbl.innerHTML = resString("CSWaitingText_Booted");
                if (meetingState == msPaused)   lbl.innerHTML = resString("CSWaitingText_Paused");
                if (meetingState == msStopped)  lbl.innerHTML = resString("CSWaitingText_Stopped");
                if (meetingState == msOwnerDisconnected) lbl.innerHTML = resString("CSWaitingText_PM_Disconnected");
            }

            if (meetingState == msActive) {
                $(".sidebar-menu-panel").css("z-index", "3");
                $("#divWaiting").hide(); 
                $("#lblWaitingMsg").hide();
                $("#divOnlineUsers").hide();
                $("#divWB").fadeTo(0, 1);
                $("#divTreeSplitter").fadeTo(0, 1);
            } else { 
                $(".sidebar-menu-panel").css("z-index", "1500");
                $("#divWaiting").show();
                $("#lblWaitingMsg").show();
                $("#divOnlineUsers").show();
                if (isAnonymous) {
                    $("#divWB").fadeTo(0, 0);
                    $("#divTreeSplitter").fadeTo(0, 0);
                }
            }

            //var is_button_visible = ((meetingOwner === -1) || (meetingOwner === cmdOwner)) && !isAnonymous;
            var is_button_visible = !isAnonymous;

            updateButtonState("#btn_start", is_button_visible, !(meetingState == msInActive || meetingState == msPaused || meetingState == msStopped));
            updateButtonState("#btn_pause", is_button_visible, meetingState == msPaused || meetingState == msStopped || meetingState == msInActive);
            updateButtonState("#btn_stop", is_button_visible, meetingState == msInActive || meetingState == msStopped || meetingState == msOwnerDisconnected);
            updateButtonState("#btn_manage_users", is_button_visible, meetingState == msInActive || meetingState == msStopped || meetingState == msOwnerDisconnected);
            updateButtonState("#btn_invite", is_button_visible, meetingState == msInActive || meetingState == msStopped || meetingState == msOwnerDisconnected);
            updateButtonState("#btn_link", is_button_visible, meetingState == msInActive || meetingState == msStopped || meetingState == msOwnerDisconnected);
            updateButtonState("#btn_settings", !isAnonymous, meetingState != msActive);
            updateButtonState("#btn_refresh", true, meetingState != msActive);

            if ($("#divNewMessage").data("dxTextBox")) {
                var divNewMessage = $("#divNewMessage").dxTextBox("instance");
                divNewMessage.option("disabled", meetingState == msStopped || meetingState == msBooted);
            }

            if ($("#divSendMessage").data("dxButton")) {
                var divSendMessage = $("#divSendMessage").dxButton("instance");
                divSendMessage.option("disabled", meetingState == msStopped || meetingState == msBooted);
            }
        }

        function updateButtonState(btn_id, visible, disabled) {
            if ($(btn_id).data("dxButton")) {
                var btn = $(btn_id).dxButton("instance");
                btn.option("visible", visible);
                btn.option("disabled", disabled);
            }
        }

        function initMeetingMode() {
            var tree_tabs = $("#divTreeTabs").dxButtonGroup("instance");
            tree_tabs.option("disabled", meetingState !== msActive);

            //$("#imgProsConsSign").hide();
            //$(".cs-whiteboard").removeClass("cs-pros-cons");

            <%--if (isAnonymous) $("#lblModelSideCaption").text("<% = ParseString("%%Alternatives%%")%>");
            if (isAnonymous) $("#lblWhiteboardSideCaption").text("<% = ParseString("%%Alternatives%%")%>");--%>

            //if (!isAnonymous && !isRisk) {
            //    tree_tabs.option("selectedItemKeys", [meetingTreeMode]);
            //}

            switch (meetingBoardMode) {
                case mmAlts:
                    //if (!isRisk) $("#imgProsConsSign").show();                    
                    //$(".cs-whiteboard").addClass("cs-pros-cons");
                    break;
                case mmAlts:
                    break;
                default:
                    <%--if (isAnonymous) {
                        var cap = "<% = ParseString("%%Objectives%%")%>";
                        if (isRisk) {
                            if (meetingBoardMode == mmImpacts) { cap = "<% = ParseString("%%Objectives(i)%%")%>" } else { cap = "<% = ParseString("%%Objectives(l)%%")%>"; }

                        }
                        $("#lblModelSideCaption").text(cap);
                        $("#lblWhiteboardSideCaption").text(cap);
                    }--%>
                    break;
            }

            resizePage();

            var alt_board_nodes_count = 0;
            for (var i = 0; i < board_nodes.length; i++) {
                if (board_nodes[i]["isalt"]) alt_board_nodes_count += 1;
            }
            if (!isAnonymous && meetingBoardMode == mmAlts && !isCheckedAlternativesCopiedToWhiteboard && alt_board_nodes_count == 0 && alternatives.length > 0) {
                isCheckedAlternativesCopiedToWhiteboard = true;

                var s = "<p style='max-width: 650px; display: inline-block; margin-top: 0px;'>" + resString("msgCopyAlternativesToBoardNeeded") + "</p>";
                s += "<div id='divRadioCopy'></div>";
                dxConfirm(s, function () {
                    csAction(<%=Command.CopyAllAltsToBoard%>, "&mode=" + $("#divRadioCopy").dxRadioGroup("instance").option("value") + "&w=" + _DEFAULT_ITEM_WIDTH + "&h=" + _DEFAULT_ITEM_HEIGHT);
                });

                $("#divRadioCopy").dxRadioGroup({
                    items: [{"text" : resString("optCopyAlternativesToBoardTile"), "value" : <%=CInt(AntiguaCopyToBoardEventArgs.CopyModes.cmTile)%>}, {"text" : resString("optCopyAlternativesToBoardList"), "value" : <%=CInt(AntiguaCopyToBoardEventArgs.CopyModes.cmList)%>}],
                    layout: "vertical",
                    displayExpr: "text",
                    valueExpr: "value",
                    value: <%=CInt(AntiguaCopyToBoardEventArgs.CopyModes.cmTile)%>
                });
            }            
        }

        function initMeetingLockState() {
            $(".meeting-lock-icon").toggle(isMeetingLocked);
            if (isAnonymous) {
                setWhiteboardMode(isMeetingLocked ? "normal" : whiteboardMode);
                $(".drawing-ui").toggle(!isMeetingLocked); //  "&& false" only PM can draw
            }
        }

        //function checkAllAlternativesExistOnWhiteboard() {
        //    var alts_guids = [];
        //    for (var i = 0; i < board_nodes.length; i++) {                
        //        if (board_nodes[i]["isalt"]) alts_guids.push(board_nodes[i]["nodeguid"]);
        //    }
        //    for (var i = 0; i < alternatives.length; i++) {
        //        if ($.inArray(alternatives[i]["nodeguid"], alts_guids) == -1) return false;
        //    }

        //    return true;
        //}

        function onClearWhiteboard(mm) {        
            if (!canDoAction()) return;
            dxConfirm(resString("msgConfirmDeleteAllItems"), function () {
                var params = meetingBoardMode == mmAlts ? "&guids=" + getNodesGuidsListToDelete("all_alts") : "&guids=" + getNodesGuidsListToDelete("all_objs");
                csAction(<%=Command.DeleteNodes%>, params + "&whiteboard_items=true");
                clearWhiteboard();
            });            
        }

        function clearWhiteboard() {
            $("#divWhiteboard").find(".draggable-item").remove();
        }
        
        function onEditGoal() {
            if (!canDoAction()) return;

            dxDialog("<div id='tbGoalName' style='width: 400px;'></div>", function () { 
                var sNewName = $("#tbGoalName").dxTextBox("instance").option("value").trim();
                if (sNewName !== "") {
                    csAction(<%=Command.EditGoal%>, "&title=" + encodeURIComponent(sNewName)); 
                } else {
                    DevExpress.ui.notify("Goal name can't be empty", "error", OPT_CHAT_MESSAGE_HIGHLIGHT_TIMOUT);
                }
            }, ";", "Enter new goal name:");
            $("#tbGoalName").dxTextBox({
                value: goalName,
                placeholder: "Type new name here..."
            });

            dxTextBoxFocus('tbGoalName');
        }

        function onEditGoalInfodoc() {
            editInfodoc(sources[0].nodeguid, <% = Cint(reObjectType.Node) %>);
        }

        function getOffsetCoords(canvasId, x, y) {
            var point = { x: 0, y: 0 };
            var offset = $("#" + canvasId).offset();

            point.x = offset.left - $(window).scrollLeft();
            point.y = offset.top - $(window).scrollTop();
            point.x = x - point.x;
            point.y = y - point.y;

            return point;
        }

        function getWhiteboardNodeByGuid(id) {
            for (var i = 0; i < board_nodes.length; i++) {
                if (board_nodes[i].nodeguid == id) return board_nodes[i];
            }
            return;
        }

        function getWhiteboardElementByGuid(id) {
            var div = $("#divWhiteboard").find("[data-guid='" + id + "']");
            if ((div) && div.length > 0) {
                return div[0];
            } else {
                return null;
            }
        }

        function deleteWhiteboardNode(guid) {
            // delete from data source
            for (var j = 0; j < board_nodes.length; j++) {
                if (board_nodes[j].nodeguid == guid) {
                    board_nodes.splice(j, 1);
                    break;
                }
            }

            // delete from whiteboard
            var div = getWhiteboardElementByGuid(guid);
            if (div !== null) {
                $(div).toggle("clip"); // animation
                setTimeout(function () { 
                    div.parentNode.removeChild(div);
                }, 300);
            }
        }

        function canCreateOnWhiteboard(node) {
            return (meetingBoardMode == mmAlts && node.isalt) || (meetingBoardMode !== mmAlts && !node.isalt && node.location == location_hid);
        }

        function initWhiteboard() {
            clearWhiteboard();

            var board = document.getElementById("divWhiteboard");
            for (var i = 0; i < board_nodes.length; i++) {
                var node = board_nodes[i];
                if (canCreateOnWhiteboard(node)) {
                    draggableItemCreateFromNode(node);
                }
            }
            checkCSData();

            //$(board).selectable( {
            //    cancel: "input,textarea,button,select,option,.content-pro,.content-con,.draggable-item,.dx-resizable",
            //    filter: "div.draggable-item",
            //    distance: 10
            //});
        }

        function draggableItemCreateFromNode(node) {
            var div = draggableItemCreateOnWhiteboard(false, undefined);
            if ((div)) {
                div.setAttribute("data-x", node.attributes.x);
                div.setAttribute("data-y", node.attributes.x);
                div.setAttribute("data-width", node.attributes.w);
                div.setAttribute("data-height", node.attributes.h);
                div.setAttribute("data-id", node.id);
                div.setAttribute("data-guid", node.nodeguid);
                div.setAttribute("data-author", node.author);
                div.setAttribute("data-lastmodifiedby", node.lastmodifiedby);
                div.style.left = node.attributes.x + "px";
                div.style.top = node.attributes.y + "px";
                div.style.width = node.attributes.w + "px";
                div.style.minHeight = node.attributes.h + "px";
                div.style.backgroundColor = node.attributes.color;
                div.childNodes[0].innerHTML = "<span class='draggable-item-content-text'>" + node.name + "</span>";
                div.title = div.childNodes[0].innerText;
                div.childNodes[0].style.overflow = "hidden";
                //div.title = node.author;                 

                // item toolbar
                var divToolbar = document.createElement("div");
                node.divToolbar = divToolbar;
                divToolbar.className = "draggable-item-toolbar-div";
                //divToolbar.style.pointerEvents = "none";
                divToolbar.style.backgroundColor = "transparent";
                divToolbar.style.position = "absolute";
                //divToolbar.style.top = "1px";
                divToolbar.style.width = "auto";
                divToolbar.style.bottom = "-25px";
                divToolbar.style.left = "50%";
                divToolbar.style.display = is_mobile ? "" : "none"; //node.attributes.w <= 200 || node.attributes.h <= 50 ? "none" : "";
                div.appendChild(divToolbar);

                $(divToolbar).dxToolbar({
                    items: div.toolbarItems,
                    //height: "10px",
                    width: "auto",
                    onContentReady: function (e) {
                        e.element[0].childNodes[0].style.left = "-50%";
                        e.element[0].childNodes[0].style.position = "relative";
                    }
                });

                $(divToolbar).find(".dx-toolbar .dx-toolbar-item").css("padding", "0px 3px 0px 0px");
                $(divToolbar).find(".dx-button-has-icon .dx-button-content").css("padding", "2px");

                $(div).mouseenter(function (e) {
                    //if (!is_mobile && !is_editing && this.getAttribute("data-width") * 1 > _MIN_ITEM_WIDTH * 1.5 && this.getAttribute("data-height") * 1 > (_MIN_ITEM_HEIGHT * 1.5) - 3) {
                    if (!is_mobile && !is_editing) { //&& e.originalEvent.x < $(this).width() && e.originalEvent.y < $(this).height()) {
                        $(this).find(".draggable-item-toolbar-div").show().css("z-index", lastZIndex);
                        lastZIndex += 1;
                        e.stopPropagation();
                        e.preventDefault();
                    }
                }).mouseleave(function (e) {
                    if (!is_mobile) $(this).find(".draggable-item-toolbar-div").hide();
                });
                                    
                // init infopdoc tooltip
                // D6402: if (!isAnonymous && node.hasinfodoc) {
                //if (node.hasinfodoc) {          
                    var divTooltip = document.createElement("div");
                    divTooltip.className = "draggable-item-tooltip-div";
                    div.appendChild(divTooltip);

                    $(divTooltip).dxTooltip({
                        closeOnOutsideClick: false,
                        //container: "#divWhiteboardContainer", //div,
                        target: div.childNodes[0],
                        showEvent: { 
                            delay: 0,
                            name: "" 
                        },
                        //hideEvent: "blur,click,keydown",
                        hideEvent: {
                            delay: 0,
                            name: ""
                        },
                        //position: {
                        //    at: "top",
                        //    my: "bottom",
                        //    collision: "flip flipfit",
                        //    offset: { y : -10 }
                        //},
                        width: 350,
                        height: 200,
                        contentTemplate: function(data) {
                            var is_alt = meetingBoardMode == mmAlts;
                            var src = replaceString('%%%%', (is_alt ? <% =CInt(reObjectType.AntiguaInfodoc) %> : <% =CInt(reObjectType.AntiguaInfodoc) %>), '<% =PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}=%%id%%&guid=%%guid%%", "%%%%", _PARAM_ID)) %><% =GetTempThemeURI(True) %>&r=' + Math.round(10000*Math.random()));
                            var nodeid = this._$element.parent()[0].getAttribute("data-id"); //this._$container[0].getAttribute("data-id");
                            var nodeguid = this._$element.parent()[0].getAttribute("data-guid"); //this._$container[0].getAttribute("data-guid");
                            src = replaceString('%%id%%', nodeid, src);
                            src = replaceString('%%guid%%', nodeguid, src);
                            data.html("<iframe frameborder='0' allowtransparency='true' style='width: 345px; height: 100%; border: 0; background: transparent;' src=''></iframe>");
                            var f = data.find("iframe");
                            setTimeout(function () { 
                                if (f.length) {
                                    f[0].parentElement.style.padding = "1px";
                                    f[0].parentElement.style.height = "100%";
                                    initFrameLoader(f[0], "/Images/loader.gif", "48px");
                                }
                            }, 30);
                            setTimeout(function () { 
                                if (f.length) f[0].src = src; 
                            }, 60);
                        }
                    });
                //}

                // Infodoc icon
                var iicon = document.createElement("i");
                iicon.className = "fas fa-info-circle draggable-item-infodoc-icon " + (!node.hasinfodoc? "toolbar-label-disabled" : "ec-icon");
                iicon.style.cursor = "pointer";
                iicon.style.position = "absolute";
                iicon.style.left = "3px";
                iicon.style.top = "4px";
                iicon.title = "";
                iicon.setAttribute("data-guid", node.nodeguid);
                iicon.onclick = function (e) {
                    var tt = $(divTooltip).dxTooltip("instance");
                    var guid = this.getAttribute("data-guid");
                    //var node = getWhiteboardNodeByGuid(guid);
                    //if ((node)) {
                        if (node.hasinfodoc) {
                            if ((tt)) tt.toggle(!tt.option("visible"));
                        } else {
                            if (!canDoAction()) return;
                            editDraggableItemInfodoc(div);
                        }
                    //}
                };                
                div.appendChild(iicon);

                // Color marker
                if (isColorCoding) div.appendChild(getBoxColorMarker(node.author, resString("lblCreatorName") + ": " + node.author + "<br>Last modified by: " + node.lastmodifiedby));

                // Pros and Cons pane
                if (node.isalt) {
                    div.appendChild(getBoxProsConsPane(node));
                    $(node.itemProsConsDiv).mouseenter(function (e) {
                        if (!is_mobile && !is_editing) {
                            $(this).find(".draggable-item-toolbar-div").hide();
                            e.stopPropagation();
                            e.preventDefault();
                        }
                    });
                }

                // Resizing
                $(div).dxResizable({
                    handles: 'right bottom',
                    width: node.attributes.w,
                    minWidth: _MIN_ITEM_WIDTH,
                    maxWidth: _MAX_ITEM_SIZE,
                    //height: node.attributes.h,
                    minHeight: _MIN_ITEM_HEIGHT,
                    maxHeight: _MAX_ITEM_SIZE,
                    onResizeStart: function (e) {
                        e.event.stopPropagation();
                        e.event.preventDefault();
                        div.style.zIndex = lastZIndex;
                        lastZIndex += 1;
                        e.element.height(Math.max(e.element.height(), _MIN_ITEM_HEIGHT) + "px");
                        e.element[0].style.minHeight = "";
                        //e.element.removeClass("is_resizing_now");
                        //e.element.addClass("is_resizing_now");
                    },
                    onResize: function (e) {
                        node.attributes.w = Math.round(e.width);
                        node.attributes.h = Math.round(e.height);
                        resizeBoxProsConsPane(node);
                    },
                    onResizeEnd: function (e) {
                        //e.element.removeClass("is_resizing_now");
                        e.width = Math.round(node.attributes.w);
                        e.height = Math.round(node.attributes.h);
                        var orig_width = e.element[0].getAttribute("data-width") * 1;
                        var orig_height = e.element[0].getAttribute("data-height") * 1;
                        var itemguid = e.element[0].getAttribute("data-guid");
                        if ((orig_width !== e.width) || (orig_height !== e.height)) {
                            e.element[0].setAttribute("data-width", e.width);
                            e.element[0].setAttribute("data-height", e.height);
                            if (typeof itemguid !== "undefined") {
                                var params = "&item_guid=" + itemguid + "&width=" + e.width + "&height=" + e.height;
                                csAction(<% = Command.ResizeItem %>, params);
                            }
                        }
                    }
                });
            }
            return div;
        }

        function checkCSData() {
            if (isAnonymous) return;
            if (has_csdata !== board_nodes.length > 0) {
                menu_setOption(menu_option_csdata, board_nodes.length > 0);
            }
        }

        function initWhiteboardContextMenu() {
            $("#divWhiteboardMenu").dxContextMenu({
                items: [
                    <% If App.isAuthorized Then %>
                    { text: "Clear All", icon: "fas fa-trash-alt", onClick: function () { onClearWhiteboard(); }, disabled: false },
                    <% End If %>
                    { text: "Export", icon: "far fa-save", disabled: true },
                    { text: "Refresh", icon: "fas fa-sync", onClick: function () { reloadPage(); } },
                ],
                onShowing: function (e) { if (!canDoAction()) e.cancel = true; },
                target: "#divWhiteboard"
            });
        }
    
        function initPolling() {
            showLoadingIcon();
            callAjax("<%=_PARAM_ACTION %>=poll&prjid=<%=PRJ.ID%>&polling_user_id=" + cmdOwner + "&is_pm=" + <%=If(App.isAuthorized AndAlso App.CanUserBePM(App.ActiveWorkgroup, App.ActiveUser.UserID, App.ActiveProject, False, True), 1, 0)%> + "&timestamp=" + timeStamp + "&participant_count=" + usersList.length, onPollCallback, ajaxMethod, true, "StructuringService.aspx");
            setTimeout(function () { hideBackgroundMessage(); }, 300);
        }

        function onPollCallback(data) {
            var res = JSON.parse(data);
            if (res) {
                if (res.Result == _res_Success) {
                    timeStamp = res.Tag + "";

                    var AnonymousActions = "";

                    if (res.Data !== "poll_completed") {
                        var commands = JSON.parse(res.Data);
                        for (var k = 0; k < commands.length; k ++) {
                            var args = commands[k];
                            if (args.isAnonymousAction && args.CmdOwner !== meetingOwner && args.CmdCode !== <% = CInt(Command.PollTimeStamp)%>) {
                                args.isAnonymousAction = false;
                                AnonymousActions += (AnonymousActions == "" ? "" : "\t") + JSON.stringify(args);
                            } else {
                                
                                //update timestamp for each user
                                var u = getUserById(args.CmdOwner);
                                if ((u)) u.ts = args.DT * 1;

                                if (args.CmdCode == <% = CInt(Command.PollTimeStamp)%>) {
                                    continue;
                                }

                                if (meetingOwner != args.MeetingOwner) {
                                    meetingOwner = args.MeetingOwner;
                                    initMeetingState();
                                    updateToolbar();
                                    initMeetingMode();
                                }                                

                                switch (args.CmdCode) {
                                    case <% = Command.Connect %>:
                                        var usr = getUserById(args.CmdOwner);
                                        if (!(usr)) {
                                            usersList.push({"id": args.CmdOwner, "name" : args.Token.mUserName, "email" : args.Token.mEmail, "can_be_pm" : args.CanBePM, "can_change_pm" : args.CanChangePM, "is_pm" : args.IsPM, ts : args.DT * 1 });
                                            repaintUserList();
                                        } else {
                                            //alert user reconnected
                                        }
                                        highlightUser(args.CmdOwner, true, false);
                                        break;
                                    case <% = Command.DisconnectUser %>:
                                    <%--case <% = Command.DisconnectUserWhenMeetingStopped %>:--%>
                                        var usr = getUserById(args.CmdOwner);
                                        if ((usr)) {
                                            highlightUser(args.CmdOwner, false, true);
                                            deleteUser(args.CmdOwner);
                                        }
                                        break;
                                    case <%=Command.RefreshUsersList%>:
                                        if (args.CmdOwner != cmdOwner) {
                                            usersList = JSON.parse(args.Tag);
                                        }
                                        break;
                                    case <%=Command.SetMeetingState%>:
                                        meetingState = args.State;
                                        initMeetingState();
                                        updateToolbar();
                                        initMeetingMode();
                                        if (meetingState == msStopped && !isAnonymous && (typeof toggleSideBarMenu == "function")) { 
                                            toggleSideBarMenu(true, 1, true); 
                                        }
                                        if (isAnonymous) {
                                            if (meetingState == msOwnerDisconnected) {
                                                WAS_OWNER_DISCONNECTED = true;
                                            }
                                            if (meetingState == msActive && WAS_OWNER_DISCONNECTED) {
                                                resetProjectAndReload();
                                            }
                                        }
                                        break;
                                    case <%=Command.SetMeetingMode%>:
                                        meetingTreeMode = args.TreeMode;
                                        meetingBoardMode = args.BoardMode;
                                        setMeetingMode();
                                        break;
                                    case <%=Command.CreateNewNode%>:                      
                                        var node = JSON.parse(jsEscape(args.Tag));
                                        if ((node) && (node.nodeguid)) {
                                            board_nodes.push(node);
                                            checkCSData();
                                    
                                            var oldIsDrag = isDragging;
                                            isDragging = false;

                                            var div = $("div").find("[data-temp-id-created='" + args.TmpID + "']");
                                            if ((div) && div.length > 0) {
                                                div.remove();
                                            }
                                            
                                            if (canCreateOnWhiteboard(node)) {
                                                draggableItemCreateFromNode(node);
                                                csHighlight(div);
                                            }

                                            isDragging = oldIsDrag;
                                        }
                                        break;
                                    case <% = Command.MoveNodesOnBoard %>:
                                        var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                        if ((node)) {
                                            node.attributes.x = args.Position.x;
                                            node.attributes.y = args.Position.y;
                                            if (args.CmdOwner != cmdOwner) {
                                                var div = getWhiteboardElementByGuid(args.NodeGuid);
                                                if (div !== null) {
                                                    div.style.zIndex = lastZIndex;
                                                    lastZIndex += 1;

                                                    performMoveNodeOnBoard(div, args.Position.x, args.Position.y);
                                                }
                                            }
                                        }
                                        break;
                                    case <%=Command.DeleteNodes%>:
                                        for (var i = 0; i < args.NodesGuids.length; i++) {
                                            if (args.IsWhiteboardItems) {
                                                deleteWhiteboardNode(args.NodesGuids[i]);
                                            } else {                                        
                                                // delete from alternatives list
                                                for (var t = 0; t < alternatives.length; t++) {
                                                    if (alternatives[t].nodeguid == args.NodesGuids[i]) {
                                                        alternatives.splice(t, 1);
                                                
                                                        //remove alternative list element
                                                        deleteTreeElementByGuid(args.NodesGuids[i]);
                                                
                                                        break;
                                                    }
                                                }                                                   

                                                // delete from hierarchy tree (sources/objectives)
                                                for (var j = 0; j < sources.length; j++) {
                                                    if (sources[j].nodeguid == args.NodesGuids[i]) {
                                                        sources.splice(j, 1);
                                                
                                                        // remove hierarchy element
                                                        deleteTreeElementByGuid(args.NodesGuids[i]);

                                                        break;
                                                    }
                                                }
                                                for (var j = 0; j < impacts.length; j++) {
                                                    if (impacts[j].nodeguid == args.NodesGuids[i]) {
                                                        impacts.splice(j, 1);
                                                
                                                        // remove hierarchy element
                                                        deleteTreeElementByGuid(args.NodesGuids[i]);

                                                        break;
                                                    }
                                                }
                                            }

                                            //todo clean-up children that have no parent (if non-covering node was deleted)
                                        }
                                        refreshProsConsList();
                                        checkCSData();
                                        break;
                                    case <%=Command.RenameItem%>:
                                        var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                        if ((node)) node.name = args.Title;

                                        node = getNodeByGuid(args.NodeGuid);
                                        if ((node)) node.name = args.Title;

                                        var div = getTreeElementByGuid(args.NodeGuid);
                                        if (div !== null) {
                                            div.childNodes[0].innerText = args.Title;
                                            div.title = div.childNodes[0].innerText;
                                            csHighlight(div);
                                        }

                                        div = getWhiteboardElementByGuid(args.NodeGuid);
                                        if (div !== null) {
                                            div.childNodes[0].innerHTML = "<span class='draggable-item-content-text'>" + args.Title + "</span>";
                                            div.title = div.childNodes[0].innerText;
                                            csHighlight(div);
                                        }
                                        break;
                                    case <%=Command.SetNodesColor%>:
                                        //for (var i = 0; i < args.NodesGuids.length; i++) {
                                            var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                            if ((node)) node.attributes.color = args.sColor;

                                            var div = getWhiteboardElementByGuid(args.NodeGuid);
                                            if (div !== null) {
                                                div.style.backgroundColor = args.sColor;
                                            }
                                        //}
                                            break;
                                    case <%=Command.ResizeItem%>:
                                        var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                        if ((node)) {
                                            node.attributes.w = args.Width;
                                            node.attributes.h = args.Height;
                                        }

                                        var div = getWhiteboardElementByGuid(args.NodeGuid);
                                        if (div !== null) {
                                            div.style.width = args.Width + "px";
                                            div.style.height = "";
                                            div.style.height = args.Height + "px";
                                            div.setAttribute("data-width", args.Width);
                                            div.setAttribute("data-height", args.Height);
                                        }

                                        resizeBoxProsConsPane(node);
                                        break;
                                    case <%=Command.SendToAlternatives%>:
                                    case <%=Command.SendToObjectives %>:
                                        var node = getWhiteboardNodeByGuid(args.SourceNodeGuid);
                                        if ((node)) { node.isalt = !node.isalt };

                                        var div = getWhiteboardElementByGuid(args.SourceNodeGuid);
                                        if (div !== null) {
                                            $(div).toggle("clip");
                                            setTimeout(function () { div.parentNode.removeChild(div); }, 500);
                                        }
                                        refreshProsConsList();
                                        checkCSData();
                                        break;
                                    case <%=Command.AddNode%>:
                                        var new_node = JSON.parse(jsEscape(args.Tag));
                                        if ((new_node) && (new_node.nodeguid)) {
                                            if (new_node["isalt"]) {
                                                alternatives.push(new_node);
                                            } else {
                                                if (new_node["hid"] == 0) sources.push(new_node);
                                                if (new_node["hid"] == 2) impacts.push(new_node);
                                            }
                                            
                                            if (meetingTreeMode == mmAlts && new_node["isalt"] || meetingTreeMode == mmSources && !new_node["isalt"] && new_node["hid"] == 0 || meetingTreeMode == mmImpacts && !new_node["isalt"] && new_node["hid"] == 2) {
                                                var ulTree = document.getElementById("ulTree");
                                                var new_element = draggableItemCreateOnList(new_node, ulTree, true, true, "draggable-list-item", false, false, new_node["level"]);
                                           
                                                if ((new_element)) {
                                                    if (args.ParentNodeID + "" == "<%=Guid.Empty.ToString%>" || args.ParentNodeID + "" == "<%=PM.ActiveObjectives.Nodes(0).NodeGuidID.ToString%>") {
                                                        insertElementAt(ulTree, new_element, <%=Integer.MaxValue%>);
                                                    } else {
                                                        if (new_node["isalt"]) {
                                                            var parent_node = getNodeByGuid(args.ParentNodeID);
                                                            var parent_node_idx = alternatives.indexOf(parent_node);
                                                            insertElementAt(ulTree, new_element, parent_node_idx + 1);
                                                        } else {
                                                            var parent_node = getNodeByGuid(args.ParentNodeID);
                                                            var nodes = parent_node.hid == 0 ? sources : impacts;
                                                            var parent_node_idx = nodes.indexOf(parent_node) + getNodeChildren(nodes, args.ParentNodeID).length;
                                                            insertElementAt(ulTree, new_element, parent_node_idx);
                                                        }
                                                    }
                                                    csHighlight(new_element);
                                                }
                                            }
                                        }
                                        break;
                                    case <%=Command.MoveFromBoardToTree%>:
                                        dropIndicatorClear();
                                    
                                        var nodes = JSON.parse(jsEscape(args.Tag));
                                        if (nodes[0].hid == 0) sources = nodes;
                                        if (nodes[0].hid == 2) impacts = nodes;
                                        if (nodes[0].hid == 0 && meetingTreeMode == mmSources || nodes[0].hid == 2 && meetingTreeMode == mmImpacts) {
                                            initTree();
                                        }

                                        for (var i = 0; i < args.NodesGuids.length; i++) {
                                            deleteWhiteboardNode(args.NodesGuids[i]);
                                        }

                                        checkCSData();
                                        break;
                                    case <%=Command.MoveAltToList%>:
                                        dropIndicatorClear();

                                        var source_node = JSON.parse(jsEscape(args.Tag));

                                        if ((source_node)) {
                                            var target_idx = args.Position;
                                       
                                            if (target_idx > -1) {
                                                alternatives.splice(target_idx, 0, source_node);
                                            } else {
                                                alternatives.push(source_node);
                                            }

                                            var ulTree = document.getElementById("ulTree");
                                            var source_element = draggableItemCreateOnList(source_node, ulTree, true, true, "draggable-list-item", false, false);
                                           
                                            if ((source_element)) {
                                                insertElementAt(ulTree, source_element, target_idx > -1 ? target_idx : <%=Integer.MaxValue%>);
                                                csHighlight(source_element);
                                            }

                                            //todo highlight linked alts
                                            refreshProsConsList();
                                            checkCSData();
                                        }
                                        break;
                                    case <%=Command.CopyAltToBoard%>:
                                        var node = JSON.parse(jsEscape(args.Tag));
                                        if ((node) && (node.nodeguid)) {
                                            board_nodes.push(node);
                                            draggableItemCreateFromNode(node);
                                            refreshProsConsList();
                                            checkCSData();
                                        }
                                        break;
                                    case <%=Command.MoveFromTreeToBoard%>:
                                        var callback_data = JSON.parse(jsEscape(args.Tag));
                                        board_nodes = callback_data[0];
                                        initWhiteboard();
                                        
                                        var nodes = callback_data[1];
                                        if (nodes[0].hid == 0) sources = nodes;
                                        if (nodes[0].hid == 2) impacts = nodes;
                                        
                                        initTree();
                                        checkCSData();
                                        break;
                                    case <%=Command.ReorderInTree%>:
                                        var nodes = JSON.parse(jsEscape(args.Tag));
                                        if (nodes[0].hid == 0) sources = nodes;
                                        if (nodes[0].hid == 2) impacts = nodes;

                                        if (meetingTreeMode == mmSources && nodes[0].hid == 0 ||meetingTreeMode == mmImpacts && nodes[0].hid == 2) {
                                            initTree();
                                        }
                                        break;
                                    case <%=Command.ReorderInAlts%>:
                                        dropIndicatorClear();

                                        var nodes = meetingTreeMode == mmAlts ? alternatives : [];

                                        var source_node = getNodeByGuid(args.SourceNodeGuid);
                                        var target_node = getNodeByGuid(args.DestNodeGuid);

                                        if ((source_node) && (target_node)) {
                                            var source_idx = nodes.indexOf(source_node);
                                            var target_idx = -1;
                                        
                                            source_node = nodes.splice(source_idx, 1)[0]; // delete source node

                                            switch (args.Action) {
                                                case nmaBeforeNode:
                                                    target_idx = nodes.indexOf(target_node);
                                                    if (target_idx < 0) target_idx = 0;
                                                    break;
                                                case nmaAfterNode:
                                                    target_idx = nodes.indexOf(target_node) + 1;
                                                    if (target_idx > nodes.length + 1) target_idx = nodes.length;
                                                    break;
                                            }
                                        
                                            if (source_idx > -1 && target_idx > -1) {
                                                nodes.splice(target_idx, 0, source_node);

                                                var ulTree = document.getElementById("ulTree");
                                                var source_element = getTreeElementByGuid(args.SourceNodeGuid);
                                                var target_element = getTreeElementByGuid(args.DestNodeGuid);
                                            
                                                if ((source_element) && (target_element)) {
                                                    ulTree.removeChild(source_element);
                                                    insertElementAt(ulTree, source_element, target_idx);
                                                }
                                            }
                                        }
                                        break;
                                    case <%=Command.Setting%>:
                                        if (args.Name == "color_coding") isColorCoding = str2bool(args.Value);
                                        if (args.Name == "synch_mode") {
                                            meetingSynchMode = args.Value * 1;
                                            setMeetingMode();
                                        }
                                        if (args.Name == "default_alt_title") _DEFAULT_ALT_TITLE = args.Value;
                                        if (args.Name == "default_obj_title") _DEFAULT_OBJ_TITLE = args.Value;
                                        if (args.Name == "default_source_title") _DEFAULT_SOURCE_TITLE = args.Value;
                                        if (args.Name == "default_alt_color") _DEFAULT_COLOR_ALT = args.Value;
                                        if (args.Name == "default_obj_color") _DEFAULT_COLOR_OBJ = args.Value;
                                        if (args.Name == "default_source_color") _DEFAULT_COLOR_SOURCE = args.Value;
                                        if (args.Name == "item_size") {
                                            if (args.Value != "") _DEFAULT_ITEM_WIDTH = args.Value * 1;
                                            if (args.Tag != "") _DEFAULT_ITEM_HEIGHT = args.Tag * 1;
                                            for (var i = 0; i < board_nodes.length; i++) {
                                                board_nodes[i].attributes.w = _DEFAULT_ITEM_WIDTH;
                                                board_nodes[i].attributes.h = _DEFAULT_ITEM_HEIGHT;
                                            }
                                        }
                                        if (args.Name == "whiteboard_draw_arr") {
                                            var newDrawing = JSON.parse(args.Value);
                                            Array.prototype.push.apply(whiteboardDrawingData, newDrawing);
                                            //if (args.CmdOwner != cmdOwner) {
                                            drawArray(newDrawing);
                                            //}
                                        }
                                        if (args.Name == "drawing_stay") {
                                            drawingLifeTime = args.Value * 1;
                                            whiteboardDrawingData = [];

                                            if (isAnonymous) {                                                
                                                $("#btn_clear_drawing_anon").dxButton("instance").option("visible", drawingLifeTime == <% = Integer.MaxValue%>);
                                            } else {
                                                drawingButtonItems[0].visible = drawingLifeTime == <% = Integer.MaxValue%>;                                                
                                                
                                                var dd_btn = $("#btn_clear_drawing_pm").dxDropDownButton("instance");
                                                dd_btn.option("items", drawingButtonItems);
                                            }

                                            clearCanvas();
                                            updateToolbar();
                                        }
                                        if (args.Name == "whiteboard_drawing_clear_my") {            
                                            whiteboardDrawingData = JSON.parse(args.Value);
                                            clearCanvas();
                                            drawArray(whiteboardDrawingData);
                                        }
                                        if (args.Name == "whiteboard_drawing_clear_all") {
                                            whiteboardDrawingData = [];
                                            clearCanvas();
                                        }
                                        //if (args.Name == "save_user_list") {
                                        //    usersList = JSON.parse(args.Value);
                                        //    repaintUserList();
                                        //}
                                        initWhiteboard();
                                        break;
                                    case <%=Command.ChatMessage%>:
                                        postBroadcastMessage(args.UserName, args.TimeStamp, args.Text);
                                        if (document.getElementById("divOnlineUsers").style.display == "none") { csHighlight( $("#btn_messages") , 10000); }
                                        break;
                                    case <%=Command.SwitchProsCons%>:
                                        if (args.Mode == "all") {
                                            SwitchProsConsForAll(args.Show);
                                        }
                                        if (args.Mode == "single") {
                                            var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                            if ((node)) {
                                                node.is_pc_open = args.Show ? 1 : 0;
                                                ToggleProsCons(node);
                                            }
                                        }
                                        if (args.Mode == "oneatatime") {
                                            if (args.CmdOwner != cmdOwner) {
                                                SwitchProsConsForAll(false);
                                                SwitchProsCons(args.NodeGuid, true);
                                            }
                                        }
                                        break;
                                    case <%=Command.CreateNewItemToProsCons%>:
                                        if ((args.StringList) && args.StringList.length > 0) {
                                            //todo paste
                                        } else {
                                            var new_items = JSON.parse(jsEscape(args.Tag));
                                            var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                            if ((node)) {
                                                for (var i = 0; i < new_items.length; i++) {
                                                    var new_item = new_items[i];
                                                    if (args.IsPro) { node["pros"].push(new_item); } else { node["cons"].push(new_item); }
                                                }

                                                // add to main pros and cons list
                                                refreshProsConsList();

                                                // add to whiteboard item pros and cons list
                                                var element = getWhiteboardElementByGuid(args.NodeGuid);
                                                var pane = $(element).find(".item_pros_cons_pane")[0];
                                                if ((element) && (pane)) {
                                                    for (var i = 0; i < new_items.length; i++) {
                                                        var new_item = new_items[i];
                                                        var div = draggableItemCreateOnList(new_item, pane, false, true, "draggable-list-item-small", true, args.IsPro, 0, node);
                                                        if ((div)) csHighlight(div);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case <%=Command.DeleteProOrCon%>:
                                        if (args.Source.length > 0 || args.DoForAll) {
                                            var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                            var element = getWhiteboardElementByGuid(args.NodeGuid);
                                            var pane = $(element).find(".item_pros_cons_pane")[0];
                                            if ((node) && (element)) {
                                                
                                                if (args.DoForAll) {
                                                    args.Source = [];
                                                    var list = args.IsPro ? node.pros : node.cons;
                                                    for (var j = 0; j < list.length; j++) {
                                                        args.Source.push(list[j]["nodeguid"]);
                                                    }
                                                }


                                                for (var i = 0; i < args.Source.length; i++) {
                                                    var item_guid = args.Source[i];
                                                    //remove from node
                                                    for (var j = 0; j < node.pros.length; j++) {
                                                        if (item_guid == node.pros[j]["nodeguid"]) {
                                                            node.pros.splice(j, 1);
                                                            break;
                                                        }
                                                    }

                                                    for (var j = 0; j < node.cons.length; j++) {
                                                        if (item_guid == node.cons[j]["nodeguid"]) {
                                                            node.cons.splice(j, 1);
                                                            break;
                                                        }
                                                    }
                                                    //remove from pane
                                                    if ((pane)) {
                                                        var item_element = $(pane).find("[data-guid='" + item_guid + "']");
                                                        item_element.toggle("fade");
                                                        setTimeout(function () { item_element.remove(); }, 400);
                                                    }
                                                }
                                            }
                                            refreshProsConsList();
                                        }
                                        break;
                                    case <%=Command.RenameProsConsItem%>:
                                        if (args.Source.length > 0) {
                                            var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                            var element = getWhiteboardElementByGuid(args.NodeGuid);
                                            var pane = $(element).find(".item_pros_cons_pane")[0];
                                            if ((node) && (element)) {
                                                for (var i = 0; i < args.Source.length; i++) {
                                                    var item_guid = args.Source[i];
                                                
                                                    //rename in node
                                                    for (var j = 0; j < node.pros.length; j++) {
                                                        if (item_guid == node.pros[j]["nodeguid"]) {
                                                            node.pros[j]["name"] = args.NewItemTitle;
                                                            break;
                                                        }
                                                    }

                                                    for (var j = 0; j < node.cons.length; j++) {
                                                        if (item_guid == node.cons[j]["nodeguid"]) {
                                                            node.cons[j]["name"] = args.NewItemTitle;
                                                            break;
                                                        }
                                                    }

                                                    //rename in pane
                                                    if ((pane)) {
                                                        var item_element = $(pane).find("[data-guid='" + item_guid + "']");
                                                        item_element[0].childNodes[0].innerText = args.NewItemTitle;
                                                        item_element.prop("title", item_element[0].childNodes[0].innerText);
                                                        csHighlight(item_element);
                                                    }
                                                }
                                            }
                                            refreshProsConsList();
                                        }
                                        break;
                                    case <%=Command.CopyFromProsConsToTree%>:
                                        dropIndicatorClear();
                                        
                                        var node = getWhiteboardNodeByGuid(args.NodeGuid);
                                        var element = getWhiteboardElementByGuid(args.NodeGuid);
                                        var pane = $(element).find(".item_pros_cons_pane")[0];

                                        for (var i = 0; i < args.Source.length; i++) {
                                            var item_guid = args.Source[i];
                                                
                                            //rename in node
                                            for (var j = 0; j < node.pros.length; j++) {
                                                if (item_guid == node.pros[j]["nodeguid"]) {
                                                    node.pros[j]["pc_moved"] = true;
                                                    var el = $(element).find(".draggable-list-item-small[data-guid='" + item_guid + "']");
                                                    if (el.length > 0) {
                                                        el[0].childNodes[1].style.display = "";
                                                    }
                                                    break;
                                                }
                                            }

                                            for (var j = 0; j < node.cons.length; j++) {
                                                if (item_guid == node.cons[j]["nodeguid"]) {
                                                    node.cons[j]["pc_moved"] = true;
                                                    var el = $(element).find(".draggable-list-item-small[data-guid='" + item_guid + "']");
                                                    if (el.length > 0) {
                                                        el[0].childNodes[1].style.display = "";
                                                    }
                                                    break;
                                                }
                                            }                                            
                                        }

                                        var nodes = JSON.parse(jsEscape(args.Tag));
                                        if (nodes[0].hid == 0) sources = nodes;
                                        if (nodes[0].hid == 2) impacts = nodes;

                                        initTree();
                                        refreshProsConsList();

                                        // highlight
                                        for (var j = 0; j < args.Result.length; j++) {
                                            var div = getTreeElementByGuid(args.Result[j]);
                                            if ((div)) { 
                                                //div.scrollIntoView();
                                                $("#ulTree").scrollTop($(div).offset().top - $("#ulTree").offset().top + $("#ulTree").scrollTop());
                                                csHighlight(div);
                                            }
                                        }
                                        break;
                                    case <%=Command.SetMeetingLock%>:
                                        isMeetingLocked = args.IsMeetingLocked;
                                        DevExpress.ui.notify(isMeetingLocked ? "The meeting was made read-only by the meeting organizer" : "The meeting is now available for editing", isMeetingLocked ? "warning" : "success", OPT_CHAT_MESSAGE_HIGHLIGHT_TIMOUT);
                                    
                                        initMeetingLockState();
                                    
                                        if (meetingOwner != cmdOwner) {
                                            //todo lock the whiteboard and tree-view/pros-cons
                                            isReadOnly = isMeetingLocked;
                                        } else {
                                            updateToolbar();
                                        }
                                        break;
                                    case <%=Command.CopyAllAltsToBoard%>:
                                        board_nodes = JSON.parse(jsEscape(args.Tag));
                                        initWhiteboard();
                                        break;
                                    case <%=Command.EditGoal%>:
                                        // edit goal
                                        goalName = args.Title;
                                        $("#lblGoalTitle").text(goalName);
                                        csHighlight($("#lblGoalTitle"));

                                        // rename tree item
                                        //todo: fix for Riskion
                                        var objs = meetingBoardMode == mmSources ? sources : impacts;
                                        node = getNodeByGuid(objs[0].nodeguid);
                                        if ((node)) node.name = args.Title;

                                        var div = getTreeElementByGuid(objs[0].nodeguid);
                                        if (div !== null) {
                                            div.childNodes[0].innerText = args.Title;
                                            csHighlight(div);
                                        }
                                }
                                debug("Action " + args.CmdCode + " performed");
                            }
                        }
                    } else {
                        debug("Poll completed.");
                    }
                    
                    // check offline users
                    if (res.Data !== "poll_completed" && meetingOwner == cmdOwner) {
                        var cur_ts = res.Tag * 1;
                        var check_ts = cur_ts - <% = CLng(TimeSpan.TicksPerSecond * (StructuringServicePage.LongPollMaxTime + 20))%>; // if user's time stamp is older than LongPollMaxTime + 20 sec. then disconnect him
                        var orig_usersList_len = usersList.length;
                        var offline_user_ids = "";
                        var i = 0;
                        while (i < usersList.length) {
                            if (usersList[i].id !== cmdOwner) {
                                if ((typeof usersList[i].ts == "undefined" || usersList[i].ts < check_ts) && usersList[i].ts !== 0) {
                                    offline_user_ids += (offline_user_ids == "", "", ",") + usersList[i].id;
                                    highlightUser(usersList[i].id, false, true);
                                    deleteUser(usersList[i].id);
                                    i -= 1;
                                }
                            }
                            i += 1;
                        }
                        if (usersList.length !== orig_usersList_len) {
                            callAjax("<%=_PARAM_ACTION %>=do&cs_action=" + <% = Command.RefreshUsersList%> + "&offline_users=" + offline_user_ids, null, ajaxMethod, true, "", false);
                        }
                    }
                
                    if (res.Message != "") {
                        DevExpress.ui.dialog.alert(res.Message);
                    }

                    setTimeout(function () { initPolling(); }, 50); // 60 * 1 seconds passed or update received, restart polling
                }

                if (AnonymousActions != "") {
                    callAjax("<%=_PARAM_ACTION %>=do_anonymous_actions&data=" + encodeURIComponent(AnonymousActions), onCSActionCallback, ajaxMethod, true);
                }
            }
        }

        function canDoAction() {
            return !(isReadOnly || (isMeetingLocked && cmdOwner != meetingOwner));
        }

        function csAction(sAction, params, callback) {
            showLoadingIcon();
            callAjax("<%=_PARAM_ACTION %>=do&cs_action=" + sAction + params, typeof callback == "function" ? callback : onCSActionCallback, ajaxMethod, false, "", true, please_wait_delay);
        }

        function onCSActionCallback(data) {
            //todo
        }

        function csHighlight(element, duration) {
            duration = duration || 800;
            $(element).effect("highlight", {}, duration);
        }

        var mousePressed = false;
        var lastX = 0, lastY = 0;
        var drawingArr = [];
        var drawingMoveEpsilon = 12;
        var drawingLifeTime = <% = PM.Parameters.CS_DrawingLifeTime%>;
        var curDrawingColor = getUserColorByName(UserName + " (" + UserEmail + ")");
        var ctx;

        var drawingButtonItems = [{
                        key: "my",
                        icon: "fas fa-user-slash",
                        text: "Clear my drawing",
                        hint: "Clear my drawing",
                        visible: drawingLifeTime == <% = Integer.MaxValue%>,
                        disabled: false
                    }, {
                        key: "all",
                        icon: "fas fa-trash",
                        text: "Clear the drawing for all participants",
                        hint: "Clear the drawing for all participants",
                        visible: true,
                        disabled: false
                    }/*, {
                        key: "color",
                        text: "Color",
                        hint: "Color",
                        template: function () {
                            return $("<div></div>").dxColorBox({
                                value: curDrawingColor,
                                onValueChanged: function (e) {
                                    curDrawingColor = e.value;
                                }, //width: 66
                            });
                        }
                    }*/];

        function initDrawing() {
            $("#divWhiteboardMode").dxButtonGroup({
                items: [{
                    key: "normal",
                    //icon: "fas fa-hand-pointer",
                    icon: "fas fa-italic",
                    hint: "Normal Mode",
                    text: "Normal",
                    template: function() {
                        return $('<i class="dx-icon fas fa-italic"></i><span class="dx-button-text panel-nowide700">Normal</span>');
                    },
                    visible: true,
                    disabled: false
                }, {
                    key: "drawing",
                    icon: "fas fa-paint-brush",
                    hint: "Drawing Mode",
                    text: "Drawing",
                    template: function() {
                        return $('<i class="dx-icon fas fa-paint-brush"></i><span class="dx-button-text panel-nowide700">Drawing</span>');
                    },
                    visible: true,
                    disabled: false
                }],
                focusStateEnabled: false,
                keyExpr: "key",
                key: "normal",
                selectedItemKeys: [whiteboardMode],
                selectionMode: "single",
                onSelectionChanged: function (e) {
                    var selectedItemKeys = e.component.option("selectedItemKeys");
                    if (selectedItemKeys.length > 0) {
                        var value = selectedItemKeys[0];
                        if (whiteboardMode != value) {
                            <%--csAction(<%=Command.Setting%>, "&name=whiteboard_drawing_mode&value=" + value);--%>
                            localStorage.setItem("CS_WbMode<%=App.Antigua_MeetingID%>", value);
                            whiteboardMode = value;
                            setWhiteboardMode(value); 
                        }
                    }
                }
            });
            
            if (isAnonymous) {
                $("#divWhiteboardDrawingClear").dxButton({
                    elementAttr: {id: 'btn_clear_drawing_anon'},
                    icon: "fas fa-eraser",
                    hint: "Clear My Drawing",
                    visible: drawingLifeTime == <% = Integer.MaxValue%>,
                    disabled: false,
                    onClick: function (e) {
                        dxConfirm("<% = ResString("msgSureClearDrawing") %>", clearMyDrawing);
                    }
                });
            } else {
                $("#divWhiteboardDrawingClear").dxDropDownButton({
                    elementAttr: {id: 'btn_clear_drawing_pm'},
                    displayExpr: "text",                    
                    dropDownOptions: { position : { of : $("#divWhiteboardDrawingClear"), offset: '-100 40' }, width : 250 },
                    icon: "fas fa-eraser",
                    items: drawingButtonItems,
                    hint: "Clear Drawing",
                    visible: true,
                    disabled: false,
                    onItemClick: function (e) {
                        if (e.itemData.key == "my") dxConfirm("<% = ResString("msgSureClearDrawing") %>", clearMyDrawing);
                        if (e.itemData.key == "all") dxConfirm("<% = ResString("msgSureClearDrawingAll") %>", clearAllDrawing);
                    }
                });
            }

            //$("#divWhiteboardDrawingColor").dxButton({
            //    displayExpr: "text",                    
            //    dropDownOptions: { width : 150 },
            //    icon: "fas fa-palette",
            //    //template: function (data, element) {
            //    //    element.append($("<div></div>").dxColorBox({
            //    //        value: curDrawingColor,
            //    //        onValueChanged: function (e) {
            //    //            curDrawingColor = e.value;
            //    //        }
            //    //    }));
            //    //},
            //    onClick: function(e) {

            //    }

            //});

            $("#divWhiteboardDrawingColor").dxColorBox({
                value: curDrawingColor,
                onValueChanged: function (e) {
                    curDrawingColor = e.value;
                },
                width: 66
            });

            $("#divWhiteboardDrawingHide").dxButtonGroup({
                items: [{
                    key: "show",
                    icon: "fas fa-eye",
                    hint: "Show/Hide Drawing"
                }],
                keyExpr: "key",
                selectedItemKeys: "show",
                selectionMode: "multiple",
                onSelectionChanged: function (e) {
                    var selectedItemKeys = e.component.option("selectedItemKeys");
                    if (selectedItemKeys.length > 0) {
                        $('#divDrawingWhiteboard').show();
                        e.component.option("items")[0].icon = "fas fa-eye";
                    } else {
                        $('#divDrawingWhiteboard').hide();
                        e.component.option("items")[0].icon = "fas fa-eye-slash";
                    }
                    e.component.repaint();
                }
            });

            var canvas = $('#divDrawingWhiteboard');
            //canvas.css("top", $("#whiteboardToolbar").outerHeight() + "px");

            ctx = document.getElementById('divDrawingWhiteboard').getContext("2d");

            canvas.mousedown(function (e) {
                mousePressed = true;
                lastX = e.pageX - $(this).offset().left; lastY = e.pageY - $(this).offset().top;
                draw(lastX, lastY, false);
            });

            canvas.mousemove(function (e) {
                if (mousePressed) {
                    draw(e.pageX - $(this).offset().left, e.pageY - $(this).offset().top, true);
                }
            });

            canvas.mouseup(function (e) {
                if (mousePressed) {
                    pushDrawing();
                    mousePressed = false;
                }
            });

            canvas.mouseleave(function (e) {
                if (mousePressed) {
                    pushDrawing();
                    mousePressed = false;
                }
            });

            drawArray(whiteboardDrawingData);
            setWhiteboardMode(whiteboardMode);
        }

        function draw(x, y, isDown) {
            if (isDown && ((Math.abs(lastX - x) > drawingMoveEpsilon) || (Math.abs(lastY - y) > drawingMoveEpsilon))) {
                lastX = Math.round(lastX);
                lastY = Math.round(lastY);
                x = Math.round(x);
                y = Math.round(y);

                ctx.beginPath();
                ctx.strokeStyle = curDrawingColor;
                ctx.lineWidth = 3;
                ctx.lineJoin = "round";
                ctx.moveTo(lastX, lastY);
                ctx.lineTo(x, y);
                //ctx.closePath();
                ctx.stroke();
                drawingArr.push([cmdOwner, curDrawingColor, ctx.lineWidth, lastX, lastY, x, y]);
                lastX = x; lastY = y;
            }
        }

        function drawArray(arr, is_clear) {
            if (!arr.length) return;
            ctx.lineJoin = "round";
            var arr_length = arr.length;
            for (var i = 0; i < arr_length; i++) {
                var d = arr[i];
                if (d[1] !== "transparent") {
                    ctx.beginPath();
                    ctx.strokeStyle = d[1];
                    ctx.lineWidth = d[2];
                    ctx.moveTo(d[3], d[4]);
                    ctx.lineTo(d[5], d[6]);
                    //ctx.closePath();
                    ctx.stroke();
                }
            }
            if (drawingLifeTime < <% = Integer.MaxValue %> && (typeof is_clear == "undefined" || !is_clear)) {
                setTimeout(function () {
                    for (var i = 0; i < arr_length; i++) {
                        var d = arr[i];
                        d[1] = "transparent";
                    }
                    clearCanvas();
                    drawArray(whiteboardDrawingData, true);
                }, drawingLifeTime);
            }
        }
	
        function clearMyDrawing() {
            var i0 = -1; var count = 0;
            for (var i = 0; i < whiteboardDrawingData.length; i++) {
                if (whiteboardDrawingData[i][0] == cmdOwner) {
                    if (i0 < 0) { i0 = i; count += 1; } else { count += 1; }
                } else {
                    if (i0 >= 0) {
                        whiteboardDrawingData.splice(i0, count);
                        i0 = -1; count = 0;
                    }
                }
            }
            if (i0 >= 0 && count > 0) {
                whiteboardDrawingData.splice(i0, count);
            }
            clearCanvas();
            drawArray(whiteboardDrawingData);
            csAction(<%=Command.Setting%>, "&name=whiteboard_drawing_clear_my&value=" + JSON.stringify(whiteboardDrawingData));
        }

        function clearAllDrawing() {
            csAction(<%=Command.Setting%>, "&name=whiteboard_drawing_clear_all");
        }

        function clearCanvas() {
            ctx.setTransform(1, 0, 0, 1, 0, 0);
            ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
        }

        function pushDrawing() {
            if (drawingArr.length) {
                csAction(<%=Command.Setting%>, "&name=whiteboard_draw_arr&value=" + JSON.stringify(drawingArr));                
            }
            drawingArr = [];
        }

        function setWhiteboardMode(value) {
            $("#divWhiteboardMode").dxButtonGroup("instance").option("selectedItemKeys", [value]);
            switch (value) {
                case "normal":
                    $("#divDrawingWhiteboard").css( { "pointer-events" : "none" });
                    break;
                case "drawing":
                    $("#divDrawingWhiteboard").css( { "pointer-events" : "auto" });
                    break;
            }
        }

        function setMeetingMode() {
            location_hid = meetingBoardMode == mmImpacts ? <%=GUILocation.BoardImpact%> : <%=GUILocation.Board%>;
            initTree();
            initTreeContextMenu();
            initWhiteboard();
            updateToolbar();
            initMeetingMode();
        }

        function showLoadingIcon() {
            $("#imgLoading").show();
            setTimeout(function () { $("#imgLoading").hide(); }, 500);
        }

        function resizePage() {
            var wbc = document.getElementById("divWB");
            var dts = document.getElementById("divTreeSplitter");
            var titleHeight = $(".cs-tree-title").outerHeight();
            if ((wbc) && (dts)) {

                var dwc = document.getElementById("divWhiteboardContainer");
                var wt = document.getElementById("whiteboardToolbar");
                dwc.style.height = "50px";
                dwc.style.width = "50px";


                wbc.style.width  = wbc.style.minWidth;
                wbc.style.height = "100px";
                dts.style.height = "200px";
                var ww = $(wbc.parentNode).innerWidth() - $(dts).outerWidth();
                wbc.style.width  = ww < wbc.style.minWidth ? wbc.style.minWidth : ww + "px";
                wbc.style.height = ($(wbc.parentNode).innerHeight() - titleHeight) + "px";
                dts.style.height = ($(wbc.parentNode).innerHeight()) + "px";

                dwc.style.height = ($(wbc.parentNode).innerHeight() - $(wt).outerHeight() - titleHeight - 4) + "px";
                dwc.style.width = wbc.style.width;
        
                if ($("#divTreeSplitter").data('dxResizable')) {
                    var splitter = $("#divTreeSplitter").dxResizable('instance');
                    splitter.option("maxWidth",  wbc.parentNode.clientWidth - 305);
                }
            }

            var divProsCons = document.getElementById("divProsCons");
            var divTree = document.getElementById("divTree");
            if ((divProsCons) && (divTree)) {
                var fullHeight = Math.floor($("#divTreeSplitter").height() - titleHeight - $("#treeToolbar").outerHeight());
                if (!isRisk && meetingBoardMode == mmAlts) {
                    $("#divProsCons").height(Math.floor(fullHeight / 2));
                    $("#divTree").height(fullHeight - $("#divProsCons").outerHeight());
                    divProsCons.style.display = "block";                    
                } else {
                    divProsCons.style.display = "none";
                    $("#divTree").height(fullHeight);
                }
            }

            var ulTreeContainer = document.getElementById("ulTreeContainer");
            ulTreeContainer.style.height = 100;

            //var h = Math.floor($("#divTree").innerHeight() - ((meetingMode == mmProsCons) ? 0 : ulTreeContainer.offsetTop));// ulTreeContainer.offsetTop);// - $("#headerTree").outerHeight(true));
            var h = Math.floor($("#divTree").innerHeight());// ulTreeContainer.offsetTop);// - $("#headerTree").outerHeight(true));
            ulTreeContainer.style.height = h + "px";

            var ulTree = document.getElementById("ulTree");
            ulTree.style.minHeight = ($("#divTree").height() - 12) + "px";
            //ulTree.style.backgroundColor = "yellow";

            resizeWhiteboardTitle();
            if (whiteboardZoom == -1) setZoom(whiteboardZoom);

            if (meetingBoardMode == mmAlts) {
                resizeProsCons();
                //if ($("#divProsConsTable").data("dxDataGrid")) { $("#divProsConsTable").dxDataGrid("instance").repaint(); }
            }

            resizeNowideCaptions();
        }

        function resizeProsCons() {
            $("#divProsConsTable").height($("#divProsCons").height() - $("#headerProsCons").height());
        }

        function resizeWhiteboardTitle() {
            $("#lblWhiteboardTitle").width($(document).width() - $("#divTreeSplitter").width() - 50);
        }

        function onPageUnload(callback_func) {
            deleteUser(cmdOwner); 
            if (typeof navigator.sendBeacon !== "function") {
                callAjax("<%=_PARAM_ACTION %>=do&cs_action=" + <% = Command.DisconnectUser %> + "&user_disconnected=" + JSON.stringify($user), callback_func, ajaxMethod, true, "", false);
                <%--if (meetingState !== msStopped) {
                    callAjax("<%=_PARAM_ACTION %>=do&cs_action=" + <%=Command.DisconnectUser%> + "&user_list=" + JSON.stringify(usersList), callback_func, ajaxMethod, true, "", false);
                }
                if (meetingState == msStopped) {
                    callAjax("<%=_PARAM_ACTION %>=do&cs_action=" + <%=Command.DisconnectUserWhenMeetingStopped%>, null, ajaxMethod, true, "", false);
                }
                if (!isAnonymous && meetingState != msStopped) {
                    callAjax("<%=_PARAM_ACTION %>=do&cs_action=" + <%=Command.SetMeetingState%> + "&value=" + msOwnerDisconnected, null, ajaxMethod, true, "", false);
                }--%>
            } else {
                <%--var data = new FormData();
                data.append('cmdowner', cmdOwner);
                data.append('meetingowner', meetingOwner);
                data.append('prj_id', <% = PRJ.ID %>);
                if (meetingState !== msStopped) {
                    data.append('action', <% = Command.DisconnectUser %>);
                    if (!isAnonymous) {
                        data.append('state', msOwnerDisconnected);
                    }
                    data.append('user_list', JSON.stringify(usersList));
                    navigator.sendBeacon("CSLogout.aspx", data);
                }
                if (meetingState == msStopped) {
                    data.append('cs_action', <% = Command.DisconnectUserWhenMeetingStopped %>);
                    navigator.sendBeacon("CSLogout.aspx", data);
                }--%>
                var data = new FormData();
                data.append('ajax', true);
                data.append('action', 'do');
                data.append('user_disconnected', JSON.stringify($user));
                data.append('cs_action', <% = Command.DisconnectUser %>);
                <%--data.append('cs_action', !isAnonymous ? <% = Command.SetMeetingState %> : <% = Command.DisconnectUser %>);--%>
                <%--if (meetingState !== msStopped) {
                } else {
                    data.append('cs_action', <% = Command.DisconnectUserWhenMeetingStopped %>);
                }--%>
                navigator.sendBeacon("Structuring.aspx", data);
                callback_func();
            }
        }
                
        //window.onunload  = onPageUnload; // not working in Chrome
        //window.onbeforeunload  = onPageUnload;

        $(window).on('beforeunload', function() {
            return onPageUnload();
        });

        logout_before = function(callback_func) {
            onPageUnload(callback_func);
        }

        function onLeavePage() {
            //onLeavePage = null;
            if (!isAnonymous) {
                return stopMeeting();
            } else {
                return true;
            }
        }

        function stopMeeting() {
            if (!isAnonymous && meetingState != msStopped) {
                var is_stopmeeting = false;
                if (usersList.length > 1) {
                    dxConfirm("<% = JS_SafeString(ResString("msgSureEndCS")) %>", function () {
                        is_stopmeeting = true;
                        doStopMeeting();
                    }, function () { initMeetingState(); });
                } else {
                    is_stopmeeting = true;
                    doStopMeeting();
                }
                return is_stopmeeting;
            }
            return true;
        }

        function doStopMeeting() {
            meetingState = msStopped;
            callAjax("<%=_PARAM_ACTION %>=do&cs_action=" + <%=Command.SetMeetingState%> + "&value=" + msStopped, null, ajaxMethod, true, "", false);
        }
        
        function hotkeys(event) {
            if (!document.getElementById) return;
            if (window.event) event = window.event;
            if (event) {
                var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
                switch (code) {
                    case KEYCODE_ESCAPE:
                        removeTemporaryItem();
                        break;
                    //case 17:                        
                    //    if (event.ctrlKey || event.metaKey) {
                    //        if (isAllowedSwitchDrawingModeOnCtrlKey && whiteboardMode !== "drawing") setWhiteboardMode("drawing");
                    //    }
                    //    break;
                }
            }
        }

        function hotkeysUp(event) {
            if (!document.getElementById) return;
            if (window.event) event = window.event;
            if (event) {
                var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
                switch (code) {
                    case 17:                        
                        if (isAllowedSwitchDrawingModeOnCtrlKey && whiteboardMode == "drawing") setWhiteboardMode("normal");
                        break;
                }
            }
        }

        document.onkeydown = hotkeys;
        document.onkeyup = hotkeysUp;
        
        //shortcuts = shortcuts.concat([{ "code" : "Ctrl+B" , "title" : "TT Brainstorming"}]);

        resize_custom = resizePage;

        $(document).ready(function () {
            toggleScrollMain();

            if (isDebug) debugDialog();

            initToolbar(); 
            initTreeSplitter();
            initTree();            
            initTreeTabs();
            initWhiteboardTabs();
            initTreeContextMenu();
            initWhiteboard();
            initWhiteboardDrop();
            initWhiteboardContextMenu();
            initTreeDrop();
            
            initMeetingState();
            updateToolbar();
            initMeetingMode();

            initDrawing();
            
            initMeetingLockState();
            initProsCons();
            initPolling();

            setZoom(whiteboardZoom);
            resizeWhiteboardTitle();
        
            $user = getUserById(cmdOwner);
            if (!($user)) {
                $user = {"id": cmdOwner, "name" : htmlStrip(UserName), "email" : htmlStrip(UserEmail), "can_change_pm" : false, "is_pm" : true, "ts" : timeStamp };
                usersList.push($user);
            }
            <%--csAction(<% = Command.Connect %>, "&user_list=" + encodeURIComponent(JSON.stringify(usersList)));--%>
            csAction(<% = Command.Connect %>, "&connected_user=" + JSON.stringify($user));
            initUserList();
            initMessages();
            updateToolbar();        
        
            $("#divOnlineUsers").draggable({
                //drag: function(event, ui) {                 
                //    if (ui.position.left < 0 || ui.position.top < 0 || ui.position.left + ui.helper.width() + 50 > $(document).width() || ui.position.top + ui.helper.height() + 150 > $(document).height()) return false; 
                //}
            });

            resizeNowideCaptions();

            // do not use dx scroll view, it is not working well
            //$("#divWhiteboardContainer").dxScrollView({ showScrollbar: "onHover", direction: "both" });
            //$("#ulTreeContainer").dxScrollView({ showScrollbar: "onHover", direction: "both" });
        });

        function resizeNowideCaptions() {
            if ($("#divTreeSplitter").width() < (isRisk ? 400 : 300)) { $(".panel-nowide400").hide(); } else { $(".panel-nowide400").show(); }
            if ($("#divWB").width() < (isRisk ? 700 : 600)) { $(".panel-nowide700").hide(); } else { $(".panel-nowide700").show(); }
        }

    </script>

    <table border='0' cellspacing="0" cellpadding="0" class="whole" id='tableStructuringMain'>
        <tr valign="top">
            <td valign="top">
                <div id="toolbar" class="dxToolbar cs-toolbar"></div>
            </td>
        </tr>
        <tr valign="top">
            <td id="tdContent" valign="top" style="overflow: hidden; white-space: nowrap; padding: 0px;" class="whole">
                <div id="divTreeSplitter" style="position: relative; display: inline-block; width: 325px; overflow: hidden;">
                    <div class="cs-tree-title" onclick="return false;" style="display: ;">
                        <div id="lblModelSideCaption" style="font-weight:bold; padding: 8px 1em 3px 0px; font-size:1.3em; text-align:center"><% = ParseString("%%Model%% Elements") %></div>
                    </div>
                    <div id="treeToolbar" class="cs-tree-toolbar">
                        <div id="divTreeTabs" style="display:inline-block;" class="blue_selected"></div>
                        <div class="cs-tree-buttons">
                            <i id="btnAddToList" title="<%=ResString("btnAddAltsCS")%>" class="fas fa-plus" style="cursor: pointer;" onclick="onAddNewItemToList(); event.stopPropagation(); event.preventDefault();"></i>
                            <% If App.isAuthorized Then %>
                            <i id="btnClearList" class="fas fa-trash-alt" title="Clear All" style="cursor: pointer;" onclick="btnClearListClick(); event.stopPropagation(); event.preventDefault();"></i>
                            <% End If %>
                        </div>
                    </div>
                    <div id="divTree" style="height: 100px; overflow: hidden; margin-right: 5px; border-left:1px solid #eaeaea;">                        
                        <div id="ulTreeContainer" style="width:100%; overflow: auto;">
                            <div id="ulTree" class="cs-tree" ondblclick="onAddNewItemToList();"></div>
                        </div>
                        <i class="meeting-lock-icon fas fa-lock fa-2x"></i>
                        <div class="cs-tree-ban-sign-parent">
                            <i class="fas fa-ban cs-tree-ban-sign"></i>
                        </div>
                    </div>
                    <div id="divProsCons" style="display: none; height: 40%; overflow: hidden; margin-right: 4px; border-left:1px solid #eaeaea;" onclick="onProsConsPaneClick(event);">
                        <div class="cs-tree-title" onclick="return false;">
                            <div id="headerProsCons" style="font-weight:bold; padding: 8px 1em 3px 0px; font-size:1.3em; text-align:center"><%=ParseString("Pros and Cons Summary")%></div>
                        </div>
                        <div id="divProsConsTable" style="height: 100%;"></div>
                    </div>
                </div>
                <div id="divWB" style="position: relative; display: inline-block; vertical-align: top; width: 1000px; min-width: 300px; height: 700px;">
                    <div class="cs-whiteboard-title" onclick="return false;">
                        <div id="lblWhiteboardSideCaption" style="font-weight: bold; padding: 8px 1em 3px 0px; font-size:1.3em; text-align:center">Brainstormed Elements</div>
                    </div>
                    <div id="whiteboardToolbar" class="cs-whiteboard-toolbar" onclick="event.stopPropagation(); event.preventDefault();">
                        <div style="display:inline-block;" id="btn_switch_view" class="blue_selected"></div>
                        <div class="cs-tree-buttons">
                            <i id="btn_all_proscons" title="<%=ParseString("Show or hide Pros and Cons for all %%alternatives%%")%>" class="fas fa-columns" style="cursor: pointer;" onclick="toggleAllProsCons(); event.stopPropagation(); event.preventDefault();"></i>
                            <i id="btn_hide_infodocs" title="<%=ParseString("Hide information documents for all items")%>" class="fas fa-info-circle" style="cursor: pointer;" onclick="hideAllInfodocs(); event.stopPropagation(); event.preventDefault();"></i>
                            <%--<i id="btnRefreshWhiteboard" title="Refresh" class="fas fa-sync" style="cursor: pointer;" onclick="reloadPage(); event.stopPropagation(); event.preventDefault();"></i>--%>
                            <% If App.isAuthorized Then %>
                            <i id="btnClearWhiteboard" class="fas fa-trash-alt" title="Clear All" style="cursor: pointer;" onclick="onClearWhiteboard(); event.stopPropagation(); event.preventDefault();"></i>
                            <% End If %>
                        </div>
                        <div style="display:inline-block; float: right; margin-right: 5px;" id="divWhiteboardDrawingClear" class="drawing-ui"></div>
                        <div style="display:inline-block; float: right; margin-right: 5px;" id="divWhiteboardDrawingColor" class="drawing-ui"></div>
                        <div style="display:inline-block; float: right; margin-right: 5px;" id="divWhiteboardDrawingHide" class="blue_selected drawing-ui"></div>
                        <div style="display:inline-block; float: right; margin-right: 5px;" id="divWhiteboardMode" class="blue_selected drawing-ui"></div>
                    </div>
                    <div id="divWhiteboardContainer" style="position: relative; display: inline-block; margin-left:-3px; vertical-align: top; width: 100%; min-width: 300px; height: 100%; overflow: auto;">
                        <div id="divWhiteboard" class="cs-whiteboard" style="overflow: ; width: <%=WhiteboardWidth%>px; height: <%=WhiteboardHeight%>px;" onclick = "whiteboardClick(event);" ondblclick="whiteboardDblClick(event);">
                            <div id="lblWhiteboardTitle" class="cs-whiteboard-title" onclick="return false;">
                                <%If Not PM.IsRiskProject Then%>
                                <div style="text-align: center; margin-top: 1em; left: 50%; z-index: 10000;">
                                    <span id="lblGoalTitle" class="highlight" style="white-space: normal; text-align: center; font-size: 15px; padding: 4px; font-weight: bold; opacity: 1;"><%=SafeFormString(PM.ActiveObjectives.Nodes(0).NodeName)%></span>
                                    <i id="btnEditGoal" class="fas fa-pencil-alt ec-icon" onclick="onEditGoal(); event.stopPropagation(); event.preventDefault();"></i>
                                    <i id="btnEditGoalInfodoc" class="fas fa-info-circle ec-icon" onclick="onEditGoalInfodoc(); event.stopPropagation(); event.preventDefault();" title="Edit Goal Information Document"></i>
                                </div>
                                <%End If%>
                                <div style="text-align: center; margin-top: 8px; left: 50%;">
                                    <span id="lblTitle0" style="white-space: normal; text-align: center; font-size: 13px; opacity: 0.9;"><%=String.Format(ResString("lblTTHelpTitle"), PRJ.ProjectName)%></span><br />
                                    <span id="lblTitle1" style="white-space: normal; text-align: center; font-size: 13px; opacity: 0.9;"><%=String.Format(ResString("lblTTHelpLabel"), ParseString(If(PM.Parameters.CS_BoardMode = Structuring.MeetingMode.Alternatives, "%%alternative%%", If(PM.Parameters.CS_BoardMode = Structuring.MeetingMode.Sources, "%%objective(l)%%", "%%objective(i)%%"))))%></span>
                                    <%--<i id="imgProsConsSign" class="fas fa-balance-scale-left cs-mode-sign" style="color: transparent;"></i>--%>
                                </div>

                                <i class="meeting-lock-icon fas fa-lock fa-2x"></i>
                                <div class="cs-whiteboard-ban-sign-parent">
                                    <i class="fas fa-ban cs-whiteboard-ban-sign"></i>
                                </div>
                            </div>
                        </div>
                        <canvas id="divDrawingWhiteboard" class="cs-drawing-whiteboard" style="pointer-events: none;" width="<%=WhiteboardWidth%>px;" height="<%=WhiteboardHeight%>px;">
                        </canvas>
                    </div>
                </div>

                <div id="divTreeMenu"></div>
                <div id="divWhiteboardMenu"></div>

                <div id="divWaiting" class="whole" style="position: absolute; left: 0; top: 34px; height: calc(100% - 34px); background-color: #f5f5f5; z-index: 1498; opacity: 0.85; text-align: left;"></div>
                <h5 id="lblWaitingMsg" class="cs-popup" style="position: absolute; top: 43px; display: block; height: 33px; z-index: 1498; margin-top: 10px; margin-right: 7px; text-align: left; min-width: 400px; border: 1px solid #ccc; padding: 10px; opacity: 1;">The meeting is inactive.</h5>
                <div id="divOnlineUsers" class="cs-popup" style="overflow: auto; position: absolute; top: 110px; cursor: move; z-index: 1498; margin-top: 10px; width: 400px; min-width: 400px; min-height: 50px; max-height: 600px; border: 1px solid #ccc; padding: 5px 10px 5px 10px; opacity: 1;">
                    <i class="fas fa-times fa toolbar-label" style="float: right; cursor: pointer; margin-right: 6px; margin-top: 3px;" onclick="toggleOnlineParticipantsView();"></i>                    
                    <h5 style="margin-top: 10px; text-align: left;"><% If App.isAuthorized Then %>
                    <%--<div title="Project Manager" style="top: 48px; right: 15px; position: absolute; cursor: default; font-size: smaller;">PM</div>--%>
                    <div id="btnManageParticipants" style="float: right; margin-right: 20px; margin-top: -4px;"></div>
                    <% End If %>
                    Participants:</h5>                    
                    <hr />
                    <div id="divUserList" style="min-height: 100px;"></div>
                    <h5 style="margin-top: 10px; text-align: left;">Messages:</h5>
                    <hr />
                    <div id="divMessagesList" style="height: 100px;"></div>
                    <div id="divNewMessage" style="width: 365px; display: inline-block;"></div>
                    <div id="divSendMessage" style="vertical-align: top;"></div>
                </div>

            </td>
        </tr>
    </table>

    <div id="popupSettings" class="cs-popup"></div>
    <div id="popupDebug" class="cs-popup"></div>    
    <div id="popupColors" class="cs-popup"></div>

</asp:Content>