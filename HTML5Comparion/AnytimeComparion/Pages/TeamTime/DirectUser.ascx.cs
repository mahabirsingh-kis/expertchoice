using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AnytimeComparion.Pages.JudgmentTemplates
{
    public partial class DirectUser : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string pairwisetypes;

        public string pairwisetype
        {
            get { return pairwisetypes; }
            set { pairwisetypes = value; }
        }

        public string alternative
        {
            get { return this.alternativeLbl.Text; }
            set { this.alternativeLbl.Text = value; }
        }
        public string objective
        {
            get { return objectLbl.Text; }
            set { objectLbl.Text = value; }
        }

        public string goal
        {
            get { return goals.Text; }
            set { goals.Text = value; }
        }
    }
}