Imports System.Xml

Partial Class ChartsPage
    Inherits clsComparionCorePage

    Public Const _TT_LINE_BREAK As String = "&#013;"    

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            If App.HasActiveProject Then Return App.ActiveProject.ProjectManager Else Return Nothing
        End Get
    End Property

    Private _Api As DashboardWebAPI = Nothing
    Public ReadOnly Property Api As DashboardWebAPI
        Get
            If _Api Is Nothing Then _Api = New DashboardWebAPI
            Return _Api
        End Get
    End Property

    Public Sub New()
        MyBase.New(_PGID_ANALYSIS_CHARTS_ALTS)
    End Sub

    Public ReadOnly Property NormalizeModeString As String
        Get
            If PM.IsRiskProject Then Return "none"

            Select Case PM.Parameters.Normalization
                Case LocalNormalizationType.ntUnnormalized
                    Return "none"
                Case LocalNormalizationType.ntNormalizedSum100
                    Return "normSelected"
                Case LocalNormalizationType.ntNormalizedMul100
                    Return "norm100"
                Case LocalNormalizationType.ntNormalizedForAll
                    Return "normAll"
            End Select
            Return "none"
        End Get
    End Property

    Public Function GetTitle() As String
        Dim retVal As String = ""
        Dim wrtNodeName As String = ""
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(Api.WRTNodeID)
        If wrtNode IsNot Nothing Then wrtNodeName = wrtNode.NodeName
        Dim RMode As String = ""
        If PM.IsRiskProject  Then
            RMode = If (PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("%%Riskmodels%%"), "Impacts")
        Else
            RMode = "Priorities"
        End If
        'retVal = String.Format("{0}<br/><small><span id='lblPageTitleAlts' style='display:none;'>{1}</span><span id='lblPageTitleObjs' style='display:none;'>{2}</span><span id='lblTitleUserNames' class='cs-text-overflow-ellipsis-line'>{3}</span><br/><span id='lblWrtNode'>{4}</span></small>", ShortString(SafeFormString(App.ActiveProject.ProjectName), 85), PageTitle(_PGID_ANALYSIS_DASHBOARD_ALTS), PageTitle(_PGID_ANALYSIS_DASHBOARD_OBJS), "", If(wrtNodeName <> "", "with respect to " + ShortString(SafeFormString(wrtNodeName), 85), ""))
        retVal = String.Format("<small><span id='lblPageTitleAlts' class='pgChartsTitle' style='display:none;'>{1}</span><span id='lblPageTitleObjs' class='pgChartsTitle' style='display:none;'>{2}</span><span id='lblTitleUserNames' class='cs-text-overflow-ellipsis-line'>{3}</span><br/><span id='lblWrtNode'>{4}</span></small>", ShortString(SafeFormString(App.ActiveProject.ProjectName), 85), ParseString("%%Alternative%% ") + RMode, ParseString("%%Objective%% ") + If(PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, "Likelihoods", "Priorities"), "", "")
        Return retVal
    End Function

#Region "Filtering by alternative attribute"
    Public Const Flt_Separator As String = vbTab

    ''' <summary>
    ''' 1-N - show top 1-N alternatives
    ''' -1  - Show All alternatives
    ''' -2  - Advanced
    ''' -3  - Select/deselect alternatives
    ''' -4  - Filter by funded alternatives in RA scenario
    ''' -5  - Filter by alternative attributes
    ''' </summary>
    ''' <returns></returns>
    Public Property AlternativesFilterValue As Integer
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_ALTS_FILTER_ID, UNDEFINED_USER_ID))
            If retVal = -5 AndAlso Not ActiveProjectHasAlternativeAttributes Then retVal = -1
            'If Not IsMixedOrAltsView() AndAlso Not IsConsensusView() AndAlso retVal < -1 Then retVal = -1
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_ALTS_FILTER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property AlternativesAdvancedFilterValue As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property AlternativesAdvancedFilterUserID As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_UID_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_UID_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Private Property IsAltSelected(AltGUID As Guid) As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RESULTS_ALTERNATIVE_IS_SELECTED_ID, AltGUID, Guid.Empty))
        End Get
        Set(value As Boolean)
            With PM
                .Attributes.SetAttributeValue(ATTRIBUTE_RESULTS_ALTERNATIVE_IS_SELECTED_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, AltGUID, Guid.Empty)
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End Set
    End Property

    Private ReadOnly Property IsAltFunded(AltGUID As Guid) As Boolean
        Get
            Return PM.ResourceAligner.Scenarios.ActiveScenario.GetAvailableAlternativeById(AltGUID.ToString) IsNot Nothing AndAlso CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID, AltGUID, Guid.Empty)) > 0
        End Get
    End Property

    Public ReadOnly Property ActiveProjectHasAlternativeAttributes As Boolean
        Get
            If App IsNot Nothing AndAlso App.ActiveProject IsNot Nothing AndAlso App.ActiveProject.ProjectManager IsNot Nothing AndAlso App.ActiveProject.ProjectManager.Attributes IsNot Nothing AndAlso App.ActiveProject.ProjectManager.Attributes.AttributesList IsNot Nothing Then
                For Each attr In App.ActiveProject.ProjectManager.Attributes.AttributesList
                    If attr.Type = CurrentAlternativeAttributesType AndAlso Not attr.IsDefault Then Return True
                Next
            End If
            Return False
        End Get
    End Property

    Private _FilterCombination As FilterCombinations = FilterCombinations.fcAnd
    Public Property FilterCombination As FilterCombinations
        Get
            Return _FilterCombination
        End Get
        Set(value As FilterCombinations)
            _FilterCombination = value
        End Set
    End Property

    ReadOnly Property SESSION_FILTER_RULES As String
        Get
            Return String.Format("RiskResultsFilterRulesAlts_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Private _CurrentFiltersList As List(Of clsFilterItem) = Nothing
    Public Property CurrentFiltersList As List(Of clsFilterItem)
        Get
            If _CurrentFiltersList Is Nothing Then
                ' D2668 ===
                Dim tSessVar = Session(SESSION_FILTER_RULES)
                If tSessVar Is Nothing OrElse Not (TypeOf tSessVar Is List(Of clsFilterItem)) Then
                    _CurrentFiltersList = New List(Of clsFilterItem)
                    Session.Add(SESSION_FILTER_RULES, _CurrentFiltersList)
                Else
                    _CurrentFiltersList = CType(tSessVar, List(Of clsFilterItem))
                End If
                ' D2668 ==
            End If
            Return _CurrentFiltersList
        End Get
        Set(value As List(Of clsFilterItem))
            _CurrentFiltersList = value
            Session(SESSION_FILTER_RULES) = value
        End Set
    End Property

    Private ReadOnly Property CurrentAlternativeAttributesType As AttributeTypes
        Get
            Return AttributeTypes.atAlternative
        End Get
    End Property

    Private ReadOnly Property CurrentObjectiveAttributesType As AttributeTypes
        Get
            Dim tCurrentAttrType As AttributeTypes = AttributeTypes.atNode
            If PM.IsRiskProject Then
                If PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
                    tCurrentAttrType = AttributeTypes.atLikelihood
                Else
                    tCurrentAttrType = AttributeTypes.atImpact
                End If
            End If
            Return tCurrentAttrType
        End Get
    End Property

    Public Function LoadAttribData() As String
        Dim sList As String = ""
        Dim sFlt As String = ""
        With App.ActiveProject.ProjectManager
            Dim fIsEmpty As Boolean = CurrentFiltersList.Count = 0
            For Each tAttr As clsAttribute In .Attributes.AttributesList
                If Not tAttr.IsDefault AndAlso tAttr.Type = CurrentAlternativeAttributesType Then
                    Dim sVals As String = ""
                    If tAttr.ValueType = AttributeValueTypes.avtEnumeration OrElse tAttr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim tVals As clsAttributeEnumeration = .Attributes.GetEnumByID(tAttr.EnumID)
                        If tVals IsNot Nothing Then
                            For Each tVal As clsAttributeEnumerationItem In tVals.Items
                                sVals += CStr(IIf(sVals = "", "", ",")) + String.Format("[""{0}"",""{1}""]", tVal.ID, JS_SafeString(tVal.Value))
                            Next
                        End If
                    End If
                    sList += CStr(IIf(sList = "", "", ",")) + String.Format("[""{0}"",""{1}"",{2},[{3}]]", tAttr.ID, JS_SafeString(App.GetAttributeName(tAttr)), CInt(tAttr.ValueType), sVals)
                    If fIsEmpty Then CurrentFiltersList.Add(New clsFilterItem With {.IsChecked = False, .SelectedAttributeID = tAttr.ID, .FilterOperation = FilterOperations.Equal})
                End If
            Next
            Dim i As Integer = 0
            While i < CurrentFiltersList.Count
                Dim tVal As clsFilterItem = CurrentFiltersList(i)
                Dim sVal As String = ""
                Dim tAttr = App.ActiveProject.ProjectManager.Attributes.GetAttributeByID(tVal.SelectedAttributeID)
                If tAttr IsNot Nothing Then
                    Select Case tAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration
                            sVal = CStr(IIf(tVal.FilterEnumItemID.Equals(Guid.Empty), "", tVal.FilterEnumItemID.ToString))
                        Case AttributeValueTypes.avtEnumerationMulti
                            sVal = ""
                            If tVal.FilterEnumItemsIDs IsNot Nothing Then
                                For Each ID As Guid In tVal.FilterEnumItemsIDs
                                    If sVal.Length > 0 Then sVal += ";"
                                    sVal += ID.ToString
                                Next
                            End If

                        Case Else
                            sVal = tVal.FilterText
                    End Select
                    sFlt += CStr(IIf(sFlt = "", "", ",")) + String.Format("[""{0}"",""{1}"",{2},""{3}""]", IIf(tVal.IsChecked, 1, 0), tAttr.ID, CInt(tVal.FilterOperation), JS_SafeString(sVal))
                    i += 1
                Else
                    CurrentFiltersList.RemoveAt(i)
                End If
            End While
        End With
        Return String.Format("var attr_list = [{0}];{1} var attr_flt = [{2}];", sList, vbCrLf, sFlt)
    End Function

    Private Sub ParseAttributesFilter(sFilter As String, sComb As String)
        CurrentFiltersList.Clear()
        If sFilter <> "" Then
            If sComb <> "" Then
                If sComb = "0" Then FilterCombination = FilterCombinations.fcAnd
                If sComb = "1" Then FilterCombination = FilterCombinations.fcOr
            End If

            Dim sRows() As String = sFilter.Trim.Split(CChar(vbLf))
            For Each sRow As String In sRows
                Dim tVals() As String = sRow.Split((CChar(Flt_Separator)))
                If tVals.Length >= 3 Then
                    Dim sChecked As String = tVals(0)
                    Dim sAttrID As String = tVals(1)
                    Dim sOperID As String = tVals(2)
                    Dim sFilterText As String = ""
                    If tVals.Length >= 4 Then sFilterText = tVals(3)

                    Dim tFilterItem As New clsFilterItem With {.FilterCombination = FilterCombination}
                    Dim tAttrID As Guid = New Guid(sAttrID)

                    With App.ActiveProject.ProjectManager
                        For Each tAttr As clsAttribute In .Attributes.AttributesList
                            If tAttr.Type = CurrentAlternativeAttributesType AndAlso tAttr.ID.Equals(tAttrID) Then
                                tFilterItem.SelectedAttributeID = tAttr.ID
                                Exit For
                            End If
                        Next
                    End With

                    tFilterItem.IsChecked = (sChecked = "1")
                    tFilterItem.FilterOperation = CType(CInt(sOperID), FilterOperations)
                    tFilterItem.FilterText = sFilterText

                    Dim tAttribute = App.ActiveProject.ProjectManager.Attributes.GetAttributeByID(tFilterItem.SelectedAttributeID)
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = CurrentAlternativeAttributesType AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumeration Then
                        tFilterItem.FilterEnumItemID = New Guid(sFilterText)
                    End If
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = CurrentAlternativeAttributesType AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim sGuids As String() = sFilterText.Split(CChar(";"))
                        For Each sGuid In sGuids
                            If sGuid.Length > 0 Then
                                If tFilterItem.FilterEnumItemsIDs Is Nothing Then tFilterItem.FilterEnumItemsIDs = New List(Of Guid)
                                tFilterItem.FilterEnumItemsIDs.Add(New Guid(sGuid))
                            End If
                        Next
                    End If

                    CurrentFiltersList.Add(tFilterItem)
                End If
            Next
        End If
    End Sub

    Function GetUsersDropdown() As String
        Dim sRes As String = ""
        For Each grp As clsCombinedGroup In PM.CombinedGroups.GroupsList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", grp.CombinedUserID, SafeFormString(grp.Name), CStr(IIf(grp.CombinedUserID = COMBINED_USER_ID, " selected='selected' ", "")))
        Next
        sRes += "<option disabled='disabled'>---------------------</option>"
        For Each user As clsUser In PM.UsersList
            sRes += String.Format("<option value='{0}'>{1}</option>", user.UserID, SafeFormString(user.UserName))
        Next
        Return String.Format("<select class='select' id='cbAdvUsers' style='width:150px;'>{0}</select>", sRes)
    End Function

    Function GetAltsNumDropdown() As String
        Dim sRes As String = ""
        Dim AH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        If AH IsNot Nothing Then
            For i As Integer = 0 To AH.TerminalNodes.Count
                sRes += String.Format("<option value='{0}' {2}>{1}</option>", i, i, CStr(If((AlternativesFilterValue = -2 AndAlso i = AlternativesAdvancedFilterValue) OrElse (AlternativesFilterValue <> -2 AndAlso i = AH.TerminalNodes.Count), " selected='selected' ", "")))
            Next
        End If
        Return String.Format("<select class='select' id='cbAdvAltsNum' style='width:150px;'>{0}</select>", sRes)
    End Function

    Function GetAltsCheckList() As String
        Dim sRes As String = ""
        Dim AH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        If AH IsNot Nothing Then
            For Each alt As clsNode In AH.TerminalNodes
                sRes += String.Format("<label><input type='checkbox' class='cust_alt_cb' value='{0}' {1}/>{2}</label><br/>", alt.NodeID, CStr(IIf(IsAltSelected(alt.NodeGuidID), " checked='checked' ", " ")), SafeFormString(alt.NodeName)) 'A1120
            Next
        End If
        Return sRes
    End Function

#End Region

End Class