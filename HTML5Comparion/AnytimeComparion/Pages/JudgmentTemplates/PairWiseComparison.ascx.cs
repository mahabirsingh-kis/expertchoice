using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AnytimeComparion.Pages.JudgmentTemplates
{
    public partial class PairWiseComparison : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //bool mobile = false;
            //System.Web.HttpBrowserCapabilities myBrowserCaps = Request.Browser;
            //if (((System.Web.Configuration.HttpCapabilitiesBase)myBrowserCaps).IsMobileDevice)
            //{
            //    mobile = true;
            //}

            //if (mobile)
            //{
            //    Response.Redirect("~/Pages/Mobile/PairwiseComparison.aspx?view=teamtime&actions=");
            //}
 
        }
        public string pairwisetypes;

        public string pairwisetype
        {
         get{ return pairwisetypes;}   
         set{ pairwisetypes = value;}
        }

        public string leftnd
        {
            get  { return this.leftnode.InnerHtml;}
            set  {this.leftnode.InnerHtml = value; }
        }
        public string rightnd
        {
            get { return rightnode.InnerHtml; }
            set { rightnode.InnerText = value; }
        }

        public string goal
        {
            set { goals.Text = value; }
        }

    }
}