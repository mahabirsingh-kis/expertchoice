using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using ECCore;
using Canvas;
using ExpertChoice.Data;
using ExpertChoice.Service;
using SpyronControls.Spyron.Core;
using ExpertChoice.Results;
using ECCore.MiscFuncs;
using ExpertChoice.Web;
using Newtonsoft.Json;
using Options = ExpertChoice.Service.Options;

namespace AnytimeComparion.Pages.external_classes
{
    public class AnytimeClass
    {
        //private clsAction _action;
        //private bool _dataInstanceStatusChecked = false;
        //private bool _dataInstance;
//        private bool _readOnlyStatusChecked = false;
//        private bool _readOnly = false;

        //matrix table
        public const string SessJudgments = "Judgments{0}";
        private const string SessDatetime = "JudgmentsDateTime";
        private const string SessSaved = "JudgmentsSaved";
        private const string SessParentId = "JudgmentsNodeID";
        private const string MimeIdentifier = "MIME";

        private const string InconsistencySortingEnabled = "InconsistencySortingEnabled";
        private const string SessionShowUnassessed = "SessionShowUnassessed";
        private const string SessionObjectivesHighestResult = "SessionObjectivesHighestResult";

        //private clsUserWorkgroup _uw;
        //private clsWorkspace _ws;

        public enum EcHierarchyId 
        {
            HidLikelihood = 0,
            HidImpact = 2
        };
        
        /**methods from Comparion Anytime**/
        public static void SetUser(ECTypes.clsUser tUser, bool fCheckDataInstance, bool fRecreatePipe)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            if (fCheckDataInstance && IsDataInstance && app.ActiveProject.ProjectManager.DataInstances.Count > 0)
            {
                var di = app.ActiveProject.ProjectManager.DataInstances[0];
                tUser = di.User;
            }

            if (tUser != null && (app.ActiveProject.ProjectManager.User != null || (app.ActiveProject.ProjectManager.User != null && tUser.UserID != app.ActiveProject.ProjectManager.User.UserID)))
            {
                app.ActiveProject.ProjectManager.User = tUser;
                app.ActiveProject.LastModify = DateTime.Today;
            }

            bool fForcePipeRebuild = false;
            bool IsIntensities = false; //hardcoded for now
            if (app.ActiveProject.HierarchyObjectives.HierarchyType == ECTypes.ECHierarchyType.htMeasure &&
                !IsIntensities)
            {
                app.ActiveProject.ProjectManager.ActiveHierarchy = app.ActiveProject.isImpact
                    ? (int)ECTypes.ECHierarchyID.hidImpact
                    : (int)ECTypes.ECHierarchyID.hidLikelihood;
            }
            app.ActiveProject.ProjectManager.ActiveHierarchy = app.ActiveProject.isImpact
                ? (int)ECTypes.ECHierarchyID.hidImpact
                : (int)ECTypes.ECHierarchyID.hidLikelihood;
            if (fRecreatePipe){
                if(app.ActiveProject.ProjectManager.ActiveHierarchy == 2 && app.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet != app.ActiveProject.ProjectManager.PipeParameters.ImpactParameterSet){
                       app.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = app.ActiveProject.ProjectManager.PipeParameters.ImpactParameterSet;
                }

                if(app.ActiveProject.ProjectManager.ActiveHierarchy == 0 && app.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet != app.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet){
                       app.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = app.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet;
                }
            }

            if (app.ActiveProject.ProjectManager.User != null)
            {
                //var fUpdateUser = false;
                //var fUpdateWorkspace = false;
                //var UserWS = (clsWorkspace) app.ActiveWorkspace;
            }


            if(!UserIsReadOnly() && fRecreatePipe) //fRecreatePipe = !isCallBack && !IsPostBack 
            {
                app.CheckAndAssignUserRole(app.ActiveProject, app.ActiveUser.UserEMail);
            }

           // fForcePipeRebuild = true;
            if (fRecreatePipe || fForcePipeRebuild)
            {
                app.SurveysManager.ActiveWorkgroupID = app.ActiveWorkgroup.ID;
                app.ActiveSurveysList = null;
                if (app.isSpyronAvailable)
                {
                    app.ActiveProject.ProjectManager.PipeBuilder.GetSurveyInfo = app.SurveysManager.GetSurveyStepsCountByGUID;
                }
                app.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = false;
                if (fForcePipeRebuild)
                {
                    app.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = false;
                }
                
                app.ActiveProject.ProjectManager.PipeBuilder.CreatePipe();
            }

        }

        //method from comparion
        public static bool IsDataInstance
        {
            get {
                return false;
            }
        }

        public static clsUserWorkgroup Uw
        {
            get
            {
                clsUserWorkgroup _uw;

                HttpContext context = HttpContext.Current;
                var app = (clsComparionCore)context.Session["App"];
                var tUserId = app.ActiveUser.UserID;
                if (UserIsReadOnly())
                {
                    //set ReadOnlyUser here
                    tUserId = GetReadOnlyUserID();
                }

                _uw = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUserId, app.ActiveWorkgroup.ID, app.UserWorkgroups);
                if (_uw != null)
                {
                    // TODO: consult with AD on this, looks weird
                    _uw = app.DBUserWorkgroupByUserIDWorkgroupID(tUserId, app.ActiveWorkgroup.ID);
                }
                return _uw;
            }
        }

        public static clsWorkspace Ws
        {
            get
            {
                clsWorkspace _ws;
                HttpContext context = HttpContext.Current;
                var app = (clsComparionCore)context.Session["App"];
                var tUserId = app.ActiveUser.UserID;
                if (UserIsReadOnly())
                {
                    //set ReadOnlyUser here
                    tUserId = GetReadOnlyUserID();
                }

                _ws = clsWorkspace.WorkspaceByUserIDAndProjectID(tUserId, app.ProjectID, app.Workspaces);

                if (_ws == null)
                {
                    _ws = app.DBWorkspaceByUserIDProjectID(tUserId, app.ProjectID);
                    if (UserIsReadOnly())
                    {
                        app.Workspaces.Add(_ws);
                    }
                }

                // TODO: consult with AD here, why this is commented out
                if (_ws == null)
                {
                    //clsApplicationUser tUser = app.ActiveUser;
                    //Boolean fCanEdit = app.CanUserModifyProject(tUserId, app.ProjectID, Uw,
                    //    clsWorkspace.WorkspaceByUserIDAndProjectID(tUserId, app.ProjectID, app.Workspaces));
                    //_ws = app.AttachProject(tUser, app.ActiveProject, !fCanEdit, app.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, fCanEdit ? ecRoleGroupType.gtProjectManager : ecRoleGroupType.gtEvaluator), "", false);

                    // if (_WS == null && App.ApplicationError.Status == ecErrorStatus.errWrongLicense)
                    //{
                    //wrong license
                    // }

                }
                //}
                return _ws;
            }
        }

        public static clsAction GetAction(int stepNumber)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];

            if (!app.HasActiveProject() || stepNumber < 1 || stepNumber > app.ActiveProject.Pipe.Count || app.ActiveProject.Pipe.Count < 1)
            {
                return null;
            }
            else
            {
                var action = (clsAction) app.ActiveProject.Pipe[stepNumber - 1];
                return action;
            }
        }

        public static bool IsImpact
        {
            get
            {
                HttpContext context = HttpContext.Current;
                var app = (clsComparionCore)context.Session["App"];
                return app.ActiveProject.ProjectManager.ActiveHierarchy == (int)EcHierarchyId.HidImpact;
            }
        }

        public static bool UserIsReadOnly()
        {
            var context = HttpContext.Current;
            return (bool) context.Session[Constants.SessionIsPipeViewOnly];
        }

        public static int GetReadOnlyUserID()
        {
            var context = HttpContext.Current;
            return (int) context.Session[Constants.SessionViewOnlyUserId];
        }

        public static clsAction Action(int step)
        {
            clsAction _action = null;

            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];

            if (step > 0 && step <= app.ActiveProject.Pipe.Count)
            {
                _action = GetAction(step);
            }
            else
            {
                var tUserId = app.ActiveUser.UserID;

                if (UserIsReadOnly())
                {
                    //set ReadOnlyUser here
                    var tUser = app.DBUserByID(GetReadOnlyUserID());
                    tUserId = tUser.UserID;
                }

                //var ws = clsWorkspace.WorkspaceByUserIDAndProjectID(tUserId, app.ProjectID, app.Workspaces);
                var anytimeUser = app.ActiveProject.ProjectManager.GetUserByEMail(app.ActiveUser.UserEMail);
                SetUser(anytimeUser, true, true);
                var workSpace = app.DBWorkspaceByUserIDProjectID(anytimeUser.UserID, app.ProjectID);
                var currentStep = workSpace.get_ProjectStep(app.ActiveProject.isImpact);
                if (currentStep > 0)
                {
                    _action = GetAction(currentStep);
                }
                else
                {
                    _action = GetAction(1);
                }

            }

            return _action;
        }

        public static List<object> Getnodes(int step)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var project = app.ActiveProject;
            clsAction tAction = GetAction(step);
            var parentNode = (clsNode)null;
            var firstNode = (clsNode)null;
            var secondNode = (clsNode)null;
            var objHierarchy = (clsHierarchy)project.ProjectManager.get_Hierarchy(project.ProjectManager.ActiveHierarchy);
            var altsHierarchy = (clsHierarchy)project.ProjectManager.get_AltsHierarchy(project.ProjectManager.ActiveAltsHierarchy);
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
                clsNode childNode;
                switch (tAction.ActionType)
                {
                    case ActionType.atInformationPage:
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        break;
                    case ActionType.atPairwise:
                        var actionData = (clsPairwiseMeasureData)tAction.ActionData;
                        parentNode = objHierarchy.GetNodeByID(actionData.ParentNodeID);
                        if (parentNode != null)
                        {
                            if (parentNode.IsTerminalNode)
                            {
                                firstNode = altsHierarchy.GetNodeByID(actionData.FirstNodeID);
                                secondNode = altsHierarchy.GetNodeByID(actionData.SecondNodeID);
                            }
                            else
                            {
                                firstNode = objHierarchy.GetNodeByID(actionData.FirstNodeID);
                                secondNode = objHierarchy.GetNodeByID(actionData.SecondNodeID);
                            }
                        }

                        list1.Add(leftnodeinfo);
                        list1.Add(rightnodeinfo);
                        list1.Add(wrtleft);
                        list1.Add(wrtright);
                        list1.Add(firstNode);
                        list1.Add(secondNode);
                        list1.Add(parentNode);
                        list1.Add(ParentNodeInfo);
                        break;

                    case ActionType.atAllPairwise:
                    case ActionType.atAllPairwiseOutcomes:

                        if (tAction.ActionData is clsAllPairwiseEvaluationActionData)
                        {
                            clsAllPairwiseEvaluationActionData allPwData = (clsAllPairwiseEvaluationActionData)tAction.ActionData;
                            parentNode = objHierarchy.GetNodeByID(allPwData.ParentNode.NodeID);
                        }
                       

                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add(parentNode);
                        list1.Add("");
                        list1.Add(parentNode);
                        list1.Add("");
                        break;
                    case ActionType.atShowLocalResults:
                        var localresultsdata = (clsShowLocalResultsActionData)tAction.ActionData;


                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add(localresultsdata.ParentNode);
                        list1.Add("");
                        list1.Add(localresultsdata.ParentNode);
                        list1.Add("");
                        break;
                    case ActionType.atShowGlobalResults:
                        var Nodes = objHierarchy.Nodes;

                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add("");
                        list1.Add(Nodes[0]);
                        list1.Add("");
                        list1.Add(Nodes[0]);
                        list1.Add("");
                        break;
                    case ActionType.atNonPWOneAtATime:
                        var actionData2 = (clsOneAtATimeEvaluationActionData)tAction.ActionData;

                        if (actionData2.Node != null && actionData2.Judgment != null)
                        {
                            switch (((clsNonPairwiseEvaluationActionData)tAction.ActionData).MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                    clsNonPairwiseMeasureData nonpwdata = (clsNonPairwiseMeasureData)actionData2.Judgment;
                                    if (actionData2.Node.IsAlternative)
                                    {
                                        parentNode = objHierarchy.Nodes[0];
                                        childNode = actionData2.Node;
                                    }
                                    else
                                    {
                                        parentNode = actionData2.Node;
                                        if (parentNode != null && parentNode.IsTerminalNode) { childNode = altsHierarchy.GetNodeByID(nonpwdata.NodeID); } else { childNode = objHierarchy.GetNodeByID(nonpwdata.NodeID); }
                                    }
                                    //childinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ChildNode.NodeID.ToString(), ChildNode.InfoDoc, isTeamTimeOwner, true, -1);
                                    //parentinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, isTeamTimeOwner, true, -1);
                                    //wrtinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID), isTeamTimeOwner, true, ParentNode.NodeID);
                                    list1.Add(childinfo);
                                    list1.Add(parentinfo);
                                    list1.Add(wrtinfo);
                                    list1.Add("");
                                    list1.Add(parentNode);
                                    list1.Add(childNode);
                                    list1.Add(parentNode);
                                    list1.Add("");

                                    break;
                                //leftnodeinfo == (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Convert.ToInt32((tAlt.IsAlternative ? reObjectType.Alternative : reObjectType.Node)), tAlt.NodeID.ToString(), tAlt.InfoDoc, true, true, -1);

                                case ECMeasureType.mtDirect:
                                    parentNode = actionData2.Node;
                                    var directData = (clsDirectMeasureData)actionData2.Judgment;
                                    childNode = parentNode.IsTerminalNode ? altsHierarchy.GetNodeByID(directData.NodeID) : objHierarchy.GetNodeByID(directData.NodeID);
                                    //childinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ChildNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ChildNode.NodeID.ToString(), ChildNode.InfoDoc, isTeamTimeOwner, true, -1);
                                    //parentinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ParentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, isTeamTimeOwner, true, -1);
                                    //wrtinfo = (string)InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID), isTeamTimeOwner, true, ParentNode.NodeID);
                                    list1.Add(childinfo);
                                    list1.Add(parentinfo);
                                    list1.Add(wrtinfo);
                                    list1.Add("");
                                    list1.Add(parentNode);
                                    list1.Add(childNode);
                                    list1.Add(parentNode);
                                    list1.Add("");
                                    break;

                                case ECMeasureType.mtStep:
                                    parentNode = actionData2.Node;
                                    var stepData = (clsStepMeasureData)actionData2.Judgment;
                                    childNode = parentNode.IsTerminalNode ? altsHierarchy.GetNodeByID(stepData.NodeID) : objHierarchy.GetNodeByID(stepData.NodeID);
                                    childinfo = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, childNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, childNode.NodeID.ToString(), childNode.InfoDoc, true, false, -1);
                                    parentinfo = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, parentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, parentNode.NodeID.ToString(), parentNode.InfoDoc, true, false, -1);
                                    wrtinfo = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, childNode.NodeID.ToString(), app.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(childNode.NodeGuidID, parentNode.NodeGuidID), false, true, parentNode.NodeID);
                                    list1.Add(childinfo);
                                    list1.Add(parentinfo);
                                    list1.Add(wrtinfo);
                                    list1.Add("");
                                    list1.Add(parentNode);
                                    list1.Add(childNode);
                                    list1.Add(parentNode);
                                    list1.Add("");
                                    break;

                                case ECMeasureType.mtRegularUtilityCurve:
                                    parentNode = actionData2.Node;
                                    var ucData = (clsUtilityCurveMeasureData)actionData2.Judgment;
                                    childNode = parentNode.IsTerminalNode ? altsHierarchy.GetNodeByID(ucData.NodeID) : objHierarchy.GetNodeByID(ucData.NodeID);
                                    childinfo = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, childNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, childNode.NodeID.ToString(), childNode.InfoDoc, true, false, -1);
                                    parentinfo = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, parentNode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node, parentNode.NodeID.ToString(), parentNode.InfoDoc, true, false, -1);
                                    wrtinfo = InfodocService.Infodoc_Unpack(app.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, childNode.NodeID.ToString(), app.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(childNode.NodeGuidID, parentNode.NodeGuidID), false, true, parentNode.NodeID);
                                    list1.Add(childinfo);
                                    list1.Add(parentinfo);
                                    list1.Add(wrtinfo);
                                    list1.Add("");
                                    list1.Add(parentNode);
                                    list1.Add(childNode);
                                    list1.Add(parentNode);
                                    list1.Add("");
                                    break;
                            }

                        }
                        break;

                    case ActionType.atNonPWAllChildren:
                    case ActionType.atNonPWAllCovObjs:
                        clsNonPairwiseEvaluationActionData nowPwAll = (clsNonPairwiseEvaluationActionData)tAction.ActionData;

                        switch (nowPwAll.MeasurementType)
                        {    
                            case ECMeasureType.mtRatings: 
                            
                                 if (tAction.ActionData is clsAllChildrenEvaluationActionData)
                                {
                                    clsAllChildrenEvaluationActionData data = (clsAllChildrenEvaluationActionData)tAction.ActionData;
                                    parentNode = data.ParentNode;
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add(null);
                                    list1.Add(parentNode);
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                }

                                if (tAction.ActionData is clsAllCoveringObjectivesEvaluationActionData)
                                {
                                    clsAllCoveringObjectivesEvaluationActionData data = (clsAllCoveringObjectivesEvaluationActionData)tAction.ActionData;
                                    childNode = data.Alternative; 
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add(childNode);
                                    list1.Add(childNode);
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                }
                               
                                
                                break;
                            case ECMeasureType.mtDirect: 

                                if (tAction.ActionData is clsAllChildrenEvaluationActionData)
                                {
                                    clsAllChildrenEvaluationActionData data = (clsAllChildrenEvaluationActionData)tAction.ActionData;
                                    parentNode = data.ParentNode;
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add(parentNode);
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                }

                                if (tAction.ActionData is clsAllCoveringObjectivesEvaluationActionData)
                                {
                                    clsAllCoveringObjectivesEvaluationActionData data = (clsAllCoveringObjectivesEvaluationActionData)tAction.ActionData;
                                    childNode = data.Alternative;
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add(childNode);
                                    list1.Add(childNode);
                                    list1.Add("");
                                    list1.Add("");
                                    list1.Add("");

                                }
                                
                                break;

                            case ECMeasureType.mtStep:
                                clsAllChildrenEvaluationActionData multiNonPwData = (clsAllChildrenEvaluationActionData)tAction.ActionData;
                                parentNode = multiNonPwData.ParentNode;
                                list1.Add("");
                                list1.Add("");
                                list1.Add("");
                                list1.Add("");
                                list1.Add(parentNode);
                                list1.Add("");
                                list1.Add("");
                                list1.Add("");
                                break;

                            case ECMeasureType.mtRegularUtilityCurve:
                                clsAllChildrenEvaluationActionData multiNonPwData1 = (clsAllChildrenEvaluationActionData)tAction.ActionData;
                                parentNode = multiNonPwData1.ParentNode;
                                list1.Add("");
                                list1.Add("");
                                list1.Add("");
                                list1.Add("");
                                list1.Add(parentNode);
                                list1.Add("");
                                list1.Add("");
                                list1.Add("");
                                break;
                        }
                    break;
                }
            }
            return list1;
        }

        public static int GetPrecisionForRatings(clsRatingScale tScale)
        {
            int sPrec = 0;
            if (tScale != null)
            {
                foreach(clsRating tIntens in tScale.RatingSet){
                    int tLen = tIntens.Value.ToString(CultureInfo.InvariantCulture).Length - 4;
                    sPrec = tLen > sPrec ? tLen : sPrec;
                }
                sPrec = sPrec > 3 ? 3 : sPrec;
            }
            return sPrec;
        }

        public static bool ShowUnassessed
        {
            get
            {
                bool showUnassessed = HttpContext.Current.Session[SessionShowUnassessed] != null && (bool) HttpContext.Current.Session[SessionShowUnassessed];
                return showUnassessed;
            }
            set { HttpContext.Current.Session[SessionShowUnassessed] = value; }
        }

        public static string get_StepInformation(clsComparionCore app, int previousStep = -1, int first = 1, int last = -1)
        {
            var steps = "";
            var disableStep = false;
            if (previousStep > -1)
            {
                steps = GetStepData(previousStep - 1, steps);
            }
            else
            {
                ShowUnassessed = false;
                if (last <= 0)
                    last = app.ActiveProject.Pipe.Count;
                for (int i = first - 1; i < last; i++)
                {
                    steps = GetStepData(i, steps);
                    if (i < last - 1)
                        steps += ",";
                }
            }

            //steps+= "]";
            //0 - step content/step number if button, 
            //1 - font color,
            //2 - background
            //3 - undefined/step number if fstep

            return steps;
        }

        public static string GetStepData(int step, string steps, bool fButton = false)
        {
            var app = (clsComparionCore)HttpContext.Current.Session["App"];
            var action = AnytimeClass.GetAction(step + 1);
            var content = GeckoClass.GetPipeStepHint(action, null);
            var color = "";
            var background = "";
            var isUndefined = 0;
            switch (GetStepStatus(action))
            {
                case false:
                    {
                        switch (GetAction(step + 1).ActionType)
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
                                color = "#000";
                                break;
                        }
                    }
                    break;
                case true:
                    {
                        color = "#e74c3c";
                        if (!app.ActiveProject.PipeParameters.AllowMissingJudgments)
                        {
                            isUndefined = 1;
                        }
                        ShowUnassessed = true;
                    }
                    break;
            }

            //if (CurrentStep == i + 1)
            //{
            //    color = "white";
            //    background = "#c77e30";
            //}
            Regex rgx = new Regex("<.*?>");

            var stepinfo = "";
            if(fButton)
            {
                stepinfo = Convert.ToString(step + 1);
            }
            else
            {
                isUndefined = step + 1;
                stepinfo = String.Format(TeamTimeClass.ResString("btnEvaluationStepHint"), step + 1, rgx.Replace(content.Replace("'", "\""), " "));
            }
                
            steps += $"['{stepinfo}','{color}','{background}','{isUndefined}']";
            return steps;
        }

        public static bool IsUndefined(clsAction action)
        {
            bool undefined = false;
            switch (action.ActionType)
            {
                case ActionType.atPairwise:
                    clsPairwiseMeasureData pw = (clsPairwiseMeasureData)action.ActionData;
                    return pw.IsUndefined;
                case ActionType.atAllPairwise:
                case ActionType.atAllPairwiseOutcomes:
                    clsAllPairwiseEvaluationActionData allPw = (clsAllPairwiseEvaluationActionData)action.ActionData;
                    if (allPw.Judgments != null)
                    {
                        foreach (clsPairwiseMeasureData tPw in allPw.Judgments)
                        {
                            if (tPw.IsUndefined)
                            {
                                undefined = true;
                            }
                        }
                    }
                    return undefined;
                case ActionType.atNonPWOneAtATime:
                    clsOneAtATimeEvaluationActionData nonPwData = (clsOneAtATimeEvaluationActionData)action.ActionData;
                    if (nonPwData.Judgment != null)
                    {
                        clsNonPairwiseMeasureData nonPwJudgment = (clsNonPairwiseMeasureData)nonPwData.Judgment;
                        if (nonPwJudgment.IsUndefined)
                        {
                            undefined = true;
                        }
                    }
                    return undefined;
                case ActionType.atNonPWAllChildren:
                    clsAllChildrenEvaluationActionData nonAllPwData = (clsAllChildrenEvaluationActionData)action.ActionData;
                    if (nonAllPwData.Children != null)
                    {
                        foreach (clsNode tAlt in nonAllPwData.Children)
                        {
                            clsNonPairwiseMeasureData md = nonAllPwData.GetJudgment(tAlt);
                            if (md != null && md.IsUndefined)
                            {
                                undefined = true;
                            }
                        }
                    }
                    return undefined;
                case ActionType.atNonPWAllCovObjs:
                    clsAllCoveringObjectivesEvaluationActionData nonAllPwObjsData = (clsAllCoveringObjectivesEvaluationActionData)action.ActionData;
                    if (nonAllPwObjsData.CoveringObjectives != null)
                    {
                        foreach (clsNode tAlt in nonAllPwObjsData.CoveringObjectives)
                        {
                            clsNonPairwiseMeasureData md = nonAllPwObjsData.GetJudgment(tAlt);
                            if (md != null && md.IsUndefined)
                            {
                                undefined = true;
                            }
                        }
                    }
                    return undefined;
                case ActionType.atAllEventsWithNoSource:
                    clsAllEventsWithNoSourceEvaluationActionData allEventsData = (clsAllEventsWithNoSourceEvaluationActionData)action.ActionData;
                    if (allEventsData.Alternatives != null)
                    {
                        foreach (clsNode tAlt in allEventsData.Alternatives)
                        {
                            clsNonPairwiseMeasureData md = allEventsData.GetJudgment(tAlt);
                            if (md != null && md.IsUndefined)
                            {
                                undefined = true;
                            }
                        }
                    }
                    return undefined;
                case ActionType.atSpyronSurvey:
                    return false; //skipped
            }
            return false;
        }

        public static bool GetStepStatus(clsAction action)
        {
            HttpContext context = HttpContext.Current;
            //Return fStatus

            if (action != null && action.ActionData != null)
            {
                switch (action.ActionType)
                {
                    case ActionType.atPairwise:
                        {
                            clsPairwiseMeasureData actionData = (clsPairwiseMeasureData)action.ActionData;
                            if (actionData.IsUndefined)
                            {
                                return true;
                            }
                        }
                        break;
                    case ActionType.atPairwiseOutcomes:
                        {
                            clsPairwiseMeasureData actionData = (clsPairwiseMeasureData)action.ActionData;
                            if (actionData.IsUndefined)
                            {
                                return true;
                            }
                        }
                        break;
                    case ActionType.atAllPairwise:
                    case ActionType.atAllPairwiseOutcomes:
                        {
                            clsAllPairwiseEvaluationActionData allPwData = (clsAllPairwiseEvaluationActionData)action.ActionData;
                            foreach (clsPairwiseMeasureData tJud in allPwData.Judgments)
                            {
                                if (tJud.IsUndefined)
                                {
                                    return true;
                                }
                            }

                        }
                        break;
                    case ActionType.atNonPWOneAtATime:
                        {
                            clsOneAtATimeEvaluationActionData actionData = (clsOneAtATimeEvaluationActionData)action.ActionData;

                            // D2946
                            if (actionData.Node != null && actionData.Judgment != null)
                            {
                                switch (((clsNonPairwiseEvaluationActionData)action.ActionData).MeasurementType)
                                {
                                    case ECMeasureType.mtRatings:
                                        clsNonPairwiseMeasureData nonpwData = (clsNonPairwiseMeasureData)actionData.Judgment;
                                        if (nonpwData.IsUndefined)
                                        {
                                            return true;
                                        }
                                        break;
                                    case ECMeasureType.mtDirect:
                                        clsDirectMeasureData directData = (clsDirectMeasureData)actionData.Judgment;
                                        if (directData.IsUndefined)
                                        {
                                            return true;
                                        }
                                        break;
                                    case ECMeasureType.mtStep:
                                        clsStepMeasureData stepData = (clsStepMeasureData)actionData.Judgment;
                                        if (stepData.IsUndefined)
                                        {
                                            return true;
                                        }

                                        break;
                                    case ECMeasureType.mtRegularUtilityCurve:
                                        clsUtilityCurveMeasureData ucData = (clsUtilityCurveMeasureData)actionData.Judgment;
                                        if (ucData.IsUndefined)
                                        {
                                            return true;
                                        }

                                        break;
                                }
                            }
                        }
                        break;

                    case ActionType.atNonPWAllChildren:
                    case ActionType.atNonPWAllCovObjs:
                        {
                            clsNonPairwiseEvaluationActionData nowPwAll = (clsNonPairwiseEvaluationActionData)action.ActionData;
                            switch (nowPwAll.MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                    if (action.ActionData is clsAllChildrenEvaluationActionData)
                                    {
                                        clsAllChildrenEvaluationActionData multiNonPwData = (clsAllChildrenEvaluationActionData)action.ActionData;
                                        foreach (clsNode tAlt in multiNonPwData.Children)
                                        {
                                            clsRatingMeasureData r = (clsRatingMeasureData)multiNonPwData.GetJudgment(tAlt);
                                            if (r.IsUndefined)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    if (action.ActionData is clsAllCoveringObjectivesEvaluationActionData)
                                    {
                                        clsAllCoveringObjectivesEvaluationActionData multiNonPwData2 = (clsAllCoveringObjectivesEvaluationActionData)action.ActionData;
                                        foreach (clsNode tAlt in multiNonPwData2.CoveringObjectives)
                                        {
                                            clsRatingMeasureData r = (clsRatingMeasureData)multiNonPwData2.GetJudgment(tAlt);
                                            if (r.IsUndefined)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    break;
                                case ECMeasureType.mtDirect:
                                    if (action.ActionData is clsAllChildrenEvaluationActionData)
                                    {
                                        clsAllChildrenEvaluationActionData multiDirectData1 = (clsAllChildrenEvaluationActionData)action.ActionData;
                                        foreach (clsNode tAlt in multiDirectData1.Children)
                                        {
                                            clsDirectMeasureData dd = (clsDirectMeasureData)multiDirectData1.GetJudgment(tAlt);
                                            if (dd.IsUndefined)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    if (action.ActionData is clsAllCoveringObjectivesEvaluationActionData)
                                    {
                                        clsAllCoveringObjectivesEvaluationActionData multiDirectData2 = (clsAllCoveringObjectivesEvaluationActionData)action.ActionData;
                                        foreach (clsNode tAlt in multiDirectData2.CoveringObjectives)
                                        {
                                            clsDirectMeasureData dd = (clsDirectMeasureData)multiDirectData2.GetJudgment(tAlt);
                                            if (dd.IsUndefined)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    break;
                            }

                        }
                        break;
                }


            }
            return false;
        }

        private static bool CanShowCombinedResults(clsShowLocalResultsActionData data)
        {
            if (CombinedUserID == ECTypes.COMBINED_USER_ID)
            {
                return data.get_CanShowGroupResults();
            }
            else
            {
                try
                {
                    return data.CanShowResultsForUser(CombinedUserID);
                }
                catch
                {
                    return true;
                }

            }
        }

        public static int CombinedUserID
        {
            get
            {
                var context = HttpContext.Current;
                var app = (clsComparionCore)context.Session["App"];
                return app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedUID;
            }
        }

        //local results
        public static object CreateLocalResults(int step, int normalization = 1)
        {
            var context = HttpContext.Current;
            context.Session["normalization"] = normalization;
            var app = (clsComparionCore)context.Session["App"];
            var actionData = AnytimeClass.Action(step).ActionData as clsShowLocalResultsActionData;
            var currentNode = new object[3];
            var highestResult = 0.00;
            var messagenote = "";
            var showTitle = true;
            var resultsTitle = "";
            
            switch (app.ActiveProject.PipeParameters.LocalResultsView)
            {
                case CanvasTypes.ResultsView.rvIndividual:
                    showTitle = actionData.CanShowIndividualResults;
                    break;
                case CanvasTypes.ResultsView.rvGroup:
                    showTitle = CanShowCombinedResults(actionData);
                    break;
                default:
                    showTitle = actionData.CanShowIndividualResults || CanShowCombinedResults(actionData);
                    break;
            }

            currentNode[0] = actionData.ParentNode.NodeName;

            if (actionData.ParentNode.Children.Count > 0)
            {
                currentNode[1] = app.ActiveProject.PipeParameters.NameObjectives;
                currentNode[2] = true;
            }
            else
            {
                currentNode[1] = app.ActiveProject.PipeParameters.NameAlternatives;
                currentNode[2] = false;
            }

            app.ActiveProject.ProjectManager.CheckCustomCombined();
            var resultsData = Anytime.Anytime.GetIntermediateResultsData(normalization);
            foreach (var result in resultsData)
            {
                double individualResults = 0;
                double combinedResults = 0;
                Double.TryParse(result[2], out individualResults);
                if (individualResults > highestResult)
                {
                    highestResult = Convert.ToDouble(result[2]);
                }
                Double.TryParse(result[3], out combinedResults);
                if (combinedResults > highestResult)
                {
                    highestResult = Convert.ToDouble(result[3]);
                }
            }
            var action = ((clsAction)AnytimeClass.GetAction((int)step - 1));
            var stepPairsData = GetPairsData();
            var objectivesData = GetObjectivesData();
            bool inconsistencyRatioStatus;
            var with1 = app.ActiveProject.ProjectManager;
            var sEmail = app.ActiveUser.UserEMail;
            var ahpUser = with1.GetUserByEMail(sEmail);
            var ahpUserId = ahpUser.UserID;
            var defaultsort = (int)app.ActiveProject.PipeParameters.LocalResultsSortMode;

            var inconsistencyRatio = actionData.InconsistencyIndividual;
            var judgmentType = ((clsAction)AnytimeClass.GetAction((int)step - 1)).ActionType;
            var measurementType = "";
            var JudgmentSaved = false;
            var equalMessage = false;
            clsNode parentNode = new clsNode();
            var pos = 0;
            var neg = 0;
            var mid = 0;
            var totalJudgment = 0;
            var IsIntensities = false; //hard-coded

            if (context.Session[Constants.Sess_ShowEqualOnce] == null)
            {
                var ObjNodes = new List<object[]>();
                foreach(clsNode node in app.ActiveProject.HierarchyObjectives.Nodes)
                {
                    var cluster = new object[2];
                    cluster[0] = node.NodeID;
                    cluster[1] = false;
                    ObjNodes.Add(cluster);
                }
                context.Session[Constants.Sess_ShowEqualOnce] = ObjNodes;
            }

            var clusterMessage = (List<object[]>)context.Session[Constants.Sess_ShowEqualOnce];
            var ShowEqualOnce = false;
            var clusterIndex = -1;
            for(int i = 0; i < clusterMessage.Count; i++)
            {
                if ((int) clusterMessage[i][0] == actionData.ParentNode.NodeID)
                {
                    if (!(bool)clusterMessage[i][1])
                    {
                        ShowEqualOnce = true;
                        clusterIndex = i;
                    }

                }
            }
            switch (judgmentType)
            {
                case ActionType.atPairwise:
                case ActionType.atPairwiseOutcomes:
                {
                    var pwData = (clsPairwiseMeasureData)AnytimeClass.Action(step - 1).ActionData;
                    parentNode = (clsNode)app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNodeID);
                    measurementType = app.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(parentNode).ToString();
                    var sStep = app.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(actionData.ParentNode, step);
                    if(Action(sStep+1).ActionType == ActionType.atAllPairwise)
                    {
                        step -= 1;
                        goto case ActionType.atAllPairwise;
                    }
                    if (ShowEqualOnce)
                    {
                        for (int i = sStep + 1; i <= step - 1; i++)
                        {
                            var tVal = (clsPairwiseMeasureData)AnytimeClass.Action(i).ActionData;
                            if (!tVal.IsUndefined)
                            {
                                if (tVal.Advantage == 0)
                                {
                                    mid += 1;
                                }
                                else if (tVal.Advantage > 0)
                                {
                                    pos += 1;
                                }
                                else
                                {
                                    neg += 1;
                                }
                            }
                        }

                        totalJudgment = actionData.ParentNode.Judgments.get_JudgmentsFromUser(ahpUser.UserID).Count;
                        if ((pos + mid >= totalJudgment || neg + mid >= totalJudgment) && ((pos + mid >= 5) || (neg + mid >= 5)))
                        {
                            messagenote = TeamTimeClass.ResString("msgSameSideJudgments");
                            equalMessage = true;
                            //11265 below commented to apply option to hide/show this message
                            //clusterMessage[clusterIndex][1] = true;
                        }
                    }
                    goto default;
                }
                case ActionType.atAllPairwise:
                case ActionType.atAllPairwiseOutcomes:
                {
                    var sStep = app.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(actionData.ParentNode, step);
                    var pwData = (clsAllPairwiseEvaluationActionData)AnytimeClass.Action(sStep + 1).ActionData;
                    parentNode = (clsNode)app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNode.NodeID);
                    measurementType = app.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(parentNode).ToString();
                    if (sStep >= 0 && ShowEqualOnce)
                    {
                        foreach (clsPairwiseMeasureData tVal in pwData.Judgments)
                        {
                            if (!tVal.IsUndefined)
                            {
                                if (tVal.Advantage == 0)
                                {
                                    mid += 1;
                                }
                                else if (tVal.Advantage > 0)
                                {
                                    pos += 1;
                                }
                                else
                                {
                                    neg += 1;
                                }
                            }
                        }
                        if ((((pos + mid >= pwData.Judgments.Count) && neg < 1) || (neg + mid >= pwData.Judgments.Count) && pos < 1) && ((pos + mid >= 5) || (neg + mid >= 5)))
                        {
                            messagenote = TeamTimeClass.ResString("msgSameSideJudgments");
                            equalMessage = true;
                            // 11265 below commented to apply option to hide / show this message
                            //clusterMessage[clusterIndex][1] = true;
                        }
                    }
                    goto default;
                }
                default:
                    if (TeamTimeClass.get_SessVar(SessParentId) != parentNode.NodeID.ToString())
                        TeamTimeClass.set_SessVar(SessSaved, null);

                    TeamTimeClass.set_SessVar(SessParentId, parentNode.NodeID.ToString());

                    if (!JudgmentsSaved)
                    {
                        SaveJudgments(parentNode.NodeID);
                    }
                    JudgmentSaved = JudgmentsSaved;
                    break;
            }

            if (!IsIntensities && (pos == totalJudgment || neg == totalJudgment))
            {
                //   showTitle = false;
            }

            //  lblResultsTitle.Text = sTitle

            resultsTitle = GeckoClass.GetPipeStepHint(AnytimeClass.Action(step), null, true, true); //see Default

            if (UserIsReadOnly())
            {
                //set ReadOnlyUser here
                ahpUser = app.DBUserByID(GetReadOnlyUserID());
                ahpUserId = ahpUser.UserID;
                sEmail = ahpUser.UserEMail;
            }

           


            if (actionData.ShowConsistency &&
                (app.ActiveProject.PipeParameters.ShowConsistencyRatio & !actionData.OnlyMainDiagonalEvaluated()))
            {
                inconsistencyRatio =
                    Convert.ToSingle(ECTypes.IsCombinedUserID(ahpUserId)
                        ? actionData.InconsistencyCombined
                        : actionData.InconsistencyIndividual);
                inconsistencyRatioStatus =
                    !((resultsData.Count > 15) && !actionData.OnlyMainDiagonalEvaluated());
            }
            else
            {
                inconsistencyRatioStatus = false;
            }
            var canshowresults = new
            {
                individual = actionData.CanShowIndividualResults,
                combined = CanShowCombinedResults(actionData)
            };

            if(!equalMessage)
            {
                if (!canshowresults.individual && !canshowresults.combined)
                {
                    messagenote = TeamTimeClass.ResString("sInsufficientData");
                }
                else if (!canshowresults.individual)
                    messagenote = TeamTimeClass.ResString("msgNoEvalDataIndividualResults");
                else if (!canshowresults.combined)
                    messagenote = TeamTimeClass.ResString("msgNoEvalDataGroupResults");
            }


            bool showIndex = app.ActiveProject.ProjectManager.Parameters.ResultsLocalShowIndex;
            var isAlternative = actionData.ParentNode.IsTerminalNode;
            var lblredNumbers = "";
            if (isAlternative)
            {
                lblredNumbers = TeamTimeClass.ResString("sInconsistencyToDoRed");
            }
            else
            {
                lblredNumbers = TeamTimeClass.ResString("sInconsistencyToDoRedAlts");

            };
            lblredNumbers = TeamTimeClass.PrepareTask(lblredNumbers);
            var clipBoardData = getClipBoardData();

            var columnValueCombined = "Combined Results";
            if (app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName != "" && (actionData.ResultsViewMode == CanvasTypes.ResultsView.rvGroup || actionData.ResultsViewMode == CanvasTypes.ResultsView.rvBoth))
            {
                columnValueCombined = app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName;
            }

            var pipeParameters = new
            {
                CurrentNode = currentNode,
                highest_result = highestResult,
                results_data = resultsData,
                defaultsort = defaultsort,
                InconsistencyRatio = inconsistencyRatio,
                InconsistencyRatioStatus = inconsistencyRatioStatus,
                JudgmentType = judgmentType.ToString(),
                StepPairsData = stepPairsData,
                ObjectivesData = objectivesData,
                matrix_highest_result = ObjectivesHighestResult,
                messagenote = messagenote,
                canshowresults = canshowresults,
                showIndex = showIndex,
                isAlternative = isAlternative,
                MeasurementType = measurementType,
                JudgmentsSaved = JudgmentSaved,
                lblredNumbers = lblredNumbers,
                equalMessage = equalMessage,
                ClipBoardData = clipBoardData,
                resultsTitle = resultsTitle,
                showTitle = showTitle,
                columnValueCombined = columnValueCombined
            };
            return pipeParameters;
        }

        private static string getClipBoardData()
        {
            var clipBoard = "";
            var Model = (DataModel)HttpContext.Current.Session[Constants.SessionModel];

            for (int i = 0; i< Model.ObjectivesData.Count; i++)
            {
                for(int j = 0; j < i; j++)
                {
                    clipBoard += "\t";
                }
                for(int j = 0; j < Model.ObjectivesData.Count; j++)
                {
                    if(i < j)
                    {
                        var p = "";
                        foreach (StepsPairs pair in Model.StepPairs)
                        {
                            if (pair.Obj1 == Model.ObjectivesData[i].ID && pair.Obj2 == Model.ObjectivesData[j].ID)
                            {
                                p += "\t" + (pair.Advantage >= 0 ? "" : "-") + pair.Value.ToString("F");
                                break;
                            }
                            if (pair.Obj2 == Model.ObjectivesData[i].ID && pair.Obj1 == Model.ObjectivesData[j].ID)
                            {
                                p += "\t" + (pair.Advantage >= 0 ? "-" : "") + pair.Value.ToString("F");
                                break;
                            }
                        }
                        if (p == "")
                        {
                            clipBoard += "\t";
                        }
                        clipBoard += p;
                    }
                }
                clipBoard += "\n";
            }
            //For i As Integer = 0 To Model.ObjectivesData.Count - 1
            //    For j As Integer = 0 To i -1
            //        ClipData += vbTab
            //    Next
            //    For j As Integer = i + 1 To Model.ObjectivesData.Count - 1
            //        Dim p As StepsPairs = Nothing
            //        Dim pn As StepsPairs = Nothing
            //        For Each pair As StepsPairs In list
            //            If pair.Obj1 = Model.ObjectivesData(i).ID AndAlso pair.Obj2 = Model.ObjectivesData(j).ID Then
            //                p = pair
            //            End If
            //            If pair.Obj2 = Model.ObjectivesData(i).ID AndAlso pair.Obj1 = Model.ObjectivesData(j).ID Then
            //                pn = pair
            //            End If
            //        Next
            //        ClipData += vbTab
            //        If p IsNot Nothing OrElse pn IsNot Nothing Then
            //            If p IsNot Nothing Then ClipData += CStr(IIf(Not p.Advantage, "", "-")) + p.Value.ToString
            //            If pn IsNot Nothing Then ClipData += CStr(IIf(Not pn.Advantage, "-", "")) + pn.Value.ToString
            //            'If j < Model.ObjectivesData.Count - 1 Then ClipData += CStr(IIf(ClipData = "", "", vbTab))
            //            'Else
            //            'If j < Model.ObjectivesData.Count - 1 Then ClipData += vbTab
            //        End If
            //    Next
            //    If i < Model.ObjectivesData.Count - 1 Then ClipData += "CRLF" 'vbNewLine
            //Next
            return clipBoard;
        }

        public static bool GetResultsPipeStepData(string data, bool thisIsBad)
        {
            //Return True
            var context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var Model = (DataModel)context.Session[Constants.SessionModel];

            bool retVal = false;
            string[] lines = data.Split('\n');
            clsProjectManager PM = app.ActiveProject.ProjectManager;
            clsNode tNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(Model.ParentID);
            int tUserID = app.ActiveProject.ProjectManager.UserID;
            if (!thisIsBad)
            {
                tNode.Judgments.DeleteJudgmentsFromUser(tUserID);
            }
            int N = Model.ObjectivesData.Count;
            if (tNode != null && IsPairwiseMeasureType(tNode.get_MeasureType()))
            {
                for (int i = 0; i <= Model.ObjectivesData.Count - 1; i++)
                {
                    if(i < lines.Length)
                    {
                        string line = lines[i];
                        string[] vals = line.Split('\t');

                            for (int j = i + 1; j <= Model.ObjectivesData.Count - 1; j++)
                            {
                                if (j < vals.Length)
                                {
                                    double v = 0;
                                    if (StringFuncs.String2Double(vals[j], ref v) && v != 0)
                                    {
                                            int Advantage = 1;
                                        if (v < 0)
                                            Advantage = -1;
                                        // write judgment for a pair                            
                                        //Dim newpwMD As clsPairwiseMeasureData = New clsPairwiseMeasureData(Model.ObjectivesData(i).ID, Model.ObjectivesData(j).ID, Advantage, v, Model.ParentID, tUserID)
                                        clsPairwiseMeasureData newpwMD = new clsPairwiseMeasureData(Model.ObjectivesData[i].ID, Model.ObjectivesData[j].ID, Advantage, Math.Abs(v), Model.ParentID, tUserID);
                                        tNode.Judgments.AddMeasureData(newpwMD);
                                        retVal = true;
                                    }
                                }
                            }
                    }

                }
                DateTime saveTime = DateTime.Now;
                app.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(app.ActiveProject.ProjectManager.GetUserByID(tUserID), saveTime);
                
                //commented this first volt
                //if (retVal) 
                   // app.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserLastJudgmentTime(app.ActiveProject.ProjectManager.User, saveTime);
            }
            // save all judgments if changed (retVal = True?)
            if (retVal)
            {
                var _with1 = app.ActiveProject.ProjectManager;
                _with1.PipeBuilder.PipeCreated = false;
                _with1.PipeBuilder.CreatePipe();
            }
            return retVal;
        }

        //local results
        public static string GetPairsData()
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var Model = (DataModel)context.Session[Constants.SessionModel];

            if (HttpContext.Current.Session[InconsistencySortingEnabled] == null)
            {
                HttpContext.Current.Session[InconsistencySortingEnabled] = true;

            }


            string retVal = "";
            if (Model != null && Model.StepPairs != null)
            {

                foreach (StepsPairs item in Model.StepPairs)
                {
                    retVal += Convert.ToString((!string.IsNullOrEmpty(retVal) ? "," : "")) +
                              $"[{item.StepNumber},{item.Value},{item.Obj1},{item.Obj2},{item.Advantage},{item.BestFitValue},{item.BestFitAdvantage},{item.Rank},{(item.IsUndefined ? "1" : "0")}]";
                }
            }

            //Return String.Format("var step_data = [{0}];", retVal)

            return $"[{retVal}]";
        }

        //local results
        public static double ObjectivesHighestResult
        {
            get
            {
                double showUnassessed = HttpContext.Current.Session[SessionObjectivesHighestResult] == null ? 0 : (double)HttpContext.Current.Session[SessionObjectivesHighestResult];
                return showUnassessed;
            }
            set { HttpContext.Current.Session[SessionObjectivesHighestResult] = value; }
        }
        public static string GetObjectivesData()
        {
            //get sorted objectives data
            string retVal = "";
            var Model = (DataModel)HttpContext.Current.Session[Constants.SessionModel];

            if (Model != null && Model.ObjectivesDataSorted != null)
            {
                foreach (Objective obj in Model.ObjectivesDataSorted)
                {
                    if (obj.Value > ObjectivesHighestResult)
                        ObjectivesHighestResult = obj.Value;
                    retVal += Convert.ToString((!string.IsNullOrEmpty(retVal) ? "," : "")) +
                              $"[{obj.ID},'{obj.Value}','{StringFuncs.JS_SafeString(obj.Name)}','{StringFuncs.Double2String(obj.Value*100, 2, true)}']";
                }
            }
            //Return String.Format("var obj_data = [{0}];", retVal)
            return $"[{retVal}]";
        }

        //global results
        public static object CreateGlobalResults(int step, int normalization = 1, int customWrtNodeID = -1) 
        {
            var wrtNodeId = -1;
            var wrtNodeName = "";
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var globalActionData = (Canvas.clsShowGlobalResultsActionData)AnytimeClass.Action(step).ActionData;
            globalActionData.WRTNode = app.ActiveProject.HierarchyObjectives.GetLevelNodes(0)[0];
            wrtNodeId = globalActionData.WRTNode.NodeID;
            wrtNodeName = globalActionData.WRTNode.NodeName;

            var defaultsort = (int)app.ActiveProject.PipeParameters.GlobalResultsSortMode;

            var hierarchy = new object[app.ActiveProject.HierarchyObjectives.Nodes.Count][];
            var highestResult = 0.00;
            for (int i = 0; i < app.ActiveProject.HierarchyObjectives.Nodes.Count; i++)
            {
                hierarchy[i] = new object[3];
                hierarchy[i][0] = app.ActiveProject.HierarchyObjectives.Nodes[i].NodeID;
                hierarchy[i][1] = app.ActiveProject.HierarchyObjectives.Nodes[i].NodeName;
                hierarchy[i][2] = app.ActiveProject.HierarchyObjectives.Nodes[i].WRTRelativeAPriority;

            }
            if (customWrtNodeID == -1)
                customWrtNodeID = globalActionData.WRTNode.NodeID;

            app.ActiveProject.ProjectManager.CheckCustomCombined();
            var dataobject = Anytime.Anytime.GetOverallResultsData(normalization, customWrtNodeID);  //GetIntermediateResultsData(1);
            var resultsData = (List<List<object>>) dataobject[0];
            //try
            //{
                foreach (List<object> result in resultsData)
                {
                    if (Convert.ToDouble(result[2]) > highestResult)
                    {
                        highestResult = Convert.ToDouble(result[2]);
                    }
                    if (Convert.ToDouble(result[3]) > highestResult)
                    {
                        highestResult = Convert.ToDouble(result[3]);
                    }
                }
            //}
            //catch
            //{

            //}
            
            wrtNodeName = (string) dataobject[2];
            string sEmail = app.ActiveUser.UserEMail;
            ECTypes.clsUser ahpUser = app.ActiveProject.ProjectManager.GetUserByEMail(sEmail);
            int ahpUserId = ahpUser.UserID;

            if (UserIsReadOnly())
            {
                //set ReadOnlyUser here
                ahpUser = app.DBUserByID(GetReadOnlyUserID());
                ahpUserId = ahpUser.UserID;
                sEmail = ahpUser.UserEMail;
            }

            bool showIndex = app.ActiveProject.ProjectManager.Parameters.ResultsGlobalShowIndex;

            var canshowresults = dataobject[1];
            //var canshowresults = new
            //{
            //    individual = globalActionData.get_CanShowIndividualResults(ahpUserId, globalActionData.WRTNode),
            //    combined = globalActionData.get_CanShowGroupResults()
            //};

            string[] messagenote = (string[]) dataobject[3];

            var columnValueCombined = "Combined Results";
            if (app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName != "" && (globalActionData.ResultsViewMode == CanvasTypes.ResultsView.rvGroup || globalActionData.ResultsViewMode == CanvasTypes.ResultsView.rvBoth))
            {
                columnValueCombined = app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName;
            }

            var pipeParameters = new
            {
                WrtNodeID = wrtNodeId,
                Hierarchy = hierarchy,
                highest_result = highestResult,
                defaultsort = defaultsort,
                results_data = resultsData,
                canshowresult = canshowresults,
                WrtNodeName = wrtNodeName,
                messagenote = messagenote,
                showIndex = showIndex,
                columnValueCombined = columnValueCombined,
                ExpectedValue = Anytime.Anytime.ExpectedValueString
            };
            return pipeParameters;
        }


        //survey
        public static object CreateSurvey(int step)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            object pipeParameters = null;
            var nodelist = new ArrayList();
            var alternativelist = new List<object>();
            var objectivelist = new List<object>();
            clsSurveyPage surveyPage = null;
            var questionNumbering = new List<int>();
            var altStringAll = new
            {
                NodeName = "All",
                isDisabled = true
            };
            alternativelist.Insert(0, altStringAll);
            clsSpyronSurveyAction data = (clsSpyronSurveyAction)Action(Convert.ToInt32(step)).ActionData;
            //List<ECCore.ECTypes.clsUser> tUserList = ECCore.MiscFuncs.ECMiscFuncs.GetUsersList(app.SpyronProjectsConnectionDefinition.ConnectionString, clsProject.StorageType, app.ActiveProject.ProviderType, app.ProjectID);
            List<ECCore.ECTypes.clsUser> tUserList = ECCore.MiscFuncs.ECMiscFuncs.GetUsersList(app.CanvasMasterConnectionDefinition.ConnectionString, clsProject.StorageType, app.ActiveProject.ProviderType, app.ProjectID);   // D6423
            Dictionary<string, clsComparionUser> aUsersList = new Dictionary<string, clsComparionUser>();
            foreach (ECTypes.clsUser user in tUserList)
            {
                aUsersList.Add(user.UserEMail, new clsComparionUser
                {
                    ID = user.UserID,
                    UserName = user.UserName
                });
            }
            app.SurveysManager.ActiveUserEmail = app.ActiveUser.UserEMail;
            var tUser = (ECTypes.clsUser)app.ActiveProject.ProjectManager.User;

            clsSurveyInfo tSurvey = app.SurveysManager.get_GetSurveyInfoByProjectID(app.ProjectID, (SurveyType)data.SurveyType, aUsersList);
            if ((tSurvey != null))
            {

                surveyPage = (clsSurveyPage)tSurvey.get_Survey(tUser.UserEMail).Pages[data.StepNumber - 1];
                if (surveyPage == null && tSurvey.get_Survey(tUser.UserEMail).Pages.Count > 0)
                    surveyPage = (clsSurveyPage)tSurvey.get_Survey(tUser.UserEMail).Pages[0];
            }
            if (surveyPage != null)
            {
                var surveyAnswers = new string[surveyPage.Questions.Count][];
                var surveyQuestions = new object[surveyPage.Questions.Count];
                clsRespondent respondent = tSurvey.get_Survey(tUser.UserEMail).RespondentByEmail(tUser.UserEMail);
                if (respondent == null)
                {
                    respondent = new clsRespondent();
                    respondent.Email = tUser.UserEMail;
                    respondent.Name = tUser.UserName;
                    tSurvey.get_SurveyDataProvider(tUser.UserEMail).SaveStreamRespondentAnswers(ref respondent);
                    //L0442
                    respondent = surveyPage.Survey.RespondentByEmail(tUser.UserEMail);
                }
                for (int i = 0; i < surveyPage.Questions.Count; i++ )
                {
                    var question = (clsQuestion)surveyPage.Questions[i];
                    clsAnswer answer = null;
                    if (respondent != null)
                    {
                        answer = respondent.AnswerByQuestionGUID(question.AGUID);
                    }
                    if (!tSurvey.HideIndexNumbers)
                        questionNumbering.Add(surveyPage.Survey.GetQuestionPageIndex(question.AGUID));
                    
                    if (answer != null)
                    {
                    
                        var answerbyQuestionString = new string[3] { answer.AnswerValuesString, question.AGUID.ToString(), question.AllowSkip.ToString() };
                        answerbyQuestionString[0] = answerbyQuestionString[0].Replace("\"", "");

                        switch (question.Type)
                        {

                            case QuestionType.qtVariantsRadio:
                                var vars = new clsVariant();
                                if (answer.AnswerVariants.Count > 0)
                                {
                                    vars = (clsVariant) answer.AnswerVariants[0];
                                }
                                if (vars.Type ==VariantType.vtOtherLine)
                                {
                                    var variantCount = question.Variants.Count - 1;
                                    var variant = (clsVariant)question.Variants[variantCount];
                                    if (variant.Type == VariantType.vtOtherLine)
                                        answerbyQuestionString[0] += ":";
                                }
                                break;
                            case QuestionType.qtVariantsCheck:
                            case QuestionType.qtVariantsCombo:
                                //if last is other then add ':' for identification
                                if (answer.AnswerVariants.Count > 0)
                                {
                                    var variantCount = answer.AnswerVariants.Count - 1;
                                    var variant = (clsVariant)answer.AnswerVariants[variantCount];
                                    if (variant.Type == VariantType.vtOtherLine)
                                        answerbyQuestionString[0] += ":";
                                }
                                break;
                            case QuestionType.qtObjectivesSelect:
                                var objStringAll = new
                                {
                                    NodeName = "All",
                                    isDisabled = true,
                                    level = 0
                                };
                                answerbyQuestionString[0] = objStringAll.isDisabled + ";";
                                objectivelist = GetObjectivesListforSurvey(app.ActiveProject.HierarchyObjectives.Nodes, ref answerbyQuestionString[0]);
                                objectivelist.Insert(0, objStringAll);
                                break;
                            case QuestionType.qtAlternativesSelect:

                                //initialize for 'ALL' item
                                
                                answerbyQuestionString[0] = altStringAll.isDisabled + ";";
                                foreach (clsNode node in app.ActiveProject.HierarchyAlternatives.Nodes)
                                {
                                    var alternativeitem = new
                                    {
                                        NodeName = node.NodeName,
                                        isDisabled = !node.get_DisabledForUser(tUser.UserID)
                                    };
                                    alternativelist.Add(alternativeitem);
                                    if (!alternativeitem.isDisabled)
                                    {
                                        altStringAll = new
                                        {
                                            NodeName = "All",
                                            isDisabled = false
                                        };
                                    }
                                    answerbyQuestionString[0] = altStringAll.isDisabled + ";";
                                }

                                break;
                            case QuestionType.qtComment:
                                question.AllowSkip = true;
                                answerbyQuestionString[2] = question.AllowSkip.ToString();
                                break;
                        }

                        //check if last variant is other

                    
                        surveyAnswers[i] = answerbyQuestionString;
                        //SurveyQuestions[i] = (object)question;
                    }
                    else
                    {
                        var answerbyQuestionString = new string[3] { "", question.AGUID.ToString(), question.AllowSkip.ToString() };
                        switch (question.Type)
                        {
                            case QuestionType.qtObjectivesSelect:
                                var objStringAll = new
                                {
                                    NodeName = "All",
                                    isDisabled = true,
                                    level = 0
                                };
                                answerbyQuestionString[0] = objStringAll.isDisabled + ";";
                                objectivelist = GetObjectivesListforSurvey(app.ActiveProject.HierarchyObjectives.Nodes, ref answerbyQuestionString[0]);
                                objectivelist.Insert(0, objStringAll);
                                break;
                            case QuestionType.qtAlternativesSelect:

                                //initialize for 'ALL' item
                                
                                answerbyQuestionString[0] = altStringAll.isDisabled + ";";
                                foreach (clsNode node in app.ActiveProject.HierarchyAlternatives.Nodes)
                                {
                                    var alternativeitem = new
                                    {
                                        NodeName = node.NodeName,
                                        isDisabled = !node.get_DisabledForUser(tUser.UserID)
                                    };
                                    alternativelist.Add(alternativeitem);
                                    if (!alternativeitem.isDisabled)
                                    {
                                        altStringAll = new
                                        {
                                            NodeName = "All",
                                            isDisabled = false
                                        };
                                    }
                                    answerbyQuestionString[0] += altStringAll.isDisabled + ";";
                                }

                                break;
                            case QuestionType.qtComment:
                                question.AllowSkip = true;
                                answerbyQuestionString[2] = question.AllowSkip.ToString();
                                break;
                        }
                        
                        surveyAnswers[i] = answerbyQuestionString;
                        //var answer = new clsAnswer();
                        //answer.Question = question;
                        //ReadAnswer(answer, question, "");
                    }

                    if (question.Text.Contains(MimeIdentifier))
                    {
                        var unpackedText = InfodocService.Infodoc_Unpack(tSurvey.ProjectID, 0, ExpertChoice.Data.Consts.reObjectType.SurveyQuestion, question.AGUID.ToString(), question.Text, true, true, -1);
                        if (unpackedText == "")
                        {
                            var path = context.Server.MapPath("~/");
                            Consts._FILE_ROOT = context.Server.MapPath("~/");
                            Consts._FILE_MHT_FILES = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, "DocMedia/MHTFiles/"));    // D3411; 
                            var sBasePath = InfodocService.Infodoc_Path(tSurvey.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.SurveyQuestion, question.AGUID.ToString(), -1);
                            var sBaseUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}";
                            //question.Text = unpacked_text;

                            //string filename = string.Format("{0}.htm", question.AGUID.ToString());
                            var samp = InfodocService.Infodoc_Pack(question.Text, sBaseUrl, sBasePath);
                            unpackedText = InfodocService.Infodoc_Unpack(tSurvey.ProjectID, 0, ExpertChoice.Data.Consts.reObjectType.SurveyQuestion, question.AGUID.ToString(), samp, true, true, -1); ;
                        }
                        question.Text = unpackedText;
                    }

                    int titleStartIndex = question.Text.IndexOf("<TITLE>", StringComparison.CurrentCultureIgnoreCase);
                    if (titleStartIndex >= 0)
                    {
                        int titleEndIndex = question.Text.IndexOf("</TITLE>", titleStartIndex, StringComparison.CurrentCultureIgnoreCase);
                        titleEndIndex += 8;
                        string titleString = question.Text.Substring(titleStartIndex, titleEndIndex - titleStartIndex);

                        //removing title text because it is concatinating with question text when we call $scope.getHtml() of MainController.js
                        question.Text = question.Text.Replace(titleString, "<TITLE></TITLE>");
                    }

                    if (question.Text.Contains("\r\n"))
                    {
                        question.Text = question.Text.Replace("\r\n", "");
                    }
                    surveyPage.Questions[i] = question;
                    
                }
                if (respondent == null || respondent.Answers.Count < 1)
                {
                    var need = false;
                    ReadPageAnswers(ref need, step, surveyAnswers);
                }
                var surveyContent = new
                {
                    Title = surveyPage.Title,
                    Questions = surveyPage.Questions,
                };
                
                pipeParameters = new
                {
                    SurveyPage = surveyContent,
                    SurveyAnswers = surveyAnswers,
                    alternativelist = alternativelist,
                    objectivelist = objectivelist,
                    QuestionNumbering = questionNumbering
                };
            }

            return pipeParameters;
        }


        //survey
        public static string ReadPageAnswers(ref bool needToRebuildPipe, int step, string[][] respondentAnswers )
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var pipeParameters = new object();
            clsSurveyPage surveyPage = null;
            var surveyAnswers = new List<string>();
            clsSpyronSurveyAction data = (clsSpyronSurveyAction)Action(Convert.ToInt32(step)).ActionData;
            //List<ECCore.ECTypes.clsUser> tUserList = ECCore.MiscFuncs.ECMiscFuncs.GetUsersList(app.SpyronProjectsConnectionDefinition.ConnectionString, clsProject.StorageType, app.ActiveProject.ProviderType, app.ProjectID);
            List<ECCore.ECTypes.clsUser> tUserList = ECCore.MiscFuncs.ECMiscFuncs.GetUsersList(app.CanvasMasterConnectionDefinition.ConnectionString, clsProject.StorageType, app.ActiveProject.ProviderType, app.ProjectID);   // D6423
            Dictionary<string, clsComparionUser> aUsersList = new Dictionary<string, clsComparionUser>();
            var groupAnswers = "";
            foreach (ECTypes.clsUser user in tUserList)
            {
                aUsersList.Add(user.UserEMail, new clsComparionUser
                {
                    ID = user.UserID,
                    UserName = user.UserName
                });
            }
            app.SurveysManager.ActiveUserEmail = app.ActiveUser.UserEMail;
            var tUser = (ECTypes.clsUser)app.ActiveProject.ProjectManager.User;
            clsSurveyInfo tSurvey = app.SurveysManager.get_GetSurveyInfoByProjectID(app.ProjectID, (SurveyType)data.SurveyType, aUsersList);
            if ((tSurvey != null))
            {
                
                surveyPage = (clsSurveyPage)tSurvey.get_Survey(tUser.UserEMail).Pages[data.StepNumber - 1];
                if (surveyPage == null && tSurvey.get_Survey(tUser.UserEMail).Pages.Count > 0)
                    surveyPage = (clsSurveyPage)tSurvey.get_Survey(tUser.UserEMail).Pages[0];
            }
            
	        if (surveyPage != null && surveyPage.Survey != null) {
                clsRespondent respondent = surveyPage.Survey.RespondentByEmail(tUser.UserEMail);
		        if (respondent == null) {
			        respondent = new clsRespondent();
			        respondent.Email = tUser.UserEMail;
			        respondent.Name = tUser.UserName;
			        tSurvey.get_SurveyDataProvider(tUser.UserEMail).SaveStreamRespondentAnswers(ref respondent);
			        //L0442
			        respondent = surveyPage.Survey.RespondentByEmail(tUser.UserEMail);
		        }
		        //L0043 ==
		        if (respondent != null) {
                    //Respondent.Answers.Clear();

			        foreach (clsQuestion aQuestion in surveyPage.Questions) 
                    {
                        var idx = surveyPage.Questions.IndexOf(aQuestion);
					    if (aQuestion.Type == QuestionType.qtAlternativesSelect | aQuestion.Type == QuestionType.qtObjectivesSelect | aQuestion.LinkedAttributeID != Guid.Empty)
						    needToRebuildPipe = true;
                        //AQuestionControl.ReadAnswer();
                        var answer = new clsAnswer();
                        answer.Question = aQuestion;
                        answer.AnswerDate = DateTime.Now;
                        var wIndex = -1;
                        if (!respondent.Answers.Contains(answer) & true) {
                            //Respondent.Answers.RemoveAt(ind);
                            if (respondent.AnswerByQuestionGUID(aQuestion.AGUID) != null)
                            {
                                var ans = respondent.AnswerByQuestionGUID(aQuestion.AGUID);
                                //string[] StringAnswer = { ans.AnswerValuesString, ans.AGUID.ToString() };
                                var ind = GetIndexofChild(respondentAnswers, aQuestion.AGUID.ToString());
                                if (aQuestion.AGUID.ToString() == respondentAnswers[ind][1])
                                {
                                    ReadAnswer(ans, aQuestion, respondentAnswers[ind][0]);
                                    wIndex = ind;
                                }
                                else
                                {
                                    ReadAnswer(ans, aQuestion, respondentAnswers[idx][0]);
                                    wIndex = idx;
                                }
                                    

                                //Respondent.Answers.RemoveAt(ind);
                                //Respondent.Answers.Add(answer);
                            }
                            else
                            {
                                respondent.Answers.Add(answer);
                                var ans = respondent.AnswerByQuestionGUID(aQuestion.AGUID);
                                var ind = GetIndexofChild(respondentAnswers, aQuestion.AGUID.ToString());
                                if (aQuestion.AGUID.ToString() == respondentAnswers[ind][1])
                                {
                                    ReadAnswer(ans, aQuestion, respondentAnswers[ind][0]);
                                    wIndex = ind;
                                }
                                else
                                {
                                    ReadAnswer(ans, aQuestion, respondentAnswers[idx][0]);
                                    wIndex = idx;
                                }
                            }
					    }

                        if ((aQuestion.Type == QuestionType.qtAlternativesSelect | aQuestion.Type == QuestionType.qtObjectivesSelect) & (app.ActiveProject.ProjectManager != null))
                        {
                            app.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserDisabledNodes(app.ActiveProject.ProjectManager.User);
                            //L0043
                        }
                        
                        if ((respondentAnswers[wIndex][0] != "" && respondentAnswers[wIndex][0] != ":") & !aQuestion.LinkedAttributeID.Equals(Guid.Empty))
                        {
                            groupAnswers = respondentAnswers[wIndex][0];

                            Attributes.clsAttribute AAttribute = null;
                            SetUserAttribute(aQuestion.LinkedAttributeID, respondent.ID, Attributes.AttributeValueTypes.avtString, respondentAnswers[wIndex][0]);
                            foreach (Attributes.clsAttribute atr in app.ActiveProject.ProjectManager.Attributes.GetUserAttributes())
                            {
                                if (atr.ID.Equals(aQuestion.LinkedAttributeID))
                                {
                                    AAttribute = atr;
                                    break; // TODO: might not be correct. Was : Exit For
                                }
                            }
                            //var ss = app.ActiveProject.ProjectManager.Attributes.GetAttributeValue(AAttribute.ID, respondent.ID);
                            if (AAttribute != null)
                            {
                                string AnswerString = "";

                                if (aQuestion.Type == QuestionType.qtVariantsCheck)
                                {
                                    var splitAnswers = groupAnswers.Split(';');
                                    foreach (string value in splitAnswers)
                                    {
                                        if ((groupAnswers != "" && groupAnswers != ":"))
                                        {
                                            AnswerString += (AnswerString == "" ? "" : ";") + "\"" + value.Trim() + "\"";
                                        }
                                    }
                                }
                                else
                                {
                                    if ((groupAnswers != "" && groupAnswers != ":"))
                                        AnswerString = "\"" + respondentAnswers[wIndex][0].Trim() + "\"";
                                }
                                SetUserAttribute(aQuestion.LinkedAttributeID, respondent.ID, AAttribute.ValueType, AnswerString);
                            }
                        }

                    }
                    if (true) {
                        tSurvey.get_SurveyDataProvider(respondent.Email).SaveStreamRespondentAnswers(ref respondent);
				        //L0442
                        tSurvey.set_SurveyDataProvider(respondent.Email, null);
				        //L0442
			        }
		        }
	        }
            return "";
        }

        //survey
        public static void ReadAnswer(clsAnswer answer, clsQuestion question, string key)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var hierarchy = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy);
            if (question.Type != QuestionType.qtComment)
            {
                if (answer == null)
                {
                    answer = new clsAnswer();
                    answer.Question = question;
                }
                answer.AnswerDate = DateTime.Now;
                //L0355
                if (question.Type == QuestionType.qtObjectivesSelect)
                {
                    var objectives = key.Split(';');
                    var objectiveshierarchy = app.ActiveProject.HierarchyObjectives.Nodes;
                    foreach (clsNode node in objectiveshierarchy)
                    {
                        if (!node.IsAlternative)
                        {
                            var idx = objectiveshierarchy.IndexOf(node);
                            node.set_DisabledForUser(hierarchy.ProjectManager.User.UserID, !Convert.ToBoolean(objectives[idx + 1]));
                        }
                    }
                }

                if (question.Type == QuestionType.qtAlternativesSelect)
                {
                    var alternatives = key.Split(';');
                    var altshierarchy = app.ActiveProject.HierarchyAlternatives.Nodes;
                    foreach (clsNode node in app.ActiveProject.HierarchyAlternatives.Nodes)
                    {
                        if (node.IsAlternative)
                        {
                            var idx = altshierarchy.IndexOf(node);
                            node.set_DisabledForUser(hierarchy.ProjectManager.User.UserID, !Convert.ToBoolean(alternatives[idx + 1]));
                        }
                    }
                }

                answer.AnswerVariants.Clear();

                if (key != null)
                {
                    switch (question.Type)
                    {
                        case QuestionType.qtOpenLine:
                        case QuestionType.qtOpenMemo:
                        {
                            string K = null;
                            clsVariant AVariant = default(clsVariant);
                            K = key;
                            if (!string.IsNullOrEmpty(K))
                            {
                                AVariant = new clsVariant();
                                AVariant.VariantValue = K;
                                AVariant.Type = VariantType.vtOtherLine;
                                answer.AnswerVariants.Add(AVariant);
                            }
                        }

                            break;
                        case QuestionType.qtVariantsCheck:
                        case QuestionType.qtImageCheck:
                        {
                            var answerVariants = key.Split(';');
                            Array.Sort(answerVariants, StringComparer.InvariantCulture);
                            if (answer.AnswerVariants.Count < 1)
                            {
                                foreach (clsVariant aVariant in question.Variants)
                                {
                                    var idx = question.Variants.IndexOf(aVariant);
                                    if (aVariant.Type == VariantType.vtText)
                                    {
                                        var ansIdx = Array.IndexOf(answerVariants, aVariant.Text);
                                        if (ansIdx > -1)
                                        {
                                            if (answerVariants[ansIdx] == aVariant.Text)
                                            {
                                                if (key != "")
                                                {
                                                    aVariant.VariantValue = answerVariants[ansIdx];
                                                    aVariant.Text = answerVariants[ansIdx];
                                                    answer.AnswerVariants.Add(aVariant);
                                                }
                                            }
                                        }
                                    }
                                    if ((aVariant.Type == VariantType.vtOtherLine) | (aVariant.Type == VariantType.vtOtherMemo))
                                    {
                                        var othervalue = "";
                                        foreach (string other in answerVariants)
                                        {
                                            if (key != "")
                                            {
                                                if (other.Contains(":"))
                                                    othervalue = other;
                                            }
                                        }
                                        var ansIdx = Array.IndexOf(answerVariants, othervalue);
                                        if (ansIdx > -1)
                                        {
                                            var values = othervalue.Split(':');
                                            //values[1] is the value entered on the other textbox while values[0] is the variant text itself
                                            aVariant.VariantValue = values[0];
                                            answer.AnswerVariants.Add(aVariant);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (clsVariant aVariant in answer.AnswerVariants)
                                {
                                    var idx = answer.AnswerVariants.IndexOf(aVariant);
                                    if ((aVariant.Type == VariantType.vtOtherLine) | (aVariant.Type == VariantType.vtOtherMemo) | (aVariant.Type == VariantType.vtText))
                                    {
                                        if (key != "")
                                        {
                                            aVariant.VariantValue = answerVariants[idx];
                                        }
                                    }
                                }
                            }

                        }

                            break;
                        case QuestionType.qtObjectivesSelect:
                            //QPrefix = "cbObjectives" + Question.AGUID.ToString() + "s";
                            //if (key.Contains(QPrefix))
                            //{
                            //    foreach (clsNode node in Hierarchy.Nodes)
                            //    {
                            //        if (key.Contains(QPrefix + node.NodeID.ToString() + "s"))
                            //        {
                            //            node.set_DisabledForUser(Hierarchy.ProjectManager.User.UserID, false);
                            //        }
                            //    }
                            //}

                            //not needed for now
                            break;
                        case QuestionType.qtAlternativesSelect:
                            //if (key != null)
                            //{
                            //    foreach (clsNode node in Hierarchy.Nodes)
                            //    {
                            //        if (key.Contains(QPrefix + node.NodeID.ToString() + "s"))
                            //        {
                            //            node.set_DisabledForUser(Hierarchy.ProjectManager.User.UserID, false);
                            //        }
                            //    }
                            //}

                            break;
                        case QuestionType.qtVariantsRadio:
                            if (answer.AnswerVariants.Count < 1)
                            {
                                var pass = false;
                                foreach (clsVariant aVariant in question.Variants)
                                {

                                    if ((aVariant.Type == VariantType.vtOtherMemo) | (aVariant.Type == VariantType.vtText))
                                    {
                                        if (key != "")
                                        {
                                            if (aVariant.VariantValue == key)
                                            {
                                                pass = true;
                                                answer.AnswerVariants.Add(aVariant);
                                            }
                                        }
                                    }
                                    if (aVariant.Type == VariantType.vtOtherLine && !pass)
                                    {
                                        if (aVariant.VariantValue != key)
                                        {
                                            aVariant.VariantValue = key;
                                            answer.AnswerVariants.Add(aVariant);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (clsVariant aVariant in answer.AnswerVariants)
                                {
                                    if ((aVariant.Type == VariantType.vtOtherLine) | (aVariant.Type == VariantType.vtOtherMemo) | (aVariant.Type == VariantType.vtText))
                                    {
                                        //If Request.Form(key).ToString() <> "" Then

                                        //Answer.AnswerVariants.Add(AVariant)
                                        //End If
                                        if (key != "")
                                        {
                                            aVariant.VariantValue = key;
                                        }
                                    }
                                }
                            }

                            break;
                        case QuestionType.qtVariantsCombo:
                            if (answer.AnswerVariants.Count < 1)
                            {
                                foreach (clsVariant aVariant in question.Variants)
                                {
                                    if (aVariant.VariantValue.Contains(key))
                                    {
                                        if ((aVariant.Type == VariantType.vtText))
                                        {
                                            if (key != "")
                                            {
                                                answer.AnswerVariants.Add(aVariant);
                                                break;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        if ((aVariant.Type == VariantType.vtOtherLine) | (aVariant.Type == VariantType.vtOtherMemo))
                                        {
                                            if (key != "")
                                            {
                                                aVariant.VariantValue = key;
                                                answer.AnswerVariants.Add(aVariant);
                                                break;
                                            }
                                        }
                                    }

                                }
                            }
                            else
                            {
                                foreach (clsVariant aVariant in answer.AnswerVariants)
                                {
                                    if ((aVariant.Type == VariantType.vtOtherLine) | (aVariant.Type == VariantType.vtOtherMemo) | (aVariant.Type == VariantType.vtText))
                                    {
                                        if (key != "")
                                        {
                                            //If Request.Form(key).ToString() <> "" Then
                                            aVariant.VariantValue = key;
                                            //Answer.AnswerVariants.Add(AVariant)
                                            //End If
                                        }
                                    }
                                }
                            }
                            break;
                        case QuestionType.qtNumber:
                        case QuestionType.qtNumberColumn:
                            {
                                string K = null;
                                clsVariant AVariant = default(clsVariant);
                                K = key;
                                if (!string.IsNullOrEmpty(K))
                                {
                                    AVariant = new clsVariant();
                                    AVariant.VariantValue = K;
                                    AVariant.Type = VariantType.vtOtherLine;
                                    answer.AnswerVariants.Add(AVariant);
                                }
                            }
                            break;
                    }
                }
                
                //if (Answer.AnswerVariants.Count == 0)
                //    Answer = null;
                //L0441
            }

        }

        //survey
        public static bool SetUserAttribute(Guid ID, int UserID, Attributes.AttributeValueTypes ValueType, object Value)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var _with1 = app.ActiveProject.ProjectManager;
            if (_with1.Attributes != null)
            {
                bool res = false;
                switch (ValueType)
                {
                    case Attributes.AttributeValueTypes.avtString:
                        res = _with1.Attributes.SetAttributeValue(ID, UserID, ValueType, Convert.ToString(Value), Guid.Empty, Guid.Empty);
                        break;
                    case Attributes.AttributeValueTypes.avtLong:
                        { 
                            long v = 0;
                            if (long.TryParse(Convert.ToString(Value), out v))
                            {
                                res = _with1.Attributes.SetAttributeValue(ID, UserID, ValueType, v, Guid.Empty, Guid.Empty);
                            }
                        }
                        break;
                    case Attributes.AttributeValueTypes.avtDouble:
                        { 
                            double v = 0;
                            // D1858
                            // D1858
                            if (StringFuncs.String2Double(Convert.ToString(Value), ref v))
                            {
                                res = _with1.Attributes.SetAttributeValue(ID, UserID, ValueType, Convert.ToDouble(v), Guid.Empty, Guid.Empty);
                            }
                        }
                        break;
                    case Attributes.AttributeValueTypes.avtBoolean:
                        {
                            bool v = false;
                            if (StringFuncs.Str2Bool(Convert.ToString(Value), ref v))
                            {
                                res = _with1.Attributes.SetAttributeValue(ID, UserID, ValueType, v, Guid.Empty, Guid.Empty);
                            }
                        }
                        break;
                }
                var ss = _with1.Attributes.WriteAttributeValues(Attributes.AttributesStorageType.astStreamsDatabase, _with1.StorageManager.ProjectLocation, _with1.StorageManager.ProviderType, _with1.StorageManager.ModelID, UserID);
                return res;
            }
            return false;
        }

        //survey
        private static int GetIndexofChild(string[][] items, string compareString)
        {
            foreach(string[] item in items)
            {
                if (item[1] == compareString)
                {
                    return Array.IndexOf(items, item);
                }
            }
            return -1;
        }

        //survey
        private static List<object> GetObjectivesListforSurvey(List<clsNode> nodes, ref string AnswerString)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var tUser = (ECTypes.clsUser)app.ActiveProject.ProjectManager.User;
            var level = new List<object>();
            foreach (clsNode node in nodes)
            {
                var idx = nodes.IndexOf(node);
                if (!node.IsAlternative)
                {
                    var objectiveitem = new
                    {
                        NodeName = node.NodeName,
                        isDisabled = idx == 0 || !node.get_DisabledForUser(tUser.UserID),
                        level = node.Level
                    };
                    AnswerString += objectiveitem.isDisabled + ";";
                    level.Add(objectiveitem);
                }
                 

            }
            return level;
        }

        //local results matrix table
        public static void SaveJudgments(int parentId)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            clsNode tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(parentId);
            if (tNode != null && IsPairwiseMeasureType(tNode.get_MeasureType()))
            {
                int tUserId = App.ActiveProject.ProjectManager.UserID;
                List<clsCustomMeasureData> tJudgments = new List<clsCustomMeasureData>();
                //clsUserJudgments tJudgments = new clsUserJudgments(tUserId); !!!
                foreach (clsCustomMeasureData tJud in tNode.Judgments.get_UsersJudgments(tUserId))
                {
                    clsPairwiseMeasureData pwMeasureData = (clsPairwiseMeasureData)tJud;
                    clsPairwiseMeasureData newpwMd = new clsPairwiseMeasureData(pwMeasureData.FirstNodeID, pwMeasureData.SecondNodeID, pwMeasureData.Advantage, pwMeasureData.Value, pwMeasureData.ParentNodeID, tUserId, pwMeasureData.IsUndefined, pwMeasureData.Comment);
                    tJudgments.Add(newpwMd);
                }
                context.Session[string.Format(SessJudgments, parentId)] = tJudgments;
                context.Session[SessDatetime] = App.ActiveProject.ProjectManager.User.LastJudgmentTime;
            }
        }

        //local results matrix table
        private static bool IsPairwiseMeasureType(ECMeasureType mt)
        {
            //A0819
            return mt == ECMeasureType.mtPairwise || mt == ECMeasureType.mtPWAnalogous;
        }


        //Private _JudgmentsSaved As Boolean = False
        public static bool JudgmentsSaved
        {
            get {
                HttpContext context = HttpContext.Current;
                var sessVal = TeamTimeClass.get_SessVar(SessSaved);
                return sessVal == "1"; }
            set {
                HttpContext context = HttpContext.Current;
                TeamTimeClass.set_SessVar(SessSaved, value ? "1" : "0");
            }
        }


        public static void RestoreJudgments(int parentId)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            clsNode tNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(parentId);
            string sSessName = string.Format(SessJudgments, parentId);
            if (tNode != null && JudgmentsSaved && context.Session[sSessName] != null)
            {
                int tUserId = app.ActiveProject.ProjectManager.UserID;
                List<clsCustomMeasureData> tJudgments = (List<clsCustomMeasureData>)context.Session[sSessName];
                DateTime? dt = null;
                if (context.Session[SessDatetime] != null)
                    dt = Convert.ToDateTime(context.Session[SessDatetime]);
                //if (tJudgments.UserID == tUserId) !!!
                //{
                    tNode.Judgments.DeleteJudgmentsFromUser(tUserId);
                    foreach (clsCustomMeasureData tJud in tJudgments)
                    {
                        clsPairwiseMeasureData pwMeasureData = (clsPairwiseMeasureData)tJud;
                        clsPairwiseMeasureData newpwMd = new clsPairwiseMeasureData(pwMeasureData.FirstNodeID, pwMeasureData.SecondNodeID, pwMeasureData.Advantage, pwMeasureData.Value, pwMeasureData.ParentNodeID, tUserId, pwMeasureData.IsUndefined, pwMeasureData.Comment);
                        tNode.Judgments.AddMeasureData(newpwMd, true);
                        app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(tNode, newpwMd);
                    }
                    //App.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(tUserID, DT)
                    //if (dt != null)
                    //{
                    //    DateTime dtt = (DateTime)dt;
                    //    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserLastJudgmentTime(app.ActiveProject.ProjectManager.User, dtt);
                    //}
                    context.Session.Remove(sSessName);
                    JudgmentsSaved = false;
                    //App.ActiveProject.ProjectManager.StorageManager.Reader.LoadUserJudgments(tUserID, ParentID)
                    var with1 = app.ActiveProject.ProjectManager;
                    with1.PipeBuilder.PipeCreated = false;
                    with1.PipeBuilder.CreatePipe();
                //}
            }
        }

        public static clsNode GetNodeByGuid(Guid nodeGuid)
        {
            clsNode node = null;

            if (!nodeGuid.Equals(new Guid()))
            {
                var App = (clsComparionCore)HttpContext.Current.Session["App"];
                node = App.ActiveProject.HierarchyAlternatives.GetNodeByID(nodeGuid);
                node = node ?? App.ActiveProject.HierarchyObjectives.GetNodeByID(nodeGuid);
            }

            return node;
        }

        public static string GetNodeTypeAndName(clsNode node, int maxLength = 40)
        {
            string nodeTypeAndName = string.Empty;

            if (node != null)
            {
                nodeTypeAndName = string.Format("{0} '{1}'", TeamTimeClass.PrepareTask(node.IsAlternative ? Consts._TPL_COMPARION_ALTS : Consts._TPL_COMPARION_OBJ), (node.NodeName.Length > maxLength ? node.NodeName.Substring(0, maxLength) : node.NodeName));
            }

            return nodeTypeAndName;
        }

        internal static bool RedirectAnonAndSignupLinks(clsComparionCore app, string passCode)
        {
            //AR Start: Code for showing RedirecToComparion when a model has insight survey page and access by anonymous or signup links
            //Ena: updated to restrict only the survey w/ alternatives or objectives type of list
            var activeProjectInsight = app.ActiveProject ?? app.DBProjectByPasscode(passCode);
            var welcomeInsightSurvey = activeProjectInsight.ProjectManager.StorageManager.Reader.IsWelcomeSurveyAvailable(activeProjectInsight.isImpact)
               && activeProjectInsight.ProjectManager.PipeParameters.ShowWelcomeSurvey;

            var thankYouInsightSurvey = activeProjectInsight.ProjectManager.StorageManager.Reader.IsThankYouSurveyAvailable(activeProjectInsight.isImpact)
              && activeProjectInsight.ProjectManager.PipeParameters.ShowThankYouSurvey;

            var newUser = IsNewUser(app);
            var hasAltOrObjQuestionType = false;

            if (activeProjectInsight.isValidDBVersion && (welcomeInsightSurvey || thankYouInsightSurvey) && newUser)
            {
                app.ActiveProject = app.DBProjectByPasscode(passCode);
                var pipeSteps = app.ActiveProject.Pipe.Count;

                for (var i = 1; i <= pipeSteps; i++)
                {
                    var anytimeAction = GetAction(i);
                    if (anytimeAction.ActionType == ActionType.atSpyronSurvey)
                    {
                        if (CheckInsightSurveyHasObjectiveOrAltenatives(app, anytimeAction))
                        {
                            hasAltOrObjQuestionType = true;
                            break;
                        }
                    }
                }
            }

            return hasAltOrObjQuestionType;
            //AR End
        }

        private static bool IsNewUser(clsComparionCore app)
        {
            var newUser = HttpContext.Current.Session["NewUser"] != null && (bool)HttpContext.Current.Session["NewUser"];

            if (!newUser && app.ActiveUser != null)
            {
                newUser = (app.ActiveUser.Created.HasValue && DateTime.Compare(app.ActiveUser.Created.Value.AddMinutes(1), DateTime.Now) > 0);
            }

            return newUser;
        }

        private static bool CheckInsightSurveyHasObjectiveOrAltenatives(clsComparionCore app, clsAction anytimeAction)
        {
            //get survey
            var userList = new Dictionary<string, clsComparionUser>();

            userList.Add(app.ActiveUser.UserEMail, new clsComparionUser
            {
                ID = app.ActiveProject.ProjectManager.UserID,
                UserName = app.ActiveProject.ProjectManager.User.UserName
            });

            var actionData = (clsSpyronSurveyAction)anytimeAction.ActionData;
            app.SurveysManager.ActiveUserEmail = app.ActiveUser.UserEMail;

            var surveyInfo = app.SurveysManager.get_GetSurveyInfoByProjectID(app.ProjectID, (SurveyType)actionData.SurveyType, userList);

            if (surveyInfo == null) return false;

            var survey = surveyInfo.get_Survey(app.ActiveUser.UserEMail);
            //check if has obj/alt list
            return survey.isSurveyContainsPipeModifiers();
        }

        internal static string GetComparionHashLink()
        {
            var context = HttpContext.Current;
            var url = HttpContext.Current.Request.Url;
            var comparionUrl = url.Host.Equals("localhost") ? $"{url.Scheme}://{url.Host}:20180/" : $"{url.Scheme}://{url.Authority}/";
            comparionUrl = comparionUrl.Replace("//r.", "//").Replace("//r-", "//");

            var hashLink = context.Session["hash"] == null ? (context.Session["hashLink"] == null ? string.Empty : context.Session["hashLink"].ToString()) : context.Session["hash"].ToString();
            context.Session["hashLink"] = hashLink;

            var isNewUser = context.Session["NewUser"] == null ? false : (bool)context.Session["NewUser"];
            context.Session["NewUser"] = isNewUser;

            if (isNewUser)
            {
                //if new user then getting user specific hash link so that it logs in automatically in non-R
                var app = (clsComparionCore)context.Session["App"];
                hashLink = GeckoClass.CreateLogonURL(app.ActiveUser, app.ActiveProject, false, "", "");
                comparionUrl += $"{hashLink}";

                hashLink = hashLink.Replace("?hash=", "");
                var hashCookie = new HttpCookie("LastHash", hashLink)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(1)
                };

                context.Response.Cookies.Add(hashCookie);
            }
            else
            {
                comparionUrl += $"?hash={hashLink}";
            }

            return $"{comparionUrl}&ignoreval=yes";
        }

        public static clsProject GetNextProject(clsProject project)
        {
            clsProject nextProject = null;

            if (project != null)
            {
                var app = (clsComparionCore)HttpContext.Current.Session["App"];
                var parameters = project.ProjectManager.Parameters;

                if (parameters.EvalOpenNextProjectAtFinish && parameters.EvalNextProjectPasscodeAtFinish != "")
                {
                    var passCode = parameters.EvalNextProjectPasscodeAtFinish.ToLower().Replace(clsProjectParametersWithDefaults.OPT_PASSCODE_RISK_CONTROLS_PIPE_PREFIX.ToLower(), "");
                    if (passCode != "")
                    {
                        nextProject = app.DBProjectByPasscode(passCode);
                        if (nextProject == null)
                        {
                            parameters.EvalOpenNextProjectAtFinish = false;
                            parameters.Save();
                        }
                        else if (nextProject.ProjectStatus != ecProjectStatus.psActive || nextProject.WorkgroupID != app.ActiveProject.WorkgroupID)
                        {
                            parameters.EvalOpenNextProjectAtFinish = false;
                            parameters.Save();
                        }
                    }
                }
            }

            return nextProject;
        }

        public static void CheckProjectIsAccessible(clsProject project, string email)
        {
            var message = GetProjectInAccessibleMessage(project, email, true);

            if (message.Length > 0)
            {
                //HttpContext.Current.Session["cannotAccessProjectMsg"] = message;
                HttpContext.Current.Session["UserSpecificHashErrorMessage"] = message;
                HttpContext.Current.Response.Redirect("~/?accError=cannotAccessProject");
            }
        }

        public static string GetProjectInAccessibleMessage(clsProject project, string email, bool canEmailEmpty)
        {
            if ((canEmailEmpty && string.IsNullOrEmpty(email)) || (bool) HttpContext.Current.Session[Constants.SessionIsPipeViewOnly])
                return string.Empty;

            email = string.IsNullOrEmpty(email) ? Guid.NewGuid().ToString() : email;

            var isPm = CanUserModifyProject(project, email);
            if (isPm) return string.Empty;

            var message = GetMessageIfProjectIsOfflineOrArchived(project);

            return message;
        }

        public static bool CanUserModifyProject(clsProject project, string email)
        {
            var app = (clsComparionCore)HttpContext.Current.Session["App"];
            var user = app.DBUserByEmail(email);

            if (project == null || user == null) return false;

            var workGroup = app.DBWorkgroupByID(project.WorkgroupID);
            var userWorkGroup = app.DBUserWorkgroupByUserIDWorkgroupID(user.UserID, workGroup.ID);
            var workSpace = app.DBWorkspaceByUserIDProjectID(user.UserID, project.ID);

            return app.CanUserModifyProject(user.UserID, project.ID, userWorkGroup, workSpace, workGroup);
        }

        public static string GetMessageIfProjectIsOfflineOrArchived(clsProject project)
        {
            var message = string.Empty;

            if (!project.isOnline)
            {
                message = string.Format(TeamTimeClass.ResString("msgAuthProjectDisabled"), project.ProjectName);
            }
            else if (project.LockInfo.LockStatus != ECLockStatus.lsUnLocked)
            {
                message = string.Format(TeamTimeClass.ResString("msgEvaluationLocked"), project.ProjectName);
            }
            else if (project.ProjectStatus == ecProjectStatus.psArchived)
            {
                message = string.Format(TeamTimeClass.ResString("msgAuthProjectReadOnly"), project.ProjectName);
            }

            return message;
        }

        public static string GetQuickHelpObjectIdFromSession()
        {
            return ((string) HttpContext.Current.Session[Constants.SessionQhNode]);
        }

        public static void SetQuickHelpObjectIdInSession(string qhObjectId)
        {
            HttpContext.Current.Session[Constants.SessionQhNode] = qhObjectId;
        }

        public static object GetSystemSettingValue(clsComparionCore app, EcSettingType settingName)
        {
            object settingValue = null;

            if (Options._Options_Individual == null || Options._Options_Individual.Count == 0)
            {
                WebOptions.OptionsIndividualInit(app);
            }

            switch (settingName)
            {
                case EcSettingType.MaxPasswordAttempts:
                    var maxPasswordAttempts = GetIntSystemSettingValue(Options._Options_Individual, "optLockPsw");
                    settingValue = maxPasswordAttempts == int.MinValue ? Consts._DEF_PASSWORD_ATTEMPTS : maxPasswordAttempts;
                    break;
                case EcSettingType.LockPasswordTimeout:
                    var tempLockValue = GetIntSystemSettingValue(Options._Options_Individual, "optLockPswTemporary");
                    if (tempLockValue == 1)
                    {
                        tempLockValue = GetIntSystemSettingValue(Options._Options_Individual, "optLockPswTimeout");
                    }
                    settingValue = tempLockValue == int.MinValue ? Consts._DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT : tempLockValue;
                    break;
            }

            return settingValue;
        }

        private static int GetIntSystemSettingValue(List<clsSetting> settings, string settingName)
        {
            int settingValue;
            var maxPasswordAttemptSetting = settings.FirstOrDefault(s => s.ResName == settingName);

            if (maxPasswordAttemptSetting == null || !int.TryParse(maxPasswordAttemptSetting.Value, out settingValue))
            {
                settingValue = int.MinValue;
            }

            return settingValue;
        }

        public static void WriteInfoToFile(string infoText, HttpServerUtility server)
        {
            try
            {
                var filePath = server.MapPath("~/App_Data/LogInfo.txt");
                //if (System.IO.File.Exists(@filePath))
                //{
                infoText += Environment.NewLine;
                System.IO.File.AppendAllText(@filePath, infoText);
                //}
            }
            catch (Exception e)
            {
                
            }
        }

        //KnowledgeOwl Help Code
        private static string _koToken = string.Empty;
        private static DateTime _koExpiresIn = DateTime.MinValue;
        public static string GetKnowledgeOwlToken(clsComparionCore app)
        {
            var context = HttpContext.Current;
            _koToken = DateTime.Compare(DateTime.Now, _koExpiresIn) >= 0 ? string.Empty : _koToken;

            if (app != null && context.Session != null && app.isAuthorized && string.IsNullOrEmpty(_koToken))
            {
                var errorString = string.Empty;
                //var email = app.ActiveUser.UserEMail;
                //var name = app.ActiveUser.UserName == "" ? app.ActiveUser.UserEMail : app.ActiveUser.UserName;
                //var groups = $"comparion,{(app.CanUserCreateNewProject(ref errorString) ? "pm" : "eval")}";

                var client = new WebClient();
                client.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{Constants.KoClientId}:{Constants.KoClientSecret}")));
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                var postData = new NameValueCollection();
                postData.Add("grant_type", "client_credentials");
                //postData.Add("reader[ssoid]", email);
                //postData.Add("reader[username]", name);
                //postData.Add("reader[Groups]", groups);

                var response = new byte[] { };

                try
                {
                    response = client.UploadValues(Constants.KoTokenUrl, "POST", postData);
                }
                catch (Exception ex)
                {
                    errorString = ex.Message;
                    var responseStream = ((WebException)ex).Response?.GetResponseStream();
                    if (responseStream != null)
                    {
                        using (var reader = new System.IO.StreamReader(responseStream))
                        {
                            errorString += $"{Environment.NewLine} {reader.ReadToEnd()}";
                        }
                    }
                }

                var responseString = response == null ? string.Empty : Encoding.ASCII.GetString(response);

                if (!string.IsNullOrEmpty(responseString))
                {
                    var authResponse = JsonConvert.DeserializeObject<KnowledgeOwlAuthToken>(responseString);
                    _koToken = string.IsNullOrEmpty(authResponse.access_token) ? string.Empty : authResponse.access_token;
                    _koExpiresIn = DateTime.Now.AddSeconds(authResponse.expires_in - 30);
                }
            }

            return _koToken;
        }
    }
}