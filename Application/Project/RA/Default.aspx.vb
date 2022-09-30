Imports System.Collections.ObjectModel
Imports System.Drawing
Imports Telerik.Web.UI
Imports Canvas.RAGlobalSettings
Imports System.IO
Imports System.Net
Imports System.Web.Script.Serialization
Imports Chilkat
Imports OfficeOpenXml
Imports SearchOption = Microsoft.VisualBasic.FileIO.SearchOption
Imports Canvas.RASolver

Partial Class RABasePage
    Inherits clsComparionCorePage

    'Public Const ColumnsVersion As Integer = 4     ' D3174 + D3781 - A1143 (moved to Canvas.RAGlobalSettings.CurrentUIColumnsVersion)

    ' D3815 ===
    Private Const OPT_SAVE_XLSX As Boolean = False  ' approach for download as xls doesn't work as expected since we can export only an html, but must have xml data
    Private Const FILE_XLSX_FOLDER As String = "Download.xlsx"
    Private Const FILE_XLSX_CONTENT As String = "xl\sharedStrings.xml"
    ' D3815 ==

    Public _GROUP_TYPE_IMAGES() As String = {"ra_group_any_14.png", "ra_group_single_14.png", "ra_group_combined_14.png", "ra_group_all_or_none14.png"} ' D4914

    ' D2843 + D4803 + D4930 ===
    Private _OPT_COLS_DEF() As Integer = {raColumnID.ID, raColumnID.Name, raColumnID.isFunded, -raColumnID.LastFundedPeriod, raColumnID.Benefit, raColumnID.EBenefit, raColumnID.Risk, -raColumnID.ProbSuccess, raColumnID.ProbFailure, raColumnID.Cost, -raColumnID.isCostTolerance, -raColumnID.CostTolerance, raColumnID.Groups, raColumnID.isPartial, raColumnID.MinPercent, raColumnID.Musts, raColumnID.MustNot} ' D2893 + D2941 + D3174 + D4348 + D4353
    Public OPT_COLS_SORTING As raColumnID() = {raColumnID.ID, raColumnID.Name, raColumnID.isFunded, raColumnID.Benefit, raColumnID.EBenefit, raColumnID.Risk, raColumnID.ProbSuccess, raColumnID.ProbFailure, raColumnID.Cost, raColumnID.isCostTolerance, raColumnID.CostTolerance, raColumnID.isPartial, raColumnID.MinPercent, raColumnID.Musts, raColumnID.MustNot} ' D2892 + D3174 + D4348 + D4353
    Public OPT_COLS_DEFNAMES As String() = {"tblGUID", "tblID", "tblAlternativeName", "tblRAFunded", "tblRALastFunded", "tblBenefit", "tblEBenefit", "tblRisk", "tblProbSuccess", "tblProbFailure", "tblCost", "tblIsCostTolerance", "tblCostTolerance", "tblRAGroups", "tblPartial", "tblMinPrc", "tblMust", "tblMustNot", "tblCustConstr"}   ' D2941 + D3120 + D3174 + D4348 + D4353 // assign from resx on page init

    ' D4803 + D4930 ==
    Public OPT_COLS_SHOW_SELECTED As Boolean = True
    Public OPT_COLS_WIDTH As Integer() = {0, 30, 0, 140, 70, 70, 70, 70, 70, 85, 85, 85, 85, 85, 55, 75, 60, 60} ' D2941 + D3174 + D4348 + D4353 + D4803 + D4930
    Public OPT_COL_WIDTH_CONSTR As Integer = 85 ' D2886

    ' D2843 ==
    Public OPT_COL_WIDTH_TIMEPERIOD As Integer = 65 ' D3905
    Private OPT_PAGES_SHOW_MAX As Integer = 15      ' D4120

    ' D3777 ===
    Public Const BTN_SOLVE As Integer = 0
    Public Const BTN_AUTOSOLVE As Integer = 1
    Public Const BTN_SOLVER As Integer = 2
    Public Const BTN_SCENARIO As Integer = 6
    Public Const BTN_MANAGE_SCENARIOS As Integer = 7
    'Public Const BTN_SEL_ALTS As Integer = 8
    Public Const BTN_RISKS As Integer = 8
    Public Const BTN_TIMEPERIODS As Integer = 9  ' D3905
    Public Const BTN_TIMEPERIODS_SETTINGS As Integer = 10  ' D3932
    Public Const BTN_SETTINGS As Integer = 12
    Public Const BTN_MORE As Integer = 13  ' D4909
    Public Const BTN_DOWNLOAD As Integer = 14
    'Public Const BTN_SYNC As Integer = 16  ' -D3905 + D3931
    Public Const BTN_HELP As Integer = 15
    ' D3777 ==

    Public Const OPT_BAR_WIDTH As Integer = 40 ' D2840
    Public Const OPT_BAR_HEIGHT As Integer = 2 ' D2843
    Public Const OPT_BAR_COLOR_FILLED As String = "#8899cc" ' D2843
    Public Const OPT_BAR_COLOR_EMPTY As String = "#d0d0d0" ' D2843

    ' D2880 ===
    Public Const SESS_RA_SHOWFUNDEDONLY As String = "RA_ShowFundedOnly"
    Public Const SESS_RA_HIDEIGNORED As String = "RA_HideIgnored"   ' D3013
    Public Const SESS_RA_HIDENORISKS As String = "RA_HideNoRisks"   ' D3017
    Public Const SESS_RA_SHOWBARS As String = "RA_ShowTinyBars"
    Public Const SESS_RA_SHOWCHANGES As String = "RA_ShowChanges"   ' D3014
    Public Const SESS_RA_SHOWSURPLUS As String = "RA_ShowCCSurplus" ' D3540
    'Public Const SESS_RA_COLUMNS As String = "RA_ColsLst"           ' D2938 + D2941
    Public Const SESS_RA_FIXEDWIDTH As String = "RA_FixedColsWidth"
    'Public Const SESS_RA_SHOW_MIN_MAX As String = "RA_ShowMinMaxExtra"  ' D2887 - D2897
    Public Const SESS_RA_FP_WARNING As String = "no_fp_warn"    ' D3643
    Public Const SESS_RA_PAGES_CURRENT As String = "RA_PagesPage_{0}"   ' D4120
    Public Const SESS_RA_SHOW_TP As String = "RA_ShowTimeperiods_{0}"    ' D3976 + D4120 + D4803
    Public Const SESS_MASTER_PRJ_LIST As String = "MasterProjectIDs"    ' D3967
    ' D2880 ==
    'Public Const SESS_RA_SOLVER As String = "RA_Solver" ' D3630 -D3876
    Public Const SESS_GUROBI_BALANCE As String = "RA_GurobiBalance" ' D4068
    Public Const SESS_BIGMODEL_CHECKED As String = "RABigModelChecked_{0}"    ' D4125 + D4725
    Public Const SESS_BIGMODEL_OLD As String = "RAIsBigModel_{0}"   ' D4219
    Public Const SESS_RA_SOLVED_MODELID As String = "RASolvedModelID"   ' D7510

    Private Const SESS_RA_SORT_ORDER As String = "RASortIDs"        ' D4417
    Private Const SESS_TP_SELECTED_RESOURCE As String = "TP_ResID_{0}"

    Public Const SESS_RA_CUSTOM_SCENARIO As String = "CustomScenario"   ' D4909
    Public Const SESS_RA_INFEAS_COUNT As String = "InfSolutionsCount"   ' D6475

    Public Const COOKIE_ASK_GUROBI_CLOUD As String = "SolverB_Conf" ' D3894
    Public Const COOKIE_GROUP_DETAILS As String = "RAGrpDetails"    ' D4810

    Public Const OPT_RA_TP_VISIBLE As String = "RATimeperiodVisible_{0}"    ' D3915
    Public Const OPT_RA_PAGES_MODE As String = "RAPagesMode"        ' D4120

    Public Const OPT_SHOW_TP_CC As Boolean = True  ' D3917 + D3932

    Public Const OPT_SHOW_AS_IGNORES As Boolean = True
    Public Const OPT_DISABLE_EDITS_ON_IGNORE As Boolean = False ' D3125
    Public Const OPT_HIDE_CALCULATE_SOFT_CONSTRAINTS As Boolean = True  ' D3678
    Public Const OPT_SHOW_TIMEPERIODS_GRID As Boolean = False   ' D7079

    Public _TAB_COEFF As Short = -1       ' D3228 + D3376 + D4568 + D4821
    Public _TAB_IDX_MAX_ALTS As Short = -1   ' D3423 + D4568 + D4821

    Private _CatAttributesList As List(Of clsAttribute) = Nothing  ' D3336
    Private _NumAttributesList As List(Of clsAttribute) = Nothing  ' D3336

    Private isSolving As Boolean = False    ' D2840
    Private isReset As Boolean = False      ' D4681
    Private SaveRA As Boolean = False       ' D2909
    Private SaveMsg As String = ""          ' D3578
    Private SaveComment As String = ""      ' D3757
    Private isSorting As Boolean = False    ' D2982
    Private isExport As Boolean = False     ' D3189
    Private hasDisabledCCWithMax As Boolean = False ' D3540

    Private BaseCaseMax As Double = -1      ' D3090

    Public StartupMessage As String = ""    ' D3129

    Private SolveTime As New TimeSpan(0)    ' D3875 + D4120
    Private StartPageDT As DateTime         ' D3880

    Private _GurobiLicense As clsGurobiLicense = Nothing        ' D3894
    Private _GurobiMachines As clsGurobiMachine() = Nothing     ' D3894

    Private _isAjax As Boolean = False  ' D3917

    Private _showCents As Boolean? = Nothing        ' D4499

    Public AssociatedRiskModelName As String = ""       ' D4488

    Private _MaxCostLen As Integer = -1     ' D4490

    'Private _CustomScenario As RAScenario = Nothing ' D4909
    
    Public Sub New()
        MyBase.New(_PGID_RA_BASE)
        StartPageDT = Now   ' D3880
    End Sub

    ' D2839 ===
    ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property
    ' D2839 ==

    ' D4909 ===
    'Public Property CustomScenario As RAScenario
    '    Get
    '        If _CustomScenario Is Nothing Then
    '            If Session(SESS_RA_CUSTOM_SCENARIO + App.ProjectID.ToString) IsNot Nothing Then
    '                _CustomScenario = CType(Session(SESS_RA_CUSTOM_SCENARIO + App.ProjectID.ToString), RAScenario)
    '            End If
    '        End If
    '        Return _CustomScenario
    '    End Get
    '    Set(value As RAScenario)
    '        _CustomScenario = value
    '        Session.Remove(SESS_RA_CUSTOM_SCENARIO + App.ProjectID.ToString)
    '        If value IsNot Nothing Then Session.Add(SESS_RA_CUSTOM_SCENARIO + App.ProjectID.ToString, value)
    '    End Set
    'End Property

    Public ReadOnly Property Scenario As RAScenario
        Get
            'If CustomScenario IsNot Nothing Then
            '    Return CustomScenario
            'Else
            Return RA.Scenarios.ActiveScenario
            'End If
        End Get
    End Property
    ' D4909 ==

    ' D3179 ===
    Private Function SESS_FUNDED_BEFORE() As String
        Return String.Format("RA_FundedBefore_{0}", App.ProjectID)
    End Function
    ' D3179 ==

    ' D4821 ===
    Public ReadOnly Property TAB_COEFF As Short
        Get
            If _TAB_COEFF < 1 Then
                _TAB_COEFF = CShort(GridAlternatives.Columns.Count + 1)
            End If
            Return _TAB_COEFF
        End Get
    End Property

    Public ReadOnly Property TAB_IDX_MAX_ALTS As Short
        Get
            If _TAB_IDX_MAX_ALTS < 1 Then
                _TAB_IDX_MAX_ALTS = CShort(Math.Round(32767 / (TAB_COEFF + 1)))
            End If
            Return _TAB_IDX_MAX_ALTS
        End Get
    End Property
    ' D4821 ==

    ' D2840 + D3180 ===
    Public Property RA_AutoSolve As Boolean
        Get
            'Return RA.Scenarios.GlobalSettings.isAutoSolve AndAlso (Not RA.Solver.OPT_GUROBI_USE_CLOUD OrElse RA.Solver.SolverLibrary <> raSolverLibrary.raGurobi)   ' D3870 + D3894
            Return RA.Scenarios.GlobalSettings.isAutoSolve ' D3870 + D3894 + D3900
        End Get
        Set(value As Boolean)
            RA.Scenarios.GlobalSettings.isAutoSolve = value
            SessVar(SESS_USE_AJAX) = Nothing    ' D3530
        End Set
    End Property
    ' D2840 + D3180 ==

    ' D4125 ===
    Public Property isBigModelChecked As Boolean
        Get
            Return (Session(String.Format(SESS_BIGMODEL_CHECKED, App.ProjectID)) IsNot Nothing AndAlso CBool(Session(String.Format(SESS_BIGMODEL_CHECKED, App.ProjectID))))   ' D4132
        End Get
        Set(value As Boolean)
            Session(String.Format(SESS_BIGMODEL_CHECKED, App.ProjectID)) = True    ' D4132
        End Set
    End Property
    ' D4125 ==

    ' D3013 ===
    Public Property RA_HideIgnored As Boolean
        Get
            If Session(SESS_RA_HIDEIGNORED) Is Nothing Then Return GetCookie(SESS_RA_HIDEIGNORED, False.ToString) = True.ToString Else Return CBool(Session(SESS_RA_HIDEIGNORED))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_HIDEIGNORED) = value
            SetCookie(SESS_RA_HIDEIGNORED, value.ToString)
        End Set
    End Property
    ' D3013 ==

    ' D3017 ===
    Public Property RA_HideNoRisks As Boolean
        Get
            If Session(SESS_RA_HIDENORISKS) Is Nothing Then Return GetCookie(SESS_RA_HIDENORISKS, True.ToString) = True.ToString Else Return CBool(Session(SESS_RA_HIDENORISKS))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_HIDENORISKS) = value
            SetCookie(SESS_RA_HIDENORISKS, value.ToString)
        End Set
    End Property
    ' D3017 ==

    ' D3014 ===
    Public Property RA_ShowChanges As Boolean
        Get
            If Session(SESS_RA_SHOWCHANGES) Is Nothing Then Return GetCookie(SESS_RA_SHOWCHANGES, True.ToString) = True.ToString Else Return CBool(Session(SESS_RA_SHOWCHANGES))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SHOWCHANGES) = value
            SetCookie(SESS_RA_SHOWCHANGES, value.ToString)
        End Set
    End Property
    ' D3014 ==

    ' D3540 ===
    Public Property RA_ShowSurplus As Boolean
        Get
            If OPT_HIDE_CALCULATE_SOFT_CONSTRAINTS Then Return True
            If Session(SESS_RA_SHOWSURPLUS) Is Nothing Then Return GetCookie(SESS_RA_SHOWSURPLUS, True.ToString) = True.ToString Else Return CBool(Session(SESS_RA_SHOWSURPLUS))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SHOWSURPLUS) = value
            SetCookie(SESS_RA_SHOWSURPLUS, value.ToString)
        End Set
    End Property
    ' D3540 ==

    ' D2880 ===
    Public Property RA_ShowFundedOnly As Boolean
        Get
            If Session(SESS_RA_SHOWFUNDEDONLY) Is Nothing Then Return GetCookie(SESS_RA_SHOWFUNDEDONLY, False.ToString) = True.ToString Else Return CBool(Session(SESS_RA_SHOWFUNDEDONLY)) ' D2893
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SHOWFUNDEDONLY) = value
            SetCookie(SESS_RA_SHOWFUNDEDONLY, value.ToString)    ' D2893
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

    Public Property RA_FixedColsWidth As Boolean
        Get
            If RA_OPT_ALLOW_FREEZE_TABLES AndAlso RA.Scenarios.GlobalSettings.ShowFrozenHeaders Then Return False   ' D4803 // for reduce layout render issues
            If Session(SESS_RA_FIXEDWIDTH) Is Nothing Then Return GetCookie(SESS_RA_FIXEDWIDTH, True.ToString) = True.ToString Else Return CBool(Session(SESS_RA_FIXEDWIDTH)) ' D2893
        End Get
        Set(value As Boolean)
            Session(SESS_RA_FIXEDWIDTH) = value
            SetCookie(SESS_RA_FIXEDWIDTH, value.ToString)    ' D2893
        End Set
    End Property

    ' D3643 ===
    Public Property RA_ShowFPWarning As Boolean
        Get
            If Session(SESS_RA_FP_WARNING) Is Nothing Then Return True Else Return CBool(Session(SESS_RA_FP_WARNING))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_FP_WARNING) = value
        End Set
    End Property
    ' D3643 ==

    ' D2893 + D3185 ===
    Public Property RA_Columns As Integer()
        Get
            If RA.Scenarios.GlobalSettings.ColumnsVersion <> CurrentUIColumnsVersion OrElse RA.Scenarios.GlobalSettings.ColumnsList.Length < 2 Then  ' D3781 + A1143
                Dim tCols As Integer() = OPT_COLS_DEF
                'Dim sVal As String = ""
                'If sVal <> "" Then
                '    Dim sVals() As String = sVal.Split(";")
                '    Dim i As Integer = 0
                '    For Each sID As String In sVals
                '        Dim ID As Integer
                '        If i < OPT_COLS_DEF.Count AndAlso Integer.TryParse(sID, ID) Then tCols(i) = ID
                '        i += 1
                '    Next
                'End If
                ''RA.Scenarios.GlobalSettings.OptionValue(raOptionName.raAllocationColumns) = tCols
                ''RA.Scenarios.GlobalSettings.OptionValue(raOptionName.raAllocationColumnsVersion) = ColumnsVersion  ' D3174 + D3185
                Return tCols
            Else
                Return RA.Scenarios.GlobalSettings.ColumnsList  ' D3781
            End If
        End Get
        Set(value As Integer())
            RA.Scenarios.GlobalSettings.ColumnsList = value ' D3781
            RA.Scenarios.GlobalSettings.ColumnsVersion = CurrentUIColumnsVersion  ' D3174 + D3185 + D3781 + A1143
        End Set
    End Property
    ' D2880 + D2893 + D3185 ==

    ' D3630 ===
    Public Property RA_Solver As raSolverLibrary
        Get
            ' D3924 ===
            If RA.Solver.SolverLibrary = raSolverLibrary.raGurobi AndAlso Not App.isGurobiAvailable Then
                If App.isXAAvailable Then   ' D4512
                    RA.Solver.SolverLibrary = raSolverLibrary.raXA
                    App.ActiveProject.ProjectManager.Parameters.RASolver = raSolverLibrary.raXA
                Else
                    ' D7543 ===
                    If App.isBaronAvailable Then
                        RA.Solver.SolverLibrary = raSolverLibrary.raBaron
                        App.ActiveProject.ProjectManager.Parameters.RASolver = raSolverLibrary.raBaron
                    End If
                End If
                ' D7543 ==
            End If
            ' D3924 ==
            Return RA.Solver.SolverLibrary  ' D3877
        End Get
        Set(value As raSolverLibrary)
            If (value = raSolverLibrary.raBaron AndAlso App.isBaronAvailable) OrElse (value = raSolverLibrary.raGurobi AndAlso App.isGurobiAvailable) OrElse (value = raSolverLibrary.raXA AndAlso App.isXAAvailable) Then  ' D7543
                ' D3877 ===
                If RA.Solver.SolverLibrary <> value Then
                    RA.Solver.SolverLibrary = value
                    App.ActiveProject.ProjectManager.Parameters.RASolver = value
                End If
            End If
            ' D3877 ==
        End Set
    End Property
    ' D3630 ==

    ' D4120 ===
    Public Property RA_Pages_Mode As ecRAGridPages
        Get
            Dim tVal As Integer = App.ActiveProject.ProjectManager.Parameters.Parameters.GetParameterValue(OPT_RA_PAGES_MODE, -1)
            If tVal = -1 Then    ' D4121  + D4480
                If Scenario.AlternativesFull.Count > 20 Then
                    If Scenario.AlternativesFull.Count > 30 Then
                        tVal = CInt(ecRAGridPages.Page20)
                    Else
                        tVal = CInt(ecRAGridPages.Page15)
                    End If
                Else
                    tVal = CInt(ecRAGridPages.NoPages)
                End If
                App.ActiveProject.ProjectManager.Parameters.Parameters.SetParameterValue(OPT_RA_PAGES_MODE, tVal)
            End If
            If tVal < 0 Then tVal = 0 ' D4480
            Return CType(tVal, ecRAGridPages)
        End Get
        Set(value As ecRAGridPages)
            App.ActiveProject.ProjectManager.Parameters.Parameters.SetParameterValue(OPT_RA_PAGES_MODE, CInt(value))
        End Set
    End Property

    Public Property RA_Pages_CurPage As Integer
        Get
            If Session(String.Format(SESS_RA_PAGES_CURRENT, App.ProjectID)) Is Nothing Then Return 0 Else Return CInt(Session(String.Format(SESS_RA_PAGES_CURRENT, App.ProjectID))) ' D4124
        End Get
        Set(value As Integer)
            Session(String.Format(SESS_RA_PAGES_CURRENT, App.ProjectID)) = value    ' D4124
        End Set
    End Property
    ' D4120 ==
    Public Property TP_RES_ID As String
        Get
            If Session(String.Format(SESS_TP_SELECTED_RESOURCE, App.ProjectID)) Is Nothing Then
                If Scenario IsNot Nothing AndAlso Scenario.TimePeriods IsNot Nothing AndAlso Scenario.TimePeriods.EnabledResources(0) IsNot Nothing Then
                    Return JS_SafeString(Scenario.TimePeriods.EnabledResources(0).ID.ToString)
                Else
                    Return "0"
                End If
            Else
                Return Session(String.Format(SESS_TP_SELECTED_RESOURCE, App.ProjectID)).ToString
            End If
        End Get
        Set(value As String)
            Session(String.Format(SESS_TP_SELECTED_RESOURCE, App.ProjectID)) = value
        End Set
    End Property
    ' D3905 ===
    Public Property RA_ShowTimeperiods As Boolean
        Get
            If Not OPT_SHOW_TIMEPERIODS_GRID Then Return False  ' D7079
            'Return App.ActiveProject.ProjectManager.Parameters.RAShowTimeperiods   ' -D3976
            If Scenario.TimePeriods.Periods.Count = 0 Then Return False ' D4487
            If Session(String.Format(SESS_RA_SHOW_TP, App.ProjectID)) IsNot Nothing Then Return CBool(Session(String.Format(SESS_RA_SHOW_TP, App.ProjectID))) Else Return False ' D3976 + D4120
        End Get
        Set(value As Boolean)
            'App.ActiveProject.ProjectManager.Parameters.RAShowTimeperiods = value  ' -D3976
            If OPT_SHOW_TIMEPERIODS_GRID Then Session(String.Format(SESS_RA_SHOW_TP, App.ProjectID)) = value  ' D3976 + D4120 + D7079
        End Set
    End Property
    ' D3905 ==

    ' D3915 ===
    Public Property RA_TimePeriodVisible(ColID As Integer) As Boolean
        Get
            Return App.ActiveProject.ProjectManager.Parameters.Parameters.GetParameterValue(String.Format(OPT_RA_TP_VISIBLE, ColID), False) ' D3917
        End Get
        Set(value As Boolean)
            App.ActiveProject.ProjectManager.Parameters.Parameters.SetParameterValue(String.Format(OPT_RA_TP_VISIBLE, ColID), value)
        End Set
    End Property
    ' D3915 ==

    ' D3877 ===
    Public Property RA_XAStrategy() As Integer
        Get
            Return RA.Solver.XAStrategy
        End Get
        Set(value As Integer)
            If RA.Solver.XAStrategy <> value Then
                RA.Solver.XAStrategy = value
                App.ActiveProject.ProjectManager.Parameters.RASolver_XAStrategy = value
                'App.ActiveProject.ProjectManager.Parameters.Save()
            End If
        End Set
    End Property

    Public Property RA_XAVariation() As String
        Get
            Return RA.Solver.XAVariation
        End Get
        Set(value As String)
            If RA.Solver.XAVariation <> value AndAlso RA.Solver.XA_VARIATIONS.Contains(value) Then
                RA.Solver.XAVariation = value
                App.ActiveProject.ProjectManager.Parameters.RASolver_XAVariation = value
                'App.ActiveProject.ProjectManager.Parameters.Save()
            End If
        End Set
    End Property

    Public Property RA_XATimeout() As Integer
        Get
            Return RA.Solver.XATimeOutGlobal
        End Get
        Set(value As Integer)
            If RA.Solver.XATimeOutGlobal <> value Then
                RA.Solver.XATimeOutGlobal = value
                App.ActiveProject.ProjectManager.Parameters.RASolver_XATimeout = value
                'App.ActiveProject.ProjectManager.Parameters.Save()
            End If
        End Set
    End Property

    Public Property RA_XATimeoutUnchanged() As Integer
        Get
            Return RA.Solver.XATimeoutUnchanged
        End Get
        Set(value As Integer)
            If RA.Solver.XATimeoutUnchanged <> value Then
                RA.Solver.XATimeoutUnchanged = value
                App.ActiveProject.ProjectManager.Parameters.RASolver_XATimeoutUnchanged = value
                'App.ActiveProject.ProjectManager.Parameters.Save()
            End If
        End Set
    End Property
    ' D3877 ==

    ' D4810 ===
    Public Property RA_ShowGroupDetails As Boolean
        Get
            Return GetCookie(COOKIE_GROUP_DETAILS, "0") = "1"
        End Get
        Set(value As Boolean)
            SetCookie(COOKIE_GROUP_DETAILS, Bool2Num(value).ToString)
        End Set
    End Property
    ' D4810 ==

    ' D4499 ===
    Public Property RA_ShowCents As Boolean
        Get
            If Not _showCents.HasValue Then
                Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_CENTS_ID, UNDEFINED_USER_ID))
            Else
                Return _showCents.Value
            End If
        End Get
        Set(value As Boolean)
            _showCents = value
            WriteSetting(App.ActiveProject, ATTRIBUTE_RISK_SHOW_CENTS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public ReadOnly Property Cents As Integer
        Get
            Return If(RA_ShowCents, 2, 0)
        End Get
    End Property
    ' D4499 ==

    ' D3530 ===
    Public Function SESS_USE_AJAX() As String
        Return String.Format("RA_Ajax_{0}", App.ProjectID)
    End Function

    Public Property UseAjax As Boolean
        Get
            Return Not RA_AutoSolve AndAlso SessVar(SESS_USE_AJAX) = "1"
        End Get
        Set(value As Boolean)
            SessVar(SESS_USE_AJAX) = If(value, "1", "0")
        End Set
    End Property
    ' D3530 ==

    ' D3570 ===
    Public Function IgnoreFP() As Boolean
        Return False    ' D3888
        'Return RA_Solver = raSolverLibrary.raMSF AndAlso Scenario.FundingPools.Pools.Count > 0   ' D3877
    End Function
    ' D3570 ==

    ' D3880 ===
    Public Function IgnoreTP() As Boolean
        Return False    ' D3888
        'Return RA_Solver = raSolverLibrary.raMSF AndAlso App.ActiveProject.ProjectManager.Parameters.TimeperiodsHasData(RA.Scenarios.ActiveScenarioID)
    End Function
    ' D3880 ==

    ' D3905 ===
    Public Function isTimeperiodsVisible() As Boolean
        Return (CheckVar("tp", False) OrElse Not isExport) AndAlso (Not RA.Scenarios.GlobalSettings.isBigModel OrElse isExport) AndAlso Scenario.TimePeriods.Periods.Count > 0 AndAlso Scenario.Settings.TimePeriods ' D3943 + D4128 + D4149 + D4222 + D4477 + D4725
    End Function
    ' D3905 ==

    ' D2897 ===
    Public ReadOnly Property RA_ShowMinMaxExtra As Boolean
        Get
            Return False
        End Get
    End Property
    ' D2897 ==

    Private Property FundedBefore As Dictionary(Of String, Double)
        Get
            If Session(SESS_FUNDED_BEFORE) IsNot Nothing Then Return CType(Session(SESS_FUNDED_BEFORE), Dictionary(Of String, Double)) Else Return (Nothing)
        End Get
        Set(value As Dictionary(Of String, Double))
            Session(SESS_FUNDED_BEFORE) = value
        End Set
    End Property

    ' D3223 ===
    Private ReadOnly Property SESS_RA_SORT_EXPR As String
        Get
            Return String.Format("RA_SORT_EXPR_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Private ReadOnly Property SESS_RA_SORT_DIR As String
        Get
            Return String.Format("RA_SORT_DIREC_{0}", App.ProjectID.ToString)
        End Get
    End Property
    ' D3223 ==

    ' D6475 + D7086 ===
    Private ReadOnly Property SESS_RA_INFEAS_CONSTR As String
        Get
            Return String.Format("RA_INFEAS_CONSTR_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property RA_Infeasibility_Solutions_Count As Integer
        Get
            Dim sCnt As String = SessVar(SESS_RA_INFEAS_COUNT)
            If String.IsNullOrEmpty(sCnt) Then Return 3 Else Return CInt(sCnt)  ' D7086
        End Get
        Set(value As Integer)
            SessVar(SESS_RA_INFEAS_COUNT) = value.ToString
        End Set
    End Property

    Public Property RA_Infeasibility_Constraints_List As List(Of String)
        Get
            If Session(SESS_RA_INFEAS_CONSTR) Is Nothing Then
                Dim lst As New List(Of String)
                Session(SESS_RA_INFEAS_CONSTR) = lst
                Return lst
            Else
                Return CType(Session(SESS_RA_INFEAS_CONSTR), List(Of String))
            End If
        End Get
        Set(value As List(Of String))
            Session(SESS_RA_INFEAS_CONSTR) = value
        End Set
    End Property
    ' D6475 + D7086 ==

    'A1010 ===
    Private Sub SaveSetting(ID As Guid, valueType As AttributeValueTypes, value As Object)
        With App.ActiveProject.ProjectManager
            .Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, valueType, value, Guid.Empty, Guid.Empty)
            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
        End With
    End Sub

    Public Property ShowAltAttributes As Boolean
        Get
            Return CBool(RA.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_SHOW_ALT_ATTRIBUTES_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            SaveSetting(ATTRIBUTE_RA_SHOW_ALT_ATTRIBUTES_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property
    'A1010 ==

    '' D2887 ===
    'Public Property RA_ShowMinMaxExtra As Boolean
    '    Get
    '        If Session(SESS_RA_SHOW_MIN_MAX) Is Nothing Then Return CBool(GetCookie(SESS_RA_SHOW_MIN_MAX, False.ToString)) Else Return CBool(Session(SESS_RA_SHOW_MIN_MAX)) ' D2893
    '    End Get
    '    Set(value As Boolean)
    '        Session(SESS_RA_SHOW_MIN_MAX) = value
    '        SetCookie(SESS_RA_SHOW_MIN_MAX, value.ToString)    ' D2893
    '    End Set
    'End Property
    '' D2887 ==

    ' D3336 ===
    Public ReadOnly Property CatAttributesList() As List(Of clsAttribute)
        Get
            If _CatAttributesList Is Nothing Then
                With App.ActiveProject.ProjectManager
                    _CatAttributesList = New List(Of clsAttribute)
                    .Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                    If .Attributes IsNot Nothing AndAlso .Attributes.AttributesList IsNot Nothing AndAlso .Attributes.AttributesList.Count > 0 Then
                        .Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, -1)
                        For Each attr In .Attributes.GetAlternativesAttributes
                            If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                                _CatAttributesList.Add(attr)
                            End If
                        Next
                    End If
                End With
            End If
            Return _CatAttributesList
        End Get
    End Property

    Public ReadOnly Property NumAttributesList() As List(Of clsAttribute)
        Get
            If _NumAttributesList Is Nothing Then
                With App.ActiveProject.ProjectManager
                    _NumAttributesList = New List(Of clsAttribute)
                    .Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                    If .Attributes IsNot Nothing AndAlso .Attributes.AttributesList IsNot Nothing AndAlso .Attributes.AttributesList.Count > 0 Then
                        .Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, -1)
                        For Each attr In .Attributes.GetAlternativesAttributes
                            If Not attr.IsDefault AndAlso (attr.ValueType = AttributeValueTypes.avtLong OrElse attr.ValueType = AttributeValueTypes.avtDouble) Then
                                _NumAttributesList.Add(attr)
                            End If
                        Next
                    End If
                End With
            End If
            Return _NumAttributesList
        End Get
    End Property
    ' D3336 ==

    ' D3894 ===
    Public ReadOnly Property GurobiLicense As clsGurobiLicense
        Get
            If _GurobiLicense Is Nothing Then
                If RA_Solver = raSolverLibrary.raGurobi AndAlso RA.Solver.OPT_GUROBI_USE_CLOUD Then    ' D3924 + D4512
                    Dim sError As String = ""
                    Dim res As String = GurobiCloudDataGet(RA.Solver.OPT_GUROBI_REST_LICENSES, sError)
                    Try
                        If sError = "" AndAlso res <> "" Then
                            Dim ser = New JavaScriptSerializer()
                            _GurobiLicense = ser.Deserialize(Of clsGurobiLicense())(res)(0)
                        End If
                    Catch ex As Exception
                        sError = "Can't parse"
                    End Try
                End If
                If _GurobiLicense Is Nothing Then _GurobiLicense = New clsGurobiLicense
            End If
            Return _GurobiLicense
        End Get
    End Property

    Public ReadOnly Property GurobiMachines As clsGurobiMachine()
        Get
            If _GurobiMachines Is Nothing Then
                If RA_Solver = raSolverLibrary.raGurobi AndAlso RA.Solver.OPT_GUROBI_USE_CLOUD Then    ' D3924 + D4512
                    Dim sError As String = ""
                    Dim res As String = GurobiCloudDataGet(String.Format("{0}?poolId={1}", RA.Solver.OPT_GUROBI_REST_MACHINES, RA.Solver.OPT_GUROBI_POOL_ID), sError)
                    Try
                        If sError = "" AndAlso res <> "" Then
                            Dim ser = New JavaScriptSerializer()
                            _GurobiMachines = ser.Deserialize(Of clsGurobiMachine())(res)
                        End If
                    Catch ex As Exception
                        sError = "Can't parse"
                    End Try
                End If
                If _GurobiMachines Is Nothing Then _GurobiMachines = New clsGurobiMachine() {}
            End If
            Return _GurobiMachines
        End Get
    End Property
    ' D3894 ==

    Dim AltColors As String() = {"#95c5f0", "#fa7000", "#9d27a8", "#e3e112", "#00687d", "#407000", "#f24961", "#663d2e", "#9600fa", "#ffbde6", "#00c49f", "#7280c4", "#009180", "#e33000", "#80bdff", "#a10040", "#0affe3", "#00523c", "#919100", "#5c00f7", "#a15f00", "#cce6ff", "#00465c", "#adff69", "#f24ba0", "#0dff87", "#ff8c47", "#349400", "#b3b3a1", "#a10067", "#ba544a", "#edc2d1", "#00e8c3", "#3f0073", "#5ec1f7", "#6e00b8", "#f5f5c4", "#e33000", "#52ba00", "#ff943b", "#0079db", "#f0e6c0", "#ffb517", "#cf0076", "#e8cfc9"}

    Public Function GetTimePeriodsCount() As Integer
        If Scenario IsNot Nothing Then
            Return Scenario.TimePeriods.Periods.Count
        End If
        Return 0
    End Function

    Public Function GetTimeperiodsDistribMode() As Integer
        Return PM.Parameters.TimeperiodsDistributeMode
    End Function

    Public Function GetRAProjectResources(altID As String, startPeriodID As Integer, duration As Integer) As String
        Dim retVal As String = ""
        If Scenario IsNot Nothing AndAlso Scenario.TimePeriods.EnabledResources IsNot Nothing Then
            Dim PeriodsCount = Scenario.TimePeriods.Periods.Count
            Dim alt = Scenario.GetAvailableAlternativeById(altID)
            For Each res As RAResource In Scenario.TimePeriods.EnabledResources
                Dim totalCost As Double = alt.Cost
                If res.ConstraintID >= 0 Then
                    totalCost = Scenario.Constraints.GetConstraintValue(res.ConstraintID, altID)
                End If

                Dim valuesString = ""
                For i = startPeriodID To startPeriodID + duration - 1
                    Dim resVal As Double = Scenario.TimePeriods.PeriodsData.GetResourceValue(i, altID, res.ID)
                    valuesString += If(valuesString <> "", ",", "") + JS_SafeNumber(resVal)
                Next
                retVal += If(retVal <> "", ",", "") + String.Format("{{idx: {0}, id:'{1}', name:'{2}', values: [{3}], totalValue: {4}}}", res.ConstraintID, JS_SafeString(res.ID.ToString), JS_SafeString(res.Name), valuesString, JS_SafeNumber(totalCost))  ' D3918
            Next
        End If
        Return retVal
    End Function

    Public Function GetRAProjects() As String
        Dim retVal As String = ""
        If Scenario IsNot Nothing Then
            ' D3982 ===
            RA.Scenarios.SyncLinkedConstraintsToResources() ' D4913
            Scenario.TimePeriods.LinkResourcesFromConstraints()
            If Scenario.TimePeriods.AllocateResourceValues() Then
                App.ActiveProject.SaveRA("Edit timeperiods", , , "Auto distribute resource value by periods")
            End If
            Dim tAlts As List(Of RAAlternative) = Scenario.Alternatives  ' D3763
            'tAlts.Sort(New RAAlternatives_Comparer(Math.Abs(CInt(RA.Scenarios.GlobalSettings.SortBy)), CInt(RA.Scenarios.GlobalSettings.SortBy) < 0, RA))
            DoSort(tAlts)  ' D4417
            For Each alt In tAlts
                Dim isFunded As Boolean = Scenario.Settings.TimePeriods AndAlso RA.Solver.SolverState = raSolverState.raSolved AndAlso alt.DisplayFunded > 0.0
                Dim AltTPData = Scenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                Dim AltResDataString = GetRAProjectResources(JS_SafeString(alt.ID), AltTPData.StartPeriod, AltTPData.Duration)
                Dim tGUID As New Guid(alt.ID)
                Dim tRealAlt As clsNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tGUID)
                Dim fHasInfodoc As Boolean = tRealAlt IsNot Nothing AndAlso tRealAlt.InfoDoc <> ""
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{id:'{0}',idx:{1},name:'{2}',periodStart:{3},periodDuration:{4},periodMin:{5},periodMax:{6},color:'{7}',isFunded:{8},resources:[{9}],hasinfodoc:{10}}}", JS_SafeString(alt.ID), alt.SortOrder, JS_SafeString(alt.Name), AltTPData.StartPeriod, AltTPData.Duration, AltTPData.MinPeriod, AltTPData.MaxPeriod, AltColors((alt.SortOrder - 1) Mod 45), Bool2JS(isFunded), AltResDataString, Bool2JS(fHasInfodoc))
                ' D3982 ==
            Next
        End If
        retVal = String.Format("[{0}]", retVal)
        Return retVal
    End Function

    Public Function GetTimePeriodsType() As Integer
        Return Scenario.TimePeriods.PeriodsType
    End Function

    Public Function GetTimePeriodNameFormat() As String
        Return Scenario.TimePeriods.NamePrefix
    End Function

    Public Function GetTimelineStartDate() As String
        Dim aDate As Date = Scenario.TimePeriods.TimelineStartDate.Date
        If aDate.Year < 1000 Then
            aDate = Now()
            Scenario.TimePeriods.TimelineStartDate = aDate
            App.ActiveProject.SaveRA("Edit timeperiods", , , "Set start date") ' D3791
        End If
        Dim aYear As Integer = aDate.Year
        Dim aMM As Integer = aDate.Month
        Dim aDD As Integer = aDate.Day
        Return String.Format("{0:D2}/{1:D2}/{2}", aMM, aDD, aYear)
    End Function

    Public Function GetRAPortfolioResources() As String
        Dim retVal As String = ""
        If Scenario IsNot Nothing AndAlso Scenario.TimePeriods.EnabledResources IsNot Nothing Then
            Dim PeriodsCount = Scenario.TimePeriods.Periods.Count
            For Each res As RAResource In Scenario.TimePeriods.EnabledResources
                For Each tp As RATimePeriod In Scenario.TimePeriods.Periods
                    Dim resMinVal As Double = tp.GetResourceMinValue(res.ID)
                    Dim resMaxVal As Double = tp.GetResourceMaxValue(res.ID)
                    retVal += If(retVal <> "", ",", "") + String.Format("{{resID:'{0}', periodID:{1}, minVal:{2}, maxVal:{3}}}", JS_SafeString(res.ID.ToString), Scenario.TimePeriods.Periods.IndexOf(tp), JS_SafeNumber(resMinVal), JS_SafeNumber(resMaxVal))  ' D3918
                Next
            Next
        End If
        retVal = String.Format("[{0}]", retVal)
        Return retVal
    End Function

    Public Function GetPeriodResults() As String
        Dim sPeriodResults As String = ""
        If Scenario IsNot Nothing Then
            If RA.Scenarios.GlobalSettings.isAutoSolve AndAlso SolveTime.TotalMilliseconds < 10 Then Solve() ' D3900 + D3969 + D4120 --disable due to second Solve, which in not expected and not required
            Dim ATPResults = Scenario.TimePeriods.TimePeriodResults
            For Each item In ATPResults
                sPeriodResults += If(sPeriodResults <> "", ",", "") + String.Format("{{projID:'{0}', start:{1}}}", JS_SafeString(item.Key), item.Value)
            Next
        End If
        sPeriodResults = String.Format("[{0}]", sPeriodResults)
        Return sPeriodResults
    End Function

    Public Function GetResourcesList() As String
        Dim retVal As String = ""
        If Scenario IsNot Nothing AndAlso Scenario.TimePeriods.EnabledResources IsNot Nothing Then
            Scenario.TimePeriods.LinkResourcesFromConstraints()
            For Each res As RAResource In Scenario.TimePeriods.EnabledResources
                retVal += If(retVal <> "", ",", "") + String.Format("{{id:'{0}', name:'{1}'}}", JS_SafeString(res.ID.ToString), JS_SafeString(res.Name))    ' D3918
            Next
        End If
        Return "[" + retVal + "]"
    End Function

    ' D4417 ===
    Private Sub DoSort(ByRef tAltsList As List(Of RAAlternative))
        If RA_AutoSolve OrElse isSolving OrElse isSorting Then
            tAltsList.Sort(New RAAlternatives_Comparer(CType(Math.Abs(RA.Scenarios.GlobalSettings.SortBy), raColumnID), RA.Scenarios.GlobalSettings.SortBy < 0, RA))    ' D4862
        Else
            Dim sList As String = SessVar(SESS_RA_SORT_ORDER)
            Dim IDs As String() = sList.Split(CType(",", Char()))
            If IDs.Count = tAltsList.Count Then
                Dim tNewLst As New List(Of RAAlternative)
                For Each sID As String In IDs
                    Dim tAlt As RAAlternative = tAltsList.Find(Function(x) x.ID = sID)
                    If tAlt IsNot Nothing Then
                        tNewLst.Add(tAlt)
                    Else
                        Exit For
                    End If
                Next
                If tAltsList.Count = tNewLst.Count Then tAltsList = tNewLst
            End If
        End If
    End Sub
    ' D4417 ==

    ' D3122 ===
    Protected Sub Page_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        If CheckVar(_PARAM_ACTION, "") = "export" Then isExport = True ' D3189
        If isExport Then StorePageID = False   ' D6591
    End Sub
    ' D3122 ==

    ' D3189 ===
    Protected Sub Page_PreRenderComplete(sender As Object, e As EventArgs) Handles Me.PreRenderComplete
        If CheckVar(_PARAM_ACTION, "") = "export" Then
            DebugInfo("Start export")
            ' D3193 ===
            Select Case CheckVar("type", "xml").Trim.ToLower
                Case "oml"
                    Export2File(raSolverExport.raOML)   ' D3226
                Case "mps"
                    Export2File(raSolverExport.raMPS)   ' D3236
                    ' D3870 ===
                Case "logs"   ' D3488
                    Select Case RA_Solver   ' D3924
                        Case raSolverLibrary.raXA
                            ExportXALogs()  ' D3488
                        Case raSolverLibrary.raGurobi
                            ExportGurobiLogs()
                        Case raSolverLibrary.raBaron    ' D7582
                            ExportBaronLogs()   ' D7582
                    End Select
                    ' D3870 ==
                Case "alloc"    ' D4582
                    ExportAllocationComparisons()   ' D4582
                Case Else
                    Export2Excel()
            End Select
            ' D3193 ==
            DebugInfo("Export end")
        End If
        If SaveRA Then App.ActiveProject.SaveRA(If(SaveMsg = "", "", "RA: " + SaveMsg), , SaveMsg <> "", SaveComment) ' D2855 + D2909 + D578 + D3757

        ' D3362 ===
        FundedBefore = New Dictionary(Of String, Double)
        For Each tAlt In Scenario.Alternatives
            If tAlt.DisplayFunded > 0 Then FundedBefore.Add(tAlt.ID, tAlt.DisplayFunded) 'A0939
        Next
        ' D3362 ==

        ' D3894 ===
        Dim fSolverActive As Boolean = True
        If RA_Solver = raSolverLibrary.raGurobi AndAlso RA.Solver.OPT_GUROBI_USE_CLOUD Then ' D3924
            If GurobiMachines Is Nothing OrElse GurobiMachines.Count = 0 Then fSolverActive = False
        End If
        SolverActive.Value = If(fSolverActive, "1", "0")
        ' D3894 ==
    End Sub
    ' D3189 ==

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If Not IsPostBack AndAlso Not isCallback AndAlso Not _isAjax Then   ' D3917
            DebugInfo("Init RA page start")
            AlignVerticalCenter = False
            isSorting = True    ' D2982
            If Scenario.Alternatives.Count = 0 Then
                ' D2876 ===
                RadToolBarMain.Items(BTN_SOLVE).Enabled = False ' solve     ' D4138
                'RadToolBarMain.Items(BTN_SEL_ALTS).Enabled = False ' sel alts
                RadToolBarMain.Items(BTN_RISKS).Enabled = False ' risks
                RadToolBarMain.Items(BTN_SETTINGS).Enabled = False ' settings
                RadToolBarMain.Items(BTN_AUTOSOLVE).Enabled = False ' auto-solve
                RadToolBarMain.Items(BTN_SOLVER).Enabled = False ' D3879
                pnlIgnore.Disabled = True
                ' -D4530
                '' D3818 ===
                'With Scenario.Settings
                '    .UseBaseCase = False
                '    .CustomConstraints = False
                '    .Dependencies = False
                '    .FundingPools = False
                '    .Groups = False
                '    .MustNots = False
                '    .Musts = False
                '    .Risks = False
                '    .TimePeriods = False
                'End With
                '' D3818 ==
            End If
            CType(RadToolBarMain.Items(BTN_AUTOSOLVE), RadToolBarButton).Checked = RA_AutoSolve ' D3139
            If RA_Solver = raSolverLibrary.raGurobi AndAlso RA.Solver.OPT_GUROBI_USE_CLOUD Then RadToolBarMain.Items(BTN_AUTOSOLVE).Enabled = False ' D3870 + D3894 + D3924
            If RA_Solver = raSolverLibrary.raGurobi AndAlso Not App.isGurobiAvailable AndAlso App.isXAAvailable Then RA_Solver = raSolverLibrary.raXA ' D3924 + D4512
            If RA_Solver = raSolverLibrary.raXA AndAlso Not App.isXAAvailable AndAlso App.isBaronAvailable Then RA_Solver = raSolverLibrary.raBaron     ' D7543
            ' D3120 ===
            RadToolBarMain.Items(BTN_SOLVE).Text = ResString("btnRASolve")
            '-A1018 RadToolBarMain.Items(1).Text = ResString("lblScenario") + ":"
            RadToolBarMain.Items(BTN_MANAGE_SCENARIOS).ToolTip = ResString("lblRAManageScenarios")
            'RadToolBarMain.Items(5).Text = ResString("lblRACustConstr") + "&hellip;"
            With CType(RadToolBarMain.Items(BTN_RISKS), RadToolBarDropDown)
                .Text = ResString("lblRARisks")
                .Buttons(0).Text = ResString("lblRACreateARM")
                .Buttons(1).Text = ResString("lblRAAssociateARM")
                .Buttons(2).Text = ResString("lblRAOpenARM")
                .Buttons(3).Text = ResString("lblRASynAltsRM")
                .Buttons(4).Text = ResString("lblRAImportFromARM")
                .Buttons(5).Text = ResString("lblRAImportPoSFromARM")   ' D3759
                .Buttons(6).Text = ResString("lblRADetachARM")
                .Buttons(7).Text = ResString("lblRADeleteRisks")    ' D3175
            End With
            With CType(RadToolBarMain.Items(BTN_SETTINGS), RadToolBarDropDown)
                .Text = ResString("lblRASettings")
                .Buttons(0).Text = ResString("lblRAOptColumns") + "&hellip;"
                .Buttons(10).Enabled = RadToolBarMain.Items(BTN_TIMEPERIODS).Enabled    ' D3944 // expand tp
                .Buttons(10).Visible = RadToolBarMain.Items(BTN_TIMEPERIODS).Visible AndAlso isTimeperiodsVisible()    ' D3944 + D4148
                .Buttons(11).Visible = RA_OPT_ALLOW_FREEZE_TABLES  ' D3623 + D3950 + D6464 // freeze    / ShowDraftPages()
                '.Buttons(11).Visible = ShowDraftPages() ' D3661 -D3875: replace with icon/dialog
                .Buttons(2).Enabled = Not .Buttons(11).Visible OrElse Not RA.Scenarios.GlobalSettings.ShowFrozenHeaders ' D4783
                'If Not ShowDraftPages() AndAlso RA_Solver <> raSolverLibrary.raXA Then    ' D3875 + D3877
                If Not App.isGurobiAvailable AndAlso RA_Solver = raSolverLibrary.raGurobi AndAlso App.isXAAvailable Then    ' D3875 + D3877 + D4809 + D7543
                    DebugInfo("Reset solver to XA")
                    'RA.Solver.SolverLibrary = raSolverLibrary.raXA ' D3623 + D3628
                    'RA_Solver = RA.Solver.SolverLibrary     ' D3630 -D3876
                    RA_Solver = raSolverLibrary.raXA    ' D3877
                    App.ActiveProject.ProjectManager.Parameters.Save()    ' D3877
                End If
                ' D7543 ===
                If Not App.isXAAvailable AndAlso RA_Solver = raSolverLibrary.raXA AndAlso App.isBaronAvailable Then
                    DebugInfo("Reset solver to Baron")
                    RA_Solver = raSolverLibrary.raBaron
                    App.ActiveProject.ProjectManager.Parameters.Save()
                End If
                ' D7543 ==
                If OPT_HIDE_CALCULATE_SOFT_CONSTRAINTS Then .Buttons(6).Visible = False
            End With
            RadToolBarMain.FindItemByValue("delete_nonfunded").Enabled = Not App.IsActiveProjectReadOnly 'A1431
            RadToolBarMain.FindItemByValue("delete_nonfunded").Text = ResString("btnDeleteNonFunded") 'A1431
            RadToolBarMain.Items(BTN_SOLVER).Visible = _RA_SOLVER_OPTION_AVAILABLE AndAlso (If(App.isXAAvailable, 2, 0) + If(App.isGurobiAvailable, 1, 0) + If(App.isBaronAvailable, 1, 0)) > 1   ' D7603 // XA has settings, so show dlg even if it one one allowed
            RadToolBarMain.Items(BTN_SOLVER + 1).Visible = RadToolBarMain.Items(BTN_SOLVER).Visible ' D3879 - D3885
            'RadToolBarMain.Items(BTN_SEL_ALTS).Text = ResString("btnRASelAlts") ' D3777
            'RadToolBarMain.Items(BTN_SEL_ALTS).Visible = RA_OPT_USE_SELECTED_ALTERNATIVES AndAlso Scenario.AlternativesFull.Count > 0 ' D3777 + D3837 + D3885 + D3937 + D4138
            ' -D3347
            '' D3246 ===
            'If RA.Scenarios.GlobalSettings.isBigModel AndAlso Not IsCallback AndAlso Not IsPostBack Then
            'RadToolBarMain.Items(7).ToolTip = String.Format(ResString("msgRADisabledAutoSolve"), RAGlobalSettings.RA_BIG_MODEL_ALTS_COUNT)
            '    RadToolBarMain.Items(7).Enabled = False
            'End If
            '' D3246 ==
            RadToolBarMain.Items(BTN_TIMEPERIODS).Text = ResString("optTimePeriods") ' D3905
            RadToolBarMain.Items(BTN_TIMEPERIODS).Enabled = RadToolBarMain.Items(BTN_SOLVE).Visible AndAlso Scenario.TimePeriods.Periods.Count > 0  ' D3905 + D3932 + D3943
            RadToolBarMain.Items(BTN_TIMEPERIODS).Visible = OPT_SHOW_TIMEPERIODS_GRID   ' D7079
            CType(RadToolBarMain.Items(BTN_TIMEPERIODS), RadToolBarButton).Checked = RA_ShowTimeperiods ' D3932
            RadToolBarMain.Items(BTN_TIMEPERIODS_SETTINGS).Enabled = RadToolBarMain.Items(BTN_TIMEPERIODS).Enabled AndAlso RA_ShowTimeperiods   ' D3932
            RadToolBarMain.Items(BTN_TIMEPERIODS_SETTINGS).Visible = RadToolBarMain.Items(BTN_TIMEPERIODS).Visible  ' D3943
            RadToolBarMain.Items(BTN_TIMEPERIODS_SETTINGS + 1).Visible = RadToolBarMain.Items(BTN_TIMEPERIODS).Visible  ' D3943

            ' D4609 ===
            With CType(RadToolBarMain.Items(BTN_MORE), RadToolBarDropDown)
                .Text = ResString("lblRAMore")
                '.Buttons(0).Enabled = Scenario.Alternatives.Count > 0
                .Buttons(0).Text = ResString("lblRAActiveAlts") ' D3777
                .Buttons(0).Visible = RA_OPT_USE_SELECTED_ALTERNATIVES AndAlso Scenario.AlternativesFull.Count > 0 ' D3777 + D3837 + D3885 + D3937 + D4138
                .Buttons(1).Text = ResString("btnRASyncCC")
                .Buttons(2).Text = ResString("lblRADeleteUnfunded")
                .Buttons(3).Text = ResString("lblRAAllocComparision") + "&#133;"   ' D4608
                '.Buttons(3).Visible = ShowDraftPages()  ' D4516
                .Buttons(4).Text = ResString("lblRAUnsolve")    ' D4681
            End With
            ' D4609 ==

            ' D3189 ===
            With CType(RadToolBarMain.Items(BTN_DOWNLOAD), RadToolBarDropDown)
                .Text = ResString("lblRADownload")
                .Buttons(0).Text = ResString("lblRADownloadECD")
                ' D3193 ===
                .Buttons(1).Text = ResString("lblRAExportXLS")
                .Buttons(1).Enabled = Scenario.Alternatives.Count > 0
                .Buttons(1).Visible = App.isExportAvailable
                ' -D4141
                '.Buttons(2).Text = ResString("lblRAExportOML")
                '.Buttons(2).Enabled = .Buttons(1).Enabled
                '.Buttons(2).Visible = .Buttons(1).Visible
                '.Buttons(3).Text = ResString("lblRAExportMPS")
                '.Buttons(3).Enabled = .Buttons(1).Enabled
                '.Buttons(3).Visible = .Buttons(1).Visible
                .Buttons(4).Visible = ShowDraftPages()  ' D3667
                ' D3193 ==
                ' -D4141
                .Buttons(4).Text = ResString("lblRAExportXALogs")   ' D3488
                .Buttons(4).Enabled = .Buttons(1).Enabled   ' D3488
                .Buttons(4).Visible = .Buttons(1).Visible   ' D3488 + D3667 + D4917
                .Buttons(5).Text = ResString("lblRAViewLogs")   ' D3488
                .Buttons(5).Visible = ShowDraftPages()  ' D4582
            End With
            'RadToolBarMain.Items(11).Text = ResString("lblRASolver") + ":"
            ' D3189 ==
            ' D3120 ==
            RadToolBarMain.Items(BTN_HELP).Text = ResString("btnRAHelp") ' D3428
            'ClientScript.RegisterStartupScript(GetType(String), "init", "InitPage();", True)    ' D2849 + D2855 + D2874
            ' D3661 ===
            If Not ShowDraftPages() AndAlso isDraftPage(_PGID_RA_TIMEPERIODS_SETTINGS) Then
                Scenario.Settings.TimePeriods = False
            End If
            ' D3661 ==
        End If
        ' D4909 ===
        If Scenario.IsInfeasibilityAnalysis Then
            ' D4929 ===
            'RadToolBarMain.Enabled = False
            For Each TBtn As RadToolBarItem In RadToolBarMain.Items
                Dim sName As String = If(TypeOf TBtn Is RadToolBarDropDown, TBtn.Text, TBtn.Value)
                Select Case sName.ToLower
                    Case "solve", "timeperiods"
                    Case Else
                        TBtn.Enabled = False
                End Select
            Next
            ' D4929 ==
        End If
        ' D4909 ==
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower 'Anti-XSS
        Select Case sAction
            Case "scenario"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    DebugInfo("Change scenario")
                    RA.Scenarios.ActiveScenarioID = ID
                    '' D3369 ===
                    'RA.Scenarios.CheckAlternatives()
                    'RA.Scenarios.UpdateScenarioBenefits()
                    'RA.Scenarios.UpdateSortOrder()
                    '' D3369 ==
                    RA.Solver.ResetSolver()
                    If (RA_AutoSolve OrElse RA.Scenarios.ActiveScenario.IsInfeasibilityAnalysis) AndAlso Scenario.Alternatives.Count > 0 Then Solve()  ' D6475
                    If RA_ShowTimeperiods AndAlso Scenario.TimePeriods.Periods.Count = 0 Then RA_ShowTimeperiods = False ' D3974
                End If
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
        End Select
        ' D2876 ==
    End Sub

    ' D3161 ===
    Public Function BenefitName() As String
        Return If(Scenario.Settings.Risks AndAlso HasRisks(), ResString("lblExpectedBenefit"), OPT_COLS_DEFNAMES(raColumnID.Benefit)) ' D3174
    End Function

    Public Function HasRisks() As Boolean
        Return Scenario.Alternatives.Sum(Function(a) a.RiskOriginal) > 0
    End Function
    ' D3161 ==

    ' D2874 ===
    Public Sub OnLoadSolveButton(sender As Object, e As EventArgs)
        If Not IsPostBack AndAlso Not isCallback Then CType(sender, RadToolBarButton).Attributes.Add("style", "text-align:center;")
    End Sub
    ' D2874 ==

    Private Sub Solve()
        If Scenario.Alternatives.Count > 0 Then  ' D2854
            DebugInfo("Start solving")
            isSolving = True    ' D2840
            'If Scenario.Settings.UseBaseCase AndAlso Scenario.Settings.BaseCaseForGroups Then BaseCaseMax = RA.Solver.GetBaseCaseMaximum Else BaseCaseMax = -1 ' D3090
            ' D3894 ===
            Dim Balance As Double = 0
            Dim HadMachines As Boolean = True
            If RA_Solver = raSolverLibrary.raGurobi AndAlso RA.Solver.OPT_GUROBI_USE_CLOUD Then ' D3924
                If GurobiLicense IsNot Nothing AndAlso GurobiLicense.id <> "" Then
                    If SessVar(SESS_GUROBI_BALANCE) IsNot Nothing Then Balance = CDbl(SessVar(SESS_GUROBI_BALANCE)) ' D4068
                    If Balance <= 0 Then Balance = GurobiLicense.credit ' D4068
                End If
                If GurobiMachines Is Nothing OrElse GurobiMachines.Count = 0 Then HadMachines = False
            End If
            ' D3894 ==
            Dim Dt As DateTime = Now
            RA.Solver.Solve(raSolverExport.raNone) ' D4526
            SolveTime = Now.Subtract(Dt)
            DebugInfo(String.Format("Solved in {0} ms.", SolveTime.TotalMilliseconds))
            If RA_Solver = raSolverLibrary.raGurobi AndAlso RA.Solver.OPT_GUROBI_USE_CLOUD Then ' D3924
                If GurobiLicense IsNot Nothing AndAlso GurobiLicense.id <> "" Then
                    Dim Spent As Double = GurobiLicense.credit - Balance
                    SessVar(SESS_GUROBI_BALANCE) = CStr(GurobiLicense.credit)   ' D4068
                    _GurobiMachines = Nothing
                    Dim sResult As String = String.Format("Solved in {0}; Balance: ${1} (-${2}); Started: {3}; Solve status: '{4}'", GetSolveTime(True), CostString(GurobiLicense.credit, Cents), Spent.ToString("F2"), Bool2YesNo(Not HadMachines AndAlso GurobiMachines IsNot Nothing AndAlso GurobiMachines.Count > 0), RA.Solver.SolverState.ToString.Substring(2))
                    App.DBSaveLog(dbActionType.actRASolveModel, dbObjectType.einfProject, App.ProjectID, String.Format("Use Solver {0} (Cloud), Scenario '{1}'", ResString("lblRASolverGurobi"), ShortString(Scenario.Name, 30, True)), sResult)
                End If
                ' D7510 ===
            Else
                Dim MID As String = SessVar(SESS_RA_SOLVED_MODELID)
                Dim sSolver = String.Format("{0}-{1}", App.ProjectID, CInt(RA_Solver))
                If String.IsNullOrEmpty(MID) OrElse MID <> sSolver Then
                    App.DBSaveLog(dbActionType.actRASolveModel, dbObjectType.einfProject, App.ProjectID, String.Format("Solve model with {0}", ResString("lbl_" + RA_Solver.ToString)), "")
                    SessVar(SESS_RA_SOLVED_MODELID) = sSolver
                End If
                ' D7510 ==
            End If

            If Scenario.Settings.UseBaseCase AndAlso Scenario.Settings.BaseCaseForGroups Then BaseCaseMax = RA.Solver.BaseCaseMaximum Else BaseCaseMax = -1 ' D3090

            UpdateAlternativesFundedAttribute() 'A1114
            DebugInfo("Solving finished")
        End If
    End Sub

    'A1114 ===
    Private Sub UpdateAlternativesFundedAttribute() 'A1114
        Dim tChanged As Boolean = False
        For Each ra_alt As RAAlternative In Scenario.Alternatives
            Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(New Guid(ra_alt.ID))
            If tAlt IsNot Nothing Then
                Dim curFundedValue As Double = ra_alt.DisplayFunded
                Dim attrFundedValue As Double = CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID, tAlt.NodeGuidID))

                If RA.Solver.SolverState <> raSolverState.raSolved Then curFundedValue = UNDEFINED_INTEGER_VALUE

                If Math.Abs(curFundedValue - attrFundedValue) > 0.0001 Then
                    PM.Attributes.SetAttributeValue(ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, ra_alt.Funded, tAlt.NodeGuidID, Guid.Empty)
                    tChanged = True
                End If
            End If
        Next
        If tChanged Then
            WriteAttributeValues(App.ActiveProject, "", "")
        End If
    End Sub
    'A1114 ==

    ' D2840 ===
    Public Function GetFundedInfo() As String
        Dim sRes As String = ""

        Dim isSolved As Boolean = RA.Solver.SolverState = raSolverState.raSolved    ' D3061
        Dim sNoData As String = "&nbsp;"

        'If Not Scenario.Settings.UseBaseCase OrElse BaseCaseMax < 0 Then BaseCaseMax = RA.Solver.TotalBenefitOriginal ' D3090 + A0918 + A0922
        BaseCaseMax = RA.Solver.BaseCaseMaximum

        BaseCaseMax = RA.Solver.BaseCaseMaximum
        Dim sPrcBenefit As Double = 0
        If isSolved AndAlso BaseCaseMax > 0 Then sPrcBenefit = (100 * RA.Solver.FundedBenefit / BaseCaseMax) ' D2839 + D2932 + D3077 + D3090 + A0917 + A0918

        'Dim sBudget As String = String.Format("<input type='text' id='tbBudgetLimit' tabindex='10' style='width:75px; padding:0px; margin:0px; height:20px; text-align:center;' value='{0}' onkeyup='onKeyUp(this.id, 0);' onblur='onBlur(this.id, ""budget"", """", 0);' onfocus='onFocus(this.id, this.value);' {1}/>", Scenario.Budget.ToString("F2"), IIf(Scenario.Alternatives.Count = 0, " disabled=1", ""))
        Dim sBudget As String = If(isExport, CostString(Scenario.Budget, Cents), String.Format("<input type='text' id='tbBudgetLimit' tabindex='10' style='width:{2}px; padding:0px 8px 0px 0px; margin:0px; height:20px; text-align:center;' class='input_cmenu' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value='{0}' onkeyup='onKeyUp(this.id, 0);' onblur='onBlur(this.id, ""budget"", """", 0);' onfocus='onFocus(this.id, this.value);' {1}/>", If(Scenario.Budget <> Double.MinValue AndAlso Not Double.IsNaN(Scenario.Budget) AndAlso Scenario.Budget <> Double.NegativeInfinity, CostString(Scenario.Budget, Cents), ""), IIf(Scenario.Alternatives.Count = 0, " disabled=1", ""), IIf(CostString(Scenario.Budget, Cents).Length > 12, 140, 110))) 'A0907 + D3189 + D3199 + D3245 + D3251 + D3302 + D4499

        ' D4960 ===
        Dim sSolverPrty As String = ""
        If RA_OPT_USE_SOLVER_PRIORITIES AndAlso Not Scenario.IsInfeasibilityAnalysis AndAlso (RA_Solver = raSolverLibrary.raGurobi OrElse Not RA_OPT_SOLVER_PRIORITIES_GUROBI_ONLY) AndAlso Not isExport Then    ' D4725 + A1580 + D4909 + D4917
            sSolverPrty = String.Format("<a href='' onclick='onShowSolverPrty(); return false;' class='text small actions' style='padding:1px 3px; background:#e5e5e5; border:1px solid #e0e0e0;'><nobr><img src='../../Images/ra/conf_9.png' width=10 height=9 title='' border=0 style='margin-right:3px'><span style='border-bottom:0px dashed'>{1}</span></nobr></a>", ImagePath, ResString("btnRASolverPriorities"))
        End If
        ' D4960 ==

        sRes = CStr(String.Format("<table border='0' cellspacing='0' cellpadding='3' style='background:#ffffff; border:3px double #f0f0f0;'><tr align='center' valign=middle>" + vbCrLf +
                             "<td class='text td_block' style='margin-right:4px;'><div class='block_name small'>{0}</div>{1}</td>" + vbCrLf +
                             If(isExport, "", "<td class='text small' rowspan='2'>&nbsp;</td>") +
                             "<td class='text td_block'><div class='block_name small'>{2}</div><div class='block_value'>{3}</div></td>" + vbCrLf +
                             "<td rowspan='2' class='text td_block' align=right valign=top><div class='text small' style='height:1.5em; margin:1px -1px 0.5em 0px'>{15}</div><div class='block_name small' style='text-align:right; padding-right:25px;'>{4}</div><div>=&nbsp;<span class='block_value' style='width:70px;'><b>{5}</b></span></div>{10}<div class='block_name small' style='text-align:right; margin-bottom:1ex;'>{13}</div></td>" + vbCrLf +
                             "</tr><tr align='center' valign=middle>" + vbCrLf +
                             "<td class='text td_block' style='border-top:4px solid #ffffff;'><div class='block_name small'>{6}</div><div class='block_value'>{7}</div>{11}</td>" + vbCrLf +
                             "<td class='text td_block'><div class='block_name small' style='border-top:1px solid #333333'>{8}</div><div class='block_value'>{9}</div></td>" + vbCrLf +
                             "</tr></table><input type='hidden' name='TotalCost' value='{12}'/><input type='hidden' name='solverState' value='{14}'>",
                             ResString("lblBudgetLimit"), sBudget,
                             BenefitName, If(isSolved, RA.Solver.FundedBenefit.ToString("F4"), sNoData),
                             "%", If(isSolved, sPrcBenefit.ToString("F2"), sNoData),
                             ResString("lblFundedCost"), If(isSolved, CostString(RA.Solver.FundedCost, Cents), sNoData),
                             ResString("lblBaseCaseMax"), If(isSolved, BaseCaseMax.ToString("F4"), sNoData),
                             If(isSolved, AddBar("", sPrcBenefit / 100, True, 56), ""),
                             If(isSolved AndAlso RA.Solver.BudgetLimit > 0, AddBar("", RA.Solver.FundedCost / RA.Solver.BudgetLimit, True, 100), ""),
                             CostString(RA.Solver.TotalCost, Cents), ResString("lblRAEffectiveness"), CInt(RA.Solver.SolverState), sSolverPrty))    ' D2941 + D3061 + D3077 + D3120 + A0907 + D3161 + A0917 + A0918 + D3199 + D3245 + D3316 + D3967 + D4360

        Return sRes
    End Function
    ' D2840 ==

    ' D3540 ===
    Public Function GetConstraintsInfo() As String
        Dim sRes As String = ""
        Dim tAll As Integer = Scenario.Constraints.Constraints.Count
        ' D3542 ===
        Dim tDis As Integer = 0
        Dim fIgnoredAllCC As Boolean = Not Scenario.Settings.CustomConstraints
        If tAll = 0 Then
            sRes = ResString("lblRANoCC")
        Else
            If Not Scenario.Settings.CustomConstraints Then
                tDis = Scenario.Constraints.Constraints.Count
            Else
                For Each tConstr As KeyValuePair(Of Integer, RAConstraint) In Scenario.Constraints.Constraints
                    If Not tConstr.Value.Enabled Then tDis += 1
                Next
            End If
            sRes = String.Format(ResString("lblRACCInfo"), tAll, tDis) + "<br><span class='small'><br><i>"
            If fIgnoredAllCC OrElse tDis = tAll Then sRes += ResString("lblRACCIgnoreAll") Else sRes += ResString(If(tDis = 0, "lblRACCIgnoreNo", "lblRAIgnoreSome"))
            sRes += "</i></span>"
        End If
        Dim sAll As String = (tAll - tDis).ToString
        Dim sDis As String = tDis.ToString
        If fIgnoredAllCC Then
            sAll = "<span style='color:#cc3333'>" + sAll + "</span>"
            If tDis > 0 Then sDis = "<span style='color:#cc8080'>" + sDis + "</span>"
        Else
            If tDis > 0 Then sDis = "<span style='color:#999999'>" + sDis + "</span>"
        End If
        If tDis = tAll AndAlso tAll > 0 Then sAll = ""
        If tDis > 0 AndAlso tDis <> tAll Then sDis = "+" + sDis
        If tDis = 0 Then sDis = ""
        Return String.Format("<span class='small' style='cursor:help; background:#fff5e0; border:1px solid #d0e0ff; padding:0px 2px; 1px 2px;' title=""{2}"">{0}{1}</span>", sAll, sDis, sRes.Replace("'", "&#39;"))
        ' D3542 ==
    End Function

    Public Function GetRALink(PgID As Integer) As String
        If isSLTheme() Then
            Return String.Format("' onclick='_pageNavigate({0}, ""{1}""); return false;", PgID, JS_SafeString(PageURL(PgID, GetTempThemeURI(True))))    ' D3851
        Else
            Return PageURL(PgID)
        End If
    End Function
    ' D3540 ==

    ' D3061 ===
    Public Function GetSolverMessage(Optional isDialog As Boolean = False) As String    ' D7052
        ' D3569 + D4521 ===
        Const _TPL_EXTRA As String = "%%extra%%"
        Dim sExtra As String = ""
        Dim sMsg As String = ""
        If Scenario.Alternatives.Count > 0 Then sMsg = SolverStateHTML(RA.Solver, _TPL_EXTRA) ' D3628 + D3818
        ' D6464 ===
        If RA.Solver.SolverState = raSolverState.raInfeasible AndAlso sMsg <> "" AndAlso Not Scenario.IsInfeasibilityAnalysis Then    ' D4909 + D4917 + D6241
            'If RA_Solver <> raSolverLibrary.raGurobi AndAlso App.isGurobiAvailable Then RA_Solver = raSolverLibrary.raGurobi   ' -D7081
            If RA_Solver = raSolverLibrary.raGurobi Then
                ' D6464 ==
                If Not isDialog Then sExtra = String.Format("<input type='button' class='button' id='btnInfeas' style='margin-left:2em; width:180px;' value='{0}' onclick='onCheckInfeas();'>", SafeFormString(ResString("lblRACheckInfeas")))  ' D7052
            End If
        End If
        ' D4521 ==
        'If Scenario.Settings.FundingPools AndAlso Scenario.FundingPools.Pools.Count > 0 Then
        ' D3880 ===
        If RA_ShowFPWarning Then
            Dim sRes As String = ""
            If IgnoreFP() AndAlso Scenario.Settings.FundingPools Then sRes = "confRAFPSwitchSolver"
            If IgnoreTP() AndAlso Scenario.Settings.TimePeriods Then sRes = CStr(IIf(sRes <> "", "confRAFPTPSwitchSolver", "confRATPSwitchSolver"))
            If sRes <> "" Then sMsg = String.Format("<div id='divFPWarning' style='text-align:center; padding:10px 0px 6px 0px;'><span class='warning' style='padding:4px 1ex;'>{0} <input type='button' class='button' style='width:40px; text-align:center' value='{1}' onclick='onSetSolver({2}, 1); theForm.cbSolver[0].checked=1; hideFPWarning();'>&nbsp;<input type='button' class='button' style='width:40px; text-align:center' value='{3}' onclick='noFPWarning();'></span></div>", String.Format(ResString(sRes), ResString("lblRASolverXA")), ResString("btnYes"), CInt(raSolverLibrary.raXA), ResString("btnNo")) + sMsg
        End If
        ' D3880 ==
        sMsg = sMsg.Replace(_TPL_EXTRA, sExtra)  ' D4521
        Return sMsg  'A0890
        ' D3569 ==
    End Function
    ' D3061 ==

    ' D3875 ===
    Public Function GetSolveTime(Optional fIsPlain As Boolean = False) As String    ' D3894
        Dim sVal As String = If(fIsPlain, "", "&nbsp;")    ' D4068
        If (RA.Solver.SolverState = raSolverState.raSolved OrElse fIsPlain) AndAlso SolveTime.TotalMilliseconds > 1 Then    ' D4068
            If SolveTime.TotalSeconds < 1 Then
                sVal = String.Format("{0} ms.", SolveTime.TotalMilliseconds.ToString("F0"))
            Else
                sVal = String.Format("{0} s.", SolveTime.TotalSeconds.ToString("F2"))
            End If
            sVal = ResString("lblRASolvedFor") + " " + sVal ' D3876
        End If
        If Not fIsPlain Then sVal = String.Format("<div title='Page processing time: {0} sec'>{1}</div>", Now.Subtract(StartPageDT).TotalSeconds.ToString("F2"), sVal) ' D3880 + D3894
        Return sVal
    End Function
    ' D3875 ==

    '' D4120 ===
    'Public Function GetPagesList() As String
    '    Dim sList = ""
    '    If RA_Pages_Mode <> ecRAGridPages.NoPages Then  ' D4122
    '        Dim tAll As Integer = 0 ' D4121
    '        If GridAlternatives.DataSource IsNot Nothing Then tAll = CType(GridAlternatives.DataSource, List(Of RAAlternative)).Count ' D4121
    '        Dim tPGSize As Integer = CInt(RA_Pages_Mode)    ' D4122
    '        Dim tCurPg As Integer = RA_Pages_CurPage        ' D4122
    '        If tAll > tPGSize AndAlso tPGSize > 0 Then
    '            Dim tPgMax As Integer = Math.Ceiling(tAll / tPGSize)
    '            For i As Integer = 0 To tPgMax - 1
    '                Dim isActive As Boolean = i = tCurPg
    '                If tPgMax > OPT_PAGES_SHOW_MAX Then
    '                    sList += String.Format("<option value='{0}'{1}>{2}</option>", i, IIf(isActive, " selected", ""), i + 1)
    '                Else
    '                    Dim sPg As String = (i + 1).ToString
    '                    If isActive Then sPg = "<span style='color:#000000'><b>" + sPg + "</b></span>" Else sPg = String.Format("<a href='' onclick='setPage({0}); return false;' class='action'>{1}</a>", i, sPg)
    '                    sList += String.Format("{0}{1}", IIf(sList = "", "", " &middot; "), sPg)
    '                End If
    '            Next
    '            If sList <> "" Then
    '                If tPgMax > OPT_PAGES_SHOW_MAX Then
    '                    sList = String.Format("<select id='pgSelect' onchange='setPage(this.value);'>{0}</select>", sList)
    '                End If
    '                sList = String.Format("<div style='text-align:center; padding-top:4px;' class='text'><b>{1}</b>: <nobr><span class='gray'>{0}</span></nobr></div>", sList, ResString("lblPage"))
    '            End If
    '        End If
    '    End If
    '    Return sList
    'End Function
    '' D4120 ==

    ' D4121 ===
    Public Function GetPageModes() As String
        Dim sList As String = ""
        Dim TRealVal As Integer = Math.Abs(App.ActiveProject.ProjectManager.Parameters.Parameters.GetParameterValue(OPT_RA_PAGES_MODE, RA_Pages_Mode))  ' D4480
        For Each t As ecRAGridPages In [Enum].GetValues(GetType(ecRAGridPages))
            If t <> ecRAGridPages.NoPages Then
                sList += String.Format("<option value='{0}'{1}>{2}</option>", CInt(t), If(TRealVal = CInt(t), " selected", ""), CInt(t))   ' D4480
            End If
        Next
        Return sList
    End Function
    ' D4121 ==

    ' D2875 ===
    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tID As Integer In RA.Scenarios.Scenarios.Keys
            sRes += If(sRes = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4}]", tID, JS_SafeString(RA.Scenarios.Scenarios(tID).Name), JS_SafeString(RA.Scenarios.Scenarios(tID).Description), RA.Scenarios.Scenarios(tID).Index, RA.Scenarios.Scenarios(tID).CombinedGroupUserID)   ' D3350 + D3737
        Next
        Return sRes
    End Function
    ' D2875 ==

    ' D4360 ===
    Public Function GetSolverPriorities() As String
        Dim sRes As String = ""
        If RA_OPT_USE_SOLVER_PRIORITIES AndAlso Scenario.SolverPriorities.Priorities IsNot Nothing Then
            RA_FUNDED_COST_NAME = String.Format("[ {0} ]", ResString("lblFundedCost"))
            Scenario.SolverPriorities.CheckAndSort()
            For Each tPrty As RASolverPriority In Scenario.SolverPriorities.Priorities
                ' D4362 ===
                Dim tCCEnabled As Boolean = tPrty.ConstraintID = RA_FUNDED_COST_ID
                If Not tCCEnabled Then
                    Dim tCC As RAConstraint = Scenario.SolverPriorities.GetConstraintByID(tPrty)
                    If tCC IsNot Nothing Then tCCEnabled = tCC.Enabled
                End If
                sRes += String.Format("{0}[{1},{2},'{3}',{4},{5},{6}]", If(sRes = "", "", ","), tPrty.Rank, tPrty.ConstraintID, JS_SafeString(Scenario.SolverPriorities.GetSolverPriorityName(tPrty)), Bool2Num(tPrty.InUse), CInt(tPrty.Condition), Bool2Num(tCCEnabled))
                ' D4362 ==
            Next
        End If
        Return sRes
    End Function
    ' D4360 ==

    ' D3777 ===
    Public Function GetAllAlternatives() As String
        Dim sRes As String = ""
        Dim Alts As List(Of RAAlternative) = Scenario.AlternativesFull
        'Alts.Sort(New RAAlternatives_Comparer(Math.Abs(CInt(RA.Scenarios.GlobalSettings.SortBy)), CInt(RA.Scenarios.GlobalSettings.SortBy) < 0, RA))
        DoSort(Alts)    ' D4417
        For Each tAlt As RAAlternative In Alts
            sRes += If(sRes = "", "", ",") + String.Format("['{0}',{1},'{2}',{3}]", tAlt.ID, tAlt.SortOrder, JS_SafeString(SafeFormString(tAlt.Name)), Bool2Num(tAlt.Enabled))
        Next
        Return sRes
    End Function
    ' D3777 ==

    ' D3350 ===
    Public Function GetCombinedGroups() As String
        Dim sRes As String = ""
        For Each tGrp As clsCombinedGroup In RA.ProjectManager.CombinedGroups.GroupsList    ' D3743
            sRes += If(sRes = "", "", ",") + String.Format("[{0},'{1}']", tGrp.CombinedUserID, JS_SafeString(tGrp.Name))   ' D3743
        Next
        Return sRes
    End Function
    ' D3350 ==

    ' D3121 ===
    Public Function GetMasterProjects() As String
        Dim sLst As String = SessVar(SESS_MASTER_PRJ_LIST)  ' D3967
        If String.IsNullOrEmpty(sLst) Then  ' D3967
            Dim AllPrj As List(Of clsProject) = App.DBProjectsByWorkgroupID(App.ActiveWorkgroup.ID)
            For Each tPrj As clsProject In AllPrj
                If tPrj.ProjectStatus = ecProjectStatus.psMasterProject AndAlso Not tPrj.isMarkedAsDeleted Then
                    sLst += String.Format("{0}[{1},'{2}']", If(sLst = "", "", ","), tPrj.ID, JS_SafeString(ShortString(tPrj.ProjectName, 60, True)))
                End If
            Next
            SessVar(SESS_MASTER_PRJ_LIST) = sLst    ' D3967
        End If
        Return sLst
    End Function
    ' D3121 ==

    ' D3875 ===
    Public Function GetRA_Strategy(tSolver As raSolverLibrary) As String
        Dim sLst As String = ""
        Select Case tSolver
            Case raSolverLibrary.raXA
                For Each ID As Integer In RA.Solver.XA_STRATEGIES
                    sLst += String.Format("<option value='{0}'{1}>{2}</option>", ID, If(ID = RA.Solver.XAStrategy, " selected", ""), ID)   ' D3877
                Next
        End Select
        If sLst <> "" Then sLst = String.Format("<select id='xa_strategy' style='width:100%'>{0}</select>", sLst) ' D3786
        Return sLst
    End Function

    ' D3876 ===
    Public Function GetRA_XAVariationIdx() As Integer
        For ID As Integer = 0 To RA.Solver.XA_VARIATIONS.Count - 1
            If RA.Solver.XA_VARIATIONS(ID) = RA.Solver.XAVariation Then Return ID
        Next
        Return 0
    End Function
    ' D3876 ==

    Public Function GetRA_Variation(tSolver As raSolverLibrary) As String
        Dim sLst As String = ""
        Select Case tSolver
            Case raSolverLibrary.raXA
                For ID As Integer = 0 To RA.Solver.XA_VARIATIONS.Count - 1
                    sLst += String.Format("<option value='{0}'{1}>{2}</option>", ID, If(RA.Solver.XA_VARIATIONS(ID) = RA.Solver.XAVariation, " selected", ""), RA.Solver.XA_VARIATIONS(ID))    ' D3877
                Next
        End Select
        If sLst <> "" Then sLst = String.Format("<select id='xa_variation' style='width:100%'>{0}</select>", sLst) ' D3876
        Return sLst
    End Function
    ' D3875 ==

    ' D2893 ===
    Public Function GetColumnsList() As String
        Dim sCols As String = ""
        For Each ID As Integer In OPT_COLS_DEF
            ID = Math.Abs(ID)
            Dim sName As String = OPT_COLS_DEFNAMES(ID)
            If ID = CInt(raColumnID.LastFundedPeriod) Then sName = ResString("lblRALastFunded") ' D4803
            'If ID = COL_BENEFIT Then sName = String.Format("{0}&nbsp;/&nbsp;{1}", OPT_COLS_DEFNAMES(ID), OPT_EBENEFIT_NAME) ' D2941 + D3013 + D3161
            If ID = CInt(raColumnID.EBenefit) Then sName = ResString("lblExpectedBenefit") ' D3174
            If ID = CInt(raColumnID.ProbSuccess) Then sName = ResString("lblProbSuccess") ' D3174
            If ID = CInt(raColumnID.ProbFailure) Then sName = ResString("lblProbFailure") ' D4348
            Dim fChecked As Boolean = RA_Columns.Contains(ID)
            ' D4930 ===
            Dim fCanShowColumn As Boolean = True
            Select Case CType(ID, raColumnID)
                Case raColumnID.Groups, raColumnID.LastFundedPeriod
                    fCanShowColumn = RA_OPT_ALLOW_LASTFUNDEDPERIOD_GROUPS_COLUMNS
                Case raColumnID.isCostTolerance, raColumnID.CostTolerance
                    fCanShowColumn = RA_OPT_ALLOW_COST_TOLERANCE_COLUMNS
            End Select
            If fCanShowColumn Then ' D4808
                ' D4930 ==
                sCols += String.Format("<div><input type='checkbox' id='cbColumn{0}'{3}{4} onclick='onChangeColumn({2},this.checked);'><label for='cbColumn{0}'>{1}</label></div>" + vbCrLf, ID, sName, ID, IIf(fChecked, " checked", ""), IIf(ID = CInt(raColumnID.Name), " disabled", "")) ' D3781
            End If
        Next
        Return sCols
    End Function
    ' D2893 ==

    ' D2843 ===
    Private Sub GridColumnsTuning()
        'If Not RA_FixedColsWidth Then GridAlternatives.Width = WebControls.Unit.Percentage(100) Else GridAlternatives.Width = WebControls.Unit.Empty ' D2880
        For i As Integer = 0 To GridAlternatives.Columns.Count - 1
            Dim tCol As DataControlField = GridAlternatives.Columns(i)
            If RA_FixedColsWidth Then   ' D2880
                Dim tWidth As Integer = OPT_COL_WIDTH_CONSTR    ' D2886
                If i < CInt(raColumnID.CustomConstraintsStart) Then tWidth = OPT_COLS_WIDTH(i) ' D2886
                If tWidth > 0 Then    ' D3267
                    tCol.HeaderStyle.Width = tWidth
                    'tCol.ItemStyle.Width = tWidth
                End If
            End If
            If OPT_COLS_SHOW_SELECTED AndAlso i < CInt(raColumnID.CustomConstraintsStart) Then    ' D2893
                tCol.Visible = Array.IndexOf(RA_Columns, i) >= 0    ' D2893
                ' D3013 ===
                If RA_HideIgnored AndAlso tCol.Visible Then
                    Select Case CType(i, raColumnID)
                        Case raColumnID.ProbFailure, raColumnID.Risk, raColumnID.ProbSuccess   ' D4348
                            tCol.Visible = Scenario.Settings.Risks
                        Case raColumnID.Musts
                            tCol.Visible = Scenario.Settings.Musts
                        Case raColumnID.MustNot
                            tCol.Visible = Scenario.Settings.MustNots
                    End Select
                End If
                ' D3017 + D3174 + D4803 ===
                'If tCol.Visible AndAlso (i = CInt(raColumnID.ProbFailure) OrElse i = CInt(raColumnID.ProbSuccess) OrElse i = CInt(raColumnID.Risk)) Then   ' D3162 + D4348 + D4696
                '    If Not Scenario.Settings.Risks Then tCol.Visible = False Else If RA_HideNoRisks Then tCol.Visible = HasRisks() ' D3162
                'End If
                '' D3013 + D3017 + D3174 ==
                'If tCol.Visible AndAlso i = CInt(raColumnID.EBenefit) Then   ' D3176
                '    If Not Scenario.Settings.Risks OrElse Not HasRisks() Then tCol.Visible = False ' D3174 + D3176
                'End If
                If tCol.Visible Then
                    Select Case CType(i, raColumnID)
                        Case raColumnID.ProbFailure, raColumnID.ProbSuccess, raColumnID.Risk
                            If Not Scenario.Settings.Risks Then tCol.Visible = False Else If RA_HideNoRisks Then tCol.Visible = HasRisks() ' D3162
                        Case raColumnID.EBenefit
                            If Not Scenario.Settings.Risks OrElse Not HasRisks() Then tCol.Visible = False ' D3174 + D3176
                        Case raColumnID.LastFundedPeriod    ' D4803
                            tCol.Visible = RA_OPT_ALLOW_LASTFUNDEDPERIOD_GROUPS_COLUMNS AndAlso Scenario.TimePeriods.Periods.Count > 0 AndAlso Scenario.Settings.TimePeriods    ' D4803 + D4808
                            'tCol.Visible = RA_OPT_ALLOW_LASTFUNDEDPERIOD_GROUPS_COLUMNS AndAlso (isTimeperiodsVisible() AndAlso Scenario.Settings.TimePeriods AndAlso RA.Solver.SolverState = raSolverState.raSolved)    ' D4803 + D4808
                        Case raColumnID.Groups              ' D4803
                            tCol.Visible = RA_OPT_ALLOW_LASTFUNDEDPERIOD_GROUPS_COLUMNS AndAlso Scenario.Settings.Groups AndAlso Scenario.Groups.HasData(Scenario.Alternatives)     ' D4803 + D4808
                        Case raColumnID.CostTolerance, raColumnID.isCostTolerance   ' D4930
                            tCol.Visible = RA_OPT_ALLOW_COST_TOLERANCE_COLUMNS      ' D4930
                    End Select
                End If
                ' D4803 ==
            End If
        Next
    End Sub

    Protected Sub GridAlternatives_DataBound(sender As Object, e As EventArgs) Handles GridAlternatives.DataBound
        If Not _isAjax Then GridColumnsTuning() ' D3917
    End Sub
    ' D2843 ==

    Protected Sub GridAlternatives_Load(sender As Object, e As EventArgs) Handles GridAlternatives.PreRender
        If GridAlternatives.DataSource Is Nothing AndAlso Not _isAjax AndAlso (Not RA_ShowTimeperiods OrElse CheckVar("action", "") = "timeperiods") Then  ' D3917 + D3948
            'If GridAlternatives.DataSource Is Nothing AndAlso Not _isAjax Then  ' D3917 + D3948
            If Not isReset AndAlso (RA_AutoSolve OrElse isSolving) Then Solve() ' D2840 + D2843 + D2874 + D4681
            BindConstraintColumns() ' D2886
            BindAttributesColumns() ' A1010
            ' D4121 ===
            Dim tAltsList As New List(Of RAAlternative)
            If RA.Solver.SolverState = raSolverState.raSolved AndAlso RA_ShowFundedOnly Then
                tAltsList.Clear()
                For Each tAlt As RAAlternative In Scenario.Alternatives
                    If tAlt.DisplayFunded > 0 Then tAltsList.Add(tAlt)
                Next
            Else
                tAltsList = Scenario.Alternatives
            End If
            GridAlternatives.DataSource = tAltsList
            ' D4121 ==
            'If RA_AutoSolve OrElse isSolving OrElse isSorting Then GridAlternatives.DataSource.Sort(New RAAlternatives_Comparer(Math.Abs(CInt(RA.Scenarios.GlobalSettings.SortBy)), CInt(RA.Scenarios.GlobalSettings.SortBy) < 0, RA)) ' D2982 + D3185
            DoSort(CType(GridAlternatives.DataSource, List(Of RAAlternative))) ' D4417
            '' D4120 ===
            'If RA_Pages_Mode = ecRAGridPages.NoPages Then
            '    GridAlternatives.AllowPaging = False
            'Else
            '    GridAlternatives.AllowPaging = True
            '    Select Case RA_Pages_Mode
            '        Case ecRAGridPages.Page100
            '            GridAlternatives.PageSize = 100
            '        Case ecRAGridPages.Page50
            '            GridAlternatives.PageSize = 50
            '        Case ecRAGridPages.Page20
            '            GridAlternatives.PageSize = 20
            '        Case ecRAGridPages.Page15
            '            GridAlternatives.PageSize = 15
            '        Case Else
            '            GridAlternatives.PageSize = 10
            '    End Select
            '    If RA_Pages_CurPage >= 0 Then GridAlternatives.PageIndex = RA_Pages_CurPage
            'End If
            '' D4120 ==
            GridAlternatives.DataBind()

            ' D4417 ===
            Dim sList As String = ""
            For Each tAlt As RAAlternative In tAltsList
                sList += String.Format("{0}{1}", If(sList = "", "", ","), tAlt.ID)
            Next
            SessVar(SESS_RA_SORT_ORDER) = sList
            ' D4417 ==
        End If
    End Sub

    ' D3336 ===
    Private Function GetConstraintLinkedInfo(CC As RAConstraint) As String
        Dim tAttr As clsAttribute = Nothing
        Dim sAtrInfo As String = ""
        Dim fLinkOK As Boolean = False
        If Not CC.LinkedAttributeID.Equals(Guid.Empty) Then
            tAttr = CatAttributesList.Find(Function(x) x.ID = CC.LinkedAttributeID)
            If tAttr Is Nothing Then
                tAttr = NumAttributesList.Find(Function(x) x.ID = CC.LinkedAttributeID)
                If tAttr IsNot Nothing Then
                    sAtrInfo = String.Format(ResString("lblRALinkedNumAtr"), App.GetAttributeName(tAttr))
                    fLinkOK = True
                End If
            Else
                sAtrInfo = String.Format(ResString("lblRALinkedCatAtr"), App.GetAttributeName(tAttr))
                Dim sItems As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(tAttr.EnumID)
                If sItems IsNot Nothing Then
                    For Each tItem As clsAttributeEnumerationItem In sItems.Items
                        If tItem.ID.Equals(CC.LinkedEnumID) Then
                            sAtrInfo = String.Format(ResString("lblRALinkedCatEnum"), App.GetAttributeName(tAttr), tItem.Value)
                            fLinkOK = True  ' D3336
                        End If
                    Next
                End If
            End If
            If sAtrInfo <> "" AndAlso CC.IsReadOnly Then sAtrInfo = String.Format("{0} ({1})", sAtrInfo, ResString("lblRACCReadOnly"))
        End If
        If CC.IsLinked AndAlso Not fLinkOK Then sAtrInfo = Nothing
        Return sAtrInfo
    End Function
    ' D3336 ==

    ' D3905 + D3915 + D3917 ===
    Private Sub AddTimePeriodCells(ByRef sColumn As String, ColID As Integer, RowID As Integer, Data As List(Of String), isEnabled As Boolean)  ' D4020
        If sColumn IsNot Nothing AndAlso isTimeperiodsVisible() AndAlso Data.Count > 0 Then ' D4474
            Data.Insert(0, CStr(If(RowID <= 0, "Total", sColumn.Clone)))
            Dim sPeriods As String = ""
            Dim fVis As Boolean = (isTimeperiodsVisible() AndAlso RA_TimePeriodVisible(ColID))  ' D3932
            For Each sData As String In Data
                sPeriods += String.Format("<td class='ra_tp_main' " + If(RowID <= 0 OrElse sPeriods = "", "", "id='tp_{1}_{2}' ") + "style='{4}{5}{6}'><div style='width:{3}px;'><nobr>{0}</nobr></div></td>", sData, ColID, RowID, OPT_COL_WIDTH_TIMEPERIOD, If(fVis OrElse RowID <= 0 OrElse sPeriods = "", "", "display:none;"), If(sPeriods = "", "border-left:0px;", ""), If(Not isEnabled AndAlso RowID > 0, "color:#808080; font-style:italic;", ""))   ' D3960 + D4020
            Next
            Dim sExtra As String = ""
            If RowID <= 0 Then sExtra = String.Format("<tr class='grid_clear'><td class='text' style='border-bottom:1px solid #d0d0d0;' colspan='{1}'>{0}</td></tr>", sColumn, Data.Count)
            sColumn = String.Format("<table width='100%' border=0 cellspacing=0 cellpadding=2>{1}<tr class='grid_clear text{4}' style='{3}' align='center'" + CStr(If(RowID > 0, "", " id='tp_{2}_hdr'")) + ">{0}</tr></table>", sPeriods, sExtra, ColID, If(fVis OrElse RowID > 0, "", "display:none;"), If(RowID > 0, "", " small")) ' D3960
            Data.RemoveAt(0)
        End If
    End Sub
    ' D3905 + D3915 + D3917 ==

    ' D3960 ===
    Function GetAltPeriodsData(alt As RAAlternative, ConstraintIdx As Integer, ByRef tMax As Double) As List(Of Double)
        Dim PeriodsResult As New List(Of Double)
        Dim AltTPData As AlternativePeriodsData = Scenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
        Dim StartPeriod As Integer = AltTPData.MinPeriod   ' D3951
        If Scenario.TimePeriods.TimePeriodResults().ContainsKey(alt.ID.ToString) Then StartPeriod = Scenario.TimePeriods.TimePeriodResults(alt.ID.ToString) ' D3951
        If AltTPData IsNot Nothing Then
            Dim _idx As Integer = 0

            ' D3916 ===
            Dim tp_id As Integer = -1
            If ConstraintIdx >= 0 Then
                Dim CC As RAConstraint = Nothing
                If Scenario.Constraints.Constraints.ContainsKey(ConstraintIdx) Then CC = Scenario.Constraints.Constraints(ConstraintIdx) ' D3921 + D3960
                If CC IsNot Nothing Then
                    tp_id = CC.ID
                    tMax = CC.GetAlternativeValue(alt.ID)   ' D3917
                Else
                    tp_id = -2
                End If
            Else
                tMax = alt.Cost ' D3917
            End If
            ' D3916 ==

            Dim CC_GUID As Guid = Guid.Empty  ' D3918
            For Each tResKey As Guid In Scenario.TimePeriods.Resources.Keys  ' D3918
                If Scenario.TimePeriods.Resources(tResKey).ConstraintID = tp_id Then
                    CC_GUID = tResKey  ' D3916 + D3918
                    Exit For
                End If
            Next

            If Not CC_GUID.Equals(Guid.Empty) Then
                Dim per_id As Integer = 0
                For Each tPeriod As RATimePeriod In Scenario.TimePeriods.Periods
                    Dim tData As Double = UNDEFINED_INTEGER_VALUE   ' D3960
                    If _idx >= StartPeriod AndAlso per_id < AltTPData.Duration AndAlso AltTPData.ResourceData.ContainsKey(per_id) Then
                        Dim data As Dictionary(Of Guid, Double) = AltTPData.ResourceData(per_id)    ' D3918
                        If data.ContainsKey(CC_GUID) Then
                            tData = data(CC_GUID)
                            per_id += 1
                        End If
                    End If
                    PeriodsResult.Add(tData)
                    _idx += 1
                Next
            End If
        End If
        Return PeriodsResult
    End Function

    Sub AddTimeperiodsTotal(tCell As TableCell, idx As Integer, sSurplus As String, isEnabled As Boolean)  ' D4020
        If isTimeperiodsVisible() AndAlso OPT_SHOW_TP_CC Then
            Dim tTotalPeriods As New List(Of Double)
            Dim tFundedPeriods As New List(Of Double)

            Dim fShowCC As Boolean = (RA.Scenarios.GlobalSettings.ShowCustomConstraints AndAlso Scenario.Constraints.Constraints.Count > 0)    ' D4128

            For Each tPeriod As RATimePeriod In Scenario.TimePeriods.Periods
                tTotalPeriods.Add(0)
                tFundedPeriods.Add(0)
            Next

            For Each tAlt As RAAlternative In Scenario.Alternatives
                Dim tMax As Double = UNDEFINED_INTEGER_VALUE
                Dim tPeriods As List(Of Double) = GetAltPeriodsData(tAlt, idx, tMax)
                For _i As Integer = 0 To tPeriods.Count - 1
                    If tPeriods(_i) <> UNDEFINED_INTEGER_VALUE Then
                        tTotalPeriods(_i) += tPeriods(_i)
                        If tAlt.Funded > 0 Then tFundedPeriods(_i) += tPeriods(_i)
                    End If
                Next
            Next

            Dim CC_GUID As Guid = Guid.Empty
            For Each tResKey As Guid In Scenario.TimePeriods.Resources.Keys
                If Scenario.TimePeriods.Resources(tResKey).ConstraintID = idx Then
                    CC_GUID = tResKey
                    Exit For
                End If
            Next

            Dim Periods As New List(Of String)
            For _i = 0 To tTotalPeriods.Count - 1
                Dim sMin As String = "&nbsp;"
                Dim sMax As String = "&nbsp;"
                Dim tPeriod As RATimePeriod = Scenario.TimePeriods.Periods(_i)
                Dim tVal As Double = tPeriod.GetResourceMinValue(CC_GUID)
                If tVal <> UNDEFINED_INTEGER_VALUE Then sMin = CostString(tVal, Cents)
                tVal = tPeriod.GetResourceMaxValue(CC_GUID)
                If tVal <> UNDEFINED_INTEGER_VALUE Then sMax = CostString(tVal, Cents)
                Dim sTotal As String = CostString(tTotalPeriods(_i), Cents)
                Dim sFunded As String = CostString(tFundedPeriods(_i), Cents)
                Periods.Add(String.Format("<div class='as_number'>{0}{1}{2}{4}{3}</div>", IIf(fShowCC, sMin + "<br>", ""), IIf(fShowCC, sMax + "<br>", ""), If(RA.Solver.SolverState = raSolverState.raSolved, String.Format("<b>{0}</b><br>", sFunded), ""), sTotal, sSurplus))    ' D4004 + D4128 + D4800
            Next
            'If String.IsNullOrEmpty(tCell.ToolTip) Then tCell.ToolTip = SafeFormString(HTML2Text(tCell.Text))   ' D6628
            AddTimePeriodCells(tCell.Text, If(idx < -1, raColumnID.Cost, idx + CInt(raColumnID.CustomConstraintsStart)), 2, Periods, isEnabled) ' D4020

        End If
    End Sub
    ' D3960 ==

    ' D2886 ===
    Private Sub BindConstraintColumns()
        If GridAlternatives.Columns.Count > CInt(raColumnID.CustomConstraintsStart) Then
            For i As Integer = GridAlternatives.Columns.Count - 1 To CInt(raColumnID.CustomConstraintsStart) Step -1
                GridAlternatives.Columns.RemoveAt(i)
            Next
        End If
        LinkedCC.Value = "" ' D3379
        hasDisabledCCWithMax = False    ' D3540
        For Each tID As Integer In Scenario.Constraints.Constraints.Keys
            Dim tConstr As RAConstraint = Scenario.Constraints.Constraints(tID)
            Dim tfield As New TemplateField()
            tfield.HeaderText = SafeFormString(ShortString(tConstr.Name, 35, True)) 'A0930
            Dim sAtrInfo As String = GetConstraintLinkedInfo(tConstr)   ' D3336
            If Not isExport AndAlso Not String.IsNullOrEmpty(sAtrInfo) Then tfield.HeaderText += String.Format("<img src='../../Images/ra/{1}' width=9 height=9 title='{2}' border=0 style='margin-left:4px'>", ImagePath, If(tConstr.IsReadOnly, "link_sm2.png", "link_sm2_.png"), SafeFormString(sAtrInfo)) ' D3336
            tfield.HeaderStyle.BackColor = If(tConstr.Enabled, Color.FromArgb(1, 210, 230, 255), Color.FromArgb(1, 200, 200, 200)) ' D3539
            If Not tConstr.Enabled OrElse Not Scenario.Settings.CustomConstraints Then   ' D3683
                tfield.HeaderStyle.Font.Strikeout = True
            End If
            tfield.ItemStyle.HorizontalAlign = HorizontalAlign.Right
            tfield.ItemStyle.BorderColor = Color.FromArgb(128, 240, 240, 240)
            tfield.ItemStyle.BorderWidth = 1
            tfield.Visible = RA.Scenarios.GlobalSettings.ShowCustomConstraints AndAlso Not (RA_HideIgnored AndAlso Not Scenario.Settings.CustomConstraints)  ' D3013 + D3781
            GridAlternatives.Columns.Add(tfield)
            If tConstr.IsLinked Then LinkedCC.Value += String.Format("{0}[{1},'{2}','{3}',{4}]", If(LinkedCC.Value = "", "", ","), tConstr.ID, JS_SafeString(SafeFormString(tConstr.Name)), If(String.IsNullOrEmpty(sAtrInfo), "", JS_SafeString(SafeFormString(sAtrInfo))), If(tConstr.IsReadOnly, 1, 0)) ' D3349
            If RA_ShowSurplus AndAlso RA.Solver.SolverState = raSolverState.raSolved AndAlso Not hasDisabledCCWithMax AndAlso Not tConstr.Enabled AndAlso (tConstr.MaxValueSet OrElse tConstr.MinValueSet) Then hasDisabledCCWithMax = True ' D3540 + D3682
        Next
    End Sub
    ' D2886 ==

    ' A1010 ===
    Private Sub BindAttributesColumns()
        Dim tShowAltAttributes As Boolean = ShowAltAttributes

        For Each attr As clsAttribute In PM.Attributes.GetAlternativesAttributes(True)
            Dim tfield As New TemplateField()
            tfield.HeaderText = SafeFormString(ShortString(App.GetAttributeName(attr), 35, True))
            'tfield.HeaderStyle.BackColor = Drawing.Color.FromArgb(1, 130, 190, 190)
            tfield.HeaderStyle.BackColor = Color.FromArgb(255, 220, 255, 220)
            tfield.ItemStyle.HorizontalAlign = HorizontalAlign.Center
            tfield.ItemStyle.BorderColor = Color.FromArgb(128, 240, 240, 240)
            tfield.ItemStyle.BorderWidth = 1
            tfield.Visible = tShowAltAttributes
            GridAlternatives.Columns.Add(tfield)
        Next
    End Sub
    ' A1010 ==

    Public Function GetOptionValue(sID As String) As String
        Dim chk As Boolean = True
        With Scenario.Settings
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
                Case "cbOptTimePeriods" ' D3630
                    chk = .TimePeriods  ' D3630
                    ' D4485 ===
                Case "cbOptTimePeriodMins"
                    chk = .ResourcesMin
                Case "cbOptTimePeriodMaxs"
                    chk = .ResourcesMax
                    ' D4485 ==
                    ' D3078 ===
                Case "cbBaseCase"
                    chk = If(OPT_SHOW_AS_IGNORES, Not .UseBaseCase, .UseBaseCase)
                Case "cbBCGroups"
                    chk = If(OPT_SHOW_AS_IGNORES, Not .BaseCaseForGroups, .BaseCaseForGroups)
                    ' D3078 ==
            End Select
        End With
        If OPT_SHOW_AS_IGNORES Then chk = Not chk ' D2931
        If chk Then Return " checked" Else Return ""
    End Function

    ' D2931 ===
    Public Function GetOptionStyle(sID As String) As String
        Dim has As Boolean = False
        With Scenario
            Select Case sID
                Case "cbOptMusts"
                    has = .Alternatives.Sum(Function(a) If(a.Must, 1, 0)) > 0
                Case "cbOptMustNots"
                    has = .Alternatives.Sum(Function(a) If(a.MustNot, 1, 0)) > 0
                Case "cbOptConstraints"
                    has = .Constraints.Constraints.Count > 0
                Case "cbOptDependencies"
                    'has = .Dependencies.Dependencies.Count > 0
                    has = .Dependencies.HasData(.Alternatives, .Groups) ' D3881
                Case "cbOptGroups", "cbBCGroups"    ' D3078
                    'has = .Groups.Groups.Count > 0
                    has = .Groups.HasData(.Alternatives)    ' D3881
                Case "cbOptFundingPools"
                    has = .FundingPools.Pools.Count > 0
                Case "cbOptRisks"
                    has = HasRisks()  ' D3017 + D3161
                Case "cbOptTimePeriods" ' D3630
                    has = Scenario.TimePeriods.Periods.Count > 0      ' D3630 + D3812 + D3841 + D3943
            End Select
        End With
        Return If(has, "style='font-weight:bold;'", "style='color:#999'") 'A0905
    End Function
    ' D2931 ==

    Private Function AddBar(sText As String, tVal As Double, Optional fLight As Boolean = False, Optional fBarWidth As Integer = OPT_BAR_WIDTH) As String
        If isExport Then Return "" ' D3189
        Dim sVal As String = sText
        If RA_ShowTinyBars AndAlso Not isExport Then   ' D2843 + D2880 + 4727
            If tVal > 1 Then tVal = 1
            If tVal < 0 Then tVal = 0
            Dim L = Math.Floor(tVal * fBarWidth)
            Dim sImg As String = String.Format("<img src='{0}' width='{{0}}' height={1} title='' border=0>", BlankImage, OPT_BAR_HEIGHT)    ' D3006
            sVal += String.Format(If(sVal = "", "", "<br>") + If(L > 0, "<span style='display:inline-block; line-height:{2}px; height:{2}px; width:{0}px; border-bottom:{2}px solid {3}; margin-top:-{2}px;'>" + String.Format(sImg, L) + "</span>", "") + If(L < fBarWidth, "<span style='display:inline-block; line-height:{2}px; height:{2}px; width:{1}px; border-bottom:{2}px solid {4}; margin-top:-{2}px;'>" + String.Format(sImg, fBarWidth - L) + "</span>", ""), L, fBarWidth - L, OPT_BAR_HEIGHT, OPT_BAR_COLOR_FILLED, If(fLight, "#ffffff", OPT_BAR_COLOR_EMPTY))    ' D2843 + D3006
        End If
        Return sVal
    End Function

    ' D2982 ===
    Private Function GetSortCaption(idx As raColumnID, sName As String) As String   ' D3185
        If GridAlternatives.AllowSorting AndAlso Not isExport Then   ' D3189
            Dim sExtra As String = ""
            Dim isDesc As Boolean = False
            If Math.Abs(CInt(RA.Scenarios.GlobalSettings.SortBy)) = CInt(idx) Then
                If CInt(RA.Scenarios.GlobalSettings.SortBy) > 0 Then isDesc = True ' D3185
                sExtra = If(isDesc, _SORT_ASC, _SORT_DESC)
            End If
            ' D6464 ===
            Dim isPrty As Boolean = False
            If idx = raColumnID.Cost OrElse idx >= raColumnID.CustomConstraintsStart Then
                For Each tPrty As RASolverPriority In Scenario.SolverPriorities.Priorities
                    'Dim RAC As RAConstraint = Scenario.SolverPriorities.GetConstraintByID(tPrty)
                    If tPrty.InUse AndAlso ((tPrty.ConstraintID = RA_FUNDED_COST_ID AndAlso idx = raColumnID.Cost) OrElse (idx >= raColumnID.CustomConstraintsStart AndAlso idx - raColumnID.CustomConstraintsStart = tPrty.ConstraintID)) Then
                        isPrty = True
                        Exit For
                    End If
                Next
            End If
            Return String.Format("<a href='' onclick='DoSort({0},{1}); return false' class='actions'{4}>{2}{3}</a>", CInt(idx), If(isDesc, 1, 0), sName, sExtra, If(isPrty, " style='color:#009900' title='Using as Solver Priority'", ""))
            ' D6464 ==
        Else
            Return sName    ' D3189
        End If
    End Function
    ' D2982 ==

    'A0910 + A0919 ===
    Private Sub CopyPasteHeaderButtons(ByRef CellText As String, idx As Integer, HasCopyButton As Boolean, HasPasteButton As Boolean, SyncID As String, isDisabledCC As Boolean, Optional HasTimePeriods As Boolean = False)    ' D3185 + A0930 + A0937 + D3343 + D3539 + D3905
        If Not isExport AndAlso (HasPasteButton OrElse HasCopyButton) Then    ' D4909
            Dim sButtons As String = String.Format("<img src='../../Images/ra/menu_dots.png' width='12' height='12' style='cursor:context-menu; padding-left:3px;' id='mnu_img_{0}' alt='' onclick='showMenu(event, ""{0}"", {2}, {3}, ""{4}"", {5});' oncontextmenu='showMenu(event, ""{0}"", {2}, {3}, ""{4}"", {5}); return false;' title=''/>", CInt(idx).ToString, ImagePath, If(HasCopyButton, "true", "false"), If(HasPasteButton, "true", "false"), SyncID, If(isDisabledCC, 1, 0))
            If Scenario.IsInfeasibilityAnalysis Then sButtons = ""
            If HasTimePeriods Then sButtons = String.Format("<img src='../../Images/ra/{4}.png' width='12' height='12' style='cursor:hand; padding-left:3px;' id='cal_img_{0}' alt='' onclick='switchTP(""{0}"");' title='{3}'/>{2}", CInt(idx).ToString, ImagePath, sButtons, SafeFormString(ResString("optTimePeriods")), If(RA_TimePeriodVisible(idx), "calendar_12", "calendar_12_")) ' D3915 + D3917

            If sButtons <> "" Then
                CellText = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:100%'><tr valign='middle' class='grid_clear'>" +
                                          "<td class='text' align='center' width='99%'>{2}</td>" +
                                          "<td style='width:{3}px' align='right'><nobr>{1}</nobr></td>" +
                                          "</tr></table>", CInt(idx).ToString, sButtons, CellText, IIf(HasTimePeriods, 28, 15)) ' D3343 + D3539 + D3905
            End If
        End If
        ' D3162 ==
    End Sub
    'A0910 ==

    ' D4201 ===
    Private Sub CheckBoxHeaderButtons(ByRef CellText As String, idx As Integer)
        If Not isExport AndAlso Not Scenario.IsInfeasibilityAnalysis Then  ' D4909
            Dim sButtons As String = String.Format("<img src='../../Images/ra/menu_dots.png' width='12' height='12' style='cursor:context-menu; padding-left:3px;' id='mnu_img_{0}' alt='' onclick='showMenuCB(event, ""{0}"");' oncontextmenu='showMenuCB(event, ""{0}""); return false;' title=''/>", CInt(idx).ToString, ImagePath)
            'If HasTimePeriods Then sButtons = String.Format("<img src='{1}{4}.png' width='12' height='12' style='cursor:hand; padding-left:3px;' id='cal_img_{0}' alt='' onclick='switchTP(""{0}"");' title='{3}'/>{2}", CInt(idx).ToString, ImagePath, sButtons, SafeFormString(ResString("optTimePeriods")), IIf(RA_TimePeriodVisible(idx), "calendar_12", "calendar_12_")) ' D3915 + D3917

            CellText = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:100%'><tr valign='middle' class='grid_clear'><td class='text' align='center' width='99%'>{2}</td><td style='width:15px' align='right'><nobr>{1}</nobr></td></tr></table>", CInt(idx).ToString, sButtons, CellText)
        End If
    End Sub
    ' D4201 ==

    ' D4490 ===
    Private ReadOnly Property MaxCostLen As Integer
        Get
            If _MaxCostLen <= 0 Then
                Dim MaxCost As Double = Scenario.Alternatives.Max(Function(a) a.Cost)
                _MaxCostLen = CostString(MaxCost, Cents).Length + 1
                If _MaxCostLen < 8 Then _MaxCostLen = 8
                If _MaxCostLen > 16 Then _MaxCostLen = 16
                Dim W As Integer = CInt(Math.Round(_MaxCostLen * 6.62 + 6))
                If W < If(isTimeperiodsVisible(), 50, 65) Then W = If(isTimeperiodsVisible(), 50, 65)
                OPT_COLS_WIDTH(raColumnID.Cost) = W
                OPT_COL_WIDTH_TIMEPERIOD = CInt(If(W < 65, W, Math.Round(W * 0.97)))
            End If
            Return _MaxCostLen
        End Get
    End Property

    Public Property OPT_COLS_DEF As raColumnID()
        Get
            Return CType(_OPT_COLS_DEF, raColumnID())
        End Get
        Set(value As raColumnID())
            _OPT_COLS_DEF = value
        End Set
    End Property
    ' D4490 ==

    Protected Sub GridAlternatives_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles GridAlternatives.RowDataBound
        ' D2893 ===
        If e.Row.RowType = DataControlRowType.Header Then
            e.Row.Attributes.Add("id", "tr_header")   ' D4122
            ' D3915 ===
            Dim TimeperiodHeaders As New List(Of String)
            Dim _idx As Integer = 0
            For Each tPeriod As RATimePeriod In Scenario.TimePeriods.Periods
                Dim sName As String = Scenario.TimePeriods.GetPeriodName(_idx, False)
                Dim sShortName As String = Scenario.TimePeriods.GetPeriodName(_idx, True)
                TimeperiodHeaders.Add(String.Format("<div class='text-overflow' title='{0}'><nobr>{1}</nobr></div>", SafeFormString(sName), SafeFormString(sShortName)))
                _idx += 1
            Next
            ' D3915 ==

            For i_int As Integer = 0 To e.Row.Cells.Count - 1
                e.Row.Cells(i_int).Wrap = True
                Dim i As raColumnID = CType(i_int, raColumnID)
                If i < OPT_COLS_DEFNAMES.Count - 1 Then
                    'e.Row.Cells(i).Text = IIf(i = COL_BENEFIT, BenefitName, OPT_COLS_DEFNAMES(i))    ' D2941 + D3161
                    e.Row.Cells(i).Text = OPT_COLS_DEFNAMES(i)    ' D2941 + D3161 + D3174
                    If OPT_COLS_SORTING.Contains(i) Then e.Row.Cells(i).Text = GetSortCaption(i, e.Row.Cells(i).Text) ' D2982
                    ' D4348 ===
                    Dim sTooltip As String = ""
                    Select Case i
                        Case raColumnID.EBenefit
                            sTooltip = ResString("lblExpectedBenefit")
                        Case raColumnID.ProbSuccess ' D4353
                            sTooltip = ResString("lblProbSuccess")
                        Case raColumnID.ProbFailure ' D4353
                            sTooltip = ResString("lblProbFailure")
                        Case raColumnID.Risk    ' D4695
                            sTooltip = ResString("lblRARiskTooltip")    ' D4695
                    End Select
                    If sTooltip <> "" Then e.Row.Cells(i).ToolTip = sTooltip
                    ' D4348 ==
                Else
                    e.Row.Cells(i).Text = GetSortCaption(i, e.Row.Cells(i).Text) ' D2982
                End If
                'A0910 ===
                If i >= raColumnID.CustomConstraintsStart AndAlso i < raColumnID.CustomConstraintsStart + Scenario.Constraints.Constraints.Count Then 'A1010
                    Dim idx As Integer = i - CInt(raColumnID.CustomConstraintsStart)
                    With Scenario.Constraints
                        If idx < .Constraints.Keys.Count Then
                            Dim tID As Integer = .Constraints.Keys(idx)
                            If .Constraints.ContainsKey(tID) Then
                                CopyPasteHeaderButtons(e.Row.Cells(i_int).Text, i_int, True, Not .Constraints(tID).IsLinked OrElse Not .Constraints(tID).IsReadOnly, If(.Constraints(tID).IsLinked, .Constraints(tID).ID.ToString, ""), Not .Constraints(tID).Enabled, OPT_SHOW_TP_CC AndAlso isTimeperiodsVisible())    ' D3185 + A0919 + A0930 + D3255 + A0937 + D3350 + D3343 + D3346 + D3539 + D3905 + D3917
                                If OPT_SHOW_TP_CC Then
                                    If String.IsNullOrEmpty(e.Row.Cells(i_int).ToolTip) Then e.Row.Cells(i_int).ToolTip = SafeFormString(HTML2Text(e.Row.Cells(i_int).Text))   ' D6628
                                    AddTimePeriodCells(e.Row.Cells(i_int).Text, i_int, 0, TimeperiodHeaders, True) ' D3905 + D3915 + D3917 + D4020
                                End If
                            End If
                        End If
                    End With
                End If
                'A0910 ==
                'A1010 ===
                If i >= raColumnID.CustomConstraintsStart + Scenario.Constraints.Constraints.Count Then
                    ' attributes columns
                    Dim tAllAttributes = PM.Attributes.GetAlternativesAttributes(True)
                    Dim index = i - (raColumnID.CustomConstraintsStart + Scenario.Constraints.Constraints.Count)
                    If index < tAllAttributes.Count Then
                        CopyPasteHeaderButtons(e.Row.Cells(i_int).Text, i_int, True, False, "", False)   ' D3905
                    End If
                End If
                'A1010 ==
            Next
            e.Row.Cells(CInt(raColumnID.Name)).Attributes.Add("id", "td_hdr_name")  ' D4790
            'A0910 + D3185 + A0919 + D3343 + D3905 ===
            CopyPasteHeaderButtons(e.Row.Cells(CInt(raColumnID.Name)).Text, CInt(raColumnID.Name), True, False, "", False)
            CopyPasteHeaderButtons(e.Row.Cells(CInt(raColumnID.Benefit)).Text, CInt(raColumnID.Benefit), True, False, "", False)
            CopyPasteHeaderButtons(e.Row.Cells(CInt(raColumnID.EBenefit)).Text, CInt(raColumnID.EBenefit), True, False, "", False)    ' D3174
            CopyPasteHeaderButtons(e.Row.Cells(CInt(raColumnID.Cost)).Text, CInt(raColumnID.Cost), True, True, "", False, isTimeperiodsVisible)
            CopyPasteHeaderButtons(e.Row.Cells(CInt(raColumnID.ProbFailure)).Text, CInt(raColumnID.ProbFailure), True, True, "", False)
            CopyPasteHeaderButtons(e.Row.Cells(CInt(raColumnID.Risk)).Text, CInt(raColumnID.Risk), True, False, "", False)   ' D4348
            CopyPasteHeaderButtons(e.Row.Cells(CInt(raColumnID.ProbSuccess)).Text, CInt(raColumnID.ProbSuccess), True, True, "", False)  ' A0969
            'A0910 + D3185 + A0919 +  D3343 + D3905 ==
            CopyPasteHeaderButtons(e.Row.Cells(CInt(raColumnID.Groups)).Text, CInt(raColumnID.Groups), True, False, "", False)  ' D4810
            ' D4201 ===
            CheckBoxHeaderButtons(e.Row.Cells(CInt(raColumnID.isPartial)).Text, CInt(raColumnID.isPartial))
            CheckBoxHeaderButtons(e.Row.Cells(CInt(raColumnID.isCostTolerance)).Text, CInt(raColumnID.isCostTolerance)) ' D4930
            CheckBoxHeaderButtons(e.Row.Cells(CInt(raColumnID.Musts)).Text, CInt(raColumnID.Musts))
            CheckBoxHeaderButtons(e.Row.Cells(CInt(raColumnID.MustNot)).Text, CInt(raColumnID.MustNot))
            ' D4201 ==

            If isTimeperiodsVisible() Then
                If String.IsNullOrEmpty(e.Row.Cells(CInt(raColumnID.Cost)).ToolTip) Then e.Row.Cells(CInt(raColumnID.Cost)).ToolTip = SafeFormString(HTML2Text(e.Row.Cells(CInt(raColumnID.Cost)).Text))   ' D6628
                AddTimePeriodCells(e.Row.Cells(CInt(raColumnID.Cost)).Text, CInt(raColumnID.Cost), 0, TimeperiodHeaders, True) ' D3905 + D3915 + D3917 + D4020
            End If
        End If
        ' D2893 ==

        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim alt = CType(e.Row.DataItem, RAAlternative)

            ' D2843 ===
            'Dim fIsFunded As Boolean = alt.Funded > 0.0001 AndAlso RA.Solver.SolverState = raSolverState.raSolved AndAlso isSolving ' D3179
            Dim fIsFunded As Boolean = alt.DisplayFunded > 0.0 AndAlso RA.Solver.SolverState = raSolverState.raSolved ' D3179 + D3246 + A0939
            Dim fPF As Boolean = fIsFunded AndAlso (alt.IsPartiallyFunded OrElse alt.AllowCostTolerance AndAlso alt.FundedCost <> alt.Cost) ' D3546 + D4933
            If RA.Solver.SolverState = raSolverState.raSolved Then
                If Not fIsFunded AndAlso RA_ShowFundedOnly Then e.Row.Visible = False ' D2880
            End If
            ' D2843 ==

            Dim cbMusts As CheckBox = CType(e.Row.FindControl("cbMusts"), CheckBox) ' D3017
            Dim cbMustNot As CheckBox = CType(e.Row.FindControl("cbMustNot"), CheckBox)
            If cbMusts IsNot Nothing AndAlso cbMustNot IsNot Nothing Then
                cbMusts.InputAttributes.Add("data_id", String.Format("musts{0}", e.Row.RowIndex))   ' D4199
                cbMusts.InputAttributes.Add("data_guid", alt.ID.ToString)   ' D4199
                cbMusts.InputAttributes.Remove("onclick")
                cbMusts.InputAttributes.Add("onclick", String.Format("last_focus=this.id; return onMustsChanged('musts', '{0}', this.checked, '{1}', event, {2});", alt.ID.ToString, cbMustNot.ClientID, e.Row.RowIndex))   ' D3376 + D4199
                cbMusts.InputAttributes.Add("onfocus", "last_focus=this.id;")   ' D3376
                cbMustNot.InputAttributes.Add("data_id", String.Format("mustnot{0}", e.Row.RowIndex))   ' D4199
                cbMustNot.InputAttributes.Add("data_guid", alt.ID.ToString)   ' D4199
                cbMustNot.InputAttributes.Remove("onclick")
                cbMustNot.InputAttributes.Add("onclick", String.Format("last_focus=this.id; return onMustsChanged('mustnot', '{0}', this.checked, '{1}', event, {2});", alt.ID.ToString, cbMusts.ClientID, e.Row.RowIndex)) ' D3376 + D4199
                cbMustNot.InputAttributes.Add("onfocus", "last_focus=this.id;")   ' D3376
            End If

            ' D4930 ===
            Dim tbTolerance As TextBox = CType(e.Row.FindControl("tbTolerance"), TextBox) ' D2837
            Dim cbTolerance As CheckBox = CType(e.Row.FindControl("cbIsTolerance"), CheckBox)
            If cbTolerance IsNot Nothing AndAlso tbTolerance IsNot Nothing Then  ' D2837
                cbTolerance.InputAttributes.Add("data_id", String.Format("istolerance{0}", e.Row.RowIndex))   ' D4199
                cbTolerance.InputAttributes.Add("data_guid", alt.ID.ToString)   ' D4199
                cbTolerance.InputAttributes.Remove("onclick")
                cbTolerance.InputAttributes.Add("onclick", String.Format("last_focus=this.id; return onPartialChanged('istolerance', '{0}', '{1}', this.checked, event, {2});", alt.ID.ToString, tbTolerance.ClientID, e.Row.RowIndex))
                cbTolerance.InputAttributes.Add("onfocus", "last_focus=this.id;")   ' D3376
                tbTolerance.Attributes.Add("data_id", String.Format("istolerance_edit{0}", e.Row.RowIndex))   ' D4199
            End If
            ' D4930 ==

            Dim tbMinPrc As TextBox = CType(e.Row.FindControl("tbMinPrc"), TextBox) ' D2837
            Dim cbPartial As CheckBox = CType(e.Row.FindControl("cbpartial"), CheckBox)
            If cbPartial IsNot Nothing AndAlso tbMinPrc IsNot Nothing Then  ' D2837
                cbPartial.InputAttributes.Add("data_id", String.Format("partial{0}", e.Row.RowIndex))   ' D4199
                cbPartial.InputAttributes.Add("data_guid", alt.ID.ToString)   ' D4199
                cbPartial.InputAttributes.Remove("onclick")
                cbPartial.InputAttributes.Add("onclick", String.Format("last_focus=this.id; return onPartialChanged('partial', '{0}', '{1}', this.checked, event, {2});", alt.ID.ToString, tbMinPrc.ClientID, e.Row.RowIndex))    ' D2837 + D4199 + D4930
                cbPartial.InputAttributes.Add("onfocus", "last_focus=this.id;")   ' D3376
                tbMinPrc.Attributes.Add("data_id", String.Format("partial_edit{0}", e.Row.RowIndex))   ' D4199
            End If

            'Dim sID As String = String.Format("tr_{0}", alt.ID, IIf(fIsFunded, "_funded", ""))
            e.Row.Attributes.Remove("id")
            e.Row.Attributes.Add("id", String.Format("tr_{0}", e.Row.RowIndex)) ' D4122
            'e.Row.Attributes.Add("aidx", CStr(Scenario.AlternativesFull.IndexOf(alt))) ' D4503
            e.Row.Attributes.Add("aidx", e.Row.DataItemIndex.ToString) ' D4503
            e.Row.CssClass = String.Format("text grid_row{0}{1}", If(e.Row.RowIndex Mod 2 = 0, "", "_alt"), If(fIsFunded, " grid_row_funded", "")) ' D4149
            If fIsFunded Then e.Row.Attributes.Add("is_funded", "1") ' D4149

            ' -D4149 ===
            'e.Row.Attributes.Remove("onmouseover")
            'e.Row.Attributes.Add("onmouseover", String.Format("RowHover(this,1,{0},{1});", Bool2Num(e.Row.RowState = DataControlRowState.Alternate), Bool2Num(fIsFunded)))  ' D4122
            'e.Row.Attributes.Remove("onmouseout")
            'e.Row.Attributes.Add("onmouseout", String.Format("RowHover(this,0,{0},{1});", Bool2Num(e.Row.RowState = DataControlRowState.Alternate), Bool2Num(fIsFunded)))  ' D4122
            ' -D4149 ==

            ' D4122 ===
            If Not isExport Then
                If RA_Pages_Mode <> ecRAGridPages.NoPages Then
                    Dim PgIdx = CInt(RA_Pages_Mode) * RA_Pages_CurPage
                    If e.Row.RowIndex < PgIdx OrElse e.Row.RowIndex >= (PgIdx + CInt(RA_Pages_Mode)) Then
                        Dim sStyle As String = e.Row.Attributes("style")
                        If sStyle Is Nothing Then sStyle = ""
                        sStyle += "display:none;"
                        e.Row.Attributes.Remove("style")
                        e.Row.Attributes.Add("style", sStyle)
                    End If
                End If
            End If
            ' D4122 ==

            For i_int As Integer = 0 To e.Row.Cells.Count - 1
                Dim i As raColumnID = CType(i_int, raColumnID)
                Dim Cell As DataControlFieldCell = CType(e.Row.Cells(i_int), DataControlFieldCell)
                Dim fRemovedCC As Boolean = False   ' D6478
                Select Case i

                    Case raColumnID.Name
                        If Not isExport Then    ' D3189
                            Dim tmpAlt As IEnumerable(Of RAAlternative) = RA.Scenarios.Scenarios(0).Alternatives.Where(Function(X As RAAlternative) X.ID = alt.ID)
                            Dim sName As String = alt.Name
                            If tmpAlt IsNot Nothing AndAlso tmpAlt.Count > 0 Then sName = tmpAlt(0).Name
                            Cell.Attributes.Add("style", "padding: 0px 1ex;")
                            Cell.Attributes.Add("id", String.Format("tr_name_{0}", e.Row.RowIndex)) ' D4546
                            Cell.ToolTip = SafeFormString(sName)
                            'Cell.ID = String.Format("tdName{0}", e.Row.RowIndex)
                            ' D4790 ===
                            Dim tGUID As New Guid(alt.ID)
                            Dim tRealAlt As clsNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tGUID)
                            Dim fHasInfodoc As Boolean = tRealAlt IsNot Nothing AndAlso tRealAlt.InfoDoc <> ""
                            Dim sImg As String = String.Format("<a href='' onclick='OpenRichEditor(""?type={4}&field=infodoc&guid={3}&callback={2}""); return false;' style='margin-right:4px'><img src='{0}{1}' id='img_name_{2}' width=12 height=12 title=''></a>", ImagePath, If(fHasInfodoc, "info12.png", "info12_dis.png"), e.Row.RowIndex, alt.ID, CInt(reObjectType.Alternative))
                            Cell.Text = String.Format("{0}<span id='name{2}' title='{5}' style='cursor:pointer' onclick='switchAltNameEditor({2},1);'>{1}</span><input type='text' id='tbName{2}' style='width:90%; min-width:{5}ex; display:none;' value='{3}' onblur='switchAltNameEditor({2},0);'><input type='hidden' id='altID{2}' value='{4}'>", sImg, SafeFormString(ShortString(sName, CInt(IIf(RA_FixedColsWidth, 65, 150)), True)), e.Row.RowIndex, SafeFormString(sName), SafeFormString(alt.ID), SafeFormString(ResString("lblRAEditName")), sName.Length)  ' D3142 + A0930 + D3959 + D3962
                            ' D4790 ==
                        End If

                    Case raColumnID.isFunded
                        'Cell.Text = CStr(IIf(Not fIsFunded OrElse RA.Solver.SolverState <> raSolverState.raSolved, "&nbsp;", IIf(alt.DisplayFunded >= 1, "<b>" + ResString("lblYes").ToUpper + "</b>", String.Format("<b>{0}%</b> ({1})", CostString(alt.DisplayFunded * 100, Cents), CostString(alt.DisplayFunded * alt.Cost, Cents))))) ' D2837 + D2840 + D2855 + D2991 + A0907 + A0912 + D3199 + A0939
                        'Cell.Text = CStr(IIf(Not fIsFunded OrElse RA.Solver.SolverState <> raSolverState.raSolved, "&nbsp;", IIf(alt.DisplayFunded >= 1, "<b>" + ResString("lblYes").ToUpper + "</b>", String.Format("<b>{0}%</b>", CostString(alt.DisplayFunded * 100, Cents))))) ' D2837 + D2840 + D2855 + D2991 + A0907 + A0912 + D3199 + A0939 + D3546
                        Cell.Text = CStr(IIf(Not fIsFunded OrElse RA.Solver.SolverState <> raSolverState.raSolved, "&nbsp;", IIf(alt.DisplayFunded >= 1 AndAlso Not (alt.AllowCostTolerance AndAlso alt.FundedCost <> alt.Cost AndAlso alt.Cost <> 0), ResString("lblYes").ToUpper, String.Format("<b>{0}%</b>", CostString(If(alt.AllowCostTolerance AndAlso alt.FundedCost <> alt.Cost, alt.FundedCost / alt.Cost, alt.DisplayFunded) * 100, Cents))))) ' D2837 + D2840 + D2855 + D2991 + A0907 + A0912 + D3199 + A0939 + D3546 + D4341 + D4933

                        ' D4803 ===
                    Case raColumnID.LastFundedPeriod
                        Cell.Text = String.Format("<nobr>{0}</nobr>", If(fIsFunded, GetLastFundedPeriod(alt), "&nbsp;"))

                    Case raColumnID.Groups
                        Dim sLst As String = ""

                        Const sHidStyle As String = "display:none;"    ' D4810
                        Dim idx As Integer = 1  ' D4810
                        Dim sPlainLst As String = ""    ' D4810
                        For Each tGrp As KeyValuePair(Of String, RAGroup) In Scenario.Groups.Groups
                            If tGrp.Value.Alternatives.ContainsKey(alt.ID) Then
                                Dim sType As String = String.Format(ResString("optGroupType" + CInt(tGrp.Value.Condition).ToString), ParseString(If(App.ActiveProject.IsRisk, "%%controls%%", "%%alternatives%%")))
                                Dim sColor As String = ""
                                Select Case tGrp.Value.Condition
                                    Case RAGroupCondition.gcAllOrNothing
                                        sColor = "339933"
                                    Case RAGroupCondition.gcEqualsOne
                                        sColor = "993333"
                                    Case RAGroupCondition.gcGreaterOrEqualsOne
                                        sColor = "9900cc"
                                    Case RAGroupCondition.gcLessOrEqualsOne
                                        sColor = "0000cc"
                                    Case Else
                                        sColor = "cccccc"
                                End Select
                                sColor = "color:#" + sColor + ";"
                                sLst += String.Format("<span class='text'{2}><span class='grp_id' style='{5}{7}'><span class='small'>#</span>{4} </span><span class='small grp_title' style='{6}'><img src='/images/ra/{0}' width=14 height=14 border=0 title='{3}' style='margin-right:2px'>{1}<br></span></span>", _GROUP_TYPE_IMAGES.GetValue(CInt(tGrp.Value.Condition)), ShortString(tGrp.Value.Name, 30), If(tGrp.Value.Enabled, "", " style='text-decoration:line-through;'"), SafeFormString(sType), idx, If(RA_ShowGroupDetails, sHidStyle, ""), If(RA_ShowGroupDetails, "", sHidStyle), sColor)   ' D4810
                                sPlainLst += String.Format("{0}#{1} {2} ({3}); ", If(sPlainLst = "", "", " <br>"), idx, tGrp.Value.Name, sType) ' D4810 + D4815
                            End If
                            idx += 1    ' D4810
                            If Not fRemovedCC AndAlso Scenario.GetInfeasibilityRemovedConstraint(ConstraintType.ctGroup, tGrp.Key.ToString) IsNot Nothing Then fRemovedCC = True  ' D6478
                        Next
                        If sLst = "" AndAlso Not isExport Then sLst = "&nbsp;"
                        Cell.Text = String.Format("<nobr>{0}</nobr>", sLst)
                        Cell.ToolTip = sPlainLst        ' D4810

                        ' D4803 ==

                    Case raColumnID.ProbFailure, raColumnID.ProbSuccess, raColumnID.MinPercent, raColumnID.CostTolerance ' D4348 + D4930
                        Dim tbName As String = ""
                        Dim tVal As String = Cell.Text
                        Select Case i
                            Case raColumnID.ProbFailure
                                tbName = "PFailure" ' D4348
                                If alt.RiskOriginal <> UNDEFINED_INTEGER_VALUE Then tVal = Double2String(alt.RiskOriginal, RA.Scenarios.GlobalSettings.Precision)
                                Cell.ToolTip = tVal 'A0911
                            Case raColumnID.ProbSuccess
                                tbName = "ProbSuccess"
                                If alt.RiskOriginal <> UNDEFINED_INTEGER_VALUE Then tVal = Double2String(1 - alt.RiskOriginal, RA.Scenarios.GlobalSettings.Precision)
                                Cell.ToolTip = tVal 'A0969
                            Case raColumnID.MinPercent
                                tbName = "MinPrc"
                                tVal = Double2String(alt.MinPercent, RA.Scenarios.GlobalSettings.Precision)
                                ' D4930 ===
                            Case raColumnID.CostTolerance
                                tbName = "Tolerance"
                                tVal = Double2String(alt.CostTolerance, RA.Scenarios.GlobalSettings.Precision)
                                ' D4930 ==
                        End Select
                        ' D3189 ===
                        If isExport Then
                            Cell.Text = CStr(IIf(i = raColumnID.MinPercent AndAlso Not alt.IsPartial OrElse i = raColumnID.CostTolerance AndAlso Not alt.AllowCostTolerance, "", tVal.Replace(ChrW(160), " ")))    ' replace special symbol #160 with space;
                            Cell.HorizontalAlign = HorizontalAlign.Right
                        Else
                            Dim is_0_1_values As Boolean = (i = raColumnID.ProbFailure OrElse i = raColumnID.Risk OrElse i = raColumnID.ProbSuccess OrElse i = raColumnID.CostTolerance OrElse i = raColumnID.MinPercent)   ' D4348 + D4930
                            Dim tbEdit As TextBox = CType(e.Row.FindControl("tb" + tbName), TextBox)
                            If tbEdit IsNot Nothing Then
                                tbEdit.Text = tVal
                                tbEdit.Attributes.Remove("onfocus")
                                tbEdit.Attributes.Add("onfocus", "onFocus(this.id, this.value);")    ' D2886
                                tbEdit.Attributes.Remove("onblur")
                                tbEdit.Attributes.Add("onblur", String.Format("onBlur(this.id, '{0}', '{1}', {2});", tbName.ToLower, alt.ID.ToString, IIf(i = raColumnID.Cost, -1, IIf(is_0_1_values, 1, 0))))  ' D2886 + D3299
                                tbEdit.Attributes.Remove("onkeyup")
                                tbEdit.Attributes.Add("onkeyup", String.Format("onKeyUp(this.id, {0});", IIf(is_0_1_values, 1, 0))) ' D2886                                
                                If Scenario.Alternatives.Count < TAB_IDX_MAX_ALTS Then tbEdit.TabIndex = CShort((e.Row.RowIndex + 1) * TAB_COEFF + i) Else tbEdit.Attributes.Add("tabindex", CStr((e.Row.RowIndex + 1) * TAB_COEFF + i)) ' D2897 + D3376 + D3423 + D4821
                                If OPT_DISABLE_EDITS_ON_IGNORE AndAlso (i = raColumnID.ProbFailure OrElse i = raColumnID.Risk OrElse i = raColumnID.ProbSuccess) AndAlso Not Scenario.Settings.Risks Then tbEdit.Enabled = False ' D3125 + D4348
                                If i = raColumnID.MinPercent AndAlso Not alt.IsPartial OrElse i = raColumnID.CostTolerance AndAlso Not alt.AllowCostTolerance Then tbEdit.Attributes.Add("style", "display:none;")   ' D4930
                                ' D6628 ===
                                If String.IsNullOrEmpty(tbEdit.ToolTip) Then
                                    Dim sTitle As String = HTML2Text(If(String.IsNullOrEmpty(GridAlternatives.HeaderRow.Cells(i).ToolTip), GridAlternatives.HeaderRow.Cells(i).Text, GridAlternatives.HeaderRow.Cells(i).ToolTip))
                                    tbEdit.ToolTip = String.Format("{0}<br>{1}", sTitle.Replace(_SORT_ASC, "").Replace(_SORT_DESC, ""), SafeFormString(alt.Name))    ' D7072
                                End If
                                ' D6628 ==
                            End If
                            ' D3189 ==
                        End If

                        ' D4348 ===
                    Case raColumnID.Risk
                        Dim tVal As Double = alt.Risk
                        If isExport Then
                            Cell.Text = tVal.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat)
                            Cell.HorizontalAlign = HorizontalAlign.Right
                        Else
                            Cell.ToolTip = Cell.Text
                            If Not String.IsNullOrEmpty(Cell.Text) Then
                                Cell.Text = AddBar(tVal.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat), tVal)
                            Else
                                If Cell.Text = "" Then Cell.Text = "&nbsp;"
                            End If
                        End If
                        ' D4348 ==

                        ' D3916 ===
                    Case raColumnID.Cost

                        Dim sValue As String = ""
                        If alt.Cost <> UNDEFINED_INTEGER_VALUE Then sValue = CostString(alt.Cost, Cents)

                        ' -D4490 ===
                        'Dim tWidth As String = "6em"
                        'If isTimeperiodsVisible() Then tWidth = String.Format("{0}px", OPT_COL_WIDTH_TIMEPERIOD)
                        'If fPF Then tWidth = String.Format("{0}px", Math.Round(IIf(isTimeperiodsVisible, OPT_COL_WIDTH_TIMEPERIOD / 1.8, 45)))
                        ' -D4490 ==

                        If isExport Then
                            Cell.Text = sValue.Replace(ChrW(160), " ")    ' replace special symbol #160 with space;
                            Cell.HorizontalAlign = HorizontalAlign.Right
                        Else
                            Cell.ToolTip = sValue   ' D3935
                            Cell.Text = String.Format("<input type='text' id='tbCost{0}' class='input as_number' value='{1}' style='width:{2}ex;' onfocus='onFocus(this.id, this.value);' onkeyup='onKeyUp(this.id, -1);' onblur='onBlur(this.id, ""cost"", ""{3}"", -1);' TabIndex={4} title='{5}'>", alt.ID.Substring(0, 6), SafeFormString(sValue), MaxCostLen, alt.ID, (e.Row.RowIndex + 1) * TAB_COEFF + i, String.Format("{0}<br>{1}", SafeFormString("Cost"), SafeFormString(alt.Name)))    ' D4821
                        End If

                        If fPF AndAlso alt.Cost <> 0 Then   ' D7075
                            Dim sPCost As String = String.Format("<nobr>{0}<span class='small'> of </span></nobr>", CostString(If(alt.AllowCostTolerance AndAlso alt.FundedCost <> alt.Cost, alt.FundedCost, alt.DisplayFunded * alt.Cost), Cents)) ' D4934
                            If isExport Then
                                If alt.Cost <> UNDEFINED_INTEGER_VALUE Then Cell.Text = CostString(alt.DisplayFunded * alt.Cost, Cents) ' D3552
                            Else
                                Cell.Text = sPCost + Cell.Text
                            End If
                        End If
                        ' D3916 ==

                    Case raColumnID.Benefit, raColumnID.EBenefit    ' D2843 + D2941 + D3174
                        Dim tVal As Double = CDbl(IIf(i = raColumnID.EBenefit, alt.Benefit, alt.BenefitOriginal))    ' D2941 + D3174 + D3175
                        ' D3189 ===
                        If isExport Then
                            Cell.Text = tVal.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat)
                            Cell.HorizontalAlign = HorizontalAlign.Right
                        Else
                            ' D3189 ==
                            Cell.ToolTip = Cell.Text 'A0910
                            If Not String.IsNullOrEmpty(Cell.Text) Then ' D2837 + D2941
                                Cell.Text = AddBar(tVal.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat), tVal)
                            Else
                                If Cell.Text = "" Then Cell.Text = "&nbsp;"
                            End If
                        End If

                        ' D2897 ===
                    Case raColumnID.isPartial
                        ' D3189 ===
                        If isExport Then
                            Cell.Text = CStr(IIf(alt.IsPartial, "X", ""))
                            Cell.HorizontalAlign = HorizontalAlign.Center
                        Else
                            ' D3189 ==
                            Dim acbPartial As CheckBox = CType(e.Row.FindControl("cbPartial"), CheckBox)
                            If acbPartial IsNot Nothing AndAlso Scenario.Alternatives.Count < TAB_IDX_MAX_ALTS Then  ' D3423
                                acbPartial.TabIndex = CShort(GetTabIndex(e.Row.RowIndex, i))   ' D3376
                            Else
                                acbPartial.InputAttributes.Add("tabindex", CStr((e.Row.RowIndex + 1) * TAB_COEFF + i))  ' D4821
                                'acbPartial.InputAttributes.Add("onfocus", "last_focus = this.id;")  ' D2986 - D3376
                            End If
                        End If
                        ' D2897 ==

                    ' D4930 ===
                    Case raColumnID.isCostTolerance
                        ' D3189 ===
                        If isExport Then
                            Cell.Text = CStr(IIf(alt.AllowCostTolerance, "X", ""))
                            Cell.HorizontalAlign = HorizontalAlign.Center
                        Else
                            Dim acbIsTolerance As CheckBox = CType(e.Row.FindControl("cbIsTolerance"), CheckBox)
                            If acbIsTolerance IsNot Nothing AndAlso Scenario.Alternatives.Count < TAB_IDX_MAX_ALTS Then
                                acbIsTolerance.TabIndex = CShort(GetTabIndex(e.Row.RowIndex, i))
                            Else
                                acbIsTolerance.InputAttributes.Add("tabindex", CStr((e.Row.RowIndex + 1) * TAB_COEFF + i))
                            End If
                        End If
                        ' D4930 ==

                        ' D2840 ===
                    Case raColumnID.Musts
                        Dim acbMusts As CheckBox = CType(e.Row.FindControl("cbMusts"), CheckBox) ' D3017
                        If acbMusts IsNot Nothing Then
                            If OPT_DISABLE_EDITS_ON_IGNORE Then acbMusts.Enabled = Scenario.Settings.Musts ' D3125
                            If Scenario.Alternatives.Count < TAB_IDX_MAX_ALTS Then
                                acbMusts.TabIndex = CShort((e.Row.RowIndex + 1) * TAB_COEFF + i) ' D2897 + D3376 + D3423
                            Else
                                acbMusts.InputAttributes.Add("tabindex", CStr((e.Row.RowIndex + 1) * TAB_COEFF + i)) ' D4821
                            End If
                            'acbMusts.InputAttributes.Add("onfocus", "last_focus = this.id;")  ' D2986 - D3376
                        End If
                        ' D3189 ===
                        If isExport Then
                            Cell.Text = CStr(IIf(acbMusts.Checked, "X", ""))
                            Cell.HorizontalAlign = HorizontalAlign.Center
                        End If
                        If Scenario.GetInfeasibilityRemovedConstraint(ConstraintType.ctMust, alt.ID.ToString) IsNot Nothing Then fRemovedCC = True  ' D6478
                        ' D3189 ==

                    Case raColumnID.MustNot
                        Dim acbMustNot As CheckBox = CType(e.Row.FindControl("cbMustNot"), CheckBox)
                        If acbMustNot IsNot Nothing Then
                            If OPT_DISABLE_EDITS_ON_IGNORE Then acbMustNot.Enabled = Scenario.Settings.MustNots ' D3125
                            If Scenario.Alternatives.Count < TAB_IDX_MAX_ALTS Then
                                acbMustNot.TabIndex = CShort((e.Row.RowIndex + 1) * TAB_COEFF + i) ' D2897 + D3376 + D3423
                            Else
                                acbMustNot.InputAttributes.Add("tabindex", CStr((e.Row.RowIndex + 1) * TAB_COEFF + i)) ' D4821
                            End If
                            'acbMustNot.InputAttributes.Add("onfocus", "last_focus = this.id;")  ' D2986 - D3376
                        End If
                        ' D2840 ==
                        ' D3189 ===
                        If isExport Then
                            Cell.Text = CStr(IIf(acbMustNot.Checked, "X", ""))
                            Cell.HorizontalAlign = HorizontalAlign.Center
                        End If
                        If Scenario.GetInfeasibilityRemovedConstraint(ConstraintType.ctMustNot, alt.ID.ToString) IsNot Nothing Then fRemovedCC = True  ' D6478
                        ' D3189 ==

                End Select

                ' D2886 ===
                Dim isCCEnabled As Boolean = True   ' D4020
                If i >= raColumnID.CustomConstraintsStart AndAlso i < raColumnID.CustomConstraintsStart + Scenario.Constraints.Constraints.Count Then 'A1010
                    Dim idx As Integer = i - CInt(raColumnID.CustomConstraintsStart)
                    With Scenario.Constraints
                        If idx < .Constraints.Keys.Count Then
                            Dim tID As Integer = .Constraints.Keys(idx)
                            If .Constraints.ContainsKey(tID) Then
                                Dim tConstr As RAConstraint = .Constraints(tID)
                                Dim tVal As Double = .GetConstraintValue(tConstr.ID, alt.ID)
                                Dim sVal As String = ""
                                If tVal <> UNDEFINED_INTEGER_VALUE Then sVal = CStr(tVal) '.ToString("F2")
                                If tConstr.isDollarValue AndAlso sVal <> "" Then sVal = CostString(tVal, Cents)   ' D6467
                                ' D3189 ===
                                Cell.Attributes.Add("clip_data_id", i_int.ToString) 'A0910 + A0919
                                Cell.Attributes.Add("clip_data", JS_SafeString(CStr(IIf(sVal = "", CLIPBOARD_CHAR_UNDEFINED_VALUE, sVal.Replace("&#160;", " "))))) 'A0910 + A0961
                                isCCEnabled = tConstr.Enabled   ' D4020
                                If isExport OrElse (tConstr.IsLinked AndAlso tConstr.IsReadOnly) Then  ' D3340
                                    Cell.Text = sVal
                                    Cell.HorizontalAlign = HorizontalAlign.Right
                                    ' D3539 ===
                                    If Not tConstr.Enabled Then
                                        Cell.ForeColor = Color.FromArgb(255, 120, 120, 120) ' D3539
                                        Cell.Font.Italic = True
                                    End If
                                    ' D3539 ==
                                    If fPF AndAlso sVal <> "" Then Cell.Text = (alt.Funded * tVal).ToString("F2") ' D3552
                                Else
                                    ' D3189 ==
                                    Cell.Text = String.Format("<input type='text' id='tbConstr{0}a{1}' class='input as_number {7}' value='{2}' style='width:{8};' onfocus='onFocus(this.id, this.value);' onkeyup='onKeyUp(this.id, -1);' onblur='onBlur(this.id, ""constraint"", ""{3}&cid={4}"", -1);'{5} TabIndex={6}>", idx, alt.ID.Substring(0, 6), SafeFormString(sVal), alt.ID, tConstr.ID, IIf(tConstr.Enabled OrElse RA_OPT_CC_EDIT_DISABLED, "", " disabled=1"), (e.Row.RowIndex + 1) * TAB_COEFF + i, String.Format("{0}{1}", IIf(fIsFunded, "ra_funded_solid_cell", ""), IIf(Not tConstr.Enabled OrElse Not Scenario.Settings.CustomConstraints, " color:#606060; font-style:italic;", "")), IIf(isTimeperiodsVisible, CStr(IIf(fPF, Math.Round(OPT_COL_WIDTH_TIMEPERIOD / 2.2), OPT_COL_WIDTH_TIMEPERIOD)) + "px", CStr(IIf(fPF, 3, 5)) + "em"))  ' D2897 + D3038 + D3376 + D3423 + D3539 + D3682 + D4821
                                    If fPF AndAlso sVal <> "" Then Cell.Text = String.Format("<nobr>{0}<span class='small'> of </span>{1}</nobr>", CostString(alt.Funded * tVal, Cents), Cell.Text) ' D3552
                                    Cell.Enabled = tConstr.Enabled OrElse RA_OPT_CC_EDIT_DISABLED ' D3539
                                End If
                            End If
                        End If
                    End With
                End If
                ' D2886 ==

                ' D3915 ===
                If isTimeperiodsVisible() AndAlso (i = raColumnID.Cost OrElse (i >= raColumnID.CustomConstraintsStart AndAlso i < raColumnID.CustomConstraintsStart + Scenario.Constraints.Constraints.Count)) AndAlso (i = raColumnID.Cost OrElse OPT_SHOW_TP_CC) Then   ' D3916 + D3921
                    ' D3960 ===
                    Dim tMax As Double = UNDEFINED_INTEGER_VALUE
                    Dim tPeriods As List(Of Double) = GetAltPeriodsData(alt, CInt(IIf(i = raColumnID.Cost, -1, i - raColumnID.CustomConstraintsStart)), tMax)

                    If tPeriods IsNot Nothing Then
                        Dim Periods As New List(Of String)
                        For Each tVal As Double In tPeriods
                            Dim sData As String = "&nbsp;"
                            If tVal <> UNDEFINED_INTEGER_VALUE Then
                                sData = CostString(tVal, Cents)
                                If RA_ShowTinyBars Then
                                    If tMax <> 0 AndAlso tMax <> UNDEFINED_INTEGER_VALUE Then
                                        sData += "<br>" + AddBar("", tVal / tMax)
                                    End If
                                End If
                            End If
                            Periods.Add(String.Format("{0}", sData))
                        Next
                        AddTimePeriodCells(Cell.Text, i_int, e.Row.RowIndex + 1, Periods, isCCEnabled) ' D3917 + D3921 + D4020
                    End If
                    ' D3960 ==
                End If
                ' D3915 ==

                'A1010 ===
                If i >= raColumnID.CustomConstraintsStart + Scenario.Constraints.Constraints.Count Then
                    ' attributes columns
                    Dim tAllAttributes = PM.Attributes.GetAlternativesAttributes(True)
                    Dim index = i - (raColumnID.CustomConstraintsStart + Scenario.Constraints.Constraints.Count)
                    If index < tAllAttributes.Count Then
                        Dim tAltGuid As Guid = New Guid(alt.ID)
                        Dim tAttr As clsAttribute = tAllAttributes(index)
                        Dim attrValue As Object = PM.Attributes.GetAttributeValue(tAttr.ID, tAltGuid)
                        Dim sVal As String = ""
                        Dim sValPlain As String = Nothing   ' D6583
                        Cell.HorizontalAlign = HorizontalAlign.Left
                        Select Case tAttr.ValueType
                            Case AttributeValueTypes.avtBoolean
                                sVal = CStr(IIf(CBool(attrValue), ResString("lblYes"), ResString("lblNo")))
                            Case AttributeValueTypes.avtEnumeration
                                If attrValue IsNot Nothing Then
                                    Dim tEnum As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(tAttr.EnumID)
                                    If tEnum IsNot Nothing Then
                                        Dim catID As Guid = Guid.Empty
                                        If TypeOf attrValue Is Guid Then catID = CType(attrValue, Guid)
                                        If TypeOf attrValue Is String AndAlso CStr(attrValue).Length > 32 Then catID = New Guid(CStr(attrValue))
                                        Dim tEnumItem As clsAttributeEnumerationItem = tEnum.GetItemByID(catID)
                                        If tEnumItem IsNot Nothing Then sVal = tEnumItem.Value
                                    End If
                                End If
                            Case AttributeValueTypes.avtEnumerationMulti
                                'not implemented
                            Case Else
                                If attrValue IsNot Nothing Then
                                    sVal = attrValue.ToString()
                                    ' D6583 ===
                                    If tAttr.ValueType = AttributeValueTypes.avtString AndAlso Not isExport AndAlso sVal.IndexOf("://") > 2 Then
                                        sValPlain = sVal
                                        sVal = ParseTextHyperlinks(sVal)
                                    End If
                                End If
                                If tAttr.ValueType = AttributeValueTypes.avtDouble OrElse tAttr.ValueType = AttributeValueTypes.avtLong Then Cell.HorizontalAlign = HorizontalAlign.Right
                                ' D6583 ==
                        End Select
                        If String.IsNullOrEmpty(sValPlain) Then sValPlain = sVal    ' D6583
                        Cell.Attributes.Add("clip_data_id", i_int.ToString)
                        Cell.Attributes.Add("clip_data", JS_SafeString(CStr(IIf(sVal = "", CLIPBOARD_CHAR_UNDEFINED_VALUE, sValPlain.Replace("&#160;", " ")))))  ' D6583
                        If isExport Then
                            Cell.Text = sVal
                        Else
                            Cell.Text = sVal
                        End If
                    End If
                End If
                'A1010 ==

                If fIsFunded Then
                    If isExport Then Cell.BackColor = System.Drawing.Color.FromArgb(255, 255, 128)
                    ' D3038 ===
                    For Each tCtrl As Object In Cell.Controls
                        If TypeOf (tCtrl) Is TextBox Then CType(tCtrl, TextBox).BackColor = Color.FromArgb(209, 230, 181) 'Color.FromArgb(255, 255, 128)
                        If TypeOf (tCtrl) Is CheckBox Then CType(tCtrl, CheckBox).BackColor = Color.FromArgb(209, 230, 181) 'Color.FromArgb(255, 255, 128)
                    Next
                    ' D3038 ==
                    ' -D3441
                    'If i = raColumnID.Name Then
                    '    Cell.Font.Bold = True
                    '    'If alt.Funded > 0 AndAlso alt.Funded < 1 Then Cell.Font.Italic = True ' D2837
                    'End If
                End If

                If fRemovedCC Then Cell.BackColor = Color.FromArgb(250, 241, 157)   ' D6478
                ' D6627 ===
                If String.IsNullOrEmpty(Cell.ToolTip) AndAlso Not isExport Then
                    Dim sTitle As String = HTML2Text(If(String.IsNullOrEmpty(GridAlternatives.HeaderRow.Cells(i).ToolTip), GridAlternatives.HeaderRow.Cells(i).Text, GridAlternatives.HeaderRow.Cells(i).ToolTip))  ' D6628
                    Cell.ToolTip = String.Format("{0}<br>{1}", sTitle, SafeFormString(alt.Name))    ' D6628
                End If
                ' D6627 ==
            Next

            ' D3014 ===
            If FundedBefore IsNot Nothing AndAlso isSolving AndAlso RA_ShowChanges Then
                If (fIsFunded AndAlso Not FundedBefore.Keys.Contains(alt.ID)) OrElse
                   (Not fIsFunded AndAlso FundedBefore.Keys.Contains(alt.ID)) OrElse
                   (fIsFunded AndAlso FundedBefore.Keys.Contains(alt.ID) AndAlso alt.DisplayFunded <> FundedBefore(alt.ID)) Then 'A0939
                    e.Row.Cells(CInt(raColumnID.ID)).Font.Italic = True ' D3441
                    e.Row.Cells(CInt(raColumnID.Name)).Font.Italic = True
                    e.Row.Cells(CInt(raColumnID.Name)).Font.Bold = True ' D3441
                    e.Row.Cells(CInt(raColumnID.Name)).CssClass += " flash"
                    e.Row.Cells(CInt(raColumnID.isFunded)).CssClass = " flash"
                    e.Row.Cells(CInt(raColumnID.isFunded)).Font.Bold = True ' D3441
                    If Not fIsFunded Then
                        e.Row.Cells(CInt(raColumnID.isFunded)).Text = "&nbsp;<span Class='tohide'>" + ResString("lblRANotFunded") + "</span>&nbsp;"
                    End If
                End If
            End If
            ' D3014 ==

            If alt.MustNot AndAlso Scenario.Settings.MustNots Then e.Row.ForeColor = Color.FromArgb(160, 80, 80) ' D2837 + D2839 + D2990
            If alt.Must AndAlso Scenario.Settings.Musts Then e.Row.ForeColor = Color.FromArgb(32, 128, 32) ' D2839 + D2990

            'e.Row.Cells(CInt(raColumnID.EBenefit)).Font.Bold = True      ' D3174 -D3441

            If Not isExport Then    ' D3189
                'A0910 + D3185 ===
                Dim DataCells() As raColumnID = {raColumnID.Name, raColumnID.Benefit, raColumnID.EBenefit, raColumnID.Cost, raColumnID.ProbFailure, raColumnID.Risk, raColumnID.ProbSuccess, raColumnID.Groups} 'A0969 + D4348 + D4810
                For Each tCol As raColumnID In DataCells
                    Dim clip_data As String = e.Row.Cells(CInt(tCol)).ToolTip 'A0961
                    If String.IsNullOrEmpty(clip_data) Then clip_data = CLIPBOARD_CHAR_UNDEFINED_VALUE 'A0961
                    e.Row.Cells(CInt(tCol)).Attributes.Add("clip_data_id", CInt(tCol).ToString) 'A0919
                    e.Row.Cells(CInt(tCol)).Attributes.Add("clip_data", JS_SafeString(clip_data.Replace("&#160;", " "))) 'A0961
                    If tCol <> raColumnID.Groups AndAlso (tCol > raColumnID.Name OrElse e.Row.Cells(CInt(tCol)).ToolTip.Length < 40) Then e.Row.Cells(CInt(tCol)).ToolTip = "" ' D3917 + D4810
                Next
                'A0910 + D3185 ==
            End If
        End If

        ' D2874 ===
        If e.Row.RowType = DataControlRowType.Footer Then
            e.Row.Attributes.Add("id", "tr_footer")   ' D4122
            ' D2892 ===
            With Scenario
                Dim FundedCount As Integer = .Alternatives.Sum(Function(a) If(a.DisplayFunded > 0, 1, 0)) 'A0939
                Dim FundedBenefitOrignal As Double = .Alternatives.Sum(Function(a) If(Double.IsNaN(a.BenefitOriginal) OrElse a.BenefitOriginal = UNDEFINED_INTEGER_VALUE, 0, a.Funded * a.BenefitOriginal)) 'A1457
                Dim MinBenefitOriginal As Double = .Alternatives.Min(Function(a) a.BenefitOriginal)
                Dim MaxBenefitOriginal As Double = .Alternatives.Max(Function(a) a.BenefitOriginal)
                Dim MinBenefit As Double = .Alternatives.Min(Function(a) a.Benefit)
                Dim MaxBenefit As Double = .Alternatives.Max(Function(a) a.Benefit)
                Dim MinRisks As Double = .Alternatives.Min(Function(a) a.RiskOriginal)
                Dim MaxRisks As Double = .Alternatives.Max(Function(a) a.RiskOriginal)
                Dim FundedRisks As Double = .Alternatives.Sum(Function(a) If(a.RiskOriginal = UNDEFINED_INTEGER_VALUE, 0, a.Funded * a.RiskOriginal))    ' D2938
                Dim TotalRisks As Double = .Alternatives.Sum(Function(a) If(a.RiskOriginal = UNDEFINED_INTEGER_VALUE, 0, a.RiskOriginal))
                Dim MinPS As Double = .Alternatives.Min(Function(a) 1 - a.RiskOriginal)
                Dim MaxPS As Double = .Alternatives.Max(Function(a) 1 - a.RiskOriginal)
                Dim FundedPS As Double = .Alternatives.Sum(Function(a) If(a.RiskOriginal = UNDEFINED_INTEGER_VALUE, 0, a.Funded * (1 - a.RiskOriginal)))
                Dim TotalPS As Double = .Alternatives.Sum(Function(a) If(a.RiskOriginal = UNDEFINED_INTEGER_VALUE, 0, 1 - a.RiskOriginal))
                Dim MinCost As Double = .Alternatives.Min(Function(a) a.Cost)
                Dim MaxCost As Double = .Alternatives.Max(Function(a) a.Cost)
                Dim PartialCount As Integer = .Alternatives.Sum(Function(a) If(a.IsPartial, 1, 0))
                Dim ToleranceCount As Integer = .Alternatives.Sum(Function(a) If(a.AllowCostTolerance, 1, 0)) ' D4930
                Dim MustsCount As Integer = .Alternatives.Sum(Function(a) If(a.Must, 1, 0))
                Dim MustNotCount As Integer = .Alternatives.Sum(Function(a) If(a.MustNot, 1, 0))
                ' D2892 ==

                Dim fShowCC As Boolean = (RA.Scenarios.GlobalSettings.ShowCustomConstraints AndAlso Scenario.Constraints.Constraints.Count > 0)    ' D3192 + D3781
                Dim fShowSurplus As Boolean = fShowCC AndAlso (hasDisabledCCWithMax OrElse Not Scenario.Settings.CustomConstraints) AndAlso RA.Solver.SolverState = raSolverState.raSolved  ' D3540 + D3681 + D3683
                Dim sBlank As String = CStr(IIf(fShowCC AndAlso isExport, "-<br>", ""))   ' D3192
                Dim sBlankSurplus As String = CStr(IIf(fShowSurplus, IIf(isExport, "-<br>", "<br>"), ""))   ' D3540

                ' D2887 + D3540 ===
                e.Row.Cells(CInt(raColumnID.isFunded)).Text = String.Format("{0}{1}{2}{3}", IIf(fShowCC AndAlso isExport, sBlank + sBlank, ""), IIf(RA.Solver.SolverState = raSolverState.raSolved, FundedCount.ToString + "<br>", ""), sBlankSurplus, Scenario.Alternatives.Count)      ' D4128
                e.Row.Cells(CInt(raColumnID.EBenefit)).Text = String.Format("{0}{1}{2}{4}{3}", IIf(RA_ShowMinMaxExtra, MinBenefit.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA_ShowMinMaxExtra, MaxBenefit.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA.Solver.SolverState = raSolverState.raSolved, RA.Solver.FundedBenefit.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", ""), RA.Solver.TotalBenefit.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat), sBlankSurplus)   ' D2941 + D3174 + D3175 + D3192
                e.Row.Cells(CInt(raColumnID.Benefit)).Text = String.Format("{0}{1}{2}{4}{3}", IIf(RA_ShowMinMaxExtra, MinBenefitOriginal.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA_ShowMinMaxExtra, MaxBenefitOriginal.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA.Solver.SolverState = raSolverState.raSolved, RA.Solver.FundedOriginalBenefit.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", ""), RA.Solver.TotalBenefitOriginal.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat), sBlankSurplus)   ' D3174 + D3175 + A0917
                e.Row.Cells(CInt(raColumnID.ProbFailure)).Text = String.Format("{0}{1}{2}{4}{3}", IIf(RA_ShowMinMaxExtra, MinRisks.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA_ShowMinMaxExtra, MaxRisks.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA.Solver.SolverState = raSolverState.raSolved, If(isExport, "-", "&nbsp;") + "<br>", ""), If(isExport, "-", "&nbsp;"), sBlankSurplus)  ' D3189 + D3251
                e.Row.Cells(CInt(raColumnID.Risk)).Text = String.Format("{0}{1}{2}{4}{3}", IIf(RA_ShowMinMaxExtra, MinRisks.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA_ShowMinMaxExtra, MaxRisks.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA.Solver.SolverState = raSolverState.raSolved, If(isExport, "-", "&nbsp;") + "<br>", ""), If(isExport, "-", "&nbsp;"), sBlankSurplus)  ' D3189 + D3251 + D4348
                e.Row.Cells(CInt(raColumnID.ProbSuccess)).Text = String.Format("{0}{1}{2}{3}&nbsp;", IIf(RA_ShowMinMaxExtra, MinPS.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA_ShowMinMaxExtra, MaxPS.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", sBlank), IIf(RA.Solver.SolverState = raSolverState.raSolved, FundedPS.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) + "<br>", ""), sBlankSurplus)
                e.Row.Cells(CInt(raColumnID.Cost)).Text = String.Format("{0}{1}<b>{2}</b>{4}{3}", IIf(RA_ShowMinMaxExtra, CostString(MinCost, Cents) + "<br>", IIf(fShowCC, "<br>", sBlank)), IIf(RA_ShowMinMaxExtra, CostString(MaxCost, Cents) + "<br>", IIf(fShowCC, "<br>", sBlank)), IIf(RA.Solver.SolverState = raSolverState.raSolved, CostString(RA.Solver.FundedCost, Cents) + "<br>", ""), CostString(RA.Solver.TotalCost, Cents), sBlankSurplus).Replace(" ", " ")    ' AD: replace special symbol #160 with space; 'A0907 + D3199 + D4004
                e.Row.Cells(CInt(raColumnID.isPartial)).Text = CStr(IIf(RA_ShowMinMaxExtra, PartialCount.ToString, "&nbsp;"))
                e.Row.Cells(CInt(raColumnID.isCostTolerance)).Text = CStr(IIf(RA_ShowMinMaxExtra, ToleranceCount.ToString, "&nbsp;"))   ' D4930
                e.Row.Cells(CInt(raColumnID.Musts)).Text = CStr(IIf(RA_ShowMinMaxExtra, MustsCount.ToString, "&nbsp;"))
                e.Row.Cells(CInt(raColumnID.MustNot)).Text = CStr(IIf(RA_ShowMinMaxExtra, MustNotCount.ToString, "&nbsp;"))
                ' D3540 ==
                ' D4203 ===
                Dim CC_offset = 0   ' D4205
                If fShowCC Then
                    Dim tCol As raColumnID = raColumnID.MustNot
                    While Not RA_Columns.Contains(CInt(tCol)) AndAlso tCol > raColumnID.isPartial
                        tCol = CType(CInt(tCol) - 1, raColumnID)
                    End While
                    If tCol > raColumnID.isPartial AndAlso RA_Columns.Contains(CInt(tCol) - 1) Then
                        e.Row.Cells(tCol - 1).ColumnSpan = 2    ' D4205
                        e.Row.Cells(tCol).Visible = False
                        e.Row.Cells(tCol - 1).Text = String.Format("<div style='padding-top:3px'><b>{0}<br>{1}<br>{2}{3}</div>", ResString("tblRowMin"), ResString("tblRowMax"), If(RA.Solver.SolverState = raSolverState.raSolved, "<br>", ""), IIf(fShowSurplus, ResString("lblRACCSurplus"), ""))    ' D4205
                        e.Row.Cells(tCol - 1).VerticalAlign = VerticalAlign.Top
                        e.Row.Cells(tCol - 1).HorizontalAlign = HorizontalAlign.Right
                        CC_offset = -1  ' D4205
                    End If
                End If
                ' D4203 ==
                e.Row.Cells(CInt(raColumnID.Name)).Text = String.Format("<b>{1}{0}{2}{3}</b>", If(RA.Solver.SolverState = raSolverState.raSolved, ResString("tblRAFunded") + ":<br>", ""), If(RA_ShowMinMaxExtra OrElse (CC_offset = 0 AndAlso fShowCC), String.Format("{0}<br>{1}<br>", ResString("tblRowMin"), ResString("tblRowMax")), IIf(fShowCC, "<br><br>", "")), If(fShowSurplus, If(CC_offset = 0, ResString("lblRACCSurplus"), "") + "<br>", ""), ResString("lblRATotal") + ":")   ' D3120 + D4203 + D4205

                AddTimeperiodsTotal(e.Row.Cells(CInt(raColumnID.Cost)), -999, sBlankSurplus, True) ' D3960 + D4004 + D4020 + D4128 + D4205

                For idx As Integer = 0 To e.Row.Cells.Count - CInt(raColumnID.CustomConstraintsStart)
                    If idx < .Constraints.Constraints.Keys.Count Then
                        Dim tID As Integer = .Constraints.Constraints.Keys(idx)
                        If .Constraints.Constraints.ContainsKey(tID) Then
                            Dim tConstr As RAConstraint = .Constraints.Constraints(tID)
                            Dim tMinVal As Double = tConstr.MinValue
                            Dim tMaxVal As Double = tConstr.MaxValue
                            Dim sMinVal As String = CStr(IIf(isExport, "-", ""))  ' D3540
                            Dim sMaxVal As String = sMinVal ' D3540
                            If tMinVal <> UNDEFINED_INTEGER_VALUE Then sMinVal = tMinVal.ToString("F2")
                            If tMaxVal <> UNDEFINED_INTEGER_VALUE Then sMaxVal = tMaxVal.ToString("F2")

                            Dim sStyle As String = CStr(IIf(tConstr.Enabled AndAlso Scenario.Settings.CustomConstraints, "", "font-style:italic; color:#808080;")) ' D3683
                            ' D6478 ===
                            Dim sBGColorMin As String = "#d1edff"
                            Dim sBGColorMax As String = "#d1edff"
                            If Scenario.GetInfeasibilityRemovedConstraint(ConstraintType.ctCustomConstraintMin, tID.ToString) IsNot Nothing Then sBGColorMin = "#faf19d"
                            If Scenario.GetInfeasibilityRemovedConstraint(ConstraintType.ctCustomConstraintMax, tID.ToString) IsNot Nothing Then sBGColorMax = "#faf19d"
                            Dim sMinEdit As String = String.Format("<input type='text' id='tbConstrMin{0}' class='input as_number' value='{1}' style='width:5em; background:{6};{5}' onfocus='onFocus(this.id, this.value);' onkeyup='onKeyUp(this.id, -1);' onblur='return onBlurCC(this.id, ""constraint_min"", ""{2}"", ""tbConstrMax{0}"", 0);'{3} TabIndex={4}>", idx, SafeFormString(sMinVal), tConstr.ID, IIf(tConstr.Enabled OrElse RA_OPT_CC_EDIT_DISABLED, "", " disabled=1"), (CInt(raColumnID.CustomConstraintsStart) + idx) + (GridAlternatives.Rows.Count + 1) * TAB_COEFF, sStyle, sBGColorMin) ' D3376 + D3423 + D3540 + D3803 + D4821
                            Dim sMaxEdit As String = String.Format("<input type='text' id='tbConstrMax{0}' class='input as_number' value='{1}' style='width:5em; background:{6};{5}' onfocus='onFocus(this.id, this.value);' onkeyup='onKeyUp(this.id, -1);' onblur='return onBlurCC(this.id, ""constraint_max"", ""{2}"", ""tbConstrMin{0}"", 1);'{3} TabIndex={4}>", idx, SafeFormString(sMaxVal), tConstr.ID, IIf(tConstr.Enabled OrElse RA_OPT_CC_EDIT_DISABLED, "", " disabled=1"), (CInt(raColumnID.CustomConstraintsStart) + idx) + (GridAlternatives.Rows.Count + 2) * TAB_COEFF, sStyle, sBGColorMax) ' D3376 + D3423 + D3540 + D3803 + D4821
                            ' D6478 ==

                            ' D3192 === 
                            If isExport Then
                                sMinEdit = sMinVal
                                sMaxEdit = sMaxVal
                            End If
                            ' D3192 ==

                            Dim tCell As TableCell = e.Row.Cells(CInt(raColumnID.CustomConstraintsStart) + idx)

                            Dim CFunded As Double = 0
                            If RA.Solver.SolverState = raSolverState.raSolved Then CFunded = RA.Solver.FundedConstraint(tConstr.ID) ' D2992
                            ' D3540 ===
                            Dim sSurplus As String = ""
                            ' D3683 ===
                            If fShowSurplus AndAlso (Not tConstr.Enabled OrElse Not Scenario.Settings.CustomConstraints) AndAlso (tConstr.MaxValueSet OrElse tConstr.MinValueSet) Then
                                Dim tSVal As Double = Integer.MinValue
                                ' D3800 ===
                                If Not (tConstr.MinValueSet AndAlso tConstr.MaxValueSet AndAlso CFunded >= tConstr.MinValue AndAlso CFunded <= tConstr.MaxValue) Then
                                    If tConstr.MinValueSet AndAlso (CFunded < tConstr.MinValue OrElse Not tConstr.MaxValueSet) Then tSVal = CFunded - tConstr.MinValue
                                    If tConstr.MaxValueSet AndAlso (CFunded > tConstr.MaxValue OrElse Not tConstr.MinValueSet) Then tSVal = tConstr.MaxValue - CFunded
                                    If tSVal <> Integer.MinValue Then
                                        sSurplus = (tSVal).ToString("F2")
                                        If Not isExport AndAlso tSVal < 0 Then sSurplus = String.Format("<span style='color:#cc3333'>{0}</span>", sSurplus)
                                    End If
                                End If
                                ' D3800 ==
                            End If
                            ' D3682 ==
                            If fShowSurplus Then sSurplus += CStr(IIf(isExport AndAlso sSurplus = "", "-", "")) + "<br>"
                            ' D6467 ===
                            Dim sFunded As String = If(CFunded >= 0, CFunded.ToString("F2"), "&nbsp;")
                            If CFunded > 0 AndAlso tConstr.isDollarValue Then sFunded = CostString(CFunded, Cents)
                            Dim sTotal As String = tConstr.TotalCost(Scenario.Alternatives).ToString("F2")
                            If tConstr.isDollarValue Then sTotal = CostString(tConstr.TotalCost(Scenario.Alternatives), Cents)
                            tCell.Text = String.Format("{0}<br>{1}<div class='as_number' style='width:5.3em; padding-right:3px;{4}'>{2}{5}{3}</div>", sMinEdit, sMaxEdit, IIf(RA.Solver.SolverState = raSolverState.raSolved, "<b>" + sFunded + "</b><br>", ""), sTotal, sStyle, sSurplus)   ' D2992  + D3884
                            ' D6467 ==
                            tCell.Enabled = tConstr.Enabled OrElse RA_OPT_CC_EDIT_DISABLED
                            ' D3540 ==
                            tCell.HorizontalAlign = If(isExport, HorizontalAlign.Right, HorizontalAlign.Center)    ' D3192
                            AddTimeperiodsTotal(tCell, idx, sBlankSurplus, tConstr.Enabled) ' D3960 + D4004 + D4020 + D4128
                        End If
                    End If
                Next

                ' D6628 ===
                For i As Integer = 0 To e.Row.Cells.Count - 1
                    Dim tCell As TableCell = e.Row.Cells(i)
                    If String.IsNullOrEmpty(tCell.ToolTip) AndAlso Not isExport Then
                        Dim sTitle As String = HTML2Text(If(String.IsNullOrEmpty(GridAlternatives.HeaderRow.Cells(i).ToolTip), GridAlternatives.HeaderRow.Cells(i).Text, GridAlternatives.HeaderRow.Cells(i).ToolTip))
                        tCell.ToolTip = SafeFormString(sTitle)
                    End If
                Next
                ' D6628 ==

            End With

            ' D2887 ==
        End If
        ' D2874 ==
    End Sub

    ' D4803 ===
    Private Function GetLastFundedPeriod(tAlt As RAAlternative) As String
        Dim tAlts As List(Of RAAlternative) = Scenario.Alternatives
        Dim resVal As String = ResString("lblRACanFunded")  ' D4808
        For Each alt As RAAlternative In tAlts
            Dim isFunded As Boolean = Scenario.Settings.TimePeriods AndAlso RA.Solver.SolverState = raSolverState.raSolved AndAlso alt.DisplayFunded > 0.0
            Dim AltTPData = Scenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
            Dim StartPeriod As Integer = AltTPData.MinPeriod   ' D3951
            If Scenario.TimePeriods.TimePeriodResults().ContainsKey(alt.ID.ToString) Then StartPeriod = Scenario.TimePeriods.TimePeriodResults(alt.ID.ToString)
            If isFunded And tAlt.ID = alt.ID Then
                resVal = Scenario.TimePeriods.GetPeriodName(StartPeriod + AltTPData.Duration - 1, True)
                Exit For
            End If
        Next
        Return resVal
    End Function
    ' D4803 ==

    Private Sub SetOptionsAll(chk As Boolean)
        If OPT_SHOW_AS_IGNORES Then chk = Not chk ' D2931
        With Scenario.Settings
            .CustomConstraints = chk
            .Dependencies = chk
            'If Not IgnoreFP() Then .FundingPools = chk Else .FundingPools = False ' D3570
            .FundingPools = chk ' D3649
            .Groups = chk
            .Musts = chk
            .MustNots = chk
            .Risks = chk
            .TimePeriods = chk  ' D3630
            .ResourcesMin = chk ' D4485
            .ResourcesMax = chk ' D4485
        End With
    End Sub

    ' D3114 ===
    Private Function CreateAssociatedModel(sName As String, tSrcPrjID As Integer) As clsProject  ' D3121
        Dim tPrj As clsProject = New clsProject(False, App.Options.ProjectForceAllowedAlts, App.ActiveUser.UserEmail, App.isRiskEnabled, , , App.Options.ProjectUseDataMapping)  ' D0183 + D0245 + D0315 + D2255 + D4465
        tPrj.ProviderType = App.DefaultProvider
        tPrj.isOnline = False
        tPrj.PasscodeLikelihood = App.ProjectUniquePasscode("", -1)
        tPrj.PasscodeImpact = App.ProjectUniquePasscode("", -1)
        tPrj.WorkgroupID = App.ActiveWorkgroup.ID
        tPrj.OwnerID = App.ActiveUser.UserID
        tPrj.ProjectName = sName
        tPrj.Comment = "Create as Associated Risk model for '" + App.ActiveProject.ProjectName + "'"
        tPrj.isRiskAssociatedModel = True   ' D3451 + D4978

        Dim tMasterPrj As clsProject = clsProject.ProjectByID(tSrcPrjID, App.ActiveProjectsList)
        If tSrcPrjID > 0 AndAlso tMasterPrj Is Nothing Then tMasterPrj = App.DBProjectByID(tSrcPrjID)

        Dim fCreated As Boolean = App.DBProjectCreate(tPrj, tPrj.Comment)
        If tMasterPrj IsNot Nothing AndAlso tMasterPrj.isValidDBVersion Then    ' D3672
            If App.DBProjectCopy(tMasterPrj, ECModelStorageType.mstCanvasStreamDatabase, tPrj.ConnectionString, tPrj.ProviderType, tPrj.ID, False) Then ' D3774
                tPrj.CheckGUID()    ' D3687
                tPrj.ResetProject()
                fCreated = True
            End If
        End If

        If fCreated Then
            ' D3167 ===
            Dim tAHPUser As clsUser = tPrj.ProjectManager.GetUserByEMail(App.ActiveUser.UserEmail)
            If tAHPUser Is Nothing Then
                tAHPUser = tPrj.ProjectManager.AddUser(App.ActiveUser.UserEmail, True, App.ActiveUser.UserName)
                tPrj.ProjectManager.StorageManager.Writer.SaveModelStructure()
            End If
            tPrj.ProjectManager.User = tAHPUser
            ' D3167 ==
            App.ImportProjectUsers(tPrj, App.ActiveUser, True, False)
            App.CheckAndAddReviewAccount(ReviewAccount, tPrj)
            tPrj.ProjectManager.PipeParameters.ProjectType = ProjectType.ptRiskAssociated   ' D3326

            For Each tAlt As clsNode In App.ActiveProject.HierarchyAlternatives.TerminalNodes
                Dim tNewAlt As New clsNode  ' D7076
                tNewAlt.NodeID = tAlt.NodeID
                tNewAlt.NodeGuidID = tAlt.NodeGuidID
                tNewAlt.NodeName = tAlt.NodeName
                tNewAlt.Comment = tAlt.Comment
                tNewAlt.InfoDoc = tAlt.InfoDoc
                tNewAlt.Enabled = tAlt.Enabled
                tNewAlt.RiskValue = tAlt.RiskValue
                tNewAlt.SOrder = tAlt.SOrder
                tPrj.HierarchyAlternatives.AddNode(tNewAlt, -1) ' D7076
            Next

            tPrj.HierarchyAlternatives.ResetNodesDictionaries()

            SetAssociatedModel(tPrj.ID, tPrj.ProjectGUID.ToString)
            tPrj.SaveStructure("Set Associated Risk model", False, True, tPrj.ProjectName)  ' D3731

            ' Copy alt attributes: get current project, remove all NON alt attribs, save to new model and reset ProjectManager for restore alt attribs list

            'For i As Integer = .AttributesList.Count - 1 To 0 Step -1
            '    Dim tAttr As clsAttribute = .AttributesList(i)
            '    If tAttr.Type = AttributeTypes.atAlternative Then
            '        .RemoveAttribute(tAttr.ID)
            '    End If
            'Next

            'Dim altAttrList As List(Of clsAttribute) = .GetAlternativesAttributes ' D3672
            'For Each tVal As clsAttributeValue In .ValuesList
            '    Dim tAttr As clsAttribute = .GetAttributeByID(tVal.AttributeID)
            '    If tAttr Is Nothing OrElse tAttr.Type = AttributeTypes.atAlternative Then
            '    End If
            'Next

            SyncAltAttribsAssociatedModel(tPrj) ' D4501

            App.ActiveProject.ResetProject(True)
            tPrj.ResetProject(True)
            ' D3672 ==

            App.Workspaces = Nothing
            App.ActiveProjectsList = Nothing
        Else
            tPrj = Nothing
        End If

        Return tPrj
    End Function

    Private Sub SetAssociatedModel(tID As Integer, sGUID As String)
        App.ActiveProject.PipeParameters.AssociatedModelIntID = tID
        App.ActiveProject.PipeParameters.AssociatedModelGuidID = sGUID
        App.ActiveProject.SaveProjectOptions(CStr(IIf(sGUID = Guid.Empty.ToString, "Reset Associated Risk model", "Set Associated Risk model")))
    End Sub
    ' D3114 ==

    ' D4501 ===
    Private Function SyncAltAttribsAssociatedModel(tPrj As clsProject) As Boolean
        ' D3672 ===
        Dim fHasChanges As Boolean = False
        For Each tAttr As clsAttribute In App.ActiveProject.ProjectManager.Attributes.GetAlternativesAttributes
            If tPrj.ProjectManager.Attributes.GetAttributeByID(tAttr.ID) Is Nothing Then
                ' copy categories if exists
                If Not Guid.Empty.Equals(tAttr.EnumID) Then
                    Dim tEnum As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(tAttr.EnumID)
                    If tEnum IsNot Nothing Then
                        Dim tNewEnum As New clsAttributeEnumeration()
                        tNewEnum.ID = tEnum.ID
                        tNewEnum.Name = tEnum.Name
                        For Each tEI As clsAttributeEnumerationItem In tEnum.Items
                            Dim tnewEI As New clsAttributeEnumerationItem()
                            tnewEI.ID = tEI.ID
                            tnewEI.Value = tEI.Value
                            tNewEnum.Items.Add(tnewEI)
                        Next
                        tPrj.ProjectManager.Attributes.Enumerations.Add(tNewEnum)
                        fHasChanges = True
                    End If
                End If
                ' create new attrib
                Dim tNewAttr As clsAttribute = tPrj.ProjectManager.Attributes.AddAttribute(tAttr.ID, App.GetAttributeName(tAttr), tAttr.Type, tAttr.ValueType, tAttr.DefaultValue, tAttr.IsDefault, tAttr.EnumID)
                ' copy attrib values 
                For Each tVal As clsAttributeValue In App.ActiveProject.ProjectManager.Attributes.ValuesList
                    If tVal.AttributeID.Equals(tAttr.ID) Then
                        tPrj.ProjectManager.Attributes.SetAttributeValue(tVal.AttributeID, tVal.UserID, tVal.ValueType, tVal.Value, tVal.ObjectID, tVal.AdditionalID)
                    End If
                Next
            End If
        Next

        If fHasChanges Then
            With tPrj.ProjectManager
                .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                ' no need to call due to already saved in WriteAttributes()
                '.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End If

        Return fHasChanges
    End Function

    Private Function SyncAltsAssociatedModel() As Boolean
        Dim fHasChanges As Boolean = False
        If App.ActiveProject.PipeParameters.AssociatedModelIntID > 0 Then
            Dim tARMPrj As clsProject = App.DBProjectByID(App.ActiveProject.PipeParameters.AssociatedModelIntID)
            If tARMPrj IsNot Nothing AndAlso (tARMPrj.isDBVersionCanBeUpdated OrElse tARMPrj.isValidDBVersion) Then
                For Each tAlt As clsNode In App.ActiveProject.HierarchyAlternatives.Nodes
                    Dim tARMNode As clsNode = tARMPrj.HierarchyAlternatives.GetNodeByID(tAlt.NodeGuidID)
                    If tARMNode Is Nothing Then
                        ' Add missing alt
                        tARMNode = New clsNode  ' D7076
                        tARMNode.NodeID = tAlt.NodeID
                        tARMNode.NodeGuidID = tAlt.NodeGuidID
                        tARMPrj.HierarchyAlternatives.AddNode(tARMNode, -1) ' D7076
                    End If
                    ' check for changes
                    If tARMNode.NodeName <> tAlt.NodeName OrElse tARMNode.Comment <> tAlt.Comment OrElse tARMNode.InfoDoc <> tAlt.InfoDoc OrElse
                        tARMNode.Enabled <> tAlt.Enabled OrElse tARMNode.RiskValue <> tAlt.RiskValue OrElse tARMNode.SOrder <> tAlt.SOrder Then
                        tARMNode.Enabled = tAlt.Enabled
                        tARMNode.NodeName = tAlt.NodeName
                        tARMNode.Comment = tAlt.Comment
                        tARMNode.InfoDoc = tAlt.InfoDoc
                        tARMNode.RiskValue = tAlt.RiskValue
                        tARMNode.SOrder = tAlt.SOrder
                        fHasChanges = True
                    End If
                Next
                tARMPrj.HierarchyAlternatives.ResetNodesDictionaries()

                For i = tARMPrj.HierarchyAlternatives.Nodes.Count - 1 To 0 Step -1
                    Dim tARMNode As clsNode = tARMPrj.HierarchyAlternatives.Nodes(i)
                    If App.ActiveProject.HierarchyAlternatives.GetNodeByID(tARMNode.NodeGuidID) Is Nothing Then
                        tARMPrj.HierarchyAlternatives.DeleteNode(tARMNode, False)
                        fHasChanges = True
                    End If
                Next
                tARMPrj.HierarchyAlternatives.ResetNodesDictionaries()

                If SyncAltAttribsAssociatedModel(tARMPrj) Then fHasChanges = True
                If fHasChanges Then tARMPrj.SaveStructure(ParseString("Synchronize %%Alternatives%% in Associated Model"), True, True, String.Format("Imported from model '{0}'", App.ActiveProject.ProjectName))

            End If
        End If

        Return fHasChanges
    End Function
    ' D4501 ==

    ' D4909 ===
    Public Sub CopyScenario(SrcID As Integer, DestID As Integer)
        Dim BaseScenario As RAScenario = RA.Scenarios.Scenarios(SrcID)
        Dim DestScenario As RAScenario = RA.Scenarios.Scenarios(DestID) ' D6724
        With DestScenario   ' D6724
            .Budget = BaseScenario.Budget
            ' D3840 ===
            If BaseScenario.AlternativesFull.Count > 0 Then
                .AlternativesFull.Clear()
                For Each alt As RAAlternative In BaseScenario.AlternativesFull
                    .AlternativesFull.Add(alt.Clone)
                Next
            End If
            ' D3840 ==
            ' D6724 ===
            .Description = BaseScenario.Description
            .Constraints = BaseScenario.Constraints.Clone(DestScenario)
            .EventGroups = BaseScenario.EventGroups.Clone()
            .Dependencies = BaseScenario.Dependencies.Clone()
            .FundingPools = BaseScenario.FundingPools.Clone()
            .Groups = BaseScenario.Groups.Clone()
            .MaxRisk = BaseScenario.MaxRisk
            .MinReduction = BaseScenario.MinReduction
            .OptimizationType = BaseScenario.OptimizationType
            .OptimizedRiskValue = BaseScenario.OptimizedRiskValue
            .OriginalAllControlsCost = BaseScenario.OriginalAllControlsCost
            .OriginalRiskValue = BaseScenario.OriginalRiskValue
            .OriginalRiskValueWithControls = BaseScenario.OriginalRiskValueWithControls
            .OriginalSelectedControlsCost = BaseScenario.OriginalSelectedControlsCost
            .OriginalSelectedControlsCount = BaseScenario.OriginalSelectedControlsCount
            .SolverPriorities = BaseScenario.SolverPriorities.Clone(DestScenario)
            .TimePeriodsDependencies = BaseScenario.TimePeriodsDependencies.Clone()
            ' D6724 ==
            .Settings = BaseScenario.Settings.Clone
            .TimePeriods = BaseScenario.TimePeriods.Clone(RA.Scenarios.Scenarios(DestID))   ' D3840
            .CombinedGroupUserID = BaseScenario.CombinedGroupUserID     ' D3737
            RA.Scenarios.CheckAndSortScenarios()    ' D3213
            SaveMsg = "Update Scenarios"    ' D3757
            SaveComment = String.Format("Copy '{0}' to '{1}'", BaseScenario.Name, .Name)    ' D3757
            SaveRA = True   ' D3159
        End With
    End Sub
    ' D4909 ==

    ' D4912 ===
    Public Function GetRemovedConstraints(Scenario As RAScenario) As String
        Dim sResult As String = ""
        If Scenario.IsInfeasibilityAnalysis AndAlso Scenario.InfeasibilityRemovedConstraints IsNot Nothing Then
            For Each tInfo As ConstraintInfo In Scenario.InfeasibilityRemovedConstraints
                sResult += String.Format("<li class='ra_infeas_item'>{0}<div class='small ra_infeas_item'>{1}</div></li>", tInfo.Name, tInfo.Description)
            Next
            If sResult <> "" Then
                sResult = "<b>Removed constraint" + If(Scenario.InfeasibilityRemovedConstraints.Count > 1, "s", "") + "</b>:<ul type=square>" + sResult + "</ul>"   ' D7086
                If Scenario.InfeasibilityRemovedConstraints.Count > 10 Then sResult = String.Format("<div style='height:12em; overflow-y:auto'>{0}</div>", sResult)
            End If
        End If
        Return sResult
    End Function
    ' D4912 ==

    ' D6475 ===

    ' -D6476 
    'Public Function GetSolutionsRemovedConstraints() As String
    '    Dim sResult As String = ""
    '    For Each ID As Integer In RA.Scenarios.Scenarios.Keys
    '        Dim tScen As RAScenario = RA.Scenarios.Scenarios(ID)
    '        If tScen.IsInfeasibilityAnalysis AndAlso Scenario.InfeasibilityRemovedConstraints IsNot Nothing Then
    '            sResult += String.Format("<div id='divRemovedConstr{0}' style='display:none'>{1}</div>", ID, GetRemovedConstraints(tScen))
    '        End If
    '    Next
    '    Return sResult
    'End Function

    ' D6477 ===
    Public Function InfeasibilitySolutionsCount() As Integer
        Dim cnt As Integer = 0
        For Each ID As Integer In RA.Scenarios.Scenarios.Keys
            Dim tScen As RAScenario = RA.Scenarios.Scenarios(ID)
            If tScen.IsInfeasibilityAnalysis AndAlso Scenario.InfeasibilityRemovedConstraints IsNot Nothing Then cnt += 1
        Next
        Return cnt
    End Function
    ' D6477 ==

    Public Function GetSolutions() As String
        Dim sResult As String = ""
        ' D6476 ===
        Dim ScenList As New List(Of RAScenario)
        For Each ID As Integer In RA.Scenarios.Scenarios.Keys
            Dim tScen As RAScenario = RA.Scenarios.Scenarios(ID)
            If tScen.IsInfeasibilityAnalysis AndAlso Scenario.InfeasibilityRemovedConstraints IsNot Nothing Then ScenList.Add(tScen)
        Next
        'ScenList.Sort(New RAScenarios_Comparer(raScenarioField.InfeasOptimalValue, True))
        For Each tScen As RAScenario In ScenList
            sResult += String.Format("<tr align=right class='tbl_row{4}'><td align=center>{0}</td><td>{1}</td><td><a href='' onclick='showRemovedConstr({3}); return false;' class='actions dashed' title='{5}' id='titleRemovedConstr{3}'>{2}</a>&nbsp;</td></tr>", String.Format(If(RA.Scenarios.ActiveScenarioID = tScen.ID, "<b>{0}</b>", "<a href='' onclick='onSetScenario({1}); return false;'>{0}</a>"), tScen.Name, tScen.ID), tScen.InfeasibilityOptimalValue.ToString("F4"), If(tScen.InfeasibilityRemovedConstraints IsNot Nothing, tScen.InfeasibilityRemovedConstraints.Count, 0), tScen.ID, If(tScen.InfeasibilityScenarioIndex Mod 1 = 0, "_alt", ""), GetRemovedConstraints(tScen).Replace("'", "&#39;"))  ' D6476
        Next
        ' D6476 ==
        If sResult = "" Then
            sResult = String.Format(" <p align=center>{0}</p>", ResString("msgRANoInfeasSolutions"))    ' D7052
        Else
            sResult = String.Format("<table class='tbl' cellpadding=2><tr class='tbl_hdr'><td>{0}</td><td><nobr>{1}</nobr></td><td>{2}</td></tr>{3}</table>", ResString("lblRAScenarios"), ResString("lblEBenefit"), ResString("lblRemoved"), sResult)       ' D6476 + D7052
        End If
        Return sResult
    End Function
    ' D6475 ==

    Protected Sub radAjaxManagerMain_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxManagerMain.AjaxRequest
        ProcessAjaxRequest(HttpUtility.ParseQueryString(e.Argument))
    End Sub

    Private Sub ProcessAjaxRequest(args As NameValueCollection)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower   'Anti-XSS
        Dim fResetSolver As Boolean = False  ' D2874

        Select Case sAction

            ' D2840 ===
            Case "solve"
                isSolving = True
                GridAlternatives.DataSource = Nothing
                fResetSolver = False    ' D2874
                ' D2840 ==

                ' D4681 ===
            Case "unsolve"
                RA.Solver.ResetSolver()
                'RA.Solver.ResetFunded()
                fResetSolver = True
                isSolving = False
                isReset = True
                ' D4681 ==

            Case "musts", "mustnot", "partial", "istolerance"   ' D4930
                Dim sGUID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid"))   'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                ' D4199 ===
                Dim sGUIDsList As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guids_lst")).Trim
                Dim tLst As New List(Of String)
                If sGUIDsList <> "" Then tLst = sGUIDsList.Split(CChar("_")).ToList
                If tLst.Count = 1 AndAlso tLst(0) = "" Then tLst.Clear()
                If tLst.Count = 0 Then tLst.Add(sGUID)
                If tLst.Count > 0 AndAlso sVal <> "" Then
                    Dim sName As String = ""
                    For Each sID As String In tLst
                        Dim fChk As Boolean = sVal = "1"
                        sID = sID.Trim
                        If sID <> "" AndAlso sID.Length >= Guid.Empty.ToString.Length Then  ' D4201
                            Dim aGUID As New Guid(sID.Trim(CChar("!")))    ' D4201
                            If sID.StartsWith("!") Then fChk = Not fChk ' D4201
                            Dim tAlt As RAAlternative = Scenario.Alternatives.Find(Function(x) x.ID = aGUID.ToString)
                            If tAlt IsNot Nothing Then
                                Select Case sAction
                                    Case "musts"
                                        tAlt.Must = fChk
                                        If fChk Then
                                            tAlt.MustNot = False
                                        End If
                                    Case "mustnot"
                                        tAlt.MustNot = fChk
                                        If fChk Then
                                            tAlt.Must = False
                                        End If
                                    Case "partial"
                                        tAlt.IsPartial = fChk
                                    Case "istolerance"  ' D4930
                                        tAlt.AllowCostTolerance = fChk  ' D4930
                                End Select
                                If Not sID.StartsWith("!") Then sName += String.Format("{0}{1}", IIf(sName = "", "", ", "), ShortString(tAlt.Name, If(tLst.Count > 1, 30, 45), True)) ' D4201
                            End If
                        End If
                    Next
                    SaveMsg = String.Format("Update {0}", sAction)  ' D3578 + D3757
                    SaveComment = String.Format("Set '{1}' for {0}", sName, Bool2YesNo(sVal = "1"))  ' D3578 + D3757
                    ' D4199 ==
                    fResetSolver = True    ' D4201 + D4894
                    SaveRA = True   ' D2909
                End If

            Case "option"   ' Solver options
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name"))   'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                Dim tVal As Integer = -1    ' D2931
                If sName <> "" AndAlso Integer.TryParse(sVal, tVal) Then
                    Dim chk As Boolean = tVal <> 0   ' D2931
                    If OPT_SHOW_AS_IGNORES Then chk = Not chk ' D2931
                    fResetSolver = True ' D3174
                    With Scenario.Settings
                        Select Case sName
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
                            Case "cbOptTimePeriods" ' D3630
                                .TimePeriods = chk  ' D3630
                                ' D3078 ===
                                ' D4485 ===
                            Case "cbOptTimePeriodMins"
                                .ResourcesMin = chk
                            Case "cbOptTimePeriodMaxs"
                                .ResourcesMax = chk
                                ' D4485 ==
                            Case "cbBaseCase"
                                .UseBaseCase = CBool(IIf(OPT_SHOW_AS_IGNORES, Not chk, chk))
                            Case "cbBCGroups"
                                .BaseCaseForGroups = CBool(IIf(OPT_SHOW_AS_IGNORES, Not chk, chk))
                                ' D3078 ==
                        End Select
                        'If RA_AutoSolve OrElse isSolving Then RA.UpdateBenefits() ' D2850 + D2854 + D3159
                        RA.UpdateBenefits() ' D2850 + D2854
                        SaveMsg = "Change ignore option"    ' D3578 + D3757
                        SaveComment = String.Format("'{0}': {1}", sName, Bool2YesNo(chk))    ' D3557
                        SaveRA = True   ' D2909
                    End With
                End If

            Case "opt_all"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                If sVal <> "" Then
                    SetOptionsAll(sVal = "1")  ' D2839
                    SaveMsg = "Change ignore option"    ' D3578 + D3757
                    SaveComment = String.Format(If(sVal = "1", "Ignore all", "Use all")) ' D3557
                    SaveRA = True   ' D2909
                    fResetSolver = True ' D4121
                End If

            Case "cost", "minprc", "pfailure", "probsuccess", "tolerance"   ' D2843 + D4349 + D4930
                Dim sGUID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid"))   'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).Trim    'Anti-XSS
                If sVal = "" Then sVal = UNDEFINED_INTEGER_VALUE.ToString ' D3224
                Dim tVal As Double = -1
                If sGUID <> "" AndAlso String2Double(sVal, tVal) AndAlso (tVal = CDbl(UNDEFINED_INTEGER_VALUE) OrElse tVal >= 0 OrElse sAction = "cost") Then  ' D2837 + D3224
                    Dim tAlt As RAAlternative = Scenario.Alternatives.Find(Function(x) x.ID = sGUID)
                    If tAlt IsNot Nothing Then
                        ' D2843 ===
                        If sAction = "cost" Then
                            'If tAlt.Cost >= 0 Then tAlt.Cost = tVal
                            RA.SetAlternativeCost(tAlt.ID, tVal)
                            'tAlt.Cost = tVal
                            If isTimeperiodsVisible() Then Scenario.TimePeriods.AllocateResourceValues() ' D3938 
                        Else
                            If tVal < 0 AndAlso (tVal <> UNDEFINED_INTEGER_VALUE OrElse sAction = "minprc" OrElse sAction = "tolerance") Then tVal = 0 ' D3224 + D4930
                            If tVal > 1 Then tVal = 1
                            ' D4930 ===
                            Select Case sAction
                                Case "minprc"
                                    tAlt.MinPercent = tVal
                                Case "tolerance"
                                    tAlt.CostTolerance = tVal
                                Case "pfailure"    ' D4349
                                    tAlt.RiskOriginal = tVal
                                    RA.SetAlternativeRisk(tAlt.ID, tAlt.RiskOriginal)   ' D3027
                                Case "probsuccess"
                                    tAlt.RiskOriginal = 1 - tVal
                                    RA.SetAlternativeRisk(tAlt.ID, tAlt.RiskOriginal)   ' D3811
                            End Select
                            ' D4930 ==
                        End If
                        ' D2843 ==
                        SaveMsg = String.Format("Update {0}", sAction)  'D3757
                        SaveComment = String.Format("'{0}': {1}", ShortString(tAlt.Name, 40, True), IIf(tVal = UNDEFINED_INTEGER_VALUE, "undef", tVal)) ' D3757
                        SaveRA = True   ' D2909
                        fResetSolver = True ' D4121
                    End If
                End If

                ' D2744 ===
            Case "budget"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                String2Double(sVal, Scenario.Budget) ' D2837
                SaveMsg = "Edit budget" ' D3757
                SaveComment = CostString(Scenario.Budget, Cents)    ' D3757
                SaveRA = True   ' D2909
                fResetSolver = True ' D4121
                ' D2744 ==

                ' D2840 ===
            Case "autosolve"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                RA_AutoSolve = (sVal = "1")
                isSolving = RA_AutoSolve    ' D4173
                fResetSolver = False
                App.ActiveProject.ProjectManager.Parameters.Save()  ' D4681
                SaveRA = True   ' D7582
                ' D2840 ==

                ' D2876 ===
            Case "edit_scenario"    ' using for add new as well (ID=-1)
                Dim sID As String = GetParam(args, "id")
                Dim sName As String = GetParam(args, "name").Trim
                Dim sDesc As String = GetParam(args, "desc").Trim
                Dim sGrpID As String = GetParam(args, "grp")    ' D3350
                Dim ID As Integer
                Dim GrpID As Integer    ' D3350
                If Integer.TryParse(sID, ID) AndAlso sName <> "" Then
                    Dim RAScenario As RAScenario = Nothing
                    If ID < 0 Then
                        ' D3123 ===
                        Dim fCopy As Boolean = GetParam(args, "copy") = "1"
                        RAScenario = RA.Scenarios.AddScenario(If(fCopy, RA.Scenarios.ActiveScenarioID, -1)) ' D3098
                        'If fCopy Then CopyScenario(Scenario, RAScenario)
                        ' D3123 ==
                    Else
                        If RA.Scenarios.Scenarios.ContainsKey(ID) Then RAScenario = RA.Scenarios.Scenarios(ID)
                    End If
                    If RAScenario IsNot Nothing Then
                        RAScenario.Name = sName
                        RAScenario.Description = sDesc
                        If Not Integer.TryParse(sGrpID, GrpID) Then GrpID = -1 ' D3350
                        RAScenario.CombinedGroupUserID = GrpID  ' D3350 + D3737
                        fResetSolver = False
                        SaveMsg = "Update Scenarios"    ' D3757
                        SaveComment = String.Format(CStr(IIf(ID < 0, "Add", "Edit")) + " '{0}'", sName)  ' D3557
                        SaveRA = True   ' D2909
                        ' D4208 ===
                        If GetParam(args, "mode").ToLower = "specific_portfolio" Then
                            RAScenario.CombinedGroupUserID = Scenario.CombinedGroupUserID
                            If RA.Solver.SolverState = raSolverState.raSolved Then
                                Dim tMusts As Boolean = GetParam(args, "m") = "1"
                                Dim tMustNot As Boolean = GetParam(args, "mn") = "1"
                                For Each tAlt As RAAlternative In RAScenario.AlternativesFull
                                    Dim tOrigAlt As RAAlternative = Scenario.Alternatives.Find(Function(x) x.ID = tAlt.ID)
                                    If tOrigAlt IsNot Nothing Then
                                        If tMusts AndAlso tOrigAlt.DisplayFunded > 0 Then tAlt.Must = True
                                        If tMustNot AndAlso tOrigAlt.DisplayFunded = 0 Then tAlt.MustNot = True
                                    End If
                                Next
                            End If
                            SaveMsg = "Create Specific Portfolio"
                            SaveComment = String.Format("Add Scenario '{0}' (based on '{1}' solve)", sName, Scenario.Name)
                        End If
                        ' D4208 ==
                    End If
                End If

                ' D3015 ===
            Case "copy_scenario"
                Dim sSrc As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "from"))    'Anti-XSS
                Dim sDest As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "to")) 'Anti-XSS
                Dim SrcID As Integer
                Dim DestID As Integer
                If Integer.TryParse(sSrc, SrcID) AndAlso Integer.TryParse(sDest, DestID) Then
                    If SrcID <> DestID AndAlso SrcID = RA.Scenarios.ActiveScenarioID AndAlso DestID >= 0 AndAlso DestID < RA.Scenarios.Scenarios.Count Then
                        CopyScenario(SrcID, DestID) ' D4909
                        fResetSolver = False    ' D4121
                    End If
                End If
                ' D3015 ==

            Case "delete_scenario"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id"))   'Anti-XSS
                Dim ID As Integer
                If Integer.TryParse(sID, ID) AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) AndAlso ID > 0 AndAlso RA.Scenarios.Scenarios.Count > 1 Then  ' D3215
                    SaveMsg = "Update Scenarios"
                    SaveComment = String.Format("Delete '{0}'", RA.Scenarios.Scenarios(ID).Name)   ' D3757
                    If ID = RA.Scenarios.ActiveScenarioID Then RA.Scenarios.ActiveScenarioID = CType(RA.Scenarios.Scenarios.ElementAt(0).Value, RAScenario).ID ' D3215
                    'App.ActiveProject.ProjectManager.Parameters.Parameters.DeleteParameter(String.Format("{0}{1}", clsProjectParametersWithDefaults.RA_TIMEPERIODS_HAS_DATA, IIf(ID > 0, ID, "")))  ' D3841 -D3943
                    RA.Scenarios.DeleteScenario(ID)
                    RA.Scenarios.CheckAndSortScenarios()    ' D3213
                    fResetSolver = False
                    SaveRA = True   ' D2909
                End If
                ' D2876 ==

                ' D2886 ===
            Case "constraint"
                Dim sAlt As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid"))    'Anti-XSS
                Dim sCID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "cid")) 'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).Trim    'Anti-XSS
                If sVal = "" Then sVal = UNDEFINED_INTEGER_VALUE.ToString ' D3224
                Dim tVal As Double = -1
                Dim tCID As Integer = -1
                If sAlt <> "" AndAlso sCID <> "" AndAlso String2Double(sVal, tVal) AndAlso Integer.TryParse(sCID, tCID) Then
                    Dim tAlt As RAAlternative = Scenario.Alternatives.Find(Function(x) x.ID = sAlt)
                    Dim tConstr As RAConstraint = Scenario.Constraints.GetConstraintByID(tCID)
                    If tAlt IsNot Nothing AndAlso tConstr IsNot Nothing Then
                        Scenario.Constraints.SetConstraintValue(tCID, sAlt, tVal)
                        If isTimeperiodsVisible() AndAlso OPT_SHOW_TP_CC Then Scenario.TimePeriods.AllocateResourceValues() ' D3938 
                        SaveMsg = "Update constraint"   ' D3757
                        SaveComment = String.Format("'{0}'/'{2}': {1}", ShortString(tConstr.Name, 40, True), IIf(tVal = UNDEFINED_INTEGER_VALUE, "undef", tVal), ShortString(tAlt.Name, 40, True))    ' D3578 + D3757 + D3798
                        SaveRA = True   ' D2909
                        fResetSolver = True ' D4121
                    End If
                End If
                ' D2886 ==

                ' D2887 ===
            Case "constraint_min", "constraint_max"
                Dim sCID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid"))    'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                Dim tCID As Integer = -1
                If sCID <> "" AndAlso Integer.TryParse(sCID, tCID) Then
                    Dim tConstr As RAConstraint = Scenario.Constraints.GetConstraintByID(tCID)
                    If tConstr IsNot Nothing Then
                        Dim tVal As Double = -1
                        If Not String2Double(sVal, tVal) Then tVal = UNDEFINED_INTEGER_VALUE
                        If sAction = "constraint_max" Then
                            tConstr.MaxValue = tVal
                        Else
                            tConstr.MinValue = tVal
                        End If
                        SaveMsg = "Update constraint"   ' D3757
                        SaveComment = String.Format("'{0}' {1}: {2}", tConstr.Name, IIf(sAction = "constraint_max", "max", "min"), IIf(tVal = UNDEFINED_INTEGER_VALUE OrElse tVal = Integer.MinValue, "undef", CostString(tVal, Cents)))  ' D3578 + D3757 + D3798
                        SaveRA = True   ' D2909
                        fResetSolver = True ' D4121
                    End If
                End If
                ' D2887 ==

                ' D2880 ===
            Case "settings"
                fResetSolver = False
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim.ToLower  'Anti-XSS
                Dim sChecked As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "chk")) 'Anti-XSS
                Dim fVal As Boolean = sChecked = "1"
                Select Case sName
                    Case "showbars"
                        RA_ShowTinyBars = fVal
                    Case "showconstraints"
                        RA.Scenarios.GlobalSettings.ShowCustomConstraints = fVal    ' D3781
                        SaveRA = True   ' D4380
                        fResetSolver = True
                    Case "showattributes" 'A1010
                        ShowAltAttributes = fVal 'A1010
                    Case "showfunded"
                        RA_ShowFundedOnly = fVal
                    Case "hideignored"   ' D3013
                        RA_HideIgnored = fVal   ' D3013
                    Case "hidenorisks"   ' D3017
                        RA_HideNoRisks = fVal   ' D3017
                    Case "showchanges"   ' D3014
                        RA_ShowChanges = fVal   ' D3014
                    Case "showsurplus"   ' D3540
                        If Not OPT_HIDE_CALCULATE_SOFT_CONSTRAINTS Then RA_ShowSurplus = fVal ' D3540 + D3678
                    Case "fixedwidth"
                        RA_FixedColsWidth = fVal
                        'Case "minmaxextra"  ' D2887 -D2897
                        '    RA_ShowMinMaxExtra = fVal   ' D2887 -D2897
                    Case "scrolling"    ' D2918
                        RA.Scenarios.GlobalSettings.ShowFrozenHeaders = fVal ' D2918 + D3185
                        SaveRA = True   ' D4380
                    Case "showcents"    ' D4499
                        RA_ShowCents = fVal ' D4499
                End Select
                App.ActiveProject.ProjectManager.Parameters.Save()  ' D4681
                ' D2880 ==

                ' D2982 ===
            Case "sorting"
                fResetSolver = False
                Dim sFld As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "fld")) 'Anti-XSS
                Dim sDir As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "dir")) 'Anti-XSS
                Dim tFld As Integer
                Dim tDir As Integer
                If Integer.TryParse(sFld, tFld) AndAlso Integer.TryParse(sDir, tDir) Then
                    ' D4415 ===
                    Dim OldSort As Integer = Math.Abs(RA.Scenarios.GlobalSettings.SortBy)
                    RA.Scenarios.GlobalSettings.SortBy = CType(tFld, raColumnID)
                    If OldSort <> RA.Scenarios.GlobalSettings.SortBy Then
                        '-A2001 Select Case RA.Scenarios.GlobalSettings.SortBy
                        '    Case raColumnID.isFunded, raColumnID.isPartial, raColumnID.MustNot, raColumnID.Musts, raColumnID.isCostTolerance ' D4930
                        '        tDir = SortDirection.Descending
                        'End Select
                        'A2001 ===
                        Select Case RA.Scenarios.GlobalSettings.SortBy
                            Case raColumnID.ID, raColumnID.Name
                                tDir = SortDirection.Ascending
                            Case Else
                                tDir = SortDirection.Descending
                        End Select
                        'A2001 ==
                    End If
                    ' D4415 ==
                    If tDir = SortDirection.Descending Then RA.Scenarios.GlobalSettings.SortBy = CType(-RA.Scenarios.GlobalSettings.SortBy, raColumnID)
                    ' D3223 ===
                    If tFld = raColumnID.ID Then Session(SESS_RA_SORT_EXPR) = 0
                    If tFld = raColumnID.Name Then Session(SESS_RA_SORT_EXPR) = 3
                    If tFld = raColumnID.ID OrElse tFld = raColumnID.Name Then
                        Session(SESS_RA_SORT_DIR) = IIf(tDir = SortDirection.Descending, "DESC", "ASC")
                    End If
                    ' D3223 ==
                    SaveRA = True   ' 4862
                    isSorting = True
                    fResetSolver = False    ' D4121
                End If
                ' D2982 ==

                ' D2893 ===
            Case "column"
                fResetSolver = False
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")).Trim.ToLower  'Anti-XSS
                Dim sChecked As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "chk")) 'Anti-XSS
                Dim fVal As Boolean = sChecked = "1"
                Dim ID As Integer
                If Integer.TryParse(sID, ID) Then
                    Dim Cols() As Integer = RA_Columns  ' D3781
                    Dim idx As Integer = Array.IndexOf(Cols, ID)
                    If idx < 0 Then idx = Array.IndexOf(Cols, -ID)
                    If idx >= 0 Then
                        Cols(idx) = CInt(IIf(fVal, ID, -ID))  ' D3781
                        RA_Columns = Cols   ' D3781
                        SaveRA = True
                        SaveMsg = ""
                        SaveComment = ""
                        fResetSolver = False    ' D4121
                    End If
                End If
                ' D2893 ==

                ' D3114 ===
            Case "select_risk"
                fResetSolver = False
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "model")).Trim.ToLower   'Anti-XSS
                Dim ID As Integer
                If Integer.TryParse(sID, ID) Then
                    Dim tPrj As clsProject = clsProject.ProjectByID(ID, App.ActiveProjectsList)
                    If tPrj IsNot Nothing AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted Then
                        SetAssociatedModel(ID, tPrj.ProjectGUID.ToString)
                    End If
                End If

            Case "unset_risk"
                fResetSolver = False
                SetAssociatedModel(-1, "")
                ' D3114 ==

                ' D4501 ===
            Case "sync_alts_risk"
                If App.ActiveProject.PipeParameters.AssociatedModelIntID > 0 Then
                    If SyncAltsAssociatedModel() Then fResetSolver = True ' D4894
                End If
                ' D4501 ==

                ' D3124 ===
            Case "import_risk"
                If App.ActiveProject.PipeParameters.AssociatedModelIntID > 0 Then
                    Dim sError As String = ""
                    ' D3145 ===
                    Dim fToAll As Boolean = GetParam(args, "to_all").Trim = "1"
                    Dim fResult As Boolean = True
                    Dim ASID As Integer = RA.Scenarios.ActiveScenarioID
                    Dim asRisk As Boolean = GetParam(args, "asrisk").Trim = "1"    ' D3759

                    ' D3650 ===
                    Dim IDs As New List(Of Integer)
                    For Each tSID As Integer In RA.Scenarios.Scenarios.Keys
                        If fToAll OrElse tSID = ASID Then IDs.Add(tSID)
                    Next
                    For i As Integer = 0 To IDs.Count - 1 Step 1
                        Dim tSID As Integer = IDs(i)
                        ' D3650 ==
                        RA.Scenarios.ActiveScenarioID = tSID
                        If Not App.ActiveProject.ProjectManager.ImportRisksFromModel(App.ActiveProject.PipeParameters.AssociatedModelIntID, asRisk, sError) Then fResult = False ' D3758
                    Next
                    ' D3145 ==
                    If fResult Then App.ActiveProject.ProjectManager.ResourceAligner.Load(PM.StorageManager.StorageType, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, App.ProjectID)    ' D4857
                    RA.Scenarios.ActiveScenarioID = ASID
                    If HasRisks() Then
                        ' D3781 ===
                        Dim Cols() As Integer = RA_Columns
                        Dim idx As Integer = Array.IndexOf(Cols, -CInt(raColumnID.ProbSuccess))
                        If (idx >= 0) Then Cols(idx) = CInt(raColumnID.ProbSuccess)
                        idx = Array.IndexOf(Cols, -CInt(raColumnID.ProbFailure))
                        If (idx >= 0) Then Cols(idx) = CInt(raColumnID.ProbFailure)
                        idx = Array.IndexOf(Cols, -CInt(raColumnID.Risk))   ' D4348
                        If (idx >= 0) Then Cols(idx) = CInt(raColumnID.Risk) ' D4348
                        RA_Columns = Cols
                        ' D3781 ==
                        Scenario.Settings.Risks = True   ' D3178
                    End If
                    'sResult = String.Format("[{0},'{1}']", IIf(fResult, 1, 0), JS_SafeString(sError))
                    If fResult Then
                        RA.Scenarios.CheckModel(True)   ' D4694
                        SaveRA = True
                        SaveMsg = "Edit Risks"
                        SaveComment = String.Format("Import {1} to {0}", IIf(fToAll, "All scenarios", String.Format("Scenario '{0}'", Scenario.Name)), IIf(asRisk, "Risks", "Prob. of Success"))
                        fResetSolver = True ' D4121
                    End If
                End If
                ' D3124 ==

                ' D3176 ===
            Case "delete_risks"
                With Scenario
                    For Each tAlt As RAAlternative In .Alternatives
                        tAlt.RiskOriginal = 0
                        RA.SetAlternativeRisk(tAlt.ID, tAlt.RiskOriginal)   ' D3674
                    Next
                End With
                App.ActiveProject.SaveRA("RA: Edit Risks", True, True, "Delete Risks")  ' D3757
                fResetSolver = True ' D4121
                ' D3176 ==

                ' D3777 ===
            Case "sel_alts"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ids")).Trim 'Anti-XSS
                If sID <> "" Then
                    Dim Lst As String() = sID.Split(CChar(","))
                    If Lst.Count > 0 Then
                        Dim sAddLst As String = ""
                        Dim sDelLst As String = ""
                        For Each tAlt As RAAlternative In Scenario.AlternativesFull
                            If Lst.Contains(tAlt.ID) Then
                                If Not tAlt.Enabled Then
                                    tAlt.Enabled = True
                                    sAddLst += String.Format("{0}'{1}'", IIf(sAddLst = "", "", ","), ShortString(tAlt.Name, 40, True))
                                End If
                            Else
                                If tAlt.Enabled Then
                                    tAlt.Enabled = False
                                    sDelLst += String.Format("{0}'{1}'", IIf(sDelLst = "", "", ","), ShortString(tAlt.Name, 40, True))
                                End If
                            End If
                        Next
                        If sAddLst <> "" OrElse sDelLst <> "" Then
                            SaveRA = True
                            SaveMsg = "Select alternatives"   ' D3757
                            If sAddLst <> "" Then SaveComment = "Added: " + sAddLst
                            If sDelLst <> "" Then SaveComment += CStr(IIf(SaveComment = "", "", "; ")) + "Removed: " + sDelLst
                            fResetSolver = True ' D4121
                        End If
                    End If
                End If
                ' D3777 ==

                ' D3343 ===
            Case "sync_constr"
                ' D3346 ===
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ids")).Trim 'Anti-XSS
                Dim Lst As New List(Of RAConstraint)
                If sID <> "" Then
                    Dim IDs As String() = sID.Split(CChar(","))
                    For Each tmpID As String In IDs
                        Dim ID As Integer
                        If Integer.TryParse(tmpID, ID) Then
                            If Scenario.Constraints.Constraints.ContainsKey(ID) Then
                                Lst.Add(Scenario.Constraints.Constraints(ID))
                                ' D3346 ==
                            End If
                        End If
                    Next
                    If Lst.Count > 0 Then If RA.Scenarios.SyncLinkedConstraintsValues(Lst) Then SaveRA = True ' D3346
                Else
                    If RA.Scenarios.SyncLinkedConstraintsValues(Nothing) Then SaveRA = True
                End If
                SaveMsg = "Update constraint"   ' D3757
                SaveComment = String.Format("Sync linked")    ' D3578 + D3757
                fResetSolver = True ' D4121
                ' D3343 ==

                ' D3539 ===
            Case "enable_constraint"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id"))   'Anti-XSS
                Dim fEnabled As Boolean = GetParam(args, "chk") = "1"
                Dim ID As Integer
                If Integer.TryParse(sID, ID) AndAlso ID >= 0 AndAlso ID < Scenario.Constraints.Constraints.Count Then    ' D3683
                    Dim RAC As RAConstraint = Scenario.Constraints.Constraints.ElementAt(ID).Value   ' D3683
                    If RAC IsNot Nothing Then RAC.Enabled = fEnabled
                    SaveMsg = "Update constraint"   ' D3757
                    SaveMsg = String.Format("'{0}' ignored: {1}", ShortString(RAC.Name, 40, True), Bool2YesNo(fEnabled))
                    SaveRA = True
                    fResetSolver = True ' D4121
                End If
                ' D3539 ==

            Case "reload"   ' D3124
                fResetSolver = True ' D3124

                ' D3933 ===
            Case "tp_usediscount"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                Scenario.TimePeriods.UseDiscountFactor = (sVal = "1")
                SaveRA = True
                SaveMsg = "Use discount factor: " + Bool2YesNo(Scenario.TimePeriods.UseDiscountFactor)
                fResetSolver = True ' D4121

            Case "tp_discount"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                Dim tVal As Double
                If String2Double(sVal, tVal) AndAlso tVal >= 0 AndAlso tVal <= 1 Then
                    Scenario.TimePeriods.DiscountFactor = tVal
                    SaveMsg = "Set discount factor: " + sVal
                    SaveRA = True
                    fResetSolver = True ' D4121
                End If
                ' D3933 ==

                ' D3948 ===
            Case "timeperiods"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                RA_ShowTimeperiods = (sVal = "1")
                App.ActiveProject.ProjectManager.Parameters.Save()
                fResetSolver = False    ' D4121
                ' D3948 ==

                ' D3407 ===
            Case "solver"
                'If ShowDraftPages() Then    ' D3623 -D3885
                ' D3628 ===
                Select Case GetParam(args, "lib")
                    'Case CInt(raSolverLibrary.raMSF).ToString  ' -D3888
                    '    RA_Solver = raSolverLibrary.raMSF   ' D3877 -D3888
                    Case CInt(raSolverLibrary.raGurobi).ToString
                        If App.isGurobiAvailable Then RA_Solver = raSolverLibrary.raGurobi ' D3877 + D3924
                        ' D7534 ===
                    Case CInt(raSolverLibrary.raBaron).ToString
                        If App.isBaronAvailable Then RA_Solver = raSolverLibrary.raBaron
                    Case CInt(raSolverLibrary.raXA).ToString
                        If App.isXAAvailable Then RA_Solver = raSolverLibrary.raXA
                        ' D7543 ==
                End Select
                'If Not ShowDraftPages() AndAlso RA_Solver <> raSolverLibrary.raXA Then RA_Solver = raSolverLibrary.raXA ' D3885
                If GetParam(args, "no_warn") = "1" Then RA_ShowFPWarning = False ' D3643
                fResetSolver = True ' D4121
                App.ActiveProject.ProjectManager.Parameters.Save()  ' D4681
                ' D3628 ==

                ' D3875 ===
                Select Case RA_Solver   ' D3877
                    Case raSolverLibrary.raXA
                        Dim Val As Integer
                        If Integer.TryParse(GetParam(args, "strategy"), Val) Then
                            If Val >= 0 AndAlso Val < RA.Solver.XA_STRATEGIES.Count Then RA_XAStrategy = Val ' D3877
                        End If
                        If Integer.TryParse(GetParam(args, "variation"), Val) Then
                            If Val >= 0 AndAlso Val < RA.Solver.XA_VARIATIONS.Count Then RA_XAVariation = RA.Solver.XA_VARIATIONS(Val) ' D3877
                        End If
                        If Integer.TryParse(GetParam(args, "timeout"), Val) Then
                            If Val > 0 Then RA_XATimeout = Val
                        End If
                        If Integer.TryParse(GetParam(args, "timeout_unch"), Val) Then
                            If Val > 0 Then RA_XATimeoutUnchanged = Val
                        End If
                End Select
                ' D3875 ==
                App.ActiveProject.ProjectManager.Parameters.Save()    ' D3877
                fResetSolver = True
                'SaveMSg = String.Format("Change solver ('{0}')", RA.Solver.SolverLibrary.ToString)  ' D3578 -D3757
                SaveRA = True
                'End If
                ' D3407 ==

                'A1431 ===
            Case "delete_nonfunded"
                ' Make snapshot
                App.ActiveProject.SaveStructure(ParseString("RA: before deleting %%alternatives%%"), True, True, ParseString("Before deleting %%alternatives%% that aren't funded"))
                ' Delete alternatives that are not funded
                Dim FundedAltsIDs As New List(Of Guid)
                For Each tAlt As RAAlternative In Scenario.Alternatives.Where(Function(raAlt) raAlt.Funded > 0)
                    FundedAltsIDs.Add(New Guid(tAlt.ID))
                Next
                Dim i As Integer = 0
                Dim AH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                While i < AH.TerminalNodes.Count
                    If Not FundedAltsIDs.Contains(AH.TerminalNodes()(i).NodeGuidID) Then
                        AH.DeleteNode(AH.TerminalNodes()(i))
                    Else
                        i += 1
                    End If
                End While
                ' Reload                
                RA.Load(PM.StorageManager.StorageType, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, App.ProjectID)    ' D4857
                ' Save                
                App.ActiveProject.SaveStructure(ParseString("RA: delete %%alternatives%%"), True, True, ParseString("Deleted %%alternatives%% that aren't funded"))
                'A1431 ==

                'A0910 ===
            Case "paste_column"
                fResetSolver = True
                GridAlternatives.DataSource = Nothing

                Dim column_id As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "column")) 'Anti-XSS
                Dim data As String = GetParam(args, "data").Replace(vbCr, "")  ' D3763 + D4193
                Dim cells As String() = data.Split(CChar(vbLf))    ' D3763
                Dim cells_count As Integer = cells.Count
                Dim fValueChanged As Boolean = False
                Dim alts_count As Integer = Scenario.Alternatives.Count
                If alts_count < cells.Count Then cells_count = alts_count
                Dim CCIndex As Integer = CInt(column_id) - CInt(raColumnID.CustomConstraintsStart)
                Dim CC As RAConstraint = Nothing
                Dim sColumn As String = column_id.ToString  ' D3763
                If CCIndex >= 0 AndAlso CCIndex < Scenario.Constraints.Constraints.Count Then
                    CC = Scenario.Constraints.Constraints.Values(CCIndex)
                    sColumn = String.Format("'{0}' custom constraint", ShortString(CC.Name, 45, True))  ' D3763
                Else
                    sColumn = OPT_COLS_DEFNAMES(CInt(column_id))  ' D3763
                End If

                Dim tAlts As List(Of RAAlternative) = Scenario.Alternatives  ' D3763
                'tAlts.Sort(New RAAlternatives_Comparer(Math.Abs(CInt(RA.Scenarios.GlobalSettings.SortBy)), CInt(RA.Scenarios.GlobalSettings.SortBy) < 0, RA)) ' D3763 + D3766
                DoSort(tAlts)   ' D4417
                Dim fSaveAttribs As Boolean = False ' D3766

                For i As Integer = 0 To cells_count - 1
                    Dim value As String = EcSanitizer.GetSafeHtmlFragment(cells(i)).Trim    'Anti-XSS
                    If String.IsNullOrEmpty(value) Then value = Nothing
                    Dim tDblValue As Double = 0
                    If String.IsNullOrEmpty(value) OrElse Not String2Double(value, tDblValue) Then tDblValue = UNDEFINED_INTEGER_VALUE
                    Dim trueVals As String() = {"1", "yes", "true", "set", "checked"}
                    Dim tBool As Boolean = trueVals.Contains(value)
                    Dim alt As RAAlternative = tAlts(i)
                    Select Case CType(CInt(column_id), raColumnID)
                        Case raColumnID.Cost
                            If alt.Cost <> tDblValue Then
                                ' assign cost value
                                RA.SetAlternativeCost(alt.ID, tDblValue, False)  ' D3766
                                'alt.Cost = tDblValue
                                fValueChanged = True
                                fSaveAttribs = True ' D3766
                            End If
                        Case raColumnID.ProbFailure
                            If alt.RiskOriginal <> tDblValue Then   ' D3763
                                If tDblValue >= 0 AndAlso tDblValue <= 1 Then
                                    ' assign risk value
                                    alt.RiskOriginal = tDblValue
                                    fValueChanged = True
                                Else
                                    alt.RiskOriginal = UNDEFINED_INTEGER_VALUE
                                    fValueChanged = True
                                End If
                                RA.SetAlternativeRisk(alt.ID, alt.RiskOriginal)  ' D3766
                                fSaveAttribs = True     ' D3766
                            End If
                            'A0969 ===
                        Case raColumnID.ProbSuccess
                            If alt.RiskOriginal <> 1 - tDblValue Then   ' D3763
                                If tDblValue >= 0 AndAlso tDblValue <= 1 Then
                                    ' assign risk value
                                    alt.RiskOriginal = 1 - tDblValue
                                    fValueChanged = True
                                Else
                                    alt.RiskOriginal = UNDEFINED_INTEGER_VALUE
                                    fValueChanged = True
                                End If 'A0969 ==
                            End If
                            ' D4202 ===
                        Case raColumnID.Musts
                            alt.Must = tBool
                            If tBool AndAlso alt.MustNot Then alt.MustNot = False
                            fValueChanged = True
                        Case raColumnID.MustNot
                            alt.MustNot = tBool
                            If tBool AndAlso alt.Must Then alt.Must = False
                            fValueChanged = True
                        Case raColumnID.isPartial
                            alt.IsPartial = tBool
                            fValueChanged = True
                            ' D4202 ==
                            ' D4930 ===
                        Case raColumnID.isCostTolerance
                            alt.AllowCostTolerance = tBool
                            fValueChanged = True
                            ' D4930 ==
                        Case Else
                            If CC IsNot Nothing Then
                                ' assign constraint value                                    
                                If CC.AlternativesData.ContainsKey(alt.ID) Then
                                    If CC.AlternativesData(alt.ID) <> tDblValue Then
                                        CC.AlternativesData(alt.ID) = tDblValue
                                        fValueChanged = True
                                    End If
                                Else
                                    CC.AlternativesData.Add(alt.ID, tDblValue)
                                    fValueChanged = True
                                End If
                            End If
                    End Select
                    'End If
                Next
                If fSaveAttribs Then PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID) ' D3766
                If fValueChanged Then
                    App.ActiveProject.SaveRA("Paste column values", True, True, sColumn) ' D3578 + D3757 + D3763
                    fResetSolver = True ' D4121
                End If
                'A0910 ==
        End Select
        If (Not isSolving AndAlso fResetSolver) OrElse (Not isSolving AndAlso SaveRA AndAlso fResetSolver) Then RA.Solver.ResetSolver() ' D2874 + D3246 + D4423
    End Sub

    ' D3112 ===
    Private Sub Ajax_Callback()
        Dim args As NameValueCollection = Page.Request.QueryString

        Dim sResult As String = ""
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION)).ToLower  'Anti-XSS
        Dim fDoSolve As Boolean = False  ' D3973

        Dim sComment As String = ""
        If RA_ShowTimeperiods Then sResult = CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction)) ' D3948

        Select Case sAction
            Case "prj_list"
                For Each tPrj As clsProject In App.ActiveProjectsList
                    ' D3114 ===
                    Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, tPrj.ID, App.Workspaces)
                    If tPrj.ID <> App.ProjectID AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted AndAlso
                       App.CanUserModifyProject(App.ActiveUser.UserID, tPrj.ID, App.ActiveUserWorkgroup, tWS) AndAlso
                       Not tPrj.isTeamTimeImpact AndAlso Not tPrj.isTeamTimeLikelihood AndAlso (tPrj.isValidDBVersion OrElse tPrj.isDBVersionCanBeUpdated) Then ' D3413
                        sResult += String.Format("{0}[{1},'{2}']", IIf(sResult = "", "", ","), tPrj.ID, JS_SafeString(ShortString(tPrj.ProjectName, 65, True)))
                    End If
                    ' D3114 ==
                Next
                sResult = "[" + sResult + "]"

                ' D3114 ===
            Case "create_risk"
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim   ' D3131 + Anti-XSS
                ' D3121 ===
                Dim sSrcID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "master_id")).Trim.ToLower    'Anti-XSS
                Dim tSrcID As Integer
                If Not Integer.TryParse(sSrcID, tSrcID) Then tSrcID = -1
                If sName <> "" Then
                    Dim tPrj As clsProject = CreateAssociatedModel(sName, tSrcID)
                    ' D3121 ==
                    If tPrj Is Nothing Then sResult = "-1" Else sResult = tPrj.ID.ToString
                End If
                ' D3114 ==

                ' D3129 ===
            Case "active"
                Dim sPrjID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "prj_id"))    'Anti-XSS
                Dim tPrjID As Integer
                If Integer.TryParse(sPrjID, tPrjID) Then
                    App.ProjectID = tPrjID
                    sResult = If(App.ProjectID = tPrjID, "1", "0")
                End If
                ' D3129 ==

                ' D3959 ===
            Case "save_altname"
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).Trim   'Anti-XSS
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id"))   'Anti-XSS
                Dim tRAAlt As RAAlternative = RA.Scenarios.Scenarios(0).GetAvailableAlternativeById(sID)
                If tRAAlt IsNot Nothing AndAlso sName <> "" AndAlso tRAAlt.Name <> sName Then
                    Dim sOld As String = tRAAlt.Name
                    tRAAlt.Name = sName
                    ' D3962 ===
                    Dim tAlt As clsNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(New Guid(tRAAlt.ID))
                    If tAlt IsNot Nothing Then
                        tAlt.NodeName = sName
                        ' D3962 ==
                        SaveRA = True
                        sResult = sName
                        App.ActiveProject.SaveStructure("Edit alternative name", True, True, String.Format("'{0}' -> '{1}'", ShortString(sOld, 60), ShortString(sName, 60)))
                    End If
                End If
                ' D3959 ==

                ' D3213 ===
            Case "scenario_reorder"
                Dim sLst As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "lst")).Trim    'Anti-XSS
                Dim IDs As String() = sLst.Split(CChar(","))
                If IDs.Count = RA.Scenarios.Scenarios.Count Then
                    For i As Integer = 0 To IDs.Count - 1
                        Dim SID As Integer = -1
                        If Integer.TryParse(IDs(i), SID) Then RA.Scenarios.Scenarios(SID).Index = i + 1
                    Next
                    RA.Scenarios.CheckAndSortScenarios()
                    sResult = App.ActiveProject.SaveRA("Update scenarios", False, , "Reorder scenarios").ToString   ' D3578 + D3757
                End If
                ' D3213 ==

                ' D3643 ===
            Case "no_fp_warn"
                RA_ShowFPWarning = False
                ' D3643 ==

            Case "use_ajax"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "use_ajax"))  'Anti-XSS
                UseAjax = sVal = "1"
                sResult = sVal

                ' D3915 ===
            Case "tp_switch"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id"))   'Anti-XSS
                Dim ID As Integer = -1
                If Integer.TryParse(sID, ID) Then
                    RA_TimePeriodVisible(ID) = (GetParam(args, "vis") = "1")
                End If
                App.ActiveProject.ProjectManager.Parameters.Save()  ' D3944
                ' D3915 ==

                ' D3944 ===
            Case "tp_switch_all"
                Dim fVis As Boolean = GetParam(args, "vis") = "1"
                RA_TimePeriodVisible(CInt(raColumnID.Cost)) = fVis
                For i As Integer = 0 To Scenario.Constraints.Constraints.Count
                    RA_TimePeriodVisible(CInt(raColumnID.CustomConstraintsStart) + i) = fVis
                Next
                App.ActiveProject.ProjectManager.Parameters.Save()

            Case "tp_distr"
                Dim sID As String = GetParam(args, "val")
                Dim ID As Integer = -1
                If Integer.TryParse(sID, ID) Then
                    RA.ProjectManager.Parameters.TimeperiodsDistributeMode = ID
                End If
                App.ActiveProject.ProjectManager.Parameters.Save()
                ' D3944 ==
                fDoSolve = RA_AutoSolve     ' D3973 + D4145
                sResult = "'ok'"

                ' D3932 + D3970 ===
            Case "timeperiods", "refresh"
                If sAction.ToLower = "timeperiods" Then
                    Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) 'Anti-XSS
                    RA_ShowTimeperiods = (sVal = "1")
                    App.ActiveProject.ProjectManager.Parameters.Save()  ' D3943
                    sResult = "timeperiods" ' D3948
                End If
                If RA_ShowTimeperiods Then
                    sResult = String.Format("['refresh', {0}, {1}, {2},'{3}','{4}']", GetRAProjects, GetRAPortfolioResources, GetPeriodResults, JS_SafeString(GetFundedInfo()), JS_SafeString(GetSolverMessage()))  ' D3982
                End If
                ' D3932 + D3970 ==

            Case "setresource"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "values")).ToLower  'Anti-XSS
                Dim sTotal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "total")).ToLower
                Dim sDM As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "dm")).ToLower
                If Scenario IsNot Nothing Then
                    Dim AVal As Double = 0
                    sResID = sResID.Replace("{", "")
                    sResID = sResID.Replace("}", "")
                    Dim values() As String = sVal.Split("x"c)
                    For i As Integer = 0 To values.Length - 1
                        AVal = 0
                        String2Double(values(i), AVal)
                        Scenario.TimePeriods.PeriodsData.SetResourceValue(i, sProjectID, New Guid(sResID), AVal)    ' D3918
                    Next

                    If sDM = "2" Then
                        Dim ATotal As Double = 0
                        If String2Double(sTotal, ATotal) Then
                            Dim res As RAResource = Scenario.TimePeriods.Resources(New Guid(sResID))
                            If res.ConstraintID < 0 Then
                                Dim alt As RAAlternative = RA.GetAlternativeById(Scenario, sProjectID)
                                If alt IsNot Nothing Then
                                    RA.SetAlternativeCost(sProjectID, ATotal)
                                    'alt.Cost = ATotal
                                End If
                            Else
                                Dim CC As RAConstraint = Scenario.Constraints.Constraints(res.ConstraintID)
                                If CC IsNot Nothing Then Scenario.Constraints.SetConstraintValue(res.ConstraintID, sProjectID, ATotal)
                            End If
                        End If
                    End If

                    sComment = "Set resource"  ' D3791
                End If
                fDoSolve = RA_AutoSolve     ' D3973 + D4145
                sResult = "'ok'"

            Case "setresourcegrid"
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                'Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "values")).ToLower  'Anti-XSS
                'Dim sTotal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "total")).ToLower
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("values", "")).ToLower  'Anti-XSS
                Dim sTotal As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("total", "")).ToLower
                If Scenario IsNot Nothing Then
                    Dim AVal As Double = 0
                    sResID = sResID.Replace("{", "")
                    sResID = sResID.Replace("}", "")
                    Dim values() As String = sVal.Split("*"c)
                    For i As Integer = 0 To values.Length - 1
                        If values(i) <> "" Then
                            Dim prjVals() As String = values(i).Split(";"c)
                            Dim sProjectID As String = prjVals(0)
                            For j As Integer = 1 To prjVals.Length - 1
                                AVal = 0
                                String2Double(prjVals(j), AVal)
                                Scenario.TimePeriods.PeriodsData.SetResourceValue(j - 1, sProjectID, New Guid(sResID), AVal)    ' D3918
                            Next
                            If sTotal <> "" Then
                                Dim ATotalVal As Double = 0
                                Dim sTotalVals() As String = sTotal.Split(";"c)
                                If String2Double(sTotalVals(i), ATotalVal) Then
                                    Dim res As RAResource = Scenario.TimePeriods.Resources(New Guid(sResID))
                                    If res.ConstraintID < 0 Then
                                        Dim alt As RAAlternative = RA.GetAlternativeById(Scenario, sProjectID)
                                        If alt IsNot Nothing Then
                                            RA.SetAlternativeCost(sProjectID, ATotalVal)
                                            'alt.Cost = ATotalVal
                                        End If
                                    Else
                                        Dim CC As RAConstraint = Scenario.Constraints.Constraints(res.ConstraintID)
                                        If CC IsNot Nothing Then Scenario.Constraints.SetConstraintValue(res.ConstraintID, sProjectID, ATotalVal)
                                    End If
                                End If
                            End If
                        End If
                    Next
                    sComment = "Set resource grid"  ' D3791
                End If
                fDoSolve = False
                sResult = "'ok'"
            Case "settotalresource"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value")).ToLower   'Anti-XSS
                If Scenario IsNot Nothing Then
                    Dim AVal As Double = 0
                    sResID = sResID.Replace("{", "")
                    sResID = sResID.Replace("}", "")
                    If String2Double(sVal, AVal) Then
                        Dim res As RAResource = Scenario.TimePeriods.Resources(New Guid(sResID))
                        If res.ConstraintID < 0 Then
                            Dim alt As RAAlternative = RA.GetAlternativeById(Scenario, sProjectID)
                            If alt IsNot Nothing Then
                                RA.SetAlternativeCost(sProjectID, AVal)
                                'alt.Cost = AVal
                            End If
                        Else
                            Dim CC As RAConstraint = Scenario.Constraints.Constraints(res.ConstraintID)
                            If CC IsNot Nothing Then Scenario.Constraints.SetConstraintValue(res.ConstraintID, sProjectID, AVal)
                        End If
                        SaveRA = True
                    End If
                End If
                fDoSolve = RA_AutoSolve ' D3973 + D4145
                sResult = "'ok'"
            Case "changeresourceid"
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                TP_RES_ID = sResID
                sResult = "'ok'"
            Case "solve"
                fDoSolve = True     ' D3973
                sResult = "'ok'"    ' D3973

            Case "expandprojects"
                If Scenario IsNot Nothing Then
                    Dim PeriodsCount = Scenario.TimePeriods.Periods.Count
                    For Each proj In Scenario.Alternatives
                        Dim projData = Scenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        Scenario.TimePeriods.PeriodsData.SetMaxPeriod(proj.ID, PeriodsCount - projData.Duration)
                    Next
                    sComment = "Expand All Projects Maxs"  ' D3791
                End If
                fDoSolve = RA_AutoSolve     ' D3973 + D4145
                sResult = "'ok'"

            Case "contractprojects"
                If Scenario IsNot Nothing Then
                    For Each proj In Scenario.Alternatives
                        Dim projData = Scenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        Scenario.TimePeriods.PeriodsData.SetMaxPeriod(proj.ID, projData.MinPeriod)
                    Next
                    sComment = "Contract All Projects Maxs"
                End If
                fDoSolve = RA_AutoSolve ' D3973 + D4145
                sResult = "'ok'"

            Case "trimtimeline"
                If Scenario IsNot Nothing Then
                    Dim maxPeriod As Integer = 1
                    For Each proj In Scenario.Alternatives
                        Dim projData = Scenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        If maxPeriod < (projData.MaxPeriod + projData.Duration) Then
                            maxPeriod = projData.MaxPeriod + projData.Duration
                        End If
                    Next
                    Dim pcount As Integer = Scenario.TimePeriods.Periods.Count
                    For i As Integer = 1 To (pcount - maxPeriod)
                        Dim periodsCount As Integer = Scenario.TimePeriods.Periods.Count
                        Dim lastPeriodID As Integer = Scenario.TimePeriods.Periods(periodsCount - 1).ID
                        Scenario.TimePeriods.DeletePeriod(lastPeriodID)
                    Next
                    sComment = "Trim Timeline"
                    fDoSolve = RA_AutoSolve     ' D4145
                End If
                sResult = "'ok'"

            Case "tpsetminval"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower  'Anti-XSS
                Dim AVal As Double = 0
                String2Double(sVal, AVal)
                Scenario.TimePeriods.Periods(CInt(sPeriodID)).SetResourceMinValue(New Guid(sResID), AVal)    ' D3918
                sComment = "Set resource minimum"  ' D3791
                fDoSolve = RA_AutoSolve     ' D3973 + D4145
                sResult = "'ok'"

            Case "tpsetallminval"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim AVal As Double = 0
                String2Double(sVal, AVal)
                For Each period In Scenario.TimePeriods.Periods
                    Scenario.TimePeriods.Periods(period.ID).SetResourceMinValue(New Guid(sResID), AVal)    ' D3918
                Next
                sComment = "Set resource minimum for all periods"  ' D3791
                fDoSolve = RA_AutoSolve     ' D3973 + D4145
                sResult = "'ok'"

            Case "tpsetmaxval"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower  'Anti-XSS
                Dim AVal As Double = 0
                String2Double(sVal, AVal)
                Scenario.TimePeriods.Periods(CInt(sPeriodID)).SetResourceMaxValue(New Guid(sResID), AVal)    ' D3918
                sComment = "Set resource maximum"  ' D3791
                fDoSolve = RA_AutoSolve ' D3973 + D4145
                sResult = "'ok'"

            Case "tpsetallmaxval"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim AVal As Double = 0
                String2Double(sVal, AVal)
                For Each period In Scenario.TimePeriods.Periods
                    Scenario.TimePeriods.Periods(period.ID).SetResourceMaxValue(New Guid(sResID), AVal)    ' D3918
                Next
                sComment = "Set resource maximum for all periods"  ' D3791
                fDoSolve = RA_AutoSolve ' D3973 + D4145
                sResult = "'ok'"

            Case "tpsetallminmaxval"
                Dim sMinVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "minval")).ToLower 'Anti-XSS
                Dim sMaxVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "maxval")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim AMinVal As Double = 0
                Dim AMaxVal As Double = 0
                String2Double(sMinVal, AMinVal)
                String2Double(sMaxVal, AMaxVal)
                For Each period In Scenario.TimePeriods.Periods
                    Scenario.TimePeriods.Periods(period.ID).SetResourceMinValue(New Guid(sResID), AMinVal)
                    Scenario.TimePeriods.Periods(period.ID).SetResourceMaxValue(New Guid(sResID), AMaxVal)
                Next
                sComment = "Set resource minimum and maximum for all periods"  ' D3791
                fDoSolve = RA_AutoSolve ' D3973 + D4145
                sResult = "'ok'"

            Case "tpcopyminrow"
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower
                Dim AScenario As RAScenario = Scenario
                If AScenario IsNot Nothing Then
                    For Each res As RAResource In AScenario.TimePeriods.EnabledResources
                        If Not res.ID.Equals(New Guid(sResID)) Then
                            For Each period As RATimePeriod In AScenario.TimePeriods.Periods
                                Dim AVal As Double = period.GetResourceMinValue(New Guid(sResID))
                                period.SetResourceMinValue(res.ID, AVal)
                            Next
                        End If
                    Next
                    sComment = "Copy min row to all resources"
                    fDoSolve = RA_AutoSolve
                End If
                sResult = "'ok'"

            Case "tpcopymaxrow"
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower
                Dim AScenario As RAScenario = Scenario
                If AScenario IsNot Nothing Then
                    For Each res As RAResource In AScenario.TimePeriods.EnabledResources
                        If Not res.ID.Equals(New Guid(sResID)) Then
                            For Each period As RATimePeriod In AScenario.TimePeriods.Periods
                                Dim AVal As Double = period.GetResourceMaxValue(New Guid(sResID))
                                period.SetResourceMaxValue(res.ID, AVal)
                            Next
                        End If
                    Next
                    sComment = "Copy max row to all resources"
                    fDoSolve = RA_AutoSolve
                End If
                sResult = "'ok'"

            Case "setpage"
                Dim sPg As String = GetParam(args, "pg")
                Dim Pg As Integer = RA_Pages_CurPage
                If Integer.TryParse(sPg, Pg) Then RA_Pages_CurPage = Pg
                sResult = RA_Pages_CurPage.ToString
                ' D4120 ==

                ' D4121 ===
            Case "pagination"
                Dim sPg As String = GetParam(args, "id")
                Dim Mode As Integer = CInt(RA_Pages_Mode)
                If Integer.TryParse(sPg, Mode) Then
                    If Mode = CInt(ecRAGridPages.NoPages) Then Mode = -RA_Pages_Mode ' D4480
                    RA_Pages_Mode = CType(Mode, ecRAGridPages)
                    App.ActiveProject.ProjectManager.Parameters.Save()
                End If
                sResult = CInt(RA_Pages_Mode).ToString
                ' D4121 + D4122 ==

                ' D4360 ===
            Case "sprty_cond"
                Dim fResult As Boolean = False
                Dim sID As String = GetParam(args, "id")
                Dim ID As Integer
                If RA_OPT_USE_SOLVER_PRIORITIES AndAlso Integer.TryParse(sID, ID) Then
                    Dim tPrty As RASolverPriority = Scenario.SolverPriorities.GetSolverPriorityByID(ID)
                    If tPrty IsNot Nothing Then
                        If tPrty.Condition = RASolverPriority.raSolverCondition.raMaximize Then tPrty.Condition = RASolverPriority.raSolverCondition.raMinimize Else tPrty.Condition = RASolverPriority.raSolverCondition.raMaximize
                        fResult = RA.SaveSolverPriorities(True) ' D4364 + D4367
                    End If
                End If
                sResult = Bool2Num(fResult).ToString

            Case "sprty_active"
                Dim fResult As Boolean = False
                Dim sID As String = GetParam(args, "id")
                Dim ID As Integer
                Dim fAct As Boolean = GetParam(args, "act") = "1"
                If RA_OPT_USE_SOLVER_PRIORITIES AndAlso Integer.TryParse(sID, ID) Then
                    Dim tPrty As RASolverPriority = Scenario.SolverPriorities.GetSolverPriorityByID(ID)
                    If tPrty IsNot Nothing Then    ' D4363
                        tPrty.InUse = fAct
                        fResult = RA.SaveSolverPriorities(True) ' D4364 + D4367
                    End If
                End If
                sResult = Bool2Num(fResult).ToString

            Case "sprty_reorder"
                Dim fResult As Boolean = False
                Dim sLst As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "lst")).Trim
                Dim IDs As String() = sLst.Split(CChar(","))
                If IDs.Count = Scenario.SolverPriorities.Priorities.Count Then
                    For i As Integer = 0 To IDs.Count - 1
                        Dim PID As Integer = -2
                        If Integer.TryParse(IDs(i), PID) Then
                            Dim tPrty As RASolverPriority = Scenario.SolverPriorities.GetSolverPriorityByID(PID)
                            If tPrty IsNot Nothing Then tPrty.Rank = i + 1
                        End If
                    Next
                    Scenario.SolverPriorities.CheckAndSort() ' D4364
                    fResult = RA.SaveSolverPriorities(True) ' D4364 + D4367
                End If
                sResult = Bool2Num(fResult).ToString
                ' D4360 ==

                ' D4362 ==
            Case "sprty_list"
                sResult = String.Format("[{0}]", GetSolverPriorities)
                ' D4362 ==

                ' D4521 ===
            Case "infeas_constr_get"
                sResult = ""    ' D4929
                Dim tList As List(Of ConstraintInfo) = RA.Solver.GetConstraintsList
                Dim oldConstr As List(Of String) = RA_Infeasibility_Constraints_List
                If tList IsNot Nothing Then
                    For Each tCI As ConstraintInfo In tList
                        sResult += String.Format("{0}['{1}','{2}','{3}',{4},{5}]", IIf(sResult = "", "", ","), JS_SafeString(tCI.ID), JS_SafeString(tCI.Name), JS_SafeString(tCI.Description), Bool2Num(If(oldConstr.Count = 0, tCI.Enabled, oldConstr.Contains(tCI.ID))), CInt(tCI.Type))  ' D7086
                    Next
                    sResult = "[" + sResult + "]"
                End If

            Case "infeas_constr_set"
                ' D6475 ===
                Dim SolCount As Integer = CheckVar("cnt", RA_Infeasibility_Solutions_Count)
                If SolCount < 1 Then SolCount = 1 Else If SolCount > 50 Then SolCount = 50
                RA_Infeasibility_Solutions_Count = SolCount
                ' D6475 ==
                Dim sLst As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("lst", "")).Trim
                Dim IDs As String() = sLst.Split(CChar(","))
                Dim tListAll As List(Of ConstraintInfo) = RA.Solver.GetConstraintsList
                Dim tLst As New List(Of ConstraintInfo)
                'tListAll.ForEach(Sub(c) If (IDs.Contains(c.ID)) Then tLst.Add(c))
                RA_Infeasibility_Constraints_List.Clear()   ' D7086
                For Each c As ConstraintInfo In tListAll
                    If IDs.Contains(c.ID) Then
                        tLst.Add(c)
                        RA_Infeasibility_Constraints_List.Add(c.ID) ' D7086
                    End If
                Next
                ' D4909 ===
                Dim fRes As Boolean = False
                ' D4912 + D6475 ===
                Dim Solutions As List(Of RAScenario) = RA.Solver.GetInfeasibilityResults(tLst, SolCount)
                Dim tmpScenario As RAScenario = Nothing
                'Dim tmpScenario As RAScenario = RA.Solver.GetInfeasibilityResults(tLst, tRemoved, tVal) ' D4909
                If Solutions IsNot Nothing AndAlso Solutions.Count > 0 Then
                    'RA_Infeasibility_Solutuions_List = Solutions
                    'Solutions.Sort(New RAScenarios_Comparer(raScenarioField.InfeasOptimalValue, True))
                    Dim idx As Integer = 1
                    For Each tScen As RAScenario In Solutions
                        'tScen.Name = String.Format("- Infeasibility Analysis{0} -", If(idx = 1, " [Best]", String.Format("[#{0}]", idx)))
                        tScen.Name = String.Format("Infeasibility Analysis #{0}", idx)
                        tScen.InfeasibilityScenarioID = Scenario.ID
                        tScen.InfeasibilityScenarioIndex = idx
                        idx += 1
                    Next
                    tmpScenario = Solutions(0)
                End If
                ' D6475 ==
                If tmpScenario IsNot Nothing AndAlso tmpScenario.ID > 0 AndAlso tmpScenario.ID <> Scenario.ID Then
                    ' D4912 ==
                    RA.Scenarios.ActiveScenarioID = tmpScenario.ID
                    RA.Solver.ResetSolver()
                    Solve()
                    If RA_ShowTimeperiods AndAlso Scenario.TimePeriods.Periods.Count = 0 Then RA_ShowTimeperiods = False
                    fRes = True
                    sResult = Bool2Num(fRes).ToString
                Else
                    sResult = GetSolverMessage(True)    ' D4929 + D7052
                End If
                ' D4909 ==
                'sResult = String.Format(RA.Solver.GetInfeasibilityInfo(tLst))
                ' D4521 ==

                ' D4909 ===
            Case "infeas_result"
                If Scenario.IsInfeasibilityAnalysis Then
                    Select Case CheckVar("mode", 0)

                        Case 1  ' save as new
                            If RA.Scenarios.Scenarios.ContainsKey(Scenario.InfeasibilityScenarioID) Then
                                Scenario.Name = String.Format("{0} (#{1})", RA.Scenarios.Scenarios(Scenario.InfeasibilityScenarioID).Name, Scenario.ID)
                            End If
                            Scenario.IsInfeasibilityAnalysis = False
                            SaveRA = True
                            SaveMsg = "Update Scenarios"    ' D3757
                            SaveComment = String.Format("Add scenario '{0}' (based on infeasibility analysis)", Scenario.Name)

                        Case 2  ' copy
                            If Scenario.IsInfeasibilityAnalysis AndAlso RA.Scenarios.Scenarios.ContainsKey(Scenario.InfeasibilityScenarioID) Then
                                Dim ID As Integer = Scenario.ID
                                CopyScenario(ID, Scenario.InfeasibilityScenarioID)
                                RA.Scenarios.ActiveScenarioID = Scenario.InfeasibilityScenarioID
                                RA.Scenarios.DeleteScenario(ID)
                                Scenario.IsInfeasibilityAnalysis = False    ' D6724
                                RA.Scenarios.CheckAndSortScenarios()    ' D3213
                                SaveRA = True
                            End If

                        Case Else   ' cancel
                            Dim ID As Integer = Scenario.ID
                            RA.Scenarios.ActiveScenarioID = Scenario.InfeasibilityScenarioID
                            RA.Scenarios.DeleteScenario(ID)
                            RA.Solver.ResetSolver()
                            If RA_AutoSolve Then Solve()
                            If RA_ShowTimeperiods AndAlso Scenario.TimePeriods.Periods.Count = 0 Then RA_ShowTimeperiods = False
                    End Select
                    ' D6475 ===
                    For i As Integer = RA.Scenarios.Scenarios.Count - 1 To 0 Step -1
                        Dim tScen As RAScenario = RA.Scenarios.Scenarios.ElementAt(i).Value
                        If tScen.IsInfeasibilityAnalysis Then RA.Scenarios.DeleteScenario(tScen.ID)
                    Next
                    ' D6475 ==
                End If
                sResult = "1"
                ' D4909 ==

            Case "alloc_table"
                sResult = GetAllocationComparisonTable()    ' D4609

            Case Else
                ProcessAjaxRequest(args)
                ' D3982 ===
                Select Case sAction.ToLower
                    Case "option", "solver" ' D4860
                        fDoSolve = RA_AutoSolve ' D4145
                End Select
                ' D3982 ==
                sResult = If(RA_ShowTimeperiods, "'ok'", "OK") ' D3948

        End Select

        If RA_ShowTimeperiods AndAlso sComment <> "" Then
            App.ActiveProject.SaveRA("Edit timeperiods", , , sComment) ' D3948
        Else
            If SaveRA Then App.ActiveProject.SaveRA(If(SaveMsg = "", "", "RA: " + SaveMsg), , SaveMsg <> "", SaveComment) ' D4150
        End If

        ' D3973 ===
        If RA_ShowTimeperiods AndAlso fDoSolve AndAlso sResult = "'ok'" Then  ' D4145
            Dim sPeriodResults As String = ""
            Dim resString As String = "solved"
            If Scenario IsNot Nothing Then
                Solve() ' D3969
                If RA.Solver.SolverState = raSolverState.raSolved Then  ' D3982
                    Dim ATPResults = Scenario.TimePeriods.TimePeriodResults
                    For Each item In ATPResults
                        sPeriodResults += CStr(IIf(sPeriodResults <> "", ",", "")) + String.Format("{{projID:'{0}', start:{1}}}", JS_SafeString(item.Key), item.Value)
                    Next
                End If
            End If
            sPeriodResults = String.Format("[{0}]", sPeriodResults)
            sResult = String.Format("['{0}',{1},'{2}','{3}']", resString, sPeriodResults, JS_SafeString(GetFundedInfo()), JS_SafeString(GetSolverMessage())) ' D3974 + D3982
        End If
        ' D3973 ==
        RawResponseStart()
        Response.ContentType = "text/plain"
        'Response.AddHeader("Content-Length", CStr(sResult))
        Response.Write(sResult)
        Response.End()
        'RawResponseEnd()
    End Sub
    ' D3112 ==

    ' D3189 ===
    Public Overrides Sub VerifyRenderingInServerForm(ctrl As Control)
        'Verifies that the control is rendered
    End Sub

    Private Sub ExportGrid2XLSX()
        Dim excel As New ExcelPackage()
        Dim workSheet = excel.Workbook.Worksheets.Add("ResourceAllocation")
        Dim totalCols = GridAlternatives.Rows(0).Cells.Count
        Dim totalRows = GridAlternatives.Rows.Count
        Dim headerRow = GridAlternatives.HeaderRow
        For i As Integer = 1 To totalCols
            workSheet.Cells(1, i).Value = headerRow.Cells(i - 1).Text
        Next
        For j As Integer = 1 To totalRows
            For i As Integer = 1 To totalCols
                workSheet.Cells(j + 1, i).Value = HTML2Text(GridAlternatives.Rows(j - 1).Cells(i - 1).Text)
                'workSheet.Cells(j + 1, i).Value = GridAlternatives.[GetType]().GetProperty(headerRow.Cells(i - 1).Text).GetValue(product, Nothing)
            Next
        Next
        Using memoryStream = New MemoryStream()
            DownloadStream(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", GetProjectFileName(App.ActiveProject.ProjectName, "RA Download", "", ".xlsx"), dbObjectType.einfFile, App.ProjectID, False)   ' D6593
            'Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            'Response.AddHeader("content-disposition", "attachment;  filename=download.xlsx")
            'excel.SaveAs(memoryStream)
            'memoryStream.WriteTo(Response.OutputStream)
            'Response.Flush()
            'Response.[End]()
        End Using
    End Sub

    Protected Sub Export2Excel()
        'RawResponseStart()
        Dim sContent As String = ""
        Dim sContentType As String = "application/vnd.ms-excel" ' D6593
        Dim sDownloadName As String = GetProjectFileName(String.Format("{0} (RA - {1}{2})", ShortString(App.ActiveProject.ProjectName, 40, True), Scenario.Name, IIf(isTimeperiodsVisible() AndAlso CheckVar("tp", False), " [with timperiods]", "")), App.ActiveProject.ProjectName, "Resource Aligner", ".xls") ' D4128

        Try
            Dim utf16 As New UTF8Encoding()
            Dim sWriter As New StringWriter()
            Dim hWriter As New HtmlTextWriter(sWriter)
            isExport = True
            Dim UseSorting As Boolean = GridAlternatives.AllowSorting
            GridAlternatives.AllowSorting = False
            Dim tWidth As Unit = GridAlternatives.Width
            GridAlternatives.Width = Unit.Empty
            GridAlternatives.BorderWidth = 1
            GridAlternatives.RenderControl(hWriter)
            GridAlternatives.AllowSorting = UseSorting
            GridAlternatives.Width = tWidth
            GridAlternatives.BorderWidth = 0

            sContent = File_GetContent(MapPath(CStr(IIf(OPT_SAVE_XLSX, FILE_XLSX_FOLDER + "\" + FILE_XLSX_CONTENT, "Allocation.xls"))), "") ' D3815
            If Not sContent.Contains("%%") Then
                sContent = sWriter.ToString
            Else
                Dim sIgnores As String = ""
                With Scenario.Settings
                    If Not .Musts Then sIgnores += CStr(IIf(sIgnores = "", "", ", ")) + ResString("optIgnoreMusts")
                    If Not .MustNots Then sIgnores += CStr(IIf(sIgnores = "", "", ", ")) + ResString("optIgnoreMustNot")
                    If Not .CustomConstraints Then sIgnores += CStr(IIf(sIgnores = "", "", ", ")) + ResString("optIgnoreCC")
                    If Not .Dependencies Then sIgnores += CStr(IIf(sIgnores = "", "", ", ")) + ResString("optIgnoreDependencies")
                    If Not .Groups Then sIgnores += CStr(IIf(sIgnores = "", "", ", ")) + ResString("optIgnoreGroups")
                    If Not .FundingPools Then sIgnores += CStr(IIf(sIgnores = "", "", ", ")) + ResString("optIgnoreFP")
                    If Not .Risks Then sIgnores += CStr(IIf(sIgnores = "", "", ", ")) + ResString("optIgnoreRisks")
                    If Not .TimePeriods Then sIgnores += CStr(IIf(sIgnores = "", "", ", ")) + ResString("optTimePeriods") ' D3630
                End With
                If sIgnores = "" Then sIgnores = ResString("lblRANone")

                Dim sBaseCase As String = ""
                With Scenario.Settings
                    If .UseBaseCase Then
                        If .BaseCaseForGroups Then sBaseCase += CStr(IIf(sBaseCase = "", "", ", ")) + ResString("optBCGroups")
                    End If
                End With
                If sBaseCase = "" Then sBaseCase = ResString("lblRANone")

                Dim sPage As String = sWriter.ToString.Replace("  ", " ")
                sPage = Regex.Replace(sPage, "(?<=<[_a-zA-Z][^<]*?)\s+(style|id|scope|tabindex|clip_data_id|clip_data|align|valign)=(""|\')[^(""|\')]*(""|\')", "")
                sPage = String.Format("<p>{0}</p><p><b>{1}</b>: {2}<br><b>{3}</b>: {4}</p>{5}", GetFundedInfo(), ResString("lblIgnorePnl"), sIgnores, ResString("lblBaseCasePnl"), sBaseCase, sPage).Replace(Chr(160), "") ' D4727
                sContent = ParseTemplate(ParseTemplate(sContent, "%%title%%", SafeFormString(String.Format(ResString("lblRATitle"), Scenario.Name)) + " (" + ResString(CStr(IIf(RA.Solver.SolverState = raSolverState.raSolved, "lblRASolved", "lblRAUnsolved"))) + ")"), "%%content%%", sPage, False) 'A0930 + D3542
            End If
            isExport = False

            ' D3815 ===
            If OPT_SAVE_XLSX Then
                Dim fCreated As Boolean = False
                Dim sPath = File_CreateTempName()
                File_Erase(sPath)
                File_CreateFolder(sPath)
                DebugInfo("Create extracted XLSX to temp folder for pack...")
                MyComputer.FileSystem.CopyDirectory(MapPath(FILE_XLSX_FOLDER), sPath, True)
                If MyComputer.FileSystem.DirectoryExists(sPath) Then

                    DebugInfo("Scan for files…")
                    MyComputer.FileSystem.WriteAllText(sPath + "\" + FILE_XLSX_CONTENT, sContent, False)
                    Dim AllFiles As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(sPath, SearchOption.SearchAllSubDirectories)
                    Dim Lst As New List(Of String)
                    For Each sFileName As String In AllFiles
                        Lst.Add(sFileName.Substring(sPath.Length + 1))
                    Next

                    Dim sError As String = ""
                    Dim sArcName As String = sPath + "\" + "Download.xlsx"
                    DebugInfo("Pack files as .xlsx")
                    If PackZipFilesAndDirs(sPath, Lst, sArcName, sError) Then fCreated = True
                    If fCreated Then sContent = File_GetContent(sArcName)

                    File_DeleteFolder(sPath)
                End If
            End If
            ' D3815 ==

            'Response.ContentType = "application/vnd.ms-excel"
            'Response.AddHeader("Content-Length", utf16.GetByteCount(sContent).ToString)

            'Dim sDownloadName As String = GetProjectFileName(String.Format("{0} (RA - {1}{2})", ShortString(App.ActiveProject.ProjectName, 40, True), Scenario.Name, IIf(isTimeperiodsVisible() AndAlso CheckVar("tp", False), " [with timperiods]", "")), App.ActiveProject.ProjectName, "Resource Aligner") ' D4128
            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}.{1}""", HttpUtility.UrlEncode(SafeFileName(sDownloadName)), IIf(OPT_SAVE_XLSX, "xlsx", "xls")))   ' D3815 + D6591

            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Export to Excel", String.Format("File: {0}; Size: {1}", sDownloadName, utf16.GetByteCount(sContent)))

        Catch ex As Exception
            sContentType = "text/plain"
            sContent = String.Format("(!) Unable to create file for export due to an error ({0}).", ex.Message) ' D4727

            'Response.ContentType = "text/plain"
            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Unable to create file for Export", ex.Message + vbCrLf + ex.StackTrace)  ' D4727
            'sContent = String.Format("(!) Unable to create file for export due to an error ({0}).", ex.Message) ' D4727
        End Try
        DownloadContent(sContent, sContentType, sDownloadName, dbObjectType.einfFile, App.ProjectID, False)    ' D6593
        'Response.Write(sContent)
        'If Response.IsClientConnected Then RawResponseEnd()
    End Sub
    ' D3189 ==

    ' D3193 ===
    Protected Sub Export2File(tExportType As raSolverExport)    ' D3236
        'RawResponseStart()
        Dim sContent As String = ""
        Dim sContentType As String = "application/octet-stream" ' D6593
        Dim sExt As String = If(tExportType = raSolverExport.raMPS, "mps", "oml")
        Dim sDownloadName As String = GetProjectFileName(App.ActiveProject.ProjectName + " - " + Scenario.Name, Scenario.Name, "", sExt)
        Try
            isExport = True
            Dim sName As String = File_CreateTempName()
            Dim sWriter As New StreamWriter(sName)
            RA.Solver.Solve(tExportType, sWriter)   ' D3236
            sWriter.Close()
            sContent = File_GetContent(sName, "")
            File_Erase(sName)
            isExport = False

            ''Response.ContentType = "application/xml"
            'Response.ContentType = "application/octet-stream"
            'Response.AddHeader("Content-Length", sContent.Length.ToString)

            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}.{1}""", HttpUtility.UrlEncode(SafeFileName(sDownloadName + " - " + Scenario.Name)), IIf(tExportType = raSolverExport.raMPS, "mps", "oml")))   ' D3236 + D6591

            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Export to " + tExportType.ToString, String.Format("File: {0}; Size: {1}", sDownloadName, sContent.Length))   ' D3236

        Catch ex As Exception
            sContentType = "text/plain"
            sContent = String.Format("(!) Unable to create file for export due to an error ({0}).", ex.Message)     ' D4727

            'Response.ContentType = "text/plain"
            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Unable to create export file " + tExportType.ToString, ex.Message + vbCrLf + ex.StackTrace)   ' D3236 + D4727
            'sContent = String.Format("(!) Unable to create file for export due to an error ({0}).", ex.Message)     ' D4727
        End Try
        DownloadContent(sContent, sContentType, sDownloadName, dbObjectType.einfFile, App.ProjectID)    ' D6593
        'Response.Write(sContent)
        'If Response.IsClientConnected Then RawResponseEnd()
    End Sub
    ' D3193 ==

    ' D3488 ===
    Protected Sub ExportXALogs()
        If RA_Solver = raSolverLibrary.raXA Then    ' D3877

            Dim sTmpFolder As String = File_CreateTempName()
            Dim sArcName As String = GetProjectFileName(String.Format("{0} - {1} (Solver Logs)", ShortString(App.ActiveProject.ProjectName, 45, True), ShortString(Scenario.Name, 45, True)), App.ActiveProject.ProjectName + " (Solver Logs)", "Solver logs", "zip").Replace("…", "_")

            If File_CreateFolder(sTmpFolder) Then
                sTmpFolder += "\"
                Dim sFullName As String = sTmpFolder + sArcName
                RA.Solver.Solve(raSolverExport.raNone, , sTmpFolder)

                'RawResponseStart()

                Dim sError As String = ""
                Dim sContent As String = ""
                Dim sContentType As String = "application/zip"
                Try
                    Dim Lst As New StringArray

                    If RA.Solver.SolverState = raSolverState.raError Then sError = String.Format("(!) Solver internal RTE: " + HTML2Text(SolverStateHTML(RA.Solver))) ' D3628

                    If sError = "" Then
                        Dim AllFiles As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(sTmpFolder)
                        DebugInfo("Scan for files…")
                        For Each sFileName As String In AllFiles
                            Lst.Append(sFileName)
                        Next

                        If Lst.Count > 0 Then
                            If PackZipFiles(Lst, sFullName, sError) Then
                                DownloadFile(sFullName, sContentType, sArcName, dbObjectType.einfFile, App.ProjectID,, False)   ' D6593
                                'Response.ContentType = "application/zip"
                                'Response.AddHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", HttpUtility.UrlEncode(SafeFileName(sArcName)))) ' D6591
                                'Dim fileLen As Long = MyComputer.FileSystem.GetFileInfo(sFullName).Length ' D0425
                                'Response.AddHeader("Content-Length", CStr(fileLen))
                                'Response.BinaryWrite(MyComputer.FileSystem.ReadAllBytes(sFullName))
                                sContent = ""
                                sError = ""
                            End If
                            File_Erase(sFullName)
                        Else
                            sError = "(!) No files were created by Solver" 'A1220
                        End If
                    End If

                    App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Export Solver logs", sError)
                    If sError <> "" Then
                        sContentType = "text/plain"
                        'Response.ContentType = "text/plain"
                        sContent = sError
                    End If

                Catch ex As Exception
                    sContentType = "text/plain"
                    'Response.ContentType = "text/plain"
                    'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Unable to create Solver logs file", ex.Message + vbNewLine + ex.StackTrace) ' D4727
                    sContent = String.Format("(!) Unable to create Solver logs due to an error ({0})", ex.Message)  ' D4727
                End Try

                File_DeleteFolder(sTmpFolder)
                'If sContent <> "" Then Response.Write(sContent)
                If sContent <> "" Then DownloadContent(sContent, sContentType, sArcName, dbObjectType.einfFile, App.ProjectID,, False)    ' D6593
                RawResponseEnd()
            End If
        End If
    End Sub
    ' D3488 ==

    ' D3870 ===
    Protected Sub ExportGurobiLogs()
        If RA_Solver = raSolverLibrary.raGurobi Then    ' D3924

            Dim sTmpFolder As String = File_CreateTempName()
            Dim sLogsName As String = GetProjectFileName(String.Format("{0} - {1} (Solver Logs)", ShortString(App.ActiveProject.ProjectName, 45, True), ShortString(Scenario.Name, 45, True)), App.ActiveProject.ProjectName + " (Solver Logs)", "Solver logs", "").Replace("…", "_")   ' D7078 // no ext

            If File_CreateFolder(sTmpFolder) Then
                sTmpFolder += "\"
                Dim sFullName As String = sTmpFolder + sLogsName
                RA.Solver.Solve(raSolverExport.raNone, , sFullName)

                Dim sContentType As String = "text/plain"

                Dim sError As String = ""
                Dim sContent As String = ""
                Try
                    If RA.Solver.SolverState = raSolverState.raError Then
                        sError = String.Format("(!) Solver internal RTE: " + HTML2Text(SolverStateHTML(RA.Solver)))
                        If RA.Solver.LastErrorReal <> RA.Solver.LastError Then sError += vbNewLine + RA.Solver.LastErrorReal    ' D7078
                    End If
                    If sError = "" Then
                        ' D7078 ===
                        'DownloadFile(sFullName, sContentType, sLogsName, dbObjectType.einfFile, App.ProjectID, True, False) ' D6593
                        Dim Lst As New StringArray
                        Dim AllFiles As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(sTmpFolder)
                        DebugInfo("Scan for files…")
                        For Each sFileName As String In AllFiles
                            Lst.Append(sFileName)
                        Next

                        If Lst.Count > 0 Then
                            Dim sArcName As String = String.Format("{0}.zip", sFullName)
                            If PackZipFiles(Lst, sArcName, sError) Then
                                sContentType = "application/zip"
                                DownloadFile(sArcName, sContentType, sLogsName + ".zip", dbObjectType.einfFile, App.ProjectID,, False)   ' D6593
                                sError = ""
                                sContent = ""
                            End If
                        Else
                            sError = "(!) No files were created by Solver" 'A1220
                        End If
                        ' D7078 ==
                    End If

                    App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Export Gurobi Solver logs", sError)
                    If sError <> "" Then
                        sContent = sError
                    End If

                Catch ex As Exception
                    sContent = String.Format("(!) Unable to create Solver logs due to an error ({0}) ", ex.Message)
                End Try

                File_DeleteFolder(sTmpFolder)
                If sContent <> "" Then DownloadContent(sContent, sContentType, sLogsName, dbObjectType.einfFile,, False, False)  ' D6593
                RawResponseEnd()
            End If
        End If
    End Sub
    ' D3870 ==

    ' D4582 ===
    Private Sub ExportAllocationComparisons()
        Dim sContent As String = ""
        Dim sContentType As String = "text/plain"
        Dim sLogsName As String = GetProjectFileName(String.Format("{0} - {1} (Sub-optimal allocations)", ShortString(App.ActiveProject.ProjectName, 45, True), ShortString(Scenario.Name, 45, True)), App.ActiveProject.ProjectName + " (Sub-optimal allocations)", "Sub-optimal allocations", "txt").Replace("…", "_") ' D4608

        Try
            ' D6623 ===
            Dim funded As New Dictionary(Of String, Double)
            For Each tAlt As RAAlternative In Scenario.AlternativesFull
                funded.Add(tAlt.ID, tAlt.Funded)
            Next

            sContent = RA.Solver.GetAllocationsText()

            For Each tAlt As RAAlternative In Scenario.AlternativesFull
                If funded.ContainsKey(tAlt.ID) Then tAlt.Funded = funded(tAlt.ID)
            Next
            ' D6623 ==
        Catch ex As Exception
            sContent = "(!) Unable to create Sub-optimal allocations report due to an error. "
        End Try

        If sContent <> "" Then DownloadContent(sContent, sContentType, sLogsName, dbObjectType.einfFile, App.ProjectID)  ' D6593
    End Sub
    ' D4582 ==

    ' D7582 ===
    Protected Sub ExportBaronLogs()
        If RA_Solver = raSolverLibrary.raBaron Then

            Dim sTmpFolder As String = File_CreateTempName()
            Dim sLogsName As String = GetProjectFileName(String.Format("{0} - {1} (Solver Logs)", ShortString(App.ActiveProject.ProjectName, 45, True), ShortString(Scenario.Name, 45, True)), App.ActiveProject.ProjectName + " (Solver Logs)", "Solver logs", "").Replace("…", "_")

            If File_CreateFolder(sTmpFolder) Then
                sTmpFolder += "\"
                Dim sFullName As String = sTmpFolder + sLogsName
                RA.Solver.Solve(raSolverExport.raNone, , sFullName)

                Dim sContentType As String = "text/plain"

                Dim sError As String = ""
                Dim sContent As String = ""
                Try
                    If RA.Solver.SolverState = raSolverState.raError Then
                        sError = String.Format("(!) Solver internal RTE: " + HTML2Text(SolverStateHTML(RA.Solver)))
                        If RA.Solver.LastErrorReal <> RA.Solver.LastError Then sError += vbNewLine + RA.Solver.LastErrorReal    ' D7078
                    End If
                    If sError = "" Then
                        ' D7078 ===
                        'DownloadFile(sFullName, sContentType, sLogsName, dbObjectType.einfFile, App.ProjectID, True, False) ' D6593
                        Dim Lst As New StringArray
                        Dim AllFiles As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(sTmpFolder)
                        DebugInfo("Scan for files…")
                        For Each sFileName As String In AllFiles
                            Lst.Append(sFileName)
                        Next

                        If Lst.Count > 0 Then
                            Dim sArcName As String = String.Format("{0}.zip", sFullName)
                            If PackZipFiles(Lst, sArcName, sError) Then
                                sContentType = "application/zip"
                                DownloadFile(sArcName, sContentType, sLogsName + ".zip", dbObjectType.einfFile, App.ProjectID,, False)   ' D6593
                                sError = ""
                                sContent = ""
                            End If
                        Else
                            sError = "(!) No files were created by Solver" 'A1220
                        End If
                        ' D7078 ==
                    End If

                    App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Export Gurobi Solver logs", sError)
                    If sError <> "" Then
                        sContent = sError
                    End If

                Catch ex As Exception
                    sContent = String.Format("(!) Unable to create Solver logs due to an error ({0}) ", ex.Message)
                End Try

                File_DeleteFolder(sTmpFolder)
                If sContent <> "" Then DownloadContent(sContent, sContentType, sLogsName, dbObjectType.einfFile,, False, False)  ' D6593
                RawResponseEnd()
            End If
        End If
    End Sub

    ' D7582 ==

    ' D4609 ===
    Private Function GetAllocByMode(tMode As AllocationMode) As AllocationResults
        Return RA.Solver.CreateAllocations().Find(Function(e) e.AllocationMode = tMode)
    End Function

    Private Function GetAllocRow(tAlloc As AllocationResults, tIdx As Integer, tMaxBenefit As Double) As String
        Return String.Format("<tr class='tbl_row{3} text'><td>{0}</td>{5}<td class='as_number'>{1}</td>{5}<td class='as_number'>{2}</td>{5}<td class='as_number'>{4}</td></tr>" + vbNewLine, RA.Solver.AllocationModeToString(tAlloc.AllocationMode), tAlloc.FundedBenefits.ToString("F4"), CostString(tAlloc.FundedCosts), If((tIdx And 1) <> 0, "_alt", ""), If(tMaxBenefit = 0, "", (100 * tAlloc.FundedBenefits / tMaxBenefit).ToString("F2") + "%"), vbTab)
    End Function

    Private Function GetAllocationComparisonTable() As String
        ' D6623 ===
        Dim funded As New Dictionary(Of String, Double)
        For Each tAlt As RAAlternative In Scenario.AlternativesFull
            funded.Add(tAlt.ID, tAlt.Funded)
        Next
        ' D6623 ==
        Dim sResult As String = ""
        Dim tOptimized As AllocationResults = GetAllocByMode(AllocationMode.amOptimization)
        Dim tMaxBenefit As Double = 0
        If tOptimized IsNot Nothing Then tMaxBenefit = tOptimized.FundedBenefits

        Dim Idx As Integer = 0
        Dim Rows As AllocationMode() = {AllocationMode.amFundMaxBenefit, AllocationMode.amBenefitfCost, AllocationMode.amOptimization}  ', AllocationMode.amFundMaxCost}    // per EF req
        For Each tMode As AllocationMode In Rows
            Dim tData As AllocationResults = GetAllocByMode(tMode)
            If tData IsNot Nothing Then
                sResult += GetAllocRow(tData, Idx, tMaxBenefit)
            End If
        Next
        ' D6623 ===
        For Each tAlt As RAAlternative In Scenario.AlternativesFull
            If funded.ContainsKey(tAlt.ID) Then tAlt.Funded = funded(tAlt.ID)
        Next
        ' D6623 ==

        If sResult <> "" Then
            sResult = String.Format("<p align=center><table class='tbl' border=0 cellspacing=1 cellpadding=2 style='width:100%'><tr class='tbl_hdr text'><td>{0}</td>{5}<td>{1}</td>{5}<td>{2}</td>{5}<td>{4}</td></tr>" + vbNewLine + "{3}</table></p>", ResString("lblRAACMode"), ResString("lblBenefits"), ResString("lblRACost"), sResult, ResString("lblRAACRatio"), vbTab)   ' D4608
        Else
            sResult = String.Format("<h6>{0}</h6>", ResString("msgRiskResultsNoData"))  ' D4608
        End If
        Return sResult
    End Function
    ' D4609 ==

    Function GurobiCloudDataGet(sRestName As String, ByRef sError As String) As String
        'Dim page As String = "https://cloud.gurobi.com/api/v2/machines?poolId=" + RA.Solver.OPT_GUROBI_POOL_ID
        ''Dim page As String = "https://cloud.gurobi.com/api/v2/licenses"

        Dim sResult As String = ""
        Dim Request As HttpWebRequest = CType(WebRequest.Create(RA.Solver.OPT_GUROBI_REST_URL_BASE + sRestName), HttpWebRequest)
        Request.Headers.Clear()
        Request.Method = "GET"
        Request.ContentType = "application/json"
        Request.Accept = "application/json"
        Request.Headers.Add("X-GUROBI-ACCESS-ID", RA.Solver.OPT_GUROBI_ACCESS_ID)
        Request.Headers.Add("X-GUROBI-SECRET-KEY", RA.Solver.OPT_GUROBI_SECRETE_KEY)
        Try
            Dim response As HttpWebResponse = CType(Request.GetResponse(), HttpWebResponse)
            Using sr = New StreamReader(response.GetResponseStream())
                sResult = sr.ReadToEnd()
                If sResult = "[]" Then sResult = ""
            End Using
        Catch ex As Exception
            sError = ex.Message
        End Try
        Return sResult
    End Function

    Private Function GetTabIndex(RowIndex As Integer, i As Integer) As Integer
        Dim TabIndex As Integer = (RowIndex + 1) * TAB_COEFF + i
        Return If(TabIndex > 32765, 32765, TabIndex)
    End Function

    Private Sub RABasePage_Init(sender As Object, e As EventArgs) Handles Me.Init
        RA.Load()
        'If Not App.isAuthorized OrElse Not App.HasActiveProject Then FetchAcetccess() ' D4379
        RA_OPT_ALLOW_COST_TOLERANCE_COLUMNS = ShowDraftPages() AndAlso RA.Solver.SolverLibrary = raSolverLibrary.raGurobi     ' D4930
        For i As Integer = 0 To OPT_COLS_DEFNAMES.Length - 1 Step 1
            OPT_COLS_DEFNAMES(i) = ResString(OPT_COLS_DEFNAMES(i), True)
            If i <> raColumnID.MustNot AndAlso i <> raColumnID.LastFundedPeriod Then OPT_COLS_DEFNAMES(i) = OPT_COLS_DEFNAMES(i).Replace(" ", "&nbsp;") ' D4201 + D4208 + D4803 // for allow to wrap "Must not" 
            'OPT_COLS_DEFNAMES(i) = ResString(OPT_COLS_DEFNAMES(i), True).Replace(" ", "&nbsp;")
        Next
        ' D3967 ===
        If isAJAX() Then
            DebugInfo("Check Ajax request")
            _isAjax = True  ' D3917
            Ajax_Callback() ' D3112
            DebugInfo("Ajax request finished")
        End If
        ' D3967 ==
        'OPT_EBENEFIT_NAME = ResString("lblExpectedBenefits")   ' -D3174
        ' D4125 + D4725 ===
        If Not IsPostBack AndAlso Not isCallback AndAlso Not isBigModelChecked Then
            If RA.Scenarios.GlobalSettings.isBigModel() Then
                If RA_AutoSolve OrElse RA_ShowTimeperiods Then
                    RA_AutoSolve = False
                    RA_ShowTimeperiods = False
                    StartupMessage = String.Format(ResString("msgRADisabledAutoSolve"), RA_BIG_MODEL_ALTS_COUNT)
                End If
                isBigModelChecked = True
            End If
        End If
        ' D4125 + D4725 ==
        ' D4219 ===
        If Not IsPostBack AndAlso Not isCallback Then
            Dim oldIsBig As String = SessVar(String.Format(SESS_BIGMODEL_OLD, App.ProjectID))
            Dim newIsBig As String = CStr(Bool2Num(RA.Scenarios.GlobalSettings.isBigModel()))
            If Not String.IsNullOrEmpty(oldIsBig) Then
                If oldIsBig <> newIsBig AndAlso oldIsBig <> "1" Then StartupMessage = ResString(If(isTimeperiodsVisible(), "msgRASwitchBigModel", "msgRASwitchBigModelNoTP")) ' D4725
            End If
            SessVar(String.Format(SESS_BIGMODEL_OLD, App.ProjectID)) = newIsBig
        End If
        ' D4219 ==
        ' D3181 ===
        If Not IsPostBack AndAlso Not isCallback AndAlso Not _isAjax Then   ' D3967
            DebugInfo("Check model start")
            If Not CheckAssociatedRiskModel(App.ActiveProject, True, AssociatedRiskModelName) Then StartupMessage = If(StartupMessage = "", "", StartupMessage + "<br><br>") + ResString("msgRANoRiskModel") ' D3129 + D4219 + D4488
            '-A1324 DebugInfo("Update benefits")
            '-A1324 RA.Scenarios.UpdateScenarioBenefits()
            '-A1324 DebugInfo("Check Alternatives")
            '-A1324 RA.Scenarios.CheckAlternatives()
            '-A1324 DebugInfo("Update sort order")
            '-A1324 RA.Scenarios.UpdateSortOrder()
            '-A1324 DebugInfo("Check model end")
            RA.Scenarios.CheckModel(, True) 'A1324 + D4929
        End If
        ' D3181 ==
        'If IgnoreFP() AndAlso Scenario.Settings.FundingPools Then Scenario.Settings.FundingPools = False ' D3569 + D3570 -D3649
    End Sub

End Class
