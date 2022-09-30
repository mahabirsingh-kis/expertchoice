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
//using ECSecurity;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Service;
using ExpertChoice.Web;
using AnytimeComparion.Pages.TeamTimeTest;

namespace AnytimeComparion.Pages.TeamTimeTest
{
    public partial class temporaryList : System.Web.UI.UserControl
    {
        clsNode ParentNode = (clsNode)null;
        clsNode FirstNode = (clsNode)null;
        clsNode SecondNode = (clsNode)null;
        public clsComparionCore App;
        public clsProject Project;
        protected void Page_Load(object sender, EventArgs e)
        {
            App = (clsComparionCore)Session["App"];
            Project = App.ActiveProject;
        }
        
    }



       
}