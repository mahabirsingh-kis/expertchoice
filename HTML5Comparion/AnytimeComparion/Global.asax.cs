using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using AnytimeComparion;
using System.Web.Http;
using System.Configuration;
using ECCore;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Web;
using AnytimeComparion.Pages.external_classes;

namespace AnytimeComparion
{
    public class Global : HttpApplication
    {
        public clsComparionCore api;
        public static string ServerError;
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterOpenAuth();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            RouteTable.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = System.Web.Http.RouteParameter.Optional }
            );

            RouteTable.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            Application["StartDate"] = DateTime.Now;
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown
        }

        void Application_Error(object sender, EventArgs e)
        {
             //Code that runs when an unhandled error occurs
            //ServerError = String.Format("<b>Message:</b><br/> {0}<br/><br/> <b>Target Site</b>:<br/> {1} <br/><br/> <b>Stack Trace:</b> <br/> {2} <br/><br/> <b>Inner Exception:</b> <br /> {3} <br/><br/> ", HttpContext.Current.Server.GetLastError().Message, HttpContext.Current.Server.GetLastError().TargetSite, HttpContext.Current.Server.GetLastError().StackTrace, HttpContext.Current.Server.GetLastError().InnerException);
            //HttpContext.Current.Server.ClearError();
            //_Default.logout();
            //Response.Redirect("~/pages/PageError.aspx");
        }

        protected void Session_Start(Object sender, EventArgs e)
        {
            //global here

            // Init main object App
            clsComparionCore App = new clsComparionCore();
            if (App.Options == null) { App.Options = new clsComparionCoreOptions(); }
            HttpContext context = HttpContext.Current;
             

            if (context != null && context.Session != null)
            {
                context.Session["App"] = App;
                System.Diagnostics.Trace.Write("\nApp is initialized!");
            }
            else
            {
                System.Diagnostics.Trace.Write("\nContext is null!");
            }

            // Specify Database name (SQLMasterDB) in web.config, appsettings.
            App.Options.CanvasMasterDBName = ConfigurationManager.AppSettings.Get("SQLMasterDB");
            App.Options.CanvasProjectsDBName = App.Options.CanvasMasterDBName;
            ExpertChoice.Database.ConnectionStringSection.GlobalDefaultProvider = ConfigurationManager.AppSettings.Get("SQLDefaultProvider");

            System.Diagnostics.Trace.Write(App.CanvasMasterDBVersion);

            if (!App.isCanvasMasterDBValid || !App.isCanvasProjectsDBValid)
                System.Diagnostics.Trace.Write(String.Format("Error: Database can't be found! Details: {0}", App.Database.LastError));

            _Default.LoadLanguange();
            WebOptions.LoadComparionCoreOptions(ref App);

            string sessionUID = "UID";
            string uId = (string) context.Session[sessionUID];
            if (string.IsNullOrEmpty(uId))
            {
                uId = context.Request.Cookies[Options._COOKIE_UID] == null ? "" : context.Request.Cookies[Options._COOKIE_UID].Value;
            }

            if (string.IsNullOrEmpty(uId))
            {
                uId = ExpertChoice.Service.StringFuncs.GetRandomString(6, true, false);
                context.Session.Add(sessionUID, uId);

                HttpCookie uIdCookie = new HttpCookie(Options._COOKIE_UID, uId)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(1)
                };

                if (FormsAuthentication.RequireSSL && context.Request.IsSecureConnection)
                {
                    uIdCookie.Secure = true;
                }

                context.Response.Cookies.Add(uIdCookie);
                //App.Options.isNewUID = true; !!!
            }
            else
            {
                //App.Options.isNewUID = false; !!!
            }

            //App.Options.UID = uId; !!!
            if (context.Request != null)
            {
                //App.Options.UserAgent = context.Request.UserAgent; !!!
                //App.Options.HostAddress = ExpertChoice.Service.Common.GetClientIP(context.Request); !!!
            }

            App.Options.SessionID = $"{uId}#{context.Session.SessionID.Substring(0, 4)}";


            //string sAppName = App.ResString(Convert.ToString((App.isRiskEnabled ? "titleApplicationNameRiskPlain" : "titleApplicationNamePlain"))).Trim().Replace(cha(169), "").Replace(Strings.Chr(174), "").Replace(Strings.Chr(153), "");
            //option_SystemEmail = clsComparionCorePage.ParseTemplateCommon(SystemEmail, _TEMPL_APPNAME, sAppName);
            //// D3858 + D3886 // ™ (c), (R), (tm)
            //// since we can't use %% on FinalBuilder
            //option_SystemEmail = option_SystemEmail.Replace("APPNAME", sAppName);
        }
    }
}
