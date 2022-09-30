Imports Telerik.Web.UI
Imports Canvas.RAGlobalSettings

Partial Class RACustomConstraints
    Inherits clsComparionCorePage

    Private _CatAttributesList As List(Of clsAttribute) = Nothing  ' D3097 + D3248
    Private _NumAttributesList As List(Of clsAttribute) = Nothing  ' D3248

    Public Sub New()
        MyBase.New(_PGID_RA_CUSTOM_CONSTRAINTS)
    End Sub

    ' D2839 ===
    ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property
    '' D2839 ==

    ' D3665 ===
    Public Function isTimePeriodsAvailable() As Boolean
        Return (ShowDraftPages() OrElse Not isDraftPage(_PGID_RA_TIMEPERIODS_SETTINGS)) AndAlso Not RA_OPT_FORCE_CC_USE_IN_TIMEPERIODS    ' D3826
    End Function
    ' D3665 ==

    ' D3096 ===
    Public ReadOnly Property CatAttributesList() As List(Of clsAttribute)   ' D3248
        Get
            If _CatAttributesList Is Nothing Then
                With App.ActiveProject.ProjectManager
                    _CatAttributesList = New List(Of clsAttribute)
                    .Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                    If .Attributes IsNot Nothing AndAlso .Attributes.AttributesList IsNot Nothing AndAlso .Attributes.AttributesList.Count > 0 Then
                        .Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, -1)
                        For Each attr In If(App.isRiskEnabled, .Attributes.GetControlsAttributes, .Attributes.GetAlternativesAttributes) ' D6049
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
    ' D3096 ==

    ' D3248 ===
    Public ReadOnly Property NumAttributesList() As List(Of clsAttribute)
        Get
            If _NumAttributesList Is Nothing Then
                With App.ActiveProject.ProjectManager
                    _NumAttributesList = New List(Of clsAttribute)
                    .Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                    If .Attributes IsNot Nothing AndAlso .Attributes.AttributesList IsNot Nothing AndAlso .Attributes.AttributesList.Count > 0 Then
                        .Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, -1)
                        For Each attr In If(App.isRiskEnabled, .Attributes.GetControlsAttributes, .Attributes.GetAlternativesAttributes)    ' D6049
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
    ' D3248 ==

    ' D3122 ===
    Protected Sub Page_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        'A1389 ===
        If App.isRiskEnabled Then CurrentPageID = _PGID_RISK_OPTIMIZER_CUSTOM_CONSTRAINTS
        'A1389 ==
    End Sub
    ' D3122 ==


    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If Not IsPostBack AndAlso Not isCallback Then
            AlignVerticalCenter = False
            '        isSorting = True    ' D2982
            'pnlLoadingPanel.Caption = ResString("msgLoading")
            'pnlLoadingPanel.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")
            'RadToolBarMain.Items(0).Text = ResString("lblScenario") + ":"
            ' D3262 ===
            With CType(RadToolBarMain.Items(3), RadToolBarDropDown)
                .Text = ResString("btnRACCImport")
                .Buttons(0).Text = ResString("btnRACreateCCFromCatAttrib")
                .Buttons(1).Text = ResString("btnRACreateCCFromNumAttrib")
            End With
            RadToolBarMain.Items(4).Text = ResString("btnRASyncSelCC") ' D3343 + D3346
            RadToolBarMain.Items(6).Text = ResString("btnRADeleteCC")
            RadToolBarMain.Items(7).Text = ResString("btnRADeleteAllCC")
            ' D3262 ==
        End If
        If isAJAX() Then Ajax_Callback() ' D3112
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower   ' Anti-XSS
        Select Case sAction
            Case "scenario"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    RA.Scenarios.ActiveScenarioID = ID
                    RA.Solver.ResetSolver()
                End If
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
        End Select
        ' D2876 ==
    End Sub

    ' D2875 ===
    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tID As Integer In RA.Scenarios.Scenarios.Keys
            sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("[{0},'{1}','{2}',{3}]", tID, JS_SafeString(RA.Scenarios.Scenarios(tID).Name), JS_SafeString(RA.Scenarios.Scenarios(tID).Description), RA.Scenarios.Scenarios(tID).Index)
        Next
        Return sRes
    End Function
    ' D2875 ==

    ' D3097 ===
    Public Function GetAttributes(fGetEnum As Boolean) As String
        Dim sRes As String = ""
        ' D3111 ===
        Dim CCList As New List(Of String)
        For Each tID As Integer In RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys
            CCList.Add(RA.Scenarios.ActiveScenario.Constraints.Constraints(tID).Name.ToLower.ToLower)
        Next
        ' D3248 ===
        Dim tAttribs As List(Of clsAttribute) = Nothing
        If fGetEnum Then tAttribs = CatAttributesList Else tAttribs = NumAttributesList

        ' D3284 ===
        Dim CCAttribs As New List(Of Guid)
        For Each tCCKey As Integer In RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys
            Dim G As Guid = RA.Scenarios.ActiveScenario.Constraints.Constraints(tCCKey).LinkedAttributeID
            If Not G.Equals(Guid.Empty) AndAlso Not CCAttribs.Contains(G) Then CCAttribs.Add(G)
        Next
        ' D3284 ==

        For Each tAttr As clsAttribute In tAttribs.Where(Function(a) Not a.IsDefault).ToList    ' D6049
            ' -D3284 
            ' D3248 ==
            'Dim fCCExists As Integer = 0
            'Dim sItems As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(tAttr.EnumID)
            'If sItems IsNot Nothing Then 'A0899
            '    For Each tItem As clsAttributeEnumerationItem In sItems.Items
            '        If CCList.Contains(tItem.Value.ToLower) Then fCCExists += 1
            '    Next
            'End If
            sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("['{0}','{1}',{2},{3}]", tAttr.ID, JS_SafeString(SafeFormString(tAttr.Name.Trim)), CInt(tAttr.ValueType), IIf(CCAttribs.Contains(tAttr.ID), 1, 0))  ' D3284
            ' D3110 ==
        Next
        Return sRes
    End Function
    ' D3097 ==

    ' D2881 ===
    Public Function GetConstraints() As String
        Dim sRes As String = ""
        Dim sCCBrokenList As String = "" ' D3336
        For Each tID As Integer In RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys
            Dim CC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.Constraints(tID)   ' D3285
            Dim tAttr As clsAttribute = Nothing
            ' D3285 ===
            Dim sAtrInfo As String = ""
            Dim fLinkOK As Boolean = False  ' D3336
            If Not CC.LinkedAttributeID.Equals(Guid.Empty) Then
                tAttr = CatAttributesList.Find(Function(x) x.ID = CC.LinkedAttributeID)
                If tAttr Is Nothing Then
                    tAttr = NumAttributesList.Find(Function(x) x.ID = CC.LinkedAttributeID)
                    If tAttr IsNot Nothing Then
                        sAtrInfo = String.Format(ResString("lblRALinkedNumAtr"), tAttr.Name)
                        fLinkOK = True  ' D3336
                    End If
                Else
                    sAtrInfo = String.Format(ResString("lblRALinkedCatAtr"), tAttr.Name)
                    Dim sItems As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(tAttr.EnumID)
                    If sItems IsNot Nothing Then
                        For Each tItem As clsAttributeEnumerationItem In sItems.Items
                            If tItem.ID.Equals(CC.LinkedEnumID) Then
                                sAtrInfo = String.Format(ResString("lblRALinkedCatEnum"), tAttr.Name, tItem.Value)
                                fLinkOK = True  ' D3336
                            End If
                        Next
                    End If
                End If
                'If sAtrInfo <> "" AndAlso CC.IsReadOnly Then sAtrInfo = String.Format("{0} [{1}]", sAtrInfo, ResString("lblRACCReadOnly")) ' -D3402
            End If
            sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("[{0},'{1}',{2},{3},'{4}','{5}','{6}',{7}]", tID, JS_SafeString(CC.Name.Trim), IIf(CC.Enabled, 1, 0), IIf(CC.IsLinked, IIf(fLinkOK, IIf(CC.IsReadOnly, 2, 1), -1), 0), JS_SafeString(SafeFormString(sAtrInfo)), JS_SafeString(CC.LinkedAttributeID.ToString), JS_SafeString(CC.LinkedEnumID.ToString), Bool2JS(CC.IsLinkedToResource)) ' D3336 + D3340 + D3615 + D3745
            If CC.IsLinked AndAlso Not fLinkOK Then sCCBrokenList += CStr(IIf(sCCBrokenList = "", "", ", ")) + CC.Name ' D3336 
            ' D3285 ==
        Next
        ' D3336 ===
        If sCCBrokenList <> "" AndAlso Not isCallback AndAlso Not IsPostBack Then
            Dim sSess As String = SessVar("FixCCLinks" + App.ProjectID.ToString)
            If String.IsNullOrEmpty(sSess) Then sSess = "0"
            If sSess < CStr(Date2ULong(Now)) Then
                ClientScript.RegisterStartupScript(GetType(String), "AskRemoveLinks", String.Format("setTimeout('onFixBrokenLinks(""{0}"");', 300);", JS_SafeString(String.Format(ResString("msgRAFixCCBrokenLinks"), sCCBrokenList))), True)
                SessVar("FixCCLinks" + App.ProjectID.ToString) = CStr(Date2ULong(Now.AddMinutes(10)))
            End If
        End If
        ' D3336 ==
        Return sRes
    End Function
    ' D2881 ==

    ' D3336 ===
    Public Sub FixBrokenLinks()
        Dim fHasChanges As Boolean = False  ' D3790
        For Each tID As Integer In RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys
            Dim CC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.Constraints(tID)
            Dim tAttr As clsAttribute = Nothing
            If Not CC.LinkedAttributeID.Equals(Guid.Empty) Then
                tAttr = CatAttributesList.Find(Function(x) x.ID = CC.LinkedAttributeID)
                If tAttr Is Nothing Then
                    tAttr = NumAttributesList.Find(Function(x) x.ID = CC.LinkedAttributeID)
                    If tAttr Is Nothing Then
                        CC.LinkedAttributeID = Guid.Empty
                        CC.LinkedEnumID = Guid.Empty
                        fHasChanges = True  ' D3790
                    End If
                Else
                    Dim sItems As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(tAttr.EnumID)
                    Dim fHasEnum As Boolean = False
                    If sItems IsNot Nothing Then
                        For Each tItem As clsAttributeEnumerationItem In sItems.Items
                            If tItem.ID.Equals(CC.LinkedEnumID) Then fHasEnum = True
                        Next
                    End If
                    If Not fHasEnum Then
                        CC.LinkedAttributeID = Guid.Empty
                        CC.LinkedEnumID = Guid.Empty
                        fHasChanges = True  ' D3790
                    End If
                End If
            End If
        Next
        If fHasChanges Then App.ActiveProject.SaveRA("Edit Custom constraints", , , "Remove links to missing attributes") ' D3790
    End Sub
    ' D3336 ==

    ' D3109 ===
    Private Sub CreateCustConstraintsFromCatAttribute(tAttr As clsAttribute, Ids As List(Of Guid), fIsRO As Boolean, isDollarValues As Boolean)  ' D3281 + D3338 + D6464
        If tAttr IsNot Nothing Then
            Dim sItems As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(tAttr.EnumID)
            If sItems IsNot Nothing Then

                Dim CCList As New Dictionary(Of String, RAConstraint)

                For Each tItem As clsAttributeEnumerationItem In sItems.Items
                    If Ids.Count = 0 OrElse Ids.Contains(tItem.ID) Then ' D3281
                        ' D3143 ===
                        'Dim tNewCC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.AddConstraint(tItem.Value) 'AS/11-18-15
                        Dim tNewCC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.AddConstraint("-1", tItem.Value) 'AS/11-18-15
                        tNewCC.LinkedAttributeID = tAttr.ID
                        tNewCC.LinkedEnumID = tItem.ID
                        tNewCC.IsReadOnly = fIsRO   ' D3338
                        CCList.Add(tItem.ID.ToString, tNewCC)
                        ' D3143 ==
                    End If
                Next

                ' D6049 ===
                Dim tLst As IEnumerable(Of Object)
                If App.isRiskEnabled Then tLst = App.ActiveProject.ProjectManager.Controls.EnabledControls Else tLst = App.ActiveProject.HierarchyAlternatives.TerminalNodes
                For Each tNodeAlt As Object In tLst
                    Dim NodeGUID As Guid = If(App.isRiskEnabled, CType(tNodeAlt, clsControl).ID, CType(tNodeAlt, clsNode).NodeGuidID)  ' D3120
                    Dim tAlt As RAAlternative = RA.Scenarios.ActiveScenario.Alternatives.Find(Function(a) (a.ID.ToLower = NodeGUID.ToString.ToLower))   ' D3120
                    If tAlt IsNot Nothing Then
                        Dim objAttrValue As Object = App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(tAttr.ID, NodeGUID)
                        ' D6049 ==
                        If objAttrValue Is Nothing AndAlso tAttr.DefaultValue IsNot Nothing Then objAttrValue = tAttr.DefaultValue ' D3379
                        Dim tAttributeValue As String = ""
                        If TypeOf (objAttrValue) Is Guid Then tAttributeValue = objAttrValue.ToString Else tAttributeValue = CStr(objAttrValue)

                        If Not String.IsNullOrEmpty(tAttributeValue) Then
                            tAttributeValue = (tAttributeValue + ";").Replace(";;", ";").Trim(CType(";", Char()))

                            Dim tParams() As String = tAttributeValue.Split(CChar(";"))
                            For Each sParam As String In tParams
                                If sParam <> "" Then
                                    If CCList.ContainsKey(sParam) Then
                                        Dim tVal As Double = If(isDollarValues AndAlso tAlt.Cost <> UNDEFINED_INTEGER_VALUE, tAlt.Cost, 1)  ' D6464
                                        RA.Scenarios.ActiveScenario.Constraints.SetConstraintValue(CCList(sParam).ID, tAlt.ID, tVal)        ' D6464
                                    End If
                                End If
                            Next
                        End If

                    End If
                Next

                App.ActiveProject.ProjectManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, App.ActiveProject.ProjectManager.StorageManager.ProjectLocation, App.ActiveProject.ProjectManager.StorageManager.ProviderType, App.ActiveProject.ProjectManager.StorageManager.ModelID)
            End If
        End If
    End Sub
    ' D3109 ==

    ' D3248 ===
    Private Sub CreateCustConstraintsFromNumAttribute(tAttr As clsAttribute, fIsRO As Boolean) ' D3338
        If tAttr IsNot Nothing Then
            Dim tItem As clsAttribute = App.ActiveProject.ProjectManager.Attributes.GetAttributeByID(tAttr.ID)
            If tItem IsNot Nothing Then

                'Dim tNewCC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.AddConstraint(tItem.Name) 'AS/11-18-15
                Dim tNewCC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.AddConstraint("-1", tItem.Name) 'AS/11-18-15
                tNewCC.LinkedAttributeID = tAttr.ID
                tNewCC.LinkedEnumID = tItem.ID
                tNewCC.IsReadOnly = fIsRO   ' D3338

                ' D6049 ===
                Dim tLst As IEnumerable(Of Object)
                If App.isRiskEnabled Then tLst = App.ActiveProject.ProjectManager.Controls.EnabledControls Else tLst = App.ActiveProject.HierarchyAlternatives.TerminalNodes
                For Each tNodeAlt As Object In tLst
                    Dim NodeGUID As Guid = If(App.isRiskEnabled, CType(tNodeAlt, clsControl).ID, CType(tNodeAlt, clsNode).NodeGuidID)  ' D3120
                    Dim tAlt As RAAlternative = RA.Scenarios.ActiveScenario.Alternatives.Find(Function(a) (a.ID.ToLower = NodeGUID.ToString.ToLower))
                    If tAlt IsNot Nothing Then
                        Dim objAttrValue As Object = App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(tAttr.ID, NodeGUID)
                        ' D6049 ==
                        If objAttrValue Is Nothing AndAlso tAttr.DefaultValue IsNot Nothing Then objAttrValue = tAttr.DefaultValue ' D3379
                        If objAttrValue IsNot Nothing Then
                            RA.Scenarios.ActiveScenario.Constraints.SetConstraintValue(tNewCC.ID, tAlt.ID, CDbl(objAttrValue))
                        End If
                    End If
                Next

                App.ActiveProject.ProjectManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, App.ActiveProject.ProjectManager.StorageManager.ProjectLocation, App.ActiveProject.ProjectManager.StorageManager.ProviderType, App.ActiveProject.ProjectManager.StorageManager.ModelID)
            End If
        End If
    End Sub
    ' D3248 ==

    ' D3112 ===
    Private Sub Ajax_Callback()
        Dim args As NameValueCollection = Page.Request.QueryString

        Dim sResult As String = ""
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION)).ToLower    ' Anti-XSS
        Dim SaveRA As Boolean = False
        Dim sComment As String = "" ' D3790

        '  D3474 ===
        Dim SortIdx As Integer = Math.Abs(RA.Scenarios.GlobalSettings.SortBy)
        Dim SortDesc As Integer = CInt(IIf(RA.Scenarios.GlobalSettings.SortBy < 0, -1, 1))
        ' Get the Idx of CC/Attribute when sorted
        SortIdx = CInt(IIf(SortIdx >= raColumnID.CustomConstraintsStart, SortIdx - raColumnID.CustomConstraintsStart, -1))
        Dim tConstrSort As Integer = -1
        Dim CC_CountOld As Integer = RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys.Count
        If SortIdx < CC_CountOld Then
            Dim Idx As Integer = 0
            For Each tID As Integer In RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys
                ' Get ID (key) of CC when sorted by CC
                If Idx = SortIdx Then
                    tConstrSort = tID
                    Exit For
                End If
                Idx += 1
            Next
        End If
        ' D3474 ==

        Select Case sAction
            ' D2882 ===
            Case "edit_constraint"    ' using for add new as well (ID=-1)
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                Dim sName As String = GetParam(args, "name").Trim
                Dim fEnabled As Boolean = GetParam(args, "chk") = "1"
                Dim fIsResource As Boolean = GetParam(args, "resource") = "1"   ' D3615
                If RA_OPT_FORCE_CC_USE_IN_TIMEPERIODS Then fIsResource = True ' D3826
                If Not RA_OPT_CC_ALLOW_ENABLED_PROPERTY Then fEnabled = True ' D3166
                Dim ID As Integer
                If Integer.TryParse(sID, ID) AndAlso sName <> "" Then
                    Dim RAC As RAConstraint = Nothing
                    If ID < 0 Then
                        ' D3019 ===
                        Dim Lst As String() = sName.Replace(vbCr, "").Trim.Split(CChar(vbLf))
                        For Each tmpName As String In Lst
                            If EcSanitizer.GetSafeHtmlFragment(tmpName).Trim <> "" Then 'Anti-XSS
                                'RAC = RA.Scenarios.ActiveScenario.Constraints.AddConstraint(tmpName.Trim) 'AS/11-18-15
                                RAC = RA.Scenarios.ActiveScenario.Constraints.AddConstraint("-1", EcSanitizer.GetSafeHtmlFragment(tmpName).Trim) 'AS/11-18-15 + Anti-XSS
                                If RAC IsNot Nothing Then
                                    RAC.Enabled = fEnabled
                                    RAC.IsLinkedToResource = fIsResource    ' D3745
                                End If
                                sResult = RAC.ID.ToString
                                sComment += String.Format("{0}'{1}'", IIf(sComment = "", "", ", "), ShortString(EcSanitizer.GetSafeHtmlFragment(tmpName).Trim, 35, True))    ' D3790 + Anti-XSS
                            End If
                        Next
                        If sComment <> "" Then sComment = "Add " + sComment ' D3790
                        SaveRA = True
                        ' D3019 ==
                    Else
                        If RA.Scenarios.ActiveScenario.Constraints.Constraints.ContainsKey(ID) Then RAC = RA.Scenarios.ActiveScenario.Constraints.Constraints(ID)
                        ' D3019 ===
                        If RAC IsNot Nothing Then
                            ' D6464 ===
                            If RAC.Name <> sName AndAlso RAC.Name.Length > 0 AndAlso sName.Length > 0 AndAlso (RAC.Name.EndsWith("$") OrElse sName.EndsWith("$")) Then
                                If RAC.IsLinked AndAlso Not RAC.LinkedEnumID.Equals(Guid.Empty) Then
                                    RAC.Name = sName
                                    Dim Lst As New List(Of RAConstraint)
                                    Lst.Add(RAC)
                                    RA.Scenarios.SyncLinkedConstraintsValues(Lst)
                                End If
                            End If
                            ' D6464 ==
                            RAC.Name = sName
                            RAC.Enabled = fEnabled
                            'TODO: assing fIsResource
                            SaveRA = True
                            sResult = RAC.ID.ToString
                            sComment = String.Format("Edit '{0}'", ShortString(RAC.Name, 40, True)) ' D3790
                        End If
                        ' D3019 ==
                    End If
                End If

            Case "enable_constraint"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                Dim fEnabled As Boolean = GetParam(args, "chk") <> "1"  ' D3540
                Dim ID As Integer
                If Integer.TryParse(sID, ID) Then
                    If RA.Scenarios.ActiveScenario.Constraints.Constraints.ContainsKey(ID) Then
                        Dim RAC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.Constraints(ID)
                        If RAC IsNot Nothing Then RAC.Enabled = fEnabled
                        SaveRA = True   ' D2909
                        sResult = RAC.ID.ToString
                        sComment = String.Format("{0} '{1}'", IIf(fEnabled, "Ignore", "Enable"), ShortString(RAC.Name, 40, True))  ' D3790
                    End If
                End If

                ' D4814 ===
            Case "enable_all"
                Dim isChecked As Boolean = GetParam(args, "val") = "1"
                For Each tConstr As RAConstraint In RA.Scenarios.ActiveScenario.Constraints.Constraints.Values
                    tConstr.Enabled = isChecked
                Next
                SaveRA = True
                sResult = String.Format("[[{0}],[{1}],[{2}]]", GetConstraints(), GetAttributes(True), GetAttributes(False))
                sComment = String.Format("{0} all custom constraints", IIf(isChecked, "Enable", "Disable"))
                ' D4814 ==

                ' D3615 ===
            Case "is_resource"
                If Not RA_OPT_FORCE_CC_USE_IN_TIMEPERIODS Then   ' D3826
                    Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                    Dim fEnabled As Boolean = GetParam(args, "chk") = "1"   ' D3745
                    Dim ID As Integer
                    If Integer.TryParse(sID, ID) Then
                        If RA.Scenarios.ActiveScenario.Constraints.Constraints.ContainsKey(ID) Then
                            Dim RAC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.Constraints(ID)
                            If RAC IsNot Nothing Then RAC.IsLinkedToResource = fEnabled ' D3745
                            SaveRA = True
                            sResult = RAC.ID.ToString
                            sComment = String.Format("'{1}' used in Timepriods: {0}", Bool2YesNo(fEnabled), ShortString(RAC.Name, 40, True))  ' D3790
                        End If
                    End If
                    ' D3615 ==
                End If

                ' D3336 ===
            Case "readonly"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                Dim fRO As Boolean = GetParam(args, "val") = "2"
                Dim ID As Integer
                If Integer.TryParse(sID, ID) Then
                    If RA.Scenarios.ActiveScenario.Constraints.Constraints.ContainsKey(ID) Then
                        Dim RAC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.Constraints(ID)
                        If RAC IsNot Nothing AndAlso Not Equals(RAC.LinkedAttributeID, Guid.Empty) Then
                            RAC.IsReadOnly = fRO
                            ' D3346 ===
                            Dim Lst As New List(Of RAConstraint)
                            Lst.Add(RAC)
                            If fRO AndAlso GetParam(args, "sync") = "1" Then RA.Scenarios.SyncLinkedConstraintsValues(Lst) ' D3340
                            ' D3346 ==
                            SaveRA = True
                            sComment = String.Format("Link '{1}' to attribute: {0}", Bool2YesNo(fRO), ShortString(RAC.Name, 40, True))  ' D3790
                        End If
                        sResult = String.Format("[[{0}],[{1}],[{2}]]", GetConstraints(), GetAttributes(True), GetAttributes(False)) ' D3294
                    End If
                End If
                ' D3336 ==

            Case "delete_constraint"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                Dim ID As Integer
                If Integer.TryParse(sID, ID) AndAlso RA.Scenarios.ActiveScenario.Constraints.Constraints.ContainsKey(ID) Then   ' D2887
                    sComment = String.Format("Delete '{0}'", ShortString(RA.Scenarios.ActiveScenario.Constraints.Constraints(ID).Name, 40, True))  ' D3790 + D3792
                    RA.Scenarios.ActiveScenario.Constraints.DeleteConstraint(ID)
                    SaveRA = True   ' D2909
                End If
                sResult = String.Format("[[{0}],[{1}],[{2}]]", GetConstraints(), GetAttributes(True), GetAttributes(False)) ' D3294
                ' D2882 ==

                ' D3262 ===
            Case "delete_constraints"
                Dim Lst As String() = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ids")).Trim.Split(CChar(","))  ' Anti-XSS
                For Each sID As String In Lst
                    Dim ID As Integer
                    If Integer.TryParse(sID, ID) AndAlso RA.Scenarios.ActiveScenario.Constraints.Constraints.ContainsKey(ID) Then   ' D2887
                        sComment += String.Format("{0}'{1}'", IIf(sComment = "", "", ", "), ShortString(RA.Scenarios.ActiveScenario.Constraints.Constraints(ID).Name, 40, True))    ' D3790 + D3792
                        RA.Scenarios.ActiveScenario.Constraints.DeleteConstraint(ID)
                        SaveRA = True   ' D2909
                    End If
                Next
                If sComment <> "" Then sComment = "Delete " + sComment ' D3790
                sResult = String.Format("[[{0}],[{1}],[{2}]]", GetConstraints(), GetAttributes(True), GetAttributes(False)) ' D3294
                ' D3262 ==

                ' D2931 ===
            Case "delete_all_constraints"
                RA.Scenarios.ActiveScenario.Constraints.Constraints.Clear()
                SaveRA = True
                sComment = "Delete all constraints" ' D3790
                sResult = String.Format("[[{0}],[{1}],[{2}]]", GetConstraints(), GetAttributes(True), GetAttributes(False)) ' D3294
                ' D2931 ==

                ' D3097 ===
            Case "import_constraint"
                Dim sAID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "aid"))   ' Anti-XSS
                ' D3248 ===
                Dim sIsCat As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "is_cat")).Trim.ToLower ' Anti-XSS
                Dim fIsRO As Boolean = GetParam(args, "ro").Trim = "1" ' D3338
                If sAID <> "" AndAlso sIsCat <> "" Then
                    Dim tAttr As clsAttribute = Nothing
                    If sIsCat = "true" OrElse sIsCat = "1" Then
                        tAttr = CatAttributesList.Find(Function(x) x.ID.ToString = sAID)
                        ' D3248 ==
                        If tAttr IsNot Nothing Then
                            ' D3281 ===
                            Dim IDs As New List(Of Guid)
                            Dim sIDS As String() = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ids")).Split(CChar(";"))  ' Anti-XSS
                            For Each sID As String In sIDS
                                If sID.Trim <> "" Then IDs.Add(New Guid(sID)) ' D3340
                            Next
                            CreateCustConstraintsFromCatAttribute(tAttr, IDs, fIsRO, tAttr.Name.EndsWith("$")) ' D3109 + D3338 + D6464
                            ' D3281 ==
                            sComment = String.Format("Import from categorical attribute '{0}'", ShortString(tAttr.Name, 40, True))  ' D3790
                            SaveRA = True
                        End If
                        ' D3248 ===
                    Else
                        tAttr = NumAttributesList.Find(Function(x) x.ID.ToString = sAID)
                        If tAttr IsNot Nothing Then
                            CreateCustConstraintsFromNumAttribute(tAttr, fIsRO)   ' D3388
                            sComment = String.Format("Import from numerical attribute '{0}'", ShortString(tAttr.Name, 40, True))  ' D3790
                            SaveRA = True
                        End If
                    End If
                    ' D3248 ==
                End If
                RA.Scenarios.SyncLinkedConstraintsToResources() ' D4913
                sResult = String.Format("[[{0}],[{1}],[{2}]]", GetConstraints(), GetAttributes(True), GetAttributes(False)) ' D3294
                ' D3097 ==

                ' D3281 ===
            Case "get_attribs"
                sResult = ""
                Dim sAID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "aid"))   ' Anti-XSS
                Dim tAttr As clsAttribute = CatAttributesList.Find(Function(x) x.ID.ToString = sAID)
                If tAttr IsNot Nothing Then
                    Dim CCLIst As New List(Of Guid)
                    For Each tCCKey As Integer In RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys
                        'If RA.Scenarios.ActiveScenario.Constraints.Constraints(tCCKey).LinkedAttributeID.Equals(tAttr.ID) Then
                        CCLIst.Add(RA.Scenarios.ActiveScenario.Constraints.Constraints(tCCKey).LinkedEnumID)
                        'End If
                    Next
                    Dim sItems As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(tAttr.EnumID)
                    If sItems IsNot Nothing Then
                        For Each tItem As clsAttributeEnumerationItem In sItems.Items
                            sResult += CStr(IIf(sResult = "", "", ",")) + String.Format("['{0}','{1}',{2}]", tItem.ID, JS_SafeString(tItem.Value), IIf(CCLIst.Contains(tItem.ID), 1, 0))
                        Next
                    End If
                End If
                sResult = "[" + sResult + "]"
                ' D3281 ==

                'A0959 ===
            Case "reorder_constraints"
                Dim lst As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "lst"))    ' Anti-XSS
                If Not String.IsNullOrEmpty(lst) Then
                    Dim newDict As Dictionary(Of Integer, RAConstraint) = New Dictionary(Of Integer, RAConstraint)
                    Dim oldDict As Dictionary(Of Integer, RAConstraint) = RA.Scenarios.ActiveScenario.Constraints.Constraints
                    Dim indices As String() = lst.Split(CChar(","))
                    For Each id As String In indices
                        Dim idx As Integer = CInt(id)
                        If oldDict.ContainsKey(idx) Then
                            newDict.Add(idx, oldDict(idx))
                            SaveRA = True
                        End If
                    Next
                    If SaveRA Then
                        RA.Scenarios.ActiveScenario.Constraints.Constraints = newDict
                        sComment = String.Format("Reorder constraints")  ' D3790
                    End If
                End If
                'A0959 ==

                ' D3336 ===
            Case "fix_links"
                FixBrokenLinks()
                sResult = String.Format("[[{0}],[{1}],[{2}]]", GetConstraints(), GetAttributes(True), GetAttributes(False)) ' D3294

                ' D3336 ===
            Case "reset_link"
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                Dim ID As Integer
                If Integer.TryParse(sID, ID) Then
                    If RA.Scenarios.ActiveScenario.Constraints.Constraints.ContainsKey(ID) Then
                        Dim RAC As RAConstraint = RA.Scenarios.ActiveScenario.Constraints.Constraints(ID)
                        If RAC IsNot Nothing Then
                            RAC.LinkedAttributeID = Guid.Empty
                            RAC.LinkedEnumID = Guid.Empty
                        End If
                        SaveRA = True
                        sComment = String.Format("Reset link for '{0}'", ShortString(RAC.Name, 40, True))  ' D3790
                        sResult = String.Format("[[{0}],[{1}],[{2}]]", GetConstraints(), GetAttributes(True), GetAttributes(False)) ' D3294
                    End If
                End If
                ' D3336 ==

                ' D3340 ===
            Case "sync_constr"
                ' D3346 ===
                Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ids")).Trim   ' Anti-XSS
                Dim Lst As New List(Of RAConstraint)
                If sID <> "" Then
                    Dim IDs As String() = sID.Split(CChar(","))
                    For Each tmpID As String In IDs
                        Dim ID As Integer
                        If Integer.TryParse(tmpID, ID) Then
                            If RA.Scenarios.ActiveScenario.Constraints.Constraints.ContainsKey(ID) Then
                                Lst.Add(RA.Scenarios.ActiveScenario.Constraints.Constraints(ID))
                                ' D3346 ==
                            End If
                        End If
                    Next
                    If Lst.Count > 0 Then If RA.Scenarios.SyncLinkedConstraintsValues(Lst) Then SaveRA = True ' D3346
                    ' D3343 ===
                Else
                    If RA.Scenarios.SyncLinkedConstraintsValues(Nothing) Then
                        sResult = "OK"
                        SaveRA = True
                    End If
                    ' D3343 ==
                End If
                If SaveRA Then sComment = String.Format("Reload constraints") ' D3790
                ' D3340 ==
        End Select

        ' D3474 ===
        ' try to search CC if it was by sorted by CC
        Dim fHasColumn As Boolean = False
        If tConstrSort >= 0 Then
            Dim Idx As Integer = 0
            For Each tID As Integer In RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys
                If tConstrSort = tID Then
                    RA.Scenarios.GlobalSettings.SortBy = CType(SortDesc * (Idx + CInt(raColumnID.CustomConstraintsStart)), raColumnID)
                    fHasColumn = True
                    Exit For
                End If
                Idx += 1
            Next
        End If
        Dim CC_CountNew As Integer = RA.Scenarios.ActiveScenario.Constraints.Constraints.Keys.Count
        If Not fHasColumn AndAlso SortIdx >= 0 Then
            If SortIdx >= CC_CountOld Then
                ' When sorted by attribute, just shift
                RA.Scenarios.GlobalSettings.SortBy = CType(SortDesc * (Math.Abs(RA.Scenarios.GlobalSettings.SortBy) + (CC_CountNew - CC_CountOld)), raColumnID)
            Else
                ' Reset to sort by ID
                RA.Scenarios.GlobalSettings.SortBy = raColumnID.ID
            End If
        End If
        ' D3474 ==

        If SaveRA Then App.ActiveProject.SaveRA("Edit custom constraints", , , sComment) ' D3790

        If sResult <> "" Then
            RawResponseStart()
            Response.ContentType = "text/plain"
            'Response.AddHeader("Content-Length", CStr(sResult))
            Response.Write(sResult)
            Response.End()
            'RawResponseEnd()
        End If
    End Sub
    ' D3112 ==

    Private Sub RACustomConstraints_Init(sender As Object, e As EventArgs) Handles Me.Init
        RA.Load()
        ' D3181 ===
        If Not IsPostBack AndAlso Not isCallback Then
            '-A1324 RA.Scenarios.CheckAlternatives()
            '-A1324 RA.Scenarios.UpdateSortOrder()
            RA.Scenarios.CheckModel(False) 'A1324
        End If
        ' D3181 ==
    End Sub

End Class
