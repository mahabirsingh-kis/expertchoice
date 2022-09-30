Imports System.Data
Imports Microsoft.Reporting.WebForms
'A0909

Partial Class RAReportPage
    Inherits clsComparionCorePage

    Public Enum RAReportIDs As Integer 'A0909
        rpModelSpecification = 0
        rpConstraintSummary = 1
        rpRelevantConstraints = 2
        rpSolverReport = 3 'A0923
    End Enum

    Private ReportTitles() As String = {"titleRAReportModelSpec", "titleRAReportCCSum", "titleRAReportRelConstr", "titleRASolverReport"}   ' D3195 + A0923
    Public ActiveReportID As RAReportIDs = RAReportIDs.rpModelSpecification  ' D3195

    'each dataset has 4 columns:
    Const IDX_ID As Integer = 0
    Const IDX_NAME As Integer = 1
    Const IDX_CONDITION As Integer = 2
    Const IDX_VALUE As Integer = 3

    Const REPORT_ALT_PREFIX As String = "A"

    'datasets
    Const IDX_PARAMS As Integer = 0 'common dataset for all types of reports
    Const IDX_ALTS As Integer = 1 '  common dataset for all types of reports
    Const IDX_MUSTS As Integer = 2
    Const IDX_MUST_NOTS As Integer = 3
    Const IDX_CONSTAINTS As Integer = 4
    Const IDX_DEPENDENCIES As Integer = 5
    Const IDX_GROUPS As Integer = 6
    Const IDX_FUND_POOLS As Integer = 7

    Const IDX_RELEVANT As Integer = 2
    Const IDX_IRRELEVANT As Integer = 3

    Public Enum ViewModes
        vmDetails = 0
        vmSummary = 1
    End Enum

    Public Function isTimePeriodsVisible() As Boolean
        Return RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0
    End Function

    ReadOnly Property SESSION_VIEW_MODE As String
        Get
            Return String.Format("RA_ReportViewMode_{0}", App.ProjectID)
        End Get
    End Property

    Public Property ViewMode As ViewModes 'Details / Summary
        Get
            Dim retVal As ViewModes = ViewModes.vmDetails
            Dim s = SessVar(SESSION_VIEW_MODE)
            If Not String.IsNullOrEmpty(s) Then retVal = CType(CInt(s), ViewModes)
            Return retVal
        End Get
        Set(value As ViewModes)
            SessVar(SESSION_VIEW_MODE) = CInt(value).ToString
        End Set
    End Property

    Public Enum ReportModes
        rmAllConstraints = 0
        rmActiveConstraints = 1
    End Enum

    ReadOnly Property SESSION_REPORT_MODE As String
        Get
            Return String.Format("RA_ReportMode_{0}", App.ProjectID)
        End Get
    End Property

    Public Property ReportMode As ReportModes 'All Constraints / Active Constraints
        Get
            Dim retVal As ReportModes = ReportModes.rmAllConstraints
            Dim s = SessVar(SESSION_REPORT_MODE)
            If Not String.IsNullOrEmpty(s) Then retVal = CType(CInt(s), ReportModes)
            Return retVal
        End Get
        Set(value As ReportModes)
            SessVar(SESSION_REPORT_MODE) = CInt(value).ToString
        End Set
    End Property

    Public Property DecimalDigits As Integer
        Get
            'Dim retVal As Integer = CInt(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_SB_DECIMALS_ID, UNDEFINED_USER_ID))
            'If retVal > 5 Then Return 5
            'Return retVal
            Return App.ActiveProject.ProjectManager.Parameters.DecimalDigits
        End Get
        Set(value As Integer)
            'WriteSetting(App.ActiveProject, ATTRIBUTE_RA_SB_DECIMALS_ID, AttributeValueTypes.avtLong, value, "RA Report - Decimal places value changed")
            'SaveSetting(ATTRIBUTE_RA_SB_DECIMALS_ID, AttributeValueTypes.avtLong, value)
            App.ActiveProject.ProjectManager.Parameters.DecimalDigits = value
            App.ActiveProject.ProjectManager.Parameters.Save()
        End Set
    End Property

    Private Sub SaveSetting(ID As Guid, valueType As AttributeValueTypes, value As Object)
        With App.ActiveProject.ProjectManager
            .Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, valueType, value, Guid.Empty, Guid.Empty)
            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
        End With
    End Sub

    Public Sub New()
        MyBase.New(_PGID_RA_REPORT_MODEL_SPEC)
    End Sub

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property

    Protected Sub Page_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        If Not IsPostBack AndAlso Not IsCallback Then
            'If RA.Scenarios.Scenarios.Count = 0 Then RA.Load() ' -D4857
        End If
    End Sub

    Protected Sub Page_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        With Master
            .EnableViewState = True
            .ViewStateMode = ViewStateMode.Inherit
        End With
        ActiveReportID = CType(CInt(CheckVar("mode", 0)), RAReportIDs)
        Select Case ActiveReportID
            Case RAReportIDs.rpConstraintSummary
                CurrentPageID = _PGID_RA_REPORT_CONSTR_SUMMARY
            Case RAReportIDs.rpModelSpecification
                CurrentPageID = _PGID_RA_REPORT_MODEL_SPEC
            Case RAReportIDs.rpRelevantConstraints
                CurrentPageID = _PGID_RA_REPORT_RELEVANT_CONSTR
            Case RAReportIDs.rpSolverReport
                CurrentPageID = _PGID_RA_REPORT_SOLVER_REPORT
        End Select

        If ActiveReportID < 0 OrElse ActiveReportID > CInt(RAReportIDs.rpSolverReport) Then
            ActiveReportID = RAReportIDs.rpModelSpecification
            CurrentPageID = _PGID_RA_REPORT_MODEL_SPEC
        End If
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Dim sSet = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_SET, "")).Trim.ToLower   ' Anti-XSS
        Select Case sSet
            Case "scenario", "viewmode", "reportmode", "decimals"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    RA.Scenarios.ActiveScenarioID = ID
                    RA.Solver.ResetSolver()
                End If

                Dim iVM As Integer = CInt(ViewMode)
                Dim VM As Integer = CheckVar("vm", iVM)
                If VM <> iVM Then ViewMode = CType(VM, ViewModes)

                Dim iRM As Integer = CInt(ReportMode)
                Dim RM As Integer = CheckVar("rm", iRM)
                If RM <> iRM Then ReportMode = CType(RM, ReportModes)

                Dim iDCM As Integer = DecimalDigits
                Dim DCM As Integer = CheckVar("dcm", iDCM)
                If DCM <> iDCM Then DecimalDigits = DCM

                Response.Redirect(PageURL(CurrentPageID), True)
        End Select

        If Not IsCallback AndAlso Not IsPostBack Then
            GenerateReport(ActiveReportID)
        End If
    End Sub

    ' D3195 ===
    Public Function GetReportLinks() As String  ' temporary
        Dim sLinks As String = ""
        Dim idx As Integer = 0
        For Each sResName As String In ReportTitles
            Dim sName = ResString(sResName)
            If idx <> CInt(ActiveReportID) Then sName = String.Format("<a href='?mode={0}' class='actions'>{1}</a>", idx, sName) Else sName = String.Format("<b>{0}</b>", sName)
            sLinks += CStr(IIf(sLinks = "", "", " | ")) + sName
            idx += 1
        Next
        Return sLinks
    End Function
    ' D3195 ==

    Public Const OPT_SCENARIO_LEN As Integer = 45       'A0965
    Public Const OPT_DESCRIPTION_LEN As Integer = 200   'A0965

    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tScen As Integer In RA.Scenarios.Scenarios.Keys
            sRes += String.Format("<option value='{0}'{2}>{1}</option>", tScen, ShortString(SafeFormString(RA.Scenarios.Scenarios(tScen).Name), OPT_SCENARIO_LEN, True) + CStr(IIf(String.IsNullOrEmpty(SafeFormString(RA.Scenarios.Scenarios(tScen).Description)), "", String.Format(" ({0})", ShortString(SafeFormString(RA.Scenarios.Scenarios(tScen).Description), OPT_DESCRIPTION_LEN)))), IIf(tScen = RA.Scenarios.ActiveScenarioID, " selected", ""))
        Next
        Return String.Format("<select id='cbScenarios' style='width:130px; margin-top:3px; margin-right:2px;' onchange='onSetScenario(this.value);'>{0}</select>", sRes)
    End Function

    'A0909 ===
    Public Function HasConstraintsAndFundingPools() As Boolean
        Return RA.Scenarios.ActiveScenario.Constraints.Constraints.Count > 0 OrElse RA.Scenarios.ActiveScenario.FundingPools.Pools.Count > 0
    End Function

    Private Sub GenerateReport(reportID As RAReportIDs)
        Dim reportFileName As String = "ModelSpecification.rdlc"
        Select Case reportID
            Case RAReportIDs.rpModelSpecification, RAReportIDs.rpConstraintSummary
                reportFileName = "ModelSpecification.rdlc"
            Case RAReportIDs.rpConstraintSummary
                reportFileName = "ConstraintSummary.rdlc"
            Case RAReportIDs.rpRelevantConstraints
                reportFileName = "RelevantConstraints.rdlc"
            Case RAReportIDs.rpSolverReport
                reportFileName = "SolverReport.rdlc"
        End Select
        'SetPageTitle(String.Format("{0} - {1}", PageMenuItem(CurrentPageID), ResString(ReportTitles(CInt(reportID))))) ' D3195
        SetPageTitle(ResString(ReportTitles(CInt(reportID)))) 'A0969
        ReportViewerMain.Reset()
        ReportViewerMain.ProcessingMode = ProcessingMode.Local
        ReportViewerMain.LocalReport.ReportPath = Server.MapPath("~/Project/RA/Reports/" + reportFileName)
        ReportViewerMain.LocalReport.DataSources.Clear()
        GetReportData(reportID)
        ReportViewerMain.DataBind()
        ReportViewerMain.LocalReport.Refresh()
        ReportViewerMain.ShowExportControls = App.isExportAvailable ' D3324
        ReportViewerMain.ShowPrintButton = App.isExportAvailable    ' D3324
    End Sub

    Private Sub GetReportData(reportID As RAReportIDs)
        Dim ds As New DataSet
        Dim dt As DataTable
        Dim dr As DataRow

        ds.Tables.Add(New DataTable() With {.TableName = "ds_params"})
        ds.Tables.Add(New DataTable() With {.TableName = "ds_alts"})

        If reportID = RAReportIDs.rpRelevantConstraints Then
            ds.Tables.Add(New DataTable() With {.TableName = "ds_relevant"})
            ds.Tables.Add(New DataTable() With {.TableName = "ds_irrelevant"})
        Else
            ds.Tables.Add(New DataTable() With {.TableName = "ds_musts"})
            ds.Tables.Add(New DataTable() With {.TableName = "ds_must_nots"})
            ds.Tables.Add(New DataTable() With {.TableName = "ds_constraints"})
            ds.Tables.Add(New DataTable() With {.TableName = "ds_dependencies"})
            ds.Tables.Add(New DataTable() With {.TableName = "ds_groups"})
            ds.Tables.Add(New DataTable() With {.TableName = "ds_funding_pools"})
        End If

        For Each t As DataTable In ds.Tables
            t.Columns.Add(New DataColumn("ID", GetType(String)))
            t.Columns.Add(New DataColumn("Name", GetType(String)))
            t.Columns.Add(New DataColumn("Condition", GetType(String)))
            t.Columns.Add(New DataColumn("Value", GetType(String)))
        Next

        Dim sSolverReport As Object = "" 'A0970

        'params
        dt = ds.Tables(IDX_PARAMS)
        With RA.Scenarios.ActiveScenario
            'params - defined
            dr = dt.NewRow()
            dr(IDX_NAME) = "Defined:"
            dr(IDX_VALUE) = CStr(IIf(.Alternatives.Sum(Function(a) CInt(IIf(a.Must, 1, 0))) > 0, "1", "0")) + _
                CStr(IIf(.Alternatives.Sum(Function(a) CInt(IIf(a.MustNot, 1, 0))) > 0, "1", "0")) + _
                CStr(IIf(.Constraints.Constraints.Count > 0, "1", "0")) + _
                CStr(IIf(.Dependencies.Dependencies.Count > 0, "1", "0")) + _
                CStr(IIf(.Groups.Groups.Count > 0, "1", "0")) + _
                CStr(IIf(.FundingPools.Pools.Count > 0, "1", "0")) + _
                CStr(IIf(.Alternatives.Sum(Function(a) a.RiskOriginal) > 0, "1", "0"))
            dt.Rows.Add(dr)
            'params - ignore
            dr = dt.NewRow()
            dr(IDX_NAME) = "Ignore:"
            dr(IDX_VALUE) = CStr(IIf(Not .Settings.Musts, "1", "0")) + _
                CStr(IIf(Not .Settings.MustNots, "1", "0")) + _
                CStr(IIf(Not .Settings.CustomConstraints, "1", "0")) + _
                CStr(IIf(Not .Settings.Dependencies, "1", "0")) + _
                CStr(IIf(Not .Settings.Groups, "1", "0")) + _
                CStr(IIf(Not .Settings.FundingPools, "1", "0")) + _
                CStr(IIf(Not .Settings.Risks, "1", "0"))
            dt.Rows.Add(dr)
            'params - base case
            dr = dt.NewRow()
            dr(IDX_NAME) = "Base Case Includes:"
            dr(IDX_VALUE) = CStr(IIf(.Settings.UseBaseCase AndAlso .Settings.BaseCaseForMusts AndAlso False, "1", "0")) + _
                CStr(IIf(.Settings.UseBaseCase AndAlso .Settings.BaseCaseForMustNots AndAlso False, "1", "0")) + _
                CStr(IIf(.Settings.UseBaseCase AndAlso .Settings.BaseCaseForConstraints AndAlso False, "1", "0")) + _
                CStr(IIf(.Settings.UseBaseCase AndAlso .Settings.BaseCaseForDependencies AndAlso False, "1", "0")) + _
                CStr(IIf(.Settings.UseBaseCase AndAlso .Settings.BaseCaseForGroups, "1", "0")) + _
                CStr(IIf(.Settings.UseBaseCase AndAlso .Settings.BaseCaseForFundingPools AndAlso False, "1", "0")) + _
                "0" 'Risks - CStr(IIf(.Settings.UseBaseCase AndAlso .Settings.BaseCaseForRisks AndAlso False, "1", "0"))
            dt.Rows.Add(dr)
        End With

        Dim Alts As IEnumerable(Of RAAlternative) = RA.Scenarios.ActiveScenario.Alternatives.OrderBy(Function(alt) alt.SortOrder)

        'alts
        dt = ds.Tables(IDX_ALTS)
        For Each tAlt As RAAlternative In Alts
            dr = dt.NewRow()
            dr(IDX_ID) = GetShortAltName(tAlt)
            dr(IDX_NAME) = tAlt.Name
            dt.Rows.Add(dr)
        Next

        Dim sDecimals As String = "F" + CStr(DecimalDigits)

        Select Case reportID
            Case RAReportIDs.rpRelevantConstraints
                ''Budget
                'Dim dt_irr As DataTable = ds.Tables(IDX_IRRELEVANT)
                'dr = dt_irr.NewRow()
                'dr(IDX_NAME) = "Total Budget of"
                'dr(IDX_VALUE) = CostString(RA.Scenarios.ActiveScenario.Budget) 'Budget
                'dt_irr.Rows.Add(dr)

                Dim tmpActiveScenario As Integer = RA.Scenarios.ActiveScenarioID
                'get a copy of active scenario
                Dim CopyScenario As RAScenario = RA.Scenarios.AddScenario(RA.Scenarios.ActiveScenarioID)
                RA.Scenarios.ActiveScenarioID = CopyScenario.ID

                ' get relevant budget
                Dim dRelevanBudget As Double = RA.Solver.GetMaxRelevantBudget()

                ' reset solver
                RA.Solver.ResetSolver()
                RA.Solver.Solve(raSolverExport.raNone)  ' D3236

                ' write budget constraint
                Dim isBudgetRelevant As Boolean = RA.Solver.SolverState = raSolverState.raSolved AndAlso CopyScenario.Budget < dRelevanBudget
                'Dim isBudgetRelevantExact As Boolean = RA.Solver.SolverState = raSolverState.raSolved AndAlso CopyScenario.Budget = dRelevanBudget
                'AddRelevancyData("Total budget of " + CostString(CopyScenario.Budget) + CStr(IIf(isBudgetRelevant, " is relevant. ", "")) + CStr(IIf(isBudgetRelevant, IIf(Not isBudgetRelevantExact, "It would remain relevant until increased by " + CostString(dRelevanBudget - CopyScenario.Budget) + " (up to: " + CostString(dRelevanBudget) + ").", ""), " is irrelevant" + CStr(IIf(dRelevanBudget > 0, " (maximum relevant budget is " + CostString(dRelevanBudget) + ").", ".")))), "", ds, isBudgetRelevant)
                AddRelevancyData("Total budget of " + CostString(CopyScenario.Budget) + CStr(IIf(isBudgetRelevant, " is relevant. ", "")) + CStr(IIf(isBudgetRelevant, "It would remain relevant until increased by " + CostString(dRelevanBudget - CopyScenario.Budget) + " (up to: " + CostString(dRelevanBudget) + ").", " is irrelevant.")), "", ds, isBudgetRelevant)

                If RA.Solver.SolverState = raSolverState.raSolved Then
                    With CopyScenario
                        Dim origAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                        'Musts
                        If .Settings.Musts Then
                            For Each tAlt As RAAlternative In .Alternatives
                                If tAlt.Must Then
                                    tAlt.Must = False

                                    RA.Solver.ResetSolver()
                                    RA.Solver.Solve(raSolverExport.raNone) ' D3236

                                    tAlt.Must = True

                                    Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                    AddRelevancyData(ParseString("Must for %%alternative%%"), GetShortAltName(tAlt), ds, Not Equal(origAltsFunded, curAltsFunded))
                                End If
                            Next
                        End If

                        'Must nots
                        If .Settings.MustNots Then
                            For Each tAlt As RAAlternative In .Alternatives
                                If tAlt.MustNot Then
                                    tAlt.MustNot = False

                                    RA.Solver.ResetSolver()
                                    RA.Solver.Solve(raSolverExport.raNone) ' D3236

                                    tAlt.MustNot = True

                                    Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                    AddRelevancyData(ParseString("Must-not for %%alternative%%"), GetShortAltName(tAlt), ds, Not Equal(origAltsFunded, curAltsFunded))
                                End If
                            Next
                        End If

                        'Custom Constraints
                        If .Settings.CustomConstraints Then
                            For Each Constraint As RAConstraint In .Constraints.Constraints.Values
                                If Constraint.Enabled AndAlso (Constraint.MinValueSet OrElse Constraint.MaxValueSet) Then
                                    Constraint.Enabled = False

                                    RA.Solver.ResetSolver()
                                    RA.Solver.Solve(raSolverExport.raNone) ' D3236

                                    Constraint.Enabled = True

                                    Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                    AddRelevancyData("Custom Constraint", """" + Constraint.Name + """", ds, Not Equal(origAltsFunded, curAltsFunded))
                                End If
                            Next
                        End If

                        'Dependencies
                        If .Settings.Dependencies Then
                            .CheckDependencies(.Dependencies)
                            .CheckDependencies(.TimePeriodsDependencies)
                            
                            'Dim checkedList As New List(Of RADependency)

                            Dim j As Integer = 0
                            While j < .Dependencies.Dependencies.Count
                                Dim d1 = .Dependencies.Dependencies(j) 
                                Dim dep = d1.Value 
                                If dep = RADependencyType.dtMutuallyDependent OrElse dep = RADependencyType.dtMutuallyExclusive Then
                                    Dim k As Integer = j + 1
                                    While k < .Dependencies.Dependencies.Count
                                        Dim d2 = .Dependencies.Dependencies(k)
                                        If d2.SecondAlternativeID = d1.FirstAlternativeID AndAlso d2.FirstAlternativeID = d1.SecondAlternativeID AndAlso d2.Value = d1.Value Then
                                            .Dependencies.Dependencies.RemoveAt(k)
                                        Else
                                            k += 1
                                        End If
                                    End While
                                End If
                                    j += 1
                            End While

                            Dim isTP As Boolean = isTimePeriodsVisible
                            Dim tpDep As RADependency = Nothing

                            For i As Integer = 0 To .Dependencies.Dependencies.Count - 1
                                Dim dep As RADependency = .Dependencies.Dependencies(i)
                                If dep.Enabled Then 
                                    dep.Enabled = False
                                    'Dim alt0 As String = dep.FirstAlternativeID
                                    'Dim alt1 As String = dep.SecondAlternativeID

                                    'checkedList.Add(dep)

                                    'Dim tmpList As List(Of RADependency) = .Dependencies.Dependencies.Where(Function (d) d.FirstAlternativeID = alt0 AndAlso d.SecondAlternativeID = alt1 AndAlso d.Enabled AndAlso d IsNot dep AndAlso Not checkedList.Contains(d)).ToList
                                    'tmpList.ForEach(Sub (d) d.Enabled = False)
                                    'tmpList.ForEach(Sub (d) checkedList.Add(d))

                                    tpDep = Nothing
                                    '-A1224 If isTP Then 
                                        tpDep = .TimePeriodsDependencies.GetDependency(dep.FirstAlternativeID, dep.SecondAlternativeID)
                                        If tpDep IsNot Nothing AndAlso tpDep.Enabled Then 
                                            tpDep.Enabled = False
                                        Else
                                            tpDep = Nothing
                                        End If
                                    'End If

                                    RA.Solver.ResetSolver()
                                    RA.Solver.Solve(raSolverExport.raNone)  ' D3236

                                    dep.Enabled = True
                                    If tpDep IsNot Nothing Then 
                                        tpDep.Enabled = True
                                    End If
                                    'tmpList.ForEach(Function (d) d.Enabled = True)
                                    If Not isTP Then tpDep = Nothing ' to skip TP dependency name in the report

                                    Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                    AddRelevancyData("Dependency", GetDependencyName(dep, tpDep, isTP), ds, Not Equal(origAltsFunded, curAltsFunded))
                                End If
                            Next

                            'checkedList.Clear()
                        End If

                        'Groups
                        If .Settings.Groups Then
                            For Each grp As RAGroup In .Groups.Groups.Values
                                If grp.Enabled Then
                                    grp.Enabled = False

                                    RA.Solver.ResetSolver()
                                    RA.Solver.Solve(raSolverExport.raNone) ' D3236

                                    grp.Enabled = True

                                    Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                    AddRelevancyData("Group", GetGroupName(grp), ds, Not Equal(origAltsFunded, curAltsFunded))
                                End If
                            Next
                        End If

                        'Funding Pools
                        If .Settings.FundingPools Then
                            For Each pool As RAFundingPool In .FundingPools.Pools.Values
                                If pool.Enabled Then
                                    pool.Enabled = False

                                    RA.Solver.ResetSolver()
                                    RA.Solver.Solve(raSolverExport.raNone) ' D3236

                                    pool.Enabled = True

                                    Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                    AddRelevancyData("Funding Pool", pool.Name, ds, Not Equal(origAltsFunded, curAltsFunded))
                                End If
                            Next
                        End If

                    End With
                End If

                RA.Scenarios.DeleteScenario(CopyScenario.ID)
                RA.Scenarios.ActiveScenarioID = tmpActiveScenario
            Case RAReportIDs.rpSolverReport
                RA.Solver.ResetSolver()
                RA.Solver.Solve(raSolverExport.raReport, sSolverReport)    ' D3236
            Case Else
                'musts
                If RA.Scenarios.ActiveScenario.Settings.Musts Then
                    dt = ds.Tables(IDX_MUSTS)
                    For Each tAlt As RAAlternative In Alts.Where(Function(alt) alt.Must)
                        dr = dt.NewRow()
                        dr(IDX_ID) = tAlt.ID
                        dr(IDX_NAME) = GetShortAltName(tAlt)
                        dt.Rows.Add(dr)
                    Next
                End If

                'must nots
                If RA.Scenarios.ActiveScenario.Settings.MustNots Then
                    dt = ds.Tables(IDX_MUST_NOTS)
                    For Each tAlt As RAAlternative In Alts.Where(Function(alt) alt.MustNot)
                        dr = dt.NewRow()
                        dr(IDX_ID) = tAlt.ID
                        dr(IDX_NAME) = REPORT_ALT_PREFIX + tAlt.SortOrder.ToString 'tAlt.Name
                        dt.Rows.Add(dr)
                    Next
                End If

                'custom constraints
                If ReportMode = ReportModes.rmAllConstraints OrElse RA.Scenarios.ActiveScenario.Settings.CustomConstraints Then
                    dt = ds.Tables(IDX_CONSTAINTS)
                    For Each kvp As KeyValuePair(Of Integer, RAConstraint) In RA.Scenarios.ActiveScenario.Constraints.Constraints
                        Dim cc As RAConstraint = kvp.Value
                        Dim i As Integer = 0
                        While i < cc.AlternativesData.Count
                            Dim alt As String = cc.AlternativesData.Keys(i)
                            If RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(alt) Is Nothing Then
                                cc.AlternativesData.Remove(alt)
                            Else
                                i += 1
                            End If
                        End While

                        Dim sNotConstrained As String = CStr(IIf(cc.MinValue = UNDEFINED_INTEGER_VALUE AndAlso cc.MaxValue = UNDEFINED_INTEGER_VALUE, "NOT CONSTRAINED", ""))
                        If Not cc.Enabled Then sNotConstrained = "SOFT CONSTRAINT"
                        If ReportMode = ReportModes.rmAllConstraints OrElse sNotConstrained = "" Then
                            If ViewMode = ViewModes.vmDetails Then
                                dr = dt.NewRow()
                                dr(IDX_NAME) = cc.Name
                                dr(IDX_CONDITION) = ""
                                dr(IDX_VALUE) = String.Format("{0} {1} {2}" + CStr(IIf(sNotConstrained = "", "", " - " + sNotConstrained)), String.Join(" + ", (From alt As KeyValuePair(Of String, Double) In cc.AlternativesData Select alt.Value.ToString(sDecimals) + " * " + REPORT_ALT_PREFIX + RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(alt.Key).SortOrder.ToString).ToArray), IIf(cc.MinValue <> UNDEFINED_INTEGER_VALUE, ">= " + cc.MinValue.ToString("F2"), ""), IIf(cc.MaxValue <> UNDEFINED_INTEGER_VALUE, "<= " + cc.MaxValue.ToString("F2"), ""))
                                dt.Rows.Add(dr)
                            Else
                                'If cc.MinValue <> UNDEFINED_INTEGER_VALUE Then
                                '    dr = dt.NewRow()
                                '    dr(IDX_NAME) = cc.Name
                                '    dr(IDX_CONDITION) = ">="
                                '    dr(IDX_VALUE) = cc.MinValue.ToString("F2")
                                '    dt.Rows.Add(dr)
                                'End If
                                'If cc.MaxValue <> UNDEFINED_INTEGER_VALUE Then
                                '    dr = dt.NewRow()
                                '    dr(IDX_NAME) = cc.Name
                                '    dr(IDX_CONDITION) = "<="
                                '    dr(IDX_VALUE) = cc.MaxValue.ToString("F2")
                                '    dt.Rows.Add(dr)
                                'End If
                                Dim sValue As String = sNotConstrained
                                If String.IsNullOrEmpty(sNotConstrained) Then
                                    If cc.MinValue <> UNDEFINED_INTEGER_VALUE Then sValue = " >= " + cc.MinValue.ToString(sDecimals)
                                    If cc.MaxValue <> UNDEFINED_INTEGER_VALUE Then sValue += " <= " + cc.MaxValue.ToString(sDecimals)
                                End If
                                dr = dt.NewRow()
                                dr(IDX_NAME) = cc.Name
                                dr(IDX_CONDITION) = ""
                                dr(IDX_VALUE) = sValue
                                dt.Rows.Add(dr)
                            End If
                        End If
                    Next
                End If

                'dependencies
                If RA.Scenarios.ActiveScenario.Settings.Dependencies Then
                    dt = ds.Tables(IDX_DEPENDENCIES)
                    For Each dep As RADependency In RA.Scenarios.ActiveScenario.Dependencies.Dependencies
                        dr = dt.NewRow()
                        Dim alt0 As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(dep.FirstAlternativeID)
                        Dim alt1 As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(dep.SecondAlternativeID)
                        If alt0 IsNot Nothing AndAlso alt1 IsNot Nothing Then
                            Dim alt0name As String = REPORT_ALT_PREFIX + alt0.SortOrder.ToString
                            Dim alt1name As String = REPORT_ALT_PREFIX + alt1.SortOrder.ToString
                            dr(IDX_NAME) = CStr(IIf(dep.Value = RADependencyType.dtDependsOn, String.Format("{0} {1} {2}", alt0name, ResString("lblDependsOn"), alt1name), String.Format("{0} {1} {2} {3} {4}", alt0name, ResString("lblAnd"), alt1name, ResString("lblAre"), IIf(dep.Value = RADependencyType.dtMutuallyDependent, ResString("lblMutuallyDependent"), ResString("lblMutuallyExclusive")))))
                            dt.Rows.Add(dr)
                        End If
                    Next
                End If

                'groups
                If RA.Scenarios.ActiveScenario.Settings.Groups Then
                    dt = ds.Tables(IDX_GROUPS)
                    For Each kvp As KeyValuePair(Of String, RAGroup) In RA.Scenarios.ActiveScenario.Groups.Groups
                        Dim grp As RAGroup = kvp.Value
                        dr = dt.NewRow()
                        dr(IDX_NAME) = grp.Name
                        dr(IDX_CONDITION) = CStr(IIf(grp.Condition = RAGroupCondition.gcEqualsOne, ResString("lblRAGroupEq1"), IIf(grp.Condition = RAGroupCondition.gcGreaterOrEqualsOne, ResString("lblRAGroupMoreOrEq1"), ResString("lblRAGroupLessOrEq1")))) + ":"
                        dr(IDX_VALUE) = String.Join(", ", (From alt As KeyValuePair(Of String, RAAlternative) In grp.Alternatives Select REPORT_ALT_PREFIX + alt.Value.SortOrder.ToString).ToArray) 'alt.Value.Name
                        dt.Rows.Add(dr)
                    Next
                End If

                'funding pools
                If ReportMode = ReportModes.rmAllConstraints OrElse RA.Scenarios.ActiveScenario.Settings.FundingPools Then
                    dt = ds.Tables(IDX_FUND_POOLS)
                    For Each kvp As KeyValuePair(Of Integer, RAFundingPool) In RA.Scenarios.ActiveScenario.FundingPools.Pools
                        Dim fp As RAFundingPool = kvp.Value

                        Dim i As Integer = 0
                        While i < fp.Values.Count
                            Dim alt As String = fp.Values.Keys(i)
                            If RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(alt) Is Nothing Then
                                fp.Values.Remove(alt)
                            Else
                                i += 1
                            End If
                        End While

                        Dim sNotConstrained As String = CStr(IIf(fp.PoolLimit <= 0, "NOT CONSTRAINED", ""))
                        If ReportMode = ReportModes.rmAllConstraints OrElse sNotConstrained = "" Then
                            If ViewMode = ViewModes.vmDetails Then
                                If reportID = RAReportIDs.rpModelSpecification Then
                                    dr = dt.NewRow()
                                    dr(IDX_ID) = fp.ID.ToString
                                    dr(IDX_NAME) = fp.Name
                                    dr(IDX_CONDITION) = ""
                                    dr(IDX_VALUE) = String.Format("{0}{1}", String.Join(" + ", (From alt As KeyValuePair(Of String, Double) In fp.Values Select CostString(CDbl(alt.Value)) + " * """ + REPORT_ALT_PREFIX + RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(alt.Key).SortOrder.ToString + """").ToArray), CStr(IIf(sNotConstrained = "", " <= " + CostString(fp.PoolLimit), " - " + sNotConstrained))) ' D3199 + A1086
                                    dt.Rows.Add(dr)
                                Else
                                    dr = dt.NewRow()
                                    dr(IDX_ID) = fp.ID.ToString
                                    dr(IDX_NAME) = fp.Name
                                    dr(IDX_CONDITION) = ""
                                    dr(IDX_VALUE) = fp.PoolLimit
                                    dt.Rows.Add(dr)
                                End If
                            Else
                                Dim sValue As String = sNotConstrained
                                If String.IsNullOrEmpty(sNotConstrained) Then
                                    If fp.PoolLimit > 0 Then sValue = " <= " + CostString(fp.PoolLimit)
                                End If
                                dr = dt.NewRow()
                                dr(IDX_ID) = fp.ID.ToString
                                dr(IDX_NAME) = fp.Name
                                dr(IDX_CONDITION) = ""
                                dr(IDX_VALUE) = sValue
                                dt.Rows.Add(dr)
                            End If
                        End If
                    Next
                End If
        End Select

        For Each t As DataTable In ds.Tables
            ReportViewerMain.LocalReport.DataSources.Add(New ReportDataSource(t.TableName, t))
        Next

        'parameters        
        Dim tTitle As String = ""
        Select Case reportID
            Case RAReportIDs.rpModelSpecification
                tTitle = ResString("lblRAReportSpecifications")
            Case RAReportIDs.rpConstraintSummary
                tTitle = ResString("lblRAReportConstraints")
            Case RAReportIDs.rpRelevantConstraints
                tTitle = ResString("lblRAReportRelevancies")
            Case RAReportIDs.rpSolverReport
                tTitle = ResString("lblRASolverReport")
        End Select

        Dim paramReportTitle As ReportParameter = New ReportParameter("paramReportTitle", tTitle)
        Dim paramScenarioName As ReportParameter = New ReportParameter("paramActiveScenarioName", RA.Scenarios.ActiveScenario.Name)

        Dim params() As ReportParameter = New ReportParameter(1) {paramReportTitle, paramScenarioName}

        Select Case reportID
            Case RAReportIDs.rpModelSpecification
                With RA.Scenarios.ActiveScenario
                    ReDim Preserve params(3)
                    Dim paramBudget As ReportParameter = New ReportParameter("paramBudget", String.Format("<= {0}", CostString(.Budget)))   ' D3199
                    Dim paramBenefits As ReportParameter = New ReportParameter("paramBenefits", String.Join(" + ", (From alt As RAAlternative In .Alternatives Select CDbl(IIf(RA.Scenarios.ActiveScenario.Settings.Risks, alt.Benefit, alt.BenefitOriginal)).ToString(sDecimals) + " * " + REPORT_ALT_PREFIX + alt.SortOrder.ToString).ToArray))
                    params(2) = paramBudget
                    params(3) = paramBenefits
                End With
            Case RAReportIDs.rpSolverReport
                With RA.Scenarios.ActiveScenario
                    For Each alt In .Alternatives
                        sSolverReport = CStr(sSolverReport).Replace(alt.ID, GetShortAltName(alt))
                    Next
                    ReDim Preserve params(2)
                    If String.IsNullOrEmpty(CStr(sSolverReport)) Then sSolverReport = ResString("msgRiskResultsNoData")
                    Dim paramReport As ReportParameter = New ReportParameter("paramReport", CStr(sSolverReport))
                    params(2) = paramReport
                End With
        End Select

        ReportViewerMain.LocalReport.SetParameters(params)
    End Sub
    'A0909 ==

    Private Function getFundedList(alts As List(Of RAAlternative)) As List(Of Double)
        Dim retVal As List(Of Double) = New List(Of Double)
        For Each alt As RAAlternative In alts
            retVal.Add(alt.DisplayFunded) 'A0939
        Next
        Return retVal
    End Function

    Private Function Equal(alts0 As List(Of Double), alts1 As List(Of Double)) As Boolean
        For i As Integer = 0 To alts0.Count - 1
            If alts0(i) <> alts1(i) Then Return False
        Next
        Return True
    End Function

    Private Sub AddRelevancyData(sName As String, sValue As String, ByRef ds As DataSet, isRelevant As Boolean)
        Dim tTable As DataTable
        If isRelevant Then tTable = ds.Tables(IDX_RELEVANT) Else tTable = ds.Tables(IDX_IRRELEVANT)
        Dim dr As DataRow = tTable.NewRow()
        dr(IDX_NAME) = sName
        dr(IDX_VALUE) = sValue
        tTable.Rows.Add(dr)
    End Sub

    Private Function GetShortAltName(tAlt As RAAlternative) As String
        Return REPORT_ALT_PREFIX + tAlt.SortOrder.ToString 'tAlt.Name
    End Function

    Private Function GetDependencyName(tDep As RADependency, tTPDep As RADependency, isTP As Boolean) As String
        Dim retVal As String = ""
        Dim tAlt0 As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(tDep.FirstAlternativeID)
        Dim tAlt1 As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(tDep.SecondAlternativeID)
        If tAlt0 IsNot Nothing AndAlso tAlt1 IsNot Nothing Then
            Dim alt0name As String = GetShortAltName(tAlt0)
            Dim alt1name As String = GetShortAltName(tAlt1)
            Select Case tDep.Value
                Case RADependencyType.dtDependsOn
                    retVal = String.Format("{0} Depends on {1}", alt0name, alt1name)
                    If isTP AndAlso tTPDep Is Nothing Then retVal += " (Non-concurrent)"
                    If isTP AndAlso tTPDep IsNot Nothing Then 
                        Select Case tTPDep.Value 
                            Case RADependencyType.dtConcurrent 
                                retVal += " (Can be concurrent)"
                            Case RADependencyType.dtSuccessive 
                                retVal += " (Non-concurrent)"
                            Case RADependencyType.dtLag
                                Select Case tTPDep.LagCondition 
                                    Case LagCondition.lcEqual 
                                        retVal += String.Format(" ({0} starts {2} periods after {1})", alt0name, alt1name, tTPDep.Lag)
                                    Case LagCondition.lcGreaterOrEqual
                                        retVal += String.Format(" ({0} starts at least {2} periods after {1})", alt0name, alt1name, tTPDep.Lag) 
                                    Case LagCondition.lcLessOrEqual 
                                        retVal += String.Format(" ({0} starts at most {2} periods after {1})", alt0name, alt1name, tTPDep.Lag)
                                    Case LagCondition.lcRange 
                                        retVal += String.Format(" ({0} starts in the interval of [{2} : {3}] periods after {1})", alt0name, alt1name, tTPDep.Lag, IIf(tTPDep.LagUpperBound = UNDEFINED_INTEGER_VALUE, "undefined", tTPDep.LagUpperBound))
                                End Select                                
                        End Select
                    End If
                Case RADependencyType.dtMutuallyDependent
                    retVal = String.Format("{0} Mutually Dependent with {1}", alt0name, alt1name)
                Case RADependencyType.dtMutuallyExclusive
                    retVal = String.Format("{0} Mutually Exclusive with {1}", alt0name, alt1name)
                Case RADependencyType.dtConcurrent
                    retVal = String.Format("{0} and {1} are Concurrent", alt0name, alt1name)
                Case RADependencyType.dtSuccessive
                    retVal = String.Format("{0} and {1} are Successive", alt0name, alt1name)
                Case RADependencyType.dtLag
                    retVal = String.Format("{0} starts {2} periods before {1} starts", alt0name, alt1name, IIf(tDep.Lag <> Integer.MinValue, tDep.Lag, "undefined"))
            End Select
        End If
        Return retVal
    End Function

    Private Function GetGroupName(tGroup As RAGroup) As String
        Dim sCondition As String = ""
        Select Case tGroup.Condition
            Case RAGroupCondition.gcLessOrEqualsOne
                sCondition = "LE1"
            Case RAGroupCondition.gcEqualsOne
                sCondition = "EQ1"
            Case RAGroupCondition.gcGreaterOrEqualsOne
                sCondition = "GE1"
        End Select
        Return String.Format("""{0}"" ({1})", tGroup.Name, sCondition)
    End Function

    Private Sub RAReportPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        RA.Load()
        If Not isCallback AndAlso Not IsPostBack Then
            RA.Scenarios.CheckModel() 'A1324
        End If
    End Sub

    ' -D3398 : AD looks as working now
    'Protected Sub ReportViewerMain_Load(sender As Object, e As System.EventArgs) Handles ReportViewerMain.Load
    '    'Hide the export to "Word" from menu (because it produces a currupt document)
    '    Dim exportOption As String = "Word"
    '    Dim extension As RenderingExtension = ReportViewerMain.LocalReport.ListRenderingExtensions().ToList().Find(Function(x) x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase))
    '    If extension IsNot Nothing Then
    '        Dim fieldInfo As System.Reflection.FieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)
    '        fieldInfo.SetValue(extension, False)
    '    End If
    'End Sub

    'Private Sub btnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
    '    Try
    '        Dim rp As ReportPrintDocument = New ReportPrintDocument(ReportViewerMain.LocalReport)
    '        rp.Print()
    '    Catch ex As Exception
    '        Response.Write("<script>alert('No printers are installed.');</script>")
    '    End Try
    'End Sub

End Class