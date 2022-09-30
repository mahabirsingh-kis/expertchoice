Imports Microsoft.VisualBasic
Imports System.Web
Imports System.Web.Hosting
Imports System.Configuration

Namespace ExpertChoice.Web

    ''' <summary>
    ''' Module with run-time options from web.config file and web-server parameters.
    ''' </summary>
    ''' <remarks>Could be used like as Options.*, when this unit is not imported</remarks>
    Public Module Options

#Region "String templates"

        ''' <summary>
        ''' Template for empty infodoc, which should be converted to HTML
        ''' </summary>
        ''' <remarks>Parse via String.Format, 0 -- Content, 1 - Title</remarks>
        Public Const _TEMPL_APPNAME As String = "%%appname%%"   ' D0060
        Public Const _TEMPL_APPNAME_PLAIN As String = "%%appname_plain%%"   ' D3886
        Public Const _TEMPL_APPNAME_TEAMTIME As String = "%%teamtime%%"     ' D3886
        Public Const _TEMPL_APPNAME_SURVEY As String = "%%survey%%"         ' D3886

        ' D6308 ===
        Public Const _TEMPL_PROJECT As String = "%%project%%"
        Public Const _TEMPL_PROJECTS As String = "%%projects%%"
        Public Const _TEMPL_MODEL As String = "%%model%%"
        Public Const _TEMPL_MODELS As String = "%%models%%"
        ' D6308 ==

        ' D0089 ===
        'Public Const _TEMPL_GROUPNAME As String = "%%groupname%%"  ' -D1830
        Public Const _TEMPL_USERNAME As String = "%%username%%"
        Public Const _TEMPL_USER_FIRSTNAME As String = "%%userfirstname%%"  ' D2279
        Public Const _TEMPL_USEREMAIL As String = "%%useremail%%"
        Public Const _TEMPL_USERPSW As String = "%%userpsw%%"    ' D0220
        ' D0089 ==
        Public Const _TEMPL_OWNERNAME As String = "%%ownername%%"   ' D0270
        Public Const _TEMPL_OWNEREMAIL As String = "%%owneremail%%" ' D0270
        Public Const _TEMPL_ROOT_URI As String = "%%root%%"         ' D0121
        Public Const _TEMPL_ROOT_PATH As String = "%%path%%"        ' D0121
        Public Const _TEMPL_ROOT_IMAGES As String = "%%images%%"    ' D0121
        Public Const _TEMPL_WORKGROUP As String = "%%workgroup%%"   ' D3401
        ' D0120 + D0220 ===
        Public Const _TEMPL_JUSTNODE As String = "%%node%%"         ' D1956
        Public Const _TEMPL_NODENAME As String = "%%nodename%%"
        Public Const _TEMPL_NODE_A As String = "%%nodeA%%"
        Public Const _TEMPL_NODE_B As String = "%%nodeB%%"
        Public Const _TEMPL_EVALCOUNT As String = "%%evalcount%%"
        Public Const _TEMPL_NODETYPE As String = "%%nodetype%%"     ' D2539
        ' D0120 ==

        ' D1503 ===
        Public Const _TEMPL_OBJECTIVES As String = "%%objectives%%"
        Public Const _TEMPL_OBJECTIVES_LIKELIHOOD As String = "%%objectives(l)%%"   ' D6080
        Public Const _TEMPL_OBJECTIVES_IMPACT As String = "%%objectives(i)%%"       ' D6080
        Public Const _TEMPL_OBJECTIVE As String = "%%objective%%"
        Public Const _TEMPL_OBJECTIVE_LIKELIHOOD As String = "%%objective(l)%%"
        Public Const _TEMPL_OBJECTIVE_IMPACT As String = "%%objective(i)%%"
        Public Const _TEMPL_ALTERNATIVES As String = "%%alternatives%%"
        Public Const _TEMPL_ALTERNATIVES_LIKELIHOOD As String = "%%alternatives(l)%%"   ' D6080
        Public Const _TEMPL_ALTERNATIVES_IMPACT As String = "%%alternatives(i)%%"       ' D6080
        Public Const _TEMPL_ALTERNATIVE As String = "%%alternative%%"
        ' D1503 ==

        Public Const _TEMPL_EVAL_OBJECT As String = "%%eval_object%%"   ' D6957

        Public Const _TEMPL_PROMT_ALT As String = "%%promt_alt%%"   ' D1819
        Public Const _TEMPL_PROMT_OBJ As String = "%%promt_obj%%"   ' D1819
        Public Const _TEMPL_PROMT_ALT_WORD As String = "%%promt_alt_word%%" ' D2457
        Public Const _TEMPL_PROMT_OBJ_WORD As String = "%%promt_obj_word%%" ' D2457

        Public Const _TEMPL_RATE_WORDING As String = "%%ratewording%%"  ' D2320
        Public Const _TEMPL_EST_WORDING As String = "%%estwording%%"    ' D2320

        Public Const _TEMPL_RATE_OBJ_WORDING As String = "%%rateobjwording%%"  ' D2449
        Public Const _TEMPL_EST_OBJ_WORDING As String = "%%estobjwording%%"    ' D2449

        Public Const _TEMPL_PRJNAME As String = "%%prjname%%"
        Public Const _TEMPL_PRJPASSCODE As String = "%%prjpasscode%%"

        Public Const _TEMPL_RISKMODEL As String = "%%riskmodel%%"       ' D1804
        Public Const _TEMPL_RISKMODELS As String = "%%riskmodels%%"     ' D3001

        Public Const _TEMPL_MEETING_ID As String = "%%meeting_id%%"     ' D0388

        Public Const _TEMPL_URL_APP As String = "%%url_app%%"
        Public Const _TEMPL_URL_APP_TT As String = "%%url_ttapp%%"  ' D3494
        Public Const _TEMPL_URL_LOGIN As String = "%%url_login%%"
        Public Const _TEMPL_URL_EVALUATE As String = "%%url_evaluate%%"
        Public Const _TEMPL_URL_EVALUATE_LIKELIHOOD As String = "%%url_evaluate_likelihood%%"   ' D2467
        Public Const _TEMPL_URL_EVALUATE_IMPACT As String = "%%url_evaluate_impact%%"           ' D2467
        Public Const _TEMPL_URL_EVALUATE_CONTROLS As String = "%%url_evaluate_controls%%"       ' D3769
        ' D1060 ===
        Public Const _TEMPL_URL_EVALUATE_ANONYM As String = "%%url_evaluate_anonym%%"
        Public Const _TEMPL_URL_EVALUATE_SIGNUP As String = "%%url_evaluate_signup%%"
        Public Const _TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY As String = "%%url_evaluate_signup_e%%"
        Public Const _TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY As String = "%%url_evaluate_signup_n%%"
        Public Const _TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW As String = "%%url_evaluate_signup_ep%%"
        ' D1060 ==
        Public Const _TEMPL_URL_EVALUATE_TT As String = "%%url_ttevaluate%%"    ' D0655
        Public Const _TEMPL_URL_MEETINGID As String = "%%url_meetingid%%"       ' D1352

        Public Const _TEMPL_URL_RESETPSW As String = "%%url_resetpsw%%"         ' D2216
        Public Const _TEMPL_PSWLOCK_TIMEOUT As String = "%%pswlock_timeout%%"   ' D6062
        Public Const _TEMPL_PSWLOCK_ATTEMPTS As String = "%%pswlock_attemps%%"  ' D6350

        ' D6377 ===
        Public Const _TEMPL_PAGE_PREFIX As String = "%%page_"
        Public Const _TEMPL_PAGE_URL_PREFIX As String = _TEMPL_PAGE_PREFIX + "url-"
        Public Const _TEMPL_PAGE_NAME_PREFIX As String = _TEMPL_PAGE_PREFIX + "name-"
        Public Const _TEMPL_PAGE_TITLE_PREFIX As String = _TEMPL_PAGE_PREFIX + "title-"
        Public Const _TEMPL_PAGE_LINK_PREFIX As String = _TEMPL_PAGE_PREFIX + "link-"
        ' D6337 ==

        Public Const _TEMPL_SERVICE_EMAIL As String = "%%serviceemail%%"

        Public Const _TEMPL_IP As String = "%%ip%%" ' D2413

        Public Const _TEMPL_MEETING_ID_STRING As String = "###-###-###"    ' D0384 + D0388

        Private _TEMPL_LIST_ALL_() As String = {_TEMPL_APPNAME, _TEMPL_APPNAME_PLAIN, _TEMPL_APPNAME_SURVEY, _TEMPL_APPNAME_TEAMTIME,
                                              _TEMPL_USEREMAIL, _TEMPL_USERNAME, _TEMPL_USER_FIRSTNAME,
                                              _TEMPL_PRJNAME, _TEMPL_PRJPASSCODE, _TEMPL_MEETING_ID,
                                              _TEMPL_URL_APP, _TEMPL_URL_APP_TT, _TEMPL_URL_LOGIN, _TEMPL_URL_EVALUATE, _TEMPL_URL_EVALUATE_TT,
                                              _TEMPL_URL_MEETINGID, _TEMPL_URL_EVALUATE_ANONYM, _TEMPL_URL_EVALUATE_SIGNUP, _TEMPL_URL_RESETPSW,
                                              _TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY,
                                              _TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW, _TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY,
                                              _TEMPL_SERVICE_EMAIL, _TEMPL_OWNERNAME, _TEMPL_OWNEREMAIL, _TEMPL_WORKGROUP}    ' D0270 + D0387 + D0439 + D0655 + D1060 + D1357 + D1830 + D2216 + D2279 + D3401 + D4772

        Public _TEMPL_LIST_NO_PRJ() As String = {_TEMPL_APPNAME, _TEMPL_APPNAME_PLAIN, _TEMPL_APPNAME_SURVEY, _TEMPL_APPNAME_TEAMTIME,
                                                 _TEMPL_USEREMAIL, _TEMPL_USERNAME, _TEMPL_USER_FIRSTNAME,
                                                 _TEMPL_URL_APP, _TEMPL_URL_LOGIN, _TEMPL_URL_RESETPSW, _TEMPL_SERVICE_EMAIL,
                                                 _TEMPL_OWNERNAME, _TEMPL_OWNEREMAIL, _TEMPL_WORKGROUP}    ' D3583 + D4772

        Private _TEMPL_LIST_RES_() As String = {"lblAplicationName", "lblAplicationNamePlain", "lblSurveyName", "lblTeamTimeName",
                                              "lblEmail", "lblUserName", "lblUserFirstName",
                                              "lblProjectName", "lblPasscode", "lblMeetingID",
                                              "lblApplicationURL", "lblApplicationTeamTimeURL", "lblLogonURL", "lblEvaluateURL", "lblEvaluateTTURL",
                                              "lblEvaluateURLMeetingID", "lblEvaluateURLAnonym", "lblEvaluateURLSignup", "lblResetPswURL",
                                              "lblEvaluateURLSignupEmailOnly",
                                              "lblEvaluateURLSignupNameOnly", "lblEvaluateURLSignupEmailPsw",
                                              "lblCustomerService", "lblOwnerName", "lblOwnerEmail", "lblWorkgroupActiveName"}    ' D0270 + D0387 + D1060 + D1357 + D1830 + D3401
        ' D0220 ==

        Public _TEMPL_LIST_EVALS As String() = {_TEMPL_OBJECTIVES, _TEMPL_OBJECTIVE, _TEMPL_ALTERNATIVES, _TEMPL_ALTERNATIVE,
                                                _TEMPL_PROMT_ALT, _TEMPL_PROMT_OBJ, _TEMPL_PROMT_ALT_WORD, _TEMPL_PROMT_OBJ_WORD,
                                                _TEMPL_RATE_WORDING, _TEMPL_EST_WORDING, _TEMPL_RATE_OBJ_WORDING, _TEMPL_EST_OBJ_WORDING}    ' D3519

        Public _TEMPL_LIST_EVALS_RISK As String() = {_TEMPL_OBJECTIVES, _TEMPL_OBJECTIVE, _TEMPL_ALTERNATIVES, _TEMPL_ALTERNATIVE,
                                                     _TEMPL_ALTERNATIVES_LIKELIHOOD, _TEMPL_ALTERNATIVES_IMPACT, _TEMPL_OBJECTIVE_LIKELIHOOD, _TEMPL_OBJECTIVE_IMPACT, _TEMPL_OBJECTIVES_LIKELIHOOD, _TEMPL_OBJECTIVES_IMPACT,
                                                     _TPL_RISK_CONTROL, _TPL_RISK_CONTROLS, _TPL_RISK_CONTROLLED, _TPL_RISK_IMPACT, _TPL_RISK_IMPACTS, _TPL_RISK_LIKELIHOOD,
                                                     _TPL_RISK_LIKELIHOODS, _TPL_RISK_RISK, _TPL_RISK_RISKS, _TPL_RISK_OPPORTUNITY, _TPL_RISK_OPPORTUNITIES, _TPL_RISK_VULNERABILITY, _TPL_RISK_VULNERABILITIES,
                                                     _TEMPL_PROMT_ALT, _TEMPL_PROMT_OBJ, _TEMPL_PROMT_ALT_WORD, _TEMPL_PROMT_OBJ_WORD,
                                                     _TEMPL_RATE_WORDING, _TEMPL_EST_WORDING, _TEMPL_RATE_OBJ_WORDING, _TEMPL_EST_OBJ_WORDING}    ' D4352 + D6736

        Public _TEMPL_LIST_URLS As String() = {_TEMPL_URL_APP, _TEMPL_URL_APP_TT, _TEMPL_URL_LOGIN,
                                               _TEMPL_URL_EVALUATE, _TEMPL_URL_EVALUATE_TT, _TEMPL_URL_MEETINGID,
                                               _TEMPL_URL_EVALUATE_ANONYM, _TEMPL_URL_EVALUATE_SIGNUP, _TEMPL_URL_RESETPSW,
                                               _TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY, _TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW, _TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY}    ' D1358 + Â4936

        Public _TEMPL_LIST_HIDE_URLS As String() = {_TEMPL_URL_EVALUATE, _TEMPL_URL_EVALUATE_ANONYM, _TEMPL_URL_EVALUATE_SIGNUP, _TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY,
                                                    _TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW, _TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY, _TEMPL_URL_EVALUATE_TT, _TEMPL_URL_LOGIN,
                                                    _TEMPL_URL_MEETINGID, _TEMPL_URL_EVALUATE_LIKELIHOOD, _TEMPL_URL_EVALUATE_IMPACT, _TEMPL_URL_EVALUATE_CONTROLS, _TEMPL_URL_RESETPSW}    ' D2477 + D3769 + D7432

        Public _SORT_ASC As String = "&nbsp;&#9650;"    ' D0162
        Public _SORT_DESC As String = "&nbsp;&#9660;"   ' D0162

        Public Const _ACCESSKEY_PREFIX As String = "~"  ' D0246

        ' (!) All backdoors constants should be in the lower case only
        Public Const _BACKDOOR_PLACESRATED As String = "placesrated"     ' D0338

        Public Const _CUSTOM_SENDER_EMAIL As Boolean = False    ' D1144

#End Region

#Region "Cookies"

        ' D0016 ===
        Public Const _COOKIE_UID As String = "UID"                      ' D2289
        Public Const _COOKIE_EMAIL As String = "CanvasUserEMail"
        Public Const _COOKIE_PASSWORD As String = "CanvasUserPsw"
        Public Const _COOKIE_START_EMAIL As String = "Start_UID"        ' D1057
        Public Const _COOKIE_START_PWD As String = "Start_PWD"          ' D1057
        'Public Const _COOKIE_PASSCODE As String = "CanvasPasscode"      ' D0033 -D6287
        Public Const _COOKIE_REMEMBER As String = "CanvasRememberMe"
        Public Const _COOKIE_LANGUAGE As String = "CanvasLanguage"      ' D0125
        Public Const _COOKIE_MEETING_ID As String = "CanvasMeetingID"       ' D0387
        Public Const _COOKIE_MEETING_LOGIN As String = "CanvasMeetingLogin" ' D0387
        Public Const _COOKIE_RET_USER As String = "retuser"             ' D1529
        Public Const _COOKIE_RET_PRJID As String = "retprj"             ' D1529
        Public Const _COOKIE_RET_HID As String = "rethid"               ' D1672
        Public Const _COOKIE_RET_PATH As String = "retpath"             ' D1529
        Public Const _COOKIE_RET_REMEMBER As String = "retrem"          ' D1696
        Public Const _COOKIE_RET_PASSCODE As String = "retpasscode"     ' D1696
        'Public Const _COOKIE_RET_ORIG_PASSCODE As String = "ret_orig_passcode"      ' D4744 -D6287
        Public Const _COOKIE_IS_ADVANCED As String = "isAdvancedMode"   ' D5083
        Public Const _COOKIE_SSO_DEBUG As String = "SSO_Debug"          ' D7424

        ' D0157 ===
        Public Const _COOKIE_DEBUG_AUTOLOGON As String = "AutoLogon"
        Public Const _COOKIE_DEBUG_LASTPAGE As String = "LastPage"
        Public Const _COOKIE_DEBUG_CHECKMODEL As String = "CheckModel"
        ' D0157 ==
        Public Const _COOKIE_FORCE_SL As String = "ForceSLShell"    ' D0779
        'Public Const _COOKIE_USED_SL As String = "SLUsed"           ' D1105

        Public Const _COOKIE_SHOW_LANDING As String = "ShowLanding"  ' D5001
        Public Const _COOKIE_SHOW_KNOWNISSUES As String = "ShowKnownIssues" ' D6034

        Public Const _COOKIE_DEBUG_AUTOCOMPLETE As String = "AutoComplete"  ' D3620

        Public Const _COOKIE_DEBUG_CHECK_SL As String = "CheckSL"   ' D0529

        Public Const _COOKIE_MASTER_PAGE As String = "MasterPage"   ' D0560 + D0781

        Public Const _COOKIE_STRUCTURING_MODE As String = "StructuringMode_" ' D0609 + D0627

        Public Const _COOKIE_WIPEOUT As String = "Wipeout"   ' D0941 + D0945

        Public Const _COOKIE_NOVICE_IDLEHELP As String = "NoviceIdleHelp"   ' D1321
        'Public Const _COOKIE_NOVICE_MISSING As String = "NoviceMissing"     ' D1321 + D2952
        Public Const _COOKIE_NOVICE_PWEXTREME As String = "NovicePWExtreme" ' D1322

        Public Const _COOKIE_QH_AUTOSHOW As String = "QHAutoShow"   ' D3738

        Public Const _COOKIE_ACTIVE_SESS As String = "ActSess"      ' D4023

        Public Const _COOKIE_FEDRAMP_NOTIFICATION As String = "NotifyAccepted"      ' D6081

        Public Const _COOKIE_LAST_NEWS As String = "LastNews"       ' D5044
        Public Const _COOKIE_LAST_ISSUES As String = "LastIssues"   ' D6034

        ''' <summary>
        ''' Timeout in seconds for cookies expiration on client-side. By default -- near the 1 month.
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _EXPIRE_COOKIES As Integer = 30 * 24 * 3600    ' D0043
        ' D0016 ==

#End Region

#Region "URLs and URI params"

        Public _URL_EXT_PLACESRATED As String = "http://placesrated.expertchoice.com/"  ' D0349

        ' D0043 ===
        'Public _URL_ROOT As String = String.Concat(HostingEnvironment.ApplicationVirtualPath, "/").Replace("//", "/")
        Public _URL_THEMES As String = _URL_ROOT + "App_Themes/"    ' D0090
        Public _URL_API As String = _URL_ROOT + "api/"              ' D7215
        Public _URL_ADMIN As String = _URL_ROOT + "Admin/"
        Public _URL_ACCOUNT As String = _URL_ROOT + "Account/"
        Public _URL_PROJECT As String = _URL_ROOT + "Project/"
        Public _URL_EVALUATE As String = _URL_PROJECT + "Evaluate/"
        Public _URL_TEAMTINE As String = _URL_PROJECT + "TeamTime/"  ' D0192 + D0391 + D4705
        Public _URL_STRUCTURE As String = _URL_PROJECT + "Structure/"
        Public _URL_ANTIGUA As String = _URL_ROOT + "Collaborative/"       ' D0580 + D0582 + A1052
        Public _URL_ANALYSIS As String = _URL_PROJECT + "Analysis/"     ' D0062
        Public _URL_SPYRON As String = _URL_ROOT + "Spyron/" ' D0047
        Public _URL_REPORTS As String = _URL_SPYRON + "Reports/" ' D0047
        ' D0144 ===
        Public _URL_SURVEYS As String = _URL_SPYRON + "Surveys/" ' D0190
        Public _URL_RA As String = _URL_PROJECT + "RA/" ' D2699
        Public _URL_DOCMEDIA As String = _URL_ROOT + "DocMedia/"
        'Public _URL_DOWNLOADS As String = _URL_DOCMEDIA + "Downloads/"  ' D0425
        Public _URL_EULA As String = _URL_DOCMEDIA + "EULA/"  ' D4628
        'Public _URL_HELP As String = _URL_DOCMEDIA + "Help/"
        'Public _URL_VIDEO_HELP As String = _URL_DOCMEDIA + "VideoHelp/" ' D0311

        Public _WEBAPI_ROOT As String = _URL_ROOT + "api/"

        ' D0076 ===
        Public Function URLWithParams(ByVal sURL As String, ByVal sParameters As String) As String
            If sParameters = "" Then Return RemoveXssFromUrl(sURL)  ' Anti-XSS
            RemoveXssFromUrl(sURL)  ' Anti-XSS
            sParameters = sParameters.Replace("&&", "&").Trim(CChar("&")).Trim(CChar("?")).Trim ' D6766
            If sURL <> "" Then sParameters = CStr(IIf(sURL.IndexOf("?") >= 0, "&", "?")) + RemoveXssFromUrl(sParameters)    ' D6552
            Return sURL + sParameters
        End Function

        Public Function URLParameter(ByVal sURL As String, ByVal sParamName As String, ByVal sParamValue As Object) As String
            Return URLWithParams(sURL, String.Format("{0}={1}", sParamName, sParamValue))
        End Function

        Public Function URLWithAction(ByVal sURL As String, ByVal sAction As String) As String
            Return URLParameter(sURL, _PARAM_ACTION, sAction)
        End Function

        Public Function URLTab(ByVal sURL As String, ByVal sTabName As String) As String
            Return URLParameter(sURL, _PARAM_TAB, sTabName)
        End Function

        ' D4645 ===
        Public Function URLPage(ByVal sURL As String, ByVal PgID As Integer) As String
            Return URLParameter(sURL, _PARAM_PAGE, PgID.ToString)
        End Function
        ' D4645 ==

        ' D5057 ===
        Public Function URLNavPage(ByVal sURL As String, ByVal PgID As Integer) As String
            Return URLParameter(sURL, _PARAM_NAV_PAGE, PgID.ToString)
        End Function
        ' D5057 ==

        Public Function URLObjectID(ByVal sURL As String, ByVal ID As Integer) As String
            Return URLParameter(sURL, _PARAM_TAB, ID)
        End Function

        Public Function URLProjectID(ByVal sURL As String, ByVal ProjectID As Integer) As String
            Return URLParameter(sURL, _PARAM_PROJECT, ProjectID)
        End Function

        ' D0076 ==

        ' D0214 ===
        Public _PARAMS_KEY() As String = {"key", "token", "code"}
        Public _PARAMS_TINYURL() As String = {"hash", "tinyurl"}    ' D0896
        Public _PARAMS_EMAIL() As String = {"email", "login"}  ' D0227
        Public _PARAMS_USERNAME() As String = {"username", "name"}  ' D0226
        Public _PARAMS_PASSWORD() As String = {"password", "pwd", "psw", "pass"}
        Public _PARAMS_PASSCODE() As String = {"passcode", "code", "access", "accesscode", "decision", "model"}
        Public _PARAMS_REMEMBERME() As String = {"remember", "rememberme", "store"}
        Public _PARAMS_SIGNUP() As String = {"allowsignup", "signupallowed", "autosignup", "signup"}    ' D0226
        Public _PARAMS_SIGNUP_MODE() As String = {"signupmode", "askonsignup"}      ' D1056
        Public _PARAMS_ANONYMOUS_SIGNUP() As String = {"anonym", "anonymous", "anonymsignup"}   ' D1056
        Public _PARAMS_MEETING_ID() As String = {"meeting_id", "meetingid", "meeting"}  ' D0389
        Public _PARAMS_HID() As String = {"hid", "hierarchy"}   ' D1672
        ' D0214 ==
        Public _PARAMS_LOGON() As String = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},ttonly,pipe,phone,fld,req,ignoreval,ignoreeval,ignoreoffline,ignorepsw,pin",
                                                         String.Join(",", _PARAMS_KEY), String.Join(",", _PARAMS_TINYURL), String.Join(",", _PARAMS_EMAIL), String.Join(",", _PARAMS_USERNAME), String.Join(",", _PARAMS_PASSWORD),
                                                         String.Join(",", _PARAMS_PASSCODE), String.Join(",", _PARAMS_REMEMBERME), String.Join(",", _PARAMS_SIGNUP), String.Join(",", _PARAMS_SIGNUP_MODE), String.Join(",", _PARAMS_ANONYMOUS_SIGNUP), String.Join(",", _PARAMS_MEETING_ID),
                                                         _PARAM_AS_PM, _PARAM_WKG_ROLEGROUP, _PARAM_ROLEGROUP, _PARAM_SPECIAL_LOGON).Split(CChar(","))    ' D6347 + D6364 + D7205

        Public Const _PARAM_SSO_PREFIX As String = "SSO_"   ' D7439

        Public Const _PARAM_ACTION As String = "action"
        Public Const _PARAM_PAGE_ACTION As String = "page_action" 'A0888
        Public Const _PARAM_STEP As String = "step"     ' D0116
        Public Const _PARAM_PROJECT As String = "project"
        Public Const _PARAM_ID As String = "id"
        Public Const _PARAM_COUNT As String = "count"
        Public Const _PARAM_NODE As String = "node"
        Public Const _PARAM_DATA As String = "data"
        Public Const _PARAM_TAB As String = "tab"
        Public Const _PARAM_PAGE As String = "pgid"         ' D4645
        Public Const _PARAM_NAV_PAGE As String = "navpgid"  ' D5057
        Public Const _PARAM_THEME As String = "theme"       ' D0652
        Public Const _PARAM_TEMP_THEME As String = "temptheme"  ' D0766
        Public Const _PARAM_SET As String = "set"  'A1544
        ' D0214 ===
        Public Const _PARAM_EMAIL As String = "email"
        Public Const _PARAM_PASSWORD As String = "password"
        Public Const _PARAM_PASSCODE As String = "passcode"
        Public Const _PARAM_REMEMBERME As String = "remember"
        Public Const _PARAM_MEETING_ID As String = "meeting_id"     ' D0389
        Public Const _PARAM_ERROR As String = "error"
        Public Const _PARAM_SPECIAL_LOGON As String = "special_logon"   ' D7205
        ' D0214 ==
        Public Const _PARAM_DATAINSTANCE As String = "datainstance" ' D0228
        Public Const _PARAM_INTENSITIES As String = "intensities"   ' D1800
        Public Const _PARAM_READONLY As String = "readonly" ' D0309

        Public Const _PARAM_INVITATION_EVAL As String = "invitation"    ' D0503 + D1358
        Public Const _PARAM_INVITATION_TT As String = "invitation_tt"   ' D1358
        'Public Const _PARAM_INVITATION_SUBJ As String = "eval_invit_subj"   ' D0509
        Public Const _PARAM_TT_INVITATION_BODY As String = "tt_invit_body"  ' D0509
        Public Const _PARAM_TT_INVITATION_SUBJ As String = "tt_invit_subj"  ' D0509
        Public Const _PARAM_INVITATION_CUSTOM As String = "custom_invite"   ' D4647

        Public Const _PARAM_ROLEGROUP As String = "rgid"        ' D1936
        Public Const _PARAM_WKG_ROLEGROUP As String = "wkgrole" ' D2285
        Public Const _PARAM_AS_PM As String = "aspm"            ' D4332

        Public Const _ACTION_LOGOUT As String = "logout"
        Public Const _ACTION_NEW As String = "new"
        Public Const _ACTION_EDIT As String = "edit"
        Public Const _ACTION_DELETE As String = "delete"
        Public Const _ACTION_LOAD As String = "load"
        Public Const _ACTION_SAVE As String = "save"
        Public Const _ACTION_SAVENAME As String = "savename"
        Public Const _ACTION_RESET As String = "reset"

        Public Const _ACTION_NAVIGATION As String = "navigation"    ' D0349

        Public Const _ROLE_HIDE As String = "hide"    ' D0044
        Public Const _ROLE_JS As String = "js"          ' D0556
        Public Const _ROLE_RISK As String = "risk"      ' D3851
        Public Const _ROLE_NORISK As String = "norisk"  ' D3851

#End Region

        ' D1394 + D2658 ===
        Public Function isIE(Request As HttpRequest) As Boolean
            If Request IsNot Nothing AndAlso Request.UserAgent IsNot Nothing Then
                Dim UA As String = Request.UserAgent.ToLower
                If UA.Contains("msie") OrElse (UA.Contains("gecko") AndAlso UA.Contains("trident") AndAlso
                                               Not UA.Contains("chrome") AndAlso Not UA.Contains("firefox") AndAlso
                                               Not UA.Contains("webkit") AndAlso Not UA.Contains("safari")) Then Return True
            End If
            Return False
        End Function
        ' D1394 + D2658 ==

        ' D0867 ===
        Public Function GetSourceNameNoCache(ByVal sSource As String) As String
            Dim param As String = sSource
            If Not System.Diagnostics.Debugger.IsAttached Then
                Dim sFilename As String = HttpContext.Current.Server.MapPath(param)
                If My.Computer.FileSystem.FileExists(sFilename) Then
                    Dim CreationDate As DateTime = System.IO.File.GetLastWriteTime(sFilename)
                    param += "?dt=" + HttpUtility.UrlEncode(CreationDate.ToString)
                End If
            End If
            Return param
        End Function
        ' D0867 ==

        ' D2467 ===
        Public Function _TEMPL_LIST_ALL(fIsRiskEnabled As Boolean) As String()
            Dim sList As String() = _TEMPL_LIST_ALL_
            If fIsRiskEnabled Then
                Dim L As Integer = sList.Length
                Array.Resize(sList, L + 5)  ' D3769
                sList(L) = _TEMPL_RISKMODEL
                sList(L + 1) = _TEMPL_RISKMODELS ' D3001 + D3247
                sList(L + 2) = _TEMPL_URL_EVALUATE_LIKELIHOOD
                sList(L + 3) = _TEMPL_URL_EVALUATE_IMPACT
                sList(L + 4) = _TEMPL_URL_EVALUATE_CONTROLS ' D3769
            End If
            Return sList
        End Function

        Public Function _TEMPL_LIST_RES(fIsRiskEnabled As Boolean) As String()
            Dim sList As String() = _TEMPL_LIST_RES_
            If fIsRiskEnabled Then
                Dim L As Integer = sList.Length
                Array.Resize(sList, L + 5)  ' D3247 + D4065
                sList(L) = "lblRiskModel"
                sList(L + 1) = "lblRiskModels"  ' D3247
                sList(L + 2) = "lblEvalURLLikelihood"
                sList(L + 3) = "lblEvalURLImpact"
                sList(L + 4) = "lblEvalURLControls" ' D4065
            End If
            Return sList
        End Function
        ' D2467 ==

        ' D4061 ===
        Public Function isSecureConnection(tReq As HttpRequest) As Boolean
            Dim loadbalancerReceivedSSLRequest As Boolean = String.Equals(tReq.Headers("X-Forwarded-Proto"), "https")
            Dim serverReceivedSSLRequest As Boolean = tReq.IsSecureConnection
            Return (loadbalancerReceivedSSLRequest Or serverReceivedSSLRequest)
        End Function
        ' D4061 ==

        ' D4988 ===
        Public Property PMShowInstruction(App As clsComparionCore, tUserID As Integer) As Boolean
            Get
                Dim retVal As Boolean = False
                Dim tOption As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(tUserID, ecExtraType.SilverLight, ecExtraProperty.ShowInstruction))
                If tOption Is Nothing Then retVal = True Else retVal = CStr(tOption.Value) = "1" ' D3622
                Return retVal
            End Get
            Set(value As Boolean)
                App.DBExtraWrite(clsExtra.Params2Extra(tUserID, ecExtraType.SilverLight, ecExtraProperty.ShowInstruction, IIf(value, 1, 0)))    ' D3622
            End Set
        End Property
        ' D4988 ==

    End Module

End Namespace
