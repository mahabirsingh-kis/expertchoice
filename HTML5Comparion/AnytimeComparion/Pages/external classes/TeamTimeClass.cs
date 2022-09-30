using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using ECCore;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Service;
using ECWeb = ExpertChoice.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using AnytimeComparion.Pages.TeamTimeTest;
using System.Windows.Forms;

namespace AnytimeComparion.Pages.external_classes
{
    public class TeamTimeClass
    {
        private static clsAction _Action;
        public static string sCallbackSave = "";
        public static bool _TeamTimeRefreshTimeoutLoaded = false;
        public static int _TeamTimeRefreshTimeout = ECWeb.WebOptions.SynchronousRefresh();

        public static string ClusterPhrase = "";
        public static bool ClusterPhraseIsCustom = false;
        public static string TaskNodeGUID = "";
        private static string sOldWRTPath = null;
        private static string SESS_WRT_PATH = "";
        private static List<ECTypes.clsUser> _TeamTimeUsersList = null;

        public static Dictionary<string, string> TaskTemplates = new Dictionary<string, string>();

        private static bool _isTeamTime = false;

        internal const string _SESS_VARIANCE = "tt_variance";
        internal const string _SESS_PW_SIDE = "tt_pw_side";
        internal const string _SESS_HIDE_COMBINED = "tt_no_combined";
        internal const string _COOKIE_USERSPAGE = "tt_pg";
        internal const string _COOKIE_USERS_PAGES_USE = "tt_up";
        internal const string _COOKIE_USERS_PAGES_SIZE = "tt_ps";
        internal const string _COOKIE_TT_NORM = "tt_res_norm";
        internal const bool _OPT_SHOW_ONLINE_ONLY_PROJECT = true;
        const string SessUserDisplay = "UserDisplayName";
        const string SessHidePie = "SessHidePie";
        private const int _TeamTimeSessionDataTimeoutCoeff = 10;
        public static int _TeamTimePipeStep = -1;

        public static bool _TeamTimeActive = true;
        internal const int ShowStepsCount = 11;

        public static string globalGUID = "";

        public static clsLanguageResource language;

        private static clsTeamTimePipe _TeamTime_Pipe = null;
        public const string _SESS_TT_Pipe = "Sess_TT_Pipe";
        private const string _SESS_TT_Step = "_SESS_TT_Step";
        private const string _SESS_STEPS_LIST = "tt_steps_list";
        private const string _SESS_TT_USERSLIST = "tt_users_list";
        private static int _wrtNodeID = -1;
        private static bool _ForceSaveStepInfo = false;
        public enum ecPipeStepStatus
        {
            psNoJudgments = 0,
            psMissingJudgments = 1,
            psAllJudgments = 2
        };
        public enum SynchronousEvaluationMode
        {
            semNone = 0,
            semOnline = 1,
            semVotingBox = 2,
            semByFacilitatorOnly = 3
        };


        public static clsAction Action(int Step)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var pipeCount = TeamTime == null ? 0 : TeamTime.Pipe.Count;

            if(Step > 0 && Step < pipeCount)
            {
                _Action = GetAction(Step);
            }
            else
            {
                int MeetingOwner;
                if (App.ActiveProject.MeetingOwner != null)
                {
                    MeetingOwner = App.ActiveProject.MeetingOwner.UserID;
                }
                else
                {
                    MeetingOwner = App.ActiveProject.OwnerID;
                }
                var WorkSpace = App.DBWorkspaceByUserIDProjectID(MeetingOwner, App.ProjectID);
                var CurrentStep = WorkSpace.get_ProjectStep(App.ActiveProject.isImpact);
                if(CurrentStep > 0 && CurrentStep <= pipeCount)
                {
                    _Action = GetAction(CurrentStep);
                    TeamTimePipeStep = CurrentStep;
                }
                else
                {
                    _Action = GetAction(1);
                    TeamTimePipeStep = 1;
                }
                
            }
            return _Action;
        }

        public static bool RemoveTeamTime()
        {
            HttpContext context = HttpContext.Current;
            _TeamTime_Pipe = null;
            context.Session[_SESS_TT_Pipe] = null;
            context.Session[_SESS_TT_Step] = null;
            context.Session[_SESS_TT_USERSLIST] = null;
            context.Session["steps"] = null;
            return true;
        }

        public static clsTeamTimePipe TeamTime
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];

                    if (isTeamTimeOwner)
                    {
                        if (_TeamTime_Pipe != null)
                        {
                            if (_TeamTime_Pipe.ProjectManager.StorageManager.ModelID != App.ProjectID)
                            {
                                _TeamTime_Pipe = null;
                            }
                        }
                        if (context.Session[_SESS_TT_Pipe] == null)
                        {
                            _TeamTime_Pipe = null;
                            if (context.Session[_SESS_TT_Pipe] != null)
                            {

                                _TeamTime_Pipe = (clsTeamTimePipe) context.Session[_SESS_TT_Pipe];
                                if ((_TeamTime_Pipe.ProjectManager.StorageManager.ModelID != App.ProjectID) ||
                                    (_TeamTime_Pipe.ProjectManager.ActiveHierarchy !=
                                     App.ActiveProject.ProjectManager.ActiveHierarchy))
                                {
                                    _TeamTime_Pipe = null;
                                }
                            }

                            if (context.Session[_SESS_TT_Pipe] == null)
                            {
                                _TeamTime_Pipe = new clsTeamTimePipe(App.ActiveProject.ProjectManager,
                                    App.ActiveProject.ProjectManager.User);
                                _TeamTime_Pipe.Override_ResultsMode = true;
                                _TeamTime_Pipe.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth;
                                if (context.Session[_SESS_TT_USERSLIST] == null)
                                {
                                    _TeamTime_Pipe.VerifyUsers(TeamTimeUsersList);
                                }
                                else
                                {
                                    _TeamTime_Pipe.VerifyUsers(
                                        (List<ECTypes.clsUser>) context.Session[_SESS_TT_USERSLIST]);
                                }

                                _TeamTime_Pipe.CreatePipe();
                                _ForceSaveStepInfo = true;
                                context.Session.Remove(_SESS_TT_Pipe);
                                context.Session.Add(_SESS_TT_Pipe, _TeamTime_Pipe);
                                context.Session.Remove(_SESS_STEPS_LIST);
                            }
                        }
                        _TeamTime_Pipe = (clsTeamTimePipe) context.Session[_SESS_TT_Pipe];
                        if (_TeamTime_Pipe.ProjectManager.ActiveHierarchy < 0)
                        {
                            _TeamTime_Pipe = new clsTeamTimePipe(App.ActiveProject.ProjectManager,
                                App.ActiveProject.ProjectManager.User);
                            _TeamTime_Pipe.Override_ResultsMode = true;
                            _TeamTime_Pipe.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth;
                            _TeamTime_Pipe.VerifyUsers(TeamTimeUsersList);
                            _TeamTime_Pipe.CreatePipe();
                            _ForceSaveStepInfo = true;
                            context.Session.Remove(_SESS_TT_Pipe);
                            context.Session.Add(_SESS_TT_Pipe, _TeamTime_Pipe);
                            context.Session.Remove(_SESS_STEPS_LIST);
                        }
                    }
                    else
                    {
                        if (_TeamTime_Pipe != null)
                        {
                            if (_TeamTime_Pipe.ProjectManager.StorageManager.ModelID != App.ProjectID)
                            {
                                _TeamTime_Pipe = new clsTeamTimePipe(App.ActiveProject.ProjectManager,
    App.ActiveProject.ProjectManager.GetUserByID(App.ActiveProject.MeetingOwnerID));
                                _TeamTime_Pipe.Override_ResultsMode = true;
                                _TeamTime_Pipe.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth;
                                _TeamTime_Pipe.VerifyUsers(TeamTimeUsersList);
                                _TeamTime_Pipe.CreatePipe();
                                _ForceSaveStepInfo = true;
                                context.Session.Remove(_SESS_TT_Pipe);
                                context.Session.Add(_SESS_TT_Pipe, _TeamTime_Pipe);
                                context.Session.Remove(_SESS_STEPS_LIST);

                            }
                        }
                    }


                    


                }
                return _TeamTime_Pipe;
            }
            set
            {
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                { 
                    _TeamTime_Pipe = value;
                    context.Session[_SESS_TT_Pipe] = _TeamTime_Pipe;
                    context.Session.Remove(_SESS_STEPS_LIST);
                }

            }
        }

        public static int WrtNodeId
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                if (isTeamTimeOwner)
                {
                    if ((_TeamTime_Pipe.ProjectManager.StorageManager.ModelID == App.ProjectID) && (_TeamTime_Pipe.ProjectManager.ActiveHierarchy == App.ActiveProject.ProjectManager.ActiveHierarchy))
                    {
                        _wrtNodeID = TeamTime.WRTNodeID;
                    }
                }
                return _wrtNodeID;
            }
        }

        public static List<ECTypes.clsUser> TeamTimeUsersList
        {
        get {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var userddd = App.ActiveUser.UserEMail;
            var userlist = new List<ECTypes.clsUser>();
            if (isTeamTimeOwner)
            {
                if (context.Session[_SESS_TT_USERSLIST] != null)
                    userlist = (List<ECTypes.clsUser>)context.Session[_SESS_TT_USERSLIST];
            }
            else
                {
                    userlist = _TeamTimeUsersList;
                    if (context.Session[_SESS_TT_USERSLIST] == null)
                        userlist = null;
                }
                


            if (_TeamTimeUsersList == null || userlist != _TeamTimeUsersList)
            {
                userlist = new List<ECTypes.clsUser>();
                List<clsWorkspace> tWSList = App.DBWorkspacesByProjectID(App.ProjectID);
                context.Session[_SESS_TT_USERSLIST] = (List<ECTypes.clsUser>)userlist;
                foreach (clsApplicationUser tAppUser in UsersList) {
                    clsWorkspace tWS = clsWorkspace.WorkspaceByUserIDAndProjectID(tAppUser.UserID, App.ProjectID, tWSList);
                    if (tWS != null && clsOnlineUserSession.OnlineSessionByUserID(tWS.UserID, SessionsList) != null)
                        tWS.set_Status(App.ActiveProject.isImpact, ecWorkspaceStatus.wsSynhronousActive);
                    // D1432 + D1945
                    // D1945
                    context.Session[_SESS_TT_USERSLIST] = (List<ECTypes.clsUser>)userlist;
                    if (tWS != null && tWS.get_isInTeamTime(App.ActiveProject.isImpact)) {
                        context.Session[_SESS_TT_USERSLIST] = (List<ECTypes.clsUser>)userlist;
                        // D1288 ==
                        bool fCanParticipate = true;
                        if (!TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess && tWS.get_TeamTimeStatus(App.ActiveProject.isImpact) == ecWorkspaceStatus.wsSynhronousReadOnly)
                            fCanParticipate = false;
                        // D1945
                        if (TeamTimePipeParams.TeamTimeHideProjectOwner && App.ActiveProject.MeetingOwnerID == tAppUser.UserID)
                            fCanParticipate = false;
                        if (fCanParticipate) {
                            // D1288 ==
                            ECTypes.clsUser tPrjUser = TeamTime.ProjectManager.GetUserByEMail(tAppUser.UserEMail);
                            bool fUpdate = false;
                            if (tPrjUser == null) {
                                tPrjUser = TeamTime.ProjectManager.AddUser(tAppUser.UserEMail, true, tAppUser.UserName);
                                // D1325
                                tPrjUser.IncludedInSynchronous = true;
                                tPrjUser.SyncEvaluationMode = (ECTypes.SynchronousEvaluationMode) SynchronousEvaluationMode.semOnline;
                                fUpdate = true;
                            } else {
                                // D1362
                                if (!tPrjUser.IncludedInSynchronous | tPrjUser.SyncEvaluationMode == (ECTypes.SynchronousEvaluationMode) SynchronousEvaluationMode.semNone)
                                {
                                    tPrjUser.IncludedInSynchronous = true;
                                    if (tPrjUser.SyncEvaluationMode == (ECTypes.SynchronousEvaluationMode) SynchronousEvaluationMode.semNone)
                                        tPrjUser.SyncEvaluationMode = (ECTypes.SynchronousEvaluationMode) SynchronousEvaluationMode.semOnline;
                                    // D1362
                                    fUpdate = true;
                                }
                            }
                            context.Session[_SESS_TT_USERSLIST] = (List<ECTypes.clsUser>)userlist;
                            if (fUpdate)
                                    TeamTime.ProjectManager.StorageManager.Writer.SaveModelStructure();
                                // D1325
                            ECTypes.clsUser tAddUser = tPrjUser.Clone();
                            tAddUser.UserName = tPrjUser.UserName;
                            // D1639                            
                            tAddUser.Active = clsOnlineUserSession.OnlineSessionByUserID(tAppUser.UserID, SessionsList) != null;
                            // D1605
                            if (!(!TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess && tPrjUser != null && tPrjUser.SyncEvaluationMode == (ECTypes.SynchronousEvaluationMode) SynchronousEvaluationMode.semByFacilitatorOnly))
                            {
                                userlist.Add(tAddUser);
                            }
                            context.Session[_SESS_TT_USERSLIST] = (List<ECTypes.clsUser>)userlist;
                        }
                    }
                    
                }
                ecUserSort Fld = ecUserSort.usEmail;
                switch (TeamTimePipeParams.TeamTimeUsersSorting) 
                {
                    case CanvasTypes.TTUsersSorting.ttusName:
                        Fld = ecUserSort.usName;
                        break;
                    case CanvasTypes.TTUsersSorting.ttusKeypadID:
                        Fld = ecUserSort.usKeyPad;
                        break;
                    default:
                        Fld = ecUserSort.usEmail;
                        break;
                }
                clsUserComparer cmp = new clsUserComparer(Fld, SortDirection.Ascending);
                userlist.Sort(cmp);
                // D1561 ==
            }
            context.Session[_SESS_TT_USERSLIST] = (List<ECTypes.clsUser>)userlist;
            _TeamTimeUsersList = userlist;
            return (List<ECTypes.clsUser>)context.Session[_SESS_TT_USERSLIST];
            }
            set
            {
                _TeamTimeUsersList = value;
            }
        }

        public static List<clsOnlineUserSession> SessionsList
        {
            get
            {
                HttpContext context = HttpContext.Current;
                List<clsOnlineUserSession> sessionsList = new List<clsOnlineUserSession>();
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    List<clsOnlineUserSession> AllOnline = App.DBOnlineSessions();

                    foreach (clsOnlineUserSession onlineUser in AllOnline)
                    {
                        if (clsApplicationUser.UserByUserID(onlineUser.UserID, UsersList) != null && (!_OPT_SHOW_ONLINE_ONLY_PROJECT || onlineUser.ProjectID == App.ProjectID))
                        { sessionsList.Add(onlineUser); }
                    }
                }
                return sessionsList;
            }
        }

        public static List<clsApplicationUser> UsersList
        {
            get
            {
                HttpContext context = HttpContext.Current;
                List<clsApplicationUser> usersList = new List<clsApplicationUser>();
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    usersList = App.DBUsersByProjectID(App.ProjectID);
                }
                return usersList;
            }
        }

        public static Canvas.PipeParameters.clsPipeParamaters TeamTimePipeParams
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    if (isTeamTimeOwner)
                    {
                        return TeamTime.PipeParameters;
                    }
                    else
                    {
                        return App.ActiveProject.PipeParameters;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static Boolean isTeamTimeOwner
        {
            get
            {
                bool teamtimeOwner = false;
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    if (isTeamTime)
                    {
                        if (App.ActiveProject.get_MeetingStatus(App.ActiveUser) == ECTeamTimeStatus.tsTeamTimeSessionOwner)
                        {
                            teamtimeOwner = true;
                        }
                    }
                    else
                    {
                        if (Consts._LOGIN_WITH_MEETINGID_TO_INACTIVE_MEETING && (App.Options.isLoggedInWithMeetingID || App.Options.OnlyTeamTimeEvaluation))
                        {
                            teamtimeOwner = false;
                        }
                    }
                }
                return teamtimeOwner;
            }
        }

        public static Boolean isTeamTime
        {
            get
            {
                bool teamtime = false;
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    if (App.ActiveProject != null)
                    {
                        teamtime = (App.ActiveProject.isTeamTimeLikelihood || App.ActiveProject.isTeamTimeImpact);
                    }
                }
                return teamtime;
            }
            set
            {
                _isTeamTime = value;
            }
        }

        public static int userdisplayname
        {
            get
            {

                var userdisplay = 0;
                int.TryParse(get_SessVar(SessUserDisplay), out userdisplay);
                return userdisplay;
            }
            set
            {
                TeamTimeClass.set_SessVar(SessUserDisplay, value.ToString());
            }
        }

        public static int HidePieChart
        {
            get
            {

                var hidePie = 0;
                int.TryParse(get_SessVar(SessHidePie), out hidePie);
                return hidePie;
            }
            set
            {
                TeamTimeClass.set_SessVar(SessHidePie, value.ToString());
            }
        }

        public static string GetPipeData()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var sData = "";
            if (isTeamTime)
            {
                if (isTeamTimeOwner)
                {
                     
                    clsAction tAction = GetAction(TeamTimePipeStep);
                    DateTime Dt = DateTime.Now;
                    List<string> tRows = App.DBTeamTimeDataReadAll(App.ProjectID, ecExtraProperty.TeamTimeJudgment);
                    if (tRows != null)
                    {
                        foreach (string sRow in tRows)
                        {
                            var val = TeamTime.ParseStringFromClient(sRow);
                            string[] sParams = sRow.Split('\t');
                            if (sParams.Count() > 4)
                                sCallbackSave += String.Format("{0}'{1}'", (sCallbackSave.Equals("") ? "" : ",").ToString(), sParams[4].Trim());
                        }
                        if (tRows.Count > 0) { App.DBTeamTimeDataDelete(App.ProjectID, Dt, ecExtraProperty.TeamTimeJudgment); }
                        if (sCallbackSave != "") { sCallbackSave = $"'pended' : [{sCallbackSave}] , "; }
                    }
                    string sType = "";
                    string sTask = "";
                    string sValue = "";

                    if (tAction != null)
                    {
                        switch (tAction.ActionType)
                        {
                            case ActionType.atInformationPage:
                                sType = "message";
                                clsInformationPageActionData Infodata = (clsInformationPageActionData)TeamTimeClass.Action(TeamTimePipeStep).ActionData;
                                sValue = string.Format("['{0}',[{1}]]", Infodata.Description.ToLower(), TeamTime.GetJSON_UsersList(TeamTimeUsersList));
                                // D1270 + D1289 + D2585
                                break;
                            case ActionType.atPairwise:
                                sType = "pairwise";
                                sValue = TeamTime.GetJSON(TeamTimeUsersList);
                                if (App.isRiskEnabled && sValue.IndexOf("%%know%%") > 0)
                                    sValue = sValue.Replace("%%known%%", StringFuncs.JS_SafeString(ResString(Convert.ToString((App.isRiskEnabled && isImpact ? "lblKnownImpactPW" : "lblKnownLikelihoodPW")))));
                                break;

                            case ActionType.atNonPWOneAtATime:
                                switch (((clsNonPairwiseEvaluationActionData)tAction.ActionData).MeasurementType)
                                {

                                    case ECMeasureType.mtRatings:
                                        sType = "rating";
                                        sValue = TeamTime.GetJSON(TeamTimeUsersList);
                                        // D1270 + D1271
                                        break;

                                    case ECMeasureType.mtRegularUtilityCurve:
                                        sType = "ruc";
                                        sValue = TeamTime.GetJSON(TeamTimeUsersList);
                                        // D1577
                                        break;

                                    case ECMeasureType.mtDirect:
                                        sType = "direct";
                                        sValue = TeamTime.GetJSON(TeamTimeUsersList);
                                        // D1509
                                        break;

                                    case ECMeasureType.mtStep:
                                        sType = "step";
                                        sValue = TeamTime.GetJSON(TeamTimeUsersList);
                                        // D1546
                                        break;

                                }

                                break;

                            case ActionType.atShowLocalResults:
                                sType = "localresults";
                                if (TeamTime.Override_ResultsMode)
                                {
                                    TeamTime.ResultsViewMode = TeamTimeHideCombined ? CanvasTypes.ResultsView.rvIndividual : CanvasTypes.ResultsView.rvBoth;
                                }
                                sValue = TeamTime.GetJSON(TeamTimeUsersList);
                                break;

                            case ActionType.atShowGlobalResults:
                                sType = "globalresults";

                                if (TeamTime.Override_ResultsMode)
                                {
                                    TeamTime.ResultsViewMode = TeamTimeHideCombined ? CanvasTypes.ResultsView.rvIndividual : CanvasTypes.ResultsView.rvBoth;
                                }
                                sValue = TeamTime.GetJSON(TeamTimeUsersList);
                                break;

                            default:
                                break;
                        }
                        if (sType != "")
                        {
                            var IfIgnoreClusterPhrase = false;
                            if (sType == "localresults" || sType == "globalresults")
                            {
                                IfIgnoreClusterPhrase = true;
                            }
                            sTask = GetPipeStepTask(tAction, null, false, IfIgnoreClusterPhrase, true);

                        }

                        if (sValue == "")
                        {
                            sType = "info";
                            sValue = String.Format(String.Format("'<h6 class=\'gray\'>{0}</h6>'", StringFuncs.JS_SafeString(App.ResString("msgTeamTimeNotSupported"))), tAction.ActionType.ToString());
                        }
                        string sEvalProgress = "";
                        if (isTeamTime)
                        {
                            foreach (ECTypes.clsUser tUser in TeamTimeUsersList)
                            {
                                string sTime = "";
                                Nullable<DateTime> tDateTime = TeamTime.GetUserJudgmentDateTime(tUser);
                                if (tDateTime.HasValue) { sTime = GetDateTimeTicks(tDateTime.Value); }
                                sEvalProgress += (sEvalProgress.Equals("") ? "" : ",") + String.Format("['{0}' , {1} , {2} , '{3}']", StringFuncs.JS_SafeString(tUser.UserEMail), TeamTime.GetUserEvaluatedCount(tUser), TeamTime.GetUserTotalCount(tUser), sTime);
                            }
                        }
                        Dictionary<string, string> tParams = default(Dictionary<string, string>);
                        var IsOffline = (bool) context.Session["offline"];
                        var ISSingleLine = (bool)context.Session["single_line"];
                        tParams = null;
                        sData = $@"'active' : {(isTeamTime ? 1 : 0)} , 
                            'progress' : {{ 'currentStep' : {TeamTimePipeStep}, 'totalSteps' : {TeamTime.Pipe.Count} , 
                                            'evaluationProgress' : [{sEvalProgress}] 
                                        }} , 
                            'options' : {{ 'refreshRate' : {TeamTimeRefreshTimeout} , 'showComments' : {(TeamTimePipeParams.ShowComments ? 1 : 0)} ,
                                            'showInfodocs' : {(TeamTimePipeParams.ShowInfoDocs ? Convert.ToInt32(TeamTimePipeParams.ShowInfoDocsMode) : -1)} ,
                                            'hideJudgments' : {(TeamTimePipeParams.SynchStartInPollingMode ? 1 : 0)} , 'isAnonymous' : {(TeamTimePipeParams.TeamTimeStartInAnonymousMode ? 1 : 0)} ,
                                            'useKeypads' : {(TeamTimePipeParams.SynchUseVotingBoxes ? 1 : 0)} , 'sortOrder' : {Convert.ToInt32(TeamTimePipeParams.TeamTimeUsersSorting)} ,
                                            'showVarianceInPoling' : {(TeamTimeShowVarianceInPollingMode ? 1 : 0)} , 
                                            'showPWSideForKeypadsInPollingMode' : {(TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess ? 1 : 0)} ,
                                            'sessionOwnerUserID' : {(TeamTimePipeParams.TeamTimeHideProjectOwner ? 1 : 0)} , 'hideOfflineUsers' : {Convert.ToInt32(IsOffline)} ,
                                            'singleLineMode' : {Convert.ToInt32(ISSingleLine)} , 'userDisplay' : {userdisplayname} , 'hidePie' : {HidePieChart}
                                        }},
                            'data' : {{ 'type' : '{sType}' , 'task' : '{StringFuncs.JS_SafeString(PrepareTask(sTask, null, false, tParams))}' ,
                                        'stepGuid' : '{tAction.StepGuid.ToString()}' , 'data' : {sValue}
                                    }}
                            ";

                        //string.Format("['active',{0}],['progress',{1},{2},[{3}]],['options',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}],['data','{17}','{18}','{19}',{20}}}]",
                        //        (isTeamTime ? 1 : 0), TeamTimePipeStep, TeamTime.Pipe.Count, sEvalProgress, TeamTimeRefreshTimeout,
                        //        (TeamTimePipeParams.ShowComments ? 1 : 0), (TeamTimePipeParams.ShowInfoDocs ? Convert.ToInt32(TeamTimePipeParams.ShowInfoDocsMode) : -1),
                        //        (TeamTimePipeParams.SynchStartInPollingMode ? 1 : 0), (TeamTimePipeParams.TeamTimeStartInAnonymousMode ? 1 : 0),
                        //        (TeamTimePipeParams.SynchUseVotingBoxes ? 1 : 0), Convert.ToInt32(TeamTimePipeParams.TeamTimeUsersSorting), (TeamTimeShowVarianceInPollingMode ? 1 : 0),
                        //        (TeamTimeShowPWSideKeypadsInPollingMode ? 1 : 0), (TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess ? 1 : 0),
                        //        (TeamTimePipeParams.TeamTimeHideProjectOwner ? 1 : 0), Convert.ToInt32(IsOffline), Convert.ToInt32(ISSingleLine), sType, StringFuncs.JS_SafeString(PrepareTask(sTask, null, false, tParams)), tAction.StepGuid.ToString(), sValue);
                        App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, "{" + sCallbackSave + sData + "}");
                        sCallbackSave = "";
                    }
                    else
                    {
                        sData = $"'action' : 'refresh' , ";
                    }
                }
                else
                {
                    bool fProjectAvailable = !(App.ActiveProject.LockInfo != null && App.ActiveProject.LockInfo.LockStatus == ECLockStatus.lsLockForModify);
                    string sMsg = "";

                    if (fProjectAvailable)
                    {
                        // Check date time for last session data
                        // D1275 ===
                        Nullable<DateTime> DT = null;
                        sData = App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, ref  DT);
                        // D1280
                        if (!DT.HasValue || DT.Value.AddSeconds(TeamTimeRefreshTimeout * _TeamTimeSessionDataTimeoutCoeff) < DateTime.Now)
                            sData = "";
                        // D1280
                        // D1275 ==

                        if (string.IsNullOrEmpty(sData))
                            sMsg = App.ResString("msgSynchronousWaitOwner");
                        // D1280
                    }
                    else
                    {
                        sMsg = "Project is temporary locked for update. Please wait session owner…";
                    }
                    if (string.IsNullOrEmpty(sData) && !string.IsNullOrEmpty(sMsg))
                        sData = $"'warning' : '{StringFuncs.JS_SafeString(sMsg)}' , ";
                }

                if (App.ActiveUser.Session != null)
                {
                    if (App.ActiveUser.Session.LastAccess.HasValue && App.ActiveUser.Session.LastAccess.Value.AddSeconds(Consts._DEF_SESS_TIMEOUT / 3) < DateTime.Now)
                    {
                        App.ActiveUser.Session.LastAccess = DateTime.Now;
                        App.DBUpdateDateTime(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_LASTVISITED, App.ActiveUser.UserID, clsComparionCore._FLD_USERS_ID);
                    }
                }

                clsAction tActions = GetAction(TeamTimePipeStep);
                DateTime Dts = DateTime.Now;
                List<string> tRowss = App.DBTeamTimeDataReadAll(App.ProjectID, ecExtraProperty.TeamTimeJudgment);
                if (tRowss != null)
                {
                    foreach (string sRow in tRowss)
                    {
                        var s = TeamTime.ParseStringFromClient(sRow);

                        string[] sParams = sRow.Split('\t');
                        if (sParams.Count() > 4)
                            sCallbackSave += String.Format("{0}'{1}'", (sCallbackSave.Equals("") ? "" : ",").ToString(), sParams[4].Trim());
                    }
                    if (tRowss.Count > 0) { App.DBTeamTimeDataDelete(App.ProjectID, Dts, ecExtraProperty.TeamTimeJudgment); }
                    if (sCallbackSave != "") { sCallbackSave = $"'pended' : [{sCallbackSave}] , "; }
                }

            }
            else
            {
                sData = String.Format("['active',0],['warning','{0}']", GetStartupMessage());
            }

            return sData;
        }

        public static int TeamTimePipeStep
        {

            get
            {
                HttpContext context = HttpContext.Current;

                var App = (clsComparionCore)context.Session["App"];
                var CurrentStep = _TeamTimePipeStep;

                if(isTeamTimeOwner)
                { 
                    if (context.Session[_SESS_TT_Step] != null)
                    {
                        _TeamTimePipeStep = (int)context.Session[_SESS_TT_Step];
                    }
                }
                else
                {
                    context.Session[_SESS_TT_Step] = _TeamTimePipeStep;
                }
                //else
                //{
                //    CurrentStep = -1;
                //}
                if (_TeamTimePipeStep < 1 || context.Session[_SESS_TT_Step] == null)
                {
                    if (isTeamTimeOwner)
                    {
                        _TeamTimePipeStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
                        context.Session[_SESS_TT_Step] = _TeamTimePipeStep;
                        // D1945
                        // D1289 ===
                        if (_TeamTimePipeStep < 1)
                            _TeamTimePipeStep = 1;
                        if (_TeamTimePipeStep > TeamTime.Pipe.Count)
                            _TeamTimePipeStep = TeamTime.Pipe.Count;
                        if (_TeamTimePipeStep != App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact))
                            App.ActiveWorkspace.set_ProjectStep(App.ActiveProject.isImpact, _TeamTimePipeStep);
                        // D1945
                        // D1289 ==
                        //TeamTimeUpdateStepInfo(_TeamTimePipeStep)   ' D1348
                        TeamTime.SetCurrentStep(_TeamTimePipeStep - 1, TeamTimeUsersList);
                        // D1270
                        // D1353 ===
                        if (_ForceSaveStepInfo)
                        {
                            TeamTimeUpdateStepInfo(_TeamTimePipeStep);
                            _ForceSaveStepInfo = false;
                        }
                        // D1353 ==
                    }
                    else
                    {
                        clsWorkspace tWS = App.DBWorkspaceByUserIDProjectID(App.ActiveProject.MeetingOwnerID, App.ProjectID);
                        if (tWS != null)
                            _TeamTimePipeStep = tWS.get_ProjectStep(App.ActiveProject.isImpact);
                        // D1945
                    }
                    if (_TeamTimePipeStep < 1)
                        _TeamTimePipeStep = 1;
                }
                context.Session[_SESS_TT_Step] = _TeamTimePipeStep;
                return _TeamTimePipeStep;
            }
            set
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                if (isTeamTimeOwner && _TeamTimePipeStep != value)
                {
                    _TeamTimePipeStep = value;
                    context.Session[_SESS_TT_Step] = _TeamTimePipeStep;
                    App.ActiveWorkspace.set_ProjectStep(App.ActiveProject.isImpact, value);
                    TeamTimeUpdateStepInfo(_TeamTimePipeStep);

                }
            }
        }

        public static void TeamTimeUpdateStepInfo(int StepID)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];

            TeamTime.SetCurrentStep(StepID - 1, TeamTimeUsersList);
            clsAction tAction = GetAction(TeamTimePipeStep);
            if (tAction != null)
            {
                String sExtra = "";
                if (tAction.ActionType == ActionType.atNonPWOneAtATime && tAction.ActionData.GetType() == typeof(clsOneAtATimeEvaluationActionData))
                {
                    clsOneAtATimeEvaluationActionData tActionData = (clsOneAtATimeEvaluationActionData)tAction.ActionData;
                    if (tActionData.MeasurementType == ECMeasureType.mtRatings)
                    {
                        clsRatingScale RS = (clsRatingScale)tActionData.Node.MeasurementScale;
                        for (int i = RS.RatingSet.Count - 1; i >= 0; i--)
                        {
                            clsRating intensity = (clsRating)RS.RatingSet[i];
                            sExtra += sExtra.Equals("") ? "" : "," + intensity.ID.ToString();
                        }
                    }
                }
                String sStepData = String.Format("{1}{0}{2}{0}{3}{0}{4}", clsTeamTimePipe.Judgment_Delimeter, StepID - 1, Convert.ToInt32(tAction.ActionType), tAction.StepGuid, sExtra);
                App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeStepData, sStepData);
            }
            else
            {
                App.DBTeamTimeDataDelete(App.ProjectID, null, ecExtraProperty.TeamTimeStepData);
            }
        }


        public static bool isImpact
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                if (isTeamTimeOwner) { return TeamTime.ProjectManager.ActiveHierarchy.Equals(ECTypes.ECHierarchyID.hidImpact); }
                else { return App.ActiveProject.ProjectManager.ActiveHierarchy.Equals(ECTypes.ECHierarchyID.hidImpact); }
            }
        }

        public static string GetDateTimeTicks(DateTime Dt)
        {
            return Dt.ToString("yyyy'/'MM'/'dd HH':'mm':'ss");
        }

        public static int TeamTimeRefreshTimeout
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                if (!_TeamTimeRefreshTimeoutLoaded)
                {

                    Nullable<DateTime> DT = default(Nullable<DateTime>);
                    string sRefresh = App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.RefreshTimeout, ref DT);
                    if (!int.TryParse(sRefresh, out _TeamTimeRefreshTimeout))
                        _TeamTimeRefreshTimeout = ECWeb.WebOptions.SynchronousRefresh();

                    if (_TeamTimeRefreshTimeout < TeamTimeMinimumRefreshTimeout)
                        _TeamTimeRefreshTimeout = TeamTimeMinimumRefreshTimeout;
                    _TeamTimeRefreshTimeoutLoaded = true;
                }
                return _TeamTimeRefreshTimeout;
            }
            set
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                if (value < 1)
                    value = 1;
                if (value > 30)
                    value = 30;
                if (value != _TeamTimeRefreshTimeout && isTeamTimeOwner)
                {
                    _TeamTimeRefreshTimeout = value;
                    App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.RefreshTimeout, Convert.ToString(value));
                    _TeamTimeRefreshTimeoutLoaded = true;
                }
            }
        }


        public static int TeamTimeMinimumRefreshTimeout
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                if (isTeamTimeOwner && TeamTimeUsersList != null && TeamTimeUsersList.Count > 0)
                { return (int)Math.Min(Math.Ceiling((double)TeamTimeUsersList.Count / 20), 5); }
                return 1;
            }
        }

        public static bool TeamTimeShowVarianceInPollingMode
        {
            get { return get_SessVar(_SESS_VARIANCE) == "1"; }
            set { set_SessVar(_SESS_VARIANCE, (value ? "1" : "0")); }
        }

        public static bool TeamTimeShowPWSideKeypadsInPollingMode
        {
            get { return get_SessVar(_SESS_PW_SIDE) == "1"; }
            set { set_SessVar(_SESS_PW_SIDE, (value ? "1" : "0")); }
        }

        public static bool TeamTimeHideCombined
        {
            get { return get_SessVar(_SESS_HIDE_COMBINED) == "1"; }
            set { set_SessVar(_SESS_HIDE_COMBINED, (value ? "1" : "0")); }
        }

        public static string GetStartupMessage()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            string sMsg = "";
            // D1275
            if (_TeamTimeActive)
            {
                if (!isTeamTime | (!isTeamTimeOwner && !isTeamTimeOwnerOnline))
                {
                    sMsg = App.ResString("msgSynchronousWaitOwner");
                }
                else
                {
                    sMsg = "Loading. Please wait…";
                }
            }
            else
            {
                sMsg = "<span class='error'>TeamTime can't work on that instance due to have wrong MasterDB version.<br>Please contact with System Administrator for upgrade database.</span>";
                // D1275
            }
            //ShowWarning("TeamTime not started yet. Go to <a href='Default.aspx' class='actions'>AnyTime evaluation</a>.")
            return sMsg;
            // D1254
        }


        public static bool isTeamTimeOwnerOnline
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                bool fAllowed = false;
                if ((App.ActiveProject.MeetingOwner != null))
                {
                    fAllowed = (clsOnlineUserSession.OnlineSessionByUserIDProjectID(App.ActiveProject.MeetingOwnerID, App.ProjectID, SessionsList) != null);
                }
                return fAllowed;
            }
        }

        public static string get_SessVar(string sName)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            if (context.Session[sName] == null)
                return null;
            else
                return Convert.ToString(context.Session[sName]);
        }
        public static void set_SessVar(string sName, string value)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            if (value == null & (context.Session[sName] != null))
            {
                context.Session.Remove(sName);
            }
            else
            {
                if (context.Session[sName] == null)
                    context.Session.Add(sName, value);
                else
                    context.Session[sName] = value;
            }
        }

        public static string PrepareTask(string sTask, object tExtraParam = null, bool fHasSubNodes = false, Dictionary<string, string> tUsedParams = null)
        {
            // D1862 + D2364
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];

            //language

            return PrepareTaskCommon(App, sTask, tExtraParam, fHasSubNodes);
        }


        public static string PrepareTaskCommon(clsComparionCore tApp, string sTask, object tExtraParam = null, bool fHasSubNodes = false, Dictionary<string, string> tUsedParams = null)
        {
            // D1862
            string sObjectives = "";
            string sObjective = "";
            string sAlternatives = "";
            string sAlternative = "";
            string sPromtObj = "";
            // D1819
            string sPromtAlt = "";
            // D1819
            if (sTask != null && tApp != null)
            {
                bool fIsImpact = false;
                // D2141
                // D3314
                if (tApp.ActiveProject != null && tApp.ActiveProject.isValidDBVersion)
                {

                    // D0237 ===
                    var _with1 = tApp.ActiveProject.ProjectManager.PipeParameters;

                    PipeParameters.ParameterSet OldParams = _with1.CurrentParameterSet;

                    string sHid = "";
                    if (tApp.ActiveProject.ProjectManager.ActiveHierarchy == Convert.ToInt32(ECTypes.ECHierarchyID.hidImpact))
                    {
                        sHid = "_Impact";
                        // D2292
                        if (!object.ReferenceEquals(_with1.CurrentParameterSet, _with1.ImpactParameterSet))
                            _with1.CurrentParameterSet = _with1.ImpactParameterSet;
                        fIsImpact = true;
                        // D2141
                    }
                    if (tApp.ActiveProject.ProjectManager.ActiveHierarchy == Convert.ToInt32(ECTypes.ECHierarchyID.hidLikelihood))
                    {
                        sHid = "_Likelihood";
                        // D2292
                        if (!object.ReferenceEquals(_with1.CurrentParameterSet, _with1.DefaultParameterSet))
                            _with1.CurrentParameterSet = _with1.DefaultParameterSet;
                    }
                    // D2037 ==

                    sAlternatives = _with1.NameAlternatives;
                    sObjectives = _with1.NameObjectives;

                    // D1819 ===
                    if (!string.IsNullOrEmpty(_with1.JudgementPromt))
                    {
                        sPromtObj = _with1.JudgementPromt;
                    }
                    else
                    {
                        // D1822 ===
                        string sTempl = "lbl_promt_obj{1}_{0}";
                        // D1822 + D2292
                        bool flag = false;
                        sPromtObj = tApp.CurrentLanguage.GetString(string.Format(sTempl, (_with1.JudgementPromtID < 0 ? 0 : _with1.JudgementPromtID), sHid), "", ref flag);
                        if (string.IsNullOrEmpty(sPromtObj))
                            sPromtObj = tApp.ResString(string.Format(sTempl, 0, sHid));
                        // D1822 ==
                    }

                    if (!string.IsNullOrEmpty(_with1.JudgementAltsPromt))
                    {
                        sPromtAlt = _with1.JudgementAltsPromt;
                    }
                    else
                    {
                        // D1822 ===
                        string sTempl = "lbl_promt_alt{1}_{0}";
                        // D2292
                        bool flag = false;
                        sPromtAlt = tApp.CurrentLanguage.GetString(string.Format(sTempl, (_with1.JudgementAltsPromtID < 0 ? 0 : _with1.JudgementAltsPromtID), sHid), "", ref flag);
                        if (string.IsNullOrEmpty(sPromtAlt))
                            sPromtAlt = tApp.ResString(string.Format(sTempl, 0, sHid));
                        // D1822 ==
                    }

                    _with1.CurrentParameterSet = OldParams;
                    // D2037

                    // D1819 ==

                }

                // D2427 ===
                if (tApp.isRiskEnabled && tApp.ActiveWorkgroup != null && tApp.ActiveWorkgroup.WordingTemplates != null)
                {
                    if (string.IsNullOrEmpty(sAlternatives) && tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(Consts._TPL_RISK_EVENTS))
                        sAlternatives = tApp.ActiveWorkgroup.WordingTemplates[Consts._TPL_RISK_EVENTS];
                    if (string.IsNullOrEmpty(sAlternative) && tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(Consts._TPL_RISK_EVENT))
                        sAlternative = tApp.ActiveWorkgroup.WordingTemplates[Consts._TPL_RISK_EVENT];
                    string sObjName = Convert.ToString((fIsImpact ? Consts._TPL_RISK_CONSEQUENCES : Consts._TPL_RISK_SOURCES));
                    // D2464
                    if (string.IsNullOrEmpty(sObjectives) && tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(sObjName))
                        sObjectives = tApp.ActiveWorkgroup.WordingTemplates[sObjName];
                    sObjName = Convert.ToString((fIsImpact ? Consts._TPL_RISK_CONSEQUENCE : Consts._TPL_RISK_SOURCE));
                    // D2464
                    if (string.IsNullOrEmpty(sObjective) && tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(sObjName))
                        sObjective = tApp.ActiveWorkgroup.WordingTemplates[sObjName];
                }
                // D2427 ==

                // D2258 ===
                if (string.IsNullOrEmpty(sAlternatives))
                    sAlternatives = tApp.ResString(Convert.ToString((tApp.isRiskEnabled ? "templ_AlternativesRisk" : "templ_Alternatives")));
                if (string.IsNullOrEmpty(sAlternative))
                    sAlternative = tApp.ResString(Convert.ToString((tApp.isRiskEnabled ? "templ_AlternativeRisk" : "templ_Alternative")));
                if (string.IsNullOrEmpty(sObjectives) || fHasSubNodes)
                    sObjectives = tApp.ResString(Convert.ToString((tApp.isRiskEnabled ? (fIsImpact ? "templ_Objectives_Impact" : "templ_ObjectivesRisk") : "templ_Objectives")));
                // D1770 + D2141
                if (string.IsNullOrEmpty(sObjective) || fHasSubNodes)
                    sObjective = tApp.ResString(Convert.ToString((tApp.isRiskEnabled ? (fIsImpact ? "templ_Objective_Impact" : "templ_ObjectiveRisk") : "templ_Objective")));
                // D1770 + D2141
                // D2258 ==

                // D1826 + D2071 ===
                if (tExtraParam != null && ((tExtraParam) is clsStepFunction || (tExtraParam) is clsRatingScale))
                {
                    if ((tExtraParam) is clsStepFunction)
                    {
                        sObjective = tApp.ResString("templ_Interval");
                        sObjectives = tApp.ResString("templ_Intervals");
                    }
                    else
                    {
                        sObjective = tApp.ResString("templ_Intensity");
                        sObjectives = tApp.ResString("templ_Intensities");
                        // D1862 ===
                    }
                    // D2071 ==
                    sAlternative = sObjective;
                    sAlternatives = sObjectives;
                    // D1862 ==
                }
                // D1826 ==

                Dictionary<string, string> tParams = new Dictionary<string, string>();
                if (tUsedParams != null)
                {
                    tParams = tUsedParams;
                }

            
                tParams.Add(ECWeb.Options._TEMPL_ALTERNATIVES, StringFuncs.JS_SafeHTML(sAlternatives));
                tParams.Add(ECWeb.Options._TEMPL_ALTERNATIVE, StringFuncs.JS_SafeHTML(sAlternative));
                tParams.Add(ECWeb.Options._TEMPL_OBJECTIVES, StringFuncs.JS_SafeHTML(sObjectives));
                tParams.Add(ECWeb.Options._TEMPL_OBJECTIVE, StringFuncs.JS_SafeHTML(sObjective));
                // D2457
                if (sTask.ToLower().Contains("%%promt_"))
                {
                    tParams.Add(ECWeb.Options._TEMPL_PROMT_OBJ, sPromtObj);
                    // D1819
                    tParams.Add(ECWeb.Options._TEMPL_PROMT_ALT, sPromtAlt);
                    // D1819
                    tParams.Add(ECWeb.Options._TEMPL_PROMT_ALT_WORD, ECWeb.clsComparionCorePage.GetPromptWordCommon(tApp, true));
                    // D2457
                    tParams.Add(ECWeb.Options._TEMPL_PROMT_OBJ_WORD, ECWeb.clsComparionCorePage.GetPromptWordCommon(tApp, false));
                    // D2457
                }
                // D2320 ===
                if (sTask.Contains("wording%%"))
                {
                    tParams.Add(ECWeb.Options._TEMPL_RATE_WORDING, ECWeb.clsComparionCorePage.GetRatingAltsWordingCommon(tApp, true));
                    tParams.Add(ECWeb.Options._TEMPL_RATE_OBJ_WORDING, ECWeb.clsComparionCorePage.GetRatingObjWordingCommon(tApp, true));
                    // D2449
                    tParams.Add(ECWeb.Options._TEMPL_EST_WORDING, ECWeb.clsComparionCorePage.GetRatingAltsWordingCommon(tApp, false));
                    tParams.Add(ECWeb.Options._TEMPL_EST_OBJ_WORDING, ECWeb.clsComparionCorePage.GetRatingObjWordingCommon(tApp, false));
                    // D2449
                }
                // D2320 ==

                sTask = StringFuncs.ParseStringTemplates(sTask, tParams);

                //If tUsedParams IsNot Nothing Then tUsedParams = tParams ' D2364
                // D2429 ===
                if (sTask.Contains("%%") && tApp.isRiskEnabled && tApp.ActiveWorkgroup != null && tApp.ActiveWorkgroup.WordingTemplates != null)
                {
                    foreach (string sName in tApp.ActiveWorkgroup.WordingTemplates.Keys)
                    {


                        if (sTask.ToLower().Contains(sName.ToLower()))
                        {
                            sTask = ParseTemplateCommon(sTask, sName, tApp.ActiveWorkgroup.WordingTemplates[sName], false);
                        }
                    }
                }
                // D2429 ==
            }
            return sTask;
        }

        public static clsAction GetAction(int stepNumber)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var pipeCount = TeamTime == null ? 0 : TeamTime.Pipe.Count;
            if (stepNumber < 1 | stepNumber > pipeCount | pipeCount < 1)
                return null;
            else
                return (clsAction)TeamTime.Pipe[stepNumber - 1];
        }

        public static string getTeamTimeData()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            Nullable<DateTime> DT = null;
            var sdata = "";
            try
            {
                sdata = App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, ref  DT);
            }
            catch (Exception e)
            {
               
            }

            return sdata;
        }

        public static List<object> getnodes(int step)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var Project = (clsTeamTimePipe)TeamTime;
            clsAction tAction = GetAction(step);
            var ParentNode = (clsNode)null;
            var FirstNode = (clsNode)null;
            var SecondNode = (clsNode)null;
            var FirstRating = (clsNode)null;
            var SecondRating = (clsNode)null;
            var ChildNode = (clsNode)null;
            var ObjHierarchy = (clsHierarchy)Project?.ProjectManager.get_Hierarchy(Project.ProjectManager.ActiveHierarchy);
            var AltsHierarchy = (clsHierarchy)Project?.ProjectManager.get_AltsHierarchy(Project.ProjectManager.ActiveAltsHierarchy);
            var wrtleft = "";
            var wrtright = "";
            var leftnodeinfo = "";
            var rightnodeinfo = "";
            var childinfo = "";
            var parentinfo = "";
            var wrtinfo = "";
            var ParentNodeInfo = "";
            var list1 = new List<object>();


            var path = context.Server.MapPath("~/");
            Consts._FILE_ROOT = context.Server.MapPath("~/");
            Consts._FILE_MHT_FILES = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, "DocMedia/MHTFiles/"));    // D3411; 

            if (tAction != null)
            {
                switch (tAction.ActionType)
                {
                    case ActionType.atInformationPage:
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        break;
                    case ActionType.atPairwise:
                        var ActionData = (clsPairwiseMeasureData)tAction.ActionData;
                        ParentNode = ObjHierarchy.GetNodeByID(ActionData.ParentNodeID);
                        if (ParentNode != null)
                        {
                            if (ParentNode.IsTerminalNode)
                            {
                                FirstNode = AltsHierarchy.GetNodeByID(ActionData.FirstNodeID);
                                SecondNode = AltsHierarchy.GetNodeByID(ActionData.SecondNodeID);
                            }
                            else
                            {
                                FirstNode = ObjHierarchy.GetNodeByID(ActionData.FirstNodeID);
                                SecondNode = ObjHierarchy.GetNodeByID(ActionData.SecondNodeID);
                            }
                        }

                        //ParentNodeInfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, isTeamTimeOwner, true, -1);
                        //leftnodeinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, FirstNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, FirstNode.NodeID.ToString(), FirstNode.InfoDoc, isTeamTimeOwner, true, -1);
                        //rightnodeinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, SecondNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, SecondNode.NodeID.ToString(), SecondNode.InfoDoc, isTeamTimeOwner, true, -1);
                        //wrtleft = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, FirstNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(FirstNode.NodeGuidID, ParentNode.NodeGuidID), isTeamTimeOwner, true, ParentNode.NodeID);
                        //wrtright = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, SecondNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(SecondNode.NodeGuidID, ParentNode.NodeGuidID), isTeamTimeOwner, true, ParentNode.NodeID);

                        list1.Add(leftnodeinfo);
                        list1.Add(rightnodeinfo);
                        list1.Add(wrtleft);
                        list1.Add(wrtright);
                        list1.Add(FirstNode);
                        list1.Add(SecondNode);
                        list1.Add(ParentNode);
                        list1.Add(ParentNodeInfo);
                        break;
                    case ActionType.atShowLocalResults:
                        clsShowLocalResultsActionData data = (clsShowLocalResultsActionData)tAction.ActionData;


                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add(data.ParentNode);
                        list1.Add("");
                        list1.Add(data.ParentNode);
                        list1.Add("");
                        break;
                    case ActionType.atShowGlobalResults:
                        var Nodes = ObjHierarchy.Nodes;

                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add(Nodes[0]);
                        list1.Add("");
                        list1.Add(Nodes[0]);
                        list1.Add("");
                        break;
                    case ActionType.atSensitivityAnalysis:
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add(tAction.ParentNode);
                        list1.Add("");
                        break;
                    case ActionType.atNonPWOneAtATime:
                        var ActionData2 = (clsOneAtATimeEvaluationActionData)tAction.ActionData;

                        if (ActionData2.Node != null && ActionData2.Judgment != null)
                        {
                            switch (((clsNonPairwiseEvaluationActionData)tAction.ActionData).MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                    clsNonPairwiseMeasureData nonpwdata = (clsNonPairwiseMeasureData)ActionData2.Judgment;
                                    ParentNode = ActionData2.Node;
                                    if (ActionData2.Node.IsAlternative)
                                    {
                                        ParentNode = ObjHierarchy.Nodes[0];
                                        ChildNode = ActionData2.Node;
                                    }
                                    else
                                    {
                                        ParentNode = ActionData2.Node;
                                        if (ParentNode != null && ParentNode.IsTerminalNode) { ChildNode = AltsHierarchy.GetNodeByID(nonpwdata.NodeID); } else { ChildNode = ObjHierarchy.GetNodeByID(nonpwdata.NodeID); }
                                    }
                                    //childinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ChildNode.NodeID.ToString(), ChildNode.InfoDoc, isTeamTimeOwner, true, -1);
                                    //parentinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, isTeamTimeOwner, true, -1);
                                    //wrtinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID), isTeamTimeOwner, true, ParentNode.NodeID);
                                    list1.Add(childinfo);
                                    list1.Add(parentinfo);
                                    list1.Add(wrtinfo);
                                    list1.Add("");
                                    list1.Add(ParentNode);
                                    list1.Add(ChildNode);
                                    list1.Add(ParentNode);
                                    list1.Add("");

                                    break;
                                //leftnodeinfo == (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Convert.ToInt32((tAlt.IsAlternative ? reObjectType.Alternative : reObjectType.Node)), tAlt.NodeID.ToString(), tAlt.InfoDoc, true, true, -1);

                                case ECMeasureType.mtDirect: ;
                                    ParentNode = ActionData2.Node;
                                    var DirectData = (clsDirectMeasureData)ActionData2.Judgment;
                                    if (ParentNode.IsTerminalNode) { 
                                        ChildNode = AltsHierarchy.GetNodeByID(DirectData.NodeID); 
                                    } else { 
                                        ChildNode = ObjHierarchy.GetNodeByID(DirectData.NodeID); 
                                    }
                                    //childinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ChildNode.NodeID.ToString(), ChildNode.InfoDoc, isTeamTimeOwner, true, -1);
                                    //parentinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, isTeamTimeOwner, true, -1);
                                    //wrtinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID), isTeamTimeOwner, true, ParentNode.NodeID);
                                    list1.Add(childinfo);
                                    list1.Add(parentinfo);
                                    list1.Add(wrtinfo);
                                    list1.Add("");
                                    list1.Add(ParentNode);
                                    list1.Add(ChildNode);
                                    list1.Add(ParentNode);
                                    list1.Add("");
                                    break;

                                case ECMeasureType.mtStep:
                                    ParentNode = ActionData2.Node;
                                    var StepData = (clsStepMeasureData)ActionData2.Judgment;
                                    if (ParentNode.IsTerminalNode) { ChildNode = AltsHierarchy.GetNodeByID(StepData.NodeID); } else { ChildNode = ObjHierarchy.GetNodeByID(StepData.NodeID); }
                                    childinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ChildNode.NodeID.ToString(), ChildNode.InfoDoc, true, false, -1);
                                    parentinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, true, false, -1);
                                    wrtinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID), false, true, ParentNode.NodeID);
                                    list1.Add(childinfo);
                                    list1.Add(parentinfo);
                                    list1.Add(wrtinfo);
                                    list1.Add("");
                                    list1.Add(ParentNode);
                                    list1.Add(ChildNode);
                                    list1.Add(ParentNode);
                                    list1.Add("");
                                    break;

                                case ECMeasureType.mtRegularUtilityCurve:
                                    ParentNode = ActionData2.Node;
                                    var UCData = (clsUtilityCurveMeasureData)ActionData2.Judgment;
                                    if (ParentNode.IsTerminalNode) { ChildNode = AltsHierarchy.GetNodeByID(UCData.NodeID); } else { ChildNode = ObjHierarchy.GetNodeByID(UCData.NodeID); };
                                    childinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ChildNode.NodeID.ToString(), ChildNode.InfoDoc, true, false, -1);
                                    parentinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, true, false, -1);
                                    wrtinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID), false, true, ParentNode.NodeID);
                                    list1.Add(childinfo);
                                    list1.Add(parentinfo);
                                    list1.Add(wrtinfo);
                                    list1.Add("");
                                    list1.Add(ParentNode);
                                    list1.Add(ChildNode);
                                    list1.Add(ParentNode);
                                    list1.Add("");
                                    break;
                            }

                        }
                        break;


                    default:
                        break;

                }
            }
            return list1;
        }


        public static string GetPipeStepTask(clsAction Action, object tExtraParam, bool fHasSubnodes = false, bool fIgnoreClusterPhrase = false, bool fCanBePathInteractive = true, bool fParseNodeNames = true)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            string sRes = "";
            Dictionary<string, string> Params = new Dictionary<string, string>();
            bool fGetResString = true;
            clsNode tClusterNode = null;

            if (App.HasActiveProject() && Action != null && Action.ActionData != null)
            {
                clsHierarchy Hierarchy = App.ActiveProject.HierarchyObjectives;
                bool isImpact = App.ActiveProject.ProjectManager.ActiveHierarchy == Convert.ToInt16(ECTypes.ECHierarchyID.hidImpact);
                bool IsRiskWithControls = false;
                // D2503

                switch (Action.ActionType)
                {

                    case ActionType.atPairwise:
                    case ActionType.atPairwiseOutcomes:
                        clsPairwiseMeasureData Data = (clsPairwiseMeasureData)Action.ActionData;
                        clsNode parentNode = null;
                        switch (Action.ActionType)
                        {
                            case ActionType.atPairwise:
                                parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(Data.ParentNodeID);
                                Hierarchy = (clsHierarchy)(parentNode.IsTerminalNode ? App.ActiveProject.HierarchyAlternatives : App.ActiveProject.HierarchyObjectives);
                                break;
                            case ActionType.atPairwiseOutcomes:
                                parentNode = Action.ParentNode;
                                break;
                        }
                        if (App.isRiskEnabled)
                        {
                            // D2732 ===
                            if (parentNode.RiskNodeType == ECTypes.RiskNodeType.ntCategory)
                            {
                                sRes = Convert.ToString((parentNode.IsTerminalNode ? "task_Pairwise_Alternatives_Category" : "task_Pairwise_Objectives_Category"));
                            }
                            else
                            {
                                // D2732 ==
                                if (parentNode.IsTerminalNode)
                                {
                                    sRes = Convert.ToString((parentNode.Level == 0 ? "task_Pairwise_AlternativesNoObj" : (isImpact ? "task_Pairwise_Alternatives" : "task_Pairwise_AlternativesLikelihood")));
                                    // D2374 + D2714
                                }
                                else
                                {
                                    if (parentNode.get_ParentNode() == null)
                                    {
                                        sRes = Convert.ToString((tExtraParam != null ? "task_Pairwise_ObjectivesIntensities" : "task_Pairwise_ObjectivesGoal"));
                                    }
                                    else
                                    {
                                        sRes = Convert.ToString((isImpact ? "task_Pairwise_Objectives" : "task_Pairwise_ObjectivesLikelihood"));
                                        // D2718
                                    }
                                }
                            }
                        }
                        else
                        {
                            sRes = Convert.ToString((parentNode.IsTerminalNode ? "task_Pairwise_Alternatives" : "task_Pairwise_Objectives"));
                        }

                        clsNode tNodeLeft = new clsNode();
                        clsNode tNodeRight = new clsNode();
                        bool fIsPWOutcomes = Action.ActionType == ActionType.atPairwiseOutcomes;
                        // D2351
                        if (fIsPWOutcomes && parentNode != null)
                        {
                            clsRatingScale tRS = (clsRatingScale)Action.PWONode.MeasurementScale;
                            sRes = Convert.ToString((parentNode.IsAlternative ? (Action.PWONode.get_ParentNode() == null ? "task_PairwiseOutcomesAltGoal" : "task_PairwiseOutcomesAlt") : (Action.PWONode == null ? "task_PairwiseOutcomesGoal" : "task_PairwiseOutcomes")));
                            // D2318 + D2351 + D2410 + D2438
                            if (parentNode.Level > 1)
                                sRes = "task_PairwiseOutcomesLevels";
                            if (tRS.IsPWofPercentages)
                                sRes = Convert.ToString((parentNode.get_ParentNode() == null ? "task_PairwiseOfPercentagesGoal" : "task_PairwiseOfPercentages"));
                            if (tRS.IsExpectedValues)
                                sRes = Convert.ToString((parentNode.get_ParentNode() == null ? "task_PairwiseExpectedValuesGoal" : "task_PairwiseExpectedValues"));
                            App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(Action, Data, ref tNodeLeft, ref tNodeRight);
                            Params.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.PWONode, fCanBePathInteractive));
                            // D2830
                        }
                        else
                        {
                            if (Hierarchy != null)
                            {
                                tNodeLeft = Hierarchy.GetNodeByID(Data.FirstNodeID);
                                tNodeRight = Hierarchy.GetNodeByID(Data.SecondNodeID);
                            }
                        }
                        if (parentNode != null)
                            Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(parentNode, fCanBePathInteractive));
                        // D2830
                        if (tNodeLeft != null)
                            Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNodeLeft.NodeName));
                        if (tNodeRight != null)
                            Params.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(tNodeRight.NodeName));
                        tClusterNode = parentNode;
                        // D2364
                        break;

                    case ActionType.atNonPWOneAtATime:
                        clsOneAtATimeEvaluationActionData data = (clsOneAtATimeEvaluationActionData)Action.ActionData;
                        if ((data != null))
                        {
                            // D2503 ===
                            if (IsRiskWithControls)
                            {
                                if (data != null && data.Assignment != null && data.Control != null)
                                {
                                    Params.Add(ECWeb.Options._TEMPL_NODENAME, data.Control.Name);
                                    clsNode tNode = null;
                                    clsNode WRT = null;

                                    switch (data.Control.Type)
                                    {
                                        case ControlType.ctCause:
                                            if (!Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty))
                                            {
                                                tNode = App.ActiveProject.ProjectManager.get_Hierarchy(Convert.ToInt16(ECTypes.ECHierarchyID.hidLikelihood)).GetNodeByID(data.Assignment.ObjectiveID);
                                                if (tNode != null)
                                                    Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNode.NodeName));
                                                WRT = tNode;
                                                tNode = App.ActiveProject.ProjectManager.get_AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID);
                                                if (tNode != null)
                                                    Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(tNode.NodeName));
                                            }
                                            break;
                                        case ControlType.ctCauseToEvent:
                                            if (!Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty))
                                            {
                                                tNode = App.ActiveProject.ProjectManager.get_Hierarchy(Convert.ToInt16(ECTypes.ECHierarchyID.hidLikelihood)).GetNodeByID(data.Assignment.ObjectiveID);
                                                if (tNode != null)
                                                    Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNode.NodeName));
                                                WRT = tNode;
                                                tNode = App.ActiveProject.ProjectManager.get_AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID);
                                                if (tNode != null)
                                                    Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(tNode.NodeName));
                                            }
                                            break;
                                        case ControlType.ctConsequenceToEvent:
                                            if (!Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty))
                                            {
                                                tNode = App.ActiveProject.ProjectManager.get_Hierarchy(Convert.ToInt16(ECTypes.ECHierarchyID.hidImpact)).GetNodeByID(data.Assignment.ObjectiveID);
                                                if (tNode != null)
                                                    Params.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(tNode.NodeName));
                                                WRT = tNode;
                                                tNode = App.ActiveProject.ProjectManager.get_AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID);
                                                if (tNode != null)
                                                    Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(tNode.NodeName));
                                            }
                                            break;
                                    }
                                    tClusterNode = WRT;
                                    // D2703

                                    string sName = "";
                                    switch (data.Control.Type)
                                    {
                                        case ControlType.ctCause:
                                            sName = App.ResString("lblControlCause");
                                            break;
                                        case ControlType.ctCauseToEvent:
                                            sName = App.ResString(Convert.ToString((WRT != null && WRT.Level == 0 ? "lblControlCauseToEventGoal" : "lblControlCauseToEvent")));
                                            // D2654
                                            break;
                                        case ControlType.ctConsequenceToEvent:
                                            sName = App.ResString("lblControlConsequence");
                                            break;
                                    }
                                    sRes = string.Format(App.ResString("taskControlNonPWOneAtATime"), sName);
                                    fGetResString = false;
                                }

                            }
                            else
                            {
                                // D2503 ==
                                switch (data.MeasurementType)
                                {
                                    case ECMeasureType.mtRatings:
                                        // D2508 ===
                                        // D2589 ===
                                        bool isAlt = data.Node != null && (data.Node.IsAlternative || data.Node.IsTerminalNode);
                                        // D2530 + D2587
                                        clsNonPairwiseMeasureData tData = (clsNonPairwiseMeasureData)data.Judgment;
                                        clsNode tNode = null;
                                        // D2530
                                        if (isAlt)
                                        {
                                            tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(((clsNonPairwiseMeasureData)data.Judgment).NodeID);
                                        }
                                        else
                                        {
                                            tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(((clsNonPairwiseMeasureData)data.Judgment).NodeID);
                                            if (tNode == null)
                                            {
                                                tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(((clsNonPairwiseMeasureData)data.Judgment).NodeID);
                                                isAlt = true;
                                            }
                                        }
                                        if (tNode != null)
                                            Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNode.NodeName));
                                        // D2589 ==
                                        // D1508 ==
                                        Params.Add(ECWeb.Options._TEMPL_NODE_B, GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive));

                                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive));

                                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(data.Node.Children.Count));

                                        // D0558 + D2830
                                        if (App.isRiskEnabled)
                                        {
                                            if (isImpact)
                                            {
                                                sRes = Convert.ToString((data.Node.Level > 0 ? "lblEvaluationRatingImpact" : "lblEvaluationRatingImpactNoLevels"));
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((data.Node.Level > 0 ? "lblEvaluationRatingRisk" : "lblEvaluationRatingNoLevelsRisk"));
                                                // D2407
                                            }
                                        }
                                        else
                                        {
                                            sRes = Convert.ToString((data.Node.Level > 0 ? "lblEvaluationRating" : "lblEvaluationRatingNoLevels"));
                                            // D2589
                                        }
                                        if (data.Node != null && !isAlt)
                                            sRes += "Obj";
                                        // D2527 + D2530
                                        break;

                                    case ECMeasureType.mtStep:
                                        clsStepMeasureData tStep = (clsStepMeasureData)data.Judgment;
                                        clsNode tParentNode = (clsNode)data.Node.Hierarchy.GetNodeByID(tStep.ParentNodeID);
                                        clsNode tAlt = default(clsNode);
                                        if (tParentNode.IsTerminalNode)
                                        {
                                            tAlt = data.Node.Hierarchy.ProjectManager.get_AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tStep.NodeID);
                                        }
                                        else
                                        {
                                            tAlt = data.Node.Hierarchy.GetNodeByID(tStep.NodeID);
                                        }

                                        Params.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, fCanBePathInteractive));
                                        // D2830

                                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tAlt, fCanBePathInteractive));

                                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(tAlt.Children.Count));
                                        // D2408 ===
                                        if (App.isRiskEnabled)
                                        {
                                            if (isImpact)
                                            {
                                                sRes = Convert.ToString((tParentNode.get_ParentNode() == null ? "lblEvaluationStepGoalImpact" : "lblEvaluationStepImpact"));
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((tParentNode.get_ParentNode() == null ? "lblEvaluationStepGoalRisk" : "lblEvaluationStepRisk"));
                                            }
                                        }
                                        else
                                        {
                                            sRes = "lblEvaluationStep";
                                        }
                                        break;
                                    // D2408 ==

                                    case ECMeasureType.mtDirect:
                                        // D2361 + D2379 + D2830
                                        // D2540 ===
                                        clsDirectMeasureData tDirect = (clsDirectMeasureData)data.Judgment;
                                        clsHierarchy tH = default(clsHierarchy);
                                        if (data.Node.IsTerminalNode)
                                        {
                                            tH = App.ActiveProject.HierarchyAlternatives;
                                        }
                                        else
                                        {
                                            tH = App.ActiveProject.HierarchyObjectives;
                                           
                                        }
                                        clsNode DirectChild = tH.GetNodeByID(tDirect.NodeID);
                                        Params.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive));
                                        // D2830
                                        // D2540 ==
                                        if (App.isRiskEnabled)
                                        {
                                            if (isImpact)
                                            {
                                                sRes = Convert.ToString((data.Node.Level == 0 ? "task_DirectDataImpactNoObj" : "task_DirectDataImpact"));
                                                // D2398
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((data.Node.Level == 0 ? "task_DirectDataRiskNoObj" : "task_DirectDataRisk"));
                                            }
                                            Params.Add(ECWeb.Options._TEMPL_NODETYPE, Convert.ToString((data.Node.IsTerminalNode ? ECWeb.Options._TEMPL_ALTERNATIVE : ECWeb.Options._TEMPL_OBJECTIVE)));
                                            // D2540
                                        }
                                        else
                                        {
                                            sRes = "task_DirectData";
                                        }

                                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tH.GetNodeByID(tDirect.NodeID), fCanBePathInteractive));

                                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(data.Node.Children.Count));

                                        break;
                                    case ECMeasureType.mtAdvancedUtilityCurve:
                                    case ECMeasureType.mtCustomUtilityCurve:
                                    case ECMeasureType.mtRegularUtilityCurve:
                                        clsNode tParentNode2 = (clsNode)data.Node.Hierarchy.GetNodeByID(((clsUtilityCurveMeasureData)data.Judgment).ParentNodeID);
                                        Params.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode2, fCanBePathInteractive));
                                        // D2830
                                        clsNode tAlt2 = default(clsNode);
                                        if (tParentNode2.IsTerminalNode)
                                        {
                                            tAlt2 = data.Node.Hierarchy.ProjectManager.get_AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(((clsUtilityCurveMeasureData)data.Judgment).NodeID);
                                        }
                                        else
                                        {
                                            tAlt2 = data.Node.Hierarchy.GetNodeByID(((clsUtilityCurveMeasureData)data.Judgment).NodeID);
                                        }
                                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tAlt2, fCanBePathInteractive));
                                        switch (data.MeasurementType)
                                        {
                                            case ECMeasureType.mtAdvancedUtilityCurve:
                                                sRes = "task_AdvancedUtilityCurve";
                                                break;
                                            default:
                                                // D2405 ===
                                                if (App.isRiskEnabled)
                                                {
                                                    if (isImpact)
                                                    {
                                                        sRes = Convert.ToString((tParentNode2.get_ParentNode() == null ? "lblEvaluationUCGoalImpact" : "lblEvaluationUCImpact"));
                                                    }
                                                    else
                                                    {
                                                        sRes = Convert.ToString((tParentNode2.get_ParentNode() == null ? "lblEvaluationUCGoalRisk" : "lblEvaluationUCRisk"));
                                                    }
                                                }
                                                else
                                                {
                                                    sRes = "lblEvaluationUC";
                                                }
                                                break;
                                            // D2405 ==
                                        }

                                        break;
                                }
                                tClusterNode = data.Node;
                                // D2364
                            }
                        }

                        break;
                    case ActionType.atNonPWAllChildren:
                        clsAllChildrenEvaluationActionData data2 = (clsAllChildrenEvaluationActionData)Action.ActionData;

                        if (data2 != null && data2.ParentNode != null)
                        {
                            switch (data2.MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                    if (App.isRiskEnabled)
                                    {
                                        if (isImpact)
                                        {
                                            sRes = Convert.ToString((data2.ParentNode.IsAlternative ? "task_MultiRatings_AllCovObjImpact" : (data2.ParentNode.get_ParentNode() == null ? "task_MultiRatings_AllAltsGoalImpact" : "task_MultiRatings_AllAltsImpact")));
                                        }
                                        else
                                        {
                                            sRes = Convert.ToString((data2.ParentNode == null || data2.ParentNode.get_ParentNode() == null ? "lblEvaluationMultiDirectDataLikelihood" : (data2.ParentNode.IsTerminalNode ? "task_MultiRatings_AllAltsRisk" : (data2.ParentNode.RiskNodeType == ECTypes.RiskNodeType.ntCategory ? "task_MultiRatings_AllObjRisk_Cat" : "task_MultiRatings_AllObjRisk"))));
                                            // D2318 + D2319 + D2964
                                        }
                                        if (Hierarchy != null && Hierarchy.Nodes.Count == 1)
                                            sRes = "task_MultiRatings_AllAlts_NoObj";
                                    }
                                    else
                                    {
                                        sRes = "task_MultiRatings_AllAlts";
                                    }

                                    break;

                                case ECMeasureType.mtDirect:
                                    if (App.isRiskEnabled)
                                    {
                                        if (data2.ParentNode.IsTerminalNode)
                                        {
                                            // D2354 ===
                                            if (isImpact)
                                            {
                                                sRes = Convert.ToString((data2.ParentNode.Level == 0 ? "lblEvaluationMultiDirectDataAltsGoalRisk" : "lblEvaluationMultiDirectDataAltsRisk"));
                                                // D2399
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((data2.ParentNode.Level == 0 ? "lblEvaluationMultiDirectDataAltsGoalLikelihood" : "lblEvaluationMultiDirectDataAltsLikelihood"));
                                            }
                                            // D2354 ==
                                        }
                                        else
                                        {
                                            if (!isImpact)
                                            {
                                                sRes = Convert.ToString((data2.ParentNode.Level > 0 ? "lblEvaluationMultiDirectDataLevelsLikelihood" : "lblEvaluationMultiDirectDataLikelihood"));
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((data2.ParentNode.get_ParentNode() == null ? "lblEvaluationMultiDirectDataGoalRisk" : "lblEvaluationMultiDirectDataRiskObj"));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (data2.ParentNode.IsTerminalNode)
                                        {
                                            sRes = "lblEvaluationMultiDirectDataAlts";
                                        }
                                        else
                                        {
                                            sRes = "lblEvaluationMultiDirectData";
                                        }
                                    }

                                    break;
                            }

                            Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(data2.ParentNode, fCanBePathInteractive && data2.ParentNode.RiskNodeType != ECTypes.RiskNodeType.ntCategory));
                            // D2830 + D2964
                            Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(data2.Children.Count));
                            tClusterNode = data2.ParentNode;
                            // D2364
                        }

                        break;

                    case ActionType.atNonPWAllCovObjs:
                        clsAllCoveringObjectivesEvaluationActionData data3 = (clsAllCoveringObjectivesEvaluationActionData)Action.ActionData;
                        //Cv2 'C0464

                        if ((data3 != null))
                        {
                            switch (data3.MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                    if (App.isRiskEnabled)
                                    {
                                        if (isImpact)
                                        {
                                            sRes = Convert.ToString((App.ActiveProject.HierarchyObjectives.GetMaxLevel() < 1 ? "task_MultiRatings_AllCovObjGoalImpact" : "task_MultiRatings_AllCovObjImpact"));
                                        }
                                        else
                                        {
                                            sRes = Convert.ToString((App.ActiveProject.HierarchyObjectives.GetMaxLevel() < 1 ? "task_MultiRatings_AllCovObjGoal" : "task_MultiRatings_AllCovObj"));
                                            //If Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1 Then sRes = "task_MultiRatings_AllAlts_NoObj" ' - D2429
                                        }
                                    }
                                    else
                                    {
                                        sRes = "task_MultiRatings_AllCovObj";
                                    }

                                    break;
                                case ECMeasureType.mtDirect:
                                    if (App.isRiskEnabled)
                                    {
                                        // D2360 ===
                                        if (data3.Alternative.IsTerminalNode)
                                        {
                                            if (Hierarchy.Nodes.Count <= 1)
                                                sRes = "lblEvaluationMultiDirectAltWRTCovRiskNoObj";
                                            else
                                                sRes = "lblEvaluationMultiDirectAltWRTCovRisk";
                                        }
                                        else
                                        {
                                            if (Hierarchy.Nodes.Count <= 1)
                                                sRes = "lblEvaluationMultiDirectDataRiskNoObj";
                                            else
                                                sRes = "lblEvaluationMultiDirectDataRisk";
                                        }
                                        // D2360 ==
                                    }
                                    else
                                    {
                                        sRes = Convert.ToString((data3.Alternative.IsTerminalNode ? "lblEvaluationMultiDirectAltWRTCov" : "lblEvaluationMultiDirectData"));
                                    }
                                    break;
                            }

                            Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(data3.Alternative, fCanBePathInteractive));
                            // D2830
                            Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(data3.CoveringObjectives.Count));
                            tClusterNode = data3.Alternative;
                            // D2364
                        }

                        break;

                    // D3250 ===
                    case ActionType.atAllEventsWithNoSource:
                        clsAllEventsWithNoSourceEvaluationActionData data4 = (clsAllEventsWithNoSourceEvaluationActionData)Action.ActionData;
                        if ((data4 != null))
                        {
                            switch (data4.MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                case ECMeasureType.mtDirect:
                                    sRes = "lblEvaluationNoSources";
                                    tClusterNode = null;
                                    break;
                            }
                        }
                        break;
                    // D3250 ==

                    case ActionType.atAllPairwise:
                    case ActionType.atAllPairwiseOutcomes:

                        if ((Action.ActionData) is clsAllPairwiseEvaluationActionData)
                        {
                            clsAllPairwiseEvaluationActionData Data5 = (clsAllPairwiseEvaluationActionData)Action.ActionData;
                            bool fIsPWOutcomes2 = Action.ActionType == ActionType.atAllPairwiseOutcomes;
                            Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(Data5.ParentNode, fCanBePathInteractive));
                            // D2830
                            Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(Data5.Judgments.Count));
                            if (fIsPWOutcomes2)
                            {
                                if (Action.ParentNode != null)
                                    Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(Action.ParentNode.NodeName));
                                if (Action.ParentNode != null && Action.ParentNode.IsAlternative)
                                {
                                    sRes = Convert.ToString((Data5.ParentNode != null && Data5.ParentNode.get_ParentNode() == null ? "task_MultiPairwise_Alternatives_PW{0}_Goal" : "task_MultiPairwise_Alternatives_PW{0}"));
                                    // D2351
                                }
                                else
                                {
                                    sRes = Convert.ToString((Data5.ParentNode != null && Data5.ParentNode.get_ParentNode() == null ? "task_MultiPairwise_Hierarchy_PW{0}" : "task_MultiPairwise_Objectives_PW{0}"));
                                }
                                clsRatingScale tRS = (clsRatingScale)Action.PWONode.MeasurementScale;
                                string sName = "Outcomes";
                                if (tRS.IsPWofPercentages)
                                    sName = "Percentages";
                                if (tRS.IsExpectedValues)
                                    sName = "ExpectedValues";
                                sRes = string.Format(sRes, sName);
                            }
                            else
                            {
                                if (Data5.ParentNode != null && Data5.ParentNode.IsTerminalNode)
                                {
                                    sRes = Convert.ToString((Data5.ParentNode.get_ParentNode() == null ? "task_MultiPairwise_AlternativesGoal" : (App.isRiskEnabled && !isImpact ? "task_MultiPairwise_AlternativesLikelihood" : "task_MultiPairwise_Alternatives")));
                                }
                                else
                                {
                                    sRes = Convert.ToString((tExtraParam != null ? ((tExtraParam) is clsStepFunction ? "task_MultiPairwise_Objectives_Intensities_SF" : "task_MultiPairwise_Objectives_Intensities") : (Data5.ParentNode.get_ParentNode() == null ? "task_MultiPairwise_ObjectivesGoal" : (App.isRiskEnabled && !isImpact ? "task_MultiPairwise_ObjectivesLikelihood" : "task_MultiPairwise_Objectives"))));
                                }
                            }
                            tClusterNode = Data5.ParentNode;
                        }

                        break;
                    case ActionType.atShowLocalResults:
                        clsShowLocalResultsActionData Data6 = (clsShowLocalResultsActionData)Action.ActionData;
                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(Data6.ParentNode, fCanBePathInteractive));

                        var Test = Data6.ParentNode.NodeGuidID;

                        bool fIsPWOutcomes3 = Data6.PWOutcomesNode != null && Data6.ParentNode.get_MeasureType() == ECMeasureType.mtPWOutcomes;
                        if (fIsPWOutcomes3)
                            Params.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Data6.PWOutcomesNode, fCanBePathInteractive));
                        // D2830
                        if (tExtraParam != null)
                        {
                            sRes = "task_LocalResultsObjectivesIntensities";
                        }
                        else
                        {
                            if (App.isRiskEnabled)
                            {
                                if (Data6.ParentNode.IsTerminalNode)
                                {
                                    sRes = Convert.ToString((!isImpact ? "task_LocalResultsAlternativesRisk" : "task_LocalResultsAlternativesImpact"));
                                }
                                else
                                {
                                    // D2723 ==
                                    if (!isImpact && Data6.ParentNode.RiskNodeType == ECTypes.RiskNodeType.ntCategory)
                                    {
                                        sRes = "task_LocalResultsObjectives_Category";
                                    }
                                    else
                                    {
                                        // D2723 ==
                                        if (!isImpact)
                                        {
                                            sRes = Convert.ToString((Data6.ParentNode.get_ParentNode() == null ? "task_LocalResultsObjectivesGoal" : "task_LocalResultsObjectivesRisk"));
                                        }
                                        else
                                        {
                                            sRes = Convert.ToString((Data6.ParentNode.get_ParentNode() == null ? "task_LocalResultsObjectivesGoalImpact" : "task_LocalResultsObjectivesImpact"));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                sRes = Convert.ToString((Data6.ParentNode.IsTerminalNode ? "task_LocalResultsAlternatives" : "task_LocalResultsObjectives"));
                            }
                        }
                        tClusterNode = Data6.ParentNode;
                        // D2364
                        break;


                    case ActionType.atShowGlobalResults:
                        if (App.isRiskEnabled)
                        {
                            sRes = Convert.ToString((isImpact ? "task_GlobalResultsImpact" : "task_GlobalResultsRisk"));
                        }
                        else
                        {
                            sRes = "task_GlobalResults";
                        }
                        tClusterNode = Hierarchy.Nodes[0];
                        // D2364
                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tClusterNode, fCanBePathInteractive));
                        // D2364 + D2830
                        break;

                    case ActionType.atSensitivityAnalysis:
                        clsSensitivityAnalysisActionData Data7 = (clsSensitivityAnalysisActionData)Action.ActionData;
                        switch (Data7.SAType)
                        {
                            case SAType.satDynamic:
                                sRes = Convert.ToString((App.isRiskEnabled ? (!isImpact ? "task_DynamicSARisk" : "task_DynamicSAImpact") : "task_DynamicSA"));
                                // D2527
                                break;
                            case SAType.satGradient:
                                sRes = Convert.ToString((App.isRiskEnabled ? (!isImpact ? "task_GradientSARisk" : "task_GradientSAImpact") : "task_GradientSA"));
                                break;
                            case SAType.satPerformance:
                                sRes = Convert.ToString((App.isRiskEnabled ? (!isImpact ? "task_PerformanceSARisk" : "task_PerformanceSAImpact") : "task_PerformanceSA"));
                                // D2527
                                break;
                        }
                        tClusterNode = Action.ParentNode;
                        // D2364
                        break;

                }
            }
            ClusterPhrase = "";
            // D2364 ===
            string sDefTask = "";
            if (fGetResString && !string.IsNullOrEmpty(sRes))
                sDefTask = ResString(sRes, false, false);
            else
                sDefTask = sRes;
            // D2503 + D2596

            // D2751 ===
            bool isMulti = false;
            Guid tAGuid = Guid.Empty;
            switch (Action.ActionType)
            {
                case ActionType.atAllPairwise:
                case ActionType.atAllPairwiseOutcomes:
                case ActionType.atNonPWAllChildren:
                case ActionType.atNonPWAllCovObjs:
                    isMulti = true;
                    break;
                case ActionType.atShowLocalResults:
                    isMulti = true;
                    if(tClusterNode != null)
                    {
                        tAGuid = tClusterNode.NodeGuidID;
                    }
                    break;
                case ActionType.atShowGlobalResults:
                case ActionType.atSensitivityAnalysis:
                     if (tClusterNode != null)
                    {
                        tAGuid = tClusterNode.NodeGuidID;
                    }
                    break;
            }

            // D2751 ==
            //var sss = App.ActiveProject.PipeParameters.JudgementPromt;
            //var sssd = App.ActiveProject.PipeParameters.JudgementPromtMulti;
            //var sssa = App.ActiveProject.PipeParameters.JudgementAltsPromt;
            //var ssss = App.ActiveProject.PipeParameters.JudgementAltsPromtMulti;
            //var sdsds = App.ActiveProject.PipeParameters.NameAlternatives;
            //var sdsds3 = App.ActiveProject.PipeParameters.ProjectPurpose;
            //var sdsds33 = App.ActiveProject.PipeParameters.TeamTimeInvitationSubject;
            //var sdsds35 = App.ActiveProject.PipeParameters.PipeMessages;
            //var sdsds38 = App.ActiveProject.PipeParameters.ProjectPurpose;
            //var sdsdss = tClusterNode.NodeGuidID;

            if (tClusterNode != null && !fIgnoreClusterPhrase)

                ClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(tClusterNode, isMulti, tAGuid, false);
            // D2692 + D2751
            if (string.IsNullOrEmpty(ClusterPhrase) || string.IsNullOrEmpty(StringFuncs.HTML2Text(ClusterPhrase)))
                ClusterPhrase = sDefTask;
            // D2729
            ClusterPhraseIsCustom = ClusterPhrase.Trim().ToLower() != sDefTask.Trim().ToLower();
            if (tClusterNode != null)
                TaskNodeGUID = tClusterNode.NodeGuidID.ToString();
            TaskTemplates = Params;

            // D2929 ===
            // D2964
            if (tClusterNode == null || (tClusterNode != null && tClusterNode.RiskNodeType != ECTypes.RiskNodeType.ntCategory))
            {
                //If fCanBePathInteractive Then  ' D2972
                string[] sNodesTempls = {
                    ECWeb.Options._TEMPL_JUSTNODE,
                    ECWeb.Options._TEMPL_NODENAME,
                    ECWeb.Options._TEMPL_NODE_A,
                    ECWeb.Options._TEMPL_NODE_B
		        };
                Dictionary<string, string> sNodeParams = new Dictionary<string, string>();
                foreach (string sTpl in sNodesTempls)
                {
                    if (ClusterPhrase.Contains(sTpl) && !ClusterPhrase.ToLower().Contains("'wrt_name'>" + sTpl.ToLower()))
                    {
                        sNodeParams.Add(sTpl, string.Format("<span class='node_name'>{0}</span>", sTpl));
                    }
                }
                if (sNodeParams.Count > 0)
                    ClusterPhrase = StringFuncs.ParseStringTemplates(ClusterPhrase, sNodeParams);
                //End If
             }
            // D2929 ==

            string sTask = "";
            if (fParseNodeNames && Params != null)
            {
                sTask = PrepareTask(StringFuncs.ParseStringTemplates(ClusterPhrase, Params), tExtraParam, fHasSubnodes, Params);
                foreach (string sKey in Params.Keys)
                {
                    if (!TaskTemplates.ContainsKey(sKey))
                        TaskTemplates.Add(sKey, Params[sKey]);
                }
            }
            else
            {
                sTask = PrepareTask(ClusterPhrase, tExtraParam, fHasSubnodes);
            }
            context.Session["sRes"] = sRes;
            context.Session["ClusterPhrase"] = ClusterPhrase;
            return sTask;

        }


        public static List<clsLanguageResource> LanguagesScanFolder(string sPath)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            List<clsLanguageResource> Languages = new List<clsLanguageResource>();
            //try
            //{
            string[] files = System.IO.Directory.GetFiles(context.Server.MapPath("~/App_GlobalResources/"), "*.resx");
            var AllFiles = new System.Collections.ObjectModel.ReadOnlyCollection<string>(files);
            _Default.debugging = "";
            foreach (string sFileName in AllFiles)
                {
                    // D0210 + D0219
                    if (System.IO.Path.GetExtension(sFileName).ToLower() == Consts._FILE_RESOURCE_EXT & !System.IO.Path.GetFileName(sFileName).StartsWith("~"))
                    {
                        _Default.debugging += (sFileName + " <br>");
                        clsLanguageResource Lng = new clsLanguageResource();
                        Lng.ResxFilename = sFileName;
                        if (Lng.isLoaded)
                        {
                            Languages.Add(Lng);
                        }
                        else
                        {

                        }
                    }
                }
                _Default.debugging += ("Done <br>");
            //}
            //catch (Exception ex)
            //{
            //    // D0330
            //    _Default.debugging += "error <br>";
            //}
            return Languages;
        }


        #region Restring
        public static string ResString(string sResourceName, bool fAsIsIfMissed = false, bool fParseTemplates = true, bool fCapitalized = false)
        {
            // D0232
            var context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var sRes = app.ResString(sResourceName, fAsIsIfMissed);

            if (sResourceName.ToLower() != ECWeb.Options._TEMPL_APPNAME.ToLower() && sResourceName.ToLower() != ECWeb.Options._TEMPL_APPNAME_PLAIN.ToLower())
            {
                if (fParseTemplates && sRes.IndexOf("%") >= 0)
                    sRes = ParseAllTemplates(sRes, app.ActiveUser, app.ActiveProject);
                // D0060 +  D0220 + D0232 + D0466
            }

            if (fCapitalized)
            {
                sRes = sRes.Substring(0, 1).ToUpper() + sRes.Substring(1).ToLower();
            }

            return sRes;
        }


        public static string ApplicationName(bool NameWithMark = true)
        {
            // D3886
            // D1627 + D1634 + D2257 + D3886
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                return App.ResString(Convert.ToString((App.isRiskEnabled ? "titleApplicationNameRisk" : "titleApplicationName")) + Convert.ToString((NameWithMark ? "" : "Plain")));
        }


        public static string ParseTemplate(string sMessage, string sTemplateName, string sTemplateValue, bool fUseJSSafeString = false)
        {
            // D0220
            return ParseTemplateCommon(sMessage, sTemplateName, sTemplateValue, fUseJSSafeString);
        }

        public static string ParseTemplateCommon(string sMessage, string sTemplateName, string sTemplateValue, bool fUseJSSafeString = false)
        {
            // D0220
            if (fUseJSSafeString)
                sTemplateValue = StringFuncs.JS_SafeString(sTemplateValue);
            // D0220
            sMessage = sMessage.Replace("%%" + StringFuncs.Capitalize(sTemplateName.Trim(Convert.ToChar("%"))) + "%%", StringFuncs.Capitalize(sTemplateValue));
            // D2427
            return sMessage.Replace(sTemplateName, sTemplateValue);
        }

        public static string ParseURLAndPathTemplates(string sMessage, bool fUseJSSafeString = false)
        {
            string sRes = sMessage;
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_ROOT_URI, Consts._URL_ROOT, fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_ROOT_PATH, Consts._FILE_ROOT, fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_ROOT_IMAGES, ThemePath + Consts._FILE_IMAGES, fUseJSSafeString);
            return sRes;
        }
        public static string ThemePath
        {
            // D1319

            get { Page page = (Page)HttpContext.Current.Session["thispage"]; return string.Format("{0}{1}/", ECWeb.Options._URL_THEMES, (string.IsNullOrEmpty(page.Theme) ? PagesList._THEME_EC09 : page.Theme)); }
        }

        public static string ApplicationURL(bool isEvaluationSite, bool fIsTeamTime)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            string sURL = context.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            if (isEvaluationSite && !string.IsNullOrEmpty(App.Options.EvalSiteURL) && (!App.Options.EvalURL4TeamTime || fIsTeamTime))
                sURL = App.Options.EvalSiteURL;
            // D3308 + D3494
            //If Not Request.Url.IsDefaultPort Then sURL = String.Concat(sURL, ":", Request.Url.Port)    '-D0345 (double port)
            return sURL;
        }

        public static string CreateLogonURL(clsApplicationUser tUser, clsProject tProject, string sOtherParams, string sPagePath, string sPasscode = null)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            // D2467
            string sURL = "";
            if ((tUser != null))
            {
                sURL = string.Format("{0}={1}&{2}={3}", ECWeb.Options._PARAM_EMAIL, HttpUtility.UrlEncode(tUser.UserEMail), ECWeb.Options._PARAM_PASSWORD, HttpUtility.UrlEncode(tUser.UserPassword));
                // D1465
                if ((tProject != null))
                {
                    sURL += string.Format("&{0}={1}", ECWeb.Options._PARAM_PASSCODE, HttpUtility.UrlEncode(Convert.ToString((string.IsNullOrEmpty(sPasscode) ? tProject.Passcode : sPasscode))));
                    // D1465 + D1529 + D2467
                }
            }
            if (!string.IsNullOrEmpty(sOtherParams))
            {
                if (!string.IsNullOrEmpty(sURL))
                    sURL += "&";
                sURL += sOtherParams;
            }
            // D0896 ===
            //Return String.Format("{0}?key={1}", sPagePath, EncodeURL(sURL, App.InstanceID)) ' D0826
            sURL = CryptService.EncodeURL(sURL, App.DatabaseID);
            string sLink = Convert.ToString((sPagePath.Contains("?") ? "&" : "?"));
            // D1529
            if (App.Options.UseTinyURL)
            {
                // D0899 ===
                int PID = -1;
                int UID = -1;
                if (tProject != null)
                    PID = tProject.ID;
                if (tUser != null)
                    UID = tUser.UserID;
                sURL = string.Format("{0}{3}{2}={1}", sPagePath, App.CreateTinyURL(sURL, PID, UID), ECWeb.Options._PARAMS_TINYURL[0], sLink);
                // D1529
                // D0899 ==
            }
            else
            {
                sURL = string.Format("{0}{3}{2}={1}", sPagePath, sURL, ECWeb.Options._PARAMS_KEY[0], sLink);
                // D1529
            }
            return sURL;
            // D0896 ==
        }

        public static string ParseAllTemplates(string sMessage, clsApplicationUser tUser, clsProject tProject, bool fUseJSSafeString = false, bool fReplaceOnlyCommon = false)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];

            // D0220 + D0312
            if (sMessage.IndexOf("%") < 0)
                return sMessage;
            // D0220

            string sRes = ParseTemplate(sMessage, ECWeb.Options._TEMPL_APPNAME, ApplicationName(), fUseJSSafeString);
            // D0090
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_APPNAME_PLAIN, ApplicationName(false), fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_APPNAME_TEAMTIME, App.ResString("titleApplicationTeamTime"), fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_APPNAME_SURVEY, App.ResString("titleApplicationSurvey"), fUseJSSafeString);

            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_PROJECTS, App.ResString("templ_Projects"), fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_PROJECT, App.ResString("templ_Project"), fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_MODELS, App.ResString("templ_Models"), fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_MODEL, App.ResString("templ_Model"), fUseJSSafeString);

            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_SERVICE_EMAIL, ECWeb.WebOptions.SystemEmail, fUseJSSafeString);
            // D0220 + D0315 + D1152

            //Dim sGroupName As String = ""  ' -D1830
            string sUserName = "";
            string sUserEmail = "";
            string sUserPsw = "*********";
            // D0220 + D2216
            if ((tUser != null))
            {
                sUserEmail = tUser.UserEMail;
                sUserName = tUser.UserName;
                // D0220 ===
                //sUserPsw = tUser.UserPassword  ' -D2216

                // -D1830
                //If Not App.ActiveWorkgroup Is Nothing Then
                //    If Not App.ActiveRoleGroup Is Nothing Then sGroupName = App.ActiveRoleGroup.Name ' D0466
                //End If
                // D0220 ==
            }

            // D0270 ===
            string sOwnerName = "";
            string sOwnerEmail = "";
            if ((App.ActiveUser != null))
            {
                sOwnerName = App.ActiveUser.UserName;
                sOwnerEmail = App.ActiveUser.UserEMail;
            }
            // D0270 ==

            string sWkgName = "";
            // D3401
            if (App.ActiveWorkgroup != null)
                sWkgName = App.ActiveWorkgroup.Name;
            // D3401

            //' D0121 ===
            //sRes = ParseTemplate(sRes, _TEMPL_ROOT_URI, _URL_ROOT, fUseJSSafeString)
            //sRes = ParseTemplate(sRes, _TEMPL_ROOT_PATH, _FILE_ROOT, fUseJSSafeString)
            //sRes = ParseTemplate(sRes, _TEMPL_ROOT_IMAGES, ThemePath + _FILE_IMAGES, fUseJSSafeString)
            //' D0121 ==
            sRes = ParseURLAndPathTemplates(sRes, fUseJSSafeString);
            // D1550

            // D0220 ===
            string sPrjName = "";
            string sPrjPasscode = "";
            string sMeetingID = "";
            // D0388
            string sRiskModel = "";
            // D1804
            string sRiskModels = "";
            // D3001
            if ((tProject != null))
            {
                //tProject = App.ActiveProject   ' -D1630: Who did this?
                sPrjName = StringFuncs.SafeFormString(tProject.ProjectName);
                //L0399
                sPrjPasscode = StringFuncs.SafeFormString(tProject.Passcode);
                //L0399
                sMeetingID = clsMeetingID.AsString(tProject.get_MeetingID());
                // D0420
                // D1640 ===
                if (!string.IsNullOrEmpty(sUserEmail) && tProject.isValidDBVersion)
                {
                    ECTypes.clsUser tPrjuser = tProject.ProjectManager.GetUserByEMail(sUserEmail);
                    if (tPrjuser != null && !string.IsNullOrEmpty(tPrjuser.UserName))
                        sUserName = tPrjuser.UserName;
                    if (App.ActiveUser != null)
                    {
                        tPrjuser = tProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEMail);
                        if (tPrjuser != null && !string.IsNullOrEmpty(tPrjuser.UserName))
                            sOwnerName = tPrjuser.UserName;
                    }
                    if (App.isRiskEnabled)
                    {
                        sRiskModel = App.ResString(Convert.ToString((tProject.ProjectManager.ActiveHierarchy == Convert.ToInt32(ECTypes.ECHierarchyID.hidImpact) ? "lblImpact" : "lblLikelihood")));
                        sRiskModels = App.ResString(Convert.ToString((tProject.ProjectManager.ActiveHierarchy == Convert.ToInt32(ECTypes.ECHierarchyID.hidImpact) ? "lblImpacts" : "lblLikelihoods")));
                        // D3001
                    }
                    else
                    {
                        sRiskModel = App.ResString("lblProject");
                        sRiskModels = App.ResString("lblProjects");
                        // D3001
                    }
                }
                // D1640 ==
            }

            if (string.IsNullOrEmpty(sUserPsw))
                sUserPsw = App.ResString("lblDummyBlankPassword");
            // D0306

            // D0312
            if (!fReplaceOnlyCommon)
            {
                //sRes = ParseTemplate(sRes, _TEMPL_GROUPNAME, sGroupName, fUseJSSafeString)  ' D0220 -D1830
                sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_USEREMAIL, sUserEmail, fUseJSSafeString);
                sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_USERNAME, sUserName, fUseJSSafeString);
                sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_USER_FIRSTNAME, StringFuncs.GetUserFirstName(sUserName), fUseJSSafeString);
                // D2279
                sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_USERPSW, sUserPsw, fUseJSSafeString);
                // D0220
            }

            // D0270 ===
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_OWNERNAME, sOwnerName, fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_OWNEREMAIL, sOwnerEmail, fUseJSSafeString);
            // D0270 ==
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_WORKGROUP, sWkgName, fUseJSSafeString);
            // D3401

            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_PRJNAME, sPrjName, fUseJSSafeString);
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_PRJPASSCODE, sPrjPasscode, fUseJSSafeString);

            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_RISKMODEL, sRiskModel, fUseJSSafeString);
            // D1804
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_RISKMODELS, sRiskModels, fUseJSSafeString);
            // D3001

            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_MEETING_ID, sMeetingID, fUseJSSafeString);
            // D0388

            string sStartURL = ApplicationURL(false, false) + Consts._URL_ROOT;
            string sEvalAnytimeURL = ApplicationURL(true, false);
            string sEvalTTURL = ApplicationURL(true, true);
            // D3308
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_APP, sStartURL, fUseJSSafeString);
            // D3494
            sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_APP_TT, sEvalTTURL, fUseJSSafeString);

            // D0312
            if (!fReplaceOnlyCommon)
            {
                // D1672 + D1734 + D3308 + D3494 ===
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_LOGIN.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_LOGIN, CreateLogonURL(tUser, null, "", sStartURL), fUseJSSafeString);
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE, CreateLogonURL(tUser, tProject, "pipe=yes", sEvalAnytimeURL), fUseJSSafeString);
                // D3150
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_TT.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_TT, CreateLogonURL(tUser, tProject, "TTOnly=1", sEvalTTURL), fUseJSSafeString);
                // D0655
                if (App.isRiskEnabled && sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_LIKELIHOOD.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_LIKELIHOOD, CreateLogonURL(tUser, tProject, "pipe=yes", sEvalAnytimeURL, tProject.PasscodeLikelihood), fUseJSSafeString);
                // D2467 + D3150
                if (App.isRiskEnabled && sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_IMPACT.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_IMPACT, CreateLogonURL(tUser, tProject, "pipe=yes", sEvalAnytimeURL, tProject.PasscodeImpact), fUseJSSafeString);
                // D2467 + D3150
                if (App.isRiskEnabled && sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_CONTROLS.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_CONTROLS, CreateLogonURL(tUser, tProject, "pipe=yes&mode=riskcontrols", sEvalAnytimeURL, tProject.PasscodeLikelihood), fUseJSSafeString);
                // D3769
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_MEETINGID.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_MEETINGID, string.Format("{0}?{1}={2}", sEvalTTURL, ECWeb.Options._PARAM_MEETING_ID, sMeetingID), fUseJSSafeString);
                // D1352
                // D1060 + D1830 + D3308 ===
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_ANONYM.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_ANONYM, CreateEvaluateSignupURL(tProject, true, "", "", sStartURL), fUseJSSafeString);
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_SIGNUP.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_SIGNUP, CreateEvaluateSignupURL(tProject, false, "", "", sStartURL), fUseJSSafeString);
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_RESETPSW.ToLower()) && tUser != null)
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_RESETPSW, CreateResetPswURL(tUser, "", sStartURL), fUseJSSafeString);
                // D2216
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY, CreateEvaluateSignupURL(tProject, false, "e", "", sStartURL), fUseJSSafeString);
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW, CreateEvaluateSignupURL(tProject, false, "ep", "", sStartURL), fUseJSSafeString);
                if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY.ToLower()))
                    sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY, CreateEvaluateSignupURL(tProject, false, "n", "", sStartURL), fUseJSSafeString);
                // D1060 + D1830 ==
            }

            if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_IP.ToLower()))
                sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_IP, context.Request.UserHostAddress, fUseJSSafeString);
            // D2413

            if(sRes.ToLower().Contains(ECWeb.Options._TEMPL_PSWLOCK_TIMEOUT.ToLower()))
                sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_PSWLOCK_TIMEOUT, Consts._DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT.ToString(), fUseJSSafeString); // D6062

            if (sRes.ToLower().Contains(ECWeb.Options._TEMPL_PSWLOCK_ATTEMPTS.ToLower()))
                sRes = ParseTemplate(sRes, ECWeb.Options._TEMPL_PSWLOCK_ATTEMPTS, Consts._DEF_PASSWORD_ATTEMPTS.ToString(), fUseJSSafeString);

            // D2427 ===
            //If sRes.Contains("%%") AndAlso App.isRiskEnabled AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.WordingTemplates IsNot Nothing Then
            // D2442
            if (sRes.Contains("%%") && App.ActiveWorkgroup != null && App.ActiveWorkgroup.WordingTemplates != null)
            {
                //A0954 ===
                string lres = sRes.ToLower();
                if (App.HasActiveProject() && App.ActiveProject != null && App.ActiveProject.ProjectManager.PipeParameters.ProjectType == CanvasTypes.ProjectType.ptOpportunities && (lres.Contains(Consts._TPL_RISK_VULNERABILITIES) || lres.Contains(Consts._TPL_RISK_VULNERABILITY) || lres.Contains(Consts._TPL_RISK_EVENT) || lres.Contains(Consts._TPL_RISK_EVENTS) || lres.Contains(Consts._TPL_RISK_CONTROL) || lres.Contains(Consts._TPL_RISK_CONTROLS) || lres.Contains(Consts._TPL_RISK_RISK) || lres.Contains(Consts._TPL_RISK_RISKS)))
                {
                    sRes = ParseTemplate(sRes, Consts._TPL_RISK_VULNERABILITY, App.ResString("templ_vulnerability_for_opportunity"), fUseJSSafeString);
                    sRes = ParseTemplate(sRes, Consts._TPL_RISK_VULNERABILITIES, App.ResString("templ_vulnerabilities_for_opportunity"), fUseJSSafeString);
                    sRes = ParseTemplate(sRes, Consts._TPL_RISK_EVENT, App.ResString("templ_event_for_opportunity"), fUseJSSafeString);
                    sRes = ParseTemplate(sRes, Consts._TPL_RISK_EVENTS, App.ResString("templ_events_for_opportunity"), fUseJSSafeString);
                    sRes = ParseTemplate(sRes, Consts._TPL_RISK_CONTROL, App.ResString("templ_control_for_opportunity"), fUseJSSafeString);
                    sRes = ParseTemplate(sRes, Consts._TPL_RISK_CONTROLS, App.ResString("templ_controls_for_opportunity"), fUseJSSafeString);
                    sRes = ParseTemplate(sRes, Consts._TPL_RISK_RISK, App.ResString("templ_risk_for_opportunity"), fUseJSSafeString);
                    sRes = ParseTemplate(sRes, Consts._TPL_RISK_RISKS, App.ResString("templ_risks_for_opportunity"), fUseJSSafeString);

                }
                //A0954 ==
                
                foreach (string sName in App.ActiveWorkgroup.WordingTemplates.Keys)
                {
                    var value = "";
                    App.ActiveWorkgroup.WordingTemplates.TryGetValue(sName, out value);
                    if (value.Contains("!"))
                        value = value.Replace("%%" , " ");
                    if (App.ActiveWorkgroup.WordingTemplates[sName].Contains("tpl_comparion_alternative"))
                    {
                        value = ResString("lblWordingTemplatesAlternative");
                    }
                    if (App.ActiveWorkgroup.WordingTemplates[sName].Contains("tpl_comparion_alternatives"))
                    {
                        value = ResString("lblWordingTemplatesAlternatives");
                    }
                    if (App.ActiveWorkgroup.WordingTemplates[sName].Contains("tpl_comparion_objective"))
                    {
                        value = ResString("lblWordingTemplatesObjective");
                    }
                    if (App.ActiveWorkgroup.WordingTemplates[sName].Contains("tpl_comparion_objectives"))
                    {
                        value = ResString("lblWordingTemplatesObjectives");
                    }
                    if (sName.Contains("%%"))
                        if (sRes.ToLower().Contains(sName.ToLower()))
                        {
                            sRes = ParseTemplate(sRes, sName, value, fUseJSSafeString);
                        }
                }
            }
            // D2427 ==

            // D2368 ===
            string[] _TPL_ALT_OBJ = {
        ECWeb.Options._TEMPL_ALTERNATIVES,
        ECWeb.Options._TEMPL_ALTERNATIVE,
        ECWeb.Options._TEMPL_OBJECTIVES,
        ECWeb.Options._TEMPL_OBJECTIVE
	};
            if (sRes.Contains("%%"))
            {
                foreach (string sTempl in _TPL_ALT_OBJ)
                {
                    if (sRes.ToLower().Contains(sTempl.ToLower()))
                        return PrepareTask(sRes);
                }
            }
            // D2368 ==

            return sRes;
        }


        public static string CreateResetPswURL(clsApplicationUser tUser, string sOtherParams, string sPagePath)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            string sURL = "";
            if (tUser != null && !tUser.CannotBeDeleted)
            {
                App.DBTinyURLDelete(-1, -2, tUser.UserID);
                sURL = string.Format("{0}=resetpsw&ue={1}&up={2}&t={3}", ECWeb.Options._PARAM_ACTION, tUser.UserEMail, tUser.UserPassword, DateTime.Now.Ticks);
                sURL = CryptService.EncodeURL(sURL, App.DatabaseID);
                sURL = string.Format("{0}Password.aspx?{2}={1}", sPagePath, App.CreateTinyURL(sURL, -2, tUser.UserID), ECWeb.Options._PARAMS_TINYURL[0]);
            }
            return sURL;
        }

        public static string CreateEvaluateSignupURL(clsProject tProject, bool fIsAnonymous, string sSignupMode, string sOtherParams, string sPagePath)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            string sURL = "";
            if (tProject == null)
                return sURL;
            sURL += string.Format("&{0}=1&{1}={2}&{3}={4}&{5}={6}", ECWeb.Options._PARAMS_SIGNUP[0], ECWeb.Options._PARAMS_ANONYMOUS_SIGNUP[0], (fIsAnonymous ? "1" : "0"), ECWeb.Options._PARAM_PASSCODE, HttpUtility.UrlEncode(App.ActiveProject.Passcode), ECWeb.Options._PARAMS_SIGNUP_MODE[0], sSignupMode);
            // D1465
            if (!string.IsNullOrEmpty(sOtherParams))
            {
                if (!string.IsNullOrEmpty(sURL))
                    sURL += "&";
                sURL += sOtherParams;
            }
            sURL = CryptService.EncodeURL(sURL, App.DatabaseID);
            if (App.Options.UseTinyURL)
            {
                int PID = -1;
                if (tProject != null)
                    PID = tProject.ID;
                sURL = string.Format("{0}?{2}={1}", sPagePath, App.CreateTinyURL(sURL, PID, -1), ECWeb.Options._PARAMS_TINYURL[0]);
            }
            else
            {
                sURL = string.Format("{0}?{2}={1}", sPagePath, sURL, ECWeb.Options._PARAMS_KEY[0]);
            }
            return sURL;
        }

        #endregion

        #region clusterphrase
        public static bool SetClusterPhraseForNode(Guid NodeID, string sClusterPhrase, bool IsMulti, Guid tAdditionlGuid = new Guid())
        {
            var fResult = false;
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            // D2751
            var _with1 = App.ActiveProject.ProjectManager;
            Attributes.clsAttribute attr = default(Attributes.clsAttribute);
            if (IsMulti)
            {
                attr = _with1.Attributes.GetAttributeByID(Attributes.ATTRIBUTE_CLUSTER_PHRASE_MULTI_ID);
                if (attr != null)
                {
                    _with1.Attributes.SetAttributeValue(Attributes.ATTRIBUTE_CLUSTER_PHRASE_MULTI_ID, ECTypes.UNDEFINED_USER_ID, Attributes.AttributeValueTypes.avtString, sClusterPhrase, NodeID, tAdditionlGuid);
                    fResult = _with1.Attributes.WriteAttributeValues(Attributes.AttributesStorageType.astStreamsDatabase, _with1.StorageManager.ProjectLocation, _with1.StorageManager.ProviderType, _with1.StorageManager.ModelID, ECTypes.UNDEFINED_USER_ID);
                }
            }
            else
            {
                attr = _with1.Attributes.GetAttributeByID(Attributes.ATTRIBUTE_CLUSTER_PHRASE_ID);
                if (attr != null)
                {
                    _with1.Attributes.SetAttributeValue(Attributes.ATTRIBUTE_CLUSTER_PHRASE_ID, ECTypes.UNDEFINED_USER_ID, Attributes.AttributeValueTypes.avtString, sClusterPhrase, NodeID, tAdditionlGuid);
                    fResult = _with1.Attributes.WriteAttributeValues(Attributes.AttributesStorageType.astStreamsDatabase, _with1.StorageManager.ProjectLocation, _with1.StorageManager.ProviderType, _with1.StorageManager.ModelID, ECTypes.UNDEFINED_USER_ID);
                }
            }
            return fResult;
        }
        #endregion

        public static string GetWRTNodeNameWithPath(clsNode tNode, bool CanBeInteractive)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            string sName = "";
            if (tNode != null)
            {
                sName = StringFuncs.JS_SafeHTML(tNode.NodeName);
                string sDivider = StringFuncs.JS_SafeHTML(App.ResString("lblObjectivePathDivider"));
                string sPath = "";
                while (tNode.get_ParentNode() != null)
                {
                    if(tNode.get_ParentNode().get_ParentNode() != null)
                    {
                        sPath = StringFuncs.JS_SafeHTML(tNode.get_ParentNode().NodeName) + sDivider + sPath;
                    }
                    tNode = tNode.get_ParentNode();
                }

                PipeParameters.ecShowObjectivePath objectivePath = App.ActiveProject.PipeParameters.ShowFullObjectivePath;

                if (objectivePath == PipeParameters.ecShowObjectivePath.CollapsePath && CanBeInteractive && !string.IsNullOrEmpty(sPath))
                {
                    if (sOldWRTPath == null)
                        sOldWRTPath = get_SessVar(SESS_WRT_PATH);
                    //Dim fCanSee As Boolean = App.ActiveProject.PipeParameters.ShowFullObjectivePath OrElse Not String.Equals(sPath, sOldWRTPath)
                    bool fCanSee = !string.Equals(sPath, sOldWRTPath);
                    sName = string.Format("<span id='wrtCollapsePath' style='cursor:pointer;' onmouseover =\"this.title='{3}';\" onclick='ToggleWRTPath();' class='wrt_link'><span id='wrt_path' class='wrt_path'{2}>{0}</span>{1}</span>", sPath, sName, (fCanSee ? "" : " style='display:none;'"), StringFuncs.JS_SafeString((sPath + sName).Replace("\"", "&#39;")));
                }
                else if (objectivePath == PipeParameters.ecShowObjectivePath.AlwaysShowFull)
                {
                    sName = sPath + sName;
                }
            }

            return sName;
        }


        public static ecPipeStepStatus getStepStatus(clsAction Action)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            ecPipeStepStatus fStatus = ecPipeStepStatus.psNoJudgments;
            //Return fStatus

            clsNode ParentNode = null;
            clsNode FirstNode = null;
            clsNode SecondNode = null;
            clsRating FirstRating = null;
            clsRating SecondRating = null;
            clsNode ChildNode = null;
            clsHierarchy ObjHierarchy = App.ActiveProject.ProjectManager.get_Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy);
            clsHierarchy AltsHierarchy = App.ActiveProject.ProjectManager.get_AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy);
            if (App.ActiveProject.isTeamTime)
            {
                ObjHierarchy = TeamTime.ProjectManager.get_Hierarchy(TeamTime.ProjectManager.ActiveHierarchy);
                AltsHierarchy = TeamTime.ProjectManager.get_AltsHierarchy(TeamTime.ProjectManager.ActiveAltsHierarchy);
            }

            if (Action != null && Action.ActionData != null && TeamTimeUsersList != null)
            {
                switch (Action.ActionType)
                {
                    case ActionType.atPairwise:
                        {
                            clsPairwiseMeasureData ActionData = (clsPairwiseMeasureData)Action.ActionData;
                            ParentNode = ObjHierarchy.GetNodeByID(ActionData.ParentNodeID);
                            // D2946
                            if (ParentNode != null)
                            {
                                if (ParentNode.IsTerminalNode)
                                {
                                    FirstNode = AltsHierarchy.GetNodeByID(ActionData.FirstNodeID);
                                    SecondNode = AltsHierarchy.GetNodeByID(ActionData.SecondNodeID);
                                }
                                else
                                {
                                    FirstNode = ObjHierarchy.GetNodeByID(ActionData.FirstNodeID);
                                    SecondNode = ObjHierarchy.GetNodeByID(ActionData.SecondNodeID);
                                }
                            }
                        }
                        break;
                    case ActionType.atPairwiseOutcomes:
                        {
                            clsPairwiseMeasureData ActionData = (clsPairwiseMeasureData)Action.ActionData;
                            // D2946
                            if (Action.PWONode != null)
                            {
                                clsRatingScale RS = (clsRatingScale)Action.PWONode.MeasurementScale;
                                // D2946
                                if (RS != null)
                                {
                                    ParentNode = Action.ParentNode;
                                    FirstRating = RS.GetRatingByID(ActionData.FirstNodeID);
                                    SecondRating = RS.GetRatingByID(ActionData.SecondNodeID);
                                }
                            }
                        }
                        break;
                    case ActionType.atAllPairwise:
                    case ActionType.atAllPairwiseOutcomes:
                        {
                            clsAllPairwiseEvaluationActionData AllPwData = (clsAllPairwiseEvaluationActionData)Action.ActionData;
                            ParentNode = ObjHierarchy.GetNodeByID(AllPwData.ParentNode.NodeID);
                        }
                        break;
                    case ActionType.atNonPWOneAtATime:
                        {
                            clsOneAtATimeEvaluationActionData ActionData = (clsOneAtATimeEvaluationActionData)Action.ActionData;

                            // D2946
                            if (ActionData.Node != null && ActionData.Judgment != null)
                            {
                                switch (((clsNonPairwiseEvaluationActionData)Action.ActionData).MeasurementType)
                                {
                                    case ECMeasureType.mtRatings:
                                        clsNonPairwiseMeasureData nonpwData = (clsNonPairwiseMeasureData)ActionData.Judgment;
                                        if (ActionData.Node.IsAlternative)
                                        {
                                            ParentNode = ObjHierarchy.Nodes[0];
                                            ChildNode = ActionData.Node;
                                        }
                                        else
                                        {
                                            ParentNode = ActionData.Node;
                                            if (ParentNode != null && ParentNode.IsTerminalNode)
                                                ChildNode = AltsHierarchy.GetNodeByID(nonpwData.NodeID);
                                            else
                                                ChildNode = ObjHierarchy.GetNodeByID(nonpwData.NodeID);
                                            // D2946
                                        }

                                        break;
                                    case ECMeasureType.mtDirect:
                                        ParentNode = ActionData.Node;
                                        clsDirectMeasureData DirectData = (clsDirectMeasureData)ActionData.Judgment;
                                        if (ParentNode.IsTerminalNode)
                                            ChildNode = AltsHierarchy.GetNodeByID(DirectData.NodeID);
                                        else
                                            ChildNode = ObjHierarchy.GetNodeByID(DirectData.NodeID);

                                        break;
                                    case ECMeasureType.mtStep:
                                        ParentNode = ActionData.Node;
                                        clsStepMeasureData StepData = (clsStepMeasureData)ActionData.Judgment;
                                        if (ParentNode.IsTerminalNode)
                                            ChildNode = AltsHierarchy.GetNodeByID(StepData.NodeID);
                                        else
                                            ChildNode = ObjHierarchy.GetNodeByID(StepData.NodeID);

                                        break;
                                    case ECMeasureType.mtRegularUtilityCurve:
                                        ParentNode = ActionData.Node;
                                        clsUtilityCurveMeasureData UCData = (clsUtilityCurveMeasureData)ActionData.Judgment;
                                        if (ParentNode.IsTerminalNode)
                                            ChildNode = AltsHierarchy.GetNodeByID(UCData.NodeID);
                                        else
                                            ChildNode = ObjHierarchy.GetNodeByID(UCData.NodeID);

                                        break;
                                }
                            }
                        }
                        break;

                    case ActionType.atNonPWAllChildren:
                    case ActionType.atNonPWAllCovObjs:
                        {
                            if (Action.ActionData is clsAllChildrenEvaluationActionData)
                            {

                                clsAllChildrenEvaluationActionData ActionData2 = (clsAllChildrenEvaluationActionData)Action.ActionData;
                                ParentNode = ActionData2.ParentNode;
                            }

                            if (Action.ActionData is clsAllCoveringObjectivesEvaluationActionData)
                            {

                                clsAllCoveringObjectivesEvaluationActionData ActionData2 = (clsAllCoveringObjectivesEvaluationActionData)Action.ActionData;
                                ParentNode = ActionData2.Alternative;
                            }

                        }
                        break;
                }

                bool fHasJudgments = false;
                bool fHasUndefined = false;
                foreach (ECTypes.clsUser tUser in TeamTimeUsersList)
                {
                    // D2946
                    if (tUser != null && IsUserJudgmentAllowed(Action, tUser))
                    {
                        switch (Action.ActionType)
                        {

                            case ActionType.atPairwise:
                                if (ParentNode != null && FirstNode != null && SecondNode != null)
                                {
                                    clsPairwiseMeasureData pwData = ((clsPairwiseJudgments)ParentNode.Judgments).PairwiseJudgment(FirstNode.NodeID, SecondNode.NodeID, tUser.UserID);
                                    if (pwData == null || pwData.IsUndefined)
                                        fHasUndefined = true;
                                    else
                                        fHasJudgments = true;
                                }

                                break;
                            case ActionType.atPairwiseOutcomes:
                                // D2946
                                if (ParentNode != null && FirstRating != null && SecondRating != null && ParentNode.PWOutcomesJudgments != null)
                                {
                                    clsPairwiseMeasureData pwData = ParentNode.PWOutcomesJudgments.PairwiseJudgment(FirstRating.ID, SecondRating.ID, tUser.UserID, Action.PWONode.NodeID);
                                    if (pwData == null || pwData.IsUndefined)
                                        fHasUndefined = true;
                                    else
                                        fHasJudgments = true;
                                }

                                break;
                            case ActionType.atAllPairwise:
                            case ActionType.atAllPairwiseOutcomes:
                                {
                                    clsAllPairwiseEvaluationActionData AllPwData = (clsAllPairwiseEvaluationActionData)Action.ActionData;
                                    foreach (clsPairwiseMeasureData pwData in AllPwData.Judgments)
                                    {
                                        if (pwData.Value > 0 )
                                        {
                                            fHasJudgments = true;
                                        }
                                        if (pwData.Value == 0)
                                        {
                                            fHasUndefined = true;
                                        }
                                    }
                                }
                                break;
                            case ActionType.atNonPWAllChildren:
                            case ActionType.atNonPWAllCovObjs:
                                {
                                    if (Action.ActionData is clsAllChildrenEvaluationActionData)
                                    {


                                        clsAllChildrenEvaluationActionData AllChildData = (clsAllChildrenEvaluationActionData)Action.ActionData;
                                        switch (AllChildData.MeasurementType)
                                        {
                                            case ECMeasureType.mtRatings:
                                                {
                                                    clsNonPairwiseMeasureData nonpwData = null;
                                                    nonpwData = AllChildData.GetJudgment(ParentNode);

                                                    if (nonpwData == null || nonpwData.IsUndefined)
                                                        fHasUndefined = true;
                                                    else
                                                        fHasJudgments = true;
                                                }
                                                break;
                                            case ECMeasureType.mtDirect:
                                                clsDirectMeasureData DirectData = null;
                                                // D3323 ===
                                                if (ParentNode.Judgments != null)
                                                {
                                                    var temp = (clsNonPairwiseMeasureData)AllChildData.GetJudgment(ParentNode);
                                                    DirectData = (clsDirectMeasureData)temp;
                                                }
                                                //DirectData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                                // D3323 ==
                                                if (DirectData == null || DirectData.IsUndefined)
                                                    fHasUndefined = true;
                                                else
                                                    fHasJudgments = true;

                                                break;
                                        }

                                    }

                                    if (Action.ActionData is clsAllCoveringObjectivesEvaluationActionData)
                                    {


                                        clsAllCoveringObjectivesEvaluationActionData AllChildData = (clsAllCoveringObjectivesEvaluationActionData)Action.ActionData;
                                        switch (AllChildData.MeasurementType)
                                        {
                                            case ECMeasureType.mtRatings:
                                                {
                                                    clsNonPairwiseMeasureData nonpwData = null;
                                                    nonpwData = AllChildData.GetJudgment(ParentNode);

                                                    if (nonpwData == null || nonpwData.IsUndefined)
                                                        fHasUndefined = true;
                                                    else
                                                        fHasJudgments = true;
                                                }
                                                break;
                                            case ECMeasureType.mtDirect:
                                                clsDirectMeasureData DirectData = null;
                                                // D3323 ===
                                                if (ParentNode.Judgments != null)
                                                {
                                                    var temp = (clsNonPairwiseMeasureData)AllChildData.GetJudgment(ParentNode);
                                                    DirectData = (clsDirectMeasureData)temp;
                                                }
                                                //DirectData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                                // D3323 ==
                                                if (DirectData == null || DirectData.IsUndefined)
                                                    fHasUndefined = true;
                                                else
                                                    fHasJudgments = true;

                                                break;
                                        }

                                    }

                                        
                                }
                                break;
                            case ActionType.atNonPWOneAtATime:
                                // D2946

                                if (ParentNode != null && ChildNode != null && Action.ActionData != null)
                                {
                                    clsOneAtATimeEvaluationActionData ActionData = (clsOneAtATimeEvaluationActionData)Action.ActionData;
                                    switch (((clsNonPairwiseEvaluationActionData)Action.ActionData).MeasurementType)
                                    {

                                        case ECMeasureType.mtRatings:
                                            {
                                                clsNonPairwiseMeasureData nonpwData = null;
                                                if (ActionData.Node.IsAlternative)
                                                {
                                                    if (ChildNode.DirectJudgmentsForNoCause != null)
                                                        nonpwData = ((clsNonPairwiseJudgments)ChildNode.DirectJudgmentsForNoCause).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID);
                                                    // D2946
                                                }
                                                else {
                                                    if (ParentNode.Judgments != null)
                                                        nonpwData = ((clsNonPairwiseJudgments)ParentNode.Judgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID);
                                                    // D2946
                                                }
                                                if (nonpwData == null || nonpwData.IsUndefined)
                                                    fHasUndefined = true;
                                                else
                                                    fHasJudgments = true;
                                            }
                                            break;
                                        case ECMeasureType.mtDirect:
                                            clsDirectMeasureData DirectData = null;
                                            // D3323 ===
                                            if (ParentNode.Judgments != null)
                                            {
                                                if (ActionData.Node.IsAlternative)
                                                {
                                                    var temp  = ((clsNonPairwiseJudgments)ChildNode.DirectJudgmentsForNoCause).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID);
                                                    DirectData = (clsDirectMeasureData)temp;
                                                }
                                                else {
                                                    var temp  = ((clsNonPairwiseJudgments)ParentNode.Judgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID);
                                                    DirectData = (clsDirectMeasureData)temp;
                                                }
                                            }
                                            //DirectData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                            // D3323 ==
                                            if (DirectData == null || DirectData.IsUndefined)
                                                fHasUndefined = true;
                                            else
                                                fHasJudgments = true;

                                            break;
                                        case ECMeasureType.mtStep:
                                            clsStepMeasureData StepData = null;
                                            // D3323 ===
                                            if (ParentNode.Judgments != null)
                                            {
                                                if (ActionData.Node.IsAlternative)
                                                {
                                                    var temp = ((clsNonPairwiseJudgments)ChildNode.DirectJudgmentsForNoCause).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID);
                                                    StepData = (clsStepMeasureData)temp;
                                                }
                                                else
                                                {
                                                    var temp = ((clsNonPairwiseJudgments)ParentNode.Judgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID);
                                                    StepData = (clsStepMeasureData)temp;
                                                }
                                            }
                                            //StepData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                            // D3323 ==
                                            if (StepData == null || StepData.IsUndefined)
                                                fHasUndefined = true;
                                            else
                                                fHasJudgments = true;

                                            break;
                                        case ECMeasureType.mtRegularUtilityCurve:
                                            clsUtilityCurveMeasureData UCData = null;
                                            // D3323 ===
                                            if (ParentNode.Judgments != null)
                                            {
                                                if (ActionData.Node.IsAlternative)
                                                {
                                                    var temp = ((clsNonPairwiseJudgments)ChildNode.DirectJudgmentsForNoCause).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID);
                                                    UCData = (clsUtilityCurveMeasureData)temp;
                                                }
                                                else {
                                                    var temp = ((clsNonPairwiseJudgments)ParentNode.Judgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID);
                                                    UCData = (clsUtilityCurveMeasureData)temp;
                                                }
                                            }
                                            //UCData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                            // D3323 ==
                                            if (UCData == null || UCData.IsUndefined)
                                                fHasUndefined = true;
                                            else
                                                fHasJudgments = true;

                                            break;
                                    }
                                }

                                break;
                        }
                    }

                }

                if (fHasJudgments && !fHasUndefined)
                    fStatus = ecPipeStepStatus.psAllJudgments;
                if (fHasJudgments && fHasUndefined)
                    fStatus = ecPipeStepStatus.psMissingJudgments;
                if (!fHasJudgments && fHasUndefined)
                    fStatus = ecPipeStepStatus.psNoJudgments;

            }
            return fStatus;
        }

        public static Boolean IsUserJudgmentAllowed(clsAction Action, ECTypes.clsUser tUser)
        {
            return tUser != null && (tUser.SyncEvaluationMode == ECTypes.SynchronousEvaluationMode.semOnline || tUser.SyncEvaluationMode == ECTypes.SynchronousEvaluationMode.semVotingBox) && Action != null && TeamTime.GetStepAvailabilityForUser(Action.ActionType, Action, tUser) && Action != null && TeamTime.GetStepAvailabilityForUser(Action.ActionType, Action, tUser);
        }





        public static void CheckProjectStatus()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            clsProject tmpPrj = App.DBProjectByID(App.ProjectID);
            if (tmpPrj != null)
            {
                // D1246 ===
                if (isTeamTimeOwner)
                {
                    // D3207
                    if (!tmpPrj.isOnline)
                    {
                        tmpPrj.isOnline = true;
                        App.ActiveProject.isOnline = true;
                        App.DBProjectUpdate(ref tmpPrj, false, "Make project on-line");
                        App.ActiveProject.LastModify = DateTime.Now;
                    }
                    if (tmpPrj.LockInfo != null && (tmpPrj.LockInfo.LockStatus == ECLockStatus.lsLockForModify | tmpPrj.LockInfo.LockStatus == ECLockStatus.lsLockForAntigua))
                    {
                        var User = App.ActiveUser;
                        var LockInfo = tmpPrj.LockInfo;
                        if (App.DBProjectLockInfoWrite(ECLockStatus.lsLockForTeamTime, ref LockInfo, ref User, DateTime.Now.AddMinutes(Consts._DEF_LOCK_TT_SESSION_TIMEOUT)))
                            App.ActiveProject.LockInfo = tmpPrj.LockInfo;
                    }
                }
                // D1246 ==
                if (tmpPrj.StatusDataLikelihood != App.ActiveProject.StatusDataLikelihood)
                    App.ActiveProject.StatusDataLikelihood = tmpPrj.StatusDataLikelihood;
                // D1944
                if (tmpPrj.StatusDataImpact != App.ActiveProject.StatusDataImpact)
                    App.ActiveProject.StatusDataImpact = tmpPrj.StatusDataImpact;
                // D1944
                if (tmpPrj.LastModify > App.ActiveProject.LastModify)
                {
                    if (!isTeamTimeOwner)
                        //ResetProject();
                        App.ActiveProject.LastModify = tmpPrj.LastModify;
                }
            }
            // D1284 ===
            if (isTeamTime && isTeamTimeOwner)
            {
                // D3066
                if (TeamTime != null && (TeamTime.ProjectManager.StorageManager.ModelID != App.ActiveProject.ProjectManager.StorageManager.ModelID || (TeamTime.ProjectManager.LastModifyTime.HasValue && App.ActiveProject.ProjectManager.LastModifyTime.HasValue && TeamTime.ProjectManager.LastModifyTime.Value.AddSeconds(1) < App.ActiveProject.ProjectManager.LastModifyTime)))
                {
                    TeamTime = null;
                }
            }
            // D1284 ==
        }

    }
}