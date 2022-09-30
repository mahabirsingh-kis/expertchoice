<%@ Page Language="VB" MasterPageFile="~/mpEC09_TT.master" EnableEventValidation="false" Inherits="TeamTimeEvaluationPage" EnableViewState="false" Codebehind="TeamTime.aspx.vb" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxCallback" TagPrefix="dxcb" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" runat="Server">
<% #Const OPT_USE_AJAX_INSTEAD_CALLBACK = False %>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/sliderbars.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/twinslider.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.ztree.core.min.js"></script>
<!--script language="JavaScript" type="text/javascript" src='<% =_URL_ROOT %>Scripts/scrolltable.js'></script-->
<script language="javascript" type="text/javascript"><!--

    var img_path  = '<% =ImagePath %>';
    var img_blank = img_path + 'blank.gif';
    var img_loading = new Image;    img_loading.src = img_path + 'devex_loading.gif';
    var img_start = new Image;      img_start.src   = img_path + 'start.png';
    var img_start_= new Image;      img_start_.src  = img_path + 'start_.png';
    var img_pause = new Image;      img_pause.src   = img_path + 'pause.png';
    var img_pause_= new Image;      img_pause_.src  = img_path + 'pause_.png';
    var img_stop  = new Image;      img_stop.src    = img_path + 'stop.png';
    var img_stop_ = new Image;      img_stop_.src   = img_path + 'stop_.png';
    var img_setup = new Image;      img_setup.src   = img_path + 'settings.gif';
    var img_setup_= new Image;      img_setup_.src  = img_path + 'settings_.gif';
    var img_view  = new Image;      img_view.src    = img_path + 'view_sm.png';
    var img_edit  = new Image;      img_edit.src    = img_path + 'edit_tiny.gif';
    var img_plus  = new Image;      img_plus.src    = img_path + 'plus.gif';
    var img_plus_ = new Image;      img_plus_.src   = img_path + 'plus_green.gif';
    var img_minus = new Image;      img_minus.src   = img_path + 'minus.gif';
    var img_minus_= new Image;      img_minus_.src  = img_path + 'minus_green.gif';
    var img_comment   = new Image;    img_comment.src   = img_path + 'comment.png';
    var img_comment_  = new Image;    img_comment_.src  = img_path + 'comment_.png';
    var img_comment_e = new Image;    img_comment_e.src = img_path + 'comment_e.png';
    var img_comment_e_= new Image;    img_comment_e_.src= img_path + 'comment_e_.png';
    var img_commentd  = new Image;    img_commentd.src  = img_path + 'comment_dis.png';
    var img_commentd_ = new Image;    img_commentd_.src = img_path + 'comment_dis_.png';
    var img_infodoc1= new Image;    img_infodoc1.src   = img_path + 'info15.png';
    var img_infodoc2= new Image;    img_infodoc2.src   = img_path + 'info15_dis.png';
    var img_WRTinfodoc1= new Image; img_WRTinfodoc1.src = img_path + 'readme.gif';
    var img_WRTinfodoc2= new Image; img_WRTinfodoc2.src = img_path + 'readme_dis.gif';
    var img_decrease = new Image;   img_decrease.src    = img_path + 'decrease.gif';
    var img_decrease_= new Image;   img_decrease_.src   = img_path + 'decrease_.gif';
    var img_increase = new Image;   img_increase.src    = img_path + 'increase.gif';
    var img_increase_= new Image;   img_increase_.src   = img_path + 'increase_.gif';
    var img_delete   = new Image;   img_delete.src      = img_path + 'delete_tiny.gif';
    var img_delete_  = new Image;   img_delete_.src     = img_path + 'nodelete_tiny.gif';
    var img_swap     = new Image;   img_swap.src        = img_path + 'swap.gif';
    var img_list     = new Image;   img_list.src        = img_path + 'bars_16.png';
    var img_list_    = new Image;   img_list_.src       = img_path + 'bars_16_.png';
    var img_group    = new Image;   img_group.src       = img_path + 'group_16.png';
    var img_group_   = new Image;   img_group_.src      = img_path + 'group_16_.png';
    var img_reload   = new Image;   img_reload.src      = img_path + 'refresh-16.png';
    var img_reload_  = new Image;   img_reload_.src     = img_path + 'refresh-16_.png';

    _img_slider_images_path = img_path;
    _img_slider_position.src = _img_slider_images_path + "slider_position.gif";
    _img_slider_handler.src = _img_slider_images_path + "slider_handler.gif";
    _img_slider_handler2.src = _img_slider_images_path + "slider_handler2.gif";
    _img_slider_blank.src = img_blank;

    var _img_slider_blue = new Image();     _img_slider_blue.src = _img_slider_images_path + "slider_blue.gif";
    var _img_slider_red = new Image();      _img_slider_red.src = _img_slider_images_path + "slider_red.gif";

    var _img_loading_global = "/Images/preload.gif";

    var show_debug = <% =iif(CheckVar("debug", False), 1, 0)  %>;
    var dbg_win = null;
    var dbg_ln = 0;

    var tinybar_width = 40;

    var can_resize = true;

    var load_steps = -1;

    var sync_prj_id = <% = App.ProjectID %>;
    var sync_show_response = 0;
    var sync_queue = [];
    var sync_active = <% =iif(_TeamTimeActive, 1, 0) %>;
    var sync_decimals = 2;
//    var sync_dec_mul  = Math.pow(10, sync_decimals);
    var sync_timeout = <% =TeamTimeRefreshTimeout * 1000 %>;
    var sync_judg_timeout = 3;
    var sync_online = <% =iif(isTeamTime, 1, 0) %>;
    var sync_faults = -1;
    var sync_missed_call = 0;
    var sync_asked_reload = 0;
    var sync_last_answer = null;
    var sync_last_update = null;
    var sync_user = '<% =App.ActiveUser.UserEmail.ToLower() %>';
    var sync_owner = <% =iif(isTeamTime AndAlso isTeamTimeOwner, 1, 0) %>;
    var sync_eval_prg_user = '<% =if(isTeamTimeOwner, TeamTimeEvalProgressUser, App.ActiveUser.UserEmail).ToLower() %>';
    var sync_eval_prg_name = '';
    var sync_is_gpw = 0;
    var sync_step = -1;
    var sync_next_unas = -1;
    var sync_steps_total = -1;
    var sync_eval_progress = -1;
    var sync_hash = "";
    var sync_hash_old = "";
    var sync_first_call= 1;
    var sync_last_params = "";
    var sync_last_data = null;
    var sync_last_data_full = null;
    var sync_last_prg  = null;
    var sync_last_type = null;
    var sync_last_judgment_time = null;
    var sync_last_serverdate = null;
    var sync_step_guid = null;
    var sync_pending   = null;
    var sync_pending_id = "";
    var sync_force_redraw    = 0;
    var sync_sliders_can_recalc = 0;
    var sync_last_comment    = "";
    var sync_results_sorting = <% =CInt(Iif(TeamTimePipeParams.LocalResultsSortMode>1 , -TeamTimePipeParams.LocalResultsSortMode-1, TeamTimePipeParams.LocalResultsSortMode+1)) %>;   // 1 - No, 2 - Name, My - 3, Combined - 4;
    //var sync_results_options = <% =CInt(TeamTimePipeParams.LocalResultsView) %>;   // rvBoth = 0, rvIndividual = 1, rvGroup = 2
    var sync_results_norm    = <% =If(App.isRiskEnabled, "1", GetCookie(_COOKIE_TT_NORM, "0")) %>;  // Force Abolute for Riskion
    var sync_usekeypads      = <% =If(TeamTimePipeParams.SynchUseVotingBoxes AndAlso App.Options.KeypadsAvailable, 1, 0) %>;
    var sync_hidejudgments   = <% =If(TeamTimePipeParams.SynchStartInPollingMode, 1, 0) %>;
    var sync_hideowner       = <% =If(TeamTimePipeParams.TeamTimeHideProjectOwner, 1, 0) %>;
    var sync_hideoffline     = <% =If(PM.Parameters.TTHideOfflineUsers, 1, 0) %>;
    var sync_showviewonly    = <% =If(TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess, 1, 0) %>;
    var sync_show_variance   = 0;
    var sync_show_pwside     = 0;
    var sync_show2me         = -1;
    var sync_anonymous       = <% =If(TeamTimePipeParams.TeamTimeStartInAnonymousMode, 1, 0) %>;
    var sync_infodocsmode    = <% =If(TeamTimePipeParams.ShowInfoDocs, CInt(TeamTimePipeParams.ShowInfoDocsMode), -1) %>;  // sidmFrame = 0, sidmPopup = 1, -1 = no display;
    var sync_hide_captions   = <% =If(PM.Parameters.EvalHideInfodocCaptions, 1, 0) %>;
    var sync_showcomments    = <% =If(TeamTimePipeParams.ShowComments, 1, 0) %>;
    var sync_users_sorting   = <% =CInt(TeamTimePipeParams.TeamTimeUsersSorting) %>;
    var show_legend          = <% =If(TeamTimePipeParams.TeamTimeShowKeypadLegends, 1, 0) %>;
    var sync_show_indiv_res  = 1;
    var sync_distr_view      = 0;   // distribution view
    var sync_ratings_active_legend = true;

    var sync_showusers_by_pages = <% = GetUsersPageEnabled() %>;
    var sync_userspage_size = <% = GetUsersPageSize() %>;
    var sync_userspage_active = <% = GetUsersPageActive() %>;
    var sync_userspage_links = "";
    var sync_user_rowheight = -1;
    var sync_user_row_starts = -1;
    var sync_user_last_type = "";
    var sync_last_users_count = -1;

    var type_pw      = "pairwise";
    var type_rating  = "rating";
    var type_direct  = "direct";
    var type_step    = "step";
    var type_ruc     = "ruc";
    var type_msg     = "message";
    var type_l_res   = "localresults";
    var type_g_res   = "globalresults";
    var type_dsa     = "dsa";
    var type_info    = "info";
    
    <%--var help_root    = "<% =JS_SafeString(GetHelpRoot()) %>";
    var help_risk    = "<% =JS_SafeString(CStr(iif(App.isRiskEnabled, ResString(CStr(IIf(App.ActiveProject.isImpact, "navHelpImpact", "navHelpLikelihood"))), ""))) %>";
    var help_pw      = help_root + help_risk + "Measure/TeamTimeEvaluationPairwiseVerbalComparisons.html";
    var help_gpw     = help_root + help_risk + "Measure/TeamTime_Evaluation_Pairwise_Graphical_Numerical.html";
    var help_rating  = help_root + help_risk + "Measure/TeamTime_Evaluation_Ratings.html"; //A1010
    var help_direct  = help_root + help_risk + "Measure/TeamTime_Evaluation_Direct_Entry.html";
    var help_step    = help_root + help_risk + "Measure/TeamTime_Evaluation_Step_Function.html";
    var help_ruc     = help_root + help_risk + "Measure/TeamTime_Evaluation_Utility_Curve.html";
    var help_welcome = help_root + help_risk + "Measure/TeamTimeEvaluationWelcome.html";
    var help_thankyou= help_root;
    var help_l_res   = help_root;
    var help_g_res   = help_root;
    var help_dsa     = help_root;--%>
    
    var id_prefix   = "tt";
    var id_groupres = 999999999;

    // judgments
    var id_uid      = 0;
    var id_email    = 1;
    var id_name     = 2;
    var id_online   = 3;
    var id_keypad   = 4;
    var id_value    = 5;
    var id_comment  = 6;
    
    slider_prefix = id_prefix + 'slider';
//    slider_on_change_code = "Sync_onSubmitDSA();";
    
    var sa_node_maxlen = 30;

    // data
    var id_type     = 1;
    var id_task     = 2;
    var id_guid     = 3;
    var id_data     = 4;

    var pw_verbal   = <% = PairwiseType.ptVerbal %>;
    var pw_gpw_9    = 9;
    var pw_gpw_99   = 99;
    var pw_gpw_unlim = <% = PairwiseType.ptGraphical %>;

    var num_maxdecimals = <% =Math.Pow(10, _OPT_JSMaxDecimalPlaces) %>;
    
    var uid_combined = -1;
    
    var fill_edit = 'pw_edit';
    var fill_view = 'pw_view';
    var fill_cons = 'pw_cons';
    
    var fill_blue     = 'fill1';
    var fill_red      = 'fill2';
    var fill_disabled = 'fill_disabled';

    var value_undef = <% =TeamTimeFuncs.UndefinedValue %>;

    var sliders = new Array();
    var slider_consensus = null;

    var gpw_autoadvance = false;

    slider_on_change = "Sync_GPWChanged(slider_id, val, adv);";

    var slider_direct_max = 1000;
    var slider_is_pressed = 0;
    var slider_is_manual = 0;
   
    var steps = 8;
    var cell_width = 14;
    var cell_height = 14;
    var bar_height = 10;
    var bar_width = 150;
    var bar_padding = 1;
    var res_tree_width  = 320;
    
    var frm_pw_width    = "250";
    var frm_pw_margin   = "227";
    var frm_nonpw_width = "238";
    var frm_nonpw_margin= "205";
    var frm_height      = "90";
    var frm_title_len   = 38;
    
    var lbl_desc        = "&nbsp;&#9660;";
    var lbl_asc         = "&nbsp;&#9650;";
    var lbl_groupres    = '<% =JS_SafeString(ResString("lblConsensus")) %>';
    var lbl_consensus   = '<% =JS_SafeString(ResString("lblConsensusValue")) %>';
    var lbl_consensus_pw= '<% =JS_SafeString(ResString("lblConsensusPWValue")) %>';
    var lbl_username    = '<nobr><% =JS_SafeString(ResString("tblSyncUserName")) %></nobr>';
    var lbl_rating      = '<% =JS_SafeString(ResString("tblSyncRating")) %>';
    var lbl_value       = '<% =JS_SafeString(ResString("tblSyncValue")) %>';
    var lbl_priority    = '<% =JS_SafeString(ResString(CStr(IIf(App.isRiskEnabled, IIf(isImpact(), "tblSyncPriority_Impact", "tblSyncPriority_Likelihood"), "tblSyncPriority")))) %>';
    var lbl_bar         = '<% =JS_SafeString(ResString("tblSyncGraph")) %>';
    var lbl_bar_result  = '<% =JS_SafeString(ResString(CStr(iif(isLikelihood, "tblEvaluationResultGraph", "tblEvaluationResultGraphImpact")))) %>';
    var lbl_undefined   = '<% =JS_SafeString(ResString("lblEvaluationNotRated")) %>';
    var lbl_pending     = '<% =JS_SafeString(ResString("lblTTPending")) %>';
    var lbl_no_res      = '<% =JS_SafeString(ResString("msgNoLocalResultsTT")) %>';
    var lbl_no_res_con  = '<% =JS_SafeString(ResString("msgNoLocalResultsTT_Con")) %>';
    var lbl_no_res_coff = '<% =JS_SafeString(ResString("msgNoLocalResultsTT_Coff")) %>';
    var lbl_nooverall   = '<% =JS_SafeString(ResString("msgNoOverallResults")) %>';
    var lbl_num         = '<% =JS_SafeString(ResString("tblEvaluationResultNum")) %>';
    var lbl_nodename    = '<% =JS_SafeString(ResString(CStr(iif(isLikelihood, "tblEvaluationResultName", "tblEvaluationResultNameImpact")))) %>';
    var lbl_individual  = '<% =JS_SafeString(ResString("lblEvaluationLegendUser")) %>';
    var lbl_combined    = '<% =JS_SafeString(GetCombinedName()) %>';
    var lbl_inconsist   = '<% =JS_SafeString(ResString("lblEvaluationInconsistencyRatio")) %>';
    var lbl_viewonly    = '<% =JS_SafeString(ResString("lblSynchronousViewOnly")) %>';
    var lbl_comment     = '<% =JS_SafeString(ResString("lblTTCommentView")) %>';
    var lbl_comment_e   = '<% =JS_SafeString(ResString("lblTTCommentEdit")) %>';
    var lbl_comment_text= '<% =JS_SafeString(ResString("lblTTCommentHint")) %>';
    var lbl_btn_OK      = '<% =JS_SafeString(ResString("btnOK")) %>';
    var lbl_btn_Cancel  = '<% =JS_SafeString(ResString("btnCancel")) %>';
    var lbl_btn_show2me = '<% =JS_SafeString(ResString("btnShowResults2Me")) %>';
    var lbl_btn_hide2me = '<% =JS_SafeString(ResString("btnHideResults2Me")) %>';
//    var lbl_no_judgment = '<% =JS_SafeString(ResString("msgNoCommentWithoutJudgment")) %>';
    var lbl_no_infodoc  = '<% =JS_SafeString(ResString("lblEmptyInfodoc")) %>';
    var lbl_infodoc_wrt = '<% =JS_SafeString(ResString(CStr(Iif(App.ActiveProject.isRisk AndAlso isLikelihood, "tblWRTLikelihood", "tblWRT")))) %>';
    var lbl_infodoc_edit= '<% =JS_SafeString(ResString("lblEditInfodoc")) %>';
    var lbl_infodoc_view= '<% =JS_SafeString(ResString("lblViewInfodoc")) %>';
    var lbl_msg_onreload= '<% =JS_SafeString(ResString("msgReloadTTOnChanges")) %>';
    var lbl_msg_restrict= '<% =JS_SafeString(ResString("msgSynchronousRestrictedStep")) %>';
    var lbl_conf_continue = '<% =JS_SafeString(ResString("msgTTContinueConfirm")) %>';
    var lbl_erase_judg    = '<% =JS_SafeString(ResString("btnEvaluationResetValue")) %>';
    var lbl_invert_judg   = '<% =JS_SafeString(ResString("lblInvertValue")) %>';
    var lbl_eval_progr_s  = '<% =JS_SafeString(ResString("btnShowEvaluationProgress")) %>';
    var lbl_eval_progress = '<% =JS_SafeString(ResString("lblUsersEvaluationProgress")) %>';
    var lbl_di_err_range  = '<% =JS_SafeString(String.Format(ResString("errWrongNumberRange"), 0, 1)) %>';
    var lbl_sf_err_range  = '<% =JS_SafeString(ResString("errNotANumber")) %>';
    var lbl_gpw_err_num   = '<% =JS_SafeString(ResString("msgGPWWrongPart")) %>';
    var lbl_gpw_err_range = '<% =JS_SafeString(ResString("msgGPWWrongNumber")) %>';
    var lbl_ruc_err_range = lbl_sf_err_range;
    var lbl_refresh     = '<% =JS_SafeString(ResString("btnRefresh")) %>';
    var lbl_objectives  = '<% =JS_SafeString(ResString("lblObjective")) %>';
    var lbl_alternatives= '<% =JS_SafeString(ResString("lblAlternative")) %>';
    var lbl_sa_sel_obj  = '<% =JS_SafeString(ResString("lblSASelectNode")) %>';
    var lbl_knownlikelihood = '<% =JS_SafeString(ResString(CStr(IIf(App.isRiskEnabled AndAlso isImpact, "lblKnownImpactPW", "lblKnownLikelihoodPW")))) %>';
    var lbl_count       = '<% =JS_SafeString(ResString("tblTTJudgmentsCount")) %>';
    var lbl_distr_view  = '<% =JS_SafeString(ResString("lblTTDistrViewHeader")) %>';
    var lbl_ulist_undef = '<% =JS_SafeString(ResString("msgTTUsersWithNoJudgment")) %>';
    var lbl_ulist_judg  = '<% =JS_SafeString(ResString("msgTTUsersWithJudgment")) %>';
    var hints_pw_between= '<% =JS_SafeString(ResString("lblEvaluationPWHintBetween")) %>';
    var hints_pw_full   = ["<% =JS_SafeString(ResString("lblEvaluationPWHintEqual")) %>", "<% =JS_SafeString(ResString("lblEvaluationPWHintModerately")) %>", "<% =JS_SafeString(ResString("lblEvaluationPWHintStrongly")) %>", "<% =JS_SafeString(ResString("lblEvaluationPWHintVeryStrongly")) %>", "<% =JS_SafeString(ResString("lblEvaluationPWHintExtremely")) %>"];
    var hints_pw_short  = ["<% =JS_SafeString(ResString("lblEvaluationPWShortHintEqual")) %>", "<% =JS_SafeString(ResString("lblEvaluationPWShortHintModerately")) %>", "<% =JS_SafeString(ResString("lblEvaluationPWShortHintStrongly")) %>", "<% =JS_SafeString(ResString("lblEvaluationPWShortHintVeryStrongly")) %>", "<% =JS_SafeString(ResString("lblEvaluationPWShortHintExtremely")) %>"];
    var hint_distr_view = '<% =JS_SafeString(ResString("lblTTDistrMode")) %>';
    var hint_norm_view  = '<% =JS_SafeString(ResString("lblTTNormalMode")) %>';
    var norm_names      = ["<% =JS_SafeString(ResString(CStr(Iif(App.isRiskEnabled AndAlso isLikelihood, "lblTTResultsNorm0Risk", "lblTTResultsNorm0")))) %>", "<% =JS_SafeString(ResString(CStr(Iif(App.isRiskEnabled AndAlso isLikelihood, "lblTTResultsNorm1Risk", "lblTTResultsNorm1")))) %>", "<% =JS_SafeString(ResString(CStr(Iif(App.isRiskEnabled AndAlso isLikelihood, "lblTTResultsNorm2Risk", "lblTTResultsNorm2")))) %>"]; 

    var comment_editor_visible = 0;

    var zTree_settings = {
        view: {
            dblClickExpand: false,
            showLine: true,
            selectedMulti: true,
            showIcon: false,
            nameIsHTML: true
        },
        data: {
            simpleData: {
                enable:true,
                idKey: "id",
                pIdKey: "pId",
                rootPId: ""
            }
        },
        callback: {
            beforeClick: zTreeClick
        }
    };

// === code ===

    
    function DebugClose()
    {
        if (show_debug)
        {
            show_debug = 0;
            if (dbg_win!=null) dbg_win.close();
            dbg_win = null;
            dbg_ln = 0;
        }    
    }
    
    function DebugMsg(msg, is_err)
    {
        if (show_debug)
        {
            if (dbg_win==null) 
            {
                dbg_win = window.open("", "Debug", "menubar=no,maximize=yes,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=600,height=450", true);
                dbg_win.document.writeln ("<html><head><title>TT debug console</title><script language=javascript> function onExit() { if (((window.opener)) && ((window.opener.DebugClose))) window.opener.DebugClose(); } window.onbeforeunload = onExit; <\/script><\/head><body><div style='font-size:8pt; font-family:Verdana,Tahoma'><h3>Debug colsole<\/h3>");
            }    
            dbg_ln +=1;
            msg = new Date().toLocaleTimeString() + "." + new Date().getMilliseconds() + " " + htmlEscape(msg);
            if ((is_err) && is_err!="0") msg = "<span style='color:#990000'>" + msg + "</span>";
            if (dbg_ln&1) msg = "<div style='background:#fafafa; margin:2px 0px'>" + msg + "</div>";
            if ((dbg_win) && (dbg_win.document)) { dbg_win.document.writeln(msg); dbg_win.scrollBy(0,48); }
        }
    }
   
    function msg(text)
    {
        if (show_debug) { window.status = text; DebugMsg(text, 0); }
    }
    
    function LoadingStatus(status)
    {
        var img = document.images["imgLoading"];
        if ((img) && (sync_timeout>1000 || !status)) img.src = (status ? img_loading.src : img_blank);
        var q = $get("lblQueue");
        if ((q))
        {
            var l = "&nbsp;";
            for (var i=0; i<sync_queue.length; i++) l+=".";
            q.innerHTML = l;
        }
    }
    
    function GetTimeout()
    {
        return (sync_online ?  1 : 3) * sync_timeout;
    }
    
    function CheckPending(id)
    {
        if (sync_pending_id == id)
        {
            msg("Reset PendingID " + sync_pending_id +" due timeout of waiting callback with judgment");
            sync_pending_id = "";
        }
    }

    function SyncSend()
    {
        if (!sync_active) return false;
        var act = <% #If OPT_USE_AJAX_INSTEAD_CALLBACK Then %>_ajax_active<% #Else %>ASPxCallbackTeamTime.InCallback()<% #End If %>;
        if (sync_queue.length>0 && !act)
        {
            LoadingStatus(1);
            var params = sync_queue.shift();
            if (params[0]=="" && sync_hash!="") params[0] = "hash=" + sync_hash + "&pid=" + sync_prj_id;
            msg("Send request <% #If OPT_USE_AJAX_INSTEAD_CALLBACK Then %>{ajax}<% #Else %>{ASPxCallback}<% #End If %> [q: " + (sync_queue.length) + "] - \"" + params[0] + "\"");
            if((params[1]) && (params[1]!="")) SyncPending(params[1], 1);
            <% #If OPT_USE_AJAX_INSTEAD_CALLBACK Then %>_ajax_ok_code = "SyncReceived(data);";
            _ajax_error_code = "SyncError();";
            _ajaxSend(params[0] + "&r=" + Math.random());<% #Else %>ASPxCallbackTeamTime.PerformCallback(params[0] + "&r=" + Math.random());<% #End If %>;
        }
        else
        {
            if (sync_queue.length>0) msg("Unable to send request. Waiting response...");
        }
    }
    
    function SyncAddCommand(params, delay, pending_id)
    {
        if (params!="" && params.indexOf("save")>0 && !delay && pending_id!="")
        {
            sync_pending_id = pending_id + "-" + Math.round(10000+90000*Math.random());
            setTimeout('CheckPending("' + sync_pending_id + '");', 1000+(sync_timeout)*5);
            params = "pending=" + sync_pending_id + "&" + params;
            msg("Set waiting for PendingID " + sync_pending_id);
        }
        sync_queue.push([params, pending_id]);
        if (delay) setTimeout("SyncSend();", GetTimeout()); else SyncSend();
        if (sync_queue.length==8) { if (confirm("It seems as connection is lost or server is busy. Would you like to reload page and try again?")) document.location.reload(); }
    }
    
    function SyncUpdateLastChangeTime()
    {
        var  lbl = $get("lblLastUpdated");
        if ((lbl))
        {
            if (lbl.innerHTML=="")
            {
                var lbl2 = $get("lblTTLastJudgment");
                if ((lbl2)) lbl2.style.display = "block";
            }
            var period = Math.round(Math.abs(new Date() - sync_last_update)/1000);
            var sec = period%60;
            lbl.innerHTML = Math.floor(period/60)+":"+(sec<10 ? "0" : "")+sec;
            lbl.style.backgroundColor = (period>sync_judg_timeout ? "#61ff48" : "#f2ff28");
            if (period>sync_judg_timeout) lbl.style.color = "#333333";
        }
    }
    
    function SyncReceived(params)
    {
        sync_last_answer = new Date();
        sync_last_params = params;
        msg("Received response [q: " + (sync_queue.length) + "]: - \"" + params + "\"");
        if (sync_first_call)
        {
            SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 0);
            sync_first_call = 0;
        }
        if (sync_show_response)
        {
            var  l = $get("divDebug");
            if ((l) && sync_active) l.innerHTML = "<div style='position:fixed; background:#fffaf0; opacity:0.85; padding:2px 3px; margin-top:-25px; margin-left:-28px; border:1px solid #cccccc; max-width:800px; margin-right:100px; word-wrap: break-all; overflow-wrap: break-all; z-index:99; '>" + replaceString(",[", ", [", replaceString("<", "&lt;", replaceString('>', '&gt;', replaceString("','", "', '", params)))) + (typeof(CopyLink=="function") ? "<br><br><span class='gray xsmall'>[ <a href='' onclick='CopyLink(sync_last_params); return false;' class='actions dashed'>Copy to clipboard</a> ]</span>" : "") + "</div>";
        }   
        if ((sync_pending)) SyncPending(sync_pending, 0);
//        if (sync_queue.length==0 || !sync_active) ParseResponse(eval(params));
        ParseResponse(eval(params));
        sync_missed_call = 0;
        var lbl = $get("lblTTLastCall");
        if (lbl) lbl.innerHTML = sync_last_answer.toLocaleTimeString() + (sync_active ? "" : " [pause]");
        LoadingStatus(0);
        if (sync_active) { if (sync_queue.length>0) SyncSend(); else SyncAddCommand("", 1, ""); }
    }
    
    function SyncError()
    {
        sync_faults += 1;
        msg("Fault callback: " + sync_faults);
        var lbl = $get("lblAJAXFaults");
        if (lbl) lbl.innerHTML = sync_faults;
        LoadingStatus(0);
    }
    
    function SyncInit()
    {
        sync_last_answer = new Date();
        UpdateTime();
        UpdateOptions();
        SyncAddCommand("", 0, "");
        CheckCallBack();
        $("body").css("overflow", "hidden");
        <% if isTeamTime AndAlso isTeamTimeOwner Then %>if (($get("imgMode"))) setTimeout('$get("imgMode").title = ' + ((sync_distr_view) ? 'hint_norm_view' : 'hint_distr_view')+';', 350);<% End if %>
    }

    function CheckCallBack()
    {
        if (sync_active && (Math.abs(new Date() - sync_last_answer) > (3*GetTimeout())))
        {
            msg("Check callback: Missed, raise new…");
            SyncAddCommand("", 0, "");
            sync_missed_call = 1;
            SyncError();
        }
        else
        {
            msg("Check callback: OK");
        }
        setTimeout("CheckCallBack();", 3*GetTimeout());
    }
    
    function UpdateTime()
    {
        time_obj = setTimeout("UpdateTime();", 1000);
        var lbl = $get("lblCurTime");
        if (lbl)
        {
            var dt = new Date();
            lbl.innerHTML = dt.toLocaleTimeString();
            var lc = $get("lblTTLastCall");
            if (sync_active && (lc))
            {
                if (Math.abs(dt-sync_last_answer) >= (2*GetTimeout()) && !sync_missed_call)
                {
                    if ((lc.innerHTML.length<=lbl.innerHTML.length)) lc.innerHTML = "<span style='color:red'>" + lbl.innerHTML + "</span>";
                    if (Math.abs(dt-sync_last_answer) >= (10*GetTimeout()) && !sync_asked_reload) { sync_asked_reload=1; if (confirm("Lost connection. Would you like to reload page?")) document.location.reload(); }
                }
            }
            if ((sync_last_update)) SyncUpdateLastChangeTime();
        }    
    }

    function RefreshChange(val)
    {
        var c = sync_timeout/1000;
        var n = c+val;
        msg("Refresh change: " + n);
        if (n>=<% =TeamTimeMinimumRefreshTimeout %> && n<=45 && c!=n) SyncAddCommand("action=refresh&value="+n, 1, "");
        sync_timeout = n*1000;        
        UpdateRefresh();
    }
    
    function UpdateRefresh()
    {
        var n = sync_timeout/1000;
        sync_timeout = n * 1000;
        var v = $get("RefreshValue");
        if (v) v.innerHTML = n;
        var m = $get("RefreshMinus");
        if (m) m.innerHTML = (n><% =TeamTimeMinimumRefreshTimeout %> ? "-" : "");
        var p = $get("RefreshPlus");
        if (p) p.innerHTML = (n<30 ? "+" : "");
    }
    
    function ParseResponse(data)
    {
        msg("Parse response start");
        
        var force_redraw = sync_force_redraw;
        if (force_redraw) msg("Force to redraw from outside");
        
        if ((data))
        {
            for (var i=0; i < data.length; i++) 
            {
                var d = data[i];
                if ((d) && (d[0]))
                switch (d[0])
                {
                    case "time":
                        if ((d[1]))
                        {
                            var dp = Date.parse(d[1]);
                            if ((dp)) sync_last_serverdate = new Date(dp);
                        }
                        break;
                    case "pended":
                        if ((d[1]) && d[1].length>0)
                        {
                            for (p=0; p<d[1].length; p++)
                                if (d[1][p] == sync_pending_id)
                                {
                                    msg("Received callback with pended judgment. Reset PendingID " + sync_pending_id);
                                    sync_pending_id = "";
                                }
                        }
                        break;
                    case "hash":
                        if (sync_hash != d[1])
                        {
                            sync_hash_old = sync_hash;
                            sync_hash = d[1];
                            msg("Set hash to " + sync_hash);
                        }
                        break;
                    case "action":
                        if (d[1]=="refresh")
                        {
                            if ((sync_active)) SyncSwitchStatus();
                            jDialog(lbl_msg_onreload, false, "document.location.href='?resetproject=yes&<% =GetTempThemeURI(true) %>'"); 
                            window.focus();
                        }
                        if (d[1]=="stop") 
                        {
                            if ((sync_active)) {
                                SyncSwitchStatus();
                                jDialog('<% = JS_SafeString(ResString("msgSynchronousSessionClosed")) %>', false, "<% If isTeamTime AndAlso isTeamTimeOwner Then %>SyncStopPM();<% End if %>");
                            }
                            window.focus();
                        }
                        break;
                    case "warning":
                        SyncResetScreen(d[1]);
                        sync_last_data = "";
                        sync_last_prg = "";
                        sync_last_type = "";
                        sync_step_guid = "";
                        <% If IsConsensus AndAlso isTeamTimeOwner Then%>SyncStopUser();<% End if %>
                        break;<% if not isTeamTimeOwner Then  %>
                    case "active":
                        if (!sync_online && d[1]==1) SyncStop(document.location.href);
                        if (sync_online && d[1]==0)
                        {
                            if ((sync_active)) {
                                //var h = "<% If App.Options.isSingleModeEvaluation Then %>SyncStop('<% =PageURL(_PGID_LOGOUT) %>');<% Else %>SyncStopUser();<% End if %>";
                                var p1 = ((window.opener) && (typeof window.opener.onEndTeamTime)!="undefined");
                                var p2 = ((window.parent) && (typeof window.parent.onEndTeamTime)!="undefined");
                                var h = <% If isTeamTimeOwner Then %>"SyncStopUser();"<% Else %>((p1) || (p2)) ? "SyncStopUser();" : "SyncStop('<% =PageURL(_PGID_LOGOUT) %>');"<% End if %>;
                                //var h = "SyncStop('<% =PageURL(_PGID_LOGOUT) %>');";
                                SyncSwitchStatus();
                                jDialog('<% = JS_SafeString(ResString("msgSynchronousSessionClosed")) %>', false, h);
                            }
                            window.focus();
                        }
                        break;<% End If %>        
                    case "options":
                        <% if not isTeamTimeOwner Then  %>var r =1*d[1];
                        if (r>0 && r<=30 && (sync_timeout != r*1000)) { sync_timeout = r*1000; msg("Change refresh rate: " + r); }<% End if %>
                        var c =1*d[2];
                        if ((c==0 || c==1) && sync_showcomments!=c) { sync_showcomment = c; msg("Show comments: " + c); }
                        var inf =1*d[3];
                        if ((inf==-1 || inf==0 || inf==1) && sync_infodocsmode!=inf) { sync_infodocsmode = inf; msg("Infodocs mode: " + inf); force_redraw = 1; }
                        var h =1*d[4];
                        if ((h==0 || h==1) && sync_hidejudgments!=h) { sync_hidejudgments = h; if ((theForm.cbHideJudgments)) theForm.cbHideJudgments.checked = h; force_redraw=1; <% if isTeamTimeOwner Then  %>SyncUpdateShow2Me();<% End if %> msg("Hide judgments: " + h); }
                        var a =1*d[5];
                        if ((a==0 || a==1) && sync_anonymous!=a) { sync_anonymous = a; if ((theForm.cbAnonymous)) theForm.cbAnonymous.checked = a; setTimeout("SyncUpdateUserNames(); sync_eval_progress = -1; SyncShowEvalProgress(true);", 100); msg("Anonymous: " + a); }
                        var k =1*d[6];
                        if ((k==0 || k==1) && sync_usekeypads!=k) { sync_usekeypads=k; msg("Use keypads: " + k); if ((sync_active)) SyncSwitchStatus(); jDialog(lbl_msg_onreload, false, 'document.location.reload(); c=theForm.cbShowPWSide; if ((c)) c.disabled=" + (!k)+ "; c = $get("divPWSide"); if ((c)) c.disabed=' +(!k) + ';'); }
                        var s =1*d[7];
                        if (sync_users_sorting!=s) { sync_users_sorting=s; theForm.ttsorting.value = s; msg("Users sorting: " + s); force_redraw = 1; }
                        var v =1*d[8];
                        if (sync_show_variance!=v) { sync_show_variance=v; msg("Show variance: " + v); if ((theForm.cbShowVariance)) theForm.cbShowVariance.checked = ((v)); force_redraw = 1; }
                        var p =1*d[9];
                        if (sync_show_pwside!=p) { sync_show_pwside=p; msg("Show PW Side: " + p); if ((theForm.cbShowPWSide)) theForm.cbShowPWSide.checked = ((p)); force_redraw = 1; }
                        var o =1*d[10];
                        if (sync_showviewonly!=o) { sync_showviewonly=o; msg("Show view only users: " + o); if ((theForm.cbShowViewOnly)) theForm.cbShowViewOnly.checked = ((o)); force_redraw = 1; setTimeout("SyncUpdateUserNames(); SyncShowEvalProgress(true);", 100); }
                        var r =1*d[11];
                        if (sync_hideowner!=r) { sync_hideowner=r; msg("Hide session owner: " + r); if ((theForm.cbHideSessionOwner)) theForm.cbHideSessionOwner.checked = ((r)); force_redraw = 1; setTimeout("SyncUpdateUserNames(); SyncShowEvalProgress(true);", 100); }
                        var so =1*d[12];
                        if ((so>0) && (sync_owner) && (so!=<% =App.ActiveUser.UserID %>)) { msg("Looks like session owner has been changed. Reload page"); document.location.reload(); }
                        var ou =1*d[13];
                        if (sync_hideoffline!=ou) { sync_hideoffline=ou; msg("Hide offline users: " + ou); if ((theForm.cbHideOfflineUsers)) theForm.cbHideOfflineUsers.checked = ((ou)); force_redraw = 1; setTimeout("SyncUpdateUserNames(); SyncShowEvalProgress(true);", 100); }
                        var hc =1*d[14];
                        if (sync_hide_captions!=hc) { sync_hide_captions=hc; msg("Hide infodoc captions: " + hc); force_redraw = 1; return false; }
                        var pid =1*d[15];
                        if (pid>0 && sync_prj_id !=pid) { msg("Another project detected!"); force_redraw = 1; document.location.reload(); return false; }
                        break;
                case "progress":
                    var s = d[1]+0;
                    var t = d[2]+0;
                    if ((sync_online) && (s) && (s!=sync_step || t!=sync_steps_total))
                    {
                        <% if isTeamTimeOwner Then %>if (sync_steps_total>0 && t!=sync_steps_total) { if ((sync_active)) SyncSwitchStatus(); jDialog(lbl_msg_onreload, false, "document.location.reload();"); }<% End If %>
                        sync_step = s;
                        sync_steps_total = t;
                        sync_last_data = "";
                        sync_last_prg  = "";
                        sync_last_type = "";
                        sync_step_guid = "";
                        sync_last_update = null;
                        sync_last_comment = "";
                        sync_show2me = -1;
                        force_redraw = 1;
                        sync_distr_view = 0;
//                        sync_hash = "";
                        msg("New step detected");
                        SyncResetScreen('');
                        var prg = $get("divSteps");
                        if ((prg)) prg.innerHTML = "<b><% =JS_SafeString(ResString("lblEvaluationSteps")) %></b>:&nbsp;<b>" + s + "</b> of <b>" + d[2] +"</b>";
                    }<% If isTeamTimeOwner Then %>
                    if ((d[4]) && d[4]!="" && d[4]!=sync_eval_prg_user) {
                        sync_eval_prg_user = d[4];
                        if (typeof(SyncShowEvalProgress)=="function") { sync_eval_progress = -1; setTimeout('SyncShowEvalProgress(true);', 30); }
                    }
                    if ((d[5]) && d[5]!="" ) {
                        sync_eval_prg_name = d[5];
                    }
                    if ((d[6]) && d[6]!="" ) {
                        sync_next_unas = d[6]*1;
                    }<% End If %>
                    if ((d[3]))
                    {
                        sync_last_prg = d[3];
                        if (typeof(SyncShowEvalProgress)=="function") { sync_eval_progress = -1; setTimeout('SyncShowEvalProgress(true);', 50); }
                    }
                    break;
                case "data":
                    sync_last_data_full = d;
                    if (sync_last_type!="" && sync_last_type!=d[id_type])
                    {
                        msg("Detect change step type. Force to reload");
                        sync_last_data = "";
                        sync_last_prg = "";
                        sync_last_update = null;
                        sync_last_comment = "";
                        force_redraw = 1;
                        <% if isTeamTimeOwner Then %>if ((sync_active)) SyncSwitchStatus(); jDialog(lbl_msg_onreload, false, "document.location.reload();");<% End if %>
                    }
                    sync_last_type = d[id_type];
                    sync_step_guid = d[id_guid];
                    var sync_prev = null;
                    SyncRenderData(d, force_redraw);
                    break;
                }
            }
            if (force_redraw) { setTimeout('DoFlashWRTPath();', 1500); DoFlashWRT("wrt_name"); }
        }    
        sync_force_redraw = 0;

        msg("Parse response end");
        return false;
    }

    function SyncRenderData(d, force_redraw)
    {
        if (!(d)) return false;
        if (force_redraw) msg("Force redraw");
        var need_redraw_smart = 0;
        if (sync_user_last_type != sync_last_type)
        {
            sync_user_row_starts = -1;
            sync_user_rowheight = -1;
            sync_user_last_type = sync_last_type;
            if (sync_showusers_by_pages>0 && sync_userspage_size<1) need_redraw_smart = 1;
        }
        var u_data = null;
        switch(d[id_type])
        {
            case type_info:
                SyncResetScreen(d[id_data]);
                break;
            case type_msg:
                if ((force_redraw) || (d[id_data]) && d[id_data]!=sync_last_data )
                {
                    var do_refresh = (sync_last_data.length==0 || d[id_data][0]!=sync_last_data[0]);
                    sync_last_data = d[id_data];
                    if (force_redraw || do_refresh) {
                        msg("Show message");
                        SyncShowMessage(d[id_data][0]);
                    }
                    //sl_help = help_welcome;
                } 
                break;
            case type_pw:
                if ((force_redraw) || (d[id_data]) && d[id_data]!=sync_last_data)
                {
                    sync_prev = sync_last_data;
                    sync_last_data = d[id_data];
                    sync_is_gpw = (sync_last_data[4] != pw_verbal);

                    msg("Show pairwise");
                    u_data = GetRowByIdxKey(sync_last_data[1], id_email, sync_user);
                    if ((u_data) && u_data[id_keypad]==-2 && !sync_owner) 
                    {
                        SyncResetScreen("<div style='text-align:center'>" + lbl_msg_restrict + "</div>");
                    } 
                    else
                    {
                        if (sync_is_gpw)
                        {
                            slider_gpw_real_max = (sync_last_data[4] == pw_gpw_9 ? 9 : 99);
                            slider_gpw_real_over_max = (sync_last_data[4] == pw_gpw_9 ? 9 : (sync_last_data[4] == pw_gpw_99 ? 99 : 999));
                        }
                        if (force_redraw || SyncCheckRows(sync_last_data, sync_prev)) SyncShowPairwise(d[id_task], d[id_data]);
                    }
                    //sl_help = (sync_is_gpw ? help_gpw : help_pw);
                } 
                break;
            case type_rating:
                var has_undef = false;
                for (var j=0; j<d[id_data][1].length; j++)
                {
                    if (d[id_data][1][j][0] == -1) has_undef = true;
                }
                if (!has_undef) d[id_data][1].push([-1,lbl_undefined, value_undef]);
                if ((force_redraw) || (d[id_data]) && d[id_data]!=sync_last_data )
                {
                    sync_prev = sync_last_data;
                    sync_last_data = d[id_data];
                    msg("Show Rating");
                    u_data = GetRowByIdxKey(sync_last_data[2], id_email, sync_user);
                    if ((u_data) && u_data[id_keypad]==-2 && !sync_owner)
                    {
                        SyncResetScreen("<div style='text-align:center'>" + lbl_msg_restrict + "</div>");
                    } 
                    else
                    {
                        if (force_redraw || SyncCheckRows(sync_last_data, sync_prev)) SyncShowRating(d[id_task], d[id_data]);
                    }
                    //sl_help = help_rating;
                } 
                break;
            case type_direct:
                if ((force_redraw) || (d[id_data]) && d[id_data]!=sync_last_data )
                {
                    sync_prev = sync_last_data;
                    sync_last_data = d[id_data];
                    msg("Show DirectInput");
                    u_data = GetRowByIdxKey(sync_last_data[1], id_email, sync_user);
                    if ((u_data) && u_data[id_keypad]==-2 && !sync_owner)
                    {
                        SyncResetScreen("<div style='text-align:center'>" + lbl_msg_restrict + "</div>");
                    } 
                    else
                    {
                        if (force_redraw || SyncCheckRows(sync_last_data, sync_prev)) SyncShowDirect(d[id_task], d[id_data]);
                    }
                    //sl_help = help_direct;
                } 
                break;
            case type_step:
                if ((force_redraw) || (d[id_data]) && d[id_data]!=sync_last_data )
                {
                    sync_prev = sync_last_data;
                    sync_last_data = d[id_data];
                    msg("Show Step Function");
                    u_data = GetRowByIdxKey(sync_last_data[2], id_email, sync_user);
                    if ((u_data) && u_data[id_keypad]==-2 && !sync_owner)
                    {
                        SyncResetScreen("<div style='text-align:center'>" + lbl_msg_restrict + "</div>");
                    } 
                    else
                    {
                        if (force_redraw || SyncCheckRows(sync_last_data, sync_prev)) SyncShowStepFunction(d[id_task], d[id_data]);
                    }
                    //sl_help = help_step;
                } 
                break;
            case type_ruc:
                if ((force_redraw) || (d[id_data]) && d[id_data]!=sync_last_data )
                {
                    sync_prev = sync_last_data;
                    sync_last_data = d[id_data];
                    msg("Show Regular Utility Curve");
                    u_data = GetRowByIdxKey(sync_last_data[2], id_email, sync_user);
                    if ((u_data) && u_data[id_keypad]==-2 && !sync_owner)
                    {
                        SyncResetScreen("<div style='text-align:center'>" + lbl_msg_restrict + "</div>");
                    } 
                    else
                    {
                        if (force_redraw || SyncCheckRows(sync_last_data, sync_prev)) SyncShowRegularUtilityCurve(d[id_task], d[id_data]);
                    }
                    //sl_help = help_ruc;
                } 
                break;
            case type_l_res:
            case type_g_res:
                if ((force_redraw) || (d[id_data]) && d[id_data]!=sync_last_data )
                {
                    sync_last_data = d[id_data];
                    msg("Show results: " + d[id_type]);
                    u_data = GetRowByIdxKey(sync_last_data[1], id_email, sync_user);
                    if ((u_data) && u_data[4]==-2 && !sync_owner) 
                    {
                        SyncResetScreen("<div style='text-align:center'>" + lbl_msg_restrict + "</div>");
                    } 
                    else
                    {
                        SyncShowResults(d[id_task], d[id_data], d[id_type]==type_g_res);
                    }
                    //sl_help = (d[id_type]==type_l_res ? help_l_res : help_g_res);
                } 
                break;
            case type_dsa:
                if ((force_redraw) || (d[id_data]) && d[id_data]!=sync_last_data)
                {
                    sync_last_data = d[id_data];
                    msg("Show DSA");
                    SyncShowDSA(d[id_task], d[id_data]);
                    //sl_help = help_dsa;
                }
                break;
        }
        if (need_redraw_smart && sync_user_row_starts>0 && sync_user_rowheight>0) SyncRenderData(d, force_redraw); 
    }

    function SyncCheckDynamicPaging()
    {
        if (sync_showusers_by_pages && sync_userspage_size<0 && sync_user_row_starts>0 && sync_user_rowheight>0)
        {
            sync_user_last_type = "";
            SyncRenderData(sync_last_data_full, true);
        }
    }
    
    function SyncContent()
    {
        return $get("tdContent");
    }
    
    function SyncResetScreen(msg)
    {
        var s = SyncContent();
        if ((s)) s.innerHTML = (msg == "" ? "<h6 class='gray'>This evaluation step is not supported.</h6>" : msg = "<h5>" + msg + "</h5>");
        SyncSetContentAlign(1);
    }
    
    function SyncSetContentAlign(is_center)
    {
        var s = SyncContent();
        if ((s)) 
        { 
            s.setAttribute("valign", (is_center ? "middle" : "top")); 
            s.setAttribute("align", (is_center ? "center" : "left")); 
            s.style.verticalAlign = (is_center ? "middle" : "top"); 
            s.style.textAlign = (is_center ? "center" : "left");
            //if (is_center) s.style.margin = "auto auto";
        }
    }
    
    function SyncShowMessage(url)
    {
        var s = SyncContent();
        if ((s)) s.innerHTML = "<div style='height:100%;'><iframe src='' id='frmMessage' frameborder=0 style='border:0px; width:100%; height:100%;'></frame></div>";
        SyncLoadFrame("frmMessage", url);
    }

    function initFrameLoader(f, _img_preload, _img_size) {
        if ((f)) {
            var d = null;
            if (typeof f.contentWindow != "undefined") d = f.contentWindow.document;
            if (typeof f.document != "undefined") d = f.document;
            //if ((d) && (d.documentElement)) d.documentElement.innerHTML = "<html><body><div style='width:99%; height:99%; opacity:0.5; background: red url(" + (typeof _img_preload == "undefined" || _img_preload == "" ? _img_loading_global : _img_preload) + ") no-repeat center center; background-size: " + (typeof _img_size == "undefined" || !(_img_size) ? "60px" : _img_size) + ";'>&nbsp</div></body></html>";
            if ((d) && typeof (d.body) != "undefined" && (d.body)) d.body.innerHTML = "<div style='width:99%; height:99%; opacity:0.5; background:url(" + (typeof _img_preload == "undefined" || _img_preload == "" ? _img_loading_global : _img_preload) + ") no-repeat center center; background-size: " + (typeof _img_size == "undefined" || !(_img_size) ? "60px" : _img_size) + ";'>&nbsp</div>";
        }
    }

    function SyncLoadFrame(id, url)
    {
        var f = document.getElementById(id);
        if ((f)) {
            initFrameLoader(f);
            setTimeout(function () { if ((document.getElementById(id))) document.getElementById(id).src = url }, 30);
        }
    }
        
    function SyncAddTask(task, parent)
    {
        var t = document.createElement("div");
        t.setAttribute("id", "lblTask");
        t.setAttribute("class", "tt_task");
        t.innerHTML = task;
        parent.appendChild(t);
    }
    
    function SyncPending(id, show)
    {
        if ((sync_pending) && (sync_pending!=id)) SyncPending(sync_pending, 0);
        var p = $get(id_prefix + id + "_pending");
        if ((p))
        {
            sync_pending = (show ? id : null);
            p.style.display = (show ? "block" : "none");
            if (show && (sync_last_serverdate) && (sync_last_answer))
            {
                var s = (new Date().getTime() - sync_last_answer.getTime());
                sync_last_judgment_time = new Date(sync_last_serverdate.getTime() + s);
                var srv = new Date(2664000000 + sync_last_answer.getTime() - sync_last_serverdate.getTime());
//                msg("Set last judgment server time as " + sync_last_judgment_time.toLocaleString() + " (time shift with server is " + srv.getHours()+ ":" + srv.getMinutes() + ":" + srv.getSeconds() + ")");
            }
        }
    }
    
    function GetDataByID(uid, data)
    {
        if (data == null || data=="") data=sync_last_data;
        if (data!="")
        {
            var lst = null;
            switch (sync_last_type)
            {
                case type_pw:
                case type_direct:
                    lst = data[1]; break;
                case type_rating:
                case type_step:
                case type_ruc:
                    lst = data[2]; break;
            }
            
            if ((lst))
                for (var i=0; i<lst.length; i++)
                    if (lst[i][id_uid]==uid) return lst[i];
        }
        return null;
    }
    
    function GetDataByEmail(email)
    {
        if ((sync_last_data) && (sync_last_data!="") && (sync_last_type == type_pw || sync_last_type==type_rating) && sync_last_data[1] && sync_last_data[1].length>0)
        {
            var lst = sync_last_data[1];
            for (var i=0; i<lst.length; i++)
                if (lst[i][id_email].toLowerCase()==email.toLowerCase()) return lst[i];
        }
        return null;
    }
    
    function isUndefinedByID(value, type)
    {
         switch (type)
         {
            case type_pw:
                return (sync_is_gpw ? isValuesEqual(value_undef, value) : Math.abs(value)>steps);
            case type_rating:
            case type_direct:
                return (value < 0 || value*1 == value_undef);
            case type_step:
            case type_ruc:
                return (value<=value_undef);
         }
         return false;      
    }

    function isValuesEqual(value_a, value_b)
    {
        var k = (Math.abs(value_a)<1000000 ? 100000 : (Math.abs(value_a)>=Math.abs(value_undef/10) ? 0.1 : 1000));
        return ((value_a==value_undef && value_b<=value_undef) || (value_b==value_undef && value_a<=value_undef) || (Math.round(k*value_a)==Math.round(k*value_b)));
    }

    function isGPWChanged(slider_id, new_value)
    {
        var v = TwinSliderGetValue(slider_id);
        return ((v) && !isValuesEqual(new_value, TwinSliderPair2Value(v.value, v.adv)));
    }
    
    function SetPairwise(id, value, style, editable, visible)
    {
        value = Math.round(num_maxdecimals*value)/num_maxdecimals;
        if (sync_is_gpw)
        {
            if (isGPWChanged(id, value)) { 
                var val = TwinSliderValue2Pair(value);
                TwinSliderSetByValue(id, val.value, val.adv); 
                Sync_GPWChanged(id, val.value, val.adv); 
            }
        }
        else
        {
            var pw = $get(id_prefix + id + "_value");
            if ((pw)) pw.innerHTML = CreatePairwise(id, value, style, editable, visible);
        }
        var d = GetDataByID(id);
        if ((d)) d[id_value] = value;
    }
    
    function SavePairwise(id, value, style, editable)
    {
        if (!SyncCheckActive()) return false;
        SetPairwise(id, value, style, editable, 1);
        var d = GetDataByID(id);
        SyncAddCommand("hash=" + sync_hash + "&<% =_PARAM_ACTION %>=save&id=" + id + "&step=" + sync_step + "&guid=" + sync_step_guid + "&value=" + value + "&comment=" + ((d) ? escape(d[id_comment]) : ""), 0, id);
        return true;
    }
    
    function SetRating(id)
    {
        var r = $get(id_prefix + id + "_ratings");
        var v = $get(id_prefix + id + "_value");
        var b = $get(id_prefix + id + "_bar");
        var n = $get(id_prefix + id + "_rating_name");
        if (!(r)) r = eval("theForm." + id_prefix + id + "_ratings");
        msg("Set rating for " + id);

        var rat = [];
        if ((sync_last_data)) rat = sync_last_data[1];
        var val = -1;

        var d = GetDataByID(id);
//        var v = ((d) ? d[id_value]*1 : "");
        var r_id = ((r) ? (r.value=="" ? -1 : r.value) : "");
        if ((r_id == "" || r_id<0)&& (d)) r_id = d[id_value]*1;
        for (var i=0; i<rat.length; i++) {
            if (rat[i][0]==r_id || (rat[i][0]<0 && r_id<0)) {
                val = rat[i][2]; 
                if((n)) n.innerHTML=rat[i][1]; 
                if ((d)) d[id_value] = r_id;
                break; 
            }
        }

        if ((v) && (b)) 
        {   
            var undef = (val<0 || val == value_undef);
//            v.innerHTML = (undef || sync_hidejudgments ? "&nbsp;" : ShowDecNum(val, sync_last_data[5]+2));
            v.innerHTML = (undef || sync_hidejudgments ? "&nbsp;" : val);
            b.innerHTML = CreateBar((undef || !sync_hidejudgments ? val : 1), (sync_hidejudgments ? fill_disabled : fill_blue));
        }
    }

    function onRatingClick(value_id, val) {
        var r = eval("theForm." + id_prefix + value_id  +"_ratings");
//        $("#" + id_prefix + value_id  +"_ratings [value='" + val + "']").attr("selected", "selected");
//        var r = $get(id_prefix + value_id  +"_ratings");
        if ((r)) { r.value = val; SaveRating(value_id); }
    }

    function SyncRefreshRating(id, val) {
        var d = GetDataByID(id);
        if ((d) && isActiveUser(d[id_email])) {
            var r =  $get("int_" + (val < 0 ? "undef" : val));
            if ((r)) r.checked = true;
        }
    }
    
    function SaveRating(id)
    {
        if (!SyncCheckActive()) return false;
        var r = $get(id_prefix + id + "_ratings");
        if (!(r)) r = eval("theForm." + id_prefix + id + "_ratings");
        if ((r))
        {
            SetRating(id);
            var d = GetDataByID(id);
            SyncAddCommand("hash=" + sync_hash + "&<% =_PARAM_ACTION %>=save&id=" + id + "&step=" + sync_step + "&guid=" + sync_step_guid + "&value=" + r.value + "&comment=" + ((d) ? escape(d[id_comment]) : ""), 0, id);
            SyncRefreshRating(id, r.value);
            return true;
        }
        return false;
    }
    
    function ShowDecNum(val, decimals)
    {
        if (decimals === 0)
        {
            s = Math.round(val);
        }
        else
        {
            if (decimals==="undefined" || isNaN(decimals)) decimals = sync_decimals;
            var dec_multi = Math.pow(10, decimals);
            var s = (Math.round(dec_multi * val)/dec_multi) + "";
            var idx = s.indexOf(".");
            if (idx<0) { s+="."; idx = s.length-1; }
            idx+=1;
            if (idx+decimals<s.length) s = s.substr(0,idx+decimals); else while (idx+decimals>s.length) s+="0";
        }
        return s;
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
    
    function UpdateCommentIcon(id, is_empty)
    {
        var im = $get(id_prefix+id+"_comment_img");
        if ((im)) im.src = (is_empty ? img_comment_e : img_comment).src;
    }
    
    function SaveComment(id)
    {
        msg("Call for save comment");
        var c = $get(id_prefix + id + "_comment");
        var e = $get(id_prefix + id + "_comment_edit");
        if (!(e)) e = eval("theForm." + id_prefix + id + "_comment_edit");
        var d = GetDataByID(id);
        if ((c) && (e) && (d))
        {
            d[id_comment] = e.value;
            UpdateCommentIcon(id, d[id_comment]=="");
            SwitchComment(id);
            sync_last_comment = "";
            SyncAddCommand("hash=" + sync_hash + "&<% =_PARAM_ACTION %>=save&id=" + id + "&step=" + sync_step + "&guid=" + sync_step_guid + "&value=" + d[id_value] + "&comment=" + encodeURIComponent(replaceString("&","%26",d[id_comment])), 0, id);
        }
    }
    
    function SwitchComment(id)
    {
        var c = $get(id_prefix + id + "_comment");
        var d = GetDataByID(id);
        if ((c) && (d))
        {
            msg("Call for switch comment #" + id);
            if (c.style.display == "none" && SyncCheckActive())
            {
//                var can_edit = (sync_owner || isActiveUser(d[id_email])) && !isUndefinedByID(d[id_value], sync_last_type);
                var can_edit = (sync_owner || isActiveUser(d[id_email]));
                if (sync_last_comment!="") SwitchComment(sync_last_comment);
                var comment = d[id_comment];
                c.style.display = "block";
//                c.innerHTML = lbl_comment_text + "<textarea rows=7 style='width:99%' class='input' name='" + id_prefix + id + "_comment_edit'>" + comment + "</textarea><div style='text-align:right; margin-top:3px'>" + (d[id_comment]=="" && (sync_owner || isActiveUser(d[id_email])) ? "<div class='text small gray' style='text-align:left'>[!]&nbsp;" + lbl_no_judgment + "</div>" : "") + (can_edit ? "<input type=button name='btn_OK' class='button button_small' value='" + lbl_btn_OK + "' " + (can_edit ? "onclick='SaveComment(\"" + id + "\"); return false;'" : " disabled=1") + ">&nbsp;" : "") + "<input type=button name='btn_Cancel' class='button button_small' value='" + lbl_btn_Cancel + "' onclick='SwitchComment(\"" + id + "\"); return false;'></div>";
                c.innerHTML = lbl_comment_text + "<textarea " + (can_edit? "" : "readonly ") + "rows=7 style='width:99%' class='input' name='" + id_prefix + id + "_comment_edit'>" + comment + "</textarea><div style='text-align:right; margin-top:3px'>" + (can_edit ? "<input type=button name='btn_OK' class='button button_small' value='" + lbl_btn_OK + "' " + (can_edit ? "onclick='SaveComment(\"" + id + "\"); return false;'" : " disabled=1") + ">&nbsp;" : "") + "<input type=button name='btn_Cancel' class='button button_small' value='" + lbl_btn_Cancel + "' onclick='SwitchComment(\"" + id + "\"); return false;'></div>";
                sync_last_comment = id + "";
                if (can_edit) setTimeout('var f = $get("' + id_prefix + id + '_comment_edit"); if (!(f)) f = eval("theForm.' + id_prefix + id + '_comment_edit"); if ((f)) f.focus();', 100);
                comment_editor_visible = 1;
            }
            else
            {
                c.style.display = "none";
                c.innerHTML = "";
                sync_last_comment = "";
                comment_editor_visible = 0;
            }
        }
    }
    
    function Sync_GPWChanged(slider_id, val, adv)
    {
        var value = TwinSliderPair2Value(val, adv);
        var slider = TwinSliderGetValue(slider_id);
        var value_slider = TwinSliderPair2Value(slider.value, slider.adv);
        var d = GetDataByID(slider_id);
        if (!isValuesEqual(value, value_slider) || ((d) && !isValuesEqual(value, d[id_value]))) setTimeout("SaveGPWPairwise(" + slider_id + ", '" + value + "', '" + value_slider + "');", 500);
    }

    function SetGPWArrow(img, diff, hover) {
        if ((img)) img.src = (hover ? (diff < 0 ? img_decrease_ : img_increase_) : (diff < 0 ? img_decrease : img_increase)).src;
    }

    function CreateBar(value, fillStyle)
    {
        if (value<0) value=0;
        if (value>1) value=1;
        if (style==fill_disabled) value=1;
        var max_width = bar_width;
        var fillWidth  = Math.round(value * max_width);
        if (fillWidth<1) fillWidth = 1; else if (fillWidth>max_width) fillWidth = max_width;
        var style = (value < 0.0001 ? "" : fillStyle);
        var sFill = "<div class='" + style + "' style='width:" + fillWidth + "px; height: " + (bar_height) + "px; font-size:2px;'><img src='" + img_blank + "' width='" + fillWidth + "' height='" + (bar_height) + "' border=0 title_='" + value + "'></div>";
        var sBar = "<div class='progress' style='height:" + (bar_height) + "px;width:" + (bar_width) + "px;padding:" + bar_padding + "px;margin:1px;'>" + sFill + "</div>";
        return sBar;
    }

    function Sync_GPWSetFocus(gpw_id, is_a)
    {
        var fld = (is_a ? TwinSliderGetEditA(gpw_id) : TwinSliderGetEditB(gpw_id));
        if ((fld)) fld.focus();
    }

    function SaveGPWPairwise(id, value, old_value)
    {
        if (!SyncCheckActive()) return false;
        var d = GetDataByID(id);
        var s = document.getElementById(slider_prefix + id);
        if ((d) && (s) && (!s.readonly) && !isGPWChanged(id, old_value) && (!isValuesEqual(value, d[id_value]) || isGPWChanged(id, value)))
        {
            var fld_a = TwinSliderGetEditA(id);
            var fld_b = TwinSliderGetEditB(id);
            if ((fld_a) && (fld_b) && (fld_a.value!="" || fld_b.value!=""))
            {
                var a = TwinSliderCheckEditField(fld_a);
                if (fld_a.value!="" && !a.valid) { jDialog(lbl_gpw_err_num, true, "setTimeout('Sync_GPWSetFocus(\'" + id + "\', 1);', 350);"); return false; }
                var b = TwinSliderCheckEditField(fld_b);
                if (fld_b.value!="" && !b.valid) { jDialog(lbl_gpw_err_num, true, "setTimeout('Sync_GPWSetFocus(\'" + id + "\', 0);', 350);"); return false; }
                <% if TeamTimeFuncs.GPW_Mode_Strict Then %>
                if (slider_gpw_real_over_max<999 && a.valid && b.valid)
                {
                    var c = (a.value>b.value ? a.value/(b.value+0.000000001) : b.value/(a.value+0.00000001));
                    if (c>slider_gpw_real_over_max) { jDialog(replaceString("{0}", slider_gpw_real_over_max, lbl_gpw_err_range) , true, "setTimeout('Sync_GPWSetFocus(\'" + id + "\', " + (b.value==1 ? 1 : 0) + ");', 350);"); return false; }
                }<% End if %>
            }

            msg("Detect end of change GPW value: " + value);
            SetPairwise(id, value, "", 1, 1);
            SyncAddCommand("hash=" + sync_hash + "&<% =_PARAM_ACTION %>=save&id=" + id + "&step=" + sync_step + "&guid=" + sync_step_guid + "&value=" + value + "&comment=" + ((d) ? escape(d[id_comment]) : ""), 0, id);
        }
        return true;
    }

    function SliderDirectChange(slider_id, dir, first) {
        var fld = eval("theForm." + id_prefix + slider_id + "_value");
        if ((fld))
        {
            var val = 1 * fld.value;
            val = (fld.value == "" ? 0.01 : val + (dir>0 ? 1 : -1)*0.01);
            if (val > 0.9999) { val = 1; slider_is_pressed = 0; }
            if (val < 0.0001) { val = 0; slider_is_pressed = 0; }
            fld.value = ShowDecNum(val);
            SliderDirectEditChange(slider_id);
            if (slider_is_pressed) setTimeout("if (slider_is_pressed) SliderDirectChange(" + slider_id + "," + dir + ",0);", (first ? 350 : 150));
        }
        return false;
    }

    function CheckAndWarnDirectEdit(id)
    {
        var fld = eval("theForm." + id_prefix + id + "_value");
        if ((fld) && !(CheckDirectEdit(fld))) { jDialog(lbl_di_err_range, true, "setTimeout('theForm." + id_prefix + id + "_value.focus();', 350);"); }
    }

    function SliderDirectEditChange(slider_id, new_val) {
        var fld = eval("theForm." + id_prefix + slider_id + "_value");
        var slider = document.getElementById(slider_prefix + slider_id);
        if ((fld) && (slider))
        {
            var valid = 1;
            var val = value_undef;
            if (typeof (new_val)!="undefined")
            {
                var v = (new_val<0 ? "" : (slider.masked) ? "x.xx" : ShowDecNum(new_val));
                fld.value = v;
                var fld_s = $get(id_prefix + slider_id + "_static");
                if ((fld_s)) fld_s.innerHTML = (v == "" ? "&nbsp;" : v);
                val = (new_val<0 ? value_undef : new_val);
                valid = 1;
            }
            else
            {
                msg("Edit direct value");
                valid = CheckDirectEdit(fld);
                val = fld.value * 1;
            }
            fld.style.backgroundColor = (valid || fld.value=="" ? "" : "#f0cccc");
            if ((valid) && (val >= 0 && val <= 1)) val = Math.round(val*slider_direct_max); else val = value_undef;
            slider_is_manual = 1;
            slider.val = val;
            drawSliderByVal(slider);
            onChangeSliderDirect(slider.val, slider_id);
            slider_is_manual = 0;
            if (!slider.readonly && slider.val!=slider.val_old && typeof new_val=="undefined") { setTimeout("SaveDirect(" + slider_id + "," + val + ");", 500); slider.val_old = slider.val; }
        }
    }

    function ResetSliderDirect(slider_id) {
        var slider = document.getElementById(slider_prefix + slider_id);
        msg("Reset direct");
        if ((slider)) { slider.val = value_undef; drawSliderByVal(slider); onChangeSliderDirect(slider.val, slider_id); }
    }
    
    function CheckDirectEdit(fld) {
        var val = fld.value;
        var valid = false;
        if (val != "") {
            val = val.replace(",", ".");
            if (val[0] == ".") val = "0" + val;
            if (val == "0.") val = "0";
            if ((val * 1) == val) {
                val = val * 1;
                valid = (val >=0 && val <= 1);
            }
        }
        return valid;
    }
    
    function onChangeSliderDirect(val, slider_id) {
        var s = document.getElementById(slider_prefix + slider_id);
        if ((s)) {
          var e = eval("theForm." + id_prefix + slider_id + "_value");
          if ((e) && !slider_is_manual) {
            msg("Slider Direct changed");
            if (s.val<s.min) 
            {
                e.value = "";
            }
            else {
                var value = s.val / slider_direct_max;
                e.value = ((s.masked) ? "x.xx" : ShowDecNum(value));
            }
            if (!s.readonly && s.val!=s.val_old) { setTimeout("SaveDirect(" + slider_id + "," + (e.value=="" ? value_undef : s.val)+ ");", 500); s.val_old = s.val; }
          }
       }
    }

    function SaveDirect(id, value)
    {
        if (!SyncCheckActive()) return false;
        var slider = document.getElementById(slider_prefix + id);
        var d = GetDataByID(id);
        if ((d) && (slider) && slider.val == value)
        {
            msg("Detect end of change direct value: " + value);
            d[id_value] = value;
            SyncAddCommand("hash=" + sync_hash + "&<% =_PARAM_ACTION %>=save&id=" + id + "&step=" + sync_step + "&guid=" + sync_step_guid + "&value=" + (value<0 ? value_undef : Math.round(value)/slider_direct_max) + "&comment=" + ((d) ? escape(d[id_comment]) : ""), 0, id);
            return true;
        }
        return false;
    }

    function CheckStepFunctionEdit(fld) {
        var val = fld.value;
        var valid = 0;
        if (val != "") {
            val = val.replace(",", ".");
            if (val[0] == ".") val = "0" + val;
            if (val == "0.") val = "0";
            if ((val * 1) == val) {
                val = val * 1;
                valid = 1;
            }
        }
        return valid;
    }

    function StepFunctionUpdateValue(id, val)
    {
        var d = GetDataByID(id);
        var undef = isUndefinedByID(val, type_step) || (id==id_groupres && val<0);
        var prty_value = value_undef;
        if (!undef) prty_value = GetStepValue(val);
        if (!undef && id == id_groupres) prty_value = val*1;
        var masked = sync_hidejudgments && sync_show2me<0 && (!(d) || !isActiveUser(d[id_email]));
        var v = (undef || val=="" ? "&nbsp;" : masked ? "x.xx" : ShowDecNum(prty_value));
        var prty =  $get(id_prefix + id + "_value");
        if ((prty)) prty.innerHTML = v;
        var fld_s = $get(id_prefix + id + "_static");
        if ((fld_s)) fld_s.innerHTML = (undef || val=="" ? "&nbsp;" : masked ? "x.xx" : ShowDecNum(val));
        var bar =  $get(id_prefix + id + "_bar");
        if ((bar)) bar.innerHTML =  CreateBar(undef ? -1 : (masked ? 1 : prty_value), (masked ? fill_disabled : (id == id_groupres ? fill_red : fill_blue)));
    }

    function CheckAndWarnStepFunctionEdit(id)
    {
        var fld = eval("theForm." + id_prefix + id + "_edit");
        if ((fld) && !(CheckStepFunctionEdit(fld))) { jDialog(lbl_sf_err_range, true, "setTimeout('theForm." + id_prefix + id + "_edit.focus();', 350);"); }
    }

    function StepFunctionEditChange(id, new_val) {
        var fld = eval("theForm." + id_prefix + id + "_edit");
        var valid = 1;
        var val = value_undef;
        var d = GetDataByID(id);
        if (typeof (new_val)!="undefined")
        {
            var act = ((d) && isActiveUser(d[id_email]));
            var v = (new_val<0 ? "" : sync_hidejudgments && sync_show2me<0 && !(act) ? "x.xx" : ShowDecNum(new_val));
            if ((fld)) fld.value = v;
            var fld_s = $get(id_prefix + id + "_static");
            if ((fld_s)) fld_s.innerHTML = (v == "" ? "&nbsp;" : v);
            val = (new_val<0 ? value_undef : new_val);
            valid = 1;
        }
        else
        {
            msg("Edit step function value");
            valid = CheckStepFunctionEdit(fld);
            val = fld.value * 1;
        }

        if (!(valid)) val = value_undef;
        StepFunctionUpdateValue(id, val);

        if ((fld))
        {
            if (isActiveUser(d[id_email])) { fld.style.backgroundColor = (valid || fld.value=="" ? "" : "#f0cccc"); }
            if (typeof new_val=="undefined" && (d) && d[id_value]!=val) setTimeout("SaveStepFunction(" + id + ",'" + fld.value + "');", 500);
        }
    }

    function SaveStepFunction(id, value)
    {
        if (!SyncCheckActive()) return false;
        var fld = eval("theForm." + id_prefix + id + "_edit");
        var d = GetDataByID(id);
        if ((d) && (fld) && (fld.value==value))
        {
            valid = CheckStepFunctionEdit(fld);
            val = (!valid || value=="" ? value_undef: fld.value * 1);
            if (d[id_value]!=val)
            {
                msg("Detect end of change step function value: " + value);
                d[id_value] = val;
                SyncAddCommand("hash=" + sync_hash + "&<% =_PARAM_ACTION %>=save&id=" + id + "&step=" + sync_step + "&guid=" + sync_step_guid + "&value=" + val + "&comment=" + ((d) ? escape(d[id_comment]) : ""), 0, id);
                return true;
            }
        }
        return false;
    }


    function CheckAndWarnRUCEdit(id)
    {
        var fld = eval("theForm." + id_prefix + id + "_edit");
        if ((fld) && !(CheckRUCEdit(fld))) { jDialog(lbl_ruc_err_range, true, "setTimeout('theForm." + id_prefix + id + "_edit.focus();', 350);"); }
    }

    function CheckRUCEdit(fld) {
        var val = fld.value;
        var valid = 0;
        if (val != "" && val!="-") {
            val = val.replace(",", ".");
            if (val[0] == ".") val = "0" + val;
            if (val == "0.") val = "0";
            if ((val * 1) == val) {
                val = val * 1;
                valid = 1;
            }
        }
        return valid;
    }

    function RUCUpdateValue(id, val)
    {
        var d = GetDataByID(id);
        var undef = isUndefinedByID(val, type_ruc) || (id==id_groupres && val<0);
        var prty_value = value_undef;
        if (!undef) prty_value = GetRUCValue(val);
        if (!undef && id == id_groupres) prty_value = val*1;
        var masked = sync_hidejudgments && sync_show2me<0 && (!(d) || !isActiveUser(d[id_email]));
        var v = (undef || val==="" ? "&nbsp;" : masked ? "x.xx" : ShowDecNum(prty_value));
        var prty =  $get(id_prefix + id + "_value");
        if ((prty)) prty.innerHTML = v;
        var fld_s = $get(id_prefix + id + "_static");
        if ((fld_s)) fld_s.innerHTML = (undef || val=="" ? "&nbsp;" : masked ? "x.xx" : ShowDecNum(val));
        var bar =  $get(id_prefix + id + "_bar");
        if ((bar)) bar.innerHTML =  CreateBar(undef ? -1 : (masked ? 1 : prty_value), (masked ? fill_disabled : (id == id_groupres ? fill_red : fill_blue)));
    }

    function RUCEditChange(id, new_val) {
        var fld = eval("theForm." + id_prefix + id + "_edit");
        var d = GetDataByID(id);
//        if ((new_val!="undefined"))
//        {
            var valid = 1;
            var val = value_undef;
            if (typeof new_val != "undefined")
            {
                var v = (isUndefinedByID(new_val, type_ruc) ? "" : sync_hidejudgments && sync_show2me && (!(d) || !isActiveUser(d[id_email])) ? "x.xx" : ShowDecNum(new_val));
                if ((fld)) fld.value = v;
                var fld_s = $get(id_prefix + id + "_static");
                if ((fld_s)) fld_s.innerHTML = (v == "" ? "&nbsp;" : v);
                val = (new_val<0 ? value_undef : new_val);
                valid = 1;
            }
            else
            {
                msg("Edit RUC value");
                valid = CheckRUCEdit(fld);
                val = fld.value * 1;
            }

            if (!(valid)) val = value_undef;
            RUCUpdateValue(id, val);
            if ((fld))
            {
                if (isActiveUser(d[id_email])) { fld.style.backgroundColor = (valid || fld.value=="" ? "" : "#f0cccc"); }
                if (typeof new_val=="undefined" && (d) && d[id_value]!=val) setTimeout("SaveRUC(" + id + ",'" + fld.value + "');", 500);
            }
//        }
    }

    function SaveRUC(id, value)
    {
        if (!SyncCheckActive()) return false;
        var fld = eval("theForm." + id_prefix + id + "_edit");
        var d = GetDataByID(id);
        if ((d) && (fld) && (fld.value==value))
        {
            valid = CheckRUCEdit(fld);
            val = (!valid || value=="" ? value_undef: fld.value * 1);
            if (d[id_value]!=val)
            {
                msg("Detect end of change RUC value: " + value);
                d[id_value] = val;
                SyncAddCommand("hash=" + sync_hash + "&<% =_PARAM_ACTION %>=save&id=" + id + "&step=" + sync_step + "&guid=" + sync_step_guid + "&value=" + val + "&comment=" + ((d) ? escape(d[id_comment]) : ""), 0, id);
                return true;
            }
        }
        return false;
    }

  
    function CreatePairwise(id, value, style, can_edit, visible)
    {
        var pw = "";
        for (var i=-steps; i<=steps; i++)
        {
            var w = cell_width;
            var cell_class = (i==0 ? (Math.abs(value)<=steps ? "equal" : "undef") : "empty");
            var fill = 0;
            var hint = hints_pw_full[Math.abs(i)>>1];
            if ((Math.abs(i)&1)) hint = replaceString("{0}", hint, replaceString("{1}", hints_pw_full[1+Math.abs(i)>>1], hints_pw_between));
            var j = Math.floor(value) + (i<0 ? 0 : 1);
            if (visible==1 && j==i && Math.abs(value%1)>0.001 && i!=0) { w=Math.round(Math.abs(cell_width * (value%1))); fill = 1; }
            if (value<0 && i<0 && Math.abs(value)<=steps) if (value<=i || fill) cell_class = "left";
            if (value>0 && i>0 && Math.abs(value)<=steps) if (value>=i || fill) cell_class = "right";
            if (visible!=1 &&  Math.abs(value)<=steps)
            {
                if (visible==0 || (value<0 && i<=0) || (value>0 && i>=0) || (value==0 && i==0)) cell_class = "disabled";
            }
            var cell = "<img src='"+ img_blank + "' width=" + w + " height=" + cell_height + " border=0 alt=\"" + hint + "\" title=\"" + hint + "\">";
            if (can_edit && visible==1) cell = "<a href='' onclick='SavePairwise(\"" + id + "\"," + i +",\"" + style + "\"," + can_edit + ");return false;'>" + cell + "</a>";
            cell = "<div class='" + style + "_" + cell_class + "' style='width:" + w + "px'>" + cell + "</div>";
            pw += "<td style='padding:0px;margin:0px;border-width:0px;width:" + cell_width + "px' align='" + (i<0 ? "right" : "left")+ "'>" + cell + "</td>";
        }
        var undef = (value < -steps);
        var is_cons = (style==fill_cons);
        var r = "<img src='" + (is_cons ? img_blank : (undef ? img_delete_ : img_delete).src) +"' width=10 height=10 style=\'margin-left:2px\' border=0 title='" + lbl_erase_judg + "'>";
        //var can_edit_ = (!is_cons && <% = iif(isTeamTimeOwner, "1", "can_edit") %>);
        if (!undef && can_edit) { r = "<a href='' onclick='SavePairwise(\"" + id + "\"," + value_undef + ",\"" + style + "\"," + can_edit + "); return false;'>" + r + "</a>"; }
        pw = "<table border=0 cellspacing=1 cellpadding=0 class='" + style + "_tbl' align='center'><tr style='font-size:" + (cell_height-2) + "px'>" + pw + "</tr></table>";
        if (visible==1 && can_edit) pw = "<table border=0 cellspacing=0 cellpadding=0 align='center'><tr class='text small'><td>"+pw+"</td><td width=14>"+r+"</td></table>";
        pw = "<p align='center' style='margin:0px; text-align:center;padding-left:1ex;" + (visible==1 && can_edit ? "" : "padding-right:18px") + "'>" + pw + "</p>";
        return pw;         
    }
    
    function CreatePending(id)
    {
//        return "<div id='" + id_prefix + id + "_pending' class='tt_pending' style='display:block'>" + lbl_pending + "</div>";
        return "<div id='" + id_prefix + id + "_pending' class='tt_pending' style='display:none;'>&nbsp;</div>";
    }
    
    function SyncCreatePopupInfodoc(node, pid, type_def)
    {
        var s = "";
        var is_wrt = (pid >= 0);
        var id = node[0];
        var guid = node[1];
        var name = node[2];
        var is_alt = node[4];
        var has_infodoc = node[(is_wrt ? 5 : 3)];
        var type = (is_wrt ? 3 : (is_alt ? 2 : 1));
        var img_id = (is_wrt ? "_wrt" : "") + (is_alt ? "_a" : "_o") + id;
        var _guid = (is_wrt ? "_" : "") + id;
        if (typeof type_def!="undefined") { type = type_def; img_id = id; }
        
        if (has_infodoc || sync_owner)
        {
            s = "<img src='" + (has_infodoc ? (is_wrt ? img_WRTinfodoc1 : img_infodoc1) : (is_wrt ? img_WRTinfodoc2 : img_infodoc2)).src + "' title='" + <% =iif(isTeamTimeOwner, "lbl_infodoc_edit", "lbl_infodoc_view") %>+"' border=0 id='" + id_prefix + img_id + "_infodoc' onmouseover='ShowInfodocTooltip(this.id,\"" + _guid + "\",\"" + type + "\",\"" + pid + "\", \"" + guid + "\", \"\"); return false;' class='aslink' style='margin:0px 4px' onclick='OpenInfodoc(this,\"" + _guid + "\",\"" + type + "\",\"" + pid + "\", \"" + guid + "\", \"\"); return false;'>";
        }    
        return s;
    }

    function SyncInitFramedInfodocs()
    {
        setTimeout('$("div.resizable").resizable({ maxHeight:850, maxWidth:1350, minHeight:32, minWidth:32, helper:"ui-resizable-helper", autoHide:true, animate:false, ghost:false, containment:"document", start: function (event, ui) { SyncSetFrameContentVis(ui, 0); }, stop:function (event,ui) { SyncSetFrameContentVis(ui, 1); SyncSetSizeByUI(ui); } });', 100);
    }
    
    function SyncCreateFramedInfodoc(node, pid, parent_name, collapse_lst, cookie_name, def_hide, also_lst, type_def)
    {
        var is_wrt = (pid >= 0);
        var id = node[0];
        var guid = node[1];
        var node_name = node[2];
        var is_alt = node[4];
        var has_infodoc = node[(is_wrt ? 5 : 3)];
        var type = (is_wrt ? 3 : (is_alt ? 2 : 1));
        var _guid = (is_wrt ? "_" : (is_alt ? "a" : "")) + id;
        if (typeof type_def != "undefined") { type = type_def; _guid = id; }
        
        var c = document.cookie;
        var do_hide = def_hide;
        if (c.indexOf(id_prefix + "frm_" + cookie_name + "=")>=0) do_hide = (c.indexOf(id_prefix + "frm_" + cookie_name + "=1")>=0);
        
        var left = (sync_last_type == type_pw && parent_name == "tdParentNode");
        
        var d = $get(parent_name);
        if ((d) && (has_infodoc || sync_owner))
        {
            var src = '<% =PageURL(_PGID_EVALUATE_INFODOC) %>?project=<% =App.ProjectID %>&type=' + type + '&id=' + id + '&pid=' + pid + '&guid=' + guid + '&pguid=&hid&empty=' + encodeURI(lbl_no_infodoc) + "&r=" + Math.round(100000*Math.random());
            
            var w = (sync_last_type == type_pw ? frm_pw_width : frm_nonpw_width);
            var w_t = (sync_last_type == type_pw ? frm_pw_margin : frm_nonpw_margin);
            var h = (left ? "184" : frm_height);
            
            if (is_wrt) node_name = lbl_infodoc_wrt;
            var frm_title_len_ = (left ? frm_title_len + 5 : frm_title_len);
            var long_title = (node_name.length>frm_title_len_)
            if (long_title) node_name = node_name.substr(0,frm_title_len_+2); else node_name += "&nbsp;&nbsp;&nbsp;";
            node_name = replaceString("<", "&lt;", replaceString(">", "&gt;", node_name));

            var sEdit='<img src="' + <% =iif(isTeamTimeOwner, "img_edit", "img_view") %>.src + '" width=10 height=10 title="' + <% =iif(isTeamTimeOwner, "lbl_infodoc_edit", "lbl_infodoc_view") %> + '" border=0 onclick="OpenInfodoc(this,\'' + _guid + '\',' + type + ',' + pid + ', \'' + guid + '\', \'\'); return false" class="aslink" style="margin-left:4px">';

            var also = '';
            var lst = eval((also_lst) ? also_lst : collapse_lst);
            for (var j=0; j<lst.length; j++) if (lst[j]!=_guid) also += (also == '' ? '' : ',') + 'inf_frm_' + lst[j];

            var sz = [w, h];
            if ((cookie_name))
            {
                var c = "; " + document.cookie;
                var idx = c.indexOf("; " + cookie_name + "=");
                if (idx>=0)
                {
                    var s = c.substr(idx+cookie_name.length+1);
                    var idx = s.indexOf("]");
                    if (idx>0) s = s.substr(0,idx+1);
                    try { sz = eval(s); }
                    catch(e) { }
                }
            }

            eval("if (!('undefined'===typeof _" + cookie_name + ")) sz = _" + cookie_name);

            var iframe = '<iframe id="frm' + _guid + '" style="border:0px solid #cccccc; width:' + sz[0] + 'px; height:' + sz[1] + 'px; display:block; position: relative;" frameborder=0 src="" class="frm_loading" onload="InfodocFrameLoaded(this);"></iframe>';
            iframe = "<div class='resizable' id='inf_frm_" + _guid + "' also='" + also + "' cookie='" + cookie_name + "' style='border:1px solid #cccccc; width:" + sz[0] + "px; height:" + sz[1] + "px; padding:0px; margin:0px;'>" + iframe + "</div>";
            var sContent = '<a href="" class="actions aslink nu" onclick="return SwitchInfodocFrame(' + collapse_lst  + ',\'' + cookie_name + '\');"><img src="' + (has_infodoc ? (do_hide ? img_plus_ : img_minus_) : (do_hide ? img_plus : img_minus)).src + '" id="div' + _guid + '_image" width=9 height=9 border=0 title="Click to expand or collapse description" style="margin-right:3px">' + (sync_hide_captions ? '' : '<span id="_inf_frm_' + _guid + '" style="width:' + (sz[0]-32) + 'px;white-space:nowrap;overflow:hidden;">' + node_name + '</span>') + '</a>' + (long_title ? '<img src="' + img_path + 'grad_end.png" title="" style="position:absolute;margin-left:-3ex;width:3ex;height:1em;border:0px;">' : '') +  sEdit + '<div id="div' + _guid + '_content" style="display:' + (do_hide ? 'none' : 'block') + ';">' + iframe + '</div>';
            var t = document.createElement("span");
            t.innerHTML = '<div id="div' + _guid + '" class="text small" style="text-align:left; ' + (parent_name = "tdParentNode" ? "" : "margin-top:1ex;") + 'padding-right:2px; font-weight:normal;"><nobr>' + sContent + '</nobr></div>';
            d.appendChild(t);
            SyncLoadFrame('frm' + _guid, src);
//            setTimeout('$("#inf_frm_"' + _guid + ').resizable({ maxHeight:550, maxWidth:850, minHeight:120, minWidth:150, helper:"ui-resizable-helper", autoHide:false, stop:function (event,ui) { SyncSetSizeByUI(ui); } });', 1000);
        }    
    }

    function SyncSetFrameContentVis(ui, vis) {
        can_resize = vis;
        if ((ui.element) && (ui.element.length==1) && (ui.element[0])) {
            ui.element[0].style.background = (vis ? '': '#f0f0f0');
            var f = ui.element[0].firstChild;
            if ((f)) f.style.display = (vis ? 'block' : 'none');
            if ((f) && vis) { f.style.width = ui.size.width; f.style.height = ui.size.height; }
            $(document.body).css("overflow", (vis ? "auto" : "hidden"));
            if (vis) setTimeout("onResize();", 150);
        }
    }

    function SyncSetFrameSize(id, w, h) {
        if ((id))
        {
            var n = id.split(",");
            for (var i=0; i<n.length; i++)
            {
                $("#" + n[i]).css("width", w).css("height", h);
                $("#_" + n[i]).css("width", w-32);
                var f = $("#" + n[i])[0].firstChild;
                if ((f)) { f.style.width = w; f.style.height = h; }
            }
            syncPWWidth();
        }
    }

    function SyncSetFrameSizeByFrame(id) {
        var o = $("#" + id);
        if ((o) && (o.attr('also'))) SyncSetFrameSize(replaceString("[","",replaceString("]","",o.attr('also'))), o.outerWidth(), o.outerHeight());
        syncPWWidth();
    }

    function SyncSetSizeByUI(ui) {
        var d = ui.originalElement.attr('also');
        var id = ui.originalElement.id;
        if ((id != "")) {
            setTimeout("SyncSetFrameSizeByFrame('" + id + "');", 70);
            if ($("#_" + id)) $("#_" + id).css("width", ui.size.width-38);
        }
        else {
            if ((d)) SyncSetFrameSize(d, ui.size.width - 8, ui.size.height - 8);
        }
        var c = ui.originalElement.attr('cookie');
        var cval = "=[" + (ui.size.width-8) + "," + (ui.size.height-8)+"]";
        if ((c)) { document.cookie = c + cval; eval("_" + c + cval); }
    }
    
    function SwitchInfodocFrame(lst, cookie_name)
    {
        var do_hide = -1;
        for (var i=0; i<lst.length; i++)
        {
            var name = lst[i];
            var n = "div" + name + "_content";
            var c = $get(n);
            if ((c))
            {
                if (do_hide==-1) do_hide = (c.style.display != "none");
                if (do_hide)
                {
                    var p = $get("div" + name);
                    if ((p) && (c.style.width)) p.style.width = c.style.width;
                }    
                c.style.display = (do_hide ? "none" : "block");
                //if (do_hide) $("#" + n).hide(400); else $("#"+n).show(400);
            }    
            var d = $get("div" + name + "_image");
            if ((d)) d.src = (do_hide ? (d.src==img_minus_.src ? img_plus_ : img_plus) : (d.src==img_plus_.src ? img_minus_ : img_minus)).src;
            syncPWWidth();
        }
        if (cookie_name!="") document.cookie = id_prefix + "frm_" + cookie_name + "=" + (do_hide ? "1" : "0") + ";";
        if (cookie_name!="scale") SyncCheckDynamicPaging();
        return false;
    }
    
    function InfodocFrameLoaded(frm)
    {
        if ((frm) && (frm.style) && (frm.contentDocument.body.innerHTML!="")) { frm.style.backgroundImage='none'; };
        //if ((frm)) setTimeout(function() {
        //    $(frm).css("position", "relative").css("z-index", "1").css("background", "red");
        //}, 1000);
    }
    
    function GetGUID(id)
    {
        return (id.substr(0,1)=="_" || id.substr(0,1)=="a" ? id.substr(1,32) : id);
    }    
    
    var rframe = null;
    var frame_init = 1;
    
    function ShowInfodocTooltip(ctrl_id, id, type, pid, guid, pguid)
    {
        var t = $find("<% =RadTooltipInfodoc.ClientID %>");
        if ((t) && (!t.isVisible() || t.get_targetControlID()!=ctrl_id))
        {
            var img = $get(ctrl_id);
            if ((img) && !sync_owner && (img.src==img_infodoc2.src || img.src==img_WRTinfodoc2.src)) { t.hide();  return false; }
            var _id = GetGUID(id);
            var is_wrt = (_id!=id);
            var f = $get("frmInfodoc");
            if ((f) && (f.contentWindow)) f.contentWindow.document.body.innerHTML = "<div style='width:99%; height:99%; background: url(" + img_path + "devex_loading.gif) no-repeat center center;'>&nbsp</div>";
            var src = '<% =PageURL(_PGID_EVALUATE_INFODOC) %>?type=' + type + '&id=' + _id + '&pid=' + pid + '&guid=' + guid + '&pguid=' + pguid + '&empty=' + encodeURI(lbl_no_infodoc) + "&r=" + Math.round(100000*Math.random());
            setTimeout("var f = $get('frmInfodoc'); f.src= \"" + src + "\";" , 200);
            t.set_targetControlID(ctrl_id);
            t.show();
            var l = $get("<% =lblInfodocPanel.ClientID %>");
            if ((l) && (rframe) && (frame_init))
            {
                frame_init = 0;
                l.style.overflow = 'hidden';
                var s = rframe.get_Size();
                s.width=<% =GetCookie("tt_frm_w", "300") %>;
                s.height=<% =GetCookie("tt_frm_h", "200") %>;
                l.style.width=s.width;
                l.style.height=s.height;
                if (l.children(0)) 
                {
                    l.children(0).children(0).style.left = (s.width-19) + "px";
                    l.children(0).children(0).style.top = (s.height-19) + "px";
                }
                if (l.children(1))
                {
                    l.children(1).style.width = s.width + "px";
                    l.children(1).style.height = s.height + "px";
                }    
                var o = $get("<% =ResizableControlExtender.ClientID %>_ClientState");
                if ((o)) o.value = s.width + "," + s.height;
             }
        }
        return false;
    }
    
    function onRFrameResize(sender, eventArgs)
    {
        if (rframe!=sender)
        {
            rframe = sender;
        }
        else
        {
            var s = sender.get_Size();
            document.cookie = id_prefix + '_frm_w=' + s.width + ';';
            document.cookie = id_prefix + '_frm_h=' + s.height + ';';
        }    
    }
    
    function OpenInfodoc(img, id, type, pid, guid, pguid)
    {
        var _id = GetGUID(id);
        <% if isTeamTimeOwner Then %>CreatePopup('<% =PageURL(_PGID_RICHEDITOR) %>?field=infodoc&type=' + type + '&id=' + _id + '&pid=' + pid + '&guid=' + guid + '&pguid=' + pguid + '&hid=<% = CInt(PM.ActiveHierarchy) %>&callback=' + id + "&r=" + Math.round(100000*Math.random()), 'InfodocEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=auto,resizable=yes,width=840,height=500');<% Else %>if ((img) && (img.src!=img_infodoc2.src && img.src!=img_WRTinfodoc2.src)) CreatePopup('<% =PageURL(_PGID_EVALUATE_INFODOC) %>?type=' + type + '&id=' +_id + '&pid=' + pid + "&guid=" + guid + "&pguid=" + pguid + "&r=" + Math.round(100000*Math.random()), '', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=auto,resizable=yes,width=760,height=560');<% End If %>
        return false;
    }

    function SyncCreateSliderWithDrags(slider, slider_id, slider_width, onChangeFunction, onResetFunction, sCustomRow)
    {
      return "<table border=0 cellspacing=3 cellpadding=0 style='border:0px'><tr valign='middle'>" + 
             "<td align='right' style='border:0px'><img src='" + (!slider.readonly ? img_decrease.src : img_blank) + "' width=17 height=17 " + (!slider.readonly  ? "style='cursor:pointer;' onmousedown='slider_is_pressed=1; " + onChangeFunction +"(" + slider_id + ",-1,1);' onmouseup='slider_is_pressed=0;' onmouseover='SetGPWArrow(this,-1,1);' onmouseout='SetGPWArrow(this,-1,0);'" : "") + " alt='Decrease value'></td>" +
             "<td align='center' style='border:0px'><div class='sa_slider' style='width:" + slider_width + "px;' id='" + slider_prefix + slider_id + "' ondblclick='" + onResetFunction + "(" + slider_id + ");'></div></td>" + 
             "<td align='left' style='border:0px'><img src='" + (!slider.readonly  ? img_increase.src : img_blank) + "' width=17 height=17 " + (!slider.readonly ? "style='cursor:pointer;' onmousedown='slider_is_pressed=1; " + onChangeFunction +"(" + slider_id + ",+1,1);' onmouseup='slider_is_pressed=0;' onmouseover='SetGPWArrow(this,+1,1);' onmouseout='SetGPWArrow(this,+1,0);'" : "") + " alt='Increase value'></td>" + 
             "</tr>" + sCustomRow + "</table>";
    }
    
    function SyncAddPWGridRow(tbl, text, value_id, value, can_edit, is_visible, is_last, row_offset)
    {
        var r = tbl.rows.length + row_offset;
        if (r<0) r = (sync_infodocsmode<0 || sync_is_gpw ? 1 : 2);
        tbl.insertRow(r);
        
        tbl.rows[r].insertCell(0);
        tbl.rows[r].insertCell(1);
        tbl.rows[r].setAttribute("valign" , "middle");
        tbl.rows[r].setAttribute("id" , id_prefix + "tr" + value_id);
        tbl.rows[r].className =  "text actions " + (is_last ? "grid_row_sel" : ((r&1) ? "grid_row" : "grid_row_alt"));
        tbl.rows[r].cells[0].innerHTML = (is_last ? "<span id='" + id_prefix + value_id + "_username'>" : CreatePending(value_id)) + text + (is_last ? "</span>" : "");
        tbl.rows[r].cells[0].setAttribute("id" , id_prefix + value_id + "_user");
        tbl.rows[r].cells[1].setAttribute("id" , id_prefix + value_id + "_value");
        tbl.rows[r].cells[1].setAttribute("align" , "center");
        if (sync_is_gpw)
        {
            var val = TwinSliderValue2Pair(value);
            var masked = (is_visible<0 ? -1 : sync_hidejudgments && sync_show2me<0 && is_visible==0);
            var readonly = (!can_edit || is_last || masked);
            var handlers = 0; //!is_last;
            tbl.rows[r].cells[1].innerHTML = TwinSliderInit(value_id, val.value, val.adv, masked, readonly, handlers);
        }
        else
        {
            tbl.rows[r].cells[1].innerHTML = CreatePairwise(value_id, value, (is_last ? fill_cons : (can_edit ? fill_edit : fill_view)), can_edit, is_visible);
        }
        if (sync_user_rowheight<0) sync_user_rowheight = tbl.rows[r].clientHeight;
    }
    
    function SyncAddRatingsGridRow(tbl, text, ratings, value_id, value, can_edit, is_visible, is_last, row_offset, hide_prty)
    {
        var is_direct = ((value+"").substr(0,1)=="<% =TeamTimeFuncs.RatingsDirectPrefix%>");
        var undef = !is_direct && (value<0 || value == value_undef);
        var val =  (undef ? value_undef : (is_direct ? value.substr(1) : value)*1);

        var r = tbl.rows.length + row_offset;
        if (r<0) r = (sync_infodocsmode<0 ? 1 : 2);
        tbl.insertRow(r);

        var sCombo = "&nbsp;";
        if (!is_last && is_visible)
        {
            if (can_edit)
            {
                sCombo  = "<select name='" + id_prefix + value_id  +"_ratings' onchange=\"SaveRating('" + value_id + "');\">";
                for (var i=0; i<ratings.length; i++)
                {
                    sCombo += "<option value='" + ratings[i][0] +"'"+(value==ratings[i][0] || (undef && ratings[i][2]==value_undef) ? " selected" : "")+" title='" + ratings[i][1] + "'>" + ShortString(ratings[i][1], 50, false) + "</option>";
                    if (value==ratings[i][0]) val = ratings[i][2];
                }
                if (is_direct) sCombo += "<option value='-2' selected disabled='1'>Direct value</option>";
                sCombo += "</select>";
            }
            else
            {
                sCombo = "";
                for (var i=0; i<ratings.length; i++)
                    if (value==ratings[i][0] || (value<0 && ratings[i][0]<0)) { 
                        sCombo = "<span id='" + id_prefix + value_id + "_rating_name'>" +  ratings[i][1] + "</span>"; 
                        val = ratings[i][2]; 
                    }
                if(is_direct) sCombo = "<span id='" + id_prefix + value_id + "_rating_name' class='gray'>Direct value</span>"; 
            }    
        }
        
        tbl.rows[r].insertCell(0);
        tbl.rows[r].insertCell(1);
        if (!hide_prty) {
            tbl.rows[r].insertCell(2);
            tbl.rows[r].insertCell(3);
        }
        tbl.rows[r].className = "text actions " + (is_last ? "grid_row_sel" : ((r&1) ? "grid_row" : "grid_row_alt"));
        tbl.rows[r].style.height = "24px";
        tbl.rows[r].setAttribute("valign" , "middle");
        tbl.rows[r].setAttribute("id" , id_prefix + "tr" + value_id);
        tbl.rows[r].cells[0].innerHTML = (is_last ? "<span id='" + id_prefix + value_id + "_username'>" : CreatePending(value_id)) + text + (is_last ? "</span>" : "");
        tbl.rows[r].cells[1].innerHTML = sCombo;
        if (!hide_prty) {
            tbl.rows[r].cells[2].setAttribute("id" , id_prefix + value_id + "_value");
            tbl.rows[r].cells[2].setAttribute("align" , "right");
            tbl.rows[r].cells[2].innerHTML = (undef || !is_visible ? "&nbsp;" : val);
            if (is_direct)  tbl.rows[r].cells[2].setAttribute("class" , "gray");
            //        tbl.rows[r].cells[2].innerHTML = (undef || !is_visible ? "&nbsp;" : ShowDecNum(val, sync_last_data[5]+2));
            tbl.rows[r].cells[3].setAttribute("id" , id_prefix + value_id + "_bar");
            tbl.rows[r].cells[3].innerHTML = CreateBar((is_visible ? val : (undef ? 0 : 1)), (is_visible ? (is_last ? fill_red : fill_blue) : fill_disabled));
        }
        if (sync_user_rowheight<0) sync_user_rowheight = tbl.rows[r].clientHeight;
    }
    
    function SyncAddDirectGridRow(tbl, text, value_id, value, can_edit, is_visible, is_last, row_offset)
    {
        var undef = (value<0 || value == value_undef);
        var val =  (undef ? value_undef : value);

        var r = tbl.rows.length + row_offset;
        if (r<0) r = (sync_infodocsmode<0 ? 1 : 2);
        tbl.insertRow(r);

        var sSlider = "&nbsp;";
        var sValue = (undef || !is_visible ? "&nbsp;" : ShowDecNum(val));
       
        tbl.rows[r].insertCell(0);
        tbl.rows[r].insertCell(1);
        tbl.rows[r].insertCell(2);
        tbl.rows[r].className = "text actions " + (is_last ? "grid_row_sel" : ((r&1) ? "grid_row" : "grid_row_alt"));
        tbl.rows[r].style.height = "24px";
        tbl.rows[r].setAttribute("valign" , "middle");
        tbl.rows[r].setAttribute("id" , id_prefix + "tr" + value_id);

        if (!can_edit)
        {
            sValue = "<span id='" + id_prefix + value_id + "_static'>" + sValue +  "</span><input type='hidden' name='" + id_prefix + value_id + "_value' value='" + (undef ? "" : ShowDecNum(val)) + "'>";
        }
        else
        {
            sValue = "<input type='text' name='" + id_prefix + value_id + "_value' value='" + (undef ? "" : (!is_visible ? "x.xx" : ShowDecNum(val))) + "' style='width:5em; text-align:right'" + (can_edit && is_visible ? "" : " disabled=1") + " onblur='if (this.value!=\"\") { SliderDirectEditChange(" + value_id + "); CheckAndWarnDirectEdit(" + value_id + "); } ' onkeyup='SliderDirectEditChange(" + value_id + ");'>";
        }

        var slider_id = value_id;
        var slider_direct = new Object();
        slider_direct.num = slider_id;
        slider_direct.min = 0;
        slider_direct.max = slider_direct_max;
        slider_direct.val = (undef ? -1 : Math.round(val*slider_direct_max));
        //slider_direct.masked = sync_hidejudgments && sync_show2me && !is_visible;
        slider_direct.masked = (is_visible<0 ? -1 : sync_hidejudgments && sync_show2me<0 && is_visible==0);

        slider_direct.onchange = onChangeSliderDirect;
        slider_direct.color = (is_last ?  "#6aa84f" : "#0058a3");

        slider_direct.readonly = !can_edit || is_last || slider_direct.masked;
        slider_direct.handler = !is_last && can_edit;
        if (is_last) slider_consensus = slider_direct; else sliders[slider_id] = slider_direct;

        tbl.rows[r].cells[2].innerHTML = SyncCreateSliderWithDrags(slider_direct, slider_id, 150, "SliderDirectChange", "ResetSliderDirect", "");

        setTimeout("sliderInitByName('" + slider_prefix + slider_direct.num + "', " + (is_last ? "slider_consensus" :  "sliders[" + slider_id + "]") + ");", 50);

        tbl.rows[r].cells[0].innerHTML = (is_last ? "<span id='" + id_prefix + value_id + "_username'>" : CreatePending(value_id)) + text + (is_last ? "</span>" : "");
        tbl.rows[r].cells[1].setAttribute("id" , id_prefix + value_id + "_value");
        tbl.rows[r].cells[1].setAttribute("align" , "right");
        tbl.rows[r].cells[1].innerHTML = (slider_direct.masked ? "x.xx" :  sValue);
        if (sync_user_rowheight<0) sync_user_rowheight = tbl.rows[r].clientHeight;
    }
    
    function SyncAddStepFunctionGridRow(tbl, text, intervals, value_id, value, can_edit, is_visible, is_last, row_offset)
    {
        var undef = isUndefinedByID(value, type_step) || (value_id == id_groupres && value<0);
        var val =  (undef ? value_undef : (is_last ? value : GetStepValue(value)));

        var r = tbl.rows.length + row_offset;
        if (r<0) r = (sync_infodocsmode<0 ? 1 : 2);
        tbl.insertRow(r);

        var sEdit = "&nbsp;";
        if (!can_edit)
        {
           if (!is_last) sEdit += "<span id='" + id_prefix + value_id + "_static'>" + (undef ? "" : (!is_visible ? "x.xx" : ShowDecNum(value))) +  "</span>";
        }
        else
        {
           sEdit += "<input type='text' name='" + id_prefix + value_id + "_edit' value='" + (undef ? "" : (!is_visible ? "x.xx" : ShowDecNum(value))) + "' style='width:5em; text-align:right'" + (can_edit && is_visible ? "" : " disabled=1") + " onblur='if (this.value!=\"\") {StepFunctionEditChange(" + value_id + "); CheckAndWarnStepFunctionEdit(" + value_id + "); }'  onkeyup='StepFunctionEditChange(" + value_id + ");'>";
        }

        tbl.rows[r].insertCell(0);
        tbl.rows[r].insertCell(1);
        tbl.rows[r].insertCell(2);
        tbl.rows[r].insertCell(3);
        tbl.rows[r].className = "text actions " + (is_last ? "grid_row_sel" : ((r&1) ? "grid_row" : "grid_row_alt"));
        tbl.rows[r].style.height = "24px";
        tbl.rows[r].setAttribute("valign" , "middle");
        tbl.rows[r].setAttribute("id" , id_prefix + "tr" + value_id);
        tbl.rows[r].cells[0].innerHTML = (is_last ? "<span id='" + id_prefix + value_id + "_username'>" : CreatePending(value_id)) + text + (is_last ? "</span>" : "");
        tbl.rows[r].cells[1].setAttribute("align" , "right");
        tbl.rows[r].cells[1].innerHTML = sEdit;
        tbl.rows[r].cells[2].setAttribute("id" , id_prefix + value_id + "_value");
        tbl.rows[r].cells[2].setAttribute("align" , "right");
        tbl.rows[r].cells[2].innerHTML = (undef || !is_visible ? "&nbsp;" : ShowDecNum(val));
        tbl.rows[r].cells[3].setAttribute("id" , id_prefix + value_id + "_bar");
        tbl.rows[r].cells[3].innerHTML = CreateBar((is_visible ? val : (undef ? 0 : 1)), (is_visible ? (is_last ? fill_red : fill_blue) : fill_disabled));
        if (sync_user_rowheight<0) sync_user_rowheight = tbl.rows[r].clientHeight;
    }
    
    function SyncAddRegularUtilityCurveGridRow(tbl, text, options, value_id, value, can_edit, is_visible, is_last, row_offset)
    {
        var undef = isUndefinedByID(value, type_ruc) || (value_id == id_groupres && value<0);
        var val =  (undef ? value_undef : (is_last ? value : GetRUCValue(value)));

        var r = tbl.rows.length + row_offset;
        if (r<0) r = (sync_infodocsmode<0 ? 1 : 2);
        tbl.insertRow(r);

//        var sEdit = "<input type='text' name='" + id_prefix + value_id + "_old' value='" + (undef ? value_undef : value) + "'>";
        var sEdit = "&nbsp;";

        if (!can_edit)
        {
            if (!is_last) sEdit += "<span id='" + id_prefix + value_id + "_static'>" + (undef ? "" : (!is_visible ? "x.xx" : ShowDecNum(value))) +  "</span>";
        }
        else
        {
            sEdit += "<input type='text' name='" + id_prefix + value_id + "_edit' value='" + (undef ? "" : (!is_visible ? "x.xx" : ShowDecNum(value))) + "' style='width:5em; text-align:right'" + (can_edit && is_visible ? "" : " disabled=1") + " onblur='if (this.value!=\"\") { RUCEditChange(" + value_id + "); CheckAndWarnRUCEdit(" + value_id + "); }' onkeyup='RUCEditChange(" + value_id + ");'>";
        }

        tbl.rows[r].insertCell(0);
        tbl.rows[r].insertCell(1);
        tbl.rows[r].insertCell(2);
        tbl.rows[r].insertCell(3);
        tbl.rows[r].className = "text actions " + (is_last ? "grid_row_sel" : ((r&1) ? "grid_row" : "grid_row_alt"));
        tbl.rows[r].style.height = "24px";
        tbl.rows[r].setAttribute("valign" , "middle");
        tbl.rows[r].setAttribute("id" , id_prefix + "tr" + value_id);
        tbl.rows[r].cells[0].innerHTML = (is_last ? "<span id='" + id_prefix + value_id + "_username'>" : CreatePending(value_id)) + text + (is_last ? "</span>" : "");
        tbl.rows[r].cells[1].setAttribute("align" , "right");
        tbl.rows[r].cells[1].innerHTML = sEdit;
        tbl.rows[r].cells[2].setAttribute("id" , id_prefix + value_id + "_value");
        tbl.rows[r].cells[2].setAttribute("align" , "right");
        tbl.rows[r].cells[2].innerHTML = (undef || !is_visible ? "&nbsp;" : ShowDecNum(val));
        tbl.rows[r].cells[3].setAttribute("id" , id_prefix + value_id + "_bar");
        tbl.rows[r].cells[3].innerHTML = CreateBar((is_visible ? val : (undef ? 0 : 1)), (is_visible ? (is_last ? fill_red : fill_blue) : fill_disabled));
        if (sync_user_rowheight<0) sync_user_rowheight = tbl.rows[r].clientHeight;
    }

    function SyncAddResultsGridRow(tbl, results, mode, max_value)
    {
        var r = tbl.rows.length;
        tbl.insertRow(r);
        
        tbl.rows[r].insertCell(0);
        tbl.rows[r].insertCell(1);
        tbl.rows[r].insertCell(2);
        tbl.rows[r].insertCell(3);
        if (!mode) tbl.rows[r].insertCell(4);

        tbl.rows[r].className = "text actions " + ((r&1) ? "grid_row" : "grid_row_alt");
        tbl.rows[r].setAttribute("valign" , "middle");
        tbl.rows[r].cells[0].setAttribute("align" , "center");
        tbl.rows[r].cells[0].innerHTML = results[0];
        tbl.rows[r].cells[1].innerHTML = results[1];
        var idx = (mode ? 2 : 3);
        tbl.rows[r].cells[idx+1].setAttribute("align" , "center");
        if (mode!=2)
        {
            var res = results[2];
            tbl.rows[r].cells[2].setAttribute("align" , (res>=0 ? "right" : "center"));
            tbl.rows[r].cells[2].innerHTML = (res>=0 ? ShowDecNum(100*res) + "%" : "—");
            tbl.rows[r].cells[2].className = "label_my";
            tbl.rows[r].cells[idx+1].innerHTML =  (res >=0 ? CreateBar(res/max_value, "graph_my") : "&nbsp;");
        }
        if (mode!=1) {
            var res = results[3];
            tbl.rows[r].cells[idx].setAttribute("align" , (res>=0 ? "right" : "center"));
            tbl.rows[r].cells[idx].innerHTML = (res>=0 ? ShowDecNum(100*res) + "%" : "—");
            tbl.rows[r].cells[idx].className = "label_combined";
            tbl.rows[r].cells[idx+1].innerHTML +=  (res>=0 ? CreateBar(res/max_value, "graph_combined") : "&nbsp;");
        }
    }
    
    function SyncCreateLegend(rows, is_values, hide_vals)
    {
        var l = "<table class='tbl' cellspacing=1 cellpadding=2 border=0>";
        for (var i=0; i<rows.length; i++) {
            l += "<tr class='" + (i==0 ? "tbl_hdr" : ((i&1) ? "tbl_row" : "tbl_row_alt")) + " tbl_margins text'><td"+(is_values ? "" : " align=center")+">" + rows[i][0] + "</td>";
            if (typeof hide_vals == "undefined" || !(hide_vals)) l += "<td"+(is_values ? " align=right" : "")+">" + rows[i][1] + "</td>";
            if (sync_usekeypads) l += "<td align=center>" + rows[i][2] + "</td>";
            l += "</tr>";
        }
        l +="</table>";
        return l;
    }

    function SyncCreateIntensitiesList(rows, act, value_id, hide_prty)
    {
        // rows[i][3] = [1,'Highly Likely','0.95','95.00%','Highly likely to occur']
        var l = "<div style='height:100%; overflow:auto;' id='divLegend'><table class='tbl' cellspacing=1 cellpadding=" + (rows.length<10 ? 1 : 0) +" border=0>";
        var bh = bar_height;
        bar_height = 8;
        var bw = bar_width;
        bar_width = 80;
        var max_len = 0;
        for (var i=1; i<rows.length; i++) {
            if (("" + rows[i][3][3]).length>max_len) max_len = ("" + rows[i][3][3]).length;
        }
        if (max_len<3) max_len = 3;
        for (var i=0; i<rows.length; i++) {
            var n = rows[i][0];
            if (i>0) {
                var is_active = (act*1==rows[i][3][0]*1);
                var nm = (rows[i][3][0]<0 ? "undef" : rows[i][3][0]);
                if ((rows[i][3]) && (rows[i][3][4]) && (rows[i][3][4]!="")) n += "<div class='text small gray' style='padding-left:20px;'>" + rows[i][3][4] + "</div>";
                n = "<label><input type='radio' name='rating_value' id='int_" + nm + "' " + (is_active ? "checked " : "") + "value='" + rows[i][1] + "' onclick='onRatingClick(" + value_id + ", " + (rows[i][3][0]<0 ? -1 : rows[i][3][0]) + ");'>" + n + "</label>";
                if (is_active) setTimeout('var o = document.getElementById("int_' + nm + '"); if ((o)) o.scrollIntoView();', 10);
            }
            l+="<tr class='" + (i==0 ? "tbl_hdr" : ((i&1) ? "tbl_row" : "tbl_row_alt")) + " tbl_margins text'><td>" + n + "</td>" + (hide_prty ? "" : "<td align=right width='" + (bar_width + 12 + max_len*6) + "'><nobr>" + (i<=0 || (typeof rows[i][3]=="undefined") || (rows[i][3][0]<0) ? "" : "<div style='width:" + bar_width + " px; float:left'>" + CreateBar(rows[i][3][2], fill_blue) + "</div>") + rows[i][1] + "</nobr></td>") + (sync_usekeypads ? "<td align=center>" + rows[i][2] + "</td>" : "") + "</tr>";
        }
        l +="</table><br></div>";
        bar_height = bh;
        bar_width = bw;
        return l;
    }
    
    function ReorderUsers(users, email, fld_idx, ignore_paging)
    {
        if ((users))
        {
            var lst = [];
            email = email.toLowerCase();
            var act_idx = -1;
            for (var i=0; i<users.length; i++) if (users[i][fld_idx].toLowerCase()==email) { lst.push(users[i]); act_idx = i; }
            var a = 0;
            var b = users.length;
            sync_last_users_count = b;
            if (!(ignore_paging) && (sync_showusers_by_pages!=0) && b>3)
            {
                var pg_size = Math.abs(sync_userspage_size);

                if ((sync_userspage_size<1) && (tdScrollableList) && (sync_user_row_starts>0) && (sync_user_rowheight>0))
                {
                    var t = tdScrollableList.clientHeight - sync_user_row_starts - sync_user_rowheight - 80;
                    var pg_size = Math.floor(t/sync_user_rowheight);
                    if (pg_size<3) pg_size = 3;
                    if (pg_size>25) pg_size = 25;
                    msg("Set dynamic page size as " + pg_size + " rows");
//                    alert(pg_size);
                }

                sync_userspage_links = "";
                if (sync_userspage_active<1) sync_userspage_active = 1;
                var pg_total = Math.ceil(b/pg_size);
                if (sync_userspage_active > pg_total) sync_userspage_active = pg_total;
                document.cookie = "<% =_COOKIE_USERSPAGE %>=" + sync_userspage_active;
                a = (sync_userspage_active-1) * (pg_size-1);
                b = a + pg_size -1;
                if (act_idx>=a && act_idx<=b) b+=1;
                if (b>users.length) b = users.length;
                for (j=0; j<pg_total; j++)
                {
                  var p = (j+1);
                  if (p==sync_userspage_active) p = "<span style='background:#e0e0e0; padding:0px 3px;'><b>" + p + "</b></span>"; else p = "<a href='' onclick='SyncShowUsersPage(" + (j+1) + "); return false;' class='actions'>" + p + "</a>";
                  sync_userspage_links += (sync_userspage_links == "" ? "" : " | ") + p;
                }
                if (pg_total<=1) sync_userspage_links = ""; else sync_userspage_links = "<% = ResString("lblTTUsersActivePage") %> " + sync_userspage_links;

                var pg = [-10, 5, 10, 15, 20, 0];
                var ps = "";
                for (var k=0; k<pg.length; k++)
                {
                  var n = pg[k];
                  var oc = 'SyncSetPageSize(' + pg[k] + ');';
                  var act = (sync_userspage_size == pg[k]);
                  if (k==0) { n = "<% =JS_SafeString(ResString("lblTTPageSmart")) %>"; if (sync_userspage_size<0) act = 1; }
                  if (k==pg.length-1)
                  {
                    n = "<% =JS_SafeString(ResString("lblTTPageAll")) %>";
                    oc = (sync_last_users_count>15 ? 'SyncConfirmSwitchPaging();' : 'SyncSwitchPaging(0);');
                  }
                  if (k<=0 || pg[k]<=users.length) ps += (ps == "" ? "" : " | ") + (act ? "<b>" + n + "</b>" : "<a href='' onclick='" + oc + "; return false;'>" + n + "</a>");
                }
                if (pg_total>1) sync_userspage_links += "</span><span style='float:right; text-align:right'><nobr>Show by: " + ps + "</nobr>";
                
                if (sync_userspage_links!="") sync_userspage_links = "<div class='text small' style='text-align:left; height:1em; verticla:align:medium; padding:3px 4px; border:1px solid #e0e0e0;'><span style='float:left; width:50%;'>" + sync_userspage_links + "</span></div>";
            } else {
                if ((sync_userspage_links!="")) sync_userspage_links = "";
            }
            for (var i=a; i<b; i++) if (i!=act_idx) lst.push(users[i]);
            return lst;
        }
        else return users;
    }

    function SyncShowUsersPage(pg)
    {
        sync_userspage_active = pg;
        SyncRenderData(sync_last_data_full, true);
        return false;
    }

    function AddUsersPageLine(tbl, colspan)
    {
        if ((tbl) && (sync_showusers_by_pages) && sync_userspage_links!="") 
        {
            var r = tbl.rows.length ;
            tbl.insertRow(r);
            tbl.rows[r].insertCell(0);
            tbl.rows[r].cells[0].setAttribute("colspan" , colspan);
            tbl.rows[r].cells[0].innerHTML = sync_userspage_links;
        }
    }

    function syncPWWidth() {
        setTimeout(function () {
            var tdpw = $("#tblPWControl");
            var scr = $("#divScrollable");
            if ((tdpw) && (tdpw.length) && (scr) && (scr.length)) {
                tdpw.css({"width": "100%"});
                scr.width(tdpw.width() + 24);
            }
        }, 110);
    }
    
    function SyncCreateEvaluationScreen(ContentName, Legend, NoScrollable, pages_options)
    {
        SyncResizeList();
        syncPWWidth();
        return  "<table id='tbl_" + ContentName + "' width=99% style='height:99%;' border=0 cellspacing=0 cellpadding=0><tr style='height:3em'><td " + (show_legend && Legend!="" ? "colspan='2' " : "") +"id='tdTask' style='height:3em' class='text'></td></tr><tr><td valign=top align=center><table style='height:100%;' border=0 cellspacing=0 cellpadding=0><tr><td valign=top align=left id='tdScrollableList'" + (NoScrollable || !pages_options ? ">" : " onmouseout='mouseover=false; onHidePageOptions();' onmouseover='mouseover=true; ShowPageOptions(1);'><span id='divPageOptions' class='text small page_options' onclick='SyncSwitchPaging(1);'><nobr><% =JS_SafeString(ResString("lblTTByPages")) %></nobr></span>") + "<div id='divScrollable' " + (NoScrollable ? "" : " class='scrollable' style='text-align:center; overflow-x: visible; padding-right:20px;'") + "><div id='" + ContentName + "'></div></div></td>" + (show_legend && Legend!="" ? "<td id='tdLegend' align=left class='text' valign='top' style='padding-left:1em'>" + Legend + "</td>" : "") + "</tr></table></td></tr></table>";
    }

    function SyncSetPageSize(pg)
    {
        sync_userspage_size = pg;
        sync_user_last_type = "";
        document.cookie = "<% =_COOKIE_USERS_PAGES_SIZE %>=" + sync_userspage_size;
        SyncRenderData(sync_last_data_full, true);
    }

    function SyncSwitchPaging(do_pages)
    {
        sync_showusers_by_pages = ((do_pages) ? 1 : 0);
        document.cookie = "<% =_COOKIE_USERS_PAGES_USE %>=" + sync_showusers_by_pages;
        sync_user_last_type = "";
        SyncRenderData(sync_last_data_full, true);
        return false;
    }

    function SyncConfirmSwitchPaging() {
        jDialog("<% =JS_SafeString(ResString("msgConfirmTTShowAll")) %>", 0, "SyncSwitchPaging(0);", " ", "<% = JS_SafeString(ResString("titleConfirmation")) %>", 400, 160, "<% = JS_SafeString(ResString("btnYes")) %>", "<% = JS_SafeString(ResString("btnNo")) %>");
    }

    var mouseover = false;
    function onHidePageOptions() {
        setTimeout(function () {
            if (!mouseover) ShowPageOptions(false);
        }, 2000);
    }

    function ShowPageOptions(vis)
    {
        if ((vis) && (sync_showusers_by_pages || sync_last_users_count<3 || sync_distr_view)) return false;
        var o = $("#divPageOptions");
        if ((o) && (o.length)) {
            if ((vis)) o.fadeIn(350); else o.fadeOut(700);
        }
    }
    
    function isActiveUser(email)
    {
        return (email.toLowerCase()==sync_user);
    }
    
    function GetUserName(data, idx)
    {
        var restr = (data[id_keypad]==-2);
        var c = (restr ? "#cc0000; font-style:italic;" : (isActiveUser(data[id_email]) ? "#0000cc" : (data[id_online] ? "#000099" : "#666666")));
        var anonym = (sync_anonymous && !isActiveUser(data[id_email]));
        var extra = ((sync_usekeypads && data[id_keypad]>0) || data[id_keypad]==-1 ? "&nbsp;<span class='small gray' style='font-size:8pt'>(" + (data[id_keypad]==-1 ? lbl_viewonly : "#"+data[id_keypad]) + ")</span>" : "");
//        return (anonym ? "Person " + (idx<10 ? "0" : "") + idx : "<a href='mailto:" + data[id_email] + "' style='color:" + c + "'>" + (data[id_name]=="" ? data[id_email] : data[id_name]) + "</a>") + extra;
        return "<nobr>" + (anonym ? "Person " + (idx<10 ? "0" : "") + idx : "<span style='color:" + c + "'" + (data[id_email]!=data[id_name] && data[id_name]!="" ? " title='" + data[id_email] + "'" : "") + ">" + htmlEscape(data[id_name]=="" ? data[id_email] : data[id_name]) + "</span>") + extra + "</nobr>";
    }
    
    function CreateUserName(data, can_edit, idx)
    {
        var user = "";
        if ((data))
        {
            var restr = (data[id_keypad]==-2);
            var act = isActiveUser(data[id_email]);
            var is_vis = (!restr && (!sync_hidejudgments || sync_show2me==sync_step || act) ? 1 : 0);
            var sComment = "";
            if (sync_showcomments)
            {
//                if (restr || !can_edit)
                if (restr || !is_vis || data[id_keypad]==-1)    // data[id_keypad]==-1 is for ViewOnly
                {
                    sComment = "<img src='" + (data[id_comment] != "" ? img_commentd_.src : img_blank) + "' width=15 height=15 title='" + replaceString("'", "&#39;", data[id_comment]) + "' border=0>";
                }
                else
                {
                    sComment = "<a href='' onclick='SwitchComment(" + data[id_uid] + "); return false;'><img src='" + (data[id_comment] == "" ? img_comment_e : img_comment).src + "' width=15 height=15 id='" + id_prefix + data[id_uid] + "_comment_img' title='" + (can_edit ? lbl_comment_e : lbl_comment) + "' border=0 onmouseover='ChangeCommentIcon(this,true);' onmouseout='ChangeCommentIcon(this,false);'></a><div style='display:none;' class='tt_comment' id='" + id_prefix + data[id_uid] + "_comment'></div>";
                }
                sComment += "&nbsp;";
            }
//            var anonym = (sync_anonymous && !isActiveUser(data[id_email]));
            user = sComment + "<span id='" + id_prefix + data[id_uid] + "_username'>" + GetUserName(data, idx) + "</span>";
         }
         return user;
    }
    
    function SyncSetUserBold(uid, type, bold)
    {
        var u = $get(id_prefix + uid + "_username");
        if ((u)) u.style.fontWeight = (bold ? "bold" : "normal");
        if (type==type_rating || type==type_direct || type==type_step || type==type_ruc)
        {
            var v = $get(id_prefix + uid + "_value");
            if ((v)) v.style.fontWeight = (bold ? "bold" : "normal");
            var n = $get(id_prefix + uid + "_rating_name");
            if ((n)) n.style.fontWeight = (bold ? "bold" : "normal");
            var s = $get(id_prefix + uid + "_static");
            if ((s)) s.style.fontWeight = (bold ? "bold" : "normal");
        }
    }
    
    function SyncUpdateUserNames()
    {
        if ((sync_last_data))
        {
            var lst = (sync_last_type == type_pw || sync_last_type == type_direct ? sync_last_data[1] : sync_last_data[2]);
            if ((lst)) {
                for (var i=0; i<lst.length; i++)
                {
                    var user = lst[i];
                    var u = $get(id_prefix + user[id_uid] + "_username");
                    if ((u)) u.innerHTML = GetUserName(user, i+1);
                }
            }
        }
    }

    function getRatingValue(val) {
        return (val + "").replace(/\*/i, "");
    }
    
    function SyncCheckRows(new_data, old_data)
    {
        if (sync_distr_view) return true;
        var res = false;
        var has_new = 0;
        var do_resize = false;
        if ((new_data) && new_data!="" && (old_data) && old_data!="" && (old_data[0][0].toString()==new_data[0][0].toString()))
        {
            msg("Check existing data");
            var idx = (sync_last_type == type_pw || sync_last_type == type_direct ? 1 :2);
            var re_render_users = "";
            var re_render_cnt = 0;
            for (var i=0; i<old_data[idx].length; i++)
            {
                var od = old_data[idx][i];
                var nd = GetDataByID(od[id_uid], new_data);
                var changed_mode = ((nd) && nd[id_keypad]!=od[id_keypad] && (nd[id_keypad]<0 || od[id_keypad]<0));
//                var changed_mode = ((nd) && nd[id_keypad]!=od[id_keypad] && (nd[id_keypad]==-1 || od[id_keypad]==-1));
                var changed = ((nd) && (nd[id_comment]!=od[id_comment] || Math.round(num_maxdecimals*getRatingValue(nd[id_value])) != Math.round(num_maxdecimals*getRatingValue(od[id_value])) || changed_mode));  // try to compare only num_maxdecimals after the point;
                if ((changed) && isActiveUser(nd[id_email]) && sync_pending_id!="")
                {
                    msg("Looks like received data for current user is not the last one, ignore it due to waiting with PendingID " + sync_pending_id);
                    changed = 0;
                    sync_hash = sync_hash_old;
                    msg("Restore hash to prev old value: " + sync_hash);
                    return false;
                }
                var changed_user = ((nd) && (nd[id_name]!=od[id_name] || nd[id_keypad]!=od[id_keypad] || nd[id_online]!=od[id_online]));
                if (changed) has_new = 1;
                if (changed_mode) { re_render_users+=";" + od[id_uid] + ";"; re_render_cnt += 1; }
                if (changed || changed_user)
                {
                    if (changed_user)
                    {
                        var u = $get(id_prefix + nd[id_uid] + "_username");
                        if ((u)) {
                            u.innerHTML = GetUserName(nd, i+1); 
                        //} else  {
                        //    msg("Unable to find user info #" + nd[id_email] + ". Force redraw");
                        //    return true;
                        }
                        msg("Detect change user info #" + nd[id_email]);
                        res = false;
                    }
                    if (nd[id_comment]!=od[id_comment]) UpdateCommentIcon(nd[id_uid], nd[id_comment]=="");
                    if (changed) { SyncSetUserBold(od[id_uid], sync_last_type, 1); setTimeout("SyncSetUserBold('" + od[id_uid] + "', '" + sync_last_type + "', 0);", sync_timeout); }
                    var act = isActiveUser(od[id_email]);
                    var restr = nd[id_keypad]==-2;
                    var can_edit = !restr && (sync_owner || act) && (nd[id_keypad]!=-1);
                    if (nd[id_value]!=od[id_value])
                    {
                        switch (sync_last_type)
                        {
                            case type_pw:
                                msg("Update PW data for " + nd[id_email]);
                                var is_vis = (!restr && (!sync_hidejudgments  || sync_show2me==sync_step || act) ? 1 : 0);
                                if (!is_vis && sync_show_pwside && nd[id_keypad]>0 && sync_usekeypads) is_vis = -1;
                                SetPairwise(od[id_uid], nd[id_value], (can_edit ? fill_edit : fill_view), can_edit, is_vis);
                                res = false;
                                break;
                            case type_rating:
                                var r = $get(id_prefix + od[id_uid] + "_ratings");
                                if (!(r)) r = eval("theForm." + id_prefix + od[id_uid] + "_ratings");
                                var v = nd[id_value]*1;
                                if (v<0) v=-1;
                                if ((r) && r.value*1 != v) r.value = v;
                                msg("Update Rating data for " + nd[id_email]);
                                SetRating(od[id_uid]);
                                SyncRefreshRating(od[id_uid], v);
                                res = false;
                                break;
                            case type_direct:
                                msg("Update Direct data for " + nd[id_email]);
                                SliderDirectEditChange(od[id_uid], nd[id_value]);
                                res = false;
                                break;
                            case type_step:
                                msg("Update SF data for " + nd[id_email]);
                                StepFunctionEditChange(od[id_uid], nd[id_value]);
                                res = false;
                                break;
                            case type_ruc:
                                msg("Update UC data for " + nd[id_email]);
                                RUCEditChange(od[id_uid], nd[id_value]);
                                res = false;
                                break;
                        }
                    }
                }
            }
            var idx_n = old_data[idx].length;

            for (var i=0; i<old_data[idx].length; i++)
            {
                var od = old_data[idx][i];
                var nd = GetDataByID(od[id_uid], new_data);
                if (!(nd) || re_render_users.indexOf(";"+od[id_uid]+";")>=0)
                {
                    var tr = $get(id_prefix+ "tr" + od[id_uid]);
                    if ((tr) && (tr.parentNode) && (tr.parentNode.rows.length>tr.rowIndex)) {
                        tr.parentNode.removeChild(tr); 
                        msg("Delete user " + od[id_email]); 
                        do_resize = true;
                    } 
                }
            }    

            var new_users = ReorderUsers(new_data[idx], sync_user, 1, true);
            for (var i=0; i<new_users.length; i++)
            {
                var nd = new_users[i];
                var od = GetDataByID(nd[id_uid], old_data);
                var u = $get(id_prefix + nd[id_uid] + "_username");
                if ((!(od) && !(u)) || (re_render_users.indexOf(";"+nd[id_uid]+";")>=0))
                {
                    idx_n += 1;
                    var idx_offset = -1;
                    var act = isActiveUser(nd[id_email]);
                    
                    if ((act)) idx_offset = -idx_n;
                    var restr = nd[id_keypad]==-2;
                    has_new = 1;
                    
                    if (new_users.length>1)
                    {
                        var nu_idx =  (i==0 ? 0 : (i>0 ? -1 : +1));
                        var pos_user = new_users[i+nu_idx];
                        if ((pos_user))
                        {
                            var row = $get(id_prefix + "tr" + pos_user[id_uid]);
                            if ((row) && (row.parentElement) && (row.parentElement.parentElement) && (row.parentElement.parentElement.rows))
                            {
                                idx_offset = row.rowIndex - nu_idx - row.parentElement.parentElement.rows.length;
                            }
                        }
                        idx_offset += re_render_cnt;
                        if (idx_offset>-1) idx_offset = -1;
                    }

                    var passed = 0;
                    can_edit = (sync_owner || act) && (nd[id_keypad]!=-1);
                    var user_data = CreateUserName(nd, can_edit, idx_n);
                    var is_vis = (!restr && (!sync_hidejudgments || sync_show2me==sync_step || act) ? 1 : 0);
                    if (!is_vis && sync_show_pwside && sync_usekeypads && nd[id_keypad]>0) is_vis = -1;
                    if ((restr)) is_vis = -1;
                    msg("Detected new user: " + nd[id_email]);
                    do_resize = true;
                    switch (sync_last_type)
                    {
                        case type_pw:
                            SyncAddPWGridRow(tblPWGrid, user_data,  nd[id_uid], nd[id_value], !restr && can_edit, is_vis, 0, idx_offset);
                            passed = 1;
                            break;
                        case type_rating:
                            var list_full = new Array().concat(new_data[1]);
                            SyncAddRatingsGridRow(tblRatingsGrid, user_data, list_full, nd[id_uid], nd[id_value], !restr && can_edit, is_vis, 0, idx_offset, new_data[6][3]);
                            passed = 1;
                            break;
                        case type_direct:
                            SyncAddDirectGridRow(tblDirectGrid, user_data,  nd[id_uid], nd[id_value], !restr && can_edit, is_vis, 0, idx_offset);
                            passed = 1;
                            break;
                        case type_step:
                            SyncAddStepFunctionGridRow(tblStepFunctionGrid, user_data, new_data[1], nd[id_uid], nd[id_value], !restr && can_edit, is_vis, 0, idx_offset);
                            passed = 1;
                            break;
                        case type_ruc:
                            SyncAddRegularUtilityCurveGridRow(tblRUCGrid, user_data, new_data[1], nd[id_uid], nd[id_value], !restr && can_edit, is_vis, 0, idx_offset);
                            passed = 1;
                            break;
                    }
                    if ((passed)) { SyncSetUserBold(nd[id_uid], sync_last_type, 1); setTimeout("SyncSetUserBold('" + nd[id_uid] + "', '" + sync_last_type + "', 10);", sync_timeout); }
                }
            }
            
            if (new_data[idx+1]!=old_data[idx+1])
            {
                has_new = 1;
                SyncSetUserBold(id_groupres, sync_last_type, 1); setTimeout("SyncSetUserBold('" + id_groupres + "', '" + sync_last_type + "', 0);", sync_timeout);
                switch (sync_last_type)
                {
                    case type_pw:
                        var pw = $get(id_prefix + id_groupres + "_value");
                        if ((pw))
                        {
                            if (sync_is_gpw)
                            {
                                var slider = document.getElementById(slider_prefix + id_groupres);
                                if ((slider)) SetPairwise(id_groupres, new_data[idx+1],"",1,1);
                            }
                            else
                            {
                                var vis = (!sync_hidejudgments || sync_show2me==sync_step);
                                if (!vis && sync_show_pwside && sync_usekeypads) vis = -1;
                                pw.innerHTML = CreatePairwise(id_groupres, new_data[idx+1], fill_cons, 0, vis);
                            }
                        }
                        break;
                    case type_rating:
                        var v = $get(id_prefix + id_groupres + "_value");
                        var b = $get(id_prefix + id_groupres + "_bar");
                        if ((v) && (b))
                        {
                            var val = new_data[idx+1];
                            var undef = (val<0 || val == value_undef);
                            v.innerHTML = (undef || sync_hidejudgments ? "&nbsp;" : ShowDecNum(val, sync_last_data[5]+2));
                            b.innerHTML = CreateBar((undef || !sync_hidejudgments ? val : 1), (sync_hidejudgments ? fill_disabled : fill_red));
                        }
                        break;
                    case type_direct:
                        SliderDirectEditChange(id_groupres, new_data[idx+1]);
                        break;
                    case type_step:
                        StepFunctionUpdateValue(id_groupres, new_data[idx+1]);
                        break;
                    case type_ruc:
                        RUCUpdateValue(id_groupres, new_data[idx+1]);
                        break;
                }                
            }
            if (new_data[idx+2]!=old_data[idx+2]) { SyncShowConsensus(new_data[idx+2]); res=false; }
            
            if (has_new) res = false;
            if ((idx!=1))
            {
                if ((new_data[idx-1]+"")!=(old_data[idx-1]+"")) res = true;
            }
        }
        if (has_new) { 
            sync_last_update = sync_last_answer; 
            SyncUpdateLastChangeTime(); 
            if (do_resize) onResize(); 
            if (typeof(SyncShowEvalProgress)=="function") { sync_eval_progress = -1; setTimeout('SyncShowEvalProgress(true);', 100); }
        }
        return res;
    }
    
    function SyncShowConsensus(val)
    {
        var d = $get("divConsensus");
        if ((d)) d.innerHTML = ((val!=null && (!sync_hidejudgments || sync_show2me==sync_step || sync_show_variance)) ? (sync_last_type == type_pw ? lbl_consensus_pw : lbl_consensus) + " <b>" + ShowDecNum(val * 100) + "%</b>" : ""); <%--DA1384--%>
        if (sync_last_type == type_pw && !sync_hidejudgments && sync_show2me<0)
        {
            d = $get("divConsensusBar");
            if ((d)) d.innerHTML = CreateBar(val, fill_blue);
        }
    }
    
    function SyncShowPairwise(task, data)
    {
        var s = SyncContent();
        if ((s))
        {
            s.innerHTML = "";
            SyncSetContentAlign(0);
           
            var nodes = data[0];

            var pw = document.createElement("div");
            pw.setAttribute("id", "divPW");
            pw.setAttribute("class", "whole");
            s.appendChild(pw);
            
            var leg = "";
            if (show_legend && (!sync_is_gpw)) 
            {
                var legend = [['<% =JS_SafeString(ResString("lblSyncLegendPWAbbr")) %>', '<% =JS_SafeString(ResString("lblSyncLegendPWMeaning")) %>', '<% =JS_SafeString(ResString("lblSyncLegendKeypads")) %>']];
                for (var i=hints_pw_short.length-1; i>=0; i--) legend.push([hints_pw_short[i], hints_pw_full[i], 2*i+1]);
                if (sync_usekeypads)
                {
                    legend.push(["&nbsp;","<i>" + lbl_erase_judg + "</i>",0]);
                    legend.push(["&nbsp;","<i>" + lbl_invert_judg + "</i>","*"]);
                }
                leg = SyncCreateLegend(legend, 0, 0);
            }
            
            pw.innerHTML = SyncCreateEvaluationScreen("tdPWControl", leg, false, true);
            SyncAddTask(task, $get("tdTask"));
            
            var sInfodocsRow = "";
            var hide_infodocs = (nodes[0][3]==-1) && (nodes[1][3]==-1) && (nodes[2][3]==-1);
            if (sync_infodocsmode==0 && !hide_infodocs)
            {
                var sInfodocsRow = "<tr valign=top><th id='tdInfodocNode1'></th><th id='tdInfodocNode2'></th></tr><tr valign=top><th id='tdInfodocNodeWRT1'></th><th id='tdInfodocNodeWRT2'></th></tr>";
            }
            
            var tdPWControl = $get("tdPWControl");
            tdPWControl.innerHTML = "<table id='tblPWGrid' border=0 cellspacing=1 cellpadding=2 class='scroll'><thead><tr valign=top align=center><th id='tdParentNode' class='pw_caption' style='width:256px; margin:0px; padding:3px 0px 0px 0px;'></th><th valign='top'><table border=0 cellspacing=1 cellpadding=0 style='width:486px'><thead><tr><th width='50%' style='width:50%' id='tdNodeLeft' class='pw_object object_left'></th><th id='tdNodeRight' class='pw_object object_right'></th></tr>" + sInfodocsRow + "</thead></table></th></tr>" + ((sync_is_gpw) ? "" : "<tr><th class='text small'>&nbsp;</th><th align=center><img src='<% =_URL_EVALUATE %>PWScale.aspx?type=tiny' border=0 style='margin-right:14px' width=272 height=17 usemap='#pwmap'></th></tr>") + "</thead><tbody></tbody></table>";

            var tblPWGrid = $get("tblPWGrid");
            var tdParentNode = $get("tdParentNode");
            var tdNodeLeft = $get("tdNodeLeft");
            var tdNodeRight = $get("tdNodeRight");
            //tdParentNode.innerHTML = nodes[0][2];
            tdNodeLeft.innerHTML = nodes[1][2];
            tdNodeRight.innerHTML = nodes[2][2];
            switch (sync_infodocsmode)
            {
                case 0:
                    var also_lst = "'"+(nodes[1][4] ? "a" : "")+nodes[1][0]+"','"+(nodes[2][4] ? "a" : "")+nodes[2][0]+"'";
                    var nodes_lst1 = "'"+ nodes[0][0] + "'";
                    var nodes_lst2 = also_lst;
                    if (nodes[1][4] || nodes[2][4]) also_lst += ",'_"+nodes[1][0]+"','_"+nodes[2][0]+"'";
                    also_lst = "["+ also_lst +"]";
                    if (nodes[0][3]>=0) SyncCreateFramedInfodoc(nodes[0], -1, "tdParentNode", "["+nodes_lst1+"]", "pnode", !nodes[0][3], "[]");
                    if (nodes[1][3]>=0) SyncCreateFramedInfodoc(nodes[1], -1, "tdInfodocNode1", "["+nodes_lst2+"]", "nodes", (!nodes[1][3]&&!nodes[2][3]), also_lst);
                    if (nodes[2][3]>=0) SyncCreateFramedInfodoc(nodes[2], -1, "tdInfodocNode2", "["+nodes_lst2+"]", "nodes", (!nodes[1][3]&&!nodes[2][3]), also_lst);
                    var obj_wrt = (nodes[0][4]==0 && nodes[1][4]==0);
                    if (nodes[1][5]>0 || nodes[1][4]>0 || obj_wrt) SyncCreateFramedInfodoc(nodes[1], nodes[0][0], "tdInfodocNodeWRT1", "['_"+nodes[1][0]+"','_"+nodes[2][0]+"']", "wrt", (!nodes[1][5]&&!nodes[2][5]), also_lst);
                    if (nodes[1][5]>0 || nodes[2][4]>0 || obj_wrt) SyncCreateFramedInfodoc(nodes[2], nodes[0][0], "tdInfodocNodeWRT2", "['_"+nodes[1][0]+"','_"+nodes[2][0]+"']", "wrt", (!nodes[1][5]&&!nodes[2][5]), also_lst);
                    SyncInitFramedInfodocs();
                    break;
                case 1:
                    tdParentNode.innerHTML = ((nodes[0][3]>=0) ? SyncCreatePopupInfodoc(nodes[0], -1) : "") + tdParentNode.innerHTML;
                    tdNodeLeft.innerHTML = ((nodes[1][3]>=0) ? SyncCreatePopupInfodoc(nodes[1], -1) : "") + tdNodeLeft.innerHTML + (nodes[1][4]>0 ? SyncCreatePopupInfodoc(nodes[1], nodes[0][0]) : "");
                    tdNodeRight.innerHTML = ((nodes[2][3]>=0) ? SyncCreatePopupInfodoc(nodes[2], -1) : "") + tdNodeRight.innerHTML + (nodes[2][4]>0 ? SyncCreatePopupInfodoc(nodes[2], nodes[0][0]) : "");
                    break;
            }

            if ((nodes[1][6]) && nodes[1][6]>=0) tdNodeLeft.innerHTML += "<div class='small' style='font-weight:normal; margin-top:1ex;'>" + lbl_knownlikelihood + ShowDecNum(nodes[1][6]) + "</div>";
            if ((nodes[2][6]) && nodes[2][6]>=0) tdNodeRight.innerHTML += "<div class='small' style='font-weight:normal; margin-top:1ex;'>" + lbl_knownlikelihood + ShowDecNum(nodes[2][6]) + "</div>";

            if ((nodes[1][6]) && nodes[1][6]!="") tdNodeLeft.innerHTML += "<div class='small' style='font-weight:normal; margin-top:0px;'>" + nodes[1][6] + "</div>";
            if ((nodes[2][6]) && nodes[2][6]!="") tdNodeRight.innerHTML += "<div class='small' style='font-weight:normal; margin-top:0px;'>" + nodes[2][6] + "</div>";
            
//            tdParentNode.innerHTML = "<div >" + tdParentNode.innerHTML + "</div>";

            sliders = [];
            var tblPWGrid = $get("tblPWGrid");
            var tmp_users = data[1];

            sync_user_row_starts = tblPWGrid.clientHeight;

            data[1] = ReorderUsers(data[1], sync_user, 1, false);
            for (var i=0; i<data[1].length; i++)
             {
                var user = data[1][i];
                var restr = (user[id_keypad]==-2);
                var act = isActiveUser(user[id_email]);
                var can_edit = (sync_owner || act) && (user[id_keypad]!=-1);
                var is_vis = (!restr && (!sync_hidejudgments  || sync_show2me==sync_step || act) ? 1 : 0);
                if (!is_vis && sync_show_pwside && user[id_keypad]>0 && sync_usekeypads) is_vis = -1;
                if ((restr)) is_vis = -1;
                SyncAddPWGridRow(tblPWGrid, CreateUserName(user, can_edit, i+1),  user[id_uid], user[id_value], !restr && can_edit, is_vis, 0, 0);
             }   
            data[1] = tmp_users;
            is_vis = (!sync_hidejudgments || sync_show2me==sync_step);
            if (!is_vis && sync_show_pwside && sync_usekeypads) is_vis = -1;
            AddUsersPageLine(tblPWGrid, 2);
            SyncAddPWGridRow(tblPWGrid, lbl_groupres, id_groupres, data[2], 0, is_vis, 1, 0);
            var c = "<table border=0 cellspacing=0 cellpadding=0><tr><td class='text' style='text-align:center;' id='divConsensus' title='<%=ResString("tooltipVariance")%>'></td><td id='divConsensusBar' style='padding-left:1em'></td></tr></table>"; <%--DA1384--%>
            tdPWControl.innerHTML = "<table border=0 cellspacing=0 cellpadding=0><tr><td valign=top align=center id='tblPWControl'>" + tdPWControl.innerHTML + "</td></tr><tr><td align=center style='height:1em'>" + c + "</td></tr></table>";
            SyncShowConsensus(data[3]);
        }
    }

    function SyncShowUsersByID(lst, val_name, undef) {
        if ((lst)) {
            var names = "";
            var l = lst.length;
            if (l>11) l =10;
            for (var i=0; i<l; i++) {
                var usr = GetDataByID(lst[i]);
                if ((usr)) names += "<li>" + (usr[id_name]=="" ? usr[id_email] : usr[id_name]) + "</li>";
            }
            if (names!="") {
                if (lst.length-l>0) names += "<br>+" + (lst.length-l) + " users more";
                var ic = jDialog_show_icon;
                jDialog_show_icon = false;
                jDialog("<b>" + replaceString("{0}", val_name, (undef ? lbl_ulist_undef : lbl_ulist_judg)) + "</b>:<ul type='square' style='margin-left:0px;'>" + names + "</ul>", false, ";", "", "", 380, -1);
                jDialog_show_icon = ic;
            }
        }
        return false;
    }
    
    function SyncShowRating(task, data)
    {
        var s = SyncContent();
        if ((s))
        {
            s.innerHTML = "";
            SyncSetContentAlign(0);
           
            var r = document.createElement("div");
            r.setAttribute("id", "divRatings");
            r.setAttribute("class", "whole");
            s.appendChild(r);
            
            var rlist = data[1].slice(0);
            
            var act_user = null;
            for (var i=0; i<data[2].length; i++)
            {
                var user = data[2][i];
                if (isActiveUser(user[id_email])) act_user = user;
            }   

            var hide_prty = data[6][3];

            var legend = [['<% =JS_SafeString(ResString("lblSyncLegendRSName")) %>', '<% =JS_SafeString(ResString("lblSyncLegendRSValue")) %>', '<% =JS_SafeString(ResString("lblSyncLegendKeypads")) %>']];
            var legend_content = "";
            var scale_node = ["scale" + data[6][0], data[6][1], "Scale description", data[6][2], 0, data[6][2]];
            if (show_legend && !sync_distr_view) 
            {
                var show_active_legend = ((act_user!=null) && (act_user[id_keypad]!=-2) && (sync_ratings_active_legend) && (!sync_hideowner || !sync_owner) && act_user[id_value]!=null);
                
                for (var i=0; i<rlist.length; i++) 
                {
//                    if (rlist[i][0]>=0) legend.push([rlist[i][1], Math.round(100*rlist[i][2])+"%", rlist.length-i-1]);
//                    if (rlist[i][0]>=0) legend.push([rlist[i][1], ShowDecNum(100*rlist[i][2], data[5])+"%", rlist.length-i-1, rlist[i][0], rlist[i][2]]);
                    var v = rlist[i][3];
                    if (v == null || v == "") v = ShowDecNum(100*rlist[i][2], data[5])+"%";
//                    if (rlist[i][0]>=0) legend.push([rlist[i][1], v, (rlist.length-i<=10 ? rlist.length-i-1 : ""), rlist[i][0], rlist[i][2]]);
                    if (rlist[i][0]>=0) legend.push([rlist[i][1], v, (rlist.length-i<=10 ? rlist.length-i-1 : ""), rlist[i]]);
                }
                if (sync_usekeypads || show_active_legend) legend.push(["<i>" + (show_active_legend ? lbl_undefined : lbl_erase_judg) + "</i>", "&nbsp;", 0, [value_undef]]);

                legend_content = (sync_infodocsmode == 1 ? SyncCreatePopupInfodoc(scale_node, -1, <% =CInt(reObjectType.MeasureScale) %>)+ "<% =JS_SafeString(ResString("lblShowDescriptionScale")) %><br>" : "") + "<br>" + (show_active_legend ? SyncCreateIntensitiesList(legend, act_user[id_value], act_user[id_uid], hide_prty) : SyncCreateLegend(legend, 1, hide_prty)) ;

            }

            r.innerHTML = SyncCreateEvaluationScreen("tdRatingsControl", legend_content, false, true);

            var tdRatingsControl = $get("tdRatingsControl");

            var nodes = data[0];
            var p_node = nodes[0][2];
            var alt_node = nodes[1][2];

            msg("Render grid with Ratings");

            var sInfodocs = "";
            switch (sync_infodocsmode)
            {
                case 0:
                    sInfodocs = "<table border=0 cellspacing=1 cellpadding=0><tr valign=top><td id='tdAlt'></td><td id='tdParent'></td><td id='tdWRT'></td></tr></table>";
                    break;
                case 1:
                    p_node = SyncCreatePopupInfodoc(nodes[0], -1) + p_node;
                    alt_node = SyncCreatePopupInfodoc(nodes[1], -1) +  alt_node + SyncCreatePopupInfodoc(nodes[1], nodes[0][0]);
                    break;    
            }

            sCaption = replaceString("%%nodeB%%", p_node, replaceString("%%nodeA%%", alt_node, task));
            SyncAddTask(sCaption, $get("tdTask"));

            if ((sync_distr_view)) {
//                tdRatingsControl.innerHTML =  (sInfodocs=="" ? "" : sInfodocs ) + "<h6 style='margin:4px;padding:0px;'>" + lbl_distr_view + "</h6><table id='tblRatingsGrid' border=0 cellspacing=1 cellpadding=1><tbody><tr class='grid_header text actions'><td>"+lbl_rating+"</td><td style='width:10em'>"+lbl_priority+"</td><td>"+lbl_count+"</td><td>&nbsp;</td></tr></tbody></table>";
                tdRatingsControl.innerHTML =  "<h6 style='margin:4px;padding:0px;'>" + lbl_distr_view + "</h6><table id='tblRatingsGrid' border=0 cellspacing=1 cellpadding=1><tbody><tr class='grid_header text actions'><td>"+lbl_rating+"</td>" + (hide_prty ? "" : "<td style='width:10em'>"+lbl_priority+"</td>") + "<td>"+lbl_count+"</td><td>&nbsp;</td></tr></tbody></table>";

//                if (sync_infodocsmode==0)
//                {
//                    var cookie_name = "nodes";
//                    var collapses = "['"+(nodes[0][4] ? "a" : "")+nodes[0][0]+"','"+(nodes[1][4] ? "a" : "")+nodes[1][0]+"','_"+nodes[1][0]+"']";
//                    var def_hide = !nodes[0][3] && !nodes[1][3] && !nodes[1][5];
//                    SyncCreateFramedInfodoc(nodes[0], -1, "tdParent", collapses, cookie_name, def_hide);
//                    SyncCreateFramedInfodoc(nodes[1], -1, "tdAlt", collapses, cookie_name, def_hide);
//                    SyncCreateFramedInfodoc(nodes[1], nodes[0][0], "tdWRT", collapses, cookie_name, def_hide);
//                    SyncInitFramedInfodocs();
//                }

                var tblRatingsGrid = $get("tblRatingsGrid");

                var lst = new Array().concat(rlist.slice(0));
//                lst.push([-1,lbl_undefined, value_undef]);

                for (var i=0; i<lst.length; i++) {
                    lst[i][5]=0;
                    lst[i][6]="";
                    for (var j=0; j<data[2].length; j++) {
                        var user = data[2][j];
                        if (user[id_value]==lst[i][0] || (user[id_value]<0 && lst[i][0]<0)) {
                            lst[i][6] += ((lst[i][5]) ? "," : "") + user[id_uid];
                            lst[i][5]++;
                        }
                    }

                    var undef = (lst[i][0]<0);

                    var r = tblRatingsGrid.rows.length;
                    tblRatingsGrid.insertRow(r);
                    tblRatingsGrid.rows[r].insertCell(0);
                    tblRatingsGrid.rows[r].insertCell(1);
                    tblRatingsGrid.rows[r].insertCell(2);
                    tblRatingsGrid.rows[r].insertCell(3);
                    tblRatingsGrid.rows[r].className = "text " + (undef ? "grid_row_sel" : ((r&1) ? "grid_row" : "grid_row_alt"));
    //                tblRatingsGrid.rows[r].style.height = "24px";
                    tblRatingsGrid.rows[r].setAttribute("valign" , "middle");
                    tblRatingsGrid.rows[r].cells[0].innerHTML = lst[i][1];
                    var idx = (hide_prty ? -1 : 0);
                    if (!hide_prty) {
                        tblRatingsGrid.rows[r].cells[1].setAttribute("align" , "right");
                        tblRatingsGrid.rows[r].cells[1].innerHTML = (undef ? "&nbsp;" : ShowDecNum(100*lst[i][2]) + "%&nbsp;");
                    }
                    tblRatingsGrid.rows[r].cells[2+idx].setAttribute("align" , "center");
                    tblRatingsGrid.rows[r].cells[2+idx].innerHTML = (lst[i][6] !="" ? "<a href='' onclick='return SyncShowUsersByID([" + lst[i][6] + "], \"" + lst[i][1] +"\", " + (undef ? 1 : 0) + ");' class='action dashed'>" + lst[i][5] + "</a>" : "0");
                    //tblRatingsGrid.rows[r].cells[3+idx].innerHTML = (data[2].length>0 ? CreateBar((lst[i][5]/data[2].length), (undef ? fill_red : fill_blue)) : "&nbsp;");
                    tblRatingsGrid.rows[r].cells[3+idx].innerHTML = (data[2].length>0 ? CreateBar((lst[i][5]/data[2].length), fill_blue) : "&nbsp;");
                }

            }
            else 
            {
                tdRatingsControl.innerHTML = "<table id='tblRatingsGrid' border=0 cellspacing=1 cellpadding=1><tbody>" +  (sInfodocs=="" ? "" : "<tr><td colspan=4 style='text-align:center; margin-bottom:1ex'>" + sInfodocs + "</td></tr>") + "<tr class='grid_header text actions'><td>"+lbl_username+"</td><td style='width:14em'>"+lbl_rating+"</td>" + (hide_prty ? "" : "<td>"+lbl_priority+"</td><td>&nbsp;</td>") + "</tr></tbody></table>";
            
                if (sync_infodocsmode==0)
                {
                    var cookie_name = "nodes";
                    var collapses = "['"+(nodes[0][4] ? "a" : "")+nodes[0][0]+"','"+(nodes[1][4] ? "a" : "")+nodes[1][0]+"','_"+nodes[1][0]+"']";
                    var def_hide = !nodes[0][3] && !nodes[1][3] && !nodes[1][5];
                    SyncCreateFramedInfodoc(nodes[0], -1, "tdParent", collapses, cookie_name, def_hide);
                    SyncCreateFramedInfodoc(nodes[1], -1, "tdAlt", collapses, cookie_name, def_hide);
                    SyncCreateFramedInfodoc(nodes[1], nodes[0][0], "tdWRT", collapses, cookie_name, def_hide);
                    SyncCreateFramedInfodoc(scale_node, -1, "divLegend", "['" + scale_node[0] + "']", "scale", !(scale_node[3]), [], <% =CInt(reObjectType.MeasureScale) %>);
                    SyncInitFramedInfodocs();
                }

                var tblRatingsGrid = $get("tblRatingsGrid");
                var list_full = new Array().concat(rlist);

                sync_user_row_starts = tblRatingsGrid.clientHeight;

                var tmp_users = data[2];
                data[2] = ReorderUsers(data[2], sync_user, 1, false);
                for (var i=0; i<data[2].length; i++)
                {
                    var user = data[2][i];
                    var restr = (user[id_keypad]==-2);
                    var act = isActiveUser(user[id_email]);
                    var can_edit = (sync_owner || act) && (user[id_keypad]!=-1);
                    SyncAddRatingsGridRow(tblRatingsGrid, CreateUserName(user, can_edit, i+1), list_full, user[id_uid], user[id_value], !restr && can_edit, !restr && (!sync_hidejudgments || sync_show2me==sync_step || act), 0, 0, hide_prty);
                }    
                data[2] = tmp_users;
                AddUsersPageLine(tblRatingsGrid, 4);
                SyncAddRatingsGridRow(tblRatingsGrid, lbl_groupres, null, id_groupres, data[3], 0, (!sync_hidejudgments || sync_show2me==sync_step ), 1, 0, hide_prty);
            }

            tdRatingsControl.innerHTML += "<p align=center><table border=0 cellspacing=0 cellpadding=0><tr><td class='text' style='text-align:center;' id='divConsensus'></td></tr></table>";
            SyncShowConsensus(data[4]);
        }
    }
    
    function SyncShowDirect(task, data)
    {
        var s = SyncContent();
        if ((s))
        {
            s.innerHTML = "";
            SyncSetContentAlign(0);
           
            var r = document.createElement("div");
            r.setAttribute("id", "divDirect");
            r.setAttribute("class", "whole");
            s.appendChild(r);
            
            r.innerHTML = SyncCreateEvaluationScreen("tdDirectControl", "", false, true);

            var tdDirectControl = $get("tdDirectControl");

            var nodes = data[0];
            var p_node = nodes[0][2];
            var alt_node = nodes[1][2];

            var sInfodocs = "";
            switch (sync_infodocsmode)
            {
                case 0:
                    sInfodocs = "<table border=0 cellspacing=1 cellpadding=0 style='width:100%'><tr valign=top><td id='tdAlt'></td><td id='tdParent'></td><td id='tdWRT'></td></tr></table>";
                    break;
                case 1:
                    p_node = SyncCreatePopupInfodoc(nodes[0], -1) + p_node;
                    alt_node = SyncCreatePopupInfodoc(nodes[1], -1) +  alt_node + SyncCreatePopupInfodoc(nodes[1], nodes[0][0]);
                    break;    
            }

            sCaption = replaceString("%%nodeA%%", p_node, replaceString("%%nodename%%", alt_node, task));
            SyncAddTask(sCaption, $get("tdTask"));

            tdDirectControl.innerHTML = "<table id='tblDirectGrid' border=0 cellspacing=1 cellpadding=1><tbody>" +  (sInfodocs=="" ? "" : "<tr><td colspan=4 style='text-align:center; margin-bottom:1ex'>" + sInfodocs + "</td></tr>") + "<tr class='grid_header text actions'><td style='width:100px'>"+lbl_username+"</td><td>"+lbl_priority+"</td><td style='width:150px'>&nbsp;</td></tr></tbody></table>";
            
            if (sync_infodocsmode==0)
            {
                var cookie_name = "nodes";
                var collapses = "['"+(nodes[0][4] ? "a" : "")+nodes[0][0]+"','"+(nodes[1][4] ? "a" : "")+nodes[1][0]+"','_"+nodes[1][0]+"']";
                var def_hide = !nodes[0][3] && !nodes[1][3] && !nodes[1][5];
                SyncCreateFramedInfodoc(nodes[0], -1, "tdParent", collapses, cookie_name, def_hide);
                SyncCreateFramedInfodoc(nodes[1], -1, "tdAlt", collapses, cookie_name, def_hide);
                SyncCreateFramedInfodoc(nodes[1], nodes[0][0], "tdWRT", collapses, cookie_name, def_hide);
                SyncInitFramedInfodocs();
            }

            var tblDirectGrid = $get("tblDirectGrid");
            var tmp_users = data[1];
            sync_user_row_starts = tblDirectGrid.clientHeight;
            data[1] = ReorderUsers(data[1], sync_user, 1, false);
            for (var i=0; i<data[1].length; i++)
            {
                var user = data[1][i];
                var restr = (user[id_keypad]==-2);
                var act = isActiveUser(user[id_email]);
                var can_edit = (sync_owner || act) && (user[id_keypad]!=-1);
                var is_vis = (!restr && (!sync_hidejudgments  || sync_show2me==sync_step || act) ? 1 : 0);
                if (!is_vis && sync_show_pwside && user[id_keypad]>0 && sync_usekeypads) is_vis = -1;
                if ((restr)) is_vis = -1;
                SyncAddDirectGridRow(tblDirectGrid, CreateUserName(user, can_edit, i+1), user[id_uid], user[id_value], !restr && can_edit, is_vis, 0, 0);
            }
            data[1] = tmp_users;
            AddUsersPageLine(tblDirectGrid, 3);
            SyncAddDirectGridRow(tblDirectGrid, lbl_groupres, id_groupres, data[2], 0, !sync_hidejudgments || sync_show2me==sync_step, 1, 0);
            tdDirectControl.innerHTML += "<p align=center><table border=0 cellspacing=0 cellpadding=0><tr><td class='text' style='text-align:center;' id='divConsensus'></td></tr></table>";
            SyncShowConsensus(data[3]);
        }
    }

    function GetStepValue(val)
    {
        var res = "";   //"&#8212;";
        if ((val!=null) && (typeof val!="undefined") && (val!=="") && !isUndefinedByID(val, type_step))
        {
            val = (""+val).replace(",", ".");
            if (val=="-") val="0";
            if (val=="0.") val="0";
            if (val=="1.") val="1";
            val = val * 1;
            var steps = sync_last_data[1];
            var l = steps.length;
            if (l>0)
            {
                var min = -1;
                var max_prty = steps[l-1];
                for (var i=0; i<l; i++)
                {
                    var s = steps[i];
                    if (val>=s[2]) min = i;
                }    
                if (min==-1) 
                {
                    val = steps[0][4];
                }
                else
                {
                    if (val>=max_prty[3]) 
                    {
                        val = max_prty[4];
                    }
                    else
                    {
                        var s = steps[min];
                        if (sync_last_data[5]==1 && min<l-1)
                         {
                            var a = steps[min];
                            var b = steps[min+1];
                            
                            var k = (b[4]-a[4])/(b[2]-a[2]);
                            var c = a[4]-k*a[2];
                            val = (k*val+c);
                         }   
                        else
                         {
                            val = s[4];
                         } 
                    }    
                }     
            }    
            res = val;
        }
        return res;
    }

    function SyncShowStepFunction(task, data)
    {
        var s = SyncContent();
        if ((s))
        {
            s.innerHTML = "";
            SyncSetContentAlign(0);
           
            var r = document.createElement("div");
            r.setAttribute("id", "divStepFunction");
            r.setAttribute("class", "whole");
            s.appendChild(r);

            var nodes = data[0];
            var p_node = nodes[0][2];
            var alt_node = nodes[1][2];
            var scale_node = ["scale", data[6], "Scale description", data[7], 0, 0, ""];

            var legend = "";
            if (show_legend)
            {
              var o = data[1];
              var d = "cnt=" + o.length;
              for (var j=0; j<o.length; j++)
              {
                  if (j==o.length-1 && o[j][3] == 0 && o[j][2]>0) o[j][3] = "<% =Integer.MaxValue %>";
                  d += "&l" + j + "=" + encodeURIComponent(o[j][2]) + "&h" + j + "=" + encodeURIComponent(o[j][3]) + "&v" + j + "=" + encodeURIComponent(o[j][4]);
              }
              if (sync_infodocsmode == 1) legend += SyncCreatePopupInfodoc(scale_node, -1, <% =CInt(reObjectType.MeasureScale) %>);
              legend += "<iframe scolling=no' id='frmScale' src='' frameborder='0' allowtransparency='true' src='' style='border:0px; width:330px; height:270px;'></iframe>";
            }

            r.innerHTML = SyncCreateEvaluationScreen("tdStepFunctionControl", legend, false, true);
            if (legend!="") SyncLoadFrame('frmScale', "<% =_URL_EVALUATE %>UC_Chart.aspx?type=sf&linear=" + data[5] + "&" + d);

            var tdSFControl = $get("tdStepFunctionControl");

            var sInfodocs = "";
            switch (sync_infodocsmode)
            {
                case 0:
                    sInfodocs = "<table border=0 cellspacing=1 cellpadding=0 style='width:100%'><tr valign=top><td id='tdAlt'></td><td id='tdParent'></td><td id='tdWRT'></td></tr></table>";
                    break;
                case 1:
                    p_node = SyncCreatePopupInfodoc(nodes[0], -1) + p_node;
                    alt_node = SyncCreatePopupInfodoc(nodes[1], -1) +  alt_node + SyncCreatePopupInfodoc(nodes[1], nodes[0][0]);
                    break;    
            }

            sCaption = replaceString("%%nodename%%", alt_node, replaceString("%%nodeA%%", p_node, task));
            SyncAddTask(sCaption, $get("tdTask"));

            tdSFControl.innerHTML = "<table id='tblStepFunctionGrid' border=0 cellspacing=1 cellpadding=1><tbody>" + (sInfodocs=="" ? "" : "<tr><td colspan=4><div style='text-align:center; margin-bottom:1ex'>" + sInfodocs + "</td></tr>") + "<tr class='grid_header text actions'><td>"+lbl_username+"</td><td>"+lbl_value+"</td><td>"+lbl_priority+"</td><td>&nbsp;</td></tr></tbody></table>";
            
            if (sync_infodocsmode==0)
            {
                var cookie_name = "nodes";
                var collapses = "['"+(nodes[0][4] ? "a" : "")+nodes[0][0]+"','"+(nodes[1][4] ? "a" : "")+nodes[1][0]+"','_"+nodes[1][0]+"']";
                var def_hide = !nodes[0][3] && !nodes[1][3] && !nodes[1][5];
                SyncCreateFramedInfodoc(nodes[0], -1, "tdParent", collapses, cookie_name, def_hide);
                SyncCreateFramedInfodoc(nodes[1], -1, "tdAlt", collapses, cookie_name, def_hide);
                SyncCreateFramedInfodoc(nodes[1], nodes[0][0], "tdWRT", collapses, cookie_name, def_hide);
                SyncCreateFramedInfodoc(scale_node, -1, "tdLegend", "['" + scale_node[0] + "']", "scale", !(scale_node[3]), [], <% =CInt(reObjectType.MeasureScale) %>);
                SyncInitFramedInfodocs();
            }

            var tblSFGrid = $get("tblStepFunctionGrid");
            var tmp_users = data[2];
            sync_user_row_starts = tblSFGrid.clientHeight;
            data[2] = ReorderUsers(data[2], sync_user, 1, false);
            for (var i=0; i<data[2].length; i++)
            {
                var user = data[2][i];
                var restr = (user[id_keypad]==-2);
                var act = isActiveUser(user[id_email]);
                var can_edit = (sync_owner || act) && (user[id_keypad]!=-1);
                SyncAddStepFunctionGridRow(tblSFGrid, CreateUserName(user, can_edit, i+1), data[1], user[id_uid], user[id_value], !restr && can_edit, !restr && (!sync_hidejudgments || sync_show2me==sync_step || act), 0, 0);
            }    
            data[2] = tmp_users;
            AddUsersPageLine(tblSFGrid, 4);
            SyncAddStepFunctionGridRow(tblSFGrid, lbl_groupres, null, id_groupres, data[3], 0, !sync_hidejudgments || sync_show2me==sync_step, 1, 0);
            tdSFControl.innerHTML += "<p align=center><table border=0 cellspacing=0 cellpadding=0><tr><td class='text' style='text-align:center;' id='divConsensus'></td></tr></table>";
            SyncShowConsensus(data[4]);
        }
    }
    
    function GetRUCValue(val)
    {
        var res = "";   //"&#8212;";
        if ((val!=null) && (typeof val!="undefined") && (val!=="") && !isUndefinedByID(val, type_ruc))
        {
            val = (""+val).replace(",", ".");
            if (val=="-") val="0";
            if (val=="0.") val="0";
            if (val=="1.") val="1";
            val = val * 1;
            var opt = sync_last_data[1];

            DebugMsg(sync_last_data[1]);
            var low = opt[0];
            var high = opt[1];
            var curv = opt[2];
//            var is_linear = opt[3];
//            var is_incr = opt[4];

            if (low>=high)
            {
                res = 0;
            }
            else
            {
                var l = (high-low);
                var n = l*curv;
                if ((opt[4])) // isIncreasing
                {
                    if (val<low) res = 0;
                    if (val>high) res = 1;
                    if (val>=low && val<=high) res = (n==0 ? (val-low)/l : (1-Math.exp(-(val-low)/n))/(1-Math.exp(-l/n))); 
                }
                else
                {
                    if (val<low) res = 1;
                    if (val>high) res = 0;
                    if (val>=low && val<=high) res = (n==0 ? (high-val)/l : (1-Math.exp(-(high-val)/n))/(1-Math.exp(-l/n)));
                }
            }
        }
        return res;
    }

    function SyncShowRegularUtilityCurve(task, data)
    {
        var s = SyncContent();
        if ((s))
        {
            s.innerHTML = "";
            SyncSetContentAlign(0);
           
            var r = document.createElement("div");
            r.setAttribute("id", "divRUC");
            r.setAttribute("class", "whole");
            s.appendChild(r);

            var nodes = data[0];
            var p_node = nodes[0][2];
            var alt_node = nodes[1][2];
            var scale_node = ["scale", data[1][5], "Scale description", data[1][6], 0, 0, ""];

            var legend = "";
            if (show_legend)
            {
              var opt = data[1];
              if (sync_infodocsmode == 1) legend += SyncCreatePopupInfodoc(scale_node, -1, <% =CInt(reObjectType.MeasureScale) %>);
              legend += "<iframe scolling=no' src='<% =_URL_EVALUATE %>UC_Chart.aspx?type=uc&low=" + opt[0] + "&high=" + opt[1] + "&curv=" + opt[2] + "&linear=" + opt[3] + "&decr=" + ((opt[4]) ? "false" : "true") + "' frameborder='0' allowtransparency='true' class='frm_loading' onload='InfodocFrameLoaded(this);' style='border:0px; width:330px; height:270px;'></iframe>";
            }

            r.innerHTML = SyncCreateEvaluationScreen("tdRUCControl", legend, false, true);

            var tdUCControl = $get("tdRUCControl");

            var sInfodocs = "";
            switch (sync_infodocsmode)
            {
                case 0:
                    sInfodocs = "<table border=0 cellspacing=1 cellpadding=0 style='width:100%'><tr valign=top><td id='tdAlt'></td><td id='tdParent'></td><td id='tdWRT'></td></tr></table>";
                    break;
                case 1:
                    p_node = SyncCreatePopupInfodoc(nodes[0], -1) + p_node;
                    alt_node = SyncCreatePopupInfodoc(nodes[1], -1) +  alt_node + SyncCreatePopupInfodoc(nodes[1], nodes[0][0]);
                    break;    
            }

            sCaption = replaceString("%%nodename%%", alt_node, replaceString("%%nodeA%%", p_node, task));
            SyncAddTask(sCaption, $get("tdTask"));

            tdUCControl.innerHTML = "<table id='tblRUCGrid' border=0 cellspacing=1 cellpadding=1><tbody>" + (sInfodocs=="" ? "" : "<tr><td colspan=4><div style='text-align:center; margin-bottom:1ex'>" + sInfodocs + "</td></tr>") + "<tr class='grid_header text actions'><td>"+lbl_username+"</td><td>"+lbl_value+"</td><td>"+lbl_priority+"</td><td>&nbsp;</td></tr></tbody></table>";
            
            if (sync_infodocsmode==0)
            {
                var cookie_name = "nodes";
                var collapses = "['"+(nodes[0][4] ? "a" : "")+nodes[0][0]+"','"+(nodes[1][4] ? "a" : "")+nodes[1][0]+"','_"+nodes[1][0]+"']";
                var def_hide = !nodes[0][3] && !nodes[1][3] && !nodes[1][5];
                SyncCreateFramedInfodoc(nodes[0], -1, "tdParent", collapses, cookie_name, def_hide);
                SyncCreateFramedInfodoc(nodes[1], -1, "tdAlt", collapses, cookie_name, def_hide);
                SyncCreateFramedInfodoc(nodes[1], nodes[0][0], "tdWRT", collapses, cookie_name, def_hide);
                SyncCreateFramedInfodoc(scale_node, -1, "tdLegend", "['" + scale_node[0] + "']", "scale", !(scale_node[3]), [], <% =CInt(reObjectType.MeasureScale) %>);
                SyncInitFramedInfodocs();
            }

            var tblRUCGrid = $get("tblRUCGrid");
            var tmp_users = data[2];
            sync_user_row_starts = tblRUCGrid.clientHeight;
            data[2] = ReorderUsers(data[2], sync_user, 1, false);
            for (var i=0; i<data[2].length; i++)
            {
                var user = data[2][i];
                var restr = (user[id_keypad]==-2);
                var act = isActiveUser(user[id_email]);
                var can_edit = (sync_owner || act) && (user[id_keypad]!=-1);
                SyncAddRegularUtilityCurveGridRow(tblRUCGrid, CreateUserName(user, can_edit, i+1), data[1], user[id_uid], user[id_value], !restr && can_edit, !restr && (!sync_hidejudgments || sync_show2me==sync_step || act), 0, 0);
            }    
            data[2] = tmp_users;
            AddUsersPageLine(tblRUCGrid, 4);
            SyncAddRegularUtilityCurveGridRow(tblRUCGrid, lbl_groupres, null, id_groupres, data[3], 0, !sync_hidejudgments || sync_show2me==sync_step, 1, 0);
            tdRUCControl.innerHTML += "<p align=center><table border=0 cellspacing=0 cellpadding=0><tr><td class='text' style='text-align:center;' id='divConsensus'></td></tr></table>";
            SyncShowConsensus(data[4]);
        }
    }

    // get item from array, where items are subarrays with uid as first item [[uid1, val1-1, val1-2, …], [uid2, val2-1, val2-2, …]…]
    function GetRowByIdxKey(data, idx, key)
    {
        if (data!=null && typeof data!="undefined")
        {
            key = (key+"").toLowerCase();
            for (var i=0; i<data.length; i++)
            {
                if ((data[i][idx] + "").toLowerCase()==key) { return data[i]; }
            }    
        }       
        return null;    
    }
    
    function CreateResultSortColumn(id, text)
    {
        var act = (Math.abs(sync_results_sorting)==id);
        return "<a href='' class='actions' onclick='return DoSort(" + (act ? -sync_results_sorting : id*(id<=2 ? 1 : -1)) + ");'>" + text + (act ? (sync_results_sorting<0 ? lbl_desc : lbl_asc) : "") + "</a>";
    }
    
    function DoSort(srt)
    {
        sync_results_sorting = srt;
        SyncShowResultsGrid(sync_last_data);
        return false;
    }
    
    function SortResults(a, b)
    {
        var res = 0;
        var idx = Math.abs(sync_results_sorting);
        var x = a[idx-1];
        var y = b[idx-1];
        if (idx==2) { x=x.toLowerCase(); y=y.toLowerCase(); }
        res = ((x==y) ? 0 : ((x<y) ? -1 : 1));
        if (sync_results_sorting<0) res=-res;
        return res;
    }
    
    // return missing results: 0 when all is fine, 1 — no individual, >1 when no combined;
    function SyncShowResultsGrid(data)
    {
        var has_indiv = 0;
        var has_comb = 0;
        var results_options = data[(sync_last_type == type_g_res ? 4 : 3)][0];
        var res_offset = (sync_last_type == type_g_res ? 1 : 0);
        var res_modes = [[-1,2],[1,0]];  // indiv, comb
        var user = GetRowByIdxKey(data[1], 1, sync_user);
        if ((data[2]))
        {
            var uid = ((user) ? user[0] : -999);
            var rows = [];
            var max_value = 0.0000001;
            for (var i=0; i<data[2].length; i++)
            {
                var r = data[2][i];
                var data_invid = GetRowByIdxKey(r[2], 0, uid);
                var data_comb = GetRowByIdxKey(r[2], 0, uid_combined);
                var ind = ((data_invid) ? data_invid[1 + sync_results_norm + res_offset] : -1);
                var comb = ((data_comb) ? data_comb[1 + sync_results_norm + res_offset] : -1);
                if (i==0)
                {
                    if ((data_invid) && (ind>=0)) has_indiv = 1;
                    if ((data_comb) && (comb>=0)) has_comb = 1;
                }
                if ((data_invid) && (ind>max_value)) max_value = ind;
                if ((data_comb) && (comb>max_value)) max_value = comb;

                rows.push([r[0], r[1], ((data_invid) ? ind : -1), ((data_comb) ? comb : -1)]);
            }
           
            if (results_options==0 && !has_indiv) results_options = 2;

            var results = rows.sort(SortResults);
            var caption = data[0];
            if (sync_last_type == type_g_res)
            {
                var node = GetRowByIdxKey(data[3], 0, data[0]);
                if ((node)) caption = node[2];
            }

            var norm_options = "";
            <% If Not App.isRiskEnabled Then %>
            var is_cov = (sync_last_type == type_g_res || data[4]);
            if ((is_cov)) {
                norm_options = "<select name='norm_option' onchange='SyncChangeNorm(this.value);'>";
                for (var j=0; j<norm_names.length; j++)
                    norm_options += "<option value='" + j + "'" + (j == sync_results_norm ? " selected" : "") + ">" + norm_names[j] + "</option>";
                norm_options += "</select>";
            }<% End If %>

            var ResultsGrid = $get("ResultsGrid");
            ResultsGrid.innerHTML = "<table id='tblResultsGrid' border=0 cellspacing=1 cellpadding=1><tbody><tr><td colspan=" + (results_options ? 4 : 5) + "><div style='text-align:center; margin-bottom:1ex'><b>" + caption + "</b><div style='text-align:right; margin-top:4px;' class='text'>" + norm_options + "</div></td></tr><tr class='grid_header text actions'><td style='width:3em'>"+CreateResultSortColumn(1, lbl_num)+"</td><td>"+CreateResultSortColumn(2, lbl_nodename)+"</td>" + (results_options==2 ? "" : "<td style='width:6em'>"+CreateResultSortColumn(3, "<% =JS_SafeString(CStr(Iif(PM.User.UserName="", PM.User.UserEmail, PM.User.UserName))) %>")+"</td>") + (results_options==1 ? "" : "<td style='width:6em'>"+CreateResultSortColumn(4, lbl_combined)+"</td>") + "<td style='width:" + (bar_width+4*bar_padding) +"'>"+lbl_bar_result+"</td></tr></tbody></table>";
            var tblResultsGrid = $get("tblResultsGrid");
            for (var i=0; i<results.length; i++) SyncAddResultsGridRow(tblResultsGrid, results[i], results_options, max_value);
        }
        onResize();
        return 2*(!has_comb)+(!has_indiv);
    }

    function SyncChangeNorm(val) {
        val = val*1;
        if (val>=0 && val<norm_names.length) {
            sync_results_norm = val;
            <% If Not App.isRiskEnabled Then %>document.cookie = "<% =_COOKIE_TT_NORM %>=" + val;<% End If %>
            SyncShowResultsGrid(sync_last_data);
        }
    }

    function Sync_SAGetSubnodesDroplist(nodes, pid, active, margin)
    {
        var lst = "";
        for (var i=0; i<nodes.length; i++)
          if (nodes[i][1]==pid)
          {
            var n = nodes[i];
            var sublist = Sync_SAGetSubnodesDroplist(nodes, n[0], active, margin+"&nbsp;&nbsp;");
            lst += "<option value='" + n[0] + "'" + (sublist == "" ? " disabled" : "") + (n[0]==active ? " selected" : "") + ">" + margin + ShortString(n[2], 50, false) +"</option>" + sublist;
          }
        return lst;
    }
    
    function SyncShowDSA(task, data)
    {
        var s = SyncContent();
        if ((s))
        {
            s.innerHTML = "";
            SyncSetContentAlign(0);
           
            var dsa = document.createElement("div");
            dsa.setAttribute("id", "divDSA");
            dsa.setAttribute("class", "whole");
            s.appendChild(dsa);
            
            dsa.innerHTML = SyncCreateEvaluationScreen("tdDSAControl", "", false, true);
            SyncAddTask(task, $get("tdTask"));
            
            var show = data[0];
            
            var tdDSAControl = $get("tdDSAControl");
            if (show)
            {
                var wrt = data[1]*1;
                var a = data[2];

                var n_all = data[3];
                var n = [];
                var t = 0;
                for (var i=0; i<n_all.length; i++)
                {
                    if (n_all[i][2].length>sa_node_maxlen) n_all[i][2] = n_all[i][2].substr(0,sa_node_maxlen-1) + "…";
                    if (n_all[i][1]==wrt) n.push(n_all[i]);
                }

                sync_sliders_can_recalc = 0;
                var alts = "";
                var nodes = "";

                var sync_sliders = [];

                for (var i=0; i<n.length; i++)
                {
                   var v = 100*n[i][4];
                   if (v<0) v=0; if (v>100) v=100;
                   var vo = 100*n[i][3];
                   if (vo<0) vo=0; if (vo>100) vo=100;
                   sync_sliders[i]=new Object();
                   sync_sliders[i].num=i;
                   sync_sliders[i].min=0;
                    sync_sliders[i].max = 100;
                   sync_sliders[i].orig_val=vo;
                   sync_sliders[i].val=v;
                   sync_sliders[i].onchange=Sync_onChangeSlider;
                   sync_sliders[i].color=slider_palette[i];
                   sync_sliders[i].readonly=<% =iif(isTeamTimeOwner, 0, 1) %>;
                   sync_sliders[i].handler=1;
                   nodes += "<tr><td class='text'><span id='val_" + i + "' class='sa_value text small'>" + ShowDecNum(sync_sliders[i].val,1) + "%</span>" + n[i][2] + "<div class='sa_slider' id='" + id_prefix + "slider" + i + "'></div></td></tr>";
                }
                if (nodes!="") nodes = "<table border=0>" + nodes + "</table>";

                for (i=0; i<a.length; i++)
                {
                   if (a[i][1].length>sa_node_maxlen) a[i][1] = a[i][1].substr(0,sa_node_maxlen-1) + "…";
                   var v = 100*a[i][2];
                   if (v<0) v=0; if (v>100) v=100;
                   var k = -i-1;
                   sync_sliders[k]=new Object();
                   sync_sliders[k].num=k;
                   sync_sliders[k].min=0;
                   sync_sliders[k].max=100
                   sync_sliders[k].val=v;
                   sync_sliders[k].color=slider_palette[slider_palette.length-i];
                   sync_sliders[k].readonly=1;
                   sync_sliders[k].handler=0;
                   alts += "<tr><td class='text'><span id='val_" + k + "' class='sa_value text small'>" + ShowDecNum(v,1) + "%</span>" + a[i][1] + "<div class='sa_slider' id='" + id_prefix + "slider" + k + "'></div></td></tr>";
                }
                if (alts!="") alts = "<table border=0>" + alts + "</table>";

                tdDSAControl.innerHTML = "<% if isTeamTimeOwner Then %><div class='text' style='text-align:center'>" + lbl_sa_sel_obj + "&nbsp;<select name='wrt_node' onchange='Sync_onChangeWRTNode(this.value);'>" + Sync_SAGetSubnodesDroplist(n_all, -1, wrt, "") + "</select></div><% End if %>" +
                                         "<table border=0 cellspacing=4 cellpadding=1 style='height:85%'>" + 
                                         "<tr valign=top><td><h6>" + lbl_objectives + "</h6><div class='sa_list'>" + nodes + "</td><td><h6>" + lbl_alternatives + "</h6><div class='sa_list'>" + alts + "</div></td></tr></table>";

                <% if IsTeamTimeOwner Then %>tdDSAControl.innerHTML += "<div style='text-align:center; margin-top:1em'><input type=button value='" + lbl_refresh + "' class='button' onclick='Sync_ResetAllSliders(); return false'></div>";<% End If %>

                for (i=0; i<n.length; i++) sliderInitByName(id_prefix + "slider" + i, sync_sliders[i]);
                for (i=0; i<a.length; i++) sliderInitByName(id_prefix + "slider" + (-i-1), sync_sliders[-i-1]);

                sync_sliders_can_recalc = 1;

            } 
            else
            {
                tdDSAControl.innerHTML = "<div style='text-align:center; margin-top:4em' class='text'>" + lbl_nooverall + "</div>";
            }    
        }
    }


    function Sync_onSubmitDSA()
    {
        <%if isTeamTimeOwner Then %>
        var nodes = "";
        var n = sync_last_data[3];
        var k = -1;
        for (var i=0; i<n.length; i++) 
            if (n[i][1]==sync_last_data[1])
            {
                k+=1;
	            var s = document.getElementById(id_prefix + "slider" + k);
                if ((s)) nodes += "&id" + k + "=" + n[i][0] + "&val" + k + "=" + (Math.round(s.val*10000)/1000000);
            }
        SyncAddCommand("?<% =_PARAM_ACTION  %>=dsa_recalc" + encodeURI(nodes) + "&cnt=" + (k+1), 0, -1);
        <%End if %>return false;
    }

    function Sync_onChangeWRTNode(id)
    {
        <% if isTeamTimeOwner Then %>SyncAddCommand("?<% =_PARAM_ACTION  %>=dsa_wrtnode&id=" + id, 0, -1);<% End If %>
        sync_first_call = 1;
        SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 1);
        return false;
    }

    function Sync_SliderUpdateTextValue(slider_id, value)
    {
        var b=document.getElementById('val_'+slider_id);
        if ((b)) b.innerHTML=ShowDecNum(value,1)+"%";
    }

    function Sync_SlidersRecalculate(idx, val)
    {
        if (sync_sliders_can_recalc && (sync_last_data))
        {
            var t = 0;
            var n = sync_last_data[3];
            var nodes = [];
            for (var i=0; i<n.length; i++) if (n[i][1]==sync_last_data[1]) nodes.push(n[i]);
            for (i=0; i<nodes.length; i++)
              if (idx!=i)
              {
	              var s = document.getElementById(id_prefix + "slider" + i);
                  if ((s)) t+=(s.val/100);
              }
            if (t==0) t = 0.00000001;
            var c = (1-(val/100))/t;

            for (i=0; i<nodes.length; i++)
              if (idx!=i)
              {
	            var s = document.getElementById(id_prefix + "slider"+i);
	            if ((s))
	            {
                   var v = s.val*c;
                   if (v<0.0001) v = s.orig_val/100000;
   	               s.val = v;
    	           drawSliderByVal(s);
                   Sync_SliderUpdateTextValue(i, v);
    	        }
              }
        }
    }

    function Sync_onChangeSlider(val, box)
    {
        Sync_SliderUpdateTextValue(box, val);
        Sync_SlidersRecalculate(box, val);
    }

    function Sync_ResetSlider(slider_id)
    {
      var slider = document.getElementById(id_prefix + "slider" + slider_id);
      if ((slider) && (slider.orig_val)!=null) { slider.val = slider.orig_val; drawSliderByVal(slider); Sync_SlidersRecalculate(slider_id, slider.val); }
    }

    function Sync_ResetAllSliders()
    {
      sync_sliders_can_recalc = 0;
//      for (var i=0; i<sync_last_data[2].length; i++) Sync_ResetSlider(-i-1);
      var n = sync_last_data[3];
      var k = 0;
      for (i=0; i<n.length; i++) if (n[i][1]==sync_last_data[1]) { Sync_ResetSlider(k); k+=1; }
      sync_sliders_can_recalc = 1;
    }
    
    function SyncShowWRTResults(id)
    {
        if (SyncCheckActive()) SyncAddCommand("?<% =_PARAM_ACTION %>=global_wrt&id=" + id, false, null);
        return false;
    }

    function getTinyBar(val) {
        return "<div style='width:" + tinybar_width + "px; height:2px; background:#f0f0f0; text-align:left; font-size:1px; margin-bottom:1px;'>" + (val>=0 ? "<div style='background:#99cce0; width:" + Math.round((val>1 ? 1 : val)*tinybar_width) + "px; height:2px;'><img src='" + img_path + "blank.gif' width=1 height=1 title='' border=0></div>" : "&nbsp;") + "</div>";
        }

    function initTree(nodes, tree_id, act_id) {
        var lst = [];
        for (var i=0; i<nodes.length; i++) {
            var n = nodes[i];
            //lst.push({id:n[3], pId:n[4], name:n[2]+"<span style='background:#ccc; width:45px; padding-left:3px; text-align:right;position:absolute; left:200px;'>100.0%</span>", open:true, rID:n[0]});
            lst.push({id:n[3], pId:n[4], name:"<table border=0 cellspacing=0 cellpadding=0 width=100% style='width:100%;'><tr><td class='text'><nobr>" + n[2]+ "</nobr></td><td style='width:"+(tinybar_width+2) + "px; padding-left:4px; text-align:right' class='text small'><nobr>" + (n[5]!="" && (n[5])*1>0.01  ? ShowDecNum(100*n[5], 1) + "%" + getTinyBar(n[5]) : "&nbsp;") + "</nobr></td></tr></table>", open:true, rID:n[0]});
        }
        $.fn.zTree.init($("#" + tree_id), zTree_settings, lst);
    
        var zTree = $.fn.zTree.getZTreeObj(tree_id);
        for (var i=0; i<nodes.length; i++) {
            if (nodes[i][0] == act_id) zTree.selectNode(zTree.getNodeByParam("id", nodes[i][3], null), true);
        }

        $(".node_name").parent().prop("title", "");
        $(".node_name").prev().remove();
        $(".ztree li a").width("calc(100% - 22px)");
        $(".ztree li a span").width("100%");
    }

    function zTreeClick(treeId, treeNode, clickFlag) {
        if ((treeNode) && treeNode.rID!=null) {
            sync_first_call = 1;
            SyncShowWRTResults(treeNode.rID*1);
            SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 1);
            return true;
        } else return false;
    };

    //function GetNodesList(data, parent_id)
    //{
    //    var nodes = [];
    //    for (var i=0; i<data.length; i++)
    //    {
    //        if (data[i][1]==parent_id) nodes.push(data[i]);
    //    }    
    //    return nodes;
    //}

    //function SyncAddHierarchyNodes(uid, data, parent_id, active_id, level, parent_is_last)
    //{
    //    var r = "";
    //    var nodes = GetNodesList(data, parent_id);
    //    for (var i=0; i<nodes.length; i++)
    //    {
    //        var n = nodes[i];
    //        var is_last = i==(nodes.length-1);
    //        var usr_prty = null; // GetRowByIdxKey(n[3], 0, sync_user);
    //        var usr = ((usr_prty) && (usr_prty[2]) ? 100*usr_prty[1] : -1); 
    //        var node = "&nbsp;" + (active_id!=n[0] ? "<a href='' onclick='SyncShowWRTResults(" + n[0] + "); return false;' class='actions' style='color:#666666'>" + n[2] + "</a>" : "<b>" + n[2] + "</b>");
    //        for (var j=0; j<level; j++) node="<span style='width:19px' class='" + (j==0 ? (is_last ? "tree_node_last" : "tree_node") : (parent_is_last ? "tree_empty" : "tree_span")) + "'>&nbsp;</span>"+node;
    //        r += "<div onmouseover='this.style.backgroundColor=\"#e0e0e0\";' onmouseout='this.style.backgroundColor=\"\";'><nobr><span style='width:" + (res_tree_width-45) + "px; overflow:hidden;'>" + node + "</span>" + (prty=-1 ? "" : "<span class='small' style='width:4em; text-align:right'>" + ShowDecNum(prty) + "</span>") + "</nobr></div>";
    //        r += SyncAddHierarchyNodes(uid, data, n[0], active_id, level+1, is_last);
    //    }
    //    return r;
    //}
    
    function SyncShowResults(task, data, is_global)
    {
        var s = SyncContent();
        if ((s))
        {
            s.innerHTML = "";
            SyncSetContentAlign(0);
           
            var r = document.createElement("div");
            r.setAttribute("id", "divResults");
            r.setAttribute("class", "whole");
            s.appendChild(r);
            
            r.innerHTML = SyncCreateEvaluationScreen("tdResults", "", false, false);
            SyncAddTask(task, $get("tdTask"));
            
            var sync_results_options = data[(is_global) ? 4 : 3][0];
            var old_res_option = sync_results_options;
            var user = GetRowByIdxKey(data[1], 1, sync_user)
           if (!(user) && sync_results_options == 0) sync_results_options = 2;
//            if ((user) && (data[2]))
            if ((data[2]))
            {
                //tdResults.innerHTML = (is_global && (data[3]) && (data[3].length>1) ? "<table border=0 cellspacing=0 cellpadding=4 style='height:99%'><tr valign=top><td align=left id='tdHierarchy' class='text' style='padding-right:8px;'><div id='divHierarchy' style='margin-top:55px; vertical-align:middle; overflow-y:auto;'><div style='background:#fafafa; border:1px solid #e0e0e0; padding:6px; '>" + SyncAddHierarchyNodes((user) ? user[0] : -999, data[3], -1, data[0], 0, 0) + "</div></div><img src='" + img_blank + "' width=" + res_tree_width + " height=1 title='' border=0></td><td id='ResultsGrid' align='center'></td></tr></table>" : "<p id='ResultsGrid' align=center style='margin:0px; padding:0px;'></p>") + "<div id='divResultsMsg'></div>";
                var show_tree = (is_global && (data[3]) && (data[3].length>1));
                tdResults.innerHTML = (show_tree ? "<table border=0 cellspacing=0 cellpadding=4 style='height:99%'><tr valign=top><td align=left id='tdHierarchy' class='text'><div id='divHierarchy' class='ztree' style='margin-top:55px; margin-right:32px; vertical-align:middle; overflow-y:auto; padding:4px; border:1px solid #f0f0f0;'></div><img src='" + img_blank + "' width=" + res_tree_width + " height=1 title='' border=0></td><td id='ResultsGrid' align='center'></td></tr></table>" : "<p id='ResultsGrid' align=center style='margin:0px; padding:0px;'></p>") + "<div id='divResultsMsg'></div>";
                if (show_tree) initTree(data[3], "divHierarchy", data[0]);
                var res = SyncShowResultsGrid(data);    // 0 = no missing, 1 - no indiv, 2 - no combined, 3 - no anything;
                // sync_results_options = // rvBoth = 0, rvIndividual = 1, rvGroup = 2;
                var incons = ((user) ? user[5] : -1);
                if (!is_global && sync_results_options!=2 && incons>=0 && res!=1) tdResults.innerHTML += "<div class='text' style='text-lign:center; padding:1ex'>" + lbl_inconsist + ": <span class='label_my'><b>" + ShowDecNum(incons) + "</b></span></div>";
                var divResultsMsg = $get("divResultsMsg");
                if (res==1 || res==3) tdTask.innerHTML = "";
//                alert(res + " " + sync_results_options + " " + old_res_option);
                if (res==1 && sync_results_options!=1) 
                {
                    if ((user) && (divResultsMsg)) divResultsMsg.innerHTML += "<div class='text' style='margin-top:2em; font-weight:bold; text-align:center'>" +  lbl_no_res + "</div>";
                }    
                else
                {
                    if (res>0) divResultsMsg.innerHTML += "<div class='text' style='margin-top:2em; font-weight:bold; text-align:center'>" +  (sync_results_options == 1 ? lbl_no_res_coff : lbl_no_res_con) + "</div>";
                } 
            }
            sync_results_options = old_res_option;
            setTimeout('syncResizeResults();', 110);
        }
    }

    function syncResizeResults() {
        var d = $("#divHierarchy");
        if ((d)) {
            d.height(50);
            d.height($("#tdResults").height()-75);
        }
    }
    
    var tt_stopped = false;
    function SyncStop(url)
    {
        if ((tt_stopped)) return false;
        msg("Call for change meeting status");
        sync_active = 0;
        UpdateOptions();
        sync_q = [];
        UpdateStatusImage();
        if (url!='') {
            SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 1);
            document.location.href=url;
        }
        return false;
    }

    function SyncCheckActive()
    {
        if (!sync_active)
        {
            if (confirm(lbl_conf_continue)) SyncSwitchStatus(); 
        }
        return sync_active;
    }
    
    function onCloseWindow()
    {
        if (sync_active) SyncStop('');
        SyncResetScreen("&nbsp;");
        SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 1);
        DebugClose();
        <% If IsConsensus Then %>if (sync_active) SyncStopPM();<% end if%>
    }
    
    function ChangeImage(obj, img)
    {
        if ((obj) && (img)) obj.src = img.src;
        return false;
    }
    
    function UpdateOptions()
    {
        var d = $get("divOptions");
        if ((d)) d.disabled = !sync_active;
        d = $get("divTTSettings");
        if ((d)) d.disabled = !sync_active;
    }
    
    function SyncSwitchStatus()
    {
        <% if isTeamTimeOwner Then %>Array.insert(sync_queue, 0, ["<% =_PARAM_ACTION %>=" + (sync_active ? "pause" : "start"), null]); SyncSend();<% End If %>
        sync_active = !sync_active;
        msg("Switch TeamTime status: " + sync_active);
        UpdateStatusImage();
        if (sync_active) SyncInit(); else UpdateOptions();
        return false;
    }
    
    function UpdateStatusImage()
    {
        var img = document.images["imgStatus"];
        if ((img)) img.src = (sync_active ? img_pause.src : img_start.src);
        return false;
    }
    
    var WRTObject = "";
    function FlashWRT(highlight)
    {
        var wrt = $get("wrt_name");
        if ((wrt)) wrt.style.color = (highlight ? "#0066ff" : "");
        if ((WRTObject) && WRTObject!="")
        {
            wrt = $get(WRTObject);
            if ((wrt)) wrt.style.backgroundColor = (highlight ? "#ffff99" : "");
        }
    }

    function DoFlashWRT(wrt_object)
    {
        WRTObject = wrt_object;
        var delay = 125;
        for (var i=1; i<=4; i++)
        {
            setTimeout("FlashWRT(1);", delay+i*2*delay);
            setTimeout("FlashWRT(0);", 2*delay+i*2*delay);
        }    
    }

    function DoFlashWRTPath()
    {
        var o = $("#wrt_path");
        if ((o) && (o.length)) o.animate({color: "#3399ff"}, 1500).animate({color: "#e0e0e0"}, 500).slideUp(500).animate({color: "#0066cc"}, 50);
    }

    function ToggleWRTPath()
    {
        $("#wrt_path").each(function() { $(this).slideToggle(); });
//        $("#wrt_path").slideToggle();
    }
    
    function Hotkeys(event)
    {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event)
        {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            <% if isTeamTimeOwner Then %>if (event.ctrlKey)
            switch (code)
            {
   	            case 37:	// prev
                    if ((theForm.btnPrev) && !(theForm.btnPrev.disabled)) theForm.btnPrev.click();
                    break;
                case 39:	// next
                    if ((theForm.btnNext) && !(theForm.btnNext.disabled)) theForm.btnNext.click();
                    break;
            }
            if (code == 13 && jump_visible) { DoJump(); return false; }
            if (code == 13 && !comment_editor_visible) return false;
            <% End if %>
            if (sync_last_comment!="")
            {
//                if ((code==13 || code==27) && (!event.altKey))
                if ((code==27) && (!event.altKey))
                 {
                    var btn = $get(code == 13 ? "btn_OK" : "btn_Cancel");
                    if ((btn)) btn.click();
                 }
            }     
        }
    }
    
   function onResize()
   {
        if ((document.body.clientHeight) && can_resize) {
//        if (sync_last_type == type_g_res)
//        {
//            var margins = 16;
//            var tdr = $get("tdResults");
//            var dh = $get("divHierarchy");
//            if ((tdr) && (dh))
//            {
////                dh.style.height = "";
////                dh.style.height = tdr.clientHeight - margins;
////                msg(dh.clientHeight + "-" + tdr.clientHeight);
//            }   
//        }
            SyncCheckDynamicPaging();
        }
        syncResizeResults();
        SyncResizeList();
   }
   
    function SyncStopUser()
    {
        sync_active = 0;
        var url = '<% =PageURL(_PGID_SERVICEPAGE, "teamtime=1&close=1&" + _PARAM_ID + "=" + App.ProjectID.ToString + CStr(IIf(isSLTheme(True), GetTempThemeURI(True), ""))) %>';    
        document.location.href=url;
        return false;
    }
    
<% If isTeamTimeOwner Then %>    
    var jump_visible = 0;

    function ShowJumpTooltip(ctrl_id)
    {
        var t = $find("<% =ttJump.ClientID %>");
        if ((t))
        {
           t.set_targetControlID(ctrl_id);
           t.show();
           jump_visible = 1;
           setTimeout('theForm.jump.focus();', 200);
        }
    }

    function HideJumpTooltip()
    {
        var t = $find("<% =ttJump.ClientID %>");
        if ((t)) t.hide();
        jump_visible = 0;
        return false;
    }
    
    function DoJump()
    {
        var j = theForm.jump;
        if ((j))
        {
            var s = j.value*1;
            if (s>0 && s<=<% =TeamTimePipe.Count %>)
            {
                HideJumpTooltip();
                SyncStep(s);
            }
            else
            {
                jDialog("<%=JS_SafeString(String.Format(ResString("msgWrongEvaluationJumpStep"), TeamTimePipe.Count)) %>", true, "setTimeout('theForm.jump.focus();', 350);"); 
            }
        }
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

//    function S(id) {
//        SetNextStep(id);
//        theForm.jump.value=id;
//        return false;
//    }

    function addStepsList() {
        var div = $get("divStepsList");
        if ((div)) {
            var s = "";
            var styles = ['button_evaluate_undefined','button_evaluate_missing','button_evaluated','button_no_evaluate'];  // 0: no judgments, 1: missing, 2: all judgments, 3: non-evaluation
//            var colors = ['button_red_c', 'button_yellow_c', 'button_green_c', 'button_c'];

            for (var i=0; i<steps_list.length; i++) {
                var d = steps_list[i];
//                s += "<div style='margin:2px' id='divStep" + d[0] + "'><input type='button' value='" + d[2] + "' class='" + colors[d[3]] + " button_small " + styles[d[1]] + (d[0]==<% =TeamTimePipeStep %> ? " button_active_list" : "") + "' onclick='SyncStep(" + d[0] + "); return false;' style='width:100%; text-align:left; padding:0px 2px; font-weight:normal;'></div>";
                s += "<div style='margin:2px' id='divStep" + d[0] + "'><input type='button' value='" + d[2] + "' class='button button_small " + styles[d[1]] + (d[0]==<% =TeamTimePipeStep %> ? " button_active_list" : "") + "' onclick='SyncStep(" + d[0] + "); return false;' style='width:100%; text-align:left; padding:0px 2px; font-weight:normal;'></div>";
//                 s += "<input type=button class='button_step " + styles[d[1]] + " " + colors[d[3]] + "'" + (d[1]==4 ? " disabled" : " onclick='S(" + d[0] + ");'") + " id='divStep" + d[0] + "' value='" + d[2] + "' ><br>";
            }
            steps_list = null;
            div.innerHTML = s;
            setTimeout('var b = $get("divStep<% =TeamTimePipeStep-(ShowStepsCount\2)-1 %>"); if ((b)) b.scrollIntoView(true);', 30);
        }
    }

    function parseStepsList(data) {
        var div = $get("divStepsList");
        if ((div) && data!="") {
            var lst = null;
            try { lst = eval(data); }
            catch(e) { }
            if ((lst) && lst.length>0) {
                steps_list = steps_list.concat(lst);
                div.innerHTML = "<p align=center class='text small gray' style='margin-top:13.5em'><img src='" + img_path + "devex_loading.gif' width=16 height=16 border=0><br><br>Loading: " + Math.round(100*steps_list.length/<% = TeamTimePipe.Count %>) + "%</p>";
                load_steps += lst.length;
                if (load_steps>0 && load_steps<<% = TeamTimePipe.Count %>) {
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
            var d = $get("divStep<% =TeamTimePipeStep-(ShowStepsCount\2)-1 %>");
            if ((d)) d.scrollIntoView(true);
        }
    }

    function ClientNodeClicking(sender, eventArgs)
    {
        var node = eventArgs.get_node();    
        if ((node))
        {
            if (node.get_value()<=0)
            {
                eventArgs.set_cancel(true);
            }
            else
            {
                SyncStep(node.get_value());
            }    
        }    
    }
    
    function EndMeeting()
    {
        <% If IsConsensus Then %>SyncStopPM();
        if ((window.opener) && (typeof window.opener.onEndTeamTime)=="function") {
            window.opener.focus();
            window.opener.onEndTeamTime(); 
        } else {
            if ((window.parent) && (typeof window.parent.onEndTeamTime)=="function") {
                window.parent.focus();
                window.parent.onEndTeamTime();
            }
        }<% Else %>if (confirm("<% =JS_SafeString(ResString("msgConfirmSynchronousClose")) %>")) SyncStopPM();<% End If%>
        return false;
    }
    
    function SyncStopPM()
    {
        SyncStop('<% =PageURL(CurrentPageID, _PARAM_ACTION + "=stop&" + _PARAM_ID + "=" + App.ProjectID.ToString + CStr(IIf(GetTempTheme() = "tt" OrElse GetTempTheme() = "sl" OrElse IsConsensus, "&temptheme=tt&close=yes", ""))) %>');    
    }
    
    function SyncStep(n)
    {
        var t = $find("<% =RadToolTipStepsList.ClientID %>");
        if ((t)) t.hide();
        t = $find("<% =RadToolTipEvaluationProgress.ClientID %>");
        if ((t)) t.hide();
        //SyncStop('?<% =_PARAM_STEP %>='+n+"<%=GetTempThemeURI(true) %>");
        sync_active = 0;
        UpdateOptions();
        sync_q = [];
        UpdateStatusImage();
        document.location.href='?<% =_PARAM_STEP %>='+n+"<%=GetTempThemeURI(true) %>";
        return false;
    }

    function Sync_ShowEvaluationProgress()
    {
        var t = $find("<% =RadToolTipEvaluationProgress.ClientID %>");
        if ((t))
        {
            if (t.isVisible())
            {
                t.hide();
            }
            else
            {
                Sync_UpdateEvaluationProgress();
                t.show();
            }
        }
        return false;
    }

    function SyncSettings()
    {
        var t = $find("<% =ttSettings.ClientID %>");
        if ((t))
        {
            if (t.isVisible())
            {
                t.hide();
            }
            else
            {
                theForm.ttsorting.value = sync_users_sorting;
                if (theForm.ttsorting.item.length>2) theForm.ttsorting.item(2).disabled = !sync_usekeypads;
                theForm.cbShowVariance.disabled = !sync_hidejudgments;
                $get("divShowVariance").disabled = !sync_hidejudgments;
                t.show();
            }
        }
        return false;
    }

    function SyncSetSorting(val)
    {
        if (val*1 != sync_users_sorting*1) { SyncAddCommand("action=sorting&value=" + val, 0, null); SyncSettings(); }
    }

    function SyncShowVariance(show)
    {
        SyncAddCommand("action=variance&value=" + ((show) ? 1 : 0), 0, null);
    }

    function SyncShowPWSide(show)
    {
        SyncAddCommand("action=pwside&value=" + ((show) ? 1 : 0), 0, null);
    }

    function SyncHideCombined(show)
    {
        msg("Call for swicth 'hide combined' option");
        SyncAddCommand("action=nocombined&value=" + ((show) ? 1 : 0), 0, null);
    }
    
    function SyncSwitchViewOnlyUsers(show)
    {
        SyncAddCommand("action=viewonly&value=" + ((show) ? 1 : 0), 0, null);
    }

    function SyncSwitchSessionOwner(hide)
    {
        SyncAddCommand("action=hideowner&value=" + ((hide) ? 1 : 0), 0, null);
    }

    function SyncSwitchOfflineUsers(hide)
    {
        SyncAddCommand("action=hideoffline&value=" + ((hide) ? 1 : 0), 0, null);
    }

    function Sync_SwitchInfodocMode() {
        if (sync_infodocsmode>=0) SyncAddCommand("action=infodocmode&value=" + sync_infodocsmode, 0, null);
    }

    function SyncSwitchDistrView() {
        sync_distr_view  = (1-sync_distr_view);
        sync_force_redraw = 1;
        var im = $get("imgMode");
        if ((im)) { 
            im.title = (sync_distr_view ? hint_norm_view : hint_distr_view); 
            ChangeImage(im, (sync_distr_view ? img_group : img_list));
        }
        SyncRenderData(sync_last_data_full, sync_force_redraw);
        return false;
    }

    function Sync_UpdateEvaluationProgress()
    {
        var lst = $get("divEvaluationProgress");
        if ((lst))
        {
            lst.innerHTML = ""; "<h6>" + lbl_eval_progress + ":&nbsp;&nbsp;</h6>";
            if (!(sync_last_prg) || sync_last_prg =="" || sync_last_prg.length==0)
            {
                lst.innerHTML += "<p>No progress information available</p>";
            }
            else
            {
                var data = "<p align='center' style='padding:0px; margin:0px'><table border=0 cellspacing=1 cellpadding=1>";

                var users = (sync_last_type == type_pw || sync_last_type == type_direct || sync_last_type == type_g_res || sync_last_type == type_msg || sync_last_type == type_l_res  ? sync_last_data[1] : sync_last_data[2]);
                users = ReorderUsers(users, sync_user, 1, true);
                if ((users))
                {
                    var usr_exists = GetRowByIdxKey(users, id_email, sync_eval_prg_user);
                    if (!(usr_exists) && sync_eval_prg_user!="") users.unshift([-1, sync_eval_prg_user, sync_eval_prg_name, 0, 0]);
                    usr_exists = GetRowByIdxKey(users, id_email, sync_user);
                    if (!(usr_exists) && sync_user!="") users.unshift([-1, sync_user, "<% =JS_SafeString(App.ActiveUser.UserName) %>", 1, 0]);
                    for (var i=0; i<users.length; i++)
                    {
                        var user = users[i];
                        if ((user))
                        {
                            var eval = null;
                            for (var j=0; j<sync_last_prg.length; j++)
                            {
                                if (sync_last_prg[j][0]==user[id_email]) { eval = sync_last_prg[j]; break; }
                            }
                            //if ((eval))
                            //{
                            var p = ((eval) && eval[2]!=0 ? eval[1]/eval[2] : 0);
                            var name = GetUserName(user, i+1);
                            var u = $get(id_prefix + user[id_uid] + "_username");
                            if ((u)) name = u.innerText;
                            if (!sync_anonymous && user[id_email]!=name) name = '<span title="' + replaceString('"', '&#39;', user[id_email]) + '">' + name + '</span>';
                            data += "<tr class='text' valign='middle'><td><nobr><input type='radio' name='eval_prg_uid' value='" + user[id_email] + "'" + (user[id_keypad]==-1 ? "disabled=disabled" : (user[id_email].toLowerCase() == sync_eval_prg_user ? " checked" : "")) + " onclick='SyncSetEvalUser(this.value);'>&nbsp;" + name + "</nobr></td>" + ((eval) ? "<td align=right><b>" + eval[1] + "</b></td><td align=center style='padding:0px'><b>/</b><td align=left><b>" + eval[2] + "</b></td><td>" + CreateBar(p, fill_blue) + "</td><td class='small' style='width:4.3em; text-align:right;'>" + Math.round(100*p, 1) + "%</td>" : "<td colspan=4 class='text small'>&nbsp;</td>") + "</tr>";
                            //}
                        }
                    }
                }
                data += "</table></p>";
                lst.innerHTML += data;
                lst.style.height = (sync_last_prg.length>20 ? "30.5em" : "");
            }
        }
    }

    function SyncSetEvalUser(usr) {
        sync_eval_prg_user = usr.toLowerCase();
        sync_eval_progress = -1;
        SyncShowEvalProgress(false);
        $("#btnNextUnas").prop("disabled", true);
        SyncAddCommand("action=eval_prg_user&value=" + encodeURIComponent(usr), 0, null);
    }

    function SwitchAnonymous()
    {
        if ((theForm.cbAnonymous)) SyncAddCommand("action=anonymous&value=" + (theForm.cbAnonymous.checked ? 1 : 0), 0,null);
        return false;
    }
    
    function SwitchHide()
    {
        if ((theForm.cbHideJudgments))
        {
            var hide = theForm.cbHideJudgments.checked;
            SyncAddCommand("action=hidejudgments&value=" + (hide ? 1 : 0), 0,null);
            theForm.cbShowVariance.disabled = !hide;
            $get("divShowVariance").disabled = !hide;
            if (sync_usekeypads)
            {
                c = theForm.cbShowPWSide; 
                if ((c)) c.disabled = !hide; 
                c = $get("divPWSide");
                if ((c)) c.disabled = !hide;
            }
        }
        return false;
    }

    function SyncUpdateShow2Me()
    {
        var btn = theForm.btnShowJudgments;
        var st = (sync_hidejudgments ? "block" : "none");
        if ((btn) && btn.style.display!=st) 
        {
            msg("Show2Me button display: " + st);
            btn.style.display = st; 
            if ((sync_hidejudgments))
            {
                btn.value = lbl_btn_show2me;
                sync_show2me = -1;
            }
        }
    }

    function SyncShow2Me()
    {
        var btn = theForm.btnShowJudgments;
        if ((btn))
        {
            sync_show2me = (sync_show2me<0 ? sync_step : -1);
            msg("Show2Me clicked: " + sync_show2me);
            var hide = (sync_show2me >=0);
            btn.value = (hide ? lbl_btn_hide2me : lbl_btn_show2me);
            sync_hash = "";
            sync_force_redraw = 1;
            SyncAddCommand("refresh", 0, -1);
        }
    }
    
    function onRichEditorRefresh(empty, infodoc, callback)
    {
        if ((callback) && (callback[0]) && callback[0]!="")
        {
            var id = callback[0];
            var f = $get("frm"+(sync_infodocsmode ? "Infodoc" : id));
            if ((f))
            {
                var src = ((callback[1]) ? callback[1] + "&empty=" + encodeURI(lbl_no_infodoc) + "&rnd=" + Math.round(10000*Math.random()) : "");
                if ((f) && (f.contentWindow)) f.contentWindow.document.body.innerHTML = "<div style='width:99%; height:99%; background: url(" + img_path + "devex_loading.gif) no-repeat center center;'>&nbsp</div>";
                setTimeout("var f = $get('" + f.id + "'); if ((f)) f.src= \"" + src + "\";", 200);
            }
            switch (sync_infodocsmode)
            {
                case 0:
                    var im = $get("div" + id + "_image");
                    var d = $get("div" + id + "_content");
                    if ((im) && (d))
                    {
                        var c = (d.style.display=="none");
                        var n = (empty==1 ? (c ? img_plus : img_minus) : (c ? img_plus_ : img_minus_)).src;
                        im.src = n;
                    }
                    break;
                case 1:
                    var guid = GetGUID(id);
                    var im = $get(id_prefix + guid + "_infodoc");
                    if ((im))
                    {
                        var wrt = (im.src == img_WRTinfodoc1.src || im.src == img_WRTinfodoc2.src);
                        empty = empty*1;
                        im.src = (!(empty) ? (wrt ? img_WRTinfodoc1.src : img_infodoc1.src) : (wrt ? img_WRTinfodoc2.src : img_infodoc2.src));
                    }
                    break;
            }
        }
        window.focus();
    }
<% Else %>
    function SyncStopPM()
    {
        SyncStopUser();
    }
<% End If%>

    function SyncShowEvalProgress(update_list) {
        var usr_prg = GetRowByIdxKey(sync_last_prg, 0, sync_eval_prg_user);
        var eval = $get("divEvalProgress");
        if ((eval) && (usr_prg) && ((usr_prg[1]*usr_prg[2])!=sync_eval_progress || update_list))
        {
            var prg_0 = "<b>" + usr_prg[1] + "</b>";
            var prg_1 = "<b>" + usr_prg[2] + "</b>";
            eval.innerHTML = replaceString("{0}", prg_0, replaceString("{1}", prg_1, "<% =JS_SafeString(ResString("lblEvaluationStatus")) %>"));
            var t = "";
            if (!isActiveUser(sync_eval_prg_user)) {
                var n = sync_eval_prg_user;
                var users = (sync_last_type == type_pw || sync_last_type == type_direct || sync_last_type == type_g_res || sync_last_type == type_msg || sync_last_type == type_l_res  ? sync_last_data[1] : sync_last_data[2]);
                var  u = null;
                if ((users)) {
                    users = ReorderUsers(users, sync_user, 1, true);
                    for (var i=0; i<users.length; i++) {
                        if (users[i][id_email].toLowerCase() == sync_eval_prg_user.toLowerCase()) {
                            u = users[i];
                            if ((u) && (u[id_name]!="" && u[id_email]!=u[id_name])) {
                                n = u[id_name] + " (" + sync_eval_prg_user + ")";
                            }
                            if (sync_anonymous) n = GetUserName(u, i+1);
                            break;
                        }
                    }
                }
                t = "<% =JS_SafeString(ResString("lblTTForUser")) %> \"" + replaceString("'", "&#39;", htmlStrip(n)) + "\"";
                eval.innerHTML = "<span title='" + t + "'><img src='" + img_path + "person_blue.png' width=10 height=10 border=0>" + replaceString(": ", "*:&nbsp;", eval.innerHTML) + "</span>";
            }

            var b = $("#btnNextUnas");
            b.prop("disabled", (sync_next_unas<=0));
            var n = replaceString("*", "", b.prop("value"));
            b.prop("value", n + (isActiveUser(sync_eval_prg_user) ? "" : "*"));
            b.prop("title", t);

            sync_eval_progress = (usr_prg[1]*usr_prg[2]);
        }
        <% if isTeamTimeOwner Then %>if (update_list) setTimeout('Sync_UpdateEvaluationProgress();', 100);<% End if %>
        if ((eval) && !(usr_prg) && eval.innerHTML!='' ) eval.innerHTML = '';
    }

    function SyncResizeUsersList()
    {
        var td = $get('tdScrollableList');
        var lst = $get('divScrollable');
        if ((td) && (lst)) 
        {
            var old_h = lst.style.height; 
            lst.style.height="1px"; 
            var h = td.clientHeight+"px"; 
            if (old_h!=h) old_h = h; 
            lst.style.height = old_h;
        }
    }

    function SyncResizeList()
    {
        setTimeout('SyncResizeUsersList();', 100);
    }
    
    print_postcommand = "onResize();";
    print_precommand = "onResize();";

    <% If CheckVar("close", False) Then %>window.close();<% End if %>

    window.onkeyup = Hotkeys;
    window.onresize = onResize;
   
//-->
</script>    

<table border='0' cellpadding='0' cellspacing='0' style='width:100%; height:100%'>
<tr style='height:1em'>
    <td class='text small gray' id='divDebug' style='font-size:7pt;' colspan='3'><% If isTeamTime AndAlso isTeamTimeOwner Then %><% If isEvaluation(GetAction(TeamTimePipeStep)) AndAlso GetAction(TeamTimePipeStep).ActionType <> ActionType.atPairwiseOutcomes Then%><a href='' onclick='Sync_SwitchInfodocMode(); return false;'><img src="<% =ImagePath %>infodoc_mode.gif" id="imgInfodocMode" width="16" height="16" border="0" title="<% =ResString("lblSwitchInfodocMode") %>" alt="" style="margin-top:4px; float:left;"/></a><% End If%><img src="<% =ImagePath %>keyboard.png" id="imgHotkeys" width="16" height="16" border="0" style="margin-top:4px; margin-left:4px; float:left; cursor: hand"/><telerik:radtooltip runat="server" id="RadtooltipHotkeys" HideEvent="ManualClose" SkinID="tooltipInfo" RelativeTo="Element" Position="BottomRight" OffsetX="-10" OffsetY="-3" TargetControlID="imgHotkeys" IsClientID="true" ShowEvent="OnClick"><div style="padding:0px 1em 1ex 1em;"><%= ResString("hintEvalHotkeys") %></div></telerik:radtooltip><% End If %>&nbsp;</td>
    <td align='right' colspan='1' class='text small' style='margin-bottom:2px'><nobr><% If isTeamTime AndAlso isTeamTimeOwner Then %><% If isEvaluation(GetAction(TeamTimePipeStep)) AndAlso GetAction(TeamTimePipeStep).ActionType = ActionType.atNonPWOneAtATime AndAlso CType(GetAction(TeamTimePipeStep).ActionData, clsNonPairwiseEvaluationActionData).MeasurementType = ECMeasureType.mtRatings Then%><a href='' onclick='return SyncSwitchDistrView();'><img src='<% =ImagePath + "bars_16_.png" %>' name='imgMode' id='imgMode' width=16 height=16 title='' border='0' onmouseover='ChangeImage(this, (sync_distr_view ? img_group : img_list));' onmouseout='ChangeImage(this, (sync_distr_view ? img_group_ : img_list_));'/></a>&nbsp;<% End If %><img src='<% =ImagePath + "settings.gif" %>' name='imgSettings' id='imgSettings' title='<% =JS_SafeString(ResString("btnSynchronousSettings")) %>' width=16 height=16 border=0 onmouseover='ChangeImage(this, img_setup_);' onmouseout='ChangeImage(this, img_setup);' class='aslink' onclick='SyncSettings(); return false;' />&nbsp;<a href='' onclick='return SyncSwitchStatus();'><img src='<% =ImagePath + "pause.png" %>' name='imgStatus' width=16 height=16 title='Pause/Continue polling' border='0' onmouseover='ChangeImage(this, (sync_active ? img_pause_ : img_start_));' onmouseout='ChangeImage(this, (sync_active ? img_pause : img_start));'/></a>&nbsp;<a href='' onclick ='return EndMeeting();'><img src='<% =ImagePath + "stop.png" %>' name='imgFinish' title='<% =JS_SafeString(ResString("btnSynchronousStop")) %>' width='16' height='16' border='0' onmouseover='ChangeImage(this, img_stop_);' onmouseout='ChangeImage(this, img_stop);' /></a><% Else %>&nbsp;<% End If%><% If isTeamTime AndAlso isTeamTimeOwner Then %>&nbsp;<a href='?resetproject=yes<% =GetTempThemeURI(true) %><% =iif(isConsensus, "&mode=consensus", "") %>'><img src='<% =ImagePath + "refresh-16_.png" %>' width=16 height=16 title='<% =JS_SafeString(ResString("btnStructureReloadProject")) %>' border=0  onmouseover='ChangeImage(this, img_reload);' onmouseout='ChangeImage(this, img_reload_);'/></a><% End if %></nobr></td>
</tr>
<tr>
    <td valign='middle' align='center' id="tdContent" class='text' colspan='4'><% =GetStartupMessage()%></td>
</tr>
<tr style='height:1em' valign='bottom'>
    <td class='text' style='width:45%; padding-right:3ex;'><% If IsConsensus Then%>&nbsp;<% Else %><table border="0" cellpadding="0" cellspacing="0"><tr>
        <td style="white-space:nowrap;" class='text'><% If isTeamTimeOwner Then%><FIELDSET class='legend' style='padding-top:0px'><LEGEND class='text legend_title'>&nbsp;<% =ResString("lblNavigationBar")%>&nbsp;<asp:Image SkinID="HelpIconRound" runat="server" ID="imgNavHelp"/>&nbsp;</LEGEND><div style='padding:0px 2px 2px 2px' class='text'><% =GetPipeSteps()%>&nbsp;<img src='<% =ImagePath %>zoom_progress.gif' width=16 height=12 border=0 id='imgEvalProgress' class='aslink' onclick="Sync_ShowEvaluationProgress();" title="<% = SafeFormString(ResString("lblUsersEvaluationProgress")) %>"></div></FIELDSET><% Else %><div style='width:35em; padding:1ex;'><span id='divSteps'>&nbsp;</span>&nbsp;&nbsp;<span id='divEvalProgress'>&nbsp;</span></div><% End if %></td>
    </tr></table><% End If %></td>
    <td class='text' style='padding-top:1ex; width:19em; white-space:nowrap;'><nobr><% If isTeamTime AndAlso isTeamTimeOwner AndAlso _TeamTimeActive AndAlso isEvaluation(GetAction(TeamTimePipeStep)) Then%><div id='divOptions' disabled='1'><input type='checkbox' id='cbAnonymous' value='1' onclick='SwitchAnonymous();' <% =iif (TeamTimePipeParams.TeamTimeStartInAnonymousMode, "checked", "") %>/><label for='cbAnonymous'><% =ResString("lblTeamTimeAnonymousMode")%></label><br /><input type='button' class='button button_small' id='btnShowJudgments' value='<% =ResString("btnShowResults2Me") %>' style='float:right;width:10em;<% =iif (TeamTimePipeParams.SynchStartInPollingMode, "", "display:none;") %>' onclick='SyncShow2Me();'/><input type='checkbox' id='cbHideJudgments' value='1' onclick='SwitchHide();' <% =iif (TeamTimePipeParams.SynchStartInPollingMode, "checked", "") %>/><label for='cbHideJudgments'><% =ResString("lblEvaliationUsePollingMode")%></label></div><% Else %>&nbsp;<% End If %></nobr></td>
    <td class='text' style='text-align:center;'>
        <img src="<% =BlankImage %>" alt="..." width="16" height="16" border="0" style="margin-bottom:4px; float:right;" name="imgLoading" onclick="sync_show_response=!sync_show_response; var v = $get('divDebug'); if ((v)) v.innerHTML=(sync_show_response ? '...' /*sync_last_data*/ : ''); ">
        <div class='text small gray' style='width:12em;white-space:nowrap; font-size:7pt; padding-left:1em; padding-bottom:4px; text-align:left'><nobr>
        <% If _TeamTimeActive Then%><% If isTeamTimeOwner Then %><div id='lblTTLastJudgment' style="display: none"><% =ResString("lblTTLastUpdateTime")%>: <span id="lblLastUpdated" style="padding:0px 3px; color:#666666"></span></div><%End If %>
        <div>Last request: <span id="lblTTLastCall">&nbsp;</span><span id="lblQueue"></span></div>
        <div>Current time: <span id="lblCurTime">&nbsp;</span></div>
        <div>Failed requests: <span id="lblAJAXFaults">0</span></div><% End If%></nobr>
        </div>
    </td>
    <td class='text' style='text-align:center; width:15em; padding-left:1em;' align='right'>
        <% If isTeamTimeOwner AndAlso Not IsConsensus Then %><nobr><input type='button' id='btnNextUnas' value='<% =ResString("btnEvaluationNextUnas") %>' class='button' style='width:12.3em; margin-bottom:3px;' tabindex='3' onclick='if (sync_next_unas>0) return SyncStep(sync_next_unas); else return false;' disabled="disabled" /><br /><input type='button' name='btnPrev' value='<% =ResString("btnEvaluationPrev") %>' class='button' style='width:6em' tabindex='2' onclick='return SyncStep(<% =(TeamTimePipeStep-1) %>);' <% =CStr(iif(TeamTimePipeStep>1, "", "disabled")) %> /><input type='button' name='btnNext' value='<% =ResString(CStr(iif(TeamTimePipeStep >= TeamTimePipe.Count, "btnEvaluationFinish", "btnEvaluationNext"))) %>' class='button' style='width:6em; margin-left:0.3em;' tabindex='1' onclick='return <% =CStr(iif(TeamTimePipeStep >= TeamTimePipe.Count, "EndMeeting();", "SyncStep(" + CStr(TeamTimePipeStep+1) + ");")) %>'/></nobr><%Else %><% If isTeamTimeOwner AndAlso IsConsensus Then%><input type='button' name='btnFinish' value='<% =ResString("btnEvaluationFinish") %>' class='button' style='width:6em; float:right; margin-right:1ex;' tabindex='5' onclick='return EndMeeting();' /><% Else %>&nbsp;<% End If %><% End If %>
    </td>
</tr>
</table>
<% #If Not OPT_USE_AJAX_INSTEAD_CALLBACK Then%>
<dxcb:ASPxCallback ID="ASPxCallbackTeamTime" runat="server" ClientInstanceName="ASPxCallbackTeamTime" EnableViewState="false">
    <ClientSideEvents CallbackComplete="function(s, e) { SyncReceived(e.result); }" />
    <ClientSideEvents CallbackError="function(s, e) { SyncError(); }" />
</dxcb:ASPxCallback><% #End If %>
<% =CreateImageScale("tiny").GetScaleMap("pwmap", False)%>
<EC:LoadingPanel id="pnlLoadingNextStep" runat="server" isWarning="true" WarningShowOnLoad="true" WarningShowCloseButton="false" Width="210"  Visible="true" EnableViewState="false" />
<% If isTeamTime Then %>
<% If isTeamTimeOwner Then%>
<telerik:RadToolTip runat="server" id="ttJump" HideEvent="FromCode" Skin="WebBlue" RelativeTo="Element" Position="TopCenter" OffsetX="0" OffsetY="-6" EnableViewState="false"><div style='padding:10px; white-space:nowrap'>
<% =ResString("lblJumpEvaluationToStep") %>: <input type='text' class='input' name='jump' value='' style='width:50px'>&nbsp;<input type='button' name="btnJump" value="OK" onclick="DoJump(); return false;" class='button btn-small' style='width:56px'/><input type='button' name='btnJumpCancel' value='<% =ResString("btnEvaluationJumpCancel") %>' onclick='HideJumpTooltip();' class='button btn-small' style="width:56px" />
</div></telerik:RadToolTip>
<telerik:RadToolTip ID="ttSettings" runat="server" AccessKey="S" OffsetX="-12" OffsetY="-3" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="FromCode" IsClientID="true" TargetControlID="imgSettings" EnableViewState="false"><div style='padding:10px 12px; white-space:nowrap' class='text' id='divTTSettings'>
<div style='border-bottom:1px solid #d0d0d0; margin-bottom:1ex; padding-bottom:2px'><b><% =ResString("btnSynchronousSettings")%></b></div>
<div><% =ResString("lblTTPollingTime")%>:&nbsp;<a href='' onclick='RefreshChange(-1); return false' id='RefreshMinus' class="actions"><% =IIf(TeamTimeRefreshTimeout <= TeamTimeMinimumRefreshTimeout, "", "-")%></a><span id="RefreshValue" style="padding:0px 3px"><% =TeamTimeRefreshTimeout%></span><a href='' onclick='RefreshChange(+1); return false' id='RefreshPlus' class="actions">+</a></div>
<div><% =ResString("lblTTUsersSorting")%>&nbsp;<select name='ttsorting' onchange='SyncSetSorting(this.value);'><option value='0'><% = ResString("lblTTSortByEmail")%></option><option value='1'><% = ResString("lblTTSortByName")%></option><% If App.Options.KeypadsAvailable Then %><option value='2'><% = ResString("lblTTSortByKeyPad")%></option><% End If %></select></div>
<div id='divShowVariance'><input type='checkbox' id='cbShowVariance' value='1' <% = iif(TeamTimeShowVarianceInPollingMode, "checked", "") %> onclick='SyncShowVariance(this.checked); return false;' /><label for='cbShowVariance'><% =ResString("lblTTShowVarianceInPolling")%></label></div>
<% If App.Options.KeypadsAvailable Then %><div id='divPWSide'<% =iif(UsingTeamTimeKeypads AndAlso TeamTimePipeParams.SynchStartInPollingMode, "", " disabled='1'") %>><input type='checkbox' id='cbShowPWSide' value='1' <% = iif(TeamTimeShowPWSideKeypadsInPollingMode, "checked", "") %> onclick='SyncShowPWSide(this.checked); return false;'<% =iif(UsingTeamTimeKeypads Or TeamTimePipeParams.SynchStartInPollingMode, "", " disabled='1'") %>/><label for='cbShowPWSide'><% =ResString("lblTTShowPWSideKeypadInPolling")%></label></div><% End If %>
<div><input type='checkbox' id='cbHideCombined' value='1' <% = iif(TeamTimeHideCombined, "checked", "") %> onclick='SyncHideCombined(this.checked);'/><label for='cbHideCombined'><% =ResString("lblTTHideCombined")%></label></div>
<div><input type='checkbox' id='cbShowViewOnly' value='1' <% = iif(TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess, "checked", "") %> onclick='SyncSwitchViewOnlyUsers(this.checked);' /><label for='cbShowViewOnly'><% =ResString("lblSyncOption_ShowViewOnlyUsers")%></label></div>
<div><input type='checkbox' id='cbHideSessionOwner' value='1' <% = iif(TeamTimePipeParams.TeamTimeHideProjectOwner, "checked", "") %> onclick='SyncSwitchSessionOwner(this.checked);' /><label for='cbHideSessionOwner'><% =ResString("lblSyncOption_HideOwner")%></label></div>
<div><input type='checkbox' id='cbHideOfflineUsers' value='1' <% = iif(PM.Parameters.TTHideOfflineUsers, "checked", "") %> onclick='SyncSwitchOfflineUsers(this.checked);' /><label for='cbHideOfflineUsers'><% =ResString("lblSyncOption_HideOfflineUsers")%></label></div>
</div></telerik:RadToolTip>
<telerik:RadToolTip ID="RadToolTipWhereAmI" runat="server" AccessKey="?" OffsetX="-10" OffsetY="-5" HideEvent="ManualClose" Position="TopRight" RelativeTo="Element" ShowEvent="OnClick" IsClientID="true" TargetControlID="imgWhereAmI" EnableViewState="false">
    <div style="margin:0px 1ex; padding:1ex;" runat="server" id="divTree" enableviewstate="false"><telerik:RadTreeView runat="server" ID="RadTreeViewHierarchy" AllowNodeEditing="false" OnClientNodeClicking="ClientNodeClicking" EnableViewState="false"/></div>
<img src="<% =BlankImage %>" width="200" height="1" title="" alt=""/>
</telerik:RadToolTip>
<telerik:RadToolTip ID="RadToolTipStepsList" runat="server" AccessKey="L" OffsetX="-10" OffsetY="-5" HideEvent="ManualClose" Position="TopCenter" RelativeTo="Element" ShowEvent="onClick" AutoCloseDelay="5000" TargetControlID="imgStepsList" IsClientID="true" OnClientShow="onShowStepsList" EnableViewState="false">
    <img src="<% =BlankImage %>" width="200" height="1" title="" alt="" style='margin-bottom:6px'/><% =GetStepsList()%>
</telerik:RadToolTip>
<telerik:RadToolTip ID="RadToolTipEvaluationProgress" runat="server" AccessKey="/" OffsetX="-10" OffsetY="-5" HideEvent="ManualClose" Position="TopCenter" RelativeTo="Element" ShowEvent="FromCode" IsClientID="true" TargetControlID="imgEvalProgress" EnableViewState="false">
    <div class="whole" style="padding:1ex 0px;"><h6><% =ResString("lblUsersEvaluationProgress") %>:</h6><div style="padding:0px 1em 0px 1ex; overflow-y:auto; overflow-x:hidden;" id="divEvaluationProgress">…</div></div>
</telerik:RadToolTip>
<% End if %>
<telerik:RadToolTip ID="RadToolTipInfodoc" runat="server" HideEvent="ManualClose" Position="BottomRight" OffsetX="-7" OffsetY="-3" RelativeTo="Element" IsClientID="true" EnableViewState="false" ScrollableContent="None" >
<ajaxToolkit:ResizableControlExtender ID="ResizableControlExtender" runat="server" SkinID="ResizableControl" TargetControlID="lblInfodocPanel" MinimumHeight="300" MinimumWidth="200" MaximumHeight="600" MaximumWidth="800" EnableViewState="false" OnClientResize="onRFrameResize" /><asp:Label ID="lblInfodocPanel" runat="server" CssClass="frameText" EnableViewState="false">
    <div style='width:98%; height:97%; padding:4px 0px 0px 4px'><iframe id='frmInfodoc' style='border:0px; padding:2px; width:99%; height:99%;' frameborder='0' allowtransparency='true' src='' class='frm_loading' onload='InfodocFrameLoaded(this);'></iframe></div>
</asp:Label>
</telerik:RadToolTip><telerik:radtooltip runat="server" id="RadtooltipNavHelp" HideEvent="ManualClose" SkinID="tooltipInfo" RelativeTo="Element" Position="TopCenter" OffsetX="0" OffsetY="-6" TargetControlID="imgNavHelp"  IsClientID="false" ShowEvent="OnClick"><div style="padding:10px;"><%= ResString("hintTTNavigation")%></div></telerik:radtooltip>
<% End If %>
<script language="javascript" type="text/javascript"><!--
    msg("Start TeamTime");
    window.onbeforeunload = onCloseWindow;
//-->
</script>
</asp:Content>