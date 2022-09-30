Imports System.Xml
Imports ECCore.ECTypes

Partial Class SynthesisPage
    Inherits clsComparionCorePage

    Private Const CheckLicenseForStartTeamTime As Boolean = True ' TODO: AD, case 9433

    Private ignoreCallback As Boolean = True    ' D4243

    Public Const _PARAM_VIEW As String = "view"
    Public Const _TT_LINE_BREAK As String = "&#013;"

    Public Enum SynthesisViews
        vtMixed = -1
        vtAlternatives = 0
        vtObjectives = 1
        vtDSA = 2
        vtPSA = 3
        vtGSA = 4
        vt2D = 5
        vtHTH = 6
        vtCV = 7    ' Consensus View
        vtTree = 8  ' Consensus View Tree
        vtASA = 9   ' Performance delta Alternatives
        vtAlternativesChart = -2
        vtObjectivesChart = -3
        vt4ASA = -4 ' Four at a time
        'vtObjsAndAlts = -5
        vtBowTie = -6
        vtHeatMap = -7
        vtDashboard = -8
    End Enum

    Public SynthesisView As SynthesisViews = SynthesisViews.vtMixed

    ' options
    ' D6850
    ' moved to PM.Parameters.DecimalDigits
    'Public Property DecimalDigits As Integer
    '    Get
    '        Dim retVal As Integer = 2
    '        retVal = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID))
    '        If retVal < 0 Then retVal = 0
    '        If retVal > 5 Then retVal = 5
    '        Return retVal
    '    End Get
    '    Set(value As Integer)
    '        WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_DECIMALS_ID, AttributeValueTypes.avtLong, value)
    '    End Set
    'End Property

    Public Const DefaultLayout As Integer = 9 '3
    Public Const DefaultWidgets As String = "[2, 3, 4, 6, -1, -1]" '"[-1, -1, -1, -1, -1, -1]"    
    Public Const maxWidgets As Integer = 6
    Public Const Flt_Separator As String = vbTab    

    Private rand As New Random(DateTime.Now.Millisecond)

    Public Enum covObjsOption
        oShowChildren = 0
        oShowCoveringObjectives = 1
        oShowEverything = 2
    End Enum

    Public SelectOnlyCoveringObjectivesOption As covObjsOption = covObjsOption.oShowChildren 'A0632    

    ' actions
    Public Const ACTION_SYNTHESIS_MODE As String = "synthesis_mode" 'Ideal / Distributive
    Public Const ACTION_GRID_WRT_STATE As String = "wrt_state"
    Public Const ACTION_SWITCH_DSA As String = "switch_dsa"
    Public Const ACTION_SWITCH_ALTS_GRID As String = "switch_alts_grid"
    Public Const ACTION_SWITCH_OBJS_GRID As String = "switch_objs_grid"
    Public Const ACTION_SHOW_OBJ_LOCAL As String = "show_obj_local"
    Public Const ACTION_SHOW_OBJ_GLOBAL As String = "show_obj_global"
    Public Const ACTION_DSA_UPDATE_VALUES As String = "dsa_update_values"
    Public Const ACTION_DSA_RESET As String = "dsa_reset"    
    Public Const ACTION_GRID_WRT_NODE_ID As String = "grid_wrt_node_id"
    Public Const ACTION_CHANGE_LAYOUT As String = "change_layout" 'Ideal / Distributive
    Public Const ACTION_COMBINED_MODE As String = "combined_mode" 'AIP / AIJ
    Public Const ACTION_CIS_MODE As String = "use_cis"
    Public Const ACTION_USER_WEIGHTS_MODE As String = "user_weights"
    Public Const ACTION_NORMALIZE_MODE As String = "normalize_mode"
    Public Const ACTION_DECIMALS As String = "decimals"
    Public Const ACTION_SET_WIDGET As String = "set_widget"
    Public Const ACTION_SELECTED_USER As String = "sa_selected_user"
    Public Const ACTION_SELECTED_USERS As String = "selected_users"
    Public Const ACTION_SELECTED_4ASA_USER As String = "selected_4asa_user"
    Public Const ACTION_4ASA_KEEP As String = "keep_4asa_user"
    Public Const ACTION_4ASA_NEXT_PORTION As String = "next_4asa_users_portion"
    Public Const ACTION_ALTS_FILTER As String = "alts_filter"
    Public Const ACTION_REFRESH_CV As String = "cv_refresh"
    Public Const ACTION_SHOW_ALL_ROWS_CV As String = "cv_show_all_rows"
    Public Const ACTION_FILTER_ROWS_CV As String = "cv_filter_rows"
    Public Const ACTION_CHECKED_NODES_CV As String = "cv_checked_nodes"
    Public Const ACTION_FILTER_BY_CV As String = "cv_filter_by"
    Public Const ACTION_RESET_MIXED_VIEW As String = "reset_mixed_widgets"
    Public Const ACTION_NODE_COLOR_SET As String = "set_node_color"
    Public Const ACTION_NODES_COLORS_RESET As String = "reset_nodes_colors"

#Region "Normalization Options"

    Protected Function GetNormalizationOptions() As String
        Dim sOptions As String = ""
        Dim tNormalizeMode As LocalNormalizationType = PM.Parameters.Normalization
        sOptions += String.Format("<option value='{0}' {2}>{1}</option>", CInt(LocalNormalizationType.ntUnnormalized), "Unnormalized", IIf(tNormalizeMode = LocalNormalizationType.ntUnnormalized, " selected='selected'", ""))
        sOptions += String.Format("<option value='{0}' {2}>{1}</option>", CInt(LocalNormalizationType.ntNormalizedForAll), "Normalized", IIf(tNormalizeMode = LocalNormalizationType.ntNormalizedForAll OrElse ((CurrentPageID = _PGID_ANALYSIS_ASA OrElse CurrentPageID = _PGID_ANALYSIS_4ASA) AndAlso tNormalizeMode <> LocalNormalizationType.ntUnnormalized), " selected='selected'", ""))
        'If CurrentPageID <> _PGID_ANALYSIS_ASA AndAlso CurrentPageID <> _PGID_ANALYSIS_4ASA Then
        'sOptions += String.Format("<option value='{0}' {2}>{1}</option>", CInt(LocalNormalizationType.ntNormalizedSum100), "Normalized for Selected", IIf(tNormalizeMode = LocalNormalizationType.ntNormalizedSum100, " selected='selected'", "")) ' -D6920
        sOptions += String.Format("<option value='{0}' {2}>{1}</option>", CInt(LocalNormalizationType.ntNormalizedMul100), "% of Maximum", IIf(tNormalizeMode = LocalNormalizationType.ntNormalizedMul100, " selected='selected'", ""))
        'End If
        Return String.Format("<select class='select' id='cbNormalizeOptions' style='width:140px;' onchange='cbNormalizeModeChange(this.value);'>{0}</select>", sOptions)
    End Function

#End Region

    Public Sub New()
        MyBase.New(_PGID_SYNTHESIS_RESULTS)
    End Sub

    Private Sub SynthesisPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit  ' D6096
        If Not App.isAuthorized OrElse Not App.HasActiveProject OrElse PM Is Nothing Then FetchAccess()  ' D6599 + D6643
        SynthesisView = SynthesisViews.vtMixed
        If PM IsNot Nothing Then   ' D6643
            Dim sView = CheckVar(_PARAM_VIEW, "")
            Select Case sView 'SynthesisView
                Case "2d"
                    SynthesisView = SynthesisViews.vt2D
                    CurrentPageID = _PGID_ANALYSIS_2D
                Case "altgrid"
                    SynthesisView = SynthesisViews.vtAlternatives
                    CurrentPageID = _PGID_ANALYSIS_OVERALL_ALTS
                Case "cv" ', SynthesisViews.vtTree
                    SynthesisView = SynthesisViews.vtCV
                    CurrentPageID = _PGID_ANALYSIS_CONSENSUS
                Case "dsa"
                    SynthesisView = SynthesisViews.vtDSA
                    CurrentPageID = _PGID_ANALYSIS_DSA
                    Dim sHid As String = CheckVar("hid", "")
                    If sHid = "0" Then
                        CurrentPageID = _PGID_RISK_DSA_LIKELIHOOD
                        PM.ActiveHierarchy = ECHierarchyID.hidLikelihood
                        PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.DoNotUse
                    End If
                    If sHid = "2" Then
                        CurrentPageID = _PGID_RISK_DSA_IMPACT
                        PM.ActiveHierarchy = ECHierarchyID.hidImpact
                        PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.DoNotUse
                    End If
                    Dim sCtrls As String = CheckVar("controls", "")
                    If sCtrls = "1" And CurrentPageID = _PGID_RISK_DSA_LIKELIHOOD Then
                        CurrentPageID = _PGID_RISK_DSA_LIKELIHOOD_WITH_CONTROLS
                        PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.UseOnlyActive
                    End If
                    If sCtrls = "1" And CurrentPageID = _PGID_RISK_DSA_IMPACT Then
                        CurrentPageID = _PGID_RISK_DSA_IMPACT_WITH_CONTROLS
                        PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.UseOnlyActive
                    End If

                    'If CurrentPageID <> _PGID_ANALYSIS_DSA OrElse RiskPageIDs.Contains(CurrentPageID) Then
                    '    PM.Parameters.RiskSensitivityParameter = 1
                    'End If
                Case "gsa"
                    SynthesisView = SynthesisViews.vtGSA
                    CurrentPageID = _PGID_ANALYSIS_GSA  ' D6032                
                Case "h2h"
                    SynthesisView = SynthesisViews.vtHTH
                    CurrentPageID = _PGID_ANALYSIS_HEAD2HEAD
                Case "objgrid"
                    SynthesisView = SynthesisViews.vtObjectives
                    CurrentPageID = _PGID_ANALYSIS_OVERALL_OBJS
                Case "psa"
                    SynthesisView = SynthesisViews.vtPSA
                    CurrentPageID = _PGID_ANALYSIS_PSA
                    Dim sHid As String = CheckVar("hid", "")
                    If sHid = "0" Then
                        CurrentPageID = _PGID_RISK_PSA_LIKELIHOOD
                        PM.ActiveHierarchy = ECHierarchyID.hidLikelihood
                        PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.DoNotUse
                    End If
                    If sHid = "2" Then
                        CurrentPageID = _PGID_RISK_PSA_IMPACT
                        PM.ActiveHierarchy = ECHierarchyID.hidImpact
                        PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.DoNotUse
                    End If
                    Dim sCtrls As String = CheckVar("controls", "")
                    If sCtrls = "1" And CurrentPageID = _PGID_RISK_PSA_LIKELIHOOD Then
                        CurrentPageID = _PGID_RISK_PSA_LIKELIHOOD_WITH_CONTROLS
                        PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.UseOnlyActive
                    End If
                    If sCtrls = "1" And CurrentPageID = _PGID_RISK_PSA_IMPACT Then
                        CurrentPageID = _PGID_RISK_PSA_IMPACT_WITH_CONTROLS
                        PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.UseOnlyActive
                    End If

                    'If CurrentPageID <> _PGID_ANALYSIS_PSA OrElse RiskPageIDs.Contains(CurrentPageID) Then
                    '    PM.Parameters.RiskSensitivityParameter = 1
                    'End If
                Case "asa"
                    SynthesisView = SynthesisViews.vtASA
                    CurrentPageID = _PGID_ANALYSIS_ASA
                Case "4asa"
                    SynthesisView = SynthesisViews.vt4ASA
                    CurrentPageID = _PGID_ANALYSIS_4ASA
                Case "4sa"
                    SynthesisView = SynthesisViews.vtMixed
                    CurrentPageID = _PGID_ANALYSIS_MIXED
                Case "altchart"
                    SynthesisView = SynthesisViews.vtAlternativesChart
                    CurrentPageID = _PGID_ANALYSIS_OVERALL_ALTS_CHART
                Case "objchart"
                    SynthesisView = SynthesisViews.vtObjectivesChart
                    CurrentPageID = _PGID_ANALYSIS_OVERALL_OBJS_CHART
                Case "dashboard"
                    SynthesisView = SynthesisViews.vtDashboard
                    CurrentPageID = _PGID_DASHBOARDS
                    ShowNavigation = False
                    ShowTopNavigation = True
                Case Else 'SynthesisViews.vtMixed
                    CurrentPageID = _PGID_SYNTHESIS_RESULTS
            End Select
        End If
    End Sub

    Public Readonly Property WRTNodeID As Integer
        Get
            Return Api.WRTNodeID(Not IsMixedOrSensitivityView)
        End Get
    End Property

    Public Function GetTitle() As String
        Dim retVal As String = ""        
        Dim wrtNodeName As String = ""
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(WRTNodeID)
        If wrtNode IsNot Nothing AndAlso PM.ActiveObjectives.Nodes(0).NodeID <> WRTNodeID Then wrtNodeName = wrtNode.NodeName
        retVal = If(SynthesisView = SynthesisViews.vtCV, String.Format("{0}<br/><small style='display: inline-block;' id='pgHeader'><span class='title-text-overflow-ellipsis-line'>{1}</span></small>", ShortString(SafeFormString(App.ActiveProject.ProjectName), 85), PageTitle(CurrentPageID)), String.Format("{0}<br/><small style='display: inline-block;'><span class='title-text-overflow-ellipsis-line' id='spanPgTitle'>{1}</span>&nbsp;<span id='lblTitleUserNames' class='title-text-overflow-ellipsis-line'>{2}</span><br/><span id='lblWrtNode'>{3}</span></small>", ShortString(SafeFormString(App.ActiveProject.ProjectName), 85), PageTitle(CurrentPageID), "", If(wrtNodeName <> "", "with respect to " + ShortString(SafeFormString(wrtNodeName), 85), "")))
        Return retVal
    End Function

    Protected Sub Page_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        'If Not isCallback AndAlso Not IsPostBack Then
        '    UserIDsWithData = PM.StorageManager.Reader.DataExistsForUsersHashset(PM.ActiveHierarchy) 'PM.DataExistsForUsers()
        'End If

        'If PM.Parameters.RiskSensitivityParameter <> 0 AndAlso Not RiskPageIDs.Contains(CurrentPageID) Then
        '    PM.Parameters.RiskSensitivityParameter = 0
        '    PM.Parameters.Save()
        'End If

        If isAJAX Then Ajax_Callback(RemoveXssFromUrl(Request.Form.ToString))  ' D6767
        If Not IsPostBack AndAlso Not isCallback Then
            CVFilterBy = CVFilters.cvfBoth
            CVShowAllRows = True
            CVShowTopNRows = -1
        End If
    End Sub

    Public RiskPageIDs() As Integer = {_PGID_RISK_DSA_LIKELIHOOD, _PGID_RISK_DSA_IMPACT, _PGID_RISK_DSA_LIKELIHOOD_WITH_CONTROLS, _PGID_RISK_DSA_IMPACT_WITH_CONTROLS, _PGID_RISK_PSA_LIKELIHOOD, _PGID_RISK_PSA_IMPACT, _PGID_RISK_PSA_LIKELIHOOD_WITH_CONTROLS, _PGID_RISK_PSA_IMPACT_WITH_CONTROLS}

    Public ReadOnly Property RiskParam As Integer
        Get
            Return If (CurrentPageID = _PGID_ANALYSIS_DSA OrElse CurrentPageID = _PGID_ANALYSIS_PSA, ProjectManager.Parameters.RiskSensitivityParameter, If(RiskPageIDs.Contains(CurrentPageID), 1, 0))
        End Get
    End Property

    Public Property selectedLayout As Integer
        Get
            Dim sOp As String = CStr(PM.Attributes.GetAttributeValue(If(SynthesisView = SynthesisViews.vtDashboard, ATTRIBUTE_DASHBOARD_LAYOUT_ID, ATTRIBUTE_SYNTHESIS_LAYOUT_ID), UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(sOp) Then
                Dim args As NameValueCollection = HttpUtility.ParseQueryString(sOp)
                selectedWidgets = ""
                For k As Integer = 0 To maxWidgets - 1
                    Dim w As String = GetParam(args, "widget" + k.ToString, True)
                    Dim selWidget As Integer = -1
                    If Not String.IsNullOrEmpty(w) Then Integer.TryParse(w, selWidget)
                    selectedWidgets += CStr(IIf(selectedWidgets = "", "", ",")) + selWidget.ToString
                Next
                selectedWidgets = String.Format("[{0}]", selectedWidgets)
                Dim param = GetParam(args, "layout", True)
                Dim i As Integer = DefaultLayout
                If Not String.IsNullOrEmpty(param) AndAlso Integer.TryParse(param, i) Then
                    Return i
                End If
            End If
            Return DefaultLayout
        End Get
        Set(value As Integer)
            Dim sValue As String = String.Format("layout={0}", value)
            Dim sOp As String = CStr(PM.Attributes.GetAttributeValue(If(SynthesisView = SynthesisViews.vtDashboard, ATTRIBUTE_DASHBOARD_LAYOUT_ID, ATTRIBUTE_SYNTHESIS_LAYOUT_ID), UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(sOp) Then
                Dim args As NameValueCollection = HttpUtility.ParseQueryString(sOp)
                args.Set("layout", value.ToString)
                sValue = ""
                For Each key As String In args.Keys
                    sValue += CStr(IIf(sValue = "", "", "&")) + String.Format("{0}={1}", key, args.Item(key).ToString)
                Next
            End If
            WriteSetting(PRJ, If(SynthesisView = SynthesisViews.vtDashboard, ATTRIBUTE_DASHBOARD_LAYOUT_ID, ATTRIBUTE_SYNTHESIS_LAYOUT_ID), AttributeValueTypes.avtString, sValue)
        End Set
    End Property

    Private _selectedWidgets As String = DefaultWidgets
    Public Property selectedWidgets() As String
        Get
            Return _selectedWidgets
        End Get
        Set(value As String)
            _selectedWidgets = value
        End Set
    End Property

    ''' <summary>
    ''' 1-N - show top 1-N alternatives
    ''' -1  - Show All alternatives
    ''' -2  - Advanced
    ''' -3  - Select/deselect alternatives
    ''' -4  - Filter by funded alternatives in RA scenario
    ''' -5  - Filter by alternative attributes
    ''' -6  - Show risks only
    ''' -5  - Show opportunities only
    ''' </summary>
    ''' <returns></returns>
    Public Property AlternativesFilterValue As Integer
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_ALTS_FILTER_ID, UNDEFINED_USER_ID))
            If retVal = -5 AndAlso Not ActiveProjectHasAlternativeAttributes Then retVal = -1
            'If Not IsMixedOrAltsView() AndAlso Not IsConsensusView() AndAlso retVal < -1 Then retVal = -1
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_ALTS_FILTER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property AlternativesAdvancedFilterValue As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property AlternativesAdvancedFilterUserID As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_UID_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_UID_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Private Property IsAltSelected(AltGUID As Guid) As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RESULTS_ALTERNATIVE_IS_SELECTED_ID, AltGUID, Guid.Empty))
        End Get
        Set(value As Boolean)
            With PM
                .Attributes.SetAttributeValue(ATTRIBUTE_RESULTS_ALTERNATIVE_IS_SELECTED_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, AltGUID, Guid.Empty)
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End Set
    End Property

    Private ReadOnly Property IsAltFunded(AltGUID As Guid) As Boolean
        Get
            Return PM.ResourceAligner.Scenarios.ActiveScenario.GetAvailableAlternativeById(AltGUID.ToString) IsNot Nothing AndAlso CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID, AltGUID, Guid.Empty)) > 0
        End Get
    End Property

    Public SETTING_SHOW_HEADER_TOTAL_USERS_NAME As String = "SynthesisShowTotalUsersInHeader"
    Public SETTING_SHOW_HEADER_TOTAL_USERS_DEFAULT_VALUE As Boolean = False

    Public SETTING_SHOW_HEADER_USERS_WITH_JUDGMENTS_NAME As String = "SynthesisShowUsersWithJudgmentsInHeader"
    Public SETTING_SHOW_HEADER_USERS_WITH_JUDGMENTS_DEFAULT_VALUE As Boolean = True

    Public SETTING_SHOW_HEADER_USERS_WITH_COMPLETED_JUDGMENTS_NAME As String = "SynthesisShowUsersWithCompletedJudgmentsInHeader"
    Public SETTING_SHOW_HEADER_USERS_WITH_COMPLETED_JUDGMENTS_DEFAULT_VALUE As Boolean = False

    Private _SynthesisSettings As Dictionary(Of String, String) = Nothing
    Public ReadOnly Property SynthesisSettings As Dictionary(Of String, String)
        Get
            If _SynthesisSettings Is Nothing Then
                _SynthesisSettings = ReadProjectSettings(PRJ)
            End If
            Return _SynthesisSettings
        End Get
    End Property

    Public Property ProjectBooleanSetting(SettingName As String, DefaultValue As Boolean) As Boolean
        Get
            If SynthesisSettings IsNot Nothing AndAlso SynthesisSettings.ContainsKey(SettingName) Then
                Return SynthesisSettings(SettingName) = "1"
            End If
            Return DefaultValue
        End Get
        Set(value As Boolean)
            WriteProjectSetting(SettingName, If(value, "1", "0"))
        End Set
    End Property

    Public Function IsConsensusView() As Boolean
        Return SynthesisView = SynthesisViews.vtCV
    End Function

    Public Function IsMixedOrGridView() As Boolean
        Return SynthesisView = SynthesisViews.vtMixed OrElse SynthesisView = SynthesisViews.vtDashboard OrElse SynthesisView = SynthesisViews.vtAlternatives OrElse SynthesisView = SynthesisViews.vtObjectives
    End Function

    Public Function IsMixedOrAltsView() As Boolean
        Return SynthesisView = SynthesisViews.vtMixed OrElse SynthesisView = SynthesisViews.vtDashboard OrElse SynthesisView = SynthesisViews.vtAlternatives OrElse SynthesisView = SynthesisViews.vtAlternativesChart
    End Function

    Public Function IsSensitivityView() As Boolean
        Return SynthesisView = SynthesisViews.vtDSA OrElse SynthesisView = SynthesisViews.vtPSA OrElse SynthesisView = SynthesisViews.vtGSA OrElse SynthesisView = SynthesisViews.vtHTH OrElse SynthesisView = SynthesisViews.vt2D OrElse SynthesisView = SynthesisViews.vtASA OrElse SynthesisView = SynthesisViews.vt4ASA
    End Function

    Public Function IsMixedOrSensitivityView() As Boolean
        Return SynthesisView = SynthesisViews.vtMixed OrElse SynthesisView = SynthesisViews.vtDashboard OrElse IsSensitivityView()
    End Function

    'ReadOnly Property SESSION_USERS_WITH_DATA_LIST_ID As String
    '    Get
    '        Return String.Format("Synthesis_UsersWithDataList_{0}", App.ProjectID)
    '    End Get
    'End Property

    'Private _UserIDsWithData As HashSet(Of Integer)
    'Private Property UserIDsWithData As HashSet(Of Integer)
    '    Get
    '        If _UserIDsWithData Is Nothing Then
    '            Dim tSess As Object = Session(SESSION_USERS_WITH_DATA_LIST_ID)
    '            If tSess Is Nothing Then
    '                _UserIDsWithData = Nothing
    '            Else
    '                _UserIDsWithData = CType(tSess, HashSet(Of Integer))
    '            End If
    '        End If
    '        Return _UserIDsWithData
    '    End Get
    '    Set(value As HashSet(Of Integer))
    '        Session(SESSION_USERS_WITH_DATA_LIST_ID) = value
    '    End Set
    'End Property

    Private Function ReadProjectSettings(tPrjCore As ExpertChoice.Data.clsProject) As Dictionary(Of String, String)
        Dim SynthesisSettings As Dictionary(Of String, String) = Nothing
        Dim tSettingsAttr As Object = tPrjCore.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_SETTINGS_ID, ECCore.UNDEFINED_USER_ID)
        If tSettingsAttr IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(tSettingsAttr)) Then
            Dim NameDelimiter = New String() {"name:="}
            Dim ValueDelimiter = New String() {"value:="}
            Dim EndChar = New Char() {CChar(";")}
            Dim opts() As String = CStr(tSettingsAttr).Split(NameDelimiter, StringSplitOptions.None)
            If opts.Length > 0 Then
                SynthesisSettings = New Dictionary(Of String, String)
                For Each opt As String In opts
                    Dim vals() As String = opt.Split(ValueDelimiter, StringSplitOptions.None)
                    If vals.Length >= 2 Then
                        Dim key As String = vals(0).TrimEnd(EndChar)
                        If Not SynthesisSettings.ContainsKey(key) Then SynthesisSettings.Add(key, vals(1).TrimEnd(EndChar))
                    End If
                Next
            End If
        End If
        Return SynthesisSettings
    End Function

    Private Function WriteProjectSetting(Name As String, Value As String) As Boolean
        Dim fChanged As Boolean = False
        Dim SynthesisSettings = ReadProjectSettings(PRJ)
        If SynthesisSettings IsNot Nothing Then
            If SynthesisSettings.ContainsKey(Name) Then
                SynthesisSettings(Name) = Value
            Else
                SynthesisSettings.Add(Name, Value)
            End If
            Dim s As String = ""
            For Each kvp As KeyValuePair(Of String, String) In SynthesisSettings
                s += String.Format("name:={0};value:={1};", kvp.Key, kvp.Value)
            Next
            PM.Attributes.SetAttributeValue(ATTRIBUTE_SYNTHESIS_SETTINGS_ID, ECCore.UNDEFINED_USER_ID, AttributeValueTypes.avtString, s, Guid.Empty, Guid.Empty)
            fChanged = True
        Else
            PM.Attributes.SetAttributeValue(ATTRIBUTE_SYNTHESIS_SETTINGS_ID, ECCore.UNDEFINED_USER_ID, AttributeValueTypes.avtString, String.Format("name:={0};value:={1};", Name, Value), Guid.Empty, Guid.Empty)
            fChanged = True
        End If

        If fChanged Then
            With PM
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End If

        Return fChanged
    End Function

    'Private Function IsGroupHasData(grp As clsCombinedGroup) As Boolean
    '    Dim retVal As Boolean = False
    '    For Each user As clsUser In grp.UsersList
    '        If UserIDsWithData.Contains(user.UserID) Then Return True
    '    Next
    '    Return retVal
    'End Function
    
    Private _Api As DashboardWebAPI = Nothing
    Public ReadOnly Property Api As DashboardWebAPI
        Get
            If _Api Is Nothing Then _Api = New DashboardWebAPI
            Return _Api
        End Get
    End Property

    Public Function GetNodesData() As String
        Dim retVal As String = ""
        Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
        For Each t As Tuple(Of Integer, Integer, clsNode) In H.NodesInLinearOrder
            Dim parentGuids As String = ""
            Dim parent As clsNode = H.GetNodeByID(t.Item2)
            If parent IsNot Nothing Then parentGuids = String.Format("""{0}""", parent.NodeGuidID.ToString)
            If t.Item3.ParentNodesGuids IsNot Nothing Then
                For Each id As Guid In t.Item3.ParentNodesGuids
                    Dim sId As String = id.ToString
                    If parentGuids.IndexOf(sId) < 0 Then
                        parentGuids += CStr(IIf(parentGuids = "", "", ",")) + String.Format("""{0}""", sId)
                    End If
                Next
            End If
            parentGuids = String.Format("[{0}]", parentGuids)

            Dim AttrGuid As Guid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID
            If PM.ActiveHierarchy = ECHierarchyID.hidImpact Then
                AttrGuid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID
            End If
            Dim sNodeColor As String = ""
            Dim tNodeColor As Long = CLng(PM.Attributes.GetAttributeValue(AttrGuid, t.Item3.NodeGuidID))
            If tNodeColor > 0 Then
                sNodeColor = LongToBrush(tNodeColor)
            End If
            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("[{0},""{1}"",""{2}"",{3},{4},{5},{6},""{7}"",{8}]", t.Item1.ToString, t.Item3.NodeGuidID.ToString, JS_SafeString(t.Item3.NodeName), parentGuids, t.Item2, If(t.Item3.IsTerminalNode, "1", "0"), t.Item3.Level, sNodeColor, Bool2JS(t.Item3.RiskNodeType = RiskNodeType.ntCategory))
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetPrioritiesWording() As String
        Dim retVal As String = "priorities"
        If PM.IsRiskProject Then
            If CurrentPageID = _PGID_RISK_DSA_LIKELIHOOD OrElse CurrentPageID = _PGID_RISK_DSA_LIKELIHOOD_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PSA_LIKELIHOOD OrElse CurrentPageID = _PGID_RISK_PSA_LIKELIHOOD_WITH_CONTROLS OrElse
                CurrentPageID = _PGID_RISK_DSA_IMPACT OrElse CurrentPageID = _PGID_RISK_DSA_IMPACT_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PSA_IMPACT OrElse CurrentPageID = _PGID_RISK_PSA_IMPACT_WITH_CONTROLS Then
                retVal = ParseString("%%risks%%")
            Else
                retVal = If (PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("%%likelihoods%%"), ParseString("%%impacts%%"))
            End If
        End If
        Return retVal
    End Function
    
    Function GetUsersDropdown() As String
        Dim sRes As String = ""
        For Each grp As clsCombinedGroup In PM.CombinedGroups.GroupsList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", grp.CombinedUserID, SafeFormString(grp.Name), CStr(IIf(grp.CombinedUserID = COMBINED_USER_ID, " selected='selected' ", "")))
        Next
        sRes += "<option disabled='disabled'>---------------------</option>"
        For Each user As clsUser In PM.UsersList
            sRes += String.Format("<option value='{0}'>{1}</option>", user.UserID, SafeFormString(user.UserName))
        Next
        Return String.Format("<select class='select' id='cbAdvUsers' style='width:150px;'>{0}</select>", sRes)
    End Function

    Function GetAltsNumDropdown() As String
        Dim sRes As String = ""
        Dim AH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        If AH IsNot Nothing Then
            For i As Integer = 0 To AH.TerminalNodes.Count
                sRes += String.Format("<option value='{0}' {2}>{1}</option>", i, i, CStr(If((AlternativesFilterValue = -2 AndAlso i = AlternativesAdvancedFilterValue) OrElse (AlternativesFilterValue <> -2 AndAlso i = AH.TerminalNodes.Count), " selected='selected' ", "")))
            Next
        End If
        Return String.Format("<select class='select' id='cbAdvAltsNum' style='width:150px;'>{0}</select>", sRes)
    End Function

    Function GetAltsCheckList() As String
        Dim sRes As String = ""
        Dim AH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        If AH IsNot Nothing Then
            For Each alt As clsNode In AH.TerminalNodes
                sRes += String.Format("<label><input type='checkbox' class='cust_alt_cb' value='{0}' {1}/>{2}</label><br/>", alt.NodeID, CStr(IIf(IsAltSelected(alt.NodeGuidID), " checked='checked' ", " ")), SafeFormString(alt.NodeName)) 'A1120
            Next
        End If
        Return sRes
    End Function

#Region "SA"
    'Data example for sa plugin
    'alts: [{ id: 0, name: 'Alt1', value: 0.7, initValue: 0.7, color: '#95c5f0' },
    '       { id: 1, name: 'Alt2', value: 0.3, initValue: 0.3, color: '#fa7000'}],
    'objs: [{ id: 1, name: 'Obj1', value: 0.3, initValue: 0.3, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.5}], gradientMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}], gradientInitMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}] },
    '   { id: 2, name: 'Obj2', value: 0.5, initValue: 0.5, gradientMaxValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.5}], gradientMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7}], gradientInitMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7}] },
    '   { id: 3, name: 'Obj3', value: 0.2, initValue: 0.2, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.3}], gradientMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1}], gradientInitMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1}]}]
    '-A1225 Dim AltColors As String() = {"#344d94", "#cb3034", "#9d27a8", "#e3e112", "#00687d", "#407000", "#f24961", "#663d2e", "#9600fa", "#ffbde6", "#00c49f", "#7280c4", "#009180", "#e33000", "#80bdff", "#a10040", "#0affe3", "#00523c", "#919100", "#5c00f7", "#a15f00", "#cce6ff", "#00465c", "#adff69", "#f24ba0", "#0dff87", "#ff8c47", "#349400", "#b3b3a1", "#a10067", "#ba544a", "#edc2d1", "#00e8c3", "#3f0073", "#5ec1f7", "#6e00b8", "#f5f5c4", "#e33000", "#52ba00", "#ff943b", "#0079db", "#f0e6c0", "#ffb517", "#cf0076", "#e8cfc9"}

    Dim ObjPriorities As New Dictionary(Of Integer, Double)
    Dim AltValues As New Dictionary(Of Integer, Double)
    Dim AltValuesInOne As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
    Dim AltValuesInZero As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

    Dim ChartObjectives As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
    Dim ChartAlternatives As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

    Public Property SAUserID As Integer
        Get
            Dim tUserID As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_SA_USER_ID, UNDEFINED_USER_ID))
            If PM.UserExists(tUserID) OrElse PM.CombinedGroups.CombinedGroupExists(tUserID) Then Return tUserID
            Return COMBINED_USER_ID
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_SA_USER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property SelectedUsersAndGroupsIDs As List(Of Integer)
        Get
            Dim sUsers As String = StringSelectedUsersAndGroupsIDs
            If String.IsNullOrEmpty(sUsers) Then Return Nothing
            Dim tList As String() = sUsers.Split(CChar(","))
            If tList.Count > 0 Then
                Dim res As New List(Of Integer)
                For Each item In tList
                    Dim tUserID As Integer
                    If Integer.TryParse(item, tUserID) AndAlso (PM.UserExists(tUserID) OrElse PM.CombinedGroups.CombinedGroupExists(tUserID)) Then
                        res.Add(tUserID)
                    End If
                Next
                If res.Count = 0 Then res.Add(COMBINED_USER_ID)
                Return res
            End If
            Dim auto = New List(Of Integer)
            auto.Add(COMBINED_USER_ID)
            Return auto
        End Get
        Set(value As List(Of Integer))
            Dim sUsers As String = ""
            For i As Integer = 0 To value.Count - 1
                sUsers = sUsers + value(i).ToString
                If i <> value.Count - 1 Then sUsers += ","
            Next
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_USERS_ID, AttributeValueTypes.avtString, sUsers)
        End Set
    End Property

    Public Property StringSelectedUsersAndGroupsIDs As String
        Get
            Dim value As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_USERS_ID, UNDEFINED_USER_ID))
            If String.IsNullOrEmpty(value) OrElse value = UNDEFINED_USER_ID.ToString Then value = COMBINED_USER_ID.ToString
            Return value
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) OrElse value = UNDEFINED_USER_ID.ToString Then value = COMBINED_USER_ID.ToString
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_USERS_ID, AttributeValueTypes.avtString, value)
        End Set
    End Property

    Private _allList As List(Of Integer) = Nothing
    Public Property allList As List(Of Integer)
        Get
            If _allList Is Nothing Then
                _allList = New List(Of Integer)
                If Api.IsGroupHasData(PM.CombinedGroups.GetGroupByCombinedID(COMBINED_USER_ID)) Then _allList.Add(COMBINED_USER_ID)
                For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
                    If group.CombinedUserID <> COMBINED_USER_ID AndAlso Api.IsGroupHasData(PM.CombinedGroups.GetGroupByCombinedID(group.CombinedUserID))Then _allList.Add(group.CombinedUserID)
                Next
                For Each user As clsUser In PM.UsersList
                    If Api.UserIDsWithData.Contains(user.UserID) Then _allList.Add(user.UserID)
                Next
            End If
            Return _allList
        End Get
        Set(value As List(Of Integer))
            _allList = value
        End Set
    End Property

    Private ASAPane As Guid() = {New Guid("{33FDAFBF-8977-4AC5-973D-C48D17E314E9}"), New Guid("{A38E4A86-C3E6-459D-8D7B-75F3D0D02F22}"), New Guid("{150330A4-2C8C-4FE8-83CC-441940A609EE}"), New Guid("{A37F70DE-8932-43F8-853D-F7EC8F9A6FFB}")}
    Public Property Selected4ASAUser(wid As Integer) As Integer
        Get
            If SynthesisView <> SynthesisViews.vt4ASA Then Return UNDEFINED_INTEGER_VALUE
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_4ASA_USERS_ID, ASAPane(wid)))
            If retVal = UNDEFINED_INTEGER_VALUE Then
                If allList.Count > wid Then retVal = allList(wid)
            Else
                If Not PM.CombinedGroups.CombinedGroupUserIDExists(retVal) AndAlso Not PM.UserExists(retVal) Then retVal = UNDEFINED_USER_ID
            End If
            Return retVal
        End Get
        Set(value As Integer)
            PRJ.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_SYNTHESIS_4ASA_USERS_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, ASAPane(wid), Guid.Empty)
            WriteAttributeValues(PRJ, "", "")   ' D3731
        End Set
    End Property

    Public ReadOnly Property Keep4ASAUser(wid As Integer) As Boolean
        Get
            If SynthesisView <> SynthesisViews.vt4ASA Then Return False
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_4ASA_USERS_ID, ASAPane(wid)))
            Return retVal <> UNDEFINED_INTEGER_VALUE AndAlso (PM.CombinedGroups.CombinedGroupUserIDExists(retVal) OrElse PM.UserExists(retVal))
        End Get
    End Property    

    Public Sub initSAData()
        If ObjPriorities.Count = 0 Then
            Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
            If H IsNot Nothing Then
                Dim wrtNode As clsNode = H.GetNodeByID(WRTNodeID)
                If wrtNode IsNot Nothing Then
                    Dim isWRTGoal = (Api.WRTState = ECWRTStates.wsGoal)
                    ObjPriorities.Clear()
                    AltValues.Clear()
                    AltValuesInOne.Clear()
                    PM.CalculationsManager.UseNormalizationForSA = PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized
                    PM.CalculationsManager.HierarchyID = H.HierarchyID
                    PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(SAUserID)) 'A1106 
                    PM.CalculationsManager.InitializeSAGradient(wrtNode.NodeID, isWRTGoal, SAUserID, ObjPriorities, AltValues, AltValuesInOne, RiskParam)
                    AltValuesInZero = PM.CalculationsManager.GetGradientData(wrtNode.NodeID, isWRTGoal, SAUserID, ObjPriorities, RiskParam)
                End If
            End If
        End If
    End Sub
   
    Public Sub updateAltValuesinZero(NewObjPriorities As Dictionary(Of Integer, Double))
        Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
        If H IsNot Nothing Then
            Dim wrtNode As clsNode = H.GetNodeByID(WRTNodeID)
            If wrtNode IsNot Nothing Then
                Dim isWRTGoal = (Api.WRTState = ECWRTStates.wsGoal)
                AltValuesInZero = PM.CalculationsManager.GetGradientData(wrtNode.NodeID, isWRTGoal, SAUserID, NewObjPriorities, 0)
            End If
        End If
    End Sub

    Private Function isDataRequired() As Boolean
        Return Not ignoreCallback OrElse (Not isCallback AndAlso Not IsPostBack)
    End Function

    Public Function GetSAComponentsData() As String
        Dim retVal As String = ""
        If IsMixedOrSensitivityView() AndAlso isDataRequired() Then
            Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
            Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            initSAData()
            Dim ComponentsData As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = PM.CalculationsManager.GetDSAComponents(WRTNodeID, , SAUserID)
            If ComponentsData.Count > 0 And ObjPriorities.Count > 0 And altH IsNot Nothing And H IsNot Nothing Then
                Dim wrtNode As clsNode = H.GetNodeByID(WRTNodeID)
                If wrtNode IsNot Nothing Then
                    For Each obj As clsNode In wrtNode.Children
                        For Each alt As clsNode In altH.TerminalNodes
                            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""altID"":{0},""objID"":{1},""comp"":{2}}}", alt.NodeID, obj.NodeID, JS_SafeNumber(ComponentsData(alt.NodeID)(obj.NodeID)))
                        Next
                    Next
                End If
            End If
        End If
        Return String.Format("[{0}]", retVal)
    End Function
   
    Public Function GetSAObjectives() As String
        Dim retVal As String = ""
        If IsMixedOrSensitivityView() AndAlso isDataRequired() Then ' D4243
            Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
            Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            initSAData()
            If ObjPriorities.Count > 0 And altH IsNot Nothing And H IsNot Nothing Then
                Dim wrtNode As clsNode = H.GetNodeByID(WRTNodeID)

                If wrtNode IsNot Nothing Then
                    Dim objs2D As New Dictionary(Of Integer, NodePriority)
                    Dim alts2D As New Dictionary(Of Integer, List(Of NodePriority))
                    PM.CalculationsManager.GetSA2DData(WRTNodeID, SAUserID, objs2D, alts2D)
                    Dim i = 0
                    For Each obj As clsNode In wrtNode.Children
                        If (obj.RiskNodeType <> RiskNodeType.ntCategory) Then
                            Dim AttrGuid As Guid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID
                            If PM.ActiveHierarchy = ECHierarchyID.hidImpact Then
                                AttrGuid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID
                            End If
                            Dim sNodeColor As String = ""
                            Dim tNodeColor As Long = CLng(PM.Attributes.GetAttributeValue(AttrGuid, obj.NodeGuidID))
                            sNodeColor = If(tNodeColor > 0, LongToBrush(tNodeColor), GetPaletteColor(CurrentPaletteID(PM), obj.NodeID)) 'i

                            Dim gradientMaxValues As String = ""
                            Dim gradientMinValues As String = ""
                            Dim values2D As New List(Of NodePriority)
                            values2D = alts2D(obj.NodeID)
                            Dim values2DString = ""
                            If AltValuesInOne.ContainsKey(obj.NodeID) Then 'A1337
                                For Each altID In AltValuesInOne(obj.NodeID).Keys
                                    gradientMaxValues += CStr(IIf(gradientMaxValues <> "", ",", "")) + String.Format("{{""altID"":{0},""val"":{1}}}", altID, JS_SafeNumber(AltValuesInOne(obj.NodeID)(altID)))
                                Next
                            End If
                            If AltValuesInZero.ContainsKey(obj.NodeID) Then 'A1337
                                For Each altID In AltValuesInZero(obj.NodeID).Keys
                                    gradientMinValues += CStr(IIf(gradientMinValues <> "", ",", "")) + String.Format("{{""altID"":{0},""val"":{1}}}", altID, JS_SafeNumber(AltValuesInZero(obj.NodeID)(altID)))
                                Next
                            End If
                            For Each altVal In values2D
                                values2DString += CStr(IIf(values2DString <> "", ",", "")) + String.Format("{{""altID"":{0},""val"":{1},""valNorm"":{2}}}", altVal.NodeID, JS_SafeNumber(altVal.GlobalPriority), JS_SafeNumber(altVal.NormalizedGlobalPriority))
                            Next
                            If values2DString = "" Then values2DString = "{}"
                            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":{0},""idx"":{1},""name"":""{2}"",""value"":{3},""initValue"":{4},""gradientMaxValues"":[{5}],""gradientMinValues"":[{6}],""gradientInitMinValues"":[{7}],""values2D"":[{8}],""color"":""{9}"", ""visible"":1, ""isCategory"":{10}}}", obj.NodeID, i, JS_SafeString(obj.NodeName), JS_SafeNumber(ObjPriorities(obj.NodeID)), JS_SafeNumber(ObjPriorities(obj.NodeID)), gradientMaxValues, gradientMinValues, gradientMinValues, values2DString, sNodeColor, Bool2JS(obj.RiskNodeType = RiskNodeType.ntCategory))
                            i += 1
                        End If
                    Next
                End If
            End If
        End If
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetSAAlternatives() As String
        Dim retVal As String = ""
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        If (IsMixedOrSensitivityView() OrElse SynthesisView = SynthesisViews.vtAlternativesChart OrElse SynthesisView = SynthesisViews.vtObjectivesChart) AndAlso isDataRequired() Then  ' D4243
            'initSAData()
            If AltValues.Count > 0 And altH IsNot Nothing Then
                Dim tVisibleAlternatives As Dictionary(Of Integer, Boolean) = Api.GetVisibleAlternatives() 'A1513
                '-FB17507
                'Dim NormCoef As Double = 1
                'If (NormalizeMode <> LocalNormalizationType.ntUnnormalized) Then
                '    Dim prtySum As Double = 0
                '    For Each alt As clsNode In altH.TerminalNodes
                '        Dim isVisible As Boolean = tVisibleAlternatives.ContainsKey(alt.NodeID) AndAlso tVisibleAlternatives(alt.NodeID)
                '        Dim AltVal = AltValues(alt.NodeID)
                '        If NormalizeMode = LocalNormalizationType.ntNormalizedSum100 And isVisible Then
                '            prtySum += AltVal
                '        End If
                '        If NormalizeMode = LocalNormalizationType.ntNormalizedForAll Then
                '            prtySum += AltVal
                '        End If
                '        'If NormalizeMode = LocalNormalizationType.ntNormalizedMul100 Then
                '        '    If prtySum < AltVal Then
                '        '        prtySum = AltVal
                '        '    End If
                '        'End If
                '    Next
                '    If prtySum > 0 Then NormCoef = 1 / prtySum
                'End If
                '-FB17507
                Dim i = 0
                For Each alt As clsNode In altH.TerminalNodes
                    Dim isVisible As Boolean = tVisibleAlternatives.ContainsKey(alt.NodeID) AndAlso tVisibleAlternatives(alt.NodeID) 'A1513
                    Dim sAltColor As String = ""
                    Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                    sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), alt.NodeID, True)) 'i
                    Dim AltVal As Double = AltValues(alt.NodeID) '-FB17507 * NormCoef
                    retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":{0},""idx"":{1},""name"":""{2}"",""value"":{3},""initValue"":{4},""color"":""{5}"",""visible"":{6},""event_type"":{7}}}", alt.NodeID, i, JS_SafeString(alt.NodeName), JS_SafeNumber(AltVal), JS_SafeNumber(AltVal), sAltColor, IIf(isVisible, "1", "0"), CInt(alt.EventType))
                    i += 1
                Next
            End If
        End If

        Return String.Format("[{0}]", retVal)
    End Function    

    Function getASADataForUserGroupID(UserGroupID As Integer) As String
        Dim retVal As String = ""
        Dim altResults As String = ""
        Dim maxAltValue As Double = 0
        Dim objH As clsHierarchy = PM.ActiveObjectives
        Dim altH As clsHierarchy = PM.ActiveAlternatives

        If objH IsNot Nothing Then
            Dim asaData As ASAData = PM.CalculationsManager.GetASAData(UserGroupID)
            maxAltValue = If(PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100, asaData.MaxAlternativePriorityNormalized, asaData.MaxAlternativePriorityUnnormalized)
            Dim tVisibleAlternatives As Dictionary(Of Integer, Boolean) = Api.GetVisibleAlternatives() 'A1513            

            Dim CalcTarget As clsCalculationTarget
            If IsCombinedUserID(UserGroupID) Then
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(UserGroupID))
            Else
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, PM.GetUserByID(UserGroupID))
                PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(UserGroupID))
            End If

            PM.CalculationsManager.Calculate(CalcTarget, objH.Nodes(0))
            Dim i = 0
            For Each alt As clsNode In altH.TerminalNodes
                Dim altVals As String = ""
                For Each node As clsNode In objH.TerminalNodes
                    Dim tValue As Tuple(Of Double, Double) = asaData.AlternativesPrioritiesByObjective(node.NodeID)(alt.NodeID)
                    Dim Value As Double = If(PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100, tValue.Item1, tValue.Item2)
                    altVals += CStr(IIf(altVals <> "", ",", "")) + JS_SafeNumber(Value)

                Next
                altVals = String.Format("[{0}]", altVals)
                Dim isVisible As Boolean = tVisibleAlternatives.ContainsKey(alt.NodeID) AndAlso tVisibleAlternatives(alt.NodeID) 'A1513
                Dim sAltColor As String = ""
                Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), alt.NodeID, True)) 'i
                Dim tGValue As Tuple(Of Double, Double) = asaData.AlternativesPriorities(alt.NodeID)
                Dim altGlobalValue As Double = If(PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100, tGValue.Item1, tGValue.Item2)
                altResults += CStr(IIf(altResults <> "", ",", "")) + String.Format("{{""id"": {0}, ""idx"": {1}, ""name"":""{2}"", ""initVals"": {3}, ""newVals"": [], ""oldVals"":[], ""value"": {4}, ""initValue"": {4}, ""color"": ""{5}"", ""visible"": {6},""event_type"":{7}}}", JS_SafeNumber(alt.NodeID), JS_SafeNumber(i), JS_SafeString(alt.NodeName), altVals, JS_SafeNumber(altGlobalValue), JS_SafeString(sAltColor), IIf(isVisible, "1", "0"), CInt(alt.EventType))
                i += 1
            Next
            'PM.CalculationsManager.Calculate(CalcTarget, objH.Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

            Dim CheckedNodes As String() = Nothing
            If CVCheckedNodes IsNot Nothing Then CheckedNodes = CVCheckedNodes.Split(CChar(","))

            i = 0
            For Each obj As clsNode In objH.TerminalNodes
                Dim isVisible As Integer = 1
                If CheckedNodes IsNot Nothing AndAlso Not CheckedNodes.Contains(CStr(obj.NodeID)) Then isVisible = 0
                Dim tObjGValue As Tuple(Of Double, Double) = asaData.ObjectivesPriorities(obj.NodeID)
                Dim objGlobalValue As Double = If(PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100, tObjGValue.Item1, tObjGValue.Item2)
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""idx"":{4}, ""id"": {0}, ""name"": ""{1}"", ""prty"": {2}, ""visible"": {3}}}", obj.NodeID, JS_SafeString(obj.NodeName), JS_SafeNumber(objGlobalValue), isVisible, i)
                i += 1
            Next
        End If
        If maxAltValue = 0 Then maxAltValue = 1
        Return String.Format("{{""objectives"": [{0}], ""alternatives"": [{1}], ""maxAltValue"": {2}, ""userID"": {3}}}", retVal, altResults, JS_SafeNumber(maxAltValue), UserGroupID)
    End Function

    Public Function GetASAData() As String

        If SynthesisView = SynthesisViews.vt4ASA Then
            Dim l As New List(Of Integer)
            l.Add(Selected4ASAUser(0))
            l.Add(Selected4ASAUser(1))
            l.Add(Selected4ASAUser(2))
            l.Add(Selected4ASAUser(3))
            Return Get4ASAData(l) 'A1395

            '-A1395 Dim aResult = ""
            'For i As Integer = 0 To 3
            '    Dim UserID As Integer = Selected4ASAUser(i)

            '    If UserID = UNDEFINED_INTEGER_VALUE Then
            '        aResult += CStr(IIf(aResult <> "", ",", "")) + "{}"
            '    Else
            '        aResult += CStr(IIf(aResult <> "", ",", "")) + getASADataForUserGroupID(UserID)
            '    End If
            'Next
            'Return String.Format("[{0}]", aResult)
        End If

        If SynthesisView = SynthesisViews.vtASA OrElse SynthesisView = SynthesisViews.vtMixed OrElse SynthesisView = SynthesisViews.vtDashboard Then
            Return getASADataForUserGroupID(SAUserID)
        End If
        Return "{}"
    End Function

    'A1395 ===
    Public Function Get4ASAData(uids As List(Of Integer)) As String
        Dim retVal = ""
        If SynthesisView = SynthesisViews.vt4ASA Then
            For i As Integer = 0 To 3
                Dim UserID As Integer = uids(i)

                If UserID = UNDEFINED_INTEGER_VALUE Then
                    retVal += CStr(IIf(retVal <> "", ",", "")) + "{}"
                Else
                    retVal += CStr(IIf(retVal <> "", ",", "")) + getASADataForUserGroupID(UserID)
                End If
            Next
            retVal = String.Format("[{0}]", retVal)
        End If

        If retVal = "" Then retVal = "{}"
        Return retVal
    End Function
    'A1395 ==

    'A1022 ===
    Public Function GetSAParticipants() As String
        Dim sRes As String = ""
        For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
            group.ApplyRules()
            sRes += String.Format("<option value='{0}' {2}>[{1}]</option>", group.CombinedUserID, SafeFormString(group.Name), IIf(group.CombinedUserID = SAUserID, " selected='selected' ", ""))
        Next
        sRes += "<option disabled='disabled'>-----------------------------</option>"
        For Each user As clsUser In PM.UsersList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", user.UserID, SafeFormString(CStr(IIf(user.UserName = "", user.UserEMail, user.UserName))), IIf(user.UserID = SAUserID, " selected='selected' ", ""))
        Next
        Return String.Format("<select class='select' style='width:120px;' onchange='displayLoadingPanel(true); sendCommand(""action=" + ACTION_SELECTED_USER + "&id=""+this.value, true);'>{0}</select>", sRes)
    End Function
    'A1022 ==

    Public Function Get4ASAParticipants(wid As Integer) As String
        If SynthesisView <> SynthesisViews.vt4ASA Then Return ""
        Dim sRes As String = ""
        Dim selUserID As Integer = Selected4ASAUser(wid)
        For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList.OrderBy(Function(g) g.Name)
            group.ApplyRules()
            If Api.IsGroupHasData(PM.CombinedGroups.GetGroupByCombinedID(group.CombinedUserID)) Then sRes += String.Format("<option value='{0}' {2}>[{1}]</option>", group.CombinedUserID, SafeFormString(group.Name), IIf(group.CombinedUserID = selUserID, " selected='selected' ", ""))
        Next
        sRes += "<option disabled='disabled'>-----------------------------</option>"
        For Each user As clsUser In PM.UsersList.OrderBy(Function(u) If(u.UserName = "", u.UserEMail, u.UserName))
            If Api.UserIDsWithData.Contains(user.UserID) Then sRes += String.Format("<option value='{0}' {2}>{1}</option>", user.UserID, SafeFormString(If(user.UserName = "", user.UserEMail, user.UserName)), IIf(user.UserID = selUserID, " selected='selected' ", ""))
        Next
        Return String.Format("<select class='select small' id='cb4ASAUsers" + CStr(wid) + "' style='display:none; width:120px; margin-right:5px;' onchange='sendCommand(""action=" + ACTION_SELECTED_4ASA_USER + "&wid=" + CStr(wid) + "&id=""+this.value, true); $(""#cbKeep" + CStr(wid) + """).prop(""checked"", true);'>{0}</select>", sRes)
    End Function

    Public Function Get4ASAKeepCb(wid As Integer) As String
        If SynthesisView <> SynthesisViews.vt4ASA Then Return ""
        Return "<label id='lblKeep" + CStr(wid) + "' style='display:none;'><input type='checkbox' class='small cb_keep' id='cbKeep" + CStr(wid) + "' onclick='updatePortionsUI(); sendCommand(""action=" + ACTION_4ASA_KEEP + "&wid=" + CStr(wid) + "&chk=""+this.checked+""&uid=""+$get(""cb4ASAUsers" + CStr(wid) + """).value, true);' " + CStr(IIf(Keep4ASAUser(wid), " checked='checked' ", "")) + ">Keep</label>"
    End Function

#End Region

#Region "CV"

    Public ReadOnly Property CanUserStartTeamTime As Boolean
        Get
            Return Not CheckLicenseForStartTeamTime OrElse App.isTeamTimeAvailable
        End Get
    End Property

    Public Property CVShowAllRows As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_CV_SHOW_ALL_ROWS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_CV_SHOW_ALL_ROWS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property CVShowTopNRows As Integer
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_CV_SHOW_TOP_N_ROWS_ID, UNDEFINED_USER_ID))
            If retVal <> -1 AndAlso retVal <> 5 AndAlso retVal <> 10 AndAlso retVal <> 25 Then retVal = -1
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_CV_SHOW_TOP_N_ROWS_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Private ReadOnly Property SESS_CV_CHECKED_NODES As String
        Get
            Return String.Format("SESS_CV_CHECKED_NODES_{0}_{1}", App.ActiveProject.ID.ToString, App.ActiveProject.ProjectManager.ActiveHierarchy) 'A1312
        End Get
    End Property

    Private Property CVCheckedNodes As String
        Get
            Return SessVar(SESS_CV_CHECKED_NODES)
        End Get
        Set(value As String)
            SessVar(SESS_CV_CHECKED_NODES) = value
        End Set
    End Property

    Public Enum CVFilters
        cvfAlternatives = 0
        cvfObjectives = 1
        cvfBoth = 2
    End Enum

    Private ReadOnly Property SESS_CV_FILTER_BY As String
        Get
            Return "SESS_CV_FILTER_BY_" + App.ActiveProject.ID.ToString
        End Get
    End Property

    Public Property CVFilterBy As CVFilters
        Get
            Dim retval As CVFilters = CVFilters.cvfBoth
            Dim s As String = SessVar(SESS_CV_FILTER_BY)
            If Not String.IsNullOrEmpty(s) Then retval = CType(CInt(s), CVFilters)
            Return retval
        End Get
        Set(value As CVFilters)
            SessVar(SESS_CV_FILTER_BY) = CStr(CInt(value))
        End Set
    End Property

    Private Class ConsensusViewItem
        Public Rank As Integer
        Public CovObjID As Integer
        Public CovObjName As String
        Public ChildID As Integer
        Public IsAlt As Boolean
        Public ChildName As String
        Public Variance As Double
        Public StdDeviation As Double
        Public StepNumber As Integer
        Public IsTTStepAvailable As Boolean
    End Class

    Public Function GetConsensusViewData(sAction As String) As String
        Dim retVal As String = ""

        If IsConsensusView() Then
            Dim DS As DataSet = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtConsensusViewOnly)
            Dim dt = DS.Tables(0) ' Consensus View table
            Dim ConsensusViewItems As List(Of ConsensusViewItem) = New List(Of ConsensusViewItem)

            Dim AltsCount As Integer = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count
            Dim FilterNum As Integer = AlternativesFilterValue
            Dim filter_alts = New List(Of Integer)

            Dim combined_alts = New Dictionary(Of Integer, List(Of NodePriority))

            If FilterNum >= AltsCount Then FilterNum = AltsCount
            If FilterNum > 0 OrElse FilterNum = -105 OrElse FilterNum = -110 OrElse FilterNum = -125 Then
                Dim objs As New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
                Dim users = New List(Of Integer)
                users.Add(COMBINED_USER_ID)

                Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)

                If H IsNot Nothing AndAlso H.Nodes.Count > 0 Then
                    PM.CalculationsManager.GetAlternativesGrid(H.Nodes(0).NodeID, users, objs, combined_alts, True, True)
                End If

                Dim combined_alts_values = combined_alts(COMBINED_USER_ID)
                combined_alts_values = combined_alts_values.OrderByDescending(Function(f) f.GlobalPriority).ToList
                'For i As Integer = 0 To Math.Min(FilterNum, combined_alts_values.Count) - 1
                '    filter_alts.Add(combined_alts_values(i).NodeID)
                'Next
                If FilterNum > 0 Then
                    For K As Integer = 0 To Math.Min(FilterNum, combined_alts_values.Count) - 1
                        filter_alts.Add(combined_alts_values(K).NodeID)
                    Next
                Else ' bottom x alts
                    For K As Integer = 0 To Math.Min(Math.Abs(FilterNum + 100), combined_alts_values.Count) - 1
                        filter_alts.Add(combined_alts_values(combined_alts_values.Count - K - 1).NodeID)
                    Next
                End If
            Else
                Select Case FilterNum
                    Case -1 ' All Alternatives
                        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            filter_alts.Add(alt.NodeID)
                        Next
                    Case -2 ' Advanced
                        Dim tAltsNum As Integer = AlternativesAdvancedFilterValue
                        Dim tUserId As Integer = AlternativesAdvancedFilterUserID

                        Dim objs As New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
                        Dim users = New List(Of Integer)
                        users.Add(tUserId)

                        Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)

                        If H IsNot Nothing AndAlso H.Nodes.Count > 0 Then
                            PM.CalculationsManager.GetAlternativesGrid(H.Nodes(0).NodeID, users, objs, combined_alts, True, True)
                        End If

                        Dim combined_alts_values = combined_alts(tUserId)
                        combined_alts_values = combined_alts_values.OrderByDescending(Function(f) f.GlobalPriority).ToList
                        For i As Integer = 0 To Math.Min(tAltsNum, combined_alts_values.Count) - 1
                            filter_alts.Add(combined_alts_values(i).NodeID)
                        Next
                    Case -3 ' Select/Deselect Alternatives
                        'Dim sAlts = AlternativesCustomFilterValue.Split(CChar(","))
                        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            'If sAlts.Contains(alt.NodeID.ToString) Then filter_alts.Add(alt.NodeID)
                            If IsAltSelected(alt.NodeGuidID) Then filter_alts.Add(alt.NodeID)
                        Next
                    Case -4 ' Filter by funded in RA scenario
                        Dim tCurrentFiltersList = CurrentFiltersList
                        For Each alt As ECCore.clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            If IsAltFunded(alt.NodeGuidID) Then filter_alts.Add(alt.NodeID)
                        Next
                        'A1172 ===
                    Case -5 ' Filter by alternative attributes         
                        Dim tCurrentFiltersList = CurrentFiltersList
                        For Each alt As ECCore.clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            If alt.IsNodeIncludedInFilter(tCurrentFiltersList) Then filter_alts.Add(alt.NodeID)
                        Next
                        'A1172 ==
                    Case -6 ' Show risks only
                        Dim tCurrentFiltersList = CurrentFiltersList
                        For Each alt As ECCore.clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            If alt.EventType = EventType.Risk Then filter_alts.Add(alt.NodeID)
                        Next
                    Case -7 ' Show opportunity only
                        Dim tCurrentFiltersList = CurrentFiltersList
                        For Each alt As ECCore.clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            If alt.EventType = EventType.Opportunity Then filter_alts.Add(alt.NodeID)
                        Next
                End Select

            End If

            For Each row As DataRow In dt.Rows
                Dim cvi As ConsensusViewItem = New ConsensusViewItem

                cvi.CovObjID = CInt(row.Item("ParentID"))

                Dim HN = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(cvi.CovObjID)
                If HN IsNot Nothing Then cvi.CovObjName = HN.NodeName Else cvi.CovObjName = ResString("lblNoSpecificCause")

                cvi.ChildID = CInt(row("ChildID"))
                cvi.IsAlt = CBool(row("IsAlternative"))

                If cvi.IsAlt Then
                    Dim alt = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(cvi.ChildID)
                    If alt IsNot Nothing Then cvi.ChildName = alt.NodeName
                Else
                    Dim obj = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(cvi.ChildID)
                    If obj IsNot Nothing Then cvi.ChildName = obj.NodeName
                End If

                cvi.Variance = CDbl(row("Variance"))
                cvi.StdDeviation = CDbl(row("StdDeviation"))
                cvi.StepNumber = CInt(row("TTStepID"))
                cvi.IsTTStepAvailable = False
                If Not IsDBNull(row("IsTTStepAvailable")) Then cvi.IsTTStepAvailable = CBool(row("IsTTStepAvailable"))

                If (Not cvi.IsAlt OrElse filter_alts.Contains(cvi.ChildID)) AndAlso (CVShowAllRows OrElse (cvi.Variance < 1 AndAlso cvi.StdDeviation < 1)) Then

                    Dim i As Integer = 0
                    While i < ConsensusViewItems.Count AndAlso ConsensusViewItems(i).StdDeviation > cvi.StdDeviation
                        i += 1
                    End While

                    ConsensusViewItems.Insert(i, cvi)
                End If
            Next

            Dim tFilterNum As Integer = CVShowTopNRows

            Dim CheckedNodes As String() = Nothing
            If CVCheckedNodes IsNot Nothing Then CheckedNodes = CVCheckedNodes.Split(CChar(","))

            Dim FilterBy As CVFilters = CVFilterBy
            Dim FilteredList As New List(Of ConsensusViewItem)

            For i As Integer = 0 To ConsensusViewItems.Count - 1
                Dim cvi = ConsensusViewItems(i)
                cvi.Rank = i + 1

                Dim isIncludedInFilter As Boolean = FilterBy = CVFilters.cvfBoth
                If Not isIncludedInFilter Then
                    If FilterBy = CVFilters.cvfAlternatives AndAlso cvi.IsAlt Then isIncludedInFilter = True
                    If FilterBy = CVFilters.cvfObjectives AndAlso Not cvi.IsAlt Then isIncludedInFilter = True
                End If

                If isIncludedInFilter Then
                    If (CheckedNodes Is Nothing OrElse CheckedNodes.Contains(CStr(cvi.CovObjID))) Then
                        FilteredList.Add(cvi)
                    End If
                End If
            Next

            Dim tCount As Integer = FilteredList.Count
            If tFilterNum <> -1 Then
                tCount = CInt(IIf(tFilterNum > tCount, tCount, tFilterNum))
            End If

            FilteredList = FilteredList.Take(tCount).ToList()

            For i As Integer = 0 To FilteredList.Count - 1
                Dim cvi = FilteredList(i)
                retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("{{""rank"":{0},""command"":""{1}"",""is_alt"":{2},""child_id"":{3},""child_name"":""{4}"",""cov_obj_id"":{5},""cov_obj_name"":""{6}"",""variance"":{7},""std_dev"":{8},""step_num"":{9},""is_step_avail"":{10}}}", cvi.Rank, "", Bool2JS(cvi.IsAlt), cvi.ChildID, JS_SafeString(SafeFormString(cvi.ChildName)), cvi.CovObjID, JS_SafeString(SafeFormString(cvi.CovObjName)), JS_SafeNumber(cvi.Variance * 100), JS_SafeNumber(cvi.StdDeviation * 100), cvi.StepNumber, Bool2JS(cvi.IsTTStepAvailable))
            Next
        End If

        If sAction = "" Then Return String.Format("[{0}]", retVal)
        Return String.Format("[""{0}"", [{1}]]", sAction, retVal)
    End Function

#End Region

#Region "Filtering by alternative attribute"
    Public ReadOnly Property ActiveProjectHasAlternativeAttributes As Boolean
        Get
            If App IsNot Nothing AndAlso App.ActiveProject IsNot Nothing AndAlso App.ActiveProject.ProjectManager IsNot Nothing AndAlso App.ActiveProject.ProjectManager.Attributes IsNot Nothing AndAlso App.ActiveProject.ProjectManager.Attributes.AttributesList IsNot Nothing Then
                For Each attr In App.ActiveProject.ProjectManager.Attributes.AttributesList
                    If attr.Type = CurrentAlternativeAttributesType AndAlso Not attr.IsDefault Then Return True
                Next
            End If
            Return False
        End Get
    End Property

    Private _FilterCombination As FilterCombinations = FilterCombinations.fcAnd
    Public Property FilterCombination As FilterCombinations
        Get
            Return _FilterCombination
        End Get
        Set(value As FilterCombinations)
            _FilterCombination = value
        End Set
    End Property

    ReadOnly Property SESSION_FILTER_RULES As String
        Get
            Return String.Format("RiskResultsFilterRulesAlts_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Private _CurrentFiltersList As List(Of clsFilterItem) = Nothing
    Public Property CurrentFiltersList As List(Of clsFilterItem)
        Get
            If _CurrentFiltersList Is Nothing Then
                ' D2668 ===
                Dim tSessVar = Session(SESSION_FILTER_RULES)
                If tSessVar Is Nothing OrElse Not (TypeOf tSessVar Is List(Of clsFilterItem)) Then
                    _CurrentFiltersList = New List(Of clsFilterItem)
                    Session.Add(SESSION_FILTER_RULES, _CurrentFiltersList)
                Else
                    _CurrentFiltersList = CType(tSessVar, List(Of clsFilterItem))
                End If
                ' D2668 ==
            End If
            Return _CurrentFiltersList
        End Get
        Set(value As List(Of clsFilterItem))
            _CurrentFiltersList = value
            Session(SESSION_FILTER_RULES) = value
        End Set
    End Property

    Private ReadOnly Property CurrentAlternativeAttributesType As AttributeTypes
        Get
            Return AttributeTypes.atAlternative
        End Get
    End Property

    Private ReadOnly Property CurrentObjectiveAttributesType As AttributeTypes
        Get
            Dim tCurrentAttrType As AttributeTypes = AttributeTypes.atNode
            If PM.IsRiskProject Then
                If PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
                    tCurrentAttrType = AttributeTypes.atLikelihood
                Else
                    tCurrentAttrType = AttributeTypes.atImpact
                End If
            End If
            Return tCurrentAttrType
        End Get
    End Property

    Public Function LoadAttribData() As String
        Dim sList As String = ""
        Dim sFlt As String = ""
        With App.ActiveProject.ProjectManager
            Dim fIsEmpty As Boolean = CurrentFiltersList.Count = 0
            For Each tAttr As clsAttribute In .Attributes.AttributesList
                If Not tAttr.IsDefault AndAlso tAttr.Type = CurrentAlternativeAttributesType Then
                    Dim sVals As String = ""
                    If tAttr.ValueType = AttributeValueTypes.avtEnumeration OrElse tAttr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim tVals As clsAttributeEnumeration = .Attributes.GetEnumByID(tAttr.EnumID)
                        If tVals IsNot Nothing Then
                            For Each tVal As clsAttributeEnumerationItem In tVals.Items
                                sVals += CStr(IIf(sVals = "", "", ",")) + String.Format("[""{0}"",""{1}""]", tVal.ID, JS_SafeString(tVal.Value))
                            Next
                        End If
                    End If
                    sList += CStr(IIf(sList = "", "", ",")) + String.Format("[""{0}"",""{1}"",{2},[{3}]]", tAttr.ID, JS_SafeString(App.GetAttributeName(tAttr)), CInt(tAttr.ValueType), sVals)
                    If fIsEmpty Then CurrentFiltersList.Add(New clsFilterItem With {.IsChecked = False, .SelectedAttributeID = tAttr.ID, .FilterOperation = FilterOperations.Equal})
                End If
            Next
            Dim i As Integer = 0
            While i < CurrentFiltersList.Count
                Dim tVal As clsFilterItem = CurrentFiltersList(i)
                Dim sVal As String = ""
                Dim tAttr = App.ActiveProject.ProjectManager.Attributes.GetAttributeByID(tVal.SelectedAttributeID)
                If tAttr IsNot Nothing Then
                    Select Case tAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration
                            sVal = CStr(IIf(tVal.FilterEnumItemID.Equals(Guid.Empty), "", tVal.FilterEnumItemID.ToString))
                        Case AttributeValueTypes.avtEnumerationMulti
                            sVal = ""
                            If tVal.FilterEnumItemsIDs IsNot Nothing Then
                                For Each ID As Guid In tVal.FilterEnumItemsIDs
                                    If sVal.Length > 0 Then sVal += ";"
                                    sVal += ID.ToString
                                Next
                            End If

                        Case Else
                            sVal = tVal.FilterText
                    End Select
                    sFlt += CStr(IIf(sFlt = "", "", ",")) + String.Format("[""{0}"",""{1}"",{2},""{3}""]", IIf(tVal.IsChecked, 1, 0), tAttr.ID, CInt(tVal.FilterOperation), JS_SafeString(sVal))
                    i += 1
                Else
                    CurrentFiltersList.RemoveAt(i)
                End If
            End While
        End With
        Return String.Format("var attr_list = [{0}];{1} var attr_flt = [{2}];", sList, vbCrLf, sFlt)
    End Function

#End Region

#Region "User Attributes Filter"
    ReadOnly Property SESSION_USERS_FILTER_RULES As String
        Get
            Return String.Format("SynthesisUserFilterRulesAlts_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Private _CurrentUsersFiltersList As List(Of clsFilterItem) = Nothing
    Public Property CurrentUsersFiltersList As List(Of clsFilterItem)
        Get
            If _CurrentUsersFiltersList Is Nothing Then
                Dim tSessVar = Session(SESSION_USERS_FILTER_RULES)
                If tSessVar Is Nothing OrElse Not (TypeOf tSessVar Is List(Of clsFilterItem)) Then
                    _CurrentUsersFiltersList = New List(Of clsFilterItem)
                    Session.Add(SESSION_USERS_FILTER_RULES, _CurrentUsersFiltersList)
                Else
                    _CurrentUsersFiltersList = CType(tSessVar, List(Of clsFilterItem))
                End If
            End If
            Return _CurrentUsersFiltersList
        End Get
        Set(value As List(Of clsFilterItem))
            _CurrentUsersFiltersList = value
            Session(SESSION_USERS_FILTER_RULES) = value
        End Set
    End Property

    Public Function GetUsersAttribData() As String
        Dim sList As String = ""
        With PM
            'Dim fIsEmpty As Boolean = CurrentUsersFiltersList.Count = 0
            For Each tAttr As clsAttribute In .Attributes.AttributesList
                If Not tAttr.IsDefault AndAlso tAttr.Type = AttributeTypes.atUser Then
                    Dim sVals As String = ""
                    If tAttr.ValueType = AttributeValueTypes.avtEnumeration OrElse tAttr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim tVals As clsAttributeEnumeration = .Attributes.GetEnumByID(tAttr.EnumID)
                        If tVals IsNot Nothing Then
                            For Each tVal As clsAttributeEnumerationItem In tVals.Items
                                sVals += CStr(IIf(sVals = "", "", ",")) + String.Format("[""{0}"",""{1}""]", tVal.ID, tVal.Value)
                            Next
                        End If
                    Else
                        If tAttr.DefaultValue IsNot Nothing Then
                            Select Case tAttr.ValueType
                                Case AttributeValueTypes.avtBoolean
                                    sVals = CStr(IIf(CBool(tAttr.DefaultValue), "true", "false"))
                                Case AttributeValueTypes.avtDouble
                                    sVals = JS_SafeNumber(CDbl(tAttr.DefaultValue))
                                Case AttributeValueTypes.avtString
                                    sVals = "'" + JS_SafeString(CStr(tAttr.DefaultValue)) + "'"
                                Case AttributeValueTypes.avtLong
                                    sVals = JS_SafeNumber(CLng(tAttr.DefaultValue))
                            End Select
                        End If
                    End If
                    Dim userVals As String = ""
                    For Each attrVal As clsAttributeValue In .Attributes.ValuesList
                        If attrVal.AttributeID.Equals(tAttr.ID) Then
                            Dim sValue As String = ""
                            Select Case tAttr.ValueType
                                Case AttributeValueTypes.avtBoolean
                                    sValue = CStr(IIf(CBool(attrVal.Value), "true", "false"))
                                Case AttributeValueTypes.avtDouble
                                    sValue = JS_SafeNumber(CDbl(attrVal.Value))
                                Case AttributeValueTypes.avtString
                                    sValue = "'" + JS_SafeString(CStr(attrVal.Value)) + "'"
                                Case AttributeValueTypes.avtLong
                                    sValue = JS_SafeNumber(CLng(attrVal.Value))
                                Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                    ' not implemented
                            End Select
                            userVals += CStr(IIf(userVals = "", "", ",")) + String.Format("[{0},{1}]", attrVal.UserID, sValue)
                        End If
                    Next
                    sList += CStr(IIf(sList = "", "", ",")) + String.Format("[""{0}"",""{1}"",{2},[{3}],[{4}]]", tAttr.ID, JS_SafeString(tAttr.Name), CInt(tAttr.ValueType), sVals, userVals)
                    'If fIsEmpty Then CurrentUsersFiltersList.Add(New clsFilterItem With {.IsChecked = False, .SelectedAttributeID = tAttr.ID, .FilterOperation = FilterOperations.Equal})
                End If
            Next
            ' add Inconsistency item
            'sList += IIf(sList = "", "", ",") + String.Format("['{0}','{1}',{2},[{3}]]", ATTRIBUTE_GROUP_BY_INCONSISTENCY_ID, JS_SafeString(ResString("optGroupByInconsistency")), CInt(AttributeValueTypes.avtDouble), "")
        End With
        Return sList
    End Function

    Public Function GetUsersFilterData() As String
        Dim sFlt As String = ""
        With PM
            Dim i As Integer = 0
            While i < CurrentUsersFiltersList.Count
                Dim tVal As clsFilterItem = CurrentUsersFiltersList(i)
                Dim sVal As String = ""
                Dim tAttr = .Attributes.GetAttributeByID(tVal.SelectedAttributeID)
                If tAttr IsNot Nothing Then
                    Select Case tAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration
                            sVal = CStr(IIf(tVal.FilterEnumItemID.Equals(Guid.Empty), "", tVal.FilterEnumItemID.ToString))
                        Case AttributeValueTypes.avtEnumerationMulti
                            sVal = ""
                            If tVal.FilterEnumItemsIDs IsNot Nothing Then
                                For Each ID As Guid In tVal.FilterEnumItemsIDs
                                    If sVal.Length > 0 Then sVal += ";"
                                    sVal += ID.ToString
                                Next
                            End If

                        Case Else
                            sVal = tVal.FilterText
                    End Select
                    sFlt += CStr(IIf(sFlt = "", "", ",")) + String.Format("[""{0}"",""{1}"",{2},""{3}""]", IIf(tVal.IsChecked, 1, 0), tAttr.ID, CInt(tVal.FilterOperation), JS_SafeString(sVal))
                    i += 1
                Else
                    CurrentUsersFiltersList.RemoveAt(i)
                End If
            End While
        End With
        Return sFlt
    End Function
    'A1157 ==

    Private Sub ParseUsersAttributesFilter(sFilter As String, sComb As String)
        CurrentUsersFiltersList.Clear()
        If sFilter <> "" Then
            If sComb <> "" Then
                If sComb = "0" Then FilterCombination = FilterCombinations.fcAnd
                If sComb = "1" Then FilterCombination = FilterCombinations.fcOr
            End If

            Dim sRows() As String = sFilter.Trim.Split(CChar(vbLf))
            For Each sRow As String In sRows
                Dim tVals() As String = sRow.Split((CChar(Flt_Separator)))
                If tVals.Length >= 3 Then
                    Dim sChecked As String = tVals(0)
                    Dim sAttrID As String = tVals(1)
                    Dim sOperID As String = tVals(2)
                    Dim sFilterText As String = ""
                    If tVals.Length > 3 Then sFilterText = tVals(3)

                    Dim tFilterItem As New clsFilterItem With {.FilterCombination = FilterCombination}
                    Dim tAttrID As Guid = New Guid(sAttrID)

                    With PM
                        For Each tAttr As clsAttribute In .Attributes.AttributesList
                            If tAttr.Type = AttributeTypes.atUser AndAlso tAttr.ID.Equals(tAttrID) Then
                                tFilterItem.SelectedAttributeID = tAttr.ID
                                Exit For
                            End If
                        Next
                    End With

                    tFilterItem.IsChecked = (sChecked = "1")
                    tFilterItem.FilterOperation = CType(CInt(sOperID), FilterOperations)
                    tFilterItem.FilterText = sFilterText

                    Dim tAttribute = PM.Attributes.GetAttributeByID(tFilterItem.SelectedAttributeID)
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = AttributeTypes.atUser AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumeration Then
                        tFilterItem.FilterEnumItemID = New Guid(sFilterText)
                    End If
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = AttributeTypes.atUser AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim sGuids As String() = sFilterText.Split(CChar(";"))
                        For Each sGuid In sGuids
                            If sGuid.Length > 0 Then
                                If tFilterItem.FilterEnumItemsIDs Is Nothing Then tFilterItem.FilterEnumItemsIDs = New List(Of Guid)
                                tFilterItem.FilterEnumItemsIDs.Add(New Guid(sGuid))
                            End If
                        Next
                    End If

                    CurrentUsersFiltersList.Add(tFilterItem)
                End If
            Next
        End If
    End Sub

    Public Function IsUserIncludedInFilter(user As ECCore.clsUser, tCurrentFiltersList As List(Of clsFilterItem)) As Boolean
        Dim retVal As Boolean = False

        If user IsNot Nothing AndAlso tCurrentFiltersList IsNot Nothing AndAlso tCurrentFiltersList.Count > 0 Then
            Dim filterOp As FilterCombinations = FilterCombinations.fcAnd
            filterOp = tCurrentFiltersList(0).FilterCombination

            Dim tNoneRuleChecked As Boolean = True

            For Each cfi As clsFilterItem In tCurrentFiltersList
                If cfi.FilterOperation <> FilterOperations.None AndAlso cfi.IsChecked Then
                    tNoneRuleChecked = False

                    Dim isCurrentRulePassed As Boolean = False

                    Dim tAttribute = PM.Attributes.GetAttributeByID(cfi.SelectedAttributeID)
                    Dim ObjValue As Object = PM.Attributes.GetAttributeValue(cfi.SelectedAttributeID, user.UserID)

                    If ObjValue IsNot Nothing Then
                        Select Case tAttribute.ValueType
                            Case ECCore.Attributes.AttributeValueTypes.avtString
                                Dim StrValue As String = CStr(ObjValue).ToLower
                                Select Case cfi.FilterOperation
                                    Case FilterOperations.Contains
                                        If StrValue.Contains(cfi.FilterText.ToString.ToLower) Then isCurrentRulePassed = True
                                    Case FilterOperations.Equal
                                        If StrValue.Trim = cfi.FilterText.ToString.ToLower.Trim Then isCurrentRulePassed = True
                                    Case FilterOperations.NotEqual
                                        If StrValue.Trim <> cfi.FilterText.ToString.ToLower.Trim Then isCurrentRulePassed = True
                                    Case FilterOperations.StartsWith
                                        If StrValue.Trim.StartsWith(cfi.FilterText.ToString.ToLower.Trim) Then isCurrentRulePassed = True
                                End Select
                            Case ECCore.Attributes.AttributeValueTypes.avtBoolean
                                Dim Value As Boolean = CBool(ObjValue)
                                Select Case cfi.FilterOperation
                                    Case FilterOperations.IsTrue
                                        If Value Then isCurrentRulePassed = True
                                    Case FilterOperations.IsFalse
                                        If Not Value Then isCurrentRulePassed = True
                                End Select
                            Case ECCore.Attributes.AttributeValueTypes.avtDouble
                                Dim Value, FilterDouble As Double
                                Value = CDbl(ObjValue)
                                If String2Double(CStr(cfi.FilterText), FilterDouble) Then
                                    Select Case cfi.FilterOperation
                                        Case FilterOperations.Equal
                                            If Value = FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.NotEqual
                                            If Value <> FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.GreaterThan
                                            If Value > FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.GreaterThanOrEqual
                                            If Value >= FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.LessThan
                                            If Value < FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.LessThanOrequal
                                            If Value <= FilterDouble Then isCurrentRulePassed = True
                                    End Select
                                Else
                                    If cfi.FilterOperation = FilterOperations.NotEqual Then isCurrentRulePassed = True
                                End If
                            Case ECCore.Attributes.AttributeValueTypes.avtLong
                                Dim Value, FilterLong As Long
                                Value = CLng(ObjValue)
                                If Long.TryParse(CStr(cfi.FilterText), FilterLong) Then
                                    Select Case cfi.FilterOperation
                                        Case FilterOperations.Equal
                                            If Value = FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.NotEqual
                                            If Value <> FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.GreaterThan
                                            If Value > FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.GreaterThanOrEqual
                                            If Value >= FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.LessThan
                                            If Value < FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.LessThanOrequal
                                            If Value <= FilterLong Then isCurrentRulePassed = True
                                    End Select
                                Else
                                    If cfi.FilterOperation = FilterOperations.NotEqual Then isCurrentRulePassed = True
                                End If
                            Case ECCore.Attributes.AttributeValueTypes.avtEnumeration
                                Dim tEnumID As Guid = CType(ObjValue, Guid)
                                Select Case cfi.FilterOperation
                                    Case FilterOperations.Equal
                                        If tEnumID.Equals(cfi.FilterEnumItemID) Then isCurrentRulePassed = True
                                    Case FilterOperations.NotEqual
                                        If Not tEnumID.Equals(cfi.FilterEnumItemID) Then isCurrentRulePassed = True
                                End Select
                            Case ECCore.Attributes.AttributeValueTypes.avtEnumerationMulti
                                Dim tEnumIDs As String = CStr(ObjValue)
                                Select Case cfi.FilterOperation
                                    Case FilterOperations.Contains
                                        If Not String.IsNullOrEmpty(tEnumIDs) AndAlso cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                            Dim tContainsAll As Boolean = cfi.FilterEnumItemsIDs.Count > 0
                                            For Each value In cfi.FilterEnumItemsIDs
                                                If Not tEnumIDs.Contains(value.ToString) Then
                                                    tContainsAll = False
                                                    Exit For
                                                End If
                                            Next
                                            If tContainsAll Then isCurrentRulePassed = True
                                        End If
                                    Case FilterOperations.Equal
                                        If Not String.IsNullOrEmpty(tEnumIDs) AndAlso cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                            Dim tEqual As Boolean = cfi.FilterEnumItemsIDs.Count > 0 AndAlso cfi.FilterEnumItemsIDs.Count = tEnumIDs.Split(CChar(";")).Count
                                            For Each value In cfi.FilterEnumItemsIDs
                                                If Not tEnumIDs.Contains(value.ToString) Then
                                                    tEqual = False
                                                    Exit For
                                                End If
                                            Next
                                            If tEqual Then isCurrentRulePassed = True
                                        End If
                                    Case FilterOperations.NotEqual
                                        If cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                            If Not String.IsNullOrEmpty(tEnumIDs) Then
                                                Dim tEqual As Boolean = cfi.FilterEnumItemsIDs.Count > 0 AndAlso cfi.FilterEnumItemsIDs.Count = tEnumIDs.Split(CChar(";")).Count
                                                For Each value In cfi.FilterEnumItemsIDs
                                                    If Not tEnumIDs.Contains(value.ToString) Then
                                                        tEqual = False
                                                        Exit For
                                                    End If
                                                Next
                                                If Not tEqual Then isCurrentRulePassed = True
                                            Else
                                                isCurrentRulePassed = True
                                            End If
                                        Else
                                            If Not String.IsNullOrEmpty(tEnumIDs) Then isCurrentRulePassed = True
                                        End If
                                End Select
                        End Select
                    End If

                    'apply set
                    Dim indexOfFirstCheckedRule As Integer = 0
                    For i As Integer = 0 To tCurrentFiltersList.Count - 1
                        If tCurrentFiltersList(i).IsChecked Then
                            indexOfFirstCheckedRule = i
                            Exit For
                        End If
                    Next

                    If tCurrentFiltersList.IndexOf(cfi) = indexOfFirstCheckedRule Then
                        If isCurrentRulePassed Then retVal = True
                    Else
                        Select Case filterOp
                            Case FilterCombinations.fcAnd
                                If Not isCurrentRulePassed Then retVal = False
                            Case FilterCombinations.fcOr
                                If isCurrentRulePassed Then retVal = True
                        End Select
                    End If
                    filterOp = cfi.FilterCombination
                End If
            Next

            If tNoneRuleChecked Then retVal = True
        Else
            retVal = True
        End If

        Return retVal
    End Function

    Private Function GetCurrentFilterRuleXML() As String
        Dim tCurrentFiltersList = CurrentUsersFiltersList
        If tCurrentFiltersList.Count > 0 Then

            Dim xDoc As XDocument = <?xml version="1.0" encoding="utf-8" standalone="yes"?>
                                    <!--FilterCombination values - 0 = AND, 1 = OR -->
                                    <Root>
                                        <Settings FilterCombination=""></Settings>
                                        <Rules></Rules>
                                    </Root>

            xDoc.Root.Element("Settings").Attribute("FilterCombination").Value = CInt(tCurrentFiltersList(0).FilterCombination).ToString

            For Each cfi As clsFilterItem In tCurrentFiltersList
                If cfi.IsChecked AndAlso Not cfi.SelectedAttributeID.Equals(Guid.Empty) Then
                    Dim tAttribute As clsAttribute = PM.Attributes.GetAttributeByID(cfi.SelectedAttributeID)
                    If tAttribute IsNot Nothing Then
                        Dim fOperation As Integer = CInt(cfi.FilterOperation)
                        Dim FilterOperationName = [Enum].GetName(GetType(FilterOperations), fOperation)
                        Dim FilterOperationID As Integer = Convert.ToInt32(fOperation)
                        Dim FilterText As String = ""
                        If cfi.FilterText IsNot Nothing Then FilterText = cfi.FilterText.Trim '.ToLower
                        If tAttribute.Name IsNot Nothing AndAlso tAttribute.Name.Trim <> "" Then
                            Dim el As XElement = New XElement("Rule", New XAttribute("AttributeID", tAttribute.ID.ToString), New XAttribute("AttributeName", tAttribute.Name), New XAttribute("OperationName", FilterOperationName), New XAttribute("OperationID", FilterOperationID), New XAttribute("FilterText", FilterText))
                            xDoc.Root.Element("Rules").Add(el)
                        End If
                    End If
                End If
            Next

            Dim sb As StringBuilder = New StringBuilder()
            Dim xws As XmlWriterSettings = New XmlWriterSettings()
            xws.OmitXmlDeclaration = False
            xws.Indent = True

            Using xw = XmlWriter.Create(sb, xws)
                xDoc.WriteTo(xw)
            End Using
            Return sb.ToString
        End If
        Return ""
    End Function

    Public Function GetFilteredUsersData() As String
        Dim sCol As String = ""
        sCol += If(sCol = "", "", ",") + String.Format("[""{0}"",""{1}"",{2},{3},""{4}""]", "Email Address", "left", Bool2JS(True), Bool2JS(True), "email")
        sCol += If(sCol = "", "", ",") + String.Format("[""{0}"",""{1}"",{2},{3},""{4}""]", "Participant Name", "left", Bool2JS(True), Bool2JS(True), "name")

        Dim UserAttrList = PM.Attributes.AttributesList.Where(Function(a) a.Type = AttributeTypes.atUser AndAlso a.IsDefault = False)

        Dim i As Integer = 0
        For Each attr As clsAttribute In UserAttrList
            Dim cellClass As String = "left"
            sCol += If(sCol = "", "", ",") + String.Format("[""{0}"",""{1}"",{2},{3},""v{4}""]", JS_SafeString(attr.Name), cellClass, Bool2JS(True), Bool2JS(True), i)
            i += 1
        Next

        Dim sRes As String = ""
        Dim tCurrentFiltersList = CurrentUsersFiltersList

        Dim tPrjUsers As List(Of ECTypes.clsUser) = PM.UsersList

        For Each tUser As ECTypes.clsUser In tPrjUsers
            Dim tUserVisible As Boolean = True

            If tCurrentFiltersList IsNot Nothing AndAlso tCurrentFiltersList.Count > 0 Then
                If tCurrentFiltersList.Where(Function(r) r.IsChecked AndAlso r.SelectedAttributeID.Equals(DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID)).Count > 0 Then
                    'TODO: AC - call a function to update the Inconsistencies for all users
                End If
                tUserVisible = IsUserIncludedInFilter(tUser, tCurrentFiltersList)
            End If

            If tUserVisible Then
                i = 0
                sRes += If(sRes = "", "", ",") + String.Format("{{""email"":""{0}"",""name"":""{1}""", JS_SafeString(tUser.UserEMail), JS_SafeString(tUser.UserName))
                For Each attr As clsAttribute In UserAttrList
                    sRes += String.Format(",""v{0}"":""{1}""", i, JS_SafeString(PM.Attributes.GetAttributeValueString(attr.ID, Guid.Empty, tUser.UserID)))
                    i += 1
                Next
                sRes += "}"
            End If
        Next

        Return String.Format("[{0}], [{1}]", sCol, sRes)
    End Function

#End Region

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = GetParam(args, "action", True)
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction))

        Select Case sAction
            Case ACTION_SYNTHESIS_MODE
                PM.CalculationsManager.SynthesisMode = CType(CInt(GetParam(args, "value", True)), ECSynthesisMode)
                PM.PipeParameters.SynthesisMode = PM.CalculationsManager.SynthesisMode
                If PM.PipeParameters.SynthesisMode = ECSynthesisMode.smDistributive AndAlso PM.Parameters.Normalization = LocalNormalizationType.ntUnnormalized Then
                    PM.Parameters.Normalization = LocalNormalizationType.ntNormalizedForAll
                    PM.Parameters.Save()
                    PM.CalculationsManager.UseNormalizationForSA = PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized
                End If
                SavePipeParameters()
                tResult = AllCallbackData(sAction)
            Case ACTION_GRID_WRT_STATE
                Api.WRTState = CType(CInt(GetParam(args, "value", True)), ECWRTStates)
                tResult = AllCallbackData(sAction)
            Case ACTION_COMBINED_MODE
                PM.CalculationsManager.CombinedMode = CType(CInt(GetParam(args, "value", True)), CombinedCalculationsMode)
                PM.PipeParameters.CombinedMode = PM.CalculationsManager.CombinedMode
                SavePipeParameters()
                tResult = AllCallbackData(sAction)
            Case ACTION_CIS_MODE
                Dim param As Boolean = Param2Bool(args, "value")
                PM.CalculationsManager.UseCombinedForRestrictedNodes = param
                PM.PipeParameters.UseCISForIndividuals = param
                SavePipeParameters()
                tResult = AllCallbackData(sAction)
            Case ACTION_USER_WEIGHTS_MODE
                Dim param As Boolean = Param2Bool(args, "value")
                PM.CalculationsManager.UseUserWeights = param
                PM.PipeParameters.UseWeights = param
                SavePipeParameters()
                tResult = AllCallbackData(sAction, , True)
            Case "include_ideal"
                Dim param As Boolean = Param2Bool(args, "value")
                PM.PipeParameters.IncludeIdealAlternative = param
                SavePipeParameters()
                tResult = AllCallbackData(sAction, , True)
            Case ACTION_DECIMALS
                PM.Parameters.DecimalDigits = CInt(GetParam(args, "value", True))
                tResult = AllCallbackData(sAction)
            Case ACTION_ALTS_FILTER
                AlternativesFilterValue = CInt(GetParam(args, "value", True))
                Select Case AlternativesFilterValue
                    Case -2
                        AlternativesAdvancedFilterValue = CInt(GetParam(args, "alts_num", True))
                        AlternativesAdvancedFilterUserID = CInt(GetParam(args, "user_id", True))
                    Case -3
                        Dim AlternativesCustomFilterValue As String() = GetParam(args, "ids", True).Split(CChar(","))   ' Anti-XSS
                        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            Dim tAltSelectedOld As Boolean = IsAltSelected(alt.NodeGuidID)
                            Dim tAltSelectedNew As Boolean = AlternativesCustomFilterValue.Contains(alt.NodeID.ToString)
                            If tAltSelectedOld <> tAltSelectedNew Then IsAltSelected(alt.NodeGuidID) = tAltSelectedNew
                        Next
                    Case -4 ' funded in RA scenario
                    Case -5 ' filter by alternative attribute
                        Dim sFilter As String = URLDecode(GetParam(args, "filter", True))
                        Dim sComb As String = GetParam(args, "combination", True)
                        Api.ParseAttributesFilter(sFilter, sComb)
                End Select
                If IsConsensusView() Then tResult = GetConsensusViewData(sAction) Else tResult = AllCallbackData(sAction)
            Case ACTION_NORMALIZE_MODE
                PM.Parameters.Normalization = CType(CInt(GetParam(args, "value", True)), LocalNormalizationType)
                PM.Parameters.Save()
                PM.CalculationsManager.UseNormalizationForSA = PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized
                initSAData()
                tResult = AllCallbackData(sAction)
            Case ACTION_SELECTED_USER
                SAUserID = CInt(GetParam(args, "id", True))
                'tResult = SACallbackData()
                tResult = AllCallbackData(sAction, , True) 'TODO: revert to prev. (commented out) version
            Case ACTION_SELECTED_USERS
                'TODO: SAUserID = CInt(GetParam(args, "sa_user").Trim())
                Dim sIDs As String = GetParam(args, "ids", True)
                StringSelectedUsersAndGroupsIDs = sIDs
                tResult = AllCallbackData(sAction, , True) 'TODO: revert to prev. (commented out) version
            Case ACTION_SELECTED_4ASA_USER
                Dim userID As Integer = CInt(GetParam(args, "id", True))
                Dim widgetIndex As Integer = CInt(GetParam(args, "wid", True))
                Selected4ASAUser(widgetIndex) = userID
                tResult = AllCallbackData(sAction)
            Case ACTION_4ASA_KEEP
                Dim widgetIndex As Integer = CInt(GetParam(args, "wid", True))
                Dim Chk As Boolean = Param2Bool(args, "chk")
                Dim Uid As Integer = Param2Int(args, "uid")
                If Chk Then
                    Selected4ASAUser(widgetIndex) = Uid
                Else
                    Selected4ASAUser(widgetIndex) = UNDEFINED_INTEGER_VALUE
                End If
                tResult = String.Format("[""{0}""]", sAction)
            Case ACTION_4ASA_NEXT_PORTION
                Dim tDir As Integer = CInt(GetParam(args, "dir", True))
                Dim sSelected As String = GetParam(args, "selected", True).Replace("undefined", UNDEFINED_INTEGER_VALUE.ToString)

                Dim s As String() = sSelected.Split(CChar(","))

                Dim uid0 As Integer = Selected4ASAUser(0)
                Dim uid1 As Integer = Selected4ASAUser(1)
                Dim uid2 As Integer = Selected4ASAUser(2)
                Dim uid3 As Integer = Selected4ASAUser(3)

                If s.Count = 4 Then
                    uid0 = CInt(s(0))
                    uid1 = CInt(s(1))
                    uid2 = CInt(s(2))
                    uid3 = CInt(s(3))
                End If

                Dim keep0 As Boolean = Keep4ASAUser(0)
                Dim keep1 As Boolean = Keep4ASAUser(1)
                Dim keep2 As Boolean = Keep4ASAUser(2)
                Dim keep3 As Boolean = Keep4ASAUser(3)

                Dim availList As New List(Of Integer)
                Dim lastID As Integer = 0

                If tDir > 0 Then
                    For Each uid As Integer In allList
                        If Not (uid = uid0) AndAlso Not (uid = uid1) AndAlso Not (uid = uid2) AndAlso Not (uid = uid3) Then
                            availList.Add(uid)
                        End If
                        If uid = uid0 And Not keep0 Or uid = uid1 And Not keep1 Or uid = uid2 And Not keep2 Or uid = uid3 And Not keep3 Then lastID = availList.Count
                    Next
                Else
                    For i As Integer = allList.Count - 1 To 0 Step -1
                        Dim uid As Integer = allList(i)
                        If Not (uid = uid0) AndAlso Not (uid = uid1) AndAlso Not (uid = uid2) AndAlso Not (uid = uid3) Then
                            availList.Add(uid)
                        End If
                        If uid = uid0 And Not keep0 Or uid = uid1 And Not keep1 Or uid = uid2 And Not keep2 Or uid = uid3 And Not keep3 Then lastID = availList.Count
                    Next
                End If

                If availList.Count > 0 AndAlso lastID <= availList.Count Then
                    If tDir > 0 Then
                        uid0 = getNextUID(keep0, availList, lastID, uid0)
                        uid1 = getNextUID(keep1, availList, lastID, uid1)
                        uid2 = getNextUID(keep2, availList, lastID, uid2)
                        uid3 = getNextUID(keep3, availList, lastID, uid3)
                    Else
                        uid3 = getNextUID(keep3, availList, lastID, uid3)
                        uid2 = getNextUID(keep2, availList, lastID, uid2)
                        uid1 = getNextUID(keep1, availList, lastID, uid1)
                        uid0 = getNextUID(keep0, availList, lastID, uid0)
                    End If
                End If
                Dim l As New List(Of Integer)
                l.Add(uid0)
                l.Add(uid1)
                l.Add(uid2)
                l.Add(uid3)

                tResult = String.Format("[""{0}"",{1}]", sAction, Get4ASAData(l))
            'Case ACTION_DSA_UPDATE_VALUES
            '    Dim s_values As String = GetParam(args, "values")
            '    Dim values() As String = s_values.Split(CChar(","))
            '    Dim s_ids As String = GetParam(args, "objids")
            '    Dim ids() As String = s_ids.Split(CChar(","))
            '    Dim ANewObjPriorities As New Dictionary(Of Integer, Double)
            '    Dim i = 0
            '    For Each objID In ids
            '        Dim APrty As Double = 0
            '        String2Double(values(i), APrty)
            '        ANewObjPriorities.Add(CInt(objID), APrty)
            '        i += 1
            '    Next
            '    updateAltValuesinZero(ANewObjPriorities)
            '    Dim ZeroValuesString As String = ""
            '    For Each Objitem In AltValuesInZero
            '        Dim ZeroAltValuesString As String = ""
            '        For Each AltItem In Objitem.Value
            '            ZeroAltValuesString += CStr(IIf(ZeroAltValuesString <> "", ",", "")) + String.Format("{{""altID"":{0},""val"":{1}}}", AltItem.Key, JS_SafeNumber(AltItem.Value))
            '        Next
            '        ZeroValuesString += CStr(IIf(ZeroValuesString <> "", ",", "")) + String.Format("[{0},[{1}]]", Objitem.Key, ZeroAltValuesString)
            '    Next
            '    Dim s_name As String = GetParam(args, "id")
            '    tResult = String.Format("[""{0}"", ""{1}"", [{2}]]", sAction, s_name, ZeroValuesString)
            Case ACTION_GRID_WRT_NODE_ID
                Api.WRTNodeID = CInt(GetParam(args, "value", True))
                Api.WRTNodeParentGUID = GetParam(args, "pid", True)
                tResult = AllCallbackData(sAction)
            Case ACTION_CHANGE_LAYOUT
                selectedLayout = CInt(GetParam(args, "value", True))
                tResult = String.Format("[""{0}""]", sAction)
            Case ACTION_SET_WIDGET
                Dim sValue As String = GetParam(args, "value", True)
                Dim index As Integer = CInt(GetParam(args, "index", True))
                Dim sOp As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_LAYOUT_ID, UNDEFINED_USER_ID))
                If Not String.IsNullOrEmpty(sOp) Then
                    Dim opt_args As NameValueCollection = HttpUtility.ParseQueryString(sOp)
                    opt_args.Set("widget" + index.ToString, sValue)
                    sValue = ""
                    For Each key As String In opt_args.Keys
                        sValue += CStr(IIf(sValue = "", "", "&")) + String.Format("{0}={1}", key, opt_args.Item(key).ToString)
                    Next
                End If
                WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_LAYOUT_ID, AttributeValueTypes.avtString, sValue)
                tResult = String.Format("[""{0}""]", sAction)
            Case ACTION_REFRESH_CV
                tResult = GetConsensusViewData(sAction)
            Case ACTION_SHOW_ALL_ROWS_CV
                CVShowAllRows = Param2Bool(args, "value")
                tResult = GetConsensusViewData(sAction)
            Case ACTION_FILTER_ROWS_CV
                CVShowTopNRows = CInt(GetParam(args, "value", True))
                tResult = GetConsensusViewData(sAction)
            Case ACTION_CHECKED_NODES_CV
                CVCheckedNodes = GetParam(args, "chk_nodes_ids", True)
                Select Case SynthesisView
                    Case SynthesisViews.vtCV
                        tResult = GetConsensusViewData(sAction)
                    Case SynthesisViews.vtASA, SynthesisViews.vt4ASA
                        tResult = AllCallbackData(sAction) 'TODO:
                End Select
            Case ACTION_FILTER_BY_CV
                CVFilterBy = CType(CInt(GetParam(args, "value", True)), CVFilters)
                tResult = GetConsensusViewData(sAction)
            Case "show_likelihoods_given_sources"
                PM.CalculationsManager.ShowDueToPriorities = Str2Bool(GetParam(args, "value", True)) 
                tResult = AllCallbackData(sAction)
            Case ACTION_RESET_MIXED_VIEW
                selectedWidgets = DefaultWidgets
                WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_LAYOUT_ID, AttributeValueTypes.avtString, Nothing)
                tResult = String.Format("[""{0}""]", sAction)
            Case "select_events"
                Dim StringSelectedEventIDs As String = GetParam(args, "event_ids", True)
                Dim IDs As String() = StringSelectedEventIDs.Split(CChar(","))
                For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                    alt.Enabled = IDs.Contains(alt.NodeID.ToString)
                Next
                PRJ.SaveStructure("Save active alternatives")
                tResult = AllCallbackData(sAction)
            Case ACTION_NODE_COLOR_SET
                Dim sValue As String = GetParam(args, "value", True)
                Dim tNodeID As Integer = CInt(GetParam(args, "id", True))
                Dim tHID As Integer = CInt(GetParam(args, "hid", True))
                Dim tNode As clsNode = Nothing
                Dim AttrGUID As Guid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID
                Dim sMsg As String = ParseString("Set %%alternative%% color")

                If tHID = -1 Then ' set alternative color
                    tNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(tNodeID)
                End If

                If tHID = -2 Then ' set active hierarchy objective color
                    If tNode Is Nothing Then
                        If PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
                            AttrGUID = ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID
                        Else
                            AttrGUID = ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID
                        End If
                        tNode = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(tNodeID)
                        sMsg = ParseString("Set %%objective%% color")
                    End If
                End If

                If tNode IsNot Nothing Then
                    sValue = sValue.Replace("#", "")
                    If sValue.Length = 6 Then sValue = "ff" + sValue
                    Dim tValue As Long = BrushToLong(sValue)

                    PM.Attributes.SetAttributeValue(AttrGUID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, tValue, tNode.NodeGuidID, Guid.Empty)
                    WriteAttributeValues(PRJ, sMsg, sMsg)
                End If
                ignoreCallback = False
                tResult = String.Format("[""{0}"",{1},{2},{3},{4},{5}]", sAction, Api.GetAlternativesData(), GetNodesData(), GetSAObjectives(), GetSAAlternatives(), GetASAData())
                ignoreCallback = True
            Case ACTION_NODES_COLORS_RESET
                Dim tHID As Integer = CInt(GetParam(args, "hid", True))
                Dim tNodes As List(Of clsNode) = Nothing
                Dim AttrGUID As Guid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID
                Dim sMsg As String = ""

                If tHID = -1 Then ' set alternative color
                    tNodes = PM.AltsHierarchy(PM.ActiveAltsHierarchy).Nodes
                    sMsg = ParseString("Reset %%alternatives%% colors")
                End If

                If tHID = -2 Then ' set active hierarchy objective color
                    If PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
                        AttrGUID = ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID
                    Else
                        AttrGUID = ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID
                    End If
                    tNodes = PM.Hierarchy(PM.ActiveHierarchy).Nodes
                    sMsg = ParseString("Reset %%objectives%% colors")
                End If

                If tNodes IsNot Nothing Then
                    For Each tNode As clsNode In tNodes
                        PM.Attributes.SetAttributeValue(AttrGUID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, Nothing, tNode.NodeGuidID, Guid.Empty)
                    Next

                    WriteAttributeValues(PRJ, sMsg, sMsg)
                End If

                ignoreCallback = False
                tResult = String.Format("[""{0}"",{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}]", sAction, Api.GetAlternativesData(), GetNodesData(), GetSAObjectives(), GetSAAlternatives(), GetASAData(), 0, 0, 0, GetSAComponentsData(), "{}", "{}", "{}", "{}", "{}", "{}", 0, "{}")
                ignoreCallback = True
            Case "show_users_count_in_header"
                Dim Chk As Boolean = Param2Bool(args, "chk")
                ProjectBooleanSetting(SETTING_SHOW_HEADER_TOTAL_USERS_NAME, False) = Chk
                tResult = String.Format("[""{0}"",{1}]", sAction, Api.GetGroupsData())
            Case "show_users_with_jdgm_in_header"
                Dim Chk As Boolean = Param2Bool(args, "chk")
                ProjectBooleanSetting(SETTING_SHOW_HEADER_USERS_WITH_JUDGMENTS_NAME, False) = Chk
                tResult = String.Format("[""{0}"",{1}]", sAction, Api.GetGroupsData())
            Case "show_users_with_all_jdgm_header"
                Dim Chk As Boolean = Param2Bool(args, "chk")
                ProjectBooleanSetting(SETTING_SHOW_HEADER_USERS_WITH_COMPLETED_JUDGMENTS_NAME, False) = Chk
                tResult = String.Format("[""{0}"",{1}]", sAction, Api.GetGroupsData())
            'Case "objectives_visibility"
            '    Dim Vis As Boolean = Param2Bool(args, "vis")
            '    PM.Parameters.Synthesis_ObjectivesVisible = Vis
            '    PM.Parameters.Save()
            '    tResult = String.Format("[""{0}"", {1}]", sAction, Bool2JS(Vis))
            Case "legends_visibility"
                Dim Vis As Boolean = Param2Bool(args, "vis")
                PM.Parameters.Synthesis_LegendsVisible = Vis
                PM.Parameters.Save()
                tResult = String.Format("[""{0}"", {1}]", sAction, Bool2JS(Vis))
            Case "asa_page_size"
                Dim value As Integer = Param2Int(args, "value")
                PM.Parameters.AsaPageSize = value
                PM.Parameters.Save()
                tResult = String.Format("[""{0}"", {1}]", sAction, value)
            Case "asa_page_num"
                Dim value As Integer = Param2Int(args, "value")
                PM.Parameters.AsaPageNum = value
                PM.Parameters.Save()
                tResult = String.Format("[""{0}"", {1}]", sAction, value)
            Case "asa_sort_by"
                Dim value As Integer = Param2Int(args, "value")
                PM.Parameters.AsaSortMode = value
                PM.Parameters.Save()
                tResult = String.Format("[""{0}"", {1}]", sAction, value)
            Case "dsa_active_sorting"
                Dim value As Boolean = Param2Bool(args, "value")
                PM.Parameters.DsaActiveSorting = value
                PM.Parameters.Save()
                tResult = String.Format("[""{0}"", {1}]", sAction, Bool2JS(value))
            Case "sa_alts_parameter"
                Dim value As Integer = Param2Int(args, "value")
                PM.Parameters.RiskSensitivityParameter = value
                PM.Parameters.Save()
                tResult = AllCallbackData(sAction)
            Case "sa_alts_sort_by"
                Dim value As Integer = Param2Int(args, "value")
                ' D6928 ===
                'AltsSortMode = value
                PM.Parameters.SensitivitySorting = CType(value, SASortMode)
                PM.Parameters.Save()
                ' D6928 ==
                tResult = String.Format("[""{0}"", {1}]", sAction, value)
                'tResult = AllCallbackData(sAction, , True)
            Case "show_components"
                Api.ShowComponents = Param2Bool(args, "value")
                tResult = String.Format("[""{0}""]", sAction)
            'Case "v_splitter_size"
            '    Dim sValue As String = GetParam(args, "value", True)
            '    Dim tValue As Double
            '    If String2Double(sValue, tValue) Then
            '        PM.Parameters.Synthesis_ObjectivesSplitterSize = Convert.ToInt32(tValue)
            '        PM.Parameters.Save()
            '    End If
            '    tResult = String.Format("[""{0}""]", sAction)
            Case "create_dyn_res_group"
                Dim sName As String = GetParam(args, "name", True)
                Dim sFilter As String = URLDecode(GetParam(args, "filter", True))
                Dim sComb As String = GetParam(args, "combination", True)
                ParseUsersAttributesFilter(sFilter, sComb)
                Dim NewGroup As clsCombinedGroup = PM.CombinedGroups.AddCombinedGroup(sName)
                NewGroup.Rule = GetCurrentFilterRuleXML()
                NewGroup.ApplyRules()
                PRJ.SaveStructure("Create new result group", , , String.Format("With rule, '{0}'", sName))
                If Not SelectedUsersAndGroupsIDs.Contains(NewGroup.CombinedUserID) Then StringSelectedUsersAndGroupsIDs += If(StringSelectedUsersAndGroupsIDs = "", "", ",") + NewGroup.CombinedUserID.ToString
                'tResult = String.Format("['{0}',{1}]", sAction, GetGroupsData())
                tResult = AllCallbackData(sAction, True, True)
            Case "get_filtered_users_data"
                Dim sFilter As String = URLDecode(GetParam(args, "filter", True))
                Dim sComb As String = GetParam(args, "combination", True)
                ParseUsersAttributesFilter(sFilter, sComb)
                tResult = String.Format("[""{0}"",{1}]", sAction, GetFilteredUsersData())
            Case "switch_show_priorities"
                Dim sValue As String = URLDecode(GetParam(args, "value", True))
                PM.CalculationsManager.ShowDueToPriorities = Str2Bool(sValue)
                tResult = AllCallbackData(sAction)
            Case "switch_obj_priorities_mode"
                Dim sValue As String = URLDecode(GetParam(args, "value", True))
                If Integer.TryParse(sValue, PM.Parameters.Synthesis_ObjectivesPrioritiesMode) Then 
                    PM.Parameters.Save()
                End If
                tResult = String.Format("[""{0}""]", sAction)
            'Case "dashboard_simple_mode"
            '    PM.Parameters.Dashboard_SimpleViewMode = Str2Bool(URLDecode(GetParam(args, "value")))
            '    PM.Parameters.Save()
            '    tResult = String.Format("[""{0}""]", sAction)
            Case "show_sim_results"
                Dim sValue As String = URLDecode(GetParam(args, "value", True))
                PM.CalculationsManager.UseSimulatedValues = Str2Bool(sValue)
                PM.Parameters.Save()
                tResult = AllCallbackData(sAction)
            Case "enable_alts"
                Dim fPrjChanged As Boolean = False
                Dim tAltID As Integer = CInt(URLDecode(GetParam(args, "id", True)))
                Dim tAltEnabled As Boolean = Str2Bool(URLDecode(GetParam(args, "enabled", True)))
                Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(tAltID)
                If tAlt IsNot Nothing Then
                    tAlt.Enabled = tAltEnabled
                    fPrjChanged = True
                End If
                If fPrjChanged Then
                    ' save project users
                    PM.StorageManager.Writer.SaveModelStructure()
                End If
                tResult = AllCallbackData(sAction)
' D6111 ===
                ' D6111 ==
        End Select

        'PM.Parameters.RiskSensitivityParameter = 0

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    ' -D6928
    'Public Property AltsSortMode As Integer
    '    Get
    '        Return PM.Parameters.PerformanceAltsSortMode
    '    End Get
    '    Set(value As Integer)
    '        PM.Parameters.PerformanceAltsSortMode = Value
    '        PM.Parameters.Save()
    '    End Set
    'End Property

    Private Function getNextUID(keep As Boolean, availList As List(Of Integer), ByRef lastID As Integer, initValue As Integer) As Integer
        Dim retVal As Integer = initValue
        If Not keep Then
            If lastID < availList.Count Then
                retVal = availList(lastID)
                lastID += 1
            Else
                retVal = availList(0)
                lastID = 1
            End If
        End If
        Return retVal
    End Function

    Private Sub SavePipeParameters()
        PRJ.SaveProjectOptions("Update overall results settings", , False, "")
    End Sub

    Private Function SACallbackData(sAction As String) As String
        ' D4243 ===
        ignoreCallback = False
        Dim sRes As String = String.Format("[""{0}"",{1},{2}]", sAction, GetSAObjectives(), GetSAAlternatives())
        ignoreCallback = True
        Return sRes
        ' D4243
    End Function

    Private Function AllCallbackData(sAction As String, Optional fRefreshGroups As Boolean = False, Optional fRefreshHierarchy As Boolean = False) As String        
        Dim forceComputed As Boolean = _PAGESLIST_RISK_SENSITIVITIES.Contains(CurrentPageID)
        Dim oldComputedMode As Boolean = PM.CalculationsManager.UseSimulatedValues
        If forceComputed Then PM.CalculationsManager.UseSimulatedValues = False

        ' D4243 ===
        ignoreCallback = False
        'Dim tOldUseSimulatedValues As Boolean = PM.CalculationsManager.UseSimulatedValues 'A1613
        Dim sRes As String = String.Format("[""{0}"",{1},{2},{3},{4},{5},[{6}],{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},""{18}"",{19},{20},{21},{22}]", sAction, GetSAObjectives(), GetSAAlternatives(), Api.get_alternatives_and_objectves_priorities(), "[]", AlternativesFilterValue, StringSelectedUsersAndGroupsIDs, "[]", GetASAData(), GetSAComponentsData(), "{}", "{}", "{}", "{}", "{}", "{}", If(fRefreshHierarchy, Api.GetCVTreeNodesData(IsMixedOrSensitivityView(), ResString("lblTreeNodeNoSources")), "[]"), "{}", JS_SafeString(If(SAUserID >= 0, PM.GetUserByID(SAUserID).UserName, PM.CombinedGroups.GetCombinedGroupByUserID(SAUserID).Name)), SAUserID, CInt(PM.Parameters.Normalization), If(fRefreshGroups, String.Format("{0}", Api.GetGroupsData()), "{}"), If(sAction = ACTION_CIS_MODE, Api.GetCVTreeNodesData(IsMixedOrSensitivityView(), ResString("lblTreeNodeNoSources")), "[]"))
        'If PM.IsRiskProject Then PM.CalculationsManager.UseSimulatedValues = ShowResultsMode = ShowResultsModes.rmSimulated 'A1613
        'If PM.IsRiskProject Then PM.CalculationsManager.UseSimulatedValues = tOldUseSimulatedValues 'A1613
        ignoreCallback = True

        If forceComputed Then PM.CalculationsManager.UseSimulatedValues = oldComputedMode

        Return sRes
        ' D4243
    End Function

    Public Function GetHeatMapUrl() As String
        Dim pgID As Integer = _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS
        If PM.ActiveHierarchy = ECHierarchyID.hidImpact Then pgID = _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS
        'todo Pass WRT Node ID
        Return PageURL(pgID, String.Format("widget=yes&{0}={1}", _PARAM_TEMP_THEME, _THEME_SL))
    End Function

    Public Function GetBowTieUrl() As String
        Dim pgID As Integer = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES
        If PM.ActiveHierarchy = ECHierarchyID.hidImpact Then pgID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS
        'todo Pass WRT Node ID
        'Return String.Format("{0}&wrt={1}", PageURL(pgID, GetTempThemeURI(True)), SAWRTNodeID)
        Return PageURL(pgID, String.Format("widget=yes&{0}={1}", _PARAM_TEMP_THEME, _THEME_SL))
    End Function

    Private Sub SynthesisPage_Unload(sender As Object, e As EventArgs) Handles Me.Unload
        If App.HasActiveProject Then    ' D4955
            'PM.Parameters.RiskSensitivityParameter = 0
            PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.DoNotUse
        End If
    End Sub

End Class