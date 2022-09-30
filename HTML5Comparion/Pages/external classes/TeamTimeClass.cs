using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using ECCore;
using ECSecurity;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Service;
using ExpertChoice.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using AnytimeComparion.Pages.TeamTimeTest;

namespace AnytimeComparion.Pages.external_classes
{
    public class TeamTimeClass 
    {
        public static clsAction Action;
        public static string sCallbackSave = "";
        public static bool _TeamTimeRefreshTimeoutLoaded = false;
        public static int _TeamTimeRefreshTimeout = WebOptions.SynchronousRefresh();

        internal const string _SESS_VARIANCE = "tt_variance";
        internal const string _SESS_PW_SIDE = "tt_pw_side";
        internal const string _SESS_HIDE_COMBINED = "tt_no_combined";
        internal const string _COOKIE_USERSPAGE = "tt_pg";
        internal const string _COOKIE_USERS_PAGES_USE = "tt_up";
        internal const string _COOKIE_USERS_PAGES_SIZE = "tt_ps";
        internal const string _COOKIE_TT_NORM = "tt_res_norm";

        private const int _TeamTimeSessionDataTimeoutCoeff = 10;
        public static int _TeamTimePipeStep = -1;

        public static bool _TeamTimeActive = true;
        internal const int ShowStepsCount = 11;

        public static string globalGUID = "";

        private static clsTeamTimePipe _TeamTime_Pipe = null;
        public const string _SESS_TT_Pipe = "Sess_TT_Pipe";
        private const string _SESS_STEPS_LIST = "tt_steps_list";
        private static bool _ForceSaveStepInfo = false;

        public static clsTeamTimePipe TeamTime
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    if (_TeamTime_Pipe == null)
                    {
                        if (context.Session[_SESS_TT_Pipe] != null)
                        {
                            _TeamTime_Pipe = (clsTeamTimePipe)context.Session[_SESS_TT_Pipe];
                            if ((_TeamTime_Pipe.ProjectManager.StorageManager.ModelID != App.ProjectID) || (_TeamTime_Pipe.ProjectManager.ActiveHierarchy != App.ActiveProject.ProjectManager.ActiveHierarchy))
                            {
                                _TeamTime_Pipe = null;
                            }
                        }

                        if (_TeamTime_Pipe == null)
                        {
                            _TeamTime_Pipe = new clsTeamTimePipe(App.ActiveProject.ProjectManager, App.ActiveProject.ProjectManager.User);
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

        public static List<ECTypes.clsUser> TeamTimeUsersList
        {
            get
            {
                HttpContext context = HttpContext.Current;
                List<ECTypes.clsUser> userList = new List<ECTypes.clsUser>();
                bool canParticipate = true;
                bool update = false;
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    List<clsWorkspace> WorkSpaceList = App.DBWorkspacesByProjectID(App.ProjectID);
                    foreach (clsApplicationUser user in UsersList)
                    {
                        clsWorkspace ws = clsWorkspace.WorkspaceByUserIDAndProjectID(user.UserID, App.ProjectID, WorkSpaceList);
                        if (ws != null && clsOnlineUserSession.OnlineSessionByUserID(user.UserID, SessionsList) != null)
                        {
                            ws.set_Status(App.ActiveProject.isImpact, ecWorkspaceStatus.wsSynhronousActive);
                            //not working,send AD an email for this
                            //ws.get_Status(App.ActiveProject.isImpact);
                            //var status = ws.set_Status((ecWorkspaceStatus)App.ActiveProject.isImpact);
                        }

                        if (ws != null && ws.get_isInTeamTime(App.ActiveProject.isImpact))
                        {
                            if (!TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess && ws.get_TeamTimeStatus(App.ActiveProject.isImpact) == ecWorkspaceStatus.wsSynhronousReadOnly)
                            {
                                canParticipate = false;
                            }
                            if (TeamTimePipeParams.TeamTimeHideProjectOwner && App.ActiveProject.MeetingOwnerID == user.UserID)
                            {
                                canParticipate = false;
                            }
                            if (canParticipate)
                            {
                                ECTypes.clsUser projectUser = TeamTime.ProjectManager.GetUserByEMail(user.UserEMail);

                                if (projectUser == null)
                                {
                                    projectUser = TeamTime.ProjectManager.AddUser(user.UserEMail, true, user.UserName);
                                    projectUser.IncludedInSynchronous = true;
                                    projectUser.SyncEvaluationMode = ECTypes.SynchronousEvaluationMode.semOnline;
                                    update = true;
                                }
                                else
                                {
                                    if (!projectUser.IncludedInSynchronous || projectUser.SyncEvaluationMode == ECTypes.SynchronousEvaluationMode.semNone)
                                    {
                                        projectUser.IncludedInSynchronous = true;
                                        if (projectUser.SyncEvaluationMode == ECTypes.SynchronousEvaluationMode.semNone)
                                        {
                                            projectUser.SyncEvaluationMode = ECTypes.SynchronousEvaluationMode.semOnline;
                                            update = true;
                                        }
                                    }
                                }
                                if (update)
                                {
                                    TeamTime.ProjectManager.StorageManager.Writer.SaveUsersInfo(projectUser);
                                }
                                ECTypes.clsUser addUser = projectUser.Clone();
                                addUser.UserName = projectUser.UserName;
                                addUser.Active = clsOnlineUserSession.OnlineSessionByUserID(user.UserID, SessionsList) != null;

                                if (!(!TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess && projectUser != null && projectUser.SyncEvaluationMode == ECTypes.SynchronousEvaluationMode.semByFacilitatorOnly))
                                {
                                    userList.Add(addUser);
                                }
                            }

                        }
                    } //end of foreach


                    ecUserSort field = ecUserSort.usEmail;
                    if (TeamTimePipeParams.TeamTimeUsersSorting == Canvas.CanvasTypes.TTUsersSorting.ttusName)
                    {
                        field = ecUserSort.usName;
                    }
                    else if (TeamTimePipeParams.TeamTimeUsersSorting == Canvas.CanvasTypes.TTUsersSorting.ttusKeypadID)
                    {
                        field = ecUserSort.usKeyPad;
                    }
                    else
                    {
                        field = ecUserSort.usEmail;
                    }

                    clsUserComparer order = new clsUserComparer(field, SortDirection.Ascending);
                    userList.Sort(order);
                }

                return userList;
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
                        if (clsApplicationUser.UserByUserID(onlineUser.UserID, UsersList) != null)
                        {
                            sessionsList.Add(onlineUser);
                        }
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
                    var sss = tAction.StepGuid.ToString();
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
                        if (sCallbackSave != "") { sCallbackSave = String.Format("['pended',[{0}]],", sCallbackSave); }
                    }
                    string sType = "";
                    string sTask = "";
                    string sValue = "";

                    if (tAction != null)
                    {
                        switch (Action.ActionType)
                        {
                            case ActionType.atPairwise:
                                sType = "pairwise";
                                sValue = TeamTime.GetJSON(TeamTimeUsersList);
                                sValue = String.Format(sValue, StringFuncs.JS_SafeString(App.ResString((App.isRiskEnabled && isImpact ? "lblKnownImpactPW" : "lblKnownLikelihoodPW").ToString())));
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
                            //sTask = ss.GetPipeStepTask(tAction, null, false, false, false);
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
                                sEvalProgress += sEvalProgress.Equals("") ? "" : "," + String.Format("['{0}',{1},{2},'{3}']", StringFuncs.JS_SafeString(tUser.UserEMail), TeamTime.GetUserEvaluatedCount(tUser), TeamTime.GetUserTotalCount(tUser), sTime);
                            }
                        }
                        Dictionary<string, string> tParams = default(Dictionary<string, string>);
                        tParams = null;
                        var _with1 = TeamTimePipeParams;
                        sData = string.Format("['active',{0}],['progress',{1},{2},[{3}]],['options',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}],['data','{15}','{16}','{17}',{18}]",
                                (isTeamTime ? 1 : 0), TeamTimePipeStep, TeamTime.Pipe.Count, sEvalProgress, TeamTimeRefreshTimeout,
                                (_with1.ShowComments ? 1 : 0), (_with1.ShowInfoDocs ? Convert.ToInt32(_with1.ShowInfoDocsMode) : -1),
                                (_with1.SynchStartInPollingMode ? 1 : 0), (_with1.TeamTimeStartInAnonymousMode ? 1 : 0),
                                (_with1.SynchUseVotingBoxes ? 1 : 0), Convert.ToInt32(_with1.TeamTimeUsersSorting), (TeamTimeShowVarianceInPollingMode ? 1 : 0),
                                (TeamTimeShowPWSideKeypadsInPollingMode ? 1 : 0), (_with1.TeamTimeDisplayUsersWithViewOnlyAccess ? 1 : 0),
                                (_with1.TeamTimeHideProjectOwner ? 1 : 0), sType, StringFuncs.JS_SafeString(PrepareTask(sTask, null, false, tParams)), tAction.StepGuid.ToString(), sValue);

                        App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, sCallbackSave + sData);
                        sCallbackSave = "";
                    }
                    else
                    {
                        sData = "['action', 'refresh']";
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
                        sData = string.Format("['warning','{0}']", StringFuncs.JS_SafeString(sMsg));
                }

                if (App.ActiveUser.Session != null)
                {
                    if (App.ActiveUser.Session.LastAccess.HasValue && App.ActiveUser.Session.LastAccess.Value.AddSeconds(Consts._DEF_SESS_TIMEOUT / 3) < DateTime.Now)
                    {
                        App.ActiveUser.Session.LastAccess = DateTime.Now;
                        App.DBUpdateDateTime(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_LASTVISITED, App.ActiveUser.UserID, clsComparionCore._FLD_USERS_ID);
                    }
                }

                clsAction tActions = Action;
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
                    if (sCallbackSave != "") { sCallbackSave = String.Format("['pended',[{0}]],", sCallbackSave); }
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
                if (_TeamTimePipeStep < 1)
                {
                    if (isTeamTimeOwner)
                    {
                        _TeamTimePipeStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
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
                else
                {
                    object DBPipeStep = App.Database.ExecuteScalarSQL(String.Format("SELECT Step from Workspace where ProjectID={0} and UserID = {1}", App.ActiveProject.ID, App.ActiveProject.MeetingOwnerID));
                    _TeamTimePipeStep = Convert.ToInt32(DBPipeStep);
                }
                return _TeamTimePipeStep;
            }
            set
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                if (isTeamTimeOwner && _TeamTimePipeStep != value)
                {
                    _TeamTimePipeStep = value;
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
            clsAction tAction = Action;
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
                        _TeamTimeRefreshTimeout = WebOptions.SynchronousRefresh();

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
            return clsComparionCorePage.PrepareTaskCommon(App, sTask, tExtraParam, fHasSubNodes);
        }

        public static clsAction GetAction(int stepNumber)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            if (stepNumber < 1 | stepNumber > TeamTime.Pipe.Count | TeamTime.Pipe.Count < 1)
                return null;
            else
                return (clsAction)TeamTime.Pipe[stepNumber - 1];
        }

        public static string getTeamTimeData()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            Nullable<DateTime> DT = null;
            var sdata = App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, ref  DT);

            return sdata;
        }

        public static List<string> getnodes(int step)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var Project = App.ActiveProject;
            clsAction tAction = GetAction(step);
            var ParentNode = (clsNode)null;
            var FirstNode = (clsNode)null;
            var SecondNode = (clsNode)null;
            var FirstRating = (clsNode)null;
            var SecondRating = (clsNode)null;
            var ChildNode = (clsNode)null;
            var ObjHierarchy = (clsHierarchy)Project.HierarchyObjectives;
            var AltsHierarchy = (clsHierarchy)Project.HierarchyAlternatives;
            var wrtleft = "";
            var wrtright = "";
            var leftnodeinfo = "";
            var rightnodeinfo = "";
            if (tAction != null)
            {
                switch (tAction.ActionType)
                {
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

                        var pat = context.Server.MapPath("~/");
                        Consts._FILE_MHT_FILES = System.IO.Path.GetFullPath(System.IO.Path.Combine(pat, "../../Application/DocMedia/MHTFiles/"));
                        leftnodeinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, FirstNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, FirstNode.NodeID.ToString(), FirstNode.InfoDoc, true, false, -1);
                        rightnodeinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, SecondNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, SecondNode.NodeID.ToString(), SecondNode.InfoDoc, true, false, -1);
                        wrtleft = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, FirstNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(FirstNode.NodeGuidID, ParentNode.NodeGuidID), false, false, ParentNode.NodeID);
                        wrtright = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, SecondNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(SecondNode.NodeGuidID, ParentNode.NodeGuidID), false, false, ParentNode.NodeID);

                        break;
                    default:
                        break;
                }
            }

            var list1 = new List<string>();
            list1.Add(leftnodeinfo);
            list1.Add(rightnodeinfo);
            list1.Add(wrtleft);
            list1.Add(wrtright);

            return list1;
        }


    }
}