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
    public class Authentication
    {
        public static List<clsApplicationUser> _ProjectUsers = null;
        public static List<clsApplicationUser> ProjectUsers
        {
            get
            {
                var App = (clsComparionCore)HttpContext.Current.Session["App"];
                if (_ProjectUsers == null)
                    _ProjectUsers = App.DBUsersByProjectID(App.ProjectID);
                return _ProjectUsers;
            }
        }
        public static List<clsWorkspace> _ProjectWorkspaces = null;
        public static List<clsWorkspace> ProjectWorkspaces
        {
            get
            {
                var App = (clsComparionCore)HttpContext.Current.Session["App"];
                if (_ProjectWorkspaces == null)
                    _ProjectWorkspaces = App.DBWorkspacesByProjectID(App.ProjectID);
                return _ProjectWorkspaces;
            }
        }

        public static void ApplyChanges(bool fSetSynchronousActive)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            // D0211
            // D0194 + D0200 ===
            var a = App.ActiveProject.ID;
            var Users = (List<clsApplicationUser>)App.DBUsersByProjectID(App.ActiveProject.ID);
            // D0506
            if (clsApplicationUser.UserByUserID(App.ActiveUser.UserID, Users) == null)
                Users.Add(App.ActiveUser);
            // D0383 + D0506
            List<clsWorkspace> SyncStatus = new List<clsWorkspace>();
            // D0383 + D0506

            // D0217 ===
            int DefGrpID = App.ActiveWorkgroup.get_GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator);
            // D0506
            foreach (ECTypes.clsUser tAHPUser in App.ActiveProject.ProjectManager.UsersList)
            {
                clsApplicationUser tUser = clsApplicationUser.UserByUserEmail(tAHPUser.UserEMail, Users);
                // D0506
                tAHPUser.IncludedInSynchronous = (tUser != null);
                if (tAHPUser.IncludedInSynchronous)
                {
                    clsWorkspace tWS = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, App.ActiveProject.ID, ProjectWorkspaces);
                    // D0506
                    clsWorkspace tWSOld = null;
                    // D0600
                    if (tWS != null)
                        tWSOld = tWS.Clone();
                    //D0660
                    // D0646 ===
                    if (clsApplicationUser.UserByUserID(tUser.UserID, Users) == null)
                    {
                        // D0660
                        if (tWS != null)
                        {
                            if (tWS.get_Status(App.ActiveProject.isImpact) != ecWorkspaceStatus.wsDisabled)
                                tWS.set_Status(App.ActiveProject.isImpact, ecWorkspaceStatus.wsEnabled);
                            // D1945
                            tWSOld.set_Status(App.ActiveProject.isImpact, tWS.get_Status(App.ActiveProject.isImpact));
                            // D0660 + D1945
                        }
                        // tAHPUser.IncludedInSynchronous = False  ' -D0660
                    }
                    else
                    {
                        int tMode = -1;

                        if (tWS == null)
                        {
                            tWS = App.AttachProject(tUser, App.ActiveProject, false, DefGrpID, "", false);
                            // D0506 + D2287 + D2644
                        }
                        if (tWS != null)
                            tWS.set_Status(App.ActiveProject.isImpact, tMode == 2 ? ecWorkspaceStatus.wsSynhronousReadOnly : ecWorkspaceStatus.wsSynhronousActive);
                        // D1490 + D1945
                        //SyncStatus.Add(tWS.Clone) '-D0660 // add on end of for..next
                        // D0383 ==
                        int PadID = 0;
                        bool fHasPad = false;
                        tAHPUser.VotingBoxID = 0;
                        // D0632
                        // D0383
                        if ((tMode == 1))
                        {
                            tAHPUser.VotingBoxID = PadID;
                            fHasPad = true;
                        }
                        tAHPUser.SyncEvaluationMode = (fHasPad ? ECTypes.SynchronousEvaluationMode.semVotingBox : ECTypes.SynchronousEvaluationMode.semOnline);
                        if (tAHPUser.SyncEvaluationMode != ECTypes.SynchronousEvaluationMode.semVotingBox) { tAHPUser.VotingBoxID = 0; }
                    }
                    // D0660 ===
                    if (tWS != null)
                    {
                        tWS.set_TeamTimeStatus(App.ActiveProject.isImpact, tWS.get_Status(App.ActiveProject.isImpact));
                        // D1945
                        tAHPUser.IncludedInSynchronous = tWS.get_isInTeamTime(App.ActiveProject.isImpact);
                        // D1945
                        // D1945
                        if (tWS.get_isInTeamTime(App.ActiveProject.isImpact))
                        {
                            SyncStatus.Add(tWS.Clone());
                        }
                        bool fWSChanged = false;
                        if (tWSOld == null)
                            fWSChanged = true;
                        else
                            fWSChanged = tWSOld.get_Status(App.ActiveProject.isImpact) != tWS.get_Status(App.ActiveProject.isImpact) | tWSOld.get_TeamTimeStatus(App.ActiveProject.isImpact) != tWS.get_TeamTimeStatus(App.ActiveProject.isImpact);
                        // D1945
                        if (fWSChanged)
                        {
                            if (!fSetSynchronousActive && tWSOld != null && !App.ActiveProject.isTeamTime && tWS.get_isInTeamTime(App.ActiveProject.isImpact))
                                tWS.set_Status(App.ActiveProject.isImpact, tWSOld.get_Status(App.ActiveProject.isImpact));
                            // D1945
                            App.DBWorkspaceUpdate(ref tWS, false, "Update user workspace for new TT settings");

                        }
                    }
                    // D0660 ==
                }
            }

            // D0217 ==

            //Dim isStart As Boolean = btnLaunch.CommandArgument <> ""
            App.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure();
            // D0660
            var Project = App.ActiveProject;

                App.DBProjectUpdateToLastVersion(ref Project);
                App.ActiveProject.StatusDataLikelihood = 1;
                App.ActiveProject.StatusDataImpact = 1;
                App.ActiveProject.isOnline = true;
                var proj = App.ActiveProject;
                App.DBProjectUpdate(ref proj);
  
            var ActHierarchy = (ECTypes.ECHierarchyID)Project.ProjectManager.ActiveHierarchy;
            var own = App.ActiveUser;
            if (fSetSynchronousActive)
            {
                App.TeamTimeStartSession(ref own, ref Project, ActHierarchy, ref Users, ref SyncStatus, true, PipeParameters.ecAppliationID.appGecko);
                // D0383 + D0506 + D1734
            }
            else
            {
                // D0660 ===
                if (App.ActiveProject.MeetingOwnerID != App.ActiveUser.UserID && App.ActiveProject.LockInfo.isLockAvailable(App.ActiveUser.UserID))
                {
                    App.ActiveProject.MeetingOwner = App.ActiveUser;
                    App.DBProjectUpdate(ref Project, false, "Set TeamTime session owner");
                }
                // D0660 ==
            }
            // D0194 ==


        }
    }
}