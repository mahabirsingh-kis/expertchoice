Imports Canvas
Imports ECCore
Imports ExpertChoice.Service

Namespace ExpertChoice.Web

    Public Module DataGridModule

        Public Function isVisibleAttr(attr As clsAttribute, Optional isDataGrid As Boolean = False, Optional isRisk As Boolean = False, Optional isDataMappingEnabled As Boolean = True) As Boolean
            Return Not attr.IsDefault Or (Not isRisk And (attr.ID = ECCore.ATTRIBUTE_RISK_ID Or attr.ID = ECCore.ATTRIBUTE_COST_ID)) Or ((attr.ID = ECCore.ATTRIBUTE_MAPKEY_ID Or attr.ID = ECCore.ATTRIBUTE_START_ID Or attr.ID = ECCore.ATTRIBUTE_FINISH_ID) And isDataGrid And isDataMappingEnabled)
        End Function

        Public Function GetRoleStatStringByGUIDs(ByRef RolesStat As List(Of RolesStatistics), ByVal AltID As Guid, ByVal ObjID As Guid) As String
            For Each RS As RolesStatistics In RolesStat
                If RS.AlternativeID.Equals(AltID) And RS.ObjectiveID.Equals(ObjID) Then
                    'Return String.Format("{1}/{0}", RS.AllowedCount, RS.EvaluatedCount)
                    Return String.Format("{0}", RS.AllowedCount)
                End If
            Next
            Return " "
        End Function

        'Public Function IsRowWarning(ByRef RolesStat As List(Of RolesStatistics), ByVal AltID As Guid) As Boolean
        '    For Each RS As RolesStatistics In RolesStat
        '        If RS.AlternativeID.Equals(AltID) Then
        '            If RS.AllowedCount = 0 Then Return True
        '        End If
        '    Next
        '    Return False
        'End Function

        Public Function GetRoleStatStringByGUIDsObj(ByRef RolesStat As List(Of RolesStatistics), ByVal ObjID As Guid) As String
            For Each RS As RolesStatistics In RolesStat
                If RS.ObjectiveID.Equals(ObjID) Then
                    'Return String.Format("{1}/{0}", RS.AllowedCount, RS.EvaluatedCount)
                    Return String.Format("{0}", RS.AllowedCount)
                End If
            Next
            Return " "
        End Function
        Public Function datagrid_GetRows(PM As clsProjectManager, UserIDs As List(Of Integer), Optional isDataGrid As Boolean = False, Optional needStats As Boolean = False, Optional showNoSources As Boolean = False) As String
            Dim retVal As String = ""
            Dim i As Integer = 0
            'Dim valDecimals As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID))
            Dim sNumberFormat As String = "F" + (PM.Parameters.DecimalDigits + 2).ToString    'D6849
            Dim RolesStat As List(Of RolesStatistics) = If(needStats, PM.GetRolesStatistics(), New List(Of RolesStatistics))
            'Dim showNoSources As Boolean = PM.IsRiskProject And PM.ActiveObjectives.HierarchyID = ECHierarchyID.hidLikelihood
            Dim uncontributedEvents As List(Of clsNode) = If(showNoSources, PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives(), New List(Of clsNode))

            If isDataGrid Then
                PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(UserIDs(0)))
                Dim CalcTarget As clsCalculationTarget
                If IsCombinedUserID(UserIDs(0)) Then
                    CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(UserIDs(0)))
                Else
                    CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, PM.GetUserByID(UserIDs(0)))
                End If
                PM.CalculationsManager.Calculate(CalcTarget, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
            End If

            Dim SelectedUsersRoles As Dictionary(Of Integer, Dictionary(Of Integer, RolesData)) = Nothing
            Dim SelectedGroupsRoles As Dictionary(Of Integer, Dictionary(Of Integer, RoleType)) = Nothing
            If UserIDs.Count > 0 AndAlso UserIDs(0) >= 0 Then
                SelectedUsersRoles = PM.UsersRoles.GetParticipantsRolesView(UserIDs)
            Else
                SelectedGroupsRoles = PM.UsersRoles.GetGroupsRolesView(UserIDs)
            End If

            Dim nodesInLinearOrder As List(Of Tuple(Of Integer, Integer, clsNode)) = PM.ActiveObjectives.NodesInLinearOrder

            For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes  ' D3925
                Dim attrValues As String = ""
                Dim attributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes()
                For Each attr As clsAttribute In attributes
                    If isVisibleAttr(attr, isDataGrid, PM.IsRiskProject, PM.UseDataMapping) Then
                        Dim sValue As String = ""
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
                                    attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("""{0}""", JS_SafeString(sValue))
                                Else
                                    sValue = aVal.ToString(sNumberFormat)
                                    attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{0}", JS_SafeString(sValue))
                                End If

                            Catch
                                sValue = " "
                                attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(sValue))
                            End Try
                        Else
                            sValue = PM.Attributes.GetAttributeValueString(attr.ID, alt.NodeGuidID)
                            attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(sValue))
                        End If
                    End If
                Next
                Dim contributions As String = ""
                If showNoSources Then
                    contributions += CStr(IIf(contributions <> "", ",", "")) + CStr(IIf(uncontributedEvents.Contains(alt), 1, 0))
                End If

                For Each t As Tuple(Of Integer, Integer, clsNode) In nodesInLinearOrder
                    Dim obj As clsNode = t.Item3
                    If obj.IsTerminalNode Then
                        contributions += CStr(IIf(contributions <> "", ",", "")) + CStr(IIf(obj.GetContributedAlternatives.Contains(alt), 1, 0))
                    End If
                Next
                'roles:
                '-1 - no contributions
                '0 - restricted
                '1 - allowed 
                '2 - undefined
                '3 - mixed
                Dim roles As String = ""
                Dim arole As Integer = 1
                If showNoSources Then
                    arole = -1
                    If SelectedUsersRoles IsNot Nothing AndAlso SelectedUsersRoles.ContainsKey(-1) Then
                        If SelectedUsersRoles(-1).ContainsKey(alt.NodeID) Then
                            Dim cellRoles As RolesData = SelectedUsersRoles(-1)(alt.NodeID)
                            arole = CoreToJSRole(cellRoles.IndividualRole)
                        End If
                    End If
                    If SelectedGroupsRoles IsNot Nothing AndAlso SelectedGroupsRoles.ContainsKey(-1) Then
                        If SelectedGroupsRoles(-1).ContainsKey(alt.NodeID) Then
                            Dim cellRoles As RoleType = SelectedGroupsRoles(-1)(alt.NodeID)
                            arole = CoreToJSRole(cellRoles)
                        End If
                    End If
                    roles += CStr(IIf(roles <> "", ",", "")) + CStr(IIf(uncontributedEvents.Contains(alt), arole, -1))
                End If

                For Each t As Tuple(Of Integer, Integer, clsNode) In nodesInLinearOrder
                    Dim obj As clsNode = t.Item3
                    If obj.IsTerminalNode Then
                        arole = -1
                        If SelectedUsersRoles IsNot Nothing AndAlso SelectedUsersRoles.ContainsKey(obj.NodeID) Then
                            If SelectedUsersRoles(obj.NodeID).ContainsKey(alt.NodeID) Then
                                Dim cellRoles As RolesData = SelectedUsersRoles(obj.NodeID)(alt.NodeID)
                                arole = CoreToJSRole(cellRoles.IndividualRole)
                            End If
                        End If
                        If SelectedGroupsRoles IsNot Nothing AndAlso SelectedGroupsRoles.ContainsKey(obj.NodeID) Then
                            If SelectedGroupsRoles(obj.NodeID).ContainsKey(alt.NodeID) Then
                                Dim cellRoles As RoleType = SelectedGroupsRoles(obj.NodeID)(alt.NodeID)
                                arole = CoreToJSRole(cellRoles)
                            End If
                        End If
                        roles += CStr(IIf(roles <> "", ",", "")) + CStr(IIf(obj.GetContributedAlternatives.Contains(alt), arole, -1))
                    End If
                Next
                Dim grouproles As String = ""
                Dim finalroles As String = ""
                Dim stat As String = ""
                Dim frole As Integer = -1
                Dim grole As Integer = 2
                If showNoSources Then
                    grole = -1
                    frole = -1
                    If SelectedUsersRoles IsNot Nothing Then
                        If SelectedUsersRoles.ContainsKey(-1) Then
                            If SelectedUsersRoles(-1).ContainsKey(alt.NodeID) Then
                                Dim cellRoles As RolesData = SelectedUsersRoles(-1)(alt.NodeID)
                                grole = CoreToJSRole(cellRoles.GroupRole)
                                frole = CoreToJSRole(cellRoles.FinalRole)
                            End If
                        End If
                    End If
                    If SelectedGroupsRoles IsNot Nothing Then
                        If SelectedGroupsRoles.ContainsKey(-1) Then
                            If SelectedGroupsRoles(-1).ContainsKey(alt.NodeID) Then
                                Dim cellRoles As RoleType = SelectedGroupsRoles(-1)(alt.NodeID)
                                grole = CoreToJSRole(cellRoles)
                                frole = CoreToJSRole(cellRoles)
                            End If
                        End If
                    End If

                    grouproles += CStr(IIf(grouproles <> "", ",", "")) + CStr(IIf(uncontributedEvents.Contains(alt), grole, -1))
                    finalroles += CStr(IIf(finalroles <> "", ",", "")) + CStr(IIf(uncontributedEvents.Contains(alt), frole, -1))
                    'stat += CStr(IIf(stat <> "", ",", "")) + "'" + GetRoleStatStringByGUIDs(RolesStat, alt.NodeGuidID, "nosources") + "'"
                    stat += CStr(IIf(stat <> "", ",", "")) + "'N/A'"
                End If

                For Each t As Tuple(Of Integer, Integer, clsNode) In nodesInLinearOrder
                    Dim obj As clsNode = t.Item3
                    If obj.IsTerminalNode Then
                        grole = -1
                        frole = -1
                        If SelectedUsersRoles IsNot Nothing Then
                            If SelectedUsersRoles.ContainsKey(obj.NodeID) Then
                                If SelectedUsersRoles(obj.NodeID).ContainsKey(alt.NodeID) Then
                                    Dim cellRoles As RolesData = SelectedUsersRoles(obj.NodeID)(alt.NodeID)
                                    grole = CoreToJSRole(cellRoles.GroupRole)
                                    frole = CoreToJSRole(cellRoles.FinalRole)
                                End If
                            End If
                        End If
                        If SelectedGroupsRoles IsNot Nothing Then
                            If SelectedGroupsRoles.ContainsKey(obj.NodeID) Then
                                If SelectedGroupsRoles(obj.NodeID).ContainsKey(alt.NodeID) Then
                                    Dim cellRoles As RoleType = SelectedGroupsRoles(obj.NodeID)(alt.NodeID)
                                    grole = CoreToJSRole(cellRoles)
                                    frole = CoreToJSRole(cellRoles)
                                End If
                            End If
                        End If

                        grouproles += CStr(IIf(grouproles <> "", ",", "")) + CStr(IIf(obj.GetContributedAlternatives.Contains(alt), grole, -1))
                        finalroles += CStr(IIf(finalroles <> "", ",", "")) + CStr(IIf(obj.GetContributedAlternatives.Contains(alt), frole, -1))
                        stat += CStr(IIf(stat <> "", ",", "")) + "'" + GetRoleStatStringByGUIDs(RolesStat, alt.NodeGuidID, obj.NodeGuidID) + "'"
                    End If
                Next
                Dim datagrid As String = ""
                If isDataGrid Then
                    Dim u As clsUser = PM.GetUserByID(UserIDs(0))
                    Dim NormMode As LocalNormalizationType = PM.Parameters.Normalization

                    ' if event with no source
                    If uncontributedEvents.Contains(alt) Then
                        Dim strVal As String = ""
                        Select Case alt.MeasureType
                            Case ECMeasureType.mtRatings
                                Dim rData As clsNonPairwiseMeasureData = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeID, UserIDs(0))
                                If rData IsNot Nothing AndAlso rData.ObjectValue IsNot Nothing Then 'A1706
                                    strVal = If(CType(rData.ObjectValue, clsRating).ID <> -1, JS_SafeString(CType(rData.ObjectValue, clsRating).Name), JS_SafeNumber(CType(rData.ObjectValue, clsRating).Value))
                                End If
                            Case Else
                                Dim nonpwData As clsNonPairwiseMeasureData = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeID, UserIDs(0))
                                If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                    strVal = CSng(nonpwData.ObjectValue).ToString(sNumberFormat)
                                End If
                        End Select
                        datagrid += CStr(IIf(datagrid <> "", ",", "")) + "'" + strVal + "'"
                    End If

                    For Each tp As Tuple(Of Integer, Integer, clsNode) In PM.ActiveObjectives.NodesInLinearOrder()
                        Dim node As clsNode = tp.Item3
                        If node.IsTerminalNode Then
                            If PM.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, UserIDs(0)) Or True Then
                                Dim UserWeights As clsUserWeights = node.Judgments.Weights.GetUserWeights(UserIDs(0), PM.PipeParameters.SynthesisMode, PM.CalculationsManager.IncludeIdealAlternative)
                                Dim PrtValue As Double = If(NormMode = LocalNormalizationType.ntUnnormalized, UserWeights.GetUnnormalizedWeightValueByNodeID(alt.NodeID), UserWeights.GetNormalizedWeightValueByNodeID(alt.NodeID))
                                Select Case node.MeasureType
                                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
                                        datagrid += CStr(IIf(datagrid <> "", ",", "")) + If(Double.IsNaN(PrtValue), "", PrtValue.ToString(sNumberFormat))
                                    Case ECMeasureType.mtRatings
                                        Dim rData As clsRatingMeasureData = CType(CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserIDs(0)), clsRatingMeasureData)
                                        If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                                            If rData.Rating.ID <> -1 Then
                                                datagrid += CStr(IIf(datagrid <> "", ",", "")) + "'" + rData.Rating.Name + "'"
                                            Else
                                                Dim rVal As Double = If(NormMode = LocalNormalizationType.ntUnnormalized Or Not IsCombinedUserID(UserIDs(0)), rData.Rating.Value, PrtValue)
                                                datagrid += CStr(IIf(datagrid <> "", ",", "")) + If(Double.IsNaN(rVal), "", rVal.ToString(sNumberFormat))
                                            End If
                                        Else
                                            datagrid += CStr(IIf(datagrid <> "", ",", "")) + " "
                                        End If
                                    Case Else
                                        Dim nonpwData As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserIDs(0))
                                        If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                            Dim rVal As Double = If(NormMode = LocalNormalizationType.ntUnnormalized Or Not IsCombinedUserID(UserIDs(0)), CType(nonpwData.ObjectValue, Double), PrtValue)
                                            datagrid += CStr(IIf(datagrid <> "", ",", "")) + If(Double.IsNaN(rVal), "", rVal.ToString(sNumberFormat))
                                        Else
                                            datagrid += CStr(IIf(datagrid <> "", ",", "")) + " "
                                        End If
                                End Select
                            Else
                                datagrid += CStr(IIf(datagrid <> "", ",", "")) + " "
                            End If
                        End If
                    Next
                    attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{0}", JS_SafeNumber(If(NormMode = LocalNormalizationType.ntUnnormalized, alt.UnnormalizedPriority, alt.WRTGlobalPriority)))
                End If
                If PM.IsRiskProject Then
                    attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(alt.EventType.ToString))
                End If
                retVal += If(retVal <> "", ",", "") + String.Format("{{""id"":""{0}"", ""idx"":{1}, ""title"":""{2}"", ""nodeid"":{3}, ""attrvals"":[{4}], ""show"": 1, ""cont"":[{5}], ""roles"":[{6}], ""groles"":[{7}], ""froles"":[{10}], ""stat"":[{9}], ""dg"":[{8}]}}", JS_SafeString(alt.NodeGuidID.ToString), i, JS_SafeString(alt.Index + " " + alt.NodeName), alt.NodeID, attrValues, contributions, roles, grouproles, datagrid, stat, finalroles)
                i += 1
            Next
            Return String.Format("[{0}]", retVal)
        End Function

        Public Function CoreToJSRole(CoreRole As RoleType) As Integer
            Dim retVal As Integer = -1
            Select Case CoreRole
                Case RoleType.Allowed
                    retVal = 1
                Case RoleType.Mixed
                    retVal = 3
                Case RoleType.Restricted
                    retVal = 0
                Case RoleType.Undefined
                    retVal = 2
            End Select
            Return retVal
        End Function

        Public Function datagrid_GetColumns(PM As clsProjectManager, UserIDs As List(Of Integer), Optional hideNoSources As Boolean = False, Optional needStats As Boolean = False) As String
            Dim retVal As String = ""
            Dim i As Integer = 0
            Dim arole As Integer = 2
            Dim grole As Integer = 2
            Dim frole As Integer = 2
            Dim SelectedUsersRoles As Dictionary(Of Integer, RolesData) = Nothing
            Dim SelectedGroupsRoles As Dictionary(Of Integer, RoleType) = Nothing
            If UserIDs.Count > 0 AndAlso UserIDs(0) >= 0 Then
                SelectedUsersRoles = PM.UsersRoles.GetParticipantsRolesViewForObjectives(UserIDs)
            Else
                SelectedGroupsRoles = PM.UsersRoles.GetGroupsRolesViewForObjectives(UserIDs)
            End If
            Dim uncontributedEvents As List(Of clsNode) = If(PM.IsRiskProject, PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives(), New List(Of clsNode))
            'If uncontributedEvents.Count > 0 Then
            If PM.IsRiskProject And Not hideNoSources Then
                retVal += If(retVal <> "", ",", "") + String.Format("{{""id"":""{0}"", ""idx"":{1}, ""title"":""{2}"", ""nodeid"":{3}, ""isTerminal"":{4}, ""parentnodeid"":{5}, ""isAttribute"":{6}, ""level"":{7}, ""terminalnodescount"":{8}, ""checkstate"":0, ""role"":{10}, ""grole"":{11}, ""frole"":{13}, ""stat"":""{12}"", ""dm"":""{9}"", ""mt"":""{14}"" }}", "nosources", -1, "NO SOURCES", -1, 1, -1, 0, 0, 0, "nosources", arole, grole, "", frole, "NS")
            End If
            Dim RolesStat As List(Of RolesStatistics) = If(needStats, PM.GetRolesStatistics(RolesToSendType.rstObjectivesOnly), New List(Of RolesStatistics))
            For Each t As Tuple(Of Integer, Integer, clsNode) In PM.ActiveObjectives.NodesInLinearOrder
                Dim obj As clsNode = t.Item3
                Dim rtarole As RoleType = RoleType.Undefined
                Dim rtgrole As RoleType = RoleType.Undefined
                Dim rtfrole As RoleType = RoleType.Undefined
                If SelectedUsersRoles IsNot Nothing AndAlso SelectedUsersRoles.ContainsKey(obj.NodeID) Then
                    rtarole = SelectedUsersRoles(obj.NodeID).IndividualRole
                    rtgrole = SelectedUsersRoles(obj.NodeID).GroupRole
                    rtfrole = SelectedUsersRoles(obj.NodeID).FinalRole
                End If

                If SelectedGroupsRoles IsNot Nothing AndAlso SelectedGroupsRoles.ContainsKey(obj.NodeID) Then
                    rtgrole = SelectedGroupsRoles(obj.NodeID)
                    rtfrole = rtgrole
                    rtarole = rtgrole
                End If

                Dim stat As String = ""
                stat = GetRoleStatStringByGUIDsObj(RolesStat, obj.NodeGuidID)
                arole = CoreToJSRole(rtarole)
                grole = CoreToJSRole(rtgrole)
                frole = CoreToJSRole(rtfrole)
                Dim mt As ECMeasureType = obj.MeasureType
                Dim strMT As String = ""
                Select Case mt
                    Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtCustomUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                        strMT = "UC"
                    Case ECMeasureType.mtDirect
                        strMT = "D"
                    Case ECMeasureType.mtPairwise
                        strMT = "PW"
                    Case ECMeasureType.mtRatings
                        strMT = "R"
                    Case ECMeasureType.mtStep
                        strMT = "S"
                    Case ECMeasureType.mtPWAnalogous
                        strMT = "PWA"
                    Case ECMeasureType.mtPWOutcomes
                        strMT = "PWO"
                    Case Else
                        strMT = ""
                End Select
                retVal += If(retVal <> "", ",", "") + String.Format("{{""id"":""{0}"", ""idx"":{1}, ""title"":""{2}"", ""nodeid"":{3}, ""isTerminal"":{4}, ""parentnodeid"":{5}, ""isAttribute"":{6}, ""level"":{7}, ""terminalnodescount"":{8}, ""checkstate"":0, ""role"":{10}, ""grole"":{11}, ""frole"":{13}, ""stat"":""{12}"", ""dm"":""{9}"", ""mt"":""{14}"" }}", JS_SafeString(obj.NodeGuidID.ToString), i, JS_SafeString(obj.NodeName), obj.NodeID, IIf(obj.IsTerminalNode, 1, 0), obj.ParentNodeID, 0, obj.Level, PM.Hierarchy(PM.ActiveHierarchy).GetRespectiveTerminalNodes(obj).Count, obj.DataMappingGUID.ToString, arole, grole, stat, frole, strMT)
                i += 1
            Next
            Return String.Format("[{0}]", retVal)
        End Function

        Public Function datagrid_GetHierarchyColumns(PM As clsProjectManager) As String
            Dim retVal As String = ""
            For Each t As Tuple(Of Integer, Integer, clsNode) In PM.Hierarchy(PM.ActiveHierarchy).NodesInLinearOrder()   ' D3925
                Dim obj As clsNode = t.Item3
                retVal += If(retVal <> "", ",", "") + String.Format("{{id:'{0}', title:'{1}', nodeid:{2}, isTerminal:{3}, parentNodeId:{4}}}", JS_SafeString(obj.NodeGuidID.ToString), JS_SafeString(obj.NodeName), obj.NodeID, IIf(obj.IsTerminalNode, 1, 0), obj.ParentNodeID)
            Next
            Return String.Format("[{0}]", retVal)
        End Function

        'A1201 ===
        ''' <summary>
        ''' String, containing visible attributes GUIDs delimited with ","
        ''' </summary>
        ''' <returns></returns>
        Public Property SelectedColumns(PRJ As clsProject) As String
            Get
                Dim s As String = CStr(PRJ.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_SELECTED_COLUMNS_ID, UNDEFINED_USER_ID))
                Return s
            End Get
            Set(value As String)
                WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_SELECTED_COLUMNS_ID, AttributeValueTypes.avtString, value)
            End Set
        End Property

        'A1201 ===
        ''' <summary>
        ''' String, containing hidden attributes GUIDs delimited with ","
        ''' </summary>
        ''' <returns></returns>
        Public Property HiddenColumns(PRJ As clsProject) As String
            Get
                Dim s As String = CStr(PRJ.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_DATAGRID_HIDDEN_COLUMNS_ID, UNDEFINED_USER_ID))
                Return s
            End Get
            Set(value As String)
                WriteSetting(PRJ, ATTRIBUTE_DATAGRID_HIDDEN_COLUMNS_ID, AttributeValueTypes.avtString, value)
            End Set
        End Property

        Public Function datagrid_GetAttributes(PM As clsProjectManager, App As clsComparionCore, Optional isDataGrid As Boolean = False) As String
            Dim retVal As String = ""
            For Each attr As clsAttribute In PM.Attributes.GetAlternativesAttributes()
                If isVisibleAttr(attr, isDataGrid, PM.IsRiskProject, PM.UseDataMapping) Then
                    Dim dm As String = JS_SafeString(attr.DataMappingGUID.ToString)
                    If attr.ID = ECCore.ATTRIBUTE_MAPKEY_ID Then
                        dm = "0"
                    End If
                    retVal += If(retVal <> "", ",", "") + String.Format("{{guid: '{0}', name: '{1}', type: '{2}', search: true, dm: '{3}'}}", JS_SafeString(attr.ID.ToString), JS_SafeString(App.GetAttributeName(attr)), JS_SafeString(attr.ValueType.ToString), dm)
                End If
            Next

            If isDataGrid Then
                retVal += If(retVal <> "", ",", "") + String.Format("{{guid: '{0}', name: '{1}', type: '{2}', search: true, dm: '0'}}", JS_SafeString("73044E75-FE7B-40E4-81A0-6141598719E2"), JS_SafeString("Total"), JS_SafeString(AttributeValueTypes.avtDouble.ToString)) 'A1480
            End If
            If PM.IsRiskProject Then
                retVal += If(retVal <> "", ",", "") + String.Format("{{guid: '{0}', name: '{1}', type: '{2}', search: true, dm: '0'}}", JS_SafeString("10998770-e88a-40e6-a514-25c870bad92b"), JS_SafeString("Event Type"), JS_SafeString(AttributeValueTypes.avtString.ToString))
            End If
            Return String.Format("[{0}]", retVal)
        End Function
        'A1201 ==

        Public Function datagrid_GetDataMappings(PM As clsProjectManager, App As clsComparionCore, Optional isDataGrid As Boolean = False) As String 'AS/25743a
            Dim retVal As String = ""
            For Each dm As clsDataMapping In PM.DataMappings
                Dim mapping As String = JS_SafeString(dm.DataMappingGUID.ToString)
                If mapping <> "" Then
                    retVal += If(retVal <> "", ",", "") + String.Format("{{guid: '{0}', db_name: '{1}', db_table: '{2}', db_field: '{3}'}}", JS_SafeString(dm.DataMappingGUID.ToString), JS_SafeString(dm.externalDBname), JS_SafeString(dm.externalTblName), JS_SafeString(dm.externalColName))
                End If
            Next

            Return String.Format("[{0}]", retVal)
        End Function
    End Module

End Namespace
