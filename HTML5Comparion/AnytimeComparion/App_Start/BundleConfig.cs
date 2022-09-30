using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace AnytimeComparion
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254726
        public static void RegisterBundles(BundleCollection bundles)
        {

            //Bundle dont accept minified versions
            /*bundles.Add(new StyleBundle("~/bundles/ScriptsAtFooter").Include(
                "~/Content/bower_components/jquery/dist/jquery.min.js",
                "~/Content/bower_components/foundation/js/foundation.min.js",
                "~/Content/bower_components/slick/dist/slick.min.js",
                "~/Content/bower_components/pizza/snapsvg.min.js",
                "~/Content/bower_components/pizza/pizza.min.js",
                "~/Content/bower_components/jquery-ui/jquery-ui.min.js",
                "~/Scripts/app.js"));*/

            bundles.Add(new StyleBundle("~/bundles/LayoutCss")
                .Include("~/Content/stylesheets/app.css", new CssRewriteUrlTransform() )
                );


            bundles.Add(new StyleBundle("~/bundles/pipeCss").Include(
                "~/Content/stylesheets/nouislider.css")
                .Include("~/Content/stylesheets/nouislider.pips.css")
                .Include("~/Content/stylesheets/responsive-tables.css")
                .Include("~/Content/stylesheets/nouislider.tooltips.css"));

            bundles.Add(new StyleBundle("~/bundles/select2css").Include(
                "~/Content/select2/select2.min.css"
                ));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.8.2.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-1.8.24.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Scripts/angular.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            "~/Content/bower_components/modernizr/modernizr.js"));

            bundles.Add(new ScriptBundle("~/bundles/headerExtension").Include(
            "~/Scripts/ng-infinite-scroll.js",
            "~/Content/bower_components/jquery.cookie/jquery.cookie.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/mainController").Include(
            "~/Scripts/Controllers/MainController.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/foundation").Include(
            "~/Content/bower_components/foundation/js/foundation.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jsExtension").Include(
            "~/Scripts/angular-sanitize.min.js",
            "~/Scripts/angular-touch.min.js",
            "~/Scripts/clipboard.js",
            "~/Scripts/ngclipboard.min.js",
            "~/Content/bower_components/smoothscroll/ScrollToPlugin.min.js",
            "~/Scripts/app.js"));

            bundles.Add(new ScriptBundle("~/bundles/anytimeController").Include(
            "~/Scripts/Controllers/AnytimeController.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/defaultBrowserController").Include(
                 "~/Scripts/Controllers/DefaultBrowserController.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/teamtimeController").Include(
            "~/Scripts/Controllers/TeamTimeController.js",
             "~/Scripts/judgment.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/pipeExtension").Include(
            "~/Scripts/underscore.min.js",
            "~/Scripts/jquery-ui.touchpunch.min.js",
            "~/Scripts/Pipe/Global.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/pairwise").Include(
            "~/Scripts/Pipe/PairwiseVerbal.js",
            "~/Scripts/Pipe/PairwiseGraphical.js",
            "~/Scripts/NoUI/nouislider.min.js",
            "~/Scripts/NoUI/wNumb.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/stepFunction").Include(
            "~/Scripts/Pipe/StepFunction.js",
            "~/Content/bower_components/Chart.js/dist/Chart.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/utilityCurve").Include(
            "~/Scripts/Pipe/UtilityCurve.js",
            "~/Content/bower_components/Chart.js/dist/Chart.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/select2").Include(
                "~/Content/select2/select2.min.js"
                ));
            
            //bundles.Add(new ScriptBundle("~/bundles/WebFormsJs").Include(
            //      "~/Scripts/WebForms/WebForms.js",
            //      "~/Scripts/WebForms/WebUIValidation.js",
            //      "~/Scripts/WebForms/MenuStandards.js",
            //      "~/Scripts/WebForms/Focus.js",
            //      "~/Scripts/WebForms/GridView.js",
            //      "~/Scripts/WebForms/DetailsView.js",
            //      "~/Scripts/WebForms/TreeView.js",
            //      "~/Scripts/WebForms/WebParts.js"));

            //bundles.Add(new ScriptBundle("~/bundles/MsAjaxJs").Include(
            //    "~/Scripts/WebForms/MsAjax/MicrosoftAjax.js",
            //    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxApplicationServices.js",
            //    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxTimer.js",
            //    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js"));


        }
    }
}