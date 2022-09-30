using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnytimeComparion.Pages.external_classes
{
    public class Constants
    {
        //SESSION NAMES
        public const string Sess_SignUp = "Sess_SignUp";
        public const string Sess_SignUp_ProjName = "Sess_SignUp_ProjName";
        public const string Sess_SignUp_Passcode = "Sess_SignUp_Passcode";
        public const string Sess_SignUpMode = "Sess_SignUpMode";
        public const string Sess_Requirements = "Sess_Requirements";
        public const string Sess_ShowMessage = "Sess_ShowMessage";
        public const string Sess_InviteMessage = "Sess_InviteMessage";
        public const string Sess_Project_Status = "Sess_Project_Status";
        public const string Sess_RoleGroup = "Sess_Role_Group";
        public const string Sess_ShowEqualOnce = "Sess_ShowEqualOnce";
        public const string Sess_ExpectedValue = "Sess_ExpectedValue";
        public const string Sess_ForceError = "Sess_ForceError";
        public const string Sess_PipeWarning = "Sess_PipeWarning";
        public const string Sess_LoginMethod = "Sess_LoginMethod";  //0 - login page, 1 - links
        public const string Sess_RemoveAnonymCookie = "Sess_RemoveAnonymCookie";
        public const string Sess_FromComparion = "Sess_FromComparion";

        public const string Cook_Extreme = "Cook_Extreme";

        public const string SessionModel = "SessionModel";
        public const string SessionQhNode = "SessionQhNode";
        public const string SessionViewOnlyUserId = "SessionViewOnlyUserId";
        public const string SessionIsPipeViewOnly = "SessionIsPipeViewOnly";
        public const string SessionParamStep = "SessionParamStep";
        public const string SessionNonRMode = "SessionNonRMode";
        public const string SessionNonRNode = "SessionNonRNode";
        public const string SessionNonRMtType = "SessionNonRMtType";
        public const string SessionIsInterResultStepFound = "SessionIsInterResultStepFound";

        //For KnowledgeOwl Help
        public const string SessionKoToken = "SessionKnowledgeOwlToken";
        public const string SessionKoExpiresIn = "SessionKnowledgeOwlExpiresIn";
        public const string KoTokenUrl = "https://app.knowledgeowl.com/oauth2/token";
        public const string KoProjectId = "5c775aa76e121c7c09b9dd85-5c775b388e121c822d196668";
        public const string KoClientId = "5c775b388e121c822d196668";
        public const string KoClientSecret = "9dbaffdc6cd182cab7a696b5116fe8eadbfc7d669dda5fdf";
    }

    public enum EcSettingType
    {
        MaxPasswordAttempts = 0,
        LockPasswordTimeout = 1
    }

    internal class KnowledgeOwlAuthToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public object scope { get; set; }
    }
}