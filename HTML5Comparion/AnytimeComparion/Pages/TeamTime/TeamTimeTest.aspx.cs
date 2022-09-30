using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ECCore;
using ECSecurity;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;


namespace AnytimeComparion.Pages.TeamTime
{
    public partial class WebForm4 : System.Web.UI.Page
    {
        public clsComparionCore App;
        public clsAction Action;
        public clsNonPairwiseEvaluationActionData measuretype;
        protected void Page_Load(object sender, EventArgs e)
        {
            App = (clsComparionCore)Session["App"];
            var Project = App.ActiveProject;
            var TeamTime_Pipe = new clsTeamTimePipe(App.ActiveProject.ProjectManager, App.ActiveProject.ProjectManager.User);

            TeamTime_Pipe.VerifyUsers(TeamTimeUsersList);
            TeamTime_Pipe.CreatePipe();

            var WorkSpace = App.DBWorkspaceByUserIDProjectID(App.ActiveProject.MeetingOwnerID, App.ProjectID);
            var CurrentStep = WorkSpace.get_ProjectStep(Project.isImpact);
            var TotalSteps = TeamTime_Pipe.Pipe.Count;
            Action = (clsAction)TeamTime_Pipe.Pipe[CurrentStep - 1];

            var ActionType = Action.ActionType;

            var ParentNode = (clsNode)null;
            var FirstNode = (clsNode)null;
            var SecondNode = (clsNode)null;
            var FirstRating = (clsNode)null;
            var SecondRating = (clsNode)null;
            var ChildNode = (clsNode)null;
            var ObjHierarchy = (clsHierarchy)Project.HierarchyObjectives;
            var AltsHierarchy = (clsHierarchy)Project.HierarchyAlternatives;


            switch (Action.ActionType)
            {
                case ActionType.atPairwise:
                    var ActionData = (clsPairwiseMeasureData)Action.ActionData;
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

                    PairWiseComparison2.leftnd = FirstNode.NodeName;
                    PairWiseComparison2.rightnd = SecondNode.NodeName;
                    PairWiseComparison2.goal = ParentNode.NodeName;
                    break;

                case ActionType.atNonPWOneAtATime:
                    var ActionData2 = (clsOneAtATimeEvaluationActionData)Action.ActionData;
                    if (ActionData2.Node != null && ActionData2.Judgment != null)
                    {
                        measuretype = (clsNonPairwiseEvaluationActionData)Action.ActionData;
                        switch (((clsNonPairwiseEvaluationActionData)Action.ActionData).MeasurementType)
                        {
                            case ECMeasureType.mtRatings:
                                clsNonPairwiseMeasureData nonpwdata = (clsNonPairwiseMeasureData)ActionData2.Judgment;
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
                                RatingScale1.alternative = ChildNode.NodeName;
                                RatingScale1.objective = ParentNode.NodeName;
                                break;

                            case ECMeasureType.mtDirect:
                                ParentNode = ActionData2.Node;
                                var DirectData = (clsDirectMeasureData)ActionData2.Judgment;
                                if (ParentNode.IsTerminalNode) { ChildNode = AltsHierarchy.GetNodeByID(DirectData.NodeID); } else { ChildNode = ObjHierarchy.GetNodeByID(DirectData.NodeID); }
                                DirectUser1.alternative = ChildNode.NodeName;
                                DirectUser1.objective = ParentNode.NodeName;
                                DirectUser1.goal = ParentNode.NodeName;
                                break;

                            case ECMeasureType.mtStep:
                                ParentNode = ActionData2.Node;
                                var StepData = (clsStepMeasureData)ActionData2.Judgment;
                                if (ParentNode.IsTerminalNode) { ChildNode = AltsHierarchy.GetNodeByID(StepData.NodeID); } else { ChildNode = ObjHierarchy.GetNodeByID(StepData.NodeID); }
                                break;

                            case ECMeasureType.mtRegularUtilityCurve:
                                ParentNode = ActionData2.Node;
                                var UCData = (clsUtilityCurveMeasureData)ActionData2.Judgment;
                                if (ParentNode.IsTerminalNode) { ChildNode = AltsHierarchy.GetNodeByID(UCData.NodeID); } else { ChildNode = ObjHierarchy.GetNodeByID(UCData.NodeID); };
                                break;
                        }

                    }

                    break;

                default:
                break;
            }

        }


        //start code here

        //private static List<clsApplicationUser> TeamTimeUsersList = null;
        //private static List<clsOnlineUserSession> SessionList = null;
        //private static List<clsApplicationUser> UsersList = null;
        private static clsTeamTimePipe _TeamTime_Pipe = null;
        public const string _SESS_TT_Pipe = "Sess_TT_Pipe" ;
        private const string _SESS_STEPS_LIST = "tt_steps_list" ;
        private static bool _ForceSaveStepInfo = false;
        public static clsTeamTimePipe TeamTime
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    if(_TeamTime_Pipe == null)
                    {
                        if(context.Session[_SESS_TT_Pipe] != null)
                        {
                            _TeamTime_Pipe = (clsTeamTimePipe) context.Session[_SESS_TT_Pipe];
                            if( (_TeamTime_Pipe.ProjectManager.StorageManager.ModelID !=  App.ProjectID) || (_TeamTime_Pipe.ProjectManager.ActiveHierarchy != App.ActiveProject.ProjectManager.ActiveHierarchy ))
                            {
                                 _TeamTime_Pipe = null;
                            }
                        }

                        if(_TeamTime_Pipe == null)
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
                        if (ws != null && clsOnlineUserSession.OnlineSessionByUserID(user.UserID, SessionsList) != null )
                        {   
                            //not working,send AD an email for this
                            //ws.Status(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousActive
                            //ws.StatusImpact = ecWorkspaceStatus.wsSynhronousActive;
                            //ws.get_Status(App.ActiveProject.isImpact);
                            //var status = ws.set_Status((ecWorkspaceStatus)App.ActiveProject.isImpact);
                        }

                        if (ws != null && ws.get_isInTeamTime(App.ActiveProject.isImpact))
                        {
                            if(!TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess && ws.get_TeamTimeStatus(App.ActiveProject.isImpact) == ecWorkspaceStatus.wsSynhronousReadOnly )
                            {
                                canParticipate = false;
                            }
                            if (TeamTimePipeParams.TeamTimeHideProjectOwner && App.ActiveProject.MeetingOwnerID == user.UserID)
                            {
                               canParticipate = false;
                            }
                            if(canParticipate)
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

        public static Canvas.PipeParameters.clsPipeParamaters  TeamTimePipeParams
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                {
                    var App = (clsComparionCore)context.Session["App"];
                    if(isTeamTimeOwner)
                    {
                        return TeamTime.PipeParameters;
                    }
                    else{
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
                    if(isTeamTime)
                    {
                        if(App.ActiveProject.get_MeetingStatus(App.ActiveUser) == ECTeamTimeStatus.tsTeamTimeSessionOwner)
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
                    teamtime = (App.ActiveProject.isTeamTimeLikelihood || App.ActiveProject.isTeamTimeImpact);
                }
                return teamtime;
            }
        }
    }
}