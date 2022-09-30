Imports System.Drawing
Imports Telerik.Web.UI

Partial Class RAFundingPoolsScrren
    Inherits clsComparionCorePage

    ' D6580 ===
    Public Const OPT_BAR_WIDTH As Integer = 40
    Public Const OPT_BAR_HEIGHT As Integer = 2
    Public Const OPT_BAR_COLOR_FILLED As String = "#8899cc"
    Public Const OPT_BAR_COLOR_EMPTY As String = "#d0d0d0"
    ' D6580 ==

    Public Const SESS_RA_AUTOSOLVE As String = "RA_AutoSolve"
    Public Const SESS_RA_SHOWFUNDEDONLY As String = "RA_ShowFundedOnly"
    Public Const SESS_RA_SHOWBARS As String = "RA_ShowTinyBars"
    Public Const SESS_RA_FP_CONSTR As String = "RA_FP_Constr"   ' D7106

    Public Const COL_INDEX As Integer = 0 'A1143
    Public Const COL_NAME As Integer = 1 'A1143
    Public Const COL_COST As Integer = 2 'A1143
    Public Const COL_POOLS_START_IDX As Integer = 3 'A1143

    Public Const OPT_ALLOW_COST_EDIT As Boolean = True  ' D2909

    ' D4365 ===
    Public Const BTN_SOLVE As Integer = 0
    Public Const BTN_AUTOSOLVE As Integer = 1
    Public Const BTN_SCENARIO As Integer = 3
    Public Const BTN_POOLS As Integer = 5
    Public Const BTN_EXHAUSTED As Integer = 6
    Public Const BTN_SETTINGS As Integer = 8
    ' D4365 ==

    Private isSolving As Boolean = False
    Private SaveRA As Boolean = False
    Private SaveComment As String = ""   ' D3790

    Public sFocusID As String = ""      ' D3039
    Public TAB_COEFF As Integer = 100    ' D4568

    Private _TPL_TOTAL As String = "<div class='as_number' style='font-weight:bold;{1}'><nobr>{0}</nobr></div>"
    Private _TPL_PERCENTAGE As String = "<div class='as_number' style='font-size:0.7rem;{1}'><nobr>{0}</nobr></div>"

    Private OldSolver As raSolverLibrary = raSolverLibrary.raXA ' D3427

    Private _FPConstrID As Integer = Integer.MaxValue       ' D7106
    Private _FPConstrGUI As Guid = Guid.Empty               ' D7106

    Public Sub New()
        MyBase.New(_PGID_RA_FUNDINGPOOLS)
    End Sub

    ReadOnly Property RA As ResourceAligner
        Get
            If PM.IsRiskProject Then 
                Return App.ActiveProject.ProjectManager.ResourceAlignerRisk
            End If
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property

    ' D7106 ===
    ReadOnly Property Scenario As RAScenario
        Get
            Return RA.Scenarios.ActiveScenario
        End Get
    End Property

    Public Function GetConstraintName() As String
        Return If(isCostConstraint(), ResString("titleCost"), GetActiveConstraint().Name)
    End Function
    ' D7106 ==

    Public Property RA_AutoSolve As Boolean
        Get
            If Session(SESS_RA_AUTOSOLVE) Is Nothing Then Return GetCookie(SESS_RA_AUTOSOLVE, False.ToString) = True.ToString Else Return CBool(Session(SESS_RA_AUTOSOLVE))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_AUTOSOLVE) = value
            SetCookie(SESS_RA_AUTOSOLVE, value.ToString)
        End Set
    End Property

    Public Property RA_ShowFundedOnly As Boolean
        Get
            If Session(SESS_RA_SHOWFUNDEDONLY) Is Nothing Then Return GetCookie(SESS_RA_SHOWFUNDEDONLY, False.ToString) = True.ToString Else Return CBool(Session(SESS_RA_SHOWFUNDEDONLY))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SHOWFUNDEDONLY) = value
            SetCookie(SESS_RA_SHOWFUNDEDONLY, value.ToString)
        End Set
    End Property

    ' D7093 + D7098 ===
    Public Property RA_UseFPPrty As Boolean
        Get
            Return App.ActiveProject.ProjectManager.Parameters.RAFundingPoolsUserPrty
        End Get
        Set(value As Boolean)
            App.ActiveProject.ProjectManager.Parameters.RAFundingPoolsUserPrty = value
            RA.Solver.UseFundingPoolsPriorities = value
        End Set
    End Property
    ' D7093 + D7098 ==

    Public Property RA_ShowTinyBars As Boolean
        Get
            If Session(SESS_RA_SHOWBARS) Is Nothing Then Return CBool(GetCookie(SESS_RA_SHOWBARS, True.ToString)) Else Return CBool(Session(SESS_RA_SHOWBARS))
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SHOWBARS) = value
            SetCookie(SESS_RA_SHOWBARS, value.ToString)
        End Set
    End Property

    ' D7106 ===
    Private Function CheckActiveConstraintID(ID As Integer) As Integer
        If ID < 0 Then
            ID = -1
        Else

            Dim fExists As Boolean = False
            For Each tVal As KeyValuePair(Of Integer, RAConstraint) In Scenario.Constraints.Constraints
                If tVal.Key = ID Then
                    fExists = True
                    Exit For
                End If
            Next
            If Not fExists Then ID = -1
        End If
        Return ID
    End Function

    Public Property ActiveConstraintID As Integer
        Get
            If _FPConstrID = Integer.MaxValue Then
                If Session(SESS_RA_FP_CONSTR) IsNot Nothing Then _FPConstrID = CInt(Session(SESS_RA_FP_CONSTR))
                _FPConstrID = CheckActiveConstraintID(_FPConstrID)
                Session(SESS_RA_FP_CONSTR) = _FPConstrID
                _FPConstrGUI = Guid.Empty
            End If
            Return _FPConstrID
        End Get
        Set(value As Integer)
            _FPConstrID = CheckActiveConstraintID(value)
            Session(SESS_RA_FP_CONSTR) = _FPConstrID
            _FPConstrGUI = Guid.Empty
        End Set
    End Property

    Public ReadOnly Property ActiveConstraintGUID() As Guid
        Get
            If _FPConstrGUI.Equals(Guid.Empty) Then
                _FPConstrGUI = If(ActiveConstraintID = -1, RA_Cost_GUID, Scenario.Constraints.GetConstraintGuidID(ActiveConstraintID))
            End If
            Return _FPConstrGUI
        End Get
    End Property

    Private Function GetActiveConstraint() As RAConstraint
        If isCostConstraint() Then Return Nothing Else Return Scenario.Constraints.GetConstraintByID(ActiveConstraintID)
    End Function

    Private Function isCostConstraint() As Boolean
        Return ActiveConstraintID = -1
    End Function

    Function ActiveFundingPools() As RAFundingPools
        Dim FPools As RAFundingPools = If(Scenario.ResourcePools.ContainsKey(ActiveConstraintGUID), Scenario.ResourcePools(ActiveConstraintGUID), Nothing)
        If FPools Is Nothing Then
            FPools = New RAFundingPools() With {.ResourceID = ActiveConstraintGUID}
            Scenario.ResourcePools.Add(ActiveConstraintGUID, FPools)
        End If
        Return FPools
    End Function
    ' D7106 ==

    ' D2909 ===
    Public Function GetFundingPools() As String
        Dim sRes As String = ""
        For Each tID As Integer In ActiveFundingPools.GetPoolsOrderAsList ' D4367
            Dim tPool As RAFundingPool = ActiveFundingPools.Pools(tID)
            sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("[{0},'{1}',{2},{3}]", tID, JS_SafeString(tPool.Name.Trim), Bool2Num(tPool.Enabled), 0)
        Next
        Return sRes
    End Function
    ' D2909 ==

    ' D3430 ===
    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tID As Integer In RA.Scenarios.Scenarios.Keys
            sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("[{0},'{1}','{2}',{3}]", tID, JS_SafeString(RA.Scenarios.Scenarios(tID).Name), JS_SafeString(RA.Scenarios.Scenarios(tID).Description), RA.Scenarios.Scenarios(tID).Index)
        Next
        Return sRes
    End Function
    ' D3430 ==

    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        If Scenario.Alternatives.Count > 0 AndAlso SaveRA Then App.ActiveProject.SaveRA("Edit Funding Pools", , , SaveComment) ' D2909 + D3790
        RA.Solver.SolverLibrary = OldSolver ' D3427
        ' D3430 ===
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower   ' Anti-XSS
        Select Case sAction
            Case "scenario"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    RA.Scenarios.ActiveScenarioID = ID
                    RA.Solver.ResetSolver()
                    ActiveConstraintID = -1 ' D7107
                End If
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
                ' D3431 ===
            Case "enable_fp"
                Scenario.Settings.FundingPools = True
                App.ActiveProject.SaveRA("Change ignore option", , , "Funding Pools: No")   ' D3790
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
                ' D3431 ==
        End Select
        ' D3430 ==
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If Not _RA_FUNDING_POOLS_AVAILABLE AndAlso Not ShowDraftPages() Then Response.Redirect(PageURL(_PGID_SERVICEPAGE, _PARAM_ACTION + "=msg&type=fp_comingsoon"), True) ' D3241 + D3244

        ' D7106 ===
        Dim sConstr As String = CheckVar("constr", "")
        If sConstr <> "" Then
            Dim tmpID As Integer
            If Integer.TryParse(sConstr, tmpID) Then ActiveConstraintID = tmpID
            Response.Redirect(PageURL(CurrentPageID), True)
        End If
        ' D7106 ==

        OldSolver = RA.Solver.SolverLibrary ' D3427
        RA.Solver.SolverLibrary = raSolverLibrary.raXA ' D3427
        '' D3431 ===
        'lblMessage.Visible = False
        'If Not Scenario.Settings.FundingPools Then
        '    lblMessage.Visible = True
        '    lblMessage.Text = String.Format("<p align='center'><span class='top_warning_nofloat'>{0}</span></p>", "123")
        'End If
        '' D3431 ==
        If Not IsPostBack AndAlso Not isCallback Then
            AlignVerticalCenter = False
            '-A1324 RA.Scenarios.CheckAlternatives()
            RA.Scenarios.CheckModel() 'A1324
            'pnlLoadingPanel.Caption = ResString("msgLoading")
            'pnlLoadingPanel.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")
            If Scenario.Alternatives.Count = 0 Then
                ' D4365 ===
                RadToolBarMain.Items(BTN_SOLVE).Visible = False ' solve
                RadToolBarMain.Items(BTN_AUTOSOLVE).Enabled = False ' pools
                RadToolBarMain.Items(BTN_POOLS).Enabled = False ' auto-solve
                RadToolBarMain.Items(BTN_EXHAUSTED).Enabled = False ' options
                RadToolBarMain.Items(BTN_SETTINGS).Enabled = False ' options
            End If
            CType(RadToolBarMain.Items(BTN_AUTOSOLVE), RadToolBarButton).Checked = RA_AutoSolve
            RadToolBarMain.Items(BTN_EXHAUSTED).Visible = RA_OPT_USE_FPOOLS_EXHAUSTED
            ' D3120 ===
            RadToolBarMain.Items(BTN_SOLVE).Text = ResString("btnRASolve")
            '-A1018 RadToolBarMain.Items(1).Text = ResString("lblScenario") + ":"   ' D3430
            RadToolBarMain.Items(BTN_AUTOSOLVE).Text = ResString("btnRAAutoSolve")
            RadToolBarMain.Items(BTN_POOLS).Text = ResString("btnRAManageFPools")
            RadToolBarMain.Items(BTN_SETTINGS).Text = ResString("btnRASettings")
            ' D4365 ==
            'With CType(RadToolBarMain.Items(5), RadToolBarDropDown)
            '    .Buttons(0).Text = "&nbsp; &nbsp;" + ResString("btnRAShowFundedOnly")
            '    .Buttons(1).Text = "&nbsp; &nbsp;" + ResString("btnRAShowBars")
            'End With
            GridAlternatives.Columns(COL_INDEX).HeaderText = ResString("tblID")
            GridAlternatives.Columns(COL_NAME).HeaderText = ResString("tblAlternativeName")
            GridAlternatives.Columns(COL_COST).HeaderText = ResString("titleCost")
            ' D3120 ==
            ' D3431 ===
            If Not Scenario.Settings.FundingPools Then
                With CType(RadToolBarMain.Items(0), RadToolBarButton)
                    .Enabled = False
                    .ForeColor = Color.Gray
                    .BorderColor = Color.LightGray
                End With
                RadToolBarMain.Items(6).Enabled = False
                CType(RadToolBarMain.Items(6), RadToolBarButton).Checked = False
                RA.Solver.ResetSolver()
            End If
            ' D3431 ==
            ClientScript.RegisterStartupScript(GetType(String), "init", "InitPage();", True)
        End If
    End Sub

    'Public Sub OnLoadSolveButton(sender As Object, e As EventArgs)
    '    If Not IsPostBack AndAlso Not isCallback Then CType(sender, RadToolBarButton).Attributes.Add("style", "text-align:center;")
    'End Sub

    Protected Sub OnLoadSolveButton(sender As Object, e As EventArgs) Handles btnSolveOnLoad.Click
        If Not IsPostBack AndAlso Not isCallback Then CType(sender, RadToolBarButton).Attributes.Add("style", "text-align:center;")
    End Sub

    Public ReadOnly Property SelectedEventIDs As List(Of Guid)
        Get
            Dim EventsIDs As New List(Of Guid)
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                If alt.Enabled Then
                    EventsIDs.Add(alt.NodeGuidID)
                End If
            Next
            Return EventsIDs
        End Get
    End Property

    Public Enum OptimizerErrors
        oeNone = 0
        oeNoControls = 1
        oeInfeasible = 2
    End Enum

    Private OptimizerError As OptimizerErrors = OptimizerErrors.oeNone

    Private Function OptimizeTreatments(ByRef totalCost As Double, ByRef optimizedRiskValue As Double, Optional OutputPath As String = "") As List(Of Guid)
        Dim fundedControls As New List(Of Guid)
        optimizedRiskValue = 0
        totalCost = 0
        OptimizerError = OptimizerErrors.oeNone

        Dim state As raSolverState

        If RA.RiskOptimizer.SolverLibrary <> raSolverLibrary.raBaron Then RA.RiskOptimizer.SolverLibrary = raSolverLibrary.raBaron

        If PM.Controls.SetControlsVars(SelectedEventIDs) > 0 Then ' If count of applicable controls = 0 then Baron solver returns a Syntax Error
            optimizedRiskValue = RA.RiskOptimizer.Optimize(SelectedEventIDs, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, fundedControls, totalCost, OutputPath, , state)
            If state = raSolverState.raInfeasible Then
                OptimizerError = OptimizerErrors.oeInfeasible
            End If
        Else
            OptimizerError = OptimizerErrors.oeNoControls
        End If

        ' D4072 ===
        If ShowDraftPages() AndAlso RA.RiskOptimizer.SolverLibrary = raSolverLibrary.raBaron AndAlso Baron_Error_Code <> BaronErrorCode.None Then
            App.DBSaveLog(dbActionType.actRASolveModel, dbObjectType.einfProject, App.ProjectID, "Riskion optimization", String.Format("Solver '{0}' error: {1}", ResString("lblRASolverBaron"), GetMessageByBaronError())) ' D4078
        End If
        ' D4072

        Return fundedControls
    End Function

    Private FundedControlsGuids As List(Of Guid) = Nothing

    Private Sub Solve()
        If Scenario.Alternatives.Count > 0 AndAlso Scenario.Settings.FundingPools Then    ' D3431
            isSolving = True
            If PM.IsRiskProject Then 
                Dim tFundedCost As Double = 0
                Dim tOptimizedRisk As Double = 0
                FundedControlsGuids = OptimizeTreatments(tFundedCost, tOptimizedRisk)
            Else
                RA.Solve()  ' D3900
            End If
        End If
    End Sub

    ' D3431 ===
    Public Function GetMessage() As String
        Dim sMessage As String = "" ' D3825
        If Scenario.Alternatives.Count > 0 Then sMessage = SolverStateHTML(RA.Solver) ' D3825
        If Not Scenario.Settings.FundingPools Then
            sMessage += String.Format("<div style='padding:4px 0px 10px 0px'><span class='top_warning_nofloat' style='font-size:10pt; color:#993333; padding:4px 6px;'><!--img src='{2}attention.png' width=14 height=14 border=0/-->&nbsp;{0}</span> <input type='button' id='btnEnableFP' value='{1}' onclick='enableFP(); return false;' class='button button_small' style='width:15em; padding:5px; margin-top:6px; color:#333366;'/></div>", ResString("msgRAFPIgnored"), SafeFormString(ResString("btnRAEnableFP")), ImagePath)
        End If
        Return sMessage
    End Function
    ' D3431 ==

    Protected Sub GridAlternatives_Load(sender As Object, e As EventArgs) Handles GridAlternatives.PreRender
        If GridAlternatives.DataSource Is Nothing Then
            If RA_AutoSolve OrElse isSolving Then Solve()
            BindPoolsColumns()    ' D2909
            GridAlternatives.DataSource = Scenario.Alternatives
            GridAlternatives.DataBind()
            CType(GridAlternatives.DataSource, List(Of RAAlternative)).Sort(New RAAlternatives_Comparer(CType(Math.Abs(RA.Scenarios.GlobalSettings.SortBy), RAGlobalSettings.raColumnID), RA.Scenarios.GlobalSettings.SortBy < 0, RA))    ' D7073
            'GridAlternatives.Columns(COL_INDEX).Visible = RA.Scenarios.GlobalSettings.IsIndexColumnVisible 'A1143
        End If
    End Sub

    ' D2909 ===
    Private Sub BindPoolsColumns()
        If GridAlternatives.Columns.Count >= COL_POOLS_START_IDX Then
            For i As Integer = GridAlternatives.Columns.Count - 1 To COL_POOLS_START_IDX Step -1
                GridAlternatives.Columns.RemoveAt(i)
            Next
        End If
        For Each tID As Integer In ActiveFundingPools.GetPoolsOrderAsList ' D4367
            Dim tFP As RAFundingPool = ActiveFundingPools.Pools(tID)

            ' D7092 ===
            Dim tPrty As New TemplateField()
            tPrty.HeaderStyle.CssClass = "text_overflow"
            tPrty.HeaderText = SafeFormString(ShortString(tFP.Name, 35, True))
            'tPrty.HeaderStyle.BackColor = Color.FromArgb(1, 210, 230, 255)
            GridAlternatives.Columns.Add(tPrty)
            ' D7092 ==

            Dim tLimit As New TemplateField()
            tLimit.HeaderStyle.CssClass = "text_overflow"
            tLimit.HeaderText = SafeFormString(ShortString(tFP.Name, 35, True)) 'A0930
            tLimit.HeaderStyle.BackColor = Color.FromArgb(1, 210, 230, 255)
            GridAlternatives.Columns.Add(tLimit)

            If Scenario.Settings.FundingPools Then   ' D3431
                Dim tfield As New TemplateField()
                tfield.ShowHeader = False
                GridAlternatives.Columns.Add(tfield)
            End If
        Next
    End Sub
    ' D2909 ==

    Private Function GetTabIndex(RowIndex As Integer, i As Integer) As Short
        Dim TabIndex As Integer = (RowIndex + 1) * TAB_COEFF + i
        Return CShort(If(TabIndex > 32765, 32765, TabIndex))
    End Function

    ' D3638 ===
    'Private Sub CopyPasteHeaderButtons(Cell As TableCell, idx As Integer, HasCopyButton As Boolean, HasPasteButton As Boolean, HasIgnoreButton As Boolean, isIgnoredFP As Boolean)
    '    Dim tUniqueID As String = idx.ToString
    '    If HasPasteButton OrElse HasCopyButton OrElse HasIgnoreButton Then
    '        'Cell.Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:100%'><tr valign='middle' class='grid_clear'>" +
    '        '                          "<td class='text' align='center' width='99%'>{4}</td>" +
    '        '                          "<td style='width:15px' align='right'><img src='{1}menu_dots.png' width='12' height='12' style='cursor:context-menu; padding-left:3px;' id='mnu_img_{0}{7}' alt='' onclick='showMenu(event, ""{0}"", {2}, {3}, {5}, {6});' oncontextmenu='showMenu(event, ""{0}"", {2}, {3}, {5}, {6}); return false;'/></td>" +
    '        '                          "</tr></table>", tUniqueID, ImagePath, Bool2JS(HasCopyButton), Bool2JS(HasPasteButton), Cell.Text, Bool2JS(HasIgnoreButton), Bool2JS(isIgnoredFP), IIf(HasIgnoreButton, "i", ""))    ' D4567 + D4568
    '        Cell.Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:100%'><tr valign='middle' class='grid_clear'>" +
    '                                 "<td class='text' align='center' width='99%'>{4}</td>" +
    '                                 "<td style='width:15px' align='right'><img src='../../Images/img/icon/copy-grey-icon.svg' height='20'></td>" +
    '                                 "</tr></table>", tUniqueID, ImagePath, Bool2JS(HasCopyButton), Bool2JS(HasPasteButton),
    '                                 Cell.Text, Bool2JS(HasIgnoreButton), Bool2JS(isIgnoredFP), IIf(HasIgnoreButton, "i", ""))    ' D4567 + D4568
    '    End If
    'End Sub

    Private Sub CopyPasteHeaderButtons(Cell As TableCell, idx As Integer, HasCopyButton As Boolean, HasPasteButton As Boolean, HasIgnoreButton As Boolean, isIgnoredFP As Boolean, cellName As String)
        Dim tUniqueID As String = idx.ToString
        If HasPasteButton OrElse HasCopyButton OrElse HasIgnoreButton Then
            Cell.Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:100%'><tr valign='middle' class='grid_clear'>" +
                                      "<td class='text' align='center' width='99%'>{4}</td>" +
                                     "<td style='width:15px' align='right'><img src='{1}menu_dots.png' width='12' height='12' style='cursor:context-menu; padding-left:3px;' id='mnu_img_{0}{7}' alt='' onclick='showMenu(event, ""{0}"", {2}, {3}, {5}, {6});' oncontextmenu='showMenu(event, ""{0}"", {2}, {3}, {5}, {6}); return false;'/></td>" +
                                    "</tr></table>", tUniqueID, ImagePath, Bool2JS(HasCopyButton), Bool2JS(HasPasteButton), Cell.Text, Bool2JS(HasIgnoreButton), Bool2JS(isIgnoredFP), IIf(HasIgnoreButton, "i", ""))    ' D4567 + D4568

        End If
    End Sub


    ' D3638 ==

    Protected Sub GridAlternatives_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles GridAlternatives.RowDataBound
        Dim tIndexVisible As Boolean = RA.Scenarios.GlobalSettings.IsIndexColumnVisible 'A1143
        Dim FPEnabled As Boolean = Scenario.Settings.FundingPools    ' D6484
        ' D2909 ===
        If e.Row.RowType = DataControlRowType.Header AndAlso e.Row.RowState = DataControlRowState.Normal Then
            If Not tIndexVisible Then e.Row.Cells(COL_INDEX).CssClass = "ra_hidden" 'A1143
            e.Row.Cells(COL_COST).Width = New Unit(98, UnitType.Pixel) 'A2011

            'D7106 ===
            Dim CList As New Dictionary(Of Integer, String)
            CList.Add(-1, ResString("titleCost"))
            For Each tVal As KeyValuePair(Of Integer, RAConstraint) In Scenario.Constraints.Constraints
                CList.Add(tVal.Key, tVal.Value.Name)
            Next

            Dim sCCList As String = String.Format("Resource:<br><select id='cc_id' style='width:120px' class='form-select bg-white' onchange='return selectConstr(this.value);'>")
            For Each tVal As KeyValuePair(Of Integer, String) In CList
                sCCList += String.Format("<option value='{0}'{1}>{2}</option>", tVal.Key, If(tVal.Key = ActiveConstraintID, " selected", ""), tVal.Value)
            Next
            sCCList += "</select>"
            If ActiveFundingPools.Pools.Count > 0 Then e.Row.Cells(COL_COST).RowSpan = 3 'A1143
            e.Row.Cells(COL_COST).Text = sCCList
            ' D7106 ==

            If ActiveFundingPools.Pools.Count > 0 Then
                e.Row.Cells(COL_INDEX).RowSpan = 3 'A1143
                e.Row.Cells(COL_NAME).RowSpan = 3 'A1143 
                e.Row.Cells(COL_NAME).Text = If(PM.IsRiskProject, ParseString("%%Control%% Name"), ParseString("%%Alternative%% Name"))

                Dim trLabels As New GridViewRow(1, -1, DataControlRowType.Header, DataControlRowState.Alternate)
                Dim trEdits As New GridViewRow(2, -1, DataControlRowType.Header, DataControlRowState.Alternate)

                Dim idx As Integer = COL_POOLS_START_IDX
                Dim idx_tab As Integer = 0  ' D4568 
                For Each tID As Integer In ActiveFundingPools.GetPoolsOrderAsList ' D4367
                    With ActiveFundingPools.Pools(tID)

                        idx_tab += 1    ' D4568

                        If RA_UseFPPrty Then   ' D7093
                            ' D7092 ==
                            Dim tCellLblPrty As New TableCell
                            tCellLblPrty.Text = ResString("lblRAFP_Prty")
                            tCellLblPrty.BorderWidth = 0
                            tCellLblPrty.Width = 70
                            If Not .Enabled Then tCellLblPrty.Text = String.Format("<i>{0}</i>", tCellLblPrty.Text)
                            trLabels.Cells.Add(tCellLblPrty)
                            'CopyPasteHeaderButtons(tCellLblPrty, .ID, True, True, False, False)
                            ' D7092 ==
                        End If

                        Dim tCellLblLimit As New TableCell
                        'tCellLblLimit.Text = ResString("lblRAFP_Limit")
                        tCellLblLimit.BorderWidth = 0
                        tCellLblLimit.Width = 90
                        tCellLblLimit.Text = "<table><tr><td>" + ResString("lblRAFP_Limit") + "</td>" +
                                             "<td style='width:100px'> <a href='javascript:void(0)' onclick='doPasteAttributeValues(" + .ID.ToString + ")' align='right'><img align='right' src='../../Images/img/Paste-icon.svg' style='' height='15'></a>&nbsp;&nbsp;" +
                                              "<a href='javascript:void(0)' align='right' onclick='doCopyToClipboardValues(" + .ID.ToString + ")'><img  src='../../Images/img/Copy_icon.svg' height='15'></a></td></tr></table>"
                        'If Not .Enabled Then tCellLblLimit.Text = String.Format("<i>{0}</i>", tCellLblLimit.Text) ' D4367
                        trLabels.Cells.Add(tCellLblLimit)
                        'CopyPasteHeaderButtons(tCellLblLimit, .ID, True, True, False, False, "")   ' D3638 + D4566

                        If FPEnabled Then   ' D3431 + D6484
                            Dim tCellLblAlloc As New TableCell
                            tCellLblAlloc.Text = ResString("lblRAFP_Allocated")
                            tCellLblAlloc.BorderWidth = 0
                            tCellLblAlloc.Width = 90
                            If Not .Enabled Then tCellLblAlloc.Font.Italic = True ' D4367
                            trLabels.Cells.Add(tCellLblAlloc)
                        End If

                        If RA_UseFPPrty Then   ' D7093
                            ' D7092 ===
                            Dim tCellHdrPrty As New TableCell
                            tCellHdrPrty.Text = "&nbsp;"
                            tCellHdrPrty.Attributes.Add("clip_data_id", CInt(.ID).ToString) ' D3638

                            tCellHdrPrty.Attributes.Add("clip_data", JS_SafeString(CLIPBOARD_CHAR_UNDEFINED_VALUE))
                            trEdits.Cells.Add(tCellHdrPrty)
                            ' D7092 ==
                        End If

                        Dim tCellEditLimit As New TableCell
                        Dim sVal As String = ""
                        If .PoolLimit <> UNDEFINED_INTEGER_VALUE Then sVal = CostString(.PoolLimit) 'A0907 + D3199
                        tCellEditLimit.Text = String.Format("<div style='text-align:right'><input type='text' id='tbPoolLimit{0}' class='input form-control' value='{1}' style='width:100%; font-weight:bold;{4}' onfocus='onFocus(this.id, this.value);' onkeyup='onKeyUp(this.id, -1);' onblur='onBlur(this.id, ""pool_limit"", ""{2}"", -1);' TabIndex='{3}'></div>", idx, SafeFormString(sVal), .ID, GetTabIndex(0, 2 * idx_tab + If(RA_UseFPPrty, 0, -1)), IIf(.Enabled, "", "font-style:italic; color:#888888; background:#f5f5f5;"))    ' D4367 + D4568 + D7092 + D7093
                        tCellEditLimit.Attributes.Add("clip_data_id", CInt(.ID).ToString) ' D3638
                        tCellEditLimit.Attributes.Add("clip_data", JS_SafeString(CStr(IIf(sVal = "", CLIPBOARD_CHAR_UNDEFINED_VALUE, sVal.Replace("&#160;", " "))))) ' D3638

                        trEdits.Cells.Add(tCellEditLimit)


                        e.Row.Cells(idx).Text = String.Format("<span title='{0}'>{1}</span>", SafeFormString(.Name), ShortString(.Name, 30)) 'A0930 + D4567
                        e.Row.Cells(idx).Text = String.Format("<div class='funding_toogle'><div class='toggle-on-off-btn-info'>
                                                    <label class='txt-info text-capitalize text-nowrap p-0'> <b class='col-blue'>{1}</b></label>&nbsp;&nbsp;
                                                    <label class='toggle-switch-info' onclick='doSwitchFP(" + .ID.ToString + "," + Bool2JS(.Enabled) + ")'> <input type='checkbox' id='toggle-list-view'> <span class='toggle-slider-info'></span>
                                                    </label><small class='fw-normal ms-2'>Ignore</small></div></div>",
                                                              SafeFormString(.Name), ShortString(.Name, 30)) 'A0930 + D4567
                        ' D4367 ===
                        If (Not .Enabled) Then
                            e.Row.Cells(idx).Font.Strikeout = True
                            e.Row.Cells(idx).Font.Italic = True
                            e.Row.Cells(idx).BackColor = Color.FromArgb(1, 210, 210, 210)
                        End If
                        ' D4367 ==
                        'CopyPasteHeaderButtons(e.Row.Cells(idx), .ID, False, False, True, Not .Enabled, .Name)   ' D4566

                        Dim span As Integer = If(RA_UseFPPrty, 2, 1)   ' D7093
                        If FPEnabled Then   ' D3431 + D6484
                            ' D2921 ===
                            Dim Allocated As Double = 0
                            If RA.Solver.SolverState = raSolverState.raSolved Then
                                For Each tAlt As RAAlternative In Scenario.Alternatives
                                    Dim tVal As Double = .GetAlternativeAllocatedValue(tAlt.ID) ' D3410
                                    If tAlt.DisplayFunded > 0 AndAlso tVal <> UNDEFINED_INTEGER_VALUE Then Allocated += tVal 'A0939 + D3427
                                Next
                            End If
                            ' D2921 ==

                            Dim tCellAllocated As New TableCell
                            tCellAllocated.HorizontalAlign = HorizontalAlign.Right
                            Dim sAlloc As String = CostString(Allocated)
                            If RA_ShowTinyBars AndAlso .PoolLimit > 0 Then sAlloc += "<br>" + AddBar("", Allocated / .PoolLimit,, 80)   ' D6580
                            tCellAllocated.Text = String.Format("<div style='text-align:right; padding:0px 2px;'>{0}</div>", IIf(RA.Solver.SolverState = raSolverState.raSolved AndAlso .Enabled, sAlloc, "&nbsp;")) ' D2921 + D4367 + D6580
                            trEdits.Cells.Add(tCellAllocated)

                            e.Row.Cells(idx).ColumnSpan = span + 1  ' D7093
                            e.Row.Cells(idx + 1).Visible = False

                            idx += 1    ' D6484
                        Else
                            e.Row.Cells(idx).ColumnSpan = span ' D7092 + D7093
                        End If
                        e.Row.Cells(idx + 1).Visible = False  ' D7093
                        idx += 2    ' D7093
                        ' D3431 ==
                    End With
                Next

                e.Row.Parent.Controls.Add(trLabels)
                e.Row.Parent.Controls.Add(trEdits)
            End If
        End If
        ' D2909 ==

        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim alt = CType(e.Row.DataItem, RAAlternative)

            Dim fIsFunded As Boolean = alt.DisplayFunded > 0 AndAlso RA.Solver.SolverState = raSolverState.raSolved 'A0939
            If PM.IsRiskProject Then 
                fIsFunded = FundedControlsGuids IsNot Nothing AndAlso FundedControlsGuids.IndexOf(New Guid(alt.ID)) <> -1 
            End If
            If RA.Solver.SolverState = raSolverState.raSolved Then
                If Not fIsFunded AndAlso RA_ShowFundedOnly Then e.Row.Visible = False
            End If

            e.Row.Cells(COL_COST).Width = New Unit(100, UnitType.Pixel)
            e.Row.Cells(COL_INDEX).HorizontalAlign = HorizontalAlign.Right 'A1143
            If Not tIndexVisible Then e.Row.Cells(COL_INDEX).CssClass = "ra_hidden" 'A1143

            Dim sID As String = String.Format("tr_{0}{1}", alt.ID, IIf(fIsFunded, "_funded", ""))

            With e.Row.Attributes
                .Remove("id")
                .Add("id", sID)
                .Remove("onmouseover")
                .Add("onmouseover", String.Format("RowHover(this,1,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
                .Remove("onmouseout")
                .Add("onmouseout", String.Format("RowHover(this,0,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
            End With

            With e.Row.Cells(COL_NAME) 'A1143
                .Attributes.Add("style", "padding: 0px 1ex;")
                .ToolTip = e.Row.Cells(COL_NAME).Text 'A1143
                Dim sImg As String = "&nbsp;"
                .Text = String.Format("{0}{1}", sImg, SafeFormString(ShortString(alt.Name, 65, True))) 'A0930
                If fIsFunded Then
                    .Font.Bold = True
                    If alt.DisplayFunded > 0 AndAlso alt.DisplayFunded < 1 Then .Font.Italic = True 'A0939
                End If
            End With

            Dim tbEdit As TextBox = CType(e.Row.FindControl("tbCost"), TextBox)
            If tbEdit IsNot Nothing Then
                ' D7106 ===
                If (isCostConstraint()) Then
                    tbEdit.Text = CostString(alt.Cost) 'A0908 + D3199
                Else
                    Dim tVal As Double = Scenario.Constraints.GetConstraintValue(ActiveConstraintID, alt.ID)
                    If tVal >= (Integer.MinValue >> 1) Then
                        tbEdit.Text = CostString(tVal)
                    Else
                        tbEdit.Text = ""
                    End If
                End If
                ' D7106 ==
                ' D3039 ===
                If sFocusID = "" Then
                    sFocusID = String.Format("setTimeout('theForm.{0}.focus();', 150);", tbEdit.ClientID)   ' D3043
                    ClientScript.RegisterStartupScript(GetType(String), "setFocus", sFocusID, True)
                End If
                ' D3039
                If OPT_ALLOW_COST_EDIT Then ' D2909
                    tbEdit.Attributes.Remove("onfocus")
                    tbEdit.Attributes.Add("onfocus", "onFocus(this.id, this.value);")
                    tbEdit.Attributes.Remove("onblur")
                    tbEdit.Attributes.Add("onblur", String.Format("onBlur(this.id, 'cost', '{0}', {1});", alt.ID.ToString, If(isCostConstraint(), 0, -1)))  ' D7109
                    tbEdit.Attributes.Remove("onkeyup")
                    tbEdit.Attributes.Add("onkeyup", String.Format("onKeyUp(this.is, 0);"))
                    tbEdit.TabIndex = GetTabIndex(e.Row.RowIndex + 2, 0) ' D4568
                Else
                    tbEdit.Enabled = False
                End If
            End If

            ' D2909 ===
            With ActiveFundingPools()

                If .Pools.Count > 0 Then
                    Dim idx As Integer = COL_POOLS_START_IDX
                    Dim idx_tab As Integer = 0  ' D4568 
                    For Each tID As Integer In .GetPoolsOrderAsList
                        idx_tab += 1  ' D4568
                        If .Pools.ContainsKey(tID) Then
                            Dim tFP As RAFundingPool = .Pools(tID)
                            Dim tVal As Double = tFP.GetAlternativeValue(alt.ID)   ' D3410 + D3427
                            Dim tPVal As Double = tFP.GetAlternativePriority(alt.ID)   ' D7092
                            Dim tAVal As Double = tFP.GetAlternativeAllocatedValue(alt.ID)
                            Dim sVal As String = ""
                            If tVal <> UNDEFINED_INTEGER_VALUE Then sVal = CostString(tVal) 'A0907 + D3199
                            ' D7093 ===
                            If RA_UseFPPrty Then
                                ' D7092 ===
                                Dim sPVal As String = ""
                                If tPVal <> UNDEFINED_INTEGER_VALUE Then sPVal = Double2String(tPVal, RA.Scenarios.GlobalSettings.Precision)
                                e.Row.Cells(idx).Text = String.Format("<input type='text' id='tbPool{0}p{1}' class='input form-control' value='{2}' style='width:100%;{6}{7}' onfocus='onFocus(this.id, this.value);' onkeyup='onKeyUp(this.id, -1);' onblur='onBlur(this.id, ""prty"", ""{3}&cid={4}"", 1);' TabIndex='{5}'>", idx, alt.ID.Substring(0, 6), SafeFormString(sPVal), alt.ID, tFP.ID, GetTabIndex(e.Row.RowIndex + 2, 2 * idx_tab + If(RA_UseFPPrty, 0, -1)), IIf(fIsFunded, " background:#d1e6b5;", ""), IIf(tFP.Enabled, "", "font-style:italic; color:#555555; background:#f5f5f5;"))
                                'e.Row.Cells(idx).Attributes.Add("clip_data_id", CInt(tID).ToString)
                                'e.Row.Cells(idx).Attributes.Add("clip_data", JS_SafeString(CStr(IIf(sPVal = "", CLIPBOARD_CHAR_UNDEFINED_VALUE, sPVal.Replace("&#160;", " ")))))
                                ' D7092 ==
                            Else
                                e.Row.Cells(idx).Visible = False  ' D7098
                            End If
                            ' D7093 ==
                            e.Row.Cells(idx + 1).Text = String.Format("<input type='text' id='tbPool{0}a{1}' class='input form-control' value='{2}' style='width:100%;{6}{7}' onfocus='onFocus(this.id, this.value);' onkeyup='onKeyUp(this.id, -1);' onblur='onBlur(this.id, ""pool"", ""{3}&cid={4}"", -1);' TabIndex='{5}'>", idx, alt.ID.Substring(0, 6), SafeFormString(sVal), alt.ID, tFP.ID, GetTabIndex(e.Row.RowIndex + 2, 2 * idx_tab + If(RA_UseFPPrty, 0, -1) + 1), IIf(fIsFunded, " background:#d1e6b5;", ""), IIf(tFP.Enabled, "", "font-style:italic; color:#555555; background:#f5f5f5;"))  ' D3038 + D4367 + D4568
                            e.Row.Cells(idx + 1).Attributes.Add("clip_data_id", CInt(tID).ToString) ' D3638
                            e.Row.Cells(idx + 1).Attributes.Add("clip_data", JS_SafeString(CStr(IIf(sVal = "", CLIPBOARD_CHAR_UNDEFINED_VALUE, sVal.Replace("&#160;", " "))))) ' D3638
                            If FPEnabled Then   ' D3431 + D6484
                                e.Row.Cells(idx + 2).Text = "&nbsp;"
                                If RA.Solver.SolverState = raSolverState.raSolved Then e.Row.Cells(idx + 2).Text = CStr(IIf(tAVal <> UNDEFINED_INTEGER_VALUE, CostString(tAVal), "&nbsp;")) ' D2921 + A0907 + D3199 + A0939 + D3427
                                e.Row.Cells(idx + 2).HorizontalAlign = HorizontalAlign.Right
                                ' D4367 ===
                                If Not tFP.Enabled Then
                                    e.Row.Cells(idx + 2).ForeColor = Color.FromArgb(255, 120, 120, 120)
                                    e.Row.Cells(idx + 2).Font.Italic = True
                                End If
                                ' D4367 ==
                                idx += 1    ' D3431
                            End If
                        End If
                        idx += 2    ' D3431 + D7092
                    Next
                End If
            End With
            ' D2909 ==

            If fIsFunded Then
                For Each Cell As TableCell In e.Row.Cells
                    Cell.BackColor = Color.FromArgb(209, 230, 181)
                    ' D3038 ===
                    For Each tCtrl As Object In Cell.Controls
                        If TypeOf (tCtrl) Is TextBox Then CType(tCtrl, TextBox).BackColor = Color.FromArgb(209, 230, 181)
                        If TypeOf (tCtrl) Is CheckBox Then CType(tCtrl, CheckBox).BackColor = Color.FromArgb(209, 230, 181)
                    Next
                    ' D3038 ==
                Next
            End If
        End If

        ' D6580 ===
        If e.Row.RowType = DataControlRowType.Footer Then
            Dim fIsFunded As Boolean = RA.Solver.SolverState = raSolverState.raSolved
            ' D7106 ===
            Dim tTotal As Double = 0
            If isCostConstraint() Then
                tTotal = Scenario.Alternatives.Sum(Function(a) If(a.Cost <> UNDEFINED_INTEGER_VALUE, a.Cost, 0))
            Else
                Dim tConstr As RAConstraint = GetActiveConstraint()
                If tConstr IsNot Nothing Then
                    tTotal = tConstr.TotalCost(Scenario.Alternatives)
                End If
            End If
            e.Row.Cells(COL_INDEX).Text = ""
            e.Row.Cells(COL_NAME).Text = String.Format("<div style='text-align:right; font-weight:bold;'><i>{0}:</i></div>", ResString("lblRATotal"))
            e.Row.Cells(COL_COST).Text = String.Format(_TPL_TOTAL, CostString(tTotal), "padding: 0px 2px;")
            ' D7106 ===

            With ActiveFundingPools()

                If .Pools.Count > 0 Then
                    Dim idx As Integer = COL_POOLS_START_IDX
                    For Each tID As Integer In .GetPoolsOrderAsList
                        If .Pools.ContainsKey(tID) Then
                            Dim tFP As RAFundingPool = .Pools(tID)
                            If RA_UseFPPrty Then
                                e.Row.Cells(idx).Text = "&nbsp;"    ' D7092
                            Else
                                e.Row.Cells(idx).Visible = False  ' D7098
                            End If
                            e.Row.Cells(idx + 1).Text = String.Format(_TPL_TOTAL, CostString(tFP.Values.Sum(Function(a) If(a.Value <> UNDEFINED_INTEGER_VALUE, a.Value, 0))), "")   ' D7092
                            idx += 2    ' D7092
                            If FPEnabled Then
                                e.Row.Cells(idx).Text = ""
                                If fIsFunded AndAlso tFP.Enabled Then
                                    Dim tAlloc As Double = tFP.AllocatedValues.Sum(Function(a) If(a.Value <> UNDEFINED_INTEGER_VALUE, a.Value, 0))
                                    e.Row.Cells(idx).Text = String.Format(_TPL_TOTAL, CostString(tAlloc), "")
                                    If tFP.PoolLimit > 0 Then
                                        Dim AllocPerc As Double = (100 * tAlloc / tFP.PoolLimit)
                                        e.Row.Cells(idx).Text += String.Format(_TPL_PERCENTAGE, AllocPerc.ToString("F1") + "%", "")
                                    End If
                                End If
                                idx += 1    ' D7092
                            End If
                        End If
                    Next
                End If
            End With
        End If

    End Sub

    Private Function AddBar(sText As String, tVal As Double, Optional fLight As Boolean = False, Optional fBarWidth As Integer = OPT_BAR_WIDTH) As String
        Dim sVal As String = sText
        If RA_ShowTinyBars Then
            If tVal > 1 Then tVal = 1
            If tVal < 0 Then tVal = 0
            Dim L = Math.Floor(tVal * fBarWidth)
            Dim sImg As String = String.Format("<img src='{0}' width='{{0}}' height={1} title='' border=0>", BlankImage, OPT_BAR_HEIGHT)
            sVal += String.Format(If(sVal = "", "", "<br>") + If(L > 0, "<span style='display:inline-block; line-height:{2}px; height:{2}px; width:{0}px; border-bottom:{2}px solid {3}; margin-top:-{2}px;'>" + String.Format(sImg, L) + "</span>", "") + If(L < fBarWidth, "<span style='display:inline-block; line-height:{2}px; height:{2}px; width:{1}px; border-bottom:{2}px solid {4}; margin-top:-{2}px;'>" + String.Format(sImg, fBarWidth - L) + "</span>", ""), L, fBarWidth - L, OPT_BAR_HEIGHT, OPT_BAR_COLOR_FILLED, If(fLight, "#ffffff", OPT_BAR_COLOR_EMPTY))
        End If
        Return sVal
    End Function
    ' D6580 ==

    Protected Sub radAjaxManagerMain_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxManagerMain.AjaxRequest
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(e.Argument)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Dim fResetSolver As Boolean = True
        Select Case sAction

            Case "solve"
                isSolving = True
                GridAlternatives.DataSource = Nothing
                fResetSolver = False

            Case "autosolve"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))   ' Anti-XSS
                RA_AutoSolve = (sVal = "1")
                isSolving = True
                fResetSolver = False

                ' D2909 ===
            Case "edit_pool"    ' using for add new as well (ID=-1)
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim    ' Anti-XSS
                Dim fEnabled As Boolean = GetParam(args, "chk") = "1"   ' D4367
                Dim ID As Integer
                If Integer.TryParse(sID, ID) AndAlso sName <> "" Then
                    Dim RAFP As RAFundingPool = Nothing
                    If ID < 0 Then
                        RAFP = ActiveFundingPools.AddPool(sName)
                    Else
                        If ActiveFundingPools.Pools.ContainsKey(ID) Then RAFP = ActiveFundingPools.Pools(ID)
                    End If
                    If RAFP IsNot Nothing Then
                        RAFP.Name = sName
                        If RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY Then RAFP.Enabled = fEnabled ' D4367
                        SaveRA = True
                        SaveComment = String.Format("{1} '{0}'", ShortString(sName, 40, True), IIf(ID < 0, "Add", "Edit")) ' D3790
                    End If
                End If

            Case "delete_pool"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                Dim ID As Integer
                If Integer.TryParse(sID, ID) AndAlso ActiveFundingPools.Pools.ContainsKey(ID) Then
                    SaveComment = String.Format("Delete '{0}'", ShortString(ActiveFundingPools.Pools(ID).Name, 40, True)) ' D3790
                    ActiveFundingPools.DeletePool(ID)
                    SaveRA = True
                End If
                ' D2882 ==

                ' D4367 ===
            Case "enable_pool"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id"))
                Dim fEnabled As Boolean = GetParam(args, "chk") = "1"
                Dim ID As Integer
                If Integer.TryParse(sID, ID) AndAlso ActiveFundingPools.Pools.ContainsKey(ID) Then
                    Dim RAFP As RAFundingPool = ActiveFundingPools.Pools(ID)
                    If RAFP IsNot Nothing Then RAFP.Enabled = fEnabled
                    SaveRA = True
                    SaveComment = String.Format("{0} '{1}'", IIf(fEnabled, "Enable", "Disable"), ShortString(RAFP.Name, 40, True))
                End If
                ' D4367 ==

                ' D2909 ===
            Case "pool", "prty" ' D7092
                Dim sAlt As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid"))  ' Anti-XSS
                Dim sCID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "cid"))   ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))   ' Anti-XSS
                Dim tCID As Integer = -1
                Dim isVal As Boolean = sAction.ToLower <> "prty"    ' D7092
                If sAlt <> "" AndAlso sCID <> "" AndAlso Integer.TryParse(sCID, tCID) Then
                    Dim tAlt As RAAlternative = Scenario.Alternatives.Find(Function(x) x.ID = sAlt)
                    Dim tFP As RAFundingPool = ActiveFundingPools.GetPoolByID(tCID)
                    If tAlt IsNot Nothing AndAlso tFP IsNot Nothing Then
                        Dim tVal As Double = -1
                        If Not String2Double(sVal, tVal) Then tVal = UNDEFINED_INTEGER_VALUE
                        ' D7092 ===
                        If isVal Then
                            tFP.SetAlternativeValue(tAlt.ID, tVal)
                        Else
                            tFP.SetAlternativePriority(tAlt.ID, tVal)
                        End If
                        SaveComment = String.Format("Edit '{0}'/'{1}' {2}", ShortString(tFP.Name, 40, True), ShortString(tAlt.Name, 40, True), If(isVal, "value", "benefit")) ' D3790
                        ' D7092 ==
                        SaveRA = True
                    End If
                End If

            Case "pool_limit"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid"))   ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))   ' Anti-XSS
                Dim tID As Integer = -1
                If sID <> "" AndAlso Integer.TryParse(sID, tID) Then
                    Dim tFP As RAFundingPool = ActiveFundingPools.GetPoolByID(tID)
                    If tFP IsNot Nothing Then
                        Dim tVal As Double = -1
                        If Not String2Double(sVal, tVal) Then tVal = UNDEFINED_INTEGER_VALUE
                        tFP.PoolLimit = tVal
                        SaveComment = String.Format("Set '{0}' limit", ShortString(tFP.Name, 40, True)) ' D3790
                        SaveRA = True
                    End If
                End If
                ' D2909 ==

                ' D3053 ===
            Case "cost"
                Dim sGUID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid")) ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))   ' Anti-XSS
                ' D7109 ===
                Dim tVal As Double = UNDEFINED_INTEGER_VALUE
                Dim fParsed As Boolean = String2Double(sVal, tVal)
                If sVal = "" AndAlso Not isCostConstraint() Then tVal = UNDEFINED_INTEGER_VALUE
                If sGUID <> "" AndAlso (Not isCostConstraint() OrElse fParsed) Then
                    ' D7109 ==
                    Dim tAlt As RAAlternative = Scenario.Alternatives.Find(Function(x) x.ID = sGUID)
                    If tAlt IsNot Nothing Then
                        ' D7106 ===
                        If (isCostConstraint()) Then
                            If tAlt.Cost >= 0 Then
                                RA.SetAlternativeCost(tAlt.ID, tVal)
                                'tAlt.Cost = tVal
                                SaveComment = String.Format("Set cost for '{0}'", ShortString(tAlt.Name, 40, True)) ' D3790
                                SaveRA = True
                            End If
                        Else
                            Dim tConstr As RAConstraint = GetActiveConstraint()
                            If tConstr IsNot Nothing Then
                                Scenario.Constraints.SetConstraintValue(tConstr.ID, tAlt.ID, tVal)
                                SaveComment = String.Format("Set constraint '{1}' value for '{0}'", ShortString(tAlt.Name, 40, True), tConstr.Name)
                                SaveRA = True
                            End If
                        End If
                        ' D7106 ==
                    End If
                End If
                ' D3053 ==

                ' D3638 ===
            Case "paste_column"
                Dim fHasChanges As Boolean = False
                Dim tID As Integer = -1
                If Integer.TryParse(GetParam(args, "column"), tID) Then
                    Dim tFP As RAFundingPool = ActiveFundingPools.GetPoolByID(tID)
                    If tFP IsNot Nothing Then
                        Dim cells As String() = GetParam(args, "data").Replace(vbCr, "").Split(CChar(vbLf))
                        Dim alts_count As Integer = Scenario.Alternatives.Count
                        Dim Idx As Integer = 0

                        Dim tDblValue As Double = 0
                        If cells.Count > alts_count Then ' Check and set pool limit
                            If String.IsNullOrEmpty(cells(0)) OrElse Not String2Double(cells(0), tDblValue) Then tDblValue = UNDEFINED_INTEGER_VALUE
                            tFP.PoolLimit = tDblValue
                            Idx += 1
                        End If

                        For i As Integer = 0 To alts_count - 1
                            If Idx < cells.Count Then
                                Dim tAlt As RAAlternative = Scenario.Alternatives(i)
                                If String.IsNullOrEmpty(cells(Idx)) OrElse Not String2Double(cells(Idx), tDblValue) Then tDblValue = UNDEFINED_INTEGER_VALUE
                                tFP.SetAlternativeValue(tAlt.ID, tDblValue)
                                Idx += 1
                            End If
                            fHasChanges = True  ' D3797
                        Next

                        If fHasChanges Then App.ActiveProject.SaveRA("Edit Funding Pools", True, , String.Format("Paste column values ('{0}')", ShortString(tFP.Name, 35, True))) ' D3797
                    End If
                End If
                If fHasChanges Then
                    fResetSolver = True
                    GridAlternatives.DataSource = Nothing
                End If
                ' D3638 ==

            Case "settings"
                fResetSolver = False
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim.ToLower    ' Anti-XSS
                Dim sChecked As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "chk"))   ' Anti-XSS
                Dim fVal As Boolean = sChecked = "1"
                Select Case sName
                    Case "showbars"
                        RA_ShowTinyBars = fVal
                    Case "showfunded"
                        RA_ShowFundedOnly = fVal
                    Case "showprty"             ' D7093
                        RA_UseFPPrty = fVal    ' D7093
                        App.ActiveProject.ProjectManager.Parameters.Save()  ' D7098
                End Select

                ' D4365 ===
            Case "exhausted"
                Dim chk As Boolean = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")) = "1"
                App.ActiveProject.ProjectManager.Parameters.RAFundingPoolsExhausted = chk
                App.ActiveProject.SaveProjectOptions(String.Format("Funding Pools exhausted: {0}", Bool2YesNo(chk)))
                fResetSolver = True
                ' D4365 ==

                ' D4367 ===
            Case "pools_reorder"
                Dim sLst As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "lst")).Trim
                Dim IDs As String() = sLst.Split(CChar(","))
                If IDs.Count = ActiveFundingPools.Pools.Count Then
                    ActiveFundingPools.SetPoolsOrderByString(sLst)
                    SaveComment = "Reorder funding pools"
                    SaveRA = True
                    If Not App.ActiveProject.ProjectManager.Parameters.RAFundingPoolsExhausted Then fResetSolver = False
                End If
                ' D4367 ==

        End Select
        If Not isSolving AndAlso fResetSolver Then RA.Solver.ResetSolver()
    End Sub

    'A1389 ===
    Private Sub RAFundingPools_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        If App.isRiskEnabled Then CurrentPageID = _PGID_RISK_OPTIMIZER_FUNDING_POOLS
    End Sub
    'A1389 ==

    Private Sub RAFundingPools_Init(sender As Object, e As EventArgs) Handles Me.Init
        RA.Load()
    End Sub


End Class