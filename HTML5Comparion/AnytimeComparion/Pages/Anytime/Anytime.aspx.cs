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
using System.Data.Entity.Core.Mapping;
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
using AnytimeComparion.Pages.external_classes;
using ExpertChoice.Results;
using SpyronControls.Spyron.Core;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace AnytimeComparion.Pages.Anytime
{
    public partial class Anytime : System.Web.UI.Page
    {
        //private clsComparionCore App;
        //private bool _canEditAutoAdvance = false;
        //private const bool AlwaysShowAutoAdvance = true;
        //private clsWorkspace WorkSpace = null;
        private const int AutoAdvanceMaxJudgments = 5;

        private const string InconsistencySortingEnabled = "InconsistencySortingEnabled";
        private const string InconsistencySortingOrder = "InconsistencySortingOrder";
        private const string BestFit = "BestFit";
        //private const string rbtninconsistency = "rbtninconsistency";

        private const string Sess_WrtNode = "Sess_WrtNode";
        private const string Sess_Recreate = "Recreate_Pipe";
        private const string SessionIsFirstTime = "IsFirstTime";
        private const string SessionAutoAdvanceJudgmentsCount = "AutoAdvanceJudgmentsCount";

        private const string SessionAutoAdvance = "AutoAdvance";
        private const string SessionIncreaseJudgmentsCount = "IncreaseJudgmentsCount";
        private const string SessionIsJudgmentAlreadySaved = "IsJudgmentAlreadySaved";
        private const string SessionMultiCollapse = "SessionMultiCollapse";
        private const string SessionSinglePwCollapse = "SessionSinglePwCollapse";
        private const string SessionParentNodeGuid = "SessionParentNodeGuid";

        private ECTypes.clsUser _OriginalAHPUser = null;

        //private bool temp_survey_signal = true;
        //private Dictionary<int, List<int>> QH_List = new Dictionary<int, List<int>>();
        //private bool Show_QH = true;

        //private string UserIsReadOnly = "0";
        //private int ReadOnly_UserID = -1;

        public enum ShowInfoDocsMode
        {
            sidmFrame = 0,
            sidmPopup = 1
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            var App = (clsComparionCore)Session["App"];

            if (!IsPostBack && App.HasActiveProject())
            {
                var updatedProject = App.DBProjectByID(App.ProjectID);
                var projectIndex = App.ActiveProjectsList.IndexOf(App.ActiveProject);
                App.ActiveProjectsList[projectIndex] = updatedProject;
            }

            HttpContext.Current.Session["showMessage"] = false;
            if (App != null)
            {
                if (App.ActiveProject!=null && App.ActiveProject.isTeamTime)
                {
                    var ecAppId = App.DBTeamTimeSessionAppID(App.ActiveProject.ID);
                    if (ecAppId == PipeParameters.ecAppliationID.appComparion)
                    {
                        //Redirect to Comparion
                        Response.Redirect(redirectToComparionTeamtime());
                    }
                    else
                    {
                        //Response.Redirect("~/pages/TeamTimeTest/teamtime.aspx");

                        //Need to change this redirection
                        //Response.Redirect("~/pages/RestrictAnytime.aspx");
                    }
                }

                CheckForNextProjectAndRedirectIfRequired(App);

                InitializeSessions(App);

                try
                {
                    clsApplicationUser CurrentUser = new clsApplicationUser();
                    var isDataInstance = false;
                    var recreatePipe = false;
                    NotRecreatePipe = false;
                    if (AnytimeClass.UserIsReadOnly())
                    {
                        //set ReadOnlyUser here
                        CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID());
                    }
                    else
                    {
                        CurrentUser = App.ActiveUser;
                        isDataInstance = true;
                        recreatePipe = true;
                        if (NotRecreatePipe)
                        {
                            recreatePipe = false;
                        }

                        NotRecreatePipe = true;
                    }


                    var AnytimeUser = App.ActiveProject.ProjectManager.GetUserByEMail(CurrentUser.UserEMail);

                    if (App.ActiveProject.IsRisk)
                    {
                        var ProjectManager = App.ActiveProject.ProjectManager;
                        ProjectManager.ActiveHierarchy = (int)(App.ActiveProject.isImpact ? ECTypes.ECHierarchyID.hidImpact : ECTypes.ECHierarchyID.hidLikelihood);
                        if (ProjectManager.ActiveHierarchy == (int)ECTypes.ECHierarchyID.hidImpact && !object.ReferenceEquals(ProjectManager.PipeParameters.CurrentParameterSet, ProjectManager.PipeParameters.ImpactParameterSet))
                            ProjectManager.PipeParameters.CurrentParameterSet = ProjectManager.PipeParameters.ImpactParameterSet;
                        if (ProjectManager.ActiveHierarchy == (int)ECTypes.ECHierarchyID.hidLikelihood && !object.ReferenceEquals(ProjectManager.PipeParameters.CurrentParameterSet, ProjectManager.PipeParameters.DefaultParameterSet))
                            ProjectManager.PipeParameters.CurrentParameterSet = ProjectManager.PipeParameters.DefaultParameterSet;
                    }

                    _OriginalAHPUser = AnytimeUser;

                    AnytimeClass.SetUser(AnytimeUser, true, !IsPostBack & !IsCallback);

                    if (AnytimeUser == null)
                    {
                        AnytimeUser = App.ActiveProject.ProjectManager.AddUser(App.ActiveUser.UserEMail, true,
                            App.ActiveUser.UserName);
                        App.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure();
                    }
                    var Project = App.ActiveProject;

                    bool fHasEvals = !App.Options.isSingleModeEvaluation;
                    bool isPM = App.CanUserModifyProject(CurrentUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);
                    if (isPM)
                    {
                        Session["AT_isOwner"] = 1;
                    }
                    else
                    {
                        Session["AT_isOwner"] = 0;
                    }
                    if (fHasEvals)
                    {

                    }



                    //GeckoClass.timeOutMessage = TeamTimeClass.ResString("msgTimeoutGecko");
                    Session["Project"] = Project;
                        

                    //FOR RISKION

                    //Project.ProjectManager.PipeBuilder.CreatePipe();



                    if (!Project.isOnline && !isPM)
                    {
                        Response.Redirect("~/?project=" + Project.ProjectName + "&is_offline=true");
                    }

                    var WorkSpace = (clsWorkspace)App.ActiveWorkspace;
                    var CurrentStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
                    Session["AT_CurrentStep"] = CurrentStep;

                    var paramStep = GetParameterStepFromSession(App);

                    if (paramStep > 0)
                    {
                        CurrentStep = paramStep;
                        CurrentStep = CurrentStep < 1 ? 1 : CurrentStep;

                        if (Project.Pipe.Count < paramStep)
                        {
                            CurrentStep = 1;
                        }

                        WorkSpace.set_ProjectStep(App.ActiveProject.isImpact, CurrentStep);
                        Session["AT_CurrentStep"] = CurrentStep;
                        Session[SessionIsFirstTime + App.ProjectID] = false;    //forcing not to show which step to choose modal
                    }

                    //var mDefaultParameterSet = new PipeParameters.ParameterSet(PipeParameters.PARAMETER_SET_DEFAULT, "DefaultParameterSet");
                    //App.ActiveProject.PipeParameters.ForceDefaultParameters = true;
                    //App.ActiveProject.PipeParameters.CurrentParameterSet = mDefaultParameterSet;

                    App.DBWorkspaceUpdate(ref WorkSpace, false, null);
                }
                catch
                {
                    // Response.Redirect("~/collect-input");
                    Session["AT_CurrentStep"] = 1;
                }

                if (Request.Cookies["loadedScreens"] == null)
                {
                    HttpCookie cookie = new HttpCookie("loadedScreens");
                    
                    cookie.Values.Add("pairwise", "0");
                    cookie.Values.Add("multiPairwise", "0");
                    cookie.Values.Add("direct", "0");
                    cookie.Values.Add("multiDirect", "0");
                    cookie.Values.Add("ratings", "0");
                    cookie.Values.Add("multiRatings", "0");
                    cookie.Values.Add("stepFunction", "0");
                    cookie.Values.Add("utility", "0");
                    cookie.Values.Add("localResults", "0");
                    cookie.Values.Add("globalResults", "0");
                    cookie.Values.Add("survey", "0");
                    cookie.Values.Add("sensitivity", "0");
                    cookie.Expires = DateTime.Now.AddDays(10);
                    Response.Cookies.Add(cookie);
                }

                //Response.Write("Total Steps:" + App.ActiveProject.Pipe.Count + "\n");
                if (App.ActiveUser == null)
                {
                    Response.Redirect(ResolveUrl("~/"));
                }

                var passcode = Session["passcode"] != null ? Session["passcode"].ToString() : "0";
                if (passcode != "0")
                {
                    if (AnytimeClass.RedirectAnonAndSignupLinks(App, passcode))
                    {
                        string redirectUrl = AnytimeClass.GetComparionHashLink();
                        _OriginalAHPUser = null;
                        App.Logout();
                        HttpContext.Current.Session.Clear();
                        HttpContext.Current.Session.Abandon();
                        HttpContext.Current.Response.Cookies["rmberme"].Expires = DateTime.Now.AddDays(-1);
                        HttpContext.Current.Response.Cookies["fullname"].Expires = DateTime.Now.AddDays(-1);

                        Response.Redirect(redirectUrl);
                        //Response.Redirect("~/pages/RedirectToComparion.aspx");
                    }
                }

            }
            else
            {
                Response.Redirect(ResolveUrl("~/"));
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if(_OriginalAHPUser != null)
            {
                AnytimeClass.SetUser(_OriginalAHPUser, false, false);
            }
        }

        private int GetParameterStepFromSession(clsComparionCore app)
        {
            var step = Session[Constants.SessionParamStep] == null ? 0 : (int)Session[Constants.SessionParamStep]; ;

            var mode = Session[Constants.SessionNonRMode] == null ? "" : (string) Session[Constants.SessionNonRMode];
            var nodeGuid = Session[Constants.SessionNonRNode] == null ? "" : (string)Session[Constants.SessionNonRNode];
            var mtType = Session[Constants.SessionNonRMtType] == null ? -1 : (int)Session[Constants.SessionNonRMtType];

            if (mode == "searchresults" && nodeGuid != "")
            {
                var index = 0;
                var hasStep = false;

                foreach (var action in app.ActiveProject.Pipe)
                {
                    index++;

                    if (action.ActionType == ActionType.atShowLocalResults)
                    {
                        var actionData = (clsShowLocalResultsActionData)action.ActionData;
                        if (actionData.ParentNode.NodeGuidID.ToString() == nodeGuid)
                        {
                            step = index;
                            hasStep = true;
                            break;
                        }
                    }

                    if (action.ActionType == ActionType.atShowGlobalResults)
                    {
                        if (!hasStep)
                        {
                            step = index;
                        }

                        var actionData = (clsShowGlobalResultsActionData)action.ActionData;
                        if (actionData.WRTNode.NodeGuidID.ToString() == nodeGuid)
                        {
                            hasStep = true;
                            break;
                        }
                    }
                }

                if (!hasStep)
                {
                    step = 0;
                    Session[Constants.SessionIsInterResultStepFound] = false;
                }
            }
            else if (mode == "getstep" && nodeGuid != "" && (mtType == 0 || mtType == 1))
            {
                var nGuid = Guid.Empty;
                if (Guid.TryParse(nodeGuid, out nGuid))
                {
                    var node = app.ActiveProject.HierarchyObjectives.GetNodeByID(nGuid);
                    node = node == null ? app.ActiveProject.HierarchyAlternatives.GetNodeByID(nGuid) : node;

                    step = app.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(node, -1);
                    step = node == null ? 0 : step + 1;
                }
            }

            return step;
        }

        private static bool NotRecreatePipe
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var sessVal = TeamTimeClass.get_SessVar(Sess_Recreate);
                return sessVal.Equals("1");
            }
            set
            {
                HttpContext context = HttpContext.Current;
                TeamTimeClass.set_SessVar(Sess_Recreate, value ? "1" : "0");
            }
        }
        public static string[] ExpectedValueString
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var expectedValue = (string[])context.Session[Constants.Sess_ExpectedValue];
                return expectedValue;
            }
            set
            {
                HttpContext context = HttpContext.Current;
                context.Session[Constants.Sess_ExpectedValue] = value;
            }
        }

        private static string PipeWarning
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context.Session[Constants.Sess_PipeWarning] == null)
                    context.Session[Constants.Sess_PipeWarning] = "";
                var pipeWarning = (string)context.Session[Constants.Sess_PipeWarning];
                return pipeWarning;
            }
            set
            {
                HttpContext context = HttpContext.Current;
                context.Session[Constants.Sess_PipeWarning] = value;
            }
        }
        public static bool CheckProject()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            if(App.ActiveProject != null)
            {
                var project = App.DBProjectByID(App.ActiveProject.ID);
                if (project.isTeamTime)
                {
                    App.ActiveProjectsList = null;
                }
                App.ActiveProject = project;
                return project.isTeamTime || project.isTeamTimeLikelihood;
            }
            return false;

        }

        [WebMethod(EnableSession = true)]
        public static Object GetDataOfPipeStep(int step)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];

            //if App has no data
            if(App.ActiveProject == null)
            {
                var returnValue = new
                {
                    message = GeckoClass.timeOutMessage,
                    status = "timeout"
                };
                var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return oSerializer.Serialize(returnValue);
            }
            if (CheckProject())
            {
                return false;
            }

            var CurrentStep = (int)context.Session["AT_CurrentStep"];

            if (CurrentStep != step)
            {
                context.Session[SessionIsJudgmentAlreadySaved] = false;
            }

            if (CurrentStep > App.ActiveProject.Pipe.Count)
            {
                CurrentStep = 1;
                step = CurrentStep;
            }
            string PairwiseType, first_node_info, second_node_info, parent_node_info, wrt_first_node_info, wrt_second_node_info, question, wording, StepTask, InformationMessage;
            PairwiseType = first_node_info = second_node_info = parent_node_info = wrt_first_node_info = wrt_second_node_info = question = wording = StepTask = InformationMessage = "";
            string FirstNodeName, SecondNodeName, ParentNodeName, ChildNodeName;
            ChildNodeName = FirstNodeName = SecondNodeName = ParentNodeName = "";
            double PWValue = -1, PWAdvantage = -1;
            double CurrentValue = -1;
            bool is_auto_advance = false;
            var IsUndefined = false;
            var PipeParameters = new object();

            //results pages
            var ParentNodeID = -1;

            //data related to multi pairwise
            var MultiPW_Data = new List<clsPairwiseLine>();
            var MultiNonPW_Data = new List<clsRatingLine>();

            var step_intervals = new List<string[]>();
            //ratings data
            var NonPWType = "";
            int precision = 0;
            var intensities = new List<string[]>();
            var multi_intensities = new List<string[]>();
            var NonPWValue = "";
            var is_direct = false;

            bool showPriorityAndDirectValue = true;
            //

            var judgment_made = 0;
            double overall = 0.00;
            var total_evaluation = 0;

            //step function
            var piecewise = false;

            //one at a time pairwise and non pw
            var comment = "";

            bool show_comments = false;

            //guids for info doc nodes
            Guid ParentNodeGUID = new Guid();
            Guid LeftNodeGUID = new Guid();
            Guid RightNodeGUID = new Guid();
            Guid LeftNodeWrtGUID = new Guid();
            Guid RightNodeWrtGUID = new Guid();

            string[] infodoc_params = new string[5];

            string done_redirect_url = "";
            var logout_at_end = false;

            bool is_infodoc_tooltip = false;

            var multi_GUIDs = new List<string[]>();
            List<string[]> multi_infodoc_params = new List<string[]>();
            //null for other screens at the moment
           
            var qh_info = "";
            var qh_yt_info = "";
            var qh_help_id = new PipeParameters.ecEvaluationStepType();
            var qh_tnode_id = 0;
            var saType = string.Empty;
            clsNode StepNode = null;
            bool show_qh_automatically = false;

            bool isPM = false;

            List<object> scaleDescriptions = new List<object>();

            var NodesData = new List<string[]>();

            var isFirstTime = (bool)context.Session[SessionIsFirstTime + App.ProjectID];
            context.Session[SessionIsFirstTime + App.ProjectID] = false;
            var firstUnassessedStep = GetFirstUnassessed(App);

            string pipeHelpUrl = "";
            var is_qh_shown = false;

            var multiCollapseStatus = HttpContext.Current.Session[SessionMultiCollapse] == null ? new Dictionary<string, List<bool>>() : (Dictionary<string, List<bool>>)HttpContext.Current.Session[SessionMultiCollapse];
            var multi_collapse_default = new List<bool>();        

            var dont_show_qh = getQHSettingCookies() == "False" ? false : true;
            var show_qh_setting = !dont_show_qh;
            Object output = null;
            Object UCData = null;
            //try
            //{
            if (App != null)
            {
                var LastJudgmentTime = DateTime.Now;
                judgment_made = App.ActiveProject.ProjectManager.GetMadeJudgmentCount(App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.UserID, ref LastJudgmentTime);

                //When there is no judgment made then it should go to fist step
                if (judgment_made == 0 && isFirstTime)
                {
                    step = 1;
                }

                //multi cookies
                bool exists = multiCollapseStatus.ContainsKey(App.ProjectID + "_" + step);
                if (!exists)
                {
                    multi_collapse_default.Add(false);
                    multi_collapse_default.Add(false);
                    multi_collapse_default.Add(false);
                    multi_collapse_default.Add(false);
                    multi_collapse_default.Add(false);
                }
                else
                {
                    multi_collapse_default = multiCollapseStatus[App.ProjectID + "_" + step];
                    var test = multi_collapse_default;
                }

                var user_id = App.ActiveUser.UserID;
                if (AnytimeClass.UserIsReadOnly())
                {
                    //set ReadOnlyUser here
                    user_id = AnytimeClass.GetReadOnlyUserID();
                }

                isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws,
              App.ActiveWorkgroup);
                var nodes = App.ActiveProject.HierarchyObjectives.Nodes;
                foreach (clsNode Node in nodes)
                {
                    var ActionType = AnytimeClass.Action(step).ActionType;
                    var node_type = Node.get_MeasureType(false).ToString().Replace("mt", "");
                    var action_type = ActionType.ToString();
                    if (action_type.Contains(node_type))
                    {
                        string[] temp_node = { "", "" };
                        temp_node[0] = Node.NodeID.ToString();
                        temp_node[1] = Node.NodeName;
                        NodesData.Add(temp_node);
                    }
                }

                var doneOptions = new Dictionary<string, object>()
                {
                    { "logout", App.ActiveProject.PipeParameters.LogOffAtTheEnd },
                    { "redirect",  App.ActiveProject.PipeParameters.RedirectAtTheEnd },
                    { "url",  App.ActiveProject.PipeParameters.TerminalRedirectURL },
                    { "closeTab", App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish },
                    { "openProject", App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish },
                    { "stayAtEval", !App.ActiveProject.PipeParameters.RedirectAtTheEnd && !App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish && !App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish }
                };

                //for Pipe Options
                var pipeOptions = new
                {
                    hideNavigation = !App.ActiveProject.PipeParameters.ShowProgressIndicator,
                    disableNavigation = !App.ActiveProject.PipeParameters.AllowNavigation,
                    showUnassessed = App.ActiveProject.PipeParameters.ShowNextUnassessed,
                    dontAllowMissingJudgment = !App.ActiveProject.PipeParameters.AllowMissingJudgments
                };

                if (HttpContext.Current.Session[SessionAutoAdvance + App.ProjectID] == null)
                {
                    is_auto_advance = App.ActiveProject.PipeParameters.AllowAutoadvance;
                    HttpContext.Current.Session[SessionAutoAdvance + App.ProjectID] = is_auto_advance;
                }
                else
                {
                    is_auto_advance = (bool)HttpContext.Current.Session[SessionAutoAdvance + App.ProjectID];
                }

                if (HttpContext.Current.Session["InfodocMode"] == null)
                {
                    if (App.ActiveProject.PipeParameters.ShowInfoDocsMode == CanvasTypes.ShowInfoDocsMode.sidmPopup)
                    {
                        is_infodoc_tooltip = true;
                        HttpContext.Current.Session["InfodocMode"] = "1";
                    }
                }
                else
                {
                    if (HttpContext.Current.Session["InfodocMode"].ToString() == "1")
                    {
                        is_infodoc_tooltip = true;
                    }
                }

                show_comments = App.ActiveProject.PipeParameters.ShowComments;
                var Project = App.ActiveProject;
                var user_email = App.ActiveUser.UserEMail;
                var isDataInstance = false;
                var recreatePipe = false;

                if (AnytimeClass.UserIsReadOnly())
                {
                    //set ReadOnlyUser here
                    var CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID());
                    user_email = CurrentUser.UserEMail;
                }
                else
                {
                    isDataInstance = true;
                    recreatePipe = true;

                    if (NotRecreatePipe)
                    {
                        isDataInstance = false;
                        recreatePipe = false;
                    }

                    NotRecreatePipe = true;
                }


                var AnytimeUser = (ECTypes.clsUser)App.ActiveProject.ProjectManager.GetUserByEMail(user_email);

                //judgment made and progress
                DateTime DTs = DateTime.Now;


                var totalObjective = App.ActiveProject.HierarchyObjectives.Nodes.Count;
                var totalAlternative = App.ActiveProject.HierarchyAlternatives.Nodes.Count;

                total_evaluation = App.ActiveProject.ProjectManager.GetTotalJudgmentCount(Project.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.UserID);
                
                var overallcalc = (double)((double)judgment_made / (double)total_evaluation) * 100;
                overall = double.IsNaN(overallcalc) ? 0 : overallcalc;
                AnytimeClass.SetUser(AnytimeUser, isDataInstance, recreatePipe);
                var PreviousStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
                App.ActiveWorkspace.set_ProjectStep(App.ActiveProject.isImpact, step);
                var workspace = App.ActiveWorkspace;
                App.DBWorkspaceUpdate(ref workspace, false, null);
                CurrentStep = step;
                context.Session["AT_CurrentStep"] = CurrentStep;

                if (context.Session["AT_PreviousStep"] == null || CurrentStep != PreviousStep)
                {
                    context.Session["AT_PreviousStep"] = PreviousStep;
                }
                else
                {
                    PreviousStep = (int)context.Session["AT_PreviousStep"];
                }

                if (step < 1)
                {
                    step = 0;
                }

                if (step > App.ActiveProject.Pipe.Count)
                {
                    step = App.ActiveProject.Pipe.Count;
                }

                //check if pm

                var AnytimeAction = AnytimeClass.GetAction(step);

                //assuming that the project uses pairwise verbal
                var path = context.Server.MapPath("~/");
                Consts._FILE_MHT_FILES = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, "DocMedia/MHTFiles/"));
                Consts._FILE_ROOT = context.Server.MapPath("~/");

                switch (AnytimeAction.ActionType)
                {
                    //Welcome and Thank You        
                    case ActionType.atInformationPage:
                        StepNode = null;
                        clsInformationPageActionData data = (clsInformationPageActionData)AnytimeAction.ActionData;
                        var isReward = false;
                        if (data.Description.ToLower() == "welcome")
                        {
                            pipeHelpUrl = TeamTimeClass.ResString("help_pipe_welcome");
                            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.Welcome;
                            InformationMessage = App.ActiveProject.PipeParameters.PipeMessages.GetWelcomeText(Canvas.PipeParameters.PipeMessageKind.pmkText, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy);

                            if (InformationMessage == "")
                            {
                                InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(false, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType == CanvasTypes.ProjectType.ptOpportunities)), App.ActiveUser, App.ActiveProject);
                            }
                            else
                            {
                                InformationMessage = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "welcome", InformationMessage, false, true, -1);
                            }
                            //context.Session["InformationMessage"] = InformationMessage;
                        }
                        else
                        {
                            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.ThankYou;
                            //case 11467 - reward page
                            if (App.ActiveProject.ProjectManager.Parameters.EvalShowRewardThankYou && GetNextUnassessed(CurrentStep) == null)
                                isReward = true;
                            InformationMessage = App.ActiveProject.PipeParameters.PipeMessages.GetThankYouText(isReward ? Canvas.PipeParameters.PipeMessageKind.pmkReward : Canvas.PipeParameters.PipeMessageKind.pmkText, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy);
                            if (InformationMessage != "")
                            {
                                InformationMessage = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "thankyou" + (isReward ? "_reward" : ""), InformationMessage, false, true, -1);
                            }
                            else
                            {
                                if (isReward)
                                {
                                    InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetIncFile(Consts._FILE_TEMPL_THANKYOU_REWARD)), App.ActiveUser, App.ActiveProject);
                                }
                                else
                                {
                                    InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(true, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType == CanvasTypes.ProjectType.ptOpportunities)), App.ActiveUser, App.ActiveProject);
                                }
                            }
                            //context.Session["InformationMessage"] = InformationMessage;
                        }
                        InformationMessage = TeamTimeClass.ParseAllTemplates(InformationMessage, App.ActiveUser, App.ActiveProject);
                        break;
                    //End of Welcome and Thank You       

                    //Pairwise        
                    case ActionType.atPairwise:
                        //this will force an error for zyza to test case 11265
                        var forceError = (bool)context.Session[Constants.Sess_ForceError];


                        var PWData = (clsPairwiseMeasureData)AnytimeAction.ActionData;
                        var ParentNode = (clsNode)App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.ParentNodeID);
                        var FirstNode = (clsNode)null;
                        var SecondNode = (clsNode)null;
                        StepNode = ParentNode;

                        comment = PWData.Comment;

                        PairwiseType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(ParentNode).ToString();

                        if (PairwiseType == "ptVerbal")
                        {
                            pipeHelpUrl = TeamTimeClass.ResString("help_pipe_singlePairwiseVerbal");
                            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.VerbalPW;
                            PipeWarning = context.Request.Cookies[Constants.Cook_Extreme] == null ? TeamTimeClass.ResString("msgPWExtreme") : "";
                        }
                        else
                        {
                            pipeHelpUrl = TeamTimeClass.ResString("help_pipe_singlePairwiseGraphical");
                            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.GraphicalPW;
                        }

                        //copied from teamtime need to refactor
                        question = "";
                        wording = "";
                        var NodeType = App.ActiveProject.HierarchyObjectives.get_TerminalNodes();
                        int index = NodeType.FindIndex(item => item.NodeName == ParentNode.NodeName);

                        if (index >= 0)
                        {
                            question = "alternatives";
                            wording = App.ActiveProject.PipeParameters.JudgementAltsPromt;
                        }

                        if (ParentNode.IsTerminalNode)
                        {
                            question = App.ActiveProject.PipeParameters.NameAlternatives;
                            wording = App.ActiveProject.PipeParameters.JudgementAltsPromt;

                            FirstNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(PWData.FirstNodeID);
                            SecondNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(PWData.SecondNodeID);
                        }
                        else
                        {
                            question = App.ActiveProject.PipeParameters.NameObjectives;
                            wording = App.ActiveProject.PipeParameters.JudgementPromt;

                            FirstNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.FirstNodeID);
                            SecondNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.SecondNodeID);
                        }

                        StepTask = "";
                        try
                        {
                            StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !FirstNode.IsTerminalNode && !SecondNode.IsTerminalNode);
                        }
                        catch
                        {
                            StepTask = "";
                        }

                        parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, true, true, -1);
                        first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, FirstNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, FirstNode.NodeID.ToString(), FirstNode.InfoDoc, true, true, -1);
                        second_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, SecondNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, SecondNode.NodeID.ToString(), SecondNode.InfoDoc, true, true, -1);
                        wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, FirstNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(FirstNode.NodeGuidID, ParentNode.NodeGuidID), true, true, ParentNode.NodeID);
                        wrt_second_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, SecondNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(SecondNode.NodeGuidID, ParentNode.NodeGuidID), true, true, ParentNode.NodeID);

                        FirstNodeName = FirstNode.NodeName;
                        SecondNodeName = SecondNode.NodeName;
                        ParentNodeName = ParentNode.NodeName;
                        PWValue = PWData.Value;
                        PWAdvantage = PWData.Advantage;
                        IsUndefined = PWData.IsUndefined;
                        ParentNodeGUID = ParentNode.NodeGuidID;
                        LeftNodeGUID = FirstNode.NodeGuidID;
                        RightNodeGUID = SecondNode.NodeGuidID;

                        infodoc_params[0] = GeckoClass.GetInfodocParams(ParentNode.NodeGuidID, Guid.Empty);
                        infodoc_params[1] = GeckoClass.GetInfodocParams(FirstNode.NodeGuidID, SecondNode.NodeGuidID);
                        infodoc_params[2] = GeckoClass.GetInfodocParams(SecondNode.NodeGuidID, FirstNode.NodeGuidID);
                        infodoc_params[3] = GeckoClass.GetInfodocParams(FirstNode.NodeGuidID, ParentNode.NodeGuidID);
                        infodoc_params[4] = GeckoClass.GetInfodocParams(SecondNode.NodeGuidID, ParentNode.NodeGuidID);

                        string default_params = "c=-1&w=200&h=200";

                        if (infodoc_params[0] == "")
                        {
                            infodoc_params[0] = default_params;
                        }

                        if (infodoc_params[1] == "")
                        {
                            infodoc_params[1] = GeckoClass.getCommonParams(infodoc_params[2]);
                        }

                        if (infodoc_params[2] == "")
                        {
                            infodoc_params[2] = GeckoClass.getCommonParams(infodoc_params[1]);
                        }

                        if (infodoc_params[2] == "" || infodoc_params[1] == "")
                        {
                            GeckoClass.SetInfodocParams(FirstNode.NodeGuidID, SecondNode.NodeGuidID, default_params);
                            GeckoClass.SetInfodocParams(SecondNode.NodeGuidID, FirstNode.NodeGuidID, default_params);
                            infodoc_params[1] = default_params;
                            infodoc_params[2] = default_params;
                        }

                        if (infodoc_params[3] == "")
                        {
                            infodoc_params[3] = GeckoClass.getCommonParams(infodoc_params[4]);
                        }

                        if (infodoc_params[4] == "")
                        {
                            infodoc_params[4] = GeckoClass.getCommonParams(infodoc_params[3]);
                        }

                        if (infodoc_params[3] == "" || infodoc_params[4] == "")
                        {
                            GeckoClass.SetInfodocParams(FirstNode.NodeGuidID, ParentNode.NodeGuidID, default_params);
                            GeckoClass.SetInfodocParams(SecondNode.NodeGuidID, ParentNode.NodeGuidID, default_params);
                            infodoc_params[3] = default_params;
                            infodoc_params[4] = default_params;
                        }

                        var debug_test = infodoc_params;

                        ParentNodeID = PWData.ParentNodeID;
                        break;
                    //End of Pairwise        

                    //Multi Pairwise        
                    case ActionType.atAllPairwise:
                    case ActionType.atAllPairwiseOutcomes:
                        bool fIsPWOutcomes = AnytimeAction.ActionType == ActionType.atAllPairwiseOutcomes;
                        int UndefIDx = -1;
                        //just checking if action is of type of "clsAllPairwiseEvaluationActionData"
                        if (AnytimeAction.ActionData is clsAllPairwiseEvaluationActionData)
                        {
                            clsAllPairwiseEvaluationActionData AllPwData = (clsAllPairwiseEvaluationActionData)AnytimeAction.ActionData;
                            bool fAlts = AllPwData.ParentNode.IsTerminalNode;

                            StepTask = "";
                            try
                            {
                                StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !fAlts); //IntensityScale
                            }
                            catch
                            {
                                StepTask = "";
                            }
                            
                            var PWType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(AllPwData.ParentNode);
                            qh_help_id = PWType == CanvasTypes.PairwiseType.ptVerbal ? Canvas.PipeParameters.ecEvaluationStepType.VerbalPW : Canvas.PipeParameters.ecEvaluationStepType.GraphicalPW;

                            //if (PWType == CanvasTypes.PairwiseType.ptVerbal && AllPwData.Judgments.Count <= 3)
                            //{
                            //    qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.VerbalPW;
                            //    if ((fAlts && App.ActiveProject.PipeParameters.ForceGraphicalForAlternatives) || (!fAlts && App.ActiveProject.PipeParameters.ForceGraphical))
                            //    {
                            //        PWType = CanvasTypes.PairwiseType.ptGraphical;
                            //        qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.GraphicalPW;
                            //    }
                            //}

                            StepNode = AllPwData.ParentNode;


                            List<ECTypes.KnownLikelihoodDataContract> L = null;
                            if (App.isRiskEnabled && AllPwData.ParentNode.get_MeasureType() == ECCore.ECMeasureType.mtPWAnalogous)
                            {
                                L = AllPwData.ParentNode.GetKnownLikelihoods();
                            }

                            //' D2989 ===
                            clsRatingScale RS = null;
                            if (fIsPWOutcomes && AnytimeAction.ParentNode != null)
                            {
                                if (AnytimeAction.ParentNode.IsAlternative)
                                {
                                    RS = (clsRatingScale)AnytimeAction.PWONode.MeasurementScale;
                                }
                                else
                                {
                                    if (AnytimeAction.ParentNode.get_ParentNode() != null)
                                    {
                                        RS = (clsRatingScale)AnytimeAction.ParentNode.get_ParentNode().MeasurementScale;
                                    }
                                }
                            }
                            // ' D2989 ==

                            List<clsPairwiseLine> Lst = new List<clsPairwiseLine>();
                            var ID = 0;
                            foreach (clsPairwiseMeasureData tJud in AllPwData.Judgments) {
                                clsNode tLeftNode = null;
                                clsNode tRightNode = null;
                                if (fIsPWOutcomes)
                                {
                                    App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(AnytimeAction, tJud, ref tLeftNode, ref tRightNode);
                                }
                                else
                                {
                                    if (fAlts)
                                    {
                                        tLeftNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.FirstNodeID);
                                        tRightNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.SecondNodeID);
                                    }
                                    else
                                    {
                                        tLeftNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.FirstNodeID);
                                        tRightNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.SecondNodeID);
                                    }
                                }

                                Double KnownLikelihoodA = -1;
                                Double KnownLikelihoodB = -1;

                                if (L != null)
                                {
                                    foreach (ECTypes.KnownLikelihoodDataContract tLikelihood in L) {
                                        if (tLikelihood.Value >= 0) {
                                            if (tLikelihood.ID == tLeftNode.NodeID)
                                            {
                                                KnownLikelihoodA = tLikelihood.Value;
                                            }
                                            if (tLikelihood.ID == tRightNode.NodeID)
                                            {
                                                KnownLikelihoodB = tLikelihood.Value;
                                            }
                                        }
                                    }
                                }

                                if (tLeftNode != null && tRightNode != null)
                                {
                                    var PW = new clsPairwiseLine(ID, tLeftNode.NodeID, tRightNode.NodeID, tLeftNode.NodeName, tRightNode.NodeName, tJud.IsUndefined, tJud.Advantage, tJud.Value, tJud.Comment, KnownLikelihoodA, KnownLikelihoodB);

                                    string[] guids = new string[3];
                                    guids[0] = AllPwData.ParentNode.NodeGuidID.ToString();
                                    guids[1] = tLeftNode.NodeGuidID.ToString();
                                    guids[2] = tRightNode.NodeGuidID.ToString();
                                    multi_GUIDs.Add(guids);

                                    if (fIsPWOutcomes && RS != null)
                                    {
                                        clsRating tRating = RS.GetRatingByID(tLeftNode.NodeGuidID);
                                        if (tRating != null)
                                        {
                                            PW.LeftNodeComment = tRating.Comment;
                                        }
                                        tRating = RS.GetRatingByID(tRightNode.NodeGuidID);
                                        if (tRating != null)
                                        {
                                            PW.RightNodeComment = tRating.Comment;
                                        }
                                    }

                                    //assuming that we are showing info docs and wrt docs by default

                                    PW.InfodocLeft = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, tLeftNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, tLeftNode.NodeID.ToString(), tLeftNode.InfoDoc, true, true, -1);
                                    PW.InfodocLeftWRT = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tLeftNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tLeftNode.NodeGuidID, AllPwData.ParentNode.NodeGuidID), true, true, AllPwData.ParentNode.NodeID);
                                    PW.InfodocRight = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, tRightNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, tRightNode.NodeID.ToString(), tRightNode.InfoDoc, true, true, -1);
                                    PW.InfodocRightWRT = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tRightNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tRightNode.NodeGuidID, AllPwData.ParentNode.NodeGuidID), true, true, AllPwData.ParentNode.NodeID);


                                    if (UndefIDx == -1 && PW.isUndefined)
                                    {
                                        UndefIDx = ID;
                                    }
                                    Lst.Add(PW);
                                    ID += 1;
                                    if (tJud.IsUndefined)
                                    {
                                        IsUndefined = true;
                                    }
                                }
                            }
                            PairwiseType = PWType.ToString();
                            MultiPW_Data = Lst;
                            ParentNodeName = AllPwData.ParentNode.NodeName;

                            ParentNodeGUID = AllPwData.ParentNode.NodeGuidID;

                            //skipped focus ID

                            //skipped other details
                            //info docs
                            parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.Node, AllPwData.ParentNode.NodeID.ToString(), AllPwData.ParentNode.InfoDoc, true, true, -1);

                            pipeHelpUrl = TeamTimeClass.ResString(PWType == CanvasTypes.PairwiseType.ptVerbal ? "help_pipe_multiPairwiseVerbal" : "help_pipe_multiPairwiseGraphical");

                            ParentNodeID = AllPwData.ParentNode.NodeID;
                            infodoc_params[0] = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, Guid.Empty, true);
                            infodoc_params[1] = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, Guid.Empty, true);
                            infodoc_params[2] = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, Guid.Empty, true);
                            infodoc_params[3] = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, AllPwData.ParentNode.NodeGuidID, true);
                            infodoc_params[4] = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, AllPwData.ParentNode.NodeGuidID, true);
                        }


                        break;
                    //End Of Multi Pairwise

                    case ActionType.atNonPWOneAtATime:
                        var single_non_pw = (clsOneAtATimeEvaluationActionData)AnytimeAction.ActionData;
                        var ObjHierarchy = (clsHierarchy)App.ActiveProject.ProjectManager.get_Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy);
                        var AltsHierarchy = (clsHierarchy)App.ActiveProject.ProjectManager.get_AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy);

                        if (single_non_pw.Node != null && single_non_pw.Judgment != null)
                        {
                            var measuretype = (clsNonPairwiseEvaluationActionData)AnytimeAction.ActionData;
                            switch (((clsNonPairwiseEvaluationActionData)AnytimeAction.ActionData).MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                    pipeHelpUrl = TeamTimeClass.ResString("help_pipe_rating");
                                    qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.Ratings;
                                    NonPWType = "mtRatings";
                                    //Ratings
                                    clsOneAtATimeEvaluationActionData r_data = (clsOneAtATimeEvaluationActionData)AnytimeAction.ActionData;

                                    clsNonPairwiseMeasureData r_judgment = (clsNonPairwiseMeasureData)r_data.Judgment;

                                    clsRatingMeasureData tJud = (clsRatingMeasureData)r_data.Judgment;

                                    NonPWValue = tJud.IsUndefined ? "-1" : tJud.Rating.Value.ToString();

                                    if (tJud.IsUndefined)
                                    {
                                        NonPWValue = "-1";
                                    }
                                    else
                                    {
                                        NonPWValue = tJud.Rating.Value.ToString();

                                        is_direct = tJud.Rating.ID == -1 ? true : false;
                                    }

                                    comment = tJud.Comment;
                                    //Skipped Params
                                    var r_ParentNode = r_data.Node;
                                    clsNode r_ChildNode = null;
                                    if (r_data.Node.IsAlternative)
                                    {
                                        r_ParentNode = ObjHierarchy.Nodes[0];
                                        r_ChildNode = r_data.Node;
                                    }
                                    else
                                    {
                                        r_ParentNode = r_data.Node;
                                        if (r_ParentNode != null && r_ParentNode.IsTerminalNode)
                                        {
                                            r_ChildNode = AltsHierarchy.GetNodeByID(r_judgment.NodeID);
                                        }
                                        else
                                        {
                                            r_ChildNode = ObjHierarchy.GetNodeByID(r_judgment.NodeID);
                                        }
                                    }

                                    StepNode = r_ParentNode;

                                    StepTask = "";
                                    try
                                    {
                                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !r_ParentNode.IsTerminalNode); //IntensityScale
                                    }
                                    catch
                                    {
                                        StepTask = "";
                                    }

                                    ParentNodeName = r_ParentNode.NodeName;
                                    ChildNodeName = r_ChildNode.NodeName;



                                    ParentNodeGUID = r_ParentNode.NodeGuidID;
                                    LeftNodeGUID = r_ChildNode.NodeGuidID;

                                    infodoc_params[0] = GeckoClass.GetInfodocParams(r_ParentNode.NodeGuidID, Guid.Empty);
                                    infodoc_params[1] = GeckoClass.GetInfodocParams(r_ChildNode.NodeGuidID, Guid.Empty);
                                    infodoc_params[3] = GeckoClass.GetInfodocParams(r_ChildNode.NodeGuidID, r_ParentNode.NodeGuidID);

                                    //skipeed risk controls

                                    clsRatingScale MScale = (clsRatingScale)r_data.MeasurementScale;

                                    if (MScale != null)
                                    {
                                        showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.get_RatingsUseDirectValue(MScale.GuidID);
                                        precision = AnytimeClass.GetPrecisionForRatings((clsRatingScale)MScale);
                                        scaleDescriptions.Add(new
                                        {
                                            Name = MScale.Name,
                                            Guid = MScale.GuidID.ToString(),
                                            Description = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.MeasureScale, MScale.GuidID.ToString(), MScale.Comment, true, true, -1)
                                        });
                                    }

                                    foreach (clsRating intensity in MScale.RatingSet)
                                    {
                                        intensities.Add(new string[] { intensity.Value.ToString(), intensity.Name.ToString(), intensity.ID.ToString(), intensity.Priority.ToString(), intensity.Comment });
                                    }

                                    intensities.Add(new string[] { "-1", "Not Rated", "-1", "0", "" });
                                    intensities.Add(new string[] { "0", "Direct Value", "-2", "0", "" });


                                    first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, r_ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, r_ChildNode.NodeID.ToString(), r_ChildNode.InfoDoc, true, true, -1);
                                    parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, r_ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, r_ParentNode.NodeID.ToString(), r_ParentNode.InfoDoc, true, true, -1);
                                    wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, r_ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(r_ChildNode.NodeGuidID, r_ParentNode.NodeGuidID), true, true, r_ParentNode.NodeID);

                                    if (tJud.IsUndefined)
                                    {
                                        IsUndefined = true;
                                    }
                                    ParentNodeID = r_data.Node.NodeID;
                                    //End Ratings
                                    break;
                                case ECMeasureType.mtDirect:
                                    qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.DirectInput;
                                    NonPWType = "mtDirect";
                                    clsNode d_tAlt = null;
                                    clsNode d_tParentNode = null;
                                    clsOneAtATimeEvaluationActionData d_data = (clsOneAtATimeEvaluationActionData)AnytimeAction.ActionData;

                                    clsNonPairwiseMeasureData d_judgment = (clsNonPairwiseMeasureData)d_data.Judgment;

                                    clsDirectMeasureData d_measure_data = (clsDirectMeasureData)d_data.Judgment;

                                    comment = d_measure_data.Comment;

                                    if (d_measure_data != null) {
                                        if (!d_measure_data.IsUndefined) {
                                            if (Convert.ToDecimal(d_measure_data.ObjectValue.ToString()) < 0)
                                            {
                                                NonPWValue = "0";
                                            }

                                            else if (Convert.ToDecimal(d_measure_data.ObjectValue.ToString()) > 1)
                                            {
                                                NonPWValue = "1";
                                            }
                                            else {
                                                //no precisions first
                                                precision = 4;
                                                NonPWValue = StringFuncs.JS_SafeNumber(d_measure_data.ObjectValue);
                                            }

                                        }
                                    }


                                    //skipped risk controls
                                    if (AnytimeAction.ActionData != null && d_data.Judgment != null)
                                    {
                                        var DirectData = (clsDirectMeasureData)d_data.Judgment;
                                        d_tParentNode = d_data.Node;
                                        StepNode = d_tParentNode;

                                        if (d_tParentNode.IsTerminalNode)
                                        {
                                            d_tAlt = AltsHierarchy.GetNodeByID(DirectData.NodeID);
                                        }
                                        else
                                        {
                                            d_tAlt = ObjHierarchy.GetNodeByID(DirectData.NodeID);
                                        }

                                        StepTask = "";
                                        try
                                        {
                                            StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !d_tParentNode.IsTerminalNode); //IntensityScale
                                        }
                                        catch
                                        {
                                            StepTask = "";
                                        }

                                        ParentNodeName = d_tParentNode.NodeName;
                                        ChildNodeName = d_tAlt.NodeName;

                                        ParentNodeGUID = d_tParentNode.NodeGuidID;
                                        LeftNodeGUID = d_tAlt.NodeGuidID;

                                        infodoc_params[0] = GeckoClass.GetInfodocParams(d_tParentNode.NodeGuidID, Guid.Empty);
                                        infodoc_params[1] = GeckoClass.GetInfodocParams(d_tAlt.NodeGuidID, Guid.Empty);
                                        infodoc_params[3] = GeckoClass.GetInfodocParams(d_tAlt.NodeGuidID, d_tParentNode.NodeGuidID);


                                        first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, d_tAlt.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, d_tAlt.NodeID.ToString(), d_tAlt.InfoDoc, true, true, -1);
                                        parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, d_tAlt.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, d_tParentNode.NodeID.ToString(), d_tParentNode.InfoDoc, true, true, -1);
                                        wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, d_tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(d_tAlt.NodeGuidID, d_tParentNode.NodeGuidID), true, true, d_tParentNode.NodeID);

                                    }

                                    if (d_measure_data.IsUndefined)
                                    {
                                        IsUndefined = true;
                                    }
                                    ParentNodeID = d_data.Node.NodeID;
                                    break;

                                case ECMeasureType.mtStep:
                                    {
                                        pipeHelpUrl = TeamTimeClass.ResString("help_pipe_stepFunction");
                                        qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.StepFunction;
                                        clsOneAtATimeEvaluationActionData StepActionData = (clsOneAtATimeEvaluationActionData)AnytimeAction.ActionData;
                                        NonPWType = StepActionData.MeasurementType.ToString();
                                        var StepData = (clsStepMeasureData)StepActionData.Judgment;
                                        StepData.StepFunction.SortByInterval();

                                        comment = StepData.Comment;

                                        var intervalx = new string[StepData.StepFunction.Intervals.Count]; var intervaly = new string[StepData.StepFunction.Intervals.Count];
                                        foreach (clsStepInterval period in StepData.StepFunction.Intervals)
                                        {
                                            var ind = StepData.StepFunction.Intervals.IndexOf(period);
                                            intervalx[ind] = period.Low.ToString();
                                            intervaly[ind] = (period.Value).ToString();
                                        }

                                        step_intervals.Add(intervalx); step_intervals.Add(intervaly);
                                        clsNode ChildNode;
                                        if (single_non_pw.Node.IsTerminalNode) { ChildNode = AltsHierarchy.GetNodeByID(StepData.NodeID); } else { ChildNode = ObjHierarchy.GetNodeByID(StepData.NodeID); }
                                        var StepParentNode = single_non_pw.Node;

                                        StepNode = StepParentNode;

                                        first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ChildNode.NodeID.ToString(), ChildNode.InfoDoc, true, true, -1);
                                        parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, StepParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, StepParentNode.NodeID.ToString(), StepParentNode.InfoDoc, true, true, -1);
                                        wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, StepParentNode.NodeGuidID), true, true, StepParentNode.NodeID);
                                        ParentNodeName = StepParentNode.NodeName;
                                        ChildNodeName = ChildNode.NodeName;
                                        ParentNodeGUID = StepParentNode.NodeGuidID;
                                        LeftNodeGUID = ChildNode.NodeGuidID;

                                        infodoc_params[0] = GeckoClass.GetInfodocParams(StepParentNode.NodeGuidID, Guid.Empty);
                                        infodoc_params[1] = GeckoClass.GetInfodocParams(ChildNode.NodeGuidID, Guid.Empty);
                                        infodoc_params[3] = GeckoClass.GetInfodocParams(ChildNode.NodeGuidID, StepParentNode.NodeGuidID);

                                        StepTask = "";
                                        try
                                        {
                                            StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !StepParentNode.IsTerminalNode); //IntensityScale
                                        }
                                        catch
                                        {
                                            StepTask = "";
                                        }

                                        var step_value = new double();

                                        if (!Double.IsNaN(Convert.ToDouble(StepData.ObjectValue)))
                                            step_value = Convert.ToDouble(StepData.ObjectValue);
                                        else
                                            step_value = ECCore.TeamTimeFuncs.TeamTimeFuncs.UndefinedValue;

                                        CurrentValue = Convert.ToDouble(step_value);
                                        piecewise = StepData.StepFunction.IsPiecewiseLinear;
                                        ParentNodeID = StepActionData.Node.NodeID;

                                        var SF = (clsStepFunction)StepActionData.MeasurementScale;
                                        float Low = ((clsStepInterval)SF.Intervals[0]).Low;
                                        float High = ((clsStepInterval)SF.Intervals[SF.Intervals.Count - 1]).Low;
                                        var XMinValue = Low - (High - Low) / 10;
                                        var XMaxValue = High + (High - Low) / 10;
                                        PipeParameters = new
                                        {
                                            min = XMinValue,
                                            max = XMaxValue
                                        };

                                        IsUndefined = StepData.IsUndefined;
                                    }
                                    break;
                                case ECMeasureType.mtRegularUtilityCurve:
                                case ECMeasureType.mtAdvancedUtilityCurve:
                                    {
                                        pipeHelpUrl = TeamTimeClass.ResString("help_pipe_utilityCurve");
                                        qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.UtilityCurve;
                                        clsNode tParentNode = null;
                                        clsNode tAlt = null;
                                        clsOneAtATimeEvaluationActionData tData = (clsOneAtATimeEvaluationActionData)AnytimeAction.ActionData;
                                        //skip risk controls

                                        NonPWType = tData.MeasurementType.ToString();
                                        var tData_Judgment = (clsUtilityCurveMeasureData)tData.Judgment;

                                        //RUC Data
                                        var RUC = (clsRegularUtilityCurve)tData.MeasurementScale;
                                        var XMinValue = RUC.Low;
                                        var XMaxValue = RUC.High;
                                        var Curvature = StringFuncs.JS_SafeString(RUC.Curvature);
                                        var Decreasing = (!RUC.IsIncreasing).ToString().ToLower();
                                        double XValue = 0;

                                        if (tData.Judgment != null)
                                        {
                                            var RUCData = tData_Judgment;
                                            if (RUCData.IsUndefined)
                                            {
                                                XValue = -2147483648000;
                                            }
                                            else
                                            {
                                                //XValue = Math.Round(RUCData.Data, 2);
                                                XValue = RUCData.Data;
                                            }
                                        }

                                        UCData = new
                                        {
                                            RUC = RUC,
                                            XMinValue = XMinValue,
                                            XMaxValue = XMaxValue,
                                            Curvature = Curvature,
                                            XValue = XValue,
                                            Decreasing = Decreasing
                                        };

                                        tParentNode = tData.Node;
                                        StepNode = tParentNode;

                                        if (tParentNode.IsTerminalNode)
                                        {
                                            tAlt = AltsHierarchy.GetNodeByID(tData_Judgment.NodeID);
                                        }
                                        else
                                        {
                                            tAlt = ObjHierarchy.GetNodeByID(tData_Judgment.NodeID);
                                        }


                                        first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, tAlt.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, tAlt.NodeID.ToString(), tAlt.InfoDoc, true, true, -1);
                                        parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, tParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, tParentNode.NodeID.ToString(), tParentNode.InfoDoc, true, true, -1);

                                        wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, tParentNode.NodeGuidID), true, true, tParentNode.NodeID);

                                        ParentNodeName = tParentNode.NodeName;
                                        ChildNodeName = tAlt.NodeName;

                                        ParentNodeGUID = tParentNode.NodeGuidID;
                                        LeftNodeGUID = tAlt.NodeGuidID;

                                        infodoc_params[0] = GeckoClass.GetInfodocParams(tParentNode.NodeGuidID, Guid.Empty);
                                        infodoc_params[1] = GeckoClass.GetInfodocParams(tAlt.NodeGuidID, Guid.Empty);
                                        infodoc_params[3] = GeckoClass.GetInfodocParams(tAlt.NodeGuidID, tParentNode.NodeGuidID);

                                        StepTask = "";
                                        try
                                        {
                                            StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !tParentNode.IsTerminalNode); //IntensityScale
                                        }
                                        catch
                                        {
                                            StepTask = "";
                                        }

                                        comment = tData_Judgment.Comment;
                                        ParentNodeID = tData.Node.NodeID;
                                        IsUndefined = tData_Judgment.IsUndefined;
                                        
                                        break;
                                    }
                            }
                        }
                        break;
                    //Non Pairwise

                    //multi direct/ratings
                    case ActionType.atNonPWAllChildren:
                    case ActionType.atNonPWAllCovObjs:
                        clsNonPairwiseEvaluationActionData now_pw_all = (clsNonPairwiseEvaluationActionData)AnytimeAction.ActionData;
                        switch (now_pw_all.MeasurementType)
                        {
                            //multi ratings
                            case ECMeasureType.mtRatings:
                                pipeHelpUrl = TeamTimeClass.ResString("help_pipe_rating");
                                qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.MultiRatings;
                                NonPWType = "mtRatings";

                                List<clsRating> Ratings = null;
                                clsMeasureScales MeasureScales = App.ActiveProject.ProjectManager.MeasureScales;

                                int UndefIdx = -1;

                                //option 1
                                if (AnytimeAction.ActionData is clsAllChildrenEvaluationActionData)
                                {
                                    clsAllChildrenEvaluationActionData MultiNonPWData = (clsAllChildrenEvaluationActionData)AnytimeAction.ActionData;

                                    StepNode = MultiNonPWData.ParentNode;

                                    if (MeasureScales != null)
                                    {
                                        if (MeasureScales.RatingsScales != null)
                                        {
                                            clsRatingScale RS = MeasureScales.GetRatingScaleByID(MultiNonPWData.ParentNode.get_RatingScaleID());
                                            if (RS != null)
                                            {
                                                showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.get_RatingsUseDirectValue(RS.GuidID);

                                                Ratings = new List<clsRating>();
                                                foreach (clsRating tRating in RS.RatingSet)
                                                {
                                                    //var tNewRating = new clsRating(tRating.ID, tRating.Name, tRating.Value, tRating.RatingScale, tRating.Comment);

                                                    var tNewRating = new clsRating(tRating.ID, tRating.Name,
                                                        tRating.Value, null, tRating.Comment);

                                                    tNewRating.GuidID = tRating.GuidID;
                                                    Ratings.Add(tNewRating);

                                                }
                                            }

                                            precision = AnytimeClass.GetPrecisionForRatings((clsRatingScale)RS);
                                        }
                                    }

                                    if (Ratings != null)
                                    {
                                        Ratings.Add(new clsRating(-1, "Not Rated", 0, null));
                                        Ratings.Add(new clsRating(-2, "Direct Value", 0, null));
                                    }

                                    List<clsRatingLine> Lst = new List<clsRatingLine>();
                                    var ID = 0;
                                    foreach (clsNode tAlt in MultiNonPWData.Children) {

                                        clsRatingMeasureData R = (clsRatingMeasureData)MultiNonPWData.GetJudgment(tAlt);
                                        int RID = -1;

                                        RID = R.Rating != null ? R.Rating.ID : RID;
                                        UndefIdx = UndefIdx == -1 && R.IsUndefined ? ID : UndefIdx;
                                        
                                        first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, tAlt.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, tAlt.NodeID.ToString(), tAlt.InfoDoc, true, true, -1);

                                        wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, MultiNonPWData.ParentNode.NodeGuidID), true, true, MultiNonPWData.ParentNode.NodeID);

                                        Single DV = -1;
                                        DV = R.Rating != null && R.Rating.RatingScaleID < 0 && R.Rating.ID < 0 ? R.Rating.Value : DV;

                                        Lst.Add(new clsRatingLine(tAlt.NodeID, R.RatingScale.GuidID.ToString(), StringFuncs.JS_SafeHTML(tAlt.NodeName), Ratings, RID, first_node_info, R.Comment, DV, "", "", wrt_first_node_info, "", "", "")); //"" are comments and info docs url

                                        // Lst[ID].Ratings = Ratings.ToList();

                                        ID++;

                                        if (R.IsUndefined)
                                        {
                                            IsUndefined = true;
                                        }

                                        scaleDescriptions.Add(new
                                        {
                                            Name = R.RatingScale.Name,
                                            Guid = R.RatingScale.GuidID.ToString(),
                                            Description = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.MeasureScale, R.RatingScale.GuidID.ToString(), R.RatingScale.Comment, true, true, -1)
                                        });
                                    }

                                    StepTask = "";
                                    try
                                    {
                                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !MultiNonPWData.ParentNode.IsTerminalNode); //IntensityScale
                                    }
                                    catch
                                    {
                                        StepTask = "";
                                    }

                                    MultiNonPW_Data = Lst;
                                    ParentNodeName = MultiNonPWData.ParentNode.NodeName;

                                    parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, MultiNonPWData.ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, MultiNonPWData.ParentNode.NodeID.ToString(), MultiNonPWData.ParentNode.InfoDoc, true, true, -1);

                                    ParentNodeID = MultiNonPWData.ParentNode.NodeID;
                                    ParentNodeGUID = MultiNonPWData.ParentNode.NodeGuidID;
                                    infodoc_params[0] = GeckoClass.GetInfodocParams(MultiNonPWData.ParentNode.NodeGuidID, Guid.Empty, true);
                                    infodoc_params[1] = GeckoClass.GetInfodocParams(MultiNonPWData.ParentNode.NodeGuidID, Guid.Empty, true);
                                    infodoc_params[2] = GeckoClass.GetInfodocParams(MultiNonPWData.ParentNode.NodeGuidID, Guid.Empty, true);
                                }
                                // option 1


                                //option 2
                                if (AnytimeAction.ActionData is clsAllCoveringObjectivesEvaluationActionData)
                                {
                                    clsAllCoveringObjectivesEvaluationActionData MultiNonPWData = (clsAllCoveringObjectivesEvaluationActionData)AnytimeAction.ActionData;

                                    List<clsRatingLine> Lst = new List<clsRatingLine>();
                                    int ID = 0;


                                    foreach (clsNode tCovObj in MultiNonPWData.CoveringObjectives) {

                                        if (MeasureScales != null)
                                        {
                                            if (MeasureScales.RatingsScales != null)
                                            {
                                                clsRatingScale RS = MeasureScales.GetRatingScaleByID(tCovObj.get_RatingScaleID());
                                                if (RS != null)
                                                {
                                                    showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.get_RatingsUseDirectValue(RS.GuidID);
                                                    scaleDescriptions.Add(new
                                                    {
                                                        Name = RS.Name,
                                                        Guid = RS.GuidID.ToString(),
                                                        Description = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.MeasureScale, RS.GuidID.ToString(), RS.Comment, true, true, -1)
                                                    });

                                                    Ratings = new List<clsRating>();
                                                    foreach (clsRating tRating in RS.RatingSet)
                                                    {
                                                        //var tNewRating = new clsRating(tRating.ID, tRating.Name, tRating.Value, tRating.RatingScale, tRating.Comment);

                                                        var tNewRating = new clsRating(tRating.ID, tRating.Name, tRating.Value, null, tRating.Comment);

                                                        tNewRating.GuidID = tRating.GuidID;
                                                        Ratings.Add(tNewRating);

                                                    }
                                                }
                                                precision = AnytimeClass.GetPrecisionForRatings((clsRatingScale)RS);

                                            }
                                        }

                                        if (Ratings != null)
                                        {
                                            Ratings.Add(new clsRating(-1, "Not Rated", 0, null));
                                            Ratings.Add(new clsRating(-2, "Direct Value", 0, null));
                                        }

                                        if (MultiNonPWData.GetJudgment(tCovObj) is clsRatingMeasureData)
                                        {
                                            clsRatingMeasureData R = (clsRatingMeasureData)MultiNonPWData.GetJudgment(tCovObj);

                                            int RID = R.Rating != null ? R.Rating.ID : -1;
                                            UndefIdx = R.IsUndefined && UndefIdx == -1 ? ID : UndefIdx;

                                            first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, tCovObj.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, tCovObj.NodeID.ToString(), tCovObj.InfoDoc, true, true, -1);

                                            wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, MultiNonPWData.Alternative.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(MultiNonPWData.Alternative.NodeGuidID, tCovObj.NodeGuidID), true, true, tCovObj.NodeID);
                                            Single DV = -1;
                                            DV = R.Rating != null && R.Rating.RatingScaleID < 0 && R.Rating.ID < 0 ? R.Rating.Value : DV;

                                            Lst.Add(new clsRatingLine(tCovObj.NodeID, R.RatingScale.GuidID.ToString(), StringFuncs.JS_SafeHTML(tCovObj.NodeName), Ratings, RID, first_node_info, R.Comment, DV, "", "", wrt_first_node_info, "", "", "")); //"" are comments and info docs url

                                            // Lst[ID].Ratings = Ratings.ToList();
                                            ID++;

                                            if (R.IsUndefined)
                                            {
                                                IsUndefined = true;
                                            }
                                        }
                                    }

                                    StepTask = "";
                                    try
                                    {
                                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !MultiNonPWData.Alternative.IsTerminalNode); //IntensityScale
                                    }
                                    catch
                                    {
                                        StepTask = "";
                                    }

                                    MultiNonPW_Data = Lst;
                                    ParentNodeName = MultiNonPWData.Alternative.NodeName;

                                    parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, MultiNonPWData.Alternative.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, MultiNonPWData.Alternative.NodeID.ToString(), MultiNonPWData.Alternative.InfoDoc, true, true, -1);
                                    ParentNodeID = MultiNonPWData.Alternative.NodeID;
                                    ParentNodeGUID = MultiNonPWData.Alternative.NodeGuidID;
                                    infodoc_params[0] = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, true);
                                    infodoc_params[1] = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, true);
                                    infodoc_params[2] = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, true);
                                }

                                //optiont 2
                                break;
                            //end of multi ratings

                            //multi direct
                            case ECMeasureType.mtDirect:
                                pipeHelpUrl = TeamTimeClass.ResString("help_pipe_directEntry");
                                qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.MultiDirectInput;
                                //option 1
                                NonPWType = "mtDirect";
                                if (AnytimeAction.ActionData is clsAllChildrenEvaluationActionData)
                                {
                                    clsAllChildrenEvaluationActionData MultiDirectData1 = (clsAllChildrenEvaluationActionData)AnytimeAction.ActionData;

                                    StepNode = MultiDirectData1.ParentNode;

                                    //question
                                    StepTask = "";
                                    try
                                    {
                                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !MultiDirectData1.ParentNode.IsTerminalNode); //IntensityScale
                                    }
                                    catch
                                    {
                                        StepTask = "";
                                    }
                                    //question

                                    //Parent Node Data
                                    ParentNodeName = MultiDirectData1.ParentNode.NodeName;

                                    parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, MultiDirectData1.ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, MultiDirectData1.ParentNode.NodeID.ToString(), MultiDirectData1.ParentNode.InfoDoc, true, true, -1);
                                    //Parent Node Data

                                    //get List of Direct Inputs
                                    List<clsRatingLine> Lst = new List<clsRatingLine>();
                                    var ID = 0;
                                    foreach (clsNode tAlt in MultiDirectData1.Children)
                                    {

                                        clsDirectMeasureData DD = (clsDirectMeasureData)MultiDirectData1.GetJudgment(tAlt);

                                        first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, tAlt.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, tAlt.NodeID.ToString(), tAlt.InfoDoc, true, true, -1);

                                        wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, MultiDirectData1.ParentNode.NodeGuidID), true, true, MultiDirectData1.ParentNode.NodeID);


                                        Single DV = -1;
                                        DV = DD.IsUndefined ? DV : DD.DirectData;

                                        Lst.Add(new clsRatingLine(tAlt.NodeID, tAlt.NodeGuidID.ToString(), StringFuncs.JS_SafeHTML(tAlt.NodeName), null, -1, first_node_info, DD.Comment, DV, "", "", wrt_first_node_info, "", "", "")); //"" are comments and info docs url
                                        ID++;
                                        if (DD.IsUndefined)
                                        {
                                            IsUndefined = true;
                                        }
                                    }
                                    MultiNonPW_Data = Lst;
                                    ParentNodeID = MultiDirectData1.ParentNode.NodeID;
                                    ParentNodeGUID = MultiDirectData1.ParentNode.NodeGuidID;
                                    infodoc_params[0] = GeckoClass.GetInfodocParams(MultiDirectData1.ParentNode.NodeGuidID, Guid.Empty, true);
                                    infodoc_params[1] = GeckoClass.GetInfodocParams(MultiDirectData1.ParentNode.NodeGuidID, Guid.Empty, true);
                                    infodoc_params[2] = GeckoClass.GetInfodocParams(MultiDirectData1.ParentNode.NodeGuidID, Guid.Empty, true);
                                }
                                //option 1

                                //option 2
                                if (AnytimeAction.ActionData is clsAllCoveringObjectivesEvaluationActionData)
                                {
                                    clsAllCoveringObjectivesEvaluationActionData MultiNonPWData = (clsAllCoveringObjectivesEvaluationActionData)AnytimeAction.ActionData;

                                    List<clsRatingLine> Lst = new List<clsRatingLine>();
                                    int ID = 0;


                                    foreach (clsNode tNode in MultiNonPWData.CoveringObjectives)
                                    {
                                        clsDirectMeasureData DD = (clsDirectMeasureData)MultiNonPWData.GetJudgment(tNode);

                                        first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, tNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, tNode.NodeID.ToString(), tNode.InfoDoc, true, true, -1);

                                        wrt_first_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, MultiNonPWData.Alternative.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(MultiNonPWData.Alternative.NodeGuidID, tNode.NodeGuidID), true, true, tNode.NodeID);

                                        Single DV = -1;
                                        DV = DD.IsUndefined ? -1 : DD.DirectData;

                                        Lst.Add(new clsRatingLine(tNode.NodeID, tNode.NodeGuidID.ToString(), StringFuncs.JS_SafeHTML(tNode.NodeName), null, -1, first_node_info, DD.Comment, DV, "", "", wrt_first_node_info, "", "", "")); //info docs url
                                        ID++;

                                        if (DD.IsUndefined)
                                        {
                                            IsUndefined = true;
                                        }

                                    }

                                    StepTask = "";
                                    try
                                    {
                                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, null, AnytimeClass.IsImpact && !MultiNonPWData.Alternative.IsTerminalNode); //IntensityScale
                                    }
                                    catch
                                    {
                                        StepTask = "";
                                    }

                                    MultiNonPW_Data = Lst;
                                    ParentNodeName = MultiNonPWData.Alternative.NodeName;

                                    parent_node_info = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, MultiNonPWData.Alternative.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, MultiNonPWData.Alternative.NodeID.ToString(), MultiNonPWData.Alternative.InfoDoc, true, true, -1);
                                    ParentNodeID = MultiNonPWData.Alternative.NodeID;
                                    ParentNodeGUID = MultiNonPWData.Alternative.NodeGuidID;
                                    infodoc_params[0] = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, true);
                                    infodoc_params[1] = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, true);
                                    infodoc_params[2] = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, true);
                                }

                                //option 2

                                break;
                                //end of multi direct
                        }
                        break;
                    //end of multu direct/ratings

                    //Local Results     
                    case ActionType.atShowLocalResults:
                        context.Session["ObjData"] = null;
                        context.Session["PairsData"] = null;
                        qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.IntermediateResults;
                        var localresultsdata = (clsShowLocalResultsActionData)AnytimeAction.ActionData;
                        StepNode = localresultsdata.ParentNode;
                        try
                        {
                            //D1503 -changed the wording there.
                            StepTask = TeamTimeClass.GetPipeStepTask(AnytimeClass.Action((int)CurrentStep), null, false, false, false);
                        }
                        catch
                        {
                            StepTask = "";
                        }
                        ParentNodeID = localresultsdata.ParentNode.NodeID;
                        ParentNodeGUID = localresultsdata.ParentNode.NodeGuidID;
                        PipeParameters = AnytimeClass.CreateLocalResults((int)CurrentStep);

                        break;
                    //Local Results       

                    //Global Results
                    case ActionType.atShowGlobalResults:
                        question = App.ActiveProject.PipeParameters.NameObjectives;
                        qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.OverallResults;
                        StepTask = "";
                        try
                        {
                            StepTask = TeamTimeClass.GetPipeStepTask(AnytimeClass.Action(step), null, false, false, false);
                        }
                        catch
                        {
                            StepTask = "";
                        }
                        var globalresultsdata = (clsShowGlobalResultsActionData)AnytimeAction.ActionData;
                        PipeParameters = AnytimeClass.CreateGlobalResults((int)CurrentStep);

                        ParentNodeID = globalresultsdata.WRTNode.NodeID;
                        break;
                    //Global Results
                    case ActionType.atSpyronSurvey:
                        qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.Survey;
                        PipeParameters = AnytimeClass.CreateSurvey((int)CurrentStep);
                        break;

                    case ActionType.atSensitivityAnalysis:
                        StepTask = "";
                        clsSensitivityAnalysisActionData sensitivities = (clsSensitivityAnalysisActionData)AnytimeAction.ActionData;
                        SensitivitiesAnalysis sensitivitiesAnalysis = new SensitivitiesAnalysis();
                        sensitivitiesAnalysis.clearData();
                        sensitivitiesAnalysis.CurrentUserID = App.ActiveProject.ProjectManager.UserID;

                        if (App.Options.BackDoor == ECWeb.Options._BACKDOOR_PLACESRATED)
                        {
                            sensitivitiesAnalysis.Opt_ShowMaxAltsCount = 10;
                            sensitivitiesAnalysis.SAUserID = App.ActiveProject.ProjectManager.UserID;
                        }
                        else
                        {
                            sensitivitiesAnalysis.SAUserID = (App.ActiveProject.PipeParameters.CalculateSAForCombined ? ECTypes.COMBINED_USER_ID : App.ActiveProject.ProjectManager.UserID);
                        }

                        saType = sensitivities.SAType.ToString();
                        switch (sensitivities.SAType)
                        {
                            case SAType.satDynamic:
                                StepTask = TeamTimeClass.ResString("lblEvaluationDynamicSA");
                                qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.DynamicSA;
                                // HelpID = 8
                                break;
                            case SAType.satGradient:
                                StepTask = TeamTimeClass.ResString("lblEvaluationGradientSA");
                                qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.GradientSA;
                                // HelpID = 9
                                break;
                            case SAType.satPerformance:
                                // HelpID = 11
                                StepTask = TeamTimeClass.ResString("lblEvaluationPerformanceSA");
                                qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.PerformanceSA;
                                break;
                        }

                        string sSeeing = "";
                        var SAUserID = sensitivitiesAnalysis.SAUserID;
                        var msgSeeingCombined = sensitivitiesAnalysis.msgSeeingCombined;
                        var CurrentUserID = sensitivitiesAnalysis.CurrentUserID;
                        var msgSeeingIndividual = sensitivitiesAnalysis.msgSeeingIndividual;
                        var ProjectManager = sensitivitiesAnalysis.ProjectManager;
                        var msgSeeingUser = sensitivitiesAnalysis.msgSeeingUser;
                        if (sensitivitiesAnalysis.SAUserID == ECTypes.COMBINED_USER_ID)
                        {
                            sSeeing = msgSeeingCombined;
                        }
                        else if (SAUserID == CurrentUserID)
                        {
                            sSeeing = msgSeeingIndividual;
                        }
                        else
                        {
                            if (SAUserID != int.MinValue && ProjectManager.User != null)
                            {
                                string sUserEmail = "";
                                ECTypes.clsUser tUser = ProjectManager.GetUserByID(SAUserID);
                                if ((tUser != null))
                                    sUserEmail = tUser.UserEMail;
                                sSeeing = string.Format(msgSeeingUser, sUserEmail);
                            }
                        }

                        StepTask += sSeeing != "" ? " " + sSeeing : "";
                        break;
                }


                if (HttpContext.Current.Session[InconsistencySortingEnabled] == null)
                {
                    HttpContext.Current.Session[InconsistencySortingEnabled] = false;
                    HttpContext.Current.Session[BestFit] = false;
                }

                if (StepNode != null)
                {
                    qh_tnode_id = StepNode.IsAlternative ? -StepNode.NodeID : StepNode.NodeID;
                }
                else
                {

                }

                bool AutoShow = show_qh_automatically;
                bool isCluster = false; // D4082
                qh_info = App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, step, ref isCluster, ref AutoShow);

                show_qh_automatically = AutoShow;
                qh_info = Regex.Replace(qh_info, "<title>.*?</title>", "");

                var qh_info_plain_text = qh_info;

                if (!qh_info.Contains("img"))
                {
                    qh_info_plain_text = Regex.Replace(qh_info, "<.*?>", "").Trim();
                }
                if (qh_info_plain_text != "")
                {
                    //removed code here
                }
                else
                {
                    if (isPM)
                    {
                        App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.SetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, step, false, AutoShow, "");   // D4082
                    }
                }

                string ObjID = InfodocService.GetQuickHelpObjectID(qh_help_id, StepNode);
                AnytimeClass.SetQuickHelpObjectIdInSession(ObjID);
                qh_info = InfodocService.Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, Consts.reObjectType.QuickHelp, ObjID, qh_info, true, true, -1);
                //qh_info = TeamTimeClass.ParseAllTemplates(qh_info, App.ActiveUser, App.ActiveProject);

                qh_yt_info = StringFuncs.ParseVideoLinks(qh_info, false);

                string defaultQhInfo = GetDefaultQhInfo(App, qh_help_id);

                if (HttpContext.Current.Session[qh_yt_info] != null)
                {
                    is_qh_shown = true;
                }

                
                var steps = "";
                
                if (PreviousStep != step && !(PreviousStep < 1 || PreviousStep > App.ActiveProject.Pipe.Count))
                    steps = AnytimeClass.get_StepInformation(App, PreviousStep);


                var extremeMessage = context.Request.Cookies[Constants.Cook_Extreme] != null;
                bool showAutoAdvanceModal = ShowAutoAdvanceModal(App, AnytimeClass.Action(step), PairwiseType, NonPWType);
                
                //ascx pull content
                var userControlContent = AnytimeAction.ActionType != ActionType.atInformationPage ? getUserControl(step) : "";
                
                bool HasCurStepCluster = false;
                var QHApplySteps = new List<int>();
                var ApplyNodes = new List<clsNode>();
                ApplyNodes = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeStepClusters(step, ref HasCurStepCluster, ref QHApplySteps);

                var apply_to = new List<string>();

                var qqq = ApplyNodes;

                var cluster_nodes = new List<string[]>();
                int n = 0;
               foreach(clsNode node in ApplyNodes)
               {
                    string[] arr = { node.NodeName.ToString(), QHApplySteps[n].ToString(), node.NodeID.ToString(), node.ParentNodeID.ToString() };
                    cluster_nodes.Add(arr);
                    n++;
               }

                string baseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
            HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/";
               var hashLink =  baseUrl + GeckoClass.CreateLogonURL(App.ActiveUser, App.ActiveProject, false, "", "");

               if (context.Request.Cookies["HideWarningMessage"] == null)
               {
                   var warningCookie = new HttpCookie("HideWarningMessage", "1")
                   {
                       HttpOnly = true,
                       Expires = DateTime.Now.AddDays(1)
                   };

                   context.Request.Cookies.Add(warningCookie);
               }

               var nextProject = AnytimeClass.GetNextProject(App.ActiveProject);
               var nextProjectId = nextProject == null ? -1 : nextProject.ID;

                //bool hideWarning = (bool) context.Session["HideWarningMessage"];
                bool hideWarning = context.Request.Cookies["HideWarningMessage"].Value == "1";

                parent_node_info = RemoveInfoDcoImageStyle(parent_node_info);
                HttpContext.Current.Session[SessionParentNodeGuid] = ParentNodeGUID;

                output = new
                {
                    hashLink = hashLink,
                    status = "active",
                    pipeOptions = pipeOptions,
                    showinfodocnode = App.ActiveProject.PipeParameters.ShowInfoDocs,
                    current_step = step,
                    previous_step = PreviousStep,
                    total_pipe_steps = App.ActiveProject.Pipe.Count,
                    is_first_time = isFirstTime,
                    show_auto_advance_modal = showAutoAdvanceModal,
                    first_unassessed_step = firstUnassessedStep,
                    help_pipe_root = TeamTimeClass.ResString("help_pipe_root"),
                    help_pipe_url = pipeHelpUrl,
                    page_type = AnytimeAction.ActionType.ToString(),
                    pairwise_type = PairwiseType,
                    first_node = FirstNodeName,
                    second_node = SecondNodeName,
                    parent_node = ParentNodeName,
                    first_node_info = StringFuncs.isHTMLEmpty(first_node_info) ? "" : first_node_info,
                    second_node_info = StringFuncs.isHTMLEmpty(second_node_info) ? "" : second_node_info,
                    parent_node_info = StringFuncs.isHTMLEmpty(parent_node_info) ? "" : parent_node_info,
                    wrt_first_node_info = StringFuncs.isHTMLEmpty(wrt_first_node_info) ? "" : wrt_first_node_info,
                    wrt_second_node_info = StringFuncs.isHTMLEmpty(wrt_second_node_info) ? "" : wrt_second_node_info,
                    ScaleDescriptions = scaleDescriptions,
                    question = question,
                    wording = wording,
                    nameAlternatives = App.ActiveProject.PipeParameters.NameAlternatives,
                    information_message = InformationMessage,
                    step_task = StepTask,
                    value = PWValue,
                    advantage = PWAdvantage,
                    IsUndefined = IsUndefined,
                    sRes = context.Session["sRes"] == null ? "" : App.ResString((string)context.Session["sRes"]),
                    current_value = CurrentValue,
                    comment = comment,
                    show_comments = show_comments,
                    ShowUnassessed = AnytimeClass.ShowUnassessed,
                    nextUnassessedStep = GetNextUnassessed(step),
                    steps = steps,
                    stepButtons = GeckoClass.loadStepButtons(CurrentStep),
                    usersComments = "",
                    currentUserEmail = App.ActiveUser.UserEMail,
                    extremeMessage = extremeMessage,
                    pipeWarning = PipeWarning,


                    //below is for results pages only
                    sess_wrt_node_id = ((clsNode)context.Session[Sess_WrtNode]).NodeID,
                    parentnodeID = ParentNodeID,
                    orderbypriority = (bool)HttpContext.Current.Session[InconsistencySortingEnabled],
                    bestfit = (bool)HttpContext.Current.Session[BestFit],
                    dont_show = (bool)HttpContext.Current.Session["showMessage"],
                    multi_GUIDs = multi_GUIDs,
                    //for multipaiwise
                    multi_pw_data = MultiPW_Data,
                    multi_infodoc_params = multi_infodoc_params,
                    //end of multipairwise

                    //for non pw one at a time
                    non_pw_type = NonPWType,
                    precision = precision,
                    showPriorityAndDirect = showPriorityAndDirectValue,
                    child_node = ChildNodeName,
                    intensities = intensities,
                    non_pw_value = NonPWValue,
                    is_direct = is_direct,
                    //end for non pw one at a time 

                    //multi non pw
                    multi_non_pw_data = MultiNonPW_Data,
                    multi_intensities = multi_intensities,
                    //end of multi non pw

                    //for step func only
                    step_intervals = step_intervals,
                    piecewise = piecewise,
                    //end for step func only

                    judgment_made = judgment_made,
                    overall = overall,
                    total_evaluation = total_evaluation,

                    //info docs guids
                    ParentNodeGUID = ParentNodeGUID,
                    LeftNodeGUID = LeftNodeGUID,
                    RightNodeGUID = RightNodeGUID,
                    LeftNodeWrtGUID = LeftNodeWrtGUID,
                    RightNodeWrtGUID = RightNodeWrtGUID,

                    infodoc_params = infodoc_params,
                    is_auto_advance = is_auto_advance,

                    //save spaces in this method by putting all the datas in this object
                    PipeParameters = PipeParameters,
                    doneOptions = doneOptions,
                    is_infodoc_tooltip = is_infodoc_tooltip,
                    defaultQhInfo = defaultQhInfo,
                    qh_info = qh_info,
                    qh_help_id = qh_help_id,
                    qh_tnode_id = qh_tnode_id,
                    qh_yt_info = qh_yt_info,
                    saType = saType,
                    show_qh_automatically = show_qh_automatically,
                    is_qh_shown = is_qh_shown,
                    dont_show_qh = dont_show_qh,
                    show_qh_setting = show_qh_setting,
                    multi_collapse_default = multi_collapse_default,
                    cluster_phrase = context.Session["ClusterPhrase"],
                    nodes_data = NodesData,

                    UCData = UCData,
                    read_only = AnytimeClass.UserIsReadOnly(),
                    read_only_user = user_email,
                    collapse_bars = App.ActiveProject.ProjectManager.Parameters.EvalCollapseMultiPWBars,
                    //showinfodocnode = App.ActiveProject.PipeParameters.ShowInfoDocs,

                    userControlContent = userControlContent,
                    isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws, App.ActiveWorkgroup),
                    cluster_nodes = cluster_nodes,
                    has_cluster = HasCurStepCluster,
                    name = App.ActiveProject.ProjectName,
                    owner = App.ActiveProject.ProjectManager.User.UserName,
                    passcode = App.ActiveProject.Passcode,
                    hideBrowserWarning = hideWarning,
                    autoFitInfoDocImages = App.ActiveProject.ProjectManager.Parameters.AutoFitInfoDocImages,
                    autoFitInfoDocImagesOptionText = TeamTimeClass.ResString("optImagesZoom"),
                    framed_info_docs = App.ActiveProject.ProjectManager.Parameters.ShowFramedInfodocsMobile,
                    hideInfoDocCaptions = App.ActiveProject.ProjectManager.Parameters.EvalHideInfodocCaptions,
                    fromComparion = (bool) context.Session[Constants.Sess_FromComparion],
                    nextProject = nextProjectId,
                    isPipeViewOnly = (bool) context.Session[Constants.SessionIsPipeViewOnly],
                    isPipeStepFound = (bool) context.Session[Constants.SessionIsInterResultStepFound]
                };
                }
            //}
            //catch{

            //}

            try
            {
                var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return oSerializer.Serialize(output);
            }
            catch (Exception e)
            {
                var error = e;
                //catch if function have recursive arrays
                return JsonConvert.SerializeObject(output, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                        });
            }
        }

        private static string RemoveInfoDcoImageStyle(string infoDocHtml)
        {
            string newInfoDocHtml = infoDocHtml;

            try
            {
                //AR: fix for image stretch issue
                HtmlDocument infoDoc = new HtmlDocument();
                infoDoc.LoadHtml(infoDocHtml);
                List<HtmlNode> imageNodes = infoDoc.DocumentNode.SelectNodes("//img").ToList();   //selecting image elements

                if (imageNodes.Count > 0)
                {
                    foreach (HtmlNode imageNode in imageNodes)
                    {
                        if (imageNode.ParentNode.Name.ToLower() == "font") continue;

                        foreach (HtmlAttribute itemAttribute in imageNode.Attributes.ToList())
                        {
                            if (itemAttribute.Name.ToLower().Equals("style") || itemAttribute.Name.ToLower().Equals("width") || itemAttribute.Name.ToLower().Equals("height"))
                            {
                                imageNode.Attributes[itemAttribute.Name].Remove(); //removing some style attributes
                            }
                        }

                        //searching for parent of image element
                        HtmlNode parentNode = imageNode.Ancestors().FirstOrDefault();
                        if (!parentNode.Name.Equals("p", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //Adding paragraph as prent to image element
                            HtmlDocument doc = new HtmlDocument();
                            HtmlNode newElement = doc.CreateElement("p");
                            newElement.AppendChild(imageNode);
                            parentNode.ReplaceChild(newElement, imageNode);
                            parentNode = newElement;
                        }

                        //removing styles attributes of paragraph node
                        foreach (HtmlAttribute parentAttribute in parentNode.Attributes.ToList())
                        {
                            if (parentAttribute.Name.ToLower().Equals("style") || parentAttribute.Name.ToLower().Equals("class"))
                            {
                                parentNode.Attributes[parentAttribute.Name].Remove(); //removing style/class attributes
                            }
                        }

                        parentNode.Attributes.Add("class", "text-center"); //making paragraph center aligned
                    }

                    newInfoDocHtml = infoDoc.DocumentNode.OuterHtml; //setting modified info doc html
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            return newInfoDocHtml;
        }

        private static string GetDefaultQhInfo(clsComparionCore app, PipeParameters.ecEvaluationStepType qhHelpId)
        {
            var defaultQhInfo = FileService.File_GetContent(GeckoClass.GetIncFile(string.Format(Consts._FILE_TEMPL_QUCIK_HELP, qhHelpId.ToString() + (app.isRiskEnabled ? (app.ActiveProject.isImpact ? "_Impact" : "_Likelihood") : ""))), "");
            if (string.IsNullOrEmpty(defaultQhInfo) && app.isRiskEnabled)
            {
                defaultQhInfo = FileService.File_GetContent(GeckoClass.GetIncFile(string.Format(Consts._FILE_TEMPL_QUCIK_HELP, qhHelpId.ToString())), "");
            }

            defaultQhInfo = TeamTimeClass.ParseAllTemplates(defaultQhInfo, app.ActiveUser, app.ActiveProject);

            return defaultQhInfo;
        }

        [WebMethod(EnableSession = true)]
        public static string SavePairwise(int step, string value, string advantage, string comments, int userId = 0)
        {
            var fChanged = false;
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return "";
            }

            var app = (clsComparionCore)context.Session["App"];

            if (app != null)
            {
                try
                {
                    var action = AnytimeClass.GetAction(step);
                    var pwData = (clsPairwiseMeasureData)action.ActionData;
                    var parentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNodeID);
                    var pwType = app.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(parentNode);

                    if (context.Request.Cookies[Constants.Cook_Extreme] == null && pwType == CanvasTypes.PairwiseType.ptVerbal)
                    {
                        if (Convert.ToInt64(value) == 9)
                        {
                            context.Response.Cookies[Constants.Cook_Extreme].Expires = DateTime.Now.AddDays(1);
                            context.Response.Cookies[Constants.Cook_Extreme].Value = "1";

                            PipeWarning = TeamTimeClass.ResString("msgPWExtreme");
                        }
                    }

                    if (userId != 0)
                    {
                        var actionPwData = (clsPairwiseMeasureData)action.ActionData;
                        var actionParentNode = action.ParentNode;
                        pwData = ((clsPairwiseJudgments)actionParentNode.Judgments).PairwiseJudgment(actionPwData.FirstNodeID, actionPwData.SecondNodeID, userId);

                        if (pwData == null)
                        {
                            pwData = new clsPairwiseMeasureData(actionPwData.FirstNodeID, actionPwData.SecondNodeID, 0, 0, actionParentNode.NodeID, userId, true);
                        }
                    }

                    if (value != "")
                    {
                        double val = 0;
                        int adv = 0;

                        if (StringFuncs.String2Double(value, ref val) && int.TryParse(advantage, out adv))
                        {
                            if (val == -2147483648000) //  equal to ctrlPairwiseBar.pwUndefinedValue
                            {
                                if (!pwData.IsUndefined)
                                {
                                    pwData.IsUndefined = true;
                                    fChanged = true;
                                }
                            }
                            else
                            {
                                if (val == 0 || adv == 0)
                                {
                                    val = 1;
                                    adv = 0;
                                }

                                pwData.IsUndefined = false;

                                if (pwData.Value != val || pwData.Advantage != adv)
                                {
                                    pwData.Value = val;
                                    pwData.Advantage = adv;
                                    fChanged = true;
                                }
                            }
                        }
                    }

                    if (app.ActiveProject.PipeParameters.ShowComments && comments != null)
                    {
                        if (comments != pwData.Comment)
                        {
                            pwData.Comment = comments;
                            fChanged = true;
                        }
                    }

                    if (fChanged)
                    {
                        //skipped riskon
                        //If IsRiskWithControls Then
                        //    App.ActiveProject.ProjectManager.PipeBuilder.SaveControlJudgment(Action)
                        //Else
                        //    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.ParentNodeID), PWData) ' D0039 'C0028
                        //End If

                        //save

                        if (action.ActionType == ActionType.atPairwiseOutcomes)
                        {
                            action.ParentNode.PWOutcomesJudgments.AddMeasureData(pwData);
                            app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(action.PWONode, pwData);
                        }
                        else
                        {
                            var tNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNodeID);
                            tNode.Judgments.AddMeasureData(pwData);
                            app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(tNode, pwData);
                        }

                        var isAlreadySaved = (bool)context.Session[SessionIsJudgmentAlreadySaved];

                        if (pwType == CanvasTypes.PairwiseType.ptVerbal && !isAlreadySaved)
                        {
                            context.Session[SessionIncreaseJudgmentsCount] = true;
                            context.Session[SessionIsJudgmentAlreadySaved] = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    var error = e;
                }
            }

            return PipeWarning;
        }

        [WebMethod(EnableSession = true)]
        public static List<object> GetOverallResultsData(int rmode, int wrtnodeID, bool isReload = false)
        {
            // D1300
            var returnObject = new List<object>();
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            int iStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
            var _with1 = App.ActiveProject.ProjectManager;
            clsAction ps = (clsAction)_with1.Pipe[iStep - 1];
            ExpectedValueString = new string[3];
            clsShowGlobalResultsActionData psLocal = (clsShowGlobalResultsActionData)AnytimeClass.GetAction(iStep).ActionData;
            psLocal.WRTNode = App.ActiveProject.ProjectManager.Hierarchies[0].GetNodeByID(wrtnodeID);
            if (isReload || context.Session[Sess_WrtNode] == null) 
                context.Session[Sess_WrtNode] = psLocal.WRTNode;



            string sEmail = App.ActiveUser.UserEMail;
           
            if (AnytimeClass.UserIsReadOnly())
            {
                //set ReadOnlyUser here
                var CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID());
                sEmail = CurrentUser.UserEMail;
            }

            ECTypes.clsUser AHPUser = _with1.GetUserByEMail(sEmail);
            int AHPUserID = AHPUser.UserID;

            var reslist = new List<List<object>>();
            bool bCanShowIndividual = false;
            bool bCanShowGroup = false;

            if (_with1.PipeBuilder.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvIndividual | _with1.PipeBuilder.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvBoth)
            {
                bCanShowIndividual = psLocal.get_CanShowIndividualResults(AHPUserID, psLocal.WRTNode);
            }
            if (_with1.PipeBuilder.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvGroup | _with1.PipeBuilder.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvBoth)
            {
                bCanShowGroup = true;
            }

            var canshowresult = new
            {
                individual = (bCanShowIndividual && (_with1.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvIndividual || _with1.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvBoth)),
                combined = (bCanShowGroup && (_with1.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvGroup || _with1.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvBoth))
            };
            var messagenote = new string[2];
            messagenote[0] = TeamTimeClass.ResString("msgNoOverallResults");

            if (!canshowresult.individual)
            {
                messagenote[1] = TeamTimeClass.ResString("msgNoEvalDataIndividualResults");
            }
            else if (!canshowresult.combined)
            {
                messagenote[1] = TeamTimeClass.ResString("msgNoEvalDataGroupResults");
            }

            ArrayList mResultsList = null;
            //C0810
            ArrayList mIndividualResultsList = null;
            //C0811
            ArrayList mGroupResultsList = null;
            //C0811

            if (_with1.PipeBuilder.PipeParameters.GlobalResultsView != CanvasTypes.ResultsView.rvNone)
            {
                //If bCanShowIndividual And _
                //    (.PipeBuilder.PipeParameters.LocalResultsView = ResultsView.rvIndividual Or .PipeBuilder.PipeParameters.LocalResultsView = ResultsView.rvBoth) Then
                var resultmode = AlternativeNormalizationOptions.anoPriority;
                if (rmode == 3)
                {
                    resultmode = AlternativeNormalizationOptions.anoUnnormalized;
                }
                if (rmode == 2)
                {
                    resultmode = AlternativeNormalizationOptions.anoPercentOfMax;
                }

                mIndividualResultsList = (ArrayList)psLocal.get_ResultsList(AHPUser.UserID, AHPUser.UserID, resultmode).Clone();
                mResultsList = mIndividualResultsList;

                //End If
                if (bCanShowGroup & (_with1.PipeBuilder.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvGroup | _with1.PipeBuilder.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvBoth))
                {
                    var resultmodes = AlternativeNormalizationOptions.anoPriority;
                    if (rmode == 3)
                    {
                        resultmodes = AlternativeNormalizationOptions.anoUnnormalized;
                    }
                    if (rmode == 2)
                    {
                        resultmodes = AlternativeNormalizationOptions.anoPercentOfMax;
                    }

                    if (AnytimeClass.CombinedUserID == ECTypes.COMBINED_USER_ID)
                    {
                        //Uncommented above 2 and below 5 lines
                        mGroupResultsList = (ArrayList)psLocal.get_ResultsList(_with1.CombinedGroups.GetDefaultCombinedGroup().CombinedUserID, AHPUser.UserID, resultmodes).Clone();
                    }
                    else
                    {
                        mGroupResultsList = (ArrayList)psLocal.get_ResultsList(AnytimeClass.CombinedUserID, AHPUser.UserID, resultmodes).Clone();
                    }

                    mResultsList = mGroupResultsList;
                }
            }

            if (_with1.PipeBuilder.PipeParameters.GlobalResultsView != CanvasTypes.ResultsView.rvNone)
            {
                if (mResultsList != null)
                {
                    bool IsSumMore1_Individual = false;
                    bool IsSumMore1_Group = false;

                    double Sum_Individual = 0;
                    double Sum_Group = 0;

                    for (int i = 0; i <= mResultsList.Count - 1; i++)
                    {
                        if (mIndividualResultsList != null && mIndividualResultsList.Count > i)
                            Sum_Individual += ((clsResultsItem)mIndividualResultsList[i]).UnnormalizedValue;
                        if (mGroupResultsList != null && mGroupResultsList.Count > i)
                            Sum_Group += ((clsResultsItem)mGroupResultsList[i]).UnnormalizedValue;
                    }

                    if (Sum_Individual > 1 + 1E-06)
                        IsSumMore1_Individual = true;
                    if (Sum_Group > 1 + 1E-06)
                        IsSumMore1_Group = true;
                    // ==

                    var objlist = new List<Objective>();
                    for (int i = 0; i <= mResultsList.Count - 1; i++)
                    {
                        var Res = new ExpertChoice.Results.Objective();
                        var reuse = new List<object>();
                        clsResultsItem listItem = (clsResultsItem)mResultsList[i];
                        reuse.Add((i + 1));
                        reuse.Add(listItem.Name);
                        Res.Name = listItem.Name;

                        if (psLocal.get_CanShowIndividualResults(AHPUserID, psLocal.WRTNode))
                        {
                            clsResultsItem R = (clsResultsItem)mIndividualResultsList[i];
                            reuse.Add(Convert.ToSingle((rmode == 1 || rmode == 2 ? R.Value : R.UnnormalizedValue)).ToString());
                            Res.Value = rmode == 1 ? R.Value : R.UnnormalizedValue;
                            // D2026 'A0854 'A1029
                            Res.GlobalValue = Res.Value * psLocal.WRTNode.WRTGlobalPriority;
                        }
                        else
                        {
                            reuse.Add("0");

                        }

                        if (bCanShowGroup & (_with1.PipeBuilder.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvGroup | _with1.PipeBuilder.PipeParameters.GlobalResultsView == CanvasTypes.ResultsView.rvBoth))
                        {
                            clsResultsItem R = (clsResultsItem)mGroupResultsList[i];
                            reuse.Add(Convert.ToSingle((rmode == 1 || rmode == 2 ? R.Value : R.UnnormalizedValue)).ToString());
                            Res.CombinedValue = rmode == 1 ? R.Value : R.UnnormalizedValue;
                            Res.GlobalValueCombined = Res.CombinedValue * psLocal.WRTNode.WRTGlobalPriority;
                        }
                        else
                        {
                            reuse.Add("0");

                        }
                        reuse.Add(App.ActiveProject.PipeParameters.GlobalResultsView.ToString());
                        reslist.Add(reuse);

                        if (psLocal.WRTNode.get_MeasureType() == ECMeasureType.mtPWAnalogous)
                        {
                            List<ECTypes.KnownLikelihoodDataContract> KnownLikelihoods = psLocal.WRTNode.GetKnownLikelihoods();
                            if (KnownLikelihoods != null)
                            {
                                int k = 0;
                                while (k <= KnownLikelihoods.Count - 1)
                                {
                                    dynamic lkhd = KnownLikelihoods[k];
                                    if (listItem.ObjectID == lkhd.ID && lkhd.Value > 0)
                                    {
                                        Res.AltWithKnownLikelihoodName = lkhd.NodeName;
                                        Res.AltWithKnownLikelihoodID = lkhd.ID;
                                        Res.AltWithKnownLikelihoodGuidID = lkhd.GuidID;
                                        Res.AltWithKnownLikelihoodValue = lkhd.Value;
                                        if (Res.AltWithKnownLikelihoodValue > 0)
                                        {
                                            Res.AltWithKnownLikelihoodValueString = Res.AltWithKnownLikelihoodValue.ToString();
                                        }
                                        k = KnownLikelihoods.Count;
                                    }
                                    k += 1;
                                }
                            }
                        }

                        Res.ID = listItem.ObjectID;
                        //Model.ObjectivesData.Add(Res);
                        objlist.Add(Res);
                        //Res.Index = Model.ObjectivesData.Count;
                    }

                    double IndivExpectedValue = 0;
                    double CombinedExpectedValue = 0;
                    bool CanShowHiddenExpectedValue = false;

                    foreach (Objective item in objlist)
                    {
                        var s = item.Name.Split(' ');
                        if (s.Length > 0)
                        {
                            //if ("1234567890,. ".Contains(item.Name[0]))
                            //{
                            //    string s = "";
                            //    int i = 0;
                            //    while (i < item.Name.Length && "1234567890,. ".Contains(item.Name[i]))
                            //    {
                            //        s += item.Name[i];
                            //        i += 1;
                            //    }
                            double d = 0;
                            if (Double.TryParse(s[0], out d))
                            {
                                CanShowHiddenExpectedValue = true;
                                IndivExpectedValue += d * item.Value;
                                CombinedExpectedValue += d * item.CombinedValue;
                            }
                        }
                    }

                    if (CanShowHiddenExpectedValue)
                    {
                        if (bCanShowIndividual)
                        {
                            ExpectedValueString[0] = $" {TeamTimeClass.ResString("lblExpectedValueIndiv")} {Math.Round(IndivExpectedValue, 4)}";
                        }
                        if (bCanShowGroup)
                        {
                            ExpectedValueString[1] = $" {TeamTimeClass.ResString("lblExpectedValueComb")} {Math.Round(CombinedExpectedValue, 2)}";
                        }
                    }

                    ExpectedValueString[2] = CanShowHiddenExpectedValue ? "1" : "0";
                    // Normalize
                    //A1029 ===
                }
            }

            //A0321 ==

            returnObject.Add(reslist);
            returnObject.Add(canshowresult);
            returnObject.Add(psLocal.WRTNode.NodeName);
            returnObject.Add(messagenote);

            var ActiveHierarchies = App.ActiveProject.ProjectManager.GetAllHierarchies();
            psLocal.WRTNode = App.ActiveProject.ProjectManager.Hierarchies[0].GetNodeByID(ActiveHierarchies[0].Nodes[0].NodeID);


            return returnObject;
        }


        [WebMethod(EnableSession = true)]
        public static List<List<string>> GetIntermediateResultsData(int rmode)
        {
            // D1300
            var Model = new DataModel();
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            int iStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
            ExpectedValueString = new string[3];
            var _with1 = App.ActiveProject.ProjectManager;
            clsAction ps = (clsAction)_with1.Pipe[iStep - 1];
            clsShowLocalResultsActionData psLocal = (clsShowLocalResultsActionData)ps.ActionData;
            string sEmail = App.ActiveUser.UserEMail;

            if (AnytimeClass.UserIsReadOnly())
            {
                //set ReadOnlyUser here
                var CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID());
                sEmail = CurrentUser.UserEMail;
            }

            ECTypes.clsUser AHPUser = _with1.GetUserByEMail(sEmail);
            int AHPUserID = AHPUser.UserID;
            //var Res = new ExpertChoice.Results.Objective();
            var reslist = new List<List<string>>();
            Model.IsForAlternatives = psLocal.ParentNode != null && psLocal.ParentNode.IsTerminalNode;
            Model.PWMode = psLocal.ParentNode != null ? _with1.PipeBuilder.GetPairwiseTypeForNode(psLocal.ParentNode) : Model.PWMode;

            switch (psLocal.ResultsViewMode) {
				case CanvasTypes.ResultsView.rvNone:
					Model.ShowIndividualResults = false;
					Model.ShowGroupResults = false;
					break;
				case CanvasTypes.ResultsView.rvIndividual:
					Model.ShowIndividualResults = true;
					Model.ShowGroupResults = false;
					break;
				case CanvasTypes.ResultsView.rvGroup:
					Model.ShowIndividualResults = false;
					Model.ShowGroupResults = true;
					break;
				case CanvasTypes.ResultsView.rvBoth:
					Model.ShowIndividualResults = true;
					Model.ShowGroupResults = true;
					break;
			}

            bool bCanShowIndividual = false;
            bool bCanShowGroup = false;
            var resultmodes = AlternativeNormalizationOptions.anoPriority;
            if (rmode == 3)
            {
                resultmodes = AlternativeNormalizationOptions.anoUnnormalized;
            }
            if (rmode == 2)
            {
                resultmodes = AlternativeNormalizationOptions.anoPercentOfMax;
            }

            if (psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvIndividual | psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvBoth)
            {
                bCanShowIndividual = psLocal.CanShowIndividualResults;
                
                Model.CanShowIndividualResults = bCanShowIndividual;
            }
            else 
            {
				Model.CanShowIndividualResults = false;
			}

			Model.CanEditModel = App.CanUserModifyProject(App.ActiveUser.UserID, App.ActiveProject.ID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup);
			Model.ShowKnownLikelihoods = psLocal.ParentNode.get_MeasureType() == ECMeasureType.mtPWAnalogous;

            if (psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvGroup | psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvBoth)
            {
                 bCanShowGroup = true;
                 Model.CanShowGroupResults = bCanShowGroup;
			} else 
            {
				Model.CanShowGroupResults = false;
			}

            if (psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvBoth && psLocal.ParentNode.Hierarchy.ProjectManager.UsersList.Count <= 1)
            {
                bCanShowGroup = false;
                Model.CanShowGroupResults = false;
                Model.ShowGroupResults = false;
            }

            Model.InsufficientInfo = !((Model.ShowIndividualResults && Model.CanShowIndividualResults) || (Model.ShowGroupResults && Model.CanShowGroupResults));
			//A0322 ===
			if (Model.ShowGroupResults && Model.CanShowGroupResults && Model.ShowIndividualResults && (!Model.CanShowIndividualResults)) {
				Model.CanNotShowLocalResults = true;
				Model.InsufficientInfo = false;
				Model.ShowIndividualResults = false;
				//For hiding the column
			}

            ArrayList mResultsList = null;
            ArrayList mIndividualResultsList = null;
            ArrayList mGroupResultsList = null;

            if (psLocal.ResultsViewMode != CanvasTypes.ResultsView.rvNone)
            {
                if (bCanShowGroup & (psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvGroup || psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvBoth))
                {
                    if (AnytimeClass.CombinedUserID == ECTypes.COMBINED_USER_ID)
                    {
                        mGroupResultsList = (ArrayList)psLocal.get_ResultsList(_with1.CombinedGroups.GetDefaultCombinedGroup().CombinedUserID, AHPUser.UserID).Clone();
                    }
                    else
                    {
                        mGroupResultsList = (ArrayList)psLocal.get_ResultsList(AnytimeClass.CombinedUserID, AHPUser.UserID).Clone();
                    }
                }

                //If bCanShowIndividual And _
                //    (.PipeBuilder.PipeParameters.LocalResultsView = ResultsView.rvIndividual Or .PipeBuilder.PipeParameters.LocalResultsView = ResultsView.rvBoth) Then
                mIndividualResultsList = (ArrayList)psLocal.get_ResultsList(AHPUser.UserID, AHPUser.UserID).Clone();
                mResultsList = mIndividualResultsList;
                //End If

                if (mResultsList == null)
                {
                    mResultsList = mGroupResultsList;
                }
            }

            if ((bCanShowIndividual || bCanShowGroup) && psLocal.ShowExpectedValue)
            {
                if (bCanShowIndividual && !ECTypes.IsCombinedUserID(AHPUserID))
                {
                    Model.ExpectedValueIndiv = psLocal.get_ExpectedValue(AHPUserID);
		            Model.ExpectedValueIndivVisible = true;
                }
                if (bCanShowGroup)
                {
                    Model.ExpectedValueComb = psLocal.get_ExpectedValue(AnytimeClass.CombinedUserID);
                    Model.ExpectedValueCombVisible = true;
                }
            }
            var ClusterAction = AnytimeClass.GetAction(iStep - 1);

            List<StepsPairs> list = GeckoClass.GetEvalPipeStepsList(psLocal.ParentNode.NodeID, iStep, psLocal.PWOutcomesNode);
            //Cycle through 1 to N "inconsitency" to find each associted pair
            int numChildren = 0;
            clsPairwiseJudgments pwJudgments = default(clsPairwiseJudgments);

            ECTypes.clsCalculationTarget calcTarget = new ECTypes.clsCalculationTarget(ECTypes.CalculationTargetType.cttUser, psLocal.ParentNode.Hierarchy.ProjectManager.GetUserByID(AHPUserID));

            Model.StepPairs = new List<StepsPairs>();
            switch(ClusterAction.ActionType)
            {
                case ActionType.atAllPairwise:
                case ActionType.atPairwise:
                case ActionType.atPairwiseOutcomes:
                case ActionType.atAllPairwiseOutcomes:
                    if (psLocal.PWOutcomesNode == null)
                    {
                        List<clsNode> nodesBelow = psLocal.ParentNode.GetNodesBelow(AHPUserID);
                        for (int i = nodesBelow.Count - 1; i >= 0; i += -1)
                        {
                            if (nodesBelow[i].RiskNodeType == ECTypes.RiskNodeType.ntCategory)
                                nodesBelow.RemoveAt(i);
                        }
                        numChildren = nodesBelow.Count;
                        pwJudgments = (clsPairwiseJudgments)psLocal.ParentNode.Judgments;
                    }
                    else
                    {
                        numChildren = ((clsRatingScale)psLocal.ParentNode.MeasurementScale).RatingSet.Count;
                        pwJudgments = psLocal.PWOutcomesNode.PWOutcomesJudgments;
                    }

                    for (int i = 1; i <= (numChildren * (numChildren - 1)) / 2; i++)
                    {
                        //Dim pwData As clsPairwiseMeasureData = CType(psLocal.ParentNode.Judgments, clsPairwiseJudgments).GetNthMostInconsistentJudgment(calcTarget, i)
                        clsPairwiseMeasureData pwData = default(clsPairwiseMeasureData);
                        if (psLocal.PWOutcomesNode == null)
                        {
                            pwData = pwJudgments.GetNthMostInconsistentJudgment(calcTarget, i);
                        }
                        else
                        {
                            pwData = pwJudgments.GetNthMostInconsistentJudgmentOutcomes(calcTarget, i, (clsRatingScale)psLocal.ParentNode.MeasurementScale);
                        }
                        if (pwData != null)
                        {
                            foreach (StepsPairs Pair in list)
                            {
                                if ((Pair.Obj1 == pwData.FirstNodeID && Pair.Obj2 == pwData.SecondNodeID) || (Pair.Obj2 == pwData.FirstNodeID && Pair.Obj1 == pwData.SecondNodeID))
                                {
                                    Pair.Rank = i;
                                }
                            }
                        }
                    }

                    foreach (StepsPairs Pair in list)
                    {
                        //Dim pwData As clsPairwiseMeasureData = CType(psLocal.ParentNode.Judgments, clsPairwiseJudgments).GetBestFitJudgment(calcTarget, Pair.Obj1, Pair.Obj2)
                        clsPairwiseMeasureData pwData = default(clsPairwiseMeasureData);
                        if (psLocal.PWOutcomesNode == null)
                        {
                            pwData = pwJudgments.GetBestFitJudgment(calcTarget, Pair.Obj1, Pair.Obj2);
                        }
                        else
                        {
                            pwData = pwJudgments.GetBestFitJudgmentOutcomes(calcTarget, Pair.Obj1, Pair.Obj2, (clsRatingScale)psLocal.ParentNode.MeasurementScale);
                        }

                        //A0605
                        if (pwData == null)
                        {
                            Pair.BestFitValue = 0;
                            Pair.BestFitAdvantage = 0;
                        }
                        else
                        {
                            Pair.BestFitValue = pwData.Value;
                            Pair.BestFitAdvantage = pwData.Advantage;
                        }
                    }

                    foreach (StepsPairs pair in list)
                    {
                        Model.StepPairs.Add(pair);
                    }

                    break;
            }

            //end ToDo






            Model.ParentNodeName = psLocal.ParentNode.NodeName;
            Model.ParentID = psLocal.ParentNode.NodeID;
            Model.IsParentNodeGoal = psLocal.ParentNode.get_ParentNode() == null;

            string parentNodeKnownLikelihood = "";
            if (psLocal.ParentNode.get_ParentNode() != null && psLocal.ParentNode.get_ParentNode().get_MeasureType() == ECMeasureType.mtPWAnalogous) {
	            List<ECTypes.KnownLikelihoodDataContract> nl = psLocal.ParentNode.get_ParentNode().GetKnownLikelihoods();
	            foreach (ECTypes.KnownLikelihoodDataContract item in nl) {
		            if (item.GuidID.Equals(psLocal.ParentNode.NodeGuidID) && item.Value > 0)
			            parentNodeKnownLikelihood = item.Value.ToString();
	            }
            }

            Model.ParentNodeKnownLikelihood = parentNodeKnownLikelihood;

            var PM = App.ActiveProject.ProjectManager;
            if (psLocal.ResultsViewMode != CanvasTypes.ResultsView.rvNone) {
	            if (bCanShowIndividual & (psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvIndividual | psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvBoth)) {
                    //get local priority of the parent node for active user
                    PM.CalculationsManager.Calculate(calcTarget, PM.get_Hierarchy(PM.ActiveHierarchy).Nodes[0], PM.ActiveHierarchy, PM.ActiveAltsHierarchy);
		            Model.ParentNodeGlobalPriority = psLocal.ParentNode.WRTGlobalPriority;
	            }

	            if (bCanShowGroup & (psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvGroup | psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvBoth)) {
                    //get local priority of the parent node for COMBINED
                    if (AnytimeClass.CombinedUserID == ECTypes.COMBINED_USER_ID)
                    {
                        ECCore.Groups.clsCombinedGroup CG = PM.CombinedGroups.GetDefaultCombinedGroup();
                        ECTypes.clsCalculationTarget calcTargetCombined = new ECTypes.clsCalculationTarget(ECTypes.CalculationTargetType.cttCombinedGroup, CG);
                        PM.CalculationsManager.Calculate(calcTargetCombined, PM.get_Hierarchy(PM.ActiveHierarchy).Nodes[0], PM.ActiveHierarchy, PM.ActiveAltsHierarchy);
                    }
                    else
                    {
                        ECTypes.clsCalculationTarget calcTargetCombined = new ECTypes.clsCalculationTarget(ECTypes.CalculationTargetType.cttUser, psLocal.ParentNode.Hierarchy.ProjectManager.GetUserByID(AnytimeClass.CombinedUserID));
                        PM.CalculationsManager.Calculate(calcTargetCombined, PM.get_Hierarchy(PM.ActiveHierarchy).Nodes[0], PM.ActiveHierarchy, PM.ActiveAltsHierarchy);
                    }

                    Model.ParentNodeGlobalPriorityCombined = psLocal.ParentNode.WRTGlobalPriority;
	            }
            }

            if (psLocal.ResultsViewMode != CanvasTypes.ResultsView.rvNone)
                    {
                        if (mResultsList != null)
                        {
                            bool IsSumMore1_Individual = false;
                            bool IsSumMore1_Group = false;

                            double Sum_Individual = 0;
                            double Sum_Group = 0;

                            for (int i = 0; i <= mResultsList.Count - 1; i++)
                            {
                                if (mIndividualResultsList != null && mIndividualResultsList.Count > i)
                                    Sum_Individual += ((clsResultsItem)mIndividualResultsList[i]).UnnormalizedValue;
                                if (mGroupResultsList != null && mGroupResultsList.Count > i)
                                    Sum_Group += ((clsResultsItem)mGroupResultsList[i]).UnnormalizedValue;
                            }

                            if (Sum_Individual > 1 + 1E-06)
                                IsSumMore1_Individual = true;
                            if (Sum_Group > 1 + 1E-06)
                                IsSumMore1_Group = true;
                            // ==

		                    Model.IsPWNLandNormalizedParticipantResults = psLocal.ParentNode.get_MeasureType() == ECMeasureType.mtPWAnalogous && IsSumMore1_Individual;
		                    Model.IsPWNLandNormalizedGroupResults = psLocal.ParentNode.get_MeasureType() == ECMeasureType.mtPWAnalogous && IsSumMore1_Group;
		                    // ==                                                
		                    Model.ObjectivesData.Clear();
                            for (int i = 0; i <= mResultsList.Count - 1; i++)
                            {
                                var Res = new ExpertChoice.Results.Objective();
                                var reuse = new List<string>();
                                clsResultsItem listItem = (clsResultsItem)mResultsList[i];
                                reuse.Add((i+1).ToString());
                                Res.Name = listItem.Name;
                                reuse.Add(listItem.Name);

                                if (true)
                                {
                                    clsResultsItem R = (clsResultsItem)mIndividualResultsList[i];
                                    reuse.Add(Convert.ToSingle((rmode == 1 ? R.Value : R.UnnormalizedValue)).ToString());
                                    //Res.Value = Convert.ToSingle((psLocal.ParentNode.get_MeasureType() == ECMeasureType.mtPWAnalogous ? (IsSumMore1_Individual ? R.Value : R.UnnormalizedValue) : R.UnnormalizedValue));
                                    Res.Value = rmode == 1 ? R.Value : R.UnnormalizedValue;
                                    // D2026 'A0854 'A1029
                                    Res.GlobalValue = Res.Value * Model.ParentNodeGlobalPriority;
                                }

                                if (psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvGroup | psLocal.ResultsViewMode == CanvasTypes.ResultsView.rvBoth)
                                {
                                    clsResultsItem R = (clsResultsItem)mGroupResultsList[i];
                                    reuse.Add(Convert.ToSingle(rmode == 1 ? R.Value : R.UnnormalizedValue).ToString());
                            // D2026 'A0854 'A1029
                                    //Res.CombinedValue = Convert.ToSingle((psLocal.ParentNode.get_MeasureType() == ECMeasureType.mtPWAnalogous ? (IsSumMore1_Individual ? R.Value : R.UnnormalizedValue) : R.UnnormalizedValue));
                                    Res.CombinedValue = rmode == 1 ? R.Value : R.UnnormalizedValue;
                                    Res.GlobalValueCombined = Res.CombinedValue * Model.ParentNodeGlobalPriorityCombined;

                                }
                                else
                                {
                                    reuse.Add("");
                                }
                                reuse.Add(psLocal.ResultsViewMode.ToString());
                                reuse.Add(listItem.ObjectID.ToString());
                                reslist.Add(reuse);

                                if (psLocal.ParentNode.get_MeasureType() == ECMeasureType.mtPWAnalogous)
                                {
                                    List<ECTypes.KnownLikelihoodDataContract> KnownLikelihoods = psLocal.ParentNode.GetKnownLikelihoods();
                                    if (KnownLikelihoods != null)
                                    {
                                        int k = 0;
                                        while (k <= KnownLikelihoods.Count - 1)
                                        {
                                            dynamic lkhd = KnownLikelihoods[k];
                                            if (listItem.ObjectID == lkhd.ID && lkhd.Value > 0)
                                            {
                                                Res.AltWithKnownLikelihoodName = lkhd.NodeName;
                                                Res.AltWithKnownLikelihoodID = lkhd.ID;
                                                Res.AltWithKnownLikelihoodGuidID = lkhd.GuidID;
                                                Res.AltWithKnownLikelihoodValue = lkhd.Value;
                                                if (Res.AltWithKnownLikelihoodValue > 0)
                                                {
                                                    Res.AltWithKnownLikelihoodValueString = Res.AltWithKnownLikelihoodValue.ToString();
                                                }
                                                k = KnownLikelihoods.Count;
                                            }
                                            k += 1;
                                        }
                                    }
                                }

                                Res.ID = listItem.ObjectID;
                                Model.ObjectivesData.Add(Res);
                                Res.Index = Model.ObjectivesData.Count;
                            }
                    //A1070 ===
                    double IndivExpectedValue = 0;
                    double CombinedExpectedValue = 0;
                    bool CanShowHiddenExpectedValue = false;
                    //A1070 ==


                    switch (resultmodes)
                    {
                        case AlternativeNormalizationOptions.anoPriority:
                            double fSumValue = Model.ObjectivesData.Sum(d => d.Value);
                            double fSumCombinedValue = Model.ObjectivesData.Sum(d => d.CombinedValue);
                            double fSumGlobalValue = Model.ObjectivesData.Sum(d => d.GlobalValue);
                            double fSumGlobalCombinedValue = Model.ObjectivesData.Sum(d => d.GlobalValueCombined);
                            if (fSumValue == 0)
                                fSumValue = 1;
                            if (fSumCombinedValue == 0)
                                fSumCombinedValue = 1;
                            if (fSumGlobalValue == 0)
                                fSumGlobalValue = 1;
                            if (fSumGlobalCombinedValue == 0)
                                fSumGlobalCombinedValue = 1;
                            for (int i = 0; i < Model.ObjectivesData.Count; i++)
                            {
                                var item_loopVariable = Model.ObjectivesData[i];
                                var item = item_loopVariable;
                                //item.Value = item.Value / fSumValue;
                                //item.CombinedValue = item.CombinedValue / fSumCombinedValue;
                                //item.GlobalValue = item.GlobalValue / fSumGlobalValue;
                                //item.GlobalValueCombined = item.GlobalValueCombined / fSumGlobalCombinedValue;
                                reslist[i][2] = (item.Value / fSumValue).ToString();
                                reslist[i][3] = (item.CombinedValue / fSumCombinedValue).ToString();
                            }
                            break;
                        case AlternativeNormalizationOptions.anoPercentOfMax:
                            double fMaxValue = Model.ObjectivesData.Max(d => d.Value);
                            double fMaxCombinedValue = Model.ObjectivesData.Max(d => d.CombinedValue);
                            double fMaxGlobalValue = Model.ObjectivesData.Max(d => d.GlobalValue);
                            double fMaxGlobalCombinedValue = Model.ObjectivesData.Max(d => d.GlobalValueCombined);
                            if (fMaxValue == 0)
                                fMaxValue = 1;
                            if (fMaxCombinedValue == 0)
                                fMaxCombinedValue = 1;
                            if (fMaxGlobalValue == 0)
                                fMaxGlobalValue = 1;
                            if (fMaxGlobalCombinedValue == 0)
                                fMaxGlobalCombinedValue = 1;
                            for (int i = 0; i < Model.ObjectivesData.Count; i++)
                            {
                                var item_loopVariable = Model.ObjectivesData[i];
                                var item = new Objective();
                                item = item_loopVariable;
                                //item.Value = item.Value / fMaxValue;
                                //item.CombinedValue = item.CombinedValue / fMaxCombinedValue;
                                //item.GlobalValue = item.GlobalValue / fMaxGlobalValue;
                                //item.GlobalValueCombined = item.GlobalValueCombined / fMaxGlobalCombinedValue;
                                reslist[i][2] = (item.Value / fMaxValue).ToString();
                                reslist[i][3] = (item.CombinedValue / fMaxCombinedValue).ToString();
                            }
                            break;
                    }

                    foreach (Objective item in Model.ObjectivesData)
                    {
                        var s = item.Name.Split(' ');
                        if (s.Length > 0)
                        {
                            //if ("1234567890,. ".Contains(item.Name[0]))
                            //{
                            //    string s = "";
                            //    int i = 0;
                            //    while (i < item.Name.Length && "1234567890,. ".Contains(item.Name[i]))
                            //    {
                            //        s += item.Name[i];
                            //        i += 1;
                            //    }
                            double d = 0;
                            if (Double.TryParse(s[0], out d))
                            {
                                CanShowHiddenExpectedValue = true;
                                IndivExpectedValue += d * item.Value;
                                CombinedExpectedValue += d * item.CombinedValue;
                            }
                        }
                    }

                    if (CanShowHiddenExpectedValue)
                    {
                        if (bCanShowIndividual)
                        {
                            ExpectedValueString[0] = " Your Expected value = " + (Math.Round(IndivExpectedValue, 2)).ToString();
                        }
                        if (bCanShowGroup)
                        {
                            ExpectedValueString[1] = " Combined Expected value = " + (Math.Round(CombinedExpectedValue, 2)).ToString();
                        }
                    }
                    ExpectedValueString[2] = CanShowHiddenExpectedValue ? "1" : "0";
                    Model.ObjectivesDataSorted = new List<Objective>();
                    Model.ObjectivesDataSorted.AddRange(Model.ObjectivesData);
                    if (HttpContext.Current.Session[InconsistencySortingEnabled] == null)
                    {
                        HttpContext.Current.Session[InconsistencySortingEnabled] = false;
                        HttpContext.Current.Session[BestFit] = false;
                    }

                    if (context.Session[InconsistencySortingOrder] == null)
                    {
                        context.Session[InconsistencySortingOrder] = new List<int>();
                    }

                    if ((bool) HttpContext.Current.Session[InconsistencySortingEnabled])
                    {
                        Model.ObjectivesDataSorted.Sort((obj1, obj2) => obj2.Value.CompareTo(obj1.Value));

                        var sortOrder = 0;
                        var sortList = new List<int>();
                        foreach (var objective in Model.ObjectivesDataSorted)
                        {
                            objective.SortOrder = sortOrder++;
                            sortList.Add(objective.ID);
                        }

                        context.Session[InconsistencySortingOrder] = sortList;
                    }
                    else
                    {
                        var sortList = (List<int>) context.Session[InconsistencySortingOrder];
                        if (sortList.Count > 0)
                        {
                            var objectivesWithOldSort = new List<Objective>();
                            foreach (var objectiveId in sortList)
                            {
                                var objective = Model.ObjectivesDataSorted.First(o => o.ID == objectiveId);
                                if (objective != null)
                                {
                                    objectivesWithOldSort.Add(objective);
                                }
                            }

                            Model.ObjectivesDataSorted = new List<Objective>();
                            Model.ObjectivesDataSorted.AddRange(objectivesWithOldSort);
                        }
                    }
                }
            }

            context.Session[Constants.SessionModel] = Model;
            return reslist;
        }

        [WebMethod(EnableSession = true)]
        public static void SaveMultiPairwiseData(int step, object[][] multivalues)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var App = (clsComparionCore)context.Session["App"];
            var Action = (clsAction)AnytimeClass.GetAction(step);
            clsAllPairwiseEvaluationActionData data = (clsAllPairwiseEvaluationActionData)Action.ActionData;
            double Value = 0;

            for (int ID = 0; ID <= data.Judgments.Count - 1; ID++)
            {
                string sIDLeft = data.Judgments[ID].FirstNodeID.ToString();
                string sIDRight = data.Judgments[ID].SecondNodeID.ToString();

                foreach (clsPairwiseMeasureData tJud in data.Judgments)
                {
                    // D3041
                    if (tJud.FirstNodeID.ToString() == sIDLeft && tJud.SecondNodeID.ToString() == sIDRight)
                    {
                        bool fUpdate = false;

                        string sValue = multivalues[ID][0].ToString();
                        string sAdv = multivalues[ID][1].ToString();
                        // D2023
                        if (string.IsNullOrEmpty(sValue))
                        {
                            fUpdate = !tJud.IsUndefined;
                            // D1331
                            tJud.IsUndefined = true;
                        }
                        else
                        {
                            // D1858
                            Value = Convert.ToDouble(sValue);
                                // D2023 ===
                                if (Math.Abs(Value) >= Math.Abs(ECCore.TeamTimeFuncs.TeamTimeFuncs.UndefinedValue))
                                {
                                    if (!tJud.IsUndefined)
                                    {
                                        tJud.IsUndefined = true;
                                        fUpdate = true;
                                    }
                                }
                                else
                                {
                                    int Adv = 0;
                                    // D2031
                                    if (int.TryParse(sAdv, out Adv) && (tJud.IsUndefined || tJud.Value.ToString("F6") != Value.ToString("F6") || tJud.Advantage != Adv))
                                    {
                                        tJud.IsUndefined = false;
                                        tJud.Value = Math.Abs(Value);
                                        tJud.Advantage = Adv;
                                        
                                    }
                                    fUpdate = true;
                                }
                            
                        }
                        string sComment = multivalues[ID][2].ToString();
                        if (sComment != tJud.Comment)
                        {
                            //tJud.Comment += sComment + ";";
                            tJud.Comment = sComment;
                            fUpdate = true;
                        }

                        if (fUpdate)
                        {
                            if (Action.ActionType == ActionType.atAllPairwiseOutcomes)
                            {
                                if (Action.PWONode != null)
                                {
                                    Action.PWONode.PWOutcomesJudgments.AddMeasureData(tJud);
                                }

                                if (Action.PWONode != null && Action.ParentNode != null)
                                {
                                    Action.ParentNode.PWOutcomesJudgments.AddMeasureData(tJud);
                                }

                                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(Action.PWONode, tJud);
                            }
                            else
                            {
                                App.ActiveProject.HierarchyObjectives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(tJud);
                                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, tJud);
                            }
                        }

                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }
        }


        [WebMethod(EnableSession = true)]
        public static void SaveStepFunction(int step, string value, string sComment)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var app = (clsComparionCore)context.Session["App"];
            var action = AnytimeClass.GetAction(step);
            var judgment = (clsStepMeasureData)((clsOneAtATimeEvaluationActionData)action.ActionData).Judgment;
            value = value == "-2147483648000" ? "" : value;

            if (sComment != judgment.Comment || value != judgment.SingleValue.ToString() || (value != "" && judgment.IsUndefined))
            {
                judgment.Comment = sComment;
                double val = 0;

                if (value != "" && StringFuncs.String2Double(value, ref val) && val > (ECTypes.UNDEFINED_SINGLE_VALUE / 2))
                {
                    judgment.ObjectValue = val;
                    judgment.IsUndefined = false;
                }
                else
                {
                    judgment.IsUndefined = true;
                }

                //if (IsRiskWithControls)
                //{
                //    App.ActiveProject.ProjectManager.PipeBuilder.SaveControlJudgment(Action);
                //}
                //else
                //{
                ((clsOneAtATimeEvaluationActionData)action.ActionData).Node.Judgments.AddMeasureData(judgment);
                app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(app.ActiveProject.HierarchyObjectives.GetNodeByID(judgment.ParentNodeID), judgment);
                //}
            }
        }

        [WebMethod(EnableSession = true)]
        public static void SaveUtilityCurve(int step, string value, string sComment)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var app = (clsComparionCore)context.Session["App"];
            var action = AnytimeClass.GetAction(step);
            var judgment = (clsUtilityCurveMeasureData)((clsOneAtATimeEvaluationActionData)action.ActionData).Judgment;
            value = value == "-2147483648000" ? "" : value;

            if (sComment != judgment.Comment || value != judgment.SingleValue.ToString() || (value != "" && judgment.IsUndefined))
            {
                judgment.Comment = sComment;

                if (value == "")
                {
                    judgment.IsUndefined = true;
                }
                else
                {
                    double val = 0;
                    if (StringFuncs.String2Double(value, ref val))
                    {
                        judgment.ObjectValue = val;
                        judgment.IsUndefined = false;
                    }
                }
            }

            //if (IsRiskWithControls)
            //{
            //    App.ActiveProject.ProjectManager.PipeBuilder.SaveControlJudgment(Action);
            //}
            //else
            //{
            ((clsOneAtATimeEvaluationActionData)action.ActionData).Node.Judgments.AddMeasureData(judgment);
            app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(app.ActiveProject.HierarchyObjectives.GetNodeByID(judgment.ParentNodeID), judgment);
            //}
            //}
        }

        [WebMethod(EnableSession = true)]
        public static void SaveRatings(int step, string RatingID, string sComment, List<string[]> intensities, int UserID = 0, string DirectValue = "")
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var app = (clsComparionCore)context.Session["App"];
            var action = AnytimeClass.GetAction(step);
            var data = (clsOneAtATimeEvaluationActionData)action.ActionData;

            clsNonPairwiseMeasureData ratingData = null;
            if (UserID == 0)
            {
                ratingData = (clsNonPairwiseMeasureData)data.Judgment;
            }
            else
            {
                ratingData = ((clsNonPairwiseJudgments)data.Node.Judgments).GetJudgement(((clsNonPairwiseMeasureData)data.Judgment).NodeID, data.Node.NodeID, UserID);
                if (ratingData == null)
                {
                    ratingData = new clsRatingMeasureData(((clsNonPairwiseMeasureData)data.Judgment).NodeID, data.Node.NodeID, UserID, null, (clsRatingScale)data.Node.MeasurementScale, true);
                }
            }

            var mScale = data.MeasurementScale;

            if (!string.IsNullOrEmpty(RatingID))
            {
                if (RatingID == "-1")
                {
                    ratingData.IsUndefined = true;
                }
                else
                {
                    if (mScale != null)
                    {
                        foreach (clsRating tR in ((clsRatingScale)mScale).RatingSet)
                        {
                            if (tR.ID.ToString() == RatingID)
                            {
                                ratingData.IsUndefined = false;
                                ratingData.ObjectValue = tR;
                                break;
                            }
                        }
                    }
                }

                double ratingDirect = -1;
                if (Convert.ToInt64(RatingID) < 0 && RatingID != "-1" && StringFuncs.String2Double(DirectValue, ref ratingDirect))
                {
                    if (ratingDirect >= 0 && ratingDirect <= 1)
                    {
                        ratingData.IsUndefined = false;
                        ratingData.ObjectValue = new clsRating(-1, "Direct input from EC Core", (float)ratingDirect, null);
                    }
                }

                if (sComment != ratingData.Comment)
                {
                    // RatingData.Comment += sComment + ";";
                    ratingData.Comment = sComment;
                }

                if (data.Node != null)
                {
                    if (data.Node.IsAlternative)
                    {
                        app.ActiveProject.HierarchyAlternatives.GetNodeByID(data.Node.NodeID).Judgments.AddMeasureData(ratingData);
                    }
                    else
                    {
                        app.ActiveProject.HierarchyObjectives.GetNodeByID(data.Node.NodeID).Judgments.AddMeasureData(ratingData);
                    }
                }

                app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.Node, ratingData);
                var isAlreadySaved = (bool)context.Session[SessionIsJudgmentAlreadySaved];

                if (!isAlreadySaved)
                {
                    context.Session[SessionIncreaseJudgmentsCount] = true;
                    context.Session[SessionIsJudgmentAlreadySaved] = true;
                }

                var isChanged = false;
                var isPm = app.CanUserModifyProject(app.ActiveUser.UserID, app.ProjectID, Uw, Ws, app.ActiveWorkgroup);

                if (isPm && mScale != null)
                {
                    foreach (clsRating intensity in ((clsRatingScale)mScale).RatingSet)
                    {
                        string[] modifiedIntensity = intensities.FirstOrDefault(i => i[2] == intensity.ID.ToString());
                        if (modifiedIntensity != null && modifiedIntensity.Length == 5 && intensity.Comment != modifiedIntensity[4])
                        {
                            intensity.Comment = modifiedIntensity[4];
                            isChanged = true;
                        }
                    }
                }

                if (isChanged)
                {
                    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static void SaveMultiRatingsData(int step, object[][] multivalues, List<List<string[]>> intensities)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var app = (clsComparionCore)context.Session["App"];
            var action = AnytimeClass.GetAction(step);

            if (action.ActionData is clsAllChildrenEvaluationActionData)
            {
                var data = (clsAllChildrenEvaluationActionData)action.ActionData;
                clsNode alt;
                int ratingID;
                double ratingDirect;

                for (var i = 0; i < data.Children.Count; i++)
                {
                    ratingID = Convert.ToInt16(multivalues[i][0]);
                    ratingDirect = Convert.ToDouble(multivalues[i][1]);
                    alt = data.Children[i];
                    var ratings = app.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(data.ParentNode.get_RatingScaleID());
                    if (ratings != null)
                    {
                        var altRating = ratings.GetRatingByID(ratingID);
                        if (altRating == null && ratingDirect >= 0 && ratingDirect <= 1)
                        {
                            altRating = new clsRating(-1, "Direct input from EC Core", (float)ratingDirect, null);
                        }

                        var R = (clsRatingMeasureData)data.GetJudgment(alt);

                        if (app.ActiveProject.PipeParameters.ShowComments && R != null)
                        {
                            //var sComment = R.Comment;
                            //sComment +=  multivalues[i][2].ToString() + ";";
                            var sComment = multivalues[i][2].ToString();
                            data.SetData(alt, altRating, sComment);
                        }
                        else
                        {
                            data.SetData(alt, altRating);
                        }

                        if (data.ParentNode.IsAlternative)
                        {
                            app.ActiveProject.HierarchyAlternatives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(data.GetJudgment(alt));
                        }
                        else
                        {
                            app.ActiveProject.HierarchyObjectives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(data.GetJudgment(alt));
                        }

                        app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, data.GetJudgment(alt));
                    }
                }
            }

            if (action.ActionData is clsAllCoveringObjectivesEvaluationActionData)
            {
                var data = (clsAllCoveringObjectivesEvaluationActionData)action.ActionData;
                clsNode covObj;
                int ratingID;
                double ratingDirect;

                for (int i = 0; i < data.CoveringObjectives.Count; i++)
                {
                    ratingID = Convert.ToInt16(multivalues[i][0]);
                    ratingDirect = Convert.ToDouble(multivalues[i][1]);
                    covObj = data.CoveringObjectives[i];
                    var ratings = app.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(covObj.get_RatingScaleID());
                    if (ratings != null)
                    {
                        var altRating = ratings.GetRatingByID(ratingID);
                        if (altRating == null && ratingDirect >= 0 && ratingDirect <= 1)
                        {
                            altRating = new clsRating(-1, "Direct input from EC Core", (float)ratingDirect, null);
                        }

                        var R = (clsRatingMeasureData)data.GetJudgment(covObj);

                        if (app.ActiveProject.PipeParameters.ShowComments && R != null)
                        {
                            //var sComment = R.Comment;
                            //sComment += multivalues[i][2].ToString() + ";";
                            var sComment = multivalues[i][2].ToString();
                            data.SetData(ref covObj, altRating, sComment);
                        }
                        else
                        {
                            data.SetData(ref covObj, altRating);
                        }

                        app.ActiveProject.HierarchyObjectives.GetNodeByID(covObj.NodeID).Judgments.AddMeasureData(data.GetJudgment(covObj));
                        app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(covObj, data.GetJudgment(covObj));
                    }
                }
            }

            if (action.ActionData is clsAllEventsWithNoSourceEvaluationActionData)
            {
                var data = (clsAllEventsWithNoSourceEvaluationActionData)action.ActionData;
                clsNode alt;
                int ratingID;
                double ratingDirect;

                for (int i = 0; i < data.Alternatives.Count; i++)
                {
                    ratingID = Convert.ToInt16(multivalues[i][0]);
                    ratingDirect = Convert.ToDouble(multivalues[i][1]);
                    alt = data.Alternatives[i];
                    var ratings = app.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(alt.get_RatingScaleID());
                    if (ratings != null)
                    {
                        var altRating = ratings.GetRatingByID(ratingID);
                        if (altRating == null && ratingDirect >= 0 && ratingDirect <= 1)
                        {
                            altRating = new clsRating(-1, "Direct input from EC Core", (float)ratingDirect, null);
                        }

                        var R = (clsRatingMeasureData)data.GetJudgment(alt);

                        if (app.ActiveProject.PipeParameters.ShowComments && R != null)
                        {
                            //var sComment = R.Comment;
                            //sComment += multivalues[i][2].ToString() + ";";
                            var sComment = multivalues[i][2].ToString();
                            data.SetData(ref alt, altRating, sComment);
                        }
                        else
                        {
                            data.SetData(ref alt, altRating);
                        }

                        app.ActiveProject.HierarchyAlternatives.GetNodeByID(alt.NodeID).DirectJudgmentsForNoCause.AddMeasureData(data.GetJudgment(alt));
                        app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(alt, data.GetJudgment(alt));
                    }
                }
            }

            var isPm = app.CanUserModifyProject(app.ActiveUser.UserID, app.ProjectID, Uw, Ws, app.ActiveWorkgroup);
            if (isPm && app.ActiveProject.ProjectManager.MeasureScales.RatingsScales != null)
            {
                var isChanged = false;
                foreach (var mScale in app.ActiveProject.ProjectManager.MeasureScales.RatingsScales)
                {
                    foreach (var intensity in ((clsRatingScale)mScale).RatingSet)
                    {
                        foreach (var savedIntensity in intensities)
                        {
                            string[] modifiedIntensity = savedIntensity.FirstOrDefault(ir => ir[0] == intensity.GuidID.ToString());
                            if (modifiedIntensity != null && intensity.Comment != modifiedIntensity[1])
                            {
                                intensity.Comment = modifiedIntensity[1];
                                isChanged = true;
                                break;
                            }
                        }
                    }
                }

                if (isChanged)
                {
                    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure();
                }
            }

        }

        [WebMethod(EnableSession = true)]
        public static void SaveDirect(int step, string value, string sComment)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var app = (clsComparionCore)context.Session["App"];
            var action = AnytimeClass.GetAction(step);

            if (action == null || action.ActionType != ActionType.atNonPWOneAtATime) return;

            var data = (clsOneAtATimeEvaluationActionData)action.ActionData;
            var directJudgment = (clsDirectMeasureData)data.Judgment;

            if (value == "-1" || value == "")
            {
                directJudgment.IsUndefined = true;
            }
            else
            {
                double directValue = -1;
                if (StringFuncs.String2Double(value, ref directValue))
                {
                    directJudgment.ObjectValue = directValue;
                    directJudgment.IsUndefined = false;
                }
            }

            if (sComment != directJudgment.Comment)
            {
                directJudgment.Comment = sComment;
            }

            ((clsOneAtATimeEvaluationActionData)action.ActionData).Node.Judgments.AddMeasureData(directJudgment);
            app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(app.ActiveProject.HierarchyObjectives.GetNodeByID(directJudgment.ParentNodeID), directJudgment);
        }

        [WebMethod(EnableSession = true)]
        public static void SaveMultiDirectData(int step, object[][] multivalues)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var app = (clsComparionCore)context.Session["App"];
            var action = AnytimeClass.GetAction(step);

            if (action.ActionData is clsAllChildrenEvaluationActionData)
            {
                var data = (clsAllChildrenEvaluationActionData)action.ActionData;
                double value = -1;

                for (int i = 0; i < data.Children.Count; i++)
                {
                    var tNode = data.Children[i];
                    var directData = (clsDirectMeasureData)data.GetJudgment(tNode);
                    var sValue = multivalues[i][0].ToString();

                    if (sValue == "")
                    {
                        directData.IsUndefined = true;
                    }
                    else
                    {
                        if (StringFuncs.String2Double(sValue, ref value))
                        {
                            directData.ObjectValue = value;
                            directData.IsUndefined = false;
                        }
                    }

                    var sComment = multivalues[i][1].ToString();
                    if (sComment != directData.Comment)
                    {
                        directData.Comment = sComment;
                    }

                    ((clsAllChildrenEvaluationActionData)action.ActionData).ParentNode.Judgments.AddMeasureData(data.GetJudgment(tNode));
                    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, directData);
                }
            }

            if (action.ActionData is clsAllCoveringObjectivesEvaluationActionData)
            {
                var data = (clsAllCoveringObjectivesEvaluationActionData)action.ActionData;
                double value = -1;

                for (int i = 0; i < data.CoveringObjectives.Count; i++)
                {
                    var tNode = data.CoveringObjectives[i];
                    var directData = (clsDirectMeasureData)data.GetJudgment(tNode);
                    var sValue = multivalues[i][0].ToString();

                    if (sValue == "")
                    {
                        directData.IsUndefined = true;
                    }
                    else
                    {
                        if (StringFuncs.String2Double(sValue, ref value))
                        {
                            directData.ObjectValue = value;
                            directData.IsUndefined = false;
                        }
                    }

                    var sComment = multivalues[i][1].ToString();
                    if (sComment != directData.Comment)
                    {
                        directData.Comment = sComment;
                    }

                    app.ActiveProject.HierarchyObjectives.GetNodeByID(tNode.NodeID).Judgments.AddMeasureData(directData);
                    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.Alternative, directData);
                }
            }

            if (action.ActionData is clsAllEventsWithNoSourceEvaluationActionData)
            {
                var data = (clsAllEventsWithNoSourceEvaluationActionData)action.ActionData;
                double value = -1;

                for (int i = 0; i < data.Alternatives.Count; i++)
                {
                    var tNode = data.Alternatives[i];
                    var directData = (clsDirectMeasureData)data.GetJudgment(tNode);
                    var sValue = multivalues[i][0].ToString();

                    if (sValue == "")
                    {
                        directData.IsUndefined = true;
                    }
                    else
                    {
                        if (StringFuncs.String2Double(sValue, ref value))
                        {
                            directData.ObjectValue = value;
                            directData.IsUndefined = false;
                        }
                    }

                    var sComment = multivalues[i][1].ToString();
                    if (sComment != directData.Comment)
                    {
                        directData.Comment = sComment;
                    }

                    app.ActiveProject.HierarchyAlternatives.GetNodeByID(tNode.NodeID).DirectJudgmentsForNoCause.AddMeasureData(directData);
                    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(tNode, directData);
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static int reviewJudgment(int parentnodeID, int current_step)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var CurrentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(parentnodeID);
            return App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(CurrentNode, current_step) + 1;
        }



        [WebMethod(EnableSession = true)]
        public static int redoPairs(int parentnodeID, int current_step, string firstNode, string secondNode, bool is_name, bool add_step)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var CurrentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(parentnodeID);
            var minStep = app.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(CurrentNode, current_step) + 1;
            var sFirstNode = new clsNode();
            var sSecondNode = new clsNode();
            AnytimeClass.JudgmentsSaved = true;
            if (add_step)
            {
                int PID = parentnodeID;
                int ID1 = -1;
                int ID2 = -1;
                if (int.TryParse(firstNode, out ID1) && int.TryParse(secondNode, out ID2))
                {
                    clsShowLocalResultsActionData Data = (clsShowLocalResultsActionData)AnytimeClass.GetAction(current_step).ActionData;
                    bool fIsPWOutcomes = Data.PWOutcomesNode != null && Data.ParentNode.get_MeasureType() == ECMeasureType.mtPWOutcomes;

                    if (fIsPWOutcomes)
                    {
                        app.ActiveProject.ProjectManager.PipeBuilder.AddPairToPipePWOutcomes(Data.PWOutcomesNode, Data.ParentNode, ID1, ID2, current_step - 1);
                    }
                    else
                    {
                        app.ActiveProject.ProjectManager.PipeBuilder.AddPairToPipe(PID, ID1, ID2, current_step - 1);
                        //C0966
                    }
                    if (!app.ActiveProject.PipeParameters.ObjectivesPairwiseOneAtATime)
                        current_step = current_step - 1;

                    NotRecreatePipe = true;
                }

            }
            else
            {
                for (int i = minStep; i < current_step; i++)
                {
                    var action = AnytimeClass.GetAction(i);
                    switch (action.ActionType)
                    {
                        case ActionType.atPairwise:
                            {
                                var pwdata = (clsPairwiseMeasureData)AnytimeClass.GetAction(i).ActionData;
                                var parentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwdata.ParentNodeID);
                                bool fAlts = parentNode.IsTerminalNode;
                                if (fAlts)
                                {
                                    sFirstNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(pwdata.FirstNodeID);
                                    sSecondNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(pwdata.SecondNodeID);
                                }
                                else
                                {
                                    sFirstNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwdata.FirstNodeID);
                                    sSecondNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwdata.SecondNodeID);
                                }
                                break;
                            }
                        case ActionType.atAllPairwise:
                        case ActionType.atAllPairwiseOutcomes:
                            {
                                var pwdata = (clsAllPairwiseEvaluationActionData)AnytimeClass.GetAction(i).ActionData;
                                var parentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwdata.ParentNode.NodeID);
                                bool fAlts = parentNode.IsTerminalNode;
                                var ID = 0;
                                foreach (clsPairwiseMeasureData tJud in pwdata.Judgments)
                                {
                                    var idx = pwdata.Judgments.IndexOf(tJud);
                                    if (fAlts)
                                    {
                                        sFirstNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.FirstNodeID);
                                        sSecondNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.SecondNodeID);
                                    }
                                    else
                                    {
                                        sFirstNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.FirstNodeID);
                                        sSecondNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.SecondNodeID);
                                    }

                                    if (is_name)
                                    {
                                        if (sFirstNode.NodeName.ToLower() == firstNode.ToLower() && sSecondNode.NodeName.ToLower() == secondNode.ToLower())
                                        {
                                            return -idx;
                                        }
                                        if (sFirstNode.NodeName.ToLower() == secondNode.ToLower() && sSecondNode.NodeName.ToLower() == firstNode.ToLower())
                                        {
                                            return -idx;
                                        }
                                    }
                                    else
                                    {
                                        if (sFirstNode.NodeID.ToString() == firstNode &&
                                        sSecondNode.NodeID.ToString() == secondNode)
                                        {
                                            return -idx;
                                        }
                                        if (sFirstNode.NodeID.ToString() == secondNode &&
                                            sSecondNode.NodeID.ToString() == firstNode)
                                        {
                                            return -idx;
                                        }
                                    }



                                }
                                break;
                            }
                    }

                    if (is_name)
                    {
                        if (sFirstNode.NodeName.ToLower() == firstNode.ToLower() && sSecondNode.NodeName.ToLower() == secondNode.ToLower())
                        {
                            return i;
                        }
                        if (sFirstNode.NodeName.ToLower() == secondNode.ToLower() && sSecondNode.NodeName.ToLower() == firstNode.ToLower())
                        {
                            return i;
                        }
                    }
                    else
                    {
                        if (sFirstNode.NodeID.ToString() == firstNode && sSecondNode.NodeID.ToString() == secondNode)
                        {
                            return i;
                        }
                        if (sFirstNode.NodeID.ToString() == secondNode && sSecondNode.NodeID.ToString() == firstNode)
                        {
                            return i;
                        }
                    }

                }
            }

            return current_step;
        }


        [WebMethod(EnableSession = true)]
        public static string[][] getInfoDocSizes()
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            bool isPm = App.ActiveUser != null && App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);
            return GeckoClass.getInfodoc_sizes(isPm);
        }

        [WebMethod(EnableSession = true)]
        public static void setInfoDocSizes(string width, string height, int index, bool is_multi)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            bool isPm = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);
            GeckoClass.setInfodoc_sizes(width, height, index, is_multi, isPm);
        }

        [WebMethod(EnableSession = true)]
        public static void setInfodocParams(string NodeID, string WrtNodeID, string value, bool is_multi = false)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            if (WrtNodeID == "")
            {
                GeckoClass.SetInfodocParams(Guid.Parse(NodeID), Guid.Empty, value, is_multi);
            }
            else
            {
                GeckoClass.SetInfodocParams(Guid.Parse(NodeID), Guid.Parse(WrtNodeID), value, is_multi);
            }
           
        }

        [WebMethod(EnableSession = true)]
        public static void GetInfodocParams(string NodeID, string WrtNodeID, bool is_multi = false)
        {
            if (WrtNodeID == "")
            {
                GeckoClass.GetInfodocParams(Guid.Parse(NodeID), Guid.Empty, is_multi);
            }
            else
            {
                GeckoClass.GetInfodocParams(Guid.Parse(NodeID), Guid.Parse(WrtNodeID), is_multi);
            }
        }

        [WebMethod(EnableSession = true)]
        public static string getQuickHelpInfo(int tNodeID, PipeParameters.ecEvaluationStepType tEvalStep, int step, bool show_qh_automatically)
        {
            var message = "";
            var context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];

            if (app != null)
            {
                var HelpID = tEvalStep;
                var AutoShow = show_qh_automatically;
                bool isCluster = false;     // D4082
                string objId = AnytimeClass.GetQuickHelpObjectIdFromSession();

                message = app.ActiveProject.ProjectManager.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(app.ActiveProject.ProjectManager, step, ref isCluster, ref AutoShow); // D4082
                message = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.QuickHelp, objId, message, true, true, -1);
                //message = TeamTimeClass.ParseAllTemplates(message, app.ActiveUser, app.ActiveProject);
                message = StringFuncs.ParseVideoLinks(message, false);
            }

            return message;
        }

        [WebMethod(EnableSession = true)]
        public static void setQuickHelpInfo(int tNodeID, PipeParameters.ecEvaluationStepType tEvalStep, int step, int actual_step, string message, bool show_qh_automatically)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var App = (clsComparionCore)context.Session["App"];

           // var action = AnytimeClass.GetAction(step);

           // PipeParameters.ecEvaluationStepType HelpID = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionStepType(action);

            if (App != null)
            {
                bool isPM = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);
                if (isPM)
                {
                    var AutoShow = show_qh_automatically;
                    Guid nodeGuid = HttpContext.Current.Session[SessionParentNodeGuid] == null ? new Guid() : (Guid)HttpContext.Current.Session[SessionParentNodeGuid];
                    clsNode node = AnytimeClass.GetNodeByGuid(nodeGuid);

                    string objectId = InfodocService.GetQuickHelpObjectID(tEvalStep, node);
                    string basePath = InfodocService.Infodoc_Path(App.ActiveProject.ID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.QuickHelp, objectId, -1);
                    var baseUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}";
                    string infoDocQh = InfodocService.Infodoc_Pack(message, baseUrl, basePath);

                    App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.SetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, step, false, AutoShow, infoDocQh);  // D4082
                    
                    string snapshotComment = AnytimeClass.GetNodeTypeAndName(node);
                    snapshotComment = string.Format("{0} {1}", tEvalStep.ToString(), snapshotComment).Trim();

                    bool fUpdated = App.ActiveProject.PipeParameters.PipeMessages.Save(PipeParameters.PipeStorageType.pstStreamsDatabase, App.ActiveProject.ConnectionString, App.ActiveProject.ProviderType, App.ActiveProject.ID);
                    if(fUpdated)
                    {
                        App.SaveProjectLogEvent(App.ActiveProject, "Edit quick help", false, snapshotComment);
                        //App.ActiveProject.SaveProjectOptions("Edit quick help", false, true, snapshotComment);
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static void setQuickHelpInfoByCluster(List<string[]> nodes, string qh_info, bool show_qh_automatically)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var App = (clsComparionCore)context.Session["App"];
            bool isPM = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);

            foreach (string[] node in nodes)
            {
                var step = Int16.Parse(node[1]);
               // var action = AnytimeClass.GetAction(step);

               // PipeParameters.ecEvaluationStepType HelpID = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionStepType(action);

                if (App != null)
                {
                  
                    if (isPM)
                    {
                        var AutoShow = show_qh_automatically;
                        App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.SetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, step, true, AutoShow, qh_info);  // D4082
                        bool fUpdated = App.ActiveProject.PipeParameters.PipeMessages.Save(PipeParameters.PipeStorageType.pstStreamsDatabase, App.ActiveProject.ConnectionString, App.ActiveProject.ProviderType, App.ActiveProject.ID);
                        if (fUpdated)
                        {
                            App.SaveProjectLogEvent(App.ActiveProject, "Edit quick help", false, "Edit QuickHelp");
                            //App.ActiveProject.SaveProjectOptions("Edit quick help", false, true, "Edit QuickHelp");   // D4082
                        }
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static void setAutoAdvance(bool value)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            if (App != null) {
                //bool isPM = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);
                //if (isPM)
                //{
                //    App.ActiveProject.PipeParameters.AllowAutoadvance = value;
                //    App.ActiveProject.SaveProjectOptions("Save auto advance");
                //}
                HttpContext.Current.Session[SessionAutoAdvance + App.ProjectID] = (bool)value;
                //HttpContext.Current.Response.Cookies["AutoAdvance"].Expires = DateTime.Now.AddDays(10);
                //HttpContext.Current.Response.Cookies["AutoAdvance"].Value = value.ToString();

            }
        }

        [WebMethod(EnableSession = true)]
        public static void setInfodocMode(string value)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var App = (clsComparionCore)context.Session["App"];
            if (App != null)
            {
                bool isPM = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);
                if (isPM)
                {
                    if (value == "1")
                    {
                        App.ActiveProject.PipeParameters.ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmPopup;
                    }
                    else
                    {
                        App.ActiveProject.PipeParameters.ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmFrame;
                    }
                    App.ActiveProject.SaveProjectOptions("Save infodoc mode");
                }
                HttpContext.Current.Session["InfodocMode"] = value;
                //HttpContext.Current.Response.Cookies["AutoAdvance"].Expires = DateTime.Now.AddDays(10);
                //HttpContext.Current.Response.Cookies["AutoAdvance"].Value = value.ToString();

            }
        }

        //QH settings in general
        [WebMethod(EnableSession = true)]
        public static string getQHSettingCookies()
        {

            if (HttpContext.Current.Request.Cookies["Dont_Show_QH"] == null)
            {
                HttpContext.Current.Response.Cookies["Dont_Show_QH"].Expires = DateTime.Now.AddDays(10);
                HttpContext.Current.Response.Cookies["Dont_Show_QH"].Value = false.ToString();
            }

            return HttpContext.Current.Request.Cookies["Dont_Show_QH"].Value.ToString();
        }

        [WebMethod(EnableSession = true)]
        public static void setQHSettingCookies(bool status)
        {
            HttpContext.Current.Response.Cookies["Dont_Show_QH"].Expires = DateTime.Now.AddDays(10);
            HttpContext.Current.Response.Cookies["Dont_Show_QH"].Value = status.ToString();
        }

        //individual step cookies for QH if shown already
        [WebMethod(EnableSession = true)]
        public static void setQHCookies(int ProjectID, int step, string status, string qh_text)
        {
            string token = ProjectID + "-" + step + "-qh";
            var cookie = token.GetHashCode().ToString();

            HttpContext.Current.Session[qh_text] = true;


            HttpContext.Current.Response.Cookies[cookie].Expires = DateTime.Now.AddDays(10);
            HttpContext.Current.Response.Cookies[cookie].Value = status.ToString();
        }

        [WebMethod(EnableSession = true)]
        public static void dontShowMessageCookie(bool status)
        {
            HttpContext.Current.Session["showMessage"] = status;
        }

        [WebMethod(EnableSession = true)]
        public static string getQHCookies(int ProjectID, int step)
        {
            string token = ProjectID + "-" + step + "-qh";
            var cookie = token.GetHashCode().ToString();
            
            if (HttpContext.Current.Request.Cookies[cookie] == null)
            {
                HttpContext.Current.Response.Cookies[cookie].Expires = DateTime.Now.AddDays(10);
                HttpContext.Current.Response.Cookies[cookie].Value = "0";
            }
            
            return HttpContext.Current.Request.Cookies[cookie].Value.ToString();
        }

        [WebMethod(EnableSession = true)]
        public static void setCollapseCookies(int projectId, string stepType, string node_type, string status)
        {
            string token = $"{projectId}-{stepType}-{node_type}";
            var cookie = token.GetHashCode().ToString();
            var isMulti = stepType.StartsWith("All", StringComparison.CurrentCultureIgnoreCase);

            if (HttpContext.Current.Request.Cookies[cookie] == null)
            {
                HttpContext.Current.Response.Cookies[cookie].Value = status;
                HttpContext.Current.Response.Cookies[cookie].Expires = DateTime.Now.AddDays(10);
            }
            else
            {
                HttpContext.Current.Response.Cookies[cookie].Value = status ;
            }

            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var multiCollapseStatus = context.Session[SessionMultiCollapse] == null ? new Dictionary<string, List<bool>>() : (Dictionary<string, List<bool>>)context.Session[SessionMultiCollapse];

            if (App != null)
            {
                bool isPM = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);
                if (isPM)
                {
                    string dictionaryKey = $"Key{projectId}_{stepType}";
                    List<bool> multi_collapse = new List<bool>();
                    bool exists = multiCollapseStatus.ContainsKey(dictionaryKey);

                    if (!exists)
                    {
                        multi_collapse.Add(false);
                        multi_collapse.Add(false);
                        multi_collapse.Add(false);
                        multi_collapse.Add(false);
                        multi_collapse.Add(false);
                        multiCollapseStatus.Add(dictionaryKey, multi_collapse);
                    }
                    else
                    {
                        multi_collapse = multiCollapseStatus[dictionaryKey];
                    }

                    bool collapse = status == "1" ? true : false;

                    if (node_type == "pw-nodes" || node_type == "parent-node")
                    {
                        if (isMulti)
                        {
                            multi_collapse[0] = collapse;
                            multi_collapse[1] = collapse;
                            multi_collapse[2] = collapse;
                        }
                        else
                        {
                            if (node_type == "pw-nodes")
                            {
                                if (stepType == "One_mtDirect" || stepType == "One_mtRatings" || stepType == "One_mtStep" || stepType == "One_mtRegularUtilityCurve")
                                {
                                    multi_collapse[0] = collapse;
                                    multi_collapse[1] = collapse;
                                    multi_collapse[3] = collapse;
                                }
                                else
                                {
                                    multi_collapse[1] = collapse;
                                    multi_collapse[2] = collapse;
                                }
                            }
                            else
                            {
                                multi_collapse[0] = collapse;
                            }
                        }
                    }
                    else if (node_type == "pw-wrt-nodes")
                    {
                        multi_collapse[3] = collapse;
                        multi_collapse[4] = collapse;
                    }

                    multiCollapseStatus[dictionaryKey] = multi_collapse;
                    context.Session[SessionMultiCollapse] = multiCollapseStatus;
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static string getCollapseCookies(int projectId, string stepType, string node_type, bool is_multi)
        {
            string token = $"{projectId}-{stepType}-{node_type}";
            var cookie = token.GetHashCode().ToString();
            var is_collapsed = "0";

            var multiCollapseStatus = HttpContext.Current.Session[SessionMultiCollapse] == null ? new Dictionary<string, List<bool>>() : (Dictionary<string, List<bool>>)HttpContext.Current.Session[SessionMultiCollapse];
            var singePwCollapseStatus = HttpContext.Current.Session[SessionSinglePwCollapse] == null ? new Dictionary<string, List<bool>>() : (Dictionary<string, List<bool>>)HttpContext.Current.Session[SessionSinglePwCollapse];

            if (HttpContext.Current.Request.Cookies[cookie] == null)
            {
                string dictionaryKey = $"Key{projectId}_{stepType}";

                if (is_multi)
                {
                    var pm_saved_collapsed = new List<bool>();
                    bool exists = multiCollapseStatus.ContainsKey(dictionaryKey);

                    if (exists)
                    {
                        pm_saved_collapsed = multiCollapseStatus[dictionaryKey];
                        if (node_type == "pw-nodes")
                        {
                            is_collapsed = pm_saved_collapsed[0] == true ? "1" : "0";
                        }
                        else
                        {
                            is_collapsed = pm_saved_collapsed[3] == true ? "1" : "0";
                        }
                    }
                    else
                    {
                        is_collapsed = "0";
                    }
                }
                else
                {
                    var pm_saved_collapsed = new List<bool>();
                    bool exists = singePwCollapseStatus.ContainsKey(dictionaryKey);

                    if (exists)
                    {
                        pm_saved_collapsed = singePwCollapseStatus[dictionaryKey];
                        try
                        {
                            if (node_type == "pw-nodes")
                            {
                                is_collapsed = pm_saved_collapsed[1] == true ? "1" : "0";
                            }
                            else if (node_type == "parent-node")
                            {
                                is_collapsed = pm_saved_collapsed[0] == true ? "1" : "0";
                            }
                            else
                            {
                                is_collapsed = pm_saved_collapsed[3] == true ? "1" : "0";
                            }
                        }
                        catch (Exception e)
                        {
                            is_collapsed = "0";
                        }
                    }
                    else
                    {
                        is_collapsed = "0";
                    }
                }
                
                HttpContext.Current.Response.Cookies[cookie].Value = is_collapsed;
                HttpContext.Current.Response.Cookies[cookie].Expires = DateTime.Now.AddDays(10);
            }

            return HttpContext.Current.Request.Cookies[cookie].Value;
        }

        [WebMethod(EnableSession = true)]
        public static void setSingleCollapsePrivateVar(int ProjectID, int step, List<bool> collapsed_status_list)
        {
            try
            {
                var singePwCollapseStatus = HttpContext.Current.Session[SessionSinglePwCollapse] == null ? new Dictionary<string, List<bool>>() : (Dictionary<string, List<bool>>)HttpContext.Current.Session[SessionSinglePwCollapse];
                singePwCollapseStatus[ProjectID + "_" + step] = collapsed_status_list;
                HttpContext.Current.Session[SessionSinglePwCollapse] = singePwCollapseStatus;
            }
            catch (Exception e)
            {

            }
            
        }

        [WebMethod(EnableSession = true)]
        public static int GetFirstUnassessed()
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            return GetFirstUnassessed(app);
        }

        private static int GetFirstUnassessed(clsComparionCore app)
        {
            int returnValue = 0;    //app.ActiveProject.Pipe.Count - 1;

            for (int i = 1; i < app.ActiveProject.Pipe.Count; i++)
            {
                if (AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)))
                {
                    returnValue = i;
                    break;
                }
            }

            return returnValue;
        }

        [WebMethod(EnableSession = true)]
         public static int[] GetNextUnassessed(int StartingStep)
         {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            int[] returnValue = new int[2];
            if (StartingStep < 0)
                StartingStep = 0;
            int unassessed_step = StartingStep;
            

            var unassessed_count = 0;
            for (int i = 1; i < App.ActiveProject.Pipe.Count; i++)
            {
                if (AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)))
                {
                    unassessed_count += 1;
                    if (unassessed_count > 2)
                        break;
                }
            }

            for (int i = unassessed_step + 1; i < App.ActiveProject.Pipe.Count; i++)
            {
                if (AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)))
                {
                    unassessed_step = i;
                    returnValue[0] = unassessed_step;
                    returnValue[1] = unassessed_count;
                    return returnValue;
                }
            }

            for (int i = 1; i < App.ActiveProject.Pipe.Count; i++)
            {
                if (AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)))
                {
                    unassessed_step = i;
                    returnValue[0] = unassessed_step;
                    returnValue[1] = unassessed_count;
                    return returnValue;
                }
            }
            return null;
         }

        [WebMethod(EnableSession = true)]
        public static string[] getExpectedValue()
        {
            var expectedvalue = ExpectedValueString;
            if (ExpectedValueString[0] == null && ExpectedValueString[1] == null)
            {
                expectedvalue = null;
            }
            return expectedvalue;
        }

        [WebMethod(EnableSession = true)]
        public static bool toggleExpectedValue(bool showvalue, string screentype)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var IsShowValue = false;
            if(screentype == "atShowLocalResults")
            {
                App.ActiveProject.PipeParameters.ShowExpectedValueLocal = showvalue;
                IsShowValue = App.ActiveProject.PipeParameters.ShowExpectedValueLocal;
            }
            if(screentype == "atShowGlobalResults")
            {
                App.ActiveProject.PipeParameters.ShowExpectedValueGlobal = showvalue;
                IsShowValue = App.ActiveProject.PipeParameters.ShowExpectedValueGlobal;
            }
            return IsShowValue;
        }

        [WebMethod(EnableSession = true)]
        public static bool GetShowExpectedValue(string screenType)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var showExpectedValue = false;

            if (screenType == "atShowLocalResults")
            {
                showExpectedValue = App.ActiveProject.PipeParameters.ShowExpectedValueLocal;
            }
            else if (screenType == "atShowGlobalResults")
            {
                showExpectedValue = App.ActiveProject.PipeParameters.ShowExpectedValueGlobal;
            }

            return showExpectedValue;
        }

        private static bool IsEvaluation(clsAction Action)
        {
            return Action != null && Action.isEvaluation;
        }

        //survey
        [WebMethod(EnableSession = true)]
        public static string saveRespondentAnswers(int step, string[][] RespondentAnswers)
        {
            step = (step != -1) ? step : 1;
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return "";
            }

            var app = (clsComparionCore)context.Session["App"];
            var NeedtoRebuildPipe = false; // rebuild for surveys with radio buttons or something maybe
            var answers = AnytimeClass.ReadPageAnswers(ref NeedtoRebuildPipe, step, RespondentAnswers);

            if (NeedtoRebuildPipe)
            {
                NotRecreatePipe = false;
                CheckProject();
                app.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = false;
                foreach(ECCore.Groups.clsCombinedGroup cg in app.ActiveProject.ProjectManager.CombinedGroups.GroupsList)
                {
                    cg.ApplyRules();
                }
                app.ActiveProject.SaveStructure("Save User Group");
                app.ActiveProject.ProjectManager.PipeBuilder.CreatePipe();
                return AnytimeClass.get_StepInformation(app);

            }


            return answers;
        }



        //local results matrix table
        #region Judgment Table
        [WebMethod(EnableSession = true)]
        public static void updateAllinMatrix(double[][] pairsData, int parent)
        {
            for (int i = 0; i < pairsData.Length; i++)
            {
                string[] content = pairsData[i].Select(x => x.ToString()).ToArray();
                doMatrixOperation(pairsData[i][1].ToString(), 1, content, parent, false);
            }
        }

        [WebMethod(EnableSession = true)]
        public static object doMatrixOperation(string judgment, int ID, string[] content, int parent, bool invert, int rmode = 1)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return null;
            }

            var App = (clsComparionCore)context.Session["App"];
            var _with1 = App.ActiveProject.ProjectManager;
            int iStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
            clsAction ps = (clsAction)_with1.Pipe[iStep - 1];
            ECTypes.clsUser AHPUser = _with1.GetUserByEMail(App.ActiveUser.UserEMail);
            clsShowLocalResultsActionData psLocal = (clsShowLocalResultsActionData)AnytimeClass.GetAction(iStep).ActionData;
            int AHPUserID = AHPUser.UserID;
            OperationID operationID = (OperationID)ID;
            int ParentID = -1;
            AnytimeClass.JudgmentsSaved = true;
            var Model = (DataModel) context.Session[Constants.SessionModel];

            if (AnytimeClass.CombinedUserID != ECTypes.COMBINED_USER_ID)
            {
                App.ActiveProject.ProjectManager.StorageManager.Reader.LoadUserJudgments(App.ActiveProject.ProjectManager.GetUserByID(AnytimeClass.CombinedUserID));
            }

            if (operationID == OperationID.oJudgmentUpdate)
            {
                int Obj1ID = Convert.ToInt32(content[2]);
                int Obj2ID = Convert.ToInt32(content[3]);
                int Advantage = Convert.ToInt32(content[4]);

                if (invert)
                {
                    Advantage *= -1;
                }

                var tValue = Convert.ToDouble(judgment);
                ParentID = parent;
                clsNode node = App.ActiveProject.HierarchyObjectives.GetNodeByID(ParentID);

                if (node != null)
                {
                    Advantage = tValue == 0 ? 0 : (Advantage == 0 ? 1 : Advantage);
                    var isUndefined = tValue == 0;

                    var pwData = ((clsPairwiseJudgments)node.Judgments).PairwiseJudgment(Obj1ID, Obj2ID, AHPUserID);
                    if (pwData == null)
                    {
                        pwData = new clsPairwiseMeasureData(Obj1ID, Obj2ID, Advantage, tValue, ParentID, AHPUserID, isUndefined);
                    }

                    pwData.Value = tValue;
                    pwData.Advantage = Advantage;
                    pwData.IsUndefined = isUndefined;

                    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(node, pwData);
                    //node.Judgments.AddMeasureData(new clsPairwiseMeasureData(Obj1ID, Obj2ID, Advantage, tValue, ParentID, AHPUserID, isUndefined));
                    //App.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(AHPUserID);

                    for (int i = 0; i < iStep; i++)
                    {
                        var action = App.ActiveProject.ProjectManager.Pipe[i];
                        switch (action.ActionType)
                        {
                            case ActionType.atAllPairwise:
                                clsAllPairwiseEvaluationActionData pwd2 = (clsAllPairwiseEvaluationActionData)action.ActionData;
                                if (pwd2.Judgments != null)
                                {
                                    var selectedJudgment = pwd2.Judgments.FirstOrDefault(j => j.FirstNodeID == Obj1ID && j.SecondNodeID == Obj2ID && j.ParentNodeID == ParentID);
                                    if (selectedJudgment != null)
                                    {
                                        selectedJudgment.Value = tValue;
                                        selectedJudgment.Advantage = Advantage;
                                        selectedJudgment.IsUndefined = isUndefined;
                                    }
                                }
                                break;
                            case ActionType.atPairwise:
                                var pairwiseData = (clsPairwiseMeasureData)action.ActionData;
                                if (pairwiseData != null && pairwiseData.FirstNodeID == Obj1ID && pairwiseData.SecondNodeID == Obj2ID && pairwiseData.ParentNodeID == ParentID)
                                {
                                    pairwiseData.Value = tValue;
                                    pairwiseData.Advantage = Advantage;
                                    pairwiseData.IsUndefined = isUndefined;
                                }
                                break;
                        }
                    }
                }

                if(context.Session["PairsData"] == null)
                    context.Session["PairsData"] = Model.StepPairs;
                if (context.Session["ObjData"] == null)
                    context.Session["ObjData"] = Model.ObjectivesDataSorted;
                var pairsdata = (List<StepsPairs>) context.Session["PairsData"];
                var objsdata = (List<Objective>)context.Session["ObjData"];
                var pairResult = "";
                var objsResult = "";
                var inconsistencyRatio = 0.00;
                var normalization = (int) context.Session["normalization"];

                HttpContext.Current.Session[InconsistencySortingEnabled] = false;
                var results = AnytimeClass.CreateLocalResults(iStep, normalization);
                //var newPairsData = Model.StepPairs;
                //var newObjsData = Model.ObjectivesDataSorted;
                //for (int i = 0; i < objsdata.Count; i++)
                //{
                //    for(int j = 0; j< newObjsData.Count; j++)
                //    {
                //        if(objsdata[i].ID == newObjsData[j].ID)
                //        {
                //            objsResult += Convert.ToString((!string.IsNullOrEmpty(objsResult) ? "," : "")) + $"[{newObjsData[j].ID},\"{newObjsData[j].Value}\",\"{StringFuncs.JS_SafeString(newObjsData[j].Name)}\",\"{StringFuncs.Double2String(newObjsData[j].Value * 100, 2, true)}\"]";
                //        }
                        
                //    }
                //}
                //objsResult = "[" + objsResult + "]";
                var output = new
                {
                    results = results,
                    ObjID = Obj1ID + "" + Obj2ID
                    //ObjectivesData = objsResult
                };
                return output;
            }



            //restore judgments
            if (operationID == OperationID.oRestoreJudgments)
            {
                context.Session["ObjData"] = null;
                context.Session["PairsData"] = null;
                ParentID = parent;
                AnytimeClass.RestoreJudgments(ParentID);

                var output = new
                {
                    PipeParameters = AnytimeClass.CreateLocalResults(iStep)
                };

                return output;
            }


            //Invert All Judgments
            if (operationID == OperationID.oInvertAllJudgments)
            {
                context.Session["ObjData"] = null;
                context.Session["PairsData"] = null;
                ParentID = parent;
                clsNode node = default(clsNode);
                if (((clsShowLocalResultsActionData)ps.ActionData).PWOutcomesNode != null)
                {
                    node = ((clsShowLocalResultsActionData)ps.ActionData).PWOutcomesNode;
                }
                else
                {
                    node = App.ActiveProject.HierarchyObjectives.GetNodeByID(ParentID);
                }
                if (node != null)
                {
                    var pairwiseMeasureDataList = new List<clsPairwiseMeasureData>();

                    if (((clsShowLocalResultsActionData)ps.ActionData).PWOutcomesNode != null)
                    {
                        foreach (clsPairwiseMeasureData judgement in node.PWOutcomesJudgments.get_JudgmentsFromUser(AHPUserID))
                        {
                            if (!judgement.IsUndefined)
                            {
                                judgement.Advantage = -judgement.Advantage;
                                pairwiseMeasureDataList.Add(judgement);
                            }
                        }
                    }
                    else
                    {
                        foreach (clsPairwiseMeasureData judgement in node.Judgments.get_JudgmentsFromUser(AHPUserID))
                        {
                            if (!judgement.IsUndefined)
                            {
                                judgement.Advantage = -judgement.Advantage;
                                pairwiseMeasureDataList.Add(judgement);
                            }
                        }
                    }
                    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(AHPUserID);

                    foreach (clsAction action in App.ActiveProject.ProjectManager.Pipe)
                    {
                        switch (action.ActionType)
                        {
                            case ActionType.atAllPairwise:
                            case ActionType.atAllPairwiseOutcomes:
                                clsAllPairwiseEvaluationActionData pwd2 = (clsAllPairwiseEvaluationActionData)action.ActionData;
                                if (pwd2.Judgments != null)
                                {
                                    foreach (var selectedJudgment in pwd2.Judgments)
                                    {
                                        if (!selectedJudgment.IsUndefined)
                                        {
                                            var changedJudgment = pairwiseMeasureDataList.FirstOrDefault(j =>
                                                j.FirstNodeID == selectedJudgment.FirstNodeID && j.SecondNodeID == selectedJudgment.SecondNodeID &&
                                                j.ParentNodeID == selectedJudgment.ParentNodeID && j.Advantage != selectedJudgment.Advantage);

                                            if (changedJudgment != null)
                                            {
                                                selectedJudgment.Advantage = -selectedJudgment.Advantage;
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                var output = new
                {
                    PipeParameters = AnytimeClass.CreateLocalResults(iStep)
                };

                return output;

            }

            return null;
        }

        [WebMethod(EnableSession = true)]
        public static object saveOBPriority(bool chb)
        {
            var context = HttpContext.Current;
            context.Session[InconsistencySortingEnabled] = chb;

            if (!chb)
            {
                //when false it will sort by original order
                context.Session[InconsistencySortingOrder] = new List<int>();
            }

            var App = (clsComparionCore)context.Session["App"];
            int step = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
            var result = AnytimeClass.CreateLocalResults(step); //case 11440, case 11505  
            context.Session["ObjData"] = null;
            context.Session["PairsData"] = null;

            return result;
        }

        [WebMethod(EnableSession = true)]
        public static void savebestfit(bool chb)
        {
            HttpContext.Current.Session[BestFit] = chb;
        }

        #endregion


        [WebMethod(EnableSession = true)]
        public static object initializeSensitivity()
        {
            
            return new SensitivitiesAnalysis().initSA();
        }

        [WebMethod(EnableSession = true)]
        public static void saveSort(int sortmode, int sortwhere)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var App = (clsComparionCore)context.Session["App"];
            switch (sortwhere)
            {
                case 1:
                    App.ActiveProject.PipeParameters.LocalResultsSortMode = (CanvasTypes.ResultsSortMode) sortmode;
                    break;
                case 2:
                    App.ActiveProject.PipeParameters.GlobalResultsSortMode = (CanvasTypes.ResultsSortMode)sortmode;
                    break;
            }
        }

        [WebMethod(EnableSession = true)]
        public static bool showResultsIndex(bool showIndex, bool global)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            if(global)
                App.ActiveProject.ProjectManager.Parameters.ResultsGlobalShowIndex = showIndex;
            else
                App.ActiveProject.ProjectManager.Parameters.ResultsLocalShowIndex = showIndex;

            App.ActiveProject.ProjectManager.Parameters.Save();
            return showIndex;
        }

        [WebMethod(EnableSession = true)]
        public static string loadStepList(int first, int last)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var steps = AnytimeClass.get_StepInformation(App, -1, first, last); //retrieve step information
            return steps;
        }

        [WebMethod(EnableSession = true)]
        public static object loadHierarchy() {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var hierarchies = GeckoClass.NodeList(App.ActiveProject.HierarchyObjectives.GetLevelNodes(0), AnytimeClass.GetAction(CurrentStep));
            var output = new Dictionary<string, object>()
            {
                {"success",  true},
                {"data", hierarchies}
            };
            return output;
        }

        [WebMethod(EnableSession = true)]
        public static string testfunction(string text)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            return TeamTimeClass.ResString(text);
        }

        [WebMethod(EnableSession = true)]
        public static string getResource(int step)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var content = "";
            if (App.ActiveProject != null)
            {
                var action = AnytimeClass.GetAction(step);
                content = GeckoClass.GetPipeStepHint(action, null);
            }
            return content;
        }

        [WebMethod(EnableSession = true)]
        public static object checkObject(string objectName)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var content = "";
            //if (App.ActiveProject != null)
            //{
            //    var action = AnytimeClass.GetAction(step);
            //    content = GeckoClass.GetPipeStepHint(action, null);
            //}
            return JsonConvert.SerializeObject(App.ActiveWorkgroup.WordingTemplates, Formatting.Indented,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            }); 
        }

        [WebMethod(EnableSession = true)]
        public static string GetParticipantsComments(int step)
        {
            //return "";//Mike you can uncomment this one if its not working on your end 
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var userList = app.ActiveProject.ProjectManager.UsersList;
            var action = AnytimeClass.GetAction(step);
            var userComments = "";
            var parentNode = new clsNode();
            var firstNode = new clsNode();
            var secondNode = new clsNode();
            switch (action.ActionType)
            {
                case ActionType.atPairwise:
                case ActionType.atPairwiseOutcomes:
                    {
                        var pwData = (clsPairwiseMeasureData)action.ActionData;
                        parentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNodeID);
                        if (parentNode.IsTerminalNode)
                        {
                            firstNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(pwData.FirstNodeID);
                            secondNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(pwData.SecondNodeID);
                        }
                        else
                        {
                            firstNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.FirstNodeID);
                            secondNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.SecondNodeID);
                        }

                        var UserListComments = "";
                        foreach(ECTypes.clsUser user in userList)
                        {
                            var userData = ((clsPairwiseJudgments)parentNode.Judgments).PairwiseJudgment(firstNode.NodeID, secondNode.NodeID, user.UserID);
                            if(userData!= null)
                            {

                                if(userData.Comment != "")
                                {
                                    var u_comments = userData.Comment.Split(';');

                                    foreach (string comment in u_comments)
                                    {
                                        var jsonData = $"{{ \"email\" : \"{user.UserEMail}\" , \"name\" : \"{user.UserName}\" , \"comment\" : \"{comment}\"  }}";

                                        UserListComments += (UserListComments.Equals("") ? "" : ",") + jsonData;
                                    }
                                }
                            }
                        }
                        userComments = "[" +  UserListComments + "]";
                    }
                    break;

                case ActionType.atAllPairwise:
                case ActionType.atAllPairwiseOutcomes:
                    {
                        var pwData = (clsAllPairwiseEvaluationActionData)action.ActionData;
                        parentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNode.NodeID);
                        
                        foreach (clsPairwiseMeasureData tJud in pwData.Judgments)
                        {
                            if (parentNode.IsTerminalNode)
                            {
                                firstNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.FirstNodeID);
                                secondNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.SecondNodeID);
                            }
                            else
                            {
                                firstNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.FirstNodeID);
                                secondNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.SecondNodeID);
                            }

                            var UserListComments = "";
                            foreach (ECTypes.clsUser user in userList)
                            {
                                var userData = ((clsPairwiseJudgments)parentNode.Judgments).PairwiseJudgment(firstNode.NodeID, secondNode.NodeID, user.UserID);
                                if (userData != null)
                                {
                                    if (userData.Comment != "")
                                    {
                                        var u_comments = userData.Comment.Split(';');

                                        foreach (string comment in u_comments)
                                        {
                                            var jsonData = $"{{ \"email\" : \"{user.UserEMail}\" , \"name\" : \"{user.UserName}\" , \"comment\" : \"{comment}\"  }}";

                                            UserListComments += (UserListComments.Equals("") ? "" : ",") + jsonData;
                                        }
                                    }
                                }
                            }
                            userComments += (userComments.Equals("") ? "" : ",") + "[" + UserListComments + "]";
                        }
                        userComments = "[" + userComments + "]";
                    }
                    break;

                case ActionType.atNonPWOneAtATime:
                    {
                        var actiondata =  (clsOneAtATimeEvaluationActionData)action.ActionData;

                        switch (((clsNonPairwiseEvaluationActionData)action.ActionData).MeasurementType)
                        {
                            case ECMeasureType.mtRatings:
                                {
                                    var nonpwdata = (clsNonPairwiseMeasureData)actiondata.Judgment;
                                    if (actiondata.Node.IsAlternative)
                                    {
                                        parentNode = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy).Nodes[0];
                                        firstNode = actiondata.Node;
                                    }
                                    else
                                    {
                                        parentNode = actiondata.Node;
                                        if (parentNode.IsTerminalNode)
                                            firstNode = app.ActiveProject.ProjectManager.get_AltsHierarchy(app.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(nonpwdata.NodeID);
                                        else
                                        {
                                            firstNode = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy).GetNodeByID(nonpwdata.NodeID);
                                        }
                                    }

                                    var UserListComments = "";
                                    foreach (ECTypes.clsUser user in userList)
                                    {
                                        if (actiondata.Node.IsAlternative)
                                        {
                                            nonpwdata = ((clsNonPairwiseJudgments)firstNode.DirectJudgmentsForNoCause).GetJudgement(firstNode.NodeID, parentNode.NodeID, user.UserID);
                                        }
                                        else
                                        {
                                            nonpwdata = ((clsNonPairwiseJudgments)parentNode.Judgments).GetJudgement(firstNode.NodeID, parentNode.NodeID, user.UserID);
                                        }
                                        
                                        if (nonpwdata != null)
                                        {

                                            if(nonpwdata.Comment != "")
                                            {
                                                var jsonData = $"{{ \"email\" : \"{user.UserEMail}\" , \"name\" : \"{user.UserName}\" , \"comment\" : \"{nonpwdata.Comment}\"  }}";
                                                UserListComments += (UserListComments.Equals("") ? "" : ",") + jsonData;
                                            }
                                           
                                        }
                                    }
                                    userComments = "[" + UserListComments + "]";
                                }
                                break;
                            case ECMeasureType.mtDirect:
                                {
                                    var nonpwdata = (clsDirectMeasureData)actiondata.Judgment;
                                    if (actiondata.Node.IsAlternative)
                                    {
                                        parentNode = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy).Nodes[0];
                                        firstNode = actiondata.Node;
                                    }
                                    else
                                    {
                                        parentNode = actiondata.Node;
                                        if (parentNode.IsTerminalNode)
                                            firstNode = app.ActiveProject.ProjectManager.get_AltsHierarchy(app.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(nonpwdata.NodeID);
                                        else
                                        {
                                            firstNode = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy).GetNodeByID(nonpwdata.NodeID);
                                        }
                                    }

                                    var UserListComments = "";
                                    foreach (ECTypes.clsUser user in userList)
                                    {
                                        var userdata = (clsNonPairwiseMeasureData)actiondata.Judgment;
                                        if (actiondata.Node.IsAlternative)
                                        {
                                            userdata = ((clsNonPairwiseJudgments)firstNode.DirectJudgmentsForNoCause).GetJudgement(firstNode.NodeID, parentNode.NodeID, user.UserID);
                                        }
                                        else
                                        {
                                            userdata = ((clsNonPairwiseJudgments)parentNode.Judgments).GetJudgement(firstNode.NodeID, parentNode.NodeID, user.UserID);
                                        }

                                        if (userdata != null)
                                        {
                                            if (userdata.Comment != "")
                                            {
                                                var u_comments = userdata.Comment.Split(';');

                                                foreach (string comment in u_comments)
                                                {
                                                    var jsonData = $"{{ \"email\" : \"{user.UserEMail}\" , \"name\" : \"{user.UserName}\" , \"comment\" : \"{comment}\"  }}";

                                                    UserListComments += (UserListComments.Equals("") ? "" : ",") + jsonData;
                                                }
                                            }
                                        }
                                    }
                                    userComments = "[" + UserListComments + "]";
                                }
                                break;
                            case ECMeasureType.mtStep:
                                {
                                    var nonpwdata = (clsStepMeasureData)actiondata.Judgment;
                                    if (actiondata.Node.IsAlternative)
                                    {
                                        parentNode = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy).Nodes[0];
                                        firstNode = actiondata.Node;
                                    }
                                    else
                                    {
                                        parentNode = actiondata.Node;
                                        if (parentNode.IsTerminalNode)
                                            firstNode = app.ActiveProject.ProjectManager.get_AltsHierarchy(app.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(nonpwdata.NodeID);
                                        else
                                        {
                                            firstNode = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy).GetNodeByID(nonpwdata.NodeID);
                                        }
                                    }

                                    var UserListComments = "";
                                    foreach (ECTypes.clsUser user in userList)
                                    {
                                        var userdata = (clsNonPairwiseMeasureData)actiondata.Judgment;
                                        if (actiondata.Node.IsAlternative)
                                        {
                                            userdata = ((clsNonPairwiseJudgments)firstNode.DirectJudgmentsForNoCause).GetJudgement(firstNode.NodeID, parentNode.NodeID, user.UserID);
                                        }
                                        else
                                        {
                                            userdata = ((clsNonPairwiseJudgments)parentNode.Judgments).GetJudgement(firstNode.NodeID, parentNode.NodeID, user.UserID);
                                        }

                                        if (userdata != null)
                                        {
                                            if (userdata.Comment != "")
                                            {
                                                var u_comments = userdata.Comment.Split(';');

                                                foreach (string comment in u_comments)
                                                {
                                                    var jsonData = $"{{ \"email\" : \"{user.UserEMail}\" , \"name\" : \"{user.UserName}\" , \"comment\" : \"{comment}\"  }}";

                                                    UserListComments += (UserListComments.Equals("") ? "" : ",") + jsonData;
                                                }
                                            }
                                        }
                                    }
                                    userComments = "[" + UserListComments + "]";
                                }
                                break;
                            case ECMeasureType.mtRegularUtilityCurve:
                                {
                                    var nonpwdata = (clsUtilityCurveMeasureData)actiondata.Judgment;
                                    if (actiondata.Node.IsAlternative)
                                    {
                                        parentNode = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy).Nodes[0];
                                        firstNode = actiondata.Node;
                                    }
                                    else
                                    {
                                        parentNode = actiondata.Node;
                                        if (parentNode.IsTerminalNode)
                                            firstNode = app.ActiveProject.ProjectManager.get_AltsHierarchy(app.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(nonpwdata.NodeID);
                                        else
                                        {
                                            firstNode = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy).GetNodeByID(nonpwdata.NodeID);
                                        }
                                    }

                                    var UserListComments = "";
                                    foreach (ECTypes.clsUser user in userList)
                                    {
                                        var userdata = (clsNonPairwiseMeasureData)actiondata.Judgment;
                                        if (actiondata.Node.IsAlternative)
                                        {
                                            userdata = ((clsNonPairwiseJudgments)firstNode.DirectJudgmentsForNoCause).GetJudgement(firstNode.NodeID, parentNode.NodeID, user.UserID);
                                        }
                                        else
                                        {
                                            userdata = ((clsNonPairwiseJudgments)parentNode.Judgments).GetJudgement(firstNode.NodeID, parentNode.NodeID, user.UserID);
                                        }

                                        if (userdata != null)
                                        {
                                            if (userdata.Comment != "")
                                            {
                                                var u_comments = userdata.Comment.Split(';');

                                                foreach (string comment in u_comments)
                                                {
                                                    var jsonData = $"{{ \"email\" : \"{user.UserEMail}\" , \"name\" : \"{user.UserName}\" , \"comment\" : \"{comment}\"  }}";

                                                    UserListComments += (UserListComments.Equals("") ? "" : ",") + jsonData;
                                                }
                                            }
                                        }
                                    }
                                    userComments = "[" + UserListComments + "]";
                                }
                                break;
                        }
                    }
                    break;
                case ActionType.atNonPWAllChildren:
                case ActionType.atNonPWAllCovObjs:
                    {
                        clsNonPairwiseEvaluationActionData now_pw_all = (clsNonPairwiseEvaluationActionData)action.ActionData;
                        switch (now_pw_all.MeasurementType)
                        {
                            case ECMeasureType.mtRatings:
                            case ECMeasureType.mtDirect:
                                {
                                    if (now_pw_all is clsAllChildrenEvaluationActionData)
                                    {
                                        var nonpwdata = (clsAllChildrenEvaluationActionData)action.ActionData;
                                        foreach (clsNode tAlt in nonpwdata.Children)
                                        {
                                            parentNode = nonpwdata.ParentNode;

                                            var UserListComments = "";
                                            foreach (ECTypes.clsUser user in userList)
                                            {
                                                var userdata = ((clsNonPairwiseJudgments)parentNode.Judgments).GetJudgement(tAlt.NodeID, parentNode.NodeID, user.UserID);
                                                //if (parentNode.IsAlternative)
                                                //{
                                                //    userdata = ((clsNonPairwiseJudgments)firstNode.DirectJudgmentsForNoCause).GetJudgement(tAlt.NodeID, parentNode.NodeID, user.UserID);
                                                //}
                                                //else
                                                //{
                                                //    userdata = ((clsNonPairwiseJudgments)parentNode.Judgments).GetJudgement(tAlt.NodeID, parentNode.NodeID, user.UserID);
                                                //}

                                                if (userdata != null)
                                                {
                                                    if(userdata.Comment != "")
                                                    {
                                                        var u_comments = userdata.Comment.Split(';');

                                                        foreach (string comment in u_comments)
                                                        {
                                                            var jsonData = $"{{ \"email\" : \"{user.UserEMail}\" , \"name\" : \"{user.UserName}\" , \"comment\" : \"{comment}\"  }}";

                                                            UserListComments += (UserListComments.Equals("") ? "" : ",") + jsonData;
                                                        }
                                                    }
                                                  
                                                }
                                            }
                                            userComments += (userComments.Equals("") ? "" : ",") + "[" + UserListComments + "]";
                                        }
                                    }
                                    if(now_pw_all is clsAllCoveringObjectivesEvaluationActionData)
                                    {
                                        var nonpwdata = (clsAllCoveringObjectivesEvaluationActionData)action.ActionData;
                                        foreach (clsNode tAlt in nonpwdata.CoveringObjectives)
                                        {
                                            parentNode = tAlt;

                                            var UserListComments = "";
                                            foreach (ECTypes.clsUser user in userList)
                                            {
                                                var userdata = ((clsNonPairwiseJudgments)parentNode.Judgments).GetJudgement(nonpwdata.Alternative.NodeID, parentNode.NodeID, user.UserID);
                                                //if (parentNode.IsAlternative)
                                                //{
                                                //    userdata = ((clsNonPairwiseJudgments)firstNode.DirectJudgmentsForNoCause).GetJudgement(nonpwdata.Alternative, parentNode.NodeID, user.UserID);
                                                //}
                                                //else
                                                //{
                                                //    userdata = ((clsNonPairwiseJudgments)parentNode.Judgments).GetJudgement(nonpwdata.Alternative, parentNode.NodeID, user.UserID);
                                                //}

                                                if (userdata != null)
                                                {
                                                    if (userdata.Comment != "")
                                                    {
                                                        var u_comments = userdata.Comment.Split(';');

                                                        foreach (string comment in u_comments)
                                                        {
                                                            var jsonData = $"{{ \"email\" : \"{user.UserEMail}\" , \"name\" : \"{user.UserName}\" , \"comment\" : \"{comment}\"  }}";

                                                            UserListComments += (UserListComments.Equals("") ? "" : ",") + jsonData;
                                                        }
                                                    }
                                                }
                                            }
                                            userComments += (userComments.Equals("") ? "" : ",") + "[" + UserListComments + "]";
                                        }
                                    }
                                    userComments = "[" + userComments + "]";
                                }
                                break;
                        }
                    }
                    break;
            }
            return userComments;
        }

        [WebMethod(EnableSession = true)]
        public static void HideEqualMessage(int step, bool allCluster)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var actionData = AnytimeClass.Action(step).ActionData as clsShowLocalResultsActionData;
            var clusterMessage = (List<object[]>)context.Session[Constants.Sess_ShowEqualOnce];
            if (allCluster)
            {

                var clusterIndex = -1;
                for (int i = 0; i < clusterMessage.Count; i++)
                {
                    clusterMessage[i][1] = true;
                }
                
            }
            else
            {
                for (int i = 0; i < clusterMessage.Count; i++)
                {
                    if ((int)clusterMessage[i][0] == actionData.ParentNode.NodeID)
                    {
                        if (!(bool)clusterMessage[i][1])
                        {
                            clusterMessage[i][1] = true;
                        }

                    }
                }
            }
            context.Session[Constants.Sess_ShowEqualOnce] = clusterMessage;
        }


        [WebMethod(EnableSession = true)]
        public static string redirectToComparionTeamtime()
        {
            var app = (clsComparionCore)HttpContext.Current.Session["App"];
            
            var hash = GeckoClass.CreateLogonURL(app.ActiveUser, app.ActiveProject, true, "", "");
            var url = HttpContext.Current.Request.Url;

            var link = url.Host.Equals("localhost") ? $"{url.Scheme}://{url.Host}:20180/{hash}" : $"{url.Scheme}://{url.Authority}/{hash}";

            return link.Replace("//r.", "//").Replace("//r-", "//");
        }

        [WebMethod(EnableSession = true)]
        public static void setMultiBarsMode(bool value)
        {
            var context = HttpContext.Current;
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return;
            }

            var App = (clsComparionCore)context.Session["App"];
            if (App != null)
            {
                bool isPM = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup);
                if (isPM)
                {

                    App.ActiveProject.ProjectManager.Parameters.EvalCollapseMultiPWBars = value;
                    App.ActiveProject.SaveProjectOptions("Save eval collapse bars mode");
                }
            }
        }

        private void InitializeSessions(clsComparionCore app)
        {
            if (app.ActiveProject != null)
            {
                if (Session[SessionIsFirstTime + app.ProjectID] == null)
                {
                    Session[SessionIsFirstTime + app.ProjectID] = true;
                }

                if (Session[SessionAutoAdvanceJudgmentsCount + app.ProjectID] == null)
                {
                    Session[SessionAutoAdvanceJudgmentsCount + app.ProjectID] = 0;
                }
                //else if (!IsPostBack)
                //{
                //    Session[SessionAutoAdvanceJudgmentsCount + app.ProjectID] = (int)Session[SessionAutoAdvanceJudgmentsCount + app.ProjectID] - 1;
                //}

                if (Session[SessionAutoAdvance + app.ProjectID] == null)
                {
                    Session[SessionAutoAdvance + app.ProjectID] = app.ActiveProject.PipeParameters.AllowAutoadvance;
                }

                if (Session[SessionIsJudgmentAlreadySaved] == null)
                {
                    Session[SessionIsJudgmentAlreadySaved] = false;
                }
            }
        }

        private static void IncreaseAutoAdvanceJudgmentsCount(clsComparionCore app, clsAction action)
        {
            int judgmentsCount = (int)HttpContext.Current.Session[SessionAutoAdvanceJudgmentsCount + app.ProjectID];
            bool isAutoAdvance = (bool)HttpContext.Current.Session[SessionAutoAdvance + app.ProjectID];

            if (IsEvaluation(action) && !isAutoAdvance && judgmentsCount >= 0
                    && (action.ActionType == ActionType.atPairwise || action.ActionType == ActionType.atNonPWOneAtATime))
            {
                HttpContext.Current.Session[SessionAutoAdvanceJudgmentsCount + app.ProjectID] = judgmentsCount + 1;
            }

            HttpContext.Current.Session[SessionIncreaseJudgmentsCount] = false;
        }

        private static bool ShowAutoAdvanceModal(clsComparionCore app, clsAction action, string pairwiseType, string nonPwType)
        {
            int judgmentsCount = (int)HttpContext.Current.Session[SessionAutoAdvanceJudgmentsCount + app.ProjectID];
            bool isAutoAdvance = (bool)HttpContext.Current.Session[SessionAutoAdvance + app.ProjectID];

            bool showAutoAdvance = IsEvaluation(action) && !isAutoAdvance && AutoAdvanceMaxJudgments == judgmentsCount
                && ((action.ActionType == ActionType.atPairwise && pairwiseType != "ptGraphical")
                || (action.ActionType == ActionType.atNonPWOneAtATime && nonPwType != "mtDirect"))
                && !(bool)HttpContext.Current.Session[SessionIsJudgmentAlreadySaved]
                && !app.ActiveProject.ProjectManager.Parameters.EvalNoAskAutoAdvance;

            if ((HttpContext.Current.Session[SessionIncreaseJudgmentsCount] != null && (bool)HttpContext.Current.Session[SessionIncreaseJudgmentsCount]) || showAutoAdvance)
            {
                IncreaseAutoAdvanceJudgmentsCount(app, action);
            }

            return showAutoAdvance;
        }


        [WebMethod(EnableSession = true)]
        public static object getUserControl(int step)
        {
            HttpContext context = HttpContext.Current;
            var action = AnytimeClass.GetAction(step);
            var userControlUrl = "";
            var cookie = context.Request.Cookies["loadedScreens"] ?? new HttpCookie("loadedScreens");
            bool isLocal = HttpContext.Current.Request.IsLocal; // if local dont return false to prevent rendering changes in html
            //if (isLocal)
            //{
                cookie.Values.Add("pairwise", "0");
                cookie.Values.Add("multiPairwise", "0");
                cookie.Values.Add("direct", "0");
                cookie.Values.Add("multiDirect", "0");
                cookie.Values.Add("ratings", "0");
                cookie.Values.Add("multiRatings", "0");
                cookie.Values.Add("stepFunction", "0");
                cookie.Values.Add("utility", "0");
                cookie.Values.Add("localResults", "0");
                cookie.Values.Add("globalResults", "0");
                cookie.Values.Add("survey", "0");
                cookie.Values.Add("sensitivity", "0");
                context.Response.AppendCookie(cookie);
            //}
            switch (action.ActionType)
            {
                case ActionType.atPairwise:
                case ActionType.atPairwiseOutcomes:
                    {
                        if (cookie.Values["pairwise"] == "1")
                            return false;
                        cookie.Values["pairwise"] = "1";
                        userControlUrl = "~/pages/anytime/Pairwise.ascx";
                    }
                    break;
                case ActionType.atAllPairwise:
                case ActionType.atAllPairwiseOutcomes:
                    {
                        if (cookie.Values["multiPairwise"] == "1")
                            return false;
                        cookie.Values["multiPairwise"] = "1";
                        userControlUrl = "~/Pages/Anytime/MultiPairwise.ascx";
                    }
                    break;
                case ActionType.atNonPWOneAtATime:
                    {
                        var measuretype = (clsNonPairwiseEvaluationActionData)action.ActionData;
                        switch (((clsNonPairwiseEvaluationActionData)action.ActionData).MeasurementType)
                        {
                            case ECMeasureType.mtDirect:
                                if (cookie.Values["direct"] == "1")
                                    return false;
                                cookie.Values["direct"] = "1";
                                userControlUrl = "~/Pages/Anytime/DirectComparison.ascx";
                                break;
                            case ECMeasureType.mtRatings:
                                if (cookie.Values["ratings"] == "1")
                                    return false;
                                cookie.Values["ratings"] = "1";
                                userControlUrl = "~/Pages/Anytime/Ratings.ascx";
                                break;
                            case ECMeasureType.mtStep:
                                if (cookie.Values["stepFunction"] == "1")
                                    return false;
                                cookie.Values["stepFunction"] = "1";
                                userControlUrl = "~/Pages/Anytime/StepFunction.ascx";
                                break;
                            case ECMeasureType.mtRegularUtilityCurve:
                            case ECMeasureType.mtCustomUtilityCurve:
                                if (cookie.Values["utility"] == "1")
                                    return false;
                                cookie.Values["utility"] = "1";
                                userControlUrl = "~/Pages/Anytime/UtilityCurve.ascx";
                                break;
                        }
                    }
                    break;
                case ActionType.atNonPWAllChildren:
                case ActionType.atNonPWAllCovObjs:
                    {
                        var measuretype = (clsNonPairwiseEvaluationActionData)action.ActionData;
                        switch (((clsNonPairwiseEvaluationActionData)action.ActionData).MeasurementType)
                        {
                            case ECMeasureType.mtDirect:
                                if (cookie.Values["multiDirect"] == "1")
                                    return false;
                                cookie.Values["multiDirect"] = "1";
                                userControlUrl = "~/Pages/Anytime/MultiDirect.ascx";
                                break;
                            case ECMeasureType.mtRatings:
                                if (cookie.Values["multiRatings"] == "1")
                                    return false;
                                cookie.Values["multiRatings"] = "1";
                                userControlUrl = "~/Pages/Anytime/MultiRatings.ascx";
                                break;
                        }
                    }
                    break;
                case ActionType.atSpyronSurvey:
                case ActionType.atSurvey:
                    if (context.Request.Cookies["loadedScreens"]["survey"] == "1")
                        return false;
                    context.Response.Cookies["loadedScreens"]["survey"] = "1";
                    userControlUrl = "~/pages/anytime/Survey.ascx";
                    break;
                case ActionType.atSensitivityAnalysis:
                    if (context.Request.Cookies["loadedScreens"]["sensitivity"] == "1")
                        return false;
                    context.Response.Cookies["loadedScreens"]["sensitivity"] = "1";
                    userControlUrl = "~/pages/anytime/SensitivitiesAnalysis.ascx";
                    break;
                case ActionType.atShowLocalResults:
                    if (cookie.Values["localResults"] == "1")
                        return false;
                    cookie.Values["localResults"] = "1";
                    
                    userControlUrl = "~/pages/anytime/localresults.ascx";
                    break;
                case ActionType.atShowGlobalResults:
                    if (cookie.Values["globalResults"] == "1")
                        return false;
                    cookie.Values["globalResults"] = "1";
                    userControlUrl = "~/Pages/Anytime/GlobalResults.ascx";
                    break;
            }
            context.Response.AppendCookie(cookie);
            using (Page objPage = new Page())
            {
                UserControl uControl = (UserControl)objPage.LoadControl(userControlUrl);
                objPage.Controls.Add(uControl);
                using (System.IO.StringWriter sWriter = new System.IO.StringWriter())
                {
                    context.Server.Execute(objPage, sWriter, false);
                    return sWriter.ToString();

                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static object changeNormalization(int normalization, int step, bool fGlobal, int wrtNodeID)
        {

            if (fGlobal)
            {
                return AnytimeClass.CreateGlobalResults(step, normalization, wrtNodeID);
            }
            else
            {
                return AnytimeClass.CreateLocalResults(step, normalization);
            }
        }

        [WebMethod(EnableSession = true)]
        public static object pasteClipBoardData(string clipBoardData, bool sameElements)
        {
            var output = new Dictionary<string, object>()
            {
                {"success",  false},
                {"data", ""}
            };

            var isPipeViewOnly = (bool) HttpContext.Current.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                output = new Dictionary<string, object>()
                {
                    {"success",  true},
                    {"data", AnytimeClass.CreateLocalResults(CurrentStep)}
                };

                return output;
            }

            var pass = AnytimeClass.GetResultsPipeStepData(clipBoardData, sameElements);
            AnytimeClass.JudgmentsSaved = true;

            if (pass)
            {
                output = new Dictionary<string, object>()
                {
                    {"success",  true},
                    {"data", AnytimeClass.CreateLocalResults(CurrentStep)}
                };
            }
            else
            {
                output["data"] = "request failed"; //error
            }
            return output;

        }

        [WebMethod(EnableSession = true)]
        public static string Ajax_Callback(string data)
        {
            NameValueCollection args = HttpUtility.ParseQueryString(data);
            string sAction = Common.GetParam(args, ExpertChoice.Web.Options._PARAM_ACTION).Trim().ToLower();
            string sResult = Convert.ToString((string.IsNullOrEmpty(sAction) ? "" : sAction));

            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var sensitivitiesAnalysis = new SensitivitiesAnalysis();

            switch (sAction)
            {

                case "node":
                    int tNodeID = -1;
                    if (int.TryParse(Common.GetParam(args, "node_id").ToLower(), out tNodeID))
                    {
                        sensitivitiesAnalysis.ProjectManager = App.ActiveProject.ProjectManager;
                        sensitivitiesAnalysis.SetSaUserId(App);
                        sensitivitiesAnalysis.CurrentNode = sensitivitiesAnalysis.ProjectManager.get_Hierarchy(sensitivitiesAnalysis.ProjectManager.ActiveHierarchy).GetNodeByID(tNodeID);
                        sensitivitiesAnalysis.ObjPriorities.Clear();
                        sensitivitiesAnalysis.AltValues.Clear();
                        sensitivitiesAnalysis.AltValuesInOne.Clear();
                        sensitivitiesAnalysis.ProjectManager.CalculationsManager.InitializeSAGradient(sensitivitiesAnalysis.CurrentNode.NodeID, false, sensitivitiesAnalysis.SAUserID, ref sensitivitiesAnalysis.ObjPriorities, ref sensitivitiesAnalysis.AltValues, ref sensitivitiesAnalysis.AltValuesInOne, 0);
                        sensitivitiesAnalysis.AltValuesInZero = sensitivitiesAnalysis.ProjectManager.CalculationsManager.GetGradientData(sensitivitiesAnalysis.CurrentNode.NodeID, false, sensitivitiesAnalysis.SAUserID, sensitivitiesAnalysis.ObjPriorities, 0);
                        // D3473
                        sResult = sensitivitiesAnalysis.GetSAData();
                    }

                    break;
                case "normalization":
                    int tID = -1;
                    if (int.TryParse(Common.GetParam(args, "norm_mode").ToLower(), out tID))
                    {
                        sensitivitiesAnalysis.NormalizationMode = (AlternativeNormalizationOptions)tID;
                        sResult = sensitivitiesAnalysis.GetSAData();
                    }
                    break;
                case SensitivitiesAnalysis.ACTION_DSA_UPDATE_VALUES:
                    string s_values = Common.GetParam(args, "values").Trim();
                    string[] values = s_values.Split(Convert.ToChar(","));
                    string s_ids = Common.GetParam(args, "objids").Trim();
                    string[] ids = s_ids.Split(Convert.ToChar(","));
                    Dictionary<int, double> ANewObjPriorities = new Dictionary<int, double>();
                    dynamic i = 0;
                    foreach (string objID_loopVariable in ids)
                    {
                        var objID = objID_loopVariable;
                        double APrty = 0;
                        Double.TryParse(values[i], out APrty);
                        ANewObjPriorities.Add(Convert.ToInt32(objID), APrty);
                        i += 1;
                    }
                    sensitivitiesAnalysis.updateAltValuesinZero(ANewObjPriorities);
                    string ZeroValuesString = "";
                    foreach (KeyValuePair<int, Dictionary<int, double>> Objitem_loopVariable in sensitivitiesAnalysis.AltValuesInZero)
                    {
                        var Objitem = Objitem_loopVariable;
                        string ZeroAltValuesString = "";
                        foreach (KeyValuePair<int, double> AltItem_loopVariable in Objitem.Value)
                        {
                            var AltItem = AltItem_loopVariable;
                            ZeroAltValuesString += Convert.ToString((!string.IsNullOrEmpty(ZeroAltValuesString) ? "," : "")) + string.Format("{{altID:{0},val:{1}}}", AltItem.Key, StringFuncs.JS_SafeNumber(AltItem.Value));
                        }
                        ZeroValuesString += Convert.ToString((!string.IsNullOrEmpty(ZeroValuesString) ? "," : "")) + string.Format("[{0},[{1}]]", Objitem.Key, ZeroAltValuesString);
                    }
                    sResult = string.Format("[[{0}]]", ZeroValuesString);
                    break;
            }

            if (!string.IsNullOrEmpty(sResult))
            {

                return sResult;
            }
            return "";

        }

        public static int CurrentStep
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var App = (clsComparionCore)context.Session["App"];
                return App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact);
            }
        }


        [WebMethod(EnableSession = true)]
        public static void HideBrowserWarningMessage()
        {
            HttpContext.Current.Response.Cookies["HideWarningMessage"].Value = "1";
        }

        private static clsUserWorkgroup Uw
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var app = (clsComparionCore)context.Session["App"];
                return app.ActiveUserWorkgroup;
            }
        }

        private static clsWorkspace Ws
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var app = (clsComparionCore)context.Session["App"];
                return app.ActiveWorkspace;
            }
        }

        [WebMethod(EnableSession = true)]
        public static bool SetAutoFitInfoDocImages(bool value)
        {
            var context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return App.ActiveProject.ProjectManager.Parameters.AutoFitInfoDocImages;
            }

            App.ActiveProject.ProjectManager.Parameters.AutoFitInfoDocImages = value;
            App.ActiveProject.ProjectManager.Parameters.Save();
            return value;
        }

        [WebMethod(EnableSession = true)]
        public static bool SetShowFramedInfodocsMobile(bool value)
        {
            var context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                return App.ActiveProject.ProjectManager.Parameters.ShowFramedInfodocsMobile;
            }

            App.ActiveProject.ProjectManager.Parameters.ShowFramedInfodocsMobile = value;
            App.ActiveProject.ProjectManager.Parameters.Save();
            return value;
        }

        [WebMethod(EnableSession = true)]
        public static object GetAllSinglePairwiseInfoDocs()
        {
            var app = (clsComparionCore)HttpContext.Current.Session["App"];
            var allInfoDocs = new List<object>();

            if (app.HasActiveProject() && app.ActiveProject.isValidDBVersion)
            {
                var totalSteps = app.ActiveProject.Pipe.Count;
                var doCheck = true;

                for (int i = 1; i <= totalSteps; i++)
                {
                    var action = (clsAction)AnytimeClass.GetAction(i);
                    if (action.ActionType == ActionType.atPairwise && doCheck)
                    {
                        var pairwiseData = (clsPairwiseMeasureData)action.ActionData;
                        var parentNode = (clsNode)app.ActiveProject.HierarchyObjectives.GetNodeByID(pairwiseData.ParentNodeID);
                        var pairwiseType = app.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(parentNode);

                        if (pairwiseType == CanvasTypes.PairwiseType.ptVerbal)
                        {
                            clsNode firstNode = null;
                            clsNode secondNode = null;

                            if (parentNode.IsTerminalNode)
                            {
                                firstNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(pairwiseData.FirstNodeID);
                                secondNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(pairwiseData.SecondNodeID);
                            }
                            else
                            {
                                firstNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pairwiseData.FirstNodeID);
                                secondNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pairwiseData.SecondNodeID);
                            }

                            //var parentNodeInfoDoc = (string)InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.Node, parentNode.NodeID.ToString(), parentNode.InfoDoc, true, true, -1);
                            var firstNodeInfoDoc = (string)InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, firstNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, firstNode.NodeID.ToString(), firstNode.InfoDoc, true, true, -1);
                            var secondNodeInfoDoc = (string)InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, secondNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, secondNode.NodeID.ToString(), secondNode.InfoDoc, true, true, -1);
                            //var wrtFirstNodeInfoDoc = (string)InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, firstNode.NodeID.ToString(), app.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(firstNode.NodeGuidID, parentNode.NodeGuidID), true, true, parentNode.NodeID);
                            //var wrtSecondNodeInfoDoc = (string)InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, secondNode.NodeID.ToString(), app.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(secondNode.NodeGuidID, parentNode.NodeGuidID), true, true, parentNode.NodeID);

                            if (string.IsNullOrEmpty(firstNodeInfoDoc) || string.IsNullOrEmpty(secondNodeInfoDoc) || !firstNodeInfoDoc.Contains("<img") || !secondNodeInfoDoc.Contains("<img"))
                            {
                                allInfoDocs = new List<object>();
                                doCheck = false;
                            }
                            else
                            {
                                HtmlDocument infoDoc = new HtmlDocument();
                                infoDoc.LoadHtml(firstNodeInfoDoc);
                                List<HtmlNode> firstInfoDocImages = infoDoc.DocumentNode.SelectNodes("//img").ToList();   //selecting image elements

                                infoDoc.LoadHtml(secondNodeInfoDoc);
                                List<HtmlNode> secondInfoDocImages = infoDoc.DocumentNode.SelectNodes("//img").ToList();   //selecting image elements

                                if (firstInfoDocImages.Count == 1 && secondInfoDocImages.Count == 1)
                                {
                                    allInfoDocs.Add(new
                                    {
                                        InfodocLeft = firstNodeInfoDoc,
                                        InfodocRight = secondNodeInfoDoc
                                    });
                                }
                                else
                                {
                                    allInfoDocs = new List<object>();
                                    doCheck = false;
                                }
                            }
                        }
                        else
                        {
                            if (allInfoDocs.Count == 0) continue;
                            break;
                        }
                    }
                    else if (action.ActionType != ActionType.atPairwise)
                    {
                        doCheck = true;
                        if (allInfoDocs.Count == 0) continue;
                        break;
                    }
                }
            }

            return allInfoDocs;
        }

        [WebMethod(EnableSession = true)]
        public static string CheckProjectLockStatus()
        {
            var context = HttpContext.Current;
            var sessionName = "IsCheckingProjectLockStatusFirstTime";
            var isFirstTime = context.Session[sessionName] == null || (bool)context.Session[sessionName];

            if (!isFirstTime)
            {
                System.Threading.Thread.Sleep(1000);
            }

            var app = (clsComparionCore)context.Session["App"];
            var isProjectLocked = true;
            var lockedMessage = string.Empty;
            var redirectUrl = string.Empty;
            context.Session[sessionName] = false;

            if (app.ActiveProject != null)
            {
                var project = app.DBProjectByID(app.ProjectID);
                isProjectLocked = !(project.LockInfo.LockerUserID == app.ActiveUser.UserID || project.LockInfo.LockStatus == ECLockStatus.lsUnLocked);
                lockedMessage = isProjectLocked ? string.Format(TeamTimeClass.ResString("msgEvaluationLocked"), project.ProjectName) : string.Empty;

                if (project.isTeamTime)
                {
                    //Get to Comparion rerirect URL when TeamTime session started
                    redirectUrl = redirectToComparionTeamtime();
                    lockedMessage = TeamTimeClass.ResString("msgTeamTimeRedirection");
                    lockedMessage = string.IsNullOrEmpty(lockedMessage)
                        ? "Anytime Evaluation is not available while TeamTime™ is in progress. You will be redirected to TeamTime™ Evaluation in {0} seconds."
                        : lockedMessage;

                    lockedMessage = string.Format(lockedMessage, 5);
                }
            }

            var isPipeViewOnly = (bool)context.Session[Constants.SessionIsPipeViewOnly];

            if (isPipeViewOnly)
            {
                isProjectLocked = false;
                lockedMessage = "";
            }

            var lockedInfo = new
            {
                status = isProjectLocked,
                message = lockedMessage,
                teamTimeUrl = redirectUrl
            };

            return new JavaScriptSerializer().Serialize(lockedInfo);
        }

        private void CheckForNextProjectAndRedirectIfRequired(clsComparionCore app)
        {
            if (app.ActiveProject == null || app.ActiveProject.isTeamTime) return;

            var context = HttpContext.Current;
            var currentStep = app.ActiveWorkspace.get_ProjectStep(app.ActiveProject.isImpact);
            var isFirstTime = context.Session[SessionIsFirstTime + app.ProjectID] == null || (bool)context.Session[SessionIsFirstTime + app.ProjectID];

            if (app.ActiveProject.Pipe.Count != currentStep || isFirstTime) return;

            var nextProject = AnytimeClass.GetNextProject(app.ActiveProject);
            if (app.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish && nextProject != null && !nextProject.isMarkedAsDeleted)
            {
                var url = context.Request.Url;
                var redirectUrl = url.Host.Equals("localhost") ? $"{url.Scheme}://{url.Host}:9793/" : $"{url.Scheme}://{url.Authority}/";
                redirectUrl = GeckoClass.CreateLogonURL(app.ActiveUser, nextProject, false, "step=1", redirectUrl, "");

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    context.Response.Redirect(redirectUrl);
                }
            }
        }
    }
}