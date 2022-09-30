Option Strict On
Imports ECCore

Namespace ECCore

    'Public Class RolesStatistics
    '    Public Property ObjectiveID As Integer
    '    Public Property AlternativeID As Integer
    '    Public Property HasRolesCount As Integer
    '    Public Property EvaluatedCount As Integer
    '    Public Sub New(ObjectiveID As Integer, AlternativesID As Integer, HasRolesCount As Integer, EvaluatedCount As Integer)
    '        Me.ObjectiveID = ObjectiveID
    '        Me.AlternativeID = AlternativeID
    '        Me.HasRolesCount = HasRolesCount
    '        Me.EvaluatedCount = EvaluatedCount
    '    End Sub

    'End Class

    Public Enum RoleType
        Allowed = 0
        Restricted = 1
        Undefined = 2
        Mixed = 3
    End Enum

    Public Enum CopyRolesOptions
        Objectives = 0
        Alternatives = 1
        ObjectivesAndAlternatives = 2
    End Enum

    Public Class RolesData
        Public GroupRole As RoleType = RoleType.Undefined
        Public IndividualRole As RoleType = RoleType.Undefined
        Public FinalRole As RoleType = RoleType.Undefined
    End Class

    <Serializable()> Public Class clsObjectivesRoles
        Public Property Allowed() As New List(Of Guid)
        Public Property Restricted() As New List(Of Guid)
        Public Property Undefined() As New List(Of Guid)
    End Class


    <Serializable()> Public Class clsAlternativesRoles
        Public Property CoveringObjectiveID() As Guid
        Public Property AllowedAlternativesList() As New HashSet(Of Guid)
        Public Property RestrictedAlternativesList() As New HashSet(Of Guid)
        Public Property UndefinedAlternativesList() As New HashSet(Of Guid)
    End Class

    <Serializable()> Public Class clsUserRoles
        Public UpdateTime As DateTime
        Public Property ObjectivesRoles() As New clsObjectivesRoles
        Public Property AlternativesRoles() As New Dictionary(Of Guid, clsAlternativesRoles)
    End Class

    <Serializable()> Public Class clsProjectUsersRoles
        Private mUserRoles As Dictionary(Of Integer, clsUserRoles)
        Public Property IsAllowedByDefault() As Boolean = True
        Public ReadOnly Property ProjectManager() As clsProjectManager

        Public Function GetUserRolesByID(ByVal UserID As Integer) As clsUserRoles
            Return If(mUserRoles.ContainsKey(UserID), mUserRoles(UserID), Nothing)
        End Function

        Public Function IsAllowedObjective(ByVal ObjectiveID As Guid, ByVal UserID As Integer, Optional ByVal CheckPureRoles As Boolean = True) As Boolean
            Dim UR As clsUserRoles = GetUserRolesByID(UserID)

            ' check individual roles first
            If UR IsNot Nothing AndAlso CheckPureRoles Then
                If UR.ObjectivesRoles.Restricted.Contains(ObjectiveID) Then Return False
                If UR.ObjectivesRoles.Allowed.Contains(ObjectiveID) Then Return True
            End If

            ' need to check all groups
            Dim isRestrictedInSomeGroup As Boolean = False
            Dim isAllowedInSomeGroup As Boolean = False
            Dim isUndefinedInDefaultGroup As Boolean = False
            Dim i As Integer = 0
            While (i < ProjectManager.CombinedGroups.GroupsList.Count) And Not isRestrictedInSomeGroup
                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GroupsList(i), clsCombinedGroup)
                'CG.ApplyRules()
                If (CG.CombinedUserID = COMBINED_USER_ID) OrElse CG.ContainsUser(UserID) Then
                    Dim GR As clsUserRoles = GetUserRolesByID(CG.CombinedUserID)
                    If GR IsNot Nothing Then
                        If GR.ObjectivesRoles.Restricted.Contains(ObjectiveID) Then isRestrictedInSomeGroup = True
                        If Not isRestrictedInSomeGroup Then
                            If GR.ObjectivesRoles.Allowed.Contains(ObjectiveID) Then isAllowedInSomeGroup = True
                            If CG.CombinedUserID = COMBINED_USER_ID Then
                                If GR.ObjectivesRoles.Undefined.Contains(ObjectiveID) Then isUndefinedInDefaultGroup = True
                            End If
                        End If
                    End If
                End If
                i += 1
            End While

            If isRestrictedInSomeGroup Then
                Return False
            Else
                If isAllowedInSomeGroup Then
                    Return True
                Else
                    Return If(isUndefinedInDefaultGroup, False, IsAllowedByDefault)
                End If
            End If
        End Function

        Private Function GetAlternativeRolesByObjectiveID(ByVal UserRoles As clsUserRoles, ByVal ObjectiveID As Guid) As clsAlternativesRoles
            Return If(UserRoles.AlternativesRoles.ContainsKey(ObjectiveID), UserRoles.AlternativesRoles(ObjectiveID), Nothing)
        End Function

        Public Function GetRestrictedObjectives(ByVal UserID As Integer) As List(Of Guid)
            Dim UR As clsUserRoles = GetUserRolesByID(UserID)
            Return If(UR IsNot Nothing, UR.ObjectivesRoles.Restricted, New List(Of Guid))
        End Function

        Public Function GetAllowedObjectives(ByVal UserID As Integer) As List(Of Guid)
            Dim UR As clsUserRoles = GetUserRolesByID(UserID)
            Return If(UR IsNot Nothing, UR.ObjectivesRoles.Allowed, New List(Of Guid))
        End Function

        Public Function GetUndefinedObjectivesForDefaultGroup() As List(Of Guid)
            Dim UR As clsUserRoles = GetUserRolesByID(COMBINED_USER_ID)
            Return If(UR IsNot Nothing, UR.ObjectivesRoles.Undefined, New List(Of Guid))
        End Function

        Public Function GetRestrictedAlternatives(ByVal UserID As Integer, ByVal ObjectiveID As Guid) As HashSet(Of Guid)
            Dim node As clsNode = ProjectManager.ActiveObjectives.GetNodeByID(ObjectiveID)

            If node Is Nothing Then
                node = ProjectManager.ActiveAlternatives.GetNodeByID(ObjectiveID)
                If node Is Nothing Then Return New HashSet(Of Guid)
            End If

            Dim UR As clsUserRoles = GetUserRolesByID(UserID)
            If UR IsNot Nothing Then
                Dim AltsRoles As clsAlternativesRoles = GetAlternativeRolesByObjectiveID(UR, ObjectiveID)
                Return If(AltsRoles IsNot Nothing, AltsRoles.RestrictedAlternativesList, New HashSet(Of Guid))
            Else
                Return New HashSet(Of Guid)
            End If
        End Function

        Public Function GetAllowedAlternatives(ByVal UserID As Integer, ByVal ObjectiveID As Guid) As HashSet(Of Guid)
            Dim node As clsNode = ProjectManager.ActiveObjectives.GetNodeByID(ObjectiveID)

            If node Is Nothing Then
                node = ProjectManager.ActiveAlternatives.GetNodeByID(ObjectiveID)
                If node Is Nothing Then Return New HashSet(Of Guid)
            End If

            Dim UR As clsUserRoles = GetUserRolesByID(UserID)
            If UR IsNot Nothing Then
                Dim AltsRoles As clsAlternativesRoles = GetAlternativeRolesByObjectiveID(UR, ObjectiveID)
                Return If(AltsRoles IsNot Nothing, AltsRoles.AllowedAlternativesList, New HashSet(Of Guid))
            Else
                Return New HashSet(Of Guid)
            End If
        End Function

        Public Function GetUndefinedAlternativesForDefaultGroup(ByVal ObjectiveID As Guid) As HashSet(Of Guid)
            Dim node As clsNode = ProjectManager.ActiveObjectives.GetNodeByID(ObjectiveID)

            If node Is Nothing Then
                node = ProjectManager.ActiveAlternatives.GetNodeByID(ObjectiveID)
                If node Is Nothing Then Return New HashSet(Of Guid)
            End If

            Dim UR As clsUserRoles = GetUserRolesByID(COMBINED_USER_ID)
            If UR IsNot Nothing Then
                Dim AltsRoles As clsAlternativesRoles = GetAlternativeRolesByObjectiveID(UR, ObjectiveID)
                Return If(AltsRoles IsNot Nothing, AltsRoles.UndefinedAlternativesList, New HashSet(Of Guid))
            Else
                Return New HashSet(Of Guid)
            End If
        End Function

        Public Sub ClearUserObjectivesRoles(ByVal UserID As Integer)
            Dim UR As clsUserRoles = GetUserRolesByID(UserID)
            If UR IsNot Nothing Then
                UR.ObjectivesRoles.Allowed.Clear()
                UR.ObjectivesRoles.Restricted.Clear()
                UR.ObjectivesRoles.Undefined.Clear()
            End If
        End Sub

        Public Sub ClearUserAlternativesRoles(ByVal UserID As Integer, ByVal ObjectiveID As Guid) 'C0930
            Dim UR As clsUserRoles = GetUserRolesByID(UserID)
            If UR IsNot Nothing Then
                Dim AltsRoles As clsAlternativesRoles = GetAlternativeRolesByObjectiveID(UR, ObjectiveID)
                If AltsRoles IsNot Nothing Then
                    AltsRoles.AllowedAlternativesList.Clear()
                    AltsRoles.RestrictedAlternativesList.Clear()
                    AltsRoles.UndefinedAlternativesList.Clear()
                End If
            End If
        End Sub

        Public Function IsAllowedAlternative(ByVal ObjectiveID As Guid, ByVal AlternativeID As Guid, ByVal UserID As Integer, Optional ByVal CheckPureRoles As Boolean = True) As Boolean
            Dim node As clsNode = ProjectManager.ActiveObjectives.GetNodeByID(ObjectiveID)
            If node Is Nothing Then
                For Each H As clsHierarchy In ProjectManager.Hierarchies
                    If H IsNot ProjectManager.ActiveObjectives Then
                        node = H.GetNodeByID(ObjectiveID)
                        If node IsNot Nothing Then Exit For
                    End If
                Next
            End If
            Dim alt As clsNode = ProjectManager.ActiveAlternatives.GetNodeByID(AlternativeID)

            If node Is Nothing OrElse alt Is Nothing Then Return False

            Dim UR As clsUserRoles = GetUserRolesByID(UserID)

            Dim AltsRoles As clsAlternativesRoles

            ' check individual roles first
            If UR IsNot Nothing AndAlso CheckPureRoles Then
                AltsRoles = GetAlternativeRolesByObjectiveID(UR, ObjectiveID)
                If AltsRoles IsNot Nothing Then
                    If AltsRoles.RestrictedAlternativesList.Contains(AlternativeID) Then Return False
                    If AltsRoles.AllowedAlternativesList.Contains(AlternativeID) Then Return True
                End If
            End If

            ' need to check all groups
            Dim isRestrictedInSomeGroup As Boolean = False
            Dim isAllowedInSomeGroup As Boolean = False
            Dim isUndefinedInDefaultGroup As Boolean = False
            Dim i As Integer = 0
            While (i < ProjectManager.CombinedGroups.GroupsList.Count) And Not isRestrictedInSomeGroup
                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GroupsList(i), clsCombinedGroup)
                'CG.ApplyRules()
                If (CG.CombinedUserID = COMBINED_USER_ID) OrElse CG.ContainsUser(UserID) Then
                    Dim GR As clsUserRoles = GetUserRolesByID(CG.CombinedUserID)
                    If GR IsNot Nothing Then
                        AltsRoles = GetAlternativeRolesByObjectiveID(GR, ObjectiveID)
                        If AltsRoles IsNot Nothing Then
                            If AltsRoles.RestrictedAlternativesList.Contains(AlternativeID) Then isRestrictedInSomeGroup = True
                            If Not isRestrictedInSomeGroup Then
                                If AltsRoles.AllowedAlternativesList.Contains(AlternativeID) Then isAllowedInSomeGroup = True
                                If CG.CombinedUserID = COMBINED_USER_ID Then
                                    If AltsRoles.UndefinedAlternativesList.Contains(AlternativeID) Then
                                        isUndefinedInDefaultGroup = True
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
                i += 1
            End While

            If isRestrictedInSomeGroup Then
                Return False
            Else
                If isAllowedInSomeGroup Then
                    Return True
                Else
                    'Return IsAllowedByDefault
                    Return If(isUndefinedInDefaultGroup, False, IsAllowedByDefault)
                End If
            End If
        End Function

        Public Function SetObjectivesRoles(ByVal UserID As Integer, ByVal ObjectiveID As Guid, ByVal Value As RolesValueType) As Boolean
            Dim nodesList As New List(Of Guid)
            nodesList.Add(ObjectiveID)

            SetObjectivesRoles(UserID, nodesList, Value)
        End Function

        Public Function SetObjectivesRoles(ByVal UserID As Integer, ByVal ObjectiveIDs As List(Of Guid), ByVal Value As RolesValueType, Optional ForceSet As Boolean = False) As Boolean
            Dim UR As clsUserRoles = GetUserRolesByID(UserID)
            If UR Is Nothing Then
                UR = New clsUserRoles
                mUserRoles.Add(UserID, UR)
            End If

            Select Case Value
                Case RolesValueType.rvtAllowed
                    If ForceSet Then
                        UR.ObjectivesRoles.Allowed = ObjectiveIDs
                    Else
                        For Each ObjectiveID As Guid In ObjectiveIDs
                            UR.ObjectivesRoles.Restricted.Remove(ObjectiveID)
                        Next
                        For Each ObjectiveID As Guid In ObjectiveIDs
                            If Not UR.ObjectivesRoles.Allowed.Contains(ObjectiveID) Then
                                UR.ObjectivesRoles.Allowed.Add(ObjectiveID)
                            End If
                        Next
                        If UserID = COMBINED_USER_ID Then
                            For Each ObjectiveID As Guid In ObjectiveIDs
                                UR.ObjectivesRoles.Undefined.Remove(ObjectiveID)
                            Next
                        End If
                    End If
                Case RolesValueType.rvtRestricted
                    If ForceSet Then
                        UR.ObjectivesRoles.Restricted = ObjectiveIDs
                    Else
                        For Each ObjectiveID As Guid In ObjectiveIDs
                            UR.ObjectivesRoles.Allowed.Remove(ObjectiveID)
                        Next
                        For Each ObjectiveID As Guid In ObjectiveIDs
                            If Not UR.ObjectivesRoles.Restricted.Contains(ObjectiveID) Then
                                UR.ObjectivesRoles.Restricted.Add(ObjectiveID)
                            End If
                        Next
                        If UserID = COMBINED_USER_ID Then
                            For Each ObjectiveID As Guid In ObjectiveIDs
                                UR.ObjectivesRoles.Undefined.Remove(ObjectiveID)
                            Next
                        End If
                    End If
                Case RolesValueType.rvtUndefined
                    If ForceSet And UserID = COMBINED_USER_ID Then
                        UR.ObjectivesRoles.Undefined = ObjectiveIDs
                    Else
                        For Each ObjectiveID As Guid In ObjectiveIDs
                            UR.ObjectivesRoles.Allowed.Remove(ObjectiveID)
                        Next
                        For Each ObjectiveID As Guid In ObjectiveIDs
                            UR.ObjectivesRoles.Restricted.Remove(ObjectiveID)
                        Next
                        If UserID = COMBINED_USER_ID Then
                            For Each ObjectiveID As Guid In ObjectiveIDs
                                If Not UR.ObjectivesRoles.Undefined.Contains(ObjectiveID) Then
                                    UR.ObjectivesRoles.Undefined.Add(ObjectiveID)
                                End If
                            Next
                        End If
                    End If
            End Select

            UR.UpdateTime = Now
            Return True
        End Function

        Public Overloads Function SetAlternativesRoles(ByVal UserID As Integer, ByVal ObjectiveID As Guid, ByVal AlternativeID As Guid, ByVal Value As RolesValueType) As Boolean 'C0901 'C1060
            Dim nodesList As New List(Of Guid)
            nodesList.Add(ObjectiveID)

            Dim alts As New HashSet(Of Guid)
            alts.Add(AlternativeID)

            Return SetAlternativesRoles(UserID, nodesList, alts, Value)
        End Function

        Public Overloads Function SetAlternativesRoles(ByVal UserID As Integer, ByVal ObjectiveIDs As List(Of Guid), ByVal AlternativeIDs As HashSet(Of Guid), ByVal Value As RolesValueType, Optional ForceSet As Boolean = False) As Boolean
            Dim UR As clsUserRoles = GetUserRolesByID(UserID)
            If UR Is Nothing Then
                UR = New clsUserRoles
                mUserRoles.Add(UserID, UR)
            End If

            For Each ObjectiveID As Guid In ObjectiveIDs
                Dim AltsRoles As clsAlternativesRoles = GetAlternativeRolesByObjectiveID(UR, ObjectiveID)
                If AltsRoles Is Nothing Then
                    AltsRoles = New clsAlternativesRoles
                    AltsRoles.CoveringObjectiveID = ObjectiveID
                    UR.AlternativesRoles.Add(ObjectiveID, AltsRoles)
                End If

                For Each AltID As Guid In AlternativeIDs
                    Select Case Value
                        Case RolesValueType.rvtAllowed
                            If ForceSet Then
                                AltsRoles.AllowedAlternativesList = AlternativeIDs
                            Else
                                AltsRoles.RestrictedAlternativesList.Remove(AltID)
                                If Not AltsRoles.AllowedAlternativesList.Contains(AltID) Then
                                    AltsRoles.AllowedAlternativesList.Add(AltID)
                                End If
                                If UserID = COMBINED_USER_ID Then
                                    AltsRoles.UndefinedAlternativesList.Remove(AltID)
                                End If
                            End If
                        Case RolesValueType.rvtRestricted
                            If ForceSet Then
                                AltsRoles.RestrictedAlternativesList = AlternativeIDs
                            Else
                                AltsRoles.AllowedAlternativesList.Remove(AltID)
                                If Not AltsRoles.RestrictedAlternativesList.Contains(AltID) Then
                                    AltsRoles.RestrictedAlternativesList.Add(AltID)
                                End If
                                If UserID = COMBINED_USER_ID Then
                                    AltsRoles.UndefinedAlternativesList.Remove(AltID)
                                End If
                            End If
                        Case RolesValueType.rvtUndefined
                            If ForceSet And UserID = COMBINED_USER_ID Then
                                AltsRoles.UndefinedAlternativesList = AlternativeIDs
                            Else
                                AltsRoles.AllowedAlternativesList.Remove(AltID)
                                AltsRoles.RestrictedAlternativesList.Remove(AltID)
                                If UserID = COMBINED_USER_ID Then
                                    If Not AltsRoles.UndefinedAlternativesList.Contains(AltID) Then
                                        AltsRoles.UndefinedAlternativesList.Add(AltID)
                                    End If
                                End If
                            End If
                    End Select
                Next
            Next

            UR.UpdateTime = Now
            Return True
        End Function

        Private Function GetRoleStatisticFromList(ObjectiveID As Guid, AlternativeID As Guid, ListOfRoles As List(Of RolesStatistics)) As RolesStatistics
            Return ListOfRoles.Find(Function(rs) (rs.ObjectiveID.Equals(ObjectiveID) And rs.AlternativeID.Equals(AlternativeID)))
        End Function

        Function GetRolesStatistics(Optional SendType As RolesToSendType = RolesToSendType.rstAlternativesOnly) As List(Of RolesStatistics)
            With ProjectManager
                Dim res As New List(Of RolesStatistics)
                Dim RS As RolesStatistics
                Dim H As clsHierarchy = .Hierarchy(.ActiveHierarchy)
                Dim AH As clsHierarchy = .AltsHierarchy(.ActiveAltsHierarchy)

                If SendType = RolesToSendType.rstObjectivesOnly Or SendType = RolesToSendType.rstAll Then
                    For Each node As clsNode In H.Nodes
                        RS = New RolesStatistics
                        RS.ObjectiveID = node.NodeGuidID
                        RS.AlternativeID = Guid.Empty
                        RS.AllowedCount = 0
                        RS.RestrictedCount = 0
                        RS.EvaluatedCount = 0
                        res.Add(RS)
                    Next
                End If

                If SendType = RolesToSendType.rstAlternativesOnly Or SendType = RolesToSendType.rstAll Then
                    For Each node As clsNode In H.TerminalNodes
                        For Each alt As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                            RS = New RolesStatistics
                            RS.ObjectiveID = node.NodeGuidID
                            RS.AlternativeID = alt.NodeGuidID
                            RS.AllowedCount = 0
                            RS.RestrictedCount = 0
                            res.Add(RS)
                        Next
                    Next

                    If .IsRiskProject And H.HierarchyID = ECHierarchyID.hidLikelihood And H.Nodes.Count > 1 Then
                        For Each alt As clsNode In H.GetUncontributedAlternatives()
                            RS = New RolesStatistics
                            RS.ObjectiveID = H.Nodes(0).NodeGuidID
                            RS.AlternativeID = alt.NodeGuidID
                            RS.AllowedCount = 0
                            RS.RestrictedCount = 0
                            RS.EvaluatedCount = 0
                            res.Add(RS)
                        Next
                    End If
                End If

                For Each user As clsUser In .UsersList
                    If user IsNot .User Then .StorageManager.Reader.LoadUserData(user)

                    If SendType = RolesToSendType.rstObjectivesOnly Or SendType = RolesToSendType.rstAll Then
                        For Each node As ECCore.clsNode In H.Nodes
                            RS = GetRoleStatisticFromList(node.NodeGuidID, Guid.Empty, res)
                            If RS IsNot Nothing Then
                                Dim isAllowed As Boolean = .UsersRoles.IsAllowedObjective(node.NodeGuidID, user.UserID)
                                RS.AllowedCount += CInt(If(isAllowed, 1, 0))
                                RS.RestrictedCount += CInt(If(isAllowed, 0, 1))
                            End If
                        Next
                    End If

                    If SendType = RolesToSendType.rstAlternativesOnly Or SendType = RolesToSendType.rstAll Then
                        For Each node As clsNode In H.TerminalNodes
                            For Each alt As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                RS = GetRoleStatisticFromList(node.NodeGuidID, alt.NodeGuidID, res)
                                If RS IsNot Nothing Then
                                    Dim isAllowed As Boolean = .UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, user.UserID)
                                    RS.AllowedCount += CInt(If(isAllowed, 1, 0))
                                    RS.RestrictedCount += CInt(If(isAllowed, 0, 1))
                                    If Not IsPWMeasurementType(node.MeasureType) Then
                                        Dim j As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, user.UserID)
                                        If j IsNot Nothing AndAlso Not j.IsUndefined Then RS.EvaluatedCount += 1
                                    Else
                                        RS.EvaluatedCount = -1
                                    End If
                                End If
                            Next
                        Next

                        If .IsRiskProject And H.HierarchyID = ECHierarchyID.hidLikelihood And H.Nodes.Count > 1 Then
                            For Each alt As clsNode In H.GetUncontributedAlternatives
                                RS = GetRoleStatisticFromList(H.Nodes(0).NodeGuidID, alt.NodeGuidID, res)
                                If RS IsNot Nothing Then
                                    Dim isAllowed As Boolean = .UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, user.UserID)
                                    RS.AllowedCount += CInt(If(isAllowed, 1, 0))
                                    RS.RestrictedCount += CInt(If(isAllowed, 0, 1))
                                    If Not IsPWMeasurementType(alt.MeasureType) Then
                                        Dim j As clsNonPairwiseMeasureData = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, H.Nodes(0).NodeID, user.UserID)
                                        If j IsNot Nothing AndAlso Not j.IsUndefined Then RS.EvaluatedCount += 1
                                    Else
                                        RS.EvaluatedCount = -1
                                    End If
                                End If
                            Next
                        End If
                    End If

                    If user IsNot .User Then
                        .CleanUpUserPermissionsFromMemory(user.UserID)
                        .CleanUpUserDataFromMemory(H.HierarchyID, user.UserID)
                    End If
                Next
                Return res
            End With
        End Function

        Public Sub CleanUpUserRoles(ByVal UserID As Integer)
            mUserRoles.Remove(UserID)
        End Sub

        Public Function GetAllowedAlternatives(ByVal AUserID As Integer, WRTNode As clsNode) As List(Of clsNode)
            With ProjectManager
                Dim res As New List(Of clsNode)

                For Each node As clsNode In .Hierarchy(.ActiveHierarchy).TerminalNodes
                    If node.Enabled AndAlso (IsCombinedUserID(AUserID) OrElse (Not IsCombinedUserID(AUserID) AndAlso Not node.DisabledForUser(AUserID) AndAlso (node.IsAllowed(AUserID) Or .CalculationsManager.UseCombinedForRestrictedNodes))) Then
                        Dim alts As List(Of clsNode)
                        If IsCombinedUserID(AUserID) Then
                            alts = node.GetNodesBelow(UNDEFINED_USER_ID)
                        Else
                            If .CalculationsManager.UseCombinedForRestrictedNodes Then
                                alts = node.GetNodesBelow(UNDEFINED_USER_ID)
                            Else
                                alts = node.GetNodesBelow(AUserID)
                            End If
                        End If
                        For Each alt As clsNode In alts
                            If alt.Enabled AndAlso Not res.Contains(alt) AndAlso Not alt.DisabledForUser(AUserID) Then
                                res.Add(alt)
                            End If
                        Next
                    End If
                Next

                If WRTNode.Hierarchy.ProjectManager.IsRiskProject AndAlso WRTNode.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso WRTNode.ParentNodeID = -1 Then
                    For Each alt As clsNode In .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes
                        If Not MiscFuncs.HasContribution(alt, .Hierarchy(.ActiveHierarchy)) Then
                            If Not res.Contains(alt) Then
                                res.Add(alt)
                            End If
                        End If
                    Next
                End If

                Return res
            End With
        End Function

        Public Function GetParticipantsRolesView(UserIDs As List(Of Integer)) As Dictionary(Of Integer, Dictionary(Of Integer, RolesData))
            Dim res As New Dictionary(Of Integer, Dictionary(Of Integer, RolesData))

            Dim uncontributedAlts As New List(Of clsNode)
            Dim nodes As New List(Of clsNode)(ProjectManager.ActiveObjectives.TerminalNodes)

            If ProjectManager.IsRiskProject AndAlso ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
                uncontributedAlts = ProjectManager.ActiveObjectives.GetUncontributedAlternatives
                If uncontributedAlts.Count > 0 Then
                    nodes.Add(ProjectManager.ActiveObjectives.Nodes(0))
                End If
            End If

            For Each userID As Integer In UserIDs
                Dim user As clsUser = ProjectManager.GetUserByID(userID)
                ProjectManager.StorageManager.Reader.LoadUserPermissions(user)
                Dim UR As clsUserRoles = GetUserRolesByID(userID)

                For Each node As clsNode In nodes
                    Dim nodeID As Integer = If(uncontributedAlts.Count > 0 AndAlso node.NodeGuidID.Equals(ProjectManager.ActiveObjectives.Nodes(0).NodeGuidID), -1, node.NodeID)
                    Dim objDic As Dictionary(Of Integer, RolesData)
                    If Not res.ContainsKey(nodeID) Then
                        objDic = New Dictionary(Of Integer, RolesData)
                        res.Add(nodeID, objDic)
                    End If
                    objDic = res(nodeID)

                    Dim alternatives As List(Of clsNode) = If(nodeID = -1, uncontributedAlts, node.GetNodesBelow(UNDEFINED_USER_ID))
                    For Each alt As clsNode In alternatives
                        Dim isAllowed As Boolean = IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, userID)
                        Dim AltsRoles As clsAlternativesRoles
                        Dim value As RolesData
                        If Not objDic.ContainsKey(alt.NodeID) Then
                            value = New RolesData

                            ' get final role
                            value.FinalRole = If(isAllowed, RoleType.Allowed, RoleType.Restricted)

                            ' get individual role
                            If UR IsNot Nothing Then
                                AltsRoles = GetAlternativeRolesByObjectiveID(UR, node.NodeGuidID)
                                If AltsRoles IsNot Nothing Then
                                    If AltsRoles.RestrictedAlternativesList.Contains(alt.NodeGuidID) Then
                                        value.IndividualRole = RoleType.Restricted
                                    Else
                                        If AltsRoles.AllowedAlternativesList.Contains(alt.NodeGuidID) Then
                                            value.IndividualRole = RoleType.Allowed
                                        Else
                                            value.IndividualRole = RoleType.Undefined
                                        End If
                                    End If
                                Else
                                    value.IndividualRole = RoleType.Undefined
                                End If
                            Else
                                value.IndividualRole = RoleType.Undefined
                            End If

                            'get group role
                            Dim isRestrictedInSomeGroup As Boolean = False
                            Dim isAllowedInSomeGroup As Boolean = False
                            Dim isUndefinedInDefaultGroup As Boolean = False
                            Dim i As Integer = 0
                            While (i < ProjectManager.CombinedGroups.GroupsList.Count) And Not isRestrictedInSomeGroup
                                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GroupsList(i), clsCombinedGroup)
                                'CG.ApplyRules()
                                If (CG.CombinedUserID = COMBINED_USER_ID) OrElse CG.ContainsUser(userID) Then
                                    Dim GR As clsUserRoles = GetUserRolesByID(CG.CombinedUserID)
                                    If GR IsNot Nothing Then
                                        AltsRoles = GetAlternativeRolesByObjectiveID(GR, node.NodeGuidID)
                                        If AltsRoles IsNot Nothing Then
                                            If AltsRoles.RestrictedAlternativesList.Contains(alt.NodeGuidID) Then isRestrictedInSomeGroup = True
                                            If Not isRestrictedInSomeGroup Then
                                                If AltsRoles.AllowedAlternativesList.Contains(alt.NodeGuidID) Then isAllowedInSomeGroup = True
                                                If CG.CombinedUserID = COMBINED_USER_ID Then
                                                    If AltsRoles.UndefinedAlternativesList.Contains(alt.NodeGuidID) Then
                                                        isUndefinedInDefaultGroup = True
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                                i += 1
                            End While

                            If isRestrictedInSomeGroup Then
                                value.GroupRole = RoleType.Restricted
                            Else
                                If isAllowedInSomeGroup Then
                                    value.GroupRole = RoleType.Allowed
                                Else
                                    'value.GroupRole = If(IsAllowedByDefault, RoleType.Allowed, RoleType.Restricted)
                                    value.GroupRole = If(If(isUndefinedInDefaultGroup, False, IsAllowedByDefault), RoleType.Allowed, RoleType.Restricted)
                                End If
                            End If

                            objDic.Add(alt.NodeID, value)
                        Else
                            value = objDic(alt.NodeID)
                            If value.FinalRole <> RoleType.Mixed Then
                                If (value.FinalRole = RoleType.Allowed And Not isAllowed) OrElse (value.FinalRole = RoleType.Restricted And isAllowed) Then
                                    value.FinalRole = RoleType.Mixed
                                End If
                            End If
                            If value.IndividualRole <> RoleType.Mixed Then
                                If UR IsNot Nothing Then
                                    AltsRoles = GetAlternativeRolesByObjectiveID(UR, node.NodeGuidID)
                                    If AltsRoles IsNot Nothing Then
                                        If AltsRoles.RestrictedAlternativesList.Contains(alt.NodeGuidID) Then
                                            If value.IndividualRole <> RoleType.Restricted Then
                                                value.IndividualRole = RoleType.Mixed
                                            End If
                                        End If
                                        If AltsRoles.AllowedAlternativesList.Contains(alt.NodeGuidID) Then
                                            If value.IndividualRole <> RoleType.Allowed Then
                                                value.IndividualRole = RoleType.Mixed
                                            End If
                                        End If
                                    Else
                                        If value.IndividualRole <> RoleType.Undefined Then
                                            value.IndividualRole = RoleType.Mixed
                                        End If
                                    End If
                                Else
                                    If value.IndividualRole <> RoleType.Undefined Then
                                        value.IndividualRole = RoleType.Mixed
                                    End If
                                End If
                            End If
                            If value.GroupRole <> RoleType.Mixed Then
                                'get group role
                                Dim isRestrictedInSomeGroup As Boolean = False
                                Dim isAllowedInSomeGroup As Boolean = False
                                Dim isUndefinedInDefaultGroup As Boolean = False
                                Dim i As Integer = 0
                                While (i < ProjectManager.CombinedGroups.GroupsList.Count) And Not isRestrictedInSomeGroup
                                    Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GroupsList(i), clsCombinedGroup)
                                    'CG.ApplyRules()
                                    If (CG.CombinedUserID = COMBINED_USER_ID) OrElse CG.ContainsUser(userID) Then
                                        Dim GR As clsUserRoles = GetUserRolesByID(CG.CombinedUserID)
                                        If GR IsNot Nothing Then
                                            AltsRoles = GetAlternativeRolesByObjectiveID(GR, node.NodeGuidID)
                                            If AltsRoles IsNot Nothing Then
                                                If AltsRoles.RestrictedAlternativesList.Contains(alt.NodeGuidID) Then isRestrictedInSomeGroup = True
                                                If Not isRestrictedInSomeGroup Then
                                                    If AltsRoles.AllowedAlternativesList.Contains(alt.NodeGuidID) Then isAllowedInSomeGroup = True
                                                    If CG.CombinedUserID = COMBINED_USER_ID Then
                                                        If AltsRoles.UndefinedAlternativesList.Contains(alt.NodeGuidID) Then
                                                            isUndefinedInDefaultGroup = True
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                    i += 1
                                End While

                                If isRestrictedInSomeGroup Then
                                    If value.GroupRole <> RoleType.Restricted Then
                                        value.GroupRole = RoleType.Mixed
                                    End If
                                Else
                                    If isAllowedInSomeGroup Then
                                        If value.GroupRole <> RoleType.Allowed Then
                                            value.GroupRole = RoleType.Mixed
                                        End If
                                    Else
                                        If value.GroupRole <> If(If(isUndefinedInDefaultGroup, False, IsAllowedByDefault), RoleType.Allowed, RoleType.Restricted) Then
                                            value.GroupRole = RoleType.Mixed
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                Next
                If user IsNot ProjectManager.User Then CleanUpUserRoles(userID)
            Next
            Return res
        End Function

        Public Function GetParticipantsRolesViewForObjectives(UserIDs As List(Of Integer)) As Dictionary(Of Integer, RolesData)
            Dim res As New Dictionary(Of Integer, RolesData)
            For Each userID As Integer In UserIDs
                Dim user As clsUser = ProjectManager.GetUserByID(userID)
                ProjectManager.StorageManager.Reader.LoadUserPermissions(user)
                Dim UR As clsUserRoles = GetUserRolesByID(userID)
                For Each node As clsNode In ProjectManager.ActiveObjectives.Nodes
                    If Not node.IsTerminalNode Then
                        Dim value As RolesData
                        If Not res.ContainsKey(node.NodeID) Then
                            value = New RolesData
                            res.Add(node.NodeID, value)
                            value.FinalRole = If(IsAllowedObjective(node.NodeGuidID, userID, True), RoleType.Allowed, RoleType.Restricted)
                            If UR IsNot Nothing Then
                                If UR.ObjectivesRoles.Allowed.Contains(node.NodeGuidID) Then
                                    value.IndividualRole = RoleType.Allowed
                                Else
                                    If UR.ObjectivesRoles.Restricted.Contains(node.NodeGuidID) Then
                                        value.IndividualRole = RoleType.Restricted
                                    Else
                                        value.IndividualRole = RoleType.Undefined
                                    End If
                                End If
                            End If

                            ' check all groups
                            Dim isRestrictedInSomeGroup As Boolean = False
                            Dim isAllowedInSomeGroup As Boolean = False
                            Dim isUndefinedInDefaultGroup As Boolean = False
                            Dim i As Integer = 0
                            While (i < ProjectManager.CombinedGroups.GroupsList.Count) And Not isRestrictedInSomeGroup
                                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GroupsList(i), clsCombinedGroup)
                                'CG.ApplyRules()
                                If (CG.CombinedUserID = COMBINED_USER_ID) OrElse CG.ContainsUser(userID) Then
                                    Dim GR As clsUserRoles = GetUserRolesByID(CG.CombinedUserID)
                                    If GR IsNot Nothing Then
                                        If GR.ObjectivesRoles.Restricted.Contains(node.NodeGuidID) Then isRestrictedInSomeGroup = True
                                        If Not isRestrictedInSomeGroup Then
                                            If GR.ObjectivesRoles.Allowed.Contains(node.NodeGuidID) Then isAllowedInSomeGroup = True
                                            If CG.CombinedUserID = COMBINED_USER_ID Then
                                                If GR.ObjectivesRoles.Undefined.Contains(node.NodeGuidID) Then isUndefinedInDefaultGroup = True
                                            End If
                                        End If
                                    End If
                                End If
                                i += 1
                            End While

                            If isRestrictedInSomeGroup Then
                                value.GroupRole = RoleType.Restricted
                            Else
                                If isAllowedInSomeGroup Then
                                    value.GroupRole = RoleType.Allowed
                                Else
                                    'value.GroupRole = If(IsAllowedByDefault, RoleType.Allowed, RoleType.Restricted)
                                    value.GroupRole = If(If(isUndefinedInDefaultGroup, False, IsAllowedByDefault), RoleType.Allowed, RoleType.Restricted)
                                End If
                            End If
                        Else
                            value = res(node.NodeID)
                            If value.FinalRole <> RoleType.Mixed Then
                                Dim isAllowed As Boolean = IsAllowedObjective(node.NodeGuidID, userID)
                                If (value.FinalRole = RoleType.Allowed And Not isAllowed) OrElse (value.FinalRole = RoleType.Restricted And isAllowed) Then
                                    value.FinalRole = RoleType.Mixed
                                End If
                            End If
                            If value.IndividualRole <> RoleType.Mixed Then
                                If UR IsNot Nothing Then
                                    Dim ObjRoles As clsObjectivesRoles = UR.ObjectivesRoles
                                    If ObjRoles.Restricted.Contains(node.NodeGuidID) Then
                                        If value.IndividualRole <> RoleType.Restricted Then
                                            value.IndividualRole = RoleType.Mixed
                                        End If
                                    End If
                                    If ObjRoles.Allowed.Contains(node.NodeGuidID) Then
                                        If value.IndividualRole <> RoleType.Allowed Then
                                            value.IndividualRole = RoleType.Mixed
                                        End If
                                    End If
                                Else
                                    If value.IndividualRole <> RoleType.Undefined Then
                                        value.IndividualRole = RoleType.Mixed
                                    End If
                                End If
                            End If

                            If value.GroupRole <> RoleType.Mixed Then
                                ' check all groups
                                Dim isRestrictedInSomeGroup As Boolean = False
                                Dim isAllowedInSomeGroup As Boolean = False
                                Dim isUndefinedInDefaultGroup As Boolean = False
                                Dim i As Integer = 0
                                While (i < ProjectManager.CombinedGroups.GroupsList.Count) And Not isRestrictedInSomeGroup
                                    Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GroupsList(i), clsCombinedGroup)
                                    'CG.ApplyRules()
                                    If (CG.CombinedUserID = COMBINED_USER_ID) OrElse CG.ContainsUser(userID) Then
                                        Dim GR As clsUserRoles = GetUserRolesByID(CG.CombinedUserID)
                                        If GR IsNot Nothing Then
                                            If GR.ObjectivesRoles.Restricted.Contains(node.NodeGuidID) Then isRestrictedInSomeGroup = True
                                            If Not isRestrictedInSomeGroup Then
                                                If GR.ObjectivesRoles.Allowed.Contains(node.NodeGuidID) Then isAllowedInSomeGroup = True
                                                If CG.CombinedUserID = COMBINED_USER_ID Then
                                                    If GR.ObjectivesRoles.Undefined.Contains(node.NodeGuidID) Then isUndefinedInDefaultGroup = True
                                                End If
                                            End If
                                        End If
                                    End If
                                    i += 1
                                End While

                                If isRestrictedInSomeGroup Then
                                    If value.GroupRole <> RoleType.Restricted Then
                                        value.GroupRole = RoleType.Mixed
                                    End If
                                Else
                                    If isAllowedInSomeGroup Then
                                        If value.GroupRole <> RoleType.Allowed Then
                                            value.GroupRole = RoleType.Mixed
                                        End If
                                    Else
                                        If value.GroupRole <> If(If(isUndefinedInDefaultGroup, False, IsAllowedByDefault), RoleType.Allowed, RoleType.Restricted) Then
                                            value.GroupRole = RoleType.Mixed
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
                If user IsNot ProjectManager.User Then CleanUpUserRoles(userID)
            Next
            Return res
        End Function

        Public Function GetGroupsRolesView(CombinedUserIDs As List(Of Integer)) As Dictionary(Of Integer, Dictionary(Of Integer, RoleType))
            Dim res As New Dictionary(Of Integer, Dictionary(Of Integer, RoleType))

            Dim uncontributedAlts As New List(Of clsNode)
            Dim nodes As New List(Of clsNode)(ProjectManager.ActiveObjectives.TerminalNodes)

            If ProjectManager.IsRiskProject AndAlso ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
                uncontributedAlts = ProjectManager.ActiveObjectives.GetUncontributedAlternatives
                If uncontributedAlts.Count > 0 Then
                    nodes.Add(ProjectManager.ActiveObjectives.Nodes(0))
                End If
            End If

            For Each cgID As Integer In CombinedUserIDs
                Dim UR As clsUserRoles = GetUserRolesByID(cgID)
                For Each node As clsNode In nodes
                    Dim objDic As Dictionary(Of Integer, RoleType)
                    Dim nodeID As Integer = If(uncontributedAlts.Count > 0 AndAlso node.NodeGuidID.Equals(ProjectManager.ActiveObjectives.Nodes(0).NodeGuidID), -1, node.NodeID)
                    If Not res.ContainsKey(nodeID) Then
                        objDic = New Dictionary(Of Integer, RoleType)
                        res.Add(nodeID, objDic)
                    End If
                    objDic = res(nodeID)
                    Dim alternatives As List(Of clsNode) = If(nodeID = -1, uncontributedAlts, node.GetNodesBelow(UNDEFINED_USER_ID))
                    For Each alt As clsNode In alternatives
                        Dim AltsRoles As clsAlternativesRoles
                        Dim value As RoleType
                        If Not objDic.ContainsKey(alt.NodeID) Then
                            If UR IsNot Nothing Then
                                AltsRoles = GetAlternativeRolesByObjectiveID(UR, node.NodeGuidID)
                                If AltsRoles IsNot Nothing Then
                                    If AltsRoles.RestrictedAlternativesList.Contains(alt.NodeGuidID) Then
                                        value = RoleType.Restricted
                                    Else
                                        If AltsRoles.AllowedAlternativesList.Contains(alt.NodeGuidID) Then
                                            value = RoleType.Allowed
                                        Else
                                            If AltsRoles.UndefinedAlternativesList.Contains(alt.NodeGuidID) Then
                                                value = RoleType.Undefined
                                            Else
                                                value = If(cgID = COMBINED_USER_ID, RoleType.Allowed, RoleType.Undefined)
                                            End If
                                        End If
                                    End If
                                Else
                                    value = If(cgID = COMBINED_USER_ID, RoleType.Allowed, RoleType.Undefined)
                                End If
                            Else
                                value = If(cgID = COMBINED_USER_ID, RoleType.Allowed, RoleType.Undefined)
                            End If

                            objDic.Add(alt.NodeID, value)
                        Else
                            value = objDic(alt.NodeID)
                            If value <> RoleType.Mixed Then
                                If UR IsNot Nothing Then
                                    AltsRoles = GetAlternativeRolesByObjectiveID(UR, node.NodeGuidID)
                                    If AltsRoles IsNot Nothing Then
                                        If AltsRoles.RestrictedAlternativesList.Contains(alt.NodeGuidID) Then
                                            If value <> RoleType.Restricted Then
                                                value = RoleType.Mixed
                                            End If
                                        End If
                                        If AltsRoles.AllowedAlternativesList.Contains(alt.NodeGuidID) Then
                                            If value <> RoleType.Allowed Then
                                                value = RoleType.Mixed
                                            End If
                                        End If
                                    Else
                                        If value <> RoleType.Undefined Then
                                            value = RoleType.Mixed
                                        End If
                                    End If
                                Else
                                    If value <> RoleType.Undefined Then
                                        value = RoleType.Mixed
                                    End If
                                End If
                            End If
                        End If
                    Next
                Next
            Next
            Return res
        End Function

        Public Function GetGroupsRolesViewForObjectives(CombinedUserIDs As List(Of Integer)) As Dictionary(Of Integer, RoleType)
            Dim res As New Dictionary(Of Integer, RoleType)
            For Each cgID As Integer In CombinedUserIDs
                Dim UR As clsUserRoles = GetUserRolesByID(cgID)
                For Each node As clsNode In ProjectManager.ActiveObjectives.Nodes
                    If Not node.IsTerminalNode Then
                        Dim value As RoleType
                        If Not res.ContainsKey(node.NodeID) Then
                            If UR IsNot Nothing Then
                                If UR.ObjectivesRoles.Allowed.Contains(node.NodeGuidID) Then
                                    value = RoleType.Allowed
                                Else
                                    If UR.ObjectivesRoles.Restricted.Contains(node.NodeGuidID) Then
                                        value = RoleType.Restricted
                                    Else
                                        If UR.ObjectivesRoles.Undefined.Contains(node.NodeGuidID) Then
                                            value = RoleType.Undefined
                                        Else
                                            value = If(cgID = COMBINED_USER_ID, RoleType.Allowed, RoleType.Undefined)
                                        End If
                                    End If
                                End If
                            Else
                                value = If(cgID = COMBINED_USER_ID, RoleType.Allowed, RoleType.Undefined)
                            End If
                            res.Add(node.NodeID, value)
                        Else
                            value = res(node.NodeID)
                            If value <> RoleType.Mixed Then
                                If UR IsNot Nothing Then
                                    If UR.ObjectivesRoles.Allowed.Contains(node.NodeGuidID) Then
                                        If value <> RoleType.Allowed Then
                                            value = RoleType.Mixed
                                        End If
                                    Else
                                        If UR.ObjectivesRoles.Restricted.Contains(node.NodeGuidID) Then
                                            If value <> RoleType.Restricted Then
                                                value = RoleType.Mixed
                                            End If
                                        Else
                                            If value <> RoleType.Undefined Then
                                                value = RoleType.Mixed
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
            Next
            Return res
        End Function

        Public Sub SetRolesForAlternatives(UserIDs As List(Of Integer), ObjectiveIDs As List(Of Integer), AlternativeIDs As List(Of Integer), Value As RolesValueType)
            For Each userID As Integer In UserIDs
                Dim user As clsUser = Nothing
                Dim CG As clsCombinedGroup = Nothing
                If Not IsCombinedUserID(userID) Then
                    user = ProjectManager.GetUserByID(userID)
                    ProjectManager.StorageManager.Reader.LoadUserPermissions(user)
                Else
                    CG = ProjectManager.CombinedGroups.GetCombinedGroupByUserID(userID)
                    ProjectManager.StorageManager.Reader.LoadGroupPermissions(CG)
                End If
                For Each objectiveID As Integer In ObjectiveIDs
                    If objectiveID = -1 Then objectiveID = ProjectManager.ActiveObjectives.Nodes(0).NodeID
                    Dim objectiveGuidID As Guid = ProjectManager.ActiveObjectives.GetNodeByID(objectiveID).NodeGuidID
                    For Each altID As Integer In AlternativeIDs
                        Dim altGuidID As Guid = ProjectManager.ActiveAlternatives.GetNodeByID(altID).NodeGuidID
                        SetAlternativesRoles(userID, objectiveGuidID, altGuidID, Value)
                    Next
                Next
                If Not IsCombinedUserID(userID) Then
                    ProjectManager.StorageManager.Writer.SaveUserPermissions(user)
                    If user IsNot ProjectManager.User Then CleanUpUserRoles(userID)
                Else
                    ProjectManager.StorageManager.Writer.SaveGroupPermissions(CG)
                End If
            Next
        End Sub

        Public Sub SetRolesForObjectives(UserIDs As List(Of Integer), ObjectiveIDs As List(Of Integer), Value As RolesValueType)
            Dim listOfGuids As List(Of Guid) = ProjectManager.ActiveObjectives.Nodes.Where(Function(node) ObjectiveIDs.Contains(node.NodeID)).Select(Function(x) x.NodeGuidID).ToList
            For Each userID As Integer In UserIDs
                Dim user As clsUser = Nothing
                Dim CG As clsCombinedGroup = Nothing
                If Not IsCombinedUserID(userID) Then
                    user = ProjectManager.GetUserByID(userID)
                    ProjectManager.StorageManager.Reader.LoadUserPermissions(user)
                Else
                    CG = ProjectManager.CombinedGroups.GetCombinedGroupByUserID(userID)
                    ProjectManager.StorageManager.Reader.LoadGroupPermissions(CG)
                End If

                SetObjectivesRoles(userID, listOfGuids, Value)

                If Not IsCombinedUserID(userID) Then
                    ProjectManager.StorageManager.Writer.SaveUserPermissions(user)
                    If user IsNot ProjectManager.User Then CleanUpUserRoles(userID)
                Else
                    ProjectManager.StorageManager.Writer.SaveGroupPermissions(CG)
                End If
            Next
        End Sub

        Public Sub CopyUserPermissions(ByVal SourceUserID As Integer, ByVal DestinationUserIDs As List(Of Integer), Optional ByVal CopyOptions As CopyRolesOptions = CopyRolesOptions.ObjectivesAndAlternatives)
            Dim sourceUser As clsUser = Nothing
            Dim sourceCG As clsCombinedGroup = Nothing
            If Not IsCombinedUserID(SourceUserID) Then
                sourceUser = ProjectManager.GetUserByID(SourceUserID)
                ProjectManager.StorageManager.Reader.LoadUserPermissions(sourceUser)
            Else
                sourceCG = ProjectManager.CombinedGroups.GetCombinedGroupByUserID(SourceUserID)
                ProjectManager.StorageManager.Reader.LoadGroupPermissions(sourceCG)
            End If

            For Each destUserID As Integer In DestinationUserIDs
                Dim destUser As clsUser = Nothing
                Dim destCG As clsCombinedGroup = Nothing
                If Not IsCombinedUserID(destUserID) Then
                    destUser = ProjectManager.GetUserByID(destUserID)
                    ProjectManager.StorageManager.Reader.LoadUserPermissions(destUser)
                Else
                    destCG = ProjectManager.CombinedGroups.GetCombinedGroupByUserID(destUserID)
                    ProjectManager.StorageManager.Reader.LoadGroupPermissions(destCG)
                End If

                If CopyOptions = CopyRolesOptions.Objectives Or CopyOptions = CopyRolesOptions.ObjectivesAndAlternatives Then
                    ProjectManager.UsersRoles.ClearUserObjectivesRoles(destUserID)
                    ProjectManager.UsersRoles.SetObjectivesRoles(destUserID, ProjectManager.UsersRoles.GetAllowedObjectives(SourceUserID), RolesValueType.rvtAllowed)
                    ProjectManager.UsersRoles.SetObjectivesRoles(destUserID, ProjectManager.UsersRoles.GetRestrictedObjectives(SourceUserID), RolesValueType.rvtRestricted)
                End If

                If CopyOptions = CopyRolesOptions.Alternatives Or CopyOptions = CopyRolesOptions.ObjectivesAndAlternatives Then
                    Dim objs As New List(Of Guid)
                    If ProjectManager.IsRiskProject And ProjectManager.ActiveObjectives.HierarchyID = ECHierarchyID.hidLikelihood And ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes.Count > 1 Then
                        objs.Add(ProjectManager.ActiveObjectives.Nodes(0).NodeGuidID)
                        ProjectManager.UsersRoles.ClearUserAlternativesRoles(destUserID, objs(0))
                        ProjectManager.UsersRoles.SetAlternativesRoles(destUserID, objs, ProjectManager.UsersRoles.GetRestrictedAlternatives(SourceUserID, objs(0)), RolesValueType.rvtRestricted)
                        ProjectManager.UsersRoles.SetAlternativesRoles(destUserID, objs, ProjectManager.UsersRoles.GetAllowedAlternatives(SourceUserID, objs(0)), RolesValueType.rvtAllowed)
                    End If

                    For Each node As clsNode In ProjectManager.ActiveObjectives.TerminalNodes
                        objs.Clear()
                        objs.Add(node.NodeGuidID)
                        ProjectManager.UsersRoles.ClearUserAlternativesRoles(destUserID, node.NodeGuidID)
                        ProjectManager.UsersRoles.SetAlternativesRoles(destUserID, objs, ProjectManager.UsersRoles.GetRestrictedAlternatives(SourceUserID, node.NodeGuidID), RolesValueType.rvtRestricted)
                        ProjectManager.UsersRoles.SetAlternativesRoles(destUserID, objs, ProjectManager.UsersRoles.GetAllowedAlternatives(SourceUserID, node.NodeGuidID), RolesValueType.rvtAllowed)
                    Next
                End If

                If Not IsCombinedUserID(destUserID) Then
                    ProjectManager.StorageManager.Writer.SaveUserPermissions(destUser)
                    If destUser IsNot ProjectManager.User Then CleanUpUserRoles(destUserID)
                Else
                    ProjectManager.StorageManager.Writer.SaveGroupPermissions(destCG)
                End If
            Next
        End Sub

        Public Sub PrintRoles()
            Debug.Print("--- GetParticipantsRolesView ---")
            Dim userIDs As New List(Of Integer)
            userIDs.Add(ProjectManager.UsersList(0).UserID)
            userIDs.Add(ProjectManager.UsersList(1).UserID)
            userIDs.Add(ProjectManager.UsersList(2).UserID)
            Dim res As Dictionary(Of Integer, Dictionary(Of Integer, RolesData)) = GetParticipantsRolesView(userIDs)
            For Each node As clsNode In ProjectManager.ActiveObjectives.TerminalNodes
                If res.ContainsKey(node.NodeID) Then
                    Debug.Print("Objective " + node.NodeName + ":")
                    For Each kvp As KeyValuePair(Of Integer, RolesData) In res(node.NodeID)
                        Debug.Print("  " + ProjectManager.ActiveAlternatives.GetNodeByID(kvp.Key).NodeName + ":")
                        Debug.Print("  " + "Group: " + kvp.Value.GroupRole.ToString + " Individual: " + kvp.Value.IndividualRole.ToString + " Final: " + kvp.Value.FinalRole.ToString)
                    Next
                Else
                    Debug.Print("Objective " + node.NodeName + " not found")
                End If
            Next
        End Sub

        Public Sub New(ByVal ProjectManager As clsProjectManager)
            mUserRoles = New Dictionary(Of Integer, clsUserRoles)
            Me.ProjectManager = ProjectManager
        End Sub
    End Class
End Namespace