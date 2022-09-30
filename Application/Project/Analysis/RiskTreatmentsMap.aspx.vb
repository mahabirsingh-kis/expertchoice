Option Strict On
Partial Class RiskSelectTreatmentsMapPage
    Inherits clsComparionCorePage

    Public Enum ecTreatmentsPageType
        ctContribution = 0
        ctEffectiveness = 1
        ctMeausurement = 2
    End Enum

    Public Const _PRIORITY_DECIMALS As Integer = 4
    Public Const _PRIORITY_FORMAT As String = "f4"
    Public Const _COST_FORMAT As String = "f2"
    Public Const _PARAM_DELIMITER As Char = CChar(";")

    Public Const LogMessagePrefix As String = "%%Risk%% %%Controls%%:" + " "

    Public Sub New()
        MyBase.New(_PGID_RISK_TREATMENTS_MAP_CAUSES)
    End Sub
    
    Private _ControlType As ControlType = ControlType.ctCause
    Public Property ControlType As ControlType
        Get
            Return _ControlType
        End Get
        Set(value As ControlType)
            _ControlType = value
        End Set
    End Property

    Private _PageType As ecTreatmentsPageType = ecTreatmentsPageType.ctContribution
    Public Property PageType As ecTreatmentsPageType 
        Get
            Return _PageType
        End Get
        Set(value As ecTreatmentsPageType)
            _PageType = value
        End Set
    End Property

    Public Property MultiselectEnabled As Boolean
        Get
            Return Not IsByTreatmentPage AndAlso CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MULTISELECT_ENABLED_ID, UNDEFINED_USER_ID)) AndAlso PageType = ecTreatmentsPageType.ctContribution
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MULTISELECT_ENABLED_ID, AttributeValueTypes.avtBoolean, value, ParseString(LogMessagePrefix + "Multiselect enabled changed to " + value.ToString))
        End Set
    End Property

    Public ReadOnly Property IsReadOnly As Boolean
        Get
            With App
                Return Not .CanUserModifyProject(.ActiveUser.UserID, PRJ.ID, .ActiveUserWorkgroup, .DBWorkspaceByUserIDProjectID(.ActiveUser.UserID, PRJ.ID), .ActiveWorkgroup)
            End With
        End Get
    End Property

    Public Function CanUserAddControls() As Boolean
        Return Not IsReadOnly AndAlso Not (ControlType = ControlType.ctCause AndAlso PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes.Count <= 1) OrElse (ControlType = ControlType.ctCauseToEvent AndAlso PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count = 0) OrElse (ControlType = ControlType.ctConsequenceToEvent AndAlso (PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count = 0 OrElse PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes.Count <= 1))
    End Function

    Public Property SelectedEventID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SELECTED_EVENT_ID, UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(s) AndAlso s.Length > 16 Then retVal = New Guid(s)
            Dim AH = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            If retVal.Equals(Guid.Empty) AndAlso AH IsNot Nothing Then
                Dim tAlt As clsNode = AH.GetNodeByID(retVal)
                If (tAlt Is Nothing OrElse Not tAlt.Enabled) Then
                    Dim tEnabledAlts As List(Of clsNode) = AH.Nodes.Where(Function (node) node.Enabled).ToList
                    If tEnabledAlts.Count > 0 Then retVal = tEnabledAlts(0).NodeGuidID
                End If
            End If
            Return retVal
        End Get
        Set(value As Guid)
            WriteSetting(PRJ, ATTRIBUTE_RISK_SELECTED_EVENT_ID, AttributeValueTypes.avtString, value.ToString, ParseString(LogMessagePrefix + "Selected Event ID changed"))
        End Set
    End Property

    Public Property SelectedTreatmentID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SELECTED_TREATMENT_ID, UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(s) AndAlso s.Length > 16 Then retVal = New Guid(s)
            Dim tList = Treatments.Where(Function (c) c.Type = ControlType)
            If tList IsNot Nothing AndAlso (retVal.Equals(Guid.Empty) OrElse GetControlByID(retVal) Is Nothing) AndAlso tList.Count > 0 Then retVal = tList(0).ID
            Return retVal
        End Get
        Set(value As Guid)
            WriteSetting(PRJ, ATTRIBUTE_RISK_SELECTED_TREATMENT_ID, AttributeValueTypes.avtString, value.ToString, ParseString(LogMessagePrefix + " Selected %%Control%% ID changed"))
        End Set
    End Property

    Public Property SelectedUserID As Integer
        Get
            If Not isCallback AndAlso Not IsPostBack Then Return PM.UserID
            Return If(PM.Parameters.Riskion_ControlsSelectedUser > Integer.MinValue, PM.Parameters.Riskion_ControlsSelectedUser, PM.UserID)
        End Get
        Set(value As Integer)
            PM.Parameters.Riskion_ControlsSelectedUser = value
            PM.Parameters.Save()
        End Set
    End Property


    Public ReadOnly Property IsByTreatmentPage As Boolean
        Get
            Return CurrentPageID = _PGID_RISK_TREATMENTS_MAP_CONSEQUENCES_BY_TREATMENT OrElse CurrentPageID = _PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES_BY_TREATMENT OrElse CurrentPageID = _PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES_BY_TREATMENT _
                OrElse CurrentPageID = _PGID_RISK_TREATMENTS_MAP_VULNERABILITIES_BY_TREATMENT OrElse CurrentPageID = _PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES_BY_TREATMENT OrElse CurrentPageID = _PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES_BY_TREATMENT
        End Get
    End Property

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim sPgid As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("pgid", _PGID_RISK_TREATMENTS_MAP_CAUSES.ToString)).ToLower   ' Anti-XSS
        CurrentPageID = CInt(sPgid)

        Select Case CurrentPageID
            Case _PGID_RISK_TREATMENTS_MAP_CAUSES, _PGID_RISK_TREATMENTS_MEASURE_CAUSES, _PGID_RISK_TREATMENTS_EFFECT_CAUSES
                ControlType = ControlType.ctCause
            Case _PGID_RISK_TREATMENTS_MAP_CONSEQUENCES, _PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES, _PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES, _PGID_RISK_TREATMENTS_MAP_CONSEQUENCES_BY_TREATMENT, _PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES_BY_TREATMENT, _PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES_BY_TREATMENT
                ControlType = ControlType.ctConsequenceToEvent
            Case _PGID_RISK_TREATMENTS_MAP_VULNERABILITIES, _PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES, _PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES, _PGID_RISK_TREATMENTS_MAP_VULNERABILITIES_BY_TREATMENT, _PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES_BY_TREATMENT, _PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES_BY_TREATMENT
                ControlType = ControlType.ctCauseToEvent
        End Select

        Select Case CurrentPageID
            Case _PGID_RISK_TREATMENTS_MAP_CAUSES, _PGID_RISK_TREATMENTS_MAP_CONSEQUENCES, _PGID_RISK_TREATMENTS_MAP_VULNERABILITIES, _PGID_RISK_TREATMENTS_MAP_CONSEQUENCES_BY_TREATMENT, _PGID_RISK_TREATMENTS_MAP_VULNERABILITIES_BY_TREATMENT
                PageType = ecTreatmentsPageType.ctContribution
            Case _PGID_RISK_TREATMENTS_MEASURE_CAUSES, _PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES, _PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES, _PGID_RISK_TREATMENTS_MEASURE_CONSEQUENCES_BY_TREATMENT, _PGID_RISK_TREATMENTS_MEASURE_VULNERABILITIES_BY_TREATMENT
                PageType = ecTreatmentsPageType.ctMeausurement
            Case _PGID_RISK_TREATMENTS_EFFECT_CAUSES, _PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES, _PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES, _PGID_RISK_TREATMENTS_EFFECT_CONSEQUENCES_BY_TREATMENT, _PGID_RISK_TREATMENTS_EFFECT_VULNERABILITIES_BY_TREATMENT
                PageType = ecTreatmentsPageType.ctEffectiveness
        End Select
    End Sub

    ReadOnly Property SESS_TREATMENTS As String
        Get
            Return String.Format("RISK_TREATMENTS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public _Treatments As List(Of clsControl) = Nothing
    Public Property Treatments As List(Of clsControl)
        Get
            If _Treatments Is Nothing Then
                Dim tSessVar = Session(SESS_TREATMENTS)
                If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is List(Of clsControl) Then
                    _Treatments = CType(tSessVar, List(Of clsControl))
                End If
            End If
            Return _Treatments
        End Get
        Set(value As List(Of clsControl))
            _Treatments = value.Where(Function (ctrl) ctrl.Type <> ControlType.ctUndefined AndAlso ctrl.Enabled).ToList
            Session(SESS_TREATMENTS) = value
        End Set
    End Property

    Public Function GetTreatmentByName(ctrlName As String, ctrlType As ControlType) As clsControl
        ctrlName = ctrlName.Trim
        Dim retVal As clsControl = Nothing
        With PM
            For Each control As clsControl In Treatments
                If control.Type = ctrlType AndAlso control.Name = ctrlName Then
                    retVal = control
                    Exit For
                End If
            Next
        End With

        Return retVal
    End Function

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower   ' Anti-XSS
        Ajax_Callback(Request.Form.ToString)
    End Sub

    Public Enum IDColumnModes
        UniqueID = 0
        IndexID = 1
    End Enum

    Public ReadOnly Property EventIDMode As IDColumnModes 
        Get
            Dim retVal As IDColumnModes = IDColumnModes.UniqueID
            Dim tSess As Object = SessVar("IDColumnMode" + App.ActiveProject.ID.ToString)
            If tSess IsNot Nothing Then 
                retVal = CType(CInt(tSess), IDColumnModes)
            End If
            Return retVal
        End Get
    End Property

    Private Function GetCellsData(ctrl As clsControl, selectedEvent As clsNode) As String
        If ctrl Is Nothing Then Return ""
        Dim apps As String = ""
        Select Case PageType
           Case ecTreatmentsPageType.ctContribution
                Select Case ControlType
                    Case ControlType.ctCause
                        For Each node In PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes
                            Dim isAssignmentActive As Boolean = False
                            For Each a As clsControlAssignment In ctrl.Assignments
                                If a.ObjectiveID = node.NodeGuidID Then
                                    isAssignmentActive = True 'a.IsActive
                                    Exit For
                                End If
                            Next
                            apps += String.Format(",""o{0}"":{1}", node.NodeID, Bool2JS(isAssignmentActive))
                        Next
                    Case ControlType.ctCauseToEvent, ControlType.ctConsequenceToEvent
                            If ControlType = ControlType.ctCauseToEvent Then
                                If PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives.Contains(selectedEvent) Then
                                    Dim isAssignmentActive As Boolean = False
                                    For Each a As clsControlAssignment In ctrl.Assignments
                                        If a.ObjectiveID = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID AndAlso a.EventID = selectedEvent.NodeGuidID Then
                                            isAssignmentActive = True 'a.IsActive
                                            Exit For
                                        End If
                                    Next
                                    apps += String.Format(",""no_source"":{0}", Bool2JS(isAssignmentActive))
                                Else
                                    apps += String.Format(",""no_source"":{0}", -1)
                                End If
                            End If
                            For Each node In PM.Hierarchy(If(ControlType = ControlType.ctCauseToEvent, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)).TerminalNodes
                                If node.GetContributedAlternatives.Contains(selectedEvent) Then
                                    Dim isAssignmentActive As Boolean = False
                                    For Each a As clsControlAssignment In ctrl.Assignments
                                        If a.ObjectiveID = node.NodeGuidID AndAlso a.EventID = selectedEvent.NodeGuidID Then
                                            isAssignmentActive = True 'a.IsActive
                                            Exit For
                                        End If
                                    Next
                                    apps += String.Format(",""o{0}"":{1}", node.NodeID, Bool2JS(isAssignmentActive))
                                Else
                                    apps += String.Format(",""o{0}"":{1}", node.NodeID, -1)
                                End If
                            Next
                End Select
            Case ecTreatmentsPageType.ctEffectiveness
                Select Case ControlType
                    Case ControlType.ctCause
                        For Each node In PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes
                            Dim assignmentValue As Double = -1
                            For Each a As clsControlAssignment In ctrl.Assignments
                                If a.ObjectiveID = node.NodeGuidID Then 'AndAlso a.IsActive Then
                                    assignmentValue = a.Value
                                    Exit For
                                End If
                            Next
                            apps += String.Format(",""o{0}"":{1}", node.NodeID, Double2String(assignmentValue, _PRIORITY_DECIMALS, , , True))
                        Next
                    Case ControlType.ctCauseToEvent, ControlType.ctConsequenceToEvent
                            If ControlType = ControlType.ctCauseToEvent Then
                                If PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives.Contains(selectedEvent) Then
                                    Dim assignmentValue As Double = -1
                                    For Each a As clsControlAssignment In ctrl.Assignments
                                        If a.ObjectiveID = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID AndAlso a.EventID = selectedEvent.NodeGuidID Then 'AndAlso a.IsActive Then
                                            assignmentValue = a.Value
                                            Exit For
                                        End If
                                    Next
                                    apps += String.Format(",""no_source"":{0}", Double2String(assignmentValue, _PRIORITY_DECIMALS, , , True))
                                Else
                                    apps += String.Format(",""no_source"":{0}", -1)
                                End If
                            End If
                            For Each node In PM.Hierarchy(If(ControlType = ControlType.ctCauseToEvent, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)).TerminalNodes
                                If node.GetContributedAlternatives.Contains(selectedEvent) Then
                                    Dim assignmentValue As Double = -1
                                    For Each a As clsControlAssignment In ctrl.Assignments
                                        If a.ObjectiveID = node.NodeGuidID AndAlso a.EventID = selectedEvent.NodeGuidID Then 'AndAlso a.IsActive Then
                                            assignmentValue = a.Value
                                            Exit For
                                        End If
                                    Next
                                    apps += String.Format(",""o{0}"":{1}", node.NodeID, Double2String(assignmentValue, _PRIORITY_DECIMALS, , , True))
                                Else
                                    apps += String.Format(",""o{0}"":{1}", node.NodeID, -1)
                                End If
                            Next
                End Select
            Case ecTreatmentsPageType.ctMeausurement                
                Dim ms As Guid = Guid.Empty
                Select Case ControlType
                    Case ControlType.ctCause
                        For Each node In PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes
                            Dim assignmentValue As Double = -1
                            For Each a As clsControlAssignment In ctrl.Assignments
                                If a.ObjectiveID = node.NodeGuidID Then 'AndAlso a.IsActive Then
                                    assignmentValue = CInt(a.MeasurementType)
                                    ms = a.MeasurementScaleGuid
                                    Exit For
                                End If
                            Next
                            apps += String.Format(",""o{0}"":{1}", node.NodeID, Double2String(assignmentValue, _PRIORITY_DECIMALS, , , True))
                            apps += String.Format(",""ms{0}"":""{1}""", node.NodeID, ms)
                        Next
                    Case ControlType.ctCauseToEvent, ControlType.ctConsequenceToEvent
                            If ControlType = ControlType.ctCauseToEvent Then
                                If PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives.Contains(selectedEvent) Then
                                    Dim assignmentValue As Double = -1
                                    For Each a As clsControlAssignment In ctrl.Assignments
                                        If a.ObjectiveID = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID AndAlso a.EventID = selectedEvent.NodeGuidID Then 'AndAlso a.IsActive Then
                                            assignmentValue = CInt(a.MeasurementType)
                                            ms = a.MeasurementScaleGuid
                                            Exit For
                                        End If
                                    Next
                                    apps += String.Format(",""no_source"":{0}", Double2String(assignmentValue, _PRIORITY_DECIMALS, , , True))
                                    apps += String.Format(",""no_source_ms"":""{0}""", ms)
                                Else
                                    apps += String.Format(",""no_source"":{0}", -1)
                                End If
                            End If
                            For Each node In PM.Hierarchy(If(ControlType = ControlType.ctCauseToEvent, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)).TerminalNodes
                                If node.GetContributedAlternatives.Contains(selectedEvent) Then
                                    Dim assignmentValue As Double = -1
                                    For Each a As clsControlAssignment In ctrl.Assignments
                                        If a.ObjectiveID = node.NodeGuidID AndAlso a.EventID = selectedEvent.NodeGuidID Then 'AndAlso a.IsActive Then
                                            assignmentValue = CInt(a.MeasurementType)
                                            ms = a.MeasurementScaleGuid
                                            Exit For
                                        End If
                                    Next
                                    apps += String.Format(",""o{0}"":{1}", node.NodeID, Double2String(assignmentValue, _PRIORITY_DECIMALS, , , True))
                                    apps += String.Format(",""ms{0}"":""{1}""", node.NodeID, ms)
                                Else
                                    apps += String.Format(",""o{0}"":{1}", node.NodeID, -1)
                                End If
                            Next
                End Select
        End Select
        Return apps
    End Function

    Public Function GetEventsData() As String
        Dim retVal As String = ""
        Dim tEventIDMode As IDColumnModes = EventIDMode
        Dim Alternatives As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        Dim ctrl As clsControl = PM.Controls.GetControlByID(SelectedTreatmentID)
        
        If Alternatives IsNot Nothing Then
            For Each selectedEvent As clsNode In If(tEventIDMode = IDColumnModes.UniqueID, Alternatives.TerminalNodes.OrderBy(Function(a) a.NodeID), Alternatives.TerminalNodes.OrderBy(Function(a) a.SOrder))
                If selectedEvent.Enabled Then
                    Dim ID As Integer = selectedEvent.SOrder
                    If tEventIDMode = IDColumnModes.UniqueID Then ID = selectedEvent.NodeID + 1                   
                    retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""index"":{2},""infodoc"":""{3}""{4}}}", selectedEvent.NodeGuidID, JS_SafeString(selectedEvent.NodeName), ID, JS_SafeString(Infodoc2Text(App.ActiveProject, selectedEvent.InfoDoc, True)), If(IsByTreatmentPage, GetCellsData(ctrl, selectedEvent), ""))
                End If
            Next
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Function GetControlData(ctrl As clsControl, index As Integer) As String
        'apps += CStr(IIf(apps <> "", ",", "")) + String.Format("{{""id"":""{0}"",""comment"":""{1}"",""value"":""{2}"",""obj_id"":""{3}"",""alt_id"":""{4}"",""mt"":""{5}"",""ms"":""{6}"",""active"":{7}}}", a.ID, JS_SafeString(a.Comment), Math.Round(a.Value(uid), 3).ToString, a.ObjectiveID, a.EventID, CInt(a.MeasurementType), a.MeasurementScaleGuid, Bool2JS(a.IsActive))
        Dim uid As Integer = SelectedUserID        
        Dim selectedEvent As clsNode = PM.ActiveAlternatives.GetNodeByID(SelectedEventID)

        Return String.Format("{{""id"":""{0}"",""name"":""{1}"",""cost"":""{3}"",""infodoc"":""{4}"",""cats"":""{5}"",""index"":{6}{2}}}", ctrl.ID, JS_SafeString(ctrl.Name), If(Not IsByTreatmentPage, GetCellsData(ctrl, selectedEvent), ""), ctrl.Cost.ToString(_COST_FORMAT), JS_SafeString(GetControlInfodoc(App.ActiveProject, ctrl, False)), CStr(IIf(ctrl.Categories Is Nothing, "", ctrl.Categories)), index)
    End Function

    Public Function GetControlsData() As String
        Dim retVal As String = ""
        Dim index As Integer = 0

        ControlsListReset()

        Dim sControlGuid As String = CheckVar("ctrl_id", "")
        If sControlGuid.Trim() <> "" Then 
            Dim tControlGuid As Guid = New Guid(sControlGuid)
            Treatments = ControlsList.Where(Function (ctrl) ctrl.ID.Equals(tControlGuid)).ToList
        Else
            Treatments = ControlsList
        End If

        For Each ctrl As clsControl In Treatments
            If ctrl.Type = ControlType AndAlso ctrl.Enabled Then
                retVal += CStr(IIf(retVal <> "", ",", "")) + GetControlData(ctrl, index + 1)
            End If
            index += 1
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetObjectivesColumns() As String
        Dim retVal As String = ""
        Dim Objectives As clsHierarchy = Nothing

        If ControlType = ControlType.ctConsequenceToEvent Then
            Objectives = PM.Hierarchy(ECHierarchyID.hidImpact)
        Else
            Objectives = PM.Hierarchy(ECHierarchyID.hidLikelihood)
        End If
        retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""alts"":[{2}],""children"":[{3}],""dataField"":""{4}"",""dataType"":""{5}"",""editable"":{6}}}", "", ResString("optIndexID"), "", "", "index", "number", Bool2JS(False))
        retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""alts"":[{2}],""children"":[{3}],""dataField"":""{4}"",""dataType"":""{5}"",""editable"":{6}}}", "", JS_SafeString(If(IsByTreatmentPage, ParseString("%%Alternative%% Name"), ParseString("%%Control%% Name"))), "", "", "name", "string", Bool2JS(False))

        'If PM.Parameters.Riskion_Control_ShowInfodocs Then 
        retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""alts"":[{2}],""children"":[{3}],""dataField"":""{4}"",""dataType"":""{5}"",""editable"":{6}}}", "", JS_SafeString("Description"), "", "", "infodoc", "string", Bool2JS(False))
        'End If

        Dim selectedEvent As clsNode = PM.ActiveAlternatives.GetNodeByID(SelectedEventID)
        Dim ucAlts = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives

        If selectedEvent IsNot Nothing AndAlso Not IsByTreatmentPage AndAlso ControlType = ControlType.ctCauseToEvent AndAlso ucAlts.Contains(selectedEvent) Then 
            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""alts"":[{2}],""children"":[{3}],""dataField"":""{4}"",""dataType"":""{5}"",""editable"":{6}}}", PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, JS_SafeString(ParseString("No Specific %%Objective(l)%%")), "", "", "no_source", "boolean", Bool2JS(Not IsReadOnly))
            Return String.Format("[{0}]", retVal)
        End If

        If ControlType = ControlType.ctCauseToEvent AndAlso IsByTreatmentPage AndAlso PM.ActiveAlternatives.Nodes.Where(Function (alt) ucAlts.Contains(alt)).Count > 0 Then 
            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""alts"":[{2}],""children"":[{3}],""dataField"":""{4}"",""dataType"":""{5}"",""editable"":{6}}}", PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, JS_SafeString(ParseString("No Specific %%Objective(l)%%")), "", "", "no_source", "boolean", Bool2JS(Not IsReadOnly))
        End If

        If ControlType = ControlType.ctCause Then
            For Each tItem As Tuple(Of Integer, Integer, clsNode) In Objectives.NodesInLinearOrder
                Dim obj As clsNode = tItem.Item3
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""alts"":[{2}],""children"":[{3}],""dataField"":""{4}"",""dataType"":""{5}"",""editable"":{6}}}", obj.NodeGuidID, JS_SafeString(obj.NodeName), getAlts(obj), "", "o" + obj.NodeID.ToString, "boolean", Bool2JS(obj.RiskNodeType <> RiskNodeType.ntCategory))
            Next
        Else
            Dim obj As clsNode = Objectives.Nodes(0)
            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""alts"":[{2}],""children"":[{3}],""dataField"":""{4}"",""dataType"":""{5}"",""editable"":{6}}}", obj.NodeGuidID, JS_SafeString(obj.NodeName), getAlts(obj), GetChildrenColumns(obj.Children), "o" + obj.NodeID.ToString, "boolean", Bool2JS(obj.RiskNodeType <> RiskNodeType.ntCategory))
        End If        

        Return String.Format("[{0}]", retVal)
    End Function

    Private Function getAlts(obj As clsNode) As String
        Dim alts As String = ""
        If obj.IsTerminalNode Then
            For Each alt As clsNode In obj.GetContributedAlternatives
                If alt.Enabled Then
                    alts += CStr(IIf(alts <> "", ",", "")) + "'" + alt.NodeGuidID.ToString + "'"
                End If
            Next
        End If
        Return alts
    End Function

    Private function GetChildrenColumns(nodes As List(Of clsNode)) As String
        Dim retVal As String = ""
        For Each obj As clsNode In nodes            
            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""alts"":[{2}],""children"":[{3}],""dataField"":""{4}"",""dataType"":""{5}"",""editable"":{6}}}", obj.NodeGuidID, JS_SafeString(obj.NodeName), getAlts(obj), GetChildrenColumns(obj.Children), "o" + obj.NodeID.ToString, "boolean", Bool2JS(obj.RiskNodeType <> RiskNodeType.ntCategory))
        Next
        Return String.Format("{0}", retVal)
    End Function

    Public Function GetScaleData() As String
        Dim retVal As String = ""
        With PM
            If .MeasureScales IsNot Nothing AndAlso .MeasureScales.AllScales.Count > 0 Then
                For Each scale As clsMeasurementScale In .MeasureScales.AllScales
                    If TypeOf scale Is clsRatingScale Then
                        Dim rs As clsRatingScale = CType(scale, clsRatingScale)
                        If (rs.Type = ScaleType.stShared OrElse rs.Type = ScaleType.stControls) AndAlso Not rs.IsExpectedValues AndAlso Not rs.IsOutcomes AndAlso Not rs.IsPWofPercentages Then
                            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""type"":{2}}}", rs.GuidID, JS_SafeString(rs.Name), CInt(ECMeasureType.mtRatings))
                        End If
                        'If (rs.Type = ScaleType.stShared OrElse rs.Type = ScaleType.stControls) AndAlso rs.IsOutcomes Then
                        '    retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""type"":{2}}}", rs.GuidID, JS_SafeString(rs.Name), CInt(ECMeasureType.mtPWOutcomes))
                        'End If
                    End If
                    If TypeOf scale Is clsStepFunction Then
                        Dim sf As clsStepFunction = CType(scale, clsStepFunction)
                        If sf.Type = ScaleType.stShared OrElse sf.Type = ScaleType.stControls Then
                            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""type"":{2}}}", sf.GuidID, JS_SafeString(sf.Name), CInt(ECMeasureType.mtStep))
                        End If
                    End If
                    If TypeOf (scale) Is clsRegularUtilityCurve Then
                        Dim uc As clsRegularUtilityCurve = CType(scale, clsRegularUtilityCurve)
                        If uc.Type = ScaleType.stShared OrElse uc.Type = ScaleType.stControls Then
                            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}"",""type"":{2}}}", uc.GuidID, JS_SafeString(uc.Name), CInt(ECMeasureType.mtRegularUtilityCurve))
                        End If
                    End If
                Next
            End If
        End With

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetCategoriesData() As String
        Dim retVal As String = ""
                
        With PM
            If .Attributes IsNot Nothing Then
                Dim attr As clsAttribute = .Attributes.GetAttributeByID(ATTRIBUTE_CONTROL_CATEGORY_ID)
                If attr IsNot Nothing AndAlso Not attr.EnumID.Equals(Guid.Empty) Then
                    Dim cat_attr_vals As clsAttributeEnumeration = .Attributes.GetEnumByID(attr.EnumID)
                    If cat_attr_vals IsNot Nothing Then
                        For Each item As clsAttributeEnumerationItem In cat_attr_vals.Items
                            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":""{0}"",""name"":""{1}""}}", item.ID, JS_SafeString(item.Value))
                        Next
                    End If
                End If
            End If
        End With

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetCategoryByName(CategoryName As String) As clsAttributeEnumerationItem
        Dim retVal As clsAttributeEnumerationItem = Nothing

        With PM
            If .Attributes IsNot Nothing Then
                Dim attr As clsAttribute = .Attributes.GetAttributeByID(ATTRIBUTE_CONTROL_CATEGORY_ID)
                If attr IsNot Nothing AndAlso Not attr.EnumID.Equals(Guid.Empty) Then
                    Dim cat_attr_vals As clsAttributeEnumeration = .Attributes.GetEnumByID(attr.EnumID)
                    If cat_attr_vals IsNot Nothing Then
                        For Each item As clsAttributeEnumerationItem In cat_attr_vals.Items
                            If item.Value = CategoryName Then retVal = item
                        Next
                    End If
                End If
            End If
        End With

        Return retVal
    End Function

    Public Function GetParticipants() As String
        Dim retVal As String = String.Format("{{""key"": -1, ""text"": ""{0}""}}", ParseString("All Participants"))

        For Each Group As clsCombinedGroup In App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.OrderBy(Function (g) g.Name)
            If Group.CombinedUserID <> COMBINED_USER_ID Then retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""key"":{0},""text"":""[{1}]""}}", Group.CombinedUserID, JS_SafeString(Group.Name))
        Next

        For Each User As clsUser In App.ActiveProject.ProjectManager.UsersList.OrderBy(Function (u) u.UserName)
            retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""key"":{0},""text"":""{1}""}}", User.UserID, JS_SafeString(User.UserName))
        Next

        Return retVal
    End Function

    Public Function GetControlByID(ID As Guid) As clsControl    ' D4859
        For Each ctrl As clsControl In Treatments.Where(Function(c) c.Type = ControlType)
            If ctrl.ID.Equals(ID) Then Return ctrl
        Next
        Return Nothing
    End Function

    Private Function GetControlAssignment(ctrl As clsControl, obj_id As Guid, event_id As Guid) As clsControlAssignment
        If ctrl IsNot Nothing Then
            For Each a As clsControlAssignment In ctrl.Assignments
                If a.ObjectiveID.Equals(obj_id) Then
                    If ControlType = ControlType.ctCause OrElse a.EventID.Equals(event_id) Then Return a
                End If
            Next
        End If
        Return Nothing
    End Function

    Private Function GetControlAssignment(ctrl As clsControl, obj_id As Integer, event_id As Guid) As clsControlAssignment
        If ctrl IsNot Nothing Then
            Dim node As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(obj_id)
            If node Is Nothing Then node = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(obj_id)
            For Each a As clsControlAssignment In ctrl.Assignments
                If a.ObjectiveID.Equals(node.NodeGuidID) Then
                    If ControlType = ControlType.ctCause OrElse a.EventID.Equals(event_id) Then Return a
                End If
            Next
        End If
        Return Nothing
    End Function

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = GetParam(args, "action")
        Dim tResult As String = "" 'CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction))

        With PM
            Select Case sAction
                ' D4344 ===
                Case "refresh"
                    tResult = If(IsByTreatmentPage, GetEventsData(), GetControlsData())
                    ' D4344 ==
                Case "activate"
                    Dim checked As Boolean = CheckVar("checked", True)
                    Dim obj_id As Guid = CheckVar("obj_id", Guid.Empty)
                    Dim control_id As Guid = CheckVar("control_id", Guid.Empty)

                    If ControlType = ControlType.ctCause Then
                        If checked Then
                            AddControlAssignment(control_id, obj_id)
                        Else
                            DeleteControlAssignment(control_id, obj_id)
                        End If
                    Else
                        Dim obj_ids As New List(Of Guid)
                        obj_ids.Add(obj_id)
                        Dim sEventID As String = GetParam(args, "event_id")
                        Dim event_ids As List(Of Guid) = New List(Of Guid)
                        If Not String.IsNullOrEmpty(sEventID) Then
                            Dim arr = sEventID.Split(_PARAM_DELIMITER)
                            If arr.Count > 0 Then
                                For Each item In arr
                                    event_ids.Add(New Guid(item))
                                Next
                            End If
                        End If
                        Dim control_ids = New List(Of Guid)
                        control_ids.Add(control_id)

                        If checked Then
                            AddControlsAssignments2(control_ids, obj_ids, event_ids)
                        Else
                            DeleteControlsAssignments2(control_ids, obj_ids, event_ids)
                        End If
                    End If
                    tResult = If(IsByTreatmentPage, GetEventsData(), GetControlsData())
                Case "mt" 'set treatment measure type                    
                    Dim mt As ECMeasureType = CType(CInt(GetParam(args, "mt")), ECMeasureType)
                    Dim obj_id As Guid = CheckVar("obj_id", Guid.Empty)
                    Dim control_id As Guid = CheckVar("control_id", Guid.Empty)
                    Dim event_id As Guid = CheckVar("event_id", Guid.Empty)
                    Dim ctrl As clsControl = PM.Controls.GetControlByID(control_id)
                    Dim appl As clsControlAssignment = GetControlAssignment(ctrl, obj_id, event_id)
                    If appl IsNot Nothing Then
                        Dim ms As Guid = Guid.Empty
                        For Each scale As clsMeasurementScale In PM.MeasureScales.AllScales
                            Select Case mt
                                Case ECMeasureType.mtRatings
                                    If TypeOf scale Is clsRatingScale AndAlso Not CType(scale, clsRatingScale).IsOutcomes AndAlso (CType(scale, clsRatingScale).Type = ScaleType.stShared OrElse CType(scale, clsRatingScale).Type = ScaleType.stControls) Then
                                        ms = scale.GuidID
                                        Exit For
                                    End If
                                Case ECMeasureType.mtPWOutcomes
                                    If TypeOf scale Is clsRatingScale AndAlso CType(scale, clsRatingScale).IsOutcomes AndAlso (CType(scale, clsRatingScale).Type = ScaleType.stShared OrElse CType(scale, clsRatingScale).Type = ScaleType.stControls) Then
                                        ms = scale.GuidID
                                        Exit For
                                    End If
                                Case ECMeasureType.mtStep
                                    If TypeOf scale Is clsStepFunction AndAlso (CType(scale, clsStepFunction).Type = ScaleType.stShared OrElse CType(scale, clsStepFunction).Type = ScaleType.stControls) Then
                                        ms = scale.GuidID
                                        Exit For
                                    End If
                                Case ECMeasureType.mtRegularUtilityCurve
                                    If TypeOf scale Is clsRegularUtilityCurve AndAlso (CType(scale, clsRegularUtilityCurve).Type = ScaleType.stShared OrElse CType(scale, clsRegularUtilityCurve).Type = ScaleType.stControls) Then
                                        ms = scale.GuidID
                                        Exit For
                                    End If
                            End Select
                        Next
                        SetControlMeasurementScale(control_id, appl.ID, mt, ms)
                    End If
                    tResult = If(IsByTreatmentPage, GetEventsData(), GetControlsData())
                Case "ms" ' set treatment measure scale
                    Dim ms As Guid = New Guid(GetParam(args, "ms"))
                    Dim obj_id As Guid = CheckVar("obj_id", Guid.Empty)
                    Dim control_id As Guid = CheckVar("control_id", Guid.Empty)
                    Dim event_id As Guid = CheckVar("event_id", Guid.Empty)
                    Dim ctrl As clsControl = PM.Controls.GetControlByID(control_id)
                    Dim appl As clsControlAssignment = GetControlAssignment(ctrl, obj_id, event_id)
                    If appl IsNot Nothing Then
                        SetControlMeasurementScale(control_id, appl.ID, appl.MeasurementType, ms)
                    End If
                    'tResult = If(IsByTreatmentPage, GetEventsData(), GetControlsData())
                Case "effectiveness"
                    Dim value As Double = 0
                    Dim fSuccess As Boolean = False
                    If String2Double(GetParam(args, "value"), value) Then
                        Dim obj_id As Guid = CheckVar("obj_id", Guid.Empty)
                        Dim control_id As Guid = CheckVar("control_id", Guid.Empty)
                        Dim event_id As Guid = CheckVar("event_id", Guid.Empty)
                        Dim ctrl As clsControl = PM.Controls.GetControlByID(control_id)
                        Dim appl As clsControlAssignment = GetControlAssignment(ctrl, obj_id, event_id)
                        If appl IsNot Nothing Then
                            Dim res As Boolean = PM.Controls.SetControlValue(ctrl.ID, appl.ID, value)
                            If res Then
                                WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% Effectiveness Changed"))
                                fSuccess = True
                                tResult = GetControlData(ctrl, Treatments.IndexOf(ctrl) + 1)
                            End If
                        End If
                    End If
                    If Not fSuccess Then tResult = String.Format("['{0}', -1]", sAction)
                Case "addcontrol"
                    Dim name As String = GetParam(args, "name").Trim
                    Dim categories As String = GetParam(args, "cat")
                    Dim cost As Double = UNDEFINED_INTEGER_VALUE
                    If Not String2Double(GetParam(args, "cost").Trim(), cost) Then cost = UNDEFINED_INTEGER_VALUE
                    Dim ctrl As clsControl = PM.Controls.AddControl(Guid.NewGuid, name, ControlType)    ' D4344
                    ctrl.Cost = cost
                    ctrl.Categories = categories
                    WriteControls(PRJ)
                    WriteAttributeValues(PRJ, ParseString(LogMessagePrefix + "%%Control%% added"), ctrl.Name)    ' D3731

                    'tResult = GetControlData(ctrl, Treatments.Count())
                    tResult = GetControlsData()
                Case "editcontrol"
                    Dim control_id As Guid = CheckVar("control_id", Guid.Empty)
                    Dim name As String = GetParam(args, "name").Trim
                    Dim categories As String = GetParam(args, "cat")
                    Dim cost As Double = 0
                    String2Double(GetParam(args, "cost"), cost)
                    Dim ctrl As clsControl = PM.Controls.GetControlByID(control_id)
                    If ctrl IsNot Nothing Then
                        ctrl.Name = name
                        'ctrl.InfoDoc = infodoc ' -D4344
                        ctrl.Cost = cost
                        ctrl.Categories = categories
                        WriteControls(PRJ)
                        WriteAttributeValues(PRJ, ParseString(LogMessagePrefix + "%%Control%% edited"), ctrl.Name)  ' D3731
                        tResult = GetControlData(ctrl, Treatments.IndexOf(ctrl) + 1)
                    End If
                Case "deletecontrol"
                    Dim control_id As Guid = CheckVar("control_id", Guid.Empty)
                    Dim res As Boolean = PM.Controls.DeleteControl(control_id)
                    ControlsListReset() ' D4245
                    Treatments = ControlsList   ' D4245
                    If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% deleted"))
                    tResult = GetControlsData()
                Case "activate_row"
                    Dim checked As Boolean = CheckVar("checked", True)
                    If IsByTreatmentPage Then
                        Dim control_id As Guid = New Guid(GetParam(args, "dropdown_element_id"))
                        Dim ctrlIDs As New List(Of Guid)
                        ctrlIDs.Add(control_id)
                        Dim sEventID As String = GetParam(args, "row_element_id")
                        Dim event_id As Guid = New Guid(sEventID)
                        Dim event_ids As List(Of Guid) = New List(Of Guid)
                        event_ids.Add(event_id)

                        If ControlType = ControlType.ctCause Then event_ids.Add(Guid.Empty) 'adding an empty guid so that the loop whoud execute once

                        Dim Alternatives As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                        Dim tAlt As clsNode = Alternatives.GetNodeByID(event_id)

                        Dim Objectives As clsHierarchy = Nothing
                        If ControlType = ControlType.ctConsequenceToEvent Then
                            Objectives = PM.Hierarchy(ECHierarchyID.hidImpact)
                        Else
                            Objectives = PM.Hierarchy(ECHierarchyID.hidLikelihood)
                        End If

                        Dim Control = GetControlByID(control_id)

                        Dim objIDs As New List(Of Guid)

                        If Objectives IsNot Nothing AndAlso Control IsNot Nothing Then
                            For Each obj As clsNode In If(ControlType = ControlType.ctConsequenceToEvent, Objectives.TerminalNodes, Objectives.Nodes)
                                Dim ContributedAlternatives = obj.GetContributedAlternatives
                                If ControlType = ControlType.ctCause OrElse (ContributedAlternatives IsNot Nothing AndAlso ContributedAlternatives.Contains(tAlt)) Then
                                    Dim tHasAssignment As Boolean = False
                                    For Each appl In Control.Assignments
                                        If ControlType = ControlType.ctCause Then
                                            If appl.ObjectiveID.Equals(obj.NodeGuidID) Then tHasAssignment = True
                                        Else
                                            If appl.ObjectiveID.Equals(obj.NodeGuidID) AndAlso appl.EventID.Equals(event_id) Then
                                                tHasAssignment = True
                                            End If
                                        End If
                                    Next
                                    If (checked AndAlso Not tHasAssignment) OrElse (Not checked AndAlso tHasAssignment) Then objIDs.Add(obj.NodeGuidID)
                                End If
                            Next
                        End If

                        If checked Then
                            If ControlType = ControlType.ctCause Then
                                AddControlsAssignments(ctrlIDs, objIDs)
                            Else
                                AddControlsAssignments2(ctrlIDs, objIDs, event_ids)
                            End If
                        Else
                            Dim applIDs As New List(Of Guid)
                            If Control IsNot Nothing Then
                                For Each appl In Control.Assignments
                                    If Objectives IsNot Nothing AndAlso Control IsNot Nothing Then
                                        For Each obj As clsNode In If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, Objectives.Nodes, Objectives.TerminalNodes)
                                            Dim ContributedAlternatives = obj.GetContributedAlternatives
                                            If ControlType = ControlType.ctCause OrElse (ContributedAlternatives IsNot Nothing AndAlso ContributedAlternatives.Contains(tAlt) AndAlso tAlt.Enabled) Then 'A1402
                                                If ControlType = ControlType.ctCause Then
                                                    If appl.ObjectiveID.Equals(obj.NodeGuidID) Then
                                                        applIDs.Add(appl.ID)
                                                    End If
                                                Else
                                                    If appl.ObjectiveID.Equals(obj.NodeGuidID) AndAlso appl.EventID.Equals(event_id) Then
                                                        applIDs.Add(appl.ID)
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                Next
                            End If
                            DeleteControlsAssignments(ctrlIDs, applIDs)
                        End If
                    Else
                        Dim control_id As Guid = New Guid(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "row_element_id")))
                        Dim sEventID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "dropdown_element_id"))  ' Anti-XSS
                        Dim event_ids As List(Of Guid) = New List(Of Guid)
                        If ControlType = ControlType.ctCause 
                            event_ids.Add(Guid.Empty) 'adding an empty guid so that the loop whoud execute once
                        Else
                            If Not String.IsNullOrEmpty(sEventID) Then
                                Dim arr = sEventID.Split(_PARAM_DELIMITER)
                                If arr.Count > 0 Then
                                    For Each item In arr
                                        Dim tGuid As Guid = New Guid(item)
                                        If Not tGuid.Equals(Guid.Empty) Then event_ids.Add(tGuid)
                                    Next
                                End If
                            End If
                        End If

                        Dim res As Boolean = False

                        Dim Alternatives As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

                        Dim Objectives As clsHierarchy = Nothing
                        If ControlType = ControlType.ctConsequenceToEvent Then
                            Objectives = PM.Hierarchy(ECHierarchyID.hidImpact)
                        Else
                            Objectives = PM.Hierarchy(ECHierarchyID.hidLikelihood)
                        End If

                        Dim Control = GetControlByID(control_id)

                        For Each event_id In event_ids
                            Dim objIDs As New List(Of Guid)
                            Dim ctrlIDs As New List(Of Guid)
                            Dim eventIDs As New List(Of Guid)

                            Dim tAlt As clsNode = Alternatives.GetNodeByID(event_id)

                            If Objectives IsNot Nothing AndAlso Control IsNot Nothing Then
                                For Each obj As clsNode In If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, Objectives.Nodes, Objectives.TerminalNodes)
                                    If ControlType = ControlType.ctCause OrElse obj.GetContributedAlternatives.Contains(tAlt) Then
                                        Dim tHasAssignment As Boolean = False
                                        For Each appl In Control.Assignments
                                            If ControlType = ControlType.ctCause Then
                                                If appl.ObjectiveID.Equals(obj.NodeGuidID) Then tHasAssignment = True
                                            Else
                                                If appl.ObjectiveID.Equals(obj.NodeGuidID) AndAlso appl.EventID.Equals(event_id) Then
                                                    tHasAssignment = True
                                                End If
                                            End If
                                        Next
                                        If (checked AndAlso Not tHasAssignment) OrElse (Not checked AndAlso tHasAssignment) Then objIDs.Add(obj.NodeGuidID)
                                    End If
                                Next
                            End If

                            eventIDs.Add(event_id)
                            ctrlIDs.Add(control_id)

                            If checked Then
                                If ControlType = ControlType.ctCause Then
                                    AddControlsAssignments(ctrlIDs, objIDs)
                                Else
                                    AddControlsAssignments2(ctrlIDs, objIDs, eventIDs)
                                End If
                            Else
                                Dim applIDs As New List(Of Guid)
                                If Control IsNot Nothing Then
                                    For Each appl In Control.Assignments
                                        If Objectives IsNot Nothing AndAlso Control IsNot Nothing Then
                                            For Each obj As clsNode In If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, Objectives.Nodes, Objectives.TerminalNodes)
                                                If ControlType = ControlType.ctCause OrElse obj.GetContributedAlternatives.Contains(tAlt) Then
                                                    If ControlType = ControlType.ctCause Then
                                                        If appl.ObjectiveID.Equals(obj.NodeGuidID) Then
                                                            applIDs.Add(appl.ID)
                                                        End If
                                                    Else
                                                        If appl.ObjectiveID.Equals(obj.NodeGuidID) AndAlso appl.EventID.Equals(event_id) Then
                                                            applIDs.Add(appl.ID)
                                                        End If
                                                    End If
                                                End If
                                            Next
                                        End If
                                    Next
                                End If
                                DeleteControlsAssignments(ctrlIDs, applIDs)
                            End If
                        Next
                    End If
                    tResult = GetControlsData()
                Case "activate_col"
                    Dim checked As Boolean = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "checked")) = "1"
                    Dim obj_id As Guid = New Guid(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "obj_id")))
                    If IsByTreatmentPage Then
                        Dim sControlID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "dropdown_element_id"))  ' Anti-XSS
                        Dim controlID As Guid = New Guid(sControlID)
                        Dim ctrl As clsControl = PM.Controls.GetControlByID(controlID)
                        Dim ctrlIDs As New List(Of Guid)
                        ctrlIDs.Add(ctrl.ID)

                        Dim Alternatives As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

                        Dim Objectives As clsHierarchy = Nothing
                        If ControlType = ControlType.ctConsequenceToEvent Then
                            Objectives = PM.Hierarchy(ECHierarchyID.hidImpact)
                        Else
                            Objectives = PM.Hierarchy(ECHierarchyID.hidLikelihood)
                        End If
                        Dim tObj As clsNode = Objectives.GetNodeByID(obj_id)

                        For Each tAlt In Alternatives.TerminalNodes.Where(Function (node) node.Enabled).ToList
                            Dim objIDs As New List(Of Guid)
                            Dim eventIDs As New List(Of Guid)

                            objIDs.Add(obj_id)

                            If tObj.GetContributedAlternatives().Contains(tAlt) Then

                                Dim tHasAssignment As Boolean = False
                                For Each appl In ctrl.Assignments
                                    If ControlType = ControlType.ctCause Then
                                        If appl.ObjectiveID.Equals(obj_id) Then
                                            tHasAssignment = True
                                        End If
                                    Else
                                        If appl.ObjectiveID.Equals(obj_id) AndAlso appl.EventID.Equals(tAlt.NodeGuidID) Then
                                            tHasAssignment = True
                                        End If
                                    End If
                                Next

                                eventIDs.Add(tAlt.NodeGuidID)

                                If checked Then
                                    If ControlType = ControlType.ctCause Then
                                        AddControlsAssignments(ctrlIDs, objIDs)
                                    Else
                                        AddControlsAssignments2(ctrlIDs, objIDs, eventIDs)
                                    End If
                                Else
                                    Dim applIDs As New List(Of Guid)
                                    For Each appl In ctrl.Assignments
                                        If ControlType = ControlType.ctCause Then
                                            If appl.ObjectiveID.Equals(obj_id) Then
                                                applIDs.Add(appl.ID)
                                            End If
                                        Else
                                            If appl.ObjectiveID.Equals(obj_id) AndAlso appl.EventID.Equals(tAlt.NodeGuidID) Then
                                                applIDs.Add(appl.ID)
                                            End If
                                        End If
                                    Next
                                    DeleteControlsAssignments(ctrlIDs, applIDs)
                                End If
                            End If
                        Next
                    Else
                        Dim sEventID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "dropdown_element_id"))  ' Anti-XSS
                        Dim event_ids As List(Of Guid) = New List(Of Guid)
                        If Not String.IsNullOrEmpty(sEventID) Then
                            Dim arr = sEventID.Split(_PARAM_DELIMITER)
                            If arr.Count > 0 Then
                                For Each item In arr
                                    event_ids.Add(New Guid(item))
                                Next
                            End If
                        End If

                        If ControlType = ControlType.ctCause Then event_ids.Add(Guid.Empty) 'adding an empty guid so that the loop whoud execute once

                        Dim Objectives As clsHierarchy = Nothing
                        If ControlType = ControlType.ctConsequenceToEvent Then
                            Objectives = PM.Hierarchy(ECHierarchyID.hidImpact)
                        Else
                            Objectives = PM.Hierarchy(ECHierarchyID.hidLikelihood)
                        End If
                        Dim tObj As clsNode = Objectives.GetNodeByID(obj_id)

                        For Each event_id In event_ids
                            Dim objIDs As New List(Of Guid)
                            Dim ctrlIDs As New List(Of Guid)
                            Dim eventIDs As New List(Of Guid)

                            objIDs.Add(obj_id)
                            Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(event_id)

                            If ControlType = ControlType.ctCause OrElse (tObj.GetContributedAlternatives().Contains(tAlt) AndAlso tAlt.Enabled) Then 'A1402

                                For Each ctrl In Treatments.Where(Function(c) c.Type = ControlType)
                                    Dim tHasAssignment As Boolean = False

                                    For Each appl In ctrl.Assignments
                                        If ControlType = ControlType.ctCause Then
                                            If appl.ObjectiveID.Equals(obj_id) Then
                                                tHasAssignment = True
                                            End If
                                        Else
                                            If appl.ObjectiveID.Equals(obj_id) AndAlso appl.EventID.Equals(event_id) Then
                                                tHasAssignment = True
                                            End If
                                        End If
                                    Next

                                    If (checked AndAlso Not tHasAssignment) OrElse (Not checked AndAlso tHasAssignment) Then ctrlIDs.Add(ctrl.ID)
                                Next
                                eventIDs.Add(event_id)

                                If checked Then
                                    If ControlType = ControlType.ctCause Then
                                        AddControlsAssignments(ctrlIDs, objIDs)
                                    Else
                                        AddControlsAssignments2(ctrlIDs, objIDs, eventIDs)
                                    End If
                                Else
                                    Dim applIDs As New List(Of Guid)
                                    For Each ctrl In Treatments.Where(Function(c) c.Type = ControlType)
                                        For Each appl In ctrl.Assignments
                                            If ControlType = ControlType.ctCause Then
                                                If appl.ObjectiveID.Equals(obj_id) Then
                                                    applIDs.Add(appl.ID)
                                                End If
                                            Else
                                                If appl.ObjectiveID.Equals(obj_id) AndAlso appl.EventID.Equals(event_id) Then
                                                    applIDs.Add(appl.ID)
                                                End If
                                            End If
                                        Next
                                    Next
                                    DeleteControlsAssignments(ctrlIDs, applIDs)
                                End If
                            End If
                        Next
                    End If

                    tResult = GetControlsData()
                Case "paste_controls"
                    Dim clipboardData As String = GetParam(args, "data")
                    Dim rows As String() = clipboardData.Split(Chr(10))
                    Dim rows_count As Integer = rows.Count
                    Dim fValueChanged As Boolean = False
                    Dim tControlType As ControlType = ControlType
                    Dim controlsData As String = ""

                    For i As Integer = 0 To rows_count - 1
                        Dim value As String = rows(i).Trim
                        If Not String.IsNullOrEmpty(value) Then
                            Dim cols As String() = value.Split(CChar(vbTab))
                            Dim cols_count As Integer = cols.Count
                            If cols_count > 0 Then
                                Dim tName As String = cols(0).Trim
                                Dim tDescription As String = ""
                                Dim tCost As Double = UNDEFINED_INTEGER_VALUE
                                If Not String.IsNullOrEmpty(tName) AndAlso GetTreatmentByName(tName.Trim, tControlType) Is Nothing Then
                                    If cols_count > 1 Then tDescription = cols(1).Trim
                                    If cols_count > 2 Then If Not String2Double(cols(2).Trim, tCost) Then tCost = UNDEFINED_INTEGER_VALUE

                                    Dim ctrl As clsControl = PM.Controls.AddControl(Guid.NewGuid, tName, tControlType, tDescription)
                                    ctrl.Cost = tCost

                                    ' parse categories
                                    Dim tCategoryGuids As New List(Of String)
                                    If cols_count > 3 Then ' Has Categories
                                        For k As Integer = 3 To cols_count - 1
                                            Dim sCat As String = cols(k)
                                            Dim tCatName As String = sCat.Trim
                                            Dim existingCategory As clsAttributeEnumerationItem = GetCategoryByName(tCatName)
                                            If tCatName <> "" AndAlso existingCategory Is Nothing Then
                                                Dim catID As Guid = Guid.NewGuid()
                                                AddEnumAttributeItem(ATTRIBUTE_CONTROL_CATEGORY_ID, AttributeTypes.atControl, catID, tCatName, False)
                                                tCategoryGuids.Add(catID.ToString)
                                            End If
                                            If existingCategory IsNot Nothing AndAlso Not tCategoryGuids.Contains(existingCategory.ID.ToString) Then
                                                tCategoryGuids.Add(existingCategory.ID.ToString)
                                            End If
                                        Next
                                    End If

                                    For Each sGuid In tCategoryGuids
                                        Dim sCategories As String = ""
                                        If ctrl.Categories IsNot Nothing Then sCategories = CStr(ctrl.Categories)
                                        If Not sCategories.Contains(sGuid) Then
                                            sCategories += CStr(IIf(sCategories = "", "", ";")) + sGuid
                                        End If
                                        If Not String.IsNullOrEmpty(sCategories) Then ctrl.Categories = sCategories
                                    Next

                                    controlsData += CStr(IIf(controlsData.Length = 0, "", ",")) + GetControlData(ctrl, Treatments.IndexOf(ctrl) + 1)
                                    fValueChanged = True
                                End If
                            End If
                        End If
                    Next

                    If fValueChanged Then
                        WriteControls(PRJ)
                        WriteAttributes(PRJ)
                        WriteAttributeValues(PRJ, ParseString(LogMessagePrefix + "Paste %%controls%% from clipboard"), "")
                    End If

                    tResult = String.Format("[[{0}],{1}]", controlsData, GetCategoriesData())
                Case "add_category" 'add a new treatment category
                    tResult = ""
                    Dim tCatName As String = GetParam(args, "name").Trim
                    If tCatName <> "" Then
                        Dim catID As Guid = Guid.NewGuid()
                        AddEnumAttributeItem(ATTRIBUTE_CONTROL_CATEGORY_ID, AttributeTypes.atControl, catID, tCatName, True)
                        tResult = catID.ToString
                    End If
                Case "delete_category"
                    tResult = ""
                    Dim catID As Guid = New Guid(GetParam(args, "cat_id"))
                    RemoveCategoryItem(catID)
                Case "rename_category"
                    tResult = ""
                    Dim catID As Guid = New Guid(GetParam(args, "cat_id"))
                    Dim catName As String = GetParam(args, "cat_name")
                    RenameCategoryItem(catID, catName)
                Case "event_changed"
                    Dim tGuid As Guid = Guid.Empty
                    If Guid.TryParse(GetParam(args, "eid"), tGuid) Then SelectedEventID = tGuid
                    tResult = String.Format("[{0},{1}]", GetObjectivesColumns(), GetControlsData())
                Case "show_descriptions"
                    PM.Parameters.Riskion_Control_ShowInfodocs = Param2Bool(args, "chk")   ' D4387
                    PM.Parameters.Save()
                    tResult = String.Format("[]")
                Case "selected_user_id"
                    PM.Parameters.Riskion_ControlsSelectedUser = Param2Int(args, "value")
                    PM.Parameters.Save()
                    tResult = GetControlsData()
                Case "treatment_changed"
                    Dim tGuid As Guid = Guid.Empty
                    if Guid.TryParse(GetParam(args, "id"), tGuid) Then SelectedTreatmentID = tGuid
                    tResult = String.Format("[{0},{1}]", GetObjectivesColumns(), GetEventsData())
            End Select
        End With

        If sAction <> "" Then App.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False ' D4134

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

#Region "Operations with Treatments"

    Public Sub AddControlAssignment(ControlID As Guid, Object1ID As Guid)
        Dim controlAssignment As clsControlAssignment = PM.Controls.AddControlAssignment(ControlID, Object1ID)
        WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignment added"))
    End Sub

    Public Sub DeleteControlAssignment(ControlID As Guid, Object1ID As Guid)
        Dim res As Boolean = False
        Dim AssignmentID As Guid = Guid.Empty
        Dim ctrl As clsControl = PM.Controls.GetControlByID(ControlID)
        If ctrl IsNot Nothing Then
            For Each appl As clsControlAssignment In ctrl.Assignments
                If appl.ObjectiveID.Equals(Object1ID) Then AssignmentID = appl.ID
            Next
        End If

        If Not AssignmentID.Equals(Guid.Empty) Then
            If PM.Controls.DeleteControlAssignment(ControlID, AssignmentID) Then
                res = True
            End If
        End If
        If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignment deleted"))
    End Sub

    Public Sub DeleteControlAssignment2(ControlID As Guid, Object1ID As Guid, EventID As Guid)
        Dim res As Boolean = False
        Dim AssignmentID As Guid = Guid.Empty
        Dim ctrl As clsControl = PM.Controls.GetControlByID(ControlID)
        If ctrl IsNot Nothing Then
            For Each appl As clsControlAssignment In ctrl.Assignments
                If appl.ObjectiveID.Equals(Object1ID) AndAlso appl.EventID.Equals(EventID) Then AssignmentID = appl.ID
            Next
        End If

        If Not AssignmentID.Equals(Guid.Empty) Then
            If PM.Controls.DeleteControlAssignment(ControlID, AssignmentID) Then
                res = True
            End If
        End If
        If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assigment deleted"))
    End Sub

    Public Sub DeleteControlsAssignments(ControlIDs As List(Of Guid), AssignmentIDs As List(Of Guid))
        Dim res As Boolean = False
        For Each ControlID In ControlIDs
            For Each AssignmentID In AssignmentIDs
                If PM.Controls.DeleteControlAssignment(ControlID, AssignmentID) Then
                    res = True
                End If
            Next
        Next
        If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments deleted"))
    End Sub

    Public Sub DeleteControlsAssignments2(ControlIDs As List(Of Guid), ObjIDs As List(Of Guid), EventIDs As List(Of Guid))
        Dim res As Boolean = False
        For Each ControlID In ControlIDs
            Dim ctrl As clsControl = PM.Controls.GetControlByID(ControlID)
            If ctrl IsNot Nothing Then
                For Each ObjID In ObjIDs
                    For Each EventID In EventIDs
                        Dim AssignmentID As Guid = Guid.Empty

                        For Each appl As clsControlAssignment In ctrl.Assignments
                            If appl.ObjectiveID.Equals(ObjID) AndAlso appl.EventID.Equals(EventID) Then AssignmentID = appl.ID
                        Next

                        If Not AssignmentID.Equals(Guid.Empty) Then
                            If PM.Controls.DeleteControlAssignment(ControlID, AssignmentID) Then
                                res = True
                            End If
                        End If
                    Next
                Next
            End If
        Next
        If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments deleted"))
    End Sub

    Public Sub AddControlsAssignments(ControlIDs As List(Of Guid), Object1IDs As List(Of Guid))
        For Each ControlID In ControlIDs
            Dim tControl = PM.Controls.GetControlByID(ControlID)
            For Each Object1ID In Object1IDs
                PM.Controls.AddControlAssignment(ControlID, Object1ID)
            Next
        Next
        WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments added"))
    End Sub

    Public Sub AddControlsAssignments2(ControlIDs As List(Of Guid), Object1IDs As List(Of Guid), Object2IDs As List(Of Guid))
        Dim Objectives As clsHierarchy = Nothing
        If ControlType = ControlType.ctConsequenceToEvent Then
            Objectives = PM.Hierarchy(ECHierarchyID.hidImpact)
        Else
            Objectives = PM.Hierarchy(ECHierarchyID.hidLikelihood)
        End If

        For Each ControlID In ControlIDs
            Dim tControl = PM.Controls.GetControlByID(ControlID)
            For Each Object1ID In Object1IDs
                Dim node As clsNode = Objectives.GetNodeByID(Object1ID)
                Dim contAlts As List(Of clsNode) = node.GetContributedAlternatives
                Dim contAltsIDs As List(Of Guid) = New List(Of Guid)
                If contAlts IsNot Nothing Then
                    For Each alt As clsNode In contAlts
                        contAltsIDs.Add(alt.NodeGuidID)
                    Next
                End If
                If node.ParentNode Is Nothing Then 'no specific source
                    contAltsIDs.Clear()
                    For Each obj2 In Object2IDs
                        contAltsIDs.Add(obj2)
                    Next
                End If
                For Each Object2ID As Guid In Object2IDs
                    For Each altID As Guid In contAltsIDs
                        If altID.Equals(Object2ID) Then
                            Dim controlAssignment As clsControlAssignment = PM.Controls.AddControlAssignment(ControlID, Object1ID, Object2ID)
                        End If
                    Next
                Next
            Next
        Next
        WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments added"))
    End Sub

    Public Sub SetControlMeasurementScale(ControlID As Guid, ControlAssignmentID As Guid, MeasurementType As ECMeasureType, MeasurementScaleID As Guid)
        Dim control As clsControl = PM.Controls.GetControlByID(ControlID)
        If control IsNot Nothing Then
            Dim controlAssignment As clsControlAssignment = control.GetAssignmentByID(ControlAssignmentID)
            If controlAssignment IsNot Nothing Then
                controlAssignment.MeasurementType = MeasurementType
                controlAssignment.MeasurementScaleGuid = MeasurementScaleID
                WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% measurement scale set"))
            End If
        End If
    End Sub

#End Region

#Region "Categories/Enum"

    Public Sub AddEnumAttributeItem(AttributeID As Guid, AttributeType As AttributeTypes, ItemID As Guid, ItemName As String, Optional saveAttributes As Boolean = True)
        Dim attr As clsAttribute = PM.Attributes.GetAttributeByID(AttributeID)
        If attr IsNot Nothing AndAlso (attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti) Then
            With PM
                Dim aEnum As clsAttributeEnumeration = .Attributes.GetEnumByID(attr.EnumID)
                If aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty) Then
                    aEnum = New clsAttributeEnumeration
                    aEnum.ID = ATTRIBUTE_CONTROL_CATEGORY_ENUM_ID
                    aEnum.Name = attr.Name
                    aEnum.Items = New List(Of clsAttributeEnumerationItem)
                    attr.EnumID = aEnum.ID
                    .Attributes.Enumerations.Add(aEnum)
                End If

                Dim eItem As clsAttributeEnumerationItem = aEnum.AddItem(ItemName)
                eItem.ID = ItemID

                If saveAttributes Then
                    .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                End If
            End With
        End If
    End Sub

    Public Sub RemoveCategoryItem(ItemID As Guid)
        With PM
            Dim aEnum As clsAttributeEnumeration = .Attributes.GetEnumByID(ATTRIBUTE_CONTROL_CATEGORY_ENUM_ID)
            If aEnum IsNot Nothing Then
                aEnum.DeleteItem(ItemID)
            End If

            .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
        End With
    End Sub

    Public Sub RenameCategoryItem(ItemID As Guid, ItemName As String)
        With PM
            Dim tSuccess As Boolean = False
            Dim aEnum As clsAttributeEnumeration = .Attributes.GetEnumByID(ATTRIBUTE_CONTROL_CATEGORY_ENUM_ID)
            If aEnum IsNot Nothing Then
                ItemName = ItemName.Trim
                Dim item As clsAttributeEnumerationItem = aEnum.GetItemByID(ItemID)
                If item IsNot Nothing AndAlso ItemName <> "" Then
                    item.Value = ItemName
                    tSuccess = True
                End If
            End If

            If tSuccess Then .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
        End With
    End Sub

#End Region

End Class