Imports System.Xml
Imports System.Data.Common 'C0236
Imports ECCore
Imports ECSecurity.ECSecurity
Imports System.IO 'C0267
Imports System.Reflection
Imports Microsoft.VisualBasic.CompilerServices

Namespace Canvas

    Public Module PipeParameters
        <Serializable()> Public Enum ParameterValueType
            pvtInteger = 0
            pvtDouble = 1
            pvtString = 2
            pvtBoolean = 3
            pvtDateTime = 4
        End Enum

        <Serializable()> Public Class ParameterInfo
            Public Property IntID As Integer
            Public Property StringID As String
            Public Property PropertyName As String
            Public Property ValueType As ParameterValueType

            Public Sub New(IntID As Integer, StringID As String, PropertyName As String, ValueType As ParameterValueType)
                Me.IntID = IntID
                Me.StringID = StringID
                Me.PropertyName = PropertyName
                Me.ValueType = ValueType
            End Sub
        End Class

        <Serializable()> Public Class ParameterSet 'C0461
            Public ID As Integer
            Public Name As String

            Public Sub New()
            End Sub

            Public Sub New(ByVal nID As Integer, ByVal sName As String)
                ID = nID
                Name = sName
            End Sub
        End Class

#Region "CONSTANTS"
        Public Const PARAMETER_SET_DEFAULT As Integer = 0 'C0461
        Public Const PARAMETER_SET_TEAM_TIME As Integer = 1 'C0461
        Public Const PARAMETER_SET_IMPACT As Integer = 1 'C0461

        Public Const PARAMETER_SET_MEASURE As Integer = 4

        Public Const PROPERTIES_DEFAULT_TABLE_NAME As String = "Properties" 'C0051
        Public Const PROPERTY_NAME_DEFAULT_DB_COLUMN_NAME As String = "PropertyName" 'C0051
        Public Const PROPERTY_VALUE_DEFAULT_DB_COLUMN_NAME As String = "PropertyValue" 'C0051

        Private Const CHUNK_PARAMETERS_SET As Integer = 2000 'C0461 'C0462

        Private Const MESSAGES_DEFAULT_TABLE_NAME As String = "PipeMessages"
        Private Const WELCOME_MESSAGE_ID As Integer = 0
        Private Const THANK_YOU_MESSAGE_ID As Integer = 1

        Private Const UNDEFINED_PROMT_ID As Integer = -1 'C0576
        Private Const UNDEFINED_ALTS_PROMT_ID As Integer = -1 'C0619

        Private Const XML_SETTINGS_NODE_NAME As String = "Settings"

        Private Const OPTION_PREFIX As String = "PIPE_OPTION_"

        Private Const EVALUATE_OBJECTIVES As String = OPTION_PREFIX + "EvaluateObjectives"
        Private Const EVALUATE_ALTERNATIVES As String = OPTION_PREFIX + "EvaluateAlternatives"
        Private Const MODEL_EVALUATION_ORDER As String = OPTION_PREFIX + "ModelEvaluationOrder"
        Private Const GRAPHICAL_PW_MODE As String = OPTION_PREFIX + "GraphicalPWMode"
        Private Const GRAPHICAL_PW_MODE_FOR_ALTS As String = OPTION_PREFIX + "GraphicalPWModeForAlts"
        Private Const OBJECTIVES_EVALUATION_DIRECTION As String = OPTION_PREFIX + "ObjectivesEvaluationDirection"
        Private Const ALTERNATIVES_EVALUATION_MODE As String = OPTION_PREFIX + "AlternativesEvaluationMode"
        Private Const PAIRWISE_TYPE As String = OPTION_PREFIX + "PairwiseType"
        Private Const PAIRWISE_TYPE_FOR_ALTERNATIVES As String = OPTION_PREFIX + "PairwiseTypeForAlternatives" 'C0985
        Private Const SYNTHESIS_MODE As String = OPTION_PREFIX + "SynthesisMode"
        Private Const COMBINED_MODE As String = OPTION_PREFIX + "CombinedMode"
        Private Const IDEAL_VIEW_TYPE As String = OPTION_PREFIX + "IdealViewType"
        Private Const EVALUATE_DIAGONALS As String = OPTION_PREFIX + "EvaluateDiagonals"
        Private Const EVALUATE_DIAGONALS_ADVANCED As String = OPTION_PREFIX + "EvaluateDiagonalsAdvanced" 'C1010
        Private Const EVALUATE_DIAGONALS_ALTERNATIVES As String = OPTION_PREFIX + "EvaluateDiagonalsAlternatives" 'C0987
        Private Const LOCAL_RESULTS_VIEW As String = OPTION_PREFIX + "LocalResultsView"
        Private Const GLOBAL_RESULTS_VIEW As String = OPTION_PREFIX + "GlobalResultsView"
        Private Const WRT_INFODOCS_SHOW_MODE As String = OPTION_PREFIX + "WRTInfoDocsShowMode" 'C1045
        Private Const LOCAL_RESULTS_SORT_MODE As String = OPTION_PREFIX + "LocalResultsSortMode" 'C0820
        Private Const GLOBAL_RESULTS_SORT_MODE As String = OPTION_PREFIX + "GlobalResultsSortMode" 'C0820
        Private Const SHOW_CONSISTENCY_RATIO As String = OPTION_PREFIX + "ShowConsistencyRatio"
        Private Const SHOW_COMMENTS As String = OPTION_PREFIX + "ShowComments"
        Private Const SHOW_INFODOCS As String = OPTION_PREFIX + "ShowInfoDocs"
        Private Const SHOW_INFODOCS_MODE As String = OPTION_PREFIX + "ShowInfoDocsMode" 'C0099
        Private Const SHOW_WELCOME_SCREEN As String = OPTION_PREFIX + "ShowWelcomeScreen"
        Private Const SHOW_THANK_YOU_SCREEN As String = OPTION_PREFIX + "ShowThankYouScreen"

        Private Const SHOW_WELCOME_SURVEY As String = OPTION_PREFIX + "ShowWelcomeSurvey" 'C0139
        Private Const SHOW_THANK_YOU_SURVEY As String = OPTION_PREFIX + "ShowThankYouSurvey" 'C0139

        Private Const SHOW_SURVEY As String = OPTION_PREFIX + "ShowSurvey"
        Private Const SHOW_SENSITIVITY_ANALYSIS As String = OPTION_PREFIX + "ShowSensitivityAnalysis"
        Private Const SHOW_SENSITIVITY_ANALYSIS_PERFORMANCE As String = OPTION_PREFIX + "ShowSensitivityAnalysisPerformance" 'C0078
        Private Const SHOW_SENSITIVITY_ANALYSIS_GRADIENT As String = OPTION_PREFIX + "ShowSensitivityAnalysisGradient" 'C0078
        Private Const SORT_SENSITIVITY_ANALYSIS As String = OPTION_PREFIX + "SortSensitivityAnalysis"
        Private Const CALC_SA_FOR_COMBINED As String = OPTION_PREFIX + "CalcSAForCombined"
        Private Const CALC_SA_WRT_GOAL As String = OPTION_PREFIX + "CalcSAWRTGoal"
        Private Const ALLOW_SWITCH_SA_NODE As String = OPTION_PREFIX + "AllowSwitchSANode"
        Private Const ALLOW_AUTOADVANCE As String = OPTION_PREFIX + "AllowAutoadvance"
        Private Const SHOW_NEXT_UNASSESSED As String = OPTION_PREFIX + "ShowNextUnassessed"
        Private Const SHOW_PROGRESS_INDICATOR As String = OPTION_PREFIX + "ShowProgressIndicator"
        Private Const ALLOW_NAVIGATION As String = OPTION_PREFIX + "AllowNavigation"
        Private Const ALLOW_MISSING_JUDGMENTS As String = OPTION_PREFIX + "AllowMissingJudgments"
        Private Const FORCE_GRAPHICAL_PAIRWISE As String = OPTION_PREFIX + "ForceGraphicalPairwise"
        Private Const FORCE_GRAPHICAL_PAIRWISE_FOR_ALTERNATIVES As String = OPTION_PREFIX + "ForceGraphicalPairwiseForAlternatives" 'C0985
        Private Const INCLUDE_IDEAL_ALTERNATIVE As String = OPTION_PREFIX + "IncludeIdealAlternative"
        Private Const SHOW_IDEAL_ALTERNATIVE As String = OPTION_PREFIX + "ShowIdealAlternative"
        Private Const MEASURE_MODE As String = OPTION_PREFIX + "MeasureMode"
        Private Const PROJECT_NAME As String = OPTION_PREFIX + "ProjectName"
        Private Const PROJECT_TYPE As String = OPTION_PREFIX + "ProjectType"
        Private Const PROJECT_PURPOSE As String = OPTION_PREFIX + "ProjectPurpose"
        Private Const OBJS_COLUMN_NAME_IN_SA As String = OPTION_PREFIX + "ObjectivesColumnNameInSA"
        Private Const ALTS_COLUMN_NAME_IN_SA As String = OPTION_PREFIX + "AlternativesColumnNameInSA"
        Private Const ALTS_DEFAULT_CONTRIBUTION As String = OPTION_PREFIX + "AltsDefaultContribution"
        Private Const ALTS_DEFAULT_CONTRIBUTION_IMPACT As String = OPTION_PREFIX + "AltsDefaultContributionImpact"
        Private Const START_DATE As String = OPTION_PREFIX + "StartDate"
        Private Const END_DATE As String = OPTION_PREFIX + "EndDate"
        'Private Const WELCOME_TEXT As String = OPTION_PREFIX + "WelcomeText" 'C0051 'C0267
        'Private Const THANK_YOU_TEXT As String = OPTION_PREFIX + "ThankYouText" 'C0051 'C0267
        Private Const TERMINAL_REDIRECT_URL As String = OPTION_PREFIX + "TerminalRedirectURL" 'C0110
        Private Const REDIRECT_AT_THE_END As String = OPTION_PREFIX + "RedirectAtTheEnd" 'C0115
        Private Const LOG_OFF_AT_THE_END As String = OPTION_PREFIX + "LogOffAtTheEnd" 'C0115
        Private Const RANDOM_SEQUENCE As String = OPTION_PREFIX + "RandomSequence" 'C0205

        Private Const SYNHRONOUS_START_IN_POLLING_MODE As String = OPTION_PREFIX + "SynchronousStartInPollingMode" 'C0104
        Private Const SYNHRONOUS_START_VOTING_ONCE_FACILITATOR_ALLOWS As String = OPTION_PREFIX + "SynchronousStartVotingOnceFacilitatorAllows" 'C0104
        Private Const SYNHRONOUS_SHOW_GROUP_RESULTS As String = OPTION_PREFIX + "SynchronousShowGroupResults" 'C0104
        Private Const SYNHRONOUS_USE_OPTIONS_FROM_DECISION As String = OPTION_PREFIX + "SynchronousUseOptionsFromDecision" 'C0104
        Private Const SYNHRONOUS_USE_VOTING_BOXES As String = OPTION_PREFIX + "SynchronousUseVotingBoxes" 'C0104

        Private Const TEAM_TIME_ALLOW_MEETING_ID As String = OPTION_PREFIX + "TTAllowMeetingID"
        Private Const TEAM_TIME_ALLOW_ONLY_EMAIL_ADDRESSES_ADDED_TO_SESSION_TO_LOGIN As String = OPTION_PREFIX + "TTAllowOnlyEmailAddressesAddedToSessionToLogin"
        Private Const TEAM_TIME_DISPLAY_USERS_WITH_VIEW_ONLY_ACCESS As String = OPTION_PREFIX + "TTDisplayUsersWithViewOnlyAccess"
        Private Const TEAM_TIME_START_IN_PRACTICE_MODE As String = OPTION_PREFIX + "StartInPracticeMode"
        Private Const TEAM_TIME_START_IN_ANONYMOUS_MODE As String = OPTION_PREFIX + "StartInAnonymousMode" 'C0744
        Private Const TEAM_TIME_SHOW_KEYPAD_NUMBERS As String = OPTION_PREFIX + "ShowKeypadNumbers" 'C0395
        Private Const TEAM_TIME_SHOW_KEYPAD_LEGENDS As String = OPTION_PREFIX + "ShowKeypadLegends" 'C0395
        Private Const TEAM_TIME_HIDE_PROJECT_OWNER As String = OPTION_PREFIX + "HideProjectOwner" 'C0432

        Private Const TEAM_TIME_INVITATION_SUBJECT As String = OPTION_PREFIX + "TTInvitationSubject" 'C0493
        Private Const TEAM_TIME_INVITATION_BODY As String = OPTION_PREFIX + "TTInvitationBody" 'C0493
        Private Const TEAM_TIME_EVALUATE_INVITATION_SUBJECT As String = OPTION_PREFIX + "TTEvaluateInvitationSubject" 'C0493
        Private Const TEAM_TIME_EVALUATE_INVITATION_BODY As String = OPTION_PREFIX + "TTEvaluateInvitationBody" 'C0493

        Private Const TEAM_TIME_INVITATION_SUBJECT2 As String = OPTION_PREFIX + "TTInvitationSubject2"
        Private Const TEAM_TIME_INVITATION_BODY2 As String = OPTION_PREFIX + "TTInvitationBody2"
        Private Const TEAM_TIME_EVALUATE_INVITATION_SUBJECT2 As String = OPTION_PREFIX + "TTEvaluateInvitationSubject2"
        Private Const TEAM_TIME_EVALUATE_INVITATION_BODY2 As String = OPTION_PREFIX + "TTEvaluateInvitationBody2"

        Private Const NAME_ALTERNATIVES As String = OPTION_PREFIX + "NameAlternatives"
        Private Const NAME_OBJECTIVES As String = OPTION_PREFIX + "NameObjectives"

        Private Const INFODOC_SIZE As String = OPTION_PREFIX + "InfoDocSize"

        'Private Const WELCOME_SCREEN_TEXT_FOR_AHP As String = OPTION_PREFIX + "WelcomeScreenForAHP" 'C0573 'C0574
        'Private Const THANK_YOU_SCREEN_TEXT_FOR_AHP As String = OPTION_PREFIX + "ThankYouScreenForAHP" 'C0573 'C0574

        Private Const JUDGEMENT_PROMT_ID As String = OPTION_PREFIX + "JudgementPromtID" 'C0576
        Private Const JUDGEMENT_ALTS_PROMT_ID As String = OPTION_PREFIX + "JudgementAltsPromtID" 'C0619

        Private Const JUDGEMENT_PROMT_MULTI_ID As String = OPTION_PREFIX + "JudgementPromtMultiID"
        Private Const JUDGEMENT_ALTS_PROMT_MULTI_ID As String = OPTION_PREFIX + "JudgementAltsPromtMultiID"

        Private Const JUDGEMENT_PROMT As String = OPTION_PREFIX + "JudgementPromt"
        Private Const JUDGEMENT_ALTS_PROMT As String = OPTION_PREFIX + "JudgementAltsPromt"

        Private Const JUDGEMENT_PROMT_MULTI As String = OPTION_PREFIX + "JudgementPromtMulti"
        Private Const JUDGEMENT_ALTS_PROMT_MULTI As String = OPTION_PREFIX + "JudgementAltsPromtMulti"

        Private Const SHOW_FULL_OBJECTIVE_PATH As String = OPTION_PREFIX + "ShowFullObjectivePath" 'C0578

        Private Const DEFAULT_COV_OBJ_MEASUREMENT_TYPE As String = OPTION_PREFIX + "DefaultCovObjMeasurementType" 'C0753
        Private Const DEFAULT_NON_COV_OBJ_MEASUREMENT_TYPE As String = OPTION_PREFIX + "DefaultNonCovObjMeasurementType"

        Private Const PROJECT_GUID As String = OPTION_PREFIX + "ProjectGuid" 'C0805

        Private Const USE_CIS As String = OPTION_PREFIX + "UseCIS" 'C0824
        Private Const USE_WEIGHTS As String = OPTION_PREFIX + "UseWeights"

        Private Const OBJECTIVES_PAIRWISE_ONE_AT_A_TIME As String = OPTION_PREFIX + "ObjectivesPairwiseOneAtATime" 'C0977
        Private Const OBJECTIVES_NON_PAIRWISE_ONE_AT_A_TIME As String = OPTION_PREFIX + "ObjectivesNonPairwiseOneAtATime" 'C0977

        Private Const ALTERNATIVES_PAIRWISE_ONE_AT_A_TIME As String = OPTION_PREFIX + "AlternativesPairwiseOneAtATime" 'C0987

        Private Const FORCE_ALL_DIAGONALS As String = OPTION_PREFIX + "ForceAllDiagonals"
        Private Const FORCE_ALL_DIAGONALS_LIMIT As String = OPTION_PREFIX + "ForceAllDiagonalsLimit"

        Private Const FORCE_ALL_DIAGONALS_FOR_ALTERNATIVES As String = OPTION_PREFIX + "ForceAllDiagonalsForAlternatives"
        Private Const FORCE_ALL_DIAGONALS_LIMIT_FOR_ALTERNATIVES As String = OPTION_PREFIX + "ForceAllDiagonalsLimitForAlternatives"

        Private Const TEAM_TIME_USERS_SORTING As String = OPTION_PREFIX + "TTUsersSorting"

        Private Const DEFAULT_GROUP_ID As String = OPTION_PREFIX + "DefaultGroupID"

        Private Const ASSOCIATED_MODEL_INT_ID As String = OPTION_PREFIX + "AssociatedModelIntID"
        Private Const ASSOCIATED_MODEL_GUID_ID As String = OPTION_PREFIX + "AssociatedModelGuidID"

        Private Const DISABLE_WARNINGS_ON_STEP_CHANGE As String = OPTION_PREFIX + "DisableWarningsOnStepChange"

        Private Const FEEDBACK_ON As String = OPTION_PREFIX + "FeedbackOn"

        Private Const SHOW_EXPECTED_VALUE_LOCAL As String = OPTION_PREFIX + "ShowExpectedValueLocal"
        Private Const SHOW_EXPECTED_VALUE_GLOBAL As String = OPTION_PREFIX + "ShowExpectedValueGlobal"


        'C0267===
        ' Integer constants
        Private Const EVALUATE_OBJECTIVES_PARAMETER As Integer = 1000
        Private Const EVALUATE_ALTERNATIVES_PARAMETER As Integer = 1001
        Private Const MODEL_EVALUATION_ORDER_PARAMETER As Integer = 1002
        Private Const OBJECTIVES_EVALUATION_DIRECTION_PARAMETER As Integer = 1003
        Private Const ALTERNATIVES_EVALUATION_MODE_PARAMETER As Integer = 1004
        Private Const PAIRWISE_TYPE_PARAMETER As Integer = 1005
        Private Const SYNTHESIS_MODE_PARAMETER As Integer = 1006
        Private Const IDEAL_VIEW_TYPE_PARAMETER As Integer = 1007
        Private Const EVALUATE_DIAGONALS_PARAMETER As Integer = 1008
        Private Const LOCAL_RESULTS_VIEW_PARAMETER As Integer = 1009
        Private Const GLOBAL_RESULTS_VIEW_PARAMETER As Integer = 1010
        Private Const SHOW_CONSISTENCY_RATIO_PARAMETER As Integer = 1011
        Private Const SHOW_COMMENTS_PARAMETER As Integer = 1012
        Private Const SHOW_INFODOCS_PARAMETER As Integer = 1013
        Private Const SHOW_INFODOCS_MODE_PARAMETER As Integer = 1014
        Private Const SHOW_WELCOME_SCREEN_PARAMETER As Integer = 1015
        Private Const SHOW_THANK_YOU_SCREEN_PARAMETER As Integer = 1016

        Private Const SHOW_WELCOME_SURVEY_PARAMETER As Integer = 1017
        Private Const SHOW_THANK_YOU_SURVEY_PARAMETER As Integer = 1018

        Private Const SHOW_SURVEY_PARAMETER As Integer = 1019
        Private Const SHOW_SENSITIVITY_ANALYSIS_PARAMETER As Integer = 1020
        Private Const SHOW_SENSITIVITY_ANALYSIS_PERFORMANCE_PARAMETER As Integer = 1021
        Private Const SHOW_SENSITIVITY_ANALYSIS_GRADIENT_PARAMETER As Integer = 1022
        Private Const SORT_SENSITIVITY_ANALYSIS_PARAMETER As Integer = 1023
        Private Const CALC_SA_FOR_COMBINED_PARAMETER As Integer = 1024
        Private Const CALC_SA_WRT_GOAL_PARAMETER As Integer = 1025
        Private Const ALLOW_SWITCH_SA_NODE_PARAMETER As Integer = 1026
        Private Const ALLOW_AUTOADVANCE_PARAMETER As Integer = 1027
        Private Const SHOW_NEXT_UNASSESSED_PARAMETER As Integer = 1028
        Private Const SHOW_PROGRESS_INDICATOR_PARAMETER As Integer = 1029
        Private Const ALLOW_NAVIGATION_PARAMETER As Integer = 1030
        Private Const ALLOW_MISSING_JUDGMENTS_PARAMETER As Integer = 1031
        Private Const FORCE_GRAPHICAL_PAIRWISE_PARAMETER As Integer = 1032
        Private Const INCLUDE_IDEAL_ALTERNATIVE_PARAMETER As Integer = 1033
        Private Const SHOW_IDEAL_ALTERNATIVE_PARAMETER As Integer = 1034
        Private Const MEASURE_MODE_PARAMETER As Integer = 1035
        Private Const PROJECT_NAME_PARAMETER As Integer = 1036
        Private Const PROJECT_PURPOSE_PARAMETER As Integer = 1037
        Private Const OBJS_COLUMN_NAME_IN_SA_PARAMETER As Integer = 1038
        Private Const ALTS_COLUMN_NAME_IN_SA_PARAMETER As Integer = 1039
        Private Const ALTS_DEFAULT_CONTRIBUTION_PARAMETER As Integer = 1040
        Private Const START_DATE_PARAMETER As Integer = 1041
        Private Const END_DATE_PARAMETER As Integer = 1042
        Private Const WELCOME_TEXT_PARAMETER As Integer = 1043
        Private Const THANK_YOU_TEXT_PARAMETER As Integer = 1044
        Private Const TERMINAL_REDIRECT_URL_PARAMETER As Integer = 1045
        Private Const REDIRECT_AT_THE_END_PARAMETER As Integer = 1046
        Private Const LOG_OFF_AT_THE_END_PARAMETER As Integer = 1047
        Private Const RANDOM_SEQUENCE_PARAMETER As Integer = 1048

        Private Const SYNHRONOUS_START_IN_POLLING_MODE_PARAMETER As Integer = 1049
        Private Const SYNHRONOUS_START_VOTING_ONCE_FACILITATOR_ALLOWS_PARAMETER As Integer = 1050
        Private Const SYNHRONOUS_SHOW_GROUP_RESULTS_PARAMETER As Integer = 1051
        Private Const SYNHRONOUS_USE_OPTIONS_FROM_DECISION_PARAMETER As Integer = 1052
        Private Const SYNHRONOUS_USE_VOTING_BOXES_PARAMETER As Integer = 1053
        'C0267==

        'C0386===
        Private Const TEAM_TIME_ALLOW_MEETING_ID_PARAMETER As Integer = 1054
        Private Const TEAM_TIME_ALLOW_ONLY_EMAIL_ADDRESSES_ADDED_TO_SESSION_TO_LOGIN_PARAMETER As Integer = 1055
        Private Const TEAM_TIME_DISPLAY_USERS_WITH_VIEW_ONLY_ACCESS_PARAMETER As Integer = 1056
        Private Const TEAM_TIME_START_IN_PRACTICE_MODE_PARAMETER As Integer = 1057
        'C0386==

        Private Const TEAM_TIME_SHOW_KEYPAD_NUMBERS_PARAMETER As Integer = 1058 'C0395
        Private Const TEAM_TIME_SHOW_KEYPAD_LEGENDS_PARAMETER As Integer = 1059 'C0395

        Private Const TEAM_TIME_HIDE_PROJECT_OWNER_PARAMETER As Integer = 1060 'C0432

        Private Const TEAM_TIME_INVITATION_SUBJECT_PARAMETER As Integer = 1061 'C0493
        Private Const TEAM_TIME_INVITATION_BODY_PARAMETER As Integer = 1062 'C0493
        Private Const TEAM_TIME_EVALUATE_INVITATION_SUBJECT_PARAMETER As Integer = 1063 'C0493
        Private Const TEAM_TIME_EVALUATE_INVITATION_BODY_PARAMETER As Integer = 1064 'C0493

        Private Const TEAM_TIME_INVITATION_SUBJECT2_PARAMETER As Integer = 1101
        Private Const TEAM_TIME_INVITATION_BODY2_PARAMETER As Integer = 1102
        Private Const TEAM_TIME_EVALUATE_INVITATION_SUBJECT2_PARAMETER As Integer = 1103
        Private Const TEAM_TIME_EVALUATE_INVITATION_BODY2_PARAMETER As Integer = 1104

        Private Const JUDGEMENT_PROMT_ID_PARAMETER As Integer = 1065 'C0576
        Private Const SHOW_FULL_OBJECTIVE_PATH_PARAMETER As Integer = 1066 'C0578

        Private Const JUDGEMENT_ALTS_PROMT_ID_PARAMETER As Integer = 1067 'C0619 'C0709

        Private Const TEAM_TIME_START_IN_ANONYMOUS_MODE_PARAMETER As Integer = 1068 'C0744

        Private Const DEFAULT_COV_OBJ_MEASUREMENT_TYPE_PARAMETER As Integer = 1069 'C0753

        Private Const PROJECT_GUID_PARAMETER As Integer = 1070 'C0805

        Private Const LOCAL_RESULTS_SORT_MODE_PARAMETER As Integer = 1071 'C0820
        Private Const GLOBAL_RESULTS_SORT_MODE_PARAMETER As Integer = 1072 'C0820

        Private Const USE_CIS_PARAMETER As Integer = 1073 'C0824
        Private Const USE_WEIGHTS_PARAMETER As Integer = 3061 'C0824

        Private Const COMBINED_MODE_PARAMETER As Integer = 1074 'C0945

        Private Const OBJECTIVES_PAIRWISE_ONE_AT_A_TIME_PARAMETER As Integer = 1075 'C0977
        Private Const OBJECTIVES_NON_PAIRWISE_ONE_AT_A_TIME_PARAMETER As Integer = 1076 'C0977

        Private Const EVALUATE_DIAGONALS_ADVANCED_PARAMETER As Integer = 1082 'C1010

        Private Const FORCE_GRAPHICAL_PAIRWISE_FOR_ALTERNATIVES_PARAMETER As Integer = 1077 'C0985
        Private Const PAIRWISE_TYPE_FOR_ALTERNATIVES_PARAMETER As Integer = 1078 'C0985

        Private Const EVALUATE_DIAGONALS_ALTERNATIVES_PARAMETER As Integer = 1079 'C0987
        Private Const ALTERNATIVES_PAIRWISE_ONE_AT_A_TIME_PARAMETER As Integer = 1080 'C0987

        Private Const WRT_INDOFOCS_SHOW_MODE_PARAMETER As Integer = 1081 'C1045

        Private Const FORCE_ALL_DIAGONALS_PARAMETER As Integer = 1084
        Private Const FORCE_ALL_DIAGONALS_LIMIT_PARAMETER As Integer = 1085

        Private Const FORCE_ALL_DIAGONALS_FOR_ALTERNATIVES_PARAMETER As Integer = 1086
        Private Const FORCE_ALL_DIAGONALS_LIMIT_FOR_ALTERNATIVES_PARAMETER As Integer = 1087

        Private Const ALTS_DEFAULT_CONTRIBUTION_IMPACT_PARAMETER As Integer = 1088

        Private Const PROJECT_TYPE_PARAMETER As Integer = 1090

        Private Const NAME_ALTERNATIVES_PARAMETER As Integer = 3000
        Private Const NAME_OBJECTIVES_PARAMETER As Integer = 3001

        Private Const TEAM_TIME_USERS_SORTING_PARAMETER As Integer = 3002

        Private Const INFODOC_SIZE_PARAMETER As Integer = 3010

        Private Const DEFAULT_GROUP_ID_PARAMETER As Integer = 3003

        Private Const JUDGEMENT_PROMT_PARAMETER As Integer = 3005
        Private Const JUDGEMENT_ALTS_PROMT_PARAMETER As Integer = 3006

        Private Const DISABLE_WARNINGS_ON_STEP_CHANGE_PARAMETER As Integer = 3007
        Private Const GRAPHICAL_PW_MODE_PARAMETER As Integer = 3008
        Private Const GRAPHICAL_PW_MODE_FOR_ALTS_PARAMETER As Integer = 3009

        Private Const JUDGEMENT_PROMT_MULTI_PARAMETER As Integer = 3020
        Private Const JUDGEMENT_ALTS_PROMT_MULTI_PARAMETER As Integer = 3021
        Private Const JUDGEMENT_ALTS_PROMT_MULTI_ID_PARAMETER As Integer = 3022
        Private Const JUDGEMENT_PROMT_MULTI_ID_PARAMETER As Integer = 3023

        Private Const ASSOCIATED_MODEL_INT_ID_PARAMETER As Integer = 3050
        Private Const ASSOCIATED_MODEL_GUID_ID_PARAMETER As Integer = 3051

        Private Const FEEDBACK_ON_PARAMETER As Integer = 3060

        Private Const DEFAULT_NON_COV_OBJ_MEASUREMENT_TYPE_PARAMETER As Integer = 3071

        Private Const SHOW_EXPECTED_VALUE_LOCAL_PARAMETER As Integer = 3072
        Private Const SHOW_EXPECTED_VALUE_GLOBAL_PARAMETER As Integer = 3073

        ' Use 4xxx for ProjectParameters options (same IDs used in CWSw)
        ' ==============================================================
        Private Const SHOW_INDEX_LOCAL_PARAMETER As Integer = 4000      ' D3786
        Private Const SHOW_INDEX_GLOBAL_PARAMETER As Integer = 4001     ' D3786 + D3977
        Private Const TEAM_TIME_HIDE_OFFLINE_USERS As Integer = 4002    ' D3977

#End Region

        Public Enum PipeStorageType
            pstXMLFile = 0
            pstDatabase = 1
            pstStreamsDatabase = 2 'C0267
        End Enum

        Public Enum PipeMessageType
            pmtGeneralInformation = 0
            pmtNodeEvaluation = 1
            pmtAlternativeEvaluation = 2
            pmtLocalResults = 3
            pmtGlobalResults = 4
            pmtDynamicSensitivity = 5
            pmtQuickHelp_Old = 6    ' D3693 + D4081
            pmtQuickHelp_v2 = 7     ' D4081
            pmtInfodocState = 8     ' D4223
        End Enum

        Public Enum PipeMessagePosition
            pmpBefore = 0
            pmpAfter = 1
        End Enum

        'C0139===
        Public Enum PipeMessageKind
            pmkText = 0
            pmkSurvey = 1
            pmkReward = 2   ' D3963
            pmlTextRiskControls = 3 ' D4293
        End Enum
        'C0139==

        ' D3693 ===
        Public Enum ecEvaluationStepType
            DirectInput = 1
            GraphicalPW = 2
            VerbalPW = 3
            Ratings = 4
            StepFunction = 5
            UtilityCurve = 6
            Other = 7
            DynamicSA = 8
            GradientSA = 9
            IntermediateResults = 10
            PerformanceSA = 11
            Survey = 12
            ThankYou = 13
            Welcome = 14
            OverallResults = 15
            MultiVerbalPW = 16
            MultiGraphicalPW = 17
            MultiRatings = 18
            MultiDirectInput = 19
            AllEventsWithNoSources = 20 ' D4711
            RiskResults = 21    ' D6662
            HeatMap = 22        ' D6662
            AlternativesRank = 23   ' D6674
        End Enum
        ' D3693 ==

        ' D4100 ===
        Public Enum ecShowObjectivePath
            DontShowPath = 0
            AlwaysShowFull = 1
            CollapsePath = 2
        End Enum
        ' D4100 ==

        ' D4224 ====
        Public Enum ecAppliationID
            appUnknown = 0      ' D4274
            appComparion = 1
            appGecko = 3
        End Enum
        ' D4224 ==

        ' D6962 ===
        Public Enum ecText2Speech
            Disabled = 0
            AutoPlay = 1
            Regular = 2
        End Enum
        ' D6962 ==

        '' D4079 ===
        '<Serializable> Public Class clsQuickHelpMessage
        '    Public HierarchyID As Integer = -1
        '    Public StepType As ecEvaluationStepType
        '    Public Message As String = ""
        '    Public AutoShow As Boolean = False
        '    Public ParentID As Integer = -1
        '    Public FirstID As Integer = -1
        '    Public SecondID As Integer = -1
        '    Private mVersion As Integer = -1

        '    Public ReadOnly Property Version As Integer
        '        Get
        '            Return mVersion
        '        End Get
        '    End Property

        '    Public Sub New(tHierarchyID As Integer, tStepType As ecEvaluationStepType, tParentID As Integer)
        '        HierarchyID = tHierarchyID
        '        StepType = tStepType
        '        ParentID = tParentID
        '    End Sub

        'End Class
        '' D4079 ==

        <Serializable()> Public Class clsPipeMessage

            ' D3697: disable GUIDs since they are not saved in the streams
            Private mMessageType As PipeMessageType
            Private mHierarchyID As Integer
            'Private mHierarchyGuidID As Guid 'C0355
            Private mAltsHierarchyID As Integer
            'Private mAltsHierarchyGuidID As Guid 'C0355
            Private mObjectID As Integer
            'Private mObjectGuidID As Guid 'C0355
            'Private mMessage As String 'C0139
            Private mMessage As Object 'C0139
            Private mPosition As PipeMessagePosition
            Private mGroupID As Integer
            'Private mGroupGuidID As Guid 'C0355
            Private mUserID As Integer
            'Private mUserGuidID As Guid 'C0355
            Private mMessageKind As PipeMessageKind 'C0139

            Public Property MessageType() As PipeMessageType
                Get
                    Return mMessageType
                End Get
                Set(ByVal value As PipeMessageType)
                    mMessageType = value
                End Set
            End Property

            Public Property MessageKind() As PipeMessageKind 'C0139
                Get
                    Return mMessageKind
                End Get
                Set(ByVal value As PipeMessageKind)
                    mMessageKind = value
                End Set
            End Property

            Public Property HierarchyID() As Integer
                Get
                    Return mHierarchyID
                End Get
                Set(ByVal value As Integer)
                    mHierarchyID = value
                End Set
            End Property

            ' -D3697
            'Public Property HierarchyGuidID() As Guid 'C0355
            '    Get
            '        Return mHierarchyGuidID
            '    End Get
            '    Set(ByVal value As Guid)
            '        mHierarchyGuidID = value
            '    End Set
            'End Property

            Public Property AltsHierarchyID() As Integer
                Get
                    Return mAltsHierarchyID
                End Get
                Set(ByVal value As Integer)
                    mAltsHierarchyID = value
                End Set
            End Property

            ' -D3697
            'Public Property AltsHierarchyGuidID() As Guid 'C0355
            '    Get
            '        Return mAltsHierarchyGuidID
            '    End Get
            '    Set(ByVal value As Guid)
            '        mAltsHierarchyGuidID = value
            '    End Set
            'End Property

            Public Property ObjectID() As Integer
                Get
                    Return mObjectID
                End Get
                Set(ByVal value As Integer)
                    mObjectID = value
                End Set
            End Property

            ' -D3697
            'Public Property ObjectGuidID() As Guid 'C0355
            '    Get
            '        Return mObjectGuidID
            '    End Get
            '    Set(ByVal value As Guid)
            '        mObjectGuidID = value
            '    End Set
            'End Property

            Public Property Message() As String
                Get
                    Return mMessage
                End Get
                Set(ByVal value As String)
                    mMessage = value
                End Set
            End Property

            Public Property Position() As PipeMessagePosition
                Get
                    Return mPosition
                End Get
                Set(ByVal value As PipeMessagePosition)
                    mPosition = value
                End Set
            End Property

            Public Property GroupID() As Integer
                Get
                    Return mGroupID
                End Get
                Set(ByVal value As Integer)
                    mGroupID = value
                End Set
            End Property

            ' -D3697
            'Public Property GroupGuidID() As Guid
            '    Get
            '        Return mGroupGuidID
            '    End Get
            '    Set(ByVal value As Guid)
            '        mGroupGuidID = value
            '    End Set
            'End Property

            Public Property UserID() As Integer
                Get
                    Return mUserID
                End Get
                Set(ByVal value As Integer)
                    mUserID = value
                End Set
            End Property

            ' -D3697
            'Public Property UserGuidID() As Guid 'C0355
            '    Get
            '        Return mUserGuidID
            '    End Get
            '    Set(ByVal value As Guid)
            '        mUserGuidID = value
            '    End Set
            'End Property

            Public Sub New()
            End Sub

            'Public Sub New(ByVal MessageType As PipeMessageType, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, _
            '    ByVal ObjectID As Integer, ByVal Message As String, ByVal Position As PipeMessagePosition, ByVal GroupID As Integer, ByVal UserID As Integer) 'C0139
            Public Sub New(ByVal MessageKind As PipeMessageKind, ByVal MessageType As PipeMessageType, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer,
                ByVal ObjectID As Integer, ByVal Message As String, ByVal Position As PipeMessagePosition, ByVal GroupID As Integer, ByVal UserID As Integer) 'C0139
                mMessageType = MessageType
                mHierarchyID = HierarchyID
                mAltsHierarchyID = AltsHierarchyID
                mObjectID = ObjectID
                mMessage = Message
                mPosition = Position
                mGroupID = GroupID
                mUserID = UserID
            End Sub

            ' D3697 ===
            'Public Sub New(ByVal MessageKind As PipeMessageKind, ByVal MessageType As PipeMessageType, ByVal HierarchyGuidID As Guid, ByVal AltsHierarchyGuidID As Guid,
            '    ByVal ObjectGuidID As Guid, ByVal Message As String, ByVal Position As PipeMessagePosition, ByVal GroupGuidID As Guid, ByVal UserGuidID As Guid) 'C0355
            '    mMessageType = MessageType
            '    mHierarchyGuidID = HierarchyGuidID
            '    mAltsHierarchyGuidID = AltsHierarchyGuidID
            '    mObjectGuidID = ObjectGuidID
            '    mMessage = Message
            '    mPosition = Position
            '    mGroupGuidID = GroupGuidID
            '    mUserGuidID = UserGuidID
            'End Sub

            Public Sub New(ByVal MessageKind As PipeMessageKind, ByVal MessageType As PipeMessageType, ByVal Message As String, ByVal Position As PipeMessagePosition) 'C0355
                mMessageType = MessageType
                mMessage = Message
                mPosition = Position
            End Sub
            ' D3697 ==

        End Class

        <Serializable()> Public Class clsPipeMessages
            Private mMessages As New ArrayList
            Private mTableName As String = MESSAGES_DEFAULT_TABLE_NAME

            Public Shared OPT_QUICK_HELP_AVAILABLE As Boolean = True        ' D4077
            Public Shared OPT_QUICK_HELP_AUTO_SHOW_ONCE As Boolean = False  ' D6670 + D7556
            Private Const OPT_DUPLICATE_QH_AS_OLD As Boolean = False        ' D0481
            Private Const OPT_OVERRIDE_STEP_QH_WITH_CLUSTER_QH As Boolean = True    ' D4082

            Public Property TableName() As String
                Get
                    Return mTableName
                End Get
                Set(ByVal value As String)
                    mTableName = value
                End Set
            End Property

            Public Property Messages() As ArrayList
                Get
                    Return mMessages
                End Get
                Set(ByVal value As ArrayList)
                    mMessages = value
                End Set
            End Property

            'Public Function GetNodeEvaluationText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal NodeID As Integer, ByVal Position As PipeMessagePosition) As String 'C0139
            Public Function GetNodeEvaluationText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal NodeID As Integer, ByVal Position As PipeMessagePosition) As String 'C0139
                For Each message As clsPipeMessage In mMessages
                    'If (message.MessageType = PipeMessageType.pmtNodeEvaluation) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = NodeID) And (message.Position = Position) Then 'C0139
                    If (message.MessageKind = MessageKind) And (message.MessageType = PipeMessageType.pmtNodeEvaluation) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = NodeID) And (message.Position = Position) Then 'C0139
                        Return message.Message
                    End If
                Next

                'C0051===
                Dim newMessage As New clsPipeMessage
                newMessage.MessageKind = MessageKind 'C0139
                newMessage.MessageType = PipeMessageType.pmtNodeEvaluation
                newMessage.HierarchyID = HierarchyID
                newMessage.AltsHierarchyID = AltsHierarchyID
                newMessage.ObjectID = NodeID
                newMessage.Position = Position
                newMessage.Message = ""
                mMessages.Add(newMessage)
                Return newMessage.Message
                'Return ""
                'C0051==
            End Function

            'Public Function GetAlternativeEvaluationText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal AlternativeID As Integer, ByVal Position As PipeMessagePosition) As String 'C0139
            Public Function GetAlternativeEvaluationText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal AlternativeID As Integer, ByVal Position As PipeMessagePosition) As String 'C0139
                For Each message As clsPipeMessage In mMessages
                    'If (message.MessageType = PipeMessageType.pmtAlternativeEvaluation) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = AlternativeID) And (message.Position = Position) Then 'C0139
                    If (message.MessageKind = MessageKind) And (message.MessageType = PipeMessageType.pmtAlternativeEvaluation) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = AlternativeID) And (message.Position = Position) Then 'C0139
                        Return message.Message
                    End If
                Next
                'C0051===
                Dim newMessage As New clsPipeMessage
                newMessage.MessageKind = MessageKind 'C0139
                newMessage.MessageType = PipeMessageType.pmtAlternativeEvaluation
                newMessage.HierarchyID = HierarchyID
                newMessage.AltsHierarchyID = AltsHierarchyID
                newMessage.ObjectID = AlternativeID
                newMessage.Position = Position
                newMessage.Message = ""
                mMessages.Add(newMessage)
                Return newMessage.Message
                'Return ""
                'C0051==
            End Function

#Region "Quick Help"

            ' D4079 ===
            Public Function GetEvaluationQuickHelpText(PM As clsProjectManager, tStepID As Integer, ByRef fIsForCluster As Boolean, ByRef fAutoShow As Boolean) As String ' D4081
                Dim sValue As String = ""
                If PM IsNot Nothing Then

                    Dim tAction As clsAction = Nothing
                    If tStepID > 0 AndAlso tStepID <= PM.Pipe.Count Then tAction = PM.Pipe(tStepID - 1)

                    If tAction IsNot Nothing Then

                        Dim ParentNode As clsNode = Nothing
                        Dim FirstNode As clsNode = Nothing
                        Dim SecondNode As clsNode = Nothing
                        Dim ParentID As Integer = -1
                        Dim NodeID As Integer = -1  ' D4081

                        Dim tStepType As ecEvaluationStepType = PM.PipeBuilder.GetPipeActionStepType(tAction)
                        PM.PipeBuilder.GetPipeActionNodes(tAction, ParentNode, FirstNode, SecondNode)

                        If ParentNode IsNot Nothing Then ParentID = CRC32.ComputeAsInt(ParentNode.NodeGuidID.ToString)
                        ' D4081 ===
                        If FirstNode IsNot Nothing Then
                            If SecondNode IsNot Nothing Then
                                NodeID = CRC32.ComputeAsInt(FirstNode.NodeGuidID.ToString + SecondNode.NodeGuidID.ToString)
                            Else
                                NodeID = CRC32.ComputeAsInt(FirstNode.NodeGuidID.ToString)
                            End If
                        End If

                        Dim tMsg As clsPipeMessage = GetEvaluationQuickHelpMessage(PM.ActiveHierarchy, tStepType, ParentID, NodeID, fIsForCluster)  'D4082
                        If tMsg Is Nothing AndAlso Not fIsForCluster Then tMsg = GetEvaluationQuickHelpMessage(PM.ActiveHierarchy, tStepType, ParentID, NodeID, True) ' D4082

                        If tMsg Is Nothing Then
                            ' D4081 ==
                            Dim NodeID_old As Integer = 0
                            If tAction.isEvaluation AndAlso ParentNode IsNot Nothing Then
                                NodeID_old = CInt(If(ParentNode.IsAlternative, -ParentNode.NodeID, ParentNode.NodeID))
                            End If
                            ' D4081 ===
                            Dim tStep As Integer = tStepID
                            Dim tAuto As Boolean = fAutoShow
                            sValue = GetEvaluationQuickHelpOldText(tStepType, NodeID_old, PM.ActiveHierarchy, tStep, tAuto)
                            fIsForCluster = tStep = 0   ' D4082
                            fAutoShow = tAuto   ' D4082
                        Else
                            sValue = tMsg.Message
                            fIsForCluster = (tMsg.AltsHierarchyID = 1) OrElse (tMsg.UserID <> 1)  ' D4082 + D4092
                            fAutoShow = tMsg.Position = PipeMessagePosition.pmpAfter    ' D4082
                        End If
                        ' D4081 ==

                    End If

                End If
                Return sValue
            End Function

            Private Function GetEvaluationQuickHelpOldMessage(ByVal tEvalStep As ecEvaluationStepType, tNodeID As Integer, ByVal HierarchyID As Integer, ByRef tStepID As Integer) As clsPipeMessage
                For Each msg As clsPipeMessage In mMessages
                    If (msg.MessageKind = PipeMessageKind.pmkText) AndAlso (msg.MessageType = PipeMessageType.pmtQuickHelp_Old) AndAlso (msg.HierarchyID = HierarchyID) AndAlso (msg.ObjectID = tNodeID) AndAlso (msg.GroupID = CInt(tEvalStep) AndAlso (msg.UserID = tStepID)) Then
                        Return msg
                    End If
                Next
                Return Nothing
            End Function
            ' D4079 ==

            ' D4081 ===
            Private Function GetEvaluationQuickHelpMessage(ByVal HierarchyID As Integer, ByVal tEvalStep As ecEvaluationStepType, tParentID As Integer, tNodeID As Integer, isForCluster As Boolean) As clsPipeMessage
                For Each msg As clsPipeMessage In mMessages
                    If (msg.MessageKind = PipeMessageKind.pmkText) AndAlso (msg.MessageType = PipeMessageType.pmtQuickHelp_v2) AndAlso (msg.HierarchyID = HierarchyID) _
                        AndAlso (msg.ObjectID = tParentID) AndAlso (msg.GroupID = CInt(tEvalStep) AndAlso (msg.UserID = tNodeID OrElse isForCluster) AndAlso (ExpertChoice.Service.Bool2Num(isForCluster) = msg.AltsHierarchyID)) Then ' D4092
                        Return msg
                    End If
                Next
                Return Nothing
            End Function
            ' D4081 ==

            ' D3693 + D3699 ===
            ''' <summary>
            ''' Get quick help infodoc for the specified evaluation pip step
            ''' </summary>
            ''' <param name="tEvalStep">Kind of step</param>
            ''' <param name="tNodeID">Must be NodeID, positive for objective, negative for alternative</param>
            ''' <param name="HierarchyID">Active HierarchyID</param>
            ''' <param name="tStepID">Use 0 for "cluster" content or step number for the specified step. Please note: ByRef, so value could be changed for zero after the call routine!</param>
            ''' <returns>MHT infodoc (see comment)</returns>
            ''' <remarks>Please note: when you call for some step, like 3, it will take a look for QH for step #3 and if this is missing, it returns "cluster" infodoc for tStepID=0 (check this parameter after call)</remarks>
            Private Function GetEvaluationQuickHelpOldText(ByVal tEvalStep As ecEvaluationStepType, tNodeID As Integer, ByVal HierarchyID As Integer, ByRef tStepID As Integer, ByRef fAutoShow As Boolean) As String    ' D3695 + D3697 + D3941 + D4079
                If Not OPT_QUICK_HELP_AVAILABLE Then Return "" ' D4077
                Dim sValue As String = ""
                'For Each msg As clsPipeMessage In mMessages
                '    If (msg.MessageKind = PipeMessageKind.pmkText) AndAlso (msg.MessageType = PipeMessageType.pmtQuickHelp) AndAlso (msg.HierarchyID = HierarchyID) AndAlso (msg.ObjectID = tNodeID) AndAlso (msg.GroupID = CInt(tEvalStep) AndAlso (msg.UserID = tStepID)) Then   ' D3695 + D3697 + D4079
                '        sValue = msg.Message
                '        fAutoShow = msg.Position = PipeMessagePosition.pmpAfter ' D3941
                '        Exit For
                '    End If
                'Next
                ' D4079 ===
                Dim tMsg As clsPipeMessage = GetEvaluationQuickHelpOldMessage(tEvalStep, tNodeID, HierarchyID, tStepID)
                If tMsg IsNot Nothing Then
                    sValue = tMsg.Message
                    fAutoShow = tMsg.Position = PipeMessagePosition.pmpAfter
                End If
                ' D4079 ==
                If sValue = "" AndAlso tStepID > 0 Then
                    tStepID = 0
                    sValue = GetEvaluationQuickHelpOldText(tEvalStep, tNodeID, HierarchyID, tStepID, fAutoShow)   ' D3941 + D4079
                End If
                Return sValue
                ' D3699 ==
            End Function
            ' D3693 ==

            ' D6662 ====
            Public Function GetEvaluationQuickHelpForCustom(PM As clsProjectManager, tStepType As ecEvaluationStepType, ID As Integer, ByRef fAutoShow As Boolean) As String
                Dim sValue As String = ""
                If PM IsNot Nothing Then
                    Dim tMsg As clsPipeMessage = GetEvaluationQuickHelpMessage(PM.ActiveHierarchy, tStepType, ID, -1, False)
                    If tMsg IsNot Nothing Then
                        sValue = tMsg.Message
                        fAutoShow = tMsg.Position = PipeMessagePosition.pmpAfter
                    End If
                End If
                Return sValue
            End Function

            Public Sub SetEvaluationQuickHelpForCustom(PM As clsProjectManager, tStepType As ecEvaluationStepType, ID As Integer, fAutoShow As Boolean, MessageText As String)
                If PM IsNot Nothing Then
                    Dim tMessage As clsPipeMessage = GetEvaluationQuickHelpMessage(PM.ActiveHierarchy, tStepType, ID, -1, False)
                    If tMessage Is Nothing Then
                        tMessage = New clsPipeMessage
                        tMessage.MessageKind = PipeMessageKind.pmkText
                        tMessage.MessageType = PipeMessageType.pmtQuickHelp_v2
                        tMessage.HierarchyID = PM.ActiveHierarchy
                        tMessage.GroupID = CInt(tStepType)
                        tMessage.ObjectID = ID
                        tMessage.UserID = -1
                        tMessage.AltsHierarchyID = 0
                        mMessages.Add(tMessage)
                    End If
                    tMessage.Message = MessageText
                    tMessage.Position = CType(If(fAutoShow, PipeMessagePosition.pmpAfter, PipeMessagePosition.pmpBefore), PipeMessagePosition)
                End If
            End Sub
            ' D6662 ==

            ' D4079 ===
            Public Sub SetEvaluationQuickHelpText(PM As clsProjectManager, tStepID As Integer, isForCluster As Boolean, fAutoShow As Boolean, ByVal MessageText As String)    ' D4081
                If PM IsNot Nothing Then

                    Dim tAction As clsAction = Nothing
                    If tStepID > 0 AndAlso tStepID <= PM.Pipe.Count Then tAction = PM.Pipe(tStepID - 1)

                    If tAction IsNot Nothing Then

                        Dim ParentNode As clsNode = Nothing
                        Dim FirstNode As clsNode = Nothing
                        Dim SecondNode As clsNode = Nothing
                        Dim ParentID As Integer = -1
                        Dim NodeID As Integer = -1  ' D4081

                        Dim tStepType As ecEvaluationStepType = PM.PipeBuilder.GetPipeActionStepType(tAction)
                        PM.PipeBuilder.GetPipeActionNodes(tAction, ParentNode, FirstNode, SecondNode)

                        If ParentNode IsNot Nothing Then ParentID = CRC32.ComputeAsInt(ParentNode.NodeGuidID.ToString)
                        ' D4081 ===
                        If FirstNode IsNot Nothing Then
                            If SecondNode IsNot Nothing Then
                                NodeID = CRC32.ComputeAsInt(FirstNode.NodeGuidID.ToString + SecondNode.NodeGuidID.ToString)
                            Else
                                NodeID = CRC32.ComputeAsInt(FirstNode.NodeGuidID.ToString)
                            End If
                        End If

                        ' D4082 ===
                        If OPT_OVERRIDE_STEP_QH_WITH_CLUSTER_QH Then
                            Dim tStepMsg As clsPipeMessage = GetEvaluationQuickHelpMessage(PM.ActiveHierarchy, tStepType, ParentID, NodeID, False)
                            If tStepMsg IsNot Nothing Then mMessages.Remove(tStepMsg)
                        End If
                        ' D4082 ==

                        Dim tMessage As clsPipeMessage = GetEvaluationQuickHelpMessage(PM.ActiveHierarchy, tStepType, ParentID, NodeID, isForCluster)

                        If tMessage Is Nothing Then
                            tMessage = New clsPipeMessage
                            tMessage.MessageKind = PipeMessageKind.pmkText
                            tMessage.MessageType = PipeMessageType.pmtQuickHelp_v2
                            tMessage.HierarchyID = PM.ActiveHierarchy
                            tMessage.GroupID = CInt(tStepType)
                            tMessage.ObjectID = ParentID
                            tMessage.UserID = NodeID
                            tMessage.AltsHierarchyID = ExpertChoice.Service.Bool2Num(isForCluster)
                            mMessages.Add(tMessage)
                        End If

                        tMessage.Message = MessageText
                        tMessage.Position = CType(If(fAutoShow, PipeMessagePosition.pmpAfter, PipeMessagePosition.pmpBefore), PipeMessagePosition)
                        ' D4081 ==

                        Dim NodeID_old As Integer = 0
                        If tAction.isEvaluation AndAlso ParentNode IsNot Nothing Then
                            NodeID_old = CInt(If(ParentNode.IsAlternative, -ParentNode.NodeID, ParentNode.NodeID))
                        End If
                        ' D0481 ===
                        If OPT_DUPLICATE_QH_AS_OLD Then ' save a copy in old version (not recommended)
                            SetEvaluationQuickHelpOldText(tStepType, NodeID_old, PM.ActiveHierarchy, If(isForCluster, 0, tStepID), MessageText, fAutoShow)
                        Else    ' check if old version exists, just remove it
                            Dim tMsg As clsPipeMessage = GetEvaluationQuickHelpOldMessage(tStepType, NodeID_old, PM.ActiveHierarchy, If(isForCluster, 0, tStepID))
                            If tMsg IsNot Nothing Then mMessages.Remove(tMsg)
                        End If
                        ' D4081 ==

                    End If

                End If
            End Sub
            ' D4079 ==

            ' D3693 + D3699  ===
            Private Sub SetEvaluationQuickHelpOldText(ByVal tEvalStep As ecEvaluationStepType, tNodeID As Integer, ByVal HierarchyID As Integer, tStepID As Integer, ByVal MessageText As String, fAutoShow As Boolean)  ' D3695 + D3697 + D3941 + D4072
                Dim tMessage As clsPipeMessage = GetEvaluationQuickHelpOldMessage(tEvalStep, tNodeID, HierarchyID, tStepID) ' D4081

                'For Each msg As clsPipeMessage In mMessages
                '    If (msg.MessageKind = PipeMessageKind.pmkText) AndAlso (msg.MessageType = PipeMessageType.pmtQuickHelp_Old) AndAlso (msg.HierarchyID = HierarchyID) AndAlso (msg.ObjectID = tNodeID) AndAlso (msg.GroupID = CInt(tEvalStep) AndAlso (msg.UserID = tStepID)) Then   ' D3695 + D3697 + D4072
                '        tMessage = msg
                '        Exit For
                '    End If
                'Next

                If tMessage Is Nothing Then
                    tMessage = New clsPipeMessage
                    tMessage.MessageKind = PipeMessageKind.pmkText
                    tMessage.MessageType = PipeMessageType.pmtQuickHelp_Old
                    tMessage.HierarchyID = HierarchyID
                    tMessage.GroupID = CInt(tEvalStep)
                    tMessage.ObjectID = tNodeID
                    tMessage.UserID = tStepID
                    mMessages.Add(tMessage)
                End If

                tMessage.Message = MessageText
                tMessage.Position = CType(If(fAutoShow, PipeMessagePosition.pmpAfter, PipeMessagePosition.pmpBefore), PipeMessagePosition)    ' D3941
            End Sub
            ' D3693 + D3699 ==

#End Region

            'Public Function GetWelcomeText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer) As String 'C0139
            Public Function GetWelcomeText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer) As String 'C0139
                For Each message As clsPipeMessage In mMessages
                    'If (message.MessageType = PipeMessageType.pmtGeneralInformation) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = WELCOME_MESSAGE_ID) Then 'C0139
                    If (message.MessageKind = MessageKind) And (message.MessageType = PipeMessageType.pmtGeneralInformation) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = WELCOME_MESSAGE_ID) Then 'C0139
                        Return message.Message
                    End If
                Next
                'C0051===
                Dim newMessage As New clsPipeMessage
                newMessage.MessageKind = MessageKind 'C0139
                newMessage.MessageType = PipeMessageType.pmtGeneralInformation
                newMessage.HierarchyID = HierarchyID
                newMessage.AltsHierarchyID = AltsHierarchyID
                newMessage.ObjectID = WELCOME_MESSAGE_ID
                newMessage.Message = ""
                mMessages.Add(newMessage)
                Return newMessage.Message
                'Return ""
                'C0051==
            End Function

            'Public Function GetThankYouText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer) As String 'C0139
            Public Function GetThankYouText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer) As String 'C0139
                For Each message As clsPipeMessage In mMessages
                    'If (message.MessageType = PipeMessageType.pmtGeneralInformation) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = THANK_YOU_MESSAGE_ID) Then 'C0139
                    If (message.MessageKind = MessageKind) And (message.MessageType = PipeMessageType.pmtGeneralInformation) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = THANK_YOU_MESSAGE_ID) Then 'C0139
                        Return message.Message
                    End If
                Next
                'C0051===
                Dim newMessage As New clsPipeMessage
                newMessage.MessageKind = MessageKind 'C0139
                newMessage.MessageType = PipeMessageType.pmtGeneralInformation
                newMessage.HierarchyID = HierarchyID
                newMessage.AltsHierarchyID = AltsHierarchyID
                newMessage.ObjectID = THANK_YOU_MESSAGE_ID
                newMessage.Message = ""
                mMessages.Add(newMessage)
                Return newMessage.Message
                'Return ""
                'C0051==
            End Function

            'Public Function GetLocalResultsText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal NodeID As Integer, ByVal Position As PipeMessagePosition) As String 'C0139
            Public Function GetLocalResultsText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal NodeID As Integer, ByVal Position As PipeMessagePosition) As String 'C0139
                For Each message As clsPipeMessage In mMessages
                    'If (message.MessageType = PipeMessageType.pmtLocalResults) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = NodeID) And (message.Position = Position) Then 'C0139
                    If (message.MessageKind = MessageKind) And (message.MessageType = PipeMessageType.pmtLocalResults) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.ObjectID = NodeID) And (message.Position = Position) Then 'C0139
                        Return message.Message
                    End If
                Next

                'C0051===
                Dim newMessage As New clsPipeMessage
                newMessage.MessageKind = MessageKind 'C0139
                newMessage.MessageType = PipeMessageType.pmtLocalResults
                newMessage.HierarchyID = HierarchyID
                newMessage.AltsHierarchyID = AltsHierarchyID
                newMessage.ObjectID = NodeID
                newMessage.Position = Position
                newMessage.Message = ""
                mMessages.Add(newMessage)
                Return newMessage.Message
                'Return ""
                'C0051==
            End Function

            'Public Function GetGlobalResultsText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal position As PipeMessagePosition) As String 'C0139
            Public Function GetGlobalResultsText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal position As PipeMessagePosition) As String 'C0139
                For Each message As clsPipeMessage In mMessages
                    'If (message.MessageType = PipeMessageType.pmtGlobalResults) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.Position = position) Then 'C0139
                    If (message.MessageKind = MessageKind) And (message.MessageType = PipeMessageType.pmtGlobalResults) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.Position = position) Then 'C0139
                        Return message.Message
                    End If
                Next

                'C0051===
                Dim newMessage As New clsPipeMessage
                newMessage.MessageKind = MessageKind 'C0139
                newMessage.MessageType = PipeMessageType.pmtGlobalResults
                newMessage.HierarchyID = HierarchyID
                newMessage.AltsHierarchyID = AltsHierarchyID
                newMessage.Position = position
                newMessage.Message = ""
                mMessages.Add(newMessage)
                Return newMessage.Message
                'Return ""
                'C0051==
            End Function

            'Public Function GetDynamicSensitivityText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal position As PipeMessagePosition) As String 'C0139
            Public Function GetDynamicSensitivityText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal position As PipeMessagePosition) As String 'C0139
                For Each message As clsPipeMessage In mMessages
                    'If (message.MessageType = PipeMessageType.pmtDynamicSensitivity) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.Position = position) Then 'C0139
                    If (message.MessageKind = MessageKind) And (message.MessageType = PipeMessageType.pmtDynamicSensitivity) And (message.HierarchyID = HierarchyID) And (message.AltsHierarchyID = AltsHierarchyID) And (message.Position = position) Then 'C0139
                        Return message.Message
                    End If
                Next

                'C0051===
                Dim newMessage As New clsPipeMessage
                newMessage.MessageKind = MessageKind 'C0139
                newMessage.MessageType = PipeMessageType.pmtDynamicSensitivity
                newMessage.HierarchyID = HierarchyID
                newMessage.AltsHierarchyID = AltsHierarchyID
                newMessage.Position = position
                newMessage.Message = ""
                mMessages.Add(newMessage)
                Return newMessage.Message
                'Return ""
                'C0051==
            End Function

            'Public Sub SetWelcomeText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal MessageText As String) 'C0139
            Public Sub SetWelcomeText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal MessageText As String) 'C0139
                Dim message As clsPipeMessage

                'C0051===
                'GetWelcomeText(HierarchyID, AltsHierarchyID) 'C0139
                GetWelcomeText(MessageKind, HierarchyID, AltsHierarchyID) 'C0139
                For Each msg As clsPipeMessage In mMessages
                    'If (msg.MessageType = PipeMessageType.pmtGeneralInformation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.ObjectID = WELCOME_MESSAGE_ID) Then 'C0139
                    If (msg.MessageKind = MessageKind) And (msg.MessageType = PipeMessageType.pmtGeneralInformation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.ObjectID = WELCOME_MESSAGE_ID) Then 'C0139
                        message = msg
                        message.Message = MessageText
                    End If
                Next

                'If GetWelcomeText(HierarchyID, AltsHierarchyID) <> "" Then
                '    For Each msg As clsPipeMessage In mMessages
                '        If (msg.MessageType = PipeMessageType.pmtGeneralInformation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.ObjectID = WELCOME_MESSAGE_ID) Then
                '            message = msg
                '            message.Message = MessageText
                '        End If
                '    Next
                'Else
                '    message = New clsPipeMessage(PipeMessageType.pmtGeneralInformation, HierarchyID, AltsHierarchyID, WELCOME_MESSAGE_ID, MessageText, PipeMessagePosition.pmpBefore, 0, 0)
                '    mMessages.Add(message)
                'End If
                'C0051==
            End Sub

            'Public Sub SetThankYouText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal MessageText As String) 'C0139
            Public Sub SetThankYouText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal MessageText As String) 'C0139
                Dim message As clsPipeMessage

                'C0051===
                'GetThankYouText(HierarchyID, AltsHierarchyID) 'C0139
                GetThankYouText(MessageKind, HierarchyID, AltsHierarchyID) 'C0139
                For Each msg As clsPipeMessage In mMessages
                    'If (msg.MessageType = PipeMessageType.pmtGeneralInformation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.ObjectID = THANK_YOU_MESSAGE_ID) Then 'C0139
                    If (msg.MessageKind = MessageKind) And (msg.MessageType = PipeMessageType.pmtGeneralInformation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.ObjectID = THANK_YOU_MESSAGE_ID) Then 'C0139
                        message = msg
                        message.Message = MessageText
                    End If
                Next

                'If GetThankYouText(HierarchyID, AltsHierarchyID) <> "" Then
                '    For Each msg As clsPipeMessage In mMessages
                '        If (msg.MessageType = PipeMessageType.pmtGeneralInformation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.ObjectID = THANK_YOU_MESSAGE_ID) Then
                '            message = msg
                '            message.Message = MessageText
                '        End If
                '    Next
                'Else
                '    message = New clsPipeMessage(PipeMessageType.pmtGeneralInformation, HierarchyID, AltsHierarchyID, THANK_YOU_MESSAGE_ID, MessageText, PipeMessagePosition.pmpBefore, 0, 0)
                '    mMessages.Add(message)
                'End If
                'C0051==
            End Sub

            'Public Sub SetGlobalResultsText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal position As PipeMessagePosition, ByVal MessageText As String) 'C0139
            Public Sub SetGlobalResultsText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal position As PipeMessagePosition, ByVal MessageText As String) 'C0139
                Dim message As clsPipeMessage

                'C0051===
                'GetGlobalResultsText(HierarchyID, AltsHierarchyID, position) 'C0139
                GetGlobalResultsText(MessageKind, HierarchyID, AltsHierarchyID, position) 'C0139
                For Each msg As clsPipeMessage In mMessages
                    'If (msg.MessageType = PipeMessageType.pmtGlobalResults) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = position) Then 'C0139
                    If (msg.MessageKind = MessageKind) And (msg.MessageType = PipeMessageType.pmtGlobalResults) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = position) Then 'C0139
                        message = msg
                        message.Message = MessageText
                    End If
                Next

                'If GetGlobalResultsText(HierarchyID, AltsHierarchyID, position) <> "" Then
                '    For Each msg As clsPipeMessage In mMessages
                '        If (msg.MessageType = PipeMessageType.pmtGlobalResults) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = position) Then
                '            message = msg
                '            message.Message = MessageText
                '        End If
                '    Next
                'Else
                '    message = New clsPipeMessage(PipeMessageType.pmtGlobalResults, HierarchyID, AltsHierarchyID, -1, MessageText, position, 0, 0)
                '    mMessages.Add(message)
                'End If
                'C0051==
            End Sub

            'Public Sub SetLocalResultsText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal NodeID As Integer, ByVal Position As PipeMessagePosition, ByVal MessageText As String) 'C0139
            Public Sub SetLocalResultsText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal NodeID As Integer, ByVal Position As PipeMessagePosition, ByVal MessageText As String) 'C0139
                Dim message As clsPipeMessage

                'C0051===
                'GetLocalResultsText(HierarchyID, AltsHierarchyID, NodeID, Position) 'C0139
                GetLocalResultsText(MessageKind, HierarchyID, AltsHierarchyID, NodeID, Position) 'C0139
                For Each msg As clsPipeMessage In mMessages
                    'If (msg.MessageType = PipeMessageType.pmtLocalResults) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = NodeID) Then 'C0139
                    If (msg.MessageKind = MessageKind) And (msg.MessageType = PipeMessageType.pmtLocalResults) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = NodeID) Then 'C0139
                        message = msg
                        message.Message = MessageText
                    End If
                Next

                'If GetLocalResultsText(HierarchyID, AltsHierarchyID, NodeID, Position) <> "" Then
                '    For Each msg As clsPipeMessage In mMessages
                '        If (msg.MessageType = PipeMessageType.pmtLocalResults) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = NodeID) Then
                '            message = msg
                '            message.Message = MessageText
                '        End If
                '    Next
                'Else
                '    message = New clsPipeMessage(PipeMessageType.pmtLocalResults, HierarchyID, AltsHierarchyID, NodeID, MessageText, Position, 0, 0)
                '    mMessages.Add(message)
                'End If
                'C0051==
            End Sub

            'Public Sub SetNodeEvaluationText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal NodeID As Integer, ByVal Position As PipeMessagePosition, ByVal MessageText As String) 'C0139
            Public Sub SetNodeEvaluationText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal NodeID As Integer, ByVal Position As PipeMessagePosition, ByVal MessageText As String) 'C0139
                Dim message As clsPipeMessage

                'C0051===
                'GetNodeEvaluationText(HierarchyID, AltsHierarchyID, NodeID, Position) 'C0139
                GetNodeEvaluationText(MessageKind, HierarchyID, AltsHierarchyID, NodeID, Position) 'C0139
                For Each msg As clsPipeMessage In mMessages
                    'If (msg.MessageType = PipeMessageType.pmtNodeEvaluation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = NodeID) Then 'C0139
                    If (msg.MessageKind = MessageKind) And (msg.MessageType = PipeMessageType.pmtNodeEvaluation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = NodeID) Then 'C0139
                        message = msg
                        message.Message = MessageText
                    End If
                Next

                'If GetNodeEvaluationText(HierarchyID, AltsHierarchyID, NodeID, Position) <> "" Then
                '    For Each msg As clsPipeMessage In mMessages
                '        If (msg.MessageType = PipeMessageType.pmtNodeEvaluation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = NodeID) Then
                '            message = msg
                '            message.Message = MessageText
                '        End If
                '    Next
                'Else
                '    message = New clsPipeMessage(PipeMessageType.pmtNodeEvaluation, HierarchyID, AltsHierarchyID, NodeID, MessageText, Position, 0, 0)
                '    mMessages.Add(message)
                'End If
                'C0051==
            End Sub

            'Public Sub SetAlternativeEvaluationText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal AlternativeID As Integer, ByVal Position As PipeMessagePosition, ByVal MessageText As String) 'C0139
            Public Sub SetAlternativeEvaluationText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal AlternativeID As Integer, ByVal Position As PipeMessagePosition, ByVal MessageText As String) 'C0139
                Dim message As clsPipeMessage

                'C0051===
                'GetAlternativeEvaluationText(HierarchyID, AltsHierarchyID, AlternativeID, Position) 'C0139
                GetAlternativeEvaluationText(MessageKind, HierarchyID, AltsHierarchyID, AlternativeID, Position) 'C0139
                For Each msg As clsPipeMessage In mMessages
                    'If (msg.MessageType = PipeMessageType.pmtAlternativeEvaluation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = AlternativeID) Then 'C0139
                    If (msg.MessageKind = MessageKind) And (msg.MessageType = PipeMessageType.pmtAlternativeEvaluation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = AlternativeID) Then 'C0139
                        message = msg
                        message.Message = MessageText
                    End If
                Next

                'If GetAlternativeEvaluationText(HierarchyID, AltsHierarchyID, AlternativeID, Position) <> "" Then
                '    For Each msg As clsPipeMessage In mMessages
                '        If (msg.MessageType = PipeMessageType.pmtAlternativeEvaluation) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = Position) And (msg.ObjectID = AlternativeID) Then
                '            message = msg
                '            message.Message = MessageText
                '        End If
                '    Next
                'Else
                '    message = New clsPipeMessage(PipeMessageType.pmtAlternativeEvaluation, HierarchyID, AltsHierarchyID, AlternativeID, MessageText, Position, 0, 0)
                '    mMessages.Add(message)
                'End If
                'C0051==
            End Sub

            'Public Sub SetDynamicSensitivityText(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal position As PipeMessagePosition, ByVal MessageText As String) 'C0139
            Public Sub SetDynamicSensitivityText(ByVal MessageKind As PipeMessageKind, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, ByVal position As PipeMessagePosition, ByVal MessageText As String) 'C0139
                Dim message As clsPipeMessage

                'C0051===
                'GetDynamicSensitivityText(HierarchyID, AltsHierarchyID, position) 'C0139
                GetDynamicSensitivityText(MessageKind, HierarchyID, AltsHierarchyID, position) 'C0139
                For Each msg As clsPipeMessage In mMessages
                    'If (msg.MessageType = PipeMessageType.pmtDynamicSensitivity) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = position) Then 'C0139
                    If (msg.MessageKind = MessageKind) And (msg.MessageType = PipeMessageType.pmtDynamicSensitivity) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = position) Then 'C0139
                        message = msg
                        message.Message = MessageText
                    End If
                Next

                'If GetDynamicSensitivityText(HierarchyID, AltsHierarchyID, position) <> "" Then
                '    For Each msg As clsPipeMessage In mMessages
                '        If (msg.MessageType = PipeMessageType.pmtDynamicSensitivity) And (msg.HierarchyID = HierarchyID) And (msg.AltsHierarchyID = AltsHierarchyID) And (msg.Position = position) Then
                '            message = msg
                '            message.Message = MessageText
                '        End If
                '    Next
                'Else
                '    message = New clsPipeMessage(PipeMessageType.pmtDynamicSensitivity, HierarchyID, AltsHierarchyID, -1, MessageText, position, 0, 0)
                '    mMessages.Add(message)
                'End If
                'C0051==
            End Sub

#Region "Infodoc state"

            ' D4223 ==
            Private Function GetEvaluationInfodocStateMessage(ByVal HierarchyID As Integer, ByVal tEvalStep As ecEvaluationStepType, tNodeID As Integer, tWRTNodeID As Integer) As clsPipeMessage
                For Each msg As clsPipeMessage In mMessages
                    If (msg.MessageKind = PipeMessageKind.pmkText) AndAlso (msg.MessageType = PipeMessageType.pmtInfodocState) AndAlso (msg.HierarchyID = HierarchyID) _
                        AndAlso (msg.ObjectID = tNodeID) AndAlso (msg.GroupID = tWRTNodeID) AndAlso (msg.UserID = CInt(tEvalStep)) Then
                        Return msg
                    End If
                Next
                Return Nothing
            End Function

            Public Function GetEvaluationInfodocState(ByVal HierarchyID As Integer, ByVal tEvalStep As ecEvaluationStepType, tNodeID As Integer, tWRTNodeID As Integer) As String
                Dim sParams As String = ""
                Dim tMessage As clsPipeMessage = GetEvaluationInfodocStateMessage(HierarchyID, tEvalStep, tNodeID, tWRTNodeID)
                If tMessage IsNot Nothing Then sParams = tMessage.Message
                Return sParams
            End Function

            Public Sub SetEvaluationInfodocState(ByVal HierarchyID As Integer, ByVal tEvalStep As ecEvaluationStepType, tNodeID As Integer, tWRTNodeID As Integer, sInfodocState As String) ' D4224
                sInfodocState = sInfodocState.Trim
                Dim tMessage As clsPipeMessage = GetEvaluationInfodocStateMessage(HierarchyID, tEvalStep, tNodeID, tWRTNodeID)
                If tMessage Is Nothing AndAlso sInfodocState <> "" Then
                    tMessage = New clsPipeMessage
                    tMessage.MessageKind = PipeMessageKind.pmkText
                    tMessage.MessageType = PipeMessageType.pmtInfodocState
                    tMessage.HierarchyID = HierarchyID
                    tMessage.ObjectID = tNodeID
                    tMessage.GroupID = tWRTNodeID
                    tMessage.UserID = CInt(tEvalStep)
                    mMessages.Add(tMessage)
                End If
                If sInfodocState <> "" Then
                    tMessage.Message = sInfodocState
                Else
                    If tMessage IsNot Nothing Then mMessages.Remove(tMessage)
                End If
            End Sub
            ' D4223 ==

#End Region

#Region "DB routines"

            'Public Function LoadFromDatabase(ByVal dbConnectionString As String) As Boolean 'C0236
            Public Function LoadFromDatabase(ByVal dbConnectionString As String, ByVal ProviderType As DBProviderType) As Boolean 'C0236
                ' check if the connection is alive
                If Not CheckDBConnection(ProviderType, dbConnectionString) Then
                    Return False
                End If

                'C0147===
                ''C0140===
                'Dim dbVersion As ECCanvasDatabaseVersion
                'dbVersion = GetCurrentDBVersion()
                ''C0140==
                'C0147==

                ' if connection is ok then open it
                'C0236===
                'Dim dbConnection As New odbc.odbcConnection(dbConnectionString)
                'Dim dbReader As odbc.odbcDataReader
                'Dim oCommand As odbc.odbcCommand

                Using dbConnection As DbConnection = GetDBConnection(ProviderType, dbConnectionString)   ' D2227
                    Dim dbReader As DbDataReader
                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection
                    'C0236==

                    dbConnection.Open()

                    'C0182===
                    'C0177===
                    'check if table exists
                    Dim schemaTable As System.Data.DataTable
                    Dim tableExists As Boolean = False

                    schemaTable = dbConnection.GetSchema("Tables")
                    For i As Integer = 0 To schemaTable.Rows.Count - 1
                        If schemaTable.Rows(i)!TABLE_NAME.ToString() = TableName Then
                            tableExists = True
                        End If
                    Next
                    schemaTable = Nothing

                    If Not tableExists Then
                        Return False
                    End If
                    'C0177==
                    'C0182==

                    ' if everything is ok then read from the table
                    'oCommand = New odbc.odbcCommand("SELECT * FROM " + TableName, dbConnection) 'C0236
                    oCommand.CommandText = "SELECT * FROM " + TableName 'C0236

                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    ' read messages from the table
                    mMessages.Clear()

                    'C0147===
                    Dim fieldChecked As Boolean = False
                    Dim fieldName As String = "MessageKind"
                    Dim fieldExists As Boolean = False
                    'C0147==

                    Dim message As clsPipeMessage
                    If Not dbReader Is Nothing Then
                        While dbReader.Read()
                            'C0147===
                            If Not fieldChecked Then
                                For i As Integer = 0 To dbReader.FieldCount - 1
                                    If dbReader.GetName(i).ToLower = fieldName.ToLower Then
                                        fieldExists = True
                                    End If
                                Next
                                fieldChecked = True
                            End If
                            'C0147==

                            ' Make sure fields "ID", "MessageType", "AltsHierarchID", "HierarchID" and "ObjectID" are not NULL
                            If (Not TypeOf (dbReader.GetValue(0)) Is DBNull) And
                                (Not TypeOf (dbReader.GetValue(1)) Is DBNull) And
                                (Not TypeOf (dbReader.GetValue(2)) Is DBNull) And
                                (Not TypeOf (dbReader.GetValue(3)) Is DBNull) And
                                (Not TypeOf (dbReader.GetValue(4)) Is DBNull) Then

                                message = New clsPipeMessage

                                ' Make TEXT as default
                                message.MessageKind = PipeMessageKind.pmkText 'C0139

                                message.MessageType = dbReader("MessageType")
                                message.HierarchyID = dbReader("HierarchyID")
                                message.AltsHierarchyID = dbReader("AltsHierarchyID")
                                message.ObjectID = dbReader("ObjectID")

                                ' Message
                                If (Not TypeOf (dbReader.GetValue(5)) Is DBNull) Then
                                    message.Message = dbReader("Message")
                                End If

                                ' Position
                                If (Not TypeOf (dbReader.GetValue(6)) Is DBNull) Then
                                    message.Position = dbReader("Position")
                                End If

                                ' GroupID
                                If (Not TypeOf (dbReader.GetValue(7)) Is DBNull) Then
                                    message.GroupID = dbReader("GroupID")
                                End If

                                ' UserID
                                If (Not TypeOf (dbReader.GetValue(8)) Is DBNull) Then
                                    message.UserID = dbReader("UserID")
                                End If

                                'C0139===
                                ' MessageKind (text/survey/etc.)
                                ' Commented out until database changes are made (added new field "MessageKind")
                                'If dbReader.FieldCount > 9 Then ' Make sure there's a field "MessageKind"
                                '    If (Not TypeOf (dbReader.GetValue(9)) Is DBNull) Then
                                '        message.MessageKind = dbReader("MessageKind")
                                '    End If
                                'End If
                                'C0139==

                                'C0140===
                                'If dbVersion.MinorVersion >= 4 Then 'C0147
                                If fieldExists Then 'C0147
                                    If (Not TypeOf (dbReader.GetValue(9)) Is DBNull) Then
                                        message.MessageKind = dbReader("MessageKind")
                                    End If
                                End If
                                'C0140==

                                mMessages.Add(message)
                            End If
                        End While
                    End If

                    oCommand = Nothing
                    dbConnection.Close()
                End Using
                Return True
            End Function

            Public Function LoadFromStreamsDatabase(ByVal Location As String, ByVal ModelID As Integer, ByVal ProviderType As DBProviderType) As Boolean 'C0355
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stPipeMessages)))

                    Dim dbReader As DbDataReader
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim MS As New MemoryStream

                    If dbReader.HasRows Then
                        dbReader.Read()

                        Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long                   ' The bytes returned from GetBytes.
                        Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        ' Continue reading and writing while there are bytes beyond the size of the buffer.
                        Do While retval = bufferSize
                            bw.Write(outbyte)
                            bw.Flush()

                            ' Reposition the start index to the end of the last buffer and fill the buffer.
                            startIndex += bufferSize
                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                        Loop

                        ' Write the remaining buffer.
                        bw.Write(outbyte, 0, retval)
                        bw.Flush()

                        'bw.Close()

                        dbReader.Close()
                        dbConnection.Close()
                        Return ParseMessagesStream(MS)
                    Else
                        dbReader.Close()
                        dbConnection.Close()
                        Return False
                    End If
                End Using
            End Function

            Public Function ReadStream(ByVal Stream As MemoryStream) As Boolean 'C0746
                Return ParseMessagesStream(Stream)
            End Function

            Protected Function ParseMessagesStream(ByVal Stream As MemoryStream) As Boolean 'C0355
                Stream.Seek(0, SeekOrigin.Begin)

                Dim BR As New BinaryReader(Stream)

                Dim count As Integer = BR.ReadInt32

                Messages.Clear()

                For i As Integer = 1 To count
                    Dim message As clsPipeMessage = New clsPipeMessage

                    message.MessageType = BR.ReadInt32
                    message.HierarchyID = BR.ReadInt32
                    'message.HierarchyGuidID = New Guid(BR.ReadBytes(16))
                    message.AltsHierarchyID = BR.ReadInt32
                    'message.AltsHierarchyGuidID = New Guid(BR.ReadBytes(16))
                    message.ObjectID = BR.ReadInt32
                    'message.ObjectGuidID = New Guid(BR.ReadBytes(16))
                    message.Message = BR.ReadString
                    message.Position = BR.ReadInt32
                    message.GroupID = BR.ReadInt32
                    'message.GroupGuidID = New Guid(BR.ReadBytes(16))
                    message.UserID = BR.ReadInt32
                    'message.UserGuidID = New Guid(BR.ReadBytes(16))
                    message.MessageKind = BR.ReadInt32

                    Messages.Add(message)
                Next

                BR.Close()

                Return True
            End Function

            'Public Function Load(ByVal StorageType As PipeStorageType, ByVal Location As String) As Boolean 'C0236
            'Public Function Load(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType) As Boolean 'C0355
            'Public Function Load(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, Optional ByVal ModelID As Integer = -1) As Boolean 'C0355 'C0420
            Public Function Load(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean 'C0420
                Select Case StorageType
                    Case PipeStorageType.pstDatabase
                        'Return LoadFromDatabase(Location) 'C0236
                        Return LoadFromDatabase(Location, ProviderType) 'C0236
                    Case PipeStorageType.pstStreamsDatabase 'C0355
                        Return LoadFromStreamsDatabase(Location, ModelID, ProviderType)
                End Select
                Return False
            End Function

            'Public Function SaveToDatabase(ByVal dbConnectionString As String) As Boolean 'C0236
            Protected Function SaveToDatabase(ByVal dbConnectionString As String, ByVal ProviderType As DBProviderType) As Boolean 'C0236
                ' check if the connection is alive
                If Not CheckDBConnection(ProviderType, dbConnectionString) Then
                    Return False
                End If

                'C0150===
                ''C0140===
                'Dim dbVersion As ECCanvasDatabaseVersion
                'dbVersion = GetCurrentDBVersion()
                ''C0140==
                'C0150==

                ' if connection is ok then open it
                'C0236===
                'Dim dbConnection As New odbc.odbcConnection(dbConnectionString)
                'Dim oCommand As New odbc.odbcCommand

                Using dbConnection As DbConnection = GetDBConnection(ProviderType, dbConnectionString)  ' D2227
                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    'C0236==

                    dbConnection.Open()

                    'C0182===
                    'C0177===
                    '' check if table exists
                    Dim schemaTable As System.Data.DataTable
                    Dim tableExists As Boolean = False

                    schemaTable = dbConnection.GetSchema("Tables")
                    For i As Integer = 0 To schemaTable.Rows.Count - 1
                        If schemaTable.Rows(i)!TABLE_NAME.ToString() = TableName Then
                            tableExists = True
                        End If
                    Next
                    schemaTable = Nothing

                    If Not tableExists Then
                        dbConnection.Close() 'C0052
                        Return False
                    End If
                    'C0177==
                    'C0182==

                    oCommand.Connection = dbConnection

                    'C0150===
                    'Check if MessageKind field exists

                    'Dim dbReader As odbc.odbcDataReader 'C0236
                    Dim dbReader As DbDataReader 'C0236

                    'Dim fieldName As String = "PipeMessages" 'C0187
                    Dim fieldName As String = "MessageKind" 'C0187
                    Dim fieldExists As Boolean = False

                    oCommand.CommandText = "select top 1 * from PipeMessages"
                    dbReader = DBExecuteReader(ProviderType, oCommand)
                    If dbReader.HasRows Then
                        For i As Integer = 0 To dbReader.FieldCount - 1
                            If dbReader.GetName(i).ToLower = fieldName.ToLower Then
                                fieldExists = True
                            End If
                        Next
                    End If
                    dbReader.Close()
                    'C0150==

                    oCommand.CommandText = "DELETE FROM " + TableName
                    DBExecuteNonQuery(ProviderType, oCommand)


                    ' write parameters to the database
                    oCommand.CommandText = "DELETE FROM " + TableName
                    DBExecuteNonQuery(ProviderType, oCommand)

                    For Each message As clsPipeMessage In mMessages
                        oCommand.Parameters.Clear()

                        'C0140===
                        'oCommand.CommandText = "INSERT INTO " + TableName + " (MessageType, HierarchyID, AltsHierarchyID, ObjectID, Message, Position, GroupID, UserID) VALUES " + _
                        '    "(?, ?, ?, ?, ?, ?, ?, ?)"
                        'C0140==

                        'C0140===
                        'If dbVersion.MinorVersion < 4 Then 'C0150
                        If Not fieldExists Then 'C0150
                            oCommand.CommandText = "INSERT INTO " + TableName + " (MessageType, HierarchyID, AltsHierarchyID, ObjectID, Message, [Position], GroupID, UserID) VALUES " +
                                "(?, ?, ?, ?, ?, ?, ?, ?)"
                        Else
                            oCommand.CommandText = "INSERT INTO " + TableName + " (MessageType, HierarchyID, AltsHierarchyID, ObjectID, Message, [Position], GroupID, UserID, MessageKind) VALUES " +
                                "(?, ?, ?, ?, ?, ?, ?, ?, ?)"
                        End If
                        'C0140==

                        'C0236===
                        'oCommand.Parameters.AddWithValue("MessageType", message.MessageType)
                        'oCommand.Parameters.AddWithValue("HierarchyID", message.HierarchyID)
                        'oCommand.Parameters.AddWithValue("AltsHierarchyID", message.AltsHierarchyID)
                        'oCommand.Parameters.AddWithValue("ObjectID", message.ObjectID)
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "MessageType", message.MessageType)) 'C0237
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "HierarchyID", message.HierarchyID)) 'C0237
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "AltsHierarchyID", message.AltsHierarchyID)) 'C0237
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ObjectID", message.ObjectID)) 'C0237
                        'C0236==

                        'C0052===
                        'C0236===
                        'Dim memoField As odbc.odbcParameter = oCommand.CreateParameter
                        'memoField.odbcType = odbc.odbcType.Text
                        'memoField.ParameterName = "memoField"
                        'memoField.Value = message.Message
                        'oCommand.Parameters.Add(memoField)

                        'C0368===
                        'Dim memoField As DbParameter = oCommand.CreateParameter
                        'memoField.Value = message.Message
                        'oCommand.Parameters.Add(memoField)
                        'C0368==

                        'C0236==
                        'oCommand.Parameters.AddWithValue("Message", message.Message) 
                        'C0052==

                        'C0368===
                        If ProviderType = DBProviderType.dbptODBC Then
                            Dim memoField As Odbc.OdbcParameter = oCommand.CreateParameter
                            memoField.OdbcType = Odbc.OdbcType.Text
                            memoField.ParameterName = "memoField"
                            memoField.Value = message.Message
                            oCommand.Parameters.Add(memoField)
                        Else
                            Dim memoField As DbParameter = oCommand.CreateParameter
                            memoField.Value = message.Message
                            oCommand.Parameters.Add(memoField)
                        End If
                        'C0368==

                        'C0236===
                        'oCommand.Parameters.AddWithValue("Position", message.Position)
                        'oCommand.Parameters.AddWithValue("GroupID", message.GroupID)
                        'oCommand.Parameters.AddWithValue("UserID", message.UserID)
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Position", message.Position)) 'C0237
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "GroupID", message.GroupID)) 'C0237
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", message.UserID)) 'C0237
                        'C0236==

                        'C0140===
                        'If dbVersion.MinorVersion >= 4 Then 'C0150
                        If fieldExists Then 'C0150
                            'oCommand.Parameters.AddWithValue("MessageKind", message.MessageKind) 'C0140 'C0236
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "MessageKind", message.MessageKind)) 'C0236 'C0237
                        End If
                        'C0140==

                        DBExecuteNonQuery(ProviderType, oCommand)
                    Next

                    oCommand = Nothing
                    dbConnection.Close()
                End Using

                Return True
            End Function

            'Public Function Save(ByVal StorageType As PipeStorageType, ByVal Location As String) As Boolean 'C0236
            'Public Function Save(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType) As Boolean 'C0236 'C0267
            'Public Function Save(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, Optional ByVal ModelID As Integer = -1) As Boolean 'C0236 'C0420
            Public Function Save(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean 'C0420
                Select Case StorageType
                    Case PipeStorageType.pstDatabase
                        'Return SaveToDatabase(Location) 'C0236
                        Return SaveToDatabase(Location, ProviderType) 'C0236 + D0329
                    Case PipeStorageType.pstStreamsDatabase 'C0267
                        Return SaveToCanvasStreamDatabase(Location, ModelID, ProviderType) 'C0355
                End Select
                Return False
            End Function

            Protected Function SaveToCanvasStreamDatabase(ByVal Location As String, ByVal ModelID As Integer, ByVal ProviderType As DBProviderType) As Boolean
                Dim MS As MemoryStream = CreateMessagesStream()


                Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    Dim transaction As DbTransaction = Nothing 'C0814
                    Try
                        'C0814===
                        transaction = dbConnection.BeginTransaction
                        oCommand.Transaction = transaction
                        'C0814==

                        oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stPipeMessages)))

                        Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                        oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType.stPipeMessages))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                        affected = DBExecuteNonQuery(ProviderType, oCommand)

                        transaction.Commit() 'C0814
                    Catch ex As Exception 'C0814
                        If transaction IsNot Nothing Then
                            transaction.Rollback()
                        End If
                    Finally 'C0814
                        oCommand = Nothing
                        transaction.Dispose()
                        dbConnection.Close()
                    End Try

                End Using

                Return True
            End Function

            Public Function WriteStream() As MemoryStream 'C0746
                Return CreateMessagesStream()
            End Function

            Private Function CreateMessagesStream() As MemoryStream 'C0355
                Dim MS As New MemoryStream
                Dim BW As New BinaryWriter(MS)

                ' D4079 === // remove all empty QH
                For i As Integer = mMessages.Count - 1 To 0 Step -1
                    If mMessages(i).MessageType = PipeMessageType.pmtQuickHelp_Old AndAlso mMessages(i).Message = "" Then mMessages.RemoveAt(i)
                Next
                ' D4079 ==

                BW.Write(mMessages.Count)

                For Each message As clsPipeMessage In mMessages
                    BW.Write(message.MessageType)
                    BW.Write(message.HierarchyID)
                    'BW.Write(message.HierarchyGuidID.ToByteArray)
                    BW.Write(message.AltsHierarchyID)
                    'BW.Write(message.AltsHierarchyGuidID.ToByteArray)
                    BW.Write(message.ObjectID)
                    'BW.Write(message.ObjectGuidID.ToByteArray)
                    BW.Write(message.Message)
                    BW.Write(message.Position)
                    BW.Write(message.GroupID)
                    'BW.Write(message.GroupGuidID.ToByteArray)
                    BW.Write(message.UserID)
                    'BW.Write(message.UserGuidID.ToByteArray)
                    BW.Write(message.MessageKind)
                Next

                BW.Close()

                Return MS
            End Function

            Public Function PipeMessagesExistInStreams(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
                Dim count As Integer = 0
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)    ' D2227
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    oCommand.CommandText = "SELECT COUNT(*) FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stPipeMessages)))

                    Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
                    count = If(obj Is Nothing, 0, CType(obj, Integer))

                    oCommand = Nothing
                    dbConnection.Close()
                End Using
                Return count > 0
            End Function

            Public Function PipeMessagesExistInAHPX(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType) As Boolean
                Dim count As Integer
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)    ' D2227
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    dbConnection.Open()

                    'check if table exists
                    Dim schemaTable As System.Data.DataTable
                    Dim tableExists As Boolean = False

                    schemaTable = dbConnection.GetSchema("Tables")
                    For i As Integer = 0 To schemaTable.Rows.Count - 1
                        If schemaTable.Rows(i)!TABLE_NAME.ToString() = "PipeMessages" Then
                            tableExists = True
                        End If
                    Next
                    schemaTable = Nothing

                    If Not tableExists Then
                        oCommand = Nothing
                        dbConnection.Close()
                        Return False
                    End If

                    oCommand.CommandText = "SELECT COUNT(*) FROM PipeMessages"

                    Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
                    count = If(obj Is Nothing, 0, CType(obj, Integer))

                    oCommand = Nothing
                    dbConnection.Close()
                End Using

                Return count > 0
            End Function

#End Region

        End Class

        Private Function DateTime2BinaryString(ByVal DT As Nullable(Of DateTime)) As String
            If DT.HasValue Then Return DT.Value.ToBinary.ToString Else Return ""
        End Function

        Private Function BinaryString2DateTime(ByVal sDT As String) As Nullable(Of DateTime)
            If String.IsNullOrEmpty(sDT) Then Return Nothing Else Return DateTime.FromBinary(CLng(sDT))
        End Function

        <Serializable()> Public Class clsPipeParamaters 'C0461
            Public Property LoadTime As Nullable(Of DateTime) = Nothing
            Public Property Parameters As List(Of ParameterInfo)

            Private mPipeMessages As New clsPipeMessages

            Private mProjectVersion As ECCanvasDatabaseVersion

            Private mForceDefaultParameters As Boolean 'C0749

            Private mTableName As String
            Private mPropertyNameColumnName As String
            Private mPropertyValueColumnName As String
            Private mPipeMessagesTableName As String
            Private mXMLSettingsNodeName As String

            Private mParameterSets As List(Of ParameterSet)
            Private mCurrentParameterSet As ParameterSet
            Private mDefaultParameterSet As ParameterSet
            Private mTeamTimeParameterSet As ParameterSet
            Private mImpactParameterSet As ParameterSet
            Private mMeasureParameterSet As ParameterSet

            Private mProjectName As String
            Private mProjectPurpose As String

            Private mProjectType As ProjectType

            Private mFeedbackOn As New Dictionary(Of Integer, Boolean)

            Private mEvaluateObjectives As New Dictionary(Of Integer, Boolean)
            Private mEvaluateAlternatives As New Dictionary(Of Integer, Boolean)

            Private mModelEvalOrder As New Dictionary(Of Integer, ModelEvaluationOrder)

            Private mGraphicalPWMode As New Dictionary(Of Integer, GraphicalPairwiseMode)
            Private mGraphicalPWModeForAlts As New Dictionary(Of Integer, GraphicalPairwiseMode)

            Private mObjsEvalDirection As New Dictionary(Of Integer, ObjectivesEvaluationDirection)

            Private mPairwiseType As New Dictionary(Of Integer, PairwiseType)
            Private mPairwiseTypeForAlternatives As New Dictionary(Of Integer, PairwiseType) 'C0985
            Private mEvaluateDiagonals As New Dictionary(Of Integer, DiagonalsEvaluation)
            Private mEvaluateDiagonalsAdvanced As New Dictionary(Of Integer, DiagonalsEvaluationAdvanced) 'C1010
            Private mEvaluateDiagonalsAlternatives As New Dictionary(Of Integer, DiagonalsEvaluation) 'C0987
            Private mPWEvalOrder As New Dictionary(Of Integer, PairwiseEvaluationOrder)

            Private mObjectivesPairwiseOneAtATime As New Dictionary(Of Integer, Boolean) 'C0977
            Private mObjectivesNonPairwiseOneAtATime As New Dictionary(Of Integer, Boolean) 'C0977

            Private mAlternativesPairwiseOneAtATime As New Dictionary(Of Integer, Boolean) 'C0987

            Private mAltsEvalMode As New Dictionary(Of Integer, AlternativesEvaluationMode)

            Private mSynthesisMode As New Dictionary(Of Integer, ECSynthesisMode)
            Private mIdealViewType As New Dictionary(Of Integer, IdealViewType)

            Private mLocalResultsView As New Dictionary(Of Integer, ResultsView)
            Private mGlobalResultsView As New Dictionary(Of Integer, ResultsView)

            Private mWRTInfoDocsShowMode As New Dictionary(Of Integer, WRTInfoDocsShowMode) 'C1045

            Private mLocalResultsSortMode As New Dictionary(Of Integer, ResultsSortMode) 'C0820
            Private mGlobalResultsSortMode As New Dictionary(Of Integer, ResultsSortMode) 'C0820

            Private mShowConsistencyRatio As New Dictionary(Of Integer, Boolean)
            Private mShowComments As New Dictionary(Of Integer, Boolean)
            Private mShowInfoDocs As New Dictionary(Of Integer, Boolean)
            Private mShowInfoDocsMode As New Dictionary(Of Integer, ShowInfoDocsMode)

            Private mShowSA As New Dictionary(Of Integer, Boolean)
            Private mShowSAPerformance As New Dictionary(Of Integer, Boolean)
            Private mShowSAGradient As New Dictionary(Of Integer, Boolean)
            Private mSortSA As New Dictionary(Of Integer, Boolean)

            Private mObjsColNameInSA As New Dictionary(Of Integer, String)
            Private mAltsColNameInSA As New Dictionary(Of Integer, String)

            Private mCalcSAWRTGoal As New Dictionary(Of Integer, Boolean)
            Private mCalcSAForCombined As New Dictionary(Of Integer, Boolean)
            Private mAllowSwitchNodesInSA As New Dictionary(Of Integer, Boolean)

            Private mShowWelcomeScreen As New Dictionary(Of Integer, Boolean)
            Private mShowThankYouScreen As New Dictionary(Of Integer, Boolean)

            Private mShowWelcomeSurvey As New Dictionary(Of Integer, Boolean)
            Private mShowThankYouSurvey As New Dictionary(Of Integer, Boolean)
            Private mShowSurvey As New Dictionary(Of Integer, Boolean)
            Private mAllowAutoadvance As New Dictionary(Of Integer, Boolean)
            Private mShowNextUnassessed As New Dictionary(Of Integer, Boolean)

            Private mShowProgressIndicator As New Dictionary(Of Integer, Boolean)
            Private mAllowNavigation As New Dictionary(Of Integer, Boolean)
            Private mAllowMissingJudgments As New Dictionary(Of Integer, Boolean)

            Private mShowExpectedValueLocal As New Dictionary(Of Integer, Boolean)
            Private mShowExpectedValueGlobal As New Dictionary(Of Integer, Boolean)

            Private mForceGraphical As New Dictionary(Of Integer, Boolean)
            Private mForceGraphicalForAlternatives As New Dictionary(Of Integer, Boolean) 'C0985

            Private mForceAllDiagonals As New Dictionary(Of Integer, Boolean)
            Private mForceAllDiagonalsLimit As New Dictionary(Of Integer, Integer)
            Private mForceAllDiagonalsForAlternatives As New Dictionary(Of Integer, Boolean)
            Private mForceAllDiagonalsLimitForAlternatives As New Dictionary(Of Integer, Integer)

            Private mDisableWarningsOnStepChange As New Dictionary(Of Integer, Boolean)

            Private mIncludeIdealAlt As New Dictionary(Of Integer, Boolean)
            Private mShowIdealAlt As New Dictionary(Of Integer, Boolean)

            Private mUseCIS As New Dictionary(Of Integer, Boolean) 'C0824
            Private mUseWeights As New Dictionary(Of Integer, Boolean)
            Private mCombinedMode As New Dictionary(Of Integer, CombinedCalculationsMode) 'C0945

            Private mMeasureMode As New Dictionary(Of Integer, ECMeasureMode)

            Private mAltsDefContribution As New Dictionary(Of Integer, ECAltsDefaultContribution)
            Private mAltsDefContributionImpact As New Dictionary(Of Integer, ECAltsDefaultContribution)
            Private mDefaultCovObjMeasurementType As New Dictionary(Of Integer, ECMeasureType) 'C0753
            Private mDefaultNonCovObjMeasurementType As New Dictionary(Of Integer, ECMeasureType)

            Private mStartDate As New Dictionary(Of Integer, Nullable(Of DateTime)) 'C0015 + D0137
            Private mEndDate As New Dictionary(Of Integer, Nullable(Of DateTime))   'C0015 + D0137

            Private mSynchStartInPollingMode As New Dictionary(Of Integer, Boolean) 'C0104
            Private mSynchStartVotingOnceFacilitatorAllows As New Dictionary(Of Integer, Boolean) 'C0104
            Private mSynchShowGroupResults As New Dictionary(Of Integer, Boolean) 'C0104
            Private mSynchUseOptionsFromDecision As New Dictionary(Of Integer, Boolean) 'C0104
            Private mSynchUseVotingBoxes As New Dictionary(Of Integer, Boolean) 'C0104

            Private mTTAllowMeetingID As New Dictionary(Of Integer, Boolean) 'C0386
            Private mTTAllowOnlyEmailAddressesAddedToSessionToLogin As New Dictionary(Of Integer, Boolean) 'C0386
            Private mTTDisplayUsersWithViewOnlyAccess As New Dictionary(Of Integer, Boolean) 'C0386
            Private mTTStartInPracticeMode As New Dictionary(Of Integer, Boolean) 'C0386
            Private mTTShowKeypadNumbers As New Dictionary(Of Integer, Boolean) 'C0395
            Private mTTShowKeypadLegends As New Dictionary(Of Integer, Boolean) 'C0395
            Private mTTHideProjectOwner As New Dictionary(Of Integer, Boolean) 'C0432

            Private mTTStartInAnonymousMode As New Dictionary(Of Integer, Boolean) 'C0744

            Private mTTUsersSorting As New Dictionary(Of Integer, TTUsersSorting)

            Private mTTInvitationSubject2 As New Dictionary(Of Integer, String)
            Private mTTInvitationBody2 As New Dictionary(Of Integer, String)
            Private mEvaluateInvitationSubject2 As New Dictionary(Of Integer, String)
            Private mEvaluateInvitationBody2 As New Dictionary(Of Integer, String)

            Private mTTInvitationSubject As String
            Private mTTInvitationBody As String
            Private mEvaluateInvitationSubject As String
            Private mEvaluateInvitationBody As String

            Private mNameAlternatives As New Dictionary(Of Integer, String)
            Private mNameObjectives As New Dictionary(Of Integer, String)

            Private mInfoDocSize As String

            Private mTerminalRedirectURL As New Dictionary(Of Integer, String) 'C0110
            Private mRedirectAtTheEnd As New Dictionary(Of Integer, Boolean) 'C0115
            Private mLogOffAtTheEnd As New Dictionary(Of Integer, Boolean) 'C0115

            Private mRandomSequence As New Dictionary(Of Integer, Boolean) 'C0204

            Private mJudgementPromtID As New Dictionary(Of Integer, Integer) 'C0576
            Private mJudgementAltsPromtID As New Dictionary(Of Integer, Integer) 'C0619

            Private mJudgementPromtMultiID As New Dictionary(Of Integer, Integer)
            Private mJudgementAltsPromtMultiID As New Dictionary(Of Integer, Integer)

            Private mJudgementPromt As New Dictionary(Of Integer, String)
            Private mJudgementAltsPromt As New Dictionary(Of Integer, String)

            Private mJudgementPromtMulti As New Dictionary(Of Integer, String)
            Private mJudgementAltsPromtMulti As New Dictionary(Of Integer, String)

            'Private mShowFullObjectivePath As New Dictionary(Of Integer, Boolean) 'C0578
            Private mShowFullObjectivePath As New Dictionary(Of Integer, ecShowObjectivePath) 'C0578 + D4100

            Private mDefaultGroupID As New Dictionary(Of Integer, Integer)

            Private mAssociatedModelIntID As Integer
            Private mAssociatedModelGuidID As String

            Private mProjectGuid As Guid 'C0805

            Public Sub DoSomething()
                Dim propertyName As String = "NameAlternatives"
                Dim res As String = Versioned.CallByName(Me, propertyName, CallType.Get).ToString()
                Versioned.CallByName(Me, propertyName, CallType.Set).ToString()
            End Sub

            Public Property ProjectGuid() As Guid 'C0805
                Get
                    Return mProjectGuid
                End Get
                Set(ByVal value As Guid)
                    mProjectGuid = value
                End Set
            End Property

            Public Property DefaultCoveringObjectiveMeasurementType() As ECMeasureType 'C0753
                Get
                    If mDefaultCovObjMeasurementType.ContainsKey(CurrentParameterSet.ID) Then
                        Return mDefaultCovObjMeasurementType(CurrentParameterSet.ID)
                    Else
                        Return mDefaultCovObjMeasurementType(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ECMeasureType)
                    mDefaultCovObjMeasurementType(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property DefaultNonCoveringObjectiveMeasurementType() As ECMeasureType
                Get
                    If mDefaultNonCovObjMeasurementType.ContainsKey(CurrentParameterSet.ID) Then
                        Return mDefaultNonCovObjMeasurementType(CurrentParameterSet.ID)
                    Else
                        Return mDefaultNonCovObjMeasurementType(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ECMeasureType)
                    mDefaultNonCovObjMeasurementType(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ForceDefaultParameters() As Boolean 'C0749
                Get
                    Return mForceDefaultParameters
                End Get
                Set(ByVal value As Boolean)
                    mForceDefaultParameters = value
                End Set
            End Property

            Public Property TeamTimeInvitationSubject2() As String
                Get
                    If mTTInvitationSubject2.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTInvitationSubject2(CurrentParameterSet.ID)
                    Else
                        Return mTTInvitationSubject2(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mTTInvitationSubject2(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeInvitationBody2() As String
                Get
                    If mTTInvitationBody2.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTInvitationBody2(CurrentParameterSet.ID)
                    Else
                        Return mTTInvitationBody2(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mTTInvitationBody2(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property EvaluateInvitationSubject2() As String
                Get
                    If mEvaluateInvitationSubject2.ContainsKey(CurrentParameterSet.ID) Then
                        Return mEvaluateInvitationSubject2(CurrentParameterSet.ID)
                    Else
                        Return mEvaluateInvitationSubject2(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mEvaluateInvitationSubject2(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeInvitationSubject() As String
                Get
                    Return mTTInvitationSubject
                End Get
                Set(ByVal value As String)
                    mTTInvitationSubject = value
                End Set
            End Property

            Public Property TeamTimeInvitationBody() As String
                Get
                    Return mTTInvitationBody
                End Get
                Set(ByVal value As String)
                    mTTInvitationBody = value
                End Set
            End Property

            Public Property EvaluateInvitationSubject() As String
                Get
                    Return mEvaluateInvitationSubject
                End Get
                Set(ByVal value As String)
                    mEvaluateInvitationSubject = value
                End Set
            End Property

            Public Property NameAlternatives() As String
                Get
                    If mNameAlternatives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mNameAlternatives(CurrentParameterSet.ID)
                    Else
                        Return mNameAlternatives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mNameAlternatives(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property NameObjectives() As String
                Get
                    If mNameObjectives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mNameObjectives(CurrentParameterSet.ID)
                    Else
                        Return mNameObjectives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mNameObjectives(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property InfoDocSize As String
                Get
                    Return mInfoDocSize
                End Get
                Set(value As String)
                    mInfoDocSize = value
                End Set
            End Property

            Public Property EvaluateInvitationBody2() As String
                Get
                    If mEvaluateInvitationBody2.ContainsKey(CurrentParameterSet.ID) Then
                        Return mEvaluateInvitationBody2(CurrentParameterSet.ID)
                    Else
                        Return mEvaluateInvitationBody2(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mEvaluateInvitationBody2(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property EvaluateInvitationBody() As String
                Get
                    Return mEvaluateInvitationBody
                End Get
                Set(ByVal value As String)
                    mEvaluateInvitationBody = value
                End Set
            End Property

            Public Property ProjectVersion() As ECCanvasDatabaseVersion
                Get
                    Return mProjectVersion
                End Get
                Set(ByVal value As ECCanvasDatabaseVersion)
                    mProjectVersion = value
                End Set
            End Property

            'Public Property CurrentParameterSet() As ParameterSet 'C0740
            'Public Property CurrentParameterSet(Optional ByVal ForceDefault As Boolean = False) As ParameterSet 'C0740 'C0749
            Public Property CurrentParameterSet() As ParameterSet 'C0749
                Get
                    Return mCurrentParameterSet
                End Get
                Set(ByVal value As ParameterSet)
                    'C0740===
                    'If ForceDefault Then 'C0749
                    If ForceDefaultParameters Then 'C0749
                        mCurrentParameterSet = mDefaultParameterSet
                    Else
                        mCurrentParameterSet = value
                    End If
                    'C0740==
                    'mCurrentParameterSet = value 'C0740
                End Set
            End Property

            Public ReadOnly Property DefaultParameterSet() As ParameterSet
                Get
                    Return mDefaultParameterSet
                End Get
            End Property

            Public ReadOnly Property TeamTimeParameterSet() As ParameterSet
                Get
                    Return mDefaultParameterSet
                End Get
            End Property

            Public ReadOnly Property ImpactParameterSet() As ParameterSet
                Get
                    Return mImpactParameterSet
                End Get
            End Property

            Public ReadOnly Property MeasureParameterSet() As ParameterSet
                Get
                    Return mMeasureParameterSet
                End Get
            End Property

            Public Function AddParameterSet(ByVal Name As String) As ParameterSet
                If GetParameterSetByName(Name) IsNot Nothing Then
                    Return GetParameterSetByName(Name)
                Else
                    Dim newPS As New ParameterSet
                    newPS.Name = Name

                    Dim maxID As Integer = -1
                    For Each ps As ParameterSet In ParameterSets
                        If ps.ID > maxID Then
                            maxID = ps.ID
                        End If
                    Next

                    newPS.ID = maxID + 1
                    Return newPS
                End If
            End Function

            Public Property ParameterSets() As List(Of ParameterSet)
                Get
                    Return mParameterSets
                End Get
                Set(ByVal value As List(Of ParameterSet))
                    mParameterSets = value
                End Set
            End Property

            Private Function SetExists(ByVal SetID As Integer) As Boolean
                For Each ps As ParameterSet In mParameterSets
                    If ps.ID = SetID Then
                        Return True
                    End If
                Next
                Return False
            End Function

            Public Function GetParameterSetByID(ByVal SetID As Integer) As ParameterSet
                For Each ps As ParameterSet In mParameterSets
                    If ps.ID = SetID Then
                        Return ps
                    End If
                Next
                Return Nothing
            End Function

            Public Function GetParameterSetByName(ByVal SetName As String) As ParameterSet
                For Each ps As ParameterSet In mParameterSets
                    If ps.Name = SetName Then
                        Return ps
                    End If
                Next
                Return Nothing
            End Function

            Public Property TeamTimeHideProjectOwner() As Boolean 'C0432
                Get
                    If mTTHideProjectOwner.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTHideProjectOwner(CurrentParameterSet.ID)
                    Else
                        Return mTTHideProjectOwner(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mTTHideProjectOwner(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeStartInAnonymousMode() As Boolean 'C0744
                Get
                    If mTTStartInAnonymousMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTStartInAnonymousMode(CurrentParameterSet.ID)
                    Else
                        Return mTTStartInAnonymousMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mTTStartInAnonymousMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeShowKeypadNumbers() As Boolean 'C0395
                Get
                    If mTTShowKeypadNumbers.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTShowKeypadNumbers(CurrentParameterSet.ID)
                    Else
                        Return mTTShowKeypadNumbers(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mTTShowKeypadNumbers(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeShowKeypadLegends() As Boolean 'C0395
                Get
                    If mTTShowKeypadLegends.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTShowKeypadLegends(CurrentParameterSet.ID)
                    Else
                        Return mTTShowKeypadLegends(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mTTShowKeypadLegends(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeAllowMeetingID() As Boolean 'C0386
                Get
                    If mTTAllowMeetingID.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTAllowMeetingID(CurrentParameterSet.ID)
                    Else
                        Return mTTAllowMeetingID(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mTTAllowMeetingID(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin() As Boolean 'C0386
                Get
                    If mTTAllowOnlyEmailAddressesAddedToSessionToLogin.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTAllowOnlyEmailAddressesAddedToSessionToLogin(CurrentParameterSet.ID)
                    Else
                        Return mTTAllowOnlyEmailAddressesAddedToSessionToLogin(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mTTAllowOnlyEmailAddressesAddedToSessionToLogin(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeDisplayUsersWithViewOnlyAccess() As Boolean 'C0386
                Get
                    If mTTDisplayUsersWithViewOnlyAccess.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTDisplayUsersWithViewOnlyAccess(CurrentParameterSet.ID)
                    Else
                        Return mTTDisplayUsersWithViewOnlyAccess(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mTTDisplayUsersWithViewOnlyAccess(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeStartInPracticeMode() As Boolean 'C0386
                Get
                    If mTTStartInPracticeMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTStartInPracticeMode(CurrentParameterSet.ID)
                    Else
                        Return mTTStartInPracticeMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mTTStartInPracticeMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property RandomSequence() As Boolean 'C0204
                Get
                    If mRandomSequence.ContainsKey(CurrentParameterSet.ID) Then
                        Return mRandomSequence(CurrentParameterSet.ID)
                    Else
                        Return mRandomSequence(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mRandomSequence(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property PipeMessages() As clsPipeMessages
                Get
                    Return mPipeMessages
                End Get
                Set(ByVal value As clsPipeMessages)
                    mPipeMessages = value
                End Set
            End Property

            Public Property ProjectName() As String
                Get
                    Return mProjectName
                End Get
                Set(ByVal value As String)
                    mProjectName = value
                End Set
            End Property

            Public Property ProjectType() As ProjectType
                Get
                    Return mProjectType
                End Get
                Set(ByVal value As ProjectType)
                    mProjectType = value
                End Set
            End Property

            Public Property ProjectPurpose() As String
                Get
                    Return mProjectPurpose
                End Get
                Set(ByVal value As String)
                    mProjectPurpose = value
                End Set
            End Property

            Public Property IncludeIdealAlternative() As Boolean
                Get
                    If mIncludeIdealAlt.ContainsKey(CurrentParameterSet.ID) Then
                        Return mIncludeIdealAlt(CurrentParameterSet.ID)
                    Else
                        Return mIncludeIdealAlt(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mIncludeIdealAlt(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowIdealAlternative() As Boolean
                Get
                    If mShowIdealAlt.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowIdealAlt(CurrentParameterSet.ID)
                    Else
                        Return mShowIdealAlt(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowIdealAlt(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property UseCISForIndividuals() As Boolean 'C0824
                Get
                    If mUseCIS.ContainsKey(CurrentParameterSet.ID) Then
                        Return mUseCIS(CurrentParameterSet.ID)
                    Else
                        Return mUseCIS(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mUseCIS(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property UseWeights() As Boolean
                Get
                    If mUseWeights.ContainsKey(CurrentParameterSet.ID) Then
                        Return mUseWeights(CurrentParameterSet.ID)
                    Else
                        Return mUseWeights(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mUseWeights(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property DisableWarningsOnStepChange() As Boolean
                Get
                    If mDisableWarningsOnStepChange.ContainsKey(CurrentParameterSet.ID) Then
                        Return mDisableWarningsOnStepChange(CurrentParameterSet.ID)
                    Else
                        Return mDisableWarningsOnStepChange(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mDisableWarningsOnStepChange(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ForceGraphical() As Boolean
                Get
                    If mForceGraphical.ContainsKey(CurrentParameterSet.ID) Then
                        Return mForceGraphical(CurrentParameterSet.ID)
                    Else
                        Return mForceGraphical(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mForceGraphical(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ForceAllDiagonals() As Boolean
                Get
                    If mForceAllDiagonals.ContainsKey(CurrentParameterSet.ID) Then
                        Return mForceAllDiagonals(CurrentParameterSet.ID)
                    Else
                        Return mForceAllDiagonals(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mForceAllDiagonals(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ForceAllDiagonalsLimit() As Integer
                Get
                    If mForceAllDiagonalsLimit.ContainsKey(CurrentParameterSet.ID) Then
                        Return mForceAllDiagonalsLimit(CurrentParameterSet.ID)
                    Else
                        Return mForceAllDiagonalsLimit(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Integer)
                    mForceAllDiagonalsLimit(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ForceAllDiagonalsForAlternatives() As Boolean
                Get
                    If mForceAllDiagonalsForAlternatives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mForceAllDiagonalsForAlternatives(CurrentParameterSet.ID)
                    Else
                        Return mForceAllDiagonalsForAlternatives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mForceAllDiagonalsForAlternatives(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ForceAllDiagonalsLimitForAlternatives() As Integer
                Get
                    If mForceAllDiagonalsLimitForAlternatives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mForceAllDiagonalsLimitForAlternatives(CurrentParameterSet.ID)
                    Else
                        Return mForceAllDiagonalsLimitForAlternatives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Integer)
                    mForceAllDiagonalsLimitForAlternatives(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ForceGraphicalForAlternatives() As Boolean 'C0985
                Get
                    If mForceGraphicalForAlternatives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mForceGraphicalForAlternatives(CurrentParameterSet.ID)
                    Else
                        Return mForceGraphicalForAlternatives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mForceGraphicalForAlternatives(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property SortSensitivityAnalysis() As Boolean
                Get
                    If mSortSA.ContainsKey(CurrentParameterSet.ID) Then
                        Return mSortSA(CurrentParameterSet.ID)
                    Else
                        Return mSortSA(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mSortSA(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property CombinedMode() As CombinedCalculationsMode 'C0945
                Get
                    If mCombinedMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mCombinedMode(CurrentParameterSet.ID)
                    Else
                        Return mCombinedMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As CombinedCalculationsMode)
                    mCombinedMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property TeamTimeUsersSorting As TTUsersSorting
                Get
                    If mTTUsersSorting.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTTUsersSorting(CurrentParameterSet.ID)
                    Else
                        Return mTTUsersSorting(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As TTUsersSorting)
                    mTTUsersSorting(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property SynthesisMode() As ECSynthesisMode
                Get
                    If mSynthesisMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mSynthesisMode(CurrentParameterSet.ID)
                    Else
                        Return mSynthesisMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ECSynthesisMode)
                    mSynthesisMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property IdealViewType() As IdealViewType
                Get
                    If mIdealViewType.ContainsKey(CurrentParameterSet.ID) Then
                        Return mIdealViewType(CurrentParameterSet.ID)
                    Else
                        Return mIdealViewType(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As IdealViewType)
                    mIdealViewType(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property PairwiseType() As PairwiseType
                Get
                    If mPairwiseType.ContainsKey(CurrentParameterSet.ID) Then
                        Return mPairwiseType(CurrentParameterSet.ID)
                    Else
                        Return mPairwiseType(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As PairwiseType)
                    mPairwiseType(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property PairwiseTypeForAlternatives() As PairwiseType 'C0985
                Get
                    If mPairwiseTypeForAlternatives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mPairwiseTypeForAlternatives(CurrentParameterSet.ID)
                    Else
                        Return mPairwiseTypeForAlternatives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As PairwiseType)
                    mPairwiseTypeForAlternatives(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property FeedbackOn() As Boolean
                Get
                    If mFeedbackOn.ContainsKey(CurrentParameterSet.ID) Then
                        Return mFeedbackOn(CurrentParameterSet.ID)
                    Else
                        Return mFeedbackOn(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mFeedbackOn(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property EvaluateObjectives() As Boolean
                Get
                    If mEvaluateObjectives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mEvaluateObjectives(CurrentParameterSet.ID)
                    Else
                        Return mEvaluateObjectives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mEvaluateObjectives(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property EvaluateAlternatives() As Boolean
                Get
                    If mEvaluateAlternatives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mEvaluateAlternatives(CurrentParameterSet.ID)
                    Else
                        Return mEvaluateAlternatives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mEvaluateAlternatives(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ObjectivesPairwiseOneAtATime() As Boolean 'C0977
                Get
                    If mObjectivesPairwiseOneAtATime.ContainsKey(CurrentParameterSet.ID) Then
                        Return mObjectivesPairwiseOneAtATime(CurrentParameterSet.ID)
                    Else
                        Return mObjectivesPairwiseOneAtATime(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mObjectivesPairwiseOneAtATime(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ObjectivesNonPairwiseOneAtATime() As Boolean 'C0977
                Get
                    If mObjectivesNonPairwiseOneAtATime.ContainsKey(CurrentParameterSet.ID) Then    ' D2020
                        Return mObjectivesNonPairwiseOneAtATime(CurrentParameterSet.ID)
                    Else
                        Return mObjectivesNonPairwiseOneAtATime(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mObjectivesNonPairwiseOneAtATime(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AlternativesPairwiseOneAtATime() As Boolean 'C0987
                Get
                    If mAlternativesPairwiseOneAtATime.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAlternativesPairwiseOneAtATime(CurrentParameterSet.ID)
                    Else
                        Return mAlternativesPairwiseOneAtATime(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mAlternativesPairwiseOneAtATime(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ModelEvalOrder() As ModelEvaluationOrder
                Get
                    If mModelEvalOrder.ContainsKey(CurrentParameterSet.ID) Then
                        Return mModelEvalOrder(CurrentParameterSet.ID)
                    Else
                        Return mModelEvalOrder(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ModelEvaluationOrder)
                    mModelEvalOrder(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property GraphicalPWMode() As GraphicalPairwiseMode
                Get
                    If mGraphicalPWMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mGraphicalPWMode(CurrentParameterSet.ID)
                    Else
                        Return mGraphicalPWMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As GraphicalPairwiseMode)
                    mGraphicalPWMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property GraphicalPWModeForAlternatives() As GraphicalPairwiseMode
                Get
                    If mGraphicalPWModeForAlts.ContainsKey(CurrentParameterSet.ID) Then
                        Return mGraphicalPWModeForAlts(CurrentParameterSet.ID)
                    Else
                        Return mGraphicalPWModeForAlts(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As GraphicalPairwiseMode)
                    mGraphicalPWModeForAlts(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ObjectivesEvalDirection() As ObjectivesEvaluationDirection
                Get
                    If mObjsEvalDirection.ContainsKey(CurrentParameterSet.ID) Then
                        Return mObjsEvalDirection(CurrentParameterSet.ID)
                    Else
                        Return mObjsEvalDirection(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ObjectivesEvaluationDirection)
                    mObjsEvalDirection(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AlternativesEvalMode() As AlternativesEvaluationMode
                Get
                    If mAltsEvalMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAltsEvalMode(CurrentParameterSet.ID)
                    Else
                        Return mAltsEvalMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As AlternativesEvaluationMode)
                    mAltsEvalMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property EvaluateDiagonals() As DiagonalsEvaluation
                Get
                    If mEvaluateDiagonals.ContainsKey(CurrentParameterSet.ID) Then
                        Return mEvaluateDiagonals(CurrentParameterSet.ID)
                    Else
                        Return mEvaluateDiagonals(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As DiagonalsEvaluation)
                    mEvaluateDiagonals(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property EvaluateDiagonalsAdvanced() As DiagonalsEvaluationAdvanced 'C1010
                Get
                    If mEvaluateDiagonalsAdvanced.ContainsKey(CurrentParameterSet.ID) Then
                        Return mEvaluateDiagonalsAdvanced(CurrentParameterSet.ID)
                    Else
                        Return mEvaluateDiagonalsAdvanced(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As DiagonalsEvaluationAdvanced)
                    mEvaluateDiagonalsAdvanced(CurrentParameterSet.ID) = value
                End Set
            End Property


            Public Property EvaluateDiagonalsAlternatives() As DiagonalsEvaluation 'C0987
                Get
                    If mEvaluateDiagonalsAlternatives.ContainsKey(CurrentParameterSet.ID) Then
                        Return mEvaluateDiagonalsAlternatives(CurrentParameterSet.ID)
                    Else
                        Return mEvaluateDiagonalsAlternatives(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As DiagonalsEvaluation)
                    mEvaluateDiagonalsAlternatives(CurrentParameterSet.ID) = value
                End Set
            End Property



            Public Property PairwiseMatrixEvaluationOrder() As PairwiseEvaluationOrder
                Get
                    If mPWEvalOrder.ContainsKey(CurrentParameterSet.ID) Then
                        Return mPWEvalOrder(CurrentParameterSet.ID)
                    Else
                        Return mPWEvalOrder(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As PairwiseEvaluationOrder)
                    mPWEvalOrder(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property LocalResultsView() As ResultsView
                Get
                    If mLocalResultsView.ContainsKey(CurrentParameterSet.ID) Then
                        Return mLocalResultsView(CurrentParameterSet.ID)
                    Else
                        Return mLocalResultsView(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ResultsView)
                    mLocalResultsView(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property GlobalResultsView() As ResultsView
                Get
                    If mGlobalResultsView.ContainsKey(CurrentParameterSet.ID) Then
                        Return mGlobalResultsView(CurrentParameterSet.ID)
                    Else
                        Return mGlobalResultsView(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ResultsView)
                    mGlobalResultsView(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property WRTInfoDocsShowMode() As WRTInfoDocsShowMode 'C1045
                Get
                    If mWRTInfoDocsShowMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mWRTInfoDocsShowMode(CurrentParameterSet.ID)
                    Else
                        Return mWRTInfoDocsShowMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As WRTInfoDocsShowMode)
                    mWRTInfoDocsShowMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property LocalResultsSortMode() As ResultsSortMode 'C0820
                Get
                    If mLocalResultsSortMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mLocalResultsSortMode(CurrentParameterSet.ID)
                    Else
                        Return mLocalResultsSortMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ResultsSortMode)
                    mLocalResultsSortMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property GlobalResultsSortMode() As ResultsSortMode 'C0820
                Get
                    If mGlobalResultsSortMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mGlobalResultsSortMode(CurrentParameterSet.ID)
                    Else
                        Return mGlobalResultsSortMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ResultsSortMode)
                    mGlobalResultsSortMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowConsistencyRatio() As Boolean
                Get
                    If mShowConsistencyRatio.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowConsistencyRatio(CurrentParameterSet.ID)
                    Else
                        Return mShowConsistencyRatio(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowConsistencyRatio(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowComments() As Boolean
                Get
                    If mShowComments.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowComments(CurrentParameterSet.ID)
                    Else
                        Return mShowComments(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowComments(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowInfoDocs() As Boolean
                Get
                    If mShowInfoDocs.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowInfoDocs(CurrentParameterSet.ID)
                    Else
                        Return mShowInfoDocs(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowInfoDocs(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowInfoDocsMode() As ShowInfoDocsMode 'C0099
                Get
                    If mShowInfoDocsMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowInfoDocsMode(CurrentParameterSet.ID)
                    Else
                        Return mShowInfoDocsMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ShowInfoDocsMode)
                    mShowInfoDocsMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowWelcomeScreen() As Boolean
                Get
                    If mShowWelcomeScreen.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowWelcomeScreen(CurrentParameterSet.ID)
                    Else
                        Return mShowWelcomeScreen(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowWelcomeScreen(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowThankYouScreen() As Boolean
                Get
                    If mShowThankYouScreen.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowThankYouScreen(CurrentParameterSet.ID)
                    Else
                        Return mShowThankYouScreen(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowThankYouScreen(CurrentParameterSet.ID) = value
                End Set
            End Property

            'C0139===
            Public Property ShowWelcomeSurvey() As Boolean
                Get
                    If mShowWelcomeSurvey.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowWelcomeSurvey(CurrentParameterSet.ID)
                    Else
                        Return mShowWelcomeSurvey(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowWelcomeSurvey(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowThankYouSurvey() As Boolean
                Get
                    If mShowThankYouSurvey.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowThankYouSurvey(CurrentParameterSet.ID)
                    Else
                        Return mShowThankYouSurvey(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowThankYouSurvey(CurrentParameterSet.ID) = value
                End Set
            End Property
            'C0139==

            Public Property ShowSurvey() As Boolean
                Get
                    If mShowSurvey.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowSurvey(CurrentParameterSet.ID)
                    Else
                        Return mShowSurvey(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowSurvey(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowSensitivityAnalysis() As Boolean
                Get
                    If mShowSA.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowSA(CurrentParameterSet.ID)
                    Else
                        Return mShowSA(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowSA(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowSensitivityAnalysisPerformance() As Boolean 'C0078
                Get
                    If mShowSAPerformance.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowSAPerformance(CurrentParameterSet.ID)
                    Else
                        Return mShowSAPerformance(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowSAPerformance(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowSensitivityAnalysisGradient() As Boolean 'C0078
                Get
                    If mShowSAGradient.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowSAGradient(CurrentParameterSet.ID)
                    Else
                        Return mShowSAGradient(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowSAGradient(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ObjectivesColumnNameInSA() As String
                Get
                    If mObjsColNameInSA.ContainsKey(CurrentParameterSet.ID) Then
                        Return mObjsColNameInSA(CurrentParameterSet.ID)
                    Else
                        Return mObjsColNameInSA(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mObjsColNameInSA(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AlternativesColumnNameInSA() As String
                Get
                    If mAltsColNameInSA.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAltsColNameInSA(CurrentParameterSet.ID)
                    Else
                        Return mAltsColNameInSA(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mAltsColNameInSA(CurrentParameterSet.ID) = value
                End Set
            End Property


            Public Property CalculateSAWRTGoal() As Boolean
                Get
                    If mCalcSAWRTGoal.ContainsKey(CurrentParameterSet.ID) Then
                        Return mCalcSAWRTGoal(CurrentParameterSet.ID)
                    Else
                        Return mCalcSAWRTGoal(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mCalcSAWRTGoal(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property CalculateSAForCombined() As Boolean
                Get
                    If mCalcSAForCombined.ContainsKey(CurrentParameterSet.ID) Then
                        Return mCalcSAForCombined(CurrentParameterSet.ID)
                    Else
                        Return mCalcSAForCombined(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mCalcSAForCombined(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AllowSwitchNodesInSA() As Boolean
                Get
                    If mAllowSwitchNodesInSA.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAllowSwitchNodesInSA(CurrentParameterSet.ID)
                    Else
                        Return mAllowSwitchNodesInSA(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mAllowSwitchNodesInSA(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AllowAutoadvance() As Boolean
                Get
                    If mAllowAutoadvance.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAllowAutoadvance(CurrentParameterSet.ID)
                    Else
                        Return mAllowAutoadvance(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mAllowAutoadvance(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowNextUnassessed() As Boolean
                Get
                    If mShowNextUnassessed.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowNextUnassessed(CurrentParameterSet.ID)
                    Else
                        Return mShowNextUnassessed(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowNextUnassessed(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowProgressIndicator() As Boolean
                Get
                    If mShowProgressIndicator.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowProgressIndicator(CurrentParameterSet.ID)
                    Else
                        Return mShowProgressIndicator(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowProgressIndicator(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AllowNavigation() As Boolean
                Get
                    If mAllowNavigation.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAllowNavigation(CurrentParameterSet.ID)
                    Else
                        Return mAllowNavigation(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mAllowNavigation(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowExpectedValueLocal() As Boolean
                Get
                    If mShowExpectedValueLocal.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowExpectedValueLocal(CurrentParameterSet.ID)
                    Else
                        Return mShowExpectedValueLocal(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowExpectedValueLocal(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property ShowExpectedValueGlobal() As Boolean
                Get
                    If mShowExpectedValueGlobal.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowExpectedValueGlobal(CurrentParameterSet.ID)
                    Else
                        Return mShowExpectedValueGlobal(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mShowExpectedValueGlobal(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AllowMissingJudgments() As Boolean
                Get
                    If mAllowMissingJudgments.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAllowMissingJudgments(CurrentParameterSet.ID)
                    Else
                        Return mAllowMissingJudgments(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mAllowMissingJudgments(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property MeasureMode() As ECMeasureMode
                Get
                    If mMeasureMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mMeasureMode(CurrentParameterSet.ID)
                    Else
                        Return mMeasureMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ECMeasureMode)
                    mMeasureMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AltsDefaultContribution() As ECAltsDefaultContribution
                Get
                    If mAltsDefContribution.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAltsDefContribution(CurrentParameterSet.ID)
                    Else
                        Return mAltsDefContribution(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ECAltsDefaultContribution)
                    mAltsDefContribution(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AltsDefaultContributionImpact() As ECAltsDefaultContribution
                Get
                    Return ECAltsDefaultContribution.adcNone

                    If mAltsDefContributionImpact.ContainsKey(CurrentParameterSet.ID) Then
                        Return mAltsDefContributionImpact(CurrentParameterSet.ID)
                    Else
                        Return mAltsDefContributionImpact(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ECAltsDefaultContribution)
                    mAltsDefContributionImpact(CurrentParameterSet.ID) = value
                End Set
            End Property


            'C0015===
            Public Property StartDate() As System.Nullable(Of DateTime) ' D0137
                Get
                    If mStartDate.ContainsKey(CurrentParameterSet.ID) Then
                        Return mStartDate(CurrentParameterSet.ID)
                    Else
                        Return mStartDate(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As System.Nullable(Of DateTime))    ' D0137
                    mStartDate(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property EndDate() As System.Nullable(Of DateTime)   ' D0137
                Get
                    If mEndDate.ContainsKey(CurrentParameterSet.ID) Then
                        Return mEndDate(CurrentParameterSet.ID)
                    Else
                        Return mEndDate(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As System.Nullable(Of DateTime))    ' D0137
                    mEndDate(CurrentParameterSet.ID) = value
                End Set
            End Property
            'C0015==

            'C0104===
            Public Property SynchStartInPollingMode() As Boolean
                Get
                    If mSynchStartInPollingMode.ContainsKey(CurrentParameterSet.ID) Then
                        Return mSynchStartInPollingMode(CurrentParameterSet.ID)
                    Else
                        Return mSynchStartInPollingMode(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mSynchStartInPollingMode(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property SynchStartVotingOnceFacilitatorAllows() As Boolean
                Get
                    If mSynchStartVotingOnceFacilitatorAllows.ContainsKey(CurrentParameterSet.ID) Then
                        Return mSynchStartVotingOnceFacilitatorAllows(CurrentParameterSet.ID)
                    Else
                        Return mSynchStartVotingOnceFacilitatorAllows(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mSynchStartVotingOnceFacilitatorAllows(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property SynchShowGroupResults() As Boolean
                Get
                    If mSynchShowGroupResults.ContainsKey(CurrentParameterSet.ID) Then
                        Return mSynchShowGroupResults(CurrentParameterSet.ID)
                    Else
                        Return mSynchShowGroupResults(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mSynchShowGroupResults(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property SynchUseOptionsFromDecision() As Boolean
                Get
                    If mSynchUseOptionsFromDecision.ContainsKey(CurrentParameterSet.ID) Then
                        Return mSynchUseOptionsFromDecision(CurrentParameterSet.ID)
                    Else
                        Return mSynchUseOptionsFromDecision(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mSynchUseOptionsFromDecision(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property SynchUseVotingBoxes() As Boolean
                Get
                    If mSynchUseVotingBoxes.ContainsKey(CurrentParameterSet.ID) Then
                        Return mSynchUseVotingBoxes(CurrentParameterSet.ID)
                    Else
                        Return mSynchUseVotingBoxes(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mSynchUseVotingBoxes(CurrentParameterSet.ID) = value
                End Set
            End Property
            'C0104==

            'C0110===
            Public Property TerminalRedirectURL() As String
                Get
                    If mTerminalRedirectURL.ContainsKey(CurrentParameterSet.ID) Then
                        Return mTerminalRedirectURL(CurrentParameterSet.ID)
                    Else
                        Return mTerminalRedirectURL(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mTerminalRedirectURL(CurrentParameterSet.ID) = value
                End Set
            End Property
            'C0110==

            'C0115===
            Public Property RedirectAtTheEnd() As Boolean
                Get
                    If mRedirectAtTheEnd.ContainsKey(CurrentParameterSet.ID) Then
                        Return mRedirectAtTheEnd(CurrentParameterSet.ID)
                    Else
                        Return mRedirectAtTheEnd(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mRedirectAtTheEnd(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property LogOffAtTheEnd() As Boolean
                Get
                    If mLogOffAtTheEnd.ContainsKey(CurrentParameterSet.ID) Then
                        Return mLogOffAtTheEnd(CurrentParameterSet.ID)
                    Else
                        Return mLogOffAtTheEnd(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Boolean)
                    mLogOffAtTheEnd(CurrentParameterSet.ID) = value
                End Set
            End Property

            'Public Property ShowFullObjectivePath() As Boolean 'C0578
            Public Property ShowFullObjectivePath() As ecShowObjectivePath 'C0578 + D4100
                Get
                    If mShowFullObjectivePath.ContainsKey(CurrentParameterSet.ID) Then
                        Return mShowFullObjectivePath(CurrentParameterSet.ID)
                    Else
                        Return mShowFullObjectivePath(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As ecShowObjectivePath)
                    mShowFullObjectivePath(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property JudgementAltsPromtID() As Integer 'C0619
                Get
                    ' D6630 ===
                    Dim tID As Integer = 0
                    If mJudgementAltsPromtID.ContainsKey(CurrentParameterSet.ID) Then
                        tID = mJudgementAltsPromtID(CurrentParameterSet.ID)
                    Else
                        tID = mJudgementAltsPromtID(DefaultParameterSet.ID)
                    End If
                    If tID < 0 AndAlso String.IsNullOrEmpty(JudgementAltsPromt) Then
                        tID = 0
                        mJudgementAltsPromtID(CurrentParameterSet.ID) = tID
                    End If
                    Return tID
                    ' D6630 ==
                End Get
                Set(ByVal value As Integer)
                    mJudgementAltsPromtID(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property JudgementAltsPromtMultiID() As Integer
                Get
                    If mJudgementAltsPromtMultiID.ContainsKey(CurrentParameterSet.ID) Then
                        Return mJudgementAltsPromtMultiID(CurrentParameterSet.ID)
                    Else
                        Return mJudgementAltsPromtMultiID(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Integer)
                    mJudgementAltsPromtMultiID(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property JudgementPromt() As String
                Get
                    If mJudgementPromt.ContainsKey(CurrentParameterSet.ID) Then
                        Return mJudgementPromt(CurrentParameterSet.ID)
                    Else
                        Return mJudgementPromt(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mJudgementPromt(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property JudgementPromtMulti() As String
                Get
                    If mJudgementPromtMulti.ContainsKey(CurrentParameterSet.ID) Then
                        Return mJudgementPromtMulti(CurrentParameterSet.ID)
                    Else
                        Return mJudgementPromtMulti(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mJudgementPromtMulti(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property JudgementAltsPromt() As String
                Get
                    If mJudgementAltsPromt.ContainsKey(CurrentParameterSet.ID) Then
                        Return mJudgementAltsPromt(CurrentParameterSet.ID)
                    Else
                        Return mJudgementAltsPromt(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mJudgementAltsPromt(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property JudgementAltsPromtMulti() As String
                Get
                    If mJudgementAltsPromtMulti.ContainsKey(CurrentParameterSet.ID) Then
                        Return mJudgementAltsPromtMulti(CurrentParameterSet.ID)
                    Else
                        Return mJudgementAltsPromtMulti(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As String)
                    mJudgementAltsPromtMulti(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property JudgementPromtID() As Integer 'C0576
                Get
                    ' D6630 ===
                    Dim tID As Integer = 0
                    If mJudgementPromtID.ContainsKey(CurrentParameterSet.ID) Then
                        tID = mJudgementPromtID(CurrentParameterSet.ID)
                    Else
                        tID = mJudgementPromtID(DefaultParameterSet.ID)
                    End If
                    If tID < 0 AndAlso String.IsNullOrEmpty(JudgementPromt) Then
                        tID = 0
                        mJudgementPromtID(CurrentParameterSet.ID) = tID
                    End If
                    Return tID
                    ' D6630 ==
                End Get
                Set(ByVal value As Integer)
                    mJudgementPromtID(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property JudgementPromtMultiID() As Integer
                Get
                    If mJudgementPromtMultiID.ContainsKey(CurrentParameterSet.ID) Then
                        Return mJudgementPromtMultiID(CurrentParameterSet.ID)
                    Else
                        Return mJudgementPromtMultiID(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Integer)
                    mJudgementPromtMultiID(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property DefaultGroupID() As Integer
                Get
                    If mDefaultGroupID.ContainsKey(CurrentParameterSet.ID) Then
                        Return mDefaultGroupID(CurrentParameterSet.ID)
                    Else
                        Return mDefaultGroupID(DefaultParameterSet.ID)
                    End If
                End Get
                Set(ByVal value As Integer)
                    mDefaultGroupID(CurrentParameterSet.ID) = value
                End Set
            End Property

            Public Property AssociatedModelIntID() As Integer
                Get
                    Return mAssociatedModelIntID
                End Get
                Set(ByVal value As Integer)
                    mAssociatedModelIntID = value
                End Set
            End Property

            Public Property AssociatedModelGuidID() As String
                Get
                    Return mAssociatedModelGuidID
                End Get
                Set(ByVal value As String)
                    mAssociatedModelGuidID = value
                End Set
            End Property

            Public Sub RestoreDefaultParameterSet() 'C0469
                If DefaultParameterSet Is Nothing Then
                    Exit Sub
                End If

                Dim oldPR As ParameterSet = CurrentParameterSet
                CurrentParameterSet = DefaultParameterSet

                FeedbackOn = False
                EvaluateObjectives = True
                EvaluateAlternatives = True
                ObjectivesPairwiseOneAtATime = True 'C0977
                ObjectivesNonPairwiseOneAtATime = True 'C0977
                AlternativesPairwiseOneAtATime = True 'C0987
                ModelEvalOrder = ModelEvaluationOrder.meoObjectivesFirst
                GraphicalPWMode = GraphicalPairwiseMode.gpwmLessThan99
                GraphicalPWModeForAlternatives = GraphicalPairwiseMode.gpwmLessThan99
                ObjectivesEvalDirection = ObjectivesEvaluationDirection.oedTopToBottom
                AlternativesEvalMode = AlternativesEvaluationMode.aemAllAlternatives
                PairwiseType = PairwiseType.ptVerbal
                PairwiseTypeForAlternatives = PairwiseType.ptVerbal 'C0985
                SynthesisMode = ECSynthesisMode.smIdeal
                CombinedMode = CombinedCalculationsMode.cmAIJ 'C0945
                IdealViewType = IdealViewType.ivtNormalized
                EvaluateDiagonals = DiagonalsEvaluation.deFirstAndSecond
                EvaluateDiagonalsAlternatives = DiagonalsEvaluation.deFirstAndSecond 'C0987
                PairwiseMatrixEvaluationOrder = PairwiseEvaluationOrder.peoDiagonals
                LocalResultsView = ResultsView.rvIndividual
                GlobalResultsView = ResultsView.rvIndividual
                WRTInfoDocsShowMode = CanvasTypes.WRTInfoDocsShowMode.idsmBoth 'C1045
                ShowConsistencyRatio = False
                ShowComments = False
                ShowInfoDocs = True
                ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmFrame
                ShowWelcomeScreen = True
                ShowThankYouScreen = True
                ShowWelcomeSurvey = False
                ShowThankYouSurvey = False
                ShowSurvey = False
                ShowSensitivityAnalysis = False
                ShowSensitivityAnalysisPerformance = False
                ShowSensitivityAnalysisGradient = False
                SortSensitivityAnalysis = True
                CalculateSAForCombined = True
                CalculateSAWRTGoal = True
                AllowSwitchNodesInSA = True
                ObjectivesColumnNameInSA = "Objectives"
                AlternativesColumnNameInSA = "Alternatives"
                AllowAutoadvance = False
                ShowNextUnassessed = True
                ShowProgressIndicator = True
                AllowNavigation = True
                AllowMissingJudgments = True
                ShowExpectedValueLocal = False
                ShowExpectedValueGlobal = False
                DisableWarningsOnStepChange = True
                ForceGraphical = True
                ForceGraphicalForAlternatives = True 'C0985
                ForceAllDiagonals = False
                ForceAllDiagonalsLimit = 5
                ForceAllDiagonalsForAlternatives = False
                ForceAllDiagonalsLimitForAlternatives = 5
                IncludeIdealAlternative = True
                ShowIdealAlternative = False
                MeasureMode = ECMeasureMode.mmImportance
                AltsDefaultContribution = ECAltsDefaultContribution.adcFull
                AltsDefaultContributionImpact = ECAltsDefaultContribution.adcFull
                DefaultCoveringObjectiveMeasurementType = ECMeasureType.mtRatings 'C0753
                DefaultNonCoveringObjectiveMeasurementType = ECMeasureType.mtPairwise
                StartDate = Nothing
                EndDate = Nothing
                SynchStartInPollingMode = False
                SynchStartVotingOnceFacilitatorAllows = False
                SynchShowGroupResults = True
                SynchUseOptionsFromDecision = True
                SynchUseVotingBoxes = False
                TeamTimeAllowMeetingID = True
                TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin = False
                TeamTimeDisplayUsersWithViewOnlyAccess = True
                TeamTimeStartInPracticeMode = False
                TeamTimeShowKeypadNumbers = True
                TeamTimeShowKeypadLegends = True
                TeamTimeHideProjectOwner = False
                TerminalRedirectURL = ""
                JudgementPromt = ""
                JudgementAltsPromt = ""
                JudgementPromtMulti = ""
                JudgementAltsPromtMulti = ""
                RedirectAtTheEnd = False
                LogOffAtTheEnd = True   ' D1168
                RandomSequence = False
                'ShowFullObjectivePath = False 'C0578
                ShowFullObjectivePath = ecShowObjectivePath.DontShowPath  'C0578 + D4100
                UseCISForIndividuals = False 'C0824
                UseWeights = False
                TeamTimeUsersSorting = TTUsersSorting.ttusEmail

                CurrentParameterSet = oldPR
            End Sub

            Public Sub RestoreTeamTimeParameterSet() 'C0469
                If TeamTimeParameterSet Is Nothing Then
                    Exit Sub
                End If

                'SET TEAM TIME PARAMETERS
                Dim oldPS As ParameterSet = CurrentParameterSet
                CurrentParameterSet = TeamTimeParameterSet

                FeedbackOn = False
                EvaluateObjectives = True
                EvaluateAlternatives = True
                ObjectivesPairwiseOneAtATime = True 'C0977
                ObjectivesNonPairwiseOneAtATime = True 'C0977
                AlternativesPairwiseOneAtATime = True 'C0987
                ModelEvalOrder = ModelEvaluationOrder.meoObjectivesFirst
                GraphicalPWMode = GraphicalPairwiseMode.gpwmLessThan99
                GraphicalPWModeForAlternatives = GraphicalPairwiseMode.gpwmLessThan99
                ObjectivesEvalDirection = ObjectivesEvaluationDirection.oedTopToBottom
                AlternativesEvalMode = AlternativesEvaluationMode.aemOnePairAtATime
                PairwiseType = PairwiseType.ptVerbal
                PairwiseTypeForAlternatives = PairwiseType.ptVerbal 'C0985
                SynthesisMode = ECSynthesisMode.smIdeal
                CombinedMode = CombinedCalculationsMode.cmAIJ 'C0945
                IdealViewType = IdealViewType.ivtNormalized
                EvaluateDiagonals = DiagonalsEvaluation.deFirstAndSecond
                EvaluateDiagonalsAdvanced = DiagonalsEvaluationAdvanced.deMedium 'C1010
                EvaluateDiagonalsAlternatives = DiagonalsEvaluation.deFirstAndSecond 'C0987
                PairwiseMatrixEvaluationOrder = PairwiseEvaluationOrder.peoDiagonals
                LocalResultsView = ResultsView.rvIndividual
                GlobalResultsView = ResultsView.rvIndividual
                WRTInfoDocsShowMode = CanvasTypes.WRTInfoDocsShowMode.idsmBoth 'C1045
                ShowConsistencyRatio = False
                ShowComments = False
                ShowInfoDocs = True
                ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmFrame
                ShowWelcomeScreen = True
                ShowThankYouScreen = True
                ShowWelcomeSurvey = False
                ShowThankYouSurvey = False
                ShowSurvey = False
                ShowSensitivityAnalysis = False
                ShowSensitivityAnalysisPerformance = False
                ShowSensitivityAnalysisGradient = False
                SortSensitivityAnalysis = True
                CalculateSAForCombined = True
                CalculateSAWRTGoal = True
                AllowSwitchNodesInSA = True
                ObjectivesColumnNameInSA = "Objectives"
                AlternativesColumnNameInSA = "Alternatives"
                AllowAutoadvance = False
                ShowNextUnassessed = True
                ShowProgressIndicator = True
                ShowExpectedValueLocal = False
                ShowExpectedValueGlobal = False
                AllowNavigation = True
                AllowMissingJudgments = True
                DisableWarningsOnStepChange = False
                ForceGraphical = True
                ForceGraphicalForAlternatives = True 'C0985
                ForceAllDiagonals = False
                ForceAllDiagonalsLimit = 5
                ForceAllDiagonalsForAlternatives = False
                ForceAllDiagonalsLimitForAlternatives = 5
                IncludeIdealAlternative = True
                ShowIdealAlternative = False
                MeasureMode = ECMeasureMode.mmImportance
                AltsDefaultContribution = ECAltsDefaultContribution.adcFull
                AltsDefaultContributionImpact = ECAltsDefaultContribution.adcFull
                DefaultCoveringObjectiveMeasurementType = ECMeasureType.mtRatings 'C0753
                DefaultNonCoveringObjectiveMeasurementType = ECMeasureType.mtPairwise
                StartDate = Nothing
                EndDate = Nothing
                SynchStartInPollingMode = False
                SynchStartVotingOnceFacilitatorAllows = False
                SynchShowGroupResults = True
                SynchUseOptionsFromDecision = True
                SynchUseVotingBoxes = False
                TeamTimeAllowMeetingID = True
                TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin = False
                TeamTimeDisplayUsersWithViewOnlyAccess = True
                TeamTimeStartInPracticeMode = False
                TeamTimeShowKeypadNumbers = True
                TeamTimeShowKeypadLegends = True
                TeamTimeHideProjectOwner = False
                TerminalRedirectURL = ""
                JudgementPromt = ""
                JudgementAltsPromt = ""
                JudgementPromtMulti = ""
                JudgementAltsPromtMulti = ""
                RedirectAtTheEnd = False
                LogOffAtTheEnd = True   ' D1168
                RandomSequence = False
                UseCISForIndividuals = False 'C0824
                UseWeights = False
                TeamTimeUsersSorting = TTUsersSorting.ttusEmail

                CurrentParameterSet = oldPS
            End Sub

            'C0115==

            Public Sub New()
                Parameters = New List(Of ParameterInfo)

                mProjectVersion = GetCurrentDBVersion()

                mProjectType = CanvasTypes.ProjectType.ptRegular

                mForceDefaultParameters = False

                mTableName = PROPERTIES_DEFAULT_TABLE_NAME
                mPropertyNameColumnName = PROPERTY_NAME_DEFAULT_DB_COLUMN_NAME
                mPropertyValueColumnName = PROPERTY_VALUE_DEFAULT_DB_COLUMN_NAME
                mPipeMessagesTableName = MESSAGES_DEFAULT_TABLE_NAME
                mXMLSettingsNodeName = XML_SETTINGS_NODE_NAME

                mParameterSets = New List(Of ParameterSet)

                mDefaultParameterSet = New ParameterSet(PARAMETER_SET_DEFAULT, "DefaultParameterSet")
                mTeamTimeParameterSet = New ParameterSet(PARAMETER_SET_TEAM_TIME, "TeamTimeParameterSet")
                mImpactParameterSet = New ParameterSet(PARAMETER_SET_IMPACT, "ImpactParameterSet")
                mMeasureParameterSet = New ParameterSet(PARAMETER_SET_MEASURE, "MeasureParameterSet")

                mParameterSets.Add(mDefaultParameterSet)
                mParameterSets.Add(mTeamTimeParameterSet)
                mParameterSets.Add(mMeasureParameterSet)

                'SET DEFAULT PARAMETERS
                mCurrentParameterSet = mDefaultParameterSet

                FeedbackOn = False
                EvaluateObjectives = True
                EvaluateAlternatives = True
                ObjectivesPairwiseOneAtATime = True 'C0977
                ObjectivesNonPairwiseOneAtATime = True 'C0977
                AlternativesPairwiseOneAtATime = True 'C0987
                ModelEvalOrder = ModelEvaluationOrder.meoObjectivesFirst
                GraphicalPWMode = GraphicalPairwiseMode.gpwmLessThan99
                GraphicalPWModeForAlternatives = GraphicalPairwiseMode.gpwmLessThan99
                ObjectivesEvalDirection = ObjectivesEvaluationDirection.oedTopToBottom
                AlternativesEvalMode = AlternativesEvaluationMode.aemAllAlternatives
                PairwiseType = PairwiseType.ptVerbal
                PairwiseTypeForAlternatives = PairwiseType.ptVerbal 'C0985
                SynthesisMode = ECSynthesisMode.smIdeal
                CombinedMode = CombinedCalculationsMode.cmAIJ 'C0945
                IdealViewType = IdealViewType.ivtNormalized
                EvaluateDiagonals = DiagonalsEvaluation.deFirstAndSecond
                EvaluateDiagonalsAdvanced = DiagonalsEvaluationAdvanced.deMedium 'C1010
                EvaluateDiagonalsAlternatives = DiagonalsEvaluation.deFirstAndSecond 'C0987
                PairwiseMatrixEvaluationOrder = PairwiseEvaluationOrder.peoDiagonals
                LocalResultsView = ResultsView.rvIndividual
                GlobalResultsView = ResultsView.rvIndividual
                WRTInfoDocsShowMode = CanvasTypes.WRTInfoDocsShowMode.idsmBoth 'C1045
                LocalResultsSortMode = ResultsSortMode.rsmNumber 'C0820
                GlobalResultsSortMode = ResultsSortMode.rsmNumber 'C0820
                ShowConsistencyRatio = False
                ShowComments = True 'C0741
                ShowInfoDocs = True
                ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmFrame
                ShowWelcomeScreen = True
                ShowThankYouScreen = True
                ShowWelcomeSurvey = False
                ShowThankYouSurvey = False
                ShowSurvey = False
                ShowSensitivityAnalysis = False
                ShowSensitivityAnalysisPerformance = False
                ShowSensitivityAnalysisGradient = False
                SortSensitivityAnalysis = True
                CalculateSAForCombined = False 'C0730 - set to False
                CalculateSAWRTGoal = True
                AllowSwitchNodesInSA = True
                ObjectivesColumnNameInSA = "Objectives"
                AlternativesColumnNameInSA = "Alternatives"
                AllowAutoadvance = False
                ShowNextUnassessed = True
                ShowProgressIndicator = True
                AllowNavigation = True
                ShowExpectedValueLocal = False
                ShowExpectedValueGlobal = False
                AllowMissingJudgments = True
                DisableWarningsOnStepChange = True
                ForceGraphical = True
                ForceGraphicalForAlternatives = True 'C0985
                ForceAllDiagonals = True    ' D1514
                ForceAllDiagonalsLimit = 5
                ForceAllDiagonalsForAlternatives = True     ' D1514
                ForceAllDiagonalsLimitForAlternatives = 5
                IncludeIdealAlternative = True
                ShowIdealAlternative = False
                MeasureMode = ECMeasureMode.mmImportance
                AltsDefaultContribution = ECAltsDefaultContribution.adcFull
                AltsDefaultContributionImpact = ECAltsDefaultContribution.adcFull
                DefaultCoveringObjectiveMeasurementType = ECMeasureType.mtRatings 'C0753
                DefaultNonCoveringObjectiveMeasurementType = ECMeasureType.mtPairwise
                StartDate = Nothing
                EndDate = Nothing
                SynchStartInPollingMode = False
                SynchStartVotingOnceFacilitatorAllows = False
                SynchShowGroupResults = True
                SynchUseOptionsFromDecision = True
                SynchUseVotingBoxes = False
                TeamTimeAllowMeetingID = True
                TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin = True 'C0729 - changed to True
                TeamTimeDisplayUsersWithViewOnlyAccess = True
                TeamTimeStartInPracticeMode = False
                TeamTimeStartInAnonymousMode = False 'C0744
                TeamTimeShowKeypadNumbers = True
                TeamTimeShowKeypadLegends = True
                TeamTimeHideProjectOwner = False
                TeamTimeInvitationBody = ""     ' D0509
                TeamTimeInvitationSubject = ""  ' D0509
                EvaluateInvitationBody = ""     ' D0509
                EvaluateInvitationSubject = ""  ' D0509
                TeamTimeInvitationBody2 = ""     ' D0509
                TeamTimeInvitationSubject2 = ""  ' D0509
                EvaluateInvitationBody2 = ""     ' D0509
                EvaluateInvitationSubject2 = ""  ' D0509
                NameAlternatives = "Alternatives"
                NameObjectives = "Objectives"
                InfoDocSize = ""
                TerminalRedirectURL = ""
                JudgementPromt = ""
                JudgementAltsPromt = ""
                JudgementPromtMulti = ""
                JudgementAltsPromtMulti = ""
                RedirectAtTheEnd = False
                LogOffAtTheEnd = True           ' D1168
                RandomSequence = False
                JudgementPromtID = UNDEFINED_PROMT_ID 'C0576
                JudgementAltsPromtID = UNDEFINED_ALTS_PROMT_ID 'C0619
                JudgementPromtMultiID = UNDEFINED_PROMT_ID
                JudgementAltsPromtMultiID = UNDEFINED_ALTS_PROMT_ID
                'ShowFullObjectivePath = False 'C0578
                ShowFullObjectivePath = ecShowObjectivePath.DontShowPath  'C0578 + D4100
                UseCISForIndividuals = False 'C0824
                UseWeights = False
                TeamTimeUsersSorting = TTUsersSorting.ttusEmail
                DefaultGroupID = -1
                AssociatedModelIntID = -1
                AssociatedModelGuidID = ""


                'SET TEAM TIME PARAMETERS
                mCurrentParameterSet = mTeamTimeParameterSet

                FeedbackOn = False

                EvaluateObjectives = True
                EvaluateAlternatives = True
                ObjectivesPairwiseOneAtATime = True 'C0977
                ObjectivesNonPairwiseOneAtATime = True 'C0977
                AlternativesPairwiseOneAtATime = True 'C0987
                ModelEvalOrder = ModelEvaluationOrder.meoObjectivesFirst
                GraphicalPWMode = GraphicalPairwiseMode.gpwmLessThan99
                GraphicalPWModeForAlternatives = GraphicalPairwiseMode.gpwmLessThan99
                ObjectivesEvalDirection = ObjectivesEvaluationDirection.oedTopToBottom
                AlternativesEvalMode = AlternativesEvaluationMode.aemOnePairAtATime
                PairwiseType = PairwiseType.ptVerbal
                PairwiseTypeForAlternatives = PairwiseType.ptVerbal 'C0985
                SynthesisMode = ECSynthesisMode.smIdeal
                CombinedMode = CombinedCalculationsMode.cmAIJ 'C0945
                IdealViewType = IdealViewType.ivtNormalized
                EvaluateDiagonals = DiagonalsEvaluation.deFirstAndSecond
                EvaluateDiagonalsAdvanced = DiagonalsEvaluationAdvanced.deMedium 'C1010
                EvaluateDiagonalsAlternatives = DiagonalsEvaluation.deFirstAndSecond 'C0987
                PairwiseMatrixEvaluationOrder = PairwiseEvaluationOrder.peoDiagonals
                LocalResultsView = ResultsView.rvIndividual
                GlobalResultsView = ResultsView.rvIndividual
                WRTInfoDocsShowMode = CanvasTypes.WRTInfoDocsShowMode.idsmBoth 'C1045
                LocalResultsSortMode = ResultsSortMode.rsmNumber 'C0820
                GlobalResultsSortMode = ResultsSortMode.rsmNumber 'C0820
                ShowConsistencyRatio = False
                ShowComments = False
                ShowInfoDocs = True 'C0741
                ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmFrame
                ShowWelcomeScreen = True
                ShowThankYouScreen = True
                ShowWelcomeSurvey = False
                ShowThankYouSurvey = False
                ShowSurvey = False
                ShowSensitivityAnalysis = True 'C0473
                ShowSensitivityAnalysisPerformance = False
                ShowSensitivityAnalysisGradient = False
                SortSensitivityAnalysis = True
                CalculateSAForCombined = True
                CalculateSAWRTGoal = True
                AllowSwitchNodesInSA = True
                ObjectivesColumnNameInSA = "Objectives"
                AlternativesColumnNameInSA = "Alternatives"
                AllowAutoadvance = False
                ShowNextUnassessed = True
                ShowExpectedValueLocal = False
                ShowExpectedValueGlobal = False
                ShowProgressIndicator = True
                AllowNavigation = True
                AllowMissingJudgments = True
                DisableWarningsOnStepChange = False
                ForceGraphical = True
                ForceGraphicalForAlternatives = True 'C0985
                ForceAllDiagonals = False
                ForceAllDiagonalsLimit = 5
                ForceAllDiagonalsForAlternatives = False
                ForceAllDiagonalsLimitForAlternatives = 5
                IncludeIdealAlternative = True
                ShowIdealAlternative = False
                MeasureMode = ECMeasureMode.mmImportance
                AltsDefaultContribution = ECAltsDefaultContribution.adcFull
                AltsDefaultContributionImpact = ECAltsDefaultContribution.adcFull
                DefaultCoveringObjectiveMeasurementType = ECMeasureType.mtRatings 'C0753
                DefaultNonCoveringObjectiveMeasurementType = ECMeasureType.mtPairwise
                StartDate = Nothing
                EndDate = Nothing
                SynchStartInPollingMode = False
                SynchStartVotingOnceFacilitatorAllows = False
                SynchShowGroupResults = True
                SynchUseOptionsFromDecision = True
                SynchUseVotingBoxes = False
                TeamTimeAllowMeetingID = True
                TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin = True 'C0474
                TeamTimeDisplayUsersWithViewOnlyAccess = True
                TeamTimeStartInPracticeMode = False
                TeamTimeStartInAnonymousMode = False 'C0744
                TeamTimeShowKeypadNumbers = True
                TeamTimeShowKeypadLegends = True
                TeamTimeHideProjectOwner = False
                TeamTimeInvitationBody = ""     ' D0509
                TeamTimeInvitationSubject = ""  ' D0509
                EvaluateInvitationBody = ""     ' D0509
                EvaluateInvitationSubject = ""  ' D0509
                TeamTimeInvitationBody2 = ""     ' D0509
                TeamTimeInvitationSubject2 = ""  ' D0509
                EvaluateInvitationBody2 = ""     ' D0509
                EvaluateInvitationSubject2 = ""  ' D0509
                NameAlternatives = "Alternatives"
                NameObjectives = "Objectives"
                InfoDocSize = ""
                TerminalRedirectURL = ""
                RedirectAtTheEnd = False
                LogOffAtTheEnd = True           ' D1168
                RandomSequence = False
                JudgementPromtID = UNDEFINED_PROMT_ID
                JudgementAltsPromtID = UNDEFINED_ALTS_PROMT_ID
                JudgementPromtMultiID = UNDEFINED_PROMT_ID
                JudgementAltsPromtMultiID = UNDEFINED_ALTS_PROMT_ID
                JudgementPromt = ""
                JudgementAltsPromt = ""
                JudgementPromtMulti = ""
                JudgementAltsPromtMulti = ""
                'ShowFullObjectivePath = False 'C0578
                ShowFullObjectivePath = ecShowObjectivePath.DontShowPath 'C0578 + D4100
                UseWeights = False
                UseCISForIndividuals = False 'C0824
                UseWeights = False
                TeamTimeUsersSorting = TTUsersSorting.ttusEmail
                DefaultGroupID = -1
                AssociatedModelIntID = -1
                AssociatedModelGuidID = ""

                mCurrentParameterSet = mMeasureParameterSet
                PairwiseType = CanvasTypes.PairwiseType.ptGraphical
                EvaluateDiagonals = DiagonalsEvaluation.deFirstAndSecond
                ShowWelcomeScreen = False
                ShowThankYouScreen = False
                ShowSurvey = False
                ShowWelcomeSurvey = False
                ShowThankYouSurvey = False
                ShowInfoDocs = False
                ShowComments = False
                ShowSensitivityAnalysis = False
                ShowSensitivityAnalysisGradient = False
                ShowSensitivityAnalysisPerformance = False
                GlobalResultsView = ResultsView.rvNone
                LocalResultsView = ResultsView.rvIndividual
                FeedbackOn = False
                EvaluateObjectives = True
                EvaluateAlternatives = False

                ' SET DEFAULT PARAMETER SET
                mCurrentParameterSet = mDefaultParameterSet
            End Sub

            Private Sub ProcessLoadSettings(ByVal reader As XmlReader)
                While reader.Read()
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = PROJECT_NAME) Then
                        reader.Read()
                        ProjectName = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = PROJECT_TYPE) Then
                        reader.Read()
                        ProjectType = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = PROJECT_PURPOSE) Then
                        reader.Read()
                        ProjectPurpose = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = FEEDBACK_ON) Then
                        reader.Read()
                        FeedbackOn = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = EVALUATE_OBJECTIVES) Then
                        reader.Read()
                        EvaluateObjectives = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = EVALUATE_ALTERNATIVES) Then
                        reader.Read()
                        EvaluateAlternatives = reader.Value = "1"
                    End If
                    'C0977===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = OBJECTIVES_PAIRWISE_ONE_AT_A_TIME) Then
                        reader.Read()
                        ObjectivesPairwiseOneAtATime = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = OBJECTIVES_NON_PAIRWISE_ONE_AT_A_TIME) Then
                        reader.Read()
                        ObjectivesNonPairwiseOneAtATime = reader.Value = "1"
                    End If
                    'C0977==

                    'C0987===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALTERNATIVES_PAIRWISE_ONE_AT_A_TIME) Then
                        reader.Read()
                        AlternativesPairwiseOneAtATime = reader.Value = "1"
                    End If
                    'C0987==

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = MODEL_EVALUATION_ORDER) Then
                        reader.Read()
                        ModelEvalOrder = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = GRAPHICAL_PW_MODE) Then
                        reader.Read()
                        GraphicalPWMode = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = GRAPHICAL_PW_MODE_FOR_ALTS) Then
                        reader.Read()
                        GraphicalPWModeForAlternatives = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = OBJECTIVES_EVALUATION_DIRECTION) Then
                        reader.Read()
                        ObjectivesEvalDirection = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALTERNATIVES_EVALUATION_MODE) Then
                        reader.Read()
                        AlternativesEvalMode = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = PAIRWISE_TYPE) Then
                        reader.Read()
                        PairwiseType = reader.Value
                    End If
                    'C0985===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = PAIRWISE_TYPE_FOR_ALTERNATIVES) Then
                        reader.Read()
                        PairwiseTypeForAlternatives = reader.Value
                    End If
                    'C0985==
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SYNTHESIS_MODE) Then
                        reader.Read()
                        SynthesisMode = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = COMBINED_MODE) Then 'C0945
                        reader.Read()
                        CombinedMode = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = IDEAL_VIEW_TYPE) Then
                        reader.Read()
                        IdealViewType = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = EVALUATE_DIAGONALS) Then
                        reader.Read()
                        EvaluateDiagonals = reader.Value
                    End If
                    'C0987===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = EVALUATE_DIAGONALS_ALTERNATIVES) Then
                        reader.Read()
                        EvaluateDiagonalsAlternatives = reader.Value
                    End If
                    'C0987==
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = LOCAL_RESULTS_VIEW) Then
                        reader.Read()
                        LocalResultsView = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = GLOBAL_RESULTS_VIEW) Then
                        reader.Read()
                        GlobalResultsView = reader.Value
                    End If

                    'C1045===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = WRT_INFODOCS_SHOW_MODE) Then
                        reader.Read()
                        WRTInfoDocsShowMode = reader.Value
                    End If
                    'C1045==

                    'C0820===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = LOCAL_RESULTS_SORT_MODE) Then
                        reader.Read()
                        LocalResultsSortMode = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = GLOBAL_RESULTS_SORT_MODE) Then
                        reader.Read()
                        GlobalResultsSortMode = reader.Value
                    End If
                    'C0820==
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_CONSISTENCY_RATIO) Then
                        reader.Read()
                        ShowConsistencyRatio = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_COMMENTS) Then
                        reader.Read()
                        ShowComments = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_INFODOCS) Then
                        reader.Read()
                        ShowInfoDocs = reader.Value = "1"
                    End If
                    'C0824===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = USE_CIS) Then
                        reader.Read()
                        UseCISForIndividuals = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = USE_WEIGHTS) Then
                        reader.Read()
                        UseWeights = reader.Value = "1"
                    End If
                    'C0824==
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_INFODOCS_MODE) Then
                        reader.Read()
                        ShowInfoDocsMode = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_WELCOME_SCREEN) Then
                        reader.Read()
                        ShowWelcomeScreen = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_THANK_YOU_SCREEN) Then
                        reader.Read()
                        ShowThankYouScreen = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_WELCOME_SURVEY) Then
                        reader.Read()
                        ShowWelcomeSurvey = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_THANK_YOU_SURVEY) Then
                        reader.Read()
                        ShowThankYouSurvey = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_SURVEY) Then
                        reader.Read()
                        ShowSurvey = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_SENSITIVITY_ANALYSIS) Then
                        reader.Read()
                        ShowSensitivityAnalysis = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_SENSITIVITY_ANALYSIS_PERFORMANCE) Then
                        reader.Read()
                        ShowSensitivityAnalysisPerformance = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_SENSITIVITY_ANALYSIS_GRADIENT) Then
                        reader.Read()
                        ShowSensitivityAnalysisGradient = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALLOW_AUTOADVANCE) Then
                        reader.Read()
                        AllowAutoadvance = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_NEXT_UNASSESSED) Then
                        reader.Read()
                        ShowNextUnassessed = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_PROGRESS_INDICATOR) Then
                        reader.Read()
                        ShowProgressIndicator = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALLOW_NAVIGATION) Then
                        reader.Read()
                        AllowNavigation = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_EXPECTED_VALUE_LOCAL) Then
                        reader.Read()
                        ShowExpectedValueLocal = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_EXPECTED_VALUE_GLOBAL) Then
                        reader.Read()
                        ShowExpectedValueGlobal = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALLOW_MISSING_JUDGMENTS) Then
                        reader.Read()
                        AllowMissingJudgments = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = DISABLE_WARNINGS_ON_STEP_CHANGE) Then
                        reader.Read()
                        DisableWarningsOnStepChange = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = FORCE_GRAPHICAL_PAIRWISE) Then
                        reader.Read()
                        ForceGraphical = reader.Value = "1"
                    End If
                    'C0985===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = FORCE_GRAPHICAL_PAIRWISE_FOR_ALTERNATIVES) Then
                        reader.Read()
                        ForceGraphicalForAlternatives = reader.Value = "1"
                    End If
                    'C0985==
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = FORCE_ALL_DIAGONALS) Then
                        reader.Read()
                        ForceAllDiagonals = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = FORCE_ALL_DIAGONALS_LIMIT) Then
                        reader.Read()
                        ForceAllDiagonalsLimit = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = FORCE_ALL_DIAGONALS_FOR_ALTERNATIVES) Then
                        reader.Read()
                        ForceAllDiagonalsForAlternatives = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = FORCE_ALL_DIAGONALS_LIMIT_FOR_ALTERNATIVES) Then
                        reader.Read()
                        ForceAllDiagonalsLimitForAlternatives = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SORT_SENSITIVITY_ANALYSIS) Then
                        reader.Read()
                        SortSensitivityAnalysis = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = CALC_SA_FOR_COMBINED) Then
                        reader.Read()
                        CalculateSAForCombined = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = CALC_SA_WRT_GOAL) Then
                        reader.Read()
                        CalculateSAWRTGoal = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALLOW_SWITCH_SA_NODE) Then
                        reader.Read()
                        AllowSwitchNodesInSA = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = OBJS_COLUMN_NAME_IN_SA) Then
                        reader.Read()
                        ObjectivesColumnNameInSA = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALTS_COLUMN_NAME_IN_SA) Then
                        reader.Read()
                        AlternativesColumnNameInSA = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = INCLUDE_IDEAL_ALTERNATIVE) Then
                        reader.Read()
                        IncludeIdealAlternative = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_IDEAL_ALTERNATIVE) Then
                        reader.Read()
                        ShowIdealAlternative = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = MEASURE_MODE) Then
                        reader.Read()
                        MeasureMode = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALTS_DEFAULT_CONTRIBUTION) Then
                        reader.Read()
                        AltsDefaultContribution = reader.Value 'C0753
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ALTS_DEFAULT_CONTRIBUTION_IMPACT) Then
                        reader.Read()
                        AltsDefaultContributionImpact = reader.Value 'C0753
                    End If
                    'C0753===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = DEFAULT_COV_OBJ_MEASUREMENT_TYPE) Then
                        reader.Read()
                        DefaultCoveringObjectiveMeasurementType = reader.Value
                    End If
                    'C0753==
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = DEFAULT_NON_COV_OBJ_MEASUREMENT_TYPE) Then
                        reader.Read()
                        DefaultNonCoveringObjectiveMeasurementType = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = START_DATE) Then
                        reader.Read()
                        StartDate = BinaryString2DateTime(reader.Value)
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = END_DATE) Then
                        reader.Read()
                        EndDate = BinaryString2DateTime(reader.Value)
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SYNHRONOUS_START_IN_POLLING_MODE) Then
                        reader.Read()
                        SynchStartInPollingMode = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SYNHRONOUS_START_VOTING_ONCE_FACILITATOR_ALLOWS) Then
                        reader.Read()
                        SynchStartVotingOnceFacilitatorAllows = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SYNHRONOUS_SHOW_GROUP_RESULTS) Then
                        reader.Read()
                        SynchShowGroupResults = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SYNHRONOUS_USE_OPTIONS_FROM_DECISION) Then
                        reader.Read()
                        SynchUseOptionsFromDecision = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SYNHRONOUS_USE_VOTING_BOXES) Then
                        reader.Read()
                        SynchUseVotingBoxes = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TERMINAL_REDIRECT_URL) Then
                        reader.Read()
                        TerminalRedirectURL = reader.Value
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = REDIRECT_AT_THE_END) Then
                        reader.Read()
                        RedirectAtTheEnd = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = LOG_OFF_AT_THE_END) Then
                        reader.Read()
                        LogOffAtTheEnd = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_ALLOW_MEETING_ID) Then
                        reader.Read()
                        TeamTimeAllowMeetingID = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_ALLOW_ONLY_EMAIL_ADDRESSES_ADDED_TO_SESSION_TO_LOGIN) Then
                        reader.Read()
                        TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_DISPLAY_USERS_WITH_VIEW_ONLY_ACCESS) Then
                        reader.Read()
                        TeamTimeDisplayUsersWithViewOnlyAccess = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_START_IN_PRACTICE_MODE) Then
                        reader.Read()
                        TeamTimeStartInPracticeMode = reader.Value = "1"
                    End If
                    'C0744===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_START_IN_ANONYMOUS_MODE) Then
                        reader.Read()
                        TeamTimeStartInAnonymousMode = reader.Value = "1"
                    End If
                    'C0744==
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_SHOW_KEYPAD_NUMBERS) Then
                        reader.Read()
                        TeamTimeShowKeypadNumbers = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_SHOW_KEYPAD_LEGENDS) Then
                        reader.Read()
                        TeamTimeShowKeypadLegends = reader.Value = "1"
                    End If
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_HIDE_PROJECT_OWNER) Then
                        reader.Read()
                        TeamTimeHideProjectOwner = reader.Value = "1"
                    End If

                    'C0493===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_INVITATION_SUBJECT) Then
                        reader.Read()
                        TeamTimeInvitationSubject = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_INVITATION_BODY) Then
                        reader.Read()
                        TeamTimeInvitationBody = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_EVALUATE_INVITATION_SUBJECT) Then
                        reader.Read()
                        EvaluateInvitationSubject = reader.Value    ' D0509
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_EVALUATE_INVITATION_BODY) Then
                        reader.Read()
                        EvaluateInvitationBody = reader.Value   ' D0509
                    End If
                    'C0493==

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_INVITATION_SUBJECT2) Then
                        reader.Read()
                        TeamTimeInvitationSubject2 = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_INVITATION_BODY2) Then
                        reader.Read()
                        TeamTimeInvitationBody2 = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_EVALUATE_INVITATION_SUBJECT2) Then
                        reader.Read()
                        EvaluateInvitationSubject2 = reader.Value    ' D0509
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_EVALUATE_INVITATION_BODY2) Then
                        reader.Read()
                        EvaluateInvitationBody2 = reader.Value   ' D0509
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TEAM_TIME_USERS_SORTING) Then
                        reader.Read()
                        TeamTimeUsersSorting = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = NAME_ALTERNATIVES) Then
                        reader.Read()
                        NameAlternatives = reader.Value   ' D0509
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = NAME_OBJECTIVES) Then
                        reader.Read()
                        NameObjectives = reader.Value   ' D0509
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = INFODOC_SIZE) Then
                        reader.Read()
                        InfoDocSize = reader.Value   ' D0509
                    End If

                    'C0576===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = JUDGEMENT_PROMT_ID) Then
                        reader.Read()
                        JudgementPromtID = CInt(reader.Value)
                    End If
                    'C0576==

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = JUDGEMENT_PROMT_MULTI_ID) Then
                        reader.Read()
                        JudgementPromtMultiID = CInt(reader.Value)
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = DEFAULT_GROUP_ID) Then
                        reader.Read()
                        DefaultGroupID = CInt(reader.Value)
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ASSOCIATED_MODEL_INT_ID) Then
                        reader.Read()
                        AssociatedModelIntID = CInt(reader.Value)
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = ASSOCIATED_MODEL_GUID_ID) Then
                        reader.Read()
                        AssociatedModelGuidID = CInt(reader.Value)
                    End If

                    'C0619===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = JUDGEMENT_ALTS_PROMT_ID) Then
                        reader.Read()
                        JudgementAltsPromtID = CInt(reader.Value)
                    End If
                    'C0619==

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = JUDGEMENT_ALTS_PROMT_MULTI_ID) Then
                        reader.Read()
                        JudgementAltsPromtMultiID = CInt(reader.Value)
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = JUDGEMENT_PROMT) Then
                        reader.Read()
                        JudgementPromt = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = JUDGEMENT_ALTS_PROMT) Then
                        reader.Read()
                        JudgementAltsPromt = reader.Value
                    End If

                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = TERMINAL_REDIRECT_URL) Then
                        reader.Read()
                        TerminalRedirectURL = reader.Value
                    End If

                    'C0578===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = SHOW_FULL_OBJECTIVE_PATH) Then
                        reader.Read()
                        'ShowFullObjectivePath = reader.Value = "1"
                        ShowFullObjectivePath = CType(reader.Value, ecShowObjectivePath)   ' D4100
                    End If
                    'C0578==

                    'C0578===
                    If (reader.NodeType = XmlNodeType.Element) And (reader.Name = PROJECT_GUID) Then
                        reader.Read()
                        ProjectGuid = New Guid(reader.Value)
                    End If
                    'C0578==
                End While
            End Sub

            Private Function ReadFromXML(ByVal FilePath As String) As Boolean
                If Not My.Computer.FileSystem.FileExists(FilePath) Then
                    Return False
                End If

                Dim reader As XmlTextReader = Nothing
                Try
                    reader = New XmlTextReader(FilePath)
                    reader.WhitespaceHandling = WhitespaceHandling.None
                    While reader.Read()
                        If (reader.NodeType = XmlNodeType.Element) And (reader.Name = XMLSettingsNodeName) Then
                            mCurrentParameterSet = GetParameterSetByID(reader.GetAttribute("ParameterSetID"))
                            If mCurrentParameterSet Is Nothing Then mCurrentParameterSet = DefaultParameterSet
                            ProcessLoadSettings(reader.ReadSubtree())
                            mCurrentParameterSet = DefaultParameterSet
                        End If
                    End While
                Finally
                    If Not (reader Is Nothing) Then
                        reader.Close()
                    End If
                End Try

                Return True
            End Function

            'Private Function ReadFromDatabase(ByVal dbConnectionString As String) As Boolean 'C0236
            Private Function ReadFromDatabase(ByVal dbConnectionString As String, ByVal ProviderType As DBProviderType) As Boolean 'C0236
                ' check if the connection is alive
                If Not CheckDBConnection(ProviderType, dbConnectionString) Then
                    Return False
                End If

                ' if connection is ok then open it
                'Dim dbConnection As New OleDb.OleDbConnection(dbConnectionString)
                'Dim dbReader As OleDb.OleDbDataReader
                'Dim oCommand As OleDb.OleDbCommand

                'C0236===
                'Dim dbConnection As New odbc.odbcConnection(dbConnectionString) 
                'Dim dbReader As odbc.odbcDataReader 
                'Dim oCommand As odbc.odbcCommand 
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, dbConnectionString)  ' D2227
                    Dim dbReader As DbDataReader
                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection
                    'C0236==

                    dbConnection.Open()

                    'C0182===
                    'C0177===
                    '' check if table exists
                    Dim schemaTable As System.Data.DataTable
                    Dim tableExists As Boolean = False

                    'schemaTable = dbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, _
                    '              New Object() {Nothing, Nothing, Nothing, "TABLE"}) 
                    schemaTable = dbConnection.GetSchema("Tables")
                    For i As Integer = 0 To schemaTable.Rows.Count - 1
                        If schemaTable.Rows(i)!TABLE_NAME.ToString() = TableName Then
                            tableExists = True
                        End If
                    Next
                    schemaTable = Nothing

                    If Not tableExists Then
                        Return False
                    End If
                    'C0177==
                    'C0182==

                    ' if everything is ok then read all properties from the table
                    'oCommand = New OleDb.OleDbCommand("SELECT * FROM " + TableName, dbConnection) 

                    'oCommand = New odbc.odbcCommand("SELECT * FROM " + TableName, dbConnection) 'C0236
                    oCommand.CommandText = "SELECT * FROM " + TableName 'C0236

                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    ' read parameters from the table
                    If Not dbReader Is Nothing Then
                        While dbReader.Read()
                            If (Not TypeOf (dbReader.GetValue(0)) Is DBNull) And
                                (Not TypeOf (dbReader.GetValue(1)) Is DBNull) Then

                                Select Case CStr(dbReader(PropertyNameColumnName))
                                    Case PROJECT_NAME
                                        ProjectName = CStr(dbReader(PropertyValueColumnName))
                                    Case PROJECT_TYPE
                                        ProjectType = CInt(dbReader(PropertyValueColumnName))
                                    Case PROJECT_PURPOSE
                                        ProjectPurpose = CStr(dbReader(PropertyValueColumnName))
                                    Case FEEDBACK_ON
                                        FeedbackOn = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case EVALUATE_OBJECTIVES
                                        EvaluateObjectives = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case EVALUATE_ALTERNATIVES
                                        EvaluateAlternatives = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case OBJECTIVES_PAIRWISE_ONE_AT_A_TIME 'C0977
                                        ObjectivesPairwiseOneAtATime = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case OBJECTIVES_NON_PAIRWISE_ONE_AT_A_TIME 'C0977
                                        ObjectivesNonPairwiseOneAtATime = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case ALTERNATIVES_PAIRWISE_ONE_AT_A_TIME 'C0987
                                        AlternativesPairwiseOneAtATime = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case MODEL_EVALUATION_ORDER
                                        ModelEvalOrder = CInt(dbReader(PropertyValueColumnName))
                                    Case GRAPHICAL_PW_MODE
                                        Dim strValue As String = dbReader(PropertyValueColumnName)
                                        Dim intValue As Integer
                                        If Integer.TryParse(strValue, intValue) Then
                                            GraphicalPWMode = intValue
                                        Else
                                            GraphicalPWMode = GraphicalPairwiseMode.gpwmLessThan99
                                        End If
                                    Case GRAPHICAL_PW_MODE_FOR_ALTS
                                        GraphicalPWModeForAlternatives = CInt(dbReader(PropertyValueColumnName))
                                    Case MODEL_EVALUATION_ORDER
                                        ModelEvalOrder = CInt(dbReader(PropertyValueColumnName))
                                    Case OBJECTIVES_EVALUATION_DIRECTION
                                        ObjectivesEvalDirection = CInt(dbReader(PropertyValueColumnName))
                                    Case ALTERNATIVES_EVALUATION_MODE
                                        AlternativesEvalMode = CInt(dbReader(PropertyValueColumnName))
                                    Case PAIRWISE_TYPE
                                        PairwiseType = CInt(dbReader(PropertyValueColumnName))
                                    Case PAIRWISE_TYPE_FOR_ALTERNATIVES 'C0985
                                        PairwiseTypeForAlternatives = CInt(dbReader(PropertyValueColumnName))
                                    Case SYNTHESIS_MODE
                                        SynthesisMode = CInt(dbReader(PropertyValueColumnName))
                                    Case COMBINED_MODE 'C0945
                                        CombinedMode = CInt(dbReader(PropertyValueColumnName))
                                    Case IDEAL_VIEW_TYPE
                                        IdealViewType = CInt(dbReader(PropertyValueColumnName))
                                    Case EVALUATE_DIAGONALS
                                        EvaluateDiagonals = CInt(dbReader(PropertyValueColumnName))
                                    Case EVALUATE_DIAGONALS_ADVANCED 'C1010
                                        EvaluateDiagonalsAdvanced = CInt(dbReader(PropertyValueColumnName))
                                    Case EVALUATE_DIAGONALS_ALTERNATIVES 'C0987
                                        EvaluateDiagonalsAlternatives = CInt(dbReader(PropertyValueColumnName))
                                    Case LOCAL_RESULTS_VIEW
                                        LocalResultsView = CInt(dbReader(PropertyValueColumnName))
                                    Case GLOBAL_RESULTS_VIEW
                                        GlobalResultsView = CInt(dbReader(PropertyValueColumnName))
                                    Case WRT_INFODOCS_SHOW_MODE 'C1045
                                        WRTInfoDocsShowMode = CInt(dbReader(PropertyValueColumnName))
                                    Case LOCAL_RESULTS_SORT_MODE 'C0820
                                        LocalResultsSortMode = CInt(dbReader(PropertyValueColumnName))
                                    Case GLOBAL_RESULTS_SORT_MODE 'C0820
                                        GlobalResultsSortMode = CInt(dbReader(PropertyValueColumnName))
                                    Case SHOW_CONSISTENCY_RATIO
                                        ShowConsistencyRatio = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_COMMENTS
                                        ShowComments = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_INFODOCS
                                        ShowInfoDocs = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_INFODOCS_MODE 'C0099
                                        ShowInfoDocsMode = CInt(dbReader(PropertyValueColumnName)) 'C0099
                                    Case USE_CIS 'C0824
                                        ' UseCISForIndividuals = (CInt(dbReader(PropertyValueColumnName)) = 1) 'AS/5-5-16
                                        UseCISForIndividuals = (Math.Abs((CInt(CBool(dbReader(PropertyValueColumnName))))) = 1) 'AS/5-5-16
                                    Case USE_WEIGHTS
                                        UseWeights = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_WELCOME_SCREEN
                                        ShowWelcomeScreen = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_THANK_YOU_SCREEN
                                        ShowThankYouScreen = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_WELCOME_SURVEY
                                        ShowWelcomeSurvey = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_THANK_YOU_SURVEY
                                        ShowThankYouSurvey = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_SURVEY
                                        ShowSurvey = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_SENSITIVITY_ANALYSIS
                                        ShowSensitivityAnalysis = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_SENSITIVITY_ANALYSIS_PERFORMANCE 'C0078
                                        ShowSensitivityAnalysisPerformance = (CInt(dbReader(PropertyValueColumnName)) = 1) 'C0078
                                    Case SHOW_SENSITIVITY_ANALYSIS_GRADIENT 'C0078
                                        ShowSensitivityAnalysisGradient = (CInt(dbReader(PropertyValueColumnName)) = 1) 'C0078
                                    Case SORT_SENSITIVITY_ANALYSIS
                                        SortSensitivityAnalysis = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case CALC_SA_FOR_COMBINED
                                        CalculateSAForCombined = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case CALC_SA_WRT_GOAL
                                        CalculateSAWRTGoal = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case ALLOW_SWITCH_SA_NODE
                                        AllowSwitchNodesInSA = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case OBJS_COLUMN_NAME_IN_SA
                                        ObjectivesColumnNameInSA = CStr(dbReader(PropertyValueColumnName))
                                    Case ALTS_COLUMN_NAME_IN_SA
                                        AlternativesColumnNameInSA = CStr(dbReader(PropertyValueColumnName))
                                    Case ALLOW_AUTOADVANCE
                                        AllowAutoadvance = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_NEXT_UNASSESSED
                                        ShowNextUnassessed = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_PROGRESS_INDICATOR
                                        ShowProgressIndicator = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case ALLOW_NAVIGATION
                                        AllowNavigation = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_EXPECTED_VALUE_LOCAL
                                        ShowExpectedValueLocal = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_EXPECTED_VALUE_GLOBAL
                                        ShowExpectedValueGlobal = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case ALLOW_MISSING_JUDGMENTS
                                        AllowMissingJudgments = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case DISABLE_WARNINGS_ON_STEP_CHANGE
                                        DisableWarningsOnStepChange = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case FORCE_GRAPHICAL_PAIRWISE
                                        ForceGraphical = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case FORCE_GRAPHICAL_PAIRWISE_FOR_ALTERNATIVES 'C0985
                                        ForceGraphicalForAlternatives = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case FORCE_ALL_DIAGONALS
                                        ForceAllDiagonals = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case FORCE_ALL_DIAGONALS_LIMIT
                                        ForceAllDiagonalsLimit = CInt(dbReader(PropertyValueColumnName))
                                    Case FORCE_ALL_DIAGONALS_FOR_ALTERNATIVES
                                        ForceAllDiagonalsForAlternatives = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case FORCE_ALL_DIAGONALS_LIMIT_FOR_ALTERNATIVES
                                        ForceAllDiagonalsLimitForAlternatives = CInt(dbReader(PropertyValueColumnName))
                                    Case INCLUDE_IDEAL_ALTERNATIVE
                                        IncludeIdealAlternative = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SHOW_IDEAL_ALTERNATIVE
                                        ShowIdealAlternative = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case MEASURE_MODE
                                        MeasureMode = CInt(dbReader(PropertyValueColumnName))
                                    Case ALTS_DEFAULT_CONTRIBUTION
                                        AltsDefaultContribution = CInt(dbReader(PropertyValueColumnName))
                                    Case ALTS_DEFAULT_CONTRIBUTION_IMPACT
                                        AltsDefaultContributionImpact = CInt(dbReader(PropertyValueColumnName))
                                    Case DEFAULT_COV_OBJ_MEASUREMENT_TYPE 'C0753
                                        DefaultCoveringObjectiveMeasurementType = CInt(dbReader(PropertyValueColumnName))
                                    Case DEFAULT_NON_COV_OBJ_MEASUREMENT_TYPE
                                        DefaultNonCoveringObjectiveMeasurementType = CInt(dbReader(PropertyValueColumnName))
                                    Case START_DATE 'C0015
                                        StartDate = BinaryString2DateTime(dbReader(PropertyValueColumnName)) 'C0015 + D0137
                                    Case END_DATE 'C0015
                                        EndDate = BinaryString2DateTime(dbReader(PropertyValueColumnName)) 'C0015 + D0137
                                    Case SYNHRONOUS_START_IN_POLLING_MODE
                                        SynchStartInPollingMode = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SYNHRONOUS_START_VOTING_ONCE_FACILITATOR_ALLOWS
                                        SynchStartVotingOnceFacilitatorAllows = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SYNHRONOUS_SHOW_GROUP_RESULTS
                                        SynchShowGroupResults = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SYNHRONOUS_USE_OPTIONS_FROM_DECISION
                                        SynchUseOptionsFromDecision = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case SYNHRONOUS_USE_VOTING_BOXES
                                        SynchUseVotingBoxes = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TERMINAL_REDIRECT_URL
                                        TerminalRedirectURL = CStr(dbReader(PropertyValueColumnName))
                                    Case REDIRECT_AT_THE_END
                                        RedirectAtTheEnd = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case LOG_OFF_AT_THE_END
                                        LogOffAtTheEnd = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case RANDOM_SEQUENCE
                                        RandomSequence = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_ALLOW_MEETING_ID
                                        TeamTimeAllowMeetingID = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_ALLOW_ONLY_EMAIL_ADDRESSES_ADDED_TO_SESSION_TO_LOGIN
                                        TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_DISPLAY_USERS_WITH_VIEW_ONLY_ACCESS
                                        TeamTimeDisplayUsersWithViewOnlyAccess = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_START_IN_PRACTICE_MODE
                                        TeamTimeStartInPracticeMode = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_START_IN_ANONYMOUS_MODE 'C0744
                                        TeamTimeStartInAnonymousMode = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_SHOW_KEYPAD_NUMBERS 'C0395
                                        TeamTimeShowKeypadNumbers = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_SHOW_KEYPAD_LEGENDS 'C0395
                                        TeamTimeShowKeypadLegends = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_HIDE_PROJECT_OWNER 'C0432
                                        TeamTimeHideProjectOwner = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                    Case TEAM_TIME_INVITATION_SUBJECT 'C0493
                                        TeamTimeInvitationSubject = CStr(dbReader(PropertyValueColumnName))
                                    Case TEAM_TIME_INVITATION_BODY 'C0493
                                        TeamTimeInvitationBody = CStr(dbReader(PropertyValueColumnName))
                                    Case TEAM_TIME_EVALUATE_INVITATION_SUBJECT 'C0493
                                        EvaluateInvitationSubject = CStr(dbReader(PropertyValueColumnName)) ' D0509
                                    Case TEAM_TIME_EVALUATE_INVITATION_BODY 'C0493
                                        EvaluateInvitationBody = CStr(dbReader(PropertyValueColumnName))    ' D0509
                                    Case TEAM_TIME_INVITATION_SUBJECT2 'C0493
                                        TeamTimeInvitationSubject2 = CStr(dbReader(PropertyValueColumnName))
                                    Case TEAM_TIME_INVITATION_BODY2 'C0493
                                        TeamTimeInvitationBody2 = CStr(dbReader(PropertyValueColumnName))
                                    Case TEAM_TIME_EVALUATE_INVITATION_SUBJECT2 'C0493
                                        EvaluateInvitationSubject2 = CStr(dbReader(PropertyValueColumnName)) ' D0509
                                    Case TEAM_TIME_EVALUATE_INVITATION_BODY2 'C0493
                                        EvaluateInvitationBody2 = CStr(dbReader(PropertyValueColumnName))    ' D0509
                                    Case NAME_ALTERNATIVES
                                        NameAlternatives = CStr(dbReader(PropertyValueColumnName))    ' D0509
                                    Case NAME_OBJECTIVES
                                        NameObjectives = CStr(dbReader(PropertyValueColumnName))    ' D0509
                                    Case INFODOC_SIZE
                                        InfoDocSize = CStr(dbReader(PropertyValueColumnName))
                                    Case JUDGEMENT_PROMT_ID 'C0576
                                        JudgementPromtID = CInt(dbReader(PropertyValueColumnName))
                                    Case JUDGEMENT_ALTS_PROMT_ID 'C0619
                                        JudgementAltsPromtID = CInt(dbReader(PropertyValueColumnName))
                                    Case JUDGEMENT_PROMT
                                        JudgementPromt = CStr(dbReader(PropertyValueColumnName))
                                    Case JUDGEMENT_ALTS_PROMT
                                        JudgementAltsPromt = CStr(dbReader(PropertyValueColumnName))
                                    Case JUDGEMENT_PROMT_MULTI_ID
                                        JudgementPromtMultiID = CInt(dbReader(PropertyValueColumnName))
                                    Case JUDGEMENT_ALTS_PROMT_MULTI_ID
                                        JudgementAltsPromtMultiID = CInt(dbReader(PropertyValueColumnName))
                                    Case JUDGEMENT_PROMT_MULTI
                                        JudgementPromtMulti = CStr(dbReader(PropertyValueColumnName))
                                    Case JUDGEMENT_ALTS_PROMT_MULTI
                                        JudgementAltsPromtMulti = CStr(dbReader(PropertyValueColumnName))
                                    Case SHOW_FULL_OBJECTIVE_PATH 'C0578
                                        'ShowFullObjectivePath = (CInt(dbReader(PropertyValueColumnName)) = 1)
                                        ShowFullObjectivePath = CType(dbReader(PropertyValueColumnName), ecShowObjectivePath)      ' D4100
                                    Case TEAM_TIME_USERS_SORTING
                                        TeamTimeUsersSorting = CInt(dbReader(PropertyValueColumnName))
                                    Case DEFAULT_GROUP_ID
                                        DefaultGroupID = CInt(dbReader(PropertyValueColumnName))
                                    Case ASSOCIATED_MODEL_INT_ID
                                        AssociatedModelIntID = CInt(dbReader(PropertyValueColumnName))
                                    Case ASSOCIATED_MODEL_GUID_ID
                                        AssociatedModelGuidID = CStr(dbReader(PropertyValueColumnName))
                                    Case PROJECT_GUID 'C0805
                                        ProjectGuid = New Guid(CStr(dbReader(PropertyValueColumnName)))

                                        'C0574===
                                        'Case WELCOME_SCREEN_TEXT_FOR_AHP
                                        '    PipeMessages.SetWelcomeText(PipeMessageKind.pmkText, 0, 1, CStr(dbReader(PropertyValueColumnName))) 'C0573
                                        'Case THANK_YOU_SCREEN_TEXT_FOR_AHP
                                        '    PipeMessages.SetThankYouText(PipeMessageKind.pmkText, 0, 1, CStr(dbReader(PropertyValueColumnName))) 'C0573
                                        'C0574==
                                End Select
                            End If
                        End While
                    End If

                    AltsDefaultContribution = ECAltsDefaultContribution.adcFull
                    AltsDefaultContributionImpact = ECAltsDefaultContribution.adcFull

                    oCommand = Nothing
                    dbConnection.Close()
                End Using

                Return True
            End Function

            Public Function ReadFromStreamsDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, Optional ByVal ModelID As Integer = -1) As Boolean 'C0268
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stPipeOptions)))

                    Dim dbReader As DbDataReader
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim MS As New MemoryStream

                    If dbReader.HasRows Then
                        dbReader.Read()

                        Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long                   ' The bytes returned from GetBytes.
                        Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        ' Continue reading and writing while there are bytes beyond the size of the buffer.
                        Do While retval = bufferSize
                            bw.Write(outbyte)
                            bw.Flush()

                            ' Reposition the start index to the end of the last buffer and fill the buffer.
                            startIndex += bufferSize
                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                        Loop

                        ' Write the remaining buffer.
                        bw.Write(outbyte, 0, retval)
                        bw.Flush()

                        'bw.Close()

                        Dim time As Nullable(Of DateTime) = Nothing
                        If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                            time = dbReader("ModifyDate")
                        End If

                        dbReader.Close()

                        'C0743===
                        ''Create pipe parameters reader
                        'Dim reader As clsPipeParametersStreamReader
                        'reader = New clsPipeParametersStreamReader_v_1_1_8()

                        'If (ProjectVersion.CanvasVersion = 1) And (ProjectVersion.MajorVersion = 1) And (ProjectVersion.MinorVersion = 8) Then
                        '    reader = New clsPipeParametersStreamReader_v_1_1_8()
                        'End If

                        'If (ProjectVersion.CanvasVersion = 1) And (ProjectVersion.MajorVersion = 1) And (ProjectVersion.MinorVersion = 9) Then
                        '    reader = New clsPipeParametersStreamReader_v_1_1_9()
                        'End If

                        'If (ProjectVersion.CanvasVersion = 1) And (ProjectVersion.MajorVersion = 1) And (ProjectVersion.MinorVersion = 10) Then
                        '    reader = New clsPipeParametersStreamReader_v_1_1_10()
                        'End If

                        ''C0637===
                        'If (ProjectVersion.CanvasVersion = 1) And (ProjectVersion.MajorVersion = 1) And (ProjectVersion.MinorVersion = 11) Then
                        '    reader = New clsPipeParametersStreamReader_v_1_1_11()
                        'End If
                        ''C0637==

                        ''C0646===
                        'If (ProjectVersion.CanvasVersion = 1) And (ProjectVersion.MajorVersion = 1) And (ProjectVersion.MinorVersion = 12) Then
                        '    reader = New clsPipeParametersStreamReader_v_1_1_12()
                        'End If
                        ''C0646==
                        'C0743==

                        If time Is Nothing Then
                            time = Now

                            oCommand.CommandText = "UPDATE ModelStructure SET ModifyDate=? WHERE ProjectID=? AND StructureType=?"
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", time))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType.stPipeOptions))
                            Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                        End If

                        dbConnection.Close()


                        Dim reader As clsPipeParametersStreamReader = GetReader() 'C0743
                        reader.PipeParameters = Me
                        Dim b As Boolean = reader.Read(MS)
                        If b Then
                            LoadTime = time
                            Return True
                        End If
                    Else
                        dbReader.Close()
                        dbConnection.Close()
                        Return False
                    End If
                End Using

            End Function

            'Public Function Read(ByVal StorageType As PipeStorageType, ByVal Location As String) As Boolean 'C0236
            'Public Function Read(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType) As Boolean 'C0236 'C0268
            'Public Function Read(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, Optional ByVal ModelID As Integer = -1) As Boolean 'C0268 'C0390
            Public Function Read(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean 'C0390
                Select Case StorageType
                    Case PipeStorageType.pstXMLFile
                        mCurrentParameterSet = DefaultParameterSet
                        Return ReadFromXML(Location)
                    Case PipeStorageType.pstDatabase
                        'mPipeMessages.LoadFromDatabase(Location) 'C0236
                        mCurrentParameterSet = DefaultParameterSet
                        mPipeMessages.LoadFromDatabase(Location, ProviderType) 'C0236
                        'Return ReadFromDatabase(Location) 'C0236
                        Return ReadFromDatabase(Location, ProviderType) 'C0236
                    Case PipeStorageType.pstStreamsDatabase 'C0268
                        mPipeMessages.LoadFromStreamsDatabase(Location, ModelID, ProviderType) 'C0355
                        ReadFromStreamsDatabase(Location, ProviderType, ModelID)
                End Select
                Return False
            End Function


            Private Function WriteToXML(ByVal FilePath As String) As Boolean
                Dim writer As XmlTextWriter = Nothing
                Try
                    writer = New XmlTextWriter(FilePath, System.Text.Encoding.Unicode)
                    writer.Formatting = Formatting.Indented
                    writer.WriteStartDocument()

                    writer.WriteStartElement(XMLSettingsNodeName)

                    ' -D2183
                    'writer.WriteStartElement(PROJECT_NAME)
                    'writer.WriteString(ProjectName)
                    'writer.WriteEndElement()

                    writer.WriteStartElement(PROJECT_PURPOSE)
                    writer.WriteString(ProjectPurpose)
                    writer.WriteEndElement()

                    writer.WriteStartElement(FEEDBACK_ON)
                    writer.WriteString(If(FeedbackOn, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(EVALUATE_OBJECTIVES)
                    writer.WriteString(If(EvaluateObjectives, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(EVALUATE_ALTERNATIVES)
                    writer.WriteString(If(EvaluateAlternatives, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(MODEL_EVALUATION_ORDER)
                    writer.WriteString(ModelEvalOrder)
                    writer.WriteEndElement()

                    writer.WriteStartElement(GRAPHICAL_PW_MODE)
                    writer.WriteString(GraphicalPWMode)
                    writer.WriteEndElement()

                    writer.WriteStartElement(GRAPHICAL_PW_MODE_FOR_ALTS)
                    writer.WriteString(GraphicalPWModeForAlternatives)
                    writer.WriteEndElement()

                    writer.WriteStartElement(OBJECTIVES_EVALUATION_DIRECTION)
                    writer.WriteString(ObjectivesEvalDirection)
                    writer.WriteEndElement()

                    writer.WriteStartElement(ALTERNATIVES_EVALUATION_MODE)
                    writer.WriteString(AlternativesEvalMode)
                    writer.WriteEndElement()

                    writer.WriteStartElement(PAIRWISE_TYPE)
                    writer.WriteString(PairwiseType)
                    writer.WriteEndElement()

                    'C0985===
                    writer.WriteStartElement(PAIRWISE_TYPE_FOR_ALTERNATIVES)
                    writer.WriteString(PairwiseTypeForAlternatives)
                    writer.WriteEndElement()
                    'C0985==

                    writer.WriteStartElement(SYNTHESIS_MODE)
                    writer.WriteString(SynthesisMode)
                    writer.WriteEndElement()

                    'C0945===
                    writer.WriteStartElement(COMBINED_MODE)
                    writer.WriteString(CombinedMode)
                    writer.WriteEndElement()
                    'C0945==

                    writer.WriteStartElement(IDEAL_VIEW_TYPE)
                    writer.WriteString(IdealViewType)
                    writer.WriteEndElement()

                    writer.WriteStartElement(EVALUATE_DIAGONALS)
                    writer.WriteString(EvaluateDiagonals)
                    writer.WriteEndElement()

                    writer.WriteStartElement(EVALUATE_DIAGONALS_ADVANCED)
                    writer.WriteString(EvaluateDiagonalsAdvanced)
                    writer.WriteEndElement()

                    'C0987===
                    writer.WriteStartElement(EVALUATE_DIAGONALS_ALTERNATIVES)
                    writer.WriteString(EvaluateDiagonalsAlternatives)
                    writer.WriteEndElement()
                    'C0987==

                    writer.WriteStartElement(LOCAL_RESULTS_VIEW)
                    writer.WriteString(LocalResultsView)
                    writer.WriteEndElement()

                    writer.WriteStartElement(GLOBAL_RESULTS_VIEW)
                    writer.WriteString(GlobalResultsView)
                    writer.WriteEndElement()

                    'C1045===
                    writer.WriteStartElement(WRT_INFODOCS_SHOW_MODE)
                    writer.WriteString(WRTInfoDocsShowMode)
                    writer.WriteEndElement()
                    'C1045==

                    'C0820===
                    writer.WriteStartElement(LOCAL_RESULTS_SORT_MODE)
                    writer.WriteString(LocalResultsSortMode)
                    writer.WriteEndElement()

                    writer.WriteStartElement(GLOBAL_RESULTS_SORT_MODE)
                    writer.WriteString(GlobalResultsSortMode)
                    writer.WriteEndElement()
                    'C0820==

                    writer.WriteStartElement(SHOW_CONSISTENCY_RATIO)
                    writer.WriteString(If(ShowConsistencyRatio, 1, 0))
                    writer.WriteEndElement()

                    'C0824===
                    writer.WriteStartElement(USE_CIS)
                    writer.WriteString(If(UseCISForIndividuals, 1, 0))
                    writer.WriteEndElement()
                    'C0824==

                    writer.WriteStartElement(USE_WEIGHTS)
                    writer.WriteString(If(UseWeights, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_COMMENTS)
                    writer.WriteString(If(ShowComments, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_INFODOCS)
                    writer.WriteString(If(ShowInfoDocs, 1, 0))
                    writer.WriteEndElement()

                    'C0099===
                    writer.WriteStartElement(SHOW_INFODOCS_MODE)
                    writer.WriteString(ShowInfoDocsMode)
                    writer.WriteEndElement()
                    'C0099==

                    writer.WriteStartElement(SHOW_WELCOME_SCREEN)
                    writer.WriteString(If(ShowWelcomeScreen, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_THANK_YOU_SCREEN)
                    writer.WriteString(If(ShowThankYouScreen, 1, 0))
                    writer.WriteEndElement()

                    'C0139===
                    writer.WriteStartElement(SHOW_WELCOME_SURVEY)
                    writer.WriteString(If(ShowWelcomeSurvey, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_THANK_YOU_SURVEY)
                    writer.WriteString(If(ShowThankYouSurvey, 1, 0))
                    writer.WriteEndElement()
                    'C0139==

                    writer.WriteStartElement(SHOW_SURVEY)
                    writer.WriteString(If(ShowSurvey, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_SENSITIVITY_ANALYSIS)
                    writer.WriteString(If(ShowSensitivityAnalysis, 1, 0))
                    writer.WriteEndElement()

                    'C0078===
                    writer.WriteStartElement(SHOW_SENSITIVITY_ANALYSIS_PERFORMANCE)
                    writer.WriteString(If(ShowSensitivityAnalysisPerformance, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_SENSITIVITY_ANALYSIS_GRADIENT)
                    writer.WriteString(If(ShowSensitivityAnalysisGradient, 1, 0))
                    writer.WriteEndElement()
                    'C0078==

                    writer.WriteStartElement(SORT_SENSITIVITY_ANALYSIS)
                    writer.WriteString(If(SortSensitivityAnalysis, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(OBJS_COLUMN_NAME_IN_SA)
                    writer.WriteString(ObjectivesColumnNameInSA)
                    writer.WriteEndElement()

                    writer.WriteStartElement(ALTS_COLUMN_NAME_IN_SA)
                    writer.WriteString(AlternativesColumnNameInSA)
                    writer.WriteEndElement()

                    writer.WriteStartElement(CALC_SA_FOR_COMBINED)
                    writer.WriteString(If(CalculateSAForCombined, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(CALC_SA_WRT_GOAL)
                    writer.WriteString(If(CalculateSAWRTGoal, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(ALLOW_SWITCH_SA_NODE)
                    writer.WriteString(If(AllowSwitchNodesInSA, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(ALLOW_AUTOADVANCE)
                    writer.WriteString(If(AllowAutoadvance, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_NEXT_UNASSESSED)
                    writer.WriteString(If(ShowNextUnassessed, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_PROGRESS_INDICATOR)
                    writer.WriteString(If(ShowProgressIndicator, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(ALLOW_NAVIGATION)
                    writer.WriteString(If(AllowNavigation, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_EXPECTED_VALUE_LOCAL)
                    writer.WriteString(If(ShowExpectedValueLocal, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_EXPECTED_VALUE_GLOBAL)
                    writer.WriteString(If(ShowExpectedValueGlobal, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(ALLOW_MISSING_JUDGMENTS)
                    writer.WriteString(If(AllowMissingJudgments, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(DISABLE_WARNINGS_ON_STEP_CHANGE)
                    writer.WriteString(If(DisableWarningsOnStepChange, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(FORCE_GRAPHICAL_PAIRWISE)
                    writer.WriteString(If(ForceGraphical, 1, 0))
                    writer.WriteEndElement()

                    'C0985===
                    writer.WriteStartElement(FORCE_GRAPHICAL_PAIRWISE_FOR_ALTERNATIVES)
                    writer.WriteString(If(ForceGraphicalForAlternatives, 1, 0))
                    writer.WriteEndElement()
                    'C0985==

                    writer.WriteStartElement(FORCE_ALL_DIAGONALS)
                    writer.WriteString(If(ForceAllDiagonals, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(FORCE_ALL_DIAGONALS_LIMIT)
                    writer.WriteString(ForceAllDiagonalsLimit)
                    writer.WriteEndElement()

                    writer.WriteStartElement(FORCE_ALL_DIAGONALS_FOR_ALTERNATIVES)
                    writer.WriteString(If(ForceAllDiagonalsForAlternatives, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(FORCE_ALL_DIAGONALS_LIMIT_FOR_ALTERNATIVES)
                    writer.WriteString(ForceAllDiagonalsLimitForAlternatives)
                    writer.WriteEndElement()

                    writer.WriteStartElement(INCLUDE_IDEAL_ALTERNATIVE)
                    writer.WriteString(If(IncludeIdealAlternative, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SHOW_IDEAL_ALTERNATIVE)
                    writer.WriteString(If(ShowIdealAlternative, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(MEASURE_MODE)
                    writer.WriteString(MeasureMode)
                    writer.WriteEndElement()

                    writer.WriteStartElement(ALTS_DEFAULT_CONTRIBUTION)
                    writer.WriteString(AltsDefaultContribution)
                    writer.WriteEndElement()

                    writer.WriteStartElement(ALTS_DEFAULT_CONTRIBUTION_IMPACT)
                    writer.WriteString(AltsDefaultContributionImpact)
                    writer.WriteEndElement()

                    'C0753===
                    writer.WriteStartElement(DEFAULT_COV_OBJ_MEASUREMENT_TYPE)
                    writer.WriteString(DefaultCoveringObjectiveMeasurementType)
                    writer.WriteEndElement()
                    'C0753==

                    writer.WriteStartElement(DEFAULT_NON_COV_OBJ_MEASUREMENT_TYPE)
                    writer.WriteString(DefaultNonCoveringObjectiveMeasurementType)
                    writer.WriteEndElement()

                    'C0015===
                    writer.WriteStartElement(START_DATE)
                    writer.WriteString(DateTime2BinaryString(StartDate)) ' D0137
                    writer.WriteEndElement()

                    writer.WriteStartElement(END_DATE)
                    writer.WriteString(DateTime2BinaryString(EndDate)) ' D0137
                    writer.WriteEndElement()
                    'C0015==

                    'C0104===
                    writer.WriteStartElement(SYNHRONOUS_START_IN_POLLING_MODE)
                    writer.WriteString(If(SynchStartInPollingMode, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SYNHRONOUS_START_VOTING_ONCE_FACILITATOR_ALLOWS)
                    writer.WriteString(If(SynchStartVotingOnceFacilitatorAllows, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SYNHRONOUS_SHOW_GROUP_RESULTS)
                    writer.WriteString(If(SynchShowGroupResults, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SYNHRONOUS_USE_OPTIONS_FROM_DECISION)
                    writer.WriteString(If(SynchUseOptionsFromDecision, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(SYNHRONOUS_USE_VOTING_BOXES)
                    writer.WriteString(If(SynchUseVotingBoxes, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(TERMINAL_REDIRECT_URL)
                    writer.WriteString(TerminalRedirectURL)
                    writer.WriteEndElement()

                    writer.WriteStartElement(REDIRECT_AT_THE_END)
                    writer.WriteString(If(RedirectAtTheEnd, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(LOG_OFF_AT_THE_END)
                    writer.WriteString(If(LogOffAtTheEnd, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(RANDOM_SEQUENCE)
                    writer.WriteString(If(RandomSequence, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_ALLOW_MEETING_ID)
                    writer.WriteString(If(TeamTimeAllowMeetingID, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_ALLOW_ONLY_EMAIL_ADDRESSES_ADDED_TO_SESSION_TO_LOGIN)
                    writer.WriteString(If(TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_DISPLAY_USERS_WITH_VIEW_ONLY_ACCESS)
                    writer.WriteString(If(TeamTimeDisplayUsersWithViewOnlyAccess, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_START_IN_PRACTICE_MODE)
                    writer.WriteString(If(TeamTimeStartInPracticeMode, 1, 0))
                    writer.WriteEndElement()

                    'C0744===
                    writer.WriteStartElement(TEAM_TIME_START_IN_ANONYMOUS_MODE)
                    writer.WriteString(If(TeamTimeStartInAnonymousMode, 1, 0))
                    writer.WriteEndElement()
                    'C0744==

                    writer.WriteStartElement(TEAM_TIME_SHOW_KEYPAD_NUMBERS)
                    writer.WriteString(If(TeamTimeShowKeypadNumbers, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_SHOW_KEYPAD_LEGENDS)
                    writer.WriteString(If(TeamTimeShowKeypadLegends, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_HIDE_PROJECT_OWNER)
                    writer.WriteString(If(TeamTimeHideProjectOwner, 1, 0))
                    writer.WriteEndElement()

                    'C0493===
                    writer.WriteStartElement(TEAM_TIME_INVITATION_SUBJECT)
                    writer.WriteString(TeamTimeInvitationSubject)   'D0509
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_INVITATION_BODY)
                    writer.WriteString(TeamTimeInvitationBody)      ' D0509
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_EVALUATE_INVITATION_SUBJECT)
                    writer.WriteString(EvaluateInvitationSubject)   ' D0509
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_EVALUATE_INVITATION_BODY)
                    writer.WriteString(EvaluateInvitationBody)      ' D0509
                    writer.WriteEndElement()
                    'C0493==

                    writer.WriteStartElement(TEAM_TIME_INVITATION_SUBJECT2)
                    writer.WriteString(TeamTimeInvitationSubject2)   'D0509
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_INVITATION_BODY2)
                    writer.WriteString(TeamTimeInvitationBody2)      ' D0509
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_EVALUATE_INVITATION_SUBJECT2)
                    writer.WriteString(EvaluateInvitationSubject2)   ' D0509
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_EVALUATE_INVITATION_BODY2)
                    writer.WriteString(EvaluateInvitationBody2)      ' D0509
                    writer.WriteEndElement()

                    writer.WriteStartElement(TEAM_TIME_USERS_SORTING)
                    writer.WriteString(TeamTimeUsersSorting)
                    writer.WriteEndElement()

                    writer.WriteStartElement(NAME_ALTERNATIVES)
                    writer.WriteString(NameAlternatives)
                    writer.WriteEndElement()

                    writer.WriteStartElement(NAME_OBJECTIVES)
                    writer.WriteString(NameObjectives)
                    writer.WriteEndElement()

                    writer.WriteStartElement(INFODOC_SIZE)
                    writer.WriteString(InfoDocSize)
                    writer.WriteEndElement()

                    'C0576===
                    writer.WriteStartElement(JUDGEMENT_PROMT_ID)
                    writer.WriteString(JudgementPromtID.ToString)
                    writer.WriteEndElement()
                    'C0576==

                    writer.WriteStartElement(JUDGEMENT_PROMT_MULTI_ID)
                    writer.WriteString(JudgementPromtMultiID.ToString)
                    writer.WriteEndElement()

                    writer.WriteStartElement(DEFAULT_GROUP_ID)
                    writer.WriteString(DefaultGroupID.ToString)
                    writer.WriteEndElement()

                    writer.WriteStartElement(ASSOCIATED_MODEL_INT_ID)
                    writer.WriteString(AssociatedModelIntID.ToString)
                    writer.WriteEndElement()

                    writer.WriteStartElement(ASSOCIATED_MODEL_GUID_ID)
                    writer.WriteString(AssociatedModelGuidID.ToString)
                    writer.WriteEndElement()

                    'C0619===
                    writer.WriteStartElement(JUDGEMENT_ALTS_PROMT_ID)
                    writer.WriteString(JudgementAltsPromtID.ToString)
                    writer.WriteEndElement()
                    'C0619==

                    writer.WriteStartElement(JUDGEMENT_ALTS_PROMT_MULTI_ID)
                    writer.WriteString(JudgementAltsPromtMultiID.ToString)
                    writer.WriteEndElement()

                    writer.WriteStartElement(JUDGEMENT_PROMT)   ' D2183
                    writer.WriteString(JudgementPromt)
                    writer.WriteEndElement()

                    writer.WriteStartElement(JUDGEMENT_PROMT_MULTI)
                    writer.WriteString(JudgementPromtMulti)
                    writer.WriteEndElement()

                    writer.WriteStartElement(JUDGEMENT_ALTS_PROMT)  ' D2183
                    writer.WriteString(JudgementAltsPromt)
                    writer.WriteEndElement()

                    writer.WriteStartElement(JUDGEMENT_ALTS_PROMT_MULTI)
                    writer.WriteString(JudgementAltsPromtMulti)
                    writer.WriteEndElement()

                    'C0578===
                    writer.WriteStartElement(SHOW_FULL_OBJECTIVE_PATH)
                    'writer.WriteString(If(ShowFullObjectivePath, 1, 0))
                    writer.WriteString(CByte(ShowFullObjectivePath).ToString)   ' D4100
                    writer.WriteEndElement()
                    'C0578==

                    'C0977===
                    writer.WriteStartElement(OBJECTIVES_PAIRWISE_ONE_AT_A_TIME)
                    writer.WriteString(If(ObjectivesPairwiseOneAtATime, 1, 0))
                    writer.WriteEndElement()

                    writer.WriteStartElement(OBJECTIVES_NON_PAIRWISE_ONE_AT_A_TIME)
                    writer.WriteString(If(ObjectivesNonPairwiseOneAtATime, 1, 0))
                    writer.WriteEndElement()
                    'C0977==

                    'C0987===
                    writer.WriteStartElement(ALTERNATIVES_PAIRWISE_ONE_AT_A_TIME)
                    writer.WriteString(If(AlternativesPairwiseOneAtATime, 1, 0))
                    writer.WriteEndElement()
                    'C0987==

                    ' -D2183
                    ''C0805===
                    'writer.WriteStartElement(PROJECT_GUID)
                    'writer.WriteString(ProjectGuid.ToString)
                    'writer.WriteEndElement()
                    ''C0805==

                    writer.WriteEndElement()
                Finally
                    If Not (writer Is Nothing) Then
                        writer.Close()
                    End If
                End Try

                Return True
            End Function

            'Private Function GetUpdatePropertySQLString(ByVal PropertyName As String, ByVal PropertyValue As String) As String
            '    Return "UPDATE " + TableName + " SET " + PropertyValueColumnName + "='" & PropertyValue + "'" + _
            '        " WHERE " + PropertyNameColumnName + " LIKE '" + PropertyName + "'"   ' D0049 + D0126
            'End Function

            'Private Function GetInsertPropertySQLString(ByVal PropertyName As String, ByVal PropertyValue As String) As String
            '    Return "INSERT INTO " + TableName + " (" + PropertyNameColumnName + ", " + PropertyValueColumnName + ") VALUES " + _
            '        "('" + PropertyName + "', '" + PropertyValue + "')"     ' D0049
            'End Function

            'Private Sub WriteParameterToDatabase(ByVal oCommand As odbc.odbcCommand, _
            '    ByVal PropertyName As String, ByVal PropertyValue As String) 'C0236

            Private Sub WriteParameterToDatabase(ByVal oCommand As DbCommand,
                ByVal ProviderType As DBProviderType, ByVal PropertyName As String, ByVal PropertyValue As String) 'C0236

                If oCommand Is Nothing Then
                    Exit Sub
                End If

                Dim affected As Integer

                Try
                    'oCommand.CommandText = GetUpdatePropertySQLString(PropertyName, PropertyValue)
                    oCommand.CommandText = "UPDATE " + TableName + " SET " + PropertyValueColumnName + "=? " +
                        " WHERE " + PropertyNameColumnName + " LIKE ?"
                    oCommand.Parameters.Clear()
                    ' D0137: next two lines swapped

                    'C0236===
                    'oCommand.Parameters.AddWithValue("PropertyValue", PropertyValue)
                    'oCommand.Parameters.AddWithValue("PropertyName", PropertyName)
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyValue", PropertyValue)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyName", PropertyName)) 'C0237
                    'C0236==

                    affected = DBExecuteNonQuery(ProviderType, oCommand)

                Catch ex As Exception
                    Exit Sub
                End Try

                Try
                    If affected = 0 Then
                        'oCommand.CommandText = GetInsertPropertySQLString(PropertyName, PropertyValue)
                        oCommand.CommandText = "INSERT INTO " + TableName + " (" + PropertyNameColumnName + ", " + PropertyValueColumnName + ") VALUES (?, ?)"
                        oCommand.Parameters.Clear()
                        'C0236===
                        'oCommand.Parameters.AddWithValue("PropertyName", PropertyName)
                        'oCommand.Parameters.AddWithValue("PropertyValue", PropertyValue)
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyName", PropertyName)) 'C0237
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyValue", PropertyValue)) 'C0237
                        'C0236==

                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    End If
                Catch ex As Exception
                    Exit Sub
                End Try
            End Sub

            'Private Function WriteToDatabase(ByVal dbConnectionString As String) As Boolean 'C0236
            Private Function WriteToDatabase(ByVal dbConnectionString As String, ByVal ProviderType As DBProviderType) As Boolean 'C0236
                ' check if the connection is alive
                If Not CheckDBConnection(ProviderType, dbConnectionString) Then
                    Return False
                End If

                ' if connection is ok then open it
                'C0236===
                'Dim dbConnection As New odbc.odbcConnection(dbConnectionString) 
                'Dim oCommand As New odbc.odbcCommand 
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, dbConnectionString)  ' D2227
                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection
                    'C0236==

                    dbConnection.Open()

                    'C0182===
                    'C0177===
                    '' check if table exists
                    Dim schemaTable As System.Data.DataTable
                    Dim tableExists As Boolean = False

                    'schemaTable = dbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, _
                    '              New Object() {Nothing, Nothing, Nothing, "TABLE"}) 
                    schemaTable = dbConnection.GetSchema("Tables")
                    For i As Integer = 0 To schemaTable.Rows.Count - 1
                        If schemaTable.Rows(i)!TABLE_NAME.ToString() = TableName Then
                            tableExists = True
                        End If
                    Next
                    schemaTable = Nothing

                    If Not tableExists Then
                        dbConnection.Close() 'C0052
                        Return False
                    End If
                    'C0177==
                    'C0182==

                    oCommand.Connection = dbConnection

                    ' write parameters to the database

                    'C0236=== - added ProviderType parameter
                    WriteParameterToDatabase(oCommand, ProviderType, PROJECT_NAME, ProjectName)
                    WriteParameterToDatabase(oCommand, ProviderType, PROJECT_TYPE, ProjectType)
                    WriteParameterToDatabase(oCommand, ProviderType, PROJECT_PURPOSE, ProjectPurpose)
                    WriteParameterToDatabase(oCommand, ProviderType, FEEDBACK_ON, If(FeedbackOn, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, EVALUATE_OBJECTIVES, If(EvaluateObjectives, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, EVALUATE_ALTERNATIVES, If(EvaluateAlternatives, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, MODEL_EVALUATION_ORDER, CStr(ModelEvalOrder))
                    WriteParameterToDatabase(oCommand, ProviderType, GRAPHICAL_PW_MODE, CStr(GraphicalPWMode))
                    WriteParameterToDatabase(oCommand, ProviderType, GRAPHICAL_PW_MODE_FOR_ALTS, CStr(GraphicalPWModeForAlternatives))
                    WriteParameterToDatabase(oCommand, ProviderType, OBJECTIVES_EVALUATION_DIRECTION, CStr(ObjectivesEvalDirection))
                    WriteParameterToDatabase(oCommand, ProviderType, ALTERNATIVES_EVALUATION_MODE, CStr(AlternativesEvalMode))
                    WriteParameterToDatabase(oCommand, ProviderType, PAIRWISE_TYPE, CStr(PairwiseType))
                    WriteParameterToDatabase(oCommand, ProviderType, PAIRWISE_TYPE_FOR_ALTERNATIVES, CStr(PairwiseType)) 'C0985
                    WriteParameterToDatabase(oCommand, ProviderType, SYNTHESIS_MODE, CStr(SynthesisMode))
                    WriteParameterToDatabase(oCommand, ProviderType, COMBINED_MODE, CStr(CombinedMode)) 'C0945
                    WriteParameterToDatabase(oCommand, ProviderType, IDEAL_VIEW_TYPE, CStr(IdealViewType))
                    WriteParameterToDatabase(oCommand, ProviderType, EVALUATE_DIAGONALS, CStr(EvaluateDiagonals))
                    WriteParameterToDatabase(oCommand, ProviderType, EVALUATE_DIAGONALS_ALTERNATIVES, CStr(EvaluateDiagonalsAlternatives)) 'C0987
                    WriteParameterToDatabase(oCommand, ProviderType, LOCAL_RESULTS_VIEW, CStr(LocalResultsView))
                    WriteParameterToDatabase(oCommand, ProviderType, GLOBAL_RESULTS_VIEW, CStr(GlobalResultsView))
                    WriteParameterToDatabase(oCommand, ProviderType, WRT_INFODOCS_SHOW_MODE, CStr(WRTInfoDocsShowMode)) 'C1045
                    WriteParameterToDatabase(oCommand, ProviderType, LOCAL_RESULTS_SORT_MODE, CStr(LocalResultsSortMode)) 'C0820
                    WriteParameterToDatabase(oCommand, ProviderType, GLOBAL_RESULTS_SORT_MODE, CStr(GlobalResultsSortMode)) 'C0820
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_CONSISTENCY_RATIO, If(ShowConsistencyRatio, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_COMMENTS, If(ShowComments, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, USE_CIS, If(UseCISForIndividuals, 1, 0).ToString) 'C0824
                    WriteParameterToDatabase(oCommand, ProviderType, USE_WEIGHTS, If(UseWeights, 1, 0).ToString) 'C0824
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_INFODOCS, If(ShowInfoDocs, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_INFODOCS_MODE, CStr(ShowInfoDocsMode)) 'C0099
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_WELCOME_SCREEN, If(ShowWelcomeScreen, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_THANK_YOU_SCREEN, If(ShowThankYouScreen, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_WELCOME_SURVEY, If(ShowWelcomeSurvey, 1, 0).ToString) 'C0139
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_THANK_YOU_SURVEY, If(ShowThankYouSurvey, 1, 0).ToString) 'C0139
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_SURVEY, If(ShowSurvey, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_SENSITIVITY_ANALYSIS, If(ShowSensitivityAnalysis, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_SENSITIVITY_ANALYSIS_PERFORMANCE, If(ShowSensitivityAnalysisPerformance, 1, 0).ToString) 'C0078
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_SENSITIVITY_ANALYSIS_GRADIENT, If(ShowSensitivityAnalysisGradient, 1, 0).ToString) 'C0078
                    WriteParameterToDatabase(oCommand, ProviderType, SORT_SENSITIVITY_ANALYSIS, If(SortSensitivityAnalysis, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, CALC_SA_FOR_COMBINED, If(CalculateSAForCombined, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, CALC_SA_WRT_GOAL, If(CalculateSAWRTGoal, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, ALLOW_SWITCH_SA_NODE, If(AllowSwitchNodesInSA, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, OBJS_COLUMN_NAME_IN_SA, ObjectivesColumnNameInSA)
                    WriteParameterToDatabase(oCommand, ProviderType, ALTS_COLUMN_NAME_IN_SA, AlternativesColumnNameInSA)
                    WriteParameterToDatabase(oCommand, ProviderType, ALLOW_AUTOADVANCE, If(AllowAutoadvance, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_NEXT_UNASSESSED, If(ShowNextUnassessed, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_PROGRESS_INDICATOR, If(ShowProgressIndicator, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, ALLOW_NAVIGATION, If(AllowNavigation, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_EXPECTED_VALUE_LOCAL, If(ShowExpectedValueLocal, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_EXPECTED_VALUE_GLOBAL, If(ShowExpectedValueGlobal, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, ALLOW_MISSING_JUDGMENTS, If(AllowMissingJudgments, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, DISABLE_WARNINGS_ON_STEP_CHANGE, If(DisableWarningsOnStepChange, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, FORCE_GRAPHICAL_PAIRWISE, If(ForceGraphical, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, FORCE_GRAPHICAL_PAIRWISE_FOR_ALTERNATIVES, If(ForceGraphicalForAlternatives, 1, 0).ToString) 'C0985
                    WriteParameterToDatabase(oCommand, ProviderType, FORCE_ALL_DIAGONALS, If(ForceAllDiagonals, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, FORCE_ALL_DIAGONALS_LIMIT, CStr(ForceAllDiagonalsLimit))
                    WriteParameterToDatabase(oCommand, ProviderType, FORCE_ALL_DIAGONALS_FOR_ALTERNATIVES, If(ForceAllDiagonalsForAlternatives, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, FORCE_ALL_DIAGONALS_LIMIT_FOR_ALTERNATIVES, CStr(ForceAllDiagonalsLimitForAlternatives))
                    WriteParameterToDatabase(oCommand, ProviderType, INCLUDE_IDEAL_ALTERNATIVE, If(IncludeIdealAlternative, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_IDEAL_ALTERNATIVE, If(ShowIdealAlternative, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, MEASURE_MODE, CStr(MeasureMode))
                    WriteParameterToDatabase(oCommand, ProviderType, ALTS_DEFAULT_CONTRIBUTION, CStr(AltsDefaultContribution))
                    WriteParameterToDatabase(oCommand, ProviderType, ALTS_DEFAULT_CONTRIBUTION_IMPACT, CStr(AltsDefaultContributionImpact))
                    WriteParameterToDatabase(oCommand, ProviderType, DEFAULT_COV_OBJ_MEASUREMENT_TYPE, CStr(DefaultCoveringObjectiveMeasurementType)) 'C0753
                    WriteParameterToDatabase(oCommand, ProviderType, DEFAULT_NON_COV_OBJ_MEASUREMENT_TYPE, CStr(DefaultNonCoveringObjectiveMeasurementType))
                    WriteParameterToDatabase(oCommand, ProviderType, START_DATE, DateTime2BinaryString(StartDate)) 'C0015 + D0137
                    WriteParameterToDatabase(oCommand, ProviderType, END_DATE, DateTime2BinaryString(EndDate)) 'C0015 + D0137
                    WriteParameterToDatabase(oCommand, ProviderType, SYNHRONOUS_START_IN_POLLING_MODE, If(SynchStartInPollingMode, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SYNHRONOUS_START_VOTING_ONCE_FACILITATOR_ALLOWS, If(SynchStartVotingOnceFacilitatorAllows, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SYNHRONOUS_SHOW_GROUP_RESULTS, If(SynchShowGroupResults, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SYNHRONOUS_USE_OPTIONS_FROM_DECISION, If(SynchUseOptionsFromDecision, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, SYNHRONOUS_USE_VOTING_BOXES, If(SynchUseVotingBoxes, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_ALLOW_MEETING_ID, If(TeamTimeAllowMeetingID, 1, 0).ToString) 'C0395
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_ALLOW_ONLY_EMAIL_ADDRESSES_ADDED_TO_SESSION_TO_LOGIN, If(TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin, 1, 0).ToString) 'C0395
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_DISPLAY_USERS_WITH_VIEW_ONLY_ACCESS, If(TeamTimeDisplayUsersWithViewOnlyAccess, 1, 0).ToString) 'C0395
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_START_IN_PRACTICE_MODE, If(TeamTimeStartInPracticeMode, 1, 0).ToString) 'C0395
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_START_IN_ANONYMOUS_MODE, If(TeamTimeStartInAnonymousMode, 1, 0).ToString) 'C0744
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_SHOW_KEYPAD_NUMBERS, If(TeamTimeShowKeypadNumbers, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_SHOW_KEYPAD_LEGENDS, If(TeamTimeShowKeypadLegends, 1, 0).ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_HIDE_PROJECT_OWNER, If(TeamTimeHideProjectOwner, 1, 0).ToString) 'C0432
                    WriteParameterToDatabase(oCommand, ProviderType, TERMINAL_REDIRECT_URL, TerminalRedirectURL) 'C0110
                    WriteParameterToDatabase(oCommand, ProviderType, REDIRECT_AT_THE_END, If(RedirectAtTheEnd, 1, 0).ToString) 'C0115
                    WriteParameterToDatabase(oCommand, ProviderType, LOG_OFF_AT_THE_END, If(LogOffAtTheEnd, 1, 0).ToString) 'C0115
                    WriteParameterToDatabase(oCommand, ProviderType, RANDOM_SEQUENCE, If(RandomSequence, 1, 0).ToString) 'C0205
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_INVITATION_SUBJECT, TeamTimeInvitationSubject) 'C0493 + D0509
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_INVITATION_BODY, TeamTimeInvitationBody) 'C0493 + D0509
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_EVALUATE_INVITATION_SUBJECT, EvaluateInvitationSubject) 'C0493 + D0509
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_EVALUATE_INVITATION_BODY, EvaluateInvitationBody) 'C0493 + D0509
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_INVITATION_SUBJECT2, TeamTimeInvitationSubject2) 'C0493 + D0509
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_INVITATION_BODY2, TeamTimeInvitationBody2) 'C0493 + D0509
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_EVALUATE_INVITATION_SUBJECT2, EvaluateInvitationSubject2) 'C0493 + D0509
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_EVALUATE_INVITATION_BODY2, EvaluateInvitationBody2) 'C0493 + D0509
                    WriteParameterToDatabase(oCommand, ProviderType, TEAM_TIME_USERS_SORTING, CStr(TeamTimeUsersSorting))
                    WriteParameterToDatabase(oCommand, ProviderType, NAME_ALTERNATIVES, NameAlternatives)
                    WriteParameterToDatabase(oCommand, ProviderType, NAME_OBJECTIVES, NameObjectives)
                    WriteParameterToDatabase(oCommand, ProviderType, INFODOC_SIZE, InfoDocSize)
                    WriteParameterToDatabase(oCommand, ProviderType, JUDGEMENT_PROMT_ID, JudgementPromtID.ToString) 'C0576
                    WriteParameterToDatabase(oCommand, ProviderType, JUDGEMENT_ALTS_PROMT_ID, JudgementAltsPromtID.ToString) 'C0818
                    WriteParameterToDatabase(oCommand, ProviderType, JUDGEMENT_PROMT_MULTI_ID, JudgementPromtMultiID.ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, JUDGEMENT_ALTS_PROMT_MULTI_ID, JudgementAltsPromtMultiID.ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, DEFAULT_GROUP_ID, DefaultGroupID.ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, ASSOCIATED_MODEL_INT_ID, AssociatedModelIntID.ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, ASSOCIATED_MODEL_GUID_ID, AssociatedModelGuidID.ToString)
                    WriteParameterToDatabase(oCommand, ProviderType, JUDGEMENT_PROMT_PARAMETER, JudgementPromt)
                    WriteParameterToDatabase(oCommand, ProviderType, JUDGEMENT_ALTS_PROMT_PARAMETER, JudgementAltsPromt)
                    WriteParameterToDatabase(oCommand, ProviderType, JUDGEMENT_PROMT_MULTI_PARAMETER, JudgementPromtMulti)
                    WriteParameterToDatabase(oCommand, ProviderType, JUDGEMENT_ALTS_PROMT_MULTI_PARAMETER, JudgementAltsPromtMulti)
                    'WriteParameterToDatabase(oCommand, ProviderType, SHOW_FULL_OBJECTIVE_PATH, If(ShowFullObjectivePath, 1, 0).ToString) 'C0578
                    WriteParameterToDatabase(oCommand, ProviderType, SHOW_FULL_OBJECTIVE_PATH, CByte(ShowFullObjectivePath).ToString) 'C0578 + D4100
                    WriteParameterToDatabase(oCommand, ProviderType, OBJECTIVES_PAIRWISE_ONE_AT_A_TIME, If(ObjectivesPairwiseOneAtATime, 1, 0).ToString) 'C0977
                    WriteParameterToDatabase(oCommand, ProviderType, OBJECTIVES_NON_PAIRWISE_ONE_AT_A_TIME, If(ObjectivesNonPairwiseOneAtATime, 1, 0).ToString) 'C0977
                    WriteParameterToDatabase(oCommand, ProviderType, ALTERNATIVES_PAIRWISE_ONE_AT_A_TIME, If(AlternativesPairwiseOneAtATime, 1, 0).ToString) 'C0987
                    WriteParameterToDatabase(oCommand, ProviderType, PROJECT_GUID, ProjectGuid.ToString) 'C0805

                    'C0573===
                    'C0574===
                    'WriteParameterToDatabase(oCommand, ProviderType, WELCOME_SCREEN_TEXT_FOR_AHP, PipeMessages.GetWelcomeText(PipeMessageKind.pmkText, 0, 1))
                    'WriteParameterToDatabase(oCommand, ProviderType, THANK_YOU_SCREEN_TEXT_FOR_AHP, PipeMessages.GetThankYouText(PipeMessageKind.pmkText, 0, 1))
                    'C0574==
                    'C0573==

                    oCommand = Nothing
                    dbConnection.Close()
                End Using

                Return True
            End Function

            Private Function GetWriter() As clsPipeParametersStreamWriter
                Dim TWriter As Type = Nothing
                Dim i As Integer = ProjectVersion.MinorVersion
                While (i >= 8) And TWriter Is Nothing ' 8 is the first version we introduced current streams version control
                    TWriter = Assembly.GetExecutingAssembly.GetType("Canvas.PipeParameters+clsPipeParametersStreamWriter_v_1_1_" + i.ToString)
                    If TWriter Is Nothing Then i -= 1
                End While

                Dim writer As clsPipeParametersStreamWriter = Activator.CreateInstance(TWriter)
                Return writer
            End Function

            Private Function GetReader() As clsPipeParametersStreamReader 'C0743
                Dim TReader As Type = Nothing
                Dim i As Integer = ProjectVersion.MinorVersion
                While (i >= 8) And TReader Is Nothing ' 8 is the first version we introduced current streams version control for pipe parameters
                    TReader = Assembly.GetExecutingAssembly.GetType("Canvas.PipeParameters+clsPipeParametersStreamReader_v_1_1_" + i.ToString)
                    If TReader Is Nothing Then i -= 1
                End While

                Dim reader As clsPipeParametersStreamReader = Activator.CreateInstance(TReader)
                Return reader
            End Function

            Private Function WriteToStreamsDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean 'C0267
                Dim writer As clsPipeParametersStreamWriter = GetWriter() 'C0743
                writer.PipeParameters = Me

                Dim MS As MemoryStream = writer.Write

                Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stPipeOptions)))

                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                    oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType.stPipeOptions))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)

                End Using

                Return True
            End Function

            Public Overloads Function WriteToStream() As MemoryStream 'C0743
                Dim writer As clsPipeParametersStreamWriter = GetWriter()
                writer.PipeParameters = Me
                Dim MS As MemoryStream = writer.Write
                Return MS
            End Function

            Public Overloads Function WriteToStream(ByRef ParamsStream As MemoryStream, ByRef MessagesStream As MemoryStream) As Boolean 'C0746
                Dim writer As clsPipeParametersStreamWriter = GetWriter()
                writer.PipeParameters = Me
                ParamsStream = writer.Write

                MessagesStream = PipeMessages.WriteStream

                Return True
            End Function

            Public Overloads Function ReadFromStream(ByVal MS As MemoryStream) As Boolean 'C0743
                'Create pipe parameters reader
                Dim reader As clsPipeParametersStreamReader = GetReader()
                reader.PipeParameters = Me
                Return reader.Read(MS)
            End Function

            Public Overloads Function ReadFromStream(ByVal ParamsStream As MemoryStream, ByVal MessagesStream As MemoryStream) As Boolean 'C0746
                'Create pipe parameters reader
                Dim reader As clsPipeParametersStreamReader = GetReader()
                reader.PipeParameters = Me
                reader.Read(ParamsStream)

                PipeMessages.ReadStream(MessagesStream)

                Return True
            End Function

            'Public Function Write(ByVal StorageType As PipeStorageType, ByVal Location As String) As Boolean 'C0236
            'Public Function Write(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType) As Boolean 'C0236 'C0267
            'Public Function Write(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, Optional ByVal ModelID As Integer = -1) As Boolean 'C0267 'C0390
            Public Function Write(ByVal StorageType As PipeStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean 'C0390
                Select Case StorageType
                    Case PipeStorageType.pstXMLFile
                        mCurrentParameterSet = DefaultParameterSet
                        Return WriteToXML(Location)
                    Case PipeStorageType.pstDatabase
                        'mPipeMessages.SaveToDatabase(Location) 'C0236
                        mPipeMessages.Save(PipeStorageType.pstDatabase, Location, ProviderType, ModelID) 'C0236 'C0420
                        'Return WriteToDatabase(Location) 'C0236
                        mCurrentParameterSet = DefaultParameterSet
                        Return WriteToDatabase(Location, ProviderType) 'C0236 'C0355
                    Case PipeStorageType.pstStreamsDatabase 'C0267
                        mPipeMessages.Save(PipeStorageType.pstStreamsDatabase, Location, ProviderType, ModelID) 'C0355
                        Return WriteToStreamsDatabase(Location, ProviderType, ModelID)
                End Select
                Return False
            End Function

            '' D6321 ===
            'Public Sub Copy(SrcSet As ParameterSet, ByRef DestSet As ParameterSet) '' not tested
            '    If SrcSet IsNot Nothing AndAlso DestSet IsNot Nothing Then
            '        Using buffer As New MemoryStream()
            '            Dim ID As Integer = DestSet.ID
            '            Dim Name As String = DestSet.Name
            '            Dim formatter As New BinaryFormatter()
            '            formatter.Serialize(buffer, SrcSet)
            '            buffer.Position = 0
            '            DestSet = CType(formatter.Deserialize(buffer), ParameterSet)
            '            DestSet.ID = ID
            '            DestSet.Name = Name
            '        End Using
            '    End If
            'End Sub
            '' D6321 ==

            Public Property TableName() As String
                Get
                    Return mTableName
                End Get
                Set(ByVal value As String)
                    mTableName = value
                End Set
            End Property

            Public Property PropertyNameColumnName() As String
                Get
                    Return mPropertyNameColumnName
                End Get
                Set(ByVal value As String)
                    mPropertyNameColumnName = value
                End Set
            End Property

            Public Property PropertyValueColumnName() As String
                Get
                    Return mPropertyValueColumnName
                End Get
                Set(ByVal value As String)
                    mPropertyValueColumnName = value
                End Set
            End Property

            Public Property PipeMessagesTableName() As String
                Get
                    Return mPipeMessagesTableName
                End Get
                Set(ByVal value As String)
                    mPipeMessagesTableName = value
                End Set
            End Property

            Public Property XMLSettingsNodeName() As String
                Get
                    Return mXMLSettingsNodeName
                End Get
                Set(ByVal value As String)
                    mXMLSettingsNodeName = value
                End Set
            End Property

            'Public Function GetPipeMessageFromDatabase(ByVal dbConnectionString As String, _
            '    ByVal MessageType As PipeMessageType, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, _
            '    ByVal ObjectID As Integer, ByVal Position As PipeMessagePosition) As String 'C0236

            Public Function GetPipeMessageFromDatabase(ByVal dbConnectionString As String,
                ByVal ProviderType As DBProviderType,
                ByVal MessageType As PipeMessageType, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer,
                ByVal ObjectID As Integer, ByVal Position As PipeMessagePosition) As String 'C0236

                ' check if the connection is alive
                If Not CheckDBConnection(ProviderType, dbConnectionString) Then
                    Return ""
                End If

                'C0236===
                'Dim dbConnection As New odbc.odbcConnection(dbConnectionString)
                'Dim dbReader As odbc.odbcDataReader
                'Dim oCommand As odbc.odbcCommand
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, dbConnectionString)  ' D2227
                    Dim dbReader As DbDataReader
                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection
                    'C0236==

                    dbConnection.Open()

                    ' check if table exists
                    Dim schemaTable As System.Data.DataTable
                    Dim tableExists As Boolean = False

                    schemaTable = dbConnection.GetSchema("Tables")
                    For i As Integer = 0 To schemaTable.Rows.Count - 1
                        If schemaTable.Rows(i)!TABLE_NAME.ToString() = TableName Then
                            tableExists = True
                        End If
                    Next
                    schemaTable = Nothing

                    If Not tableExists Then
                        Return ""
                    End If

                    ' if everything is ok then read all properties from the table
                    'oCommand = New odbc.odbcCommand("SELECT * FROM " + TableName, dbConnection) 'C0236
                    oCommand.CommandText = "SELECT * FROM " + TableName 'C0236
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    ' read parameters from the table
                    If Not dbReader Is Nothing Then
                        While dbReader.Read()
                            If (Not TypeOf (dbReader.GetValue(0)) Is DBNull) And
                                (Not TypeOf (dbReader.GetValue(1)) Is DBNull) Then

                                Select Case CStr(dbReader(PropertyNameColumnName))
                                End Select
                            End If
                        End While
                    End If

                    oCommand = Nothing
                    dbConnection.Close()
                End Using

                Return True
            End Function

            'Public Function GetPipeMessage(ByVal StorageType As PipeStorageType, ByVal Location As String, _
            '    ByVal MessageType As PipeMessageType, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, _
            '    ByVal ObjectID As Integer, ByVal Position As PipeMessagePosition) As String 'C0236

            Public Function GetPipeMessage(ByVal StorageType As PipeStorageType, ByVal Location As String,
                ByVal ProviderType As DBProviderType,
                ByVal MessageType As PipeMessageType, ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer,
                ByVal ObjectID As Integer, ByVal Position As PipeMessagePosition) As String 'C0236

                Select Case StorageType
                    Case PipeStorageType.pstXMLFile
                        'Return ReadFromXML(Location)
                    Case PipeStorageType.pstDatabase
                        'Return GetPipeMessageFromDatabase(Location, MessageType, HierarchyID, AltsHierarchyID, ObjectID, Position) 'C0236
                        Return GetPipeMessageFromDatabase(Location, ProviderType, MessageType, HierarchyID, AltsHierarchyID, ObjectID, Position) 'C0236
                End Select
                Return ""
            End Function

            Public Function CopyParameterSet(ByVal SourceSet As ParameterSet, ByVal DestinationSet As ParameterSet) As Boolean 'C0468
                If SourceSet Is Nothing Or DestinationSet Is Nothing Then
                    Return False
                End If

                'If Not ParameterSets.Contains(SourceSet) Or Not ParameterSets.Contains(DestinationSet) Then    ' -D6321 due to problem with copy from/to when Impact
                '    Return False
                'End If

                mFeedbackOn(DestinationSet.ID) = mFeedbackOn(SourceSet.ID)
                mEvaluateObjectives(DestinationSet.ID) = mEvaluateObjectives(SourceSet.ID)
                mEvaluateAlternatives(DestinationSet.ID) = mEvaluateAlternatives(SourceSet.ID)
                mModelEvalOrder(DestinationSet.ID) = mModelEvalOrder(SourceSet.ID)
                mGraphicalPWMode(DestinationSet.ID) = mGraphicalPWMode(SourceSet.ID)
                mGraphicalPWModeForAlts(DestinationSet.ID) = mGraphicalPWModeForAlts(SourceSet.ID)
                mObjsEvalDirection(DestinationSet.ID) = mObjsEvalDirection(SourceSet.ID)
                mAltsEvalMode(DestinationSet.ID) = mAltsEvalMode(SourceSet.ID)
                mPairwiseType(DestinationSet.ID) = mPairwiseType(SourceSet.ID)
                mPairwiseTypeForAlternatives(DestinationSet.ID) = mPairwiseTypeForAlternatives(SourceSet.ID) 'C0985
                mSynthesisMode(DestinationSet.ID) = mSynthesisMode(SourceSet.ID)
                mCombinedMode(DestinationSet.ID) = mCombinedMode(SourceSet.ID) 'C0945
                mIdealViewType(DestinationSet.ID) = mIdealViewType(SourceSet.ID)
                mEvaluateDiagonals(DestinationSet.ID) = mEvaluateDiagonals(SourceSet.ID)
                mEvaluateDiagonalsAlternatives(DestinationSet.ID) = mEvaluateDiagonalsAlternatives(SourceSet.ID) 'C0987
                mPWEvalOrder(DestinationSet.ID) = mPWEvalOrder(SourceSet.ID)
                mLocalResultsView(DestinationSet.ID) = mLocalResultsView(SourceSet.ID)
                mGlobalResultsView(DestinationSet.ID) = mGlobalResultsView(SourceSet.ID)
                mWRTInfoDocsShowMode(DestinationSet.ID) = mWRTInfoDocsShowMode(SourceSet.ID) 'C1045
                mLocalResultsSortMode(DestinationSet.ID) = mLocalResultsSortMode(SourceSet.ID) 'C0820
                mGlobalResultsSortMode(DestinationSet.ID) = mGlobalResultsSortMode(SourceSet.ID) 'C0820
                mShowConsistencyRatio(DestinationSet.ID) = mShowConsistencyRatio(SourceSet.ID)
                mUseCIS(DestinationSet.ID) = mUseCIS(SourceSet.ID) 'C0824
                mUseWeights(DestinationSet.ID) = mUseWeights(SourceSet.ID) 'C0824
                mShowComments(DestinationSet.ID) = mShowComments(SourceSet.ID)
                mShowInfoDocs(DestinationSet.ID) = mShowInfoDocs(SourceSet.ID)
                mShowInfoDocsMode(DestinationSet.ID) = mShowInfoDocsMode(SourceSet.ID)
                mShowWelcomeScreen(DestinationSet.ID) = mShowWelcomeScreen(SourceSet.ID)
                mShowThankYouScreen(DestinationSet.ID) = mShowThankYouScreen(SourceSet.ID)
                mShowWelcomeSurvey(DestinationSet.ID) = mShowWelcomeSurvey(SourceSet.ID)
                mShowThankYouSurvey(DestinationSet.ID) = mShowThankYouSurvey(SourceSet.ID)
                mShowSurvey(DestinationSet.ID) = mShowSurvey(SourceSet.ID)
                mShowSA(DestinationSet.ID) = mShowSA(SourceSet.ID)
                mShowSAPerformance(DestinationSet.ID) = mShowSAPerformance(SourceSet.ID)
                mShowSAGradient(DestinationSet.ID) = mShowSAGradient(SourceSet.ID)
                mSortSA(DestinationSet.ID) = mSortSA(SourceSet.ID)
                mCalcSAForCombined(DestinationSet.ID) = mCalcSAForCombined(SourceSet.ID)
                mCalcSAWRTGoal(DestinationSet.ID) = mCalcSAWRTGoal(SourceSet.ID)
                mAllowSwitchNodesInSA(DestinationSet.ID) = mAllowSwitchNodesInSA(SourceSet.ID)
                mObjsColNameInSA(DestinationSet.ID) = mObjsColNameInSA(SourceSet.ID)
                mAltsColNameInSA(DestinationSet.ID) = mAltsColNameInSA(SourceSet.ID)
                mAllowAutoadvance(DestinationSet.ID) = mAllowAutoadvance(SourceSet.ID)
                mShowNextUnassessed(DestinationSet.ID) = mShowNextUnassessed(SourceSet.ID)
                mShowProgressIndicator(DestinationSet.ID) = mShowProgressIndicator(SourceSet.ID)
                mAllowNavigation(DestinationSet.ID) = mAllowNavigation(SourceSet.ID)
                mAllowMissingJudgments(DestinationSet.ID) = mAllowMissingJudgments(SourceSet.ID)
                mShowExpectedValueLocal(DestinationSet.ID) = mShowExpectedValueLocal(SourceSet.ID)
                mShowExpectedValueGlobal(DestinationSet.ID) = mShowExpectedValueGlobal(SourceSet.ID)
                mDisableWarningsOnStepChange(DestinationSet.ID) = mDisableWarningsOnStepChange(SourceSet.ID)
                mForceGraphical(DestinationSet.ID) = mForceGraphical(SourceSet.ID)
                mForceGraphicalForAlternatives(DestinationSet.ID) = mForceGraphicalForAlternatives(SourceSet.ID) 'C0985
                mForceAllDiagonals(DestinationSet.ID) = mForceAllDiagonals(SourceSet.ID)
                mForceAllDiagonalsLimit(DestinationSet.ID) = mForceAllDiagonalsLimit(SourceSet.ID)
                mForceAllDiagonalsForAlternatives(DestinationSet.ID) = mForceAllDiagonalsForAlternatives(SourceSet.ID)
                mForceAllDiagonalsLimitForAlternatives(DestinationSet.ID) = mForceAllDiagonalsLimitForAlternatives(SourceSet.ID)
                mIncludeIdealAlt(DestinationSet.ID) = mIncludeIdealAlt(SourceSet.ID)
                mShowIdealAlt(DestinationSet.ID) = mShowIdealAlt(SourceSet.ID)
                mMeasureMode(DestinationSet.ID) = mMeasureMode(SourceSet.ID)
                mAltsDefContribution(DestinationSet.ID) = mAltsDefContribution(SourceSet.ID)
                mAltsDefContributionImpact(DestinationSet.ID) = mAltsDefContributionImpact(SourceSet.ID)
                mDefaultCovObjMeasurementType(DestinationSet.ID) = mDefaultCovObjMeasurementType(SourceSet.ID)
                mDefaultNonCovObjMeasurementType(DestinationSet.ID) = mDefaultNonCovObjMeasurementType(SourceSet.ID)
                mStartDate(DestinationSet.ID) = mStartDate(SourceSet.ID)
                mEndDate(DestinationSet.ID) = mEndDate(SourceSet.ID)
                mSynchStartInPollingMode(DestinationSet.ID) = mSynchStartInPollingMode(SourceSet.ID)
                mSynchStartVotingOnceFacilitatorAllows(DestinationSet.ID) = mSynchStartVotingOnceFacilitatorAllows(SourceSet.ID)
                mSynchShowGroupResults(DestinationSet.ID) = mSynchShowGroupResults(SourceSet.ID)
                mSynchUseOptionsFromDecision(DestinationSet.ID) = mSynchUseOptionsFromDecision(SourceSet.ID)
                mSynchUseVotingBoxes(DestinationSet.ID) = mSynchUseVotingBoxes(SourceSet.ID)
                mTTAllowMeetingID(DestinationSet.ID) = mTTAllowMeetingID(SourceSet.ID)
                mTTAllowOnlyEmailAddressesAddedToSessionToLogin(DestinationSet.ID) = mTTAllowOnlyEmailAddressesAddedToSessionToLogin(SourceSet.ID)
                mTTDisplayUsersWithViewOnlyAccess(DestinationSet.ID) = mTTDisplayUsersWithViewOnlyAccess(SourceSet.ID)
                mTTStartInPracticeMode(DestinationSet.ID) = mTTStartInPracticeMode(SourceSet.ID)
                mTTShowKeypadNumbers(DestinationSet.ID) = mTTShowKeypadNumbers(SourceSet.ID)
                mTTShowKeypadLegends(DestinationSet.ID) = mTTShowKeypadLegends(SourceSet.ID)
                mTTHideProjectOwner(DestinationSet.ID) = mTTHideProjectOwner(SourceSet.ID)
                mTerminalRedirectURL(DestinationSet.ID) = mTerminalRedirectURL(SourceSet.ID)
                mRedirectAtTheEnd(DestinationSet.ID) = mRedirectAtTheEnd(SourceSet.ID)
                mLogOffAtTheEnd(DestinationSet.ID) = mLogOffAtTheEnd(SourceSet.ID)
                mRandomSequence(DestinationSet.ID) = mRandomSequence(SourceSet.ID)
                mDefaultGroupID(DestinationSet.ID) = mDefaultGroupID(SourceSet.ID) 'C0576
                mJudgementPromtID(DestinationSet.ID) = mJudgementPromtID(SourceSet.ID) 'C0576
                mJudgementAltsPromtID(DestinationSet.ID) = mJudgementAltsPromtID(SourceSet.ID) 'C0619
                mJudgementPromt(DestinationSet.ID) = mJudgementPromt(SourceSet.ID)
                mJudgementAltsPromt(DestinationSet.ID) = mJudgementAltsPromt(SourceSet.ID)
                mJudgementPromtMultiID(DestinationSet.ID) = mJudgementPromtMultiID(SourceSet.ID)
                mJudgementAltsPromtMultiID(DestinationSet.ID) = mJudgementAltsPromtMultiID(SourceSet.ID)
                mJudgementPromtMulti(DestinationSet.ID) = mJudgementPromtMulti(SourceSet.ID)
                mJudgementAltsPromtMulti(DestinationSet.ID) = mJudgementAltsPromtMulti(SourceSet.ID)
                mShowFullObjectivePath(DestinationSet.ID) = mShowFullObjectivePath(SourceSet.ID) 'C0578
                mObjectivesPairwiseOneAtATime(DestinationSet.ID) = mObjectivesPairwiseOneAtATime(SourceSet.ID) 'C0977
                mObjectivesNonPairwiseOneAtATime(DestinationSet.ID) = mObjectivesNonPairwiseOneAtATime(SourceSet.ID) 'C0977
                mAlternativesPairwiseOneAtATime(DestinationSet.ID) = mAlternativesPairwiseOneAtATime(SourceSet.ID) 'C0987

                Return True     ' D0444
            End Function
        End Class

        <Serializable()> Public MustInherit Class clsPipeParametersStreamReader 'C0461
            Private mPipeParameters As clsPipeParamaters

            Public Property PipeParameters() As clsPipeParamaters
                Get
                    Return mPipeParameters
                End Get
                Set(ByVal value As clsPipeParamaters)
                    mPipeParameters = value
                End Set
            End Property

            Protected MustOverride Sub ReadOneParametersSet(ByVal BR As BinaryReader)
            Public MustOverride Function Read(ByVal Stream As MemoryStream) As Boolean
        End Class

        <Serializable()> Public Class clsPipeParametersStreamReader_v_1_1_8 'C0461
            Inherits clsPipeParametersStreamReader

            Protected Overrides Sub ReadOneParametersSet(ByVal BR As BinaryReader)
                Dim ParamID As Integer

                Dim res As Boolean = True 'C0468

                With PipeParameters
                    'While (BR.BaseStream.Position < BR.BaseStream.Length - 1) 'C0468
                    While (BR.BaseStream.Position < BR.BaseStream.Length - 1) And res 'C0468
                        ParamID = BR.ReadInt32
                        Select Case ParamID
                            Case FEEDBACK_ON_PARAMETER
                                .FeedbackOn = BR.ReadBoolean
                            Case EVALUATE_OBJECTIVES_PARAMETER
                                .EvaluateObjectives = BR.ReadBoolean
                            Case EVALUATE_ALTERNATIVES_PARAMETER
                                .EvaluateAlternatives = BR.ReadBoolean
                            Case MODEL_EVALUATION_ORDER_PARAMETER
                                .ModelEvalOrder = BR.ReadInt32
                            Case GRAPHICAL_PW_MODE_PARAMETER
                                .GraphicalPWMode = BR.ReadInt32
                            Case GRAPHICAL_PW_MODE_FOR_ALTS_PARAMETER
                                .GraphicalPWModeForAlternatives = BR.ReadInt32
                            Case OBJECTIVES_EVALUATION_DIRECTION_PARAMETER
                                .ObjectivesEvalDirection = BR.ReadInt32
                            Case ALTERNATIVES_EVALUATION_MODE_PARAMETER
                                .AlternativesEvalMode = BR.ReadInt32
                            Case PAIRWISE_TYPE_PARAMETER
                                .PairwiseType = BR.ReadInt32
                            Case PAIRWISE_TYPE_FOR_ALTERNATIVES_PARAMETER 'C0985
                                .PairwiseTypeForAlternatives = BR.ReadInt32
                            Case SYNTHESIS_MODE_PARAMETER
                                .SynthesisMode = BR.ReadInt32
                            Case COMBINED_MODE_PARAMETER 'C0945
                                .CombinedMode = BR.ReadInt32
                            Case IDEAL_VIEW_TYPE_PARAMETER
                                .IdealViewType = BR.ReadInt32
                            Case EVALUATE_DIAGONALS_PARAMETER
                                .EvaluateDiagonals = BR.ReadInt32
                            Case EVALUATE_DIAGONALS_ADVANCED_PARAMETER 'C1010
                                .EvaluateDiagonalsAdvanced = BR.ReadInt32
                            Case EVALUATE_DIAGONALS_ALTERNATIVES_PARAMETER 'C0987
                                .EvaluateDiagonalsAlternatives = BR.ReadInt32
                            Case LOCAL_RESULTS_VIEW_PARAMETER
                                .LocalResultsView = BR.ReadInt32
                            Case GLOBAL_RESULTS_VIEW_PARAMETER
                                .GlobalResultsView = BR.ReadInt32
                            Case WRT_INDOFOCS_SHOW_MODE_PARAMETER 'C1045
                                .WRTInfoDocsShowMode = BR.ReadInt32
                            Case LOCAL_RESULTS_SORT_MODE_PARAMETER 'C0820
                                .LocalResultsSortMode = BR.ReadInt32
                            Case GLOBAL_RESULTS_SORT_MODE_PARAMETER 'C0820
                                .GlobalResultsSortMode = BR.ReadInt32
                            Case SHOW_CONSISTENCY_RATIO_PARAMETER
                                .ShowConsistencyRatio = BR.ReadBoolean
                            Case USE_CIS_PARAMETER 'C0824
                                .UseCISForIndividuals = BR.ReadBoolean
                            Case USE_WEIGHTS_PARAMETER 'C0824
                                .UseWeights = BR.ReadBoolean
                            Case SHOW_COMMENTS_PARAMETER
                                .ShowComments = BR.ReadBoolean
                            Case SHOW_INFODOCS_PARAMETER
                                .ShowInfoDocs = BR.ReadBoolean
                            Case SHOW_INFODOCS_MODE_PARAMETER
                                .ShowInfoDocsMode = BR.ReadInt32
                            Case SHOW_WELCOME_SCREEN_PARAMETER
                                .ShowWelcomeScreen = BR.ReadBoolean
                            Case SHOW_THANK_YOU_SCREEN_PARAMETER
                                .ShowThankYouScreen = BR.ReadBoolean
                            Case SHOW_WELCOME_SURVEY_PARAMETER
                                .ShowWelcomeSurvey = BR.ReadBoolean
                            Case SHOW_THANK_YOU_SURVEY_PARAMETER
                                .ShowThankYouSurvey = BR.ReadBoolean
                            Case SHOW_SURVEY_PARAMETER
                                .ShowSurvey = BR.ReadBoolean
                            Case SHOW_SENSITIVITY_ANALYSIS_PARAMETER
                                .ShowSensitivityAnalysis = BR.ReadBoolean
                            Case SHOW_SENSITIVITY_ANALYSIS_PERFORMANCE_PARAMETER
                                .ShowSensitivityAnalysisPerformance = BR.ReadBoolean
                            Case SHOW_SENSITIVITY_ANALYSIS_GRADIENT_PARAMETER
                                .ShowSensitivityAnalysisGradient = BR.ReadBoolean
                            Case SORT_SENSITIVITY_ANALYSIS_PARAMETER
                                .SortSensitivityAnalysis = BR.ReadBoolean
                            Case CALC_SA_FOR_COMBINED_PARAMETER
                                .CalculateSAForCombined = BR.ReadBoolean
                            Case CALC_SA_WRT_GOAL_PARAMETER
                                .CalculateSAWRTGoal = BR.ReadBoolean
                            Case ALLOW_SWITCH_SA_NODE_PARAMETER
                                .AllowSwitchNodesInSA = BR.ReadBoolean
                            Case ALLOW_AUTOADVANCE_PARAMETER
                                .AllowAutoadvance = BR.ReadBoolean
                            Case SHOW_NEXT_UNASSESSED_PARAMETER
                                .ShowNextUnassessed = BR.ReadBoolean
                            Case SHOW_PROGRESS_INDICATOR_PARAMETER
                                .ShowProgressIndicator = BR.ReadBoolean
                            Case ALLOW_NAVIGATION_PARAMETER
                                .AllowNavigation = BR.ReadBoolean
                            Case SHOW_EXPECTED_VALUE_LOCAL_PARAMETER
                                .ShowExpectedValueLocal = BR.ReadBoolean
                            Case SHOW_EXPECTED_VALUE_GLOBAL_PARAMETER
                                .ShowExpectedValueGlobal = BR.ReadBoolean
                            Case ALLOW_NAVIGATION_PARAMETER
                                .AllowNavigation = BR.ReadBoolean
                            Case ALLOW_MISSING_JUDGMENTS_PARAMETER
                                .AllowMissingJudgments = BR.ReadBoolean
                            Case DISABLE_WARNINGS_ON_STEP_CHANGE_PARAMETER
                                .DisableWarningsOnStepChange = BR.ReadBoolean
                            Case FORCE_GRAPHICAL_PAIRWISE_PARAMETER
                                .ForceGraphical = BR.ReadBoolean
                            Case FORCE_GRAPHICAL_PAIRWISE_FOR_ALTERNATIVES_PARAMETER 'C0985
                                .ForceGraphicalForAlternatives = BR.ReadBoolean
                            Case FORCE_ALL_DIAGONALS_PARAMETER
                                .ForceAllDiagonals = BR.ReadBoolean
                            Case FORCE_ALL_DIAGONALS_LIMIT_PARAMETER
                                .ForceAllDiagonalsLimit = BR.ReadInt32
                            Case FORCE_ALL_DIAGONALS_FOR_ALTERNATIVES_PARAMETER
                                .ForceAllDiagonalsForAlternatives = BR.ReadBoolean
                            Case FORCE_ALL_DIAGONALS_LIMIT_FOR_ALTERNATIVES_PARAMETER
                                .ForceAllDiagonalsLimitForAlternatives = BR.ReadInt32
                            Case INCLUDE_IDEAL_ALTERNATIVE_PARAMETER
                                .IncludeIdealAlternative = BR.ReadBoolean
                            Case SHOW_IDEAL_ALTERNATIVE_PARAMETER
                                .ShowIdealAlternative = BR.ReadBoolean
                            Case MEASURE_MODE_PARAMETER
                                .MeasureMode = BR.ReadInt32
                            Case PROJECT_NAME_PARAMETER
                                .ProjectName = BR.ReadString
                            Case PROJECT_TYPE_PARAMETER
                                .ProjectType = BR.ReadInt32
                            Case PROJECT_PURPOSE_PARAMETER
                                .ProjectPurpose = BR.ReadString
                            Case OBJS_COLUMN_NAME_IN_SA_PARAMETER
                                .ObjectivesColumnNameInSA = BR.ReadString
                            Case ALTS_COLUMN_NAME_IN_SA_PARAMETER
                                .AlternativesColumnNameInSA = BR.ReadString
                            Case ALTS_DEFAULT_CONTRIBUTION_PARAMETER
                                .AltsDefaultContribution = BR.ReadInt32
                            Case ALTS_DEFAULT_CONTRIBUTION_IMPACT_PARAMETER
                                .AltsDefaultContributionImpact = BR.ReadInt32
                            Case DEFAULT_COV_OBJ_MEASUREMENT_TYPE_PARAMETER 'C0753
                                .DefaultCoveringObjectiveMeasurementType = BR.ReadInt32
                            Case DEFAULT_NON_COV_OBJ_MEASUREMENT_TYPE_PARAMETER
                                .DefaultNonCoveringObjectiveMeasurementType = BR.ReadInt32
                            Case START_DATE_PARAMETER
                                .StartDate = BinaryString2DateTime(BR.ReadString)
                            Case END_DATE_PARAMETER
                                .EndDate = BinaryString2DateTime(BR.ReadString)
                            Case TERMINAL_REDIRECT_URL_PARAMETER
                                .TerminalRedirectURL = BR.ReadString
                            Case REDIRECT_AT_THE_END_PARAMETER
                                .RedirectAtTheEnd = BR.ReadBoolean
                            Case LOG_OFF_AT_THE_END_PARAMETER
                                .LogOffAtTheEnd = BR.ReadBoolean
                            Case RANDOM_SEQUENCE_PARAMETER
                                .RandomSequence = BR.ReadBoolean
                            Case SYNHRONOUS_START_IN_POLLING_MODE_PARAMETER
                                .SynchStartInPollingMode = BR.ReadBoolean
                            Case SYNHRONOUS_START_VOTING_ONCE_FACILITATOR_ALLOWS_PARAMETER
                                .SynchStartVotingOnceFacilitatorAllows = BR.ReadBoolean
                            Case SYNHRONOUS_SHOW_GROUP_RESULTS_PARAMETER
                                .SynchShowGroupResults = BR.ReadBoolean
                            Case SYNHRONOUS_USE_OPTIONS_FROM_DECISION_PARAMETER
                                .SynchUseOptionsFromDecision = BR.ReadBoolean
                            Case SYNHRONOUS_USE_VOTING_BOXES_PARAMETER
                                .SynchUseVotingBoxes = BR.ReadBoolean
                            Case TEAM_TIME_ALLOW_MEETING_ID_PARAMETER
                                .TeamTimeAllowMeetingID = BR.ReadBoolean
                            Case TEAM_TIME_ALLOW_ONLY_EMAIL_ADDRESSES_ADDED_TO_SESSION_TO_LOGIN_PARAMETER
                                .TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin = BR.ReadBoolean
                            Case TEAM_TIME_DISPLAY_USERS_WITH_VIEW_ONLY_ACCESS_PARAMETER
                                .TeamTimeDisplayUsersWithViewOnlyAccess = BR.ReadBoolean
                            Case TEAM_TIME_START_IN_PRACTICE_MODE_PARAMETER
                                .TeamTimeStartInPracticeMode = BR.ReadBoolean
                            Case TEAM_TIME_START_IN_ANONYMOUS_MODE_PARAMETER 'C0744
                                .TeamTimeStartInAnonymousMode = BR.ReadBoolean
                            Case TEAM_TIME_SHOW_KEYPAD_NUMBERS_PARAMETER 'C0395
                                .TeamTimeShowKeypadNumbers = BR.ReadBoolean
                            Case TEAM_TIME_SHOW_KEYPAD_LEGENDS_PARAMETER 'C0395
                                .TeamTimeShowKeypadLegends = BR.ReadBoolean
                            Case TEAM_TIME_HIDE_PROJECT_OWNER_PARAMETER 'C0432
                                .TeamTimeHideProjectOwner = BR.ReadBoolean
                            Case TEAM_TIME_INVITATION_SUBJECT_PARAMETER 'C0493
                                .TeamTimeInvitationSubject = BR.ReadString
                            Case TEAM_TIME_INVITATION_BODY_PARAMETER 'C0493
                                .TeamTimeInvitationBody = BR.ReadString
                            Case TEAM_TIME_EVALUATE_INVITATION_SUBJECT_PARAMETER 'C0493
                                .EvaluateInvitationSubject = BR.ReadString  ' D0509
                            Case TEAM_TIME_EVALUATE_INVITATION_BODY_PARAMETER 'C0493
                                .EvaluateInvitationBody = BR.ReadString ' D0509
                            Case TEAM_TIME_INVITATION_SUBJECT2_PARAMETER 'C0493
                                .TeamTimeInvitationSubject2 = BR.ReadString
                            Case TEAM_TIME_INVITATION_BODY2_PARAMETER 'C0493
                                .TeamTimeInvitationBody2 = BR.ReadString
                            Case TEAM_TIME_EVALUATE_INVITATION_SUBJECT2_PARAMETER 'C0493
                                .EvaluateInvitationSubject2 = BR.ReadString  ' D0509
                            Case TEAM_TIME_EVALUATE_INVITATION_BODY2_PARAMETER 'C0493
                                .EvaluateInvitationBody2 = BR.ReadString ' D0509
                            Case TEAM_TIME_USERS_SORTING_PARAMETER
                                .TeamTimeUsersSorting = BR.ReadInt32
                            Case NAME_ALTERNATIVES_PARAMETER
                                .NameAlternatives = BR.ReadString
                            Case NAME_OBJECTIVES_PARAMETER
                                .NameObjectives = BR.ReadString
                            Case INFODOC_SIZE_PARAMETER
                                .InfoDocSize = BR.ReadString
                            Case DEFAULT_GROUP_ID_PARAMETER
                                .DefaultGroupID = BR.ReadInt32
                            Case ASSOCIATED_MODEL_INT_ID_PARAMETER  ' D3114
                                .AssociatedModelIntID = BR.ReadInt32
                            Case ASSOCIATED_MODEL_GUID_ID_PARAMETER ' D3114
                                .AssociatedModelGuidID = BR.ReadString
                            Case JUDGEMENT_PROMT_ID_PARAMETER 'C0576
                                .JudgementPromtID = BR.ReadInt32
                            Case JUDGEMENT_ALTS_PROMT_ID_PARAMETER 'C0619
                                .JudgementAltsPromtID = BR.ReadInt32
                            Case JUDGEMENT_PROMT_PARAMETER
                                .JudgementPromt = BR.ReadString
                            Case JUDGEMENT_ALTS_PROMT_PARAMETER
                                .JudgementAltsPromt = BR.ReadString
                            Case JUDGEMENT_PROMT_MULTI_ID_PARAMETER
                                .JudgementPromtMultiID = BR.ReadInt32
                            Case JUDGEMENT_ALTS_PROMT_MULTI_ID_PARAMETER
                                .JudgementAltsPromtMultiID = BR.ReadInt32
                            Case JUDGEMENT_PROMT_MULTI_PARAMETER
                                .JudgementPromtMulti = BR.ReadString
                            Case JUDGEMENT_ALTS_PROMT_MULTI_PARAMETER
                                .JudgementAltsPromtMulti = BR.ReadString
                            Case SHOW_FULL_OBJECTIVE_PATH_PARAMETER 'C0578
                                '.ShowFullObjectivePath = BR.ReadBoolean
                                .ShowFullObjectivePath = CType(BR.ReadByte, ecShowObjectivePath)   ' D4100
                            Case OBJECTIVES_PAIRWISE_ONE_AT_A_TIME_PARAMETER 'C0977
                                .ObjectivesPairwiseOneAtATime = BR.ReadBoolean
                            Case OBJECTIVES_NON_PAIRWISE_ONE_AT_A_TIME_PARAMETER 'C0977
                                .ObjectivesNonPairwiseOneAtATime = BR.ReadBoolean
                            Case PROJECT_GUID_PARAMETER 'C0805
                                .ProjectGuid = New Guid(BR.ReadBytes(16))
                            Case ALTERNATIVES_PAIRWISE_ONE_AT_A_TIME_PARAMETER 'C0987
                                .AlternativesPairwiseOneAtATime = BR.ReadBoolean
                            Case CHUNK_PARAMETERS_SET
                                BR.BaseStream.Seek(-4, SeekOrigin.Current)
                                res = False
                        End Select
                    End While
                End With
            End Sub

            Public Overrides Function Read(ByVal Stream As MemoryStream) As Boolean
                If PipeParameters Is Nothing Or Stream Is Nothing Then
                    Return False
                End If

                Stream.Seek(0, SeekOrigin.Begin)

                Dim BR As New BinaryReader(Stream)

                ReadOneParametersSet(BR)

                BR.Close()

                Return True
            End Function
        End Class

        <Serializable()> Public Class clsPipeParametersStreamReader_v_1_1_10 'C0461
            Inherits clsPipeParametersStreamReader_v_1_1_8

            Public Overrides Function Read(ByVal Stream As MemoryStream) As Boolean
                If PipeParameters Is Nothing Or Stream Is Nothing Then
                    Return False
                End If

                Stream.Seek(0, SeekOrigin.Begin)

                Dim BR As New BinaryReader(Stream)

                Dim chunk As Int32
                Dim res As Boolean = True

                'C0791===
                Dim oldForceDefault As Boolean = PipeParameters.ForceDefaultParameters
                PipeParameters.ForceDefaultParameters = False
                'C0791==

                While (Stream.Position < Stream.Length - 1) And res
                    chunk = BR.ReadInt32
                    Dim ChunkSize As Integer = BR.ReadInt32
                    Dim curPos As Integer = BR.BaseStream.Position

                    Dim NextChunkPosition As Integer = BR.BaseStream.Position - 4 + ChunkSize

                    Select Case chunk
                        Case CHUNK_PARAMETERS_SET
                            Dim ID As Integer = BR.ReadInt32
                            Dim name As String = BR.ReadString
                            Dim PS As ParameterSet = PipeParameters.AddParameterSet(name)

                            If PS IsNot Nothing Then
                                PipeParameters.CurrentParameterSet = PS
                                ReadOneParametersSet(BR)
                            End If
                        Case Else
                            BR.BaseStream.Seek(NextChunkPosition, SeekOrigin.Begin)
                    End Select
                End While

                BR.Close()

                PipeParameters.ForceDefaultParameters = oldForceDefault 'C0791

                PipeParameters.CurrentParameterSet = PipeParameters.DefaultParameterSet

                Return True
            End Function
        End Class

        <Serializable()> Public MustInherit Class clsPipeParametersStreamWriter 'C0461
            Private mPipeParameters As clsPipeParamaters

            Public Property PipeParameters() As clsPipeParamaters
                Get
                    Return mPipeParameters
                End Get
                Set(ByVal value As clsPipeParamaters)
                    mPipeParameters = value
                End Set
            End Property

            Protected MustOverride Sub WriteOneParametersSet(ByVal BW As BinaryWriter)
            Public MustOverride Function Write() As MemoryStream
        End Class

        <Serializable()> Public Class clsPipeParametersStreamWriter_v_1_1_8 'C0461
            Inherits clsPipeParametersStreamWriter

            Protected Overrides Sub WriteOneParametersSet(ByVal BW As BinaryWriter)
                With PipeParameters
                    BW.Write(FEEDBACK_ON_PARAMETER)
                    BW.Write(.FeedbackOn)

                    BW.Write(EVALUATE_OBJECTIVES_PARAMETER)
                    BW.Write(.EvaluateObjectives)

                    BW.Write(EVALUATE_ALTERNATIVES_PARAMETER)
                    BW.Write(.EvaluateAlternatives)

                    BW.Write(MODEL_EVALUATION_ORDER_PARAMETER)
                    BW.Write(.ModelEvalOrder)

                    BW.Write(GRAPHICAL_PW_MODE_PARAMETER)
                    BW.Write(.GraphicalPWMode)

                    BW.Write(GRAPHICAL_PW_MODE_FOR_ALTS_PARAMETER)
                    BW.Write(.GraphicalPWModeForAlternatives)

                    BW.Write(OBJECTIVES_EVALUATION_DIRECTION_PARAMETER)
                    BW.Write(.ObjectivesEvalDirection)

                    BW.Write(ALTERNATIVES_EVALUATION_MODE_PARAMETER)
                    BW.Write(.AlternativesEvalMode)

                    BW.Write(PAIRWISE_TYPE_PARAMETER)
                    BW.Write(.PairwiseType)

                    'C0985===
                    BW.Write(PAIRWISE_TYPE_FOR_ALTERNATIVES_PARAMETER)
                    BW.Write(.PairwiseTypeForAlternatives)
                    'C0985==

                    BW.Write(SYNTHESIS_MODE_PARAMETER)
                    BW.Write(.SynthesisMode)

                    'C0945===
                    BW.Write(COMBINED_MODE_PARAMETER)
                    BW.Write(.CombinedMode)
                    'C0945==

                    BW.Write(IDEAL_VIEW_TYPE_PARAMETER)
                    BW.Write(.IdealViewType)

                    BW.Write(EVALUATE_DIAGONALS_PARAMETER)
                    BW.Write(.EvaluateDiagonals)

                    'C0987===
                    BW.Write(EVALUATE_DIAGONALS_ALTERNATIVES_PARAMETER)
                    BW.Write(.EvaluateDiagonalsAlternatives)
                    'C0987==

                    BW.Write(LOCAL_RESULTS_VIEW_PARAMETER)
                    BW.Write(.LocalResultsView)

                    BW.Write(GLOBAL_RESULTS_VIEW_PARAMETER)
                    BW.Write(.GlobalResultsView)

                    'C1045===
                    BW.Write(WRT_INDOFOCS_SHOW_MODE_PARAMETER)
                    BW.Write(.WRTInfoDocsShowMode)
                    'C1045==

                    'C0820===
                    BW.Write(LOCAL_RESULTS_SORT_MODE_PARAMETER)
                    BW.Write(.LocalResultsSortMode)

                    BW.Write(GLOBAL_RESULTS_SORT_MODE_PARAMETER)
                    BW.Write(.GlobalResultsSortMode)
                    'C0820==

                    BW.Write(SHOW_CONSISTENCY_RATIO_PARAMETER)
                    BW.Write(.ShowConsistencyRatio)

                    BW.Write(SHOW_COMMENTS_PARAMETER)
                    BW.Write(.ShowComments)

                    'C0824===
                    BW.Write(USE_CIS_PARAMETER)
                    BW.Write(.UseCISForIndividuals)
                    'C0824==

                    BW.Write(USE_WEIGHTS_PARAMETER)
                    BW.Write(.UseWeights)

                    BW.Write(SHOW_INFODOCS_PARAMETER)
                    BW.Write(.ShowInfoDocs)

                    BW.Write(SHOW_INFODOCS_MODE_PARAMETER)
                    BW.Write(.ShowInfoDocsMode)

                    BW.Write(SHOW_WELCOME_SCREEN_PARAMETER)
                    BW.Write(.ShowWelcomeScreen)

                    BW.Write(SHOW_THANK_YOU_SCREEN_PARAMETER)
                    BW.Write(.ShowThankYouScreen)

                    BW.Write(SHOW_WELCOME_SURVEY_PARAMETER)
                    BW.Write(.ShowWelcomeSurvey)

                    BW.Write(SHOW_THANK_YOU_SURVEY_PARAMETER)
                    BW.Write(.ShowThankYouSurvey)

                    BW.Write(SHOW_SURVEY_PARAMETER)
                    BW.Write(.ShowSurvey)

                    BW.Write(SHOW_SENSITIVITY_ANALYSIS_PARAMETER)
                    BW.Write(.ShowSensitivityAnalysis)

                    BW.Write(SHOW_SENSITIVITY_ANALYSIS_PERFORMANCE_PARAMETER)
                    BW.Write(.ShowSensitivityAnalysisPerformance)

                    BW.Write(SHOW_SENSITIVITY_ANALYSIS_GRADIENT_PARAMETER)
                    BW.Write(.ShowSensitivityAnalysisGradient)

                    BW.Write(SORT_SENSITIVITY_ANALYSIS_PARAMETER)
                    BW.Write(.SortSensitivityAnalysis)

                    BW.Write(CALC_SA_FOR_COMBINED_PARAMETER)
                    BW.Write(.CalculateSAForCombined)

                    BW.Write(CALC_SA_WRT_GOAL_PARAMETER)
                    BW.Write(.CalculateSAWRTGoal)

                    BW.Write(ALLOW_SWITCH_SA_NODE_PARAMETER)
                    BW.Write(.AllowSwitchNodesInSA)

                    BW.Write(ALLOW_AUTOADVANCE_PARAMETER)
                    BW.Write(.AllowAutoadvance)

                    BW.Write(SHOW_NEXT_UNASSESSED_PARAMETER)
                    BW.Write(.ShowNextUnassessed)

                    BW.Write(SHOW_PROGRESS_INDICATOR_PARAMETER)
                    BW.Write(.ShowProgressIndicator)

                    BW.Write(ALLOW_NAVIGATION_PARAMETER)
                    BW.Write(.AllowNavigation)

                    BW.Write(SHOW_EXPECTED_VALUE_LOCAL_PARAMETER)
                    BW.Write(.ShowExpectedValueLocal)

                    BW.Write(SHOW_EXPECTED_VALUE_GLOBAL_PARAMETER)
                    BW.Write(.ShowExpectedValueGlobal)

                    BW.Write(ALLOW_MISSING_JUDGMENTS_PARAMETER)
                    BW.Write(.AllowMissingJudgments)

                    BW.Write(DISABLE_WARNINGS_ON_STEP_CHANGE_PARAMETER)
                    BW.Write(.DisableWarningsOnStepChange)

                    BW.Write(FORCE_GRAPHICAL_PAIRWISE_PARAMETER)
                    BW.Write(.ForceGraphical)

                    'C0985===
                    BW.Write(FORCE_GRAPHICAL_PAIRWISE_FOR_ALTERNATIVES_PARAMETER)
                    BW.Write(.ForceGraphicalForAlternatives)
                    'C0985==

                    BW.Write(FORCE_ALL_DIAGONALS_PARAMETER)
                    BW.Write(.ForceAllDiagonals)

                    BW.Write(FORCE_ALL_DIAGONALS_LIMIT_PARAMETER)
                    BW.Write(.ForceAllDiagonalsLimit)

                    BW.Write(FORCE_ALL_DIAGONALS_FOR_ALTERNATIVES_PARAMETER)
                    BW.Write(.ForceAllDiagonalsForAlternatives)

                    BW.Write(FORCE_ALL_DIAGONALS_LIMIT_FOR_ALTERNATIVES_PARAMETER)
                    BW.Write(.ForceAllDiagonalsLimitForAlternatives)

                    BW.Write(INCLUDE_IDEAL_ALTERNATIVE_PARAMETER)
                    BW.Write(.IncludeIdealAlternative)

                    BW.Write(SHOW_IDEAL_ALTERNATIVE_PARAMETER)
                    BW.Write(.ShowIdealAlternative)

                    BW.Write(MEASURE_MODE_PARAMETER)
                    BW.Write(.MeasureMode)

                    BW.Write(PROJECT_NAME_PARAMETER)
                    'BW.Write(ProjectName) 'C0347
                    BW.Write(If(.ProjectName Is Nothing, "", .ProjectName)) 'C0347

                    BW.Write(PROJECT_TYPE_PARAMETER)
                    BW.Write(.ProjectType)

                    BW.Write(PROJECT_PURPOSE_PARAMETER)
                    'BW.Write(ProjectPurpose) 'C0347
                    BW.Write(If(.ProjectPurpose Is Nothing, "", .ProjectPurpose)) 'C0347

                    BW.Write(OBJS_COLUMN_NAME_IN_SA_PARAMETER)
                    BW.Write(.ObjectivesColumnNameInSA)

                    BW.Write(ALTS_COLUMN_NAME_IN_SA_PARAMETER)
                    BW.Write(.AlternativesColumnNameInSA)

                    BW.Write(ALTS_DEFAULT_CONTRIBUTION_PARAMETER)
                    BW.Write(.AltsDefaultContribution)

                    BW.Write(ALTS_DEFAULT_CONTRIBUTION_IMPACT_PARAMETER)
                    BW.Write(.AltsDefaultContributionImpact)

                    'C0753===
                    BW.Write(DEFAULT_COV_OBJ_MEASUREMENT_TYPE_PARAMETER)
                    BW.Write(.DefaultCoveringObjectiveMeasurementType)
                    'C0753==
                    BW.Write(DEFAULT_NON_COV_OBJ_MEASUREMENT_TYPE_PARAMETER)
                    BW.Write(.DefaultNonCoveringObjectiveMeasurementType)

                    BW.Write(START_DATE_PARAMETER)
                    BW.Write(DateTime2BinaryString(.StartDate))

                    BW.Write(END_DATE_PARAMETER)
                    BW.Write(DateTime2BinaryString(.EndDate))

                    BW.Write(TERMINAL_REDIRECT_URL_PARAMETER)
                    BW.Write(.TerminalRedirectURL)

                    BW.Write(REDIRECT_AT_THE_END_PARAMETER)
                    BW.Write(.RedirectAtTheEnd)

                    BW.Write(LOG_OFF_AT_THE_END_PARAMETER)
                    BW.Write(.LogOffAtTheEnd)

                    BW.Write(RANDOM_SEQUENCE_PARAMETER)
                    BW.Write(.RandomSequence)

                    BW.Write(SYNHRONOUS_START_IN_POLLING_MODE_PARAMETER)
                    BW.Write(.SynchStartInPollingMode)

                    BW.Write(SYNHRONOUS_START_VOTING_ONCE_FACILITATOR_ALLOWS_PARAMETER)
                    BW.Write(.SynchStartVotingOnceFacilitatorAllows)

                    BW.Write(SYNHRONOUS_SHOW_GROUP_RESULTS_PARAMETER)
                    BW.Write(.SynchShowGroupResults)

                    BW.Write(SYNHRONOUS_USE_OPTIONS_FROM_DECISION_PARAMETER)
                    BW.Write(.SynchUseOptionsFromDecision)

                    BW.Write(SYNHRONOUS_USE_VOTING_BOXES_PARAMETER)
                    BW.Write(.SynchUseVotingBoxes)

                    'C0386===
                    BW.Write(TEAM_TIME_ALLOW_MEETING_ID_PARAMETER)
                    BW.Write(.TeamTimeAllowMeetingID)

                    BW.Write(TEAM_TIME_ALLOW_ONLY_EMAIL_ADDRESSES_ADDED_TO_SESSION_TO_LOGIN_PARAMETER)
                    BW.Write(.TeamTimeAllowOnlyEmailAddressesAddedToSessionToLogin)

                    BW.Write(TEAM_TIME_DISPLAY_USERS_WITH_VIEW_ONLY_ACCESS_PARAMETER)
                    BW.Write(.TeamTimeDisplayUsersWithViewOnlyAccess)

                    BW.Write(TEAM_TIME_START_IN_PRACTICE_MODE_PARAMETER)
                    BW.Write(.TeamTimeStartInPracticeMode)
                    'C0386==

                    'C0744===
                    BW.Write(TEAM_TIME_START_IN_ANONYMOUS_MODE_PARAMETER) 'C0751
                    BW.Write(.TeamTimeStartInAnonymousMode)
                    'C0744==

                    'C0395===
                    BW.Write(TEAM_TIME_SHOW_KEYPAD_NUMBERS_PARAMETER)
                    BW.Write(.TeamTimeShowKeypadNumbers)

                    BW.Write(TEAM_TIME_SHOW_KEYPAD_LEGENDS_PARAMETER)
                    BW.Write(.TeamTimeShowKeypadLegends)
                    'C0395==

                    'C0432===
                    BW.Write(TEAM_TIME_HIDE_PROJECT_OWNER_PARAMETER)
                    BW.Write(.TeamTimeHideProjectOwner)
                    'C0432==

                    'C0493===
                    BW.Write(TEAM_TIME_INVITATION_SUBJECT_PARAMETER)
                    BW.Write(.TeamTimeInvitationSubject)

                    BW.Write(TEAM_TIME_INVITATION_BODY_PARAMETER)
                    BW.Write(.TeamTimeInvitationBody)

                    BW.Write(TEAM_TIME_EVALUATE_INVITATION_SUBJECT_PARAMETER)
                    BW.Write(.EvaluateInvitationSubject)    ' D0509

                    BW.Write(TEAM_TIME_EVALUATE_INVITATION_BODY_PARAMETER)
                    BW.Write(.EvaluateInvitationBody)   ' D0509
                    'C0493==

                    BW.Write(TEAM_TIME_INVITATION_SUBJECT2_PARAMETER)
                    BW.Write(.TeamTimeInvitationSubject2)

                    BW.Write(TEAM_TIME_INVITATION_BODY2_PARAMETER)
                    BW.Write(.TeamTimeInvitationBody2)

                    BW.Write(TEAM_TIME_EVALUATE_INVITATION_SUBJECT2_PARAMETER)
                    BW.Write(.EvaluateInvitationSubject2)    ' D0509

                    BW.Write(TEAM_TIME_EVALUATE_INVITATION_BODY2_PARAMETER)
                    BW.Write(.EvaluateInvitationBody2)   ' D0509

                    BW.Write(TEAM_TIME_USERS_SORTING_PARAMETER)
                    BW.Write(.TeamTimeUsersSorting)

                    BW.Write(NAME_ALTERNATIVES_PARAMETER)
                    BW.Write(.NameAlternatives)

                    BW.Write(NAME_OBJECTIVES_PARAMETER)
                    BW.Write(.NameObjectives)

                    BW.Write(INFODOC_SIZE_PARAMETER)
                    BW.Write(.InfoDocSize)

                    BW.Write(DEFAULT_GROUP_ID_PARAMETER)
                    BW.Write(.DefaultGroupID)   ' D0509

                    BW.Write(ASSOCIATED_MODEL_INT_ID_PARAMETER)
                    BW.Write(.AssociatedModelIntID)

                    BW.Write(ASSOCIATED_MODEL_GUID_ID_PARAMETER)
                    BW.Write(.AssociatedModelGuidID)

                    BW.Write(JUDGEMENT_PROMT_ID_PARAMETER)
                    BW.Write(.JudgementPromtID)   ' D0509

                    BW.Write(JUDGEMENT_ALTS_PROMT_ID_PARAMETER)
                    BW.Write(.JudgementAltsPromtID)   ' D0509

                    BW.Write(JUDGEMENT_PROMT_PARAMETER)
                    BW.Write(.JudgementPromt)

                    BW.Write(JUDGEMENT_ALTS_PROMT_PARAMETER)
                    BW.Write(.JudgementAltsPromt)

                    BW.Write(JUDGEMENT_PROMT_MULTI_ID_PARAMETER)
                    BW.Write(.JudgementPromtMultiID)

                    BW.Write(JUDGEMENT_ALTS_PROMT_MULTI_ID_PARAMETER)
                    BW.Write(.JudgementAltsPromtMultiID)

                    BW.Write(JUDGEMENT_PROMT_MULTI_PARAMETER)
                    BW.Write(.JudgementPromtMulti)

                    BW.Write(JUDGEMENT_ALTS_PROMT_MULTI_PARAMETER)
                    BW.Write(.JudgementAltsPromtMulti)

                    'C0578===
                    BW.Write(SHOW_FULL_OBJECTIVE_PATH_PARAMETER)
                    BW.Write(CByte(.ShowFullObjectivePath)) ' D4100
                    'C0578==

                    'C0977===
                    BW.Write(OBJECTIVES_PAIRWISE_ONE_AT_A_TIME_PARAMETER)
                    BW.Write(.ObjectivesPairwiseOneAtATime)

                    BW.Write(OBJECTIVES_NON_PAIRWISE_ONE_AT_A_TIME_PARAMETER)
                    BW.Write(.ObjectivesNonPairwiseOneAtATime)
                    'C0977==

                    'C0987===
                    BW.Write(ALTERNATIVES_PAIRWISE_ONE_AT_A_TIME_PARAMETER)
                    BW.Write(.AlternativesPairwiseOneAtATime)
                    'C0987==

                    'C0805===
                    BW.Write(PROJECT_GUID_PARAMETER)
                    BW.Write(.ProjectGuid.ToByteArray)
                    'C0805==
                End With
            End Sub

            Public Overrides Function Write() As System.IO.MemoryStream
                If PipeParameters Is Nothing Then
                    Return Nothing
                End If

                Dim MS As New MemoryStream
                Dim BW As New BinaryWriter(MS)

                Dim oldPS As ParameterSet = PipeParameters.CurrentParameterSet
                PipeParameters.CurrentParameterSet = PipeParameters.DefaultParameterSet

                WriteOneParametersSet(BW)

                PipeParameters.CurrentParameterSet = oldPS

                BW.Close()

                Return MS
            End Function
        End Class

        <Serializable()> Public Class clsPipeParametersStreamWriter_v_1_1_10 'C0461
            Inherits clsPipeParametersStreamWriter_v_1_1_8

            Public Overrides Function Write() As System.IO.MemoryStream
                If PipeParameters Is Nothing Then
                    Return Nothing
                End If

                Dim MS As New MemoryStream
                Dim BW As New BinaryWriter(MS)

                Dim oldPS As ParameterSet = PipeParameters.CurrentParameterSet

                'C0791===
                Dim oldForceDefault As Boolean = PipeParameters.ForceDefaultParameters
                PipeParameters.ForceDefaultParameters = False
                'C0791==

                For Each PS As ParameterSet In PipeParameters.ParameterSets
                    PipeParameters.CurrentParameterSet = PS

                    BW.Write(CHUNK_PARAMETERS_SET)
                    Dim curPos As Integer = BW.BaseStream.Position
                    BW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    BW.Write(PS.ID)
                    BW.Write(PS.Name)

                    WriteOneParametersSet(BW)

                    Dim chunkSize As Integer = BW.BaseStream.Position - curPos
                    BW.Seek(-chunkSize, SeekOrigin.Current)
                    BW.Write(chunkSize)
                    BW.Seek(0, SeekOrigin.End)
                Next

                PipeParameters.ForceDefaultParameters = oldForceDefault 'C0791

                PipeParameters.CurrentParameterSet = oldPS

                BW.Close()

                Return MS
            End Function
        End Class

    End Module
End Namespace