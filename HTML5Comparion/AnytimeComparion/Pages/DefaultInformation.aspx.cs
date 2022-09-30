using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using ECCore;
using ExpertChoice.Data;
using ECService = ExpertChoice.Service;
using ExpertChoice.Web;
using System.Web.Services;
using AnytimeComparion.Pages.external_classes;
using System.IO;
using static AnytimeComparion.Pages.external_classes.Constants;
using Newtonsoft.Json;

namespace AnytimeComparion.Pages
{
    public partial class DefaultInformationaspx : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var App = (clsComparionCore)Session["App"];
            if (App != null && App.ActiveUser!=null)
            {
                UserName.InnerHtml = App.ActiveUser.UserName;
            }
        }
    }
}