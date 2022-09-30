Partial Class BayesianUpdatingPage
    Inherits clsComparionCorePage

    Public Const OPT_PRECISION As Integer = 4
    Public Const COL_ID As Integer = 0
    Public Const COL_INDEX As Integer = 1
    Public Const COL_NAME As Integer = 2
    Public Const OPT_NAME_COL_WIDTH As Integer = 200
    Public Const OPT_DATA_COL_WIDTH As Integer = 200

    Private SUMMARY_HEADER_BACKGROUND As System.Drawing.Color = System.Drawing.Color.FromArgb(240, 245, 255)
    Private ALTERNATE_ROW_BACKGROUND As System.Drawing.Color = System.Drawing.Color.FromArgb(250, 250, 250)

    Private PagePrefix As String = "Bayesian Updating"

    Public Enum BayesianModes
        bmProbability = 0
        bmOdds = 1
    End Enum

    Public Sub New()
        MyBase.New(_PGID_BAYESIAN_UPDATING)
    End Sub

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return PRJ.ProjectManager
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean
        Get
            With App
                Return Not .CanUserModifyProject(.ActiveUser.UserID, PRJ.ID, .ActiveUserWorkgroup, .DBWorkspaceByUserIDProjectID(.ActiveUser.UserID, PRJ.ID), .ActiveWorkgroup)
            End With
        End Get
    End Property

    'todo AC - move to Bayesian class
    Public Property BayesianMode As BayesianModes
        Get
            Return CType(PM.Parameters.Riskion_BayesianMode, BayesianModes)
        End Get
        Set(value As BayesianModes)
            PM.Parameters.Riskion_BayesianMode = CInt(value)
            PM.Parameters.Save()
        End Set
    End Property

    'todo AC - move to Bayesian class
    Public BayesianUseGlobally As Boolean = False

    Public Function GetColumns() As String
        Dim retVal As String = String.Format("[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ResString("optUniqueID"), "false", "td_center", "", "false", "id")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ResString("optIndexID"), "false", "dt-body-right", "", "false", "alt_idx")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ParseString("Information"), "true", "dt-body-left", "", "false", "data_name")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ParseString("%%Alternative%% (E)"), "true", "dt-body-left", "", "false", "name")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ParseString("Odds: Prior P(E)"), "false", "dt-body-right", "html", "false", "prior_odds")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ParseString("Prior P(E)"), "true", "dt-body-right", "html", "false", "prior")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ParseString("%%Source%% of Prior"), "false", "dt-body-left", "", "false", "source_prior")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ParseString("Data GUID"), "false", "td_center", "", "false", "data_guid")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, "P (I/E)", "true", "dt-body-left", "", "false", "prob_e")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, "P (I/&#274;)", "true", "dt-body-left", "", "false", "prob_not_e")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ParseString("LR"), "false", "td_center", "", "false", "lr")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, "Odds: Prior (E)", "false", "td_center", "", "false", "prior_e_odds")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, "Odds: Posterior (E)", "false", "td_center", "", "false", "post_e_odds")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, "Posterior Probability (P(E))", "true", "td_center", "", "false", "post_e")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}']", 0, ParseString("Actions"), "true", "td_center", "", "false", "actions")

        Return String.Format("[{0}]", retVal)
    End Function

    Private ReadOnly Property BCD As BayesianUpdating
        Get
            Return PM.BayesianCalculationManager
        End Get
    End Property    

    Public Function GetBayesianData() As String        
        BCD.Calculate()

        'If BCD.BayesData.Count = 0 AndAlso PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count > 0 Then
        '    For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
        '        BCD.BayesData.Add(New BayesianDataSource With {.DataGuid = Guid.NewGuid, .EventGuid = alt.NodeGuidID, .EventIndex = alt.Index, .Prior = Math.Round(alt.UnnormalizedPriority, OPT_PRECISION)})
        '    Next
        'End If

        Dim orderedAlts = PM.ActiveAlternatives.TerminalNodes.OrderBy(Function (a) a.EventType).ToList

        Dim retVal As New List(Of BayesPageDataSource)
        For Each el As BayesianDataSource In BCD.BayesData
            Dim alt As clsNode = PM.ActiveAlternatives.GetNodeByID(el.EventGuid)
            Dim altIndex As Integer = orderedAlts.IndexOf(alt)
            retVal.Add(New BayesPageDataSource With {.id = el.EventGuid.ToString, .alt_idx = altIndex, .alt_event_type = CInt(alt.EventType), .name = alt.NodeName, .prior_odds = Math.Round(el.PriorOdds, OPT_PRECISION), .prior = Math.Round(el.Prior, OPT_PRECISION), .source_prior = el.SourceOfPriorGuid.ToString, .prob_e = el.Probability_E, .prob_e_comment = el.Probability_E_Comment, .prob_not_e = el.Probability_Not_E, .prob_not_e_comment = el.Probability_Not_E_Comment, .lr = Math.Round(el.LR, OPT_PRECISION), .data_guid = el.DataGuid.ToString, .data_name = el.DataName, .data_comment = el.DataComment, .actions = "", .prior_e_odds = Math.Round(el.Prior_E_Odds, OPT_PRECISION), .post_e_odds = Math.Round(el.Posterior_E_Odds, OPT_PRECISION), .post_e = Math.Round(el.Posterior_E, OPT_PRECISION)})
        Next

        Return Newtonsoft.Json.JsonConvert.SerializeObject(retVal)
    End Function

    Private ReadOnly Property SESS_LAST_EVENT_ID As String
        Get
            Return String.Format("LAST_EVENT_ID_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property LastEventID As Guid
        Get
            Dim tSessVar = Session(SESS_LAST_EVENT_ID)
            If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Guid Then
                Return CType(tSessVar, Guid)
            End If
            Return Guid.Empty
        End Get
        Set(value As Guid)
            Session(SESS_LAST_EVENT_ID) = value
        End Set
    End Property

    Private ReadOnly Property SESS_LAST_DATA_ID As String
        Get
            Return String.Format("LAST_DATA_ID_{0}", App.ProjectID.ToString)
        End Get
    End Property
    
    Public Function GetEventsDropdown() As String
        Dim selNodeID As Guid = Guid.Empty
        Dim nodes As List(Of clsNode) = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
        If nodes.Count > 0 Then selNodeID = nodes(0).NodeGuidID
        If PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(LastEventID) IsNot Nothing Then selNodeID = LastEventID
        Dim lblRisk As String = ParseString("%%Risk%%")
        Dim lblOpportunity As String = ParseString("%%Opportunity%%")
        Dim sRes As String = ""
        For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes.OrderBy(Function (a) a.EventType)
            sRes += String.Format("<option value='{0}' {2}>[{3}] {1}</option>", alt.NodeGuidID, SafeFormString(ShortString(alt.NodeName, 100)), If(alt.NodeGuidID = selNodeID, " selected", ""), If(alt.EventType = EventType.Risk, lblRisk, lblOpportunity))
        Next
        Return String.Format("<select id='cbEvents' disabled='disabled;' style='width:400px; margin-top:0px; margin-right:2px; vertical-align: top;' onchange='getEventPE();'>{0}</select>", sRes)
    End Function

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not isCallback AndAlso Not IsPostBack Then
            PM.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(COMBINED_USER_ID)), PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy)
        End If
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Ajax_Callback(Request.Form.ToString)
    End Sub

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim tAction As String = CheckVar("action", "").ToLower
        Dim tResult As String = tAction

        If Not String.IsNullOrEmpty(tAction) Then
            Select Case tAction
                Case "calc_mode"
                    BayesianMode = CType(Param2Int(args, "value"), BayesianModes)
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetBayesianData())
                Case "bayes_global"
                    BayesianUseGlobally = Param2Bool(args, "chk")
                    tResult = String.Format("[""{0}""]", tAction)
                Case "get_event_pe"
                    Dim EventID As Guid = New Guid(CheckVar("event_id", "").ToLower)
                    Dim sPDE As String = CheckVar("pde", "").ToLower
                    Dim sPD_E As String = CheckVar("pd_e", "").ToLower
                    Dim DataGuid As Guid = Guid.Empty 
                    Dim sDataGuid As String = CheckVar("data_guid", "").ToLower
                    If Not String.IsNullOrEmpty(sDataGuid) Then DataGuid = New Guid(sDataGuid)

                    Dim Alt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(EventID)

                    Dim PriorPE As Double = UNDEFINED_INTEGER_VALUE
                    Dim PE As Double = UNDEFINED_INTEGER_VALUE

                    Dim PDE As Double
                    Dim PD_E As Double
                    If Not String2Double(sPDE, PDE) 
                        PDE = UNDEFINED_INTEGER_VALUE
                    Else 
                        If PDE > 1 Then PDE = 1
                        If PDE < 0 Then PDE = 0
                    End If
                    If Not String2Double(sPD_E, PD_E) Then
                        PD_E = UNDEFINED_INTEGER_VALUE
                    Else
                        If PD_E > 1 Then PD_E = 1
                        If PD_E < 0 Then PD_E = 0
                    End If

                    If DataGuid = Guid.Empty Then ' new data
                        Dim NewData As BayesianDataSource = New BayesianDataSource With {.EventGuid = EventID, .DataGuid = Guid.NewGuid, .EventIndex = Alt.Index, .Probability_E = PDE, .Probability_Not_E = PD_E}

                        ' Add new data                            
                        Dim fChanged As Boolean = False
                        For i As Integer = BCD.BayesData.Count - 1 To 0 Step -1
                            If BCD.BayesData(i).EventGuid = EventID Then 
                                BCD.BayesData.Insert(i + 1, NewData)
                                fChanged = True
                                Exit For
                            End If
                        Next

                        If Not fChanged Then 
                            BCD.BayesData.Add(NewData)
                        End If

                        BCD.Calculate()

                        PriorPE = If(BayesianMode = BayesianModes.bmProbability, NewData.Prior, NewData.Prior_E_Odds)
                        PE = If(BayesianMode = BayesianModes.bmProbability, NewData.Posterior_E, NewData.Posterior_E_Odds)

                        ' Delete new data after calculating
                        If BCD.BayesData.Contains(NewData) Then BCD.BayesData.Remove(NewData)
                    Else ' existing data

                        For i As Integer = BCD.BayesData.Count - 1 To 0 Step -1
                            If BCD.BayesData(i).DataGuid = DataGuid Then
                                Dim ExistingData As BayesianDataSource = BCD.BayesData(i)
                                Dim oldPDE = ExistingData.Probability_E, oldPD_E = ExistingData.Probability_Not_E

                                ExistingData.Probability_E = PDE
                                ExistingData.Probability_Not_E = PD_E

                                BCD.Calculate()

                                PriorPE = If(BayesianMode = BayesianModes.bmProbability, ExistingData.Prior, ExistingData.Prior_E_Odds)
                                PE = If(BayesianMode = BayesianModes.bmProbability, ExistingData.Posterior_E, ExistingData.Posterior_E_Odds)

                                ExistingData.Probability_E = oldPDE
                                ExistingData.Probability_Not_E = oldPD_E

                                Exit For
                            End If
                        Next
                    End If
                    tResult = String.Format("[""{0}"",{1},{2}]", tAction, JS_SafeNumber(Math.Round(PE, OPT_PRECISION)), JS_SafeNumber(Math.Round(PriorPE, OPT_PRECISION)))
                Case "add_an_update_info"
                    Dim EventID As Guid = New Guid(CheckVar("event_id", "").ToLower)
                    Dim DataGuid As Guid = Guid.Empty 
                    Dim sDataGuid As String = CheckVar("data_guid", "").ToLower
                    If Not String.IsNullOrEmpty(sDataGuid) Then DataGuid = New Guid(sDataGuid)
                    Dim DataName As String = CheckVar("data_name", "")
                    Dim sPDE As String = CheckVar("pde", "").ToLower
                    Dim sPD_E As String = CheckVar("pd_e", "").ToLower
                    Dim sDataComment As String = CheckVar("data_comment", "")
                    Dim sPDEComment As String = CheckVar("pde_comment", "")
                    Dim sPD_EComment As String = CheckVar("pd_e_comment", "")
                    Dim SnapshotMessage As String = "After adding Bayesian data"

                    Dim Alt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(EventID)

                    Dim PDE As Double
                    Dim PD_E As Double
                    If Not String2Double(sPDE, PDE) 
                        PDE = UNDEFINED_INTEGER_VALUE
                    Else 
                        If PDE > 1 Then PDE = 1
                        If PDE < 0 Then PDE = 0
                    End If
                    If Not String2Double(sPD_E, PD_E) Then
                        PD_E = UNDEFINED_INTEGER_VALUE
                    Else
                        If PD_E > 1 Then PD_E = 1
                        If PD_E < 0 Then PD_E = 0
                    End If

                    LastEventID = EventID
                    Dim NewData As BayesianDataSource = New BayesianDataSource With {.EventGuid = EventID, .DataGuid = Guid.NewGuid, .DataName = DataName, .DataComment = sDataComment, .EventIndex = Alt.Index, .Probability_E = PDE, .Probability_Not_E = PD_E, .Probability_E_Comment = sPDEComment, .Probability_Not_E_Comment = sPD_EComment}

                    If DataGuid = Guid.Empty Then
                        ' Add new data                            
                        Dim fChanged As Boolean = False
                        For i As Integer = BCD.BayesData.Count - 1 To 0 Step -1
                            If BCD.BayesData(i).EventGuid = EventID Then 
                                BCD.BayesData.Insert(i + 1, NewData)
                                fChanged = True
                                Exit For
                            End If
                        Next

                        If Not fChanged Then 
                            BCD.BayesData.Add(NewData)
                        End If
                    Else
                        ' Update existing data
                        For i As Integer = BCD.BayesData.Count - 1 To 0 Step -1
                            If BCD.BayesData(i).DataGuid = DataGuid Then
                                NewData.DataGuid = DataGuid 
                                BCD.BayesData(i) = NewData
                                Exit For
                            End If
                        Next

                        SnapshotMessage = "After editing Bayesian data"
                    End If

                    BCD.Save()

                    App.SnapshotSaveProject(ecSnapShotType.RestorePoint, SnapshotMessage, App.ProjectID, True, DataName)

                    tResult = String.Format("[""{0}"",{1}]", tAction, GetBayesianData())                    
                Case "delete_info"
                    Dim DataGuid As Guid = New Guid(CheckVar("data_guid", "").ToLower)
                    Dim fChanged As Boolean = False
                    Dim DataName As String = ""

                    For i As Integer = BCD.BayesData.Count - 1 To 0 Step -1
                        If BCD.BayesData(i).DataGuid = DataGuid Then 
                            DataName = BCD.BayesData(i).DataName
                            BCD.BayesData.RemoveAt(i)
                            fChanged = True
                            Exit For
                        End If
                    Next

                    If fChanged Then
                        BCD.Save()

                        App.SnapshotSaveProject(ecSnapShotType.RestorePoint , "After deleting Bayesian data", App.ProjectID, True, DataName)
                    End If                                           
                    
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetBayesianData())
                Case "set_pde", "set_pd_e"
                    Dim DataGuid As Guid = New Guid(CheckVar("data_guid", "").ToLower)
                    Dim sValue As String = CheckVar("value", "").Trim()
                    Dim Value As Double
                    Dim fChanged As Boolean = False
                    Dim DataName As String = ""

                    For i As Integer = BCD.BayesData.Count - 1 To 0 Step -1
                        If BCD.BayesData(i).DataGuid = DataGuid Then
                            DataName = BCD.BayesData(i).DataName
                            If tAction = "set_pde" Then 
                                If sValue = "" OrElse Not String2Double(sValue, Value) Then 
                                    BCD.BayesData(i).Probability_E = UNDEFINED_INTEGER_VALUE
                                    fChanged = True
                                Else
                                    If String2Double(sValue, Value) Then
                                        If Value > 1 Then Value = 1
                                        If Value < 0 Then Value = 0
                                        BCD.BayesData(i).Probability_E = Value                                        
                                        fChanged = True
                                    End If
                                End If
                                DataName += " (P (I/E))"
                            End If
                            If tAction = "set_pd_e" Then 
                                If sValue = "" OrElse Not String2Double(sValue, Value) Then 
                                    BCD.BayesData(i).Probability_Not_E = UNDEFINED_INTEGER_VALUE
                                    fChanged = True
                                Else
                                    If String2Double(sValue, Value) Then
                                        If Value > 1 Then Value = 1
                                        If Value < 0 Then Value = 0
                                        BCD.BayesData(i).Probability_Not_E = Value
                                        fChanged = True
                                    End If
                                End If
                                DataName += " (P (I/&#274;))"
                            End If
                            Exit For
                        End If
                    Next

                    If fChanged Then
                        BCD.Save()
                        App.SnapshotSaveProject(ecSnapShotType.RestorePoint , "After editing Bayesian data", App.ProjectID, True, DataName)
                    End If                                           
                    
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetBayesianData())
            End Select
        End If

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

End Class