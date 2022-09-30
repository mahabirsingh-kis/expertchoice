using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace AnytimeComparion
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.EnableFriendlyUrls();

            //main pages
            routes.MapPageRoute("",
                "Judgement",
                "~/Pages/JudgmentTemplates/Judgement.aspx");
            routes.MapPageRoute("",
               "Resources",
               "~/Pages/Resources.aspx");
            routes.MapPageRoute("",
               "Participants",
               "~/Pages/Participants.aspx");
          
  
          
          
            //help and resource center
            routes.MapPageRoute("",
               "sample-videos",
               "~/Pages/sample-videos.aspx");
            routes.MapPageRoute("",
               "getting-started-videos-and-documentation",
               "~/Pages/getting-started-videos-and-documentation.aspx");
            routes.MapPageRoute("",
               "resource-allocation-videos",
               "~/Pages/resource-allocation-videos.aspx");
            routes.MapPageRoute("",
               "comparison-sample-project-files",
               "~/Pages/comparison-sample-project-files.aspx");
            routes.MapPageRoute("",
               "comparison-sample-template-files",
               "~/Pages/comparison-sample-template-files.aspx");
            routes.MapPageRoute("",
               "help-and-other-resources",
               "~/Pages/help-and-other-resources.aspx");


            //my projects page
            routes.MapPageRoute("",
               "my-projects",
               "~/Pages/my-projects.aspx");
            
            routes.MapPageRoute("",
               "structure-project",
               "~/Pages/structure-project.aspx");
            
            routes.MapPageRoute("",
               "synthesize-result",
               "~/Pages/Synthesize/synthesize-result.aspx");

            routes.MapPageRoute("",
               "allocate",
               "~/Pages/allocate.aspx");
            routes.MapPageRoute("",
               "results-reports",
               "~/Pages/results-reports.aspx");
            routes.MapPageRoute("",
              "settings",
              "~/Pages/settings.aspx");
          
            //collect input main page
            routes.MapPageRoute("",
               "collect-input",
               "~/Pages/CollectInput/CollectInputMain.aspx");
            
            //sub pages under collect input
            routes.MapPageRoute("",
               "collect-input/collect-my-input",
               "~/Pages/CollectInput/CollectMyInput.aspx");
            
            
            //sub pages under my projects
            routes.MapPageRoute("",
               "my-projects/archive",
               "~/Pages/MyProjects/archive.aspx");
            
            routes.MapPageRoute("",
               "my-projects/trash",
               "~/Pages/MyProjects/trash.aspx");

            routes.MapPageRoute("",
               "my-projects/templates",
               "~/Pages/MyProjects/templates.aspx");

          
            //temp pages
            routes.MapPageRoute("",
               "Directory",
               "~/Pages/JudgmentTemplates/Directory.aspx");

            //collect input sub pages
            //http://localhost:9793/my-projects/templates
            routes.MapPageRoute("",
               "CollectInput/MeetingSetup",
               "~/Pages/Gecko/MeetingSetup.aspx");

            routes.MapPageRoute("",
               "evaluationDone",
               "~/Pages/you-are-done.aspx");

        }
    }
}