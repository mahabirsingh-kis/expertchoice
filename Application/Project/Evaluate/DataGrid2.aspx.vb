Partial Class DataGrid2Page
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_REPORT_DATAGRID2)
    End Sub
    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public SelectedUserID As Integer = -1
    Public Function GetDataGridColumns() As String
        Dim retVal As String = ""
        Dim altCount As Integer = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count
        Dim ItemFormat As String = "{{dataField:'{0}', caption:'{1}', width: 90}}" ', hidingPriority: {2}
        retVal = String.Format("{{dataField:'{0}', caption:'{1}', minWidth:'200px', fixed: true, fixedPosition: 'left'}}", "title", "Node") ', hidingPriority: {2} , altCount
        retVal += "," + String.Format("{{dataField:'{0}', caption:'{1}', width: 80}}", JS_SafeString("col_m_type"), JS_SafeString("Measurement Type")) ', hidingPriority: {2}, altCount
        retVal += "," + String.Format("{{dataField:'{0}', caption:'{1}', width: 80}}", JS_SafeString("col_lprty"), JS_SafeString("Local Priority")) ', hidingPriority: {2}, altCount
        retVal += "," + String.Format("{{dataField:'{0}', caption:'{1}', width: 80}}", JS_SafeString("col_gprty"), JS_SafeString("Global Priority")) ', hidingPriority: {2}, altCount
        Dim i As Integer = 0
        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
            retVal += "," + String.Format(ItemFormat, JS_SafeString("alt" + alt.NodeID.ToString), JS_SafeString(alt.NodeName)) ', altCount - i
            i += 1
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetDataGridSource() As String
        Dim isDataGrid As Boolean = True
        Dim UserID As Integer = SelectedUserID
        Dim Attr_RootID As Integer = 1000
        Dim retVal As String = ""
        'Dim valDecimals As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID))
        Dim sNumberFormat As String = "F" + PM.Parameters.DecimalDigits.ToString    ' D6850
        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(UserID))

        Dim attributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes()
        Dim visibleAttributes As New List(Of clsAttribute)
        Dim i As Integer = 0
        For Each attr As clsAttribute In attributes
            If isVisibleAttr(attr, isDataGrid, PM.IsRiskProject, PM.UseDataMapping) Then
                visibleAttributes.Add(attr)
            End If
        Next

        Dim UserRoles As clsUserRoles = PM.UsersRoles.GetUserRolesByID(UserID)
        Dim RolesStat As List(Of RolesStatistics) = PM.GetRolesStatistics()
        If isDataGrid Then
            Dim CalcTarget As clsCalculationTarget
            If IsCombinedUserID(UserID) Then
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(UserID))
            Else
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, PM.GetUserByID(UserID))
            End If
            PM.CalculationsManager.Calculate(CalcTarget, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
        End If
        Dim RowFormat As String = "{{id:'{0}', idx:{1}, title:'{2}', nodeid:{3}, isTerminal:{4}, pid:{5}{6}}}"
        i = 0
        For Each obj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).Nodes
            Dim colVals As String = ", col_m_type:'" + JS_SafeString(obj.MeasureType.ToString) + "'"
            colVals += ", col_lprty:'" + obj.LocalPriority(UserID).ToString(sNumberFormat) + "', col_gprty:'" + obj.SAGlobalPriority().ToString(sNumberFormat) + "'"
            If obj.IsTerminalNode Or True Then
                For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                    Dim altPrefix As String = ",alt" + alt.NodeID.ToString
                    Select Case obj.MeasureType
                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
                            colVals += altPrefix + ":'" + obj.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID).ToString(sNumberFormat) + "'"
                        Case ECMeasureType.mtRatings
                            Dim rData As clsRatingMeasureData = CType(CType(obj.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, obj.NodeID, UserID), clsRatingMeasureData)
                            If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                                If rData.Rating.ID <> -1 Then
                                    colVals += altPrefix + ":'" + rData.Rating.Name + "'"
                                Else
                                    colVals += altPrefix + ":'" + rData.Rating.Value.ToString(sNumberFormat) + "'"
                                End If
                            Else
                                colVals += altPrefix + ":' '"
                            End If
                        Case Else
                            Dim nonpwData As clsNonPairwiseMeasureData = CType(obj.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, obj.NodeID, UserID)
                            If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                colVals += altPrefix + ":'" + nonpwData.ObjectValue.ToString + "'"
                            Else
                                colVals += altPrefix + ":' '"
                            End If
                    End Select
                Next
            End If
            retVal += If(retVal <> "", ",", "") + String.Format(RowFormat, JS_SafeString(obj.NodeGuidID.ToString), i, JS_SafeString(obj.NodeName), obj.NodeID + 1, IIf(obj.IsTerminalNode, 1, 0), obj.ParentNodeID + 1, colVals)
            i += 1
        Next

        If visibleAttributes.Count > 0 Then
            retVal += If(retVal <> "", ",", "") + String.Format(RowFormat, "attrs_root", Attr_RootID, "Attributes", Attr_RootID, 0, 0, "")
        End If

        For Each attr As clsAttribute In visibleAttributes
            Dim colVals As String = ""
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                Dim altPrefix As String = ",alt" + alt.NodeID.ToString
                Dim sValue As String = ""
                If attr.ValueType = AttributeValueTypes.avtDouble Or attr.ValueType = AttributeValueTypes.avtLong Then
                    Try 'AS/14404 incorporated Try-Catch
                        Dim aVal As Double = CDbl(PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID))
                        If aVal = Int32.MinValue Then
                            sValue = " "
                            colVals += altPrefix + String.Format(":'{0}'", JS_SafeString(sValue))
                        Else
                            sValue = JS_SafeNumber(aVal)
                            colVals += altPrefix + String.Format(":'{0}'", JS_SafeString(sValue))
                        End If

                    Catch
                        sValue = " "
                        colVals += altPrefix + String.Format(":'{0}'", JS_SafeString(sValue))
                    End Try
                Else
                    sValue = PM.Attributes.GetAttributeValueString(attr.ID, alt.NodeGuidID)
                    colVals += altPrefix + String.Format(":'{0}'", JS_SafeString(sValue))
                End If
            Next
            retVal += If(retVal <> "", ",", "") + String.Format(RowFormat, JS_SafeString(attr.ID.ToString), i, JS_SafeString(attr.Name), visibleAttributes.IndexOf(attr) + Attr_RootID + 1, 1, Attr_RootID, colVals)
            i += 1
        Next
        Return String.Format("[{0}]", retVal)

        'For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes  ' D3925
        '    Dim attrValues As String = ""
        '    Dim attributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes()
        '    For Each attr As clsAttribute In attributes
        '        If isVisibleAttr(attr, isDataGrid, PM.IsRiskProject, PM.UseDataMapping) Then
        '            Dim sValue As String = ""
        '            If attr.ValueType = AttributeValueTypes.avtDouble Or attr.ValueType = AttributeValueTypes.avtLong Then 'AS/14488a added avtLong
        '                Try 'AS/14404 incorporated Try-Catch
        '                    Dim aVal As Double = CDbl(PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID))
        '                    If aVal = Int32.MinValue Then
        '                        sValue = " "
        '                        attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(sValue))
        '                    Else
        '                        sValue = JS_SafeNumber(aVal)
        '                        attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{0}", JS_SafeString(sValue))
        '                    End If

        '                Catch
        '                    sValue = " "
        '                    attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(sValue))
        '                End Try
        '            Else
        '                sValue = PM.Attributes.GetAttributeValueString(attr.ID, alt.NodeGuidID)
        '                attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(sValue))
        '            End If
        '        End If
        '    Next
        '    Dim contributions As String = ""
        '    For Each obj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
        '        contributions += CStr(IIf(contributions <> "", ",", "")) + CStr(IIf(obj.GetContributedAlternatives.Contains(alt), 1, 0))
        '    Next
        '    'roles:
        '    '-1 - no contributions
        '    '0 - restricted
        '    '1 - allowed 
        '    '2 - undefined
        '    '3 - mixed
        '    Dim roles As String = ""
        '    Dim arole As Integer = 1
        '    For Each obj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
        '        'Dim altUserRole As clsAlternativesRoles = UserRoles.AlternativesRoles(obj.NodeGuidID)
        '        'if altUserRole.AllowedAlternativesList.Contains(alt.NodeGuidID) Then
        '        '    arole = 1
        '        'End If
        '        'if altUserRole.RestrictedAlternativesList.Contains(alt.NodeGuidID) Then
        '        '    arole = 0
        '        'End If
        '        'if altUserRole.UndefinedAlternativesList.Contains(alt.NodeGuidID) Then
        '        '    arole = 2
        '        'End If
        '        arole = CInt(Math.Ceiling(Rnd() * 4)) - 1
        '        roles += CStr(IIf(roles <> "", ",", "")) + CStr(IIf(obj.GetContributedAlternatives.Contains(alt), arole, -1))
        '    Next
        '    Dim grouproles As String = ""
        '    Dim stat As String = ""
        '    For Each obj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
        '        arole = CInt(Math.Ceiling(Rnd() * 4)) - 1
        '        grouproles += CStr(IIf(grouproles <> "", ",", "")) + CStr(IIf(obj.GetContributedAlternatives.Contains(alt), arole, -1))
        '        stat += CStr(IIf(stat <> "", ",", "")) + "'" + GetRoleStatStringByGUIDs(RolesStat, alt.NodeGuidID, obj.NodeGuidID) + "'"
        '    Next
        'Dim datagrid As String = ""
        'If isDataGrid Then
        '    For Each node As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
        '        Select Case node.MeasureType
        '            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
        '                DataGrid += CStr(IIf(DataGrid <> "", ",", "")) + JS_SafeNumber(node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID))
        '            Case ECMeasureType.mtRatings
        '                Dim rData As clsRatingMeasureData = CType(CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserID), clsRatingMeasureData)
        '                If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
        '                    If rData.Rating.ID <> -1 Then
        '                        DataGrid += CStr(IIf(DataGrid <> "", ",", "")) + "'" + rData.Rating.Name + "'"
        '                    Else
        '                        DataGrid += CStr(IIf(DataGrid <> "", ",", "")) + JS_SafeNumber(rData.Rating.Value)
        '                    End If
        '                Else
        '                    DataGrid += CStr(IIf(DataGrid <> "", ",", "")) + " "
        '                End If
        '            Case Else
        '                Dim nonpwData As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserID)
        '                If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
        '                    DataGrid += CStr(IIf(DataGrid <> "", ",", "")) + JS_SafeNumber(nonpwData.ObjectValue)
        '                Else
        '                    DataGrid += CStr(IIf(DataGrid <> "", ",", "")) + " "
        '                End If
        '        End Select
        '    Next
        '    attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{0}", JS_SafeNumber(alt.UnnormalizedPriority))
        'End If
        '    retVal += If(retVal <> "", ",", "") + String.Format("{{id:'{0}', idx:{1}, title:'{2}', nodeid:{3}, attrvals:[{4}], show: 1, cont:[{5}], roles:[{6}], groles:[{7}], stat:[{9}], dg:[{8}]}}", JS_SafeString(alt.NodeGuidID.ToString), i, JS_SafeString(alt.NodeName), alt.NodeID, attrValues, contributions, roles, grouproles, datagrid, stat)
        '    i += 1
        'Next
        'Return String.Format("[{0}]", retVal)
    End Function


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
                        End If
                    End If
                    If Not DMGuid.Equals(Guid.Empty) Then
                        Dim DM As clsDataMapping = PM.DataMappings.Find(Function(d) d.DataMappingGUID.Equals(DMGuid))
                        If DM IsNot Nothing Then PM.DataMappings.Remove(DM)
                    End If
                    If sName <> "" Then App.ActiveProject.SaveStructure("Delete Data Mapping", , , sName)
                End If
                tResult = Bool2Num(sName <> "").ToString
                ' D4491 ==
            Case "userchanged"
                SelectedUserID = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")))
                tResult = String.Format("['{0}',{1}]", "rows", GetDataGridSource())
        End Select

        If tResult <> "" Then
            If tResult.StartsWith("['ok',") Then App.ActiveProject.SaveStructure(sSnapshotMessage, True, True, sSnapshotComment) 'A1202 'If tResult = "'ok'" Then PM.StorageManager.Writer.SaveProject(True,)
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Sub DataGrid2Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If isCallback Then Ajax_Callback(Request.Form.ToString)
    End Sub
End Class