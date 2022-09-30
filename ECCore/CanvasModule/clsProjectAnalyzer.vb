Imports ECCore
Imports Canvas
Imports ECCore.MiscFuncs

Namespace Canvas

    Public Enum SynchronousCapabilityError 'C0101
        sceUnknown = 0
        sceHasRegularUtilityCurveNodes = 1
        sceHasAdvancedUtilityCurveNodes = 2
        sceHasDirectNodes = 3
        sceStepFunctionNodes = 4
        sceHasLessThanTwoParticipants = 5
    End Enum

    <Serializable()> Public Class clsProjectAnalyzer 'C0062

        Public Property ProjectManager() As clsProjectManager
        Public Property PipeParameters() As clsPipeParamaters



        Public Function GetTotalJudgmentsCount(ByVal UserID As Integer, ByVal HierarchyID As Integer, Optional ByVal PermissionsUserID As Integer = UNDEFINED_USER_ID, Optional LoadUserData As Boolean = True) As Integer
            If (ProjectManager Is Nothing) OrElse (PipeParameters Is Nothing) Then Return 0

            Dim H As clsHierarchy = ProjectManager.GetAnyHierarchyByID(HierarchyID)
            If H Is Nothing Then Return 0

            If ProjectManager.ActiveObjectives.Nodes.Count = 0 Then Return 0

            Dim user As clsUser = ProjectManager.GetUserByID(UserID)
            If user Is Nothing Then Return 0

            Dim SameUser As Boolean = False
            If PermissionsUserID = UNDEFINED_USER_ID OrElse PermissionsUserID = UserID Then
                PermissionsUserID = UserID
                SameUser = True
            End If

            If Not SameUser Then
                Dim PermissionsUser As clsUser = ProjectManager.GetUserByID(PermissionsUserID)
                If PermissionsUser Is Nothing Then Return False
                ProjectManager.StorageManager.Reader.LoadUserPermissions(PermissionsUser)
            End If
            If LoadUserData Then
                ProjectManager.StorageManager.Reader.LoadUserPermissions(user)
                ProjectManager.StorageManager.Reader.LoadUserDisabledNodes(user)
            End If

            Dim totalCount = 0

            For Each node As clsNode In H.Nodes
                Dim isTerminalNode As Boolean = node.IsTerminalNode
                If ((Not isTerminalNode And PipeParameters.EvaluateObjectives) OrElse (isTerminalNode And PipeParameters.EvaluateAlternatives)) Then
                    Dim isDisabled As Boolean
                    Dim isAllowed As Boolean
                    If SameUser Then
                        isDisabled = node.DisabledForUser(UserID)
                        isAllowed = node.IsAllowed(UserID)
                    Else
                        isDisabled = node.DisabledForUser(UserID) OrElse node.DisabledForUser(PermissionsUserID)
                        isAllowed = node.IsAllowed(UserID) AndAlso node.IsAllowed(PermissionsUserID)
                    End If

                    If node.Enabled AndAlso Not isDisabled AndAlso isAllowed Then
                        Dim nodeTotal As Integer

                        Dim NodesList As List(Of clsNode)
                        If SameUser Then
                            NodesList = node.GetNodesBelow(UserID,,,,, False)
                        Else
                            Dim OriginalAllowedNodesBelow As List(Of clsNode) = node.GetNodesBelow(UserID)
                            Dim PermissionsNodesBelow As List(Of clsNode) = node.GetNodesBelow(PermissionsUserID)
                            Dim GeneralNodesBelow As List(Of clsNode) = node.GetNodesBelow(UNDEFINED_USER_ID)
                            Dim resNodesBelow As New List(Of clsNode)

                            For Each nd As clsNode In GeneralNodesBelow
                                If OriginalAllowedNodesBelow.Contains(nd) And PermissionsNodesBelow.Contains(nd) Then
                                    resNodesBelow.Add(nd)
                                End If
                            Next

                            NodesList = resNodesBelow
                        End If

                        'If ProjectManager.IsRiskProject AndAlso node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso Not node.IsTerminalNode Then
                        '    NodesList.RemoveAll(Function(n) (n.RiskNodeType = RiskNodeType.ntCategory))
                        'End If

                        'If ProjectManager.IsRiskProject AndAlso node.IsTerminalNode Then
                        '    NodesList.RemoveAll(Function(n) (n.DisabledForUser(UserID)))
                        'End If

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                Dim n As Integer
                                Dim isSet As Boolean = False
                                Dim evalDiagonals As DiagonalsEvaluation
                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                    If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                        evalDiagonals = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                        isSet = True
                                    End If
                                    Dim RS As clsRatingScale = CType(node.MeasurementScale, clsRatingScale)
                                    n = RS.RatingSet.Count
                                Else
                                    n = NodesList.Count
                                End If

                                If Not isSet Then
                                    If ProjectManager.Attributes.IsValueSet(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID) Then
                                        evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                    Else
                                        If node.IsTerminalNode Then
                                            evalDiagonals = PipeParameters.EvaluateDiagonalsAlternatives
                                            If PipeParameters.ForceAllDiagonalsForAlternatives And (n < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                                evalDiagonals = DiagonalsEvaluation.deAll
                                            End If
                                        Else
                                            evalDiagonals = PipeParameters.EvaluateDiagonals
                                            If PipeParameters.ForceAllDiagonals And (n < PipeParameters.ForceAllDiagonalsLimit) Then
                                                evalDiagonals = DiagonalsEvaluation.deAll
                                            End If
                                        End If
                                    End If
                                End If

                                If n > 1 Then
                                    Select Case evalDiagonals
                                        Case DiagonalsEvaluation.deFirst
                                            nodeTotal = (n - 1)
                                        Case DiagonalsEvaluation.deFirstAndSecond
                                            nodeTotal = (n - 1) + (n - 2)
                                        Case DiagonalsEvaluation.deAll
                                            nodeTotal = n * (n - 1) / 2
                                    End Select
                                Else
                                    nodeTotal = 0
                                End If
                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                    nodeTotal *= NodesList.Count
                                End If
                            Case Else
                                nodeTotal = node.Judgments.TotalJudgmentsCountFromUser(UserID, PermissionsUserID, NodesList)
                        End Select

                        totalCount = totalCount + nodeTotal
                    End If
                End If
            Next

            If ProjectManager.IsRiskProject AndAlso (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                For Each alt As clsNode In H.GetUncontributedAlternatives
                    If ProjectManager.UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, UserID) AndAlso Not alt.DisabledForUser(UserID) Then
                        If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                            Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, alt.NodeGuidID)
                            Dim RS As clsRatingScale = CType(alt.MeasurementScale, clsRatingScale)
                            Dim n As Integer = RS.RatingSet.Count
                            If PipeParameters.ForceAllDiagonalsForAlternatives And (n < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                evalDiagonals = DiagonalsEvaluation.deAll
                            End If
                            If n > 1 Then
                                Select Case evalDiagonals
                                    Case DiagonalsEvaluation.deFirst
                                        totalCount += (n - 1)
                                    Case DiagonalsEvaluation.deFirstAndSecond
                                        totalCount += (n - 1) + (n - 2)
                                    Case DiagonalsEvaluation.deAll
                                        totalCount += n * (n - 1) / 2
                                End Select
                            End If
                        Else
                            totalCount += 1
                        End If
                    End If
                Next
            End If

            If ProjectManager.IsRiskProject AndAlso H.HierarchyID = ECHierarchyID.hidLikelihood Then
                For Each e As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                    If ProjectManager.Edges.Edges.ContainsKey(e.NodeGuidID) Then
                        For Each edge As Edge In ProjectManager.Edges.Edges(e.NodeGuidID)
                            Dim ToEvent As clsNode = edge.ToNode
                            If ToEvent IsNot Nothing Then
                                totalCount += 1
                            End If
                        Next
                    End If
                Next
            End If

            If ProjectManager.FeedbackOn Then
                For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                    If Not IsPWMeasurementType(alt.FeedbackMeasureType) Then
                        totalCount += ProjectManager.GetContributedCoveringObjectives(alt, H).Count
                    End If
                Next
            End If

            Return totalCount
        End Function

        Public Function GetDefaultTotalJudgmentsCount(ByVal HierarchyID As Integer) As Integer
            If (ProjectManager Is Nothing) OrElse (PipeParameters Is Nothing) Then Return 0
            If ProjectManager.Hierarchy(HierarchyID).Nodes.Count = 0 Then Return 0

            Dim totalCount = 0

            Dim defaultUserID As Integer = ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID

            For Each node As clsNode In ProjectManager.Hierarchy(HierarchyID).Nodes
                If node.Enabled AndAlso ProjectManager.UsersRoles.IsAllowedObjective(node.NodeGuidID, defaultUserID) AndAlso ((PipeParameters.EvaluateObjectives AndAlso Not node.IsTerminalNode) OrElse (PipeParameters.EvaluateAlternatives AndAlso node.IsTerminalNode)) Then
                    Dim nodeTotal As Integer

                    Dim NodesList As List(Of clsNode) = node.GetNodesBelow(defaultUserID,,,,, False)

                    'If ProjectManager.IsRiskProject AndAlso node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso Not node.IsTerminalNode Then
                    '    NodesList.RemoveAll(Function(n) (n.RiskNodeType = RiskNodeType.ntCategory))
                    'End If

                    Select Case node.MeasureType
                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                            Dim n As Integer
                            Dim isSet As Boolean = False
                            Dim evalDiagonals As DiagonalsEvaluation
                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                    evalDiagonals = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                    isSet = True
                                End If
                                Dim RS As clsRatingScale = CType(node.MeasurementScale, clsRatingScale)
                                n = RS.RatingSet.Count
                            Else
                                n = NodesList.Count
                            End If

                            If Not isSet Then
                                evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                If node.IsTerminalNode Then
                                    If PipeParameters.ForceAllDiagonalsForAlternatives And (n < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If PipeParameters.ForceAllDiagonals And (n < PipeParameters.ForceAllDiagonalsLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                'If ProjectManager.Attributes.IsValueSet(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID) Then
                                '    evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                'Else
                                '    If node.IsTerminalNode Then
                                '        If PipeParameters.ForceAllDiagonalsForAlternatives And (n < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                '            evalDiagonals = DiagonalsEvaluation.deAll
                                '        End If
                                '    Else
                                '        If PipeParameters.ForceAllDiagonals And (n < PipeParameters.ForceAllDiagonalsLimit) Then
                                '            evalDiagonals = DiagonalsEvaluation.deAll
                                '        End If
                                '    End If
                                'End If
                            End If

                            If n > 1 Then
                                Select Case evalDiagonals
                                    Case DiagonalsEvaluation.deFirst
                                        nodeTotal = (n - 1)
                                    Case DiagonalsEvaluation.deFirstAndSecond
                                        nodeTotal = (n - 1) + (n - 2)
                                    Case DiagonalsEvaluation.deAll
                                        nodeTotal = n * (n - 1) / 2
                                End Select
                            Else
                                nodeTotal = 0
                            End If

                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                nodeTotal *= NodesList.Count
                            End If
                        Case Else
                            nodeTotal = node.Judgments.DefaultTotalJudgmentsCount
                    End Select

                    totalCount = totalCount + nodeTotal
                End If
            Next

            If ProjectManager.IsRiskProject And (ProjectManager.Hierarchy(HierarchyID).HierarchyID = ECHierarchyID.hidLikelihood) Then
                For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                    Dim HasContributions As Boolean = MiscFuncs.HasContribution(alt, ProjectManager.Hierarchy(HierarchyID))
                    If Not HasContributions Then
                        If ProjectManager.UsersRoles.IsAllowedAlternative(ProjectManager.Hierarchy(HierarchyID).Nodes(0).NodeGuidID, alt.NodeGuidID, defaultUserID) Then
                            totalCount += 1
                        End If
                    End If
                Next
            End If

            Return totalCount
        End Function

        Public Function HasNodesWithMeasurementType(ByVal MeasurementType As ECMeasureType, Optional ByVal HierarchyID As Integer = -1) As Boolean 'C0101
            If ProjectManager Is Nothing Then Return False
            Dim H As clsHierarchy = If(HierarchyID = -1, ProjectManager.ActiveObjectives, ProjectManager.Hierarchy(HierarchyID))
            Return H.Nodes.Exists(Function(n) (n.MeasureType = MeasurementType))
        End Function

        Private Function HasAdvancedMeasurementType() As Boolean 'C0463
            Return HasNodesWithMeasurementType(ECMeasureType.mtRegularUtilityCurve) Or
                    HasNodesWithMeasurementType(ECMeasureType.mtAdvancedUtilityCurve) Or
                    HasNodesWithMeasurementType(ECMeasureType.mtStep) Or
                    HasNodesWithMeasurementType(ECMeasureType.mtDirect)
        End Function

        Public Function HasAdvancedMeasurementTypeAndKeyPadsAreON() As Boolean 'C0463
            Return HasAdvancedMeasurementType() And PipeParameters.SynchUseVotingBoxes
        End Function

        Public Function HasAdvancedMeasurementTypeAndKeyPadsAreOFF() As Boolean 'C0463
            Return HasAdvancedMeasurementType() And Not PipeParameters.SynchUseVotingBoxes
        End Function

        Private Function RatingScaleHasMoreThan9Intensities() As Boolean 'C0463
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                If RS.RatingSet.Count > 9 Then
                    Return ProjectManager.ActiveObjectives.Nodes.Exists(Function(n) ((n.MeasureType = ECMeasureType.mtRatings) AndAlso (n.MeasurementScale Is RS)))
                End If
            Next
            Return False
        End Function

        Public Function RatingScaleHasMoreThan9IntensitiesAndKeyPadsAreON() As Boolean 'C0463
            Return RatingScaleHasMoreThan9Intensities() And PipeParameters.SynchUseVotingBoxes
        End Function

        Public Function SharedInputSourceIsInUse() As Boolean 'C0463
            Return False
        End Function

        Public Function ProjectOwnerCannotEvaluateAnyObjective(ByVal ProjectOwnerID As Integer) As Boolean
            If Not PipeParameters.EvaluateObjectives Then Return False
            Dim CannotEvaluateAnyObj As Boolean = True
            Dim AtLeastOneObjectiveHasChildrenToEvaluate As Boolean = False 'C0479

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                If Not node.IsTerminalNode Then
                    If node.GetNodesBelow(UNDEFINED_USER_ID).Count > 1 Then AtLeastOneObjectiveHasChildrenToEvaluate = True
                    If node.IsAllowed(ProjectOwnerID) Then CannotEvaluateAnyObj = False
                End If
            Next

            Return CannotEvaluateAnyObj And AtLeastOneObjectiveHasChildrenToEvaluate
        End Function

        Public Function ProjectOwnerCannotEvaluateSomeObjectives(ByVal ProjectOwnerID As Integer) As Boolean
            If Not PipeParameters.EvaluateObjectives Then Return False
            Dim CannotEvaluateSomeObj As Boolean = ProjectManager.ActiveObjectives.Nodes.Exists(Function(n) (Not n.IsTerminalNode AndAlso Not n.IsAllowed(ProjectOwnerID)))
            Return CannotEvaluateSomeObj AndAlso Not ProjectOwnerCannotEvaluateAnyObjective(ProjectOwnerID)
        End Function

        Public Function ProjectOwnerCannotEvaluateAnyAlternative(ByVal ProjectOwnerID As Integer) As Boolean
            If Not PipeParameters.EvaluateAlternatives Then Return False
            If (ProjectManager.ActiveAlternatives.TerminalNodes.Count = 0) Then Return False
            Dim CanEvaluateSomeAlt As Boolean = ProjectManager.ActiveObjectives.TerminalNodes.Exists(Function(n) (n.GetNodesBelow(ProjectOwnerID).Count > 0))
            Return Not CanEvaluateSomeAlt
        End Function

        Public Function ProjectOwnerCannotEvaluateSomeAlternatives(ByVal ProjectOwnerID As Integer) As Boolean
            If PipeParameters.EvaluateAlternatives Then Return False
            Dim CannotEvaluateSomeAlt As Boolean = ProjectManager.ActiveObjectives.TerminalNodes.Exists(Function(n) ((n.ChildrenAlts.Count <> 0) AndAlso (n.GetNodesBelow(ProjectOwnerID).Count = 0)))
            Return CannotEvaluateSomeAlt AndAlso Not ProjectOwnerCannotEvaluateAnyAlternative(ProjectOwnerID)
        End Function

        Public Function GetNodeIndexByID(ByVal NodesList As List(Of clsNode), ByVal NodeID As Integer) As Integer
            Return If(NodesList Is Nothing, -1, NodesList.FindIndex(Function(n) (n.NodeID = NodeID)))
        End Function

        Public Sub New(Optional ProjectManager As clsProjectManager = Nothing)
            PipeParameters = New clsPipeParamaters
            Me.ProjectManager = ProjectManager
        End Sub
    End Class
End Namespace
