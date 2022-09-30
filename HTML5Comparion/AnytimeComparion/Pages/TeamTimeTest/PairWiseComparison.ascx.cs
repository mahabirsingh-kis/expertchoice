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
using AnytimeComparion.Pages.external_classes;

namespace AnytimeComparion.Pages.TeamTimeTest
{
    public partial class PairWiseComparison : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            //if(TeamTimeClass.isTeamTimeOwner)
            //    var nodes = TeamTimeClass.getnodes(TeamTimeClass.TeamTimePipeStep);
        }
        public string pairwisetypes;

        public string pairwisetype
        {
            get { return pairwisetypes; }
            set { pairwisetypes = value; }
        }

        
    }
}