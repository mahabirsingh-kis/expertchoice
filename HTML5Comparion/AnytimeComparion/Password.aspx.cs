using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security.AntiXss;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using AnytimeComparion.Pages.external_classes;
using ExpertChoice.Data;
using ExpertChoice.Service;
using Options = ExpertChoice.Web.Options;

namespace AnytimeComparion
{
    public partial class Password : System.Web.UI.Page
    {
        private const string PasswordResetUser = "PasswordResetUser";
        private const string PasswordResetHash = "PasswordResetHash";

        protected void Page_Init(object sender, EventArgs e)
        {
            var key = ExpertChoice.Service.Common.ParamByName(Request.QueryString, Options._PARAMS_TINYURL).Trim();
            Session[PasswordResetHash] = key;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public static clsApplicationUser CurrentUser
        {
            get
            {
                var context = HttpContext.Current;
                if (context.Session[PasswordResetUser] == null)
                {
                    var App = (clsComparionCore) context.Session["App"];
                    if (App.ActiveUser != null)
                    {
                        return App.ActiveUser;
                    }
                }

                return (clsApplicationUser) context.Session[PasswordResetUser];
            }
        }

        [WebMethod(EnableSession = true)]
        public static object CheckPasswordResetUrl()
        {
            var context = HttpContext.Current;
            bool hasError = false;
            string message = "";

            var App = (clsComparionCore)context.Session["App"];
            string hash = (string) context.Session[PasswordResetHash];
            string key = hash;

            if (key != "")
            {
                key = App.DecodeTinyURL(key);
                if (!string.IsNullOrEmpty(key))
                {
                    key = ExpertChoice.Service.CryptService.DecodeURL(key, App.DatabaseID);
                    var paramList = HttpUtility.ParseQueryString(key);

                    if (paramList[Options._PARAM_ACTION] == "resetpsw")
                    {
                        string userEmail = paramList["ue"];
                        string userPassword = paramList["up"];

                        if (userEmail != "")
                        {
                            var user = App.DBUserByEmail(userEmail);
                            if (user != null && user.UserPassword == userPassword)
                            {
                                if (double.Parse(App.CanvasMasterDBVersion) >= 0.99)
                                {
                                    string sql = "SELECT Created FROM PrivateURLs WHERE hash = ?";
                                    List<object> sqlParam = new List<object>();
                                    sqlParam.Add(hash);

                                    var scalarValue = App.Database.ExecuteScalarSQL(sql, sqlParam);
                                    if (scalarValue != null)
                                    {
                                        var dateTime = (DateTime)scalarValue;
                                        if (dateTime < DateTime.Now.AddSeconds(-Consts._DEF_PASSWORD_LINK_TIMEOUT))
                                        {
                                            message = TeamTimeClass.ResString("errResetPswExpired");
                                            App.DBTinyURLDelete(-1, -2, user.UserID);
                                            hasError = true;
                                        }

                                        if (!hasError)
                                        {
                                            context.Session[PasswordResetUser] = user;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    message = TeamTimeClass.ResString("errTokenNotAvailable");
                    hasError = true;
                }
            }

            if (message.Length > 10)
            {
                int startIndex = message.IndexOf("<a");
                int endIndex = message.IndexOf(">", startIndex);
                var subString = message.Substring(startIndex, endIndex - startIndex);
                message = message.Replace(subString, "<a href='#' data-reveal-id='PasswordResetModal'");
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var returnObject = new
            {
                hasError = hasError,
                redirectUrl = hash == "" ? "Default.aspx" : "",
                message = message,
                passwordFieldText = TeamTimeClass.ResString("lblPassword"),
                passwordAgainFieldText = TeamTimeClass.ResString("lblPasswordAgain"),
                passwordEqualMessage = message = TeamTimeClass.ResString("msgPasswordsMustBeEqual")
        };

            return serializer.Serialize(returnObject);
        }

        [WebMethod(EnableSession = true)]
        public static object SaveNewPassword(string password, string reTypePassword)
        {
            var context = HttpContext.Current;
            string message = "", redirectUrl = "";
            bool isSuccess = false;

            if (CurrentUser != null)
            {
                if (password != reTypePassword)
                {
                    message = TeamTimeClass.ResString("msgPasswordsMustBeEqual");
                }
                else
                {
                    if (!ExpertChoice.Web.WebOptions.AllowBlankPsw() && password == "")
                    {
                        message = TeamTimeClass.ResString("msgBlankPsw");
                    }
                    else
                    {
                        var App = (clsComparionCore)context.Session["App"];
                        bool isAdmin = (CurrentUser.CannotBeDeleted && App.AvailableWorkgroups(CurrentUser).Count > 1);

                        if (!isAdmin)
                        {
                            bool justNewPsw = (CurrentUser.PasswordStatus == -1 || CurrentUser.UserPassword == "");
                            CurrentUser.UserPassword = AntiXssEncoder.HtmlEncode(password, true);
                            CurrentUser.PasswordStatus = 0;

                            if (App.DBUserUpdate(CurrentUser, false, "Create user password"))
                            {
                                App.DBTinyURLDelete(-1, -2, CurrentUser.UserID);

                                if (justNewPsw)
                                {
                                    context.Session.Remove(PasswordResetHash);
                                    context.Session.Remove(PasswordResetUser);

                                    isSuccess = true;
                                    redirectUrl = "Default.aspx";
                                }
                                else
                                {
                                    context.Session.Remove(PasswordResetHash);
                                    context.Session.Remove(PasswordResetUser);

                                    message = TeamTimeClass.ResString("msgPasswordSaved");
                                    isSuccess = true;
                                    App.Logout();
                                }
                            }
                            else
                            {
                                message = TeamTimeClass.ResString("errOnSavePsw");
                            }
                        }
                        else
                        {
                            message = TeamTimeClass.ResString("errResetPswNoAllowed");
                        }
                    }
                }
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var returnObject = new
            {
                isSuccess = isSuccess,
                redirectUrl = redirectUrl,
                message = message
            };

            return serializer.Serialize(returnObject);
        }
    }
}