Partial Class RADependenciesPage
    Inherits clsComparionCorePage

    Public Const OPT_SCENARIO_LEN As Integer = 45       'A0965
    Public Const OPT_DESCRIPTION_LEN As Integer = 200   'A0965
    Public Const OPT_ALT_NAME_LEN As Integer = 40       'A1411
    Public Const OPT_MAX_ALTS_GRID As Integer = 100     'A1438 '25 ' D3424 // grid around 2500 cells;

    Dim DepTypeNames() As String = {"lblDependsOn", "lblMutuallyDependent", "lblMutuallyExclusive", "lblRADependencyConcurrent", "lblRADependencySuccessive", "lblRADependencyLag"} ' D3790 + A1161 + A1179

    Public Sub New()
        MyBase.New(_PGID_RA_DEPENDENCIES)
    End Sub

    ReadOnly Property RA As ResourceAligner
        Get
            Return If(App.ActiveProject.ProjectManager.IsRiskProject, App.ActiveProject.ProjectManager.ResourceAlignerRisk, App.ActiveProject.ProjectManager.ResourceAligner)
        End Get
    End Property

    ReadOnly Property Optimizer As RASolver
        Get
            Return RA.Solver
        End Get
    End Property    

    Public ReadOnly Property SESSION_DROPDOWN_SELECTED_DEP_TYPE As String
        Get
            Return String.Format("RA_Dependencies_DropdownSelected_{0}", App.ProjectID)
        End Get
    End Property

    Public Property DropdownSelectedDependency As RADependencyType
        Get
            Dim retVal As RADependencyType = RADependencyType.dtDependsOn
            Dim tSess As String = SessVar(SESSION_DROPDOWN_SELECTED_DEP_TYPE)
            If Not String.IsNullOrEmpty(tSess) AndAlso [Enum].IsDefined(GetType(RADependencyType), CInt(tSess)) Then
                retVal = Ctype(CInt(tSess), RADependencyType)
            End If
            If retVal = RADependencyType.dtDependsOn Then 
                retVal = RADependencyType.dtConcurrent
            End If
            Return retVal
        End Get
        Set(value As RADependencyType)
            SessVar(SESSION_DROPDOWN_SELECTED_DEP_TYPE) = CStr(CInt(value))
        End Set
    End Property

    Private Sub RADependenciesPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        If App.isRiskEnabled Then CurrentPageID = _PGID_RISK_OPTIMIZER_DEPENDENCIES
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If Not IsPostBack AndAlso Not IsCallback Then
            RA.Scenarios.CheckModel(False) 'A1324
            '-A1324 RA.Scenarios.UpdateSortOrder()  ' D3181
        End If
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower 'Anti-XSS
        If Not isGridAllowed() Then RA.Scenarios.GlobalSettings.DependenciesView = True ' D3424 + D3781
        Select Case sAction
            Case "scenario"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    RA.Scenarios.ActiveScenarioID = ID
                    Optimizer.ResetSolver()
                End If
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
        End Select
        Ajax_Callback(Request.Form.ToString)
    End Sub

    Protected Sub Page_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        If Not IsPostBack AndAlso Not IsCallback Then
            'If RA.Scenarios.Scenarios.Count = 0 Then RA.Load() ' -D4857
            'ClientScript.RegisterStartupScript(GetType(String), "Init", "onPageResize(); setTimeout('updateToolbarDropDown(); InitPage();', 350);", True)    ' D2883
        End If
    End Sub

    Public Function isTimePeriodsVisible() As Boolean
        Return RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 1 ' D3943 + A1208
    End Function

    Public ReadOnly Property isTimePeriodsLagsVisible As Boolean
        Get
            Return ShowDraftPages() AndAlso isTimePeriodsVisible
        End Get
    End Property

    ' D3424 ===
    Public Function isGridAllowed() As Boolean
        Return Optimizer.Alternatives.Count <= OPT_MAX_ALTS_GRID
    End Function
    ' D3424 ==

    Public Function LoadAlternativesData() As String
        Dim sAllAlts As String = ""
        Dim tIndexVisible As Boolean = RA.Scenarios.GlobalSettings.IsIndexColumnVisible 'A1143
        Dim tMaxIndex As Integer = 0

        For Each tAlt As RAAlternative In Optimizer.Alternatives
            sAllAlts += If(sAllAlts = "", "", ",") + String.Format("['{0}','{1}',false]", tAlt.ID, JS_SafeString(If(tIndexVisible, tAlt.SortOrder.ToString + ". ", "") + tAlt.Name)) 'A1143
            tMaxIndex = Math.Max(tMaxIndex, tAlt.SortOrder)
        Next

        If App.ActiveProject.IsRisk Then 
            For Each tGroup As KeyValuePair(Of String, RAGroup) In Optimizer.Groups.Groups
                sAllAlts += If(sAllAlts = "", "", ",") + String.Format("['{0}','{1}',true]", tGroup.Value.ID, JS_SafeString(If(tIndexVisible, (tMaxIndex + 1).ToString + ". ", "") + tGroup.Value.Name))
                tMaxIndex += 1
            Next
        End If

        Return String.Format("[{0}]", sAllAlts)
    End Function

    Private Function GetDependencyData(tDep As RADependency) As String
        Dim sExtra As String = ""
        If tDep.Value = RADependencyType.dtLag Then 
            sExtra = String.Format("{0},{1},{2}", tDep.Lag, CInt(tDep.LagCondition), tDep.LagUpperBound)
        End If
        Dim retVal As String = String.Format("['{0}','{1}',{2},[{3}]]", tDep.FirstAlternativeID, tDep.SecondAlternativeID, CInt(tDep.Value), sExtra)
        Return retVal
    End Function

    Public Function LoadDependenciesData() As String
        Dim sDeps As String = ""

        For Each tDep As RADependency In Optimizer.Dependencies.Dependencies
            sDeps += If(sDeps = "", "", ",") + GetDependencyData(tDep)
        Next

        Return String.Format("[{0}]", sDeps)
    End Function

    Public Function LoadTPDependenciesData() As String
        Dim sDeps As String = ""

        For Each tDep As RADependency In Optimizer.TimePeriodsDependencies.Dependencies
            sDeps += If(sDeps = "", "", ",") + GetDependencyData(tDep)
        Next

        Return String.Format("[{0}]", sDeps)
    End Function

    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tScen As String In RA.Scenarios.Scenarios.Keys
            Dim ID As Integer = CInt(tScen)
            sRes += String.Format("<option value='{0}'{2}>{1}</option>", tScen, ShortString(SafeFormString(RA.Scenarios.Scenarios(ID).Name), OPT_SCENARIO_LEN, True) + If(String.IsNullOrEmpty(RA.Scenarios.Scenarios(ID).Description), "", String.Format(" ({0})", ShortString(SafeFormString(RA.Scenarios.Scenarios(ID).Description), OPT_DESCRIPTION_LEN))), If(ID = RA.Scenarios.ActiveScenarioID, " selected", ""))
        Next
        Return String.Format("<select id='cbScenarios' style='width:130px; margin-top:3px' onchange='onSetScenario(this.value);'>{0}</select>", sRes)
    End Function

    ' D3790 ===
    Private Function GetAltName(sID As String) As String
        For Each tAlt As RAAlternative In Optimizer.Alternatives
            If tAlt.ID = sID Then Return ShortString(SafeFormString(tAlt.Name), OPT_ALT_NAME_LEN, True) 'A1411
        Next
        For Each tGroup As KeyValuePair(Of String, RAGroup) In Optimizer.Groups.Groups
            If tGroup.Value.ID = sID Then Return ShortString(SafeFormString(tGroup.Value.Name), OPT_ALT_NAME_LEN, True)
        Next
        Return ""
    End Function
    ' D3790 ==

    Private Function RemoveOldDependency(Deps As RADependencies, exAlt1ID As String, exAlt2ID As String) As Integer
        Dim index As Integer = -1
        If exAlt1ID <> "" AndAlso exAlt2ID <> "" Then
            index = Deps.Dependencies.IndexOf(Deps.GetDependency(exAlt1ID, exAlt2ID))
            Deps.DeleteDependency(exAlt1ID, exAlt2ID)
        End If
        Return index
    End Function

    Private Sub MoveNewDependency(Deps As RADependencies, index As Integer)
        If index >= 0 Then
            Dim item As RADependency = Deps.Dependencies.Last()
            Deps.Dependencies.Remove(item)
            Deps.Dependencies.Insert(index, item)
        End If
    End Sub

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim tResult As String = ""
        Dim tSuccess As Boolean = False
        Dim tAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower   'Anti-XSS
        Dim sComment As String = "" ' D3790

        If Not String.IsNullOrEmpty(tAction) Then

            Select Case tAction
                Case "dd"
                    DropdownSelectedDependency = Ctype(CInt(GetParam(args, "val")), RADependencyType)
                Case "clear"
                    Optimizer.Dependencies.Dependencies.Clear()
                    Optimizer.TimePeriodsDependencies.Dependencies.Clear()
                    tSuccess = True
                    sComment = "Clear all dependencies" ' D3790
                Case "tableview", "listview"
                    RA.Scenarios.GlobalSettings.DependenciesView = Not (tAction = "tableview" AndAlso isGridAllowed())   ' D3185 + D3424 + D3781
                Case "del"
                    ' delete dependency from the List View
                    Dim Alt1ID As String = GetParam(args, "i") 'Alt1ID
                    Dim Alt2ID As String = GetParam(args, "j") 'Alt2ID
                    Optimizer.Dependencies.DeleteDependency(Alt1ID, Alt2ID)
                    Optimizer.TimePeriodsDependencies.DeleteDependency(Alt1ID, Alt2ID)
                    tSuccess = True
                    sComment = String.Format("Delete dependency for '{0}'/'{1}'", GetAltName(Alt1ID), GetAltName(Alt2ID))
                Case "convert_all"
                    Dim tFrom As Integer = CInt(GetParam(args, "from"))
                    Dim tTo As RADependencyType = Ctype(CInt(GetParam(args, "to")), RADependencyType)

                    ' convert timeperiods dependencies
                    For Each dp As RADependency In Optimizer.Dependencies.Dependencies
                        If dp.Value = RADependencyType.dtDependsOn Then
                            Optimizer.TimePeriodsDependencies.SetDependency(dp.FirstAlternativeID, dp.SecondAlternativeID, tTo)                            
                        End If                        
                    Next

                    RA.Scenarios.CheckModel(False)

                    sComment = String.Format("Convert all dependencies of a type to another dependency type")
                    tSuccess = True
                Case "set"
                    'set dependency for the cell Alt1ID/Alt2ID
                    Dim tDep As Integer = CInt(GetParam(args, "type"))
                    Dim Alt1ID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "i")) 'Alt1ID + Anti-XSS
                    Dim Alt2ID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "j")) 'Alt2ID + Anti-XSS

                    Dim exAlt1ID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ex_alt1_id")) 'Alt1ID + Anti-XSS
                    Dim exAlt2ID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ex_alt2_id")) 'Alt2ID + Anti-XSS                    

                    Select Case tDep 
                        Case RADependencyType.dtDependsOn, RADependencyType.dtConcurrent, RADependencyType.dtSuccessive, RADependencyType.dtMutuallyDependent, RADependencyType.dtMutuallyExclusive
                           DropdownSelectedDependency = CType(tDep, RADependencyType)
                    End Select

                    Select Case tDep
                        Case RADependencyType.dtMutuallyDependent, RADependencyType.dtMutuallyExclusive
                            Dim index As Integer = RemoveOldDependency(Optimizer.Dependencies, exAlt1ID, exAlt2ID)
                            Dim c As Integer = Optimizer.Dependencies.DeleteDependency(Alt1ID, Alt2ID)
                            c = Optimizer.TimePeriodsDependencies.DeleteDependency(Alt1ID, Alt2ID)
                            Optimizer.Dependencies.Dependencies.Add(New RADependency(Alt1ID, Alt2ID, CType(tDep, RADependencyType)))
                            MoveNewDependency(Optimizer.Dependencies, index)

                            If tDep = RADependencyType.dtMutuallyExclusive Then Optimizer.TimePeriodsDependencies.DeleteDependency(Alt1ID, Alt2ID) ' mutually exclusive alts can't have time periods dependencies
                            tSuccess = True
                            sComment = String.Format("Set as '{2}' for '{0}'/'{1}'", GetAltName(Alt1ID), GetAltName(Alt2ID), ResString(DepTypeNames(tDep))) ' D3790
                        Case RADependencyType.dtDependsOn, RADependencyType.dtConcurrent, RADependencyType.dtSuccessive
                            ' add Depends On dependency
                            Dim index As Integer = RemoveOldDependency(Optimizer.Dependencies, exAlt1ID, exAlt2ID)
                            Optimizer.Dependencies.DeleteDependency(Alt1ID, Alt2ID)
                            Optimizer.Dependencies.Dependencies.Add(New RADependency(Alt1ID, Alt2ID, RADependencyType.dtDependsOn))
                            MoveNewDependency(Optimizer.Dependencies, index)

                            ' add Successive/Concurrent dependency
                            index = RemoveOldDependency(Optimizer.TimePeriodsDependencies, exAlt1ID, exAlt2ID)
                            Optimizer.TimePeriodsDependencies.DeleteDependency(Alt1ID, Alt2ID)
                            If tDep = RADependencyType.dtDependsOn OrElse tDep = RADependencyType.dtConcurrent Then
                                Optimizer.TimePeriodsDependencies.Dependencies.Add(New RADependency(Alt1ID, Alt2ID, RADependencyType.dtConcurrent))
                            Else
                                Optimizer.TimePeriodsDependencies.Dependencies.Add(New RADependency(Alt1ID, Alt2ID, RADependencyType.dtSuccessive))
                            End If
                            MoveNewDependency(Optimizer.TimePeriodsDependencies, index)
                            tSuccess = True
                            sComment = String.Format("Set as '{2}' for '{0}'/'{1}'", GetAltName(Alt1ID), GetAltName(Alt2ID), ResString(DepTypeNames(tDep)))
                        Case RADependencyType.dtLag
                            Dim value As Integer = 0
                            If Integer.TryParse(GetParam(args, "value"), value) Then ' Lag value
                                Dim condition As Integer = CInt(GetParam(args, "condition")) ' Lag condition
                                Dim ubound As Integer = ECCore.UNDEFINED_INTEGER_VALUE

                                If Not Integer.TryParse(GetParam(args, "ubound"), ubound) Then ' Lag upper bound
                                    ubound = ECCore.UNDEFINED_INTEGER_VALUE
                                End If

                                ' add Depends On dependency
                                Dim index As Integer = RemoveOldDependency(Optimizer.Dependencies, exAlt1ID, exAlt2ID)
                                Optimizer.Dependencies.DeleteDependency(Alt1ID, Alt2ID)
                                Optimizer.Dependencies.Dependencies.Add(New RADependency(Alt1ID, Alt2ID, RADependencyType.dtDependsOn))
                                MoveNewDependency(Optimizer.Dependencies, index)

                                ' add Lag dependecy
                                index = RemoveOldDependency(Optimizer.TimePeriodsDependencies, exAlt1ID, exAlt2ID)
                                Optimizer.TimePeriodsDependencies.DeleteDependency(Alt1ID, Alt2ID)
                                Dim tLagDep As RADependency = New RADependency(Alt1ID, Alt2ID, RADependencyType.dtLag)
                                tLagDep.Lag = value
                                tLagDep.LagCondition = CType(condition, LagCondition)
                                tLagDep.LagUpperBound = ubound
                                Optimizer.TimePeriodsDependencies.Dependencies.Add(tLagDep)
                                MoveNewDependency(Optimizer.TimePeriodsDependencies, index)

                                tSuccess = True
                                sComment = String.Format("Set lag = '{2}' for '{0}'/'{1}'", GetAltName(Alt1ID), GetAltName(Alt2ID), value)
                            End If
                        Case -1 ' clear cell
                            Optimizer.Dependencies.DeleteDependency(Alt1ID, Alt2ID)
                            Optimizer.TimePeriodsDependencies.DeleteDependency(Alt1ID, Alt2ID)
                            tSuccess = True
                            sComment = String.Format("Reset '{0}'/'{1}'", GetAltName(Alt1ID), GetAltName(Alt2ID))
                    End Select
            End Select         
            tResult = String.Format("['{0}']", tAction)              
        End If

        If tSuccess Then App.ActiveProject.SaveRA("Update dependencies", , , sComment) ' D3790

        If (tAction = "set" OrElse tAction = "convert_all") AndAlso RA.Scenarios.GlobalSettings.DependenciesView Then
            tResult = String.Format("['{0}',{1},{2}]", tAction, LoadDependenciesData(), LoadTPDependenciesData())
        End If

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Sub RADependenciesPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        RA.Load()
    End Sub

End Class