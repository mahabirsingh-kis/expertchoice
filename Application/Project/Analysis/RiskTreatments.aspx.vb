Imports System.Collections.ObjectModel
Imports System.Xml
Imports Chilkat
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security

Partial Class RiskSelectTreatmentsPage
    Inherits clsComparionCorePage

    Public Const _PRIORITY_FORMAT As String = "f4"
    Public Const _PARAM_MODE As String = "mode"
    Public Const _MODE_OPTIMIZE As String = "solve"
    Public Const _OPT_ALLOW_CHOSE_SOLVER As Boolean = False     ' D4075
    Public Const _OPT_ALLOW_SA_REDUCTIONS As Boolean = True
    Public Const UNDEFINED_ATTRIBUTE_VALUE As String = "- undefined -"
    Public Const SESS_RA_PAGES_CURRENT As String = "RA_PagesPage_{0}"   ' D4120
    Public Const OPT_RA_PAGES_MODE As String = "RAPagesMode"        ' D4120

    ' D4371 ===
    Dim sTagsExtra As String() = {"objective", "potential-assessments", "statement"}
    Dim sTagsTitle As String() = {"ASSESSMENT OBJECTIVE", "POTENTIAL ASSESSMENT METHODS AND OBJECT", "POTENTIAL ASSESSMENT METHODS AND OBJECT"}
    Dim sTagsCommon As String() = {"number", "title", "family", "priority", "baseline-impact", "control-enhancements"} ' A1364
    ' D4371 ==

    Public ReadOnly Property PagePrefix As String
        Get
            Return String.Format("{0}: ", Me.Title)
        End Get
    End Property

    Public Enum OptimizerErrors
        oeNone = 0
        oeNoControls = 1
        oeInfeasible = 2
    End Enum

    Private OptimizerError As OptimizerErrors = OptimizerErrors.oeNone

    Public IsEditable As Boolean = False

    Public Sub New()
        MyBase.New(_PGID_RISK_SELECT_TREATMENTS)
    End Sub

    Public ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAlignerRisk
        End Get
    End Property

    Public ReadOnly Property IsOptimizationAllowed() As Boolean
        Get
            Return Not IsEditable 'True 'CurrentPageID = _PGID_RISK_OPTIMIZER
        End Get
    End Property

    'ReadOnly Property SESS_SHOW_OPTIONS As String
    '    Get
    '        Return String.Format("RISK_SHOW_OPTIONS_{0}", App.ProjectID.ToString)
    '    End Get
    'End Property

    'Public Property ShowOptions As Boolean
    '    Get
    '        Dim tSessVar = Session(SESS_SHOW_OPTIONS)
    '        If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Boolean Then
    '            Return CBool(tSessVar)
    '        End If
    '        Return True
    '    End Get
    '    Set(value As Boolean)
    '        Session(SESS_SHOW_OPTIONS) = value
    '    End Set
    'End Property

    Public Function CanUserAddControls() As Boolean
        Return CurrentPageID = _PGID_RISK_TREATMENTS_DICTIONARY AndAlso Not App.IsActiveProjectReadOnly
    End Function

    Public ReadOnly Property CanUserSelectControlsManually() As Boolean
        Get
            Return CurrentPageID <> _PGID_RISK_OPTIMIZER_OVERALL And CurrentPageID <> _PGID_RISK_OPTIMIZER_FROM_SOURCES And CurrentPageID <> _PGID_RISK_OPTIMIZER_TO_OBJS
        End Get
    End Property

    Private ReadOnly Property SESS_TREATMENTS As String
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
            If Not IsEditable Then
                _Treatments = value.Where(Function(ctrl) ctrl.Type <> ControlType.ctUndefined).ToList()
            Else
                _Treatments = value
            End If
            Session(SESS_TREATMENTS) = value
        End Set
    End Property

    Private ReadOnly Property SESS_LOAD_ALL_CONTROLS As String
        Get
            Return String.Format("RISK_LOAD_ALL_CONTROLS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property LoadAllControls As Boolean
        Get
            Dim tSessVar = Session(SESS_LOAD_ALL_CONTROLS)
            If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Boolean Then
                Return CBool(tSessVar)
            End If
            Return True
        End Get
        Set(value As Boolean)
            Session(SESS_LOAD_ALL_CONTROLS) = value
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

    Private Property new_category_name As String
        Get
            Return SessVar("tmpRiskCategoryName")
        End Get
        Set(value As String)
            SessVar("tmpRiskCategoryName") = value
        End Set
    End Property

    Dim _showCents As Boolean? = Nothing
    Public Property ShowCents As Boolean
        Get
            If Not _showCents.HasValue Then
                Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_CENTS_ID, UNDEFINED_USER_ID))
            Else
                Return _showCents.Value
            End If
        End Get
        Set(value As Boolean)
            _showCents = value
            WriteSetting(PRJ, ATTRIBUTE_RISK_SHOW_CENTS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public ReadOnly Property CostDecimalDigits As Integer
        Get
            If ShowCents Then Return 2
            Return 0
        End Get
    End Property

    Public ReadOnly Property StringSelectedEventIDs As String
        Get
            Dim retVal As String = ""
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                If alt.Enabled Then
                    retVal += If(retVal = "", "", ",") + alt.NodeGuidID.ToString
                End If
            Next
            Return retVal
        End Get
    End Property

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

    Public ReadOnly Property StringSelectedControlIDs As String
        Get
            Dim retVal As String = ""
            For Each ctrl As clsControl In PM.Controls.EnabledControls
                retVal += If(retVal = "", "", ",") + ctrl.ID.ToString
            Next
            Return retVal
        End Get
    End Property

    Public ReadOnly Property SelectedControlIDs As List(Of Guid)
        Get
            Dim IDs As New List(Of Guid)
            For Each ctrl As clsControl In PM.Controls.EnabledControls
                IDs.Add(ctrl.ID)
            Next
            Return IDs
        End Get
    End Property

    Public Property OptimizationTypeAttribute As RiskOptimizationType
        Get
            If Not PM.Parameters.RiskionShowRiskReductionOptions Then Return RiskOptimizationType.BudgetLimit
            Return CType(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_TYPE_ID, UNDEFINED_USER_ID), RiskOptimizationType)
        End Get
        Set(value As RiskOptimizationType)
            RA.RiskOptimizer.CurrentScenario.OptimizationType = value
            WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_TYPE_ID, AttributeValueTypes.avtLong, CInt(value)) ', ParseString("%%Controls%% optimization - optimization type changed (" + [Enum].GetName(GetType(RiskOptimizationType), value) + ")"))
        End Set
    End Property

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

    'Public Property ControlsSelectedManually As Boolean
    '    Get
    '        Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_CONTROLS_MANUAL_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Boolean)
    '        WriteSetting(PRJ, ATTRIBUTE_RISK_CONTROLS_MANUAL_ID, AttributeValueTypes.avtBoolean, value, "", "")
    '    End Set
    'End Property

    'Public Property BudgetLimitAttribute As Double
    '    Get
    '        Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_BUDGET_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Double)
    '        RA.RiskOptimizer.CurrentScenario.Budget = value
    '        WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_BUDGET_ID, AttributeValueTypes.avtDouble, value) ', ParseString("%%Controls%% optimization - budget limit changed (" + CostString(value) + ")"))
    '    End Set
    'End Property

    'Public Property MaxRiskAttribute As Double
    '    Get
    '        Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_MAX_RISK_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Double)
    '        RA.RiskOptimizer.CurrentScenario.MaxRisk = value
    '        WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_MAX_RISK_ID, AttributeValueTypes.avtDouble, value) ', ParseString("%%Controls%% optimization - max %%risk%% changed (" + value.ToString + ")"))
    '    End Set
    'End Property

    'Public Property MinReductionAttribute As Double
    '    Get
    '        Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_MIN_REDUCTION_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Double)
    '        RA.RiskOptimizer.CurrentScenario.MinReduction = value
    '        WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_MIN_REDUCTION_ID, AttributeValueTypes.avtDouble, value) ', ParseString("%%Controls%% optimization - %%risk%% reduction changed (" + value.ToString + ")"))
    '    End Set
    'End Property

'#Region "Loss Exceedance Curve properties"

'    Public Property RandSeed As Integer
'        Get
'            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_RAND_SEED_ID, UNDEFINED_USER_ID))
'        End Get
'        Set(value As Integer)
'            WriteSetting(PRJ, ATTRIBUTE_LEC_RAND_SEED_ID, AttributeValueTypes.avtLong, value)
'        End Set
'    End Property

'    Public Property KeepRandSeed As Boolean
'        Get
'            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_KEEP_RAND_SEED_ID, UNDEFINED_USER_ID))
'        End Get
'        Set(value As Boolean)
'            WriteSetting(PRJ, ATTRIBUTE_LEC_KEEP_RAND_SEED_ID, AttributeValueTypes.avtBoolean, value)
'        End Set
'    End Property

'#End Region

    Public Function GetAllObjsData() As String
        Dim retVal As String = ""
        For Each Node As clsNode In PM.Hierarchy(ECHierarchyID.hidImpact).Nodes
            Dim sDollValue As String = ""
            If Node.DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso Node.DollarValue <> 0 Then sDollValue = String.Format(" ({0})", CostString(Node.DollarValue, CostDecimalDigits, True))

            Dim margin As String = New String(CChar(" "), Node.Level)
            margin = If(margin = "", "", margin.Replace(" ", "&nbsp;") + "�&nbsp;")
            retVal += If(retVal = "", "", ",") + String.Format("['{0}', '{1}{2}{3}', {4}]", Node.NodeGuidID, margin, ShortString(JS_SafeString(Node.NodeName), 50), SafeFormString(sDollValue), JS_SafeNumber(Node.DollarValue))
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Private Function AddTreeNodeData(tNode As clsNode) As String
        Dim sCaption = tNode.NodeName

        Dim sClassName As String = "{ 'color':'black', 'font-weight':'normal' }" '"ztree_tnn"
        If tNode.RiskNodeType = RiskNodeType.ntCategory Then
            sClassName = "{ 'color':'blue', 'font-weight':'bold' }" '"ztree_cat"
        End If

        Dim sName As String = "<span class='" + sClassName + " text-overflow'>" + SafeFormString(sCaption) + "</span>"
        Dim sID As String = tNode.NodeGuidID.ToString
        Dim sNode As String = "{name:""" + JS_SafeString(tNode.NodeName) + """,title:""" + JS_SafeString(tNode.NodeName) + """,font:" + sClassName + ",open:true,nocheck:false,checked:" + Bool2JS(tNode.Enabled) + ",nodeid:" + tNode.NodeID.ToString + ",id:""" + sID + """,chkDisabled:" + Bool2JS(tNode.ParentNodes Is Nothing OrElse tNode.ParentNodes.Count = 0) + ",children:["

        If tNode.Children IsNot Nothing AndAlso tNode.Children.Count > 0 Then
            Dim sChildren As String = ""
            GetTreeNodeData(tNode.Children, sChildren)
            sNode += sChildren
        End If
        sNode += "]}"

        Return sNode
    End Function

    Private Sub GetTreeNodeData(Nodes As List(Of clsNode), ByRef retVal As String)
        If Nodes IsNot Nothing Then
            For Each tNode As clsNode In Nodes
                retVal += If(retVal = "", "", ",") + AddTreeNodeData(tNode)
            Next
        End If
    End Sub

    Public Function GetObjsData(HID As ECHierarchyID) As String
        Dim retVal As String = ""

        GetTreeNodeData(PM.Hierarchy(HID).GetLevelNodes(0), retVal)

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetEventsData() As String
        Dim retVal As String = ""

        If IsOptimizationAllowed Then
            Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            If AltH IsNot Nothing Then
                For Each alt As clsNode In AltH.TerminalNodes
                    Dim sDollValueString As String = ""
                    'If alt.DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso alt.DollarValue <> 0 Then sDollValueString = String.Format(" ({0})", CostString(alt.DollarValue, CostDecimalDigits, True))

                    Dim fChk As Integer = If(alt.Enabled, 1, 0)
                    retVal += If(retVal <> "", ",", "") + String.Format("['{0}','{1}{2}',{3},{4}]", alt.NodeGuidID, JS_SafeString(alt.NodeName), JS_SafeString(sDollValueString), fChk, JS_SafeNumber(alt.DollarValue))
                Next
            End If
        End If

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
                            retVal += If(retVal <> "", ",", "") + String.Format("['{0}','{1}']", item.ID, JS_SafeString(item.Value))
                        Next
                    End If
                End If
            End If
        End With

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetCategoryByName(CategoryName As String, Optional CategoricalAttribute As clsAttribute = Nothing) As clsAttributeEnumerationItem
        Dim retVal As clsAttributeEnumerationItem = Nothing

        With PM
            If .Attributes IsNot Nothing Then
                If CategoricalAttribute Is Nothing Then CategoricalAttribute = .Attributes.GetAttributeByID(ATTRIBUTE_CONTROL_CATEGORY_ID)
                If CategoricalAttribute IsNot Nothing AndAlso Not CategoricalAttribute.EnumID.Equals(Guid.Empty) Then
                    Dim cat_attr_vals As clsAttributeEnumeration = .Attributes.GetEnumByID(CategoricalAttribute.EnumID)
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

    Public Function GetControlsColumns() As String
        Dim retVal As String = String.Format("[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 0, ResString("optIndexID"), "true", "right", "", "true", "id", -1, 50)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 1, "Active", "true", "center", "", "false", "active", -1, 80)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 2, "Actions", "true", "left", "html", "true", "actions", -1, 50)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 2, ParseString("%%Control%% Name"), "true", "left", "html", "true", "name", -1, 300)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 3, ParseString("%%Control%% for"), "true", "left", "", "true", "type", -1, 170)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 4, "Selected", "true", "center", "", "true", "funded", -1, 80)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 4, "Disabled", "true", "center", "", "true", "dis", -1, 80)
        'Dim btnCopyPaste As String = String.Format("<img src=\'{0}menu_dots.png\' width=\'12\' height=\'12\' style=\'cursor:context-menu; padding-left:3px;\' alt=\'\' onclick=\'showColumnMenu(event, this, {1}, {2});\' oncontextmenu=\'showColumnMenu(event, this, {1}, {2}); return false;\'/>", ImagePath, "true", If(App.IsActiveProjectReadOnly, "false", "true"))       
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 5, ResString("tblCost"), "true", "right", "", "true", "cost", -1, 100)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 6, ParseString("Applications"), "true", "center", "", "true", "app_cnt", -1, 80)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 7, ResString("lblRACategories"), "true", "center", "", "true", "cat", -1, 80)
        'retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5}]", 8, ParseString("S.A. Reduction"), "true", "td_right" + If(IsEditable, " datatables_hide_column",""), "", "true")
        If _OPT_ALLOW_SA_REDUCTIONS Then
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 8, "S.A. Reduction, %", "false", "right", "", "true", "sa", -1, 80)
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 8, "S.A. Reduction, " + System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, "false", "right", "", "true", "sa_doll", -1, 80)
        End If
        'If IsOptimizationAllowed AndAlso Not CanUserSelectControlsManually Then
        'If IsOptimizationAllowed Then
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 9, ResString("tblMust"), "true", "center", "", "true", "must", -1, 50)
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}']", 10, ResString("tblMustNot"), "true", "center", "", "true", "mustnot", -1, 50)
        'End If

        Dim index As Integer = 12
        Dim attrIdx As Integer = 0
        For Each attr As clsAttribute In AttributesList
            Dim btnCopyPasteAttr As String = ""
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},'{6}',{7},'{8}',{9}]", index, JS_SafeString(App.GetAttributeName(attr)), "true", "left", "html", "true", "v" + attrIdx.ToString, attrIdx, 90, CInt(attr.ValueType))
            index += 1
            attrIdx += 1
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function ControlTypeToString(ctrlType As ControlType) As String
        Dim retVal As String = ""
        Select Case ctrlType
            Case ControlType.ctUndefined
                retVal = ParseString("Undefined")
            Case ControlType.ctCause
                retVal = ParseString("%%Likelihood%% Of %%Objectives(l)%%")
            Case ControlType.ctCauseToEvent
                retVal = ParseString("%%Likelihood%% Of %%Alternatives%%")
            Case ControlType.ctConsequence ' OBSOLETE
                retVal = ParseString("Consequence")
            Case ControlType.ctConsequenceToEvent
                retVal = ParseString("Consequences Of %%Alternatives%% To %%Objectives(i)%%")
        End Select
        Return retVal
    End Function

    Public Function GetControlData(ctrl As clsControl) As String
        Dim appl_count As Integer
        GetApplicationsData(ctrl.ID, appl_count)

        Dim attrCategory As clsAttribute = PM.Attributes.GetAttributeByID(ATTRIBUTE_CONTROL_CATEGORY_ID)
        Dim tCategories As clsAttributeEnumeration = Nothing
        If attrCategory IsNot Nothing AndAlso Not attrCategory.EnumID.Equals(Guid.Empty) Then
            tCategories = PM.Attributes.GetEnumByID(attrCategory.EnumID)
        End If

        Dim sCategroies As String = ""
        If tCategories IsNot Nothing AndAlso TypeOf ctrl.Categories Is String Then
            Dim sItems As String = CStr(ctrl.Categories)
            If Not String.IsNullOrEmpty(sItems) Then
                For Each item In tCategories.Items
                    If sItems.Contains(item.ID.ToString) Then
                        sCategroies += If(sCategroies = "", "", ", ") + item.Value
                    End If
                Next
            End If
        End If

        Dim sAttributes As String = ""
        Dim attrIdx As Integer = 0
        Dim sAttrVals As String = ""
        For Each attr As clsAttribute In AttributesList
            Dim clip_data As String = ""    'clipboard data
            Dim attrValue As Object = PM.Attributes.GetAttributeValue(attr.ID, ctrl.ID, attr)
            Dim sValue As String = ""

            Select Case attr.ValueType
                Case AttributeValueTypes.avtString '0
                    If attrValue Is Nothing OrElse Not TypeOf attrValue Is String Then attrValue = ""
                    clip_data = CStr(attrValue)
                    sValue = String.Format("'{0}'", JS_SafeString(CStr(attrValue)))
                Case AttributeValueTypes.avtBoolean '1
                    If attrValue Is Nothing OrElse Not TypeOf attrValue Is Boolean Then attrValue = False
                    clip_data = If(CBool(attrValue), ResString("lblYes"), ResString("lblNo"))
                    sValue = If(CBool(attrValue), "1", "0")
                Case AttributeValueTypes.avtLong '2
                    If attrValue Is Nothing OrElse Not (TypeOf attrValue Is Long OrElse TypeOf attrValue Is Integer) Then attrValue = ""
                    clip_data = CStr(attrValue).Replace("&#160;", " ")
                    sValue = ""
                    If CStr(attrValue) <> "" Then sValue = JS_SafeNumber(CStr(attrValue))
                Case AttributeValueTypes.avtDouble '3
                    If attrValue Is Nothing OrElse Not (TypeOf attrValue Is Double OrElse TypeOf attrValue Is Integer OrElse TypeOf attrValue Is Long OrElse TypeOf attrValue Is Single) Then attrValue = ""
                    clip_data = CStr(attrValue).Replace("&#160;", " ")
                    sValue = ""
                    If CStr(attrValue) <> "" Then sValue = JS_SafeNumber(CStr(attrValue))
                Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti  '4, 5
                    sValue = "'" + JS_SafeString(PM.Attributes.GetAttributeValueString(attr.ID, ctrl.ID)) + "'"
                    sAttrVals += String.Format(",'vguid{0}':'{1}'", attrIdx, If(sValue = "" OrElse attrValue Is Nothing, "", attrValue.ToString))
            End Select
            sAttributes += If(sAttributes = "", "", ", ") + "v" + attrIdx.ToString + ":" + If(sValue <> "", sValue, "''") + sAttrVals
            attrIdx += 1
        Next

        'Dim fChk As Integer = If(ctrl.Enabled, 1, 0)
        If sAttributes <> "" Then sAttributes = "," + sAttributes

        'If _OPT_ALLOW_SA_REDUCTIONS AndAlso CurrentPageID <> _PGID_RISK_TREATMENTS_DICTIONARY AndAlso PM.Parameters.Riskion_ShowControlsRiskReduction AndAlso ctrl.SARiskReduction = UNDEFINED_INTEGER_VALUE Then
        '    ' calculate individual risk reductions
        '    ctrl.SARiskReduction = RA.Solver.GetControlRiskReductions(ctrl.ID, SelectedEventIDs)
        '    ctrl.SARiskReductionMonetary = If(ctrl.SARiskReduction = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, ctrl.SARiskReduction * PM.DollarValueOfEnterprise)
        'End If

        Dim sControl As RAAlternative = Scenario.GetAvailableAlternativeById(ctrl.ID.ToString)

        Return String.Format("{{""id"":{0},""guid"":""{18}"",""active"":{1},""name"":""{2}"",""type"":{3},""funded"":{4},""cost"":{5},""app_cnt"":{6},""cat"":""{7}"",""risk"":{8},""must"":{9},""mustnot"":{10},""guid"":""{11}"",""infodoc"":""{12}"",""cat_ids"":""{13}"",""dis"":{14},""sa_red"":{15},""sa_red_doll"":""{16}""{17}}}", If(sControl IsNot Nothing, sControl.SortOrder, Treatments.IndexOf(ctrl) + 1), Bool2JS(ctrl.Enabled AndAlso ctrl.Active), JS_SafeString(ctrl.Name), CInt(ctrl.Type), Bool2JS(ctrl.Enabled AndAlso ctrl.Active), JS_SafeNumber(If(ctrl.IsCostDefined, ctrl.Cost, 0)), appl_count, JS_SafeString(sCategroies), JS_SafeNumber(ctrl.SARiskReduction), Bool2JS(ctrl.Must), Bool2JS(ctrl.MustNot), ctrl.ID.ToString, JS_SafeString(GetControlInfodoc(PRJ, ctrl, True)), If(ctrl.Categories Is Nothing, "", JS_SafeString(ctrl.Categories)), Bool2JS(Not ctrl.Enabled), JS_SafeNumber(ctrl.SARiskReduction), JS_SafeNumber(ctrl.SARiskReductionMonetary), sAttributes, ctrl.ID.ToString) ' D4345
    End Function

    Public Function GetControlsData() As String
        Dim retVal As String = ""

        ControlsListReset()
        Treatments = ControlsList

        For Each ctrl As clsControl In Treatments
            retVal += If(retVal = "", "", ",") + GetControlData(ctrl)
        Next
        Return String.Format("[{0}]", retVal)
    End Function

#Region "Control Attributes"

    Private tAttributesList As List(Of clsAttribute) = Nothing
    Private ReadOnly Property AttributesList As List(Of clsAttribute)
        Get
            If tAttributesList Is Nothing Then
                tAttributesList = PM.Attributes.AttributesList.Where(Function(a) a.Type = AttributeTypes.atControl AndAlso Not a.IsDefault AndAlso Not a.ID.Equals(ATTRIBUTE_CONTROL_COST_ID)).ToList
            End If
            Return tAttributesList
        End Get
    End Property

    Public Function GetAttributeItemsData(attr As clsAttribute, items As clsAttributeEnumeration) As String
        Dim sItems As String = ""
        If items IsNot Nothing Then

            Dim defaultItemsGuids As New List(Of Guid)
            If attr.DefaultValue IsNot Nothing Then
                If attr.ValueType = AttributeValueTypes.avtEnumeration Then
                    If TypeOf attr.DefaultValue Is Guid Then defaultItemsGuids.Add(CType(attr.DefaultValue, Guid))
                    If TypeOf attr.DefaultValue Is String Then defaultItemsGuids.Add(New Guid(CStr(attr.DefaultValue)))
                End If
                If attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                    If TypeOf attr.DefaultValue Is Guid Then defaultItemsGuids.Add(CType(attr.DefaultValue, Guid))
                    If TypeOf attr.DefaultValue Is String Then
                        Dim sGuids As String() = CStr(attr.DefaultValue).Split(CChar(";"))
                        For Each sGuid As String In sGuids
                            defaultItemsGuids.Add(New Guid(sGuid))
                        Next
                    End If
                End If
            End If

            For Each item As clsAttributeEnumerationItem In items.Items
                Dim isDefault As Boolean = defaultItemsGuids.Contains(item.ID)
                sItems += If(sItems = "", "", ",") + String.Format("['{0}','{1}',{2}]", item.ID, JS_SafeString(item.Value), If(isDefault, "1", "0"))
            Next
        End If
        Return sItems
    End Function

    Public Function GetAttributesData() As String
        Dim sAttrs As String = ""
        Dim attrIndex As Integer = 0
        For Each attr As clsAttribute In AttributesList
            Dim sValue As String = "''" ' enum attribute items or string/int/double/bool default value
            If attr.DefaultValue IsNot Nothing Then sValue = attr.DefaultValue.ToString()

            Select Case attr.ValueType
                Case AttributeValueTypes.avtString '0
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("'{0}'", JS_SafeString(attr.DefaultValue))
                Case AttributeValueTypes.avtBoolean '1
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", If(CBool(attr.DefaultValue), 1, 0))
                Case AttributeValueTypes.avtLong '2
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", JS_SafeNumber(attr.DefaultValue))
                Case AttributeValueTypes.avtDouble '3
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", JS_SafeNumber(attr.DefaultValue))
                Case AttributeValueTypes.avtEnumeration '4
                    sValue = String.Format("[{0}]", GetAttributeItemsData(attr, PM.Attributes.GetEnumByID(attr.EnumID)))
                Case AttributeValueTypes.avtEnumerationMulti '5
                    sValue = String.Format("[{0}]", GetAttributeItemsData(attr, PM.Attributes.GetEnumByID(attr.EnumID)))
            End Select

            sAttrs += If(sAttrs = "", "", ",") + String.Format("[{0},'{1}',{2},{3},{4}]", attrIndex, JS_SafeString(App.GetAttributeName(attr)), sValue, CInt(attr.ValueType), Bool2JS(Not attr.ID.Equals(ATTRIBUTE_CONTROL_CATEGORY_ID))) ' Index, Name, Default Value (or available enum values), ValueType, Can attribute be removed
            attrIndex += 1
        Next
        Return String.Format("[{0}]", sAttrs)
    End Function

#End Region

    Public Function getInitialOptimizedRisk() As String
        Dim tShowDollarValue As Boolean = ShowDollarValue
        With RA.RiskOptimizer.CurrentScenario
            .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
            ''A1470 ===
            'If Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput Then
            '    '.OriginalRiskValue = LEC.GetSimulatedRisk(ControlsUsageMode.DoNotUse, PRJ.isOpportunityModel)
            '    '.OriginalRiskValueWithControls = LEC.GetSimulatedRisk(ControlsUsageMode.UseAll, PRJ.isOpportunityModel)
            '    .OptimizedRiskValue = LEC.GetSimulatedRisk(ControlsUsageMode.UseOnlyActive, PRJ.isOpportunityModel)
            'End If
        End With
        'A1470 ==
        Dim sRiskValue As String = ""
        If tShowDollarValue Then
            sRiskValue = CostString(RA.RiskOptimizer.CurrentScenario.OptimizedRiskValue * PM.DollarValueOfEnterprise, CostDecimalDigits, True)
            sRiskValue += DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * PM.DollarValueOfEnterprise, RA.RiskOptimizer.CurrentScenario.OptimizedRiskValue * PM.DollarValueOfEnterprise, CostDecimalDigits, tShowDollarValue)
        Else
            sRiskValue = (RA.RiskOptimizer.CurrentScenario.OptimizedRiskValue * 100).ToString("F2") + "%"
            sRiskValue += DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue, RA.RiskOptimizer.CurrentScenario.OptimizedRiskValue, CostDecimalDigits, tShowDollarValue)
        End If
        Return sRiskValue
    End Function

    Public Property ShowDollarValue As Boolean
        Get
            Dim UseDollarValue As Boolean = CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, UNDEFINED_USER_ID)) AndAlso PM.CalculationsManager.DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso PM.CalculationsManager.DollarValueTarget <> UNDEFINED_STRING_VALUE
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, AttributeValueTypes.avtBoolean, value, "", "")
        End Set
    End Property

    ' dollar value of DollarValueTarget (which can be an impact objective or event)
    Public Property DollarValue As Double
        Get
            Return PM.CalculationsManager.DollarValue
        End Get
        Set(value As Double)
            WriteSetting(PRJ, ATTRIBUTE_DOLLAR_VALUE_ID, AttributeValueTypes.avtDouble, value, ParseString(PagePrefix + ": Total Cost Changed to " + CostString(value)), "")
        End Set
    End Property

    Public Property DollarValueTarget As String
        Get
            Return PM.CalculationsManager.DollarValueTarget
        End Get
        Set(value As String)
            WriteSetting(PRJ, ATTRIBUTE_DOLLAR_VALUE_TARGET_ID, AttributeValueTypes.avtString, value, ParseString(PagePrefix + ": Cost Target Changed"), "")
        End Set
    End Property

    Public Function GetTargetName() As String
        Dim retVal As String = ResString("lblEnterprise")
        If DollarValueTarget <> "" AndAlso DollarValueTarget <> UNDEFINED_STRING_VALUE Then
            Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(New Guid(DollarValueTarget))
            If tAlt IsNot Nothing Then retVal = tAlt.NodeName
            Dim tNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(New Guid(DollarValueTarget))
            If tNode IsNot Nothing Then retVal = tNode.NodeName
            Return "&quot;" + ShortString(retVal, 40) + "&quot;"
        End If
        Return retVal
    End Function

    Public Function GetDollarValueFullString() As String
        Dim tImpactGoalNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0)
        Dim retVal As String = ""
        retVal = String.Format("{0}&nbsp;{1}:&nbsp;{2}", ResString("lblValueOf"), ResString("lblEnterprise"), CostString(PM.DollarValueOfEnterprise, CostDecimalDigits, True))
        If DollarValueTarget <> "" AndAlso DollarValueTarget <> UNDEFINED_STRING_VALUE AndAlso Not tImpactGoalNode.NodeGuidID.Equals(New Guid(DollarValueTarget)) Then
            Dim tNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(New Guid(DollarValueTarget))
            Dim sDollvalue As String = ""
            If DollarValue <> UNDEFINED_INTEGER_VALUE Then sDollvalue = CostString(DollarValue, CostDecimalDigits, True)
            retVal += If(retVal = "", "", ",") + String.Format("{0}&nbsp;{1}:&nbsp;{2}", ResString("lblValueOf"), GetTargetName(), sDollvalue)
        End If
        Return If(DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso retVal <> "", " (" + retVal + ")", "")
    End Function

    Public Function GetScenariosData() As String
        Dim sRes As String = ""
        For Each tID As Integer In RA.Scenarios.Scenarios.Keys
            sRes += If(sRes = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4}]", tID, JS_SafeString(RA.Scenarios.Scenarios(tID).Name), JS_SafeString(RA.Scenarios.Scenarios(tID).Description), RA.Scenarios.Scenarios(tID).Index, RA.Scenarios.Scenarios(tID).CombinedGroupUserID)   ' D3350 + D3737
        Next
        Return String.Format("[{0}]", sRes)
    End Function

    Public Function GetTitle() As String
        Dim retVal As String = ParseString("Select %%Controls%%")
        If CurrentPageID = _PGID_RISK_OPTIMIZER_OVERALL Or CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES Or CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS Then
            retVal = String.Format(ParseString("%%Controls%% optimization <span id='divNodeName'>{1}</span> for &quot;{0}&quot;"), ShortString(SafeFormString(App.ActiveProject.ProjectName), 85), ShortString(SafeFormString(WRTNodeName), 85))
        End If
        If CurrentPageID = _PGID_RISK_TREATMENTS_DICTIONARY Then
            retVal = String.Format(ParseString("%%Controls%% for &quot;{0}&quot;"), ShortString(SafeFormString(App.ActiveProject.ProjectName), 85))
        End If
        Return retVal
    End Function

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim sMode As String = CheckVar(_PARAM_MODE, "").ToLower
        IsEditable = sMode = "edit"
        If sMode.ToLower = _MODE_OPTIMIZE Then
            CurrentPageID = _PGID_RISK_OPTIMIZER_OVERALL
            'PagePrefix = "Optimize %%Controls%%"
            Dim sPgid As String = CheckVar("pgid", "")
            Select Case sPgid
                Case "77202"
                    CurrentPageID = _PGID_RISK_OPTIMIZER_OVERALL
                Case "77203"
                    CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES
                Case "77204"
                    CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS
                Case "77210"
                    CurrentPageID = _PGID_RISK_OPTIMIZER_TIME_PERIODS_VIEW
            End Select
        End If
        If IsEditable Then
            CurrentPageID = _PGID_RISK_TREATMENTS_DICTIONARY
        End If
        'Treatments = GetTreatments().OrderBy(Function(ctrl) ctrl.Type).ToList
        ControlsListReset()
        Treatments = ControlsList   ' D4187
        If Not _OPT_ALLOW_CHOSE_SOLVER AndAlso RA.RiskOptimizer.SolverLibrary <> raSolverLibrary.raBaron Then RA.RiskOptimizer.SolverLibrary = raSolverLibrary.raBaron ' D4075

        ' D4459 === // Moved from PreInit
        If Not isCallback AndAlso Not IsPostBack Then
            '-A1432 ControlsListReset()
            '-A1432 Treatments = ControlsList
            RA.RiskOptimizer.isOpportunityModel = PRJ.isOpportunityModel

            With RA.RiskOptimizer.CurrentScenario
                .OptimizationType = OptimizationTypeAttribute
                '.Budget = BudgetLimitAttribute
                '.MaxRisk = MaxRiskAttribute
                '.MinReduction = MinReductionAttribute
                If IsOptimizationAllowed Then 'CurrentPageID <> _PGID_RISK_TREATMENTS_DICTIONARY Then
                    'Dim tSelectedEventIDs As List(Of Guid) = SelectedEventIDs
                    .OriginalRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.DoNotUse)
                    .OriginalRiskValueWithControls = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseAll)
                    'A1470 ===
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    'If Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput Then
                    '    .OriginalRiskValue = LEC.GetSimulatedRisk(ControlsUsageMode.DoNotUse, PRJ.isOpportunityModel)
                    '    .OriginalRiskValueWithControls = LEC.GetSimulatedRisk(ControlsUsageMode.UseAll, PRJ.isOpportunityModel)
                    '    .OptimizedRiskValue = LEC.GetSimulatedRisk(ControlsUsageMode.UseOnlyActive, PRJ.isOpportunityModel)
                    'End If
                    'A1470 ==
                End If

                .OriginalAllControlsCost = 0
                .OriginalAllControlsWithAssignmentsCost = 0
                .OriginalSelectedControlsCost = 0
                .OriginalSelectedControlsCount = 0
                For Each ctrl As clsControl In Treatments
                    If ctrl.Enabled AndAlso ctrl.IsCostDefined Then
                        .OriginalAllControlsCost += ctrl.Cost
                        If RA.RiskOptimizer.ControlHasApplications(ctrl, SelectedHierarchyNode, SelectedEventIDs) Then
                            .OriginalAllControlsWithAssignmentsCost += ctrl.Cost
                        End If
                    End If
                    If ctrl.Enabled AndAlso ctrl.Active Then
                        If ctrl.IsCostDefined Then
                            .OriginalSelectedControlsCost += ctrl.Cost
                        End If
                        .OriginalSelectedControlsCount += 1
                    End If
                    ctrl.SARiskReduction = UNDEFINED_INTEGER_VALUE
                    ctrl.SARiskReductionMonetary = UNDEFINED_INTEGER_VALUE
                Next

            End With

            PM.Parameters.Riskion_ShowControlsRiskReduction = False
        End If
        ' D4459 ==

    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Dim sAction = CheckVar(_PARAM_ACTION, "").Trim.ToLower
        If CheckVar(_PARAM_ACTION, "") = "download" Then
            If CheckVar("type", "bar").Trim.ToLower = "bar" Then
                DownloadBaronFiles()
            End If
        End If
        If CheckVar(_PARAM_ACTION, "") = "scenario" Then
            Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
            If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                RA.Scenarios.ActiveScenarioID = ID
                RA.Solver.ResetSolver()
            End If
            If RA_ShowTimeperiods AndAlso RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count = 0 Then RA_ShowTimeperiods = False
            Dim sUrl As String = PageURL(CurrentPageID, GetTempThemeURI(False))
            Response.Redirect(sUrl, True)
        End If
        If CheckVar(_PARAM_ACTION, "") = "show_timeperiods" Then
            RA_ShowTimeperiods = CheckVar("value", RA_ShowTimeperiods)
            'Dim sUrl As String = PageURL(CurrentPageID, GetTempThemeURI(False))
            'Response.Redirect(sUrl, True)
        End If
        If Not isCallback AndAlso Not IsPostBack Then FileUpload.Attributes.Add("onchange", "if (this.value!='') { theForm.btnUpload.disabled=0; theForm.btnUpload.focus(); }") ' D4369
        ' D4370 ===
        If FileUpload.HasFile Then
            Dim fUploaded As Boolean = False
            Dim sUploadedFileName As String = File_CreateTempName()
            FileUpload.SaveAs(sUploadedFileName)
            Dim sData As String = File_GetContent(sUploadedFileName, "")
            If sData.TrimStart().StartsWith("<?xml") Then
                CreateControlsFromXML(sData)
                fUploaded = True
            End If
            File_Erase(sUploadedFileName)
            If fUploaded Then
                Response.Redirect(RemoveXssFromUrl(Request.Url.OriginalString), True)   ' D6767
            Else
                ClientScript.RegisterStartupScript(GetType(String), "msg", "setTimeout('msgWrongFile();', 1000);", True)
            End If
        End If
        ' D4370 ==

        Ajax_Callback(Request.Form.ToString)
    End Sub

    Public Function GetApplicationsData(ByVal ID As Guid, ByRef AvailableApplicationsCount As Integer) As String
        Dim appData As String = "[]"

        AvailableApplicationsCount = 0

        Dim ctrl As clsControl = GetControlByID(ID)
        If ctrl IsNot Nothing Then
            Dim Alternatives As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            Dim Sources As clsHierarchy = PM.Hierarchy(ECHierarchyID.hidLikelihood)
            Dim Objectives As clsHierarchy = PM.Hierarchy(ECHierarchyID.hidImpact)

            appData = CInt(ctrl.Type).ToString + ", '" + JS_SafeString(ctrl.Name) + "'" 'Applications data: [Type, Name, [[event, source, objective, measure type, effectiveness],[...]]]

            Dim apps As String = ""
            If ctrl IsNot Nothing Then
                For Each item In ctrl.Assignments
                    Dim MT As String = ""
                    Select Case item.MeasurementType
                        Case ECMeasureType.mtDirect
                            MT = "Direct"
                        Case ECMeasureType.mtRatings
                            MT = "Ratings"
                        Case ECMeasureType.mtStep
                            MT = "Step Function"
                        Case ECMeasureType.mtRegularUtilityCurve
                            MT = "Utility Curve"
                        Case Else
                            MT = item.MeasurementType.ToString
                    End Select

                    If PM.Controls.IsValidControlAssignment(ctrl, item) Then
                        Dim Alt As String = ""
                        Dim AltID As String = Guid.Empty.ToString
                        Dim tAlt As clsNode = Alternatives.GetNodeByID(item.EventID)
                        If tAlt IsNot Nothing Then
                            Alt = tAlt.NodeName
                            AltID = tAlt.NodeGuidID.ToString
                        End If

                        Dim nodeID As String = ""
                        Dim Source As String = ""
                        If (ctrl.Type = ControlType.ctCause OrElse ctrl.Type = ControlType.ctCauseToEvent) AndAlso Not item.ObjectiveID.Equals(Guid.Empty) Then
                            Dim tSource As clsNode = Sources.GetNodeByID(item.ObjectiveID)
                            If tSource IsNot Nothing Then 'AndAlso (tSource.IsTerminalNode OrElse tSource Is Sources.Nodes(0)) Then
                                Source = tSource.NodeName
                                If Not tSource.IsTerminalNode AndAlso tSource Is Sources.Nodes(0) Then
                                    Source = ResString("lblNoSpecificCause")
                                End If
                                nodeID = tSource.NodeGuidID.ToString
                            End If
                        End If

                        Dim Obj As String = ""
                        If ctrl.Type = ControlType.ctConsequenceToEvent AndAlso Not item.ObjectiveID.Equals(Guid.Empty) Then
                            Dim tObj As clsNode = Objectives.GetNodeByID(item.ObjectiveID)
                            If tObj IsNot Nothing AndAlso tObj.IsTerminalNode Then
                                Obj = tObj.NodeName
                                nodeID = tObj.NodeGuidID.ToString
                            End If
                        End If

                        AvailableApplicationsCount += 1
                        Dim sOnClick As String = String.Format("evaluateControlEffectiveness(""{0}"", ""{1}"", ""{2}"", ""{3}""); return false;", ctrl.ID.ToString, AltID, nodeID, item.ID.ToString)
                        apps += If(apps = "", "", ",") + String.Format("['{0}','{1}','{2}','{3}','{4}','{5}']", JS_SafeString(Alt), JS_SafeString(Source), JS_SafeString(Obj), MT, JS_SafeNumber(item.Value.ToString(_PRIORITY_FORMAT)), JS_SafeString(sOnClick))
                    End If
                Next
            End If
            appData = "[" + appData + ",[" + apps + "]]"
        End If

        Return appData
    End Function

    Private Function GetControlByID(ID As Guid) As clsControl
        For Each ctrl In Treatments
            If ctrl.ID.Equals(ID) Then Return ctrl
        Next
        Return Nothing
    End Function

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = GetParam(args, "action").ToLower
        Dim tResult As String = If(String.IsNullOrEmpty(sAction), "", sAction)
        Dim sResult As String = "" ' for Time Periods functions
        Dim sComment As String = ""
        Dim fDoSolve As Boolean = False

        Select Case sAction
            Case "refresh", "refresh_full" ' reload controls in order to update controls descriptions
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetControlsData(), GetAttributesData(), GetControlsColumns())
            Case "activate_all"
                Dim sType As String = CStr(GetParam(args, "type").ToLower)
                Dim sCtrlType As String = ParseString("%%Controls%%")
                Dim sChk As String = GetParam(args, "chk").ToLower
                Dim chk As Boolean = Str2Bool(sChk)
                Dim res As Boolean = False

                'ControlsSelectedManually = True

                If sType = "all" Then
                    res = PM.Controls.SetControlsActive(ControlType.ctCause, chk) And PM.Controls.SetControlsActive(ControlType.ctCauseToEvent, chk) And PM.Controls.SetControlsActive(ControlType.ctConsequenceToEvent, chk)
                Else
                    Dim ctrlType As ControlType = CType(CInt(sType), ControlType)
                    sCtrlType = ParseString("%%Controls%% for %%Sources%%")
                    If ctrlType = ControlType.ctCauseToEvent Then sCtrlType = ParseString("%%Controls%% for %%Vulnerabilities%%")
                    If ctrlType = ControlType.ctConsequenceToEvent Then sCtrlType = ParseString("%%Controls%% for Consequences")
                    res = PM.Controls.SetControlsActive(ctrlType, chk)
                End If

                If res Then
                    'RA.RiskOptimizer.CurrentScenario.Budget = 0
                    'RA.Save()
                    WriteControls(PRJ, ParseString(PagePrefix + ": All " + sCtrlType + If(chk, " activated", " deactivated")))

                    If PM.Parameters.Riskion_ControlsActualSelectionMode <> 0 Then
                        PM.Parameters.Riskion_ControlsActualSelectionMode = 0
                        PM.Parameters.Save()
                    End If
                End If

                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)

                    tResult = String.Format("['{0}', {1}, '{2}',{3},{4}]", sAction, GetControlsData(), getInitialOptimizedRisk(), sCostData, sRiskData)
                End With

            Case "editcontrol"
                Dim control_id As Guid = New Guid(GetParam(args, "control_id"))
                Dim name As String = GetParam(args, "name").Trim
                'Dim infodoc As String = GetParam(args, "descr").Trim
                Dim categories As String = GetParam(args, "cat").Trim
                Dim cost As Double = 0
                String2Double(GetParam(args, "cost").Trim(), cost)
                Dim control As clsControl = PM.Controls.GetControlByID(control_id)
                If control IsNot Nothing Then
                    control.Name = name
                    'control.InfoDoc = infodoc
                    control.Cost = cost
                    control.Categories = categories
                    'SetControlInfodoc(PRJ, control, infodoc, String.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Host), ParseString(PagePrefix + ": %%Control%% edited"))    ' D4345
                    ' comment due to save on SetControlInfodoc()
                    WriteControls(PRJ)
                    WriteAttributeValues(PRJ, ParseString(PagePrefix + ": %%Control%% edited"), control.Name)    ' D3731 + A1388
                End If
                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)

                    tResult = String.Format("['{0}', {1}, '{2}',{3},{4}]", sAction, GetControlsData(), getInitialOptimizedRisk(), sCostData, sRiskData)
                End With
            Case "deletecontrol"
                Dim control_id As Guid = New Guid(GetParam(args, "control_id"))
                Dim res As Boolean = PM.Controls.DeleteControl(control_id)
                ControlsListReset() ' D4245
                Treatments = ControlsList   ' D4245
                If res Then WriteControls(PRJ, ParseString(PagePrefix + ": %%Control%% deleted"))

                'ControlsSelectedManually = True

                If PM.Parameters.Riskion_ControlsActualSelectionMode <> 0 Then
                    PM.Parameters.Riskion_ControlsActualSelectionMode = 0
                    PM.Parameters.Save()
                End If

                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)

                    tResult = String.Format("['{0}', {1}, '{2}',{3},{4}]", sAction, GetControlsData(), getInitialOptimizedRisk(), sCostData, sRiskData)
                End With
            Case "remove_controls" ' remove multiple controls on control dictionary page
                Dim res As Boolean = False

                Dim sIds As String = GetParam(args, "ids").Trim
                Dim s As String() = sIds.Split(CChar(","))
                If s.Length > 0 Then
                    For Each sId In s
                        If sId.Length > 32 Then
                            if PM.Controls.DeleteControl(New Guid(sId)) Then res = True
                        End If
                    Next
                End If

                ControlsListReset() ' D4245
                Treatments = ControlsList   ' D4245

                If res Then WriteControls(PRJ, ParseString(PagePrefix + ": %%Control%%(s) removed"), ParseString(PagePrefix + ": %%Control%%(s) removed"))

                'ControlsSelectedManually = True

                If PM.Parameters.Riskion_ControlsActualSelectionMode <> 0 Then
                    PM.Parameters.Riskion_ControlsActualSelectionMode = 0
                    PM.Parameters.Save()
                End If

                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()

                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)

                    tResult = String.Format("['{0}', {1}, '{2}',{3},{4}]", sAction, GetControlsData(), getInitialOptimizedRisk(), sCostData, sRiskData)
                End With
            Case "add_category" 'add a new treatment category
                new_category_name = ""
                Dim catID As Guid = Guid.NewGuid()
                Dim tCatName As Object = GetParam(args, "name")
                If tCatName IsNot Nothing Then
                    Dim catName As String = CStr(tCatName).Trim
                    If catName <> "" Then
                        AddEnumAttributeItem(ATTRIBUTE_CONTROL_CATEGORY_ID, AttributeTypes.atControl, catID, catName)
                        new_category_name = catID.ToString
                    End If
                End If
                tResult = String.Format("['{0}', {1}]", sAction, GetCategoriesData())
            Case "delete_category"
                new_category_name = ""
                Dim catID As Guid = New Guid(GetParam(args, "cat_id"))
                RemoveCategoryItem(catID)
                tResult = String.Format("['{0}', {1}, {2}]", sAction, GetCategoriesData(), GetControlsData())
            Case "rename_category"
                new_category_name = ""
                Dim catID As Guid = New Guid(GetParam(args, "cat_id"))
                Dim catName As String = GetParam(args, "cat_name")
                RenameCategoryItem(catID, catName)
                tResult = String.Format("['{0}', {1}]", sAction, GetCategoriesData())
            Case "solver"
                Dim solverID As raSolverLibrary = CType(CInt(GetParam(args, "id").ToLower), raSolverLibrary)
                RA.RiskOptimizer.SolverLibrary = solverID
                'TODO: save solver library
                tResult = String.Format("['ok']")
            Case "solve"
                Dim tShowDollarValue As Boolean = ShowDollarValue
                Dim sIDs As String = ""
                Dim sPeriodResults As String = ""

                If IsOptimizationAllowed Then
                    Dim tDblValue As Double = 0
                    If Not PM.Parameters.RiskionShowRiskReductionOptions Then RA.RiskOptimizer.CurrentScenario.OptimizationType = RiskOptimizationType.BudgetLimit
                    Select Case RA.RiskOptimizer.CurrentScenario.OptimizationType
                        Case RiskOptimizationType.BudgetLimit
                            Dim sBudget As String = GetParam(args, "value")
                            If Not String.IsNullOrEmpty(sBudget) AndAlso String2Double(sBudget, tDblValue) AndAlso RA.RiskOptimizer.BudgetLimit <> tDblValue Then
                                RA.RiskOptimizer.CurrentScenario.Budget = tDblValue
                                RA.Save()
                            End If
                        Case RiskOptimizationType.MaxRisk
                            Dim sMaxRisk As String = GetParam(args, "risk")
                            If String2Double(sMaxRisk, tDblValue) Then
                                If ShowDollarValue AndAlso PM.DollarValueOfEnterprise <> 0 Then
                                    tDblValue = Math.Round(tDblValue / PM.DollarValueOfEnterprise, 5)
                                Else
                                    tDblValue /= 100
                                End If
                                If RA.RiskOptimizer.CurrentScenario.MaxRisk <> tDblValue Then
                                    RA.RiskOptimizer.CurrentScenario.MaxRisk = tDblValue
                                    RA.Save()
                                End If
                            End If
                        Case RiskOptimizationType.MinReduction
                            Dim sMinReduction As String = GetParam(args, "reduction")
                            If String2Double(sMinReduction, tDblValue) Then
                                If ShowDollarValue AndAlso PM.DollarValueOfEnterprise <> 0 Then
                                    tDblValue = Math.Round(tDblValue / PM.DollarValueOfEnterprise, 5)
                                Else
                                    tDblValue /= 100
                                End If
                                If RA.RiskOptimizer.CurrentScenario.MinReduction <> tDblValue Then
                                    RA.RiskOptimizer.CurrentScenario.MinReduction = tDblValue
                                    RA.Save()
                                End If
                            End If
                    End Select

                    'ActivateAllControls() 'A1232
                    'ControlsSelectedManually = False

                    Dim tFundedCost As Double = 0
                    Dim tOptimizedRisk As Double = 0
                    Dim retVal As List(Of Guid) = OptimizeTreatments(tFundedCost, tOptimizedRisk)
                    For Each Id As Guid In retVal
                        sIDs += If(sIDs = "", "", ",") + String.Format("'{0}'", Id.ToString)
                    Next
                    WriteControls(PRJ, ParseString(PagePrefix + ": %%Control%%(s) activated by optimizer"))

                    'If RA.Solver.SolverState = raSolverState.raSolved Then
                    Dim ATPResults = Scenario.TimePeriods.TimePeriodResults
                    For Each item In ATPResults
                        sPeriodResults += CStr(IIf(sPeriodResults <> "", ",", "")) + String.Format("{{projID:'{0}', start:{1}}}", JS_SafeString(item.Key), item.Value)
                    Next
                    'End If
                    sPeriodResults = String.Format("[{0}]", sPeriodResults)
                End If

                ' A1232 ==
                Dim sCostData As String = GetCostParamsString()
                'Calculate the model in order to get the Risk Dollar Value
                Dim sRiskData As String = ""
                Dim sBudgetData As String = GetBudgetParamsString(RA.RiskOptimizer.BudgetLimit, RA.RiskOptimizer.CurrentScenario.MaxRisk, RA.RiskOptimizer.CurrentScenario.MinReduction)

                If IsOptimizationAllowed Then
                    With RA.RiskOptimizer.CurrentScenario
                        Dim tSelectedEventIDs As List(Of Guid) = SelectedEventIDs
                        .OriginalRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.DoNotUse)
                        .OriginalRiskValueWithControls = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseAll)
                        .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                        .OriginalRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.DoNotUse)
                        sRiskData = GetRiskParamsString(tShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)
                    End With
                End If
                ' D4078 ===
                Dim sError As String = GetMessageByBaronError()
                Dim sOptimizerError As String = ""

                Dim tMode As Integer = PM.Parameters.Riskion_ControlsActualSelectionMode
                Select Case OptimizerError
                    Case OptimizerErrors.oeNone
                        tMode = 1 ' Optimized
                    Case OptimizerErrors.oeNoControls
                        tMode = 0 ' Manually selected
                        sOptimizerError = ResString("msgBaronNoControls")
                    Case OptimizerErrors.oeInfeasible
                        tMode = 0 ' Manually selected
                        sOptimizerError = ResString("errRAInfeasible")
                End Select

                If (CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES OrElse CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS) AndAlso Not RA.RiskOptimizer.HasControlsForOptimization(WRTNode, SelectedEventIDs) Then
                    OptimizerError = OptimizerErrors.oeNoControls
                    sOptimizerError = ParseString("No %%controls%% available for specified " + If(WRTNode.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood, "%%objectives(l)%%", "%%objectives(i)%%"))
                End If

                If tMode <> PM.Parameters.Riskion_ControlsActualSelectionMode Then
                    PM.Parameters.Riskion_ControlsActualSelectionMode = tMode
                    PM.Parameters.Save()
                End If

                tResult = String.Format("['{0}', [{1}], '{2}', {3}, {4}, {5}, '{6}', '{7}',{8}]", sAction, sIDs, JS_SafeString(CostString(RA.RiskOptimizer.BudgetLimit)), sCostData, sRiskData, sBudgetData, JS_SafeString(sError), JS_SafeString(sOptimizerError), sPeriodResults) 'A1368
                ' D4078 ==
            Case "select_events", "select_controls", "select_sources", "select_impacts"
                Dim sIDs As String = GetParam(args, "ids")
                Dim sResultData As String = ""
                If sAction = "select_events" Then
                    For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                        alt.Enabled = sIDs.Contains(alt.NodeGuidID.ToString)
                    Next
                    PRJ.SaveStructure(ParseString("Save active %%alternatives%%"))
                    sResultData = GetEventsData()
                End If
                If sAction = "select_sources" Then
                    For Each node As clsNode In PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes
                        node.Enabled = sIDs.Contains(node.NodeGuidID.ToString)
                    Next
                    PRJ.SaveStructure(ParseString("Save active %%sources%%"))
                    sResultData = GetObjsData(ECHierarchyID.hidLikelihood)
                End If
                If sAction = "select_impacts" Then
                    For Each node As clsNode In PM.Hierarchy(ECHierarchyID.hidImpact).Nodes
                        node.Enabled = sIDs.Contains(node.NodeGuidID.ToString)
                    Next
                    PRJ.SaveStructure(ParseString("Save active consequences"))
                    sResultData = GetObjsData(ECHierarchyID.hidImpact)
                End If
                If sAction = "select_controls" Then
                    For Each ctrl As clsControl In PM.Controls.DefinedControls
                        ctrl.Enabled = sIDs.Contains(ctrl.ID.ToString)
                    Next
                    'WriteControls(PRJ, ParseString(PagePrefix + ": %%Control%%(s) enabled"), ParseString(PagePrefix + ": %%Control%%(s) enabled"))
                    'WriteAttributeValues(PRJ, ParseString(PagePrefix + ": %%Control%%(s) enabled"), ParseString(PagePrefix + ": %%Control%%(s) enabled")) 'TODO: 
                    PRJ.SaveStructure(ParseString("Save active %controls%%"))
                    sResultData = GetControlsData()
                End If
                With RA.RiskOptimizer.CurrentScenario
                    Dim tSelectedEventIDs As List(Of Guid) = SelectedEventIDs
                    .OriginalRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.DoNotUse)
                    .OriginalRiskValueWithControls = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseAll)

                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)

                    tResult = String.Format("['{0}',{1},{2},{3},{4}]", sAction, sResultData, 0, sCostData, sRiskData)
                End With
            Case "activate"
                Dim chk As Boolean = Param2Bool(args, "chk")
                Dim tIDs As String() = Param2Array(args, "ids")
                Dim res As Boolean = False

                'ControlsSelectedManually = True

                If tIDs.Length > 0 Then
                    For Each sID As String In tIDs
                        If Not String.IsNullOrEmpty(sID) Then
                            Dim ctrl As clsControl = GetControlByID(New Guid(sID))
                            If ctrl IsNot Nothing Then
                                If PM.Controls.SetControlActive(ctrl.ID, chk) Then
                                    res = True
                                End If
                            End If
                        End If
                    Next
                End If

                If res Then
                    'RA.RiskOptimizer.CurrentScenario.Budget = 0
                    'RA.Save()
                    WriteControls(PRJ, ParseString(PagePrefix + ": %%Control%%(s)" + If(chk, " activated", " deactivated"))) ' D4181

                    If PM.Parameters.Riskion_ControlsActualSelectionMode <> 0 Then
                        PM.Parameters.Riskion_ControlsActualSelectionMode = 0
                        PM.Parameters.Save()
                    End If
                End If

                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)
                    tResult = String.Format("['{0}','{1}','{2}', {3}, {4}]", sAction, "", getInitialOptimizedRisk(), sCostData, sRiskData)
                End With
            Case "edit_budget"
                Dim sValue As String = GetParam(args, "value")
                Dim dValue As Double = 0
                Dim res As Boolean = False
                If String.IsNullOrEmpty(sValue) AndAlso RA.RiskOptimizer.BudgetLimit <> 0 Then
                    RA.RiskOptimizer.CurrentScenario.Budget = 0
                    res = True
                Else
                    If String2Double(sValue, dValue) AndAlso RA.RiskOptimizer.BudgetLimit <> dValue Then
                        RA.RiskOptimizer.CurrentScenario.Budget = dValue
                        res = True
                    End If
                End If
                If res Then
                    RA.Save()
                End If
                tResult = String.Format("['{0}']", sAction)
            Case "edit_cost"
                Dim ctrlID As Guid = New Guid(GetParam(args, "ctrl_id"))
                Dim sIDs As String = GetParam(args, "ids") ' if multiple controls marked then this parameter will not be empty
                Dim sValue As String = GetParam(args, "value")
                Dim dValue As Double = 0
                Dim res As Boolean = False

                Dim tControls As New List(Of clsControl)
                If sIDs <> "" Then
                    Dim s As String() = sIDs.Split(CChar(","))
                    If s.Length > 0 Then
                        For Each sId In s
                            If sId.Length > 32 Then
                                tControls.Add(GetControlByID(New Guid(sId)))
                            End If
                        Next
                    End If
                Else
                    tControls.Add(GetControlByID(ctrlID))
                End If

                If tControls.Count > 0 AndAlso (String.IsNullOrEmpty(sValue) OrElse String2Double(sValue, dValue)) Then
                    For Each tControl As clsControl In tControls
                        If String.IsNullOrEmpty(sValue) Then
                            res = PM.Controls.SetControlCost(tControl.ID, UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE)
                        Else
                            res = PM.Controls.SetControlCost(tControl.ID, dValue)
                        End If
                    Next

                    If res Then
                        RA.Save()
                        Dim sMsg As String = ParseString(PagePrefix + ": Set %%Control%% cost = " + dValue.ToString)
                        WriteAttributeValues(PRJ, sMsg, sMsg)
                    End If
                End If

                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)

                    tResult = String.Format("['{0}', '{1}', '{2}', {3}, {4},{5}]", sAction, "", "", sCostData, sRiskData, GetControlsData())
                End With
            Case "showapplications"
                tResult = String.Format("['error']")
                Dim ClickedControlID As Guid = Guid.Empty
                Dim ID As Guid = New Guid(GetParam(args, "id"))
                Dim ctrl As clsControl = GetControlByID(ID)
                If ctrl IsNot Nothing Then
                    ClickedControlID = ID
                    Dim appCount As Integer
                    tResult = String.Format("['{0}', {1}]", sAction, GetApplicationsData(ClickedControlID, appCount))
                End If
            Case "set_must", "set_must_not", "set_disabled"
                Dim sIDs As String = GetParam(args, "ids")
                Dim Chk As Boolean = Param2Bool(args, "value")
                Dim IDs As List(Of Guid) = Param2GuidList(sIDs)
                Dim fSaveControls As Boolean = False
                Dim sActionName As String = "Must"
                For Each ID As Guid In IDs
                    Dim ctrl As clsControl = GetControlByID(ID)
                    If ctrl IsNot Nothing Then
                        If sAction = "set_must" Then
                            ctrl.Must = Chk
                            If Chk AndAlso ctrl.MustNot Then ctrl.MustNot = Not Chk
                        End If
                        If sAction = "set_must_not" Then
                            sActionName = "Must Not"
                            ctrl.MustNot = Chk
                            If Chk AndAlso ctrl.Must Then ctrl.Must = Not Chk
                        End If
                        If sAction = "set_disabled" Then
                            sActionName = "Disabled"
                            ctrl.Enabled = Not Chk
                        End If
                        fSaveControls = True
                    End If
                Next
                If fSaveControls Then
                    WriteControls(PRJ, ParseString(PagePrefix + ": Set %%Control%% " + sActionName + " (" + Bool2JS(Chk) + ")"))
                    RA.Save()
                End If
                Dim bHasMusts As Boolean = Treatments.Where(Function(a) a.Must).Count > 0
                Dim bHasMustNots As Boolean = Treatments.Where(Function(a) a.MustNot).Count > 0
                tResult = String.Format("['{0}', '{1}', {2}, {3}]", sAction, "", Bool2JS(bHasMusts), Bool2JS(bHasMustNots))
            Case "optimizer_type"
                OptimizationTypeAttribute = CType(GetParam(args, "value"), RiskOptimizationType)
                tResult = String.Format("['{0}']", sAction)
            Case "show_dollar_value"
                ShowDollarValue = Param2Bool(args, "chk")
                With RA.RiskOptimizer.CurrentScenario
                    Dim sRiskData As String = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)
                    Dim sBudgetData As String = GetBudgetParamsString(RA.RiskOptimizer.BudgetLimit, RA.RiskOptimizer.CurrentScenario.MaxRisk, RA.RiskOptimizer.CurrentScenario.MinReduction)
                    tResult = String.Format("['{0}',{1},{2}]", sAction, sBudgetData, sRiskData)
                End With
            Case "selected_dollar_value_target"
                Dim sTarget As String = GetParam(args, "value")
                If DollarValueTarget <> sTarget Then DollarValueTarget = sTarget
                Dim sValue As String = GetParam(args, "doll_value")
                Dim tValue As Double
                If Not String.IsNullOrEmpty(sValue) AndAlso String2Double(sValue, tValue) AndAlso DollarValue <> tValue Then
                    DollarValue = tValue
                End If
                With RA.RiskOptimizer.CurrentScenario
                    RA.Solver.InitOriginalValues()
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)
                    Dim sBudgetData As String = GetBudgetParamsString(RA.RiskOptimizer.BudgetLimit, RA.RiskOptimizer.CurrentScenario.MaxRisk, RA.RiskOptimizer.CurrentScenario.MinReduction)
                    tResult = String.Format("['{0}', '{1}', '{2}', {3}, {4}, {5}, '{6}', {7}, {8}]", sAction, "", "", sCostData, sRiskData, sBudgetData, JS_SafeString(GetDollarValueFullString), GetAllObjsData(), GetEventsData())
                End With
            Case "set_dollar_value"
                Dim sValue As String = GetParam(args, "value")
                Dim sTarget As String = GetParam(args, "target")
                Dim dValue As Double
                If Not String.IsNullOrEmpty(sValue) AndAlso String2Double(sValue, dValue) Then
                    DollarValue = dValue
                End If
                If DollarValueTarget <> sTarget Then DollarValueTarget = sTarget
                With RA.RiskOptimizer.CurrentScenario
                    RA.Solver.InitOriginalValues()
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)
                    Dim sBudgetData As String = GetBudgetParamsString(RA.RiskOptimizer.BudgetLimit, RA.RiskOptimizer.CurrentScenario.MaxRisk, RA.RiskOptimizer.CurrentScenario.MinReduction)
                    tResult = String.Format("['{0}', '{1}', '{2}', {3}, {4}, {5}, '{6}', {7}, {8}]", sAction, "", "", sCostData, sRiskData, sBudgetData, JS_SafeString(GetDollarValueFullString), GetAllObjsData(), GetEventsData())
                End With
            Case "set_option"
                Dim sParam As String = GetParam(args, "param")
                Dim Chk As Boolean = Param2Bool(args, "value")
                Select Case sParam
                    Case "musts"
                        RA.RiskOptimizer.Settings.Musts = Not Chk
                    Case "mustnots"
                        RA.RiskOptimizer.Settings.MustNots = Not Chk
                    Case "dependencies"
                        RA.RiskOptimizer.Settings.Dependencies = Not Chk
                    Case "groups"
                        RA.RiskOptimizer.Settings.Groups = Not Chk
                    Case "constraints"
                        RA.RiskOptimizer.Settings.CustomConstraints = Not Chk
                    Case "fundingpools"
                        RA.RiskOptimizer.Settings.FundingPools = Not Chk
                    Case "timeperiods"
                        RA.RiskOptimizer.Settings.TimePeriods = Not Chk
                End Select
                RA.Save()
                tResult = String.Format("['{0}', '{1}']", sAction, "")
            Case "show_risk_reduction_options"
                PM.Parameters.RiskionShowRiskReductionOptions = Param2Bool(args, "value")
                PM.Parameters.Save()
                tResult = String.Format("['{0}',{1}]", sAction, Bool2JS(PM.Parameters.RiskionShowRiskReductionOptions))
            Case "show_individual_risk"
                PM.Parameters.Riskion_ShowControlsRiskReduction = Param2Bool(args, "value")
                PM.Parameters.Save()
                'tResult = String.Format("['{0}', {1}]", sAction, GetControlsData())
                tResult = String.Format("['{0}']", sAction)
            Case "show_descriptions"
                PM.Parameters.Riskion_Control_ShowInfodocs = Param2Bool(args, "chk")    ' D4387
                PM.Parameters.Save()
                tResult = String.Format("['{0}']", sAction)
            Case "pagination"
                PM.Parameters.Riskion_Control_Pagination = Param2Int(args, "value")
                PM.Parameters.Save()
                tResult = String.Format("['{0}', {1}]", sAction, PM.Parameters.Riskion_Control_Pagination)
            Case "use_simulated_values"
                PM.Parameters.Riskion_Use_Simulated_Values = Param2Int(args, "value")
                PM.Parameters.Save()
                Dim bRecalc As Boolean = Param2Bool(args, "recalc")
                If bRecalc Then
                    With RA.RiskOptimizer.CurrentScenario
                        Dim sCostData As String = GetCostParamsString()
                        'Calculate the model in order to get the Risk Dollar Value
                        Dim sRiskData As String = ""
                        Dim tSelectedEventIDs As List(Of Guid) = SelectedEventIDs
                        .OriginalRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.DoNotUse)
                        .OriginalRiskValueWithControls = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseAll)
                        'A1470 ===
                        .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                        'If Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput Then
                        '    .OriginalRiskValue = LEC.GetSimulatedRisk(ControlsUsageMode.DoNotUse, PRJ.isOpportunityModel)
                        '    .OriginalRiskValueWithControls = LEC.GetSimulatedRisk(ControlsUsageMode.UseAll, PRJ.isOpportunityModel)
                        '    .OptimizedRiskValue = LEC.GetSimulatedRisk(ControlsUsageMode.UseOnlyActive, PRJ.isOpportunityModel)
                        'End If
                        sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)

                        tResult = String.Format("['{0}', {1}, {2}, {3}]", sAction, sRiskData, sCostData, GetControlsData())
                    End With
                Else
                    tResult = String.Format("['{0}']", sAction)
                End If
            Case "control_type"
                Dim ctrlID As Guid = New Guid(GetParam(args, "id"))
                Dim sIDs As String = GetParam(args, "ids") ' if multiple controls marked then this parameter will not be empty
                Dim value As ControlType = CType(CInt(GetParam(args, "value")), ControlType)

                Dim tControls As New List(Of clsControl)
                If sIDs <> "" Then
                    Dim s As String() = sIDs.Split(CChar(","))
                    If s.Length > 0 Then
                        For Each sId In s
                            If sId.Length > 32 Then
                                tControls.Add(GetControlByID(New Guid(sId)))
                            End If
                        Next
                    End If
                Else
                    tControls.Add(GetControlByID(ctrlID))
                End If

                If tControls.Count() > 0 Then
                    For Each tControl As clsControl In tControls
                        tControl.Type = value
                        tControl.Assignments.Clear()
                    Next
                    WriteControls(PRJ, ParseString(String.Format("{0} %%Control%%)s) type changed to {1}", PagePrefix, value.ToString)))
                End If
                With RA.RiskOptimizer
                    Dim sCostData As String = GetCostParamsString()
                    tResult = String.Format("['{0}', '{1}', '{2}', {3}, {4}]", sAction, "", "", sCostData, "[]")
                End With
            Case "addcontrol"
                Dim name As String = GetParam(args, "name").Trim
                'Dim infodoc As String = GetParam(args, "descr").Trim ' -D4344
                Dim categories As String = GetParam(args, "cat").Trim
                Dim cost As Double = UNDEFINED_INTEGER_VALUE
                If Not String2Double(GetParam(args, "cost").Trim(), cost) Then cost = UNDEFINED_INTEGER_VALUE
                'Dim ctrl As clsControl = PM.Controls.AddControl(Guid.NewGuid, name, ControlType, infodoc)
                Dim ctrl As clsControl = PM.Controls.AddControl(Guid.NewGuid, name, ControlType.ctUndefined)    ' D4344
                ctrl.Cost = cost
                ctrl.Categories = categories
                WriteControls(PRJ)
                WriteAttributeValues(PRJ, ParseString(PagePrefix + "%%Control%% added"), ctrl.Name)    ' D3731
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetControlsData(), GetCategoriesData(), GetCostParamsString())
            Case "paste_controls"
                Dim clipboardData As String = GetParam(args, "data")
                clipboardData = htmlUnescape(Uri.UnescapeDataString(clipboardData))

                '' D4371 ===
                'If clipboardData.ToLower.Contains("nist.gov") AndAlso clipboardData.ToLower.StartsWith("http") Then
                '    Using client As New WebClient()
                '        Try
                '            client.UseDefaultCredentials = True
                '            client.Headers.Clear()
                '            client.Headers.Add(HttpRequestHeader.CacheControl, "no-cache")
                '            Dim sXML As String = client.DownloadString(clipboardData.Trim)
                '            If Not String.IsNullOrEmpty(sXML) AndAlso sXML.TrimStart().StartsWith("<?xml") Then clipboardData = sXML
                '        Catch ex As Exception
                '            clipboardData = ""
                '        End Try
                '    End Using
                'End If
                '' D4371 ==

                If clipboardData.TrimStart().StartsWith("<?xml") Then
                    CreateControlsFromXML(clipboardData)
                Else
                    CreateControlsFromString(clipboardData)
                End If

                'tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetControlsData(), GetCategoriesData(), GetCostParamsString())
                tResult = String.Format("['{0}',{1},{2},{3},{4},{5}]", sAction, GetAttributesData(), GetControlsData(), GetControlsColumns(), GetCategoriesData(), GetCostParamsString())
            Case "randomize_controls"
                ' Randomize controls
                RA.RiskOptimizer.RandomizeControls()
                'TODO: Save project snapshot
                ' Return updated controls data
                tResult = String.Format("['{0}', {1}]", "refresh", GetControlsData())
            Case "load_all_controls"
                LoadAllControls = Param2Bool(args, "value")
                tResult = String.Format("['{0}']", sAction)

            ' D3213 ===
            Case "scenario_reorder"
                Dim sLst As String = GetParam(args, "lst")
                Dim IDs As String() = sLst.Split(CChar(","))
                If IDs.Count = RA.Scenarios.Scenarios.Count Then
                    For i As Integer = 0 To IDs.Count - 1
                        Dim SID As Integer = -1
                        If Integer.TryParse(IDs(i), SID) Then RA.Scenarios.Scenarios(SID).Index = i + 1
                    Next
                    RA.Scenarios.CheckAndSortScenarios()
                    App.ActiveProject.SaveRA("Update scenarios", False, , "Reorder scenarios")
                    tResult = String.Format("[""{0}"", {1}]", sAction, GetScenariosData()) ' D3578 + D3757 + A1380
                End If
                ' D3213 ==
            Case "edit_scenario"    ' using for add new as well (ID=-1)
                Dim sID As String = GetParam(args, "id")
                Dim sName As String = GetParam(args, "name").Trim
                Dim sDesc As String = GetParam(args, "desc").Trim
                'Dim sGrpID As String = GetParam(args, "grp")    ' D3350
                Dim ID As Integer
                'Dim GrpID As Integer    ' D3350
                If Integer.TryParse(sID, ID) AndAlso sName <> "" Then
                    Dim RAScenario As RAScenario = Nothing
                    If ID < 0 Then
                        ' D3123 ===
                        Dim fCopy As Boolean = Str2Bool(GetParam(args, "copy"))
                        RAScenario = RA.Scenarios.AddScenario(If(fCopy, RA.Scenarios.ActiveScenarioID, -1)) ' D3098 + A1380
                        'If fCopy Then CopyScenario(RA.Scenarios.ActiveScenario, RAScenario)
                        ' D3123 ==
                    Else
                        If RA.Scenarios.Scenarios.ContainsKey(ID) Then RAScenario = RA.Scenarios.Scenarios(ID)
                    End If
                    If RAScenario IsNot Nothing Then
                        RAScenario.Name = sName
                        RAScenario.Description = sDesc
                        'If Not Integer.TryParse(sGrpID, GrpID) Then GrpID = -1 ' D3350
                        'RAScenario.CombinedGroupUserID = GrpID  ' D3350 + D3737
                        'fResetSolver = False
                        Dim SaveMsg As String = "Update Scenarios"    ' D3757
                        Dim SaveComment As String = String.Format(If(ID < 0, "Add", "Edit") + " '{0}'", sName)  ' D3557
                        Dim SaveRA As Boolean = True   ' D2909
                        ' D4208 ===
                        If GetParam(args, _PARAM_MODE).ToLower = "specific_portfolio" Then
                            RAScenario.CombinedGroupUserID = RA.Scenarios.ActiveScenario.CombinedGroupUserID
                            If RA.Solver.SolverState = raSolverState.raSolved Then
                                Dim tMusts As Boolean = Str2Bool(GetParam(args, "m"))
                                Dim tMustNot As Boolean = Str2Bool(GetParam(args, "mn"))
                                For Each tAlt As RAAlternative In RAScenario.AlternativesFull
                                    Dim tOrigAlt As RAAlternative = RA.Scenarios.ActiveScenario.Alternatives.Find(Function(x) x.ID = tAlt.ID)
                                    If tOrigAlt IsNot Nothing Then
                                        If tMusts AndAlso tOrigAlt.DisplayFunded > 0 Then tAlt.Must = True
                                        If tMustNot AndAlso tOrigAlt.DisplayFunded = 0 Then tAlt.MustNot = True
                                    End If
                                Next
                            End If
                            SaveMsg = "Create Specific Portfolio"
                            SaveComment = String.Format("Add Scenario '{0}' (based on '{1}' solve)", sName, RA.Scenarios.ActiveScenario.Name)
                        End If
                        ' D4208 ==
                        App.ActiveProject.SaveRA(If(SaveMsg = "", "", "RA: " + SaveMsg), , SaveMsg <> "", SaveComment) 'A1390
                        tResult = String.Format("[""{0}"", {1}]", sAction, GetScenariosData()) 'A1380
                    End If
                End If

                ' D3015 ===
            Case "copy_scenario"
                Dim sSrc As String = GetParam(args, "from")
                Dim sDest As String = GetParam(args, "to")
                Dim SrcID As Integer
                Dim DestID As Integer
                If Integer.TryParse(sSrc, SrcID) AndAlso Integer.TryParse(sDest, DestID) Then
                    If SrcID <> DestID AndAlso SrcID = RA.Scenarios.ActiveScenarioID AndAlso DestID >= 0 AndAlso DestID < RA.Scenarios.Scenarios.Count Then
                        Dim BaseScenario As RAScenario = RA.Scenarios.Scenarios(SrcID)
                        With RA.Scenarios.Scenarios(DestID)
                            .Budget = BaseScenario.Budget
                            ' D3840 ===
                            If BaseScenario.AlternativesFull.Count > 0 Then
                                .AlternativesFull.Clear()
                                For Each alt As RAAlternative In BaseScenario.AlternativesFull
                                    .AlternativesFull.Add(alt.Clone)
                                Next
                            End If
                            ' D3840 ==
                            .Settings = BaseScenario.Settings.Clone
                            .TimePeriods = BaseScenario.TimePeriods.Clone(RA.Scenarios.Scenarios(DestID))   ' D3840
                            .CombinedGroupUserID = BaseScenario.CombinedGroupUserID     ' D3737
                            RA.Scenarios.CheckAndSortScenarios()    ' D3213
                            Dim SaveMsg = "Update Scenarios"    ' D3757
                            Dim SaveComment = String.Format("Copy '{0}' to '{1}'", BaseScenario.Name, .Name)    ' D3757
                            App.ActiveProject.SaveRA(If(SaveMsg = "", "", "RA: " + SaveMsg), , SaveMsg <> "", SaveComment) 'A1390
                            tResult = String.Format("[""{0}"", {1}]", sAction, GetScenariosData()) 'A1380
                        End With
                    End If
                End If
                ' D3015 ==

            Case "delete_scenario"
                Dim sID As String = GetParam(args, "id")
                Dim ID As Integer
                If Integer.TryParse(sID, ID) AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) AndAlso ID > 0 AndAlso RA.Scenarios.Scenarios.Count > 1 Then  ' D3215
                    Dim SaveMsg = "Update Scenarios"
                    Dim SaveComment = String.Format("Delete '{0}'", RA.Scenarios.Scenarios(ID).Name)   ' D3757
                    If ID = RA.Scenarios.ActiveScenarioID Then RA.Scenarios.ActiveScenarioID = CType(RA.Scenarios.Scenarios.ElementAt(0).Value, RAScenario).ID ' D3215
                    'App.ActiveProject.ProjectManager.Parameters.Parameters.DeleteParameter(String.Format("{0}{1}", clsProjectParametersWithDefaults.RA_TIMEPERIODS_HAS_DATA, If(ID > 0, ID, "")))  ' D3841 -D3943
                    RA.Scenarios.DeleteScenario(ID)
                    RA.Scenarios.CheckAndSortScenarios()    ' D3213
                    App.ActiveProject.SaveRA(If(SaveMsg = "", "", "RA: " + SaveMsg), , SaveMsg <> "", SaveComment) 'A1390
                    tResult = String.Format("[""{0}"", {1}]", sAction, GetScenariosData()) 'A1380
                End If
                ' D2876 ==
            Case "paste_costs"
                Dim clip_data As String = GetParam(args, "data", True)
                Dim visControlsIds As String = GetParam(args, "ids", True)
                Dim cells As String() = clip_data.Split(Chr(10))
                Dim cells_count As Integer = cells.Count
                Dim ids As String() = visControlsIds.Split(CChar(","))
                Dim fValueChanged As Boolean = False
                If ids.Count < cells.Count Then cells_count = ids.Count
                For i As Integer = 0 To cells_count - 1
                    Dim sId As String = ids(i)
                    Dim tId As Guid = New Guid(ids(i))
                    Dim value As String = cells(i).Trim
                    If String.IsNullOrEmpty(value) Then value = Nothing
                    Dim alt As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(sId)
                    Dim ctrl As clsControl = PM.Controls.GetControlByID(tId)
                    Dim tDblValue As Double = 0
                    If value Is Nothing OrElse Not String2Double(value, tDblValue) Then tDblValue = UNDEFINED_INTEGER_VALUE
                    If alt IsNot Nothing AndAlso alt.Cost <> tDblValue Then
                        alt.Cost = tDblValue
                        fValueChanged = True
                    End If
                    If ctrl IsNot Nothing AndAlso ctrl.Cost <> tDblValue Then
                        ctrl.Cost = tDblValue
                        fValueChanged = True
                    End If
                Next
                If fValueChanged Then
                    PRJ.SaveRA(ParseString("Edit %%control%% register"), , , "Paste costs")
                    WriteAttributeValues(PRJ, "", "")
                    If RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve()
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetControlsData(), GetCategoriesData(), GetCostParamsString())
            ' COLUMNS
            Case "paste_attr"
                Dim clip_data As String = GetParam(args, "data", True)
                Dim AttrIndex As Integer = CInt(GetParam(args, "attr_idx", True))
                Dim visControlsIds As String = GetParam(args, "ids", True)
                Dim cells As String() = clip_data.Split(Chr(10))
                Dim cells_count As Integer = cells.Count
                Dim ids As String() = visControlsIds.Split(CChar(","))
                Dim fValueChanged As Boolean = False
                Dim fItemsChanged As Boolean = False
                If ids.Count < cells.Count Then cells_count = ids.Count
                Dim attrName As String = ""
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        attrName = attr.Name
                        Dim aEnum As clsAttributeEnumeration = Nothing
                        If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then aEnum = PM.Attributes.GetEnumByID(attr.EnumID)
                        For i As Integer = 0 To cells_count - 1
                            Dim sId As String = ids(i)
                            Dim tId As Guid = New Guid(ids(i))
                            Dim value As String = cells(i).Trim
                            If String.IsNullOrEmpty(value) Then value = Nothing
                            Dim ctrl As clsControl = PM.Controls.GetControlByID(tId)
                            If ctrl IsNot Nothing Then
                                Select Case attr.ValueType
                                    Case AttributeValueTypes.avtString
                                        If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, ctrl.ID, Guid.Empty) Then fValueChanged = True
                                    Case AttributeValueTypes.avtBoolean
                                        If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, Str2Bool(value), ctrl.ID, Guid.Empty) Then fValueChanged = True
                                    Case AttributeValueTypes.avtLong
                                        Dim intValue As Integer
                                        If String.IsNullOrEmpty(value) Then value = Nothing
                                        If String.IsNullOrEmpty(value) OrElse Integer.TryParse(value, intValue) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, intValue, ctrl.ID, Guid.Empty) Then fValueChanged = True
                                    Case AttributeValueTypes.avtDouble
                                        Dim dblValue As Double
                                        If String.IsNullOrEmpty(value) Then value = Nothing
                                        If String.IsNullOrEmpty(value) OrElse String2Double(value, dblValue) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, dblValue, ctrl.ID, Guid.Empty) Then fValueChanged = True
                                    Case AttributeValueTypes.avtEnumeration
                                        If Not String.IsNullOrEmpty(value) AndAlso value <> UNDEFINED_ATTRIBUTE_VALUE Then
                                            ' check if enum item exists and create new if not
                                            Dim enumItem As clsAttributeEnumerationItem = Nothing

                                            If aEnum IsNot Nothing AndAlso Not attr.EnumID.Equals(Guid.Empty) Then
                                                enumItem = aEnum.GetItemByValue(value)
                                            Else
                                                aEnum = New clsAttributeEnumeration()
                                                aEnum.ID = Guid.NewGuid
                                                aEnum.Name = attr.Name
                                                attr.EnumID = aEnum.ID
                                                PM.Attributes.Enumerations.Add(aEnum)
                                            End If

                                            If enumItem Is Nothing Then
                                                enumItem = aEnum.AddItem(value)
                                                fItemsChanged = True
                                            End If
                                            ' assign attribute value
                                            If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, enumItem.ID, ctrl.ID, Guid.Empty) Then fValueChanged = True
                                        Else
                                            If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, Nothing, ctrl.ID, Guid.Empty) Then fValueChanged = True
                                        End If
                                End Select
                            End If
                        Next
                    End If
                End If
                If fItemsChanged Then SaveAttributes(String.Format("Paste attribute data '{0}'", ShortString(attrName, 40)))
                If fValueChanged Then SaveAttributesValues(ParseString("Paste %%controls%% attribute values from clipboard"))
                tResult = String.Format("['{0}',{1},{2},{3},{4}]", sAction, GetControlsData(), GetCategoriesData(), GetCostParamsString(), GetAttributesData())
            Case "rename_column"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim NewName As String = GetParam(args, "name").Trim
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    AttributesList(AttrIndex).Name = NewName
                    Dim attr As clsAttribute = AttributesList(AttrIndex) 'PM.Attributes.GetAttributeByID(AttributesList(AttrIndex).ID)
                    If attr IsNot Nothing Then
                        attr.Name = NewName
                        SaveAttributes(String.Format("Rename column '{0}'", ShortString(NewName, 40, True)))    ' D3790
                    End If
                End If
                tResult = String.Format("['{0}',{1}]", sAction, GetAttributesData())
            Case "add_column"
                Dim NewName As String = GetParam(args, "name").Trim
                Dim NewType As AttributeValueTypes = CType(GetParam(args, "type"), AttributeValueTypes)
                Dim attr As clsAttribute = PM.Attributes.AddAttribute(Guid.NewGuid(), NewName, AttributeTypes.atControl, NewType, Nothing, False, Guid.Empty)
                If attr IsNot Nothing Then
                    'try to assign the default value
                    If NewType = AttributeValueTypes.avtString OrElse NewType = AttributeValueTypes.avtLong OrElse NewType = AttributeValueTypes.avtDouble OrElse NewType = AttributeValueTypes.avtBoolean Then
                        Dim DefVal As String = GetParam(args, "def_val").Trim
                        If Not String.IsNullOrEmpty(DefVal) Then
                            Select Case NewType
                                Case AttributeValueTypes.avtString
                                    attr.DefaultValue = DefVal
                                Case AttributeValueTypes.avtLong
                                    Dim tIntVal As Integer
                                    If Integer.TryParse(DefVal, tIntVal) Then attr.DefaultValue = tIntVal
                                Case AttributeValueTypes.avtDouble
                                    Dim tDblVal As Double
                                    If String2Double(DefVal, tDblVal) Then attr.DefaultValue = tDblVal
                                Case AttributeValueTypes.avtBoolean
                                    Dim tIntVal As Integer
                                    If Integer.TryParse(DefVal, tIntVal) Then attr.DefaultValue = tIntVal = 1
                            End Select
                        End If
                    End If

                    SaveAttributes(String.Format("Add attribute '{0}'", ShortString(NewName, 40)))    ' D3790
                End If

                tResult = String.Format("['{0}',{1},{2}]", sAction, GetAttributesData(), CInt(NewType))
            Case "del_column"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Dim sName As String = AttributesList(AttrIndex).Name    ' D3790
                        PM.Attributes.RemoveAttribute(attr.ID)
                        SaveAttributes(String.Format("Delete column '{0}'", ShortString(sName, 40)))    ' D3790
                    End If
                End If
                tResult = String.Format("['{0}',{1}]", sAction, GetAttributesData())
                'ITEMS
            Case "rename_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim ItemIndex As Integer = CInt(GetParam(args, "item"))
                Dim NewName As String = GetParam(args, "name").Trim
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                        If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 AndAlso ItemIndex >= 0 AndAlso ItemIndex < items.Items.Count Then
                            items.Items(ItemIndex).Value = NewName
                            SaveAttributes(String.Format("Rename item '{0}'", ShortString(NewName, 40)))    ' D3790
                        End If
                    End If
                End If
                tResult = String.Format("['{0}',{1}]", sAction, GetAttributesData())
            Case "add_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim NewName As String = GetParam(args, "name").Trim
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty) Then
                        aEnum = New clsAttributeEnumeration()
                        aEnum.ID = Guid.NewGuid
                        aEnum.Name = attr.Name
                        attr.EnumID = aEnum.ID
                        PM.Attributes.Enumerations.Add(aEnum)
                    End If

                    Dim eItem As clsAttributeEnumerationItem = aEnum.AddItem(NewName)
                    SaveAttributes(String.Format("Add item '{0}'", ShortString(NewName, 40)))    ' D3790
                End If
                tResult = String.Format("['{0}',{1}]", sAction, GetAttributesData())
            Case "del_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim ItemIndex As Integer = CInt(GetParam(args, "item"))
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 AndAlso ItemIndex >= 0 AndAlso ItemIndex < items.Items.Count Then
                        Dim sName As String = items.Items(ItemIndex).Value
                        items.Items.RemoveAt(ItemIndex)
                        SaveAttributes(String.Format("Delete item '{0}'", ShortString(sName, 40)))    ' D3790
                    End If
                End If
                tResult = String.Format("['{0}',{1}]", sAction, GetAttributesData())
            Case "set_attr_value"
                Dim AttrIndex As Integer = CInt(GetParam(args, "attr_idx"))
                Dim CtrlGUID As Guid = New Guid(CStr(GetParam(args, "alt_idx")))
                Dim sIDs As String = GetParam(args, "ids") ' if multiple controls marked then this parameter will not be empty
                Dim attrValueType As Integer = CInt(AttributeValueTypes.avtString)
                Dim fValueChanged As Boolean = False

                Dim tControls As New List(Of clsControl)
                If sIDs <> "" Then
                    Dim s As String() = sIDs.Split(CChar(","))
                    If s.Length > 0 Then
                        For Each sId In s
                            If sId.Length > 32 Then
                                tControls.Add(GetControlByID(New Guid(sId)))
                            End If
                        Next
                    End If
                Else
                    tControls.Add(GetControlByID(CtrlGUID))
                End If

                If tControls.Count() > 0 Then
                    If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                        Dim attr As clsAttribute = AttributesList(AttrIndex)
                        attrValueType = CInt(attr.ValueType)
                        Dim sValue As String = GetParam(args, "value")
                        For Each tControl As clsControl In tControls
                            Select Case attr.ValueType
                                Case AttributeValueTypes.avtString
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, attr.ValueType, sValue, tControl.ID, Guid.Empty) Then
                                        fValueChanged = True
                                    End If
                                Case AttributeValueTypes.avtBoolean
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, attr.ValueType, Str2Bool(sValue), tControl.ID, Guid.Empty) Then
                                        fValueChanged = True
                                    End If
                                Case AttributeValueTypes.avtLong
                                    Dim intValue As Long
                                    If (String.IsNullOrEmpty(sValue) OrElse Long.TryParse(sValue, intValue)) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, attr.ValueType, If(String.IsNullOrEmpty(sValue), Nothing, intValue), tControl.ID, Guid.Empty) Then
                                        fValueChanged = True
                                    End If
                                Case AttributeValueTypes.avtDouble
                                    Dim dblValue As Double
                                    If (String.IsNullOrEmpty(sValue) OrElse String2Double(sValue, dblValue)) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, attr.ValueType, If(String.IsNullOrEmpty(sValue), Nothing, dblValue), tControl.ID, Guid.Empty) Then
                                        fValueChanged = True
                                    End If
                                Case AttributeValueTypes.avtEnumeration
                                    Dim sEnumIdx As String = CStr(GetParam(args, "enum_idx"))
                                    Dim ItemGUID As Object = Nothing
                                    If sEnumIdx <> "-1" AndAlso sEnumIdx.Length >= 32 Then ItemGUID = New Guid(sEnumIdx)
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, ItemGUID, tControl.ID, Guid.Empty) Then
                                        fValueChanged = True
                                    End If
                                Case AttributeValueTypes.avtEnumerationMulti
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumerationMulti, sValue, tControl.ID, Guid.Empty) Then
                                        fValueChanged = True
                                    End If
                            End Select

                        Next
                        If fValueChanged Then
                            SaveAttributesValues(String.Format("Set attribute value '{0}'", ShortString(App.GetAttributeName(attr), 40)))
                        End If
                    End If

                End If
                'tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetControlsData(), If(tControls.Count() > 1, 1, 0), attrValueType)
                tResult = String.Format("['{0}',{1}]", sAction, Bool2JS(fValueChanged))

            Case "attributes_reorder"
                Dim fChanged As Boolean = False
                Dim lst As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "lst", True))    ' Anti-XSS
                If Not String.IsNullOrEmpty(lst) Then
                    Dim attrList As List(Of clsAttribute) = PM.Attributes.GetControlsAttributes(True)
                    Dim globalI As New List(Of Integer)
                    For Each attr In attrList
                        globalI.Add(PM.Attributes.AttributesList.IndexOf(attr))
                    Next
                    Dim indices As String() = lst.Split(CChar(","))
                    Dim i As Integer = 0
                    For Each id As String In indices
                        Dim idx As Integer = CInt(id)
                        If idx >= 0 AndAlso idx < attrList.Count AndAlso idx <> i Then
                            Dim attrN As clsAttribute = attrList(idx)
                            PM.Attributes.AttributesList.RemoveAt(globalI(i))
                            PM.Attributes.AttributesList.Insert(globalI(i), attrN)
                            fChanged = True
                        End If
                        i += 1
                    Next
                End If
                If fChanged Then
                    SaveAttributes("Reorder attributes")    ' D3790
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", "refresh_full", GetControlsData(), GetAttributesData(), GetControlsColumns())

            Case "enum_items_reorder"
                Dim fChanged As Boolean = False
                Dim lst As String = GetParam(args, "lst", True)
                Dim attrIdx As Integer = CInt(GetParam(args, "clmn", True))
                Dim attrList As List(Of clsAttribute) = PM.Attributes.GetControlsAttributes(True)
                If Not String.IsNullOrEmpty(lst) AndAlso attrIdx >= 0 AndAlso attrIdx < attrList.Count Then
                    Dim attr As clsAttribute = attrList(attrIdx)
                    If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                        If aEnum IsNot Nothing Then
                            Dim ordered_guids As String() = lst.Split(CChar(","))
                            Dim enumList As New List(Of clsAttributeEnumerationItem)
                            For Each id As String In ordered_guids
                                Dim item_guid As Guid = New Guid(id)
                                enumList.Add(aEnum.GetItemByID(item_guid))
                            Next
                            aEnum.Items = enumList
                            fChanged = True
                        End If
                    End If
                End If
                If fChanged Then SaveAttributes("Items reorder") ' D3790
                tResult = String.Format("['{0}',{1}]", sAction, GetAttributesData())

            Case "set_default_value"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim value As String = GetParam(args, "def_val").Trim
                Dim fValueChanged As Boolean = False
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Select Case attr.ValueType
                            Case AttributeValueTypes.avtString
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                Else
                                    attr.DefaultValue = value
                                End If
                                fValueChanged = True
                            Case AttributeValueTypes.avtBoolean
                                attr.DefaultValue = Str2Bool(value)
                                fValueChanged = True
                            Case AttributeValueTypes.avtLong
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                Else
                                    Dim intValue As Integer
                                    If Integer.TryParse(value, intValue) Then
                                        attr.DefaultValue = intValue
                                        fValueChanged = True
                                    End If
                                End If
                            Case AttributeValueTypes.avtDouble
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                Else
                                    Dim dblValue As Double
                                    If String2Double(value, dblValue) Then
                                        attr.DefaultValue = dblValue
                                        fValueChanged = True
                                    End If
                                End If
                            Case AttributeValueTypes.avtEnumeration
                                Dim ItemIndex As Integer = CInt(GetParam(args, "item_index"))
                                If Str2Bool(value) Then
                                    Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                    If aEnum IsNot Nothing Then
                                        If ItemIndex >= 0 AndAlso ItemIndex < aEnum.Items.Count Then
                                            attr.DefaultValue = aEnum.Items(ItemIndex).ID
                                            fValueChanged = True
                                        End If
                                    End If
                                Else
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                End If
                            Case AttributeValueTypes.avtEnumerationMulti
                                Dim ItemIndex As Integer = CInt(GetParam(args, "item_index"))
                                If value = "1" Then
                                    Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                    If aEnum IsNot Nothing Then
                                        If ItemIndex >= 0 AndAlso ItemIndex < aEnum.Items.Count Then
                                            attr.DefaultValue = aEnum.Items(ItemIndex).ID.ToString
                                            fValueChanged = True
                                        End If
                                    End If
                                Else
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                End If
                        End Select
                        If fValueChanged Then SaveAttributes(String.Format("Set default for '{0}'", ShortString(App.GetAttributeName(attr), 40))) ' D3790
                    End If
                End If
                tResult = String.Format("['{0}',{1},{2}]", sAction, GetAttributesData(), GetControlsData())
            'Case "show_hidden_settings"
            '    ShowHiddenSettings = Param2Bool(args, "val")
            '    tResult = String.Format("['{0}']", sAction)
            'Case "show_options"
            '    ShowOptions = Param2Bool(args, "val")
            '    tResult = String.Format("['{0}']", sAction)
            Case "selected_node"
                Dim sGuid As String = GetParam(args, "value", True)
                Dim sMessage As String = ""
                'If (CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES Or CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS) AndAlso Not RA.RiskOptimizer.HasControlsForOptimization(SelectedHierarchyNode, SelectedEventIDs) Then
                '    sMessage = ParseString("No %%controls%% available for specified " + If(SelectedHierarchyNode.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood, "%%objectives(l)%%", "%%objectives(i)%%"))
                'End If
                SelectedHierarchyNodeID = New Guid(sGuid)
                tResult = String.Format("[""{0}"",""{1}"",""{2}""]", sAction, JS_SafeString(WRTNodeName), JS_SafeString(sMessage))
            Case "num_sim"
                Dim sVal As String = GetParam(args, "val")
                Dim iVal As Integer = UNDEFINED_INTEGER_VALUE
                If Not String.IsNullOrEmpty(sVal) Then
                    If Integer.TryParse(sVal, iVal) Then 'AndAlso LEC.NumberOfSimulations <> iVal AndAlso iVal > 0 AndAlso iVal <= _MAX_NUM_SIMULATIONS Then 
                        PM.RiskSimulations.NumberOfTrials = iVal
                    End If
                End If
                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)
                    tResult = String.Format("['{0}','{1}','{2}', {3}, {4}]", sAction, "", getInitialOptimizedRisk(), sCostData, sRiskData)
                End With
            Case "rand_seed"
                Dim sVal As String = GetParam(args, "val")
                Dim iVal As Integer = UNDEFINED_INTEGER_VALUE
                If Not String.IsNullOrEmpty(sVal) Then
                    If Integer.TryParse(sVal, iVal) AndAlso PM.RiskSimulations.RandomSeed <> iVal AndAlso iVal > 0 Then
                        PM.RiskSimulations.RandomSeed = iVal
                    End If
                End If
                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)
                    tResult = String.Format("['{0}','{1}','{2}', {3}, {4}]", sAction, "", getInitialOptimizedRisk(), sCostData, sRiskData)
                End With
            Case "keep_rand_seed"
                Dim bVal As Boolean = Param2Bool(args, "val")
                PM.RiskSimulations.KeepRandomSeed = bVal
                With RA.RiskOptimizer.CurrentScenario
                    Dim sCostData As String = GetCostParamsString()
                    'Calculate the model in order to get the Risk Dollar Value
                    Dim sRiskData As String = ""
                    .OptimizedRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
                    sRiskData = GetRiskParamsString(ShowDollarValue, .OptimizedRiskValue, .OriginalRiskValueWithControls, .OriginalRiskValue)
                    tResult = String.Format("['{0}','{1}','{2}', {3}, {4}]", sAction, "", getInitialOptimizedRisk(), sCostData, sRiskData)
                End With
            Case "show_timeperiods"
                tResult = String.Format("['{0}']", sAction)
            Case "objective_function_type"
                PM.ResourceAlignerRisk.Solver.ObjectiveFunctionType = CType(Param2Int(args, "value"), ObjectiveFunctionType)
                PM.ResourceAlignerRisk.Save()
                tResult = String.Format("['{0}']", sAction)

                 ' RA/Time Periods
            Case "setresource"
                Dim sProjectID As String = GetParam(args, "project_id")
                Dim sResID As String = GetParam(args, "res_id")
                Dim sVal As String = GetParam(args, "values")
                Dim sTotal As String = GetParam(args, "total")
                Dim sDM As String = GetParam(args, "dm")
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
                                    alt.Cost = ATotal
                                    RA.SetAlternativeCost(sProjectID, ATotal)
                                End If
                            Else
                                Dim CC As RAConstraint = Scenario.Constraints.Constraints(res.ConstraintID)
                                If CC IsNot Nothing Then Scenario.Constraints.SetConstraintValue(res.ConstraintID, sProjectID, ATotal)
                            End If
                        End If
                    End If

                    sComment = "Set resource"  ' D3791
                End If
                fDoSolve = False
                sResult = "'ok'"

            Case "setresourcegrid"
                Dim sResID As String = GetParam(args, "res_id")
                'Dim sVal As String = GetParam(args, "values")
                'Dim sTotal As String = GetParam(args, "total")
                Dim sVal As String = CheckVar("values", "")
                Dim sTotal As String = CheckVar("total", "")
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
                                            alt.Cost = ATotalVal
                                            RA.SetAlternativeCost(sProjectID, ATotalVal)
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
                Dim sProjectID As String = GetParam(args, "project_id")
                Dim sResID As String = GetParam(args, "res_id")
                Dim sVal As String = GetParam(args, "value")
                If Scenario IsNot Nothing Then
                    Dim AVal As Double = 0
                    sResID = sResID.Replace("{", "")
                    sResID = sResID.Replace("}", "")
                    If String2Double(sVal, AVal) Then
                        Dim res As RAResource = Scenario.TimePeriods.Resources(New Guid(sResID))
                        If res.ConstraintID < 0 Then
                            Dim alt As RAAlternative = RA.GetAlternativeById(Scenario, sProjectID)
                            If alt IsNot Nothing Then
                                alt.Cost = AVal
                                RA.SetAlternativeCost(sProjectID, AVal)
                            End If
                        Else
                            Dim CC As RAConstraint = Scenario.Constraints.Constraints(res.ConstraintID)
                            If CC IsNot Nothing Then Scenario.Constraints.SetConstraintValue(res.ConstraintID, sProjectID, AVal)
                        End If
                        RA.Save()
                    End If
                End If
                fDoSolve = False
                sResult = "'ok'"

            Case "changeresourceid"
                Dim sResID As String = GetParam(args, "res_id")
                TP_RES_ID = sResID
                sResult = "'ok'"

            Case "expandprojects" ' obsolete
                If Scenario IsNot Nothing Then
                    Dim PeriodsCount = Scenario.TimePeriods.Periods.Count
                    For Each proj In Scenario.Alternatives
                        Dim projData = Scenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        Scenario.TimePeriods.PeriodsData.SetMaxPeriod(proj.ID, PeriodsCount - projData.Duration)
                    Next
                    sComment = "Expand All Projects Maxs"  ' D3791
                End If
                fDoSolve = False
                sResult = "'ok'"

            Case "contractprojects" ' obsolete
                If Scenario IsNot Nothing Then
                    For Each proj In Scenario.Alternatives
                        Dim projData = Scenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(proj.ID)
                        Scenario.TimePeriods.PeriodsData.SetMaxPeriod(proj.ID, projData.MinPeriod)
                    Next
                    sComment = "Contract All Projects Maxs"
                End If
                fDoSolve = False
                sResult = "'ok'"

            Case "trimtimeline" 'obsolete
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
                    fDoSolve = False
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
                fDoSolve = False
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
                fDoSolve = False
                sResult = "'ok'"

            Case "tpsetmaxval"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower 'Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower    'Anti-XSS
                Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower  'Anti-XSS
                Dim AVal As Double = 0
                String2Double(sVal, AVal)
                Scenario.TimePeriods.Periods(CInt(sPeriodID)).SetResourceMaxValue(New Guid(sResID), AVal)    ' D3918
                sComment = "Set resource maximum"  ' D3791
                fDoSolve = False
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
                fDoSolve = False
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
                fDoSolve = False
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
                    fDoSolve = False
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
                    fDoSolve = False
                End If
                sResult = "'ok'"

            Case "setpage"
                Dim sPg As String = GetParam(args, "pg")
                Dim Pg As Integer = RA_Pages_CurPage
                If Integer.TryParse(sPg, Pg) Then RA_Pages_CurPage = Pg
                sResult = RA_Pages_CurPage.ToString
                tResult = String.Format("['{0}']", sAction)

        End Select

        If sResult = "'ok'" Then
            tResult = String.Format("['{0}']", sAction)
            PRJ.SaveRA("Time periods changes")
        End If

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Sub SaveAttributes(sComment As String)  ' D3790
        PM.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
        'RA.Scenarios.SyncLinkedConstraints(Nothing) ' D3379
        If sComment <> "" Then App.ActiveProject.SaveRA("Edit control attributes", , , sComment) ' D3790
    End Sub

    Private Sub SaveAttributesValues(sComment As String)    ' D3790
        PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
        'RA.Scenarios.SyncLinkedConstraints(Nothing) ' D3379
        If sComment <> "" Then App.ActiveProject.SaveRA("Edit control attributes", , , sComment) ' D3790
    End Sub


    Private Function GetBudgetParamsString(tBudgetLimit As Double, tMaxRisk As Double, tMinRiskReduction As Double) As String
        Dim sBudgetData As String = ""
        sBudgetData += String.Format("{0}", JS_SafeNumber(Math.Round(tBudgetLimit, CostDecimalDigits)))
        sBudgetData += String.Format(",{0}", If(ShowDollarValue, JS_SafeNumber(Math.Round(tMaxRisk * PM.DollarValueOfEnterprise, CostDecimalDigits)), JS_SafeNumber(tMaxRisk * 100)))
        sBudgetData += String.Format(",{0}", If(ShowDollarValue, JS_SafeNumber(Math.Round(tMinRiskReduction * PM.DollarValueOfEnterprise, CostDecimalDigits)), JS_SafeNumber(tMinRiskReduction * 100)))
        sBudgetData = String.Format("[{0}]", sBudgetData)
        Return sBudgetData
    End Function

    Private Function GetCostParamsString() As String
        Dim tFundedCost As Double = Treatments.Sum(Function(ctrl) If(ctrl.Enabled AndAlso ctrl.Active AndAlso ctrl.IsCostDefined, ctrl.Cost, 0))
        Dim tAllCost As Double = Treatments.Sum(Function(ctrl) If(ctrl.Enabled AndAlso ctrl.IsCostDefined, ctrl.Cost, 0))
        Dim tWithAssignmentsCost As Double = Treatments.Sum(Function(ctrl) If(ctrl.Enabled AndAlso ctrl.IsCostDefined AndAlso RA.Solver.ControlHasApplications(ctrl, SelectedHierarchyNode, SelectedEventIDs), ctrl.Cost, 0))
        Dim tActiveCount As Integer = Treatments.Where(Function(ctrl) ctrl.Enabled AndAlso ctrl.Active).Count
        Dim sCostData As String = ""
        sCostData += String.Format("'{0}'", JS_SafeString(CostString(tFundedCost, CostDecimalDigits, True) + DeltaString(tAllCost, tFundedCost, CostDecimalDigits, True, ResString("lblUnfunded") + ":")))
        sCostData += String.Format(",'{0}'", JS_SafeString(CostString(tAllCost, CostDecimalDigits, True)))
        sCostData += String.Format(",'{0}'", tActiveCount)
        sCostData += String.Format(",'{0}'", JS_SafeString(CostString(tWithAssignmentsCost, CostDecimalDigits, True)))
        sCostData = String.Format("[{0}]", sCostData)
        Return sCostData
    End Function

    Function GetCurRiskReduction(tShowDollarValue As Boolean) As Double
        With RA.RiskOptimizer.CurrentScenario
            Dim tRiskReduction As Double = 0
            If .OriginalRiskValue <> 0 Then
                tRiskReduction = 1 - .OptimizedRiskValue / .OriginalRiskValue
            End If
            If tShowDollarValue Then tRiskReduction *= PM.DollarValueOfEnterprise
            Return tRiskReduction
        End With
    End Function

    Public ReadOnly Property Use_Simulated_Values As SimulatedValuesUsageMode
        Get
            'If Not ShowHiddenSettings Then Return SimulatedValuesUsageMode.Computed
            Dim retVal As SimulatedValuesUsageMode = CType(PM.Parameters.Riskion_Use_Simulated_Values, SimulatedValuesUsageMode)
            If retVal = SimulatedValuesUsageMode.Computed Then Return SimulatedValuesUsageMode.Computed Else Return SimulatedValuesUsageMode.SimulatedInputAndOutput
            'Return CType(PM.Parameters.Riskion_Use_Simulated_Values, SimulatedValuesUsageMode)
        End Get
    End Property

    Private Function GetRiskParamsString(tShowDollarValue As Boolean, tOptimizedRisk As Double, OriginalRiskValueWithControls As Double, OriginalRiskValue As Double) As String
        If Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput Then
            PM.RiskSimulations.SimulateCommon(COMBINED_USER_ID, ControlsUsageMode.DoNotUse, WRTNode)
            OriginalRiskValue = PM.RiskSimulations.SimulatedRisk 'LEC.GetSimulatedRisk(ControlsUsageMode.DoNotUse, PRJ.isOpportunityModel)
            PM.RiskSimulations.SimulateCommon(COMBINED_USER_ID, ControlsUsageMode.UseAll, WRTNode)
            OriginalRiskValueWithControls = PM.RiskSimulations.SimulatedRisk 'LEC.GetSimulatedRisk(ControlsUsageMode.UseAll, PRJ.isOpportunityModel)
            PM.RiskSimulations.SimulateCommon(COMBINED_USER_ID, ControlsUsageMode.UseOnlyActive, WRTNode)
            tOptimizedRisk = PM.RiskSimulations.SimulatedRisk 'LEC.GetSimulatedRisk(ControlsUsageMode.UseOnlyActive, PRJ.isOpportunityModel)
            'RA.RiskOptimizer.CurrentScenario.OriginalRiskValue = OriginalRiskValue
            'RA.RiskOptimizer.CurrentScenario.OptimizedRiskValue = tOptimizedRisk
            RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls = OriginalRiskValueWithControls
        End If

        Dim sRiskData As String = ""
        If tShowDollarValue Then
            Dim tDollEnterprise As Double = PM.DollarValueOfEnterprise
            sRiskData += String.Format("'{0}'", If(tOptimizedRisk >= 0, JS_SafeString(CostString(tOptimizedRisk * tDollEnterprise, CostDecimalDigits, True) + DeltaString(OriginalRiskValue * tDollEnterprise, tOptimizedRisk * tDollEnterprise, CostDecimalDigits, True)), JS_SafeString("<span class='error'>Error</span>")))
            sRiskData += String.Format(",'{0}'", JS_SafeString(CostString(OriginalRiskValueWithControls * tDollEnterprise, CostDecimalDigits, True) + DeltaString(OriginalRiskValue * tDollEnterprise, OriginalRiskValueWithControls * tDollEnterprise, CostDecimalDigits, True)))
            sRiskData += String.Format(",'{0}'", JS_SafeString(CostString(OriginalRiskValue * tDollEnterprise, CostDecimalDigits, True)))
            Dim tRiskReduction As Double = 0
            If OriginalRiskValue <> 0 Then
                tRiskReduction = 1 - tOptimizedRisk / OriginalRiskValue
                tRiskReduction *= tDollEnterprise
            End If
            sRiskData += String.Format(",'{0}'", JS_SafeString(CostString(tRiskReduction, CostDecimalDigits, True))) ' Risk Reduction
        Else
            sRiskData += String.Format("'{0}'", If(tOptimizedRisk >= 0, JS_SafeString((tOptimizedRisk * 100).ToString("F2") + "%" + DeltaString(OriginalRiskValue, tOptimizedRisk, CostDecimalDigits, False)), JS_SafeString("<span class='error'>Error</span>")))
            sRiskData += String.Format(",'{0}'", JS_SafeString((OriginalRiskValueWithControls * 100).ToString("F2")) + "%" + DeltaString(OriginalRiskValue, OriginalRiskValueWithControls, CostDecimalDigits, False))
            sRiskData += String.Format(",'{0}'", JS_SafeString((OriginalRiskValue * 100).ToString("F2")) + "%")
            Dim tRiskReduction As Double = 0
            If OriginalRiskValue <> 0 Then
                tRiskReduction = 1 - tOptimizedRisk / OriginalRiskValue
            End If
            sRiskData += String.Format(",'{0}'", JS_SafeString((tRiskReduction * 100).ToString("F2")) + "%") ' Risk Reduction
        End If
        sRiskData = String.Format("[{0},{1}]", sRiskData, Bool2JS(Use_Simulated_Values <> SimulatedValuesUsageMode.Computed))
        Return sRiskData
    End Function

    Private Function OptimizeTreatments(ByRef totalCost As Double, ByRef optimizedRiskValue As Double, Optional OutputPath As String = "") As List(Of Guid)
        Dim fundedControls As New List(Of Guid)
        optimizedRiskValue = 0
        totalCost = 0
        OptimizerError = OptimizerErrors.oeNone

        Dim tOldConsequenceSimulationsMode As ConsequencesSimulationsMode = PM.CalculationsManager.ConsequenceSimulationsMode
        If PM.CalculationsManager.ConsequenceSimulationsMode <> ConsequencesSimulationsMode.Diluted Then
            PM.CalculationsManager.ConsequenceSimulationsMode = ConsequencesSimulationsMode.Diluted
        End If

        Dim state As raSolverState

        If PM.Controls.SetControlsVars(SelectedEventIDs) > 0 Then ' If count of applicable controls = 0 then Baron solver returns a Syntax Error
            optimizedRiskValue = RA.RiskOptimizer.Optimize(SelectedEventIDs, SelectedHierarchyNodeID, fundedControls, totalCost, OutputPath,, state)
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

        If PM.CalculationsManager.ConsequenceSimulationsMode <> tOldConsequenceSimulationsMode Then
            PM.CalculationsManager.ConsequenceSimulationsMode = tOldConsequenceSimulationsMode
        End If

        Return fundedControls
    End Function

#Region "Create Controls from XML or Paste"

    Private Function UnescapeDescriptions(inputString As String) As String
        Dim retVal As String = ""
        Dim i As Integer = 0
        Dim quoteFound As Boolean = False
        While i < inputString.Length
            Dim c = inputString(i)
            If quoteFound Then
                If c = """" Then
                    quoteFound = False
                Else
                    retVal += If(c <> vbCr AndAlso c <> vbLf AndAlso c <> vbCrLf AndAlso c <> vbTab AndAlso c <> Chr(10) AndAlso c <> Chr(9), c, " ")
                End If
            Else
                If c = """" Then
                    quoteFound = True
                Else
                    retVal += c
                End If
            End If
            i += 1
        End While
        Return retVal
    End Function

    Private Sub CreateControlsFromString(ByVal inputString As String)
        inputString = UnescapeDescriptions(inputString)
        Dim rows As String() = inputString.Split(Chr(10))

        Dim existingAttributes As List(Of clsAttribute) = PM.Attributes.GetControlsAttributes(True, True, AttributeValueTypes.avtEnumeration)
        Dim fSaveAttributes As Boolean = False
        Dim fValueChanged As Boolean = False

        For i As Integer = 0 To rows.Count - 1
            Dim value As String = rows(i).Trim
            If Not String.IsNullOrEmpty(value) Then
                Dim cols As String() = value.Split(CChar(vbTab))
                Dim cols_count As Integer = cols.Count
                If cols_count > 0 Then
                    Dim tName As String = cols(0).Trim
                    Dim tDescription As String = ""
                    Dim tCost As Double = UNDEFINED_INTEGER_VALUE
                    If Not String.IsNullOrEmpty(tName) AndAlso GetTreatmentByName(tName.Trim, ControlType.ctUndefined) Is Nothing Then
                        If cols_count > 1 Then tDescription = cols(1).Trim
                        If cols_count > 2 Then If Not String2Double(cols(2).Trim, tCost) Then tCost = UNDEFINED_INTEGER_VALUE

                        Dim ctrl As clsControl = PM.Controls.AddControl(Guid.NewGuid, tName, ControlType.ctUndefined, tDescription)
                        ctrl.Cost = tCost
                        fValueChanged = True

                        ' parse categories
                        If cols_count > 3 Then ' Has Categories
                            For k As Integer = 3 To cols_count - 1
                                Dim sCat As String = cols(k)
                                Dim tCatName As String = sCat.Trim
                                Dim curAttr As clsAttribute
                                If existingAttributes.Count > k - 3 Then
                                    curAttr = existingAttributes(k - 3)
                                Else
                                    Dim tEnum As clsAttributeEnumeration = New clsAttributeEnumeration()
                                    tEnum.ID = Guid.NewGuid
                                    curAttr = PM.Attributes.AddAttribute(Guid.NewGuid, "Category " + (k - 2).ToString, AttributeTypes.atControl, AttributeValueTypes.avtEnumeration, , False, tEnum.ID)
                                    PM.Attributes.Enumerations.Add(tEnum)
                                    existingAttributes = PM.Attributes.GetControlsAttributes(True, True, AttributeValueTypes.avtEnumeration)
                                    fValueChanged = True
                                End If

                                If curAttr IsNot Nothing AndAlso tCatName <> "" Then
                                    Dim existingCategory As clsAttributeEnumerationItem = GetCategoryByName(tCatName, curAttr)
                                    Dim catID As Guid
                                    If existingCategory Is Nothing Then
                                        catID = Guid.NewGuid()
                                        AddEnumAttributeItem(curAttr.ID, AttributeTypes.atControl, catID, tCatName, False)
                                    Else
                                        catID = existingCategory.ID
                                    End If
                                    PM.Attributes.SetAttributeValue(curAttr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, catID, ctrl.ID, Guid.Empty)
                                    fValueChanged = True
                                End If
                            Next
                        End If

                    End If
                End If
            End If
        Next

        If fValueChanged Then
            WriteControls(PRJ)
            WriteAttributes(PRJ)
            WriteAttributeValues(PRJ, ParseString(PagePrefix + "Paste %%controls%% from clipboard"), "Paste")
        End If
    End Sub

    Private HasAttributeFamily As Boolean = False
    Private HasAttributePriority As Boolean = False
    Private HasAttributeImpactLow As Boolean = False
    Private HasAttributeImpactModerate As Boolean = False
    Private HasAttributeImpactHigh As Boolean = False
    Private AttributeFamilyID As Guid
    Private AttributePriorityID As Guid
    Private AttributeImpactLowID As Guid
    Private AttributeImpactModerateID As Guid
    Private AttributeImpactHighID As Guid
    Private AttributeFamily As clsAttribute
    Private AttributePriority As clsAttribute
    Private AttributeImpactLow As clsAttribute
    Private AttributeImpactModerate As clsAttribute
    Private AttributeImpactHigh As clsAttribute

    Private Sub CreateControlsFromXML(ByVal XMLString As String)
        Dim fValueChanged As Boolean = False
        Dim xDoc As XmlDocument = New XmlDocument()

        Try
            xDoc.LoadXml(XMLString)
        Catch
        End Try

        If xDoc.DocumentElement IsNot Nothing AndAlso xDoc.DocumentElement.Name = "controls:controls" AndAlso xDoc.DocumentElement.HasChildNodes Then
            Dim xControlsList As XmlNodeList = xDoc.DocumentElement.ChildNodes

            If xControlsList IsNot Nothing Then

                Dim fHasInfodocs As Boolean = False     ' D4371
                Dim fLoadEnhancementsAsControls As Boolean = LoadAllControls

                Dim attrFamilyList = AttributesList.Where(Function(attr) attr.Name.Trim.ToLower = "family" AndAlso attr.ValueType = AttributeValueTypes.avtEnumeration)
                HasAttributeFamily = attrFamilyList.Count > 0
                If HasAttributeFamily Then
                    AttributeFamily = attrFamilyList(0)
                    AttributeFamilyID = attrFamilyList(0).ID
                Else
                    AttributeFamilyID = Guid.NewGuid
                    AttributeFamily = PM.Attributes.AddAttribute(AttributeFamilyID, "Family", AttributeTypes.atControl, AttributeValueTypes.avtEnumeration, , , , "tblControlFamily")
                    fValueChanged = True
                End If
                Dim attrPriorityList = AttributesList.Where(Function(attr) attr.Name.Trim.ToLower = "priority" AndAlso attr.ValueType = AttributeValueTypes.avtEnumeration)
                HasAttributePriority = attrPriorityList.Count > 0
                If HasAttributePriority Then
                    AttributePriority = attrPriorityList(0)
                    AttributePriorityID = attrPriorityList(0).ID
                Else
                    AttributePriorityID = Guid.NewGuid
                    AttributePriority = PM.Attributes.AddAttribute(AttributePriorityID, "Priority", AttributeTypes.atControl, AttributeValueTypes.avtEnumeration, , , , "tblControlPriority")
                    fValueChanged = True
                End If
                Dim attrLowList = AttributesList.Where(Function(attr) attr.Name.Trim.ToLower = "impact: low" AndAlso attr.ValueType = AttributeValueTypes.avtBoolean)
                HasAttributeImpactLow = attrLowList.Count > 0
                If HasAttributeImpactLow Then
                    AttributeImpactLow = attrLowList(0)
                    AttributeImpactLowID = attrLowList(0).ID
                Else
                    AttributeImpactLowID = Guid.NewGuid
                    AttributeImpactLow = PM.Attributes.AddAttribute(AttributeImpactLowID, "Impact: Low", AttributeTypes.atControl, AttributeValueTypes.avtBoolean, , , , "tblControlImpactLow")
                    fValueChanged = True
                End If
                Dim attrModList = AttributesList.Where(Function(attr) attr.Name.Trim.ToLower = "impact: moderate" AndAlso attr.ValueType = AttributeValueTypes.avtBoolean)
                HasAttributeImpactModerate = attrModList.Count > 0
                If HasAttributeImpactModerate Then
                    AttributeImpactModerate = attrModList(0)
                    AttributeImpactModerateID = attrModList(0).ID
                Else
                    AttributeImpactModerateID = Guid.NewGuid
                    AttributeImpactModerate = PM.Attributes.AddAttribute(AttributeImpactModerateID, "Impact: Moderate", AttributeTypes.atControl, AttributeValueTypes.avtBoolean, , , , "tblControlImpactModerate")
                    fValueChanged = True
                End If
                Dim attrHighList = AttributesList.Where(Function(attr) attr.Name.Trim.ToLower = "impact: high" AndAlso attr.ValueType = AttributeValueTypes.avtBoolean)
                HasAttributeImpactHigh = attrHighList.Count > 0
                If HasAttributeImpactHigh Then
                    AttributeImpactHigh = attrHighList(0)
                    AttributeImpactHighID = attrHighList(0).ID
                Else
                    AttributeImpactHighID = Guid.NewGuid
                    AttributeImpactHigh = PM.Attributes.AddAttribute(AttributeImpactHighID, "Impact: High", AttributeTypes.atControl, AttributeValueTypes.avtBoolean, , , , "tblControlImpactHigh")
                    fValueChanged = True
                End If

                For Each xControlNode As XmlNode In xControlsList
                    CreateControlFromXMLNode(xControlNode, fLoadEnhancementsAsControls, fValueChanged, fHasInfodocs)
                Next

                If fHasInfodocs Then PM.StorageManager.Writer.SaveInfoDocs() ' D4371
            End If

        End If

        If fValueChanged Then
            WriteControls(PRJ)
            WriteAttributes(PRJ)
            WriteAttributeValues(PRJ, ParseString(PagePrefix + "Import %%controls%% from clipboard"), "Import from XML")
        End If
    End Sub

    Private Sub CreateControlFromXMLNode(xControlNode As XmlNode, fLoadEnhancementsAsControls As Boolean, ByRef fValueChanged As Boolean, ByRef fHasInfodocs As Boolean, Optional ByVal ControlFamily As String = "")
        If (xControlNode.Name = "controls:control" OrElse xControlNode.Name = "control-enhancement") AndAlso xControlNode.HasChildNodes Then
            Dim sControlName As String = "" ' number + " " + title
            Dim sInfodoc As String = ""
            Dim sControlFamily As String = "" ' <family></family>
            Dim sControlPriority As String = "" ' <priority></priority>
            Dim sControlImpact As List(Of String) = New List(Of String) ' <baseline-impact></baseline-impact>

            For Each NodeProp As XmlNode In xControlNode.ChildNodes
                ' D4371 ===
                Dim sTag As String = NodeProp.Name.ToLower
                If sTag.ToLower = "number" Then sControlName = NodeProp.InnerText.Trim + " " + sControlName
                If sTag.ToLower = "title" Then sControlName = sControlName + NodeProp.InnerText.Trim
                If sTag.ToLower = "family" Then sControlFamily = NodeProp.InnerText.Trim
                If sTag.ToLower = "priority" Then
                    sControlPriority = NodeProp.InnerText.Trim
                    sInfodoc = "<b>Priority</b>: " + SafeFormString(sControlPriority) + vbNewLine
                End If
                If sTag.ToLower = "baseline-impact" Then
                    sControlImpact.Add(NodeProp.InnerText.Trim.ToUpper)
                    sInfodoc += "<b>Impact</b>: " + SafeFormString(NodeProp.InnerText.Trim) + vbNewLine
                End If

                ' put all extra info into an infodoc
                If Not sTagsCommon.Contains(NodeProp.Name.ToLower) OrElse (Not fLoadEnhancementsAsControls AndAlso NodeProp.Name.ToLower = "control-enhancements") Then
                    sInfodoc += If(sInfodoc = "", "", vbNewLine + "<hr>")
                    Dim sData As String = LoadAllTextFromXmlNode(NodeProp)
                    Dim sTitle = ""
                    For i As Integer = 0 To sTagsExtra.Count - 1
                        If sTagsExtra(i) = sTag Then sTitle = sTagsTitle(i)
                    Next
                    If sTitle = "" Then sTitle = NodeProp.Name.ToUpper.Replace("-", " ").Replace("_", " ").Trim
                    If sData <> "" Then
                        sInfodoc += String.Format("<h4>{0}:</h4><p>{1}</p>" + vbNewLine, sTitle, sData)
                    Else
                        sInfodoc += String.Format("<h5>{0}</h5>", sTitle)
                    End If
                End If
            Next

            Dim sMHT As String = ""
            If sInfodoc <> "" Then
                sMHT = ParseTextHyperlinks(sInfodoc.Trim(CChar(vbNewLine)))
                sMHT = sMHT.Trim(CChar(vbNewLine)).Replace(vbNewLine + vbNewLine, vbNewLine).Replace(vbNewLine + "<p>", "<p>").Replace(vbNewLine + "<hr>", "<hr>").Trim(CChar(vbNewLine))
                sInfodoc = HTML2Text(sInfodoc).Trim(CChar(vbNewLine))
                sMHT = String.Format(_TEMPL_EMPTY_INFODOC, sMHT.Replace(vbNewLine, "<br>" + vbNewLine), SafeFormString(sControlName), ThemePath)    ' D6504
                sMHT = Infodoc_Pack(sMHT, "", "")
                fHasInfodocs = True
            End If
            ' D4371 ==

            Dim newControl As clsControl = Nothing

            If sControlName <> "" Then
                Dim existingControl As clsControl = PM.Controls.GetControlByName(sControlName)
                If existingControl IsNot Nothing AndAlso existingControl.Type = ControlType.ctUndefined Then
                    newControl = existingControl
                    newControl.InfoDoc = sInfodoc
                Else
                    newControl = PM.Controls.AddControl(Guid.NewGuid(), sControlName, ControlType.ctUndefined, sInfodoc)
                End If
                fValueChanged = True
            End If

            ' D4371 ===
            If fValueChanged AndAlso sMHT <> "" AndAlso newControl IsNot Nothing Then
                PM.InfoDocs.SetCustomInfoDoc(sMHT, newControl.ID, Guid.Empty)
            End If
            ' D4371 ==

            If sControlFamily = "" Then sControlFamily = ControlFamily

            If fValueChanged AndAlso sControlFamily <> "" Then
                Dim existingCategory As clsAttributeEnumerationItem = GetCategoryByName(sControlFamily)
                Dim catID As Guid = Guid.Empty
                If existingCategory Is Nothing Then
                    catID = Guid.NewGuid()
                    AddEnumAttributeItem(ATTRIBUTE_CONTROL_CATEGORY_ID, AttributeTypes.atControl, catID, sControlFamily, False)
                Else
                    catID = existingCategory.ID
                End If
                newControl.Categories = catID.ToString
            End If

            If fValueChanged AndAlso sControlFamily <> "" Then
                Dim aEnum As clsAttributeEnumeration
                If AttributeFamily.EnumID <> Guid.Empty Then
                    aEnum = PM.Attributes.GetEnumByID(AttributeFamily.EnumID)
                Else
                    aEnum = New clsAttributeEnumeration
                    aEnum.ID = Guid.NewGuid
                    aEnum.Name = AttributeFamily.Name
                    aEnum.Items = New List(Of clsAttributeEnumerationItem)
                    AttributeFamily.EnumID = aEnum.ID
                    PM.Attributes.Enumerations.Add(aEnum)
                End If
                Dim existingValue As clsAttributeEnumerationItem = aEnum.GetItemByValue(sControlFamily)
                Dim catID As Guid = Guid.Empty
                If existingValue Is Nothing Then
                    Dim aEnumItem = aEnum.AddItem(sControlFamily)
                    aEnumItem.ID = Guid.NewGuid
                    catID = aEnumItem.ID
                Else
                    catID = existingValue.ID
                End If
                PM.Attributes.SetAttributeValue(AttributeFamilyID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, catID, newControl.ID, Guid.Empty)
            End If

            If fValueChanged AndAlso sControlPriority <> "" Then
                Dim aEnum As clsAttributeEnumeration
                If AttributePriority.EnumID <> Guid.Empty Then
                    aEnum = PM.Attributes.GetEnumByID(AttributePriority.EnumID)
                Else
                    aEnum = New clsAttributeEnumeration
                    aEnum.ID = Guid.NewGuid
                    aEnum.Name = AttributePriority.Name
                    aEnum.Items = New List(Of clsAttributeEnumerationItem)
                    AttributePriority.EnumID = aEnum.ID
                    PM.Attributes.Enumerations.Add(aEnum)
                End If
                Dim existingValue As clsAttributeEnumerationItem = aEnum.GetItemByValue(sControlPriority)
                Dim catID As Guid = Guid.Empty
                If existingValue Is Nothing Then
                    Dim aEnumItem = aEnum.AddItem(sControlPriority)
                    aEnumItem.ID = Guid.NewGuid
                    catID = aEnumItem.ID
                Else
                    catID = existingValue.ID
                End If
                PM.Attributes.SetAttributeValue(AttributePriorityID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, catID, newControl.ID, Guid.Empty)
            End If

            If fValueChanged AndAlso sControlImpact.Count > 0 Then
                If sControlImpact.Contains("LOW") Then PM.Attributes.SetAttributeValue(AttributeImpactLowID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, True, newControl.ID, Guid.Empty)
                If sControlImpact.Contains("MODERATE") Then PM.Attributes.SetAttributeValue(AttributeImpactModerateID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, True, newControl.ID, Guid.Empty)
                If sControlImpact.Contains("HIGH") Then PM.Attributes.SetAttributeValue(AttributeImpactHighID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, True, newControl.ID, Guid.Empty)
            End If

            'A1364 === pull out control enahcements and make the controls out of them
            If fLoadEnhancementsAsControls Then
                For Each NodeProp As XmlNode In xControlNode.ChildNodes
                    Dim sTag As String = NodeProp.Name.ToLower
                    If sTag.ToLower = "control-enhancements" AndAlso NodeProp.HasChildNodes Then
                        For Each CtrlEnhancementNode As XmlNode In NodeProp.ChildNodes
                            CreateControlFromXMLNode(CtrlEnhancementNode, fLoadEnhancementsAsControls, fValueChanged, fHasInfodocs, sControlFamily)
                        Next
                    End If
                Next
            End If
            'A1364 ==
        End If
    End Sub

    Private Function LoadAllTextFromXmlNode(ByVal Node As XmlNode) As String
        Dim sResult As String = ""
        If Node IsNot Nothing Then
            ' read attributes
            If Node.Attributes IsNot Nothing AndAlso Node.Attributes.Count > 0 Then
                Dim sAttrs As String = ""
                For Each Attr As XmlAttribute In Node.Attributes
                    sAttrs += If(sAttrs = "", "", ", ") + Attr.InnerText
                Next
                If sAttrs <> "" Then sResult = String.Format("<b><i>{0}</i></b>" + vbNewLine, SafeFormString(sAttrs))
            End If

            If Node.HasChildNodes Then

                Dim SubsCnt As New Dictionary(Of String, Integer)
                For Each Child As XmlNode In Node.ChildNodes
                    If SubsCnt.ContainsKey(Child.Name) Then SubsCnt(Child.Name) += 1 Else SubsCnt.Add(Child.Name, If(Child.Name = Node.Name, 2, 1))
                Next

                Dim SubsVal As New Dictionary(Of String, String)
                For Each Child As XmlNode In Node.ChildNodes
                    Dim sChild As String = LoadAllTextFromXmlNode(Child)
                    If sChild <> "" Then
                        If SubsCnt(Child.Name) > 1 Then sChild = "<li>" + sChild
                        If Child.Name = "number" Then sChild = String.Format("<i>{0}</i>", sChild)
                        If SubsVal.ContainsKey(Child.Name) Then
                            SubsVal(Child.Name) += sChild
                        Else
                            SubsVal.Add(Child.Name, sChild)
                        End If
                    End If
                Next

                Dim sNames As String = ""
                Dim sLists As String = ""
                For Each sTag As KeyValuePair(Of String, String) In SubsVal
                    If SubsCnt(sTag.Key) = 1 Then
                        sNames += String.Format("{0}{1}", If(sNames = "", "", If(sNames.Contains("<li>") OrElse sTag.Value.Contains("<li>"), "", ", ")), sTag.Value)
                    Else
                        sLists += String.Format("<ul>{0}</ul>", sTag.Value)
                    End If
                Next
                sResult += sNames + sLists

            Else
                ' read text content
                sResult += SafeFormString(Node.InnerText)
            End If

        End If
        Return sResult
    End Function

#End Region

#Region "Hierarchy Tree View"

    Public ReadOnly Property WRTNode As clsNode
        Get
            Return SelectedHierarchyNode
        End Get
    End Property

    Public ReadOnly Property WRTNodeName As String
        Get
            Dim retVal As String = ""
            If CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES Or CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS Then
                Dim node As clsNode = WRTNode
                If node IsNot Nothing Then
                    retVal = String.Format("from {0} ""{1}""", If(node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood, ParseString("%%Objective(l)%%"), ParseString("%%Objective(i)%%")), node.NodeName)
                End If
            End If
            Return retVal
        End Get
    End Property

    Public PagesWrtObjective As Integer() = {_PGID_RISK_OPTIMIZER_FROM_SOURCES, _PGID_RISK_OPTIMIZER_TO_OBJS}

    Public ReadOnly Property SelectedHierarchyNode As clsNode
        Get
            Dim tGuid As Guid = SelectedHierarchyNodeID
            Dim retVal As clsNode = Nothing
            If tGuid <> Guid.Empty Then
                retVal = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tGuid)
                If retVal Is Nothing Then retVal = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(tGuid)
            End If
            Return retVal
        End Get
    End Property

    Public Property SelectedHierarchyNodeID As Guid
        Get
            If PagesWrtObjective.Contains(CurrentPageID) Then
                Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)

                Dim retVal As Guid = Guid.Empty
                Dim s As String = CStr(PM.Attributes.GetAttributeValue(If(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), UNDEFINED_USER_ID))
                If Not String.IsNullOrEmpty(s) AndAlso s.Length > 16 Then retVal = New Guid(s)

                If PM.Hierarchy(HierarchyID).GetNodeByID(retVal) Is Nothing Then retVal = PM.Hierarchy(HierarchyID).Nodes(0).NodeGuidID
                Return retVal
            Else
                Return PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID
            End If
        End Get
        Set(value As Guid)
            Dim HierarchyID As ECHierarchyID = CType(PM.ActiveHierarchy, ECHierarchyID)
            If PagesWrtObjective.Contains(CurrentPageID) Then
                HierarchyID = If(CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
            End If
            WriteSetting(PRJ, CType(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), Guid), AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    'todo use a get_hierarchy_data function from api
    Public Function GetHierarchyData() As String
        If Not PagesWrtObjective.Contains(CurrentPageID) Then Return "[]"

        Dim retVal As String = ""

        Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)

        Dim H As clsHierarchy = PM.Hierarchy(HierarchyID)
        For Each nodeTuple As Tuple(Of Integer, Integer, clsNode) In H.NodesInLinearOrder
            Dim node As clsNode = nodeTuple.Item3
            retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""guid"":""{1}"",""name"":""{2}"",""pguid"":""{3}""", node.NodeID, node.NodeGuidID, JS_SafeString(node.NodeName), If(node.ParentNode Is Nothing, "", node.ParentNode.NodeGuidID.ToString))
            retVal += String.Format(",""info"":""{0}""", JS_SafeString(Infodoc2Text(PRJ, node.InfoDoc, True)))

            retVal += "}"
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetHierarchyColumns() As String
        If Not PagesWrtObjective.Contains(CurrentPageID) Then Return "[]"

        Dim retVal As String = ""

        Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)

        retVal = String.Format("[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 0, "ID", Bool2JS(False), "", "id", 0, "false", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 1, ParseString(If(HierarchyID = ECHierarchyID.hidLikelihood, "%%Objective(l)%% Name", "%%Objective(i)%% Name")), Bool2JS(True), "", "name", 0, "true", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 2, ParseString("Description"), Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs), "", "info", 0, "false", "false")

        Return String.Format("[{0}]", retVal)
    End Function

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

                If aEnum.GetItemByValue(ItemName) Is Nothing Then
                    Dim eItem As clsAttributeEnumerationItem = aEnum.AddItem(ItemName)
                    eItem.ID = ItemID
                End If

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

    Private Sub RiskSelectTreatmentsPage_PreRenderComplete(sender As Object, e As EventArgs) Handles Me.PreRenderComplete
        If CheckVar(_PARAM_ACTION, "") = "export" Then
            DebugInfo("Start export")
            If CheckVar("type", "logs").Trim.ToLower = "logs" Then
                ExportXALogs()
            End If
        End If
    End Sub

    'Private Sub ActivateAllControls() 'A1232
    '    Dim res As Boolean = PM.Controls.SetControlsActive(ControlType.ctCause, True) And PM.Controls.SetControlsActive(ControlType.ctCauseToEvent, True) And PM.Controls.SetControlsActive(ControlType.ctConsequenceToEvent, True)
    '    If res Then WriteControls(PRJ, "")
    'End Sub

    ' D3488 ===
    Protected Sub ExportXALogs()
        '-A1220 If RA_Solver = raSolverLibrary.raXA Then    ' D3877

        Dim sTmpFolder As String = File_CreateTempName()
        Dim sArcName As String = GetProjectFileName(String.Format("{0} - {1} (Solver Logs)", ShortString(App.ActiveProject.ProjectName, 45, True), ResString("%%Controls%%")), App.ActiveProject.ProjectName + " (Solver Logs)", "Solver logs", "zip").Replace("�", "_")

        If File_CreateFolder(sTmpFolder) Then
            sTmpFolder += "\"
            Dim sFullName As String = sTmpFolder + sArcName
            Dim tFundedCost As Double = 0
            Dim tOptimizedRisk As Double = 0

            'ActivateAllControls() 'A1232

            OptimizeTreatments(tFundedCost, tOptimizedRisk, sTmpFolder)

            'RawResponseStart()

            Dim sError As String = ""
            Dim sContent As String = ""
            Try
                Dim Lst As New StringArray

                '-A1220 If RA.Solver.SolverState = raSolverState.raError Then sError = String.Format("(!) Solver internal RTE: " + SolverStateHTML(RA.Solver)) ' D3628

                If sError = "" Then
                    Dim AllFiles As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(sTmpFolder)
                    DebugInfo("Scan for files�")
                    For Each sFileName As String In AllFiles
                        Lst.Append(sFileName)
                    Next

                    If Lst.Count > 0 Then
                        If PackZipFiles(Lst, sFullName, sError) Then
                            DownloadFile(sFullName, "application/zip", sArcName, dbObjectType.einfFile, App.ProjectID,, False)  ' D6593
                            'Response.ContentType = "application/zip"
                            'Response.AddHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", HttpUtility.UrlEncode(SafeFileName(sArcName))))	' D6591
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
                    'Response.ContentType = "text/plain"
                    sContent = sError
                End If

            Catch ex As Exception
                'Response.ContentType = "text/plain"
                'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Unable to create Solver logs file", ex.Message)
                sContent = "(!) Unable to create Solver logs due to an error. "
            End Try

            File_DeleteFolder(sTmpFolder)
            'If sContent <> "" Then Response.Write(sContent)
            If sContent <> "" Then DownloadContent(sContent, "text/plain", sArcName, dbObjectType.einfFile, App.ProjectID,, False)   ' D6593
        End If
        '-A1220 End If
    End Sub
    ' D3488 ==


    ' D4075 ===
    Protected Sub DownloadBaronFiles()
        Dim sTmpFolder As String = File_CreateTempName()
        Dim sArcName As String = GetProjectFileName(String.Format("{0} (Solver files)", ShortString(App.ActiveProject.ProjectName, 65, True)), App.ActiveProject.ProjectName + " (Solver Files)", "Solver Files", "zip").Replace("�", "_")

        If File_CreateFolder(sTmpFolder) Then

            Dim sFullName As String = sTmpFolder + "\" + sArcName

            Dim tFundedCost As Double = 0
            Dim tOptimizedRisk As Double = 0

            Baron_Custom_OutputFolder = sTmpFolder

            'ActivateAllControls() 'A1232

            OptimizeTreatments(tFundedCost, tOptimizedRisk, sTmpFolder)
            Baron_Custom_OutputFolder = ""

            'RawResponseStart()

            Dim sError As String = ""
            Dim sContent As String = ""
            Try
                Dim Lst As New StringArray

                Dim AllFiles As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(sTmpFolder)
                DebugInfo("Scan for files�")
                For Each sFileName As String In AllFiles
                    Lst.Append(sFileName)
                Next

                If Lst.Count > 0 Then
                    If PackZipFiles(Lst, sFullName, sError) Then
                        DownloadFile(sFullName, "application/zip", sArcName, dbObjectType.einfFile, App.ProjectID,, False)   ' D6593
                        'Response.ContentType = "application/zip"
                        'Response.AddHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", SafeFileName(sArcName)))
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

                App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Download Solver files", sError)
                If sError <> "" Then
                    'Response.ContentType = "text/plain"
                    sContent = sError
                End If

            Catch ex As Exception
                'Response.ContentType = "text/plain"
                'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfFile, App.ProjectID, "RA: Unable to create Solver logs file", ex.Message)
                sContent = "(!) Unable to create Solver logs due to an error. "
            End Try

            File_DeleteFolder(sTmpFolder)
            'If sContent <> "" Then Response.Write(sContent)
            If sContent <> "" Then DownloadContent(sContent, "text/plain", sArcName, dbObjectType.einfFile, App.ProjectID,, False)   ' D6593
        End If
    End Sub
    ' D4075 ==

    Private Shared Function ValidateCertificate(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) As Boolean
        Return True
    End Function

#Region "Risk Time Periods"

    Dim AltColors As String() = {"#95c5f0", "#fa7000", "#9d27a8", "#e3e112", "#00687d", "#407000", "#f24961", "#663d2e", "#9600fa", "#ffbde6", "#00c49f", "#7280c4", "#009180", "#e33000", "#80bdff", "#a10040", "#0affe3", "#00523c", "#919100", "#5c00f7", "#a15f00", "#cce6ff", "#00465c", "#adff69", "#f24ba0", "#0dff87", "#ff8c47", "#349400", "#b3b3a1", "#a10067", "#ba544a", "#edc2d1", "#00e8c3", "#3f0073", "#5ec1f7", "#6e00b8", "#f5f5c4", "#e33000", "#52ba00", "#ff943b", "#0079db", "#f0e6c0", "#ffb517", "#cf0076", "#e8cfc9"}

    Public ReadOnly Property Scenario As RAScenario
        Get
            Return RA.Scenarios.ActiveScenario
        End Get
    End Property

    Private SESS_RA_SHOW_TP As String = "RA_ShowTimeperiods_{0}"

    Public Property RA_ShowTimeperiods As Boolean
        Get
            If CurrentPageID = _PGID_RISK_OPTIMIZER_TIME_PERIODS_VIEW Then Return True
            If RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count = 0 OrElse Not (CurrentPageID = _PGID_RISK_OPTIMIZER Or CurrentPageID = _PGID_RISK_OPTIMIZER_OVERALL Or CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES Or CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS) Then Return False
            If Session(String.Format(SESS_RA_SHOW_TP, App.ProjectID)) IsNot Nothing Then Return CBool(Session(String.Format(SESS_RA_SHOW_TP, App.ProjectID))) Else Return False
        End Get
        Set(value As Boolean)
            Session(String.Format(SESS_RA_SHOW_TP, App.ProjectID)) = value
        End Set
    End Property

    Public Const SESS_RA_SHOWFUNDEDONLY As String = "RA_ShowFundedOnly"

    Public Property RA_ShowFundedOnly As Boolean
        Get
            If Session(SESS_RA_SHOWFUNDEDONLY) Is Nothing Then Return GetCookie(SESS_RA_SHOWFUNDEDONLY, False.ToString) = True.ToString Else Return CBool(Session(SESS_RA_SHOWFUNDEDONLY)) ' D2893
        End Get
        Set(value As Boolean)
            Session(SESS_RA_SHOWFUNDEDONLY) = value
            SetCookie(SESS_RA_SHOWFUNDEDONLY, value.ToString)    ' D2893
        End Set
    End Property

    Friend Property RA_ShowCents As Boolean
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

    Private Const SESS_TP_SELECTED_RESOURCE As String = "TP_ResID_{0}"

    Public Property TP_RES_ID As String
        Get
            If Session(String.Format(SESS_TP_SELECTED_RESOURCE, App.ProjectID)) Is Nothing Then
                If Scenario IsNot Nothing AndAlso Scenario.TimePeriods IsNot Nothing AndAlso Scenario.TimePeriods.EnabledResources(0) IsNot Nothing Then
                    Return JS_SafeString(Scenario.TimePeriods.EnabledResources(0).ID.ToString)
                Else
                    Return "0"
                End If
            Else
                Return CStr(Session(String.Format(SESS_TP_SELECTED_RESOURCE, App.ProjectID)))
            End If
        End Get
        Set(value As String)
            Session(String.Format(SESS_TP_SELECTED_RESOURCE, App.ProjectID)) = value
        End Set
    End Property

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
                    valuesString += CStr(IIf(valuesString <> "", ",", "")) + JS_SafeNumber(resVal)
                Next
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{idx: {0}, id:'{1}', name:'{2}', values: [{3}], totalValue: {4}}}", res.ConstraintID, JS_SafeString(res.ID.ToString), JS_SafeString(res.Name), valuesString, JS_SafeNumber(totalCost))  ' D3918
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
            'DoSort(tAlts)  ' D4417
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
                    retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{resID:'{0}', periodID:{1}, minVal:{2}, maxVal:{3}}}", JS_SafeString(res.ID.ToString), Scenario.TimePeriods.Periods.IndexOf(tp), JS_SafeNumber(resMinVal), JS_SafeNumber(resMaxVal))  ' D3918
                Next
            Next
        End If
        retVal = String.Format("[{0}]", retVal)
        Return retVal
    End Function

    Public Function GetPeriodResults() As String
        Dim sPeriodResults As String = ""
        If Scenario IsNot Nothing Then
            'If RA.Scenarios.GlobalSettings.isAutoSolve AndAlso SolveTime.TotalMilliseconds < 10 Then Solve() ' D3900 + D3969 + D4120 --disable due to second Solve, which in not expected and not required
            Dim ATPResults = Scenario.TimePeriods.TimePeriodResults
            For Each item In ATPResults
                sPeriodResults += CStr(IIf(sPeriodResults <> "", ",", "")) + String.Format("{{projID:'{0}', start:{1}}}", JS_SafeString(item.Key), item.Value)
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
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{id:'{0}', name:'{1}'}}", JS_SafeString(res.ID.ToString), JS_SafeString(res.Name))    ' D3918
            Next
        End If
        Return "[" + retVal + "]"
    End Function

#End Region


End Class