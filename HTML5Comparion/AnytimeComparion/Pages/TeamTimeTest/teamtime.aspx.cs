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
//using ECSecurity;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Service;
using ExpertChoice.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using AnytimeComparion.Pages.TeamTimeTest;
using AnytimeComparion.Pages.external_classes;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;

namespace AnytimeComparion.test
{
    public partial class teamtime : System.Web.UI.Page
    {
        public clsComparionCore App;
        public clsNonPairwiseEvaluationActionData measuretype;

        public InformationPage InformationPage1;
        public ThankYou ThankYou;
        public PairWiseComparison PairWiseComparison2;
        public RatingScale RatingScale1;
        public Pages.JudgmentTemplates.DirectUser DirectUser1;
        public LocalResults LocalResults;
        public WaitingPage WaitingPage;
        public UtilityCurve UtilityCurve;
        public StepFunction StepFunction;
        public GlobalResults GlobalResults;
        
        public static bool[] InfodocDropdown = { false, false, false, false, false };
        public static string[] InfoDocs = { "", "", "", "", "" };
        public static string[,] InfodocsSizes = new string[5, 2];
        private static string[,] P_InfodocsSizes = new string[5, 2];
        public static int PmOverwriteInfoDocs = 1;
        public static int PmHideAndShowInfoDocs = 1;
        public static int indexOfLoggednInUser = 0;
        public static string globalGUID = "";

        public static string Sess_TT_hash_count = "Sess_TT_hash_count";

        private static string CheckTTJSON = "";
        private static string SessHierarchy = "SessHierarchy";
        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["hashed_data"] == null)
            {
                Session["hashed_data"] = "";
            }
            
            if (Session["showtoggle"] == null)
            {
                Session["showtoggle"] = 1;
            }
            if (Session["grouptoggle"] == null)
            {
                Session["grouptoggle"] = 0;
            }
            if (Session["UsersListPagination"] == null)
            {
                int[] paginate = { 1, 5 };
                Session["UsersListPagination"] = paginate;
            }
            if (Session["offline"] == null)
            {
                Session["offline"] = false;
            }
            Session[Sess_TT_hash_count] = 0;

            if(Session["single_line"] == null)
            {
                Session["single_line"] = false;
            }

            Session["isPM"] = TeamTimeClass.isTeamTimeOwner;
            if (IsPostBack)
            {
                CheckTTJSON = "";
            }


            if (App.ActiveProject.isTeamTime)
            {
                    Session["TTstart"] = true;
                if (TeamTimeClass.isTeamTimeOwner)
                {
                    var Project = App.ActiveProject;
                    var TeamTime = TeamTime_Pipe();
                    if (TeamTimeClass.isTeamTimeOwner)
                        TeamTimeClass.TeamTime = TeamTime;


                    List<string> users = new List<String>();
                    HttpContext context = HttpContext.Current;
                    //temporary only
                    foreach (ECTypes.clsUser user in TeamTimeClass.TeamTimeUsersList)
                    {
                        users.Add(user.UserEMail.ToLower());
                    }

                    context.Session["ttUsers"] = users;

                    //check if TeamTimeOwner
                    context.Session["ttOwner"] = TeamTimeClass.isTeamTimeOwner;
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


                    SiteMaster.storepageinfo();
                    int CurrentStep = 0;
                    if (TeamTime.Pipe.Count < TeamTimeClass.TeamTimePipeStep) { TeamTimeClass.TeamTimePipeStep = 1; }
                    if (TeamTimeClass.TeamTimePipeStep >= 1) { CurrentStep = TeamTimeClass.TeamTimePipeStep; }
                    if (CurrentStep < 1) { CurrentStep = 0; }
                    if (Request.QueryString["pipe"] != null)
                    {
                        CurrentStep = Convert.ToInt32(Request.QueryString["pipe"]);
                        if (TeamTime.Pipe.Count < Convert.ToInt32(Request.QueryString["pipe"])) { CurrentStep = 1; } 
                        WorkSpace.set_ProjectStep(App.ActiveProject.isImpact, CurrentStep);
                        TeamTimeClass.TeamTimePipeStep = CurrentStep;
                    }
                    App.DBWorkspaceUpdate(ref WorkSpace, false, null);
                    TeamTime.SetCurrentStep(CurrentStep - 1, TeamTimeClass.TeamTimeUsersList);
                    TeamTimeClass.TeamTimeUpdateStepInfo(CurrentStep);
                    Session["curstep"] = CurrentStep;
                    Session["team"] = TeamTime;

                    var path = context.Server.MapPath("~/");
                    Consts._FILE_MHT_FILES = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, "DocMedia/MHTFiles/"));    // D3411
                    Consts._FILE_ROOT = context.Server.MapPath("~/");   // D3411

                    TeamTimeClass.GetPipeData();
                    TeamTimeClass.isTeamTime = true;
                }

            }
        }



        public static int PreviousStep
        {
            get
            {
                var app = (clsComparionCore)HttpContext.Current.Session["App"];
                
                if (HttpContext.Current.Session["PreviousStep"] != null)
                {
                    _previousStep = (int) HttpContext.Current.Session["PreviousStep"];
                }
                var workSpace = app.DBWorkspaceByUserIDProjectID(app.ActiveUser.UserID, app.ActiveProject.ID);
                if (_previousStep < 1)
                {
                    _previousStep = workSpace.get_ProjectStep(app.ActiveProject.isImpact);
                }
                if (_previousStep > TeamTimeClass.TeamTime.Pipe.Count)
                    _previousStep = 1;

                return _previousStep;
            }
            set { _previousStep = value; }
        }

        private static int _previousStep = -1;

        [WebMethod(EnableSession = true)]
        public static void MovePipeStep(int step)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var workSpace = app.DBWorkspaceByUserIDProjectID(app.ActiveUser.UserID, app.ActiveProject.ID);
            context.Session["PreviousStep"] = workSpace.get_ProjectStep(app.ActiveProject.isImpact);
            workSpace.set_ProjectStep(app.ActiveProject.isImpact, step); 
            app.DBWorkspaceUpdate(ref workSpace, false, null);
            TeamTimeClass.TeamTimePipeStep = step;
            context.Session["steps"] = null;
            context.Session[SessHierarchy] = null;
            TeamTimeClass.TeamTimeUpdateStepInfo(TeamTimeClass.TeamTimePipeStep);
            TeamTimeClass.GetPipeData();

        }

        [WebMethod(EnableSession = true)]
        public static bool is_tt_owner()
        {
            return TeamTimeClass.isTeamTimeOwner;

        }

        protected void prevBtn_Click(object sender, EventArgs e)
        {
            var team = (clsTeamTimePipe)Session["team"];
            var WorkSpace = App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, App.ActiveProject.ID);
            var h = WorkSpace.get_ProjectStep(App.ActiveProject.isImpact) - 1;
            if (h >= 1)
            {
                WorkSpace.set_ProjectStep(App.ActiveProject.isImpact, h); App.DBWorkspaceUpdate(ref WorkSpace, false, null);
                Response.Redirect("~/pages/teamtimetest/teamtime.aspx?pipe=" + h);
            }

        }

        protected void nextBtn_Click(object sender, EventArgs e)
        {
            var team = (clsTeamTimePipe)Session["team"];
            var WorkSpace = App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, App.ActiveProject.ID);
            var h = WorkSpace.get_ProjectStep(App.ActiveProject.isImpact) + 1;
            if (h <= team.Pipe.Count)
            {
                WorkSpace.set_ProjectStep(App.ActiveProject.isImpact, h); App.DBWorkspaceUpdate(ref WorkSpace, false, null);
                Response.Redirect("~/pages/teamtimetest/teamtime.aspx?pipe=" + h);
            }

        }

        protected void CheckProject()
        {
            if (App.ActiveUser == null)
            {
                Response.Redirect(Request.ApplicationPath);
            }
            if (App.ActiveProject == null)
            {
                Response.Redirect("~/pages/my-projects.aspx");
            }
            var sPasscode = App.ActiveProject.Passcode;
//            App.Logon(App.ActiveUser.UserEMail, App.ActiveUser.UserPassword, ref sPasscode, false);
        }


        [WebMethod(EnableSession = true)]
        public static Object refreshPage()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var sdata = "";



            Nullable<DateTime> DT = null;

            if(App.ActiveProject != null)
            {
                var isTeamTime = App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionPasscode, ref DT) != null;
                Object output = null;

                if (!isTeamTime || (!TeamTimeClass.isTeamTimeOwner && App.ActiveProject.MeetingOwnerID < 0))
                {
                    //App.LogonTeamTime(App.ActiveUser.UserEMail, App.ActiveUser.UserPassword);
                    var sPasscode = App.ActiveProject.Passcode;
                    App.Logon(App.ActiveUser.UserEMail, App.ActiveUser.UserPassword, ref sPasscode, false, true, false);
                    //context.Session["App"] = App;
                }

                if (isTeamTime || TeamTimeClass.isTeamTime)
                {
                    sdata = App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, ref DT);
                    var current_user = new string[] { App.ActiveUser.UserEMail.ToLower(), "" };
                    var MeetingOwner = App.ActiveProject.MeetingOwner.UserEMail.ToLower();
                    var EvaluationInfo = getEvaluationInfo(App.ActiveProject);
                    var goal = App.ActiveProject.HierarchyObjectives.GetLevelNodes(0)[0].NodeName;

                    var CurrentStep = TeamTimeClass.TeamTimePipeStep;
                    var nodes = (List<object>)TeamTimeClass.getnodes(CurrentStep);
                    var nodetype = App.ActiveProject.HierarchyObjectives.get_TerminalNodes();
                    var mtype = "";
                    object pipeData = null;
                    var showPriorityAndDirectValue = true;

                    switch (TeamTimeClass.Action(CurrentStep).ActionType)
                    {
                        case ActionType.atInformationPage:
                        {
                            clsInformationPageActionData Infodata = (clsInformationPageActionData)TeamTimeClass.Action(CurrentStep).ActionData;
                            var InformationMessage = "";
                            switch (Infodata.Description.ToLower())
                            {
                                case "welcome":
                                    InformationMessage = App.ActiveProject.PipeParameters.PipeMessages.GetWelcomeText(Canvas.PipeParameters.PipeMessageKind.pmkText, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy);         // D3411
                                    if (InformationMessage == "")
                                    {
                                        InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(false, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType == CanvasTypes.ProjectType.ptOpportunities)), App.ActiveUser, App.ActiveProject);
                                    }
                                    else
                                    {

                                        InformationMessage = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "welcome", InformationMessage, false, true, -1);     // D3411
                                    }
                                    InformationMessage = TeamTimeClass.ParseAllTemplates(InformationMessage, App.ActiveUser, App.ActiveProject);
                                    break;

                                case "thankyou":
                                    InformationMessage = App.ActiveProject.PipeParameters.PipeMessages.GetThankYouText(Canvas.PipeParameters.PipeMessageKind.pmkText, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy);
                                    if (InformationMessage == "")
                                    {
                                        InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(true, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType == CanvasTypes.ProjectType.ptOpportunities)), App.ActiveUser, App.ActiveProject);
                                    }
                                    else
                                    {

                                        InformationMessage = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "thankyou", InformationMessage, false, true, -1);    // D3411
                                    }
                                    InformationMessage = TeamTimeClass.ParseAllTemplates(InformationMessage, App.ActiveUser, App.ActiveProject);
                                    break;
                            }
                            pipeData = new
                            {
                                InformationMessage = InformationMessage
                            };
                        }

                            break;
                        case ActionType.atPairwise:
                        {
                            var ParentNodeID = ((clsNode)nodes[6]).NodeID;
                            var ParentNode = (clsNode)nodes[6];
                            var LeftNode = (clsNode)nodes[4];
                            var RightNode = (clsNode)nodes[5];
                            var question = "";
                            var wording = "";


                            int index = nodetype.FindIndex(item => item.NodeName == ParentNode.NodeName);

                            if (index >= 0)
                            {
                                question = "alternatives";
                                wording = App.ActiveProject.PipeParameters.JudgementAltsPromt;
                            }
                            mtype = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(ParentNode).ToString();

                            var ParentNodeInfo = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, true, true, -1);
                            var LeftNodeInfo = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, LeftNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, LeftNode.NodeID.ToString(), LeftNode.InfoDoc, true, true, -1);
                            var RightNodeInfo = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, RightNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, RightNode.NodeID.ToString(), RightNode.InfoDoc, true, true, -1);
                            var WrtLeftNodeInfo = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, LeftNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(LeftNode.NodeGuidID, ParentNode.NodeGuidID), true, true, ParentNode.NodeID);
                            var WrtRightNodeInfo = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, RightNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(RightNode.NodeGuidID, ParentNode.NodeGuidID), true, true, ParentNode.NodeID);

                            pipeData = new
                            {
                                ParentNodeInfo = ParentNodeInfo,
                                LeftNodeInfo = LeftNodeInfo,
                                RightNodeInfo = RightNodeInfo,
                                WrtLeftNodeInfo = WrtLeftNodeInfo,
                                WrtRightNodeInfo = WrtRightNodeInfo,
                                ParentNodeID = ParentNodeID
                            };
                        }
                            break;

                        case ActionType.atNonPWOneAtATime:
                        {
                            var ParentNodeID = ((clsNode)nodes[6]).NodeID;
                            var ParentNode = (clsNode)nodes[6];
                            var tAction = (clsAction)TeamTimeClass.Action(CurrentStep);
                            var data = (clsOneAtATimeEvaluationActionData)tAction.ActionData;
                            clsNode ChildNode = null;
                            var ObjHierarchy = (clsHierarchy)App.ActiveProject.ProjectManager.get_Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy);
                            var AltsHierarchy = (clsHierarchy)App.ActiveProject.ProjectManager.get_AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy);
                            var ParentNodeInfo = "";
                            var LeftNodeInfo = "";
                            var WrtLeftNodeInfo = "";
                            mtype = ((clsNonPairwiseEvaluationActionData)tAction.ActionData).MeasurementType.ToString();
                            var XMinValue = 0.00; var XMaxValue = 0.00;
                            var piecewise = false;

                            var Curvature = "";
                            var Decreasing = "";


                            if (data.Node != null && data.Judgment != null)
                            {
                                switch (((clsNonPairwiseEvaluationActionData)tAction.ActionData).MeasurementType)
                                {
                                    case ECMeasureType.mtRatings:
                                        clsNonPairwiseMeasureData ratings_data = (clsNonPairwiseMeasureData)data.Judgment;
                                        ParentNode = data.Node;
                                        
                                        clsMeasureScales MeasureScales = App.ActiveProject.ProjectManager.MeasureScales;
                                        clsRatingScale RS = MeasureScales.GetRatingScaleByID(ParentNode.get_RatingScaleID());
                                        if (RS != null)
                                        {
                                            showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.get_RatingsUseDirectValue(RS.GuidID);
                                        }

                                        if (ParentNode.IsAlternative)
                                        {
                                            ParentNode = ObjHierarchy.Nodes[0];
                                            ChildNode = data.Node;
                                        }
                                        else
                                        {
                                            ParentNode = data.Node;
                                            if (ParentNode != null && ParentNode.IsTerminalNode)
                                            {
                                                ChildNode = AltsHierarchy.GetNodeByID(ratings_data.NodeID);
                                            }
                                            else
                                            {
                                                ChildNode = ObjHierarchy.GetNodeByID(ratings_data.NodeID);
                                            }
                                        }

                                        break;

                                    case ECMeasureType.mtDirect:
                                        clsDirectMeasureData direct_data = (clsDirectMeasureData)data.Judgment;

                                        ParentNode = data.Node;
                                        var DirectData = (clsDirectMeasureData)data.Judgment;
                                        if (ParentNode.IsTerminalNode)
                                        {
                                            ChildNode = AltsHierarchy.GetNodeByID(DirectData.NodeID);
                                        }
                                        else
                                        {
                                            ChildNode = ObjHierarchy.GetNodeByID(DirectData.NodeID);
                                        }
                                        break;


                                    case ECMeasureType.mtStep:
                                        clsOneAtATimeEvaluationActionData StepActionData = (clsOneAtATimeEvaluationActionData)data;

                                        var SF = (clsStepFunction)StepActionData.MeasurementScale;
                                        float Low = ((clsStepInterval)SF.Intervals[0]).Low;
                                        float High = ((clsStepInterval)SF.Intervals[SF.Intervals.Count - 1]).Low;
                                        XMinValue = Low - (High - Low) / 10;
                                        XMaxValue = High + (High - Low) / 10;
                                        var StepData = (clsStepMeasureData)StepActionData.Judgment;
                                        piecewise = StepData.StepFunction.IsPiecewiseLinear;

                                        if (data.Node.IsTerminalNode)
                                        {
                                            ChildNode = AltsHierarchy.GetNodeByID(StepData.NodeID);
                                        }
                                        else
                                        {
                                            ChildNode = ObjHierarchy.GetNodeByID(StepData.NodeID);
                                        }
                                        ParentNode = data.Node;

                                        break;

                                    case ECMeasureType.mtRegularUtilityCurve:
                                    case ECMeasureType.mtAdvancedUtilityCurve:
                                        clsOneAtATimeEvaluationActionData tData = (clsOneAtATimeEvaluationActionData)data;
                                        //skip risk controls
                                        var tData_Judgment = (clsUtilityCurveMeasureData)tData.Judgment;

                                        //RUC Data
                                        var RUC = (clsRegularUtilityCurve)tData.MeasurementScale;
                                        XMinValue = RUC.Low;
                                        XMaxValue = RUC.High;
                                        Curvature = StringFuncs.JS_SafeString(RUC.Curvature);
                                        Decreasing = (!RUC.IsIncreasing).ToString().ToLower();

                                        ParentNode = tData.Node;

                                        if (ParentNode.IsTerminalNode)
                                        {
                                            ChildNode = AltsHierarchy.GetNodeByID(tData_Judgment.NodeID);
                                        }
                                        else
                                        {
                                            ChildNode = ObjHierarchy.GetNodeByID(tData_Judgment.NodeID);
                                        }

                                        break;
                                }
                            }

                            LeftNodeInfo = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ChildNode.NodeID.ToString(), ChildNode.InfoDoc, true, true, -1);
                            ParentNodeInfo = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, true, true, -1);
                            WrtLeftNodeInfo = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID), true, true, ParentNode.NodeID);

                            pipeData = new
                            {
                                ParentNodeInfo = ParentNodeInfo,
                                LeftNodeInfo = LeftNodeInfo,
                                WrtLeftNodeInfo = WrtLeftNodeInfo,
                                ParentNodeID = ParentNodeID,
                                XMinValue = XMinValue,
                                XMaxValue = XMaxValue,
                                piecewise = piecewise,
                                Decreasing = Decreasing,
                                Curvature = Curvature
                            };
                            break;
                        }

                        case ActionType.atShowLocalResults:
                        {
                            var defaultsort = 0;
                            var CurrentNode = new object[3];
                            var ActionData = (Canvas.clsShowLocalResultsActionData)TeamTimeClass.Action(CurrentStep).ActionData;
                            CurrentNode[0] = ActionData.ParentNode.NodeName;
                            if (ActionData.ParentNode.Children.Count > 0)
                            {
                                CurrentNode[1] = App.ActiveProject.PipeParameters.NameObjectives;
                                CurrentNode[2] = true;
                            }
                            else
                            {
                                CurrentNode[1] = App.ActiveProject.PipeParameters.NameAlternatives;
                                CurrentNode[2] = false;
                            }
                            defaultsort = (int)App.ActiveProject.PipeParameters.LocalResultsSortMode;
                            var canshowresults = new
                            {
                                individual = ActionData.CanShowIndividualResults,
                                combined = ActionData.get_CanShowGroupResults()
                            };
                            string messagenote = "";
                            if (!canshowresults.individual || !canshowresults.combined)
                            {
                                messagenote = TeamTimeClass.ResString("msgNoLocalResultsTT");
                            }
                            pipeData = new
                            {
                                defaultsort = defaultsort,
                                currentNode = CurrentNode,
                                ParentNodeID = ActionData.ParentNode.NodeID,
                                messagenote = messagenote
                            };
                        }
                            break;

                        case ActionType.atShowGlobalResults:
                        {
                            var ActionData = (Canvas.clsShowGlobalResultsActionData)TeamTimeClass.Action(CurrentStep).ActionData;
                            var Hierarchy = new object[App.ActiveProject.HierarchyObjectives.Nodes.Count][];
                            for (int i = 0; i < App.ActiveProject.HierarchyObjectives.Nodes.Count; i++)
                            {
                                Hierarchy[i] = new object[2];
                                Hierarchy[i][0] = App.ActiveProject.HierarchyObjectives.Nodes[i].NodeID;
                                Hierarchy[i][1] = App.ActiveProject.HierarchyObjectives.Nodes[i].NodeName;
                            }
                            var defaultsort = (int)App.ActiveProject.PipeParameters.GlobalResultsSortMode;
                            var wrtNode = App.ActiveProject.HierarchyObjectives.GetLevelNodes(0)[0];
                            if (TeamTimeClass.isTeamTimeOwner)
                                if (TeamTimeClass.TeamTime.WRTNodeID < 0)
                                {
                                    TeamTimeClass.TeamTime.WRTNodeID = wrtNode.NodeID;
                                }

                            pipeData = new
                            {
                                defaultsort = defaultsort,
                                hierarchy = Hierarchy,
                                ParentNodeID = App.ActiveProject.HierarchyObjectives.Nodes[0].NodeID,
                                objective = App.ActiveProject.PipeParameters.NameObjectives
                            };
                            //for Hierarchies

                        }
                            break;
                    }

                    //check for please wait when PM disconnects
                    bool fProjectAvailable = !(App.ActiveProject.LockInfo != null && App.ActiveProject.LockInfo.LockStatus == ECLockStatus.lsLockForModify);
                    if (fProjectAvailable)
                    {
                        if (!DT.HasValue || DT.Value.AddSeconds(TeamTimeClass.TeamTimeRefreshTimeout * 10) < DateTime.Now)
                            sdata = null;
                    }

                    if (TeamTimeClass.isTeamTimeOwner) { 
                        if (CurrentStep < 1) { CurrentStep = 1; }
                        //path of info docs
                        var path = context.Server.MapPath("~/");
                        Consts._FILE_MHT_FILES = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, "DocMedia/MHTFiles/"));    // D3411
                        Consts._FILE_ROOT = context.Server.MapPath("~/");   // D3411
                        var Hierarchies = new object();
                        

                        //infodoc sizes
                        var sizes = App.ActiveProject.ProjectManager.PipeParameters.InfoDocSize;
                        string[,] infodoc_sizes = new string[5, 2];
                        //what sizes to pass
                        if (TeamTimeClass.isTeamTimeOwner)
                        {
                            infodoc_sizes = InfodocsSizes;
                        }
                        else
                        {
                            if (PmOverwriteInfoDocs == 1)
                            {
                                infodoc_sizes = InfodocsSizes;
                            }
                            else
                            {
                                infodoc_sizes = (string[,])HttpContext.Current.Session["P_InfodocsSizes"];
                                if (infodoc_sizes == null)
                                {
                                    infodoc_sizes = new string[5, 2] { { "0", "0" }, { "0", "0" }, { "0", "0" }, { "0", "0" }, { "0", "0" } };
                                }
                            }
                        }
                        //what docs to show 
                        bool[] infodoc_dropdown = { false, false, false, false, false };
                        if (TeamTimeClass.isTeamTimeOwner)
                        {
                            infodoc_dropdown = InfodocDropdown;
                        }
                        else
                        {
                            if (PmHideAndShowInfoDocs == 1)
                            {
                                infodoc_dropdown = InfodocDropdown;
                            }
                            else
                            {
                                infodoc_dropdown = (bool[])HttpContext.Current.Session["P_InfodocsVisible"];
                                if (infodoc_dropdown == null)
                                {
                                    infodoc_dropdown = new bool[] { false, false, false, false, false };
                                }
                            }
                        }

                        if (context.Session[SessHierarchy] == null)
                        {
                            Hierarchies = GeckoClass.NodeList(App.ActiveProject.HierarchyObjectives.GetLevelNodes(0), TeamTimeClass.Action(CurrentStep));
                            context.Session[SessHierarchy] = Hierarchies;
                        }
                        Hierarchies = context.Session[SessHierarchy];

                        string steps = "";
                        if(PreviousStep != CurrentStep)
                            steps = GetStepInformation(App, PreviousStep);

                        


                        //overall
                        DateTime DTs = DateTime.Now;
                        int jMade = App.ActiveProject.ProjectManager.GetMadeJudgmentCount(App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.UserID, ref DTs);
                        

                        var Action = (clsAction)TeamTimeClass.GetAction(TeamTimeClass.TeamTimePipeStep);

                        output = new
                        {
                            showinfodocnode = App.ActiveProject.PipeParameters.ShowInfoDocs,
                            mtype = mtype,
                            sdata = sdata,
                            current_user = current_user,
                            current_step = CurrentStep,
                            previous_step = PreviousStep,
                            totalPipe = TeamTimeClass.TeamTime.Pipe.Count,
                            actions = Action.ActionType.ToString(),
                            pipeCount = TeamTimeClass.TeamTime.Pipe.Count,
                            jmade = jMade,
                            toggle = context.Session["showtoggle"],
                            grouptoggle = context.Session["grouptoggle"],
                            //infodoc_path = context.Session["infodoc_path"]    // -D3411
                            infodoc_dropdown = infodoc_dropdown,             //for accordion dropdowns
                            isPM = TeamTimeClass.isTeamTimeOwner,
                            pagination = (object)context.Session["UsersListPagination"],                //pagination value
                            TeamTimeOn = isTeamTime,
                            Hierarchies = Hierarchies,
                            infodoc_sizes = infodoc_sizes,
                            steps = steps,
                            sRes = App.ResString((string)context.Session["sRes"]),
                            cluster_phrase = (string)context.Session["ClusterPhrase"],
                            meetingOwner = MeetingOwner,
                            wrtNodeID = TeamTimeClass.WrtNodeId,
                            evaluation = EvaluationInfo,
                            pipeData = pipeData,
                            goal = goal,
                            nameAlternatives = App.ActiveProject.PipeParameters.NameAlternatives,
                            nameObjectives = App.ActiveProject.PipeParameters.NameObjectives,
                            showPriorityAndDirect = showPriorityAndDirectValue
                        };
                    }
                    else
                    {
                        //participant

                        output = new
                        {
                            showinfodocnode = App.ActiveProject.PipeParameters.ShowInfoDocs,
                            mtype = mtype,
                            current_user = current_user,
                            evaluation = EvaluationInfo,
                            goal = goal,
                            grouptoggle = context.Session["grouptoggle"],
                            isPM = TeamTimeClass.isTeamTimeOwner,
                            meetingOwner = MeetingOwner,
                            nameAlternatives = App.ActiveProject.PipeParameters.NameAlternatives,
                            nameObjectives = App.ActiveProject.PipeParameters.NameObjectives,
                            pagination = (object)context.Session["UsersListPagination"],
                            sdata = sdata,
                            toggle = context.Session["showtoggle"],
                            pipeData = pipeData,
                            TeamTimeOn = isTeamTime
                        };
                    }
                }
                else
                {
                    var val = 0;
                    List<string> TTRows = App.DBTeamTimeDataReadAll(App.ProjectID, ecExtraProperty.TeamTimeSessionData);
                    var TTtotal = TTRows.Count;
                    if (TTtotal > 0) { val = 1; }
                    output = new
                    {
                        showinfodocnode = true,
                        sdata = val,
                        current_step = 0,
                        actions = 0,
                        leftnodeinfo = 0,
                        rightnodeinfo = 0,
                        WrtLeftInfo = 0,
                        WrtRightInfo = 0,
                        ParentNodeInfo = 0,
                        isPM = TeamTimeClass.isTeamTimeOwner,
                        pipeCount = TeamTimeClass.TeamTime == null ? 0 : TeamTimeClass.TeamTime.Pipe.Count,
                        jmade = 0,
                        question = "none",
                        wording = "none",
                        TeamTimeOn = TeamTimeClass.isTeamTime

                        //infodoc_path = context.Session["infodoc_path"]    // -D3411
                    };
                }

                if (!string.IsNullOrEmpty(sdata))
                {
                    if (TeamTimeClass.isTeamTimeOwner)
                    {
                        TeamTimeClass.TeamTimeUpdateStepInfo(TeamTimeClass.TeamTimePipeStep);
                        TeamTimeClass.GetPipeData();
                    }
                }
                //this is the one who inserts the data in the DB
                context.Session["restart_TT_list"] = false;

                var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var valuetohash = oSerializer.Serialize(output);
                var hash = StringFuncs.GetMD5(valuetohash);
                var sessionhash = (string)context.Session["hashed_data"];
                if (sessionhash != hash)
                {
                    context.Session[Sess_TT_hash_count] = 0;
                    context.Session["hashed_data"] = hash;
                    context.Session["restart_TT_list"] = true;
                }
                var TTData = new
                {
                    output = output,
                    hash = hash
                };

                return oSerializer.Serialize(TTData);
            }
            return null;
            //this is the one who inserts the data in the DB


           
        }

        //private static bool IsPriorityAndDirectValueShowing(clsComparionCore App)
        //{
        //    bool showPriorityAndDirectValue = true;
        //    clsMeasureScales MeasureScales = App.ActiveProject.ProjectManager.MeasureScales;
        //    clsAllChildrenEvaluationActionData MultiNonPWData = (clsAllChildrenEvaluationActionData)AnytimeAction.ActionData;

        //    showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.get_RatingsUseDirectValue(RS.GuidID);

        //    if (MeasureScales != null)
        //    {
        //        if (MeasureScales.RatingsScales != null)
        //        {
        //            clsRatingScale RS = MeasureScales.GetRatingScaleByID(MultiNonPWData.ParentNode.get_RatingScaleID());
        //            if (RS != null)
        //            {
        //            }
        //        }
        //    }

        //    return showPriorityAndDirectValue;
        //}

        private static string GetStepInformation(clsComparionCore app, int previousStep = -1)
        {
            string steps = "";
            if (previousStep > -1)
            {
                steps = GetStepData(previousStep - 1, steps);
            }
            else
            {
                for (int i = 0; i < TeamTimeClass.TeamTime.Pipe.Count; i++)
                {
                    steps = GetStepData(i, steps);
                    if (i < TeamTimeClass.TeamTime.Pipe.Count - 1)
                        steps += ",";
                }
            }

            //steps+= "]";
            //0 - step content, 
            //1 - font color,
            //2 - background
            return steps;
        }

        private static string GetStepData(int step, string steps)
        {
            var action = TeamTimeClass.GetAction(step + 1);
            var content = GeckoClass.GetPipeStepHint(action, null);
            var color = "";
            var background = "";

            switch (TeamTimeClass.GetAction(step + 1).ActionType)
            {
                case ActionType.atShowLocalResults:
                case ActionType.atShowGlobalResults:
                case ActionType.atInformationPage:
                case ActionType.atSurvey:
                case ActionType.atSensitivityAnalysis:
                case ActionType.atSpyronSurvey:
                    color = "#0058a3";
                    break;
                default:
                    switch (TeamTimeClass.getStepStatus(TeamTimeClass.GetAction(step + 1)))
                    {
                        case TeamTimeClass.ecPipeStepStatus.psAllJudgments:
                            {
                                color = "#000";
                            }
                            break;
                        case TeamTimeClass.ecPipeStepStatus.psMissingJudgments:
                            {
                                color = "#f4ab5e";
                            }
                            break;
                        case TeamTimeClass.ecPipeStepStatus.psNoJudgments:
                            {
                                color = "#e74c3c";
                            }
                            break;
                        default:
                            color = "#000";
                            break;
                    }
                    break;
            }


            Regex rgx = new Regex("<.*?>");

            var stepinfo = String.Format(TeamTimeClass.ResString("btnEvaluationStepHint"), step + 1, rgx.Replace(content.Replace("'", "\""), " "));
            steps += $"['{stepinfo}','{color}','{background}']";
            return steps;
        }

        [WebMethod(EnableSession = true)]
        public static string setjson(string status, string svalue, string sname, string sguid, string scomment)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            ECTypes.clsUser _user = App.ActiveProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEMail);
            var comment = scomment;
            if (TeamTimeClass.isTeamTimeOwner & sname != "")
            {
                _user = App.ActiveProject.ProjectManager.GetUserByEMail(sname);
            }
            clsAction tAction = TeamTimeClass.GetAction(TeamTimeClass.TeamTimePipeStep);

            if (status == "anonymous")
            {
                var tval = svalue.Split('_');
                bool val = tval[0] == "true";
                TeamTimeClass.TeamTimePipeParams.TeamTimeStartInAnonymousMode = val;
                if (Convert.ToInt16(tval[1]) >= 0)
                {
                    TeamTimeClass.userdisplayname = Convert.ToInt16(tval[1]);
                }
                
            }

            if (status == "group")
            {
                int val = Convert.ToInt16(svalue);
                context.Session["grouptoggle"] = val;
            }

            if (status == "hide")
            {
                bool val = svalue == "true";
                TeamTimeClass.TeamTimePipeParams.SynchStartInPollingMode = val;
            }

            if (status == "individual")
            {
                int val = Convert.ToInt16(svalue);
                context.Session["showtoggle"] = val;
            }

            if (status == "node")
            {
                var Tval = svalue.Split('_');
                bool Bval = Tval[0] == "true";
                var Node = Convert.ToUInt16(Tval[1]);
                InfodocDropdown[Node] = Bval;
            }

            if (status == "infodoc-size")
            {
                var value = svalue.Split('_');
                var index = Convert.ToUInt16(value[0]);
                var width = value[1];
                var height = value[2];
                InfodocsSizes[index, 0] = width;
                InfodocsSizes[index, 1] = height;
            }

            if (status == "offline")
            {
                context.Session["offline"] =  svalue.ToLower() == "1";
            }

            if (status == "pagination")
            {
                var val = svalue.Split('_');
                int[] paginate = { Convert.ToInt32(val[0]), Convert.ToInt32(val[1]) };
                context.Session["UsersListPagination"] = paginate;
            }

            if (status == "pie-chart")
            {
                int val = Convert.ToInt16(svalue);
                TeamTimeClass.HidePieChart = val;
            }

            if (status == "save" || status == "comment")
            {
                if (status == "comment")
                {
                    var SplitValue = svalue.Split(new string[] { "___" }, StringSplitOptions.None);
                    svalue = SplitValue[0];
                    comment = SplitValue[1];
                    comment = comment + " @@" + DateTime.Now.Date.ToLongDateString() + " @@" + DateTime.Now.ToString("hh:mm:ss tt");
                    
                }

                string sStep = TeamTimeClass.TeamTimePipeStep.ToString();
                string sGUID = sguid;
                string sValue = svalue;
                string sUID = _user.UserID.ToString();
                string sComment = comment;
                Random rnd = new Random();
                string pendingID = rnd.Next(0, 5) + "-" + rnd.Next(10000, 99999);
                int UID = -1;
                int St = -1;
                if (int.TryParse(sStep, out St) && int.TryParse(sUID, out UID) && !string.IsNullOrEmpty(sGUID))
                {
                    ECTypes.clsUser tUser = App.ActiveProject.ProjectManager.GetUserByID(UID);
                    if (tUser != null)
                    {
                        if (TeamTimeClass.isTeamTimeOwner | tUser.UserEMail.ToLower() == App.ActiveUser.UserEMail.ToLower())
                        {
                            string sJudgment = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{6}{0}{5}", clsTeamTimePipe.Judgment_Delimeter, UID, St - 1, sGUID, sValue, sComment, pendingID);
                            App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveUser.UserID, ecExtraProperty.TeamTimeJudgment, sJudgment, false);
                            if (App.ActiveProject.LockInfo != null)
                            {
                                clsApplicationUser tOwner = App.ActiveProject.MeetingOwner;
                                if (tOwner == null)
                                    tOwner = clsApplicationUser.UserByUserID(App.ActiveProject.MeetingOwnerID, TeamTimeClass.UsersList);
                                if (tOwner != null)
                                {
                                    var linfo = App.ActiveProject.LockInfo;
                                    var mowner = App.ActiveProject.MeetingOwner;
                                    App.DBProjectLockInfoWrite(ECLockStatus.lsLockForTeamTime, ref linfo, ref mowner, DateTime.Now.AddSeconds(Consts._DEF_LOCK_TT_JUDGMENT_TIMEOUT));
                                }
                            }
                        }
                    }
                }
            }

            if (status == "single_line")
            {
                if (TeamTimeClass.isTeamTimeOwner)
                {
                    context.Session["single_line"] = Convert.ToBoolean(svalue);
                }
            }

            if (status == "wrtID")
            {
                if (TeamTimeClass.isTeamTimeOwner)
                {
                    TeamTimeClass.TeamTime.WRTNodeID = Convert.ToInt16(svalue);
                }
            }

            if (status != "node")
            {
                if (TeamTimeClass.isTeamTimeOwner)
                {
                    TeamTimeClass.TeamTimeUpdateStepInfo(TeamTimeClass.TeamTimePipeStep);
                    TeamTimeClass.GetPipeData();
                }
            }




            return "yes";
        }

        [WebMethod(EnableSession = true)]
        public static string loadTeamTime()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            return TeamTimeClass.getTeamTimeData();
        }


        [WebMethod(EnableSession = true)]
        public static void ApplyCustomWordingToNodes(List<int> node_ids, string custom_word, bool is_multi, bool is_evaluation)
        {
            var context = HttpContext.Current;
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var ObjHierarchy = (clsHierarchy)App.ActiveProject.HierarchyObjectives;
            foreach (int node_id in node_ids)
            {
                var node = (clsNode)ObjHierarchy.GetNodeByID(node_id);
                var tAdditionGUID = Guid.Empty;
                if (!is_evaluation)
                {
                    tAdditionGUID = node.NodeGuidID;
                }
                TeamTimeClass.SetClusterPhraseForNode(node.NodeGuidID, custom_word, is_multi, tAdditionGUID);
            }
        }

        [WebMethod(EnableSession = true)]
        public static object ResizeParticipantInfoDocs(int index, string width, string height)
        {
            var context = HttpContext.Current;
            string[,] session_sizes = (string[,])context.Session["P_InfodocsSizes"];
            string[,] p_sizes = new string[5, 2];
            if (session_sizes == null)
            {
                session_sizes = p_sizes;

            }
            else
            {
                session_sizes[index, 0] = width;
                session_sizes[index, 1] = height;

            }

            context.Session["P_InfodocsSizes"] = session_sizes;
            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return oSerializer.Serialize(context.Session["P_InfodocsSizes"]);
        }

        [WebMethod(EnableSession = true)]
        public static object HideAndShowInfoDocs(int index, string value)
        {
            var context = HttpContext.Current;
            bool[] session_infodocs_dropdown = (bool[])context.Session["P_InfodocsVisible"];
            bool[] p_infodocs_dropdown = new bool[] { false, false, false, false, false };
            if (session_infodocs_dropdown == null)
            {
                session_infodocs_dropdown = p_infodocs_dropdown;
            }
            else
            {
                bool Bval = value == "True";
                session_infodocs_dropdown[index] = Bval;
            }

            context.Session["P_InfodocsVisible"] = session_infodocs_dropdown;
            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return oSerializer.Serialize(context.Session["P_InfodocsVisible"]);
        }

        [WebMethod(EnableSession = true)]
        public static void SetResizeInfoDocsToAll(int value)
        {
            var context = HttpContext.Current;
            PmOverwriteInfoDocs = value;
        }

        [WebMethod(EnableSession = true)]
        public static void SetHideAndShowInfoDocsToAll(int value)
        {
            var context = HttpContext.Current;
            if (Convert.ToInt16(value) == 1)
            {
                context.Session["InfoDocsChecked"] = "checked";
            }
            else
            {
                context.Session["InfoDocsChecked"] = "";
            }

            PmHideAndShowInfoDocs = value;
        }

        [WebMethod(EnableSession = true)]
        public static int settoggle(int toggle)
        {
            Page objp = new Page();
            objp.Application["toggle"] = toggle;

            return toggle;
        }



        protected void Page_Init(object sender, System.EventArgs e)
        {
            App = (clsComparionCore)Session["App"];
            CheckProject();
            clsWorkspace tWs = App.ActiveWorkspace;
            if (tWs == null)
                tWs = App.AttachProject(App.ActiveUser, App.ActiveProject, false, App.ActiveWorkgroup.get_GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator), "Attach user to project", false);
            // D2644
            // D1945
            if (tWs != null && !tWs.get_isInTeamTime(App.ActiveProject.isImpact) && TeamTimeClass.isTeamTime)
            {
                tWs.set_TeamTimeStatus(App.ActiveProject.isImpact, ecWorkspaceStatus.wsSynhronousActive);
                // D1945
                tWs.set_Status(App.ActiveProject.isImpact, ecWorkspaceStatus.wsSynhronousActive);
                // D1945
                App.DBWorkspaceUpdate(ref tWs, false, "Enable TeamTime for user");
            }

            CheckTTJSON = "";
        }

        [WebMethod(EnableSession = true)]
        public static void TeamTimeExecuteEvent(string TeamTimeEvent)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            if (TeamTimeEvent == "pause")
            {
                if (TeamTimeClass.isTeamTimeOwner)
                {
                    App.DBTeamTimeDataDelete(App.ProjectID, null, ecExtraProperty.TeamTimeSessionData, -1);
                    App.DBTeamTimeDataDelete(App.ProjectID, null, ecExtraProperty.TeamTimeStepData);
                }
            }
            if (TeamTimeEvent == "resume")
            {
                if (TeamTimeClass.isTeamTimeOwner)
                {
                    var clsusers = (List<clsApplicationUser>)App.DBUsersByProjectID(6);
                    var clsworkspaces = (List<clsWorkspace>)App.DBWorkspacesByProjectID(6);
                    var Project = App.ActiveProject;
                    var ActHierarchy = (ECTypes.ECHierarchyID)Project.ProjectManager.ActiveHierarchy;
                    var own = App.ActiveUser;
                    var starts = App.TeamTimeResumeSession(own, Project, ActHierarchy, null, null, null, false, -1, PipeParameters.ecAppliationID.appGecko);
                    HttpContext.Current.Session["TTstart"] = true;
                    TeamTimeClass.TeamTimeUpdateStepInfo(TeamTimeClass.TeamTimePipeStep);
                    TeamTimeClass.GetPipeData();
                }
            }
            if (TeamTimeEvent == "stop")
            {
                var Project = App.ActiveProject;
                App.TeamTimeEndSession(ref Project, false);
                App.ActiveProject.isTeamTimeLikelihood = false;
                App.ActiveProject.isTeamTimeImpact = false;
                App.ActiveProject.isTeamTime = false;
                try
                {
                    foreach (clsApplicationUser user in App.DBUsersByProjectID(App.ActiveProject.ID))
                    {
                        if (App.ActiveProject.OwnerID != App.ActiveUser.UserID)
                        {
                            if (user.isOnline)
                            {

                                App.Database.ExecuteSQL("UPDATE users SET isOnline=0 WHERE ID='" + user.UserID + "'");

                            }
                        }
                    }

                }
                catch
                {
                }
                TeamTimeClass.TeamTime = null;
                HttpContext.Current.Session["team"] = null;
                HttpContext.Current.Session["TTstart"] = false;
            }
        }

        public static clsTeamTimePipe TeamTime_Pipe()
        {

            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var _TeamTime_Pipe = new clsTeamTimePipe(App.ActiveProject.ProjectManager, App.ActiveProject.ProjectManager.GetUserByID(App.ActiveProject.MeetingOwnerID));

            if (HttpContext.Current.Session[TeamTimeClass._SESS_TT_Pipe] == null)
            {
                _TeamTime_Pipe.Override_ResultsMode = true;
                _TeamTime_Pipe.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth;
                _TeamTime_Pipe.VerifyUsers(TeamTimeClass.TeamTimeUsersList);
                _TeamTime_Pipe.CreatePipe();
                HttpContext.Current.Session.Add(TeamTimeClass._SESS_TT_Pipe, _TeamTime_Pipe);
            }
            else
            {
                _TeamTime_Pipe = (clsTeamTimePipe) HttpContext.Current.Session[TeamTimeClass._SESS_TT_Pipe];
            }



            return _TeamTime_Pipe;
        }

        public static List<int> getEvaluationInfo(clsProject Project)
        {

            var EvaluationInfo = new List<int>();
            var TotalEvaluation = Project.ProjectManager.GetTotalJudgmentCount(Project.ProjectManager.ActiveHierarchy, Project.ProjectManager.UserID);
            EvaluationInfo.Add(TotalEvaluation);

            var LastJudgmentTime = DateTime.Now;

            var JudgmentCompleted = Project.ProjectManager.GetMadeJudgmentCount(Project.ProjectManager.ActiveHierarchy, Project.ProjectManager.UserID, ref LastJudgmentTime);
            EvaluationInfo.Add(JudgmentCompleted);

            var overall = (Convert.ToDouble(JudgmentCompleted) / Convert.ToDouble(TotalEvaluation == 0 ? 1 : TotalEvaluation)) * 100;
            EvaluationInfo.Add((int) overall);

            return EvaluationInfo;
        }

        [WebMethod(EnableSession = true)]
        public static string LoadStepList()
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var steps = GetStepInformation(app); //retrieve step information
            return steps;
        }

        [WebMethod(EnableSession = true)]
        public static string GetInformationMessage(string messageId)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["app"];
            var InformationMessage = "";
            switch (messageId)
            {
                case "welcome":
                    InformationMessage = app.ActiveProject.PipeParameters.PipeMessages.GetWelcomeText(Canvas.PipeParameters.PipeMessageKind.pmkText, app.ActiveProject.ProjectManager.ActiveHierarchy, app.ActiveProject.ProjectManager.ActiveAltsHierarchy);         // D3411
                    if (InformationMessage == "")
                    {
                        InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(false, app.ActiveProject.isImpact, app.ActiveProject.PipeParameters.ProjectType == CanvasTypes.ProjectType.ptOpportunities)), app.ActiveUser, app.ActiveProject);
                    }
                    else
                    {

                        InformationMessage = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "welcome", InformationMessage, false, true, -1);     // D3411
                    }
                    InformationMessage = TeamTimeClass.ParseAllTemplates(InformationMessage, app.ActiveUser, app.ActiveProject);
                    break;

                case "thankyou":
                    InformationMessage = app.ActiveProject.PipeParameters.PipeMessages.GetThankYouText(Canvas.PipeParameters.PipeMessageKind.pmkText, app.ActiveProject.ProjectManager.ActiveHierarchy, app.ActiveProject.ProjectManager.ActiveAltsHierarchy);
                    if (InformationMessage == "")
                    {
                        InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(true, app.ActiveProject.isImpact, app.ActiveProject.PipeParameters.ProjectType == CanvasTypes.ProjectType.ptOpportunities)), app.ActiveUser, app.ActiveProject);
                    }
                    else
                    {

                        InformationMessage = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "thankyou", InformationMessage, false, true, -1);    // D3411
                    }
                    InformationMessage = TeamTimeClass.ParseAllTemplates(InformationMessage, app.ActiveUser, app.ActiveProject);
                    break;
            }
            return InformationMessage;
        }

        
    }
}