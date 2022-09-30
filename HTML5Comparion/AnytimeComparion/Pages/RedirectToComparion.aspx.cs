using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AnytimeComparion.Pages.external_classes;
using ExpertChoice.Data;
namespace AnytimeComparion.Pages
{
    public partial class RedirecToComparion : System.Web.UI.Page
    {
        public string ComparionUrl = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            ComparionUrl = AnytimeClass.GetComparionHashLink();
        }
    }
}