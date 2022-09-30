<%@ Page Language="VB" Inherits="PipeParamsPage" Codebehind="PipeParams.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">
    
    var evalWhatVals = <%= GetEvalWhatVals() %>;
    var evalHowVals = <%= GetEvalHowVals() %>;
    var dispWhatVals = <%= GetDispWhatVals() %>;

    var prj_list = null;
    var risk_prefix = "<% =JS_SafeString(clsProjectParametersWithDefaults.OPT_PASSCODE_RISK_CONTROLS_PIPE_PREFIX) %>";
    var val_old = "";
 
    var is_risk = <% =Bool2JS(App.isRiskEnabled) %>;
    var prj_id = <% =App.ProjectID %>;
    var hid = <% =iif(App.ActiveProject.isImpact, 1,  0) %>;

    var h_list = ["<% = JS_SafeString(ResString_("lblLikelihood").SubString(0,1)) %>", "<% = JS_SafeString(ResString_("lblImpact").SubString(0,1)) %>", "<% = JS_SafeString(ResString_("lblControls").SubString(0,1)) %>"];

    var rich_url_prefix = "?type=<%=CInt(reObjectType.PipeMessage)%>&field=comment&<% =_PARAM_ID %>=";

    function RO() {
        DevExpress.ui.notify("<% =JS_SafeString(ResString("lblProjectReadOnly")) %>", 'warning', 3000);
    }
  
    function sendCommand(cmd, extra_cmd) {
        <% If isReadOnly() Then %> RO(); return false;<% End If %>
        showLoadingPanel();
        _ajax_ok_code = SyncReceived;
        if (typeof extra_cmd!="undefined" && extra_cmd!="") _ajax_ok_code += extra_cmd;
        _ajaxSend(cmd);
    }

    function SyncReceived(data) {
        hideLoadingPanel();

        //A1315 ===
        var received_data = eval(data);
        if ((received_data) && (received_data.length > 1)) {
            if (received_data[0] == "AlternativesEvalMode") {
                // show a warning message if any
                if (received_data[2] != "") {
                    dxDialog(received_data[2], ";", "undefined", "<%=ResString("msgWarning")%>");
                }
                // set the EvalAlts radio checked if server returned other value
                $("input[name=EvalAlts][value=" + received_data[1] + "]").prop("checked", true);
            }
            if (received_data[0] == "UseCaching") {
                //var c = $("#cbUseCaching").is(':checked');
                var c = received_data[1]*1;
                if ((window.opener) && (typeof window.opener.onUpdatePrtyCaching)!="undefined") return window.opener.onUpdatePrtyCaching(c);
                if ((window.parent) && (typeof window.parent.onUpdatePrtyCaching)!="undefined") return window.parent.onUpdatePrtyCaching(c);
            }
            if (received_data[0] == "copy_settings") {
                if ((received_data[1])) {
                    DevExpress.ui.notify('All settings has been copied.', 'success');
                } else {
                    DevExpress.ui.notify('Unable to copy settings', 'error');
                }
            }
        } // A1315 ==
    }

    function showAdditionalObjOptions(isShow) {
        if (isShow) {
            $(".opt_obj_span").show();
            $(".opt_td_objs1").addClass("opt_td").addClass("opt_td_objs");
        } else {
            $(".opt_obj_span").hide();
            $(".opt_td_objs1").removeClass("opt_td").removeClass("opt_td_objs");
        };
    };

    function showAdditionalAltOptions(isShow) {
        if (isShow) {
            $(".opt_alt_span").show();
            $(".opt_td_alts1").addClass("opt_td").addClass("opt_td_alts");
        } else {
            $(".opt_alt_span").hide();
            $(".opt_td_alts1").removeClass("opt_td").removeClass("opt_td_alts");
        };
    };

    function onRichEditorRefresh(empty, infodoc, callback_param)
    {
        window.focus();
    };

    function onOpenSurvey(is_welcome, has_survey) {
        var url =  urlWithParams("temptheme=sl&close=0", (is_welcome ? "<% =PageURL(_PGID_SURVEY_EDIT_PRE) %>" : "<% =PageURL(_PGID_SURVEY_EDIT_POST) %>"));
        //var editor = "<table width=100% height=100% border='0' cellspacing=0 cellpadding=0 class='whole'><tr style='height:99%'>come) ? <% =IIf(App.ActiveProject.isImpact, "3 : 4", "1 : 2") %>);
        var editor = "<table width=100% height=100% border='0' cellspacing=0 cellpadding=0 class='whole'><tr style='height:99%'><td class='text'><iframe frameborder='0' src='' class='whole' id='frmSurvey' name='frmSurvey' style='width:100%; height:99%'></iframe></td></tr>";
        editor += "<tr><td style='padding-top:1ex;' class='text' align=center><input type='button' class='button' value='<% =JS_SafeString(ResString("btnClose")) %>' onclick='$(\"#divEditSpyron\").dialog(\"close\"); return false;'></td></tr></table>";
        $("body").css("overflow", "hidden");
        //$("#divPipeParams").height(300);
        var w = $("#divPipeGlobal").width()-32;
        var h = $("body").height()-32;
        //onResize();
        $("#divEditSpyron").html(editor).dialog({
            modal: true,
            autoOpen: true,
//            dialogClass: "no-close",
            closeOnEscape: false,
            bgiframe: true,
            zIndex: 255,
            title: "Edit " + ((is_welcome) ? "Welcome" : "Thank You") +" Survey",
            width: w,
            height: h,
            open: function () { jDialog_opened = true; },
            beforeClose: function( event, ui ) { if (!canCloseSurvey()) return false; },
            close: function () { jDialog_opened = false; $("body").css("overflow", "auto"); onResize(); },
//            resize: function( event, ui ) { $("#divModels").height(30); $("#divModels").height(Math.round(ui.size.height-176)); },
            position: { my: "center", at: "center", of: $("body"), within: $("body") }
        });
        var n = ((is_welcome) ? "Welcome" : "ThankYou");
        $("#btnEdit" + n + "Survey").prop("value", "<% = JS_SafeString(ResString("btnEditH"))%>");
        $("#btn" + n + "SurveyDownload").prop("disabled", "");
        if (!has_survey) {
            $("#cbShow" + n + "Survey").prop("checked", true).trigger("change");
        }
        initFrameLoader(document.getElementById("frmSurvey"));
        frmSurvey.location.replace(url);   // for avoid to add to browser history used replace() instead of simple assign to .src;
    }

    function canCloseSurvey() {
        var frm = null;
        for (var i=0; i<window.frames.length; i++)
        {
            try {
                var fr = window.frames[i];
                if (typeof(fr) != "undefined" && fr.name == "frmSurvey") {
                    frm = fr;
                    break;
                }
            }
            catch (e) {
            }
        }
        if ((frm) && typeof(frm) !=undefined && typeof(frm.CheckExitEvent)=="function") return (frm.CheckExitEvent());
        return true;
    }

    function UploadSurvey(is_welcome) {
        var w = CreatePopup("<% =PageURL(_PGID_SURVEY_UPLOAD, "?prjid=" + App.ProjectID.ToString + "&type="" + (is_welcome ? " + If(App.ActiveProject.isImpact, "3 : 4", "1 : 2") + ") + ""&temptheme=sl") %>", "UploadSurvey", "menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=600,height=180", true);
        if ((w)) w.focus();
    }

    function onUploadSurvey(id) {
        var n = ((id=="2" || id=="4") ? "ThankYou":  "Welcome");
        $("#cbShow" + n + "Survey").prop("checked", true).trigger("change");
        setTimeout('showLoadingPanel(); document.location.reload();', 1500);
    }

    function onDownloadSurvey(is_welcome) {
        document.location.href = "<% =PageURL(_PGID_SURVEY_DOWNLOAD, "?action=download&prjid=" + App.ProjectID.ToString + "&st="" + (is_welcome ? " + If(App.ActiveProject.isImpact, "3 : 4", "1 : 2") + ")") %>;
    }

    function onResize() {
        //$("#divPipeParams").height(100).width(300).height($("#divPipeGlobal").height()-34).width($("#divPipeGlobal").width()-30);
        if (jDialog_opened && $("#divEditSpyron").html()!="") {
            //$("#divEditSpyron").dialog("option", "width", $("#divPipeGlobal").width()-70).dialog("option", "height", $("#divPipeGlobal").height()-50);
        }
    }

    function checkNum(val, id) {
        if (trim(val)=="") return true;
        if (validInteger(val) && (val*1==val)) return true;
        dxDialog("<% =JS_SafeString(ResString_("errWrongNumber")) %>", (typeof id!="undefined" && id!="" ? 'setTimeout(\'$("#' + id + '").focus();\', 100);' : ";"), "undefined", "<%=ResString("lblError") %>");
        return false;
    }

    function checkStr(val, id) {
        if (trim(val)!="") return true;
        dxDialog("<% =JS_SafeString(ResString("errEmptyString"))%>", (typeof id!="undefined" && id!="" ? 'setTimeout(\'$("#' + id + '").focus();\', 100);' : ";"), "undefined", "<%=ResString("lblError") %>");
        return false;
    }

    function checkInput(id, val) {
        $inp = $("#"+id);
        if ($inp.is(":focus") && $inp.val()==val) {
            $inp.blur().focus();
        }
    }

    function initCheckBox(id, val, param_name) {
        return $("#" + id).prop("checked", val).change(function () { <% If isReadOnly() Then %> RO(); return false;<% End If %> var isChecked = $(this).is(':checked'); sendCommand("action=" + param_name + "&checked=" + isChecked); });
    }

    function initCombobox(id, val, param_name) {
        return $("#" + id).val(val).change(function () { <% If isReadOnly() Then %> RO(); return false;<% End If %> sendCommand("action=" + param_name + "&val=" + this.value); });
    }

    function initRadioGroup(name, val, param_name) {
        var input = $('input:radio[name=' + name + ']');
        input.filter('[value=' + val + ']').prop('checked', true);
        input.change(function () { <% If isReadOnly() Then %> RO(); return false;<% End If %> sendCommand("action=" + param_name + "&val=" + $(this).val()); });
        return input;
    }

    function initInput(id, val, param_name, check_num, check_empty_str) {
        input = $("#" + id).val(val);
        input.on('focus', function () { val_old = $(this).val(); });
        input.keypress(function(e) {
            if (e.keyCode == 13) $(this).blur();
            if (e.keyCode == 27) $(this).val(val_old).blur();
        });
        input.on('keyup', function(e) {
            if ($(this).val()!="") setTimeout('checkInput("' + this.id + '", "' + replaceString("'", "\'", $(this).val()) + '");', 2500);
        });
        input.on('blur', function () {
            <% If isReadOnly() Then %> RO(); return false;<% End If %>
            var rVal = $(this).val();
            if (rVal!=val_old) {
                if ((check_num && !checkNum(rVal, this.id)) || (check_empty_str && !checkStr(rVal, this.id))) { /*if (rVal!="" && val_old!="") $(this).val(val_old);*/ $(this).focus(); return false; }
                sendCommand("action=" + param_name + "&val=" +  encodeURIComponent(rVal));
            }
        });
        return input;
    }

    // D4162 ===
    function checkAtFinishOptions() {
        $('#tbRedirectURL').prop('disabled', !($("#rbNavRedirect").is(':checked')));
        var is_next = ($("#rbNavOpenPrj").is(':checked'));
        var is_join = (is_risk ? ($("#rbNavJoin").is(':checked')) : false);
        $('#cbNavLogOff').prop('disabled', is_next || is_join);
        $("#rbNavOpenPrj").prop('disabled', ($('#cbNavLogOff').is(':checked')));
        $('#tbNextModelName').prop('disabled', !is_next);
        <% If Not isReadOnly() Then %>$("#btnSelProject").prop('disabled', !is_next);<% End If %>
    }

    function updateNextProjectName() {
        var p = replaceString(risk_prefix.toLowerCase(), "", evalHowVals[11].toLowerCase());
        var n = "";
        var h = 0;
        prj_idx = -1;
        if (prj_list != null) {
            for (var i = 0; i < prj_list.length; i++) {
                if (p == prj_list[i][2].toLowerCase() || p == prj_list[i][3].toLowerCase()) {
                    n = prj_list[i][1];
                    if (p!="" && p == prj_list[i][3].toLowerCase()) h = 1;
                    if (p!="" && p != evalHowVals[11].toLowerCase()) h = 2;
                    prj_idx = i;
                    break;
                }
            }
        }
        if (n=="" && p!="" && prj_idx>0) n = "<% =JS_SafeString(ResString("msgNavNoProject")) %>";
        if (is_risk && p!="") {
            n = "[" + h_list[h] + "] " + n;
        }
        $("#tbNextModelName").val(n);
        if ((is_risk) && (theForm.optEvalPipe)) theForm.optEvalPipe[h].checked=true;
    }

    function getProjectsList() {
        showLoadingPanel();
        sendCommand("action=prj_list", "onGetProjectsList(data);");
        return false;
    }

    function onGetProjectsList(data) {
        hideLoadingPanel();
        if (data.length>3 && data[0]=="[") {
            prj_list = eval(data);
            //showProjects();
        }
        updateNextProjectName();
    }

    var prj_count = 0;
    var prj_idx = -1;
    function InitProjectsList(lst) {
        var t = $("#divModels");
        if ((t) && (lst)) {
            var prj = "<table width='100%' border=0 cellspacing=0 cellpadding=1>\n";
            prj_count = lst.length;
            for (var i=0; i<lst.length; i++) {
                //if (lst[i][0]<0 || lst[i][0]==evalHowVals[11]) prj_idx = i;
                var n = ShortString(lst[i][1], (is_risk ? 52 : 58), false);
                if (i==0 && lst[i][2]=="") n = "<i>" + n + "</i>";
                var e = "&nbsp;";
                var ei = "";
                if (lst[i][2]!="") {
                    e = (is_risk ? "<b>L</b>:&nbsp;" : "") + lst[i][2];
                    if ((is_risk)) {
                        ei = "&nbsp;";
                        if (lst[i][3]!="" && lst[i][2]!=lst[i][3]) ei = "<b>I</b>:&nbsp;" + lst[i][3];
                        ei = "<td class='text xsmall gray' align='left' id='prj_p2_" + i + "' title='" + replaceString("'", "&#39;", htmlEscape(lst[i][3])) + "'><nobr>" + ei + "</nobr></td>";
                    }
                }
                prj += "<tr valign='middle'"+(i&1 ? " style='background:#f0f0f0'" : "") + "id='prj" + i + "'><td class='text'><nobr><input type='radio' class='radio' id='prjName" + i + "' name='prjName' value='" + i + "'" + (i==prj_idx ? " checked" : "") +" onclick='SetNextProject(this.value);'><label for='prjName" + i + "'><span id='prj_title" + i + "' title='" + replaceString("'", "&#39;", htmlEscape(n)) + "'>" + n + "</span></label></nobr></td><td class='text xsmall gray' align='left' id='prj_p_" + i + "' title='" + replaceString("'", "&#39;", htmlEscape(lst[i][2])) + "'><nobr>" + e + "</nobr></td>" + ei + "</tr>\n";
            }
            prj += "</table><div id='divPrjMsg' class='h5 gray' style='text-align:center; padding-top:5em; display:none'></div>";
            t.html(prj);
        }
    }

    function showSearchText(n, txt) {
        var s = 0;
        var txt_len = txt.length;
        while (n.toLowerCase().indexOf(txt, s)>=0) {
            var idx = n.toLowerCase().indexOf(txt, s);
            var n_ = n.slice(0, idx) + "<span class='src'>" + n.slice(idx, idx+txt_len) + "</span>" + n.slice(idx+txt_len);
            n = n_;
            s = idx + txt_len + 25;
            //show = true;
        }
        return n;
    }

    function filterProjectsList() {
        var e = document.getElementById("inpSearch");
        if ((e)) {
            var txt = e.value.toLowerCase();
            var has_files = false;
            for (var i=0; i<prj_count; i++) {
                var p = document.getElementById("prj_title" + i);
                var pa_orig = document.getElementById("prj_p_" + i);
                var pa_i_orig = document.getElementById("prj_p2_" + i);
                if ((p) && (p.title) && (p.title!="") && (pa_orig)) {
                    var n = p.title;
                    var pa = pa_orig.title;
                    var pa_i = ((pa_i_orig) ? pa_i_orig.title : "");
                    var show = (txt=="");
                    if (!show) {
                        n = showSearchText(n, txt);
                        var pa = showSearchText(pa, txt);
                        var pa_i = showSearchText(pa_i, txt);
                        show = (n!=p.title || (pa!="" && (pa_orig) && pa_orig.title!=pa) || (pa_i!="" && (pa_i_orig) && pa_i_orig.title!=pa_i));
                    }
                    if (show) {
                        $("#prj" + i).show(); 
                        has_files= true; 
                    } else  { 
                        $("#prj" + i).hide(); 
                    }
                    if (show) {
                        n = "<nobr>" + n + "</nobr>";
                        pa = "<nobr>" + (!is_risk || pa=="" ? "" : "<b>L</b>:&nbsp;") + pa + "</nobr>";
                        pa_i = "<nobr>" + (pa_i == "" ? "" : "<b>I</b>:&nbsp;") + pa_i + "</nobr>";
                        if (p.innerHTML!=n) p.innerHTML = n;
                        if (pa_orig.innerHTML != pa) pa_orig.innerHTML = pa;
                        if ((pa_i_orig) && pa_i_orig.innerHTML!=pa_i) pa_i_orig.innerHTML = pa_i;
                    }
                }
            }
            if (has_files) {
                $("#divPrjMsg").hide();
            } else {
                $("#divPrjMsg").html("<div class='text'><% = JS_SafeString(ResString("errNoSearch"))%></div>").show();
            }
        }
        //if (prj_idx>=0) { setTimeout('scrollPrjList();', 25); }
    }

    function onSearchProject() {
        setTimeout("filterProjectsList();", 25);
    }

    function showProjects() {
        <% If isReadOnly() Then %> RO(); return false;<% End If %>
        showLoadingPanel();
        InitProjectsList(prj_list);
        dlg_models = $("#divModelsList").dialog({
            autoOpen: true,
            //              height: 500, //(lst.length>10 ? 400 : "auto"),
            minWidth: 530,
            maxWidth: 950,
            minHeight: 250,
            maxHeight: 500,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString_("lblDlgSelectPrj"))%>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                OK: {
                    id: "btnSelectPrj",
                    text: "<% = JS_SafeString(ResString("btnSelect")) %>",
                    disabled: <% =Bool2JS(isReadOnly) %>,
                    click: function() {
                        SaveNextProject();
                        $("#divModelsList").dialog("close");
                    }
                },
                Cancel: function() {
                    $("#divModelsList").dialog("close");
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
                if (prj_idx>=0) { setTimeout('scrollPrjList();', 25); }
                setTimeout('filterProjectsList(); SetNextProject(prj_idx); resizePrjList();', 10);
            },
            close: function() {
                $("body").css("overflow", "auto");
                //                $("#divModelsList").dialog("destroy");
            },
            resize: function( event, ui ) { resizePrjList(); }
        });
        hideLoadingPanel();
    }

    function resizePrjList() {
        var h = $("#divModelsList").height(); 
        if (h*1>100) { 
            $("#divModels").height(30); 
            $("#divModels").height(h-($("#divPipe").is(":visible") ? 90 : 54)); 
        } 
    }

    function scrollPrjList() {
        var p = $("#prj" + prj_idx).offset().top - $("#divModels").offset().top - 20; 
        if (p>0) $("#divModels").animate({ scrollTop: p }, 1000);
    }

    function SetNextProject(id) {
        prj_idx = id;
        if ((is_risk)) {
            $("#rbEvalL").prop("disabled", "");
            $("#rbEvalI").prop("disabled", "");
            if (id>0 && prj_list[id][0]==prj_id) {
                $("#rbEval" + (hid==0 ? "L" : "I")).prop("disabled", "disabled");
                if ($("#rbEval" + (hid==0 ? "L" : "I")).is(":checked")) $("#rbEval" + (hid==0 ? "I" : "L")).prop('checked', true);
            }
        }
        //$("#btnSelectPrj").button({disabled: (prj_id>0)});
        //updateNextProjectName();
    }

    function SaveNextProject() {
        if (prj_idx>=0) {
            var prj = prj_list[prj_idx];
            if (prj.length>3) {
                var h = $('input[name="optEvalPipe"]:checked').val();
                var p = prj[2];
                if (h==1 && prj[3]!="") p = prj[3];
                if (h==2 && p!="") p = risk_prefix + p;
                evalHowVals[11] = p;
                sendCommand("action=nextproject&val=" + p);
                updateNextProjectName();
            }
        }
    }
    // D4162 ==

    //function updateTemplate(name, value) {
    //    if (typeof _templates != "undefined") {
    //        var keys = Object.keys(_templates);
    //        for (var i = 0; i < keys.length; i++) {
    //            if (keys[i] == name) {
    //                _templates[name] = value;
    //                if (menu_last) initSideBarMenu(menu_last);
    //                break;
    //            }
    //        }
    //    }
    //}


    function initEvalWhat() {
        // _PGID_PROJECT_OPTION_EVALUATE 20013
        // === Evaluate What ===

        initCheckBox("cbEvalObjectives", evalWhatVals[0], "EvaluateObjectives").click(function () { showAdditionalObjOptions($(this).is(':checked'));} );
        showAdditionalObjOptions(evalWhatVals[0]);

        initCheckBox("cbEvalAlternatives", evalWhatVals[1], "EvaluateAlternatives").click(function () { showAdditionalAltOptions($(this).is(':checked'));} );
        showAdditionalAltOptions(evalWhatVals[1]);

        //initCheckBox("cbDefaultMeasurementTypeObjs", evalWhatVals[2], "DefaultNonCoveringObjectiveMeasurementType");
        initCombobox("cbDefaultMeasurementTypeAlts", evalWhatVals[3], "DefaultCoveringObjectiveMeasurementType");

        initRadioGroup("optEvalObjsOrder", evalWhatVals[4], "ObjectivesEvalDirection");

        <% If _OPT_SHOW_FEEDBACK_ON Then %>
        initCheckBox("cbFeedBack", evalWhatVals[5], "FeedbackOn");
        <% End If %>

        initRadioGroup("EvalWhatObjs", ((evalWhatVals[6]) ? 1 : 0), "ObjectivesPairwiseOneAtATime");
        initRadioGroup("EvalWhatAlts", ((evalWhatVals[7]) ? 1 : 0), "AlternativesPairwiseOneAtATime");
        initRadioGroup("EvalAlts", evalWhatVals[8], "AlternativesEvalMode");

        initRadioGroup("Diagonals", evalWhatVals[9], "EvaluateDiagonals");
        initRadioGroup("DiagonalsAlts", evalWhatVals[10], "EvaluateDiagonalsAlternatives");

        initCheckBox("cbEvalPWAllDiagonals", evalWhatVals[11], "ForceAllDiagonals").click(function() { $('#tbForceAllDiagonalsLimit').prop('disabled', !$(this).is(':checked')); } );
        initCheckBox("cbEvalPWAllDiagonalsAlts", evalWhatVals[12], "ForceAllDiagonalsForAlternatives").click(function() { $('#tbForceAllDiagonalsLimitAlts').prop('disabled', !$(this).is(':checked')); } );

        initInput("tbForceAllDiagonalsLimit", evalWhatVals[13], "ForceAllDiagonalsLimit", true, true).prop('disabled', !evalWhatVals[11]);
        initInput("tbForceAllDiagonalsLimitAlts", evalWhatVals[14], "ForceAllDiagonalsLimitForAlternatives", true, true).prop('disabled', !evalWhatVals[11]);

        initCheckBox("cbEvalPWForceGraphical", evalWhatVals[15], "ForceGraphical").prop('disabled', (evalWhatVals[19]==2));
        initCheckBox("cbEvalPWForceGraphicalAlts", evalWhatVals[16], "ForceGraphicalForAlternatives").prop('disabled', (evalWhatVals[20]==2));

        initRadioGroup("GPWMode", evalWhatVals[17], "GraphicalPWMode");
        initRadioGroup("GPWModeAlts", evalWhatVals[18], "GraphicalPWModeForAlternatives");

        initRadioGroup("PWType", (evalWhatVals[19]==1 ? 1 : 2), "PairwiseType").click(function () { $('#cbEvalPWForceGraphical').prop('disabled', ($(this).val() == 2)); } );
        initRadioGroup("PWTypeAlts", (evalWhatVals[20]==1 ? 1 : 2), "PairwiseTypeForAlternatives").click(function () { $('#cbEvalPWForceGraphicalAlts').prop('disabled', ($(this).val() == 2)); } );

        initRadioGroup("EvalOrder", evalWhatVals[21], "ModelEvalOrder");

        var objs_old = "";
        var alts_old = "";
        initInput("tbObjName", evalWhatVals[22], "NameObjectives", false, true).on("focus", function() {
            objs_old = this.value;
        }).on("blur", function() {
            if (objs_old != this.value) {
                $(".wording_obj").html(this.value);
                //updateTemplate("<% = JS_SafeString(_TEMPL_OBJECTIVES).Trim(CChar("%")).ToLower %>", this.value);
                //document.location.reload();
            }
        });
        initInput("tbAltName", evalWhatVals[23], "NameAlternatives", false, true).on("focus", function() {
            alts_old = this.value;
        }).on("blur", function() {
            if (alts_old != this.value) {
                $(".wording_alt").html(this.value);
                //updateTemplate("<% = JS_SafeString(_TEMPL_ALTERNATIVES).Trim(CChar("%")).ToLower %>", this.value);
                //document.location.reload();
            }
        });

        initInput("tbCustomWordingObjs", evalWhatVals[24], "JudgementPromt", false, true).hide();
        initInput("tbCustomWordingAlts", evalWhatVals[25], "JudgementAltsPromt", false, true).hide();

        if (is_risk) {
            initCheckBox("cbParseValueFromName", evalWhatVals[27], "EvalTryParseValuesFromNames");

            initCheckBox("cbShowEventID", evalWhatVals[28], "ShowEventID").click(function() { $('#cbIDColumnMode').prop('disabled', !$(this).is(':checked')); } );
            initCombobox("cbIDColumnMode", evalWhatVals[29], "IDColumnMode");
        }

        var exists = false;
        if (evalWhatVals[24] != "") {
            $('#cbWordingObjs option').each(function(){
                if (this.value == evalWhatVals[24]) {
                    exists = true;
                    return false;
                }
            });
        }
        if (!exists) $('#tbCustomWordingObjs').show();

        initCombobox("cbWordingObjs", (exists ? evalWhatVals[24] : ""), "JudgementPromt").on("change", function() {
            if ($(this).val() == '') {
                $('#tbCustomWordingObjs').show().focus().select();
            } else {
                $('#tbCustomWordingObjs').hide();
            };
        });

        exists = false;
        if (evalWhatVals[25] != "") {
            $('#cbWordingAlts option').each(function(){
                if (this.value == evalWhatVals[25]) {
                    exists = true;
                    return false;
                }
            });
        }
        if (!exists) $('#tbCustomWordingAlts').show();

        initCombobox("cbWordingAlts", (exists ? evalWhatVals[25] : ""), "JudgementAltsPromt").on("change", function() {
            if ($(this).val() == '') {
                $('#tbCustomWordingAlts').show().focus().select();
            } else {
                $('#tbCustomWordingAlts').hide();
            };
        });

        if (evalWhatVals[26]) {
            $('#divGPWMode').show();
            $('#divGPWModeAlts').show();
        }else{
            $('#divGPWMode').hide();
            $('#divGPWModeAlts').hide();
        };
    }

    function initEvalHow() {
        //_PGID_PROJECT_OPTION_NAVIGATE 20014
        // === Evaluate How ===

        initCheckBox("cbNavHideNavBox", evalHowVals[0], "ShowProgressIndicator").click(function () { $('#cbNavDisableNavButtons').prop('disabled', $(this).is(':checked')); } );
        //initCheckBox("cbNavDisableNavButtons", evalHowVals[1], "AllowNavigation").click(function () { $('#cbNavShowNext').prop('disabled', $(this).is(':checked')); } ).prop('disabled', evalHowVals[0]);
        initCheckBox("cbNavDisableNavButtons", evalHowVals[1], "AllowNavigation").prop('disabled', evalHowVals[0]);
        //initCheckBox("cbNavShowNext", evalHowVals[2], "ShowNextUnassessed").prop('disabled', evalHowVals[1]);
        initCheckBox("cbNavShowNext", evalHowVals[2], "ShowNextUnassessed");
        initCheckBox("cbNavMustComplete", evalHowVals[4], "AllowMissingJudgments");

        initRadioGroup("optNavAutoAdvance", evalHowVals[3], "AllowAutoadvance");

        var tVal = 0; // keep
        if (evalHowVals[9]) tVal = 1;   // close
        if (evalHowVals[5]) tVal = 2;   // redirect
        if (evalHowVals[10]) tVal = 3;   // next prj
        if (is_risk && evalHowVals[8]) tVal = 4;  // join
        initRadioGroup("optNavAtFinish", tVal, "ActionAtFinish").click( function () { 
            var isChecked = $(this).is(':checked');
            checkAtFinishOptions();
            if ($(this).val() == 2 && isChecked) $('#tbRedirectURL').focus();
            if ($(this).val() == 3) {
                var logoff = $("#cbNavLogOff");
                if (isChecked && logoff.is(':checked')) logoff.prop("checked", false);
                if (isChecked && $("#tbNextModelName").val()=="") {
                    showProjects();
                }
            }
        });

        initInput("tbRedirectURL", evalHowVals[6], "TerminalRedirectURL", false, false).prop('disabled', !evalHowVals[5]);
        initCheckBox("cbNavLogOff", evalHowVals[7], "LogOffAtTheEnd").click(function () { checkAtFinishOptions(); });

        initCheckBox("cbUseCaching", evalHowVals[12], "UseCaching");

        $("#tbNextModelName").prop('readonly', true);
        checkAtFinishOptions();
    }

    function initDispWhat() {
        // _PGID_PROJECT_OPTION_DISPLAY 20015
        // === Display What ===

        initRadioGroup("optDispInterRes", dispWhatVals[0], "LocalResultsView");
        initRadioGroup("optDispLocalResSort", dispWhatVals[1], "LocalResultsSortMode");

        initCheckBox("cbShowExpValInterRes", dispWhatVals[2], "ShowExpectedValueLocal");
        initCheckBox("cbShowIndexInterRes", dispWhatVals[22], "show_index");
        <% If Not App.isRiskEnabled Then %>initCheckBox("cbHideInterNormalization", dispWhatVals[33], "HideLocalNormalization");<% End if %>
        initCheckBox("cbShowInterBars", dispWhatVals[35], "show_local_bars");

        initRadioGroup("optDispOverallRes", dispWhatVals[3], "GlobalResultsView");
        initRadioGroup("optDispGlobalResSort", dispWhatVals[4], "GlobalResultsSortMode");
        
        initCheckBox("cbShowExpValOverallRes", dispWhatVals[5], "ShowExpectedValueGlobal");
        initCheckBox("cbShowIndexOverallRes", dispWhatVals[23], "show_index_global");
        <% If Not App.isRiskEnabled Then %>initCheckBox("cbHideOverallNormalization", dispWhatVals[34], "HideGlobalNormalization");<% End if %>
        initCheckBox("cbShowOverallBars", dispWhatVals[36], "show_global_bars");

        initCheckBox("cbShowWelcomePage", dispWhatVals[6], "ShowWelcomeScreen")
        initCheckBox("cbShowThankYouPage", dispWhatVals[7], "ShowThankYouScreen").click(function () { 
            var isChecked = $(this).is(':checked');
            $("#cbShowRewardThankYouPage").prop("disabled", (isChecked ? "" : "disabled"));
            <% if Not isReadOnly() Then %>$("#btnEditRewardThankyou").prop("disabled", (isChecked ? "" : "disabled"));<% End If %>
        });

        initCheckBox("cbShowRewardThankYouPage", dispWhatVals[25], "ShowRewardThankYouScreen").prop("disabled", (dispWhatVals[7] ? "" : "disabled"));
        <% If Not isReadOnly() Then %>$("#btnEditRewardThankyou").prop("disabled", (dispWhatVals[7] ? "" : "disabled"));<% End If %>
        
        initCheckBox("cbDispInconsist", dispWhatVals[8], "ShowConsistencyRatio");
        initCheckBox("cbShowInfodoc", dispWhatVals[9], "ShowInfoDocs").click(function () { 
            var isChecked = $(this).is(':checked');
            $("#cbShowFramedInfodoc").prop("disabled", (isChecked ? "" : "disabled"));
            $("#cbHideInfodocCaptions").prop("disabled", (isChecked && $("#cbShowFramedInfodoc").is(":checked") ? "" : "disabled"));
        });
        initCheckBox("cbShowFramedInfodoc", dispWhatVals[10], "ShowInfoDocsMode").prop("disabled", ((dispWhatVals[9]) ? "" : "disabled")).click(function () { 
            var isChecked = $(this).is(':checked');
            $("#cbHideInfodocCaptions").prop("disabled", (isChecked ? "" : "disabled"));
        });
        initCheckBox("cbHideInfodocCaptions", dispWhatVals[28], "HideInfodocCaptions").prop("disabled", ((dispWhatVals[9] && dispWhatVals[10]) ? "" : "disabled"));
        initCheckBox("cbImagesZoom", dispWhatVals[30], "ImagesZoom");
        
        initRadioGroup("cbDispFullPath", dispWhatVals[11], "ShowFullObjectivePath");
        <% If _OPT_EVAL_T2S_ALLOWED Then %>initRadioGroup("cbT2S", dispWhatVals[32], "Text2Speech");<% End If %>
        initCheckBox("cbDispComments", dispWhatVals[12], "ShowComments");
        initCheckBox("cbDispIndividualCIS", dispWhatVals[13], "UseCISForIndividuals");
        initCheckBox("cbNavDisableWarningsOnStep", dispWhatVals[14], "DisableWarningsOnStepChange");
        
        //initCheckBox("cbCollapseMultiPWBars", dispWhatVals[24], "collapse_multipw_bars");
        
        initCheckBox("cbDispDSA", dispWhatVals[15], "ShowSensitivityAnalysis");
        initCheckBox("cbDispGSA", dispWhatVals[16], "ShowSensitivityAnalysisGradient");
        initCheckBox("cbDispPSA", dispWhatVals[17], "ShowSensitivityAnalysisPerformance");

        initRadioGroup("DispResSA", ((dispWhatVals[18]) ? 1 : 0), "CalculateSAForCombined");
        initRadioGroup("DispSynMode", ((dispWhatVals[19]) ? 1 : 0), "SynthesisMode");

        initCheckBox("cbShowRiskResults", dispWhatVals[29], "ShowRiskResults");
        initCheckBox("cbShowHeatMap", dispWhatVals[31], "ShowHeatMap");
    }

    function initSurveys() {
        // _PGID_PROJECT_OPTION_SURVEY 20016
        // === Show Surveys ===
        var surveys_allowed = <% =Bool2JS(CanEditSurveys) %>;

        initCheckBox("cbShowWelcomeSurvey", dispWhatVals[20], "ShowWelcomeSurvey").on("change", function () { $("input[name=rbWelcomeSurveyFirst]").prop("disabled", (surveys_allowed && $(this).is(':checked') && dispWhatVals[6] ? "" : "disabled")); } );
        initCheckBox("cbShowThankYouSurvey", dispWhatVals[21], "ShowThankYouSurvey").on("change", function () { $("input[name=rbThankYouSurveyLast]").prop("disabled", (surveys_allowed && $(this).is(':checked') && dispWhatVals[7] ? "" : "disabled")); } );

        initRadioGroup("rbWelcomeSurveyFirst", ((dispWhatVals[26]) ? 1 : 0), "WelcomeSurveyFirst");
        $("input[name=rbWelcomeSurveyFirst]").prop("disabled", (surveys_allowed && dispWhatVals[20] && dispWhatVals[6] ? "" : "disabled"));
        initRadioGroup("rbThankYouSurveyLast", ((dispWhatVals[27]) ? 1 : 0), "ThankYouSurveyLast");
        $("input[name=rbThankYouSurveyLast]").prop("disabled", (surveys_allowed && dispWhatVals[21] && dispWhatVals[7] ? "" : "disabled"));
    }

    <% If _OPT_CUSTOM_COMBINED Then %>
    var usr_list = null;
    var CID = <% = COMBINED_USER_ID %>;
    var combined_name_def = "<% =JS_SafeString(ResString_("lblEvaluationLegendCombined")) %>";
    var combined_uid = <% = PM.Parameters.ResultsCustomCombinedUID %>;
    var combined_name = "<% = JS_SafeHTML(PM.Parameters.ResultsCustomCombinedName) %>";

    function editCustomCombined() {
        if (usr_list==null) {
            getUsersWhoHasData();
        } else {
            showCustomCombined();
        }
    }

    function getUsersWhoHasData() {
        sendCommand("action=getusers", "onGetUsersWhoHasData(data);");
    }

    function onGetUsersWhoHasData(data) {
        hideLoadingPanel();
        if ((data) && data!="") usr_list = eval(data);
        showCustomCombined();
    }

    function showCustomCombined() {
        if (usr_list == null) usr_list = [];
        if (!usr_list.length || usr_list[0][0]!=CID) usr_list.splice(0, 0, [CID, "", "[ <% = JS_SafeString(ResString_("lblEvaluationLegendCombined").ToUpper) %> ]"]);
        //if (combined_name == "" && combined_uid<0) combined_name = combined_name_def;
        var lst = "";
        for (var i=0; i<usr_list.length; i++) {
            var sname = usr_list[i][2];
            if (sname=="") sname = usr_list[i][1];
            if (usr_list[i][1]!="") sname = "<a href='mailto:" + usr_list[i][1] + "'>" + sname + "</a>";
            lst += "<option value='" + usr_list[i][0] + "'" + (combined_uid == usr_list[i][0] ? " selected" : "") + (usr_list[i][0]<0 ? " style='background:#f0f5fa;'" : "") +  ">" + sname + "</option>";
        }
        lst = "<div style='margin:1em 0px 6px 0px; text-align:right'><nobr><b><% =JS_SafeString(ResString_("lblParamsCombinedUser")) %></b>&nbsp;<select name='combined_id' style='width:250px' onchange='onChangeCombined(this.value);'>" + lst + "</select></nobr></div>";
        var sText = "<p class='text' align='justify' style='margin:0px; max-width:485px;'><% = JS_SafeString(ResString_("lblPipeCustomCombined")) %></p>";
        sText += lst + "<div style='text-align:right'><nobr><b><% =JS_SafeString(ResString_("lblParamsCombinedName")) %></b>&nbsp;<input type='text' id='tbCombinedName' value='" + (combined_name == "" && combined_uid<0 ? combined_name_def : combined_name) + "' style='width:250px' onkeyup ='combined_name = this.value; CheckEmptyValue(this.value);'/></nobr></div>";
        dxDialog(sText, "setCombinedUser();", ";", "<% =JS_SafeString(ResString_("lblParamsSelectCombined")) %>");
        setTimeout('CheckEmptyValue($("#tbCombinedName").val());', 100);
    }

    function onChangeCombined(val) {
        var n = $("#tbCombinedName");
        //if (combined_name == "" && val>=0) {
        if (val>=0) {
            for (var i=0; i<usr_list.length; i++) {
                if (usr_list[i][0] == val) {
                    combined_name = usr_list[i][2];
                    n.val(combined_name); 
                    break; 
                }
            }
        }
        //if (val==CID && (n.val()=="" || combined_name=="")) n.val(combined_name_def);
        if (val==CID) {
            combined_name = "";
            n.val(combined_name_def); 
        }
        combined_uid = val;
    }

    function CheckEmptyValue(val) {
        dxDialogBtnDisable(true, (val.trim()==""));
    }

    function setCombinedUser() {
        sendCommand("action=set_combined&uid=" + combined_uid + "&name=" + encodeURIComponent(combined_name), "onSetCombined(data);");
    }

    function onSetCombined(data) {
        hideLoadingPanel();
        if (data!="") $(".custom_combined_name").html(data);
    }
    <% End If%>

    function onSetPage(pgid) {
        var curPgID = pgid % <%=_PGID_MAX_MOD%>;
        switch (curPgID) {
            case <% =CInt(_PGID_PROJECT_OPTION_EVALUATE)%>:
            case <% =CInt(_PGID_PROJECT_OPTION_NAVIGATE)%>:
            case <% =CInt(_PGID_PROJECT_OPTION_DISPLAY)%>:
            case <% =CInt(_PGID_PROJECT_OPTION_SURVEY)%>:
                pgID = pgid;
                initOptions();
                if (typeof nav_json != "undefined") {
                    var pg = pageByID(nav_json, pgid);
                    if ((pg) && typeof pg.title!="undefined") {
                        $("#divPageTitle").html(pg.title);
                    }
                }
                return true;
        }
        return false;
    }

    function copySettings() {
        var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("confOverrideSettings")) %>", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
        result.done(function (dialogResult) {
            if (dialogResult) {
                sendCommand("action=copy_settings");
            }
        });
    }

    function initOptions() {
        showLoadingPanel();
        var curPgID = pgID % <%=_PGID_MAX_MOD%>;
        $("#divEvalWhat").toggle(curPgID == <% =CInt(_PGID_PROJECT_OPTION_EVALUATE)%>);
        $("#divEvalHow").toggle(curPgID == <% =CInt(_PGID_PROJECT_OPTION_NAVIGATE)%>);
        $("#divDispWhat").toggle(curPgID == <% =CInt(_PGID_PROJECT_OPTION_DISPLAY)%>);
        $("#divSurveys").toggle(curPgID == <% =CInt(_PGID_PROJECT_OPTION_SURVEY)%>);
        <% If App.isRiskEnabled AndAlso Not isReadOnly() Then %>var b = $("#btnCopySettings");
        if ((b) && (b.length)) {
            b.show().dxButton({
                text: "<% = JS_SafeString(ResString(If(App.ActiveProject.isImpact, "btnCopySettingsToLikelihood", "btnCopySettingsToImpact"))) %>",
                //type: "success",
                useSubmitBehavior: false,
                onClick: function (e) {
                    copySettings();
                    return false;
                }
            });
        }
        <% End If %>
        initForm();
        onResize();
        hideLoadingPanel();
        if (curPgID == <% =CInt(_PGID_PROJECT_OPTION_NAVIGATE)%> && prj_list == null) getProjectsList();
    }


    function initForm() {
        <%  If isReadOnly() Then %>$("#divPipeGlobal").find("input").prop("disabled", "disabled");
        $("#divPipeGlobal").find("select").prop("disabled", "disabled"); 
        theForm.disabled = true; <% Else %>$("#tmpInput").focus().blur().hide();<% End If %>
    }

    custom_resize = onResize();

    $(document).ready(function () {
        showLoadingPanel();
        initEvalWhat();
        initEvalHow();
        initDispWhat();
        initSurveys();
        initOptions();
        hideLoadingPanel();
    });

//-->
</script>
  
<div id="divPipeGlobal" class="whole"><% If App.isRiskEnabled AndAlso Not isReadOnly() Then %><div id="btnCopySettings" style="display:none; float:right; margin:1.1ex 1em;"></div><% End If %><h4 style="padding:1ex;<% If App.isRiskEnabled AndAlso CanEditActiveProject Then %>margin-left:8em;<% End if %>" id="divPageTitle"><% = PageTitle(CurrentPageID) %></h4>
<input type="text" id="tmpInput" style="width:1px;height:1px;border:0px" />
<div id="divEvalWhat" style="display:none;">
<table style="width:99%;">
    <tr>
        <td class="opt_td opt_td_objs text">
            <input id="cbEvalObjectives" type="checkbox" checked="checked" /><label for="cbEvalObjectives" style="font-weight:bold"><% =ResString_("optEvalObjectives")%></label><br />
            <br />
            <span class="opt_obj_span">
<%--            <label style="font-weight:bold"><% =ResString_("optEvalDefaultCovObjsType")%></label>
            <select id="cbDefaultMeasurementTypeObjs">
                <option value="0"><% =ResString_("optPairwise")%></option>
                <option value="5"><% =ResString_("optDirect")%></option>
            </select><br />
            <br />--%>
            <span style="font-weight:bold"><% =ResString_("optEvalObjsOrder")%></span><br />
            <input id="rbEvalFromTop" type="radio" name="optEvalObjsOrder" value="0" /><label for="rbEvalFromTop"><% =ResString_("optEvalObjsOrderTop")%></label><br />                                 
            <input id="rbEvalFromBottom" type="radio" name="optEvalObjsOrder" value="1" /><label for="rbEvalFromBottom"><% =ResString_("optEvalObjsOrderBottom")%></label>  
            </span>      
        </td>
        <td class="opt_td opt_td_alts text">
            <input id="cbEvalAlternatives" type="checkbox" /><label for="cbEvalAlternatives" style="font-weight:bold"><% =ResString_("optEvalAlternatives")%></label><br />
            <br />
            <span class="opt_alt_span">
            <span style="font-weight:bold"><% =ResString_("optEvalDefaultCovObjsType")%></span>
                <select id="cbDefaultMeasurementTypeAlts">
                    <option value="0"><% =ResString_("optPairwise")%></option>
                    <option value="1"><% =ResString_("optRatingScale")%></option>
                    <option value="2"><% =ResString_("optUtilityCurve")%></option>
                    <option value="5"><% =ResString_("optDirect")%></option>
                    <option value="6"><% =ResString_("optStepFunction")%></option>
                </select><br />
            <% If _OPT_SHOW_FEEDBACK_ON Then %>
            <br />
            <input id="cbFeedBack" type="checkbox" /><label for="cbFeedBack" style="font-weight:bold"><% =ResString_("optEvalAltsFeedback")%></label><br />
            <% End If %>
            </span>
        </td>
    </tr>
    <tr>
        <td class="opt_td opt_td_objs text opt_td_objs1">
            <span class="opt_obj_span">
                <span style="font-weight:bold"><% =ResString_("optEvalWhatObjs")%></span><br />
                <input type="radio" name="EvalWhatObjs" value="1" id="rbPrioritizeObjectives1"/><label for="rbPrioritizeObjectives1"><% =ResString_("optEvalObjectivesPairwiseOne")%></label><br />                                 
                <input type="radio" name="EvalWhatObjs" value="0" id="rbPrioritizeObjectives2"/><label for="rbPrioritizeObjectives2"><% =ResString_("optEvalObjectivesPairwiseAll")%></label><br />    
            </span>
        </td>
        <td class="opt_td opt_td_alts text opt_td_alts1">
            <span class="opt_alt_span">
                <span style="font-weight:bold"><% =ResString_("optEvalWhatAlts")%></span><br />
                <br />
                <span style="font-weight:bold"><% =ResString_("optEvalIFPairwise")%></span><br />
                <input id="rbPrioritizeAlts1" type="radio" name="EvalWhatAlts" value="1"/><label for="rbPrioritizeAlts1"><% =ResString_("optEvalAlternativesPairwiseOne")%></label><br />                                 
                <input id="rbPrioritizeAlts2" type="radio" name="EvalWhatAlts" value="0"/><label for="rbPrioritizeAlts2"><% =ResString_("optEvalAlternativesPairwiseAll")%></label><br />
                <br />
                <span style="font-weight:bold"><% =ResString_("optEvalIFRatings")%></span><br />
                <input id="rbEvalRSOneObjToAlts" type="radio" name="EvalAlts" value="1"/><label for="rbEvalRSOneObjToAlts"><% =ResString_("optEvalRSOneObjToAlts")%></label><br />                                 
                <input id="rbEvalOneAltToObjs" type="radio" name="EvalAlts" value="2"/><label for="rbEvalOneAltToObjs"><% =ResString_("optEvalRSOneAltToObjs")%></label><br />
                <input id="rbEvalRSOneAltAlts" type="radio" name="EvalAlts" value="0"/><label for="rbEvalRSOneAltAlts"><% =ResString_("optEvalRSOneAltAlts")%></label><br />                                 
                <input id="rbEvalRSOneAltObjs" type="radio" name="EvalAlts" value="3"/><label for="rbEvalRSOneAltObjs"><% =ResString_("optEvalRSOneAltObjs")%></label><br />
                <br />
                <% If App.isRiskEnabled Then%>
                <input id="cbShowEventID" type="checkbox" /><label for="cbShowEventID"><% =ResString_("lblShowEventNumbers")%></label>: 
                <select id="cbIDColumnMode">
                    <option value="<% = CInt(IDColumnModes.IndexID) %>"><% =ResString_("optIndexID")%></option>
                    <option value="<% = CInt(IDColumnModes.UniqueID) %>"><% =ResString_("optUniqueID")%></option>
                </select><% End If%>
            </span>
        </td>
    </tr>
    <tr>
        <td class="opt_td opt_td_objs text opt_td_objs1">
            <span class="opt_obj_span">
                <span style="font-weight:bold"><% =ResString_("optEvalDiagonals")%></span><br />
                <label><% =ResString_("optEvalDiagonalsHint")%></label><br />
                <input id="rbEvalDiagAll" type="radio" name="Diagonals" value="0" /><label for="rbEvalDiagAll"><% =ResString_("optEvalDiagAll")%></label><br />                                 
                <input id="rbEvalDiagFirstSecond" type="radio" name="Diagonals" value="2" /><label for="rbEvalDiagFirstSecond"><% =ResString_("optEvalDiagFirstSecond")%></label><br />
                <input id="rbEvalDiagFirst" type="radio" name="Diagonals" value ="1"/><label for="rbEvalDiagFirst"><% =ResString_("optEvalDiagFirst")%></label><br />
                <input id="cbEvalPWAllDiagonals" type="checkbox" /><label for="cbEvalPWAllDiagonals"><% =ResString_("optEvalPWAllDiagonals")%></label>
                <input id="tbForceAllDiagonalsLimit" type="number" value="0" style="width:30px; margin:5px" /><label><% =ResString_("optEvalPWAllDiagonals2")%></label><br />
                <br />
                <span style="font-weight:bold"><% =ResString_("optEvalPairwiseType")%></span><br />
                <input id="rbEvalPWGraphical" type="radio" name="PWType" value="2"/><label for="rbEvalPWGraphical"><% =ResString_("optEvalPWGraphical")%></label><br /> 
                <div id="divGPWMode" style="margin-left:18px">
                    <input id="rbGPWMode9" type="radio" name="GPWMode" value="0"/><label for="rbGPWMode9"><% =ResString_("optEvalGPW_9")%></label><br />
                    <input id="rbGPWMode99" type="radio" name="GPWMode" value="1"/><label for="rbGPWMode99"><% =ResString_("optEvalGPW_99")%></label><br />
                    <input id="rbGPWModeUnlim" type="radio" name="GPWMode" value="2"/><label for="rbGPWModeUnlim"><% =ResString_("optEvalGPW_Unlimited")%></label><br />
                </div>                                 
                <input id="rbEvalPWVerbal" type="radio" name="PWType" value="1"/><label for="rbEvalPWVerbal"><% =ResString_("optEvalPWVerbal")%></label><br />
                <input id="cbEvalPWForceGraphical" type="checkbox" style="margin-left:18px" /><label for="cbEvalPWForceGraphical"><% =ResString_("optEvalPWForceGraphical")%></label>
            </span>
        </td>
        <td class="opt_td opt_td_alts text opt_td_alts1">
            <span class="opt_alt_span">
                <span style="font-weight:bold"><% =ResString_("optEvalDiagonals")%></span><br />
                <label><% =ResString_("optEvalDiagonalsHint")%></label><br />
                <input id="rbEvalDiagAllAlts" type="radio" name="DiagonalsAlts"  value="0" /><label for="rbEvalDiagAllAlts"><% =ResString_("optEvalDiagAll")%></label><br />                                 
                <input id="rbEvalDiagFirstSecondAlts" type="radio" name="DiagonalsAlts" value="2" /><label for="rbEvalDiagFirstSecondAlts"><% =ResString_("optEvalDiagFirstSecond")%></label><br />
                <input id="rbEvalDiagFirstAlts" type="radio" name="DiagonalsAlts" value ="1"/><label for="rbEvalDiagFirstAlts"><% =ResString_("optEvalDiagFirst")%></label><br />
                <input id="cbEvalPWAllDiagonalsAlts" type="checkbox" /><label for="cbEvalPWAllDiagonalsAlts"><% =ResString_("optEvalPWAllDiagonals")%></label>
                <input id="tbForceAllDiagonalsLimitAlts" type="number" value="5" style="width:30px; margin:5px" /><label><% =ResString_("optEvalPWAllDiagonals2")%></label><br />
                <br />
                <span style="font-weight:bold"><% =ResString_("optEvalPairwiseType")%></span><br />
                <input id="rbEvalPWGraphicalAlts" type="radio" name="PWTypeAlts" value="2"/><label for="rbEvalPWGraphicalAlts"><% =ResString_("optEvalPWGraphical")%></label><br />
                <div id="divGPWModeAlts" style="margin-left:18px">
                    <input id="rbGPWModeAlts9" type="radio" name="GPWModeAlts" value="0"/><label for="rbGPWModeAlts9"><% =ResString_("optEvalGPW_9")%></label><br />
                    <input id="rbGPWModeAlts99" type="radio" name="GPWModeAlts" value="1"/><label for="rbGPWModeAlts99"><% =ResString_("optEvalGPW_99")%></label><br />
                    <input id="rbGPWModeAltsUnlim" type="radio" name="GPWModeAlts" value="2"/><label for="rbGPWModeAltsUnlim"><% =ResString_("optEvalGPW_Unlimited")%></label><br />
                </div>                               
                <input id="rbEvalPWVerbalAlts" type="radio" name="PWTypeAlts" value="1"/><label for="rbEvalPWVerbalAlts"><% =ResString_("optEvalPWVerbal")%></label><br />
                <input id="cbEvalPWForceGraphicalAlts" type="checkbox" style="margin-left:18px" /><label for="cbEvalPWForceGraphicalAlts"><% =ResString_("optEvalPWForceGraphical")%></label> 
            </span>
        </td>
    </tr>
    <tr>
        <td class="opt_td opt_td_objs text opt_td_objs1"> 
            <span class="opt_obj_span">   
            <span style="font-weight:bold"><% =ResString_("optNavWordingObjs")%></span><br /><br />
                <label><% =ResString_("optWhichOfTheTwo")%></label>
                <input id="tbObjName" type="text" style="width:80px" />
                <label><% =ResString_("optBelow")%></label>
                <select id="cbWordingObjs">
<% =GetWordingOptions(false) %>
                </select>
                <input id="tbCustomWordingObjs" type="text" />
            </span>
        </td>
        <td class="opt_td opt_td_alts text opt_td_alts1">  
            <span class="opt_alt_span">
                <span style="font-weight:bold"><% =ResString_("optNavWordingAlts")%></span><br /><br />
                <label><% =ResString_("optWhichOfTheTwo")%></label>
                <input id="tbAltName" type="text" style="width:80px" />
                <label><% =ResString_("optBelow")%></label>
                <select id="cbWordingAlts">
<% =GetWordingOptions(true) %>
                </select>
                <input id="tbCustomWordingAlts" type="text" />
            </span>
        </td>
    </tr>
    <tr>
        <td colspan="2" class="opt_td text">   
            <span style="font-weight:bold"><% =ResString_("optEvalOrder")%></span><br />
            <input id="rbEvalOrder1" type="radio" name="EvalOrder" value="0" /><label for="rbEvalOrder1"><% =ResString_("optEvalOrderObjs")%></label><br />
            <input id="rbEvalOrder2" type="radio" name="EvalOrder" value="1" /><label for="rbEvalOrder2"><% =ResString_("optEvalOrderAlts")%></label><br />
            <% If App.isRiskEnabled Then%><br />
            <span  style="font-weight:bold"><% =ResString_("optParseValueFromNameHeader")%></span><br />
            <input id="cbParseValueFromName" type="checkbox" /><label for="cbParseValueFromName"><% =ResString_("optParseValueFromName")%></label><br /><% End if %>
        </td>
    </tr>
</table>
</div>

<div id="divEvalHow" style="display:none">
<table style="width:99%;" >
    <tr>
        <td colspan="2" class="opt_td text">   
            <input id="cbNavHideNavBox" type="checkbox" /><label for="cbNavHideNavBox"><% =ResString_("optNavHideNavBox")%></label><br /> 
            <input id="cbNavDisableNavButtons" type="checkbox" style="margin-left:18px" /><label for="cbNavDisableNavButtons"><% =ResString_("optNavDisableNavButtons")%></label><br />
            <input id="cbNavShowNext" type="checkbox" /><label for="cbNavShowNext"><% =ResString_("optNavShowNext")%></label><br />  
            <input id="cbNavMustComplete" type="checkbox" /><label for="cbNavMustComplete"><% =ResString_("optNavMustComplete")%></label><br />  
            <%--<input id="cbNavJoinPipes" type="checkbox" /><label for="cbNavJoinPipes"><% =ResString_("optNavJoinPipes")%></label><br />--%>
            <br />
            <%--<input id="cbNavAutoAdvance" type="checkbox" /><label for="cbNavAutoAdvance"><% =ResString_("optNavAutoAdvance")%></label><br />  --%>
            <input type="radio" name="optNavAutoAdvance" value="1" id="rbAutoAdvanceOn" /><label for="rbAutoAdvanceOn"><% =ResString_("optAutoAdvanceOn")%></label><br />
            <input type="radio" name="optNavAutoAdvance" value="0" id="rbAutoAdvanceOff" /><label for="rbAutoAdvanceOff"><% =ResString_("optAutoAdvanceOff")%></label><br />
            <input type="radio" name="optNavAutoAdvance" value="-1" id="rbAutoAdvanceOffNoAsk" /><label for="rbAutoAdvanceOffNoAsk"><% =ResString_("optAutoAdvanceOffNoAsk")%></label><br />
            <br />
            <b><% =ResString_("optAfterCollect")%></b>:<br />
            <input type="radio" name="optNavAtFinish" value="0" id="rbNavKeepPage" checked /><label for="rbNavKeepPage"><% =ResString_("optNavKeepPage")%></label><br />
            <input type="radio" name="optNavAtFinish" value="1" id="rbNavCloseWin"/><label for="rbNavCloseWin"><% =ResString_("optNavCloseWin")%></label><br />
            <% If App.isRiskEnabled Then%><input type="radio" name="optNavAtFinish" value="4" id="rbNavJoin"/><label for="rbNavJoin"><% =ResString_("optNavJoinPipes")%></label><br /><% End If %>
            <input type="radio" name="optNavAtFinish" value="2" id="rbNavRedirect" /><label for="rbNavRedirect"><% =ResString_("optNavRedirect")%>:</label>&nbsp;<input id="tbRedirectURL" type="text" disabled="disabled" value="" style="width:420px;" spellcheck="false" /><br />
            <input type="radio" name="optNavAtFinish" value="3" id="rbNavOpenPrj" /><label for="rbNavOpenPrj"><% =ResString_("optNavOpenPrj")%>:</label>&nbsp;<input id="tbNextModelName" type="text" disabled="disabled" value="" style="width:308px;" readonly="readonly" autocomplete="off" spellcheck="false" onfocus="this.blur();" />&nbsp;<input type="button" id="btnSelProject" class="button" disabled="disabled" value="Choose..." style="padding: 2px;" onclick="showProjects(); return false;" /><br />
            <input id="cbNavLogOff" type="checkbox" /><label for="cbNavLogOff"><% =ResString_("optNavLogOff")%></label><br />
            <% If PM.Parameters.SpecialMode <> "" Then %><br /><br /><b><% =ResString_("lblSpecialMode") %></b>: <% =If(App.CurrentLanguage.Resources.ParameterExists("lblMode" + PM.Parameters.SpecialMode), ResString("lblMode" + PM.Parameters.SpecialMode), PM.Parameters.SpecialMode) %> &nbsp;<a href="?pgid=<% =CurrentPageID %>&mode=clear" onclick="return confirm('<% =JS_SafeString(ResString("confResetSpecialMode")) %>');"><i class="fas fa-trash-alt"></i></a><% End if %>
        </td>
    </tr>
</table>

<div id="divModelsList" style="display:none;">
    <div style="margin-bottom:3px; text-align:right;"><% =ResString_("lblOptSearchModel")%>: <input type='text' name='inpSearch' id='inpSearch'  value='' onkeyup='onSearchProject();' onmouseup='onSearchProject();' style="width:150px" /></div>
    <h6><% = JS_SafeString(ResString_("lblOptSelectProject"))%>:</h6>
    <div style="overflow-y:auto; height:350px; border:1px solid #f0f0f0; padding:3px; margin:3px 0px;" class="text" id="divModels"></div>
    <div style="padding:4px 0px; <% =IIf(App.isRiskEnabled, "", "display:none;") %>" id="divPipe"><% =ResString_("lblOptSelPipe") %>:&nbsp; 
        <input type="radio" name="optEvalPipe" value="0" id="rbEvalL" checked /><label for="rbEvalL"><% = ParseString(ResString_("lblLikelihood")) %></label>&nbsp; 
        <input type="radio" name="optEvalPipe" value="1" id="rbEvalI" /><label for="rbEvalI"><% = ParseString(ResString_("lblImpact"))%></label>&nbsp; 
        <input type="radio" name="optEvalPipe" value="2" id="rbEvalR" /><label for="rbEvalR"><% = ParseString(ResString_("lblControls"))%></label>
    </div>
</div>

<%--<div class="text" style="margin:1em;">
    <input id="cbUseCaching" type="checkbox" /><label for="cbUseCaching"><% =ResString_("optUseCaching")%></label>&nbsp;<sup class="gray">beta</sup><br />
</div>--%>

</div>

<div id="divDispWhat" style="display:none">
<table style="width:99%;">
    <tr>
        <td style="width:50%" class="opt_td text">  
            <span style="font-weight:bold"><% =ResString_("optDispInterRes")%></span><br />
            <input type="radio" name="optDispInterRes" value="-1" id="rbIntermediateHide" /><label for="rbIntermediateHide"><% =ResString_("optDispResHide")%></label><br />
            <input type="radio" name="optDispInterRes" value="1" id="rbIntermediateIndividual" /><label for="rbIntermediateIndividual"><% =ResString_("optDispResIndividual")%></label> <nobr>(<% =ResString("lblATOnly") %>)</nobr><br />
            <input type="radio" name="optDispInterRes" value="2" id="rbIntermediateCombined" /><label for="rbIntermediateCombined"><% =GetCombinedName(True) %></label> <nobr>(<% =ResString("lblATOnly") %>)</nobr><br />
            <input type="radio" name="optDispInterRes" value="0" id="rbIntermediateBoth" /><label for="rbIntermediateBoth"><% =ResString_("optDispResBoth")%></label><br />
            <input type="checkbox" id="cbShowExpValInterRes" /><label for="cbShowExpValInterRes"><% =ResString_("optShowExpectedValue")%></label><br />
            <input type="checkbox" id="cbShowIndexInterRes" /><label for="cbShowIndexInterRes"><% =ResString_("lblShowIndex")%></label><br />
            <% If Not App.isRiskEnabled Then %><input type="checkbox" id="cbHideInterNormalization" /><label for="cbHideInterNormalization"><% =ResString_("lblEvalHideNormalization")%></label><br /><% End If %>
            <input type="checkbox" id="cbShowInterBars" /><label for="cbShowInterBars"><% =ResString_("lblResultsShowBars")%></label><br />
            <br />
            <span style="font-weight:bold"><% =ResString_("optDispLocalResSort")%></span><br />
            <input type="radio" name="optDispLocalResSort" value="0" id="rbLocalResSortNumber" /><label for="rbLocalResSortNumber"><% =ResString_("optDispResSortNumber")%></label><br />
            <input type="radio" name="optDispLocalResSort" value="1" id="rbLocalResSortName" /><label for="rbLocalResSortName"><% =ResString_("optDispResSortName")%></label><br />
            <input type="radio" name="optDispLocalResSort" value="2" id="rbLocalResSortPriority" /><label for="rbLocalResSortPriority"><% =ResString_(If(App.isRiskEnabled, "optDispResSortPrtyRisk", "optDispResSortPrty"))%></label><br />
            <input type="radio" name="optDispLocalResSort" value="3" id="rbLocalResSortCombined"  /><label for="rbLocalResSortCombined"><% =ResString_(If(App.isRiskEnabled, "optDispResSortCombinedRisk", "optDispResSortCombined"))%></label><br />
        </td>
        <td style="width:50%" class="opt_td text">  
            <span style="font-weight:bold"><% =ResString_("optDispOverallRes")%></span><br />
            <input type="radio" name="optDispOverallRes" value="-1" id="rbOverallHide" /><label for="rbOverallHide"><% =ResString_("optDispResHide")%></label><br />
            <input type="radio" name="optDispOverallRes" value="1" id="rbOverallIndividual" /><label for="rbOverallIndividual"><% =ResString_("optDispResIndividual")%></label> <nobr>(<% =ResString("lblATOnly") %>)</nobr><br />
            <input type="radio" name="optDispOverallRes" value="2" id="rbOverallCombined" /><label for="rbOverallCombined"><% =GetCombinedName(true) %></label> <nobr>(<% =ResString("lblATOnly") %>)</nobr><br />
            <input type="radio" name="optDispOverallRes" value="0" id="rbOverallBoth" /><label for="rbOverallBoth"><% =ResString_("optDispResBoth")%></label><br />
            <input type="checkbox" id="cbShowExpValOverallRes" /><label for="cbShowExpValOverallRes"><% =ResString_("optShowExpectedValue")%></label><br />
            <input type="checkbox" id="cbShowIndexOverallRes" /><label for="cbShowIndexOverallRes"><% =ResString_("lblShowIndex")%></label><br />
            <% If Not App.isRiskEnabled Then %><input type="checkbox" id="cbHideOverallNormalization" /><label for="cbHideOverallNormalization"><% =ResString_("lblEvalHideNormalization")%></label><br /><% End If %>
            <input type="checkbox" id="cbShowOverallBars" /><label for="cbShowOverallBars"><% =ResString_("lblResultsShowBars")%></label><br />
            <%--<input type="checkbox" id="cbUseAIP" /><label for="cbUseAIP"><% =ResString_("optUseAIP")%></label><br />  --%>
            <br />
            <span style="font-weight:bold"><% =ResString_("optDispGlobalResSort")%></span><br />
            <input type="radio" name="optDispGlobalResSort" value="0" id="rbGlobalResSortNumber" /><label for="rbGlobalResSortNumber"><% =ResString_("optDispResSortNumber")%></label><br />
            <input type="radio" name="optDispGlobalResSort" value="1" id="rbGlobalResSortName" /><label for="rbGlobalResSortName"><% =ResString_("optDispResSortName")%></label><br />
            <input type="radio" name="optDispGlobalResSort" value="2" id="rbGlobalResSortPriority" /><label for="rbGlobalResSortPriority"><% =ResString_(If(App.isRiskEnabled, "optDispResSortPrtyRisk", "optDispResSortPrty"))%></label><br />
            <input type="radio" name="optDispGlobalResSort" value="3" id="rbGlobalResSortCombined" /><label for="rbGlobalResSortCombined"><% =ResString_(If(App.isRiskEnabled, "optDispResSortCombinedRisk", "optDispResSortCombined"))%></label><br />
        </td>
    </tr>
    <tr>
        <td colspan="2" class="opt_td text">
            <table border="0" cellspacing="0" cellpadding="0">
                <tr valign="middle"><td class="text"><input type="checkbox" id="cbShowWelcomePage" /><label for="cbShowWelcomePage"><% =ResString_("optDispWelcomePage")%></label></td><td style="padding-left:1ex;"><input type="button" id="btnEditWelcome" <% =If(isReadOnly(), "disabled='disabled'", "") %> class="button" value="<% =ResString_("btnEditH")%>" onclick="OpenRichEditor(rich_url_prefix + 'welcome');" /></td></tr>
                <tr valign="middle"><td class="text"><input type="checkbox" id="cbShowThankYouPage" /><label for="cbShowThankYouPage"><% =ResString_("optDispThankYouPage")%></label></td><td style="padding-left:1ex;"><input type="button" id="btnEditThankyou" <% =If(isReadOnly(), "disabled='disabled'", "") %> class="button" value="<% =ResString_("btnEditH")%>" onclick="OpenRichEditor(rich_url_prefix + 'thankyou');"/></td></tr>
                <tr valign="middle"><td class="text" style="padding-left:16px;"><input type="checkbox" id="cbShowRewardThankYouPage" /><label for="cbShowRewardThankYouPage"><% =ResString_("optDispRewardThankYouPage")%></label></td><td style="padding-left:1ex;"><input type="button" id="btnEditRewardThankyou" <% =If(isReadOnly(), "disabled='disabled'", "") %> class="button" value="<% =ResString_("btnEditH")%>" onclick="OpenRichEditor(rich_url_prefix + 'thankyou&mode=reward');"/></td></tr>
            </table>
            <br />
            <span style="font-weight:normal; padding-left:0px;"><% =ResString_("optDispFullPath")%></span>: 
            <input type="radio" name="cbDispFullPath" value="0" id="cbDispFullPathNever" /><label for="cbDispFullPathNever"><% =ResString_("optDispFullPathNever")%></label>&nbsp; 
            <input type="radio" name="cbDispFullPath" value="1" id="cbDispFullPathAlways" /><label for="cbDispFullPathAlways"><% =ResString_("optDispFullPathAlways")%></label>&nbsp; 
            <input type="radio" name="cbDispFullPath" value="2" id="cbDispFullPathCollapse" /><label for="cbDispFullPathCollapse"><% =ResString_("optDispFullPathCollapse")%></label><br />
            <br />
            <% If _OPT_EVAL_T2S_ALLOWED AndAlso (App.isRiskEnabled OrElse Not App.isEvalURL_EvalSite) Then %> <span style="font-weight:normal; padding-left:0px;"><% =ResString_("optT2S")%></span>: 
            <input type="radio" name="cbT2S" value="0" id="cbT2SDisabled" /><label for="cbT2SDisabled"><% =ResString_("optT2SDisabled")%></label>&nbsp; 
            <input type="radio" name="cbT2S" value="1" id="cbT2SAuto" /><label for="cbT2SAuto"><% =ResString_("optT2SAuto")%></label>&nbsp; 
            <input type="radio" name="cbT2S" value="2" id="cbT2SRegular" /><label for="cbT2SRegular"><% =ResString_("optT2SRegular")%></label><br />
            <br /><% End If %>

            <input type="checkbox" id="cbDispInconsist" /><label for="cbDispInconsist"><% =ResString_("optDispInconsist")%></label>
            <span style="font-style:italic"><% =ResString_("optShowInconsistencyRatioHint")%></span><br />
            <input type="checkbox" id="cbShowInfodoc" /><label for="cbShowInfodoc"><% =ResString_("optDispInfoDocs")%></label><br />
            <div style="margin-left:18px">
                <input type="checkbox" id="cbShowFramedInfodoc" /><label for="cbShowFramedInfodoc"><% =ResString_("optShowFramedInfodocs")%></label>
                <span style="font-style:italic"><% =ResString_("optShowFramedInfodocsExtra")%></span><br />
                <div style="margin-left:18px">
                    <input type="checkbox" id="cbHideInfodocCaptions" /><label for="cbHideInfodocCaptions"><% =ResString_("optHideInfodocCaptions")%></label><br />
                </div>
            </div>
            <input type="checkbox" id="cbImagesZoom" /><label for="cbImagesZoom"><% =ResString_("optImagesZoom")%></label><br />
            <%--<input type="checkbox" id="cbDispFullPath" /><label for="cbDispFullPath"><% =ResString_("optDispFullPath")%></label><br />--%>
            <input type="checkbox" id="cbDispComments" /><label for="cbDispComments"><% =ResString_("optDispComments")%></label><br />
            <input type="checkbox" id="cbDispIndividualCIS" /><label for="cbDispIndividualCIS"><% =ResString_("optDispIndividualCIS")%></label><br />
            <input type="checkbox" id="cbNavDisableWarningsOnStep" /><label for="cbNavDisableWarningsOnStep"><% =ResString_("optNavDisableWarningsOnStep")%></label><br />
            <%--<input type="checkbox" id="cbCollapseMultiPWBars" /><label for="cbCollapseMultiPWBars"><% =ResString_("optCollapseMultiPWBars")%></label><br />--%>
        </td>
    </tr>
    <tr>
        <td style="width:50%" class="opt_td text" <% If CanShowEmbedded Then %>rowspan="2"<% End if %>>
            <span style="font-weight:bold"><% =ResString_("optDispSA")%></span><br />
            <input type="checkbox" id="cbDispDSA" /><label for="cbDispDSA"><% =ResString_("optDispDSA")%></label><br />
            <input type="checkbox" id="cbDispGSA" /><label for="cbDispGSA"><% =ResString_("optDispGSA")%></label><br />
            <input type="checkbox" id="cbDispPSA" /><label for="cbDispPSA"><% =ResString_("optDispPSA")%></label><br />
            <br />
            <span style="font-weight:bold"><% =ResString_("optDispResSA")%></span><br />
            <input type="radio" name="DispResSA" value="0" id="rbDispResIndividual" /><label for="rbDispResIndividual"><% =ResString_("optDispResIndividual")%></label><br />
            <input type="radio" name="DispResSA" value="1" id="rbDispResCombined" /><label for="rbDispResCombined"><% =ResString_("lblEvaluationLegendCombined")%></label><br />
        </td>
        <td style="width:50%" class="opt_td text">
            <span style="font-weight:bold"><% =ResString_("optDispSynthesis")%></span><br />
            <input type="radio" name="DispSynMode" value="0" id="rbDispDistribMode" /><label for="rbDispDistribMode"><% =ResString_("optDispDistribMode")%></label><br />
            <input type="radio" name="DispSynMode" value="1" id="rbDispIdealMode" /><label for="rbDispIdealMode"><% =ResString_("optDispIdealMode")%></label><br />
        </td>    
    </tr><% If CanShowEmbedded Then %>
    <tr>
        <td style="width:50%" class="opt_td text">
            <span style="font-weight:bold"><% =ResString_("lblEmbeddedContent")%></span><br />
            <input type="checkbox" id="cbShowRiskResults" /><label for="cbShowRiskResults"><% =ResString_("optShowRiskResults")%></label><br />
            <input type="checkbox" id="cbShowHeatMap" /><label for="cbShowHeatMap"><% =ResString_("optShowHeatMap")%></label><br />
        </td>    
    </tr><% End If %>
</table>
</div>

<div id="divSurveys" style="display:none">
<table style="width:99%;">
    <tr>
        <td class="opt_td text">
            
            <b><% =ResString_("optNavSurveyStart")%></b>
            <div style="padding:10px 0px 30px 0px">
                <input type="button" class="button" <% =If(isReadOnly(), "disabled='disabled'", "") %> id="btnEditWelcomeSurvey" value="<% =ResString_(CStr(IIf(CanEditSurveys, IIf(HasWelcomeSurvey, "btnEditH", "btnCreateH"), "btnSurveyView")))%>" style="width:11em; padding:4px 3px;" onclick="onOpenSurvey(true, <% =Bool2JS(HasWelcomeSurvey) %>);" <% =IIf(HasWelcomeSurvey OrElse CanEditSurveys(), "", "disabled=1")%> />
                <input type="button" class="button" <% =If(isReadOnly(), "disabled='disabled'", "") %> id="btnWelcomeSurveyUpload" value="<% =ResString_("btnSurveyUpload")%>" style="width:11em; padding:4px 3px;" onclick="UploadSurvey(true);" <% =IIf(CanEditSurveys, "", "disabled=1")%>/>
                <input type="button" class="button" <% =If(isReadOnly(), "disabled='disabled'", "") %> id="btnWelcomeSurveyDownload" value="<% =ResString_("btnSurveyDownload")%>" style="width:11em; padding:4px 3px;" onclick="onDownloadSurvey(true);" <% =IIf(HasWelcomeSurvey AndAlso HasSurveysLicense(), "", "disabled=1")%>/>
                <div style="margin-top:4px"><input type="checkbox" id="cbShowWelcomeSurvey" <% =IIf(HasWelcomeSurvey AndAlso CanEditSurveys(), "", "disabled=1")%>/><label for="cbShowWelcomeSurvey"><% =ResString_("lblShowSurvey")%></label>
                    <div style="padding:2px 0px 0px 20px">
                        <input type="radio" name="rbWelcomeSurveyFirst" value="1" id="rbWelcomeSurveyBefore" /><label for="rbWelcomeSurveyBefore"><% =ResString_("lblSurveyBeforeWelcome")%></label><br />
                        <input type="radio" name="rbWelcomeSurveyFirst" value="0" id="rbWelcomeSurveyAfter" /><label for="rbWelcomeSurveyAfter"><% =ResString_("lblSurveyAfterWelcome")%></label><br />
                    </div>
                </div>
            </div>

            <b><% =ResString_("optNavSurveyEnd")%></b>
            <div style="padding:10px 0px 30px 0px">
                <input type="button" class="button" <% =If(isReadOnly(), "disabled='disabled'", "") %> id="btnEditThankYouSurvey" value="<% =ResString_(CStr(IIf(CanEditSurveys, IIf(HasThankYouSurvey, "btnEditH", "btnCreateH"), "btnSurveyView")))%>" style="width:11em; padding:4px 3px;" onclick="onOpenSurvey(false, <% =Bool2JS(HasThankYouSurvey) %>);" <% =IIf(HasThankYouSurvey OrElse CanEditSurveys(), "", "disabled=1")%>/>
                <input type="button" class="button" <% =If(isReadOnly(), "disabled='disabled'", "") %> id="btnThankYouSurveyUpload" value="<% =ResString_("btnSurveyUpload")%>" style="width:11em; padding:4px 3px;" onclick="UploadSurvey(false);" <% =IIf(CanEditSurveys, "", "disabled=1")%>/>
                <input type="button" class="button" <% =If(isReadOnly(), "disabled='disabled'", "") %> id="btnThankYouSurveyDownload" value="<% =ResString_("btnSurveyDownload")%>" style="width:11em; padding:4px 3px;" onclick="onDownloadSurvey(false);" <% =IIf(HasThankYouSurvey AndAlso HasSurveysLicense(), "", "disabled=1")%>/>
                <div style="margin-top:4px"><input type="checkbox" id="cbShowThankYouSurvey" <% =IIf(HasWelcomeSurvey AndAlso CanEditSurveys(), "", "disabled=1")%>/><label for="cbShowThankYouSurvey"><% =ResString_("lblShowSurvey")%></label>
                    <div style="padding:2px 0px 0px 20px">
                        <input type="radio" name="rbThankYouSurveyLast" value="0" id="rbThankYouSurveyLastBefore" /><label for="rbThankYouSurveyLastBefore"><% =ResString_("lblSurveyBeforeThankYou")%></label><br />
                        <input type="radio" name="rbThankYouSurveyLast" value="1" id="rbThankYouSurveyLastAfter" /><label  for="rbThankYouSurveyLastAfter"><% =ResString_("lblSurveyAfterThankYou")%></label><br />
                    </div>
                </div>
            </div>
            <% If Not HasSurveysLicense() Then %><div style="text-align:center"><div class="warning" style="display:inline-block; margin:0px auto 1ex auto; padding:1ex 1em"><h6 style="padding:0px;"><% = String.Format(ResString("msgNoLicense"), ResString("titleApplicationSurvey")) %></h6></div></div><% End if %>
        </td>
    </tr>
</table>
<div id="divEditSpyron" style="display:none"></div>
</div>
</div>
</asp:Content>
