Imports ExpertChoice.Service

Namespace ExpertChoice.Web

    Partial Public Class clsComparionCorePage

        Public Const SESSION_MAIN_SURVEY_INFO_EDIT As String = "AMainSurveyInfoEdit" 'L0456
        Public Const SESSION_MAIN_SURVEY_INFO_VIEW As String = "AMainSurveyInfoView" 'L0456
        Public Const SESSION_NEW_QUESTION As String = "NewQuestion"

        Public Const OPT_SURVEY_PARSE_LINKS As Boolean = True   ' D1039 + D4102

        Public Const SESSION_ORIGINAL_HID As String = "IntensitiesOriginalHID"   ' D1800 + D1965
        Public Const SESSION_SCALE_ID As String = "IntensitiesMeasureScaleID"    ' D1800 + D1965
        Public Const SESSION_EXCLUDE_GUIDS As String = "IntensitiesExcludeGUIDs" ' D1935 + D2044

        Public Const SESSION_PIPE_PGID As String = "EvalPageID"     ' D7242

        Public Const SESSION_NO_REPORTS As String = "NoReports"     ' D4872

        Private _PagesList As List(Of clsPageAction) = Nothing  ' D0043
        Private _CurrentPageID As Integer = _PGID_UNKNOWN   ' D0043
        Private _NavigationPageID As Integer = -1           ' D0088

        Private Sub LoadPagesList()
            ' main section with description for all pages

            ' === common pages ===============================
            AddPage(New clsPageAction(_PGID_START, "Start", _URL_ROOT + "Default.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_LOGOUT, "Logout", URLWithAction(_URL_ROOT + "Default.aspx", _ACTION_LOGOUT), ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_SSO_ASSERT, "SSOAssert", _URL_ROOT + "SAML/Assertion.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))  ' D6550
            AddPage(New clsPageAction(_PGID_START_WITH_SIGNUP, "StartSignup", _URL_ROOT + "Start.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))  ' D1056
            AddPage(New clsPageAction(_PGID_EULA, "EULA", _URL_ROOT + "EULA.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))   ' D0272
            AddPage(New clsPageAction(_PGID_FEDRAMP_NOTIFICATION, "FedRAMPNotification", _URL_ROOT + "Notification.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))    ' D6081
            AddPage(New clsPageAction(_PGID_INFO_PAGE, "Help", _URL_ROOT + "Info.aspx", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paEveryone)) ' D4705 + D6027
            ' D4994 + D6027 ===
            AddPage(New clsPageAction(_PGID_LANDING_COMMON, "Landing", _URL_PROJECT + "Landing.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))    ' D6047
            AddPage(New clsPageAction(_PGID_LANDING_STRUCTURE, "Structure", _URL_PROJECT + "Landing.aspx?page=structure", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_LANDING_MEASURE, "Measure", _URL_PROJECT + "Landing.aspx?page=measure", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_LANDING_RESULTS, "SeeResults", _URL_PROJECT + "Landing.aspx?page=results", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_LANDING_ALLOCATE, "Allocate", _URL_PROJECT + "Landing.aspx?page=allocate", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_LANDING_REPORTS, "ViewReports", _URL_PROJECT + "Landing.aspx?page=reports", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_LANDING_RISK_IDENTIFY, "IdentifyStructure", _URL_PROJECT + "Landing.aspx?page=structure&risk", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paEveryone))   ' D6061
            ' D4994 + D6027 ==
            AddPage(New clsPageAction(_PGID_RESOURCE_CENTER, "ResourceCenter", _URL_ROOT + "ResourceCenter.html", ecPagePermission.ppAuthorized, ecActionType.at_alCreateNewModel, ecPageAccessType.paEveryone))    '  D5015
            AddPage(New clsPageAction(_PGID_CUSTOMER_SUPPORT, "CustomerSupport", _URL_ROOT + "CustomerSupport.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paEveryone))       ' D0407 + D4266
            AddPage(New clsPageAction(_PGID_KNOWN_ISSUES, "KnownIssues", _URL_ROOT + "KnownIssues.txt", ecPagePermission.ppAuthorized, ecActionType.at_alCanBePM, ecPageAccessType.paEveryone))       ' D6028
            AddPage(New clsPageAction(_PGID_SEND_MAIL, "SendMail", _URL_ROOT + "SendMail.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paEveryone))    ' D3579
            'AddPage(New clsPageAction(_PGID_PROJECTSLIST_STARRED, "StarredProjects", _URL_PROJECT + "Favorites.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects))   'A1715

            '=== projects ====================================
            AddPage(New clsPageAction(_PGID_PROJECTSLIST, "ProjectsListActive", _URL_PROJECT + "List.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_PROJECTSLIST_ARCHIVED, "ProjectsListArchived", URLTab(_URL_PROJECT + "List.aspx", "archived"), ecPagePermission.ppAuthorized, ecActionType.at_alCanBePM, ecPageAccessType.paOnlyProjects))   ' D6356
            AddPage(New clsPageAction(_PGID_PROJECTSLIST_TEMPLATES, "ProjectsListTemplates", URLTab(_URL_PROJECT + "List.aspx", "templates"), ecPagePermission.ppAuthorized, ecActionType.at_alCreateNewModel, ecPageAccessType.paOnlyProjects))  ' D0206 + D2785 + D6356
            AddPage(New clsPageAction(_PGID_PROJECTSLIST_MASTERPROJECTS, "ProjectsListMaster", URLTab(_URL_PROJECT + "List.aspx", "master"), ecPagePermission.ppAuthorized, ecActionType.at_alManageWorkgroupUsers, ecPageAccessType.paOnlyProjects))   ' D2479 + D2785 + D6255
            AddPage(New clsPageAction(_PGID_PROJECTSLIST_DELETED, "ProjectsListDeleted", URLTab(_URL_PROJECT + "List.aspx", "deleted"), ecPagePermission.ppAuthorized, ecActionType.at_alCanBePM, ecPageAccessType.paOnlyProjects))        ' D0789 + D6356
            If _OPT_ALLOW_REVIEW_ACCOUNT Then AddPage(New clsPageAction(_PGID_PROJECTSLIST_REVIEW, "ProjectsListReview", URLTab(_URL_PROJECT + "List.aspx", "review"), ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects))           ' D4640 + D5068

            '-A1520 AddPage(New clsPageAction(_PGID_PROJECT_PROPERTIES, "ProjectProperties", _URL_PROJECT + "Properties.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_PROJECT_DESCRIPTION, "Description", _URL_PROJECT + "Details.aspx", ecPagePermission.ppAuthorized, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))   ' D4942 + D5092
            'AddPage(New clsPageAction(_PGID_PROJECT_PROPERTIES, "ProjectInfo", _URL_PROJECT + "Details.aspx?mode=edit", ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_PROJECT_PROPERTIES, "ProjectInfo", _URL_PROJECT + "Details.aspx?mode=edit", ecPagePermission.ppAuthorized, ecActionType.at_mlDownloadModel, ecPageAccessType.paOnlyProjects))    ' D5092
            'AddPage(New clsPageAction(_PGID_PROJECT_STATUS, "ProjectOnlineStatus", _URL_PROJECT + "Status.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects)) 'A1217
            AddPage(New clsPageAction(_PGID_PROJECT_OPTION_EVALUATE, "PipePropsEvaluate", URLPage(_URL_PROJECT + "PipeParams.aspx", _PGID_PROJECT_OPTION_EVALUATE), ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))    ' D3624
            AddPage(New clsPageAction(_PGID_PROJECT_OPTION_NAVIGATE, "PipePropsNavigate", URLPage(_URL_PROJECT + "PipeParams.aspx", _PGID_PROJECT_OPTION_NAVIGATE), ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))    ' D3624
            AddPage(New clsPageAction(_PGID_PROJECT_OPTION_DISPLAY, "PipePropsDisplay", URLPage(_URL_PROJECT + "PipeParams.aspx", _PGID_PROJECT_OPTION_DISPLAY), ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))       ' D3624
            AddPage(New clsPageAction(_PGID_PROJECT_OPTION_SURVEY, "PipePropsSurvey", URLPage(_URL_PROJECT + "PipeParams.aspx", _PGID_PROJECT_OPTION_SURVEY), ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))          ' D3624

            AddPage(New clsPageAction(_PGID_PROJECT_USERS, "ProjectUsers", _URL_PROJECT + "Users.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects))    ' D0052 + D0500

            AddPage(New clsPageAction(_PGID_PROJECT_CREATE, "ProjectCreate", URLWithAction(_URL_PROJECT + "Properties.aspx", _ACTION_NEW), ecPagePermission.ppAuthorized, ecActionType.at_alCreateNewModel, ecPageAccessType.paOnlyProjects))     ' D0045
            AddPage(New clsPageAction(_PGID_PROJECT_UPLOAD, "ProjectUpload", URLWithAction(_URL_PROJECT + "Properties.aspx", "upload"), ecPagePermission.ppAuthorized, ecActionType.at_alUploadModel, ecPageAccessType.paOnlyProjects))    ' D0045
            AddPage(New clsPageAction(_PGID_PROJECT_DELETE, "ProjectDelete", URLWithAction(_URL_PROJECT + "Properties.aspx", _ACTION_DELETE), ecPagePermission.ppProjectWithLock, ecActionType.at_mlDeleteModel, ecPageAccessType.paOnlyProjects))    ' D0052
            'AddPage(New clsPageAction(_PGID_PROJECT_UNDELETE, "ProjectUnDelete", URLWithAction(_URL_PROJECT + "Properties.aspx", "undelete"), ecPagePermission.ppProject, ecActionType.at_mlDeleteModel, ecPageAccessType.paOnlyProjects))    ' D0789
            AddPage(New clsPageAction(_PGID_PROJECT_COPY, "ProjectCopy", URLWithAction(_URL_PROJECT + "Properties.aspx", "copy"), ecPagePermission.ppProject, ecActionType.at_alCreateNewModel, ecPageAccessType.paOnlyProjects))   ' D0130
            AddPage(New clsPageAction(_PGID_PROJECT_CREATE_FROM_TPL, "ProjectCreateFromTemplate", URLWithAction(_URL_PROJECT + "Properties.aspx", "usetemplate"), ecPagePermission.ppProject, ecActionType.at_alCreateNewModel, ecPageAccessType.paOnlyProjects))   ' D0324

            'AddPage(New clsPageAction(_PGID_PROJECT_LOCK, "ProjectLock", URLWithAction(_URL_PROJECT + "Properties.aspx", "lock"), ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects))   ' D0137
            'AddPage(New clsPageAction(_PGID_PROJECT_UNLOCK, "ProjectUnlock", URLWithAction(_URL_PROJECT + "Properties.aspx", "unlock"), ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects))   ' D0130
            AddPage(New clsPageAction(_PGID_PROJECT_DOWNLOAD, "ProjectDownload", _URL_PROJECT + "Details.aspx?dl=yes", ecPagePermission.ppAuthorized, ecActionType.at_mlDownloadModel, ecPageAccessType.paOnlyProjects))  ' D0857 + D2239
            AddPage(New clsPageAction(_PGID_PROJECT_UPDATE, "ProjectUpdate", _URL_PROJECT + "Update.aspx", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects))      ' D0622
            AddPage(New clsPageAction(_PGID_PROJECT_SNAPSHOTS, "ProjectSnapshots", _URL_PROJECT + "Snapshots.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects))   ' D3558
            AddPage(New clsPageAction(_PGID_PROJECT_LOGS, "ProjectLogs", _URL_PROJECT + "ProjectLogs.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects))    ' D3559

            AddPage(New clsPageAction(_PGID_MEASURE_EVAL_PROGRESS, "EvalProgress", URLPage(_URL_PROJECT + "Evaluators.aspx", _PGID_MEASURE_EVAL_PROGRESS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))    'A1044 + D7652
            AddPage(New clsPageAction(_PGID_CONTROLS_EVAL_PROGRESS, "EvalProgress", URLPage(_URL_PROJECT + "Evaluators.aspx", _PGID_CONTROLS_EVAL_PROGRESS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))    'A1040 + D7652

            AddPage(New clsPageAction(_PGID_STRUCTURE_ALTERNATIVES, "AlternativesEditor", URLTab(_URL_STRUCTURE + "Hierarchy.aspx", "Alternatives"), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  'A1326
            AddPage(New clsPageAction(_PGID_STRUCTURE_HIERARCHY, "HierarchyEditor", URLTab(_URL_STRUCTURE + "Hierarchy.aspx", "Objectives"), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_SOURCES, "SourcesEditor", URLNavPage(_URL_STRUCTURE + "Hierarchy.aspx?hid=0", _PGID_STRUCTURE_SOURCES), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_SOURCES_HIERARCHY, "RiskSourcesHierarchy", URLNavPage(_URL_STRUCTURE + "Hierarchy.aspx?hid=0", _PGID_STRUCTURE_SOURCES_HIERARCHY), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_SOURCES_CONTRIBUTIONS, "RiskSourcesContributions", URLNavPage(_URL_STRUCTURE + "AltsContribution.aspx?hid=0", _PGID_STRUCTURE_SOURCES_CONTRIBUTIONS), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyAltsContributesTo, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_OBJECTIVES, "ObjectivesEditor", URLNavPage(_URL_STRUCTURE + "Hierarchy.aspx?hid=2", _PGID_STRUCTURE_OBJECTIVES), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_OBJECTIVES_HIERARCHY, "RiskObjectivesHierarchy", URLNavPage(_URL_STRUCTURE + "Hierarchy.aspx?hid=2", _PGID_STRUCTURE_OBJECTIVES_HIERARCHY), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_OBJECTIVES_CONTRIBUTIONS, "RiskObjectivesContributions", URLNavPage(_URL_STRUCTURE + "AltsContribution.aspx?hid=2", _PGID_STRUCTURE_OBJECTIVES_CONTRIBUTIONS), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyAltsContributesTo, ecPageAccessType.paOnlyProjects))
            'AddPage(New clsPageAction(_PGID_STRUCTURE_ALTERNATIVES, "AlternativesEditor", _URL_RA + "StrategicBuckets.aspx?action=alts", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  'A1326
            AddPage(New clsPageAction(_PGID_STRUCTURE_ALTSCONTRIB, "AltsContribution", _URL_STRUCTURE + "AltsContribution.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyAltsContributesTo, ecPageAccessType.paOnlyProjects))    ' D0097
            AddPage(New clsPageAction(_PGID_STRUCTURE_INFODOCS, "Infodocs", _URL_STRUCTURE + "Infodocs.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))    ' D3565
            AddPage(New clsPageAction(_PGID_STRUCTURE_MYRISKREWARD, "MyRiskReward", _URL_STRUCTURE + "MyRiskReward.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))     ' D6799

            'AddPage(New clsPageAction(_PGID_MAP_GRID_VIEW_RISK, "AltsContribution", _URL_STRUCTURE + "AltsContribution.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyAltsContributesTo, ecPageAccessType.paOnlyProjects))    'A1475

            'AddPage(New clsPageAction(_PGID_STRUCTURE_DEFINEMEASUREMENT, "DefineMeasurement", _URL_STRUCTURE + "MeasurementType.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects))  ' D0160
            'AddPage(New clsPageAction(_PGID_STRUCTURE_MEASUREMENT_SCALES_ALL, "DefineMeasurement", _URL_STRUCTURE + "MeasurementScales.aspx?view=all", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects))  'A1045
            AddPage(New clsPageAction(_PGID_BAYESIAN_UPDATING, "BayesianUpd", _URL_STRUCTURE + "BayesianUpdating.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects))  'A1464

            AddPage(New clsPageAction(_PGID_STRUCTURE_MEASUREMENT_SCALES_DEFAULT, "DefaultScales", _URL_STRUCTURE + "MeasurementScales.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects))  'A1045

            AddPage(New clsPageAction(_PGID_STRUCTURE_MEASUREMENT_METHODS_OBJS, "MethodsObjs", _URL_STRUCTURE + "MeasurementMethods.aspx?view=obj", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_MEASUREMENT_METHODS_ALTS, "MethodsAlts", _URL_STRUCTURE + "MeasurementMethods.aspx?view=alt", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_MEASUREMENT_METHODS, "DefineMeasurement", _URL_STRUCTURE + "MeasurementMethods.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects))  'A1083            

            'AddPage(New clsPageAction(_PGID_STRUCTURE_PERMISSIONS, "PermissionsObj", _URL_STRUCTURE + "Permissions.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlSetSpecificRolesEvaluation, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_STRUCTURE_PERMISSION_ALTS, "Roles", _URL_STRUCTURE + "Permissions.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlSetSpecificRolesEvaluation, ecPageAccessType.paOnlyProjects))   ' D3565
            AddPage(New clsPageAction(_PGID_STRUCTURE_MISSING_ROLES_REPORT, "MissingRolesReport", _URL_STRUCTURE + "MissingRoles.aspx", ecPagePermission.ppProject, ecActionType.at_mlSetSpecificRolesEvaluation, ecPageAccessType.paOnlyProjects))   ' D6092

            AddPage(New clsPageAction(_PGID_RICHEDITOR, "RichEditor", _URL_STRUCTURE + "RichEditor.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paUnspecified))    ' D0107 + D1146 (allow access to everyone, was authorized)

            ' D0074 ===
            'AddPage(New clsPageAction(_PGID_PROJECT_OVERVIEW, "ProjectOverview", _URL_PROJECT + "Details.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewModel, ecPageAccessType.paOnlyProjects))
            'AddPage(New clsPageAction(_PGID_OVERVIEW_PURPOSE, "OverviewPurpose", URLTab(_URL_PROJECT + "Overview.aspx", "Purpose"), ecPagePermission.ppProject, ecActionType.at_mlViewModel, ecPageAccessType.paOnlyProjects))   ' D2239
            'AddPage(New clsPageAction(_PGID_OVERVIEW_OBJECTIVES, "OverviewObjectives", URLTab(_URL_PROJECT + "Overview.aspx", "Objectives"), ecPagePermission.ppProject, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))  ' D2239
            'AddPage(New clsPageAction(_PGID_OVERVIEW_ALTERNATIVES, "OverviewAlternatives", URLTab(_URL_PROJECT + "Overview.aspx", "Alternatives"), ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' D2239
            'AddPage(New clsPageAction(_PGID_OVERVIEW_CONTRIBUTION, "OverviewContribution", URLTab(_URL_PROJECT + "Overview.aspx", "Contribution"), ecPagePermission.ppProject, ecActionType.at_mlModifyAltsContributesTo, ecPageAccessType.paOnlyProjects)) ' D2239
            'AddPage(New clsPageAction(_PGID_OVERVIEW_EVALUATORS, "OverviewEvaluators", URLTab(_URL_PROJECT + "Overview.aspx", "Evaluators"), ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects))  ' D2239
            'AddPage(New clsPageAction(_PGID_OVERVIEW_INVITATIONS, "OverviewInvitations", URLTab(_URL_PROJECT + "Overview.aspx", "Invitations"), ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects))  ' D0299 + D2239
            ' D0074 ==

            'AddPage(New clsPageAction(_PGID_RESULTGROUPS, "ResultGroups", _URL_PROJECT + "ResultGroups.aspx", ecPagePermission.ppProject, ecActionType.at_mlSetSpecificRolesViewing, ecPageAccessType.paOnlyProjects))  ' D0675
            AddPage(New clsPageAction(_PGID_RESULTGROUPS, "ResultGroups", _URL_ANALYSIS + "Groups.aspx", ecPagePermission.ppProject, ecActionType.at_mlSetSpecificRolesViewing, ecPageAccessType.paOnlyProjects))

            '-A1564 AddPage(New clsPageAction(_PGID_ANTIGUA_ADMIN, "AntiguaAdmin", _URL_ANTIGUA + "AdminUI.aspx", ecPagePermission.ppProjectWithStructuringLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))   ' D0580 + D0589
            '-A1564 AddPage(New clsPageAction(_PGID_ANTIGUA_MEETING, "AntiguaMeeting", _URL_ANTIGUA + "Structuring.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paUnspecified))      ' D0585
            AddPage(New clsPageAction(_PGID_ANTIGUA_MEETING, "Structuring", _URL_TEAMTINE + "Structuring.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))      'A1564 + D4920
            AddPage(New clsPageAction(_PGID_ANTIGUA_MEETING_LIKELIHOOD, "StructuringLikelihood", _URL_TEAMTINE + "Structuring.aspx?pgid=" + _PGID_ANTIGUA_MEETING_LIKELIHOOD.ToString, ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_ANTIGUA_MEETING_IMPACT, "StructuringImpact", _URL_TEAMTINE + "Structuring.aspx?pgid=" + _PGID_ANTIGUA_MEETING_IMPACT.ToString, ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))

            AddPage(New clsPageAction(_PGID_EVALUATION, "EvaluateNow", _URL_EVALUATE + "Default.aspx", ecPagePermission.ppProject, ecActionType.at_mlEvaluateModel, ecPageAccessType.paOnlyProjects))   ' D0493
            'AddPage(New clsPageAction(_PGID_EVALUATENOW, "EvaluateNow", _URL_EVALUATE + "Default.aspx", ecPagePermission.ppProject, ecActionType.at_mlEvaluateModel, ecPageAccessType.paOnlyProjects))  ' D0192
            AddPage(New clsPageAction(_PGID_EVALUATE_INVITE, "EvaluateInvite", _URL_EVALUATE + "Invite.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects))   ' D4705
            'AddPage(New clsPageAction(_PGID_EVALUATE_DATAINSTANCES, "EvaluateDataInstance", _URL_EVALUATE + "Default.aspx?" + _PARAM_DATAINSTANCE + "=true", ecPagePermission.ppProject, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))  ' D0228
            AddPage(New clsPageAction(_PGID_EVALUATE_INTENSITIES, "EvaluateIntensities", _URL_EVALUATE + "Default.aspx?" + _PARAM_INTENSITIES + "=true", ecPagePermission.ppProject, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))    ' D1800
            AddPage(New clsPageAction(_PGID_EVALUATE_READONLY, "EvaluateReadOnly", _URL_EVALUATE + "Default.aspx?" + _PARAM_READONLY + "=true", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))   ' D0309
            AddPage(New clsPageAction(_PGID_EVALUATE_RISK_CONTROLS, "EvaluateRiskWithControl", _URL_EVALUATE + "Default.aspx?mode=riskcontrols", ecPagePermission.ppProject, ecActionType.at_mlEvaluateModel, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' D2452 + D2955
            AddPage(New clsPageAction(_PGID_EVALUATE_RISK_CONTROLS_READONLY, "EvaluateRiskWithControlReadOnly", _URL_EVALUATE + "Default.aspx?" + _PARAM_READONLY + "=true&mode=riskcontrols", ecPagePermission.ppProject, ecActionType.at_mlEvaluateModel, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' D4285
            AddPage(New clsPageAction(_PGID_EVALUATE_INFODOC, "EvaluateNow", _URL_EVALUATE + "Infodoc.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))  ' D0787 + D6402
            AddPage(New clsPageAction(_PGID_TEAMTIME, "EvaluateTeamTime", _URL_TEAMTINE + "Default.aspx", ecPagePermission.ppProject, ecActionType.at_mlEvaluateModel, ecPageAccessType.paOnlyProjects))   ' D1234
            AddPage(New clsPageAction(_PGID_TEAMTIME_CV, "EvaluateTeamTime", _URL_TEAMTINE + "Default.aspx?mode=consensus", ecPagePermission.ppProject, ecActionType.at_mlEvaluateModel, ecPageAccessType.paOnlyProjects))    ' D3563
            AddPage(New clsPageAction(_PGID_TEAMTIME_INVITE, "EvaluateTeamTimeInvite", _URL_TEAMTINE + "Invite.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects))   ' D4705

            AddPage(New clsPageAction(_PGID_TEAMTIME_STATUS, "TTStatus", _URL_TEAMTINE + "Status.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects))   ' A1217
            AddPage(New clsPageAction(_PGID_TEAMTIME_USERS, "TTUsers", _URL_TEAMTINE + "Users.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects))      ' D4705

            'AddPage(New clsPageAction(_PGID_ITERATE_MEASUREMENT, "IterateMeasurement", _URL_STRUCTURE + "Iterate.aspx?mode=measure", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects)) 'A1217
            'AddPage(New clsPageAction(_PGID_ITERATE_STRUCTURING, "IterateStructuring", _URL_STRUCTURE + "Iterate.aspx?mode=structure", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects)) 'A1217
            'AddPage(New clsPageAction(_PGID_ITERATE_MEASUREMENT_IMPACT, "IterateMeasurement", _URL_STRUCTURE + "Iterate.aspx?mode=measure&hid=2", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects))
            'AddPage(New clsPageAction(_PGID_ITERATE_STRUCTURING_IMPACT, "IterateStructuring", _URL_STRUCTURE + "Iterate.aspx?mode=structure&hid=2", ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))

            ' === accounts ===================================
            AddPage(New clsPageAction(_PGID_CREATE_PASSWORD, "CreatePassword", _URL_ROOT + "Password.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))     ' D2215
            AddPage(New clsPageAction(_PGID_ACCOUNT_EDIT, "EditAccount", _URL_ACCOUNT + "Edit.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_PINCODE, "PinCode", _URL_ROOT + "pin/", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))        ' D7187

            'AddPage(New clsPageAction(_PGID_WORKGROUP_SELECT, "SelectWorkgroup", _URL_ROOT + "SelectWorkgroup.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))  ' D0096 -D4842

            'AddPage(New clsPageAction(_PGID_ACCOUNT_FORGOTTEN, "ForgottenPassword", _URL_ACCOUNT + "Forgotten.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))  ' D0080

            ' === reports ====================================
            AddPage(New clsPageAction(_PGID_REPORTS, "Reports", _URL_REPORTS + "ReportView.aspx", ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0069 +  D0206
            AddPage(New clsPageAction(_PGID_REPORT_GENERATOR, "ReportGenerator", _URL_PROJECT + "Report/Default.aspx", ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))    ' D6511
            'AddPage(New clsPageAction(_PGID_REPORT_LOGS, "ReportLogs", URLTab(_URL_REPORTS + "ReportView.aspx", "Logs"), ecPagePermission.ppAuthorized, ecActionType.at_slViewAnyWorkgroupLogs, ecPageAccessType.paEveryone))  ' D0047 + D0050 + D0087
            AddPage(New clsPageAction(_PGID_REPORT_STRUCTURE, "ReportStructure", URLTab(_URL_REPORTS + "ReportView.aspx", "Structure"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0050
            AddPage(New clsPageAction(_PGID_REPORT_OBJECTIVES, "ReportObjectives", URLTab(_URL_REPORTS + "ReportView.aspx", "Objectives"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0062
            AddPage(New clsPageAction(_PGID_REPORT_ALTERNATIVES, "ReportAlternatives", URLTab(_URL_REPORTS + "ReportView.aspx", "Alternatives"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' L0193
            AddPage(New clsPageAction(_PGID_REPORT_OBJANDALTS, "ReportObjectivesAndAlternatives", URLTab(_URL_REPORTS + "ReportView.aspx", "ObjAndAlts"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0062
            AddPage(New clsPageAction(_PGID_REPORT_JUDGMENTS, "ReportJudgments", URLTab(_URL_REPORTS + "ReportView.aspx", "Judgments"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0054
            AddPage(New clsPageAction(_PGID_REPORT_JUDGMENTS_ALTS, "ReportJudgmentsAlts", URLTab(_URL_REPORTS + "ReportView.aspx", "Judgments_Alts"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D3703
            AddPage(New clsPageAction(_PGID_REPORT_OBJPRIORITIES, "ReportObjPriorities", URLTab(_URL_REPORTS + "ReportView.aspx", "ObjPriorities"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0065
            AddPage(New clsPageAction(_PGID_REPORT_OBJANDALTPRIORITIES, "ReportObjAndAltPriorities", URLTab(_URL_REPORTS + "ReportView.aspx", "ObjAndAltPriorities"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0065
            AddPage(New clsPageAction(_PGID_REPORT_OVERALLRESULTS, "ReportOverallAltResults", URLTab(_URL_REPORTS + "ReportView.aspx", "Overall"), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))  ' D0065
            AddPage(New clsPageAction(_PGID_REPORT_USERSOVERALLRESULTS, "ReportUsersOverallResults", URLTab(_URL_REPORTS + "ReportView.aspx", "OverallResults"), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))  ' L0390

            AddPage(New clsPageAction(_PGID_REPORT_CUSTOM, "ReportCustom", _URL_REPORTS + "Custom.aspx", ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0138 + D0707
            AddPage(New clsPageAction(_PGID_REPORT_CUSTOM_PRIORITY, "ReportCustomPriority", URLTab(_URL_REPORTS + "Custom.aspx", "Priority"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0707
            AddPage(New clsPageAction(_PGID_REPORT_CUSTOM_JUDGMENTS, "ReportCustomJudgments", URLTab(_URL_REPORTS + "Custom.aspx", "Judgments"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))  ' D0707
            AddPage(New clsPageAction(_PGID_REPORT_CUSTOM_OBJ_PRIORITY, "ReportCustomObjPrty", URLTab(_URL_REPORTS + "Custom.aspx", "tab3"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))      ' D4677
            AddPage(New clsPageAction(_PGID_REPORT_CUSTOM_EVAL_PRG, "ReportCustomEvalPrg", URLTab(_URL_REPORTS + "Custom.aspx", "tab4"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))          ' D4677
            AddPage(New clsPageAction(_PGID_REPORT_CUSTOM_INCONSIST, "ReportCustomInconsist", URLTab(_URL_REPORTS + "Custom.aspx", "tab5"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))       ' D4677
            AddPage(New clsPageAction(_PGID_SYNTHESIZE_INCONSIST, "ReportCustomInconsist", URLTab(_URL_REPORTS + "Custom.aspx", "tab7"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))          ' D4785
            AddPage(New clsPageAction(_PGID_REPORT_CUSTOM_SURVEY, "ReportCustomSurvey", URLTab(_URL_REPORTS + "Custom.aspx", "tab6"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))             ' D4677
            AddPage(New clsPageAction(_PGID_SYNTHESIZE_SURVEY, "ReportCustomSurvey", URLTab(_URL_REPORTS + "Custom.aspx", "tab8"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))                ' D4785

            AddPage(New clsPageAction(_PGID_REPORT_DATAGRID_RISK_TREATMENTS, "ReportDataGridTreatments", _URL_ANALYSIS + "RiskDatagrid.aspx?with_controls=1", ecPagePermission.ppProject, ecActionType.at_mlDownloadModel, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))     ' D3853 + A1222
            AddPage(New clsPageAction(_PGID_REPORT_DATAGRID_RISK, "ReportDataGridRisk", _URL_ANALYSIS + "RiskDatagrid.aspx", ecPagePermission.ppProject, ecActionType.at_mlDownloadModel, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))     ' D2310 + A1222
            AddPage(New clsPageAction(_PGID_REPORT_DATAGRID, "ReportDataGrid", _URL_EVALUATE + "DataGrid.aspx?mode=synth", ecPagePermission.ppProject, ecActionType.at_mlDownloadModel, ecPageAccessType.paOnlyProjects))     ' D1559 + D2239
            AddPage(New clsPageAction(_PGID_REPORT_DATAGRID2, "ReportDataGrid2", _URL_EVALUATE + "DataGrid2.aspx", ecPagePermission.ppProject, ecActionType.at_mlDownloadModel, ecPageAccessType.paOnlyProjects))     ' A1588
            AddPage(New clsPageAction(_PGID_REPORT_DATAGRID_NOUPLOAD, "ReportDataGrid", _URL_EVALUATE + "DataGrid.aspx?upload=0", ecPagePermission.ppProject, ecActionType.at_mlDownloadModel, ecPageAccessType.paOnlyProjects))     ' D3853
            AddPage(New clsPageAction(_PGID_REPORT_DATAGRID_UPLOAD, "ReportDataGridUpload", _URL_EVALUATE + "UploadDataGrid.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))     ' D1492 + D2239
            AddPage(New clsPageAction(_PGID_REPORT_DATA_MAPPING, "DataMapping", _URL_STRUCTURE + "DataMapping.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))     ' D4087

            AddPage(New clsPageAction(_PGID_REPORT_COMBINED, "CombinedReport", _URL_ANALYSIS + "CombinedReport.aspx", ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects))     ' D6239

            AddPage(New clsPageAction(_PGID_REPORT_GET_DATA, "ReportGetData", _URL_REPORTS + "GetData.aspx", ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects)) ' D2556
            AddPage(New clsPageAction(_PGID_REPORT_CSV, "ReportGetData", URLTab(_URL_REPORTS + "Custom.aspx", "csv"), ecPagePermission.ppProject, ecActionType.at_mlUsePredefinedReports, ecPageAccessType.paOnlyProjects)) ' D6299

            AddPage(New clsPageAction(_PGID_REPORT_MAXOUT, "ReportMaxOut", _URL_REPORTS + "MaxOut.aspx", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paEveryone))  ' D0138 + D0387

            ' === analysis ===================================
            'AddPage(New clsPageAction(_PGID_ANALYSIS_DYNAMIC, "DynamicSA", URLWithAction(_URL_ANALYSIS + "SA.aspx", "dynamic"), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))  ' D0062 + D0118
            'AddPage(New clsPageAction(_PGID_ANALYSIS_PERFORMANCE, "PerformanceSA", URLWithAction(_URL_ANALYSIS + "SA.aspx", "performance"), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))  ' D0118
            'AddPage(New clsPageAction(_PGID_ANALYSIS_GRADIENT, "GradientSA", URLWithAction(_URL_ANALYSIS + "SA.aspx", "gradient"), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))  ' D0118
            'AddPage(New clsPageAction(_PGID_ANALYSIS_ADVANCED, "AdvancedAnalysis", _URL_ANALYSIS + "Advanced.aspx", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))  ' D0311

            ' D3564 ===
            AddPage(New clsPageAction(_PGID_SYNTHESIS_RESULTS, "MixedHTML", _URL_ANALYSIS + "Synthesis.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))  ' A1023
            AddPage(New clsPageAction(_PGID_DASHBOARDS, "Dashboard", _URL_ANALYSIS + "Synthesis.aspx?view=dashboard", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))  ' A1491
            AddPage(New clsPageAction(_PGID_ANALYSIS_OVERALL_ALTS, "OverallResultsAlts", _URL_ANALYSIS + "Synthesis.aspx?view=altgrid", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_ANALYSIS_OVERALL_OBJS, "OverallResultsObj", _URL_ANALYSIS + "Synthesis.aspx?view=objgrid", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_ANALYSIS_OVERALL_ALTS, "OverallResultsAlts", URLPage(_URL_ANALYSIS + "Grids.aspx", _PGID_ANALYSIS_OVERALL_ALTS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects)) ' D6258
            AddPage(New clsPageAction(_PGID_ANALYSIS_OVERALL_OBJS, "OverallResultsObj", URLPage(_URL_ANALYSIS + "Grids.aspx", _PGID_ANALYSIS_OVERALL_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))  ' D6258
            AddPage(New clsPageAction(_PGID_ANALYSIS_OVERALL_ALTS_CHART, "OverallResultsAltsChart", _URL_ANALYSIS + "Synthesis.aspx?view=altchart", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects)) 'A1058
            AddPage(New clsPageAction(_PGID_ANALYSIS_OVERALL_OBJS_CHART, "OverallResultsObjChart", _URL_ANALYSIS + "Synthesis.aspx?view=objchart", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects)) 'A1058
            AddPage(New clsPageAction(_PGID_ANALYSIS_DSA, "DynamicSA", _URL_ANALYSIS + "Synthesis.aspx?view=dsa", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))     ' D0556
            AddPage(New clsPageAction(_PGID_ANALYSIS_PSA, "PerformanceSA", _URL_ANALYSIS + "Synthesis.aspx?view=psa", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects)) ' D0556
            AddPage(New clsPageAction(_PGID_ANALYSIS_GSA, "GradientSA", _URL_ANALYSIS + "Synthesis.aspx?view=gsa", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))    ' D0556
            AddPage(New clsPageAction(_PGID_ANALYSIS_2D, "2DAnalysis", _URL_ANALYSIS + "Synthesis.aspx?view=2d", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))      ' D0660
            AddPage(New clsPageAction(_PGID_ANALYSIS_HEAD2HEAD, "AnalysisH2H", _URL_ANALYSIS + "Synthesis.aspx?view=h2h", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))      ' D0660
            AddPage(New clsPageAction(_PGID_ANALYSIS_CONSENSUS, "ConsensusView", _URL_ANALYSIS + "Synthesis.aspx?view=cv", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_ANALYSIS_ASA, "ASA", _URL_ANALYSIS + "Synthesis.aspx?view=asa", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))    ' A1216
            AddPage(New clsPageAction(_PGID_ANALYSIS_4ASA, "4ASA", _URL_ANALYSIS + "Synthesis.aspx?view=4asa", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))      ' A1216
            AddPage(New clsPageAction(_PGID_ANALYSIS_MIXED, "4SA", _URL_ANALYSIS + "Synthesis.aspx?view=4sa", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects))      ' A1450
            AddPage(New clsPageAction(_PGID_ANALYSIS_DASHBOARD, "Dashboard", _URL_ANALYSIS + "Dashboard.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))      ' D6536
            AddPage(New clsPageAction(_PGID_ANALYSIS_CHARTS_ALTS, "OverallResultsAltsChart", URLPage(_URL_ANALYSIS + "Charts.aspx", _PGID_ANALYSIS_CHARTS_ALTS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))   'A1058 + D6258 + D6536
            AddPage(New clsPageAction(_PGID_ANALYSIS_CHARTS_OBJS, "OverallResultsObjChart", URLPage(_URL_ANALYSIS + "Charts.aspx", _PGID_ANALYSIS_CHARTS_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))    'A1058 + D6258 + D6536
            AddPage(New clsPageAction(_PGID_ANALYSIS_GRIDS, "OverallResultsGrids", _URL_ANALYSIS + "Grids.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects)) 'A1732
            AddPage(New clsPageAction(_PGID_ANALYSIS_GRIDS_ALTS, "OverallResultsAlts", URLPage(_URL_ANALYSIS + "Grids.aspx", _PGID_ANALYSIS_GRIDS_ALTS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects)) ' D6258
            AddPage(New clsPageAction(_PGID_ANALYSIS_GRIDS_OBJS, "OverallResultsObj", URLPage(_URL_ANALYSIS + "Grids.aspx", _PGID_ANALYSIS_GRIDS_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))  ' D6258
            AddPage(New clsPageAction(_PGID_EXPORT_CHARTS_ALTS, "ExportAltsChart", URLPage(_URL_ANALYSIS + "Export.aspx", _PGID_EXPORT_CHARTS_ALTS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))   ' D6470
            AddPage(New clsPageAction(_PGID_EXPORT_CHARTS_OBJS, "ExportObjsChart", URLPage(_URL_ANALYSIS + "Export.aspx", _PGID_EXPORT_CHARTS_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects))   ' D6470
            ' D3564 ==

            'A1523 ===
            AddPage(New clsPageAction(_PGID_RISK_DSA_LIKELIHOOD, "RiskDSALikelihood", _URL_ANALYSIS + "Synthesis.aspx?view=dsa&hid=0", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_DSA_IMPACT, "RiskDSAImpact", _URL_ANALYSIS + "Synthesis.aspx?view=dsa&hid=2", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_DSA_LIKELIHOOD_WITH_CONTROLS, "RiskWCDSALikelihood", _URL_ANALYSIS + "Synthesis.aspx?view=dsa&hid=0&controls=1", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_DSA_IMPACT_WITH_CONTROLS, "RiskWCDSAImpact", _URL_ANALYSIS + "Synthesis.aspx?view=dsa&hid=2&controls=1", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A1523 ==
            AddPage(New clsPageAction(_PGID_RISK_PSA_LIKELIHOOD, "RiskPSALikelihood", _URL_ANALYSIS + "Synthesis.aspx?view=psa&hid=0", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_PSA_IMPACT, "RiskPSAImpact", _URL_ANALYSIS + "Synthesis.aspx?view=psa&hid=2", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_PSA_LIKELIHOOD_WITH_CONTROLS, "RiskWCPSALikelihood", _URL_ANALYSIS + "Synthesis.aspx?view=psa&hid=0&controls=1", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_PSA_IMPACT_WITH_CONTROLS, "RiskWCPSAImpact", _URL_ANALYSIS + "Synthesis.aspx?view=psa&hid=2&controls=1", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))

            ' D2256 ===
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_GRID, "RiskGrid", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_GRID), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))       ' D2040
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_CHART, "RiskChart", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_CHART), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))    ' D2040
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_IMPACT_GRID, "RiskGridImpact", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_IMPACT_GRID), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))    ' A0750
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_IMPACT_CHART, "RiskChartImpact", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_IMPACT_CHART), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly)) ' A0750
            ' D2256 ==

            'A0783 ===
            'AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS_4PIPE, "RiskGridOverall", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS), ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' D3365 + D6772
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS, "RiskGridOverall", _URL_ANALYSIS + "RiskRegister.aspx?mode=overall", ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' D3365
            'AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES, "RiskGridSpecCauses", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS, "RiskGridSpecObjs", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES, "RiskGridSpecCauses", _URL_ANALYSIS + "RiskRegister.aspx?mode=sources", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS, "RiskGridSpecObjs", _URL_ANALYSIS + "RiskRegister.aspx?mode=objectives", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))

            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES, "RiskGridAllCauses", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_ALL_OBJS, "RiskGridAllObjs", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_ALL_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, "RiskGridWithContorlsOverall", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, "RiskGridWithContorlsOverall", _URL_ANALYSIS + "RiskRegister.aspx?mode=overall_wc", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A0783 ==
            'A0801 ===
            'AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, "RiskWithControlsGridLikelihood", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS, "RiskWithControlsGridImpact", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, "RiskWithControlsGridLikelihood", _URL_ANALYSIS + "RiskRegister.aspx?mode=sources_wc", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS, "RiskWithControlsGridImpact", _URL_ANALYSIS + "RiskRegister.aspx?mode=objectives_wc", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A0801 ==
            'A1407 ===
            AddPage(New clsPageAction(_PGID_RISK_EVENT_GROUPS, "EventsGroups", _URL_RA + "Groups.aspx?tab=events", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_RISK_THREAT_GROUPS, "ThreatGroups", _URL_RA + "Groups.aspx?tab=causes", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_RISK_EVENT_DEPENDENCIES, "EventDependencies", _URL_STRUCTURE + "EventDependencies.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))
            'A1407 ==
            AddPage(New clsPageAction(_PGID_RISK_SIMULATION_GROUPS, "SimulationGroups", _URL_STRUCTURE + "SimulationGroups.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))
            'A0807 ===
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES, "RiskWithControlsGridAllCauses", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS, "RiskWithControlsGridAllObjs", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A0807 ==
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_BY_EVENT, "RiskGridByEvent", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_BY_EVENT), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))    'A0778
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_IMPACT_BY_EVENT, "RiskGridByEventImpact", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_ANALYSIS_RISK_RESULTS_IMPACT_BY_EVENT), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))    'A0778
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT, "RiskExtraPlot", _URL_ANALYSIS + "RiskPlot.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))    'A1231
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES, "RiskExtraPlotFromSources", _URL_ANALYSIS + "RiskPlot.aspx?mode=sources", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS, "RiskExtraPlotToObjs", _URL_ANALYSIS + "RiskPlot.aspx?mode=objectives", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS, "RiskExtraPlotWC", _URL_ANALYSIS + "RiskPlot.aspx?can_use_controls=1", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES, "RiskExtraPlotWCFromSources", _URL_ANALYSIS + "RiskPlot.aspx?mode=sources&can_use_controls=1", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS, "RiskExtraPlotWCToObjs", _URL_ANALYSIS + "RiskPlot.aspx?mode=objectives&can_use_controls=1", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A0871 ===
            'AddPage(New clsPageAction(_PGID_RISK_INVITE_USERS, "RiskInviteUsers", _URL_ANALYSIS + "RiskInviteUsers.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_INVITE_USERS, "RiskInviteUsers", _URL_PROJECT + "Users.aspx" + "?mode=controls", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly)) 'A1444
            AddPage(New clsPageAction(_PGID_RISK_SELECT_TREATMENTS, "RiskTreatments", _URL_ANALYSIS + "RiskTreatments.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_REPORT_CAUSES, "RiskTreatmentsReport", _URL_ANALYSIS + "RiskTreatmentsReport.aspx?mode=1", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_REPORT_VULNERABILITIES, "RiskTreatmentsReport", _URL_ANALYSIS + "RiskTreatmentsReport.aspx?mode=2", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_REPORT_CONSEQENCES, "RiskTreatmentsReport", _URL_ANALYSIS + "RiskTreatmentsReport.aspx?mode=3", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_ROLES, "RiskRoles", _URL_ANALYSIS + "RiskRoles.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageModelUsers, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_SOURCE_GROUPS, "RiskSourceGroups", _URL_STRUCTURE + "RiskGroups.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects))
            'A0871 ==
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MAP_CAUSES, "RiskControlsMapCauses", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MAP_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MAP_VULNERABILITIES, "RiskByEvent", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MAP_VULNERABILITIES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MAP_CONSEQUENCES, "RiskByEvent", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MAP_CONSEQUENCES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MAP_VULNERABILITIES_BY_TREATMENT, "RiskByControl", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MAP_VULNERABILITIES_BY_TREATMENT), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MAP_CONSEQUENCES_BY_TREATMENT, "RiskByControl", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MAP_CONSEQUENCES_BY_TREATMENT), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MEASURE_CAUSES, "RiskControlsMeasureCauses", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MEASURE_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES, "RiskByEvent", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES, "RiskByEvent", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES_BY_TREATMENT, "RiskByControl", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES_BY_TREATMENT), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES_BY_TREATMENT, "RiskByControl", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES_BY_TREATMENT), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_EFFECT_CAUSES, "RiskControlsEffectCauses", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_EFFECT_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES, "RiskByEvent", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES, "RiskByEvent", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES_BY_TREATMENT, "RiskByControl", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES_BY_TREATMENT), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES_BY_TREATMENT, "RiskByControl", URLPage(_URL_ANALYSIS + "RiskTreatmentsMap.aspx", _PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES_BY_TREATMENT), ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))            
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_OVERALL, "RiskControlsOptimizeOverall", _URL_ANALYSIS + "RiskTreatments.aspx?mode=solve&pgid=77202", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_TIME_PERIODS_VIEW, "RiskControlsOptimizeTP", _URL_ANALYSIS + "RiskTreatments.aspx?mode=solve&pgid=77210", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_FROM_SOURCES, "RiskControlsOptimizeFromSources", _URL_ANALYSIS + "RiskTreatments.aspx?mode=solve&pgid=77203", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_TO_OBJS, "RiskControlsOptimizeToObjs", _URL_ANALYSIS + "RiskTreatments.aspx?mode=solve&pgid=77204", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_DEPENDENCIES, "RODependencies", _URL_RA + "Dependencies.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' A1220
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_GROUPS, "ROGroups", _URL_RA + "Groups.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' A1220
            'AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_OVERALL, "ROIncreasingBudgetsOverall", _URL_RA + "EfficientFrontier.aspx?pgid=77996", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' A1229
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_OVERALL, "ROIncreasingBudgetsOverall", _URL_RA + UrlPage("IncreasingBudgets2.aspx", _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_OVERALL), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' A1229
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES, "ROIncreasingBudgetsFromSources", UrlPage(_URL_RA + "IncreasingBudgets2.aspx", _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_TO_OBJS, "ROIncreasingBudgetsToObjs", _URL_RA + UrlPage("IncreasingBudgets2.aspx", _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_TO_OBJS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_PLOT, "ROPlotAlternatives", _URL_RA + "PlotAlternatives.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' A1375
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_FUNDING_POOLS, "RAFundingPools", _URL_RA + "FundingPools.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly)) 'A1389
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_CUSTOM_CONSTRAINTS, "RACustomConstr", _URL_RA + "CustomConstraints.aspx", ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly)) 'A1389
            AddPage(New clsPageAction(_PGID_RISK_OPTIMIZER_TIME_PERIODS, "RATimeperiods", URLPage(_URL_RA + "Timeline.aspx", _PGID_RISK_OPTIMIZER_TIME_PERIODS), ecPagePermission.ppProject, ecActionType.at_mlViewOverallResults, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly)) 'A1389 + D6031
            AddPage(New clsPageAction(_PGID_RISK_CONTROLS_DATAGRID, "RiskControlsDataGrid", _URL_ANALYSIS + "RiskTreatmentsGrid.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_STANDALONE_REDUCTIONS_GRID, "SAReduction", _URL_ANALYSIS + "SAReduction.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyMeasurementScales, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))

            'A0877 ===
            AddPage(New clsPageAction(_PGID_RISK_PLOT_OVERALL, "RiskPlotOverall", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_PLOT_OVERALL), ecPagePermission.ppProject, ecActionType.atUnspecified, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))  ' D6663
            AddPage(New clsPageAction(_PGID_RISK_PLOT_CAUSES, "RiskPlotCauses", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_PLOT_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_PLOT_OBJECTIVES, "RiskPlotObjectives", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_PLOT_OBJECTIVES), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_PLOT_OVERALL_WITH_CONTROLS, "RiskPlotOverallWT", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_PLOT_CAUSES_WITH_CONTROLS, "RiskWithControlsPlotCauses", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS, "RiskWithControlsPlotObjectives", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A0877 ==
            'A0885 ===
            AddPage(New clsPageAction(_PGID_RISK_BOW_TIE, "BowTiePriorities", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_BOW_TIE), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_BOW_TIE_CAUSES, "BowTiePrioritiesCauses", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_BOW_TIE_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_BOW_TIE_OBJS, "BowTiePrioritiesObjs", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_BOW_TIE_OBJS), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_BOW_TIE_DEFINE_CONTROLS, "RiskBowTieDefineControls", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_BOW_TIE_DEFINE_CONTROLS), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_BOW_TIE_WITH_CONTROLS, "RiskWithControlsBowTiePriorities", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_BOW_TIE_WITH_CONTROLS), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES, "RiskWithControlsBowTiePrioritiesCauses", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS, "RiskWithControlsBowTiePrioritiesObjs", URLPage(_URL_ANALYSIS + "RiskResults.aspx", _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS), ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A0885 ==
            'A1161 ===
            AddPage(New clsPageAction(_PGID_MAP_DIAGRAM_VIEW_RISK, "RiskDiagramView", URLPage(_URL_STRUCTURE + "DiagramView.aspx", _PGID_MAP_DIAGRAM_VIEW_RISK), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK, "RiskDiagramViewByObj", URLPage(_URL_STRUCTURE + "DiagramView.aspx", _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK), ecPagePermission.ppProjectWithLock, ecActionType.at_mlModifyModelHierarchy, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A1161 ==
            'A1061 ===
            AddPage(New clsPageAction(_PGID_RISK_REGISTER_DEFAULT, "RiskRegisterEventsDefault", _URL_ANALYSIS + "RiskRegister.aspx", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly)) 'A1453
            AddPage(New clsPageAction(_PGID_RISK_REGISTER_WITH_CONTROLS, "RiskRegisterEvents", _URL_ANALYSIS + "RiskRegister.aspx?mode=withcontrols", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_REGISTER, "RiskRegisterTreatments", _URL_ANALYSIS + "RiskRegister.aspx?mode=treatments", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            AddPage(New clsPageAction(_PGID_RISK_ACCEPTANCE_REGISTER, "RiskRegisterAcceptance", _URL_ANALYSIS + "RiskRegister.aspx?mode=acc", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly))
            'A1061 ==
            AddPage(New clsPageAction(_PGID_RISK_TREATMENTS_DICTIONARY, "RiskTreatmentsDict", _URL_ANALYSIS + "RiskTreatments.aspx?mode=edit", ecPagePermission.ppProject, ecActionType.at_mlPerformSensitivityAnalysis, ecPageAccessType.paOnlyProjects, ecPageRiskAccessType.raRiskOnly)) 'A1353

            ' === Surveys ====================================
            ' -D2780
            'AddPage(New clsPageAction(_PGID_SURVEY_LIST, "SurveyList", _URL_SURVEYS + "SurveyList.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alManageSurveys, ecPageAccessType.paOnlyProjects))  ' D0225
            AddPage(New clsPageAction(_PGID_SURVEY_EDIT_PRE, "SurveyEdit", URLParameter(_URL_SURVEYS + "SurveyEdit.aspx", "type", "pre"), ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))  ' D0190 + D0225 + D1962
            AddPage(New clsPageAction(_PGID_SURVEY_EDIT_POST, "SurveyEditPost", URLParameter(_URL_SURVEYS + "SurveyEdit.aspx", "type", "post"), ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))  ' D0190 + D0225 + D1962
            AddPage(New clsPageAction(_PGID_SURVEY_RESULTS, "SurveyResults", _URL_SURVEYS + "ViewResults.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))  'L0449 + D1962
            AddPage(New clsPageAction(_PGID_SURVEY_QUESTION_EDIT, "SurveyEditQuestion", _URL_SURVEYS + "QuestionEdit.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))  ' D0225 + D1962
            AddPage(New clsPageAction(_PGID_SURVEY_QUESTION_NEW, "SurveyNewQuestion", _URL_SURVEYS + "NewQuestion.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))  ' D0385 + D1962
            AddPage(New clsPageAction(_PGID_SURVEY_QUESTION_EDITOR, "SurveyQuestionEditor", _URL_SURVEYS + "QuestionEditor.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))  ' D0644 + D1962
            AddPage(New clsPageAction(_PGID_SURVEY_GROUP_FILTER, "SurveyGroupFilter", _URL_SURVEYS + "GroupFilter.aspx", ecPagePermission.ppProject, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))  ' D0644 + D1962
            AddPage(New clsPageAction(_PGID_SURVEY_DOWNLOAD, "SurveyDownload", _URL_SURVEYS + "Download.aspx", ecPagePermission.ppProject, ecActionType.at_mlDownloadModel, ecPageAccessType.paOnlyProjects))  ' D0297 + D1962
            AddPage(New clsPageAction(_PGID_SURVEY_UPLOAD, "SurveyUpload", _URL_SURVEYS + "Upload.aspx", ecPagePermission.ppProjectWithLock, ecActionType.at_mlManageProjectOptions, ecPageAccessType.paOnlyProjects))  ' D1459 + D1962

            ' === RA =========================================
            AddPage(New clsPageAction(_PGID_RA_BASE, "RABase", _URL_RA + "Default.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' D2699
            AddPage(New clsPageAction(_PGID_RA_CUSTOM_CONSTRAINTS, "RACustomConstr", _URL_RA + "CustomConstraints.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' D3257
            AddPage(New clsPageAction(_PGID_RA_DEPENDENCIES, "RADependencies", _URL_RA + "Dependencies.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' A2844
            AddPage(New clsPageAction(_PGID_RA_FUNDINGPOOLS, "RAFundingPools", _URL_RA + "FundingPools.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' A2844
            AddPage(New clsPageAction(_PGID_RA_GROUPS, "RAGroups", _URL_RA + "Groups.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' D2844
            AddPage(New clsPageAction(_PGID_RA_INC_BUDGETS, "RAIncreasingBudgets", _URL_RA + "IncreasingBudgets2.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' A0856
            AddPage(New clsPageAction(_PGID_RA_PLOT_ALTS, "RAPlotAlternatives", _URL_RA + "PlotAlternatives.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' D2844
            AddPage(New clsPageAction(_PGID_RA_STRATEGIC_BUCKETS, "RAStrategicBuckets", _URL_RA + "StrategicBuckets.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' D2844
            AddPage(New clsPageAction(_PGID_RA_REPORT, "RAReport", _URL_RA + "Report.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' D3194
            AddPage(New clsPageAction(_PGID_RA_REPORT_MODEL_SPEC, "RAReportModelSpec", _URL_RA + "Report.aspx?mode=0", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' A0969
            AddPage(New clsPageAction(_PGID_RA_REPORT_CONSTR_SUMMARY, "RAReportCCSum", _URL_RA + "Report.aspx?mode=1", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' A0969
            AddPage(New clsPageAction(_PGID_RA_REPORT_RELEVANT_CONSTR, "RAReportRelConstr", _URL_RA + "Report.aspx?mode=2", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' A0969
            AddPage(New clsPageAction(_PGID_RA_REPORT_SOLVER_REPORT, "RAReport", _URL_RA + "Report.aspx?mode=3", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' A0969
            AddPage(New clsPageAction(_PGID_RA_SCENARIOS, "RAScenarios", _URL_RA + "Scenarios.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))  ' A0878
            AddPage(New clsPageAction(_PGID_RA_TIMEPERIODS_SETTINGS, "RATimeperiods", _URL_RA + "Timeline.aspx", ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))
            'AddPage(New clsPageAction(_PGID_RA_PROJECT_RESOURCES, "RAResources", URLPage(_URL_RA + "Timeline.aspx", _PGID_RA_PROJECT_RESOURCES), ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))
            AddPage(New clsPageAction(_PGID_RA_PERIOD_RESULTS, "RAPeriods", URLPage(_URL_RA + "Timeline.aspx", _PGID_RA_PERIOD_RESULTS), ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))
            'AddPage(New clsPageAction(_PGID_RA_PORTFOLIO_RESOURCES, "RAMinMax", URLPage(_URL_RA + "Timeline.aspx", _PGID_RA_PORTFOLIO_RESOURCES), ecPagePermission.ppProject, ecActionType.at_mlModifyAlternativeHierarchy, ecPageAccessType.paOnlyProjects))

            ' === Admins =====================================
            'AddPage(New clsPageAction(_PGID_ADMIN_WELCOME, "AdminWelcome", _URL_ADMIN + "Welcome.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paEveryone))  ' D0089
            '-A1510 AddPage(New clsPageAction(_PGID_ADMIN_USERSLIST, "WorkgroupUsersList", _URL_ADMIN + "Users.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alManageWorkgroupUsers, ecPageAccessType.paEveryone))  ' D0087 + D0091 + D0727            
            AddPage(New clsPageAction(_PGID_ADMIN_USERSLIST, "WorkgroupUsersList", _URL_PROJECT + "Users.aspx?mode=wgusers", ecPagePermission.ppAuthorized, ecActionType.at_alManageWorkgroupUsers, ecPageAccessType.paEveryone))  ' A1495 + A1510
            'AddPage(New clsPageAction(_PGID_WORKGROUP_USERSLIST, "WorkgroupUsersList", _URL_PROJECT + "Users.aspx&mode=wgusers", ecPagePermission.ppAuthorized, ecActionType.at_alManageWorkgroupUsers, ecPageAccessType.paEveryone))  ' A1495
            AddPage(New clsPageAction(_PGID_ADMIN_GENERATE_USER, "AutogeneratingUser", _URL_ADMIN + "SignupDemo.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alManageWorkgroupUsers, ecPageAccessType.paEveryone))  ' D0453 + D0727
            AddPage(New clsPageAction(_PGID_ADMIN_ROLEGROUPS, "RoleGroups", _URL_ADMIN + "RoleGroups.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageAnyWorkgroup, ecPageAccessType.paEveryone))    ' D0064 +  D0087 + D0262 + D0450
            'AddPage(New clsPageAction(_PGID_ADMIN_WORKGROUPS, "Workgroups", _URL_ADMIN + "Workgroups.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageOwnWorkgroup, ecPageAccessType.paEveryone))    ' D0077 + D0087 + D0716
            AddPage(New clsPageAction(_PGID_ADMIN_WORKGROUPS, "Workgroups", _URL_ADMIN + "WkgList.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageOwnWorkgroup, ecPageAccessType.paEveryone))    ' D0077 + D0087 + D0716 + D1346 + D3288 + D6635 + D7269 + D7270
            AddPage(New clsPageAction(_PGID_ADMIN_WORKGROUP_CREATE, "WorkgroupCreate", URLWithAction(_URL_ADMIN + "WorkgroupEdit.aspx", _ACTION_NEW), ecPagePermission.ppAuthorized, ecActionType.at_slCreateWorkgroup, ecPageAccessType.paEveryone))    ' D0091 + D0717
            AddPage(New clsPageAction(_PGID_ADMIN_WORKGROUP_EDIT, "WorkgroupEdit", URLWithAction(_URL_ADMIN + "WorkgroupEdit.aspx", _ACTION_EDIT), ecPagePermission.ppAuthorized, ecActionType.at_slManageOwnWorkgroup, ecPageAccessType.paEveryone))    ' D0091 + D0717
            AddPage(New clsPageAction(_PGID_ADMIN_WORKGROUP_DELETE, "WorkgroupDelete", URLWithAction(_URL_ADMIN + "WkgList.aspx", _ACTION_DELETE), ecPagePermission.ppAuthorized, ecActionType.at_slDeleteOwnWorkgroup, ecPageAccessType.paEveryone))    ' D0091 + D0717 + D1786
            AddPage(New clsPageAction(_PGID_ADMIN_WORKGROUP_TEMPLATES, "WorkgroupTemplates", _URL_ADMIN + "WorkgroupTemplates.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alManageAnyModel, ecPageAccessType.paEveryone))    ' D0360 + D0717 + D1081 + D4833
            AddPage(New clsPageAction(_PGID_ADMIN_WORKGROUP_SAMPLES, "WorkgroupSamples", URLWithParams(_URL_ADMIN + "WorkgroupTemplates.aspx", "type=samples"), ecPagePermission.ppAuthorized, ecActionType.at_alManageAnyModel, ecPageAccessType.paEveryone))    ' D1081 + D4833
            AddPage(New clsPageAction(_PGID_ADMIN_WORKGROUP_WORDING_TEMPLATE, "WordingTemplate", _URL_ADMIN + "WordingTemplate.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alManageWorkgroupUsers, ecPageAccessType.paEveryone))    'A1113
            AddPage(New clsPageAction(_PGID_ADMIN_LICENSE, "License", _URL_ADMIN + "License.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slViewLicenseOwnWorkgroup, ecPageAccessType.paEveryone)) ' D0257 + D0302 + D0717 + D3036
            AddPage(New clsPageAction(_PGID_ADMIN_VIEWLOGS, "ViewLogs", _URL_ADMIN + "ViewLogs.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alManageWorkgroupUsers, ecPageAccessType.paEveryone))        ' D0168 + D0302 + D0717 + D6635
            AddPage(New clsPageAction(_PGID_ADMIN_VIEW_SIGNUP, "ViewSignup", _URL_ADMIN + "ViewSignup.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slViewOwnWorkgroupReports, ecPageAccessType.paEveryone))    ' D1174
            AddPage(New clsPageAction(_PGID_ADMIN_ONLINE_USERS, "OnlineUsers", _URL_ADMIN + "BigBrother.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slViewOwnWorkgroupReports, ecPageAccessType.paEveryone))  ' D0181 + D0302 + D0717
            AddPage(New clsPageAction(_PGID_ADMIN_LOGINS_STAT, "WorkgroupStat", _URL_ADMIN + "WkgStat.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageAnyWorkgroup, ecPageAccessType.paEveryone))         ' D0889
            AddPage(New clsPageAction(_PGID_ADMIN_STATISTIC, "Statistic", _URL_ADMIN + "Statistic.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slViewOwnWorkgroupReports, ecPageAccessType.paEveryone))        ' D3526 + D3527

            AddPage(New clsPageAction(_PGID_ADMIN_SERVICEHASH, "ServiceHash", _URL_ADMIN + "Token.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageAnyWorkgroup, ecPageAccessType.paEveryone))         ' D1630
            AddPage(New clsPageAction(_PGID_ADMIN_USER_INFO, "AdminUserInfo", _URL_ADMIN + "UserInfo.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageAnyWorkgroup, ecPageAccessType.paEveryone))      ' D1643
            AddPage(New clsPageAction(_PGID_ADMIN_PRJ_LOOKUP, "AdminPrjLookup", _URL_ADMIN + "ProjectLookup.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alCanBePM, ecPageAccessType.paEveryone))   ' D3442 + D3621 + D4460
            AddPage(New clsPageAction(_PGID_ADMIN_PRJ_STAT, "AdminPrjStat", _URL_ADMIN + "ProjectStat.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alCanBePM, ecPageAccessType.paEveryone))         ' D4577

            ' === system and service pages ===================
            AddPage(New clsPageAction(_PGID_DB_SETUP, "DBSetup", _URL_ROOT + "Install/Default.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))                     ' D0045 + D0371
            AddPage(New clsPageAction(_PGID_DB_DECISIONS, "DBDecisions", _URL_ROOT + "Install/Decisions.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageAnyWorkgroup, ecPageAccessType.paEveryone))   ' D0371 + D0920
            AddPage(New clsPageAction(_PGID_DB_RESTORE, "DBRestore", _URL_ROOT + "Install/Restore.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageAnyWorkgroup, ecPageAccessType.paEveryone))         ' D0796 + D0920
            AddPage(New clsPageAction(_PGID_DB_SQL, "DBSQL", _URL_ROOT + "Install/SQL.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageAnyWorkgroup, ecPageAccessType.paEveryone))         ' D0920
            AddPage(New clsPageAction(_PGID_SYSTEM_MESSAGE, "SystemMessage", _URL_ROOT + "Install/Message.aspx", ecPagePermission.ppAuthorized, ecActionType.at_alManageAnyModel, ecPageAccessType.paEveryone))         ' D0796
            AddPage(New clsPageAction(_PGID_SYSTEM_SETTINGS, "SystemSettings", _URL_ROOT + "Install/Settings.aspx", ecPagePermission.ppAuthorized, ecActionType.at_slManageAnyWorkgroup, ecPageAccessType.paEveryone))      ' D3821

            AddPage(New clsPageAction(_PGID_ERROR_403, "Error403", _URL_ROOT + "Default.aspx?error=403", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))
            AddPage(New clsPageAction(_PGID_ERROR_404, "Error404", _URL_ROOT + "404.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))   ' D0116
            AddPage(New clsPageAction(_PGID_ERROR_500, "Error500", _URL_ROOT + "Error.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))   ' D0116
            AddPage(New clsPageAction(_PGID_ERROR_503, "Error503", _URL_ROOT + "License.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))   ' D0257

            'AddPage(New clsPageAction(_PGID_FEEDBACK, "Feedback", WebConfigOption(WebOptions._OPT_FOGBUGZ_FEEDBACK), ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))   ' D0118 - D0883

            AddPage(New clsPageAction(_PGID_JS_RESOURCES, "JSResources", _URL_ROOT + "Scripts/Resources/Default.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone))   ' D4629

            AddPage(New clsPageAction(_PGID_SILVERLIGHT_UI, "SilverlightUI", _URL_ROOT + "Comparion.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone)) ' D0652 + D0718
            AddPage(New clsPageAction(_PGID_SERVICEPAGE, "SilverlightService", _URL_ROOT + "ServicePage.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone)) ' D0725

            AddPage(New clsPageAction(_PGID_WEBAPI, "WebAPI", _WEBAPI_ROOT, ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone)) ' D5024
            AddPage(New clsPageAction(_PGID_WEBAPI_HELP, "WebAPIHelp", _WEBAPI_ROOT + "Default.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone)) ' D7222 + D7584

            'AddPage(New clsPageAction(_PGID_TEST_SILVERLIGHT, "CheckSL", _URL_ROOT + "SilverLightTest.aspx", ecPagePermission.ppAuthorized, ecActionType.atUnspecified, ecPageAccessType.paEveryone)) ' D0529 -D1399
            AddPage(New clsPageAction(_PGID_TEST, "Test", _URL_ROOT + "Test/", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone)) ' D0326
            AddPage(New clsPageAction(_PGID_PING, "Ping", _URL_ROOT + "Ping.aspx", ecPagePermission.ppEveryone, ecActionType.atUnspecified, ecPageAccessType.paEveryone)) ' D3987

            AddPage(New clsPageAction(_PGID_UNKNOWN, "Unknown", _URL_ROOT, ecPagePermission.ppUnspecified, ecActionType.atUnspecified, ecPageAccessType.paUnspecified))
        End Sub

        ' D0219 ===
        Public Function ApplicationURL(isEvaluationSite As Boolean, fIsTeamTime As Boolean) As String   ' D3494
            Dim sURL As String = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)
            ' D6020 ===
            Dim TT As Boolean = If(App.isRiskEnabled, App.Options.EvalURL4TeamTime_Riskion, App.Options.EvalURL4TeamTime)
            Dim AT As Boolean = If(App.isRiskEnabled, App.Options.EvalURL4Anytime_Riskion, App.Options.EvalURL4Anytime)
            If isEvaluationSite AndAlso App.Options.EvalSiteURL <> "" AndAlso ((TT AndAlso fIsTeamTime) OrElse (AT AndAlso Not fIsTeamTime)) Then sURL = App.Options.EvalSiteURL ' D3308 + D3494 + D6016
            ' D6020 ==
            'If Not Request.Url.IsDefaultPort Then sURL = String.Concat(sURL, ":", Request.Url.Port)    '-D0345 (double port)
            Return sURL.Trim(CChar("/")) ' D4925
        End Function
        ' D0219 ==

        Public ReadOnly Property Pages() As List(Of clsPageAction)
            Get
                If _PagesList Is Nothing Then
                    _PagesList = New List(Of clsPageAction)
                    LoadPagesList()
                    ' D5057 ===
                    For i As Integer = 0 To _PagesList.Count - 2
                        Dim sURL As String = _PagesList(i).URL.ToLower
                        For j As Integer = i + 1 To _PagesList.Count - 1
                            If sURL = _PagesList(j).URL.ToLower AndAlso _PagesList(i).RiskAccess = _PagesList(j).RiskAccess Then
                                Select Case _PagesList(j).ID
                                    Case _PGID_ERROR_403, _PGID_WEBAPI
                                    Case Else
                                        Debug.WriteLine("Page URL for #{0} and #{1} are equal: '{2}'", _PagesList(i).ID, _PagesList(j).ID, sURL)  ' D5066
                                        'If Not _PagesList(j).URL.Contains(_PARAM_NAV_PAGE + "=") Then
                                        '    _PagesList(j).URL = URLNavPage(_PagesList(j).URL, _PagesList(j).ID)
                                        'End If
                                End Select
                            End If
                        Next
                    Next
                    ' D5057 ==
                End If
                Return _PagesList
            End Get
        End Property

        ' D0466 ===
        'Public Function AddPage(ByVal NewPage As clsPageAction) As Boolean
        '    Dim fAdded As Boolean = False
        '    If Not NewPage Is Nothing Then
        '        For Each tPg As clsPageAction In Pages
        '            If tPg.ID = NewPage.ID Then
        '                Throw New System.Exception(String.Format("Duplicate ID '{0}' for Application Page Actions. Existed Page: '{1}', new page: '{2}'", NewPage.ID, tPg.Name, NewPage.Name))
        '            End If
        '        Next
        '        Pages.Add(NewPage)
        '    End If
        '    Return fAdded
        'End Function

        Public Sub AddPage(ByVal NewPage As clsPageAction)
            Pages.Add(NewPage)
        End Sub
        ' D0466 ==

        Public Property CurrentPageID() As Integer ' D0466
            Get
                Return _CurrentPageID
            End Get
            Set(ByVal value As Integer)
                If _CurrentPageID <> value Then
                    _CurrentPageID = value
                    If _CurrentPageID <> _PGID_UNKNOWN Then CheckPermissions()
                End If
            End Set
        End Property

        ' D0088 ===
        Public Property NavigationPageID() As Integer   ' D0466
            Get
                If _NavigationPageID <= 0 Then Return CurrentPageID Else Return _NavigationPageID
            End Get
            Set(ByVal value As Integer)
                _NavigationPageID = value
            End Set
        End Property
        ' D0088 ==

        Public Property GetPageName() As String ' D6027
            Get
                ' D4994 ===
                Dim sTitle As String = ""
                If _sOptionalPageTitle <> "" Then
                    sTitle = _sOptionalPageTitle
                Else
                    If PageAction(CurrentPageID) IsNot Nothing Then
                        sTitle = PageTitle(CurrentPageID) ' D0045
                        If sTitle.IndexOf("(!)") >= 0 Then sTitle = PageMenuItem(CurrentPageID)
                    End If
                End If
                Return sTitle
                ' D4994 ==
            End Get
            Set(ByVal value As String)
                _sOptionalPageTitle = value
            End Set
        End Property

        ' D4851 ===
        Public Function GetPageTitle(Optional sCustomTitle As String = Nothing) As String    ' D4995 + D6027
            Dim sPage As String = GetPageName
            If Not String.IsNullOrEmpty(sCustomTitle) Then sPage = sCustomTitle ' D6027
            If App.HasActiveProject Then
                sPage = String.Format(ResString("titleProject"), sPage, SafeFormString(ShortString(App.ActiveProject.ProjectName, 25, True))) ' D956
                If App.isRiskEnabled Then sPage += String.Format(" [{0}]", If(App.ActiveProject.isImpact, "I", "L")) ' D6040
            End If
            If sPage = ApplicationName Then Return sPage  ' D5003
            Return If(sPage = "", ApplicationName, String.Format(ResString("titleWindow"), sPage, ApplicationName)).Replace("&amp;", "&") ' D4956
        End Function
        ' D4851 ==

        ' D0043 ===
        Public Sub SetPageTitle(ByVal sTitle As String)
            _sOptionalPageTitle = sTitle
        End Sub
        ' D0043 ==

        Public Function TextPageLink(ByVal PageID As Integer, Optional ByVal tProject As clsProject = Nothing, Optional ByVal sURL As String = "", Optional ByVal sName As String = "", Optional ByVal sOtherLinkParam As String = "", Optional ByVal fHideRestricted As Boolean = False, Optional ByVal sInactiveStyle As String = "inactive", Optional ByVal fByDeafultIsAvailable As Boolean = True) As String   ' D0265
            Dim sLink As String = ""
            Dim pg As clsPageAction = PageByID(PageID)
            Dim fAvailable As Boolean = HasPermission(pg, tProject) And fByDeafultIsAvailable  ' D0265 + D0466
            If fAvailable Or Not fHideRestricted Then
                If sName = "" Then sName = PageMenuItem(PageID)
                If sURL = "" Then sURL = PageURL(PageID)
                sLink = HTMLTextLink(sURL, sName, fAvailable, sOtherLinkParam, sInactiveStyle)  ' D0091 + D0466
            End If
            Return sLink
        End Function

        Public Function PageByID(ByVal PageID As Integer, Optional ByVal PgList As List(Of clsPageAction) = Nothing) As clsPageAction
            If PgList Is Nothing Then PgList = Pages
            If Not PgList Is Nothing Then
                For Each tPage As clsPageAction In PgList
                    If tPage.ID = PageID Then Return tPage
                Next
            End If
            Return Nothing
        End Function


        ' D0652 ===
        Public Function PageByName(ByVal PageName As String, Optional ByVal PgList As List(Of clsPageAction) = Nothing) As clsPageAction
            If PgList Is Nothing Then PgList = Pages
            If Not PgList Is Nothing Then
                PageName = PageName.ToLower
                For Each tPage As clsPageAction In PgList
                    If tPage.Name.ToLower = PageName Then Return tPage
                Next
            End If
            Return Nothing
        End Function
        ' D0652 ==

        Public Property PageAction(ByVal PageID As Integer, Optional ByVal CanVirtual As Boolean = True) As clsPageAction
            Get
                Dim tPage As clsPageAction = PageByID(PageID)
                If tPage Is Nothing AndAlso CanVirtual Then Return New clsPageAction(-1, "", "", ecPagePermission.ppUnspecified, ecActionType.atUnspecified, ecPageAccessType.paUnspecified) Else Return tPage ' D0460
            End Get
            Set(ByVal value As clsPageAction)
                Dim tPage As clsPageAction = PageByID(PageID)
                If tPage Is Nothing Then Pages.Add(value) Else tPage = value
            End Set
        End Property

        Public ReadOnly Property PageTitle(ByVal PageID As Integer) As String
            Get
                Dim tPage As clsPageAction = PageByID(PageID)
                If tPage Is Nothing Then
                    Return ""
                Else
                    ' D2465 ===
                    Dim sName As String = ""
                    If App.isRiskEnabled AndAlso App.HasActiveProject Then
                        ' D5057 ===
                        If App.ActiveProject.isOpportunityModel Then
                            sName = _PGTITLE_PREFIX + tPage.Name + "Opportunity"
                            If Not App.CurrentLanguage.Resources.ParameterExists(sName) Then sName = ""
                        End If
                        ' D5057 ==
                        sName = _PGTITLE_PREFIX + tPage.Name + CStr(IIf(App.ActiveProject.isImpact, "Impact", "Likelihood"))
                        If Not App.CurrentLanguage.Resources.ParameterExists(sName) Then sName = ""
                    End If
                    If sName = "" Then sName = _PGTITLE_PREFIX + tPage.Name
                    Return PrepareTask(ResString(sName))
                    ' D2465 ==
                End If
            End Get
        End Property

        Public ReadOnly Property PageMenuItem(ByVal PageID As Integer) As String
            Get
                Dim sResult As String = ""  ' D2036
                Dim tPage As clsPageAction = PageByID(PageID)
                If tPage Is Nothing Then
                    sResult = PageTitle(PageID) ' D2036
                Else
                    Dim sResName As String = ""
                    ' D2465 ===
                    If App.isRiskEnabled AndAlso App.HasActiveProject Then
                        ' D5057 ===
                        If App.ActiveProject.isOpportunityModel Then
                            sResName = _PGMENU_PREFIX + tPage.Name + "Opportunity"
                            If Not App.CurrentLanguage.Resources.ParameterExists(sResName) Then sResName = ""
                        End If
                        ' D5057 ==
                        sResName = _PGMENU_PREFIX + tPage.Name + CStr(IIf(App.ActiveProject.isImpact, "Impact", "Likelihood"))
                        If Not App.CurrentLanguage.Resources.ParameterExists(sResName) Then sResName = ""
                    End If
                    If sResName = "" Then sResName = _PGMENU_PREFIX + tPage.Name
                    ' D2465 ==
                    sResult = ResString(sResName, True) ' D2036
                    If sResult = sResName Then sResult = PageTitle(PageID) ' D2036
                End If
                Return PrepareTask(sResult) ' D2036
            End Get
        End Property

        Public ReadOnly Property PageURL(ByVal PageID As Integer, Optional ByVal sURIParams As String = "", Optional fCheckEvalSite As Boolean = False) As String   ' D6359
            Get
                Dim tPage As clsPageAction = PageByID(PageID)
                If tPage Is Nothing Then
                    Return _URL_ROOT
                Else
                    ' D0046 ===
                    Dim sURL As String = tPage.URL
                    ' D6359 ===
                    If fCheckEvalSite AndAlso App.isEvalURL_EvalSite AndAlso App.Options.EvalSiteURL <> "" Then ' D9403
                        Select Case PageID
                            Case _PGID_EVALUATION
                                If If(App.isRiskEnabled, App.Options.EvalURL4Anytime_Riskion, App.Options.EvalURL4Anytime) Then
                                    sURL = PageURL(_PGID_SERVICEPAGE, String.Format("{0}={1}", _PARAM_ACTION, "eval_anytime"))
                                End If
                            Case _PGID_TEAMTIME_STATUS
                                If If(App.isRiskEnabled, App.Options.EvalURL4TeamTime_Riskion, App.Options.EvalURL4TeamTime) Then
                                    sURL = PageURL(_PGID_SERVICEPAGE, String.Format("{0}={1}", _PARAM_ACTION, "eval_teamtime"))
                                End If
                        End Select
                    End If
                    ' D6359 ==
                    If sURIParams <> "" Then
                        If sURL.IndexOf("?") >= 0 Then sURL += "&" Else sURL += "?"
                        sURL += sURIParams
                        sURL = sURL.Trim.Replace("??", "?") ' D0226
                    End If
                    ' D0046 ==
                    Return sURL
                End If
            End Get
        End Property
        ' D0045 ==

        ' D5084 ===
        Public Function PageWithExtraParameter(sURIParams As String, Optional sBaseURL As String = Nothing) As String
            Dim URL As String = If(String.IsNullOrEmpty(sBaseURL) AndAlso Request IsNot Nothing AndAlso Request.Url IsNot Nothing, Request.Url.OriginalString, sBaseURL)
            If URL Is Nothing Then URL = ""
            URL += If(URL.IndexOf("?") > 0, "&", "?") + sURIParams.TrimStart(CChar("?")).TrimStart(CChar("&"))
            Return URL
        End Function
        ' D5084 ==

        ' D0075 ===
        Public Overloads Function PagePopupAction(ByVal PageID As Integer, ByVal sURLParams As String, ByVal WinWidth As Integer, ByVal WinHeight As Integer, ByVal fResizeable As Boolean, Optional ByVal fScrollbars As Boolean = False) As String    ' D0433
            Dim sURL As String = PageURL(PageID, sURLParams)
            Return PagePopupAction(sURL, String.Format("win{0}", PageID), WinWidth, WinHeight, fResizeable, fScrollbars) ' D0099 + D0433
        End Function
        ' D0075 ==

        ' D0099 ===
        Public Overloads Function PagePopupAction(ByVal sURL As String, ByVal sWinName As String, ByVal WinWidth As Integer, ByVal WinHeight As Integer, ByVal fResizeable As Boolean, Optional ByVal fScrollbars As Boolean = False) As String    ' D0433
            If sURL = "" Then
                Return "return false"
            Else
                Return String.Format("CreatePopup('{0}', '{1}', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars={5},resizable={2},width={3},height={4}'); return false;", sURL, sWinName, CStr(IIf(fResizeable, "yes", "no")), WinWidth, WinHeight, IIf(fScrollbars, "yes", "no"))  ' D0433 + D1896
            End If
        End Function
        ' D0099 ==

        ' D0459 ===
        Public Function isDraftPage(ByVal tPageID As Integer) As Boolean
            Return HasListPage(_PAGESLIST_DRAFT, tPageID) OrElse (Not _RA_TIMEPERIODS_AVAILABLE AndAlso HasListPage(_PAGESLIST_RA_TIMEPERIODS, tPageID))    ' D3725
        End Function

        Public Function isDraftPage(ByVal tPagesForCheck() As Integer) As Boolean
            Return HasListPage(_PAGESLIST_DRAFT, tPagesForCheck) OrElse (Not _RA_TIMEPERIODS_AVAILABLE AndAlso HasListPage(_PAGESLIST_RA_TIMEPERIODS, tPagesForCheck))    ' D3725
        End Function
        ' D0459 ==

    End Class

End Namespace
