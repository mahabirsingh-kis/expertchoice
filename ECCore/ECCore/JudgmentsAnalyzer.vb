Imports System.Linq

Namespace ECCore
    <Serializable()> Public Class clsJudgmentsAnalyzer
        Public Property SynthesisMode() As ECSynthesisMode
        Public Property ProjectManager() As clsProjectManager

        Public Function CanShowResultsForNode(ByVal UserID As Integer, ByVal node As clsNode, ByRef AlternativesEvaluated As Boolean, Optional Recursive As Boolean = True) As Boolean
            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                AlternativesEvaluated = True
                Return True 'TODO: This is temporary
            End If

            AlternativesEvaluated = True
            Return True

            Dim useCombined As Boolean = node.Hierarchy.ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes

            If (UserID >= 0) AndAlso Not node.IsAllowed(UserID) AndAlso Not useCombined Then Return False

            useCombined = False

            If Not node.IsTerminalNode Then
                ' if regular user or custom combined group
                If Not IsCombinedUserID(UserID) OrElse (IsCombinedUserID(UserID) AndAlso (UserID <> COMBINED_USER_ID)) Then
                    If Not node.IsAllowed(UserID) Then
                        useCombined = True
                    Else
                        ' if all participants group
                        useCombined = True
                        For Each u As clsUser In ProjectManager.CombinedGroups.GetCombinedGroupByUserID(UserID).UsersList
                            If node.IsAllowed(u.UserID) Then
                                useCombined = False
                                Exit For
                            End If
                        Next
                    End If
                End If

                'If Not IsCombinedUserID(UserID) Then
                '    useCombined = Not node.IsAllowed(UserID)
                'Else
                '    useCombined = True
                '    For Each u As clsUser In ProjectManager.CombinedGroups.GetCombinedGroupByUserID(UserID).UsersList
                '        ' load users permissions?
                '        If node.IsAllowed(u.UserID) Then
                '            useCombined = False
                '            Exit For
                '        End If
                '    Next
                'End If
            Else
                If Not IsCombinedUserID(UserID) Then
                    If node.GetNodesBelow(UserID).Count <> node.GetNodesBelow(UNDEFINED_USER_ID).Count Then
                        useCombined = True
                    End If
                Else
                    Dim nodesCount As Integer = node.GetNodesBelow(UNDEFINED_USER_ID).Count
                    For Each u As clsUser In ProjectManager.CombinedGroups.GetCombinedGroupByUserID(UserID).UsersList
                        If node.GetNodesBelow(u.UserID).Count <> nodesCount Then
                            useCombined = True
                            Exit For
                        End If
                    Next
                End If
            End If

            Dim CanShow As Boolean = True
            Dim tmpBool As Boolean

            Dim UserNodesBelow As List(Of clsNode) = node.GetNodesBelow(UserID)
            Dim AllNodesBelow As List(Of clsNode) = node.GetNodesBelow(UNDEFINED_USER_ID, , , UserID)

            Dim isAllCategories As Boolean = True

            If Not node.IsTerminalNode And node.Hierarchy.ProjectManager.IsRiskProject And node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                For i As Integer = UserNodesBelow.Count - 1 To 0 Step -1
                    If UserNodesBelow(i).RiskNodeType = RiskNodeType.ntCategory Then
                        UserNodesBelow.RemoveAt(i)
                    End If
                Next
                For i As Integer = AllNodesBelow.Count - 1 To 0 Step -1
                    If AllNodesBelow(i).RiskNodeType = RiskNodeType.ntCategory Then
                        AllNodesBelow.RemoveAt(i)
                    Else
                        isAllCategories = False
                    End If
                Next
            Else
                isAllCategories = False
            End If

            If isAllCategories Then
                CanShow = True
            Else
                Dim nodes As List(Of clsNode)
                Dim uID As Integer
                If Not IsCombinedUserID(UserID) AndAlso (Not useCombined Or (useCombined And node.IsAllowed(UserID) And (UserNodesBelow.Count = AllNodesBelow.Count))) Then 'C0901
                    nodes = UserNodesBelow
                    uID = UserID
                Else
                    nodes = AllNodesBelow
                    If IsCombinedUserID(UserID) Then
                        uID = UserID
                    Else
                        uID = node.Hierarchy.ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID
                    End If

                    If Not node.CombinedLoaded.Contains(uID) Then
                        Dim CG As clsCombinedGroup = node.Hierarchy.ProjectManager.CombinedGroups.GetCombinedGroupByUserID(uID)
                        ProjectManager.CalculationsManager.HierarchyID = ProjectManager.ActiveHierarchy
                        ProjectManager.CalculationsManager.CreateCombinedJudgments(CG)
                    End If
                End If

                If (nodes.Count = 0) Or (nodes.Count = 1) AndAlso (nodes.Count <> AllNodesBelow.Count) AndAlso IsPWMeasurementType(node.MeasureType) Then
                    CanShow = False
                Else
                    Select Case node.MeasureType
                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                            If Not CType(node.Judgments, clsPairwiseJudgments).HasSpanningSet(uID) Then
                                CanShow = False
                            End If
                        Case ECMeasureType.mtPWOutcomes
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                If Not child.PWOutcomesJudgments.HasSpanningSet(uID) Then
                                    CanShow = False
                                    Exit For
                                End If
                            Next
                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtStep, ECMeasureType.mtDirect, ECMeasureType.mtAdvancedUtilityCurve
                            Dim nonpwMD As clsNonPairwiseMeasureData

                            ' at least one judgment should be there
                            tmpBool = False

                            For Each childNode As clsNode In nodes
                                If Not useCombined Then
                                    nonpwMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, uID)
                                Else
                                    If Not IsCombinedUserID(uID) Then
                                        nonpwMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, COMBINED_USER_ID)
                                    Else
                                        For Each u As clsUser In ProjectManager.CombinedGroups.GetCombinedGroupByUserID(uid).UsersList
                                            If ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, childNode.NodeGuidID, u.UserID) Then
                                                useCombined = False
                                                Exit For
                                            End If
                                        Next
                                    End If
                                    nonpwMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, If(useCombined, COMBINED_USER_ID, uID))
                                End If
                                'If UserNodesBelow.Contains(childNode) Then
                                '    nonpwMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, uID) 'C0786
                                'Else
                                '    If useCombined Then
                                '        nonpwMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, COMBINED_USER_ID) 'C0786
                                '    Else
                                '        nonpwMD = Nothing
                                '    End If
                                'End If
                                If nonpwMD IsNot Nothing AndAlso Not nonpwMD.IsUndefined Then
                                    tmpBool = True
                                    Exit For
                                End If
                            Next

                            If Not tmpBool Then CanShow = False
                        Case Else
                            CanShow = False
                    End Select
                End If
            End If

            ' CanShow flag now tells us if we can calculate local priorities for children of node

            If Not Recursive Then Return CanShow

            If CanShow Then
                If node.IsTerminalNode Then
                    AlternativesEvaluated = True
                Else
                    For Each child As clsNode In node.Children
                        CanShowResultsForNode(UserID, child, AlternativesEvaluated)
                    Next
                End If
            End If
        End Function

        Private Function CanShowResultsForUser(ByVal UserID As Integer, Optional ByVal WRTNode As clsNode = Nothing) As Boolean
            Dim H As clsHierarchy = ProjectManager.ActiveObjectives
            Dim CalcWRTNode As clsNode = If(WRTNode Is Nothing, H.Nodes(0), WRTNode)

            Dim altsEvaluated As Boolean = False
            CanShowResultsForNode(UserID, CalcWRTNode, altsEvaluated)

            If ProjectManager.IsRiskProject AndAlso H.HierarchyID = ECHierarchyID.hidLikelihood Then
                If H.Nodes(0) Is WRTNode Then
                    If H.GetUncontributedAlternatives.Count > 0 Then
                        altsEvaluated = True
                    End If
                End If
            End If

            Return altsEvaluated
        End Function

        Public ReadOnly Property CanShowIndividualResults(ByVal UserID As Integer, Optional ByVal WRTNode As clsNode = Nothing) As Boolean
            Get
                If UserID >= 0 Then
                    Dim user As clsUser = ProjectManager.GetUserByID(UserID)
                    If (user IsNot Nothing) AndAlso (ProjectManager.User IsNot user) Then
                        If Not ProjectManager.StorageManager.Reader.JudgmentsMadeBefore(user.UserID, ProjectManager.ActiveHierarchy, -1, user.LastJudgmentTime) Then
                            ProjectManager.StorageManager.Reader.LoadUserData(user)
                        End If
                    End If
                End If
                Return CanShowResultsForUser(UserID, WRTNode)
            End Get
        End Property

        Public ReadOnly Property CanShowGroupResults(Optional ByVal WRTNode As clsNode = Nothing, Optional ByVal CombinedGroup As clsCombinedGroup = Nothing) As Boolean
            Get
                Dim CG As clsCombinedGroup = If(CombinedGroup Is Nothing, ProjectManager.CombinedGroups.GetDefaultCombinedGroup, CombinedGroup)
                Return CanShowResultsForUser(CG.CombinedUserID, WRTNode)
            End Get
        End Property

        Public Sub New(ByVal SynthesisMode As ECSynthesisMode, ByVal ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
        End Sub
    End Class
End Namespace