using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using ECCore;
using ExpertChoice.Data;
using ECService = ExpertChoice.Service;
using ExpertChoice.Web;
using System.Web.Services;
using AnytimeComparion.Pages.external_classes;
using System.IO;
using Newtonsoft.Json;

namespace AnytimeComparion
{
    public partial class _Default : Page
    {
        public static string debugging;
        public clsComparionCore App;

        internal class clsUserEvaluationProgressData
        {
            public int EvaluatedCount = 0;
            public int TotalCount = 0;
            public DateTime? LastJudgmentTime = null;
            public string LastJudgmentTimeUTC = "";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var app = (clsComparionCore)Session["App"];

            var messageError = "";
            var isMasterExists = app.isCanvasMasterDBValid;
            var isProjectsExists = app.isCanvasProjectsDBValid;
            var isDbExists = (isMasterExists & isProjectsExists);

            // If there's an error in DB or DB is not existing case 15243
            if (!isDbExists)
            {
                var dbName = "";
                if (!isProjectsExists)
                {
                    dbName = app.Options.CanvasProjectsDBName;
                }

                if (!isMasterExists)
                {
                    dbName = app.Options.CanvasMasterDBName;
                }

                messageError = "<div style=\'text-align:center\' class='alert-box alert radius tt-alert msg-stat' data-alert>"
                               + string.Format(TeamTimeClass.ResString("msgErrorDBConnection"), dbName)
                               + "<a href='#' class='close'>&times;</a></div>";
            }

            //var hash = Convert.ToString(Request.QueryString["hash"]);
            if (Session["User"] != null)
            {
                //var sss = app.ActiveUser.UserName;
            }

            var userRole = false;
            if (app.ActiveUser != null)
            {
                userRole = isPM();
                //var sss = app.ActiveUserWorkgroup;
            }

            if (app.ActiveUser != null)
            {
                UserName.InnerHtml = app.ActiveUser.UserName;
            }

            HttpContext.Current.Session["isMember"] = userRole;

            if (app.ActiveUser != null)
            {
                if (HttpContext.Current.Session["LoggedInViaHash"] != null && (bool)HttpContext.Current.Session["LoggedInViaHash"])
                {
                    
                }
                else
                {
                    if (!HttpContext.Current.Request.Path.EndsWith("my-projects.aspx", StringComparison.InvariantCultureIgnoreCase) && Request.QueryString["hash"] == null)
                        Response.Redirect("~/pages/my-projects.aspx");
                }
            }

            if (Request.Cookies["Filters"] == null)
            {
                var filter = new HttpCookie("Filters");
                filter.Values.Add("ProjectStatus", "1");
                filter.Values.Add("ProjectAccess", "1");
                filter.Values.Add("LastAccess", "0");
                filter.Values.Add("LastModified", "0");
                filter.Values.Add("DateCreated", "0");
                filter.Values.Add("OverallJudgmentProcess", "0");
                filter.Expires = DateTime.Now.AddDays(10);
                Response.Cookies.Add(filter);
            }

            if (Request.Cookies["HideWarningMessage"] == null)
            {
                var warningCookie = new HttpCookie("HideWarningMessage", "1")
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(1)
                };

                Response.Cookies.Add(warningCookie);
            }

            if (Request.QueryString["noMsg"] != null && Request.QueryString["noMsg"] == "1")
            {
                Response.Cookies["HideWarningMessage"].Value = "1";
            }

            Div2.InnerHtml = messageError;
        }

        [WebMethod(EnableSession = true)]
        public static Object login(string email, string password, string passcode, string rememberme, string MeetingID, string meeting_name, string meeting_email)
        {
            HttpContext context = HttpContext.Current;
            if (context.Session[Constants.Sess_LoginMethod] != null)
            {
                if ((int)context.Session[Constants.Sess_LoginMethod] != 1)
                {
                    context.Session[Constants.Sess_LoginMethod] = 0;
                }
            }
            else
            {
                context.Session[Constants.Sess_LoginMethod] = 0;
            }



            //AD: Put your e-mail/psw here for login:

            // load language after login
            LoadLanguange();


            //get credentials
            bool fUserExist = true;
            bool success = false;
            string message = "";
            var sPasscode = "";
            var userrole = false;

            var App = (ExpertChoice.Data.clsComparionCore)context.Session["App"];
            var passwordAuth = true;
            
            if (passcode != "")
            {
                System.Diagnostics.Trace.Write("-----------" + passcode + "-----------------");
                return loginbyPasscode(App, email, password, passcode);
            }
            //---------------------------------team time  codes below---------------------
            if (MeetingID != "")
            {
                return loginbyMeetingID(App, MeetingID, meeting_email, meeting_name);
            }
            //---------------------------------team time codes above-------------------------
            var AuthRes = ecAuthenticateError.aeUnknown;
            if (context != null && context.Session != null)
            {
                if (email != "pm" && email != "evaluator" && email != "participant")
                {
                    var LastWorkgroupID = -1;
                    var currentUser = App.DBUserByEmail(email);
                    if(currentUser != null)
                    {
                        LastWorkgroupID = currentUser.Session.WorkgroupID;
                    }
                   
                    AuthRes = App.Logon(email, password, ref sPasscode, false, true, false);
                    
                    switch (AuthRes)
                    {
                        case ecAuthenticateError.aeWrongPassword:
                            if(password == "")
                            {
                                passwordAuth = false;
                            }
                            break;
                        default:
                            break;
                    }



                    if (AuthRes != ExpertChoice.Data.ecAuthenticateError.aeNoErrors)
                    {
                        //message = String.Format("Error: Can't pass authentication. Details: {0}", AuthRes);

                        message = ProcessLoginAuthentication(App, AuthRes, email);

                        if (ExpertChoice.Data.ecAuthenticateError.aeNoUserFound == AuthRes)
                            fUserExist = false;

                        //if (AuthRes == ExpertChoice.Data.ecAuthenticateError.aeNoWorkgroupSelected) //depricated
                        //{
                        //    int LastVisitedWGID = -1;
                        //    // If last visited workgroup has been saved before

                        //    if ((App.ActiveUser.Session != null) && App.ActiveUser.Session.WorkgroupID > 0)
                        //    {
                        //        message = "If last visited workgroup has been saved before!";
                        //        if (App.UserWorkgroups.Count > 1)
                        //        {
                        //            LastVisitedWGID
                        //                = App.ActiveUser.Session.WorkgroupID;
                        //            // Find wkg by ID
                        //            ExpertChoice.Data.clsWorkgroup WorkGroup = ExpertChoice.Data.clsWorkgroup.WorkgroupByID(LastVisitedWGID, App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups));
                        //            // Check below, Is this wkg allowed for open
                        //            App.ActiveWorkgroup = WorkGroup;
                        //            if (WorkGroup != null)
                        //            {
                        //                //add this later && WorkGroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate, null, true)
                        //                if (WorkGroup.Status == ExpertChoice.Data.ecWorkgroupStatus.wsEnabled && WorkGroup.License.isValidLicense)
                        //                {

                        //                    App.ActiveWorkgroup = WorkGroup;
                        //                    App.ActiveUserWorkgroup = null;
                        //                    App.Workspaces = null;
                        //                    App.ActiveProject = null;
                        //                    AuthRes = ExpertChoice.Data.ecAuthenticateError.aeNoErrors;
                        //                    success = true;

                        //                    System.Diagnostics.Trace.Write("\nSuccess Login!");

                        //                    var user = App.ActiveUser;
                        //                    //user.UserPassword = {new Passwor?}
                        //                    context.Session["User"] = user;
                        //                    context.Response.Cookies["fullname"].Value = user.UserName.ToString();
                        //                    System.Diagnostics.Trace.Write("user:" + user.UserName);
                        //                    success = true;

                        //                    System.Diagnostics.Trace.Write("Login:" + email + password);

                        //                }
                        //            }
                        //            else
                        //            {
                        //                message = "No Work Group Found!";
                        //            }
                        //        }
                        //        else if (App.UserWorkgroups.Count == 1)
                        //        {
                        //            // Find wkg by ID
                        //            ExpertChoice.Data.clsWorkgroup WorkGroup = ExpertChoice.Data.clsWorkgroup.WorkgroupByID(App.ActiveUser.Session.WorkgroupID, App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups));
                        //            if (App.ActiveWorkgroup == null)
                        //            {
                        //                WorkGroup = App.DBWorkgroupByID(2);
                        //            }
                        //            // Check below, Is this wkg allowed for open
                        //            if (WorkGroup != null)
                        //            {
                        //                //add this later && WorkGroup.License.CheckParameterByID    (ecLicenseParameter.ExpirationDate, null, true)
                        //                if (WorkGroup.Status == ExpertChoice.Data.ecWorkgroupStatus.wsEnabled && WorkGroup.License.isValidLicense)
                        //                {

                        //                    App.ActiveWorkgroup = WorkGroup;
                        //                    App.ActiveUserWorkgroup = null;
                        //                    App.Workspaces = null;
                        //                    App.ActiveProject = null;
                        //                    AuthRes = ExpertChoice.Data.ecAuthenticateError.aeNoErrors;
                        //                    success = true;

                        //                    System.Diagnostics.Trace.Write("\nSuccess Login!");

                        //                    var user = App.ActiveUser;
                        //                    //user.UserPassword = {new Passwor?}
                        //                    context.Session["User"] = user;
                        //                    context.Response.Cookies["fullname"].Value = user.UserName.ToString();
                        //                    System.Diagnostics.Trace.Write("user:" + user.UserName);
                        //                    success = true;

                        //                    System.Diagnostics.Trace.Write("Login:" + email + password);



                        //                }
                        //            }
                        //            else
                        //            {
                        //                message = "No Work Group Found!";
                        //            }
                        //        }
                        //        else
                        //        {
                        //            success = true;
                        //            //show all the workgroups
                        //            //App.AvailableWorkgroups() 
                        //            message += String.Format("Choose a workgroup first: {0}", App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups));
                        //        }
                        //    }
                        //    else
                        //    {
                        //        List<ExpertChoice.Data.clsWorkgroup> Workgroups = (List<ExpertChoice.Data.clsWorkgroup>)App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups);

                        //        foreach (ExpertChoice.Data.clsWorkgroup workgroup in Workgroups)
                        //        {
                        //            message += String.Format("Choose a workgroup first: {0}", workgroup.Name);
                        //        }

                        //        context.Session["Workgroups"] = Workgroups;
                        //    }
                        //}
                    }
                    else
                    {
                        //If NO ERROR

                        System.Diagnostics.Trace.Write("-4-");
                        System.Diagnostics.Trace.Write("\nSuccess Login!");
                        App.ActiveUser.Session.WorkgroupID = LastWorkgroupID;
                        App.DBUserUpdate(App.ActiveUser, false);
                        var workgroup = App.DBWorkgroupByID(LastWorkgroupID);
                        App.ActiveWorkgroup = workgroup;
                        var user = App.ActiveUser;
                        //user.UserPassword = {new Passwor?}
                        context.Session["User"] = user;
                        context.Response.Cookies["fullname"].Value = user.UserName.ToString();
                        System.Diagnostics.Trace.Write("user:" + user.UserName);
                        success = true;
                        fUserExist = true;



                    }
                }
                else
                {

                    if (email == "pm")
                    {
                        context.Session["UserType"] = "pm";
                    }
                    if (email == "evaluator")
                    {
                        context.Session["UserType"] = "evaluator";
                    }
                    if (email == "participant")
                    {
                        context.Session["UserType"] = "participant";
                    }
                    success = true;
                }
            }
            bool mobile = false;

            if (context.Session["mobile"] != null && (bool)context.Session["mobile"])
            {
                mobile = true;
            }


            userrole = isPM();


            Object output = new
            {
                success = success,
                message = message,
                mobile = mobile,
                teamtime = false,
                userrole = userrole,
                passwordAuth = passwordAuth,
                userExist = fUserExist,
                is_invalid = fUserExist && passwordAuth
            };

            //remember me
            if(App.ActiveUser != null)
            {
                if (rememberme == "True")
                {

                    //expiration
                    context.Response.Cookies["usernam"].Expires = DateTime.Now.AddDays(10);
                    context.Response.Cookies["passwor"].Expires = DateTime.Now.AddDays(10);
                    context.Response.Cookies["rmberme"].Expires = DateTime.Now.AddDays(10);
                    context.Response.Cookies["fullname"].Expires = DateTime.Now.AddDays(10);


                }
                else
                {
                    context.Response.Cookies["usernam"].Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies["passwor"].Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies["rmberme"].Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies["fullname"].Expires = DateTime.Now.AddDays(-1);
                }
                context.Response.Cookies["rmberme"].Value = "1";
                context.Response.Cookies["usernam"].Value = email;
                context.Response.Cookies["passwor"].Value = password;
            }
            else
            {
                context.Response.Cookies["usernam"].Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies["passwor"].Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies["rmberme"].Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies["fullname"].Expires = DateTime.Now.AddDays(-1);
            }



            context.Session["isMember"] = userrole;

            SiteMaster.storepageinfo();

            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return oSerializer.Serialize(output);
        }

        private static string ProcessLoginAuthentication(clsComparionCore app, ecAuthenticateError authRes, string email)
        {
            var message = TeamTimeClass.ParseAllTemplates(app.GetMessageByAuthErrorCode(authRes), app.ActiveUser, null);

            if (authRes == ecAuthenticateError.aeWrongPassword)
            {
                if (float.Parse(app.CanvasMasterDBVersion) >= 0.9996 && app.Database.Connect() && email != "")
                {
                    try
                    {
                        var user = app.DBUserByEmail(email);
                        if (user != null)
                        {
                            var newStatus = GetWrongPasswordAttemptsFromLog(app, user);
                            var oldStatus = user.PasswordStatus;
                            user.PasswordStatus = newStatus;

                            if (user.PasswordStatus != oldStatus)
                            {
                                var sqlParams = new List<object> { email };

                                app.Database.ExecuteSQL($"UPDATE {clsComparionCore._TABLE_USERS} SET PasswordStatus = {user.PasswordStatus} WHERE {clsComparionCore._FLD_USERS_EMAIL}=?", sqlParams);

                                if (user.PasswordStatus == Consts._DEF_PASSWORD_ATTEMPTS)
                                {
                                    CheckUserPasswordStatusAndSendEmail(app, user, Consts._DEF_PASSWORD_ATTEMPTS);
                                }

                                System.Threading.Thread.Sleep(1000);
                            }

                            if (user.PasswordStatus >= Consts._DEF_PASSWORD_ATTEMPTS)
                            {
                                app.DBUpdateDateTime(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_LASTVISITED, user.UserID);
                                authRes = ecAuthenticateError.aeUserLockedByWrongPsw;
                                message = TeamTimeClass.ParseAllTemplates(app.GetMessageByAuthErrorCode(authRes), app.ActiveUser, null);
                            }

                            app.DBSaveLogonEvent(email, authRes, ecAuthenticateWay.awRegular, HttpContext.Current.Request, message);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            if (authRes == ecAuthenticateError.aeUserLockedByWrongPsw)
            {
                message = string.Format(message, "Password Help?");
                message += "<br> " + string.Format(TeamTimeClass.ResString("msgAuthContact"), WebOptions.SystemEmail).Replace("class='error'", "style='text-decoration: underline;'");
            }
            else if(authRes != ecAuthenticateError.aeWrongPassword)
            {
                var passCode = (string)HttpContext.Current.Session[Constants.Sess_SignUp_Passcode];
                if (!string.IsNullOrEmpty(passCode))
                {
                    var project = app.DBProjectByPasscode(passCode);
                    var newMessage = AnytimeClass.GetProjectInAccessibleMessage(project, email, false);
                    message = string.IsNullOrEmpty(newMessage) ? message : newMessage;
                }
            }

            return message;
        }

        private static int GetWrongPasswordAttemptsFromLog(clsComparionCore app, clsApplicationUser user)
        {
            var errorCount = 0;
            if (Consts._DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK)
            {
                var actionIds = $"{(int)dbActionType.actLogon},{(int)dbActionType.actTokenizedURLLogon},{(int)dbActionType.actCredentialsLogon},{(int)dbActionType.actUnLock}";
                var sqlCommand = $"SELECT * FROM Logs WHERE ActionID IN ({actionIds}) AND TypeID={(int)dbObjectType.einfUser} AND (ObjectID={user.UserID} OR COMMENT LIKE ?) AND DT>? ORDER BY DT";
                var sqlParams = new List<object>();
                sqlParams.Add($"{user.UserEMail} %");
                sqlParams.Add(DateTime.Now.AddMinutes(-Consts._DEF_PASSWORD_ATTEMPTS_PERIOD));
                var tries = app.Database.SelectBySQL(sqlCommand, sqlParams);

                foreach (var row in tries)
                {
                    if (row["Result"] == DBNull.Value) continue;

                    if (Convert.ToInt32(row["ActionID"]) == (int)dbActionType.actUnLock)
                    {
                        errorCount = 0;
                    }
                    else
                    {
                        var result = row["Result"].ToString().ToLower();
                        if (result.StartsWith(ecAuthenticateError.aeWrongPassword.ToString().ToLower()) || result.StartsWith(ecAuthenticateError.aeUserLockedByWrongPsw.ToString().ToLower()))
                        {
                            if (result.StartsWith(ecAuthenticateError.aeUserLockedByWrongPsw.ToString().ToLower()) && errorCount < Consts._DEF_PASSWORD_ATTEMPTS)
                            {
                                errorCount = Consts._DEF_PASSWORD_ATTEMPTS;
                            }
                            else
                            {
                                errorCount++;
                            }
                        }
                        else
                        {
                            errorCount = 0;
                        }
                    }
                }
            }
            else
            {
                errorCount = user.PasswordStatus;
            }

            errorCount = errorCount < 0 ? 0 : errorCount;
            errorCount++;
            errorCount = errorCount > Consts._DEF_PASSWORD_ATTEMPTS ? Consts._DEF_PASSWORD_ATTEMPTS : errorCount;

            return errorCount;
        }

        [WebMethod(EnableSession = true)]
        public static Boolean logout()
        {
            var context = HttpContext.Current;
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            if(App.ActiveProject != null)
                if (App.ActiveProject.isTeamTime)
                    TeamTimeClass.TeamTimeUsersList = null; //this refresh the list when user logs out

            //var loginMethod = (int) context.Session[Sess_LoginMethod];
            //if (loginMethod == 1)

            App.Logout();
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
            HttpContext.Current.Response.Cookies["rmberme"].Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies["fullname"].Expires = DateTime.Now.AddDays(-1);
            
            return true;
        }

        [WebMethod(EnableSession = true)]
        public static string FillStartMeetingModal()
        {
            var App = (ExpertChoice.Data.clsComparionCore)HttpContext.Current.Session["App"];
            Nullable<DateTime> DT = null;
            bool isTeamTime = false;
            if (App.ActiveProject != null)
            {
                if (App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionPasscode, ref  DT) != null)
                {
                    isTeamTime = true;
                }
                var output = new
                {
                    isTeamTime = isTeamTime,
                    isTeamTimeMeetingOwner = TeamTimeClass.isTeamTimeOwner,
                    CurrentWorkgroup = App.ActiveWorkgroup.Name,
                    CurrentProject = App.ActiveProject.ProjectName
                };
                var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return oSerializer.Serialize(output);

            }
            return "";
        }

        [WebMethod(EnableSession = true)]
        public static bool StartMeeting(bool IfRestartPipe)
        {
            var context = HttpContext.Current;
            var App = (ExpertChoice.Data.clsComparionCore)context.Session["App"];
            var pascode = App.ActiveProject.Passcode;
            var Authres = App.Logon(App.ActiveUser.UserEMail, App.ActiveUser.UserPassword, ref pascode, false, true, false);
            context.Session["Project"] = App.ActiveProject;
            if (!TeamTimeClass.isTeamTime)
            {
                Authentication.ApplyChanges(true);
                context.Session["TTstart"] = true;
                if (IfRestartPipe)
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
                    var workspace = App.DBWorkspaceByUserIDProjectID(MeetingOwner, App.ProjectID);
                    workspace.set_ProjectStep(App.ActiveProject.isImpact, 1);
                    App.DBWorkspaceUpdate(ref workspace, false, null);
                }
                TeamTimeClass.RemoveTeamTime();
            }
            return true;
            //Response.Redirect("~/pages/teamtimetest/teamtime.aspx");
        }

        public static object loginbyPasscode(clsComparionCore App, string email, string password, string passcode)
        {
            bool success = false;
            string message = "";
            bool mobile = false;
            bool teamtime = false;
            bool anytime = false;
            var passwordAuth = true;
            HttpContext context = HttpContext.Current;
            if (App.DBProjectByPasscode(passcode) != null)
            {
                var AuthRes = App.Logon(email, password, ref passcode, false, true, false);
                if (AuthRes == ecAuthenticateError.aeNoErrors)
                {
                    context.Session["User"] = App.ActiveUser;
                    context.Response.Cookies["fullname"].Value = App.ActiveUser.UserName.ToString();
                    context.Session["Project"] = App.ActiveProject;

                    if (App.ActiveProject.isTeamTime == true)
                    {
                        success = true;
                        teamtime = true;
                    }
                    else
                    {
                        success = true;
                        anytime = true;
                        StartAnytime(App.ProjectID);
                    }
                }
                else if (AuthRes == ecAuthenticateError.aeWrongPassword)
                {
                    if (password == "")
                    {
                        passwordAuth = false;
                    }
                }
                if (AuthRes != ExpertChoice.Data.ecAuthenticateError.aeNoErrors)
                {
                    message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(AuthRes), App.ActiveUser, null);
                }
                    //if (context.Session["mobile"] != null && (bool)context.Session["mobile"])
                    //{
                    //    mobile = true;
                    //}


                }
            else
            {
                success = false;
                message = "The access code you have selected is not recognized. Please check the spelling and try again. If it still does not work, please contact the project manager to get the correct access code";
            }



            Object output = new
            {
                success = success,
                message = message,
                mobile = mobile,
                teamtime = teamtime,
                passwordAuth = passwordAuth,
                anytime = anytime
            };

            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return oSerializer.Serialize(output);
        }

        public static object loginbyMeetingID(clsComparionCore App, string MeetingID, string meeting_email, string meeting_name)
        {
            bool success = false;
            string message = "";
            bool mobile = false;
            bool teamtime = false;
            HttpContext context = HttpContext.Current;
            if (MeetingID.Contains("-"))
            {
//                var splitID = MeetingID.Split('-');
//                for (int i = 0; i < splitID.Length; i++)
//                {
//                    meetID += splitID[i];
//                }
                MeetingID = MeetingID.Replace("-", "");
            }

            var meet = (clsProject)App.DBProjectByMeetingID(Convert.ToInt64(MeetingID));
            if (meet != null)
            {
                var fUserNotExists = false;
                var sPasscode = meet.Passcode;
                if (App.DBUserByEmail(meeting_email) == null)
                {
                    var CurrentProject = App.DBProjectByPasscode(sPasscode);
                    if (CurrentProject.PipeParameters.TeamTimeAllowMeetingID)
                    {
                        var sUserExists = "";
                        App.UserWithSignup(meeting_email, meeting_name, "admin", "Quick Sign-up", ref sUserExists, true);
                        fUserNotExists = true;
                    }
                    else
                    {
                        return false;
                    }
                }
                var user = App.DBUserByEmail(meeting_email);
                var email = user.UserEMail;
                var password = user.UserPassword;

                var AuthRes = App.Logon(email, password, ref sPasscode, false, true, false);
                context.Session["User"] = App.ActiveUser;
                context.Session["meetingid"] = MeetingID;
                context.Response.Cookies["fullname"].Value = App.ActiveUser.UserName.ToString();
                context.Session["Project"] = meet;
                if (context.Session["mobile"] != null && (bool)context.Session["mobile"])
                {
                    mobile = true;
                }

                if (App.ActiveProject.isTeamTime == true)
                {
                    success = true;
                    teamtime = true;
                }
                else
                {
                    success = true;
                    teamtime = false;
                }
                var URlRedirection = "";
                if (App.ActiveProject.isTeamTime)
                {
                    //URlRedirection = "pages/teamtimetest/teamtime.aspx";
                }
                if (App.ActiveProject != null)
                {
                    if (!isPM())
                    {
                        //URlRedirection = "pages/teamtimetest/teamtime.aspx";
                    }

                }


                SiteMaster.storepageinfo();

                Object output = new
                {
                    success = success,
                    message = message,
                    mobile = mobile,
                    teamtime = teamtime,
                    url = URlRedirection

                };



                var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return oSerializer.Serialize(output);

            }
            else
            {
                Object output = new
                {
                    success = false,
                    message = "Invalid Details. Please try again."
                };
                var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return oSerializer.Serialize(output);
            }
        }

        public static bool isPM()
        {

            var _isPM = false;
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            if (App.ActiveRoleGroup == null) { return false; }
            if (App.ActiveRoleGroup.GroupType != ecRoleGroupType.gtUser)
            {
                _isPM = true;
            }
            return _isPM;
        }

        [WebMethod(EnableSession = true)]
        public static void preferences(string option, string value)
        {
            HttpContext context = HttpContext.Current;
            context.Session[option] = value;
        }

        [WebMethod(EnableSession = true)]
        public static bool get_preferences(string option)
        {
            HttpContext context = HttpContext.Current;
            bool has_gradient = false;
            if (context.Session[option] != null)
            {
                string session_str = (string)(context.Session[option]);
                if (session_str == "1")
                {
                    has_gradient = true;
                }
                else
                {
                    has_gradient = false;
                }
            }
            return has_gradient;
        }

        [WebMethod(EnableSession = true)]
        public static void SaveInfoDocs(string nodetxt, string obj, string node, int current_step, int node_id, string node_guid = "")
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            //get path
            var CurrentProject = App.ActiveProject;

            var node_data = (Object[,])GeckoClass.GetInfoDocData(obj, node, current_step, node_id, node_guid);
            var sBasePath = (string)node_data[0, 0];
            var tempnode = (clsNode)node_data[0, 1];
            var _ObjectType = (Consts.reObjectType)node_data[0, 2];
            var ParentNodeID = (int)node_data[0, 3];
            var ObjHierarchy = (clsHierarchy)CurrentProject.HierarchyObjectives;
            var ParentNode = (clsNode)ObjHierarchy.GetNodeByID(ParentNodeID);
            var is_CovObj = (bool)node_data[0, 5];
            var action_type= (Canvas.ActionType) node_data[0, 6];
            bool is_Multi = (bool)node_data[0, 4];
            Guid additionalGuid = Guid.Empty;
           
            if (obj == "0")
            {
                if (is_CovObj)
                {
                    TeamTimeClass.SetClusterPhraseForNode(tempnode.NodeGuidID, nodetxt, is_Multi, additionalGuid);
                }
                else
                {
                    if (action_type == Canvas.ActionType.atShowLocalResults  || action_type == Canvas.ActionType.atShowGlobalResults || action_type == Canvas.ActionType.atSensitivityAnalysis)
                    {
                        additionalGuid = ParentNode.NodeGuidID;
                    }

                  
                    var result = TeamTimeClass.SetClusterPhraseForNode(ParentNode.NodeGuidID, nodetxt, is_Multi, additionalGuid);

                    if (result && additionalGuid != Guid.Empty)
                    {
                        App.SaveProjectLogEvent(App.ActiveProject, "Update custom cluster phrase", false, "");
                    }
                }
                
            }
            else
            {
                try
                {

                    var sInfoDoc = ECService.InfodocService.Infodoc_Pack(nodetxt, Consts._FILE_ROOT, sBasePath);

                    if (_ObjectType == Consts.reObjectType.AltWRTNode)
                    {
                       CurrentProject.ProjectManager.InfoDocs.SetNodeWRTInfoDoc(tempnode.NodeGuidID, ParentNode.NodeGuidID, sInfoDoc);

                       //InfodocService.Infodoc_Pack(nodetxt, Consts._FILE_ROOT, sBasePath);

                    }
                    else if (_ObjectType == Consts.reObjectType.MeasureScale)
                    {
                        tempnode.MeasurementScale.Comment = ECService.InfodocService.Infodoc_Pack(nodetxt, Consts._FILE_ROOT, sBasePath);
                    }
                    else
                    {
                        tempnode.InfoDoc = ECService.InfodocService.Infodoc_Pack(nodetxt, Consts._FILE_ROOT, sBasePath);

                    }

                    if (_ObjectType == Consts.reObjectType.MeasureScale)
                    {
                        CurrentProject.ProjectManager.StorageManager.Writer.SaveModelStructure();
                    }
                    else
                    {
                        CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs();
                    }

                }
                catch (Exception e)
                {
                    var test = e;
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static string GetInfoDocPath(int ProjectID, int ActiveHierarchyID, Consts.reObjectType InfoDocType, string sInfodocID, int WRTParentNode)
        {
            return ECService.InfodocService.Infodoc_Path(ProjectID, ActiveHierarchyID, InfoDocType, sInfodocID, WRTParentNode);
        }

        [WebMethod(EnableSession = true)]
        public static string UploadImage(string Based64BinaryString, string node_type, string node_location, int current_step, int node_id=0)
        {

            string result = "";
            try
            {
                string format = "";
                var node_data = (Object[,])GeckoClass.GetInfoDocData(node_type, node_location, current_step, node_id);
                var path = node_data[0, 0];
                string name = DateTime.Now.ToString("hhmmss");

                if (Based64BinaryString.Contains("data:image/jpeg;base64,"))
                {
                    format = "jpg";
                }
                if (Based64BinaryString.Contains("data:image/png;base64,"))
                {
                    format = "png";
                }

                string str = Based64BinaryString.Replace("data:image/jpeg;base64,", " ");//jpg check
                str = str.Replace("data:image/png;base64,", " ");//png check

                byte[] data = Convert.FromBase64String(str);


                MemoryStream ms = new MemoryStream(data, 0, data.Length);
                ms.Write(data, 0, data.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                image.Save(path + "\\media\\" + name + ".jpg");
                string full_path = path + "\\media\\" + name + ".jpg";
                string[] ShortPath = full_path.Split(new string[] { "DocMedia" }, StringSplitOptions.None);
                result = "\\DocMedia\\" + ShortPath[1];

            }
            catch (Exception ex)
            {
                result = "Error : " + ex;
            }
            return result;
        }

        [WebMethod(EnableSession = true)]
        public static void RememberFilter(bool filter, string filtername)
        {
            HttpContext context = HttpContext.Current;
            var cookie = context.Request.Cookies["Filters"];
            string value = (filter == true) ? "1" : "0";
            switch (filtername)
            {
                case "project_status":
                    cookie.Values["ProjectStatus"] = value;
                    break;
                case "project_access":
                    cookie.Values["ProjectAccess"] = value;
                    break;
                case "last_access":
                    cookie.Values["LastAccess"] = value;
                    break;
                case "last_modified":
                    cookie.Values["LastModified"] = value;
                    break;
                case "date_created":
                    cookie.Values["DateCreated"] = value;
                    break;
                case "overal_judgment_process":
                    cookie.Values["OverallJudgmentProcess"] = value;
                    break;
            }
            context.Response.AppendCookie(cookie);
        }
        [WebMethod(EnableSession = true)]
        public static Object getWorkgroupsandProjects(int ecProjectStatus = 0)
        {
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            if(ecProjectStatus == -1)
            {
                ecProjectStatus = (int)context.Session[Constants.Sess_Project_Status];
            }
            var ProjectStatus = (ecProjectStatus)ecProjectStatus;
            context.Session[Constants.Sess_Project_Status] = ProjectStatus;
            if (App.ActiveUser != null)
            {
                var Workgroups = App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups);

                var CurrentWorkgroup = new object[2];
                try
                {
                    if (App.ActiveWorkgroup == null)
                    {
                        var LastVisitedWGID = App.ActiveUser.Session.WorkgroupID;
                        CurrentWorkgroup[0] = LastVisitedWGID;
                        if (CurrentWorkgroup[0] == null)
                        {
                            CurrentWorkgroup[0] = Workgroups[0].ID;
                        }
                    }
                    else
                    {
                        CurrentWorkgroup[0] = App.ActiveWorkgroup.ID;
                    }
                }
                catch
                {

                }



                var WorkgroupsandProjects = new List<object[]>();
                try
                {
                    for (int i = 0; i < Workgroups.Count; i++)
                    {
                        var msg = "";
                        var IfAvailable = App.CheckLicense(Workgroups[i], ref msg, true);
                        if (IfAvailable)
                        {
                            var TempWorkGroup = new object[2];
                            TempWorkGroup[0] = Workgroups[i].ID;
                            TempWorkGroup[1] = Workgroups[i].Name;
                            WorkgroupsandProjects.Add(TempWorkGroup);
                            if (Convert.ToInt32(CurrentWorkgroup[0]) == Workgroups[i].ID)
                            {
                                CurrentWorkgroup[1] = Workgroups[i].ID;
                            }
                        }
                    }
                }
                catch
                {

                }

                var CurrentProject = -1;
                if (App.ActiveProject != null) { CurrentProject = App.ActiveProject.ID; }
                var Projects = App.DBProjectsByWorkgroupID(Convert.ToInt32(CurrentWorkgroup[0]));
                var ProjectInfo = new string[Projects.Count][];
                var UserIsPM = new bool();
                var fCanManageAnyDecision = App.CanUserDoAction(ecActionType.at_alManageAnyModel, App.ActiveUserWorkgroup);
                var fCanSeeAllDecision = App.CanUserDoAction(ecActionType.at_alViewAllModels, App.ActiveUserWorkgroup);
                int debugprojects = 0;
                var isAdmin = false;
                var activeWorkgroup = App.ActiveWorkgroup;

                isAdmin = checkRoleGroup;
                for (int i = 0; i < Projects.Count; i++)
                {
                    var fCanModifyProject = App.CanUserModifyProject(App.ActiveUser.UserID, Projects[i].ID,
                        App.ActiveUserWorkgroup,
                        App.ActiveWorkspace);
                    if (isAdmin || fCanModifyProject)
                    {
                        if (Projects[i].isMarkedAsDeleted)
                        {
                            if (Convert.ToInt32(ProjectStatus) == 4)
                            {
                                var meetingOwner = "";
                                if (Projects[i].MeetingOwner != null)
                                    meetingOwner = Projects[i].MeetingOwner.UserEMail.ToString();
                                ProjectInfo[i] = new string[13];
                                ProjectInfo[i][0] = Projects[i].ID.ToString();
                                ProjectInfo[i][1] = Projects[i].ProjectName;
                                ProjectInfo[i][2] = App.DBUserByID(Projects[i].OwnerID).UserName;
                                ProjectInfo[i][3] = Projects[i].isTeamTime.ToString();
                                ProjectInfo[i][4] = Projects[i].isOnline.ToString();
                                ProjectInfo[i][5] = Projects[i].MeetingOwnerID.ToString();
                                ProjectInfo[i][6] = meetingOwner;
                                ProjectInfo[i][7] = Projects[i].LastModify.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
                                ProjectInfo[i][8] = Projects[i].Created.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
                                var userscount = App.DBUsersByProjectID(Projects[i].ID).Count;
                                ProjectInfo[i][9] = userscount.ToString();
                                ProjectInfo[i][10] = Projects[i].LastVisited.HasValue ? Projects[i].LastVisited.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString() : "";
                                ProjectInfo[i][11] = fCanModifyProject.ToString();
                                ProjectInfo[i][12] = Projects[i].isValidDBVersion.ToString();
                                //ProjectInfo[i][12] = GetEvaluationProgressData(Convert.ToInt32(Projects[i].ID)).ToString();
                            }
                        }
                        else
                        {
                            debugprojects += 1;
                            try
                            {
                                if (Projects[i].ProjectStatus == ProjectStatus && (isAdmin || clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, Projects[i].ID, App.Workspaces) != null) && Projects[i].ProjectStatus == ProjectStatus && !Projects[i].isMarkedAsDeleted)
                                {
                                    var meetingOwner = "";
                                    if (Projects[i].MeetingOwner != null)
                                        meetingOwner = Projects[i].MeetingOwner.UserEMail.ToString();
                                    ProjectInfo[i] = new string[13];
                                    ProjectInfo[i][0] = Projects[i].ID.ToString();
                                    ProjectInfo[i][1] = Projects[i].ProjectName;
                                    ProjectInfo[i][2] = App.DBUserByID(Projects[i].OwnerID).UserName;
                                    ProjectInfo[i][3] = Projects[i].isTeamTime.ToString();
                                    ProjectInfo[i][4] = Projects[i].isOnline.ToString();
                                    ProjectInfo[i][5] = Projects[i].MeetingOwnerID.ToString();
                                    ProjectInfo[i][6] = meetingOwner;
                                    ProjectInfo[i][7] = Projects[i].LastModify.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
                                    ProjectInfo[i][8] = Projects[i].Created.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
                                    var userscount = App.DBUsersByProjectID(Projects[i].ID).Count;
                                    ProjectInfo[i][9] = userscount.ToString();
                                    ProjectInfo[i][10] = Projects[i].LastVisited.HasValue ? Projects[i].LastVisited.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString() : "";
                                    ProjectInfo[i][11] = fCanModifyProject.ToString();
                                    ProjectInfo[i][12] = Projects[i].isValidDBVersion.ToString();
                                    //ProjectInfo[i][12] = GetEvaluationProgressData(Convert.ToInt32(Projects[i].ID)).ToString();
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (fCanSeeAllDecision || fCanManageAnyDecision || clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, Projects[i].ID, App.Workspaces) != null && Projects[i].ProjectStatus == ProjectStatus && !Projects[i].isMarkedAsDeleted)
                            {
                                var meetingOwner = "";
                                if (Projects[i].MeetingOwner != null)
                                    meetingOwner = Projects[i].MeetingOwner.UserEMail.ToString();
                                ProjectInfo[i] = new string[13];
                                ProjectInfo[i][0] = Projects[i].ID.ToString();
                                ProjectInfo[i][1] = Projects[i].ProjectName;
                                ProjectInfo[i][2] = App.DBUserByID(Projects[i].OwnerID).UserName;
                                ProjectInfo[i][3] = Projects[i].isTeamTime.ToString();
                                ProjectInfo[i][4] = Projects[i].isOnline.ToString();
                                ProjectInfo[i][5] = Projects[i].MeetingOwnerID.ToString();
                                ProjectInfo[i][6] = meetingOwner;
                                ProjectInfo[i][7] = Projects[i].LastModify.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
                                ProjectInfo[i][8] = Projects[i].Created.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
                                var userscount = App.DBUsersByProjectID(Projects[i].ID).Count;
                                ProjectInfo[i][9] = userscount.ToString();
                                ProjectInfo[i][10] = Projects[i].LastVisited.HasValue ? Projects[i].LastVisited.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString() : "";
                                ProjectInfo[i][11] = fCanModifyProject.ToString();
                                    ProjectInfo[i][12] = Projects[i].isValidDBVersion.ToString();
                                //ProjectInfo[i][12] = GetEvaluationProgressData(Projects[i].ID).ToString();

                            }
                        }
                        catch
                        {

                        }
                    }
                }


                var ListofProjects = new List<string[]>();
                var UserList = new List<int>();
                try
                {
                    for (int i = 0; i < Projects.Count; i++)
                    {
                        if (ProjectInfo[i] != null)
                        {
                            ListofProjects.Add(ProjectInfo[i]);
                        }
                    }
                }
                catch
                {

                }


                
                var CurrentUserID = App.ActiveUser.UserID;
                var s = new System.Web.Script.Serialization.JavaScriptSerializer();
                object[] obj = s.Deserialize<string[]>(HttpContext.Current.Request.Cookies["ProjectListSort"].Value);
                try
                {
                    ListofProjects = ListofProjects.OrderBy(si => si[Convert.ToInt32(obj[0])]).ToList();
                }
                catch{
                    ListofProjects = ListofProjects.OrderBy(si => si[7]).ToList();
                    obj[0] = "7";
                }

                bool freverse;
                if (bool.TryParse((string)obj[1], out freverse))
                {
                    if(freverse)
                        ListofProjects.Reverse();
                }
                var cookie = context.Request.Cookies["Filters"];
                var project_status = cookie.Values["ProjectStatus"] !=null && cookie.Values["ProjectStatus"] == "1" ? true : false;
                var project_access = cookie.Values["ProjectAccess"] != null && cookie.Values["ProjectAccess"] == "1" ? true : false;
                var last_access = cookie.Values["LastAccess"] != null &&  cookie.Values["LastAccess"] == "1" ? true : false;
                var last_modified = cookie.Values["LastModified"] != null &&  cookie.Values["LastModified"] == "1" ? true : false;
                var date_created = cookie.Values["DateCreated"] != null &&  cookie.Values["DateCreated"] == "1" ? true : false;
                var overal_judgment_process = cookie.Values["OverallJudgmentProcess"] != null &&  cookie.Values["OverallJudgmentProcess"] == "1" ? true : false;
                var workgroup_rolegroup = App.ActiveRoleGroup.ID + App.ActiveWorkgroup.ID;

                if (context.Request.Cookies["HideWarningMessage"] == null)
                {
                    var warningCookie = new HttpCookie("HideWarningMessage", "1")
                    {
                        HttpOnly = true,
                        Expires = DateTime.Now.AddDays(1)
                    };

                    context.Request.Cookies.Add(warningCookie);
                }

                bool hideWarning = context.Request.Cookies["HideWarningMessage"].Value == "1";

                var output = new
                {
                    workgroups = WorkgroupsandProjects,
                    projects = ListofProjects,
                    active_workgroup_id = CurrentWorkgroup,
                    active_project_id = CurrentProject,
                    combined_group_id = App.ActiveRoleGroup.ID,
                    role_workgroup_id = App.Options.WorkgroupRoleGroupID,
                    users = UserList,
                    currentUserID = CurrentUserID.ToString(),
                    isPM = isAdmin,
                    pageSize = (string) HttpContext.Current.Session["ProjectListSize"],
                    sort = obj,
                    totalProjects = Projects.Count,
                    debugprojects = debugprojects,
                    ProjectStatus = ProjectStatus,
                    project_status = project_status,
                    project_access = project_access,
                    last_access = last_access,
                    last_modified = last_modified,
                    date_created = date_created,
                    overal_judgment_process = overal_judgment_process,
                    hideBrowserWarning = hideWarning
                };
                var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                //return false;
                return oSerializer.Serialize(output);
            }
            return null;

        }

        [WebMethod(EnableSession = true)]
        public static List<clsApplicationUser> getUsersByProject(int projectID)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var ListofUsers = App.DBUsersByProjectID(projectID);
            return ListofUsers;
        }
        
        private static int GetEvaluationProgressData(int projectID)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            int OverallProgress = 0;
            List<clsUserEvaluationProgressData> data = new List<clsUserEvaluationProgressData>();
                data = GetEvaluationProgress(projectID);
                int Cnt = 0;
                double Total = 0;
                float fPrc = 0;
                foreach (clsUserEvaluationProgressData item in data)
                {
                    if (item.TotalCount > 0)
                    {
                        fPrc = Convert.ToSingle((item.EvaluatedCount * 100 / item.TotalCount));
                        Total += fPrc;
                        Cnt += 1;
                    }
                }
               return OverallProgress = Convert.ToInt32((Total / Cnt));
            
                

        }
        
        private static List<clsUserEvaluationProgressData> GetEvaluationProgress(int ProjectID)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            ECTypes.clsUser user;
            List<clsUserEvaluationProgressData> res = new List<clsUserEvaluationProgressData>();
            var Project = App.DBProjectByID(ProjectID);
            List<clsApplicationUser> tAllUsers = getUsersByProject(ProjectID);
            int uCount = tAllUsers.Count;
            //Evaluation progress default total for Anytime
            int defTotal = Project.ProjectManager.GetDefaultTotalJudgmentCount(Project.ProjectManager.ActiveHierarchy);

            for (int i = 0; i < uCount; i++)
            {
                ECTypes.clsUser tAppUser = tAllUsers[i];
                string email = tAppUser.UserEMail;
                user = Project.ProjectManager.GetUserByEMail(email);
                clsUserEvaluationProgressData progress = new clsUserEvaluationProgressData();
                if (user != null)
                {
                    DateTime userLastJudgmentTime = ECTypes.VERY_OLD_DATE;
                    int totalCount = 0;
                    int madeCount = 0;

                    // Evaluation progress for Anytime
                    totalCount = Project.ProjectManager.ProjectAnalyzer.GetTotalJudgmentsCount(user.UserID,Project.ProjectManager.ActiveObjectives.HierarchyID);
                    madeCount = Project.ProjectManager.GetMadeJudgmentCount(Project.ProjectManager.ActiveHierarchy, user.UserID, ref userLastJudgmentTime);

                    if (madeCount > totalCount)
                        madeCount = totalCount;

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
                res.Add(progress);
            }
            return res;
        }

        private static bool checkRoleGroup
        {
            get
            {
                var isAdmin = false;
                var App = (clsComparionCore)HttpContext.Current.Session["App"];
                switch(App.ActiveRoleGroup.GroupType)
                {
                    case ecRoleGroupType.gtAdministrator:
                    case ecRoleGroupType.gtWorkgroupManager:
                    case ecRoleGroupType.gtECAccountManager:
                    case ecRoleGroupType.gtProjectManager:
                    case ecRoleGroupType.gtProjectOrganizer:
                        isAdmin = true;
                        break;
                }
                return isAdmin;

            }
        }

        [WebMethod(EnableSession = true)]
        public static object StartAnytime(int projID)
        {
            Object output = null;
            var fPass = true;

            try
            {
                var App = (clsComparionCore)HttpContext.Current.Session["App"];
                //App.ActiveProject = null;
                App.ProjectID = projID;
                //App.ActiveProject = App.DBProjectByID(projID);
                var Project = App.DBProjectByID(projID);
                var isAdmin = checkRoleGroup;
              
                if (isAdmin && App.ActiveProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEMail) == null)
                {
                    var adminEmail = App.ActiveUser.UserEMail;
                    App.ActiveProject.ProjectManager.AddUser(adminEmail);
                }
                    if (App.HasActiveProject())
                {
                    
                    if (Project.isTeamTime)
                    {
                        fPass = false;
                    }
                    else
                    {
                        App.ActiveProject.isTeamTimeLikelihood = false;
                        App.ActiveProject.isTeamTimeImpact = false;
                        App.ActiveProject.isTeamTime = false;
                        var sss = (clsNode)Project.HierarchyObjectives.GetLevelNodes(0)[0];
                        HttpContext.Current.Session["Sess_WrtNode"] = sss;

                        //case 11215 - restart the equal message session everytime AT starts
                        HttpContext.Current.Session[Constants.Sess_ShowEqualOnce] = null;

                        AnytimeClass.JudgmentsSaved = false;
                        if (!Project.isValidDBVersion && Project.isDBVersionCanBeUpdated) App.DBProjectUpdateToLastVersion(ref Project);
                        if (Project.isValidDBVersion)
                        {
                            Project.StatusDataLikelihood = 1;
                            Project.StatusDataImpact = 1;
                            Project.isOnline = true;

                        }
                        SiteMaster.storepageinfo();
                    }
                }
                var groupId = (string) HttpContext.Current.Session[Constants.Sess_RoleGroup];
                addUsertoGroup(groupId);

                LoadLanguange();
                output = new
                {
                    name = Project.ProjectName,
                    owner = App.ActiveProject.ProjectManager.User.UserName,
                    start_anytime = fPass,
                    passcode = Project.Passcode,
                };

            }
            catch (Exception e)
            {
                //System.Diagnostics.Trace.Write(HttpContext.Current.Response.)
                //error here
            }

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

        [WebMethod(EnableSession = true)]
        public static List<KeyValuePair<string, string>> getTemplateValues()
        {
            return GeckoClass.getTemplateValues();
        }

        [WebMethod(EnableSession = true)]
        public static object GeneralLinkLogin(string email, string password)
        {
            HttpContext.Current.Session["NewUser"] = false;
            var app = (clsComparionCore)HttpContext.Current.Session["App"];
            var passCode = (string)HttpContext.Current.Session[Constants.Sess_SignUp_Passcode];

            System.Threading.Thread.Sleep(1000);

            var authRes = app.Logon(email, password, ref passCode, false, true, false);
            var isPass = false;
            var message = "";
            var output = new object();
            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            if (authRes == ecAuthenticateError.aeNoErrors)
            {
                HttpContext.Current.Session["User"] = app.ActiveUser;
                HttpContext.Current.Session["Project"] = app.ActiveProject;
                _Default.StartAnytime(app.ActiveProject.ID);
                isPass = true;
                ChangeLastHasToUserSpecific(app);
            }
            else
            {
                message = ProcessLoginAuthentication(app, authRes, email);
            }
            //else if (authRes == ecAuthenticateError.aeWrongPassword)
            //{
            //    message = string.Format(TeamTimeClass.ResString("msgWrongPassword"), app.DBProjectByPasscode(passCode).ProjectName);
            //}
            //else if (authRes == ecAuthenticateError.aeNoUserFound)
            //{
            //    message = string.Format(TeamTimeClass.ResString("msgWrongEmailOrPasswordStart"));
            //}

            output = new
            {
                message = message,
                pass = isPass
            };

            return oSerializer.Serialize(output);
        }

        [WebMethod(EnableSession = true)]
        public static object GeneralLinkSignUp(string email, string name, string password, string sPhone, bool signup_mode)
        {
            var context = HttpContext.Current;
            var app = (clsComparionCore)context.Session["App"];
            var passCode = (string)context.Session[Constants.Sess_SignUp_Passcode];
            var nully = "";

            var isUserExist = app.DBUserByEmail(email) != null;
            var isPass = false;
            var message = "";

            if (isUserExist && email != "")
            {
                message = $"{TeamTimeClass.ResString("errUseRegisteredForm")}";
            }
            else
            {
                var project = app.DBProjectByPasscode(passCode);
                message = AnytimeClass.GetProjectInAccessibleMessage(project, email, false);

                if (string.IsNullOrEmpty(message))
                {
                    var user = app.UserWithSignup(email, name, password, "", ref nully, false);
                    var authRes = app.Logon(email, password, ref passCode, false, true, false);

                    if (user == null || (authRes == ecAuthenticateError.aeNoUserFound && email == ""))
                    {
                        authRes = SiteMaster.forceSignUponAnonymous(project, name);

                        switch (authRes)
                        {
                            case ecAuthenticateError.aePasscodeNotAllowed:
                                message = string.Format(TeamTimeClass.ResString("msgDisabledPasscode"), project.ProjectName);
                                break;
                            case ecAuthenticateError.aeNoErrors:
                                isPass = true;
                                break;
                            case ecAuthenticateError.aeProjectLocked:
                                message = string.Format(TeamTimeClass.ResString("msgEvaluationLocked"), project.ProjectName);
                                break;
                            case ecAuthenticateError.aeProjectReadOnly:
                                message = "Sorry, but decision " + project.ProjectName + " is read-only and not available for collect input judgments. Please contact your Project Organizer to request permission to access this project.";
                                break;
                            default:
                                message = TeamTimeClass.ParseAllTemplates(app.GetMessageByAuthErrorCode(authRes), app.ActiveUser, null);
                                break;
                        }
                    }

                    if (authRes == ecAuthenticateError.aeNoErrors || app.ActiveUser != null)
                    {
                        // context.Response.Cookies["anonymous"].Expires = DateTime.Now.AddDays(3);
                        //context.Response.Cookies["anonymous"].Value = app.ActiveUser.UserEMail;
                        context.Session["User"] = app.ActiveUser;
                        context.Session["Project"] = app.ActiveProject;

                        AddSignUpUserPhoneNumber(app, sPhone);
                        _Default.StartAnytime(app.ActiveProject.ID);

                        isPass = true;
                        ChangeLastHasToUserSpecific(app);
                    }
                }
            }

            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var output = new object();

            output = new
            {
                message = message,
                pass = isPass
            };

            return oSerializer.Serialize(output);
        }

        private static void AddSignUpUserPhoneNumber(clsComparionCore app, string phoneNumber)
        {
            if (app.HasActiveProject() && (app.ActiveProject.isValidDBVersion || app.ActiveProject.isDBVersionCanBeUpdated) && phoneNumber != "")
            {
                var pm = app.ActiveProject.ProjectManager;
                if (pm.Attributes.GetAttributeByID(Attributes.ATTRIBUTE_USER_PHONE_ID) == null)
                {
                    pm.Attributes.AddAttribute(Attributes.ATTRIBUTE_USER_PHONE_ID, TeamTimeClass.ResString("lblUserPhone"), Attributes.AttributeTypes.atUser, Attributes.AttributeValueTypes.avtString, "", false, default(Guid), "lblUserPhone");
                    pm.Attributes.WriteAttributes(Attributes.AttributesStorageType.astStreamsDatabase, pm.StorageManager.ProjectLocation, pm.StorageManager.ProviderType, pm.StorageManager.ModelID);
                }

                var currentUser = pm.GetUserByEMail(app.ActiveUser.UserEMail);
                if (currentUser == null)
                {
                    currentUser = pm.AddUser(app.ActiveUser.UserEMail, true, app.ActiveUser.UserName);
                }

                pm.Attributes.SetAttributeValue(Attributes.ATTRIBUTE_USER_PHONE_ID, currentUser.UserID, Attributes.AttributeValueTypes.avtString, phoneNumber, Guid.Empty, Guid.Empty);
                pm.Attributes.WriteAttributeValues(Attributes.AttributesStorageType.astStreamsDatabase, pm.StorageManager.ProjectLocation, pm.StorageManager.ProviderType, pm.StorageManager.ModelID, currentUser.UserID);
            }
        }

        [WebMethod(EnableSession = true)]
        public static object GenerateLink(bool is_teamtime, int projID)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            App.ActiveProject = App.DBProjectByID(projID);
            var users = App.DBUsersByProjectID(App.ActiveProject.ID);
            var listofhashlinks = new List<string[]>();
            var project = App.ActiveProject;
            string baseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
            HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/";
            foreach (clsApplicationUser user in users)
            {
                if(user.UserEMail.Equals("admin", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                var arrayofstring = new string[2];
                arrayofstring[0] = user.UserEMail;
                
                if (!is_teamtime)
                {
                    arrayofstring[1] = baseUrl + GeckoClass.CreateLogonURL(user, project, false, "", "");
                }
                else
                {
                    arrayofstring[1] = baseUrl + GeckoClass.CreateLogonURL(user, project, true, "", "");
                }
                listofhashlinks.Add(arrayofstring);
            }
            return listofhashlinks;
        }

        [WebMethod(EnableSession = true)]
        public static object getCurrentProjectInfo()
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var has_project = false; var is_teamtime = false; var is_online = false; var project_name = "";
            var wkgname = "";
            long meetingID = -1;
            if (App.ActiveUser != null)
            {
                if (App.ActiveProject != null)
                {
                    has_project = true;
                    project_name = App.ActiveProject.ProjectName;
                    if (App.ActiveProject.isTeamTime)
                    {
                        is_teamtime = true;
                        is_online = App.ActiveProject.isOnline;
                    }
                    meetingID = App.ActiveProject.get_MeetingID();
                }
                wkgname = App.ActiveWorkgroup.Name;
              
            }

            var accessCode = App.ActiveProject == null ? "" : App.ActiveProject.get_Passcode(App.ActiveProject.isImpact);

            var output = new
            {
                has_project = has_project,
                is_teamtime = is_teamtime,
                project_id = App.ProjectID,
                is_online = is_online,
                project_name = project_name,
                access_code = accessCode,
                workgroup_name = wkgname,
                meetingID = meetingID
            };
            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            //return false;
            return oSerializer.Serialize(output);
        }

        [WebMethod(EnableSession = true)]
        public static string getGeneralLink(int tmode, int projectID, string signupmode, string otherparams, int combinedGroupID, int wkgRoleGroupId)
        {
            //check for changes in file: ~\CWSw\CoreWS_OperationContracts.vb, and method: GetEvaluationLink

            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            
            string sResult = "";
            if (!App.HasActiveProject())
                App.ProjectID = projectID;
            var tProject = App.ActiveProject;

            //getting real RoleGroup id
            clsRoleGroup roleGroup = App.ActiveWorkgroup.RoleGroups.FirstOrDefault(rg => rg.GroupType == (ecRoleGroupType)(wkgRoleGroupId + 9));
            wkgRoleGroupId = roleGroup == null ? 0 : roleGroup.ID;

            if (tProject != null && wkgRoleGroupId > 0)
            {
                
                
                var baseurl = context.Request.Url.Scheme + "://" + context.Request.Url.Authority + context.Request.ApplicationPath.TrimEnd('/') + "/";

                if (tmode == 3)
                {
                    sResult = baseurl + "?passcode=" + HttpUtility.UrlEncode(tProject.Passcode);
                }
                else
                {
                    if (otherparams != "")
                        otherparams = "req=" + otherparams;

                    sResult = CreateEvaluationSignupURL(tProject, tProject.Passcode, tmode == 1, signupmode, otherparams, baseurl, combinedGroupID, wkgRoleGroupId);
                }
            }

            return sResult;
        }

        public static string CreateEvaluationSignupURL(clsProject tProject, string sPasscode, bool fIsAnonymous, string sSignupMode, string sOtherParams, string sPagePath, int tGroupID, int tWkgRoleGroupID)
        {
            //check for changes in file: ~\CWSw\CoreWS_System.vb, and method: CreateEvaluationSignupURL

            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            
            string sURL = "";

            if (tProject == null)
                return sURL;

            sURL += string.Format("&{0}=1&{1}={2}&{3}={4}&{5}={6}", Options._PARAMS_SIGNUP[0], Options._PARAMS_ANONYMOUS_SIGNUP[0], (fIsAnonymous ? "1" : "0"), Options._PARAM_PASSCODE, HttpUtility.UrlEncode(sPasscode), Options._PARAMS_SIGNUP_MODE[0], sSignupMode);
            if (tGroupID >= 0)
                sURL += string.Format("&{0}={1}", Options._PARAM_ROLEGROUP, tGroupID);

            if (tWkgRoleGroupID >= 0)
                sURL += string.Format("&{0}={1}", Options._PARAM_WKG_ROLEGROUP, tWkgRoleGroupID);

            if (sOtherParams != "")
            {
                if (sURL != "")
                    sURL += "&";

                sURL += sOtherParams;
            }

            sURL = ECService.CryptService.EncodeURL(sURL, App.DatabaseID);

            if (App.Options.UseTinyURL)
            {
                int PID = tProject.ID;
                //if (tProject != null)
                //    PID = tProject.ID;

                sURL = string.Format("{0}?{2}={1}", sPagePath, App.CreateTinyURL(sURL, PID, -1), Options._PARAMS_TINYURL[0]);
            }
            else
            {
                sURL = string.Format("{0}?{2}={1}", sPagePath, sURL, Options._PARAMS_KEY[0]);
            }

            return sURL;
        }

        [WebMethod(EnableSession = true)]
        public static bool setShowMessage( bool message)
        {
            HttpContext.Current.Session["showmessage"] = message;
            return message;
        }

        [WebMethod(EnableSession = true)]
        public static bool setInviteMessage(bool message)
        {
            HttpContext.Current.Session["invitemessage"] = message;
            return message;
        }

        [WebMethod(EnableSession = true)]
        public static bool updateUser(string email, string name, string password, string passwordtochange = "")
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var fpass = false;
            var user = App.DBUserByEmail(email);
            if (user != null)
            {
                if(password == user.UserPassword)
                {
                    user.UserName = name;
                    user.UserPassword = password;
                    if (passwordtochange != "")
                    {
                        user.UserPassword = passwordtochange;
                    }
                    App.DBUserUpdate(user, false, "Create New User");
                    fpass = true;
                }
                App.ActiveUser = user;
            }
            return fpass;
        }

        [WebMethod(EnableSession = true)]
        public static object getDataonLoad()
        {
            //this must replace everything that needs to be loaded on browser load

            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var user = new clsApplicationUser();

            if (App.ActiveUser != null)
                user = App.ActiveUser;

            var output = new
            {
                ActiveUser = user,
                Options = new
                {
                    SessionID = App.Options.SessionID
                }
            };

            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return oSerializer.Serialize(output);
        }

        public static void LoadLanguange()
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];

            var Path = HttpContext.Current.Server.MapPath("~/Res/resx/English.aspx");
            var SampleUrl = System.IO.Path.GetFullPath(System.IO.Path.Combine(Path, "Res/resx/"));
            //var sss = Consts._FILE_DATA_RESX.ToString();
            var languagefolder = TeamTimeClass.LanguagesScanFolder(Path);
            TeamTimeClass.language = clsLanguageResource.LanguageByCode("Resource1", languagefolder);
            App.CurrentLanguage = TeamTimeClass.language;

        }

        public static void addUsertoGroup(string groupId)
        {
            if( groupId != null && groupId != "")
            {
                var app = (clsComparionCore)HttpContext.Current.Session["App"];
                var group = (ECCore.Groups.clsCombinedGroup)app.ActiveProject.ProjectManager.CombinedGroups.GetGroupByID(Convert.ToInt32(groupId));
                if (group != null)
                {
                    var ahpuser = app.ActiveProject.ProjectManager.GetUserByEMail(app.ActiveUser.UserEMail);
                    if (!group.ContainsUser(ahpuser))
                    {
                        group.UsersList.Add(ahpuser);
                        app.ActiveProject.SaveStructure("Update user group");
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public static string getOriginalLink()
        {
            HttpContext context = HttpContext.Current;
            return context.Session["AbsoluteUri"].ToString();
        }

        [WebMethod(EnableSession = true)]
        public static string setAllowIESession()
        {
            HttpContext context = HttpContext.Current;
            context.Session["AllowIE"] = true;
            return context.Session["ComparionResponsiveLink"].ToString();
        }

        [WebMethod(EnableSession = true)]
        public static void unsetAllowIESession()
        {
            HttpContext context = HttpContext.Current;
            context.Session["AllowIE"] = null;
        }

        [WebMethod(EnableSession = true)]
        public static string IsExistingUser(string email)
        {
            System.Threading.Thread.Sleep(1000);
            var responseText = "false";

            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    var app = (clsComparionCore)HttpContext.Current.Session["App"];
                    var emailUser = app.DBUserByEmail(email);
                    responseText = emailUser == null ? "false" : "true";
                }
                catch (Exception ex)
                {
                    responseText = $"Error: {ex.Message}";
                }
            }

            return responseText;
        }

        [WebMethod]
        public static string GetLastHash()
        {
            var lastHash = "";
            var context = HttpContext.Current;

            if (context.Request.Cookies["LastHash"] != null)
            {
                lastHash = context.Request.Cookies["LastHash"].Value;
            }

            return lastHash;
        }

        private static void ChangeLastHasToUserSpecific(clsComparionCore App)
        {
            string userSpecificHash = GeckoClass.CreateLogonURL(App.ActiveUser, App.ActiveProject, false, "", "");
            userSpecificHash = userSpecificHash.Replace("?hash=", "");

            HttpCookie hashCookie = new HttpCookie("LastHash", userSpecificHash)
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(1)
            };

            HttpContext.Current.Response.Cookies.Add(hashCookie);
        }

        private static bool CheckUserPasswordStatusAndSendEmail(clsComparionCore app, clsApplicationUser user, int maxPasswordAttempts)
        {
            var result = false;

            if (user != null && user.PasswordStatus == maxPasswordAttempts)
            {
                var errorString = "";
                app.DBSaveLog(dbActionType.actLock, dbObjectType.einfUser, user.UserID, "Lock user due to max login attempts" + (Consts._DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK ? " in a period" : ""), ExpertChoice.Service.Common.GetClientIP(HttpContext.Current.Request));
                
                var subject = TeamTimeClass.ParseAllTemplates(TeamTimeClass.ResString("subjLockedPswAttempts", false, false), user, null);
                var body = TeamTimeClass.ParseAllTemplates(TeamTimeClass.ResString("bodyLockedPswAttempts", false, false), user, null);

                result = ECService.Common.SendMail(WebOptions.SystemEmail, user.UserEMail, subject, body, ref errorString, "", false, WebOptions.SMTPSSL());
                app.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, user.UserID, "Email about locking account due to wrong psw", errorString);
            }

            return result;
        }

        [WebMethod]
        public static object ResetPassword(string email)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var isSuccess = false;
            var message = "";

            if (string.IsNullOrEmpty(email))
            {
                message = string.Format(TeamTimeClass.ResString("lblValidatorField"), TeamTimeClass.ResString("lblEmail"));
            }
            else
            {
                clsApplicationUser resetUser = App.DBUserByEmail(email);

                if (resetUser == null)
                {
                    message = string.Format(TeamTimeClass.ResString("errUserEmailNotExists"), email);
                }
                else
                {
                    if (resetUser.CannotBeDeleted)
                    {
                        message = string.Format(TeamTimeClass.ResString("errResetPswNoAllowed"), email);
                    }
                    else
                    {
                        isSuccess = ECService.Common.SendMail(WebOptions.SystemEmail, email, TeamTimeClass.ParseAllTemplates(TeamTimeClass.ResString("subjReminder", false, false), resetUser, null), TeamTimeClass.ParseAllTemplates(App.GetPswReminderBodyText(TeamTimeClass.ResString("bodyReminder", false, false), false, false), resetUser, null), ref message, "", false, WebOptions.SMTPSSL());
                        App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, resetUser.UserID, "Request for forgotten password", message);
                        message = string.Format(TeamTimeClass.ResString(isSuccess ? "msgReminderOK" : "msgReminderError"), email, WebOptions.SystemEmail);
                    }
                }
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var returnObject = new
            {
                isSuccess = isSuccess,
                message = message
            };

            return serializer.Serialize(returnObject);
        }
    }
}