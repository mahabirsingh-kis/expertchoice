Partial Public Class DataGridWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Private Function GetDataGridSource(SelectedUserID As Integer) As String
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
    End Function

    Public Function getdatasource(SelectedUserID As Integer) As jActionResult
        Dim tRes As New jActionResult

        tRes.Result = ecActionResult.arSuccess
        'tRes.Message = RA.Solver.LastError
        tRes.Data = GetDataGridSource(SelectedUserID)
        tRes.ObjectID = SelectedUserID

        Return tRes
    End Function

    Private Sub DGWebAPIOnLoad(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject()

        Select Case _Page.Action
            Case "getdatasource"
                Dim ID As Integer
                If Not Integer.TryParse(GetParam(_Page.Params, "id"), ID) Then ID = -1
                _Page.ResponseData = getdatasource(ID)

                'Case "set_active_scenario"
                '    Dim ID As Integer
                '    If Not Integer.TryParse(GetParam(_Page.Params, "id"), ID) Then ID = ActiveScenarioID
                '    _Page.ResponseData = set_active_scenario(ID)

        End Select
    End Sub

End Class
