Imports DevExpress.Web.ASPxGridView
Imports System.Data
Imports Telerik.Web.UI
Imports System.Diagnostics 'AS/11339

Partial Class DataGridPage
    Inherits clsComparionCorePage

    'L0476 ===
    Public ReadOnly Property UseSession() As Boolean
        Get
            Return GetCookie("UseSession", "True") = "True" ' D4663
        End Get
    End Property
    'L0476 ==

    Public Sub New()
        MyBase.New(_PGID_REPORT_DATAGRID)   ' D1559
    End Sub

    Private Sub ClearSessionDS()
        DS6 = Nothing
    End Sub

    Public Property SelectedUserID As Integer
        Get
            Return If(Session("DGUID" + App.ProjectID.ToString) Is Nothing, -1, CType(Session("DGUID" + App.ProjectID.ToString), Integer))
        End Get
        Set(value As Integer)
            Session("DGUID" + App.ProjectID.ToString) = value
        End Set
    End Property
    Public IsGroupSelected As Integer = 1 'AS/16038
    Public Function getSelectedUserID() As Integer 'AS/16036
        Return SelectedUserID
    End Function

    'L0503 ===
    Private _DS6 As DataSet = Nothing
    Public Property DS6() As DataSet
        Get
            If UseSession Then
                Return CType(Session("DS6"), DataSet)
            Else
                Return _DS6
            End If
        End Get
        Set(value As DataSet)
            If UseSession Then
                Session("DS6") = value
            Else
                _DS6 = value
            End If
        End Set
    End Property
    'L0503 ==

    'ReadOnly Property ddlGroups() As DropDownList
    '    Get
    '        Return CType(RadToolbarMain.Items(1).FindControl("ddlGroups"), DropDownList)
    '    End Get
    'End Property

    Private mDGColCaptions As Dictionary(Of String, String)

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = False
        AlignVerticalCenter = False
        PM.ProjectDataProvider.FullAlternativePath = False
        If Session("DSADHOC") Is Nothing Then
            Session("DSADHOC") = App.ActiveProject.Passcode
        End If

        With App.ActiveProject.ProjectManager
            .Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
        End With

        If CheckVar("reset", "") <> "" Then
            Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False) + "&id=" + HttpUtility.UrlEncode(CheckVar("id", "").ToString)), True)    ' D1559
        End If
        'If CheckVar("upload", "") = "0" Then
        '    btnUploadDataGrid.Visible = False
        'End If
        Dim Participants As New List(Of clsUser)
        'If ddlGroups.Items.Count = 0 Then
        '    ddlGroups.Items.Clear()
        '    For Each Group As clsCombinedGroup In App.ActiveProject.ProjectManager.CombinedGroups.GroupsList
        '        ddlGroups.Items.Add("[" + Group.Name + "]")
        '    Next
        '    Dim t As String = PageURL(_PGID_REPORT_DATAGRID_UPLOAD, _PARAM_TEMP_THEME + "=" + _THEME_SL)
        '    Dim sUser As String = HttpUtility.UrlDecode(CheckVar("id", "")).ToLower    ' D1559
        '    For Each User As clsUser In App.ActiveProject.ProjectManager.UsersList
        '        Dim AListItem As New ListItem(User.UserName, User.UserEMail)
        '        If User.UserEMail.ToLower = sUser Then
        '            AListItem.Selected = True ' D1559
        '            SelectedUserID = User.UserID
        '        End If
        '        ddlGroups.Items.Add(AListItem)
        '    Next
        '    If String.IsNullOrEmpty(ddlGroups.Attributes("onchange")) Then ddlGroups.Attributes.Add("onchange", "ShowPleaseWait();")
        'End If
        If mDGColCaptions Is Nothing Then
            mDGColCaptions = New Dictionary(Of String, String)

            mDGColCaptions.Add(clsProjectDataProvider.dgColNum.ToString, ResString("dgNum"))
            mDGColCaptions.Add(clsProjectDataProvider.dgColAltName.ToString, ResString("dgAlt"))
            mDGColCaptions.Add(clsProjectDataProvider.dgColTotal.ToString, ResString("dgTotal"))

            For Each node As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
                mDGColCaptions.Add(node.NodeGuidID.ToString, node.NodeName)
            Next
            For Each altAttr As clsAttribute In PM.Attributes.GetAlternativesAttributes
                If altAttr.ID <> ECCore.ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID And altAttr.ID <> ECCore.ATTRIBUTE_RA_ALT_SORT_ID And (altAttr.ID <> ECCore.ATTRIBUTE_KNOWN_VALUE_ID Or PM.IsRiskProject) And altAttr.ID <> ECCore.ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID And altAttr.ID <> ECCore.ATTRIBUTE_RESULTS_ALTERNATIVE_IS_SELECTED_ID Then
                    mDGColCaptions.Add(altAttr.ID.ToString, App.GetAttributeName(altAttr))
                End If
            Next

            mDGColCaptions.Add(clsProjectDataProvider.dgColEarliest.ToString, "Earliest")
            mDGColCaptions.Add(clsProjectDataProvider.dgColLatest.ToString, "Latest")
            mDGColCaptions.Add(clsProjectDataProvider.dgColDuration.ToString, "Duration")
        End If
        ' D4746 ===
        Dim sAction As String = CheckVar(_PARAM_ACTION, "").ToLower
        SelectedUserID = CheckVar("id", SelectedUserID)
        If SelectedUserID >= 0 Then 'AS/21354c===
            IsGroupSelected = 0
        Else
            IsGroupSelected = 1
        End If 'AS/21354c==

        Select Case sAction
            Case "download"
                DownloadDataGrid()
        End Select
        ' D4746 ==
        If isCallback Then Ajax_Callback(Request.Form.ToString)
    End Sub

    Public Function getGroupsUsersList() As String
        Dim resVal As String = ""
        Dim groupsString = ""
        For Each Group As clsCombinedGroup In App.ActiveProject.ProjectManager.CombinedGroups.GroupsList
            resVal += CStr(IIf(resVal = "", "", ",")) + String.Format("{{""id"":{0},""text"":""[{1}]""}}", Group.CombinedUserID, JS_SafeString(Group.Name))
        Next
        For Each User As clsUser In App.ActiveProject.ProjectManager.UsersList
            resVal += CStr(IIf(resVal = "", "", ",")) + String.Format("{{""id"":{0},""text"":""{1}""}}", User.UserID, JS_SafeString(User.UserName))
        Next
        Return String.Format("[{0}]", resVal)
    End Function

    Protected Sub DownloadDataGrid()
        StorePageID = False
        Dim FileName As String = "DataGrid"
        Dim filePath As String = File_CreateTempName()
        Dim sContentType As String = "application/octet-stream"
        Dim isExport As Boolean = False
        Dim sUserName As String = ""

        Dim ADataGrid As New clsDataGrid()
        'If ddlGroups.SelectedIndex >= PM.CombinedGroups.GroupsList.Count Then
        '   ADataGrid.CurrentUser = PM.UsersList(ddlGroups.SelectedIndex - PM.CombinedGroups.GroupsList.Count)
        If SelectedUserID >= 0 Then
            ADataGrid.CurrentUser = App.ActiveProject.ProjectManager.GetUserByID(SelectedUserID)
            If ADataGrid.CurrentUser.UserName <> "" Then sUserName = String.Format(" ({0})", ADataGrid.CurrentUser.UserName)
        Else
            ADataGrid.CurrentGroup = App.ActiveProject.ProjectManager.CombinedGroups.GetCombinedGroupByUserID(SelectedUserID)
            'ADataGrid.CurrentGroup = PM.CombinedGroups.GroupsList(ddlGroups.SelectedIndex)
            If ADataGrid.CurrentGroup.Name <> "" Then sUserName = String.Format(" [{0}]", ADataGrid.CurrentGroup.Name)
        End If
        ADataGrid.ProjectName = App.ActiveProject.ProjectName
        ADataGrid.AddNewAltsLabel = ResString("lblDataGridAddAlts")

        Select Case App.ActiveProject.PipeParameters.SynthesisMode
            Case ECSynthesisMode.smDistributive
                ADataGrid.CalcMode = ResString("lbl_smDistributive")
            Case ECSynthesisMode.smIdeal
                ADataGrid.CalcMode = ResString("lbl_smIdeal")
        End Select

        If ADataGrid.CurrentUser IsNot Nothing Then
            App.ActiveProject.ProjectManager.StorageManager.Reader.LoadUserData(ADataGrid.CurrentUser)
        ElseIf ADataGrid.CurrentGroup IsNot Nothing Then
            For Each u As clsUser In ADataGrid.CurrentGroup.UsersList
                App.ActiveProject.ProjectManager.StorageManager.Reader.LoadUserData(u)
            Next
        End If

        Dim CT As clsCalculationTarget = Nothing
        If ADataGrid.CurrentUser IsNot Nothing Then
            CT = New clsCalculationTarget(CalculationTargetType.cttUser, ADataGrid.CurrentUser)
            Dim tPrjUser As clsApplicationUser = Nothing
            If ADataGrid.CurrentUser.UserEMail.ToLower = App.ActiveUser.UserEmail.ToLower Then tPrjUser = App.ActiveUser Else tPrjUser = App.DBUserByEmail(ADataGrid.CurrentUser.UserEMail)
            If tPrjUser Is Nothing Then tPrjUser = App.ActiveUser
            If App.CanUserModifyProject(tPrjUser.UserID, App.ProjectID, App.DBUserWorkgroupByUserIDWorkgroupID(tPrjUser.UserID, App.ActiveWorkgroup.ID), App.DBWorkspaceByUserIDProjectID(tPrjUser.UserID, App.ProjectID), App.ActiveWorkgroup) Then
                ADataGrid.AttributesReadOnly = False
            Else
                ADataGrid.AttributesReadOnly = True
            End If
        ElseIf ADataGrid.CurrentGroup IsNot Nothing Then
            CT = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ADataGrid.CurrentGroup)
            ADataGrid.AttributesReadOnly = True
        End If

        Dim DGUserID As Integer = CT.GetUserID

        App.ActiveProject.ProjectManager.CalculationsManager.Calculate(CT, App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy).Nodes(0))

        Dim uncontributedEvents As List(Of clsNode) = If(PM.IsRiskProject, PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives(), New List(Of clsNode))
        For Each tp In App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy).NodesInLinearOrder()
            'For Each node In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
            Dim node As clsNode = tp.Item3
            If node.IsTerminalNode Then
                ADataGrid.Objs.Add(node)
            End If
        Next

        Dim altAttributes As List(Of clsAttribute) = App.ActiveProject.ProjectManager.Attributes.GetAlternativesAttributes()
        'remove unwanted alt attributes
        Dim aa As clsAttribute
        Dim i As Integer = altAttributes.Count - 1
        While i >= 0
            aa = altAttributes(i)
            If aa.IsDefault And Not aa.ID.Equals(ATTRIBUTE_COST_ID) And Not aa.ID.Equals(ATTRIBUTE_RISK_ID) And Not (aa.ID.Equals(ATTRIBUTE_KNOWN_VALUE_ID) And App.isRiskEnabled) Then
                altAttributes.Remove(aa)
            End If
            If App.isRiskEnabled And (aa.ID.Equals(ATTRIBUTE_COST_ID) Or aa.ID.Equals(ATTRIBUTE_RISK_ID)) Then
                altAttributes.Remove(aa)
            End If
            If aa.ID.Equals(ATTRIBUTE_MAPKEY_ID) And Not PM.UseDataMapping Then
                altAttributes.Remove(aa)
            End If
            If aa.ID = ATTRIBUTE_RISK_ID Then
                aa.Name = App.GetAttributeName(aa)
            End If
            ' D4356 ==
            i -= 1
        End While
        Dim RA As ResourceAligner = App.ActiveProject.ProjectManager.ResourceAligner
        If RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0 Then
            altAttributes.Add(New clsAttribute(clsProjectDataProvider.dgColEarliest, "Earliest", AttributeTypes.atAlternative, AttributeValueTypes.avtLong, 1))
            altAttributes.Add(New clsAttribute(clsProjectDataProvider.dgColLatest, "Latest", AttributeTypes.atAlternative, AttributeValueTypes.avtLong, 1))
            altAttributes.Add(New clsAttribute(clsProjectDataProvider.dgColDuration, "Duration", AttributeTypes.atAlternative, AttributeValueTypes.avtLong, 1))
        End If

        ADataGrid.AltAttributeNames = altAttributes
        For Each alt In App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).TerminalNodes
            ADataGrid.Alts.Add(alt)
            Dim AltTPData As AlternativePeriodsData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.NodeGuidID.ToString)
            Dim d As New Dictionary(Of Guid, String)
            For Each a As clsAttribute In ADataGrid.AltAttributeNames
                Select Case a.ValueType
                    Case AttributeValueTypes.avtBoolean, AttributeValueTypes.avtDouble, AttributeValueTypes.avtLong, AttributeValueTypes.avtString
                        Dim Val As String = ""
                        Try
                            Select Case a.ID
                                Case ATTRIBUTE_COST_ID
                                    Val = CType(App.ActiveProject.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.Alternatives.Find(Function(x) (x.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Cost, String)
                                Case ATTRIBUTE_RISK_ID
                                    Val = CType(App.ActiveProject.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.Alternatives.Find(Function(x) (x.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).RiskOriginal, String)
                                Case clsProjectDataProvider.dgColEarliest
                                    Val = CType(AltTPData.MinPeriod + 1, String)
                                Case clsProjectDataProvider.dgColLatest
                                    Val = CType(AltTPData.MaxPeriod + 1, String)
                                Case clsProjectDataProvider.dgColDuration
                                    Val = CType(AltTPData.Duration, String)
                                Case Else
                                    Dim v As Object = App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(a.ID, alt.NodeGuidID)   ' D7533
                                    Val = If(v Is Nothing, UNDEFINED_ATTRIBUTE_DEFAULT_VALUE, v.ToString)  ' D7533
                            End Select
                        Catch ex As Exception
                            Val = ""
                        End Try

                        If Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE.ToString Then
                            Val = ""
                        ElseIf Val = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE.ToString Then
                            Val = "0"
                        End If
                        d.Add(a.ID, Val)
                    Case AttributeValueTypes.avtEnumeration
                        Dim sValue As String = ""
                        Dim AValue As Object = App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(a.ID, alt.NodeGuidID)
                        If AValue IsNot Nothing Then
                            Dim Val As String = AValue.ToString
                            If Not String.IsNullOrEmpty(Val) AndAlso Val <> UNDEFINED_ATTRIBUTE_DEFAULT_VALUE Then  ' D7533
                                Dim enm As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(a.EnumID)
                                If enm IsNot Nothing Then
                                    Dim ei As clsAttributeEnumerationItem = enm.GetItemByID(New Guid(Val))
                                    If ei IsNot Nothing Then
                                        sValue = ei.Value
                                    End If
                                End If
                            End If
                        End If
                        d.Add(a.ID, sValue)
                    Case AttributeValueTypes.avtEnumerationMulti
                        Dim sValue As String = "["
                        Dim AValue As Object = App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(a.ID, alt.NodeGuidID)
                        If AValue IsNot Nothing Then
                            Dim Val As String = AValue.ToString
                            If Not String.IsNullOrEmpty(Val) AndAlso Val <> UNDEFINED_ATTRIBUTE_DEFAULT_VALUE Then  ' D7533
                                Dim enm As clsAttributeEnumeration = App.ActiveProject.ProjectManager.Attributes.GetEnumByID(a.EnumID)
                                If enm IsNot Nothing Then
                                    Dim isFirstVal As Boolean = True
                                    For Each sVal As String In Val.Split(CType(";", Char()))
                                        If sVal <> "" Then
                                            Dim ei As clsAttributeEnumerationItem = enm.GetItemByID(New Guid(sVal))
                                            If ei IsNot Nothing Then
                                                If isFirstVal Then
                                                    sValue += ei.Value
                                                    isFirstVal = False
                                                Else
                                                    sValue += ", " + ei.Value
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        sValue += "]"
                        d.Add(a.ID, sValue)
                End Select
            Next
            ADataGrid.AltAttributeValues.Add(alt.NodeGuidID, d)
        Next

        Dim DGData As Object = Nothing
        Dim DGPriority As Double = Nothing
        For Each alt As clsNode In ADataGrid.Alts
            'TOTAL
            ADataGrid.RatingValues.Add(New clsDataGrid.CellValue(alt.NodeID, clsProjectDataProvider.TotalCol, alt.NodeGuidID, clsDataGrid.TotalGUID, alt.UnnormalizedPriority))

            ' if event with no source
            If uncontributedEvents.Contains(alt) Then
                Select Case alt.MeasureType
                    Case ECMeasureType.mtRatings
                        Dim rData As clsRatingMeasureData = CType(alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeID, DGUserID), clsRatingMeasureData)
                        If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                            DGData = IIf(rData.Rating.ID <> -1, rData.Rating.Name, rData.Rating.Value)
                            DGPriority = rData.Rating.Value
                        Else
                            DGData = ""
                            DGPriority = 0
                        End If
                    Case Else
                        Dim nonpwData As clsNonPairwiseMeasureData = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeID, DGUserID)
                        If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                            DGData = CSng(nonpwData.ObjectValue)
                            DGPriority = nonpwData.SingleValue
                        Else
                            DGData = ""
                            DGPriority = 0
                        End If
                End Select
                ADataGrid.RatingValues.Add(New clsDataGrid.CellValue(alt.NodeID, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeID, alt.NodeGuidID, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, DGData, DGPriority))
            Else
                For Each covObj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes ' PM.ActiveHierarchy
                    Select Case covObj.MeasureType
                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                            DGPriority = covObj.Judgments.Weights.GetUserWeights(DGUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                            DGData = DGPriority
                        Case ECMeasureType.mtRatings
                            Dim rData As clsRatingMeasureData = CType(CType(covObj.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, covObj.NodeID, DGUserID), clsRatingMeasureData)
                            If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                                DGData = IIf(rData.Rating.ID <> -1, rData.Rating.Name, rData.Rating.Value)
                                DGPriority = rData.Rating.Value
                            Else
                                DGData = ""
                                DGPriority = 0
                            End If
                        Case Else
                            Dim nonpwData As clsNonPairwiseMeasureData = CType(covObj.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, covObj.NodeID, DGUserID)
                            If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                DGData = CSng(nonpwData.ObjectValue)
                                DGPriority = nonpwData.SingleValue
                            Else
                                DGData = ""
                                DGPriority = 0
                            End If
                    End Select

                    ADataGrid.RatingValues.Add(New clsDataGrid.CellValue(alt.NodeID, covObj.NodeID, alt.NodeGuidID, covObj.NodeGuidID, DGData, DGPriority))
                Next
            End If
        Next

        FileName = "xlsx"
        isExport = True

        ADataGrid.Write(filePath)

        If isExport Then
            FileName = GetProjectFileName(App.ActiveProject.ProjectName + sUserName, App.ActiveProject.Passcode, "AdHoc report", FileName)  ' D1492 + D1619
            DownloadFile(filePath, sContentType, FileName, dbObjectType.einfFile, App.ProjectID) ' D6593
            'RawResponseStart()
            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", HttpUtility.UrlEncode(SafeFileName(FileName))))     ' D1492 + D3478 + D6591
            'Response.ContentType = sContentType
            'Dim fileLen As Long = MyComputer.FileSystem.GetFileInfo(filePath).Length
            'Response.AddHeader("Content-Length", CStr(fileLen))
            'DebugInfo(String.Format("Start transferring for {0} bytes", fileLen))
            'Response.BinaryWrite(MyComputer.FileSystem.ReadAllBytes(filePath))
            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProjectReport, App.ProjectID, "Export DataGrid", String.Format("Filename: {0}; Size: {1}", FileName, fileLen))  ' D2236
            'File_Erase(filePath)
            'RawResponseEnd()
        End If
    End Sub

    Protected Sub RadToolbarMain_ButtonClick(sender As Object, e As RadToolBarEventArgs) 'Handles RadToolbarMain.ButtonClick
        Dim FileName As String = "DataGrid"
        Dim filePath As String = File_CreateTempName()
        Dim sContentType As String = "application/octet-stream"
        Dim isExport As Boolean = False
        Dim sUserName As String = ""    ' D1619

        Select Case CType(e.Item, RadToolBarButton).CommandName
            'Case "ExporttoRTF"
            '    ASPxPivotGridExporter2.ExportToRtf(filePath)
            '    sContentType = "application/msword"
            '    FileName = "rtf"
            '    isExport = True
            'Case "ExporttoPDF"
            '    ASPxPivotGridExporter2.ExportToPdf(filePath)
            '    sContentType = "application/pdf"
            '    FileName = "pdf"
            '    isExport = True
            'Case "ExporttoXLS"
            '    ASPxPivotGridExporter2.ExportToXls(filePath)
            '    sContentType = "application/vnd.ms-excel"
            '    FileName = "xls"
            '    isExport = True
            Case "Download"
            Case "DownloadTotalsByInstance" 'AS/11349
                Dim ADataGrid As New clsDataGrid()
                'If ddlGroups.SelectedIndex >= App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count Then
                '    ADataGrid.CurrentUser = App.ActiveProject.ProjectManager.UsersList(ddlGroups.SelectedIndex - App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count)
                '    If ADataGrid.CurrentUser.UserName <> "" Then sUserName = String.Format(" ({0})", ADataGrid.CurrentUser.UserName) ' D1619
                'Else
                '    ADataGrid.CurrentGroup = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup)
                '    If ADataGrid.CurrentGroup.Name <> "" Then sUserName = String.Format(" [{0}]", ADataGrid.CurrentGroup.Name) ' D1619
                'End If
                ADataGrid.ProjectName = App.ActiveProject.ProjectName 'AS/11339===
                ADataGrid.AddNewAltsLabel = ResString("lblDataGridAddAlts")

                Select Case App.ActiveProject.PipeParameters.SynthesisMode
                    Case ECSynthesisMode.smDistributive
                        ADataGrid.CalcMode = ResString("lbl_smDistributive")
                    Case ECSynthesisMode.smIdeal
                        ADataGrid.CalcMode = ResString("lbl_smIdeal")
                End Select 'AS/11339==

                If ADataGrid.CurrentUser IsNot Nothing Then
                    PM.StorageManager.Reader.LoadUserData(ADataGrid.CurrentUser)
                ElseIf ADataGrid.CurrentGroup IsNot Nothing Then
                    For Each u As clsUser In ADataGrid.CurrentGroup.UsersList
                        PM.StorageManager.Reader.LoadUserData(u)
                    Next
                End If

                'For Each user As clsUser In PM.UsersList 'AS/11339=== incorporated loop through all participants
                '        PM.StorageManager.Reader.LoadUserData(user)
                'Dim user As clsUser = PM.UsersList(2) 'AS/11339 temp do fo a single user
                'PM.StorageManager.Reader.LoadUserData(user) 'AS/11339
                Dim CT As clsCalculationTarget = Nothing

                If ADataGrid.CurrentUser IsNot Nothing Then
                    CT = New clsCalculationTarget(CalculationTargetType.cttUser, User)
                    Dim tPrjUser As clsApplicationUser = Nothing
                    If ADataGrid.CurrentUser.UserEMail.ToLower = App.ActiveUser.UserEmail.ToLower Then tPrjUser = App.ActiveUser Else tPrjUser = App.DBUserByEmail(ADataGrid.CurrentUser.UserEMail)
                    If tPrjUser Is Nothing Then tPrjUser = App.ActiveUser
                    If App.CanUserModifyProject(tPrjUser.UserID, App.ProjectID, App.DBUserWorkgroupByUserIDWorkgroupID(tPrjUser.UserID, App.ActiveWorkgroup.ID), App.DBWorkspaceByUserIDProjectID(tPrjUser.UserID, App.ProjectID), App.ActiveWorkgroup) Then
                        ADataGrid.AttributesReadOnly = False
                    Else
                        ADataGrid.AttributesReadOnly = True
                    End If
                ElseIf ADataGrid.CurrentGroup IsNot Nothing Then
                    CT = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ADataGrid.CurrentGroup)
                    ADataGrid.AttributesReadOnly = True
                End If

                Dim DGUserID As Integer = CT.GetUserID

                PM.CalculationsManager.Calculate(CT, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0))

                For Each node In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
                    ADataGrid.Objs.Add(node)
                Next

                Dim altAttributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes()
                'remove unwanted alt attributes
                Dim aa As clsAttribute
                Dim i As Integer = altAttributes.Count - 1
                While i >= 0
                    aa = altAttributes(i)
                    If aa.ID = ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID Or aa.ID = ATTRIBUTE_RA_ALT_SORT_ID Then
                        altAttributes.Remove(aa)
                    End If
                    If aa.ID = ATTRIBUTE_KNOWN_VALUE_ID And Not App.isRiskEnabled Then
                        altAttributes.Remove(aa)
                    End If
                    i -= 1
                End While

                ADataGrid.AltAttributeNames = altAttributes
                For Each alt In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                    ADataGrid.Alts.Add(alt)
                    Dim d As New Dictionary(Of Guid, String)
                    For Each a As clsAttribute In ADataGrid.AltAttributeNames
                        Select Case a.ValueType
                            Case AttributeValueTypes.avtBoolean, AttributeValueTypes.avtDouble, AttributeValueTypes.avtLong, AttributeValueTypes.avtString
                                Dim Val As String = ""
                                Try
                                    Select Case a.ID
                                        Case ATTRIBUTE_COST_ID
                                            Val = CType(App.ActiveProject.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.Alternatives.Find(Function(x) (x.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Cost, String)
                                        Case ATTRIBUTE_RISK_ID
                                            Val = CType(App.ActiveProject.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.Alternatives.Find(Function(x) (x.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).RiskOriginal, String)
                                        Case Else
                                            Val = PM.Attributes.GetAttributeValue(a.ID, alt.NodeGuidID).ToString
                                    End Select
                                Catch ex As Exception
                                    Val = ""
                                End Try

                                If Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE.ToString Then
                                    Val = ""
                                ElseIf Val = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE.ToString Then
                                    Val = "0"
                                End If
                                d.Add(a.ID, Val)
                            Case AttributeValueTypes.avtEnumeration
                                Dim sValue As String = ""
                                Dim AValue As Object = PM.Attributes.GetAttributeValue(a.ID, alt.NodeGuidID)
                                If AValue IsNot Nothing Then
                                    Dim Val As String = AValue.ToString
                                    If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) Then
                                        Dim enm As clsAttributeEnumeration = PM.Attributes.GetEnumByID(a.EnumID)
                                        If enm IsNot Nothing Then
                                            Dim ei As clsAttributeEnumerationItem = enm.GetItemByID(New Guid(Val))
                                            If ei IsNot Nothing Then
                                                sValue = ei.Value
                                            End If
                                        End If
                                    End If
                                End If
                                d.Add(a.ID, sValue)
                            Case AttributeValueTypes.avtEnumerationMulti
                                Dim sValue As String = "["
                                Dim AValue As Object = PM.Attributes.GetAttributeValue(a.ID, alt.NodeGuidID)
                                If AValue IsNot Nothing Then
                                    Dim Val As String = AValue.ToString
                                    If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) Then
                                        Dim enm As clsAttributeEnumeration = PM.Attributes.GetEnumByID(a.EnumID)
                                        If enm IsNot Nothing Then
                                            Dim isFirstVal As Boolean = True
                                            For Each sVal As String In Val.Split(CType(";", Char()))
                                                Dim ei As clsAttributeEnumerationItem = enm.GetItemByID(New Guid(sVal))
                                                If ei IsNot Nothing Then
                                                    If isFirstVal Then
                                                        sValue += ei.Value
                                                        isFirstVal = False
                                                    Else
                                                        sValue += ", " + ei.Value
                                                    End If
                                                End If
                                            Next
                                        End If
                                    End If
                                End If
                                sValue += "]"
                                d.Add(a.ID, sValue)
                        End Select
                    Next
                    ADataGrid.AltAttributeValues.Add(alt.NodeGuidID, d)
                Next

                For Each alt As clsNode In ADataGrid.Alts
                    'TOTAL
                    ADataGrid.RatingValues.Add(New clsDataGrid.CellValue(alt.NodeID, clsProjectDataProvider.TotalCol, alt.NodeGuidID, clsDataGrid.TotalGUID, alt.UnnormalizedPriority))

                    For Each covObj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes ' PM.ActiveHierarchy
                        Dim DGData As Object = Nothing
                        Dim DGPriority As Double = Nothing
                        Select Case covObj.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                DGPriority = covObj.Judgments.Weights.GetUserWeights(DGUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                DGData = DGPriority
                            Case ECMeasureType.mtRatings
                                Dim rData As clsRatingMeasureData = CType(CType(covObj.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, covObj.NodeID, DGUserID), clsRatingMeasureData)
                                If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                                    DGData = IIf(rData.Rating.ID <> -1, rData.Rating.Name, rData.Rating.Value)
                                    DGPriority = rData.Rating.Value
                                Else
                                    DGData = ""
                                    DGPriority = 0
                                End If
                            Case Else
                                Dim nonpwData As clsNonPairwiseMeasureData = CType(covObj.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, covObj.NodeID, DGUserID)
                                If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                    DGData = CSng(nonpwData.ObjectValue)
                                    DGPriority = nonpwData.SingleValue
                                Else
                                    DGData = ""
                                    DGPriority = 0
                                End If
                        End Select
                        ADataGrid.RatingValues.Add(New clsDataGrid.CellValue(alt.NodeID, covObj.NodeID, alt.NodeGuidID, covObj.NodeGuidID, DGData, DGPriority))
                    Next
                Next

                FileName = "xlsx"
                isExport = True

                ADataGrid.WriteTotalsByInstance(filePath, PM)
                ADataGrid.AltAttributeValues.Clear()

        End Select

        If isExport Then
            FileName = GetProjectFileName(App.ActiveProject.ProjectName + sUserName, App.ActiveProject.Passcode, "AdHoc report", FileName)  ' D1492 + D1619
            DownloadFile(filePath, sContentType, FileName, dbObjectType.einfFile, App.ProjectID)   ' D6593
            'RawResponseStart()
            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", SafeFileName(FileName)))     ' D1492 + D3478
            'Response.ContentType = sContentType
            'Dim fileLen As Long = MyComputer.FileSystem.GetFileInfo(filePath).Length
            'Response.AddHeader("Content-Length", CStr(fileLen))
            'DebugInfo(String.Format("Start transferring for {0} bytes", fileLen))
            'Response.BinaryWrite(MyComputer.FileSystem.ReadAllBytes(filePath))
            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProjectReport, App.ProjectID, "Export DataGrid", String.Format("Filename: {0}; Size: {1}", FileName, fileLen))  ' D2236
            'File_Erase(filePath)
            'RawResponseEnd()
        End If
    End Sub
    'L0503 ==
    Public Function GetDataMapping() As String
        Dim retVal As String = ""
        For Each obj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).Nodes() ' D3925
            If obj.InfoDoc <> "" Then
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{nodeGuid:'{0}'}}", obj.NodeGuidID)
            End If
        Next
        For Each alt As clsNode In PM.AltsHierarchies(0).Nodes
            If alt.InfoDoc <> "" Then
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{nodeGuid:'{0}'}}", alt.NodeGuidID)
            End If
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function IsGroupUser() As Integer 'AS/16038
        If SelectedUserID >= 0 Then 'AS/21354c===
            IsGroupSelected = 0
        Else
            IsGroupSelected = 1
        End If 'AS/21354c==

        Return IsGroupSelected
    End Function

    Protected Sub ddlGroups_Load(sender As Object, e As EventArgs)
        'If Not IsPostBack And Session("GroupUserID" + App.ActiveProject.ID.ToString) IsNot Nothing Then
        '    ddlGroups.SelectedIndex = CInt(Session("GroupUserID" + App.ActiveProject.ID.ToString))
        'End If
        'If ddlGroups.SelectedIndex >= App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count Then
        '    CType(RadToolbarMain.Items(3), RadToolBarButton).Text = "Upload to " + ddlGroups.SelectedItem.Text
        '    CType(RadToolbarMain.Items(3), RadToolBarButton).Enabled = True
        'Else
        '    CType(RadToolbarMain.Items(3), RadToolBarButton).Enabled = False
        '    CType(RadToolbarMain.Items(3), RadToolBarButton).Text = "Upload"
        'End If
        'IsGroupSelected = 1 'AS/16038 'AS/21354c

    End Sub
    'L0493 ==

    '    Protected Sub ddlGroups_SelectedIndexChanged(sender As Object, e As EventArgs)
    '        'LoadGrid()
    '        Dim UID As Integer
    '        If ddlGroups.SelectedIndex >= PM.CombinedGroups.GroupsList.Count Then
    '            UID = PM.UsersList(ddlGroups.SelectedIndex - PM.CombinedGroups.GroupsList.Count).UserID
    '        Else
    '            UID = CType(PM.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
    '        End If
    '        SelectedUserID = UID
    '    End Sub
    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public ReadOnly Property IDColumnMode As String
        Get
            Dim tSess As Object = SessVar("IDColumnMode" + App.ActiveProject.ID.ToString)
            If String.IsNullOrEmpty(CType(tSess, String)) OrElse CStr(tSess) = "0" Then Return "id" Else Return "index"
        End Get
    End Property

    Public Function GetRows() As String
        Dim UserIDs As New List(Of Integer)
        UserIDs.Add(SelectedUserID)
        Return datagrid_GetRows(PM, UserIDs, True)
    End Function

    Public Function GetColumns() As String
        Dim UserIDs As New List(Of Integer)
        UserIDs.Add(SelectedUserID)
        Return datagrid_GetColumns(PM, UserIDs, True)
    End Function

    Public Function GetHierarchyColumns() As String
        Return datagrid_GetHierarchyColumns(PM)
    End Function

    'A1201 ===
    Public Function GetAttributes() As String
        Return datagrid_GetAttributes(PM, App, True)
    End Function
    'A1201 ==

    Public Function GetDataMappings() As String 'AS/25743a
        Return datagrid_GetDataMappings(PM, App, True)
    End Function

    Public Function HasMapKey() As String
        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
            Dim sValue As String = ""
            sValue = PM.Attributes.GetAttributeValueString(ECCore.ATTRIBUTE_MAPKEY_ID, alt.NodeGuidID)
            If sValue <> "" Then
                Return True.ToString.ToLower()
            End If
        Next

        Return False.ToString.ToLower()
    End Function
    Public Function GetAltsDMGUID() As String
        For Each DM As clsDataMapping In PM.DataMappings
            If DM.eccMappedColID.Equals(clsProjectDataProvider.dgColAltName) Then
                Return DM.DataMappingGUID.ToString.ToLower
            End If
        Next
        Return Guid.Empty.ToString.ToLower
    End Function

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", String.Format("['{0}','']", sAction)))
        Dim sSnapshotMessage As String = ""
        Dim sSnapshotComment As String = ""

        Select Case sAction
            ' -D4746
            'Case "download"
            '    DownloadDataGrid()
            '    tResult = String.Format("['{0}','']", "downloaded")
            Case "select_columns"
                Dim tColumnsIDs As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "column_ids"))
                HiddenColumns(App.ActiveProject) = tColumnsIDs
                tResult = String.Format("['{0}','']", "updated")
                'A1201 ==
            Case "contributed_nodes"
                Dim EventID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "event_id")).ToLower) ' Anti-XSS
                Dim alt = PM.AltsHierarchy(PM.ActiveAltsHierarchy).Nodes(EventID)
                Dim retVal As String = ""
                Dim H As clsHierarchy = Nothing
                If App.isRiskEnabled Then
                    If PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then H = PM.Hierarchy(ECHierarchyID.hidImpact)
                    If PM.ActiveHierarchy = ECHierarchyID.hidImpact Then H = PM.Hierarchy(ECHierarchyID.hidLikelihood)
                Else
                    H = PM.Hierarchy(PM.ActiveHierarchy)
                End If
                For Each obj In H.TerminalNodes
                    If obj.GetContributedAlternatives.Contains(alt) Then
                        retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},'{1}']", obj.NodeID, JS_SafeString(obj.NodeName))
                    End If
                Next
                tResult = String.Format("['{0}',[{1}]]", "contributed_nodes", retVal)
            Case "setresource"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower  ' Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower  ' Anti-XSS
                Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower    ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower   ' Anti-XSS
                tResult = String.Format("['{0}','']", "ok")
            Case "delete_event"
                Dim sAltID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "event_id")) ' Anti-XSS                
                Dim AltID As Integer = CInt(sAltID)
                Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                Dim Alt As clsNode = AltH.GetNodeByID(AltID)
                If Alt IsNot Nothing Then
                    sSnapshotMessage = "Delete " + ParseString("%%alternative%%")
                    sSnapshotComment = String.Format("Delete ""{0}""", Alt.NodeName)
                    AltH.DeleteNode(Alt, False)
                    tResult = String.Format("['{0}','']", "ok")
                End If
            Case "update_event_name"
                Dim sAltID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "event_id"))  ' Anti-XSS                
                Dim AltID As Integer = CInt(sAltID)
                Dim sAltName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))  ' Anti-XSS                
                Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                Dim Alt As clsNode = AltH.GetNodeByID(AltID)
                If Alt IsNot Nothing Then
                    sSnapshotMessage = "Update " + ParseString("%%alternative%%") + " name"
                    sSnapshotComment = String.Format("Update ""{0}"" name", Alt.NodeName)
                    Alt.NodeName = sAltName
                    tResult = String.Format("['{0}','']", "ok")
                End If

                ' D4491 ===
            Case "dm_delete"
                Dim sGUID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid")).ToLower.Trim
                Dim sName As String = ""
                If sGUID <> "" Then
                    Dim tGUID As New Guid(sGUID)
                    Dim DMGuid As Guid
                    If tGUID.Equals(clsProjectDataProvider.dgColAltName) Then
                        sName = "Attributes list"
                        Dim Dm As clsDataMapping = PM.DataMappings.Find(Function(d) d.eccMappedColID.Equals(clsProjectDataProvider.dgColAltName))
                        If Dm IsNot Nothing Then DMGuid = Dm.DataMappingGUID
                    Else
                        Dim tAttr As clsAttribute = PM.Attributes.GetAttributeByID(tGUID)
                        If tAttr IsNot Nothing Then
                            DMGuid = tAttr.DataMappingGUID
                            tAttr.DataMappingGUID = Guid.Empty
                            sName = String.Format("Attribute '{0}'", App.GetAttributeName(tAttr))
                        Else 'AS/16309=== check if it is covering objective
                            Dim objH As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
                            Dim tNode As clsNode = objH.GetNodeByID(tGUID)
                            If tNode IsNot Nothing Then
                                DMGuid = tNode.DataMappingGUID
                                tNode.DataMappingGUID = Guid.Empty
                                sName = String.Format("Node '{0}'", tNode.NodeName)
                            End If 'AS/16309==
                        End If
                    End If
                    If Not DMGuid.Equals(Guid.Empty) Then
                        Dim DM As clsDataMapping = PM.DataMappings.Find(Function(d) d.DataMappingGUID.Equals(DMGuid))
                        If DM IsNot Nothing Then PM.DataMappings.Remove(DM)

                        Using PgDM As New DataMappingPage 'AS/24507===
                            If PM.DataMappings.Count = 0 Then
                                PgDM.ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtSQL 'default
                            End If
                        End Using 'AS/24507==

                    End If
                    If sName <> "" Then App.ActiveProject.SaveStructure("Delete Data Mapping", , , sName)
                End If
                tResult = Bool2Num(sName <> "").ToString
                ' D4491 ==
            Case "userchanged"
                SelectedUserID = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")))
                Dim sm As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "sm")))
                Dim nm As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "nm")))
                PM.Parameters.Normalization = CType(nm, LocalNormalizationType)
                PM.PipeParameters.SynthesisMode = CType(sm, ECSynthesisMode)
                If SelectedUserID >= 0 Then 'AS/21354c===
                IsGroupSelected = 0
                Else
                    IsGroupSelected = 1
                End If 'AS/21354c==
                tResult = String.Format("['{0}',{1}]", "rows", GetRows())
        End Select

        If tResult <> "" Then
            If tResult.StartsWith("['ok',") Then App.ActiveProject.SaveStructure(sSnapshotMessage, True, True, sSnapshotComment) 'A1202 'If tResult = "'ok'" Then PM.StorageManager.Writer.SaveProject(True,)
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Sub DataGridPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        CurrentPageID = If(CheckVar("mode", "").ToLower = "synth" OrElse CheckVar("upload", True), _PGID_REPORT_DATAGRID, _PGID_REPORT_DATAGRID_NOUPLOAD)
    End Sub

End Class