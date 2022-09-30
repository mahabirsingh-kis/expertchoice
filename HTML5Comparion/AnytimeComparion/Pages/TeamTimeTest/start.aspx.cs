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

namespace AnytimeComparion.test
{
    public partial class start : System.Web.UI.Page
    {
        public clsComparionCore App;
        protected void Page_Load(object sender, EventArgs e)
        {
            App = (clsComparionCore)Session["App"];
        }

        [WebMethod(EnableSession = true)]
        public static string GetLink(int ProjectID)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var Passcode = App.DBProjectByID(ProjectID).Passcode.ToString();
            return Passcode;
        }

        [WebMethod(EnableSession = true)]
        public static bool CheckProject(int ProjectID, bool Status)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            bool fUpdate = false;
            if (Status == false)
            {
                App.ActiveProject = null;
                HttpContext.Current.Session["Project"] = null;

            }
            else
            {
                App.ActiveProject = App.DBProjectByID(ProjectID);
                var Project = App.ActiveProject;
                HttpContext.Current.Session["Project"] = App.ActiveProject;
                fUpdate = (bool)App.DBProjectUpdateToLastVersion(ref Project);
            }
            return fUpdate;
        }

        [WebMethod(EnableSession = true)]
        public static bool displayStatus(int projid)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var Project = App.DBProjectByID(projid);
            var status = false;
            if (Project.isOnline)
            { 
                Project.StatusDataLikelihood = 0;
                Project.StatusDataImpact = 0;
                Project.isOnline = false;
                status = false;
            }
            else
            {
                Project.StatusDataLikelihood = 1;
                Project.StatusDataImpact = 1;
                Project.isOnline = true;
                status = true;
            }
            App.DBProjectUpdate(ref Project);
            return status;
        }

        [WebMethod(EnableSession = true)]
        public static void setWorkgroup(string sVal)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var Workgroup = App.DBWorkgroupByName(sVal);
            App.ActiveWorkgroup = Workgroup;
        }
    }
}