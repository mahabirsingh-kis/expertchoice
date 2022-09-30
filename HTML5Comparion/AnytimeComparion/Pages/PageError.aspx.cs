using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Service;
using ECWeb = ExpertChoice.Web;

namespace AnytimeComparion.Pages
{
    public partial class WebForm5 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public clsComparionCore App
        {
           get
            {
                return (clsComparionCore)Session["App"];
            }
        }
    }
}