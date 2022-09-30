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
using AnytimeComparion.Pages.external_classes;
using System.Web.Script.Serialization;

namespace AnytimeComparion
{
    public partial class SiteMaster : MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;
        public clsComparionCore App;
        public ECWeb.clsComparionCorePage corepage;
        public int _roles;

        public string GoogleUA { get
            {
                return ExpertChoice.Service.Options.WebConfigOption(ExpertChoice.Web.WebOptions._OPT_GOOGLE_UID, ExpertChoice.Web.WebOptions._DEF_GOOGLE_UA, false);
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (ECWeb.WebOptions.ForceSSL(Request) && !Request.IsSecureConnection && Request.Url != null && Request.Url.Host != "localhost" && !string.IsNullOrEmpty(Request.Url.AbsoluteUri))
            {
                Response.Redirect(Request.Url.AbsoluteUri.Replace("http:", "https:"), true);
            }

            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
            restoreApplicationStartTime();
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var App = (clsComparionCore)Session["App"];

            RedirectWhenOldSessionExists(App);


            Session[Constants.Sess_FromComparion] = Session[Constants.Sess_FromComparion] == null ? false : (bool)Session[Constants.Sess_FromComparion];

            if (Session[Constants.SessionIsPipeViewOnly] == null)
            {
                Session[Constants.SessionIsPipeViewOnly] = false;
            }

            if (Session[Constants.SessionViewOnlyUserId] == null)
            {
                Session[Constants.SessionViewOnlyUserId] = -1;
            }

            if (Session[Constants.SessionIsInterResultStepFound] == null)
            {
                Session[Constants.SessionIsInterResultStepFound] = true;
            }

            Session[Constants.Sess_SignUp_ProjName] = "";
            var context = HttpContext.Current;
            checkQueryString();
            Session[Constants.Sess_RemoveAnonymCookie] = false;
            var steps = Request.QueryString["steps"];

            var TeamTimeStatus = Request.QueryString["teamtime"];
            Session["thispage"] = Page;
            //for sort cookie to save
            if (HttpContext.Current.Request.Cookies["ProjectListSort"] == null)
            {
                var sort_datas = new object[2];
                sort_datas[0] = 10;
                sort_datas[1] = true;
                string myObjectJson = new JavaScriptSerializer().Serialize(sort_datas);
                context.Response.Cookies["ProjectListSort"].Expires = DateTime.Now.AddDays(1);
                context.Response.Cookies["ProjectListSort"].Value = myObjectJson;
            }

            
            if (App.ActiveProject != null)
            {
                ECTypes.clsUser curuser = App.ActiveProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEMail);
                if(curuser == null)
                {
                    curuser = App.ActiveProject.ProjectManager.AddUser(App.ActiveUser.UserEMail, true, App.ActiveUser.UserName);
                    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure();
                }
                UserNameLabel_mobile.Text = curuser.UserName;
                Project_Name.InnerText = App.ActiveProject.ProjectName;
            }
            else 
            {
                if (App.ActiveUser != null)
                {
                    UserWorkSpaceUpdateLastProjectOnly();
                }

            }

            //if (App.ActiveUser != null)
            //{
            //    var Path = Server.MapPath("~/Res/resx/English.aspx");
            //    var SampleUrl = System.IO.Path.GetFullPath(System.IO.Path.Combine(Path, "Res/resx/"));
            //    var sdsdsd = TeamTimeClass.LanguagesScanFolder(Path);
            //    TeamTimeClass.language = clsLanguageResource.LanguageByCode("Resource1", sdsdsd);
            //    App.CurrentLanguage = TeamTimeClass.language;
            //}

            if (steps == "false")
                context.Session["steps"] = null;

            if(TeamTimeStatus == "stop")
            {
                var project = App.ActiveProject;
                App.TeamTimeEndSession(ref project, false);
                Response.Redirect("~");
            }
            //Do shortcuts and also read query strings

            if (Request.QueryString["hash"] != null && !Request.Path.Contains("Password.aspx"))
            {
                
                Session["UserSpecificHashErrorMessage"] = null;
                Session["LoggedInViaHash"] = true;
                Session[Constants.Sess_FromComparion] = Request.QueryString["from"] != null && Request.QueryString["from"] == "comparion";


                //check if mobile or not

                //case 13198: not needed since we will show warning messages on pages that are slow to use chrome and safari

                //if (!IsMobileBrowser() && Request.Url.AbsoluteUri.Contains("//r."))
                //    {
                //        if ( (Request.QueryString["redirect"] != null && Request.QueryString["redirect"] != "no") || Request.QueryString["redirect"] == null)
                //        {
                //            Response.Redirect(Request.Url.AbsoluteUri.Replace("//r.", "//"));
                //        }
                //    }

                //App.Logout(); //logout for refresh


                Session[Constants.Sess_LoginMethod] = 1;

                var evaluation_session = new string[] {"TTOnly", "Pipe"};
                var otherparams = new string[] { "req" };
                var showmessage = new string[] { "msg" };
                var messagecontent = new string[] { "msgcnt" };
                var rgid = new string[] { ECWeb.Options._PARAM_ROLEGROUP };
                string sInputs = Request.QueryString["hash"].Trim();
                if (sInputs.Contains(" "))
                {
                    var splitResult = sInputs.Split(' ');
                    sInputs = splitResult[0];
                }

                string sResults = App.DecodeTinyURL(sInputs);
                sResults = CryptService.DecodeURL(sResults, App.DatabaseID);
                NameValueCollection sParamss = HttpUtility.ParseQueryString(sResults);
                var email = Common.ParamByName(sParamss, ECWeb.Options._PARAMS_EMAIL);
                var pass = Common.ParamByName(sParamss, ECWeb.Options._PARAMS_PASSWORD);
                var pscode = Common.ParamByName(sParamss, ECWeb.Options._PARAMS_PASSCODE);
                var allowsignup = Common.ParamByName(sParamss, ECWeb.Options._PARAMS_SIGNUP);
                var signupmode = Common.ParamByName(sParamss, ECWeb.Options._PARAMS_SIGNUP_MODE);
                var req = Common.ParamByName(sParamss, otherparams);
                var anonymous = Common.ParamByName(sParamss, ECWeb.Options._PARAMS_ANONYMOUS_SIGNUP);
                var evaluation_what = Common.ParamByName(sParamss, evaluation_session);
                var msg= Common.ParamByName(sParamss, showmessage);
                var msgcnt = Common.ParamByName(sParamss, messagecontent);
                var rg = Common.ParamByName(sParamss, rgid);
                var step = Common.GetParam(sParamss, ECWeb.Options._PARAM_STEP);

                SetParameterStepInSession(App, step, email);
                RedirectRestrictedUser(App, email, pscode, Common.ParamByName(sParamss, new[] { ECWeb.Options._PARAM_WKG_ROLEGROUP }));

                Session[Constants.Sess_RoleGroup] = rg;
                Session["passcode"] = pscode;
                var passcode = pscode;

                var check_login = App.Logon(email, pass, ref pscode, false, true, false);

                if (string.IsNullOrEmpty(sResults))
                {
                    Response.Redirect("~/?pageError=invalidLink&debug="+ sInputs);
                }

                var project = App.DBProjectByPasscode(passcode);

                //Do shortcuts and also read query strings

                if (!readQueryStrings(Request.QueryString.AllKeys))
                {
                    Response.Redirect("?hash="+Request.QueryString["hash"]);
                }

                if (project != null)
                {
                    LoadCustomSignUpPageContent(project);

                    if (App.ActiveProject != null)
                    {
                        AnytimeClass.CheckProjectIsAccessible(App.ActiveProject, email);
                    }
                    else
                    {
                        AnytimeClass.CheckProjectIsAccessible(project, email);
                    }

                    var anyTimeUrl = "~/pages/Anytime/Anytime.aspx";

                    if (allowsignup != "")
                    {
                        //For Anonymous Link
                        if (anonymous != "" && anonymous != "0")
                        {
                            //RedirectAnonAndSignupLinks(App, passcode);
                            Session["NewUser"] = true;
                            forceSignUponAnonymous(project);
                            context.Session["User"] = App.ActiveUser;
                            context.Session["Project"] = App.ActiveProject;
                            InitName(App.ActiveUser);
                            //if(App.ActiveUser == null)
                            //{
                            //    Response.Redirect("~/Pages/PageError.aspx");
                            //}
                            try
                            {
                                Response.Cookies["anonymous"].Expires = DateTime.Now.AddDays(1);
                                Response.Cookies["anonymous"].Value = App.ActiveUser.UserEMail;
                            }
                            catch
                            {
                                Response.Redirect("~/?pageError=inviteNoAccess&passCode=" + passcode);
                            }
                            Response.Redirect(anyTimeUrl);


                        }
                        //For Passcode Link
                        if (App.ActiveUser != null && anonymous != "0")
                        {
                            _Default.StartAnytime(project.ID);
                            Response.Redirect(anyTimeUrl);
                        }
                        else
                        {
                            //For Sign Up Link
                            if (App.ActiveUser != null)
                            {
                                Session["NewUser"] = true;
                                //RedirectAnonAndSignupLinks(App, passcode);

                                //if user is logged in
                                storepageinfo();
                                var AppUser = App.DBUserByEmail(App.ActiveUser.UserEMail);
                                var Authres = App.Logon(AppUser.UserEMail, AppUser.UserPassword, ref passcode, false, true, false);
                                context.Session["User"] = App.ActiveUser;
                                context.Session["Project"] = App.ActiveProject;
                                _Default.StartAnytime(project.ID);
                                Response.Redirect(anyTimeUrl);
                            }
                            else
                            {
                                //if user is not yet logged in
                                Session["NewUser"] = true;
                                //RedirectAnonAndSignupLinks(App, passcode);
                                if (Request.Cookies["anonymous"] != null && !string.IsNullOrEmpty(Request.Cookies["anonymous"].Value) && !(bool)Session[Constants.Sess_RemoveAnonymCookie])
                                {
                                    var useremail = (string)Request.Cookies["anonymous"].Value;
                                    var AppUser = App.DBUserByEmail(useremail);
                                    var Authres = App.Logon(AppUser.UserEMail, AppUser.UserPassword, ref passcode, false, true, false);
                                    context.Session["User"] = App.ActiveUser;
                                    context.Session["Project"] = App.ActiveProject;
                                    _Default.StartAnytime(project.ID);
                                    Response.Redirect(anyTimeUrl);
                                }
                                Response.Cookies["anonymous"].Expires = DateTime.Now.AddDays(-1);
                                Response.Cookies["anonymous"].Value = null;
                                Session[Constants.Sess_SignUp] = true;
                                Session[Constants.Sess_SignUp_ProjName] = project.ProjectName;
                                Session[Constants.Sess_SignUp_Passcode] = passcode;
                                Session[Constants.Sess_SignUpMode] = signupmode;
                                Session[Constants.Sess_Requirements] = req;
                                Session[Constants.Sess_ShowMessage] = msg;
                                Session[Constants.Sess_InviteMessage] = msgcnt;
                            }
                        }
                    }
                    else
                    {
                        //for c2eval hash linkst AT/TT
                        try
                        {
                            //for c2eval hash links
                            if (sParamss.Count < 5)
                            {
                                var action = Request.QueryString["action"];
                                var authres = App.Logon(email, pass, ref pscode, false, true, false);
                                App.ActiveProject = project;
                                context.Session["User"] = App.ActiveUser;
                                context.Session["Project"] = App.ActiveProject;
                                context.Session["App"] = App;
                                InitName(App.ActiveUser);
                                if (action == "eval_teamtime")
                                {
                                    //_Default.StartMeeting(true);
                                    //Response.Redirect(GetComparionTeamTimeUrl());
                                }
                                else if (action == "eval_anytime")
                                {
                                    HttpContext.Current.Session["Sess_WrtNode"] = (clsNode)App.ActiveProject.HierarchyObjectives.GetLevelNodes(0)[0];
                                    Response.Redirect(anyTimeUrl);
                                }
                            }
                        }
                        catch
                        {

                        }

                        //For anytime links
                        if (pscode != "")
                        {
                            var authres = App.Logon(email, pass, ref pscode, false, true, false);
                            var message = string.Empty;

                            switch (authres)
                            {
                                case ecAuthenticateError.aeProjectLocked:
                                    message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project);
                                    message = message.Replace("''", "'" + project.ProjectName + "'");
                                    Session["UserSpecificHashErrorMessage"] = message;
                                    Response.Redirect("~/?pageError=inviteNoAccess&passCode=" + passcode);
                                    break;
                                case ecAuthenticateError.aeUserWorkgroupLocked:
                                case ecAuthenticateError.aeWorkspaceLocked:
                                case ecAuthenticateError.aeUserLockedByWrongPsw:
                                    message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project);
                                    message = message.Replace("''", "'" + project.ProjectName + "'");
                                    Session["UserSpecificHashErrorMessage"] = message;
                                    break;
                                default:
                                    break;
                            }
                            storepageinfo();
                            if (App.ActiveUser != null)
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
                                else
                                {
                                    context.Session["User"] = App.ActiveUser;
                                    context.Session["Project"] = App.ActiveProject;
                                }

                                //anytime
                                if (sParamss.Count < 4)
                                {
                                    InitName(App.ActiveUser);
                                    _Default.StartAnytime(project.ID);
                                    Response.Redirect(anyTimeUrl);
                                }

                                //anytime invitation email
                                if (evaluation_what == "yes")
                                {
                                    InitName(App.ActiveUser);
                                    _Default.StartAnytime(project.ID);
                                    Response.Redirect(anyTimeUrl);
                                }

                            }

                            //for Teamtime links
                            if (evaluation_what == "1")
                            {

                                if (App.ActiveUser != null)
                                {
                                    //var userd = App.ActiveUserWorkgroup.RoleGroupID;
                                    //var rolename = (clsRoleGroup)App.DBRoleGroupByID(userd);
                                    //if (rolename.Name.Contains("Workgroup Member") == false)
                                    //{
                                    //    if (App.ActiveProject.isTeamTime != true && App.ActiveProject.MeetingOwner != App.ActiveUser)
                                    //    {
                                    //        Authentication.ApplyChanges(true);
                                    //        Session["TTstart"] = true;
                                    //        int MeetingOwner;
                                    //        if (App.ActiveProject.MeetingOwner != null)
                                    //        {
                                    //            MeetingOwner = App.ActiveProject.MeetingOwner.UserID;
                                    //        }
                                    //        else
                                    //        {
                                    //            MeetingOwner = App.ActiveProject.OwnerID;
                                    //        }
                                    //        var workspace = App.DBWorkspaceByUserIDProjectID(MeetingOwner, App.ProjectID);
                                    //        workspace.set_ProjectStep(App.ActiveProject.isImpact, 1);
                                    //        App.DBWorkspaceUpdate(ref workspace, false, null);
                                    //    }
                                    //}
                                    //storepageinfo();

                                    if (!_Default.isPM())
                                    {
                                        //Response.Redirect(GetComparionTeamTimeUrl());
                                    }
                                    else
                                    {
                                        Session["isMember"] = true;
                                        if (App.ActiveProject.isTeamTime)
                                        {
                                            //Response.Redirect(GetComparionTeamTimeUrl());
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                else
                {
                    var authres = App.Logon(email, pass, ref pscode, false, true, false);
                    switch (authres)
                    {
                        case ecAuthenticateError.aeProjectLocked:
                            Response.Redirect("~/?pageError=inviteNoAccess&passCode=" + passcode);
                            break;
                        case ecAuthenticateError.aeUserWorkgroupLocked:
                        case ecAuthenticateError.aeWorkspaceLocked:
                        case ecAuthenticateError.aeWrongPasscode:
                        case ecAuthenticateError.aeNoUserFound:
                            var message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project);
                            Session["UserSpecificHashErrorMessage"] = message;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (!readQueryStrings(Request.QueryString.AllKeys))
                {
                    Response.Redirect("~");
                }
            }
            
            //if remember me is checked
            if (Request.Cookies["rmberme"] != null && Request.Cookies["fullname"] != null)
            {

                Session["UserType"] = Request.Cookies["usernam"].Value.ToString();
                Session["users"] = Request.Cookies["fullname"].Value.ToString();
            }


            InitName(App.ActiveUser);

            //logged in if remember me
            if (Request.Cookies["rmberme"] != null && Request.Cookies["usernam"] != null && Request.Cookies["passwor"] != null && (Session["remove_anonymous"] !=null && (bool)Session["remove_anonymous"]))
            {
                var email = Request.Cookies["usernam"].Value.ToString();
                var password = Request.Cookies["passwor"].Value.ToString();
                var sPasscode = "";
                if (App.ActiveUser == null)
                {
                    var AuthRes = App.Logon(email, password, ref sPasscode, false, true, false);
                    //if (AuthRes == ecAuthenticateError.aeNoWorkgroupSelected) // deprecated
                    //{
                    //    int LastVisitedWGID = -1;
                    //    LastVisitedWGID = App.ActiveUser.Session.WorkgroupID;
                    //    clsWorkgroup WorkGroup = clsWorkgroup.WorkgroupByID(LastVisitedWGID, App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups));
                    //    App.ActiveWorkgroup = WorkGroup;
                    //    App.ActiveUserWorkgroup = null;
                    //    App.Workspaces = null;
                    //    App.ActiveProject = null;
                    //    AuthRes = ecAuthenticateError.aeNoErrors;

                    //}
                    Response.Redirect("~/pages/my-projects.aspx");
                }

                context.Session["User"] = App.ActiveUser;
                context.Session["App"] = App;

            }
        }

        private void LoadCustomSignUpPageContent(clsProject project)
        {
            if (project == null) return;

            customTitle.InnerHtml = $"Sign Up Below<br/> {project.ProjectName}";

            var signUpContent = "";

            var signUpTitle = string.Format(TeamTimeClass.ResString("msgSignUpEvaluate"), project.ProjectName);
            if (project.ProjectManager.Parameters.InvitationCustomTitle != "")
            {
                signUpTitle = StringFuncs.SafeFormString(TeamTimeClass.ParseAllTemplates(project.ProjectManager.Parameters.InvitationCustomTitle, null, project));
            }

            if (project.ProjectManager.Parameters.InvitationCustomText != "")
            {
                var htmlString = project.ProjectManager.Parameters.InvitationCustomText.Trim();
                if (htmlString != "")
                {
                    if (InfodocService.isMHT(htmlString))
                    {
                        htmlString = InfodocService.Infodoc_Unpack(project.ID, project.ProjectManager.ActiveHierarchy, Consts.reObjectType.ExtraInfo, ECWeb.Options._PARAM_INVITATION_CUSTOM, htmlString, true, true, -1);
                    }

                    signUpContent = StringFuncs.HTML2TextWithSafeTags(htmlString, (StringFuncs._DEF_SAFE_TAGS + "IMG;TABLE;TR;TH;TD;")).Trim();

                    if (signUpContent != "")
                    {
                        signUpContent = $"{TeamTimeClass.ParseAllTemplates(signUpContent, null, project)}";
                    }
                }
            }

            customTitle.InnerHtml = project.ProjectManager.Parameters.InvitationCustomTitle != "" ? signUpTitle : $"Sign up or log in below.<br/> {project.ProjectName}";
            customContent.InnerHtml = signUpContent;
        }

        private void RedirectAnonAndSignupLinks(clsComparionCore App, string passcode)
        {
            //AR Start: Code for showing RedirecToComparion when a model has insight survey page and access by anonymous or signup links
            var activeProjectInsight = App.ActiveProject ?? App.DBProjectByPasscode(passcode);
            //if (activeProjectInsight.isValidDBVersion && activeProjectInsight.ProjectManager.StorageManager.Reader.IsWelcomeSurveyAvailable(activeProjectInsight.isImpact)
            //    && (activeProjectInsight.ProjectManager.PipeParameters.ShowWelcomeSurvey || activeProjectInsight.ProjectManager.PipeParameters.ShowThankYouSurvey))
            bool WelComeInsightSurvey = activeProjectInsight.ProjectManager.StorageManager.Reader.IsWelcomeSurveyAvailable(activeProjectInsight.isImpact)
               && activeProjectInsight.ProjectManager.PipeParameters.ShowWelcomeSurvey;

            bool ThankYouInsightSurvey = activeProjectInsight.ProjectManager.StorageManager.Reader.IsThankYouSurveyAvailable(activeProjectInsight.isImpact)
              && activeProjectInsight.ProjectManager.PipeParameters.ShowThankYouSurvey;

            if (activeProjectInsight.isValidDBVersion && (WelComeInsightSurvey || ThankYouInsightSurvey))
            {
                Response.Redirect("~/pages/RedirectToComparion.aspx");
            }
            //AR End
        }

        public void InitName(clsApplicationUser user)
        {
            //init user in session
            try
            {
                Session["users"] = (user == null ? "" : user.UserName);
                string users = (string)Session["users"];
                if (users != null && users != "")
                {
                    //UserNameLabel.Text = users;
                }
                else if (user != null)
                {
                    //UserNameLabel.Text = user.UserName;

                }
                else
                {
                    //for Mike A

                    string UserType = (string)Session["UserType"];

                    if (UserType != null)
                    {
                        if (UserType == "pm")
                        {
                            //UserNameLabel.Text = "Project Manager";
                        }
                        if (UserType == "evaluator")
                        {
                            //UserNameLabel.Text = "Evaluator";
                        }
                        if (UserType == "participant")
                        {
                            //UserNameLabel.Text = "Participant";
                        }
                    }
                    else
                    {
                        //UserNameLabel.Text = "No Logged in User";
                    }
                }

                if (App != null && App.ActiveUser != null)
                {
                    if (_Default.isPM() == false)
                    {
                        _roles = 1;
                    }
                    else
                    {
                        _roles = 0;
                    }
                }
                //System.Diagnostics.Trace.Write(UserNameLabel.Text);
            }
            catch
            {

            }
            
        }

        public static bool storepageinfo()
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            try
            {
                App.Database.ExecuteSQL("UPDATE users SET isOnline=1 WHERE ID='" + App.ActiveUser.UserID + "'");
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public static bool UserWorkSpaceUpdateLastProjectOnly()
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            try
            {
                App.Database.ExecuteSQL("UPDATE userworkgroups SET LastProjectID=-1 WHERE UserID='" + App.ActiveUser.UserID + "' and WorkGroupID='" + App.ActiveUserWorkgroup.WorkgroupID + "'");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static ecAuthenticateError forceSignUponAnonymous(clsProject project, string name = "")
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            var random_char = new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            var email = "Anonym-" + project.Passcode + "_" + random_char;
            if(name == "")
                name = email;
            var nully = "";
            var Authres = ecAuthenticateError.aeNoErrors;
            if (HttpContext.Current.Request.Cookies["anonymous"] != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies["anonymous"].Value))
            {
                email = HttpContext.Current.Request.Cookies["anonymous"].Value;
                var passcode = project.Passcode;
                Authres = App.Logon(email, "", ref passcode, false, true, false);
                _Default.StartAnytime(project.ID);
            }
            else
            {
                var user = App.UserWithSignup(email, name, "", "Sign-up via URL", ref nully, false);
                if (user == null)
                {
                    forceSignUponAnonymous(project);
                }
                else
                {
                    var passcode = project.Passcode;
                    Authres = App.Logon(email, "", ref passcode, false, true, false);
                    _Default.StartAnytime(project.ID);
                    Global.ServerError += Authres + " " + email + " " + passcode;
                }
            }
            return Authres;
        }

        private bool readQueryStrings(string[] QueryStrings)
        {
            var returnVal = true;
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            foreach (string key in QueryStrings)
            {
                if(key == "clear" | key == "c")
                {
                    //clear anonymous users
                    switch(Request.QueryString[key])
                    {
                        case "1":
                            Response.Cookies["anonymous"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["anonymous"].Value = "";
                            Session[Constants.Sess_RemoveAnonymCookie] = true;
                            if (App.ActiveUser != null)
                            {
                                returnVal = false;
                                _Default.logout();
                            }

                            break; 
                        case "sort":
                            Response.Cookies["ProjectListSort"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["ProjectListSort"].Value = null;
                            break;
                        case "rememberme":
                            Response.Cookies["rmberme"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["rmberme"].Value = null;
                            break;
                        case "equalMessage":
                            Response.Cookies["equalMessage"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["equalMessage"].Value = null;
                            break;
                        case "all":
                            string[] myCookies = Request.Cookies.AllKeys;
                            foreach (string cookie in myCookies)
                            {
                                Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
                            }
                            if (App.ActiveUser != null)
                            {
                                returnVal = false;
                                _Default.logout();
                            }
                            break;
                    }
                }
            }
            return returnVal;

        }

        private void checkQueryString()
        {
            Session["debug"] = Request.QueryString["debug"];
            Session["hash"] = Request.QueryString["hash"];

            if (Session["hash"] != null && !string.IsNullOrEmpty((string)Session["hash"]))
            {
                var hashLink = (string)Session["hash"];
                HttpCookie hashCookie = new HttpCookie("LastHash", hashLink)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(1)
                };

                Response.Cookies.Add(hashCookie);
            }

            if (Session["hashinCR"] == null)
                Session["hashinCR"] = Request.QueryString["hash"];
            Session["LoginviaMeetingID"] = Request.QueryString["LoginviaMeetingID"];

            //case 11265 - force an error
            Session[Constants.Sess_ForceError] = true;
            if (!string.IsNullOrEmpty(Request.QueryString["forceError"]))
                Session[Constants.Sess_ForceError] = Request.QueryString["forceError"];
        }

        private void restoreApplicationStartTime()
        {
            if (Request.Cookies["StartDate"] != null)
            {
                var startDate = (DateTime)Application["StartDate"];
                var cookieStartDate = Request.Cookies["StartDate"].Value;
                if (cookieStartDate != startDate.ToString())
                {
                    Response.Cookies["loadedScreens"].Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies["StartDate"].Value = startDate.ToString();
                    Response.Cookies["StartDate"].Expires = DateTime.Now.AddDays(30);
                }
            }
            if (Request.Cookies["StartDate"] == null)
            {
                Response.Cookies["loadedScreens"].Expires = DateTime.Now.AddDays(-1);
                var startDate = (DateTime)Application["StartDate"];
                Response.Cookies["StartDate"].Value = startDate.ToString();
                Response.Cookies["StartDate"].Expires = DateTime.Now.AddDays(30);
            }
        }

        private void SetParameterStepInSession(clsComparionCore app, string paramStep, string email)
        {
            var stepString = Request.QueryString["step"] == null ? paramStep : Request.QueryString["step"];

            if (!string.IsNullOrEmpty(stepString))
            {
                int step;
                if (int.TryParse(stepString, out step))
                {
                    Session[Constants.SessionParamStep] = step;
                }
            }

            var mode = Request.QueryString["mode"] == null ? "" : Request.QueryString["mode"].Trim().ToLower();
            var nodeGuid = Request.QueryString["node"] == null ? "" : Request.QueryString["node"].Trim().ToLower();
            var mtType = Request.QueryString["mt_type"] == null ? "" : Request.QueryString["mt_type"].Trim().ToLower();
            var readOnly = Request.QueryString["readonly"] == null ? "" : Request.QueryString["readonly"].Trim().ToLower();
            var id = Request.QueryString["id"] == null ? "" : Request.QueryString["id"].Trim().ToLower();

            if (string.IsNullOrEmpty(nodeGuid))
            {
                nodeGuid = Request.QueryString["node_id"] == null ? "" : Request.QueryString["node_id"].Trim().ToLower();
            }

            if ((mode == "searchresults" || mode == "getstep") && nodeGuid != "")
            {
                Session[Constants.SessionNonRMode] = mode;
                Session[Constants.SessionNonRNode] = nodeGuid;

                var mtTypeValue = -1;
                int.TryParse(mtType, out mtTypeValue);
                Session[Constants.SessionNonRMtType] = mtTypeValue;
            }

            if (readOnly == "true" || readOnly == "yes" || readOnly == "1")
            {
                Session[Constants.SessionIsPipeViewOnly] = true;
            }

            clsApplicationUser user = null;
            int userId;
            if (int.TryParse(id, out userId))
            {
                user = app.DBUserByID(userId);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                user = app.DBUserByEmail(email);
            }

            if (user != null)
            {
                Session[Constants.SessionViewOnlyUserId] = user.UserID;
            }
        }

        private void CheckDefaultBrowser()
        {
            bool isAllowed = false;
            bool isLocalHost = Request.Url.AbsoluteUri.Contains("//localhost");

            //if localhost, if mobile, and if hash link with redirect=no (which means do not redirect to comparion)
            if (isLocalHost || IsMobileBrowser())
            {
                HttpBrowserCapabilities browser = Request.Browser;
                string userAgent = Request.ServerVariables["HTTP_USER_AGENT"];

                switch (browser.Browser)
                {
                    case "Chrome":
                        if (!(userAgent.Contains("Edge") || userAgent.Contains("OPR")))
                        {
                            isAllowed = true;
                        }
                        break;
                    case "Safari":
                        if (!(userAgent.Contains("FxiOS") || userAgent.Contains("OPiOS")))
                        {
                            isAllowed = true;
                        }
                        break;
                }
            }

            bool is_hash_link = (Request.QueryString["redirect"] != null && Request.QueryString["redirect"].Equals("no", StringComparison.CurrentCultureIgnoreCase)) || (Request.QueryString["hash"] != null) || (((Request.Url.AbsoluteUri.Contains("//r.") || Request.Url.AbsoluteUri.Contains("//r-")) && Request.QueryString["hash"] != null &&
                   Request.QueryString["from"] != null && Request.QueryString["from"] == "comparion"));

            if (isLocalHost || is_hash_link)
            {
                if(is_hash_link)
                {
                    Session["LoggedInViaHash"] = true;
                }
                isAllowed = true;
            }



            // if you need to test for IE/Edge/Firefox/Opera then uncomment the next line
            //isAllowed = true;

            if (!isAllowed)
            {
                //when hash link is generated from comparion
                //if (Request.Url.AbsoluteUri.Contains("//r.") && Request.QueryString["hash"] != null &&
                //    Request.QueryString["from"] != null && Request.QueryString["from"] == "comparion")
                //{
                //    Session["ComparionResponsiveLink"] = Request.Url.AbsoluteUri.Replace("&from=comparion", "");
                //}
                //else
                //{
                //    Session.Remove("ComparionResponsiveLink");
                //}
                //Session["ComparionResponsiveLink"] = Request.Url.AbsoluteUri.Replace("&from=comparion", "");
                //Response.Redirect("~/pages/DefaultBrowser.aspx");
                //Response.Redirect(Server.MapPath("~/DefaultBrowser.aspx"));
                if (!HttpContext.Current.Request.Path.EndsWith("DefaultBrowser.aspx",
                    StringComparison.InvariantCultureIgnoreCase))
                    Response.Redirect("~/pages/DefaultBrowser.aspx");
            }
            else
            {
                //LabelBrowser.Text = GetBrowserInfo();
            }

           // Session["ComparionResponsiveLink"] = "http://localhost:9793/Pages/DefaultBrowser.aspx";
        }

        private bool IsMobileBrowser()
        {
            //HttpBrowserCapabilities browser = Request.Browser;
            string userAgent = Request.ServerVariables["HTTP_USER_AGENT"];

            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone")
                || userAgent.Contains("iPad"))
            {
                return true;
            }

            return false;
        }

        private string GetBrowserInfo()
        {
            HttpBrowserCapabilities browser = Request.Browser;
            string s = "<br /><br />&nbsp;&nbsp;Type = " + browser.Type + ";"
                + "&nbsp;Name = " + browser.Browser + ";"
                + "&nbsp;Version = " + browser.Version + ";"
                + "&nbsp;UserAgent = " + Request.ServerVariables["HTTP_USER_AGENT"];

            return s;
        }

        private void RedirectRestrictedUser(clsComparionCore app, string email, string passCode, string userRole)
        {
            if (!IsUserRestricted(app, email, passCode, userRole) || (bool)Session[Constants.SessionIsPipeViewOnly]) return;


            var context = HttpContext.Current;
            var redirectUrl = AnytimeClass.GetComparionHashLink();
            context.Session.Clear();
            context.Session.Abandon();
            context.Response.Cookies["rmberme"].Expires = DateTime.Now.AddDays(-1);
            context.Response.Cookies["fullname"].Expires = DateTime.Now.AddDays(-1);

            context.Response.Redirect(redirectUrl);
        }

        private bool IsUserRestricted(clsComparionCore app, string email, string passCode, string userRole)
        {
            //AR: all Evaluators hash (except Viewer) can access responsive Anytime
            clsRoleGroup roleGroup = null;

            if (string.IsNullOrEmpty(email))
            {
                var workGroupRoleId = -1;
                int.TryParse(userRole, out workGroupRoleId);
                roleGroup = app.DBRoleGroupByID(workGroupRoleId, false);
            }
            else
            {
                var sqlText = @"SELECT DISTINCT G.* FROM RoleGroups G LEFT JOIN Projects P ON P.WorkgroupID = G.WorkgroupID 
                            LEFT JOIN Workspace WS ON WS.GroupID = G.ID LEFT JOIN Users U ON U.ID = WS.UserID 
                            WHERE (P.Passcode = ? OR P.Passcode2 = ?) AND (LOWER(U.Email) = ?)
                            ORDER BY G.Created DESC";

                var sqlParams = new List<object> { passCode, passCode, email };
                var groupList = app.Database.SelectBySQL(sqlText, sqlParams);

                if (groupList != null && groupList.Count > 0)
                {
                    roleGroup = app.DBParse_RoleGroup(groupList.First());
                }
            }

            return (roleGroup == null || roleGroup.GroupType == ecRoleGroupType.gtViewer);
        }

        private string GetComparionTeamTimeUrl()
        {
            var context = HttpContext.Current;
            var oldValue = context.Session["NewUser"];
            context.Session["NewUser"] = false;

            var url = AnytimeClass.GetComparionHashLink();
            context.Session["NewUser"] = oldValue;

            return url;
        }

        private void RedirectWhenOldSessionExists(clsComparionCore app)
        {
            var context = HttpContext.Current;
            var hash = context.Request.QueryString["hash"] == null ? string.Empty : Request.QueryString["hash"].Trim();

            if (hash.Length > 0 && app.ActiveUser != null)
            {
                var url = context.Request.Url.AbsoluteUri;
                _Default.logout();

                context.Response.Redirect(url);
            }
        }
    }
}