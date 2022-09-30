using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static AnytimeComparion.Pages.external_classes.TeamTimeClass;
using ExpertChoice.Data;
using ExpertChoice.Service;
using ExpertChoice.Web;
using static ExpertChoice.Data.Consts;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace AnytimeComparion.Pages
{
    public partial class synthesize_result : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        internal enum SynthesisViews
        {
            vtDefault = -1,
            vtAlternatives = 0,
            vtObjectives = 1,
            vtDSA = 2,
            vtPSA = 3,
            vtGSA = 4,
            vt2D = 5,
            vtHTH = 6,
            vtCV = 7,
            // Consensus View
            vtTree = 8,
            // Consensus View Tree
            vtAlternativesChart = -2,
            vtObjectivesChart = -3
        }

        internal static SynthesisViews SynthesisView = SynthesisViews.vtDefault;

        //        internal string ImagesPath = ThemePath() + _FILE_IMAGES;

        [WebMethod(EnableSession = true)]
        public static string GetData()
        {
            switch (SynthesisView)
            {
                
            }
            return null;
        }

    }
}