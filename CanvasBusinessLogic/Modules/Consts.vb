Imports System.Web.Hosting
Imports ExpertChoice.Service.FileService

Namespace ExpertChoice.Data

    Public Module Consts

        Public Enum ecDBType    ' D0375 + D0459
            dbCanvasMaster = 1
            dbCanvasProjects = 2
            'dbSpyronMaster = 3       ' -D6423
            'dbSpyronProjects = 4     ' D0375 -D6423
        End Enum

        ''' <summary>
        ''' Type of Object for edit Richtext content
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum reObjectType
            Unspecified = 0
            Node = 1
            Alternative = 2
            AltWRTNode = 3      ' D1003 + D1004
            PipeMessage = 5
            AntiguaInfodoc = 6  ' D0978
            Description = 7     ' D2064
            ExtraInfo = 8       ' D0299
            SurveyQuestion = 9  ' D1277
            MeasureScale = 10   ' D2249
            QuickHelp = 11      ' D3693
            ResultGroup = 12    ' D4283
            Attribute = 14      ' D4283
            AttributeValue = 15 ' D4283
            Control = 16        ' D4344
            Uploads = 17        ' D6461
            DashboardInfodoc = 18   ' D7011
        End Enum


#Region "Common params"

        ''' <summary>
        ''' Default timeout for project lock (in seconds). By default is 600 (10 minutes).
        ''' </summary>
        ''' <remarks>This value could be overloaded via web.config option "LockTimeout".</remarks>
        Public Const _DEF_LOCK_TIMEOUT As Integer = 10 * 60   ' D0135 // in seconds

        Public Const _DEF_LOCK_TT_SESSION_TIMEOUT As Integer = 8 * 60 * 60  ' D1074 (8 hours for started meeting)
        Public Const _DEF_LOCK_TT_JUDGMENT_TIMEOUT As Integer = 2 * 60 * 60 ' D1074 (2 hours since last judgment in TT)
        Public Const _DEF_LOCK_TT_CONSENSUS_VIEW As Integer = 3 * 60        ' D6553 ( 3 minutes for started from mCV)

        Public _DEF_PASSWORD_ATTEMPTS As Integer = 3  ' D2213 + D3521
        Public _DEF_PASSWORD_ATTEMPTS_PERIOD As Integer = 15            ' //minutes D6062
        Public _DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT As Integer = 30      ' //minutes D6062
        Public _DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK As Boolean = True      ' D6074

        ' D6646 ===
        Public _DEF_PASSWORD_COMPLEXITY As Boolean = False
        Public _DEF_PASSWORD_MIN_LENGTH As Integer = 0
        Public _DEF_PASSWORD_MAX_LENGTH As Integer = 0
        Public _DEF_PASSWORD_MIN_CHANGES As Integer = 0
        'Public _DEF_PASSWORD_MIN_LIFETIME As Integer = 0   ' -D6657
        Public _DEF_PASSWORD_MAX_LIFETIME As Integer = 0
        Public _DEF_PASSWORD_KEEP_HASHES As Integer = 0
        Public Const _DEF_PASSWORD_HASHES_MAX As Integer = 35   ' D6659
        ' D6646 ==

        Public _FEDRAMP_MODE As Boolean = False ' D6074
        Public _SSO_MODE As Boolean = False     ' D6532
        Public _SSO_ONLY As Boolean = True      ' D6532
        Public _ADMINS_LOCAL_ONLY As Boolean = False    ' D6640
        Public _LOGOUT_AFTER_TIMEOUT As Boolean = False ' D6642

        Public _DEF_ASK_PSW_NEW_USER_BY_INVITES As Boolean = True   ' D7147

        Public Const _DEF_PASSWORD_LINK_TIMEOUT As Integer = 60 * 60 * 72  ' D2798 + D4437 // 72 hours

        Public _PINCODE_ALLOWED As Boolean = False              ' D7249
        Public Const _DEF_PINCODE_TIMEOUT As Integer = 5 * 60   ' seconds
        Public Const _OPT_PREFIX_PIN = "#"                      ' D7501
        Public Const _DEF_ALEXA_PRJID_FLAG As Integer = -2      ' D7250

        Public _MFA_REQUIRED As Boolean = False                 ' D7502
        Public _MFA_CODE_RESEND_TIMEOUT As Integer = 60         ' D7502

        Public _MFA_EMAIL_ALLOWED As Boolean = True             ' D7501
        Public Const _DEF_MFA_EMAIL_TIMEOUT As Integer = 5 * 60 ' seconds
        Public Const _OPT_PREFIX_MFA_EMAIL = "@"                ' D7501

        ''' <summary>
        ''' Default timeout for session timeout (in seconds). By default is 10 minutes.
        ''' </summary>
        ''' <remarks>Used for getting list of on-line users</remarks>
        Public Const _DEF_SESS_TIMEOUT As Integer = 5 * 60   ' D0181 + D0501 // in seconds

        Public Const _DEF_PING_TIMEOUT As Integer = 5       ' D6022 // in seconds

        Public Const _DEF_PROJECTS_WIPEOUP_DAYS As Integer = 90     ' D0941

        Public Const _DEF_SYNC_MASTER_PRJ_TIMEOUT As Integer = 60 * 60 * 2  ' D2657 + D3141 // in seconds

        ''' <summary>
        ''' User this constant for reflect current version of EULA
        ''' </summary>
        ''' <remarks>I recommend to use timestamp</remarks>
        Public Const _EULA_REVISION As String = "200128"    ' D0272 + D0285 + D0299 + D4450 + D6444 + D6577

        Public Const _SPYRON_AVAILABLE As Boolean = True Or clsLicense.SPYRON_ALLOW_FOR_ALL ' D0236 + D0820

        Public Const _TEAMTIME_AVAILABLE As Boolean = True  ' D0494

        Public Const _TEAMTIME_FINISH_ON_OPEN_OTHER_MODEL As Boolean = False ' D4956

        Public Const _RA_AVAILABLE As Boolean = True        ' D0741

        Public Const _RA_GUROBI_AVAILABLE As Boolean = False    ' D3923 + D4526 + D4550 + 7248

        Public Const _RA_XA_AVAILABLE As Boolean = True        ' D7543

        Public Const _RA_XA_AVAILABLE_WHEN_DRAFT_ONLY As Boolean = True ' D7603

        Public Const _RA_BARON_AVAILABLE As Boolean = True      ' D7543

        Public Const _RA_SOLVER_OPTION_AVAILABLE As Boolean = True ' D7543

        Public Const _RA_TIMEPERIODS_AVAILABLE As Boolean = True    ' D3725      // must be the same as 'ShowRATimeperiods' (ViewModelBase.vb)

        Public Const _RA_FUNDING_POOLS_AVAILABLE As Boolean = True   ' D3241

        Public Const _EXPORT_AVAILABLE As Boolean = True    ' D0912

        Public Const _COMMERCIAL_USE As Boolean = True      ' D0912 + D0917

        Public Const _RISK_ENABLED As Boolean = True        ' D2056

        Public Const _MEETING_ID_AVAILABLE As Boolean = True    ' D0387 + D0397 + D0406

        Public Const _LOGIN_WITH_MEETINGID_TO_INACTIVE_MEETING As Boolean = True    ' D0424

        Public Const _LICENSES_ALLOW_TO_DISABLE As Boolean = False              ' D0267

        Public Const _LICENSE_DELETE_KEYWORD As String = "DELETELICENSE"        ' D0266

        ''' <summary>
        ''' Default period in seconds for Synchronous requests
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _DEF_SYNCH_REFRESH As Integer = 3     ' D0194 + D0771 // in seconds

        Public _RES_WARNING_MSG As String = "(!) Resource with name '{0}' not found!"   ' D0461

        Public Const _USE_USER_EMAIL_FOR_EMPTY_FACILITATOR As Boolean = True    ' D0811

        Public Const _OPT_SHOW_HELP_AUTHORIZED As Boolean = False       ' D4022
        Public Const _OPT_HELP_ROBOHELP As Boolean = False              ' D6230
        Public Const _OPT_HELP_LOAD_ONDEMAND As Boolean = True          ' D6508 + D6510

        Public Const _CLEAN_UP_USERS_ON_DELETE As Boolean = True        ' D0827

        Public Const _OPT_ALLOW_STARRED_PROJECTS As Boolean = True      ' A1715 + D5006

        Public _OPT_SNAPSHOTS_ENABLE As Boolean = True            ' D3829

        Public Const _OPT_SNAPSHOTS_ARCHIVE As Boolean = True           ' D3509
        Public Const _OPT_SNAPSHOTS_ARCHIVE_MIN_SIZE As Integer = 4096  ' D3509

        Public Const _OPT_SNAPSHOTS_CHECK_ONLY_MD5 As Boolean = True    ' D3595
        Public Const _OPT_SNAPSHOTS_MAX_COUNT As Integer = 50           ' D3592
        Public Const _OPT_SNAPSHOTS_MANUAL_MAX_COUNT As Integer = 40    ' D3890 + D4156
        Public Const _OPT_SNAPSHOTS_KEEP_PERIOD_MINS As Integer = 30    ' D3890
        Public Const _OPT_SNAPSHOTS_EXPIRATION_DAYS As Integer = 90     ' D4700
        Public Const _OPT_SNAPSHOTS_SAVE_ON_RESTORE As Boolean = True   ' D3723 + D3726
        Public Const _OPT_SNAPSHOTS_SAVE_LAST_STEPS As Boolean = False  ' D3726
        Public Const _OPT_SNAPSHOTS_SAVE_USEREMAIL As Boolean = True    ' D7505
        Public Const _OPT_SNAPSHOTS_REPLACE_SIMILAR As Boolean = True   ' D3754
        Public Const _OPT_SNAPSHOTS_SIMILAR_PERIOD_MINS As Integer = 24 * 60 ' D7485 // save snapshot with the same action but another hash in case if the last one older than that period
        Public Const _OPT_SNAPSHOTS_CHECKLAST_MSEC As Integer = 3000    ' D3757
        Public Const _OPT_SNAPSHOTS_CREATE_ON_RTE As Boolean = True     ' D4175

        Public Const _OPT_SAVE_PROJECT_DETAILED_LOGS As Boolean = True  ' D3571

        Public Const _SNAPSHOT_WORKSPACE_DELIM As Char = CChar(vbTab)   ' D3511
        Public Const _SNAPSHOT_DETAILS_DELIM As String = "; "           ' D3754

        Public Const _SNAPSHOT_USE_IN_AHPS As Boolean = True            ' D3892
        Public Const _SNAPSHOT_STREAM_HEADER As String = "Snapshots"    ' D3847
        Public Const _SNAPSHOT_STREAM_VERSION As Short = 1              ' D3847

        Public Const _SNAPSHOT_CREATE_NEW_USERS_ONRESTORE As Boolean = True ' D3851

        Public Const _OPT_EVAL_SITE_BY_DEFAULT As Boolean = False       ' D3836

        Public Const _OPT_SHOW_CONTOL_IDX As Boolean = True             ' D4188

        ''' <summary>
        ''' Web.config option name for store filename with default pipe-parameters
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _OPT_DEFAULTPIPEPARAMS As String = "DefaultPipeParams" ' D0133
        Public Const _OPT_DEFAULTPIPEPARAMS_RISK As String = "DefaultPipeParamsRisk" ' D2575

        Public Const _OPT_DEFAULTPIPEPARAMSETS As String = "DefaultPipeParamSets" ' D0832
        Public Const _OPT_DEFAULTPIPEPARAMSETS_RISK As String = "DefaultPipeParamSetsRisk"  ' D2256

        Public Const _SESS_CMD_TERMINATE As String = "CmdTerminate" ' D7356

        Public Const _OPT_MODE_AREA_VALIDATION2 = "av2"     ' D7566
        Public Const _OPT_MODE_ALEXA_PROJECT = "alexa"      ' D7584

#End Region

#Region "DB constants"

        ''' <summary>
        ''' Required version for Canvas Master DB
        ''' </summary>
        ''' <remarks></remarks>
        Public Const _DB_MINVER_MASTERDB As String = "0.99992"   ' D0086 + D0091 + D0094 + D0261 + D0419 + D0461 + D0496 + D0548 + В0585 + D0660 + D0795 + D0891 + D0896 + D0899 + D0919 + D1125 + D1139 + D1275 + D1287 + D1380 + D2175 + D2184 + D2211 + D2213 + D2384 + D3333 + D3438 + D3729

        Public Const _DB_MINVER_PROJECTSDB As String = "1.0.5"  ' D0496 + D2175 + D2182 + D2363

        ''' <summary>
        ''' Flag for check AHP version before upload data to server. By default is true.
        ''' </summary>
        ''' <remarks>used for suppress situations with AHP models, created under beta EC version</remarks>
        Public _DB_CHECK_AHP_VERSION As Boolean = True    ' D0172

        Public _DB_CANVAS_MASTERDB As String = "CoreDB"     ' D0329

        Public _DB_VERSION_STRING As String = "ver. "         ' D0375
        Public _DB_VERSION_UNKNOWN As String = "unknown"      ' D0375

        ''' <summary>
        ''' Prefix for separate extra AHP table from Canvas' ProjectDB tables
        ''' </summary>
        ''' <remarks></remarks>
        Public _AHP_EXTRATABLES_PREFIX As String = "AHP_"


#End Region

#Region "File paths and names"

        ' D1534 ===
        Public _FILE_ROOT As String
        Public _FILE_API As String  ' D7215
        Public _FILE_DATA As String
        Public _FILE_DOCMEDIA As String
        Public _FILE_MHT_FILES As String
        Public _FILE_IMAGE_BLANK As String
        ' D0035 ===
        Public _FILE_DATA_SQL As String
        Public _FILE_DATA_RESX As String
        Public _FILE_DATA_INC As String
        ' D0035 ==
        Public _FILE_DATA_LICENSE As String
        Public _FILE_DATA_SETTINGS As String
        Public _FILE_DATA_TEMPLATES As String
        Public _FILE_DATA_TEMPLATES_RISK As String  ' D2811
        Public _FILE_DATA_SAMPLES As String
        Public _FILE_DATA_SAMPLES_RISK As String    ' D2811
        Public _FILE_DATA_MASTERPROJECTS As String  ' D2600
        Public _FILE_DATA_MASTERPROJECTS_RISK As String  ' D2600

        Public _FILE_NODESETS_SAMPLES As String     ' D5041
        Public _FILE_NODESETS_WKG As String         ' D5041

        Public _FILE_REPORT_TEMPLATES As String
        Public _FILE_SPECIAL As String

        Public _FILE_SQL_EMPTY_MDB As String
        Public _FILE_SQL_CANVASDB As String
        Public _FILE_SQL_CANVAS_STREAMSDB As String
        Public _FILE_SQL_PROJECTDB As String
        Public _FILE_SQL_SPYRONDB As String
        Public _FILE_SQL_SPYRON_STREAMSDB As String
        
        Public _FILE_SQL_SPYRON_PROJECTDB As String
        Public _FILE_PROJECTDB_AHP As String
        Public _FILE_PROJECTDB_AHPX As String

        Public Const _FILE_MHT_MEDIADIR As String = "media"     ' D0131
        Public Const _FILE_IMAGES As String = "images/"   ' D0121
        Public Const _FILE_SETTINGS_DEFPIPEPARAMS As String = "pipe_default.xml"    ' D0133
        ' D1534 ==

        Public Const _FILE_LEFTHAND_MENU As String = "~/lefthand_menu.xml" ' D0029
        Public Const _FILE_RESOURCE_EXT As String = ".resx"       ' D0030

        Public Const _FILE_OPEN_API_YAML = "openapi.yaml"        ' D7222
        Public Const _FILE_OPEN_API_JSON = "openapi.json"        ' D7222

        ' D0800 === (remove _FILE_DATA_INC)
        Public Const _FILE_TEMPL_WELCOME_ADMIN As String = "welcome_admin.inc"                  ' D0089
        Public Const _FILE_TEMPL_WELCOME_EVALUATE As String = "welcome_evaluate.inc"            ' D0023 + D0060
        Public Const _FILE_TEMPL_WELCOME_EVALUATE_OPPORTUNITY As String = "welcome_evaluate_op.inc"     ' D3326
        Public Const _FILE_TEMPL_WELCOME_IMPACT As String = "welcome_impact.inc"                ' D2325
        Public Const _FILE_TEMPL_WELCOME_LIKELIHOOD As String = "welcome_likelihood.inc"        ' D2325
        Public Const _FILE_TEMPL_WELCOME_IMPACT_OPPORTUNITY As String = "welcome_impact_op.inc"         ' D3326
        Public Const _FILE_TEMPL_WELCOME_LIKELIHOOD_OPPORTUNITY As String = "welcome_likelihood_op.inc" ' D3326
        Public Const _FILE_TEMPL_WELCOME_RISK As String = "welcome_risk.inc"                    ' D2537
        Public Const _FILE_TEMPL_WELCOME_RISK_OPPORTUNITY As String = "welcome_risk_op.inc"     ' D3327
        Public Const _FILE_TEMPL_WELCOME_MANAGE As String = "welcome_manage.inc"                ' D0060
        Public Const _FILE_TEMPL_THANKYOU As String = "thankyou.inc"                            ' D0023
        Public Const _FILE_TEMPL_THANKYOU_OPPORTUNITY As String = "thankyou_op.inc"             ' D3326
        Public Const _FILE_TEMPL_THANKYOU_INTENSITIES As String = "thankyou_intensities.inc"    ' D2325
        Public Const _FILE_TEMPL_THANKYOU_IMPACT As String = "thankyou_impact.inc"              ' D2325
        Public Const _FILE_TEMPL_THANKYOU_LIKELIHOOD As String = "thankyou_likelihood.inc"      ' D2325
        Public Const _FILE_TEMPL_THANKYOU_IMPACT_OPPORTUNITY As String = "thankyou_impact_op.inc"           ' D3326
        Public Const _FILE_TEMPL_THANKYOU_LIKELIHOOD_OPPORTUNITY As String = "thankyou_likelihood_op.inc"   ' D3326
        Public Const _FILE_TEMPL_THANKYOU_RISK As String = "thankyou_risk.inc"                  ' D2537
        Public Const _FILE_TEMPL_THANKYOU_RISK_OPPORTUNITY As String = "thankyou_risk_op.inc"  ' D3327
        Public Const _FILE_TEMPL_THANKYOU_REWARD As String = "thankyou_reward.inc"              ' D3972
        Public Const _FILE_TEMPL_ABOUT As String = "about.inc"                                  ' D0089
        Public Const _FILE_TEMPL_QUCIK_HELP As String = "qh_{0}.inc"                            ' D3693

        Public Const _FILE_INC_EULA As String = "EULA.html"   ' D0285
        Public Const _FILE_INC_EULA_NONCOMM As String = "EULA_noncom.html"   ' D3475
        ' D0800 ==

        Public Const _FILE_WKG_EMAIL_SUBJ As String = "EmailSubject.txt"    ' D3589
        Public Const _FILE_WKG_EMAIL_BODY As String = "EmailBody.txt"       ' D3589

        Public Const _FILE_MASTERPROJECTS_DT As String = "date.txt"   ' D2632
        Public Const _FILE_MASTERPROJECTS_WIPE As String = "wipelist.txt"   ' D3141

        Public Const _FILE_WHATS_NEW As String = "whatsnew.txt"         ' D5044
        Public Const _FILE_KNOWN_ISSUES As String = "knownissues.htm"   ' D6028 + D6034
        Public Const _FILE_KNOWN_ISSUES_RISKION As String = "knownissues_riskion.htm"   ' D6956

        Public Const _LANG_DEFCODE As String = "English"     ' D0030

        Public _FILE_DEFAULT_MASTERPAGE As String = "mpDesktop.master"   ' D0335 + D0567

        'Public _FILE_EXT_UPLOAD() As String = {_FILE_EXT_AHP, _FILE_EXT_AHPX, _FILE_EXT_AHPS, _FILE_EXT_AHPZ, _FILE_EXT_RAR, _FILE_EXT_ZIP}   ' D2601 + D4756
        Public _FILE_EXT_UPLOAD() As String = {_FILE_EXT_AHPS, _FILE_EXT_AHP, _FILE_EXT_AHPZ, _FILE_EXT_ZIP, _FILE_EXT_RAR}   ' D2601 + D4756 + D6018

        ' D1534 ===
        Public Sub InitPaths(sFileRootPath As String)
            _FILE_ROOT = sFileRootPath
            _FILE_API = _FILE_ROOT + "api\"     ' D7215
            _FILE_DATA = _FILE_ROOT + "App_Data\"
            _FILE_DOCMEDIA = _FILE_ROOT + "DocMedia\"  ' D0131
            _FILE_MHT_FILES = _FILE_DOCMEDIA + "MHTFiles\"  ' D0131
            _FILE_IMAGE_BLANK = _FILE_IMAGES + "blank.gif" ' D0136
            _FILE_DATA_SQL = _FILE_DATA + "sql\"
            _FILE_DATA_RESX = _FILE_DATA + "resx\"
            _FILE_DATA_INC = _FILE_DATA + "inc\"
            _FILE_DATA_LICENSE = _FILE_DATA + "licenses\"   ' D0085 + D0257
            _FILE_DATA_SETTINGS = _FILE_DATA + "settings\" ' D0133
            ' D2811 ===
            _FILE_DATA_TEMPLATES = _FILE_ROOT + "Templates\Comparion\"   ' D0326 + D1198
            _FILE_DATA_TEMPLATES_RISK = _FILE_ROOT + "Templates\Riskion\"
            _FILE_DATA_SAMPLES = _FILE_ROOT + "Samples\Comparion\"       ' D1081 + D1198
            _FILE_DATA_SAMPLES_RISK = _FILE_ROOT + "Samples\Riskion\"
            ' D2811 ==
            _FILE_NODESETS_SAMPLES = _FILE_ROOT + "NodeSets\"           ' D5041
            _FILE_NODESETS_WKG = _FILE_DOCMEDIA + "NodeSets\"           ' D5041

            _FILE_REPORT_TEMPLATES = _FILE_ROOT + "Project\Analysis\Reports\"

            _FILE_DATA_MASTERPROJECTS = _FILE_ROOT + "MasterProjects\Comparion\"    ' D2600
            _FILE_DATA_MASTERPROJECTS_RISK = _FILE_ROOT + "MasterProjects\Risk\"    ' D2600
            _FILE_SPECIAL = _FILE_ROOT + "Special\"   ' D0349
            _FILE_SQL_EMPTY_MDB = _FILE_DATA_SQL + "EmptyDB.mdb"                   ' D0792
            _FILE_SQL_CANVASDB = _FILE_DATA_SQL + "CanvasDB.sql"                   ' D0375
            _FILE_SQL_CANVAS_STREAMSDB = _FILE_DATA_SQL + "StreamProjectsDB.sql"   ' D0354 + D0375
            _FILE_SQL_PROJECTDB = _FILE_DATA_SQL + "ProjectDB.sql"
            _FILE_SQL_SPYRONDB = _FILE_DATA_SQL + "SurveysDB.sql"                  ' D0236 + D0375 + D0385
            _FILE_SQL_SPYRON_STREAMSDB = _FILE_DATA_SQL + "StreamSurveysDB.sql"    ' D0375 + D0385
            _FILE_SQL_SPYRON_PROJECTDB = _FILE_DATA_SQL + "SpyronProjectDB.sql" ' D0236
            _FILE_PROJECTDB_AHP = _FILE_DATA_SQL + "ProjectDB.ahp"     ' D0127
            _FILE_PROJECTDB_AHPX = _FILE_DATA_SQL + "ProjectDB.ahpx"   ' D0368
        End Sub
        ' D1534 ==

#End Region

#Region "Default accounts, workgroups and pre-defined roles"

        ''' <summary>
        ''' Predefined ID for user, marked in AHP model as Facilitator
        ''' </summary>
        ''' <remarks>Used for import users. When user have this ID, he will attached as to decision as project owner.</remarks>
        Public Const FACILITATOR_USER_ID As Integer = 0         ' D0070

        ' D0045 ===
        ''' <summary>
        ''' Default name for System workgroup
        ''' </summary>
        ''' <remarks>Should be non-empty and not NULL</remarks>
        Public _DB_DEFAULT_SYSWORKGROUP_NAME As String = "System Workgroup"

        Public _DB_DEFAULT_STARTUPWORKGROUP_NAME As String = "Startup_default"  ' D0584

        ''' <summary>
        ''' Predefined Account with highest privileges, named as Administrator.
        ''' </summary>
        ''' <remarks>User with this login couldn't be deleted or disabled. Account will attached to each object in system automatically.</remarks>
        Public _DB_DEFAULT_ADMIN_LOGIN As String = "Admin"
        ''' <summary>
        ''' Predefined password for default Administrator account. Could be changed with 'Edit Account' screen after log in to any workgroup.
        ''' </summary>
        ''' <remarks></remarks>
        Public _DB_DEFAULT_ADMIN_PSW As String = "admin"

        Public _DEF_PASSWORD_LENGTH As Integer = 8   ' D0286

        Public _DEF_PASSWORD_FAKE As String = "********"   ' D5025

        Public _DEF_PASSCODE_LENGTH As Integer = 10  ' D0286 + D1712

        Public _DEF_MEETING_ID_LENGTH As Integer = 9    ' D0384



        ' D0087 + D0091 ===
        ''' <summary>
        ''' List of ActionTypes for predefined group "Administrator"
        ''' </summary>
        ''' <remarks>System/App level</remarks>
        Public _DEFROLE_ADMINISTRATOR() As ecActionType = {ecActionType.at_slCreateWorkgroup, ecActionType.at_slManageAnyWorkgroup, ecActionType.at_slManageOwnWorkgroup, ecActionType.at_slDeleteAnyWorkgroup, ecActionType.at_slDeleteOwnWorkgroup, ecActionType.at_slManageAllUsers, ecActionType.at_slViewLicenseAnyWorkgroup, ecActionType.at_slViewLicenseOwnWorkgroup, ecActionType.at_slSetLicenseAnyWorkgroup, ecActionType.at_slSetLicenseOwnWorkgroup, ecActionType.at_slViewAnyWorkgroupReports, ecActionType.at_slViewOwnWorkgroupReports, ecActionType.at_slViewAnyWorkgroupLogs, ecActionType.at_slViewOwnWorkgroupLogs, ecActionType.at_alCreateNewModel, ecActionType.at_alUploadModel, ecActionType.at_alDeleteAnyModel, ecActionType.at_alManageAnyModel, ecActionType.at_alManageWorkgroupRights, ecActionType.at_alManageWorkgroupUsers, ecActionType.at_alCanBePM, ecActionType.at_alViewAllModels}    ' D0190 + D0513 + D0528 + D0727 + D2780

        ''' <summary>
        ''' List of ActionTypes for predefined group "EC Account manager"
        ''' </summary>
        ''' <remarks>System level</remarks>
        Public _DEFROLE_ECACCOUNTMANAGER() As ecActionType = {ecActionType.at_slCreateWorkgroup, ecActionType.at_slManageOwnWorkgroup, ecActionType.at_alManageWorkgroupUsers, ecActionType.at_slDeleteOwnWorkgroup, Data.ecActionType.at_slSetLicenseOwnWorkgroup, ecActionType.at_slViewLicenseOwnWorkgroup, Data.ecActionType.at_slViewOwnWorkgroupReports}

        ''' <summary>
        ''' List of ActionTypes for predefined group "Workgroup Manager"
        ''' </summary>
        ''' <remarks>App level</remarks>
        Public _DEFROLE_WORKGROUPMANAGER() As ecActionType = {ecActionType.at_slViewLicenseOwnWorkgroup, Data.ecActionType.at_slViewOwnWorkgroupReports, ecActionType.at_alManageWorkgroupUsers, ecActionType.at_alViewAllModels, ecActionType.at_alCreateNewModel, ecActionType.at_alUploadModel, ecActionType.at_alDeleteAnyModel, ecActionType.at_alManageAnyModel, ecActionType.at_alCanBePM} ' D0445 + D0528 + D0727 + D2780 + D2790

        ''' <summary>
        ''' List of ActionTypes for predefined group "Project Organizer"
        ''' </summary>
        ''' <remarks>App level</remarks>
        Public _DEFROLE_PROJECTORGANIZER() As ecActionType = {ecActionType.at_alCreateNewModel, ecActionType.at_alUploadModel, ecActionType.at_alCanBePM}    ' D0114 + D0190 + D0528 + D2780

        ' -D2780 + D3288
        ''' <summary>
        ''' List of ActionTypes for predefined group "Workgroups Viewer" (was "Technical Support")
        ''' </summary>
        ''' <remarks>System level</remarks>
        Public _DEFROLE_TECHSUPPORT() As ecActionType = {ecActionType.at_slViewLicenseAnyWorkgroup, ecActionType.at_slViewLicenseOwnWorkgroup} ' D3288
        'Public _DEFROLE_TECHSUPPORT() As ecActionType = {ecActionType.at_slViewLicenseAnyWorkgroup, Data.ecActionType.at_slViewAnyWorkgroupLogs, ecActionType.at_slViewAnyWorkgroupReports, Data.ecActionType.at_slViewAnyWorkgroupReports}

        ' D0087 + D0091 ==
        ''' <summary>
        ''' List of ActionTypes for predefined group "User"
        ''' </summary>
        ''' <remarks>System level</remarks>
        Public _DEFROLE_USER() As ecActionType = {}

        ''' <summary>
        ''' List of ActionTypes for predefined group "Project Manager"
        ''' </summary>
        ''' <remarks>Project level</remarks>
        Public _DEFROLE_PROJECTMANAGER() As ecActionType = {ecActionType.at_mlEvaluateModel, ecActionType.at_mlViewModel, ecActionType.at_mlDownloadModel, ecActionType.at_mlManageModelUsers, ecActionType.at_mlManageProjectOptions, ecActionType.at_mlModifyAlternativeHierarchy, ecActionType.at_mlModifyAltsContributesTo, ecActionType.at_mlModifyMeasurementScales, ecActionType.at_mlModifyModelHierarchy, ecActionType.at_mlPerformSensitivityAnalysis, ecActionType.at_mlViewOverallResults, ecActionType.at_mlSetSpecificRolesEvaluation, ecActionType.at_mlSetSpecificRolesViewing, ecActionType.at_mlUsePredefinedReports, ecActionType.at_mlDeleteModel}  ' D0052 + D0065 + D0087
        ''' <summary>
        ''' List of ActionTypes for predefined group "Evaluator"
        ''' </summary>
        ''' <remarks>Project level</remarks>
        Public _DEFROLE_EVALUATOR() As ecActionType = {ecActionType.at_mlEvaluateModel}    ' D0052
        ' D0045 ==

        Public _DEFROLE_VIEWER() As ecActionType = {ecActionType.at_mlPerformSensitivityAnalysis, ecActionType.at_mlUsePredefinedReports, ecActionType.at_mlViewModel, ecActionType.at_mlViewOverallResults}    ' D2239 + D2857

        Public _DEFROLE_EVALUATOR_N_VIEWER() As ecActionType = {ecActionType.at_mlEvaluateModel, ecActionType.at_mlPerformSensitivityAnalysis, ecActionType.at_mlUsePredefinedReports, ecActionType.at_mlViewModel, ecActionType.at_mlViewOverallResults}    ' D2857

        Public _ROLES_MODIFYPROJECT() As ecActionType = {ecActionType.at_mlManageProjectOptions, ecActionType.at_mlManageModelUsers, ecActionType.at_mlDeleteModel, ecActionType.at_mlModifyModelHierarchy, ecActionType.at_mlModifyAlternativeHierarchy, ecActionType.at_mlModifyAltsContributesTo, ecActionType.at_mlModifyMeasurementScales, ecActionType.at_mlSetSpecificRolesEvaluation}   ' D0466

#End Region

#Region "Workgroup templates"

        ' D3626 ===
        Public Const _TPL_COMPARION_OBJ As String = "%%objective%%"
        Public Const _TPL_COMPARION_OBJS As String = "%%objectives%%"
        Public Const _TPL_COMPARION_ALT As String = "%%alternative%%"
        Public Const _TPL_COMPARION_ALTS As String = "%%alternatives%%"
        ' D3626 ==

        ' D2427 ===
        Public Const _TPL_RISK_EVENT As String = "%%event%%"
        Public Const _TPL_RISK_EVENTS As String = "%%events%%"
        Public Const _TPL_RISK_SOURCE As String = "%%source%%"
        Public Const _TPL_RISK_SOURCES As String = "%%sources%%"
        Public Const _TPL_RISK_CONSEQUENCE As String = "%%consequence%%"
        Public Const _TPL_RISK_CONSEQUENCES As String = "%%consequences%%"
        Public Const _TPL_RISK_CONTROL As String = "%%control%%"
        Public Const _TPL_RISK_CONTROLS As String = "%%controls%%"
        Public Const _TPL_RISK_CONTROLLED As String = "%%controlled%%"
        Public Const _TPL_RISK_LIKELIHOOD As String = "%%likelihood%%"
        Public Const _TPL_RISK_LIKELIHOODS As String = "%%likelihoods%%"    ' D2428
        Public Const _TPL_RISK_IMPACT As String = "%%impact%%"
        Public Const _TPL_RISK_IMPACTS As String = "%%impacts%%"            ' D2428
        Public Const _TPL_RISK_RISK As String = "%%risk%%"
        Public Const _TPL_RISK_RISKS As String = "%%risks%%"                ' D2428
        Public Const _TPL_RISK_VULNERABILITY As String = "%%vulnerability%%"     'A0950
        Public Const _TPL_RISK_VULNERABILITIES As String = "%%vulnerabilities%%" 'A0950
        Public Const _TPL_RISK_LOSS As String = "%%loss%%" 'A1377
        Public Const _TPL_RISK_GAIN As String = "%%gain%%" 'A1377
        Public Const _TPL_RISK_OPPORTUNITY As String = "%%opportunity%%"
        Public Const _TPL_RISK_OPPORTUNITIES As String = "%%opportunities%%"

        Public _TPL_LIST_COMPARION As String() = {_TPL_COMPARION_OBJ, _TPL_COMPARION_OBJS, _TPL_COMPARION_ALT, _TPL_COMPARION_ALTS}    ' D3626

        Public _TPL_LIST_RISK As String() = {_TPL_RISK_EVENT, _TPL_RISK_EVENTS, _TPL_RISK_SOURCE, _TPL_RISK_SOURCES, _
                                             _TPL_RISK_CONSEQUENCE, _TPL_RISK_CONSEQUENCES, _TPL_RISK_CONTROL, _TPL_RISK_CONTROLS, _TPL_RISK_CONTROLLED, _
                                             _TPL_RISK_LIKELIHOOD, _TPL_RISK_LIKELIHOODS,
                                             _TPL_RISK_IMPACT, _TPL_RISK_IMPACTS, _TPL_RISK_RISK, _TPL_RISK_RISKS, _TPL_RISK_OPPORTUNITY, _TPL_RISK_OPPORTUNITIES, _
                                             _TPL_RISK_VULNERABILITY, _TPL_RISK_VULNERABILITIES}   ' D2428 + A0950 + A0957
        ' D2427 ==
#End Region

        Public _URL_ROOT As String = String.Concat(HostingEnvironment.ApplicationVirtualPath, "/").Replace("//", "/")

    End Module

End Namespace
