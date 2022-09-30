using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace AnytimeComparion.Pages
{
    public partial class DefaultBrowser : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Master != null)
            {
                Control listItem = Master.FindControl("liLogin");
                if (listItem != null)
                {
                    //listItem.Visible = false;
                    HtmlGenericControl anchorTag = new HtmlGenericControl("a");
                    anchorTag.InnerHtml = "&nbsp;";
                    anchorTag.Attributes.Add("href", "#");
                    listItem.Controls.Add(anchorTag);
                }

                Control linkLogin = Master.FindControl("linkLogin");
                if (linkLogin != null)
                {
                    linkLogin.Visible = false;
                }

                //if (Session["ComparionResponsiveLink"] != null)
                //{
                //    //HtmlGenericControl divControl = (HtmlGenericControl) FindControl("DivComparionResponsiveLink");
                //    //DefaultMessageDiv.Visible = false;
                //    DivComparionResponsiveLink.Attributes["class"] = "large-9 columns large-centered";

                //    //HtmlGenericControl spanControl = (HtmlGenericControl) divControl.FindControl("SpanComparionResponsiveLink");
                //    SpanComparionResponsiveLink.Value = Session["ComparionResponsiveLink"].ToString();
                //    CopyResponsiveLinkBtn.Attributes["data-clipboard-text"] = Session["ComparionResponsiveLink"].ToString();
                //    //LabelComparionResponsiveLink.Text = "Copy the following link and paste it on the browser mentioned above:\n" + Session["ComparionResponsiveLink"];
                //}
                //else
                //{
                //   // DefaultMessageDiv.Visible = true;
                //}
                //DefaultMessageDiv.Visible = true;
            }
            
            //CheckDefaultBrowser();
        }

        //Android Default Browsers: SamsungBrowser,
        private void CheckDefaultBrowser()
        {
            HttpBrowserCapabilities browser = Request.Browser;
            string userAgent = Request.ServerVariables["HTTP_USER_AGENT"];
            bool isDefault = false;

            switch (browser.Browser)
            {
                case "Chrome":
                    if (!(userAgent.Contains("Edge") || userAgent.Contains("OPR")))
                    {
                        isDefault = true;
                    }
                    break;
                case "Safari":
                    if (!(userAgent.Contains("FxiOS") || userAgent.Contains("OPiOS")))
                    {
                        isDefault = true;
                    }
                //case "Firefox":
                //case "InternetExplorer":
                    //isDefault = true;
                    break;
            }

            if (isDefault)
            {
                Response.Redirect("~/");
            }
            else
            {
                //LabelBrowserName.Text = GetBrowserInfo();
            }
        }

        private string GetBrowserInfo()
        {
            HttpBrowserCapabilities browser = Request.Browser;
            string s = "&nbsp;&nbsp;Type = " + browser.Type + ";"
                + "&nbsp;Name = " + browser.Browser + ";"
                + "&nbsp;Version = " + browser.Version + ";"
                + "&nbsp;UserAgent = " + Request.ServerVariables["HTTP_USER_AGENT"];

            return s;
        }

        //private string GetBrowserData()
        //{
        //    HttpBrowserCapabilities browser = Request.Browser;
        //    string s = "Browser Capabilities\n"
        //        + "Type = " + browser.Type + "\n"
        //        + "Name = " + browser.Browser + "\n"
        //        + "Version = " + browser.Version + "\n"
        //        + "Major Version = " + browser.MajorVersion + "\n"
        //        + "Minor Version = " + browser.MinorVersion + "\n"
        //        + "Platform = " + browser.Platform + "\n"
        //        + "Is Beta = " + browser.Beta + "\n"
        //        + "Is Crawler = " + browser.Crawler + "\n"
        //        + "Is AOL = " + browser.AOL + "\n"
        //        + "Is Win16 = " + browser.Win16 + "\n"
        //        + "Is Win32 = " + browser.Win32 + "\n"
        //        + "Supports Frames = " + browser.Frames + "\n"
        //        + "Supports Tables = " + browser.Tables + "\n"
        //        + "Supports Cookies = " + browser.Cookies + "\n"
        //        + "Supports VBScript = " + browser.VBScript + "\n"
        //        + "Supports JavaScript = " +
        //            browser.EcmaScriptVersion.ToString() + "\n"
        //        + "Supports Java Applets = " + browser.JavaApplets + "\n"
        //        + "Supports ActiveX Controls = " + browser.ActiveXControls
        //              + "\n"
        //        + "Supports JavaScript Version = " +
        //            browser["JavaScriptVersion"] + "\n"
        //        + "Capabilities = " + browser.Capabilities[""];

        //    return s;
        //}
    }
}