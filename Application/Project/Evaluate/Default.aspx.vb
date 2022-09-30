Imports System.Drawing
Imports ExpertChoice.Web.Controls 'C0384
Imports Telerik.Web.UI  ' D0535
Imports SpyronControls.Spyron.Core
Imports ExpertChoice.Results

Partial Class EvaluationPage2
    Inherits clsComparionCorePage

    Private _FRAMED_INFODOC_COOKIES As String() = {"node", "alt", "wrt", "goal",
                                                   "di_node", "di_alt", "di_wrt",
                                                   "md", "mr",
                                                   "mp", "mp_wrt",
                                                   "pw_goal", "pw_node", "pw_wrt",
                                                   "r_node", "r_alt", "r_wrt",
                                                   "sf_node", "sf_alt", "sf_wrt",
                                                   "uc_node", "uc_alt", "uc_wrt"}   ' D1864

    Private Const JudgUndefined = "undef."      ' D7262
    Private Const JudgComment = ", +comment"    ' D7262

    Public StepsCount As Integer = 11           ' D0023 + D1636 + D4855 + D4933
    Public HelpID As ecEvaluationStepType = ecEvaluationStepType.Other   ' D1028 + D3695
    Public ShowQuickHelp As Boolean = False     ' D3695
    Public AutoShowQuickHelp As Boolean = False ' D3941
    Public QuickHelpContent As String = ""      ' D3696 + D3720
    Public QuickHelpParams As String = ""       ' D3695
    Private JudgLogs As String = ""             ' D7262
    Private evalProgressTotal As Integer = 0    ' D7262
    Private evalProgressMade As Integer = 0     ' D7262

    'Private RISKION_JOINED_PIPE As Boolean = True     ' D3253 + D3771 -D4103 // Use .Parameters.EvalJoinRiskionPipes

    Private _OPT_RISK_RESULTS_PGID As Integer = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS '_PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS_4PIPE
    Private Riskion_ShowRiskResultsScreen As Boolean = True   ' D4715
    Public Const SHOW_OPTIMIZATION_SWICTH As Boolean = False    ' D3069

    Private _SESS_PREV_STEP As String = "PrevStep"  ' D3730
    Private _SESS_EARLIER_STEP As String = "EarlierStep"    ' D7604

    Public Const Autoadvance_Max_Judgments As Integer = 5   ' D2078
    Public COOKIE_CHECK_AUTOADVANCE As String = "aa_"       ' D2078
    Public COOKIE_CHROME_PM As String = "ChromePM"          ' D3227
    Public AutoAdvance_Judgment_Types As ActionType() = {ActionType.atPairwise, ActionType.atPairwiseOutcomes} ' D2078

    Private Const AlwaysShowAutoAdvance As Boolean = True    ' D0901
    Private _TIMEOUT_BEFORE As Integer = 5 * 60     ' D0651
    Private _TIMEOUT_IDLE_PW As Integer = 30        ' D1310 // user idle in second for show help 

    Private SurveyHasRequiredMissed As Boolean = False  ' D7365

    Private _ShowSLLocalResults As Boolean = True       ' D0566 + D0776 + D0855 + D2305
    Private _CheckedSLLocalResults As Boolean = False   ' D2303

    Private _WS As clsWorkspace = Nothing       ' D0051
    Private _UW As clsUserWorkgroup = Nothing   ' D1063
    Private _CurAction As clsAction = Nothing   ' D4116
    Private _CurNode As clsNode = Nothing       ' D4116

    'Private _Extra_AutoAdvance As Boolean = False   ' D0163
    'Private _Loaded_AutoAdvance As Boolean = False  ' D0163
    Private _CanEdit_AutoAdvance As Boolean = False ' D0167
    Private CanUserEditClusterPhrase As Boolean = False     ' D4116

    'Private _ResultsControl As ctrlShowResults2 = Nothing  ' D0210 -D3388

    Private Const _SeekUnassessedFromCurrent As Boolean = False ' D0148

    Private Const pwMapName As String = "pwmap"         ' D2025 + D2038

    Private _NextProject As clsProject = Nothing        ' D4163
    Private _NextProjectChecked As Boolean = False      ' D4163

    Private _DataInstanceStatusChecked As Boolean = False   ' D0228
    Private _DataInstance As Boolean = False    ' D0228

    Private _ReadOnlyStatusChecked As Boolean = False   ' D0309
    Private _ReadOnly As Boolean = False    ' D0309

    Private _IsRiskWithControlsChecked As Boolean = False   ' D2502
    Private _IsRiskWithControls As Boolean = False          ' D2502

    Private _IntensitiesStatusChecked As Boolean = False    ' D1800
    Private _Intensities As Boolean = False                 ' D1800
    Private _IntensityScale As clsMeasurementScale = Nothing    ' D1800

    Private _RO_UID As Integer = -1

    Const SESSION_VIEW_MODE As String = "ViewAsEvaluator"               ' D4233
    Private _OriginalAHPUser As clsUser = Nothing   ' D0231

    Public CurrentStepBtnID As String = ""  ' D2154

    Public CanShowApplyTo As Boolean = True ' D2651
    Public CanShowTitleApplyTo As Boolean = True ' D2651

    Public USD_OptionVisible As Boolean = False ' D3795
    Public USD_onClientClick As String = "" ' D3795

    Public isMultiRatingsOrDirect As Boolean = False   ' D2926

    Private Const _OPT_STEPS_PAGE_SIZE As Integer = 150 ' D3073
    Private Const _OPT_STEPS_SHOW_MAX As Integer = 100  ' D3073

    Public isGPWStep As Boolean = False        ' D3437

    Public CanUserEditActiveProject As Boolean = False  ' D4233
    Public IsPM As Boolean = False      ' D3795
    Public IsRealPM As Boolean = False  ' D4233

    ' D0855 ===
    Private ReadOnly Property ShowSLLocalResults() As Boolean
        Get
            If Not _CheckedSLLocalResults Then  ' D2303
                '_ShowSLLocalResults = isSLTheme()   ' D2303 -D2305
                If GetCookie("results", "").ToLower = "html" Then _ShowSLLocalResults = False
                If GetCookie("results", "").ToLower = "sl" Then _ShowSLLocalResults = True ' D2303
                If CheckVar(_PARAM_ACTION, "").ToLower = "html" Then
                    _ShowSLLocalResults = False
                    SetCookie("results", "html", True, False)
                End If
                ' D2303 ===
                If CheckVar(_PARAM_ACTION, "").ToLower = "sl" Then
                    _ShowSLLocalResults = True
                    SetCookie("results", "sl", True, False)
                End If
                ' D2303 ==
                _CheckedSLLocalResults = True   ' D2303
            End If
            Return _ShowSLLocalResults
        End Get
    End Property
    ' D0855 ==

    ' D1938 ===
    Public ReadOnly Property ShowInfoDocs As Boolean    ' D1941
        Get
            'Return IIf(IsRiskWithControls, False, IIf(IsIntensities, True, App.ActiveProject.ProjectManager.PipeBuilder.ShowInfoDocs))    ' D1941 + D2502
            Return If(IsIntensities, True, App.ActiveProject.ProjectManager.PipeBuilder.ShowInfoDocs)    ' D1941 + D2502 + D4346
        End Get
    End Property

    Public ReadOnly Property ShowComments As Boolean    ' D1941
        Get
            'Return IIf(IsRiskWithControls, False, IIf(IsIntensities, False, App.ActiveProject.ProjectManager.PipeBuilder.ShowComments))     ' D1941 + D2502
            Return If(IsIntensities, False, App.ActiveProject.ProjectManager.PipeBuilder.ShowComments)     ' D1941
        End Get
    End Property
    ' D1938 ==

    '' D2091 ===
    'Public ReadOnly Property CanUserEditActiveProject As Boolean
    '    Get
    '        Return CanEditActiveProject AndAlso App.CanUserModifyProject(IIf(IsReadOnly, ReadOnly_UserID, App.ActiveUser.UserID), App.ProjectID, UW, WS, App.ActiveWorkgroup)
    '    End Get
    'End Property
    '' D2091 ==

    Public Property InfodocParams(nodeid As Guid, wrtnodeid As Guid) As String
        Get
            Return CStr(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_INFODOC_PARAMS_ID, nodeid, wrtnodeid))
        End Get
        Set(value As String)
            With App.ActiveProject.ProjectManager
                .Attributes.SetAttributeValue(ATTRIBUTE_INFODOC_PARAMS_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, nodeid, wrtnodeid)
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End Set
    End Property

    ' D3738 ===
    ReadOnly Property SESSION_SHOW_DOLLAR_VALUE As String
        Get
            Return String.Format("RiskResultsShowCostOfGoal_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property USD_CostOfGoal As Double    ' D3795
        Get
            Return CDbl(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Double)
            App.ActiveProject.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_DOLLAR_VALUE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, value, Guid.Empty, Guid.Empty)   ' D3795
            WriteAttributeValues(App.ActiveProject, "Total Cost Changed to " + CostString(value), "")   ' D3795
        End Set
    End Property

    Public Property USD_ShowCostOfGoal As Boolean
        Get
            Dim retVal As Boolean = False
            If USD_CostOfGoal <> UNDEFINED_INTEGER_VALUE AndAlso USD_OptionVisible Then    ' D3864
                Dim s = SessVar(SESSION_SHOW_DOLLAR_VALUE)
                If Not String.IsNullOrEmpty(s) AndAlso s = "1" Then retVal = True
            End If
            Return retVal
        End Get
        Set(value As Boolean)
            SessVar(SESSION_SHOW_DOLLAR_VALUE) = CStr(IIf(value, "1", "0"))
        End Set
    End Property
    ' D3738 ==

    ' D0051 ===
    Private Property WS() As clsWorkspace
        Get
            If _WS Is Nothing AndAlso App.HasActiveProject Then    ' D7577
                ' D0194 ===
                Dim tUserID As Integer = App.ActiveUser.UserID  ' D0486
                ' D0309 ===
                If IsReadOnly AndAlso ReadOnly_UserID > 0 Then tUserID = ReadOnly_UserID ' D0486
                ' D0309 ==

                ' D0527 ===
                _WS = clsWorkspace.WorkspaceByUserIDAndProjectID(tUserID, App.ProjectID, App.Workspaces)
                If _WS Is Nothing Then
                    _WS = App.DBWorkspaceByUserIDProjectID(tUserID, App.ProjectID) ' D0486
                    If IsReadOnly Then App.Workspaces.Add(_WS)
                End If
                ' D0527 ==
                ' D0194 ==
                ' D0486 ===
                If _WS Is Nothing Then
                    Dim tUser As clsApplicationUser = App.ActiveUser
                    Dim fCanEdit As Boolean = App.CanUserModifyProject(tUserID, App.ProjectID, UW, clsWorkspace.WorkspaceByUserIDAndProjectID(tUserID, App.ProjectID, App.Workspaces)) ' D0835 + D1063
                    _WS = App.AttachProject(tUser, App.ActiveProject, Not fCanEdit, App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, If(fCanEdit, ecRoleGroupType.gtProjectManager, ecRoleGroupType.gtEvaluator)), "", False)    ' D0499 + D2287 + D2644 + D2780
                    If _WS Is Nothing AndAlso App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then FetchAccessByWrongLicense() ' D1490
                End If
                ' D0486 ==
            End If
            Return _WS
        End Get
        Set(value As clsWorkspace)
            _WS = value
        End Set
    End Property
    ' D0051 ==

    ' D1063 ===
    Private Property UW() As clsUserWorkgroup
        Get
            If _UW Is Nothing Then
                Dim tUserID As Integer = App.ActiveUser.UserID
                If IsReadOnly AndAlso ReadOnly_UserID > 0 Then tUserID = ReadOnly_UserID

                _UW = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUserID, App.ActiveWorkgroup.ID, App.UserWorkgroups)
                If _UW Is Nothing Then
                    _UW = App.DBUserWorkgroupByUserIDWorkgroupID(tUserID, App.ActiveWorkgroup.ID)
                End If
            End If
            Return _UW
        End Get
        Set(value As clsUserWorkgroup)
            _UW = value
        End Set
    End Property
    ' D1063 ==

    ' D1916 ===
    Public ReadOnly Property isImpact() As Boolean  ' D2010
        Get
            Return App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
        End Get
    End Property

    Public ReadOnly Property isLikelihood() As Boolean  ' D2010
        Get
            Return App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
        End Get
    End Property
    ' D1916 ==

    ' D4118 ===
    Public ReadOnly Property CurAction As clsAction
        Get
            If _CurAction Is Nothing Then _CurAction = GetAction(CurStep)
            Return _CurAction
        End Get
    End Property

    Public ReadOnly Property CurStepNode As clsNode
        Get
            If _CurNode Is Nothing Then _CurNode = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNode(CurAction)
            Return _CurNode
        End Get
    End Property
    ' D4188 ==

    ' D0023 ===
    Public Property CurStep() As Integer
        Get
            If App.HasActiveProject Then    ' D7577
                Dim S As Integer = WS.ProjectStep(App.ActiveProject.isImpact)   ' D0420 + D1945
                If S < 1 Then S = 1 Else If S > App.ActiveProject.Pipe.Count Then S = App.ActiveProject.Pipe.Count
                Return S
            Else
                Return -1
            End If
        End Get
        Set(value As Integer)
            ' D0308 ===
            If IsReadOnly OrElse IsIntensities Then
                WS.ProjectStep(App.ActiveProject.isImpact) = value  ' D1945
                _CurAction = Nothing    ' D4116
                _CurNode = Nothing      ' D4116
            Else
                If value < 1 Then value = 1 ' D0625
                ' D0308 ==
                If WS.ProjectStep(App.ActiveProject.isImpact) <> value Then 'C0404
                    '' D3730 ===
                    'If WS.ProjectStep(App.ActiveProject.isImpact) > 0 Then
                    '    Session(_SESS_PREV_STEP + If(App.ActiveProject.isImpact, "_i_", "") + App.ProjectID.ToString) = WS.ProjectStep(App.ActiveProject.isImpact) ' D6676
                    'End If
                    '' D3730 ==
                    WS.ProjectStep(App.ActiveProject.isImpact) = value  ' D0420 + D1945
                    App.DBWorkspaceUpdate(WS, False, Nothing)  ' D0420
                    _CurAction = Nothing    ' D4116
                    _CurNode = Nothing      ' D4116
                End If
            End If
        End Set
    End Property
    ' D0023 ==

    ' D4163 ===
    Public ReadOnly Property NextProject As clsProject
        Get
            If Not _NextProjectChecked Then
                _NextProject = GetNextProject(App.ActiveProject)
                _NextProjectChecked = True
            End If
            Return _NextProject
        End Get
    End Property
    ' D4163 ==

    ' D1635 ==
    Public Function GetMaxStepNumber() As Integer
        ' D2001 ===
        If isAllowMissingJudgments() Then
            Return App.ActiveProject.Pipe.Count
        Else
            Dim NU As Integer = GetNextUnassessed(1)
            If NU < 1 Then NU = App.ActiveProject.Pipe.Count
            Return NU
        End If
        ' D2001 ==
    End Function
    ' D1635 ==

    ' D0308 + D0309  ===
    Public Property IsReadOnly() As Boolean
        Get
            If Not _ReadOnlyStatusChecked And App.HasActiveProject() Then
                If Not _ReadOnly Then _ReadOnly = (CurrentPageID = _PGID_EVALUATE_READONLY OrElse CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS_READONLY) ' D4285
                If Request.Params(_PARAM_READONLY) IsNot Nothing Then _ReadOnly = CheckVar(_PARAM_READONLY, _ReadOnly)
                If _ReadOnly AndAlso Not App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace) AndAlso
                   Not App.CanUserDoProjectAction(ecActionType.at_mlViewOverallResults, App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup) Then _ReadOnly = False ' D0486 + D0835 + D2650
                If Not App.ActiveProject Is Nothing Then _ReadOnlyStatusChecked = True
                If _ReadOnly Then NavigationPageID = If(IsRiskWithControls, _PGID_EVALUATE_RISK_CONTROLS, _PGID_EVALUATION)       ' D4945 + D7173
            End If
            Return _ReadOnly
        End Get
        Set(value As Boolean)
            _ReadOnly = value
        End Set
    End Property
    ' D0308 + D0309 ==

    ' D4103 ===
    Public ReadOnly Property JoinRiskionPipes As Boolean
        Get
            Return Not IsRiskWithControls AndAlso App.ActiveProject.ProjectManager.Parameters.EvalJoinRiskionPipes
        End Get
    End Property
    ' D4103 ==

    ' D2712 ===
    Public Function COOKIE_CHECK_SF() As String
        Return "CheckSF" + App.ProjectID.ToString
    End Function
    ' D2712 ==

    ' D2952 ===
    Public Function _COOKIE_NOVICE_MISSING() As String
        Return "NoviceMissing" + App.ProjectID.ToString
    End Function
    ' D2952 ==

    ' D6962 ===
    Public Function COOKIE_T2S_MODE() As String
        Return "t2s" + App.ProjectID.ToString
    End Function

    Public Function showT2S() As Boolean
        Return If(_OPT_EVAL_T2S_ALLOWED, App.ActiveProject.ProjectManager.Parameters.EvalTextToSpeech <> ecText2Speech.Disabled AndAlso (isEvaluation(CurAction) OrElse CurAction.ActionType = ActionType.atShowLocalResults OrElse CurAction.ActionType = ActionType.atShowGlobalResults), False) ' D6991 + D7005 + D7066
    End Function
    ' D6962 ==

    ' D2502 ===
    Public Property IsRiskWithControls() As Boolean
        Get
            If Not _IsRiskWithControlsChecked AndAlso App.isRiskEnabled AndAlso App.HasActiveProject() Then
                If Not _IsRiskWithControls Then _IsRiskWithControls = CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS ' D4285
                If Request.Params("mode") IsNot Nothing Then _IsRiskWithControls = CheckVar("mode", "").ToLower.Trim = "riskcontrols"
                _IsRiskWithControlsChecked = True
            End If
            Return _IsRiskWithControls
        End Get
        Set(value As Boolean)
            _IsRiskWithControls = value
        End Set
    End Property
    ' D2502 ==

    ' D1800 ===
    Public Property IsIntensities() As Boolean
        Get
            If Not _IntensitiesStatusChecked And App.HasActiveProject() Then
                _Intensities = CurrentPageID = _PGID_EVALUATE_INTENSITIES
                If Not Request.Params(_PARAM_INTENSITIES) Is Nothing Then _Intensities = CheckVar(_PARAM_INTENSITIES, _Intensities)
                If _Intensities AndAlso Not IsPM Then _Intensities = False ' D4116
                If _Intensities Then
                    Dim sRID As String = SessVar(SESSION_SCALE_ID)  ' D1965
                    If sRID Is Nothing Then sRID = ""
                    Dim sGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("rid", sRID)).Trim.ToLower   'Anti-XSS
                    If sGUID <> "" Then
                        For Each tRating As clsRatingScale In App.ActiveProject.ProjectManager.MeasureScales.RatingsScales
                            If tRating.GuidID.ToString.ToLower = sGUID Then _IntensityScale = tRating
                        Next
                        For Each tStepFunction As clsStepFunction In App.ActiveProject.ProjectManager.MeasureScales.StepFunctions
                            If tStepFunction.GuidID.ToString.ToLower = sGUID Then _IntensityScale = tStepFunction
                        Next
                    End If
                End If
                If Not _Intensities Then _IntensityScale = Nothing
                If _Intensities AndAlso _IntensityScale IsNot Nothing Then
                    SessVar(SESSION_ORIGINAL_HID) = CStr(App.ActiveProject.ProjectManager.ActiveHierarchy)    ' D1965
                    SessVar(SESSION_SCALE_ID) = _IntensityScale.GuidID.ToString ' D1965

                    ' D1935 ===
                    Dim sExcludeGUIDs As String = ""
                    sExcludeGUIDs = EcSanitizer.GetSafeHtmlFragment(CheckVar("exclude", "")).Trim() 'Anti-XSS
                    If sExcludeGUIDs = "" AndAlso Request("exclude") Is Nothing AndAlso SessVar(SESSION_EXCLUDE_GUIDS) IsNot Nothing AndAlso SessVar(SESSION_EXCLUDE_GUIDS) <> "" Then sExcludeGUIDs = SessVar(SESSION_EXCLUDE_GUIDS) ' D2070
                    If sExcludeGUIDs Is Nothing Then sExcludeGUIDs = ""
                    SessVar(SESSION_EXCLUDE_GUIDS) = sExcludeGUIDs
                    Dim ExcludeGUIDs As String() = {}   ' D2065
                    If sExcludeGUIDs <> "" Then ExcludeGUIDs = sExcludeGUIDs.Split(CType(",", Char())) ' D2065
                    ' D1935 ==

                    ScriptManager.RegisterStartupScript(Me, GetType(String), "fixHTMLHost", "if ((window.parent) && ((typeof window.parent.fixEvalIntensHtmlHost)=='function')) window.parent.fixEvalIntensHtmlHost(); else if ((window.opener) && ((typeof window.opener.fixEvalIntensHtmlHost)=='function')) window.opener.fixEvalIntensHtmlHost();", True)

                    Dim tHierarchy As clsHierarchy = Nothing
                    If TypeOf (_IntensityScale) Is clsRatingScale Then tHierarchy = App.ActiveProject.ProjectManager.MeasureScales.CreateHierarchyFromRatingScale(CType(_IntensityScale, clsRatingScale), ExcludeGUIDs) ' D1935
                    If TypeOf (_IntensityScale) Is clsStepFunction Then tHierarchy = App.ActiveProject.ProjectManager.MeasureScales.CreateHierarchyFromStepFunction(CType(_IntensityScale, clsStepFunction), ExcludeGUIDs) ' D1821
                    If tHierarchy IsNot Nothing Then
                        App.ActiveProject.ProjectManager.ActiveHierarchy = tHierarchy.HierarchyID

                        ' D2020 ===
                        Dim sType As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("Type", "")) 'Anti-XSS
                        If sType <> "" Then
                            Dim InfodocsMode As ShowInfoDocsMode = App.ActiveProject.ProjectManager.PipeParameters.ShowInfoDocsMode ' D4320
                            Dim fOverrideSettings As Boolean = App.ActiveProject.PipeParameters.CurrentParameterSet IsNot App.ActiveProject.PipeParameters.MeasureParameterSet  ' D4320

                            App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = App.ActiveProject.ProjectManager.PipeParameters.MeasureParameterSet

                            App.ActiveProject.PipeParameters.PairwiseType = If(sType = "2", PairwiseType.ptGraphical, PairwiseType.ptVerbal)
                            App.ActiveProject.PipeParameters.ForceGraphical = CheckVar("ForceGraphical", "0") = "1"

                            Dim sDiags As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("NumberOfJudgments", "1"))  'Anti-XSS
                            App.ActiveProject.PipeParameters.ForceAllDiagonals = False  ' D6123
                            App.ActiveProject.PipeParameters.EvaluateDiagonals = If(sDiags = "0", DiagonalsEvaluation.deAll, If(sDiags = "1", DiagonalsEvaluation.deFirst, DiagonalsEvaluation.deFirstAndSecond))

                            App.ActiveProject.PipeParameters.ShowConsistencyRatio = CheckVar("ShowInconsistency", True)
                            App.ActiveProject.PipeParameters.ObjectivesPairwiseOneAtATime = Not CheckVar("ShowMultiPairwise", True)

                            If fOverrideSettings Then App.ActiveProject.ProjectManager.PipeParameters.ShowInfoDocsMode = InfodocsMode ' D4320

                            App.ActiveProject.SaveProjectOptions("Edit options on access intensities", , False)
                        End If
                        ' D2020 ==

                        'App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False

                    Else
                        _IntensityScale = Nothing
                    End If

                End If
                _IntensitiesStatusChecked = True
            End If
            Return _Intensities
        End Get
        Set(value As Boolean)
            _Intensities = value
        End Set
    End Property

    Public Property IntensityScale As clsMeasurementScale
        Get
            If IsIntensities Then Return _IntensityScale Else Return Nothing
        End Get
        Set(value As clsMeasurementScale)
            _IntensityScale = value
        End Set
    End Property
    ' D1800 ==

    ' D0309 ===
    Private ReadOnly Property Sess_RO_UserID() As String
        Get
            Return String.Format("RO_UID_{0}", App.ProjectID)
        End Get
    End Property

    ' D2346 ===
    Public Property ReadOnly_UserID() As Integer
        Get
            If _RO_UID <= 0 Then
                If Session(Sess_RO_UserID) IsNot Nothing Then Integer.TryParse(Session(Sess_RO_UserID).ToString, _RO_UID)
                Dim sUID As String = Request(_PARAM_ID)
                If IsReadOnly AndAlso Not String.IsNullOrEmpty(sUID) Then
                    If Integer.TryParse(sUID, _RO_UID) Then
                        If _RO_UID > 0 AndAlso App.DBUserByID(_RO_UID) Is Nothing Then _RO_UID = -1
                    End If
                    If _RO_UID <= 0 Then
                        Dim tUser As clsApplicationUser = App.DBUserByEmail(sUID)
                        If tUser IsNot Nothing Then
                            SessVar(Sess_RO_UserID) = tUser.UserID.ToString

                            ' D2823 ===
                            Dim sMode As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("mode", "")).Trim.ToLower    'Anti-XSS
                            Dim sNodeGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("node", "")).Trim    'Anti-XSS
                            Dim sExtra As String = ""
                            If sMode <> "" AndAlso sNodeGUID <> "" Then sExtra = String.Format("mode={0}&node={1}&", sMode, sNodeGUID)
                            ' D2823 ==
                            Response.Redirect(PageURL(If(IsRiskWithControls, _PGID_EVALUATE_RISK_CONTROLS_READONLY, _PGID_EVALUATE_READONLY), sExtra + GetTempThemeURI(False)), True)   ' D2348 + D4285
                        End If
                    End If
                End If
                If _RO_UID <= 0 Then _RO_UID = App.ActiveUser.UserID
                If Session(Sess_RO_UserID) Is Nothing Then Session.Add(Sess_RO_UserID, _RO_UID) Else Session(Sess_RO_UserID) = _RO_UID
            End If
            Return _RO_UID
            ' D2346 ==
        End Get
        Set(value As Integer)
            Session(Sess_RO_UserID) = value
            _RO_UID = value
        End Set
    End Property
    ' D0309 ==

    ' D0598 ===
    Private Function isAllowMissingJudgments() As Boolean
        If IsReadOnly Then Return True Else Return (IsIntensities OrElse (App.ActiveProject.PipeParameters.AllowMissingJudgments AndAlso Not SurveyHasRequiredMissed)) ' D2333 + D7365
    End Function
    ' D0598 ==

    ' D0163 ===
    Property isAutoAdvance() As Boolean
        Get
            If _CanEdit_AutoAdvance Then    ' D0167 + D0194
                Dim val As String = GetCookie("AutoAdvance" + App.ProjectID.ToString, CStr(IIf(App.ActiveProject.PipeParameters.AllowAutoadvance, "1", ""))) ' D0503 + D0901
                Return val = "1"
            Else
                Return True   ' D0167
            End If
        End Get
        Set(value As Boolean)
            If _CanEdit_AutoAdvance Then    ' D0167 + D0194
                SetCookie("AutoAdvance" + App.ProjectID.ToString, CStr(IIf(value, "1", "0"))) ' D0503
            End If
        End Set
    End Property
    ' D0163 ==

    ' D2078 ===
    Public Property Autoadvance_JudgmentsCount As Integer
        Get
            Dim Cnt As Integer = 0
            If Not Integer.TryParse(GetCookie(COOKIE_CHECK_AUTOADVANCE + App.ProjectID.ToString, "0"), Cnt) Then Cnt = 0
            Return Cnt
        End Get
        Set(value As Integer)
            SetCookie(COOKIE_CHECK_AUTOADVANCE + App.ProjectID.ToString, CStr(value), True, False)
        End Set
    End Property
    ' D2078 ==

    ' D4012 ===
    Private Function CanApplyAutoAdvance(tAction As clsAction) As Boolean
        Return (AutoAdvance_Judgment_Types.Contains(tAction.ActionType) OrElse (tAction.ActionType = ActionType.atNonPWOneAtATime AndAlso CType(tAction.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType = ECMeasureType.mtRatings))
    End Function
    ' D4012 ==

    ' D4328 ===
    Public Function CanEditClusterPhrase() As Boolean
        Return CanUserEditClusterPhrase AndAlso Not IsReadOnly AndAlso Not IsIntensities AndAlso CanUserEditActiveProject
    End Function
    ' D4328 ==

    '' D0228 ===
    'Public Property isDataInstance() As Boolean
    '    Get
    '        If Not _DataInstanceStatusChecked And App.HasActiveProject() Then
    '            If Not Request.Params(_PARAM_DATAINSTANCE) Is Nothing Then _DataInstance = CheckVar(_PARAM_DATAINSTANCE, _DataInstance)
    '            If Not IsPM Then _DataInstance = False ' D0486 + D0835 + D4116
    '            If Not App.ActiveProject Is Nothing Then
    '                _DataInstanceStatusChecked = True
    '            End If
    '        End If
    '        Return _DataInstance
    '    End Get
    '    Set(value As Boolean)
    '        _DataInstance = value
    '    End Set
    'End Property
    '' D0228 ==

    ' D0229 ===
    Private ReadOnly Property HasTerminalButton() As Boolean
        Get
            Dim FHasTerminal As Boolean = (App.ActiveProject.PipeParameters.TerminalRedirectURL <> "" AndAlso App.ActiveProject.PipeParameters.RedirectAtTheEnd) OrElse
                                           App.ActiveProject.PipeParameters.LogOffAtTheEnd OrElse App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish OrElse
                                           (App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish AndAlso NextProject IsNot Nothing AndAlso Not IsRiskWithControls) OrElse IsIntensities   ' D1800 + D4160 + D4162 + D4163 + D4177
            If (App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE OrElse
                (JoinRiskionPipes AndAlso Not App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE AndAlso CanEditActiveProject)) AndAlso
                App.ActiveProject.IsRisk AndAlso Not IsIntensities AndAlso Not IsRiskWithControls Then   ' D3275 + D3278 + D3448 + D4103 + D7053
                If App.ActiveProject.isImpact Then FHasTerminal = Riskion_ShowRiskResultsScreen AndAlso HasPermission(_OPT_RISK_RESULTS_PGID, App.ActiveProject) Else FHasTerminal = True ' D3256 + D4715 + D6772
            End If
            Return FHasTerminal
        End Get
    End Property
    ' D0229 ==

    ' D0211 ===
    Private Sub SetUser(tUser As clsUser, fCheckDataInstance As Boolean, fRecreatePipe As Boolean) ' D0231 + D0274
        '' D0229 ===
        'If fCheckDataInstance And isDataInstance And App.ActiveProject.ProjectManager.DataInstances.Count > 0 Then    ' D0231
        '    Dim DI As clsDataInstance = App.ActiveProject.ProjectManager.DataInstances(0)
        '    tUser = DI.User
        'End If
        ' D0774 ===
        If Not tUser Is Nothing And (App.ActiveProject.ProjectManager.User Is Nothing Or (App.ActiveProject.ProjectManager.User IsNot Nothing AndAlso tUser.UserID <> App.ActiveProject.ProjectManager.User.UserID)) Then
            DebugInfo("Set Project Manager User")
            App.ActiveProject.ProjectManager.User = tUser
            App.ActiveProject.LastModify = Now
        End If
        ' D0774 ==

        Dim fForcePipeRebuild As Boolean = False ' D0522

        ' D0431 + D0433 ===
        'With App.ActiveProject.ProjectManager.PipeParameters
        '    If Not .ForceDefaultParameters AndAlso (.CurrentParameterSet IsNot .DefaultParameterSet) Then   ' D0805
        '        .CurrentParameterSet = .DefaultParameterSet
        '        App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False  ' D0452
        '        fForcePipeRebuild = True ' D0522
        '    End If
        'End With
        ' D0431 + D0433 ==

        ' D3603 ===
        If App.ActiveProject.HierarchyObjectives.HierarchyType = ECHierarchyType.htMeasure AndAlso Not IsIntensities Then
            App.ActiveProject.ProjectManager.ActiveHierarchy = If(App.ActiveProject.isImpact, ECHierarchyID.hidImpact, ECHierarchyID.hidLikelihood)
        End If
        ' D3603 ==

        ' D1676 ===
        With App.ActiveProject.ProjectManager
            If fRecreatePipe Then
                If .ActiveHierarchy = 2 AndAlso .PipeParameters.CurrentParameterSet IsNot .PipeParameters.ImpactParameterSet Then .PipeParameters.CurrentParameterSet = .PipeParameters.ImpactParameterSet
                If .ActiveHierarchy = 0 AndAlso .PipeParameters.CurrentParameterSet IsNot .PipeParameters.DefaultParameterSet Then .PipeParameters.CurrentParameterSet = .PipeParameters.DefaultParameterSet
            End If
        End With
        ' D1676 ==

        ' D0448 ===
        If Not App.ActiveProject.ProjectManager.User Is Nothing Then
            Dim fUpdateUser As Boolean = False
            Dim fUpdateWorkspace As Boolean = False
            Dim UserWS As clsWorkspace = App.ActiveWorkspace    ' D0486   App.Workspace(ProjectID, App.ActiveUser.UserID)
        End If
        ' D0448 ==

        If Not IsReadOnly AndAlso Not isCallback AndAlso Not IsPostBack Then App.CheckAndAssignUserRole(App.ActiveProject, App.ActiveUser.UserEmail) ' D1937
        If IsIntensities AndAlso App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet IsNot App.ActiveProject.ProjectManager.PipeParameters.MeasureParameterSet Then App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = App.ActiveProject.ProjectManager.PipeParameters.MeasureParameterSet ' D1958

        If App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated AndAlso
          ((App.ActiveProject.ProjectManager.PipeBuilder.PipeType <> ecPipeType.ptAnytime AndAlso Not IsRiskWithControls) OrElse
          (App.ActiveProject.ProjectManager.PipeBuilder.PipeType = ecPipeType.ptAnytime AndAlso IsRiskWithControls)) Then fForcePipeRebuild = True ' D2505

        If fRecreatePipe OrElse fForcePipeRebuild Then 'C0404 + D0522
            App.SurveysManager.ActiveWorkgroupID = App.ActiveWorkgroup.ID   ' D1127
            App.ActiveSurveysList = Nothing 'L0495
            If fForcePipeRebuild Then App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False ' D0522

            ' D2502 ===
            If IsRiskWithControls Then
                If fRecreatePipe Then App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False ' D4133
                Dim LJT As DateTime 'A1042
                App.ActiveProject.ProjectManager.PipeBuilder.CreatePipeForControls(LJT) 'A1042
            Else
                ' D2502 ==
                'If RISKION_JOINED_PIPE AndAlso App.isRiskEnabled AndAlso CanUserEditActiveProject AndAlso Not IsIntensities AndAlso Not IsReadOnly Then App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE = True ' D3253
                If JoinRiskionPipes AndAlso App.isRiskEnabled AndAlso Not IsIntensities Then ' D4103
                    ' D3273 ===
                    'If Not App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE AndAlso _
                    '   (Not App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated OrElse fRecreatePipe) AndAlso _
                    '   ((App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated OrElse fRecreatePipe) AndAlso (CurStep = 1 OrElse CurStep = App.ActiveProject.ProjectManager.Pipe.Count)) Then  ' D4123 + D4163

                    Dim S As Integer = WS.ProjectStep(App.ActiveProject.isImpact)
                    If App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated Then S = CurStep

                    If Not App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE AndAlso
                       (Not App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated OrElse fRecreatePipe) AndAlso
                       ((App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated OrElse fRecreatePipe) AndAlso
                       (Not App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE OrElse S <= 1 OrElse (App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated AndAlso S >= App.ActiveProject.ProjectManager.PipeBuilder.Pipe.Count))) Then  ' D4123 + D4163 + D4403 + D4458
                        Dim tOldHID As ECHierarchyID = CType(App.ActiveProject.ProjectManager.ActiveHierarchy, ECHierarchyID)
                        If App.ActiveProject.isImpact Then
                            App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                            App.ActiveProject.PipeParameters.CurrentParameterSet = App.ActiveProject.PipeParameters.DefaultParameterSet ' D4403
                        Else
                            App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                            App.ActiveProject.PipeParameters.CurrentParameterSet = App.ActiveProject.PipeParameters.ImpactParameterSet  ' D4403
                        End If
                        App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
                        App.ActiveProject.ProjectManager.PipeBuilder.CreatePipe()
                        Dim tHasEval As Boolean = False
                        For Each tmpAction As clsAction In App.ActiveProject.ProjectManager.Pipe
                            If isEvaluation(tmpAction) OrElse tmpAction.ActionType = ActionType.atSpyronSurvey OrElse tmpAction.ActionType = ActionType.atSurvey Then   ' D6378
                                tHasEval = True
                                Exit For
                            End If
                        Next
                        App.ActiveProject.ProjectManager.ActiveHierarchy = tOldHID
                        App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
                        App.ActiveProject.ProjectManager.PipeBuilder.ResetPipeCreatedForNodes() ' D4403
                        'App.ActiveProject.PipeParameters.CurrentParameterSet = IIf(tOldHID = ECHierarchyID.hidImpact, App.ActiveProject.PipeParameters.ImpactParameterSet, App.ActiveProject.PipeParameters.DefaultParameterSet)    ' D4403
                        If tHasEval Then App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE = True ' D3253 + D2358
                        App.ActiveProject.PipeParameters.CurrentParameterSet = If(App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact, App.ActiveProject.PipeParameters.ImpactParameterSet, App.ActiveProject.PipeParameters.DefaultParameterSet)    ' D4403
                    End If
                    ' D3273 ==
                End If
                App.ActiveProject.ProjectManager.PipeBuilder.CreatePipe() ' D0053 + D0231 + D0274
            End If

            If CheckVar(_PARAM_ACTION, "").ToLower = "add_step" Then
                Dim PID As Integer = -1
                Dim ID1 As Integer = -1
                Dim ID2 As Integer = -1
                If Integer.TryParse(CheckVar("pid", ""), PID) AndAlso Integer.TryParse(CheckVar("id1", ""), ID1) AndAlso Integer.TryParse(CheckVar("id2", ""), ID2) Then
                    Dim Data As clsShowLocalResultsActionData = CType(CurAction.ActionData, clsShowLocalResultsActionData)
                    Dim fIsPWOutcomes As Boolean = Data.PWOutcomesNode IsNot Nothing AndAlso Data.ParentNode.MeasureType = ECMeasureType.mtPWOutcomes

                    If fIsPWOutcomes Then
                        App.ActiveProject.ProjectManager.PipeBuilder.AddPairToPipePWOutcomes(Data.PWOutcomesNode, Data.ParentNode, ID1, ID2, CurStep - 1)
                    Else
                        App.ActiveProject.ProjectManager.PipeBuilder.AddPairToPipe(PID, ID1, ID2, CurStep - 1) 'C0966
                    End If
                    If IsIntensities AndAlso Not App.ActiveProject.PipeParameters.ObjectivesPairwiseOneAtATime Then CurStep = CurStep - 1
                    Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(True)), True)
                End If
            End If
        End If
        ' D0229 ==
    End Sub
    ' D0211 ==

    ' D0650 ===
    Public Function GetTimeoutMessage() As String
        Return JS_SafeString(String.Format(ResString("msgTimeoutWarning"), (SessionTimeout - _TIMEOUT_BEFORE) \ 60, _TIMEOUT_BEFORE \ 60))
    End Function
    ' D0650 ==

    ' D1864 ===
    Private Sub FramedInfodocs_ProcessCookies()
        Dim tParams As NameValueCollection = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
        For Each sName As String In _FRAMED_INFODOC_COOKIES
            sName = sName.ToLower
            Dim sVal As String = GetCookie(sName + ctrlFramedInfodoc._OPTION_SAVE_SIZE_SUFFIX, "")
            If sVal <> "" Then
                tParams(sName) = sVal
                SetCookie(sName + ctrlFramedInfodoc._OPTION_SAVE_SIZE_SUFFIX, "", True, False)  ' D3452
            End If
        Next
        Dim sValues As String = ctrlFramedInfodoc.List2String(tParams)
        If App.ActiveProject.PipeParameters.InfoDocSize <> sValues Then
            App.ActiveProject.PipeParameters.InfoDocSize = sValues
            App.ActiveProject.SaveProjectOptions("Save infodoc frame settings", , False)
        End If
    End Sub
    ' D1864 ==

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init    ' D0251
        'CurrentPageID = CInt(If(isDataInstance, _PGID_EVALUATE_DATAINSTANCES, _PGID_EVALUATION))    'D0228 + D0493
        If Not App.HasActiveProject OrElse Not App.isAuthorized Then FetchAccess(_PGID_PROJECTSLIST) ' D0507
        AlignHorizontalCenter = False
        AlignVerticalCenter = False
        CanUserEditActiveProject = CanEditActiveProject AndAlso App.CanUserModifyProject(If(IsReadOnly, ReadOnly_UserID, App.ActiveUser.UserID), App.ProjectID, UW, WS, App.ActiveWorkgroup)   ' D4233
        IsRealPM = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, UW, WS, App.ActiveWorkgroup)  ' D4233
        IsPM = IsRealPM AndAlso (CanUserEditActiveProject OrElse App.ActiveProject.ProjectStatus = ecProjectStatus.psMasterProject)  ' D3795 + D4232 + D4233 + D4560
        'If App.ActiveProject.isTeamTime Then Response.Redirect(PageURL(_PGID_EVALUATE_TEAMTIME, GetTempThemeURI(False)), True) ' D1286 + D2025
        ' D1953 ===
        If Not isSLTheme() AndAlso (App.ActiveProject.isImpact AndAlso App.ActiveProject.isTeamTimeImpact OrElse Not App.ActiveProject.isImpact AndAlso App.ActiveProject.isTeamTimeLikelihood) _
            AndAlso Not IsPostBack AndAlso Not isCallback AndAlso Not CheckVar("debug", False) Then Response.Redirect(PageURL(_PGID_TEAMTIME, GetTempThemeURI(False)), True) ' D1286 + D1953

        If App.isSpyronAvailable AndAlso App.ActiveProject.ProjectManager.PipeBuilder.GetSurveyInfo Is Nothing Then App.ActiveProject.ProjectManager.PipeBuilder.GetSurveyInfo = AddressOf App.SurveysManager.GetSurveyStepsCountByGUID 'C0460  + D7643

        ' D4233 ===
        If IsRealPM AndAlso CheckVar("view_mode", "") <> "" Then
            SessVar(SESSION_VIEW_MODE) = CStr(IIf(CheckVar("view_mode", "") = "1", 1, 0))
            'Dim sURL As String = Request.Url.OriginalString.Replace("view_mode=" + CheckVar("view_mode", ""), "").Replace("&&", "&").TrimEnd("&").TrimEnd("?") ' -D4236
            'Response.Redirect(sURL, True)  ' -D4236 due to form submit
        End If
        If CanUserEditActiveProject AndAlso SessVar(SESSION_VIEW_MODE) = "1" Then CanUserEditActiveProject = False
        ' D4233 ==

        ' -D2927
        'If App.ActiveProject.isTeamTimeLikelihood OrElse App.ActiveProject.isTeamTimeImpact Then
        '    App.ApplicationError.Init(ecErrorStatus.errMessage, CurrentPageID, ResString("msgTTSessionisActive"))
        '    Response.Redirect(PageURL(_PGID_SERVICEPAGE, String.Format("{0}=msg&type=err&pg={1}&{2}", _PARAM_ACTION, CInt(_PGID_ERROR_503), GetTempThemeURI(False))), True)
        'End If

        ' D2972 ===
        'If isSLTheme() AndAlso (App.ActiveProject.isTeamTimeLikelihood OrElse App.ActiveProject.isTeamTimeImpact OrElse Not App.isTeamTimeAvailable) Then
        If isSLTheme() AndAlso (App.ActiveProject.isTeamTimeLikelihood OrElse App.ActiveProject.isTeamTimeImpact) Then  ' D3619
            UpdatePanel()
            Exit Sub
            'ShowMessage(ResString("lblTeamTimeIsActive") + String.Format("<p align='center' style='margin-top:4em'><input type=button class='button' style='width:18em' value='{0}' onclick='window.open(""{1}"", ""TeamTime""); return false;'></p>", ResString("btnTeamTimeOpen"), PageURL(_PGID_EVALUATE_TEAMTIME, "?temptheme=tt")))   ' D2538
            'Dim fCanManage As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)    ' D2538
            'Dim isTTOwner As Boolean = fCanManage AndAlso App.ActiveProject.MeetingStatus(App.ActiveUser) = ECTeamTimeStatus.tsTeamTimeSessionOwner
            'If isTTOwner Then
            '    ClientScript.RegisterClientScriptBlock(GetType(String), "Navigate", "if ((window.parent) && (typeof (window.parent.NavigateToPage) == 'function')) window.parent.NavigateToPage(30502,'');", True)
            'End If
        End If
        ' D1953 + D2927 ==

        If IsRiskWithControls Then
            CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS ' D2502
            'RISKION_JOINED_PIPE = False     ' D3771 -D4103
        End If

        Dim tAHPUser As clsUser = App.ActiveProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEmail)   ' D0225
        ' D0309 ===
        If IsReadOnly Then
            Dim tUser As clsApplicationUser = App.DBUserByID(ReadOnly_UserID)   ' D0488
            If Not tUser Is Nothing AndAlso Not App.DBWorkspaceByUserIDProjectID(tUser.UserID, App.ProjectID) Is Nothing Then tAHPUser = App.ActiveProject.ProjectManager.GetUserByEMail(tUser.UserEmail) ' D0488
            If Not tAHPUser Is Nothing Then
                CurrentPageID = If(IsRiskWithControls, _PGID_EVALUATE_RISK_CONTROLS_READONLY, _PGID_EVALUATE_READONLY)  ' D7173
                _ReadOnly = True    ' D4285
            End If
        Else
            If CanUserEditActiveProject Then FramedInfodocs_ProcessCookies() ' D1864 + D4232
        End If
        ' D0309 ==

        If IsReadOnly AndAlso IsRiskWithControls Then CurrentPageID = _PGID_EVALUATE_RISK_CONTROLS_READONLY ' D4285

        ' D1800 ===
        If IsIntensities Then
            CurrentPageID = _PGID_EVALUATE_INTENSITIES
        End If
        If App.ActiveProject.ProjectStatus = ecProjectStatus.psMasterProject Then CheckPermissions() ' D2561
        ' D1800 ==
        ' D0230 ===
        If tAHPUser Is Nothing Then
            Dim tOldGrp As Integer = App.ActiveProject.ProjectManager.DefaultGroupID ' D2163
            If App.Options.UserRoleGroupID >= 0 Then App.ActiveProject.ProjectManager.DefaultGroupID = App.Options.UserRoleGroupID ' D2163
            ' D0232 ===
            tAHPUser = App.ActiveProject.ProjectManager.AddUser(App.ActiveUser.UserEmail, True, App.ActiveUser.UserName)
            App.ActiveProject.ProjectManager.DefaultGroupID = tOldGrp   ' D2163
            App.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
            'Project.ProjectManager.User = App.ActiveUser
            ' D0232 ==
        End If
        ' D0230 ==
        'If isDataInstance Then If _OriginalAHPUser Is Nothing Then _OriginalAHPUser = tAHPUser ' D0231

        ' D1416 ===
        Dim sHid As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("hid", ""))   'Anti-XSS
        Dim Hid As Integer = App.ActiveProject.ProjectManager.ActiveHierarchy
        If sHid <> "" AndAlso Integer.TryParse(sHid, Hid) AndAlso Hid >= 0 AndAlso App.ActiveProject.ProjectManager.ActiveHierarchy <> Hid Then App.ActiveProject.ProjectManager.ActiveHierarchy = Hid
        ' D1416 ==

        ' D3084 + D4009 ===
        If CheckVar("show_indiv", False) Then
            With App.ActiveProject.PipeParameters
                If .LocalResultsView = ResultsView.rvNone Then .LocalResultsView = ResultsView.rvIndividual Else .LocalResultsView = ResultsView.rvBoth
                App.ActiveProject.SaveProjectOptions("Change view mode for local results", , False)  ' D3089
                App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
            End With
        End If
        ' D3084 + D4009 ==

        Riskion_ShowRiskResultsScreen = Not HasEmbeddedContent(App.ActiveProject.ProjectManager.Parameters.EvalEmbeddedContent, EmbeddedContentType.RiskResults) AndAlso Not HasEmbeddedContent(App.ActiveProject.ProjectManager.Parameters.EvalEmbeddedContent, EmbeddedContentType.HeatMap)    ' D4715 + D6664
        SetUser(tAHPUser, True, Not IsPostBack AndAlso Not isCallback) ' D0211 + D0225 + D0231 + D0274 + D4137
        _CanEdit_AutoAdvance = Not App.ActiveProject.PipeParameters.AllowAutoadvance OrElse AlwaysShowAutoAdvance  ' D0167 + D0901
        CanUserEditClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.CanEditClusterPhrase(CurAction)  ' D4116 + D4241

        ' D0308 ===
        If IsReadOnly Then
            ' D1405 ===
            Dim sUser As String = App.ActiveProject.ProjectManager.User.UserEMail
            If App.ActiveProject.ProjectManager.User.UserName.ToLower <> sUser.ToLower Then sUser = String.Format("&lt;{1}&gt;, {0}", ShortString(App.ActiveProject.ProjectManager.User.UserName, 20), HTMLEmailLink(sUser, sUser)) ' D1407
            lblReadOnly.InnerHtml = String.Format("{0}&nbsp; <span class='small' style='font-weight:normal;white-space:nowrap;'>({1})</span>{2}", ResString("lblReadOnlyMode"), sUser, IIf(isSLTheme, String.Format("<span id='lblReadOnlyCloseTab' style='margin-left:1em; display:none'>{0}</span>", ResString("lblReadOnlyModeClose")), "")) ' D0801 + D2882
            If isSLTheme() Then ClientScript.RegisterStartupScript(GetType(String), "InitCloseMsg", "CheckParentAndShowClose();", True) ' D2822
            ' D1405 ==
            lblReadOnly.Visible = True  '  D0801
            rowEdit.Visible = True      ' D4945
        Else
            rowEdit.Visible = False  ' D4040
        End If
        ' D0308 ==

        ' D3029 ===
        Dim fCanEvaluate As Boolean = True
        If Not IsReadOnly AndAlso CheckVar(_PARAM_ACTION, "") = "getstep" Then
            Dim sMsg As String = ""
            Dim MType As Integer = CheckVar("mt_type", -1)
            Dim nGUID As Guid = Guid.Empty
            Try
                nGUID = New Guid(CheckVar("node_id", ""))
            Catch ex As Exception
            End Try
            ' D4143 ===
            If Not nGUID.Equals(Guid.Empty) AndAlso (MType = -1) Then
                Dim aGUID As Guid = Guid.Empty
                Dim cGUID As Guid = Guid.Empty
                Try
                    Dim paramAltID As String = CheckVar("alt_id", "") 'A1265
                    If Not String.IsNullOrEmpty(paramAltID) Then aGUID = New Guid(paramAltID) 'A1265
                    Dim paramCtrlID As String = CheckVar("control_id", "") 'A1265
                    If Not String.IsNullOrEmpty(paramCtrlID) Then cGUID = New Guid(paramCtrlID) 'A1265
                Catch ex As Exception
                End Try
                If Not nGUID.Equals(Guid.Empty) AndAlso Not cGUID.Equals(Guid.Empty) Then
                    Dim tStep As Integer = -1
                    For i As Integer = 0 To App.ActiveProject.Pipe.Count - 1
                        Dim tmpAction As clsAction = App.ActiveProject.Pipe(i)
                        If tmpAction.isEvaluation Then
                            Dim ParentNode As clsNode = Nothing
                            Dim FirstNode As clsNode = Nothing
                            Dim SecondNode As clsNode = Nothing
                            App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNodes(tmpAction, ParentNode, FirstNode, SecondNode)
                            If ParentNode IsNot Nothing AndAlso ParentNode.NodeGuidID.Equals(nGUID) AndAlso SecondNode IsNot Nothing AndAlso SecondNode.NodeGuidID.Equals(cGUID) Then
                                If FirstNode Is Nothing OrElse aGUID.Equals(Guid.Empty) OrElse (FirstNode IsNot Nothing AndAlso FirstNode.NodeGuidID.Equals(aGUID)) Then
                                    tStep = i
                                    Exit For
                                End If
                            End If
                        End If
                    Next
                    If tStep >= 0 Then
                        Dim OldStep As Integer = CurStep
                        CurStep = tStep + 1
                        SetCookie("askopen", App.ProjectID.ToString)
                        Response.Redirect(PageURL(CurrentPageID, String.Format("ret={1}&retpage={2}{0}", GetTempThemeURI(True), OldStep, CheckVar("retpage", ""))), True)
                    Else
                        sMsg = ResString("msgEvalStepCantFindRiskStep")
                        fCanEvaluate = False
                    End If
                End If
            End If
            ' D4143 ==
            If Not nGUID.Equals(Guid.Empty) AndAlso (MType = 0 OrElse MType = 1) Then
                Dim tNode As clsNode = Nothing
                'If MType = 0 Then tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(nGUID) Else tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(nGUID)
                tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(nGUID)
                If tNode Is Nothing Then tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(nGUID) ' D3378
                Dim tStep As Integer = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(tNode, -1)
                If tNode Is Nothing Then
                    sMsg = ResString("msgEvalStepCantFindNode")
                    fCanEvaluate = False
                Else
                    If tStep < 0 Then
                        sMsg = ResString("msgEvalStepCantFindStep")
                        fCanEvaluate = False
                    Else
                        CurStep = tStep + 1
                        Response.Redirect(PageURL(CurrentPageID, String.Format("oid={0}&aid={0}{1}", tNode.NodeID, GetTempThemeURI(True))), True) ' D3378
                    End If
                End If
                'If sMsg <> "" AndAlso Not fCanEvaluate Then ShowMessage(String.Format(sMsg, ParseAllTemplates(CStr(IIf(MType = 0, _TEMPL_OBJECTIVE, _TEMPL_ALTERNATIVE)), App.ActiveUser, App.ActiveProject)))
            End If
            If sMsg <> "" AndAlso Not fCanEvaluate Then ShowMessage(String.Format(sMsg, ParseAllTemplates(_TEMPL_OBJECTIVE, App.ActiveUser, App.ActiveProject)))
        End If
        ' D3029 ==

        ' D7365 ===
        If isAllowMissingJudgments() Then
            For i As Integer = 0 To App.ActiveProject.Pipe.Count - 1
                Dim tAction As clsAction = App.ActiveProject.Pipe(i)
                If tAction.ActionType = ActionType.atSpyronSurvey Then
                    If isSurveyStepRequired(tAction) Then
                        SurveyHasRequiredMissed = True
                        Exit For
                    End If
                End If
            Next
        End If
        ' D7365 ==

        CurStep = CheckVar(_PARAM_STEP, CheckVar("pipe", CurStep)) ' D0108

        ' D2825 ===
        'If IsReadOnly AndAlso Not IsPostBack AndAlso Not IsCallback Then
        If Not IsPostBack AndAlso Not isCallback Then   ' D3670 // for check ste when logged by user, not only view mode
            Dim sMode As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("mode", "")).Trim.ToLower    'Anti-XSS
            Dim sNodeGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("node", "")).Trim    'Anti-XSS
            If sMode = "searchresults" AndAlso sNodeGUID <> "" Then
                Dim idx As Integer = 0
                Dim fHasStep As Boolean = False
                For Each tAction As clsAction In App.ActiveProject.Pipe
                    idx += 1
                    If tAction.ActionType = ActionType.atShowLocalResults Then
                        Dim Data As clsShowLocalResultsActionData = CType(tAction.ActionData, clsShowLocalResultsActionData)
                        If Data.ParentNode IsNot Nothing AndAlso Data.ParentNode.NodeGuidID.ToString = sNodeGUID Then
                            CurStep = idx
                            fHasStep = True
                            Exit For
                        End If
                    End If
                    If tAction.ActionType = ActionType.atShowGlobalResults Then
                        If Not fHasStep Then CurStep = idx
                        Dim Data As clsShowGlobalResultsActionData = CType(tAction.ActionData, clsShowGlobalResultsActionData)
                        If Data.WRTNode IsNot Nothing AndAlso Data.WRTNode.NodeGuidID.ToString = sNodeGUID Then
                            fHasStep = True
                            Exit For
                        End If
                    End If
                Next

                If Not fHasStep Then
                    Dim G As Guid
                    If Guid.TryParse(sNodeGUID, G) Then
                        Dim tNode As clsNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(G)
                        Dim sNodeName As String = sNodeGUID
                        If tNode IsNot Nothing Then sNodeName = GetWRTNodeNameWithPath(tNode, False)
                        ClientScript.RegisterStartupScript(GetType(String), "ShowMsg", String.Format("setTimeout(""CantFindStep('{0}');"", 200);", JS_SafeString(sNodeName)), True)
                    End If
                End If
            End If
            ctrlFramedInfodoc.HideFrameCaption = App.ActiveProject.ProjectManager.Parameters.EvalHideInfodocCaptions   ' D4713
        End If
        ' D2825 ==

        ' D0467 ===
        Dim CurStepType As ActionType = ActionType.atNone
        Dim tCurAction As clsAction = CurAction
        If Not tCurAction Is Nothing Then CurStepType = tCurAction.ActionType() ' D0251
        ' D0467 ==
        If Not IsPostBack Or CurStepType = ActionType.atSensitivityAnalysis Or CurStepType = ActionType.atShowGlobalResults Or CurStepType = ActionType.atShowLocalResults Or CurStepType = ActionType.atSpyronSurvey Then  ' D0155 + D0157 + D0251
            btnNext.Text = ResString("btnEvaluationNext")
            btnPrev.Text = ResString("btnEvaluationPrev")
            btnFirst.Text = ResString("btnEvaluationStart")
            btnNextUnas.Text = ResString("btnEvaluationNextUnas")
            btnSave.Text = ResString("btnSave") ' D2154
            If fCanEvaluate Then UpdatePanel() ' D3029
            imgWhereAmI.AlternateText = ResString("lblWhereAmI")    ' D0146
        End If

        ' D4329 ===
        If SaveCustomText(False, "SaveNewTask", "TaskNodeGUID", "txtTask", "TaskNodesList") OrElse SaveCustomText(True, "SaveNewTitle", "TitleNodeGUID", "txtTitle", "TitleNodesList") Then
            Dim sChanged As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(isChanged.UniqueID, isChanged.Value)) 'Anti-XSS
            If (sChanged = "" OrElse sChanged = "0") Then Response.Redirect(Request.Url.AbsoluteUri, True) ' D2712 + D6767
        End If
        ' D4329 ==

        'If Not IsPostBack Then
        '    SessVar("DI" + CStr(App.ProjectID)) = CStr(IIf(CurrentPageID = _PGID_EVALUATE_DATAINSTANCES, 1, 0))    ' D0672
        'End If

        ' D0650 + D0822 ===
        If Not IsPostBack AndAlso Not isCallback Then
            ' D0822 ==
            If Not IsReadOnly AndAlso SessionTimeout >= (_TIMEOUT_BEFORE + 60) AndAlso CurrentStepBtnID <> "" Then ClientScript.RegisterStartupScript(GetType(String), "InitTimeout", String.Format("setTimeout('ShowTimeoutWarning();', {0});", 1000 * (SessionTimeout - _TIMEOUT_BEFORE)), True) ' D1730 + D4277

            ' D3377 ===
            If App.isRiskEnabled AndAlso App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE AndAlso App.Options.isSingleModeEvaluation AndAlso Not App.ActiveProject.isImpact Then
                Dim fHasEvals As Boolean = False
                For Each tAction In App.ActiveProject.ProjectManager.Pipe
                    If isEvaluation(tAction) OrElse tAction.ActionType = ActionType.atSpyronSurvey OrElse tAction.ActionType = ActionType.atSurvey Then   ' D6378
                        fHasEvals = True
                        Exit For
                    End If
                Next
                If Not fHasEvals Then
                    App.Options.SingleModeProjectPasscode = App.ActiveProject.PasscodeImpact
                    Response.Redirect(PageURL(CurrentPageID, _PARAMS_PASSCODE(0) + "=" + App.Options.SingleModeProjectPasscode + GetTempThemeURI(True)), True)
                End If
            End If
            ' D3377 ==

            ' D1017 + D1698 - D2309 ===
            'Dim sShowWait As String = ""
            'Dim sAct As clsAction = CurAction
            'If sAct IsNot Nothing AndAlso Not (sAct.ActionType = ActionType.atShowGlobalResults Or sAct.ActionType = ActionType.atShowLocalResults) Then sShowWait = "show_wait=1;" ' -D1651 - D2308
            'If sAct IsNot Nothing AndAlso sAct.ActionType <> ActionType.atShowLocalResults Then sShowWait = "show_wait=1;" ' D1651 + D2308
            'ClientScript.RegisterOnSubmitStatement(GetType(String), "OnPostData", "alert_on_exit=0; " + sShowWait)   ' D0654
            ClientScript.RegisterOnSubmitStatement(GetType(String), "OnPostData", "alert_on_exit=0;")   ' D0654
            ' D0117 + D1698 + D2309 ==
        End If
        ' D0650 ==

        If Not IsPostBack AndAlso Not isCallback Then   ' D1312
            If App.HasActiveProject Then
                InconsistencySortingEnabled = True
            End If

            '' D0872 ===
            'pnlLoadingNextStep.Caption = ResString("msgLoading")
            'pnlLoadingNextStep.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")
            '' D0872 ==

            ' A0469 ===
            ' D1025 + D3693 ===
            'If isSLTheme() Then    ' -D1304

            Dim tAction As clsAction = CurAction
            If tAction IsNot Nothing AndAlso App.ActiveProject IsNot Nothing Then HelpID = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionStepType(tAction) ' D3730 + D4079

            ' D3695 ===
            If HelpID <> ecEvaluationStepType.Other Then
                QuickHelpParams = String.Format("&qh={0}&step={1}", CInt(HelpID), CurStep)  ' D3699
                Dim tNodeID As Integer = 0  ' D3697
                ' D4116 ===
                If CurStepNode IsNot Nothing AndAlso (isEvaluation(tAction) OrElse HelpID = ecEvaluationStepType.IntermediateResults) Then
                    tNodeID = CurStepNode.NodeID  ' D3697
                    If CurStepNode.IsAlternative Then tNodeID = -tNodeID ' D3697
                    QuickHelpParams += String.Format("&guid={0}", CurStepNode.NodeGuidID)
                    ' D4116 ==
                End If
                Dim isCluster As Boolean = False    ' D4082
                QuickHelpContent = App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, CurStep, isCluster, AutoShowQuickHelp).Trim ' D3696 + D3699 + D3720 + D3941 + D4079 + D4081 + D4082
                ShowQuickHelp = clsPipeMessages.OPT_QUICK_HELP_AVAILABLE AndAlso ((IsRealPM AndAlso CanUserEditActiveProject AndAlso Not IsIntensities) OrElse QuickHelpContent <> "")  ' D3696 + D3720 + D4077 + D4287 + D6975
            End If
            ' D1025 + D3695 ==

            ' D1310 + D3716 + D3727 ===
            If Not IsPostBack AndAlso Not isCallback AndAlso Not IsReadOnly Then
                Dim fShowQH As Boolean = False
                ' D3720 ===
                Dim isForward As Boolean = True
                If Session(_SESS_PREV_STEP + If(App.ActiveProject.isImpact, "_i_", "") + App.ProjectID.ToString) IsNot Nothing Then ' D6676
                    isForward = CInt(Session(_SESS_PREV_STEP + If(App.ActiveProject.isImpact, "_i_", "") + App.ProjectID.ToString)) < CurStep + If(clsPipeMessages.OPT_QUICK_HELP_AUTO_SHOW_ONCE, 0, 1)  ' D6676 + D7556
                End If
                ' D7604 ===
                If Not isForward AndAlso Session(_SESS_EARLIER_STEP + If(App.ActiveProject.isImpact, "_i_", "") + App.ProjectID.ToString) IsNot Nothing Then
                    If CInt(Session(_SESS_EARLIER_STEP + If(App.ActiveProject.isImpact, "_i_", "") + App.ProjectID.ToString)) > CurStep Then isForward = True   ' Looks like move forward but for cases when we try to see the step, that didn't visited earlier
                End If
                ' D7604 ==

                ' D3738 ===
                If clsPipeMessages.OPT_QUICK_HELP_AVAILABLE AndAlso AutoShowQuickHelp AndAlso GetCookie(_COOKIE_QH_AUTOSHOW + App.ProjectID.ToString, "") <> "0" AndAlso QuickHelpContent <> "" AndAlso isForward Then ' D3740 + D3741
                    fShowQH = CheckShow_QuickHelp(QuickHelpContent)
                    If fShowQH Then ClientScript.RegisterStartupScript(GetType(String), "PWHelp", String.Format("setTimeout('{0}', {1});", "ShowIdleQuickHelp();", 500), True)
                    ' D3738 ==
                End If
                If CurrentPageID = _PGID_EVALUATION AndAlso Not App.isRiskEnabled AndAlso isEvaluation(tAction) AndAlso GetCookie(_COOKIE_NOVICE_IDLEHELP, "") = "" AndAlso Not fShowQH Then ' D6715
                    'ClientScript.RegisterStartupScript(GetType(String), "PWHelp", String.Format("setTimeout('{0}', {1});", IIf(QuickHelpContent = "", "ShowIdleHelp();", "ShowIdleQuickHelp();"), _TIMEOUT_IDLE_PW * 1000), True)
                    ClientScript.RegisterStartupScript(GetType(String), "PWHelp", String.Format("setTimeout('{0}', {1});", "ShowIdleHelp();", _TIMEOUT_IDLE_PW * 1000), True)
                End If
            End If
            ' D1310 + D3716 + D3727 ==

            ' D1304 + D3701 ===
            Dim _HelpID As Integer = CInt(HelpID)
            Select Case HelpID
                Case ecEvaluationStepType.MultiDirectInput
                    _HelpID = CInt(ecEvaluationStepType.DirectInput)
                Case ecEvaluationStepType.MultiGraphicalPW
                    _HelpID = CInt(ecEvaluationStepType.GraphicalPW)
                Case ecEvaluationStepType.MultiRatings
                    _HelpID = CInt(ecEvaluationStepType.Ratings)
                Case ecEvaluationStepType.MultiVerbalPW
                    _HelpID = CInt(ecEvaluationStepType.VerbalPW)
            End Select

            ' -D6256
            'If isSLTheme() Then
            '    ClientScript.RegisterStartupScript(GetType(String), "InitHelpID", "if ((window.parent)) { window.parent.pipe_help_id='" + _HelpID.ToString + "'; } " + vbCrLf, True)
            'Else
            '    ClientScript.RegisterStartupScript(GetType(String), "InitHelpID", "sl_help='" + JS_SafeString(GetEvaluationHelpPageName(_HelpID, isLikelihood)) + "';", True)    ' D2010
            'End If
            ' D1304 + D3701 ==
            ' A0469 ==
        End If  ' D1312

        ' D3227 ===
        If GetCookie(COOKIE_CHROME_PM, "") = "" AndAlso App.Options.isSingleModeEvaluation AndAlso Not isSLTheme() AndAlso SessVar(_SESS_FORCE_PIPE) <> "1" AndAlso App.ActiveProjectRoleGroup IsNot Nothing AndAlso
           Request IsNot Nothing AndAlso (Request.UserAgent.ToLower.Contains("chrome") OrElse Request.UserAgent.ToLower.Contains("firefox")) AndAlso Request.UserAgent.ToLower.Contains("windows") AndAlso Not Request.UserAgent.ToLower.Contains("edge/") AndAlso
           (App.ActiveProjectRoleGroup.ActionStatus(ecActionType.at_mlViewModel) = ecActionStatus.asGranted OrElse IsPM) Then   ' D4116 + D4404
            ClientScript.RegisterStartupScript(GetType(String), "Chrome", String.Format("dxDialog('{0}', false, ';', '', undefined, 500);", JS_SafeString(ResString("msgChromeSL"))), True)
            SetCookie(COOKIE_CHROME_PM, "1", False, False)
        End If
        ' D3227 ==

        ' D3073 ===
        If CheckVar(_PARAM_ACTION, "") = "stepslist" Then
            RawResponseStart()

            Dim sContent As String = ""
            Dim s As Integer = CheckVar("from", 1)
            If s < 1 Then s = 1
            Dim f As Integer = s + _OPT_STEPS_PAGE_SIZE
            If f > App.ActiveProject.Pipe.Count Then f = App.ActiveProject.Pipe.Count
            Dim NU As Integer = GetNextUnassessed(1)
            For i As Integer = s To f

                Dim St As Integer = 0   ' 0 = evaluated, 1 = undefinde, 2: non-evaluation, 3: disabled
                Dim Action As clsAction = GetAction(i)
                If isEvaluation(Action) Then
                    If isUndefined(Action) Then St = 1 Else St = 0
                Else
                    St = 2
                End If
                '' D7365 ===
                'Dim isSurveyReq = False
                'If Action.ActionType = ActionType.atSpyronSurvey Then
                '    isSurveyReq = isSurveyStepRequired(Action)
                '    If isSurveyReq AndAlso NU > i Then NU = i
                'End If
                'If (Not isAllowMissingJudgments() OrElse isSurveyReq) AndAlso Not IsReadOnly AndAlso NU >= 0 AndAlso i > NU Then St = 3
                '' D7365 ==
                If Not isAllowMissingJudgments() AndAlso Not IsReadOnly AndAlso NU >= 0 AndAlso i > NU Then St = 3

                Dim sName As String = String.Format(ResString("btnEvaluationStepHint"), i, GetPipeStepHint(Action, IntensityScale))
                sName = HTML2Text(sName).Replace(vbLf, " ").Replace(vbCr, "").Replace("  ", " ").Trim   ' D3567
                Dim sName_ As String = ShortString(sName, 140 - ID.ToString.Length, True)   ' D3567
                sContent += String.Format("{0}[{1},{2},'{3}','{4}']", IIf(sContent = "", "", ","), i, St, JS_SafeString(sName_.Replace("'", "&#39;")), IIf(sName = sName_, "", JS_SafeString(sName.Replace("'", "&#39;"))))  ' D3567
            Next

            sContent = "[" + sContent.Replace("""", "&quot;") + "]"

            Response.ContentType = "text/plain"
            'Dim utf16 As New UTF8Encoding()
            'Dim byteCount As Integer = utf16.GetByteCount(sContent)
            'Response.AddHeader("Content-Length", byteCount.ToString)
            Response.Write(sContent)
            Response.End()
            'Response.Flush()

            'RawResponseEnd()
        End If
        ' D3073 ==

        ' D6373 ===
        If IsReadOnly AndAlso isEvaluation(CurAction) Then
            ClientScript.RegisterOnSubmitStatement(GetType(String), "beforeSubmit", "return checkROForm();")
        End If
        ' D6373 ==

        'ClientScript.RegisterStartupScript(GetType(String), "ShowContent", "ShowContent();", True) ' - D0679
    End Sub

    ' D4329 ===
    Private Function SaveCustomText(fIsTitle As Boolean, sSaveFlagName As String, sGUIDName As String, sTextName As String, sNodesName As String) As Boolean
        Dim fResult As Boolean = False
        ' D2094 ===
        If IsPostBack AndAlso CheckVar(sSaveFlagName, False) AndAlso CanUserEditActiveProject AndAlso Not IsReadOnly AndAlso CanUserEditClusterPhrase Then  ' D4053 + D4116 + D4232
            Try
                Dim AGuid As New Guid(CheckVar(sGUIDName, ""))
                Dim sTask As String = CheckVar(sTextName, "")   ' D4352
                ' D4063 ===
                sTask = HTML2TextWithSafeTags(sTask).Trim
                '<p><font style="background-color: rgb(240, 240, 240);">
                Dim sTaskLower As String = sTask.ToLower
                If sTaskLower.StartsWith("<p>") Then
                    sTask = sTask.Substring(3)
                    If sTaskLower.EndsWith("</p>") Then sTask = sTask.Substring(0, sTask.Length - 4)
                End If
                sTaskLower = sTask.ToLower
                Dim sFont As String = "<font style=""background-color: rgb(240, 240, 240);"">".ToLower
                If sTaskLower.StartsWith(sFont) Then
                    sTask = sTask.Substring(sFont.Length)
                    If sTaskLower.EndsWith("</font>") Then sTask = sTask.Substring(0, sTask.Length - 7)
                End If
                ' D4063 ==
                ' D4148 ===
                If sTask <> "" AndAlso sTask.Contains("%%") Then
                    Dim tList As Dictionary(Of String, String) = GetUserTemplateReplacements(False, CurStepNode)
                    If tList IsNot Nothing Then
                        For Each sName As String In tList.Keys
                            sTask = ParseTemplate(sTask, sName, tList(sName))
                        Next
                    End If
                End If
                ' D4148 ==
                ' D2651 ===
                Dim tGuids As New List(Of Guid)
                Dim GuidsList As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(sNodesName, "")).Trim   'Anti-XSS
                If GuidsList <> "" Then
                    Dim sList As String() = GuidsList.Split(CType(",", Char()))
                    If sList.Count > 0 Then
                        For Each sGuid As String In sList
                            Try
                                Dim G As New Guid(sGuid)
                                If tGuids.IndexOf(G) < 0 Then tGuids.Add(G)
                            Catch ex As Exception
                            End Try
                        Next
                    End If
                End If
                If tGuids.IndexOf(AGuid) < 0 Then tGuids.Add(AGuid)
                ' D2751 ===
                Dim isMulti As Boolean = False
                Select Case CurAction.ActionType
                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                        isMulti = True
                        ' D4053 ===
                    Case ActionType.atShowLocalResults
                        isMulti = True
                        ' D4053 ==
                End Select
                ' D2751 ==
                For Each tGuid As Guid In tGuids
                    ' D4053 ===
                    Dim tAGuid As Guid = Guid.Empty
                    Select Case CurAction.ActionType
                        Case ActionType.atShowLocalResults, ActionType.atShowGlobalResults, ActionType.atSensitivityAnalysis
                            tAGuid = tGuid
                    End Select
                    If fIsTitle Then
                        ' D7677 ===
                        Dim tAdditionalGuid As Guid = Guid.Empty
                        If IsIntensities AndAlso IntensityScale IsNot Nothing Then tAdditionalGuid = IntensityScale.GuidID
                        If App.ActiveProject.ProjectManager.PipeBuilder.SetClusterTitleForNode(tGuid, sTask, tAdditionalGuid) Then fResult = True
                    Else
                        If App.ActiveProject.ProjectManager.PipeBuilder.SetClusterPhraseForNode(tGuid, sTask, isMulti, tAGuid, IsRiskWithControls) Then fResult = True ' D2751
                        ' D7677 ==
                    End If
                    ' D4053 ==
                Next
                ' D2651 ==
                ' D3912 ===
                If fResult Then
                    Dim sComment As String = ""
                    Dim tNode As clsNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(AGuid)
                    If tNode Is Nothing Then tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(AGuid)
                    If tNode IsNot Nothing Then
                        sComment = GetNodeTypeAndName(tNode)
                        If isMulti AndAlso tGuids.Count > 1 Then sComment += String.Format(" (+{0} more)", tGuids.Count - 1)
                    End If
                    App.SaveProjectLogEvent(App.ActiveProject, If(fIsTitle, "Update custom title for results", "Update custom cluster phrase"), False, sComment)
                End If
                ' D3912 ==
            Catch ex As Exception
            End Try
        End If
        ' D2094 ==
        Return fResult
    End Function
    ' D4329 ==

    Dim mFirstRun As Boolean
    Protected Sub Page_PreRenderComplete(sender As Object, e As EventArgs) Handles Me.PreRenderComplete
        ' D1538 ===
        If Not isCallback AndAlso Not IsPostBack AndAlso App.HasActiveProject Then ' D7577
            Dim sInitScript As String = ""
            If App.ActiveProject.Pipe.Count = CurStep AndAlso Not isEvaluation(CurAction) AndAlso btnNextUnas.Visible AndAlso btnNextUnas.Enabled Then
                sInitScript += "setTimeout('ShowNotCompleted();', 250); " ' D1731 + D3253
                ' D3722 ===
            Else
                Dim Unassessed As Integer = GetNextUnassessed(1)
                If Not IsReadOnly AndAlso Not IsIntensities AndAlso GetCookie("askopen", "") <> App.ProjectID.ToString AndAlso CurStep > 1 AndAlso Unassessed > 0 AndAlso Unassessed <> CurStep AndAlso CheckVar(_PARAM_ACTION, "") <> "getstep" AndAlso CheckVar("mode", "") = "" Then  ' D4143 + D6362
                    sInitScript += String.Format("ask_step_shown = true; setTimeout('AskOnOpen({0});', 250);", Unassessed)  ' D3730
                End If
            End If
            ' D3795 ===
            If USD_OptionVisible Then
                If USD_CostOfGoal <= 0 Then
                    USD_ShowCostOfGoal = False
                    If Not IsPM Then USD_OptionVisible = False
                End If
                If USD_OptionVisible Then sInitScript += "InitUSD();"
            End If
            ' D3795 ==
            SetCookie("askopen", App.ProjectID.ToString, True, False)
            ' D3722 ==
            ' D7370 ===
            Dim isSurvey As Boolean = False
            Dim tAction As clsAction = GetAction(CurStep)
            If tAction IsNot Nothing AndAlso tAction.ActionType = ActionType.atSpyronSurvey Then isSurvey = True    ' D7365
            ' D7370 ==
            If btnNext.Visible AndAlso Not btnNext.Enabled Then
                btnNext.Enabled = True
                If Not isSurvey Then sInitScript += String.Format("theForm.{0}.disabled=1;", btnNext.ClientID)  ' D7365
            End If
            If btnNextUnas.Visible AndAlso Not btnNextUnas.Enabled Then
                btnNextUnas.Enabled = True
                If Not isSurvey Then sInitScript += String.Format("theForm.{0}.disabled=1;", btnNextUnas.ClientID)   ' D7365
            End If
            If sInitScript <> "" Then ScriptManager.RegisterStartupScript(Me, GetType(String), "InitNextButtons", sInitScript, True)
            ' D7604 ===
            Dim tEStep As Integer = CurStep
            Dim sEarlier As Object = Session(_SESS_EARLIER_STEP + If(App.ActiveProject.isImpact, "_i_", "") + App.ProjectID.ToString)
            If sEarlier IsNot Nothing Then
                If CInt(sEarlier) < tEStep Then tEStep = CInt(sEarlier)
            End If
            Session(_SESS_EARLIER_STEP + If(App.ActiveProject.isImpact, "_i_", "") + App.ProjectID.ToString) = tEStep
            ' D7604 ==
            Session(_SESS_PREV_STEP + If(App.ActiveProject.isImpact, "_i_", "") + App.ProjectID.ToString) = CurStep ' D6676
        End If
        ' D1538 ==
    End Sub

    ' D4264 ===
    Private Sub RestoreIntensitiesHID()
        ' D1800 ===
        Dim tHID As Integer = -1
        If _IntensitiesStatusChecked AndAlso IsIntensities AndAlso Integer.TryParse(SessVar(SESSION_ORIGINAL_HID), tHID) Then ' D1965 + D2672
            If App.ActiveProject.ProjectManager.IsValidHierarchyID(tHID) Then
                App.ActiveProject.ProjectManager.ActiveHierarchy = tHID
                'App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
            End If
            If IsIntensities AndAlso App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet IsNot App.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet Then App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = App.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet ' D1958 + D2672
        End If
        ' D1800 ==
    End Sub
    ' D4264 ==

    ' D0231 ===
    Protected Sub Page_Unload(sender As Object, e As EventArgs) Handles Me.Unload
        If App.HasActiveProject Then    ' D7577
            If Not _OriginalAHPUser Is Nothing Then SetUser(_OriginalAHPUser, False, False) ' D0274
            RestoreIntensitiesHID()   ' D4264
        End If
    End Sub
    ' D0231 ==

    Protected Sub btnStep_Click(sender As Object, e As EventArgs) Handles btnNext.Click, btnPrev.Click, btnNextUnas.Click, btnJump.Click, btnSave.Click, btnFirst.Click  ' D1170 + D2154 + D6684
        If Not App.HasActiveProject Then Exit Sub ' D4382
        If Not TypeOf (sender) Is Button Then
            If isCallback Then Exit Sub
            Response.Redirect(RemoveXssFromUrl(Request.RawUrl), True) ' D0204 + D6766
        End If
        Dim btn As Button = CType(sender, Button)

        ' D1840 ===
        If CanUserEditActiveProject AndAlso CheckVar(_PARAM_ACTION, "").ToLower = "infodoc_mode" Then   ' D4232
            App.ActiveProject.PipeParameters.ShowInfoDocsMode = If(App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame, ShowInfoDocsMode.sidmPopup, ShowInfoDocsMode.sidmFrame)
            App.ActiveProject.SaveProjectOptions("Switch infodoc mode", , False)
        End If
        ' D1840 ==

        ' D0485 ===
        Dim tStep As Integer = CheckVar("curstepid", CurStep)
        If tStep < 1 Then tStep = 1
        If tStep > App.ActiveProject.Pipe.Count Then tStep = App.ActiveProject.Pipe.Count
        Dim tAction As clsAction = GetAction(tStep)
        ' D0485 ==

        ' D0253 ===
        'If SurveySpyron.Visible And CurAction.ActionType = ActionType.atSpyronSurvey And (btn Is btnNext Or btn Is btnPrev) Then
        If tAction IsNot Nothing AndAlso SurveySpyron.Visible AndAlso tAction.ActionType = ActionType.atSpyronSurvey Then  ' D0428 + D0485 + D0983 // allow postback from any button, not from only prev/next
            Dim Data As clsSpyronSurveyAction = CType(GetAction(tStep).ActionData, clsSpyronSurveyAction)   ' D0500

            SurveySpyron.RespondentEMail = App.ActiveUser.UserEmail 'L0454
            'L0492 ===
            If IsReadOnly Then
                Dim tUser As clsApplicationUser = App.DBUserByID(ReadOnly_UserID)
                If tUser IsNot Nothing Then
                    SurveySpyron.RespondentEMail = tUser.UserEmail
                End If
            End If
            Dim SurveyLoadEmail As String = SurveySpyron.RespondentEMail
            If IsReadOnly Then SurveyLoadEmail = "" 'if readonly - load all participants answers
            'L0492 ==
            Dim UsersList As New Dictionary(Of String, clsComparionUser)
            'UsersList.Add(App.ActiveUser.UserEmail, New clsComparionUser() With {.ID = App.ActiveUser.UserID, .UserName = App.ActiveUser.UserName})
            UsersList.Add(App.ActiveUser.UserEmail, New clsComparionUser() With {.ID = App.ActiveProject.ProjectManager.UserID, .UserName = App.ActiveProject.ProjectManager.User.UserName})
            App.SurveysManager.ActiveUserEmail = SurveyLoadEmail
            Dim tSurvey As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, CType(Data.SurveyType, SurveyType), UsersList)  ' D0379
            If Not tSurvey Is Nothing Then
                Dim ARespondent As clsRespondent = tSurvey.Survey(SurveyLoadEmail).RespondentByEmail(SurveySpyron.RespondentEMail) 'L0043 'L0442 'L0492
                ' D0428 ===
                If Not IsReadOnly Then 'L0492
                    Dim NeedToRebuildPipe As Boolean = False
                    SurveySpyron.ReadPageAnswers(NeedToRebuildPipe)
                    If NeedToRebuildPipe Then
                        App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
                        For Each cg As clsCombinedGroup In App.ActiveProject.ProjectManager.CombinedGroups.GroupsList
                            cg.ApplyRules()
                        Next
                    End If
                End If
                ' D0428 ==
                ' -D0428 === // was for multi-questions in one pipe step
                'If btn Is btnNext Then
                '    'L0043 ===
                '    If SurveySpyron.SurveyPage.GetNextPage(ARespondent) IsNot Nothing Then
                '        SurveySpyron.ReadPageAnswers()
                '        Session("SurveyCurrentPage") = SurveySpyron.SurveyPage.GetNextPage(ARespondent)
                '        SurveySpyron.SurveyPage = Session("SurveyCurrentPage")
                '        Exit Sub
                '    End If
                '    'If SurveySpyron.MoveNextPage() Then Exit Sub
                '    'L0043 ==
                'End If
                'If btn Is btnPrev Then
                '    'L0043 ===
                '    If SurveySpyron.SurveyPage.GetPrevPage(ARespondent) IsNot Nothing Then
                '        SurveySpyron.ReadPageAnswers()
                '        Session("SurveyCurrentPage") = SurveySpyron.SurveyPage.GetPrevPage(ARespondent)
                '        SurveySpyron.SurveyPage = Session("SurveyCurrentPage")
                '        Exit Sub
                '    End If
                '    'If SurveySpyron.MovePrevPage() Then Exit Sub
                '    'L0043 ==
                'End If
                ' -D0428 ==
            End If
        End If
        ' D0253 ==
        If cbAutoAdvance.Visible Then isAutoAdvance = cbAutoAdvance.Checked ' D0163
        ' D0023 ===
        If tAction IsNot Nothing AndAlso isChanged.Value <> "0" And Not IsReadOnly Then   ' D0308 + D0983
            'Dim Action As clsAction = CurAction -D0485: use tAction

            ' D3461 ===
            If isEvaluation(tAction) AndAlso cbAutoAdvance.Visible AndAlso Not IsReadOnly AndAlso Not isAutoAdvance AndAlso CanApplyAutoAdvance(tAction) AndAlso Autoadvance_JudgmentsCount >= 0 Then   ' D4011 + D4012
                ' -D3467
                'If tAction.ActionType = ActionType.atPairwise Then
                '    isGPWStep = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(App.ActiveProject.HierarchyObjectives.GetNodeByID(CType(tAction.ActionData, clsPairwiseMeasureData).ParentNodeID))
                'End If
                'If tAction.ActionType = ActionType.atPairwiseOutcomes Then
                '    isGPWStep = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(tAction.PWONode)
                'End If
                If tAction.ActionType = ActionType.atPairwise OrElse tAction.ActionType = ActionType.atPairwiseOutcomes Then CreatePairwise(tAction, ActionContent.Controls, lblTask.Text) ' D3467 + D4008
                If Not isGPWStep Then Autoadvance_JudgmentsCount += 1 ' D2078 + D3437
            End If
            ' D3461 ==

            Dim sStepType As String = ""    ' D2236
            Select Case tAction.ActionType
                Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                    SavePairwiseData(tAction, CheckVar("pwValue", ""), CheckVar("pwAdv", ""), CheckVar("PWComment", ""))    ' D0198 + D1846
                    sStepType = "Pairwise"    ' D2236
                    ' D0039 ===
                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                    SaveMultiPairwiseData(tAction)    ' D1155
                    sStepType = "Multi Pairwise"    ' D2236
                Case ActionType.atNonPWOneAtATime 'Cv2
                    'Cv2===
                    Select Case CType(tAction.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType 'C0464
                        Case ECMeasureType.mtRatings
                            SaveRatingData(tAction, Request("RatingValue"), Request("RatingComment"))    ' D0195
                            sStepType = "Rating"    ' D2236
                        Case ECMeasureType.mtRegularUtilityCurve    ' D0569
                            SaveRegularUtilityCurve(tAction)        ' D0569
                            sStepType = "Utility Curve"    ' D2236
                        Case ECMeasureType.mtDirect     ' D0167
                            SaveDirectData(tAction)     ' D0167
                            sStepType = "Direct Input"    ' D2236
                        Case ECMeasureType.mtStep       ' D0268
                            SaveStepFunction(tAction)   ' D0268
                            sStepType = "Step Function"    ' D2236
                    End Select
                    'Cv2==
                    ' D0039 ==
                    ' D0077 ===
                Case ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs 'Cv2 + D0677
                    'Cv2===
                    Select Case CType(tAction.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType 'C0464
                        Case ECMeasureType.mtRatings
                            SaveMultiRatings(tAction)
                            sStepType = "Multi Ratings"    ' D2236
                        Case ECMeasureType.mtDirect ' D0678
                            SaveMultiDirectInputs(tAction)  ' D0678
                            sStepType = "Multi Direct"    ' D2236
                    End Select
                    'Cv2==
                    ' D0077 ==
                    ' D3250 ===
                Case ActionType.atAllEventsWithNoSource
                    Select Case CType(tAction.ActionData, clsAllEventsWithNoSourceEvaluationActionData).MeasurementType
                        Case ECMeasureType.mtRatings
                            SaveMultiRatings(tAction)
                            sStepType = "Multi Ratings"
                        Case ECMeasureType.mtDirect
                            SaveMultiDirectInputs(tAction)
                            sStepType = "Multi Direct"
                    End Select
                    ' D3250 ==

                    ' D6671 ===
                Case ActionType.atEmbeddedContent
                    If tAction.EmbeddedContentType = EmbeddedContentType.AlternativesRank AndAlso tAction.ActionData IsNot Nothing Then
                        Dim sIDs As String = CheckVar("altRanks", "")
                        If Not String.IsNullOrEmpty(sIDs) Then
                            Dim IDs As List(Of Integer) = Param2IntList(sIDs)
                            With App.ActiveProject.ProjectManager
                                If IDs IsNot Nothing Then
                                    Dim Alts As List(Of clsNode) = CType(tAction.ActionData, List(Of clsNode))
                                    For Each tAlt As clsNode In Alts
                                        Dim Idx As Integer = IDs.IndexOf(tAlt.NodeID)
                                        tAlt.UserRank = If(Idx < 0, -1, Idx + 1)
                                        .Attributes.SetAttributeValue(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID, .UserID, AttributeValueTypes.avtLong, tAlt.UserRank, tAlt.NodeGuidID, Guid.Empty)
                                    Next
                                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, .UserID)
                                End If
                            End With
                        End If

                    End If
                    ' D6671 ==

            End Select

            App.DBUpdateDateTime(clsComparionCore._TABLE_PROJECTS, clsComparionCore._FLD_PROJECTS_LASTVISITED, App.ProjectID)    ' D0507

            ' D2236 ===
            If IsIntensities Then sStepType += " (Intensity)"
            ' D7262 ===
            Dim sStepInfo As String = sStepType
            If JudgLogs <> "" Then sStepInfo += ": " + JudgLogs
            App.DBSaveLog(dbActionType.actMakeJudgment, dbObjectType.einfProject, App.ProjectID, String.Format("Step #{1} of {2}", sStepInfo, CurStep, App.ActiveProject.Pipe.Count), sStepInfo)
            ' D2236 + D7262 ==

        End If
        ' D0229 === '// moved here 0517
        Dim btnValue As Integer = -1
        If btn Is btnJump Then btn.CommandArgument = CheckVar("jump", "") ' D1170
        If btn.Enabled And btn.CommandName.ToLower = _PARAM_STEP.ToLower And Integer.TryParse(btn.CommandArgument, btnValue) Then ' D4699
            ' D3256 ===
            If btnValue < 1 AndAlso App.ActiveProject.isImpact AndAlso App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE Then
                Dim sURL As String = ""
                If isSLTheme() Then
                    sURL = PageURL(_PGID_SERVICEPAGE, "action=flow&type=likelihood")
                Else
                    sURL = PageURL(CurrentPageID, String.Format("{0}={1}&step={2}", _PARAM_PASSCODE, App.ActiveProject.PasscodeLikelihood, If(btn Is btnFirst, 1, 999999)))   ' D6058 + D6684
                End If
                If App.Options.isSingleModeEvaluation Then App.Options.SingleModeProjectPasscode = CStr(IIf(App.ActiveProject.isImpact, App.ActiveProject.PasscodeLikelihood, App.ActiveProject.PasscodeImpact)) ' D3368
                If sURL <> "" Then Response.Redirect(sURL, True)
            End If
            ' D3256 ==
            ' D3253 ===
            If btnValue > App.ActiveProject.Pipe.Count AndAlso (App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE OrElse (App.ActiveProject.IsRisk AndAlso JoinRiskionPipes AndAlso Not App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE)) AndAlso Not IsIntensities Then    ' D3275 + D3278 + D3448 + D4103
                Dim sURL As String = ""
                Dim fShowResults As Boolean = (Not isImpact OrElse Riskion_ShowRiskResultsScreen) AndAlso ((App.ActiveProject.isImpact AndAlso App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE) OrElse (JoinRiskionPipes AndAlso Not App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE))    ' D3377 + D4103 + D4715
                If fShowResults Then    ' D3377
                    If isSLTheme() Then
                        sURL = PageURL(_PGID_SERVICEPAGE, "action=flow&type=riskresults")
                    Else
                        sURL = PageURL(_OPT_RISK_RESULTS_PGID, "back=" + CurrentPageID.ToString + "&h=" + (App.ActiveProject.ProjectManager.ActiveHierarchy).ToString + If(CanEditActiveProject AndAlso IsRealPM, "", "&widget=yes")) ' D6048 + D6058 + D6772
                    End If
                Else
                    If isSLTheme() Then
                        sURL = PageURL(_PGID_SERVICEPAGE, "action=flow&type=impact")
                    Else
                        sURL = PageURL(CurrentPageID, String.Format("{0}={1}&step=1", _PARAM_PASSCODE, App.ActiveProject.PasscodeImpact))
                    End If
                End If
                If App.Options.isSingleModeEvaluation Then App.Options.SingleModeProjectPasscode = CStr(IIf(App.ActiveProject.isImpact OrElse fShowResults, App.ActiveProject.PasscodeLikelihood, App.ActiveProject.PasscodeImpact)) ' D3363 + D3377
                If sURL <> "" Then Response.Redirect(sURL, True)
            End If
            ' D3253 ==
            If btnValue > App.ActiveProject.Pipe.Count AndAlso Not IsIntensities Then
                Dim sURL As String = ""
                Dim sUserEmail As String = App.ActiveUser.UserEmail
                If App.ActiveProject.PipeParameters.RedirectAtTheEnd And App.ActiveProject.PipeParameters.TerminalRedirectURL <> "" Then
                    sURL = ParseAllTemplates(App.ActiveProject.PipeParameters.TerminalRedirectURL, App.ActiveUser, App.ActiveProject, True) ' D1507
                End If
                ' D4160 ===
                Dim sCloseURL As String = ""
                If App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish Then ' D4162
                    sCloseURL = PageURL(_PGID_SERVICEPAGE, "close=yes")
                End If
                ' D4160 ==
                If App.ActiveProject.PipeParameters.LogOffAtTheEnd Then
                    LogoutAndCheckReturnUser(String.Format("Project '{0}' Request{1}", App.ActiveProject.Passcode, IIf(sURL <> "", String.Format("; Redirect to URL: {0}", ShortString(sURL, 40, False)), ""))) ' D1529
                    If sCloseURL <> "" Then sURL = sCloseURL ' D4160
                    If sURL = "" Then sURL = PageURL(_PGID_START)
                Else
                    If sURL <> "" Then App.DBSaveLog(dbActionType.actRedirect, dbObjectType.einfUser, App.ActiveUser.UserID, "Redirect at the end of Evaluation", String.Format("URL: {0}", ShortString(sURL, 40, False))) ' D0496
                End If
                If sURL <> "" And IsReadOnly Then sURL = PageURL(_PGID_PROJECTSLIST)  ' D0308
                'If sURL <> "" Then Response.Redirect(sURL, True)
                If App.HasActiveProject AndAlso (App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish AndAlso NextProject IsNot Nothing AndAlso Not NextProject.isMarkedAsDeleted AndAlso Not IsRiskWithControls) Then  ' D4177 + D4382
                    Dim sExtra As String = "step=1" + CStr(IIf(NextProject.isTeamTimeImpact OrElse NextProject.isTeamTimeLikelihood, "&pipe=no", "&pipe=yes"))  ' D4177
                    Dim sPasscode = App.ActiveProject.ProjectManager.Parameters.EvalNextProjectPasscodeAtFinish
                    Dim fIsRisk = App.isRiskEnabled AndAlso sPasscode.ToLower.StartsWith(clsProjectParametersWithDefaults.OPT_PASSCODE_RISK_CONTROLS_PIPE_PREFIX.ToLower)   ' D4177
                    If fIsRisk Then
                        sPasscode = sPasscode.ToLower.Replace(clsProjectParametersWithDefaults.OPT_PASSCODE_RISK_CONTROLS_PIPE_PREFIX.ToLower, "")  ' D4177
                        sExtra += String.Format("{0}mode=riskcontrols", IIf(sExtra = "", "", "&"))
                    End If
                    sURL = CreateLogonURL(App.ActiveUser, NextProject, sExtra, _URL_ROOT, sPasscode, , True)  ' D4616
                End If
                If sURL <> "" Then
                    If isSLTheme() Then sURL = PageURL(_PGID_SERVICEPAGE, String.Format("?{0}=redirect&url={1}", _PARAM_ACTION, HttpUtility.UrlEncode(sURL)))
                    Response.Redirect(sURL, True)
                End If
                If sCloseURL <> "" Then Response.Redirect(sCloseURL) ' D4160
            End If
        End If
        ' D0299 ==
        ' D0023 ==
        If btn.CommandArgument <> "" Then
            ' D0196 ===
            Dim BtnStep As Integer = CurStep    ' D1170
            If Not Integer.TryParse(btn.CommandArgument, BtnStep) Then BtnStep = CurStep ' D1170
            Dim sURL As String = PageURL(CurrentPageID, GetTempThemeURI(False)) ' D0228 + D0766
            If Not String.IsNullOrEmpty(btn.Attributes.Item("extra")) Then sURL = String.Format("{0}{1}{2}", sURL, IIf(sURL.Contains("?"), "&", "?"), btn.Attributes("extra")) ' D4109
            ' D1481 ===
            Dim tRet As Integer = -1
            tRet = CheckVar("ret", tRet)
            If tRet = BtnStep AndAlso CheckVar("noir", False) Then sURL += CStr(IIf(sURL.Contains("?"), "&", "?")) + "return=" + CurStep.ToString ' D1491
            ' D1481 ==
            ' D4143 ===
            Dim sRetPage As Integer = CheckVar("retpage", -1)
            If sRetPage > 0 Then
                If isSLTheme() Then
                    sURL = PageURL(_PGID_SERVICEPAGE, String.Format("action=navigate&pgid={0}{1}", sRetPage, GetTempThemeURI(True)))
                Else
                    sURL = PageURL(sRetPage, GetTempThemeURI(True))
                End If
            End If
            ' D4143 ==

            If BtnStep <= App.ActiveProject.Pipe.Count Then CurStep = BtnStep ' D0443
            If CheckVar("noir", False) AndAlso isEvaluation(CurAction) Then sURL += If(sURL.Contains("?"), "&", "?") + "noir=1" ' D1491
            'If CurStep = App.ActiveProject.Pipe.Count AndAlso CheckVar(_PARAM_ACTION, "").ToLower = "finish" Then
            If CurStep = App.ActiveProject.Pipe.Count AndAlso isFinish.Value <> "" Then   ' D2969
                'sURL += IIf(sURL.Contains("?"), "&", "?") + "action=finish" ' D1500
                isFinish.Value = "1"    ' D2969
                If IsIntensities Then
                    ' D1821 ===
                    Dim fExtracted As Boolean = False
                    If TypeOf (_IntensityScale) Is clsRatingScale Then fExtracted = App.ActiveProject.ProjectManager.MeasureScales.ExtractRatingScaleFromHierarchy(App.ActiveProject.HierarchyObjectives, CType(IntensityScale, clsRatingScale))
                    If TypeOf (_IntensityScale) Is clsStepFunction Then fExtracted = App.ActiveProject.ProjectManager.MeasureScales.ExtractStepFunctionFromHierarchy(App.ActiveProject.HierarchyObjectives, CType(IntensityScale, clsStepFunction))
                    App.DBSaveLog(dbActionType.actMakeJudgment, dbObjectType.einfProject, App.ProjectID, "Extract intensity scale", fExtracted.ToString)    ' D2236
                    Dim sComment As String = "" ' D3731
                    If _IntensityScale IsNot Nothing Then sComment = _IntensityScale.Name ' D3731
                    RestoreIntensitiesHID()   ' D4264
                    If fExtracted Then
                        ' D1821 ==
                        sURL = PageURL(_PGID_SERVICEPAGE, _PARAM_ACTION + "=intensities&close=0" + GetTempThemeURI(True)) 'A0882
                        App.ActiveProject.SaveStructure("Save accessed intensities list", True, True, sComment)   ' D3571 + D3770 + D3731
                        'App.ActiveProject.ProjectManager.StorageManager.Writer.SaveProject(True)
                        ' D2020 ===
                        SessVar(SESSION_ORIGINAL_HID) = Nothing
                        SessVar(SESSION_SCALE_ID) = Nothing
                        SessVar(SESSION_EXCLUDE_GUIDS) = Nothing    ' D2044
                        App.ActiveProject.PipeParameters.CurrentParameterSet = App.ActiveProject.PipeParameters.DefaultParameterSet
                        ' D2020 ==
                    End If
                End If
            End If

            Response.Redirect(sURL, True)   ' D0116
            ' D0196 ==
        End If
    End Sub

    ' D0137 ===
    Private Sub ShowMessage(sMsg As String)
        lblMessage.Text = String.Format("<h4 style='margin:1em 6em'>{0}</h4>", sMsg)    ' D0342
        lblMessage.Visible = True
        pnlEvaluate.Visible = False
        AlignVerticalCenter = True
        AlignHorizontalCenter = True
    End Sub
    ' D0137 ==

    Private Sub UpdatePanel()
        If App.ActiveProject.ProjectStatus <> ecProjectStatus.psActive AndAlso Not (App.ActiveProject.ProjectStatus = ecProjectStatus.psMasterProject AndAlso CurrentPageID = _PGID_EVALUATE_INTENSITIES) Then Response.Redirect(GetBackURL(PageURL(_DEF_PGID_ONEVALUATEPROJECT)), True) ' D0144 + D0206 + D0300 + D0459 + D0492 + D0818 + D2501 + D2561

        ' D0134 + D0137 ===
        Dim fCanEvaluate As Boolean = True
        If Not CanEditActiveProject AndAlso CurrentPageID <> _PGID_EVALUATE_INTENSITIES Then    ' D0135 + D0194 + D2561
            If App.ActiveProject.isValidDBVersion Then
                ' D1547 + D1849 + D2501 ===
                Dim sRefreshURL As String = PageURL(CurrentPageID, "resetproject=yes" + GetTempThemeURI(True))  ' D2538
                Dim sMessage As String = ResString("msgEvaluationLocked")
                If App.ActiveProject.LockInfo IsNot Nothing Then
                    Select Case App.ActiveProject.LockInfo.LockStatus
                        Case ECLockStatus.lsLockForAntigua
                            sMessage = ResString("msgEvaluationLockedAntigua")
                        Case ECLockStatus.lsLockForSystem
                            sMessage = ResString("msgEvaluationLockedBySystem")
                    End Select
                End If
                ShowMessage(String.Format("{0}<div style='margin-top:6em; text-align:center'><input type=button value='{1}' class='btn' style='width:10em' onclick='document.location.href=""{2}""; l=$get(""{3}""); if ((l)) l.style.display=""none""; showLoadingPanel(); this.disabled=1;'></div>", sMessage, ResString("btnRefreshProjects"), sRefreshURL, lblMessage.ClientID))
                ' D2501 ==
                ScriptManager.RegisterStartupScript(Page, GetType(String), "AutoReload", String.Format("setTimeout('document.location.href=""{0}""', 20000);", sRefreshURL), True)
                ' D1849 ==
                fCanEvaluate = False
            Else
                If Not isProjectDBValidOrUpdated(App.ActiveProject, CurrentPageID) Then    ' D0622
                    ShowMessage(ResString("msgEvaluationNotValidDB"))
                    fCanEvaluate = False
                End If
                ' D1547 ==
            End If
        End If

        ' D0811 ===
        If (Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) OrElse Not App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) OrElse Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID) OrElse Not App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID)) Then    ' D0913 + D3947
            Dim sLicError As String = ""
            If App.CheckLicense(App.SystemWorkgroup, sLicError, True) Then App.CheckLicense(App.ActiveWorkgroup, sLicError, True)
            If sLicError = "" Then sLicError = ResString(String.Format(ResString("errLicense"), "expiration date"))
            ShowMessage(sLicError)
            fCanEvaluate = False
        End If
        ' D0811 ==

        ' D2165 ===
        If App.ActiveProject.isTeamTime Then
            If isSLTheme() Then ' D2927
                ShowMessage(ResString("lblTeamTimeIsActive") + String.Format("<p align='center' style='margin-top:4em'><input type=button class='button' style='width:18em' value='{0}' onclick='window.open(""{1}"", ""TeamTime""); return false;'></p>", ResString("btnTeamTimeOpen"), PageURL(_PGID_TEAMTIME, "?temptheme=tt")))   ' D2538
                fCanEvaluate = False
                Dim isTTOwner As Boolean = IsPM AndAlso App.ActiveProject.MeetingStatus(App.ActiveUser) = ECTeamTimeStatus.tsTeamTimeSessionOwner   ' D4116
                If isTTOwner AndAlso Not IsReadOnly AndAlso Not IsIntensities Then ' D3495
                    ClientScript.RegisterStartupScript(GetType(String), "Navigate", "if ((window.parent) && (typeof (window.parent.NavigateToPage) == 'function')) window.parent.NavigateToPage(30502,'');", True)
                End If
            Else
                If Not IsReadOnly AndAlso Not IsIntensities Then Response.Redirect(PageURL(_PGID_TEAMTIME, GetTempThemeURI(False)), True) ' D1286 + D2025 + D3495
            End If
        End If
        ' D2165 ==

        ' D0342 ===
        'If App.ActiveProject.ProjectParticipating = ecProjectParticipating.ppOffline And Not App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup) Then  ' D0488 -D0748
        If fCanEvaluate AndAlso (Not App.ActiveProject.isOnline AndAlso Not App.Options.ignoreOffline AndAlso Not Str2Bool(SessVar(_SESS_FORCE_PIPE)) AndAlso Not IsPM AndAlso Not App.CanUserDoProjectAction(ecActionType.at_mlViewModel, App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup)) Then  ' D0488 + D0748 + D0835 + D2650 + D4416 + D6619
            ShowMessage(ResString("msgProjectIsOffline"))
            fCanEvaluate = False
        End If
        ' D0342 ==

        ' D0228 ===
        If fCanEvaluate AndAlso (App.ActiveProject.ProjectStatus = ecProjectStatus.psArchived Or App.ActiveProject.isMarkedAsDeleted) Then    ' D0300 + D0789
            ShowMessage(ResString("msgProjectIsArchived"))
            fCanEvaluate = False
        End If
        ' D0288 ==

        If Not IsPM Then  ' D0488 + D0835 + D4116
            If App.ActiveProject.PipeParameters.StartDate.HasValue Then
                If App.ActiveProject.PipeParameters.StartDate.Value > Now.Date Then
                    ShowMessage(ResString("msgEvaluationNotStarted"))
                    fCanEvaluate = False
                End If
            End If
            If App.ActiveProject.PipeParameters.EndDate.HasValue Then
                If App.ActiveProject.PipeParameters.EndDate.Value < Now.Date Then
                    ShowMessage(ResString("msgEvaluationIsCompleted"))
                    fCanEvaluate = False
                End If
            End If

            If App.ActiveWorkspace IsNot Nothing AndAlso App.ActiveWorkspace.Status(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsDisabled Then ' D1945
                ShowMessage(ResString("msgEvaluationIsDisabled"))
                fCanEvaluate = False
            End If

        End If

        '' D0229 ===
        'If fCanEvaluate And isDataInstance And App.ActiveProject.ProjectManager.DataInstances.Count = 0 Then
        '    ShowMessage(String.Format(ResString("msgNoDataInstance"), HTMLTextLink(PageURL(_PGID_STRUCTURE_DATAINSTANCES), PageTitle(_PGID_STRUCTURE_DATAINSTANCES), IsPM)))    ' D0488 + D0835 + D4116
        '    fCanEvaluate = False
        'End If
        '' D0229 ==

        ' D0467 ===
        If fCanEvaluate And App.ActiveProject.Pipe.Count = 0 Then
            ShowMessage(ResString("msgNoEvaluationSteps"))
            fCanEvaluate = False
        End If
        ' D0467 ==

        If fCanEvaluate AndAlso IsIntensities AndAlso IntensityScale Is Nothing Then
            ShowMessage(ResString("msgIntensityNotFound"))
            fCanEvaluate = False
        End If

        If fCanEvaluate Then
            ' D0398 ===
            Dim fShowContent As Boolean = True

            ' D0137 ==
            pnlEvaluate.Visible = True
            lblMessage.Visible = False
            If IsIntensities OrElse Not ShowTopNavigation OrElse Not ShowNavigation Then pnlEvaluate.CssClass = "panel"    ' D4940 + D4941

            If CurStep < 0 Then CurStep = 1
            LoadStepButtons()
            If fShowContent Then LoadStepContent()
            ' D0398 ==

        Else
            If Not Request.UrlReferrer Is Nothing AndAlso Not isSLTheme() AndAlso App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsUnLocked Then lblMessage.Text += String.Format("<div style='margin-top:8em; font-weight:bold' class='text'>[ {0} ]</div>", HTMLTextLink(GetBackURL, ResString("btnBack"), True, " style='actions'")) ' D0149 + D0229 + D0942 + D1849
            App.ActiveProject.ResetProject(True)    ' D3568
        End If
        ' D0134 ==
    End Sub

    ' -D3382
    '' D3381 ===
    'Private Function HasCompleteHierarchy() As Boolean
    '    Dim HasCH As Boolean = False
    '    For Each tNode As clsNode In App.ActiveProject.HierarchyObjectives.Nodes
    '        Dim PN As List(Of clsNode) = tNode.ParentNodes
    '        If PN IsNot Nothing AndAlso PN.Count > 1 Then
    '            HasCH = True
    '            Exit For
    '        End If
    '    Next
    '    Return HasCH
    'End Function
    '' D3381 ==

    ' D0019 ===
    Private mPollStatus As Boolean = False

    Private Sub LoadStepContent()
        ' D0349 ===
        Select Case App.Options.BackDoor
            Case _BACKDOOR_PLACESRATED
                If CurStep <> 2 Then
                    tdTask.Attributes.Remove("style")
                    tdTask.Attributes.Add("style", "padding:1ex 1em 1ex 1ex;")
                    tdColRight.Visible = True
                    tdColRight.Attributes.Add("valign", "top")
                    tdColRight.Attributes.Add("style", "width:200px; padding-top:1ex;")
                    lblColRight.Visible = True
                    ' D0350 ===
                    If Request.IsLocal Then
                        lblColRight.InnerHtml = "<div style='padding:140px 1em; border:1px solid #f0f0f0; text-align:center' class='text small gray'>Disabled loading content<br> for local request</div>"
                    Else
                        lblColRight.InnerHtml = File_GetContent(_FILE_SPECIAL + "PlacesRated_ad.inc") ' D0350
                    End If
                    ' D0350 ==
                End If
        End Select
        ' D0349 ==

        If CurStep >= 1 Then
            Dim Action As clsAction = CurAction
            lblTask.Text = ""
            'SA.Visible = False   ' D0155
            'RadAjaxPanelResults.Visible = False     ' D0157 -D1856

            Select Case Action.ActionType
                Case ActionType.atInformationPage
                    CreateInfopage(Action, ActionContent.Controls, lblTask.Text)

                Case ActionType.atPairwise, ActionType.atPairwiseOutcomes   ' D1956
                    CreatePairwise(Action, ActionContent.Controls, lblTask.Text)

                    ' D1155 ===
                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes ' D1956
                    CreateMultiPairwise(Action, ActionContent.Controls, lblTask.Text)
                    ' D1155 ==

                    ' D0250 ===
                Case ActionType.atSpyronSurvey
                    CreateSurvey(Action, ActionContent.Controls, lblTask.Text)
                    ' D0250 ==

                    ' D4715 ===
                Case ActionType.atEmbeddedContent
                    CreateEmbeddedContent(Action, ActionContent.Controls, lblTask.Text)
                    ' D4715 ==

                    ' D0058 ===
                Case ActionType.atNonPWOneAtATime 'Cv2
                    'Cv2===
                    Select Case CType(Action.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType 'C0464
                        Case ECMeasureType.mtRatings
                            CreateRating(Action, ActionContent.Controls, lblTask.Text)
                        Case ECMeasureType.mtRegularUtilityCurve    ' D0173
                            CreateRegularUtilityCurve(Action, ActionContent.Controls, lblTask.Text) ' D0173
                        Case ECMeasureType.mtDirect ' D0175
                            CreateDirectData(Action, ActionContent.Controls, lblTask.Text) ' D0175
                        Case ECMeasureType.mtStep   ' D0268
                            CreateStepFunction(Action, ActionContent.Controls, lblTask.Text)    ' D0268
                    End Select
                    'Cv2==
                    ' D0058 ==

                    ' D0077 ===
                Case ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs 'Cv2 + D0677
                    'Cv2===
                    Select Case CType(Action.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType 'C0464
                        Case ECMeasureType.mtRatings
                            CreateMultiRatings(Action, ActionContent.Controls, lblTask.Text)
                        Case ECMeasureType.mtDirect ' D0678
                            CreateMultiDirectInputs(Action, ActionContent.Controls, lblTask.Text)   ' D0678
                    End Select
                    'Cv2==
                    ' D0077 ==

                    ' D3250 ===
                Case ActionType.atAllEventsWithNoSource
                    Select Case CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData).MeasurementType
                        Case ECMeasureType.mtRatings
                            CreateMultiRatings(Action, ActionContent.Controls, lblTask.Text)
                        Case ECMeasureType.mtDirect
                            CreateMultiDirectInputs(Action, ActionContent.Controls, lblTask.Text)
                    End Select

                    ' D0153 ===
                Case ActionType.atSensitivityAnalysis
                    CreateSensitivityAnalysis(Action, ActionContent.Controls, lblTask.Text) ' D0182
                    ' D0153 ==

                Case ActionType.atShowLocalResults
                    CreateLocalResults(Action, ActionContent.Controls, lblTask.Text)

                Case ActionType.atShowGlobalResults
                    CreateGlobalResults(Action, ActionContent.Controls, lblTask.Text)

            End Select

            ' D2093 ===
            'If (isEvaluation(Action) OrElse Action.ActionType = ActionType.atShowLocalResults OrElse Action.ActionType = ActionType.atShowGlobalResults) AndAlso Not IsReadOnly AndAlso Not IsIntensities Then  ' D2364
            'If CanUserEditClusterPhrase AndAlso Not IsReadOnly AndAlso Not IsIntensities AndAlso Not IsRiskWithControls AndAlso IsPM Then ' D2651 + D2709 + D4116
            If CanEditClusterPhrase() Then ' D2651 + D2709 + D4116 + D4133
                divEditTask.Visible = True
                tdTask.Attributes.Add("style", "padding-left:8em;")         ' D6917
                lblTask.Attributes.Add("style", "padding:0px; width:85%")   ' D2187
                Dim jHTMLCSS As New Literal
                jHTMLCSS.Text = "<link rel='Stylesheet' type='text/css' href='../../App_Themes/jHTMLArea/jHtmlArea.css' />" + vbCrLf +
                                "<link rel='Stylesheet' type='text/css' href='../../App_Themes/jHTMLArea/jHtmlArea.ColorPickerMenu.css' />" + vbCrLf
                Page.Header.Controls.Add(jHTMLCSS)
            End If
            ' D2093 ==

            ' D2078 ===
            If isEvaluation(Action) AndAlso CanApplyAutoAdvance(Action) AndAlso Not IsReadOnly AndAlso cbAutoAdvance.Visible AndAlso Not isAutoAdvance AndAlso Autoadvance_JudgmentsCount = Autoadvance_Max_Judgments AndAlso Not isGPWStep AndAlso Not App.ActiveProject.ProjectManager.Parameters.EvalNoAskAutoAdvance Then  ' D3437 + D3963 + D4012
                ClientScript.RegisterStartupScript(GetType(String), "AutoAdvance", "setTimeout('ComfirmEnableAutoadvance();', 1000);", True)
            End If
            ' D2078 ==

            'D0038 ===
            'If ActionContent.Controls.Count = 0 AndAlso Not SA.Visible AndAlso Not SurveySpyron.Visible AndAlso Not pnlSLResults.Visible AndAlso Not tblHTMLResults.Visible Then    ' D2331
            If ActionContent.Controls.Count = 0 AndAlso Not SurveySpyron.Visible AndAlso Not pnlSLResults.Visible AndAlso Not tblHTMLResults.Visible Then    ' D2331
                Dim l As New Label
                'l.Text = String.Format(String.Format("<h4 class='error' style='margin-top:7em'>{0}</h4>", ResString("msgEvaluationUnknownActionType")), Action.ActionType.ToString)    ' D0075 + D0244
                l.Text = String.Format(String.Format("<h4 style='margin-top:7em; width:75%'>{0}</h4>", ResString("msgEvaluationUnknownActionType")), Action.ActionType.ToString)    ' D0075 + D0244 + D1155
                l.ID = "lblUnknown"    ' D1239
                ActionContent.Controls.Add(l)
            End If
            ' D0038 ==

            ' D0146 ===
            If App.ActiveProject.PipeParameters.AllowNavigation AndAlso App.ActiveProject.PipeParameters.ShowProgressIndicator AndAlso App.ActiveProject.Pipe.Count > 0 Then    ' D1634 + D1657 + D2511
                'If Not IsIntensities AndAlso Not IsRiskWithControls AndAlso Not HasCompleteHierarchy() Then imgWhereAmI.Visible = True ' D2511 + D2654 + D3381 -D3382
                If Not IsIntensities AndAlso (App.isRiskEnabled OrElse App.ActiveProject.HierarchyObjectives.Nodes.Count > 1) Then imgWhereAmI.Visible = True ' D2511 + D2654 + D4376
                If Not IsIntensities AndAlso Not IsRiskWithControls Then imgWhereAmI.Visible = True ' D2511 + D2654 + D3381
                RadToolTipWhereAmI.Visible = True

                ' D1636 ===
                imgStepsList.Visible = True
                RadToolTipStepsList.Visible = True
                imgStepsList.AlternateText = ResString("lblStepsList")
                If App.ActiveProject.Pipe.Count > 18 Then divStepsList.Attributes("style") += "height:30em; overflow-y:scroll; margin-top:8px;"
                If App.ActiveProject.Pipe.Count <= _OPT_STEPS_SHOW_MAX Then  ' D3073
                    Dim Steps(App.ActiveProject.Pipe.Count - 1) As Integer
                    For i As Integer = 1 To Steps.Length
                        Steps(i - 1) = i
                    Next
                    RepStepsList.DataSource = Steps
                    RepStepsList.DataBind()
                    ' D3073 ===
                Else
                    RepStepsList.Visible = False
                    divStepsList.InnerHtml = String.Format("<p align=center style='margin-top:12em;'><img src='{0}devex_loading.gif' width=16 height=16 style='margin-bottom:1em;' ><br>{1}</p>", ImagePath, ResString("lblPleaseWait"))
                    ScriptManager.RegisterStartupScript(Me, GetType(String), "onLoadSteps", "load_steps = 1;", True)
                End If
                ' D3073 ==
                ' D1636 ==
            End If
            ' D0146 ==

            If Not divLocalResultsHeader.Visible AndAlso Not tdExtraContent.Visible Then tdMainCell.Height = "100%"    ' D5073

            imgNavHelp.Attributes("style") = "cursor:hand"  ' D2383
            imgNavHelp.Visible = App.ActiveProject.PipeParameters.ShowProgressIndicator  ' D6284
            tdTask.Visible = (lblTask.Text <> "" AndAlso tdTask.Visible) OrElse tdColLeft.Visible Or tdColRight.Visible    ' D0075 + D0349 + D2335
            If lblTask.Text = "" AndAlso tdTask.Visible Then tdTask.Attributes.Add("Style", "height:1px") ' D0349
            If Not tdTask.Visible AndAlso Not tdColLeft.Visible AndAlso Not tdColRight.Visible Then trHead.Visible = False ' D2335
            isChanged.Value = "0"
        End If

        If isSLTheme() AndAlso IsIntensities AndAlso Not pnlEvaluate.CssClass.ToLower.Contains("panel") Then pnlEvaluate.CssClass += " panel" ' D2511
    End Sub
    ' D0019 ==

    ' D3222 ===
    Public Function GetPMSwitch() As String
        Dim sRes As String = ""
        ' -D3227
        'If App.HasActiveProject AndAlso App.Options.isSingleModeEvaluation AndAlso Not isSLTheme() AndAlso App.ActiveProjectRoleGroup IsNot Nothing AndAlso _
        '   (App.ActiveProjectRoleGroup.ActionStatus(ecActionType.at_mlViewModel) = ecActionStatus.asGranted OrElse App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace)) Then
        '    sRes = String.Format("<span style='float:right; margin:3px 0px 3px 0px; height:28px;' class='top_warning'><a href='{0}' class='text actions'>{1}</a><br/><span class='text small gray' style='font-weight:normal'>(Silverlight required)</span></span><br/>", IIf(Request IsNot Nothing AndAlso Request.UserAgent.ToLower.Contains("chrome") AndAlso Request.UserAgent.ToLower.Contains("windows") AndAlso Not Request.UserAgent.ToLower.Contains("edge/"), _URL_ROOT + "Chrome.aspx", ParseAllTemplates(_TEMPL_URL_EVALUATE + "&forcesl=1", App.ActiveUser, App.ActiveProject)), String.Format(ResString("lblSwitchToPMMode"), App.ActiveProjectRoleGroup.Name))
        'End If
        Return sRes
    End Function
    ' D3222 ==

#Region "Build Steps"
    ' D0023 ===
    Private Sub LoadStepButtons()
        Dim AllSteps As Integer = App.ActiveProject.Pipe.Count
        Dim NU0 As Integer = GetNextUnassessed(1)     ' D0049
        If Not isAllowMissingJudgments() AndAlso (NU0 >= 0 And CurStep > NU0) Then CurStep = GetNextUnassessed(1) ' D0049 + D0447 + D0598

        Dim tRet As Integer = -1
        tRet = CheckVar("ret", tRet)

        'rowEdit.Visible = (Not isSLTheme() OrElse IsReadOnly OrElse IsIntensities) AndAlso App.CanUserDoProjectAction(ecActionType.at_mlViewOverallResults, App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup)  ' D0106 +D0488 + D0835 + D0938 + D0947 + D2650 + D2748
        'rowEdit.Visible = (Not isSLTheme() OrElse IsReadOnly) AndAlso App.CanUserDoProjectAction(ecActionType.at_mlViewOverallResults, App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup)  ' D0106 +D0488 + D0835 + D0938 + D0947 + D2650 + D2748 + D4935

        btnNext.Enabled = CurStep < AllSteps    ' D0046
        Dim fNeedLogout As Boolean = False   ' D4044
        If HasTerminalButton And Not btnNext.Enabled Then
            btnNext.Enabled = CurStep <= AllSteps ' D0228
            ' D1168 ===
            If HasTerminalButton Then
                ' D3253 ===
                Dim fCanShowRiskionNext As Boolean = (Not isImpact OrElse Riskion_ShowRiskResultsScreen) AndAlso ((App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE) OrElse (App.ActiveProject.IsRisk AndAlso CanEditActiveProject AndAlso JoinRiskionPipes AndAlso Not App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE)) AndAlso Not IsIntensities   ' D3275 + D3278 + D3448 + D4103 + D4715
                If App.ActiveProject.IsRisk AndAlso fCanShowRiskionNext Then    ' D3259 + D3275
                    Dim fShowResults As Boolean = (App.ActiveProject.isImpact AndAlso App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE) OrElse (JoinRiskionPipes AndAlso Not App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE) AndAlso Not IsIntensities     ' D3275 + D3278 + D3448 + D4103
                    If App.ActiveProject.PipeParameters.AllowNavigation Then    ' D6703
                        divNextHint.Visible = True
                        divNextHint.InnerHtml = "*" + ResString(CStr(IIf(fShowResults, "lblNextRiskResults", "lblNextImpact")))   ' D3275
                        btnNext.Text += "*"
                    End If
                    If isSLTheme() AndAlso Not isEvaluation(CurAction) Then
                            If fShowResults Then    ' D3275
                                btnNext.OnClientClick = String.Format("RiskResults(); return false;")
                            Else
                                btnNext.OnClientClick = String.Format("StartEvalImpact(); return false;")
                            End If
                        End If
                    Else
                        btnNext.Text = ResString("btnEvaluationFinish")
                End If
                ' D3253 ==
                ' D4163 ===
                If Not JoinRiskionPipes AndAlso App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish AndAlso NextProject IsNot Nothing Then
                    btnNext.Text = ResString("btnNextProject")
                    divNextHint.Visible = True
                    ' D4177 ===
                    If Not App.isRiskEnabled OrElse NextProject.ID <> App.ProjectID Then
                        divNextHint.InnerHtml = ResString("lblNextModelEval") ' D4177
                    Else
                        Dim sPasscode As String = App.ActiveProject.ProjectManager.Parameters.EvalNextProjectPasscodeAtFinish.ToLower
                        If App.ActiveProject.PipeParameters.AllowNavigation Then
                            If App.isRiskEnabled AndAlso sPasscode.StartsWith(clsProjectParametersWithDefaults.OPT_PASSCODE_RISK_CONTROLS_PIPE_PREFIX.ToLower) Then
                                divNextHint.InnerHtml = "*" + ResString("lblNextRiskControlsPipe") ' D4177
                            Else
                                divNextHint.InnerHtml = "*" + ResString(CStr(IIf(sPasscode = App.ActiveProject.PasscodeLikelihood.ToLower, "lblNextLikelihood", "lblNextImpact")))
                            End If
                        End If
                    End If
                    ' D4177 ==
                End If
                ' D4163 ==
                ' D1800 ===
                If IsIntensities Then
                    isFinish.Value = "1"    ' D2969
                    'btnNext.PostBackUrl = PageURL(CurrentPageID, "&action=finish" + GetTempThemeURI(True))
                Else
                    ' D1800 ==
                    If Not fCanShowRiskionNext Then  ' D3253 + D3275
                        If App.ActiveProject.PipeParameters.TerminalRedirectURL = "" OrElse Not App.ActiveProject.PipeParameters.RedirectAtTheEnd Then
                            'fNeedLogout = True   ' D4044
                            fNeedLogout = Not App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish OrElse NextProject Is Nothing ' D4188
                            'If isSLTheme() Then
                            '    btnNext.OnClientClick = "return DoLogoutEval();"
                            'Else
                            '    btnNext.OnClientClick = "return (ConfirmLogout());" ' D1507
                            'End If
                        End If
                    End If
                End If
            End If
            ' D1168 ==
        End If
        btnNext.CommandArgument = CStr(IIf(btnNext.Enabled, CurStep + 1, ""))   ' D0049
        ' D0196 ===
        ' D1500 ===
        If CurStep = AllSteps AndAlso (Not btnNext.Enabled OrElse fNeedLogout) Then   ' D2513 + D4044
            Dim tLastAction As clsAction = CurAction
            If isEvaluation(tLastAction) OrElse (tLastAction.ActionType = ActionType.atSpyronSurvey OrElse tLastAction.ActionType = ActionType.atSurvey) Then
                If (Not fNeedLogout) Then
                    btnNext.CommandArgument = AllSteps.ToString
                    btnNext.Enabled = True
                    btnNext.Text = ResString("btnSave") ' D4044
                End If
                'If CheckVar(_PARAM_ACTION, "").ToLower <> "finish" Then
                If isFinish.Value <> "" Then  ' D2969
                    'btnNext.PostBackUrl = PageURL(CurrentPageID, "&action=finish" + GetTempThemeURI(True))  ' D2513
                    isFinish.Value = "1"    ' D2969
                Else
                    ' D2513 ===
                    If IsRiskWithControls OrElse Not App.Options.isSingleModeEvaluation Then
                        'btnNext.Attributes("disabled") = "1"
                        'btnNext.Enabled = False
                    Else
                        If fNeedLogout Then ' D4416
                            If isSLTheme() Then btnNext.OnClientClick = "return DoLogoutEval();" Else btnNext.OnClientClick = "return (confirmLogout());"
                        End If
                    End If
                    ' D2513 ==
                End If
            Else
                If fNeedLogout AndAlso Not App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish AndAlso Not (App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish AndAlso NextProject IsNot Nothing) Then ' D4160 + D4162 + D4164
                    If isSLTheme() Then btnNext.OnClientClick = "return DoLogoutEval();" Else btnNext.OnClientClick = "return (confirmLogout());"
                End If
            End If
        End If
        ' D1500 ==
        btnNext.OnClientClick += String.Format("SetNextStep({0}); __doPostBack('{1}',''); return false;", btnNext.CommandArgument, btnNext.UniqueID) ' D1132 + D2950
        ' D0196 ==
        'btnNext.ToolTip = ResString("hintEvaluationNext")    ' D0108
        btnPrev.Enabled = CurStep > 1
        btnPrev.CommandArgument = CStr(IIf(btnPrev.Enabled, CurStep - 1, ""))   ' D0049
        'btnPrev.ToolTip = ResString("hintEvaluationPrev")    ' D0108
        btnPrev.OnClientClick += String.Format("SetNextStep({0});", btnPrev.CommandArgument) ' D1132
        ' D6684 ===
        btnFirst.Enabled = CurStep > 1
        btnFirst.CommandArgument = If(App.ActiveProject.isImpact AndAlso App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE, -1, 1).ToString
        btnFirst.OnClientClick += String.Format("SetNextStep({0});", btnFirst.CommandArgument)
        ' D6684 ==

        ' D3256 ===
        If CurStep = 1 AndAlso App.ActiveProject.IsRisk AndAlso App.ActiveProject.isImpact AndAlso App.ActiveProject.ProjectManager.PipeBuilder.OPT_RISKION_JOIN_PIPE AndAlso Not IsIntensities Then    ' D7185
            ' D3377 ===
            Dim fHasEvals As Boolean = Not App.Options.isSingleModeEvaluation
            If Not fHasEvals Then
                App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = App.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet
                App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
                App.ActiveProject.ProjectManager.PipeBuilder.CreatePipe()
                For Each tAction In App.ActiveProject.ProjectManager.Pipe
                    If isEvaluation(tAction) OrElse tAction.ActionType = ActionType.atSpyronSurvey OrElse tAction.ActionType = ActionType.atSurvey Then   ' D6378
                        fHasEvals = True
                        Exit For
                    End If
                Next
                App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = App.ActiveProject.ProjectManager.PipeParameters.ImpactParameterSet
                App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
                App.ActiveProject.ProjectManager.PipeBuilder.CreatePipe()
            End If

            If fHasEvals Then
                ' D3377 ==
                btnPrev.Enabled = True
                btnPrev.CommandArgument = "0"
                divNextHint.Visible = True
                If App.ActiveProject.PipeParameters.AllowNavigation Then    ' D6703
                    divNextHint.InnerHtml = "*" + ResString("lblPrevLikelihood")
                    btnPrev.Text += "*"
                End If
                btnFirst.Enabled = True
                ' D4163 ===
                If App.ActiveProject.isImpact AndAlso App.ActiveProject.ProjectManager.Pipe.Count < 2 AndAlso btnNext.Enabled AndAlso App.ActiveProject.PipeParameters.AllowNavigation Then ' D6703
                    btnNext.Text += "*"
                    divNextHint.InnerHtml += "<br>**" + ResString("lblNextRiskResults")
                End If
                ' D4163 ==

                If isSLTheme() Then
                    If isEvaluation(CurAction) Then
                        btnPrev.OnClientClick = String.Format("if (theForm.{0}.value == '0') {{ StartEvalLikelihood(); return false; }}", isChanged.ClientID)
                    Else
                        btnPrev.OnClientClick = "StartEvalLikelihood(); return false;"
                    End If
                End If
            End If
        End If
        ' D3256 ==

        btnSave.CommandArgument = CurStep.ToString ' D2154
        btnSave.OnClientClick += String.Format("SetNextStep({0});", btnSave.CommandArgument) ' D2155
        btnSave.Attributes.Add("style", "margin:2 0 0 2px")  ' D2154

        ' D0026 ===
        ' D0049 ===
        Dim NU As Integer = GetNextUnassessed(CurStep + 1)
        If NU < 0 And Not _SeekUnassessedFromCurrent Then NU = GetNextUnassessed(1) ' D0148 + D0178
        If NU = CurStep Then NU = -1 ' D0645
        btnNextUnas.CommandArgument = CStr(IIf(NU >= 0, NU, ""))
        btnNextUnas.Enabled = NU >= 0
        If btnNextUnas.Enabled AndAlso Not isAllowMissingJudgments() AndAlso NU > GetNextUnassessed(1) Then btnNextUnas.Enabled = False ' D0598
        'btnNextUnas.ToolTip = ResString("hintEvaluationNextUnas")    ' D0108
        btnNextUnas.OnClientClick += String.Format("SetNextStep({0});", btnNextUnas.CommandArgument) ' D1132

        If btnNext.Enabled AndAlso (Not isAllowMissingJudgments() AndAlso Not IsReadOnly) Then   ' D0308 + D0447 + D0598
            Dim Action As clsAction = CurAction
            If isEvaluation(Action) AndAlso isUndefined(Action) Then
                ' D0255 ===
                Dim fCanDisable As Boolean = True
                If Action.ActionType = ActionType.atNonPWOneAtATime Then
                    Select Case CType(Action.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType 'C0464
                        Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtCustomUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                            fCanDisable = False
                    End Select
                End If
                If fCanDisable Then btnNext.Enabled = False
                ' D0255 ==
            End If
        End If
        'btnNext.Visible = App.ActiveProject.PipeParameters.AllowNavigation
        ' D6686 ===
        btnPrev.Visible = True  ' App.ActiveProject.PipeParameters.AllowNavigation  ' D0447
        btnNextUnas.Visible = App.ActiveProject.PipeParameters.ShowNextUnassessed   'App.ActiveProject.PipeParameters.AllowNavigation AndAlso App.ActiveProject.PipeParameters.ShowNextUnassessed
        btnFirst.Visible = (Not App.ActiveProject.PipeParameters.ShowProgressIndicator OrElse Not App.ActiveProject.PipeParameters.AllowNavigation) AndAlso Not IsIntensities AndAlso StepsCount > 1  ' App.ActiveProject.PipeParameters.AllowNavigation AndAlso Not App.ActiveProject.PipeParameters.ShowProgressIndicator AndAlso Not IsIntensities AndAlso StepsCount > 1   ' D6684
        ' D0049 + D6686 ==

        If pnlNavigation.Visible AndAlso App.ActiveProject.PipeParameters.ShowProgressIndicator Then    ' D6686

            If App.ActiveProject.PipeParameters.AllowNavigation Then
                If AllSteps = 0 Then Exit Sub ' D0046
                StepsCount = If(App.isMobileBrowser, 9, 13) ' D4855

                Dim Terminals As Integer = 2

                Dim btnStart As Integer = CurStep - (StepsCount - Terminals) \ 2
                If btnStart < 2 Then
                    btnStart = 2
                Else
                    Terminals += 1
                    btnStart += 1
                End If

                Dim btnEnd As Integer = btnStart + (StepsCount - Terminals - 1)
                If btnEnd >= AllSteps Then
                    btnEnd = AllSteps - 1
                    btnStart = btnEnd - (StepsCount - Terminals - 1)
                    If btnStart < 2 Then btnStart = 2
                Else
                    Terminals += 1
                    btnEnd -= 1
                End If

                Dim Steps As New ArrayList
                If App.ActiveProject.PipeParameters.ShowProgressIndicator OrElse IsIntensities Then    ' D0049 + D0447
                    If AllSteps > 0 Then Steps.Add(1) ' D0467
                    If btnStart > 2 Then
                        Dim St As Integer = btnStart - (StepsCount - 3) \ 2
                        If St < 2 Then St = 2
                        If btnStart = 3 Then St = -2
                        Steps.Add(-St)
                    End If
                    For i As Integer = btnStart To btnEnd
                        Steps.Add(i)
                    Next
                    If btnEnd < AllSteps - 1 Then
                        Dim St As Integer = btnEnd + (StepsCount - 3) \ 2
                        If St > AllSteps - 1 Then St = AllSteps - 1
                        If btnEnd = AllSteps - 2 Then St = -(AllSteps - 1)
                        Steps.Add(-St)
                    End If
                    If AllSteps > 1 Then Steps.Add(AllSteps) ' D0467
                End If

                RepeaterSteps.DataSource = Steps
                RepeaterSteps.DataBind()
            End If

            lblEvaluated.Text = ""
            If IsRiskWithControls Then
                evalProgressTotal = App.ActiveProject.ProjectManager.PipeBuilder.GetControlsTotalJudgmentsCount
                evalProgressMade = App.ActiveProject.ProjectManager.PipeBuilder.GetControlsMadeJudgmentsCount
            Else
                Dim uList As New List(Of clsUser) From {App.ActiveProject.ProjectManager.User}
                Dim evalProgress As Dictionary(Of String, UserEvaluationProgressData) = App.ActiveProject.ProjectManager.StorageManager.Reader.GetEvaluationProgress(uList, App.ActiveProject.ProjectManager.ActiveHierarchy, evalProgressMade, evalProgressTotal)
            End If

            ' D0194 ===
            RepeaterSteps.Visible = True
            btnNext.Visible = True
            btnPrev.Visible = True
            'btnNextUnas.Visible = True

            If Not App.ActiveProject.PipeParameters.AllowNavigation AndAlso Not IsIntensities Then    ' D0049 + D0210 + D0212 + D0447 + D2511 + D6686
                lblEvaluated.Text = String.Format(ResString("lblEvaluationCurStep"), String.Format("<b>{0}</b>", CurStep), String.Format("<b>{0}</b>", AllSteps)) + " &nbsp; "  ' D0210
                lblStep.Text = ""
                RepeaterSteps.Visible = False
            Else
                lblStep.Text = String.Format("<b>{0}</b>:&nbsp;", ResString("lblEvaluationSteps"))
            End If
            If evalProgressTotal > 0 Then lblEvaluated.Text += String.Format(ResString("lblEvaluationStatus"), String.Format("<b>{0}</b>", evalProgressMade), String.Format("<b>{0}</b>", evalProgressTotal)) 'C0781
            'If jTotal > 0 Then lblEvaluated.Text += String.Format(ResString("lblEvaluationStatus"), String.Format("<b>{0}</b>", jTotal - jMissed), String.Format("<b>{0}</b>", jTotal)) ' D0031 'C0781

            LoadWhereAmI()  ' D0113 + D0209
            'D0194 ==
        Else
            pnlNavigation.Visible = False
        End If

        ' D0248 ===

        ' -D3677
        '' D0253 ===
        'Dim tData As clsAction = CurAction
        'If tData.ActionType = ActionType.atSpyronSurvey Then
        '    btnNextUnas.Visible = False
        '    pnlNavigation.Visible = False
        'End If
        '' D0253 ==

        ' D0565 ===
        If tRet > 0 AndAlso ShowSLLocalResults Then  ' D2303
            pnlNavigation.Visible = False
            btnPrev.Visible = False
            btnNextUnas.Visible = False
            btnFirst.Visible = False    ' D6684
            btnNext.CommandArgument = CStr(tRet)
        End If
        ' D0565 ==

        btnJump.Text = ResString("btnEvaluationJumpOK")
        ' D0026 ==
    End Sub

    Private Sub LoadWhereAmI()
        RadTreeViewHierarchy.Nodes.Clear()
        ' D4136 ===
        Dim H As clsHierarchy = App.ActiveProject.HierarchyObjectives
        'If App.isRiskEnabled AndAlso IsRiskWithControls AndAlso CurStepNode IsNot Nothing AndAlso CurStepNode.Hierarchy IsNot Nothing AndAlso CurStepNode.Hierarchy.HierarchyID = ECHierarchyID.hidImpact Then H = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact) ' D4137
        If App.isRiskEnabled AndAlso IsRiskWithControls Then H = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood) ' D4137 + D4146
        If H IsNot Nothing Then

            ' D4207 ===
            Dim H_offset As Integer = 0
            If App.isRiskEnabled AndAlso Not App.ActiveProject.isImpact Then
                Dim tUnContrib As List(Of clsNode) = H.GetUncontributedAlternatives()
                If tUnContrib.Count > 0 Then
                    divNoSources.Visible = True
                    H_offset = tUnContrib.Count
                    If tUnContrib.Count > 5 Then
                        divNoSources.Attributes("style") += "height:10em; overflow-y:scroll; margin-top:8px;"
                        H_offset = 5
                    End If
                    RadTreeViewNoSources.Nodes.Clear()
                    Dim rNode As New RadTreeNode
                    rNode.Text = String.Format("<span class='tree_root_popup'>{0}</span>", JS_SafeHTML(ResString("lblNoSourcesGoal")))
                    rNode.Expanded = True
                    RadTreeViewNoSources.Nodes.Add(rNode)
                    AddNodesToRadTree(tUnContrib, rNode.Nodes, True, 1, GetNextUnassessed(1))   ' D4213
                End If
            End If
            ' D4207 ==

            AddNodesToRadTree(H.GetLevelNodes(0), RadTreeViewHierarchy.Nodes, False, 0, GetNextUnassessed(1))    ' D0125 + D1274
            If H.Nodes.Count > (15 - H_offset) Then divTree.Attributes("style") += String.Format("height:{0}em; overflow-y:scroll; margin-top:8px;", 30 - 2 * H_offset) ' D1316 + D4207
            ' D4146 ===
            If IsRiskWithControls Then
                RadTreeViewImpact.Nodes.Clear()
                H = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact)
                If H IsNot Nothing Then
                    divTreeImpact.Visible = True
                    AddNodesToRadTree(H.GetLevelNodes(0), RadTreeViewImpact.Nodes, False, 0, GetNextUnassessed(1))
                    If H.Nodes.Count > (15 - H_offset) Then divTreeImpact.Attributes("style") += String.Format("height:{0}em; overflow-y:scroll; margin-top:8px;", 30 - 2 * H_offset) ' D4207
                    Dim tabID As Integer = If(CurStepNode IsNot Nothing AndAlso CurStepNode.Hierarchy IsNot Nothing AndAlso CurStepNode.Hierarchy.HierarchyID = ECHierarchyID.hidImpact, 1, 0)
                    ' D4354 ===
                    If RadTreeViewNoSources.Visible AndAlso RadTreeViewNoSources.SelectedNode IsNot Nothing Then tabID = 0
                    ScriptManager.RegisterStartupScript(Page, GetType(String), "InitWhereAmI", String.Format("$('#WhereAmITabs').tabs({{active: {0}}});", tabID), True)
                    ' D4354 ==
                End If
            End If
            ' D4354 ===
            RadTreeViewHierarchy.ClearSelectedNodes()
            RadTreeViewImpact.ClearSelectedNodes()
            RadTreeViewNoSources.ClearSelectedNodes()
            ' D4146 + D4354 ==
        Else
            RadToolTipWhereAmI.Visible = False
        End If
        ' D4136 ==
    End Sub

    Private Sub Node2RadTreeNode(sCaption As String, ID As Integer, Level As Integer, ByRef rNode As RadTreeNode)
        rNode.Text = JS_SafeHTML(ShortString(sCaption, If(Level = 0, 65, 80 - 4 * Level), True))
        rNode.Value = CStr(ID)
    End Sub

    Private Function AddRadTreeNode(tNode As clsNode, ByRef RadNodes As RadTreeNodeCollection, Level As Integer, isAlts As Boolean, isCategory As Boolean, isCurrent As Boolean) As RadTreeNode ' D3604 + D4354
        Dim rNode As New RadTreeNode
        Node2RadTreeNode(tNode.NodeName, If(isAlts, -1, 1) * tNode.NodeID, Level, rNode)   ' D4109
        If isAlts Then Level += 1
        Dim asRoot As Boolean = Level = 0
        ' D0146 ===
        If isCurrent Then
            rNode.Selected = True       ' D4354
            rNode.ExpandParentNodes()   ' D4354
        End If
        rNode.Text = String.Format("<span class='{1}'>{0}</span>", rNode.Text, If(asRoot, "tree_root_popup", "tree_node_popup") + If(isCurrent, " tree_current", If(isCategory, " tree_cat", ""))) ' D0148 + D3604
        rNode.CssClass = "aslink"
        ' D0146 ==
        RadNodes.Add(rNode)
        Return rNode
    End Function

    Private Sub AddNodesToRadTree(Nodes As List(Of clsNode), ByRef RadNodes As RadTreeNodeCollection, isAlts As Boolean, LevelOffset As Integer, NextUnass As Integer)     ' D0074 + D0125 + C0384 + D1274
        If Nodes Is Nothing Or RadNodes Is Nothing Then Exit Sub
        For Each tNode As clsNode In Nodes
            Dim Childs As List(Of clsNode) = tNode.Children    'tNode.GetNodesBelow.Clone ' D0125  - D0146
            ' D4108 ===
            Dim isCurrent As Boolean = (CurStepNode IsNot Nothing AndAlso CurStepNode Is tNode)   ' D4354
            Dim NodeStep As Integer = -1
            If isAlts AndAlso tNode.IsAlternative AndAlso tNode.ParentNode Is Nothing Then
                For i As Integer = 0 To App.ActiveProject.Pipe.Count - 1
                    Dim tPNode As Guid
                    Dim tLNode As Guid
                    Dim tRNode As Guid
                    App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNodeGUIDs(App.ActiveProject.Pipe(i), tPNode, tLNode, tRNode)
                    If tPNode.Equals(tNode.NodeGuidID) OrElse tLNode.Equals(tNode.NodeGuidID) OrElse tRNode.Equals(tNode.NodeGuidID) Then
                        NodeStep = i
                        Exit For
                    End If
                Next
                If NodeStep < 0 AndAlso isMultiEvaluation() Then NodeStep = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(tNode, -1) ' D0148 + D4212 + D4213
            Else
                NodeStep = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(tNode, -1) ' D0148
            End If
            'If Not tNode.IsAlternative Then
            '    'NodeStep = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(tNode, -1) ' D0148
            '    For i = 0 To App.ActiveProject.ProjectManager.Pipe.Count - 1
            '        Dim tStepNode As clsNode = GetStepNode(App.ActiveProject.ProjectManager, App.ActiveProject.ProjectManager.Pipe(i))
            '        If tNode Is tStepNode Then
            '            NodeStep = i
            '            Exit For
            '        End If
            '    Next
            'End If
            ' D4108 ==
            If CurStep - 1 = NodeStep Then isCurrent = True ' D4354
            Dim rNode As RadTreeNode = AddRadTreeNode(tNode, RadNodes, tNode.Level + LevelOffset, isAlts, tNode.RiskNodeType = RiskNodeType.ntCategory, isCurrent)    ' D3604 + D4109 + D4354
            If NextUnass > 0 AndAlso Not isAllowMissingJudgments() AndAlso NodeStep > NextUnass Then  ' D7365
                NodeStep = -1 ' D127
                rNode.Font.Italic = True ' D1634
            End If
            If NodeStep >= 0 Then
                rNode.Value = (NodeStep + 1).ToString ' D0608
                ' D4108 ===
                Dim tAction As clsAction = App.ActiveProject.ProjectManager.Pipe(NodeStep)
                Dim tRealNode As clsNode = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNode(tAction)
                If tRealNode IsNot Nothing AndAlso App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNode(tAction) IsNot Nothing AndAlso tAction.isEvaluation AndAlso isMultiEvaluation(tAction) Then
                    rNode.Attributes.Add("extra", String.Format("oid={0}&aid={1}", tNode.NodeID.ToString, tRealNode.NodeID.ToString))
                End If
                ' D4108 ==
            Else
                rNode.Value = "-1"        ' D0793
                rNode.CssClass = "gray" ' D0793 + D1634
            End If
            If Not String.IsNullOrEmpty(rNode.Value) AndAlso rNode.Value.Length > 0 AndAlso rNode.Value(0) <> "-" Then rNode.Text += String.Format("<span class='text small gray'>&nbsp;(#{0})</span>", rNode.Value) ' D1651
            If Childs.Count > 0 Then
                AddNodesToRadTree(Childs, rNode.Nodes, tNode.IsTerminalNode, LevelOffset, NextUnass)   ' D0125 + D1274
                rNode.Expanded = True
            End If
        Next
    End Sub
    ' D0113 ==

    Protected Sub RepeaterSteps_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles RepeaterSteps.ItemDataBound, RepStepsList.ItemDataBound   ' D1636
        Dim ID As Integer = Integer.Parse(e.Item.DataItem.ToString)
        Dim btn As Button = CType(e.Item.FindControl("btnStep"), Button)
        btn.CommandArgument = CStr(Math.Abs(ID))
        btn.Text = CStr(IIf(ID < 0, "…", CStr(ID)))
        btn.Enabled = App.ActiveProject.PipeParameters.AllowNavigation  ' D0447
        If ID < 0 Then
            btn.Text = "…"
            btn.CssClass += " button_ellipse"
            btn.Style.Add("border-width", "0px")    ' D0044
            btn.OnClientClick = String.Format("ShowJumpTooltip('{0}'); return false;", btn.ClientID)    ' D1170
        Else
            btn.Text = CStr(ID)
            Dim btnWidth As Integer = App.ActiveProject.Pipe.Count.ToString.Length
            If btnWidth > 3 Then btnWidth -= 1 ' D0271
            If btnWidth < 2 Then btnWidth = 2 ' D0048
            btn.Style.Add("width", String.Format("{0}ex", btnWidth + 4))    ' D0271
            btn.Style.Add("border-width", "1px")    ' D0044
        End If
        Dim NU As Integer = GetNextUnassessed(1)    ' D0049

        ' D7365 ===
        Dim Action As clsAction = CType(App.ActiveProject.Pipe(Math.Abs(ID) - 1), clsAction)

        'Dim AllowMissing = isAllowMissingJudgments()
        'If Action.ActionType = ActionType.atSpyronSurvey Then
        '    If isSurveyStepRequired(Action) Then
        '        AllowMissing = False
        '        If NU > ID Then NU = ID
        '    End If
        'End If

        'If btn.Enabled AndAlso (Not AllowMissing AndAlso Not IsReadOnly) AndAlso (NU >= 0 AndAlso ID > NU) Then btn.Enabled = False ' D0049 + D0308 + D0447 + D0498 + D1635
        If btn.Enabled AndAlso (Not isAllowMissingJudgments() AndAlso Not IsReadOnly) AndAlso (NU >= 0 AndAlso ID > NU) Then btn.Enabled = False ' D0049 + D0308 + D0447 + D0498 + D1635
        '' D7365 ==

        ' D0026 ===
        If ID >= 0 AndAlso isEvaluation(Action) Then
            btn.CssClass += CStr(IIf(isUndefined(Action), " button_evaluate_undefined", " button_evaluated"))
        Else
            btn.CssClass += " button_no_evaluate"
        End If
        ' D1138 ===
        If CurStep = ID Then btn.CssClass += CStr(IIf(sender Is RepStepsList, " button_active_list", " button_active")) ' D1693 + D1695

        'If CurStep = ID AndAlso CurrentStepBtnID = "" AndAlso (isEvaluation(Action) OrElse Action.ActionType = ActionType.atSpyronSurvey) AndAlso pnlNavigation.Visible AndAlso RepeaterSteps.Visible Then CurrentStepBtnID = btn.ClientID ' D2154 + D4277
        If CurStep = ID AndAlso CurrentStepBtnID = "" AndAlso pnlNavigation.Visible AndAlso RepeaterSteps.Visible AndAlso btn.Visible Then CurrentStepBtnID = btn.ClientID ' D2154 + D4277 + D4292

        If CurStep + 1 = ID AndAlso Not btn.Enabled Then
            Dim PrevAction As clsAction = CType(App.ActiveProject.Pipe(Math.Abs(ID) - 1), clsAction)
            If PrevAction.ActionType = ActionType.atSpyronSurvey Then btn.Enabled = True
        End If
        ' D1138 ==
        ' D0026 ==
        btn.OnClientClick += String.Format("SetNextStep({0});", btn.CommandArgument) ' D1132

        Dim sHint As String = Action.ActionType.ToString

        'btn.Attributes("title") = String.Format(ResString("btnEvaluationStepHint"), btn.CommandArgument, GetHint(ActiveProject.HierarchyObjectives, CType(ActiveProject.Pipe(Math.Abs(ID) - 1), clsAction)))  ' D0031
        If ID >= 0 Then btn.ToolTip = String.Format(ResString("btnEvaluationStepHint"), btn.CommandArgument, GetPipeStepHint(CType(App.ActiveProject.Pipe(Math.Abs(ID) - 1), clsAction), IntensityScale)) ' D0031 + D1636 + D1653 + D1870
        'RadToolTipManagerEvaluation.TargetControls.Add(btn.ClientID, True)   ' D0108   - D0247

        ' D1636 ===
        If sender Is RepStepsList Then
            'btn.CssClass += " button_ellipse"
            btn.Style("width") = "100%"
            btn.Style.Add("text-align", "left")
            btn.Style.Add("padding", "0px 2px")
            btn.Style.Add("font-weight", "normal")
            btn.Style("border") = "1px solid #dddddd"
            If Not btn.Enabled Then btn.Style.Add("font-style", "italic")
            btn.ToolTip = HTML2Text(btn.ToolTip).Replace(vbLf, " ").Replace(vbCr, "").Replace("  ", " ").Trim
            btn.Text = ShortString(btn.ToolTip, 140 - If(btn.Enabled, 2, 0) - ID.ToString.Length, True)    ' D3567
            If btn.ToolTip.Length > 100 Then btn.Attributes.Add("onmouseover", String.Format("this.title='{0}';", JS_SafeString(btn.ToolTip))) ' D3567
            btn.ToolTip = ""
        Else
            btn.Style.Add("padding", "0px")
            ' D4941 ===
            Dim Lst = CType(RepeaterSteps.DataSource, ArrayList)
            If Lst.Count > 7 Then
                Dim cnt As Integer = Lst.Count
                If (e.Item.ItemIndex > 0 AndAlso e.Item.ItemIndex < cnt) Then
                    Dim bj As Boolean = CInt(Lst(1)) < 0
                    Dim ej As Boolean = CInt(Lst(cnt - 2)) < 0
                    If bj Then
                        If e.Item.ItemIndex = 2 Then btn.CssClass += " nowide" + If(ej, "900", "1000")
                        If e.Item.ItemIndex = 3 Then btn.CssClass += " nowide" + If(ej, "800", "900")
                        If Not ej AndAlso e.Item.ItemIndex = 4 Then btn.CssClass += " nowide800"
                    End If
                    If ej Then
                        If e.Item.ItemIndex = cnt - 3 Then btn.CssClass += " nowide" + If(bj, "900", "1000")
                        If e.Item.ItemIndex = cnt - 4 Then btn.CssClass += " nowide" + If(bj, "800", "900")
                        If Not bj AndAlso e.Item.ItemIndex = cnt - 5 Then btn.CssClass += " nowide800"
                    End If
                End If
            End If
            ' D4941 ==
        End If
        ' D1636 ==

        AddHandler btn.Click, AddressOf btnStep_Click
    End Sub
    ' D0023 ==

    ' D0163 ===
    Private Sub EnableAutoAdvance()
        cbAutoAdvance.Visible = False
        If (Not App.ActiveProject.PipeParameters.AllowAutoadvance OrElse AlwaysShowAutoAdvance) AndAlso App.ActiveProject.PipeParameters.AllowNavigation Then  ' D0171 + D0194 + D0891
            cbAutoAdvance.Visible = True
            cbAutoAdvance.Checked = isAutoAdvance
            cbAutoAdvance.Text = ResString("lblEvaluationAutoAdvance")
        End If
    End Sub
    ' D0163 ==

    ' D0026 ===
    Public Function isEvaluation(Action As clsAction) As Boolean  ' D1840
        If Action Is Nothing Then Return False ' D0467
        Select Case Action.ActionType
            Case ActionType.atPairwise, ActionType.atAllPairwise, ActionType.atNonPWOneAtATime, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atPairwiseOutcomes, ActionType.atAllPairwiseOutcomes, ActionType.atAllEventsWithNoSource, ActionType.atSpyronSurvey 'Cv2 + D0677 + D1155 + D3250 + D7365
                Return True
            Case Else
                Return False
        End Select
    End Function

    ' D3064 ===
    Public Function isMultiEvaluation(Optional tAction As clsAction = Nothing) As Boolean   ' D4109
        Dim fres As Boolean = False
        If tAction Is Nothing Then tAction = CurAction
        If isEvaluation(tAction) Then
            Select Case tAction.ActionType
                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atAllEventsWithNoSource    ' D3250
                    fres = True
            End Select
        End If
        Return fres
    End Function
    ' D3064 ==

    ''' <summary>
    ''' Please note: for Surveys it returns true for steps that have required questions with missing answers
    ''' </summary>
    ''' <param name="Action"></param>
    ''' <returns></returns>
    Public Function isUndefined(Action As clsAction) As Boolean   ' D2951
        If Action.ActionData IsNot Nothing Then ' D1376
            Select Case Action.ActionType
                Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                    Return CType(Action.ActionData, clsPairwiseMeasureData).IsUndefined
                    ' D1155 ===
                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                    Dim fHasUndef As Boolean = False
                    Dim Data As clsAllPairwiseEvaluationActionData = CType(Action.ActionData, clsAllPairwiseEvaluationActionData)
                    If Data.Judgments IsNot Nothing Then    ' D1376
                        For Each tPW As clsPairwiseMeasureData In Data.Judgments
                            If tPW.IsUndefined Then
                                fHasUndef = True
                                Exit For
                            End If
                        Next
                    End If
                    Return fHasUndef
                    ' D1155 ==
                    ' D0039 ===
                Case ActionType.atNonPWOneAtATime 'Cv2
                    Dim Data As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData) ' D1376
                    If Data.Judgment IsNot Nothing Then Return CType(Data.Judgment, clsNonPairwiseMeasureData).IsUndefined Else Return False 'Cv2 + C0464 + D1376
                    ' D0039 ==
                    ' D0075 ===
                Case ActionType.atNonPWAllChildren 'Cv2 + D0677
                    Dim fHasUndef As Boolean = False
                    Dim Data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData) 'Cv2 'C0464 + D0677
                    If Data.Children IsNot Nothing Then ' D1376
                        For Each tAlt As clsNode In Data.Children  'Cv2 + D0677
                            Dim MD As clsNonPairwiseMeasureData = CType(Data.GetJudgment(tAlt), clsNonPairwiseMeasureData) 'Cv2
                            If MD IsNot Nothing AndAlso MD.IsUndefined Then fHasUndef = True 'Cv2 + D1376
                        Next
                    End If
                    Return fHasUndef
                Case ActionType.atNonPWAllCovObjs 'Cv2
                    Dim fHasUndef As Boolean = False
                    Dim Data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData) 'Cv2 'C0464
                    If Data.CoveringObjectives IsNot Nothing Then   ' D1376
                        For Each tAlt As clsNode In Data.CoveringObjectives
                            Dim MD As clsNonPairwiseMeasureData = CType(Data.GetJudgment(tAlt), clsNonPairwiseMeasureData) 'Cv2
                            If MD IsNot Nothing AndAlso MD.IsUndefined Then fHasUndef = True 'Cv2 + D1376
                        Next
                    End If
                    Return fHasUndef
                    ' D0075 ==
                    ' D3250 ===
                Case ActionType.atAllEventsWithNoSource
                    Dim fHasUndef As Boolean = False
                    Dim Data As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)
                    If Data.Alternatives IsNot Nothing Then
                        For Each tAlt As clsNode In Data.Alternatives
                            Dim MD As clsNonPairwiseMeasureData = CType(Data.GetJudgment(tAlt), clsNonPairwiseMeasureData)
                            If MD IsNot Nothing AndAlso MD.IsUndefined Then fHasUndef = True
                        Next
                    End If
                    Return fHasUndef
                    ' D3250 ==
                    ' D1132 ===
                Case ActionType.atSpyronSurvey
                    Dim UsersList As New Dictionary(Of String, clsComparionUser)
                    UsersList.Add(App.ActiveUser.UserEmail, New clsComparionUser() With {.ID = App.ActiveProject.ProjectManager.UserID, .UserName = App.ActiveProject.ProjectManager.User.UserName})
                    Dim Data As clsSpyronSurveyAction = CType(Action.ActionData, clsSpyronSurveyAction)
                    App.SurveysManager.ActiveUserEmail = App.ActiveUser.UserEmail
                    Dim tSurvey As SpyronControls.Spyron.Core.clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, CType(Data.SurveyType, SurveyType), UsersList) ' D1653
                    If tSurvey IsNot Nothing Then
                        Dim tmpSurvey As clsSurvey = tSurvey.Survey(App.ActiveProject.ProjectManager.User.UserEMail)   ' D1308
                        ' D7365 ===
                        If tmpSurvey IsNot Nothing AndAlso Data.StepNumber > 0 AndAlso tmpSurvey.Pages IsNot Nothing AndAlso tmpSurvey.Pages.Count >= Data.StepNumber AndAlso Data.StepNumber > 0 Then    ' D1308 + D1587
                            'If tmpSurvey.HasUndefined(Data.StepNumber - 1, App.ActiveProject.ProjectManager.User.UserEMail) Then Return True
                            If Not tmpSurvey.AllowToSkipPage(Data.StepNumber - 1, App.ActiveProject.ProjectManager.User.UserEMail) Then Return True ' D7366
                        End If
                        ' D7365 ==
                    End If
                    Return False
                    ' D1132 ==
            End Select
        End If
        Return False
    End Function

    Private Function GetNextUnassessed(FirstStep As Integer) As Integer   ' D0049
        For i As Integer = FirstStep To App.ActiveProject.Pipe.Count   ' D0049
            If isUndefined(CType(App.ActiveProject.Pipe(i - 1), clsAction)) Then Return i
        Next
        Return -1
    End Function
    ' D0026 ==

    ' D7365 ===
    Public Function isSurveyStepRequired(Action As clsAction) As Boolean
        Dim tRes As Boolean = False
        If Action IsNot Nothing AndAlso Action.ActionType = ActionType.atSpyronSurvey AndAlso Not IsReadOnly Then
            Dim UsersList As New Dictionary(Of String, clsComparionUser)
            UsersList.Add(App.ActiveUser.UserEmail, New clsComparionUser() With {.ID = App.ActiveProject.ProjectManager.UserID, .UserName = App.ActiveProject.ProjectManager.User.UserName})
            Dim Data As clsSpyronSurveyAction = CType(Action.ActionData, clsSpyronSurveyAction)
            App.SurveysManager.ActiveUserEmail = App.ActiveUser.UserEmail
            Dim tSurvey As SpyronControls.Spyron.Core.clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, CType(Data.SurveyType, SurveyType), UsersList)
            If Not tSurvey Is Nothing Then
                Dim tmpSurvey As clsSurvey = tSurvey.Survey(App.ActiveUser.UserEmail)
                If tmpSurvey IsNot Nothing AndAlso Data.StepNumber > 0 AndAlso tmpSurvey.Pages IsNot Nothing AndAlso tmpSurvey.Pages.Count >= Data.StepNumber AndAlso Data.StepNumber > 0 Then
                    tRes = Not tmpSurvey.AllowToSkipPage(Data.StepNumber - 1, App.ActiveProject.ProjectManager.User.UserEMail)
                End If
            End If
        End If
        Return tRes
    End Function
    ' D7365 ==

#End Region

#Region "Create Evaluation content"

    ' D0023 ===
    Public Function GetAction(stepNumber As Integer) As clsAction ' D1840
        If stepNumber < 1 Or stepNumber > App.ActiveProject.Pipe.Count Or App.ActiveProject.Pipe.Count < 1 Then Return Nothing Else Return CType(App.ActiveProject.Pipe(stepNumber - 1), clsAction) ' D0443 + D0467 + D0983
    End Function

    Private Sub CreateInfopage(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim Data As clsInformationPageActionData = CType(Action.ActionData, clsInformationPageActionData) 'C0464
        Dim info As New Literal
        info.ID = "lblInfo" ' D1239
        ' D0113 ===
        Dim sMessage As String = ""
        Dim isReward As Boolean = False
        ' D4293 ===
        Dim tType As PipeMessageKind = PipeMessageKind.pmkText
        If IsRiskWithControls Then tType = PipeMessageKind.pmlTextRiskControls
        Dim sPostfix As String = If(IsRiskWithControls, "_risk", "")
        Dim sEvalURL As String = App.Options.EvalSiteURL    ' D4924
        App.Options.EvalSiteURL = ""    ' D4924
        ' D4293 ==
        Select Case Data.Description.ToLower
            Case "welcome"
                sMessage = App.ActiveProject.PipeParameters.PipeMessages.GetWelcomeText(tType, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy) 'C0139 + D4293
                If sMessage = "" Then sMessage = ParseAllTemplates(File_GetContent(GetWelcomeThankYouIncFile(False, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType = ProjectType.ptOpportunities, IsRiskWithControls)), App.ActiveUser, App.ActiveProject) Else sMessage = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, "welcome" + sPostfix, sMessage, False, True, -1) ' D0131 + D0800 + D1669 + D2325 + D3326 + D4293
            Case "thankyou"
                ' D3964 ===
                If App.ActiveProject.ProjectManager.Parameters.EvalShowRewardThankYou AndAlso GetNextUnassessed(1) < 1 Then isReward = True
                sMessage = App.ActiveProject.PipeParameters.PipeMessages.GetThankYouText(If(isReward, PipeMessageKind.pmkReward, tType), App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy) 'C0139 + D4293
                ' D3972 ===
                If sMessage = "" Then
                    If isReward Then
                        sMessage = ParseAllTemplates(File_GetContent(GetIncFile(_FILE_TEMPL_THANKYOU_REWARD)), App.ActiveUser, App.ActiveProject)
                    Else
                        sMessage = ParseAllTemplates(File_GetContent(If(IsIntensities, GetIncFile(_FILE_TEMPL_THANKYOU_INTENSITIES), GetWelcomeThankYouIncFile(True, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType = ProjectType.ptOpportunities, IsRiskWithControls))), App.ActiveUser, App.ActiveProject)    ' D4293
                    End If
                Else
                    sMessage = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, "thankyou" + CStr(If(isReward, "_reward", "")) + sPostfix, sMessage, False, True, -1) ' D0131 + D0800 + D1669 + D1863 + D2325 + D3326 + D4293
                End If
                ' D3964 + D3972 ==
        End Select
        ' D0113 ==
        ' D3722 ===
        Dim sEdit As String = ""
        If CanUserEditActiveProject AndAlso Not IsReadOnly AndAlso IsPM Then    ' D3775 + D4116
            Dim sName As String = ResString(If(isReward, "lblEditRewardThankYou", If(Data.Description.ToLower = "welcome", "lblEditWelcome", "lblEditThankYou"))) ' D4297
            sEdit = String.Format("<a href='' onclick=""{0}""><img src='{1}icon_document.png' width=16 height=16 title='{2}' border=0 style='float:right; margin-top:7px; margin-right:2px;'></a>", PopupRichEditor(reObjectType.PipeMessage, CStr("&field=comment&id=" + Data.Description.ToLower + If(IsRiskWithControls, "&risk=1", "") + If(isReward, "&mode=reward", ""))), ImagePath, sName)   ' D3964 + D4293 + D4297
            sEdit += String.Format("<script type='text/javascript'><!--" + vbNewLine +
                                   "function onRichEditorRefresh(empty, infodoc, callback) {{ " + vbNewLine +
                                   "   S({0}); /* showLoadingPanel(); setTimeout('document.location.reload();', 150); */" + vbNewLine +
                                   "}} " + vbNewLine + "//--></script>", CurStep)    ' D3735 + D4114
        End If
        sMessage = CutHTMLHeaders(sMessage) ' D6727
        info.Text = String.Format("{1}<div id='divInfopage' class='text' style='text-align:left; margin-top:24px; padding:0px 1em; height:98%; z-index:1; overflow:auto;'>{0}</div>", ParseAllTemplates(sMessage, App.ActiveUser, App.ActiveProject), sEdit)   ' D0060 + D0075 + D0089 + D0220 + D4373 + D4377
        App.Options.EvalSiteURL = sEvalURL ' D4924
        ScriptManager.RegisterStartupScript(Me, GetType(String), "InitInfopage", "infoPageResize(); window.onresize=infoPageResize;", True) ' D4373
        ' D3722 ==
        Controls.Add(info)
    End Sub

    ' D0995 ===
    Private Function GetInfodocEditorCall(isAlt As Boolean, NodeID As Integer, param As String) As String
        Return PopupRichEditor(CType(IIf(isAlt, reObjectType.Alternative, reObjectType.Node), reObjectType), _PARAM_ID + "=" + NodeID.ToString + "&" + param)
    End Function
    ' D0995 ==

    ' D3610 ===
    Private Function GetWRTCaption(isCategory As Boolean) As String
        Dim sName As String = "lblShowDescriptionWRT"
        If isCategory Then
            sName = "lblShowDescriptionWRTCategory"
        Else
            If App.ActiveProject.IsRisk AndAlso Not App.ActiveProject.isImpact Then sName = "lblShowDescriptionWRTLikelihood"
        End If
        Return ResString(sName)
    End Function
    ' D3610 ==

    Private Sub CreatePairwise(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim tParams As New Dictionary(Of String, String)    ' D0120
        Dim Data As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
        Dim parentNode As clsNode = Nothing
        Dim H As clsHierarchy = Nothing
        Select Case Action.ActionType
            Case ActionType.atPairwise
                parentNode = If(Action.ParentNode Is Nothing, App.ActiveProject.HierarchyObjectives.GetNodeByID(Data.ParentNodeID), Action.ParentNode)    ' D7547
                H = If(parentNode.IsTerminalNode, App.ActiveProject.HierarchyAlternatives, App.ActiveProject.HierarchyObjectives)
            Case ActionType.atPairwiseOutcomes
                parentNode = Action.ParentNode
        End Select
        If parentNode Is Nothing Then parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(Data.ParentNodeID) ' D7546
        H = If(parentNode Is Nothing OrElse Not parentNode.IsAlternative AndAlso parentNode.IsTerminalNode OrElse parentNode.IsAlternative AndAlso parentNode.Children.Count > 0, App.ActiveProject.HierarchyAlternatives, App.ActiveProject.HierarchyObjectives)   ' D7546

        Dim pwBar As ctrlPairwiseBar = CType(LoadControl("~/ctrlPairwise.ascx"), ctrlPairwiseBar)  ' D0195 + D0995
        pwBar.sRootPath = _URL_ROOT ' D1593

        Dim tNodeLeft As New clsNode
        Dim tNodeRight As New clsNode

        Dim fIsPWOutcomes As Boolean = Action.ActionType = ActionType.atPairwiseOutcomes
        If fIsPWOutcomes Then
            App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(Action, Data, tNodeLeft, tNodeRight)
            tParams.Add(_TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.PWONode, True))
            pwBar.PWType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(Action.PWONode)  ' D2749
            ' D2996 ===
            Dim RS As clsRatingScale = Nothing  ' D2996
            If Action.ParentNode IsNot Nothing Then
                If Action.ParentNode.IsAlternative Then
                    RS = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                Else
                    If Action.ParentNode.ParentNode IsNot Nothing Then RS = CType(Action.ParentNode.ParentNode.MeasurementScale, clsRatingScale)
                End If
                If RS IsNot Nothing Then
                    Dim tRating As clsRating = RS.GetRatingByID(tNodeLeft.NodeGuidID)
                    If tRating IsNot Nothing Then pwBar.LeftNodeComment = tRating.Comment
                    tRating = RS.GetRatingByID(tNodeRight.NodeGuidID)
                    If tRating IsNot Nothing Then pwBar.RightNodeComment = tRating.Comment
                End If
            End If
            ' D2996 ==
        Else
            If H IsNot Nothing Then
                tNodeLeft = H.GetNodeByID(Data.FirstNodeID)
                tNodeRight = H.GetNodeByID(Data.SecondNodeID)
            End If
            pwBar.PWType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(parentNode)  ' D2722 + D2749
        End If

        'If parentNode IsNot Nothing Then tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(parentNode, True)) ' D0558
        If parentNode IsNot Nothing Then tParams.Add(_TEMPL_NODENAME, JS_SafeHTML(parentNode.NodeName)) ' D0558 + D4112
        If tNodeLeft IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tNodeLeft.NodeName))
        If tNodeRight IsNot Nothing Then tParams.Add(_TEMPL_NODE_B, JS_SafeHTML(tNodeRight.NodeName))
        ' D1956 ==

        Dim sCaption As String = ParseStringTemplates(ResString("lblEvaluationPWCaption"), tParams)
        Dim sFirstNode As String = ParseStringTemplates(ResString("lblEvaluationPWNodeA"), tParams)
        Dim sSecondNode As String = ParseStringTemplates(ResString("lblEvaluationPWNodeB"), tParams)

        sTask = GetPipeStepTask(Action, IntensityScale, isImpact AndAlso Not tNodeLeft.IsTerminalNode AndAlso Not tNodeRight.IsTerminalNode) ' D2316

        pwBar.ID = "PW"
        pwBar.Width = "90%"     ' D0256 + D0996
        pwBar.ImagePath = ImagePath  ' D0090 + D0121 + D1002
        ' D0049 + D0142 ===
        'If pwBar.PWType = PairwiseType.ptVerbal AndAlso (((parentNode.IsTerminalNode AndAlso App.ActiveProject.PipeParameters.ForceGraphicalForAlternatives) Or (Not parentNode.IsTerminalNode AndAlso App.ActiveProject.PipeParameters.ForceGraphical))) Then   ' D0194 + D1257 + D2749
        '    ' -D2739
        '    If fIsPWOutcomes Then
        '        Dim RS As clsRatingScale
        '        If parentNode.IsAlternative Then
        '            RS = CType(Action.PWONode.MeasurementScale, clsRatingScale)
        '        Else
        '            RS = CType(parentNode.ParentNode.MeasurementScale, clsRatingScale)
        '        End If
        '        ' D2152 ===
        '        If RS.RatingSet.Count <= 3 Then pwBar.PWType = PairwiseType.ptGraphical
        '        'If RS IsNot Nothing Then
        '        '    'pwBar.PWType = App.ActiveProject.ProjectManager.GetRatingScalePairwiseType(RS)
        '        '    If pwBar.PWType <> PairwiseType.ptGraphical AndAlso App.ActiveProject.ProjectManager.GetRatingScaleForceGraphical(RS) AndAlso RS.RatingSet.Count <= 3 Then pwBar.PWType = PairwiseType.ptGraphical
        '        'End If
        '        ' D2152 ==
        '    Else
        '        If parentNode.GetNodesBelow(Data.UserID).Count <= 3 Then pwBar.PWType = PairwiseType.ptGraphical 'C0460
        '    End If
        'End If
        'End If
        ' D0142 ==

        If Not isAllowMissingJudgments() And Not btnNext.Enabled And btnNext.CommandArgument <> "" Then   ' D0598
            pwBar.onChangeAction = If(pwBar.PWType = PairwiseType.ptGraphical, String.Format("theForm.{0}.disabled=(theForm.pwValue.value=={1}); ", btnNext.ClientID, ctrlPairwiseBar.pwUndefinedValue), String.Format("theForm.{0}.disabled=(theForm.pwValue.value<-8); ", btnNext.ClientID))    ' D0255 + D1863
            If btnNextUnas.Visible And btnNextUnas.CommandArgument <> "" Then pwBar.onChangeAction += String.Format("theForm.{1}.disabled = theForm.{0}.disabled; ", btnNext.ClientID, btnNextUnas.ClientID)
        End If
        ' D0163 ===
        pwBar.onChangeAction += String.Format("if (!theForm.{0}.disabled && isAutoAdvance(){1}) setTimeout('theForm.{0}.click();', 500);", btnNext.ClientID, IIf(pwBar.PWType = PairwiseType.ptGraphical, " && (gpw_autoadvance)", "")) ' D0255 + D0667 + D1486 + D1887
        ' D0163 ==

        ' D0049 ==
        Controls.Add(pwBar)
        ' D0106 ===

        ' D0120 + D0198 + D0995 ===
        pwBar.GoalName = sCaption
        pwBar.LeftNodeName = sFirstNode
        pwBar.RightNodeName = sSecondNode
        ' D4112 ===
        If parentNode IsNot Nothing Then
            tParams(_TEMPL_NODENAME) = GetWRTNodeNameWithPath(parentNode, True)
            pwBar.Caption = ParseStringTemplates(ResString("lblEvaluationPWCaption"), tParams)
        End If
        ' D0120 + D0198 + D0995 + D4112 ==

        pwBar.Comment = Data.Comment
        pwBar.lblCommentTitle = ResString("lblEvaluationPWComment")  ' D0120  + D1002
        pwBar.ShowComment = ShowComments AndAlso Not fIsPWOutcomes      ' D1938 + D1956
        pwBar.isCommentCollapsed = GetCookie("comment_status", If(pwBar.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D1069 + D2949
        pwBar.msgWrongNumber = ResString("msgGPWWrongNumber")   ' D1111 + D1126
        pwBar.msgWrongNumberPart = ResString("msgGPWWrongPart") ' D1116
        pwBar.msgPWExtreme = ResString("msgPWExtreme")          ' D1322
        pwBar.ShowPWExtremeWarning = GetCookie(_COOKIE_NOVICE_PWEXTREME, "") = ""     ' D1322

        pwBar.ImgScalePath = _URL_EVALUATE + "PWScale.aspx"     ' D0171
        Dim ImageScale As clsPairwiseData = CreateImageScale("")    ' D0216
        pwBar.PWImageMap = ImageScale.GetScaleMap(ctrlPairwiseBar.MapName, True) ' D0216 + D1395 + D1997

        pwBar.KnownLikelihoodA = -1
        pwBar.KnownLikelihoodB = -1
        If App.isRiskEnabled AndAlso parentNode IsNot Nothing AndAlso parentNode.MeasureType = ECMeasureType.mtPWAnalogous Then
            Dim L As List(Of KnownLikelihoodDataContract) = parentNode.GetKnownLikelihoods()
            If L IsNot Nothing Then
                For Each tLikelihood As KnownLikelihoodDataContract In L
                    If tLikelihood.Value >= 0 Then
                        If tLikelihood.ID = tNodeLeft.NodeID Then pwBar.KnownLikelihoodA = tLikelihood.Value
                        If tLikelihood.ID = tNodeRight.NodeID Then pwBar.KnownLikelihoodB = tLikelihood.Value
                    End If
                Next
                pwBar.KnownLikelihoodTitle = ResString(If(isImpact, "lblKnownImpactPW", "lblKnownLikelihoodPW"))   ' D3051
            End If
        End If

        If ShowInfoDocs Then    ' D1938 + D1956 + D4112
            ' D0131 + D1669 ===
            If parentNode IsNot Nothing Then pwBar.GoalInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(parentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), parentNode.NodeID.ToString, parentNode.InfoDoc, True, True, -1)    ' D0995 + D1004 + D7546
            If Not fIsPWOutcomes Then   ' D4112
                pwBar.LeftNodeInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tNodeLeft.IsAlternative, reObjectType.Alternative, reObjectType.Node), tNodeLeft.NodeID.ToString, tNodeLeft.InfoDoc, True, True, -1)
                pwBar.RightNodeInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tNodeRight.IsAlternative, reObjectType.Alternative, reObjectType.Node), tNodeRight.NodeID.ToString, tNodeRight.InfoDoc, True, True, -1)
            End If
            ' D0131 + D1669 ==
            ' D0204 ===
            'pwBar.ShowFramedInfodocs = Not IsIntensities AndAlso App.ActiveProject.PipeParameters.ShowInfoDocsMode = CheckVar("framed", ShowInfoDocsMode.sidmFrame) <> CInt(ShowInfoDocsMode.sidmFrame).ToString   ' D0205 + D1001 + D2292
            pwBar.ShowFramedInfodocs = CheckVar("framed", App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame)   ' D0205 + D1001 + D2292 + D4319
            If Not isHTMLEmpty(pwBar.GoalInfodoc) Then pwBar.GoalNodeInfodoc_URL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(parentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, parentNode.NodeID, parentNode.NodeGuidID, GetRandomString(10, True, False))) ' D0688 + D0787 + D0995 + D2419
            If Not fIsPWOutcomes Then   ' D4112
                If Not isHTMLEmpty(pwBar.LeftNodeInfodoc) Then pwBar.LeftNodeInfodoc_URL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(tNodeLeft.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tNodeLeft.NodeID, tNodeLeft.NodeGuidID, GetRandomString(10, True, False))) ' D0787 + D2419
                If Not isHTMLEmpty(pwBar.RightNodeInfodoc) Then pwBar.RightNodeInfodoc_URL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(tNodeRight.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tNodeRight.NodeID, tNodeRight.NodeGuidID, GetRandomString(10, True, False))) ' D0787 + D2419
            End If
            ' D0204 ==

            ' D0995 + D1002 ===
            pwBar.lblInfodocTitleGoal = ResString("lblShowDescriptionGoal")
            pwBar.lblInfodocTitleNode = ResString(CStr(IIf(tNodeLeft IsNot Nothing AndAlso Not tNodeLeft.IsAlternative, "lblShowDescriptionNode", "lblShowDescriptionAlt")))    ' D0997
            pwBar.lblInfodocTitleWRT = GetWRTCaption(parentNode Is Nothing OrElse parentNode.RiskNodeType = RiskNodeType.ntCategory)    ' D3031 + D3604 + D3610
            ' D0995 + D1002 ==

            ' D0997 ===
            pwBar.CanEditInfodocs = ShowInfoDocs AndAlso Not IsIntensities AndAlso CanUserEditActiveProject      ' D1063 + D4116
            'pwBar.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject      ' D1063 + D4116 + D4319
            pwBar.ImageEditInfodoc = "edit_tiny.gif"    ' D1002
            ' D1063 ===
            pwBar.ImageEditTitle = ResString("lblEditInfodoc")
            pwBar.ImageViewTitle = ResString("lblViewInfodoc")
            pwBar.ImageViewInfodoc = "view_sm.png"
            ' D1063 ==

            If parentNode IsNot Nothing Then
                ' D1004 + D1011 ===
                Dim fObjWRTObj As Boolean = Not parentNode.IsAlternative AndAlso tNodeLeft IsNot Nothing AndAlso Not tNodeLeft.IsAlternative    ' D2451
                pwBar.ShowWRTInfodocs = (parentNode.IsTerminalNode OrElse fObjWRTObj) AndAlso Not fIsPWOutcomes AndAlso Not IsIntensities   ' D1011 + D2451 + D4112 + D4319
                If pwBar.CanEditInfodocs Then
                    pwBar.GoalNodeInfodoc_EditURL = PopupRichEditor(CType(If(parentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), String.Format("field=infodoc&{0}={1}&guid={2}&callback=goal", _PARAM_ID, parentNode.NodeID, parentNode.NodeGuidID))
                    If Not fIsPWOutcomes Then   ' D4112
                        pwBar.LeftNodeInfodoc_EditURL = PopupRichEditor(If(tNodeLeft.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=left", _PARAM_ID, tNodeLeft.NodeID, tNodeLeft.NodeGuidID)) ' D1003 + D1054
                        pwBar.RightNodeInfodoc_EditURL = PopupRichEditor(If(tNodeLeft.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=right", _PARAM_ID, tNodeRight.NodeID, tNodeRight.NodeGuidID)) ' D1003 + D1054
                    End If
                End If
                ' D1011 ==
                If tNodeLeft IsNot Nothing AndAlso (tNodeLeft.IsAlternative OrElse fObjWRTObj) AndAlso pwBar.ShowWRTInfodocs Then   ' D1011 + D2451
                    pwBar.LeftNodeWRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tNodeLeft.NodeID.ToString, App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tNodeLeft.NodeGuidID, parentNode.NodeGuidID), False, True, parentNode.NodeID)  ' D1669
                    If Not isHTMLEmpty(pwBar.LeftNodeWRTInfodoc) Then pwBar.LeftNodeWRTInfodoc_URL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&guid={4}&pguid={5}&r={6}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tNodeLeft.NodeID, parentNode.NodeID, tNodeLeft.NodeGuidID, parentNode.NodeGuidID, GetRandomString(10, True, False))) ' D2451
                    If pwBar.CanEditInfodocs Then pwBar.LeftNodeWRTInfodoc_EditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=left_wrt", _PARAM_ID, tNodeLeft.NodeID, tNodeLeft.NodeGuidID, parentNode.NodeID, parentNode.NodeGuidID)) ' D1003
                End If
                If tNodeRight IsNot Nothing AndAlso (tNodeRight.IsAlternative OrElse fObjWRTObj) AndAlso pwBar.ShowWRTInfodocs Then    ' D1011 + D2451
                    pwBar.RightNodeWRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tNodeRight.NodeID.ToString, App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tNodeRight.NodeGuidID, parentNode.NodeGuidID), False, True, parentNode.NodeID)   ' D1669
                    If Not isHTMLEmpty(pwBar.RightNodeWRTInfodoc) Then pwBar.RightNodeWRTInfodoc_URL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&guid={4}&pguid={5}&r={6}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tNodeRight.NodeID, parentNode.NodeID, tNodeRight.NodeGuidID, parentNode.NodeGuidID, GetRandomString(10, True, False))) ' D2451
                    If pwBar.CanEditInfodocs Then pwBar.RightNodeWRTInfodoc_EditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=right_wrt", _PARAM_ID, tNodeRight.NodeID, tNodeRight.NodeGuidID, parentNode.NodeID, parentNode.NodeGuidID)) ' D1003
                End If
                ' D1004 ==
            End If
            ' D0997 ==

            ' D1097 ===
            If pwBar.ShowFramedInfodocs Then
                ' D0688 + D1004 + D1069 + D2949 ===
                pwBar.isInfodocGoalCollapsed = GetCookie("pw_goal_status", If(isHTMLEmpty(pwBar.GoalInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"     ' D0997 + D1064
                pwBar.isInfodocLeftCollapsed = GetCookie("pw_node_status", If(isHTMLEmpty(pwBar.LeftNodeInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"    ' D1064
                pwBar.isInfodocRightCollapsed = GetCookie("pw_node_status", If(isHTMLEmpty(pwBar.RightNodeInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0" ' D1064 + D1068
                pwBar.isInfodocLeftWRTCollapsed = GetCookie("pw_wrt_status", If(isHTMLEmpty(pwBar.LeftNodeWRTInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"   ' D0997 + D1064
                pwBar.isInfodocRightWRTCollapsed = GetCookie("wrt_status", If(isHTMLEmpty(pwBar.RightNodeWRTInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D0997 + D1064 + D1068
                ' D0688 + D1004 + D1069 + D2949 ==
                ' D1864 ===
                pwBar.SaveSizeMessage = ResString("msgOnSaveInfodocSize")
                pwBar.AllowSaveSize = CanUserEditActiveProject
                pwBar.SizesList = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
                ' D1864 ==
            End If
            ' D1097 ==

        End If
        ' D0106 ==
        ' D0031 ===
        pwBar.VerbalHints(0) = ResString("lblEvaluationPWHintEqual")
        pwBar.VerbalHints(1) = ResString("lblEvaluationPWHintModerately")
        pwBar.VerbalHints(2) = ResString("lblEvaluationPWHintStrongly")
        pwBar.VerbalHints(3) = ResString("lblEvaluationPWHintVeryStrongly")
        pwBar.VerbalHints(4) = ResString("lblEvaluationPWHintExtremely")    ' D1235 + D1318
        pwBar.VerbalHints(5) = ResString("lblEvaluationPWExtremelyDesc")    ' D1318
        pwBar.lblEraseJudgment = ResString("btnEvaluationResetValue") ' D0039 + D1002
        pwBar.VerbalHintBetween = ResString("lblEvaluationPWHintBetween")
        ' D0031 ==
        If parentNode Is Nothing OrElse parentNode.IsTerminalNode Then pwBar.GPWMode = App.ActiveProject.PipeParameters.GraphicalPWModeForAlternatives Else pwBar.GPWMode = App.ActiveProject.PipeParameters.GraphicalPWMode ' D2088 + D2091 + D2097 + D7546
        If Not TeamTimeFuncs.GPW_Mode_Allowed Then pwBar.GPWMode = TeamTimeFuncs.GPW_Mode_Default ' D2156
        'If Action.ActionType = ActionType.atPairwiseOutcomes Then pwBar.GPWMode = GraphicalPairwiseMode.gpwmLessThan9 ' D2092
        pwBar.Data = Data
        ' D2048 ===
        If pwBar.PWType = PairwiseType.ptVerbal AndAlso Not Data.IsUndefined AndAlso ((Data.Value Mod 1) <> 0 OrElse Data.Value > 9) Then
            pwBar.Data = New clsPairwiseMeasureData(Data.FirstNodeID, Data.SecondNodeID, Data.Advantage, Data.Value, Data.ParentNodeID, Data.UserID, Data.IsUndefined, Data.Comment)
            Dim Val As Double = Math.Round(Data.Value, 1)   ' D3010
            If Val > 9 Then Val = 9
            pwBar.Data.Value = Val
            pwBar.Message = String.Format("<div class='text note' style='margin-top:1ex;'><nobr>{0}</nobr></div>", ResString("msgGPWJudgment")) ' D3010
        End If
        ' D2048 ==
        isGPWStep = pwBar.PWType = PairwiseType.ptGraphical ' D3437
        If Not isGPWStep Then EnableAutoAdvance() ' D0667 + D3440
        pwBar.ChangeFlagID = isChanged.ClientID
        pwBar.Visible = True
    End Sub

    Public Function SavePairwiseData(ByRef Action As clsAction, sValue As String, sAdvantage As String, sComment As String, Optional ByVal UserID As Integer = Integer.MinValue) As clsPairwiseMeasureData 'C0403 + D1846
        Dim fChanged As Boolean = False ' D0106

        'C0403===
        Dim PWData As clsPairwiseMeasureData
        If UserID = Integer.MinValue Then
            PWData = CType(Action.ActionData, clsPairwiseMeasureData)
        Else
            Dim ActionPWData As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
            'Dim ParentNode As clsNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(ActionPWData.ParentNodeID)
            Dim ParentNode As clsNode = Action.ParentNode
            PWData = CType(ParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(ActionPWData.FirstNodeID, ActionPWData.SecondNodeID, UserID)
            If PWData Is Nothing Then
                PWData = New clsPairwiseMeasureData(ActionPWData.FirstNodeID, ActionPWData.SecondNodeID, 0, 0, ParentNode.NodeID, UserID, True)
            End If
        End If
        'C0403==

        ' D0144 ===
        'Dim sValue As String = CheckVar("pwValue", "")
        If Not String.IsNullOrEmpty(sValue) Then    ' D0026
            Dim val As Double = 0   ' D1858
            Dim adv As Integer = 0  ' D1846
            If String2Double(sValue, val) AndAlso Integer.TryParse(sAdvantage, adv) Then  ' D0147 + D1846
                ' D0144 ==
                If val = ctrlPairwiseBar.pwUndefinedValue Then  ' D0039 + D2082
                    If Not PWData.IsUndefined Then  ' D2082
                        PWData.IsUndefined = True
                        fChanged = True
                        JudgLogs = JudgUndefined    ' D7262
                    End If
                Else
                    If val = 0 OrElse adv = 0 Then
                        val = 1
                        adv = 0
                    End If
                    PWData.IsUndefined = False
                    If PWData.Value <> val OrElse PWData.Advantage <> adv Then
                        PWData.Value = val
                        PWData.Advantage = adv
                        fChanged = True
                        JudgLogs = GetPWLogVal(val, adv, App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(Action.ParentNode))    ' D7262
                    End If
                End If
            End If
        End If
        ' D0106 ===
        If ShowComments And Not sComment Is Nothing Then    ' D1938
            'Dim sComment As String = CheckVar("PWComment", PWData.Comment)
            If sComment <> PWData.Comment Then
                PWData.Comment = sComment
                fChanged = True
                JudgLogs += JudgComment ' D7262
            End If
        End If
        'If fChanged Then App.ActiveProject.ProjectManager.StorageManager.SaveJudgment(ActiveProject.HierarchyObjectives.GetNodeByID(PWData.ParentNodeID), PWData) ' D0039 'C0028
        If fChanged Then
            ' D2510 ===
            If IsRiskWithControls Then
                App.ActiveProject.ProjectManager.PipeBuilder.SaveControlJudgment(Action)
            Else
                If Action.ActionType = ActionType.atPairwiseOutcomes Then
                    Action.ParentNode.PWOutcomesJudgments.AddMeasureData(PWData)
                    ' D4420 ===
                    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(Action.PWONode, PWData)
                Else
                    Dim tNode As clsNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.ParentNodeID)
                    tNode.Judgments.AddMeasureData(PWData)
                    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(tNode, PWData) ' D0039 'C0028
                    ' D4420 ==
                End If
            End If
            ' D2510 ==
        End If
        ' D0106 ==

        Return PWData 'C0403
    End Function


    ' D1155 ===
    Private Sub CreateMultiPairwise(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim tParams As New Dictionary(Of String, String)
        Dim AllPW As ctrlMultiPairwise = CType(LoadControl("~/ctrlMultiPairwise.ascx"), ctrlMultiPairwise)
        AllPW.sRootPath = _URL_ROOT ' D1593
        AllPW.ChangeFlagID = isChanged.ClientID
        If Not isAllowMissingJudgments() And Not btnNext.Enabled And btnNext.CommandArgument <> "" Then
            AllPW.onChangeAction = String.Format("theForm.{0}.disabled=HasUndefined(); ", btnNext.ClientID)
            If btnNextUnas.Visible And btnNextUnas.CommandArgument <> "" Then AllPW.onChangeAction = String.Format("{{ {2} theForm.{1}.disabled = theForm.{0}.disabled; }}", btnNext.ClientID, btnNextUnas.ClientID, AllPW.onChangeAction)
        End If

        Dim fIsPWOutcomes As Boolean = Action.ActionType = ActionType.atAllPairwiseOutcomes ' D1956
        AllPW.isSecondNodeMode = fIsPWOutcomes ' D4337

        'AllPW.ShowFramedInfodocs = Not IsIntensities AndAlso App.ActiveProject.PipeParameters.ShowInfoDocsMode = CheckVar("framed", ShowInfoDocsMode.sidmFrame) <> CInt(ShowInfoDocsMode.sidmFrame).ToString  ' D1890 + D2292
        AllPW.ShowFramedInfodocs = CheckVar("framed", App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame)  ' D1890 + D2292 + D4319

        AllPW.KnownLikelihoodTitle = ResString(If(isImpact, "lblKnownImpactPW", "lblKnownLikelihoodPW"))   ' D2738 + D3051
        Dim isCategory As Boolean = False   ' D3604


        Dim UndefIDx As Integer = -1    ' D1438
        If TypeOf (Action.ActionData) Is clsAllPairwiseEvaluationActionData Then
            Dim data As clsAllPairwiseEvaluationActionData = CType(Action.ActionData, clsAllPairwiseEvaluationActionData)
            Dim fAlts As Boolean = data.ParentNode.IsTerminalNode   ' D1161
            isCategory = data.ParentNode.RiskNodeType = RiskNodeType.ntCategory ' D3604

            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.ParentNode, True))
            tParams.Add(_TEMPL_EVALCOUNT, data.Judgments.Count.ToString) ' D1163
            If fIsPWOutcomes AndAlso Action.ParentNode IsNot Nothing Then tParams.Add(_TEMPL_JUSTNODE, JS_SafeHTML(Action.ParentNode.NodeName)) ' D1956
            AllPW.Caption = ParseStringTemplates(ResString(CStr(IIf(fIsPWOutcomes, "lblEvaluationMultiPairwiseOutcomes", "lblEvaluationMultiPairwise"))), tParams)
            If fIsPWOutcomes AndAlso Action.ParentNode IsNot Nothing Then
                AllPW.CaptionName = Action.ParentNode.NodeName
            Else
                AllPW.CaptionName = data.ParentNode.NodeName ' D4115
            End If
            AllPW.CaptionName = JS_SafeHTML(AllPW.CaptionName)  ' D1374 + D4112 + D4115
            AllPW.ParentNodeID = data.ParentNode.NodeID.ToString  ' D1141

            'AllPW.PWType = IIf(fAlts, App.ActiveProject.PipeParameters.PairwiseTypeForAlternatives, App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseType(CurStep - 1))    ' D1165 + D1257 + D2717
            AllPW.PWType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(data.ParentNode)    ' D1165 + D1257 + D2717 + D2722 + D2726
            ' -D4708
            '' D1596 ===
            'If AllPW.PWType = PairwiseType.ptVerbal AndAlso data.Judgments.Count <= 3 Then
            '    If (fAlts AndAlso App.ActiveProject.PipeParameters.ForceGraphicalForAlternatives) OrElse (Not fAlts AndAlso App.ActiveProject.PipeParameters.ForceGraphical) Then AllPW.PWType = PairwiseType.ptGraphical
            'End If
            '' D1596 ==

            Dim L As List(Of KnownLikelihoodDataContract) = Nothing ' D2738
            If App.isRiskEnabled AndAlso data.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous Then L = data.ParentNode.GetKnownLikelihoods() ' D2738

            ' -D2739
            '' D2152 ===
            'If fIsPWOutcomes AndAlso Action.ParentNode IsNot Nothing Then
            '    Dim RS As clsRatingScale = Nothing
            '    If Action.ParentNode.IsAlternative Then
            '        RS = CType(Action.PWONode.MeasurementScale, clsRatingScale)
            '    Else
            '        If Action.ParentNode.ParentNode IsNot Nothing Then RS = CType(Action.ParentNode.ParentNode.MeasurementScale, clsRatingScale)
            '    End If
            '    ' D2152 ===
            '    If RS IsNot Nothing Then
            '        AllPW.PWType = App.ActiveProject.ProjectManager.GetRatingScalePairwiseType(RS)
            '        If AllPW.PWType <> PairwiseType.ptGraphical AndAlso App.ActiveProject.ProjectManager.GetRatingScaleForceGraphical(RS) AndAlso RS.RatingSet.Count <= 3 Then AllPW.PWType = PairwiseType.ptGraphical
            '    End If
            '    ' D2152 ==
            'End If
            '' D2152 ==

            ' D2989 ===
            Dim RS As clsRatingScale = Nothing
            If fIsPWOutcomes AndAlso Action.ParentNode IsNot Nothing Then
                If Action.ParentNode.IsAlternative Then
                    RS = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                Else
                    If Action.ParentNode.ParentNode IsNot Nothing Then RS = CType(Action.ParentNode.ParentNode.MeasurementScale, clsRatingScale)
                End If
            End If
            ' D2989 ==

            'AllPW.ShowWRTInfodocs = Not data.ParentNode.IsAlternative AndAlso data.ParentNode.IsTerminalNode
            AllPW.ShowWRTInfodocs = Not data.ParentNode.IsAlternative AndAlso Not fIsPWOutcomes AndAlso Not IsIntensities   ' D3597 + D4112 + D4319

            sTask = GetPipeStepTask(Action, IntensityScale) ' D2316
            Dim Lst As New List(Of clsPairwiseLine)
            Dim ID As Integer = 0
            Dim oID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("oid", "")) ' D1425 + Anti-XSS
            Dim aID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("aid", "")) ' D1425 + Anti-XSS
            For Each tJud As clsPairwiseMeasureData In data.Judgments
                Dim tLeftNode As clsNode = Nothing
                Dim tRightNode As clsNode = Nothing
                If fIsPWOutcomes Then
                    App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(Action, tJud, tLeftNode, tRightNode)
                Else
                    If fAlts Then
                        tLeftNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.FirstNodeID)
                        tRightNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.SecondNodeID)
                    Else
                        tLeftNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.FirstNodeID)
                        tRightNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.SecondNodeID)
                    End If
                End If

                If (tJud.FirstNodeID.ToString = oID AndAlso tJud.SecondNodeID.ToString = aID) Or
                   (tJud.FirstNodeID.ToString = aID AndAlso tJud.SecondNodeID.ToString = oID) Then
                    AllPW._FocusID = ID ' D1425
                    If IsIntensities AndAlso CheckVar("is_single", False) Then AllPW.ShowOnlyFocusedRow = True ' D2677
                End If

                ' D2738 ===
                Dim KnownLikelihoodA As Double = -1
                Dim KnownLikelihoodB As Double = -1

                If L IsNot Nothing Then
                    For Each tLikelihood As KnownLikelihoodDataContract In L
                        If tLikelihood.Value >= 0 Then
                            If tLikelihood.ID = tLeftNode.NodeID Then KnownLikelihoodA = tLikelihood.Value
                            If tLikelihood.ID = tRightNode.NodeID Then KnownLikelihoodB = tLikelihood.Value
                        End If
                    Next
                End If
                ' D2738 ==

                If tLeftNode IsNot Nothing AndAlso tRightNode IsNot Nothing Then
                    Dim PW As New clsPairwiseLine(ID, tLeftNode.NodeID, tRightNode.NodeID, tLeftNode.NodeName, tRightNode.NodeName, tJud.IsUndefined, tJud.Advantage, tJud.Value, tJud.Comment, KnownLikelihoodA, KnownLikelihoodB, SafeFormString(RR_GetScenario(tLeftNode)), SafeFormString(RR_GetScenario(tRightNode))) ' D2738 + D3041
                    ' D2989 ===
                    If fIsPWOutcomes AndAlso RS IsNot Nothing Then
                        Dim tRating As clsRating = RS.GetRatingByID(tLeftNode.NodeGuidID)
                        If tRating IsNot Nothing Then PW.LeftNodeComment = tRating.Comment
                        tRating = RS.GetRatingByID(tRightNode.NodeGuidID)
                        If tRating IsNot Nothing Then PW.RightNodeComment = tRating.Comment
                    End If
                    ' D2989 ==
                    If ShowInfoDocs AndAlso Not fIsPWOutcomes Then   ' D1890 + D1938 + D1956
                        InitInfodocs(ID * 2, tLeftNode, data.ParentNode, AllPW.ShowWRTInfodocs, PW.InfodocLeft, PW.InfodocLeftURL, PW.InfodocLeftEditURL, PW.InfodocLeftWRT, PW.InfodocLeftWRTURL, PW.InfodocLeftWRTEditURL, If(AllPW.ShowFramedInfodocs, "nodeleft", "alt"), If(AllPW.ShowFramedInfodocs, "wrtleft", "wrt"), IsIntensities)    ' D1161 + D1374 + D4319
                        InitInfodocs(ID * 2 + 1, tRightNode, data.ParentNode, AllPW.ShowWRTInfodocs, PW.InfodocRight, PW.InfodocRightURL, PW.InfodocRightEditURL, PW.InfodocRightWRT, PW.InfodocRightWRTURL, PW.InfodocRightWRTEditURL, If(AllPW.ShowFramedInfodocs, "noderight", "alt"), If(AllPW.ShowFramedInfodocs, "wrtright", "wrt"), IsIntensities) ' D1161 + D4319
                    End If
                    If UndefIDx = -1 AndAlso PW.isUndefined Then UndefIDx = ID ' D1438
                    Lst.Add(PW)
                    ID += 1
                End If
            Next
            AllPW.Data = Lst
            If UndefIDx >= 0 AndAlso AllPW._FocusID < 0 Then AllPW._FocusID = UndefIDx ' D1438 'A0845
            If AllPW._FocusID < 0 AndAlso AllPW.Data.Count > 0 Then AllPW._FocusID = 0 ' D1441

            If data.ParentNode.IsTerminalNode Then AllPW.GPWMode = App.ActiveProject.PipeParameters.GraphicalPWModeForAlternatives Else AllPW.GPWMode = App.ActiveProject.PipeParameters.GraphicalPWMode ' D2091 + D2097
            If Not TeamTimeFuncs.GPW_Mode_Allowed Then AllPW.GPWMode = TeamTimeFuncs.GPW_Mode_Default ' D2156
            'If Action.ActionType = ActionType.atAllPairwiseOutcomes Then AllPW.GPWMode = GraphicalPairwiseMode.gpwmLessThan9 ' D2092
            If ShowInfoDocs Then    ' D1938 + D1956
                ' D4112 ===
                Dim tParent As clsNode = If(fIsPWOutcomes, Action.ParentNode, data.ParentNode)
                AllPW.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.Node, tParent.NodeID.ToString, tParent.InfoDoc, True, True, -1) ' D1669
                AllPW.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(tParent.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tParent.NodeID, tParent.NodeGuidID, GetRandomString(6, False, False)))    ' D2419
                AllPW.CaptionInfodocEditURL = PopupRichEditor(If(tParent.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, tParent.NodeID, tParent.NodeGuidID))  ' D2419
                ' D4122 ==

                ' D4337 ===
                If fIsPWOutcomes AndAlso data.ParentNode IsNot Nothing AndAlso data.ParentNode IsNot tParent Then   ' D4425
                    AllPW.SecondNodeName = data.ParentNode.NodeName
                    AllPW.SecondNodeInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.Node, data.ParentNode.NodeID.ToString, data.ParentNode.InfoDoc, True, True, -1)
                    AllPW.SecondNodeInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(data.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, data.ParentNode.NodeID, data.ParentNode.NodeGuidID, GetRandomString(6, False, False)))
                    AllPW.SecondNodeInfodocEditURL = PopupRichEditor(If(data.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=noderight", _PARAM_ID, data.ParentNode.NodeID, data.ParentNode.NodeGuidID))
                End If
                ' D4337 ==

                ' D1165 ===
                If AllPW.ShowFramedInfodocs Then

                    ' D1438 ===
                    Dim fHasInfodoc As Boolean = Not isHTMLEmpty(AllPW.CaptionInfodoc)
                    If AllPW._FocusID >= 0 AndAlso Not fHasInfodoc Then fHasInfodoc = Not isHTMLEmpty(AllPW.Data(AllPW._FocusID).InfodocLeft) OrElse Not isHTMLEmpty(AllPW.Data(AllPW._FocusID).InfodocRight)
                    AllPW.isInfodocCaptionCollapsed = GetCookie("mp_status", If(fHasInfodoc AndAlso Not App.isMobileBrowser, "1", "0")) = "0"    ' D1374 + D2949

                    fHasInfodoc = False
                    If AllPW._FocusID >= 0 Then fHasInfodoc = Not isHTMLEmpty(AllPW.Data(AllPW._FocusID).InfodocLeftWRT) OrElse Not isHTMLEmpty(AllPW.Data(AllPW._FocusID).InfodocRightWRT)
                    AllPW.InfodocsCollapsed = GetCookie("mpwrt_status", If(fHasInfodoc AndAlso Not App.isMobileBrowser, "1", "0")) = "0"  ' D1374 + D2949
                    ' D1438 ==

                    ' D1864 ===
                    AllPW.SaveSizeMessage = ResString("msgOnSaveInfodocSize")
                    AllPW.AllowSaveSize = CanUserEditActiveProject
                    AllPW.SizesList = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
                    ' D1864 ==

                End If
                ' D1165 ==
            End If

        End If

        With AllPW  ' D1158
            .ImagePath = ImagePath
            .CanEditInfodocs = ShowInfoDocs AndAlso Not IsIntensities AndAlso CanUserEditActiveProject ' D1890 + D1938 + D2292 + D4112
            '.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject ' D1890 + D1938 + D2292 + D4112 + D4319
            .AllowSaveSize = .CanEditInfodocs AndAlso Not IsIntensities     ' D4319
            .ImageEditInfodoc = "edit_tiny.gif"
            .ImageViewInfodoc = "view_sm.png"   ' D1426
            .lblInfodocTitleGoal = ResString("lblShowDescriptionFor")
            .lblInfodocTitleNode = ResString("lblShowDescriptionFor")
            .lblInfodocTitleWRT = GetWRTCaption(isCategory) ' D3031 + D3604 + D3610
            .ShowComment = ShowComments AndAlso Not fIsPWOutcomes     ' D1938 + D1956
            .lblCommentTitle = ResString("lblEvaluationPWComment") ' D1012
            .isCommentCollapsed = GetCookie("comment_status", If(AllPW.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"   ' D2949
            .CaptionSaveComment = ResString("btnOK")        ' D2344
            .CaptionCloseComment = ResString("btnClose")    ' D1161
            .ImageCommentExists = ImagePath + "comment.png"  ' D2769
            .ImageCommentEmpty = ImagePath + "comment_e.png" ' D2769
            .ShowFloatLegend = True 'IsIntensities    ' D4320

            ' D1158 ===
            .BlankImage = BlankImage

            .VerbalHints(0) = ResString("lblEvaluationPWHintEqual")
            .VerbalHints(1) = ResString("lblEvaluationPWHintModerately")
            .VerbalHints(2) = ResString("lblEvaluationPWHintStrongly")
            .VerbalHints(3) = ResString("lblEvaluationPWHintVeryStrongly")
            .VerbalHints(4) = ResString("lblEvaluationPWHintExtremely")
            .VerbalHints(5) = ResString("lblEvaluationPWExtremelyDesc")    ' D1367

            .VerbalShortHints(0) = ResString("lblEvaluationPWShortHintEqual")
            .VerbalShortHints(1) = ResString("lblEvaluationPWShortHintModerately")
            .VerbalShortHints(2) = ResString("lblEvaluationPWShortHintStrongly")
            .VerbalShortHints(3) = ResString("lblEvaluationPWShortHintVeryStrongly")
            .VerbalShortHints(4) = ResString("lblEvaluationPWShortHintExtremely")

            .VerbalHintBetween = ResString("lblEvaluationPWHintBetween")
            ' D1158 ==

            ' D7566 ===
            If PM.Parameters.SpecialMode.ToLower = _OPT_MODE_AREA_VALIDATION2 Then
                Dim sHints As String() = ResString("lblEvaluationPW_AreaValidation2").Split(CChar(vbCr))
                Dim h_cnt = sHints.Count
                If h_cnt > 5 Then h_cnt = 5
                For i As Integer = 0 To h_cnt - 1
                    Dim sHintRow As String() = sHints(i).Trim(CChar(vbLf)).Trim().Split(CChar(vbTab))
                    If sHintRow.Count > 0 Then
                        If sHintRow.Count > 1 Then
                            .VerbalShortHints(i) = sHintRow(0)
                            .VerbalHints(i) = sHintRow(1)
                        Else
                            .VerbalHints(i) = sHintRow(0)
                        End If
                    End If
                Next
                .VerbalHints(5) = ResString("lblEvaluationPWExtremelyDesc_AV2")
                .LegendReverseOrder = True
            End If
            ' D7566 ==

            Dim ImageScale As clsPairwiseData = CreateImageScale("tiny")    ' D1163
            .PWImageMap = ImageScale.GetScaleMap(pwMapName, False)     ' D1163 + D1395 + D1997

            ' D1165 ===
            .msgWrongNumber = ResString("msgGPWWrongNumber")
            .msgWrongNumberPart = ResString("msgGPWWrongPart")
            ' D1165 ==

            .Message = String.Format("<div style='margin-bottom:1ex'><span class='text note'>{0}</span></div>", ResString("msgGPWJudgment"))    ' D2048

        End With
        btnSave.Visible = False  ' D2154
        isGPWStep = AllPW.PWType = PairwiseType.ptGraphical ' D3437

        'If Not IsReadOnly AndAlso Not App.ActiveProject.PipeParameters.DisableWarningsOnStepChange Then ClientScript.RegisterStartupScript(GetType(String), "HookSubmit", "_postback_custom_code = 'confirmMissingJudgments();'; HookSubmit();", True) ' D2950

        Controls.Add(AllPW)
    End Sub

    ' D1161 ===
    Private Sub InitInfodocs(tID As Integer, tNode As clsNode, tParentNode As clsNode, fShowWRT As Boolean, ByRef sInfodoc As String, ByRef sInfodocURL As String, ByRef sInfodocEditURL As String, ByRef sInfodocWRT As String, ByRef sInfodocWRTURL As String, ByRef sInfodocWRTEditURL As String, sInfodocObjName As String, sWRTObjName As String, Optional isPlainPromt As Boolean = False)    ' D4319
        If tNode IsNot Nothing Then
            sInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, CType(IIf(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), tNode.NodeID.ToString, tNode.InfoDoc, True, True, -1)    ' D1669
            sInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(CType(IIf(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType)), _PARAM_ID, tNode.NodeID, tNode.NodeGuidID, GetRandomString(6, False, False)))  ' D2419
            Dim fCanEdit As Boolean = CanUserEditActiveProject  ' D1448 + D4116
            If fCanEdit Then
                'If isPlainPromt Then
                '    sInfodocEditURL = String.Format("editText('{0}','{1}',['{2}']);", JS_SafeString(String.Format(ResString("promtEditText"), ShortString(tNode.NodeName, 30))), JS_SafeString(sInfodoc), JS_SafeString(sInfodocObjName))   ' D4319
                'Else
                sInfodocEditURL = PopupRichEditor(CType(IIf(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), String.Format("field=infodoc&{0}={1}&guid={2}&callback={4}{3}", _PARAM_ID, tNode.NodeID, tNode.NodeGuidID, tID, sInfodocObjName)) ' D1448
                'End If
            End If
            sInfodocWRT = ""
            sInfodocWRTURL = ""
            sInfodocWRTEditURL = ""
            If fShowWRT AndAlso tParentNode IsNot Nothing Then
                sInfodocWRT = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tNode.NodeID.ToString, App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tNode.NodeGuidID, tParentNode.NodeGuidID), False, True, tParentNode.NodeID)   ' D1669
                sInfodocWRTURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&guid={4}&pguid={5}&r={6}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tNode.NodeID, tParentNode.NodeID, tNode.NodeGuidID, tParentNode.NodeGuidID, GetRandomString(6, False, False))) ' D1455 + D2419
                If fCanEdit Then sInfodocWRTEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback={6}{5}", _PARAM_ID, tNode.NodeID, tNode.NodeGuidID, tNode.NodeGuidID, tParentNode.NodeGuidID, tID, sWRTObjName)) ' D1448
            End If
        End If
    End Sub
    ' D1161 ==

    ' D7262 ===
    Private Function GetPWLogVal(Val As Double, adv As Integer, pwtype As PairwiseType) As String
        Val = Math.Abs(Val)
        If Val = 1 Then
            Return ResString("lblEvaluationPWHintEqual")
        Else
            If pwtype = PairwiseType.ptVerbal AndAlso Math.Round(Val) = Val AndAlso Val <= 9 Then
                Dim Words As String() = {ResString("lblEvaluationPWShortHintEqual"), ResString("lblEvaluationPWShortHintModerately"), ResString("lblEvaluationPWShortHintStrongly"), ResString("lblEvaluationPWShortHintVeryStrongly"), ResString("lblEvaluationPWShortHintExtremely")}
                Dim idx = CInt(Val) - 1
                If (idx Mod 2) = 1 Then
                    Return String.Format("{0}({1}-{2})", If(adv > 0, "-", ""), Words(idx >> 1), Words((idx + 1) >> 1))
                Else
                    Return String.Format("{0}{1}", If(adv > 0, "-", ""), Words(idx >> 1))
                End If

            Else
                Return String.Format(If(adv > 0, "{0}: 1", "1:{0}"), Double2String(Val, 2))
            End If
        End If
    End Function
    ' D7262 ==

    Protected Sub SaveMultiPairwiseData(ByRef Action As clsAction)
        ' D1161 ===
        If TypeOf (Action.ActionData) Is clsAllPairwiseEvaluationActionData Then
            Dim data As clsAllPairwiseEvaluationActionData = CType(Action.ActionData, clsAllPairwiseEvaluationActionData)
            Dim Value As Double ' D1858
            JudgLogs = ""    ' D7262
            Dim PWType As PairwiseType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(data.ParentNode)   ' D7262
            ' D3041 ===
            For ID As Integer = 0 To data.Judgments.Count - 1

                Dim sIDLeft As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("id_left{0}", ID), ""))  'Anti-XSS
                Dim sIDRight As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("id_right{0}", ID), ""))    'Anti-XSS

                For Each tJud As clsPairwiseMeasureData In data.Judgments
                    If tJud.FirstNodeID.ToString = sIDLeft AndAlso tJud.SecondNodeID.ToString = sIDRight Then   ' D3041
                        Dim fUpdate As Boolean = False
                        Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("value{0}", ID), "")) 'Anti-XSS
                        Dim sAdv As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("adv{0}", ID), ""))  ' D2023 + Anti-XSS
                        If sValue = "" Then
                            fUpdate = Not tJud.IsUndefined  ' D1331
                            tJud.IsUndefined = True
                            JudgLogs += If(JudgLogs = "", "", "; ") + JudgUndefined     ' D7262
                        Else
                            If String2Double(sValue, Value) Then    ' D1858
                                ' D2023 ===
                                If Math.Abs(Value) >= Math.Abs(TeamTimeFuncs.UndefinedValue) Then
                                    If Not tJud.IsUndefined Then
                                        tJud.IsUndefined = True
                                        fUpdate = True
                                        JudgLogs += If(JudgLogs = "", "", "; ") + JudgUndefined   ' D7262
                                    End If
                                Else
                                    Dim Adv As Integer
                                    If Integer.TryParse(sAdv, Adv) AndAlso (tJud.IsUndefined OrElse tJud.Value.ToString("F6") <> Value.ToString("F6") OrElse tJud.Advantage <> Adv) Then    ' D2031
                                        tJud.IsUndefined = False
                                        tJud.Value = Math.Abs(Value)
                                        tJud.Advantage = Adv
                                        fUpdate = True
                                    End If
                                    JudgLogs += If(JudgLogs = "", "", "; ") + getPWLogVal(Value, Adv, PWType) ' D7262
                                End If
                                ' D2023 ==
                                'If Value = 0 Then
                                '    fUpdate = tJud.IsUndefined Or tJud.Value <> 1 Or tJud.Advantage <> 0    ' D1331
                                '    tJud.Value = 1
                                '    tJud.Advantage = 0
                                '    tJud.IsUndefined = False
                                'Else
                                '    ' D1331 ===
                                '    If Math.Abs(Value) > 99 Then
                                '        fUpdate = Not tJud.IsUndefined
                                '        tJud.IsUndefined = True
                                '    Else
                                '        ' D1331 ==
                                '        If (Math.Abs(Value) < 0.001) Then Value = 0
                                '        tJud.Value = Math.Abs(Value) + 1
                                '        tJud.Advantage = CInt(IIf(Value < 0, -1, 1))
                                '        tJud.IsUndefined = False
                                '        fUpdate = True
                                '    End If
                                'End If
                            End If
                        End If
                        Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("Comment{0}", ID), tJud.Comment))   'Anti-XSS
                        If sComment <> tJud.Comment Then
                            tJud.Comment = sComment
                            fUpdate = True
                            JudgLogs += JudgComment ' D7262
                        End If

                        If fUpdate Then
                            If Action.ActionType = ActionType.atAllPairwiseOutcomes Then
                                If Action.PWONode IsNot Nothing Then Action.PWONode.PWOutcomesJudgments.AddMeasureData(tJud)
                                If Action.PWONode IsNot Nothing AndAlso Action.ParentNode IsNot Nothing Then Action.ParentNode.PWOutcomesJudgments.AddMeasureData(tJud)
                                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(Action.PWONode, tJud)   ' D4420
                            Else
                                App.ActiveProject.HierarchyObjectives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(tJud)
                                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, tJud)  ' D4420
                            End If
                        End If

                        Exit For ' D3041
                    End If
                Next
            Next
        End If
        ' D1161 ==
    End Sub
    ' D1155 ==

    '' D2120 ===
    'Private Function GetPrecisionForRatings(tScale As clsRatingScale, DefPrecision As Integer) As Integer
    '    Dim sPrec As Integer = DefPrecision    ' D3782
    '    If tScale IsNot Nothing Then
    '        For Each tIntens As clsRating In tScale.RatingSet
    '            If tIntens.Value <= 1 Then
    '                ' D4391 ===
    '                'Dim tLen As Integer = Math.Abs(tIntens.Value Mod 1).ToString.Length - 2
    '                Dim sVal As String = JS_SafeNumber(tIntens.Value * 100)
    '                Dim tIdx As Int16 = sVal.IndexOf(".") + 1
    '                If tIdx > 0 Then
    '                    Dim tLen As Integer = sVal.Length - tIdx
    '                    If tLen > sPrec Then sPrec = tLen
    '                End If
    '                ' D4391 ==
    '            End If
    '        Next
    '        'If sPrec > 4 AndAlso sPrec > DefPrecision + 1 Then sPrec = 4 ' D3782
    '        If sPrec > 6 Then sPrec = 6 ' D3782 + D4391
    '        If sPrec < 1 Then sPrec = 1 ' D3782
    '    End If
    '    Return sPrec
    'End Function
    '' D2120 ==

    ' D0038 ===
    Private Sub CreateRating(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim Data As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData) 'Cv2 'C0464

        Dim tParams As New Dictionary(Of String, String)    ' D0120

        Dim tAlt As clsNode = Nothing
        Dim tParentNode As clsNode = Nothing
        If IsRiskWithControls Then
            tAlt = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Data.Assignment.ObjectiveID)
            If tAlt Is Nothing Then tAlt = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Data.Assignment.ObjectiveID) ' D4347
        Else
            tAlt = App.ActiveProject.HierarchyAlternatives.GetNodeByID(CType(Data.Judgment, clsNonPairwiseMeasureData).NodeID)
            tParentNode = Data.Node
        End If

        If tAlt IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, JS_SafeHTML(tAlt.NodeName))
        If tParentNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_B, GetWRTNodeNameWithPath(tParentNode, True)) ' D0558

        sTask = GetPipeStepTask(Action, IntensityScale) ' D2316 + D2407

        Dim Rating As ctrlRating = CType(LoadControl("~/ctrlRating.ascx"), ctrlRating)  ' D0995
        Rating.sRootPath = _URL_ROOT ' D1593
        Rating.ID = "Rating"    ' D1239
        Rating.Data = Data
        ' D2510 ===
        Dim MScale As clsMeasurementScale = Nothing
        If IsRiskWithControls Then MScale = App.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(Data.Assignment.MeasurementScaleGuid) Else MScale = Data.MeasurementScale
        Rating.MeasurementScale = MScale
        If MScale IsNot Nothing Then
            'Rating.Precision = GetPrecisionForRatings(CType(MScale, clsRatingScale), Rating.Precision) ' D2120 + D3782
            Rating.ShowDirectValue = App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(MScale.GuidID)   ' D4170
            Rating.ShowPrtyValues = Rating.ShowDirectValue ' D4170
        End If
        ' D2510 ==
        ' D0049 ===
        If Not isAllowMissingJudgments() And Not btnNext.Enabled And btnNext.CommandArgument <> "" Then   ' D0598
            Rating.onChangeAction = String.Format("theForm.{0}.disabled=(ID==-1 || ID===''); ", btnNext.ClientID)
            If btnNextUnas.Visible And btnNextUnas.CommandArgument <> "" Then Rating.onChangeAction += String.Format("theForm.{1}.disabled = theForm.{0}.disabled; ", btnNext.ClientID, btnNextUnas.ClientID)
        End If
        ' D0163 ===
        EnableAutoAdvance()
        Rating.onChangeAction += String.Format("if (isAutoAdvance() && !theForm.{0}.disabled && ID>=0) theForm.{0}.click();", btnNext.ClientID)  ' D1313
        ' D0163 ==
        ' D0049 ==
        Controls.Add(Rating)
        If tAlt IsNot Nothing Then Rating.ParentNodeID = tAlt.NodeID.ToString ' D2505 + D3068
        If Data.Node IsNot Nothing Then Rating.ObjectiveName = Data.Node.NodeName ' D1084
        ' D0120 ===
        'sTask = GetPipeStepHint(Action, IntensityScale)    ' D2258 + D2316 + D2516
        Rating.Width = CStr(IIf(Rating.ShowFramedInfodocs, "80%", "60%"))    ' D0689 + D1006

        Rating.ImagePath = ImagePath ' D0689
        Rating.ImageBlank = BlankImage  ' D0689
        Rating.Width = "75%"    ' D0689
        If tAlt IsNot Nothing Then Rating.AlternativeName = tAlt.NodeName ' D1003

        ' D0120 ==
        If ShowInfoDocs Then    ' D1938
            Rating.lblInfodocTitleGoal = ResString("lblShowDescriptionFor") ' D0689 + D1003
            Rating.lblInfodocTitleNode = ResString("lblShowDescriptionFor") ' D1003
            Rating.lblInfodocTitleWRT = GetWRTCaption(tParentNode Is Nothing OrElse tParentNode.RiskNodeType = RiskNodeType.ntCategory) ' D1003 + D3031 + D3604 + D3610
            Rating.lblInfodocTitleScale = ResString("lblShowDescriptionScale") ' D2249
            ' D0241 ===
            If MScale IsNot Nothing Then
                Rating.ScaleInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.MeasureScale, MScale.GuidID.ToString, MScale.Comment, True, True, -1) ' D2250
                Rating.ScaleInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&scale={3}&r={4}", CInt(reObjectType.MeasureScale), _PARAM_ID, MScale.GuidID.ToString, CInt(Data.MeasurementType), GetRandomString(10, True, False))) ' D2250
            End If
            Dim fCanShowWRT As Boolean = True ' D2411
            If Not tAlt Is Nothing Then
                Rating.AlternativeName = tAlt.NodeName ' D1003
                If ShowInfoDocs Then
                    Rating.AlternativeInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), tAlt.NodeID.ToString, tAlt.InfoDoc, True, True, -1) ' D0689 + D1669 + D2419
                    ' D4346 ===
                    Dim sPID As String = ""
                    If Data.Node IsNot Nothing Then sPID = Data.Node.NodeID.ToString
                    Rating.AlternativeInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&r={4}", CInt(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tAlt.NodeID, sPID, GetRandomString(10, True, False))) ' D1064 + D2419
                    ' D4346 ==
                    fCanShowWRT = Data.Node IsNot Nothing AndAlso tAlt.NodeGuidID.ToString <> Data.Node.NodeGuidID.ToString  ' D2411
                    If fCanShowWRT Then ' D2411
                        Rating.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(Data.Node.IsAlternative, reObjectType.Alternative, reObjectType.Node), Data.Node.NodeID.ToString, Data.Node.InfoDoc, True, True, -1) ' D0107 + D0131 + D1669
                        Rating.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(Data.Node.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, Data.Node.NodeID, GetRandomString(10, True, False))) ' D0787
                        Dim sWRTInfodoc As String = App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, Data.Node.NodeGuidID)  ' D1003
                        Rating.WRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tAlt.NodeID.ToString, sWRTInfodoc, False, True, Data.Node.NodeID) ' D1003 + D1669
                        Rating.WRTInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&r={4}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tAlt.NodeID, Data.Node.NodeID, GetRandomString(10, True, False))) ' D1003
                    End If
                    ' D4346 ===
                    If IsRiskWithControls AndAlso Data.Control IsNot Nothing Then
                        Rating.ObjectiveName = Data.Control.Name
                        Rating.CaptionInfodoc = GetControlInfodoc(App.ActiveProject, Data.Control, False)
                        Rating.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid={1}&r={2}", CInt(reObjectType.Control), Data.Control.ID, GetRandomString(10, True, False)))
                        Rating.CaptionInfodocEditURL = PopupRichEditor(reObjectType.Control, String.Format("field=infodoc&guid={0}&callback=node", Data.Control.ID))
                    End If
                    ' D4346 ==
                    ' D4347 ===
                    If Data.Assignment IsNot Nothing AndAlso Not Data.Assignment.EventID.Equals(Guid.Empty) Then
                        Dim tNode As clsNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(Data.Assignment.EventID)
                        If tNode IsNot Nothing Then
                            Rating.WRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), tNode.NodeID.ToString, tNode.InfoDoc, True, True, -1)
                            Rating.WRTInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tNode.NodeID, GetRandomString(10, True, False)))
                            Rating.WRTInfodocEditURL = PopupRichEditor(If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=wrt", _PARAM_ID, tNode.NodeID, tNode.NodeGuidID))
                            Rating.lblInfodocTitleWRT = tNode.NodeName
                        End If
                    End If
                    ' D4347 ==
                    Rating.ShowFramedInfodocs = CheckVar("framed", App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame)   ' D1003
                    'If Rating.ShowFramedInfodocs And Rating.AlternativeInfodoc <> "" Then    ' D0251
                    If Rating.ShowFramedInfodocs Then    ' D0251 + D1084
                        Rating.AlternativeInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, GetRandomString(10, True, False)))   ' D0787 + D4347
                        ' D0689 + D2949 ===
                        Rating.AlternativeInfodocCollapsed = GetCookie("alt_status", If(isHTMLEmpty(Rating.AlternativeInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0" ' D1006 + D1064 + D1069
                        Rating.CaptionInfodocCollapsed = GetCookie("node_status", If(isHTMLEmpty(Rating.CaptionInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"    ' D1006 + D1064 + D1069
                        Rating.WRTInfodocCollapsed = GetCookie("wrt_status", If(isHTMLEmpty(Rating.WRTInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D1003 + D1006 + D1064 + D1069
                        Rating.ScaleInfodocCollapsed = GetCookie("scale_status", If(Rating.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D2250
                        ' D0689 + D2949 ==
                        ' D1864 ===
                        Rating.SaveSizeMessage = ResString("msgOnSaveInfodocSize")
                        Rating.AllowSaveSize = CanUserEditActiveProject
                        Rating.SizesList = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
                        ' D1864 ==
                    End If

                    If fCanShowWRT AndAlso Data.Node IsNot Nothing Then Rating.CaptionInfodocEditURL = PopupRichEditor(If(Data.Node.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, Data.Node.NodeID, Data.Node.NodeGuidID)) ' D1006 + D2411
                    If tAlt IsNot Nothing Then Rating.AlternativeInfodocEditURL = PopupRichEditor(If(Data.Node IsNot Nothing AndAlso Data.Node.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID)) ' D1006 + D2419
                    If fCanShowWRT AndAlso tAlt IsNot Nothing AndAlso Data.Node IsNot Nothing Then Rating.WRTInfodocEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=wrt", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, Data.Node.NodeID, Data.Node.NodeGuidID)) ' D1006 + D2411
                    If MScale IsNot Nothing Then Rating.ScaleInfodocEditURL = PopupRichEditor(reObjectType.MeasureScale, String.Format("{0}={1}&scale={2}&callback=scale", _PARAM_ID, MScale.GuidID.ToString, CInt(Data.MeasurementType))) ' D2250
                End If
            End If
            ' D0241 ==

            ' D1003 ===
            Rating.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject ' D1063 + D2505 + D4116
            Rating.ImageEditInfodoc = "edit_tiny.gif"
            ' D1003 ==

            'If Not Rating.ShowFramedInfodocs AndAlso tAlt IsNot Nothing Then Rating.Caption = GetWRTNodeName(tAlt) ' D2407
        End If

        Rating.ImageViewInfodoc = "view_sm.png"
        Rating.Comment = CType(Data.Judgment, clsNonPairwiseMeasureData).Comment    ' D0108
        Rating.lblCommentTitle = ResString("lblEvaluationPWComment")  ' D0120 + D1003
        Rating.ShowComment = ShowComments    ' D0108 + D1938
        Rating.isCommentCollapsed = GetCookie("comment_status", If(Rating.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D1069 + D2949
        ' D0039 ===
        Rating.NotRatedCaption = ResString("lblEvaluationNotRated")
        Rating.ChangeFlagID = isChanged.ClientID
        ' D0039 ==
        Rating.msgDirectRatingValue = ResString("msgDirectRating")  ' D1748
        Rating.lblDirectValue = ResString("lblDirectValueRating")   ' D1748
    End Sub

    Protected Function SaveRatingData(ByRef Action As clsAction, RatingID As String, sComment As String, Optional ByVal UserID As Integer = Integer.MinValue) As clsRatingMeasureData 'C0403
        Dim fChanged As Boolean = False ' D0108
        If Action.ActionType = ActionType.atNonPWOneAtATime Then 'Cv2
            Dim Data As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData) 'Cv2 'C0464

            'Dim RatingData As clsNonPairwiseMeasureData = CType(Data.Judgment, clsNonPairwiseMeasureData)   ' D108 'C0403
            'C0403===
            Dim RatingData As clsNonPairwiseMeasureData
            If UserID = Integer.MinValue Then
                RatingData = CType(Data.Judgment, clsNonPairwiseMeasureData)
            Else
                RatingData = CType(Data.Node.Judgments, clsNonPairwiseJudgments).GetJudgement(CType(Data.Judgment, clsNonPairwiseMeasureData).NodeID, Data.Node.NodeID, UserID)
                If RatingData Is Nothing Then
                    RatingData = New clsRatingMeasureData(CType(Data.Judgment, clsNonPairwiseMeasureData).NodeID, Data.Node.NodeID, UserID, Nothing, CType(Data.Node.MeasurementScale, clsRatingScale), True)
                End If
            End If
            'C0403==

            Dim MScale As clsMeasurementScale = Nothing ' D2510
            If IsRiskWithControls Then MScale = App.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(Data.Assignment.MeasurementScaleGuid) Else MScale = Data.MeasurementScale ' D2510

            'Dim ID As String = Request("RatingValue")  ' - D0195
            If Not String.IsNullOrEmpty(RatingID) Then
                If RatingID = "-1" Then  ' D0200
                    RatingData.IsUndefined = True 'Cv2 + D0108
                    JudgLogs = JudgUndefined    ' D7262
                Else
                    If MScale IsNot Nothing Then    ' D2570
                        For Each tR As clsRating In CType(MScale, clsRatingScale).RatingSet
                            If tR.ID.ToString = RatingID Then
                                RatingData.IsUndefined = False 'Cv2 + D0108
                                RatingData.ObjectValue = tR 'Cv2 + D0108
                                JudgLogs = tR.Name  ' D7262
                                Exit For
                            End If
                        Next
                    End If
                End If

                ' D1748 ===
                Dim RatingDirect As Double = -1 ' D1858
                If RatingID < "0" AndAlso RatingID <> "-1" AndAlso String2Double(CheckVar("DirectRating", ""), RatingDirect) Then ' D1784 + D1858
                    If RatingDirect >= 0 AndAlso RatingDirect <= 1 Then
                        RatingData.IsUndefined = False
                        RatingData.ObjectValue = New clsRating(-1, "Direct input from EC Core", CSng(RatingDirect), Nothing)
                        JudgLogs = Double2String(RatingDirect, 2)   ' D7262
                    End If
                End If
                ' D1748 ==

                fChanged = True     ' D0108
            End If
            ' D0108 ===
            If ShowComments And Not sComment Is Nothing Then ' D0195 + D1938
                'Dim sComment As String = CheckVar("RatingComment", RatingData.Comment) ' - D0195
                If sComment <> RatingData.Comment Then
                    RatingData.Comment = sComment
                    fChanged = True
                    JudgLogs += JudgComment   ' D7262
                End If
            End If
            'If fChanged Then App.ActiveProject.ProjectManager.StorageManager.SaveJudgment(Data.Node, Data.Judgment) ' D0039 'Cv2 'C0028
            'If fChanged Then App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(Data.Node, Data.Judgment) ' D0039 'Cv2 'C0028 'C0403
            If fChanged Then
                ' D2510 ===
                If IsRiskWithControls Then
                    App.ActiveProject.ProjectManager.PipeBuilder.SaveControlJudgment(Action)
                Else
                    ' D4350 ===
                    If Data.Node IsNot Nothing Then
                        If Data.Node.IsAlternative Then
                            App.ActiveProject.HierarchyAlternatives.GetNodeByID(Data.Node.NodeID).Judgments.AddMeasureData(RatingData)
                        Else
                            App.ActiveProject.HierarchyObjectives.GetNodeByID(Data.Node.NodeID).Judgments.AddMeasureData(RatingData)
                        End If
                    End If
                    ' D4350 ==
                    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(Data.Node, RatingData) 'C0403
                End If
                ' D2510 ==
            End If
            ' D0108 ==

            ' D2281 ===
            fChanged = False
            If CanUserEditActiveProject AndAlso MScale IsNot Nothing Then   ' D2570
                For Each tR As clsRating In CType(MScale, clsRatingScale).RatingSet
                    Dim sDesc As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("Intensity" + tR.ID.ToString, tR.Comment))   'Anti-XSS
                    If tR.Comment <> sDesc Then
                        tR.Comment = sDesc
                        fChanged = True
                    End If
                Next
            End If
            If fChanged Then App.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
            ' D2281 ==

            Return CType(RatingData, clsRatingMeasureData) 'C0403
        Else 'C0403
            Return Nothing
        End If
    End Function
    ' D0038 ==

    Private _HierarchyTerminalNodes As List(Of clsNode) = Nothing
    Public ReadOnly Property HierarchyTerminalNodes As List(Of clsNode)
        Get
            If _HierarchyTerminalNodes Is Nothing Then
                _HierarchyTerminalNodes = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
            End If
            Return _HierarchyTerminalNodes
        End Get
    End Property

    Private Function RR_GetScenario(tAlt As clsNode) As String
        Dim sName As String = ""
        If tAlt IsNot Nothing AndAlso tAlt.IsAlternative AndAlso (App.ActiveProject.isMyRiskRewardModel OrElse App.ActiveProject.isMixedModel) Then
            Dim covObjs As New List(Of clsNode)
            For Each node In HierarchyTerminalNodes
                Dim contributedAlternatives = node.GetContributedAlternatives
                If contributedAlternatives IsNot Nothing AndAlso contributedAlternatives.Contains(tAlt) AndAlso Not covObjs.Contains(node) Then covObjs.Add(node)
            Next
            For Each node In covObjs
                sName += String.Format("{0}[{1}] {2}", If(sName = "", "", ", " + vbNewLine), HierarchyTerminalNodes.IndexOf(node) + 1, ShortString(node.NodeName, If(covObjs.Count > 1, 50, 100), True))
            Next
        End If
        Return sName
    End Function

    ' D0077 ===
    Private Sub CreateMultiRatings(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim tParams As New Dictionary(Of String, String)    ' D0120
        Dim AllAlts As ctrlMultiRatings = CType(LoadControl("~/ctrlMultiRatings.ascx"), ctrlMultiRatings) ' D0995
        AllAlts.sRootPath = _URL_ROOT ' D1593
        AllAlts.ID = "AllAlts"  ' D1239

        Dim isCategory As Boolean = False   ' D3604
        Dim isGoal As Boolean = False       ' D3742

        AllAlts.ChangeFlagID = isChanged.ClientID
        If Not isAllowMissingJudgments() And Not btnNext.Enabled And btnNext.CommandArgument <> "" Then   ' D0598
            AllAlts.onChangeAction = String.Format("theForm.{0}.disabled=HasUndefined(); ", btnNext.ClientID)
            If btnNextUnas.Visible And btnNextUnas.CommandArgument <> "" Then AllAlts.onChangeAction += String.Format("theForm.{1}.disabled = theForm.{0}.disabled; ", btnNext.ClientID, btnNextUnas.ClientID)
        End If

        'If isAllowMissingJudgments() Then AllAlts.msgMissingRatings = ResString("msgMissingRatings") ' D1034 -D2158
        'If App.ActiveProject.PipeParameters.AllowAutoadvance Then Rating.onChangeAction += String.Format("if (!theForm.{0}.disabled) theForm.{0}.click();", btnNext.ClientID)

        ' D1008 ===
        AllAlts.ImagePath = ImagePath
        Dim Ratings As List(Of clsRating) = Nothing
        Dim MeasureScales As clsMeasureScales = App.ActiveProject.ProjectManager.MeasureScales
        Dim sNotRatedCaption As String = ResString("lblEvaluationNotRated")
        ' D1008 ==

        AllAlts.ShowFramedInfodocs = CheckVar("framed", App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame)   ' D1372

        Dim oID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("oid", "")) ' D1425 + Anti-XSS
        Dim aID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("aid", "")) ' D1425 + Anti-XSS
        Dim UndefIdx As Integer = -1    ' D1438

        If TypeOf (Action.ActionData) Is clsAllChildrenEvaluationActionData Then 'Cv2 'C0464
            Dim data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData) 'Cv2 'C0464 + Â0677
            'AllAlts.MeasureScales = App.ActiveProject.ProjectManager.MeasureScales

            AllAlts.ReverseInfodocFrames = True
            isCategory = data.ParentNode.RiskNodeType = RiskNodeType.ntCategory ' D3604
            isGoal = data.ParentNode IsNot Nothing AndAlso data.ParentNode.ParentNode Is Nothing    ' D3742

            ' D1008 ===
            If Not MeasureScales Is Nothing Then
                If Not MeasureScales.RatingsScales Is Nothing Then
                    Dim RS As clsRatingScale = MeasureScales.GetRatingScaleByID(data.ParentNode.RatingScaleID)
                    If Not RS Is Nothing Then
                        AllAlts.ShowDirectValue = App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(RS.GuidID)   ' D4170
                        AllAlts.ShowPrtyValues = AllAlts.ShowDirectValue ' D4170
                        Ratings = New List(Of clsRating)
                        For Each tRating As clsRating In RS.RatingSet
                            ' D2282 ===
                            Dim tNewRating As New clsRating(tRating.ID, tRating.Name, tRating.Value, tRating.RatingScale, If(ShowInfoDocs, tRating.Comment, ""))  ' D4014
                            tNewRating.GuidID = tRating.GuidID
                            Ratings.Add(tNewRating)   ' D1029
                            ' D2282 ==
                        Next
                        AllAlts.ScanRatingsForMaxLen(Ratings)
                        'AllAlts.Precision = GetPrecisionForRatings(RS, AllAlts.Precision) ' D2120 + D3782
                    End If
                End If
            End If
            If Not Ratings Is Nothing Then Ratings.Insert(0, New clsRating(-1, sNotRatedCaption, 0, Nothing)) ' D2282 + D4712

            Dim isAlts As Boolean = False   ' D4105
            Dim Lst As New List(Of clsRatingLine)
            Dim ID As Integer = 0
            For Each tAlt As clsNode In data.Children
                If tAlt.IsAlternative Then isAlts = True ' D4105
                If (tAlt.NodeID.ToString = oID AndAlso data.ParentNode.NodeID.ToString = aID) OrElse (tAlt.NodeID.ToString = aID AndAlso data.ParentNode.ToString = oID) Then AllAlts._FocusID = ID ' D1425
                Dim R As clsRatingMeasureData = CType(data.GetJudgment(tAlt), clsRatingMeasureData)
                Dim RID As Integer = -1
                If R IsNot Nothing AndAlso R.Rating IsNot Nothing Then RID = R.Rating.ID    ' D6681
                If UndefIdx = -1 AndAlso R IsNot Nothing AndAlso R.IsUndefined Then UndefIdx = ID ' D1438 + D6681

                ' D1508 ===
                Dim sInfodoc As String = Nothing
                Dim sInfodocURL As String = Nothing
                Dim sInfodocEditURL As String = Nothing
                Dim sInfodocWRT As String = Nothing
                Dim sInfodocWRTURL As String = Nothing
                Dim sInfodocWRTEditURL As String = Nothing

                If ShowInfoDocs Then    ' D1938
                    ' D2412 ===
                    sInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), tAlt.NodeID.ToString, tAlt.InfoDoc, True, True, -1)    ' D1669
                    sInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tAlt.NodeID, GetRandomString(6, False, False))) ' D1064
                    sInfodocEditURL = PopupRichEditor(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt{3}", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, ID))
                    ' D2412 ==
                    sInfodocWRT = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tAlt.NodeID.ToString, App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, data.ParentNode.NodeGuidID), False, True, data.ParentNode.NodeID) ' D1669
                    sInfodocWRTURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&guid={4}&pguid={5}&r={6}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tAlt.NodeID, data.ParentNode.NodeID, tAlt.NodeGuidID, data.ParentNode.NodeGuidID, GetRandomString(6, False, False))) ' D1064
                    sInfodocWRTEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=wrt{5}", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, tAlt.NodeGuidID, data.ParentNode.NodeGuidID, ID))
                End If
                ' D1508 ==

                Dim DV As Single = -1
                If R IsNot Nothing AndAlso R.Rating IsNot Nothing AndAlso R.Rating.RatingScaleID < 0 AndAlso R.Rating.ID < 0 Then DV = R.Rating.Value   ' D6681
                If R IsNot Nothing AndAlso R.RatingScale IsNot Nothing AndAlso tAlt IsNot Nothing Then  ' D6681
                    Lst.Add(New clsRatingLine(ID, R.RatingScale.GuidID.ToString(), JS_SafeHTML(tAlt.NodeName), Ratings, RID, sInfodoc, If(ShowInfoDocs, R.Comment, ""), DV, sInfodocURL, sInfodocEditURL, sInfodocWRT, sInfodocWRTURL, sInfodocWRTEditURL, R.RatingScale.Comment, tAlt.Index, tAlt, SafeFormString(RR_GetScenario(tAlt)))) ' D0108 + D0131 + D0260 + D0647 + D1064 + D2251 + D2252 + D2274 + D2420 + D4013 + D4014 + D4106 + D4180 + D4431 + D6822
                    ID += 1
                End If
            Next
            AllAlts.Ratings = Lst
            AllAlts.ShowIndex = App.isRiskEnabled AndAlso isAlts AndAlso App.ActiveProject.ProjectManager.Parameters.NodeVisibleIndexMode <> IDColumnModes.Rank ' D4013 + D4105 + D4323 + A1342 + D6685
            ' D1008 ==

            'AllAlts.Data = data
            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.ParentNode, True))  ' D0120 + D0558 + D0677
            tParams.Add(_TEMPL_EVALCOUNT, data.Children.Count.ToString)  ' D0120 + D0677
            AllAlts.CaptionName = JS_SafeHTML(data.ParentNode.NodeName) ' D1372
            'AllAlts.Caption = PrepareTask(ParseStringTemplates(ResString(IIf(App.isRiskEnabled, "lblEvaluationAllAlternativesRisk", "lblEvaluationAllAlternatives")), tParams))  ' D0120 + D1990 + D2066 + D2258 + D2320
            AllAlts.ParentNodeID = data.ParentNode.NodeID.ToString    ' D1139
            If ShowInfoDocs Then    ' D1938
                AllAlts.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(data.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), data.ParentNode.NodeID.ToString, data.ParentNode.InfoDoc, True, True, -1) ' D0108 + D0131 + D0677 + D1669 + D2419
                AllAlts.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(data.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, data.ParentNode.NodeID, GetRandomString(6, False, False))) ' D1064 + D2419
                AllAlts.CaptionInfodocEditURL = PopupRichEditor(If(data.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, data.ParentNode.NodeID, data.ParentNode.NodeGuidID)) ' D1007 + D2419
            End If

            sTask = GetPipeStepTask(Action, IntensityScale) ' D2316
            AllAlts.lblInfodocTitleWRT = GetWRTCaption(isCategory) ' D1528  + D3031 + D3604 + D3610
        End If

        If TypeOf (Action.ActionData) Is clsAllCoveringObjectivesEvaluationActionData Then 'Cv2 'C0464
            Dim data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData) 'Cv2 'C0464
            'AllAlts.MeasureScales = App.ActiveProject.ProjectManager.MeasureScales
            'AllAlts.Data = data
            isGoal = False  ' D3742

            ' D1008 ===
            Dim Lst As New List(Of clsRatingLine)
            Dim ID As Integer = 0
            For Each tCovObj As clsNode In data.CoveringObjectives
                If TypeOf (data.GetJudgment(tCovObj)) Is clsRatingMeasureData Then ' D0152
                    Dim R As clsRatingMeasureData = CType(data.GetJudgment(tCovObj), clsRatingMeasureData) 'Cv2
                    Dim RID As Integer = -1
                    If Not R.Rating Is Nothing Then RID = R.Rating.ID

                    If (R.NodeID.ToString = oID AndAlso R.ParentNodeID.ToString = aID) Or (R.NodeID.ToString = aID AndAlso R.ParentNodeID.ToString = oID) Then AllAlts._FocusID = ID ' D1425

                    If Not MeasureScales Is Nothing Then
                        If Not MeasureScales.RatingsScales Is Nothing Then
                            Dim RS As clsRatingScale = MeasureScales.GetRatingScaleByID(tCovObj.RatingScaleID)
                            If Not RS Is Nothing Then
                                AllAlts.ShowDirectValue = AllAlts.ShowDirectValue AndAlso App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(RS.GuidID)   ' D4170
                                AllAlts.ShowPrtyValues = AllAlts.ShowDirectValue ' D4170
                                Ratings = New List(Of clsRating)
                                For Each tRating As clsRating In RS.RatingSet
                                    ' D2282 ===
                                    Dim tNewRating As New clsRating(tRating.ID, tRating.Name, tRating.Value, tRating.RatingScale, If(ShowInfoDocs, tRating.Comment, ""))   ' D4015
                                    tNewRating.GuidID = tRating.GuidID
                                    Ratings.Add(tNewRating)   ' D1029
                                    ' D2282 ==
                                Next
                                AllAlts.ScanRatingsForMaxLen(Ratings)
                                'AllAlts.Precision = GetPrecisionForRatings(RS, AllAlts.Precision) ' D2120 + D3782
                            End If
                        End If
                    End If

                    If UndefIdx = -1 AndAlso R.IsUndefined Then UndefIdx = ID ' D1438

                    If Not Ratings Is Nothing Then Ratings.Insert(0, New clsRating(-1, sNotRatedCaption, 0, Nothing)) ' D2282 + D4712

                    ' D1508 ===
                    Dim sInfodoc As String = Nothing
                    Dim sInfodocURL As String = Nothing
                    Dim sInfodocEditURL As String = Nothing
                    Dim sInfodocWRT As String = Nothing
                    Dim sInfodocWRTURL As String = Nothing
                    Dim sInfodocWRTEditURL As String = Nothing

                    If ShowInfoDocs Then    ' D1938
                        sInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tCovObj.IsAlternative, reObjectType.Alternative, reObjectType.Node), tCovObj.NodeID.ToString, tCovObj.InfoDoc, True, True, -1) ' D1669 + D2419
                        sInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(tCovObj.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tCovObj.NodeID, GetRandomString(6, False, False))) ' D1064 +  D2419
                        sInfodocEditURL = PopupRichEditor(reObjectType.Node, String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt{3}", _PARAM_ID, tCovObj.NodeID, tCovObj.NodeGuidID, ID))
                        sInfodocWRT = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, data.Alternative.NodeID.ToString, App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(data.Alternative.NodeGuidID, tCovObj.NodeGuidID), False, True, tCovObj.NodeID) ' D1669
                        sInfodocWRTURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&r={4}", CInt(reObjectType.AltWRTNode), _PARAM_ID, data.Alternative.NodeID, tCovObj.NodeID, GetRandomString(6, False, False))) ' D1064
                        sInfodocWRTEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=wrt{5}", _PARAM_ID, data.Alternative.NodeID, data.Alternative.NodeGuidID, tCovObj.NodeGuidID, tCovObj.NodeGuidID, ID))
                    End If
                    ' D1508 ==

                    ' D1745 ===
                    Dim DV As Single = -1
                    If R.Rating IsNot Nothing AndAlso R.Rating.RatingScaleID < 0 AndAlso R.Rating.ID < 0 Then DV = R.Rating.Value
                    Dim sScaleInfodoc As String = ""    ' D4105
                    If ShowInfoDocs Then sScaleInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.Node, (tCovObj.NodeID + 1).ToString, tCovObj.InfoDoc, True, True, -1) ' D4015
                    Lst.Add(New clsRatingLine(ID, R.RatingScale.GuidID.ToString, JS_SafeHTML(tCovObj.NodeName), Ratings, RID, sScaleInfodoc, If(ShowInfoDocs, R.Comment, ""), DV, sInfodocURL, sInfodocEditURL, sInfodocWRT, sInfodocWRTURL, sInfodocWRTEditURL, R.RatingScale.Comment, tCovObj.Index, tCovObj, SafeFormString(RR_GetScenario(tCovObj)))) ' D0108 + D0131 + D0260 + D0647 + D1064 + D2251 + D2251 + D2274 + D2420 + D4013 + D4106 + D4180 + D6822
                    ' D1745 == 
                    ID += 1
                End If
            Next
            AllAlts.Ratings = Lst
            ' D1008 ==

            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.Alternative, True))  ' D0120 + D0558
            tParams.Add(_TEMPL_EVALCOUNT, data.CoveringObjectives.Count.ToString)  ' D0120
            AllAlts.ParentNodeID = data.Alternative.NodeID.ToString   ' D1139
            If ShowInfoDocs Then    ' D1938
                AllAlts.CaptionName = data.Alternative.NodeName ' D1372
                AllAlts.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(data.Alternative.IsAlternative, reObjectType.Alternative, reObjectType.Node), data.Alternative.NodeID.ToString, data.Alternative.InfoDoc, True, True, -1) ' D0108 + D0131 + D1669 + D2419
                AllAlts.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(data.Alternative.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, data.Alternative.NodeID, GetRandomString(6, False, False))) ' D1064 + D2419
                AllAlts.CaptionInfodocEditURL = PopupRichEditor(If(data.Alternative.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, data.Alternative.NodeID, data.Alternative.NodeGuidID)) ' D1007 + D2419
            End If

            sTask = GetPipeStepTask(Action, IntensityScale) ' D2316
            AllAlts.lblInfodocTitleWRT = ResString("lblShowDescriptionWRTInvert")   ' D1528
        End If

        ' D3250 ===
        If TypeOf (Action.ActionData) Is clsAllEventsWithNoSourceEvaluationActionData Then
            Dim data As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)

            isGoal = False  ' D3742

            Dim Lst As New List(Of clsRatingLine)
            Dim ID As Integer = 0
            Dim isAlts As Boolean = False   ' D4105
            For Each tAlt As clsNode In data.Alternatives
                If tAlt.IsAlternative Then isAlts = True ' D4105
                If TypeOf (data.GetJudgment(tAlt)) Is clsRatingMeasureData Then
                    Dim R As clsRatingMeasureData = CType(data.GetJudgment(tAlt), clsRatingMeasureData)
                    If R IsNot Nothing Then ' D6688
                        Dim RID As Integer = -1
                        If Not R.Rating Is Nothing Then RID = R.Rating.ID

                        If Not MeasureScales Is Nothing Then
                            If Not MeasureScales.RatingsScales Is Nothing Then
                                Dim RS As clsRatingScale = MeasureScales.GetRatingScaleByID(tAlt.RatingScaleID)
                                If Not RS Is Nothing Then
                                    Ratings = New List(Of clsRating)
                                    For Each tRating As clsRating In RS.RatingSet
                                        Dim tNewRating As New clsRating(tRating.ID, tRating.Name, tRating.Value, tRating.RatingScale, If(ShowInfoDocs, tRating.Comment, ""))  ' D4015
                                        tNewRating.GuidID = tRating.GuidID
                                        Ratings.Add(tNewRating)
                                    Next
                                    AllAlts.ScanRatingsForMaxLen(Ratings)
                                    'AllAlts.Precision = GetPrecisionForRatings(RS, AllAlts.Precision)  ' D3782
                                    AllAlts.ShowDirectValue = App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(RS.GuidID)   ' D4529
                                    AllAlts.ShowPrtyValues = AllAlts.ShowDirectValue    ' D4529
                                End If
                            End If
                        End If

                        If UndefIdx = -1 AndAlso R.IsUndefined Then UndefIdx = ID
                        If R.NodeID.ToString = oID OrElse R.NodeID.ToString = aID Then AllAlts._FocusID = ID ' D3378

                        If Not Ratings Is Nothing Then Ratings.Insert(0, New clsRating(-1, sNotRatedCaption, 0, Nothing))   ' D4712

                        Dim sInfodoc As String = Nothing
                        Dim sInfodocURL As String = Nothing
                        Dim sInfodocEditURL As String = Nothing

                        If ShowInfoDocs Then
                            sInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), tAlt.NodeID.ToString, tAlt.InfoDoc, True, True, -1)
                            sInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tAlt.NodeID, GetRandomString(6, False, False)))
                            sInfodocEditURL = PopupRichEditor(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt{3}", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, ID))
                        End If

                        Dim DV As Single = -1
                        If R.Rating IsNot Nothing AndAlso R.Rating.RatingScaleID < 0 AndAlso R.Rating.ID < 0 Then DV = R.Rating.Value
                        Dim sScaleInfodoc As String = ""    ' D4015
                        If ShowInfoDocs Then sScaleInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.Node, (tAlt.NodeID + 1).ToString, tAlt.InfoDoc, True, True, -1) ' D4015
                        Lst.Add(New clsRatingLine(ID, If(R.RatingScale Is Nothing, Guid.Empty, R.RatingScale.GuidID).ToString, JS_SafeHTML(tAlt.NodeName), Ratings, RID, sScaleInfodoc, If(ShowInfoDocs, R.Comment, ""), DV, sInfodocURL, sInfodocEditURL, Nothing, Nothing, Nothing, If(R.RatingScale Is Nothing, "", R.RatingScale.Comment), tAlt.Index, tAlt, SafeFormString(RR_GetScenario(tAlt))))    ' D4013 + D4106 + D4180 + D6688 + D6822
                        ID += 1
                        Ratings = Nothing   ' D6718
                    End If
                End If
            Next
            AllAlts.Ratings = Lst
            AllAlts.ShowIndex = App.isRiskEnabled AndAlso isAlts AndAlso App.ActiveProject.ProjectManager.Parameters.NodeIndexIsVisible AndAlso App.ActiveProject.ProjectManager.Parameters.NodeVisibleIndexMode <> IDColumnModes.Rank ' D4013 +D4105 + D4323 + A1342 + D6685

            ''tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.Alternative, True))  ' D0120 + D0558
            tParams.Add(_TEMPL_EVALCOUNT, data.Alternatives.Count.ToString)  ' D0120
            ''AllAlts.ParentNodeID = data.Alternative.NodeID  ' D1139
            If ShowInfoDocs Then
                AllAlts.CaptionName = Nothing
                AllAlts.CaptionInfodoc = Nothing
                AllAlts.CaptionInfodocURL = Nothing
                AllAlts.CaptionInfodocEditURL = Nothing
            End If

            sTask = GetPipeStepTask(Action, IntensityScale)
            AllAlts.lblInfodocTitleWRT = ResString("lblShowDescriptionWRTInvert")
        End If
        ' D3250 ==

        If ShowInfoDocs Then
            AllAlts.ScaleInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&scale={3}", CInt(reObjectType.MeasureScale), _PARAM_ID, "%%guid%%", CInt(ECMeasureType.mtRatings))) ' D2251
            AllAlts.ScaleInfodocEditURL = PopupRichEditor(reObjectType.MeasureScale, String.Format("{0}={1}&scale={2}&callback=scale", _PARAM_ID, "%%guid%%", CInt(ECMeasureType.mtRatings))) ' D2251
        End If
        AllAlts.lblInfodocTitleScale = ResString("lblShowDescriptionScale") ' D2252

        If AllAlts._FocusID = -1 AndAlso UndefIdx <> -1 Then AllAlts._FocusID = UndefIdx ' D1438 + D1751
        If AllAlts._FocusID < 0 AndAlso AllAlts.Ratings IsNot Nothing AndAlso AllAlts.Ratings.Count > 0 Then AllAlts._FocusID = 0 ' D1441 + D3250
        ' D1372 ===
        If AllAlts.ShowFramedInfodocs AndAlso ShowInfoDocs Then    ' D1508 + D1938
            ' D1438 ===
            Dim fHasInfodoc As Boolean = Not isHTMLEmpty(AllAlts.CaptionInfodoc)
            If Not fHasInfodoc AndAlso AllAlts._FocusID >= 0 Then fHasInfodoc = AllAlts.Ratings.Count > 0 AndAlso AllAlts.Ratings.Count > AllAlts._FocusID AndAlso (Not isHTMLEmpty(AllAlts.Ratings(AllAlts._FocusID).Infodoc) OrElse Not isHTMLEmpty(AllAlts.Ratings(AllAlts._FocusID).InfodocWRT)) ' D4431
            AllAlts.CaptionInfodocCollapsed = GetCookie("mr_status", If(fHasInfodoc AndAlso Not App.isMobileBrowser, "1", "0")) = "0"  ' D2949
            ' D1438 ==
            AllAlts.AlternativeInfodocCollapsed = AllAlts.CaptionInfodocCollapsed
            AllAlts.WRTInfodocCollapsed = AllAlts.CaptionInfodocCollapsed
            ' D1864 ===
            AllAlts.ScaleInfodocCollapsed = GetCookie("scale_status", If(fHasInfodoc AndAlso Not App.isMobileBrowser, "1", "0")) = "0" ' D2251 + D2949
            AllAlts.SaveSizeMessage = ResString("msgOnSaveInfodocSize")
            AllAlts.AllowSaveSize = CanUserEditActiveProject
            AllAlts.SizesList = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
            ' D1864 ==
        End If
        ' D1372 ==

        ' D0647 ===
        AllAlts.ShowComment = ShowComments ' D1007 + D1938
        AllAlts.lblCommentTitle = ResString("lblEvaluationPWComment")   ' D1007
        AllAlts.isCommentCollapsed = GetCookie("comment_status", If(AllAlts.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D1069 + D2949
        AllAlts.CaptionSaveComment = ResString("btnSave")     ' D2344
        AllAlts.ImageCommentExists = ImagePath + "comment.png"   ' D2769
        AllAlts.ImageCommentEmpty = ImagePath + "comment_e.png"  ' D2769
        AllAlts.ImageBlank = BlankImage ' D1372
        ' D0647 ==

        ' D1007 ===
        AllAlts.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject     ' D1063 + D2505 + D4116
        AllAlts.ImageEditInfodoc = "edit_tiny.gif"
        AllAlts.ImageViewInfodoc = "view_sm.png"    ' D1426
        AllAlts.lblInfodocTitleGoal = ResString("lblShowDescriptionFor")
        AllAlts.lblInfodocTitleNode = ResString("lblShowDescriptionFor")
        ' D1007 ==

        ' D1035 ===
        AllAlts.tblIntensity = ResString("tblMultiRatingsIntensity")
        AllAlts.tblPriority = ResString(CStr(IIf(App.isRiskEnabled AndAlso Not App.ActiveProject.isImpact, "tblMultiRatingsLikelihood", "tblMultiRatingsPriority")))    ' D4014
        AllAlts.msgLoading = ResString("msgMultiRatingsLoading")
        ' D1035 ==

        AllAlts.msgDirectRatingValue = ResString("msgDirectRating") ' D1747
        AllAlts.lblDirectValue = ResString("lblDirectValueRating") ' D1747

        ' D6661 ===
        If App.isRiskEnabled AndAlso Not String.IsNullOrEmpty(ClusterPhrase) Then
            Dim sTitleOrig As String = ClusterPhrase
            Dim idx = sTitleOrig.ToLower.IndexOf("wording%%")
            If idx > 0 Then
                sTitleOrig = sTitleOrig.Substring(0, idx + 7)
                idx = sTitleOrig.LastIndexOf("%%")
                If idx >= 0 Then
                    sTitleOrig = sTitleOrig.Substring(idx) + "%%"
                    sTitleOrig = PrepareTask(sTitleOrig)
                    If Not sTitleOrig.Contains("%%") Then AllAlts.RatingsTitle = String.Format("{0} ", sTitleOrig, ResString("lblOf"))  ' add " of " in the control
                End If
            End If
        End If
        ' D6661 ==

        If ShowDraftPages() AndAlso App.ActiveProject.IsRisk AndAlso Not isCategory AndAlso isImpact Then   ' D4259
            'If ShowDraftPages() AndAlso App.ActiveProject.IsRisk Then   ' D4016
            USD_OptionVisible = True
            USD_onClientClick = "switchUSD(this.checked); Changed();"
        End If
        ' D3742 + D3749 + D3795

        btnSave.Visible = False  ' D2154
        isMultiRatingsOrDirect = True   ' D2926

        'If Not IsReadOnly AndAlso Not App.ActiveProject.PipeParameters.DisableWarningsOnStepChange Then ClientScript.RegisterStartupScript(GetType(String), "HookSubmit", "_postback_custom_code = 'confirmMissingJudgments();'; HookSubmit();", True) ' D2950

        Controls.Add(AllAlts)
    End Sub

    Protected Sub SaveMultiRatings(ByRef Action As clsAction)
        Dim RCount As Integer = 0
        If Integer.TryParse(CheckVar("RatingsCount", "0"), RCount) Then
            JudgLogs = "" ' D7262
            If TypeOf (Action.ActionData) Is clsAllChildrenEvaluationActionData Then 'Cv2 'C0464 + D0677
                Dim data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData) 'Cv2 'C0464 + D0677
                Dim RatingID As Integer
                Dim RatingDirect As Double  ' D1745 + D1858
                Dim Alt As clsNode
                For i As Integer = 0 To data.Children.Count - 1 'Cv2 + D0677
                    If Integer.TryParse(CheckVar(String.Format("Rating{0}", i), ""), RatingID) AndAlso String2Double(CheckVar(String.Format("RatingDirect{0}", i), ""), RatingDirect) Then    ' D1745 + D1858
                        Alt = data.Children(i) 'Cv2 + D0677
                        Dim Ratings As clsRatingScale = App.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(data.ParentNode.RatingScaleID)    ' D0677
                        If Not Ratings Is Nothing Then
                            Dim AltRating As clsRating = Ratings.GetRatingByID(RatingID)

                            ' D1745 ===
                            If AltRating Is Nothing AndAlso RatingDirect >= 0 AndAlso RatingDirect <= 1 Then
                                AltRating = New clsRating(-1, "Direct input from EC Core", CSng(RatingDirect), Nothing)
                                JudgLogs += If(JudgLogs = "", "", "; ") + Double2String(RatingDirect, 4)    ' D7262
                                ' D1745 ==
                            Else
                                JudgLogs += If(JudgLogs = "", "", "; ") + If(AltRating Is Nothing, JudgUndefined, AltRating.Name)   ' D7262
                            End If
                            ' D0647 ===
                            Dim R As clsRatingMeasureData = CType(data.GetJudgment(Alt), clsRatingMeasureData)
                            If ShowComments AndAlso R IsNot Nothing Then    ' D1938
                                data.SetData(Alt, AltRating, EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("Comment{0}", i), R.Comment)))  'Anti-XSS
                            Else
                                data.SetData(Alt, AltRating) 'Cv2
                            End If
                            ' D0647 ==

                            'Project.ProjectManager.StorageManager.SaveJudgment(data.CoveringObjective, data.GetJudgment(Alt)) 'Cv2 'C0028
                            ' D4350 ===
                            If data.ParentNode.IsAlternative Then
                                App.ActiveProject.HierarchyAlternatives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(data.GetJudgment(Alt))
                            Else
                                App.ActiveProject.HierarchyObjectives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(data.GetJudgment(Alt))
                            End If
                            ' D4350 ==
                            App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, data.GetJudgment(Alt)) 'Cv2 'C0028 + D0677
                        End If
                    End If
                Next
            End If ' AllAlts

            If TypeOf (Action.ActionData) Is clsAllCoveringObjectivesEvaluationActionData Then 'Cv2 'C0464
                Dim data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData) 'Cv2 'C0464
                Dim RatingID As Integer
                Dim RatingDirect As Double  ' D1745 + D1858
                Dim CovObj As clsNode
                For i As Integer = 0 To data.CoveringObjectives.Count - 1
                    If Integer.TryParse(CheckVar(String.Format("Rating{0}", i), ""), RatingID) AndAlso String2Double(CheckVar(String.Format("RatingDirect{0}", i), ""), RatingDirect) Then    ' D1745 + D1858
                        CovObj = data.CoveringObjectives(i)
                        Dim Ratings As clsRatingScale = App.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(CovObj.RatingScaleID)
                        If Not Ratings Is Nothing Then
                            Dim AltRating As clsRating = Ratings.GetRatingByID(RatingID)

                            ' D1745 ===
                            If AltRating Is Nothing AndAlso RatingDirect >= 0 AndAlso RatingDirect <= 1 Then
                                AltRating = New clsRating(-1, "Direct input from EC Core", CSng(RatingDirect), Nothing)
                                JudgLogs += If(JudgLogs = "", "", "; ") + Double2String(RatingDirect, 4) ' D7262
                                ' D1745 ==
                                ' D0647 ===
                            Else
                                JudgLogs += If(JudgLogs = "", "", "; ") + If(AltRating Is Nothing, JudgUndefined, AltRating.Name)   ' D7262
                            End If
                            ' D0647 ==

                            Dim R As clsRatingMeasureData = CType(data.GetJudgment(CovObj), clsRatingMeasureData)
                            If ShowComments AndAlso R IsNot Nothing Then    ' D1938
                                data.SetData(CovObj, AltRating, EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("Comment{0}", i), R.Comment)))   'Anti-XSS
                            Else
                                data.SetData(CovObj, AltRating) 'Cv2
                            End If

                            'Project.ProjectManager.StorageManager.SaveJudgment(CovObj, data.GetJudgment(CovObj)) 'Cv2 'C0028
                            App.ActiveProject.HierarchyObjectives.GetNodeByID(CovObj.NodeID).Judgments.AddMeasureData(data.GetJudgment(CovObj))
                            App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(CovObj, data.GetJudgment(CovObj)) 'Cv2 'C0028
                        End If
                    End If
                Next
            End If ' AllCovObj

            ' D3250 ===
            If TypeOf (Action.ActionData) Is clsAllEventsWithNoSourceEvaluationActionData Then
                Dim data As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)
                Dim RatingID As Integer
                Dim RatingDirect As Double
                Dim Alt As clsNode
                For i As Integer = 0 To data.Alternatives.Count - 1
                    If Integer.TryParse(CheckVar(String.Format("Rating{0}", i), ""), RatingID) AndAlso String2Double(CheckVar(String.Format("RatingDirect{0}", i), ""), RatingDirect) Then
                        Alt = data.Alternatives(i)
                        Dim Ratings As clsRatingScale = App.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(Alt.RatingScaleID)
                        If Not Ratings Is Nothing Then
                            Dim AltRating As clsRating = Ratings.GetRatingByID(RatingID)

                            If AltRating Is Nothing AndAlso RatingDirect >= 0 AndAlso RatingDirect <= 1 Then
                                AltRating = New clsRating(-1, "Direct input from EC Core", CSng(RatingDirect), Nothing)
                                If AltRating IsNot Nothing Then JudgLogs += If(JudgLogs = "", "", "; ") + Double2String(RatingDirect, 4) ' D7262
                            Else
                                JudgLogs += If(JudgLogs = "", "", "; ") + If(AltRating Is Nothing, JudgUndefined, AltRating.Name)   ' D7262
                            End If

                            Dim R As clsRatingMeasureData = CType(data.GetJudgment(Alt), clsRatingMeasureData)
                            If ShowComments AndAlso R IsNot Nothing Then
                                data.SetData(Alt, AltRating, EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("Comment{0}", i), R.Comment)))  'Anti-XSS
                            Else
                                data.SetData(Alt, AltRating)
                            End If

                            App.ActiveProject.HierarchyAlternatives.GetNodeByID(Alt.NodeID).DirectJudgmentsForNoCause.AddMeasureData(data.GetJudgment(Alt))
                            App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(Alt, data.GetJudgment(Alt))
                        End If
                    End If
                Next
            End If ' No Sources
            ' D3250  ==

            ' D2282 ===
            If CanUserEditActiveProject Then
                Dim fChanged As Boolean = False
                With App.ActiveProject.ProjectManager.MeasureScales.RatingsScales
                    For i = 0 To .Count - 1
                        For Each tRating As clsRating In CType(.Item(i), clsRatingScale).RatingSet
                            Dim sName As String = String.Format("Intensity_{0}", GetMD5(tRating.GuidID.ToString))   ' D3068 + D3070
                            If Request(sName) IsNot Nothing Then
                                Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(sName, tRating.Comment))  'Anti-XSS
                                If sComment <> tRating.Comment Then
                                    tRating.Comment = sComment
                                    fChanged = True
                                    JudgLogs += JudgComment   ' D7262
                                End If
                            End If
                        Next
                    Next
                End With
                If fChanged Then App.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
            End If
            ' D2282 ==

            ' D3472 ===
            If CheckVar("optUSD", False) Then USD_ShowCostOfGoal = CheckVar("cbUSD", False)
            Dim sTotalUSD As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("totalUSD", "")) 'Anti-XSS
            If sTotalUSD <> "" AndAlso IsPM Then
                Dim tCost As Double
                If String2Double(sTotalUSD, tCost) AndAlso tCost > 0 Then USD_CostOfGoal = tCost
            End If
            ' D3472 ==

        End If ' TryParse
    End Sub
    ' D0077 ==

    ' D0678 ===
    Private Sub CreateMultiDirectInputs(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim tParams As New Dictionary(Of String, String)
        Dim AllDI As ctrlMultiDirectInputs = CType(LoadControl("~/ctrlMultiDirectInputs.ascx"), ctrlMultiDirectInputs)   ' D0995
        AllDI.sRootPath = _URL_ROOT ' D1593
        AllDI.ID = "AllDI"  ' D1239
        'AllDI.ProjectID = App.ProjectID
        AllDI.ChangeFlagID = isChanged.ClientID
        AllDI.ShowFramedInfodocs = CheckVar("framed", App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame)   ' D1373
        If Not isAllowMissingJudgments() And Not btnNext.Enabled And btnNext.CommandArgument <> "" Then
            AllDI.onChangeAction = String.Format("theForm.{0}.disabled=HasUndefined(); ", btnNext.ClientID)
            If btnNextUnas.Visible And btnNextUnas.CommandArgument <> "" Then AllDI.onChangeAction = String.Format("{{ {2} theForm.{1}.disabled = theForm.{0}.disabled; }}", btnNext.ClientID, btnNextUnas.ClientID, AllDI.onChangeAction)
        End If
        Dim isCategory As Boolean = False   ' D3604

        Dim oID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("oid", "")) ' D1425 + Anti-XSS
        Dim aID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("aid", "")) ' D1425 + Anti-XSS

        ' D4015 ===
        Dim sInfodoc As String = ""
        Dim sInfodocURL As String = ""
        Dim sInfodocEditURL As String = ""
        Dim sInfodocWRT As String = ""
        Dim sInfodocWRTURL As String = ""
        Dim sInfodocWRTEditURL As String = ""
        ' D4015 ==

        If TypeOf (Action.ActionData) Is clsAllChildrenEvaluationActionData Then
            Dim data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData)
            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.ParentNode, True))
            tParams.Add(_TEMPL_EVALCOUNT, data.Children.Count.ToString)
            AllDI.ReverseInfodocFrames = data.Children.Count > 1 AndAlso data.Children(0).IsAlternative ' D1902
            ' D2258 ===
            'AllDI.Caption = GetPipeStepTask(Action, IntensityScale) ' D2316
            sTask = GetPipeStepTask(Action, IntensityScale) ' D2316 + D2421
            ' D2258 ==
            AllDI.ParentNodeID = data.ParentNode.NodeID.ToString  ' D1141
            AllDI.CaptionName = data.ParentNode.NodeName    ' D1373
            'AllDI.Caption = AllDI.CaptionName   ' D2421

            If ShowInfoDocs Then    ' D1938
                AllDI.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(data.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), data.ParentNode.NodeID.ToString, data.ParentNode.InfoDoc, True, True, -1) ' D1669 + D2419
                AllDI.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(data.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, data.ParentNode.NodeID, GetRandomString(6, False, False))) ' D1064 + D2419
                AllDI.CaptionInfodocEditURL = PopupRichEditor(If(data.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, data.ParentNode.NodeID, data.ParentNode.NodeGuidID))  ' D2419
            End If
            'If App.ActiveProject.PipeParameters.ShowInfoDocs Then AllDI.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, reObjectType.Node, data.ParentNode.NodeID.ToString, data.ParentNode.InfoDoc)
            isCategory = data.ParentNode.RiskNodeType = RiskNodeType.ntCategory     ' D3604

            AllDI.ShowWRTInfodocs = True
            'AllDI.ShowWRTInfodocs = Not data.ParentNode.IsAlternative AndAlso data.ParentNode.IsTerminalNode

            'sTask = PrepareTask(ParseStringTemplates(ResString("task_MultiDirectdata"), tParams))  ' D1503
            ' D1012 ===
            Dim Lst As New List(Of clsRatingLine)
            Dim ID As Integer = 0
            For Each tAlt As clsNode In data.Children

                If (tAlt.NodeID.ToString = oID AndAlso data.ParentNode.NodeID.ToString = aID) Or
                   (tAlt.NodeID.ToString = aID AndAlso data.ParentNode.NodeID.ToString = oID) Then AllDI._FocusID = ID ' D1425

                Dim DD As clsDirectMeasureData = CType(data.GetJudgment(tAlt), clsDirectMeasureData)
                If ShowInfoDocs Then    ' D4015
                    sInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, CType(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), tAlt.NodeID.ToString, tAlt.InfoDoc, True, True, -1) ' D1669
                    sInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(CType(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType)), _PARAM_ID, tAlt.NodeID, GetRandomString(6, False, False))) ' D1064
                    sInfodocEditURL = PopupRichEditor(CType(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt{3}", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, ID))
                    sInfodocWRT = ""
                    sInfodocWRTURL = ""
                    sInfodocWRTEditURL = ""
                    If AllDI.ShowWRTInfodocs Then
                        sInfodocWRT = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tAlt.NodeID.ToString, App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, data.ParentNode.NodeGuidID), False, True, data.ParentNode.NodeID) ' D1669
                        sInfodocWRTURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&guid={4}&pguid={5}&r={6}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tAlt.NodeID, data.ParentNode.NodeID, tAlt.NodeGuidID, data.ParentNode.NodeGuidID, GetRandomString(6, False, False))) ' D1064 + D2914
                        sInfodocWRTEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=wrt{5}", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, tAlt.NodeGuidID, data.ParentNode.NodeGuidID, ID))
                    End If
                End If
                Lst.Add(New clsRatingLine(ID, ID.ToString, JS_SafeHTML(tAlt.NodeName), Nothing, -1, sInfodoc, DD.Comment, CSng(IIf(DD.IsUndefined, Single.NaN, DD.DirectData)), sInfodocURL, sInfodocEditURL, sInfodocWRT, sInfodocWRTURL, sInfodocWRTEditURL, , , tAlt, SafeFormString(RR_GetScenario(tAlt))))  ' D1064 + D2251 + D4106 + D6822
                ID += 1
            Next
            AllDI.Values = Lst
            ' D1012 ==

        End If

        ' D1428 ===
        If TypeOf (Action.ActionData) Is clsAllCoveringObjectivesEvaluationActionData Then
            Dim data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData)
            tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.Alternative, True))
            tParams.Add(_TEMPL_EVALCOUNT, data.CoveringObjectives.Count.ToString)
            'AllDI.Caption = GetPipeStepTask(Action, IntensityScale) ' D2316
            sTask = GetPipeStepTask(Action, IntensityScale) ' D2316 + D2421
            ' D2258 ==
            AllDI.ParentNodeID = data.Alternative.NodeID.ToString
            AllDI.CaptionName = data.Alternative.NodeName
            'AllDI.Caption = AllDI.CaptionName   ' D2421

            AllDI.ReverseInfodocFrames = True   ' D2393

            If ShowInfoDocs Then    ' D1938
                AllDI.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(data.Alternative.IsAlternative, reObjectType.Alternative, reObjectType.Node), data.Alternative.NodeID.ToString, data.Alternative.InfoDoc, True, True, -1)    ' D1669 + D2419
                AllDI.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(data.Alternative.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, data.Alternative.NodeID, GetRandomString(6, False, False)))    ' D2419
                AllDI.CaptionInfodocEditURL = PopupRichEditor(If(data.Alternative.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, data.Alternative.NodeID, data.Alternative.NodeGuidID))   ' D2419
            End If

            AllDI.ShowWRTInfodocs = ShowInfoDocs    ' D4015
            'sTask = PrepareTask(ParseStringTemplates(ResString("task_MultiDirectdata"), tParams))   ' D1503

            Dim Lst As New List(Of clsRatingLine)
            Dim ID As Integer = 0
            For Each tNode As clsNode In data.CoveringObjectives

                If (tNode.NodeID.ToString = oID AndAlso data.Alternative.NodeID.ToString = aID) Or
                   (tNode.NodeID.ToString = aID AndAlso data.Alternative.NodeID.ToString = oID) Then AllDI._FocusID = ID

                Dim DD As clsDirectMeasureData = CType(data.GetJudgment(tNode), clsDirectMeasureData)
                If ShowInfoDocs Then    ' D4015
                    sInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, CType(IIf(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), tNode.NodeID.ToString, tNode.InfoDoc, True, True, -1)  ' D1669
                    sInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(CType(IIf(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType)), _PARAM_ID, tNode.NodeID, GetRandomString(6, False, False)))
                    sInfodocEditURL = PopupRichEditor(CType(IIf(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt{3}", _PARAM_ID, tNode.NodeID, tNode.NodeGuidID, ID))
                    sInfodocWRT = ""
                    sInfodocWRTURL = ""
                    sInfodocWRTEditURL = ""
                    If AllDI.ShowWRTInfodocs Then
                        sInfodocWRT = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, data.Alternative.NodeID.ToString, App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(data.Alternative.NodeGuidID, tNode.NodeGuidID), False, True, tNode.NodeID) ' D1669
                        sInfodocWRTURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={4}&r={3}", CInt(reObjectType.AltWRTNode), _PARAM_ID, data.Alternative.NodeID, GetRandomString(6, False, False), tNode.NodeID))    ' D2393
                        sInfodocWRTEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=wrt{5}", _PARAM_ID, data.Alternative.NodeID, data.Alternative.NodeGuidID, data.Alternative.NodeGuidID, tNode.NodeGuidID, ID))
                    End If
                End If
                Lst.Add(New clsRatingLine(ID, ID.ToString, JS_SafeHTML(tNode.NodeName), Nothing, -1, sInfodoc, DD.Comment, CSng(IIf(DD.IsUndefined, Single.NaN, DD.DirectData)), sInfodocURL, sInfodocEditURL, sInfodocWRT, sInfodocWRTURL, sInfodocWRTEditURL, , , tNode, SafeFormString(RR_GetScenario(tNode))))    ' D2251 + D4106 + D6822
                ID += 1
            Next
            AllDI.Values = Lst
        End If
        ' D1428 ==

        ' D3250 ===
        If TypeOf (Action.ActionData) Is clsAllEventsWithNoSourceEvaluationActionData Then
            Dim data As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)
            'tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(data.Alternative, True))
            tParams.Add(_TEMPL_EVALCOUNT, data.Alternatives.Count.ToString)
            sTask = GetPipeStepTask(Action, IntensityScale)
            'AllDI.ParentNodeID = data.Alternative.NodeID
            AllDI.CaptionName = Nothing

            AllDI.ReverseInfodocFrames = True

            If ShowInfoDocs Then
                AllDI.CaptionInfodoc = Nothing
                AllDI.CaptionInfodocURL = Nothing
                AllDI.CaptionInfodocEditURL = Nothing
            End If

            AllDI.ShowWRTInfodocs = ShowInfoDocs    ' D4015

            Dim Lst As New List(Of clsRatingLine)
            Dim ID As Integer = 0
            For Each tAlt As clsNode In data.Alternatives

                Dim DD As clsDirectMeasureData = CType(data.GetJudgment(tAlt), clsDirectMeasureData)
                sInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, CType(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), tAlt.NodeID.ToString, tAlt.InfoDoc, True, True, -1)
                sInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(CType(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType)), _PARAM_ID, tAlt.NodeID, GetRandomString(6, False, False)))
                sInfodocEditURL = PopupRichEditor(CType(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt{3}", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, ID))
                Lst.Add(New clsRatingLine(ID, ID.ToString, JS_SafeHTML(tAlt.NodeName), Nothing, -1, sInfodoc, DD.Comment, CSng(IIf(DD.IsUndefined, Single.NaN, DD.DirectData)), sInfodocURL, sInfodocEditURL, Nothing, Nothing, Nothing, , , tAlt, SafeFormString(RR_GetScenario(tAlt))))   ' D4106 + D6822
                ID += 1
            Next
            AllDI.Values = Lst
        End If
        ' D3250 ==

        ' D1373 ===
        If ShowInfoDocs AndAlso AllDI.ShowFramedInfodocs Then
            'sTask = AllDI.Caption  ' -D2421
            AllDI.CaptionInfodocCollapsed = GetCookie("mr_status", If(isHTMLEmpty(AllDI.CaptionInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"   ' D2949
            AllDI.AlternativeInfodocCollapsed = AllDI.CaptionInfodocCollapsed
            AllDI.WRTInfodocCollapsed = AllDI.CaptionInfodocCollapsed
            ' D1864 ===
            AllDI.SaveSizeMessage = ResString("msgOnSaveInfodocSize")
            AllDI.AllowSaveSize = CanUserEditActiveProject
            AllDI.SizesList = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
            ' D1864 ==
        End If
        ' D1373 ==

        ' D1012 ===
        AllDI.ImagePath = ImagePath
        AllDI.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject  ' D1063 + D2505 + D4116
        AllDI.ImageEditInfodoc = "edit_tiny.gif"
        AllDI.ImageViewInfodoc = "view_sm.png"      ' D1426
        AllDI.lblInfodocTitleGoal = ResString("lblShowDescriptionFor")
        AllDI.lblInfodocTitleNode = ResString("lblShowDescriptionFor")
        AllDI.lblInfodocTitleWRT = GetWRTCaption(isCategory)    ' D3031 + D3604 + D3610
        ' D1012 ==
        AllDI.ShowComment = ShowComments    ' D1938
        AllDI.lblCommentTitle = ResString("lblEvaluationPWComment") ' D1012
        AllDI.isCommentCollapsed = GetCookie("comment_status", If(AllDI.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D1069 + D2949
        AllDI.CaptionSaveComment = ResString("btnOK")       ' D2344
        AllDI.ImageCommentExists = ImagePath + "comment.png"     ' D2769
        AllDI.ImageCommentEmpty = ImagePath + "comment_e.png"    ' D2769
        AllDI.msgWrongNumber = ResString("errWrongNumber")
        AllDI.msgWrongNumberRange = String.Format(ResString("errWrongNumberRange"), "0.0", "1.0")
        AllDI.ValuePrecision = 4

        'If Not IsReadOnly AndAlso Not App.ActiveProject.PipeParameters.DisableWarningsOnStepChange Then ClientScript.RegisterStartupScript(GetType(String), "HookSubmit", "_postback_custom_code = 'confirmMissingJudgments();'; HookSubmit();", True) ' D2950

        btnSave.Visible = False  ' D2154
        isMultiRatingsOrDirect = True   ' D2926

        Controls.Add(AllDI)
    End Sub

    Protected Sub SaveMultiDirectInputs(ByRef Action As clsAction)
        Dim RCount As Integer = 0
        If Integer.TryParse(CheckVar("DICount", "0"), RCount) Then
            JudgLogs = ""   ' D7262
            If TypeOf (Action.ActionData) Is clsAllChildrenEvaluationActionData Then
                Dim data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData)
                Dim tNode As clsNode
                Dim Value As Double ' D1858
                For i As Integer = 0 To data.Children.Count - 1
                    tNode = data.Children(i)
                    Dim D As clsDirectMeasureData = CType(data.GetJudgment(tNode), clsDirectMeasureData)
                    Dim fUpdate As Boolean = False
                    JudgLogs += If(JudgLogs = "", "", "; ") + GetNodeName(tNode, 20)    ' D7262
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("DI{0}", i), "")) 'Anti-XSS
                    If sValue = "" Then
                        D.IsUndefined = True
                        fUpdate = True
                        JudgLogs += JudgUndefined   ' D7262
                    Else
                        If String2Double(sValue, Value) Then    ' D1858
                            D.ObjectValue = Value
                            D.IsUndefined = False
                            fUpdate = True
                            JudgLogs += Double2String(Value, 2) ' D7262
                        End If
                    End If
                    Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("Comment{0}", i), D.Comment))   'Anti-XSS
                    If sComment <> D.Comment Then
                        D.Comment = sComment
                        fUpdate = True
                        JudgLogs += JudgComment  ' D7262
                    End If

                    If fUpdate Then
                        'App.ActiveProject.HierarchyObjectives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(data.GetJudgment(tNode))
                        CType(Action.ActionData, clsAllChildrenEvaluationActionData).ParentNode.Judgments.AddMeasureData(data.GetJudgment(tNode))  ' D4401
                        App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, data.GetJudgment(tNode))
                    End If
                Next
            End If

            ' D1428 ===
            If TypeOf (Action.ActionData) Is clsAllCoveringObjectivesEvaluationActionData Then
                Dim data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData)
                Dim tNode As clsNode
                Dim Value As Double ' D1858
                For i As Integer = 0 To data.CoveringObjectives.Count - 1
                    tNode = data.CoveringObjectives(i)
                    Dim D As clsDirectMeasureData = CType(data.GetJudgment(tNode), clsDirectMeasureData)
                    Dim fUpdate As Boolean = False
                    JudgLogs += If(JudgLogs = "", "", "; ") + GetNodeName(tNode, 20)    ' D7262

                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("DI{0}", i), "")) 'Anti-XSS
                    If sValue = "" Then
                        D.IsUndefined = True
                        fUpdate = True
                        JudgLogs += JudgUndefined   ' D7262
                    Else
                        If String2Double(sValue, Value) Then    ' D1858
                            D.ObjectValue = Value
                            D.IsUndefined = False
                            fUpdate = True
                            JudgLogs += Double2String(Value, 2) ' D7262
                        End If
                    End If
                    Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("Comment{0}", i), D.Comment))   'Anti-XSS
                    If sComment <> D.Comment Then
                        D.Comment = sComment
                        fUpdate = True
                        JudgLogs += JudgComment     ' D7262

                    End If

                    If fUpdate Then
                        App.ActiveProject.HierarchyObjectives.GetNodeByID(tNode.NodeID).Judgments.AddMeasureData(data.GetJudgment(tNode))
                        App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.Alternative, data.GetJudgment(tNode))
                    End If
                Next
            End If
            ' D1428 ==

            ' D3250 ===
            If TypeOf (Action.ActionData) Is clsAllEventsWithNoSourceEvaluationActionData Then
                Dim data As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)
                Dim tAlt As clsNode
                Dim Value As Double
                For i As Integer = 0 To data.Alternatives.Count - 1
                    tAlt = data.Alternatives(i)
                    Dim D As clsDirectMeasureData = CType(data.GetJudgment(tAlt), clsDirectMeasureData)
                    Dim fUpdate As Boolean = False

                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("DI{0}", i), "")) 'Anti-XSS
                    If sValue = "" Then
                        D.IsUndefined = True
                        fUpdate = True
                    Else
                        If String2Double(sValue, Value) Then
                            D.ObjectValue = Value
                            D.IsUndefined = False
                            fUpdate = True
                        End If
                    End If
                    Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(String.Format("Comment{0}", i), D.Comment))   'Anti-XSS
                    If sComment <> D.Comment Then
                        D.Comment = sComment
                        fUpdate = True
                    End If

                    If fUpdate Then
                        App.ActiveProject.HierarchyAlternatives.GetNodeByID(tAlt.NodeID).DirectJudgmentsForNoCause.AddMeasureData(data.GetJudgment(tAlt))
                        App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(tAlt, data.GetJudgment(tAlt))
                    End If
                Next
            End If
            ' D3250 ==

        End If
    End Sub
    ' D0678 ==

    Private Function IsPairwiseMeasureType(MT As ECMeasureType) As Boolean  'A0819
        Return MT = ECMeasureType.mtPairwise OrElse MT = ECMeasureType.mtPWAnalogous
    End Function

    ' D4382 ===
    Public ReadOnly Property CombinedUserID As Integer
        Get
            Return App.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedUID
        End Get
    End Property
    ' D4382 ==

    ' D4375 ===
    Private Function CanShowCombinedResults(Data As clsShowLocalResultsActionData) As Boolean
        If CombinedUserID = COMBINED_USER_ID Then Return Data.CanShowGroupResults() Else Return Data.CanShowResultsForUser(CombinedUserID)
    End Function
    ' D4375 ==

    Private Sub CreateLocalResults(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim Data As clsShowLocalResultsActionData = CType(Action.ActionData, clsShowLocalResultsActionData) 'C0464

        ' D0120 ===
        Dim tParams As New Dictionary(Of String, String)
        tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(Data.ParentNode, False))  ' D0558
        ' D2137 ===
        Dim fIsPWOutcomes As Boolean = Data.PWOutcomesNode IsNot Nothing AndAlso Data.ParentNode.MeasureType = ECMeasureType.mtPWOutcomes
        If fIsPWOutcomes Then tParams.Add(_TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Data.PWOutcomesNode, False))

        ' D2331 ===
        Dim fCanShowTitle As Boolean = True
        ' D2335 ===
        Select Case App.ActiveProject.PipeParameters.LocalResultsView
            Case ResultsView.rvIndividual
                fCanShowTitle = Data.CanShowIndividualResults
            Case ResultsView.rvGroup
                fCanShowTitle = CanShowCombinedResults(Data)    ' D4375 Data.CanShowGroupResults
            Case Else
                fCanShowTitle = Data.CanShowIndividualResults OrElse CanShowCombinedResults(Data)   ' D4375 Data.CanShowGroupResults
        End Select
        Dim fHasResults As Boolean = fCanShowTitle ' D7057
        ' D0120 + D2237 + D2331 + D2335 ==
        sTask = GetPipeStepTask(Action, IntensityScale, False, False, False) ' D2316 + D2692 + D2830 + D4053
        ' D2258 ==

        ' D1457 ===
        If CheckVar("debug", "") <> "" Then
            Dim lblDebug As New Label
            '' AC, please modify next line
            Dim pwData As clsPairwiseMeasureData = CType(Data.ParentNode.Judgments, clsPairwiseJudgments).GetNthMostInconsistentJudgment(New clsCalculationTarget(CalculationTargetType.cttUser, Data.ParentNode.Hierarchy.ProjectManager.GetUserByID(Data.UserID)), 1)
            If pwData IsNot Nothing Then
                lblDebug.Text = String.Format("<div class='text small gray' style='text-align:center'>1<sup>st</sup> node: {0}; &nbsp; 2<sup>nd</sup> node: {1}; &nbsp; Advantage: {2}; &nbsp; Value: {3}</div>", Data.ParentNode.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeName + " (ID: " + pwData.FirstNodeID.ToString + ")", Data.ParentNode.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeName + " (ID: " + pwData.SecondNodeID.ToString + ")", pwData.Advantage, pwData.Value)
            Else
                lblDebug.Text = String.Format("<div class='text small gray' style='text-align:center'>1<sup>st</sup> node: {0}; &nbsp; 2<sup>nd</sup> node: {1}; &nbsp; Advantage: {2}; &nbsp; Value: {3}</div>", "#1, Obj1", "#2, Obj2", 1, 1)
            End If
            Controls.Add(lblDebug)
        End If
        ' D1457 ==

        ' D2331 ===
        Dim sStep As Integer = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(Data.ParentNode, CurStep)
        Dim Neg As Integer = 0
        Dim Pos As Integer = 0
        'Dim Undef As Integer = 0    ' D2353
        Dim tTotal As Integer = 0   ' D2367
        If IsPairwiseMeasureType(Data.ParentNode.MeasureType) Then 'A0819
            For i = sStep + 1 To CurStep - 1
                Dim tAction As clsAction = GetAction(i)
                If tAction.ActionType = ActionType.atAllPairwise Then
                    Dim tData As clsAllPairwiseEvaluationActionData = CType(tAction.ActionData, clsAllPairwiseEvaluationActionData)
                    tTotal = tData.Judgments.Count  ' D2367
                    For Each tVal As clsPairwiseMeasureData In tData.Judgments
                        If Not tVal.IsUndefined Then
                            If tVal.Advantage > 0 Then Pos += 1 Else If tVal.Advantage < 0 Then Neg += 1
                            'Else
                            '    Undef += 1  ' D2353
                        End If
                    Next
                End If
                If tAction.ActionType = ActionType.atPairwise Then
                    Dim tVal As clsPairwiseMeasureData = CType(tAction.ActionData, clsPairwiseMeasureData)
                    If Not tVal.IsUndefined Then
                        If tVal.Advantage > 0 Then Pos += 1 Else If tVal.Advantage < 0 Then Neg += 1
                        'Else
                        '    Undef += 1    ' D2353
                    End If
                End If
            Next
            If Not IsIntensities AndAlso (Pos = tTotal OrElse Neg = tTotal) Then fCanShowTitle = False ' D2353 + D2367
        End If
        ' D2331 ==

        ' D0565 ===
        If ShowSLLocalResults AndAlso (IsPairwiseMeasureType(Data.ParentNode.MeasureType) OrElse fIsPWOutcomes) Then 'A0819
            CreateSLResults(Data, Controls)
            'tblHTMLResults.Attributes.Add("style", "display:none;")    ' D0855
            tblHTMLResults.Visible = False      ' D0937
            'End If ' D0855
        Else   ' -D0855 + D0937
            ' D0565 ==
            Dim GridShowResults As ctrlShowResults2 = CreateResults(Data, Controls)    ' D3388
            ' D1506 ===
            If Data.ParentNode IsNot Nothing Then

                ' D2152 ===
                If fIsPWOutcomes Then
                    Dim RS As clsRatingScale
                    If Data.ParentNode.IsAlternative Then
                        RS = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                    Else
                        RS = CType(Data.ParentNode.MeasurementScale, clsRatingScale)
                    End If
                    If RS IsNot Nothing Then GridShowResults.ShowInconsistencyRatio = App.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleShowInconsistencRatioy(RS)
                End If
                ' D2152 ==

                If sStep >= 0 Then
                    GridShowResults.ButtonReturnCaption = ResString("btnReviewMissedJudgments")
                    GridShowResults.ButtonReturnURL = String.Format("?{0}={1}{2}", _PARAM_STEP, sStep + 1, GetTempThemeURI(True))
                    If IsPairwiseMeasureType(Data.ParentNode.MeasureType) Then 'A0819
                        ' D2331 ===
                        If Pos > 5 OrElse Neg > 5 Then
                            GridShowResults.ButtonReturnCaption = ResString("btnReviewJudgments")
                            GridShowResults.MessageCustom = ResString("msgSameSideJudgments")
                        End If
                        ' D2331 ==
                    End If
                End If
            End If
            ' D1506 ==

            If GridShowResults.ShowInconsistencyRatio AndAlso Data.ParentNode IsNot Nothing AndAlso Data.ParentNode.MeasureType = ECMeasureType.mtPWOutcomes AndAlso Data.PWOutcomesNode Is Nothing Then GridShowResults.ShowInconsistencyRatio = False ' D2653

        End If  ' -D0855
        ' D2331 ===

        ' D4328 ===
        Dim sTitle As String = GetPipeStepHint(Action, IIf(IsIntensities, IntensityScale, Nothing), True, True)
        If CanEditClusterPhrase() Then
            divEditTitle.Visible = True
        End If
        divLocalResultsHeader.Visible = True
        lblResultsTitle.Text = sTitle
        lblResultsTitle.Attributes.Add("style", String.Format("padding:2px 6px; margin-left:{1}{0}", IIf(fCanShowTitle, "", "display:none;"), IIf(divEditTitle.Visible, "6em;", "0px;")))
        If Not fCanShowTitle Then trHead.Attributes("style") = "display:none;" Else Page.ClientScript.RegisterStartupScript(GetType(String), "Flash", String.Format("setTimeout(""DoFlashWRT('{0}');"", 1000);", lblResultsTitle.ClientID), True) ' D2830
        ' D2331 + D4328 ==

        ' D7057 ===
        Dim sExtraRes As String = ""
        If (fHasResults AndAlso Not IsIntensities) Then
            sExtraRes = If(App.isRiskEnabled, If(isImpact, "lblReviewLocalResultsRiskImpact", "lblReviewLocalResultsRisk"), "lblReviewLocalResults")
            If Data.ParentNode Is Nothing AndAlso sExtraRes <> "" Then sExtraRes += "Goal"
            If sExtraRes <> "" Then
                trGoalText.Visible = True
                ' D7062 ===
                Dim sTpl As String = If(Data.ParentNode IsNot Nothing AndAlso Data.ParentNode.IsTerminalNode, _TEMPL_ALTERNATIVES, _TEMPL_OBJECTIVES)
                sExtraRes = ParseString(String.Format(ResString(sExtraRes), sTpl))
                tdGoalText.InnerHtml = String.Format("<div style='text-align:center; padding:4px 5%; margin:4px auto;'><b>{0}</b></div>", ParseStringTemplates(sExtraRes, tParams))
                ' D7062 ==
            End If
        End If
        ' D7057 ==

        ' -D2834
        ''A0862 + D2832 ===
        'If fCanShowTitle AndAlso Data.ParentNode IsNot Nothing AndAlso Data.ParentNode.ParentNode IsNot Nothing AndAlso Data.ParentNode.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous Then
        '    Dim sKnownLikelihood As String = ""
        '    Dim nl As List(Of KnownLikelihoodDataContract) = Data.ParentNode.ParentNode.GetKnownLikelihoods()
        '    For Each item As KnownLikelihoodDataContract In nl
        '        If item.GuidID.Equals(Data.ParentNode.NodeGuidID) AndAlso item.Value > 0 Then sKnownLikelihood = item.Value.ToString
        '    Next
        '    If Not String.IsNullOrEmpty(sKnownLikelihood) Then
        '        sKnownLikelihood = String.Format(ResString("lblParentNodeKnownLikelihood"), Data.ParentNode.NodeName, sKnownLikelihood)
        '        Dim lblKL As New Label
        '        lblKL.Text = String.Format("<h6 style='padding:0px; margin:1ex; 0px; 0px; 0px;'>{0}</h6>", sKnownLikelihood)
        '        Controls.Add(lblKL)
        '    End If
        'End If
        ''A0862 + D2832 ==
    End Sub

    Private Sub CreateGlobalResults(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim Data As clsShowGlobalResultsActionData = CType(Action.ActionData, clsShowGlobalResultsActionData) 'C0464
        ' D0120 ===
        Dim tParams As New Dictionary(Of String, String)
        Dim tGoal As clsNode = App.ActiveProject.HierarchyObjectives.Nodes(0)   ' D2094
        tParams.Add(_TEMPL_NODENAME, GetWRTNodeNameWithPath(tGoal, True))   ' D0558 + D2094
        ' D1446 ===
        ' D0120 ==
        ActionContent.Visible = False
        trGoalText.Visible = True
        Dim sRes As String = ""
        ' D2258 ===
        If App.isRiskEnabled Then
            sRes = CStr(IIf(isLikelihood, "lblEvaluationResultOverallRisk", "lblEvaluationResultOverallImpact"))
        Else
            sRes = "lblEvaluationResultOverall"
        End If
        tdGoalText.InnerHtml = String.Format("<div style='text-align:center'><b>{0}</b></div>", PrepareTask(ParseStringTemplates(ResString(sRes), tParams), IntensityScale))   ' D1446 + D1862 + D1932
        ' D1446 + D2258 ==
        sTask = GetPipeStepTask(Action, IntensityScale, False, False, False) ' D2316 + D2692 + D2830 + D4053
        ' D2258 ==
        CreateResults(Data, Controls)

        If Data.WRTNode Is Nothing Then 'C0485
            Data.WRTNode = App.ActiveProject.HierarchyObjectives.Nodes(0) ' D0232 
        End If

    End Sub
    ' D0023 ==

    ' D0025 ===
    Private Function CreateResults(Data As Object, Controls As ControlCollection) As ctrlShowResults2   ' D3388
        ' D0123 ===
        ' D3069 ===
        If CheckVar(_PARAM_ACTION, "") = "optimization" Then
            App.ActiveProject.ProjectManager.OptimizationOn = Not App.ActiveProject.ProjectManager.OptimizationOn
            Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(True)), True)
        End If
        ' D3069 ==

        'Dim dt As DateTime = Now    ' D3069
        ' D0157 + D3388 ===
        Dim res As ctrlShowResults2 = CType(LoadControl("~/ctrlShowResults2.ascx"), ctrlShowResults2) ' D3388
        res.AllowSorting = True
        res.ID = "GridShowResults"

        res.Visible = True  ' D0672
        ' D3388 ==
        tdExtraContent.Visible = True   ' D0938
        ' D0157 ==

        ' D2655 ===
        Dim fIsNodes As Boolean = False
        If App.isRiskEnabled AndAlso TypeOf (Data) Is clsShowLocalResultsActionData Then
            Dim LR As clsShowLocalResultsActionData = CType(Data, clsShowLocalResultsActionData)
            If LR.ParentNode IsNot Nothing Then fIsNodes = Not LR.ParentNode.IsTerminalNode
        End If
        ' D2655 ==

        res.columnValueMy = ResString("lblEvaluationLegendUser")
        res.columnValueCombined = ResString("lblEvaluationLegendCombined")
        res.columnName = ResString(If(Not App.isRiskEnabled, "tblEvaluationResultName", If(isLikelihood, "tblEvaluationResultNameRisk", "tblEvaluationResultNameImpact") + If(fIsNodes, "Obj", "")))   ' D1926 + D2258 + D2271 + D2655
        res.columnLikelihood = ResString("tblEvaluationResultLIkelihood")   ' D2691
        res.columnGraph = ResString(If(Not App.isRiskEnabled, "tblEvaluationResultGraph", If(isLikelihood, "tblEvaluationResultGraphRisk", "tblEvaluationResultGraphImpact")))    ' D1926 + D2258 + D2271
        res.messageNoIndividualResults = ResString("msgNoEvalDataIndividualResults")
        res.messageNoCombinedResults = ResString("msgNoEvalDataGroupResults")
        res.InconsistencyRatioMyTemplate = String.Format("<b>{0}</b>: <span class='label_my'>{1}</span>", ResString("lblEvaluationInconsistencyRatioMy"), "{0}")
        res.InconsistencyRatioCombinedTemplate = String.Format("<b>{0}</b>: <span class='label_combined'>{1}</span>", ResString("lblEvaluationInconsistencyRatioCombined"), "{0}")
        res.BlankImagePath = BlankImage ' D0136
        res.isCISEnabled = App.ActiveProject.PipeParameters.UseCISForIndividuals    ' D1456
        res.messageTurnCIS = ResString("msgTurnCIS")    ' D1456
        res.lblExpectedValueIndividual = ResString("lblExpectedValueIndiv")    ' D2130
        res.lblExpectedValueCombined = ResString("lblExpectedValueComb")    ' D2130

        ' D0631 ===
        Dim opt As Array
        opt = [Enum].GetValues(GetType(AlternativeNormalizationOptions))
        Dim tpl As String = ResString("lblProps_Template")
        Dim NormNames(opt.Length - 1) As String
        For Each St As AlternativeNormalizationOptions In opt   ' D2114
            If St <> AlternativeNormalizationOptions.anoMultipleOfMin Then  ' D2114
                Dim Name As String = St.ToString
                Dim idx As Integer = CInt(St)
                If NormNames.GetUpperBound(0) < idx Then Array.Resize(NormNames, idx + 1)
                If App.isRiskEnabled AndAlso Not App.ActiveProject.isImpact Then Name += "Likelihood" ' D2496
                NormNames(idx) = ResString(String.Format(tpl, Name))
            End If
        Next
        res.NormalizationCaptions = NormNames
        res.NormalizationModeCaption = ResString("lbl_NormalizationMode")
        If App.isRiskEnabled Then 'A1212
            res.NormalizationMode = AlternativeNormalizationOptions.anoUnnormalized 'A1212
        Else
            res.NormalizationMode = AlternativeNormalizationOptions.anoPriority ' D3503
        End If
        ' D0631 ==

        res.ShowGraphForMax = True ' D0150 + D0323 + D0345

        res.UserID = App.ActiveProject.ProjectManager.UserID     ' D0046
        res.ShowInconsistencyRatio = App.ActiveProject.PipeParameters.ShowConsistencyRatio
        res.Show_Normalization = Not App.isRiskEnabled AndAlso Not If(TypeOf (Data) Is clsShowGlobalResultsActionData, PM.Parameters.EvalHideGlobalNormalizationOptions, PM.Parameters.EvalHideLocalNormalizationOptions) ' D7238 + D7556
        If PM.Parameters.SpecialMode = _OPT_MODE_AREA_VALIDATION2 Then res.ValuePrecision = 0       ' D7568
        res.Data = Data

        ' D0152 ===
        If TypeOf (Data) Is clsShowGlobalResultsActionData Then 'C0464
            res.StyleGraphMy = "graph_global_my"
            res.StyleGraphCombined = "graph_global_combined"
            res.StyleLabelMy = "label_global_my"
            res.StyleLabelCombined = "label_global_combined"
            res.ShowAltsIdx = App.ActiveProject.ProjectManager.Parameters.ResultsGlobalShowIndex AndAlso App.ActiveProject.ProjectManager.Parameters.NodeVisibleIndexMode <> IDColumnModes.Rank   ' D3786 + D6685

            If Not IsPostBack Then
                res.SortExpression = App.ActiveProject.PipeParameters.GlobalResultsSortMode ' D0937
                If res.SortExpression = ResultsSortMode.rsmName Or res.SortExpression = ResultsSortMode.rsmNumber Then res.SortDirection = SortDirection.Ascending ' D1017
            End If

            res.messageNoAnyResults = PrepareTask(ResString("msgNoOverallResults"))  ' D0348 + D1927
            res.ShowInfodocs = App.ActiveProject.PipeParameters.ShowInfoDocs    ' D2112
            res.ImgPath = ImagePath  ' D2112

            res.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject ' D3538 + D4116
            res.InfodocURL = PageURL(If(res.CanEditInfodocs, _PGID_RICHEDITOR, _PGID_EVALUATE_INFODOC))    ' D2112 + D3538

            ' D0349 ===
            Select Case App.Options.BackDoor
                Case _BACKDOOR_PLACESRATED
                    res.ShowTree = False
                    res.ShowMaxAlternatives = 10
                    'res.ShowRefreshButton = False   ' D0356 + D2608
                Case Else
                    res.ShowTree = True
                    res.ShowMaxAlternatives = 0
            End Select
            ' D0349 ==

            If App.isRiskEnabled AndAlso Not App.ActiveProject.isImpact AndAlso Not IsPostBack Then res.NormalizationMode = AlternativeNormalizationOptions.anoUnnormalized ' D3532

            ' D3691 ===
            If App.ActiveProject.ProjectManager.PipeParameters.ShowExpectedValueGlobal Then
                res.ShowOverallExpectedValueMode = App.ActiveProject.PipeParameters.GlobalResultsView
            Else
                res.ShowOverallExpectedValueMode = ResultsView.rvNone
            End If
            ' D3691 ==
            res.Show_Bars = PM.Parameters.ResultsGlobalShowBars ' D7561

        Else

            If Not IsPostBack Then res.SortExpression = App.ActiveProject.PipeParameters.LocalResultsSortMode ' D0937
            res.ShowAltsIdx = App.ActiveProject.ProjectManager.Parameters.ResultsLocalShowIndex AndAlso App.ActiveProject.ProjectManager.Parameters.NodeVisibleIndexMode <> IDColumnModes.Rank    ' D3786 + D6685

            res.messageNoAnyResults = ResString("msgNoLocalResults")    ' D0348

            If res.Show_Normalization AndAlso res.CurrentNode IsNot Nothing AndAlso Not res.CurrentNode.IsTerminalNode Then res.Show_Normalization = False  ' D7297

            ' -D2608
            '' D0356 ===
            'Select Case App.Options.BackDoor
            '    Case _BACKDOOR_PLACESRATED
            '        res.ShowRefreshButton = False
            'End Select
            '' D0356 ==
            res.Show_Bars = PM.Parameters.ResultsLocalShowBars  ' D7561

        End If
        ' D0152 ==
        ' D0123 ==

        ' D4376 ===
        App.ActiveProject.ProjectManager.CheckCustomCombined()  ' D4382
        res.CombinedUserID = CombinedUserID
        If App.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName <> "" Then res.columnValueCombined = App.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName ' D4384
        ' D4376 ==

        'Dim StopWatch As TimeSpan = Now.Subtract(dt)    ' D3069
        'If SHOW_OPTIMIZATION_SWICTH Then ScriptManager.RegisterStartupScript(Me, GetType(String), "SW", String.Format("$('#divStopWatch').html('{0}');", StopWatch.ToString), True) ' D3069
        placeResults.Controls.Add(res)
        tblHTMLResults.Visible = True   ' D3677

        Return res  ' D3388
    End Function
    ' D0025 ==

    '-A0844 ' D2670 ===
    'Public Function GetHID() As String
    '    If IsIntensities OrElse String.IsNullOrEmpty(SessVar(SESSION_ORIGINAL_HID)) Then Return SessVar(SESSION_ORIGINAL_HID) Else Return CStr(App.ActiveProject.ProjectManager.ActiveHierarchy)
    'End Function
    '' D2670 ==

    '' D2926 ===
    'Public Function HotKeysHint() As String
    '    Dim sHint As String = ResString("hintEvalHotkeys")
    '    If isMultiRatingsOrDirect Then sHint += ResString("hintMultiEvalHotkeys")
    '    Return sHint
    'End Function
    '' D2926 ==

    ' D0565 ===
    Public Function GetParams() As String
        Return String.Format("{0}{1}?step={2}&uid={3}&rnd={4}{5}", _URL_EVALUATE, "ResultsXML.aspx", CurStep, IIf(IsReadOnly, ReadOnly_UserID, App.ActiveUser.UserID), GetRandomString(6, True, False), IIf(IsIntensities, "&intensities=1", "")) ' D1199 + D1200 + D1481 + D1485 + D1865
    End Function

    Private Sub CreateSLResults(Data As Object, Controls As ControlCollection)
        pnlSLResults.Visible = True
        tdExtraContent.Visible = True   ' D0938
        btnNext.OnClientClick = String.Format("if (sl_next==1) {{ sl_next = 2; CallSLGrid(); }} {0}", btnNext.OnClientClick)    ' D1491 + D1493
        ScriptManager.RegisterOnSubmitStatement(Me, GetType(String), "CheckSLResults", "if (sl_next==2) return false;")  ' D1491 + D1493
        CreateIntermediateResults()
    End Sub
    ' D0565 ==

    ' D0173 ===
    Private Sub CreateRegularUtilityCurve(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim tParams As New Dictionary(Of String, String)
        Dim RUC As ctrlUtilityCurve = CType(LoadControl("~/ctrlUtilityCurve.ascx"), ctrlUtilityCurve)
        RUC.sRootPath = _URL_ROOT ' D1593
        RUC.ID = "RUC"  ' D1239
        RUC.Data = Action.ActionData
        RUC.YAxisCaption = getScaleWording()
        Dim sPrty As String = CStr(IIf(App.isRiskEnabled, IIf(IsRiskWithControls, "lblEffectiveness", IIf(App.ActiveProject.isImpact, "lblContributionImpact", "lblContributionLikelihood")), "lblContribution"))    ' D2560 + D2696
        ' D2510 ===
        Dim tParentNode As clsNode = Nothing
        Dim tAlt As clsNode = Nothing
        Dim tData As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData)
        If IsRiskWithControls Then
            If tData.Assignment IsNot Nothing AndAlso tData.Control IsNot Nothing Then
                tParentNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tData.Assignment.ObjectiveID)
                RUC.AlternativeName = GetControlName(tData.Control) ' D4187
            End If
        Else
            tParentNode = CType(tData.Node.Hierarchy.GetNodeByID(CType(tData.Judgment, clsUtilityCurveMeasureData).ParentNodeID), clsNode)   ' D1011
            If tParentNode IsNot Nothing Then
                If tParentNode.IsTerminalNode Then
                    tAlt = tData.Node.Hierarchy.ProjectManager.AltsHierarchy(tData.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(CType(tData.Judgment, clsUtilityCurveMeasureData).NodeID)
                Else
                    tAlt = CType(tData.Node.Hierarchy.GetNodeByID(CType(tData.Judgment, clsUtilityCurveMeasureData).NodeID), clsNode)
                End If
            End If
        End If
        'If tAlt IsNot Nothing AndAlso tAlt.isUncontributedAlternative Then tParentNode = Nothing ' D4213
        If tAlt IsNot Nothing AndAlso App.ActiveProject.IsRisk AndAlso tAlt.IsAlternative AndAlso App.ActiveProject.HierarchyObjectives.GetUncontributedAlternatives().Contains(tAlt) Then tParentNode = Nothing ' D4213 + D4589
        ' D2510 ==

        ' D0569 ===
        RUC.Comment = CType(CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsUtilityCurveMeasureData).Comment
        RUC.lblCommentTitle = ResString("lblEvaluationPWComment")
        RUC.isCommentCollapsed = GetCookie("comment_status", If(RUC.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D1069 + D2949
        RUC.ShowComment = ShowComments  ' D1938
        RUC.ChangeFlagID = isChanged.ClientID
        ' D0569 ==
        ' D6712 ===
        If Not isAllowMissingJudgments() AndAlso btnNext.Enabled AndAlso btnNext.CommandArgument <> "" Then ' D6720
            RUC.onChangeAction = String.Format("theForm.{0}.disabled=HasUndefined(); ", btnNext.ClientID)
            If btnNextUnas.Visible And btnNextUnas.CommandArgument <> "" Then RUC.onChangeAction += String.Format("theForm.{1}.disabled = theForm.{0}.disabled; ", btnNext.ClientID, btnNextUnas.ClientID)
        End If
        ' D6712 ==

        ' D0222 ===
        If tParentNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, True)) ' D0558 + D1011
        If tAlt IsNot Nothing Then tParams.Add(_TEMPL_NODENAME, JS_SafeHTML(tAlt.NodeName)) ' D1011
        ' D0222 ==

        sTask = GetPipeStepTask(Action, IntensityScale)    ' D2316

        ' D1011 ==
        RUC.ImagePath = ImagePath
        RUC.lblInfodocTitleGoal = ResString("lblShowDescriptionFor")
        RUC.lblInfodocTitleNode = ResString("lblShowDescriptionFor")
        RUC.lblInfodocTitleWRT = GetWRTCaption(tParentNode Is Nothing OrElse tParentNode.RiskNodeType = RiskNodeType.ntCategory)  ' D3031 + D3610

        RUC.ShowFramedInfodocs = CheckVar("framed", App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame)

        ' D4933 ===
        RUC.lblInfodocTitleScale = ResString("lblScaleInfodoc")
        If ShowInfoDocs Then
            Dim scale As clsCustomUtilityCurve = CType(tData.Judgment, clsUtilityCurveMeasureData).UtilityCurve
            RUC.ScaleInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.MeasureScale, scale.GuidID.ToString, scale.Comment, True, True, -1)
            RUC.ScaleInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid={1}&r={2}", CInt(reObjectType.MeasureScale), scale.GuidID, GetRandomString(10, True, False)))
            RUC.ScaleInfodocEditURL = PopupRichEditor(reObjectType.MeasureScale, String.Format("field=infodoc&guid={0}&callback=scale", scale.GuidID))
        End If
        ' D4933 ==

        If tParentNode IsNot Nothing Then
            RUC.CaptionName = tParentNode.NodeName
            If ShowInfoDocs Then
                RUC.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.Node, tParentNode.NodeID.ToString, tParentNode.InfoDoc, True, True, -1)    ' D1669
                RUC.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(tParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tParentNode.NodeID, tParentNode.NodeGuidID, GetRandomString(10, True, False)))  ' D2418
                RUC.CaptionInfodocEditURL = PopupRichEditor(If(tParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, tParentNode.NodeID, tParentNode.NodeGuidID))    ' D2418
            End If
        End If
        ' D4346 ==
        If IsRiskWithControls AndAlso ShowInfoDocs Then
            If tData.Assignment IsNot Nothing AndAlso tData.Control IsNot Nothing Then
                RUC.AlternativeName = tData.Control.Name
                RUC.AlternativeInfodoc = GetControlInfodoc(App.ActiveProject, tData.Control, False)
                RUC.AlternativeInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid={1}&r={2}", CInt(reObjectType.Control), tData.Control.ID, GetRandomString(10, True, False)))
                RUC.AlternativeInfodocEditURL = PopupRichEditor(reObjectType.Control, String.Format("field=infodoc&guid={0}&callback=node", tData.Control.ID))
            End If
            ' D4347 ===
            If tData.Assignment IsNot Nothing AndAlso Not tData.Assignment.EventID.Equals(Guid.Empty) Then
                Dim tNode As clsNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tData.Assignment.EventID)
                If tNode IsNot Nothing Then
                    RUC.WRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), tNode.NodeID.ToString, tNode.InfoDoc, True, True, -1)
                    RUC.WRTInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tNode.NodeID, GetRandomString(10, True, False)))
                    RUC.WRTInfodocEditURL = PopupRichEditor(If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=wrt", _PARAM_ID, tNode.NodeID, tNode.NodeGuidID))
                    RUC.lblInfodocTitleWRT = tNode.NodeName
                End If
            End If
            ' D4347 ==
        End If
        ' D4346 ==

        If tAlt IsNot Nothing AndAlso (tParentNode Is Nothing OrElse Not tAlt.NodeGuidID.Equals(tParentNode.NodeGuidID)) Then   ' D3384
            RUC.AlternativeName = tAlt.NodeName
            If ShowInfoDocs Then
                Dim sWRTInfodoc As String = ""
                If tParentNode IsNot Nothing Then
                    sWRTInfodoc = App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, tParentNode.NodeGuidID)
                End If
                If tAlt IsNot Nothing Then
                    RUC.AlternativeInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), tAlt.NodeID.ToString, tAlt.InfoDoc, True, True, -1)    ' D1669 + D2419
                    RUC.AlternativeInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, GetRandomString(10, True, False)))    ' D2418
                    RUC.AlternativeInfodocEditURL = PopupRichEditor(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID))  ' D2419
                    If tParentNode IsNot Nothing Then
                        RUC.WRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tAlt.NodeID.ToString, sWRTInfodoc, False, True, tParentNode.NodeID)  ' D1669
                        RUC.WRTInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&guid={4}&pguid={5}&r={6}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tAlt.NodeID, tParentNode.NodeID, tAlt.NodeGuidID, tParentNode.NodeGuidID, GetRandomString(10, True, False))) ' D2418
                        RUC.WRTInfodocEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=wrt", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, tParentNode.NodeID, tParentNode.NodeGuidID))
                    End If
                End If
                If RUC.ShowFramedInfodocs Then
                    RUC.AlternativeInfodocCollapsed = GetCookie("alt_status", If(isHTMLEmpty(RUC.AlternativeInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0" ' D1064 + D1069 + D2949
                    RUC.CaptionInfodocCollapsed = GetCookie("node_status", If(isHTMLEmpty(RUC.CaptionInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"    ' D1064 + D1069 + D2949
                    RUC.WRTInfodocCollapsed = GetCookie("wrt_status", If(isHTMLEmpty(RUC.WRTInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"   ' D1064 + D1069 + D2949
                    ' D1864 ===
                    RUC.SaveSizeMessage = ResString("msgOnSaveInfodocSize")
                    RUC.AllowSaveSize = CanUserEditActiveProject
                    RUC.SizesList = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
                    ' D1864 ==
                End If
            End If
        End If
        RUC.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject  ' D1063 + D2505 + D4116
        RUC.ImageEditInfodoc = "edit_tiny.gif"
        RUC.ImageViewInfodoc = "view_sm.png"    ' D1359
        ' D1011 ==

        Controls.Add(RUC)
    End Sub
    ' D0173 ==

    ' D0569 ===
    Private Sub SaveRegularUtilityCurve(Action As clsAction)
        Dim Judgment As clsUtilityCurveMeasureData = CType(CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsUtilityCurveMeasureData)
        Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("UCComment", Judgment.Comment))   'Anti-XSS
        Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("UCValue", Judgment.SingleValue.ToString))  'Anti-XSS
        If sComment <> Judgment.Comment OrElse sValue <> Judgment.SingleValue.ToString OrElse (sValue <> "" AndAlso Judgment.IsUndefined) Then
            Judgment.Comment = sComment
            ' D2510 + D3301 ===
            If sValue = "" Then
                Judgment.IsUndefined = True
                JudgLogs = JudgUndefined ' D7262
            Else
                ' D3301 ==
                Dim tVal As Double
                If String2Double(sValue, tVal) Then
                    Judgment.ObjectValue = tVal
                    'Judgment.IsUndefined = tVal < Judgment.UtilityCurve.Low ' D3855
                    Judgment.IsUndefined = False
                    JudgLogs = Double2String(tVal, 2)   ' D7262
                End If
            End If
            If IsRiskWithControls Then
                App.ActiveProject.ProjectManager.PipeBuilder.SaveControlJudgment(Action)
            Else
                CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Node.Judgments.AddMeasureData(Judgment) ' D4401
                'App.ActiveProject.HierarchyObjectives.GetNodeByID(Judgment.ParentNodeID).Judgments.AddMeasureData(Judgment)
                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(App.ActiveProject.HierarchyObjectives.GetNodeByID(Judgment.ParentNodeID), Judgment)
            End If
            ' D2510 ==
        End If
    End Sub
    ' D0569 ==

    ' D0175 ===
    Private Sub CreateDirectData(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        sTask = GetPipeStepTask(Action, IntensityScale) ' D2316
        ' D2258 ==
        Dim DD As ctrlDirectData = CType(LoadControl("~/ctrlDirectData.ascx"), ctrlDirectData)
        DD.sRootPath = _URL_ROOT ' D1593
        DD.ID = "DD"    ' D1239
        DD.Data = Action.ActionData
        ' D0312 + D2503 ===
        Dim tAlt As clsNode = Nothing
        Dim tParentNode As clsNode = Nothing
        Dim tData As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData)
        If IsRiskWithControls Then
            If tData.Assignment IsNot Nothing AndAlso tData.Control IsNot Nothing Then
                tAlt = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tData.Assignment.ObjectiveID)
                If tAlt Is Nothing Then tAlt = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(tData.Assignment.ObjectiveID) ' D4347
                If tAlt Is Nothing Then tAlt = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tData.Assignment.ObjectiveID)    ' D6567
                DD.CaptionName = GetControlName(tData.Control)  ' D4187
                End If
            Else
            'If Action.ActionData IsNot Nothing AndAlso tData.Judgment IsNot Nothing Then tAlt = Action.ActionData.Node.Hierarchy.ProjectManager.AltsHierarchy(Action.ActionData.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(CType(Action.ActionData.Judgment, clsDirectMeasureData).NodeID) ' D1010 + D2345
            If Action.ActionData IsNot Nothing AndAlso tData.Judgment IsNot Nothing Then
                If tData.Node.Hierarchy.ProjectManager.FeedbackOn And Action.IsFeedback Then
                    tAlt = tData.Node.Hierarchy.ProjectManager.Hierarchy(tData.Node.Hierarchy.ProjectManager.ActiveHierarchy).GetNodeByID(CType(tData.Judgment, clsDirectMeasureData).NodeID) ' D1010 + D2345
                Else
                    tAlt = tData.Node.Hierarchy.ProjectManager.AltsHierarchy(tData.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(CType(tData.Judgment, clsDirectMeasureData).NodeID) ' D1010 + D2345
                End If
            End If
            tParentNode = tData.Node     ' D1010
        End If
        ' D0312 + D2503 ==

        DD.lblCommentTitle = ResString("lblEvaluationPWComment")    ' D1010
        If Action.ActionData IsNot Nothing AndAlso CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Judgment IsNot Nothing Then DD.Comment = CType(CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsDirectMeasureData).Comment ' D0167 'C0464 + D2345 + D4178
        DD.isCommentCollapsed = GetCookie("comment_status", If(DD.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"  ' D1069 + D2949
        DD.ShowComment = ShowComments    ' D0167 + D1938
        DD.msgWrongNumber = ResString("errWrongNumber")     ' D0269 + D0678
        DD.msgWrongNumberRange = String.Format(ResString("errWrongNumberRange"), "0.0", "1.0")    ' D0678
        DD.Promt = ResString("lblDDPromt")
        DD.ChangeFlagID = isChanged.ClientID
        DD.NextButtonClientID = btnNext.ClientID    ' D0786
        ' D0255 ===
        If Not isAllowMissingJudgments() And Not btnNext.Enabled And btnNext.CommandArgument <> "" Then   ' D0598
            DD.onChangeAction = String.Format("theForm.{0}.disabled=(theForm.DDValue.value=='' || theForm.DDValue.value=='{1}'); ", btnNext.ClientID, JS_SafeString(ctrlPairwiseBar.pwUndefinedValue.ToString)) ' D6708
            If btnNextUnas.Visible And btnNextUnas.CommandArgument <> "" Then DD.onChangeAction += String.Format("theForm.{1}.disabled = theForm.{0}.disabled; ", btnNext.ClientID, btnNextUnas.ClientID)
        End If
        ' D0255 ==
        DD.ValuePrecision = 4

        ' D1010 ==
        DD.ImagePath = ImagePath
        DD.lblInfodocTitleGoal = ResString("lblShowDescriptionFor")
        DD.lblInfodocTitleNode = ResString("lblShowDescriptionFor")
        DD.lblInfodocTitleWRT = GetWRTCaption(tParentNode Is Nothing OrElse tParentNode.RiskNodeType = RiskNodeType.ntCategory)   ' D3031 + D3604 + D3610

        DD.ShowFramedInfodocs = CheckVar("framed", App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame)

        'If tAlt IsNot Nothing Then DD.Header = GetWRTNodeName(tAlt) ' D2398
        'If tAlt IsNot Nothing AndAlso Not IsRiskWithControls Then DD.Header = GetWRTNodeName(tAlt) ' D2398 + D2654
        'DD.Header = ParseStringTemplates(ResString(IIf(isLikelihood, "lblEvaluationDirectInput", IIf(tParentNode.Level > 0, "lblEvaluationDirectImpact", "lblEvaluationDirectImpactNoLevels"))), tParams)  ' D1115 + D1928
        If tParentNode IsNot Nothing Then
            DD.CaptionName = tParentNode.NodeName
            DD.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), tParentNode.NodeID.ToString, tParentNode.InfoDoc, True, True, -1)    ' D1669 + D2419
            DD.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(tParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tParentNode.NodeID, GetRandomString(10, True, False))) ' D2419
            DD.CaptionInfodocEditURL = PopupRichEditor(If(tParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, tParentNode.NodeID, tParentNode.NodeGuidID)) ' D2419
        End If

        ' D4346 ===
        If IsRiskWithControls AndAlso ShowInfoDocs Then
            If tData.Assignment IsNot Nothing AndAlso tData.Control IsNot Nothing Then
                DD.CaptionInfodoc = GetControlInfodoc(App.ActiveProject, tData.Control, False)
                DD.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid={1}&r={2}", CInt(reObjectType.Control), tData.Control.ID, GetRandomString(10, True, False)))
                DD.CaptionInfodocEditURL = PopupRichEditor(reObjectType.Control, String.Format("field=infodoc&guid={0}&callback=node", tData.Control.ID))
            End If
            ' D4347 ===
            If tData.Assignment IsNot Nothing AndAlso Not tData.Assignment.EventID.Equals(Guid.Empty) Then
                Dim tNode As clsNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tData.Assignment.EventID)
                If tNode IsNot Nothing Then
                    DD.WRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), tNode.NodeID.ToString, tNode.InfoDoc, True, True, -1)
                    DD.WRTInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tNode.NodeID, GetRandomString(10, True, False)))  ' D6567
                    DD.WRTInfodocEditURL = PopupRichEditor(If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=wrt", _PARAM_ID, tNode.NodeID, tNode.NodeGuidID))
                    DD.lblInfodocTitleWRT = tNode.NodeName
                End If
            End If
            ' D4347 ==
        End If
        ' D4346 ==

        If tAlt IsNot Nothing AndAlso tAlt IsNot tParentNode Then   ' D6713
            DD.AlternativeName = tAlt.NodeName
            ' D2503 ==
            If ShowInfoDocs AndAlso (tParentNode Is Nothing OrElse Not tAlt.NodeGuidID.Equals(tParentNode.NodeGuidID)) Then ' D3384
                DD.AlternativeInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), tAlt.NodeID.ToString, tAlt.InfoDoc, True, True, -1) ' D1669 + D2419
                Dim sWRTInfodoc As String = ""
                If tParentNode IsNot Nothing Then
                    sWRTInfodoc = App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, tParentNode.NodeGuidID)
                    DD.WRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tAlt.NodeID.ToString, sWRTInfodoc, False, True, tParentNode.NodeID)  ' D1669
                    DD.WRTInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&r={4}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tAlt.NodeID, tParentNode.NodeID, GetRandomString(10, True, False)))
                End If
                ' D2503 ==
                DD.AlternativeInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, GetRandomString(10, True, False)))   ' D2419 + D4347
                If DD.ShowFramedInfodocs Then
                    DD.AlternativeInfodocCollapsed = GetCookie("alt_status", If(isHTMLEmpty(DD.AlternativeInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0" ' D1064 + D1069 + D2949
                    DD.CaptionInfodocCollapsed = GetCookie("node_status", If(isHTMLEmpty(DD.CaptionInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"    ' D1064 + D1069 + D2949
                    DD.WRTInfodocCollapsed = GetCookie("wrt_status", If(isHTMLEmpty(DD.WRTInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0" ' D1064 + D1069 + D2949
                    ' D1864 ===
                    DD.SaveSizeMessage = ResString("msgOnSaveInfodocSize")
                    DD.AllowSaveSize = CanUserEditActiveProject
                    DD.SizesList = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
                    ' D1864 ==
                End If
                DD.AlternativeInfodocEditURL = PopupRichEditor(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID))   ' D2419
                If tParentNode IsNot Nothing Then DD.WRTInfodocEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=wrt", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, tParentNode.NodeID, tParentNode.NodeGuidID)) ' D4346
            End If
        End If
        DD.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject ' D1063 + D2503 + D4116
        DD.ImageEditInfodoc = "edit_tiny.gif"
        DD.ImageViewInfodoc = "view_sm.png"     ' D1359
        ' D1010 ==

        'If App.ActiveProject.PipeParameters.ShowInfoDocs Then DD.Infodoc = Infodoc_Unpack(App.ProjectID, reObjectType.Node, tNode.NodeID.ToString, tNode.InfoDoc)
        Controls.Add(DD)
    End Sub
    ' D0175 ==

    ' D0176 ===
    Private Sub SaveDirectData(Action As clsAction)
        Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("DDValue", "")) 'Anti-XSS
        Dim Value As Double     ' D1858
        Dim Judgment As clsDirectMeasureData = CType(CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsDirectMeasureData) 'C0464
        Dim fUpdate As Boolean = False
        If sValue = "" Then
            Judgment.IsUndefined = True
            fUpdate = True
            JudgLogs = JudgUndefined    ' D7262
        Else
            If String2Double(sValue, Value) Then    ' D1858
                Judgment.ObjectValue = Value
                Judgment.IsUndefined = False
                fUpdate = True
                JudgLogs = Double2String(Value, 2)  ' D7262
            End If
        End If
        Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("DDComment", Judgment.Comment))   'Anti-XSS
        If sComment <> Judgment.Comment Then
            Judgment.Comment = sComment
            fUpdate = True
            JudgLogs += JudgComment    ' D7262
        End If
        If fUpdate Then
            ' D2503 ===
            If IsRiskWithControls Then
                App.ActiveProject.ProjectManager.PipeBuilder.SaveControlJudgment(Action)
            Else
                'App.ActiveProject.HierarchyObjectives.GetNodeByID(Judgment.ParentNodeID).Judgments.AddMeasureData(Judgment)
                CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Node.Judgments.AddMeasureData(Judgment) ' D4401
                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(App.ActiveProject.HierarchyObjectives.GetNodeByID(Judgment.ParentNodeID), Judgment)
            End If
            ' D2503 ==
        End If
    End Sub
    ' D0176 ==

    Private Function getScaleWording() As String
        Return If(PM.IsRiskProject, Capitalize(ParseString(If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, "%%Impact%%", "%%Likelihood%%"))), ResString("lblPriority"))
    End Function

    ' D0268 ===
    Private Sub CreateStepFunction(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        ' D1699 ===

        Dim tParams As New Dictionary(Of String, String)
        Dim SF As ctrlUtilityCurve = CType(LoadControl("~/ctrlUtilityCurve.ascx"), ctrlUtilityCurve)
        SF.sRootPath = _URL_ROOT
        SF.ID = "SF"
        SF.Data = Action.ActionData
        SF.YAxisCaption = getScaleWording()

        Dim tData As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData)
        Dim tStep As clsStepMeasureData = CType(tData.Judgment, clsStepMeasureData)
        Dim sXSteps As String = ""
        Dim sYSteps As String = ""
        tStep.StepFunction.SortByInterval()
        For Each tPeriod As clsStepInterval In tStep.StepFunction.Intervals
            sXSteps += If(sXSteps = "", "", "+") + JS_SafeNumber(tPeriod.Low)
            If tPeriod.Value > 1 Then tPeriod.Value = 1 'D1704
            sYSteps += If(sYSteps = "", "", "+") + JS_SafeNumber(tPeriod.Value)
        Next

        ' D2665 ===
        If CanUserEditActiveProject AndAlso Not IsReadOnly AndAlso tStep.StepFunction.Intervals.Count < 2 AndAlso IsPM AndAlso GetCookie(COOKIE_CHECK_SF, "") <> "1" Then   ' D4116
            ' with checkbox
            ClientScript.RegisterClientScriptBlock(GetType(String), "SFWarning", String.Format("function showSFWarning() {{ dxDialog('<p>{0}<p><input type=checkbox id=""cbSFCheck"" value=1><label for=""cbSFCheck"">{3}</label>', 'var sf= $get(""SF""); if ((sf)) sf.focus(); var b = $get(""cbSFCheck""); if ((b) && (b.checked)) document.cookie=""{2}=1;"";', null, '{1}'); }}", JS_SafeString(ResString("msgStepIntervalsShouldBeSpecified")), JS_SafeString(ResString("lblWarning")), COOKIE_CHECK_SF, JS_SafeString(ResString("optDontShowAgain"))), True)
            '' no checkbox
            'ClientScript.RegisterClientScriptBlock(GetType(String), "SFWarning", String.Format("function showSFWarning() {{ dxDialog('{0}', 0, 'document.cookie=""{2}=1;"";', null, '{1}', 480, 220); }}", JS_SafeString(ResString("msgStepIntervalsShouldBeSpecified")), JS_SafeString(ResString("lblWarning")), COOKIE_CHECK_SF), True)    ' D2712
            SF.CustomCodeOnInit = "setTimeout('showSFWarning();', 1500);"
        End If
        ' D2665 ==

        Dim sPrty As String = CStr(IIf(App.isRiskEnabled, IIf(IsRiskWithControls, "lblEffectiveness", IIf(App.ActiveProject.isImpact, "lblContributionImpact", "lblContributionLikelihood")), "lblContribution"))    ' D2560 + D2696

        Dim tParentNode As clsNode = Nothing
        Dim tAlt As clsNode = Nothing
        If IsRiskWithControls Then
            tParentNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tData.Assignment.ObjectiveID)
            SF.CaptionName = GetControlName(tData.Control) ' D4187
        Else
            tParentNode = CType(tData.Node.Hierarchy.GetNodeByID(tStep.ParentNodeID), clsNode)
            If tParentNode.IsTerminalNode Then
                tAlt = tData.Node.Hierarchy.ProjectManager.AltsHierarchy(tData.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tStep.NodeID)
            Else
                tAlt = CType(tData.Node.Hierarchy.GetNodeByID(tStep.NodeID), clsNode)
            End If
        End If
        'If tAlt IsNot Nothing AndAlso tAlt.isUncontributedAlternative Then tParentNode = Nothing ' D4213
        If tAlt IsNot Nothing AndAlso App.ActiveProject.IsRisk AndAlso tAlt.IsAlternative AndAlso App.ActiveProject.HierarchyObjectives.GetUncontributedAlternatives().Contains(tAlt) Then tParentNode = Nothing ' D4213 + D4589

        SF.Comment = tStep.Comment
        SF.lblCommentTitle = ResString("lblEvaluationPWComment")
        SF.isCommentCollapsed = GetCookie("comment_status", If(SF.Comment = "" OrElse App.isMobileBrowser, "0", "1")) = "0"    ' D2949
        SF.ShowComment = ShowComments   ' D1938
        SF.ChangeFlagID = isChanged.ClientID
        ' D6712 ===
        If Not isAllowMissingJudgments() And Not btnNext.Enabled And btnNext.CommandArgument <> "" Then   ' D0598
            SF.onChangeAction = String.Format("theForm.{0}.disabled=HasUndefined(); ", btnNext.ClientID)
            If btnNextUnas.Visible And btnNextUnas.CommandArgument <> "" Then SF.onChangeAction += String.Format("theForm.{1}.disabled = theForm.{0}.disabled; ", btnNext.ClientID, btnNextUnas.ClientID)
        End If
        ' D6712 ==

        If tParentNode IsNot Nothing Then tParams.Add(_TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, True))
        If tAlt IsNot Nothing Then tParams.Add(_TEMPL_NODENAME, JS_SafeHTML(tAlt.NodeName))

        sTask = GetPipeStepTask(Action, IntensityScale) ' D2316

        SF.ImagePath = ImagePath
        SF.lblInfodocTitleGoal = ResString("lblShowDescriptionFor")
        SF.lblInfodocTitleNode = ResString("lblShowDescriptionFor")
        SF.lblInfodocTitleWRT = GetWRTCaption(tParentNode Is Nothing OrElse tParentNode.RiskNodeType = RiskNodeType.ntCategory)   ' D3031 + D3604 + D3610

        SF.ShowFramedInfodocs = CheckVar("framed", App.ActiveProject.PipeParameters.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame)

        ' D4933 ===
        SF.lblInfodocTitleScale = ResString("lblScaleInfodoc")
        If ShowInfoDocs Then
            Dim scale As clsStepFunction = tStep.StepFunction
            SF.ScaleInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.MeasureScale, scale.GuidID.ToString, scale.Comment, True, True, -1)
            SF.ScaleInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid={1}&r={2}", CInt(reObjectType.MeasureScale), scale.GuidID, GetRandomString(10, True, False)))
            SF.ScaleInfodocEditURL = PopupRichEditor(reObjectType.MeasureScale, String.Format("field=infodoc&guid={0}&callback=scale", scale.GuidID))
        End If
        ' D4933 ==

        If tParentNode IsNot Nothing Then
            SF.ParentNodeID = tParentNode.NodeID.ToString    ' D2505 + D3068
            SF.CaptionName = tParentNode.NodeName
            If ShowInfoDocs Then    ' D4015
                SF.CaptionInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.Node, tParentNode.NodeID.ToString, tParentNode.InfoDoc, True, True, -1)
                SF.CaptionInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(If(tParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tParentNode.NodeID, tParentNode.NodeGuidID, GetRandomString(10, True, False)))    ' D2418
                SF.CaptionInfodocEditURL = PopupRichEditor(If(tParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=node", _PARAM_ID, tParentNode.NodeID, tParentNode.NodeGuidID)) ' D2418
            End If
        End If
        ' D4346 ==
        If IsRiskWithControls AndAlso ShowInfoDocs Then
            If tData.Assignment IsNot Nothing AndAlso tData.Control IsNot Nothing Then
                SF.AlternativeName = tData.Control.Name
                SF.AlternativeInfodoc = GetControlInfodoc(App.ActiveProject, tData.Control, False)
                SF.AlternativeInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid={1}&r={2}", CInt(reObjectType.Control), tData.Control.ID, GetRandomString(10, True, False)))
                SF.AlternativeInfodocEditURL = PopupRichEditor(reObjectType.Control, String.Format("field=infodoc&guid={0}&callback=node", tData.Control.ID))
            End If
            ' D4347 ===
            If tData.Assignment IsNot Nothing AndAlso Not tData.Assignment.EventID.Equals(Guid.Empty) Then
                Dim tNode As clsNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tData.Assignment.EventID)
                If tNode IsNot Nothing Then
                    SF.WRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), tNode.NodeID.ToString, tNode.InfoDoc, True, True, -1)
                    SF.WRTInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&r={3}", CInt(If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tNode.NodeID, GetRandomString(10, True, False)))
                    SF.WRTInfodocEditURL = PopupRichEditor(If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=wrt", _PARAM_ID, tNode.NodeID, tNode.NodeGuidID))
                    SF.lblInfodocTitleWRT = tNode.NodeName
                End If
            End If
            ' D4347 ==
        End If
        ' D4346 ==
        If tAlt IsNot Nothing AndAlso (tParentNode Is Nothing OrElse Not tAlt.NodeGuidID.Equals(tParentNode.NodeGuidID)) Then   ' D3384
            SF.AlternativeName = tAlt.NodeName
            If ShowInfoDocs Then
                SF.AlternativeInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), tAlt.NodeID.ToString, tAlt.InfoDoc, True, True, -1) ' D1669 + D2419
                SF.AlternativeInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}&r={4}", CInt(IIf(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node)), _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, GetRandomString(10, True, False))) ' D2418
                If tParentNode IsNot Nothing Then   ' D4213
                    Dim sWRTInfodoc As String = App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, tParentNode.NodeGuidID)
                    SF.WRTInfodoc = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tAlt.NodeID.ToString, sWRTInfodoc, False, True, tParentNode.NodeID)   ' D1669
                    SF.WRTInfodocURL = PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&pid={3}&guid={4}&pguid={5}&r={6}", CInt(reObjectType.AltWRTNode), _PARAM_ID, tAlt.NodeID, tParentNode.NodeID, tAlt.NodeGuidID, tParentNode.NodeGuidID, GetRandomString(10, True, False)))    ' D2418
                End If
                If SF.ShowFramedInfodocs Then
                    SF.AlternativeInfodocCollapsed = GetCookie("alt_status", If(isHTMLEmpty(SF.AlternativeInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"    ' D2949
                    SF.CaptionInfodocCollapsed = GetCookie("node_status", If(isHTMLEmpty(SF.CaptionInfodoc) OrElse App.isMobileBrowser, "0", "1")) = "0"   ' D2949
                    SF.WRTInfodocCollapsed = GetCookie("wrt_status", If(isHTMLEmpty(SF.WRTInfodoc), Bool2Num(App.isMobileBrowser).ToString, "1")) = "0"    ' D2949
                    ' D1864 ===
                    SF.SaveSizeMessage = ResString("msgOnSaveInfodocSize")
                    SF.AllowSaveSize = CanUserEditActiveProject
                    SF.SizesList = ctrlFramedInfodoc.String2List(App.ActiveProject.PipeParameters.InfoDocSize)
                    ' D1864 ==
                End If
                SF.AlternativeInfodocEditURL = PopupRichEditor(If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), String.Format("field=infodoc&{0}={1}&guid={2}&callback=alt", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID))   ' D2418
                If tParentNode IsNot Nothing Then SF.WRTInfodocEditURL = PopupRichEditor(reObjectType.AltWRTNode, String.Format("field=infodoc&{0}={1}&guid={2}&pid={3}&pguid={4}&callback=wrt", _PARAM_ID, tAlt.NodeID, tAlt.NodeGuidID, tParentNode.NodeID, tParentNode.NodeGuidID)) ' D4213
            End If
        End If
        SF.CanEditInfodocs = ShowInfoDocs AndAlso CanUserEditActiveProject   ' D2505
        SF.ImageEditInfodoc = "edit_tiny.gif"
        SF.ImageViewInfodoc = "view_sm.png"

        Controls.Add(SF)
    End Sub

    Private Sub SaveStepFunction(Action As clsAction)
        ' D1699 ===
        Dim Judgment As clsStepMeasureData = CType(CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsStepMeasureData)
        Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("UCComment", Judgment.Comment))   'Anti-XSS
        Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("UCValue", Judgment.SingleValue.ToString))  'Anti-XSS
        If sComment <> Judgment.Comment OrElse sValue <> Judgment.SingleValue.ToString OrElse (sValue <> "" AndAlso Judgment.IsUndefined) Then
            Judgment.Comment = sComment
            Dim tVal As Double
            If sValue <> "" AndAlso String2Double(sValue, tVal) AndAlso tVal > (UNDEFINED_SINGLE_VALUE / 2) Then    ' D3856 + D3861
                Judgment.ObjectValue = tVal
                Judgment.IsUndefined = False    ' D3861
                JudgLogs = Double2String(tVal, 2)       ' D7262
            Else
                Judgment.IsUndefined = True ' D3856
                JudgLogs = JudgUndefined    ' D7262
            End If
            ' D2510 ===
            If IsRiskWithControls Then
                App.ActiveProject.ProjectManager.PipeBuilder.SaveControlJudgment(Action)
            Else
                CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Node.Judgments.AddMeasureData(Judgment) ' D4401
                'App.ActiveProject.HierarchyObjectives.GetNodeByID(Judgment.ParentNodeID).Judgments.AddMeasureData(Judgment)
                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(App.ActiveProject.HierarchyObjectives.GetNodeByID(Judgment.ParentNodeID), Judgment)
            End If
            ' D2510 ==
        End If

    End Sub
    ' D0268 ==

    ' D0153 ===
    Private Sub CreateSensitivityAnalysis(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)    ' D0182
        Dim tParams As New Dictionary(Of String, String)

        ' D0182 ===
        Dim SA As ctrlSensitivityAnalysis = CType(LoadControl("~/ctrlSensitivityAnalysis.ascx"), ctrlSensitivityAnalysis)
        SA.Data = CType(Action.ActionData, clsSensitivityAnalysisActionData) 'C0464 + D2987
        SA.Opt_isMobile = App.isMobileBrowser OrElse Request.IsLocal '   isMobileBrowser     ' D2988

        SA.ProjectManager = App.ActiveProject.ProjectManager
        'SA.pnlLoadingID = pnlLoadingNextStep.ClientID

        SA.msgNoEvaluationData = ResString("msgNoEvalDataShowSA")
        SA.msgNoGroupData = ResString("msgNoGroupDataShowSA")
        SA.lblSelectNode = PrepareTask(ResString("lblSASelectNode"))
        SA.lblRefreshCaption = ResString("btnReset")
        SA.lblKeepSortedAlts = ResString("lblSAKeppSorted")     ' D3477
        SA.lblShowLines = ResString("lblSAShowLines") ' D3477
        SA.lblLineUp = ResString("lblSALineUp")     ' D3719
        SA.lblShowLegend = ResString("lblSAShowLegend")    ' D3481

        ' -D3778
        '' D0374 ===
        'SA.Opt_ShowYouAreSeeing = True
        'SA.msgSeeingCombined = ResString("lblSASeeingCombined")
        'SA.msgSeeingIndividual = ResString("lblSASeeingIndividual")
        'SA.msgSeeingUser = ResString("lblSASeeingForUser")
        '' D0374 ==

        ' D2114 ===
        Dim opt As Array = [Enum].GetValues(GetType(AlternativeNormalizationOptions))
        Dim tpl As String = ResString("lblProps_Template")
        SA.NormalizationsList = New Dictionary(Of AlternativeNormalizationOptions, String)
        For Each St As AlternativeNormalizationOptions In opt
            If St <> AlternativeNormalizationOptions.anoMultipleOfMin Then
                Dim Name As String = St.ToString
                Dim idx As Integer = CInt(St)
                SA.NormalizationsList.Add(St, ResString(String.Format(tpl, Name)))
            End If
        Next
        SA.lblNormalization = ResString("lblSANormalization")
        ' D2114 ==

        SA.CurrentUserID = App.ActiveProject.ProjectManager.UserID
        If App.Options.BackDoor = _BACKDOOR_PLACESRATED Then
            SA.Opt_ShowMaxAltsCount = 10 ' D0360
            'SA.CalculateForCombined = False ' D0365 -D0374
            SA.SAUserID = App.ActiveProject.ProjectManager.UserID ' D0374
        Else
            'SA.CalculateForCombined = App.ActiveProject.PipeParameters.CalculateSAForCombined   ' D0365 -D0374
            SA.SAUserID = If(App.ActiveProject.PipeParameters.CalculateSAForCombined, COMBINED_USER_ID, App.ActiveProject.ProjectManager.UserID) ' D0374
        End If

        SA.opt_ShowNormalization = Not App.isRiskEnabled    ' D7234
        Select Case SA.Data.SAType
            Case SAType.satDynamic
                sTask = ResString("lblEvaluationDynamicSA")     ' D1471
            Case SAType.satGradient
                sTask = ResString("lblEvaluationGradientSA")    ' D1471
            Case SAType.satPerformance
                sTask = ResString("lblEvaluationPerformanceSA") ' D1471
        End Select
        sTask = PrepareTask(ParseStringTemplates(sTask, tParams))  ' D0075 + D0120 + D1503
        'SA.msgHint = PrepareTask(GetPipeStepTask(Action, IntensityScale))  ' D1541 + D2316 -D3778
        ' D0182 ==

        ' D3778 ===
        Dim sSeeing As String = ""
        Select Case SA.SAUserID
            Case COMBINED_USER_ID
                sSeeing = ResString("lblSASeeingCombined")
            Case SA.CurrentUserID
                sSeeing = ResString("lblSASeeingIndividual")
            Case Else
                If SA.SAUserID <> Integer.MinValue AndAlso SA.ProjectManager.User IsNot Nothing Then
                    Dim sUserEmail As String = ""
                    Dim tUser As clsUser = SA.ProjectManager.GetUserByID(SA.SAUserID)
                    If Not tUser Is Nothing Then sUserEmail = tUser.UserEMail
                    sSeeing = String.Format(ResString("lblSASeeingForUser"), sUserEmail)
                End If
        End Select
        If sSeeing <> "" Then sTask += " " + sSeeing
        ' D3778 ==

        Controls.Add(SA)
    End Sub
    ' D0153 ==

    'L0043 ===
    Private Sub CreateSurvey(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)
        Dim Data As clsSpyronSurveyAction = CType(Action.ActionData, clsSpyronSurveyAction)
        ' App.ProjectID 

        'Dim tUserList As List(Of clsUser) = MiscFuncs.GetUsersList(App.SpyronProjectsConnectionDefinition.ConnectionString, clsProject.StorageType, App.ActiveProject.ProviderType, App.ProjectID)
        Dim tUserList As List(Of clsUser) = MiscFuncs.GetUsersList(App.CanvasProjectsConnectionDefinition.ConnectionString, clsProject.StorageType, App.ActiveProject.ProviderType, App.ProjectID)  ' D6423

        Dim AUsersList As New Dictionary(Of String, clsComparionUser)
        For Each User As clsUser In tUserList
            AUsersList.Add(User.UserEMail, New clsComparionUser() With {.ID = User.UserID, .UserName = User.UserName})
        Next
        App.SurveysManager.ActiveUserEmail = App.ActiveUser.UserEmail
        Dim tSurvey As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, CType(Data.SurveyType, SurveyType), AUsersList)
        If Not tSurvey Is Nothing Then
            SurveySpyron.Visible = True
            tdExtraContent.Visible = True   ' D0938
            trHead.Visible = False      ' D1033
            divSpyron.Visible = True    ' D1033
            'L0492 ===
            Dim tUser As clsUser = App.ActiveProject.ProjectManager.User
            Dim SurveyLoadEmail As String = tUser.UserEMail
            If IsReadOnly Then SurveyLoadEmail = "" 'if readonly - load all participants answers
            'L0492 ==

            ScriptManager.RegisterStartupScript(Me, GetType(String), "InitUI", "_SpyronResize();", True)    ' D3677

            SurveySpyron.RespondentID = tUser.UserID
            SurveySpyron.RespondentEMail = tUser.UserEMail 'L0492
            SurveySpyron.RespondentName = tUser.UserName 'L0454 'L0492
            SurveySpyron.ProjectManager = App.ActiveProject.ProjectManager  ' D0320
            SurveySpyron.SurveyInfo = tSurvey
            SurveySpyron.ReadOnlyAnswers = IsReadOnly
            ' D0428 ===
            SurveySpyron.SurveyPage = CType(SurveySpyron.SurveyInfo.Survey(SurveyLoadEmail).Pages(Data.StepNumber - 1), clsSurveyPage) 'L0442 'L0492
            If SurveySpyron.SurveyPage Is Nothing AndAlso SurveySpyron.SurveyInfo.Survey(SurveyLoadEmail).Pages.Count > 0 Then SurveySpyron.SurveyPage = CType(SurveySpyron.SurveyInfo.Survey(SurveyLoadEmail).Pages(0), clsSurveyPage) 'L0442 'L0492
            'Session("SurveyCurrentPage") = SurveySpyron.SurveyPage
            ' D0428 ==
            ' D1127 ===
            tdMainCell.Height = "1%"    ' D7365
        Else
            Dim lbl As New Label()
            lbl.ID = "lblNoSurvey"  ' D1239
            lbl.Text = String.Format("<h4 class='error'>{0}</h4>", ResString("errSurveyNotFound"))
            Controls.Add(lbl)
            ' D1127 ==
        End If
    End Sub
    'L0043 ==
    ' D0250 ==

    ' D4715 ===
    Private Sub CreateEmbeddedContent(Action As clsAction, Controls As ControlCollection, ByRef sTask As String)

        Dim info As New Literal
        info.ID = "lblEmbed"
        If Action IsNot Nothing Then
            Select Case Action.EmbeddedContentType
                Case EmbeddedContentType.RiskResults, EmbeddedContentType.HeatMap   ' D6664
                    If App.isRiskEnabled AndAlso App.ActiveProject.isImpact Then
                        info.Text = String.Format("<div class='text whole' style='text-align:center'><object type='text/html' id='frmEmbed' class='whole' data='{0}'></object></div><script type='text/javascript'> initFrameLoader(document.getElementById(""frmEmbed"")); </script>", PageURL(If(Action.EmbeddedContentType = EmbeddedContentType.HeatMap, _PGID_RISK_PLOT_OVERALL, _OPT_RISK_RESULTS_PGID), "temptheme=sl&widget=yes"))  ' D6664 + D6669 + D6772
                        ClientScript.RegisterStartupScript(GetType(String), "InitEmbed", "showLoadingPanel();", True)
                    End If
                    ' D6671 ===
                Case EmbeddedContentType.AlternativesRank
                    sTask = GetPipeStepTask(Action, Nothing)
                    Dim sAlts As String = ""
                    Dim MaxLen As Integer = 15  ' D6674
                    If Action.ActionData IsNot Nothing Then
                        Dim AltsListWithRanks As List(Of clsNode) = CType(Action.ActionData, List(Of clsNode)).Where(Function(x) x.UserRank > 0).OrderBy(Function(x) x.UserRank).ToList
                        Dim AltsListWithNoRanks As List(Of clsNode) = CType(Action.ActionData, List(Of clsNode)).Where(Function(x) x.UserRank <= 0).ToList
                        Dim AltsList As New List(Of clsNode)

                        Dim fSaveChanges As Boolean = False
                        Dim tAltsWithRanksCount As Integer = 0

                        For i As Integer = 0 To AltsListWithRanks.Count - 1
                            If AltsListWithRanks(i).UserRank <> i + 1 Then
                                fSaveChanges = True
                                AltsListWithRanks(i).UserRank = i + 1
                                With App.ActiveProject.ProjectManager
                                    .Attributes.SetAttributeValue(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID, .UserID, AttributeValueTypes.avtLong, AltsListWithRanks(i).UserRank, AltsListWithRanks(i).NodeGuidID, Guid.Empty)
                                End With
                            End If
                            tAltsWithRanksCount = i + 1
                        Next


                        For i As Integer = 0 To AltsListWithNoRanks.Count - 1
                            If AltsListWithNoRanks(i).UserRank <> tAltsWithRanksCount + i + 1 Then
                                fSaveChanges = True
                                AltsListWithNoRanks(i).UserRank = tAltsWithRanksCount + i + 1
                                With App.ActiveProject.ProjectManager
                                    .Attributes.SetAttributeValue(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID, .UserID, AttributeValueTypes.avtLong, AltsListWithNoRanks(i).UserRank, AltsListWithNoRanks(i).NodeGuidID, Guid.Empty)
                                End With
                            End If
                        Next

                        AltsList.AddRange(AltsListWithRanks)
                        AltsList.AddRange(AltsListWithNoRanks)

                        If fSaveChanges Then 
                            With App.ActiveProject.ProjectManager
                            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, .UserID)
                            End With
                        End If

                        For Each tAlt As clsNode In AltsList
                            sAlts += String.Format("{0}{{'id':{1}, 'title':'{2}', 'urank':'{3}'}}", If(sAlts = "", "", ", "), tAlt.NodeID, JS_SafeString(tAlt.NodeName), If(tAlt.UserRank > 0, tAlt.UserRank.ToString, ""))
                            If tAlt.NodeName.Length > MaxLen Then MaxLen = tAlt.NodeName.Length  ' D6674
                        Next
                    End If
                    info.Text = String.Format("<script type='text/javascript'> var alt_ranks = [{0}]; </script><div id='divAltsRank' style='margin:26px auto 1ex auto; max-width:{1}'></div><input type='hidden' name='altRanks' value=''>", sAlts, (MaxLen + 18).ToString + "ex;") ' D6674
                    ClientScript.RegisterStartupScript(GetType(String), "initRank", "initAltsRank();", True)
                    ' D6671 ==
            End Select
        End If
        If info.Text <> "" Then Controls.Add(info)
    End Sub
    ' D4715 ==

#End Region

    ' D0043 ===
    Public Sub New()
        MyBase.New(_PGID_EVALUATION)   ' D0192 + D0493
    End Sub
    ' D0043 ==

    ' D0608 ===
    Protected Sub RadTreeViewHierarchy_NodeClick(sender As Object, e As RadTreeNodeEventArgs) Handles RadTreeViewHierarchy.NodeClick, RadTreeViewImpact.NodeClick   ' D4146
        If e.Node IsNot Nothing AndAlso e.Node.Value <> "" Then
            Dim iStep As Integer = -1
            Dim btn As New Button
            btn.CommandArgument = e.Node.Value
            btn.CommandName = _PARAM_STEP   ' D4699
            If e.Node.Attributes("extra") IsNot Nothing Then btn.Attributes.Add("extra", e.Node.Attributes("extra")) ' D4109
            btnStep_Click(btn, Nothing)
        End If
    End Sub
    ' D0608 ==

    '' D2093 ===
    'Private Function SetClusterPhraseForNode(NodeID As Guid, sClusterPhrase As String, IsMulti As Boolean, tAdditionlGuid As Guid) As Boolean   ' D2751 + D4053
    '    Dim fResult As Boolean = False  ' D3912
    '    With App.ActiveProject.ProjectManager
    '        Dim attr As clsAttribute
    '        ' D4133 ===
    '        Dim AttrGUID As Guid
    '        If IsMulti Then
    '            If IsRiskWithControls Then AttrGUID = ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_MULTI_ID Else AttrGUID = ATTRIBUTE_CLUSTER_PHRASE_MULTI_ID
    '        Else
    '            If IsRiskWithControls Then AttrGUID = ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_ID Else AttrGUID = ATTRIBUTE_CLUSTER_PHRASE_ID
    '        End If
    '        attr = .Attributes.GetAttributeByID(AttrGUID)
    '        If attr IsNot Nothing Then
    '            .Attributes.SetAttributeValue(AttrGUID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, sClusterPhrase, NodeID, tAdditionlGuid) ' D4053
    '            fResult = .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
    '        End If
    '        ' D4133 ==
    '    End With
    '    Return fResult  ' D3912
    'End Function
    '' D2093 ==

    '' D2093 ===
    'Private Function SetClusterTitleForNode(NodeID As Guid, sClusterPhrase As String) As Boolean
    '    Dim fResult As Boolean = False
    '    With App.ActiveProject.ProjectManager
    '        Dim attr As clsAttribute = .Attributes.GetAttributeByID(ATTRIBUTE_CLUSTER_PHRASE_LOCAL_RES_ID)
    '        If attr IsNot Nothing Then
    '            Dim tAdditionlGuid As Guid = Guid.Empty
    '            If IsIntensities AndAlso IntensityScale IsNot Nothing Then tAdditionlGuid = IntensityScale.GuidID
    '            .Attributes.SetAttributeValue(ATTRIBUTE_CLUSTER_PHRASE_LOCAL_RES_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, sClusterPhrase, NodeID, tAdditionlGuid)
    '            fResult = .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
    '        End If
    '    End With
    '    Return fResult
    'End Function
    '' D2093 ==

    ' D2096 ===
    Protected Sub RadToolTipHints_Load(sender As Object, e As EventArgs) Handles RadToolTipHints.Load, RadToolTipTitleHints.Load    ' D4329
        Dim sTooltip As String = String.Format("<b>{0}</b>:<ul type=square style='margin:1ex 2em 1ex 1em'>", ResString("lblEditTaskTemplates"))

        Dim Tpls As New Dictionary(Of String, String)
        For Each tKey As KeyValuePair(Of String, String) In TaskTemplates
            Tpls.Add(tKey.Key, tKey.Value)
        Next
        Dim sAllTpl As String = ""
        ' D4352 ===
        Dim Lst As String() = _TEMPL_LIST_EVALS
        'If App.isRiskEnabled Then
        '    Lst = _TEMPL_LIST_EVALS_RISK
        'Dim tList As Dictionary(Of String, String) = GetUserTemplateReplacements(True)
        'If tList IsNot Nothing Then
        '    For i As Integer = 0 To Lst.Length - 1
        '        If tList.Keys.Contains(Lst(i)) Then Lst(i) = tList(Lst(i))
        '    Next
        'End If
        'End If
        For Each sTpl As String In Lst
            ' D4352 ==
            sAllTpl += sTpl + vbCrLf
        Next
        Dim ParsedTpl As String() = PrepareTask(sAllTpl).Split(CType(vbCrLf, Char()))
        ' D4352 ===
        Dim tList As Dictionary(Of String, String) = GetUserTemplateReplacements(True)
        For i As Integer = 0 To ParsedTpl.Length - 1
            If ParsedTpl(i) <> "" AndAlso i < Lst.Length Then
                Dim sName As String = Lst(i)
                If tList.Keys.Contains(sName) Then sName = tList(sName)
                Tpls.Add(sName, ParsedTpl(i))
            End If
            ' D4352 ==
        Next

        If Tpls.Count = 0 Then
            sTooltip = "<div style='padding:3em'><i>no items</i></div>"
        Else
            Dim idx As Integer = 0
            For Each sKey As String In Tpls.Keys.OrderBy(Of String)(Function(x) x)
                sTooltip += String.Format("<li{4}><a href='' onclick='return InsertTemplate({3}, ""{2}"")' class='actions'><span id='_tpl_{5}'>{0}</a>: <span id='_val_{5}'>{1}</span>{6}</li>", sKey, If(sKey = _TEMPL_EVAL_OBJECT, Tpls(sKey), SafeFormString(ShortString(HTML2Text(Tpls(sKey)), 50, True))), JS_SafeString(sKey), IIf(sender Is RadToolTipHints, 0, 1), If(sKey = _TEMPL_EVAL_OBJECT, " id='tplEvalObject' style='display:none; margin:2px 0px;'", " style='margin:2px 0px;'"), idx, If(sKey = _TEMPL_EVAL_OBJECT, " <i>[Focused element]</i>", "")) ' D2830 + D4327 + D4329 + D6960 + D6961
                idx += 1  ' D6961
            Next
            sTooltip += String.Format("</ul><input type='hidden' id='_tpl_count' name='_tpl_count' value='{1}'/><div style='margin-top:1em' class='text small'>{0}</div>", ResString("msgClick2Insert"), idx - 1)  ' D6961
        End If
        CType(sender, RadToolTip).Text = String.Format("<div style='padding:1em'>{0}</div>", sTooltip)  ' D4329
    End Sub
    ' D2096 ==

    ' D2651 ===
    Protected Sub RadToolTipClusterSteps_Load(sender As Object, e As EventArgs) Handles RadToolTipClusterSteps.Load, RadToolTipTitleSteps.Load
        Dim isTitle As Boolean = sender Is RadToolTipTitleSteps ' D4329
        ' D4108 ===
        Dim HasQHCluster As Boolean = False
        Dim QHApplySteps As New List(Of Integer)
        ' D4329 ===
        Dim ApplyNodes As List(Of clsNode) = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeStepClusters(CurStep, HasQHCluster, QHApplySteps)  ' D4116
        'If isTitle Then
        '    For i As Integer = 0 To App.ActiveProject.Pipe.Count - 1
        '        Dim tAct As clsAction = CType(App.ActiveProject.Pipe(i), clsAction)
        '        If tAct.ActionType = ActionType.atShowLocalResults Then ApplyNodes.Add(CType(tAct.ActionData, clsShowLocalResultsActionData).ParentNode)
        '    Next
        'Else

        'End If
        Dim fCanShow As Boolean = QHApplySteps.Count > 1 OrElse HasQHCluster
        If isTitle Then CanShowTitleApplyTo = fCanShow Else CanShowApplyTo = fCanShow
        If fCanShow AndAlso CurAction IsNot Nothing Then
            ' D4329 ==
            Dim sTooltip As String = ""
            ' D4116 ===
            Dim isMulti As Boolean = False
            Dim tAGuid As Guid = Guid.Empty
            Select Case CurAction.ActionType
                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atShowLocalResults
                    isMulti = True
            End Select
            Select Case CurAction.ActionType
                Case ActionType.atShowLocalResults, ActionType.atShowGlobalResults
                    ' D4329 ===
                    If isTitle Then
                        If IsIntensities AndAlso IntensityScale IsNot Nothing Then tAGuid = IntensityScale.GuidID
                    Else
                        If CurStepNode IsNot Nothing Then tAGuid = CurStepNode.NodeGuidID
                    End If
            End Select
            Dim tCurClusterPhrase As String = ""
            If isTitle Then
                tCurClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterTitleForResults(CurStepNode.NodeGuidID, tAGuid)
            Else
                tCurClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(CurStepNode, isMulti, tAGuid, IsRiskWithControls)   ' D4133
            End If
            ' D4329 ==
            Dim i As Integer = 1
            For Each tNode As clsNode In ApplyNodes
                Dim fActive As Boolean = (tNode Is CurStepNode) OrElse (CurStep = QHApplySteps(i - 1))
                Dim fChecked As Boolean = fActive
                If Not fChecked AndAlso ClusterPhrase <> "" Then
                    tAGuid = Guid.Empty
                    Select Case CurAction.ActionType
                        Case ActionType.atShowLocalResults, ActionType.atShowGlobalResults
                            ' D4329 ===
                            If Not isTitle Then tAGuid = tNode.NodeGuidID
                    End Select
                    Dim sStepClusterPhrase As String = ""
                    If isTitle Then
                        sStepClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterTitleForResults(tNode.NodeGuidID, tAGuid)
                    Else
                        sStepClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(tNode, isMulti, tAGuid, IsRiskWithControls) ' D4133
                    End If
                    ' D4329 ==
                    If sStepClusterPhrase = ClusterPhrase Then fChecked = True
                End If
                sTooltip += String.Format("<li><nobr><input type='checkbox' id='{5}{1}'{2}{4} value='{3}'><label for='task_guid_{1}'>{0}</label></nobr></li>", SafeFormString(ShortString(tNode.NodeName, 40, True)), i, IIf(fActive, " disabled", ""), tNode.NodeGuidID.ToString, IIf(fChecked, " checked", ""), IIf(isTitle, "title_guid_", "task_guid_"))
                ' D4116 ==
                i += 1
            Next
            sTooltip = String.Format("<div style='padding:3ex'><b>{0}</b><ul type=square style='margin:1ex 2em 1ex 1em;{1}'>{2}</ul><div style='text-align:center; margin-top:1em;'><input type='button' class='button button_small' id='btnApplyTo' value='{3}' onclick='SaveTaskMulti({4}); return false;' style='width:15em'></div></div>", ResString("lblEditTaskChooseSteps"), IIf(ApplyNodes.Count > 10, " height:10em; overflow-y:scroll;", ""), sTooltip, ResString("btnEditTaskSaveMulti"), IIf(isTitle, 1, 0)) ' D4329
            CType(sender, RadToolTip).Text = sTooltip   ' D4329
        End If
        ' D4108 ==
    End Sub
    ' D2651 ==

    ' D7177 ===
    Private Sub EvaluationPage2_Error(sender As Object, e As EventArgs) Handles Me.[Error]
        Dim ex As Exception = Server.GetLastError
        If ex IsNot Nothing AndAlso ex.Message.Contains("is not a valid value for Int32") AndAlso ex.StackTrace.Contains("Telerik.Web.UI.RadTreeView.") Then
            ' ignore that RTE for Telerik
        Else
            ErrorFeedback(sender, e)
        End If
    End Sub
    ' D7177 ==

End Class

#Region "Intermediate Results"

Partial Public Class EvaluationPage2 'A0872
    Inherits clsComparionCorePage

    ' D1702 ===
    Private Const _SESS_JUDGMENTS As String = "Judgments{0}"
    Private Const _SESS_DATETIME As String = "JudgmentsDateTime"
    Private Const _SESS_SAVED As String = "JudgmentsSaved"

    'Private _JudgmentsSaved As Boolean = False
    Public Property JudgmentsSaved As Boolean
        Get
            Return SessVar(_SESS_SAVED) = "1"
        End Get
        Set(value As Boolean)
            SessVar(_SESS_SAVED) = If(value, "1", "0")
        End Set
    End Property
    ' D1702 ==


    Private Function ParamVar(sParam As String, sVar As String, DefValue As String) As String
        Dim retVal As String = DefValue
        Dim params As String() = sParam.Split(CType("&", Char()))
        Dim varFound As Boolean = False
        If params IsNot Nothing Then
            Dim i As Integer = 0
            While (Not varFound) AndAlso (i < params.Count)
                Dim param As String = params(i)
                Dim item As String() = param.Split(CType("=", Char()))
                If item IsNot Nothing AndAlso item.Count = 2 AndAlso item(0).Trim.ToLower = sVar.Trim.ToLower Then
                    retVal = item(1).Trim
                    varFound = True
                End If
                i += 1
            End While
        End If
        Return retVal
    End Function

    Public ReadOnly Property _SESS_NORM_MODE As String
        Get
            Return String.Format("ShowResults_NormMode_{0}_{1}", App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy)
        End Get
    End Property

    Public Const ACTION_NORMALIZE_MODE As String = "normalize_mode"

    Private _NormalizationMode As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoPriority
    Public Property NormalizationMode() As AlternativeNormalizationOptions
        Get
            If Model IsNot Nothing AndAlso Not Model.IsForAlternatives Then
                Return AlternativeNormalizationOptions.anoUnnormalized
            End If
            Dim sVal As String = SessVar(_SESS_NORM_MODE)   ' D4850
            Dim tVal As Integer = 0
            If Not String.IsNullOrEmpty(sVal) AndAlso Integer.TryParse(sVal, tVal) Then
                _NormalizationMode = CType(tVal, AlternativeNormalizationOptions)
            Else
                If App.isRiskEnabled AndAlso Not App.ActiveProject.isImpact Then
                    _NormalizationMode = AlternativeNormalizationOptions.anoUnnormalized
                Else
                    _NormalizationMode = AlternativeNormalizationOptions.anoPriority
                End If
            End If
            Return _NormalizationMode
        End Get
        Set(value As AlternativeNormalizationOptions)
            _NormalizationMode = value
            Session(_SESS_NORM_MODE) = _NormalizationMode   ' D2610
        End Set
    End Property

    Protected Function GetNormalizationOptions() As String
        Dim sOptions As String = ""

        Dim opt As Array
        opt = [Enum].GetValues(GetType(AlternativeNormalizationOptions))
        Dim tpl As String = ResString("lblProps_Template")
        Dim NormNames(opt.Length - 1) As String
        For Each St As AlternativeNormalizationOptions In opt   ' D2114
            If St <> AlternativeNormalizationOptions.anoMultipleOfMin Then
                Dim Name As String = St.ToString
                Dim idx As Integer = CInt(St)
                If NormNames.GetUpperBound(0) < idx Then Array.Resize(NormNames, idx + 1)
                If App.isRiskEnabled AndAlso Not App.ActiveProject.isImpact Then Name += "Likelihood"
                NormNames(idx) = ResString(String.Format(tpl, Name))
                sOptions += String.Format("<option value='{0}' {2}>{1}</option>", idx, NormNames(idx), IIf(CInt(NormalizationMode) = idx, " selected='selected'", ""))
            End If
        Next
        Return String.Format("<select class='select' id='cbNormalizeOptions' style='width:140px;' onchange='cbNormalizeModeChange(this.value);'>{0}</select>", sOptions)
    End Function

    Private Function UpdateUserJudgment(tValue As Double, tAdvantage As Integer, AHPUserID As Integer, tActionData As clsShowLocalResultsActionData, ParentID As Integer, Obj1ID As Integer, Obj2ID As Integer, Optional OnlyInvertAdvantage As Boolean = False, Optional tIsUndefined As Boolean = False) As Boolean
        Dim retVal As Boolean = false
        If tAdvantage = 0 Then tAdvantage = 1

        Dim node As clsNode = GetPipeStepParentNode(tActionData, ParentID)
        If node IsNot Nothing Then
            Dim pair As clsPairwiseMeasureData = GetEvalPipePWStepData(ParentID, Obj1ID, Obj2ID)            
            If OnlyInvertAdvantage
                If pair IsNot Nothing Then
                    pair.Advantage = -pair.Advantage
                    pair.IsUndefined = tIsUndefined
                End If
            Else
                If pair IsNot Nothing Then
                    pair.Advantage = tAdvantage
                    pair.Value = tValue
                    pair.IsUndefined = tIsUndefined
                Else
                    node.Judgments.AddMeasureData(New clsPairwiseMeasureData(Obj1ID, Obj2ID, tAdvantage, tValue, ParentID, AHPUserID, tIsUndefined), False)
                End If
            End If

            Dim judgment As clsPairwiseMeasureData = GetJudgmentFromUser(AHPUserID, node, tActionData, ParentID, Obj1ID, Obj2ID)
            If OnlyInvertAdvantage
                If judgment IsNot Nothing Then
                    judgment.Advantage = -judgment.Advantage
                    judgment.IsUndefined = tIsUndefined
                End If
            Else
                If judgment IsNot Nothing Then
                    judgment.Advantage = tAdvantage
                    judgment.Value = tValue
                    judgment.IsUndefined = tIsUndefined
                Else
                    node.Judgments.AddMeasureData(New clsPairwiseMeasureData(Obj1ID, Obj2ID, tAdvantage, tValue, ParentID, AHPUserID, tIsUndefined), False)
                End If
            End If
            retVal = True
        End If
        Return retVal
    End Function

    Private ExpectedValueString As String = "" 'A1070

    Private Function GetPipeStepParentNode(psActionData As Object, ParentID As Integer) As clsNode
        Dim node As clsNode
        If CType(psActionData, clsShowLocalResultsActionData).PWOutcomesNode IsNot Nothing Then
            node = CType(psActionData, clsShowLocalResultsActionData).PWOutcomesNode
        Else
            node = App.ActiveProject.HierarchyObjectives.GetNodeByID(ParentID)
        End If
        Return node
    End Function

    Protected Sub GetIntermediateResultsData(tOperationID As OperationID, params As String)
        'StorePageID = False     ' D1300
        Model = New DataModel
        ExpectedValueString = "" 'A1070
        ClipData = ""

        If App.isAuthorized And App.HasActiveProject Then
            Dim iStep As Integer = CurStep
            If Integer.TryParse(ParamVar(params, _PARAM_STEP, iStep.ToString), iStep) Then
                With App.ActiveProject.ProjectManager

                    If iStep > .Pipe.Count Then iStep = .Pipe.Count ' D2555
                    If iStep < 1 Then iStep = 1 ' D2555

                    ' D1807 ===
                    Dim fIsIntensties As Boolean = False
                    Dim fOriginalHID As Integer = .ActiveHierarchy
                    Dim sScaleID As String = SessVar(SESSION_SCALE_ID) ' D2020
                    If ParamVar(params, "intensities", "false") <> "false" AndAlso Not String.IsNullOrEmpty(sScaleID) Then   ' D1965
                        For Each tmpHierarchy As clsHierarchy In App.ActiveProject.ProjectManager.GetAllHierarchies
                            If tmpHierarchy.HierarchyType = ECHierarchyType.htMeasure AndAlso tmpHierarchy.HierarchyGuidID.ToString = sScaleID AndAlso .ActiveHierarchy <> tmpHierarchy.HierarchyID Then ' D1965 + D2555
                                .ActiveHierarchy = tmpHierarchy.HierarchyID
                                ' D1965 ===
                                If .PipeParameters.CurrentParameterSet IsNot .PipeParameters.MeasureParameterSet Then .PipeParameters.CurrentParameterSet = .PipeParameters.MeasureParameterSet ' D1958
                                '.PipeBuilder.PipeCreated = False
                                .PipeBuilder.CreatePipe(False)
                                ' D1965 ==
                                fIsIntensties = True
                            End If
                        Next
                    End If
                    ' D1807 ==

                    If iStep > 0 AndAlso iStep <= .Pipe.Count Then

                        Dim ps As clsAction = CType(.Pipe(iStep - 1), clsAction)
                        If ps.ActionType = ActionType.atShowLocalResults Then

                            Dim sEmail As String = App.ActiveUser.UserEmail
                            Dim UID As Integer = -1
                            If Integer.TryParse(ParamVar(params, "uid", ""), UID) Then
                                If UID <> App.ActiveUser.UserID Then
                                    Dim tUser As clsApplicationUser = App.DBUserByID(UID)
                                    If tUser IsNot Nothing Then sEmail = tUser.UserEmail
                                End If
                            End If
                            Dim AHPUser As clsUser = .GetUserByEMail(sEmail)
                            If AHPUser IsNot Nothing Then
                                Dim AHPUserID As Integer = AHPUser.UserID

                                '' D4375 ===
                                ''If CombinedUserID <> COMBINED_USER_ID AndAlso Not CombinedJudgmentsLoaded Then ' -D4376
                                'If CombinedUserID <> COMBINED_USER_ID Then
                                '    App.ActiveProject.ProjectManager.StorageManager.Reader.LoadUserJudgments(.GetUserByID(CombinedUserID))
                                '    'CombinedJudgmentsLoaded = True ' -D4376
                                'End If
                                '' D4375 ==

                                If tOperationID = OperationID.oJudgmentUpdate Then
                                    Dim Obj1ID As Integer = -1
                                    Dim Obj2ID As Integer = -1
                                    Dim tValue As Double = 0
                                    Dim Advantage As Integer = 0
                                    Dim ParentID As Integer = -1
                                    If Integer.TryParse(ParamVar(params, "Obj1", ""), Obj1ID) AndAlso Integer.TryParse(ParamVar(params, "Obj2", ""), Obj2ID) AndAlso String2Double(ParamVar(params, "Value", ""), tValue) AndAlso Integer.TryParse(ParamVar(params, "Adv", ""), Advantage) AndAlso Integer.TryParse(ParamVar(params, "PID", ""), ParentID) Then ' D1858
                                        If UpdateUserJudgment(tValue, Advantage, AHPUserID, CType(ps.ActionData, clsShowLocalResultsActionData), ParentID, Obj1ID, Obj2ID, False) Then
                                            .StorageManager.Writer.SaveUserJudgments(AHPUserID)
                                        End If
                                    End If
                                End If

                                If tOperationID = OperationID.oResetJudgments Then
                                    Dim Count As Integer = 0
                                    Dim ParentID As Integer = -1
                                    If Integer.TryParse(ParamVar(params, "Count", ""), Count) AndAlso Integer.TryParse(ParamVar(params, "PID", ""), ParentID) Then
                                        Dim node As clsNode = GetPipeStepParentNode(ps.ActionData, ParentID)
                                        If node IsNot Nothing Then
                                            For i As Integer = 0 To Count - 1
                                                Dim Obj1ID As Integer = -1
                                                Dim Obj2ID As Integer = -1
                                                Dim tValue As Double = 0
                                                Dim Advantage As Integer = 0
                                                If Integer.TryParse(ParamVar(params, String.Format("O{0}1", i), ""), Obj1ID) AndAlso Integer.TryParse(ParamVar(params, String.Format("O{0}2", i), ""), Obj2ID) AndAlso String2Double(ParamVar(params, String.Format("V{0}", i), ""), tValue) AndAlso Integer.TryParse(ParamVar(params, String.Format("A{0}", i), ""), Advantage) Then   ' D1858
                                                    'node.Judgments.AddMeasureData(New clsPairwiseMeasureData(Obj1ID, Obj2ID, CInt(IIf(Advantage = 0, 1, Advantage)), tValue, ParentID, AHPUserID, True))
                                                    If UpdateUserJudgment(tValue, Advantage, AHPUserID, CType(ps.ActionData, clsShowLocalResultsActionData), ParentID, Obj1ID, Obj2ID, , True) Then
                                                        .StorageManager.Writer.SaveUserJudgments(AHPUserID)
                                                    End If
                                                End If
                                            Next
                                            'App.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(AHPUserID)
                                        End If
                                    End If
                                End If

                                If tOperationID = OperationID.oRestoreJudgments Then 'Restore Judgments
                                    Dim ParentID As Integer = -1
                                    If Integer.TryParse(ParamVar(params, "PID", ""), ParentID) Then
                                        RestoreJudgments(ParentID, GetPipeStepParentNode(ps.ActionData, ParentID))
                                    End If
                                End If

                                If tOperationID = OperationID.oSaveJudgments Then 'Save Judgments
                                    Dim ParentID As Integer = -1
                                    If Integer.TryParse(ParamVar(params, "PID", ""), ParentID) Then
                                        SaveJudgments(ParentID, GetPipeStepParentNode(ps.ActionData, ParentID))
                                    End If
                                End If

                                If tOperationID = OperationID.oInvertAllJudgments Then 'Invert All Judgments
                                    Dim ParentID As Integer = -1
                                    If Integer.TryParse(ParamVar(params, "PID", ""), ParentID) Then
                                        Dim node As clsNode = GetPipeStepParentNode(ps.ActionData, ParentID)
                                        If node IsNot Nothing Then
                                            If CType(ps.ActionData, clsShowLocalResultsActionData).PWOutcomesNode IsNot Nothing Then
                                                For Each judgment As clsPairwiseMeasureData In node.PWOutcomesJudgments.JudgmentsFromUser(AHPUserID)
                                                    If Not judgment.IsUndefined Then
                                                        'judgment.Advantage = -judgment.Advantage
                                                        UpdateUserJudgment(UNDEFINED_INTEGER_VALUE, 0, AHPUserID, CType(ps.ActionData, clsShowLocalResultsActionData), ParentID, judgment.FirstNodeID, judgment.SecondNodeID, True)
                                                    End If
                                                Next
                                            Else
                                                For Each judgment As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(AHPUserID)
                                                    If Not judgment.IsUndefined Then
                                                        'judgment.Advantage = -judgment.Advantage
                                                        UpdateUserJudgment(UNDEFINED_INTEGER_VALUE, 0, AHPUserID, CType(ps.ActionData, clsShowLocalResultsActionData), ParentID, judgment.FirstNodeID, judgment.SecondNodeID, True)
                                                    End If
                                                Next
                                            End If
                                            .StorageManager.Writer.SaveUserJudgments(AHPUserID)
                                        End If
                                    End If
                                End If

                                If tOperationID = OperationID.oUpdatePWNL Then ' Update PWNL
                                    Dim ParentID As Integer = -1
                                    Dim AltID As Integer = -1
                                    Dim AltVal As Double = -1
                                    If Integer.TryParse(ParamVar(params, "AltID", ""), AltID) AndAlso Integer.TryParse(ParamVar(params, "PID", ""), ParentID) AndAlso String2Double(ParamVar(params, "AltVal", ""), AltVal) Then
                                        Dim node As clsNode = GetPipeStepParentNode(ps.ActionData, ParentID)
                                        If node IsNot Nothing Then
                                            Dim nodes As List(Of clsNode) = node.GetNodesBelow(UNDEFINED_USER_ID)

                                            If nodes IsNot Nothing Then
                                                For Each nd As clsNode In nodes
                                                    .Attributes.SetAttributeValue(ATTRIBUTE_KNOWN_VALUE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, IIf(nd.NodeID.Equals(AltID), AltVal, Nothing), nd.NodeGuidID, node.NodeGuidID)
                                                Next
                                            End If

                                            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                                        End If
                                    End If
                                End If

                                If ps.ActionType = ActionType.atShowLocalResults Then
                                    Dim psLocal As clsShowLocalResultsActionData = CType(ps.ActionData, clsShowLocalResultsActionData)

                                    Model.IsForAlternatives = psLocal.ParentNode IsNot Nothing AndAlso psLocal.ParentNode.IsTerminalNode
                                    If psLocal.ParentNode IsNot Nothing Then Model.PWMode = .PipeBuilder.GetPairwiseTypeForNode(psLocal.ParentNode) ' D3637

                                    If tOperationID <> OperationID.oSaveJudgments Then 'Not save judgments
                                        'C0810===
                                        Select Case psLocal.ResultsViewMode     ' D4589
                                            Case ResultsView.rvNone
                                                Model.ShowIndividualResults = False
                                                Model.ShowGroupResults = False
                                            Case ResultsView.rvIndividual
                                                Model.ShowIndividualResults = True
                                                Model.ShowGroupResults = False
                                            Case ResultsView.rvGroup
                                                Model.ShowIndividualResults = False
                                                Model.ShowGroupResults = True
                                            Case ResultsView.rvBoth
                                                Model.ShowIndividualResults = True
                                                Model.ShowGroupResults = True
                                        End Select

                                        Dim bCanShowIndividual As Boolean
                                        Dim bCanShowGroup As Boolean

                                        If psLocal.ResultsViewMode = ResultsView.rvIndividual Or psLocal.ResultsViewMode = ResultsView.rvBoth Then  ' D4589
                                            bCanShowIndividual = psLocal.CanShowIndividualResults()
                                            Model.CanShowIndividualResults = bCanShowIndividual
                                        Else
                                            Model.CanShowIndividualResults = False
                                        End If

                                        Model.CanEditModel = IsPM   ' D4116
                                        Model.ShowKnownLikelihoods = psLocal.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous

                                        If psLocal.ResultsViewMode = ResultsView.rvGroup Or psLocal.ResultsViewMode = ResultsView.rvBoth Then   ' D4589
                                            bCanShowGroup = CanShowCombinedResults(psLocal) ' D4375 psLocal.CanShowGroupResults()
                                            Model.CanShowGroupResults = bCanShowGroup
                                        Else
                                            Model.CanShowGroupResults = False
                                        End If
                                        'C0810==

                                        'A1100 ===
                                        If psLocal.ResultsViewMode = ResultsView.rvBoth AndAlso psLocal.ParentNode.Hierarchy.ProjectManager.UsersList.Count <= 1 Then ' D4589
                                            bCanShowGroup = False
                                            Model.CanShowGroupResults = False
                                            Model.ShowGroupResults = False
                                        End If
                                        'A1100 ==

                                        Model.InsufficientInfo = Not ((Model.ShowIndividualResults AndAlso Model.CanShowIndividualResults) OrElse (Model.ShowGroupResults AndAlso Model.CanShowGroupResults))
                                        'A0322 ===
                                        If Model.ShowGroupResults AndAlso Model.CanShowGroupResults AndAlso Model.ShowIndividualResults AndAlso (Not Model.CanShowIndividualResults) Then
                                            Model.CanNotShowLocalResults = True
                                            Model.InsufficientInfo = False
                                            Model.ShowIndividualResults = False 'For hiding the column
                                        End If
                                        'A0322 ==

                                        'C0810===
                                        'psLocal.ParentNode.Judgments.CombineJudgments()
                                        'psLocal.ParentNode.CalculateLocal(acp.ProjectManager.User.UserID)
                                        'C0810==

                                        Dim mResultsList As ArrayList = Nothing 'C0810
                                        Dim mIndividualResultsList As ArrayList = Nothing 'C0811
                                        Dim mGroupResultsList As ArrayList = Nothing 'C0811

                                        If psLocal.ResultsViewMode <> ResultsView.rvNone Then   ' D4589
                                            ' D4385: get combined data at first since need to have real Inconsistency
                                            If bCanShowGroup And
                                                (psLocal.ResultsViewMode = ResultsView.rvGroup Or psLocal.ResultsViewMode = ResultsView.rvBoth) Then    ' D4589
                                                ' D4375 ===
                                                If CombinedUserID = COMBINED_USER_ID Then
                                                    mGroupResultsList = CType(psLocal.ResultsList(.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, AHPUserID).Clone, ArrayList)  ' D4386 // add .Clone
                                                Else
                                                    mGroupResultsList = CType(psLocal.ResultsList(CombinedUserID, CombinedUserID).Clone, ArrayList)
                                                End If
                                                ' D4375 ==
                                            End If

                                            'If bCanShowIndividual And _
                                            '    (.PipeBuilder.PipeParameters.LocalResultsView = ResultsView.rvIndividual Or .PipeBuilder.PipeParameters.LocalResultsView = ResultsView.rvBoth) Then
                                            mIndividualResultsList = CType(psLocal.ResultsList(AHPUserID, AHPUserID).Clone, ArrayList)
                                            mResultsList = mIndividualResultsList
                                            'End If

                                            If mResultsList Is Nothing Then
                                                mResultsList = mGroupResultsList
                                            End If

                                        End If

                                        'A0054 ===
                                        If (bCanShowIndividual OrElse bCanShowGroup) AndAlso psLocal.ShowExpectedValue Then
                                            If bCanShowIndividual AndAlso Not IsCombinedUserID(AHPUserID) Then
                                                Model.ExpectedValueIndiv = psLocal.ExpectedValue(AHPUserID)
                                                Model.ExpectedValueIndivVisible = True
                                            End If

                                            If bCanShowGroup Then
                                                Model.ExpectedValueComb = psLocal.ExpectedValue(CombinedUserID) ' D4375
                                                Model.ExpectedValueCombVisible = True
                                            End If
                                        End If

                                        Dim list As List(Of StepsPairs) = GetEvalPipeStepsList(psLocal.ParentNode.NodeID, iStep, psLocal.PWOutcomesNode)

                                        'Cycle through 1 to N "inconsitency" to find each associted pair
                                        Dim numChildren As Integer
                                        Dim pwJudgments As clsPairwiseJudgments
                                        If psLocal.PWOutcomesNode Is Nothing Then
                                            Dim nodesBelow As List(Of clsNode) = psLocal.ParentNode.GetNodesBelow(AHPUserID)
                                            For i As Integer = nodesBelow.Count - 1 To 0 Step -1
                                                If nodesBelow(i).RiskNodeType = RiskNodeType.ntCategory Then nodesBelow.RemoveAt(i)
                                            Next
                                            numChildren = nodesBelow.Count
                                            pwJudgments = CType(psLocal.ParentNode.Judgments, clsPairwiseJudgments)
                                        Else
                                            numChildren = CType(psLocal.ParentNode.MeasurementScale, clsRatingScale).RatingSet.Count
                                            pwJudgments = psLocal.PWOutcomesNode.PWOutcomesJudgments
                                        End If

                                        Dim calcTarget As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttUser, psLocal.ParentNode.Hierarchy.ProjectManager.GetUserByID(AHPUserID))
                                        For i As Integer = 1 To CInt((numChildren * (numChildren - 1)) / 2)
                                            'Dim pwData As clsPairwiseMeasureData = CType(psLocal.ParentNode.Judgments, clsPairwiseJudgments).GetNthMostInconsistentJudgment(calcTarget, i)
                                            Dim pwData As clsPairwiseMeasureData
                                            If psLocal.PWOutcomesNode Is Nothing Then
                                                pwData = pwJudgments.GetNthMostInconsistentJudgment(calcTarget, i)
                                            Else
                                                pwData = pwJudgments.GetNthMostInconsistentJudgmentOutcomes(calcTarget, i, CType(psLocal.ParentNode.MeasurementScale, clsRatingScale))
                                            End If
                                            If pwData IsNot Nothing Then
                                                For Each Pair As StepsPairs In list
                                                    If (Pair.Obj1 = pwData.FirstNodeID AndAlso Pair.Obj2 = pwData.SecondNodeID) OrElse (Pair.Obj2 = pwData.FirstNodeID AndAlso Pair.Obj1 = pwData.SecondNodeID) Then
                                                        Pair.Rank = i
                                                    End If
                                                Next
                                            End If
                                        Next

                                        For Each Pair As StepsPairs In list
                                            'Dim pwData As clsPairwiseMeasureData = CType(psLocal.ParentNode.Judgments, clsPairwiseJudgments).GetBestFitJudgment(calcTarget, Pair.Obj1, Pair.Obj2)
                                            Dim pwData As clsPairwiseMeasureData
                                            If psLocal.PWOutcomesNode Is Nothing Then
                                                pwData = pwJudgments.GetBestFitJudgment(calcTarget, Pair.Obj1, Pair.Obj2)
                                            Else
                                                pwData = pwJudgments.GetBestFitJudgmentOutcomes(calcTarget, Pair.Obj1, Pair.Obj2, CType(psLocal.ParentNode.MeasurementScale, clsRatingScale))
                                            End If

                                            If pwData Is Nothing Then 'A0605
                                                Pair.BestFitValue = 0
                                                Pair.BestFitAdvantage = 0
                                            Else
                                                Pair.BestFitValue = pwData.Value
                                                Pair.BestFitAdvantage = pwData.Advantage
                                            End If
                                        Next
                                        'end ToDo

                                        Model.StepPairs = New List(Of StepsPairs)
                                        For Each pair As StepsPairs In list
                                            Model.StepPairs.Add(pair)
                                        Next
                                        'A0054 ==

                                        Model.ParentNodeName = psLocal.ParentNode.NodeName
                                        Model.ParentID = psLocal.ParentNode.NodeID
                                        Model.ParentNode = GetPipeStepParentNode(ps.ActionData, Model.ParentID)
                                        Model.IsParentNodeGoal = psLocal.ParentNode.ParentNode Is Nothing

                                        Dim parentNodeKnownLikelihood As String = ""
                                        If psLocal.ParentNode.ParentNode IsNot Nothing AndAlso psLocal.ParentNode.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous Then
                                            Dim nl As List(Of KnownLikelihoodDataContract) = psLocal.ParentNode.ParentNode.GetKnownLikelihoods()
                                            For Each item As KnownLikelihoodDataContract In nl
                                                If item.GuidID.Equals(psLocal.ParentNode.NodeGuidID) AndAlso item.Value > 0 Then parentNodeKnownLikelihood = item.Value.ToString
                                            Next
                                        End If
                                        Model.ParentNodeKnownLikelihood = parentNodeKnownLikelihood

                                        Dim PM = App.ActiveProject.ProjectManager
                                        If psLocal.ResultsViewMode <> ResultsView.rvNone Then  ' D4589
                                            If bCanShowIndividual And (psLocal.ResultsViewMode = ResultsView.rvIndividual OrElse psLocal.ResultsViewMode = ResultsView.rvBoth) Then ' D4589
                                                'get local priority of the parent node for active user
                                                PM.CalculationsManager.Calculate(calcTarget, PM.ActiveObjectives.Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
                                                Model.ParentNodeGlobalPriority = psLocal.ParentNode.WRTGlobalPriority
                                            End If

                                            If bCanShowGroup And
                                                (psLocal.ResultsViewMode = ResultsView.rvGroup OrElse psLocal.ResultsViewMode = ResultsView.rvBoth) Then    ' D4589
                                                'get local priority of the parent node for COMBINED
                                                ' D4375 ===
                                                If CombinedUserID = COMBINED_USER_ID Then
                                                    Dim CG As clsCombinedGroup = PM.CombinedGroups.GetDefaultCombinedGroup
                                                    Dim calcTargetCombined As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                                                    PM.CalculationsManager.Calculate(calcTargetCombined, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
                                                Else
                                                    Dim calcTargetCombined As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttUser, psLocal.ParentNode.Hierarchy.ProjectManager.GetUserByID(CombinedUserID))
                                                    PM.CalculationsManager.Calculate(calcTargetCombined, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
                                                End If
                                                ' D4375 ==
                                                Model.ParentNodeGlobalPriorityCombined = psLocal.ParentNode.WRTGlobalPriority 'A1372
                                            End If
                                        End If

                                        'A0321 ===
                                        If psLocal.ResultsViewMode <> ResultsView.rvNone Then   ' D4589

                                            If mResultsList IsNot Nothing Then
                                                ' === For PWNL measure type we need to show unnormalized results is sum<=100%, otherwise normalize, see case 6668
                                                Dim IsSumMore1_Individual As Boolean = False
                                                Dim IsSumMore1_Group As Boolean = False

                                                Dim Sum_Individual As Double = 0
                                                Dim Sum_Group As Double = 0

                                                For i As Integer = 0 To mResultsList.Count - 1
                                                    If mIndividualResultsList IsNot Nothing AndAlso mIndividualResultsList.Count > i Then Sum_Individual += CType(mIndividualResultsList(i), clsResultsItem).UnnormalizedValue
                                                    If mGroupResultsList IsNot Nothing AndAlso mGroupResultsList.Count > i Then Sum_Group += CType(mGroupResultsList(i), clsResultsItem).UnnormalizedValue
                                                Next

                                                If Sum_Individual > 1 + 0.000001 Then IsSumMore1_Individual = True
                                                If Sum_Group > 1 + 0.000001 Then IsSumMore1_Group = True
                                                ' ==

                                                ' ===  this is for case 6635 - indicate if normaliztion was forced
                                                Model.IsPWNLandNormalizedParticipantResults = psLocal.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous AndAlso IsSumMore1_Individual
                                                Model.IsPWNLandNormalizedGroupResults = psLocal.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous AndAlso IsSumMore1_Group
                                                ' ==                                                

                                                Model.ObjectivesData.Clear()
                                                For i As Integer = 0 To mResultsList.Count - 1
                                                    Dim listItem As clsResultsItem = CType(mResultsList(i), clsResultsItem)
                                                    Dim Res As New Objective
                                                    Res.Parent = Model.ObjectivesData
                                                    Res.Name = listItem.Name

                                                    'If bCanShowIndividual And (.PipeBuilder.PipeParameters.LocalResultsView = ResultsView.rvIndividual Or .PipeBuilder.PipeParameters.LocalResultsView = ResultsView.rvBoth) Then
                                                    If True Then
                                                        Dim R As clsResultsItem = CType(mIndividualResultsList(i), clsResultsItem)
                                                        Res.Value = CSng(IIf(psLocal.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous, IIf(IsSumMore1_Individual, R.Value, R.UnnormalizedValue), R.UnnormalizedValue))   ' D2026 'A0854 'A1029
                                                        Res.GlobalValue = Res.Value * Model.ParentNodeGlobalPriority
                                                    End If

                                                    If bCanShowGroup And
                                                        (psLocal.ResultsViewMode = ResultsView.rvGroup OrElse psLocal.ResultsViewMode = ResultsView.rvBoth) Then    ' D4589
                                                        Dim R As clsResultsItem = CType(mGroupResultsList(i), clsResultsItem)
                                                        Res.CombinedValue = CSng(IIf(psLocal.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous, IIf(IsSumMore1_Individual, R.Value, R.UnnormalizedValue), R.UnnormalizedValue)) ' D2026 'A0854 'A1029
                                                        Res.GlobalValueCombined = Res.CombinedValue * Model.ParentNodeGlobalPriorityCombined
                                                    End If

                                                    If psLocal.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                        Dim KnownLikelihoods As List(Of KnownLikelihoodDataContract) = psLocal.ParentNode.GetKnownLikelihoods()
                                                        If KnownLikelihoods IsNot Nothing Then
                                                            Dim k As Integer = 0
                                                            While k <= KnownLikelihoods.Count - 1
                                                                Dim lkhd = KnownLikelihoods(k)
                                                                If listItem.ObjectID = lkhd.ID AndAlso lkhd.Value > 0 Then
                                                                    Res.AltWithKnownLikelihoodName = lkhd.NodeName
                                                                    Res.AltWithKnownLikelihoodID = lkhd.ID
                                                                    Res.AltWithKnownLikelihoodGuidID = lkhd.GuidID
                                                                    Res.AltWithKnownLikelihoodValue = lkhd.Value
                                                                    If Res.AltWithKnownLikelihoodValue > 0 Then
                                                                        Res.AltWithKnownLikelihoodValueString = Res.AltWithKnownLikelihoodValue.ToString
                                                                    End If
                                                                    k = KnownLikelihoods.Count
                                                                End If
                                                                k += 1
                                                            End While
                                                        End If
                                                    End If

                                                    Res.ID = listItem.ObjectID
                                                    Model.ObjectivesData.Add(Res)
                                                    Res.Index = Model.ObjectivesData.Count
                                                Next

                                                For Each obj As Objective In Model.ObjectivesData
                                                    obj.StepPairs = Model.StepPairs
                                                Next

                                                ' Normalize

                                                'A1070 ===
                                                Dim IndivExpectedValue As Double = 0
                                                Dim CombinedExpectedValue As Double = 0
                                                Dim CanShowHiddenExpectedValue As Boolean = False
                                                'A1070 ==

                                                'A1029 ===
                                                If Model.ObjectivesData IsNot Nothing AndAlso Model.ObjectivesData.Count > 0 Then
                                                    For Each item In Model.ObjectivesData
                                                        item.UnnormalizedValue = item.Value
                                                        item.UnnormalizedCombinedValue = item.CombinedValue
                                                    Next
                                                    Select Case NormalizationMode
                                                        Case AlternativeNormalizationOptions.anoPriority
                                                            Dim fSumValue As Double = Model.ObjectivesData.Sum(Function(d) d.Value)
                                                            Dim fSumCombinedValue As Double = Model.ObjectivesData.Sum(Function(d) d.CombinedValue)
                                                            Dim fSumGlobalValue As Double = Model.ObjectivesData.Sum(Function(d) d.GlobalValue)
                                                            Dim fSumGlobalCombinedValue As Double = Model.ObjectivesData.Sum(Function(d) d.GlobalValueCombined)
                                                            If fSumValue = 0 Then fSumValue = 1
                                                            If fSumCombinedValue = 0 Then fSumCombinedValue = 1
                                                            If fSumGlobalValue = 0 Then fSumGlobalValue = 1
                                                            If fSumGlobalCombinedValue = 0 Then fSumGlobalCombinedValue = 1
                                                            For Each item In Model.ObjectivesData
                                                                item.Value = item.UnnormalizedValue / fSumValue
                                                                item.CombinedValue = item.CombinedValue / fSumCombinedValue
                                                                item.GlobalValue = item.GlobalValue / fSumGlobalValue
                                                                item.GlobalValueCombined = item.GlobalValueCombined / fSumGlobalCombinedValue
                                                            Next
                                                        Case AlternativeNormalizationOptions.anoPercentOfMax
                                                            Dim fMaxValue As Double = Model.ObjectivesData.Max(Function(d) d.Value)
                                                            Dim fMaxCombinedValue As Double = Model.ObjectivesData.Max(Function(d) d.CombinedValue)
                                                            Dim fMaxGlobalValue As Double = Model.ObjectivesData.Max(Function(d) d.GlobalValue)
                                                            Dim fMaxGlobalCombinedValue As Double = Model.ObjectivesData.Max(Function(d) d.GlobalValueCombined)
                                                            If fMaxValue = 0 Then fMaxValue = 1
                                                            If fMaxCombinedValue = 0 Then fMaxCombinedValue = 1
                                                            If fMaxGlobalValue = 0 Then fMaxGlobalValue = 1
                                                            If fMaxGlobalCombinedValue = 0 Then fMaxGlobalCombinedValue = 1
                                                            For Each item In Model.ObjectivesData
                                                                item.Value = item.UnnormalizedValue / fMaxValue
                                                                item.CombinedValue = item.CombinedValue / fMaxCombinedValue
                                                                item.GlobalValue = item.GlobalValue / fMaxGlobalValue
                                                                item.GlobalValueCombined = item.GlobalValueCombined / fMaxGlobalCombinedValue
                                                            Next
                                                    End Select

                                                    ' parse the intensity name and try to calculate the Expected Value (hidden and available on Grid View double click)
                                                    'A1070 ===
                                                    For Each item In Model.ObjectivesData
                                                        Dim s As String() = item.Name.Split(CType(" ", Char()))
                                                        If s.Length > 0 Then
                                                            'If "1234567890,. ".Contains(item.Name(0)) Then
                                                            '    Dim s As String = ""
                                                            '    Dim i As Integer = 0
                                                            '    While i < item.Name.Length AndAlso "1234567890,. ".Contains(item.Name(i))
                                                            '        s += item.Name(i)
                                                            '        i += 1
                                                            '    End While
                                                            Dim d As Double = 0
                                                            If String2Double(s(0), d) Then
                                                                CanShowHiddenExpectedValue = True
                                                                IndivExpectedValue += d * item.UnnormalizedValue
                                                                CombinedExpectedValue += d * item.UnnormalizedCombinedValue
                                                            End If
                                                        End If
                                                    Next
                                                    'A1070 ==

                                                    ' fill in ClipData - clipboard data
                                                    For i As Integer = 0 To Model.ObjectivesData.Count - 1
                                                        For j As Integer = 0 To i - 1
                                                            ClipData += vbTab
                                                        Next
                                                        For j As Integer = i + 1 To Model.ObjectivesData.Count - 1
                                                            Dim p As StepsPairs = Nothing
                                                            Dim pn As StepsPairs = Nothing
                                                            For Each pair As StepsPairs In list
                                                                If pair.Obj1 = Model.ObjectivesData(i).ID AndAlso pair.Obj2 = Model.ObjectivesData(j).ID Then
                                                                    p = pair
                                                                End If
                                                                If pair.Obj2 = Model.ObjectivesData(i).ID AndAlso pair.Obj1 = Model.ObjectivesData(j).ID Then
                                                                    pn = pair
                                                                End If
                                                            Next
                                                            ClipData += vbTab
                                                            If p IsNot Nothing OrElse pn IsNot Nothing Then
                                                                If p IsNot Nothing Then ClipData += CStr(IIf(p.Advantage <= 0, "-", "")) + p.Value.ToString
                                                                If pn IsNot Nothing Then ClipData += CStr(IIf(pn.Advantage <= 0, "", "-")) + pn.Value.ToString
                                                                'If j < Model.ObjectivesData.Count - 1 Then ClipData += CStr(IIf(ClipData = "", "", vbTab))
                                                                'Else
                                                                'If j < Model.ObjectivesData.Count - 1 Then ClipData += vbTab
                                                            End If
                                                        Next
                                                        If i < Model.ObjectivesData.Count - 1 Then ClipData += "CRLF" 'vbNewLine
                                                    Next
                                                End If
                                                'A1029 ==

                                                'A1070 ===
                                                If CanShowHiddenExpectedValue Then
                                                    If bCanShowIndividual Then
                                                        ExpectedValueString = "Expected value (Participant) = " + Double2String(IndivExpectedValue)
                                                    End If
                                                    If bCanShowGroup Then
                                                        ExpectedValueString += CStr(IIf(ExpectedValueString = "", "", "<br/>")) + "Expected value (Combined) = " + Double2String(CombinedExpectedValue)
                                                    End If
                                                End If
                                                'A1070 ==

                                                ' Sort
                                                If SortExpression = IntermediateResultsSortMode.smNone Then
                                                    Select Case App.ActiveProject.PipeParameters.LocalResultsSortMode
                                                        Case ResultsSortMode.rsmNumber
                                                            SortExpression = IntermediateResultsSortMode.smIndex
                                                            SortDirection = SortDirection.Ascending
                                                        Case ResultsSortMode.rsmName
                                                            SortExpression = IntermediateResultsSortMode.smName
                                                            SortDirection = SortDirection.Ascending
                                                        Case ResultsSortMode.rsmPriority
                                                            SortExpression = IntermediateResultsSortMode.smValue
                                                            SortDirection = SortDirection.Descending
                                                        Case ResultsSortMode.rsmCombined
                                                            SortExpression = IntermediateResultsSortMode.smCombined
                                                            SortDirection = SortDirection.Descending
                                                    End Select
                                                End If
                                                'With Model
                                                '    Select Case App.ActiveProject.PipeParameters.LocalResultsSortMode
                                                '        Case ResultsSortMode.rsmNumber
                                                '            .ObjectivesData.Sort(New ObjectivesIndexComparer)
                                                '        Case ResultsSortMode.rsmName
                                                '            .ObjectivesData.Sort(New ObjectivesNamesComparer)
                                                '        Case ResultsSortMode.rsmPriority
                                                '            If .ShowIndividualResults AndAlso .CanShowIndividualResults Then .ObjectivesData.Sort(New ObjectivesPriorityComparer)
                                                '        Case ResultsSortMode.rsmCombined
                                                '            If .ShowGroupResults AndAlso .CanShowGroupResults Then .ObjectivesData.Sort(New ObjectivesCombinedPriorityComparer)
                                                '    End Select
                                                'End With

                                                Model.ObjectivesDataSorted = New List(Of Objective)
                                                Model.ObjectivesDataSorted.AddRange(Model.ObjectivesData)

                                                If (InconsistencySortingEnabled Is Nothing) Then
                                                    'restore previous sorting order if possible
                                                    'Dim soList As List(Of Integer) = InconsistencySortingOrderSavedList
                                                    'If soList IsNot Nothing Then
                                                    '    For Each obj As Objective In Model.ObjectivesDataSorted
                                                    '        obj.SortOrder = Integer.MaxValue
                                                    '        If soList.Contains(obj.ID) Then obj.SortOrder = soList.IndexOf(obj.ID)
                                                    '    Next
                                                    '    Model.ObjectivesDataSorted.Sort(Function(obj1, obj2) obj1.SortOrder.CompareTo(obj2.SortOrder))
                                                    'End If
                                                Else
                                                    If InconsistencySortingEnabled.Value Then ' Sort by priority
                                                        If InconsistencySortingDir = "desc" Then
                                                            Model.ObjectivesDataSorted.Sort(Function(obj1, obj2) obj2.Value.CompareTo(obj1.Value))
                                                        Else
                                                            Model.ObjectivesDataSorted.Sort(Function(obj1, obj2) obj1.Value.CompareTo(obj2.Value))
                                                        End If
                                                    Else 'original sorting order
                                                    End If

                                                    Dim SortOrder As Integer = 0
                                                    Dim soList As New List(Of Integer)
                                                    For Each obj As Objective In Model.ObjectivesDataSorted
                                                        obj.SortOrder = SortOrder
                                                        SortOrder += 1
                                                        soList.Add(obj.ID)
                                                    Next

                                                    'InconsistencySortingOrderSavedList = soList
                                                    InconsistencySortingEnabled = Nothing
                                                End If
                                            End If
                                        End If
                                        'A0321 ==

                                        If psLocal.ShowConsistency AndAlso .PipeParameters.ShowConsistencyRatio And Not psLocal.OnlyMainDiagonalEvaluated AndAlso bCanShowIndividual Then
                                            Model.Inconsistency = CSng(IIf(IsCombinedUserID(AHPUserID), psLocal.InconsistencyCombined, psLocal.InconsistencyIndividual))
                                            Model.InconsistencyVisible = Not (Model.ObjectivesData IsNot Nothing AndAlso Model.ObjectivesData.Count > 15 AndAlso Not psLocal.OnlyMainDiagonalEvaluated)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If

                    ' D1965 ===
                    If fIsIntensties Then
                        .ActiveHierarchy = fOriginalHID ' D1807
                        '.PipeBuilder.PipeCreated = False
                        If .PipeParameters.CurrentParameterSet IsNot .PipeParameters.DefaultParameterSet Then .PipeParameters.CurrentParameterSet = .PipeParameters.DefaultParameterSet
                    End If
                    ' D1965 ==

                End With
            End If
        End If
    End Sub

    Private Function PasteJudgmentsDataFromText(data As String) As Boolean
        'Return True
        Dim retVal As Boolean = False
        Dim lines As String() = data.Split(Chr(10))
        Dim PM As clsProjectManager = App.ActiveProject.ProjectManager
        Dim tNode As clsNode = Model.ParentNode 'todo OutcomesNode
        Dim tUserID As Integer = App.ActiveProject.ProjectManager.UserID
        tNode.Judgments.DeleteJudgmentsFromUser(tUserID)
        Dim N As Integer = Model.ObjectivesData.Count
        If lines.Length < N Then N = lines.Length
        Dim valsDec As Integer = 0
        If tNode IsNot Nothing AndAlso IsPairwiseMeasureType(tNode.MeasureType) Then
            For i As Integer = 0 To N - 1
                Dim line As String = lines(i)
                Dim vals As String() = line.Split(CChar(vbTab))
                If i = 0 Then
                    If String.IsNullOrEmpty(vals(0)) OrElse vals(0) = vbLf Then valsDec = 0 Else valsDec = 1
                End If
                For j As Integer = i + 1 To Model.ObjectivesData.Count - 1
                    Dim v As Double = 0
                    If vals.Length > j - valsDec AndAlso Not String.IsNullOrEmpty(vals(j - valsDec)) AndAlso Not vals(j - valsDec) = vbLf AndAlso String2Double(vals(j - valsDec), v) Then
                        Dim Advantage As Integer = 1
                        If v < 0 Then Advantage = -1
                        ' write judgment for a pair                            
                        'Dim newpwMD As clsPairwiseMeasureData = New clsPairwiseMeasureData(Model.ObjectivesData(i).ID, Model.ObjectivesData(j).ID, Advantage, v, Model.ParentID, tUserID)
                        Dim newpwMD As clsPairwiseMeasureData = New clsPairwiseMeasureData(Model.ObjectivesData(i).ID, Model.ObjectivesData(j).ID, Advantage, Math.Abs(v), Model.ParentID, tUserID)
                        tNode.Judgments.AddMeasureData(newpwMD)
                        retVal = True
                    End If
                Next
            Next
            Dim saveTime As DateTime = Now
            App.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(tUserID)
        End If
        ' save all judgments if changed (retVal = True?)
        If retVal Then
            With App.ActiveProject.ProjectManager
                .PipeBuilder.PipeCreated = False
                .PipeBuilder.CreatePipe()
            End With
        End If
        Return retVal
    End Function

    Public Function GetJudgmentFromUser(tAHPUserID As Integer, node As clsNode, data As clsShowLocalResultsActionData, wrtnodeID As Integer, obj1 As Integer, obj2 As Integer) As clsPairwiseMeasureData
        If data.PWOutcomesNode IsNot Nothing Then
            For Each judgment As clsPairwiseMeasureData In node.PWOutcomesJudgments.JudgmentsFromUser(tAHPUserID)
                If judgment.ParentNodeID = wrtnodeID AndAlso judgment.FirstNodeID = obj1 AndAlso judgment.SecondNodeID = obj2 Then Return judgment
            Next
        Else
            For Each judgment As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(tAHPUserID)
                If judgment.ParentNodeID = wrtnodeID AndAlso judgment.FirstNodeID = obj1 AndAlso judgment.SecondNodeID = obj2 Then Return judgment
            Next
        End If
        Return Nothing
    End Function

    Function GetEvalPipePWStepData(wrtnodeID As Integer, obj1 As Integer, obj2 As Integer) As clsPairwiseMeasureData
        For i As Integer = 0 To App.ActiveProject.ProjectManager.Pipe.Count - 1
            Dim action As clsAction = CType(App.ActiveProject.ProjectManager.Pipe(i), clsAction)
            Select Case action.ActionType
                Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                    Dim pwd As clsPairwiseMeasureData = CType(action.ActionData, clsPairwiseMeasureData)
                    If (action.ActionType = ActionType.atPairwise And pwd.ParentNodeID = wrtnodeID) Or (action.ActionType = ActionType.atPairwiseOutcomes And action.ParentNode IsNot Nothing AndAlso action.ParentNode.NodeID = wrtnodeID) Then
                        If pwd.FirstNodeID = obj1 AndAlso pwd.SecondNodeID = obj2 Then
                            Return pwd
                        End If
                    End If
                Case ActionType.atAllPairwise
                    Dim pwd As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)
                    If pwd.Judgments IsNot Nothing AndAlso pwd.Judgments.Count > 0 Then
                        For Each judgment As clsPairwiseMeasureData In pwd.Judgments
                            If judgment.ParentNodeID = wrtnodeID Then
                                If judgment.FirstNodeID = obj1 AndAlso judgment.SecondNodeID = obj2 Then 
                                    Return judgment
                                End If
                            End If
                        Next
                    End If
            End Select
        Next

        Return Nothing
    End Function

    Private Function GetEvalPipeStepsList(wrtnodeID As Integer, CurrentStep As Integer, OutcomesNode As clsNode) As List(Of StepsPairs)
        Dim list As New List(Of StepsPairs)
        Dim action As clsAction
        For i As Integer = 0 To CurrentStep - 1
            action = CType(App.ActiveProject.ProjectManager.Pipe(i), clsAction)
            Select Case action.ActionType
                Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                    Dim pwd As clsPairwiseMeasureData = CType(action.ActionData, clsPairwiseMeasureData)
                    If (action.ActionType = ActionType.atPairwise And pwd.ParentNodeID = wrtnodeID) Or (action.ActionType = ActionType.atPairwiseOutcomes And action.ParentNode IsNot Nothing AndAlso action.ParentNode.NodeID = wrtnodeID) Then
                        Dim pair As New StepsPairs
                        pair.Obj1 = pwd.FirstNodeID
                        pair.Obj2 = pwd.SecondNodeID
                        pair.Value = pwd.Value
                        pair.Advantage = pwd.Advantage
                        pair.IsUndefined = pwd.IsUndefined
                        pair.StepNumber = i
                        list.Add(pair)
                    End If
                Case ActionType.atAllPairwise
                    Dim pwd As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)
                    If pwd.Judgments IsNot Nothing AndAlso pwd.Judgments.Count > 0 Then
                        For Each judgment As clsPairwiseMeasureData In pwd.Judgments
                            If judgment.ParentNodeID = wrtnodeID Then
                                Dim pair As New StepsPairs
                                pair.Obj1 = judgment.FirstNodeID
                                pair.Obj2 = judgment.SecondNodeID
                                pair.Value = judgment.Value
                                pair.Advantage = judgment.Advantage
                                pair.IsUndefined = judgment.IsUndefined
                                pair.StepNumber = i
                                list.Add(pair)
                            End If
                        Next
                    End If
                Case ActionType.atAllPairwiseOutcomes
                    Dim pwd As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)
                    If pwd.ParentNode.NodeID = wrtnodeID Then
                        If pwd.Judgments IsNot Nothing AndAlso pwd.Judgments.Count > 0 Then
                            For Each judgment As clsPairwiseMeasureData In pwd.Judgments
                                Dim pair As New StepsPairs
                                pair.Obj1 = judgment.FirstNodeID
                                pair.Obj2 = judgment.SecondNodeID
                                pair.Value = judgment.Value
                                pair.Advantage = judgment.Advantage
                                pair.IsUndefined = judgment.IsUndefined
                                pair.StepNumber = i
                                list.Add(pair)
                            Next
                        End If
                    End If
            End Select
        Next

        Dim parentNode As clsNode
        If OutcomesNode IsNot Nothing Then
            parentNode = OutcomesNode
        Else
            parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(wrtnodeID)
        End If
        If parentNode IsNot Nothing Then
            Dim judgments As List(Of clsCustomMeasureData)
            If OutcomesNode IsNot Nothing Then
                judgments = parentNode.PWOutcomesJudgments.JudgmentsFromUser(App.ActiveProject.ProjectManager.User.UserID)
            Else
                judgments = parentNode.Judgments.JudgmentsFromUser(App.ActiveProject.ProjectManager.User.UserID)
            End If
            For Each J As clsCustomMeasureData In judgments
                If Not J.IsUndefined Then
                    Dim pwData As clsPairwiseMeasureData = CType(J, clsPairwiseMeasureData)
                    Dim exists As Boolean = False
                    For Each Pair As StepsPairs In list
                        If (Pair.Obj1 = pwData.FirstNodeID And Pair.Obj2 = pwData.SecondNodeID) Or
                            (Pair.Obj2 = pwData.FirstNodeID And Pair.Obj1 = pwData.SecondNodeID) Then
                            exists = True
                        End If
                    Next
                    If Not exists Then
                        Dim pair As New StepsPairs
                        pair.Obj1 = pwData.FirstNodeID
                        pair.Obj2 = pwData.SecondNodeID
                        pair.Value = pwData.Value
                        pair.Advantage = pwData.Advantage
                        pair.IsUndefined = pwData.IsUndefined
                        pair.StepNumber = -1
                        list.Add(pair)
                    End If
                End If
            Next
        End If
        Return list
    End Function

    Private Sub SaveJudgments(ParentID As Integer, tNode As clsNode)
        ' D1702 ===
        If tNode IsNot Nothing AndAlso IsPairwiseMeasureType(tNode.MeasureType) Then
            Dim tUserID As Integer = App.ActiveProject.ProjectManager.UserID
            Dim tJudgments As New List(Of clsCustomMeasureData)
            For Each tJud As clsCustomMeasureData In tNode.Judgments.UsersJudgments(tUserID)
                Dim pwMeasureData As clsPairwiseMeasureData = CType(tJud, clsPairwiseMeasureData)
                Dim newpwMD As clsPairwiseMeasureData = New clsPairwiseMeasureData(pwMeasureData.FirstNodeID, pwMeasureData.SecondNodeID, pwMeasureData.Advantage, pwMeasureData.Value, pwMeasureData.ParentNodeID, tUserID, pwMeasureData.IsUndefined, pwMeasureData.Comment)
                tJudgments.Add(newpwMD)
            Next
            Session(String.Format(_SESS_JUDGMENTS, ParentID)) = tJudgments
            Session(_SESS_DATETIME) = App.ActiveProject.ProjectManager.User.LastJudgmentTime
            JudgmentsSaved = True
        End If
        ' D1702 ==
    End Sub

    Private Sub RestoreJudgments(ParentID As Integer, tNode As clsNode)
        ' D1702 ===
        Dim sSessName As String = String.Format(_SESS_JUDGMENTS, ParentID)
        If tNode IsNot Nothing AndAlso JudgmentsSaved AndAlso Session(sSessName) IsNot Nothing Then
            Dim tUserID As Integer = App.ActiveProject.ProjectManager.UserID
            Dim tJudgments As List(Of clsCustomMeasureData) = CType(Session(sSessName), List(Of clsCustomMeasureData))
            Dim DT As Nullable(Of Date) = Nothing
            If Session(_SESS_DATETIME) IsNot Nothing Then DT = CType(Session(_SESS_DATETIME), Date)
            tNode.Judgments.DeleteJudgmentsFromUser(tUserID)
            For Each tJud As clsCustomMeasureData In tJudgments
                Dim pwMeasureData As clsPairwiseMeasureData = CType(tJud, clsPairwiseMeasureData)
                Dim newpwMD As clsPairwiseMeasureData = New clsPairwiseMeasureData(pwMeasureData.FirstNodeID, pwMeasureData.SecondNodeID, pwMeasureData.Advantage, pwMeasureData.Value, pwMeasureData.ParentNodeID, tUserID, pwMeasureData.IsUndefined, pwMeasureData.Comment)
                tNode.Judgments.AddMeasureData(newpwMD, True)
                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(tNode, newpwMD)
            Next
            'App.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(tUserID, DT)
            Session.Remove(sSessName)
            JudgmentsSaved = False
            'App.ActiveProject.ProjectManager.StorageManager.Reader.LoadUserJudgments(tUserID, ParentID)
            With App.ActiveProject.ProjectManager
                .PipeBuilder.PipeCreated = False
                .PipeBuilder.CreatePipe()
            End With
        End If
        ' D1702 ==
    End Sub

#Region "Service"

    Public Model As DataModel

    ReadOnly Property _SESS_INCON_SORT As String
        Get
            Return String.Format("SESS_INCON_SORT_{0}_{1}", App.ProjectID, CurStep)
        End Get
    End Property

    Private _InconsistencySortingEnabled As Boolean? = True 'Set =Nothing for original order of objectives by default
    Public Property InconsistencySortingEnabled As Boolean?
        Get
            If App.HasActiveProject Then    ' D7573
                Dim tSess As Object = Session(_SESS_INCON_SORT)
                'If tSess Is Nothing Then _InconsistencySortingEnabled = True Else _InconsistencySortingEnabled = CType(tSess, Boolean?)
                _InconsistencySortingEnabled = CType(tSess, Boolean?)

                'disable sorting by default for Pairwise With Probabilities (Outcomes)
                Dim tAction As clsAction = CurAction
                If tAction IsNot Nothing AndAlso tAction.ActionType = ActionType.atShowLocalResults AndAlso tAction.ActionData IsNot Nothing AndAlso TypeOf tAction.ActionData Is clsShowLocalResultsActionData AndAlso CType(tAction.ActionData, clsShowLocalResultsActionData).PWOutcomesNode IsNot Nothing Then
                    _InconsistencySortingEnabled = False
                End If
            End If
            Return _InconsistencySortingEnabled
        End Get
        Set(value As Boolean?)
            _InconsistencySortingEnabled = value
            Session(_SESS_INCON_SORT) = value
        End Set
    End Property

    ReadOnly Property _SESS_INCON_SORT_DIR As String
        Get
            Return String.Format("SESS_INCON_SORT_DIR_{0}_{1}", App.ProjectID, CurStep)
        End Get
    End Property

    Public Property InconsistencySortingDir As String
        Get
            Dim tSessVar As Object = SessVar(_SESS_INCON_SORT_DIR)
            If tSessVar Is Nothing Then Return "desc"
            Return CStr(tSessVar)
        End Get
        Set(value As String)
            SessVar(_SESS_INCON_SORT_DIR) = value.ToString
        End Set
    End Property

    'ReadOnly Property _SESS_INCON_SORT_ORDER As String
    '    Get
    '        Return String.Format("SESS_INCON_SORT_ORDER_{0}_{1}", App.ProjectID, CurStep)
    '    End Get
    'End Property

    'Public Property InconsistencySortingOrderSavedList As List(Of Integer)
    '    Get
    '        Dim tSessVar As Object = Session(_SESS_INCON_SORT_ORDER)
    '        If tSessVar Is Nothing Then Return Nothing
    '        Return CType(tSessVar, List(Of Integer))
    '    End Get
    '    Set(value As List(Of Integer))
    '        Session(_SESS_INCON_SORT_ORDER) = value
    '    End Set
    'End Property

    Public Sub GetResultsPipeStepData()
        GetIntermediateResultsData(OperationID.oGetPipeStepData, GetParams())
    End Sub

    Public Sub ResetJudgmentValue(Obj1ID As Integer, Obj2ID As Integer, ParentID As Integer)
        If Not IsReadOnly Then 'A0546
            Dim param As String = String.Format("&Count=1&O01={1}&O02={2}&V0={3}&A0={4}&PID={5}", OperationID.oResetJudgments, Obj1ID, Obj2ID, 0, 1, ParentID)
            GetIntermediateResultsData(OperationID.oResetJudgments, GetParams() + param)
        End If
    End Sub

    Public Sub UpdateJudgmentValue(tValue As Double, Obj1ID As Integer, Obj2ID As Integer, Advantage As Integer, ParentID As Integer)
        If Not IsReadOnly Then 'A0546
            Dim param As String = String.Format("&SetVal=1&Obj1={0}&Obj2={1}&Value={2}&Adv={3}&PID={4}", Obj1ID, Obj2ID, tValue, Advantage, ParentID)
            GetIntermediateResultsData(OperationID.oJudgmentUpdate, GetParams() + param)
        End If
    End Sub

    Public Sub RestoreJudgmentsClient(ParentID As Integer)
        If Not IsReadOnly Then
            Dim params As String = String.Format("&SetVal={0}&PID={1}", OperationID.oRestoreJudgments, ParentID)
            GetIntermediateResultsData(OperationID.oRestoreJudgments, GetParams() + params)
        End If
    End Sub

    Public Sub InvertAllJudgments(ParentID As Integer)
        If Not IsReadOnly Then
            Dim params As String = String.Format("&SetVal={0}&PID={1}", OperationID.oInvertAllJudgments, ParentID)
            GetIntermediateResultsData(OperationID.oInvertAllJudgments, GetParams() + params)
        End If
    End Sub

    Public Sub SavePWNL(tPWNL_AltID As Integer, tPWNL_NewValue As Double, ParentID As Integer)
        If Not IsReadOnly Then
            Dim params As String = String.Format("&SetVal={0}&PID={1}&AltID={2}&AltVal={3}", OperationID.oUpdatePWNL, ParentID, tPWNL_AltID, tPWNL_NewValue)
            GetIntermediateResultsData(OperationID.oUpdatePWNL, GetParams() + params)
        End If
    End Sub

#End Region

#Region "Intermediate results MainPage"

    Protected Sub Page_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        If Not IsPostBack AndAlso Not isCallback AndAlso App.HasActiveProject Then
            'pnlLoadingPanel.Caption = ResString("msgLoading")
            'pnlLoadingPanel.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")
            CallbackRestoreSortByPriority = ""
            CallbackEditsMade = ""
        End If
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack AndAlso Not isCallback AndAlso App.HasActiveProject Then ' D7577
            'If App.isRiskEnabled AndAlso Not App.ActiveProject.isImpact Then
            '    NormalizationMode = AlternativeNormalizationOptions.anoUnnormalized
            'Else
            '    NormalizationMode = AlternativeNormalizationOptions.anoPriority 'A1027
            'End If
            Dim tAction As clsAction = CurAction
            If tAction IsNot Nothing AndAlso tAction.ActionType = ActionType.atShowLocalResults Then
                BuildGridViewControl()
            End If
        End If
        Session(SESSION_PIPE_PGID) = CurrentPageID.ToString ' D7242
    End Sub

    Public ReadOnly Property SESSION_ALL_JUDGMENTS_SAME_SIDE As String
        Get
            Return String.Format("SESSION_ALL_JUDGMENTS_SAME_SIDE_{0}_{1}_{2}", App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, CurStep)
        End Get
    End Property

    Public Property AllJudgmentsOnSameSideWarningShown As Boolean
        Get
            Dim tSess As Object = SessVar(SESSION_ALL_JUDGMENTS_SAME_SIDE)
            If tSess IsNot Nothing Then Return CStr(tSess) = "1"
            Return False
        End Get
        Set(value As Boolean)
            SessVar(SESSION_ALL_JUDGMENTS_SAME_SIDE) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    Public fMultiEditsEnabled As Boolean = False
    Public fFocusedCellPair As StepsPairs = Nothing

    Public ReadOnly Property PARAM_HIDE_MESSAGES_IN_INCONSYSTENCY_PLUGIN As String
        Get
            Return String.Format("PARAM_HIDE_MESSAGES_IN_INCONSYSTENCY_PLUGIN_{0}", App.ProjectID)
        End Get
    End Property

    Public Const INTENSITY_CERTAIN_NAME As String = "Certain"

    Public Function IsReturn() As Boolean
        Return CheckVar("return", "0") > "0"
    End Function

    Public Sub CreateIntermediateResults()
        App.ActiveProject.ProjectManager.CheckCustomCombined()   ' D4376 + D4382
        ' D4375 + D4382 ===
        If Not IsPostBack AndAlso Not isCallback AndAlso CombinedUserID <> COMBINED_USER_ID Then
            App.ActiveProject.ProjectManager.StorageManager.Reader.LoadUserData(App.ActiveProject.ProjectManager.GetUserByID(CombinedUserID))
            ''CombinedJudgmentsLoaded = True ' -D4376
        End If
        ' D4375 + D4382 ==
        GetResultsPipeStepData()
        ReadIsolatedStorageSiteSettings()
        Model.ReadOnlyUI = IsReadOnly
        If Model IsNot Nothing AndAlso Not Model.ReadOnlyUI AndAlso Not IsPostBack AndAlso Not isCallback AndAlso Not IsReturn() Then SaveJudgments(Model.ParentID, Model.ParentNode)
    End Sub

    Public tMaxIntensityID, tCertainIntensityID As Integer

    Function MaxAndCertainStepID() As Integer
        Return GetStepNumber(tCertainIntensityID, tMaxIntensityID)
    End Function

    Public Function GetStepNumber(Obj1 As Integer, Obj2 As Integer) As Integer
        Dim retVal As Integer = Integer.MinValue
        If Model IsNot Nothing AndAlso Model.StepPairs IsNot Nothing Then
            For Each pair As StepsPairs In Model.StepPairs
                If ((pair.Obj1 = Obj1) And (pair.Obj2 = Obj2)) _
                Or ((pair.Obj1 = Obj2) And (pair.Obj2 = Obj1)) Then
                    retVal = pair.StepNumber
                End If
            Next
        End If
        Return retVal
    End Function

    Public Function GetFirstUndefinedStep() As String
        Dim retVal As String = "[]"
        Dim tNavigatedToUndefinedJudgment As Boolean = False
        If Model.StepPairs IsNot Nothing Then
            For Each pair In Model.StepPairs
                If pair.IsUndefined AndAlso Not tNavigatedToUndefinedJudgment Then
                    tNavigatedToUndefinedJudgment = True
                    retVal = String.Format("['{0}','{1}','{2}']", pair.StepNumber, pair.Obj1, pair.Obj2)
                End If
            Next
            If Not tNavigatedToUndefinedJudgment AndAlso Model.StepPairs.Count > 0 Then
                With Model.StepPairs(0)
                    retVal = String.Format("['{0}','{1}','{2}']", .StepNumber, .Obj1, .Obj2)
                End With
            End If
        End If
        Return retVal
    End Function

    Public ReadOnly Property GetFirstClusterStep() As Integer
        Get
            If Model.StepPairs IsNot Nothing AndAlso Model.StepPairs.Count > 0 Then Return Model.StepPairs(0).StepNumber Else Return -1
        End Get
    End Property

    Public ReadOnly Property GetFirstClusterStepObj1() As Integer
        Get
            If Model.StepPairs IsNot Nothing AndAlso Model.StepPairs.Count > 0 Then Return Model.StepPairs(0).Obj1 Else Return -1
        End Get
    End Property

    Public ReadOnly Property GetFirstClusterStepObj2() As Integer
        Get
            If Model.StepPairs IsNot Nothing AndAlso Model.StepPairs.Count > 0 Then Return Model.StepPairs(0).Obj2 Else Return -1
        End Get
    End Property

    Private Sub ReadIsolatedStorageSiteSettings()
        Model.HideHintMessages = False
        Dim tSessHideMessages As String = SessVar(PARAM_HIDE_MESSAGES_IN_INCONSYSTENCY_PLUGIN)
        If Not String.IsNullOrEmpty(tSessHideMessages) Then
            Model.HideHintMessages = tSessHideMessages.ToLower = "1" OrElse tSessHideMessages.ToLower = "true"
        End If
    End Sub

    Public Function AllJudgmentsOnSameSide() As Boolean
        AllJudgmentsOnSameSideWarningShown = True
        Dim res As Boolean = True
        If Model.StepPairs IsNot Nothing AndAlso Model.StepPairs.Count > 5 Then
            Dim sign As Integer = Model.StepPairs(0).Advantage
            For Each pair As StepsPairs In Model.StepPairs
                If pair.IsUndefined OrElse pair.Advantage <> sign Then
                    res = False
                    Exit For
                End If
            Next
        Else
            Return False
        End If
        Return res
    End Function

    Public Function HasCertainIntensityAndItIsLowerThanMax() As Boolean
        Dim retVal As Boolean = False
        Dim maxVal As Double = Double.MinValue

        tCertainIntensityID = -1
        tMaxIntensityID = -1

        If Model.ObjectivesData IsNot Nothing AndAlso Model.ObjectivesData.Count > 0 Then
            For Each item As Objective In Model.ObjectivesData
                If Not retVal AndAlso item.Name.Trim.ToLower.StartsWith(INTENSITY_CERTAIN_NAME.ToLower) Then
                    tCertainIntensityID = item.ID
                    retVal = True
                End If
                If maxVal < item.Value Then
                    maxVal = item.Value
                    tMaxIntensityID = item.ID
                End If
            Next
        End If

        Return retVal AndAlso tMaxIntensityID <> tCertainIntensityID
    End Function

#End Region

#Region "Intermediate Results Grid"

    Private Const COL_CHK As Integer = 0
    Private Const COL_INDEX As Integer = 1
    Private Const COL_TITLE As Integer = 2
    Private Const COL_PWNL As Integer = 3
    Private Const COL_RESULTS As Integer = 4
    Private Const COL_RESULTS_GLOBAL As Integer = 5
    Private Const COL_COMBINED As Integer = 6
    Private Const COL_COMBINED_GLOBAL As Integer = 7
    Private Const COL_GRAPH_BAR As Integer = 8
    Private max_bar_val As Double = 0

    Private Sub BuildGridViewControl()
        If Model IsNot Nothing Then
            If SortExpression <> IntermediateResultsSortMode.smNone Then
                Dim Comparer As New clsIntermediateResultsItemComparer2(SortExpression, SortDirection)
                Model.ObjectivesData.Sort(Comparer)
                Comparer = Nothing
            End If

            For Each obj In Model.ObjectivesData
                If obj.Value > max_bar_val Then max_bar_val = obj.Value
                If obj.CombinedValue > max_bar_val Then max_bar_val = obj.CombinedValue
            Next

            GridResults.DataSource = Model.ObjectivesData
            GridResults.DataBind()
            GridResults.Columns(1).Visible = App.ActiveProject.ProjectManager.Parameters.ResultsLocalShowIndex AndAlso App.ActiveProject.ProjectManager.Parameters.NodeVisibleIndexMode <> IDColumnModes.Rank 'A1139 + D6685
            If (GridResults.Columns.Count > 2) Then GridResults.Columns(GridResults.Columns.Count - 1).Visible = PM.Parameters.ResultsLocalShowBars    ' D7561
        End If
    End Sub

    Private Const _SESS_SHOW_GLOBAL_PWNL As String = "SESS_SHOW_GLOBAL_CLMN"
    Public Property ShowGlobalPWNL As Boolean
        Get
            Dim tSess As String = SessVar(_SESS_SHOW_GLOBAL_PWNL)
            If Not String.IsNullOrEmpty(tSess) Then Return tSess = "1"
            Return False
        End Get
        Set(value As Boolean)
            SessVar(_SESS_SHOW_GLOBAL_PWNL) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    Protected Sub GridControls_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles GridResults.RowDataBound
        If e.Row.Cells.Count <= 1 Then Exit Sub

        e.Row.Cells(COL_CHK).CssClass = "chk_cell"

        If Not Model.ShowKnownLikelihoods Then e.Row.Cells(COL_PWNL).Visible = False
        If Not Model.ShowIndividualResults Then
            e.Row.Cells(COL_RESULTS).Visible = False
            e.Row.Cells(COL_RESULTS_GLOBAL).Visible = False
        End If
        If Not Model.ShowGroupResults Then
            e.Row.Cells(COL_COMBINED).Visible = False
            e.Row.Cells(COL_COMBINED_GLOBAL).Visible = False
        End If
        If Not App.isRiskEnabled OrElse Model.IsParentNodeGoal Then
            e.Row.Cells(COL_RESULTS_GLOBAL).Visible = False
            e.Row.Cells(COL_COMBINED_GLOBAL).Visible = False
        Else
            e.Row.Cells(COL_RESULTS_GLOBAL).Visible = ShowGlobalPWNL AndAlso Model.ShowIndividualResults
            e.Row.Cells(COL_COMBINED_GLOBAL).Visible = ShowGlobalPWNL AndAlso Model.ShowGroupResults
        End If

        e.Row.Cells(COL_RESULTS).ForeColor = Color.FromArgb(CInt("&H84bd3e"))   ' D3849
        e.Row.Cells(COL_RESULTS_GLOBAL).ForeColor = Color.FromArgb(CInt("&H84bd3e"))    ' D3849

        e.Row.Cells(COL_COMBINED).ForeColor = Color.FromArgb(CInt("&H005395"))    ' D3849
        e.Row.Cells(COL_COMBINED_GLOBAL).ForeColor = Color.FromArgb(CInt("&H005395"))    ' D3849

        If e.Row.RowType = DataControlRowType.Header Then
            e.Row.Cells(COL_CHK).Text = "&nbsp;"
            e.Row.Height = 30

            ' D4375 ===
            e.Row.Cells(COL_TITLE).Text = ResString("tblEvaluationResultName")
            e.Row.Cells(COL_RESULTS).Text = ResString("lblEvaluationLegendUser")
            e.Row.Cells(COL_COMBINED).Text = ResString("lblEvaluationLegendCombined")
            ' D4375 ==

            If App.isRiskEnabled AndAlso Not IsIntensities Then
                Select Case App.ActiveProject.ProjectManager.ActiveHierarchy
                    Case ECHierarchyID.hidLikelihood
                        Dim header1 = String.Format(ResString("lblLikelihoodGiven"), Model.ParentNodeName)
                        Dim header2 = String.Format(ResString("lblLikelihoodAnd"), Model.ParentNodeName)
                        e.Row.Cells(COL_RESULTS).Text = header1
                        e.Row.Cells(COL_RESULTS_GLOBAL).Text = header2
                        e.Row.Cells(COL_COMBINED).Text = String.Format("{0} {1}", ResString("sGridHeaderCombined"), header1)
                        e.Row.Cells(COL_COMBINED_GLOBAL).Text = String.Format("{0} {1}", ResString("sGridHeaderCombined"), header2)
                    Case Else
                        Dim header1 = String.Format(ResString("lblImpactGiven"), Model.ParentNodeName)
                        Dim header2 = String.Format(ResString("lblImpactAnd"), Model.ParentNodeName)
                        e.Row.Cells(COL_RESULTS).Text = header1
                        e.Row.Cells(COL_RESULTS_GLOBAL).Text = header2
                        e.Row.Cells(COL_COMBINED).Text = String.Format("{0} {1}", ResString(" sGridHeaderCombined"), header1)
                        e.Row.Cells(COL_COMBINED_GLOBAL).Text = String.Format("{0} {1}", ResString("sGridHeaderCombined"), header2)
                End Select

                If Model.IsPWNLandNormalizedParticipantResults OrElse Model.IsPWNLandNormalizedGroupResults Then
                    Select Case App.ActiveProject.ProjectManager.ActiveHierarchy
                        Case ECHierarchyID.hidLikelihood
                            e.Row.Cells(COL_PWNL).Text = ResString("sGridHeaderPWNLAbsolute")
                        Case Else
                            e.Row.Cells(COL_PWNL).Text = ResString("sGridHeaderPWNImpactAbsolute")
                    End Select
                End If

                If Model.IsPWNLandNormalizedParticipantResults Then e.Row.Cells(COL_RESULTS).Text = ResString("sGridHeaderResultsNormalized")
                If Model.IsPWNLandNormalizedGroupResults Then e.Row.Cells(COL_COMBINED).Text = ResString("sGridHeaderCombinedNormalized")
            End If

            If App.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName <> "" Then e.Row.Cells(COL_COMBINED).Text = SafeFormString(App.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName) ' D4376 + D4384 + A1458

        Else
            e.Row.Cells(COL_INDEX).Text = String.Format("&nbsp;{0}&nbsp;", e.Row.Cells(COL_INDEX).Text)
            e.Row.Cells(COL_PWNL).HorizontalAlign = HorizontalAlign.Center
            e.Row.Cells(COL_RESULTS).HorizontalAlign = HorizontalAlign.Center
            e.Row.Cells(COL_RESULTS_GLOBAL).HorizontalAlign = HorizontalAlign.Center
            e.Row.Cells(COL_COMBINED).HorizontalAlign = HorizontalAlign.Center
            e.Row.Cells(COL_COMBINED_GLOBAL).HorizontalAlign = HorizontalAlign.Center

            If e.Row.DataItem IsNot Nothing Then
                Dim tRow As Objective = CType(e.Row.DataItem, Objective)
                e.Row.Cells(COL_CHK).HorizontalAlign = HorizontalAlign.Center
                Dim id As String = "cbChk_" + tRow.Index.ToString
                e.Row.Cells(COL_CHK).Text = "<input type='checkbox' class='chkOneOfPair' id='" + id + "' onClick='CheckPairs(" + tRow.Index.ToString + ",(this.checked),""" + id + """)' value='" + tRow.ID.ToString + "' />"
                e.Row.Cells(COL_TITLE).Text = "<div style='width: 300px; overflow: hidden; white-space: wrap; text-overflow: ellipsis; margin:5px;'>" + SafeFormString(tRow.Name) + "</div>"

                Dim isIndBar As Boolean = False 'has individual graph bar
                Dim isCmbBar As Boolean = False 'nas combined graph bar

                Dim bar1 As String = ""
                Dim bar2 As String = ""

                If Model.ShowIndividualResults Then
                    If (Model.MaxAltPriority > 0) AndAlso (Not Double.IsNaN(tRow.Value)) Then tRow.Value = Math.Round((tRow.Value / Model.MaxAltPriority) * 100)
                    isIndBar = True
                    bar1 = HTMLCreateGraphBarWithValues(CSng(tRow.Value * 100), CSng(max_bar_val * 100), 158, 8, "#84bd3e", ImagePath, Double2String(tRow.Value * 100, , True))  ' D3057 + D3849
                End If
                If Model.ShowGroupResults Then
                    If (Model.MaxAltPriority > 0) AndAlso (Not Double.IsNaN(tRow.CombinedValue)) Then tRow.CombinedValue = Math.Round((tRow.CombinedValue / Model.MaxAltPriority) * 100)
                    isCmbBar = True
                    bar2 = HTMLCreateGraphBarWithValues(CSng(tRow.CombinedValue * 100), CSng(max_bar_val * 100), 158, 8, "#005395", ImagePath, Double2String(tRow.CombinedValue * 100, , True))  ' D3057 + D3849
                End If

                If isCmbBar OrElse isIndBar Then
                    e.Row.Cells(COL_GRAPH_BAR).Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:164px'><tr valign=middle><td>{0}{1}</td></tr></table>", bar1, bar2)
                    e.Row.Cells(COL_GRAPH_BAR).HorizontalAlign = HorizontalAlign.Center
                End If

                e.Row.Attributes.Remove("onmouseover")
                e.Row.Attributes.Add("onmouseover", String.Format("RowHover(this,1,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
                e.Row.Attributes.Remove("onmouseout")
                e.Row.Attributes.Add("onmouseout", String.Format("RowHover(this,0,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))

                If App.isRiskEnabled Then
                    If Model.CanEditModel AndAlso Not IsReadOnly Then
                        e.Row.Cells(COL_PWNL).Attributes.Add("style", "cursor:pointer")
                        e.Row.Cells(COL_PWNL).Attributes.Add("onclick", String.Format("editPwnlClick('{0}'); return false;", tRow.ID.ToString))
                        e.Row.Cells(COL_PWNL).Attributes.Add("id", "pwnl_" + tRow.ID.ToString)
                    End If
                    e.Row.Cells(COL_PWNL).Text = String.Format("{0}", CStr(IIf(tRow.AltWithKnownLikelihoodValue = 0, "", Double2String(tRow.AltWithKnownLikelihoodValue * 100, , True))))
                    e.Row.Cells(COL_PWNL).HorizontalAlign = HorizontalAlign.Center
                End If
            End If
        End If

        If e.Row.RowType = DataControlRowType.Header Then
            For i As Integer = 1 To e.Row.Cells.Count - 2
                Dim sSort As String = ""
                If CInt(SortExpression) = i Then
                    sSort = CStr(IIf(SortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC))
                End If
                e.Row.Cells(i).Text = String.Format("<a href='' onclick='sendCommand(""action=sort&fld={0}""); return false;' class='actions'>{1}</a>{2}", IIf(SortDirection = SortDirection.Ascending, -i, i), e.Row.Cells(i).Text, sSort)
            Next
        End If
    End Sub

    Public Function HTMLCreateGraphBarWithValues(Value As Single, MaxValue As Single, BWidth As Integer, BHeight As Integer, sColor As String, ImagesPath As String, Optional ByVal sGraphHint As String = "", Optional fShowValueOverride As Boolean = False) As String    ' D0136 + D0323 + D2188
        Const tMargin As Integer = 2

        Dim sFill As String = ""    ' D2923

        If MaxValue < 0 Then Return "&nbsp;"
        If MaxValue = 0 Then Value = 0 Else Value = CSng(Value / (MaxValue + 0.00000001))

        Dim FillWidth As Integer = -1
        If Value >= 0 And Value <= 100 Then FillWidth = CInt(Math.Round((BWidth - tMargin) * Value)) - 1 ' D0147 'A0756
        If FillWidth < 0 Then FillWidth = 0
        If FillWidth > BWidth - tMargin Then FillWidth = BWidth - tMargin

        'If Not IsExport Then    ' D2923
        Dim sLH As String = Math.Floor(100 * BHeight / 9).ToString   ' D2417
        Dim sBG As String = If(FillWidth > 0 AndAlso FillWidth < BWidth - tMargin, " background: url(" + ImagesPath + "prg_bg_white.gif) repeat-y " + (FillWidth).ToString + "px", "")
        If FillWidth > 0 Then sBG += If(sBG = "", " background:", "") + " " + sColor
        Dim sVal As String = If(fShowValueOverride, sGraphHint, "&nbsp;")
        Dim sBar As String = String.Format("<div class='bar_value' style='line-height:{0}%; width:100%; height:100%;{1}' title='{3}'>{2}</div>", sLH, sBG, sVal, SafeFormString(sGraphHint))
        sFill = String.Format("<div class='progress' style='height:{1}px;width:{2}px;padding:1px;'>{0}</div>", sBar, BHeight, BWidth)
        'Else
        '    sFill = String.Format("<div><nobr><div style='display:inline;line-height:{2}px; height:{2}px; font-size:1px; width:{0}px; background:{3}; border:1px solid #d0d0d0; border-right:0px;'></div><div style='display:inline;line-height:{2}px; height:{2}px; font-size:1px; width:{1}px; background:{4}; border:1px solid #d0d0d0; border-left:0px;'></div><div class='text' style='display:inline;width:9ex; text-align:right'>{5}</div></nobr></div>", FillWidth, BWidth - FillWidth - tMargin, BHeight - tMargin, sColor, "#eaeaea", SafeFormString(sGraphHint), BWidth + 4)
        'End If
        Return sFill
    End Function

    Public Function GetStepsPairs() As String
        Dim retVal As String = ""

        If Model IsNot Nothing AndAlso Model.StepPairs IsNot Nothing Then
            For Each sp In Model.StepPairs
                retVal += If(retVal <> "", ",", "") + String.Format("[{0},{1},{2}]", sp.StepNumber, sp.Obj1, sp.Obj2)
            Next
        End If

        Return String.Format("var steps = [{0}];", retVal)
    End Function

    Public Enum IntermediateResultsSortMode
        smNone = 0
        smIndex = 1
        smName = 2
        smPWNL = 3
        smValue = 4
        smGlobal = 5
        smCombined = 6
        smCombinedGlobal = 7
    End Enum

    Public _SESS_SORT As String = "IntermediateResults_Sort"

    Private _SortField As IntermediateResultsSortMode = IntermediateResultsSortMode.smNone
    Public Property SortExpression() As IntermediateResultsSortMode
        Get
            Return _SortField
        End Get
        Set(value As IntermediateResultsSortMode)
            If _SortField <> value Then
                _SortField = value
                Session(_SESS_SORT) = _SortField
            End If
        End Set
    End Property

    Public _SESS_DIR As String = "IntermediateResults_Dir"

    Private _SortDir As SortDirection = SortDirection.Descending
    Public Property SortDirection() As SortDirection
        Get
            Return _SortDir
        End Get
        Set(value As SortDirection)
            If _SortDir <> value Then
                _SortDir = value
                Session(_SESS_DIR) = _SortDir
            End If
        End Set
    End Property

    Public Class clsIntermediateResultsItemComparer2
        Implements IComparer(Of Objective)

        Private _SortField As IntermediateResultsSortMode = IntermediateResultsSortMode.smNone
        Private _Direction As SortDirection = SortDirection.Descending

        Public Sub New(tSortField As IntermediateResultsSortMode, tDirection As SortDirection)
            _SortField = tSortField
            _Direction = tDirection
        End Sub

        Public Function Compare(A As Objective, B As Objective) As Integer Implements IComparer(Of Objective).Compare
            Dim Res As Integer = 0
            Select Case _SortField
                Case IntermediateResultsSortMode.smIndex
                    Res = A.Index.CompareTo(B.Index)
                Case IntermediateResultsSortMode.smName
                    Res = A.Name.CompareTo(B.Name)
                Case IntermediateResultsSortMode.smPWNL
                    Res = A.AltWithKnownLikelihoodValue.CompareTo(B.AltWithKnownLikelihoodValue)
                Case IntermediateResultsSortMode.smValue
                    Res = A.Value.CompareTo(B.Value)
                Case IntermediateResultsSortMode.smGlobal
                    Res = A.GlobalValue.CompareTo(B.GlobalValue)
                Case IntermediateResultsSortMode.smCombined
                    Res = A.CombinedValue.CompareTo(B.CombinedValue)
                Case IntermediateResultsSortMode.smCombinedGlobal
                    Res = A.GlobalValueCombined.CompareTo(B.GlobalValueCombined)
            End Select
            If _Direction = SortDirection.Descending AndAlso Res <> 0 Then Return -Res Else Return Res
        End Function
    End Class

#End Region

#Region "Inconsistency Improvement"

    Public Function GetObjectivesData() As String 'get sorted objectives data
        Dim retVal As String = ""

        If Model IsNot Nothing AndAlso Model.ObjectivesDataSorted IsNot Nothing Then
            For Each obj In Model.ObjectivesDataSorted
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("[{0},'{1}','{2}','{3}']", obj.ID, obj.Value, JS_SafeString(obj.Name), Double2String(obj.Value * 100, , True))
            Next
        End If

        'Return String.Format("var obj_data = [{0}];", retVal)
        Return String.Format("[{0}];", retVal)
    End Function

    Public Function GetPairsData() As String
        Dim retVal As String = ""

        If Model IsNot Nothing AndAlso Model.StepPairs IsNot Nothing Then
            For Each item In Model.StepPairs
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("[{0},'{1}',{2},{3},{4},'{5}',{6},'{7}','{8}']", item.StepNumber, item.Value, item.Obj1, item.Obj2, item.Advantage, item.BestFitValue, item.BestFitAdvantage, item.Rank, IIf(item.IsUndefined, "1", "0"))
            Next
        End If

        'Return String.Format("var step_data = [{0}];", retVal)
        Return String.Format("[{0}];", retVal)
    End Function

    Protected Sub divObjectivesData_PreRender(sender As Object, e As EventArgs) Handles divObjectivesData.PreRender
        divObjectivesData.InnerText = GetObjectivesData()
    End Sub

    Protected Sub divStepsData_PreRender(sender As Object, e As EventArgs) Handles divStepsData.PreRender
        divStepsData.InnerText = GetPairsData()
    End Sub

#End Region

    Public AjaxCallback As String = ""
    Public Property CallbackEditsMade As String
        Get
            Return SessVar("SESSION_CallbackEditsMade" + App.ProjectID.ToString)
        End Get
        Set(value As String)
            SessVar("SESSION_CallbackEditsMade" + App.ProjectID.ToString) = value
        End Set
    End Property

    Public ClipData As String = ""

    Public Property CallbackRestoreSortByPriority As String
        Get
            Return SessVar("SESSION_CallbackRestoreSortByPriority" + App.ProjectID.ToString)    ' D7573
        End Get
        Set(value As String)
            SessVar("SESSION_CallbackRestoreSortByPriority" + App.ProjectID.ToString) = value   ' D7573
        End Set
    End Property

    Private Sub CheckSortOrder()
        If InconsistencySortingEnabled.HasValue AndAlso InconsistencySortingEnabled.Value Then
            InconsistencySortingEnabled = False
            CallbackRestoreSortByPriority = "1"
        End If
    End Sub

    Protected Sub RadAjaxManagerMain_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxManagerMain.AjaxRequest
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(e.Argument)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower   'Anti-XSS
        AjaxCallback = sAction
        With App.ActiveProject.ProjectManager
            Select Case sAction
                Case "judgment"
                    CallbackEditsMade = "1"
                    Dim sValue As String = GetParam(args, "value").Trim
                    Dim value As Double = 0
                    Dim obj1 As Integer = -1
                    Dim obj2 As Integer = -1
                    If ((String2Double(sValue, value) AndAlso value >= 0) Or sValue = "") AndAlso Integer.TryParse(GetParam(args, "obj1"), obj1) AndAlso Integer.TryParse(GetParam(args, "obj2"), obj2) Then
                        Dim pair As clsPairwiseMeasureData = GetEvalPipePWStepData(Model.ParentID, obj1, obj2)
                        If pair IsNot Nothing Then
                            If sValue = "" Then
                                ResetJudgmentValue(obj1, obj2, Model.ParentID)
                            Else
                                If pair IsNot Nothing Then UpdateJudgmentValue(value, obj1, obj2, CInt(IIf(value < 0, -1, 1)) * pair.Advantage, Model.ParentID)  ' D7553
                            End If
                        Else
                            pair = GetEvalPipePWStepData(Model.ParentID, obj2, obj1)
                            If sValue = "" Then
                                ResetJudgmentValue(obj2, obj1, Model.ParentID)
                            Else
                                If pair IsNot Nothing Then UpdateJudgmentValue(value, obj2, obj1, CInt(IIf(value < 0, -1, 1)) * pair.Advantage, Model.ParentID) ' D7553
                            End If
                        End If
                    End If
                Case "restore_all"
                    CallbackEditsMade = "-1"
                    ' D4844 ===
                    If Model IsNot Nothing Then
                        Dim PNode As New Guid
                        If CurAction IsNot Nothing AndAlso CurAction.ActionType = ActionType.atShowLocalResults Then
                            Dim LR As clsShowLocalResultsActionData = CType(CurAction.ActionData, clsShowLocalResultsActionData)
                            If LR.ParentNode IsNot Nothing Then PNode = LR.ParentNode.NodeGuidID
                        End If
                        RestoreJudgmentsClient(Model.ParentID)
                        SaveJudgments(Model.ParentID, Model.ParentNode)
                        If Not PNode.Equals(Guid.Empty) Then
                            With App.ActiveProject.ProjectManager.Pipe
                                For i = CurStep - 1 To 0 Step -1
                                    If CType(.Item(i), clsAction).ActionType = ActionType.atShowLocalResults Then
                                        Dim LR As clsShowLocalResultsActionData = CType(CurAction.ActionData, clsShowLocalResultsActionData)
                                        If LR.ParentNode IsNot Nothing AndAlso PNode.Equals(LR.ParentNode.NodeGuidID) Then
                                            CurStep = i + 1
                                            Exit For
                                        End If
                                    End If
                                Next
                            End With
                        End If
                    End If
                    ' D4844 ==
                    'App.ActiveProject.ResetProject()    ' D4844
                Case "invert_judgment"
                    CallbackEditsMade = "1"
                    Dim obj1 As Integer = -1
                    Dim obj2 As Integer = -1
                    Dim ParentID As Integer = -1
                    If Integer.TryParse(GetParam(args, "obj1"), obj1) AndAlso Integer.TryParse(GetParam(args, "obj2"), obj2) AndAlso Integer.TryParse(GetParam(args, "pid"), ParentID) Then
                        Dim pair As clsPairwiseMeasureData = GetEvalPipePWStepData(ParentID, obj1, obj2)
                        If pair IsNot Nothing Then
                            UpdateJudgmentValue(pair.Value, obj1, obj2, -pair.Advantage, Model.ParentID)
                        Else
                            pair = GetEvalPipePWStepData(ParentID, obj2, obj1)
                            If pair IsNot Nothing Then
                                UpdateJudgmentValue(pair.Value, obj2, obj1, -pair.Advantage, Model.ParentID)
                            End If
                        End If
                    End If
                Case "invert_all"
                    CallbackEditsMade = "1"
                    InvertAllJudgments(Model.ParentID)
                Case "set_pwnl"
                    Dim altID As Integer = 0
                    Dim value As Double = 0
                    If String2Double(GetParam(args, "value"), value) AndAlso value >= 0 AndAlso value <= 1 AndAlso Integer.TryParse(GetParam(args, "alt_id"), altID) Then
                        SavePWNL(altID, value, Model.ParentID)
                    End If
                Case "sort"  'sort gridview by click on column header
                    Dim fld As Integer = -1
                    If Integer.TryParse(GetParam(args, "fld"), fld) Then
                        SortExpression = CType(Math.Abs(fld), IntermediateResultsSortMode)
                        If fld < 0 Then SortDirection = SortDirection.Descending Else SortDirection = SortDirection.Ascending
                    End If
                Case "sort_by_priority" 'checkbox "Sort by priority" on Inconsistency improvement                    
                    InconsistencySortingDir = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "dir")).ToLower 'Anti-XSS
                    InconsistencySortingEnabled = Param2Bool(args, "do")
                    GetResultsPipeStepData()
                Case "reset_sorting_and_copy_judgments" ', "reset_sorting"
                    CheckSortOrder()
                    GetResultsPipeStepData()
                Case "paste_from_clipboard"
                    CallbackEditsMade = "1"
                    CheckSortOrder()
                    If Not IsReadOnly Then
                        'Dim data As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "data"))    'Anti-XSS
                        Dim data As String = GetParam(args, "data") 'Not use ECSanitizer here, because it removes the first tab character and data gets corrupted
                        PasteJudgmentsDataFromText(data)
                    End If
                    GetResultsPipeStepData()
                Case "toggle_global" 'checkbox "Show global"
                    ShowGlobalPWNL = Not ShowGlobalPWNL
                Case ACTION_NORMALIZE_MODE
                    NormalizationMode = CType(CInt(GetParam(args, "value").Trim()), AlternativeNormalizationOptions)
                    GetResultsPipeStepData()
            End Select
        End With
        BuildGridViewControl()
    End Sub

    Protected Sub divCommand_PreRender(sender As Object, e As EventArgs) Handles divCommand.PreRender
        divCommand.InnerText = String.Format("['{0}', '{1}', '{2}']", AjaxCallback, IIf(CallbackEditsMade <> "-1" AndAlso (CallbackEditsMade = "1" OrElse IsReturn()), "1", "0"), IIf(InconsistencySortingEnabled.HasValue AndAlso InconsistencySortingEnabled.Value, "1", IIf(CallbackRestoreSortByPriority = "1", "2", "0")))
    End Sub

    Protected Sub divClipData_PreRender(sender As Object, e As EventArgs) Handles divClipData.PreRender
        divClipData.InnerText = ClipData
    End Sub

    'A1070 ===
    'Protected Sub divExpectedValue_PreRender(sender As Object, e As System.EventArgs) Handles divCommand.PreRender
    '    divExpectedValue.InnerText = String.Format("{0}", ExpectedValueString)
    'End Sub
    'A1070 ==
    'A1093 ===
    Private Sub lblExpectedValueLocal_PreRender(sender As Object, e As EventArgs) Handles lblExpectedValueLocal.PreRender
        lblExpectedValueLocal.InnerHtml = CStr(IIf(App.ActiveProject.ProjectManager.PipeParameters.ShowExpectedValueLocal, String.Format("{0}", ExpectedValueString + "<br/>"), ""))
        If String.IsNullOrEmpty(lblExpectedValueLocal.InnerHtml) Then lblExpectedValueLocal.Visible = False
    End Sub
    'A1093 ==

End Class

#End Region