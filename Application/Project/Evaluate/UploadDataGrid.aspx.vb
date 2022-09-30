Imports System.Diagnostics

Partial Class DataGridUpload
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_REPORT_DATAGRID_UPLOAD)
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not IsPostBack AndAlso Not isCallback Then
            AlignHorizontalCenter = False
            AlignVerticalCenter = False
            btnUpload.Text = ResString("btnUpload")
            FileUpload.Attributes.Add("onchange", "CheckUpload()")
            FileUpload.Attributes.Add("onkeydown", "CheckUpload()")
            btnUpload.Enabled = False
            ClientScript.RegisterOnSubmitStatement(GetType(String), "onSubmit", String.Format("showLoadingPanel('{0}');", JS_SafeString(ResString("lblFileUploading"))))   ' D7056
        End If
    End Sub

    Public Class DataGridCell
        Private fAltID As Guid
        Private fCovObjID As Guid
        Private fDataValue As Single = 0
        Private fDataVerbalValue As String = ""
        Public Property AltID As Guid
            Get
                Return fAltID
            End Get
            Set(value As Guid)
                fAltID = value
            End Set
        End Property
        Public Property CovObjID As Guid
            Get
                Return fCovObjID
            End Get
            Set(value As Guid)
                fCovObjID = value
            End Set
        End Property
        Public Property DataValue As Single
            Get
                Return fDataValue
            End Get
            Set(value As Single)
                fDataValue = value
            End Set
        End Property
        Public Property DataVerbalValue As String
            Get
                Return fDataVerbalValue
            End Get
            Set(value As String)
                fDataVerbalValue = value
            End Set
        End Property
        Public Sub New(AAltID As Guid, ACovObjID As Guid, ADataValue As Single, ADataVerbalValue As String)
            AltID = AAltID
            CovObjID = ACovObjID
            DataValue = ADataValue
            DataVerbalValue = ADataVerbalValue
        End Sub
    End Class

    Protected Sub btnUpload_Click(sender As Object, e As EventArgs) Handles btnUpload.Click
        Dim fUploaded As Boolean = False
        If FileUpload.HasFile Then

            Dim sOriginalFile As String = FileUpload.FileName

            Dim sUploadedFileName As String = File_CreateTempName()
            FileUpload.SaveAs(sUploadedFileName)

            Dim UserID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(CheckVar("id", "")))   ' D1559 + Anti-XSS
            Dim SelectedProjectUser As clsUser = App.ActiveProject.ProjectManager.GetUserByID(UserID)
            Dim SelectedUserEmail As String = ""
            If SelectedProjectUser IsNot Nothing Then
                SelectedUserEmail = SelectedProjectUser.UserEMail.ToLower
            End If
            Dim dg As Dictionary(Of String, clsDataGrid.AltDataItem) = Nothing  'D1632
            Dim dgError As DataGridException = Nothing

            Dim IsProjectManager As Boolean = False
            Dim tPrjUser As clsApplicationUser = Nothing
            If SelectedUserEmail = App.ActiveUser.UserEmail.ToLower Then tPrjUser = App.ActiveUser Else tPrjUser = App.DBUserByEmail(SelectedUserEmail)
            If tPrjUser Is Nothing Then tPrjUser = App.ActiveUser
            If App.CanUserModifyProject(tPrjUser.UserID, App.ProjectID, App.DBUserWorkgroupByUserIDWorkgroupID(tPrjUser.UserID, App.ActiveWorkgroup.ID), App.DBWorkspaceByUserIDProjectID(tPrjUser.UserID, App.ProjectID), App.ActiveWorkgroup) Then
                IsProjectManager = True
            End If

            Try
                Dim H As clsHierarchy = App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy)
                Dim Nodes As New Dictionary(Of Guid, clsNode)
                For Each n As clsNode In H.Nodes
                    Nodes.Add(n.NodeGuidID, n)
                Next
                dg = clsDataGrid.Read(sUploadedFileName, Nodes, App.ActiveProject.ProjectManager, IsProjectManager)
            Catch ex As DataGridException
                dgError = ex
                dg = Nothing
            End Try

            If dgError Is Nothing Then
                Dim nodeMapping As New Dictionary(Of Guid, String)
                For Each n As clsNode In App.ActiveProject.ProjectManager.Hierarchies(0).TerminalNodes
                    nodeMapping.Add(n.NodeGuidID, n.NodeID.ToString)
                Next

                Dim altAttribMapping As New Dictionary(Of Guid, String)
                For Each n As clsAttribute In App.ActiveProject.ProjectManager.Attributes.GetAlternativesAttributes
                    altAttribMapping.Add(n.ID, CInt(n.ValueType).ToString)
                Next

                Dim DataGridData As New List(Of DataGridCell)
                With App.ActiveProject
                    Dim fAttributesChanged As Boolean = False 'A1486

                    Dim Hierarchy As clsHierarchy = .ProjectManager.AltsHierarchy(.ProjectManager.ActiveAltsHierarchy)
                    Dim uploadedAlts As New Dictionary(Of Guid, clsNode)
                    For Each kvp As KeyValuePair(Of String, clsDataGrid.AltDataItem) In dg
                        Dim alt As clsDataGrid.AltDataItem = kvp.Value
                        If alt.NewAlt Then
                            If IsProjectManager Then
                                Dim findNode As clsNode = Hierarchy.Nodes.Find(Function(c As clsNode) c.NodeName = alt.Name)
                                If findNode Is Nothing Then
                                    'add new node to model
                                    If Hierarchy IsNot Nothing Then
                                        Dim node As clsNode
                                        node = Hierarchy.AddNode(-1)
                                        node.NodeName = alt.Name
                                        alt.ID = node.NodeGuidID
                                    End If
                                Else
                                    'Node was added already using same datagrid.  Not a good practice but it should work.
                                    alt.ID = findNode.NodeGuidID
                                End If
                            End If
                        End If

                        For Each col As KeyValuePair(Of Guid, String) In alt.CoveringData
                            Dim ANumValue As Single = 0
                            DataGridData.Add(New DataGridCell(alt.ID, col.Key, 0, col.Value))
                        Next

                        If IsProjectManager Then
                            Dim a As clsNode = Hierarchy.GetNodeByID(alt.ID)
                            If a IsNot Nothing Then
                                If alt.Name <> "" Or Not alt.NewAlt Then
                                    uploadedAlts.Add(a.NodeGuidID, a)
                                End If
                                For Each attr As KeyValuePair(Of Guid, String) In alt.CustomAttrData
                                    If a IsNot Nothing AndAlso (altAttribMapping.ContainsKey(attr.Key) Or (attr.Key.ToString.ToLower = clsProjectDataProvider.dgColEarliest.ToString.ToLower) Or (attr.Key.ToString.ToLower = clsProjectDataProvider.dgColLatest.ToString.ToLower) Or (attr.Key.ToString.ToLower = clsProjectDataProvider.dgColDuration.ToString.ToLower)) Then
                                        Dim aType As AttributeValueTypes
                                        If altAttribMapping.ContainsKey(attr.Key) Then
                                            aType = CType(CInt(altAttribMapping(attr.Key)), AttributeValueTypes)
                                        Else
                                            aType = AttributeValueTypes.avtLong
                                        End If

                                        Dim atValue As Object = Nothing
                                        Select Case aType
                                            Case AttributeValueTypes.avtBoolean
                                                Try
                                                    atValue = CBool(attr.Value)
                                                Catch ex As Exception
                                                    atValue = False
                                                End Try
                                            Case AttributeValueTypes.avtDouble
                                                Try
                                                    atValue = CDbl(attr.Value)
                                                Catch ex As Exception
                                                    atValue = 0
                                                End Try
                                            Case AttributeValueTypes.avtLong
                                                Try
                                                    atValue = CLng(attr.Value)
                                                Catch ex As Exception
                                                    atValue = 0
                                                End Try
                                            Case AttributeValueTypes.avtString
                                                Try
                                                    atValue = CStr(attr.Value).Trim
                                                Catch ex As Exception
                                                    atValue = ""
                                                End Try
                                            Case AttributeValueTypes.avtEnumeration
                                                Try
                                                    atValue = CStr(attr.Value).Trim
                                                Catch ex As Exception
                                                    atValue = ""
                                                End Try

                                                If atValue.ToString <> "" Then
                                                    Dim attribute As clsAttribute = .ProjectManager.Attributes.GetAttributeByID(attr.Key)
                                                    If attribute IsNot Nothing Then
                                                        Dim AttrEnum As clsAttributeEnumeration = .ProjectManager.Attributes.GetEnumByID(attribute.EnumID)
                                                        If AttrEnum IsNot Nothing Then
                                                            Dim AttrEnumItem As clsAttributeEnumerationItem = AttrEnum.GetItemByValue(atValue.ToString)
                                                            If AttrEnumItem Is Nothing Then
                                                                atValue = AttrEnum.AddItem(atValue.ToString).ID 'A1486
                                                                '-A1486 If attribute.DefaultValue IsNot Nothing Then
                                                                '-A1486     AttrEnumItem = AttrEnum.GetItemByValue(attribute.DefaultValue)
                                                                '-A1486     If AttrEnumItem Is Nothing Then AttrEnumItem = AttrEnum.GetItemByID(New Guid(attribute.DefaultValue.ToString))
                                                                '-A1486     If AttrEnumItem IsNot Nothing Then atValue = AttrEnumItem.ID Else atValue = Nothing
                                                                '-A1486 Else
                                                                '-A1486     atValue = Nothing
                                                                '-A1486 End If
                                                                '-A1486 'AttrEnumItem = AttrEnum.AddItem(atValue)
                                                            Else
                                                                atValue = AttrEnumItem.ID
                                                            End If
                                                            'A1486 ===
                                                        Else
                                                            Dim newEnum As clsAttributeEnumeration = New clsAttributeEnumeration With {.ID = Guid.NewGuid(), .Name = ""}
                                                            Dim newItem As clsAttributeEnumerationItem = newEnum.AddItem(atValue.ToString)
                                                            atValue = newItem.ID
                                                            attribute.EnumID = newEnum.ID
                                                            .ProjectManager.Attributes.Enumerations.Add(newEnum)
                                                            fAttributesChanged = True
                                                        End If
                                                        'A1486 ==
                                                    End If

                                                Else
                                                    atValue = Nothing
                                                End If
                                            Case AttributeValueTypes.avtEnumerationMulti
                                                Dim attribute As clsAttribute = .ProjectManager.Attributes.GetAttributeByID(attr.Key)
                                                If attribute IsNot Nothing Then

                                                End If
                                            Case Else
                                                atValue = Nothing
                                        End Select
                                        'If atValue IsNot Nothing Then
                                        Dim RA = .ProjectManager.ResourceAligner


                                        Select Case attr.Key.ToString.ToLower
                                            Case ATTRIBUTE_COST_ID.ToString.ToLower
                                                .ProjectManager.ResourceAligner.SetAlternativeCost(a.NodeGuidID.ToString, CDbl(atValue))
                                                'SetCostValue(a.NodeGuidID, atValue)
                                            Case (ATTRIBUTE_RISK_ID.ToString.ToLower)
                                                .ProjectManager.ResourceAligner.SetAlternativeRisk(a.NodeGuidID.ToString, CDbl(atValue))
                                                'SetRiskValue(a.NodeGuidID, atValue)
                                            Case clsProjectDataProvider.dgColEarliest.ToString.ToLower
                                                RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMinPeriod(alt.ID.ToString, CInt(atValue) - 1)
                                            Case clsProjectDataProvider.dgColLatest.ToString.ToLower
                                                RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetMaxPeriod(alt.ID.ToString, CInt(atValue) - 1)
                                            Case clsProjectDataProvider.dgColDuration.ToString.ToLower
                                                RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.SetDuration(alt.ID.ToString, CInt(atValue))
                                                '-A1486 Case Else
                                                '-A1486     .ProjectManager.Attributes.SetAttributeValue(attr.Key, UNDEFINED_USER_ID, aType, atValue, a.NodeGuidID, Guid.Empty)
                                        End Select
                                        '-A1486 If .ProjectManager.ResourceAligner.Scenarios.ActiveScenario IsNot Nothing AndAlso .ProjectManager.ResourceAligner.Scenarios.ActiveScenario.ID = 0 Then
                                        If atValue IsNot Nothing Then .ProjectManager.Attributes.SetAttributeValue(attr.Key, UNDEFINED_USER_ID, aType, atValue, a.NodeGuidID, Guid.Empty)
                                        '-A1486 End If
                                        'End If
                                        If alt.Name <> "" Then a.NodeName = alt.Name
                                    End If
                                Next
                            End If
                        End If
                    Next

                    Dim AltsToDelete As New Dictionary(Of Guid, clsNode)
                    If IsProjectManager Then ' delete nodes not in the uploaded spreadsheet
                        For Each node As clsNode In Hierarchy.Nodes
                            If Not uploadedAlts.ContainsKey(node.NodeGuidID) Then
                                AltsToDelete.Add(node.NodeGuidID, node)
                            End If
                        Next
                        For Each p As KeyValuePair(Of Guid, clsNode) In AltsToDelete
                            Dim node As clsNode = p.Value
                            'work with AC to delete node
                            Hierarchy.DeleteNode(node)
                            'Debug.Print(node.NodeName)
                        Next
                    End If

                    'A1486 ===
                    If fAttributesChanged Then
                        .ProjectManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .ProjectManager.StorageManager.ProjectLocation, .ProjectManager.StorageManager.ProviderType, .ProjectManager.StorageManager.ModelID)
                    End If

                    .ProjectManager.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .ProjectManager.StorageManager.ProjectLocation, .ProjectManager.StorageManager.ProviderType, .ProjectManager.StorageManager.ModelID, UNDEFINED_USER_ID)
                    .ProjectManager.ResourceAligner.Save()
                    .ProjectManager.StorageManager.Writer.SaveProject(True)

                    If .ProjectManager IsNot Nothing Then
                        Dim user As clsUser = .ProjectManager.GetUserByID(UserID) 'FB871 --- replace Admin with selected user in dropdown.
                        If user IsNot Nothing Then
                            Dim H As clsHierarchy = .ProjectManager.Hierarchy(.ProjectManager.ActiveHierarchy)
                            Dim AH As clsHierarchy = .ProjectManager.AltsHierarchy(.ProjectManager.ActiveAltsHierarchy)

                            .ProjectManager.StorageManager.Reader.LoadUserData(user)

                            'Debug.Print(" === DataGrid Cells start ===")

                            Dim CovObj As clsNode
                            Dim Alt As clsNode
                            For Each cell As DataGridCell In DataGridData
                                'Debug.Print("AltID = " + cell.AltID.ToString + " NodeID = " + cell.CovObjID.ToString + " DataValue = " + cell.DataValue.ToString + " DataVerbalValue = " + cell.DataVerbalValue.ToString)

                                CovObj = H.GetNodeByID(cell.CovObjID)
                                Alt = AH.GetNodeByID(cell.AltID)
                                If Alt IsNot Nothing AndAlso AltsToDelete.ContainsKey(Alt.NodeGuidID) Then
                                    Alt = Nothing
                                End If
                                If CovObj IsNot Nothing And Alt IsNot Nothing Then
                                    Dim nonpwData As clsNonPairwiseMeasureData = Nothing
                                    Select Case CovObj.MeasureType
                                        Case ECMeasureType.mtRatings
                                            If cell.DataVerbalValue = "" Then
                                                ' create blank judgment to erase existing one if there is one
                                                nonpwData = New clsRatingMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, Nothing, CType(CovObj.MeasurementScale, clsRatingScale), True)
                                            Else
                                                Dim rating As clsRating = Nothing
                                                If CovObj.MeasurementScale IsNot Nothing Then
                                                    rating = CType(CovObj.MeasurementScale, clsRatingScale).GetRatingByName(cell.DataVerbalValue)
                                                End If
                                                If rating IsNot Nothing Then
                                                        nonpwData = New clsRatingMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, rating, CType(CovObj.MeasurementScale, clsRatingScale))
                                                    Else
                                                        Dim aSingle As Single
                                                        If Single.TryParse(MiscFuncs.FixStringWithSingleValue(cell.DataVerbalValue), aSingle) Then
                                                            Dim R As New clsRating
                                                            R.ID = -1
                                                            R.Name = "Direct Entry from EC11.5"
                                                            R.Value = aSingle

                                                            ' we're creating dummy ratings with special intensity ID = -1 which means that this is a direct entry and passing Nothing as measurement scale

                                                            nonpwData = New clsRatingMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, R, Nothing)
                                                        End If
                                                    End If
                                                End If

                                                Case ECMeasureType.mtDirect, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep

                                            If cell.DataVerbalValue = "" Then
                                                Select Case CovObj.MeasureType
                                                    Case ECMeasureType.mtDirect
                                                        nonpwData = New clsDirectMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, 0, True)
                                                    Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                                                        nonpwData = New clsUtilityCurveMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, 0, CType(CovObj.MeasurementScale, clsCustomUtilityCurve), True)
                                                    Case ECMeasureType.mtStep
                                                        nonpwData = New clsStepMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, 0, CType(CovObj.MeasurementScale, clsStepFunction), True)
                                                End Select
                                            Else
                                                Dim aSingle As Single
                                                If Single.TryParse(cell.DataVerbalValue, aSingle) Then
                                                    Select Case CovObj.MeasureType
                                                        Case ECMeasureType.mtDirect
                                                            nonpwData = New clsDirectMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, aSingle)
                                                        Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                                                            nonpwData = New clsUtilityCurveMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, aSingle, CType(CovObj.MeasurementScale, clsCustomUtilityCurve))
                                                        Case ECMeasureType.mtStep
                                                            nonpwData = New clsStepMeasureData(Alt.NodeID, CovObj.NodeID, user.UserID, aSingle, CType(CovObj.MeasurementScale, clsStepFunction))
                                                    End Select
                                                End If
                                            End If
                                    End Select
                                    If nonpwData IsNot Nothing Then
                                        nonpwData.ModifyDate = Now
                                        CType(CovObj.Judgments, clsNonPairwiseJudgments).AddMeasureData(nonpwData)
                                    End If
                                End If
                            Next

                            'Debug.Print(" === DataGrid Cells end ===")

                            .ProjectManager.StorageManager.Writer.SaveUserJudgments(user.UserID)
                        End If
                    End If

                End With

                File_Erase(sUploadedFileName)
                App.ActiveProject.ResetProject()
                divUpload.Visible = False
                lblError.Text = String.Format("<h6 style='margin-top:1em'>{0}</h6>", ResString("lblPleaseWait"))
                lblError.Visible = True
                ScriptManager.RegisterStartupScript(Me, GetType(String), "Init", "onUploadFile();", True)
            Else
                Dim ErrMsg As String = dgError.GetType().ToString + " " + dgError.Message
                App.DBSaveLog(dbActionType.actDatagridUpload, dbObjectType.einfProject, App.ProjectID, "Datgrid upload error", ErrMsg)

                Select Case True
                    Case TypeOf dgError Is DataGridExceptionDuplicateGUID
                        lblError.Text = String.Format("<div class='text error'><b>{0}</b></div>", String.Format(ResString("lblDataGridUploadErrorDuplicateID"), dgError.Message.Trim))
                    Case TypeOf dgError Is DataGridExceptionInvalidRisk
                        lblError.Text = String.Format("<div class='text error'><b>{0}</b></div>", String.Format(ResString("lblDataGridUploadErrorInvalidRiskScore"), dgError.Message.Trim))
                    Case TypeOf dgError Is DataGridExceptionInvalidDirect
                        lblError.Text = String.Format("<div class='text error'><b>{0}</b></div>", String.Format(ResString("lblDataGridUploadErrorInvalidDirectScore"), dgError.Message.Trim))
                    Case TypeOf dgError Is DataGridExceptionInvalidRating   ' D7515
                        lblError.Text = String.Format("<div class='text error'><b>{0}</b></div>", String.Format(ResString("lblDataGridUploadErrorInvalidRatingValue"), dgError.Message.Trim, PageURL(_PGID_STRUCTURE_MEASUREMENT_METHODS)))   ' D7517
                    Case Else
                        lblError.Text = String.Format("<div class='text error'><b>{0}</b></div>", ResString("lblDataGridUploadError"))
                End Select
                ScriptManager.RegisterStartupScript(Me, GetType(String), "Init", "CheckUpload();", True)

                lblError.Visible = True
            End If
        End If
    End Sub

    Private Sub SetRiskValue(altID As Guid, value As Double)
        Dim RA = App.ActiveProject.ProjectManager.ResourceAligner
        For Each alt In RA.Scenarios.Scenarios(0).AlternativesFull
            If alt.ID = altID.ToString Then
                alt.RiskOriginal = value
                'alt.RiskOriginal = value
                Exit Sub
            End If
        Next
    End Sub

    Private Sub SetCostValue(altID As Guid, value As Double)
        Dim RA = App.ActiveProject.ProjectManager.ResourceAligner
        For Each alt In RA.Scenarios.Scenarios(0).AlternativesFull
            If alt.ID = altID.ToString Then
                alt.Cost = value
                Exit Sub
            End If
        Next
    End Sub

End Class