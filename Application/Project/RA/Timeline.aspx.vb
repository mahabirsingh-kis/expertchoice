Imports Canvas.RAGlobalSettings
Imports System.Globalization

Partial Class RATimelinePage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_RA_TIMEPERIODS_SETTINGS)
    End Sub

    Public ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property
    Dim AltColors As String() = {"#95c5f0", "#fa7000", "#9d27a8", "#e3e112", "#00687d", "#407000", "#f24961", "#663d2e", "#9600fa", _
                             "#ffbde6", "#00c49f", "#7280c4", "#009180", "#e33000", "#80bdff", "#a10040", "#0affe3", "#00523c", _
                             "#919100", "#5c00f7", "#a15f00", "#cce6ff", "#00465c", "#adff69", "#f24ba0", "#0dff87", "#ff8c47", _
                             "#349400", "#b3b3a1", "#a10067", "#ba544a", "#edc2d1", "#00e8c3", "#3f0073", "#5ec1f7", "#6e00b8", _
                             "#f5f5c4", "#e33000", "#52ba00", "#ff943b", "#0079db", "#f0e6c0", "#ffb517", "#cf0076", "#e8cfc9"}

    '{ id: 0, idx: 1, name: 'Project1', periodStart: 0, periodDuration: 1, periodMin: 0, periodMax: 1, color: '#95c5f0' }

    Public Function GetLink(PgID As Integer) As String
        If isSLTheme() Then
            Return String.Format("' onclick='navOpenPage({0}, true); return false;", PgID)
        Else
            Return PageURL(PgID)
        End If
    End Function

    'A1142 ===
    Public Const OPT_SCENARIO_LEN As Integer = 45
    Public Const OPT_DESCRIPTION_LEN As Integer = 200

    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tScen As Integer In RA.Scenarios.Scenarios.Keys
            sRes += String.Format("<option value='{0}'{2}>{1}</option>", tScen, SafeFormString(ShortString(RA.Scenarios.Scenarios(tScen).Name, OPT_SCENARIO_LEN, True) + CStr(IIf(String.IsNullOrEmpty(RA.Scenarios.Scenarios(tScen).Description), "", String.Format(" ({0})", ShortString(RA.Scenarios.Scenarios(tScen).Description, OPT_DESCRIPTION_LEN))))), IIf(tScen = RA.Scenarios.ActiveScenarioID, " selected", ""))
        Next
        Return String.Format("<select id='cbScenarios' style='width:130px; height:21px; margin-right:2px;' onchange='onSetScenario(this.value);'>{0}</select>", sRes)
    End Function
    'A1142 ==

    Public Function GetTimePeriodsCount() As Integer
        If RA.Scenarios.ActiveScenario IsNot Nothing Then
            Return RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count
        End If
        Return 0
    End Function

    Public Const SESS_RA_SHOWFUNDEDONLY As String = "RA_ShowFundedOnly"
    Private Const SESS_TP_SELECTED_RESOURCE As String = "TP_ResID_{0}"

    Public Function RA_ShowFundedOnly() As String
        If Session(SESS_RA_SHOWFUNDEDONLY) Is Nothing Then Return CBool(GetCookie(SESS_RA_SHOWFUNDEDONLY, False.ToString)).ToString().ToLower() Else Return CBool(Session(SESS_RA_SHOWFUNDEDONLY)).ToString().ToLower()
    End Function

    Public Function GetTimePeriodsType() As Integer
        Return RA.Scenarios.ActiveScenario.TimePeriods.PeriodsType
    End Function

    Public Function GetTimePeriodsStep() As Integer
        Return RA.Scenarios.ActiveScenario.TimePeriods.PeriodsStep
    End Function

    Public Function GetTimePeriodDiscount() As String
        Return JS_SafeNumber(RA.Scenarios.ActiveScenario.TimePeriods.DiscountFactor)
    End Function

    Public Function GetTimePeriodDiscountChecked() As String
        Return RA.Scenarios.ActiveScenario.TimePeriods.UseDiscountFactor.ToString().ToLower
    End Function

    Public Function GetTimePeriodNameFormat() As String
        Return RA.Scenarios.ActiveScenario.TimePeriods.NamePrefix
    End Function

    Public Function GetTimelineStartDate() As String
        Dim aDate As Date = RA.Scenarios.ActiveScenario.TimePeriods.TimelineStartDate.Date
        If aDate.Year < 1000 Then
            aDate = Now()
            RA.Scenarios.ActiveScenario.TimePeriods.TimelineStartDate = aDate
            App.ActiveProject.SaveRA("Edit timeperiods", , , "Set start date") ' D3791
        End If
        Dim aYear As Integer = aDate.Year
        Dim aMM As Integer = aDate.Month
        Dim aDD As Integer = aDate.Day
        Return String.Format("{0:D2}/{1:D2}/{2}", aMM, aDD, aYear)
    End Function

    Public Function GetTimeperiodsDistribMode() As Integer
        Return PM.Parameters.TimeperiodsDistributeMode
    End Function

    Public Function GetRAProjectResources(altID As String, startPeriodID As Integer, duration As Integer) As String
        Dim retVal As String = ""
        If RA.Scenarios.ActiveScenario IsNot Nothing AndAlso RA.Scenarios.ActiveScenario.TimePeriods.EnabledResources IsNot Nothing Then
            Dim PeriodsCount = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count
            Dim alt = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(altID)
            For Each res In RA.Scenarios.ActiveScenario.TimePeriods.EnabledResources
                Dim totalCost As Double = alt.Cost
                If res.ConstraintID >= 0 Then
                    totalCost = RA.Scenarios.ActiveScenario.Constraints.GetConstraintValue(res.ConstraintID, altID)
                End If

                Dim valuesString = ""
                For i = startPeriodID To startPeriodID + duration - 1
                    Dim resVal As Double = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetResourceValue(i, altID, res.ID)
                    valuesString += CStr(IIf(valuesString <> "", ",", "")) + JS_SafeNumber(resVal)
                Next
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{idx: {0}, id:'{1}', name:'{2}', values: [{3}], totalValue: {4}}}", res.ConstraintID, JS_SafeString(res.ID.ToString), JS_SafeString(res.Name), valuesString, JS_SafeNumber(totalCost))  ' D3918
            Next
        End If
        Return retVal
    End Function

    Public Function GetRAViewMode() As String
        Select Case CurrentPageID
            'Case _PGID_RA_PORTFOLIO_RESOURCES
            '    Return "portfolio"
            Case _PGID_RA_PERIOD_RESULTS
                Return "results"
                'Case _PGID_RA_PROJECT_RESOURCES
                '    Return "resources"
                'Case _PGID_RA_PROJECTS_TIMELINE
            Case Else
                Return "edit"
        End Select

    End Function

    Public Function GetRAPortfolioResources() As String
        Dim retVal As String = ""
        If RA.Scenarios.ActiveScenario IsNot Nothing AndAlso RA.Scenarios.ActiveScenario.TimePeriods.EnabledResources IsNot Nothing Then
            Dim PeriodsCount = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count
            For Each res In RA.Scenarios.ActiveScenario.TimePeriods.EnabledResources
                For Each tp As Canvas.RATimePeriod In RA.Scenarios.ActiveScenario.TimePeriods.Periods
                    Dim resMinVal As Double = tp.GetResourceMinValue(res.ID)
                    Dim resMaxVal As Double = tp.GetResourceMaxValue(res.ID)
                    retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{resID:'{0}', periodID:{1}, minVal:{2}, maxVal:{3}}}", JS_SafeString(res.ID.ToString), RA.Scenarios.ActiveScenario.TimePeriods.Periods.IndexOf(tp), JS_SafeNumber(resMinVal), JS_SafeNumber(resMaxVal))  ' D3918
                Next
            Next
        End If
        retVal = String.Format("[{0}]", retVal)
        Return retVal
    End Function

    Public Function GetSolverState() As String
        Dim retVal = ""
        If RA.Scenarios.ActiveScenario IsNot Nothing Then
            Select Case RA.Solver.SolverState
                Case raSolverState.raSolved
                    retVal = "solved"
                Case raSolverState.raInfeasible
                    retVal = "infeasible"
            End Select
        End If
        Return retVal
    End Function


    Public Function GetRAProjects() As String
        Dim retVal As String = ""
        If RA.Scenarios.ActiveScenario IsNot Nothing Then
            ' D3982 ===
            RA.Scenarios.SyncLinkedConstraintsToResources() ' D4913
            RA.Scenarios.ActiveScenario.TimePeriods.LinkResourcesFromConstraints()
            If RA.Scenarios.ActiveScenario.TimePeriods.AllocateResourceValues() Then
                App.ActiveProject.SaveRA("Edit timeperiods", , , "Auto distribute resource value by periods")
            End If
            Dim tAlts As List(Of RAAlternative) = RA.Scenarios.ActiveScenario.Alternatives  ' D3763
            tAlts.Sort(New RAAlternatives_Comparer(CType(Math.Abs(CInt(RA.Scenarios.GlobalSettings.SortBy)), raColumnID), CInt(RA.Scenarios.GlobalSettings.SortBy) < 0, RA))
            For Each alt In tAlts
                Dim isFunded As Boolean = RA.Scenarios.ActiveScenario.Settings.TimePeriods AndAlso RA.Solver.SolverState = raSolverState.raSolved AndAlso alt.DisplayFunded > 0.0
                Dim AltTPData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                Dim AltResDataString = GetRAProjectResources(JS_SafeString(alt.ID), AltTPData.StartPeriod, AltTPData.Duration)
                Dim tGUID As New Guid(alt.ID)
                Dim tRealAlt As clsNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tGUID)
                Dim fHasInfodoc As Boolean = tRealAlt IsNot Nothing AndAlso tRealAlt.InfoDoc <> ""
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{id:'{0}',idx:{1},name:'{2}',periodStart:{3},periodDuration:{4},periodMin:{5},periodMax:{6},color:'{7}',isFunded:{8},resources:[{9}],mustPeriod:{10},hasMust:{11},hasinfodoc:{12}}}", JS_SafeString(alt.ID), alt.SortOrder, JS_SafeString(alt.Name), AltTPData.StartPeriod, AltTPData.Duration, AltTPData.MinPeriod, AltTPData.MaxPeriod, AltColors((alt.SortOrder - 1) Mod 45), Bool2JS(isFunded), AltResDataString, AltTPData.MustPeriod, Bool2JS(AltTPData.HasMust), Bool2JS(fHasInfodoc))
                ' D3982 ==
            Next
        End If
        retVal = String.Format("[{0}]", retVal)
        Return retVal
    End Function

    Public Function GetResourcesList() As String
        Dim retVal As String = ""
        If RA.Scenarios.ActiveScenario IsNot Nothing AndAlso RA.Scenarios.ActiveScenario.TimePeriods.EnabledResources IsNot Nothing Then
            RA.Scenarios.ActiveScenario.TimePeriods.LinkResourcesFromConstraints()
            For Each res In RA.Scenarios.ActiveScenario.TimePeriods.EnabledResources
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{id:'{0}', name:'{1}'}}", JS_SafeString(res.ID.ToString), JS_SafeString(res.Name))    ' D3918
            Next
        End If
        Return "[" + retVal + "]"
    End Function

    Public Function GetPeriodResults() As String
        Dim sPeriodResults As String = ""
        If RA.Scenarios.ActiveScenario IsNot Nothing AndAlso CurrentPageID = _PGID_RA_PERIOD_RESULTS Then
            If RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900
            Dim ATPResults = RA.Scenarios.ActiveScenario.TimePeriods.TimePeriodResults
            For Each item In ATPResults
                sPeriodResults += CStr(IIf(sPeriodResults <> "", ",", "")) + String.Format("{{projID:'{0}', start:{1}}}", JS_SafeString(item.Key), item.Value)
            Next
        End If
        sPeriodResults = String.Format("[{0}]", sPeriodResults)
        Return sPeriodResults
    End Function

    ' D3902 ===
    Public Function GetMessage() As String
        Dim sMessage As String = ""
        'If RA.Scenarios.ActiveScenario.Alternatives.Count > 0 Then sMessage = SolverStateHTML(RA.Solver)
        If Not RA.Scenarios.ActiveScenario.Settings.TimePeriods Then
            sMessage += String.Format("<div style='padding:4px 0px 10px 0px; text-align:center;'><nobr><span class='top_warning_nofloat' style='font-size:10pt; color:#993333; padding:4px 6px;'><!--img src='{2}attention.png' width=14 height=14 border=0/-->&nbsp;{0}</span></nobr> <input type='button' id='btnEnableTP' value='{1}' onclick='enableTP(); return false;' class='button button_small' style='width:15em; padding:2px; margin-top:6px; color:#333366;'/></div>", ResString("msgRATPIgnored"), SafeFormString(ResString("btnRAEnableTP")), ImagePath)
        End If
        Return sMessage
    End Function
    ' D3902 ==

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower   'Anti-XSS
        Dim sComment As String = "" ' D3791
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction))

        Select Case sAction

            Case "add_period"
                'Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower 'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    RA.Scenarios.ActiveScenario.TimePeriods.AddPeriod()
                    sComment = "Add period" ' D3791
                End If
                tResult = "'ok'"

            Case "remove_period"
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim periodsCount As Integer = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count
                    Dim lastPeriodID As Integer = RA.Scenarios.ActiveScenario.TimePeriods.Periods(periodsCount - 1).ID
                    RA.Scenarios.ActiveScenario.TimePeriods.DeletePeriod(lastPeriodID)
                    periodsCount -= 1
                    For Each proj As RAAlternative In RA.Scenarios.ActiveScenario.Alternatives
                        Dim RAData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        If (RAData.MaxPeriod + RAData.Duration) > periodsCount Then
                            If RAData.MaxPeriod > RAData.MinPeriod Then
                                RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(proj.ID, RAData.MaxPeriod - 1)
                            Else
                                If RAData.MinPeriod > 0 Then
                                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMinPeriod(proj.ID, RAData.MinPeriod - 1)
                                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(proj.ID, RAData.MaxPeriod - 1)
                                Else
                                    If RAData.Duration > 1 Then
                                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetDuration(proj.ID, RAData.Duration - 1)
                                    End If
                                End If
                            End If
                        End If
                    Next
                    sComment = "Delete period"  ' D3791
                End If
                tResult = "'ok'"

            Case "max_inc"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AltTPData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID)
                    If AltTPData IsNot Nothing Then
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(sProjectID, AltTPData.MaxPeriod + 1)
                        sComment = "Increase max period"  ' D3791
                    End If
                End If
                tResult = "'ok-syncprojs'"

            Case "max_dec"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AltTPData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID)
                    If AltTPData IsNot Nothing Then
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(sProjectID, AltTPData.MaxPeriod - 1)
                        sComment = "Decrease max period"  ' D3791
                    End If
                End If
                tResult = "'ok-syncprojs'"

            Case "set_max"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                Dim sMaxVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "maxval")).ToLower   'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AVal As Integer = CInt(sMaxVal)
                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(sProjectID, AVal)
                    sComment = "Set max period"  ' D3791
                End If
                tResult = "'ok-syncprojs'"

            Case "minmax_inc"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AltTPData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID)
                    If AltTPData IsNot Nothing Then
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(sProjectID, AltTPData.MaxPeriod + 1)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMinPeriod(sProjectID, AltTPData.MinPeriod + 1)
                        sComment = "Increase periods"  ' D3791
                    End If
                End If
                tResult = "'ok-syncprojs'"

            Case "minmax_dec"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AltTPData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID)
                    If AltTPData IsNot Nothing Then
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(sProjectID, AltTPData.MaxPeriod - 1)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMinPeriod(sProjectID, AltTPData.MinPeriod - 1)
                        sComment = "Decrease periods"  ' D3791
                    End If
                End If
                tResult = "'ok-syncprojs'"

            Case "dur_inc"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AltTPData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID)
                    If AltTPData IsNot Nothing Then
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(sProjectID, AltTPData.MaxPeriod - 1)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetDuration(sProjectID, AltTPData.Duration + 1)
                        sComment = "Increase duration"  ' D3791
                    End If
                End If
                tResult = "'ok-syncprojs'"

            Case "dur_dec"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AltTPData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID)
                    If AltTPData IsNot Nothing Then
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(sProjectID, AltTPData.MaxPeriod + 1)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetDuration(sProjectID, AltTPData.Duration - 1)
                        sComment = "Decrease duration"  ' D3791
                    End If
                End If
                tResult = "'ok-syncprojs'"

            Case "tpsetminmaxdur"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim min As Integer = CInt(GetParam(args, "min").ToLower)
                    Dim max As Integer = CInt(GetParam(args, "max").ToLower)
                    Dim dur As Integer = CInt(GetParam(args, "dur").ToLower)
                    Dim pcount As Integer = CInt(GetParam(args, "pcount").ToLower)
                    Dim AltTPData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID)
                    If AltTPData IsNot Nothing Then
                        While RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count < pcount
                            RA.Scenarios.ActiveScenario.TimePeriods.AddPeriod()
                        End While
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetDuration(sProjectID, dur)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(sProjectID, max)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMinPeriod(sProjectID, min)
                        sComment = "Adjust project periods data and periods count"  ' D3791
                    End If
                End If
                tResult = "'ok-syncprojs'"

            Case "setresource"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                'Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower 'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "values")).ToLower  'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AVal As Double = 0
                    sResID = sResID.Replace("{", "")
                    sResID = sResID.Replace("}", "")
                    Dim values() As String = sVal.Split("x"c)
                    For i As Integer = 0 To values.Length - 1
                        AVal = 0
                        String2Double(values(i), AVal)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetResourceValue(i, sProjectID, New Guid(sResID), AVal)    ' D3918
                    Next
                    sComment = "Set resource"  ' D3791
                End If
                tResult = "'ok'"

            Case "settotalresource"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value")).ToLower   'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AVal As Double = 0
                    sResID = sResID.Replace("{", "")
                    sResID = sResID.Replace("}", "")
                    If String2Double(sVal, AVal) Then
                        Dim res As RAResource = RA.Scenarios.ActiveScenario.TimePeriods.Resources(New Guid(sResID))
                        If res.ConstraintID < 0 Then
                            Dim alt As RAAlternative = RA.GetAlternativeById(RA.Scenarios.ActiveScenario, sProjectID)
                            If alt IsNot Nothing Then
                                RA.SetAlternativeCost(sProjectID, AVal)
                                'alt.Cost = AVal
                            End If
                        Else
                            Dim CC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.Constraints(res.ConstraintID)
                            If CC IsNot Nothing Then RA.Scenarios.ActiveScenario.Constraints.SetConstraintValue(res.ConstraintID, sProjectID, AVal)
                        End If
                    End If
                End If
                tResult = "'ok'"
            Case "changeresourceid"
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                TP_RES_ID = sResID
                tResult = "'ok'"

            Case "tpdistribmode"
                Dim sNamingMode As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mode")).ToLower 'Anti-XSS
                Dim tpMode As Integer = CInt(sNamingMode)
                PM.Parameters.TimeperiodsDistributeMode = tpMode
                PM.Parameters.Save()
                tResult = "'ok'"

            Case "tpnamingmode"
                Dim sNamingMode As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mode")).ToLower 'Anti-XSS
                Dim tpMode As Integer = CInt(sNamingMode)
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsType = CType(tpMode, TimePeriodsType)
                    sComment = "Change periods type"  ' D3791
                End If
                tResult = "'ok'"

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
                        tResult = String.Format("['{0}','{1}']", "save_altname", sName)
                        App.ActiveProject.SaveStructure("Edit alternative name", True, True, String.Format("'{0}' -> '{1}'", ShortString(sOld, 60), ShortString(sName, 60)))
                    End If
                End If

            Case "tpstartdate"
                Dim sStartDate As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "startdate")).ToLower 'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim aDate As DateTime = Now()
                    If DateTime.TryParseExact(sStartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, aDate) Then
                        RA.Scenarios.ActiveScenario.TimePeriods.TimelineStartDate = aDate
                        sComment = "Set start date"  ' D3791
                    End If
                End If
                tResult = "'ok'"

            Case "tpdiscount"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "discount")).ToLower    'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AVal As Double = 0
                    String2Double(sVal, AVal)
                    RA.Scenarios.ActiveScenario.TimePeriods.DiscountFactor = AVal
                    sComment = "Set discount factor"  ' D3791
                End If
                tResult = "'ok'"

            Case "tpusediscount"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "usediscount")).ToLower 'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AVal As Boolean = CBool(sVal)
                    RA.Scenarios.ActiveScenario.TimePeriods.UseDiscountFactor = AVal
                    sComment = "Use discount factor: " + Bool2YesNo(AVal)  ' D3791
                End If
                tResult = "'ok'"

            Case "tpstep"
                Dim sStep As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "step")).ToLower   'Anti-XSS
                Dim tpStep As Integer = CInt(sStep)
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsStep = tpStep
                    sComment = "Set periods step"  ' D3791
                End If
                tResult = "'ok'"

            Case "solve"
                Dim sPeriodResults As String = ""
                Dim resString As String = "solved"
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    RA.Solve()
                    Dim ATPResults = RA.Scenarios.ActiveScenario.TimePeriods.TimePeriodResults
                    For Each item In ATPResults
                        sPeriodResults += CStr(IIf(sPeriodResults <> "", ",", "")) + String.Format("{{projID:'{0}', start:{1}}}", JS_SafeString(item.Key), item.Value)
                    Next
                    If RA.Solver.SolverState = raSolverState.raInfeasible Then
                        resString = "infeasible"
                    End If
                End If
                sPeriodResults = String.Format("[{0}]", sPeriodResults)
                tResult = String.Format("['{0}',{1}]", resString, sPeriodResults)

            Case "expandprojects"
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim PeriodsCount = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count
                    For Each proj In RA.Scenarios.ActiveScenario.Alternatives
                        Dim projData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(proj.ID, PeriodsCount - projData.Duration)
                    Next
                    sComment = "Expand All Projects Maxs"  ' D3791
                End If
                tResult = "'ok'"

            Case "contractprojects"
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    For Each proj In RA.Scenarios.ActiveScenario.Alternatives
                        Dim projData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(proj.ID, projData.MinPeriod)
                    Next
                    sComment = "Contract All Projects Maxs"
                End If
                tResult = "'ok'"

            Case "trimtimeline"
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim maxPeriod As Integer = 1
                    For Each proj In RA.Scenarios.ActiveScenario.Alternatives
                        Dim projData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        If maxPeriod < (projData.MaxPeriod + projData.Duration) Then
                            maxPeriod = projData.MaxPeriod + projData.Duration
                        End If
                    Next
                    Dim pcount As Integer = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count
                    For i As Integer = 1 To (pcount - maxPeriod)
                        Dim periodsCount As Integer = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count
                        Dim lastPeriodID As Integer = RA.Scenarios.ActiveScenario.TimePeriods.Periods(periodsCount - 1).ID
                        RA.Scenarios.ActiveScenario.TimePeriods.DeletePeriod(lastPeriodID)
                    Next
                    sComment = "Trim Timeline"
                End If
                tResult = "'ok'"

            Case "tpsetminval"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower  'Anti-XSS
                Dim AVal As Double = 0
                String2Double(sVal, AVal)
                RA.Scenarios.ActiveScenario.TimePeriods.Periods(CInt(sPeriodID)).SetResourceMinValue(New Guid(sResID), AVal)    ' D3918
                sComment = "Set resource minimum"  ' D3791
                tResult = "'ok'"
                'RA.Solve()
                'Dim solvedResults = GetPeriodResults()
                'Dim projects = GetRAProjects()
                'Dim pres = GetRAPortfolioResources()
                'tResult = String.Format("['resolved',{0},{1},{2}]", projects, pres, solvedResults)

            Case "tpsetmaxval"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower  'Anti-XSS
                Dim AVal As Double = 0
                String2Double(sVal, AVal)
                RA.Scenarios.ActiveScenario.TimePeriods.Periods(CInt(sPeriodID)).SetResourceMaxValue(New Guid(sResID), AVal)    ' D3918
                sComment = "Set resource maximum"  ' D3791
                tResult = "'ok'"
                'RA.Solve()
                'Dim solvedResults = GetPeriodResults()
                'Dim projects = GetRAProjects()
                'Dim pres = GetRAPortfolioResources()
                'tResult = String.Format("['resolved',{0},{1},{2}]", projects, pres, solvedResults)
            Case "tpsetmustperiod"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower    'Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower 'Anti-XSS
                Dim sHasMust As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "hasmust")).ToLower 'Anti-XSS
                If RA.Scenarios.ActiveScenario IsNot Nothing Then
                    Dim AVal As Integer = CInt(sVal)
                    Dim AHasMust As Integer = CInt(sHasMust)
                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID).MustPeriod = AVal
                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(sProjectID).HasMust = (AHasMust = 1)
                End If
                tResult = "'ok'"
            Case "paste_column"
                Dim periodsCount As Integer = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count
                Dim colID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "column")).ToLower)
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "data")).ToLower
                Dim cells As String() = sVal.Split(Chr(10))
                Dim cells_count As Integer = cells.Count
                Dim alts_count As Integer = RA.Scenarios.ActiveScenario.Alternatives.Count
                If alts_count < cells.Count Then cells_count = alts_count
                For i As Integer = 0 To cells_count - 1
                    Dim value As String = cells(i).Trim
                    If String.IsNullOrEmpty(value) Then value = Nothing
                    Dim alt As RAAlternative = RA.Scenarios.ActiveScenario.Alternatives(i)
                    Dim intValue As Integer

                    If value IsNot Nothing AndAlso Integer.TryParse(value, intValue) Then
                        Dim pData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Select Case colID
                            Case 0
                                If pData.MaxPeriod >= intValue - 1 And intValue > 0 And (intValue - 1 + pData.Duration) <= periodsCount Then
                                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMinPeriod(alt.ID, intValue - 1)
                                End If
                            Case 1
                                If pData.MinPeriod <= intValue - 1 And intValue > 0 And (intValue - 1 + pData.Duration) <= periodsCount Then
                                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(alt.ID, intValue - 1)
                                End If
                            Case 2
                                If intValue > 0 And (intValue + pData.MaxPeriod) <= periodsCount Then
                                    RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetDuration(alt.ID, intValue)
                                End If
                        End Select
                    End If

                Next
                tResult = String.Format("['paste',{0}]", GetRAProjects())
            Case "tpsetallminmaxval"
                Dim Scenario = RA.Scenarios.ActiveScenario
                Dim sMinVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "minval")).ToLower 'Anti-XSS
                Dim sMaxVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "maxval")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim AMinVal As Double = 0
                Dim AMaxVal As Double = 0
                If args.AllKeys.Contains("minval") Then String2Double(sMinVal, AMinVal)
                If args.AllKeys.Contains("maxval") Then String2Double(sMaxVal, AMaxVal)
                For Each period In Scenario.TimePeriods.Periods
                    If args.AllKeys.Contains("minval") Then Scenario.TimePeriods.Periods(period.ID).SetResourceMinValue(New Guid(sResID), AMinVal)
                    If args.AllKeys.Contains("maxval") Then Scenario.TimePeriods.Periods(period.ID).SetResourceMaxValue(New Guid(sResID), AMaxVal)
                Next
                sComment = "Set resource minimum and maximum for all periods"  ' D3791
                tResult = "'ok'"
        End Select

        If tResult <> "" Then
            App.ActiveProject.SaveRA("Edit timeperiods", , , sComment)   ' D3971

            ' -D3943
            'If sAction <> "solve" Then
            '    PM.Parameters.TimeperiodsHasData(RA.Scenarios.ActiveScenarioID) = True  ' D3841
            '    PM.Parameters.Save()
            'End If

            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Public Property TP_RES_ID As String
        Get
            If Session(String.Format(SESS_TP_SELECTED_RESOURCE, App.ProjectID)) Is Nothing Then
                If RA.Scenarios.ActiveScenario IsNot Nothing AndAlso RA.Scenarios.ActiveScenario.TimePeriods IsNot Nothing AndAlso RA.Scenarios.ActiveScenario.TimePeriods.EnabledResources(0) IsNot Nothing Then
                    Return JS_SafeString(RA.Scenarios.ActiveScenario.TimePeriods.EnabledResources(0).ID.ToString)
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
    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        'A1142 ===
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower 'Anti-XSS
        Select Case sAction
            Case "scenario"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    RA.Scenarios.ActiveScenarioID = ID
                    RA.Solver.ResetSolver()
                End If
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)

                ' D3902 ===
            Case "enable_tp"
                RA.Scenarios.ActiveScenario.Settings.TimePeriods = True
                App.ActiveProject.SaveRA("Change ignore option", , , "TimePeriods: No")
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
                ' D3902 ==
        End Select
        'A1142 ==

        Ajax_Callback(Request.Form.ToString)
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim pgid As Integer = CheckVar("pgid", CurrentPageID)
        If App.isRiskEnabled AndAlso pgid = _PGID_RA_TIMEPERIODS_SETTINGS Then pgid = _PGID_RISK_OPTIMIZER_TIME_PERIODS
        Select Case pgid
            'Case _PGID_RA_PORTFOLIO_RESOURCES, _PGID_RA_TIMEPERIODS_SETTINGS, _PGID_RA_PROJECT_RESOURCES, _PGID_RA_PERIOD_RESULTS, _PGID_RISK_OPTIMIZER_TIME_PERIODS
            Case _PGID_RA_TIMEPERIODS_SETTINGS, _PGID_RA_PERIOD_RESULTS, _PGID_RISK_OPTIMIZER_TIME_PERIODS
                CurrentPageID = pgid
        End Select
        ' D4929 ===
        If Not isCallback AndAlso Not IsPostBack Then
            RA.Load()
            RA.Scenarios.CheckModel(, False)    ' D7096 + D7102
        End If
        ' D4929 ==
    End Sub

End Class