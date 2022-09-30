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


namespace AnytimeComparion.test
{
    public partial class start : System.Web.UI.Page
    {
        public clsComparionCore apps;

        protected void Page_Load(object sender, EventArgs e)
        {
            apps = (clsComparionCore)Session["App"];
            
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            apps.ActiveProject.isTeamTime = true;
            Response.Redirect("teamtime.aspx");
        }

        protected void Button1_Click1(object sender, EventArgs e)
        {
            apps.ActiveProject.isTeamTime = false;
        }
    }
}