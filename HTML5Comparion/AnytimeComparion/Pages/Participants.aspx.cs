using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AnytimeComparion.Pages
{
    public partial class Participants : System.Web.UI.Page
    {
        

        public ExpertChoice.Data.clsComparionCore apps;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            apps = (ExpertChoice.Data.clsComparionCore)Session["App"];

        }
    }
}