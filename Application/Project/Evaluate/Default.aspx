<%@ Page Language="VB" EnableEventValidation="false" Inherits="EvaluationPage2" Codebehind="Default.aspx.vb"%>
<%@ Register TagPrefix="EC" TagName="SurveySpyron" Src="~/Spyron/Surveys/ctrlSurveyPageView.ascx" %>
<%@ Register TagPrefix="EC" TagName="PairwiseBar" Src="~/ctrlPairwise.ascx" %>
<%@ Register TagPrefix="EC" TagName="MultiPairwise" Src="~/ctrlMultiPairwise.ascx" %>
<%@ Register TagPrefix="EC" TagName="Rating" Src="~/ctrlRating.ascx" %>
<%@ Register TagPrefix="EC" TagName="MultiRating" Src="~/ctrlMultiRatings.ascx" %>
<%@ Register TagPrefix="EC" TagName="MultiDirectInputs" Src="~/ctrlMultiDirectInputs.ascx" %>
<%@ Register TagPrefix="EC" TagName="ShowResults" Src="~/ctrlShowResults2.ascx" %>
<%@ Register TagPrefix="EC" TagName="UtilityCurve" Src="~/ctrlUtilityCurve.ascx" %>
<%@ Register TagPrefix="EC" TagName="DirectData" Src="~/ctrlDirectData.ascx" %>
<%@ Register TagPrefix="EC" TagName="SensitivityAnalysis" Src="~/ctrlSensitivityAnalysis.ascx" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" runat="Server">
<% If divEditTask.Visible Then%><script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jHtmlArea-0.8.js?rev=160322"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jHtmlArea.ColorPickerMenu-0.8.min.js"></script><% End If%>
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="600" LoadScriptsBeforeUI="true"></asp:ScriptManager>

<asp:Panel runat="server" ID="pnlEvaluate" Height="99%" Width="99%">

<table border="0" cellpadding="0" cellspacing="0" width="100%" class="whole" style="margin-top:-1px; height:100%" id="tblEvalFooter">
    <tr runat="server" id="rowEdit" class="text small" visible="false" style="height:16px">
        <td colspan="4" align="center" valign="top" style="padding:4px;" class="text small"><span runat="server" id="lblReadOnly" Visible="false" class='text top_warning error' style='padding:2px 2ex;white-space:nowrap;'></span></td>
        <%--<td colspan="2" align="right" valign="top" style="padding-right:0px;padding-bottom:2px;" class="text small">&nbsp;<% If IsIntensities AndAlso CurStep <= App.ActiveProject.Pipe.Count Then%><a href='' onclick="ConfrimCancelIntensities(); return false;"><img src='<% =ImagePath + "stop.png" %>' name='imgFinish' title='<% =ResString("btnCloseIntensities") %>' width='16' height='16' border='0' onmouseover='ChangeImage(this, img_stop_);' onmouseout='ChangeImage(this, img_stop);' ></a><% End If%></td>--%>
    </tr>
    <tr>
        <td colspan="4" align="center" valign="middle" height="100%"><div style='overflow:auto; width:100%; height:100%; margin:0px 0px;'>

            <table border="0" cellspacing="0" cellpadding="0" style="width:100%; height:99%">
                <tr runat="server" id="trHead">
                    <td rowspan="2" style="width:1px" runat="server" id="tdColLeft" visible="false"><div runat="server" id="lblColLeft" visible="false"></div></td>
                    <td valign="middle" align="center" runat="server" id="tdTask" class="eval_task"><div runat="server" id="divEditTask" visible="false">
                        <input type='hidden' name='SaveNewTask' id='SaveNewTask' value='0' /><input type='hidden' name='TaskNodeGUID' id='TaskNodeGUID' value='<% =TaskNodeGUID %>' /><input type='hidden' id='TaskNodesList' name='TaskNodesList' value='<% =CurStep %>' />
                        <input type='button' name='btnTaskEditor' id='btnTaskEditor' value='<% =ResString("btnEdit") %><% = iif(ClusterPhraseIsCustom, "*", "") %>' class='button button_small<% =iif(ClusterPhraseIsCustom, " button_evaluate_undefined", "") %>' style='float:right; margin:6px 0px 6px 1em; width:6em; padding:2px;' onclick='SwitchTaskEditor(0); return false;' <% =iif(TaskNodeGUID<>"", "", "disabled=1") %> />
                        <div id='divClusterPhrase' style='display:none;'><textarea name='txtTask' id='txtTask' class='textarea' cols='65' rows='3' style='width:950px; height:6.5em;'><% = SafeFormString(ClusterPhrase) %></textarea><telerik:RadToolTip ID="RadToolTipHints" runat="server" HideEvent="ManualClose" ShowEvent="FromCode" ShowDelay="50" Position="BottomCenter" RelativeTo="Mouse" TargetControlID="divClusterPhrase" OffsetY="-6" OffsetX="-32" IsClientID="true" /><telerik:RadToolTip ID="RadToolTipClusterSteps" runat="server" HideEvent="ManualClose" ShowEvent="FromCode" ShowDelay="50" Position="BottomCenter" RelativeTo="Mouse" TargetControlID="divClusterPhrase" OffsetY="-6" OffsetX="-32" IsClientID="true" /></div>
                    </div><asp:Label ID="lblTask" runat="server"/><span id="divT2S" style="font-size:20px; margin-left:1ex; display:none"></span><input type="hidden" id="txtEvalObject" name="txtEvalObject" value="" /></td>
                    <td rowspan="2" style="width:1px" runat="server" id="tdColRight" visible="false"><div runat="server" id="lblColRight" visible="false"></div></td>
                </tr>
                <tr runat="server" id="trContent" style="height:99%">
                <td runat="server" id="tdContent" valign="top" align="center" style="text-align:center;" height="100%">
                <table border="0" cellspacing="0" cellpadding="0" style="width:100%;height:100%"><% ="" %>
                <tr><td valign="top" align="center" style="text-align:center;" id="tdMainCell" runat="server"><% If ShowQuickHelp Then%><% If CanUserEditActiveProject Then%><a href='' onclick="editQuickHelp('<% =JS_SafeString(QuickHelpParams) %>'); return false;"><img src="<% =ImagePath %>edit_small.gif" width="16" height="16" border="0" title="Edit Quick Help" alt="" style="margin-top:7px; margin-left:2px; float:right"/></a><% End if %><a href="" id="lnkQHVew" onclick="showQuickHelp('<% =JS_SafeString(QuickHelpParams) %>', false, <% =Bool2JS(CanUserEditActiveProject) %>, onLoadQH); return false;"><img src="<% =ImagePath %>help/<% =IIf(QuickHelpContent = "", "icon-question_dis.png", "icon-question.png")  %>" id="imgQH" width="16" height="16" border="0" title="Show Quick Help" alt="" style="margin-top:7px; float:right"/></a><% End If %><% If App.HasActiveProject AndAlso ShowInfoDocs AndAlso CanUserEditActiveProject AndAlso isEvaluation(GetAction(CurStep)) AndAlso Not IsIntensities AndAlso Not GetAction(CurStep).ActionType = ActionType.atSpyronSurvey Then%><a href='' onclick='SwitchInfodocMode(); return false;'><img src="<% =ImagePath %>infodoc_mode.gif" width="16" height="16" border="0" title="<% =ResString("lblSwitchInfodocMode") %>" alt="" style="margin:2px; float:left;"/></a><% End if %><%--<img src="<% =ImagePath %>keyboard.png" id="imgHotkeys" width="16" height="16" border="0" style="margin:2px; float:left; cursor: pointer"/>--%><% =GetPMSwitch() %>
                    <div id="divLocalResultsHeader" runat="server" visible="false" style='padding:8px 0px; margin:0px;'><div runat="server" id="divEditTitle" visible="false" style="text-align:center;">
                        <input type='hidden' name='SaveNewTitle' id='SaveNewTitle' value='0' /><input type='hidden' name='TitleNodeGUID' id='TitleNodeGUID' value='<% =TaskNodeGUID %>' /><input type='hidden' id='TitleNodesList' name='TitleNodesList' value='<% =CurStep %>' />
                        <input type='button' name='btnTitleEditor' id='btnTitleEditor' value='<% =ResString("btnEdit") %><% = iif(ClusterTitleIsCustom, "*", "") %>' class='button button_small<% =iif(ClusterTitleIsCustom, " button_evaluate_undefined", "") %>' style='float:right; margin:0px 3em 0px 1em; width:6em; padding:2px;' onclick='SwitchTaskEditor(1); return false;' <% =iif(TaskNodeGUID<>"", "", "disabled=1") %> />
                        <div id='divClusterTitle' style='display:none; height:4.75em;'><div style='width:954px; position: absolute; left:0; right:0; margin:auto; '><textarea name='txtTitle' id='txtTitle' class='textarea' cols='65' rows='3' style='width:950px; height:5.75em;'><% = SafeFormString(ClusterTitle) %></textarea><telerik:RadToolTip ID="RadToolTipTitleHints" runat="server" HideEvent="ManualClose" ShowEvent="FromCode" ShowDelay="50" Position="BottomCenter" RelativeTo="Mouse" TargetControlID="divClusterTitle" OffsetY="-6" OffsetX="-32" IsClientID="true" /><telerik:RadToolTip ID="RadToolTipTitleSteps" runat="server" HideEvent="ManualClose" ShowEvent="FromCode" ShowDelay="50" Position="BottomCenter" RelativeTo="Mouse" TargetControlID="divClusterTitle" OffsetY="-6" OffsetX="-32" IsClientID="true" /></div></div>
                    </div><asp:Label ID="lblResultsTitle" runat="server" CssClass="h4" Font-Bold="true" /></div>
                    <asp:PlaceHolder ID="ActionContent" runat="server" EnableViewState="False" /></td></tr>
                <tr runat="server" id="tdExtraContent" visible="false"><td valign="top" align="center" style="text-align:center; height:90%;">
                <div runat="server" id="divSpyron" style="height:98%; overflow-y:auto; overflow-x:hidden;" visible="false" class="text">
                    <script type="text/javascript">
                        function _SpyronResize() {
                            $("#<% =divSpyron.ClientID %>").height(10).height($("#<% =tdExtraContent.ClientID %>").height()-16);
                        }
                        resize_custom = _SpyronResize;
                    </script>
                    <EC:SurveySpyron runat="server" ID="SurveySpyron" Visible="false"/>
                </div>
                <asp:Panel runat="server" ID="pnlSLResults" Visible="false" Height="98%" BorderWidth="0" HorizontalAlign="Center">
<telerik:RadScriptBlock runat="server" ID="ScriptBlock">
    <script type="text/javascript">

        var avoid_is_return = 0;
        var sl_next = 0;

        function ShowResultsTitle(vis) {
            var o = $get("<% =lblResultsTitle.ClientID %>");
            if ((o)) o.style.display = ((vis) ? 'block' : 'none');
            var t = $get("<% = trHead.ClientID %>");
            if ((t)) t.style.display = ((vis) ? '' : 'none'); /*A1462*/
            var h = $("#<% =divLocalResultsHeader.ClientID %>");
            if ((h)) { if ((vis)) h.show(); else h.hide(); }
            onPageResize();
        }

        function NavigateToSubPipe(step, id1, id2, isSingle) {
            SetBusyText("<%=ResString("htmlPleaseWait") %>");
            showLoadingPanel();
            document.location.href = '?step=' + (step * 1 + 1) + '&oid=' + id1 + '&aid=' + id2 + '<% =GetTempThemeURI(true) %>&ret=<% =CurStep %><% =iif(IsReadOnly,"&readonly=true","") %><% =iif(IsIntensities, "&" + _PARAM_INTENSITIES + "=true", "") %>&noir=' + (avoid_is_return ? 1 : 0)+'&is_single='+isSingle;
            return false;
        }
        
        function NavigateToSubPipeNoReturn(step, id1, id2) {
            SetBusyText("<%=ResString("htmlPleaseWait") %>");
            showLoadingPanel();
            document.location.href = '?step=' + (step * 1 + 1) + '&oid=' + id1 + '&aid=' + id2 + '<% =GetTempThemeURI(true) %><% =iif(IsReadOnly,"&readonly=true","") %><% =iif(IsIntensities, "&" + _PARAM_INTENSITIES + "=true", "") %>&noir=' + (avoid_is_return ? 1 : 0);
            return false;
        }

        function CertainWarning2(cmd) {
            var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgCertainNotHigh")) %>", resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    eval(cmd);
                }
            });
        }

        function CertainWarning(cmd) {
            setTimeout('CertainWarning2("' + cmd + '")', 600);
        }

        function NavigateToExtraStep(pid, id1, id2, no_return, isSingle) {
            showLoadingPanel();
            document.location.href = '?action=add_step&pid=' + pid + '&id1=' + id1 + '&id2=' + id2 + (no_return ? '' : '&ret=<% =CurStep+1 %>') + '<% =GetTempThemeURI(true) %><% =iif(IsReadOnly,"&readonly=true","") %><% =iif(IsIntensities, "&" + _PARAM_INTENSITIES + "=true", "") %>&noir=' + (avoid_is_return ? 1 : 0)+'&is_single='+isSingle;
            return false;
        }
        
        function RequireCallbackOnNext(fBool) {
            // true - when Next is pressed there should be called a callback function: SLPlugin.ShowPrioritiesTable()
            // false (default) - on Next button do navigate to the next pipe step as usual
            setTimeout("sl_next = '" + ((fBool) ? 1 : 0) + "';", 300);
        }

        function RequireIsReturn(fBool) {
            // if TRUE  - then calculate and pass is_return to silverligth, otherwise do nothing
            avoid_is_return = ((fBool));
        }

        function CallSLGrid() {
//            var sl = document.getElementById("slLocalResults");
//            if ((sl)) sl.Content.SLPlugin.ShowPrioritiesTable();
            SetActiveView(avPrioritiesDerived);
            setTimeout("sl_next = 0;", 300);            
            return false;
        }

        //if (!(window.Silverlight) || !(window.Silverlight.isInstalled('5.0'))) { document.location.href = '?<% =_PARAM_ACTION %>=html<% =GetTempThemeURI(True) %>'; }

        /* Intermediate results */

        var cur_cmd = "";
        var ActiveView = 0; //PrioritiesDerived
        var text = ("textContent" in document) ? "textContent" : "innerText";
        
        // Notmalization Modes
        var ntNormalizedForAll = <%=AlternativeNormalizationOptions.anoPriority%>;
        var ntNormalizedMul100 = <%=AlternativeNormalizationOptions.anoPercentOfMax%>;
        var ntNormalizedSum100 = <%=AlternativeNormalizationOptions.anoMultipleOfMin%>;
        var ntUnnormalized     = <%=AlternativeNormalizationOptions.anoPercentOfMax%>;

        var NormalizeMode = <%=CInt(NormalizationMode)%>;
        var ACTION_NORMALIZE_MODE    = "<%=ACTION_NORMALIZE_MODE%>";

        function sendCommand(cmd) {
            cur_cmd = cmd;
           
            showLoadingPanel();           
            var am = $find("<%=RadAjaxManagerMain.ClientID%>");
            if ((am)) { am.ajaxRequest(cmd); return true; }        
            return false;
        }

        function onRequestError(sender, e) {
            hideLoadingPanel();
            DevExpress.ui.dialog.alert("(!) Callback error.\nStatus code: " + e.Status + "\nResponse Text: " + e.ResponseText, "AJAX request error");
        }

        var CMD_ACTION = 0;
        var CMD_EDIT   = 1;
        var CMD_SORT   = 2;
        var RestoreSortByPriority = "<%=CallbackRestoreSortByPriority%>" == "1";

        function onResponseEnd(sender, argmnts) {
            var d = $get("<% =divCommand.ClientID %>");
            if ((d)) {
                var cmd = eval(d[text]);                
                AnyEditMade = cmd[CMD_EDIT] == "1";
                SortByPriority = cmd[CMD_SORT] == "1";
                RestoreSortByPriority = cmd[CMD_SORT] == "2";
                switch (cmd[CMD_ACTION]+"") {
                    case "invert_judgment":
                    case "invert_all":
                    case "restore_all":
                    case "judgment":
                    case "sort_by_priority":                    
                    case "paste_from_clipboard":
                        SetActiveView(avInconsistencyTooHigh);
                        break;
                    case "reset_sorting_and_copy_judgments":
                        SetActiveView(avInconsistencyTooHigh);
                        doCopyJudgments();
                        break;
                    //case "reset_sorting":
                    //    SetActiveView(avInconsistencyTooHigh);
                    //    break;
                    case "sort":
                    case "toggle_global":
                    case "set_pwnl":
                    case ACTION_NORMALIZE_MODE:
                        SetActiveView(avPrioritiesDerived);
                        break;
                }
            }

            //if (cur_cmd.indexOf("action=link") == 0) { }
    
            onPageResize();
            hideLoadingPanel();
            
            //var cbSort = $get("cbSort");
            //if ((cbSort)) cbSort.disabled = false;
            
            gridDblClickHandler();
            focusLastCell();
            setTimeout("focusLastCell();", 200);
        };

        function SetBusyText(busyText) {
            hideAll();
            var lbl = $get("htmlPleaseWait");
            if ((lbl)) lbl[text] = busyText;
            $("#<%=ctrlPleaseWait.ClientID%>").show();
        }
        
        function hideAll() {
            //alert("hide all");
            $("#<%=ctrlPleaseWait.ClientID%>").hide();
            $("#<%=ctrlQueryWhatsWrong.ClientID%>").hide();
            $("#<%=ctrlInconsistencyTooHigh.ClientID%>").hide();
            $("#<%=ctrlInsufficiencyOfData.ClientID%>").hide();
            $("#<%=ctrlPrioritiesDerived.ClientID%>").hide();
            $("#<%=ctrlReviewJudgments.ClientID%>").hide();
        }

        //av - ActiveView variants
        var avPrioritiesDerived = 0;
        var avQueryWhatsWrong = 1;
        var avElicitSpecificPair = 2;
        var avInconsistencyTooHigh = 3;
        var avGotoPairwisePipeStep = 4;
        var avGotoFirstPairwisePipeStep = 5;
        var avInsufficientInfo = 6;
        var avProjectOffline = 7;
        var avPleaseWait = 8;
        var avReviewJudgments = 9;
        var avInvertAllJudgments = 10;
        var avAdjustManually = 11;
        var parentNodeID = <% =Model.ParentID %>;
        
        function SetActiveView(newView) {
            hideAll();
            //alert("SetActiveView: " + newView);

            if (newView == avQueryWhatsWrong) {
            <% If IsReadOnly %>
                newView = avInconsistencyTooHigh;
            <% End If %>
            }
            InitData();
            switch (newView) {
                case avPleaseWait:
                    SetBusyText("<%=ResString("htmlPleaseWait") %>");
                    break;
                case avPrioritiesDerived:
                    RequireCallbackOnNext(false);
                    $("#<%=ctrlPrioritiesDerived.ClientID%>").show();                    
                    $(".chk_cell").hide(); // hide checkboxes column
                    ShowResultsTitle(true);  // D2385
                    $("#btnNotReasonable1").show();
                    $("#btnNotSatisfactory").show();
                    $("#tblSpecificInfo").hide();
                    break;
                case avQueryWhatsWrong:
                    $("#<%=ctrlQueryWhatsWrong.ClientID%>").show();
                    ShowResultsTitle(false);
                    break;
                case avElicitSpecificPair:
                    $("#<%=ctrlPrioritiesDerived.ClientID%>").show();                    
                    $(".chk_cell").show(); // show checkboxes column
                    $("#btnNotReasonable1").hide();
                    $("#btnNotSatisfactory").hide();
                    $("#tblSpecificInfo").show();
                    break;
                case avInconsistencyTooHigh:                    
                    if (RestoreSortByPriority) {$("#btnRestoreSortOrder").show();} else {$("#btnRestoreSortOrder").hide();}
                    InitObjectivesTable(ShowTextBoxes, SortByPriority, ShowBestFit);
                    $("#<%=ctrlInconsistencyTooHigh.ClientID%>").show();                    
                    break;
                case avGotoPairwisePipeStep:
                    var count = $('.chkOneOfPair:checked').length;
                    if (count == 2) {
                        var obj_ids = [];
                        $('.chkOneOfPair:checked').each(function() {
                            obj_ids.push(parseInt($(this).val()));
                        });                    
                        var obj1 = obj_ids[0];
                        var obj2 = obj_ids[1];
                        var stepNum = GetStepNumber(obj1, obj2);
                        if (stepNum >= 0) {
                            NavigateToSubPipe(stepNum, obj1, obj2, 0);
                        } else {
                            NavigateToExtraStep(parentNodeID, obj1, obj2, 0);
                        };
                    } else {
                        dxDialog("<% = JS_SafeString(ResString("msgSelectTwoObjectives")) %>", ';', ';', resString("titleError"));
                        SetActiveView(avElicitSpecificPair);
                    };
                    break;
                case avGotoFirstPairwisePipeStep:
                    var hasSteps = ("<% =(Model.StepPairs IsNot Nothing AndAlso Model.StepPairs.Count > 0) %>").toLowerCase();
                    if ((hasSteps == "1") || (hasSteps == "true")) {
                        var StepNumber = <% =GetFirstClusterStep() %>;
                        var Obj1 = <% =GetFirstClusterStepObj1() %>;
                        var Obj2 = <% =GetFirstClusterStepObj2() %>;
                        NavigateToSubPipeNoReturn(StepNumber, Obj1, Obj2);
                    }
                    break;
                case avInsufficientInfo:
                    $("#<%=ctrlInsufficiencyOfData.ClientID%>").show();
                    break;
                case avProjectOffline:
                    SetBusyText("<% =ResString("msgWarningProjectOffline") %>");
                    break;
                case avReviewJudgments:
                    $("#<%=ctrlReviewJudgments.ClientID%>").show();
                    break;
                case avInvertAllJudgments:
                    //SetBusyText("<%=ResString("htmlPleaseWait") %>");
                    var mesg = "<%=ResString("msgSureInvertAllJudgments")%>";
                    dxConfirm(mesg, "sendCommand('action=invert_all');", "SetActiveView(avInconsistencyTooHigh);");
                    break;
                case avAdjustManually:
                    //TODO:
                    alert('Not implemented');
                    break;
            }
        }

        function GetPipeStepDataCompleted() {
            var isReturn = "<% =CStr(IIf(IsReturn(), "1", "0")) %>";
            if (isReturn == "1") {
                ActiveView = avInconsistencyTooHigh;
                ShowResultsTitle(false);
            }

            var hasInfo = <% = CStr(IIf(Not Model.InsufficientInfo, "1", "0")) %>;
            var isInts  = <% = CStr(IIf(IsIntensities AndAlso App.isRiskEnabled AndAlso HasCertainIntensityAndItIsLowerThanMax(), "1", "0")) %>;

            if (hasInfo == "1") {
                if (isInts == "1") CallJSCertainWarning();
                ContinueSetActiveView(); 
            } else { 
                ActiveView = avInsufficientInfo;
                SetActiveView(ActiveView);
            }
            //initRightClick();
        }

        function GetStepNumber(Obj1Id, Obj2Id) {
            var retVal = -1;
            var o1 = parseInt(Obj1Id);
            var o2 = parseInt(Obj2Id);

            if ((step_data)) {
                for (var i=0; i < step_data.length; i++) {
                    var s = step_data[i];
                    if (((s[IDX_OBJ1] == o1) && (s[IDX_OBJ2] == o2)) || ((s[IDX_OBJ1] == o2) && (s[IDX_OBJ2] == o1))) retVal = s[0];
                }
            }
            return retVal;
        }
    
        function CallJSCertainWarning() {            
            var CertID = <% = tCertainIntensityID %>;
            var MaxID = <% = tMaxIntensityID %>;
            var StepNumber = parseInt("<% = MaxAndCertainStepID %>");            
            //   alert("CertID=" + CertID + ": MaxID=" + MaxID + ": Step=" + StepNumber );
            var cmd = "";
            if (StepNumber >= 0) {
                cmd = "NavigateToSubPipe(" + StepNumber + "," + CertID + "," + MaxID + ",1)";
            } else {
                cmd = "NavigateToExtraStep(" + StepNumber + "," + CertID + "," + MaxID + ",1)"; 
            }
            CertainWarning(cmd);
        }

        function ContinueSetActiveView() {            
            var sameSideJudgments = <% = Cstr(IIF((Not IsIntensities) AndAlso (Not App.isRiskEnabled) AndAlso (Not AllJudgmentsOnSameSideWarningShown) AndAlso AllJudgmentsOnSameSide(), "1", "0")) %>;
            if (sameSideJudgments == "1") {
                SetActiveView(avReviewJudgments);                
            } else {
                SetActiveView(ActiveView);
            }
        }

        function ReviewMissingJudgments() {
            var undefStep = <% =GetFirstUndefinedStep() %>;
            if (undefStep.length>0) {
                NavigateToSubPipeNoReturn(parseInt(undefStep[0]), parseInt(undefStep[1]), parseInt(undefStep[2]));
            }            
        }

        function cbNormalizeModeChange(value) {
            NormalizeMode = value*1;
            sendCommand("action=" + ACTION_NORMALIZE_MODE + "&value=" + value, true);
        }

        var last_cb_id = "";

        function CheckPairs(index, value, id) {                       
            var count = $('.chkOneOfPair:checked').length;
            if (count > 2) {
                $("#" + last_cb_id).prop('checked', false);
            }
            last_cb_id = id + "";
            count = $('.chkOneOfPair:checked').length;
            $("#btnReEval").prop('disabled', (count != 2));
        }

        function RowHover(r, hover, alt) {
            if ((r)) {
                var bg = (hover == 1 ? (alt ? "#eaf0f5" : "#f0f5ff") : (alt ? "#fafafa" : "#ffffff"));
                for (var i = 0; i < r.cells.length; i++) {
                    r.cells[i].style.background = bg;
                }
            }
        }

        function onPageResize() {
            $("#<% = divGrid.ClientID %>").height(100);            
            $("#<% = divGrid.ClientID %>").height($("#tdGrid").height() - 2);            
        }

        var is_context_menu_open = false;

        function initRightClick() {
            $("#tblIntermediateResults").bind("contextmenu", function(event) {                                 
                event.preventDefault();                
                $("div.context-menu").hide().remove();                
                var sMenu = "<div class='context-menu'>";                   
                   sMenu += "<a href='' class='context-menu-item' onclick='onShowEvaluationPipeHelp(false);return false;'><div><nobr><% =ResString("btnHelp") %></nobr></div></a>";
                   sMenu += "<a href='' class='context-menu-item' onclick='onShowEvaluationPipeHelp(true); return false;'><div><nobr><% =ResString("btnResourceCenter") %></div></nobr></a>";
                   sMenu += "</div>";                
                var x = event.pageX;
                var s = $(sMenu).appendTo("body").css({top: event.pageY + "px", left: x + "px"});
                if ((s)) { var w = s.width();var pw = $("#tblIntermediateResults").width(); if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"}); }
                is_context_menu_open = true;                
                $("div.context-menu").fadeIn(500);                
            });
            $(document).bind("click", function(event) { $("div.context-menu").hide(200); is_context_menu_open = false; });
        }

        resize_custom = onPageResize;

        /* Inconsistency improvement screen */             
        function round(s) {
            var dec = Math.pow(10, DECIMALS);
            if ((s) && (s!="") && validFloat(s)) {
                var value = str2double(s);
                return Math.round(value * dec) / dec;
            }
            return "";
        }

        function getValue(value) {
            if (value + "" == "0") return "";
            var v0 = str2double(value + "");
            var v1 = Math.round(v0 * 1000) / 1000;
            var v2 = Math.round(v0 * 100) / 100;
            return v0 == v1 ? v0 : v2;
        }

        //function sortByChanged(value) {
        //    var sCommand = "action=sort_by_priority";
        //    if (value*1 == 0) {SortByPriority = false; sCommand += "&do=false";}
        //    if (value*1 == 1) {SortByPriority = true; SortByPriorityDir = "desc"; sCommand += "&do=true&dir=desc";}
        //    if (value*1 == 2) {SortByPriority = true; SortByPriorityDir = "asc"; sCommand += "&do=true&dir=asc";}
        //    sendCommand(sCommand);
        //}

        function onSortByPriorityClick() {
            sendCommand("action=sort_by_priority&dir=desc&do=true");
        }

        function onSortByOriginalOrderClick() {
            sendCommand("action=sort_by_priority&dir=desc&do=false");
        }

        var obj_data = [];
        /* < =GetObjectivesData() > */
        var IDX_ID    = 0;
        var IDX_VALUE = 1;
        var IDX_NAME  = 2;
        var IDX_VALUE_P = 3; //percentage

        var step_data = [];
        /* < =GetPairsData() > */
        var IDX_STEP = 0;
      //var IDX_VALUE= 1;
        var IDX_OBJ1 = 2;
        var IDX_OBJ2 = 3;
        var IDX_ADVANTAGE = 4;
        var IDX_BF_VALUE  = 5;
        var IDX_BF_ADV    = 6;
        var IDX_RANK = 7;
        var IDX_UNDEFINED = 8;

        var HIGHLIGHT_COLOR   = "#eaf0f5"; // yellow: "#ffff99";
        var CELL_BACKGROUND   = "#fffafa"; //"#f0f5fa";
        var DISABLED_CELL_BACKGROUND = "#778899";
        var HDR_BACKGROUND   =  "#ffffff"; //"#e0e0e0";
    
        var ROW_HEADERS_WIDTH_INT = 210;
        var ROW_HEADERS_WIDTH = ROW_HEADERS_WIDTH_INT+"px";
        var ROW_HEADERS_HEIGHT = "33px";
        var DATA_CELL_WIDTH   = "65px";
        var DATA_CELL_HEIGHT  = "30px"; // 33px;
        var INNER_CELL_HEIGHT = "16px"; // 33px;
        var TRIM_LEN  =  25;
        var DECIMALS  =  2;

        function InitData() {
            var oData = $get("<% =divObjectivesData.ClientID %>");
            if ((oData)) obj_data = eval(oData[text]);

            var sData = $get("<% =divStepsData.ClientID %>");
            if ((sData)) step_data = eval(sData[text]);

            //alert(obj_data);
            //alert(step_data);
        }

        /* settings */
        var AnyEditMade = "<%=IIf(CallbackEditsMade="1" OrElse IsReturn(),"1","0") %>" == "1";
        var ShowTextBoxes = true;
        var SortByPriority = "<% =IIf(InconsistencySortingEnabled.HasValue AndAlso InconsistencySortingEnabled.Value, "1", "0") %>" == "1";
        var SortByPriorityDir = "<%=InconsistencySortingDir%>";
        var ShowBestFit = false;
        var ShowRanks   = false;
        var ShowLegends = true;

        function InitObjectivesTable(fShowTextBoxes, fSortByPriority, fShowBestFit) {
            ShowTextBoxes = fShowTextBoxes;
            SortByPriority = fSortByPriority;
            ShowBestFit = fShowBestFit;

            var btnRest = $get("btnRestore");
            if ((btnRest)) btnRest.disabled = !AnyEditMade;
            var cbBestFit = $get("cbBestFit");
            if ((cbBestFit)) {
                cbBestFit.checked = ShowBestFit;
                //cbBestFit.visible = !ShowTextBoxes;
            }
            //var cbSort = $get("cbSort");
            //if ((cbSort)) cbSort.checked = SortByPriority;
            var rbReviewCluster = $get("rbReviewCluster");
            if ((rbReviewCluster)) rbReviewCluster.checked = !ShowTextBoxes;
            var rbMultiEdit = $get("rbMultiEdit");
            if ((rbMultiEdit)) rbMultiEdit.checked = ShowTextBoxes;

            InitData();

            var has_any_red_numbers = false;

            var grid = $get("tblJudgments");
            $("#tblJudgments").empty();
    
            if ((grid) && (obj_data) && (step_data)) {
                grid.style.backgroundColor = CELL_BACKGROUND;                

                //init columns headers
                var header = grid.createTHead();
                var hRow = header.insertRow(0);            
                hRow.style.height = ROW_HEADERS_HEIGHT;
                var dc = hRow.insertCell(0); //dummy top left cell
                dc.style.display = "table-cell"; //"inline-block";
                dc.style.height = ROW_HEADERS_HEIGHT;
                dc.style.width = ROW_HEADERS_WIDTH;
                dc.style.border = "1px solid #cccccc";
                dc.style.backgroundColor = HDR_BACKGROUND;
                
                // sort by priority arrows and cancel buttons
                //var sOptions = "<option value='0' " + (!SortByPriority ? "selected" : "" ) + ">No sorting</option>";
                //sOptions += "<option value='1' " + (SortByPriority && SortByPriorityDir == "desc" ? "selected" : "" ) + ">Sort by priority DESC</option>";
                //sOptions += "<option value='2' " + (SortByPriority && SortByPriorityDir != "desc" ? "selected" : "" ) + ">Sort by priority ASC</option>";
                //dc.innerHTML = "<select onchange='sortByChanged(this.value);'>" + sOptions + "</select>";
                var sBtnSort = "<button type='button' class='button' style='width: 100%; text-align: left; padding: 3px 0px 1px 20px;' id='btnSortByPriority' onclick='onSortByPriorityClick();'><i class='fas fa-sort-amount-down' style='vertical-align:middle; padding-bottom:0px;'></i>&nbsp;Sort by priority&nbsp;</button>";
                var sBtnNoSort = "<button type='button' class='button' style='width: 100%; text-align: left; padding: 3px 0px 1px 20px;' id='btnSortByPriority' onclick='onSortByOriginalOrderClick();'><i class='fas fa-list' style='vertical-align:middle; padding-bottom:0px;'></i>&nbsp;Sort by original order&nbsp;</button>";
                dc.innerHTML = sBtnSort + "<br>" + sBtnNoSort;
                
                var obj_data_len = obj_data.length;                
                var step_data_len = step_data.length;

                for (var i=0; i < obj_data_len; i++) {                
                    var dc1 = hRow.insertCell(i + 1);
                    dc1.setAttribute("id", "clmn_hdr_" + i);
                    dc1.style.height = ROW_HEADERS_HEIGHT;
                    //dc1.style.width = DATA_CELL_WIDTH;
                    dc1.style.display = "table-cell"; //"inline-block";
                    dc1.style.textAlign = "center";
                    dc1.style.border = "1px solid #cccccc";
                    dc1.style.fontSize = "12px";
                    dc1.style.backgroundColor = HDR_BACKGROUND;
                    dc1.className = "text-overflow";
                    dc1.innerHTML = "<b>" + ShortString(obj_data[i][IDX_NAME], TRIM_LEN, true) + "</b>";
                    dc1.title = obj_data[i][IDX_NAME] + " : " +  obj_data[i][IDX_VALUE_P];
                }

                // find max priority (individual or combined) to adjust bar widths
                var MaxValue = 0;
                for (var i=0; i < obj_data_len; i++)
                {
                    var v = str2double(obj_data[i][IDX_VALUE]);
                    if (v > MaxValue) MaxValue = v;
                }

                //init rows (headers and content cells)
                for (var i=0; i < obj_data_len; i++)
                {               
                    var obj1_id = parseInt(obj_data[i][IDX_ID]);

                    var tr = grid.insertRow(i + 1);
                    tr.style.height = DATA_CELL_HEIGHT;
                    var rHeader = tr.insertCell(0); //row header
                    rHeader.style.display = "table-cell"; //"inline-block";
                    rHeader.setAttribute("id", "row_hdr_" + i);
                    rHeader.style.height = DATA_CELL_HEIGHT;
                    rHeader.style.width = ROW_HEADERS_WIDTH;
                    rHeader.style.border = "1px solid #cccccc";
                    rHeader.style.fontSize = "12px";
                    rHeader.style.backgroundColor = HDR_BACKGROUND;
                    rHeader.className = "text";
                    var o_value = round(obj_data[i][IDX_VALUE]);
                    var o_name = htmlEscape(obj_data[i][IDX_NAME]);
                    //nodes names only:
                    //rHeader.innerHTML = "<b>" + ShortString(o_name, TRIM_LEN) + "</b>";
                    //nodes names and graph bars:
                    var tMargin = 0;
                    var BHeight  = 8;
                    var BWidth  = ROW_HEADERS_WIDTH_INT - 6;
                    var sColor  = "#1ca9c9"; //"#64d6ed";                    
                    var FillWidth = -1;
                    if (MaxValue == 0) {o_value = 0} else {o_value = o_value / (MaxValue + 0.00000001)};
                    if ((o_value >= 0) && (o_value <= 100)) FillWidth = Math.round((BWidth - tMargin) * o_value) - 1;
                    if (FillWidth < 0) FillWidth = 0;
                    if (FillWidth > BWidth - tMargin) FillWidth = BWidth - tMargin;
                    var sLH = Math.floor(100 * BHeight / 9) + "";
                    var sBG = (FillWidth > 0) && (FillWidth <= BWidth - tMargin) ? " background: url(<% =ImagePath %>prg_bg_white.gif) repeat-y " + FillWidth + "px" : "";
                    //alert(FillWidth + " < " + (BWidth - tMargin).toString());
                    if (FillWidth > 0) sBG += (sBG = ""? " background:": "") + " " + sColor;
                    var sVal = "<b><nobr>&nbsp;" + ShortString(o_name, TRIM_LEN, true) + "</nobr></b>";
                    var sBar = "<div style='font-size:6px; line-height:"+sLH+"%; width:100%; height:100%;"+sBG+"'></div>";
                    rHeader.innerHTML = sVal+"<br><div class='progress' style='display:inline-block;height:"+BHeight+"px;width:"+BWidth+"px;padding:1px;'>" + sBar + "</div>";
                    
                    // tooltip
                    rHeader.title = obj_data[i][IDX_NAME] + " : " + obj_data[i][IDX_VALUE_P];

                    for (var j=0; j<obj_data_len; j++) {                        
                        var tCell = tr.insertCell(j + 1);                        
                        tCell.style.display = "table-cell"; //"inline-block";
                        tCell.style.height = DATA_CELL_HEIGHT;
                        //tCell.style.width = DATA_CELL_WIDTH;
                        tCell.style.border = "1px solid #cccccc";
                        tCell.style.textAlign = "center";
                        if (j <= i)
                        {
                            // diagonal cells
                            tCell.style.backgroundColor = DISABLED_CELL_BACKGROUND;
                            tCell.innerHTML = "&nbsp;";
                        } else {                            
                            var obj2_id = parseInt(obj_data[j][IDX_ID]);
                            var cellTitle = obj_data[i][IDX_NAME] + " : " + obj_data[j][IDX_NAME];
                            tCell.style.backgroundColor = CELL_BACKGROUND;
                            tCell.setAttribute("id", "cell_" + i +"_" + j);
                            tCell.setAttribute("o1", obj_data[i][IDX_ID]);
                            tCell.setAttribute("o2", obj_data[j][IDX_ID]);                           
                            tCell.setAttribute("title", cellTitle);
                            tCell.setAttribute("align", "center");
                            //tCell.setAttribute("class", "dep_cell");                            
                            tCell.style.fontSize = "12px";
                            var cur = "";
                            <% If Not IsReadOnly %>
                            cur = "cursor:pointer; ";
                            tCell.style.cursor     = "pointer";                            
                            tCell.onclick = function () { onCellClick(this) };
                            tCell.onmouseover = function() {this.style.backgroundColor=HIGHLIGHT_COLOR}; 
                            tCell.onmouseout  = function() {this.style.backgroundColor=CELL_BACKGROUND};
                            <% End If %>
                            //find a step pair
                            var Advantage = 1;
                            var BFAdvantage = 1;
                            var pair = null;
                            for (var u=0; u<step_data_len; u++) { 
                                var step_obj1 = parseInt(step_data[u][IDX_OBJ1]);
                                var step_obj2 = parseInt(step_data[u][IDX_OBJ2]);
                                if ((obj1_id == step_obj2) && (obj2_id == step_obj1)) {
                                    Advantage = parseInt(step_data[u][IDX_ADVANTAGE]) * -1;
                                    BFAdvantage = parseInt(step_data[u][IDX_BF_ADV]) * -1;
                                    pair = step_data[u];
                                } else {
                                    if ((obj1_id == step_obj1) && (obj2_id == step_obj2)) {
                                        Advantage = parseInt(step_data[u][IDX_ADVANTAGE]);
                                        BFAdvantage = parseInt(step_data[u][IDX_BF_ADV]);
                                        pair = step_data[u];
                                    }
                                }
                            }
                            if ((pair)) {
                                var s = "&nbsp;";
                                if (pair[IDX_UNDEFINED] == "0") {                                    
                                    var sVal   = getValue(pair[IDX_VALUE]);
                                    var sBFVal = getValue(pair[IDX_BF_VALUE]);
                                    if ((Advantage < 0) && (sVal != 1)) has_any_red_numbers = true;
                                    if (!ShowTextBoxes) { // "review judgments in cluster"
                                        s  = "<label style='" + cur + "height:" + INNER_CELL_HEIGHT + "; width:60%; vertical-align:middle; margin-top:5px; display:inline-block; text-align:right; float:left;" + (((Advantage < 0) && (sVal != 1)) ? "color: Red;" : "") + "'>" + sVal + "</label>"; // Judgment value
                                    } else { // "make changes on this screen"
                                        tCell.style.cursor = "";
                                        tCell.onclick = null;
                                        //s  = "<input type='text' class='input' id='tbJudg_" + obj1_id + "_" + obj2_id +"' style='width:" + (ShowRanks || ShowBestFit ? "60%" : "100%") + "; vertical-align:middle; text-align:right;' value ='' onchange='updateJudgment(this.value, " + obj1_id + ", " + obj2_id + ", this.id, null)' onkeydown='cell_keydown(event, this.value, " + obj1_id + ", " + obj2_id + ", this.id, null);' onkeypress='return noenter();' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onclick='$(this).select();' >"; // Text box: Judgment value
                                        s  = "<input type='text' class='input' id='tbJudg_" +obj1_id + "_" + obj2_id +"' style='width:" + (ShowRanks || ShowBestFit ? "60%" : "80%") + "; vertical-align:middle; text-align:right;" + (((Advantage < 0) && (sVal != 1))?"color:Red;":"") + "' value ='" + (pair[IDX_VALUE] == 0 ? "" : round(pair[IDX_VALUE])) + "' onchange='updateJudgment(this.value, " + obj1_id + ", " + obj2_id + ", this.id, \"" + round(pair[IDX_VALUE]) + "\")' onkeydown='cell_keydown(event, this.value, " + obj1_id + ", " + obj2_id + ", this.id, \"" + round(pair[IDX_VALUE]) + "\");' onkeypress='return noenter();' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onclick='$(this).select();' >"; // Text box: Judgment value
                                    }
                                    if (ShowRanks || ShowBestFit) {
                                        //s += "<div style='" + cur + "float:right; height:" + INNER_CELL_HEIGHT + "; width:40%; text-align:right; font-size:12px;'>";
                                        s += "<div style='" + cur + "float:right; height:30px; width: 20%; text-align:right; font-size:12px;'>";
                                    }

                                    if (ShowRanks) {
                                        s += "<label style='" + cur + "color:Blue; ' title='" + htmlEscape(cellTitle) + "&#013;Rank:&nbsp;" + pair[IDX_RANK]+ "'>" + pair[IDX_RANK] + "</label>"; // Rank
                                    }
                                    
                                    if (ShowRanks || ShowBestFit) {
                                        s += "<br>";
                                    }

                                    if (ShowBestFit) {
                                        if ((BFAdvantage < 0) && (sBFVal != 1)) has_any_red_numbers = true;
                                        s += "<label style='" + cur + (((BFAdvantage < 0) && (sBFVal != 1)) ? "color:Red" : "") + ";' title='" + htmlEscape(cellTitle) + "&#013;Best&nbsp;Fit:&nbsp;" + sBFVal + "'>" + sBFVal+ "</label></div>"; // Best Fit
                                    }
                                    tCell.innerHTML = s;
                                } else {
                                    if (ShowTextBoxes) {
                                        tCell.style.cursor     = "";
                                        tCell.onclick = null;
                                        s  = "<input type='text' class='input' id='tbJudg_" +obj1_id + "_" + obj2_id +"' style='width:100%; vertical-align:middle; text-align:right;' value ='' onchange='updateJudgment(this.value, " + obj1_id + ", " + obj2_id + ", this.id, null)' onkeydown='cell_keydown(event, this.value, " + obj1_id + ", " + obj2_id + ", this.id, null);' onkeypress='return noenter();' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onclick='$(this).select();'>"; // Text box: Judgment value
                                    }
                                    tCell.innerHTML = s;
                                } 
                            } else {
                                tCell.innerHTML = "&nbsp;";
                                if (ShowTextBoxes) { 
                                    tCell.style.cursor     = "";
                                    tCell.onclick = null;                                    
                                }
                            }
                        }
                    }
                }
            }

            // setTimeout('$get("btnPriorities").focus();', 100);            
            $get("lblRedNumbers").style.display = (has_any_red_numbers ? "inline-block" : "none");
        }        

        function focusLastCell() {
            if (last_obj1 >= 0 && last_obj2 >= 0) {
                var o = $get("tbJudg_" + last_obj1 + "_" + last_obj2);
                var o2 = $get("tbJudg_" + last_obj2 + "_" + last_obj1);
                if ((o2)) o = o2;
                if ((o)) {
                    $(o).css({"border": "2px solid " + COLOR_SELECTION_BORDER});
                    o.focus();
                    $(o).select();
                }
            }
        }

        function onCellClick(sender) {
            var obj1 = sender.getAttribute("o1");
            var obj2 = sender.getAttribute("o2");
            if ((!is_context_menu_open) && (sender.innerHTML.length != 0)) {
                //TODO: save judgments before going to step                
                RequireIsReturn(true);
                RequireCallbackOnNext(true);
                
                var stepNum = GetStepNumber(obj1, obj2);
                if (stepNum >= 0) {
                    NavigateToSubPipe(stepNum, obj1, obj2, 0);
                } else {
                    NavigateToExtraStep(parentNodeID, obj1, obj2, 0);
                };     
            }
            is_context_menu_open = false;
        }

        function updateJudgment(value, obj1, obj2, tb_id, orig_value) {
            value = value.trim();
            if ((validFloat(value)) && (round(value) >= 0) && (round(value) != orig_value)) {
                RequireCallbackOnNext(true);
                sendCommand("action=judgment&value=" + value + "&obj1=" + obj1 + "&obj2=" + obj2);
            } else {
                if (value == "" && orig_value != "" && orig_value != 0) {
                    sendCommand("action=judgment&value=" + value + "&obj1=" + obj1 + "&obj2=" + obj2);
                } else {
                    var tb = $get(tb_id);
                    if ((tb)) tb.value = (orig_value == null ? "" : orig_value); 
                    return false;
                }            
            }
        }

        function noenter() {
            return !(window.event && window.event.keyCode == 13); 
        }

        function restoreJudgments() {            
            var mesg = "<%=ResString("msgSureRestoreAllJudgments")%>";
            dxConfirm(mesg, "RequireCallbackOnNext(false);sendCommand('action=restore_all');", "SetActiveView(avInconsistencyTooHigh);");            
        }

        function copyJudgments() {
            //if (document.getElementById('cbSort').checked) {
                //$("#cbSort").prop("disabled", true); 
                sendCommand("action=reset_sorting_and_copy_judgments");
            //}// else {
            //doCopyJudgments();
            //}
        }
        
        function pasteJudgments() {
            //if (document.getElementById('cbSort').checked) {
            //    $("#cbSort").prop("disabled", true);                 
            //}
            doPasteJudgments();
        }

        function doCopyJudgments() {
            var sData = $get("<% =divClipData.ClientID %>");
            if ((sData)) copyDataToClipboard(replaceString("CRLF", "\r\n", sData[text]));
        }
        
        function doPasteJudgments() {
            var data = "";
            if (window.clipboardData) { 
                data = window.clipboardData.getData('Text');
                pasteData(data);
            } else { 
                dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChrome();", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
            };                
        }

        function pasteData(data) {
            if (typeof data != "undefined" && data != "") {
                sendCommand("action=paste_from_clipboard&data=" + encodeURIComponent(data));
            } else { dxDialog("<%=ResString("msgUnableToPaste") %>", "", undefined, "<%=ResString("lblError") %>"); }
        }

        function commitPasteChrome() {
            var pasteBox = $get("pasteBox");
            if ((pasteBox)) {
                pasteData(pasteBox.value);
            }
        }
        /* End Copy/Paste */

        function validFloat(s) {
            return !isNaN(parseFloat(replaceString(",", ".", s + "")));
        }

        function set_pwnl(altID, orig_value) {
            var lbl = $get("edit_pwnl_"+altID);
            var cell = $get("pwnl_"+altID);
            if ((lbl) && (cell)) {
                last_altID = -1;
                last_value = -1.0;
                var f = parseFloat(lbl.value);
                if ((!isNaN(f)) && (f/100 >=0) && (f/100 <= 1) && (f/100 + " %" != orig_value)) {
                    f = f/100;                    
                    cell.innerHtml = f == 0 ? "" : Math.round(f * 100) / 100;
                    sendCommand("action=set_pwnl&value=" + f + "&alt_id=" + altID);
                } else {
                    cell.innerHtml = orig_value;                
                }
            }
        }

        var last_altID = -1;
        var last_value = -1.0;

        function editPwnlClick(altID) {
            if (last_altID != altID) {
                if (last_altID >= 0) {                
                    //save previous edit
                    set_pwnl(last_altID, last_value);
                } else {
                    // start edititng
                    var cell = $get("pwnl_"+altID);
                    if ((cell)) {
                        last_altID = altID;
                        var sValue = replaceString("%", "", cell.innerHTML);
                        //last_value = parseFloat(sValue);
                        last_value = sValue;
                        //if (!isNaN(last_value)) 
                        cell.innerHTML = "<input type='text' id='edit_pwnl_" + altID + "' value='" + cell.innerHTML + "' style='width:95%' class='input' onchange='set_pwnl(" + altID + ", " + last_value + "); return false;' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onclick='$(this).select();'>";
                    }
                }
            }
        }

        //A1070 ===
        function gridDblClickHandler() {            
            return false; // disabled the dbl click handler
            if ($get("PageContent_GridResults")) {
                $get("PageContent_GridResults").ondblclick = function() {
                    var d = $get("<% =divExpectedValue.ClientID %>");
                    if ((d) && (d[text] != "")) {
                        alert(d[text]);
                    }
                };
            }
        }
        //A1070 ==

        var last_obj1 = -1;
        var last_obj2 = -1;

        function cell_keydown(event, value, obj1, obj2, tb_id, orig_val) {
            if (!document.getElementById) return;
            if (window.event) event = window.event;
            if (event) {
                last_obj1 = obj1;
                last_obj2 = obj2;
                var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
                //if (event.ctrlKey)
                switch (code) {
   	                // message how to invert judgment
                    case 111: // divide key
                        var mesg = "<% =JS_SafeString(ResString("msgHowToInvertJudgment")) %>";
                        dxDialog(mesg, ";", ";", resString("titleError"));
                        //event.preventDefault();
                        //event.stopPropagation();
                        return false;
                        break;
                    // invert judgment
                    case 189:  // -
                    case 109:  // subtract
                    case 73:   // i
                        RequireCallbackOnNext(true);
                        sendCommand("action=invert_judgment&obj1="+obj1+"&obj2="+obj2+"&pid="+parentNodeID);
                        //if (event.preventDefault) event.preventDefault();
                        //event.stopPropagation();
                        return false;
                        break;
                    case 13: //Enter
                        updateJudgment(value, obj1, obj2, tb_id, orig_val);
                        //if (event.preventDefault) event.preventDefault();
                        //event.stopPropagation();
                        return false;
                        break;
                }
            }
            return true;
        }

    </script>
</telerik:RadScriptBlock>

<telerik:RadAjaxManager ID="RadAjaxManagerMain" runat="server" ClientEvents-OnRequestError="onRequestError" ClientEvents-OnResponseEnd="onResponseEnd" EnableAJAX="true">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="divGrid">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="divGrid" />
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<%--<EC:LoadingPanel id="pnlLoadingPanel" runat="server" isWarning="true" WarningShowOnLoad="false" WarningShowCloseButton="false" Width="230" Visible="true"/>--%>

<table id='tblIntermediateResults' border="0" cellpadding="0" cellspacing="0" style="height:99%" align="center">
<tr id='tdGrid' class='whole' style='vertical-align:top;'><td>
<telerik:RadCodeBlock ID="RadPaneGrid" runat="server">
<div id='divGrid' runat='server' style='overflow:auto;'><div id='divActiveView' runat="server" style='display:none'></div><%--hidden field to pass ActiveView parameter--%>
    <div runat='server' id='MainPage' style='overflow:auto;' onscroll='$("div.context-menu").hide(200); is_context_menu_open = false;'>

        <div id='divCommand' runat="server" style='display:none'></div> <!-- [command, edits made, sorting enabled ] -->
        <div id='divExpectedValue' runat="server" style='display:none'></div> <!-- hidden field for storing expected value -->
        <div id='divObjectivesData' runat="server" style='display:none'></div>
        <div id='divStepsData' runat="server" style='display:none'></div>
        <div id='divClipData' runat="server" style='display:none'></div>

        <div runat='server' id='ctrlPleaseWait' class='ir_ctrl' style='display:none;'>
            <label runat='server' class='note_msg' id='lblPleaseWait'>Please&nbsp;wait&hellip;</label>
        </div>
        
        <div runat='server' id='ctrlPrioritiesDerived' class='ir_ctrl' style='display:none;'>
            <% ="" %>
            <center>
            <%If Model.IsForAlternatives AndAlso Not Model.InsufficientInfo AndAlso Not App.isRiskEnabled AndAlso Not PM.Parameters.EvalHideLocalNormalizationOptions Then%>
            <div style="margin-bottom:1ex" class="text"><%=ResString("lbl_NormalizationMode")%>&nbsp;<%=GetNormalizationOptions()%></div>
            <%End If%>
            <% If Model.ExpectedValueIndivVisible Then%>
                <label id='lblExpValIndiv' class='text' style='text-align:center; color:#138da4; font-size:larger; cursor:default;'><b><% =ResString("lblExpectedValueIndiv")%></b>&nbsp;<%=Model.ExpectedValueIndivString%></label>&nbsp;&nbsp;&nbsp;
            <%End If%>
            <% If Model.ExpectedValueCombVisible Then%>
                <label id='lblExpValComb'  class='text' style='text-align:center; color:#ffa500; font-size:larger; cursor:default;'><b><% =ResString("lblExpectedValueComb")%></b>&nbsp;<%=Model.ExpectedValueCombString%></label><br />
            <%End If%></center>
            <% If App.isRiskEnabled AndAlso Not Model.IsParentNodeGoal Then%>
                <div id='divShowGlobal' style='text-align:right; margin-right:13px;'><label id='cbShowGlobal' class='text'><input type="checkbox" <% = IIf(ShowGlobalPWNL, "checked", "") %> onclick="sendCommand('action=toggle_global');" />Show global</label></div>
            <% End If%>
            <asp:GridView AutoGenerateColumns="false" EnableViewState="false" AllowSorting="true" AllowPaging="false" ID="GridResults" runat="server" BorderWidth="0" BorderColor="#e0e0e0" CellSpacing="1" CellPadding="0" TabIndex="1" Width="99%">
                <RowStyle VerticalAlign="Middle" CssClass="text grid_row"/>
                <HeaderStyle CssClass="text grid_header actions" />
                <AlternatingRowStyle CssClass="text grid_row_alt" />
                <FooterStyle VerticalAlign="Middle" CssClass="text grid_row" Font-Bold="true" />
                <EmptyDataTemplate><h6 style='margin:8em 2em'><nobr><% =GetEmptyMessage()%></nobr></h6></EmptyDataTemplate>
                <Columns>
                    <asp:BoundField HeaderText="" DataField="IsChecked" /><%--Checkbox column--%>
                    <asp:BoundField HeaderText="No" DataField="Index" />
                    <asp:BoundField HeaderText="Name" DataField="Name" ItemStyle-HorizontalAlign="Left"/>
                    <asp:BoundField HeaderText="Known Likelihood" DataField="AltWithKnownLikelihoodValue" />
                    <asp:TemplateField HeaderText="Participant Results">
                        <headerstyle CssClass="label_my"></headerstyle>
                        <ItemTemplate>
                            <asp:Label ID="lblValue" runat="server" Text='<%# Double2String(CDbl(Eval("Value")) * 100, If(PM.Parameters.SpecialMode = _OPT_MODE_AREA_VALIDATION2, 0, 2), True) %>' /> <!--String.Format("{0:p}", CDbl(Eval("Value"))-->
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Global">
                        <ItemTemplate>
                            <asp:Label ID="lblGlobal" runat="server" Text='<%# Double2String(CDbl(Eval("GlobalValue")) * 100, If(PM.Parameters.SpecialMode = _OPT_MODE_AREA_VALIDATION2, 0, 2), True) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Combined">
                        <ItemTemplate>
                            <asp:Label ID="lblCombined" runat="server" Text='<%# Double2String(CDbl(Eval("CombinedValue")) * 100, If(PM.Parameters.SpecialMode = _OPT_MODE_AREA_VALIDATION2, 0, 2), True) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="GlobalCombined">
                        <ItemTemplate>
                            <asp:Label ID="lblGlobalCombined" runat="server" Text='<%# Double2String(CDbl(Eval("GlobalValueCombined")) * 100, If(PM.Parameters.SpecialMode = _OPT_MODE_AREA_VALIDATION2, 0, 2), True) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="" />                
                </Columns>
            </asp:GridView><br />
            <center>
            <label id='lblExpectedValueLocal' runat="server" class='text' style='text-align:center; margin-bottom:2px; cursor:default;'></label>
            <%If Model.InconsistencyVisible Then%>
                <label id='lblInconsistency' class='text' style='text-align:center; cursor:default;'><b><% =ResString("sInconsistencyRatio")%></b>&nbsp;<%=Model.InconsistencyString%></label><br />
            <%End If%>
            <% If Model.NotInconsistencyBtnVisible Then%>
                <input type='button' class='button' id='btnNotReasonable1' style='width:40em; white-space:wrap; height:2em; margin:2px;' value='<% =ResString("sPrioritiesNotReasonable") %>' onclick='SetActiveView(avElicitSpecificPair); return false;' />
            <% End If%>
            <% If Model.InconsistencyBtnVisible Then%>
                <input type='button' class='button' id='btnNotSatisfactory' style='width:40em; white-space:wrap; height:2em; margin:2px; margin-top:5px;' value='<% =IIf(IsReadOnly, ResString("sJudgmentsTableReadOnly"), ResString("sNotSatisfactory")) %>' onclick='SetActiveView(avQueryWhatsWrong); return false;' />
            <% End If%>
            </center>
            <table id='tblSpecificInfo' border="0" cellpadding="0" cellspacing="0">
            <tr valign='bottom'><td valign="middle" align="center" style='width:100%;'> 
                <div id='ir_hint_msg' style='border:1px solid grey; background:#ffff99; width:450px; height:43px; padding:5px; box-shadow:2px 2px 1px #888888; -moz-box-shadow:2px 2px 1px #888888; -webkit-box-shadow:2px 2px 1px #888888;'>
                    <label id='lblSelectPair' class='text' style='text-align:center; font-size:13px; color:#138da4; white-space:wrap; vertical-align:middle;'><% =ResString("sSelectPairToDo")%></label>
                </div>
            </td>
            <td valign="middle" style='width:170px; text-align:center; padding:0px 12px 0px 0px;'> 
                <input type='button' class='button' id='btnCancel2' style='width:12em; height:1.8em; margin:4px;' value='<% =ResString("btnCancel") %>' onclick='SetActiveView(avPrioritiesDerived); return false;' /><br />
                <input type='button' class='button' id='btnReEval'  style='width:12em; height:1.8em; margin:4px;' value='<% =ResString("btnReEvaluate") %>' onclick='RequireCallbackOnNext(true); SetActiveView(avGotoPairwisePipeStep); return false;' disabled='disabled' />
            </td>
            </tr>
            </table>
        </div>
        <div runat='server' id='ctrlQueryWhatsWrong' class='ir_ctrl' style='display:none;'>
            <input type='button' class='button' id='btnReviewAll'     style='width:40em; white-space:wrap; height:2em; margin:4px;' value='<% =ResString("btnInconsistencyToDo") %>' onclick='SetActiveView(avGotoFirstPairwisePipeStep); return false;' /><br />
            <input type='button' class='button' id='btnTooHigh' style='width:40em; white-space:wrap; height:2em; margin:4px;' value='<% =ResString("sInconsistencyTooHigh") %>' onclick='SetActiveView(avInconsistencyTooHigh); return false;' /><br /><br />
            <center><label class='text'><% =ResString("lblInconsistencySubLabel")%></label></center><br />
            <input type='button' class='button' id='btnNotReasonable2'  style='width:40em; white-space:wrap; height:2em; margin:4px;' value='<% =ResString("sPrioritiesNotReasonable") %>' onclick='SetActiveView(avElicitSpecificPair); return false;' /><br />
            <input type='button' class='button' id='btnAdjustManually' style='width:40em; white-space:wrap; height:2em; margin:4px; display:none;' value='<% =ResString("sPrioritiesAdjustManually") %>' onclick='SetActiveView(avAdjustManually); return false;' /><br />
            <input type='button' class='button' id='btnCancel'     style='width:12em; height:1.7em; margin:4px; float:right;' value='<% =ResString("btnCancel") %>' onclick='SetActiveView(avPrioritiesDerived); return false;' />
        </div>
        <div runat='server' id='ctrlInconsistencyTooHigh' class='ir_ctrl' style='display:none;'>                            
            <table border="0" cellpadding="0" cellspacing="0">
                <tr><td valign="middle" align="center"><label class='text' style='font-size:16px; font-weight:bold; cursor:default;'><% = ResString("lblJudgmentTable")%></label></td></tr>
                <%--TODO: add expander for this row--%><tr><td align='center'><label class='text' style='white-space: wrap; margin-top:4px; display:inline-block; width:70%; cursor:default;'><% = ResString("sInconsistencyToDoTop")%></label></td></tr>
                <tr><td valign="middle" align="center"><table border="0" cellpadding="0" cellspacing="0">
                    <tr valign="top">
                        <%--<td><img src="<% =ThemePath + _FILE_IMAGES %>blank.gif" width="120" height="1" title="" /></td>--%>
                        <td><table id='tblJudgments' border="0" cellpadding="3" cellspacing="0" style='margin:6px auto; width:90%; table-layout:fixed; border-collapse:collapse;'><tr><td><h4>&lt;JUDGMENTS TABLE&gt;</h4></td></tr></table></td>
                        <td align="left" style="padding:8px;"><%--<input type="button" class="button" style="width:200px; margin-bottom:6px;" id="btnRestoreSortOrder" value="Restore Order by Priority" onclick="onSortByPriorityClick();" />--%><%If Model.PWMode = PairwiseType.ptVerbal Then%><div id="legendIntermediateResults" class="text" style="min-width:100px;border:0.5px solid #ccc; background:#ffffdf; padding:1em; text-align:left;"><h6 style="margin-bottom:8px;"><% =ResString("cbLegends") %>:</h6><%=ResString("lblLegendsIntermediateResults")%></div><% End If%><%--<img src="<% =ThemePath + _FILE_IMAGES %>blank.gif" width="100" height="1" title="" />--%></td>
                    </tr>
                    </table>
                </td></tr>
                <%--TODO: add expander for 2 rows--%><% If Not IsReadOnly Then%><tr><td valign="middle" align="center"><span class='text' style='white-space: wrap; display:inline-block; width:70%;'><% = ResString("sInconsistencyToDoBottom")%></span></td></tr><% End If%>
                <tr><td valign="middle" align="center"><span id="lblRedNumbers" class='text' style='white-space: wrap; display:none; width:70%; color:Red; margin:5px 0px;'><% = PrepareTask(ResString(CStr(IIf(Model.IsForAlternatives, "sInconsistencyToDoRedAlts", "sInconsistencyToDoRed"))))%></span></td></tr>
                <%If Model.InconsistencyVisible Then%>
                    <tr><td valign="middle" align="center"><span id='lblInconsistency2' class='text' style='text-align:center; cursor:default;'><b><% =ResString("sInconsistencyRatio")%></b>&nbsp;<%=Model.InconsistencyString%></span></td></tr>
                <%End If%>             
                <tr align="center"><td>
                    <table id='tblInconsistencyTooHigh' border="0" cellpadding="2" cellspacing="2"><tr>
                    <td valign="top" align="left">
                        <label class='text'><input type='checkbox' id='cbRanks' onclick='ShowRanks = this.checked; InitObjectivesTable(ShowTextBoxes, SortByPriority, ShowBestFit);' /><% =ResString("cbRanks")%></label>&nbsp;&nbsp;<br />
                        <%If Model.PWMode = PairwiseType.ptVerbal Then%><label class='text'><input type='checkbox' id='cbLegends' checked="checked" onclick='ShowLegends = this.checked; if (ShowLegends) {$("#legendIntermediateResults").show();} else {$("#legendIntermediateResults").hide();}' /><% =ResString("cbLegends")%></label>&nbsp;&nbsp;<% End If%>
                    </td>
                    <td valign="top" align="left">
                        <label class='text'><input type='checkbox' id='cbBestFit' onclick='InitObjectivesTable(ShowTextBoxes, SortByPriority, this.checked);' /><% =ResString("cbBestFit")%></label>&nbsp;&nbsp;<br />
                        <%--<label class='text'><input type='checkbox' id='cbSort' onclick='this.disabled=true; sendCommand("action=sort_by_priority&do="+(this.checked));' <% IIf(InconsistencySortingEnabled, "checked='checked'", "") %> /><  % =ResString("lblSortAltsPipe")% ></label>&nbsp;&nbsp;--%>
                    </td>
                    <td valign="top" align="left">
                        <% If Not IsReadOnly Then%>
                        <label class='text'><input type='radio' name='rb111' id="rbMultiEdit" checked='checked' onclick="InitObjectivesTable(this.checked, SortByPriority, ShowBestFit);" /><% =ResString("sMultiEdit")%></label>&nbsp;&nbsp;&nbsp;<br />
                        <label class='text'><input type='radio' name='rb111' id="rbReviewCluster" onclick='InitObjectivesTable(!this.checked, SortByPriority, ShowBestFit);' /><% =ResString("sReviewCluster")%></label>&nbsp;&nbsp;&nbsp;
                        <% End If%>
                    </td>
                    <td valign="middle" align="center" style="padding:2px 8px;">
                        <button class='button' type='button' style="cursor:pointer; background-position:2px 2px;" id='btnCopyJudgments'  onclick='copyJudgments();' title="<%=ResString("btnCopyJudgmentsEvalRes") %>"><i class="fas fa-copy"></i></button>
                        <% If Not IsReadOnly Then%>
                        <button class='button' type='button' style="cursor:pointer; background-position:3px 2px;" id='btnPasteJudgments' onclick='pasteJudgments();' title="<%=ResString("btnPasteJudgmentsEvalRes") %>"><i class="fas fa-paste"></i></button>
                        <% End If%>
                    </td>
                    <td valign="middle" align="left">
                        <% If Not IsReadOnly Then%>
                        <input type='button' class='button' id="btnRestore" style='width:12em; height:1.8em;' value='<% =ResString("btnRestoreJudgments")%>' <% IIf(JudgmentsSaved,"","disabled='disabled'") %> onclick='restoreJudgments(); return false;' />
                        <% If Model.IsForAlternatives Then%>
                        <br /><input type='button' class='button' id="btnInvertAllJudgments" style='width:12em; height:1.8em; margin-top:2px;' value='<% =ResString("sInvertAllJudgments")%>' onclick='SetActiveView(avInvertAllJudgments); return false;' />
                        <% End If%>
                        <% End If%>
                    </td>
                    <td valign="middle" align="right" style="padding-left:3em;">                        
                        <input type='button' class='button' id="btnPriorities" style='width:11em; height:2em; font-size:10pt; color:#005292' value='<% =ResString("btnShowPriorities")%>&nbsp;&#9658;' onclick='SetActiveView(avPrioritiesDerived); return false;' /><br />
                    </td>
                    </tr></table>
                </td></tr>
            </table>
        </div>
        <div runat='server' id='ctrlInsufficiencyOfData' class='ir_ctrl' style='display:none; text-align:center;'>
            <h6 style="margin-top:4em;"><% =ResString("sInsufficientData").Replace(vbCr, "<br>")%></h6>
            <% If Model Is Nothing OrElse Model.StepPairs Is Nothing OrElse Model.StepPairs.Count = 0 Then%>
                <input type='button' class='button' id='btnReviewMissingJudgments' style='width:28em; white-space:wrap; height:2em; margin:4px;' value='<% =ResString("btnReviewMissedJudgments") %>' onclick='ReviewMissingJudgments(); return false;' />
            <%End If%>
            </p>
        </div>        
        <div runat='server' id='ctrlReviewJudgments' class='ir_ctrl' style='display:none; width:650px;'>
            <center><label class='note_msg' style='margin:20px 20px;'><% =ResString("msgSameSideJudgments")%></label><br /><br />            
            <input type='button' class='button' id='btnReview' style='width:28em; white-space:wrap; height:2em; margin:4px;' value='<% =ResString("btnReviewJudgments") %>' onclick='SetActiveView(avGotoFirstPairwisePipeStep); return false;' />&nbsp;
            <input type='button' class='button' id='btnContinue' style='width:12em; white-space:wrap; height:2em; margin:4px;' value='<% =ResString("btnContinue") %>' onclick='SetActiveView(ActiveView); return false;' /></center>
        </div>
    </div>
</div>
<script type="text/javascript">
    // SetActiveView(0);
    $(".ir_ctrl").hide(); 
    GetPipeStepDataCompleted();    
</script>
</telerik:RadCodeBlock></td></tr>
</table>
</asp:Panel>
<!-- End of Intermediate Results -->

            <table id='tblHTMLResults' runat='server' border='0' cellpadding='0' cellspacing='0' style="width:100%; height:98%;" visible="false"><tr><td align='center'>
            <% If SHOW_OPTIMIZATION_SWICTH AndAlso GetAction(CurStep).ActionType = ActionType.atShowGlobalResults AndAlso Not IsReadOnly AndAlso Not IsIntensities Then%><div style='float:right; text-align:center; width:95px;'><div class='text small' style='padding:2px 3px; background:#f5f5f5; border:1px solid #f0f0f0;'><a href='?action=optimization<% =GetTempThemeURI(True) %>' onclick='showLoadingPanel();' class='nu'><img src='<% =ImagePath %>clock_.png' width='16' height='16' border='0' style='margin-top:6px; float:left; <% =Iif(App.ActiveProject.ProjectManager.OptimizationOn, "", "opacity: 0.5; filter: alpha(opacity=50);") %>'>Optimization<br /> is <% =IIf(App.ActiveProject.ProjectManager.OptimizationOn, "ON", "OFF")%></a></div><div style='color:#cccccc; margin-top:4px; font-size:7pt' id='divStopWatch'></div></div><% End If%>
            <%--<EC:ShowResults runat="server" ID="GridShowResults" AllowSorting="true" Visible="false"/>--%><asp:PlaceHolder runat="server" ID="placeResults"/></td></tr></table>
                </td></tr>
                <tr runat="server" id="trGoalText" visible="false"><td style='height:1em; padding-top:1ex' class='text' runat="server" id="tdGoalText"></td></tr>
                </table></td>
                </tr>
            </table><asp:HiddenField ID="isChanged" runat="server" />
        </div></td>
    </tr>
    <!-- buttons line -->
    <tr style="height: 1em;" valign="bottom">
        <td align="left"><table border="0" cellpadding="0" cellspacing="0" class="zoom800"><tr><td style="white-space:nowrap;">
            <fieldset class="legend" runat="server" id="pnlNavigation">
            <% If App.ActiveProject.PipeParameters.AllowNavigation Then %><legend class="text legend_title">&nbsp;<% = ResString("lblNavigationBar") %>&nbsp;<asp:Image SkinID="HelpIconRound" runat="server" ID="imgNavHelp"/>&nbsp;</legend><% End If %>
            <div style="padding:0.3ex 0.5ex 0.3ex 0">
                <asp:HiddenField ID="cStep" EnableViewState="true" runat="server" />
                <asp:Image runat="server" ID="imgWhereAmI" SkinID="TreeImage" Visible="false" CssClass="aslink" />
                <asp:Label ID="lblStep" runat="server" cssclass="text nowide900"/><asp:Repeater ID="RepeaterSteps" runat="server">
                <ItemTemplate><asp:Button ID="btnStep" runat="server" Text="#" CssClass="button button_small" CommandName="step" UseSubmitBehavior="false" OnClick="btnStep_Click"/></ItemTemplate>
                </asp:Repeater>
                <asp:Image runat="server" ID="imgStepsList" SkinID="ListImage" Visible="false" CssClass="aslink" />
                <span class="text" style="margin-left:1ex"><asp:Literal runat="server" ID="lblEvaluated"></asp:Literal></span>
             </div>
            </fieldset></td></tr></table>
        </td>
        <td align="left" valign="middle" class="text small zoom800" style="max-width:21em; padding:1ex; white-space:nowrap;">
            <div><asp:CheckBox ID="cbAutoAdvance" runat="server" CssClass="text" Visible="false" AutoPostBack="false" AccessKey="A"/></div>
            <% If IsRealPM AndAlso Not IsReadOnly AndAlso Not IsIntensities Then%><div><nobr><input type="checkbox" id="cbViewAsParticipant" onclick="onSwitchPMMode(this.checked);" <% =IIf(CanUserEditActiveProject, "", "checked") %> /><label for="cbViewAsParticipant"><% = ResString("lblViewAsParticipant") %></label></nobr></div><% End if %>
            <% If USD_OptionVisible Then%><div><nobr><input type="hidden" name="optUSD" value="1" /><input type="hidden" name="totalUSD" value="" /><input type="checkbox" class="checkbox" name="cbUSD" onclick="<% =USD_onClientClick %>" value="1" <% =IIf(USD_ShowCostOfGoal, "checked", "")%> /><label for="cbUSD"><% =String.Format(ResString("lblUSDShowCost"), String.Format("<span id='divUSD'>{0}</span>", "$" + CostString(USD_CostOfGoal))) %></label><% If IsPM Then%>&nbsp;<a href='' onclick='editUSD(); return false;'><img src='<% =ImagePath + "edit_tiny.gif" %>' name='imgEditUSD' width='10' height='10' border='0'/></a><% End If %></nobr></div><% End If%>
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="button" CommandName="step" UseSubmitBehavior="False" Width="8em" Visible="false"/>
        </td>
        <td align="right" id="tdNavBlock" style="white-space:nowrap; width:12%" class="zoom800">
            <div style="padding-bottom: 4px"><asp:Button ID="btnNextUnas" runat="server" Text="Next Unassessed" CssClass="button" CommandName="step" width="12.1em" TabIndex="10" UseSubmitBehavior="False"/></div>
            <div><asp:Button ID="btnFirst" runat="server" Text="Start" CssClass="button" CommandName="step" UseSubmitBehavior="False" Visible="false" Width="6em" TabIndex="4" /><asp:Button ID="btnPrev" runat="server" Text="Prev" CssClass="button" CommandName="step" UseSubmitBehavior="False" Width="6em" TabIndex="3" /><img src="<% =ImagePath %>arrow_down_red_48.png" width="48" height="38" title="Click here to continue" border="0" style="position:absolute; z-index:254; margin-top:-40px; margin-left:12px; display:none;" id='btnClickHere' /><asp:Button ID="btnNext" runat="server" Text="Next" CssClass="button" CommandName="step" TabIndex="1" Width="6em" /><div runat="server" id='divNextHint' class='text small gray' style='width:14em; text-align:center; margin-top:4px; word-wrap: break-word;' visible='false'></div><input type="hidden" name="curstepid" value="<% =CurStep %>"/><asp:HiddenField runat="server" id="isFinish" value=""/></div>
        </td>
    </tr>
</table><script language="javascript" type="text/javascript">
//<!--

    var img_path  = '<% =ImagePath %>';
    var img_comment   = new Image;    img_comment.src   = img_path + 'comment.png';
    var img_comment_  = new Image;    img_comment_.src  = img_path + 'comment_.png';
    var img_comment_e = new Image;    img_comment_e.src = img_path + 'comment_e.png';
    var img_comment_e_= new Image;    img_comment_e_.src= img_path + 'comment_e_.png';

<%--    var pnl_loading_id = "<% =pnlLoadingNextStep.ClientID %>";
--%>

    var img_stop  = new Image;      img_stop.src    = img_path + 'stop.png';
    var img_stop_ = new Image;      img_stop_.src   = img_path + 'stop_.png';

    var img_qh  = new Image;        img_qh.src    = img_path + 'help/icon-question.png';
    var img_qh_ = new Image; img_qh_.src = img_path + 'help/icon-question_dis.png';

    var qh_text_plain = "<% = JS_SafeString(HTML2Text(QuickHelpContent).Trim) %>";

    var load_steps = -1;
    var ask_step_shown = false;
    var show_qh = false;

    var CurrencyThousandSeparator = "<% = UserLocale.NumberFormat.CurrencyGroupSeparator %>";
    var usd_show = <% = IIf(USD_ShowCostOfGoal, 1, 0)%>;
    var usd_cost = <% = USD_CostOfGoal%>;

    function ChangeImage(obj, img)
    {
        if ((obj) && (img)) obj.src = img.src;
        return false;
    }

    function SwitchInfodocMode()
    {
        var j = theForm.jump;
        var btn = $get("<% =btnJump.ClientID %>"); 
        if ((j) && (btn)) 
        {
            theForm.action += (theForm.action.indexOf("?")>0 ? "&" : "?") + "action=infodoc_mode";
            j.value = '<% =CurStep %>';
            SetNextStep(j.value*1);
            btn.click();
        }
        return false;
    }

    function ConfrimCancelIntensities()
    {
        var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgCancelIntensities")) %>", resString("titleConfirmation"));
        result.done(function (dialogResult) {
            if (dialogResult) {
                onFinishIntensities();
            }
        });
        return false;
    }

    function onFinishIntensities() {
        alert_on_exit = 0; 
        var w = ((window.opener) || (window.parent));
        if (w && (typeof w.onEndEvaluateIntensities == 'function')) w.onEndEvaluateIntensities(false); else document.location.href = "<%= JS_SafeString(PageURL(_PGID_SERVICEPAGE, _PARAM_ACTION + "=intensities" + GetTempThemeURI(True)))%>";
    }

    var jump_visible = 0;
    
    function ConfirmMissingJudgment()
    {
        <% If GetCookie(_COOKIE_NOVICE_MISSING, "") < "3" Then%>if (NeedCheckStep())
        {
            if (!confirm('<% =JS_Safestring(ResString(CStr(iif(isMultiEvaluation, "msgMissingJudgmentMultiEval", "msgConfirmMissingJudgment")))) %>')) return false;
            document.cookie="<% =_COOKIE_NOVICE_MISSING %>=" + (1*'<% =JS_SafeString(GetCookie(_COOKIE_NOVICE_MISSING, "0")) %>'+1)+";path=/;expires=Thu, 31-Dec-2037 23:59:59 GMT;";
        }<% End If %>
        return true;
    }
    
    // D1170 ===    
    function ShowJumpTooltip(ctrl_id)
    {
        var t = $find("<% =ttJump.ClientID %>");
        if (t)
        {
           if (ctrl_id!="") t.set_targetControlID(ctrl_id);
           t.show();
           jump_visible = 1;
           setTimeout('theForm.jump.focus();', 200);
        }
    }
    
    function HideJumpTooltip()
    {
        var t = $find("<% =ttJump.ClientID %>");
        if (t) t.hide();
        jump_visible = 0 ;
        return false;
    }
    
    function DoJump()
    {
        var res = false;
        var j = theForm.jump;
        jump_visible = 0;
        var max = <% =GetMaxStepNumber() %>;
        if ((j))
        {
            var s = j.value*1;
            if (s>0 && s<=max)
            {
                SetNextStep(s);
                res = true;
            }
            else
            {
                dxDialog("<%=JS_SafeString(String.Format(ResString("msgWrongEvaluationJumpStep"), GetMaxStepNumber())) %>", "theForm.jump.focus()", undefined, resString("titleError")); 
                res = false;
            }
        }
        if (res) HideJumpTooltip();
        return res;
    }
    // D1170 ==
    
    function isAutoAdvance()
    {
        <% if cbAutoAdvance.visible Then  %>if (theForm.<% =cbAutoAdvance.ClientID %>) return (theForm.<% =cbAutoAdvance.ClientID %>.checked);
        <% End if %>return <% =Cint(iif(isGPWStep OrElse Not isAutoAdvance, 0, 1)) %>;
    }

    function ComfirmEnableAutoadvance()
    {
        document.cookie = "<% = COOKIE_CHECK_AUTOADVANCE + App.ProjectID.ToString() %>=-1;";
        // don't ask about the auto advanced per EF request
        var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgEnabledAutoadvance")) %>", resString("titleConfirmation"));
        result.done(function (dialogResult) {
            if (dialogResult) {
                theForm.<% =cbAutoAdvance.ClientID %>.checked=true;
            }
        });
    }
    
    function CheckParentAndShowClose()
    {
        if (!(window.parent) || (typeof (window.parent.fixC1HTMLHostEvent) != "function" )) $("#lblReadOnlyCloseTab").show();
    }

    function CantFindStep(node)
    {
        DevExpress.ui.dialog.alert(replaceString('{0}', node, '<% = JS_SafeString(ResString("msgCantStepWithResults")) %>'), "Message");
    }

    function onGetDirectLink() {
        return (document.location.href.indexOf("step")>0 ? "" : "<% = _PARAM_STEP + "=" + CurStep.ToString %>" + document.location.href.indexOf("&id=")>0 ? "" : "<% = if(IsReadOnly, "&id=" + ReadOnly_UserID.ToString, "") %>");
    }

    function Hotkeys(event) {
        if (!document.getElementById) return;
        if (window.event) event = window.event;
        if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);	// D0073
            if (event.ctrlKey && !event.altKey && !event.shiftKey && code == 37) {
                    if ((theForm.<% =btnPrev.ClientID %>) && !(theForm.<% =btnPrev.ClientID %>.disabled)) theForm.<% =btnPrev.ClientID %>.click();
            }
            if (event.ctrlKey && !event.altKey && !event.shiftKey && code == 39) {
                if ((theForm.<% =btnNext.ClientID %>) && !(theForm.<% =btnNext.ClientID %>.disabled)) theForm.<% =btnNext.ClientID %>.click();
            }
            if (event.ctrlKey && !event.altKey && event.shiftKey && code == 51) ShowJumpTooltip("tdNavBlock");  // Ctl+Shift+3 (Ctrl+#)
            if (code == 13 && jump_visible) { var btn = $get("<% =btnJump.ClientID %>"); if ((btn)) btn.click(); return false; }
        }
    }
    
    function NextStep()
    {
        if ((theForm.<% =btnNext.ClientID %>) && !(theForm.<% =btnNext.ClientID %>.disabled)) theForm.<% =btnNext.ClientID %>.click();
    }

    function ClientNodeClicking(sender, eventArgs)
    {
     var node = eventArgs.get_node();
     if(node.get_value() <=0)
      {
          eventArgs.set_cancel(true);
      }
    }

    function open_infodoc_edit(url) {
        CreatePopup(url + '<% =GetTempThemeURI(True) %>', 'win20199', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=' + dlgMaxWidth(840) + ',height=' + dlgMaxHeight(500)); return false;
    }
    
    function open_infodoc(url)
    {
        CreatePopup(url, '', 'menubar=no,maximize=no,titlebar=yes,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=' + dlgMaxWidth(800) + ',height=' + dlgMaxHeight(560));
        return false;
    }

    function onRichEditorRefreshQH(empty, infodoc, callback)
    { 
        window.focus();
        ChangeImage($get("imgQH"), ((empty=="1") ? img_qh_ : img_qh));
        if ((callback) && (callback.length>1) && (callback[0]!="") && (callback[0])) {
            var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("confQHPreview")) %>", resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    setTimeout('$(\"#lnkQHVew\")[0].click();', 350);
                }
            });
        }
    }
    
    function DoFlashWRT(wrt_object)
    {
        var del = 300;
        var per = 6;
        var o = $("#" + wrt_object);
        var w = $("#wrt_name");
        for (var i=0; i<per; i++)
        {
          if ((o)) o.animate({backgroundColor: "#ffff99"}, del);
          if ((w)) w.animate({color: "#003366"}, del);
          if ((o)) o.animate({backgroundColor: "#ffffff"}, del);
          if ((w)) w.animate({color: "#0066ff"}, del);
        }
    }

    function DoFlashWRTLink(wrt_object)
    {
        var del = 250;
        var per = 8;
        var o = $("#" + wrt_object);
        var w = $("#wrt_link");
        for (var i=0; i<per; i++)
        {
          if ((o)) o.animate({backgroundColor: "#ffff99"}, del);
          if ((w)) w.animate({color: "#000033"}, del);
          if ((o)) o.animate({backgroundColor: "#ffffff"}, del);
          if ((w)) w.animate({color: "#0066ff"}, del);
          if ((o)) o.animate({backgroundColor: "#f0f0f0"}, del);
      }
    }
    
    function DoFlashWRTPath()
    {
        $("#wrt_path").animate({color: "#3399ff"}, 2500).animate({color: "#e0e0e0"}, 1500).fadeOut(500).animate({color: "#0066ff"}, 150);
    }

    function ToggleWRTPath()
    {
        $("#wrt_path").each(function() { $(this).toggle(); });
//        $("#wrt_path").slideToggle();
    }

    function setObjectName(name, t2s_text) {
        var ph = $("#evalObject");
        if ((ph) && (ph.length) && (ph.html() != name)) {
            ph.html(name);
            <% If showT2S Then %>if (('speechSynthesis' in window)) {
                if ((window.speechSynthesis.speaking)) {
                    window.speechSynthesis.cancel();
                }
                if ((t2s_auto)) setTimeout(function () { doT2S(t2s_text); }, 200);
            }<% End if %>
            setTimeout(function() {
                ph.addClass("blink_twice");
                setTimeout(function () {
                    ph.removeClass("blink_twice");
                }, 2000);
            }, 300);
        }
        $("#templ_evalObject").text(name);
        if (theForm.txtEvalObject) theForm.txtEvalObject.value = name;
    }

    var task_editor_active = -1;
    var task_text_prev = "";

<% If divEditTask.Visible Then %>   
    //setTimeout("SwitchTaskEditor(1);", 1000);

    var jHTMLAreas = [["<% =lblTask.ClientID %>", "txtTask", "<% = RadToolTipHints.ClientID %>", "<% =RadToolTipClusterSteps.ClientID %>", "SaveNewTask", "btnTaskEditor", "divClusterPhrase", "TaskNodesList", "task_guid_"],
                      ["<% =lblResultsTitle.ClientID %>", "txtTitle", "<% = RadToolTipTitleHints.ClientID %>", "<% =RadToolTipTitleSteps.ClientID %>", "SaveNewTitle", "btnTitleEditor", "divClusterTitle", "TitleNodesList", "title_guid_"]];

    var jh_idx_lbl = 0;
    var jh_idx_textarea = 1;
    var jh_idx_hints = 2;
    var jh_idx_steps = 3;
    var jh_idx_save_val = 4;
    var jh_idx_btn = 5;
    var jh_idx_editor = 6;
    var jh_idx_nodes = 7;
    var jh_idx_guid_prefix = 8;

    function SwitchTaskEditor(edit_idx)
    {
        var ids = jHTMLAreas[edit_idx];
        var btn = $("#" + ids[jh_idx_btn]);
        var ed =  $("#" + ids[jh_idx_editor]);
        var lbl = $("#" + ids[jh_idx_lbl]);

        if ((btn) && (lbl) && (ed) && (btn.length) && (lbl.length) && (ed.length))
        {
            var vis = !(ed.is(":visible"));
            //jDialog_HideSliders(vis);
            if (vis)
            {
                if (task_editor_active>=0 && task_editor_active!=edit_idx) SwitchTaskEditor(task_editor_active);
                lbl.hide();
                btn.hide();
                ed.show();
                <% If showT2S Then %>$("#divT2S").hide();<% End if %>
            }
            else
            {
                ed.hide();
                btn.show();
                lbl.show();
                <% If showT2S Then %>$("#divT2S").show();<% End if %>
            }

            var ta = $("#" + ids[jh_idx_textarea]);
            task_editor_active = (vis ? edit_idx : -1);
            if (task_editor_active>=0) window.onbeforeunload = "";
            if ((vis) && (ta) && (ta.length))
            {
                task_text_prev = ta.val();
                ta.focus();
                $("#" + ids[jh_idx_textarea]).htmlarea({
                    toolbar: [ "bold", "italic", "underline", "|",
                               "forecolor", "increasefontsize", "decreasefontsize", "|", 
                               "cut", "copy", "paste", 
                               { css: 'custom_clean', text: '<% = JS_SafeString(ResString("lblEditTaskResetStyles")) %>', action: function (btn) { this.textarea.htmlarea('removeFormat'); } },
                               "|", 
                               "justifyleft", "justifyright", "justifycenter", "indent", "outdent", "|",
                               "link", "unlink",
                               { css: 'custom_br', text: '<% = JS_SafeString(ResString("lblEditTaskBR")) %>', action: function (btn) { this.pasteHTML("<br>"); } },// "|",
                               { css: 'custom_template', text: '<% = JS_SafeString(ResString("lblEditTaskTemplates")) %>', action: function (btn) { SwitchTemplates(this, -1); } },
                               //"|",
                               { css: 'custom_reset', text: '<% = JS_SafeString(ResString("btnEditTaskReset")) %>', action: function (btn) { ConfrimResetTask(this); } },
                               { css: 'custom_apply', text: '<% = JS_SafeString(ResString("btnEditTaskOK")) %>', action: function (btn) { SaveTask(this, false); } },
                               { css: 'custom_apply_to', text: '<% = JS_SafeString(ResString("btnApplyTaskTo")) %>', action: function (btn) { ShowTaskClusters(this, 1); } },
                               { css: 'custom_cancel', text: '<% = JS_SafeString(ResString("btnCancel")) %>', action: function (btn) { onCancelEditor(this); } }
                               ] ,
                    css: '<% =ApplicationURL(False, False) + _URL_THEMES %>jHTMLArea/' + (edit_idx ? 'jHtmlArea.Editor.white.css' : 'jHtmlArea.Editor.css') + '?r<% =Now.Ticks %>'
                }).data("idx", edit_idx);

                $(".custom_template").html("<% = JS_SafeString(ResString("btnTaskTemplates")) %>");
                $(".custom_reset").html("<% = JS_SafeString(ResString("btnEditTaskReset")) %>");
                $(".custom_apply").html("<% = JS_SafeString(ResString("btnEditTaskOK")) %>");
                $(".custom_apply_to").html("<% = JS_SafeString(ResString("btnApplyTaskTo")) %>");
                $(".custom_cancel").html("<% = JS_SafeString(ResString("btnCancel")) %>");
                <% If Not CanShowApplyTo Then%>if (edit_idx == 0) $(".custom_apply_to").hide();<% End if %>
                <% If Not CanShowTitleApplyTo Then%>if (edit_idx == 1) $(".custom_apply_to").hide();<% End if %>

                var d = jHtmlArea($("#" + ids[jh_idx_textarea])[0]);
                if ((d)) d.iframe[0].contentWindow.focus();
            }
            if (task_editor_active<0) setTimeout("window.onbeforeunload = OnExitPage;", 200);
            if (edit_idx == 1 && typeof(onResultsResized) == "function") onResultsResized();
        }
    }

    function onCancelEditor(obj) {

        if ((obj) && (obj.textarea)) {
            var ta = obj.textarea;
            jHtmlArea(ta).html(task_text_prev); 
            ta.val(task_text_prev); 
            SwitchTemplates(obj, 0);
            var idx = ta.data("idx");
            SwitchTaskEditor(idx);
        }
    }

    function ShowTaskClusters(obj, vis)
    {
        var idx = obj.textarea.data("idx");
        var t  = $find(jHTMLAreas[idx][jh_idx_steps]);
        if ((t))
        {
            if ((t.isVisible() && vis==-1) || vis==0) t.hide(); else t.show();
        }
    }

    function SwitchTemplates(obj, vis)
    {
        //        AvoidWrongExit();
        var idx = obj.textarea.data("idx");
        var t  = $find(jHTMLAreas[idx][jh_idx_hints]);
        if ((t))
        {
            if ((t.isVisible() && vis==-1) || vis==0) {
                t.hide(); 
            } else { 
                t.show();
                if (theForm.txtEvalObject) {
                    if (theForm.txtEvalObject.value == "") {
                        $("#tplEvalObject").hide();
                    } else {
                        $("#tplEvalObject").show();
                        $("#templ_evalObject").text(theForm.txtEvalObject.value);
                    }
                }
            }
        }
    }

    function InsertTemplate(idx, txt) {
        var ta = $("#"+jHTMLAreas[idx][jh_idx_textarea]);
        if ((ta) && (ta.length)) {
            ta.htmlarea("pasteHTML", txt);
            SwitchTemplates(jHtmlArea(ta), 0);
        }
        return false;
    }

    function getPreviewTemplate(txt) {
        var res = txt;
        //if (theForm._tpl_count) {
        //    var cnt = 0;
        //    cnt = theForm._tpl_count.value  * 1;
        //    for (var i=1; i<=cnt; i++) {
        //        var tpl = $("#_tpl_" + i);
        //        var val = $("#_val_" + i);
        //        if ((tpl) && (tpl.length) && (val) && (val.length)) {
        //            res = replaceString(tpl.val(), val.val(), res);
        //        }
        //    }
        //}
        return res;
    }

    function AvoidWrongExit()
    {
        var w = show_wait;
        var a = alert_on_exit;
        show_wait = false;
        alert_on_exit = false;
        setTimeout("show_wait = '" + w + "'; alert_on_exit = '" + a + "';", 500);
    }

    function ConfrimResetTask(obj) {
        SwitchTemplates(obj, 0);        
        dxConfirm("<% =JS_SafeString(ResString("msgResetCustomPhrase")) %>", "SaveTaskByID(" + obj.textarea.data("idx") + ", true);", ";");
    }

    function SaveTaskByID(idx, DoReset) {
        var ta = $("#"+jHTMLAreas[idx][jh_idx_textarea]);
        var obj = jHtmlArea(ta);
        SaveTask(obj, DoReset);
    }

    function SaveTask(obj, DoReset)
    {
        var idx = obj.textarea.data("idx");
        SwitchTaskEditor(idx);
        var ta = $("#"+jHTMLAreas[idx][jh_idx_textarea]);
        var sTask = "";
        if (!(DoReset)) sTask = jHtmlArea(ta).html(); else jHtmlArea(ta).html("");
        ta.val(sTask);
        task_text_prev = sTask;
        $("#" + jHTMLAreas[idx][jh_idx_save_val]).val(1);
        ShowTaskClusters(obj, 0);
        SwitchTemplates(obj, 0);
        <% If isSLTheme Then %>UpdateSLProject();<% end If %>
        <%--        if (cur_step_btn=="") {
            __doPostBack('', '');
        }
        else {
            SetNextStep(<% =CurStep %>);
            eval("theForm." + cur_step_btn + ".click();");
        }--%>
        if (sTask!="")
        {
            var lbl_id = jHTMLAreas[idx][jh_idx_lbl];
            var lbl  =$("#" + lbl_id);
            if ((lbl) && (lbl.length)) { lbl.html(getPreviewTemplate(sTask)); DoFlashWRT(lbl_id); }
        }
        showLoadingPanel();
        setTimeout(function() { S(<% =CurStep%>); }, 30);
        return false;
    }

    function SaveTaskMulti(idx)
    {
        var lst = "";
        for (var i=1; i<=<% =App.ActiveProject.Pipe.Count %>;i ++)
        {
            var c = eval("theForm." + jHTMLAreas[idx][jh_idx_guid_prefix] +  i);
            if ((c) && (c.checked)) lst += (lst=="" ? "" : ",") + c.value;
        }
        if (lst!="") eval("theForm." + jHTMLAreas[idx][jh_idx_nodes]).value = lst;
        SwitchTaskEditor(idx);
        SaveTaskByID(idx, false);
    }

<% End if %>

    var ts2s_avail = <% =Bool2JS(showT2S) %>;
    var t2s = null;
    var can_autoplay = <% =IIf(App.isMobileBrowser, "false", "true; //!(is_chrome)") %>;
    var t2s_auto = <% =Bool2JS(GetCookie(COOKIE_T2S_MODE, CStr(IIf(Request.IsLocal, "0", IIf(App.ActiveProject.ProjectManager.Parameters.EvalTextToSpeech = ecText2Speech.AutoPlay, "1", "0")))) = "1") %>;

    function initT2S() {<% If showT2S Then %>
        var div_t2s = $("#divT2S");
        if ((div_t2s) && (div_t2s.length)) {
            div_t2s.show();
            if ('speechSynthesis' in window) {
                div_t2s.html("<a href='' onclick='doT2S(); return false;' class='actions'><i class='far fa-play-circle'  style='font-size:1.33rem; margin-right:8px; margin-top:-3px;' title='Play/Pause' id='btnT2SPlay'></i>" + "<a href='' onclick='toggleT2S(); return false;' class='_actions'><i class='fas " + (t2s_auto ? "fa-volume-down" : "fa-volume-mute") + "' title=\"Switch auto-play\" id='btnT2SAuto'></i></a>");
                if (!can_autoplay) setTimeout(function () {
                    $("#btnT2SPlay").addClass("blink_twice");
                }, 3000);
                <% If tdTask.Visible AndAlso lblTask.Text <> "" Then %>if (!can_autoplay && t2s_auto) {
                    var task = $("#<% = lblTask.ClientID %>");
                    //if ((task) && (task.length) && task.text()!="") {
                    //    setTimeout(function () {
                    //        if (!(window.speechSynthesis.speaking)) DevExpress.ui.notify("Your browser doesn't support auto-play. Please click the play button.", 'warning', 3000);
                    //    }, 1750);
                    //}
                }<% End If %>
                t2s = new SpeechSynthesisUtterance();
                //var voices = window.speechSynthesis.getVoices();
                //t2s.voice = voices[1]; 
                //t2s.volume = 1; // From 0 to 1
                t2s.rate = 0.75; // From 0.1 to 10
                t2s.pitch = 0.8; // From 0 to 2
                t2s.lang = "<% =JS_SafeString(ResString("_LanguageCode")) %>";
                t2s.onstart = function () {
                    var qh_btn = $("#btnQHPlayTS");
                    if ((qh_btn) && (qh_btn.data("dxButton")) && (qh_t2s_play)) {
                        qh_btn.addClass("blink_me").dxButton("option", { "icon": "fas fa-pause", text: "" });
                        var qh_stop = $("#btnQHStopTS");
                        if ((qh_stop) && (qh_stop.data("dxButton"))) {
                            qh_stop.dxButton("option", { "disabled": false });
                        }
                    } else {
                        $("#btnT2SPlay").addClass("blink_me").removeClass("fa-play-circle").addClass("fa-pause-circle");
                    }
                };
                t2s.onend = function () {
                    var qh_btn = $("#btnQHPlayTS");
                    if ((qh_btn) && (qh_btn.data("dxButton"))) {
                        qh_btn.removeClass("blink_me").dxButton("option", { "icon": "fas fa-play", text: "" });
                        qh_t2s_play = false;
                        qh_t2s_in_pause = false;
                    }
                    var qh_stop = $("#btnQHStopTS");
                    if ((qh_stop) && (qh_stop.data("dxButton"))) {
                        qh_stop.dxButton("option", { "disabled": true });
                    }
                    $("#btnT2SPlay").removeClass("blink_me").addClass("fa-play-circle").removeClass("fa-pause-circle");
                };

                if (t2s_auto && can_autoplay) {
                    if (localStorage.getItem("not2s") != "1") {
                        setTimeout(function() {
                          if (!(window.speechSynthesis.speaking)) doT2S();
                        }, 1500);
                    } else {
                        localStorage.removeItem("not2s");
                    }
                }
            } else {
                var msg_unavail = "Sorry, your browser doesn't support the speaker mode.";
                div_t2s.html("<i class='fas fa-volume-mute gray' style='font-size:1.0rem' title=\"" + msg_unavail + "\"></i>");
            }
        }<% End If %>
    }

    function doT2S(text) {<% If showT2S Then %>
        var task = $("#<% = lblTask.ClientID %>");
        if ((t2s) && (t2s!=null) && (task) && (task.length) && ('speechSynthesis' in window)) {
            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
            } else {
                if (typeof text == "undefined" || text=="") text = task.text();
                t2s.text = text;
                window.speechSynthesis.speak(t2s);
            }
        }<% End If %>
    }

    function toggleT2S() {<% If showT2S Then %>
        t2s_auto = !t2s_auto;
        document.cookie = "<% =COOKIE_T2S_MODE %>=" + (t2s_auto ? "1" : "0") + ";path=/;expires=Thu, 31-Dec-2037 23:59:59 GMT";
        if (t2s_auto) {
            $("#btnT2SAuto").addClass("fa-volume-down").removeClass("fa-volume-mute");
            doT2S();
        } else {
            $("#btnT2SAuto").removeClass("fa-volume-down").addClass("fa-volume-mute");
            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
            }
        }<% End If %>
    }

    var timeout_show = 1;
    function ShowTimeoutWarning() {
    <% If Not IsReadOnly Then %>
        if (timeout_show) {
            timeout_show = 0;
            if (t2s_auto) localStorage.setItem("not2s", "1");
            if ((theForm.<% =isChanged.ClientID %>.value>'0')) {
                S(<% =CurStep%>);
            } else {
                document.location.reload(true);
            }

<%--            var time = new Date();
            var msg = '<% =GetTimeoutMessage() %>  ' + time.toLocaleString() + ' \n\n';
            if (cur_step_btn=="") {
                dxDialog(msg + '<%= JS_SafeString(ResString("msgTimeoutWarningAction")) %>', null, null, "<% = ResString("lblWarning") %>", 530, 300);
            }
            else {
                if (confirm(msg + '<% = JS_SafeString(ResString(CStr(IIf(isEvaluation(CurAction), "msgSaveJudgments", "msgPageRefresh")))) %>')) { 
                    SetNextStep(<% =CurStep %>);
                    var b =  eval("theForm." + cur_step_btn); 
                    if ((b)) b.click();
                }
            }--%>
        }
    <% End If %>        
    }
    
    function OnExitPage(is_logout)
    {        
        <% If showT2S Then %>if (('speechSynthesis' in window)) {
            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
            }
        }<% End if %>        
        if  (!(alert_on_exit) && (show_wait))
        {
            var t = $find("<% =RadToolTipStepsList.ClientID %>");
            if ((t)) t.hide();
            var r = $get("tblIntermediateResults");
            if ((r)) r.style.display = "none";
            var p = $get("<% =pnlEvaluate.ClientID %>");
            if ((p)) p.style.display = "none";
            showLoadingPanel();
        }
        <% If Not IsReadOnly Then %>
        if ((alert_on_exit) && theForm.<% =isChanged.ClientID %>.value>'0') {
            _loadPanelPersist = false;
            hideLoadingPanel();
            return ((is_logout) ? '<% =JS_SafeString(ResString("msgExitUnsavedEvaluation")) %>' : '<% =JS_SafeString(ResString("msgUnloadUnsavedEvaluation")) %>');               
        }
        <% End If %>
    }

<% If IsReadOnly Then %>var check_form = true; 
    function checkROForm() {
        if ((check_form) && (theForm.<% =isChanged.ClientID %>) && (theForm.<% =isChanged.ClientID %>.value>0)) {
            check_form = false;
            var a = DevExpress.ui.dialog.alert("<% = JS_SafeString(ResString("lblReadOnlyMode")) %>",  "<% = JS_SafeString(ResString("lblWarning")) %>");
            a.done(function () {
                S(step_next);
            });
            return false;
        }
    }        
<% End If %>
    function CheckExitEvent(is_logout)
    {
        <% If IsReadOnly Then %>alert_on_exit = false; return true;<% End If %>
        timeout_show = 0;
        <%--var r = $get("tblIntermediateResults");
        if ((r) && r.style.display != "none")
        { 
            r.style.display = "none";
            SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 1);
            document.location.href = "<% =_URL_ROOT %>dummy.htm";
        }--%>
        if ((alert_on_exit) && theForm.<% =isChanged.ClientID %>.value>'0')
        {
            var sw = show_wait;
            show_wait = false;
            var res = confirm(OnExitPage(is_logout));
            show_wait = sw;
            if (res) alert_on_exit = false;
            return res;
        }
        else 
        {
            if ((is_logout)) return ""; else return true;
        }
    }

    function ShowNotCompleted()
    {
        var btn = theForm.<% =btnNextUnas.ClientID %>;
        if ((btn) && !btn.disabled && NeedCheckStep())
        {
            var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgNotCompletedEvaluation")) %>", resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    theForm.<% =btnNextUnas.ClientID %>.click();
                }
            });
            
        }
    }

    function AskOnOpen(step) {
        var conf_options = {
            "title": "<% =JS_SafeString(ResString("titleEvalAskOnOpen")) %>",
            "messageHtml": "<% =JS_SafeString(ResString("confEvalAskOnOpen")) %>", 
            "buttons": [{
                text: "<% =JS_SafeString(ResString("btnFirstUnassessed")) %>",
                onClick: function () {
                    S(step);
                }
            }, { 
                text: "<% =JS_SafeString(ResString("btnStayHere")) %>",
                onClick: function () {
                    //$(this).dialog("close"); 
                    if (show_qh) { 
                        ask_step_shown=false;
                        setTimeout('ShowIdleQuickHelp();', 500); 
                    }
                }
            }, { 
                text: "<% =JS_SafeString(ResString("btnBeginning")) %>",
                onClick: function () {
                    S(1);
                }            
            }]
        };
        var a = DevExpress.ui.dialog.custom(conf_options);
        a.show();
    }

<%--    function initAskOnLoadButtons(step) {
        $(jDialog_ID).dialog("option", "buttons", [
            {
                text: "<% =JS_SafeString(ResString("btnFirstUnassessed"))%>",
                tabIndex:   -1,
                click: function() { $(this).dialog("close"); S(step); }
            },
            {
                text: "<% =JS_SafeString(ResString("btnStayHere")) %>",
                tabIndex:   1,
                click: function() { $(this).dialog("close"); if (show_qh) { ask_step_shown=false; setTimeout('ShowIdleQuickHelp();', 500); }  }
            },
            {
                text: "<% =JS_SafeString(ResString("btnBeginning")) %>",
                tabIndex:   -1,
                click: function() { $(this).dialog("close"); S(1); }
            }
        ]);
    }--%>

<% If CurAction IsNot Nothing AndAlso CurAction.ActionType = ActionType.atEmbeddedContent AndAlso CurAction.EmbeddedContentType = EmbeddedContentType.AlternativesRank Then %>
    
    var rank_width = -1;        
    function initAltsRank() {
        if ((alt_ranks)) {
            $("#divAltsRank").css({"min-width": (dlgMaxWidth(200) - 24) + "px;" });
            $("#divAltsRank").dxDataGrid({
                width: "100%",
                columns: [{ "caption" : "Rank", "alignment" : "right", "allowSorting" : false, "allowSearch" : false, "visible" : true, "dataField" : "urank", "encodeHtml" : false, width: "60px",
                            //customizeText : function (cellInfo) {
                            //    return cellInfo.value*1 > 0 ? cellInfo.value : "";
                            //}
                          }, { "caption" : "Name", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, "visible" : true, "dataField" : "title", "encodeHtml" : false},],
                dataSource: alt_ranks,
                columnAutoWidth: true, 
                paging: {
                    enabled: false
                },
                showBorders: true,
                noDataText: function() { 
                    var msg = "<% =SafeFormString(ParseString("No %%alternatives%% selected, return to the previous step"))%>";
                    var d = $(".dx-datagrid-nodata");
                    if ((d) && (d.length)) {
                        d.parent().find(".dx-scrollable-wrapper").hide();
                        d.html("<div class='text' style='padding:1em;'>" + msg + "</div>");
                    } else {
                        return msg;
                    }
                },
                rowDragging: {
                    allowDropInsideItem: false,
                    allowReordering: true,
                    dropFeedbackMode: "indicate",
                    onReorder: function(e) {
                        var visibleRows = e.component.getVisibleRows(),
                            toIndex = alt_ranks.indexOf(visibleRows[e.toIndex].data),
                            fromIndex = alt_ranks.indexOf(e.itemData);

                        alt_ranks.splice(fromIndex, 1);
                        alt_ranks.splice(toIndex, 0, e.itemData);

                        var ids = "";
                        for (var i = 0; i < alt_ranks.length; i++) {
                            alt_ranks[i]["urank"] = i + 1;
                            ids += (ids == "" ? "" : ",") + alt_ranks[i]["id"];
                            //ids.push(alt_ranks[i]["id"]);
                        }
                        if ((theForm.altRanks)) theForm.altRanks.value = ids;
                        <%--if (theForm.<% =isChanged.ClientID %>) theForm.<% =isChanged.ClientID %>.value = 1;--%>

                        e.component.refresh();
                        resize_custom();
                    }
                },
            });

            var ids = "";
            for (var i = 0; i < alt_ranks.length; i++) {
                ids += (ids == "" ? "" : ",") + alt_ranks[i]["id"];
                //ids.push(alt_ranks[i]["id"]);
            }
            if ((theForm.altRanks)) theForm.altRanks.value = ids;
            if (theForm.<% =isChanged.ClientID %>) theForm.<% =isChanged.ClientID %>.value = 1;
            if (rank_width<0) rank_width = $("#divAltsRank").width();

            resize_custom = function (force, w, h) {
                if (rank_width>0) {
                    var gw = $("#divAltsRank").width();
                    var gw_ = gw;
                    $("#divAltsRank").hide();
                    var w = ($("#divAltsRank").parent().width() - 24);
                    $("#divAltsRank").show();
                    if (gw > w) {
                        gw = w;
                    } else {
                        if (gw < rank_width) gw = (w < rank_width ? w : rank_width);
                    }
                    if (Math.abs(gw_-gw)>8) $("#divAltsRank").dxDataGrid("option", "width", gw);
                }

            }
        }
    }
<% End If %>

<% =JS_CollapsedContent() %>;
    var step_next = -1;
    
    function SetNextStep(step)
    {
      step_next = step;
    }

    function NeedCheckStep() 
    {
        <% If isReadOnly OrElse (App.ActiveProject.PipeParameters.DisableWarningsOnStepChange AndAlso GetAction(CurStep) IsNot Nothing AndAlso GetAction(CurStep).ActionType<>ActionType.atSpyronSurvey) Then %>return false;<% Else %>
        <% If Not IsReadOnly AndAlso GetAction(CurStep) IsNot Nothing AndAlso GetAction(CurStep).ActionType = ActionType.atSpyronSurvey Then%>if ((step_next!=-1) && (step_next<=<% =CurStep %>)) { showLoadingPanel(); return false; }<% End If%>
        return true;<% End if %>
    }

    function DoLogoutEval()
    {
        if ((window.parent))
        {
            if (logout_before != "") eval(logout_before);
            var result = DevExpress.ui.dialog.confirm(logout_confirmation, resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    window.parent.Logout();
                }
            });
            return false;
        }
        document.location.href="<% =JS_SafeString(PageURL(_PGID_LOGOUT)) %>";
    }

    function ShowIdleHelp()
    {
        var c = theForm.<% = isChanged.ClientID %>;
        if ((c) && (c.value<='0'))
        {
            // don't show per EF request
            //DevExpress.ui.dialog.alert("<% = JS_SafeString(ResString("msgShowHelp")) %>", "<% =JS_SafeString(ResString("titleInformation")) %>");
            document.cookie="<% =_COOKIE_NOVICE_IDLEHELP %>=1;path=/;expires=Thu, 31-Dec-2037 23:59:59 GMT";
        }    
    }

    function ShowIdleQuickHelp()
    {
        show_qh = true;
        if (ask_step_shown==false)
        {
            showQuickHelp("<% =JS_SafeString(QuickHelpParams) %>", false, <% =Bool2JS(CanUserEditActiveProject) %>, onLoadQH);
        }
    }

    function editQuickHelp(cmd) {
        OpenRichEditor("?type=11&" + cmd);
        /*<% =PopupRichEditor(reObjectType.QuickHelp, QuickHelpParams)%>*/
    }

    var steps_list = [];

    function loadStepsList(idx) {
        $.ajax({
            type: "GET",
            //            data: "ajax=yes&" + params,
            dataType: "text",
            async: true,
            cache: false,
            url: "<% = PageURL(CurrentPageID, _PARAM_ACTION + "=stepslist") %>&from=" + load_steps + "&rnd=" + Math.round(10000000*Math.random()),
            success: function(data) { parseStepsList(data); },
            timeout: _ajax_timeout
        });
    }

    function S(id) {
        SetNextStep(id);
        theForm.jump.value=id;
        __doPostBack("<% =btnJump.UniqueID %>","");
        return false;
    }

    function addStepsList() {
        var div = $get("<% =divStepsList.ClientID %>");
        if ((div)) {
            var s = "";
            var styles = ['button_evaluated','button_evaluate_undefined','button_no_evaluate','button_evaluate_undefined']; // 0 = evaluated, 1 = undefined, 2: non-evaluation, 3: disabled
            for (var i=0; i<steps_list.length; i++) {
                var d = steps_list[i];
                 s += "<input type=button class='button_step " + styles[d[1]] + (d[0]==<% =CurStep %> ? " button_active_list" : "") + "'" + (d[1]==3 ? " disabled=disabled' style='font-style: italic;'" : " onclick='S(" + d[0] + ");'") + " id='divStep" + d[0] + "' value='" + d[2] + "' title='" + d[3] + "'><br>";
            }
            steps_list = null;
            div.innerHTML = s;
            setTimeout('var b = $get("divStep<% =CurStep-(StepsCount\2)-1 %>"); if ((b)) b.scrollIntoView(true);', 30);
        }
    }

    function parseStepsList(data) {
        var div = $get("<% =divStepsList.ClientID %>");
        if ((div) && data!="") {
            if (data.substr(-2)!="]]" && data.substr(-1)=="]") data+="]"; 
            var lst = null;
            try { lst = eval(data); }
            catch(e) { }
            if ((lst) && lst.length>0) {
                steps_list = steps_list.concat(lst);
                div.innerHTML = "<p align=center class='text small gray' style='margin-top:13.5em'><img src='" + img_path + "devex_loading.gif' width=16 height=16 border=0><br><br>Loading: " + Math.round(100*steps_list.length/<% = App.ActiveProject.Pipe.Count %>) + "%</p>";
                load_steps += lst.length;
                if (load_steps>0 && load_steps<<% = App.ActiveProject.Pipe.Count %>) {
                    setTimeout('loadStepsList();', 10);
                } else {
                    load_steps = -1;
                    addStepsList();
                }
            }
        }
    }

    function onShowStepsList(sender, args)
    {
        if (load_steps>=0) {
            loadStepsList();
        } else {
            var d = $get("divStep<% =CurStep-(StepsCount\2)-1 %>");
            if ((d)) d.scrollIntoView(true);
        }
//        if ((sender) && (sender.get_text()=="")) args.set_cancel(true);
    }

    var t_where_chk = false;
    function onShowWhereAmI(sender, args)
    {
        <% If IsRiskWithControls AndAlso divTreeImpact.Visible Then%>if (!t_where_chk) {
            var lh = $("#divLikelihood").css({"padding-left":"0","padding-top":"0","padding-right":"0","padding-bottom":"0","margin-left":"0","margin-top":"0","margin-right":"0","margin-bottom":"0"}).height();
            var ih = $("#divImpact").css({"padding-left":"0","padding-top":"0","padding-right":"0","padding-bottom":"0","margin-left":"0","margin-top":"0","margin-right":"0","margin-bottom":"0"}).height();
            var lw = $("#divLikelihood").width();
            var iw = $("#divImpact").width();
            if (lh>0 && ih>0) {
                var mh = lh;
                if (ih>mh) mh = ih;
                var mw = lw;
                if (iw>mw) mw = iw;
                $("#WhereAmITabs").height(Math.round(mh+40)).width(Math.round(mw+4));
                t_where_chk = true;
                sender.hide();
                sender.show();
            }
        }
        <% End If%>
    }

    function onShowEvaluationPipeHelp(showResourceCenter)
    {
        var sl_help = "";
        if (showResourceCenter) {
            var sl_help = "ResourceCenter.html";
            var is_risk = <% =CStr(IIf(App.isRiskEnabled, "1", "0")) %>;
            if (is_risk == "1") sl_help = "ResourceCenter_Riskion.html";
        } else {
            sl_help = '<% =JS_SafeString(GetEvaluationHelpPageName(10, isLikelihood)) %>'; // 10 = LocalResults;
        }
        CreatePopup('<% =ApplicationURL(false, False) + _URL_ROOT %>' + sl_help, 'SLHelp', 'menubar=no,maximize=yes,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=800,height=600');
        return false;
    }

    function ChangeCommentIcon(img, is_over)
    {
        if ((img))
        {
            if (is_over)
            {
                img.src = (img.src == img_comment.src ? img_comment_ : img_comment_e_).src;
            }
            else
            {
                img.src = (img.src == img_comment_.src ? img_comment : img_comment_e).src;
            }
        }
    }
     

    <% if Not isReadOnly Then %>
            logout_before = function(callback_func) {
                alert_on_exit = 0; 
                logout_confirmation = (((theForm.<% =isChanged.ClientID %>)) && (theForm.<% =isChanged.ClientID %>.value>0) ? '<% =JS_SafeString(ResString("msgExitUnsavedEvaluation")) %>' : logout_confirmation);
                if (typeof callback_func == "function") callback_func();
            }
    <% End If %>

    var cur_step_btn = "<% = CurrentStepBtnID %>";
    if (cur_step_btn != "") {
        var btn_c = eval("theForm." + cur_step_btn);
        if (!(btn_c)) cur_step_btn = "";
    }
    <% If btnNext.Visible %>if (cur_step_btn == "") {
        var n = "<% =btnNext.ClientID %>";
        var btn_n = eval("theForm." + n);
        if ((btn_n) && !(btn_n.disabled)) cur_step_btn = n;
    }<% End if%>

    var alert_on_exit = 1;
    var show_wait = 1;  // D2309

    function CheckFrame()
    {
        var f = window.frames;
    }

    var btnnext_flashed = false;
    function FlashNextButton()
    {
        <%  If Not isEvaluation(GetAction(CurStep)) Then %>return false;<% End If %>
        if (btnnext_flashed) return false;
        var del = 300;
        var b = $("#<% =btnNext.ClientID %>");
        var im = $("#btnClickHere");
//        if (im) im.fadeIn(del*12);
        if ((b)) {
            for (var i=0; i<3; i++) {
                if ((im)) im.fadeTo(del*3, 1).fadeTo(del*3, 0.1);
                b.animate({borderColor: "#ff3333"}, del).animate({color: "#cc0000"}, del);
                b.animate({borderColor: "#c0c0c0"}, del).animate({color: "#000000"}, del);
            }
            b.animate({ color: "#f37900" }, del * 2);
            if ((im)) im.fadeTo(del, 0);
        }
//        if (im) im.fadeOut(del*3);
        btnnext_flashed = true;
        setTimeout(function () { btnnext_flashed = false; }, 2000);
    }

    function UpdateSLProject()
    {
        if ((window.opener) && (typeof window.opener.onUpdateSLProjectWithDelay)!="undefined") return window.opener.onUpdateSLProjectWithDelay();
        if ((window.parent) && (typeof window.parent.onUpdateSLProjectWithDelay)!="undefined") return window.parent.onUpdateSLProjectWithDelay();
        return false;
    }

    function StartEvalImpact()
    {
        if ((window.opener) && (typeof window.opener.EvalImpact)!="undefined") return window.opener.EvalImpact();
        if ((window.parent) && (typeof window.parent.EvalImpact)!="undefined") return window.parent.EvalImpact();
        return false;
    }

    function StartEvalLikelihood()
    {
        if ((window.opener) && (typeof window.opener.EvalLikelihood)!="undefined") return window.opener.EvalLikelihood();
        if ((window.parent) && (typeof window.parent.EvalLikelihood)!="undefined") return window.parent.EvalLikelihood();
        return false;
    }

    function RiskResults()
    {
        if ((window.opener) && (typeof window.opener.onOpenPage)!="undefined") return window.opener.onOpenPage(70016, "");
        if ((window.parent) && (typeof window.parent.onOpenPage)!="undefined") return window.parent.onOpenPage(70016, "");
        return false;
    }

    function InitUSD() {
        var c = showCost(usd_cost);
        if (c=="") c = "&mdash;";
        $("#divUSD").html(c);
        var f = theForm.cbUSD;
        if ((f)) {
            f.disabled = (usd_cost<=0);
            if ((usd_show) && (f.disabled)) {
                f.checked = false;
                usd_show = false;
            }
        }
    }

    function editUSD() {
        var conf_options = {
            "title": "<% =JS_SafeString(ResString("titleEditUSD")) %>",
            "messageHtml": "<p align=center><% = JS_SafeString(ResString("lblUSDValue")) %>:&nbsp;</span><input id='tbCostOfGoal' type='text' style='text-align:right; width:7em;' value='" + (usd_cost > -1073741824 ? usd_cost : "") + "'></p>", 
            "buttons": [{
                text: "<% =JS_SafeString(ResString("btnSave")) %>",
                onClick: function () {
                    var val = $("#tbCostOfGoal").val(); 
                    if (val=="") { 
                        setUSD(<%= UNDEFINED_INTEGER_VALUE %>); 
                    } else if (validFloat(val)) { 
                        setUSD(str2double(val));
                    }
                }
            }, { 
                text: "<% =JS_SafeString(ResString("btnReset")) %>",
                onClick: function () {
                    setUSD(0);
                    return false;
                }
            }, { 
                text: "<% =JS_SafeString(ResString("btnCancel")) %>",
                onClick: function () {
                }            
            }]
        };
        var a = DevExpress.ui.dialog.custom(conf_options);
        a.show().done(function(dialogResult) {
            if (typeof hook_keypress!="undefined") hook_keypress = true;
        });
        if (typeof hook_keypress!="undefined") hook_keypress = false;
        setTimeout(function () {
            $("#tbCostOfGoal").focus().select();
        }, 550);
    }

    function setUSD(val) {
        usd_cost = val * 1;
        theForm.totalUSD.value = usd_cost; 
        var c = showCost(usd_cost);
        if (c=="") c = "&mdash;";
        $("#divUSD").html(c);
        var f = theForm.cbUSD;
        if ((f)) f.disabled = (usd_cost<=0);
        theForm.<% =isChanged.ClientID %>.value = 1;
        <% =USD_onClientClick %>;
    }

    function onSwitchPMMode(mode) {
        theForm.action += (theForm.action.indexOf("?")>0 ? "&" : "?") + "view_mode=" + (mode ? 1 : 0);
        if (cur_step_btn=="") {
            __doPostBack('', '');
        }
        else {
            SetNextStep(<% =CurStep %>);
            eval("theForm." + cur_step_btn + ".click();");
        }
<%--        SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 1);
        var url = document.location.href;
        url += (url.indexOf("?")>0 ? "&" : "?") + "view_mode=" + (mode ? 1 : 0);
        document.location.href = url;
        SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 1);--%>
    }

//    var "undefined"_checked = false;
//    function confirmMissingJudgments() {
//        if ((theForm.<% =isChanged.ClientID %>.value>'0') && !undefined_checked && (typeof (HasUndefined) == "function") && HasUndefined()) {
//            var t = $find("<% =RadToolTipStepsList.ClientID %>");
//            if ((t)) t.hide();
//            dxDialog("<% =JS_SafeString(ResString("msgMissingJudgmentMultiEval")) %>", "__doPostBack('" + _postback_eventTarget + "', '" + _postback_eventArgument + "');", "undefined_checked=false;", "<% = JS_SafeString(ResString("titleConfirmation")) %>", jDialog_def_width, jDialog_def_height, "<% = JS_SafeString(ResString("btnYes")) %>", "<% = JS_SafeString(ResString("btnNo")) %>");
//            "undefined"_checked = true;
//            return false;
//        }
//        return true;
//    }

//    var __oldDoPostBack = null;
//    var _postback_eventTarget = null;
//    var _postback_eventArgument = null;
//    var _postback_custom_code = "";

//    function HookSubmit()
//    {
//        __oldDoPostBack = __doPostBack;
//        __doPostBack = evalDoPostBack;
//    }

//    function evalDoPostBack(eventTarget, eventArgument) {
//        _postback_eventTarget = eventTarget;
//        _postback_eventArgument = eventArgument;
//        if ((_postback_custom_code) && (_postback_custom_code!="")) if (!eval(_postback_custom_code)) return false;
//        if ((__oldDoPostBack)) {
//            return __oldDoPostBack (eventTarget, eventArgument);
//        } else {
//            if (!theForm.onsubmit || (theForm.onsubmit() != false)) {
//                theForm.__EVENTTARGET.value = eventTarget;
//                theForm.__EVENTARGUMENT.value = eventArgument;
//                theForm.submit();
//            }
//        }
//    }

    function infoPageResize() {
        var h_old = $('#divInfopage').height();
        $('#divInfopage').height(100);
        var h = $("#<% =tdContent.ClientID %>").height();
        if (h>100) {
            h-=24;
            var t = $('#divInfopage table:first');
            if ((t) && (t.length)) t.css("margin", "0 auto");
        } else {
            h = h_old;
        }
        $('#divInfopage').height(h);
    }

    function _resizeEval() {
<%--        var pnl = $("#<% =pnlEvaluate.ClientID %>");
        //var tbl = $("#tblEvalFooter");
        if ((pnl) && (pnl.length)) {
            var p = pnl.parent();
            if ((p) && (p.length)) {
                pnl.width(200).height(100);
                var w = p.width() - 8;
                if (w>100) pnl.width(w);
                var h = p.height() - 16;
                if (h>100) pnl.height(h);
            }
        }--%>
    }

    Telerik.Web.UI.RadTreeView.prototype.saveClientState = function () {
        return "{\"expandedNodes\":" + this._expandedNodesJson +
        ",\"collapsedNodes\":" + this._collapsedNodesJson +
        ",\"logEntries\":" + this._logEntriesJson +
        ",\"selectedNodes\":" + this._selectedNodesJson +
        ",\"checkedNodes\":" + this._checkedNodesJson +
        ",\"scrollPosition\":" + Math.round(this._scrollPosition) + "}";
    }

    shortcuts = shortcuts.concat([{"code":"Ctrl+&larr;", "title": "<% =JS_SafeString(ResString("lblShortcutCtrlLeft")) %>"}, {"code":"Ctrl+&rarr;", "title": "<% =JS_SafeString(ResString("lblShortcutCtrlRight")) %>"},<% If CanEditActiveProject Then %>{"code":"Ctrl+#", "title": "<% =JS_SafeString(ResString("lblJumpEvaluationToStep")) %>"},<% End if %><% If isMultiRatingsOrDirect %>{"code":"Ctrl+&uarr;", "title": "<% =JS_SafeString(ResString("lblShortcutCtrlUp")) %>"}, {"code":"Ctrl+&darr;", "title": "<% =JS_SafeString(ResString("lblShortcutCtrlDown")) %>"}, <% end if %>{"code":"-", "title": "-"}]);
    keyup_custom = Hotkeys;
    window.onbeforeunload = function () {return OnExitPage(false) };
    window.onload = function() { if(typeof gridDblClickHandler!= 'undefined') gridDblClickHandler(); };
    
    <%If IsIntensities Then%> window.focus(); <%End If%>

    $(window).bind("resize", function () {
        setTimeout(_resizeEval, 500);
    });

    setTimeout("CheckFrame();", 3000);
    $(document).ready(function () {
        <% If Not isSLTheme() Then %>if (typeof toggleSideBarMenu == "function") toggleSideBarMenu(true, 0, false);<% End if %>
        $("textarea").each(function () { $(this).val($(this).val().trim()); });
        <% If (HelpID <> ecEvaluationStepType.Other) Then %>setTimeout( function () {
            help_uri = 'anytime-<% =JS_SafeString(HelpID.ToString.ToLower) %>';
            if (typeof updateHelpPage == 'function') updateHelpPage(help_uri); 
        }, 500); <% End if %>
        <% if IsReadOnly Then %>setTimeout(function () {
            DevExpress.ui.notify("<% = JS_SafeString(ResString("lblReadOnlyMode")) %>", "warning");
        }, 1500);<% End if %>
        <% If tdTask.Visible AndAlso lblTask.Text <> "" Then %>initT2S();<% End if %>
        _resizeEval();
    });
    
// -->
</script>
 <telerik:RadToolTip ID="RadToolTipWhereAmI" runat="server" AccessKey="?" OffsetX="-10" OffsetY="-5" HideEvent="ManualClose" Position="TopRight" RelativeTo="Element" ShowEvent="onClick" TargetControlID="imgWhereAmI" OnClientShow="onShowWhereAmI">
    <div id="WhereAmITabs">
    <% If IsRiskWithControls AndAlso divTreeImpact.Visible Then%><ul>
        <li><a href="#divLikelihood"><% = ResString("lblLikelihood") %></a></li>
        <li><a href="#divImpact"><% = ResString("lblImpact") %></a></li>
    </ul><% End if %>
    <div id="divLikelihood"><div style="margin:1ex; padding:1ex;" runat="server" id="divTree"><telerik:RadTreeView runat="server" ID="RadTreeViewHierarchy" AllowNodeEditing="false" OnClientNodeClicking="ClientNodeClicking" OnNodeClick="RadTreeViewHierarchy_NodeClick"/></div><div style="margin:1ex; padding:1ex; border-top:1px solid #e0e0e0; background:#fafafa;" runat="server" id="divNoSources" visible="false"><telerik:RadTreeView runat="server" ID="RadTreeViewNoSources" AllowNodeEditing="false" OnClientNodeClicking="ClientNodeClicking" OnNodeClick="RadTreeViewHierarchy_NodeClick"/></div></div>
    <% If IsRiskWithControls AndAlso divTreeImpact.Visible Then%><div id="divImpact"><div style="margin:1ex; padding:1ex;" runat="server" id="divTreeImpact" visible="false"><telerik:RadTreeView runat="server" ID="RadTreeViewImpact" AllowNodeEditing="false" OnClientNodeClicking="ClientNodeClicking" OnNodeClick="RadTreeViewHierarchy_NodeClick"/></div></div><% End If%>
    </div>
    <img src="<% =BlankImage %>" width="200" height="1" title="" alt=""/>
 </telerik:RadToolTip>
 <telerik:RadToolTip ID="RadToolTipStepsList" runat="server" AccessKey="L" OffsetX="-10" OffsetY="-5" HideEvent="ManualClose" Position="TopCenter" RelativeTo="Element" ShowEvent="onClick" AutoCloseDelay="5000" TargetControlID="imgStepsList" OnClientShow="onShowStepsList">
    <img src="<% =BlankImage %>" width="200" height="1" title="" alt="" style='margin-bottom:6px'/><div style="margin:4px; padding:2px;" runat="server" id="divStepsList"><asp:Repeater ID="RepStepsList" runat="server">
    <ItemTemplate><div style='margin:2px' id="divStep<%#DataBinder.Eval(Container, "ItemIndex")%>"><asp:Button ID="btnStep" runat="server" Text="#" CssClass="button button_small" CommandName="step" UseSubmitBehavior="false" OnClick="btnStep_Click"/></div></ItemTemplate>
    </asp:Repeater></div>
 </telerik:RadToolTip>
 </asp:Panel>
<asp:Label ID="lblMessage" runat="server" Visible="false" />

<telerik:radtooltip runat="server" id="ttJump" HideEvent="FromCode" Skin="WebBlue" RelativeTo="Element" Position="TopCenter" OffsetX="0" OffsetY="-6"><div style='padding:10px; white-space:nowrap'>
<% =ResString("lblJumpEvaluationToStep") %> <input type='text' class='input' name='jump' value='' style='width:50px' />&nbsp;<asp:Button runat="server" ID="btnJump" Text="OK" OnClientClick="if (!DoJump()) return false;" CommandName="step" CssClass='button btn-small' Width="56" /><input type='button' name='btnJumpCancel' value='<% =ResString("btnEvaluationJumpCancel") %>' onclick='HideJumpTooltip();' class='button btn-small' style="width:56px" />
</div></telerik:radtooltip>
<telerik:radtooltip runat="server" id="RadtooltipNavHelp" HideEvent="ManualClose" SkinID="tooltipInfo" RelativeTo="Element" Position="TopCenter" OffsetX="0" OffsetY="-6" TargetControlID="imgNavHelp"  IsClientID="false" ShowEvent="OnClick"><div style="padding:10px;"><%= ResString("hintEvalNavigation")%></div></telerik:radtooltip>
<%--<telerik:radtooltip runat="server" id="RadtooltipHotkeys" HideEvent="ManualClose" SkinID="tooltipInfo" RelativeTo="Element" Position="BottomRight" OffsetX="-10" OffsetY="-3" TargetControlID="imgHotkeys" IsClientID="true" ShowEvent="OnClick"><div style="padding:0px 1em 1ex 1em;"><%= HotKeysHint() %></div></telerik:radtooltip>--%>
</asp:Content>