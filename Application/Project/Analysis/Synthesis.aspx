<%@ Page Language="VB" Inherits="SynthesisPage" title="Synthesis Results" Codebehind="Synthesis.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
<script type="text/javascript" src="/Scripts/jszip.min.js"></script>
<!--[if lt IE 9]><script language="javascript" type="text/javascript" src="/scripts/excanvas.min.js"></script><script language="javascript" type="text/javascript">var is_excanvas = true;</script><![endif]-->
<script language="javascript" type="text/javascript" src="/scripts/drawMisc.js"></script>
<script language="javascript" type="text/javascript" src="/scripts/ec.sa.js"></script>
<script language="javascript" type="text/javascript" src="/Scripts/download.js"></script>
<script language="javascript" type="text/javascript" src="/scripts/datatables_only.min.js"></script>
<script language="javascript" type="text/javascript" src="/scripts/datatables.extra.js"></script>
<%If SynthesisView = SynthesisViews.vtMixed OrElse SynthesisView = SynthesisViews.vtDashboard OrElse SynthesisView = SynthesisViews.vtObjectivesChart Then%>
<script language="javascript" type="text/javascript" src="/Scripts/polyfillrAF.js"></script>
<script language="javascript" type="text/javascript" src="/Scripts/canvg.min.js"></script>
<script language="javascript" type="text/javascript" src="/Scripts/jspdf.min.js"></script>
<script language="javascript" type="text/javascript" src="/Scripts/svg.min.js"></script>
<%End If%>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">

</style>
<script language="javascript" type="text/javascript">
    
    var isReadOnly = <%=Bool2JS(App.IsActiveProjectReadOnly)%>;
    
    var vtMixed        = <%=CInt(SynthesisViews.vtMixed)%>;
    var vtAlternatives = <%=CInt(SynthesisViews.vtAlternatives)%>;
    var vtObjectives   = <%=CInt(SynthesisViews.vtObjectives)%>;    
    var vtDSA          = <%=CInt(SynthesisViews.vtDSA)%>;
    var vtPSA          = <%=CInt(SynthesisViews.vtPSA)%>;
    var vtGSA          = <%=CInt(SynthesisViews.vtGSA)%>;
    var vt2D           = <%=CInt(SynthesisViews.vt2D)%>;
    var vtHTH          = <%=CInt(SynthesisViews.vtHTH)%>;
    var vtCV           = <%=CInt(SynthesisViews.vtCV)%>;
    var vtTree         = <%=CInt(SynthesisViews.vtTree)%>;
    var vtASA          = <%=CInt(SynthesisViews.vtASA)%>;
    var vt4ASA         = <%=CInt(SynthesisViews.vt4ASA)%>;
    var vtAlternativesChart = <%=CInt(SynthesisViews.vtAlternativesChart)%>;
    var vtObjectivesChart   = <%=CInt(SynthesisViews.vtObjectivesChart)%>;
    var vtBowTie = <%=CInt(SynthesisViews.vtBowTie)%>;
    var vtHeatMap = <%=CInt(SynthesisViews.vtHeatMap)%>;
    var vtDashboard = <%=CInt(SynthesisViews.vtDashboard)%>;

    var tSplitterSize = sessionStorage.getItem("SynthesisSplitterSize<%=App.ProjectID%>");
    var treeSplitterSize = typeof tSplitterSize != "undefined" && tSplitterSize != null && tSplitterSize != "" ? tSplitterSize*1 : 200;

    <%--var treeSplitterSize = <%=JS_SafeNumber(PM.Parameters.Synthesis_ObjectivesSplitterSize)%>;--%>
    var showHierarchyTree = <%=Bool2JS(PM.Parameters.Synthesis_ObjectivesVisibility)%>;
    var showTreePrioritiesL = <%=Bool2JS(PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 2 OrElse PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4)%>;
    var showTreePrioritiesG = <%=Bool2JS(PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 3 OrElse PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4)%>;

    var showSAMarkers = localStorage.getItem("SynthesisShowSAMarkers<%=App.ProjectID%>");
    showSAMarkers = typeof showSAMarkers != "undefined" && showSAMarkers != null && showSAMarkers != "" && showSAMarkers == "true";

    var lblNA = "<%=ResString("lblNA")%>";

    var SynthesisView  = <%=CInt(SynthesisView)%>; // >=0 if need to show only a single page (ALTS/OBJS/SA)
    var selectedLayout = (SynthesisView == vtMixed || SynthesisView == vtDashboard ? <%=selectedLayout%> : 100);
    var selectedWidgets= <%=selectedWidgets%>;
    var defaultLayout  = <%=DefaultLayout%>; 
    var defaultWidgets = <%=DefaultWidgets%>;
    <%--var dashboardSimpleViewMode = <% = Bool2JS(PM.Parameters.Dashboard_SimpleViewMode)%>;--%>
    var showLikelihoodsGivenSources = <% = Bool2JS(PM.Parameters.ShowLikelihoodsGivenSources)%>;

    var maxWidgets     = <% = maxWidgets %>;    
    var DecimalDigits  = <% = PM.Parameters.DecimalDigits %>;
    var synthMode = <%=CInt(App.ActiveProject.ProjectManager.CalculationsManager.SynthesisMode)%>;
    
    var total_users_and_groups_count = <%=PM.CombinedGroups.GroupsList.Count + PM.UsersList.Count%>;

    var btn_styles = "ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only";

    function keep_users_count() {
        return $("input.cb_keep:checked").length;
    }

    var cur_portions_page = 1;
    var total_portions_pages = -1;
    
    var UNDEFINED_INTEGER_VALUE = <%=UNDEFINED_INTEGER_VALUE%>;
    
    var dlg_users      = null;
    var dlg_alts_advanced      = null;
    var dlg_alts_custom        = null;
    var dlg_alts_attributes    = null;
    var dlg_asa_objectives     = null;
    var dlg_settings           = null;
    var dlg_filter_users       = null;
    var table_users    = null; 
    var table_groups   = null;

    var mh = 500;   // maxHeight for dialogs;

    var tableObjCollection = {};
    //var tableCVCollection = {};
    var tableObjParams = {};
    var treeCVCollection = {};
    var chartCollection = {};

    var availableLayouts = [0, 1, 2, 3, 4, 8, 9, 15];
    var numLayouts     = availableLayouts.length; //16;

    var users_data = <% = Api.GetUsersData()%>;         // users
    var groups_data = <% = Api.GetGroupsData()%>;         // groups
    //var synthesize_data= <%-- = GetSynthesizeData() --%>; // All Synthesize Data

    var IDX_USER_CHECKED = 0;
    var IDX_USER_ID      = 1;
    var IDX_USER_NAME    = 2;
    var IDX_USER_EMAIL   = 3;
    var IDX_USER_HAS_DATA= 4;
    var IDX_GROUP_PARTICIPANTS = 5;
    var IDX_GROUP_NAME_EXTRA = 6;
    var IDX_GROUP_NAME_HINT = 7;

    var alts_data = <% = Api.GetAlternativesData() %>; // alternatives IDs, Names and attributes (color, etc)    
        
    var IDX_ID        = 0;
    var IDX_ALT_NAME  = 1;
    var IDX_ALT_COLOR = 2;
    var IDX_ALT_SELECTED = 3; // 0 or 1
    var IDX_ALT_ENABLED = 4; // 0 or 1
    var IDX_ALT_INDEX = 5; // UniqueID or Index
    var IDX_ALT_TYPE = 6;
    var IDX_SCENARIOS = 7;
    var IDX_ALT_VALUES= 2;

    var IDX_OBJ_NAME  = 1;
    var IDX_OBJ_VALUES= 2;

    <% If CurrentPageID = _PGID_ANALYSIS_OVERALL_ALTS OrElse CurrentPageID = _PGID_ANALYSIS_OVERALL_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_MIXED Then %>
    var api_data = <% = Api.get_alternatives_and_objectves_priorities() %>;
    var alts_priorities = api_data.alts_priorities; // alternatives priorities - [user ID, [alts priorities]]
    var objs_priorities = api_data.objs_priorities;    // objectives   priorities - [user ID, [objs priorities]]
    var expected_values = api_data.expected_values;          // expected values - [user ID, expected value, can show expected value (1/0)]
    <% End If %>
    var columns_user_ids= [<% = StringSelectedUsersAndGroupsIDs%>];

    var IDX_CAN_SHOW_RESULTS = 0;
    var IDX_NODE_GLOBAL_PRTY = 1;    
    var IDX_NODE_LOCAL_PRTY = 2;

    var IDX_ALT_PRIORITIES = 1;
    var IDX_OBJ_PRIORITIES = 1;

    var BAR_DEFAULT_COLOR = "#b0c4de";

    var nodes_data = <% = GetNodesData() %>; // objectives IDs, Names, parent guids and users priorities (L, G)
    var coveringSources = <% = Api.GetCoveringSources() %>;
        
    var IDX_NODE_ID    = 0;
    var IDX_NODE_GUID  = 1;    
    var IDX_NODE_NAME  = 2;
    var IDX_NODE_PARENT_GUIDS  = 3;
    var IDX_NODE_PARENT_ID  = 4;
    var IDX_NODE_IS_TERMINAL = 5;
    var IDX_NODE_LEVEL = 6;
    var IDX_NODE_COLOR = 7;
    var IDX_NODE_IS_CATEGORY = 8;

    var WRTNodeID = <% = WRTNodeID %>;
    var WRTNodeParentGUID = "<% = Api.WRTNodeParentGUID %>";
    var WRTState = <% = CInt(Api.WRTState) %>; // WRT state - 0 = Goal, 1 = Selected Node
    var combinedMode = <% = CInt(App.ActiveProject.ProjectManager.CalculationsManager.CombinedMode) %>;

    <%--    var sa_objs = <%= GetSAObjectives()%>;
    var sa_alts = <%= GetSAAlternatives()%>;
    var sa_comps = <%= GetSAComponentsData()%>;--%>
    var sa_objs, sa_alts, sa_comps, sa_userid, sa_username;
    var sa_altfilter = <%= AlternativesFilterValue %>;
    var asa_data = <%= GetASAData()%>;
    var asa_pagenum = <%= PM.Parameters.AsaPageNum %>;
    var asa_pagesize = <%= PM.Parameters.AsaPageSize %>;
    var asa_sortby = <%=PM.Parameters.AsaSortMode%>;
    var user_ids_4asa = [<%=Selected4ASAUser(0)%>, <%=Selected4ASAUser(1)%>, <%=Selected4ASAUser(2)%>, <%=Selected4ASAUser(3)%>];
    var sa_alts_sortby = <%= CInt(PM.Parameters.SensitivitySorting) %>;
    var sa_userid = "<%=SAUserID%>";
    var sa_username = "<%=JS_SafeString(If(SAUserID >= 0, PM.GetUserByID(SAUserID).UserName, PM.CombinedGroups.GetCombinedGroupByUserID(SAUserID).Name))%>";

    var selected_events_type = <% = CInt(PM.Parameters.RiskionShowEventsType) %>;

    var cv_data = <% = GetConsensusViewData("") %>;
    var CVI_RANK = "rank";
    var CVI_COMMAND = "command";
    var CVI_IS_ALT = "is_alt";
    var CVI_CHILD_ID = "child_id";    
    var CVI_CHILD_NAME = "child_name";
    var CVI_OBJ_ID = "cov_obj_id";
    var CVI_OBJ_NAME = "cov_obj_name";
    var CVI_VARIANCE = "variance";
    var CVI_STD_DEVIATION = "std_dev";
    var CVI_STEP_NUM = "step_num";
    var CVI_IS_TT_STEP_AVAIL = "is_step_avail";

    var cv_tree_nodes_data = <% = Api.GetCVTreeNodesData(IsMixedOrSensitivityView(), ResString("lblTreeNodeNoSources")) %>;
    var CVFilterBy = <%=CInt(CVFilterBy)%>;

    var CanUserStartTeamTime = "<%=Bool2JS(CanUserStartTeamTime)%>";
    //var cv_sort_order = [0, "asc"];

    // actions
    var ACTION_SYNTHESIS_MODE    = "<%=ACTION_SYNTHESIS_MODE%>";
    var ACTION_GRID_WRT_STATE    = "<%=ACTION_GRID_WRT_STATE%>";
    var ACTION_SWITCH_DSA        = "<%=ACTION_SWITCH_DSA%>";
    var ACTION_SWITCH_ALTS_GRID  = "<%=ACTION_SWITCH_ALTS_GRID%>";
    var ACTION_SWITCH_OBJS_GRID  = "<%=ACTION_SWITCH_OBJS_GRID%>";
    var ACTION_SHOW_OBJ_LOCAL    = "<%=ACTION_SHOW_OBJ_LOCAL%>";
    var ACTION_SHOW_OBJ_GLOBAL   = "<%=ACTION_SHOW_OBJ_GLOBAL%>";
    var ACTION_DSA_UPDATE_VALUES = "<%=ACTION_DSA_UPDATE_VALUES%>";
    var ACTION_DSA_RESET         = "<%=ACTION_DSA_RESET%>";
    var ACTION_GRID_WRT_NODE_ID  = "<%=ACTION_GRID_WRT_NODE_ID%>";
    var ACTION_CHANGE_LAYOUT     = "<%=ACTION_CHANGE_LAYOUT%>";
    var ACTION_COMBINED_MODE     = "<%=ACTION_COMBINED_MODE%>";
    var ACTION_CIS_MODE          = "<%=ACTION_CIS_MODE%>";
    var ACTION_USER_WEIGHTS_MODE = "<%=ACTION_USER_WEIGHTS_MODE%>";
    var ACTION_NORMALIZE_MODE    = "<%=ACTION_NORMALIZE_MODE%>";
    var ACTION_DECIMALS          = "<%=ACTION_DECIMALS%>";
    var ACTION_SET_WIDGET        = "<%=ACTION_SET_WIDGET%>";
    var ACTION_SELECTED_USER     = "<%=ACTION_SELECTED_USER%>";
    var ACTION_SELECTED_USERS    = "<%=ACTION_SELECTED_USERS%>";
    var ACTION_SELECTED_4ASA_USER= "<%=ACTION_SELECTED_4ASA_USER%>";
    var ACTION_ALTS_FILTER       = "<%=ACTION_ALTS_FILTER%>";
    var ACTION_REFRESH_CV        = "<%=ACTION_REFRESH_CV%>";
    var ACTION_SHOW_ALL_ROWS_CV  = "<%=ACTION_SHOW_ALL_ROWS_CV%>";
    var ACTION_FILTER_ROWS_CV    = "<%=ACTION_FILTER_ROWS_CV%>";
    var ACTION_CHECKED_NODES_CV  = "<%=ACTION_CHECKED_NODES_CV%>";
    var ACTION_FILTER_BY_CV      = "<%=ACTION_FILTER_BY_CV%>";
    var ACTION_RESET_MIXED_VIEW  = "<%=ACTION_RESET_MIXED_VIEW%>";
    var ACTION_4ASA_KEEP         = "<%=ACTION_4ASA_KEEP%>";
    var ACTION_4ASA_NEXT_PORTION = "<%=ACTION_4ASA_NEXT_PORTION%>";
    var ACTION_NODE_COLOR_SET    = "<%=ACTION_NODE_COLOR_SET%>";
    var ACTION_NODES_COLORS_RESET= "<%=ACTION_NODES_COLORS_RESET%>";

    // Notmalization Modes
    var ntNormalizedForAll = <%=LocalNormalizationType.ntNormalizedForAll %>;
    var ntNormalizedMul100 = <%=LocalNormalizationType.ntNormalizedMul100 %>;
    var ntNormalizedSum100 = <%=LocalNormalizationType.ntNormalizedSum100 %>;
    var ntUnnormalized     = <%=LocalNormalizationType.ntUnnormalized     %>;

    var NormalizeMode = <%=CInt(PM.Parameters.Normalization) %>;
    var showComponents = <%=Bool2JS(Api.ShowComponents)%>;

    var AlternativesFilterValue  = <%=AlternativesFilterValue%>;
    var AlternativesAdvancedFilterUserID = <%=AlternativesAdvancedFilterUserID%>;
    var AlternativesAdvancedFilterValue  = <%=AlternativesAdvancedFilterValue%>;

    var SelectOnlyCoveringObjectivesOption = <%=CInt(SelectOnlyCoveringObjectivesOption)%>;

    var old_filter_val = AlternativesFilterValue;

    var urlHeatMap = "<%=GetHeatMapUrl()%>";
    var urlBowTie = "<%=GetBowTieUrl()%>";

    /* Filtering by alt attributes vars area */
    <% = LoadAttribData() %>
    var aidx_id = 0;
    var aidx_name = 1;
    var aidx_type = 2;
    var aidx_vals = 3;
    
    var fidx_chk = 0;
    var fidx_id = 1;
    var fidx_oper = 2;
    var fidx_val = 3;

    var avtString = 0;
    var avtBoolean = 1;
    var avtLong = 2;
    var avtDouble = 3;
    var avtEnumeration = 4;
    var avtEnumerationMulti = 5;

    var flt_max_id = -1;
    var oper_list = ["Contains", "Equal", "Not Equal", "Starts With", "Greater Than", "Greater Than Or Equal", "Less Than", "Less Than Or Equal", "<%=ResString("lblYes")%>", "<%=ResString("lblNo")%>"];   
    var oper_available = [[0,1,2,3],
                          [8,9],
                          [1,2,4,5,6,7],
                          [1,2,4,5,6,7],
                          [1,2],
                          [0,1,2]];

    /* Filtering by User Attributes */
    var attr_data = [<%=GetUsersAttribData()%>];   // participant attributes
    var attr_user_flt = [<%=GetUsersFilterData()%>];    // current filters list

    var DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID = "<%=DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID.ToString%>";

    <%--function onSetPage(pgid) {
        return false;
        var res = false;
        var saType = "";
        switch (pgid % _pgid_max_mod) {
            case <% =_PGID_ANALYSIS_2D %>:
                saType = "2D";
                break;
            case <% =_PGID_ANALYSIS_DSA %>:
                saType = "DSA";
                break;
            case <% =_PGID_ANALYSIS_PSA %>:
                saType = "PSA";
                break;
            case <% =_PGID_ANALYSIS_HEAD2HEAD %>:
                saType = "HTH";
                break;
            case <% =_PGID_ANALYSIS_GSA %>:
                saType = "GSA";
                break;
        };
        if (saType != "") {
            pgID = pgid;
            $('canvas.DSACanvas').sa("option", "viewMode", saType);
            $('canvas.DSACanvas').sa("option", "PSAShowLines", true);
            $('canvas.DSACanvas').sa("redrawSA");
            res = true;
        };
            return res;
        }--%>

    /* jQuery Ajax */
    function syncReceived(params) {
        if ((params)) {
            var received_data = eval(params);
            if ((received_data)) {
                var cmd = "action=" + received_data[0];
                if (IsAction(ACTION_DSA_UPDATE_VALUES, cmd) || IsAction(ACTION_SELECTED_USER, cmd) || IsAction(ACTION_SELECTED_USERS, cmd) || IsAction(ACTION_GRID_WRT_NODE_ID, cmd) || IsAction(ACTION_SYNTHESIS_MODE, cmd) || IsAction(ACTION_GRID_WRT_STATE, cmd) || IsAction(ACTION_COMBINED_MODE, cmd) || IsAction(ACTION_CIS_MODE, cmd) || IsAction(ACTION_USER_WEIGHTS_MODE, cmd) || IsAction("include_ideal", cmd) || IsAction(ACTION_DECIMALS, cmd) || IsAction(ACTION_ALTS_FILTER, cmd) || IsAction(ACTION_REFRESH_CV, cmd) || IsAction(ACTION_SHOW_ALL_ROWS_CV, cmd) || IsAction(ACTION_FILTER_ROWS_CV, cmd) || IsAction(ACTION_CHECKED_NODES_CV, cmd) || IsAction(ACTION_FILTER_BY_CV, cmd) || IsAction(ACTION_NORMALIZE_MODE, cmd) || IsAction(ACTION_SELECTED_4ASA_USER,cmd) || IsAction("select_events", cmd) || IsAction(ACTION_4ASA_NEXT_PORTION, cmd) || IsAction(ACTION_NODE_COLOR_SET, cmd) || IsAction(ACTION_NODES_COLORS_RESET, cmd) || IsAction("create_dyn_res_group", cmd) || IsAction("get_filtered_users_data", cmd) || IsAction("switch_show_priorities", cmd) || IsAction("sa_alts_parameter", cmd) || IsAction("show_sim_results", cmd) || IsAction("sa_alts_sort_by", cmd) || IsAction("refresh", cmd) /*|| IsAction("dashboard_simple_mode", cmd)*/ || IsAction("show_likelihoods_given_sources", cmd)) {
                    if (SynthesisView != vtCV) {
                        //if (IsAction(ACTION_DSA_UPDATE_VALUES, cmd)) {                
                        //    var canvasName = received_data[1];
                        //    $("#" + canvasName).sa("GradientMinValues", received_data[2]);
                        //}

                        if (IsAction(ACTION_SELECTED_USER, cmd) || IsAction(ACTION_SELECTED_USERS, cmd) || IsAction(ACTION_USER_WEIGHTS_MODE, cmd) || IsAction("include_ideal", cmd) || IsAction("create_dyn_res_group", cmd) || IsAction("refresh", cmd)) {
                            cv_tree_nodes_data = received_data[16];
                        }

                        if (IsAction(ACTION_GRID_WRT_NODE_ID, cmd) || IsAction(ACTION_NORMALIZE_MODE, cmd) || IsAction(ACTION_SELECTED_USER, cmd) || IsAction(ACTION_SELECTED_USERS, cmd) || IsAction(ACTION_SYNTHESIS_MODE, cmd) || IsAction(ACTION_GRID_WRT_STATE, cmd) || IsAction(ACTION_COMBINED_MODE, cmd) || IsAction(ACTION_CIS_MODE, cmd) || IsAction(ACTION_USER_WEIGHTS_MODE, cmd) || IsAction("include_ideal", cmd) || IsAction(ACTION_DECIMALS, cmd) || IsAction(ACTION_ALTS_FILTER, cmd) || IsAction(ACTION_CHECKED_NODES_CV, cmd) || IsAction(ACTION_SELECTED_4ASA_USER, cmd) || IsAction("select_events", cmd) || IsAction("sa_alts_parameter", cmd) || IsAction("show_sim_results", cmd) || IsAction("refresh", cmd) || isAction("show_likelihoods_given_sources", cmd)) {
                            sa_objs = received_data[1];
                            sa_alts = received_data[2];
                            alts_priorities = received_data[3].alts_priorities;
                            objs_priorities = received_data[3].objs_priorities;
                            expected_values = received_data[3].expected_values;
                            sa_comps = received_data[9];
                            //chart_datasource = received_data[10];
                            //chart_datasource_pie = received_data[11];
                            //chart_datasource_comp = received_data[12];
                            //chart_datasource_obj = received_data[13];
                            //chart_datasource_obj_pie = received_data[14];
                            //chart_datasource_obj_comp = received_data[15];
                            //chart_datasource_sunburst = received_data[17];
                            sa_username = received_data[18];
                            sa_userid = received_data[19];
                            var altsFilterValue = received_data[5];
                            $('canvas.DSACanvas').sa("option", "altFilterValue", altsFilterValue); 

                            if (IsAction("refresh", cmd)) {
                                $('canvas.DSACanvas').sa("option", "normalizationMode", getNormModeSA());
                            }

                            asa_data = received_data[8];
                            if (SynthesisView == vt4ASA) {
                                $('#DSACanvas0').sa("option", "ASAdata", asa_data[0]);
                                $('#DSACanvas0').sa("option", "userID", asa_data[0].userID);
                                $('#DSACanvas0').sa("option", "userName", getUserByID(asa_data[0].userID)[IDX_USER_NAME]);
                                $('#DSACanvas1').sa("option", "ASAdata", asa_data[1]);
                                $('#DSACanvas1').sa("option", "userID", asa_data[1].userID);
                                $('#DSACanvas1').sa("option", "userName", getUserByID(asa_data[1].userID)[IDX_USER_NAME]);
                                $('#DSACanvas2').sa("option", "ASAdata", asa_data[2]);
                                $('#DSACanvas2').sa("option", "userID", asa_data[2].userID);
                                $('#DSACanvas2').sa("option", "userName", getUserByID(asa_data[2].userID)[IDX_USER_NAME]);
                                $('#DSACanvas3').sa("option", "ASAdata", asa_data[3]);
                                $('#DSACanvas3').sa("option", "userID", asa_data[3].userID);
                                $('#DSACanvas3').sa("option", "userName", getUserByID(asa_data[3].userID)[IDX_USER_NAME]);
                            } else {
                                $('canvas.DSACanvas').sa("option", "ASAdata", asa_data); 
                                $('canvas.DSACanvas').sa("option", "userID", sa_userid); 
                                $('canvas.DSACanvas').sa("option", "userName", sa_username);
                            }          
                            $('canvas.DSACanvas').sa("option", "objs", sa_objs); 
                            $('canvas.DSACanvas').sa("option", "alts", sa_alts);  
                            $('canvas.DSACanvas').sa("option", "DSAComponentsData", sa_comps);
                            if (SynthesisView == vt4ASA || SynthesisView == vtASA) {
                                $('canvas.DSACanvas').sa("refreshASA");
                            } else {
                                $('canvas.DSACanvas').sa("resetSA");
                            }

                            resetAllGrids();                            
                            updateToolbar();
                            updateInfobar();

                            if (IsAction(ACTION_SELECTED_USER, cmd) || IsAction(ACTION_SELECTED_USERS, cmd) || IsAction(ACTION_DECIMALS, cmd) || IsAction("refresh", cmd) || IsAction(ACTION_CIS_MODE, cmd)) {
                                if (IsAction(ACTION_CIS_MODE, cmd)) cv_tree_nodes_data = received_data[22];
                                initCVTree("cvTree-1");
                            }

                        }

                        // 4 ASA
                        if (IsAction(ACTION_SELECTED_4ASA_USER, cmd) || IsAction(ACTION_4ASA_NEXT_PORTION, cmd)) {
                            if (IsAction(ACTION_4ASA_NEXT_PORTION, cmd)) {
                                asa_data = received_data[1];
                            } else {
                                asa_data = received_data[8];
                            }
                            
                            $('#DSACanvas0').sa("option", "ASAdata", asa_data[0]).sa("option", "userID", asa_data[0].userID).sa("option", "userName", getUserByID(asa_data[0].userID)[IDX_USER_NAME]).sa("resetSA");
                            $('#DSACanvas1').sa("option", "ASAdata", asa_data[1]).sa("option", "userID", asa_data[1].userID).sa("option", "userName", getUserByID(asa_data[1].userID)[IDX_USER_NAME]).sa("resetSA");
                            $('#DSACanvas2').sa("option", "ASAdata", asa_data[2]).sa("option", "userID", asa_data[2].userID).sa("option", "userName", getUserByID(asa_data[2].userID)[IDX_USER_NAME]).sa("resetSA");
                            $('#DSACanvas3').sa("option", "ASAdata", asa_data[3]).sa("option", "userID", asa_data[3].userID).sa("option", "userName", getUserByID(asa_data[3].userID)[IDX_USER_NAME]).sa("resetSA");
                            $('canvas.DSACanvas').sa("resetSA");
                        }

                        if (IsAction(ACTION_4ASA_NEXT_PORTION, cmd)) {
                            $('#cb4ASAUsers0').val(asa_data[0].userID + "");
                            $('#cb4ASAUsers1').val(asa_data[1].userID + "");
                            $('#cb4ASAUsers2').val(asa_data[2].userID + "");
                            $('#cb4ASAUsers3').val(asa_data[3].userID + "");
                        }
                
                        if (IsAction(ACTION_SELECTED_USERS, cmd) || IsAction("create_dyn_res_group", cmd)) {
                            columns_user_ids = received_data[6];
                        
                            alts_priorities = received_data[3].alts_priorities;
                            objs_priorities = received_data[3].objs_priorities;
                            expected_values = received_data[3].expected_values;

                            if (IsAction("create_dyn_res_group", cmd) && received_data.length >= 20) {
                                groups_data = received_data[21];
                            }
                            
                            initWidgets();
                        }
                        
                        if (IsAction(ACTION_SYNTHESIS_MODE, cmd) || IsAction(ACTION_GRID_WRT_STATE, cmd) || IsAction(ACTION_COMBINED_MODE, cmd) || IsAction(ACTION_CIS_MODE, cmd) || IsAction(ACTION_USER_WEIGHTS_MODE, cmd) || IsAction("include_ideal", cmd) || IsAction(ACTION_DECIMALS, cmd) || IsAction(ACTION_ALTS_FILTER, cmd) || IsAction("select_events", cmd) || IsAction("enable_alts", cmd) || IsAction("switch_show_priorities", cmd) || IsAction("refresh", cmd)) {
                            columns_user_ids = received_data[6];

                            alts_priorities = received_data[3].alts_priorities;
                            objs_priorities = received_data[3].objs_priorities;
                            expected_values = received_data[3].expected_values;

                            NormalizeMode = received_data[20];
                            var cbNO = document.getElementById("cbNormalizeOptions");
                            if ((cbNO)) cbNO.value = NormalizeMode;

                            resetAllGrids();
                        }

                        if (IsAction(ACTION_USER_WEIGHTS_MODE, cmd) || IsAction("include_ideal", cmd) || IsAction("refresh", cmd)) {
                            initCVTree("cvTree-1");
                        }

                        if ((IsAction(ACTION_NODE_COLOR_SET, cmd) && <%= Bool2JS(Not IsSensitivityView()) %>) || IsAction(ACTION_NODES_COLORS_RESET, cmd)) {
                            alts_data = received_data[1];
                            nodes_data =  received_data[2];    
                            
                            sa_objs = received_data[3];
                            sa_alts = received_data[4];
                            asa_data = received_data[5];

                            //sa_comps = received_data[9];
                            //chart_datasource = received_data[10];
                            //chart_datasource_pie = received_data[11];
                            //chart_datasource_comp = received_data[12];
                            //chart_datasource_obj = received_data[13];
                            //chart_datasource_obj_pie = received_data[14];
                            //chart_datasource_obj_comp = received_data[15];
                            //chart_datasource_sunburst = received_data[17];

                            // reset the grids
                            resetAllGrids();
                    
                            // reset sensitivities
                            if (SynthesisView == vt4ASA) {
                                $('#DSACanvas0').sa("option", "ASAdata", asa_data[0]);
                                $('#DSACanvas0').sa("option", "userID", asa_data[0].userID);
                                $('#DSACanvas0').sa("option", "userName", getUserByID(asa_data[0].userID)[IDX_USER_NAME]);
                                $('#DSACanvas1').sa("option", "ASAdata", asa_data[1]);
                                $('#DSACanvas1').sa("option", "userID", asa_data[1].userID);
                                $('#DSACanvas1').sa("option", "userName", getUserByID(asa_data[1].userID)[IDX_USER_NAME]);
                                $('#DSACanvas2').sa("option", "ASAdata", asa_data[2]);
                                $('#DSACanvas2').sa("option", "userID", asa_data[2].userID);
                                $('#DSACanvas2').sa("option", "userName", getUserByID(asa_data[2].userID)[IDX_USER_NAME]);
                                $('#DSACanvas3').sa("option", "ASAdata", asa_data[3]);
                                $('#DSACanvas3').sa("option", "userID", asa_data[3].userID);
                                $('#DSACanvas3').sa("option", "userName", getUserByID(asa_data[3].userID)[IDX_USER_NAME]);
                            } else {
                                $('canvas.DSACanvas').sa("option", "ASAdata", asa_data); 
                            };
                            // $('canvas.DSACanvas').sa("option", "objs", sa_objs); 
                            $('canvas.DSACanvas').sa("option", "alts", sa_alts); 
                            if (SynthesisView == vt4ASA || SynthesisView == vtASA) {
                                $('canvas.DSACanvas').sa("refreshASA");
                            } else {
                                $('canvas.DSACanvas').sa("resetSA");
                            };                           
                        }

                        // update alternatives grids when changing the normalize mode
                        if (IsAction(ACTION_NORMALIZE_MODE, cmd) || IsAction("refresh", cmd)) {
                            alts_priorities = received_data[3].alts_priorities;
                            objs_priorities = received_data[3].objs_priorities;
                            expected_values = received_data[3].expected_values;

                            $("div.alts_table").each( function (index, val) { getAlternativesGridHTML($(val).attr("wid")*1) });
                        }

                    } else {
                        // Consensus View only actions
                        if (IsAction(ACTION_REFRESH_CV, cmd) || IsAction(ACTION_SHOW_ALL_ROWS_CV, cmd) || IsAction(ACTION_FILTER_ROWS_CV, cmd) || IsAction(ACTION_CHECKED_NODES_CV, cmd) || IsAction(ACTION_FILTER_BY_CV, cmd) || IsAction(ACTION_ALTS_FILTER, cmd)) {
                            cv_data = received_data[1];
                            refreshCVGrid();
                        }
                        // if need to rebuild columns
                        if (IsAction(ACTION_FILTER_BY_CV, cmd)) {
                            cv_data = received_data[1];
                            rebuildCVGrid();
                        }
                    }
                }
                if (IsAction("show_users_count_in_header", cmd) || IsAction("show_users_with_jdgm_in_header", cmd) || IsAction("show_users_with_all_jdgm_header", cmd)) {
                    groups_data = received_data[1];
                    resetAllGrids();
                }
                if (IsAction("legends_visibility", cmd)) {

                }
                //if (IsAction("dashboard_simple_mode", cmd)) {
                //    initLayout();
                //    changeLayout(selectedLayout);
                //    initWidgets();
                //}
                if (IsAction("asa_page_size", cmd)) {
                    asa_pagesize = received_data[1]*1;
                    cbASAPageNumChange(1); 
                    updatePageList(asa_pagesize);
                    $('canvas.DSACanvas').sa("option", "ASAPageSize", asa_pagesize);
                    $('canvas.DSACanvas').sa("refreshASA");
                }
                if (IsAction("asa_page_num", cmd)) {
                    var value = received_data[1]*1;
                    $('canvas.DSACanvas').sa("option", "ASACurrentPage", value);
                    $('canvas.DSACanvas').sa("refreshASA");
                }
                if (IsAction("asa_sort_by", cmd)) {
                    var value = received_data[1]*1;
                    $('canvas.DSACanvas').sa("option", "ASASortBy", value);
                    if (SynthesisView == vt4ASA || SynthesisView == vtASA) {
                        $('canvas.DSACanvas').sa("refreshASA");
                    } else {
                        $('canvas.DSACanvas').sa("redrawSA");
                    };
                    displayLoadingPanel(false);
                }
                if (IsAction("sa_alts_sort_by", cmd)) {
                    var value = received_data[1]*1;
                    $('canvas.DSACanvas').sa("option", "SAAltsSortBy", value);
                    $('canvas.DSACanvas').sa("option", "alts", sa_alts); 
                    $('canvas.DSACanvas').sa("applyAltsSorting");
                    $('canvas.DSACanvas').sa("redrawSA");
                    if (SynthesisView == vt4ASA || SynthesisView == vtASA) {
                        $('canvas.DSACanvas').sa("refreshASA");
                    } else {
                        $('canvas.DSACanvas').sa("resetSA");
                    };
                    if (SynthesisView == vtAlternativesChart) {    
                        displayLoadingPanel(false);
                    };
                }
                if (IsAction("get_filtered_users_data", cmd)) {
                    filtered_users_columns = received_data[1];
                    filtered_users_data = received_data[2];
                    drawUsersFilteredTable();
                }
                if (IsAction(ACTION_DECIMALS, cmd)) {
                    initCVTree("cvTree-1");
                }
            }
        }        
        
        displayLoadingPanel(false);

        refreshSelectedUsersLabel();
        resizePage();
    }    

    function resetAllGrids() {
        $("div.alts_table").each( function (index, val) { getAlternativesGridHTML($(val).attr("wid")*1) });
        $("div.objs_table").each( function (index, val) { getObjectivesGridHTML($(val).attr("wid")*1) });
    }

    function refreshSelectedUsersLabel() {
        var lblSelectedCount = document.getElementById("lblSelectedCount");
        if ((lblSelectedCount)) {
            lblSelectedCount.innerHTML = columns_user_ids.length + "/" + (users_data.length + groups_data.length);
        }
    }
    
    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }

    function sendCommand(params, showPleaseWait) {
        if (showPleaseWait) displayLoadingPanel(true);
        
        callAjax(params, syncReceived, undefined, !showPleaseWait);        
    }

    function initPage() {
        initLayout();
        //initLayoutMenu();
        updateToolbar();
        updateInfobar();
        setWidgetContent(vtTree, -1, true);
        updateWRTState();
        updatePageList(asa_pagesize);
        if (SynthesisView == vtMixed || SynthesisView == vtDashboard) {
            changeLayout(selectedLayout); 
            initWidgets(false);
        } else {
            if (SynthesisView != vt4ASA) changeLayout(100);
            initSingleModeWidget();
            $(".widget_cell").css({"border" : 0, "padding" : 0, "box-shadow" : "0px 0px 0px white"});
        }
        DisableAnalysisPagesOnAIP(<%=Bool2JS(App.ActiveProject.ProjectManager.CalculationsManager.CombinedMode <> CombinedCalculationsMode.cmAIJ)%>, false);
    }

    function resizePage(force, w, h) {
        $("#drawer").width(100);
        $("#drawer").width($("#tdDrawer").width()<% = If(isSLTheme, "-5", "") %>);
        //$("#drawer").height(150);
        //$("#drawer").height($("#main_toolbar").height());
        if ($(".dx-drawer-panel-content").length) {
            if ($("#drawer").data("dxDrawer")) {
                if ($("#drawer").dxDrawer("instance").option("opened")) {
                    $(".dx-drawer-panel-content").height($("#main_toolbar").height());
                } else {
                    $(".dx-drawer-panel-content").height(0);
                }
            }
        }
        resizeWidgets();
        setTimeout(function () { resizeWidgetsContent(); }, 50);
    }

    // Layouts / Widgets Management 
    function initLayout() {
        $("#divContent").empty();
        var d = document.getElementById("divContent");

        for (var i = 0; i < maxWidgets; i++) {            
            var cell =  document.createElement("div");
            cell.className = "widget_cell";
            cell.id = "divWidget" + i;
            cell.innerHTML = getWidgetDefaultContent(i);
            cell.style.display = "none";
            cell.style.overflow = "hidden";
            d.appendChild(cell);
        }    
    }

    function initLayoutMenu() {        
        if (SynthesisView != vtMixed && SynthesisView != vtDashboard) return;

        var d = document.getElementById("divLayoutMenu");
        if ((d)) {
            var menuItemWidth   = 70;
            var menuItemsInRow  = 4;
            var menuItemsMargin = 2;
            var menuWidth = 25 + (menuItemWidth + menuItemsMargin * 2) * menuItemsInRow;

            d.style.width   = menuWidth + "px";
            d.style.padding = "10px";

            for (var i = 0; i < numLayouts; i++) {
                var div = document.createElement("div");
                div.style.width   = (menuItemWidth + menuItemsMargin * 2) + "px";
                div.style.padding = menuItemsMargin + "px";
                div.style.float   = "left";
                div.innerHTML = "<a href='' onclick='changeLayout(" + availableLayouts[i] + "); initWidgets(); return false;'><img src='<% =ImagePath%>old/layoutTempl" + availableLayouts[i] + ".png' width='70' height='49' border='0'></a>";
                d.appendChild(div);
            }

            // add a 'Reset' link  
            var reset_div = document.createElement("div");
            reset_div.id = "reset_div";
            reset_div.style.dispaly = "none";
            reset_div.style.width   = menuWidth - 50 + "px";
            reset_div.style.padding = menuItemsMargin + "px";
            reset_div.style.float   = "left";
            reset_div.innerHTML = "<a href='' class='actions text' onclick='resetMixedLayout(); return false;'>Reset to defaults</a>";
            d.appendChild(reset_div);
            
            toggleResetLayoutLink();
        }
    }

    function toggleResetLayoutLink() {        
        $("#reset_div").hide();
        if ((defaultLayout != selectedLayout) || (defaultWidgets.join() != selectedWidgets.join())) {
            $("#reset_div").show();
        }
    }

    function toggleLayoutsMenu() {
        $("#popupContainer1").dxPopup({
            contentTemplate: function() {
                return $("<div><div id='divLayoutMenu'></div><div id='btnCancelDlg' style='margin-top:1em; margin-right: 4px; float: right;'></div></div>");
            },
            width: 400,
            height: 250,
            showTitle: true,
            title: "",
            dragEnabled: true,
            shading: false,
            closeOnOutsideClick: true,
            resizeEnabled: false,
            showCloseButton: true
        });
        $("#popupContainer1").dxPopup("show");
        initLayoutMenu();
        $("#btnCancelDlg").dxButton({
            text: "<% =JS_SafeString(ResString("btnClose")) %>",
            icon: "",
            type: "normal",
            width: 100,
            onClick: function (e) {
                closePopup("#popupContainer1");
            }
        });
    }

    function resetMixedLayout() {
        changeLayout(defaultLayout);
        selectedWidgets = defaultWidgets.slice();
        initWidgets();
        sendCommand("action=" + ACTION_RESET_MIXED_VIEW, false);
    }

    function getWidgetDefaultContent(index) {
        var s = "<table id='tblWidgetArea"+index+"' border='0' cellpadding='0' cellspacing='0' class='whole text'><tr style='max-height:24px;'><td style='padding-left: 4px;'><div id='divWidgetToolbar"+index+"'></div></td>";
        if ((SynthesisView == vtMixed || SynthesisView == vtDashboard || SynthesisView == vt4ASA)) { // if allow multiple layouts then allow to see the [Edit] and Minimize ([-]) hyperlinks            
            s += "<td align='right' style='width:110px;'><a href='' class='actions collapsed on_advanced widget" + index +"' style='position:absolute;right:10px;top:6px;color:#ccf;margin-right:6px;cursor:pointer;z-index:3;' onclick='toggleWidgetExpanded(this," + index + "); return false;' title='Maximize'><i class='far fa-window-maximize'></i></a>";
            if (SynthesisView == vtMixed || SynthesisView == vtDashboard) {
                s += "<a href='' class='editwidget widget" + index + " set-widget-content-link on_advanced' style='position:absolute;right:28px;top:6px;color:#ccf;margin-right:6px;cursor:pointer;z-index:3;' data-index='" + index + "' title='Edit' onclick='setWidgetContent(vtMixed, this.getAttribute(\"data-index\") * 1); return false;'><i class='fas fa-th-large'></i></a>";
            };
            s += "</td>";
        }                
        s += "</tr><tr id='trWidgetContent" + index + "' style='height:100%;'><td colspan='2' valign='top' style='text-align:center;'><div id='divInnerContent" + index + "' style='overflow: auto; height: 100%;'>"; //"Content " + index + "<br>";
        
        s += "<div id='divDefaultWidgetMenu" + index + "' style='display: none;'>";
        if (SynthesisView == vtMixed || SynthesisView == vtDashboard) {
            s += "<a href='' class='actions' onclick='setWidgetContent(vtAlternatives, " + index + "); return false; '><%=ParseString("%%Alternatives%%")%> Grid</a><br>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vtObjectives, " + index + "); return false; '><%=ParseString("%%Objectives%%")%> Grid</a><br>";
            <%--s += "<a href='' class='actions' onclick='setWidgetContent(vtAlternativesChart, " + index + "); return false; '><%=ParseString("%%Alternatives%%")%> Chart</a><br>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vtObjectivesChart, " + index + "); return false; '><%=ParseString("%%Objectives%%")%> Chart</a><br>";--%>
            s += "<a href='' class='actions' onclick='setWidgetContent(vtDSA, " + index + "); return false; '>Dynamic Sensitivity</a><br>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vtPSA, " + index + "); return false; '>Performance Sensitivity</a><br>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vtGSA, " + index + "); return false; '>Gradient Sensitivity</a><br>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vt2D,  " + index + "); return false; '>2D Sensitivity</a><br>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vtHTH, " + index + "); return false; '>Head to Head Analysis</a><br>";
            <%--s += "<a href='' class='actions' onclick='setWidgetContent(vtASA, " + index + "); return false; '>Sensitivity Δ <%=ParseString("%%Alternatives%%")%> (One at a time)</a><br>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vt4ASA," + index + "); return false; '>Sensitivity Δ <%=ParseString("%%Alternatives%%")%> (Four at a time)</a><br>";--%>
            <%If PM.IsRiskProject Then%>
            s += "<hr style='width: 100px;'></hr>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vtBowTie,  " + index + "); return false; '><%=ParseString("Bow-Tie")%></a><br>";
            s += "<a href='' class='actions' onclick='setWidgetContent(vtHeatMap, " + index + "); return false; '><%=ParseString("%%Risk%% Head Map")%></a><br>";
            <%End If%>
        }
        s += "</div>";
        s += "<div id='divDefaultWidgetLoading" + index + "' style='display: none; cursor: wait; vertical-align: middle; height: 100%;'>";
        s += "<img src='<%=ImagePath%>old/wait.gif' style='margin: auto; vertical-align: middle;' alt='<%=ResString("htmlPleaseWait") %>' title='<%=ResString("htmlPleaseWait") %>' >";
        s += "</div>";
        return s + "</div></td></tr></table>";
    }

    function saveLayout(value) {
        selectedLayout = value;
        toggleResetLayoutLink();
        sendCommand("action=" + ACTION_CHANGE_LAYOUT + "&value=" + value);
    }

    function changeLayout(value) {                
        if (selectedLayout != value && (SynthesisView == vtMixed || SynthesisView == vtDashboard)) saveLayout(value);        
        $("div.widget_cell").hide();
        
        closePopup("#popupContainer1");

        var img = document.getElementById("imgToggleLayout");
        if ((img)) img.src = "<% =ImagePath%>old/layoutTempl" + value + ".png";

        resizeWidgets();
        setTimeout(function () { resizeWidgetsContent(); }, 50);
    }

    function resizeWidgets() {
        var tdContent = document.getElementById("tdContent");
        if (!tdContent) return false;

        $("div.widget_cell").width(10).height(10);
        $('canvas.DSACanvas').width(10).height(10);

        var pw = function (name, wl, wt, ww, wh) {
            var wdg = document.getElementById(name);
            if ((wdg)) {
                wdg.className = "widget_cell";
                wdg.style.position = "absolute";
                //wdg.style.margin = 0;
                wdg.style.left = wl + "px";
                wdg.style.top = wt + "px";
                wdg.style.width = ww + "px";
                wdg.style.height = wh + "px";
                wdg.style.display = "block";
            }
            return wdg;
        };

        var tree = document.getElementById("divTree");
        var content = document.getElementById("divContent");
        $(content).width(10);

        var width = tdContent.clientWidth<% =IIf(isSLTheme, "-5", "") %>;
        var height = tdContent.clientHeight;  
        var margin = 2;
        

        var treerect = tree.getBoundingClientRect();

        if (!showHierarchyTree) {
            var leftCon = document.getElementById("divLeftCon");
            treerect = leftCon.getBoundingClientRect();
            treerect.right = treerect.left;
        }

        //content.style.left = treerect.right + "px";
        //content.style.top = treerect.top + "px";
        
        var c_width = width - (treerect.right - treerect.left) - 2;
        content.style.width = c_width + "px";
        content.style.height = height + "px";

        $("#pgHeader").css("margin-left", treerect.right - treerect.left);

        tree.style.height = height + "px";

        switch (selectedLayout * 1) {
            case 0:                                                
                pw("divWidget0", 0, 0, c_width, height);
                break;
            case 1:
                var cell00width = Math.round(c_width / 2) - margin - 10;
                pw("divWidget0", 0, 0, cell00width, height);
                pw("divWidget1", cell00width + margin + 10, 0, c_width - cell00width - margin * 2 - 15, height);
                break;
            case 2:
                var cell00height = Math.round(height / 2) - margin;
                pw("divWidget0", 0, 0, c_width - 10 - 2, cell00height);
                pw("divWidget1", 0, cell00height + margin + 10, c_width - 10 - 2, height - cell00height - margin * 2 - 5);
                break;
            case 3:
                var cell00width = Math.round(c_width / 3) - margin - 10;
                pw("divWidget0", 0, 0, cell00width, height);
                pw("divWidget1", cell00width + margin + 10, 0, c_width - cell00width - margin * 2 - 15, height);
                break;
            case 4:
                var cell00width = Math.round(c_width / 3) - margin - 10;
                pw("divWidget0", 0, 0, cell00width, height);
                pw("divWidget1", cell00width + margin + 10, 0, cell00width, height);
                pw("divWidget2", cell00width * 2 + margin + 25, 0, c_width - cell00width * 2 - margin * 2 - 30, height);
                break;
            case 5:
            case 6:
            case 7:
            case 8:
                var cell00height = Math.round(height / 3) - margin * 2;
                pw("divWidget0", 0, 0, c_width - 10 - 2, cell00height);
                pw("divWidget1", 0, cell00height + margin + 10, width - 10 - 2, cell00height);
                pw("divWidget2", 0, cell00height * 2 + margin + 25, c_width - 10 - 2, height - cell00height * 2 - margin * 2 - 5 - 15);
                break;
            case 9: // 4ASA, Mixed 4x4                
                var cell00width = Math.round(c_width / 2) - margin * 4;
                var cell00height = Math.round(height / 2) - margin * 4;
                pw("divWidget0", 0, 0, cell00width, cell00height);
                pw("divWidget1", cell00width, 0, c_width - cell00width, cell00height);
                pw("divWidget2", 0, cell00height, cell00width, height - cell00height);
                pw("divWidget3", cell00width, cell00height, c_width - cell00width, height - cell00height);
                break;
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
                var cell00width = Math.round(c_width / 3) - margin - 10;
                var cell00height = Math.round(height / 2) - margin;
                pw("divWidget0", 0, 0, cell00width, cell00height);
                pw("divWidget1", cell00width + margin + 10, 0, cell00width, cell00height);
                pw("divWidget2", cell00width * 2 + margin + 25, 0, c_width - cell00width * 2 - margin * 2 - 30, cell00height);

                pw("divWidget3", 0, cell00height + margin + 10, cell00width, height - cell00height - margin * 2 - 5);
                pw("divWidget4", cell00width + margin + 10, cell00height + margin + 10, cell00width, height - cell00height - margin * 2 - 5);
                pw("divWidget5", cell00width * 2 + margin + 25, cell00height + margin + 10, c_width - cell00width * 2 - margin * 2 - 30, height - cell00height - margin * 2 - 5);
                break;
            case 100: // special layout for Consensus View
                pw("divWidget0", 0, 0, c_width, height);
                break;
        }
    }
    
    function resizeWidgetsContent() {
        if (showHierarchyTree) {
            $("#divTree").height(100);
        }

        resizeAllDSA();
        resizeAllGrids();
        resizeAllCharts();

        if (showHierarchyTree) {
            $("#divTree").height($("#tdContent").parent().height());
        }
    }

    function resizeAllGrids() {
        $(".alts_table, .objs_table, .cv_table").each(function (index, val) {
            if ($(this).data("dxDataGrid")) {
                var dg = $(this).dxDataGrid("instance");
                dg.option("width", 0);
                dg.option("height", 0);
                dg.option("width", $(this).parent().width() - 16);
                dg.option("height", $(this).parent().height());
            }
            if ($(this).data("dxTreeList")) {
                var dg = $(this).dxTreeList("instance");
                dg.option("width", 0);
                dg.option("height", 0);
                dg.option("width", $(this).parent().width());
                dg.option("height", $(this).parent().height());
            }
        });
    }

    function resizeAllDSA() {
        var dsaIDs = getDSAControlsNames();
        
        for (var i = 0; i < dsaIDs.length; i++) {
            var widget = $('#'+dsaIDs[i])[0];
            if (widget.style.display != "none" && $(widget).hasClass("widget-created")) {
                $('#'+dsaIDs[i]).width(100).height(50);
                $('#'+dsaIDs[i]).sa("resizeCanvas", 100, 50);
            }
        };

        for (var i = 0; i < dsaIDs.length; i++) {
            var widget = $('#'+dsaIDs[i])[0];
            if (widget.style.display != "none" && $(widget).hasClass("widget-created")) {
                var w = $(widget).parent()[0].clientWidth - 8;
                var h = $(widget).parent()[0].clientHeight - 4;
                if (w > 0 && h > 0) { 
                    $('#'+dsaIDs[i]).width(w).height(h);
                    $('#'+dsaIDs[i]).sa("resizeCanvas", w, h);
                }
            }
        };
    }

    function resizeAllCharts() {
        $("div.chart_placeholder_div").each(function(index, val) { 
            var obj = $(this);
            if ((this.getAttribute("data-chart-type") == "sunburst")) {
                obj.width(150).height(100);
                obj.ecChart("resize", 150, 100);
                var w = obj.parent()[0].clientWidth;
                var h = obj.parent()[0].clientHeight;
                obj.width(w).height(h);
                obj.ecChart("resize", w, h);
            } else {
                var chart; 
                if (obj.data("dxPolarChart")) chart = obj.dxPolarChart("instance");
                if (obj.data("dxPieChart")) chart = obj.dxPieChart("instance");
                if (obj.data("dxChart")) chart = obj.dxChart("instance");
                if ((chart)) {
                    obj.hide();
                    var w = obj.parent()[0].clientWidth;
                    var h = obj.parent()[0].clientHeight;
                    obj.show();
                    chart.option("width", w);
                    chart.option("height", h);
                    chart.render();
                }
            }
        });
    }

    function initWidgets(do_init_tree) {
        if (SynthesisView == vtMixed || SynthesisView == vtDashboard) {
            for (var i = 0; i < selectedWidgets.length; i++){
                setWidgetContent(selectedWidgets[i], i, true);
            }            
        } else {
            initSingleModeWidget();
        }
        if (typeof do_init_tree == "undefined" || do_init_tree) { initCVTree("cvTree-1"); }
    }

    var activeWidgets = [];
    function initSingleModeWidget() {                      
        if (SynthesisView == vt4ASA) {
            selectedLayout = 9;
            activeWidgets = [];
            if (user_ids_4asa[0] != UNDEFINED_INTEGER_VALUE) {
                selectedLayout = 0; // single widget layout
                activeWidgets.push(0);
            }
            if (user_ids_4asa[1] != UNDEFINED_INTEGER_VALUE) {
                selectedLayout = 1; // 50% + 50% widget layout
                activeWidgets.push(1);                
            }
            if (user_ids_4asa[2] != UNDEFINED_INTEGER_VALUE) {
                selectedLayout = 7; // 3 widgets
                activeWidgets.push(2);
            }
            if (user_ids_4asa[3] != UNDEFINED_INTEGER_VALUE) {
                selectedLayout = 9; // 2 x 2
                activeWidgets.push(3);
            }
            setTimeout(function () { changeLayout(selectedLayout); }, 50);
            setTimeout(function () { for (var i = 0; i < activeWidgets.length; i++) {setWidgetContent(vt4ASA, activeWidgets[i], true);} }, 150);
        } else {
            setWidgetContent(SynthesisView, 0, true);
        }
    }

    var collapsed_w = 0.0;
    var collapsed_h = 0.0;
    var frmAssess = null;

    function toggleWidgetExpanded(btn, widget_index) {
        //if (btn.className == "actions collapsed widget" + widget_index) {
        if (btn.className.indexOf("collapsed")>0) {
            btn.innerHTML = "<i class='far fa-window-close'></i>";
            //btn.className = "actions expanded widget" + widget_index;
            $(btn).removeClass("collapsed").addClass("expanded");
            btn.title = "Close";

            collapsed_w = $("#tblWidgetArea" + widget_index).width();
            collapsed_h = $("#tblWidgetArea" + widget_index).height();
            
            //var w = Math.round($(window).width()-96);
            //var h = Math.round($(window).height()-96);
            var w = dlgMaxWidth(1600)-16;
            var h = dlgMaxHeight(1200)-48;
            var conf_options = {
                showTitle: false,
                dragEnabled: false,
                message: "<div id='divContentExpanded' style='width:" + w + "px; height:" + h + "px;'></div>", 
                showCloseButton: false,
                buttons: [{
                    text: resString("btnClose"),
                    type: "default",
                    elementAttr: { class: "button_esc"},
                    onClick: function(e) {
                        toggleWidgetExpanded(btn, widget_index);
                    }
                }]
            };
            frmAssess = DevExpress.ui.dialog.custom(conf_options);
            frmAssess.show();

            $("#tblWidgetArea" + widget_index).appendTo($("#divContentExpanded"));
            
            $('#DSACanvas' + widget_index).width(100).height(100);
            
            expanded_w = $("#divInnerContent" + widget_index).width();
            expanded_h = $("#divInnerContent" + widget_index).height();

            $('#DSACanvas' + widget_index).width(expanded_w).height(expanded_h);
            $('#DSACanvas' + widget_index).sa("resizeCanvas", expanded_w, expanded_h);

            //$('.actions.maximized.widget' + widget_index).hide();
            //$('.actions.minimized.widget' + widget_index).hide();

            $(".editwidget.widget" + widget_index + ".set-widget-content-link").hide();

            //resizeAltsGrid("tableAlts"+widget_index);
            //resizeGrid("tableObjs"+widget_index);
            resizeAllGrids();
            //resizeGrid("tableCV"+widget_index);
        } else {                        
            btn.innerHTML = "<i class='far fa-window-maximize'></i>";
            //btn.className = "actions collapsed widget" + widget_index;
            $(btn).removeClass("expanded").addClass("collapsed");
            btn.title = "Maximize";

            $("#tblWidgetArea" + widget_index).appendTo($("#divWidget" + widget_index));
           
            if ((frmAssess)) frmAssess.hide();

            $("#DSACanvas" + widget_index).width(10).height(10);
            
            var w = $("#divInnerContent" + widget_index).width();
            var h = $("#divInnerContent" + widget_index).height();
            
            $('#DSACanvas' + widget_index).width(w).height(h);
            $('#DSACanvas' + widget_index).sa("resizeCanvas",w, h);

            //$('.actions.maximized.widget' + widget_index).show();
            //$('.actions.minimized.widget' + widget_index).show();

            $(".editwidget.widget" + widget_index + ".set-widget-content-link").show();
            
            //resizeAltsGrid("tableAlts"+widget_index);
            //resizeGrid("tableObjs"+widget_index);
            resizeAllGrids();
            //resizeGrid("tableCV"+widget_index);
        }        
    }

    function setWidgetDefaultContent(widget_index) {
        $("#divWidget" + widget_index).html(getWidgetDefaultContent(widget_index));
        $("#divDefaultWidgetMenu" + widget_index).show();
    }

    function setWidgetContent(view, widget_index, skip_save_widgets) {
        if (widget_index >= 0) {    
            $("#divDefaultWidgetLoading" + widget_index).show();

            if ((typeof skip_save_widgets) == 'undefined' || !skip_save_widgets) {
                selectedWidgets[widget_index] = view;
                toggleResetLayoutLink();
                sendCommand('action=' + ACTION_SET_WIDGET + '&value=' + view + '&index=' + widget_index);
            }
        }

        $("#divInnerContent" + widget_index).empty().removeClass().addClass("whole").attr("wid", widget_index);

        switch (view) {
            case vtMixed:
            case vtDashboard:
                setWidgetDefaultContent(widget_index);
                break;
            case vtAlternatives:
                // init widget toolbar                
                var buttons = ""; //"<select style='width: 1px;opacity:0;' onclick='return false;'></select>";
                <%--"<a href='' style='cursor: pointer; position: absolute; margin-left: 7px; z-index: 101;' onclick='btnGridSettingsClick(); return false;'><img style='vertical-align: top; margin-top: 2px;' src='<% =ImagePath %>old/settings-16.png' alt='<%=ResString("lblRiskSettings") %>' title='<%=ResString("lblRiskSettings") %>' ></a>";--%>
                $("#divWidgetToolbar" + widget_index).html(buttons);

                $("#divInnerContent" + widget_index).addClass("alts_table");
                $("#divInnerContent" + widget_index).html("<div id='tableAlts" + widget_index + "' wid='" + widget_index + "' class='alts_table' style='margin: 0px; width: 0;'></div>");
                getAlternativesGridHTML(widget_index);
                break;
            case vtObjectives:
                // init widget toolbar                
                var buttons = "<a href='' id='btnObjLocal"+widget_index+"' class='actions' onclick='switchObjPriorities("+widget_index+",0); return false;' style='border:1px solid <% = If(PM.Parameters.Synthesis_ObjectivesPrioritiesMode = 0, "#ccc", "#fff")%>; padding:2px;'>Local</a>"; //&nbsp;|&nbsp;";
                buttons += "<a href='' id='btnObjGlobal"+widget_index+"' class='actions' onclick='switchObjPriorities("+widget_index+",1); return false;' style='border:1px solid <% = If(PM.Parameters.Synthesis_ObjectivesPrioritiesMode > 0, "#ccc", "#fff")%>; padding:2px;'>Global</a>";
                buttons += "<select class='select' style='width:220px; margin-right:7px; margin-left:7px;' onchange='cbObjDisplayChange(this.value, " + widget_index + ");'>";
                buttons += "<option value='0' "+(SelectOnlyCoveringObjectivesOption == 0 ? "selected='selected'" :  "") + "><%=ResString("optObjShowChildren")%></option>";
                buttons += "<option value='1' "+(SelectOnlyCoveringObjectivesOption == 1 ? "selected='selected'" :  "") + "><%=ResString("optObjShowCoveringObjs")%></option>";
                buttons += "<option value='2' "+(SelectOnlyCoveringObjectivesOption == 2 ? "selected='selected'" :  "") + "><%=ResString("optObjShowEverythingBelow")%></option>";
                buttons += "</select>";
                <%--buttons += "<a href='' style='cursor: pointer; position: absolute; margin-left: 7px; z-index: 101;' onclick='btnGridSettingsClick(); return false;'><img style='vertical-align: top; margin-top: 2px;' src='<% =ImagePath %>old/settings-16.png' alt='<%=ResString("lblRiskSettings") %>' title='<%=ResString("lblRiskSettings") %>' ></a>";--%>
                $("#divWidgetToolbar" + widget_index).html(buttons);

                // init widget content                
                $("#divInnerContent" + widget_index).addClass("objs_table");
                $("#divInnerContent" + widget_index).html("<div id='tableObjs"+widget_index+"' wid='" + widget_index + "' class='objs_table' style='margin: 0px; width: 0;'></div>");
                getObjectivesGridHTML(widget_index);
                break;
            case vtDSA:
                //$("#divInnerContent" + widget_index).html("<canvas id='DSACanvas" + widget_index + "' class='DSACanvas no-tooltip' style='background:#eeeeee;'></canvas>");
                //var w = $("#trWidgetContent" + widget_index).width();
                //var h = $("#trWidgetContent" + widget_index).height();
                //initSACanvas(widget_index, w, h, 'DSA');
                loadSA(widget_index, 'DSA');
                break;
            case vtPSA:
                // init widget toolbar
                var buttons = "<a href='' id='btnPSAMode0"+widget_index+"' class='actions widget_toolbar_button' onclick='setPSAMode(0, \"btnPSAModeImg\","+widget_index+"); return false;'><img id='btnPSAModeImg"+widget_index+"_0' class='toggle_btn_checked border-highlight' align='left' style='vertical-align:middle;' alt='Lines' title='Lines' src='<%=ImagePath()%>old/psa_lines_16.png' ></a>";
                buttons += "<a href='' id='btnPSAMode1"+widget_index+"' class='actions widget_toolbar_button' onclick='setPSAMode(1, \"btnPSAModeImg\","+widget_index+"); return false;'><img id='btnPSAModeImg"+widget_index+"_1' class='toggle_btn_unchecked' align='left' style='vertical-align:middle;' alt='Ticks' title='Ticks' src='<%=ImagePath()%>old/psa_arrows_16.png' ></a>&nbsp&nbsp";
                buttons += "<a href='' id='btnPSAAlign0"+widget_index+"' class='actions widget_toolbar_button_pad_left' onclick='setPSAAlign(0, \"btnPSAAlignImg\","+widget_index+"); return false;'><img id='btnPSAAlignImg"+widget_index+"_0' class='toggle_btn_checked border-highlight' align='left' style='vertical-align:middle;' alt='Align Labels to Values' title='Align Labels to Values' src='<%=ImagePath()%>old/psa_alignB_16.png' ></a>";
                buttons += "<a href='' id='btnPSAAlign1"+widget_index+"' class='actions widget_toolbar_button' onclick='setPSAAlign(1, \"btnPSAAlignImg\","+widget_index+"); return false;'><img id='btnPSAAlignImg"+widget_index+"_1' class='toggle_btn_unchecked' align='left' style='vertical-align:middle;' alt='Expand Labels' title='Expand Labels' src='<%=ImagePath()%>old/psa_alignA_16.png'></a>&nbsp&nbsp";
                buttons += "<a href='' id='btnPSAView0"+widget_index+"' class='actions widget_toolbar_button_pad_left' onclick='setPSAView(0, \"btnPSAViewImg\","+widget_index+"); return false;'><img id='btnPSAViewImg"+widget_index+"_0' class='toggle_btn_checked border-highlight' align='left' style='vertical-align:middle;' alt='Vertical Bars' title='Vertical Bars' src='<%=ImagePath()%>old/psa_column_16.png' ></a>";
                buttons += "<a href='' id='btnPSAView1"+widget_index+"' class='actions widget_toolbar_button' onclick='setPSAView(1, \"btnPSAViewImg\","+widget_index+"); return false;'><img id='btnPSAViewImg"+widget_index+"_1' class='toggle_btn_unchecked' align='left' style='vertical-align:middle;' alt='Radar Chart' title='Radar Chart' src='<%=ImagePath()%>old/psa_radar_16.png' ></a>";
                $("#divWidgetToolbar" + widget_index).html(buttons);
                
                // init widget content
                //$("#divInnerContent" + widget_index).html("<canvas id='DSACanvas" + widget_index + "' class='DSACanvas no-tooltip' style='background:#eeeeee'></canvas>");
                //var w = $("#trWidgetContent" + widget_index).width();
                //var h = $("#trWidgetContent" + widget_index).height();
                //initSACanvas(widget_index, w, h, 'PSA');
                loadSA(widget_index, 'PSA');
                break;
            case vtGSA:
                // init widget toolbar                
                var buttons = "<table style='width:100%;'><tr><td class='text'><span>Legend:&nbsp;</span><a href='' id='btnGSAShowLegend"+widget_index+"' class='actions' onclick='switchGSALegend("+widget_index+",1); return false;' style='border:1px solid <%=IIf(SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard, "#fff", "#ccc")%>; padding:2px;'>Show</a>";
                buttons += "<a href='' id='btnGSAHideLegend"+widget_index+"' class='actions' onclick='switchGSALegend("+widget_index+",0); return false;' style='border:1px solid <%=IIf(SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard, "#ccc", "#fff")%>; padding:2px;'>Hide</a></td><td><div id='DSACanvas" + widget_index + "Options'></div></td></tr></table>";
                $("#divWidgetToolbar" + widget_index).html(buttons);
                
                // init widget content
                //$("#divInnerContent" + widget_index).html("<canvas id='DSACanvas" + widget_index + "' class='DSACanvas no-tooltip' style='background:#eeeeee'></canvas>");
                //var w = $("#trWidgetContent" + widget_index).width();
                //var h = $("#trWidgetContent" + widget_index).height();
                //initSACanvas(widget_index, w, h, 'GSA');
                loadSA(widget_index, 'GSA');
                break;
            case vt2D:
                // init widget toolbar                
                var buttons = "<table style='width:100%;'><tr><td class='text'><span>Legend:&nbsp;</span><a href='' id='btnGSAShowLegend"+widget_index+"' class='actions' onclick='switchGSALegend("+widget_index+",1); return false;' style='border:1px solid <%=IIf(SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard, "#fff", "#ccc")%>; padding:2px;'>Show</a>";
                buttons += "<a href='' id='btnGSAHideLegend"+widget_index+"' class='actions' onclick='switchGSALegend("+widget_index+",0); return false;' style='border:1px solid <%=IIf(SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard, "#ccc", "#fff")%>; padding:2px;'>Hide</a></td><td><div id='DSACanvas" + widget_index + "Options'></div></td></tr></table>"; 
                $("#divWidgetToolbar" + widget_index).html(buttons);

                //$("#divInnerContent" + widget_index).html("<canvas id='DSACanvas" + widget_index + "' class='DSACanvas no-tooltip' style='background:#eeeeee'></canvas>");
                //var w = $("#trWidgetContent" + widget_index).width();
                //var h = $("#trWidgetContent" + widget_index).height();
                //initSACanvas(widget_index, w, h, '2D');
                loadSA(widget_index, '2D');
                break;
            case vtHTH:
                // init widget toolbar                
                var buttons = "<div id='DSACanvas" + widget_index + "Options'></div>";
                $("#divWidgetToolbar" + widget_index).html(buttons);

                //$("#divInnerContent" + widget_index).html("<canvas id='DSACanvas" + widget_index + "' class='DSACanvas no-tooltip' style='background:#eeeeee'></canvas>");
                //var w = $("#trWidgetContent" + widget_index).width();
                //var h = $("#trWidgetContent" + widget_index).height();
                //initSACanvas(widget_index, w, h, 'HTH');
                loadSA(widget_index, 'HTH');
                break;
            case vtCV:
                var infoMsgCV = "<div id='divCVInnerHeader'><span class='note' style='display:inline-block;margin-bottom:2px;'><%=SafeFormString(ResString("lblCVDescription"))%></span></div>";
                $("#divWidgetToolbar" + widget_index).html(infoMsgCV);
                $("#divInnerContent" + widget_index).html("<div id='tableCV" + widget_index + "' wid='" + widget_index + "' class='text cv_table' style='width:100%;'></table>");
                getCVGridHTML(widget_index);
                break;
            case vtTree:                
                $("#divInnerContent" + widget_index).html("<div id='divCVTree" + widget_index + "' wid='" + widget_index + "' class='whole'><div id='cvTree" + widget_index + "'></div></div>");
                initCVTree("cvTree" + widget_index);
                break;
            case vtASA:
                $("#divInnerContent" + widget_index).html("<canvas id='DSACanvas" + widget_index + "' class='DSACanvas no-tooltip' style='background:#eeeeee'></canvas>");
                var w = $("#trWidgetContent" + widget_index).width();
                var h = $("#trWidgetContent" + widget_index).height();
                //loadSA(widget_index, 'ASA');
                initSACanvas(widget_index, w, h, 'ASA');
                break;
            case vt4ASA:
                // init widget toolbar                                   
                //$("#divWidgetToolbar" + widget_index).removeClass("on_advanced").show();
                $("#divWidgetToolbar" + widget_index).html("&nbsp;<span class='small text'>Participant&#47;Group:&nbsp;</span>");
                $("#cb4ASAUsers" + widget_index).appendTo("#divWidgetToolbar" + widget_index).show();
                $("#lblKeep" + widget_index).appendTo("#divWidgetToolbar" + widget_index).show();
                //$("<label><input type='checkbox' class='small' id='cbKeep" + widget_index + "' onclick='sendCommand(\"action=" + ACTION_4ASA_KEEP + "&wid=" + widget_index + "\", true);'>Keep</label>").appendTo("#divWidgetToolbar" + widget_index);
                // init widget content
                $("#divInnerContent" + widget_index).html("<canvas id='DSACanvas" + widget_index + "' class='DSACanvas no-tooltip' style='background:#eeeeee'></canvas>");

                updatePortionsUI();                
                
                var w = $("#trWidgetContent" + widget_index).innerWidth();
                var h = $("#trWidgetContent" + widget_index).innerHeight();
                //loadSA(widget_index, 'ASA');
                initSACanvas(widget_index, w, h, 'ASA'); 
                break;
            case vtBowTie:
                //$("#divInnerContent" + widget_index).load(urlBowTie + "&wrt=" + WRTNodeID);
                document.getElementById("divInnerContent" + widget_index).innerHTML='<object type="text/html" class="whole" data="' + urlBowTie + "&wrt=" + WRTNodeID + '" ></object>';
                break;
            case vtHeatMap:
                //$("#divInnerContent" + widget_index).load(urlHeatMap + "&wrt=" + WRTNodeID);
                document.getElementById("divInnerContent" + widget_index).innerHTML='<object type="text/html" class="whole" data="' + urlHeatMap + "&wrt=" + WRTNodeID + '" ></object>';
                break;
        }
    } 

   // function btnGridSettingsClick() {
   //     initGridSettingsDlg();
   //     dlg_settings.dialog("open");
   // }

   // function initGridSettingsDlg() {                
   //    dlg_settings = $("#divGridSettings").dialog({
   //        autoOpen: false,
   //        width: 630,
   //        height: "auto",
   //        modal: true,
   //        closeOnEscape: true,
   //        dialogClass: "no-close",
   //        bgiframe: true,
   //        title: "Settings",
   //        position: { my: "center", at: "center", of: $("body"), within: $("body") },
   //        buttons: [{ text:"Close", click: function() { dlg_settings.dialog( "close" ); }}],
   //        open:  function() { $("body").css("overflow", "hidden"); },
   //        close: function() { $("body").css("overflow", "auto"); }
   //    });
   //}

    function getDSAControlsNames() {        
        var names = [];
        $('canvas.DSACanvas').each(function() { names.push(this.getAttribute("id")) });
        return names;
    }

    function resetSA() {
        var dsaIDs = getDSAControlsNames();
        for (var i = 0; i < dsaIDs.length; i++) {
            $('#'+dsaIDs[i]).sa("resetSA");
        };
    };

    function setPSAMode(value, btn, widget_index) {
        var img0 = $("#" + btn + widget_index + "_0");
        var img1 = $("#" + btn + widget_index + "_1");

        if ((img0) && (img1)) {
            switch (value*1) {
                case 0:
                    img0.addClass('border-highlight');
                    img1.removeClass('border-highlight');
                    $('#DSACanvas'+widget_index).sa("option", "PSAShowLines", true);
                    break;
                case 1:
                    img1.addClass('border-highlight');
                    img0.removeClass('border-highlight');
                    $('#DSACanvas'+widget_index).sa("option", "PSAShowLines", false);
                    break;
            }
        }

        $('#DSACanvas'+widget_index).sa("redrawSA");
    }

    function setPSAAlign(value, btn, widget_index) {
        var img0 = $("#" + btn + widget_index + "_0");
        var img1 = $("#" + btn + widget_index + "_1");

        if ((img0) && (img1)) {
            switch (value*1) {
                case 0:
                    img0.addClass('border-highlight');
                    img1.removeClass('border-highlight');
                    $('#DSACanvas'+widget_index).sa("option", "PSALineup", true);
                    break;
                case 1:
                    img1.addClass('border-highlight');
                    img0.removeClass('border-highlight');
                    $('#DSACanvas'+widget_index).sa("option", "PSALineup", false);
                    break;
            }
        }

        $('#DSACanvas'+widget_index).sa("redrawSA");
    }

    function setPSAView(value, btn, widget_index) {
        var img0 = $("#" + btn + widget_index + "_0");
        var img1 = $("#" + btn + widget_index + "_1");

        if ((img0) && (img1)) {
            switch (value*1) {
                case 0:
                    img0.addClass('border-highlight');
                    img1.removeClass('border-highlight');
                    $('#DSACanvas'+widget_index).sa("option", "showRadar", false);
                    break;
                case 1:
                    img1.addClass('border-highlight');
                    img0.removeClass('border-highlight');
                    $('#DSACanvas'+widget_index).sa("option", "showRadar", true);
                    break;
            }
        }

        $('#DSACanvas'+widget_index).sa("redrawSA");
    }

    function cbASAPageSizeChange(value) {
        sendCommand('action=asa_page_size&value=' + value, true);
    }

    function cbASAPageNumChange(value) {
        asa_pagenum = value*1;
        sendCommand('action=asa_page_num&value=' + value, true);
    }

    function cbASASortByChange(value) {
        resetSA();
        asa_sortby = value*1;
        sendCommand('action=asa_sort_by&value=' + value, true);
    }

    function cbSAAltsSortByChange(value) {
        sa_alts_sortby = value*1;
        $("#cbActiveSorting").prop("disabled", sa_alts_sortby != 1);
        $("#lblActiveSorting").css("color", sa_alts_sortby != 1 ? "#909090" : "black");
        sendCommand('action=sa_alts_sort_by&value=' + value, true);
    }

    function onSASortingEvent(sortMode, sortBy) {
        if (sortMode == 1) {
            $("#cbSAAltsSortBy").val(sortBy);
            cbSAAltsSortByChange(sortBy);
        } else {
            $("#cbASASortBy").val(sortBy);
            cbASASortByChange(sortBy);
        };
    }

    function cbSAAltsParameterChange(value) {
        RiskParam = value;
        sendCommand('action=sa_alts_parameter&value=' + value, true);
    }

    function updatePageList(pagesize) {
        if (SynthesisView == vt4ASA || SynthesisView == vtASA){
            $('#cbASAPageNum').empty();
            var idx = 0;
            var objs = (SynthesisView == vt4ASA ? asa_data[0].objectives : asa_data.objectives);
            for (var i = 0; i < objs.length; i++) {
                var obj = objs[i];
                if (obj.visible === 1) {
                    idx++;
                }
            }

            for (var i = 1; i <= Math.floor((idx + pagesize -1) / pagesize); i++) {
                $('#cbASAPageNum').append($('<option>', {
                    value: i,
                    text: i
                }));          
            }
            $('#cbASAPageNum').val(asa_pagenum);       
        };
    }
    // end Layouts / Widgets Management

    function getUserByID(id) {
        for (var u = 0; u < users_data.length; u++) {
            if (users_data[u][IDX_USER_ID] == id) return users_data[u];
        }
        for (var u = 0; u < groups_data.length; u++) {
            if (groups_data[u][IDX_USER_ID] == id) return groups_data[u];
        }
        return false;
    }

    function getExpectedValue(uid) {
        var sExpVal = "";
        if ((expected_values) && expected_values.length > 0) {
            for (var k = 0; k < expected_values.length; k++) {
                if (expected_values[k][0] == uid && expected_values[k][2] == 1) {
                    sExpVal = "<small><br>Expected value: " + roundTo(expected_values[k][1], DecimalDigits) + "</small>";
                }
            }
        }
        return sExpVal;
    }

    var userColumnsCount = 0;

    function getAlternativesGridHTML(widget_index) {               
        var table = (($("#tableAlts" + widget_index).data("dxDataGrid"))) ? $("#tableAlts" + widget_index).dxDataGrid("instance") : null;
        if (table !== null) { table.dispose(); }

        //init columns headers                
        var columns = [];
        columns.push({ "caption" : "<% = CStr(If(PM.Parameters.NodeVisibleIndexMode = IDColumnModes.IndexID, ResString("tblNo_"), If(PM.Parameters.NodeVisibleIndexMode = IDColumnModes.UniqueID, ResString("optUniqueID"), ResString("optRank"))))%>", "alignment" : "left", "allowSorting" : true, "allowSearch" : false, "allowEditing" : false, "allowHiding" : false, "dataField": "id", "width": "60px" });
        columns.push({ "caption" : "<% = ParseString("%%Alternative%%")%> Name", "cssClass": "no-tooltip", "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "allowEditing" : false, "allowHiding" : false, "dataField": "name", "width": 300, "encodeHtml" : false, showInColumnChooser: false });
        <% If PM.IsRiskProject %>
        columns.push({ "caption" : "<% = ResString("colEventType") %>", "alignment" : "left", "allowSorting" : true, "dataField": "etype", "encodeHtml" : false, groupIndex: 1, showInColumnChooser: false });
        <% End If %>
        <% If PM.IsRiskProject AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) Then %>
        columns.push({ "caption" : "<% = ParseString("%%Objective(l)%%") %>", "cssClass": "no-tooltip", "alignment" : "left", "allowSorting" : true, "dataField": "likelihood_contributions", "encodeHtml" : false, "width": 400, showInColumnChooser: true, /*customizeText : function(cellInfo) {
            var retVal = "";
            cellInfo.value.forEach(function(item) {
                retVal += (retVal == "" ? "" : ", ") + coveringSources[item].name;
            });
            return retVal;
        },*/ groupCellTemplate : function (element, info) {
            var retVal = "";
            info.value.forEach(function(item) {
                retVal += (retVal == "" ? "" : ", ") + coveringSources[item].name + (coveringSources[item].info == "" ? "" : " – " + coveringSources[item].info);
            });
            if (retVal == "") retVal = "<% = ParseString("No %%Objective(l)%%") %>";
            info.displayValue = retVal;
            element.append(retVal);
        } });
        <% End If %>
        <%--columns.push({ "caption" : "<%=ResString("tblAlternativeStatus")%>", "alignment" : "center", "allowSorting" : true, "allowSearch" : false, "allowEditing" : true, "allowHiding" : true, "visible" : false, "dataField": "disabled", "width": "80px" });--%>

        var cellTemplate = function(element, info) {            
            if (info.text !== lblNA) {
                var fVal = info.value;
                var sClickHandler = 'curNodeID=' + info.data["IDX_ID"] + '; curNodeHid=-1; ecColorPicker(event, this, true, ' + info.data["IDX_ID"] + ');';
                var sVal = htmlGraphBarWithValue(fVal, 100, fVal+"" != "<%=Double.NaN.ToString%>" ? fVal + "%" : fVal, "<% =ImagePath %>", 100, 11, info.data["color"] != "" ? info.data["color"] : BAR_DEFAULT_COLOR, "", sClickHandler, info.data["etype"] == "<% = ParseString("%%Opportunity%%") %>");
                element.append(sVal);
            } else {
                element.append(lblNA);
            }
        };

        for (var j = 0; j < columns_user_ids.length; j++) {
            for (var i = 0; i < users_data.length; i++) {                
                if (users_data[i][IDX_USER_ID] === columns_user_ids[j]) {
                    columns.push({ showInColumnChooser: false, headerCellTemplate : function (columnHeader, headerInfo) {
                        var usr = users_data[headerInfo.column["uid"]];
                        return columnHeader.append(htmlEscape(usr[IDX_USER_NAME]) + getExpectedValue(usr[IDX_USER_ID]));
                        }, 
                        "caption" : users_data[i][IDX_USER_NAME], "alignment" : "center", "allowSorting" : true, "allowSearch" : true, "allowEditing" : false, "allowHiding" : false, "dataField": "user" + j, "dataType" : "number", "cellTemplate" : cellTemplate, "uid": i, "width": "120px" });
                }
            }
            for (var i = 0; i < groups_data.length; i++) {
                if (groups_data[i][IDX_USER_ID] === columns_user_ids[j]) {
                    columns.push({ showInColumnChooser: false, headerCellTemplate : function (columnHeader, headerInfo) {
                        var grp = groups_data[headerInfo.column["uid"]];
                        return columnHeader.append("<span title='" + grp[IDX_GROUP_NAME_HINT] + "'>" + htmlEscape(grp[IDX_USER_NAME]) + grp[IDX_GROUP_NAME_EXTRA] + getExpectedValue(grp[IDX_USER_ID]) + "</span>");
                        }, 
                        "caption" : groups_data[i][IDX_USER_NAME], "alignment" : "center", "allowSorting" : true, "allowSearch" : true, "allowEditing" : false, "allowHiding" : false, "dataField": "user" + j, "dataType" : "number", "cellTemplate" : cellTemplate, "uid": i, "width": "120px" });
                }
            }
        }

        //init dataset
        var dataset = [];

        for (var j = 0; j < alts_priorities.length; j++) {
            var data = {};
            var alt = getAlternativeByID(alts_priorities[j][IDX_ID]);
            if ((alt)) {
                data["id"] = alt[IDX_ALT_INDEX];
                data["IDX_ID"] = alt[IDX_ID];
                data["name"] = htmlEscape(alt[IDX_ALT_NAME]);
                data["etype"] = alt[IDX_ALT_TYPE];
                data["disabled"] = !alt[IDX_ALT_ENABLED];
                data["color"] = alt[IDX_ALT_COLOR];

                var threats = "";
                alt[IDX_SCENARIOS].forEach(function(item) {
                    threats += (threats == "" ? "" : ", ") + coveringSources[item].name;
                });
                data["likelihood_contributions"] = threats;

                for (var k = IDX_ALT_VALUES; k < alts_priorities[j].length; k++) {
                    var sVal = lblNA;
                    if (alts_priorities[j][k][IDX_CAN_SHOW_RESULTS] == 1) {
                        sVal = roundTo(alts_priorities[j][k][IDX_NODE_GLOBAL_PRTY] * 100, DecimalDigits);
                    }
                    data["user" + (k - IDX_ALT_VALUES)] = sVal;
                    data["user" + (k - IDX_ALT_VALUES) + "_has_value"] = alts_priorities[j][k][IDX_CAN_SHOW_RESULTS] == 1;                    
                }
                dataset.push(data);
            }
        }

        table = $("#tableAlts" + widget_index).dxDataGrid( {
            allowColumnResizing: true,
            dataSource: dataset,
            columns: columns,
            columnAutoWidth: true,
            columnResizingMode: 'widget',
            //width: "100%",
            //height: "100%",
            <% If PM.IsRiskProject AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) Then%>
            columnChooser: {
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: true
            },
            <% End If %>
            searchPanel: {
                visible: true,
                width: 240,
                placeholder: "<%=ResString("btnDoSearch")%>..."
            },
            columnChooser: {
                mode: "select",
                enabled: false
            },
            grouping: {
                autoExpandAll: true,
            },
            groupPanel: {
                visible: false
            },
            pager: {
                allowedPageSizes: <% = _OPT_PAGINATION_PAGE_SIZES %>,
                showPageSizeSelector: true,
                showNavigationButtons: true,
                visible: dataset.length > <% = _OPT_PAGINATION_LIMIT %>
            },
            paging: {
                enabled: dataset.length > <% = _OPT_PAGINATION_LIMIT %>,
            },
            rowAlternationEnabled: true,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            hoverStateEnabled: true,
            <%If App.isExportAvailable Then%>
            "export": {
                enabled: true,
                fileName: "<%=ParseString("%%Alternatives%% Grid")%> for <%=SafeFileName(App.ActiveProject.ProjectName)%>" + ", results with respect to " + getObjectiveByID(WRTNodeID)[IDX_NODE_NAME]
            },
            <%End If%>
            editing: {
                mode: "cell", //"row", //"batch",
                useIcons: true,
                allowUpdating: !isReadOnly
            },
            onRowUpdated: function (e) {
                //for (var j = 0; j < dataset.length; j++) {
                //    if (dataset[j].IDX_ID == e.key.IDX_ID) {
                //        dataset[j]["disabled"] = e.data["disabled"];
                //        var alt = getAlternativeByID(e.key.IDX_ID);
                //        if ((alt)) alt[IDX_ALT_ENABLED] = !e.data["disabled"];
                //    }
                //}
                //var params = "&id=" + e.key.IDX_ID + "&enabled=" + !e.data["disabled"];                
                //sendCommand("action=enable_alts" + params, false);                
            },
            stateStoring: {
                enabled: true,
                type: "sessionStorage",
                storageKey: "synthAltTbl_PRJID_<%=App.ProjectID%>"
            },
            onRowPrepared: function (e) {
                if (e.rowType != "data") return;
                var row = e.rowElement;
                var data = e.data;
                $("td", row).fadeTo('slow', data["disabled"] ? 0.4 : 1); // disabled alternative                
            },
            noDataText: "<% = GetEmptyMessage() %>",
            wordWrapEnabled: false
        });
        setTimeout(function() {
            resizeAllGrids();
        }, 300);
    }

    var curNodeID = '';
    var curNodeHid = -1; //alternative -1, lkhd : 0, imp : 1
    var curElement;
    //var is_color_picker_open = false;
    var colorDlg = null, 
        colorDlgOptions = {
            animation: null,
            width: 200,
            height: "auto",
            contentTemplate: function() {
                return  $("<div style='padding:1ex; text-align: center;'></div>").append(
                    $("<div id='div_colorpicker'></div>"), $("<br>"),
                    $("<div><a href='' id='btnResetAll' class='actions text' style='margin-top: 4px;' onclick='nodesColorsReset(); return false;'><%=ResString("btnResetAll")%></a></div>"),
                    $("<div id='btnColorPickerClose'></div>")
                );
            },
            showTitle: true,
            title: "Select color",
            dragEnabled: true,
            shading: false,
            closeOnOutsideClick: true,
            resizeEnabled: false,
            showCloseButton: true
        };

    function ecColorPicker(e, element, has_reset_all_option, key, color) {
        if (e.preventDefault) { e.preventDefault(); } else { e.returnValue = false; }
        curElement = element;
        if ((key)) curNodeID = key;

        if (colorDlg) $(".colorDlg").remove();
        var $popupContainer = $("<div></div>").addClass("colorDlg").appendTo($("#colorDlg"));
        colorDlg = $popupContainer.dxPopup(colorDlgOptions).dxPopup("instance");
        colorDlg.show();

        $("#btnResetAll").toggle(has_reset_all_option);

        $("#div_colorpicker").dxColorBox({ 
            value: typeof color !== "undefined" ? color : (curElement) ? curElement.firstChild.style.backgroundColor : null,
            disabled: isReadOnly,
            onValueChanged: function (e) {
                nodeColorApply(e.value);
            }
        });

        $("#btnColorPickerClose").dxButton({
            text: "Close",
            icon: "fas fa-times",
            onClick: function() {
                colorDlg.hide();
            }
        });
    }

    function nodeColorApply(newValue) {
        if ((curElement)) curElement.firstChild.style.backgroundColor = newValue;
        $('canvas.DSACanvas').sa("setAltColor", curNodeID, newValue); 
        sendCommand("action=" + ACTION_NODE_COLOR_SET + "&id=" + curNodeID + "&value=" + replaceString("#", "", newValue) + "&hid=" + curNodeHid, false);
    }

    function nodesColorsReset() {
        dxConfirm(curNodeHid == -1 ? resString("msgSureResetAltsColors") : resString("msgSureResetAltsColors"), "sendCommand('action=" + ACTION_NODES_COLORS_RESET + "&hid=" + curNodeHid + "', true);");
    }

    function getAlternativeByID(id) {
        for (var j = 0; j < alts_data.length; j++) {
            if (alts_data[j][IDX_ID] == id) return alts_data[j];
        }
        return;
    }

    function getObjectiveByID(id) {
        for (var j = 0; j < nodes_data.length; j++) {
            if (nodes_data[j][IDX_NODE_ID] == id) return nodes_data[j];
        }
        return null;
    }

    function getObjectiveByGUID(guid) {
        for (var j = 0; j < nodes_data.length; j++) {
            if (nodes_data[j][IDX_NODE_GUID] == guid) return nodes_data[j];
        }
        return null;
    }

    function isChildOf(obj, parent_guids) {        
        var pids = obj[IDX_NODE_PARENT_GUIDS];
        if (pids.length >= 0) {
            for (var i = 0; i < pids.length; i++) {
                var parent = getObjectiveByGUID(pids[i]);
                if ((parent)) {
                    for (var k = 0; k < parent_guids.length; k++) {
                        parent_id = parent_guids[k];
                        if ((parent[IDX_NODE_GUID] == parent_id)) {
                            return true;
                        } else {
                            return isChildOf(parent, parent[IDX_NODE_PARENT_GUIDS]);
                        }
                    }
                }
            }
        } else {
            return false;
        }
    }

    function nodeChildrenCount(obj_id) {
        var retVal = 0;
        for (var j = 0; j < nodes_data.length; j++) {
            if (nodes_data[j][IDX_NODE_PARENT_ID] == obj_id) retVal += 1;
        }
        return retVal;
    }
    // end Alterantives Grid / Chart

    // Objectives Grid    
    var WRTNodeLevel = 0;
    function getObjectivesGridHTML(widget_index) {
        var table = (($("#tableObjs" + widget_index).data("dxDataGrid"))) ? $("#tableObjs" + widget_index).dxDataGrid("instance") : null;
        if (table !== null) { table.dispose(); }

        var priority_mode = (typeof tableObjParams["priority_mode" + widget_index] !== "undefined" ? tableObjParams["priority_mode" + widget_index] : <% = PM.Parameters.Synthesis_ObjectivesPrioritiesMode %>); // Local priorities by default

        //init columns headers                
        var columns = [];
        // columns.push({ "title" : "ObjID", "bVisible" : false, "searchable" : false });
        var nameCellTemplate = function(element, info) {            
            element.append("<span>" + (info.data["is_cat"] ? "<i>" : "") + info.data["name"] + (info.data["is_cat"] ? "</i>" : "") + "</span>");
        };

        columns.push({ "caption" : "<%=ParseString("%%Objective%%")%> Name", "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "allowEditing" : false, "allowHiding": false, "dataField" : "name", "cellTemplate" : nameCellTemplate, "width": 300, "encodeHtml" : false });

        var priorityCellTemplate = function(element, info) {
            if (info.data[info.column["dataField"]] !== lblNA) {
                var fVal = info.data[info.column["dataField"]]; //roundTo(info.data[info.column["dataField"]] * 100, DecimalDigits);
                var sClickHandler = 'curNodeID=' + info.data["node_id"] + '; curNodeHid=-2; ecColorPicker(event, this, true, \"' + info.data["node_id"] + '\");';
                if (!info.data["is_cat"]) {
                    element.append(htmlGraphBarWithValue(fVal, 100, fVal+"" != "<%=Double.NaN.ToString%>" ? fVal + "%" : fVal, "<% =ImagePath %>", 100, 11, info.data["color"] != "" ? info.data["color"] : BAR_DEFAULT_COLOR, "", sClickHandler));
                }
            } else {
                element.append(lblNA);
            }
        }

        for (var j = 0; j < columns_user_ids.length; j++) {
            for (var i = 0; i < users_data.length; i++) {
                if (users_data[i][IDX_USER_ID] === columns_user_ids[j]) {
                    columns.push({ "caption" : users_data[i][IDX_USER_NAME], "alignment" : "center", "allowSorting" : true, "allowSearch" : true, "allowEditing" : false, "dataField" : priority_mode == 0 ? "local" + j : "global" + j, "cellTemplate" : priorityCellTemplate, "width": "120px" });
                }
            }
            for (var i = 0; i < groups_data.length; i++) {
                if (groups_data[i][IDX_USER_ID] === columns_user_ids[j]) {
                    var groupColumnHeaderExport = replaceString("<br>", "\n", "[" + groups_data[i][IDX_USER_NAME] + "]" + groups_data[i][IDX_GROUP_NAME_EXTRA] + getExpectedValue(columns_user_ids[j]));
                    var groupColumnHeaderHint = groups_data[i][IDX_USER_NAME] + groups_data[i][IDX_GROUP_NAME_EXTRA] + getExpectedValue(columns_user_ids[j]);
                    columns.push({ "caption" : groupColumnHeaderExport, "tmplt" : "<span title='" + groups_data[i][IDX_GROUP_NAME_HINT] + "'>" + groupColumnHeaderHint + "</span>", "alignment" : "center", "allowSorting" : true, "allowSearch" : true, "allowEditing" : false, "dataField" : priority_mode == 0 ? "local" + j : "global" + j, "cellTemplate" : priorityCellTemplate, "width": "120px",
                        headerCellTemplate: function (columnHeader, headerInfo) {
                            columnHeader.append(headerInfo.column.tmplt);
                        }
                    });
                }
            }
        }

        var wrtNode = getObjectiveByID(WRTNodeID);
        var treeNode = getTreeNode(cv_tree_nodes_data[0], wrtNode[IDX_NODE_GUID]);
        var parent = getObjectiveByID(WRTNodeID);
        var GridWRTNodeGUID = parent[IDX_NODE_GUID];
        
        //init dataset
        var dataset = []; //objs_priorities.slice;
        var t = 0;
        for (var j = 0; j < objs_priorities.length; j++) {            
            var obj = getObjectiveByID(objs_priorities[j][IDX_ID]);            
            if ((obj)) {
                var vis = true;
                switch (SelectOnlyCoveringObjectivesOption) {
                    case 0: //oShowChildren
                        //if (obj[IDX_NODE_PARENT_ID] != WRTNodeID) vis = false;
                        if ($.inArray(GridWRTNodeGUID, obj[IDX_NODE_PARENT_GUIDS]) == -1) vis = false;
                        break;
                    case 1: //oShowCoveringObjectives
                        if (!isChildOf(obj, [GridWRTNodeGUID]) || obj[IDX_NODE_IS_TERMINAL] != 1) vis = false;
                        break;
                    case 2: //oShowEverything
                        if (!isChildOf(obj, [GridWRTNodeGUID])) vis = false;
                        break;
                }
                if (vis) {
                    dataset.push({});
                    dataset[t]["name"] = obj[IDX_NODE_NAME];
                    if (SelectOnlyCoveringObjectivesOption !== 0) {
                        for (var b = 1; b < obj[IDX_NODE_LEVEL]; b++) {
                            dataset[t]["name"] = "&nbsp;&nbsp;&nbsp;&nbsp;" + dataset[t]["name"];
                        }
                    }
                    for (var k = 2; k < objs_priorities[j].length; k++) {
                        //dataset[t].push([objs_priorities[j][k][0], objs_priorities[j][k][1], objs_priorities[j][k][2]]);
                        dataset[t]["has_results"] = objs_priorities[j][k][0] == 1;
                        var sValG = lblNA;
                        var sValL = lblNA;
                        if (objs_priorities[j][k][0] == 1) {
                            sValG = roundTo(objs_priorities[j][k][1] * 100, DecimalDigits);
                            sValL = roundTo(objs_priorities[j][k][2] * 100, DecimalDigits);
                        }
                        dataset[t]["global" + (k-2)] = sValG;
                        dataset[t]["local" + (k-2)] = sValL;
                    }
                    dataset[t]["level"] = obj[IDX_NODE_LEVEL];
                    dataset[t]["node_id"] = obj[IDX_NODE_ID];
                    dataset[t]["color"] = obj[IDX_NODE_COLOR];
                    dataset[t]["is_cat"] = obj[IDX_NODE_IS_CATEGORY];
                    dataset[t]["pid"] = SelectOnlyCoveringObjectivesOption == 1 || obj[IDX_NODE_PARENT_ID] == wrtNode[IDX_ID] ? "" : obj[IDX_NODE_PARENT_ID];
                    t += 1;
                }
            }
        }

        WRTNodeLevel = wrtNode[IDX_NODE_LEVEL];
        
        table = $("#tableObjs" + widget_index).dxDataGrid( {
            autoExpandAll: true,
            dataSource: dataset,
            columns: columns,
            columnAutoWidth: false,
            width: "100%",
            keyExpr: "node_id",
            parentIdExpr: "pid",
            rootValue: "",
            searchPanel: {
                visible: true,
                width: 240,
                placeholder: "<%=ResString("btnDoSearch")%>..."
            },
            columnChooser: {
                mode: "select",
                enabled: false
            },
            rowAlternationEnabled: true,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            sorting: { mode : 'none' },
            hoverStateEnabled: true,
            <%If App.isExportAvailable Then%>
            "export": {
                enabled: true,
                fileName: "<%=ParseString("%%Objectives%% Grid")%> for <%=SafeFileName(App.ActiveProject.ProjectName)%>" + ", results for nodes below " + getObjectiveByID(WRTNodeID)[IDX_NODE_NAME]
            },
            customizeExportData: function (columns, rows) {
                rows.forEach(function (row) {
                    var rowValues = row.values;
                    if (rowValues.length > 0) rowValues[0] = replaceString("&nbsp;", " ", row.data["name"]);
                    if (row.data["is_cat"]) {
                        for (k = 1; k < rowValues.length; k++) {
                            rowValues[k] = "";
                        }
                    }
                })
            },
            <%End If%>
            editing: {
                mode: "cell", //"row", //"batch",
                useIcons: true,
                allowUpdating: !isReadOnly
            },
            stateStoring: {
                enabled: true,
                type: "sessionStorage",
                storageKey: "synthesisObjsTable_PRJID_<%=App.ProjectID%>"
            },
            onRowPrepared: function (e) {
                if (e.rowType != "data") return;
                var row = e.rowElement;
                var data = e.data;
                $("td", row).fadeTo('slow', data["disabled"] ? 0.4 : 1); // disabled objective
            },
            pager: {
                allowedPageSizes: <% = _OPT_PAGINATION_PAGE_SIZES %>,
                showPageSizeSelector: true,
                showNavigationButtons: true,
                visible: alts_data.length > <% = _OPT_PAGINATION_LIMIT %>
            },
            paging: {
                enabled: alts_data.length > <% = _OPT_PAGINATION_LIMIT %>
            },
            noDataText: function () {
                if (SynthesisView == vtObjectives && treeNode.nodeid != cv_tree_nodes_data[0].nodeid && treeNode.isterminal) {
                    setTimeout(function() {
                        var noDataSpan = $(".dx-treelist-nodata");
                        noDataSpan.html("<span class='dx-treelist-nodata' style='z-index: 3;'><%=ResString("msgNoObjectivesUnderNode")%><br><a class='actions' href='' onclick='openAlts(); return false;'>Yes</a> / No</span>");
                    }, 50);
                }
                return "<% =GetEmptyMessage()%>";
            }
        });

        setTimeout(function() {
            resizeAllGrids();
        }, 300);
    }

    function openAlts() {        
        <%--openPage(pageByID(nav_json, <% = _PGID_ANALYSIS_GRIDS_ALTS %>));--%>
        if ((window.opener) && (typeof window.opener.onOpenPage) != "undefined") { window.opener.onOpenPage(<% = _PGID_ANALYSIS_OVERALL_ALTS %>); return false; }
        if ((window.parent) && (typeof window.parent.onOpenPage) != "undefined") { window.parent.onOpenPage(<% = _PGID_ANALYSIS_OVERALL_ALTS %>); return false; }
        return false;
    }

    function switchObjPriorities(widget_index, priority_mode) {
        var btnL = document.getElementById("btnObjLocal"  + widget_index);
        var btnG = document.getElementById("btnObjGlobal" + widget_index);

        tableObjParams["priority_mode"+widget_index] = priority_mode*1;

        sendCommand("action=switch_obj_priorities_mode&value=" + priority_mode, false);        

        if ((btnL) && (btnG)) {
            switch (priority_mode*1) {
                case 0: // Local Priorities
                    btnL.style.border = "1px solid #ccc";
                    btnG.style.border = "1px solid #fff";
                    break;
                case 1: // Global Priorities
                    btnL.style.border = "1px solid #fff";
                    btnG.style.border = "1px solid #ccc";
                    break;
            }            
        }

        getObjectivesGridHTML(widget_index);
    }

    function cbObjDisplayChange(value, widget_index) {
        SelectOnlyCoveringObjectivesOption = value * 1;
        getObjectivesGridHTML(widget_index);
    }
    // end Objectives Grid

    // Consensus View Grid
    function getCVGridHTML(widget_index) {        
        var table = (($("#tableCV" + widget_index).data("dxDataGrid"))) ? $("#tableCV" + widget_index).dxDataGrid("instance") : null;
        if (table !== null) { table.dispose(); }

        var colAlternativeCaption = "";
        var colWRTCaption = "With respect to: ";
        switch (CVFilterBy*1) {
            case 0:
                colAlternativeCaption = "<%=ParseString("%%Alternative%%")%>";
                colWRTCaption += "<%=ParseString("Covering %%Objectives%%")%>";
                break;
            case 1:
                colAlternativeCaption = "<%=ParseString("%%Objectives%%")%>";
                colWRTCaption += "<%=ParseString("%%Objective%%")%>";
                break;
            case 2:
                colAlternativeCaption = "<%=ParseString("%%Objective%% / %%Alternative%%")%>";
                colWRTCaption += "<%=ParseString("%%Objective%% / Covering %%Objective%%")%>";
                break;
        }
       
        //init columns headers                
        var columns = [];
        columns.push({ "caption" : "Rank", "alignment" : "center", "allowSorting" : true, "dataField" : CVI_RANK });
        columns[columns.length - 1].cellTemplate = function(element, info) {            
            if (CanUserStartTeamTime && info.data[CVI_IS_TT_STEP_AVAIL]) {
                element.append(info.data[CVI_RANK]);
            }
        }
        columns.push({ "caption" : "Command", "visible" : false, "dataField" : CVI_COMMAND });        
        columns.push({ "caption" : "ChildIsAlt", "visible" : false, "dataField" : CVI_IS_ALT });
        columns.push({ "caption" : "ChildID", "visible" : false, "dataField" : CVI_CHILD_ID });                
        columns.push({ "caption" : colAlternativeCaption, "alignment" : "left", "allowSorting" : true, "dataField" : CVI_CHILD_NAME });
        columns[columns.length - 1].cellTemplate = function(element, info) {            
            if (info.data[CVI_IS_ALT]) {
                element.append("<span style='color:#009933;'>" + info.value + "</span>");
            } else {
                element.append(info.value);
            }
        }        
        columns.push({ "caption" : "ObjID", "visible" : false, "dataField" : CVI_OBJ_ID });
        columns.push({ "caption" : colWRTCaption, "alignment" : "left", "allowSorting" : true, "dataField" : CVI_OBJ_NAME });
        columns.push({ "caption" : "<%=ResString("tblVariance")%>, %;", "alignment" : "left", "allowSorting" : true, "width" : 150, "visible" : false, "dataField" : CVI_VARIANCE });
        columns.push({ "caption" : "<%=ResString("tblStandardDeviation")%>, %", "alignment" : "left", "allowSorting" : true, "width" : 150, "visible" : true, "dataField" : CVI_STD_DEVIATION });
        columns[columns.length - 1].cellTemplate = function(element, info) {            
            var fVal = info.value;
            var sVal = roundTo(fVal, DecimalDigits);
            var barColor = "#ffb3b3";
            if (fVal < 10) { barColor = "#8deb8d" } else { if (fVal < 20) barColor = "#fff3b5" };
            var sBar = htmlGraphBarWithValue(fVal, 100, sVal+"" != "<%=Double.NaN.ToString%>" ? sVal + "%" : fVal, "<% =ImagePath %>", 140, 11, barColor);
            element.append(sBar);
        }
        columns.push({ "caption" : "Step", "alignment" : "center", "visible" : true, "dataField" : CVI_STEP_NUM });
        columns[columns.length - 1].cellTemplate = function(element, info) {            
            if (CanUserStartTeamTime && info.data[CVI_IS_TT_STEP_AVAIL]) {
                element.append((info.data[CVI_STEP_NUM]>0 ? "<a href='' class='actions' onclick='OnStartTeamTimeCV(" + info.data[CVI_OBJ_ID] + "," + info.data[CVI_CHILD_ID] + "); return false;' style='color: #3586d6; font-weight: 600;'>" + info.data[CVI_STEP_NUM] + "</a>" : "<span class='gray'>&ndash;</span>")); // Step #
            }
        }
        columns.push({ "caption" : "IsTTStepAvailable", "visible" : false, "allowSorting" : true, "dataField" : CVI_IS_TT_STEP_AVAIL });

        //init dataset
        var dataset = cv_data;

        table = $("#tableCV" + widget_index).dxDataGrid( {
            dataSource: dataset,
            columns: columns,
            columnHidingEnabled: false,
            allowColumnResizing: true,
            columnAutoWidth: false,
            "export": {
                enabled: true,
                fileName: "Consensus View for <%=SafeFileName(App.ActiveProject.ProjectName)%>"
            },
            rowAlternationEnabled: true,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            searchPanel: {
                visible: true,
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            paging: {
                enabled: false
            },
            keyExpr: CVI_RANK,
            stateStoring: {
                enabled: true,
                type: "sessionStorage",
                storageKey: "CV_Datagrid"
            },
            noDataText: "<% = ResString("msgConsensusViewError")%>",
            loadPanel: {
                enabled: false,
            },
            width: "auto"
        });        
        setTimeout(function() {
            resizeAllGrids();
        }, 300);
    }

    function rebuildCVGrid() {
        $("div.cv_table").each( function (index, val) { getCVGridHTML($(val).attr("wid")*1); });
    }

    function refreshCVGrid() {
        $("div.cv_table").each( function (index, val) { 
            if ($(this).data("dxDataGrid")) {
                var dg = $(this).dxDataGrid("instance");
                dg.option("dataSource", cv_data);
                dg.refresh();
            }
        });
    }

    function OnStartTeamTimeCV(obj_id, alt_id) {
        return (CreatePopup("<% =PageURL(_PGID_TEAMTIME_CV) %>&objid=" + obj_id + "&altid=" + alt_id, "cv","")!="undefined");
    }

    function onEndTeamTime() {
        showLoadingPanel();
        setTimeout(function () { document.location.reload(); }, 250);
    }
    // end Consensus View Grid

    /* CV Tree View */
    var isGridView = <% = Bool2JS(SynthesisView = SynthesisViews.vtAlternatives OrElse SynthesisView = SynthesisViews.vtObjectives) %>;

    var checked_nodes = "";

    function onCheck(selectedRowKeys) {        
        checked_nodes = "";
        getCheckedCVTreeNode(selectedRowKeys);
        selectedKeys = selectedRowKeys;
        sendCommand("action=" + ACTION_CHECKED_NODES_CV + "&chk_nodes_ids=" + checked_nodes, true);
    }    

    function getCheckedCVTreeNode(nodes) {
        if ((nodes) && nodes.length > 0) {
            var nodes_len = nodes.length;
            for (var i = 0; i < nodes_len; i++) {
                var key = eval(nodes[i]);
                var id = key[0];
                var pid = key[1];
                var treeNode = getTreeNode(cv_tree_nodes_data[0], id);
                if ((treeNode)) checked_nodes += (checked_nodes == "" ? "" : ",") + treeNode.nodeid;
            }
        }
    }

    function isSATerminalNode(treeNode) {
        if (!(treeNode)) return false;
        return SynthesisView == vtCV || SynthesisView == vtASA || (treeNode.isterminal && !isGridView && treeNode.nodeid != cv_tree_nodes_data[0].nodeid) || (treeNode.isterminal && (SynthesisView == vtAlternativesChart || SynthesisView == vtObjectivesChart) && selChartType() == "comp");
    }
    
    var selectedKeys = [];
    var is_init = true;

    function initCVTree(tree_id) {
        var table = (($("#" + tree_id).data("dxTreeList"))) ? $("#" + tree_id).dxTreeList("instance") : null;
        if (table !== null) { 
            table.dispose();
        }

        var columns = [];

        //init columns headers                
        columns.push({ "caption" : "<%=ParseString("%%Objectives%%")%>", "alignment" : "left", "allowSorting" : false, "allowSearch" : false,  "allowResizing" : true, "fixed" : false, "fixedPosition" : "left", "allowHiding": false, "dataField" : "name", "width": 170, "minWidth": 170 });
        columns.push({ "caption" : "sortOrder", "alignment" : "left", "allowSorting" : true, "allowSearch" : false, "dataField" : "sortOrder", dataType: "number", "visible" : false, "sortOrder" : "asc", 
            sortingMethod: function (value1, value2) {
                // Handling null values
                if(!value1 && value2 || value1 < value2) return -1;
                if(!value1 && !value2 || value1 == value2) return 0;
                if(value1 && !value2 || value1 > value2) return 1;
                // Determines whether two strings are equivalent in the current locale
                return value1.localeCompare(value2);
            } 
        });        

        var isMultiselectEnabled = SynthesisView == vtCV || SynthesisView == vtASA || SynthesisView == vt4ASA;

        if (isMultiselectEnabled && is_init) {
            selectedKeys = [];
            getCVTreeCheckedKeys(cv_tree_nodes_data, selectedKeys);
            is_init = false;
        }

        //init priorities columns
        if ((showTreePrioritiesL || showTreePrioritiesG) && combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>) {
            <%If IsMixedOrSensitivityView() Then%>            
            columns.push(addPriorityColumn(sa_username, sa_userid < 0 ? "g" : "u", sa_userid));
            <%Else%>
            for (var i = 0; i < users_data.length; i++) {
                if (users_data[i][IDX_USER_CHECKED] == 1) {
                    columns.push(addPriorityColumn(users_data[i][IDX_USER_NAME], "u", users_data[i][IDX_USER_ID]));            
                }
            }

            for (var i = 0; i < groups_data.length; i++) {
                if (groups_data[i][IDX_USER_CHECKED] == 1) {
                    columns.push(addPriorityColumn(groups_data[i][IDX_USER_NAME], "g", groups_data[i][IDX_USER_ID]));
                }
            }
            <%End If%>
        }
        
        var focusedNode = getObjectiveByID(WRTNodeID);    
        if (getObjectiveByID(WRTNodeParentGUID) == null && focusedNode[IDX_NODE_PARENT_GUIDS].length) {
            WRTNodeParentGUID = focusedNode[IDX_NODE_PARENT_GUIDS][0];
        } else {
            WRTNodeParentGUID = "";
        }
        var focusedNodeGUID = "['" + focusedNode[IDX_NODE_GUID] + "','" + WRTNodeParentGUID + "']";
        var h = $("#" + tree_id).parent().innerHeight();

        $(".selected-node-options").toggle(WRTNodeParentGUID != "");

        if (!isMultiselectEnabled) {
            var treeNode = getTreeNode(cv_tree_nodes_data[0], focusedNodeGUID);
            if (isSATerminalNode(treeNode) && (treeNode.id != cv_tree_nodes_data[0].id)) {
                //onWRTNodeChange(getTreeNodeParent(cv_tree_nodes_data[0], focusedNodeGUID).id);
                //initCVTree("cvTree-1");
                //return;
            }
        }

        $("#" + tree_id).removeClass("dx-treelist-withlines").addClass("dx-treelist-withlines").dxTreeList({
            autoExpandAll: true,
            allowColumnResizing: true,
            columnAutoWidth: false,
            height: "100%",
            width: "100%",
            columns: columns,
            columnResizingMode: "widget",
            dataSource: cv_tree_nodes_data,
            dataStructure: "tree", //"plain",
            keyExpr: "id",
            focusedRowEnabled: true,
            focusedRowKey: isMultiselectEnabled ? "" : focusedNodeGUID,
            hoverStateEnabled: true,
            <%If SynthesisView <> SynthesisViews.vt4ASA Then%>
            onFocusedRowChanging: function (e) {                
                if ((e.rows[e.newRowIndex])) {
                    var key = eval(e.rows[e.newRowIndex].key);
                    var id = key[0];
                    var pid = key[1];
                    var treeNode = getTreeNode(cv_tree_nodes_data[0], id, pid);
                    if (isSATerminalNode(treeNode)) {
                        e.cancel = true;
                    }
                    $(".selected-node-options").toggle(pid !== "");
                }
            },
            onFocusedRowChanged: function (e) {
                if ((e.row)) { 
                    var key = eval(e.row.key);
                    var id = key[0];
                    var pid = key[1];
                    onWRTNodeChange(id, pid); 
                }
            },
            <%End If%>
            onDataErrorOccurred: null,
            onSelectionChanged: function (e) {
                //onCheck(e.component.getSelectedRowKeys("leavesOnly"));
                onCheck(e.component.getSelectedRowKeys("all"));
            },
            selection: {
                allowSelectAll: true,
                mode: isMultiselectEnabled ? "multiple" : "none",
                recursive: true
            },
            selectedRowKeys: selectedKeys,
            repaintChangesOnly: true,
            onCellPrepared: function(e) {
                if (e.rowType === "data") {
                    if (e.data.iscategory) {
                        //e.cellElement.css("color", "#0000ff");
                        e.cellElement.addClass("categorical");
                    }
                    if (isSATerminalNode(e.data) && !isMultiselectEnabled) {
                        e.cellElement.css("color", "#999");                    
                    }
                }

                getDxTreeListNodeConnectingLinesOnCellPrepared(e);
            },
            showBorders: false,
            <%--showColumnHeaders: (showTreePrioritiesL || showTreePrioritiesG) && combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>,--%>
            showColumnHeaders: showTreePrioritiesL || showTreePrioritiesG,
            showColumnLines: true,
            showRowLines: false,
            scrolling: {
                showScrollbar: "always",
                mode: "standard"
            },
            sorting: { mode: "single" },
            stateStoring: { enabled: false},
            visible: true,
            wordWrapEnabled: false
        });
    }

    function getCVTreeCheckedKeys(nodes, keys) {
        var nodes_len = nodes.length;
        for (var i = 0; i < nodes_len; i++) {
            if (nodes[i].checked) {
                keys.push(nodes[i].id);
            }
            if (nodes[i].items.length > 0) { getCVTreeCheckedKeys(nodes[i].items, keys); }
        }
    }

        function addPriorityColumn(name, pref, i) {
            var cellTemplateFunc = function(element, info) {
                var data = info.data[info.column.dataField];
                if (typeof data !== "undefined") {
                    switch (data) {
                        case UNDEFINED_INTEGER_VALUE:
                            element.html(lblNA);
                            break;
                        case UNDEFINED_INTEGER_VALUE + 1:
                            element.html("");
                            break;
                        default:
                            var percent = roundTo(data * 100, DecimalDigits);
                            element.html(percent + "%");
                            break;
                    }
                }
            };
            var columnsPriorities = { caption : name, "alignment" : "center", "allowSorting" : false, "allowSearch" : false, columns : [] };
            if (showTreePrioritiesL) columnsPriorities.columns.push({ "caption" : "<% = ResString("optLocal")  %>", "alignment" : "right", "allowSorting" : false, "allowResizing" : false, "width" : 65, "allowSearch" : false, "dataField" : pref + "l" + i, cellTemplate: cellTemplateFunc });
            if (showTreePrioritiesG) columnsPriorities.columns.push({ "caption" : "<% = ResString("optGlobal") %>", "alignment" : "right", "allowSorting" : false, "allowResizing" : false, "width" : 65, "allowSearch" : false, "dataField" : pref + "g" + i, cellTemplate: cellTemplateFunc });
            return columnsPriorities;
        }

        function getTreeNode(root, key) {
            if (typeof root != "undefined") {
                if (root.id.indexOf(key) !== -1) return root;
                var items_len = root.items.length;
                for (var i = 0; i < items_len; i++) {
                    var c = getTreeNode(root.items[i], key);
                    if (typeof c != "undefined") return c;
                }
            }
            return;
        }

        function getTreeNodeParent(root, key, parent) {
            if (typeof root != "undefined") {
                if (root.id == key && typeof parent != "undefined") return parent;
                var items_len = root.items.length;
                for (var i = 0; i < items_len; i++) {
                    var c = getTreeNodeParent(root.items[i], key, root.items[i]);
                    if (typeof c != "undefined") return c;
                }
            }
            return;
        }
        /* end CV Tree View */

        function cbShowStepsWithNoJudgmentChange(value) {
            sendCommand("action=" + ACTION_SHOW_ALL_ROWS_CV + "&value=" + (value ? "1" : "0"), true);
        }

        function cbCVFilterRowsChange(value) {
            sendCommand("action=" + ACTION_FILTER_ROWS_CV + "&value=" + value, true);
        }

        function cbCVFilterByChange(value) {
            CVFilterBy = value * 1;
            sendCommand("action=" + ACTION_FILTER_BY_CV + "&value=" + value, true);
        }

        //function cbShowLikelihoodsGivenSourcesChange(value) {
        //    sendCommand("action=show_likelihoods_given_sources&value=" + value, true);
        //}

    // SENSITIVITIES
        var RiskParam = <% = RiskParam %>;

        function loadSA(widget_index, viewMode) {
            callAPI("pm/dashboard/?action=<%=DashboardWebAPI.ACTION_DSA_INIT_DATA%>&force_computed=<% = Bool2JS(_PAGESLIST_RISK_SENSITIVITIES.Contains(CurrentPageID)) %>", {view: viewMode, risk: RiskParam}, function (data) {
                sa_objs = data.objs;
                sa_alts = data.alts;
                sa_comps = data.comps;
                sa_userid = data.saUserID;
                sa_username = data.saUserName;
                asa_data = data.ASAdata;
                $("#divInnerContent" + widget_index).html("<canvas id='DSACanvas" + widget_index + "' class='DSACanvas no-tooltip' style='background:#eeeeee'></canvas>");
                var w = $("#trWidgetContent" + widget_index).width();
                var h = $("#trWidgetContent" + widget_index).height();
                initSACanvas(widget_index, w, h, viewMode);
            });
        }

        function initSACanvas(widget_index, width, height, viewMode) {
            var canvas = document.getElementById('DSACanvas' + widget_index);
            if (!(canvas)) return false;
            canvas.width  = width;
            canvas.height = height;
            var asaData = asa_data;
            <%If SynthesisView = SynthesisViews.vt4ASA Then%>
            asaData = asa_data[widget_index];
            <%End If%>

            $('#DSACanvas' + widget_index).sa({
                viewMode: viewMode,
                hideButtons: false,
                isMultiView: (SynthesisView == vtMixed || SynthesisView == vtDashboard),
                objs: sa_objs,
                alts: sa_alts,
                ASAdata: asaData,
                ASAPageSize: <%=PM.Parameters.AsaPageSize %>,
                ASACurrentPage: <%=PM.Parameters.AsaPageNum %>,
                ASASortBy: asa_sortby,
                SAAltsSortBy: sa_alts_sortby,
                altFilterValue: sa_altfilter,
                valDigits: DecimalDigits,
                normalizationMode: getNormModeSA(),
                showComponents: showComponents,
                selected_events_type: selected_events_type,
                showMarkers: showSAMarkers,
                sessionID: 'saState' + <%=App.ProjectID%>,
                PSAShowLines: true,
                DSAComponentsData: sa_comps,
                DSAActiveSorting: <% = Bool2JS(PM.Parameters.DsaActiveSorting)%>,
                GSAShowLegend: <%=Bool2JS(SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard Or SynthesisView = SynthesisViews.vt2D Or SynthesisView = SynthesisViews.vtDSA Or SynthesisView = SynthesisViews.vtPSA Or SynthesisView = SynthesisViews.vtGSA)%>,
                titleAlternatives: "<%=ParseString("%%Alternatives%%")%>",
                titleObjectives: "<%=ParseString("%%Objectives%%")%>",
                onSortingEvent: onSASortingEvent,
                onMouseUpEvent: onSAMouseUp,
                userID: sa_userid,
                userName: (viewMode == "ASA" ? getUserByID(asaData.userID)[IDX_USER_NAME] : sa_username),
                });
        }
    
    function onSAMouseUp(values, objids, element) {
        callAPI("pm/dashboard/?action=" + ACTION_DSA_UPDATE_VALUES, {
            "values": values, 
            "objIds": objids, 
            "canvasid": element.options.canvas.id, 
            "sauserid": sa_userid,
            "iswrtgoal": (WRTState == 0),
            "risk": RiskParam
            }, onSAMouseUpFinished, true)
        //sendCommand("action=" + ACTION_DSA_UPDATE_VALUES + "&values=" + values + "&objIds=" + objids + "&id=" + element.options.canvas.id, false);
    }

    function onSAMouseUpFinished(received_data) {
        var canvasName = received_data[1];
        $("#" + canvasName).sa("GradientMinValues", received_data[2]);
    }

        function switchGSALegend(widget_index, is_show) {
            var btnShow = document.getElementById("btnGSAShowLegend" + widget_index);
            var btnHide = document.getElementById("btnGSAHideLegend" + widget_index);

            if ((btnShow) && (btnHide)) {
                switch (is_show*1) {
                    case 1: // Show Legends
                        btnShow.style.border = "1px solid #ccc";
                        btnHide.style.border = "1px solid #fff";
                        $('#DSACanvas'+widget_index).sa("option", "GSAShowLegend", true).sa("redrawSA");
                        break;
                    case 0: // Hide Legends
                        btnShow.style.border = "1px solid #fff";
                        btnHide.style.border = "1px solid #ccc";
                        $('#DSACanvas'+widget_index).sa("option", "GSAShowLegend", false).sa("redrawSA");
                        break;
                }            
            }        
        }
        // end SENSITIVITIES

        function getNormModeSA() {
            var normMode = 'unnormalized';
            switch (NormalizeMode){
                case ntNormalizedForAll:
                    normMode = 'normAll';
                    break;
                case ntNormalizedSum100:
                    normMode = 'normSelected';
                    break;
                case ntNormalizedMul100:
                    normMode = 'normMax100';
                    break;
                default:
                    normMode = 'unnormalized';
            }
            return normMode;
        }

        function cbDecimalsChange(value) {
            DecimalDigits = value*1;
            sendCommand("action=" + ACTION_DECIMALS + "&value=" + value, true);
            $('canvas.DSACanvas').sa("option", "valDigits", value);
            $('canvas.DSACanvas').sa("redrawSA");
            // reset CV grids (no data updates, just Decimals)        
            refreshCVGrid();
        }

        function onActiveSorting(value) {
            sendCommand("action=dsa_active_sorting&value=" + value, true);
            $('canvas.DSACanvas').sa("option", "DSAActiveSorting", value);
            //$('canvas.DSACanvas').sa("resetSA");
            $('canvas.DSACanvas').sa("redrawSA");
        }

        function onShowComponents(value) {
            showComponents = value;
            sendCommand("action=show_components&value=" + value, true);
            $('canvas.DSACanvas').sa("option", "showComponents", value);
            $('canvas.DSACanvas').sa("redrawSA");
        }

        function showMarkers(value) {
            showSAMarkers = value;
            localStorage.setItem("SynthesisShowSAMarkers<%=App.ProjectID%>", value.toString());
            $('canvas.DSACanvas').sa("option", "showMarkers", value);
            $('canvas.DSACanvas').sa("redrawSA");
        }

        function setOldVal() {
            var cb = document.getElementById("cbAltsFilter");
            if ((cb)) old_filter_val = cb.value;
        }

        function resetOldVal() {
            var cb = document.getElementById("cbAltsFilter");
            if ((cb)) cb.value = old_filter_val;
            updateToolbar();
        }

        function applyAltsFilter(value) {        
            if (value == -4) {
                $("#tdScenarioName").show();
            } else {
                $("#tdScenarioName").hide();
            }

            switch (value) {
                case -2:
                    filterAltsAdvanced();
                    break;
                case -3:
                    filterAltsCustom();
                    break;
                case -5:
                    filterAltsAttributes();
                    break;
                default:
                    sendCommand("action=" + ACTION_ALTS_FILTER + "&value=" + value, true);
                    setOldVal();
            }        
            updateToolbar();
        }

        function editAdvClick() {
            var cb = document.getElementById("cbAltsFilter");
            if ((cb)) {
                switch (cb.value*1) {
                    case -2:
                        filterAltsAdvanced();
                        break;
                    case -3:
                        filterAltsCustom();
                        break;
                    case -5:
                        filterAltsAttributes();
                        break;
                }
            }
        }

        function updateToolbar() {
            var lbl = document.getElementById("lblAdvFilter");
            if ((lbl)) lbl.style.display = "none";
            var cb = document.getElementById("cbAltsFilter");
            if ((cb)) {
                if (cb.value*1 == -2 || cb.value*1 == -3 || cb.value*1 == -5) {
                    $("#btnAdvEdit").show();
                } else {
                    $("#btnAdvEdit").hide();
                }

                var cbN = document.getElementById("cbAdvAltsNum");
                var cbP = document.getElementById("cbAdvUsers");

                if ((lbl) && (cbN) && (cbP) && cb.value*1 == -2) {
                    var direction = "top";
                    if ([-105, -110, -125].indexOf(cb.value*1) > -1) direction = "bottom";
                    lbl.innerHTML = "<small>Showing " + direction + " " + cbN.options[cbN.selectedIndex].value + "&nbsp;<%=ParseString("%%alternatives%%")%> based on " + cbP.options[cbP.selectedIndex].text + " priorities</small>";
                    lbl.style.display = "";
                }

                // enable or disable the "Normalize for Selected" depending on applied filter
                var cbNO = document.getElementById("cbNormalizeOptions");
                <% If CurrentPageID <> _PGID_ANALYSIS_ASA AndAlso CurrentPageID <> _PGID_ANALYSIS_4ASA Then %>
                if ((cbNO)) {
                    //cbNO.options[2].disabled = cb.value*1 == -1;
                    if (cb.value*1 == -1 && cbNO.selectedIndex == 2) cbNO.selectedIndex = 1;
                }            
                <% End If %>
            }
            $(".on_advanced").toggle(isAdvancedMode);
            //$("#tdOptUseCISWeight").toggle(isAdvancedMode);
            //$("#tdOptSynthMode").toggle(isAdvancedMode);
            //$("#tdOptCombinedMode").toggle(isAdvancedMode);
            //$("#tdOptNormalization").toggle(isAdvancedMode);

            // update the Select Events icon
            var img = document.getElementById("imgFilterEvents");        
            if ((img)) {
                var alts_data_len = alts_data.length;
                var checkedEvents = 0;
                for (var k = 0; k < alts_data_len; k++) {
                    if (alts_data[k][IDX_ALT_ENABLED] == 1) checkedEvents += 1;
                }
                img.className = (checkedEvents == alts_data_len ? "fas fa-filter fa-2x" : " fas fa-exclamation-circle fa-2x");
            }

            $("#cbShowMarkers").prop("checked", showSAMarkers);
        }

        function updateInfobar() {
            var cb = document.getElementById("cbNormalizeOptions");
            if ((cb)) {
                var options = cb.getElementsByTagName("option");
                options[0].disabled = false;
                cb.value = NormalizeMode;
                if (synthMode == 0) {
                    options[0].disabled = true;
                }
            }

            var lblWrt = document.getElementById("lblWrtNode");
            if ((lblWrt)) {
                var sNodeName = "";
                var wrt_node = getObjectiveByID(WRTNodeID);
                if ((wrt_node)) sNodeName = wrt_node[IDX_NODE_NAME];
                if ($("#btnWRTState").length && WRTState == 0) sNodeName = nodes_data[0][IDX_NODE_NAME];
                if (<% = Bool2JS(SynthesisView <> SynthesisViews.vtASA AndAlso SynthesisView <> SynthesisViews.vt4ASA) %>) {
                    <% If Not RiskPageIDs.Contains(CurrentPageID) Then %>
                    if ((wrt_node) && wrt_node[IDX_NODE_ID] == nodes_data[0][IDX_NODE_ID]) {
                            lblWrt.innerHTML = "<%If PM.IsRiskProject Then%><%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("%%Likelihoods%%"), ParseString("%%Impacts%%"))%><%Else%><%End If%>";
                    } else {
                        if (showLikelihoodsGivenSources) {
                            lblWrt.innerHTML = (SynthesisView == vtObjectives ? "for nodes below" : "<%If PM.IsRiskProject Then%><%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("%%Likelihoods%% Due To"), ParseString("%%Impact%% Of %%Alternative%% On"))%><%Else%>with respect to<%End If%>") + " " + sNodeName;
                        } else {
                            lblWrt.innerHTML = (SynthesisView == vtObjectives ? "for nodes below" : "<%If PM.IsRiskProject Then%><%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("Vulnerabilities Of %%Alternatives%% To"), ParseString("%%Alternative%% Consequences To"))%><%Else%>with respect to<%End If%>") + " " + sNodeName;
                        }
                    }
                    <% Else %>
                    if ((wrt_node) && wrt_node[IDX_NODE_ID] == nodes_data[0][IDX_NODE_ID]) {
                            lblWrt.innerHTML = "";
                    } else {
                        lblWrt.innerHTML = "<% = ParseString("%%Risks%% with respect to") %>" + " " + sNodeName;
                    }
                    <% End If %>
                } else {
                    lblWrt.innerHTML = ""
                }
            }

            var lblUserNames = document.getElementById("lblTitleUserNames");
            if ((lblUserNames) && SynthesisView != vtAlternatives && SynthesisView != vtObjectives ) {                
                <%If IsMixedOrSensitivityView Then%>
                lblUserNames.innerHTML = " for " + sa_username;
                <%If SynthesisView = SynthesisViews.vt4ASA Then%>
                lblUserNames.innerHTML = ""
                <%End If%>
                <%Else%>
                var sUserNames = "";
                for (var i = 0; i < users_data.length; i++) {
                    if (columns_user_ids.indexOf(users_data[i][IDX_USER_ID]) != -1) {
                        sUserNames += (sUserNames == "" ? "" : ", ") + users_data[i][IDX_USER_NAME];
                    }
                }
                for (var i = 0; i < groups_data.length; i++) {
                    if (columns_user_ids.indexOf(groups_data[i][IDX_USER_ID]) != -1) {
                        sUserNames += (sUserNames == "" ? "" : ", ") + groups_data[i][IDX_USER_NAME];
                    }
                }
                lblUserNames.innerHTML = sUserNames == "" ? "" : " for " + sUserNames;
                <%End If%>
            }
            updateWRTState();
        }
    
        function onWRTNodeChange(guid, parent_guid) {
            var treeNode = getTreeNode(cv_tree_nodes_data[0], guid);
            if ((treeNode)) {
                WRTNodeChange(treeNode.nodeid, parent_guid);
                updateInfobar();
            }
        }

        function WRTNodeChange(value, parent_guid) {
            if (WRTNodeID == value * 1 && WRTNodeParentGUID == parent_guid + "") return;
            WRTNodeParentGUID = parent_guid + "";
            WRTNodeID = value * 1;
            updateInfobar();
            sendCommand("action=" + ACTION_GRID_WRT_NODE_ID + "&value=" + value + "&pid=" + WRTNodeParentGUID, true);
        }

    function rbSynthesisModeChange(value) {
        synthMode = value*1;                
        updateInfobar();
        sendCommand("action=" + ACTION_SYNTHESIS_MODE + "&value=" + value, true);
    }

    function exportSA() {
        var dsaIDs = getDSAControlsNames();
        for (var i = 0; i < dsaIDs.length; i++) {
            $('#'+dsaIDs[i]).sa("export", curPrjData.Name);
        };
    }

    function toggleWRTState() {
        if (!$("#btnWRTState").hasClass("disabled")) {
            WRTState = WRTState == 0 ? 1 : 0;

            updateWRTState();

            sendCommand("action=" + ACTION_GRID_WRT_STATE + "&value=" + WRTState, true);
        }
    }

    function updateWRTState() {
        // make button disabled/enabled deending on WRT node
        var is_disabled = WRTNodeID*1 == nodes_data[0][IDX_NODE_ID]*1;
        var allowComponents = (WRTNodeID*1 == nodes_data[0][IDX_NODE_ID]*1 || WRTState == 1);
        $("#cbShowComponents").prop("disabled", !allowComponents);
        if (!allowComponents) { 
            $("#cbShowComponents").prop("checked", false);
            onShowComponents(false);
        };
        var tOpacity = (is_disabled ? 0.5 : 1);
        $("#btnWRTStateImg").fadeTo(0, tOpacity);
        if (is_disabled) {
            $("#btnWRTState").css({cursor:"default"}).addClass("disabled");
        } else {
            $("#btnWRTState").css({cursor:"pointer"}).removeClass("disabled");
        }
        
        // update the button tooltip
        var btn_title = "";
        if (!is_disabled) {
            if (WRTState == 0) {
                // wrt Goal
                btn_title = "WRT " + nodes_data[0][IDX_NODE_NAME];
            } else {
                var wrt_node = getObjectiveByID(WRTNodeID);
                if ((wrt_node)) btn_title = "WRT " + wrt_node[IDX_NODE_NAME];
            }
        }
        var btn = document.getElementById("btnWRTState");
        if ((btn)) btn.title = btn_title;

        is_checked = WRTState == 0; // if WRT Goal then 'checked'

        // emulate the button 'pressed'
        $("#btnWRTState").css( { border : is_checked ? "1px solid #999" : "1px solid #e0e0e0", background : is_checked ? "#e0e0e0" : "#fefefe" } );

        // update the button image
        var img = document.getElementById("btnWRTStateImg");
        if ((img)) {
            img.src = "<%=ImagePath()%>old/" + (is_checked ? "objectives_goal_32.png" : "objectives_node_32.png");
            img.className = (is_checked ? "toggle_btn_checked" : "toggle_btn_unchecked");
        }
    }

    function DisableAnalysisPagesOnAIP(doDisable, IsUserInitiated) {
        menu_setOption(menu_option_noAIP, doDisable);
        if (typeof onSetAIP == "function") onSetAIP();
        if ((window.parent) && (typeof window.parent.onSetAIP == "function") && (window.parent.document != document)) {
            window.parent.onSetAIP();
        }
    }

    function rbCombinedModeChange(value) {
        combinedMode = value*1;
        <%--if (combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>) { $(".ObjectivesPrioritiesSwitch").show(); } else { $(".ObjectivesPrioritiesSwitch").hide(); } --%>
        DisableAnalysisPagesOnAIP(value != <%=CInt(CombinedCalculationsMode.cmAIJ)%>, true);
        sendCommand("action=" + ACTION_COMBINED_MODE + "&value=" + value, true);
    }

    function cbUseCISChange(value) {
        sendCommand("action=" + ACTION_CIS_MODE + "&value=" + (value ? "1" : "0"), true);
    }

    function cbUseWeightsChange(value) {
        sendCommand("action=" + ACTION_USER_WEIGHTS_MODE + "&value=" + (value ? "1" : "0"), true);
    }

    function cbIncludeIdealChange(value) {
        sendCommand("action=include_ideal&value=" + (value ? "1" : "0"), true);
    }

    function cbNormalizeModeChange(value) {
        NormalizeMode = value*1;
        sendCommand("action=" + ACTION_NORMALIZE_MODE + "&value=" + value, true);                
        <%--OnNormalizeLocal(<%=Bool2JS(PM.ActiveHierarchy = ECHierarchyID.hidImpact)%>, value);--%>

        $('canvas.DSACanvas').sa("option", "normalizationMode", getNormModeSA());
        //$('canvas.DSACanvas').sa("redrawSA");
    }

    function SelectAllUsers(action) {
        var cb_arr = $("input:checkbox.user_cb");
        $.each(cb_arr, function(index, val) { 
            if (action == 0) val.checked = false; 
            if (action == 1) val.checked = true;
            if (action == 2) { val.checked = val.getAttribute("has_data") == "1" }
        });
        updateUsersWithDataInGroupCheckState();
        return false;
    }

    var dialog_result = false;

    function showSelectUsersDialog() {
        selectUsersDialog();
        return; 

        if (users_data.length == 0 || groups_data.length == 0) {
            callAPI("pm/dashboard/?action=get_users_and_groups", {}, function (data) {
                users_data = data.users_data;
                groups_data = data.groups_data;
                selectUsersDialog();
            }, false, 2000);
        } else {
            selectUsersDialog();
        }
    }

    function selectUsersDialog() {
        if ((dlg_users)) dlg_users = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_users = $("#divUsersAndGroups").dialog({
            autoOpen: false,
            width: 880,
            height: 435,
            minHeight: 150,
            maxHeight: 550,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "<%=SafeFormString(ResString("btnParticipantsAndGroups"))%>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", click: function() { dialog_result = true; dlg_users.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_users.dialog( "close" ); }}],
            open:  function() { 
                $("body").css("overflow", "hidden"); 
                initUsersTable(); 
                initGroupsTable(); 
                document.body.style.cursor = "default";
            },
            close: onUsersDialogClose
        });
        dlg_users.dialog('open');
    }

    function onUsersDialogClose() {
        $("body").css("overflow", "auto");
        if (dialog_result) {
            <%If Not IsMixedOrSensitivityView() Then%>
            var params = "";
            var cb_arr = $("input:checkbox.user_cb");
            $.each(cb_arr, function(index, val) { var uid = val.getAttribute("uid")*1; var u = getUserByID(uid); if ((u)) u[IDX_USER_CHECKED] = (val.checked ? 1 : 0); });
            for (var i = 0; i < users_data.length; i++) {                
                if (users_data[i][IDX_USER_CHECKED] > 0) { params += (params == "" ? "" : ",") + users_data[i][IDX_USER_ID]; } 
            }
            for (var i = 0; i < groups_data.length; i++) {                
                if (groups_data[i][IDX_USER_CHECKED] > 0) { params += (params == "" ? "" : ",") + groups_data[i][IDX_USER_ID]; } 
            }
            if (params == "") params = "<% = COMBINED_USER_ID.ToString %>";
            if (!$("input:checkbox.user_cb:checked").length) getUserByID(<% = COMBINED_USER_ID%>)[IDX_USER_CHECKED] = 1;
            sendCommand("action="+ACTION_SELECTED_USERS+"&ids="+params, true);        
            <%Else%>
            var rb_arr = $("input:radio.user_cb");
            $.each(rb_arr, function(index, val) { if (val.checked) { var uid = val.getAttribute("uid")*1; sa_userid = uid; return false; }});
            sendCommand("action=" + ACTION_SELECTED_USER + "&id=" + sa_userid, true);
            <%End If%>
        }        
    }
      
    function getSelectedUserGroup(userGroupArray) {
        for (var i = 0; i < userGroupArray.length; i++) {
            if (userGroupArray[i][IDX_USER_CHECKED] > 0) {
                return userGroupArray[i];
            };
        };
        return null;
    }

    function getUsersGroupsWithData(userGroupArray) {
        var resVal = [];
        var isCIS = document.getElementById("cbCIS").checked;
        for (var i = 0; i < userGroupArray.length; i++) {
            var item = userGroupArray[i];
            if (item[IDX_USER_HAS_DATA] == 1 || isCIS) {
                resVal.push(item);
            };
        };
        return resVal;
    }

    function sortUsersGroupsByName(userGroupArray) {
        userGroupArray.sort(function (a, b) {
            return ('' + a[IDX_USER_NAME]).localeCompare(b[IDX_USER_NAME]);
        })
    }

    function getUserGroupIDXByID(userGroupArray, id) {
        for (var i = 0; i < userGroupArray.length; i++) {
            var item = userGroupArray[i];
            if (item[IDX_USER_ID] == id) return i;
        };
        return -1;
    }

    function selectPrevUser() {
        var uid = -1;
        var usersWithData = getUsersGroupsWithData(users_data);
        var groupsWithData = getUsersGroupsWithData(groups_data);
        sortUsersGroupsByName(usersWithData);
        sortUsersGroupsByName(groupsWithData);

        var selUser = getSelectedUserGroup(users_data);
        var selGroup = getSelectedUserGroup(groups_data);

        var selUID = -1;
        var selIDX = -1;
        if (selUser !== null) {
            selUser[IDX_USER_CHECKED] = 0;
            selUID = selUser[IDX_USER_ID];
            selIDX = getUserGroupIDXByID(usersWithData, selUID);
            if ((selIDX - 1) >= 0) {
                uid = usersWithData[selIDX - 1][IDX_USER_ID];
                usersWithData[selIDX - 1][IDX_USER_CHECKED] = 1;
            } else {
                uid = groupsWithData[groupsWithData.length - 1][IDX_USER_ID];
                groupsWithData[groupsWithData.length - 1][IDX_USER_CHECKED] = 1;
            }
        }
        if (selGroup !== null) {
            selGroup[IDX_USER_CHECKED] = 0;
            selUID = selGroup[IDX_USER_ID];
            selIDX = getUserGroupIDXByID(groupsWithData, selUID);
            if ((selIDX - 1) >= 0) {
                uid = groupsWithData[selIDX - 1][IDX_USER_ID];
                groupsWithData[selIDX - 1][IDX_USER_CHECKED] = 1;
            } else {
                uid = usersWithData[usersWithData.length - 1][IDX_USER_ID];
                usersWithData[usersWithData.length - 1][IDX_USER_CHECKED] = 1;
            }
        }

        sendCommand("action=" + ACTION_SELECTED_USER + "&id=" + uid, true);
    }

    function selectNextUser() {
        var uid = -1;
        var usersWithData = getUsersGroupsWithData(users_data);
        var groupsWithData = getUsersGroupsWithData(groups_data);
        sortUsersGroupsByName(usersWithData);
        sortUsersGroupsByName(groupsWithData);

        var selUser = getSelectedUserGroup(users_data);
        var selGroup = getSelectedUserGroup(groups_data);

        var selUID = -1;
        var selIDX = -1;
        if (selUser !== null) {
            selUser[IDX_USER_CHECKED] = 0;
            selUID = selUser[IDX_USER_ID];
            selIDX = getUserGroupIDXByID(usersWithData, selUID);
            if ((selIDX + 1) < usersWithData.length) {
                uid = usersWithData[selIDX + 1][IDX_USER_ID];
                usersWithData[selIDX + 1][IDX_USER_CHECKED] = 1;
            } else {
                uid = groupsWithData[0][IDX_USER_ID];
                groupsWithData[0][IDX_USER_CHECKED] = 1;
            }
        }
        if (selGroup !== null) {
            selGroup[IDX_USER_CHECKED] = 0;
            selUID = selGroup[IDX_USER_ID];
            selIDX = getUserGroupIDXByID(groupsWithData, selUID);
            if ((selIDX + 1) < groupsWithData.length) {
                uid = groupsWithData[selIDX + 1][IDX_USER_ID];
                groupsWithData[selIDX + 1][IDX_USER_CHECKED] = 1;
            } else {
                uid = usersWithData[0][IDX_USER_ID];
                usersWithData[0][IDX_USER_CHECKED] = 1;
            }
        }

        sendCommand("action=" + ACTION_SELECTED_USER + "&id=" + uid, true);
    }

    function initUsersTable() {        
        var columns = [];

        //init columns headers                
        columns.push({ "title" : "", "class" : "td_center", "sortable" : true, "searchable" : false });
        columns.push({ "title" : "UserID", "bVisible" : false, "searchable" : false });
        columns.push({ "title" : "<%=ResString("tblSyncUserName")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });
        columns.push({ "title" : "<%=ResString("tblEmailAddress")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });        
        columns.push({ "title" : "<nobr><%=ResString("lblHasData")%>&nbsp;&nbsp;</nobr>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });
        
        var isCIS = document.getElementById("cbCIS").checked;

        $.each(columns, function(index, val) { val.title += "&nbsp;&nbsp;"; });
        $('#tableUsers').empty();
        table_users = $('#tableUsers').DataTable( {
            dom: 'frti',
            data: users_data,
            columns: columns,
            destroy: true,
            paging:    false,
            ordering:  true,
            "order": [[ 2, 'asc' ]],
            scrollY: 245,
            //scrollX: true,
            stateSave: false,
            searching: true,
            info:      false,
            "rowCallback": function( row, data, index ) {                
                <%If IsMixedOrSensitivityView() Then%>
                $("td:eq(0)", row).html("<input type='radio' name='participant_radio' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' " + (!(data[IDX_USER_HAS_DATA] == 1 || isCIS), " disabled='disabled' ", "") + " "+(data[IDX_USER_ID] == sa_userid ? " checked " : "")+" onclick='updateUsersWithDataInGroupCheckState();' >");
                <%Else%>
                $("td:eq(0)", row).html("<input type='checkbox' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' " + (!(data[IDX_USER_HAS_DATA] == 1 || isCIS), " disabled='disabled' ", "") + " "+(data[IDX_USER_CHECKED] == 1?" checked ":"")+" onclick='updateUsersWithDataInGroupCheckState();' >");
                <%End If%>
                $("td:eq(1)", row).html(htmlEscape(data[IDX_USER_NAME]));
                $("td:eq(3)", row).html((data[IDX_USER_HAS_DATA] == 1 ? "<%=ResString("lblYes")%>" : ""));
                if (!(data[IDX_USER_HAS_DATA] == 1 || isCIS)) { $("td", row).css("color", "#909090"); }
            },
            "language" : {"emptyTable" : "<h6 style='margin:2em 10em'><nobr><% =GetEmptyMessage()%></nobr></h6>"}
        });
        
        setTimeout(function () { $(".dataTables_filter").css({"float":"left", "padding-bottom":"10px"}); }, 100);
        setTimeout(function () { $("input[type=search]").focus(); }, 1000);

        // search in groups table when typing in a search field of the users table
        table_users.on('search.dt', function () {
            table_groups.search(table_users.search()).draw();
        } );
    }

    function allGroupUsersWithDataChecked(grp_id) {
        var retVal = true;

        var g = [];
        for (var i = 0; i < groups_data.length; i++) {
            if (groups_data[i][IDX_USER_ID] == grp_id) g = groups_data[i];
        }

        if (g.length > 0) {
            if (g[IDX_GROUP_PARTICIPANTS].length == 0) retVal = false;
            for (var i = 0; i < g[IDX_GROUP_PARTICIPANTS].length; i++) {
                var u = getUserByID(g[IDX_GROUP_PARTICIPANTS][i]);                                
                if ((u)) {
                    if (u[IDX_USER_HAS_DATA] == 1 && u[IDX_USER_CHECKED] != 1) retVal = false;
                }
            }
        }

        return retVal;
    }

    function checkUsersWithDataInGroup(grp_id, chk) {
        var g = [];
        for (var i = 0; i < groups_data.length; i++) {
            if (groups_data[i][IDX_USER_ID] == grp_id) g = groups_data[i];
        }

        if (g.length > 0) {
            if (g[IDX_GROUP_PARTICIPANTS].length == 0) retVal = false;
            var cb_arr = $("input:checkbox.user_cb");
            for (var i = 0; i < g[IDX_GROUP_PARTICIPANTS].length; i++) {
                var u = getUserByID(g[IDX_GROUP_PARTICIPANTS][i]);                                
                if ((u) && u[IDX_USER_HAS_DATA] == 1) {
                    $.each(cb_arr, function(index, val) { var uid = val.getAttribute("uid")*1; if (uid == g[IDX_GROUP_PARTICIPANTS][i]) val.checked = chk; });
                }
            }
        }
    }

    function updateUsersWithDataInGroupCheckState() {        
        var cb_arr = $("input:checkbox.group_all_users_cb");
        $.each(cb_arr, function(index, val) { 
            var gid = val.getAttribute("uid")*1; 
            var g = [];
            for (var i = 0; i < groups_data.length; i++) {
                if (groups_data[i][IDX_USER_ID] == gid) g = groups_data[i];
            }
            if (g.length > 0) {                
                var u_arr = $("input:checkbox.user_cb");
                var all_checked = g[IDX_GROUP_PARTICIPANTS].length > 0;
                $.each(u_arr, function(u_index, u_val) { 
                    var uid = u_val.getAttribute("uid")*1; 
                    var u = getUserByID(uid);
                    if ($.inArray(uid, g[IDX_GROUP_PARTICIPANTS]) >= 0 && u[IDX_USER_HAS_DATA] == 1 && !u_val.checked) all_checked = false; 
                });
                val.checked = all_checked;
            }
        });
    }

    function initGroupsTable() {        
        var columns = [];

        //init columns headers                
        columns.push({ "title" : "", "class" : "td_center", "sortable" : true, "searchable" : false });
        columns.push({ "title" : "GroupID", "bVisible" : false, "searchable" : false });
        columns.push({ "title" : "<%=ResString("lblGroupName")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });
        columns.push({ "title" : "<nobr><%=ResString("lblHasData")%>&nbsp;&nbsp;</nobr>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });        
        <%If Not IsMixedOrSensitivityView() Then%>
        columns.push({ "title" : "<small>Select all users with data</small>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });
        <%End If%>
        $.each(columns, function(index, val) { val.title += "&nbsp;&nbsp;"; });
        $('#tableGroups').empty();

        table_groups = $('#tableGroups').DataTable( {
            dom: 'rti',
            data: groups_data,
            columns: columns,
            destroy: true,
            paging:    false,
            ordering:  true,
            "order": [[ 0, 'desc' ]],
            scrollY: 245,
            //scrollX: true,
            stateSave: false,
            searching: true,
            info:      false,
            "rowCallback": function( row, data, index ) {
                <%If IsMixedOrSensitivityView() Then%>
                $("td:eq(0)", row).html("<input type='radio' class='user_cb' name='participant_radio' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' "+(data[IDX_USER_ID] == sa_userid?" checked ":"")+" >");
                <%Else%>
                $("td:eq(0)", row).html("<input type='checkbox' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' "+(data[IDX_USER_CHECKED] == 1?" checked ":"")+" >");
                <%End If%>                
                $("td:eq(1)", row).html((data[IDX_USER_ID] != -1 ? "<img id='imgGrp" + data[IDX_USER_ID] + "' src='<% =ImagePath%>old/plus.gif' width='9' height='9' border='0' style='margin-top:2px; margin-right:4px; cursor:pointer;' onclick='expandGroupUsers(" + data[IDX_USER_ID] + ");' >" : "") + htmlEscape(data[IDX_USER_NAME]) + "<div id='divExpandedUsers" + data[IDX_USER_ID] + "'></div>");
                $("td:eq(2)", row).html((data[IDX_USER_HAS_DATA] == 1 ? "<%=ResString("lblYes")%>" : "<%=ResString("lblNo")%>"));                
                $("td:eq(3)", row).html("<input type='checkbox' class='group_all_users_cb' uid='"+data[IDX_USER_ID]+"' " + (allGroupUsersWithDataChecked(data[IDX_USER_ID]) ? " checked ":"")+" onclick='checkUsersWithDataInGroup(\"" + data[IDX_USER_ID] + "\", this.checked);' " + (data[IDX_GROUP_PARTICIPANTS].length == 0 ? " disabled='disabled' " : "") + " >");                
                if (data[IDX_USER_HAS_DATA] != 1) { $("td", row).css("color", "#909090"); $("td:eq(0)", row).children().first().prop("disabled", true); }
            },
        });
    }

    /* Filter Results By Participant Attributes */
    function filterUsersDialog() {
        cancelled = false;
        dlg_filter_users = $("#divUserAttributes").dialog({
              autoOpen: true,
              width: 750,
              height: "auto",
              maxHeight: mh,
              modal: true,
              dialogClass: "no-close",
              closeOnEscape: true,
              bgiframe: true,
              title: "<%=JS_SafeString("Filter by participant attributes and create dynamic groups") %>",
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons: {
                  Ok: {
                      id: 'jDialog_btnOK', text: "Create Dynamic Group", click: function () {
                          onCreateDynamicResGroup();                          
                      }
                  },
                  Cancel: function () {
                      cancelled = true;
                      dlg_filter_users.dialog("close");
                  }
              },
              open: function() {
                  document.body.style.cursor = "default";
                  $("#divUsersFilters").html("");                  
                  initUsersFiltersGrid();
                  initUsersFilteredTable();
                  on_hit_enter = ";";
                  $("body").css("overflow", "hidden");
              },
              close: function() {                
                  $("body").css("overflow", "auto");
                  on_hit_enter = "";
                  //$("#divUserAttributes").dialog("destroy");
                  if (!cancelled) {
                      //sendCommand("action=&lst=" + encodeURIComponent(x));
                  }
                  //dlg_filter_users = null;
              },
              resize: function( event, ui ) { }
        });
    }

    function onUsersFiltersAddFilerRow(d, i) {
        var tbl = document.getElementById("tblUsersFilters");
        if ((tbl)) {
            if (i>flt_max_id) flt_max_id = i;

            var td = document.getElementById("uflt_tr_" + i);
            if (!(td))
            {
                var r = tbl.rows.length;
                td = tbl.insertRow(r);
                td.id = "uflt_tr_" + i;
                td.style.verticalAlign = "middle";
                td.style.textAlign  = "left";
                td.className = "text";
                td.insertCell(0);
                td.insertCell(1);
                td.insertCell(2);
            }

            var attr = null;

            var sAttribsValues = [];
            for (var j = 0; j < attr_data.length; j++) {
                var act = (d[fidx_id] == attr_data[j][aidx_id]);
                sAttribsValues.push({"ID" : attr_data[j][aidx_id], "text" : htmlEscape(attr_data[j][aidx_name])});
                if (act) attr = attr_data[j];
            }
            // add "inconsistency" item
            if (d[fidx_id] == DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID) {
                attr = [DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID, htmlEscape("<%=ResString("optDynamicGroupInconsistencyAttribute")%>"), avtDouble, [],[]];
            }
            sAttribsValues.push({"ID" : DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID, "text" : htmlEscape("<%=ResString("optDynamicGroupInconsistencyAttribute")%>")});
            
            sAttribs = "<div id='uflt_attr_" + i + "' style='display: inline-block;'></div>";

            sOpers = "";
            var sOpersValues = [];
            var sVal = "";

            if ((attr)) {
                var o = oper_available[attr[aidx_type]];
                if (o) {
                    for (j = 0; j < o.length; j++) {
                        sOpersValues.push({"ID" : o[j], "text" : oper_list[o[j]]});
                    }
                }
                sOpers = "<div id='uflt_oper_" + i + "' style='display: inline-block;'></div>";

                if (attr[aidx_type] != avtBoolean) {
                    sVal = "<div id='uflt_val_" + i + "' style='display: inline-block;'></div>";
                }
            }

            td.cells[0].innerHTML = "<a href='' onclick='onUsersFiltersDeleteRule(" + i + "); return false;'><i class='fas fa-times' style='color: LightCoral;'></i></a>";
            td.cells[1].innerHTML = "<nobr>" + sAttribs + "&nbsp;" + sOpers + "</nobr>";
            td.cells[2].innerHTML = sVal;

            //init attributes dropdown
            $("#uflt_attr_" + i).dxSelectBox({
                dataSource: sAttribsValues,
                displayExpr: "text",
                valueExpr: "ID",
                width: "200px",
                searchEnabled: false,
                value: d[fidx_id],
                elementAttr: {"data-idx": i},
                onValueChanged: function (e) {                    
                    onUsersFiltersChangeFilterAttr(e.element.attr("data-idx"), e.value);
                }
            });

            //init opers dropdown
            $("#uflt_oper_" + i).dxSelectBox({
                dataSource: sOpersValues,
                displayExpr: "text",
                valueExpr: "ID",
                width: "200px",
                searchEnabled: false,
                value: d[fidx_oper],
                elementAttr: {"data-idx": i}
            });

            //init vals text box
            if (attr[aidx_type] == avtString || attr[aidx_type] == avtLong || attr[aidx_type] == avtDouble) {
                $("#uflt_val_" + i).dxTextBox({
                    width: "200px",
                    value: htmlEscape(d[fidx_val]),
                    elementAttr: {"data-idx": i}
                });
            }            
        }
    }

    function onUsersFiltersChangeFilterAttr(id, val) {
        var o = document.getElementById("uflt_oper_" + id);
        var d = [1, val, o.value, ''];
        onUsersFiltersAddFilerRow(d, id);
    }

    function onUsersFiltersAddNewRule() {
        var d = [1, attr_data[0][aidx_id], 1, ''];
        onUsersFiltersAddFilerRow(d, flt_max_id+1);
    }

    function onUsersFiltersDeleteRule(id) {
        UsersFiltersDeleteRule(id);
    }

    function UsersFiltersDeleteRule(i) {
        if ($("#uflt_attr_" + i).data("dxSelectBox")) $("#uflt_attr_" + i).dxSelectBox("instance").dispose();
        if ($("#uflt_oper_" + i).data("dxSelectBox")) $("#uflt_oper_" + i).dxSelectBox("instance").dispose();      
        if (($("#uflt_val_" + i).data("dxTextBox")))  $("#uflt_val_" + i).dxTextBox("instance").dispose();
        
        var td = document.getElementById("uflt_tr_" + i);
        if ((td)) td.parentNode.removeChild(td);
    }

    function initUsersFilteredTable() {
        sendCommand("action=get_filtered_users_data&filter=" + encodeURIComponent(getUsersFiltersString()) + "&combination=" + $("#cbUsersFiltersCombination").dxSelectBox("instance").option("value"));
    }

    var table_filter_users = null;
    var filtered_users_columns = [];
    var filtered_users_data = [];

    function drawUsersFilteredTable() {        
        table_filter_users = (($("#tableFilteredUsers").data("dxDataGrid"))) ? $("#tableFilteredUsers").dxDataGrid("instance") : null;
        if (table_filter_users !== null) { 
            table_filter_users.dispose();
        }
        var columns = [];

        //init columns headers                
        for (var i = 0; i < filtered_users_columns.length; i++) {            
            columns.push({ "caption" : filtered_users_columns[i][0], "alignment" : filtered_users_columns[i][1], "allowSorting" : filtered_users_columns[i][2], "allowSearch" : filtered_users_columns[i][3], "dataField" : filtered_users_columns[i][4] });
        }

        table_filter_users = $("#tableFilteredUsers").dxDataGrid({
            dataSource: filtered_users_data,
            columns: columns,
            searchPanel: {
                visible: true,
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            stateStoring: {
                enabled: true,
                type: "sessionStorage",
                storageKey: "FltUsrsDataGrid"
            },
            noDataText: "<% =GetEmptyMessage()%>, press \"Preview\" button"
        });

        $("#tableFilteredUsers").find(".dx-datagrid-content").css("min-height", "100px");
    }

    function initUsersFiltersGrid() {
        var d = document.getElementById("divUsersFilters");
        if ((d)) d.innerHTML = "<table id='tblUsersFilters' border=0 cellspacing=4 cellpadding=0></table>";
        if ((attr_user_flt) && (attr_data)) {
            for (var i = 0; i < attr_user_flt.length; i++) {
                var d = attr_user_flt[i];
                onUsersFiltersAddFilerRow(d, i);
            }
        }

        $("#cbUsersFiltersCombination").dxSelectBox({
            dataSource: [ {ID: 0, text: "AND"}, {ID: 1, text: "OR"} ],
            displayExpr: "text",
            valueExpr: "ID",
            width: "90px",
            searchEnabled: false,
            value: <%=CInt(FilterCombination)%>
        });
    }

    function onUsersFiltersApplyFilter(do_reset) {
        var sData = (do_reset ? " " : getUsersFiltersString());
        initUsersFilteredTable();
    }

    function onUsersFiltersReset() {
        dxConfirm(resString("msgResetFilter"), "DoUsersFiltersReset();");
    }

    function DoUsersFiltersReset() {
        for (var i = 0; i <= flt_max_id; i++) {
            onUsersFiltersDeleteRule(i);
        }
        flt_max_id = 0;
        onUsersFiltersApplyFilter(true);
    }

    function onCreateDynamicResGroup() {
        var sDlgGroupName = "<span style='margin-left:15px;'><% = JS_SafeString(ResString("lblGroupName")) %>:&nbsp;</span><input id='tbDynResGroupName' type='text' class='text' style='text-align:left; width: 220px;' value='' >";
        var groupNameDialog = DevExpress.ui.dialog.custom({
            title: "<% = JS_SafeString(ResString("titleCreateGroup")) %>",
            message: sDlgGroupName,
            buttons: [{ text: "<% =JS_SafeString(ResString("btnOK"))%>", onClick : function () { return true; }}, {text: "<% =JS_SafeString(ResString("btnCancel")) %>", onClick : function () { return false; }}]
        });
        groupNameDialog.show().done(function(dialogResult){
            if (dialogResult) onSaveDynResGroup($('#tbDynResGroupName').val());
        });
        setTimeout(function () { $("#tbDynResGroupName").focus(); }, 500);
    }

    function onSaveDynResGroup(value) {        
        if (typeof value != 'undefined' && value.trim() != "") {
            value = value.trim();
            sendCommand("action=create_dyn_res_group&name=" + value + "&filter=" + encodeURIComponent(getUsersFiltersString()) + "&combination=" + $("#cbUsersFiltersCombination").dxSelectBox("instance").option("value"));
            dlg_filter_users.dialog("close");
        }
    }

    function getUsersFiltersString() {
        attr_user_flt = [];
        var sData = "";
        for (var i = 0; i <= flt_max_id; i++) {
            var a = $("#uflt_attr_" + i).dxSelectBox("instance");            
            var o = $("#uflt_oper_" + i).dxSelectBox("instance");
            var el_v = $("#uflt_val_" + i);
            var v;
            if (el_v.data("dxTextBox")) v = el_v.dxTextBox("instance");
            if (el_v.data("dxSelectBox")) v = el_v.dxSelectBox("instance");
            
            var selValue = ""            

            if (!(v)) {
                selValue = "";
                
                //approach 2: checkbox list
                var ul = document.getElementById("uflt_ul_" + i);                
                if (ul) {
                    var items = ul.getElementsByTagName("input");
                    for (var k=0; k < items.length; k++)
                    {                    
                        if (items[k].checked) 
                        {
                            if (selValue.length > 0) selValue += ";"
                            selValue += items[k].value;
                        }
                    }
                }
            } else { selValue = v.option("value"); }
            
            if ((a) && (o))
            {
                attr_user_flt.push([1, a.option("value"), o.option("value"), selValue]);
                sData += (sData == "" ? "" : "\n") + 1 + "<% =Flt_Separator %>" + a.option("value")  + "<% =Flt_Separator %>" + o.option("value")  + "<% =Flt_Separator %>" + selValue;
            }
        }
        return sData;
    }
    /* end Filter Results By Participant Attributes */

    /* Filter Alternatives - Advanced */
    function filterAltsAdvanced() {
        if ((dlg_alts_advanced)) dlg_alts_advanced = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_alts_advanced = $("#divAltsAdvanced").dialog({
            autoOpen: false,
            width: 600,
            height: 200,
            minHeight: 150,
            maxHeight: 850,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "Advanced",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", click: function() { dialog_result = true; dlg_alts_advanced.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_alts_advanced.dialog( "close" ); }}],
            open:  function() { 
                $("body").css("overflow", "hidden");
                document.body.style.cursor = "default";
            },
            close: onAltsAdvancedDialogClose
        });
        dlg_alts_advanced.dialog('open');
    }

    function expandGroupUsers(grp_id) {
        var s = "";
        var g = getUserByID(grp_id);
        if ((g)) {
            for (var i = 0; i < g[IDX_GROUP_PARTICIPANTS].length; i++) {
                var u = getUserByID(g[IDX_GROUP_PARTICIPANTS][i]);
                if ((u)) {
                    var hd = u[IDX_USER_HAS_DATA];
                    s += (s == "" ? "" : "<br>") + "<nobr><small " + (hd != 1 ? "style='color:#909090;'" : "") +">&nbsp;&#8226;&nbsp;" + htmlEscape(u[IDX_USER_NAME]) + "</small></nobr>";
                }
            }
            $("#divExpandedUsers" + grp_id).html(s);
            var img = document.getElementById("imgGrp" + g[IDX_USER_ID]);
            if ((img)) {
                img.src = "<%=ImagePath%>old/minus.gif";
                img.onclick = function () { collapseGroupUsers(g[IDX_USER_ID]); };
            }
        }
    }

    function collapseGroupUsers(grp_id) {
        $("#divExpandedUsers" + grp_id).html("");
        var img = document.getElementById("imgGrp" + grp_id);
        if ((img)) {
            img.src = "<%=ImagePath%>old/plus.gif";
            img.onclick = function () { expandGroupUsers(grp_id); };
        }
    }

    function onAltsAdvancedDialogClose() {
        $("body").css("overflow", "auto");
        if (dialog_result) {
            sendCommand("action="+ACTION_ALTS_FILTER+"&value=-2&alts_num="+$("#cbAdvAltsNum").val()+"&user_id="+$("#cbAdvUsers").val(), true);
            setOldVal();
        } else {
            resetOldVal();
        }
        updateToolbar();
    }

    // Filter Alternatives - Custom
    function filterAltsCustom() {
        if ((dlg_alts_custom)) dlg_alts_custom = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_alts_custom = $("#divAltsCustom").dialog({
            autoOpen: false,
            width: 760,
            height: 435,
            minHeight: 150,
            maxHeight: 550,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "Select/Deselect <%=ParseString("%%Alternatives%%")%>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", click: function() { dialog_result = true; dlg_alts_custom.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_alts_custom.dialog( "close" ); }}],
            open:  function() { 
                $("body").css("overflow", "hidden");
                document.body.style.cursor = "default";
            },
            close: onAltsCustomDialogClose
        });
        dlg_alts_custom.dialog('open');
        $(".ui-dialog-buttonset").prepend($("#pnlAllOrNoneSelector"));
    }

    function onAltsCustomDialogClose() {
        $("body").css("overflow", "auto");
        $("#divAltsCustom").prepend($("#pnlAllOrNoneSelector"));
        if (dialog_result) {
            var params = "";
            var cb_arr = $("input:checkbox.cust_alt_cb");
            $.each(cb_arr, function(index, val) { var aid = val.value*1; if (val.checked) { params += (params == "" ? "" : ",") + aid; } });
            sendCommand("action="+ACTION_ALTS_FILTER + "&value=-3&ids=" + params, true);
            setOldVal();
        } else {
            resetOldVal();
        }
        updateToolbar();
    }

    function filterAltsCustomSelect(chk) {
        $("input:checkbox.cust_alt_cb").prop('checked', chk*1 == 1);
    }

    // Filter by alternative attributes
    function filterAltsAttributes() {
        if ((dlg_alts_attributes)) dlg_alts_attributes = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_alts_attributes = $("#divAltsAttributes").dialog({
            autoOpen: false,
            width: 560,
            height: 235,
            minHeight: 150,
            maxHeight: 550,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "Filter by <%=ParseString("%%alternative%%")%> attributes",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", id: "btnApplyFilter", click: function() { dialog_result = true; dlg_alts_attributes.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_alts_attributes.dialog( "close" ); }}],
            open:  function() { 
                $("body").css("overflow", "hidden");
                document.body.style.cursor = "default";
                InitFilterGrid();
            },
            close: onAltsAttributesDialogClose
        });
        dlg_alts_attributes.dialog('open');
    }

    function onAltsAttributesDialogClose() {
        $("body").css("overflow", "auto");
        if (dialog_result) {
            ApplyFilter(false);            
            setOldVal();
        } else {
            resetOldVal();
        }
        updateToolbar();
    }

    function AddFilerRow(d, i) {
        var tbl = document.getElementById("tblFilters");
        if ((tbl))
        {
            if (i>flt_max_id) flt_max_id = i;

            var td = document.getElementById("flt_tr_" + i);
            if (!(td))
            {
                var r = tbl.rows.length;
                td = tbl.insertRow(r);
                td.id = "flt_tr_" + i;
                td.style.verticalAlign = "top";
                td.style.textAlign  = "left";
                td.className = "text";
                td.insertCell(0);
                td.insertCell(1);
                td.insertCell(2);
            }

            var attr = null;
                
            var sChk = "<input type='checkbox' id='flt_chk_" + i + "' value='1' onclick='IsAnyFilterChecked(); '" + (d[fidx_chk]=="1" ? " checked" : "") + ">";
            var sAttribs = "";            
            for (var j=0; j<attr_list.length; j++)
            {
                var act = (d[fidx_id] == attr_list[j][aidx_id]);
                sAttribs += "<option value='" + attr_list[j][aidx_id] + "'" + (act ? " selected" : "") + ">" + replaceString("'", "&#39;", attr_list[j][aidx_name]) + "</option>";
                if (act) attr = attr_list[j];
            }
            sAttribs = "<select id='flt_attr_" + i + "' style='width:24ex' onChange='ChangeFilterAttr(" + i + ", this.value);'>" + sAttribs + "</select>";

            sOpers = "";
            var sVal = "";

            if ((attr))
            {
                var o = oper_available[attr[aidx_type]];
                if (o) {
                    for (j=0; j<o.length; j++)
                    {
                        sOpers += "<option value='" + o[j] + "'" + (d[fidx_oper] == o[j] ? " selected" : "") + ">" + oper_list[o[j]] + "</option>";
                    }
                }
                sOpers = "<select id='flt_oper_" + i + "' style='width:15ex'>" + sOpers + "</select>";

                if ((attr[aidx_type] == avtEnumeration) || (attr[aidx_type] == avtEnumerationMulti)) //approach 1 - select multiple
                {
                    var v = attr[aidx_vals];
                    var vals = [d[fidx_val]];
                    if (attr[aidx_type] == avtEnumerationMulti) vals = d[fidx_val].split(";");                    
                    
                    for (j=0; j<v.length; j++)
                    {
                        var is_selected = false;
                        for (k=0; k<vals.length; k++)
                        {
                            if (vals[k] == v[j][0]) is_selected = true;
                        }
                        sVal += "<option value='" + v[j][0] + "'" + (is_selected ? " selected" : "") + ">" + replaceString("'", "&#39;", v[j][1]) + "</option>";
                    }
                    var multi = "";
                    if (attr[aidx_type] == avtEnumerationMulti) multi = "multiple='multiple'";
                    sVal = "<select " + multi + " id='flt_val_" + i + "' style='width:24ex; margin-top:3px;'>" + sVal + "</select>";

                    //approach 2 - check list box
                    if (attr[aidx_type] == avtEnumerationMulti)
                    {
                        sVal="";
                        for (j=0; j<v.length; j++)
                        {
                            var is_selected = false;
                            for (k=0; k<vals.length; k++)
                            {
                                if (vals[k] == v[j][0]) is_selected = true;                            
                            }
                            sVal += "<li style='margin:0; padding:0;' title='" + replaceString("'", "&#39;", v[j][1]) +"'><label for='chk" + j + "_" + k + "'><input type='checkbox' id='chk" + j + "_" + k + "' value='" + v[j][0] + "'" + (is_selected ? " checked" : "") + ">" + replaceString("'", "&#39;", v[j][1]) + "</label></li>";
                        }
                        sVal = "<ul id='flt_ul_" + i + "' style='height:12ex; overflow-x: hidden; overflow-y:auto; border:1px solid #999999; text-align:left; margin:0px; padding:0px;'>" + sVal + "</ul>";
                    }
                }
                else
                {
                    if (attr[aidx_type] != avtBoolean) {
                        sVal = "<input type='text' style='width:14ex; margin-top:2px;' id='flt_val_" + i + "' value='" + replaceString("'", "&#39;", d[fidx_val]) + "'>";
                    }
                }
            }

            td.cells[0].innerHTML = "<nobr>" + sChk + sAttribs + "&nbsp;" + sOpers + "</nobr>";
            td.cells[1].innerHTML = sVal;
            td.cells[2].innerHTML = "<input type=button class='button' style='width:22px;height:22px;' id='flt_del_" + i + "' value='X' onclick='onDeleteRule(" + i + "); IsAnyFilterChecked(); return false;'>";
        }
    }

    function ChangeFilterAttr(id, val) {
        var c = document.getElementById("flt_chk_" + id);
        var o = document.getElementById("flt_oper_" + id);
        var d = [((c) && c.checked ? 1 : 0), val, o.value, ''];
        AddFilerRow(d, id);
    }

    function onAddNewRule() {
        var d = [1, attr_list[0][aidx_id], 1, ''];
        AddFilerRow(d, flt_max_id+1);
    }

    function onDeleteRule(id) {
        dxConfirm(resString("msgAreYouSure"), "DeleteRule(" + id + ");");
    }

    function DeleteRule(id) {
        var td = document.getElementById("flt_tr_" + id);
        if ((td)) td.parentNode.removeChild(td);
    }

    function InitFilterGrid() {
        var d = document.getElementById("divFilters");
        if ((d)) d.innerHTML = "<table id='tblFilters' border='0' cellspacing='4' cellpadding='0'></table><div id='msgCheck' style='display: none; color: red;'>To activate a filter, check the box next to it</div>";
        if ((attr_flt) && (attr_list)) {
            for (var i=0; i<attr_flt.length; i++) {
                var d = attr_flt[i];
                AddFilerRow(d, i);
            }
        }
        IsAnyFilterChecked();
    }

    function ApplyFilter(do_reset) {
        var sData = (do_reset ? " " : getFilterString());
        sendCommand("action="+ACTION_ALTS_FILTER + "&value=-5&filter=" + encodeURIComponent(sData) + "&combination=" + $("#cbFilterCombination").val(), true);
    }

    function onFilterReset() {
        dxConfirm(resString("msgResetFilter"), "DoFilterReset();");
    }

    function DoFilterReset() {
        for (var i=0; i<=flt_max_id; i++) {
            var c = document.getElementById("flt_chk_" + i);
            if ((c)) c.checked = 0;
        }
        ApplyFilter(true);
    }

    function IsAnyFilterChecked() {
        var retVal = false;
        for (var i=0; i<=flt_max_id; i++) {            
            var c = document.getElementById("flt_chk_" + i);
            if ((c) && (c.checked)) {
                retVal = true;
            }
        }    
        $("#msgCheck").toggle(!retVal);        
        var btn = document.getElementById("btnApplyFilter");    
        if ((btn)) { btn.disabled = !retVal; }   
    }

    function getFilterString() {
        attr_flt = [];
        var sData = "";
        for (var i=0; i<=flt_max_id; i++) {
            var c = document.getElementById("flt_chk_" + i);
            var a = document.getElementById("flt_attr_" + i);
            var o = document.getElementById("flt_oper_" + i);
            var v = document.getElementById("flt_val_" + i);
            
            var selValue = ""            

            if (!(v)) {
                selValue = "";
                
                //approach 2: checkbox list
                var ul = document.getElementById("flt_ul_" + i);                
                if (ul) {
                    var items = ul.getElementsByTagName("input");
                    for (var k=0; k < items.length; k++)
                    {                    
                        if (items[k].checked) 
                        {
                            if (selValue.length > 0) selValue += ";"
                            selValue += items[k].value;
                        }
                    }
                }
            } else { selValue = v.value; }
            
            if ((c) && (a) && (o))
            {
                attr_flt.push([c.checked ? 1 : 0, a.value, o.value, selValue]);
                sData += (sData == "" ? "" : "\n") + (c.checked ? 1 : 0) + "<% =Flt_Separator %>" + a.value  + "<% =Flt_Separator %>" + o.value  + "<% =Flt_Separator %>" + selValue;
            }
        }
        return sData;
    }
    // End filtering by alternative attributes

    // 4SA looping by portions of participants/groups
    function hbEnable(hb_id, is_enabled) {
        if ($("#" + hb_id).data("dxButton")) {
            var btn = $("#" + hb_id).dxButton("instance");
            btn.option("disabled", !is_enabled);
        }
    }

    function updatePortionsUI() {      
        var canSwitchPortions = (total_users_and_groups_count > 4) && (keep_users_count() < 4) && ((total_users_and_groups_count - keep_users_count()) > 1);
        hbEnable("btnUsersPrev", canSwitchPortions);
        hbEnable("btnUsersNext", canSwitchPortions);
    }

    function onNextClick(cb, direction) {
        if (!$(cb).hasClass("disabled")) {
            var uid0 = $("#cb4ASAUsers0").val();
            var uid1 = $("#cb4ASAUsers1").val();
            var uid2 = $("#cb4ASAUsers2").val();
            var uid3 = $("#cb4ASAUsers3").val();
            sendCommand("action=" + ACTION_4ASA_NEXT_PORTION + "&dir=" + direction + "&selected=" + uid0 + "," + uid1 + "," + uid2 + "," + uid3, true);
        }
    }
    // end 4SA looping by portions of participants/groups

    // Select Events Dialog
    <%--function onSelectEventsClick() {
        if (alts_data.length > 0) {
            initSelectEventsForm("Select <%=ParseString("%%Alternatives%%")%>");
            dlg_select_events.dialog("open");
            $("#jDialog_btnOK").button("disable");
        }
    }--%>

    //function initSelectEventsForm(_title) {
    //    cancelled = false;

    //    var labels = "";

    //    // generate list of events
    //    var alts_data_len = alts_data.length;
    //    for (var k = 0; k < alts_data_len; k++) {
    //        var checked = alts_data[k][IDX_ALT_ENABLED] == 1;
    //        labels += "<div><label><input type='checkbox' class='select_event_cb' value='" + alts_data[k][IDX_EVENT_ID] + "' " + (checked ? " checked='checked' " : " ") + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + htmlEscape(alts_data[k][IDX_EVENT_NAME]) + "</label></div>";
    //    }

    //    $("#divSelectEvents").html(labels);

    //    dlg_select_events = $("#selectEventsForm").dialog({
    //        autoOpen: false,
    //        modal: true,
    //        width: 420,
    //        minWidth: 530,
    //        maxWidth: 950,
    //        minHeight: 250,
    //        maxHeight: mh,
    //        dialogClass: "no-close",
    //        closeOnEscape: true,
    //        bgiframe: true,
    //        title: _title,
    //        position: { my: "center", at: "center", of: $("body"), within: $("body") },
    //        buttons: {
    //            Ok: {
    //                id: 'jDialog_btnOK', text: "OK", click: function () {
    //                    dlg_select_events.dialog("close");
    //                }
    //            },
    //            Cancel: function () {
    //                cancelled = true;
    //                dlg_select_events.dialog("close");
    //            }
    //        },
    //        open: function () {
    //            $("body").css("overflow", "hidden");
    //        },
    //        close: function () {
    //            $("body").css("overflow", "auto");
    //            if (!cancelled) {
    //                sEventIDs = "";
    //                var cb_arr = $("input:checkbox.select_event_cb");
    //                var chk_count = 0;
    //                $.each(cb_arr, function (index, val) { 
    //                    var cid = val.value + ""; 
    //                    if (val.checked) { 
    //                        sEventIDs += (sEventIDs == "" ? "" : ",") + cid; 
    //                        chk_count += 1;
    //                    } 
    //                    alts_data[index][IDX_ALT_ENABLED] = (val.checked ? 1 : 0);
    //                });
    //                sendCommand('action=select_events&event_ids=' + sEventIDs); // save the selected events via ajax
    //            }
    //        }
    //    });
    //    $(".ui-dialog").css("z-index", 9999);
    //}

    //function filterSelectAllEvents(chk) {
    //    $("input:checkbox.select_event_cb").prop('checked', chk * 1 == 1);
    //    $("#jDialog_btnOK").button("enable");
    //}
    // end Select Events Dialog

    function switchSimResults(value) {        
        sendCommand("action=show_sim_results&value=" + value);
    }

    function onRefreshCVClick() {
        sendCommand("action=" + ACTION_REFRESH_CV, true);
    }

    //function cbObjectivesSwitchClick(vis) {
    //    showHierarchyTree = vis;
    //    sendCommand("action=objectives_visibility&vis=" + vis, true);
    //}

    <%--function cbObjectivesPrioritiesSwitchClick(chk) {
        showTreePriorities = chk && combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>;
        localStorage.setItem("SynthesisShowTreePriorities<%=App.ProjectID%>", chk.toString());
        initCVTree("cvTree-1");
    }--%>

    function cbLegendsSwitchClick(vis) {
        showLegend = vis;
        //chartLegendsToggle();
        sendCommand("action=legends_visibility&vis=" + vis, false);
    }

    function switchShowPriorities(value) {
        showLikelihoodsGivenSources = value == 1;
        sendCommand("action=switch_show_priorities&value=" + (value == 1), true);
    }

    function initSplitters() {
        if (showHierarchyTree) {
            $(".split_left").resizable({
                autoHide: false,
                handles: 'e',
                maxWidth: 600,
                resize: function(e, ui) {
                    //$("#divContent").hide();
                    var parent = ui.element.parent();
                    var divTwo = ui.element.next();
                    divTwo.width(10);
                    var remainingSpace = parent.width() - ui.element.outerWidth();
                    var divTwoWidth = remainingSpace - (divTwo.outerWidth() - divTwo.width());
                    if (divTwoWidth > 20) divTwo.width(divTwoWidth);
                },
                stop: function(e, ui) {
                    treeSplitterSize = ui.element.outerWidth();
                    sessionStorage.setItem("SynthesisSplitterSize<%=App.ProjectID%>", treeSplitterSize);
                    //sendCommand("action=v_splitter_size&value=" + treeSplitterSize, false);
                    //$("#divContent").show();

                    //resizePage();
                }
            });
            $(".ui-resizable-e").addClass("splitter_v");
        }
    }

    $(document).ready(function () { 
        <%--if (combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>) { $(".ObjectivesPrioritiesSwitch").show(); } else { $(".ObjectivesPrioritiesSwitch").hide(); } --%>
        initDrawerToolbar();
        setTimeout(function () { 
            initPage(); 
            initSplitters();                        
        }, 50);      
        $("#divTree").width(treeSplitterSize);
        //setTimeout(function () {resizePage();}, 600);
        //$(".sa_toolbar").css("height", "100px");
        //$(".setting_toolbar").css("height", "100px");
    });

    function initDrawerToolbar() {
        var drawer = $("#drawer").dxDrawer({
            animationEnabled: false,
            revealMode: "expand",
            openedStateMode: "shrink",
            opened: getSettingsToolbarOpened() && isAdvancedMode,
            onOptionChanged: function(e) {
                if (e.name == "opened") {
                    $(".dx-drawer-wrapper").css("overflow", e.value ? "visible" : "");
                    $(".dx-drawer-panel-content").css("overflow", e.value ? "visible" : "");
                    setSettingsToolbarOpened(e.value);
                    setTimeout("resizePage();", 200);
                }
            },
            position: "top",
            template: function() {
                var toolbarContent = $("#main_toolbar");
                if (typeof $(".sa_toolbar").html() == "undefined" || $(".sa_toolbar").html().trim() == "") $(".sa_toolbar").hide().css("height", 0);
                return toolbarContent;
            }
        }).dxDrawer("instance");

        $("#toolbar").dxToolbar({
            items: [{
                widget: "dxButton",
                location: "before",
                options: {
                    icon: "fas fa-sliders-h",
                    hint: "Toolbar",
                    elementAttr: { id: 'btnToggleToolbar' },
                    onClick: function() {
                        drawer.toggle();
                        resizePage();
                    }
                }
            }, {  
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButtonGroup',
                visible: true,
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: [<%=If(PM.Parameters.Synthesis_ObjectivesVisibility, "1,", "")%><%=If(PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 2 OrElse PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4, "2,", "")%><%=If(PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 3 OrElse PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4, "3", "")%>],
                    displayExpr: "text",
                    disabled: false,
                    onItemClick: function (e) { 
                        var sel = e.component.option("selectedItemKeys");

                        if (e.itemData.ID == 1 && sel.indexOf(1) > -1) showHierarchyTree = true;
                        if (e.itemData.ID == 1 && sel.indexOf(1) == -1) showHierarchyTree = false;
                        if (e.itemData.ID == 2 && sel.indexOf(2) > -1) showTreePrioritiesL =  combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>;
                        if (e.itemData.ID == 2 && sel.indexOf(2) == -1) showTreePrioritiesL = false;
                        if (e.itemData.ID == 3 && sel.indexOf(3) > -1) showTreePrioritiesG =  combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>;
                        if (e.itemData.ID == 3 && sel.indexOf(3) == -1) showTreePrioritiesG = false;
                        
                        if ((e.itemData.ID == 2 || e.itemData.ID == 3) && sel.indexOf(1) == -1) {                            
                            sel.push(1);
                            showHierarchyTree = true;
                            e.component.option("selectedItemKeys", sel);
                        }

                        $("#divTree").css("display", showHierarchyTree ? "" : "none");

                        initCVTree("cvTree-1");
                        resizePage();
                        e.itemElement.blur();

                        callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=objectives_visibility", { "hierarchy" : showHierarchyTree, "local_priorities" :  showTreePrioritiesL, "global_priorities" : showTreePrioritiesG }, function () {
                        }, true);
                    },
                    selectionMode: "multiple",
                    items: [{"ID" : 1, "text" : "", "hint" : "<%=ParseString("Hierarchy")%>", "icon" : "fas fa-sitemap" },
                            {"ID" : 2, "text" : "<% = ResString("optLocal")  %>", "hint" : "<%=ParseString("Local Priorities")%>", disabled : false, visible: combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%> },
                            {"ID" : 3, "text" : "<% = ResString("optGlobal") %>", "hint" : "<%=ParseString("Global Priorities")%>", disabled : false, visible: combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%> }]
                }
            }, <% If PM.IsRiskProject AndAlso { _PGID_ANALYSIS_OVERALL_ALTS, _PGID_ANALYSIS_OVERALL_OBJS }.Contains(CurrentPageID) Then %> 
            { location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: true,
                disabled: false,
                options: {
                    showText: true,
                    text: "Simulated Results",
                    value: <% = Bool2JS(PM.Parameters.Riskion_Use_Simulated_Values <> 0) %>,
                    elementAttr: {id: 'cbSimulatedSwitch'},
                    hint: "",
                    onValueChanged: function (e) { 
                        sendCommand("action=show_sim_results&value=" + e.value, true);
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: false,
                options: {
                    beginGroup: true,
                    icon: "fas fa-cog", text: "Preferences", hint: "Preferences",
                    template: function() {
                        return $('<i class="dx-icon fas fa-cog"></i><span class="dx-button-text nowide1200">Preferences</span>');
                    },
                    disabled: false,
                    elementAttr: {id: 'btn_settings'},
                    onClick: function (e) {
                        //settingsDialog();
                    }
                }
            }, <% End If %>
            <%--<%If SynthesisView = SynthesisViews.vtDashboard OrElse SynthesisView = SynthesisViews.vtMixed Then%>{
                widget: "dxCheckBox",
                location: "before",
                options: {
                    text: "View Mode",
                    showText: true,
                    value: dashboardSimpleViewMode,
                    elementAttr: { id: 'cbViewMode' },
                    onValueChanged: function (e) {
                        if (dashboardSimpleViewMode != e.value) {
                            dashboardSimpleViewMode = e.value;
                            sendCommand("action=dashboard_simple_mode&value=" + dashboardSimpleViewMode, false);
                        }
                    }
                }
            },<%End If%>--%><%If IsMixedOrSensitivityView() OrElse SynthesisView = SynthesisViews.vtASA OrElse SynthesisView = SynthesisViews.vt4ASA Then%>{
                widget: "dxButton",
                location: "before",
                options: {
                    icon: "fas fa-undo",
                    text: "Reset",
                    hint: "Reset",
                    onClick: function() {
                        resetSA();
                    }
                }
            },<%End If%><% If SynthesisView = SynthesisViews.vt4ASA Then%>{
                widget: "dxButton",
                location: "center",
                locateInMenu: 'never',
                options: {
                    icon: "fas fa-chevron-left",
                    hint: "",
                    elementAttr: { id: 'btnUsersPrev' },
                    onClick: function() {
                        onNextClick(this, -1);
                    }
                }
            }, {   location: 'center', locateInMenu: 'never',  template: '<i class="fas fa-th-large fa-2x toolbar-label"></i>' },
            {
                widget: "dxButton",
                location: "center",
                locateInMenu: 'never',
                options: {
                    icon: "fas fa-chevron-right",
                    hint: "",
                    elementAttr: { id: 'btnUsersNext' },
                    onClick: function() {
                        onNextClick(this, 1);
                    }
                }
            },
            <%End If%>
            <%If IsMixedOrSensitivityView() Then%>
            {
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButton',
                options: {
                    icon: "fas fa-file-export",
                    text: "<% = ResString("btnExport")%>",
                    hint: "<% = ResString("btnExport")%>",
                    elementAttr: { id: 'btnExportChart'},                            
                    onClick: function (e) {
                        exportSA();
                    }
                }
            },
            <% End If %>
            <%--<%If IsSensitivityView() Then%>
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                visible: true,
                options: {
                    searchEnabled: false,
                    valueExpr: "ID",
                    value: <% = CInt(PM.Parameters.RiskionShowEventsType) %>,
                    disabled: false,
                    displayExpr: "Text",
                    elementAttr: {id: 'btn_switch_show_events_type'},
                    onValueChanged: function (e) { 
                        selected_events_type = e.value * 1;
                        callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=selected_events_type", { "value" : e.value }, function () {
                            if (SynthesisView == vt4ASA) {
                                $('#DSACanvas0').sa("option", "selected_events_type", selected_events_type);
                                $('#DSACanvas1').sa("option", "selected_events_type", selected_events_type);
                                $('#DSACanvas2').sa("option", "selected_events_type", selected_events_type);
                                $('#DSACanvas3').sa("option", "selected_events_type", selected_events_type);
                            } else {
                                $('canvas.DSACanvas').sa("option", "selected_events_type", selected_events_type); 
                            }          
                            if (SynthesisView == vt4ASA || SynthesisView == vtASA) {
                                $('canvas.DSACanvas').sa("refreshASA");
                            } else {
                                $('canvas.DSACanvas').sa("resetSA");
                            };
                        }, true);
                    },
                    items: [{"ID": <%=CInt(DashboardWebAPI.RiskionShowEventsType.etRisks)%>, "Text": "<% = ParseString("Show %%risks%% only")%>"}, {"ID": <%=CInt(DashboardWebAPI.RiskionShowEventsType.etGains)%>, "Text": "<% = ParseString("Show opportunities only")%>"}, {"ID": <%=CInt(DashboardWebAPI.RiskionShowEventsType.etBoth)%>, "Text": "<% = ParseString("Show both %%risks%% and opportunities")%>"}]
                }
            }
            <% End If %>--%>
            ]
        });
        if ($("#drawer").dxDrawer('instance').option('opened')) {
            $(".dx-drawer-wrapper").css("overflow", "visible");
            $(".dx-drawer-panel-content").css("overflow", "visible");
        }
    }

    function getSettingsToolbarOpened() {
        var sValue = localStorage.getItem("SettingsToolbarOpened<%=App.ProjectID%>");
        return !(typeof sValue == "undefined" || sValue == null || sValue == "") && sValue == "true";
    }

    function setSettingsToolbarOpened(value) {
        localStorage.setItem("SettingsToolbarOpened<%=App.ProjectID%>", value.toString());
    }

    resize_custom = resizePage;
    toggleScrollMain();

    onSwitchAdvancedMode = function (value) { 
        if (value) {
            $("#drawer").dxDrawer("instance").show();
        } else {
            $("#drawer").dxDrawer("instance").hide();
        }
        setTimeout(function () {
            resizePage();
            updateToolbar();
        }, 200);
    };


<% Select Case CurrentPageID
        Case _PGID_ANALYSIS_OVERALL_ALTS, _PGID_ANALYSIS_OVERALL_OBJS, _PGID_ANALYSIS_PSA, _PGID_ANALYSIS_GSA, _PGID_ANALYSIS_2D, _PGID_ANALYSIS_HEAD2HEAD, _PGID_ANALYSIS_ASA, _PGID_ANALYSIS_DSA, _PGID_ANALYSIS_OVERALL_ALTS
%>
    function getWidgetParams() {
        <% If CurrentPageID = _PGID_ANALYSIS_OVERALL_ALTS OrElse CurrentPageID = _PGID_ANALYSIS_OVERALL_OBJS Then %>
        var all = {"userIDs": columns_user_ids, 
            "wrtNodeID": WRTNodeID,
            "normalizationMode": NormalizeMode,
            "decimalDigits": DecimalDigits,
            "synthMode": synthMode,
            "combinedMode": combinedMode,
            "useCIS": document.getElementById("cbCIS").checked,
            "useUserWeights": document.getElementById("cbUserWeights").checked,
            "alternativesFilterValue": AlternativesFilterValue,
            "alternativesAdvancedFilterUserID": AlternativesAdvancedFilterUserID,
            "alternativesAdvancedFilterValue": AlternativesAdvancedFilterValue,
            "selectOnlyCoveringObjectivesOption": SelectOnlyCoveringObjectivesOption,
            "prioritiesMode" : tableObjParams["priority_mode" + 0], // 0-local, 1-global
            //"showLikelihoodsGivenSources": document.getElementById("cbSwitchShowPriorities").options[document.getElementById("cbSwitchShowPriorities").selectedIndex].value
        };
        <% Else %>
        var options = $('canvas.DSACanvas').sa("getChangedOptions");
        var params= {};
        var all = joinlists(params, options);
        <% End If %>
        return all;
    }

    function onGetDirectLink() {
        return list2params(getWidgetParams());
    };

    function onGetReportTitle() {
        var t = "";
        //var h = <% If CurrentPageID = _PGID_ANALYSIS_OVERALL_ALTS OrElse CurrentPageID = _PGID_ANALYSIS_OVERALL_OBJS Then %>$(".title-text-overflow-ellipsis-line");<% Else %>$("#pgHeader span:visible");<% End If %>
        var h = $("#spanPgTitle");
        if ((h) && (h.length>0)) {
            for (var i =0; i<h.length; i++) {
                t += (t == "" ? "" : " ") + h[i].innerText;
            }
        }
        return t;
    }

    function onGetReportLink() {
        var url = document.location.href;
        var pg = pageByID(pgID);
        if ((pg) && typeof pg.url != "undefined" && pg.url != "") url == pg.url;
        return url + "&<% =_PARAM_ACTION %>=upload&" + onGetDirectLink();
    }

    function onGetReport(addReportItem) {
        var t = _rep_item_DSA;
        if (typeof addReportItem == "function") {
            switch (pgID) {
                case <% = _PGID_ANALYSIS_PSA %>:
                    t = _rep_item_PSA;
                    break;
                case <% = _PGID_ANALYSIS_GSA %>:
                    t = _rep_item_GSA;
                    break;
                case <% = _PGID_ANALYSIS_2D %>:
                    t = _rep_item_Analysis2D;
                    break;
                case <% = _PGID_ANALYSIS_HEAD2HEAD  %>:
                    t = _rep_item_HTH;
                    break;            
                case <% = _PGID_ANALYSIS_ASA  %>:
                    t = _rep_item_ASA;
                    break;
                case <% =_PGID_ANALYSIS_OVERALL_ALTS %>:
                    t = _rep_item_AltsGrid;
                    break;
                case <% =_PGID_ANALYSIS_OVERALL_OBJS %>:
                    t = _rep_item_ObjsGrid;
                    break;
            }
            localStorage.setItem(_sess_dash_alts_flt, (AlternativesFilterValue != -1));
            addReportItem({"name": onGetReportTitle(), "type": t, "edit": document.location.href, "export": "", "ContentOptions": getWidgetParams()});
        }
    }
<% End Select %>
</script>
<table class="whole" border='0' cellspacing="0" cellpadding="0" style="height:calc(100% - 6px);">
    <tr valign="top">
        <td valign="top">
            <div id="toolbar" class="dxToolbar"></div>
        </td>
    </tr>
    <tr valign="top">
        <td id="tdDrawer" valign="top">
            <div id="drawer" style="margin-left: 4px;"></div>
            <ul id="main_toolbar" class="ec-wrap-panel">                        
                        <%If App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.ResourceAlignerEnabled, Nothing, True) Then%>
                        <li id="tdScenarioName" class="setting_toolbar ec-wrap-panel-item note" <%=CStr(IIf(AlternativesFilterValue = -4, "", "style='display:none;'")) %>>
                            <span>Funded <%=ParseString("%%Alternatives%%") %> of</span><br/>
                            <span><%=SafeFormString(App.ActiveProject.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.Name)%></span>
                        </li>
                        <%End If%>
                        <%If SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard Then%>
                        <li class="setting_toolbar ec-wrap-panel-item">                            
                            <a href='' id='btnToggleLayout' onclick='toggleLayoutsMenu(); return false;'><img id="imgToggleLayout" src='<% =ImagePath%>old/layoutTempl0.png' width='50' height='35' border='0' style='margin-top:2px;'/></a>
                        </li>
                        <%End If%>
                        <%--<%If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood AndAlso Not IsConsensusView() Then%>
                        <li id="tdGiven" class="setting_toolbar ec-wrap-panel-item">
                            <label><input type="checkbox" <%=IIf(PM.Parameters.ShowLikelihoodsGivenSources, " checked='checked' ", "")%> onclick="cbShowLikelihoodsGivenSourcesChange(this.checked);" />Given</label>
                        </li>
                        <%End If%>--%>
                        <%If IsConsensusView() Then%>                        
                        <li class="setting_toolbar ec-wrap-panel-item">
                            <a id="btnCVRefresh" class="dt-button buttons-html5 ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only" style="margin: auto 2px; width: 7em;" href="#" onclick="onRefreshCVClick();"><span><%=ResString("btnRefresh")%></span></a>
                            <label>
                                <input type='checkbox' class='checkbox' id='cbCVShowSteps' style='padding-top:2px;' <%=IIf(CVShowAllRows, " checked='checked' ", "")%> onclick="cbShowStepsWithNoJudgmentChange(this.checked);">
                                <%=ResString("btnCVShowNoJudgements")%>
                            </label>
                            <br />
                        </li>
                        <%End If%>
                        <%If Not IsConsensusView() Then%>
                        <%--<%If IsMixedOrGridView() OrElse SynthesisView = SynthesisViews.vtAlternativesChart OrElse SynthesisView = SynthesisViews.vtObjectivesChart Then%>--%>
                        <% If SynthesisView = SynthesisViews.vt2D OrElse SynthesisView = SynthesisViews.vtASA OrElse SynthesisView = SynthesisViews.vtDSA OrElse SynthesisView = SynthesisViews.vtGSA OrElse SynthesisView = SynthesisViews.vtHTH OrElse SynthesisView = SynthesisViews.vtPSA Then%>
                        <li class="setting_toolbar ec-wrap-panel-item">
                            <a href='' id="btnPrevUser" class='actions' style="position: relative;" onclick='selectPrevUser(); return false;' title="Select Prev User/Group">
                                <i class="fas fa-chevron-left fa-2x"></i>
                            </a>
                        </li>                
                        <%End If %>
                        <% If SynthesisView <> SynthesisViews.vt4ASA Then %>
                        <li class="setting_toolbar ec-wrap-panel-item">
                            <a href='' id="btnUserSelection" class='actions' style="position: relative;" onclick='showSelectUsersDialog(); return false;' title="<%=SafeFormString(ResString("btnParticipantsAndGroups"))%>&hellip;">
                                <i class="fas fa-users fa-2x"></i><%--<br/>--%>
                                <%If Not IsMixedOrSensitivityView() Then%>
                                <div class="small" style="margin: 0px 2px; display: inline-block; font-weight: bold; vertical-align: bottom;" id="lblSelectedCount"><%=String.Format("{0}/{1}", SelectedUsersAndGroupsIDs.Count, PM.UsersList.Count + PM.CombinedGroups.GroupsList.Count)%></div>
                                <%End If%>
                            </a>
                        </li>
                        <%End If %>
                        <% If SynthesisView = SynthesisViews.vt2D OrElse SynthesisView = SynthesisViews.vtASA OrElse SynthesisView = SynthesisViews.vtDSA OrElse SynthesisView = SynthesisViews.vtGSA OrElse SynthesisView = SynthesisViews.vtHTH OrElse SynthesisView = SynthesisViews.vtPSA Then%>
                        <li class="setting_toolbar ec-wrap-panel-item">
                            <a href='' id="btnNextUser" class='actions' style="position: relative;" onclick='selectNextUser(); return false;' title="Select Prev User/Group">
                                <i class="fas fa-chevron-right fa-2x"></i>
                            </a>
                        </li>                
                        <%End If %>
                        <li class="setting_toolbar ec-wrap-panel-item">
                            <%If PM.Attributes.GetUserAttributes().Where(Function(u) Not u.IsDefault).Count > 0 Then%>
                            <a href='' class='actions' onclick='filterUsersDialog(); return false;' title="<%=JS_SafeString("Filter by participant attributes and create dynamic groups")%>&hellip;"><i class="fas fa-users-cog fa-2x"></i></a>
                            <%End If%>
                        </li>
                        <%--<%End If%>--%>
                        <%End If%>   
                        <%If SynthesisView <> SynthesisViews.vtObjectives AndAlso SynthesisView <> SynthesisViews.vtObjectivesChart Then%>
                        <li class="setting_toolbar ec-wrap-panel-item">                          
                            <nobr><span>Filter <%=ParseString("%%Alternatives%%").ToLower%>:</span></nobr><br/>
                            <nobr><select class='select' id='cbAltsFilter' style='width:250px;' onchange='applyAltsFilter(this.value*1);'>
                                <option value='-1'<%=IIf(AlternativesFilterValue = -1, " selected='selected'", "")%>>Show all <%=ParseString("%%Alternatives%%").ToLower%></option>
                                <option value='5' <%=IIf(AlternativesFilterValue = 5, " selected='selected'", "")%>>Show top 5 <%=ParseString("%%Alternatives%%").ToLower%> based on All Participants <% = GetPrioritiesWording() %></option>
                                <option value='10'<%=IIf(AlternativesFilterValue = 10, " selected='selected'", "")%>>Show top 10 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = GetPrioritiesWording() %></option>
                                <option value='25'<%=IIf(AlternativesFilterValue = 25, "  selected='selected'", "")%>>Show top 25 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = GetPrioritiesWording() %></option>
                                <option value='-105' <%=IIf(AlternativesFilterValue = -105, " selected='selected'", "")%>>Show bottom 5 <%=ParseString("%%Alternatives%%").ToLower%> based on All Participants <% = GetPrioritiesWording() %></option>
                                <option value='-110'<%=IIf(AlternativesFilterValue = -110, " selected='selected'", "")%>>Show bottom 10 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = GetPrioritiesWording() %></option>
                                <option value='-125'<%=IIf(AlternativesFilterValue = -125, "  selected='selected'", "")%>>Show bottom 25 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = GetPrioritiesWording() %></option>
                                <option value='-2'<%=IIf(AlternativesFilterValue = -2, " selected='selected'", "")%>>Advanced: show top X <%=ParseString("%%alternatives%%").ToLower%></option>
                                <%If App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.ResourceAlignerEnabled, Nothing, True) Then%>
                                <option value='-4'<%=IIf(AlternativesFilterValue = -4, " selected='selected'", "")%> title="Show funded in current scenario">Show funded <%=ParseString("%%alternatives%%").ToLower%> - <%=SafeFormString(App.ActiveProject.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.Name)%></option>
                                <%End If%>
                                <option value='-3'<%=IIf(AlternativesFilterValue = -3, " selected='selected'", "")%>>Select/deselect <%=ParseString("%%alternatives%%").ToLower%></option>
                                <option value='-5'<%=IIf(AlternativesFilterValue = -5, " selected='selected'", "")%> <%=IIf(ActiveProjectHasAlternativeAttributes, "", " disabled='disabled' ")%>>Filter by <%=ParseString("%%alternative%%").ToLower%> attributes</option>
                                <%If PM.IsRiskProject AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) Then%>
                                <option value='-6'<%=IIf(AlternativesFilterValue = -6, " selected='selected'", "")%>>Show <% = ParseString("%%risks%%").ToLower %> only</option>
                                <option value='-7'<%=IIf(AlternativesFilterValue = -7, " selected='selected'", "")%>>Show <% = ParseString("%%opportunities%%").ToLower %> only</option>
                                <%End If%>
                            </select>
                            <a href='#' id='btnAdvEdit' class='actions' onclick='editAdvClick(); return false;' style="display:none;"><img id='btnAdvEditImg' style='vertical-align:middle; margin:0px 3px 3px 1px;' src='<%=ImagePath()%>old/edit_tiny.gif' alt='Edit' /></a>
                            </nobr><br><span class="text" id="lblAdvFilter" style="position: absolute;"></span>
                        </li>
                        <%If PM.IsRiskProject AndAlso Not RiskPageIDs.Contains(CurrentPageID) Then%>
                        <li class="setting_toolbar ec-wrap-panel-item selected-node-options">
                            <nobr><span>Show</span></nobr><br/><select class='select' id='cbSwitchShowPriorities' style='min-width:70px;' onchange='switchShowPriorities(this.value*1);'>
                                <option value="0" <%=If(Not PM.Parameters.ShowLikelihoodsGivenSources, "selected='selected'", "") %>><%=ParseString(If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, "%%Vulnerability%%", "Consequence Of %%Alternative%% On %%Objective(i)%%"))%></option>
                                <option value="1" <%=If(PM.Parameters.ShowLikelihoodsGivenSources, "selected='selected'", "") %>><%=ParseString(If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, "%%Likelihood%%", "%%Impact%% Of %%Alternative%%"))%></option>
                            </select>
                        </li>
                        <%End If%>
                        <%End If%>
                        <%If Not IsConsensusView() Then%>
                        <%If SynthesisView <> SynthesisViews.vtObjectives AndAlso SynthesisView <> SynthesisViews.vtObjectivesChart AndAlso Not PM.IsRiskProject Then%>
                        <li class="setting_toolbar ec-wrap-panel-item" id="tdOptNormalization">
                            <span>Normalize Options:</span><br /><%=GetNormalizationOptions()%>
                        </li>
                        <%End If%>
                        <%If PM.IsRiskProject AndAlso False Then%>
                        <li class="setting_toolbar ec-wrap-panel-item">
                            <nobr>
                                <div style="margin-top: 6px; text-align: left;">
                                    <div style="display: inline-block; position:relative; margin-top: -4px; padding-right:3px; height:14px; text-align: right; vertical-align: middle;">
                                        <span class="text">Simulated Results</span>
                                    </div>
                                    <label class="ec_switch small_element" style="position: relative; vertical-align: middle;">
                                      <input id="cbSimulatedSwitch" type="checkbox" <%=If(PM.CalculationsManager.UseSimulatedValues, " checked='checked' ", "") %> onclick="switchSimResults(this.checked);">
                                      <span class="ec_slider small"></span>
                                    </label>
                                    </span>
                                </div>
                            </nobr>
                        </li>
                        <%End If%>
                        <%If Not PM.IsRiskProject Then%>
                        <%If SynthesisView <> SynthesisViews.vtASA AndAlso SynthesisView <> SynthesisViews.vt4ASA Then%>
                        <li class="setting_toolbar ec-wrap-panel-item on_advanced" id="tdOptSynthMode">
                            <nobr><label><input type='radio' class='radio' id='rbIdeal'        name='radioSynthMode' value='<%=CInt(ECSynthesisMode.smIdeal)%>' onclick='rbSynthesisModeChange(this.value);' <%=IIf(App.ActiveProject.ProjectManager.CalculationsManager.SynthesisMode = ECSynthesisMode.smIdeal, "checked='checked'", "")%>>Ideal</label></nobr><br />
                            <nobr><label><input type='radio' class='radio' id='rbDistributive' name='radioSynthMode' value='<%=CInt(ECSynthesisMode.smDistributive)%>' onclick='rbSynthesisModeChange(this.value);' <%=IIf(App.ActiveProject.ProjectManager.CalculationsManager.SynthesisMode = ECSynthesisMode.smDistributive, "checked='checked'", "")%>>Distributive</label></nobr>
                        </li>
                        <%End If%>
                        <%If ((Not IsSensitivityView() AndAlso SynthesisView <> SynthesisViews.vtObjectives AndAlso SynthesisView <> SynthesisViews.vtObjectivesChart)) AndAlso SynthesisView <> SynthesisViews.vtASA AndAlso SynthesisView <> SynthesisViews.vt4ASA AndAlso SynthesisView <> SynthesisViews.vtGSA Then %>
                        <li class="setting_toolbar ec-wrap-panel-item on_advanced" id="tdOptCombinedMode">
                            <nobr><label><input type='radio' class='radio' id='rbAIJ' name='radioPrioritiesMode' value='<%=CInt(CombinedCalculationsMode.cmAIJ)%>' onclick='rbCombinedModeChange(this.value);' <%=IIf(App.ActiveProject.ProjectManager.CalculationsManager.CombinedMode = CombinedCalculationsMode.cmAIJ, "checked='checked'", "")%>>AIJ</label></nobr><br />
                            <nobr><label><input type='radio' class='radio' id='rbAIP' name='radioPrioritiesMode' value='<%=CInt(CombinedCalculationsMode.cmAIPTotals)%>' onclick='rbCombinedModeChange(this.value);' <%=IIf(App.ActiveProject.ProjectManager.CalculationsManager.CombinedMode <> CombinedCalculationsMode.cmAIJ, "checked='checked'", "")%>>AIP</label></nobr>
                        </li>
                        <%End If%>
                        <%If SynthesisView <> SynthesisViews.vtObjectives Then%>
                        <li class="setting_toolbar ec-wrap-panel-item on_advanced">
                            <nobr><label><input type='checkbox' class='checkbox' id='cbIncludeIdeal' style='padding-top:2px;' <% = If(App.ActiveProject.ProjectManager.PipeParameters.IncludeIdealAlternative, " checked='checked' ", "") %> onclick="cbIncludeIdealChange(this.checked);">Include ideal <% = ParseString("%%alternative%%") %></label></nobr><br>                            
                        </li>     
                        <%End If%>                                                                  
                        <%End If%>
                        <li class="setting_toolbar ec-wrap-panel-item on_advanced" id="tdOptUseCISWeight">
                            <nobr><label><input type='checkbox' class='checkbox' id='cbCIS'         style='padding-top:2px;' <%=IIf(App.ActiveProject.ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes, " checked='checked' ", "")%> onclick="cbUseCISChange(this.checked);">CIS</label></nobr><br>
                            <nobr><label><input type='checkbox' class='checkbox' id='cbUserWeights' style='padding-top:2px;' <%=IIf(App.ActiveProject.ProjectManager.CalculationsManager.UseUserWeights, " checked='checked' ", "")%> onclick="cbUseWeightsChange(this.checked);">User Priorities</label></nobr>
                        </li>
                        <%End If%>
                        <%If IsConsensusView() Then%>                        
                        <li class="setting_toolbar ec-wrap-panel-item">
                            <nobr><span><%=ResString("lblCVFilterBy")%>:</span><br />
                            <select class='select' id='cbCVFilterBy' style='width:33ex;' onchange='cbCVFilterByChange(this.value);'>
                                  <option value='0'<%=IIf(CInt(CVFilterBy) = 0, " selected='selected'", "")%>><%=ParseString("Judgments about %%Alternatives%%")%></option>
                                  <option value='1'<%=IIf(CInt(CVFilterBy) = 1, " selected='selected'", "")%>><%=ParseString("Judgments about %%Objectives%%")%></option>
                                  <option value='2'<%=IIf(CInt(CVFilterBy) = 2, " selected='selected'", "")%>><%=ParseString("Judgments about both %%Alternatives%% and %%Objectives%%")%></option>
                            </select>
                            </nobr>
                        </li>
                        <li class="setting_toolbar ec-wrap-panel-item">
                            <nobr><span><%=ResString("lblCVFilterRows")%>:</span><br />
                            <select class='select' id='cbCVFilterRows' style='width:33ex;' onchange='cbCVFilterRowsChange(this.value);'>
                                  <option value='-1'<%=IIf(CVShowTopNRows = -1, " selected='selected'", "")%>><%=ResString("optCVShowAllRows")%></option>
                                  <option value= '5'<%=IIf(CVShowTopNRows = 5, "  selected='selected'", "")%>><%=String.Format(ResString("optCVShowTopHighRows"), 5)%></option>
                                  <option value='10'<%=IIf(CVShowTopNRows = 10, " selected='selected'", "")%>><%=String.Format(ResString("optCVShowTopHighRows"), 10)%></option>              
                                  <option value='25'<%=IIf(CVShowTopNRows = 25, " selected='selected'", "")%>><%=String.Format(ResString("optCVShowTopHighRows"), 25)%></option>
                            </select>
                            </nobr>
                        </li>
                        <%End If%>
                        <li class="setting_toolbar ec-wrap-panel-item" style="border-right: 1px solid #cccccc;">
                            <nobr><span><%=ResString("lblDecimals")%>:</span><br />
                            <select class='select' id='cbDecimals' style='width:7ex;' onchange='cbDecimalsChange(this.value);'>
                                  <option value='0'<%=IIf(PM.Parameters.DecimalDigits = 0, " selected='selected'", "") %>>0</option>
                                  <option value='1'<%=IIf(PM.Parameters.DecimalDigits = 1, " selected='selected'", "") %>>1</option>
                                  <option value='2'<%=IIf(PM.Parameters.DecimalDigits = 2, " selected='selected'", "") %>>2</option>              
                                  <option value='3'<%=IIf(PM.Parameters.DecimalDigits = 3, " selected='selected'", "") %>>3</option>
                                  <option value='4'<%=IIf(PM.Parameters.DecimalDigits = 4, " selected='selected'", "") %>>4</option>
                                  <option value='5'<%=IIf(PM.Parameters.DecimalDigits = 5, " selected='selected'", "") %>>5</option>
                            </select>
                            </nobr>
                        </li>
            <%If Not IsConsensusView() Then%>
            <%If IsMixedOrSensitivityView() OrElse SynthesisView = SynthesisViews.vtASA OrElse SynthesisView = SynthesisViews.vt4ASA OrElse SynthesisView = SynthesisViews.vtAlternativesChart OrElse SynthesisView = SynthesisViews.vtObjectivesChart Then%>                    
                        <%If SynthesisView = SynthesisViews.vtDSA Or SynthesisView = SynthesisViews.vtPSA Or SynthesisView = SynthesisViews.vtGSA Or SynthesisView = SynthesisViews.vtASA Or SynthesisView = SynthesisViews.vt4ASA Or SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard Or SynthesisView = SynthesisViews.vtObjectivesChart Or SynthesisView = SynthesisViews.vt2D Or SynthesisView = SynthesisViews.vtHTH Then%>
                        <li id="tdSortObjectivesBy" class="sa_toolbar ec-wrap-panel-item" style="border-right: 1px solid #cccccc; padding: 2px;">
                            <nobr><span><%=ParseString("Sort %%objectives%% by")%>: </span><br/>
                            <select class='select' id='cbASASortBy' style='text-align: right; min-width: 100px;' onchange='cbASASortByChange(this.value);'>
                                <option value='0' <%=If(PM.Parameters.AsaSortMode = 0, " selected='selected'", "")%>><%=ResString("lblNone")%></option>
                                <option value='1' <%=If(PM.Parameters.AsaSortMode = 1, " selected='selected'", "")%>>Priority</option>
                                <option value='2' <%=If(PM.Parameters.AsaSortMode = 2, " selected='selected'", "")%>>Name</option>
                            </select>
                            </nobr>
                        </li>
                        <%End If%>
                        <%If SynthesisView = SynthesisViews.vtDSA OrElse SynthesisView = SynthesisViews.vt2D OrElse SynthesisView = SynthesisViews.vtGSA OrElse SynthesisView = SynthesisViews.vtMixed OrElse SynthesisView = SynthesisViews.vtDashboard OrElse SynthesisView = SynthesisViews.vtAlternativesChart OrElse SynthesisView = SynthesisViews.vtHTH Then%>
                        <li class="sa_toolbar ec-wrap-panel-item" style="border-right: 1px solid #cccccc; padding: 2px;">
                            <div style="display: inline-block;">
                            <nobr><span><%=ParseString("Sort %%alternatives%% by")%>: </span></nobr><br/>
                            <select class='select' id='cbSAAltsSortBy' style='text-align: right; min-width: 100px;' onchange='cbSAAltsSortByChange(this.value);'>
                                <option value='0' <%=If(PM.Parameters.SensitivitySorting = SASortMode.AltsByIndex, " selected='selected'", "")%>><%=ResString("lblNone")%></option>
                                <option value='1' <%=If(PM.Parameters.SensitivitySorting = SASortMode.AltsByPrty, " selected='selected'", "")%>><% = If(PM.IsRiskProject, ParseString("%%Risk%%"), "Priority") %></option>
                                <option value='2' <%=If(PM.Parameters.SensitivitySorting = SASortMode.AltsByName, " selected='selected'", "")%>>Name</option>
                            </select>
                            </div>
                            <%If SynthesisView = SynthesisViews.vtDSA OrElse SynthesisView = SynthesisViews.vtMixed OrElse SynthesisView = SynthesisViews.vtDashboard Then%>
                            <div style="display: inline-block;">
                                <label id="lblActiveSorting" style="<% = If(PM.Parameters.SensitivitySorting <> SASortMode.AltsByPrty, "color: #909090", "")%>"><input type='checkbox' class='checkbox' id="cbActiveSorting" style='padding-top:2px;' onclick="onActiveSorting(this.checked);" <%=If(PM.Parameters.DsaActiveSorting, " checked='checked' ", " ")%> <%=If(PM.Parameters.SensitivitySorting <> SASortMode.AltsByPrty, " disabled='disabled' ", " ")%>/><%=ResString("btnActiveSorting")%></label>
                            </div>
                            <% End If %>
                        </li>     
                        <%End If%>
                        <%If SynthesisView = SynthesisViews.vtDSA Or SynthesisView = SynthesisViews.vtPSA Or SynthesisView = SynthesisViews.vtGSA Or SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard Then%>
                        <%If Not RiskPageIDs.Contains(CurrentPageID) AndAlso PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidImpact Then%>
                        <li class="sa_toolbar ec-wrap-panel-item">
                            <div style="display: inline-block;">
                            <nobr><span><%=ParseString("%%Alternatives%% parameter")%>:</span></nobr><br/>
                            <select class='select' id='cbSAAltsParameter' style='text-align: right; min-width: 100px;' onchange='cbSAAltsParameterChange(this.value);'>
                                <option value='0' <%=If(PM.Parameters.RiskSensitivityParameter = 0, " selected='selected'", "")%>><%=ParseString("%%Impact%%")%></option>
                                <option value='1' <%=If(PM.Parameters.RiskSensitivityParameter = 1, " selected='selected'", "")%>><%=ParseString("%%Risk%%")%></option>
                            </select>
                            </div>
                        </li>
                        <%End If%>
                        <%End If%>                   
                        <%--<%If IsMixedOrSensitivityView() OrElse SynthesisView = SynthesisViews.vtASA OrElse SynthesisView = SynthesisViews.vt4ASA Then%>                    
                        <td class="ec_toolbar_td_separator">
                            <a href='' class='actions' onclick='resetSA(); return false;'>
                                <i class="fas fa-undo fa-2x"></i>
                            </a>
                        </td>
                        <%End If%>--%>
                        <%If (SynthesisView = SynthesisViews.vtASA OrElse SynthesisView = SynthesisViews.vt4ASA) Then%>                        
                        <li class="sa_toolbar ec-wrap-panel-item">
                            <nobr><span>Page size:</span><br/>
                            <select class='select' id='cbASAPageSize' style='width:50px; text-align: right;' onchange='cbASAPageSizeChange(this.value);'>
                                  <option value='5'<%=If(PM.Parameters.AsaPageSize = 5, " selected='selected'", "")%>>5</option>
                                  <option value='10'<%=If(PM.Parameters.AsaPageSize = 10, " selected='selected'", "")%>>10</option>
                                  <option value='15'<%=If(PM.Parameters.AsaPageSize = 15, " selected='selected'", "")%>>15</option>
                                  <option value='20'<%=If(PM.Parameters.AsaPageSize = 20, " selected='selected'", "")%>>20</option>
                            </select>
                            </nobr>
                        </li>
                        <li class="sa_toolbar ec-wrap-panel-item">
                            <nobr><span>Page num:</span><br/>
                            <select class='select' id='cbASAPageNum' style='width:50px; text-align: right;' onchange='cbASAPageNumChange(this.value);'>
                            </select>
                            </nobr>
                        </li>
                        <%End If%>                        
                        <%If IsMixedOrSensitivityView() AndAlso SynthesisView <> SynthesisViews.vtASA AndAlso SynthesisView <> SynthesisViews.vt4ASA AndAlso SynthesisView <> SynthesisViews.vtHTH Then%>
                        <li class="sa_toolbar ec-wrap-panel-item on_advanced">
                            <a href='' id='btnWRTState' class='actions' onclick='toggleWRTState(); return false;' style="display:inline-block;"><img id='btnWRTStateImg' class='<%=CStr(IIf(Api.WRTState = ECWRTStates.wsGoal, "toggle_btn_checked", "toggle_btn_unchecked")) %>' align='left' style='vertical-align:middle;' src='<%=ImagePath() + CStr(IIf(Api.WRTState = ECWRTStates.wsGoal, "objectives_goal_32.png", "objectives_node_32.png"))%>' alt='' /></a>
                        </li>
                        <%End If%>  
                        <%If SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard Or SynthesisView = SynthesisViews.vtDSA Then%>
                        <li class="sa_toolbar ec-wrap-panel-item" style="vertical-align: middle;">
                            <label><input type='checkbox' class='checkbox' id="cbShowComponents" style='padding-top:2px;' onclick="onShowComponents(this.checked);" <%=IIf(Api.ShowComponents, " checked='checked' ", " ")%> /><%=ResString("btnShowComponents")%></label>
                            <br class="on_advanced" />
                            <label class="on_advanced" title="<%=ResString("titleShowMarkers")%>"><input type='checkbox' class='checkbox' id='cbShowMarkers' style='padding-top:2px;' onclick="showMarkers(this.checked);"/><%=ResString("btnShowMarkers")%></label>
                        </li>
                        <%End If%>                        
                        <%If SynthesisView = SynthesisViews.vtAlternativesChart OrElse SynthesisView = SynthesisViews.vtObjectivesChart Then%>
                        <li class="sa_toolbar ec-wrap-panel-item">
                            <div id="divChartsToolbar"></div>
                        </li>
                        <%End If%>
            <%End If%>
            <%End If%> 
            </ul>
        </td>
    </tr>
    <tr id="trInfobar" valign="top">
        <td valign="top">
            <div id='divInfobar' style="margin-bottom:-5px; margin-top: 2px; margin-left: 0px; margin-right: 0px;">
                <h5 id="pgHeader"><% = GetTitle() %></h5>
             </div>
        </td>
    </tr>
    <tr id="trContent" class="whole" valign="top">
        <td id='tdContent' class="whole" valign='top'<% If isSLTheme Then %> style="border-top:1px solid #cccccc;"<% End if %>>
            <div id="divLeftCon" style="width: 0; height: 0; text-align: left; vertical-align: top;"></div>
            <div id="divTree" class="split_left splitter_left" style="float: left; border-spacing: 0px; height: 100px; display:<%=If(PM.Parameters.Synthesis_ObjectivesVisibility, "", "none")%>; width: 200px;">
                <div id='divInnerContent-1' style='margin-right: 8px;'></div>
            </div>            
            <div id='divContent' style='position: absolute; display: inline-block; overflow: hidden; vertical-align: top;'>
            </div>            
        </td>        
    </tr>
</table>

<%-- Users --%>
<div id="divUsersAndGroups" style="display:none;position:relative;">
    <table border="0" cellpadding="0" cellspacing="0" style="width:100%;">
        <tr valign="top">
            <td valign="bottom" style="width:450px;"><table id='tableUsers' class='text cell-border hover order-column' style='width:450px;'></table></td>
            <td valign="bottom" style="padding-left:5px;width:300px;"><table id='tableGroups' class='text cell-border hover order-column' style='width:100%;'></table></td>
        </tr>
    </table>
    <%If Not IsMixedOrSensitivityView() Then%>
    <div style='text-align:center; margin-top:1ex; width:100%;'>
        <a href="" onclick="return SelectAllUsers(1)" class="actions"><% =ResString("lblSelectAll")%></a> |
        <a href="" onclick="return SelectAllUsers(2)" class="actions"><% =ResString("btnSelectAllWithData")%></a> | 
        <a href="" onclick="return SelectAllUsers(0)" class="actions"><% =ResString("lblDeselectAll")%></a>
    </div>
    <%End If%>
</div>

<%-- Filter Users By Attributes --%>
<div id="divUserAttributes" style="display:none;">
    <div style="padding:0px 5px 0px 5px; text-align:left;">
        <!-- Filter combinations toolbar -->
        <div style='text-align: left; margin-top:1ex' class='text'>
            <nobr><div id="cbUsersFiltersCombination" style="display: inline-block;"></div> &nbsp;          
            <a href="" title="Add Rule" onclick="onUsersFiltersAddNewRule(); return false;"><i class="fas fa-plus fa-2x" style="color: MediumSeaGreen;"></i></a>&nbsp;
            <a href="" title="Preview" onclick="initUsersFilteredTable(); return false;"><i class="fas fa-check fa-2x" style="color: MediumSeaGreen;"></i></a>&nbsp;
            <a href="" title="Clear" onclick="onUsersFiltersReset(); return false;"><i class="fas fa-eraser fa-2x" style="color: LightCoral;"></i></a>
            </nobr>
        </div>

        <p align="left" id='divUsersFilters' style="margin:3px 3px 3px 20px;">Loading...</p>
    </div>
    <div style='width:100%; height: 240px;'>
        <div id="tableFilteredUsers"></div>
    </div>
</div>

<%-- 4 ASA Users --%>
<%=Get4ASAParticipants(0)%>
<%=Get4ASAParticipants(1)%>
<%=Get4ASAParticipants(2)%>
<%=Get4ASAParticipants(3)%>

<%-- 4 ASA Keep checkboxes --%>
<%=Get4ASAKeepCb(0)%>
<%=Get4ASAKeepCb(1)%>
<%=Get4ASAKeepCb(2)%>
<%=Get4ASAKeepCb(3)%>

<%-- Filter Alternatives - Advanced --%>
<div id="divAltsAdvanced" style="display:none;position:relative; text-align:center; padding-top:30px; vertical-align:middle;">
    Select top <% = GetAltsNumDropdown() %>&nbsp;<%=ParseString("%%alternatives%%")%> based on <% = GetUsersDropdown()%>&nbsp;<% = GetPrioritiesWording() %>
</div>

<%-- Filter Alternatives - Select/Deselect --%>
<div id="divAltsCustom" style="display:none;position:relative;">
    <div style="padding:5px; text-align:left;">
        <%=GetAltsCheckList()%>
    </div>
    <div id="pnlAllOrNoneSelector" style='float:left; margin-top:2ex; margin-right:250px;'>
        <a href="" onclick="filterAltsCustomSelect(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterAltsCustomSelect(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>

<%-- Filter by alternative attributes --%>
<div id="divAltsAttributes" style="display:none;position:relative;">
    <div style="padding:5px; text-align:left;">
        <!-- Filter combinations toolbar -->
        <div style='text-align:center; margin-top:1ex' class='text'>
            <nobr>Use: <select id="cbFilterCombination">
                <option value='0'<% =IIf(FilterCombination = 0, " selected", "") %>>AND</option>
                <option value='1'<% =IIf(FilterCombination = 1, " selected", "") %>>OR</option>                
            </select> &nbsp;          
            <input type='button' class='button' style='width:11ex' id='btnAddRule' value='Add&nbsp;Rule' onclick='onAddNewRule(); return false;' />
            <input type='button' class='button' style='width:11ex' id='btnRest' value='Reset' onclick='onFilterReset(); return false;' /></nobr>
        </div>

        <p align="center" id='divFilters'>Loading...</p>
    </div>    
</div>

<%-- Events with Checkboxes dialog --%>
<div id='selectEventsForm' style='display: none; overflow:hidden;'>
    <div id="divSelectEvents" style="padding: 5px; text-align: left; overflow:auto; height:300px;"></div>
    <div style='text-align: center; margin-top: 1ex; width: 100%;'>
        <a href="" onclick="filterSelectAllEvents(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterSelectAllEvents(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>

    <div id="colorDlg"></div>
</asp:Content>