Imports ECCore.ECTypes
Imports System.IO
Imports System
Imports System.Xml
Imports SpyronControls.Spyron.Core
Imports GemBox.Document 'AS/17533a
Imports GemBox.Presentation 'AS/17533b
Imports GemBox.Spreadsheet 'AS/17533f
Imports GemBox.Pdf

Partial Class DashboardWebAPI
    Inherits clsComparionCorePage

    Private _UserIDsWithData As HashSet(Of Integer)
    Private Const _TT_LINE_BREAK As String = "&#013;"

    Public Const ACTION_DSA_UPDATE_VALUES As String = "dsa_update_values"
    Public Const ACTION_DSA_INIT_DATA As String = "dsa_init_data"

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property

    Public ReadOnly Property Scenario As RAScenario
        Get
            Return RA.Scenarios.ActiveScenario
        End Get
    End Property
    'D6850: moved to PM.Parameters.DecimalDigits
    Private Property DecimalDigits As Integer
        Get
            Dim retVal As Integer = 2
            retVal = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID))
            If retVal < 0 Then retVal = 0
            If retVal > 5 Then retVal = 5
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_DECIMALS_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Enum RiskionShowEventsType
        etRisks = 0
        etGains = 1
        etBoth = 2
    End Enum

    Public Property ShowEventsOfType As RiskionShowEventsType
        Get
            If Not PM.IsRiskProject Then Return RiskionShowEventsType.etBoth
            Return CType(PM.Parameters.RiskionShowEventsType, RiskionShowEventsType)
        End Get
        Set(value As RiskionShowEventsType)
            PM.Parameters.RiskionShowEventsType = value
            PM.Parameters.Save()
        End Set
    End Property

    ' D6876 ===
    Public Property GridWRTNodeID As Integer
        Get
            Return PM.Parameters.Synthesis_WRTNodeID
        End Get
        Set(value As Integer)
            PM.Parameters.Synthesis_WRTNodeID = value
        End Set
    End Property
    ' D6876 ==

    Private ReadOnly Property SESSION_USERS_WITH_DATA_LIST_ID As String
        Get
            Return String.Format("Synthesis_UsersWithData_List_{0}", App.ProjectID)
        End Get
    End Property

    ' D6455 ===
    Function GetReportOptions() As ReportGeneratorOptions
        Dim Options As New ReportGeneratorOptions
        For Each prop As Reflection.FieldInfo In GetType(ReportGeneratorOptions).GetFields
            If prop.IsPublic Then
                If HasParam(_Page.Params, prop.Name, True) Then
                    Dim sVal As String = GetParam(_Page.Params, prop.Name, True)
                    Select Case prop.FieldType
                        Case GetType(String)
                            prop.SetValue(Options, sVal)
                        Case GetType(Integer)
                            Dim tVal As Integer
                            If Integer.TryParse(sVal, tVal) Then prop.SetValue(Options, tVal)
                        Case GetType(Single)
                            Dim tVal As Single
                            If Single.TryParse(sVal, tVal) Then prop.SetValue(Options, tVal)
                        Case GetType(Boolean)
                            prop.SetValue(Options, Str2Bool(sVal))
                    End Select
                End If
            End If
        Next
        Return Options
    End Function
    ' D6455 ==

    Private Sub CreateGemBoxSpreadsheetWorkbook(ByVal filepath As String, ByRef docGB As DocumentModel) 'AS/17533f added docGB
        Dim ReportTest_xlsx As New GemboxTestSpreadsheet(App)
        'docGB = ReportTest_xlsx.Example_CreateChartInWordViaExcel(filepath)
        'docGB = ReportTest_xlsx.Test_CreateChartInWordViaExcel(filepath)
    End Sub

    Private Sub CreateGemBoxSpreadsheetWorkbook(ByVal filepath As String, ByRef workbookGB As ExcelFile) 'AS/17533f

        Dim ExcelExport As New ExcelExport(App, GridWRTNodeID) 'AS/19486a
        workbookGB = ExcelExport.GenerateExcelReport(filepath, Server, GetReportOptions()) 'AS/20761 temporary commented out while working on RA Reports
        'workbookGB = ExcelExport.GenerateExcelReport_RA(filepath, Server, GetReportOptions()) 'AS/20761 'AS/12-3-2020

        'workbookGB = ExcelExport.GenerateExcelReport_DEBUG(filepath, Server, GetReportOptions()) 'AS/19486q

        'Dim Report_xlsx As New ReportGenerator_Excel(App, GridWRTNodeID) 'AS/19464a added GridWRTNodeID 'AS/19486a
        'workbookGB = Report_xlsx.GenerateExcelReport(filepath, Server, GetReportOptions())
        'workbookGB = Report_xlsx.GenerateExcelReport_DEBUG(filepath, Server, GetReportOptions()) 'Timing and built-in colors and styles

        'Dim DoForUser As Integer = -1 'user ID to create the report for, default - All Participants
        'Dim ReportTest_xlsx As New GemboxTestSpreadsheet(App)
        'workbookGB = ReportTest_xlsx.Exsample_CondtionalFormatting(filepath)
        'workbookGB = ReportTest_xlsx.GenerateExcelReport(filepath, Server, GetReportOptions(), DoForUser)
        'workbookGB = ReportTest_xlsx.Test_AlternativesBarChart(filepath) 'AS/17533f
        'workbookGB = ReportTest_xlsx.Example_ComponentsChart(filepath) 'AS/17533f`
        'workbookGB = ReportTest_xlsx.Example_CreateSamplePieChart(filepath)
        'workbookGB = ReportTest_xlsx.Example_CreateSampleBarChart(filepath)

    End Sub

    Private Sub CreateGemBoxWordDocument(ByVal filepath As String, ByRef docGB As DocumentModel) 'AS/17663 'AS/17533a added docGB
        Dim Report_docx As New ReportGenerator_Word(App)
        docGB = Report_docx.GenerateWordReport(filepath, Server, GetReportOptions())    ' D6455

        'Dim ReportTest_docx As New GemBoxTestDoc(App)
        'docGB = ReportTest_docx.Test_InfodocAndPictureAndHierarchy(filepath, Server)
        'docGB = ReportTest_docx.Draw_Datagrid(filepath)
        'docGB = ReportTest_docx.Test_PageSetup(filepath)
        'docGB = ReportTest_docx.Test_SplitTable(filepath)
        'docGB = ReportTest_docx.Test_AutosizeTextbox(filepath)
        'docGB = ReportTest_docx.Draw_HierarchyVertical(filepath)
    End Sub

    Private Sub CreateGemBoxPresentation(ByVal filepath As String, ByRef presentationGB As GemBox.Presentation.PresentationDocument)
        Dim ReportTest_pptx As New GemBoxTestPresentation(App)
        Dim DoForUser As Integer = -1 'user ID to create the report for, default - All Participants
        presentationGB = ReportTest_pptx.GeneratePPTReport(filepath, Server, GetReportOptions(), DoForUser)

        'presentationGB = ReportTest_pptx.Test_TemplateUse_PPTX(filepath) 'AS/17533c
        'presentationGB = ReportTest_pptx.Example_TemplateUse_PPTX(filepath)
        'presentationGB = ReportTest_pptx.Test_InfodocAndHierarchy_PPTX(filepath)
        'presentationGB = ReportTest_pptx.Example_Treeview_PPTX(filepath)
        'presentationGB = ReportTest_pptx.Example_Table_PPTX(filepath)
        'presentationGB = ReportTest_pptx.Example_TableFormat_PPTX(filepath)

        'Dim ReportTest_pptx As New GemBoxTestPresentation. 'AS/17587
        'ReportTest_pptx.CreatePackage(filepath, App) 'AS/17587 'AS/17663b added parameter App to pass

        'Dim ReportHierarchy_pptx As New GeneratePresentation.GenerateHierarchyReport() 'AS/17533c
        'ReportHierarchy_pptx.CreatePackage(filepath) 'AS/17533c

    End Sub

    Private Sub CreateGemBoxPDF(ByVal filepath As String, ByRef pdfGB As GemBox.Pdf.PdfDocument) 'AS/17533o
        Dim ReportTest_pdf As New GemboxTestPDF(App)
        'pdfGB = ReportTest_pdf.HelloWorld(filepath)
        pdfGB = ReportTest_pdf.Example_AddWatermark(filepath)
    End Sub

    Public Property UserIDsWithData As HashSet(Of Integer)
        Get
            If _UserIDsWithData Is Nothing Then
                Dim tSess As Object = Session(SESSION_USERS_WITH_DATA_LIST_ID)
                If tSess Is Nothing Then
                    _UserIDsWithData = PM.StorageManager.Reader.DataExistsForUsersHashset(PM.ActiveHierarchy)
                    Session(SESSION_USERS_WITH_DATA_LIST_ID) = _UserIDsWithData
                Else
                    _UserIDsWithData = CType(tSess, HashSet(Of Integer))
                End If
            End If
            Return _UserIDsWithData
        End Get
        Set(value As HashSet(Of Integer))
            Session(SESSION_USERS_WITH_DATA_LIST_ID) = value
        End Set
    End Property

    Private Property SelectedUsersAndGroupsIDs As List(Of Integer)
        Get
            Dim sUsers As String = UNDEFINED_USER_ID.ToString
            sUsers = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_USERS_ID, UNDEFINED_USER_ID))
            If String.IsNullOrEmpty(sUsers) Then Return Nothing
            Dim tList As String() = sUsers.Split(CChar(","))
            If tList.Count > 0 Then
                Dim res As New List(Of Integer)
                For Each item In tList
                    Dim tUserID As Integer
                    If Integer.TryParse(item, tUserID) AndAlso (PM.UserExists(tUserID) OrElse PM.CombinedGroups.CombinedGroupExists(tUserID)) Then
                        res.Add(tUserID)
                    End If
                Next
                If res.Count = 0 Then res.Add(COMBINED_USER_ID)
                Return res
            End If
            Dim auto = New List(Of Integer)
            auto.Add(COMBINED_USER_ID)
            Return auto
        End Get
        Set(value As List(Of Integer))
            Dim sUsers As String = ""
            For i As Integer = 0 To value.Count - 1
                sUsers = sUsers + value(i).ToString
                If i <> value.Count - 1 Then sUsers += ","
            Next
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_USERS_ID, AttributeValueTypes.avtString, sUsers)
        End Set
    End Property

    Private Property StringSelectedUsersAndGroupsIDs As String
        Get
            Return CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_USERS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As String)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_USERS_ID, AttributeValueTypes.avtString, value)
        End Set
    End Property
    ''' <summary>
    ''' Return true if at least one user in the group has data (judgements)
    ''' </summary>
    ''' <param name="grp"></param>
    ''' <returns></returns>
    Public Function IsGroupHasData(grp As clsCombinedGroup) As Boolean
        Dim retVal As Boolean = False
        For Each user As clsUser In grp.UsersList
            If UserIDsWithData.Contains(user.UserID) Then Return True
        Next
        Return retVal
    End Function
    ''' <summary>
    ''' Returns list of users in form of JSON array string, each item in array contains [isChecked, UserID, UserName, UserEmail, HasData]
    ''' </summary>
    ''' <returns></returns>
    Public Function GetUsersData() As String
        Dim retVal As String = ""
        Dim userList As List(Of Integer) = SelectedUsersAndGroupsIDs

        For Each usr As clsUser In PM.UsersList
            Dim IsChecked As Boolean = userList.Contains(usr.UserID)
            Dim HasData As Boolean = UserIDsWithData.Contains(usr.UserID)
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},{1},""{2}"",""{3}"",{4}]", CStr(IIf(IsChecked, 1, 0)), usr.UserID, JS_SafeString(usr.UserName), JS_SafeString(usr.UserEMail), IIf(HasData, 1, 0)) ' isChecked, ID, name, email, HasData
        Next

        Return String.Format("[{0}]", retVal)
    End Function
    ''' <summary>
    ''' Get information about groups in form of JSON array string.
    ''' Each item in array contains isChecked, ID, name, 'email - obsolete', HasData (1/0), Participants, Column Header Extra Text, Column Header Tooltip
    ''' </summary>
    ''' <returns></returns>
    Public Function GetGroupsData() As String
        Dim retVal As String = ""
        Dim userList As List(Of Integer) = SelectedUsersAndGroupsIDs

        ' apply rules for dynamic groups
        With PM.StorageManager
            PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .ProjectLocation, .ProviderType, .ModelID, -1)
        End With

        ' get groups data
        If PM.CombinedGroups IsNot Nothing AndAlso PM.CombinedGroups.GroupsList IsNot Nothing Then
            For Each cg As clsCombinedGroup In PM.CombinedGroups.GroupsList
                cg.ApplyRules()
            Next

            For Each grp As clsCombinedGroup In PM.CombinedGroups.GroupsList
                Dim IsChecked As Boolean = userList.Contains(grp.CombinedUserID) 'grp.CombinedUserID = COMBINED_USER_ID
                Dim GroupHasData As Boolean = IsGroupHasData(grp)
                Dim sUsers As String = ""
                For Each u As clsUser In grp.UsersList
                    sUsers += CStr(IIf(sUsers = "", "", ",")) + u.UserID.ToString
                Next
                ' extra info
                Dim sParticipants As String = " participant"
                If grp.UsersList.Count <> 1 Then sParticipants += "s"

                'Dim HaveAnyJudgmentsCount As Integer = 0
                'Dim CompletedJudgmentsCount As Integer = 0
                '
                'Dim uList As New List(Of ECCore.clsUser)
                'For Each u As ECCore.clsUser In grp.UsersList
                '    uList.Add(u)
                'Next
                'For Each u As ECCore.clsUser In uList
                '    Dim lastTime As DateTime
                '    Dim totalCount As Integer = PM.GetTotalJudgmentCount(PM.ActiveHierarchy, u.UserID)
                '    Dim madeCount As Integer = PM.GetMadeJudgmentCount(PM.ActiveHierarchy, u.UserID, lastTime)
                '
                '    If madeCount > 0 Then HaveAnyJudgmentsCount += 1
                '    If (madeCount = totalCount) And (madeCount <> 0) Then CompletedJudgmentsCount += 1
                'Next
                '
                'Dim tTotal As String = grp.UsersList.Count.ToString + sParticipants
                'Dim tWithJudgments As String = HaveAnyJudgmentsCount.ToString + " with judgments"
                'Dim tWithCompleted As String = CompletedJudgmentsCount.ToString + " with completed judgments"
                '
                'Dim fShowTotal As Boolean = True 'ProjectBooleanSetting(SETTING_SHOW_HEADER_TOTAL_USERS_NAME, SETTING_SHOW_HEADER_TOTAL_USERS_DEFAULT_VALUE)
                'Dim fShowWithAny As Boolean = True 'ProjectBooleanSetting(SETTING_SHOW_HEADER_USERS_WITH_JUDGMENTS_NAME, SETTING_SHOW_HEADER_USERS_WITH_JUDGMENTS_DEFAULT_VALUE)
                'Dim fShowWithCompleted As Boolean = True 'ProjectBooleanSetting(SETTING_SHOW_HEADER_USERS_WITH_COMPLETED_JUDGMENTS_NAME, SETTING_SHOW_HEADER_USERS_WITH_COMPLETED_JUDGMENTS_DEFAULT_VALUE)

                Dim grpClmnHeaderExtra As String = "" 'If(fShowTotal, "<br/>" + tTotal, "") + If(fShowWithAny, "<br/>" + tWithJudgments, "") + If(fShowWithCompleted, "<br/>" + tWithCompleted, "")
                Dim grpClmnTooltip As String = "" '"[" + JS_SafeString(grp.Name) + "]" + _TT_LINE_BREAK + tTotal + _TT_LINE_BREAK + _TT_LINE_BREAK + tWithJudgments + _TT_LINE_BREAK + tWithCompleted
                retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},{1},""{2}"",""{3}"",{4},[{5}],""{6}"",""{7}""]", CStr(IIf(IsChecked, 1, 0)), grp.CombinedUserID, JS_SafeString(grp.Name), "", IIf(GroupHasData, 1, 0), sUsers, grpClmnHeaderExtra, grpClmnTooltip) ' isChecked, ID, name, 'email - obsolete', HasData (1/0), Participants, Column Header Extra Text, Column Header Tooltip
            Next
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    ''' <summary>
    ''' 1-N - show top 1-N alternatives
    ''' -1  - Show All alternatives
    ''' -2  - Advanced
    ''' -3  - Select/deselect alternatives
    ''' -4  - Filter by funded alternatives in RA scenario
    ''' -5  - Filter by alternative attributes
    ''' </summary>
    ''' <returns></returns>
    Private Property AlternativesFilterValue As Integer
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

    Private Property AlternativesAdvancedFilterValue As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_ALTS_ADV_FILTER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Private Property AlternativesAdvancedFilterUserID As Integer
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

    Public Const Flt_Separator As String = vbTab

    Private _FilterCombination As FilterCombinations = FilterCombinations.fcAnd
    Public Property FilterCombination As FilterCombinations
        Get
            Return _FilterCombination
        End Get
        Set(value As FilterCombinations)
            _FilterCombination = value
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

    Public Sub ParseAttributesFilter(sFilter As String, sComb As String)
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
                        Dim tEnumItemID As Guid = Guid.Empty
                        If Guid.TryParse(sFilterText, tEnumItemID) Then                         
                            tFilterItem.FilterEnumItemID = tEnumItemID
                        End If
                    End If
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = CurrentAlternativeAttributesType AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim sGuids As String() = sFilterText.Split(CChar(";"))
                        For Each sGuid In sGuids
                            If sGuid.Length > 0 Then
                                If tFilterItem.FilterEnumItemsIDs Is Nothing Then tFilterItem.FilterEnumItemsIDs = New List(Of Guid)
                                Dim tEnumItemID As Guid = Guid.Empty
                                If Guid.TryParse(sGuid, tEnumItemID) Then                         
                                    tFilterItem.FilterEnumItemsIDs.Add(tEnumItemID)
                                End If
                            End If
                        Next
                    End If

                    CurrentFiltersList.Add(tFilterItem)
                End If
            Next
        End If
    End Sub

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

    Private ReadOnly Property ActiveProjectHasAlternativeAttributes As Boolean
        Get
            If App IsNot Nothing AndAlso App.ActiveProject IsNot Nothing AndAlso App.ActiveProject.ProjectManager IsNot Nothing AndAlso App.ActiveProject.ProjectManager.Attributes IsNot Nothing AndAlso App.ActiveProject.ProjectManager.Attributes.AttributesList IsNot Nothing Then
                For Each attr In App.ActiveProject.ProjectManager.Attributes.AttributesList
                    If attr.Type = AttributeTypes.atAlternative AndAlso Not attr.IsDefault Then Return True
                Next
            End If
            Return False
        End Get
    End Property
    ''' <summary>
    ''' Get information about which alternative is visible (checked manually or filter applied)
    ''' </summary>
    ''' <returns>Dictionary(Of Integer, Boolean) key = Aleternative ID value = true if visible </returns>
    Public Function GetVisibleAlternatives() As Dictionary(Of Integer, Boolean)
        Dim retVal As Dictionary(Of Integer, Boolean) = New Dictionary(Of Integer, Boolean)

        Dim FilterNum As Integer = AlternativesFilterValue
        Dim combined_alts = New Dictionary(Of Integer, List(Of NodePriority))
        Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)

        'A1099 ===
        ' Filternum = -3
        'Dim tAlternativesCustomFilterValue As String = AlternativesCustomFilterValue
        Dim sAlts As String() = {}

        ' Filternum > 0
        If FilterNum > PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count Then FilterNum = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count

        If FilterNum > 0 OrElse FilterNum = -105 OrElse FilterNum = -110 OrElse FilterNum = -125 OrElse FilterNum = -2 Then
            Dim users = New List(Of Integer)
            If FilterNum = -2 Then 
                users.Add(AlternativesAdvancedFilterUserID)
            Else
                users.Add(COMBINED_USER_ID)
            End If

            If PM.IsRiskProject AndAlso PM.CalculationsManager.UseSimulatedValues Then
                Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(WRTNodeID)

                For Each userID As Integer In users
                    PM.RiskSimulations.SimulateCommon(userID, ControlsUsageMode.DoNotUse, wrtNode)

                    Dim UserObjPriorities As New List(Of Tuple(Of Integer, Integer, NodePriority))
                    Dim UserAltPriorities As New List(Of NodePriority)

                    Dim calcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(userID)

                    If Not IsCombinedUserID(userID) Then
                        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(userID))
                    End If
                    Dim JA As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ECSynthesisMode.smDistributive, PM)

                    Dim CanShowResults As Boolean = If(IsCombinedUserID(calcTarget.GetUserID), JA.CanShowGroupResults(wrtNode, CType(calcTarget.Target, clsCombinedGroup)), JA.CanShowIndividualResults(calcTarget.GetUserID, wrtNode))
                    For Each alt As clsNode In PM.ActiveAlternatives.Nodes
                        Dim nPriority As New NodePriority
                        nPriority.NodeID = alt.NodeID
                        nPriority.ParentID = -1
                        nPriority.LocalPriority = -1
                        nPriority.GlobalPriority = If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, alt.SimulatedAltLikelihood, alt.SimulatedAltImpact)
                        nPriority.NormalizedGlobalPriority = nPriority.GlobalPriority

                        'If PM.IsRiskProject AndAlso PM.CalculationsManager.ShowDueToPriorities Then
                        '    nPriority.GlobalPriority = alt.UnnormalizedPriority * WRTNodeUnnormalizedPriority
                        'End If

                        nPriority.CanShowResults = CanShowResults

                        UserAltPriorities.Add(nPriority)
                    Next
                    combined_alts.Add(userID, UserAltPriorities)
                Next

            Else
                Dim objs As New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))

                If H IsNot Nothing AndAlso H.Nodes.Count > 0 Then
                    'PM.CalculationsManager.GetAlternativesGrid(H.Nodes(0).NodeID, users, objs, combined_alts)
                    PM.CalculationsManager.GetAlternativesGrid(WRTNodeID, users, objs, combined_alts, True, True)
                End If
            End If
        End If

        If FilterNum > 0 OrElse FilterNum = -105 OrElse FilterNum = -110 OrElse FilterNum = -125 Then           
            Dim combined_alts_values = combined_alts(COMBINED_USER_ID)
            combined_alts_values = combined_alts_values.OrderByDescending(Function(f) f.GlobalPriority).ToList

            Dim listAlts As New List(Of String)

            If FilterNum > 0 Then
                For K As Integer = 0 To System.Math.Min(FilterNum, combined_alts_values.Count) - 1
                    listAlts.Add(combined_alts_values(K).NodeID.ToString)
                Next
            Else 'bottom x alts
                For K As Integer = 0 To System.Math.Min(System.Math.Abs(FilterNum + 100), combined_alts_values.Count) - 1
                    listAlts.Add(combined_alts_values(combined_alts_values.Count - K - 1).NodeID.ToString)
                Next
            End If

            sAlts = listAlts.ToArray
        End If

        ' Filternum = -2
        If FilterNum = -2 Then
            Dim tAltsNum As Integer = AlternativesAdvancedFilterValue
            Dim listAlts As New List(Of String)

            Dim combined_alts_values = combined_alts(AlternativesAdvancedFilterUserID)
            combined_alts_values = combined_alts_values.OrderByDescending(Function(f) f.GlobalPriority).ToList
            For k As Integer = 0 To System.Math.Min(tAltsNum, combined_alts_values.Count) - 1 'AS/11-19-19
                listAlts.Add(combined_alts_values(k).NodeID.ToString)
            Next
            sAlts = listAlts.ToArray
        End If
        'A1099 ==

        Dim tCurrentFiltersList = CurrentFiltersList 'A1172 ===
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        For Each alt As clsNode In altH.TerminalNodes
            'A1099 ===
            Dim isVisible As Boolean = True
            Select Case FilterNum
                Case -1
                    isVisible = True
                Case -2
                    isVisible = sAlts.Contains(alt.NodeID.ToString)
                Case -3
                    'isVisible = tAlternativesCustomFilterValue = "" OrElse sAlts.Contains(alt.NodeID.ToString)
                    isVisible = IsAltSelected(alt.NodeGuidID)
                Case -4 ' filter by Funded in RA scenario
                    isVisible = IsAltFunded(alt.NodeGuidID)
                Case -5 ' Filter by alternative attributes                    
                    isVisible = alt.IsNodeIncludedInFilter(tCurrentFiltersList)
                Case -6 ' Show risks only
                    isVisible = alt.EventType = EventType.Risk
                Case -7 ' Show opportunity only
                    isVisible = alt.EventType = EventType.Opportunity
                Case Else
                    isVisible = sAlts.Contains(alt.NodeID.ToString)
            End Select
            'A1099 ==
            retVal.Add(alt.NodeID, isVisible)
        Next

        Return retVal
    End Function

    Private ReadOnly Property IsAltFunded(AltGUID As Guid) As Boolean
        Get
            Return PM.ResourceAligner.Scenarios.ActiveScenario.GetAvailableAlternativeById(AltGUID.ToString) IsNot Nothing AndAlso CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID, AltGUID, Guid.Empty)) > 0
        End Get
    End Property

    Public Function GetSAComponentsData(wrtID As Integer) As String
        Dim retVal As String = ""

        Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        Dim ComponentsData As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = PM.CalculationsManager.GetDSAComponents(wrtID,, SAUserID)
        If ComponentsData.Count > 0 And ObjPriorities.Count > 0 And altH IsNot Nothing And H IsNot Nothing Then
            Dim wrtNode As clsNode = H.GetNodeByID(wrtID)
            If wrtNode IsNot Nothing Then
                For Each obj As clsNode In wrtNode.Children
                    If (Not PM.IsRiskProject OrElse PM.IsRiskProject AndAlso obj.RiskNodeType <> RiskNodeType.ntCategory) Then
                        For Each alt As clsNode In altH.TerminalNodes
                            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""altID"":{0},""objID"":{1},""comp"":{2}}}", alt.NodeID, obj.NodeID, JS_SafeNumber(ComponentsData(alt.NodeID)(obj.NodeID)))
                        Next
                    End If
                Next
            End If
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Private globalIdx As Integer = -1

    ''' <summary>
    ''' General function to get data for Charts and Sensitivity Analysis for dashboards panels
    ''' </summary>
    ''' <param name="wrtNodeID"></param>
    ''' <param name="dataItems">list of active dashboard panels</param>
    ''' <param name="UserIDs">list of selected users/combined groups</param>
    ''' <returns></returns>
    Public Function Synthesize(Optional wrtNodeID As Integer = -1, Optional dataItems As String() = Nothing, Optional UserIDs As List(Of Integer) = Nothing) As String
        If dataItems Is Nothing Then
            dataItems = New String() {}
        End If
        If UserIDs Is Nothing Then
            UserIDs = New List(Of Integer)
            UserIDs.Add(-1)
        End If
        UserIDsWithData = PM.StorageManager.Reader.DataExistsForUsersHashset(PM.ActiveHierarchy) 'PM.DataExistsForUsers()
        Dim sObjs As String = ""
        Dim sAlts As String = ""
        Dim sUsers As String = ""
        Dim sPriorities As String = ""
        Dim hPriorities As String = ""
        Dim testPriorities As String = ""
        Dim sProsAndCons As String = ""
        'Dim sPortfolioGrid As String = ""
        Dim objH As clsHierarchy = PM.ActiveObjectives
        Dim altH As clsHierarchy = PM.ActiveAlternatives
        Dim Goal As clsNode = objH.Nodes(0)
        If objH IsNot Nothing And altH IsNot Nothing Then
            Dim tVisibleAlternatives = GetVisibleAlternatives()
            Dim wrtNode As clsNode = objH.GetNodeByID(wrtNodeID)
            If wrtNode Is Nothing Then
                wrtNode = Goal
            End If
            'List of Objectives
            globalIdx = -1
            wrtNode.Tag = "-1"
            GetObjectivesData(PM.ActiveObjectives.GetLevelNodes(0), Nothing, sObjs, wrtNode.NodeID, wrtNode)
            'List of Alternatives
            globalIdx = 0
            sAlts = String.Format("{{""idx"":{0},""id"":{1},""name"":""{2}"",""color"":""{3}"",""visible"":{4},""event_type"":{5}}}", 0, -2, JS_SafeString("Alternatives"), "#ffffff", "1", 0)
            If Not PM.AntiguaDashboard.IsPanelLoaded Then
                PM.AntiguaDashboard.LoadPanel(ECModelStorageType.mstCanvasStreamDatabase, PM.StorageManager.Location, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, App.ActiveProject.ID)
            End If

            For Each alt As clsNode In altH.TerminalNodes
                Dim isVisible As Boolean = tVisibleAlternatives.ContainsKey(alt.NodeID) AndAlso tVisibleAlternatives(alt.NodeID) 'A1513
                Dim sAltColor As String = ""
                Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), alt.NodeID, True))
                'Dim AltVal As Double = AltValues(alt.NodeID) * NormCoef

                Dim attrValues As String = ""
                'If dataItems.Contains(ecReportItemType.AltsGrid.ToString) OrElse dataItems.Contains(ecReportItemType.Alternatives.ToString) Then  ' D7308  // always load attributes
                Dim attributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes()
                    For Each attr As clsAttribute In attributes
                        If isVisibleAttr(attr, True, PM.IsRiskProject, False) Then
                            Dim sValue As String = ""
                            Dim sName As String = JS_SafeString(attr.Name)
                            Dim sGuid As String = JS_SafeString(attr.ID.ToString)

                            If attr.ValueType = AttributeValueTypes.avtDouble Or attr.ValueType = AttributeValueTypes.avtLong Then 'AS/14488a added avtLong
                                Try 'AS/14404 incorporated Try-Catch
                                    Dim aVal As Double = CDbl(PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID))

                                    Select Case attr.ID
                                        Case ATTRIBUTE_COST_ID
                                            aVal = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Cost
                                        Case ATTRIBUTE_RISK_ID
                                            aVal = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).RiskOriginal
                                    End Select

                                    If aVal = Int32.MinValue Then
                                        sValue = " "
                                        attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{{""guid"":""{0}"", ""name"":""{1}"", ""val"":""{2}""}}", sGuid, sName, JS_SafeString(sValue))
                                    Else
                                        sValue = JS_SafeNumber(aVal)
                                        attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{{""guid"":""{0}"", ""name"":""{1}"", ""val"":{2}}}", sGuid, sName, JS_SafeString(sValue))
                                    End If

                                Catch
                                    sValue = " "
                                    attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{{""guid"":""{0}"", ""name"":""{1}"", ""val"":""{2}""}}", sGuid, sName, JS_SafeString(sValue))
                                End Try
                            Else
                                sValue = PM.Attributes.GetAttributeValueString(attr.ID, alt.NodeGuidID)
                                attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{{""guid"":""{0}"", ""name"":""{1}"", ""val"":""{2}""}}", sGuid, sName, JS_SafeString(sValue))
                            End If
                        End If
                    Next
                    'End If

                    sAlts += CStr(IIf(sAlts <> "", ",", "")) + String.Format("{{""idx"":{0},""id"":{1},""index"":""{2}"",""name"":""{3}"",""color"":""{4}"",""visible"":{5},""attr"":[{6}]}}", globalIdx, alt.NodeID, alt.Index, JS_SafeString(alt.NodeName), sAltColor, If(isVisible, "1", "0"), attrValues)
                globalIdx += 1
            Next
            'List of Users and Combined Groups
            globalIdx = 0
            Dim userList As List(Of Integer) = SelectedUsersAndGroupsIDs
            If PM.CombinedGroups IsNot Nothing AndAlso PM.CombinedGroups.GroupsList IsNot Nothing Then
                For Each grp As clsCombinedGroup In PM.CombinedGroups.GroupsList
                    Dim IsChecked As Boolean = userList.Contains(grp.CombinedUserID)
                    Dim GroupHasData As Boolean = IsGroupHasData(grp)
                    Dim sColor As String = GetPaletteColor(CurrentPaletteID(PM), grp.ID, False)
                    sUsers += CStr(IIf(sUsers <> "", ",", "")) + String.Format("{{""idx"":{0},""id"":{1},""name"":""{2}"",""email"":""{3}"",""hasdata"":{4},""checked"":{5},""color"":""{6}"", ""group"":{7}}}", globalIdx, grp.CombinedUserID, JS_SafeString("[" + grp.Name + "]"), "", Bool2JS(GroupHasData), Bool2JS(IsChecked), sColor, Bool2JS(True))
                    globalIdx += 1
                Next
            End If

            For Each usr As clsUser In PM.UsersList
                Dim IsChecked As Boolean = userList.Contains(usr.UserID)
                Dim HasData As Boolean = UserIDsWithData.Contains(usr.UserID)
                Dim sColor As String = GetPaletteColor(CurrentPaletteID(PM), usr.UserID, True)
                sUsers += CStr(IIf(sUsers <> "", ",", "")) + String.Format("{{""idx"":{0},""id"":{1},""name"":""{2}"",""email"":""{3}"",""hasdata"":{4},""checked"":{5},""color"":""{6}"", ""group"":{7}}}", globalIdx, usr.UserID, JS_SafeString(usr.UserName), JS_SafeString(usr.UserEMail), Bool2JS(HasData), Bool2JS(IsChecked), sColor, Bool2JS(False))
                globalIdx += 1
            Next

            'Objectives and Alternatives priorities for users and groups
            sPriorities = getPriorities(UserIDs, wrtNode.NodeID)
            If wrtNode.NodeID <> Goal.NodeID Then
                hPriorities = getPriorities(UserIDs, Goal.NodeID, True)
            End If


            Dim dataTypes As New List(Of Integer)
            dataTypes.Add(pdtObjs)
            dataTypes.Add(pdtAlts)
            dataTypes.Add(pdtComps)
            dataTypes.Add(pdtSA)
            'testPriorities = getUserPriorities(-1, dataTypes, , Goal.Children(0).NodeID)

            'Pros and Cons
            If dataItems.Contains(ecReportItemType.ProsAndCons.ToString) Then   ' D7208
                For Each alt As clsNode In altH.TerminalNodes
                    Dim sPros As String = ""
                    Dim sCons As String = ""
                    Dim vNode As clsVisualNode = PM.AntiguaDashboard.GetNodeByGuid(alt.NodeGuidID)
                    If vNode IsNot Nothing Then
                        Dim pros As List(Of clsVisualNode) = vNode.ProsList
                        Dim cons As List(Of clsVisualNode) = vNode.ConsList
                        If pros IsNot Nothing Then
                            For Each p As clsVisualNode In pros
                                sPros += CStr(If(sPros <> "", ",", "")) + String.Format("""{0}""", JS_SafeString(p.Text))
                            Next
                        End If

                        If cons IsNot Nothing Then
                            For Each c As clsVisualNode In cons
                                sCons += CStr(If(sCons <> "", ",", "")) + String.Format("""{0}""", JS_SafeString(c.Text))
                            Next
                        End If
                        If (sPros <> "" Or sCons <> "") Then
                            sProsAndCons += CStr(If(sProsAndCons <> "", ",", "")) + String.Format("{{""id"":{0},""pros"":[{1}],""cons"":[{2}]}}", alt.NodeID, sPros, sCons)
                        End If
                    End If
                Next
            End If
            ' -D7027
            'If dataItems.Contains("PortfolioGrid") Then
            '    RA.Load()
            '    If (RA.Solver.SolverState <> raSolverState.raSolved) Then
            '        RA.Scenarios.CheckModel(, True)
            '        RA.Solver.Solve(raSolverExport.raNone)
            '    End If
            '    For Each tAlt As RAAlternative In Scenario.Alternatives
            '        sPortfolioGrid += CStr(If(sPortfolioGrid <> "", ",", "")) + String.Format("{{""id"":{0},""name"":""{1}"",""funded"":{2},""benefit"":{3},""ebenefit"":{4},""risk"":{5},""pfailure"":{6},""cost"":{7},""must"":{8},""mustnot"":{9}}}", tAlt.SortOrder, JS_SafeString(tAlt.Name), tAlt.Funded, tAlt.BenefitOriginal, tAlt.Benefit, tAlt.Risk, tAlt.RiskOriginal, tAlt.Cost, Bool2JS(tAlt.Must), Bool2JS(tAlt.MustNot))
            '    Next
            'End If
        End If

        'Dim Result = String.Format("{{""objs"":[{0}],""alts"":[{1}],""users"":[{2}],""priorities"":[{3}],""hpriorities"":[{4}],""prosandcons"":[{5}],""wrt"":{6},""uprty"":[{7}],""portfoliogrid"":[{8}]}}", sObjs, sAlts, sUsers, sPriorities, hPriorities, sProsAndCons, wrtNodeID, testPriorities, sPortfolioGrid).Replace("\'", "'")  ' D6856
        Dim Result = String.Format("{{""objs"":[{0}],""alts"":[{1}],""users"":[{2}],""priorities"":[{3}],""hpriorities"":[{4}],""prosandcons"":[{5}],""wrt"":{6},""uprty"":[{7}]}}", sObjs, sAlts, sUsers, sPriorities, hPriorities, sProsAndCons, wrtNodeID, testPriorities).Replace("\'", "'")  ' D6856 + D7027

        Return Result
    End Function

    Private Sub GetObjectivesData(Nodes As List(Of clsNode), wrtNode As clsNode, ByRef retVal As String, selectedNodeID As Integer, wrtCalcNode As clsNode)
        If Nodes IsNot Nothing Then
            For Each tNode As clsNode In Nodes
                retVal += CStr(IIf(retVal = "", "", ",")) + AddObjectiveData(tNode, wrtNode, selectedNodeID, wrtCalcNode)
            Next
        End If
    End Sub

    Private Function AddObjectiveData(tNode As clsNode, wrtParentNode As clsNode, selectedNodeID As Integer, wrtCalcNode As clsNode) As String
        Dim sNode As String = ""
        tNode.Tag = If(wrtParentNode IsNot Nothing, wrtParentNode.NodeID.ToString, "-1")
        'For Each t As Tuple(Of Integer, Integer, clsNode) In PM.ActiveObjectives.NodesInLinearOrder
        Dim AttrGuid As Guid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID
        If PM.ActiveHierarchy = ECHierarchyID.hidImpact Then
            AttrGuid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID
        End If
        Dim sNodeColor As String = ""
        Dim tPID As Integer = If(wrtParentNode IsNot Nothing, wrtParentNode.NodeID, -1)
        Dim tNodeColor As Long = CLng(PM.Attributes.GetAttributeValue(AttrGuid, tNode.NodeGuidID))
        sNodeColor = If(tNodeColor > 0, LongToBrush(tNodeColor), GetPaletteColor(CurrentPaletteID(PM), tNode.NodeID))
        globalIdx += 1

        sNode += If(sNode = "", "", ",") + String.Format("{{""idx"":{0}, ""id"":{1}, ""key_id"":""[{1},{6}]""{7}, {2} ""name"":""{3}"", ""color"":""{4}"", ""isTerminal"":{5}, ""isCategory"":{8}, ""isChildNode"":{9}}}", globalIdx, tNode.NodeID, If(tPID = -1, "", """int_pid"":" + tPID.ToString + ","), JS_SafeString(tNode.NodeName), sNodeColor, If(tNode.IsTerminalNode, "1", "0"), tPID, If(tPID = -1, "", String.Format(", ""pid"":{0}, ""key_pid"":""[{0},{1}]""", tPID, CStr(wrtParentNode.Tag))), Bool2JS(tNode.RiskNodeType = RiskNodeType.ntCategory), Bool2JS(PM.ActiveObjectives.IsChildOf(tNode, wrtCalcNode)))
        If tNode.Children IsNot Nothing AndAlso tNode.Children.Count > 0 Then
            Dim sChildren As String = ""
            GetObjectivesData(tNode.Children, tNode, sChildren, selectedNodeID, wrtCalcNode)
            sNode += "," + sChildren
        End If
        'Next
        Return sNode
    End Function

    Const pdtObjs As Integer = 0
    Const pdtAlts As Integer = 1
    Const pdtComps As Integer = 2
    Const pdtSA As Integer = 3

    Private Function getUserPriorities(UserID As Integer, dataType As List(Of Integer), Optional wrtNodeID As Integer = -1, Optional saNodeID As Integer = -1) As String
        Dim sPriorities = ""
        Dim objH As clsHierarchy = PM.ActiveObjectives
        Dim altH As clsHierarchy = PM.ActiveAlternatives
        If objH IsNot Nothing And altH IsNot Nothing Then
            Dim wrtNode As clsNode = objH.GetNodeByID(wrtNodeID)
            If wrtNode Is Nothing Then
                wrtNode = objH.Nodes(0)
            End If
            Dim CalcTarget As clsCalculationTarget
            PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(UserID))
            If IsCombinedUserID(UserID) Then
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(UserID))
            Else
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, PM.GetUserByID(UserID))
            End If
            If CalcTarget IsNot Nothing Then
                Dim JA As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ECSynthesisMode.smDistributive, PM)
                Dim CanShowResults = If(CalcTarget.GetUserID >= 0, JA.CanShowIndividualResults(CalcTarget.GetUserID, wrtNode), Not (Not PM.UsersRoles.IsAllowedObjective(wrtNode.NodeGuidID, CalcTarget.GetUserID) AndAlso Not PM.CalculationsManager.UseCombinedForRestrictedNodes))
                If CanShowResults Then
                    Dim WRTNodeUnnormalizedPriority As Double = 0
                    If PM.IsRiskProject Then
                        PM.CalculationsManager.Calculate(CalcTarget, PM.ActiveObjectives.Nodes(0))
                        WRTNodeUnnormalizedPriority = wrtNode.UnnormalizedPriority
                    End If

                    PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
                    Dim sOP As String = ""
                    If dataType.Contains(pdtObjs) Then
                        For Each t As Tuple(Of Integer, Integer, clsNode) In PM.ActiveObjectives.NodesInLinearOrder
                            Dim node As clsNode = t.Item3
                            Dim sPrty As String = JS_SafeNumber(node.WRTGlobalPriority)
                            Dim sUPrty As String = JS_SafeNumber(node.UnnormalizedPriority)
                            Dim sLPrty As String = JS_SafeNumber(If(PM.IsRiskProject, node.LocalPriorityUnnormalized(CalcTarget), node.LocalPriorityNormalized(CalcTarget)))
                            If sPrty = "NaN" Then sPrty = "0"
                            If sUPrty = "NaN" Then sUPrty = "0"
                            If sLPrty = "NaN" Then sLPrty = "0"
                            sOP += CStr(IIf(sOP <> "", ",", "")) + String.Format("{{""oid"":{0}, ""prty"":{1}, ""uprty"":{2}, ""lprty"":{3}}}", node.NodeID, sPrty, sUPrty, sLPrty)
                        Next
                    End If

                    Dim sAP As String = ""
                    If dataType.Contains(pdtAlts) Then
                        Dim ComponentsData As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = If(dataType.Contains(pdtComps), PM.CalculationsManager.GetDSAComponents(wrtNode.NodeID, True, UserID), Nothing)
                        If ComponentsData.Count > 0 Then
                            For Each alt As clsNode In PM.ActiveAlternatives.Nodes
                                Dim sPrty As String = JS_SafeNumber(alt.WRTGlobalPriority)
                                Dim sUPrty As String = JS_SafeNumber(If(PM.IsRiskProject AndAlso PM.CalculationsManager.ShowDueToPriorities, alt.UnnormalizedPriority * WRTNodeUnnormalizedPriority, alt.UnnormalizedPriority))
                                If sPrty = "NaN" Then sPrty = "0"
                                If sUPrty = "NaN" Then sUPrty = "0"
                                Dim sComp As String = ""
                                If dataType.Contains(pdtComps) Then
                                    For Each oid In ComponentsData(alt.NodeID).Keys
                                        Dim cval = ComponentsData(alt.NodeID)(oid)
                                        If cval <> 0 Then
                                            sComp += CStr(IIf(sComp <> "", ",", "")) + String.Format("{{""oid"":{0}, ""val"":{1}}}", oid, JS_SafeNumber(cval))
                                        End If
                                    Next
                                End If
                                sAP += CStr(IIf(sAP <> "", ",", "")) + String.Format("{{""aid"":{0}, ""prty"":{1}, ""uprty"":{2}, ""comps"":[{3}]}}", alt.NodeID, sPrty, sUPrty, sComp)
                            Next
                        End If
                    End If
                        Dim sSA As String = ""
                    If dataType.Contains(pdtSA) Then
                        Dim ComponentsData As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = PM.CalculationsManager.GetDSAComponents(wrtNode.NodeID, True, UserID)
                        For Each alt As clsNode In PM.ActiveAlternatives.Nodes
                            Dim sComp As String = ""
                            If dataType.Contains(pdtComps) Then
                                For Each oid In ComponentsData(alt.NodeID).Keys
                                    Dim cval = ComponentsData(alt.NodeID)(oid)
                                    If cval <> 0 Then
                                        sComp += CStr(IIf(sComp <> "", ",", "")) + String.Format("{{""oid"":{0}, ""sa0"":{1}, ""sa1"":{2}}}", oid, JS_SafeNumber(cval), JS_SafeNumber(cval), JS_SafeNumber(cval))
                                    End If
                                Next
                            End If
                            Dim SA0 As Double = 0
                            Dim SA1 As Double = 1
                            sSA += CStr(IIf(sSA <> "", ",", "")) + String.Format("{{""aid"":{0}, ""sa0"":{1}, ""sa1"":{2}, ""comps"":[{3}]}}", alt.NodeID, JS_SafeNumber(SA0), JS_SafeNumber(SA1), sComp)
                        Next
                    End If
                    sPriorities = String.Format("{{""uid"":{0}, ""wrt"":{1}, ""objs"":[{2}], ""alts"":[{3}], ""sanodeid"":{4}, ""sa"":[{5}]}}", UserID, wrtNodeID, sOP, sAP, saNodeID, sSA)
                End If
            End If
        End If
        Return sPriorities
    End Function

    Private Function getPriorities(UserIDs As List(Of Integer), Optional wrtNodeID As Integer = -1, Optional objsPrtyOnly As Boolean = False) As String
        Dim sPriorities = ""
        Dim objH As clsHierarchy = PM.ActiveObjectives
        Dim altH As clsHierarchy = PM.ActiveAlternatives
        If objH IsNot Nothing And altH IsNot Nothing Then
            Dim wrtNode As clsNode = objH.GetNodeByID(wrtNodeID)
            If wrtNode Is Nothing Then
                wrtNode = objH.Nodes(0)
            End If

            'Objectives and Alternatives priorities for users and groups
            Dim UserObjPriorities As Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
            Dim UserAltPriorities As Dictionary(Of Integer, List(Of NodePriority))
            Dim UserObjPriorities2 As Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
            PM.CalculationsManager.GetAlternativesGrid(objH.Nodes(0).NodeID, UserIDs, UserObjPriorities, UserAltPriorities, True, wrtNode Is objH.Nodes(0))

            For Each UserID As Integer In UserIDs
                Dim objPriority As List(Of Tuple(Of Integer, Integer, NodePriority))
                objPriority = UserObjPriorities(UserID)

                Dim sOP As String = ""
                'sOP += CStr(IIf(sOP <> "", ",", "")) + String.Format("{{""oid"":{0},""prty"":{1},""uprty"":{2},""lprty"":{3}}}", objH.Nodes(0).NodeID, 1, 1, 1)
                For Each t As Tuple(Of Integer, Integer, NodePriority) In objPriority
                    Dim nodePrty As NodePriority = t.Item3
                    If nodePrty.CanShowResults Then
                        Dim sPrty As String = JS_SafeNumber(nodePrty.NormalizedGlobalPriority)
                        Dim sUPrty As String = JS_SafeNumber(nodePrty.GlobalPriority)
                        Dim sLPrty As String = JS_SafeNumber(nodePrty.LocalPriority)
                        If sPrty = "NaN" Then sPrty = "0"
                        If sUPrty = "NaN" Then sUPrty = "0"
                        If sLPrty = "NaN" Then sLPrty = "0"
                        sOP += CStr(IIf(sOP <> "", ",", "")) + String.Format("{{""oid"":{0},""prty"":{1},""uprty"":{2},""lprty"":{3}}}", t.Item1, sPrty, sUPrty, sLPrty)
                    End If
                Next

                Dim sAP As String = ""
                If Not objsPrtyOnly Then
                    If wrtNode IsNot objH.Nodes(0) Then
                        PM.CalculationsManager.GetAlternativesGrid(wrtNode.NodeID, UserIDs, UserObjPriorities2, UserAltPriorities, True, True)
                    End If
                    Dim altPriority As List(Of NodePriority) = UserAltPriorities(UserID)
                    For Each altPrty As NodePriority In altPriority
                        Dim sPrty As String = JS_SafeNumber(altPrty.NormalizedGlobalPriority)
                        Dim sUPrty As String = JS_SafeNumber(altPrty.GlobalPriority)
                        If sPrty = "NaN" Then sPrty = "0"
                        If sUPrty = "NaN" Then sUPrty = "0"
                        sAP += CStr(IIf(sAP <> "", ",", "")) + String.Format("{{""aid"":{0},""prty"":{1},""uprty"":{2}}}", altPrty.NodeID, sPrty, sUPrty)
                    Next
                End If

                Dim sComp = ""
                If Not objsPrtyOnly Then '' AndAlso Not (PM.IsRiskProject AndAlso PM.CalculationsManager.UseSimulatedValues) Then ?
                    Dim ComponentsData As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = PM.CalculationsManager.GetDSAComponents(wrtNode.NodeID, True, UserID)
                    For Each aid In ComponentsData.Keys
                        For Each oid In ComponentsData(aid).Keys
                            Dim cval = ComponentsData(aid)(oid)
                            If cval <> 0 Then
                                sComp += CStr(IIf(sComp <> "", ",", "")) + String.Format("{{""aid"":{0},""oid"":{1},""val"":{2}}}", aid, oid, JS_SafeNumber(cval))
                            End If
                        Next
                    Next
                End If
                sPriorities += CStr(IIf(sPriorities <> "", ",", "")) + String.Format("{{""uid"":{0},""objs"":[{1}],""alts"":[{2}],""comps"":[{3}],""expval"":{4}}}", UserID, sOP, sAP, sComp, 0)
            Next
        End If
        Return sPriorities
    End Function

    Private Sub Normalize(ByRef AllAlts As Dictionary(Of Integer, List(Of NodePriority)), tVisibleAlternatives As Dictionary(Of Integer, Boolean))
        If PM.Parameters.Normalization = LocalNormalizationType.ntNormalizedMul100 OrElse PM.Parameters.Normalization = LocalNormalizationType.ntNormalizedSum100 Then
            For Each UserAlts In AllAlts
                If UserAlts.Value.Count > 0 Then
                    Dim total As Double = 0
                    If PM.Parameters.Normalization = LocalNormalizationType.ntNormalizedMul100 Then total = UserAlts.Value.Max(Function(node) node.GlobalPriority)
                    If PM.Parameters.Normalization = LocalNormalizationType.ntNormalizedSum100 Then total = UserAlts.Value.Sum(Function(node) If(tVisibleAlternatives.ContainsKey(node.NodeID) AndAlso tVisibleAlternatives(node.NodeID), node.GlobalPriority, 0))
                    If total = 0 Then total = 1
                    For Each v In UserAlts.Value
                        v.NormalizedGlobalPriority = v.GlobalPriority / total
                    Next
                End If
            Next
        End If
    End Sub

    Private Function GetExpectedValue(UserID As Integer, alts As Dictionary(Of Integer, List(Of NodePriority)), ByRef CanShowHiddenExpectedValue As Boolean) As Double
        Dim retVal As Double = 0
        If alts.ContainsKey(UserID) Then
            For Each alt In alts(UserID)
                Dim prjAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(alt.NodeID)
                If prjAlt IsNot Nothing Then
                    Dim s As String() = prjAlt.NodeName.Split(CChar(" "))
                    If s.Length > 0 Then
                        Dim d As Double = 0
                        If String2Double(s(0), d) Then
                            CanShowHiddenExpectedValue = True
                            retVal += d * alt.GlobalPriority
                        End If
                    End If
                End If
            Next
        End If
        Return retVal
    End Function

    Dim objs As Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority))) = Nothing
    ''' <summary>
    ''' Get Alternatives and Objectives priorities for overall results grids with expected values if applicable
    ''' </summary>
    ''' <param name="UserIDs">List of users/groups combined IDs</param>
    ''' <returns></returns>
    Public Function get_alternatives_and_objectves_priorities(Optional UserIDs As List(Of Integer) = Nothing) As String
        Dim retVal As String = ""
        Dim ObjsPrioritiesData = ""
        Dim ExpectedValuesData = ""

        Dim alts As New Dictionary(Of Integer, List(Of NodePriority))
        objs = New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
        Dim users As List(Of Integer) = If(UserIDs Is Nothing, SelectedUsersAndGroupsIDs, UserIDs)

        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(WRTNodeID)

        If PM.IsRiskProject AndAlso PM.CalculationsManager.UseSimulatedValues Then

            For Each userID As Integer In users

                PM.RiskSimulations.SimulateCommon(userID, ControlsUsageMode.DoNotUse, wrtNode)

                Dim UserObjPriorities As New List(Of Tuple(Of Integer, Integer, NodePriority))
                Dim UserAltPriorities As New List(Of NodePriority)

                Dim calcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(userID)

                If Not IsCombinedUserID(userID) Then
                    PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(userID))
                End If
                Dim JA As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ECSynthesisMode.smDistributive, PM)

                Dim CanShowResults As Boolean = If(IsCombinedUserID(calcTarget.GetUserID), JA.CanShowGroupResults(wrtNode, CType(calcTarget.Target, clsCombinedGroup)), JA.CanShowIndividualResults(calcTarget.GetUserID, wrtNode))
                For Each alt As clsNode In PM.ActiveAlternatives.Nodes
                    Dim nPriority As New NodePriority
                    nPriority.NodeID = alt.NodeID
                    nPriority.ParentID = -1
                    nPriority.LocalPriority = -1
                    nPriority.GlobalPriority = If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, alt.SimulatedAltLikelihood, alt.SimulatedAltImpact)
                    nPriority.NormalizedGlobalPriority = nPriority.GlobalPriority

                    'If PM.IsRiskProject AndAlso PM.CalculationsManager.ShowDueToPriorities Then
                    '    nPriority.GlobalPriority = alt.UnnormalizedPriority * WRTNodeUnnormalizedPriority
                    'End If

                    nPriority.CanShowResults = CanShowResults

                    UserAltPriorities.Add(nPriority)
                Next
                alts.Add(userID, UserAltPriorities)

                For Each node As clsNode In PM.ActiveObjectives.Nodes
                    Dim nPriority As New NodePriority
                    nPriority.NodeID = node.NodeID
                    nPriority.ParentID = -1
                    nPriority.LocalPriority = -1
                    nPriority.GlobalPriority = node.SimulatedPriority
                    nPriority.NormalizedGlobalPriority = nPriority.GlobalPriority

                    nPriority.CanShowResults = CanShowResults

                    UserObjPriorities.Add(New Tuple(Of Integer, Integer, NodePriority)(node.NodeID, WRTNode.NodeID, nPriority))
                Next
                objs.Add(userID, UserObjPriorities)
            Next

        Else
            PM.CalculationsManager.GetAlternativesGrid(WRTNodeID, users, objs, alts, True, True)
        End If

        ' Calculate expected values before normalizing and filtering
        If App.ActiveProject.ProjectManager.PipeParameters.ShowExpectedValueGlobal Then
            For Each uid As Integer In users
                Dim CanShowHiddenExpectedValue As Boolean = False
                ExpectedValuesData += CStr(IIf(ExpectedValuesData = "", "", ",")) + String.Format("[{0},{1},{2}]", uid, JS_SafeNumber(GetExpectedValue(uid, alts, CanShowHiddenExpectedValue)), CStr(IIf(CanShowHiddenExpectedValue, "1", "0")))
            Next
        End If

        Dim tNormMode As LocalNormalizationType = PM.Parameters.Normalization

        Dim tVisibleAlternatives As Dictionary(Of Integer, Boolean) = GetVisibleAlternatives() 'A1513
        Normalize(alts, tVisibleAlternatives)

        If alts.Count > 0 AndAlso alts.First.Value.Count > 0 Then
            For i As Integer = 0 To alts.First.Value.Count - 1
                If tVisibleAlternatives.ContainsKey(alts.First.Value(i).NodeID) AndAlso tVisibleAlternatives(alts.First.Value(i).NodeID) Then
                    Dim row As String = alts.First.Value(i).NodeID.ToString + ",0" ' + If(tAlt.Enabled, "false", "true")
                    For Each alt As KeyValuePair(Of Integer, List(Of NodePriority)) In alts
                        Dim altPriority As Double
                        If tNormMode = LocalNormalizationType.ntUnnormalized Then
                            altPriority = alt.Value(i).GlobalPriority
                        Else
                            altPriority = alt.Value(i).NormalizedGlobalPriority
                        End If
                        row += ",[" + CStr(IIf(alt.Value(i).CanShowResults, 1, 0)) + "," + JS_SafeNumber(altPriority) + "]"
                    Next
                    retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0}]", row)
                End If
            Next
        End If

        If objs.Count > 0 AndAlso objs.First.Value.Count > 0 Then
            For Each t As Tuple(Of Integer, Integer, clsNode) In PM.Hierarchy(PM.ActiveHierarchy).NodesInLinearOrder
                Dim NodeId As Integer = t.Item1
                Dim ParentNodeID As Integer = t.Item2
                If objs.First.Value.Find(Function(x) x.Item1 = NodeId AndAlso x.Item2 = ParentNodeID) IsNot Nothing Then
                    Dim row As String = NodeId.ToString + ",0"
                    For Each obj As KeyValuePair(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority))) In objs
                        Dim nPriority As NodePriority = obj.Value.Find(Function(x) x.Item1 = NodeId AndAlso x.Item2 = ParentNodeID).Item3
                        row += ",[" + If(nPriority.CanShowResults, 1, 0).ToString + "," + JS_SafeNumber(nPriority.GlobalPriority) + "," + JS_SafeNumber(nPriority.LocalPriority) + "]"
                    Next
                    ObjsPrioritiesData += CStr(IIf(ObjsPrioritiesData = "", "", ",")) + String.Format("[{0}]", row)
                End If
            Next
        End If

        Return String.Format("{{""alts_priorities"":[{0}], ""objs_priorities"":[{1}], ""expected_values"":[{2}]}}", retVal, ObjsPrioritiesData, ExpectedValuesData)
    End Function
    
#Region "Hierarchy Tree View / CV Tree View"

    Public Property WRTNodeID(Optional AllowTerminalNodes As Boolean = True) As Integer
        Get
            Dim Node As clsNode = PM.ActiveObjectives.GetNodeByID(PM.Parameters.Synthesis_WRTNodeID)

            If CurrentPageID = _PGID_ANALYSIS_CONSENSUS OrElse Node Is Nothing Then
                WRTNodeParentGUID = ""
                Return PM.ActiveObjectives.Nodes(0).NodeID
            End If

            If Not AllowTerminalNodes AndAlso Node.IsTerminalNode AndAlso Node.ParentNode IsNot Nothing Then 
                WRTNodeParentGUID = If(Node.ParentNode.ParentNode IsNot Nothing, Node.ParentNode.ParentNode.NodeGuidID.ToString, "")
                Return Node.ParentNode.NodeID
            End If

            Return PM.Parameters.Synthesis_WRTNodeID
        End Get
        Set(value As Integer)
            PM.Parameters.Synthesis_WRTNodeID = value
        End Set
    End Property

    Public Property WRTNodeParentGUID As String
        Get
            Return PM.Parameters.WRTNodeParentGUID
        End Get
        Set(value As String)
            If (PM.Parameters.WRTNodeParentGUID <> value) Then
                PM.Parameters.WRTNodeParentGUID = value
                PM.Parameters.Save()
            End If
        End Set
    End Property

    Public Function GridWRTNodeIsTerminal() As Boolean
        Dim ID As Integer = WRTNodeID
        Dim node As clsNode = PM.ActiveObjectives.GetNodeByID(ID)
        Return node IsNot Nothing AndAlso node.IsTerminalNode
    End Function

    Public Function GridWRTNodePID() As Integer
        Dim ID As Integer = WRTNodeID
        Dim node As clsNode = PM.ActiveObjectives.GetNodeByID(ID)
        Return If(node IsNot Nothing AndAlso node.ParentNode IsNot Nothing, node.ParentNode.NodeID, -1)
    End Function

    Private Function AddTreeNodeData(tNode As clsNode, wrtParentNode As clsNode, CheckedNodes As String(), users As List(Of Integer), objs As Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority))), IsSensitivityView As Boolean) As String
        Dim sCaption = tNode.NodeName
        Dim lblNA As String = "'N/A'"

        Dim sClassName As String = "{ 'color':'black', 'font-weight':'normal' }" '"ztree_tnn"
        If tNode.RiskNodeType = RiskNodeType.ntCategory Then
            sClassName = "{ 'color':'blue', 'font-weight':'bold' }" '"ztree_cat"
        Else
            If IsSensitivityView AndAlso tNode.Children.Count = 0 AndAlso tNode.NodeID <> PM.Hierarchy(PM.ActiveHierarchy).Nodes(0).NodeID Then
                sClassName = "{ 'color': 'grey' }" '"grey-text"
            End If
        End If

        If Not tNode.Enabled Then
            sClassName = "{ 'color': '#a8a8a8' }" '"grey-text"
        End If

        Dim sName As String = "<span class='" + sClassName + " text-overflow'>" + SafeFormString(sCaption) + "</span>"
        Dim sID As String = String.Format("['{0}','{1}']", tNode.NodeGuidID, If(wrtParentNode Is Nothing, "", wrtParentNode.NodeGuidID.ToString))
        Dim isVisible As Boolean = True
        If CheckedNodes IsNot Nothing AndAlso Not CheckedNodes.Contains(CStr(tNode.NodeID)) Then isVisible = False
        Dim sNode As String = "{name:""" + JS_SafeString(tNode.NodeName) + """,title:""" + JS_SafeString(tNode.NodeName) + """,font:" + sClassName + ",open:true,nocheck:false,checked:" + Bool2JS(isVisible) + ",nodeid:" + tNode.NodeID.ToString + ",id:""" + sID + """,sortOrder:" + CVTreeItemSortOrder.ToString + ",isterminal:" + Bool2JS(tNode.IsTerminalNode) + ",iscategory:" + Bool2JS(tNode.RiskNodeType = RiskNodeType.ntCategory) + ",items:["

        CVTreeItemSortOrder += 1

        If tNode.Children IsNot Nothing AndAlso tNode.Children.Count > 0 Then
            Dim sChildren As String = ""
            GetTreeNodeData(tNode.Children, tNode, sChildren, CheckedNodes, users, objs, IsSensitivityView)
            sNode += sChildren
        End If
        sNode += "]"

        Dim sPriorities As String = ""
        For Each iUser As Integer In users
            Dim localPrty As Double = 0
            Dim globalPrty As Double = 0
            If tNode.RiskNodeType = RiskNodeType.ntCategory OrElse tNode.ParentNodes Is Nothing OrElse tNode.ParentNodes.Count = 0 Then
                localPrty = UNDEFINED_INTEGER_VALUE + 1
                globalPrty = UNDEFINED_INTEGER_VALUE + 1
            Else
                Dim nPriority As NodePriority = Nothing
                Dim item = objs(iUser).Find(Function(x) x.Item1 = tNode.NodeID AndAlso x.Item2 = wrtParentNode.NodeID)
                If item IsNot Nothing then nPriority = item.Item3
                localPrty = If(nPriority IsNot Nothing AndAlso nPriority.CanShowResults, nPriority.LocalPriority, UNDEFINED_INTEGER_VALUE)
                globalPrty = If(nPriority IsNot Nothing AndAlso nPriority.CanShowResults, nPriority.GlobalPriority, UNDEFINED_INTEGER_VALUE)

            End If
            sPriorities += String.Format(",""{0}l{1}"":{2},""{0}g{1}"":{3}", If(iUser >= 0, "u", "g"), iUser, JS_SafeNumber(Math.Round(localPrty, PM.Parameters.DecimalDigits + 2)), JS_SafeNumber(Math.Round(globalPrty, PM.Parameters.DecimalDigits + 2)))
        Next
        sNode += sPriorities
        sNode += "}"

        Return sNode
    End Function

    Private _HierarchyTerminalNodes As List(Of clsNode) = Nothing
    Public ReadOnly Property HierarchyTerminalNodes As List(Of clsNode)
        Get
            If _HierarchyTerminalNodes Is Nothing Then
                _HierarchyTerminalNodes = PM.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
            End If
            Return _HierarchyTerminalNodes
        End Get
    End Property

    Public Function GetCoveringSources() As String
        Dim retVal As String = ""
        For Each node As clsNode In HierarchyTerminalNodes
            retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""name"":""{1}"",""info"":""{2}""}}", node.NodeID, JS_SafeString(node.NodeName), JS_SafeString(ShortString(Infodoc2Text(PRJ, node.InfoDoc, True), 350, True)))
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Private Sub GetTreeNodeData(Nodes As List(Of clsNode), wrtNode As clsNode, ByRef retVal As String, CheckedNodes As String(), users As List(Of Integer), objs As Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority))), IsSensitivityView As Boolean)
        If Nodes IsNot Nothing Then
            For Each tNode As clsNode In Nodes
                retVal += CStr(IIf(retVal = "", "", ",")) + AddTreeNodeData(tNode, wrtNode, CheckedNodes, users, objs, IsSensitivityView)
            Next
        End If
    End Sub
    ''' <summary>
    ''' Get Alternatives list in form of JSON Array
    ''' Items in array: AltID, AltName, AltColor, IsAltSelected, AltEnabled, AltIndex, "Risk"/"Opportunity", [CovObjIDs]
    ''' </summary>
    ''' <returns></returns>
    Public Function GetAlternativesData() As String
        Dim retVal As String = ""
        Dim H As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        If H IsNot Nothing AndAlso H.Nodes.Count > 0 Then
            Dim tOpportunity As String = "Opportunity"
            Dim tRisk As String = "Risk"
            For Each alt As clsNode In H.Nodes
                Dim sAltColor As String = ""
                Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                If altColor > 0 Then
                    sAltColor = LongToBrush(altColor)
                End If
                'retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},'{1}','{2}',{3},{4},'{5}']", alt.NodeID, JS_SafeString(alt.NodeName), "#cfc", CStr(IIf(IsAltSelected(alt.NodeGuidID), 1, 0)), CStr(IIf(alt.Enabled, 1, 0)), alt.Index)
                Dim covObjIDs As String = ""
                If PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then
                    Dim covObjs As New List(Of clsNode)
                    For Each node In HierarchyTerminalNodes
                        Dim contributedAlternatives = node.GetContributedAlternatives
                        If contributedAlternatives IsNot Nothing AndAlso contributedAlternatives.Contains(alt) AndAlso Not covObjs.Contains(node) Then covObjs.Add(node)
                    Next
                    For Each node In covObjs
                        covObjIDs += If(covObjIDs = "", "", ",") + HierarchyTerminalNodes.IndexOf(node).ToString
                    Next
                End If
                retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},""{1}"",""{2}"",{3},{4},""{5}"",""{6}"",[{7}]]", alt.NodeID, JS_SafeString(alt.NodeName), sAltColor, CStr(IIf(IsAltSelected(alt.NodeGuidID), 1, 0)), CStr(IIf(alt.Enabled, 1, 0)), alt.Index, If(alt.EventType = EventType.Risk, tRisk, tOpportunity), covObjIDs)
            Next
        End If
        Return String.Format("[{0}]", retVal)
    End Function


    Private CVTreeItemSortOrder As Integer = 0
    ''' <summary>
    ''' Get DataSource for Consensus View Tree in form of JSON Array
    ''' </summary>
    ''' <param name="IsSensitivityView">set true if you need to disable covering objectives for the tree</param>
    ''' <param name="lblTreeNodeNoSources">set title for "No Sources" node in the tree</param>
    ''' <returns></returns>
    Public Function GetCVTreeNodesData(IsSensitivityView As Boolean, lblTreeNodeNoSources As String) As String
        Dim retVal As String = ""

        CVTreeItemSortOrder = 0

        Dim users As List(Of Integer)
        If IsSensitivityView Then
            users = New List(Of Integer)
            users.Add(SAUserID)
        Else
            users = SelectedUsersAndGroupsIDs
        End If

        Dim objsContainsAllKeys As Boolean = False
        If objs IsNot Nothing Then
            For each UserID As Integer In users
                If Not objs.ContainsKey(UserID) Then 
                    objsContainsAllKeys = False 
                    Exit For
                End If
            Next
        End If

        If objs Is Nothing OrElse Not objsContainsAllKeys OrElse WRTNodeID <> PM.ActiveObjectives.Nodes(0).NodeID Then 
            Dim alts As New Dictionary(Of Integer, List(Of NodePriority))
            objs = New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
            PM.CalculationsManager.GetAlternativesGrid(PM.ActiveObjectives.Nodes(0).NodeID, users, objs, alts, True, False)
        End If

        'If SynthesisView <> SynthesisViews.vtMixed Then
        Dim CheckedNodes As String() = Nothing
        If CVCheckedNodes IsNot Nothing Then CheckedNodes = CVCheckedNodes.Split(CChar(","))

        GetTreeNodeData(PM.Hierarchy(PM.ActiveHierarchy).GetLevelNodes(0), Nothing, retVal, CheckedNodes, users, objs, IsSensitivityView)

        If App.isRiskEnabled AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood AndAlso PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetUncontributedAlternatives.Count > 0 Then
            Dim EventsWithNoSources As clsNode = New clsNode() With {.NodeName = lblTreeNodeNoSources.ToUpper, .NodeID = -2}
            retVal += CStr(IIf(retVal = "", "", ",")) + AddTreeNodeData(EventsWithNoSources, Nothing, CheckedNodes, users, objs, IsSensitivityView)
        End If
        'End If

        Return String.Format("[{0}]", retVal)
    End Function

#End Region
    ''' <summary>
    ''' Set Dashboard chart option
    ''' </summary>
    ''' <param name="OptionName">Option Name</param>
    ''' <param name="OptionValue">Option Value as string</param>
    Public Sub SetOption(OptionName As String, OptionValue As String)
        Dim sOptions As String = PM.Parameters.Dashboard_ChartOptions
        Dim tOptions As Newtonsoft.Json.Linq.JObject = CType(JsonConvert.DeserializeObject(Of Object)(sOptions), Newtonsoft.Json.Linq.JObject)
        Dim fChanged As Boolean = False

        If OptionName <> "" AndAlso OptionValue <> "" Then
            Dim tIntegerValue As Integer
            If Integer.TryParse(OptionValue, tIntegerValue) Then
                If tOptions.Property(OptionName) IsNot Nothing Then
                    tOptions.Property(OptionName).Value = tIntegerValue
                Else
                    tOptions.Add(OptionName, tIntegerValue)
                End If
                fChanged = True
            Else
                Dim tBooleanValue As Boolean
                If Str2Bool(OptionValue, tBooleanValue) Then
                    If tOptions.Property(OptionName) IsNot Nothing Then
                        tOptions.Property(OptionName).Value = tBooleanValue
                    Else
                        tOptions.Add(OptionName, tBooleanValue)
                    End If
                    fChanged = True
                Else
                    If tOptions.Property(OptionName) IsNot Nothing Then
                        tOptions.Property(OptionName).Value = OptionValue
                    Else
                        tOptions.Add(OptionName, OptionValue)
                    End If
                    fChanged = True
                End If
            End If
            If OptionName = "WRTNodeID" Then
                Dim Node = PM.ActiveObjectives.GetNodeByID(CInt(OptionValue))
                If Node IsNot Nothing Then
                    PM.Parameters.WRTNodeParentGUID = If(Node.ParentNode IsNot Nothing, Node.ParentNode.NodeGuidID.ToString, "")
                    PM.Parameters.Save()
                End If
            End If
            If OptionName = "useSimulatedValues" Then
                PM.CalculationsManager.UseSimulatedValues = Str2Bool(OptionValue)
                PM.Parameters.Save()
            End If
        End If

        '' D6890 ===
        'If OptionName = "WRTNodeID" Then
        '    If Integer.TryParse(OptionValue, GridWRTNodeID) Then
        '        SAWRTNodeID = GridWRTNodeID
        '        PM.Parameters.Save()
        '    End If
        'End If
        '' D6890 ==
        If fChanged Then
            PM.Parameters.Dashboard_ChartOptions = JsonConvert.SerializeObject(tOptions)
            PM.Parameters.Save()
        End If
    End Sub

    ' D6625 ===
    ''' <summary>
    ''' Get Dashboard Chart options
    ''' </summary>
    ''' <returns></returns>
    Public Function Options() As String
        Return PM.Parameters.Dashboard_ChartOptions
    End Function

#Region "GemBox"
    ''' <summary>
    ''' Download XLSX GemBox report
    ''' </summary>
    ''' <param name="sType"></param>
    Public Sub Download_GemBox_XLSX(Optional sType As String = "")
        Dim sFName As String = File_CreateTempName()
        Dim sDLName As String = GetProjectFileName(App.ActiveProject.ProjectName, "", "", ".xlsx")  ' D6593

        GemBox.Spreadsheet.SpreadsheetInfo.SetLicense("SN-2019Oct23-AKAqJAF7VQbNwpzNxMw+YMGUPQKtiac8SV2w3r4AR43N/ctUteUub15BB7pw2rqTqZB6PzdDiG4vHXzKw5SRVIMv9Gw==A")

        Dim workbookGB As ExcelFile = New ExcelFile 'AS/17533f
        sFName = Replace(sFName, ".tmp", ".xlsx") 'AS/17533f
        CreateGemBoxSpreadsheetWorkbook(sFName, workbookGB)
        DownloadFile(sFName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sDLName, dbObjectType.einfProjectReport, App.ProjectID, sType <> "open")  ' D6593
    End Sub
    ' D6625 ==
    ''' <summary>
    ''' Download DOCX GemBox report
    ''' </summary>
    ''' <param name="sType"></param>
    Public Sub Download_GemBox_DOCX(Optional sType As String = "")
        Dim sFName As String = File_CreateTempName()
        sFName = Replace(sFName, ".tmp", ".docx") 'AS/17533a
        Dim sDLName As String = GetProjectFileName(App.ActiveProject.ProjectName, "", "", ".docx")  ' D6593

        GemBox.Document.ComponentInfo.SetLicense("DN-2019Oct23-o9cPV8FqJpXV/58cTjlRakTEbT0YmbK2qfwJS4OkrVTGqcdjqxHlgRDfWwwv2q3j9qruUP258vApmepOW0Z46sRTc6Q==A") 'AS/17533a 'AS/17533b added GemBox.Document. because of the error is ambiguous, imported from the namespaces or types 'DocumentFormat.OpenXml.Packaging, GemBox.Presentation'." caused by the fact that we have references both to OpenXml and Gembox
        Dim docGB As DocumentModel = New DocumentModel 'AS/17533a
        CreateGemBoxWordDocument(sFName, docGB)
        DownloadFile(sFName, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", sDLName, dbObjectType.einfProjectReport, App.ProjectID, sType <> "open")    ' D6593
    End Sub
    ''' <summary>
    ''' Download PPTX GemBox report
    ''' </summary>
    ''' <param name="sType"></param>
    Public Sub Download_GemBox_PPTX(Optional sType As String = "")
        Dim sFName As String = File_CreateTempName()
        sFName = Replace(sFName, ".tmp", ".pptx") 'AS/17533b===
        Dim sDLName As String = GetProjectFileName(App.ActiveProject.ProjectName, "", "", ".pptx")  ' D6593

        GemBox.Presentation.ComponentInfo.SetLicense("PN-2019Oct23-7esqnSuMJVQEFrRWgpgmjvPBQJyxMtrJ+gyZpUocTKpiNFnadOxsvCrZJYM5/nk1w+KbQKspn1xCUQDmPWsvtZqJJNQ==A")
        Dim presentationGB As GemBox.Presentation.PresentationDocument = New GemBox.Presentation.PresentationDocument() 'AS/17533b==
        CreateGemBoxPresentation(sFName, presentationGB)
        DownloadFile(sFName, "application/vnd.openxmlformats-officedocument.presentationml.presentation", sDLName, dbObjectType.einfProjectReport, App.ProjectID, sType <> "open")  ' D6593
    End Sub

    ' D6426 ===
    ''' <summary>
    ''' Download GemBox PDF report
    ''' </summary>
    ''' <param name="sType"></param>
    Public Sub Download_GemBox_PDF(Optional sType As String = "")
        Dim sFName As String = File_CreateTempName()
        sFName = Replace(sFName, ".tmp", ".pdf")
        Dim sDLName As String = GetProjectFileName(App.ActiveProject.ProjectName, "", "", ".pdf")  ' D6593

        GemBox.Pdf.ComponentInfo.SetLicense("AZNK-TASR-VC9J-1SFL")
        'Dim pdfGB As GemBox.Pdf.PdfDocument = New GemBox.Pdf.PdfDocument() 'AS/17533t===
        'CreateGemBoxPDF(sFName, pdfGB)
        GemBox.Document.ComponentInfo.SetLicense("DN-2019Oct23-o9cPV8FqJpXV/58cTjlRakTEbT0YmbK2qfwJS4OkrVTGqcdjqxHlgRDfWwwv2q3j9qruUP258vApmepOW0Z46sRTc6Q==A") 'AS/17533a 'AS/17533b added GemBox.Document. because of the error is ambiguous, imported from the namespaces or types 'DocumentFormat.OpenXml.Packaging, GemBox.Presentation'." caused by the fact that we have references both to OpenXml and Gembox
        Dim docGB As DocumentModel = New DocumentModel 'AS/17533a
        CreateGemBoxWordDocument(sFName, docGB) 'AS/17533t==

        DownloadFile(sFName, "application/pdf", sDLName, dbObjectType.einfProjectReport, App.ProjectID, sType <> "open")    ' D6593
    End Sub
    ' D6426 ==

    ' D6493 ===
    ''' <summary>
    ''' Download PDF GemBox report with embed Image
    ''' </summary>
    ''' <param name="sType"></param>
    Private Sub Download_GemBox_PDF_Img(Optional sType As String = "")
        Dim sFName As String = File_CreateTempName()
        sFName = Replace(sFName, ".tmp", ".pdf")
        Dim sDLName As String = GetProjectFileName(App.ActiveProject.ProjectName, "", "", ".pdf")  ' D6593

        GemBox.Pdf.ComponentInfo.SetLicense("AZNK-TASR-VC9J-1SFL")
        GemBox.Document.ComponentInfo.SetLicense("DN-2019Oct23-o9cPV8FqJpXV/58cTjlRakTEbT0YmbK2qfwJS4OkrVTGqcdjqxHlgRDfWwwv2q3j9qruUP258vApmepOW0Z46sRTc6Q==A")
        'Dim docGB As DocumentModel = New DocumentModel
        'CreateGemBoxWordDocument(sFName, docGB)

        'Using pdfdoc As New PdfDocument()
        'pdfdoc.Pages.Add()

        Dim doc = New DocumentModel()
        Dim sec = New Section(doc)

        sec.Blocks.Add(New Paragraph(doc, New GemBox.Document.Run(doc, App.ActiveProject.ProjectName)))

        Dim sIMG As String = GetParam(_Page.Params, "img", True)
        If Not String.IsNullOrEmpty(sIMG) Then
            Dim sFile As String = Server.MapPath(sIMG)
            If MyComputer.FileSystem.FileExists(sFile) Then
                Dim pic = New GemBox.Document.Picture(doc, sFile)
                Dim pgSize As Size = New Size(sec.PageSetup.PageWidth - sec.PageSetup.PageMargins.Left - sec.PageSetup.PageMargins.Right, sec.PageSetup.PageHeight - sec.PageSetup.PageMargins.Top - sec.PageSetup.PageMargins.Bottom)
                Dim KX = pic.Layout.Size.Width / pic.Layout.Size.Height
                pic.Layout = New InlineLayout(New Size(pgSize.Width, pgSize.Width / KX)) With {.LockAspectRatio = True}
                sec.Blocks.Add(New Paragraph(doc, pic))
                doc.Sections.Add(sec)
            End If
        End If
        doc.Save(sFName, GemBox.Document.SaveOptions.PdfDefault)
        'pdfdoc.Save(sFName)
        'End Using
        DownloadFile(sFName, "application/pdf", sDLName, dbObjectType.einfProjectReport, App.ProjectID, sType <> "open")    ' D6593
    End Sub
    ' D6493 ==


#End Region

#Region "SA"
    Dim ObjPriorities As New Dictionary(Of Integer, Double)
    Dim AltValues As New Dictionary(Of Integer, Double)
    Dim AltValuesInOne As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
    Dim AltValuesInZero As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

    Public Property WRTState As ECWRTStates
        Get
            Return CType(CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_WRT_STATE_ID, UNDEFINED_USER_ID)), ECWRTStates)
        End Get
        Set(value As ECWRTStates)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_WRT_STATE_ID, AttributeValueTypes.avtLong, CInt(value))
        End Set
    End Property

    Public Property ShowComponents As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_SHOW_COMPONENTS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_SHOW_COMPONENTS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property SAUserID As Integer
        Get
            Dim tUserID As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_SA_USER_ID, UNDEFINED_USER_ID))
            If PM.UserExists(tUserID) OrElse PM.CombinedGroups.CombinedGroupExists(tUserID) Then Return tUserID
            Return COMBINED_USER_ID
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_SA_USER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Private ReadOnly Property SESS_CV_CHECKED_NODES As String
        Get
            Return String.Format("SESS_CV_CHECKED_NODES_{0}_{1}", App.ActiveProject.ID.ToString, App.ActiveProject.ProjectManager.ActiveHierarchy) 'A1312
        End Get
    End Property
    Private Property CVCheckedNodes As String
        Get
            Return SessVar(SESS_CV_CHECKED_NODES)
        End Get
        Set(value As String)
            SessVar(SESS_CV_CHECKED_NODES) = value
        End Set
    End Property

    Public Sub initSAData(Optional wrtID As Integer = -1, Optional RiskParam As Integer = 0)
        If ObjPriorities.Count = 0 Then
            Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
            If H IsNot Nothing Then
                Dim wrtNode As clsNode = H.GetNodeByID(If(wrtID = -1, WRTNodeID(False), wrtID))
                If wrtNode IsNot Nothing Then
                    Dim isWRTGoal = (WRTState = ECWRTStates.wsGoal)
                    ObjPriorities.Clear()
                    AltValues.Clear()
                    AltValuesInOne.Clear()
                    PM.CalculationsManager.UseNormalizationForSA = (PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100)
                    PM.CalculationsManager.HierarchyID = H.HierarchyID
                    PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(SAUserID)) 'A1106 
                    PM.CalculationsManager.InitializeSAGradient(wrtNode.NodeID, isWRTGoal, SAUserID, ObjPriorities, AltValues, AltValuesInOne, RiskParam)
                    AltValuesInZero = PM.CalculationsManager.GetGradientData(wrtNode.NodeID, isWRTGoal, SAUserID, ObjPriorities, RiskParam)
                End If
            End If
        End If
    End Sub
    ''' <summary>
    ''' Get Objectives List for Sensitivity Analysis in form of JSON objects array string
    ''' </summary>
    ''' <param name="wrtID">WRT Node ID</param>
    ''' <param name="view">2D, HTH, DSA, PSA, GSA or ASA</param>
    ''' <returns></returns>
    Public Function GetSAObjectives(wrtID As Integer, view As String) As String
        Dim retVal As String = ""
        Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        If ObjPriorities.Count > 0 And altH IsNot Nothing And H IsNot Nothing Then
            Dim wrtNode As clsNode = H.GetNodeByID(If(wrtID = -1, WRTNodeID(False), wrtID))

            If wrtNode IsNot Nothing AndAlso wrtNode.Children.Count > 0 Then
                Dim objs2D As New Dictionary(Of Integer, NodePriority)
                Dim alts2D As New Dictionary(Of Integer, List(Of NodePriority))
                If view = "2D" Or view = "HTH" Then
                    PM.CalculationsManager.GetSA2DData(If(wrtID = -1, WRTNodeID(False), wrtID), SAUserID, objs2D, alts2D)
                End If
                Dim i = 0
                For Each obj As clsNode In wrtNode.Children
                    If (Not PM.IsRiskProject OrElse PM.IsRiskProject AndAlso obj.RiskNodeType <> RiskNodeType.ntCategory) Then
                        Dim AttrGuid As Guid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID
                        If PM.ActiveHierarchy = ECHierarchyID.hidImpact Then
                            AttrGuid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID
                        End If
                        Dim sNodeColor As String = ""
                        Dim tNodeColor As Long = CLng(PM.Attributes.GetAttributeValue(AttrGuid, obj.NodeGuidID))
                        sNodeColor = If(tNodeColor > 0, LongToBrush(tNodeColor), GetPaletteColor(CurrentPaletteID(PM), obj.NodeID)) 'i

                        Dim gradientMaxValues As String = ""
                        Dim gradientMinValues As String = ""
                        Dim values2D As New List(Of NodePriority)
                        If alts2D.ContainsKey(obj.NodeID) Then
                            values2D = alts2D(obj.NodeID)
                        End If

                        Dim values2DString = ""
                        If AltValuesInOne.ContainsKey(obj.NodeID) Then 'A1337
                            For Each altID In AltValuesInOne(obj.NodeID).Keys
                                gradientMaxValues += CStr(IIf(gradientMaxValues <> "", ",", "")) + String.Format("{{""altID"":{0},""val"":{1}}}", altID, JS_SafeNumber(AltValuesInOne(obj.NodeID)(altID)))
                            Next
                        End If
                        If AltValuesInZero.ContainsKey(obj.NodeID) Then 'A1337
                            For Each altID In AltValuesInZero(obj.NodeID).Keys
                                gradientMinValues += CStr(IIf(gradientMinValues <> "", ",", "")) + String.Format("{{""altID"":{0},""val"":{1}}}", altID, JS_SafeNumber(AltValuesInZero(obj.NodeID)(altID)))
                            Next
                        End If
                        For Each altVal In values2D
                            values2DString += CStr(IIf(values2DString <> "", ",", "")) + String.Format("{{""altID"":{0},""val"":{1},""valNorm"":{2}}}", altVal.NodeID, JS_SafeNumber(altVal.GlobalPriority), JS_SafeNumber(altVal.NormalizedGlobalPriority))
                        Next
                        If values2DString = "" Then values2DString = "{}"
                        retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":{0},""idx"":{1},""name"":""{2}"",""value"":{3},""initValue"":{4},""gradientMaxValues"":[{5}],""gradientMinValues"":[{6}],""gradientInitMinValues"":[{7}],""values2D"":[{8}],""color"":""{9}"", ""visible"":1}}", obj.NodeID, i, JS_SafeString(obj.NodeName), JS_SafeNumber(ObjPriorities(obj.NodeID)), JS_SafeNumber(ObjPriorities(obj.NodeID)), gradientMaxValues, gradientMinValues, gradientMinValues, values2DString, sNodeColor)
                        i += 1
                    End If
                Next
            End If
        End If
        Return String.Format("[{0}]", retVal)
    End Function
    ''' <summary>
    ''' Get List of Alternatives for Sensitivity Analysis in form of JSON objects array string
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSAAlternatives() As String
        Dim retVal As String = ""
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        If AltValues.Count > 0 And altH IsNot Nothing Then
            Dim tVisibleAlternatives As Dictionary(Of Integer, Boolean) = GetVisibleAlternatives() 'A1513
            '-FB17507
            'Dim NormCoef As Double = 1
            'If (NormalizeMode <> LocalNormalizationType.ntUnnormalized) Then
            '    Dim prtySum As Double = 0
            '    For Each alt As clsNode In altH.TerminalNodes
            '        Dim isVisible As Boolean = tVisibleAlternatives.ContainsKey(alt.NodeID) AndAlso tVisibleAlternatives(alt.NodeID)
            '        Dim AltVal = AltValues(alt.NodeID)
            '        If NormalizeMode = LocalNormalizationType.ntNormalizedSum100 And isVisible Then
            '            prtySum += AltVal
            '        End If
            '        If NormalizeMode = LocalNormalizationType.ntNormalizedForAll Then
            '            prtySum += AltVal
            '        End If
            '        'If NormalizeMode = LocalNormalizationType.ntNormalizedMul100 Then
            '        '    If prtySum < AltVal Then
            '        '        prtySum = AltVal
            '        '    End If
            '        'End If
            '    Next
            '    If prtySum > 0 Then NormCoef = 1 / prtySum
            'End If
            '-FB17507
            Dim i = 0
            For Each alt As clsNode In altH.TerminalNodes
                Dim isVisible As Boolean = tVisibleAlternatives.ContainsKey(alt.NodeID) AndAlso tVisibleAlternatives(alt.NodeID) 'A1513
                Dim sAltColor As String = ""
                Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), alt.NodeID, True)) 'i
                Dim AltVal As Double = AltValues(alt.NodeID) '-FB17507 * NormCoef
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""id"":{0},""idx"":{1},""name"":""{2}"",""value"":{3},""initValue"":{4},""color"":""{5}"",""visible"":{6},""event_type"":{7}}}", alt.NodeID, i, JS_SafeString(alt.NodeName), JS_SafeNumber(AltVal), JS_SafeNumber(AltVal), sAltColor, IIf(isVisible, "1", "0"), CInt(alt.EventType))
                i += 1
            Next
        End If
        Return String.Format("[{0}]", retVal)
    End Function
    ''' <summary>
    ''' Get Sesitivity Data for Sensitivity delta alternatives view
    ''' </summary>
    ''' <param name="UserGroupID">Specify Combined User ID</param>
    ''' <returns></returns>
    Function getASADataForUserGroupID(UserGroupID As Integer) As String
        Dim retVal As String = ""
        Dim altResults As String = ""
        Dim maxAltValue As Double = 0
        Dim objH As clsHierarchy = PM.ActiveObjectives
        Dim altH As clsHierarchy = PM.ActiveAlternatives

        If objH IsNot Nothing Then
            Dim asaData As ASAData = PM.CalculationsManager.GetASAData(UserGroupID)
            maxAltValue = If(PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100, asaData.MaxAlternativePriorityNormalized, asaData.MaxAlternativePriorityUnnormalized)
            Dim tVisibleAlternatives As Dictionary(Of Integer, Boolean) = GetVisibleAlternatives() 'A1513            

            Dim i = 0
            For Each alt As clsNode In altH.TerminalNodes
                Dim altVals As String = ""
                For Each node As clsNode In objH.TerminalNodes
                    Dim tValue As Tuple(Of Double, Double) = asaData.AlternativesPrioritiesByObjective(node.NodeID)(alt.NodeID)
                    Dim Value As Double = If(PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100, tValue.Item1, tValue.Item2)
                    'Dim Value As Single = If(NormalizeMode = LocalNormalizationType.ntUnnormalized, alt.UnnormalizedPriority, alt.WRTGlobalPriority)
                    altVals += CStr(IIf(altVals <> "", ",", "")) + JS_SafeNumber(Value)
                Next
                altVals = String.Format("[{0}]", altVals)
                Dim isVisible As Boolean = tVisibleAlternatives.ContainsKey(alt.NodeID) AndAlso tVisibleAlternatives(alt.NodeID) 'A1513
                Dim sAltColor As String = ""
                Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), alt.NodeID, True)) 'i
                Dim tGValue As Tuple(Of Double, Double) = asaData.AlternativesPriorities(alt.NodeID)
                Dim altGlobalValue As Double = If(PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100, tGValue.Item1, tGValue.Item2)
                altResults += CStr(IIf(altResults <> "", ",", "")) + String.Format("{{""id"": {0}, ""idx"": {1}, ""name"":""{2}"", ""initVals"": {3}, ""newVals"": [], ""oldVals"":[], ""value"": {4}, ""initValue"": {4}, ""color"": ""{5}"", ""visible"": {6}, ""event_type"": {7}}}", JS_SafeNumber(alt.NodeID), JS_SafeNumber(i), JS_SafeString(alt.NodeName), altVals, JS_SafeNumber(altGlobalValue), JS_SafeString(sAltColor), IIf(isVisible, "1", "0"), CInt(alt.EventType))
                i += 1
            Next

            Dim CheckedNodes As String() = Nothing
            If CVCheckedNodes IsNot Nothing Then CheckedNodes = CVCheckedNodes.Split(CChar(","))

            i = 0
            For Each obj As clsNode In objH.TerminalNodes
                Dim isVisible As Integer = 1
                If CheckedNodes IsNot Nothing AndAlso Not CheckedNodes.Contains(CStr(obj.NodeID)) Then isVisible = 0
                Dim tObjGValue As Tuple(Of Double, Double) = asaData.ObjectivesPriorities(obj.NodeID)
                Dim objGlobalValue As Double = If(PM.Parameters.Normalization <> LocalNormalizationType.ntUnnormalized And PM.Parameters.Normalization <> LocalNormalizationType.ntNormalizedMul100, tObjGValue.Item1, tObjGValue.Item2)
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{""idx"":{4}, ""id"": {0}, ""name"": ""{1}"", ""prty"": {2}, ""visible"": {3}}}", obj.NodeID, JS_SafeString(obj.NodeName), JS_SafeNumber(objGlobalValue), isVisible, i)
                i += 1
            Next
        End If
        If maxAltValue = 0 Then maxAltValue = 1
        Return String.Format("{{""objectives"": [{0}], ""alternatives"": [{1}], ""maxAltValue"": {2}, ""userID"": {3}}}", retVal, altResults, JS_SafeNumber(maxAltValue), UserGroupID)
    End Function

#End Region
#Region "Participant Result Groups"

    Private _UsersList As List(Of clsApplicationUser) = Nothing
    Private _WSList As List(Of clsWorkspace) = Nothing
    Private _UWFullList As List(Of clsUserWorkgroup) = Nothing
    Private _UWPrjList As List(Of clsUserWorkgroup) = Nothing

    Public ReadOnly Property UsersList As List(Of clsApplicationUser)
        Get
            If _UsersList Is Nothing Then
                App.CheckProjectManagerUsers(PRJ)
                _UsersList = App.DBUsersByProjectID(App.ProjectID)
            End If
            Return _UsersList
        End Get
    End Property

    Public ReadOnly Property WSList As List(Of clsWorkspace)
        Get
            If _WSList Is Nothing Then _WSList = App.DBWorkspacesByProjectID(App.ProjectID)
            Return _WSList
        End Get
    End Property

    Public ReadOnly Property UWFullList As List(Of clsUserWorkgroup)
        Get
            If _UWFullList Is Nothing Then _UWFullList = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)
            Return _UWFullList
        End Get
    End Property

    Public ReadOnly Property UWPrjList As List(Of clsUserWorkgroup)
        Get
            If _UWPrjList Is Nothing Then _UWPrjList = App.DBUserWorkgroupsByProjectIDWorkgroupID(App.ProjectID, App.ActiveWorkgroup.ID)
            Return _UWPrjList
        End Get
    End Property

    Public Function UserByID(tUserID As Integer) As clsApplicationUser
        Return clsApplicationUser.UserByUserID(tUserID, UsersList)
    End Function

    Public Function UserByEmail(sEmail As String) As clsApplicationUser
        Return clsApplicationUser.UserByUserEmail(sEmail, UsersList)
    End Function

    Private Function WSByUserID(tUserID As Integer) As clsWorkspace
        Return clsWorkspace.WorkspaceByUserIDAndProjectID(tUserID, App.ProjectID, WSList)
    End Function

    Private Function UWByUserID(tUserID As Integer) As clsUserWorkgroup
        Return clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUserID, App.ActiveWorkgroup.ID, UWPrjList)
    End Function
    ''' <summary>
    ''' Get list of participants, groups and attributes in form of JSON string
    ''' </summary>
    ''' <returns></returns>
    Public Function get_result_groups() As String
        Dim retVal As String = ""
        Dim retGroups As String = ""
        Dim retAttrs As String = ""

        Dim AttributesList = PM.Attributes.GetUserAttributes().Where(Function(attr) Not attr.IsDefault).ToList

        Dim tPrjUsers As List(Of ECTypes.clsUser) = PM.UsersList

        PM.CombinedGroups.UpdateDynamicGroups()
        Dim gUsers As New Dictionary(Of Integer, HashSet(Of Integer))
        Dim groups As New List(Of clsCombinedGroup)
        For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
            If group.CombinedUserID <> COMBINED_USER_ID Then
                Dim hs As New HashSet(Of Integer)
                For Each u As clsUser In group.UsersList
                    hs.Add(u.UserID)
                Next
                gUsers.Add(group.ID, hs)
                groups.Add(group)
                retGroups += If(retGroups = "", "", ",") + String.Format("{{""id"":{0},""name"":""{1}"",""is_dynamic"":{2}}}", group.ID, JS_SafeString(group.Name), Bool2JS(Not String.IsNullOrEmpty(group.Rule)))
            End If
        Next

        Dim linkedAttributesIDs As List(Of Guid) = GetLinkedUserAttributesIDs()
        For Each tAttr As clsAttribute In AttributesList
            If Not tAttr.IsDefault AndAlso tAttr.Type = AttributeTypes.atUser Then
                retAttrs += If(retAttrs = "", "", ",") + String.Format("{{""id"":""{0}"",""name"":""{1}"", ""readonly"":{2}}}", tAttr.ID, JS_SafeString(tAttr.Name), Bool2JS(linkedAttributesIDs.Contains(tAttr.ID)))
            End If
        Next

        For Each tUser As clsApplicationUser In UsersList
            Dim sName As String = tUser.UserName
            Dim AHPUserID As Integer = -1
            Dim tPrjUser As clsUser = clsApplicationUser.AHPUserByUserEmail(tUser.UserEmail, tPrjUsers)
            If tPrjUser IsNot Nothing Then
                AHPUserID = tPrjUser.UserID
                If Not String.IsNullOrEmpty(tPrjUser.UserName) Then sName = tPrjUser.UserName
            End If

            Dim fIsDisabled As Integer = 0
            Dim tGrpID As Integer = -1
            Dim sRole As String = ""
            Dim tWS As clsWorkspace = WSByUserID(tUser.UserID)
            If tWS IsNot Nothing Then
                tGrpID = tWS.GroupID
                If tWS.Status(PRJ.isImpact) = ecWorkspaceStatus.wsDisabled Then fIsDisabled = 1
                Dim tRole As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tWS.GroupID, App.ActiveWorkgroup.RoleGroups)
                If tRole IsNot Nothing Then
                    sRole = tRole.Name
                    Dim tUW As clsUserWorkgroup = UWByUserID(tUser.UserID)
                    If tUW IsNot Nothing AndAlso tUW.Status = ecUserWorkgroupStatus.uwDisabled Then fIsDisabled = 2
                End If
            End If

            If tUser.Status = ecUserStatus.usDisabled Then fIsDisabled = 2
            Dim tUserVisible As Boolean = True
            If tUserVisible Then
                Dim IsWSDisabled As Boolean = tWS.Status(PM.ActiveHierarchy = ECHierarchyID.hidImpact) = ecWorkspaceStatus.wsDisabled

                Dim sAttrVals As String = ""
                Dim i As Integer = 0
                For Each tAttr As clsAttribute In AttributesList
                    If Not tAttr.IsDefault AndAlso tAttr.Type = AttributeTypes.atUser Then
                        Dim sVal As String = ""
                        Dim tAttrVal As Object = PM.Attributes.GetAttributeValue(tAttr.ID, AHPUserID)
                        If tAttrVal IsNot Nothing Then
                            Select Case tAttr.ValueType
                                Case AttributeValueTypes.avtBoolean
                                    sVal = CStr(IIf(CBool(tAttrVal), "true", "false"))
                                Case AttributeValueTypes.avtDouble
                                    sVal = JS_SafeNumber(CDbl(tAttrVal))
                                Case AttributeValueTypes.avtString
                                    sVal = """" + JS_SafeString(CStr(tAttrVal)) + """"
                                Case AttributeValueTypes.avtLong
                                    sVal = JS_SafeNumber(CLng(tAttrVal))
                                Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                    ' not implemented
                            End Select
                        End If
                        sAttrVals += String.Format(",""a{0}"":{1}", i, If(sVal = "", """""", sVal))
                        i += 1
                    End If
                Next

                Dim sGrpVals As String = ""
                For Each tGrp As clsCombinedGroup In groups
                    sGrpVals += String.Format(",""g{0}"":{1}", tGrp.ID, Bool2JS(tPrjUser IsNot Nothing AndAlso gUsers(tGrp.ID).Contains(tPrjUser.UserID)))  ' D7409
                Next

                retVal += If(retVal = "", "", ",") + String.Format("{{""id"":{0},""email"":""{1}"",""name"":""{2}"",""role"":""{3}"",""is_disabled"":{4}{5}{6}}}", tUser.UserID, JS_SafeString(tUser.UserEmail), JS_SafeString(sName), sRole, Bool2JS(fIsDisabled > 0), sAttrVals, sGrpVals)
            End If
        Next

        Return String.Format("{{""participants"":[{0}],""groups"":[{1}],""attributes"":[{2}]}}", retVal, retGroups, retAttrs)
    End Function
    ''' <summary>
    ''' Add or Remove user from group
    ''' </summary>
    ''' <param name="group_id">specify group ID</param>
    ''' <param name="user_email">specify user email</param>
    ''' <param name="value">if true then add user, if false then remove</param>
    ''' <returns></returns>
    Public Function assign_result_group(group_id As Integer, user_email As String, value As Boolean) As String
        Dim fPrjChanged As Boolean = False
        Dim tPrjUser As clsUser = PM.GetUserByEMail(user_email)
        Dim tResGroup As clsCombinedGroup = CType(PM.CombinedGroups.GetGroupByID(group_id), clsCombinedGroup)
        If tResGroup IsNot Nothing Then 
            If value Then
                    If Not tResGroup.UsersList.Contains(tPrjUser) Then
                        tResGroup.UsersList.Add(tPrjUser)
                        fPrjChanged = True
                    End If
                Else
                    If tResGroup.UsersList.Contains(tPrjUser) Then
                        tResGroup.UsersList.Remove(tPrjUser)
                        fPrjChanged = True
                    End If
                End If
        End If

        If fPrjChanged Then
            App.ActiveProject.SaveStructure("Updated group assignment", False, True, String.Format("User: {0}, group: {1}, value: {2}", user_email, group_id, Bool2JS(value)))
        End If
        Return "{}"
    End Function
    ''' <summary>
    ''' Create new result group
    ''' </summary>
    ''' <param name="name">Specify group name</param>
    ''' <returns></returns>
    Public Function add_result_group(name As String) As String
        Dim tResGroup As clsCombinedGroup = CType(PM.CombinedGroups.AddCombinedGroup(name), clsCombinedGroup)
        If tResGroup IsNot Nothing Then             
            App.ActiveProject.SaveStructure("Create participant group", False, True, String.Format("Group name: {0}", name))
        End If
        Return "{}"
    End Function
    ''' <summary>
    ''' Change result group name
    ''' </summary>
    ''' <param name="id">result group ID</param>
    ''' <param name="name">New result group name</param>
    ''' <returns></returns>
    Public Function edit_result_group(id As Integer, name As String) As String
        Dim tResGroup As clsCombinedGroup = CType(PM.CombinedGroups.GetGroupByID(id), clsCombinedGroup)
        If tResGroup IsNot Nothing Then             
            tResGroup.Name = name
            App.ActiveProject.SaveStructure("Edited participant group name", False, True, String.Format("Group name: {0}", name))
        End If
        Return "{}"
    End Function
    ''' <summary>
    ''' Delete Result group
    ''' </summary>
    ''' <param name="id">Result group ID to delete</param>
    ''' <returns></returns>
    Public Function delete_result_group(id As Integer) As String
        Dim tResGroup As clsCombinedGroup = CType(PM.CombinedGroups.GetGroupByID(id), clsCombinedGroup)
        If tResGroup IsNot Nothing Then
            PM.CombinedGroups.DeleteGroup(tResGroup)
            App.ActiveProject.SaveStructure("Deleted participant group", False, True, String.Format("Group name: {0}", tResGroup.Name))
        End If
        Return "{}"
    End Function
    ''' <summary>
    ''' Reorder Result Group
    ''' </summary>
    ''' <param name="fromIndex">Old Index</param>
    ''' <param name="toIndex">New Index</param>
    ''' <returns></returns>
    Public Function move_result_group(fromIndex As Integer, toIndex As Integer) As String
        Dim tSourceGroup As clsGroup = PM.CombinedGroups.GroupsList(fromIndex)
        PM.CombinedGroups.GroupsList.Remove(tSourceGroup)
        PM.CombinedGroups.GroupsList.Insert(toIndex, tSourceGroup)
        App.ActiveProject.SaveStructure("Moved participant group", False, True, String.Format("Group name: {0}", tSourceGroup.Name))
        Return "{}"
    End Function
    ''' <summary>
    ''' Get List of Participants and groups
    ''' </summary>
    ''' <returns></returns>
    Public Function get_users_and_groups() As String
        Return String.Format("{{""users_data"":{0},""groups_data"":{1}}}", GetUsersData(), GetGroupsData())
    End Function
#End Region

#Region "Add attribute from survey question"

    Private Function GetSurvey(ByVal ASurveyType As SurveyType, Optional ByVal LoadAnswers As Boolean = False) As clsSurvey
        Dim retVal As clsSurvey = Nothing

        Dim UserEmail As String = If(LoadAnswers, "", "-")

        Dim AUsersList As New Dictionary(Of String, clsComparionUser)
        For Each User As clsUser In PM.UsersList
            AUsersList.Add(User.UserEMail, New clsComparionUser() With {.ID = User.UserID, .UserName = User.UserName})
        Next

        Dim tSurveyInfo As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(PRJ.ID, ASurveyType, AUsersList)
        If tSurveyInfo IsNot Nothing Then
            retVal = tSurveyInfo.Survey(UserEmail)
        End If

        Return retVal
    End Function

    Private Function ParseSurveyQuestionText(AQuestion As clsQuestion) As String
        Dim sText As String = AQuestion.Text
        If isMHT(sText) Then
            sText = Infodoc_Unpack(PRJ.ID, PM.ActiveHierarchy, reObjectType.SurveyQuestion, "", sText, True, True, -1)
            sText = HTML2Text(sText).Trim
            Dim sLines As String() = sText.Split(CChar(vbCrLf))
            sText = ""
            Dim idx = 0
            While sText.Length < 5 AndAlso idx <= sLines.GetUpperBound(0)
                If sLines(idx).Trim <> "" Then sText += sLines(idx).Trim + " "
                idx += 1
            End While
            sText = sText.Trim
        End If
        If sText = "" Then sText = String.Format("{0}-{1}", IIf(AQuestion.Name = "", AQuestion.Type.ToString, AQuestion.Name), AQuestion.AGUID.ToString)
        Return sText
    End Function
    ''' <summary>
    ''' Get Survey questions list
    ''' </summary>
    ''' <returns></returns>
    Public Function GetProjectSurveysQuestionsList() As String
        Dim retVal As String = ""

        If PM IsNot Nothing Then
            With PM
                If .PipeParameters.ShowWelcomeSurvey Then
                    Dim ASurvey As clsSurvey = GetSurvey(SurveyType.stWelcomeSurvey, True) 'A0644
                    If ASurvey IsNot Nothing Then
                        For Each APage As clsSurveyPage In ASurvey.Pages
                            For Each AQuestion As clsQuestion In APage.Questions
                                If AQuestion.Type <> QuestionType.qtAlternativesSelect And
                                    AQuestion.Type <> QuestionType.qtComment And
                                    AQuestion.Type <> QuestionType.qtObjectivesSelect Then
                                    retVal += If(retVal = "", "", ",") + String.Format("{{""guid"":""{0}"", ""is_thankyou"":{1}, ""linked_attr_id"":""{2}"", ""text"":""{3}"", ""unique_answers"":[{4}]}}", AQuestion.AGUID, Bool2JS(False), If(App.ActiveProject.ProjectManager.Attributes.GetAttributeByID(AQuestion.LinkedAttributeID) Is Nothing, "", AQuestion.LinkedAttributeID.ToString), JS_SafeString(ParseSurveyQuestionText(AQuestion)), GetUniqueAnswers(ASurvey, AQuestion.AGUID))
                                End If
                            Next
                        Next
                    End If
                End If
                If .PipeParameters.ShowThankYouSurvey Then
                    Dim ASurvey As clsSurvey = GetSurvey(SurveyType.stThankyouSurvey)
                    If ASurvey IsNot Nothing Then
                        For Each APage As clsSurveyPage In ASurvey.Pages
                            For Each AQuestion As clsQuestion In APage.Questions
                                If AQuestion.Type <> QuestionType.qtAlternativesSelect And
                                    AQuestion.Type <> QuestionType.qtComment And
                                    AQuestion.Type <> QuestionType.qtObjectivesSelect Then
                                    retVal += If(retVal = "", "", ",") + String.Format("{{""guid"":""{0}"", ""is_thankyou"":{1}, ""linked_attr_id"":""{2}"", ""text"":""{3}"", ""unique_answers"":[{4}]}}", AQuestion.AGUID, Bool2JS(True), If(App.ActiveProject.ProjectManager.Attributes.GetAttributeByID(AQuestion.LinkedAttributeID) Is Nothing, "", AQuestion.LinkedAttributeID.ToString), JS_SafeString(ParseSurveyQuestionText(AQuestion)), GetUniqueAnswers(ASurvey, AQuestion.AGUID))
                                End If
                            Next
                        Next
                    End If
                End If
            End With
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Private Function GetUniqueAnswers(tSurvey As clsSurvey, tQuestionGUID As Guid) As String
        Dim res As String = ""
        Dim tQuestion = tSurvey.QuestionByGUID(tQuestionGUID)
        If tQuestion IsNot Nothing Then
            If tQuestion.AllowVariants AndAlso tQuestion.Variants IsNot Nothing AndAlso tQuestion.Variants.Count > 0 Then
                For Each tVariant In tQuestion.Variants
                    res += If(res = "", "", ",") + String.Format("""{0}""", JS_SafeString(CType(tVariant, clsVariant).Text))
                Next
            End If
        End If
        Return res
    End Function

    Public Function LinkAttributeToSurveyQuestion(ByVal aAttributeGUID As Guid, ByVal aQuestionGUID As Guid, aSurveyType As SurveyType) As Boolean
        Dim retVal As Boolean = False

        Dim tSurveyInfo As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(PRJ.ID, aSurveyType, New Dictionary(Of String, clsComparionUser))
        If tSurveyInfo IsNot Nothing Then
            Dim ASurvey As clsSurvey = tSurveyInfo.Survey("-")
            If ASurvey IsNot Nothing Then
                Dim AQuestion As clsQuestion = ASurvey.QuestionByGUID(aQuestionGUID)
                If AQuestion IsNot Nothing Then
                    AQuestion.LinkedAttributeID = aAttributeGUID
                    tSurveyInfo.SaveSurvey(False)
                    retVal = True
                End If
            End If
        End If

        Return retVal
    End Function

    Public Function GetAnswersByQuestionGUID(ByVal sQuestionGUID As String, IsWelcomeSurvey As Boolean) As Dictionary(Of String, String)
        Dim AResult As New Dictionary(Of String, String)

        With PM
            If IsWelcomeSurvey Then
                If .PipeParameters.ShowWelcomeSurvey Then
                    Dim ASurvey As clsSurvey = GetSurvey(SurveyType.stWelcomeSurvey, True)
                    If ASurvey IsNot Nothing Then
                        For Each ARespondent As clsRespondent In ASurvey.Respondents
                            Dim AAnswer As clsAnswer = ARespondent.AnswerByQuestionGUID(New Guid(sQuestionGUID))
                            If AAnswer IsNot Nothing Then AResult.Add(ARespondent.Email, AAnswer.AnswerValuesString)
                        Next
                    End If
                End If
            Else
                If .PipeParameters.ShowThankYouSurvey Then
                    Dim ASurvey As clsSurvey = GetSurvey(SurveyType.stThankyouSurvey, True)
                    If ASurvey IsNot Nothing Then
                        For Each ARespondent As clsRespondent In ASurvey.Respondents
                            Dim AAnswer As clsAnswer = ARespondent.AnswerByQuestionGUID(New Guid(sQuestionGUID))
                            If AAnswer IsNot Nothing Then AResult.Add(ARespondent.Email, AAnswer.AnswerValuesString)
                        Next
                    End If
                End If
            End If
        End With

        Return AResult
    End Function

    Public Function AddUserAttributeFromSurvey(ByVal sQuestionGUID As String, ByRef AttrGUID As Guid, ByVal AttributeName As String, ByVal Type As ECCore.Attributes.AttributeValueTypes, ByVal DefaultValue As Object, IsWelcomeSurvey As Boolean) As String
        Dim retVal As String = ""
        Dim fError As Boolean = False
        Dim sErrors As String = ""

        With PM
            AttributeName = AttributeName.Trim
            If String.IsNullOrEmpty(AttributeName) OrElse AttributeName = "" Then AttributeName = "New Attribute " + (.Attributes.AttributesList.Count + 1).ToString
            Dim attr As clsAttribute = .Attributes.AddAttribute(Guid.NewGuid, AttributeName, AttributeTypes.atUser, Type, DefaultValue)
            AttrGUID = attr.ID
            LinkAttributeToSurveyQuestion(attr.ID, New Guid(sQuestionGUID), CType(IIf(IsWelcomeSurvey, SurveyType.stWelcomeSurvey, SurveyType.stThankyouSurvey), SurveyType))
            Dim answers As Dictionary(Of String, String) = GetAnswersByQuestionGUID(sQuestionGUID, IsWelcomeSurvey)

            If answers IsNot Nothing AndAlso answers.Count > 0 Then
                For Each answer In answers
                    Dim user = .GetUserByEMail(answer.Key)
                    If user IsNot Nothing Then
                        Dim value As String
                        If answer.Value IsNot Nothing Then value = answer.Value.ToString Else value = ""
                        Dim canConvertType As Boolean = True
                        Select Case Type
                            Case AttributeValueTypes.avtBoolean
                                Dim boolValue As Boolean
                                canConvertType = ExpertChoice.Service.Str2Bool(value, boolValue)
                            Case AttributeValueTypes.avtDouble
                                Dim sngValue As Double  ' D1858
                                canConvertType = ExpertChoice.Service.String2Double(value, sngValue)    ' D1858
                            Case AttributeValueTypes.avtLong
                                Dim lngValue As Long
                                canConvertType = Long.TryParse(value, lngValue)
                            Case AttributeValueTypes.avtString
                        End Select
                        If canConvertType Then
                            PM.Attributes.SetAttributeValue(attr.ID, user.UserID, Type, value, Nothing, Nothing)
                        Else
                            sErrors += String.Format("{0}User {1} ({2}) - answer ""{3}""", Environment.NewLine, user.UserName, user.UserEMail, value)
                            fError = True
                        End If
                    End If
                Next
            End If
            .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
        End With
        If fError Then retVal = ResString("errGetUserAttr") + Environment.NewLine + sErrors

        Return retVal
    End Function

    Public Function RemoveLinkedAttribute(aAttributeGUID As Guid) As Boolean
        Dim retVal As Boolean = False

        Dim tSurveyInfo As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(PRJ.ID, SurveyType.stWelcomeSurvey, New Dictionary(Of String, clsComparionUser))
        Dim ASurvey As clsSurvey
        If tSurveyInfo IsNot Nothing Then
            ASurvey = tSurveyInfo.Survey("-")
            If ASurvey IsNot Nothing Then
                For Each APage As clsSurveyPage In ASurvey.Pages
                    For Each AQuestion As clsQuestion In APage.Questions
                        If AQuestion.LinkedAttributeID = aAttributeGUID Then
                            AQuestion.LinkedAttributeID = Guid.Empty
                            tSurveyInfo.SaveSurvey(False)
                            retVal = True
                        End If
                    Next
                Next
            End If
        End If

        tSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(PRJ.ID, SurveyType.stThankyouSurvey, New Dictionary(Of String, clsComparionUser))
        If tSurveyInfo IsNot Nothing Then
            ASurvey = tSurveyInfo.Survey("-")
            If ASurvey IsNot Nothing Then
                For Each APage As clsSurveyPage In ASurvey.Pages
                    For Each AQuestion As clsQuestion In APage.Questions
                        If AQuestion.LinkedAttributeID = aAttributeGUID Then
                            AQuestion.LinkedAttributeID = Guid.Empty
                            tSurveyInfo.SaveSurvey(False)
                            retVal = True
                        End If
                    Next
                Next
            End If
        End If

        Return retVal
    End Function

    Public Function GetLinkedUserAttributesIDs() As List(Of Guid)

        Dim AResult As New List(Of Guid)
        Dim ASurvey As clsSurvey = GetSurvey(SurveyType.stWelcomeSurvey, False)
        If ASurvey IsNot Nothing Then
            For Each APage As clsSurveyPage In ASurvey.Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.LinkedAttributeID <> Guid.Empty AndAlso App.ActiveProject.ProjectManager.Attributes.GetAttributeByID(AQuestion.LinkedAttributeID) IsNot Nothing Then
                        AResult.Add(AQuestion.LinkedAttributeID)
                    End If
                Next
            Next
        End If

        ASurvey = GetSurvey(SurveyType.stThankyouSurvey, False)
        If ASurvey IsNot Nothing Then
            For Each APage As clsSurveyPage In ASurvey.Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.LinkedAttributeID <> Guid.Empty AndAlso App.ActiveProject.ProjectManager.Attributes.GetAttributeByID(AQuestion.LinkedAttributeID) IsNot Nothing Then
                        AResult.Add(AQuestion.LinkedAttributeID)
                    End If
                Next
            Next
        End If
        Return AResult
    End Function

    Public Function CreateNewResultGroupsWithRules(ByVal tGroups As Dictionary(Of String, String)) As Dictionary(Of Integer, Integer)
        Dim res As Dictionary(Of Integer, Integer) = Nothing

        If tGroups IsNot Nothing AndAlso tGroups.Count > 0 Then
            res = New Dictionary(Of Integer, Integer)
            For Each grp In tGroups
                Dim NewGroup As clsCombinedGroup = PM.CombinedGroups.AddCombinedGroup(grp.Key)
                NewGroup.Rule = grp.Value
                NewGroup.ApplyRules()
                res.Add(NewGroup.ID, NewGroup.CombinedUserID)
            Next
            PRJ.SaveStructure("Create new result group", , , String.Format("Added {0} result group(s) with rules", tGroups.Count))
        End If

        Return res
    End Function

    Public Function GetFilterRule(attrID As Guid, tFilterText As String) As String

        Dim FilterCombination As ECTypes.FilterCombinations = ECTypes.FilterCombinations.fcAnd
        Dim fc As System.Xml.Linq.XElement = New System.Xml.Linq.XElement("FilterCombination", FilterCombination.ToString)

        Dim xDoc As System.Xml.Linq.XDocument =
        <?xml version="1.0" encoding="utf-8" standalone="yes"?>
        <!--FilterCombination values - 0 = AND, 1 = OR -->
        <Root>
            <Settings FilterCombination=""></Settings>
            <Rules></Rules>
        </Root>

        xDoc.Root.Element("Settings").Attribute("FilterCombination").Value = CInt(FilterCombination).ToString
        Dim FilterOperationID As Integer = FilterOperations.Contains
        Dim el As System.Xml.Linq.XElement = New System.Xml.Linq.XElement("Rule", New System.Xml.Linq.XAttribute("AttributeID", attrID.ToString), New System.Xml.Linq.XAttribute("OperationID", FilterOperationID), New System.Xml.Linq.XAttribute("FilterText", String.Format("""{0}""", tFilterText)))
        xDoc.Root.Element("Rules").Add(el)

        Dim sb As Text.StringBuilder = New Text.StringBuilder()
        Dim xws As XmlWriterSettings = New XmlWriterSettings()
        xws.OmitXmlDeclaration = False
        xws.Indent = True

        Using xw = XmlWriter.Create(sb, xws)
            xDoc.WriteTo(xw)
        End Using
        Return sb.ToString
    End Function

#End Region

    Private Sub ProjectManagerWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject()
        Select Case _Page.Action

            Case "synthesize"
                Dim wrtID As Integer = -1
                If Not Integer.TryParse(GetParam(_Page.Params, "wrt", True), wrtID) Then wrtID = WRTNodeID
                If wrtID = -1 Then
                    wrtID = PM.ActiveObjectives.Nodes(0).NodeID
                End If
                SetOption("WRTNodeID", wrtID.ToString)
                Dim SType As String = GetParam(_Page.Params, "chartType", True)
                If Not String.IsNullOrEmpty(SType) Then
                    SetOption("chartType", SType)
                End If
                Dim SselNode As String = GetParam(_Page.Params, "selectedNodeID", True)
                If Not String.IsNullOrEmpty(SselNode) Then
                    SetOption("selectedNodeID", SselNode)
                    Dim tValue As Integer = 0
                    If Integer.TryParse(SselNode, tValue) Then
                        WRTNodeID = tValue
                    End If
                End If
                Dim tSimMode As String = GetParam(_Page.Params, "useSimulatedValues", True)
                If Not String.IsNullOrEmpty(tSimMode) Then
                    SetOption("useSimulatedValues", tSimMode)
                End If
                Dim itms As String = GetParam(_Page.Params, "items", True).Trim()
                Dim dataItems() As String = itms.Split(CChar(","))
                Dim users As String = GetParam(_Page.Params, "users", True).Trim()
                Dim UserIDs As New List(Of Integer)
                If users = "selected" Then
                    UserIDs = SelectedUsersAndGroupsIDs()
                Else
                    Dim usersStr() As String = users.Split(CChar(","))
                    For Each idStr As String In usersStr
                        Dim uid As Integer = -1
                        If Not Integer.TryParse(idStr, uid) Then uid = -1
                        If Not UserIDs.Contains(uid) Then UserIDs.Add(uid)
                    Next
                End If

                _Page.ResponseData = Synthesize(wrtID, dataItems, UserIDs)

            Case "options"
                _Page.ResponseData = Options()  ' D6225

            Case "set_option"
                Dim sOptionName As String = GetParam(_Page.Params, "name", True)
                Dim sOptionValue As String = GetParam(_Page.Params, "value", True)
                SetOption(sOptionName, sOptionValue)
                _Page.ResponseData = "{}"
                Select Case sOptionName
                    Case "decimals"
                        Dim tValue As Integer = 0
                        If Integer.TryParse(sOptionValue, tValue) Then
                            PM.Parameters.DecimalDigits = tValue    ' D6849
                        End If
                    Case "normalizationMode"
                        Select Case sOptionValue
                            Case "none"
                                PM.Parameters.Normalization = LocalNormalizationType.ntUnnormalized
                            Case "normSelected"
                                PM.Parameters.Normalization = LocalNormalizationType.ntNormalizedSum100
                            Case "norm100"
                                PM.Parameters.Normalization = LocalNormalizationType.ntNormalizedMul100
                            Case "normAll"
                                PM.Parameters.Normalization = LocalNormalizationType.ntNormalizedForAll
                        End Select
                        PM.Parameters.Save()
                    Case "selectedNodeID"
                        Dim sPID As String = GetParam(_Page.Params, "pid", True)
                        Dim tValue As Integer = 0
                        If Integer.TryParse(sOptionValue, tValue) Then
                            WRTNodeID = tValue
                            If sPID <> "" Then sPID = PM.ActiveObjectives.GetNodeByID(CInt(sPID)).NodeGuidID.ToString
                            PM.Parameters.WRTNodeParentGUID = sPID
                            PM.Parameters.Save()
                        End If
                        _Page.ResponseData = String.Format("{{""wrtNodeParent"":""{0}""}}", PM.Parameters.WRTNodeParentGUID)                    
                End Select

            Case "download_gembox_xlsx"  ' D6225
                Download_GemBox_XLSX(GetParam(_Page.Params, "type", True))   ' D6625

            Case "download_gembox_docx"
                Download_GemBox_DOCX(GetParam(_Page.Params, "type", True))

            Case "download_gembox_pdf"      ' D6426
                Download_GemBox_PDF(GetParam(_Page.Params, "type", True))   ' D6426

            Case "download_gembox_pdf_img"  ' D6493
                Download_GemBox_PDF_Img()   ' D6493

            Case "download_gembox_pptx"
                Download_GemBox_PPTX(GetParam(_Page.Params, "type", True))

            Case "set_active_grids_tab"
                Integer.TryParse(GetParam(_Page.Params, "value", True), PM.Parameters.Synthesis_GridsTab)   ' D7031
                PM.Parameters.Save()
                _Page.ResponseData = "{}"

            Case "selected_users"
                Dim sIDs As String = GetParam(_Page.Params, "value", True).Trim()
                If sIDs = "" Then sIDs = UNDEFINED_USER_ID.ToString
                StringSelectedUsersAndGroupsIDs = sIDs
                _Page.ResponseData = "{}"

            Case "alts_filter"
                AlternativesFilterValue = CInt(GetParam(_Page.Params, "value", True))
                Select Case AlternativesFilterValue
                    Case -2
                        Integer.TryParse(GetParam(_Page.Params, "alts_num", True), AlternativesAdvancedFilterValue) ' D7031
                        Integer.TryParse(GetParam(_Page.Params, "user_id", True), AlternativesAdvancedFilterUserID) ' D7031
                    Case -3
                        Dim AlternativesCustomFilterValue As String() = GetParam(_Page.Params, "ids", True).Split(CChar(","))
                        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            Dim tAltSelectedOld As Boolean = IsAltSelected(alt.NodeGuidID)
                            Dim tAltSelectedNew As Boolean = AlternativesCustomFilterValue.Contains(alt.NodeID.ToString)
                            If tAltSelectedOld <> tAltSelectedNew Then IsAltSelected(alt.NodeGuidID) = tAltSelectedNew
                        Next
                    Case -4 ' funded in RA scenario
                    Case -5 ' filter by alternative attribute
                        Dim sFilter As String = URLDecode(GetParam(_Page.Params, "filter", True))
                        Dim sComb As String = GetParam(_Page.Params, "combination", True)
                        ParseAttributesFilter(sFilter, sComb)
                    Case -6 ' Show risks only
                        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            Dim tAltSelectedOld As Boolean = IsAltSelected(alt.NodeGuidID)
                            Dim tAltSelectedNew As Boolean = alt.EventType = EventType.Risk
                            If tAltSelectedOld <> tAltSelectedNew Then IsAltSelected(alt.NodeGuidID) = tAltSelectedNew
                        Next
                    Case -7 ' Show opportunity only
                        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            Dim tAltSelectedOld As Boolean = IsAltSelected(alt.NodeGuidID)
                            Dim tAltSelectedNew As Boolean = alt.EventType = EventType.Risk
                            If tAltSelectedOld <> tAltSelectedNew Then IsAltSelected(alt.NodeGuidID) = tAltSelectedNew
                        Next
                End Select
                _Page.ResponseData = "{}"

            Case "switch_show_priorities"
                Dim sValue As String = GetParam(_Page.Params, "value", True)
                PM.CalculationsManager.ShowDueToPriorities = Str2Bool(sValue)
                _Page.ResponseData = "{}"

            'Case "v_splitter_size"
            '    Dim sValue As String = GetParam(_Page.Params, "value", True)
            '    Dim tValue As Double
            '    If String2Double(sValue, tValue) Then
            '        PM.Parameters.Synthesis_ObjectivesSplitterSize = Convert.ToInt32(tValue)
            '        PM.Parameters.Save()
            '    End If
            '    _Page.ResponseData = "{}"

            Case "objectives_visibility"
                PM.Parameters.Synthesis_ObjectivesVisibility = Str2Bool(GetParam(_Page.Params, "hierarchy", True))
                Dim ShowLocal As Boolean = Str2Bool(GetParam(_Page.Params, "local_priorities", True))
                Dim ShowGlobal As Boolean = Str2Bool(GetParam(_Page.Params, "global_priorities", True))
                If Not ShowLocal AndAlso Not ShowGlobal Then PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 0
                If ShowLocal AndAlso Not ShowGlobal Then PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 2
                If Not ShowLocal AndAlso ShowGlobal Then PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 3
                If ShowLocal AndAlso ShowGlobal Then PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4
                PM.Parameters.Save()
                _Page.ResponseData = "{}"

            Case "selected_events_type"
                ShowEventsOfType = CType(CInt(GetParam(_Page.Params, "value", True)), RiskionShowEventsType)
                _Page.ResponseData = "{}"
            Case "selected_events_type"
                Dim tmp As Integer  ' D7031
                If Integer.TryParse(GetParam(_Page.Params, "value", True), tmp) Then ShowEventsOfType = CType(tmp, RiskionShowEventsType)   ' D7031
                _Page.ResponseData = "{}"

            Case ACTION_DSA_UPDATE_VALUES
                Dim wrtID As Integer = -1
                If Not Integer.TryParse(GetParam(_Page.Params, "wrt", True), wrtID) Then wrtID = WRTNodeID(False)
                If wrtID = -1 Then
                    wrtID = PM.ActiveObjectives.Nodes(0).NodeID
                End If
                SetOption("WRTNodeID", wrtID.ToString)
                Dim s_ids As String = GetParam(_Page.Params, "objids", True).Trim()
                Dim s_values As String = GetParam(_Page.Params, "values", True).Trim()
                Dim s_name As String = GetParam(_Page.Params, "canvasid", True).Trim()
                Dim s_sauserid As String = GetParam(_Page.Params, "sauserid", True).Trim()
                Dim isWRTGoal As Boolean = Str2Bool(GetParam(_Page.Params, "iswrtgoal", True).Trim())


                Dim SAUserID As Integer = -1
                Integer.TryParse(s_sauserid, SAUserID)

                Dim values() As String = s_values.Split(CChar(","))
                Dim ids() As String = s_ids.Split(CChar(","))

                Dim ANewObjPriorities As New Dictionary(Of Integer, Double)
                Dim i = 0
                For Each objID In ids
                    Dim APrty As Double = 0
                    String2Double(values(i), APrty)
                    ANewObjPriorities.Add(CInt(objID), APrty)
                    i += 1
                Next

                Dim AltValuesInZero As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

                Dim SAWRTNodeID As Integer = WRTNodeID(False)
                Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
                If H IsNot Nothing Then
                    Dim wrtNode As clsNode = H.GetNodeByID(If(wrtID = -1, SAWRTNodeID, wrtID))
                    If wrtNode IsNot Nothing Then
                        ' D7225 ===
                        Dim RiskParam As Integer = 1
                        Dim sRisk As String = GetParam(_Page.Params, "risk", True)
                        If sRisk = "" OrElse Not App.isRiskEnabled OrElse Not Integer.TryParse(sRisk, RiskParam) Then RiskParam = 1
                        AltValuesInZero = PM.CalculationsManager.GetGradientData(wrtNode.NodeID, isWRTGoal, SAUserID, ANewObjPriorities, RiskParam)
                        ' D7225 ==
                    End If
                End If

                Dim ZeroValuesString As String = ""
                For Each Objitem In AltValuesInZero
                    Dim ZeroAltValuesString As String = ""
                    For Each AltItem In Objitem.Value
                        ZeroAltValuesString += CStr(IIf(ZeroAltValuesString <> "", ",", "")) + String.Format("{{""altID"":{0},""val"":{1}}}", AltItem.Key, JS_SafeNumber(AltItem.Value))
                    Next
                    ZeroValuesString += CStr(IIf(ZeroValuesString <> "", ",", "")) + String.Format("[{0},[{1}]]", Objitem.Key, ZeroAltValuesString)
                Next
                _Page.ResponseData = String.Format("[""{0}"", ""{1}"", [{2}]]", ACTION_DSA_UPDATE_VALUES, s_name, ZeroValuesString)

            Case ACTION_DSA_INIT_DATA
                Dim wrtID As Integer = -1
                If Not Integer.TryParse(GetParam(_Page.Params, "wrt", True), wrtID) Then wrtID = WRTNodeID(False)
                If wrtID = -1 Then
                    wrtID = PM.ActiveObjectives.Nodes(0).NodeID
                End If
                SetOption("WRTNodeID", wrtID.ToString)
                Dim sViewMode As String = GetParam(_Page.Params, "view", True)
                Dim forceComputed As Boolean = False
                Dim oldComputedMode As Boolean = False
                If HasParam(_Page.Params, "force_computed", True) Then 
                    forceComputed = Str2Bool(GetParam(_Page.Params, "force_computed", True))
                End If

                If forceComputed Then 
                    oldComputedMode = PM.CalculationsManager.UseSimulatedValues
                    PM.CalculationsManager.UseSimulatedValues = False
                End If
                ' D6891 ===
                Dim s_sauserid As String = GetParam(_Page.Params, "sauserid", True).Trim()
                Dim _SAUserID As Integer = -1
                If Integer.TryParse(s_sauserid, _SAUserID) Then
                    If PM.UserExists(_SAUserID) OrElse PM.CombinedGroups.GetGroupByCombinedID(_SAUserID) IsNot Nothing Then
                        SAUserID = _SAUserID
                    End If
                End If
                ' D6891 ==

                Dim RiskParam As Integer
                If Not App.isRiskEnabled OrElse Not Integer.TryParse(GetParam(_Page.Params, "risk", True), RiskParam) Then RiskParam = 0     ' D7142 + D7225
                initSAData(wrtID, RiskParam)
                Dim sa_objs As String = GetSAObjectives(wrtID, sViewMode)
                Dim sa_alts As String = GetSAAlternatives()
                Dim isASA As Boolean = Str2Bool(GetParam(_Page.Params, "asa", True))
                Dim asaData As String = ""
                If isASA Then asaData = getASADataForUserGroupID(SAUserID)  ' D6612
                Dim ASAPageSize As Integer = PM.Parameters.AsaPageSize
                Dim ASACurrentPage As Integer = PM.Parameters.AsaPageNum
                'ASASortBy: asa_sortby,
                'SAAltsSortBy: sa_alts_sortby,
                'altFilterValue: sa_altfilter,
                'valDigits: DecimalDigits,
                'Dim normalizationMode As String = getNormModeSA()
                'showComponents: showComponents,
                'showMarkers: showSAMarkers,
                'SessionId 'saState' + <%=App.ProjectID%>,
                'PSAShowLines: true,
                Dim sa_comps As String = GetSAComponentsData(wrtID)
                'DSAActiveSorting: <% = Bool2JS(PM.Parameters.DsaActiveSorting)%>,
                'GSAShowLegend: <%= Bool2JS(SynthesisView = SynthesisViews.vtMixed Or SynthesisView = SynthesisViews.vtDashboard Or SynthesisView = SynthesisViews.vt2D Or SynthesisView = SynthesisViews.vtDSA Or SynthesisView = SynthesisViews.vtPSA Or SynthesisView = SynthesisViews.vtGSA)%>,
                'titleAlternatives: "<%=ParseString("%%Alternatives%%")%>",
                'titleObjectives: "<%=ParseString("%%Objectives%%")%>",
                If asaData <> "" Then
                    asaData = String.Format(" ""ASAdata"":{0},", asaData)
                End If
                Dim cGroup As clsCombinedGroup = PM.CombinedGroups.GetCombinedGroupByUserID(SAUserID)
                Dim cUser As clsUser = PM.GetUserByID(SAUserID)
                Dim SAUserName As String = ""
                If (cUser IsNot Nothing) Then
                    SAUserName = cUser.UserName
                End If
                If (cGroup IsNot Nothing) Then
                    SAUserName = "[" + cGroup.Name + "]"
                End If
                Dim Result As String = String.Format("{{""objs"":{0},""alts"":{1}, ""comps"":{2},{3} ""saUserID"":{4}, ""saUserName"":""{5}""}}", sa_objs, sa_alts, sa_comps, asaData, SAUserID, JS_SafeString(SAUserName)).Replace("\'", "'")
                _Page.ResponseData = Result

                If forceComputed Then 
                    PM.CalculationsManager.UseSimulatedValues = oldComputedMode
                End If
            Case "get_alternatives_and_objectves_priorities"

                _Page.ResponseData = get_alternatives_and_objectves_priorities()

            Case "get_result_groups"
                _Page.ResponseData = get_result_groups()

            Case "add_result_group"
                _Page.ResponseData = add_result_group(GetParam(_Page.Params, "name", True))

            Case "edit_result_group"
                _Page.ResponseData = edit_result_group(CInt(GetParam(_Page.Params, "id", True)), GetParam(_Page.Params, "name", True))

            Case "assign_result_group"
                _Page.ResponseData = assign_result_group(CInt(GetParam(_Page.Params, "group_id", True)), GetParam(_Page.Params, "user_email", True), Str2Bool(GetParam(_Page.Params, "value", True)))

            Case "delete_result_group"
                _Page.ResponseData = delete_result_group(CInt(GetParam(_Page.Params, "id", True)))
            
            Case "move_result_group"
                _Page.ResponseData = move_result_group(CInt(GetParam(_Page.Params, "fromindex", True)), CInt(GetParam(_Page.Params, "toindex", True)))

            Case "get_users_and_groups"
                _Page.ResponseData = get_users_and_groups()

        End Select
    End Sub

End Class