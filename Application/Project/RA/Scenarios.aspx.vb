Imports System.Data
Imports System.Drawing
Imports System.IO
Imports Canvas.RAGlobalSettings
Imports Telerik.Web.UI

Partial Class RAScenariosPage
    Inherits clsComparionCorePage

    Public Const COL_INDEX As Integer = 0
    Public Const COL_ALT_ID As Integer = 1
    Public Const COL_SORT As Integer = 2
    Public Const COL_NAME As Integer = 3
    Public Const COL_SCENARIOS As Integer = 4

    Public Const ROW_DESCRIPTION As Integer = 0
    Public Const ROW_BUDGET As Integer = 1
    Public Const ROW_BENEFIT As Integer = 2
    Public Const ROW_COST As Integer = 3
    Public Const ROW_SOLVER_MSG As Integer = 4
    Public Const ROW_CONSTR_ENABLED As Integer = 5
    Public Const ROW_CONSTR_MIN As Integer = 6
    Public Const ROW_CONSTR_MAX As Integer = 7
    Public Const ROW_CONSTR_FUNDED As Integer = 8
    Public Const ROW_CONSTR_TOTAL As Integer = 9
    Public Const ROW_FIELD_TITLE As Integer = 10
    Public Const ROW_ALTERNATIVES_START As Integer = 11

    Public Const OPT_SHOW_AS_IGNORES As Boolean = True
    Public Const OPT_PRECISION As Integer = 4
    Public Const OPT_PRECISION_PERCENT As Integer = 2
    Public Const OPT_SHOW_IGNORE_LINKS As Boolean = False

    Public Const OPT_BAR_WIDTH As Integer = 50
    Public Const OPT_BAR_HEIGHT As Integer = 2
    Public Const OPT_BAR_COLOR_FILLED As String = "#8899cc"
    Public Const OPT_BAR_COLOR_EMPTY As String = "#d0d0d0"
    Public Const OPT_BLANK_PIE_COLOR As String = "#ffffff"
    Public Const OPT_EXCLUDED_PIE_COLOR As String = "#dcdcdc"
    Public Const OPT_PIE_BORDERS As Boolean = False

    Public Const OPT_MIN_PIE_PLOT_SIZE As Integer = 150
    Public Const OPT_MAX_PIE_PLOT_SIZE As Integer = 500

    Private IsExport As Boolean = False
    Private PlotData As String = ""
    Private FUNDED_CELL_FILL_COLOR As Color = Color.FromArgb(209, 230, 181) 'Color.FromArgb(255, 255, 150)
    Private SUMMARY_HEADER_COLOR As Color = Color.Teal 'Drawing.Color.CadetBlue
    Private SUMMARY_HEADER_BACKGROUND As Color = Color.FromArgb(240, 245, 255)
    Private WHITE_BACKGROUND As Color = Color.FromArgb(255, 255, 255)
    Private CUSTOM_CONSTRAINT_HEADER_COLOR As Color = Color.Peru 'Drawing.Color.DarkOrange

    Private Const FUNDED_CELL_BKG_FILLED As Boolean = True
    Private Const HEADER_SEPARATOR As Char = CChar(vbTab)
    Private Const HEADER_ID_SEPARATOR As String = "{65BFC}"
    Private Const SESS_RA_SHOWBARS As String = "RA_ShowTinyBars" 'A0912
    Private Const SESS_RA_SHOW_EXCLUDED_ALTS As String = "RA_ShowExcludedAlts" 'A0915

    Private Const HTML_CHECKMARK_THIN As String = "&#10003;"
    Private Const HTML_CHECKMARK_THICK As String = "&#10004;"

    Private Const DBL_EXCLUDED_ALT_MARK As Double = Double.PositiveInfinity
    Private IMG_EXCLUDED_ALT_MARK As String = "../../images/ra/discard_tiny.gif"
    Private CHAR_EXCLUDED_ALT_MARK As String = "X"

    Public ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property

    Public ReadOnly Property BaseScenario As RAScenario
        Get
            Return RA.Scenarios.Scenarios(0)
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public Const SESS_RA_SCROLLING As String = "RA_Scrolling"

    Public Property RA_Scrolling As Boolean
        Get
            If Session(SESS_RA_SCROLLING) Is Nothing Then Return GetCookie(SESS_RA_SCROLLING, False.ToString) = True.ToString Else Return CBool(Session(SESS_RA_SCROLLING))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SCROLLING) = value
            SetCookie(SESS_RA_SCROLLING, value.ToString)
        End Set
    End Property

    Private ReadOnly Property SESS_RA_DISPLAY_FIELD As String
        Get
            Return String.Format("RA_DISPLAY_FIELD_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Enum DisplayFields
        dfFunded = 0
        dfBenefit = 1
        dfEBenefit = 2
        dfPFailure = 3      ' D4357
        dfCost = 4
        dfMust = 5
        dfMustNot = 6
        dfCustomConstraints = 7
        dfFundingByCategory = 8
        dfObjPriorities = 9
        dfDependencies = 10
        dfGroups = 11
        dfMustsAndMustNots = 12
        dfRisk = 14             ' D4357
        dfPSuccess = 15         ' D4357
    End Enum

    Public Enum FundedFilteringModes
        ffAll = 0
        ffFunded = 1
        ffPartiallyFunded = 2
        ffFullyFunded = 3
        ffNotFunded = 4
    End Enum

    Public Enum ViewModes
        vmGridView = 0
        vmColumnChart = 1
        vmPieChart = 2
        vmDonutChart = 3
    End Enum

    Public Enum GroupingModes
        gmScenario = 0
        gmAlternative = 1
    End Enum

    Public Enum ObjPriotityModes
        pmLocal = 0
        pmGlobal = 1
    End Enum

    Public Property DisplayField As DisplayFields
        Get
            Dim s = SessVar(SESS_RA_DISPLAY_FIELD)
            Dim retVal As DisplayFields = DisplayFields.dfFunded
            If Not String.IsNullOrEmpty(s) Then retVal = CType(CInt(s), DisplayFields)
            Return retVal
        End Get
        Set(value As DisplayFields)
            SessVar(SESS_RA_DISPLAY_FIELD) = CStr(CInt(value))
        End Set
    End Property

    Private ReadOnly Property SESS_RA_SORT_EXPR As String
        Get
            Return String.Format("RA_SORT_EXPR_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property RASortColumn As Integer
        Get
            ' D3188 ===
            If String.IsNullOrEmpty(CStr(Session(SESS_RA_SORT_EXPR))) Then
                Dim Fld As Integer = Math.Abs(CInt(RA.Scenarios.GlobalSettings.SortBy))
                Select Case Fld
                    Case raColumnID.Name
                        Return COL_NAME
                    Case Else
                        Return COL_INDEX
                End Select
            Else
                Return CInt(Session(SESS_RA_SORT_EXPR))
            End If
            ' D3188 ==
        End Get
        Set(value As Integer)
            Session(SESS_RA_SORT_EXPR) = value
            ' D3188 ===
            If value = COL_INDEX OrElse value = COL_NAME Then
                Dim Fld As Integer
                Select Case value
                    Case COL_NAME
                        Fld = raColumnID.Name
                    Case Else
                        Fld = raColumnID.ID
                End Select
                If RASortDirection = "DESC" Then Fld = -Fld
                RA.Scenarios.GlobalSettings.SortBy = CType(Fld, raColumnID)
            End If
            ' D3188 ==
        End Set
    End Property

    Private ReadOnly Property SESS_RA_SORT_DIR As String
        Get
            Return String.Format("RA_SORT_DIREC_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property RASortDirection As String
        Get
            ' D3188 ===
            If String.IsNullOrEmpty(CStr(Session(SESS_RA_SORT_DIR))) Then
                If (CInt(RA.Scenarios.GlobalSettings.SortBy)) < 0 Then
                    Return "DESC"
                Else
                    Return "ASC"
                End If
            Else
                Return CStr(Session(SESS_RA_SORT_DIR))
            End If
            ' D3188 ==
        End Get
        Set(value As String)
            Session(SESS_RA_SORT_DIR) = value
            ' D3223 ===
            If RASortColumn = COL_INDEX OrElse RASortColumn = COL_NAME Then
                RA.Scenarios.GlobalSettings.SortBy = CType(CInt(IIf(value.ToLower = "desc", -1, 1)) * Math.Round(RA.Scenarios.GlobalSettings.SortBy), raColumnID)
            End If
            ' D3223 ==
        End Set
    End Property

    Private ReadOnly Property SESS_RA_SCENARIOS_COLUMNS As String
        Get
            Return String.Format("RA_SCENARIOS_COLUMNS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Private _ScenariosColumns As List(Of Integer) = Nothing
    Public Property ScenariosColumns As List(Of Integer)
        Get
            Dim tSess As Object = Session(SESS_RA_SCENARIOS_COLUMNS)
            If tSess Is Nothing Then Return _ScenariosColumns Else Return CType(Session(SESS_RA_SCENARIOS_COLUMNS), List(Of Integer))
        End Get
        Set(value As List(Of Integer))
            Session(SESS_RA_SCENARIOS_COLUMNS) = value
        End Set
    End Property

    'ReadOnly Property SESSION_IS_GRAPHICAL_MODE As String
    '    Get
    '        Return String.Format("RA_ScenarioComparisonGraphicalMode_{0}", App.ProjectID)
    '    End Get
    'End Property

    'Public Property IsGraphicalMode As Boolean
    '    Get
    '        Return SessVar(SESSION_IS_GRAPHICAL_MODE) = "1"
    '    End Get
    '    Set(value As Boolean)
    '        SessVar(SESSION_IS_GRAPHICAL_MODE) = CStr(IIf(value, "1", "0"))
    '    End Set
    'End Property

    ReadOnly Property SESSION_SELECTED_CATEGORY_GUID As String
        Get
            Return String.Format("RA_SelecteCategoryGuid_{0}", App.ProjectID)
        End Get
    End Property

    Public Property SelectedCategoryID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s = SessVar(SESSION_SELECTED_CATEGORY_GUID)
            If Not String.IsNullOrEmpty(s) Then retVal = New Guid(s)
            Return retVal
        End Get
        Set(value As Guid)
            SessVar(SESSION_SELECTED_CATEGORY_GUID) = value.ToString
        End Set
    End Property

    ReadOnly Property SESSION_FUNDED_FILTERING_MODE As String 'A0888
        Get
            Return String.Format("RA_FundedFilteringMode_{0}", App.ProjectID)
        End Get
    End Property

    Public Property FundedFilteringMode As FundedFilteringModes
        Get
            Dim retVal As FundedFilteringModes = FundedFilteringModes.ffFunded
            Dim s = SessVar(SESSION_FUNDED_FILTERING_MODE)
            If Not String.IsNullOrEmpty(s) Then retVal = CType(CInt(s), FundedFilteringModes)
            Return retVal
        End Get
        Set(value As FundedFilteringModes)
            SessVar(SESSION_FUNDED_FILTERING_MODE) = CInt(value).ToString
        End Set
    End Property

    ReadOnly Property SESSION_GRID_VIEW_MODE As String
        Get
            Return String.Format("RA_ScenariosGridViewMode_{0}", App.ProjectID)
        End Get
    End Property

    Public Property ViewMode As ViewModes 'Grid / Column Chart / Pie Chart / Donut Chart
        Get
            Dim retVal As ViewModes = ViewModes.vmGridView
            Dim s = SessVar(SESSION_GRID_VIEW_MODE)
            If Not String.IsNullOrEmpty(s) Then retVal = CType(CInt(s), ViewModes)
            If retVal <> ViewModes.vmGridView AndAlso retVal <> ViewModes.vmColumnChart AndAlso DisplayField <> DisplayFields.dfBenefit AndAlso DisplayField <> DisplayFields.dfEBenefit AndAlso DisplayField <> DisplayFields.dfPFailure AndAlso DisplayField <> DisplayFields.dfPSuccess AndAlso DisplayField <> DisplayFields.dfRisk AndAlso DisplayField <> DisplayFields.dfObjPriorities Then  ' D4357
                retVal = ViewModes.vmGridView
                SessVar(SESSION_GRID_VIEW_MODE) = CInt(retVal).ToString
            End If
            'If DisplayField = DisplayFields.dfObjPriorities AndAlso retVal = GridViewModes.gvmPieChart Then retVal = GridViewModes.gvmDonutChart
            Return retVal
        End Get
        Set(value As ViewModes)
            SessVar(SESSION_GRID_VIEW_MODE) = CInt(value).ToString
        End Set
    End Property

    ReadOnly Property SESSION_GROUPING_MODE As String
        Get
            Return String.Format("RA_ScenariosGroupingMode_{0}", App.ProjectID)
        End Get
    End Property

    Public Property GroupingMode As GroupingModes 'group by Scenario / Alternative
        Get
            Dim retVal As GroupingModes = GroupingModes.gmScenario
            Dim s = SessVar(SESSION_GROUPING_MODE)
            If Not String.IsNullOrEmpty(s) Then retVal = CType(CInt(s), GroupingModes)
            Return retVal
        End Get
        Set(value As GroupingModes)
            SessVar(SESSION_GROUPING_MODE) = CInt(value).ToString
        End Set
    End Property

    Public Property RA_ShowTinyBars As Boolean
        Get
            If Session(SESS_RA_SHOWBARS) Is Nothing Then Return GetCookie(SESS_RA_SHOWBARS, True.ToString) = True.ToString Else Return CBool(Session(SESS_RA_SHOWBARS)) ' D2893
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SHOWBARS) = value
            SetCookie(SESS_RA_SHOWBARS, value.ToString)    ' D2893
        End Set
    End Property

    ReadOnly Property SESSION_OBJ_PRTY_MODE As String
        Get
            Return String.Format("RA_ScenariosObjPrtyMode_{0}", App.ProjectID)
        End Get
    End Property

    Public Property ObjPriorityMode As ObjPriotityModes 'Local / Global
        Get
            Dim retVal As ObjPriotityModes = ObjPriotityModes.pmLocal
            Dim s = SessVar(SESSION_OBJ_PRTY_MODE)
            If Not String.IsNullOrEmpty(s) Then retVal = CType(CInt(s), ObjPriotityModes)
            Return retVal
        End Get
        Set(value As ObjPriotityModes)
            SessVar(SESSION_OBJ_PRTY_MODE) = CInt(value).ToString
        End Set
    End Property

    Public Property ShowExcludedAlts As Boolean 'show the alternatives which don't match the Funded/Not Funded/Part.Funded/etc. selection
        Get
            If Session(SESS_RA_SHOW_EXCLUDED_ALTS) IsNot Nothing Then Return CBool(Session(SESS_RA_SHOW_EXCLUDED_ALTS)) Else Return True
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SHOW_EXCLUDED_ALTS) = value
        End Set
    End Property

    Public AttributesList As New List(Of clsAttribute)

    Public Sub New()
        MyBase.New(_PGID_RA_SCENARIOS)
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        InitAttributesList()

        RA.Load()

        If Not IsPostBack AndAlso Not IsCallback Then
            RA.Scenarios.CheckModel() 'A1324

            InitToolBarResources()
            InitScenariosColumns()

            Solve()
        End If
    End Sub

    Private Sub InitToolBarResources()
        RadToolBarMain.FindItemByValue("export").Text = ResString("btnExport")
        RadToolBarMain.FindItemByValue("export").Visible = App.isExportAvailable    ' D3324
        RadToolBarMain.FindItemByValue("export").Attributes.Add("style", "margin-top:2px")
        'RadToolBarMain.FindItemByValue("lbl_display").Text = ResString("lblDisplay") + ":"
        'RadToolBarMain.FindItemByValue("lbl_mode").Text = ResString("lblMode") + ":"
        'RadToolBarMain.FindItemByValue("lbl_view").Text = ResString("btnView") + ":"
        'RadToolBarMain.FindItemByValue("lbl_ctgs").Text = ResString("lblRAAttributes") + ":"
        'RadToolBarMain.FindItemByValue("lbl_group_by").Text = ResString("lblGroupBy") + ":"
        RadToolBarMain.FindItemByText("Select_Scenarios").Text = ResString("lblSelectScenarios")
        If ShowDraftPages() Then    ' -D3785 +A1122
            RadToolBarMain.FindItemByText("Customize").Visible = True
        End If
    End Sub

    Private Sub InitScenariosColumns()
        'If ScenariosColumns Is Nothing Then
        ScenariosColumns = New List(Of Integer)
        If RA.Scenarios.Scenarios.Values.Count > 0 AndAlso RA.Scenarios.Scenarios.Values.Where(Function(sc) sc.IsCheckedCS).Count = 0 Then BaseScenario.IsCheckedCS = True
        For Each sc As RAScenario In RA.Scenarios.Scenarios.Values
            If sc.IsCheckedCS Then ScenariosColumns.Add(sc.ID)
        Next
        'Else
        'Dim i As Integer = 0
        'While i < ScenariosColumns.Count
        '    Dim id As Integer = ScenariosColumns(i)
        '    If Not RA.Scenarios.Scenarios.ContainsKey(id) Then
        '        ScenariosColumns.RemoveAt(i)
        '    Else
        '        i += 1
        '    End If
        'End While
        'End If
    End Sub

    Private Sub InitAttributesList()
        PM.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
        AttributesList.Clear()
        Dim HasAttributes As Boolean = PM.Attributes IsNot Nothing AndAlso PM.Attributes.AttributesList IsNot Nothing AndAlso PM.Attributes.AttributesList.Count > 0
        If HasAttributes Then
            PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, -1)
            For Each attr In PM.Attributes.GetAlternativesAttributes
                If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                    AttributesList.Add(attr)
                End If
            Next
        End If
    End Sub

    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tID As Integer In RA.Scenarios.Scenarios.Keys
            Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)
            sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("[{0},'{1}',{2},'{3}',{4}]", tID, JS_SafeString(sc.Name), JS_SafeNumber(CInt(IIf(sc.IsCheckedCS, 1, 0))), CStr(IIf(String.IsNullOrEmpty(sc.Description), "", JS_SafeString(sc.Description))), JS_SafeNumber(Math.Round(sc.Alternatives.Sum(Function(a) a.RiskOriginal), 5)))
        Next
        Return sRes
    End Function

    Public Function IsSingleScenarioChecked() As Boolean
        Return RA.Scenarios.Scenarios.Values.Where(Function(sc) sc.IsCheckedCS).Count = 1
    End Function

    ReadOnly Property GetSelectedViewName() As String
        Get
            Dim sMode As String = ""
            Select Case FundedFilteringMode
                Case FundedFilteringModes.ffAll
                    sMode = ResString("lblRAViewCatAll")
                Case FundedFilteringModes.ffFunded
                    sMode = ResString("lblRAViewCatFunded")
                Case FundedFilteringModes.ffPartiallyFunded
                    sMode = ResString("lblRAViewCatPartially")
                Case FundedFilteringModes.ffFullyFunded
                    sMode = ResString("lblRAViewCatFully")
                Case FundedFilteringModes.ffNotFunded
                    sMode = ResString("lblRAViewCatNotFunded")
            End Select
            Return sMode
        End Get
    End Property

    ReadOnly Property GetSelectedFieldName(Optional asTitle As Boolean = False) As String
        Get
            'Dim sGridModeDropdown As String = ""
            'If Not asTitle Then
            '    sGridModeDropdown += String.Format("<option value='{0}' {2}>{1}</option>", CInt(GridViewModes.gvmTableView), ResString("optRAGrid"), IIf(GridViewMode = GridViewModes.gvmTableView, " selected='selected' ", ""))
            '    'If DisplayField <> DisplayFields.dfObjPriorities Then
            '    sGridModeDropdown += String.Format("<option value='{0}' {2}>{1}</option>", CInt(GridViewModes.gvmPieChart), ResString("optRAPieChart"), IIf(GridViewMode = GridViewModes.gvmPieChart, " selected='selected' ", ""))
            '    'End If
            '    sGridModeDropdown += String.Format("<option value='{0}' {2}>{1}</option>", CInt(GridViewModes.gvmDonutChart), ResString("optRADonutChart"), IIf(GridViewMode = GridViewModes.gvmDonutChart, " selected='selected' ", ""))
            '    sGridModeDropdown = String.Format("<select id='cbGridViewMode' style='width:105px;margin-top:0px;margin-bottom:0px;margin-left:10px;' onchange='onSetGridViewMode(this.value);' {1}>{0}</select>", sGridModeDropdown, IIf(BaseScenario.AlternativesFull.Count = 0, " disabled='disabled' ", ""))
            'End If
            Dim sViewName As String = CStr(IIf(asTitle, "", IIf(ViewMode <> ViewModes.vmGridView, String.Format(" - {0} {1}", GetSelectedViewName(), ParseString("%%alternatives%%")), "")))

            Dim retVal As String = ""
            Select Case DisplayField
                Case DisplayFields.dfFunded
                    retVal = ResString("optFunded")
                Case DisplayFields.dfBenefit
                    retVal = ResString("lblBenefit") + sViewName
                Case DisplayFields.dfEBenefit
                    retVal = ResString("lblExpectedBenefit") + sViewName
                Case DisplayFields.dfPFailure
                    retVal = ResString("optPFailure") + sViewName   ' D4356
                    ' D4357 ===
                Case DisplayFields.dfPSuccess
                    retVal = ResString("optPSuccess") + sViewName
                Case DisplayFields.dfRisk
                    retVal = ResString("optRisk") + sViewName
                    ' D4357 ==
                Case DisplayFields.dfCost
                    retVal = ResString("optCost")
                Case DisplayFields.dfMust
                    retVal = ResString("optMust")
                Case DisplayFields.dfMustNot
                    retVal = ResString("optMustNot")
                Case DisplayFields.dfMustsAndMustNots
                    retVal = ResString("optMustsAndMustNots")
                Case DisplayFields.dfDependencies
                    retVal = ResString("optDependencies")
                Case DisplayFields.dfGroups
                    retVal = ResString("optGroups")
                Case DisplayFields.dfCustomConstraints
                    retVal = ResString("optCustomConstraints")
                Case DisplayFields.dfFundingByCategory
                    If asTitle Then ' main title
                        'retVal = ResString("optFundingByCategory")
                        retVal = String.Format(ResString("lblRAByCategory"), "").Trim
                    Else ' red text in the datatable
                        retVal = String.Format(ResString("lblRAByCategory"), GetSelectedViewName())
                    End If
                Case DisplayFields.dfObjPriorities
                    Dim sPrtySwitch As String = "" 'Local/Global priorities switch
                    sPrtySwitch += String.Format("<option value='{0}' {2}>{1}</option>", CInt(ObjPriotityModes.pmLocal), ResString("optRAObjLocal"), IIf(ObjPriorityMode = ObjPriotityModes.pmLocal, " selected='selected' ", ""))
                    sPrtySwitch += String.Format("<option value='{0}' {2}>{1}</option>", CInt(ObjPriotityModes.pmGlobal), ResString("optRAObjGlobal"), IIf(ObjPriorityMode = ObjPriotityModes.pmGlobal, " selected='selected' ", ""))
                    sPrtySwitch = String.Format("<select id='cbObjPrty' style='width:105px;margin-top:0px;margin-bottom:0px;margin-left:10px;' onchange='onSetObjPrtyMode(this.value);' {1}>{0}</select>", sPrtySwitch, IIf(RA.ProjectManager.Hierarchy(RA.ProjectManager.ActiveHierarchy).Nodes.Count < 1, " disabled='disabled' ", ""))
                    'retVal = ResString("optObjPriorities") + CStr(IIf(asTitle, "", sPrtySwitch + "&nbsp;&nbsp;" + ResString("btnView") + ":&nbsp;" + sGridModeDropdown))
                    retVal = CStr(IIf(ViewMode = ViewModes.vmPieChart, ResString("lblRACoveringObj"), ResString("optObjPriorities"))) + CStr(IIf(asTitle, "", sPrtySwitch))
            End Select
            Return CStr(IIf(retVal = "", "", String.Format(CStr(IIf(asTitle, "({0})", "{0}")), retVal)))
        End Get
    End Property

    Private Function HasOption(sID As String, tScenario As RAScenario) As Boolean
        Dim has As Boolean = False
        With tScenario
            Select Case sID
                Case "cbOptMusts"
                    has = .Alternatives.Sum(Function(a) CInt(IIf(a.Must, 1, 0))) > 0
                Case "cbOptMustNots"
                    has = .Alternatives.Sum(Function(a) CInt(IIf(a.MustNot, 1, 0))) > 0
                Case "cbOptConstraints"
                    has = .Constraints.Constraints.Count > 0
                Case "cbOptDependencies"
                    'has = .Dependencies.Dependencies.Count > 0
                    has = .Dependencies.HasData(.Alternatives, .Groups)  ' D3881
                Case "cbOptGroups", "cbBCGroups"
                    'has = .Groups.Groups.Count > 0
                    has = .Groups.HasData(.Alternatives)    ' D3881
                Case "cbOptFundingPools"
                    has = .FundingPools.Pools.Count > 0
                Case "cbOptRisks"
                    has = .Alternatives.Sum(Function(a) a.RiskOriginal) > 0
                Case "cbOptTimePeriods" ' D3824
                    has = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0    ' D3824 + D3841 + A1137 + D3943
            End Select
        End With
        Return has
    End Function

    Public Function GetOptionStyle(sID As String) As String
        Dim has As Boolean = False
        If IsSingleScenarioChecked() Then
            has = HasOption(sID, RA.Scenarios.ActiveScenario)
        Else
            For Each scenario As RAScenario In RA.Scenarios.Scenarios.Values.Where(Function(sc) sc.IsCheckedCS)
                If Not has Then has = HasOption(sID, scenario)
            Next
        End If
        If has Then Return "style='font-weight:bold;'" Else Return "style='color:#999'"
    End Function

    Public Function IsOptionChecked(sID As String, settings As RASettings) As Boolean
        Dim chk As Boolean = False
        With settings
            Select Case sID
                Case "cbOptMusts"
                    chk = .Musts
                Case "cbOptMustNots"
                    chk = .MustNots
                Case "cbOptConstraints"
                    chk = .CustomConstraints
                Case "cbOptDependencies"
                    chk = .Dependencies
                Case "cbOptGroups"
                    chk = .Groups
                Case "cbOptFundingPools"
                    chk = .FundingPools
                Case "cbOptRisks"
                    chk = .Risks
                Case "cbOptTimePeriods" ' D3824
                    chk = .TimePeriods  ' D3824
                    ' D3078 ===
                Case "cbBaseCase"
                    chk = CBool(IIf(OPT_SHOW_AS_IGNORES, Not .UseBaseCase, .UseBaseCase))
                Case "cbBCGroups"
                    chk = CBool(IIf(OPT_SHOW_AS_IGNORES, Not .BaseCaseForGroups, .BaseCaseForGroups))
                    ' D3078 ==
            End Select
        End With
        Return chk
    End Function

    Public Function GetOptionValue(sID As String) As String
        Dim chk As Boolean = False
        If Not chk Then chk = IsOptionChecked(sID, CType(IIf(IsSingleScenarioChecked(), RA.Scenarios.ActiveScenario.Settings, RA.Scenarios.GlobalSettings.ScenarioComparisonSettings), RASettings))
        If OPT_SHOW_AS_IGNORES Then chk = Not chk ' D2931
        If chk Then Return " checked" Else Return ""
    End Function

    Private SummaryDescriptions As New Dictionary(Of Integer, String)
    Private SolverMessages As New Dictionary(Of Integer, String)
    Private ConstraintsDifferences As New Dictionary(Of String, Boolean) 'String=AlternativesID, Boolean=(has any differences in constraints or groups)
    Private ConstraintsData As New List(Of ConstraintData) 'used for Constraints and Groups displays
    Private DependenciesData As New List(Of RADependency) 'used for Dependencies
    Private AltsByCategory As New Dictionary(Of Integer, Dictionary(Of Guid, List(Of RAAlternative))) ' Scenario Id / Category Guid / Alternatives Names    
    Private ScenarioSums As New Dictionary(Of Integer, Double) ' Scenario Id / Sum of Alternatives selected DisplayField

    Private Class ConstraintData
        Public ScenarioID As Integer

        Public ConstraintsCount As Integer = 0
        Public Col0ID As Integer 'index of the first column
        Public Col1ID As Integer 'index of the last first column

        'Public Min As Double = Double.MinValue
        'Public Max As Double = Double.MinValue
        'Public Funded As Double = Double.MinValue
        'Public Total As Double = Double.MinValue
    End Class

    Private ColumnIndex As Integer = -1

    Private Function NewDataColumn(ColumnName As String, ColumnType As Type) As DataColumn
        ColumnIndex += 1
        Return New DataColumn(ColumnName + HEADER_ID_SEPARATOR + ColumnIndex.ToString, ColumnType)
    End Function

    Private Function GetColumnName(ColumnName As String) As String
        Dim retVal As String = ColumnName
        Dim indOfUniqueID As Integer = ColumnName.IndexOf(HEADER_ID_SEPARATOR)
        If indOfUniqueID >= 0 Then
            retVal = ColumnName.Substring(0, indOfUniqueID)
        End If
        Return retVal
    End Function

    'Private Function GetAltByGuid(Alts As List(Of RAAlternative), Id As Guid) As RAAlternative
    '    Dim sId As String = Id.ToString
    '    For Each alt As RAAlternative In Alts
    '        If alt.ID = sId Then Return alt
    '    Next
    '    Return Nothing
    'End Function

    Private Sub Solve()
        ColumnIndex = -1

        Dim oldActiveScenarioID As Integer = RA.Scenarios.ActiveScenarioID

        SummaryDescriptions.Clear()
        SolverMessages.Clear()
        ConstraintsData.Clear()
        ConstraintsDifferences.Clear()
        DependenciesData.Clear()
        AltsByCategory.Clear()
        ScenarioSums.Clear()

        Dim DataTable As New DataTable()
        DataTable.Columns.Add(NewDataColumn(ResString("tblNo_"), GetType(Integer)))
        DataTable.Columns.Add(NewDataColumn("Alt ID", GetType(String))) 'hidden column
        DataTable.Columns.Add(NewDataColumn("Sort", GetType(Byte))) 'hidden column
        DataTable.Columns.Add(NewDataColumn(CStr(IIf(DisplayField = DisplayFields.dfFundingByCategory, ResString("lblRACategories"), IIf(DisplayField = DisplayFields.dfObjPriorities, ResString("tblRAObjectiveName"), IIf(DisplayField = DisplayFields.dfDependencies, ResString("tblDependencies"), ResString("tblAlternativeName"))))), GetType(String))) 'Name

        DataTable.Rows.Add(DataTable.NewRow()) 'summary description row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary budget row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary benefit row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary cost row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary solver message row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary constraint enabled row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary constraint min row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary constraint max row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary constraint funded row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary constraint total row
        DataTable.Rows.Add(DataTable.NewRow()) 'summary field title

        DataTable.Rows(ROW_DESCRIPTION)(COL_NAME) = ResString("tblRowDescription")
        DataTable.Rows(ROW_BUDGET)(COL_NAME) = ResString("tblRowBudget")
        DataTable.Rows(ROW_BENEFIT)(COL_NAME) = ResString("tblRowEffective") '"Benefit / Expected Benefit:"
        DataTable.Rows(ROW_COST)(COL_NAME) = ResString("tblRowFundedCost")
        DataTable.Rows(ROW_SOLVER_MSG)(COL_NAME) = ResString("tblRowSolverState")
        DataTable.Rows(ROW_CONSTR_ENABLED)(COL_NAME) = ResString("tblRowEnabled")
        DataTable.Rows(ROW_CONSTR_MIN)(COL_NAME) = ResString("tblRowMin")
        DataTable.Rows(ROW_CONSTR_MAX)(COL_NAME) = ResString("tblRowMax")
        DataTable.Rows(ROW_CONSTR_FUNDED)(COL_NAME) = ResString("tblRowFunded")
        DataTable.Rows(ROW_CONSTR_TOTAL)(COL_NAME) = ResString("tblRowTotal")

        If DisplayField = DisplayFields.dfGroups Then
            DataTable.Rows(ROW_CONSTR_FUNDED)(COL_NAME) = ResString("tblRowGroupCondition")
            DataTable.Rows(ROW_CONSTR_TOTAL)(COL_NAME) = ResString("tblRowGroupAltsCount")
        End If

        For i As Integer = 0 To ROW_ALTERNATIVES_START - 1
            DataTable.Rows(i)(COL_SORT) = i
        Next

        Dim tSelectedCategoryID As Guid = SelectedCategoryID
        Dim tSelectedCategoryEnum As clsAttributeEnumeration = Nothing
        If (tSelectedCategoryID.Equals(Guid.Empty) OrElse AttributesList.Where(Function(attr) attr.ID.Equals(tSelectedCategoryID)).Count = 0) AndAlso AttributesList.Count > 0 Then tSelectedCategoryID = AttributesList(0).ID
        Dim tSelectedAttr As clsAttribute = Nothing
        Dim tSelectedCategoryMultiValues As List(Of String) = New List(Of String)
        For Each attr In AttributesList
            If attr.ID = tSelectedCategoryID Then tSelectedAttr = attr
        Next
        If tSelectedAttr IsNot Nothing AndAlso Not tSelectedAttr.EnumID.Equals(Guid.Empty) Then tSelectedCategoryEnum = PM.Attributes.GetEnumByID(tSelectedAttr.EnumID)

        Dim rowsCreated As Boolean = False
        Dim j As Integer = COL_SCENARIOS

        PlotData = ""
        Dim SeriesLabels As String = ""
        Dim SeriesColors As Dictionary(Of Integer, List(Of String)) = New Dictionary(Of Integer, List(Of String)) ' Scenario ID / Series Colors
        Dim PlotLabels As String = ""
        Dim PlotSeries As Dictionary(Of Integer, String) = New Dictionary(Of Integer, String)
        'Dim PlotSeriesDonut As Dictionary(Of Integer, Dictionary(Of Integer, String)) = New Dictionary(Of Integer, Dictionary(Of Integer, String))
        Dim index As Integer = 0

        Dim isPlotDataGroupedByAlternative As Boolean = (ViewMode = ViewModes.vmPieChart OrElse ViewMode = ViewModes.vmDonutChart) OrElse (ViewMode = ViewModes.vmColumnChart AndAlso GroupingMode = GroupingModes.gmAlternative)

        'For Each kvp As KeyValuePair(Of Integer, RAScenario) In RA.Scenarios.Scenarios
        '    Dim scenario As RAScenario = kvp.Value
        '    scenario.ID = kvp.Key
        For Each scen_id As Integer In ScenariosColumns
            Dim scenario As RAScenario = Nothing
            If RA.Scenarios.Scenarios.ContainsKey(scen_id) Then
                scenario = RA.Scenarios.Scenarios(scen_id)
                scenario.ID = scen_id
            End If
            If scenario IsNot Nothing AndAlso (DisplayField <> DisplayFields.dfCustomConstraints OrElse scenario.Constraints.Constraints.Count > 0) AndAlso (DisplayField <> DisplayFields.dfGroups OrElse scenario.Groups.Groups.Count > 0) AndAlso (DisplayField <> DisplayFields.dfDependencies OrElse scenario.Dependencies.Dependencies.Count > 0) Then
                RA.Scenarios.ActiveScenarioID = scen_id
                RA.Solver.ResetSolver()

                Dim oldSettings As RASettings = New RASettings
                With RA.Scenarios.ActiveScenario.Settings
                    'save current settings
                    oldSettings.Musts = .Musts
                    oldSettings.MustNots = .MustNots
                    oldSettings.CustomConstraints = .CustomConstraints
                    oldSettings.Dependencies = .Dependencies
                    oldSettings.Groups = .Groups
                    oldSettings.FundingPools = .FundingPools
                    oldSettings.Risks = .Risks
                    oldSettings.TimePeriods = .TimePeriods  ' D3828
                    oldSettings.UseBaseCase = .UseBaseCase
                    oldSettings.BaseCaseForGroups = .BaseCaseForGroups

                    'use scenario comparison settings - ignores
                    If Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions Then
                        .Musts = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Musts
                        .MustNots = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.MustNots
                        .CustomConstraints = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.CustomConstraints
                        .Dependencies = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Dependencies
                        .Groups = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Groups
                        .FundingPools = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.FundingPools
                        .Risks = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Risks
                        .TimePeriods = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.TimePeriods   ' D3828
                    End If

                    'use scenario comparison settings - base case
                    If Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCaseOptions Then
                        .UseBaseCase = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCase
                        .BaseCaseForGroups = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.BaseCaseForGroups
                    End If

                    RA.UpdateBenefits()
                End With

                'Dim Alts As List(Of RAAlternative) = RA.Scenarios.ActiveScenario.Alternatives
                Dim BaseCaseMax As Double = -1
                If RA.Scenarios.ActiveScenario.Settings.UseBaseCase AndAlso RA.Scenarios.ActiveScenario.Settings.BaseCaseForGroups Then BaseCaseMax = RA.Solver.GetBaseCaseMaximum Else BaseCaseMax = -1

                'solve
                RA.Solve()

                If Not RA.Scenarios.ActiveScenario.Settings.UseBaseCase OrElse BaseCaseMax < 0 Then BaseCaseMax = RA.Solver.TotalBenefitOriginal 'A0918 + A0922

                SummaryDescriptions.Add(scenario.ID, CStr(IIf(String.IsNullOrEmpty(scenario.Description), "", scenario.Description)))
                SolverMessages.Add(scenario.ID, CStr(IIf(String.IsNullOrEmpty(SolverStateHTML(RA.Solver)), "&nbsp;", SolverStateHTML(RA.Solver))))  ' D3628
                ConstraintsData.Add(New ConstraintData With {.ScenarioID = scenario.ID})
                AltsByCategory.Add(scenario.ID, New Dictionary(Of Guid, List(Of RAAlternative)))
                ScenarioSums.Add(scenario.ID, 0)

                Select Case DisplayField
                    Case DisplayFields.dfFundingByCategory
                        If tSelectedAttr Is Nothing Then Exit Select

                        Dim scenarioColumn As DataColumn = NewDataColumn(SafeFormString(scenario.Name), GetType(Double))
                        DataTable.Columns.Add(scenarioColumn)

                        Select Case tSelectedAttr.ValueType
                            Case AttributeValueTypes.avtEnumeration
                                If Not rowsCreated Then
                                    Dim r_index As Integer = 1
                                    'Add rows for every enumeration item
                                    If tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items IsNot Nothing Then
                                        For i As Integer = 0 To tSelectedCategoryEnum.Items.Count - 1
                                            Dim item As clsAttributeEnumerationItem = tSelectedCategoryEnum.Items(i)
                                            Dim dr = DataTable.NewRow()
                                            dr(COL_INDEX) = i + 1
                                            dr(COL_ALT_ID) = item.ID.ToString
                                            dr(COL_SORT) = 255
                                            dr(COL_NAME) = SafeFormString(item.Value)
                                            DataTable.Rows.Add(dr)
                                            r_index += 1
                                        Next
                                    End If
                                    'Add a row for No Category
                                    Dim drNC = DataTable.NewRow()
                                    drNC(COL_INDEX) = r_index
                                    drNC(COL_ALT_ID) = Guid.Empty.ToString
                                    drNC(COL_SORT) = 255
                                    drNC(COL_NAME) = SafeFormString(ResString("lblNoCategory"))
                                    DataTable.Rows.Add(drNC)
                                End If
                                rowsCreated = True

                                'Fill in data (funding by category)
                                Dim cdata As New Dictionary(Of Guid, Integer)
                                cdata.Add(Guid.Empty, 0) 'no category 

                                For Each mAlt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                                    Dim attrVal As Object = PM.Attributes.GetAttributeValue(tSelectedCategoryID, mAlt.NodeGuidID)
                                    If attrVal Is Nothing Then attrVal = tSelectedAttr.DefaultValue
                                    Dim tAlt As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(mAlt.NodeGuidID.ToString)
                                    If tAlt IsNot Nothing Then
                                        Dim id As Guid = Guid.Empty
                                        If attrVal IsNot Nothing AndAlso TypeOf attrVal Is Guid Then id = CType(attrVal, Guid)
                                        Dim tValue As Integer = 1
                                        Select Case FundedFilteringMode
                                            Case FundedFilteringModes.ffFunded
                                                tValue = CInt(IIf(RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded > 0, 1, 0)) 'A0939
                                            Case FundedFilteringModes.ffPartiallyFunded
                                                tValue = CInt(IIf(RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.IsPartiallyFunded, 1, 0)) 'A0933
                                            Case FundedFilteringModes.ffFullyFunded
                                                tValue = CInt(IIf(RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded = 1, 1, 0)) 'A0939
                                            Case FundedFilteringModes.ffNotFunded
                                                tValue = CInt(IIf(RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded = 0, 1, 0)) 'A0939
                                        End Select
                                        If Not cdata.ContainsKey(id) Then cdata.Add(id, tValue) Else cdata(id) += tValue
                                        If Not AltsByCategory(scenario.ID).ContainsKey(id) Then AltsByCategory(scenario.ID).Add(id, New List(Of RAAlternative))
                                        If tValue > 0 Then AltsByCategory(scenario.ID)(id).Add(tAlt)
                                    End If
                                Next
                                For k As Integer = ROW_ALTERNATIVES_START To DataTable.Rows.Count - 1
                                    Dim id As Guid = New Guid(CStr(DataTable.Rows(k)(COL_ALT_ID)))
                                    If cdata.ContainsKey(id) Then
                                        DataTable.Rows(k)(j) = cdata(id)
                                    End If
                                Next

                            Case AttributeValueTypes.avtEnumerationMulti

                        End Select
                        'Dim mAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(altGuid)
                        'If mAlt IsNot Nothing Then
                        '    tColor = NoSpecificColor
                        'Dim attrVal As Object = PM.Attributes.GetAttributeValue(tSelectedCategoryID, altGuid)

                        'If attrVal IsNot Nothing AndAlso tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items.Count > 0 Then
                        '    Select Case tSelectedAttr.ValueType
                        '        Case AttributeValueTypes.avtEnumeration
                        '            Dim item As clsAttributeEnumerationItem = Nothing
                        '            If TypeOf attrVal Is String Then item = tSelectedCategoryEnum.GetItemByID(New Guid(CStr(attrVal)))
                        '            If TypeOf attrVal Is Guid Then item = tSelectedCategoryEnum.GetItemByID(CType(attrVal, Guid))
                        '            If item IsNot Nothing Then
                        '                Dim index As Integer = tSelectedCategoryEnum.Items.IndexOf(item)
                        '                If index >= 0 Then
                        '                    sCategory = item.ID.ToString
                        '                    tColor = ColorPalette(index Mod ColorPalette.Count)
                        '                End If
                        '            End If
                        '        Case AttributeValueTypes.avtEnumerationMulti
                        '            Dim sVal As String = CStr(attrVal)
                        '            If Not String.IsNullOrEmpty(sVal) Then
                        '                sCategory = sVal
                        '                If Not tSelectedCategoryMultiValues.Contains(sVal) Then tSelectedCategoryMultiValues.Add(sVal)
                        '                tColor = ColorPalette(tSelectedCategoryMultiValues.IndexOf(sVal) Mod ColorPalette.Count)
                        '            End If
                        '    End Select
                        'End If
                        'End If
                    Case DisplayFields.dfDependencies
                        Dim scenarioColumn As DataColumn = NewDataColumn(SafeFormString(scenario.Name), GetType(Double))
                        DataTable.Columns.Add(scenarioColumn)

                        Dim depDColor As String = "SlateBlue"
                        Dim depMColor As String = "MediumAquaMarine"
                        Dim depXColor As String = "LightCoral"

                        Dim lblAnd As String = ResString("lblAnd")
                        Dim lblAre As String = ResString("lblAre")

                        For Each dep As RADependency In scenario.Dependencies.Dependencies
                            Dim tDep As RADependency = dep
                            If DependenciesData.Count = 0 OrElse DependenciesData.Where(Function(d) d.FirstAlternativeID.Equals(tDep.FirstAlternativeID) AndAlso d.SecondAlternativeID.Equals(tDep.SecondAlternativeID) AndAlso d.Value.Equals(tDep.Value)).Count = 0 Then
                                DependenciesData.Add(dep)

                                Dim depTitle As String = ""
                                Dim depTitleFormat As String = "<div style='float:left; width:20px; height:20px; text-align:center; padding-top:3px; margin-top:1px; background-color:{1};'><font color='white'><b>{2}</b></font></div>&nbsp;<span style='display:inline-block; margin-top:4px; margin-bottom:3px;'>{0}</span>"

                                Dim alt1name As String = SafeFormString(RA.GetAlternativeById(BaseScenario, dep.FirstAlternativeID).Name)
                                Dim alt2name As String = SafeFormString(RA.GetAlternativeById(BaseScenario, dep.SecondAlternativeID).Name)

                                Select Case dep.Value
                                    Case RADependencyType.dtDependsOn
                                        Dim depname As String = String.Format("<font color='{1}'>{0}</font>", getDependencyName(dep.Value), depDColor)
                                        depTitle = String.Format("<b>{0}</b> {1} <b>{2}</b>", alt1name, depname, alt2name)
                                        depTitle = String.Format(depTitleFormat, depTitle, depDColor, "D")
                                    Case RADependencyType.dtMutuallyDependent
                                        Dim depname As String = String.Format("<font color='{1}'>{0}</font>", getDependencyName(dep.Value), depMColor)
                                        depTitle = String.Format("<b>{0}</b> <font color='{5}'>{1}</font> <b>{2}</b> <font color='{5}'>{3}</font> {4}", alt1name, lblAnd, alt2name, lblAre, depname, depMColor)
                                        depTitle = String.Format(depTitleFormat, depTitle, depMColor, "M")
                                    Case RADependencyType.dtMutuallyExclusive
                                        Dim depname As String = String.Format("<font color='{1}'>{0}</font>", getDependencyName(dep.Value), depXColor)
                                        depTitle = String.Format("<b>{0}</b> <font color='{5}'>{1}</font> <b>{2}</b> <font color='{5}'>{3}</font> {4}", alt1name, lblAnd, alt2name, lblAre, depname, depXColor)
                                        depTitle = String.Format(depTitleFormat, depTitle, depXColor, "X")
                                End Select

                                'add a new row for each dependency
                                Dim dr = DataTable.NewRow()
                                dr(COL_INDEX) = DependenciesData.Count
                                dr(COL_ALT_ID) = Guid.Empty.ToString
                                dr(COL_SORT) = 254
                                dr(COL_NAME) = depTitle
                                DataTable.Rows.Add(dr)
                            End If
                            Dim depCreated As RADependency = DependenciesData.Where(Function(d) d.FirstAlternativeID.Equals(tDep.FirstAlternativeID) AndAlso d.SecondAlternativeID.Equals(tDep.SecondAlternativeID) AndAlso d.Value.Equals(tDep.Value))(0)
                            DataTable.Rows(ROW_ALTERNATIVES_START + DependenciesData.IndexOf(depCreated))(j) = 1
                        Next
                    Case DisplayFields.dfCustomConstraints
                        ConstraintsData.Last.Col0ID = DataTable.Columns.Count
                        ConstraintsData.Last.ConstraintsCount = RA.Scenarios.ActiveScenario.Constraints.Constraints.Count
                        For Each constraint As RAConstraint In RA.Scenarios.ActiveScenario.Constraints.Constraints.Values
                            Dim constrColumn As DataColumn = NewDataColumn(SafeFormString(scenario.Name) + HEADER_SEPARATOR + SafeFormString(Constraint.Name), GetType(Double))
                            DataTable.Columns.Add(constrColumn)

                            Dim CFunded As Double = 0
                            If RA.Solver.SolverState = raSolverState.raSolved Then CFunded = RA.Solver.FundedConstraint(constraint.ID)

                            DataTable.Rows(ROW_CONSTR_ENABLED)(DataTable.Columns.Count - 1) = CInt(IIf(constraint.Enabled, 1, 0))
                            DataTable.Rows(ROW_CONSTR_MIN)(DataTable.Columns.Count - 1) = constraint.MinValue
                            DataTable.Rows(ROW_CONSTR_MAX)(DataTable.Columns.Count - 1) = constraint.MaxValue
                            DataTable.Rows(ROW_CONSTR_FUNDED)(DataTable.Columns.Count - 1) = CFunded
                            DataTable.Rows(ROW_CONSTR_TOTAL)(DataTable.Columns.Count - 1) = constraint.TotalCost(RA.Scenarios.ActiveScenario.Alternatives)  ' D3884

                            For i As Integer = 0 To BaseScenario.AlternativesFull.Count - 1
                                Dim defAlt As RAAlternative = BaseScenario.AlternativesFull(i)

                                If Not rowsCreated Then
                                    Dim dr = DataTable.NewRow()
                                    dr(COL_SORT) = 255
                                    If defAlt IsNot Nothing Then
                                        dr(COL_INDEX) = defAlt.SortOrder 'i + 1
                                        dr(COL_ALT_ID) = defAlt.ID
                                        dr(COL_NAME) = SafeFormString(defAlt.Name)
                                    Else ' alt not found
                                        dr(COL_INDEX) = i + 1
                                        dr(COL_ALT_ID) = Guid.Empty.ToString
                                        dr(COL_NAME) = "-"
                                    End If
                                    DataTable.Rows.Add(dr)
                                End If

                                Dim alt As RAAlternative = scenario.GetAvailableAlternativeById(defAlt.ID)
                                If CStr(DataTable.Rows(ROW_ALTERNATIVES_START + i)(COL_ALT_ID)) = defAlt.ID Then 'to make sure the row is for this alternative
                                    If alt IsNot Nothing Then
                                        If constraint.AlternativesData.ContainsKey(alt.ID) Then
                                            If constraint.AlternativesData(alt.ID) <> UNDEFINED_INTEGER_VALUE Then DataTable.Rows(ROW_ALTERNATIVES_START + i)(DataTable.Columns.Count - 1) = constraint.AlternativesData(alt.ID) 'A0961
                                        End If
                                    Else
                                        DataTable.Rows(ROW_ALTERNATIVES_START + i)(DataTable.Columns.Count - 1) = DBL_EXCLUDED_ALT_MARK
                                    End If
                                End If
                            Next
                            rowsCreated = True
                        Next
                        ConstraintsData.Last.Col1ID = DataTable.Columns.Count - 1
                        CheckConstraintsDifferences()
                    Case DisplayFields.dfGroups
                        ConstraintsData.Last.Col0ID = DataTable.Columns.Count
                        ConstraintsData.Last.ConstraintsCount = RA.Scenarios.ActiveScenario.Groups.Groups.Count
                        For Each group As RAGroup In RA.Scenarios.ActiveScenario.Groups.Groups.Values
                            Dim groupColumn As DataColumn = NewDataColumn(SafeFormString(scenario.Name) + HEADER_SEPARATOR + SafeFormString(Group.Name), GetType(Double))
                            DataTable.Columns.Add(groupColumn)

                            DataTable.Rows(ROW_CONSTR_ENABLED)(DataTable.Columns.Count - 1) = CInt(IIf(group.Enabled, 1, 0))
                            DataTable.Rows(ROW_CONSTR_FUNDED)(DataTable.Columns.Count - 1) = CInt(group.Condition)
                            DataTable.Rows(ROW_CONSTR_TOTAL)(DataTable.Columns.Count - 1) = group.Alternatives.Count

                            For i As Integer = 0 To BaseScenario.AlternativesFull.Count - 1
                                Dim defAlt As RAAlternative = BaseScenario.AlternativesFull(i)
                                If Not rowsCreated Then
                                    Dim dr = DataTable.NewRow()
                                    dr(COL_SORT) = 255
                                    If defAlt IsNot Nothing Then
                                        dr(COL_INDEX) = defAlt.SortOrder 'i + 1
                                        dr(COL_ALT_ID) = defAlt.ID
                                        dr(COL_NAME) = SafeFormString(defAlt.Name)
                                    Else ' alt not found
                                        dr(COL_INDEX) = i + 1
                                        dr(COL_ALT_ID) = Guid.Empty.ToString
                                        dr(COL_NAME) = "-"
                                    End If
                                    DataTable.Rows.Add(dr)
                                End If

                                Dim alt As RAAlternative = scenario.GetAvailableAlternativeById(defAlt.ID)
                                If CStr(DataTable.Rows(ROW_ALTERNATIVES_START + i)(COL_ALT_ID)) = defAlt.ID Then 'to make sure the row is for this alternative
                                    If alt IsNot Nothing Then
                                        If group.Alternatives.ContainsKey(alt.ID) Then
                                            DataTable.Rows(ROW_ALTERNATIVES_START + i)(DataTable.Columns.Count - 1) = 1
                                        End If
                                    Else
                                        DataTable.Rows(ROW_ALTERNATIVES_START + i)(DataTable.Columns.Count - 1) = DBL_EXCLUDED_ALT_MARK
                                    End If
                                End If
                            Next
                            rowsCreated = True
                        Next
                        ConstraintsData.Last.Col1ID = DataTable.Columns.Count - 1
                        CheckGroupsDifferences()
                    Case DisplayFields.dfObjPriorities
                        Dim scenarioColumn As DataColumn = NewDataColumn(SafeFormString(scenario.Name), GetType(Double))
                        DataTable.Columns.Add(scenarioColumn)

                        Dim plotValue As String = ""

                        Dim H As clsHierarchy = RA.ProjectManager.Hierarchy(RA.ProjectManager.ActiveHierarchy)
                        Dim NodesInLinearOrder As List(Of Tuple(Of Integer, Integer, clsNode)) = H.NodesInLinearOrder
                        For i As Integer = 0 To NodesInLinearOrder.Count - 1
                            Dim tObj As clsNode = NodesInLinearOrder(i).Item3
                            If Not rowsCreated Then
                                Dim dr = DataTable.NewRow()
                                dr(COL_INDEX) = i + 1
                                dr(COL_SORT) = 255
                                If tObj IsNot Nothing Then
                                    dr(COL_ALT_ID) = tObj.NodeGuidID.ToString
                                    dr(COL_NAME) = String.Concat(Enumerable.Repeat("&nbsp;", 4 * tObj.Level)) + SafeFormString(tObj.NodeName)
                                Else ' Objective not found                                    
                                    dr(COL_ALT_ID) = Guid.Empty.ToString
                                    dr(COL_NAME) = "-"
                                End If
                                DataTable.Rows.Add(dr)
                                Dim tObjName = tObj.NodeName
                                If ViewMode <> ViewModes.vmPieChart AndAlso ViewMode <> ViewModes.vmDonutChart Then tObjName = ShortString(tObj.NodeName, 20, True)
                                If tObj.Level > 0 AndAlso ((ViewMode = ViewModes.vmColumnChart AndAlso GroupingMode = GroupingModes.gmAlternative) OrElse (ViewMode = ViewModes.vmPieChart AndAlso tObj.IsTerminalNode) OrElse ViewMode = ViewModes.vmDonutChart) Then PlotLabels += CStr(IIf(PlotLabels = "", "", ",")) + String.Format("'{0}'", JS_SafeString(tObjName))
                            End If

                            If CStr(DataTable.Rows(ROW_ALTERNATIVES_START + i)(COL_ALT_ID)) = tObj.NodeGuidID.ToString Then 'to make sure the row is for this objective
                                Select Case ObjPriorityMode
                                    Case ObjPriotityModes.pmLocal
                                        Dim tObjLocalPriority As Single = tObj.LocalPriority(PM.CalculationsManager.GetCalculationTargetByUserID(COMBINED_USER_ID), PM.ActiveObjectives.GetNodeByID(NodesInLinearOrder(i).Item2))
                                        DataTable.Rows(ROW_ALTERNATIVES_START + i)(DataTable.Columns.Count - 1) = tObjLocalPriority
                                        plotValue = JS_SafeNumber(Math.Round(tObjLocalPriority * 100, 2))
                                        ScenarioSums(scen_id) += tObjLocalPriority * 100
                                    Case ObjPriotityModes.pmGlobal
                                        Dim tObjGlobalPriority As Single = tObj.WRTGlobalPriority
                                        DataTable.Rows(ROW_ALTERNATIVES_START + i)(DataTable.Columns.Count - 1) = tObjGlobalPriority
                                        plotValue = JS_SafeNumber(Math.Round(tObjGlobalPriority * 100, 2))
                                        ScenarioSums(scen_id) += tObjGlobalPriority * 100
                                End Select

                                Select Case ViewMode
                                    Case ViewModes.vmPieChart
                                        If tObj.IsTerminalNode Then
                                            'Data Pie Chart - covering objectives only
                                            If Not PlotSeries.ContainsKey(scen_id) Then
                                                PlotSeries.Add(scen_id, "")
                                            End If

                                            PlotSeries(scen_id) += CStr(IIf(PlotSeries(scen_id) = "", "", ",")) + String.Format("['{0}%',{0}]", plotValue)
                                            If i = NodesInLinearOrder.Count - 1 Then PlotSeries(scen_id) += CStr(IIf(PlotSeries(scen_id) = "", "", ",")) + String.Format("['',BlankVal{0}]", scen_id) 'Fake objective
                                        End If

                                    Case ViewModes.vmDonutChart
                                        'Data Hierarchical Donut Chart - all nodes
                                        If Not PlotSeries.ContainsKey(scen_id) Then
                                            PlotSeries.Add(scen_id, "")
                                        End If
                                        If tObj.ParentNode Is Nothing Then
                                            PlotSeries(scen_id) = String.Format("['{0}',{1},{2},{3}]", SafeFormString(tObj.NodeName), plotValue, tObj.NodeID, tObj.ParentNodeID)
                                        Else
                                            PlotSeries(scen_id) += CStr(IIf(PlotSeries(scen_id) = "", "", ",")) + String.Format("['{0}%',{0},{1},{2}]", plotValue, tObj.NodeID, tObj.ParentNodeID)
                                        End If
                                    Case ViewModes.vmColumnChart
                                        If tObj.Level > 0 Then
                                            'Data for Column Chart
                                            If Not isPlotDataGroupedByAlternative Then 'group by scenario
                                                If Not PlotSeries.ContainsKey(i) Then
                                                    PlotSeries.Add(i, "")
                                                    SeriesLabels += CStr(IIf(SeriesLabels = "", "", ",")) + String.Format("{{label:'{0}'}}", JS_SafeString(ShortString(tObj.NodeName, 20, True)))
                                                End If

                                                PlotSeries(i) += CStr(IIf(PlotSeries(i) = "", "", ",")) + String.Format("{0}", plotValue)
                                            Else 'group by objective
                                                If Not PlotSeries.ContainsKey(scen_id) Then
                                                    PlotSeries.Add(scen_id, "")
                                                    SeriesLabels += CStr(IIf(SeriesLabels = "", "", ",")) + String.Format("{{label:'{0}'}}", JS_SafeString(scenario.Name))
                                                End If

                                                PlotSeries(scen_id) += CStr(IIf(PlotSeries(scen_id) = "", "", ",")) + String.Format("{0}", plotValue)
                                            End If
                                        End If
                                End Select

                            End If
                        Next
                    Case Else
                        Dim scenarioColumn As DataColumn = NewDataColumn(SafeFormString(scenario.Name), GetType(Double))
                        DataTable.Columns.Add(scenarioColumn)
                        Dim AltsCount As Integer = BaseScenario.AlternativesFull.Count

                        For i As Integer = 0 To AltsCount - 1
                            Dim defAlt As RAAlternative = BaseScenario.AlternativesFull(i)
                            Dim alt As RAAlternative = scenario.GetAvailableAlternativeById(defAlt.ID)
                            Dim altGuid As Guid = New Guid(defAlt.ID)

                            If Not rowsCreated Then
                                Dim dr = DataTable.NewRow()
                                dr(COL_SORT) = 255
                                If defAlt IsNot Nothing Then
                                    dr(COL_INDEX) = defAlt.SortOrder 'i + 1
                                    dr(COL_ALT_ID) = defAlt.ID
                                    dr(COL_NAME) = SafeFormString(defAlt.Name)
                                Else
                                    dr(COL_NAME) = "-"
                                End If
                                DataTable.Rows.Add(dr)

                                If isPlotDataGroupedByAlternative Then
                                    Dim tAltName = defAlt.Name
                                    If ViewMode <> ViewModes.vmPieChart AndAlso ViewMode <> ViewModes.vmDonutChart Then tAltName = ShortString(defAlt.Name, 20, True)
                                    PlotLabels += CStr(IIf(PlotLabels = "", "", ",")) + String.Format("'{0}'", JS_SafeString(tAltName))
                                    If ViewMode <> ViewModes.vmColumnChart AndAlso i = AltsCount - 1 Then
                                        PlotLabels += CStr(IIf(PlotLabels = "", "", ",")) + "" 'Fake alternative
                                    End If
                                End If
                            End If

                            For k As Integer = ROW_ALTERNATIVES_START To DataTable.Rows.Count - 1 'loop all the rows because the order of the alternatives may be different in each scenario
                                If CStr(DataTable.Rows(k)(COL_ALT_ID)) = defAlt.ID Then 'to make sure the row is for this alternative
                                    Dim plotValue As String = ""
                                    Dim plotDblValue As Double = 0
                                    Dim tableValue As Object = Nothing
                                    Select Case DisplayField
                                        Case DisplayFields.dfFunded
                                            If alt IsNot Nothing Then
                                                Dim fIsFunded As Boolean = alt.DisplayFunded > 0 AndAlso RA.Solver.SolverState = raSolverState.raSolved 'A0939
                                                tableValue = CDbl(IIf(fIsFunded, alt.DisplayFunded, 0)) 'A0939
                                                'plotValue = CStr(IIf(fIsFunded, IIf(alt.Funded >= 1, "'3" + ResString("lblYes") + "'", "'2" + ResString("lblPartFunded") + "'"), "'1" + ResString("lblNo") + "'"))
                                                plotValue = CStr(IIf(fIsFunded, IIf(alt.DisplayFunded >= 1, "3", "2"), "1")) 'A0939
                                            Else
                                                plotValue = "0"
                                            End If
                                        Case DisplayFields.dfBenefit
                                            If alt IsNot Nothing Then
                                                tableValue = alt.BenefitOriginal
                                                plotDblValue = alt.BenefitOriginal * 100
                                            Else
                                                plotDblValue = 0
                                            End If
                                            plotValue = JS_SafeNumber(Math.Round(plotDblValue, 2))
                                            ScenarioSums(scen_id) += plotDblValue
                                        Case DisplayFields.dfEBenefit
                                            If alt IsNot Nothing Then
                                                tableValue = alt.Benefit
                                                plotDblValue = alt.Benefit * 100
                                            Else
                                                plotDblValue = 0
                                            End If
                                            plotValue = JS_SafeNumber(Math.Round(plotDblValue, 2))
                                            ScenarioSums(scen_id) += plotDblValue
                                        Case DisplayFields.dfPFailure
                                            If alt IsNot Nothing Then
                                                tableValue = alt.RiskOriginal
                                                plotDblValue = alt.RiskOriginal * 100
                                            Else
                                                plotDblValue = 0
                                            End If
                                            plotValue = JS_SafeNumber(Math.Round(plotDblValue, 2))
                                            ScenarioSums(scen_id) += plotDblValue
                                            ' D4357 ===
                                        Case DisplayFields.dfPSuccess
                                            If alt IsNot Nothing Then
                                                tableValue = (1 - alt.RiskOriginal)
                                                plotDblValue = (1 - alt.RiskOriginal) * 100
                                            Else
                                                plotDblValue = 0
                                            End If
                                            plotValue = JS_SafeNumber(Math.Round(plotDblValue, 2))
                                            ScenarioSums(scen_id) += plotDblValue
                                        Case DisplayFields.dfRisk
                                            If alt IsNot Nothing Then
                                                tableValue = alt.Risk
                                                plotDblValue = alt.Risk * 100
                                            Else
                                                plotDblValue = 0
                                            End If
                                            plotValue = JS_SafeNumber(Math.Round(plotDblValue, 2))
                                            ScenarioSums(scen_id) += plotDblValue
                                            ' D4357 ==
                                        Case DisplayFields.dfCost
                                            If alt IsNot Nothing Then
                                                tableValue = alt.Cost
                                                plotValue = JS_SafeNumber(alt.Cost) 'A0933
                                            Else
                                                plotValue = "0"
                                            End If
                                        Case DisplayFields.dfMust
                                            If alt IsNot Nothing Then
                                                tableValue = CInt(IIf(alt.Must, 1, 0))
                                                'plotValue = CStr(IIf(alt.Must, "'" + ResString("lblMust") + "'", "'" + ResString("lblNotSet") + "'"))
                                                plotValue = CStr(IIf(alt.Must, "2", "1"))
                                            Else
                                                plotValue = "0"
                                            End If
                                        Case DisplayFields.dfMustNot
                                            If alt IsNot Nothing Then
                                                tableValue = CInt(IIf(alt.MustNot, 1, 0))
                                                'plotValue = CStr(IIf(alt.MustNot, "'" + ResString("lblMustNot") + "'", "'" + ResString("lblNotSet") + "'"))
                                                plotValue = CStr(IIf(alt.MustNot, "2", "1"))
                                            Else
                                                plotValue = "0"
                                            End If
                                        Case DisplayFields.dfMustsAndMustNots
                                            Dim res As Integer = 0
                                            If alt IsNot Nothing Then
                                                If alt.Must AndAlso alt.MustNot Then res = 2
                                                If alt.Must AndAlso Not alt.MustNot Then res = 1
                                                If Not alt.Must AndAlso alt.MustNot Then res = -1
                                            End If
                                            tableValue = res
                                            plotValue = CStr(res)
                                    End Select

                                    If ViewMode = ViewModes.vmGridView AndAlso (alt Is Nothing OrElse tableValue IsNot Nothing) Then DataTable.Rows(k)(j) = IIf(alt IsNot Nothing, tableValue, DBL_EXCLUDED_ALT_MARK)
                                    Dim isAltExcluded As Boolean = False

                                    If RA.Solver.SolverState = raSolverState.raSolved Then
                                        If (alt Is Nothing OrElse (FundedFilteringMode = FundedFilteringModes.ffFunded) AndAlso Not (alt.DisplayFunded > 0)) OrElse _
                                            (FundedFilteringMode = FundedFilteringModes.ffPartiallyFunded AndAlso Not (alt.IsPartiallyFunded)) OrElse _
                                            (FundedFilteringMode = FundedFilteringModes.ffFullyFunded AndAlso Not (alt.DisplayFunded = 1)) OrElse _
                                            (FundedFilteringMode = FundedFilteringModes.ffNotFunded AndAlso Not (alt.DisplayFunded = 0)) Then 'A0933 'A0939
                                            isAltExcluded = True
                                            If alt IsNot Nothing AndAlso (Not ShowExcludedAlts OrElse ViewMode = ViewModes.vmColumnChart) Then
                                                plotValue = "0"
                                                ScenarioSums(scen_id) -= plotDblValue
                                            End If
                                        End If
                                    End If

                                    Dim fundedPercent As String = ""
                                    Dim fundedCost As String = ""

                                    If FundedFilteringMode = FundedFilteringModes.ffPartiallyFunded AndAlso RA.Solver.SolverState = raSolverState.raSolved AndAlso alt IsNot Nothing AndAlso alt.IsPartiallyFunded Then
                                        fundedPercent = JS_SafeString(CStr(Math.Round(alt.Funded * 100, 2)) + "%")
                                        fundedCost = JS_SafeString(CostString(alt.Funded * alt.Cost))
                                    End If

                                    If Not isPlotDataGroupedByAlternative Then 'group by scenario
                                        If Not PlotSeries.ContainsKey(i) Then
                                            PlotSeries.Add(i, "")
                                            SeriesLabels += CStr(IIf(SeriesLabels = "", "", ",")) + String.Format("{{label:'{0}'}}", JS_SafeString(defAlt.Name))
                                        End If

                                        PlotSeries(i) += CStr(IIf(PlotSeries(i) = "", "", ",")) + String.Format("{0}", plotValue)

                                        If Not SeriesColors.ContainsKey(i) Then
                                            SeriesColors.Add(i, New List(Of String))
                                        End If

                                        'Dim tColor As String = GetAlternativeColor(PM, BaseScenario.AlternativesFull.IndexOf(defAlt), altGuid)
                                        Dim tColor As String = GetAlternativeColor(PM, k - ROW_ALTERNATIVES_START, altGuid)

                                        If ViewMode <> ViewModes.vmColumnChart Then
                                            If isAltExcluded AndAlso ShowExcludedAlts Then
                                                tColor = OPT_EXCLUDED_PIE_COLOR
                                            End If
                                        End If

                                        SeriesColors(i).Add(String.Format("'{0}'", tColor))
                                    Else 'group by alternative / Pie Charts
                                        If Not PlotSeries.ContainsKey(scen_id) Then
                                            PlotSeries.Add(scen_id, "")
                                            SeriesLabels += CStr(IIf(SeriesLabels = "", "", ",")) + String.Format("{{label:'{0}'}}", JS_SafeString(scenario.Name))
                                        End If

                                        If Not SeriesColors.ContainsKey(scen_id) Then
                                            SeriesColors.Add(scen_id, New List(Of String))
                                        End If

                                        'Dim tColor As String = GetAlternativeColor(PM, BaseScenario.AlternativesFull.IndexOf(defAlt), altGuid)
                                        Dim tColor As String = GetAlternativeColor(PM, k - ROW_ALTERNATIVES_START, altGuid)

                                        If ViewMode <> ViewModes.vmColumnChart Then
                                            If isAltExcluded AndAlso ShowExcludedAlts Then
                                                tColor = OPT_EXCLUDED_PIE_COLOR
                                            End If
                                        End If

                                        SeriesColors(scen_id).Add(String.Format("'{0}'", tColor))
                                        PlotSeries(scen_id) += CStr(IIf(PlotSeries(scen_id) = "", "", ",")) + String.Format(CStr(IIf(ViewMode <> ViewModes.vmColumnChart, "['{0}%',{0},'{1}%','{2}']", "{0}")), plotValue, fundedPercent, fundedCost)

                                        If ViewMode <> ViewModes.vmColumnChart AndAlso i = AltsCount - 1 Then
                                            PlotSeries(scen_id) += CStr(IIf(PlotSeries(scen_id) = "", "", ",")) + String.Format("['',BlankVal{0},'','']", scen_id)
                                            SeriesColors(scen_id).Add(String.Format("'{0}'", OPT_BLANK_PIE_COLOR))
                                        End If
                                    End If
                                End If
                            Next
                        Next
                End Select

                rowsCreated = True

                Dim tCellBenefit As Double = 0
                If BaseCaseMax > 0 Then tCellBenefit = (100 * RA.Solver.FundedBenefit) / BaseCaseMax 'A0918

                If j < DataTable.Columns.Count Then
                    DataTable.Rows(ROW_DESCRIPTION)(j) = IIf(SummaryDescriptions(scenario.ID) = "", DBNull.Value, 1)
                    DataTable.Rows(ROW_BUDGET)(j) = scenario.Budget
                    If RA.Solver.SolverState = raSolverState.raSolved Then
                        DataTable.Rows(ROW_BENEFIT)(j) = tCellBenefit
                        DataTable.Rows(ROW_COST)(j) = RA.Solver.FundedCost
                    Else
                        DataTable.Rows(ROW_BENEFIT)(j) = DBNull.Value
                        DataTable.Rows(ROW_COST)(j) = DBNull.Value
                    End If
                End If

                'j += CInt(IIf(DisplayField <> DisplayFields.dfCustomConstraints, 1, RA.Scenarios.ActiveScenario.Constraints.Constraints.Count))
                Select Case DisplayField
                    Case DisplayFields.dfCustomConstraints
                        j += RA.Scenarios.ActiveScenario.Constraints.Constraints.Count
                    Case DisplayFields.dfGroups
                        j += RA.Scenarios.ActiveScenario.Groups.Groups.Count
                    Case Else
                        j += 1
                End Select

                'PlotLabels += CStr(IIf(PlotLabels = "", "", ",")) + String.Format("'{0}'", JS_SafeString(scenario.Name + "<br />Budget: " + JS_SafeNumber(scenario.Budget.ToString("F2")) + "<br />Benefit: " + JS_SafeNumber(Math.Round(tCellBenefit, 4)) + "<br />Cost:" + JS_SafeNumber(_sumCost.ToString("F2"))))
                If ViewMode = ViewModes.vmColumnChart AndAlso GroupingMode = GroupingModes.gmScenario Then
                    PlotLabels += CStr(IIf(PlotLabels = "", "", ",")) + String.Format("'{0}'", JS_SafeString(ShortString(scenario.Name, 20, True)))
                End If

                With RA.Scenarios.ActiveScenario.Settings
                    'restore current settions
                    .Musts = oldSettings.Musts
                    .MustNots = oldSettings.MustNots
                    .CustomConstraints = oldSettings.CustomConstraints
                    .Dependencies = oldSettings.Dependencies
                    .Groups = oldSettings.Groups
                    .FundingPools = oldSettings.FundingPools
                    .Risks = oldSettings.Risks
                    .TimePeriods = oldSettings.TimePeriods  ' D3828
                    .UseBaseCase = oldSettings.UseBaseCase
                    .BaseCaseForGroups = oldSettings.BaseCaseForGroups
                    RA.UpdateBenefits()
                End With
            End If
            'index += 1
        Next

        Dim PlotSeriesString As String = ""

        'If Not IsGraphicalMode AndAlso DisplayField = DisplayFields.dfObjPriorities AndAlso GridViewMode <> GridViewModes.gvmTableView Then ' Objectives Donut Chart
        '    For Each item As Dictionary(Of Integer, String) In PlotSeriesDonut.Values
        '        Dim seriesData As String = String.Join("],[", item.Values)
        '        PlotSeriesString += CStr(IIf(PlotSeriesString = "", "", ",")) + String.Format("[{0}]", seriesData)
        '    Next
        '    PlotSeriesString = String.Format("[{0}]", PlotSeriesString)
        'Else
        For Each item As String In PlotSeries.Values
            PlotSeriesString += CStr(IIf(PlotSeriesString = "", "", ",")) + String.Format("[{0}]", item)
        Next
        'End If

        If DataTable.Rows.Count = ROW_ALTERNATIVES_START AndAlso DisplayField <> DisplayFields.dfFundingByCategory AndAlso DisplayField <> DisplayFields.dfDependencies Then 'no data to display
            DataTable.Rows.Clear()
            DataTable.Columns.Clear()
        End If

        ' generate gridview columns
        GridAlternatives.Columns.Clear()
        For Each col As DataColumn In DataTable.Columns
            Dim bfield As BoundField = New BoundField()
            bfield.DataField = col.ColumnName
            bfield.HeaderText = GetColumnName(col.ColumnName)
            bfield.HtmlEncode = False
            GridAlternatives.Columns.Add(bfield)
        Next

        Dim DS As DataView = New DataView(DataTable)

        If DisplayField <> DisplayFields.dfCustomConstraints AndAlso DisplayField <> DisplayFields.dfGroups AndAlso DisplayField <> DisplayFields.dfDependencies AndAlso DisplayField <> DisplayFields.dfObjPriorities AndAlso Not (DisplayField = DisplayFields.dfFundingByCategory AndAlso RASortColumn > COL_NAME) AndAlso RASortColumn >= 0 AndAlso Not String.IsNullOrEmpty(RASortDirection) AndAlso DataTable.Columns.Count > 0 AndAlso DataTable.Columns.Count > RASortColumn Then
            DS.Sort = "Sort" + HEADER_ID_SEPARATOR + COL_SORT.ToString + " ASC, " + DataTable.Columns(RASortColumn).ColumnName + " " + RASortDirection
        End If

        GridAlternatives.DataSource = DS
        GridAlternatives.DataBind()

        Dim yAxisLabel As String = ""
        Dim yAxisCategorical As String = "0"
        Dim yAxisTicks As String = "[]"

        Select Case DisplayField
            Case DisplayFields.dfFunded
                yAxisLabel = ResString("optFunded")
                'yAxisCategorical = "1"
                yAxisTicks = String.Format("[[0.8,''],[1,'{0}'],[2,'{1}'],[3,'{2}'],[3.2,'']]", ResString("lblNo"), ResString("lblPartFunded"), ResString("lblYes"))
            Case DisplayFields.dfBenefit
                yAxisLabel = ResString("lblBenefit")
            Case DisplayFields.dfEBenefit
                yAxisLabel = ResString("lblExpectedBenefit")
            Case DisplayFields.dfPFailure
                yAxisLabel = ResString("optPFailure")   ' D4356
                ' D4357 ===
            Case DisplayFields.dfPSuccess
                yAxisLabel = ResString("optPSuccess")
            Case DisplayFields.dfRisk
                yAxisLabel = ResString("optRisk")
                ' D4357 ==
            Case DisplayFields.dfCost
                yAxisLabel = ResString("optCost")
            Case DisplayFields.dfMust
                yAxisLabel = ResString("optMust")
                'yAxisCategorical = "1"
                yAxisTicks = String.Format("[[0.5,''],[1,'{0}'],[2,'{1}'],[2.5,'']]", ResString("lblNotSet"), ResString("lblMust"))
            Case DisplayFields.dfMustNot
                yAxisLabel = ResString("optMustNot")
                'yAxisCategorical = "1"
                yAxisTicks = String.Format("[[0.5,''],[1,'{0}'],[2,'{1}'],[2.5,'']]", ResString("lblNotSet"), ResString("lblMustNot"))
            Case DisplayFields.dfMustsAndMustNots
                yAxisLabel = ResString("optMustsAndMustNots")
                yAxisTicks = String.Format("[[-1.2,''],[-1,'{0}'],[0,'{1}'],[1,'{2}'],[1.2,'']]", ResString("lblMustNot"), ResString("lblNotSet"), ResString("lblMust"))
            Case DisplayFields.dfCustomConstraints
                yAxisLabel = ResString("optCustomConstraints")
            Case DisplayFields.dfFundingByCategory
                yAxisLabel = ResString("optFundingByCategory")
            Case DisplayFields.dfGroups
                yAxisLabel = ResString("optGroups")
            Case DisplayFields.dfDependencies
                yAxisLabel = ResString("optDependencies")
        End Select

        RA.Scenarios.ActiveScenarioID = oldActiveScenarioID
        RA.Solver.ResetSolver()

        If ViewMode <> ViewModes.vmGridView Then
            'normalize pie/donut charts
            If ViewMode = ViewModes.vmPieChart OrElse ViewMode = ViewModes.vmDonutChart Then
                Dim max As Double = Double.MinValue
                If ScenarioSums.Values.Count > 0 Then max = ScenarioSums.Values.Max(Function(s) s)
                For Each v As KeyValuePair(Of Integer, Double) In ScenarioSums
                    PlotSeriesString = PlotSeriesString.Replace(String.Format("BlankVal{0}", v.Key), JS_SafeNumber(Math.Round(max - v.Value, 2)))
                Next
            End If
            'prepare series colors
            Dim SeriesColorsString As String = ""
            If SeriesColors.Count > 0 Then
                If ViewMode = ViewModes.vmColumnChart Then
                    If GroupingMode = GroupingModes.gmAlternative Then
                        SeriesColorsString = String.Join(",", SeriesColors.Values(0).ToArray)
                    Else
                        For Each s As List(Of String) In SeriesColors.Values
                            If s.Count > 0 Then SeriesColorsString += CStr(IIf(SeriesColorsString = "", "", ",")) + String.Format("{0}", s(0))
                        Next
                    End If
                Else
                    'Pie / Donut
                    For Each s As List(Of String) In SeriesColors.Values
                        SeriesColorsString += CStr(IIf(SeriesColorsString = "", "", ",")) + String.Format("[{0}]", String.Join(",", s.ToArray))
                    Next
                End If

            End If
            PlotData = String.Format("[[{0}],[{1}],[{2}],'{3}',{4},{5},[{6}]]", PlotLabels, SeriesLabels, PlotSeriesString, yAxisLabel, yAxisCategorical, yAxisTicks, SeriesColorsString)
        End If
    End Sub

    Private Function getDependencyName(dType As RADependencyType) As String
        Select Case dType
            Case RADependencyType.dtDependsOn
                Return ResString("lblDependsOn")
            Case RADependencyType.dtMutuallyDependent
                Return ResString("lblMutuallyDependent")
            Case RADependencyType.dtMutuallyExclusive
                Return ResString("lblMutuallyExclusive")
        End Select
        Return ""
    End Function

    Private Sub CheckConstraintsDifferences()
        ConstraintsDifferences.Clear()
        If DisplayField = DisplayFields.dfCustomConstraints AndAlso ScenariosColumns.Count > 1 Then
            Dim scenario0index As Integer = 0
            Dim scenario0 As RAScenario = RA.Scenarios.Scenarios(ScenariosColumns(scenario0index))

            While scenario0.Constraints.Constraints.Count = 0 AndAlso scenario0index < ScenariosColumns.Count
                scenario0index += 1
            End While

            If scenario0index < ScenariosColumns.Count - 1 Then
                Dim Alts As List(Of RAAlternative) = BaseScenario.AlternativesFull
                For Each alt As RAAlternative In Alts
                    Dim has_differences As Boolean = False

                    For i As Integer = scenario0index To ScenariosColumns.Count - 1
                        Dim scenarioI As RAScenario = RA.Scenarios.Scenarios(ScenariosColumns(i))
                        If scenarioI.Constraints.Constraints.Count > 0 Then
                            For Each constr0 As RAConstraint In scenario0.Constraints.Constraints.Values
                                Dim alt0Val As Double = constr0.GetAlternativeValue(alt.ID)
                                Dim constrI As RAConstraint = scenarioI.Constraints.GetConstraintByName(constr0.Name)
                                If (constrI Is Nothing AndAlso alt0Val <> UNDEFINED_INTEGER_VALUE) OrElse (constrI IsNot Nothing AndAlso alt0Val <> constrI.GetAlternativeValue(alt.ID)) Then has_differences = True
                                If has_differences Then Exit For
                            Next
                            For Each constrI As RAConstraint In scenarioI.Constraints.Constraints.Values
                                Dim altIVal As Double = constrI.GetAlternativeValue(alt.ID)
                                Dim constr0 As RAConstraint = scenario0.Constraints.GetConstraintByName(constrI.Name)
                                If (constr0 Is Nothing AndAlso altIVal <> UNDEFINED_INTEGER_VALUE) OrElse (constr0 IsNot Nothing AndAlso altIVal <> constr0.GetAlternativeValue(alt.ID)) Then has_differences = True
                                If has_differences Then Exit For
                            Next
                        End If
                        If has_differences Then Exit For
                    Next

                    ConstraintsDifferences.Add(alt.ID, has_differences)
                Next
            End If
        End If
    End Sub

    Private Sub CheckGroupsDifferences()
        ConstraintsDifferences.Clear()
        If DisplayField = DisplayFields.dfGroups AndAlso ScenariosColumns.Count > 1 Then
            Dim scenario0index As Integer = 0
            Dim scenario0 As RAScenario = RA.Scenarios.Scenarios(ScenariosColumns(scenario0index))

            While scenario0.Groups.Groups.Count = 0 AndAlso scenario0index < ScenariosColumns.Count
                scenario0index += 1
            End While

            If scenario0index < ScenariosColumns.Count - 1 Then
                Dim Alts As List(Of RAAlternative) = BaseScenario.AlternativesFull
                For Each alt As RAAlternative In Alts
                    Dim has_differences As Boolean = False

                    For i As Integer = scenario0index To ScenariosColumns.Count - 1
                        Dim scenarioI As RAScenario = RA.Scenarios.Scenarios(ScenariosColumns(i))
                        If scenarioI.Groups.Groups.Count > 0 Then
                            For Each group0 As RAGroup In scenario0.Groups.Groups.Values
                                Dim alt0Val As Boolean = group0.Alternatives.ContainsKey(alt.ID)
                                Dim groupI As RAGroup = scenarioI.Groups.GetGroupByName(group0.Name)
                                If (groupI Is Nothing AndAlso alt0Val) OrElse (groupI IsNot Nothing AndAlso alt0Val <> groupI.Alternatives.ContainsKey(alt.ID)) Then has_differences = True
                                If has_differences Then Exit For
                            Next
                            For Each groupI As RAGroup In scenarioI.Groups.Groups.Values
                                Dim altIVal As Boolean = groupI.Alternatives.ContainsKey(alt.ID)
                                Dim group0 As RAGroup = scenario0.Groups.GetGroupByName(groupI.Name)
                                If (group0 Is Nothing AndAlso altIVal) OrElse (group0 IsNot Nothing AndAlso altIVal <> group0.Alternatives.ContainsKey(alt.ID)) Then has_differences = True
                                If has_differences Then Exit For
                            Next
                        End If
                        If has_differences Then Exit For
                    Next

                    ConstraintsDifferences.Add(alt.ID, has_differences)
                Next
            End If
        End If
    End Sub

    'A0912 ===
    Private Function AddBar(sText As String, tVal As Double, Optional fLight As Boolean = False, Optional fBarWidth As Integer = OPT_BAR_WIDTH) As String
        Dim sVal As String = sText
        If RA_ShowTinyBars Then   ' D2843 + D2880
            If tVal > 1 Then tVal = 1
            If tVal < 0 Then tVal = 0
            Dim L = Math.Floor(tVal * fBarWidth)
            Dim sImg As String = String.Format("<img src='{0}' width='{{0}}' height={1} title='' border=0>", BlankImage, OPT_BAR_HEIGHT)    ' D3006
            sVal += String.Format(CStr(IIf(sVal = "", "", "<br>")) + CStr(IIf(L > 0, "<span style='display:inline-block; line-height:{2}px; height:{2}px; width:{0}px; border-bottom:{2}px solid {3}; margin-top:-{2}px;'>" + CStr(IIf(IsExport, "", String.Format(sImg, L))) + "</span>", "")) + CStr(IIf(L < fBarWidth, "<span style='display:inline-block; line-height:{2}px; height:{2}px; width:{1}px; border-bottom:{2}px solid {4}; margin-top:-{2}px;'>" + CStr(IIf(IsExport, "", String.Format(sImg, fBarWidth - L))) + "</span>", "")), L, fBarWidth - L, OPT_BAR_HEIGHT, OPT_BAR_COLOR_FILLED, IIf(fLight, "#ffffff", OPT_BAR_COLOR_EMPTY))    ' D2843 + D3006
        End If
        Return sVal
    End Function
    'A0912 ==

    Private Function GetSortCaption(idx As Integer, sName As String, mnu As String) As String
        If DisplayField = DisplayFields.dfCustomConstraints OrElse DisplayField = DisplayFields.dfGroups OrElse DisplayField = DisplayFields.dfDependencies OrElse DisplayField = DisplayFields.dfObjPriorities OrElse (DisplayField = DisplayFields.dfFundingByCategory AndAlso idx >= COL_SCENARIOS) Then Return sName + mnu
        Dim sExtra As String = ""
        Dim Dir As Integer = CInt(IIf(RASortDirection = "ASC", SortDirection.Descending, SortDirection.Ascending))
        If RASortColumn = idx Then
            sExtra = CStr(IIf(RASortDirection = "DESC", _SORT_DESC, _SORT_ASC))
        Else
            Dir = CInt(IIf(Dir = SortDirection.Ascending, SortDirection.Descending, SortDirection.Ascending))
        End If
        Return String.Format(CStr(IIf(Not IsExport, "<a href='' onclick='DoSort({0},{1}); return false;' class='actions'>{2}{3}</a>{4}", "{2}{3}")), idx, Dir, ShortString(sName, 80, True), sExtra, mnu)
    End Function

    'A0910 + A0919 + D3267 ===
    Private Sub CopyPasteHeaderButtons(Cells As TableCellCollection, idx As Integer)    ' D3185 + A0930 + A0937
        If Not IsExport Then
            Dim Cell As TableCell = Cells(idx)      ' D3185

            ' AD: version when icon exactly at the end of caption text
            'Dim sp_idx As Integer = Cell.Text.LastIndexOf(CChar(" "))
            'If sp_idx > 0 AndAlso Not Cell.Text.Substring(sp_idx + 1).Trim.ToLower.StartsWith("class='") Then Cell.Text.Insert(sp_idx + 1, "<nobr>") Else Cell.Text = "<nobr>" + Cell.Text
            'Cell.Text = String.Format("{4}<img style='cursor:context-menu; padding-left:3px;' id='mnu_img_{0}' src='{1}menu_dots.png' alt='' onclick='showMenu(event, ""{0}"", {2}, {3});' /></nobr>", tUniqueID, ImagePath, IIf(HasCopyButton, "true", "false"), IIf(HasPasteButton, "true", "false"), Cell.Text)

            'AD: version when icon aligner at the right, but caption enclosed to the table
            Cell.Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:100%'><tr valign='middle' class='grid_clear'>" + _
                                      "<td class='text' align='center' width='99%'>{2}</td>" + _
                                      "<td style='width:15px' align='right'><img src='{1}menu_dots.png' width='12' height='12' style='cursor:context-menu; padding-left:3px;' id='mnu_img_{0}' alt='' onclick='showMenu(event, ""{0}"");' oncontextmenu='showMenu(event, ""{0}""); return false;'/></td>" + _
                                      "</tr></table>", GetScenarioID(idx), "../../images/ra/", Cell.Text)
        End If
        ' D3162 ==
    End Sub
    ' D3267 ==

    'A0910 ==
    'Private Function GetMenuImageForHeaderCell(i As Integer) As String
    '    Return String.Format("&nbsp;&nbsp;<img align='right' style='position:absolute;vertical-align:middle;cursor:context-menu;' id='mnu_img_{0}' src='{1}menu_dots.png' alt='' onclick='showMenu(event, {0});' />", GetScenarioID(i), ImagePath)
    'End Function

    'Private Sub AddHeaderCellMouseHandlers(cell As TableCell, i As Integer)
    '    cell.Attributes.Remove("onmouseover")
    '    cell.Attributes.Add("onmouseover", String.Format("$get('mnu_img_{0}').style.visibility = 'visible';", GetScenarioID(i)))
    '    cell.Attributes.Remove("onmouseout")
    '    cell.Attributes.Add("onmouseout", String.Format("$get('mnu_img_{0}').style.visibility = 'hidden';", GetScenarioID(i)))
    'End Sub

    Public Function GetCategories() As String
        Dim sRes As String = ""
        For Each tAttr As clsAttribute In AttributesList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", tAttr.ID, SafeFormString(ShortString(tAttr.Name, 40, True)), IIf(tAttr.ID = SelectedCategoryID, " selected='selected' ", ""))
        Next
        Return String.Format("<select id='cbCategories' style='width:145px; margin-top:3px' onchange='onSetCategory(this.value);' {1}>{0}</select>", sRes, IIf(AttributesList.Count = 0, " disabled='disabled' ", ""))
    End Function

    Public Function GetViewOptions() As String
        Dim sRes As String = ""
        sRes += String.Format("<option value='{0}' {2}>{1}</option>", CInt(FundedFilteringModes.ffAll), ResString("lblRAViewCatAll"), IIf(FundedFilteringMode = FundedFilteringModes.ffAll, " selected='selected' ", ""))
        sRes += String.Format("<option value='{0}' {2}>{1}</option>", CInt(FundedFilteringModes.ffFunded), ResString("lblRAViewCatFunded"), IIf(FundedFilteringMode = FundedFilteringModes.ffFunded, " selected='selected' ", ""))
        sRes += String.Format("<option value='{0}' {2}>{1}</option>", CInt(FundedFilteringModes.ffPartiallyFunded), ResString("lblRAViewCatPartially"), IIf(FundedFilteringMode = FundedFilteringModes.ffPartiallyFunded, " selected='selected' ", ""))
        sRes += String.Format("<option value='{0}' {2}>{1}</option>", CInt(FundedFilteringModes.ffFullyFunded), ResString("lblRAViewCatFully"), IIf(FundedFilteringMode = FundedFilteringModes.ffFullyFunded, " selected='selected' ", ""))
        sRes += String.Format("<option value='{0}' {2}>{1}</option>", CInt(FundedFilteringModes.ffNotFunded), ResString("lblRAViewCatNotFunded"), IIf(FundedFilteringMode = FundedFilteringModes.ffNotFunded, " selected='selected' ", ""))
        'Return String.Format("<select id='cbFundedFilter' style='width:145px; margin-top:3px' onchange='onSetFundedFilter(this.value);' {1}>{0}</select>", sRes, IIf(AttributesList.Count = 0, " disabled='disabled' ", ""))
        Return String.Format("<select id='cbFundedFilter' style='width:145px; margin-top:3px' onchange='onSetFundedFilter(this.value);'>{0}</select>", sRes)
    End Function

    Private Function MarkCellIgnored(cellText As String, tScenario As RAScenario, tIgnoredField As DisplayFields, Optional Value As Integer = 0) As String
        Dim tIgnored As Boolean = False
        Select Case tIgnoredField
            Case DisplayFields.dfPFailure
                tIgnored = CBool(IIf(Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Risks, Not tScenario.Settings.Risks))
            Case DisplayFields.dfMust
                tIgnored = CBool(IIf(Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Musts, Not tScenario.Settings.Musts))
            Case DisplayFields.dfMustNot
                tIgnored = CBool(IIf(Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.MustNots, Not tScenario.Settings.MustNots))
            Case DisplayFields.dfMustsAndMustNots
                tIgnored = CBool(IIf(Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, (Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Musts AndAlso Value > 0) OrElse (Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.MustNots AndAlso Value < 0), (Not tScenario.Settings.Musts AndAlso Value > 0) OrElse (Not tScenario.Settings.MustNots AndAlso Value < 0)))
            Case DisplayFields.dfCustomConstraints
                tIgnored = CBool(IIf(Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.CustomConstraints, Not tScenario.Settings.CustomConstraints))
            Case DisplayFields.dfGroups
                tIgnored = CBool(IIf(Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Groups, Not tScenario.Settings.Groups))
            Case DisplayFields.dfDependencies
                tIgnored = CBool(IIf(Not IsSingleScenarioChecked() AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Dependencies, Not tScenario.Settings.Dependencies))
        End Select
        Return CStr(IIf(tIgnored, "<span style='color:#a0a0a0'><s>" + cellText + "</s></span>", cellText))
    End Function

    Protected Sub GridAlternatives_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles GridAlternatives.RowDataBound
        If ViewMode = ViewModes.vmColumnChart Then Exit Sub
        Dim tIndexVisible As Boolean = RA.Scenarios.GlobalSettings.IsIndexColumnVisible 'A1143

        For Each cell As TableCell In e.Row.Cells
            cell.Attributes.Add("style", "padding:0em 1em;") '"border-left:1px solid #ccc;border-right:1px solid #ccc;"
        Next

        If e.Row.RowType <> DataControlRowType.EmptyDataRow Then
            e.Row.Cells(COL_ALT_ID).Visible = False
            e.Row.Cells(COL_SORT).Visible = False
            If Not tIndexVisible Then e.Row.Cells(COL_INDEX).CssClass = "ra_hidden" 'A1143
        End If

        If e.Row.RowType = DataControlRowType.Header Then
            e.Row.Height = 34 'CInt(IIf(DisplayField = DisplayFields.dfBenefit AndAlso ScenariosColumns IsNot Nothing AndAlso ScenariosColumns.Count > 0, 54, 34))
            'e.Row.Cells(COL_NAME).Width = 200
            'e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Left
            For i As Integer = 0 To e.Row.Cells.Count - 1
                If i = COL_NAME OrElse i = COL_INDEX Then
                    e.Row.Cells(i).Text = "&nbsp;"
                    e.Row.Cells(i).Font.Bold = True
                    With e.Row.Cells(COL_NAME)
                        '.Font.Size = 11
                        '.VerticalAlign = VerticalAlign.Middle
                        .HorizontalAlign = HorizontalAlign.Right
                        .ForeColor = SUMMARY_HEADER_COLOR
                        .Text = ResString("tblRAScenarioName") + ":"
                        .ToolTip = .Text
                    End With
                Else
                    e.Row.Cells(i).Wrap = True
                    'e.Row.Cells(i).VerticalAlign = VerticalAlign.Middle
                    If (DisplayField <> DisplayFields.dfCustomConstraints AndAlso DisplayField <> DisplayFields.dfGroups) OrElse i <= COL_NAME Then
                        e.Row.Cells(i).ToolTip = e.Row.Cells(i).Text
                        Dim mnu As String = ""
                        'If i >= COL_SCENARIOS Then mnu = GetMenuImageForHeaderCell(i)
                        'e.Row.Cells(i).Text = GetSortCaption(i, e.Row.Cells(i).Text, CStr(IIf(Not IsExport AndAlso i >= COL_SCENARIOS, mnu, "")))
                        e.Row.Cells(i).Text = GetSortCaption(i, e.Row.Cells(i).Text, CStr(IIf(Not IsExport, mnu, "")))
                        If i >= COL_SCENARIOS Then
                            CopyPasteHeaderButtons(e.Row.Cells, i) ' D3267
                            'A1162 ===
                            Dim sTooltip As String = ""
                            Dim scenID As Integer = GetScenarioID(i)
                            If RA.Scenarios.Scenarios.ContainsKey(scenID) Then
                                sTooltip = SafeFormString(RA.Scenarios.Scenarios(scenID).Description)
                            End If
                            If Not String.IsNullOrEmpty(sTooltip) Then e.Row.Cells(i).ToolTip = sTooltip
                            'A1162 ==
                        End If
                        'If Not IsExport AndAlso i >= COL_SCENARIOS Then AddHeaderCellMouseHandlers(e.Row.Cells(i), i)
                    End If
                End If
            Next
            'If Not IsGraphicalMode AndAlso GridViewMode <> GridViewModes.gvmTableView Then e.Row.Cells(COL_INDEX).Width = Unit.Pixel(50)
        End If

        'Split scenario and constraints headers
        Dim tGridRows As TableRowCollection = CType(GridAlternatives.Controls(0), Table).Rows
        Dim CellsCount = tGridRows(0).Cells.Count
        Dim AltsCount As Integer = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count

        If (DisplayField = DisplayFields.dfCustomConstraints OrElse DisplayField = DisplayFields.dfGroups) AndAlso e.Row.RowIndex > 0 AndAlso e.Row.RowIndex = ROW_CONSTR_ENABLED Then
            'put scenarios names and constraints names to a separate rows
            Dim h2 As GridViewRow = New GridViewRow(ROW_CONSTR_ENABLED, -1, DataControlRowType.Header, DataControlRowState.Normal)
            For i As Integer = 0 To CellsCount - 1
                Dim tc As TableCell = New TableHeaderCell
                h2.Cells.Add(tc)
            Next
            tGridRows.AddAt(ROW_CONSTR_ENABLED, h2)
            For i As Integer = 0 To COL_NAME
                'tGridRows(0).Cells(i).RowSpan = 2
                'h2.Cells(i).Visible = False
                h2.Cells(i).Text = "&nbsp;"
            Next

            h2.Cells(COL_NAME).Text = CStr(IIf(DisplayField = DisplayFields.dfGroups, ResString("optGroups"), ResString("optCustomConstraints"))) + ":&nbsp;&nbsp;&nbsp;"
            h2.Cells(COL_NAME).ToolTip = h2.Cells(COL_NAME).Text
            h2.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Right
            h2.Cells(COL_NAME).ForeColor = CUSTOM_CONSTRAINT_HEADER_COLOR

            h2.Cells(COL_ALT_ID).Visible = False
            h2.Cells(COL_SORT).Visible = False

            For Each cdata As ConstraintData In ConstraintsData
                If cdata.ConstraintsCount >= 1 Then
                    Dim s() As String = {}
                    For i As Integer = cdata.Col0ID To cdata.Col1ID
                        s = tGridRows(0).Cells(i).Text.Split(HEADER_SEPARATOR)
                        If s.Count >= 2 Then h2.Cells(i).Text = GetColumnName(s(1))
                    Next
                    s = tGridRows(0).Cells(cdata.Col0ID).Text.Split(HEADER_SEPARATOR)
                    If s.Count >= 2 Then
                        'tGridRows(0).Cells(cdata.Col0ID).Text = GetColumnName(s(0)) + CStr(IIf(IsExport, "", GetMenuImageForHeaderCell(cdata.Col0ID)))
                        tGridRows(0).Cells(cdata.Col0ID).Text = GetColumnName(s(0)) ' D3267
                        If Not IsExport Then CopyPasteHeaderButtons(tGridRows(0).Cells, (cdata.Col0ID)) ' D3267
                        'AddHeaderCellMouseHandlers(tGridRows(0).Cells(cdata.Col0ID), cdata.Col0ID)
                    End If

                    If cdata.ConstraintsCount > 1 Then
                        tGridRows(0).Cells(cdata.Col0ID).ColumnSpan = cdata.ConstraintsCount
                        tGridRows(0).Cells(cdata.Col0ID).Attributes.Add("style", "border-left:1px solid #ccc; border-right:1px solid #ccc")
                    End If

                    For i As Integer = cdata.Col0ID + 1 To cdata.Col1ID
                        tGridRows(0).Cells(i).Visible = False
                    Next
                End If
                If cdata.ConstraintsCount > 0 Then
                    h2.Cells(cdata.Col0ID).Attributes.Add("style", "border-left:1px solid #ccc")
                    h2.Cells(cdata.Col1ID).Attributes.Add("style", "border-right:1px solid #ccc")
                End If
            Next
        End If

        If (DisplayField = DisplayFields.dfCustomConstraints OrElse DisplayField = DisplayFields.dfGroups) AndAlso e.Row.RowType <> DataControlRowType.EmptyDataRow Then
            For Each cdata As ConstraintData In ConstraintsData
                If cdata.ConstraintsCount > 0 Then
                    e.Row.Cells(cdata.Col0ID).Attributes.Add("style", "border-left:1px solid #ccc")
                    e.Row.Cells(cdata.Col1ID).Attributes.Add("style", "border-right:1px solid #ccc")
                End If
            Next
        End If

        If e.Row.RowType = DataControlRowType.DataRow AndAlso e.Row.DataItem IsNot Nothing Then
            Dim tRow As DataRowView = CType(e.Row.DataItem, DataRowView)

            e.Row.Cells(COL_INDEX).HorizontalAlign = HorizontalAlign.Right
            e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Left
            e.Row.Cells(COL_NAME).Wrap = True
            If Not IsDBNull(tRow(COL_NAME)) Then
                e.Row.Cells(COL_NAME).ToolTip = e.Row.Cells(COL_NAME).Text
                e.Row.Cells(COL_NAME).Text = ShortString(e.Row.Cells(COL_NAME).Text, 500)
            End If
            'e.Row.Cells(COL_NAME).Text = ShortString(e.Row.Cells(COL_NAME).Text, 60, True)

            If e.Row.RowIndex = ROW_DESCRIPTION Then
                For i As Integer = COL_SCENARIOS To e.Row.Cells.Count - 1
                    If Not IsDBNull(tRow(i)) Then
                        Dim scenID = GetScenarioID(i)
                        e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Left
                        If SummaryDescriptions.ContainsKey(scenID) Then e.Row.Cells(i).Text = "<small>" + SafeFormString(ShortString(SummaryDescriptions(scenID), 500, True)) + "</small>"
                    End If
                Next
            End If

            If e.Row.RowIndex = ROW_BUDGET OrElse e.Row.RowIndex = ROW_COST Then
                For i As Integer = COL_SCENARIOS To e.Row.Cells.Count - 1
                    If Not IsDBNull(tRow(i)) Then
                        e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                        Dim tValue As Double = CDbl(tRow(i))
                        e.Row.Cells(i).Text = CostString(tValue)    ' D3199
                    End If
                Next
            End If

            If e.Row.RowIndex = ROW_BENEFIT Then
                For i As Integer = COL_SCENARIOS To e.Row.Cells.Count - 1
                    If Not IsDBNull(tRow(i)) Then
                        e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                        Dim tValue As Double = CDbl(tRow(i))
                        e.Row.Cells(i).Text = AddBar(String.Format("{0}%", Math.Round(tValue, OPT_PRECISION_PERCENT)), tValue / 100) 'A0912
                    End If
                Next
            End If

            If (DisplayField = DisplayFields.dfCustomConstraints OrElse DisplayField = DisplayFields.dfGroups) AndAlso e.Row.RowIndex < ROW_CONSTR_ENABLED Then
                For Each cdata As ConstraintData In ConstraintsData
                    If cdata.ConstraintsCount > 1 Then
                        e.Row.Cells(cdata.Col0ID).ColumnSpan = cdata.ConstraintsCount
                        e.Row.Cells(cdata.Col0ID).Attributes.Add("style", "border-left:1px solid #ccc; border-right:1px solid #ccc")
                        For i As Integer = cdata.Col0ID + 1 To cdata.Col1ID
                            e.Row.Cells(i).Visible = False
                        Next
                    End If
                Next
            End If

            If (DisplayField = DisplayFields.dfCustomConstraints OrElse DisplayField = DisplayFields.dfGroups) AndAlso e.Row.RowIndex >= ROW_CONSTR_ENABLED AndAlso e.Row.RowIndex <= ROW_CONSTR_TOTAL Then
                For i As Integer = COL_SCENARIOS To e.Row.Cells.Count - 1

                    Dim scenID As Integer = GetScenarioID(i)
                    Dim tScenario As RAScenario = Nothing
                    If RA.Scenarios.Scenarios.ContainsKey(scenID) Then
                        tScenario = RA.Scenarios.Scenarios(scenID)
                    End If

                    e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                    If Not IsDBNull(tRow(i)) Then
                        Select Case e.Row.RowIndex
                            Case ROW_CONSTR_ENABLED
                                e.Row.Cells(i).Text = CStr(IIf(CDbl(tRow(i)) > 0, MarkCellIgnored(ResString("lblYes"), tScenario, DisplayField), "&nbsp;"))
                            Case ROW_CONSTR_MIN, ROW_CONSTR_MAX
                                If DisplayField = DisplayFields.dfCustomConstraints Then
                                    Dim tVal As Double = CDbl(tRow(i))
                                    If tVal <> UNDEFINED_INTEGER_VALUE Then
                                        e.Row.Cells(i).Text = MarkCellIgnored(tVal.ToString("F2"), tScenario, DisplayField)
                                    Else
                                        e.Row.Cells(i).Text = "&nbsp;"
                                    End If
                                End If
                            Case ROW_CONSTR_FUNDED, ROW_CONSTR_TOTAL
                                If DisplayField = DisplayFields.dfCustomConstraints Then
                                    Dim tVal As Double = CDbl(tRow(i))
                                    e.Row.Cells(i).Text = MarkCellIgnored(tVal.ToString("F2"), tScenario, DisplayField)
                                End If
                                If DisplayField = DisplayFields.dfGroups Then
                                    Dim tVal As String = CStr(tRow(i))
                                    If e.Row.RowIndex = ROW_CONSTR_FUNDED Then 'Group condition
                                        Select Case tVal
                                            Case CInt(RAGroupCondition.gcEqualsOne).ToString
                                                tVal = "=1"
                                            Case CInt(RAGroupCondition.gcGreaterOrEqualsOne).ToString
                                                tVal = ">=1"
                                            Case CInt(RAGroupCondition.gcLessOrEqualsOne).ToString
                                                tVal = "<=1"
                                        End Select
                                    End If
                                    e.Row.Cells(i).Text = MarkCellIgnored(tVal, tScenario, DisplayField)
                                End If
                        End Select
                    End If
                Next
            End If

            If e.Row.RowIndex = ROW_FIELD_TITLE Then
                If e.Row.Cells.Count > COL_SCENARIOS Then
                    e.Row.Cells(COL_SCENARIOS).HorizontalAlign = HorizontalAlign.Center
                    e.Row.Cells(COL_SCENARIOS).VerticalAlign = VerticalAlign.Middle
                    'e.Row.Cells(COL_SCENARIOS).Text = "<span style='color:#f77;font-size:10pt;font-weight:bold;cursor:default;'>" + GetSelectedFieldName() + CStr(IIf(DisplayField = DisplayFields.dfFundingByCategory, ":&nbsp;", "")) + "</span>"
                    e.Row.Cells(COL_SCENARIOS).Text = "<h6 style='margin:0px 0px;padding:0px 0px;'>" + GetSelectedFieldName() '+ CStr(IIf(DisplayField = DisplayFields.dfFundingByCategory, ":&nbsp;", ""))
                    'If DisplayField = DisplayFields.dfFundingByCategory Then e.Row.Cells(COL_SCENARIOS).Text += GetCategories() + "&nbsp;&nbsp;" + ResString("btnView") + ":&nbsp;" + GetViewOptions()
                    e.Row.Cells(COL_SCENARIOS).Text += "</h6>"
                    e.Row.Cells(COL_SCENARIOS).ColumnSpan = e.Row.Cells.Count - COL_SCENARIOS
                    If DisplayField = DisplayFields.dfCustomConstraints OrElse DisplayField = DisplayFields.dfGroups Then
                        e.Row.Cells(COL_SCENARIOS).Attributes.Add("style", "border-left:1px solid #ccc; border-right:1px solid #ccc")
                    End If
                    For i As Integer = COL_SCENARIOS + 1 To e.Row.Cells.Count - 1
                        e.Row.Cells(i).Visible = False
                    Next
                    'e.Row.Cells(COL_SCENARIOS).ForeColor = Drawing.Color.FromArgb(255, 120, 120)
                    'e.Row.Cells(COL_SCENARIOS).Font.Size = 14
                End If
                'Alternative ID - moved to ROW_FIELD_TITLE
                Dim colName As String = CStr(CType(GridAlternatives.DataSource, DataView).ToTable.Columns(COL_INDEX).ColumnName)
                e.Row.Cells(COL_INDEX).ToolTip = colName.Substring(0, colName.IndexOf(HEADER_ID_SEPARATOR))
                e.Row.Cells(COL_INDEX).Text = GetSortCaption(COL_INDEX, e.Row.Cells(COL_INDEX).ToolTip, "")
                e.Row.Cells(COL_INDEX).HorizontalAlign = HorizontalAlign.Center
                'Alternative Name - moved to ROW_FIELD_TITLE
                colName = CStr(CType(GridAlternatives.DataSource, DataView).ToTable.Columns(COL_NAME).ColumnName)
                e.Row.Cells(COL_NAME).ToolTip = colName.Substring(0, colName.IndexOf(HEADER_ID_SEPARATOR))
                e.Row.Cells(COL_NAME).Text = GetSortCaption(COL_NAME, e.Row.Cells(COL_NAME).ToolTip, "")
                e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Center
            End If

            If e.Row.RowIndex = ROW_SOLVER_MSG Then
                If SolverMessages.Count = 0 OrElse SolverMessages.Values.Where(Function(s) (s <> "&nbsp;" AndAlso s.Length > 0)).Count = 0 Then
                    For i As Integer = 0 To e.Row.Cells.Count - 1
                        e.Row.Cells(i).Text = ""
                    Next
                    e.Row.Height = 0
                    e.Row.Visible = False
                Else
                    For i As Integer = COL_SCENARIOS To e.Row.Cells.Count - 1
                        e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                        Dim scenID = GetScenarioID(i)
                        If e.Row.Cells(i).Visible AndAlso SolverMessages.ContainsKey(scenID) Then e.Row.Cells(i).Text = SolverMessages(scenID)
                    Next
                End If
            End If

            If DisplayField <> DisplayFields.dfCustomConstraints AndAlso DisplayField <> DisplayFields.dfGroups AndAlso e.Row.RowIndex >= ROW_CONSTR_ENABLED AndAlso e.Row.RowIndex <= ROW_CONSTR_TOTAL Then
                For i As Integer = 0 To e.Row.Cells.Count - 1
                    e.Row.Cells(i).Text = ""
                Next
                e.Row.Height = 0
                e.Row.Visible = False
            End If

            'hide the Min and Max rows for Groups
            If DisplayField = DisplayFields.dfGroups AndAlso (e.Row.RowIndex = ROW_CONSTR_MIN OrElse e.Row.RowIndex = ROW_CONSTR_MAX) Then
                For i As Integer = 0 To e.Row.Cells.Count - 1
                    e.Row.Cells(i).Text = ""
                Next
                e.Row.Height = 0
                e.Row.Visible = False
            End If

            If e.Row.RowIndex >= ROW_ALTERNATIVES_START Then
                'Pie Charts Mode
                If ViewMode = ViewModes.vmPieChart OrElse ViewMode = ViewModes.vmDonutChart Then
                    'Pie Chart divs
                    If e.Row.RowIndex = ROW_ALTERNATIVES_START Then
                        'Pie Chart legend
                        'e.Row.Cells(COL_INDEX).Text = "<small>" + e.Row.Cells(COL_INDEX).Text + "</small>"                        
                        e.Row.Cells(COL_INDEX).RowSpan = AltsCount
                        e.Row.Cells(COL_NAME).RowSpan = AltsCount
                        e.Row.Cells(COL_INDEX).ColumnSpan = 2
                        e.Row.Cells(COL_INDEX).HorizontalAlign = HorizontalAlign.Left
                        e.Row.Cells(COL_INDEX).VerticalAlign = VerticalAlign.Top
                        e.Row.Cells(COL_NAME).Visible = False
                        For i As Integer = 0 To e.Row.Cells.Count - 1
                            e.Row.Cells(i).BackColor = WHITE_BACKGROUND
                        Next
                        'e.Row.Cells(COL_NAME).Text = String.Format("<div style='float:left;width:16px;height:16px;border:1px solid #ccc;padding:1px;background:{1};'></div><small>{0}</small>", e.Row.Cells(COL_NAME).Text, ColorPalette((e.Row.RowIndex - ROW_ALTERNATIVES_START) Mod ColorPalette.Count))
                        'Dim sColorLegend As String = "<div id='divPieLegend' style='overflow:auto; width:100%;'><table id='TablePieColorLegend' class='text grid' style='border-collapse:collapse; margin-top:25px;' cellspacing='0'>"
                        Dim sColorLegend As String = "<div id='divPieLegend' style='overflow:auto; padding-top:10px;'>"
                        If DisplayField <> DisplayFields.dfObjPriorities Then
                            Dim altIndex As Integer = 0
                            For Each alt As RAAlternative In BaseScenario.AlternativesFull
                                'Dim altIndex As Integer = BaseScenario.AlternativesFull.IndexOf(alt)
                                Dim sColor As String = NoSpecificColor
                                Dim tAttrColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, New Guid(alt.ID), Guid.Empty))
                                If tAttrColor >= 0 Then
                                    sColor = Long2HTMLColor(tAttrColor)
                                Else
                                    sColor = GetPaletteColor(CurrentPaletteID(PM), altIndex, True)
                                End If
                                'sColorLegend += "<tr valign='top' id='legendRow" + altIndex.ToString + "' onmouseover='pieLegendRowHover(this," + altIndex.ToString + ",1,-1);' onmouseout='pieLegendRowHover(this," + altIndex.ToString + ",0,-1);'><td class='text small' style='padding:2px 2px;border:0px solid #ccc; width:50px;'>" + alt.SortOrder.ToString + "</td><td style='padding:2px 2px;border:0px solid #ccc; width:18px;'>" + String.Format("<div style='width:16px;height:16px;border:1px solid #ccc;padding:1px;background:{1};'></div></td><td style='word-wrap:break-word; border:0px solid #ccc; padding:1px 3px; min-width:150px;' title='{2}' class='text small' valign='center'>{0}", SafeFormString(alt.Name), sColor, SafeFormString(SafeFormString(alt.Name))) + "</td></tr>"
                                'sColorLegend += "<tr valign='top' id='legendRow" + altIndex.ToString + "' onmouseover='pieLegendRowHover(this," + altIndex.ToString + ",1,-1);' onmouseout='pieLegendRowHover(this," + altIndex.ToString + ",0,-1);'><td class='text small' style='padding:2px 2px;border:0px solid #ccc; width:50px;'>" + alt.SortOrder.ToString + "</td><td style='padding:2px 2px;border:0px solid #ccc; width:16px;'>" + String.Format("<img src='" + ImagePath + "legend-15.png" + "' width='15' height='15' title='' border='0' class='pie-legend-mark' style='background-color:{1}'></td><td style='word-wrap:break-word; border:0px solid #ccc; padding:1px 3px; min-width:150px;' title='{2}' class='text small' valign='center'>{0}", SafeFormString(alt.Name), sColor, SafeFormString(SafeFormString(alt.Name))) + "</td></tr>"
                                sColorLegend += "<div valign='top' id='legendRow" + altIndex.ToString + "' onmouseover='pieLegendRowHover(this," + altIndex.ToString + ",1,-1);' onmouseout='pieLegendRowHover(this," + altIndex.ToString + ",0,-1);'><span class='text small' style='padding:2px 2px;border:0px solid #ccc; width:50px;'>" + alt.SortOrder.ToString + "</span><span style='padding:2px 2px;border:0px solid #ccc; width:16px;display:inline-block;vertical-align:middle;'>" + String.Format("<img src='../../images/ra/legend-15.png" + "' width='15' height='15' title='' border='0' class='pie-legend-mark' style='background-color:{1}'></span><span style='word-wrap:break-word; border:0px solid #ccc; padding:1px 3px; min-width:150px;' title='{2}' class='text small' valign='center'>{0}", SafeFormString(alt.Name), sColor, SafeFormString(SafeFormString(alt.Name))) + "</span></div>"
                                altIndex += 1
                            Next
                        Else
                            Dim index As Integer = 0
                            For Each t As Tuple(Of Integer, Integer, clsNode) In RA.ProjectManager.ActiveObjectives.NodesInLinearOrder
                                Dim tObj As clsNode = t.Item3
                                If (ViewMode = ViewModes.vmDonutChart AndAlso tObj.ParentNode IsNot Nothing) OrElse tObj.IsTerminalNode Then
                                    Dim sColor As String = NoSpecificColor
                                    If ViewMode = ViewModes.vmPieChart Then sColor = GetPaletteColor(CurrentPaletteID(PM), index)
                                    If ViewMode = ViewModes.vmPieChart Then
                                        sColorLegend += "<div valign='top' id='legendRow" + index.ToString + "' onmouseover='pieLegendRowHover(this," + index.ToString + ",1,-1);' onmouseout='pieLegendRowHover(this," + index.ToString + ",0,-1);'><span style='padding:2px 2px;border:0px solid #ccc; width:18px;'>" + String.Format("<div id='boxLegendColor" + tObj.NodeID.ToString + "' style='width:16px;height:16px;border:1px solid #ccc;padding:1px;background:{1};display:inline-block;vertical-align:middle;'></div></span><span style='word-wrap:break-word; border:0px solid #ccc; padding:1px 3px; min-width:150px;' title='{0}' class='text small' valign='center'>{0}", SafeFormString(tObj.NodeName), sColor) + "</span></div>"
                                    Else
                                        sColorLegend += "<div valign='top' id='legendRow" + tObj.NodeID.ToString + "' onmouseover='HpieLegendRowHover(this," + tObj.NodeID.ToString + ",1,-1);' onmouseout='HpieLegendRowHover(this," + tObj.NodeID.ToString + ",0,-1);'><span style='padding:2px 2px;border:0px solid #ccc; width:18px;'>" + String.Format("<div id='boxLegendColor" + tObj.NodeID.ToString + "' style='width:16px;height:16px;border:1px solid #ccc;padding:1px;background:{1};display:inline-block;vertical-align:middle;'></div></span><span style='word-wrap:break-word; border:0px solid #ccc; padding:1px 3px; min-width:150px;' title='{0}' class='text small' valign='center'>{0}", SafeFormString(tObj.NodeName), sColor) + "</span></div>"
                                    End If
                                    '<tr onmouseover='LegendRowHover(this,1);' onmouseout='LegendRowHover(this,0);'
                                    index += 1
                                End If
                            Next
                        End If
                        sColorLegend += "</div>"
                        e.Row.Cells(COL_INDEX).Text = sColorLegend
                        '<div style="width:16px;height:16px;border:1px solid #ccc;padding:1px;background:#ffffff;"><span class="text small" style="width:12px;height:12px;background:'+val[0]+'"><img src="<% =ImagePath %>blank.gif" width=12 height=12 border=0></span></div></td><td style="word-wrap:break-word; border:0px solid #ccc; padding:1px 3px;" title="'+val[1]+'" id="legend_row_'+index+'" class="text small" valign="center">'+htmlEscape(val[1])+'</td></tr>');

                        'GridAlternatives.AlternatingRowStyle.CssClass = ""
                        'GridAlternatives.RowStyle.CssClass = ""
                        For i As Integer = COL_SCENARIOS To e.Row.Cells.Count - 1
                            e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                            e.Row.Cells(i).VerticalAlign = VerticalAlign.Top
                            e.Row.Cells(i).RowSpan = AltsCount
                            If ViewMode = ViewModes.vmDonutChart AndAlso DisplayField = DisplayFields.dfObjPriorities Then
                                e.Row.Cells(i).Text = String.Format("<canvas id='pieChart{0}' style='width:{1}px; height:{1}px; {2} margin:10px 0px;'></canvas><span id='lblHPiePleaseWait{0}'>" + ResString("lblPleaseWait") + "</span>", i - COL_SCENARIOS, OPT_MIN_PIE_PLOT_SIZE, IIf(OPT_PIE_BORDERS, "border:1px solid #fafafa;", ""))
                            Else
                                e.Row.Cells(i).Text = String.Format("<div id='pieChart{0}' style='width:{1}px; height:{1}px; {2} margin:10px 0px;'><center><p style='vertical-align:middle; margin-top:8ex;'>" + ResString("lblPleaseWait") + "</p></center></div>", i - COL_SCENARIOS, OPT_MIN_PIE_PLOT_SIZE, IIf(OPT_PIE_BORDERS, "border:1px solid #fafafa;", ""))
                            End If
                        Next
                    Else
                        e.Row.Cells(COL_INDEX).Visible = False
                        e.Row.Cells(COL_NAME).Visible = False
                        For i As Integer = COL_SCENARIOS To e.Row.Cells.Count - 1
                            e.Row.Cells(i).Visible = False
                        Next
                    End If
                Else
                    Dim has_data As Boolean = True
                    Dim altID As String = CStr(tRow(COL_ALT_ID))

                    ' indent for the objective
                    'If DisplayField = DisplayFields.dfObjPriorities AndAlso altID.Length = Guid.Empty.ToString.Length Then
                    '    Dim tObj As clsNode = RA.ProjectManager.Hierarchy(RA.ProjectManager.ActiveHierarchy).GetNodeByID(New Guid(altID))
                    '    e.Row.Cells(COL_NAME).Attributes.Remove("padding-left")
                    '    If tObj IsNot Nothing Then e.Row.Cells(COL_NAME).Attributes.Add("padding-left", String.Format("{0}px", tObj.Level * 5))
                    'End If

                    For i As Integer = COL_SCENARIOS To e.Row.Cells.Count - 1
                        e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center

                        Dim alt As RAAlternative = Nothing
                        Dim scenID As Integer = GetScenarioID(i)

                        has_data = scenID >= 0

                        If Not has_data Then Exit For

                        Dim tScenario As RAScenario = Nothing
                        If RA.Scenarios.Scenarios.ContainsKey(scenID) Then
                            tScenario = RA.Scenarios.Scenarios(scenID)
                            alt = tScenario.GetAvailableAlternativeById(altID)
                        End If

                        If Not IsDBNull(tRow(i)) Then
                            If CDbl(tRow(i)) = DBL_EXCLUDED_ALT_MARK Then
                                If Not IsExport Then
                                    e.Row.Cells(i).Text = String.Format("<img align='center' style='vertical-align:middle;' src='{0}' alt='{1}' />", IMG_EXCLUDED_ALT_MARK, SafeFormString(ResString("lblAltNotIncluded")))
                                Else
                                    e.Row.Cells(i).Text = CHAR_EXCLUDED_ALT_MARK
                                End If

                                'A1150 ===
                                If DisplayField = DisplayFields.dfCustomConstraints OrElse DisplayField = DisplayFields.dfGroups Then
                                    For Each cdata As ConstraintData In ConstraintsData
                                        If cdata.ConstraintsCount >= 1 Then
                                            If e.Row.RowType = DataControlRowType.DataRow Then
                                                If Not IsDBNull(tRow(cdata.Col0ID)) AndAlso CDbl(tRow(cdata.Col0ID)) = DBL_EXCLUDED_ALT_MARK Then
                                                    If cdata.ConstraintsCount > 1 Then
                                                        e.Row.Cells(cdata.Col0ID).ColumnSpan = cdata.ConstraintsCount
                                                        e.Row.Cells(cdata.Col0ID).Attributes.Add("style", "border-left:1px solid #ccc; border-right:1px solid #ccc")
                                                    End If
                                                    For k As Integer = cdata.Col0ID + 1 To cdata.Col1ID
                                                        e.Row.Cells(k).Visible = False
                                                    Next
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                                'A1150 ==
                            Else
                                Select Case DisplayField
                                    Case DisplayFields.dfFunded
                                        Dim funded As Double = CDbl(tRow(i))
                                        'e.Row.Cells(i).Text = CStr(IIf(funded > 0, IIf(funded < 1, lblPARTIAL, lblYES), lblNO))
                                        Dim altCost As Double = 0
                                        If alt IsNot Nothing Then altCost = alt.Cost
                                        e.Row.Cells(i).Text = CStr(IIf(funded >= 1, ResString("lblYes"), IIf(funded > 0.0001, String.Format("{0}% ({1})", (funded * 100).ToString("F2"), CostString(funded * altCost)), "")))
                                        If FUNDED_CELL_BKG_FILLED AndAlso funded > 0 Then
                                            MarkCellFunded(e.Row.Cells(i), funded)
                                        End If
                                    Case DisplayFields.dfBenefit, DisplayFields.dfEBenefit
                                        Dim bValue As Double = Math.Round(Math.Abs(CDbl(tRow(i)) * 100), OPT_PRECISION_PERCENT) 'A0912
                                        e.Row.Cells(i).Text = AddBar(String.Format("{0}%", bValue), bValue / 100) 'A0921
                                    Case DisplayFields.dfPFailure
                                        Dim rValue As Double = Math.Round(Math.Abs(CDbl(tRow(i)) * 100), OPT_PRECISION_PERCENT) 'A0912
                                        e.Row.Cells(i).Text = AddBar(MarkCellIgnored(String.Format("{0}%", rValue), tScenario, DisplayFields.dfPFailure), rValue / 100) 'A0912
                                        ' D4357 ===
                                    Case DisplayFields.dfPSuccess
                                        Dim rValue As Double = Math.Round(Math.Abs(CDbl(tRow(i)) * 100), OPT_PRECISION_PERCENT)
                                        e.Row.Cells(i).Text = AddBar(MarkCellIgnored(String.Format("{0}%", rValue), tScenario, DisplayFields.dfPSuccess), rValue / 100)
                                    Case DisplayFields.dfRisk
                                        Dim rValue As Double = Math.Round(Math.Abs(CDbl(tRow(i)) * 100), OPT_PRECISION_PERCENT)
                                        e.Row.Cells(i).Text = AddBar(MarkCellIgnored(String.Format("{0}%", rValue), tScenario, DisplayFields.dfRisk), rValue / 100)
                                        ' D4357 ==
                                    Case DisplayFields.dfCost
                                        e.Row.Cells(i).Text = CostString(CDbl(tRow(i))) ' D3199
                                    Case DisplayFields.dfMust, DisplayFields.dfMustNot, DisplayFields.dfMustsAndMustNots
                                        Dim MustColor As Color = Color.FromArgb(32, 128, 32)
                                        Dim MustNotColor As Color = Color.FromArgb(160, 80, 80)
                                        Dim UndefinedColor As Color = Color.FromArgb(200, 200, 200)

                                        Dim value As Integer = CInt(tRow(i))
                                        If DisplayField = DisplayFields.dfMustsAndMustNots Then
                                            Dim cellText As String = ResString("lblNotSet")
                                            If value = 1 Then cellText = ResString("lblMust")
                                            If value = -1 Then cellText = ResString("lblMustNot")
                                            If value = 2 Then cellText = String.Format("{0} / {1}", ResString("lblMust"), ResString("lblMustNot")) 'If both Must and Must Not are set (which is not possible in the UI)
                                            e.Row.Cells(i).Text = MarkCellIgnored(cellText, tScenario, DisplayFields.dfMustsAndMustNots, value)
                                            Select Case value
                                                Case 0, 2
                                                    e.Row.Cells(i).ForeColor = UndefinedColor
                                                Case -1
                                                    e.Row.Cells(i).ForeColor = MustNotColor
                                                Case 1
                                                    e.Row.Cells(i).ForeColor = MustColor
                                            End Select
                                        Else
                                            e.Row.Cells(i).Text = CStr(IIf(value > 0, CStr(IIf(DisplayField = DisplayFields.dfMust, MarkCellIgnored(ResString("lblMust"), tScenario, DisplayFields.dfMust), MarkCellIgnored(ResString("lblMustNot"), tScenario, DisplayFields.dfMustNot))), ResString("lblNotSet"))) 'Show "Must"/"Must Not"
                                            'e.Row.Cells(i).Text = String.Format("<input type='checkbox' class='checkbox' onclick='return false;' onkeydown='return false;' onchange='return false;' {0} />", CStr(IIf(value > 0.0001, " checked='checked' ", ""))) 'Show checkboxes
                                            e.Row.Cells(i).ForeColor = CType(IIf(value > 0, IIf(DisplayField = DisplayFields.dfMust, MustColor, MustNotColor), UndefinedColor), Color)
                                        End If
                                    Case DisplayFields.dfFundingByCategory
                                        Dim value As Integer = CInt(tRow(i))
                                        Dim catId As Guid = New Guid(altID)
                                        If value > 0 Then
                                            'e.Row.Cells(i).Text = String.Format("{0} {2} ({1}%)", value, (value * 100 / AltsCount).ToString("F2"), CStr(IIf(value <= 1, ResString("tblAlternative"), ResString("lblStructureAlternatives")))) 'singular or plural
                                            e.Row.Cells(i).Text = String.Format("<span title='{0} {1}' style='cursor:default;text-align:center;'>{0} {1}</span>", value, CStr(IIf(value <= 1, ResString("tblAlternative"), ResString("lblStructureAlternatives")))) 'singular or plural
                                            e.Row.Cells(i).Attributes.Add("style", "padding-top:4px; padding-bottom:4px;")
                                            If AltsByCategory.ContainsKey(scenID) AndAlso AltsByCategory(scenID).ContainsKey(catId) AndAlso AltsByCategory(scenID)(catId) IsNot Nothing AndAlso AltsByCategory(scenID)(catId).Count > 0 Then
                                                e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Left
                                                e.Row.Cells(i).Text = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + e.Row.Cells(i).Text
                                                e.Row.Cells(i).Text += ":<small>"
                                                For Each ra_alt As RAAlternative In AltsByCategory(scenID)(catId)
                                                    If ra_alt IsNot Nothing Then
                                                        Dim W As Integer = CInt(Math.Round(138 / (e.Row.Cells.Count - COL_SCENARIOS) + 1))
                                                        If W < 25 Then W = 25
                                                        e.Row.Cells(i).Text += "<br/>" + String.Format("<span title='{0}' style='cursor:default;text-align:left;'>{1}</span>", SafeFormString(SafeFormString(ra_alt.Name)), SafeFormString(ShortString(ra_alt.Name, W, True)))
                                                    End If
                                                Next
                                                e.Row.Cells(i).Text += "</small>"
                                            End If
                                        Else
                                            e.Row.Cells(i).Text = "&nbsp;"
                                        End If
                                    Case DisplayFields.dfCustomConstraints
                                        e.Row.Cells(i).Text = MarkCellIgnored(e.Row.Cells(i).Text, tScenario, DisplayField)
                                    Case DisplayFields.dfGroups, DisplayFields.dfDependencies
                                        If e.Row.Cells(i).Text <> "" Then e.Row.Cells(i).Text = MarkCellIgnored(HTML_CHECKMARK_THIN, tScenario, DisplayField)
                                    Case DisplayFields.dfObjPriorities
                                        Dim oValue As Double = Math.Round(Math.Abs(CDbl(tRow(i)) * 100), OPT_PRECISION_PERCENT)
                                        e.Row.Cells(i).Text = AddBar(String.Format("{0}%", oValue), oValue / 100)
                                End Select
                            End If
                        End If
                        If FUNDED_CELL_BKG_FILLED AndAlso DisplayField <> DisplayFields.dfFunded AndAlso DisplayField <> DisplayFields.dfDependencies AndAlso DisplayField <> DisplayFields.dfFundingByCategory Then
                            If alt IsNot Nothing AndAlso alt.ID.Equals(altID) AndAlso alt.DisplayFunded > 0 Then 'A0939
                                MarkCellFunded(e.Row.Cells(i), alt.DisplayFunded) 'A0939
                            End If
                        End If
                    Next

                    'higlight differences (Custom Constraints OR Groups)
                    If (DisplayField = DisplayFields.dfCustomConstraints OrElse DisplayField = DisplayFields.dfGroups) AndAlso ConstraintsDifferences.Count > 0 Then
                        If ConstraintsDifferences.ContainsKey(altID) AndAlso ConstraintsDifferences(altID) Then
                            For i As Integer = 0 To e.Row.Cells.Count - 1
                                e.Row.Cells(i).ForeColor = Color.MediumSlateBlue 'DodgerBlue 'SlateBlue
                                e.Row.Cells(i).Font.Bold = True
                                'e.Row.Cells(i).Font.Italic = True
                            Next
                        End If
                    End If
                End If
            Else
                'summary rows style
                For i As Integer = 0 To e.Row.Cells.Count - 1
                    e.Row.Cells(i).BackColor = SUMMARY_HEADER_BACKGROUND
                Next
                e.Row.Cells(COL_INDEX).Font.Bold = True
                e.Row.Cells(COL_NAME).Font.Bold = True
                If (DisplayField <> DisplayFields.dfCustomConstraints AndAlso DisplayField <> DisplayFields.dfGroups AndAlso DisplayField <> DisplayFields.dfDependencies) OrElse e.Row.RowIndex < ROW_CONSTR_ENABLED Then
                    e.Row.Cells(COL_INDEX).ForeColor = SUMMARY_HEADER_COLOR
                    e.Row.Cells(COL_NAME).ForeColor = SUMMARY_HEADER_COLOR
                Else
                    If e.Row.RowIndex <> ROW_FIELD_TITLE Then e.Row.Cells(COL_NAME).ForeColor = CUSTOM_CONSTRAINT_HEADER_COLOR
                End If
                If e.Row.RowIndex <> ROW_FIELD_TITLE Then e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Right
            End If

            If DisplayField = DisplayFields.dfObjPriorities AndAlso (e.Row.RowIndex < ROW_ALTERNATIVES_START OrElse ViewMode = ViewModes.vmGridView) Then e.Row.Cells(COL_INDEX).Text = "&nbsp;"

            If Not IsExport AndAlso ViewMode = ViewModes.vmGridView Then
                e.Row.Attributes.Remove("onmouseover")
                e.Row.Attributes.Add("onmouseover", String.Format("RowHover(this,1,{0},{1});", IIf(e.Row.RowState = DataControlRowState.Alternate AndAlso e.Row.RowIndex >= ROW_ALTERNATIVES_START, 1, 0), IIf(e.Row.RowIndex < ROW_ALTERNATIVES_START, 1, 0)))
                e.Row.Attributes.Remove("onmouseout")
                e.Row.Attributes.Add("onmouseout", String.Format("RowHover(this,0,{0},{1});", IIf(e.Row.RowState = DataControlRowState.Alternate AndAlso e.Row.RowIndex >= ROW_ALTERNATIVES_START, 1, 0), IIf(e.Row.RowIndex < ROW_ALTERNATIVES_START, 1, 0)))
            End If
        End If

    End Sub

    Private Function GetScenarioID(ColumnID As Integer) As Integer
        Dim scenID As Integer = -1

        If DisplayField <> DisplayFields.dfCustomConstraints AndAlso DisplayField <> DisplayFields.dfGroups Then
            scenID = ColumnID - COL_SCENARIOS
            If scenID < ScenariosColumns.Count Then scenID = ScenariosColumns(scenID)
        Else
            For sid As Integer = 0 To ConstraintsData.Count - 1
                If (ColumnID >= ConstraintsData(sid).Col0ID) AndAlso (ColumnID <= ConstraintsData(sid).Col1ID) AndAlso (ConstraintsData(sid).ConstraintsCount > 0) Then
                    scenID = ConstraintsData(sid).ScenarioID
                End If
            Next
        End If
        Return scenID
    End Function

    Private Sub MarkCellFunded(cell As TableCell, funded As Double)
        If funded > 0 Then
            'e.Row.Cells(i).ForeColor = Drawing.Color.FromArgb(140, 200, 80)
            cell.BackColor = FUNDED_CELL_FILL_COLOR
            cell.Attributes.Add("class", "funded_cell")
            'If funded < 1 Then cell.Font.Italic = True
        End If
    End Sub

    Protected Sub RadAjaxManagerMain_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxManagerMain.AjaxRequest
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(e.Argument)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Select Case sAction
            Case "field"
                Dim value As Integer = CInt(GetParam(args, "value"))
                DisplayField = CType(value, DisplayFields)
            Case "scenarios_load"
                Dim isChanged As Boolean = False
                Dim param_ids = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ids"))    ' Anti-XSS
                Dim ids As String() = param_ids.Split(CChar("-"))
                If ids.Length > 0 Then
                    Dim cur_ids As String = ""
                    For Each scenario As RAScenario In RA.Scenarios.Scenarios.Values
                        If scenario.IsCheckedCS Then
                            cur_ids += CStr(IIf(cur_ids = "", "", "-")) + scenario.ID.ToString
                        End If
                    Next
                    isChanged = param_ids <> cur_ids
                End If

                If isChanged Then
                    ScenariosColumns.Clear()
                    For Each scenario As RAScenario In RA.Scenarios.Scenarios.Values
                        scenario.IsCheckedCS = False
                    Next

                    If ids.Length > 0 Then
                        For Each id As String In ids
                            Dim tID As Integer = 0
                            If Integer.TryParse(id, tID) AndAlso RA.Scenarios.Scenarios.ContainsKey(tID) Then
                                RA.Scenarios.Scenarios(tID).IsCheckedCS = True
                                ScenariosColumns.Add(tID)
                            End If
                        Next
                    End If
                    RA.Save()
                End If
                'Case "scenario_checked"
                '    Dim id As Integer = CInt(GetParam(args, "id"))
                '    Dim chk As String = GetParam(args, "chk").ToLower
                '    If RA.Scenarios.Scenarios.ContainsKey(id) Then
                '        Dim scenario As RAScenario = RA.Scenarios.Scenarios(id)
                '        scenario.IsCheckedCS = chk = "1" OrElse chk = "true"
                '        If scenario.IsCheckedCS Then
                '            If Not ScenariosColumns.Contains(id) Then ScenariosColumns.Add(id)
                '        Else
                '            If ScenariosColumns.Contains(id) Then ScenariosColumns.Remove(id)
                '        End If
                '        RA.Save()
                '    End If
                'Case "check_all"
                '    Dim chk As String = GetParam(args, "chk").ToLower
                '    Dim IsChecked As Boolean = chk = "1" OrElse chk = "true"
                '    For Each kvp As KeyValuePair(Of Integer, RAScenario) In RA.Scenarios.Scenarios
                '        kvp.Value.IsCheckedCS = IsChecked
                '        If IsChecked AndAlso Not ScenariosColumns.Contains(kvp.Key) Then ScenariosColumns.Add(kvp.Key)
                '    Next
                '    If Not IsChecked Then ScenariosColumns.Clear()
                '    RA.Save()
            Case "sort"
                Dim dir As Integer = CInt(GetParam(args, "dir"))
                RASortDirection = CStr(IIf(dir = 0, "ASC", "DESC"))
                RASortColumn = CInt(GetParam(args, "idx"))
            Case "edit"
                Dim id As Integer = CInt(GetParam(args, "id"))
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim    ' Anti-XSS
                Dim sDescr As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "descr")).Trim  ' Anti-XSS
                If RA.Scenarios.Scenarios.ContainsKey(id) Then
                    RA.Scenarios.Scenarios(id).Name = sName
                    RA.Scenarios.Scenarios(id).Description = sDescr
                    RA.Save()
                End If
            Case "paste"
                Dim id As Integer = CInt(GetParam(args, "scenario_id"))
                Dim data As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "data"))  ' Anti-XSS
                Dim cells As String() = data.Split(Chr(10))
                Dim cells_count As Integer = cells.Count
                Dim fChanged As Boolean = False
                If RA.Scenarios.Scenarios.ContainsKey(id) Then
                    Solve()
                    Dim alts_count As Integer = RA.Scenarios.Scenarios(id).Alternatives.Count
                    If alts_count < cells.Count Then cells_count = alts_count
                    For i As Integer = 0 To cells_count - 1
                        Dim value As Double = 0
                        Dim altID As String = CStr(CType(GridAlternatives.DataSource, DataView).ToTable.Rows(ROW_ALTERNATIVES_START + i)(COL_ALT_ID))
                        If String2Double(cells(i), value) Then
                            For Each alt As RAAlternative In RA.Scenarios.Scenarios(id).Alternatives
                                If alt.ID = altID Then
                                    Select Case DisplayField
                                        Case DisplayFields.dfCost
                                            alt.Cost = value
                                        Case DisplayFields.dfPFailure
                                            alt.RiskOriginal = value
                                    End Select
                                    fChanged = True
                                End If
                            Next
                        End If
                    Next
                End If
                If fChanged Then RA.Save()
                'Case "display_mode"
                '    IsGraphicalMode = CInt(GetParam(args, "mode")) = 1 ' Table View / Graphical
            Case "group_mode"
                GroupingMode = CType(CInt(GetParam(args, "mode")), GroupingModes) ' By Scenario / By Alternative
            Case "view_mode"
                ViewMode = CType(CInt(GetParam(args, "mode")), ViewModes) ' Grid / Column / Pie / Donut
            Case "obj_mode"
                ObjPriorityMode = CType(CInt(GetParam(args, "mode")), ObjPriotityModes) ' Local / Global
            Case "option"   ' Solver options
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")) ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))   ' Anti-XSS
                Dim tVal As Integer = -1
                If sName <> "" AndAlso Integer.TryParse(sVal, tVal) Then
                    Dim chk As Boolean = tVal <> 0
                    If OPT_SHOW_AS_IGNORES Then chk = Not chk
                    SetOptionValue(sName, chk)
                    RA.Save()
                End If
            Case "opt_all"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))   ' Anti-XSS
                If sVal <> "" Then
                    SetOptionsAll(sVal = "1")
                    RA.Save()
                End If
            Case "use_ignore_options"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))   ' Anti-XSS
                If sVal <> "" Then
                    RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions = sVal = "1"
                    RA.Save()   ' D3240
                End If
            Case "use_base_case_options"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))   ' Anti-XSS
                If sVal <> "" Then
                    RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCaseOptions = sVal = "1"
                    RA.Save()   ' D3240
                End If
            Case "select_category"
                SelectedCategoryID = New Guid(GetParam(args, "cat_guid"))
            Case "funding_filter"
                FundedFilteringMode = CType(CInt(GetParam(args, "value")), FundedFilteringModes)
            Case "show_excluded"
                ShowExcludedAlts = GetParam(args, "val") = "1"
        End Select

        Solve()
    End Sub

    Private Sub SetOptionsAll(chk As Boolean)
        If OPT_SHOW_AS_IGNORES Then chk = Not chk ' D2931
        Dim settings As RASettings = CType(IIf(IsSingleScenarioChecked, RA.Scenarios.ActiveScenario.Settings, RA.Scenarios.GlobalSettings.ScenarioComparisonSettings), RASettings)
        With settings
            .CustomConstraints = chk
            .Dependencies = chk
            .FundingPools = chk
            .Groups = chk
            .Musts = chk
            .MustNots = chk
            .Risks = chk
            .TimePeriods = chk  ' D3824
        End With
        RA.Save() ' D3240
    End Sub

    Private Sub SetOptionValue(sID As String, chk As Boolean)
        Dim settings As RASettings = CType(IIf(IsSingleScenarioChecked, RA.Scenarios.ActiveScenario.Settings, RA.Scenarios.GlobalSettings.ScenarioComparisonSettings), RASettings)
        With settings
            Select Case sID
                Case "cbOptMusts"
                    .Musts = chk
                Case "cbOptMustNots"
                    .MustNots = chk
                Case "cbOptConstraints"
                    .CustomConstraints = chk
                Case "cbOptDependencies"
                    .Dependencies = chk
                Case "cbOptGroups"
                    .Groups = chk
                Case "cbOptFundingPools"
                    .FundingPools = chk
                Case "cbOptRisks"
                    .Risks = chk
                Case "cbOptTimePeriods" ' D3824
                    .TimePeriods = chk ' D3824 + D3828
                Case "cbBaseCase"
                    .UseBaseCase = CBool(IIf(OPT_SHOW_AS_IGNORES, Not chk, chk))
                Case "cbBCGroups"
                    .BaseCaseForGroups = CBool(IIf(OPT_SHOW_AS_IGNORES, Not chk, chk))
            End Select
        End With
        RA.Save() ' D3240
    End Sub


    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        If CheckVar(_PARAM_ACTION, "").ToLower = "download" Then Export()
    End Sub

    Public Overrides Sub VerifyRenderingInServerForm(ctrl As Control)
        'Verifies that the control is rendered
    End Sub

    Protected Sub Export()
        Try
            'RawResponseStart()
            Dim utf16 As New UTF8Encoding()
            Dim sWriter As New StringWriter()
            Dim hWriter As New HtmlTextWriter(sWriter)

            IsExport = True
            Solve()
            GridAlternatives.RenderControl(hWriter)
            IsExport = False

            Dim sContent As String = File_GetContent(MapPath("Scenarios.xls"), "")
            If Not sContent.Contains("%%") Then
                sContent = sWriter.ToString
            Else
                sContent = ParseTemplate(ParseTemplate(sContent, "%%title%%", Page.Title, False), "%%content%%", sWriter.ToString, False)
            End If

            DownloadContent(sContent, "application/vnd.ms-excel", GetProjectFileName(App.ActiveProject.ProjectName, "Scenarios", "", ".xls"), dbObjectType.einfFile, App.ProjectID) ' D6593

            'Response.ContentType = "application/vnd.ms-excel"
            'Response.AddHeader("Content-Length", utf16.GetByteCount(sContent).ToString)

            'Dim sDownloadName As String = SafeFileName(App.ActiveProject.ProjectName) 'Path.GetFileName(App.ActiveProject.ProjectName)
            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}.xls""", HttpUtility.UrlEncode(sDownloadName)))	' D6591

            'Response.Write(sContent)
            'If Response.IsClientConnected Then RawResponseEnd()

        Catch ex As Exception
        End Try
    End Sub

    Protected Sub DivPlotData_PreRender(sender As Object, e As EventArgs) Handles DivPlotData.PreRender
        DivPlotData.InnerText = PlotData
    End Sub

End Class