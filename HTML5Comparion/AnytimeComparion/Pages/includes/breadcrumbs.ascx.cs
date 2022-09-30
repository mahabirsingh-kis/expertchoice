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
using ECSecurity;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using System.Web.Services;
using System.Web.Script.Serialization;
using AnytimeComparion.Pages.external_classes;

namespace AnytimeComparion.Pages.includes
{
    public partial class breadcrumbs : System.Web.UI.UserControl
    {
        public clsComparionCore App;
        protected void Page_Load(object sender, EventArgs e)
        {
            App = (clsComparionCore)Session["App"];
        }



        protected void RefreshTeamTime(object sender, EventArgs e)
        {
            Response.Redirect("~/pages/teamtimetest/teamtime.aspx");
        }

        public string MeetingID
        {
            get { return Meeting_ID.InnerHtml; }
            set { Meeting_ID.InnerHtml = value; }
        }


    }
}