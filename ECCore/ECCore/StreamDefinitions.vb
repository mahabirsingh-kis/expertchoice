Public Module StreamDefinitions 'C0259

    Public Const CHUNK_PROJECT As Integer = 0

    Public Const CHUNK_PROJECT_STORAGE_VERSION As Integer = 100

    Public Const CHUNK_USERS_GROUPS As Integer = 200
    Public Const CHUNK_USER_GROUP As Integer = 201

    Public Const CHUNK_USERS_LIST As Integer = 300
    Public Const CHUNK_USER As Integer = 301

    Public Const CHUNK_COMBINED_GROUPS_USERS As Integer = 400

    Public Const CHUNK_DATA_INSTANCES As Integer = 500
    Public Const CHUNK_DATA_INSTANCE As Integer = 501

    Public Const CHUNK_MEASUREMENT_SCALES As Integer = 600

    Public Const CHUNK_RATINGS_SCALES As Integer = 610
    Public Const CHUNK_RATING_SCALE As Integer = 611
    Public Const CHUNK_RATING_INTENSITIES As Integer = 612
    Public Const CHUNK_RATING_INTENSITY As Integer = 613

    Public Const CHUNK_REGULAR_UTILITY_CURVES As Integer = 620
    Public Const CHUNK_REGULAR_UTILITY_CURVE As Integer = 621

    Public Const CHUNK_ADVANCED_UTILITY_CURVES As Integer = 630
    Public Const CHUNK_ADVANCED_UTILITY_CURVE As Integer = 631
    Public Const CHUNK_AUC_POINTS As Integer = 632

    Public Const CHUNK_STEP_FUNCTIONS As Integer = 640
    Public Const CHUNK_STEP_FUNCTION As Integer = 641
    Public Const CHUNK_STEP_INTERVALS As Integer = 642
    Public Const CHUNK_STEP_INTERVAL As Integer = 643

    Public Const CHUNK_HIERARCHIES As Integer = 700
    Public Const CHUNK_HIERARCHY As Integer = 701
    Public Const CHUNK_HIERARCHY_NODES As Integer = 702
    Public Const CHUNK_HIERARCHY_NODE As Integer = 703

    Public Const CHUNK_ALTERNATIVES_CONTRIBUTION As Integer = 800
    Public Const CHUNK_ALTERNATIVES_CONTRIBUTION_SET As Integer = 801

    Public Const CHUNK_NODES_PERMISSIONS As Integer = 900
    Public Const CHUNK_ALTERNATIVES_PERMISSIONS As Integer = 1000
    Public Const CHUNK_NODE_ALTERNATIVES_PERMISSIONS As Integer = 1001

    Public Const CHUNK_CONTROLS_PERMISSIONS As Integer = 950

    Public Const CHUNK_HIERARCHY_INFODOCS As Integer = 1100 'C0276
    Public Const CHUNK_INFODOC As Integer = 1101 'C0920

    Public Const CHUNK_HIERARCHY_DISABLED_NODES As Integer = 1200 'C0330

    Public Const CHUNK_CUSTOM_ATTRIBUTES As Integer = 1300 'C1020
    Public Const CHUNK_NODE_ATTRIBUTES As Integer = 1301 'C1020
    Public Const CHUNK_USER_ATTRIBUTES As Integer = 1302 'C1020

    Public Const CHUNK_HIERARCHY_JUDGMENTS As Integer = 2000
    Public Const CHUNK_NODE_JUDGMENTS As Integer = 2001

    Public Const DUMMY_SIZE_OF_THE_CHUNK As Integer = 0 'C0262

    Public Const CHUNK_CONTROL_JUDGMENTS As Integer = 3000
    Public Const CHUNK_CONTROL_ASSIGNMENT_JUDGMENTS As Integer = 3001


    'C0371===
    Public Const CHUNK_CANVAS_STREAMS_PROJECT As Integer = 7777777

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_BASE As Integer = 70000

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_MODEL_STRUCTURE As Integer = 70000
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_PROPERTIES As Integer = 70001
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_PIPE_OPTIONS As Integer = 70002
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_INFO_DOCS As Integer = 70003
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_AHP_EXTRA_TABLES As Integer = 70004
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION As Integer = 70005
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_PIPE_MESSAGES As Integer = 70008
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_ANTIGUA_DASHBOARD As Integer = 70009 'C0783
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_ANTIGUA_RECYCLE_BIN As Integer = 70010 'C0783
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_ADVANCED_INFODOCS As Integer = 70012 'C0947
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_SPYRON_MODEL_VERSION As Integer = 70016 'C0947
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_SPYRON_STRUCTURE_WELCOME As Integer = 70017 'C0947
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_SPYRON_STRUCTURE_THANK_YOU As Integer = 70018 'C0947
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_SPYRON_STRUCTURE_IMPACT_WELCOME As Integer = 70050
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_SPYRON_STRUCTURE_IMPACT_THANK_YOU As Integer = 70051

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_STATUS As Integer = 70061

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_PARAMETERS As Integer = 70071
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_PARAMETERS_VALUES As Integer = 70072

    Public Const CHUNK_CANVAS_STREAMS_EVENTS_GROUPS As Integer = 70073
    Public Const CHUNK_CANVAS_STREAMS_BAYESIAN_DATA As Integer = 70075


    Public Const CHUNK_CANVAS_STREAMS_PROJECT_ANTIGUA_INFODOCS As Integer = 70013 'C1020
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_ATTRIBUTES As Integer = 70014 'C1020

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_ANTIGUA_DASHBOARD_IMPACT As Integer = 70020
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_ANTIGUA_RECYCLE_BIN_IMPACT As Integer = 70021
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_ANTIGUA_INFODOCS_IMPACT As Integer = 70023

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_CONTROLS As Integer = 70032
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_REGIONS As Integer = 70041

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_RESOURCE_ALIGNER As Integer = 70100
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_RESOURCE_ALIGNER_NEW As Integer = 70101
    'Public Const CHUNK_CANVAS_STREAMS_PROJECT_RESOURCE_ALIGNER_TIME_PERIODS As Integer = 70102
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_RESOURCE_ALIGNER_TIME_PERIODS As Integer = 70103

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_DATA_MAPPING As Integer = 70201

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE As Integer = 80000

    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_JUDGMENTS As Integer = 80000
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_PERMISSIONS As Integer = 80001
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_DISABLED_NODES As Integer = 80002
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_SPYRON_ANSWERS_WELCOME As Integer = 80006
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_SPYRON_ANSWERS_THANK_YOU As Integer = 80007
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_SPYRON_ANSWERS_IMPACT_WELCOME As Integer = 80008
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_SPYRON_ANSWERS_IMPACT_THANK_YOU As Integer = 80009
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_ATTRIBUTE_VALUES As Integer = 80004
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_JUDGMENTS_CONTROLS As Integer = 80011    ' D3891 // udtJudgmentsControls = 11
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_PERMISSIONS_CONTROLS As Integer = 80012  ' D3891 // udtPermissionsControls = 12
    Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_COMMENTS As Integer = 80015

    Public Const CHUNK_CANVAS_STREAMS_SNAPSHOTS_STREAM As Integer = 90900   ' D3890

    Public Const DEFAULT_ENCRYPTION_PASSWORD As String = "expertchoice" 'C0380

    Public Const _DEBUG_ENCRYPTION_ENABLED As Boolean = False 'C0380

    Public Const UNDEFINED_COST_VALUE As String = "UNDEFINED_COST_VALUE" 'C0626

    Public Enum StructureType
        stModelStructure = 0
        stProperties = 1
        stPipeOptions = 2
        stInfoDocs = 3
        stAHPExtraTables = 4
        stModelVersion = 5
        stTeamTimeCombinedResults = 6 'C0352
        stTeamTimeSAValues = 7 'C0352
        stPipeMessages = 8 'C0355
        stAntiguaDashboard = 9 'C0606
        stAntiguaRecycleBin = 10 'C0606
        stAntiguaTreeView = 11 'C0606
        stAdvancedInfoDocs = 12 'C0920
        stAntiguaInfoDocs = 13 'C0829
        stAttributes = 14 'C1020
        stSpyronModelVersion = 16
        stSpyronStructureWelcome = 17
        stSpyronStructureThankYou = 18
        stAntiguaDashboardImpact = 20
        stAntiguaRecycleBinImpact = 21
        stAntiguaTreeViewImpact = 22
        stAntiguaInfoDocsImpact = 23
        stControls = 32
        stRegions = 41
        stSpyronStructureImpactWelcome = 50
        stSpyronStructureImpactThankyou = 51
        stResourceAligner = 100
        stResourceAlignerNew = 101
        'stResourceAlignerTimePeriods = 102
        stResourceAlignerTimePeriods = 103
        stStatus = 61
        stProjectParameters = 71
        stProjectParametersValues = 72
        stEventsGroups = 73
        stEdges = 74
        stBayesianData = 75
        stDataMapping = 201
    End Enum

    Public Enum UserDataType
        udtJudgments = 0
        udtPermissions = 1
        udtDisabledNodes = 2
        udtTeamTimeJudgment = 3 'C0352
        udtSpyronAnswersWelcome = 6
        udtSpyronAnswersThankYou = 7
        udtSpyronAnswersImpactWelcome = 8
        udtSpyronAnswersImpactThankyou = 9
        udtAttributeValues = 4 'C1020
        udtJudgmentsControls = 11
        udtPermissionsControls = 12
        udtComments = 15
    End Enum

End Module