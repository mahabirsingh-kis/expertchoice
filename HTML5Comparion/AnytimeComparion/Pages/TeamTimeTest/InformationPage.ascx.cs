using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AnytimeComparion.Pages.TeamTimeTest
{
    public partial class InformationPage : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string information_text
        {
            set
            {
                //informationText.InnerHtml = value; 
            }
        }
    }
}