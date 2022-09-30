Imports System.Web.Hosting
Imports ExpertChoice.Service
Imports ExpertChoice.Database

Namespace ExpertChoice.Web

    ''' <summary>
    ''' Module with run-time options from web.config file and web-server parameters.
    ''' </summary>
    ''' <remarks>Could be used like as Options.*, when this unit is not imported</remarks>
    Public Module WebOptions

#Region "Web.config Option names"

        ''' <summary>
        ''' Web.config option name for store Master CanvasDB name
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_CANVASMASTERDB As String = "SQLMasterDB"
        ''' <summary>
        ''' Web.config option name for store Username for access to SQL Server
        ''' </summary>
        ''' <remarks></remarks>

        Public Const _OPT_CANVASPROJECTSDB As String = "SQLProjectsDB"          ' D0354

        'Public Const _OPT_SPYRONMASTERDB As String = "SQLSpyronDB"              ' D0236 -D6423

        'Public Const _OPT_SPYRONPROJECTSDB As String = "SQLSpyronProjectsDB"    ' D0375 -D6423

        Public Const _OPT_DEFAULT_PROVIDER As String = "SQLDefaultProvider"    ' D0330

        Public Const _OPT_MASTER_PAGE As String = "MasterPage"  ' D0335

        Public Const _OPT_BACKDOOR As String = "BackDoor"   ' D0338

        ''' <summary>
        ''' Web.config option name for define pre-defined .resx-file with language
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_DEFLANG As String = "LanguageFile"    ' D0030
        ''' <summary>
        ''' Option for show languages selector. By default is true
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_SHOW_LANGS As String = "ShowLanguages"    ' D0211
        ''' <summary>
        ''' Web.config option name for use Application via forced SSL connection
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_FORCESSL As String = "ForceSSL"       ' D0035
        ''' <summary>
        ''' Web.config option name for use secure connections for SMTP
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_SMTPSSL As String = "SMTP_USESSL"         ' D0758

        Public Const _OPT_DISABLEAUTOCOMPLETEFORLOGIN As String = "DisableAutoCompleteForLogin"

        ''' <summary>
        ''' Web.config option name for store FogBugz URL (http://)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_FOGBUGZ_URL As String = "FogBugz_URL"             ' D0076

        Public Const _OPT_CHANGESET As String = "Changeset"                 ' D1400

        ' -D0883
        '''' <summary>
        '''' Web.config option name for URL for FogBugz URL with Create New Case form
        '''' </summary>
        '''' <remarks></remarks>
        'Public Const _OPT_FOGBUGZ_FEEDBACK As String = "FogBugz_Feedback"   ' D0118
        ''' <summary>
        ''' Web.config option name for store FogBugz ScoutName for post feedbacks
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_FOGBUGZ_USERNAME As String = "FogBugz_UserName"   ' D0118
        ''' <summary>
        ''' Web.config option name for store FogBugz ScoutProject for post feedbacks
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_FOGBUGZ_PROJECT As String = "FogBugz_Project"     ' D0118
        ''' <summary>
        ''' Web.config option name for store FogBugz ScoutArea for post feedbacks
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_FOGBUGZ_AREA As String = "FogBugz_Area"           ' D0118

        ''' <summary>
        ''' Mailbox for sending bug reports to FogBugz inbox
        ''' </summary>
        ''' <remarks>By default is empty. Use address like "cases@....fogbugz.com" </remarks>
        Public Const _OPT_FOGBUGZ_INBOX As String = "FogBugz_Inbox"         ' D0880

        ' -D3083
        ' ''' <summary>
        ' ''' Default E-mail address for submit feedback/Customer Support to FogBugz
        ' ''' </summary>
        ' ''' <remarks></remarks>
        'Public Const _OPT_FOGBUGZ_NOREPLY As String = "FogBugz_NoReply"     ' D0886

        ''' <summary>
        ''' FogBugz feedback submission: 1 for auto-submit and hide "Feedback" button. By default is disabled for local instances.
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_FOGBUGZ_AUTOSUBMIT As String = "FogBugz_AutoSubmit"   ' D0880

        ''' <summary>
        ''' Option for enable/disabled trace/output info. Set "1" for enable when debugging. By default is "0".
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_TRACE_ENABLED As String = "TraceEnabled"  ' D0461

        Public Const _OPT_OPTIONS_OVERRIDE As String = "OptionsOverride"    ' D3821

        ''' <summary>
        ''' Web.config option name for store project lock timeout
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_LOCKTIMEOUT As String = "LockTimeout"             ' D0135
        ''' <summary>
        ''' Web.config option name for store LoadOnDemand option
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_LOADONDEMAND As String = "LoadOnDemand"           ' D0138

        Public Const _OPT_FORCEALLOWEDALTS As String = "ForceAllowedAlts"           ' D0245

        Public Const _OPT_USE_DATAMAPPING As String = "UseDataMapping"      ' D4465

        ''' <summary>
        ''' Period in second for send request on Evaluation screen during Synchronous sessions
        ''' </summary>
        ''' <remarks>By default is 5</remarks>
        Public Const _OPT_SYNCHRONOUS_REFRESH As String = "SynchronousRefresh"      ' D0194

        ' -D2622
        ' ''' <summary>
        ' ''' Count of concurrent participants for one TeamTime session
        ' ''' </summary>
        ' ''' <remarks>Use -1 as unlimited</remarks>
        'Public Const _OPT_SYNCHRONOUS_MAXUSERS As String = "SynchronousMaxUsers"     ' D0450

        ''' <summary>
        ''' Show unfinished screens in navigation
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_SHOW_DRAFT As String = "ShowDraftPages"

        Public Const _OPT_LICENSE_MODE As String = "LicenseMode"    ' D0267

        Public Const _OPT_CHECK_EULA As String = "CheckEULA"    ' D0304

        Public Const _OPT_HOOK_ERRORS As String = "HookErrors"  ' D0560

        ''' <summary>
        ''' Use this option for force all not-secured requests to HTTPS calls (value="1/0"). By default is "0".
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_SLSHELL_BY_DEFAULT As String = "SLShellByDefault"     ' D0729

        Public Const _OPT_SHOW_CHAT_SUPPORT As String = "ShowChatSupport"       ' D4220

        Public Const _OPT_ALLOW_REVIEW_ACCOUNT As Boolean = False               ' D5068

        Public Const _OPT_KEYPADS_AVAIL As String = "KeypadsAvailable"          ' D6429

        Public Const _OPT_SHOW_RELEASE_NOTES As Boolean = False                 ' D6417

        ' D1020 ===
        Public _OPT_LDAP_QUERY As String = "LDAP_Query"
        Public _OPT_LDAP_USERNAME As String = "LDAP_Username"
        Public _OPT_LDAP_PASSWORD As String = "LDAP_Password"
        Public _OPT_LDAP_SEARCH As String = "LDAP_SearchQuery"  ' D1077
        'Public _OPT_LDAP_PREFIX As String = "LDAP_DomainPrefix"
        ' D1020 ==

        Public Const _OPT_GOOGLE_UID As String = "Google-UA"    ' D6746
        Public Const _DEF_GOOGLE_UA As String = "UA-386731-1"   ' D6745

        Public Const _OPT_ALLOW_IGNORE_EULA As Boolean = False      ' D6262

        Public Const _OPT_REVIEW_ACCOUNT As String = "TrialConsultant"  ' D1381
        Public Const _OPT_HIDEMASTERHEADER As String = "HideMasterHeader"   ' D1534 // change case
        Public Const _OPT_CUSTOM_FILE_ROOT As String = "CustomFileRoot"     ' D1534

        Public Const _OPT_EVAL_SITE As String = "EvalSite"          ' D3308
        'Public Const _OPT_EVAL_TT_SITE As String = "EvalTTSite"     ' D3827
        'Public Const _OPT_EVAL_TEAMTIME As String = "EvalTeamTime"  ' D6016
        'Public Const _OPT_EVAL_ANYTIME As String = "EvalAnytime"    ' D6016
        Public Const _OPT_MOBILE_EVAL_SITE As String = "ShowEvalSite4Mobile"    ' D4142
        Public Const _OPT_EVAL_SITE_ONLY As String = "ShowEvalSiteOnly" ' D4773

        Public Const _OPT_EVAL_T2S_ALLOWED As Boolean = True                ' D6991
        Public Const _OPT_MY_RISK_REWARD_ALLOWED As Boolean = False         ' D7067

        Public Const _OPT_SUPPORT_EMAIL As String = "SupportEmail"         ' D2157

        Public Const _OPT_LIC_PSW_HASH As String = "LicensePswHash"         ' D3036

        Public Const _OPT_LOCK_PSW_ATTEMPTS As String = "LockPswAttempts"   ' D3521
        Public Const _OPT_LOCK_PSW_PERIOD As String = "LockPswPeriod"       ' D6071
        Public Const _OPT_LOCK_PSW_TIMEOUT As String = "LockPswTimeout"     ' D6071
        Public Const _OPT_UNLOCK_PSW_AUTO As String = "AutoUnLockPsw"       ' D6074

        Public Const _OPT_ASK_PSW_WHEN_NEW_USR As String = "AskPswNewUsers"      ' D7147 // ask new user to setup a psw when came by invitation link to AT/TT evaluation

        Public Const _OPT_FEDRAMP_MODE As String = "FedRAMP"                ' D6074
        Public Const _OPT_SSO_MODE As String = "UseSSO"                     ' D6532
        Public Const _OPT_SSO_ONLY As String = "SSO_Only"                   ' D6550
        Public Const _OPT_SSO_DEF_WKG As String = "SSO_DefWkg"              ' D7412
        Public Const _OPT_SSO_DEF_ROLE As String = "SSO_DefRole"            ' D7412
        Public Const _OPT_SHOW_LINKS_WHEN_SSO_ONLY As Boolean = False       ' D7420

        Public Const _OPT_ALLOW_ADMINS_LOCAL_ONLY As String = "AdminLocalOnly"      ' D6640
        Public Const _OPT_LOGOUT_AFTER_TIMEOUT As String = "LogoutAfterTimeout"     ' D6642

        'Public Const _OPT_NEWRELIC_ENABLED As String = "NewRelic.agentEnabled"  ' D3827 -D4538
        'Public Const _OPT_NEWRELIC_APPNAME As String = "NewRelic.AppName"       ' D3827 -D4538

        Public Const _OPT_SNAPSHOT_ON_RTE As String = "SnapshotOnRTE"       ' D4175

        Public Const _OPT_ALLOW_BLANK_PSW As String = "AllowBlankPassword"  ' D4494

        Public Const _OPT_IGNORE_NEW_MASTERPRJ As String = "IgnoreNewMasterPrj" ' D6398

        Public Const _OPT_FIPS_MODE As String = "FIPSmode"              ' D6689

        Public Const _OPT_MFA_EMAIL_ALLOWED As String = "MFA_Email"         ' D7501
        Public Const _OPT_PINCODE_ALLOWED As String = "PINAllowed"          ' D7249

        ' D6276 ===
        Public Const _OPT_PSW_HIGH_COMPLEXITY As String = "PswHighComplexity"
        Public Const _OPT_PSW_MIN_LEN As String = "PswMinLen"
        Public Const _OPT_PSW_MAX_LEN As String = "PswMaxLen"           ' D6646
        Public Const _OPT_PSW_MIN_CHANGES As String = "PswMinChanges"   ' D6646
        Public Const _OPT_PSW_MIN_LIFETIME As String = "PswMinLifetime" ' D6646
        Public Const _OPT_PSW_MAX_LIFETIME As String = "PswMaxLifetime" ' D6646
        Public Const _OPT_PSW_PREV_HASHES As String = "PswPrevHashes"
        ' D6276 ==

        Public Const _OPT_PAGINATION_LIMIT As Integer = 100 'A2009
        Public Const _OPT_PAGINATION_PAGE_SIZES As String = "[10,15,20,25,50,100,500,1000]" 'A2009        

        Private option_SystemEmail As String = Nothing          ' D0415 + D1152 + D2012

        Private option_SessionTimeout As Integer = -1           ' D0644

        Private _ShowDraftPages As Boolean = False              ' D3965

#End Region

        ' D3821 ===
        Public Sub OptionsIndividualInit(App As clsComparionCore)
            _Options_Individual = Nothing

            'If _OPT_USE_CUSTOM_SETTINGS Then

            Dim _Options_tmp As New List(Of clsSetting)
            ' D3827 ===
            _Options_tmp.Add(New clsSetting(_OPT_FIPS_MODE, "optFIPSMode", GetType(Boolean), False))                ' D6689
            _Options_tmp.Add(New clsSetting(_OPT_DISABLEAUTOCOMPLETEFORLOGIN, "optDisableAutoComplete", GetType(Boolean), False))
            '_Options_tmp.Add(New clsSetting(_OPT_SLSHELL_BY_DEFAULT, "optSLShellByDef", GetType(Boolean), False))
            _Options_tmp.Add(New clsSetting(_OPT_LOCK_PSW_ATTEMPTS, "optLockPsw", GetType(Integer), False, _DEF_PASSWORD_ATTEMPTS.ToString))
            _Options_tmp.Add(New clsSetting(_OPT_UNLOCK_PSW_AUTO, "optLockPswTemporary", GetType(Boolean), False))  ' D6074
            _Options_tmp.Add(New clsSetting(_OPT_LOCK_PSW_PERIOD, "optLockPswPeriod", GetType(Integer), False))     ' D6072
            _Options_tmp.Add(New clsSetting(_OPT_LOCK_PSW_TIMEOUT, "optLockPswTimeout", GetType(Integer), False))   ' D6072
            _Options_tmp.Add(New clsSetting(_OPT_ALLOW_BLANK_PSW, "optAllowBlankPsw", GetType(Boolean), False)) ' D4494
            ' D6276 ===
            _Options_tmp.Add(New clsSetting(_OPT_PSW_HIGH_COMPLEXITY, "optPswHighComplexity", GetType(Boolean), False, "0"))
            _Options_tmp.Add(New clsSetting(_OPT_PSW_MIN_LEN, "optPswMinLen", GetType(Integer), False, "0"))
            _Options_tmp.Add(New clsSetting(_OPT_PSW_MAX_LEN, "optPswMaxLen", GetType(Integer), False, "0"))
            _Options_tmp.Add(New clsSetting(_OPT_PSW_MIN_CHANGES, "optPswMinChanges", GetType(Integer), False, "0"))
            '_Options_tmp.Add(New clsSetting(_OPT_PSW_MIN_LIFETIME, "optPswMinLifetime", GetType(Integer), False, "0")) ' D6657
            _Options_tmp.Add(New clsSetting(_OPT_PSW_MAX_LIFETIME, "optPswMaxLifetime", GetType(Integer), False, "0"))
            _Options_tmp.Add(New clsSetting(_OPT_PSW_PREV_HASHES, "optPswPrevHashes", GetType(Integer), False, "0"))
            ' D6276 ==
            _Options_tmp.Add(New clsSetting(_OPT_ASK_PSW_WHEN_NEW_USR, "optAskPswNewUser", GetType(Boolean), False))      ' D7147
            _Options_tmp.Add(New clsSetting(_OPT_MFA_EMAIL_ALLOWED, "optMFAEmailAllowed", GetType(Boolean), False))       ' D7501
            _Options_tmp.Add(New clsSetting(_OPT_PINCODE_ALLOWED, "optPINCodeAllowed", GetType(Boolean), False))          ' D7249
            _Options_tmp.Add(New clsSetting(_OPT_CHECK_EULA, "optCheckEULA", GetType(Boolean), False))
            _Options_tmp.Add(New clsSetting(_OPT_FEDRAMP_MODE, "optFedRAMP", GetType(Boolean), False))      ' D6074
            _Options_tmp.Add(New clsSetting(_OPT_SSO_MODE, "optSSO", GetType(Boolean), False))              ' D6532
            _Options_tmp.Add(New clsSetting(_OPT_SSO_ONLY, "optSSO_Only", GetType(Boolean), False))              ' D6532
            _Options_tmp.Add(New clsSetting(_OPT_SSO_DEF_WKG, "optSSO_Wkg", GetType(String), False))             ' D7413
            _Options_tmp.Add(New clsSetting(_OPT_SSO_DEF_ROLE, "optSSO_Role", GetType(String), False))           ' D7413
            _Options_tmp.Add(New clsSetting(_OPT_ALLOW_ADMINS_LOCAL_ONLY, "optAdminsLocalOnly", GetType(Boolean), False))           ' D6640
            _Options_tmp.Add(New clsSetting(_OPT_LOGOUT_AFTER_TIMEOUT, "optLogoutAfterTimeout", GetType(Boolean), False))           ' D6642
            If Not App.isSelfHost Then _Options_tmp.Add(New clsSetting(_OPT_SHOW_DRAFT, "optShowDraft", GetType(Boolean), False))   ' D3965
            _Options_tmp.Add(New clsSetting(_OPT_FORCESSL, "optForceSSL", GetType(Boolean), False))
            _Options_tmp.Add(New clsSetting(_OPT_SMTPSSL, "optSMTPSSL", GetType(Boolean), False))
            _Options_tmp.Add(New clsSetting(_OPT_TRACE_ENABLED, "optTraceEnabled", GetType(Boolean), False))
            _Options_tmp.Add(New clsSetting(_OPT_HOOK_ERRORS, "optHookErrors", GetType(Boolean), False))
            _Options_tmp.Add(New clsSetting(_OPT_SHOW_CHAT_SUPPORT, "optShowChatSupport", GetType(Boolean), False))  ' D4220 + D4238
            _Options_tmp.Add(New clsSetting(_OPT_FOGBUGZ_AUTOSUBMIT, "optFBAutoSubmit", GetType(Boolean), False))
            _Options_tmp.Add(New clsSetting(_OPT_FOGBUGZ_INBOX, "optFBInbox", GetType(String), False))
            '_Options_tmp.Add(New clsSetting(_OPT_NEWRELIC_ENABLED, "optNewRelicEnabled", GetType(Boolean), False))     ' -D4538
            '_Options_tmp.Add(New clsSetting(_OPT_NEWRELIC_APPNAME, "optNewRelicAppName", GetType(String), False))      ' -D4538

            _Options_tmp.Add(New clsSetting(_OPT_KEYPADS_AVAIL, "optKeypadsAvail", GetType(Boolean), False))            ' D6429
            _Options_tmp.Add(New clsSetting(_OPT_SYNCHRONOUS_REFRESH, "optSyncRefresh", GetType(Integer), False))
            _Options_tmp.Add(New clsSetting(_OPT_LOCKTIMEOUT, "optLockTimeout", GetType(Integer), False))

            _Options_tmp.Add(New clsSetting(_OPT_EVAL_SITE, "optEvalSite", GetType(String), False))
            '_Options_tmp.Add(New clsSetting(_OPT_EVAL_TEAMTIME, "optEvalSiteTT", GetType(Boolean), False))
            '_Options_tmp.Add(New clsSetting(_OPT_EVAL_ANYTIME, "optEvalSiteAT", GetType(Boolean), False))
            _Options_tmp.Add(New clsSetting(_OPT_MOBILE_EVAL_SITE, "optMobileEvalSite", GetType(Boolean), False))   ' D4142
            'If _OPT_ALLOW_REVIEW_ACCOUNT Then _Options_tmp.Add(New clsSetting(_OPT_EVAL_SITE_ONLY, "optEvalSiteOnly", GetType(Boolean), False))       ' D4773 + D5068
            _Options_tmp.Add(New clsSetting(_OPT_EVAL_SITE_ONLY, "optEvalSiteOnly", GetType(Boolean), False))       ' D4773 + D5068

            '_Options_tmp.Add(New clsSetting(_OPT_REVIEW_ACCOUNT, "optReviewAccount", GetType(String), False))      ' -D7019
            _Options_tmp.Add(New clsSetting(_OPT_DEFLANG, "optDefLanguage", GetType(String), False))

            _Options_tmp.Add(New clsSetting(_OPT_USE_DATAMAPPING, "optUseDataMapping", GetType(Boolean), False))   ' D4465
            '_Options_tmp.Add(New clsSetting(_OPT_LIC_PSW_HASH, "optLicPswHash", GetType(String), False))   -D3949
            '_Options_tmp.Add(New clsSetting(_OPT_BACKDOOR, "optBackdoor", GetType(String), False))
            _Options_tmp.Add(New clsSetting(_OPT_IGNORE_NEW_MASTERPRJ, "optNoUpdateMasterProj", GetType(Boolean), False))  ' D6398

            '_Options_WebConfig.Add(New clsSetting(_OPT_CWSW_URL, "optCWSwURL", GetType(String), False))
            '_Options_WebConfig.Add(New clsSetting(_OPT_WCF_CORE_URL, "optCoreServiceURL", GetType(String), False))
            '_Options_WebConfig.Add(New clsSetting(_OPT_ANTIGUA_SERVICESRV_URL, "optAntiguaURL", GetType(String), False))
            '_Options_WebConfig.Add(New clsSetting(_OPT_ANTIGUA_ADMINSRV_URL, "optAntiguaAdminURL", GetType(String), False))
            ' D3827 ==

            _Options_tmp.Add(New clsSetting(_OPT_GOOGLE_UID, "optGoogleUA", GetType(String), False, _DEF_GOOGLE_UA))    ' D6745

            '_Options_tmp.Sort(New clsSettingComparer())    ' -D3827
            _Options_Individual = _Options_tmp

            If App IsNot Nothing AndAlso App.Database IsNot Nothing AndAlso App.isCanvasMasterDBValid Then  ' D7199
                Dim tExtras As List(Of clsExtra) = App.DBExtrasByDetails(, ecExtraType.Common, ecExtraProperty.WebConfigSetting)
                If tExtras IsNot Nothing Then
                    For Each tExtra As clsExtra In tExtras
                        If tExtra.Value IsNot Nothing Then
                            Dim tOpt As clsSetting = clsSetting.SettingByID(_Options_Individual, tExtra.ObjectID)
                            If tOpt IsNot Nothing Then tOpt.Value = CStr(tExtra.Value)
                        End If
                    Next
                End If
            End If

            'End If
        End Sub
        ' D3821 ==

        ' D0315 + D0329 ===
        Private Function GetTimeout(ByVal _OPT_NAME As String, ByVal DefValue As Integer, ByVal Minvalue As Integer) As Integer
            Dim seconds As String = WebConfigOption(_OPT_NAME, DefValue.ToString)
            Dim timeout As Integer = 0
            Integer.TryParse(seconds, timeout)
            If timeout < Minvalue Then timeout = Minvalue
            Return timeout
        End Function

        ' D0462 ===
        Public Sub LoadComparionCoreOptions(ByRef _App As clsComparionCore)

            ' D1534 ===
            InitPaths(HostingEnvironment.ApplicationPhysicalPath)

            Dim sCustomRoot As String = WebConfigOption(_OPT_CUSTOM_FILE_ROOT, "", True)
            If sCustomRoot <> "" Then
                Try
                    If Not sCustomRoot.Contains(":") Then sCustomRoot = My.Computer.FileSystem.CombinePath(_FILE_ROOT, sCustomRoot)
                    If sCustomRoot <> "" AndAlso Not sCustomRoot.EndsWith("\") Then sCustomRoot += "\"
                    If Not My.Computer.FileSystem.DirectoryExists(sCustomRoot) OrElse Not My.Computer.FileSystem.DirectoryExists(sCustomRoot + "App_Data") Then sCustomRoot = ""
                Catch ex As Exception
                    sCustomRoot = ""
                End Try
                If sCustomRoot <> "" Then InitPaths(sCustomRoot)
            End If
            ' D1534 ==

            If _App.Options Is Nothing Then _App.Options = New clsComparionCoreOptions ' D0465


            _App.Options.CanvasMasterDBName = WebConfigOption(WebOptions._OPT_CANVASMASTERDB, _DB_CANVAS_MASTERDB, True)
            ' D0415 ===
            _App.Options.CanvasProjectsDBName = WebConfigOption(WebOptions._OPT_CANVASPROJECTSDB, _App.Options.CanvasMasterDBName, True)  ' D0354
            '_App.Options.SpyronMasterDBName = WebConfigOption(WebOptions._OPT_SPYRONMASTERDB, _App.Options.CanvasMasterDBName, True)       ' -D6423
            '_App.Options.SpyronProjectsDBName = WebConfigOption(WebOptions._OPT_SPYRONPROJECTSDB, _App.Options.CanvasMasterDBName, True) ' D0375 -D6423
            ' D0415 ==
            GlobalDefaultProvider = WebConfigOption(WebOptions._OPT_DEFAULT_PROVIDER, "SqlServer", True)  ' D0330 + D0456

            _OPT_USE_CUSTOM_SETTINGS = WebConfigOption(WebOptions._OPT_OPTIONS_OVERRIDE, "", True) = "1"    ' D3821
            OptionsIndividualInit(_App)    ' D3821

            ECSecurity.ECSecurity.FIPS_MODE = Str2Bool(WebConfigOption(WebOptions._OPT_FIPS_MODE, Bool2Num(ECSecurity.ECSecurity.FIPS_MODE).ToString, True))    ' D6689
            If ECSecurity.ECSecurity.FIPS_MODE Then _App.SystemWorkgroup = Nothing  ' D6689

            _ShowDraftPages = WebConfigOption(_OPT_SHOW_DRAFT, "0", True) <> "0" AndAlso Not _App.isSelfHost    ' D3965
            _App.LicenseOption_ShowDraft = _ShowDraftPages

            _App.Options.CheckEULA = WebConfigOption(WebOptions._OPT_CHECK_EULA, "1", True) <> "0"
            _App.LanguageCode = WebConfigOption(WebOptions._OPT_DEFLANG, _LANG_DEFCODE, True)

            _App.Options.CheckLicense = Not _LICENSES_ALLOW_TO_DISABLE Or WebConfigOption(WebOptions._OPT_LICENSE_MODE, "Default", True).ToLower <> "off"

            _App.Options.ProjectForceAllowedAlts = WebConfigOption(WebOptions._OPT_FORCEALLOWEDALTS, "0", True) <> "0"
            _App.Options.ProjectLoadOnDemand = WebConfigOption(WebOptions._OPT_LOADONDEMAND, "0", True) <> "0"
            _App.Options.ProjectLockTimeout = GetTimeout(WebOptions._OPT_LOCKTIMEOUT, _DEF_LOCK_TIMEOUT, 10) ' D0851
            _App.Options.ProjectUseDataMapping = WebConfigOption(WebOptions._OPT_USE_DATAMAPPING, "0", True) <> "0"     ' D4465

            _App.Options.BackDoor = WebConfigOption(WebOptions._OPT_BACKDOOR, "")    ' D0338
            '_App.Options.ShowSilverlightShellOnLogon = WebConfigOption(WebOptions._OPT_SLSHELL_BY_DEFAULT, "1", True) <> "0"    ' D0729

            _App.Options.DefParamSet = WebConfigOption(_OPT_DEFAULTPIPEPARAMSETS, "", True)          ' D2256
            _App.Options.DefParamSetRisk = WebConfigOption(_OPT_DEFAULTPIPEPARAMSETS_RISK, "", True) ' D2256

            _App.Options.KeypadsAvailable = Str2Bool(WebConfigOption(_OPT_KEYPADS_AVAIL, "1", True))    ' D6429

            _App.Options.EvalSiteURL = WebConfigOption(WebOptions._OPT_EVAL_SITE, "")    ' D3308

            _App.Options.EvalSiteURL = WebConfigOption(WebOptions._OPT_EVAL_SITE, "")    ' D3308
            _App.Options.EvalSiteOnly = WebConfigOption(WebOptions._OPT_EVAL_SITE_ONLY, "0") <> "0" ' D6359
            ' D3521 ===
            Dim sPswLock As String = WebConfigOption(WebOptions._OPT_LOCK_PSW_ATTEMPTS, _DEF_PASSWORD_ATTEMPTS.ToString)
            If Integer.TryParse(sPswLock, _DEF_PASSWORD_ATTEMPTS) Then
                If _DEF_PASSWORD_ATTEMPTS <= 1 Then _DEF_PASSWORD_ATTEMPTS = Integer.MaxValue
            End If
            ' D3521 ==
            ' D6072 ===
            Dim sCnt As String = WebConfigOption(WebOptions._OPT_LOCK_PSW_PERIOD, _DEF_PASSWORD_ATTEMPTS_PERIOD.ToString)
            If Integer.TryParse(sCnt, _DEF_PASSWORD_ATTEMPTS_PERIOD) Then
                If _DEF_PASSWORD_ATTEMPTS_PERIOD <= 1 Then _DEF_PASSWORD_ATTEMPTS_PERIOD = 60 * 24 * 365   ' max is a 1 year in minutes
            End If
            sCnt = WebConfigOption(WebOptions._OPT_LOCK_PSW_TIMEOUT, _DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT.ToString)
            If Integer.TryParse(sCnt, _DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT) Then
                If _DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT < 1 Then _DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT = 1    ' lock for at least 1 minute
            End If
            ' D6072 ==

            ' D6646 ===
            _DEF_PASSWORD_COMPLEXITY = Str2Bool(WebConfigOption(WebOptions._OPT_PSW_HIGH_COMPLEXITY, Bool2Num(_DEF_PASSWORD_COMPLEXITY).ToString, False))
            Integer.TryParse(WebConfigOption(WebOptions._OPT_PSW_MIN_LEN, _DEF_PASSWORD_MIN_LENGTH.ToString, False), _DEF_PASSWORD_MIN_LENGTH)
            If Integer.TryParse(WebConfigOption(WebOptions._OPT_PSW_MAX_LEN, _DEF_PASSWORD_MAX_LENGTH.ToString, False), _DEF_PASSWORD_MAX_LENGTH) Then
                If _DEF_PASSWORD_MAX_LENGTH > 0 AndAlso _DEF_PASSWORD_MIN_LENGTH > 0 AndAlso _DEF_PASSWORD_MAX_LENGTH < _DEF_PASSWORD_MIN_LENGTH Then _DEF_PASSWORD_MAX_LENGTH = _DEF_PASSWORD_MIN_LENGTH
            End If
            'Integer.TryParse(WebConfigOption(WebOptions._OPT_PSW_MIN_LIFETIME, _DEF_PASSWORD_MIN_LIFETIME.ToString, False), _DEF_PASSWORD_MIN_LIFETIME)    ' -D6657
            If Integer.TryParse(WebConfigOption(WebOptions._OPT_PSW_MAX_LIFETIME, _DEF_PASSWORD_MAX_LIFETIME.ToString, False), _DEF_PASSWORD_MAX_LIFETIME) Then
                'If _DEF_PASSWORD_MAX_LIFETIME > 0 AndAlso _DEF_PASSWORD_MIN_LIFETIME > 0 AndAlso _DEF_PASSWORD_MAX_LIFETIME < _DEF_PASSWORD_MIN_LIFETIME Then _DEF_PASSWORD_MAX_LIFETIME = _DEF_PASSWORD_MIN_LIFETIME  ' -D6657
            End If
            Integer.TryParse(WebConfigOption(WebOptions._OPT_PSW_PREV_HASHES, _DEF_PASSWORD_KEEP_HASHES.ToString, False), _DEF_PASSWORD_KEEP_HASHES)
            If _DEF_PASSWORD_KEEP_HASHES > _DEF_PASSWORD_HASHES_MAX Then _DEF_PASSWORD_KEEP_HASHES = _DEF_PASSWORD_HASHES_MAX     ' D6659
            Integer.TryParse(WebConfigOption(WebOptions._OPT_PSW_MIN_CHANGES, _DEF_PASSWORD_MIN_CHANGES.ToString, False), _DEF_PASSWORD_MIN_CHANGES)    ' D6658
            ' D6646 ==

            _DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK = WebConfigOption(WebOptions._OPT_UNLOCK_PSW_AUTO, "1", True) = "1"   ' D6074
            _DEF_ASK_PSW_NEW_USER_BY_INVITES = WebConfigOption(WebOptions._OPT_ASK_PSW_WHEN_NEW_USR, "1", True) = "1"   ' D7147

            _FEDRAMP_MODE = WebConfigOption(WebOptions._OPT_FEDRAMP_MODE, "", True) = "1"   ' D6074
            _SSO_MODE = WebConfigOption(WebOptions._OPT_SSO_MODE, "", True) = "1"           ' D6532
            _SSO_ONLY = WebConfigOption(WebOptions._OPT_SSO_ONLY, "", True) = "1"           ' D6550
            _ADMINS_LOCAL_ONLY = WebConfigOption(WebOptions._OPT_ALLOW_ADMINS_LOCAL_ONLY, "", True) = "1"   ' D6640
            _LOGOUT_AFTER_TIMEOUT = WebConfigOption(WebOptions._OPT_LOGOUT_AFTER_TIMEOUT, "", True) = "1"   ' D6642

            _MFA_REQUIRED = WebConfigOption(WebOptions._OPT_MFA_EMAIL_ALLOWED, "", True) = "1"   ' D7501 + D7502
            _PINCODE_ALLOWED = WebConfigOption(WebOptions._OPT_PINCODE_ALLOWED, "", True) = "1"     ' D7249

            ' D3940 ===
            Dim sAppName As String = _App.ResString(CStr(IIf(_App.isRiskEnabled, "titleApplicationNameRiskPlain", "titleApplicationNamePlain"))).Trim.Replace(Chr(169), "").Replace(Chr(174), "").Replace(Chr(153), "") ' // ™ (c), (R), (tm)
            option_SystemEmail = clsComparionCorePage.ParseTemplateCommon(SystemEmail, _TEMPL_APPNAME, sAppName)    ' D3858 + D3886 // ™ (c), (R), (tm)
            option_SystemEmail = option_SystemEmail.Replace("APPNAME", sAppName)    ' since we can't use %% on FinalBuilder
            ' D3940 ==

            'If _App.Options.ShowSilverlightShellOnLogon Then _App.Options.RestoreLastVisitedProject = False ' D0733 -D0736

            DebugInfo("Application options was loaded from web.config")
        End Sub
        ' D0462 ==

#Region "Getting Options from Web.config"

        ' D0211 ===
        ''' <summary>
        ''' Show languages selector.
        ''' </summary>
        ''' <returns>True, when select languages available</returns>
        ''' <remarks>True by default</remarks>
        Public Function ShowLanguages() As Boolean
            Return ShowDraftPages() AndAlso WebConfigOption(_OPT_SHOW_LANGS, "1", True) <> "0"  ' D4581
        End Function
        ' D0211 ==

        ' D0035 + D4061 ===
        ''' <summary>
        ''' Use for check ["Upgrade-Insecure-Requests"] header request from browser for switch to SSL connection
        ''' </summary>
        ''' <returns>True by default</returns>
        ''' <remarks></remarks>
        Public Function ForceSSL(tReq As HttpRequest) As Boolean
            Dim fCheckSSL As Boolean = WebConfigOption(_OPT_FORCESSL, "0", True) <> "0"
            ' -D4062
            'If tReq IsNot Nothing Then
            '    fCheckSSL = Not String.IsNullOrEmpty(tReq.Headers("Upgrade-Insecure-Requests")) AndAlso tReq.Headers("Upgrade-Insecure-Requests") = "1"
            'End If
            Return fCheckSSL
        End Function
        ' D0035 + D4061 ==

        ' D0758 ===
        ''' <summary>
        ''' Use SSL connection for SMTP
        ''' </summary>
        ''' <returns>False by default</returns>
        ''' <remarks></remarks>
        Public Function SMTPSSL() As Boolean
            Return WebConfigOption(_OPT_SMTPSSL, "0", True) <> "0"
        End Function
        ' D0758 ==

        ' D2157 ===
        Public Function SupportEmail() As String
            'Return SystemEmail  ' D3827
            Return WebConfigOption(_OPT_SUPPORT_EMAIL, "support@expertchoice.com", True) ' D7197
        End Function
        ' D2157 ==

        ' D0415 ===
        ''' <summary>
        ''' System e-mail (was Customer Service email)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Used in warning and error messages, on start page</remarks>
        Public ReadOnly Property SystemEmail() As String ' D1152
            Get
                If option_SystemEmail Is Nothing Then    ' D0152 + D2012
                    Try ' D2012
                        Dim Config As System.Configuration.Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/")
                        If Not Config Is Nothing Then
                            Dim sect As System.Configuration.ConfigurationSectionGroup = Config.GetSectionGroup("system.net/mailSettings")
                            If sect IsNot Nothing Then option_SystemEmail = CType(sect, System.Net.Configuration.MailSettingsSectionGroup).Smtp.From    ' D1152
                            If String.IsNullOrEmpty(option_SystemEmail) Then option_SystemEmail = SupportEmail()
                        End If
                    Catch ex As Exception
                        option_SystemEmail = SupportEmail()  ' D2012 + D3858
                    End Try
                End If
                Return option_SystemEmail   ' D1152
            End Get
        End Property
        ' D0415 ==

        ' D0644 ===
        Public ReadOnly Property SessionTimeout() As Integer
            Get
                If option_SessionTimeout <= 0 Then
                    Try ' D2012
                        Dim Config As System.Configuration.Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/")
                        If Not Config Is Nothing Then
                            Dim sect As System.Web.Configuration.SessionStateSection = CType(Config.GetSection("system.web/sessionState"), System.Web.Configuration.SessionStateSection)
                            If Not sect Is Nothing Then option_SessionTimeout = CInt(sect.Timeout.TotalSeconds)
                        End If
                        'If option_SessionTimeout < 300 Then option_SessionTimeout = 300
                    Catch ex As Exception
                        option_SessionTimeout = 20 * 60 ' D2012
                    End Try
                End If
                Return option_SessionTimeout
            End Get
        End Property
        ' D0644 ==

        ' D0194 ===
        Public Function SynchronousRefresh() As Integer
            Dim seconds As String = WebConfigOption(_OPT_SYNCHRONOUS_REFRESH)
            Dim Refresh As Integer = _DEF_SYNCH_REFRESH
            Integer.TryParse(seconds, Refresh)
            If Refresh < 1 Then Refresh = _DEF_SYNCH_REFRESH ' D0771
            Return Refresh
        End Function
        ' D0194 ==

        ' -D2622
        '' D0450 ===
        'Public Function SynchronousMaxUsers() As Integer
        '    Dim sUsers As String = WebConfigOption(_OPT_SYNCHRONOUS_MAXUSERS)
        '    Dim Users As Integer = -1
        '    Integer.TryParse(sUsers, Users)
        '    If Users < 2 AndAlso Users <> -1 Then Users = -1
        '    Return Users
        'End Function
        '' D0450 ==

        '' D0197 ===
        '''' <summary>
        '''' Show and allow to view unfinished screens
        '''' </summary>
        '''' <returns></returns>
        '''' <remarks>By default is disabled</remarks>
        Public Function ShowDraftPages() As Boolean ' D0315
            Return _ShowDraftPages  ' D3965
        End Function
        '' D0197 ==

        ' D4059 ===
        Private Function GetServicePostfix(sURL As String, tReq As HttpRequest) As String
            If tReq IsNot Nothing AndAlso isSecureConnection(tReq) Then ' D4061
                If sURL.ToLower.StartsWith("http:") Then sURL = sURL.Insert(4, "s")
                sURL = sURL.TrimEnd(CChar("/")) + "/ssl"
            End If
            Return sURL
        End Function
        ' D4059 ==

        ' D0556 ===
        Private mHideMasterHeader As String = Nothing
        Public ReadOnly Property HideMasterHeader As Boolean
            Get
                If mHideMasterHeader Is Nothing Then
                    mHideMasterHeader = WebConfigOption(_OPT_HIDEMASTERHEADER, "False", True)
                End If
                Return CBool(mHideMasterHeader)
            End Get
        End Property

        ' D0560 ===
        Public Function HookErrors() As Boolean
            Return WebConfigOption(_OPT_HOOK_ERRORS, "1", True) <> "0"
        End Function
        ' D0560 ==

        ' D1020 ===
        Public Function LDAPQuery() As String
            Return WebConfigOption(_OPT_LDAP_QUERY, "", True).Trim
        End Function

        Public Function LDAPUserName() As String
            Return WebConfigOption(_OPT_LDAP_USERNAME, "", True).Trim
        End Function

        'Public Function LDAPUserPrefix() As String
        '    Return WebConfigOption(_OPT_LDAP_PREFIX, "", True).Trim
        'End Function

        Public Function LDAPUserPassword() As String
            Return WebConfigOption(_OPT_LDAP_PASSWORD, "", True)
        End Function
        ' D1020 ==

        ' D1077 ===
        Public Function LDAPSearchQuery() As String
            Return WebConfigOption(_OPT_LDAP_SEARCH, "(&(objectCategory=Person)(objectClass=user)(mail=*)(|(mail={0})(displayname={0}))(!userAccountControl:1.2.840.113556.1.4.803:=2))", True).Trim
        End Function
        ' D1077 ==

        ' D1381 ===
        Public Function ReviewAccount() As String
            Return If(_OPT_ALLOW_REVIEW_ACCOUNT, WebConfigOption(_OPT_REVIEW_ACCOUNT, "", True), "")    ' D5068
        End Function
        ' D1381 ==

        ' D4220 ===
        Public Function ShowChatSupport(ShowByDefault As Boolean) As Boolean
            Return WebConfigOption(_OPT_SHOW_CHAT_SUPPORT, CStr(IIf(ShowByDefault, "1", "0")), True) <> "0"
        End Function
        ' D4220 ==

        ' D4494 ===
        Public Function AllowBlankPsw() As Boolean
            Return Not _DEF_PASSWORD_COMPLEXITY AndAlso WebConfigOption(_OPT_ALLOW_BLANK_PSW, "1", True) <> "0" ' D6646 + D7133
        End Function
        ' D4494 ==

#End Region

    End Module

End Namespace
