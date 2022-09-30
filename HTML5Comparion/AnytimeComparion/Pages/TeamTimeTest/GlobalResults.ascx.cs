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

namespace AnytimeComparion.Pages.TeamTimeTest
{
    public partial class GlobalResults : System.Web.UI.UserControl
    {
        public clsComparionCore App;
        public clsHierarchy hierarchyObjectives;
        protected void Page_Load(object sender, EventArgs e)
        {
            App = (clsComparionCore)Session["App"];
            hierarchyObjectives = App.ActiveProject.HierarchyAlternatives;

        }
    }
}