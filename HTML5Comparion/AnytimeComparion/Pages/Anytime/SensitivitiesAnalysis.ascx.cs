using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using ECCore;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Service;
using System.Web.Services;
using System.Web.Script.Serialization;
using AnytimeComparion.Pages.external_classes;
using System.IO;
using System.Collections.Specialized;
using ExpertChoice.Web;

namespace AnytimeComparion.Pages.Anytime
{
    public partial class SensitivitiesAnalysis : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public object initSA()
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var WorkSpace = (clsWorkspace)App.ActiveWorkspace;
            var CurrentStep = WorkSpace.get_ProjectStep(App.ActiveProject.isImpact);
            var AnytimeAction = (clsAction)AnytimeClass.GetAction(CurrentStep);

            CurrentUserID = App.ActiveProject.ProjectManager.UserID;
            SetSaUserId(App);

            if (AnytimeAction.ActionType == ActionType.atSensitivityAnalysis)
            {
                clsSensitivityAnalysisActionData sensitivities = (clsSensitivityAnalysisActionData)AnytimeAction.ActionData;
                _SA_Data = sensitivities;
                lblKeepSortedAlts = TeamTimeClass.ResString("lblSAKeppSorted");
                lblRefreshCaption = TeamTimeClass.ResString("btnReset");
                lblShowLines = TeamTimeClass.ResString("lblSAShowLines");
                lblLineUp = TeamTimeClass.ResString("lblSALineUp");
                lblShowLegend = TeamTimeClass.ResString("lblSAShowLegend");
                lblSelectNode = TeamTimeClass.PrepareTask(TeamTimeClass.ResString("lblSASelectNode"));
                msgHint = TeamTimeClass.PrepareTask(TeamTimeClass.GetPipeStepTask(AnytimeClass.Action(CurrentStep), null));

                //for messages to display as a title
                msgSeeingCombined = TeamTimeClass.ResString("lblSASeeingCombined");
                msgSeeingIndividual = TeamTimeClass.ResString("lblSASeeingIndividual");
                msgSeeingUser = TeamTimeClass.ResString("lblSASeeingForUser");
                InitComponent();
            }
            var output = new
            {
                lblSeeing = lblSeeing,
                lblMessage = lblMessage,
                GetSAObjectives = GetSAObjectives(),
                GetSAAlternatives = GetSAAlternatives(),
                GetDecimalsValue = GetDecimalsValue(),
                GetOptions = GetOptions(),
                GetNodesList = GetNodesList(),
                GetGSASubobjectives = GetGSASubobjectives(),
                msgHint = msgHint,
                ACTION_DSA_UPDATE_VALUES = ACTION_DSA_UPDATE_VALUES,
                ACTION_DSA_RESET = ACTION_DSA_RESET,
                GetSATypeString = GetSATypeString(),
                lblRefreshCaption = lblRefreshCaption,
                Opt_ShowYouAreSeeing = Opt_ShowYouAreSeeing,
                pnlLoadingID = pnlLoadingID,
                sensitivities = _SA_Data

            };
            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return oSerializer.Serialize(output);
        }

        public void SetSaUserId(clsComparionCore app)
        {
            if (app.Options.BackDoor == ExpertChoice.Web.Options._BACKDOOR_PLACESRATED)
            {
                Opt_ShowMaxAltsCount = 10;
                SAUserID = app.ActiveProject.ProjectManager.UserID;
            }
            else
            {
                SAUserID = (app.ActiveProject.PipeParameters.CalculateSAForCombined ? ECTypes.COMBINED_USER_ID : app.ActiveProject.ProjectManager.UserID);
            }
        }

        public void clearData() {
            ObjPriorities.Clear();
            AltValues.Clear();
            ProjectManager = null;
            AltValuesInZero.Clear();
            AltValuesInOne.Clear();
            SAUserID = int.MinValue;
            CurrentUserID = int.MinValue;
            _NodesList = null;

        }

        public const string ACTION_DSA_UPDATE_VALUES = "dsa_update_values";

        public const string ACTION_DSA_RESET = "dsa_reset";
        public string msgNoEvaluationData = "no data for {0}";
        public string msgNoGroupData = "no group data";
        public string msgSeeingCombined = "combined";
        public string msgSeeingIndividual = "individual";
        public string msgSeeingUser = "user";

        public string msgHint = "drag bars";
        public string lblNormalization = "Normaization: ";
        public string lblSeeing = "";
        public string lblMessage = "";
        public string lblSelectNode = "Select: ";
        public string lblRefreshCaption = "Refresh";
        // D3477
        public string lblKeepSortedAlts = "Freeze order of alternatives (?)";
        // D3477
        public string lblShowLines = "Show lines";
        public string lblLineUp = "Align Labels";
        // D3481
        public string lblShowLegend = "Show Legend";


        public string pnlLoadingID = "";
        public clsProjectManager ProjectManager = null;

        public Dictionary<AlternativeNormalizationOptions, string> NormalizationsList = new Dictionary<AlternativeNormalizationOptions, string>();
        public int Opt_ShowMaxAltsCount = -1;
        public bool Opt_ShowYouAreSeeing = true;

        public bool Opt_isMobile = false;
        private int _Current_UserID = int.MinValue;
        private int _SA_UserID = int.MinValue;
        // D2987
        private clsSensitivityAnalysisActionData _SA_Data = null;
        // D2114
        private AlternativeNormalizationOptions _NormalizationMode = AlternativeNormalizationOptions.anoPercentOfMax;

        private Dictionary<int, string> _NodesList = null;
        // D3473
        public const string _SESS_SA_NORMALIZATION = "SANormMode";
        // D3473
        public const string _SESS_SA_WRT_NODE = "SAWrtNode";
        // D2686
        public const bool _OPT_IGNORE_CATEGORIES = true;


        // D2987 ===
        public clsSensitivityAnalysisActionData Data
        {
            get { return _SA_Data; }
            set { _SA_Data = value; }
        }
        // D2987 ==

        // D2988 ===
        public SAType SAType
        {
            get
            {
                if (Data == null)
                    return Canvas.SAType.satNone;
                else
                    return Data.SAType;
            }
        }
        // D2988 ==

        // D3473 ===
        internal string ModelID()
        {
            if (ProjectManager == null)
                return "";
            else
                return string.Format("_{0}", ProjectManager.StorageManager.ModelID);
        }
        // D3473 ==

        // D2114 ===
        public AlternativeNormalizationOptions NormalizationMode
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context.Session[_SESS_SA_NORMALIZATION + ModelID()] != null)
                {
                    _NormalizationMode = (AlternativeNormalizationOptions)context.Session[_SESS_SA_NORMALIZATION + ModelID()];
                }
                return _NormalizationMode;
            }
            set
            {
                _NormalizationMode = value;
                HttpContext.Current.Session.Remove(_SESS_SA_NORMALIZATION + ModelID());
                HttpContext.Current.Session.Add(_SESS_SA_NORMALIZATION + ModelID(), _NormalizationMode);
            }
        }
        // D2114 ==

        public clsNode CurrentNode
        {
            get
            {
                HttpContext context = HttpContext.Current;
                int NodeID = -1;
                if (context.Session[_SESS_SA_WRT_NODE + ModelID()] != null)
                {
                    NodeID = Convert.ToInt32(context.Session[_SESS_SA_WRT_NODE + ModelID()]);
                }
                clsNode tNode = null;
                if (ProjectManager != null)
                {
                    tNode = ProjectManager.get_Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(NodeID);
                    if (tNode == null && ProjectManager.get_Hierarchy(ProjectManager.ActiveHierarchy).Nodes.Count > 0)
                        tNode = ProjectManager.get_Hierarchy(ProjectManager.ActiveHierarchy).Nodes[0];
                }
                // D3473 ==
                return tNode;
            }
            set
            {
                if (ProjectManager != null && value != null)
                {
                    HttpContext.Current.Session.Remove(_SESS_SA_WRT_NODE + ModelID());
                    // D3473
                    HttpContext.Current.Session.Add(_SESS_SA_WRT_NODE + ModelID(), value.NodeID);
                    // D3473
                }
            }
        }

        public int SAUserID
        {
            get
            {
                if (_SA_UserID == int.MinValue && ProjectManager != null)
                {
                    _SA_UserID = ProjectManager.UserID;
                }
                return _SA_UserID;
            }
            set { _SA_UserID = value; }
        }

        public int CurrentUserID
        {
            get
            {
                if (_Current_UserID == int.MinValue && ProjectManager != null)
                {
                    _Current_UserID = ProjectManager.UserID;
                }
                return _Current_UserID;
            }
            set { _Current_UserID = value; }
        }
        // D0374 ==

        // D3000
        private void GetOverTerminalNodes(ref Dictionary<int, string> tNodesList, clsNode pNode, string margin)
        {
            // D1905 + D2686
            if (pNode != null && !pNode.IsTerminalNode && !pNode.get_DisabledForUser(CurrentUserID))
            {
                //Dim fIgnoreCategories As Boolean = _OPT_IGNORE_CATEGORIES AndAlso Project.IsRisk  ' D2686
                //If (Not fIgnoreCategories OrElse pNode.RiskNodeType <> RiskNodeType.ntCategory) Then tNodesList.Add(pNode) ' D2686
                tNodesList.Add(pNode.NodeID, StringFuncs.ShortString((string.IsNullOrEmpty(margin) ? "" : margin.Replace(" ", "&nbsp;") + "• ") + StringFuncs.SafeFormString(pNode.NodeName), 50 - margin.Length));
                // D3474
                // D3000
                foreach (clsNode cNode in pNode.GetNodesBelow(SAUserID))
                {
                    GetOverTerminalNodes(ref tNodesList, cNode, margin + " ");
                    // D3474
                }
            }
        }

        // D3000
        private void GetNodesWithParents(ref Dictionary<int, string> tNodesList, clsNode pNode, string margin)
        {
            bool fIgnoreCategories = _OPT_IGNORE_CATEGORIES && ProjectManager.IsRiskProject;
            // D2686
            // D3000
            foreach (clsNode cNode in pNode.GetNodesBelow(SAUserID))
            {
                // D1905
                if (!cNode.get_DisabledForUser(CurrentUserID) && !cNode.IsAlternative)
                {
                    if ((!fIgnoreCategories || cNode.RiskNodeType != ECTypes.RiskNodeType.ntCategory))
                    {
                        tNodesList.Add(cNode.NodeID, margin + cNode.NodeName);
                    }
                    if (cNode.GetNodesBelow(SAUserID) != null)
                        GetNodesWithParents(ref tNodesList, cNode, margin + " • ");
                }
            }
        }

        public Dictionary<int, string> NodesList
        {
            get
            {
                if (_NodesList == null)
                {
                    _NodesList = new Dictionary<int, string>();
                    clsNode tRoot = ProjectManager.get_Hierarchy(ProjectManager.ActiveHierarchy).Nodes[0];
                    switch (SAType)
                    {
                        //Case Canvas.SAType.satGradient
                        //    GetNodesWithParents(_NodesList, tRoot, " ")
                        default:
                            GetOverTerminalNodes(ref _NodesList, tRoot, "");
                            break;
                    }
                }
                return _NodesList;
            }
        }

        public string GetNodesList()
        {
            string sList = "";
            if (NodesList != null && NodesList.Count > 1)
            {
                foreach (int ID in NodesList.Keys)
                {
                    string sActive = "";
                    if (CurrentNode != null && CurrentNode.NodeID == ID)
                        sActive = " selected";
                    sList += string.Format("<option value='{0}'{1}>{2}</option>", ID, sActive, NodesList[ID]);
                    // D3473
                }
                sList = string.Format("{0} <select id='nodes' onchange='onChangeNode(this.value);' style='width:210px'>{1}</option>", lblSelectNode, sList);
            }
            if (string.IsNullOrEmpty(sList))
                sList = "";
            return sList;
        }

        public List<object> GetGSASubobjectives()
        {
            var retVal = new List<object>();

            ECCore.clsNode wrtNode = CurrentNode;
            if (wrtNode != null)
            {
                dynamic i = 0;
                foreach (ECCore.clsNode obj in wrtNode.GetNodesBelow(SAUserID))
                {
                    string sActive = "";
                    var list = new List<object>();
                    list.Add(i);
                    list.Add(sActive);
                    list.Add(obj.NodeName);
                    retVal.Add(list);
                    i += 1;
                }
            }
            return retVal;
        }

        internal string GetNormalizationList()
        {
            string sList = "";
            if (NormalizationsList != null && NormalizationsList.Count > 1)
            {
                foreach (AlternativeNormalizationOptions ID in NormalizationsList.Keys)
                {
                    string sActive = "";
                    if (NormalizationMode == ID)
                        sActive = " selected";
                    sList += string.Format("<option value='{0}'{1}>{2}</option>", Convert.ToInt32(ID), sActive, StringFuncs.SafeFormString(NormalizationsList[ID]));
                }
                sList = string.Format("{0} <select id='norm_mode' onchange='onChangeNormalization(this.value);'>{1}</option>", lblNormalization, sList);
            }
            if (string.IsNullOrEmpty(sList))
                sList = "&nbsp;";
            return sList;
        }

        // D3477 ===
        public string GetOptions()
        {
            string sRes = "";

            if (SAType == Canvas.SAType.satDynamic)
            {
                sRes += string.Format("&nbsp;<label><input type=checkbox class='checkbox' name='cbKeepSorted' value='1' onclick='onKeepSorted(this.checked);' {0}>{1}</label>", "", lblKeepSortedAlts);
            }

            if (SAType == Canvas.SAType.satPerformance)
            {
                sRes += string.Format("<br /><label class='large-6 small-6 columns'><input type=checkbox class='checkbox' name='cbShowLines' value='1' onclick='onShowLines(this.checked);' {0}>{1}</label>", "checked", lblShowLines);
                sRes += String.Format("<label class='large-6 small-6 columns'><input type=checkbox class='checkbox' name='cbLineUp' value='1' onclick='onLineUp(this.checked);' {0}>{1}</label>", "checked", lblLineUp);
            }

            // D3481 ===
            if (SAType == Canvas.SAType.satGradient)
            {
                sRes += string.Format("&nbsp;<label><input type=checkbox class='checkbox' name='cbShowLegend' value='1' onclick='onShowLegend(this.checked);' {0}>{1}</label>", "checked", lblShowLegend);
            }
            // D3481 ==

            return sRes;
        }
        // D3477 ==

        public void InitComponent()
        {
            string sMessage = "";
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            ProjectManager = App.ActiveProject.ProjectManager;
            if (ProjectManager != null)
            {
                // D0520
                if (NodesList.Count > 0)
                {
                    bool fIsCombined = ECTypes.IsCombinedUserID(SAUserID);
                    bool fCanShowIndividual = false;
                    bool fCanShowGlobal = false;

                    clsJudgmentsAnalyzer PrjAnalyzer = new clsJudgmentsAnalyzer(ProjectManager.PipeParameters.SynthesisMode, ProjectManager);
                    if (!fIsCombined)
                    {
                        fCanShowIndividual = PrjAnalyzer.get_CanShowIndividualResults(SAUserID, CurrentNode);
                    }
                    else
                    {
                        fCanShowGlobal = PrjAnalyzer.get_CanShowGroupResults(CurrentNode);
                    }
                    PrjAnalyzer = null;

                    //C0565 + D0521
                    if ((fIsCombined && !fCanShowGlobal) | (!fIsCombined && !fCanShowIndividual))
                    {
                        sMessage = msgNoGroupData;
                    }
                    else
                    {
                        // D0373
                        if (CurrentNode != null)
                        {
                            // D3473
                            if (!fIsCombined && !fCanShowIndividual)
                            {
                                sMessage = string.Format(msgNoEvaluationData, CurrentNode.NodeName);
                            }


                            if (Opt_ShowYouAreSeeing)
                            {
                                string sSeeing = "";
                                if (SAUserID == ECTypes.COMBINED_USER_ID)
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


                                if (!string.IsNullOrEmpty(sSeeing))
                                    lblSeeing = sSeeing;
                                else
                                    Opt_ShowYouAreSeeing = false;
                            }

                        }

                    }

                }
                else
                {
                    sMessage = msgNoGroupData;
                }

                lblMessage = sMessage;
                //if (!string.IsNullOrEmpty(sMessage))
                //    lblMessage = true;

            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            Anytime.Ajax_Callback(Request.Form.ToString());
        }

        //[WebMethod(EnableSession = true)]
        //public static void Ajax_Callback(string data)
        //{
        //    NameValueCollection args = HttpUtility.ParseQueryString(data);
        //    string sAction = Common.GetParam(args, ExpertChoice.Web.Options._PARAM_ACTION).Trim().ToLower();
        //    string sResult = Convert.ToString((string.IsNullOrEmpty(sAction) ? "" : sAction));

        //    switch (sAction)
        //    {

        //        case "node":
        //            int tNodeID = -1;
        //            if (int.TryParse(Common.GetParam(args, "node_id").ToLower(), out tNodeID))
        //            {
        //                CurrentNode = ProjectManager.get_Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(tNodeID);
        //                ObjPriorities.Clear();
        //                AltValues.Clear();
        //                AltValuesInOne.Clear();
        //                ProjectManager.CalculationsManager.InitializeSAGradient(CurrentNode.NodeID, false, SAUserID, ref ObjPriorities, ref AltValues, ref AltValuesInOne);
        //                AltValuesInZero = ProjectManager.CalculationsManager.GetGradientData(CurrentNode.NodeID, false, SAUserID, ObjPriorities);
        //                // D3473
        //                sResult = GetSAData();
        //            }

        //            break;
        //        case "normalization":
        //            int tID = -1;
        //            if (int.TryParse(Common.GetParam(args, "norm_mode").ToLower(), out tID))
        //            {
        //                NormalizationMode = (AlternativeNormalizationOptions)tID;
        //                sResult = GetSAData();
        //            }
        //            break;
        //        case ACTION_DSA_UPDATE_VALUES:
        //            string s_values = Common.GetParam(args, "values").Trim();
        //            string[] values = s_values.Split(Convert.ToChar(","));
        //            string s_ids = Common.GetParam(args, "objids").Trim();
        //            string[] ids = s_ids.Split(Convert.ToChar(","));
        //            Dictionary<int, double> ANewObjPriorities = new Dictionary<int, double>();
        //            dynamic i = 0;
        //            foreach (string objID_loopVariable in ids)
        //            {
        //                var objID = objID_loopVariable;
        //                double APrty = 0;
        //                Double.TryParse(values[i], out APrty);
        //                ANewObjPriorities.Add(Convert.ToInt32(objID), APrty);
        //                i += 1;
        //            }
        //            updateAltValuesinZero(ANewObjPriorities);
        //            string ZeroValuesString = "";
        //            foreach (KeyValuePair<int, Dictionary<int, double>> Objitem_loopVariable in AltValuesInZero)
        //            {
        //                var Objitem = Objitem_loopVariable;
        //                string ZeroAltValuesString = "";
        //                foreach (KeyValuePair<int, double> AltItem_loopVariable in Objitem.Value)
        //                {
        //                    var AltItem = AltItem_loopVariable;
        //                    ZeroAltValuesString += Convert.ToString((!string.IsNullOrEmpty(ZeroAltValuesString) ? "," : "")) + string.Format("{{altID:{0},val:{1}}}", AltItem.Key, StringFuncs.JS_SafeNumber(AltItem.Value));
        //                }
        //                ZeroValuesString += Convert.ToString((!string.IsNullOrEmpty(ZeroValuesString) ? "," : "")) + string.Format("[{0},[{1}]]", Objitem.Key, ZeroAltValuesString);
        //            }
        //            sResult = string.Format("[[{0}]]", ZeroValuesString);
        //            break;
        //    }

        //    if (!string.IsNullOrEmpty(sResult))
        //    {
        //        HttpContext.Current.Response.Clear();
        //        HttpContext.Current.Response.ContentType = "text/plain";
        //        HttpContext.Current.Response.Write(sResult);
        //        HttpContext.Current.Response.End();
        //    }

        //}

        public string GetSATypeString()
        {
            switch (SAType)
            {
                case Canvas.SAType.satDynamic:
                    return "DSA";
                case Canvas.SAType.satGradient:
                    return "GSA";
                case Canvas.SAType.satPerformance:
                    return "PSA";
            }
            return "None";
        }

        internal string GetSAData()
        {
            return string.Format("[{0}, {1}]", GetSAObjectives(), GetSAAlternatives());
        }


        #region "SA"
        //Data example for sa plugin
        //alts: [{ id: 0, name: 'Alt1', value: 0.7, initValue: 0.7, color: '#95c5f0' },
        //       { id: 1, name: 'Alt2', value: 0.3, initValue: 0.3, color: '#fa7000'}],
        //objs: [{ id: 1, name: 'Obj1', value: 0.3, initValue: 0.3, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.5}], gradientMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}], gradientInitMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}] },
        //   { id: 2, name: 'Obj2', value: 0.5, initValue: 0.5, gradientMaxValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.5}], gradientMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7}], gradientInitMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7}] },
        //   { id: 3, name: 'Obj3', value: 0.2, initValue: 0.2, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.3}], gradientMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1}], gradientInitMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1}]}]
        public string[] AltColors = {
"#344d94",
    "#cb3034",
    "#9d27a8",
    "#e3e112",
    "#00687d",
    "#407000",
    "#f24961",
    "#663d2e",
    "#9600fa",
    "#ffbde6",
    "#00c49f",
    "#7280c4",
    "#009180",
    "#e33000",
    "#80bdff",
    "#a10040",
    "#0affe3",
    "#00523c",
    "#919100",
    "#5c00f7",
    "#a15f00",
    "#cce6ff",
    "#00465c",
    "#adff69",
    "#f24ba0",
    "#0dff87",
    "#ff8c47",
    "#349400",
    "#b3b3a1",
    "#a10067",
    "#ba544a",
    "#edc2d1",
    "#00e8c3",
    "#3f0073",
    "#5ec1f7",
    "#6e00b8",
    "#f5f5c4",
    "#e33000",
    "#52ba00",
    "#ff943b",
    "#0079db",
    "#f0e6c0",
    "#ffb517",
    "#cf0076",
    "#e8cfc9"

};
        public Dictionary<int, double> ObjPriorities = new Dictionary<int, double>();
        public Dictionary<int, double> AltValues = new Dictionary<int, double>();
        public Dictionary<int, Dictionary<int, double>> AltValuesInOne = new Dictionary<int, Dictionary<int, double>>();

        public Dictionary<int, Dictionary<int, double>> AltValuesInZero = new Dictionary<int, Dictionary<int, double>>();

        internal void initSAData()
        {
            if (ObjPriorities.Count == 0)
            {
                ECCore.clsHierarchy H = ProjectManager.get_Hierarchy(ProjectManager.ActiveHierarchy);
                if (H != null)
                {
                    ECCore.clsNode wrtNode = CurrentNode;
                    if (wrtNode != null)
                    {
                        ObjPriorities.Clear();
                        AltValues.Clear();
                        AltValuesInOne.Clear();
                        ProjectManager.CalculationsManager.InitializeSAGradient(wrtNode.NodeID, false, SAUserID, ref ObjPriorities, ref AltValues, ref AltValuesInOne, 0);
                        AltValuesInZero = ProjectManager.CalculationsManager.GetGradientData(wrtNode.NodeID, false, SAUserID, ObjPriorities, 0);
                    }
                }
            }
        }

        internal void updateAltValuesinZero(Dictionary<int, double> NewObjPriorities)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            ProjectManager = App.ActiveProject.ProjectManager;

            ECCore.clsHierarchy H = ProjectManager.get_Hierarchy(ProjectManager.ActiveHierarchy);
            if (H != null)
            {
                ECCore.clsNode wrtNode = CurrentNode;
                if (wrtNode != null)
                {
                    AltValuesInZero = ProjectManager.CalculationsManager.GetGradientData(wrtNode.NodeID, false, SAUserID, NewObjPriorities, 0);
                }
            }
        }

        public string GetSAObjectives()
        {
            string retVal = "";
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            ProjectManager = App.ActiveProject.ProjectManager;
            ECCore.clsHierarchy H = ProjectManager.get_Hierarchy(ProjectManager.ActiveHierarchy);
            ECCore.clsHierarchy altH = ProjectManager.get_AltsHierarchy(ProjectManager.ActiveAltsHierarchy);
            initSAData();
            if (ObjPriorities.Count > 0 & altH != null & H != null)
            {
                ECCore.clsNode wrtNode = CurrentNode;
                if (wrtNode != null)
                {
                    var i = 0;
                    foreach (ECCore.clsNode obj in wrtNode.GetNodesBelow(SAUserID))
                    {
                        Guid AttrGuid = ECCore.Attributes.ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID;
                        if (ProjectManager.ActiveHierarchy == (int) ECCore.ECTypes.ECHierarchyID.hidImpact)
                        {
                            AttrGuid = ECCore.Attributes.ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID;
                        }

                        string sNodeColor = "";
                        long tNodeColor = Convert.ToInt64(ProjectManager.Attributes.GetAttributeValue(AttrGuid, obj.NodeGuidID));
                        sNodeColor = tNodeColor > 0 ? WebPageUtils.LongToBrush(tNodeColor) : Misc.GetPaletteColor(Misc.CurrentPaletteID(ProjectManager), obj.NodeID);

                        string gradientMaxValues = "";
                        string gradientMinValues = "";
                        foreach (int altID in AltValuesInOne[obj.NodeID].Keys)
                        {
                            gradientMaxValues += Convert.ToString(!string.IsNullOrEmpty(gradientMaxValues) ? "," : "") + $"{{altID:{altID},val:{StringFuncs.JS_SafeNumber(AltValuesInOne[obj.NodeID][altID])}}}";
                        }

                        foreach (int altID in AltValuesInZero[obj.NodeID].Keys)
                        {
                            gradientMinValues += Convert.ToString(!string.IsNullOrEmpty(gradientMinValues) ? "," : "") + $"{{altID:{altID},val:{StringFuncs.JS_SafeNumber(AltValuesInZero[obj.NodeID][altID])}}}";
                        }

                        retVal += Convert.ToString((!string.IsNullOrEmpty(retVal) ? "," : "")) + $"{{id:{obj.NodeID},idx:{i},name:'{StringFuncs.JS_SafeString(obj.NodeName)}',value:{StringFuncs.JS_SafeNumber(ObjPriorities[obj.NodeID])},initValue:{StringFuncs.JS_SafeNumber(ObjPriorities[obj.NodeID])},gradientMaxValues:[{gradientMaxValues}],gradientMinValues:[{gradientMinValues}],gradientInitMinValues:[{gradientMinValues}],color:'{sNodeColor}'}}";
                        i += 1;
                    }
                }
            }

            retVal = $"[{retVal}]";
            return retVal;
        }

        public string GetSAAlternatives()
        {
            string retVal = "";
            ECCore.clsHierarchy altH = ProjectManager.get_AltsHierarchy(ProjectManager.ActiveAltsHierarchy);
            initSAData();
            if (AltValues.Count > 0 & altH != null)
            {
                dynamic i = 0;
                foreach (ECCore.clsNode alt in altH.get_TerminalNodes())
                {
                    var altColor = Convert.ToInt64(ProjectManager.Attributes.GetAttributeValue(ECCore.Attributes.ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID));
                    var sAltColor = altColor > 0 ? WebPageUtils.LongToBrush(altColor) : Misc.GetPaletteColor(Misc.CurrentPaletteID(ProjectManager), alt.NodeID, true);
                    retVal += Convert.ToString((!string.IsNullOrEmpty(retVal) ? "," : "")) + string.Format("{{id:{0},idx:{1},name:'{2}',value:{3},initValue:{4},color:'{5}',visible:1}}", alt.NodeID, i, StringFuncs.JS_SafeString(alt.NodeName), StringFuncs.JS_SafeNumber(AltValues[alt.NodeID]), StringFuncs.JS_SafeNumber(AltValues[alt.NodeID]), sAltColor);
                    i += 1;

                }
            }
            retVal = string.Format("[{0}]", retVal);
            return retVal;
        }

        public int GetDecimalsValue()
        {
            return Convert.ToInt32(ProjectManager.Attributes.GetAttributeValue(ECCore.Attributes.ATTRIBUTE_SYNTHESIS_DECIMALS_ID, ECTypes.UNDEFINED_USER_ID));
        }
        #endregion

    }
}
