using System;
using System.Collections.Generic;
using System.Web;
using ExpertChoice.Data;
using System.Web.Services;
using AnytimeComparion.Pages.external_classes;
using ECCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;


namespace AnytimeComparion.Pages
{
    public partial class evaluation_progress : System.Web.UI.Page
    {
        class clsUserEvaluationProgressData
        {
            public int UserID = -1;
            public string Name = "";
            public string Email = "";
            public int EvaluatedCount = 0;
            public int TotalCount = 0;
            public DateTime? LastJudgmentTime = null;
            public string LastJudgmentTimeUTC = "";
            public bool IsDisabled = false;
            public bool IsOnline = false;
            public bool CanEditProject  = false;
            public bool CanEvaluateProject  = false;
            public bool IsLinkEnabled  = true;
            public bool IsLinkGoEnabled  = true;
            public bool IsPreviewLinkEnabled  = true;
            public string EvaluationLink = "";
        }


        protected void Page_Load(object sender, EventArgs e)
        {

        }

        
        private static List<clsUserEvaluationProgressData> GetEvaluationProgress()
        {
            bool IsEvaluationProgressForTreatments = false;
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            List<clsUserEvaluationProgressData> res = new List<clsUserEvaluationProgressData>();
            if (App != null)
            {
                var PM = App.ActiveProject.ProjectManager;
                DateTime startTime = DateTime.Now;
                List<clsApplicationUser> tAllUsers = App.DBUsersByProjectID(App.ProjectID);
                int defTotal = 0;
                //IsEvaluationProgressForTreatments
                if (IsEvaluationProgressForTreatments)
                {
                    defTotal = PM.GetDefaultTotalJudgmentCount(
                        PM.ActiveHierarchy);
                }
                

                List<clsWorkspace> tWSList = App.DBWorkspacesByProjectID(App.ProjectID);
                PM.StorageManager.Reader.LoadUserPermissions();
                PM.StorageManager.Reader.LoadUserDisabledNodes();

                ECTypes.clsUser user;
                int uCount = tAllUsers.Count;
                DateTime StartDT = DateTime.Now;
                string baseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/";
                for (int i = 0; i < uCount; i++)
                {
                    var tAppUser = tAllUsers[i];
                    string email = tAppUser.UserEMail;
                    user = PM.GetUserByEMail(email);
                    string userName = tAppUser.UserName;
                    if (user != null)
                    {
                        userName = user.UserName;
                    }

                    clsUserEvaluationProgressData progress = new clsUserEvaluationProgressData();
                    progress.UserID = tAppUser.UserID;
                    progress.Email = email;
                    progress.Name = !string.IsNullOrEmpty(userName) ? userName : email;
                    progress.IsOnline = tAppUser.isOnline;

                    clsWorkspace tWS = clsWorkspace.WorkspaceByUserIDAndProjectID(tAppUser.UserID, App.ProjectID,
                        tWSList);

                    if (tWS != null)
                    {
                        clsRoleGroup tGroup = App.ActiveWorkgroup.get_RoleGroup(tWS.GroupID,
                            App.ActiveWorkgroup.RoleGroups);
                        if (tGroup != null)
                        {
                            progress.CanEditProject =
                                (tGroup.get_ActionStatus(ecActionType.at_mlManageProjectOptions) ==
                                 ecActionStatus.asGranted) ||
                                tGroup.get_ActionStatus(ecActionType.at_mlModifyModelHierarchy) ==
                                 ecActionStatus.asGranted; 
                            progress.CanEvaluateProject = App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tAppUser.UserID,
                                    App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup);
                        }
                        progress.IsDisabled =
                            tWS.get_Status(PM.ActiveHierarchy ==
                                           (int) ECTypes.ECHierarchyID.hidImpact) ==
                            ecWorkspaceStatus.wsDisabled;

                        //User Link Enabled/Disabled
                        if (tAppUser.CannotBeDeleted)
                        {
                            progress.IsLinkEnabled = false;
                        }
                        if (progress.IsLinkEnabled &&
                            !App.CanUserModifyProject(tAppUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, tWS))
                        {
                            progress.IsLinkEnabled = false;
                        }
                        if (progress.IsLinkGoEnabled && App.ActiveProject != null &&
                            (App.ActiveProject.isTeamTimeImpact || App.ActiveProject.isTeamTimeLikelihood) &&
                            App.ActiveUser != null && App.ActiveUser.UserID == App.ActiveProject.MeetingOwnerID)
                        {
                            progress.IsLinkGoEnabled = false;
                        }


                        //User Open Link Enabled/Disabled
                        if (tAppUser.CannotBeDeleted)
                        {
                            progress.IsLinkGoEnabled = false;
                        }
                        if(progress.IsLinkEnabled  && !App.CanUserModifyProject(tAppUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, tWS, App.ActiveWorkgroup))
                        {
                            progress.IsLinkGoEnabled = false;
                        }
                        if (progress.IsLinkGoEnabled && App.ActiveProject != null && (App.ActiveProject.isTeamTimeImpact || App.ActiveProject.isTeamTimeLikelihood) && App.ActiveUser != null&& App.ActiveUser.UserID == App.ActiveProject.MeetingOwnerID)
                        {
                            progress.IsLinkGoEnabled = false;
                        }

                        if (progress.IsLinkGoEnabled && progress.IsDisabled)
                        {
                            progress.IsLinkGoEnabled = false;
                        }

                       
                        // User Preview Enabled/Disabled
                        if (App.ActiveProject != null)
                        {
                            if (!progress.CanEvaluateProject)
                            {
                                progress.IsPreviewLinkEnabled = false;
                            }
                            if (progress.IsPreviewLinkEnabled &&
                                (App.ActiveProject.isTeamTimeImpact || App.ActiveProject.isTeamTimeLikelihood))
                            {
                                progress.IsPreviewLinkEnabled = false;
                            }
                        }
                    }

                    //IsEvaluationProgressForTreatments
                    if (IsEvaluationProgressForTreatments) { 
                        progress.IsLinkEnabled = false;
                        progress.IsLinkGoEnabled = false;
                        progress.IsPreviewLinkEnabled = false;
                    }

                    if (user != null)
                    {
                        DateTime userLastJudgmentTime = ECTypes.VERY_OLD_DATE;
                        int totalCount = 0;
                        int madeCount = 0;

                        //IsEvaluationProgressForTreatments
                        if (IsEvaluationProgressForTreatments)
                        {
                            //Evaluation progress for Riskion Treatments
                            PM.PipeBuilder.GetControlsEvaluationProgress(user.UserID, ref madeCount, ref totalCount,
                                ref userLastJudgmentTime);
                        }
                        else
                        {
                            totalCount =
                                PM.ProjectAnalyzer.GetTotalJudgmentsCount(user.UserID, PM.ActiveObjectives.HierarchyID,
                                    int.MinValue, false);
                            madeCount =
                                PM.GetMadeJudgmentCount(
                                    PM.ActiveHierarchy, user.UserID,
                                    ref userLastJudgmentTime);
                        }

                        madeCount = madeCount > totalCount ? totalCount : madeCount;

                        if (user != PM.User && !IsEvaluationProgressForTreatments)
                        {
                            foreach (clsNode node in PM.get_Hierarchy(PM.ActiveHierarchy).Nodes)
                            {
                                node.Judgments.DeleteJudgmentsFromUser(user.UserID);
                            }
                            PM.UsersRoles.CleanUpUserRoles(user.UserID);
                        }

                        progress.EvaluatedCount = madeCount;
                        progress.TotalCount = totalCount;

                        if (madeCount > 0)
                        {
                            progress.LastJudgmentTime = userLastJudgmentTime;
                            progress.LastJudgmentTimeUTC = userLastJudgmentTime.ToString();
                        }
                        else
                        {
                            progress.LastJudgmentTime = null;
                            progress.LastJudgmentTimeUTC = "";
                        }

                    }
                    else
                    {
                        progress.EvaluatedCount = 0;
                        progress.TotalCount = defTotal;
                        progress.LastJudgmentTime = null;
                        progress.LastJudgmentTimeUTC = "";
                    }

                    if (progress.IsLinkEnabled || progress.IsLinkGoEnabled || progress.IsPreviewLinkEnabled)
                    {

                        progress.EvaluationLink = baseUrl + GeckoClass.CreateLogonURL(tAppUser, App.ActiveProject, false, "", "");
                    }

                    res.Add(progress);

                }

            }

            return res;
        }



        [WebMethod(EnableSession = true)]
        public static object GetEvaluationProgressData()
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];

            List<clsUserEvaluationProgressData> res = new List<clsUserEvaluationProgressData>();
            if (App != null)
            {
                if (App.ActiveProject == null)
                {
                    var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    return oSerializer.Serialize(new { error="No project is open!" });
                }
            }

            List<clsUserEvaluationProgressData> data = GetEvaluationProgress();
            int Cnt = 0;
            double Total = 0;

         

            //over all progress
            foreach (clsUserEvaluationProgressData item in data)
            {
                if (item.TotalCount > 0)
                {
                    Total += Convert.ToDouble(item.EvaluatedCount) / Convert.ToDouble(item.TotalCount);
                    Cnt += 1;
                }
            }

            double OverallProgress = 0;

            if (Cnt != 0)
            {
                OverallProgress = Convert.ToInt32(100*(Total/Cnt));
            }

            Object output = new
            {
                evaluation_progress = data,
                OverallProgress =OverallProgress
            };

            try
            {
                var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return oSerializer.Serialize(output);
            }
            catch (Exception e)
            {
                //catch if function have recursive arrays
                return JsonConvert.SerializeObject(output, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                        });
            }

        }
    }
}