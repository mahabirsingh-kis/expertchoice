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
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Service;
using ECWeb = ExpertChoice.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using ExpertChoice.Results;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using SpyronControls.Spyron.Core;

namespace AnytimeComparion.Pages.external_classes
{
    public class GeckoClass
    {
        //private string[][] NodeSizes = new string[3][];
        //private string _timeOutMessage;
        private const string SessionNodeSizes = "SessionNodeSizes";

        public static string timeOutMessage
        {
            get
            {
                //if(string.IsNullOrEmpty(_timeOutMessage))
                    //_timeOutMessage = TeamTimeClass.ResString("msgTimeoutGecko");
                var timeOutMessage = TeamTimeClass.ResString("msgTimeoutGecko");
                return timeOutMessage;
            }
            //set
            //{
            //    _timeOutMessage = value;
            //}
        }

        public static Object GetInfoDocData(string node_type, string node_location, int current_step, int node_id = 0, string node_guid = "" )
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            //get path
            var _ObjectType = Consts.reObjectType.Unspecified;
            var CurrentProject = App.ActiveProject;
            var tempnode = new clsNode();
     
            var sBaseURL = Consts._FILE_ROOT;
            var ObjHierarchy = (clsHierarchy)CurrentProject.HierarchyObjectives;

            clsNode ParentNode = null;
            clsNode FirstNode = null;
            clsNode SecondNode = null;

            bool is_CovObj = false;
            bool is_Multi = false;

            string path = Consts._FILE_MHT_FILES;

            clsAction tAction = null;
            var nodes = new List<object>();


            var TeamTimeData = (clsTeamTimePipe)context.Session["team"];

            if (TeamTimeClass.isTeamTime)
            {
                tAction = TeamTimeClass.GetAction(current_step);
                nodes = (List<object>)TeamTimeClass.getnodes(current_step);
            }
            else
            {
                tAction = AnytimeClass.GetAction(current_step);
                nodes = (List<object>)AnytimeClass.Getnodes(current_step);
            }

            var qh_help_id = new PipeParameters.ecEvaluationStepType();

            switch (tAction.ActionType)
            {
                case ActionType.atInformationPage:
                    clsInformationPageActionData info = (clsInformationPageActionData)tAction.ActionData;
                    if (info.Description.ToLower() == "welcome")
                    {
                        qh_help_id = PipeParameters.ecEvaluationStepType.Welcome;
                    }
                    else
                    {
                        qh_help_id = PipeParameters.ecEvaluationStepType.ThankYou;
                    }
                       
                    break;
                case ActionType.atPairwise:

                    ParentNode = (clsNode)nodes[6];
                    FirstNode = (clsNode)nodes[4];
                    SecondNode = (clsNode)nodes[5];

                    var PairwiseType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(ParentNode).ToString();
                    if (PairwiseType == "ptVerbal")
                    {
                        qh_help_id = PipeParameters.ecEvaluationStepType.VerbalPW;
                    }
                    else
                    {
                        qh_help_id = PipeParameters.ecEvaluationStepType.GraphicalPW;
                    }
                    break;
                case ActionType.atNonPWOneAtATime:
                    var temp_ActionData = (clsOneAtATimeEvaluationActionData)tAction.ActionData;
                    if (temp_ActionData.Node != null && temp_ActionData.Judgment != null)
                    {
                        var measuretype = (clsNonPairwiseEvaluationActionData)tAction.ActionData;
                        switch (((clsNonPairwiseEvaluationActionData)tAction.ActionData).MeasurementType)
                        {
                            case ECMeasureType.mtRatings:
                                qh_help_id = PipeParameters.ecEvaluationStepType.Ratings;
                                ParentNode = (clsNode)nodes[4];
                                FirstNode = (clsNode)nodes[5];
                            break;
                            case ECMeasureType.mtDirect:
                                qh_help_id = PipeParameters.ecEvaluationStepType.DirectInput;
                                ParentNode = (clsNode)nodes[4];
                                FirstNode = (clsNode)nodes[5];
                            break;
                            case ECMeasureType.mtStep:
                                qh_help_id = PipeParameters.ecEvaluationStepType.StepFunction;
                                ParentNode = (clsNode)nodes[4];
                                FirstNode = (clsNode)nodes[5];
                            break;
                            case ECMeasureType.mtRegularUtilityCurve:
                            case ECMeasureType.mtAdvancedUtilityCurve:
                                qh_help_id = PipeParameters.ecEvaluationStepType.UtilityCurve;


                                ParentNode = (clsNode)nodes[4];
                                 FirstNode = (clsNode)nodes[5];
                                break;
                        }
                    }
                    break;
                    case ActionType.atAllPairwise:
                    case ActionType.atAllPairwiseOutcomes:
                        is_Multi = true;
                        bool fIsPWOutcomes = tAction.ActionType == ActionType.atAllPairwiseOutcomes;
                        clsAllPairwiseEvaluationActionData AllPwData = (clsAllPairwiseEvaluationActionData)tAction.ActionData;
                        ParentNode = AllPwData.ParentNode;
                        bool fAlts = AllPwData.ParentNode.IsTerminalNode;
                        if (fAlts)
                        {
                            FirstNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(node_id);
                            SecondNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(node_id);
                        }
                        else
                        {
                            FirstNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(node_id);
                            SecondNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(node_id);
                        }

                        var PWType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(AllPwData.ParentNode);

                        if (PWType == CanvasTypes.PairwiseType.ptVerbal && AllPwData.Judgments.Count <= 3)
                        {
                            qh_help_id = PipeParameters.ecEvaluationStepType.VerbalPW;
                            if ((fAlts && App.ActiveProject.PipeParameters.ForceGraphicalForAlternatives) || (!fAlts && App.ActiveProject.PipeParameters.ForceGraphical))
                            {
                                PWType = CanvasTypes.PairwiseType.ptGraphical;
                                qh_help_id = PipeParameters.ecEvaluationStepType.GraphicalPW;
                            }
                        }
                    break;

                    case ActionType.atNonPWAllChildren:
                    case ActionType.atNonPWAllCovObjs:
                        is_Multi = true;
                        //node id here the index  of the nodes in multi 
                        ParentNode = (clsNode)nodes[4];
                        if (node_type != "0")
                        {
                            var temp_node_id = Guid.Empty;
                            if (node_guid != "")
                            {
                                temp_node_id = new Guid(node_guid);
                            }

                            if (temp_node_id != Guid.Empty)
                            {
                                FirstNode = CurrentProject.HierarchyAlternatives.GetNodeByID(temp_node_id);
                                if (FirstNode == null)
                                {
                                    FirstNode = CurrentProject.HierarchyObjectives.GetNodeByID(temp_node_id);
                                }
                            }

                            if (FirstNode == null)
                            {

                                FirstNode = CurrentProject.HierarchyAlternatives.GetNodeByID(node_id);
                                if (FirstNode == null)
                                {
                                    FirstNode = CurrentProject.HierarchyObjectives.GetNodeByID(temp_node_id);
                                }
                            }


                            SecondNode = FirstNode;
                        }
                        

                    clsNonPairwiseEvaluationActionData now_pw_all = (clsNonPairwiseEvaluationActionData)tAction.ActionData;
                        switch (now_pw_all.MeasurementType)
                        {
                            //multi ratings
                            case ECMeasureType.mtRatings:
                                qh_help_id = PipeParameters.ecEvaluationStepType.MultiRatings;
                            break;
                            case ECMeasureType.mtDirect:
                                qh_help_id = PipeParameters.ecEvaluationStepType.MultiDirectInput;
                            break;
                        }
                    break;
                case ActionType.atShowLocalResults:
                    is_Multi = true;
                    ParentNode = (clsNode)nodes[4];
                    qh_help_id = PipeParameters.ecEvaluationStepType.IntermediateResults;
                break;
                case ActionType.atShowGlobalResults:
                    ParentNode = (clsNode)nodes[4];
                    qh_help_id = PipeParameters.ecEvaluationStepType.OverallResults;
                break;
                case ActionType.atSpyronSurvey:
                    qh_help_id = PipeParameters.ecEvaluationStepType.Survey;
                break;
                case ActionType.atSensitivityAnalysis:
                    clsSensitivityAnalysisActionData sensitivities = (clsSensitivityAnalysisActionData)tAction.ActionData;
                    switch (sensitivities.SAType)
                    {
                        case SAType.satDynamic:
                            qh_help_id = PipeParameters.ecEvaluationStepType.DynamicSA;
                            // HelpID = 8
                            break;
                        case SAType.satGradient:
                            qh_help_id = PipeParameters.ecEvaluationStepType.GradientSA;
                            // HelpID = 9
                            break;
                        case SAType.satPerformance:
                            // HelpID = 11
                            qh_help_id = PipeParameters.ecEvaluationStepType.PerformanceSA;
                        break;
                    }
               break;
            }
            var ParentNodeID = -1;
            if (ParentNode != null)
            {
                ParentNodeID = ParentNode.NodeID;
            }
           
          
            if (node_type != "0" && node_type != "quick-help")
            {//custom node

                if (node_location == "1") //left node
                {
                    //tempnode = (clsNode)nodes[4];
                    tempnode = FirstNode;
                }
                else
                {
                    tempnode = SecondNode;
                }
    
                if(is_CovObj){
                    //multi non pairwise
                    if (node_type == "-1"){
                        tempnode = ParentNode;
                    }
                }

              


                if (node_type == "2" || (is_CovObj && node_type == "-1")) //alternative or objective
                {
                    _ObjectType = tempnode.IsAlternative ? Consts.reObjectType.Alternative : Consts.reObjectType.Node;

                }
                else
                {
                    if (node_type == "3") //wrt
                    {
                        _ObjectType = Consts.reObjectType.AltWRTNode;
                    }
                    else if (node_type == "4")
                    {
                        _ObjectType = Consts.reObjectType.MeasureScale;
                        ParentNodeID = -1;
                        tempnode = ParentNode;
                        //tempnode = qh_help_id == PipeParameters.ecEvaluationStepType.MultiRatings ? tempnode : ParentNode;
                    }
                    else //parent
                    {
                        _ObjectType = Consts.reObjectType.Node;
                        ParentNodeID = -1;
                        tempnode = ParentNode;
                    }
                }

               

                try
                {
                    if (is_CovObj && node_type == "3")
                    {
                        path = InfodocService.Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, ParentNodeID.ToString(), tempnode.NodeID);
                    }
                    else if (node_type == "4")
                    {
                        path = InfodocService.Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tempnode.MeasurementScale.GuidID.ToString(), ParentNodeID);
                    }
                    else
                    {
                        path = InfodocService.Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tempnode.NodeID.ToString(), ParentNodeID);
                    }
                    
                    
                }
                catch
                {

                }
            }
            else
            {
                if(node_type == "quick-help"){
                    string ObjID = InfodocService.GetQuickHelpObjectID(qh_help_id, ParentNode);
                    path = InfodocService.Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.QuickHelp, ObjID, -1);
                }
                else
                {
                    if (is_CovObj)
                    {
                        tempnode = (clsNode)nodes[4];
                    }
                }
            }
           
            //Object data = new {
            //    path = path,
            //    node = tempnode,
            //    object_type = _ObjectType,
            //    ParentNodeID = ParentNodeID
                                
            //};
            var test = tempnode.NodeID;
            var kur = _ObjectType;
            var l = ParentNodeID;
            var data = new object[7, 7];
            if (is_CovObj && node_type == "3")
            {
                data[0, 1] = ParentNode;
                data[0, 3] = tempnode.NodeID;

            }
            else if (is_CovObj && node_type == "0")
            {
                data[0, 1] = tempnode;
                data[0, 3] = tempnode.NodeID;
            }
            else{
               
                data[0, 1] = tempnode;
                data[0, 3] = ParentNodeID;
              
                
            }
            data[0, 0] = path;
            data[0, 2] = _ObjectType;
            data[0, 4] = is_Multi;
            data[0, 5] = is_CovObj;
            data[0, 6] = tAction.ActionType;
            return data;
        }

        public static List<KeyValuePair<string, string>> getTemplateValues()
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];

            clsAction Action = new clsAction();

            if (TeamTimeClass.isTeamTime)
            {
                Action = TeamTimeClass.GetAction(TeamTimeClass.TeamTimePipeStep);
            }
            else
            {
                var workSpace = AnytimeClass.Ws;
                var currentStep = workSpace.get_ProjectStep(App.ActiveProject.isImpact);
                currentStep = currentStep > 0 ? currentStep: 1;
                Action = AnytimeClass.GetAction(currentStep);
            }


            bool fCanBePathInteractive =  App.ActiveProject.PipeParameters.ShowFullObjectivePath == PipeParameters.ecShowObjectivePath.CollapsePath;
            Dictionary<string, string> Params = new Dictionary<string, string>();


            string[] pairwise_templates = {"%%nodename%%", "%%nodeA%%", "%%nodeB%%", "%%objective%%",
                                    "%%alternatives%%", "%%alternative%%", "%%promt_alt%%", "%%promt_obj%%",
                                    "%%promt_alt_word%%", "%%promt_obj_word%%", "%%ratewording%%", "%%estwording%%", "%%rateobjwording%%", "%%estobjwording%%"};
            string[] non_pw_templates = {"%%nodename%%", "%%evalcount%%", "%%objective%%",
                                    "%%alternatives%%", "%%alternative%%", "%%promt_alt%%", "%%promt_obj%%",
                                    "%%promt_alt_word%%", "%%promt_obj_word%%", "%%ratewording%%", "%%estwording%%", "%%rateobjwording%%", "%%estobjwording%%"};

            var is_pairwise = false;
            if (App.HasActiveProject() && Action != null && Action.ActionData != null)
            {
                clsHierarchy Hierarchy = App.ActiveProject.HierarchyObjectives;
                switch (Action.ActionType)
                {

                    case ActionType.atPairwise:
                    case ActionType.atPairwiseOutcomes:
                        is_pairwise = true;
                        clsPairwiseMeasureData Data = (clsPairwiseMeasureData)Action.ActionData;
                        clsNode parentNode = null;
                        switch (Action.ActionType)
                        {
                            case ActionType.atPairwise:
                                is_pairwise = true;
                                parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(Data.ParentNodeID);
                                Hierarchy = (clsHierarchy)(parentNode.IsTerminalNode ? App.ActiveProject.HierarchyAlternatives : App.ActiveProject.HierarchyObjectives);
                                break;
                            case ActionType.atPairwiseOutcomes:
                                is_pairwise = true;
                                parentNode = Action.ParentNode;
                                break;
                        }
                        clsNode tNodeLeft = new clsNode();
                        clsNode tNodeRight = new clsNode();

                        bool fIsPWOutcomes = Action.ActionType == ActionType.atPairwiseOutcomes;
                        // D2351
                        if (fIsPWOutcomes && parentNode != null)
                        {

                            Params.Add(ECWeb.Options._TEMPL_JUSTNODE, TeamTimeClass.GetWRTNodeNameWithPath(Action.PWONode, fCanBePathInteractive));
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
                            Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(parentNode, fCanBePathInteractive));
                        // D2830
                        if (tNodeLeft != null)
                            Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNodeLeft.NodeName));
                        if (tNodeRight != null)
                            Params.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(tNodeRight.NodeName));
                        // D2364
                        break;

                    case ActionType.atNonPWOneAtATime:
                        clsOneAtATimeEvaluationActionData data = (clsOneAtATimeEvaluationActionData)Action.ActionData;
                        if ((data != null))
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
                                        Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive));

                                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(data.Node.Children.Count));
                                        // D0558 + D2830
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

                                        // D2830
                                        Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(tAlt, fCanBePathInteractive));
                                        
                                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(tAlt.Children.Count));
                                        // D2408 ===
                                        break;
                                    // D2408 ==

                                    case ECMeasureType.mtDirect:
                                        Params.Add(ECWeb.Options._TEMPL_NODE_A, TeamTimeClass.GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive));
                                        // D2361 + D2379 + D2830
                                        // D2540 ===
                                        clsDirectMeasureData tDirect = (clsDirectMeasureData)data.Judgment;
                                        clsHierarchy tH = default(clsHierarchy);
                                        if (data.Node.IsTerminalNode)
                                            tH = App.ActiveProject.HierarchyAlternatives;
                                        else
                                            tH = App.ActiveProject.HierarchyObjectives;
                                        Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive));
                                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(data.Node.Children.Count));
                                        // D2830
                                        // D2540 ==
                                        break;
                                    case ECMeasureType.mtAdvancedUtilityCurve:
                                    case ECMeasureType.mtCustomUtilityCurve:
                                    case ECMeasureType.mtRegularUtilityCurve:
                                        clsNode tParentNode2 = (clsNode)data.Node.Hierarchy.GetNodeByID(((clsUtilityCurveMeasureData)data.Judgment).ParentNodeID);
                                        Params.Add(ECWeb.Options._TEMPL_NODE_A, TeamTimeClass.GetWRTNodeNameWithPath(tParentNode2, fCanBePathInteractive));
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
                                        Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(tAlt2, fCanBePathInteractive));
                                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(tAlt2.Children.Count));
                                        break;
                                }
                        }

                        break;
                    case ActionType.atNonPWAllChildren:
                        clsAllChildrenEvaluationActionData data2 = (clsAllChildrenEvaluationActionData)Action.ActionData;

                        if (data2 != null && data2.ParentNode != null)
                        {
                            Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(data2.ParentNode, fCanBePathInteractive && data2.ParentNode.RiskNodeType != ECTypes.RiskNodeType.ntCategory));
                            // D2830 + D2964
                            Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(data2.Children.Count));
                            // D2364
                        }
                        break;

                    case ActionType.atNonPWAllCovObjs:
                        clsAllCoveringObjectivesEvaluationActionData data3 = (clsAllCoveringObjectivesEvaluationActionData)Action.ActionData;
                        //Cv2 'C0464
                        if ((data3 != null))
                        {
                            Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(data3.Alternative, fCanBePathInteractive));
                            // D2830
                            Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(data3.CoveringObjectives.Count));
                            // D2364
                        }

                        break;

                    // D3250 ===
                    case ActionType.atAllEventsWithNoSource:
                        break;
                    // D3250 ==

                    case ActionType.atAllPairwise:
                        is_pairwise = true;
                        if ((Action.ActionData) is clsAllPairwiseEvaluationActionData)
                        {
                            clsAllPairwiseEvaluationActionData Data5 = (clsAllPairwiseEvaluationActionData)Action.ActionData;
                            bool fIsPWOutcomes2 = Action.ActionType == ActionType.atAllPairwiseOutcomes;
                            Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(Data5.ParentNode, fCanBePathInteractive));
                            // D2830
                            Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(Data5.Judgments.Count));
                            if (fIsPWOutcomes2)
                            {
                                if (Action.ParentNode != null)
                                    Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(Action.ParentNode.NodeName));
                               
                            }
                        }
                        break;
                    case ActionType.atAllPairwiseOutcomes:
                        is_pairwise = true;
                        if ((Action.ActionData) is clsAllPairwiseEvaluationActionData)
                        {
                            clsAllPairwiseEvaluationActionData Data5 = (clsAllPairwiseEvaluationActionData)Action.ActionData;
                            Params.Add(ECWeb.Options._TEMPL_NODENAME, TeamTimeClass.GetWRTNodeNameWithPath(Data5.ParentNode, fCanBePathInteractive));
                            // D2830
                            Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(Data5.Judgments.Count));
                        }

                        break;
                    case ActionType.atShowLocalResults:
                        break;


                    case ActionType.atShowGlobalResults:
                        break;

                }
            }

            //TeamTimeClass.TaskTemplates = Params;

            List<KeyValuePair<string, string>> templateValues = new List<KeyValuePair<string, string>>();
            string[] final_templates = {};

            if (is_pairwise)
            {
                final_templates = pairwise_templates;
            }
            else
            {
                final_templates = non_pw_templates;
            }

           


            for (int i = 0; i < final_templates.Length; i++)
            {

               // string sDefTask = TeamTimeClass.ResString(final_templates[i], false, false);
              //  string templateValue = TeamTimeClass.PrepareTask(StringFuncs.ParseStringTemplates(final_templates[i], Params), false, true, Params);
                string templateValue = TeamTimeClass.PrepareTask(StringFuncs.ParseStringTemplates(final_templates[i], Params), null, false, Params);
                templateValues.Insert(i, new KeyValuePair<string, string>(final_templates[i].ToString(), templateValue));
            }
            //int index = 0;
            //foreach (KeyValuePair<string, string> param in Params)
            //{
            //    string templateValue = TeamTimeClass.PrepareTask(StringFuncs.ParseStringTemplates(param.Key, Params), null, false, Params);
            //    templateValues.Insert(index, new KeyValuePair<string, string>(param.Key, templateValue));
            //    index++;
            //}

            //if (Params != null)
            //{
            //    foreach (string sKey in Params.Keys)
            //    {
            //        if (!TeamTimeClass.TaskTemplates.ContainsKey(sKey))
            //            TeamTimeClass.TaskTemplates.Add(sKey, Params[sKey]);
            //    }
            //}

            return templateValues;

        }

        internal static double GetNodePrty(clsAction action, string sNodeId)
        {
            double sValue = 0.00;
            var app = (clsComparionCore)HttpContext.Current.Session["App"];
            
            var hierarchy = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy);
            if (!string.IsNullOrEmpty(sNodeId) && true && hierarchy != null)
            {
                
                int NID = -1;
                if (int.TryParse(sNodeId, out NID))
                {
                    clsNode tNode = hierarchy.GetNodeByID(NID);
                    // D2682 ===
                    if (hierarchy.ProjectManager.IsRiskProject && hierarchy.HierarchyID == (int) ECTypes.ECHierarchyID.hidLikelihood && (tNode.get_ParentNode() == null || tNode.RiskNodeType == ECTypes.RiskNodeType.ntCategory))
                    {
                        // no need to show prty (Case 6524)
                    }
                    else
                    {
                        try
                        {
                            var globalActionData = (clsShowGlobalResultsActionData) action.ActionData;

                            //var prty = globalActionData.ResultsViewMode == Canvas.CanvasTypes.ResultsView.rvGroup
                            //    ? -1
                            //    : app.ActiveProject.ProjectManager.UserID;

                            //if (!globalActionData.get_CanShowIndividualResults(app.ActiveProject.ProjectManager.UserID))
                            //    prty = -1;

                            var UserID4Tree = app.ActiveProject.ProjectManager.UserID;

                            if (globalActionData.ResultsViewMode == Canvas.CanvasTypes.ResultsView.rvGroup || (globalActionData.ResultsViewMode == Canvas.CanvasTypes.ResultsView.rvBoth && !globalActionData.get_CanShowIndividualResults(UserID4Tree)))
                            {
                                UserID4Tree = ECTypes.COMBINED_USER_ID;
                            }

                            tNode.CalculateLocal(UserID4Tree);

                            if (tNode != null)
                                sValue = (100*tNode.get_LocalPriority(UserID4Tree));
                        }
                        catch
                        {
                            
                        }
                        // D2518
                    }
                    // D2682 ==
                }
            }
            return sValue;
        }


        public static object NodeList(List<clsNode> nodes, clsAction action, bool isChild = false)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var Level = new object[nodes.Count][];
//            var globalActionData = (clsShowGlobalResultsActionData)action.ActionData;
            for (int i = 0; i < Level.GetLength(0); i++)
            {
                var priority = GetNodePrty(action,Convert.ToString(nodes[i].NodeID));
                if (nodes[i].Children.Count > 0)
                {
                    


                    Level[i] = new object[6];
                    Level[i][0] = nodes[i].NodeID;
                    Level[i][1] = nodes[i].NodeName;
                    Level[i][2] = priority;
                    //if (isChild)
                    //{
                    //    var parent = nodes[i].get_ParentNode();
                    //    Level[i][2] = priority / (isgroup ? parent.WRTRelativeAPriority : parent.WRTGlobalPriority);
                    //}
                    Level[i][3] = 1;
                    if (App.ActiveProject.isTeamTime)
                    {
                        Level[i][4] = TeamTimeClass.TeamTime.PipeBuilder.GetFirstEvalPipeStepForNode(nodes[i], -1) + 1;
                    }
                    else
                    {
                        Level[i][4] = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(nodes[i], -1) + 1;
                    }
                    Level[i][5] = NodeList(nodes[i].Children, action, true);

                }
                else
                {
                    Level[i] = new object[5];
                    Level[i][0] = nodes[i].NodeID;
                    Level[i][1] = nodes[i].NodeName;
                    Level[i][2] = priority;
                    //if (isChild)
                    //{
                    //    var parent = nodes[i].get_ParentNode();
                    //    priority = priority / (isgroup ? parent.WRTRelativeAPriority : parent.WRTGlobalPriority);
                    //}
                    
                    if (Double.IsNaN(priority))
                        Level[i][2] = 0;
                    Level[i][3] = 1;
                    if (App.ActiveProject.isTeamTime)
                    {
                        Level[i][4] = TeamTimeClass.TeamTime.PipeBuilder.GetFirstEvalPipeStepForNode((clsNode)nodes[i]) + 1;
                    }
                    else
                    {
                        Level[i][4] = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(nodes[i], -1) + 1;
                    }
                }
            }
            return Level;
        }

        public static bool ShowInfoDocs()
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            return App.ActiveProject.ProjectManager.PipeBuilder.ShowInfoDocs;
        }


        public static List<StepsPairs> GetEvalPipeStepsList(int wrtnodeID, int CurrentStep, clsNode OutcomesNode)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            List<StepsPairs> list = new List<StepsPairs>();
            clsAction action = default(clsAction);
            for (int i = 0; i <= CurrentStep - 1; i++)
            {
                action = (clsAction)App.ActiveProject.ProjectManager.Pipe[i];
                switch (action.ActionType)
                {
                    case ActionType.atPairwise:
                    case ActionType.atPairwiseOutcomes:
                        ECCore.clsPairwiseMeasureData pwd = (ECCore.clsPairwiseMeasureData)action.ActionData;
                        if ((action.ActionType == ActionType.atPairwise & pwd.ParentNodeID == wrtnodeID) | (action.ActionType == ActionType.atPairwiseOutcomes & action.ParentNode != null && action.ParentNode.NodeID == wrtnodeID))
                        {
                            StepsPairs pair = new StepsPairs();
                            pair.Obj1 = pwd.FirstNodeID;
                            pair.Obj2 = pwd.SecondNodeID;
                            pair.Value = pwd.Value;
                            pair.Advantage = pwd.Advantage;
                            pair.IsUndefined = pwd.IsUndefined;
                            pair.StepNumber = i;
                            list.Add(pair);
                        }
                        break;
                    case ActionType.atAllPairwise:
                        clsAllPairwiseEvaluationActionData pwd2 = (clsAllPairwiseEvaluationActionData) action.ActionData;
                        if (pwd2.Judgments != null && pwd2.Judgments.Count > 0)
                        {
                            foreach (clsPairwiseMeasureData judgment in pwd2.Judgments)
                            {
                                if (judgment.ParentNodeID == wrtnodeID)
                                {
                                    StepsPairs pair = new StepsPairs();
                                    pair.Obj1 = judgment.FirstNodeID;
                                    pair.Obj2 = judgment.SecondNodeID;
                                    pair.Value = judgment.Value;
                                    pair.Advantage = judgment.Advantage;
                                    pair.IsUndefined = judgment.IsUndefined;
                                    pair.StepNumber = i;
                                    list.Add(pair);
                                }
                            }
                        }
                        break;
                    case ActionType.atAllPairwiseOutcomes:
                        clsAllPairwiseEvaluationActionData pwd3 = (clsAllPairwiseEvaluationActionData)action.ActionData;
                        if (pwd3.ParentNode.NodeID == wrtnodeID)
                        {
                            if (pwd3.Judgments != null && pwd3.Judgments.Count > 0)
                            {
                                foreach (clsPairwiseMeasureData judgment in pwd3.Judgments)
                                {
                                    StepsPairs pair = new StepsPairs();
                                    pair.Obj1 = judgment.FirstNodeID;
                                    pair.Obj2 = judgment.SecondNodeID;
                                    pair.Value = judgment.Value;
                                    pair.Advantage = judgment.Advantage;
                                    pair.IsUndefined = judgment.IsUndefined;
                                    pair.StepNumber = i;
                                    list.Add(pair);
                                }
                            }
                        }
                        break;
                }
            }

            clsNode parentNode = default(clsNode);
            if (OutcomesNode != null)
            {
                parentNode = OutcomesNode;
            }
            else
            {
                parentNode = App.ActiveProject.ProjectManager.get_Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy).GetNodeByID(wrtnodeID);
            }
            if (parentNode != null)
            {
                List<clsCustomMeasureData> judgments = default(List<clsCustomMeasureData>);
                if (OutcomesNode != null)
                {
                    judgments = parentNode.PWOutcomesJudgments.get_JudgmentsFromUser(App.ActiveProject.ProjectManager.User.UserID);
                }
                else
                {
                    judgments = parentNode.Judgments.get_JudgmentsFromUser(App.ActiveProject.ProjectManager.User.UserID);
                }
                foreach (clsCustomMeasureData J in judgments)
                {
                    if (!J.IsUndefined)
                    {
                        try 
                        { 

                            var jj = J;
                            clsPairwiseMeasureData pwData = (clsPairwiseMeasureData) jj;
                            bool exists = false;
                            foreach (StepsPairs Pair in list)
                            {
                                if ((Pair.Obj1 == pwData.FirstNodeID & Pair.Obj2 == pwData.SecondNodeID) | (Pair.Obj2 == pwData.FirstNodeID & Pair.Obj1 == pwData.SecondNodeID))
                                {
                                    exists = true;
                                }
                            }
                            if (!exists)
                            {
                                StepsPairs pair = new StepsPairs();
                                pair.Obj1 = pwData.FirstNodeID;
                                pair.Obj2 = pwData.SecondNodeID;
                                pair.Value = pwData.Value;
                                pair.Advantage = pwData.Advantage;
                                pair.IsUndefined = pwData.IsUndefined;
                                pair.StepNumber = -1;
                                list.Add(pair);
                            }
                        }
                        catch 
                        { 

                        }
                    }
                }
            }
            return list;
        }

        public static string CreateLogonURL(clsApplicationUser tUser, clsProject tProject, bool isTeamTime, string sOtherParams, string sPagePath, string sPasscode = null)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
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

                sURL += isTeamTime ? "&TTOnly=1" : "&pipe=yes";
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

        public static string[][] getInfodoc_sizes(bool isPm)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            string[][] NodeSizes = HttpContext.Current.Session[SessionNodeSizes] == null ? new string[3][] : (string[][])HttpContext.Current.Session[SessionNodeSizes];
            var getSizesFromProject = (isPm || HttpContext.Current.Session[SessionNodeSizes] == null);

            if(getSizesFromProject)
            {
                try
                {
                    var Sizes = App.ActiveProject.PipeParameters.InfoDocSize;
                    var NodeSections = Sizes.Split(';');

                    NodeSizes[0] = new string[2];
                    NodeSizes[1] = new string[2];
                    NodeSizes[2] = new string[2];
                    if (Sizes != "")
                    {
                        foreach (string NodeSection in NodeSections)
                        {
                            var nodenames = NodeSection.Split('=');
                            var nodeElements = nodenames[1].Split(':');

                            if(nodeElements[0] == null)
                            {
                                nodeElements[0] = "60%";
                            }

                            if (nodeElements[1] == null)
                            {
                                nodeElements[1] = "150";
                            }

                            switch (nodenames[0])
                            {
                                case "pw_node":
                                    NodeSizes[1][0] = nodeElements[0];
                                    NodeSizes[1][1] = nodeElements[1];
                                    break;
                                case "pw_wrt":
                                    NodeSizes[2][0] = nodeElements[0];
                                    NodeSizes[2][1] = nodeElements[1];
                                    break;
                                case "pw_goal":
                                    NodeSizes[0][0] = nodeElements[0];
                                    NodeSizes[0][1] = nodeElements[1];
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                    NodeSizes = null;
                }

                HttpContext.Current.Session[SessionNodeSizes] = NodeSizes;
            }

            return NodeSizes;
        }

        public static void setInfodoc_sizes(string width, string height, int index, bool is_multi, bool isPm)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            string[][] NodeSizes = HttpContext.Current.Session[SessionNodeSizes] == null ? new string[3][] : (string[][])HttpContext.Current.Session[SessionNodeSizes];

            //set sizes on index
            NodeSizes[index][0] = width;
            NodeSizes[index][1] = height;

            if (isPm)
            {
                var tempstring = "";
                try
                {
                    var Sizes = App.ActiveProject.PipeParameters.InfoDocSize;

                    var NodeSections = Sizes.Split(';');
                    //if (NodeSections.Length <= 3) //Volt I remove this condition and worked on hadling the duplicates below
                    //{
                    switch (index)
                    {
                        case 0:
                            Sizes += (Sizes == "" ? "" : ";") + "pw_goal=" + NodeSizes[index][0] + ":" +
                                     NodeSizes[index][1];
                            break;
                        case 1:
                            Sizes += (Sizes == "" ? "" : ";") + "pw_node=" + NodeSizes[index][0] + ":" +
                                     NodeSizes[index][1];
                            break;
                        case 2:
                            Sizes += (Sizes == "" ? "" : ";") + "pw_wrt=" + NodeSizes[index][0] + ":" +
                                     NodeSizes[index][1];
                            break;
                    }

                    //NodeSections = new string[3];
                    NodeSections = Sizes.Split(';');
                    //}

                    var pair = "";
                    var tempNodeNames = new List<string>();
                    foreach (string NodeSection in NodeSections)
                    {
                        var NodeSectionTemp = "";

                        if (NodeSection != "")
                        {
                            var nodenames = NodeSection.Split('=');
                            //var nodeElements = nodenames[1].Split(':');

                            //if duplicate do nothing
                            if (!tempNodeNames.Contains(nodenames[0]))
                            {
                                switch (nodenames[0])
                                {
                                    case "pw_node":
                                        if (index == 1)
                                        {
                                            pair = "pw_goal";
                                            NodeSectionTemp = nodenames[0] + "=" + NodeSizes[index][0] + ":" + NodeSizes[index][1];
                                            break;
                                        }

                                        NodeSectionTemp = NodeSection;
                                        break;
                                    case "pw_wrt":
                                        if (index == 2)
                                        {
                                            pair = "";
                                            NodeSectionTemp = nodenames[0] + "=" + NodeSizes[index][0] + ":" + NodeSizes[index][1];
                                            break;
                                        }

                                        NodeSectionTemp = NodeSection;
                                        break;
                                    case "pw_goal":
                                        if (index == 0)
                                        {
                                            pair = "pw_node";
                                            NodeSectionTemp = nodenames[0] + "=" + NodeSizes[index][0] + ":" + NodeSizes[index][1];
                                            break;
                                        }

                                        NodeSectionTemp = NodeSection;
                                        break;
                                }

                                if (is_multi && pair != "")
                                {
                                    tempNodeNames.Add(pair);
                                    NodeSectionTemp += ";" + pair + "=" + NodeSizes[index][0] + ":" + NodeSizes[index][1];
                                }

                                tempstring += tempstring == "" ? NodeSectionTemp : ";" + NodeSectionTemp;
                                tempNodeNames.Add(nodenames[0]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    tempstring = "pw_goal=60%;150px;pw_wrt=60%;150px;pw_goal=60%;150px";
                }

                App.ActiveProject.PipeParameters.InfoDocSize = tempstring;
                App.ActiveProject.SaveProjectOptions("Save infodoc frame settings");
            }

            HttpContext.Current.Session[SessionNodeSizes] = NodeSizes;
        }

        public static string getCommonParams(string node_params)
        {
            char[] delimiterChars = { '&' };
            string[] params_str = node_params.Split(delimiterChars);
            string tmp_str = "";
            if (params_str.Length > 0)
            {
               foreach( string s in params_str)
               {
                    if (!s.Contains('t'))
                    {
                        if (tmp_str !=  "")
                        {
                            tmp_str += "&" + s;
                        }
                        else
                        {
                            tmp_str += s;
                        }
                       
                    }
               }
            }
            return tmp_str;
        }



        public static string GetInfodocParams(Guid NodeID, Guid WRTNodeID, bool is_multi=false)
        {
            //expand
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var test = App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(Attributes.ATTRIBUTE_INFODOC_PARAMS_GECKO_ID , NodeID, WRTNodeID).ToString();

            if(test == null)
            {
                test = "";
            }
            return test;
          
        }

        public static void SetInfodocParams(Guid NodeID, Guid WRTNodeID, string value, bool is_multi = false)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];

            if (App != null)
            {
                bool isPM = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, AnytimeClass.Uw, AnytimeClass.Ws, App.ActiveWorkgroup);
                if (isPM)
                {

                    
                    //split the value params
                    var value_params = value.Split('&');

                    if (value_params.Length < 4)
                    {
                        //do not override the heading settings
                        var temp_value = GetInfodocParams(NodeID, WRTNodeID);
                        var temp_value_arr = temp_value.Split('&');
                        if (temp_value_arr.Length >= 4)
                        {
                            //get the value params but used the old heading
                            value = value_params[0] + "&" + value_params[1] + "&" + value_params[2] + "&" + temp_value_arr[3];  //pass the heading params = value_params[0] + "&" + value_params[1] + "&" + value_params[2] + "&" + temp_value_arr[3]  //pass the heading params
                        }
                    }
                    //c = 1 & h = 100 & width = 200 & s = 1; c = 1 & h = 100 & width = 200 & s = 1;

                    if (is_multi)
                    {
                        App.ActiveProject.ProjectManager.Attributes.SetAttributeValue(Attributes.ATTRIBUTE_INFODOC_PARAMS_GECKO_ID, ECTypes.UNDEFINED_USER_ID, Attributes.AttributeValueTypes.avtString, value, NodeID, WRTNodeID);
                    }
                    else
                    {
                        App.ActiveProject.ProjectManager.Attributes.SetAttributeValue(Attributes.ATTRIBUTE_INFODOC_PARAMS_GECKO_ID, ECTypes.UNDEFINED_USER_ID, Attributes.AttributeValueTypes.avtString, value, NodeID, WRTNodeID);
                    }

                   
                    App.ActiveProject.ProjectManager.Attributes.WriteAttributeValues(Attributes.AttributesStorageType.astStreamsDatabase, App.ActiveProject.ProjectManager.StorageManager.ProjectLocation, App.ActiveProject.ProjectManager.StorageManager.ProviderType, App.ActiveProject.ProjectManager.StorageManager.ModelID, ECTypes.UNDEFINED_USER_ID);
                }
            }
        }


        public static string GetWelcomeThankYouIncFile(bool fIsThankYou, bool fIsImpact, bool fIsOpportunity)
        {
            // D3326
                var App = (clsComparionCore)HttpContext.Current.Session["App"];
                string sName = "";
                if (App.isRiskEnabled)
                {
                        if (fIsThankYou)
                        {
                            // D3326
                            if (fIsOpportunity)
                            {
                                sName = Convert.ToString((fIsImpact ? Consts._FILE_TEMPL_THANKYOU_IMPACT_OPPORTUNITY : Consts._FILE_TEMPL_THANKYOU_LIKELIHOOD_OPPORTUNITY));
                                // D3326
                            }
                            else
                            {
                                sName = Convert.ToString((fIsImpact ? Consts._FILE_TEMPL_THANKYOU_IMPACT : Consts._FILE_TEMPL_THANKYOU_LIKELIHOOD));
                            }
                        }
                        else
                        {
                            // D3326
                            if (fIsOpportunity)
                            {
                                sName = Convert.ToString((fIsImpact ? Consts._FILE_TEMPL_WELCOME_IMPACT_OPPORTUNITY : Consts._FILE_TEMPL_WELCOME_LIKELIHOOD_OPPORTUNITY));
                                // D3326
                            }
                            else
                            {
                                sName = Convert.ToString((fIsImpact ? Consts._FILE_TEMPL_WELCOME_IMPACT : Consts._FILE_TEMPL_WELCOME_LIKELIHOOD));
                            }
                        }
                    
                }
                else
                {
                    // D3326
                    if (fIsOpportunity)
                    {
                        sName = Convert.ToString((fIsThankYou ? Consts._FILE_TEMPL_THANKYOU_OPPORTUNITY : Consts._FILE_TEMPL_WELCOME_EVALUATE_OPPORTUNITY));
                        // D3326
                    }
                    else
                    {
                        sName = Convert.ToString((fIsThankYou ? Consts._FILE_TEMPL_THANKYOU : Consts._FILE_TEMPL_WELCOME_EVALUATE));
                    }
                }
                sName = GetIncFile(sName);
                if (App.isRiskEnabled && !System.IO.Directory.Exists(sName))
                    sName = GetIncFile(Convert.ToString((fIsThankYou ? Consts._FILE_TEMPL_THANKYOU : Consts._FILE_TEMPL_WELCOME_EVALUATE)));
                return sName;
            
        }

        public static string GetIncFile(string sFilename)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            string sPath = Consts._FILE_DATA_INC + App.LanguageCode + "\\" + sFilename;
            // D2325
            if (System.IO.File.Exists(sPath))
                return sPath;

            sPath = Consts._FILE_DATA_INC + Consts._LANG_DEFCODE + "\\" + sFilename;
            // D2325
            if (System.IO.File.Exists(sPath))
                return sPath;
            else
                return Consts._FILE_DATA_INC + sFilename;
        }



        //public clsProject CurrentProject
        //{
        //    get
        //    {

        //        if (_Project == null)
        //        {
        //            string sAction = CheckVar(_PARAM_ACTION, "").ToLower;
        //            // D0130

        //            switch (sAction)
        //            {
        //                // D0130

        //                case _ACTION_NEW.ToLower:
        //                case "upload":
        //                case "copy":
        //                case "usetemplate":
        //                    // D0324

        //                    _Project = new clsProject(false, App.ECWeb.Options.ProjectForceAllowedAlts, App.ActiveUser.UserEmail, App.isRiskEnabled, App.onProjectSavingEvent, App.onProjectSavedEvent);
        //                    // D0183 + D0245 + D0315 + D2255 + D3571
        //                    _Project.ProviderType = App.DefaultProvider;
        //                    // D0329 + D0330
        //                    //_Project.ProjectParticipating = ecProjectParticipating.ppOffline   ' D0300 -D0748
        //                    _Project.isOnline = false;
        //                    // D0300 + D0748

        //                    //If Not _CanBeOnline Then _Project.ProjectParticipating = ecProjectParticipating.ppOffline  ' -D0748

        //                    _Project.PasscodeLikelihood = App.ProjectUniquePasscode("", -1);
        //                    // D0286 + D1709
        //                    _Project.PasscodeImpact = App.ProjectUniquePasscode("", -1);
        //                    // D1709
        //                    //If Not App.isUniquePasscode(_Project.Passcode, -1) Then _Project.Passcode = GetRandomString(_DEF_PASSCODE_LENGTH, True, False) ' D0178 + D0286 + D0443 -D1709
        //                    _Project.WorkgroupID = App.ActiveWorkgroup.ID;
        //                    _Project.OwnerID = App.ActiveUser.UserID;

        //                    if (_isUpload)
        //                    {
        //                        _Project.isLoadOnDemand = App.ECWeb.Options.ProjectLoadOnDemand;
        //                        // D0183 + D0315
        //                        CurrentPageID = _PGID_PROJECT_UPLOAD;
        //                        rowFile.Visible = true;
        //                        rowFilename.Visible = false;
        //                        cbStrongPasswords.Visible = true;
        //                        // D0272
        //                    }
        //                    else
        //                    {
        //                        // D0130 ===
        //                        // D0324
        //                        if (sAction == "copy" | sAction == "usetemplate")
        //                        {
        //                            CurrentPageID = _PGID_PROJECT_COPY;
        //                            clsProject SrcProject = clsProject.ProjectByID(App.ProjectID, App.ActiveProjectsList);
        //                            // D0479
        //                            if ((SrcProject != null))
        //                            {
        //                                //_Project.FileName = SrcProject.FileName    ' -D1193
        //                                _Project.Comment = SrcProject.Comment;
        //                                if (sAction == "copy")
        //                                    _Project.ProjectName = string.Format("{0} ({1} {2})", SrcProject.ProjectName, Now.ToShortDateString, Now.ToShortTimeString);
        //                                else
        //                                    _Project.ProjectName = SrcProject.ProjectName;
        //                                // D0324 + D0842
        //                                //If sAction = "copy" Then _Project.ProjectName = String.Format("{0} ({1})", SrcProject.ProjectName, Now.ToString("yyMMdd-HHmmss")) Else _Project.ProjectName = SrcProject.ProjectName ' D0324
        //                                _Project.StatusDataLikelihood = SrcProject.StatusDataLikelihood;
        //                                // D1944
        //                                _Project.StatusDataImpact = SrcProject.StatusDataImpact;
        //                                // D1944
        //                                if (sAction == "usetemplate")
        //                                    _Project.ProjectStatus = ecProjectStatus.psActive;
        //                                // D0324
        //                                //_Project.ProjectParticipating = ecProjectParticipating.ppOffline   ' D0272 + D0300 -D0748
        //                                _Project.isOnline = false;
        //                                // D0272 + D0300 + D0748
        //                                _Project.isPublic = false;
        //                                // D0748
        //                                _Project.PipeParameters.StartDate = SrcProject.PipeParameters.StartDate;
        //                                // D0183
        //                                _Project.PipeParameters.EndDate = SrcProject.PipeParameters.EndDate;
        //                                // D0183
        //                            }
        //                            // D0324 ===
        //                            if (sAction == "copy")
        //                            {
        //                                cbCopyUsers.Visible = true;
        //                                // D2479 ===
        //                                rbSaveAsTemplate.Visible = true;
        //                                // D0206
        //                                if (rbSaveAsTemplate.Items.Count == 0)
        //                                {
        //                                    rbSaveAsTemplate.Items.Add(new ListItem(ResString("lblSaveAsCopy"), 0));
        //                                    rbSaveAsTemplate.Items.Add(new ListItem(ResString("lblSaveAsTemplate"), 1));
        //                                    rbSaveAsTemplate.Items.Add(new ListItem(ResString("lblSaveAsMasterProject"), 2));
        //                                    //rbSaveAsTemplate.Attributes.Add("onchange", "CheckSaveAsTemplate()")
        //                                    rbSaveAsTemplate.Attributes.Add("onclick", "CheckSaveAsTemplate()");
        //                                    rbSaveAsTemplate.SelectedValue = 0;
        //                                }
        //                                // D2479 ==
        //                            }
        //                            // D0324 ==
        //                        }
        //                        else
        //                        {
        //                            CurrentPageID = _PGID_PROJECT_CREATE;
        //                            LoadXMLPipeParams(_Project);
        //                            // D0133
        //                            //_Project.ProjectParticipating = ecProjectParticipating.ppOffline    ' D0418
        //                            _Project.isOnline = false;
        //                            // D0418 + D0748
        //                        }
        //                        // D0130 ==
        //                        rowFilename.Visible = false;
        //                        rowFile.Visible = false;
        //                    }

        //                    break;
        //                default:
        //                    _Project = clsProject.ProjectByID(App.ProjectID, App.ActiveProjectsList);
        //                    // D0479
        //                    if (_Project == null)
        //                        FetchAccess();
        //                    CurrentPageID = _PGID_PROJECT_PROPERTIES;
        //                    // D0345 ===
        //                    switch (sAction)
        //                    {
        //                        case "copy":
        //                            CurrentPageID = _PGID_PROJECT_COPY;
        //                            // D0130
        //                            break;
        //                        case "template":
        //                            CurrentPageID = _PGID_PROJECT_CREATE_FROM_TPL;
        //                            // D0324
        //                            break;
        //                        case "delete":
        //                            CurrentPageID = _PGID_PROJECT_DELETE;
        //                            break;
        //                        case "undelete":
        //                            CurrentPageID = _PGID_PROJECT_UNDELETE;
        //                            // D0789
        //                            break;
        //                    }
        //                    // D0345 ==
        //                    rowFile.Visible = false;
        //                    rowFilename.Visible = true;
        //                    break;
        //            }

        //        }
        //        return _Project;
        //    }
        //    set { _Project = value; }
        //}

        //public void uploadFile()
        //{
        //    var App = (clsComparionCore)HttpContext.Current.Session["App"];
        //    var CurrentProject = App.ActiveProject;
        //    if (CurrentProject == null)
        //        return;
        //    // D0134
        //    if (!CurrentProject.LockInfo.isLockAvailable(App.ActiveUser))
        //        return;
        //    // D0487 + D0589
        //    clsProjectLockInfo tLock = App.DBProjectLockInfoRead(CurrentProject.ID);
        //    bool fKeepEditor = true;
        //    bool fUpdated = false;
        //    bool fShowWarningOnUpload = false;
        //    // D0690
        //    bool fReloadPage = false;
        //    // D0302
        //    //lblError.Text = "";
        //    //lblError.Visible = false;

        //    string sURL = "";
        //    // D0255

        //    // D0893 === -- THIS IS FOR REPLACING PROJECT THAT EXIST
        //    //if (btnOK.CommandName == "replace")
        //    //{
        //    //    int PrjID = -1;
        //    //    if (App.HasActiveProject && int.TryParse(CheckVar("prjid", ""), PrjID))
        //    //    {
        //    //        clsProject tPrj = clsProject.ProjectByID(PrjID, App.ActiveProjectsList);
        //    //        if (tPrj != null && tPrj.ID != App.ProjectID)
        //    //        {
        //    //            tPrj.ProjectGUID = Guid.NewGuid;
        //    //            tPrj.ReplacedID = App.ActiveProject.ID;
        //    //            tPrj.isMarkedAsDeleted = true;
        //    //            tPrj.LastModify = Now;
        //    //            App.ActiveProject.PasscodeLikelihood = tPrj.PasscodeLikelihood;
        //    //            App.ActiveProject.PasscodeImpact = tPrj.PasscodeImpact;
        //    //            tPrj.PasscodeLikelihood = App.ProjectUniquePasscode("", -1);
        //    //            // D1709
        //    //            tPrj.PasscodeImpact = App.ProjectUniquePasscode("", -1);
        //    //            // D1709
        //    //            App.DBProjectUpdate(tPrj, false, string.Format("Replace project with new version ('{0}', #{1})", App.ActiveProject.Passcode, App.ActiveProject.ID));
        //    //            App.DBProjectUpdate(App.ActiveProject, false, "Set access code from replaced decision");
        //    //        }
        //    //        if (!string.IsNullOrEmpty(btnOK.CommandArgument))
        //    //            sURL = btnOK.CommandArgument;
        //    //        if (string.IsNullOrEmpty(sURL))
        //    //            sURL = PageURL(_DEF_PGID_ONSELECTPROJECT);
        //    //        Response.Redirect(sURL, true);
        //    //    }
        //    //}
        //    // D0893 ==

        //    //if (!tbPasscode.Enabled)
        //    //    tbPasscode.Text = CurrentProject.Passcode;
        //    // D0302
        //    string sIsRisk = "";
        //    // D2646

        //    switch ("New")
        //    {

        //        //case _ACTION_EDIT:
        //        //case "":
        //        //    if (!App.isUniquePasscode(tbPasscode.Text, CurrentProject.ID))
        //        //        tbPasscode.Text = CurrentProject.Passcode;
        //        //    // D0178 + D0443
        //        //    SetOptions(CurrentProject);
        //        //    if (App.DBProjectUpdate(CurrentProject, false, "Edit Project Properties"))
        //        //    {
        //        //        CurrentProject.SaveProjectOptions();
        //        //        // D0127
        //        //        fUpdated = true;
        //        //        fKeepEditor = true;
        //        //        // D0136
        //        //        fReloadPage = true;
        //        //        // D0302
        //        //    }

        //        //    break;
        //        //// D0130 ===
        //        //case "copy":
        //        //case "usetemplate":
        //        //    // D0323
        //        //    SetOptions(CurrentProject);
        //        //    clsProject SrcProject = clsProject.ProjectByID(App.ProjectID, App.ActiveProjectsList);
        //        //    // D0479
        //        //    string sError = "";

        //        //    if ((SrcProject != null) & App.DBProjectCreate(CurrentProject))
        //        //    {
        //        //        // D0183 ===
        //        //        //C0278
        //        //        if (App.DBProjectCopy(SrcProject, ECModelStorageType.mstCanvasStreamDatabase, CurrentProject.ConnectionString, CurrentProject.ProviderType, CurrentProject.ID))
        //        //        {
        //        //            CurrentProject.ResetProject();
        //        //            SetOptions(CurrentProject);
        //        //            CurrentProject.SaveProjectOptions("Save option on project copy");
        //        //        }
        //        //        // D0183 ==
        //        //        //' D0183 ==

        //        //        // D0397 ===
        //        //        bool fSaveAsTemplate = rbSaveAsTemplate.Visible & rbSaveAsTemplate.Enabled & rbSaveAsTemplate.SelectedValue.ToString != "0";
        //        //        // D2479
        //        //        bool fCopyUsers = cbCopyUsers.Enabled & cbCopyUsers.Checked & cbCopyUsers.Visible;

        //        //        if (fSaveAsTemplate | !fCopyUsers)
        //        //        {
        //        //            List<clsUser> UsersOrig = CurrentProject.ProjectManager.UsersList;
        //        //            //C0385
        //        //            List<clsUser> Users = new List<clsUser>();
        //        //            Users.AddRange(UsersOrig.ToArray);
        //        //            DebugInfo("Start for delete users");
        //        //            // D0827
        //        //            foreach (clsUser tUser in Users)
        //        //            {
        //        //                CurrentProject.ProjectManager.DeleteUser(tUser);
        //        //            }
        //        //            CurrentProject.ProjectManager.StorageManager.Writer.DeleteCalculatedWeights();
        //        //            CurrentProject.ProjectManager.StorageManager.Writer.DeleteAllCombinedJudgments();
        //        //            DebugInfo("Users deleted");
        //        //            // D0827
        //        //        }
        //        //        // D0397 ==

        //        //        // D0206 ===
        //        //        // D0324 + D0397
        //        //        if (fSaveAsTemplate)
        //        //        {
        //        //            App.AttachProject(App.ActiveUser, CurrentProject, false, App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager), "", false);
        //        //            // D2287 + D2780
        //        //            bool fIsMaster = rbSaveAsTemplate.SelectedValue.ToString == "2";
        //        //            // D2479
        //        //            if (fIsMaster)
        //        //                CurrentProject.ProjectStatus = ecProjectStatus.psMasterProject;
        //        //            else
        //        //                CurrentProject.ProjectStatus = ecProjectStatus.psTemplate;
        //        //            // D0300 + D2479
        //        //            //CurrentProject.ProjectParticipating = ecProjectParticipating.ppOffline     ' D0300 -D0748
        //        //            CurrentProject.isOnline = false;
        //        //            // D0300 + D0748
        //        //            CurrentProject.isPublic = false;
        //        //            // D0748
        //        //            App.DBProjectUpdate(CurrentProject, false, (fIsMaster ? "Mark project as Master" : "Mark project as template"));
        //        //            // D2479
        //        //            CurrentProject.SaveStructure(string.Format("Save data ({0})", CurrentProject.ProjectStatus.ToString));
        //        //            sURL = PageURL((fIsMaster ? _PGID_PROJECTSLIST_MASTERPROJECTS : _PGID_PROJECTSLIST_TEMPLATES));
        //        //            // D2479
        //        //        }
        //        //        else
        //        //        {
        //        //            //D0206 ==
        //        //            // D0131 ===
        //        //            // D0479
        //        //            foreach (clsWorkspace tWS in App.DBWorkspacesByProjectID(SrcProject.ID))
        //        //            {
        //        //                // D0206 + D0397
        //        //                if (tWS.UserID == App.ActiveUser.UserID | fCopyUsers)
        //        //                {
        //        //                    clsWorkspace tNewWS = tWS.Clone;
        //        //                    tNewWS.ProjectID = CurrentProject.ID;
        //        //                    // D0674 ===
        //        //                    string sMsg = string.Format("Copy user to project '{0}'", CurrentProject.Passcode);
        //        //                    if (tWS.UserID == App.ActiveUser.UserID)
        //        //                    {
        //        //                        tNewWS.GroupID = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager);
        //        //                        // D2780
        //        //                        sMsg = string.Format("Attach Project Owner '{0}' to project '{1}'", App.ActiveUser.UserEmail, CurrentProject.Passcode);
        //        //                    }
        //        //                    // D0674 ==
        //        //                    // D0190 ===
        //        //                    //Dim tUser As clsApplicationUser = App.User(tWS.UserID)
        //        //                    //Dim sUser As String = ""
        //        //                    //If Not tUser Is Nothing Then sUser = tUser.UserEmail
        //        //                    //App.DB_WorkspaceUpdate(tNewWS, True, String.Format("Copy user {0} workspace", sUser), CurrentProject.Passcode, String.Format("Source Project: '{0}'", SrcProject.Passcode))
        //        //                    App.DBWorkspaceUpdate(tNewWS, true, sMsg);
        //        //                    // D0479
        //        //                    App.DBSaveLog(dbActionType.actCreate, dbObjectType.einfWorkspace, tNewWS.ID, sMsg, "", App.ActiveUser.UserID, App.ActiveWorkgroup.ID);
        //        //                    // D0674
        //        //                    // D0190 ==
        //        //                }
        //        //            }
        //        //            // D0131 ==
        //        //            // D0821 ===
        //        //            CurrentProject.Created = SrcProject.Created;
        //        //            CurrentProject.LastModify = SrcProject.LastModify;
        //        //            List<object> tParams = new List<object>();
        //        //            tParams.Add(CurrentProject.Created);
        //        //            tParams.Add(CurrentProject.LastModify);
        //        //            tParams.Add(CurrentProject.ID);
        //        //            App.Database.ExecuteSQL(string.Format("UPDATE {0} SET {1}=?, {2}=? WHERE {3}=?", clsComparionCore._TABLE_PROJECTS, "Created", clsComparionCore._FLD_PROJECTS_LASTMODIFY, clsComparionCore._FLD_PROJECTS_ID), tParams);
        //        //            // D0491
        //        //            // D0821 ==
        //        //        }

        //        //        fUpdated = true;
        //        //        fKeepEditor = false;
        //        //        sError = "";
        //        //    }
        //        //    else
        //        //    {
        //        //        lblError.Text = ResString("errCreateProject");
        //        //        lblError.Visible = true;
        //        //        fKeepEditor = true;
        //        //    }
        //        //    break;
        //        // D0130 ==

        //        //case "New":
        //        //    // BELOW IS FOR CHECKING EMAIL BEFORE UPLOAD
        //        //    //DebugCheckEmailsBeforeUpload = cbCheckEmailBeforeUpload.Checked;

        //        //    //SetOptions(CurrentProject);
        //        //    if (!App.DBProjectCreate(ref CurrentProject, "Create empty project"))
        //        //    {
        //        //        lblError.Text = ResString("errCreateProject");
        //        //        //If App.Database.LastError <> "" Then lblError.Text += String.Format("<div style='margin-top:1em'>Details: {0}</div>", App.Database.LastError)
        //        //        lblError.Visible = true;
        //        //        fKeepEditor = true;
        //        //    }
        //        //    else
        //        //    {
        //        //        //CurrentProject.ProjectManager.Initialize() 'C20070731 'C0275
        //        //        CurrentProject.ProjectManager.User = App.ActiveUser;
        //        //        //C0275
        //        //        //CurrentProject.SaveStructure() 'C20070731 'C0275
        //        //        App.ImportProjectUsers(CurrentProject, App.ActiveUser, !cbStrongPasswords.Checked, cbCheckEmailBeforeUpload.Checked);
        //        //        // D0272 + D0345 + D0479 + D1985
        //        //        App.CheckAndAddReviewAccount(ReviewAccount, CurrentProject);
        //        //        // D1382 + D1408
        //        //        LoadXMLPipeParams(_Project);
        //        //        // D0133
        //        //        CurrentProject.SaveProjectOptions("Load settings from XML file");
        //        //        // D0127
        //        //        fKeepEditor = false;
        //        //        fUpdated = true;
        //        //    }

        //        //    break;
        //        case "upload":

        //            if (FileUploadProject.HasFile)
        //            {
        //                DebugCheckEmailsBeforeUpload = cbCheckEmailBeforeUpload.Checked;
        //                // D1016
        //                if (cbIgnoreTimePeriods.Visible)
        //                    IgnoreTimePeriods = cbIgnoreTimePeriods.Checked;
        //                // D3575

        //                string sUploadedFileName = File_CreateTempName();
        //                // D0132
        //                FileUploadProject.SaveAs(sUploadedFileName);

        //                string sError = "";
        //                bool fHasError = false;
        //                string sOriginalFName = Path.GetFileName(FileUploadProject.FileName).ToLower;
        //                // D0387

        //                App.DBSaveLog(dbActionType.actCreate, dbObjectType.einfProject, -1, "Upload started", string.Format("File: '{0}' ({1} bytes)", sOriginalFName, My.Computer.FileSystem.GetFileInfo(sUploadedFileName).Length));
        //                // D3113

        //                // D0298 ===
        //                if (isSupportedArchive(FileUploadProject.FileName))
        //                {
        //                    ArrayList ExtList = new ArrayList();
        //                    ExtList.Add(_FILE_EXT_AHP.ToLower);
        //                    ExtList.Add(_FILE_EXT_AHPX.ToLower);
        //                    ExtList.Add(_FILE_EXT_AHPS.ToLower);
        //                    // D0378
        //                    ExtList.Add(_FILE_EXT_TXT.ToLower);
        //                    // D2132
        //                    string sExtractedFile = ExtractArchiveForFile(App, FileUploadProject.FileName, sUploadedFileName, ExtList, CurrentProject.Passcode, sError, sOriginalFName);
        //                    // D0505
        //                    if (string.IsNullOrEmpty(sError) & !string.IsNullOrEmpty(sExtractedFile))
        //                    {
        //                        sUploadedFileName = sExtractedFile;
        //                    }
        //                    else
        //                    {
        //                        fHasError = true;
        //                    }
        //                    ExtList = null;
        //                }
        //                // D0298 ==

        //                string FileConnString = clsConnectionDefinition.BuildJetConnectionDefinition(sUploadedFileName, DBProviderType.dbptODBC).ConnectionString;
        //                // D0315 + D0329 + D0330 + MF0332 + D0459

        //                // D0378 ===
        //                //Dim isCanvasProject As Boolean = MiscFuncs.IsCanvasDatabase(FileConnString, DBProviderType.dbptODBC)    ' D0329

        //                // D0387 ===
        //                ECModelStorageType fStorageType = default(ECModelStorageType);
        //                switch (Path.GetExtension(sOriginalFName).ToLower)
        //                {
        //                    case _FILE_EXT_AHP:
        //                        fStorageType = ECModelStorageType.mstAHPDatabase;
        //                        break;
        //                    case _FILE_EXT_AHPX:
        //                        fStorageType = ECModelStorageType.mstCanvasDatabase;
        //                        break;
        //                    case _FILE_EXT_AHPS:
        //                        fStorageType = ECModelStorageType.mstAHPSStream;
        //                        break;
        //                    case _FILE_EXT_TXT:
        //                        // D2132
        //                        fStorageType = ECModelStorageType.mstTextFile;
        //                        // D2132
        //                        break;
        //                }
        //                // D0387 ==

        //                string sTextModel = "";
        //                // D2132

        //                if (!fHasError)
        //                {
        //                    switch (fStorageType)
        //                    {
        //                        // D0387
        //                        case ECModelStorageType.mstAHPSStream:
        //                            // D0387
        //                            IO.FileStream AHPStream = new IO.FileStream(sUploadedFileName, FileMode.Open, FileAccess.Read);
        //                            fHasError = !MiscFuncs.IsAHPSStream(AHPStream);
        //                            // D2753 ===
        //                            if (fHasError)
        //                            {
        //                                sError = ResString("errWrongUploadFile");
        //                            }
        //                            else
        //                            {
        //                                AHPStream.Seek(0, SeekOrigin.Begin);
        //                                ECCanvasDatabaseVersion DBVersion = MiscFuncs.GetProjectVersion_AHPSStream(AHPStream);
        //                                if (DBVersion.MajorVersion * 10000 + DBVersion.MinorVersion > GetCurrentDBVersion.MajorVersion * 10000 + GetCurrentDBVersion.MinorVersion)
        //                                {
        //                                    fHasError = true;
        //                                    sError = string.Format(ResString("msgWrongProjectDBVersion"), DBVersion.GetVersionString, GetCurrentDBVersion.GetVersionString);
        //                                }
        //                            }
        //                            AHPStream.Close();
        //                            break;
        //                        // D2753 ==

        //                        case ECModelStorageType.mstAHPDatabase:
        //                            // D0387
        //                            // D0240
        //                            if (_DB_CHECK_AHP_VERSION & !fHasError)
        //                            {
        //                                float AHP_DBVer = GetAHPDBVersion(FileConnString, DBProviderType.dbptODBC);
        //                                // D0329
        //                                //If AHP_DBVer < AHP_DB_DEFAULT_VERSION Then 'AS/om0010=== 'AS/10-8-15=== put back 'AS/11-17-15===
        //                                //    fHasError = True
        //                                //    'sError = String.Format(ResString("errWrongAHPVersion"), AHP_DB_DEFAULT_VERSION) 'AS/11-10-15
        //                                //    sError = String.Format(ResString("errOldAHPVersion"), AHP_DB_DEFAULT_VERSION) 'AS/11-10-15
        //                                //Else
        //                                //    CheckOld_AHP(FileConnString, DBProviderType.dbptODBC, AHP_DBVer, sError)    ' D2751
        //                                //End If 'AS/om0010== 'AS/10-8-15==
        //                                //'If AHP_DBVer < AHP_DB_LATEST_VERSION Then 'AS/om0010=== 'AS/10-8-15===
        //                                //'    CheckOld_AHP(FileConnString, DBProviderType.dbptODBC, AHP_DBVer, sError)
        //                                //'End If 'AS/om0010== 'AS/10-8-15== 'AS/11-17-15==

        //                                //AS/11-17-15===
        //                                if (AHP_DBVer < AHP_DB_DEFAULT_VERSION)
        //                                {
        //                                    fHasError = true;
        //                                    sError = string.Format(ResString("errOldAHPVersion"), AHP_DB_DEFAULT_VERSION);
        //                                }
        //                                else if (AHP_DBVer < AHP_DB_LATEST_VERSION)
        //                                {
        //                                    //CheckOld_AHP(FileConnString, DBProviderType.dbptODBC, AHP_DBVer, sError) 'AS/6-28-16
        //                                    string AHP_LastUploadToECC = GetAHPLastUploadToECC(FileConnString, DBProviderType.dbptODBC);
        //                                    //AS/6-28-16===
        //                                    bool AHP_ChangedInECD = (Strings.Len(AHP_LastUploadToECC) > 0 ? true : false);
        //                                    CheckOld_AHP(FileConnString, DBProviderType.dbptODBC, AHP_DBVer, AHP_ChangedInECD, sError);
        //                                    //AS/6-28-16==
        //                                }
        //                                //AS/11-17-15==
        //                            }

        //                            //AS/4-29-16===
        //                            if (IgnoreTimePeriods)
        //                            {
        //                                clsTranslateDBversion TDBV = new clsTranslateDBversion(DBProviderType.dbptODBC, FileConnString);
        //                                if (!TDBV.TimePeriodsRemoved())
        //                                {
        //                                    Interaction.MsgBox("Time Periods not removed");
        //                                    return;
        //                                }
        //                            }
        //                            //AS/4-29-16==
        //                            break;

        //                        case ECModelStorageType.mstCanvasDatabase:
        //                            // D0387
        //                            fHasError = !MiscFuncs.IsCanvasDatabase(FileConnString, DBProviderType.dbptODBC);
        //                            if (fHasError)
        //                                sError = ResString("errWrongUploadFile");

        //                            break;
        //                        // D2132 ===
        //                        case ECModelStorageType.mstTextFile:
        //                            try
        //                            {
        //                                sTextModel = My.Computer.FileSystem.ReadAllText(sUploadedFileName);
        //                                fHasError = !clsTextModel.isValidContent(sTextModel, sError);
        //                                // D2133 ===
        //                                if (!fHasError)
        //                                {
        //                                    System.Security.Cryptography.MD5CryptoServiceProvider objMD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        //                                    byte[] arrData = System.Text.Encoding.UTF8.GetBytes(sTextModel.Length.ToString + SubString(sTextModel, 1024));
        //                                    byte[] arrHash = objMD5.ComputeHash(arrData);
        //                                    if (arrHash.GetLength(0) != 16)
        //                                        Array.Resize(arrHash, 16);
        //                                    CurrentProject.ProjectGUID = new Guid(arrHash);
        //                                }
        //                                // D2133 ==
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                fHasError = true;
        //                                sError = "Error on read uploaded data";
        //                            }
        //                            break;
        //                            // D2132 ==
        //                    }
        //                }

        //                // D0157 ===
        //                // D0345
        //                if (!fHasError)
        //                {
        //                    fHasError = !App.CheckUsersListBeforeUpload(sUploadedFileName, FileConnString, DBProviderType.dbptODBC, fStorageType, cbCheckEmailBeforeUpload.Checked, sError, IgnoreLicenseWarning.Value != FileUploadProject.FileName);
        //                    // D0172 + D0329 + D0345 + D0503 + D1226 + D1519
        //                }
        //                // D0378 ==

        //                // D0925 ===
        //                if (!fHasError && App.ActiveWorkgroup != null && App.ActiveWorkgroup.License.isValidLicense)
        //                {
        //                    long ObjMax = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxObjectives);
        //                    long AltMax = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxAlternatives);
        //                    long LevelMax = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxLevelsBelowGoal);

        //                    if (ObjMax != UNLIMITED_VALUE | AltMax != UNLIMITED_VALUE | LevelMax != UNLIMITED_VALUE)
        //                    {
        //                        // D2133 ===
        //                        int ObjsCount = 1;
        //                        int AltsCount = 0;
        //                        int LevelsCount = 1;

        //                        switch (fStorageType)
        //                        {
        //                            case ECModelStorageType.mstTextFile:
        //                                clsProject tmpPrj = new clsProject(true, false, App.ActiveUser.UserEmail, App.isRiskEnabled);
        //                                // D2255
        //                                clsTextModel.ReadModel(tmpPrj, sTextModel, App.isRiskEnabled, sError);
        //                                // D2426
        //                                // D3306
        //                                if (string.IsNullOrEmpty(sError))
        //                                {
        //                                    var _with1 = tmpPrj.ProjectManager;
        //                                    if (_with1.HierarchyCount > 0)
        //                                    {
        //                                        ObjsCount = _with1.Hierarchy(_with1.ActiveHierarchy).Nodes.Count;
        //                                        LevelsCount = _with1.Hierarchy(_with1.ActiveHierarchy).GetMaxLevel();
        //                                    }
        //                                    if (_with1.AltsHierarchyCount > 0)
        //                                        AltsCount = _with1.AltsHierarchy(_with1.ActiveAltsHierarchy).Nodes.Count;
        //                                    _with1.CloseProject();
        //                                }
        //                                tmpPrj = null;

        //                                break;
        //                            default:
        //                                dynamic tmpPrjManager = new clsCanvasProjectManager(true, false, App.isRiskEnabled);
        //                                // D2255
        //                                var _with2 = tmpPrjManager;
        //                                _with2.ForceAllowedPermissions = App.ECWeb.Options.ProjectForceAllowedAlts;
        //                                _with2.StorageManager.ProviderType = DBProviderType.dbptODBC;
        //                                _with2.StorageManager.ProjectLocation = FileConnString;
        //                                _with2.StorageManager.StorageType = fStorageType;
        //                                if (fStorageType == ECModelStorageType.mstAHPSStream)
        //                                {
        //                                    _with2.StorageManager.StorageType = ECModelStorageType.mstAHPSFile;
        //                                    _with2.StorageManager.ProjectLocation = sUploadedFileName;
        //                                }
        //                                if (fStorageType == ECModelStorageType.mstAHPDatabase)
        //                                {
        //                                    FixRAcontraintsTableBeforeUpload(sUploadedFileName);
        //                                }
        //                                _with2.StorageManager.ReadDBVersion();
        //                                if (_with2.LoadProject(_with2.StorageManager.ProjectLocation, DBProviderType.dbptODBC, _with2.StorageManager.StorageType))
        //                                {
        //                                    if (_with2.HierarchyCount > 0)
        //                                    {
        //                                        ObjsCount = _with2.Hierarchy(_with2.ActiveHierarchy).Nodes.Count;
        //                                        LevelsCount = _with2.Hierarchy(_with2.ActiveHierarchy).GetMaxLevel();
        //                                    }
        //                                    if (_with2.AltsHierarchyCount > 0)
        //                                        AltsCount = _with2.AltsHierarchy(_with2.ActiveAltsHierarchy).Nodes.Count;
        //                                    _with2.CloseProject();
        //                                }
        //                                tmpPrjManager = null;

        //                                break;
        //                        }

        //                        if (ObjMax != UNLIMITED_VALUE && ObjsCount > ObjMax + 1)
        //                        {
        //                            sError = ParseAllTemplates(App.LicenseErrorMessage(App.ActiveWorkgroup.License, ecLicenseParameter.MaxObjectives), App.ActiveUser, App.ActiveProject);
        //                            // D2904
        //                            fHasError = true;
        //                        }
        //                        // D3304
        //                        if (LevelMax != UNLIMITED_VALUE && LevelsCount > LevelMax)
        //                        {
        //                            sError += (string.IsNullOrEmpty(sError) ? "" : ";<br> ") + ParseAllTemplates(App.LicenseErrorMessage(App.ActiveWorkgroup.License, ecLicenseParameter.MaxLevelsBelowGoal), App.ActiveUser, App.ActiveProject);
        //                            // D2904
        //                            fHasError = true;
        //                        }
        //                        // D3050 + D3304
        //                        if (AltMax != UNLIMITED_VALUE && AltsCount > AltMax)
        //                        {
        //                            sError += (string.IsNullOrEmpty(sError) ? "" : ";<br> ") + ParseAllTemplates(App.LicenseErrorMessage(App.ActiveWorkgroup.License, ecLicenseParameter.MaxAlternatives), App.ActiveUser, App.ActiveProject);
        //                            // D2904
        //                            fHasError = true;
        //                        }
        //                        // D2133 ==

        //                    }
        //                }
        //                // D0925 ==

        //                // D0172
        //                if (!fHasError)
        //                {
        //                    // D0157 ==
        //                    string sComment = string.Format("Upload '{0}' model", Path.GetFileName(FileUploadProject.FileName));
        //                    // D0378

        //                    bool fUploaded = false;
        //                    SetOptions(CurrentProject);
        //                    // D0178
        //                    // D0378 + D0387 + D0479
        //                    if (App.DBProjectCreate(CurrentProject, sComment))
        //                    {

        //                        tbPasscode.Text = CurrentProject.Passcode;
        //                        // D0178

        //                        // D0378 ===
        //                        switch (fStorageType)
        //                        {
        //                            case ECModelStorageType.mstAHPDatabase:
        //                                // D0387
        //                                FixRAcontraintsTableBeforeUpload(sUploadedFileName);
        //                                //C0427
        //                                fUploaded = App.DBProjectCreateFromAHPFile(CurrentProject, sUploadedFileName, sError, _FILE_DATA_SETTINGS + GetXMLPipeParamsFilename());
        //                                // D0162 + D0166 + D0479
        //                                break;

        //                            case ECModelStorageType.mstCanvasDatabase:
        //                                // D0387
        //                                fUploaded = App.DBProjectCreateFromAHPXFile(CurrentProject, sUploadedFileName, sError);
        //                                //C0271 + D0479
        //                                break;

        //                            case ECModelStorageType.mstAHPSStream:
        //                                // D0387
        //                                IO.FileStream FS = null;
        //                                try
        //                                {
        //                                    FS = new IO.FileStream(sUploadedFileName, FileMode.Open, FileAccess.Read);
        //                                    CurrentProject.ResetProject(true);
        //                                    // D0387
        //                                    fUploaded = CurrentProject.ProjectManager.StorageManager.Writer.SaveFullProjectStream_CanvasStreamDatabase(FS);
        //                                    // D2646 ===
        //                                    if (fUploaded && App.isRiskEnabled)
        //                                    {
        //                                        CurrentProject.IsRisk = false;
        //                                        sIsRisk = Convert.ToString((CurrentProject.ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) ? 1 : 0));
        //                                        // D2646
        //                                        CurrentProject.IsRisk = App.isRiskEnabled;
        //                                    }
        //                                    // D2646 ==
        //                                    // D3651 ===
        //                                    if (fUploaded && CurrentProject.IsProjectLoaded)
        //                                    {
        //                                        CurrentProject.ProjectManager.StorageManager.ReadDBVersion();
        //                                        if (CurrentProject.ProjectManager.StorageManager.CanvasDBVersion.MinorVersion <= 30)
        //                                        {
        //                                            if (CurrentProject.ProjectManager != null && CurrentProject.ProjectManager.ResourceAligner != null && CurrentProject.ProjectManager.ResourceAligner.Solver != null && !string.IsNullOrEmpty(CurrentProject.ProjectManager.ResourceAligner.Solver.LastError))
        //                                            {
        //                                                sError = CurrentProject.ProjectManager.ResourceAligner.Solver.LastError;
        //                                                CurrentProject.ProjectManager.ResourceAligner.Solver.LastError = "";
        //                                            }
        //                                        }
        //                                    }
        //                                    // D3651 ==
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    sError = ex.Message;
        //                                    fUpdated = false;
        //                                    App.DBProjectDelete(CurrentProject, true);
        //                                }
        //                                finally
        //                                {
        //                                    if ((FS != null))
        //                                        FS.Close();
        //                                }


        //                                break;
        //                            // D2132 ===
        //                            case ECModelStorageType.mstTextFile:
        //                                fUploaded = clsTextModel.ReadModel(CurrentProject, sTextModel, App.isRiskEnabled, sError);
        //                                // D2426
        //                                if (fUploaded)
        //                                {
        //                                    CurrentProject.SaveStructure("Save data based on txt file");
        //                                    CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs();
        //                                    // D2133
        //                                    CurrentProject.ProjectManager.StorageManager.Writer.SaveUsersInfo();
        //                                    if (!string.IsNullOrEmpty(CurrentProject.Comment))
        //                                        App.DBProjectUpdate(CurrentProject, false, "Set project description");
        //                                    // D2804
        //                                }
        //                                else
        //                                {
        //                                    App.DBProjectDelete(CurrentProject, true);
        //                                    // D2146
        //                                }
        //                                break;
        //                                // D2132 ==
        //                        }
        //                        // D0378 ==

        //                        if (fUploaded)
        //                        {
        //                            // D0690 ===
        //                            if (!CurrentProject.isValidDBVersion)
        //                                App.DBProjectUpdateToLastVersion(CurrentProject);

        //                            //' AC, put your check here (set fShowWarningOnUpload for true for show warning after upload):

        //                            //' Examples:
        //                            //fShowWarningOnUpload = True
        //                            //fShowWarningOnUpload = Not MiscFuncs.IsCanvasDatabase(FileConnString, DBProviderType.dbptODBC)
        //                            //fShowWarningOnUpload = CurrentProject.ProjectManager IsNot Nothing

        //                            // D0690 ==

        //                            CurrentProject.ResetProject();
        //                            // D0138
        //                            SetOptions(CurrentProject);
        //                            // D0174
        //                            App.ImportProjectUsers(CurrentProject, App.ActiveUser, !cbStrongPasswords.Checked, cbCheckEmailBeforeUpload.Checked);
        //                            // D0113 + D0272 + D0345 + D0479 + D1985
        //                            // D2646 ===

        //                            // D2741 ===
        //                            //If Not App.isRiskEnabled AndAlso fStorageType = ECModelStorageType.mstAHPSStream Then sIsRisk = CStr(IIf(CurrentProject.ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact), 1, 0)) ' D2646
        //                            if (!App.isRiskEnabled && fStorageType == ECModelStorageType.mstAHPSStream)
        //                            {
        //                                // Was only check for Impact hierarchy. Now check for non-empty for avoid msg
        //                                sIsRisk = "0";
        //                                foreach (clsHierarchy H in CurrentProject.ProjectManager.Hierarchies)
        //                                {
        //                                    if (H.HierarchyID == ECHierarchyID.hidImpact)
        //                                    {
        //                                        if (H.Nodes.Count > 1)
        //                                            sIsRisk = "1";
        //                                        break; // TODO: might not be correct. Was : Exit For
        //                                    }
        //                                }
        //                            }
        //                            // D2741 ==

        //                            // D0126 ===
        //                            App.CheckAndAddReviewAccount(ReviewAccount, CurrentProject);
        //                            // D1382 + D1408

        //                            bool fDescUpdated = true;
        //                            // D0178
        //                            // D0174
        //                            if (string.IsNullOrEmpty(CurrentProject.Comment) & (CurrentProject.PipeParameters.ProjectPurpose != null))
        //                            {
        //                                CurrentProject.Comment = CurrentProject.PipeParameters.ProjectPurpose;
        //                                // D0174
        //                                fDescUpdated = true;
        //                            }
        //                            // D0174
        //                            if (string.IsNullOrEmpty(CurrentProject.ProjectName) & (CurrentProject.PipeParameters.ProjectName != null))
        //                            {
        //                                fDescUpdated = true;
        //                                CurrentProject.ProjectName = CurrentProject.PipeParameters.ProjectName;
        //                                // D0174
        //                            }
        //                            // D0174 ===
        //                            if (!string.IsNullOrEmpty(CurrentProject.Comment) & string.IsNullOrEmpty(CurrentProject.PipeParameters.ProjectPurpose))
        //                            {
        //                                CurrentProject.PipeParameters.ProjectPurpose = CurrentProject.Comment;
        //                                fDescUpdated = true;
        //                            }
        //                            if (!string.IsNullOrEmpty(CurrentProject.ProjectName) & string.IsNullOrEmpty(CurrentProject.PipeParameters.ProjectName))
        //                            {
        //                                fDescUpdated = true;
        //                                CurrentProject.PipeParameters.ProjectName = CurrentProject.ProjectName;
        //                            }
        //                            // D0174 ==
        //                            // D0892 ===
        //                            if (CurrentProject.PipeParameters.ProjectGuid != Guid.Empty)
        //                            {
        //                                CurrentProject.ProjectGUID = CurrentProject.PipeParameters.ProjectGuid;
        //                                fDescUpdated = true;
        //                            }
        //                            else
        //                            {
        //                                if (CurrentProject.CheckGUID())
        //                                    fDescUpdated = true;
        //                            }
        //                            // D0892 ==
        //                            // D3438 ===
        //                            if (CurrentProject.PipeParameters.ProjectType == ProjectType.ptOpportunities)
        //                            {
        //                                CurrentProject.isOpportunityModel = true;
        //                                fDescUpdated = true;
        //                            }
        //                            if (CurrentProject.PipeParameters.ProjectType == ProjectType.ptRiskAssociated)
        //                            {
        //                                CurrentProject.isRiskAssociatedModel = true;
        //                                fDescUpdated = true;
        //                            }
        //                            // D3438 ==
        //                            if (fDescUpdated)
        //                            {
        //                                //CurrentProject.PipeParameters.Write(PipeStorageType.pstDatabase, CurrentProject.ConnectionString, CurrentProject.ProviderType)    ' D0174 + D0329 'C0349
        //                                //CurrentProject.PipeParameters.Write(CurrentProject.Pipe_StorageType, CurrentProject.ConnectionString, CurrentProject.ProviderType) 'C0349 + D0369 + D0376 'C0390
        //                                CurrentProject.PipeParameters.Write(PipeStorageType.pstStreamsDatabase, CurrentProject.ConnectionString, CurrentProject.ProviderType, CurrentProject.ID);
        //                                //C0390 + D0479
        //                                App.DBProjectUpdate(CurrentProject, false, "Get Project Properties from model");
        //                            }
        //                            // D0126 ==
        //                            fUpdated = true;
        //                            fKeepEditor = false;
        //                            //sError = ""    ' -D3561
        //                        }
        //                        else
        //                        {
        //                            fKeepEditor = true;
        //                            App.DBSaveLog(dbActionType.actCreate, dbObjectType.einfProject, CurrentProject.ID, "Project Uploading Error", sError);
        //                            // D0496
        //                        }

        //                    }
        //                    // create project
        //                }
        //                // check users

        //                File_Erase(sUploadedFileName);
        //                //If sArcFolder <> "" Then File_DeleteFolder(sArcFolder) ' D0240 + D0242

        //                // D0172
        //                if (!string.IsNullOrEmpty(sError) | fHasError)
        //                {
        //                    if (sError.Trim.ToLower == "weight")
        //                        sError = ResString("errUploadWeightTblMissing");
        //                    // D0801
        //                    if (sError.Trim.ToLower == "organization")
        //                        sError = ResString("errUploadOrganizationTblMissing");
        //                    // D0915
        //                    // D3651 ===
        //                    if (sError.Trim.ToLower == "old_ra")
        //                    {
        //                        sError = ResString("errUploadOldRA");
        //                        // D3561
        //                        App.DBSaveLog(dbActionType.actShowMessage, dbObjectType.einfProject, CurrentProject.ID, "Warning on upload", sError);
        //                        btnOK.Text = ResString("btnContinue");
        //                        if (isSLTheme())
        //                        {
        //                            sURL = PageURL(_PGID_SILVERLIGHT_SERVICE, _PARAM_ACTION + "=upload&" + _PARAM_ID + "=" + CurrentProject.ID.ToString + "&risk=" + sIsRisk + GetTempThemeURI(true));
        //                            // D0893 + D2646
        //                            btnOK.OnClientClick = string.Format("document.location.href='{0}'; return false;", JS_SafeString(sURL));
        //                        }
        //                        btnCancel.Visible = false;
        //                        rowFile.Visible = false;
        //                        rowName.Visible = false;
        //                        rowProps.Visible = false;
        //                    }
        //                    // D3651 ==

        //                    lblError.Text = sError;
        //                    lblError.Visible = true;
        //                    fKeepEditor = true;
        //                    ASPxPageControlProps.ActiveTabIndex = 0;
        //                    // D0178
        //                    // D1519 ===
        //                    if (App.ApplicationError.Status == ecErrorStatus.errWrongLicense && App.ApplicationError.CustomData != null && (App.ApplicationError.CustomData == ecLicenseParameter.MaxUsersInProject.ToString || App.ApplicationError.CustomData == ecLicenseParameter.MaxUsersInWorkgroup.ToString))
        //                    {
        //                        btnOK.Text = ResString("btnContinue");
        //                        lblError.Text += "<br>" + ResString("msgYouCanUploadFileWithPartialData");
        //                        IgnoreLicenseWarning.Value = FileUploadProject.FileName;
        //                        App.ApplicationError.Reset();
        //                    }
        //                    // D1519 ==
        //                }

        //            }
        //            // has uploaded file
        //            break;

        //    }

        //    if ((CurrentProject != null) && fUpdated)
        //    {
        //        // D0126 ===
        //        if (string.IsNullOrEmpty(CurrentProject.ProjectName))
        //        {
        //            CurrentProject.ProjectName = CurrentProject.Passcode;
        //            App.DBProjectUpdate(CurrentProject, false, "Set Project Name as Passcode");
        //        }
        //        // D0126 ==
        //        //ProjectSaveInfo(CurrentProject)  ' D0286
        //    }

        //    App.ActiveProjectsList = null;
        //    // D0479
        //    App.Workspaces = null;
        //    // D0479

        //    if (fReloadPage & (Request != null))
        //        Response.Redirect(Request.Url.OriginalString, true);
        //    // D0302

        //    if (fUpdated & !fKeepEditor & string.IsNullOrEmpty(lblError.Text))
        //    {
        //        if ((CurrentProject != null))
        //        {
        //            App.ProjectID = CurrentProject.ID;
        //            sURL = (!string.IsNullOrEmpty(sURL) ? sURL : (_isUpload | _isNewProject ? URLProjectID(PageURL((_isUpload ? _DEF_PGID_ONSELECTPROJECT : _DEF_PGID_ONNEWPROJECTS)), CurrentProject.ID) : GetBackURL));
        //            // D0690
        //            if (fShowWarningOnUpload)
        //                sURL = URLWithParams(sURL, "warning=yes");
        //            // D0690

        //            // D0893 ===
        //            bool fReplace = false;
        //            if (_isUpload && CurrentProject.ProjectGUID != Guid.Empty)
        //            {
        //                List<clsProject> tExistedLst = clsProject.ProjectsByGUID(CurrentProject.ProjectGUID, App.ActiveProjectsList, false);
        //                if (tExistedLst != null && tExistedLst.Count > 1)
        //                {
        //                    lblReplace.InnerHtml = string.Format(ResString("lblReplaceModelTitle"), ShortString(CurrentProject.ProjectName, 45));
        //                    System.DateTime LM = Now.AddYears(-1);
        //                    bool LMFound = false;
        //                    foreach (clsProject tPrj in tExistedLst)
        //                    {
        //                        if (tPrj.ID != CurrentProject.ID && tPrj.ProjectStatus == ecProjectStatus.psActive && !tPrj.isMarkedAsDeleted)
        //                        {
        //                            if (tPrj.LastModify.HasValue && LM < tPrj.LastModify)
        //                            {
        //                                LM = tPrj.LastModify;
        //                                LMFound = true;
        //                            }
        //                            if (tPrj.Created.HasValue && LM < tPrj.Created)
        //                                LM = tPrj.Created;
        //                            string sCrt = "?";
        //                            if (tPrj.Created.HasValue)
        //                                sCrt = tPrj.Created.Value.ToString;
        //                            string sLM = "?";
        //                            if (tPrj.LastModify.HasValue)
        //                                sLM = tPrj.LastModify.Value.ToString;
        //                            lblReplace.InnerHtml += string.Format("<div><input type='radio' name='prjid' value='{0}' id='prj{0}'{2}><label for='prj{0}' class='text'>{1}</label></div>", tPrj.ID, string.Format(ResString("lblReplaceModelTemplate"), ShortString(tPrj.ProjectName, 30), tPrj.Passcode, sCrt, sLM), ((tPrj.LastModify.HasValue && LM == tPrj.LastModify) | (tPrj.Created.HasValue && LM == tPrj.Created) ? " checked" : ""));
        //                            fReplace = true;
        //                        }
        //                    }
        //                    if (fReplace)
        //                    {
        //                        lblReplace.InnerHtml += string.Format("<input type='radio' name='prjid' value='-1' id='prj-1' /><label for='prj-1' class='text'{1}>{0}</label>", ResString("lblReplaceModelKeepAsNew"), (LMFound ? "" : " checked"));
        //                        ASPxPageControlProps.Visible = false;
        //                        pnlReplaceModel.Visible = true;
        //                        btnOK.CommandName = "replace";
        //                        btnCancel.Visible = true;
        //                        // D0971
        //                        btnCancel.UseSubmitBehavior = false;
        //                        // D0971
        //                        btnCancel.OnClientClick = string.Format("setTimeout('theForm.disabled=1;', 500); document.location.href='{0}'; return false;", URLProjectID(PageURL(_PGID_PROJECT_DELETE, (isSLTheme ? "&close=yes" : "") + GetTempThemeURI(true)), CurrentProject.ID));
        //                        // D0971 + D0973
        //                    }
        //                }
        //            }
        //            // D0893 ==

        //            // D0725 ===
        //            // D0729 + D0766 + D0893
        //            if (isSLTheme() && _isUpload)
        //            {
        //                sURL = PageURL(_PGID_SILVERLIGHT_SERVICE, _PARAM_ACTION + "=upload&" + _PARAM_ID + "=" + CurrentProject.ID.ToString + "&risk=" + sIsRisk + GetTempThemeURI(true));
        //                // D0893 + D2646
        //            }
        //            // D0725 ==

        //            if (!fReplace)
        //                Response.Redirect(sURL, true);
        //            else
        //                btnOK.CommandArgument = sURL;
        //            // D0255 + D0316 + D0690 + D0893
        //        }
        //    }

        //}


        public static string GetPipeStepHint(clsAction Action, object tExtraParam = null, bool fCanBePathInteractive = false, bool fGetResultsCustomTitle = false)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            string sRes = "";
            
            if (App.HasActiveProject() && Action != null && Action.ActionData != null)
            {
                clsHierarchy Hierarchy = App.ActiveProject.HierarchyObjectives;
                Dictionary<string, string> tParams = new Dictionary<string, string>();
                bool isImpact = App.ActiveProject.ProjectManager.ActiveHierarchy == (int)ECTypes.ECHierarchyID.hidImpact;
                bool IsRiskWithControls = false; /*(CurrentPageID == _PGID_EVALUATE_RISK_CONTROLS);*/
                switch (Action.ActionType)
                {

                    case ActionType.atInformationPage:
                        {
                            string sPage = "";
                            switch (((clsInformationPageActionData)Action.ActionData).Description.ToLower())
                            {
                                //C0464
                                case "welcome":
                                    sPage = "lblWelcome";
                                    break;
                                case "thankyou":
                                    sPage = "lblThankYou";
                                    break;
                            }
                            sRes = string.Format(TeamTimeClass.ResString("lblEvaluationInfoPage"), TeamTimeClass.ResString(sPage));
                        }
                        break;


                    case ActionType.atSpyronSurvey:
                        {
                            string sHint = TeamTimeClass.ResString("lblSpyronSurvey");
                            Dictionary<string, SpyronControls.Spyron.Core.clsComparionUser> UsersList = new Dictionary<string, SpyronControls.Spyron.Core.clsComparionUser>();
                            //UsersList.Add(App.ActiveUser.UserEmail, New clsComparionUser() With {.ID = App.ActiveUser.UserID, .UserName = App.ActiveUser.UserName})
                            UsersList.Add(App.ActiveUser.UserEMail, new SpyronControls.Spyron.Core.clsComparionUser
                            {
                                ID = App.ActiveProject.ProjectManager.UserID,
                                UserName = App.ActiveProject.ProjectManager.User.UserName
                            });
                            clsSpyronSurveyAction Data = (clsSpyronSurveyAction)Action.ActionData;
                            App.SurveysManager.ActiveUserEmail = App.ActiveUser.UserEMail;
                            SpyronControls.Spyron.Core.clsSurveyInfo tSurvey = App.SurveysManager.get_GetSurveyInfoByProjectID(App.ProjectID, (SpyronControls.Spyron.Core.SurveyType)Data.SurveyType, UsersList);

                            if ((tSurvey != null))
                            {
                                SpyronControls.Spyron.Core.clsSurvey tmpSurvey = tSurvey.get_Survey(App.ActiveUser.UserEMail);

                                if (tmpSurvey != null && Data.StepNumber > 0 && tmpSurvey.Pages != null && tmpSurvey.Pages.Count <= Data.StepNumber && Data.StepNumber > 0)
                                {
                                    SpyronControls.Spyron.Core.clsSurveyPage tPage = (clsSurveyPage)tmpSurvey.Pages[Data.StepNumber - 1];

                                    if ((tPage != null))
                                        sHint = string.Format("{0}: {1}", sHint, tPage.Title);
                                }
                            }
                            sHint = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sHint, true, false), tParams);
                            sRes = sHint;
                            break;
                        }




                    case ActionType.atShowLocalResults:
                        {
                            clsShowLocalResultsActionData LRData = (clsShowLocalResultsActionData)Action.ActionData;
                            if ((LRData.ParentNode != null))
                            {

                                tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(LRData.ParentNode, fCanBePathInteractive));

                                bool LRfIsPWOutcomes = LRData.PWOutcomesNode != null && LRData.ParentNode.get_MeasureType() == ECMeasureType.mtPWOutcomes;
                                if (LRfIsPWOutcomes)
                                    tParams.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(LRData.PWOutcomesNode, fCanBePathInteractive));

                                if (tExtraParam != null)
                                {
                                    sRes = Convert.ToString((LRData.ParentNode.get_ParentNode() != null ? "lblEvaluationResultIntensities" : "lblEvaluationResultObjectiveIntensities"));
                                }
                                else
                                {
                                    if (LRfIsPWOutcomes)
                                    {
                                        if (LRData.PWOutcomesNode != null && LRData.PWOutcomesNode.IsAlternative)
                                        {
                                            sRes = Convert.ToString((LRData.ParentNode.get_ParentNode() != null ? "lblEvaluationResultPW{0}AltsHierarchy" : "lblEvaluationResultPW{0}Alts"));
                                        }
                                        else
                                        {
                                            sRes = Convert.ToString((LRData.ParentNode.get_ParentNode() != null ? "lblEvaluationResultPW{0}Hierarchy" : "lblEvaluationResultPW{0}"));

                                        }

                                        string sName = "Outcomes";
                                        if (Action.PWONode != null && Action.PWONode.MeasurementScale != null)
                                        {
                                            clsRatingScale tRS = (clsRatingScale)Action.PWONode.MeasurementScale;
                                            if (tRS.IsPWofPercentages)
                                                sName = "Percentages";
                                            if (tRS.IsExpectedValues)
                                                sName = "ExpectedValues";
                                        }
                                        sRes = string.Format(sRes, sName);


                                    }
                                    else
                                    {
                                        if (LRData.ParentNode.IsTerminalNode)
                                        {
                                            if (App.isRiskEnabled)
                                            {
                                                if (isImpact)
                                                {
                                                    sRes = Convert.ToString((Hierarchy != null && Hierarchy.Nodes.Count == 1 ? "lblEvaluationResultAlternativesNoObjImpact" : "lblEvaluationResultAlternativesImpact"));
                                                }
                                                else
                                                {
                                                    sRes = Convert.ToString((Hierarchy != null && Hierarchy.Nodes.Count == 1 ? "lblEvaluationResultAlternativesNoObjsRisk" : "lblEvaluationResultAlternativesRisk"));
                                                }
                                            }
                                            else
                                            {
                                                sRes = "lblEvaluationResultAlternatives";
                                            }
                                        }
                                        else
                                        {
                                            if (App.isRiskEnabled)
                                            {

                                                if (!isImpact && LRData.ParentNode.RiskNodeType == ECTypes.RiskNodeType.ntCategory)
                                                {
                                                    sRes = "lblEvaluationResultObjective_Category";
                                                }
                                                else
                                                {

                                                    if (LRData.ParentNode.get_ParentNode() == null)
                                                    {
                                                        sRes = Convert.ToString((isImpact ? "lblEvaluationResultObjectiveGoalImpact" : "lblEvaluationResultObjectiveGoalRisk"));
                                                    }
                                                    else
                                                    {
                                                        sRes = Convert.ToString((isImpact ? "lblEvaluationResultObjectiveImpact" : "lblEvaluationResultObjectiveRisk"));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                sRes = "lblEvaluationResultObjective";
                                            }
                                        }
                                    }
                                }

                                sRes = TeamTimeClass.ResString(sRes, true, false);

                                if (LRData.ParentNode != null && fGetResultsCustomTitle)
                                {
                                    var tAddGUID = Guid.Empty;
                                    if (tExtraParam != null && tExtraParam is clsMeasurementScale)
                                    {
                                        var temp = (clsMeasurementScale)tExtraParam;
                                        tAddGUID = temp.GuidID;
                                    }

                                    var ClusterTitle = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterTitleForResults(LRData.ParentNode.NodeGuidID, tAddGUID);

                                    if (ClusterTitle == "" || StringFuncs.HTML2Text(ClusterTitle) == "")
                                    {
                                        ClusterTitle = sRes;
                                    }
                                    var ClusterTitleIsCustom = ClusterTitle.Trim().ToLower() != sRes.Trim().ToLower();
                                    if (ClusterTitleIsCustom && ClusterTitle.Trim() != "")
                                    {
                                        sRes = ClusterTitle;
                                    }

                                }

                                sRes = TeamTimeClass.PrepareTask(StringFuncs.ParseStringTemplates(sRes, tParams), tExtraParam);

                            }
                        }
                        break;
                    case ActionType.atShowGlobalResults:
                        
                        if (App.ActiveProject.HierarchyObjectives.Nodes.Count > 0)
                        {
                            tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(App.ActiveProject.HierarchyObjectives.Nodes[0], fCanBePathInteractive));

                            if (App.isRiskEnabled)
                            {
                                sRes = Convert.ToString((isImpact ? "lblEvaluationResultsOverallImpact" : "lblEvaluationResultsOverallRisk"));
                            }
                            else
                            {
                                sRes = "lblEvaluationResultsOverall";
                            }
                            sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes, true, false), tParams);
                            
                        }
                        break;
                    

                    
                    case ActionType.atPairwise:
                    case ActionType.atPairwiseOutcomes:
                        {
                            clsPairwiseMeasureData Act = (clsPairwiseMeasureData)Action.ActionData;
                            clsNode parentNode = null;
                            clsHierarchy H = null;

                            bool fIsPWOutcomes = Action.ActionType == ActionType.atPairwiseOutcomes;
                            if (fIsPWOutcomes)
                            {
                                parentNode = Action.ParentNode;
                            }
                            else
                            {
                                parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(Act.ParentNodeID);
                                if (parentNode == null)
                                    parentNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(Act.ParentNodeID);

                                if (parentNode != null)
                                    H = (clsHierarchy)(parentNode.IsAlternative || parentNode.IsTerminalNode ? App.ActiveProject.HierarchyAlternatives : App.ActiveProject.HierarchyObjectives);

                            }

                            clsNode tNodeLeft = new clsNode();
                            clsNode tNodeRight = new clsNode();

                            if (fIsPWOutcomes)
                            {
                                App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(Action, Act, ref tNodeLeft, ref tNodeRight);
                                tParams.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.PWONode, fCanBePathInteractive));

                            }
                            else
                            {
                                if (H != null)
                                {
                                    tNodeLeft = H.GetNodeByID(Act.FirstNodeID);
                                    tNodeRight = H.GetNodeByID(Act.SecondNodeID);
                                }
                            }

                            if (parentNode != null)
                                tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(parentNode, fCanBePathInteractive));

                            if (tNodeLeft != null)
                                tParams.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNodeLeft.NodeName));
                            if (tNodeRight != null)
                                tParams.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(tNodeRight.NodeName));

                            if (fIsPWOutcomes)
                            {

                                clsRatingScale tRS = (clsRatingScale)Action.PWONode.MeasurementScale;

                                if (tRS != null)
                                {
                                    sRes = Convert.ToString((parentNode != null && parentNode.IsAlternative ? (Action.PWONode != null && Action.PWONode.get_ParentNode() == null ? "lblEvaluationPWOutcomesAltsGoal" : "lblEvaluationPWOutcomesAlts") : "lblEvaluationPWOutcomes"));

                                    if (parentNode.Level > 1)
                                        sRes = "lblEvaluationPWOutcomesLevels";
                                    if (tRS.IsPWofPercentages)
                                        sRes = "lblEvaluationPWPercentages";
                                    if (tRS.IsExpectedValues)
                                        sRes = "lblEvaluationExpectedValues";

                                }
                            }
                            else
                            {

                                if (App.isRiskEnabled && parentNode.RiskNodeType == ECTypes.RiskNodeType.ntCategory)
                                {
                                    sRes = Convert.ToString((parentNode.IsTerminalNode ? "lblEvaluationPWAlt_Category" : "lblEvaluationPW_Category"));
                                }
                                else
                                {

                                    sRes = Convert.ToString((parentNode.Level == 0 ? "lblEvaluationPWNoObj" : (App.isRiskEnabled && !isImpact ? (parentNode.IsTerminalNode ? "lblEvaluationPWLikelihoodAlts" : "lblEvaluationPWLikelihood") : "lblEvaluationPW")));

                                }
                            }
                            sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);
                        }
                        break;



                    case ActionType.atAllPairwise:
                    case ActionType.atAllPairwiseOutcomes:
                        {
                            clsNode tNode = ((clsAllPairwiseEvaluationActionData)Action.ActionData).ParentNode;
                            if ((tNode != null))
                            {
                                tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tNode, fCanBePathInteractive));

                                if (Action.ActionType == ActionType.atAllPairwiseOutcomes)
                                {
                                    if (Action.ParentNode != null)
                                        tParams.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.ParentNode, fCanBePathInteractive));



                                    if (Action.ParentNode != null && Action.ParentNode.IsAlternative)
                                    {
                                        sRes = Convert.ToString((tNode != null && tNode.get_ParentNode() == null ? "task_MultiPairwise_Alternatives_PW{0}_Goal" : "task_MultiPairwise_Alternatives_PW{0}"));

                                    }
                                    else
                                    {
                                        sRes = Convert.ToString((tNode != null && tNode.get_ParentNode() == null ? "task_MultiPairwise_Hierarchy_PW{0}" : "task_MultiPairwise_Objectives_PW{0}"));
                                    }

                                    clsRatingScale tRs = (clsRatingScale)Action.PWONode.MeasurementScale;
                                    string sName = "Outcomes";
                                    if (tRs.IsPWofPercentages)
                                        sName = "Percentages";
                                    if (tRs.IsExpectedValues)
                                        sName = "ExpectedValues";
                                    sRes = string.Format(sRes, sName);

                                }
                                else
                                {
                                    sRes = "lblEvaluationAllPW";

                                    if (App.isRiskEnabled && !isImpact)
                                    {
                                        sRes = Convert.ToString((tNode != null && tNode.IsTerminalNode ? "lblEvaluationAllPWLikelihoodAlt" : "lblEvaluationAllPWLikelihoodObj"));
                                    }

                                }
                                sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);

                            }
                        }

                        break;



                    case ActionType.atNonPWOneAtATime:

                        clsOneAtATimeEvaluationActionData data = (clsOneAtATimeEvaluationActionData)Action.ActionData;


                        if (IsRiskWithControls)
                        {
                            //if (data != null && data.Assignment != null && data.Control != null)
                            //{
                            //    tParams.Add(_TEMPL_NODENAME, data.Control.Name);
                            //    clsNode tNode = null;

                            //    switch (data.Control.Type)
                            //    {
                            //        case ControlType.ctCause:
                            //            if (!Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty))
                            //            {
                            //                tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID);
                            //                if (tNode != null)
                            //                    tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tNode.NodeName));
                            //                tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID);
                            //                if (tNode != null)
                            //                    tParams.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName));
                            //            }
                            //            break;
                            //        case ControlType.ctCauseToEvent:
                            //            if (!Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty))
                            //            {
                            //                tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID);
                            //                if (tNode != null)
                            //                    tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tNode.NodeName));
                            //                tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID);
                            //                if (tNode != null)
                            //                    tParams.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName));
                            //            }
                            //            break;
                            //        case ControlType.ctConsequenceToEvent:
                            //            if (!Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty))
                            //            {
                            //                tNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(data.Assignment.ObjectiveID);
                            //                if (tNode != null)
                            //                    tParams.Add(_TEMPL_NODE_B, JS_SafeHTML(tNode.NodeName));
                            //                tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID);
                            //                if (tNode != null)
                            //                    tParams.Add(_TEMPL_JUSTNODE, JS_SafeHTML(tNode.NodeName));
                            //            }
                            //            break;
                            //    }

                            //    string sName = "";
                            //    switch (data.Control.Type)
                            //    {
                            //        case ControlType.ctCause:
                            //            sName = ResString("lblControlCause");
                            //            break;
                            //        case ControlType.ctCauseToEvent:
                            //            sName = ResString(Convert.ToString((tNode != null && tNode.ParentNode == null ? "lblControlCauseToEventGoal" : "lblControlCauseToEvent")));

                            //            break;
                            //        case ControlType.ctConsequenceToEvent:
                            //            sName = ResString("lblControlConsequence");
                            //            break;
                            //    }
                            //    sRes = string.Format(ResString("taskControlNonPWOneAtATime"), sName);
                            //    sRes = ParseStringTemplates(sRes, tParams);
                            //}

                        }
                        else
                        {


                            if (data != null && data.Judgment != null)
                            {

                                switch (data.MeasurementType)
                                {
                                    case ECMeasureType.mtDirect:

                                        clsDirectMeasureData tDirect = (clsDirectMeasureData)data.Judgment;
                                        tParams.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(data.Node.NodeName));
                                        clsHierarchy tH = default(clsHierarchy);

                                        if (Action.IsFeedback & App.ActiveProject.ProjectManager.FeedbackOn)
                                        {
                                            tH = App.ActiveProject.HierarchyObjectives;
                                        }
                                        else
                                        {
                                            if (data.Node.IsTerminalNode)
                                                tH = App.ActiveProject.HierarchyAlternatives;
                                            else
                                                tH = App.ActiveProject.HierarchyObjectives;
                                        }

                                        tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tH.GetNodeByID(tDirect.NodeID), fCanBePathInteractive));

                                        if (App.isRiskEnabled)
                                        {
                                            if (isImpact)
                                            {
                                                sRes = Convert.ToString((data.Node.Level > 0 ? "lblEvaluationDirectImpact" : "lblEvaluationDirectImpactNoLevels"));
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((data.Node.Level == 0 ? "lblEvaluationDirectRiskNoObj" : "lblEvaluationDirectRisk"));

                                            }
                                            tParams.Add(ECWeb.Options._TEMPL_NODETYPE, Convert.ToString((data.Node.IsTerminalNode ? ECWeb.Options._TEMPL_ALTERNATIVE : ECWeb.Options._TEMPL_OBJECTIVE)));

                                        }
                                        else
                                        {
                                            sRes = "lblEvaluationDirect";
                                        }
                                        sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);

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

                                        tParams.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, fCanBePathInteractive));

                                        tParams.Add(ECWeb.Options._TEMPL_NODENAME, StringFuncs.JS_SafeHTML(tAlt.NodeName));

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

                                        sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);
                                        break;



                                    case ECMeasureType.mtRegularUtilityCurve:
                                    case ECMeasureType.mtAdvancedUtilityCurve:
                                    case ECMeasureType.mtCustomUtilityCurve:
                                        clsNode CurvetParentNode = (clsNode)data.Node.Hierarchy.GetNodeByID(((clsUtilityCurveMeasureData)data.Judgment).ParentNodeID);
                                        tParams.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(CurvetParentNode, fCanBePathInteractive));

                                        clsNode CurvetAlt = default(clsNode);
                                        if (CurvetParentNode.IsTerminalNode)
                                        {
                                            tAlt = data.Node.Hierarchy.ProjectManager.get_AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(((clsUtilityCurveMeasureData)data.Judgment).NodeID);
                                        }
                                        else
                                        {
                                            tAlt = data.Node.Hierarchy.GetNodeByID(((clsUtilityCurveMeasureData)data.Judgment).NodeID);
                                        }
                                        tParams.Add(ECWeb.Options._TEMPL_NODENAME, StringFuncs.JS_SafeHTML(tAlt.NodeName));
                                        switch (data.MeasurementType)
                                        {
                                            case ECMeasureType.mtAdvancedUtilityCurve:
                                                sRes = "task_AdvancedUtilityCurve";
                                                break;
                                            default:

                                                if (App.isRiskEnabled)
                                                {
                                                    if (isImpact)
                                                    {
                                                        sRes = Convert.ToString((CurvetParentNode.get_ParentNode() == null ? "lblEvaluationUCGoalImpact" : "lblEvaluationUCImpact"));
                                                    }
                                                    else
                                                    {
                                                        sRes = Convert.ToString((CurvetParentNode.get_ParentNode() == null ? "lblEvaluationUCGoalRisk" : "lblEvaluationUCRisk"));
                                                    }
                                                }
                                                else
                                                {
                                                    sRes = "lblEvaluationUC";
                                                }
                                                break;

                                        }
                                        sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);
                                        break;


                                    default:
                                        {
                                            bool isAlt = data.Node != null && (data.Node.IsAlternative || data.Node.IsTerminalNode);

                                            clsNonPairwiseMeasureData tData = (clsNonPairwiseMeasureData)data.Judgment;
                                            clsNode curvetNode = null;
                                            clsNode tNode = null;
                                            if (isAlt)
                                            {
                                                tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tData.NodeID);
                                            }
                                            else
                                            {
                                                tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tData.NodeID);
                                                if (tNode == null)
                                                {
                                                    tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tData.NodeID);
                                                    isAlt = true;
                                                }
                                            }
                                            if (tNode != null)
                                                tParams.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNode.NodeName));

                                            tParams.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(data.Node.NodeName));

                                            bool fHasLevels = data.Node.Level > 0;
                                            if (App.isRiskEnabled)
                                            {
                                                if (isImpact)
                                                {
                                                    sRes = Convert.ToString((fHasLevels ? "lblEvaluationRatingImpact" : "lblEvaluationRatingImpactNoLevels"));
                                                }
                                                else
                                                {
                                                    sRes = Convert.ToString((fHasLevels ? "lblEvaluationRatingRisk" : "lblEvaluationRatingNoLevelsRisk"));
                                                }
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((fHasLevels ? "lblEvaluationRating" : "lblEvaluationRatingNoLevels"));
                                            }
                                            if (data.Node != null && !isAlt)
                                                sRes += "Obj";

                                            sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);

                                        }
                                        break;

                                }

                            }
                        }
                        break;

                    case ActionType.atNonPWAllChildren:

                        clsAllChildrenEvaluationActionData NPCdata = (clsAllChildrenEvaluationActionData)Action.ActionData;

                        if (NPCdata != null && NPCdata.ParentNode != null)
                        {

                            switch (NPCdata.MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                    if (App.isRiskEnabled)
                                    {
                                        if (isImpact)
                                        {
                                            sRes = Convert.ToString((NPCdata.ParentNode.IsAlternative ? "task_MultiRatings_AllCovObjImpact" : (NPCdata.ParentNode.get_ParentNode() == null ? "task_MultiRatings_AllAltsGoalImpact" : "task_MultiRatings_AllAltsImpact")));
                                        }
                                        else
                                        {
                                            sRes = Convert.ToString((NPCdata.ParentNode == null || NPCdata.ParentNode.get_ParentNode() == null ? "lblEvaluationMultiDirectDataLikelihood" : (NPCdata.ParentNode.IsTerminalNode ? "task_MultiRatings_AllAltsRisk" : (NPCdata.ParentNode.RiskNodeType == ECTypes.RiskNodeType.ntCategory ? "task_MultiRatings_AllObjRisk_Cat" : "task_MultiRatings_AllObjRisk"))));
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
                                        if (NPCdata.ParentNode.IsTerminalNode)
                                        {
                                            // D2355 ===
                                            if (isImpact)
                                            {
                                                sRes = Convert.ToString((NPCdata.ParentNode.Level == 0 ? "lblEvaluationMultiDirectDataAltsGoalRisk" : "lblEvaluationMultiDirectDataAltsRisk"));
                                                // D2399
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((NPCdata.ParentNode.Level == 0 ? "lblEvaluationMultiDirectDataAltsGoalLikelihood" : "lblEvaluationMultiDirectDataAltsLikelihood"));
                                            }
                                            // D2355 ==
                                        }
                                        else
                                        {
                                            if (!isImpact)
                                            {
                                                sRes = Convert.ToString((NPCdata.ParentNode.Level > 0 ? "lblEvaluationMultiDirectDataLevelsLikelihood" : "lblEvaluationMultiDirectDataLikelihood"));
                                            }
                                            else
                                            {
                                                sRes = Convert.ToString((NPCdata.ParentNode.get_ParentNode() == null ? "lblEvaluationMultiDirectDataGoalRisk" : "lblEvaluationMultiDirectDataRiskObj"));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (NPCdata.ParentNode.IsTerminalNode)
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

                            tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(NPCdata.ParentNode, fCanBePathInteractive));

                            tParams.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(NPCdata.Children.Count));
                            sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);
                        }

                        break;
                    case ActionType.atNonPWAllCovObjs:

                        clsAllCoveringObjectivesEvaluationActionData NPAdata = (clsAllCoveringObjectivesEvaluationActionData)Action.ActionData;

                        if ((NPAdata != null))
                        {

                            tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(NPAdata.Alternative, fCanBePathInteractive));

                            if (NPAdata.MeasurementType == ECMeasureType.mtDirect)
                            {
                                if (App.isRiskEnabled)
                                {
                                    sRes = Convert.ToString((Hierarchy.Nodes.Count <= 1 ? "lblEvaluationAllCovObjsRiskNoObj" : "lblEvaluationAllCovObjsRisk"));
                                }
                                else
                                {
                                    sRes = "lblEvaluationAllCovObjs";
                                }
                            }

                            if (NPAdata.MeasurementType == ECMeasureType.mtRatings)
                            {
                                if (App.isRiskEnabled)
                                {
                                    if (isImpact)
                                    {
                                        sRes = Convert.ToString((App.ActiveProject.HierarchyObjectives.GetMaxLevel() < 1 ? "task_MultiRatings_AllCovObjGoalImpact" : "task_MultiRatings_AllCovObjImpact"));
                                    }
                                    else
                                    {
                                        sRes = Convert.ToString((App.ActiveProject.HierarchyObjectives.GetMaxLevel() < 1 ? "task_MultiRatings_AllCovObjGoal" : "task_MultiRatings_AllCovObj"));

                                    }
                                }
                                else
                                {
                                    sRes = "lblEvaluationAllCovObjsRatings";

                                }
                            }
                            sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);

                        }

                        break;

                    case ActionType.atAllEventsWithNoSource:
                        clsAllEventsWithNoSourceEvaluationActionData AEWdata = (clsAllEventsWithNoSourceEvaluationActionData)Action.ActionData;
                        if ((AEWdata != null))
                        {
                            switch (AEWdata.MeasurementType)
                            {
                                case ECMeasureType.mtRatings:
                                case ECMeasureType.mtDirect:
                                    sRes = "lblEvaluationNoSources";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(sRes))
                            sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams);
                        break;



                    case ActionType.atSensitivityAnalysis:
                        switch (((clsSensitivityAnalysisActionData)Action.ActionData).SAType)
                        {

                            case SAType.satDynamic:
                                sRes = TeamTimeClass.ResString("lblEvaluationDynamicSA");

                                break;
                            case SAType.satGradient:
                                sRes = TeamTimeClass.ResString("lblEvaluationGradientSA");

                                break;
                            case SAType.satPerformance:
                                sRes = TeamTimeClass.ResString("lblEvaluationPerformanceSA");

                                break;
                        }
                        break;


                }
            }
            return TeamTimeClass.PrepareTask(sRes, tExtraParam);

        }


        public static string GetWRTNodeNameWithPath(clsNode tNode, bool CanBeInteractive)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            bool DontShowPath = App.ActiveProject.PipeParameters.ShowFullObjectivePath == PipeParameters.ecShowObjectivePath.DontShowPath;
            string sName = "";
            if (tNode != null)
            {
                sName = StringFuncs.JS_SafeHTML(tNode.NodeName);
                string sDivider = StringFuncs.JS_SafeHTML(TeamTimeClass.ResString("lblObjectivePathDivider"));
                string sPath = "";
                while (tNode.get_ParentNode() != null)
                {
                    if (tNode.get_ParentNode().get_ParentNode() != null)
                        sPath = StringFuncs.JS_SafeHTML(tNode.get_ParentNode().NodeName) + sDivider + sPath;
                    
                    tNode = tNode.get_ParentNode();
                }

                if (DontShowPath && CanBeInteractive && !string.IsNullOrEmpty(sPath))
                {
                    //if (sOldWRTPath == null)
                    //    sOldWRTPath = SessVar(SESS_WRT_PATH);
                    
                    //bool fCanSee = !string.Equals(sPath, sOldWRTPath);
                    
                    //sName = string.Format("<span onmouseover=\"this.title='{3}';\" onclick='ToggleWRTPath();' class='wrt_link'><span id='wrt_path' class='wrt_path'{2}>{0}</span>{1}</span>", sPath, sName, "", JS_SafeString((sPath + sName).Replace("\"", "&#39;")));
                   
                    //if (!App.ActiveProject.PipeParameters.ShowFullObjectivePath)
                    //{
                    //    if (fCanSee)
                    //    {
                    //        ClientScript.RegisterStartupScript(typeof(string), "InitWRTPath", "setTimeout('DoFlashWRTLink(\"wrt_path\");', 1500);", true);
                            
                    //        SessVar(SESS_WRT_PATH) = sPath;
                    //    }
                    //    else
                    //    {
                    //        ClientScript.RegisterStartupScript(typeof(string), "InitWRTPathLink", "setTimeout('DoFlashWRTPath();', 1500);", true);
                            
                    //    }
                    //}
                }
                else
                {
                    if (!DontShowPath)
                        return sPath + sName;
                    else
                        return sName;
                }
            }
            return sName;
        }

        public static ECCore.Groups.clsCombinedGroup GetCombinedGroupbyUserId(int userId)
        {
            var app = (clsComparionCore)HttpContext.Current.Session["App"];
            var groupList = app.ActiveProject.ProjectManager.CombinedGroups.GroupsList;
            foreach(ECCore.Groups.clsCombinedGroup group in groupList)
            {
                if (group.ContainsUser(userId) && group.CombinedUserID != ECTypes.COMBINED_USER_ID)
                    return group;
            }
            return app.ActiveProject.ProjectManager.CombinedGroups.GetDefaultCombinedGroup();
        }

        public static string loadStepButtons(int step)
        {
            HttpContext context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];

            var stepButtons = "";
            var totalStep = app.ActiveProject.Pipe.Count;
            var left = 15;
            var right = 15;
            if (step < 15)
            {
                right = 15 + step;
            }
            else if (step > totalStep - 15)
            {
                var no = step - (totalStep - 15);
                left = 15 + no;
            }

            for (int i = 0; i < totalStep; i++)
            {
                if (i == 0)
                {
                    stepButtons = AnytimeClass.GetStepData(i, stepButtons, true);
                    stepButtons += ",";
                }
                else if (i == totalStep - 1)
                {
                    stepButtons = AnytimeClass.GetStepData(i, stepButtons, true);
                }
                else if (i >= step - left && i <= step + right)
                {
                    stepButtons = AnytimeClass.GetStepData(i, stepButtons, true);
                    stepButtons += ",";
                }
            }

            return stepButtons;
        }
    }
}