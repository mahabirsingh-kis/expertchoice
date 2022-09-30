'Option Strict On

Imports ECCore.MathFuncs
Imports ECCore
Imports System.Linq

Namespace ECCore

    <Serializable()> Public Class clsWeight
        Public Property ChildNodeID() As Integer
        Public Property Value() As Single
        Public Property UnnormValue() As Single

        Public Sub New()
        End Sub

        Public Sub New(ByVal ChildNodeID As Integer, ByVal Value As Single)
            Me.ChildNodeID = ChildNodeID
            Me.Value = Value
        End Sub
    End Class

    <Serializable()> Public Class clsUserWeights 'C0159
        Public Timestamp As DateTime
        Public ReadOnly Property UserID() As Integer
        Public Property SynthesisMode() As ECSynthesisMode
        Public Property IncludeIdealAlternative() As Boolean
        Public Property Values() As New Dictionary(Of Integer, clsWeight)
        Public Property UnnormalizedValues() As New Dictionary(Of Integer, clsWeight)
        Public Property NormalizedValues() As New Dictionary(Of Integer, clsWeight)

        Public Function GetWeightValueByNodeID(ByVal NodeID As Integer) As Single
            Dim weight As clsWeight = If(Values.ContainsKey(NodeID), Values(NodeID), Nothing)
            'Dim weight As clsWeight = Values.FirstOrDefault(Function(w) (w.ChildNodeID = NodeID))
            Return If(weight IsNot Nothing, weight.Value, 0)
        End Function

        Public Function GetUnnormalizedWeightValueByNodeID(ByVal NodeID As Integer) As Single
            Dim weight As clsWeight = If(UnnormalizedValues.ContainsKey(NodeID), UnnormalizedValues(NodeID), Nothing)
            'Dim weight As clsWeight = UnnormalizedValues.FirstOrDefault(Function(w) (w.ChildNodeID = NodeID))
            Return If(weight IsNot Nothing, weight.Value, 0)
        End Function

        Public Function GetNormalizedWeightValueByNodeID(ByVal NodeID As Integer) As Single
            Dim weight As clsWeight = If(NormalizedValues.ContainsKey(NodeID), NormalizedValues(NodeID), Nothing)
            'Dim weight As clsWeight = NormalizedValues.FirstOrDefault(Function(w) (w.ChildNodeID = NodeID))
            Return If(weight IsNot Nothing, weight.Value, 0)
        End Function

        Public Sub New(ByVal UserID As Integer, Optional ByVal SynthesisMode As ECSynthesisMode = ECSynthesisMode.smDistributive, Optional ByVal IncludeIdealAlternative As Boolean = False) 'C0338
            Timestamp = VERY_OLD_DATE
            Me.UserID = UserID
            Me.SynthesisMode = SynthesisMode
            Me.IncludeIdealAlternative = IncludeIdealAlternative
        End Sub
    End Class

    <Serializable()> Public Class clsWeights
        Private UserWeightsList As New List(Of clsUserWeights)

        Public ReadOnly Property Node() As clsNode

        Public Sub ClearUserWeights(Optional ByVal UserID As Integer = Integer.MinValue)
            If UserID = Integer.MinValue Then
                UserWeightsList.Clear()
            Else
                UserWeightsList.RemoveAll(Function(uw) (uw.UserID = UserID))
            End If
        End Sub

        Public Function GetUserWeightsTime(ByVal UserID As Integer) As DateTime
            Dim uw As clsUserWeights = UserWeightsList.FirstOrDefault(Function(w) (w.UserID = UserID))
            Return If(uw Is Nothing, uw.Timestamp, VERY_OLD_DATE)
        End Function

        Public Function GetUserWeights(ByVal UserID As Integer, ByVal SynthesisMode As ECSynthesisMode, ByVal IncludeIdealAlternative As Boolean) As clsUserWeights 'C0955
            Dim isRisk As Boolean = Node.Hierarchy.ProjectManager.IsRiskProject

            Dim UW As clsUserWeights
            Dim defaultUW As clsUserWeights = Nothing

            'trying to find in existing list of weights
            For Each UW In UserWeightsList
                If (UW.UserID = UserID) And (UW.SynthesisMode = ECSynthesisMode.smNone) Then
                    defaultUW = UW
                End If
                If (UW.UserID = UserID) And (UW.SynthesisMode = SynthesisMode) And (UW.IncludeIdealAlternative = IncludeIdealAlternative) Then
                    Return UW
                End If
            Next

            If defaultUW Is Nothing Then
                defaultUW = New clsUserWeights(UserID)
                defaultUW.SynthesisMode = ECSynthesisMode.smNone
                defaultUW.IncludeIdealAlternative = False
                UserWeightsList.Add(defaultUW)
                Return defaultUW
            End If

            ' if there's no such user weights yet then create it
            UW = New clsUserWeights(UserID)

            Dim sumOfDefault As Single = 0
            Dim maxOfDefault As Single = 0

            If isRisk And Node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                For Each w As clsWeight In defaultUW.UnnormalizedValues.Values
                    sumOfDefault += w.Value
                    If w.Value > maxOfDefault Then
                        maxOfDefault = w.Value
                    End If
                Next
            Else
                For Each w As clsWeight In defaultUW.Values.Values
                    sumOfDefault += w.Value
                    If w.Value > maxOfDefault Then
                        maxOfDefault = w.Value
                    End If
                Next
            End If

            If IsPWMeasurementType(Node.MeasureType) Then
                'maxOfDefault = sumOfDefault
            End If

            If isRisk And Node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                Dim d As Single = If(Node.ParentNode Is Nothing OrElse Node.MeasureType = ECMeasureType.mtPWAnalogous, 1, If(sumOfDefault <= 1, 1, sumOfDefault))
                d = 1
                For Each w As clsWeight In defaultUW.Values.Values
                    UW.Values.Add(w.ChildNodeID, New clsWeight(w.ChildNodeID, w.Value / d))
                Next
                For Each w As clsWeight In defaultUW.UnnormalizedValues.Values
                    UW.UnnormalizedValues.Add(w.ChildNodeID, New clsWeight(w.ChildNodeID, w.Value))
                Next
                For Each w As clsWeight In defaultUW.NormalizedValues.Values
                    UW.NormalizedValues.Add(w.ChildNodeID, New clsWeight(w.ChildNodeID, w.Value / If(sumOfDefault <= 1, 1, sumOfDefault)))
                Next

                UW.Timestamp = UW.Timestamp
                UW.SynthesisMode = SynthesisMode
                UW.IncludeIdealAlternative = IncludeIdealAlternative
            Else
                Dim d As Single = 1
                Dim dUnnorm As Single = 1
                If IncludeIdealAlternative Then
                    If SynthesisMode = ECSynthesisMode.smDistributive Then
                        If Node.IsTerminalNode Then
                            If IsPWMeasurementType(Node.MeasureType) Then
                                d = maxOfDefault
                                dUnnorm = sumOfDefault + maxOfDefault
                            Else
                                d = sumOfDefault + 1
                                dUnnorm = sumOfDefault + 1
                            End If
                        Else
                            d = sumOfDefault
                            dUnnorm = sumOfDefault
                        End If
                    Else
                        If Node.IsTerminalNode Then
                            If IsPWMeasurementType(Node.MeasureType) Then
                                d = maxOfDefault
                                dUnnorm = maxOfDefault
                            Else
                                d = 1
                                dUnnorm = 1
                            End If
                        Else
                            d = sumOfDefault
                            dUnnorm = sumOfDefault
                        End If
                    End If
                Else
                    If SynthesisMode = ECSynthesisMode.smDistributive Then
                        d = sumOfDefault
                        dUnnorm = sumOfDefault
                    Else
                        If Node.IsTerminalNode Then
                            d = maxOfDefault
                            If IsPWMeasurementType(Node.MeasureType) Then
                                dUnnorm = maxOfDefault
                            Else
                                dUnnorm = 1
                            End If
                        Else
                            d = sumOfDefault
                            dUnnorm = sumOfDefault
                        End If
                    End If
                End If

                If d = 0 Then
                    d = 1
                End If
                If dUnnorm = 0 Then
                    dUnnorm = 1
                End If

                For Each w As clsWeight In defaultUW.Values.Values
                    UW.Values.Add(w.ChildNodeID, New clsWeight(w.ChildNodeID, w.Value / d))
                Next
                For Each w As clsWeight In defaultUW.UnnormalizedValues.Values
                    UW.UnnormalizedValues.Add(w.ChildNodeID, New clsWeight(w.ChildNodeID, w.Value / dUnnorm))
                Next
                For Each w As clsWeight In defaultUW.Values.Values
                    'UW.NormalizedValues.Add(New clsWeight(w.ChildNodeID, w.Value / d))
                    UW.NormalizedValues.Add(w.ChildNodeID, New clsWeight(w.ChildNodeID, w.Value / sumOfDefault))
                Next
            End If

            UW.Timestamp = UW.Timestamp
            UW.SynthesisMode = SynthesisMode
            UW.IncludeIdealAlternative = IncludeIdealAlternative

            UserWeightsList.Add(UW)
            Return UW
        End Function

        Public Function UserWeightsExist(ByVal UserID As Integer) As Boolean
            Return UserWeightsList.Exists(Function(uw) ((uw.UserID = UserID) AndAlso (uw.Timestamp <> VERY_OLD_DATE)))
        End Function

        Public Sub New(ByVal Node As clsNode)
            Me.Node = Node
        End Sub
    End Class

    <Serializable()> Public MustInherit Class clsCustomJudgments
        Public ReadOnly Property Node() As clsNode

        Private mWeights As clsWeights
        Public Property Weights() As clsWeights
            Get
                Return mWeights
            End Get
            Set(ByVal value As clsWeights)
                mWeights = value
            End Set
        End Property

        Private mUsersJudgments As New Dictionary(Of Integer, List(Of clsCustomMeasureData))

        Private mProjectManager As clsProjectManager
        Public Property ProjectManager As clsProjectManager
            Get
                If mProjectManager IsNot Nothing Then
                    Return mProjectManager
                Else
                    If Node IsNot Nothing AndAlso Node.Hierarchy IsNot Nothing Then
                        Return Node.Hierarchy.ProjectManager
                    Else
                        Return Nothing
                    End If
                End If
            End Get
            Set(value As clsProjectManager)
                mProjectManager = value
            End Set
        End Property

        Public Property UsersJudgments(ByVal UserID As Integer) As List(Of clsCustomMeasureData)
            Get
                If Not mUsersJudgments.ContainsKey(UserID) Then
                    mUsersJudgments.Add(UserID, New List(Of clsCustomMeasureData))
                End If

                Return mUsersJudgments(UserID)
            End Get
            Set(ByVal value As List(Of clsCustomMeasureData))
                If mUsersJudgments.ContainsKey(UserID) Then
                    mUsersJudgments(UserID) = value
                End If
            End Set
        End Property

        Public Sub ClearCombinedJudgments(Optional ByVal CombinedGroup As clsCombinedGroup = Nothing)
            If CombinedGroup IsNot Nothing Then
                DeleteJudgmentsFromUser(CombinedGroup.CombinedUserID)
            Else
                For Each cg As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                    DeleteJudgmentsFromUser(cg.CombinedUserID)
                Next
            End If
        End Sub

        Protected MustOverride Function CalculateWeightsUsingCombinedForRestrictedNodes(ByVal CalculationTarget As clsCalculationTarget) As Boolean 'C0785
        Public MustOverride Function CalculateWeights(ByVal CalculationTarget As clsCalculationTarget, Optional RS As clsRatingScale = Nothing) As Boolean 'C0159

        Public ReadOnly Property JudgmentsFromUser(ByVal UserID As Integer, Optional OutcomesNodeID As Integer = UNDEFINED_INTEGER_VALUE) As List(Of clsCustomMeasureData)
            Get
                If UserID >= 0 Then
                    If Not ProjectManager.UserExists(UserID) Then Return Nothing
                Else
                    If IsCombinedUserID(UserID) AndAlso Not ProjectManager.CombinedGroups.CombinedGroupUserIDExists(UserID) Then Return Nothing
                End If

                If OutcomesNodeID <> UNDEFINED_INTEGER_VALUE And (TypeOf (Me) Is clsPairwiseJudgments) Then
                    Dim res As New List(Of clsCustomMeasureData)
                    For Each J As clsPairwiseMeasureData In UsersJudgments(UserID)
                        If J.OutcomesNodeID = OutcomesNodeID Then res.Add(J)
                    Next
                    Return res
                Else
                    Return UsersJudgments(UserID)
                End If
            End Get
        End Property

        Public ReadOnly Property JudgmentsFromUsers(ByVal UsersIDList As ArrayList) As List(Of clsCustomMeasureData)
            Get
                If UsersIDList Is Nothing Then Return Nothing
                Dim res As New List(Of clsCustomMeasureData)
                For Each userID As Integer In UsersIDList
                    For Each J As clsCustomMeasureData In JudgmentsFromUser(userID)
                        res.Add(J)
                    Next
                Next
                Return res
            End Get
        End Property

        Public ReadOnly Property JudgmentsFromAllUsers() As List(Of clsCustomMeasureData)
            Get
                Dim res As New List(Of clsCustomMeasureData)
                For Each user As clsUser In ProjectManager.UsersList
                    For Each J As clsCustomMeasureData In JudgmentsFromUser(user.UserID)
                        res.Add(J)
                    Next
                Next
                Return res
            End Get
        End Property


        Public ReadOnly Property DefaultTotalJudgmentsCount() As Integer
            Get
                Dim count As Integer = 0

                Dim nodesBelow As List(Of clsNode) = GetNodesBelow(UNDEFINED_USER_ID)
                Dim n As Integer = nodesBelow.Count

                Select Case Node.MeasureType
                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                        count = n * (n - 1) / 2
                    Case ECMeasureType.mtPWAnalogous
                        Dim rcount As Integer = CType(Node.MeasurementScale, clsRatingScale).RatingSet.Count
                        count = n * (rcount * (rcount - 1) / 2)
                    Case Else
                        count = n
                End Select

                Return count
            End Get
        End Property

        Public ReadOnly Property TotalJudgmentsCountFromUser(ByVal UserID As Integer, Optional ByVal PermissionsUserID As Integer = UNDEFINED_USER_ID, Optional Nodes As List(Of clsNode) = Nothing) As Integer
            Get
                If PermissionsUserID = UNDEFINED_USER_ID Then PermissionsUserID = UserID

                Dim count As Integer = 0

                Dim allowedNodesBelow As List(Of clsNode)
                If Nodes Is Nothing Then
                    allowedNodesBelow = GetNodesBelow(PermissionsUserID)
                Else
                    allowedNodesBelow = Nodes
                End If

                If Node.MeasureType = ECMeasureType.mtPairwise Then
                    Dim n As Integer = allowedNodesBelow.Count
                    count = n * (n - 1) / 2
                Else
                    count = allowedNodesBelow.Count
                End If

                Return count
            End Get
        End Property

        Public Sub DeleteJudgmentsFromUser(ByVal UserID As Integer)
            UsersJudgments(UserID).Clear()
        End Sub

        Public Sub DeleteJudgmentsFromAllUsers()
            mUsersJudgments.Clear()
        End Sub

        Public MustOverride Sub AddMeasureData(ByVal data As clsCustomMeasureData, Optional ByVal ForceAdd As Boolean = False) 'C0260 'C0313

        Public ReadOnly Property UserWeights(ByVal UserID As Integer, ByVal ChildNodeID As Integer, ByVal SynthesisMode As ECSynthesisMode, ByVal IncludeIdealAlternative As Boolean) As Single
            Get
                Dim d As Dictionary(Of Integer, clsWeight) = Weights.GetUserWeights(UserID, SynthesisMode, IncludeIdealAlternative).Values
                Dim W As clsWeight = If(d.ContainsKey(ChildNodeID), d(ChildNodeID), Nothing)
                'Dim W As clsWeight = Weights.GetUserWeights(UserID, SynthesisMode, IncludeIdealAlternative).Values.FirstOrDefault(Function(uw) (uw.ChildNodeID = ChildNodeID))
                Return If(W IsNot Nothing, W.Value, 0)
            End Get
        End Property

        Public Function UserWeightsExist(ByVal UserID As Integer) As Boolean
            Return Weights.UserWeightsExist(UserID)
        End Function

        Public MustOverride Function GetWeight(ByVal UserID As Integer, ByVal ChildNodeID As Integer, Optional NodesBelow As HashSet(Of Integer) = Nothing) As Single

        Protected Function GetNodesBelow(ByVal UserID As Integer) As List(Of clsNode)
            Dim res As List(Of clsNode) = Node.GetNodesBelow(UserID,,,,, False)
            'If Node.Hierarchy.ProjectManager.IsRiskProject AndAlso Node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
            '    res.RemoveAll(Function(n) (n.RiskNodeType = RiskNodeType.ntCategory))
            'End If
            Return res
        End Function

        'Protected Function GetNodesBelowHS(ByVal UserID As Integer) As HashSet(Of clsNode)
        '    Dim res As HashSet(Of clsNode) = Node.GetNodesBelowHS(UserID, False)
        '    If Node.Hierarchy.ProjectManager.IsRiskProject AndAlso Node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
        '        res.RemoveAll(Function(n) (n.RiskNodeType = RiskNodeType.ntCategory))
        '    End If
        '    Return res
        'End Function

        Public Sub New(ByVal node As clsNode, Optional ProjectManager As clsProjectManager = Nothing)
            Me.Node = node
            mWeights = New clsWeights(node)
            Me.ProjectManager = ProjectManager
        End Sub
    End Class

    <Serializable()> Public Class clsPairwiseJudgments
        Inherits clsCustomJudgments
        Public ReadOnly Property EigenCalcs() As New clsEigenCalcs
        Private mMatrix As Double(,)

        Private Function GetPairIndex(ByVal pair As clsPairwiseMeasureData) As Integer
            Dim i As Integer
            Dim tmpPair As clsPairwiseMeasureData

            For i = 0 To JudgmentsFromUser(pair.UserID).Count - 1
                tmpPair = CType(JudgmentsFromUser(pair.UserID).Item(i), clsPairwiseMeasureData)
                If (tmpPair.ParentNodeID = pair.ParentNodeID) AndAlso (tmpPair.OutcomesNodeID = pair.OutcomesNodeID) And ((tmpPair.FirstNodeID = pair.FirstNodeID) And (tmpPair.SecondNodeID = pair.SecondNodeID) Or
                    (tmpPair.SecondNodeID = pair.FirstNodeID) And (tmpPair.FirstNodeID = pair.SecondNodeID)) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Private Sub InitMatrix(ByVal n As Integer)
            Dim i As Integer
            Dim j As Integer
            For i = 0 To n - 1
                For j = 0 To n - 1
                    mMatrix(i, j) = If(i = j, 1, 0)
                Next
            Next
        End Sub

        Private Function GetNodeIndexByID(ByVal NodesList As List(Of clsNode), ByVal NodeID As Integer) As Integer
            If NodesList Is Nothing Then Return -1
            Return NodesList.FindIndex(Function(n) (n.NodeID = NodeID))
        End Function

        Private Function GetRatingIndexByID(ByVal RatingScale As clsRatingScale, ByVal RatingID As Integer) As Integer
            If RatingScale Is Nothing Then Return -1
            Return RatingScale.RatingSet.FindIndex(Function(r) (r.ID = RatingID))
        End Function

        ' returns true if at least one judgment was entered
        Private Function FillMatrixWithPairwiseData(ByVal UserID As Integer, Optional M As Double(,) = Nothing) As Boolean
            Dim nodesBelow As List(Of clsNode) = GetNodesBelow(UserID)
            Dim Matrix As Double(,) = If(M Is Nothing, mMatrix, M)

            Dim res As Boolean = False

            For Each pair As clsPairwiseMeasureData In JudgmentsFromUser(UserID)
                If Not pair.IsUndefined AndAlso pair.Value <> 0 Then
                    Dim pairNode1 As clsNode
                    Dim pairNode2 As clsNode
                    If Node.IsTerminalNode Then
                        pairNode1 = Node.Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(pair.FirstNodeID)
                        pairNode2 = Node.Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(pair.SecondNodeID)
                    Else
                        pairNode1 = Node.Hierarchy.GetNodeByID(pair.FirstNodeID)
                        pairNode2 = Node.Hierarchy.GetNodeByID(pair.SecondNodeID)
                    End If

                    If nodesBelow.Contains(pairNode1) AndAlso nodesBelow.Contains(pairNode2) Then
                        If pair.Advantage > 0 Then
                            Matrix(GetNodeIndexByID(nodesBelow, pair.FirstNodeID), GetNodeIndexByID(nodesBelow, pair.SecondNodeID)) = pair.Value
                            Matrix(GetNodeIndexByID(nodesBelow, pair.SecondNodeID), GetNodeIndexByID(nodesBelow, pair.FirstNodeID)) = 1 / pair.Value
                        Else
                            Matrix(GetNodeIndexByID(nodesBelow, pair.SecondNodeID), GetNodeIndexByID(nodesBelow, pair.FirstNodeID)) = pair.Value
                            Matrix(GetNodeIndexByID(nodesBelow, pair.FirstNodeID), GetNodeIndexByID(nodesBelow, pair.SecondNodeID)) = 1 / pair.Value
                        End If
                        res = True
                    End If
                End If
            Next
            Return res
        End Function

        Public Function GetPairwiseMatrix(ByVal UserID As Integer, ByRef Size As Integer) As Double(,)
            Dim nodesBelow As List(Of clsNode) = GetNodesBelow(UserID)
            Dim Matrix As Double(,)
            Size = nodesBelow.Count
            ReDim Matrix(Size - 1, Size - 1)

            For i As Integer = 0 To Size - 1
                For j As Integer = 0 To Size - 1
                    Matrix(i, j) = If(i = j, 1, 0)
                Next
            Next

            For Each pair As clsPairwiseMeasureData In JudgmentsFromUser(UserID)
                If Not pair.IsUndefined AndAlso pair.Value <> 0 Then
                    Dim pairNode1 As clsNode
                    Dim pairNode2 As clsNode
                    If Node.IsTerminalNode Then
                        pairNode1 = Node.Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(pair.FirstNodeID)
                        pairNode2 = Node.Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(pair.SecondNodeID)
                    Else
                        pairNode1 = Node.Hierarchy.GetNodeByID(pair.FirstNodeID)
                        pairNode2 = Node.Hierarchy.GetNodeByID(pair.SecondNodeID)
                    End If

                    If nodesBelow.Contains(pairNode1) AndAlso nodesBelow.Contains(pairNode2) Then
                        If pair.Advantage > 0 Then
                            Matrix(GetNodeIndexByID(nodesBelow, pair.FirstNodeID), GetNodeIndexByID(nodesBelow, pair.SecondNodeID)) = pair.Value
                            Matrix(GetNodeIndexByID(nodesBelow, pair.SecondNodeID), GetNodeIndexByID(nodesBelow, pair.FirstNodeID)) = 1 / pair.Value
                        Else
                            Matrix(GetNodeIndexByID(nodesBelow, pair.SecondNodeID), GetNodeIndexByID(nodesBelow, pair.FirstNodeID)) = pair.Value
                            Matrix(GetNodeIndexByID(nodesBelow, pair.FirstNodeID), GetNodeIndexByID(nodesBelow, pair.SecondNodeID)) = 1 / pair.Value
                        End If
                    End If
                End If
            Next
            Return Matrix
        End Function

        Public Sub DiagonalsEvaluated(UserID As Integer, ByRef First As Boolean, ByRef Second As Boolean, ByRef All As Boolean)
            Dim size As Integer
            Dim matrix As Double(,) = GetPairwiseMatrix(UserID, size)
            First = True
            Second = True
            All = True
            For i As Integer = 0 To size - 2
                If matrix(i, i + 1) = 0 Then
                    First = False
                End If
                If i < size - 2 Then
                    If matrix(i, i + 2) = 0 Then
                        Second = False
                    End If
                End If
            Next
            For i As Integer = 0 To size - 1
                For j As Integer = 0 To size - 1
                    If matrix(i, j) = 0 Then
                        All = False
                    End If
                Next
            Next
        End Sub

        Public Function OnlyMainDiagonalEvaluated(ByVal UserID As Integer) As Boolean
            Dim nodesBelow As List(Of clsNode) = GetNodesBelow(UserID)
            Dim hasJudgments As Boolean = False
            For Each pair As clsPairwiseMeasureData In JudgmentsFromUser(UserID)
                If Not pair.IsUndefined AndAlso pair.Value <> 0 Then
                    hasJudgments = True
                    Dim H As clsHierarchy = If(Node.IsTerminalNode, Node.Hierarchy.ProjectManager.ActiveAlternatives, Node.Hierarchy)
                    Dim pairNode1 As clsNode = H.GetNodeByID(pair.FirstNodeID)
                    Dim pairNode2 As clsNode = H.GetNodeByID(pair.SecondNodeID)

                    If nodesBelow.Contains(pairNode1) AndAlso nodesBelow.Contains(pairNode2) Then
                        If Math.Abs(GetNodeIndexByID(nodesBelow, pair.FirstNodeID) - GetNodeIndexByID(nodesBelow, pair.SecondNodeID)) <> 1 Then
                            Return False
                        End If
                    End If
                End If
            Next
            Return hasJudgments
        End Function

        ' returns true is at least one judgment was entered
        Private Function FillMatrixWithPairwiseDataOutcomes(ByVal UserID As Integer, ByRef M As Double(,), RS As clsRatingScale) As Boolean
            Dim res As Boolean = False
            For Each pair As clsPairwiseMeasureData In JudgmentsFromUser(UserID)
                If Not pair.IsUndefined Then
                    If RS.GetRatingByID(pair.FirstNodeID) IsNot Nothing AndAlso RS.GetRatingByID(pair.SecondNodeID) IsNot Nothing Then
                        If pair.Advantage > 0 Then
                            M(GetRatingIndexByID(RS, pair.FirstNodeID), GetRatingIndexByID(RS, pair.SecondNodeID)) = pair.Value
                            M(GetRatingIndexByID(RS, pair.SecondNodeID), GetRatingIndexByID(RS, pair.FirstNodeID)) = 1 / pair.Value
                        Else
                            M(GetRatingIndexByID(RS, pair.SecondNodeID), GetRatingIndexByID(RS, pair.FirstNodeID)) = pair.Value
                            M(GetRatingIndexByID(RS, pair.FirstNodeID), GetRatingIndexByID(RS, pair.SecondNodeID)) = 1 / pair.Value
                        End If
                        res = True
                    End If
                End If
            Next
            Return res
        End Function

        Private Function IsEmptyMatrix(ByVal n As Integer) As Boolean
            Dim res As Boolean = True
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    If (i <> j) AndAlso (mMatrix(i, j) <> 0) Then
                        res = False
                    End If
                Next
            Next
            Return res
        End Function

        ' returns true is at least one judgment was entered
        Private Function FillMatrixWithPairwiseDataUsingCombinedForRestricted(ByVal UserID As Integer, Optional M As Double(,) = Nothing) As Boolean
            Dim res As Boolean = False
            Dim AllNodesBelow As List(Of clsNode) = GetNodesBelow(UNDEFINED_USER_ID)
            Dim Matrix As Double(,) = If(M Is Nothing, mMatrix, M)
            Dim combinedUserID As Integer = Node.Hierarchy.ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID

            Dim ind1 As Integer
            Dim ind2 As Integer
            Dim pairNode1 As clsNode
            Dim pairNode2 As clsNode

            For Each pair As clsPairwiseMeasureData In JudgmentsFromUser(combinedUserID)
                If Not pair.IsUndefined AndAlso pair.Value <> 0 Then
                    If Node.IsTerminalNode Then
                        pairNode1 = Node.Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(pair.FirstNodeID)
                        pairNode2 = Node.Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(pair.SecondNodeID)
                    Else
                        pairNode1 = Node.Hierarchy.GetNodeByID(pair.FirstNodeID)
                        pairNode2 = Node.Hierarchy.GetNodeByID(pair.SecondNodeID)
                    End If

                    If AllNodesBelow.Contains(pairNode1) AndAlso AllNodesBelow.Contains(pairNode2) Then
                        ind1 = GetNodeIndexByID(AllNodesBelow, pair.FirstNodeID)
                        ind2 = GetNodeIndexByID(AllNodesBelow, pair.SecondNodeID)

                        If ind1 >= 0 AndAlso ind2 >= 0 Then
                            If pair.Advantage > 0 Then
                                Matrix(ind1, ind2) = pair.Value
                                Matrix(ind2, ind1) = 1 / pair.Value
                            Else
                                Matrix(ind2, ind1) = pair.Value
                                Matrix(ind1, ind2) = 1 / pair.Value
                            End If
                        End If
                        res = True
                    End If
                End If
            Next

            If Node.IsAllowed(UserID) Then
                For Each pair As clsPairwiseMeasureData In JudgmentsFromUser(UserID)
                    If Not pair.IsUndefined AndAlso pair.Value <> 0 Then
                        If Node.IsTerminalNode Then
                            pairNode1 = Node.Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(pair.FirstNodeID)
                            pairNode2 = Node.Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(pair.SecondNodeID)
                        Else
                            pairNode1 = Node.Hierarchy.GetNodeByID(pair.FirstNodeID)
                            pairNode2 = Node.Hierarchy.GetNodeByID(pair.SecondNodeID)
                        End If
                        If Not Node.IsTerminalNode OrElse
                            (Node.Hierarchy.ProjectManager.UsersRoles.IsAllowedAlternative(Node.NodeGuidID, pairNode1.NodeGuidID, UserID) AndAlso
                             Node.Hierarchy.ProjectManager.UsersRoles.IsAllowedAlternative(Node.NodeGuidID, pairNode2.NodeGuidID, UserID)) Then

                            ind1 = GetNodeIndexByID(AllNodesBelow, pair.FirstNodeID)
                            ind2 = GetNodeIndexByID(AllNodesBelow, pair.SecondNodeID)

                            If ind1 >= 0 AndAlso ind2 >= 0 Then
                                If pair.Advantage > 0 Then
                                    Matrix(ind1, ind2) = pair.Value
                                    Matrix(ind2, ind1) = 1 / pair.Value
                                Else
                                    Matrix(ind2, ind1) = pair.Value
                                    Matrix(ind1, ind2) = 1 / pair.Value
                                End If
                            End If
                            res = True
                        End If
                    End If
                Next
            End If
            Return res
        End Function

        Protected Overrides Function CalculateWeightsUsingCombinedForRestrictedNodes(ByVal CalculationTarget As clsCalculationTarget) As Boolean
            Dim userID As Integer = CalculationTarget.GetUserID
            Dim AllNodesBelow As List(Of clsNode) = GetNodesBelow(UNDEFINED_USER_ID)
            Dim n As Integer = AllNodesBelow.Count
            If n = 0 Then Return True

            Weights.ClearUserWeights(userID)

            Dim UW As clsUserWeights = Weights.GetUserWeights(userID, ECSynthesisMode.smNone, False)
            Dim W As Dictionary(Of Integer, clsWeight) = UW.Values
            Dim WUnnorm As Dictionary(Of Integer, clsWeight) = UW.UnnormalizedValues

            If n = 1 Then
                W.Add(AllNodesBelow(0).NodeID, New clsWeight(AllNodesBelow(0).NodeID, 1))
                WUnnorm.Add(AllNodesBelow(0).NodeID, New clsWeight(AllNodesBelow(0).NodeID, 1))
                Return True
            End If

            ReDim mMatrix(n - 1, n - 1)
            InitMatrix(n)

            Dim filledSomething As Boolean = FillMatrixWithPairwiseDataUsingCombinedForRestricted(userID)

            'If IsEmptyMatrix(n) Or Not HasSpanningSet(mMatrix, n) Then
            If Not filledSomething OrElse Not HasSpanningSet(mMatrix, n) Then
                For i As Integer = 0 To n - 1
                    W.Add(AllNodesBelow(i).NodeID, New clsWeight(AllNodesBelow(i).NodeID, 0))
                    WUnnorm.Add(AllNodesBelow(i).NodeID, New clsWeight(AllNodesBelow(i).NodeID, 0))
                Next
            Else
                EigenCalcs.SetMatrix(mMatrix, n)
                EigenCalcs.Calculate()

                For i As Integer = 0 To n - 1
                    W.Add(AllNodesBelow(i).NodeID, New clsWeight(AllNodesBelow(i).NodeID, EigenCalcs.EigenVector(i)))
                    WUnnorm.Add(AllNodesBelow(i).NodeID, New clsWeight(AllNodesBelow(i).NodeID, EigenCalcs.EigenVector(i)))
                Next
            End If

            If Node.MeasureType = ECMeasureType.mtPWAnalogous Then
                Dim attr As clsAttribute = ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_KNOWN_VALUE_ID)
                If attr IsNot Nothing Then
                    Dim KnownValue As Double = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE
                    Dim childID As Integer = -1
                    Dim i As Integer = 0
                    While (childID = -1) And (i < AllNodesBelow.Count)
                        Dim value As Double = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_KNOWN_VALUE_ID, AllNodesBelow(i).NodeGuidID)
                        If value <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                            KnownValue = value
                            childID = i
                        End If
                        i += 1
                    End While
                    If KnownValue <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                        Dim MasterPriority As Double = W(childID).Value
                        Dim uMasterPriority As Double = WUnnorm(childID).Value

                        For Each chID As Integer In W.Keys
                            If MasterPriority <> 0 Then
                                W(i).Value = CSng(KnownValue * W(chID).Value / MasterPriority)
                            End If
                            If uMasterPriority <> 0 Then
                                WUnnorm(i).Value = CSng(KnownValue * WUnnorm(chID).Value / uMasterPriority)
                            End If
                        Next
                    End If
                End If
            End If

            UW.Timestamp = Now

            Return True
        End Function

        Private Function CalculatePWOutcomes(ByVal CalculationTarget As clsCalculationTarget, RS As clsRatingScale) As Boolean
            Dim userID As Integer = CalculationTarget.GetUserID

            If Not IsCombinedUserID(userID) And Not Node.IsAllowed(userID) Then
                Weights.ClearUserWeights(userID)
                Return True
            End If

            Dim nodesbelow As List(Of clsNode)
            Dim nodeId As Integer
            If Node.IsAlternative AndAlso Node.Hierarchy.ProjectManager.ActiveObjectives.GetUncontributedAlternatives.Contains(Node) Then
                nodesbelow = New List(Of clsNode)
                nodesbelow.Add(Node)
                nodeId = UNDEFINED_USER_ID
            Else
                nodesbelow = GetNodesBelow(CalculationTarget.GetUserID)
                nodeId = Node.NodeID
            End If

            Weights.ClearUserWeights(userID)

            Dim UW As clsUserWeights = Weights.GetUserWeights(userID, ECSynthesisMode.smNone, False)
            Dim W As Dictionary(Of Integer, clsWeight) = UW.Values
            Dim WUnnorm As Dictionary(Of Integer, clsWeight) = UW.UnnormalizedValues

            Dim outcomes As List(Of clsRating) = RS.RatingSet

            If outcomes.Count = 1 Then
                W.Add(nodesbelow(0).NodeID, New clsWeight(nodesbelow(0).NodeID, 1))
                WUnnorm.Add(nodesbelow(0).NodeID, New clsWeight(nodesbelow(0).NodeID, 1))
                Return True
            End If

            Dim sum As Single = 0
            For Each child As clsNode In nodesbelow
                child.PWOutcomesJudgments.Weights.ClearUserWeights(userID)
                child.Hierarchy = Node.Hierarchy                

                Dim childUW As clsUserWeights = child.PWOutcomesJudgments.Weights.GetUserWeights(userID, ECSynthesisMode.smNone, False)
                Dim childW As Dictionary(Of Integer, clsWeight) = childUW.Values
                Dim childWUnnorm As Dictionary(Of Integer, clsWeight) = childUW.UnnormalizedValues

                Dim Matrix As Double(,)
                ReDim Matrix(outcomes.Count - 1, outcomes.Count - 1)

                For i As Integer = 0 To outcomes.Count - 1
                    For j As Integer = 0 To outcomes.Count - 1
                        Matrix(i, j) = If(i = j, 1, 0)
                    Next
                Next

                For Each pair As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(userID, nodeId)
                    If Not pair.IsUndefined Then
                        If RS.GetRatingByID(pair.FirstNodeID) IsNot Nothing AndAlso RS.GetRatingByID(pair.SecondNodeID) IsNot Nothing Then
                            If pair.Advantage > 0 Then
                                Matrix(GetRatingIndexByID(RS, pair.FirstNodeID), GetRatingIndexByID(RS, pair.SecondNodeID)) = pair.Value
                                Matrix(GetRatingIndexByID(RS, pair.SecondNodeID), GetRatingIndexByID(RS, pair.FirstNodeID)) = 1 / pair.Value
                            Else
                                Matrix(GetRatingIndexByID(RS, pair.SecondNodeID), GetRatingIndexByID(RS, pair.FirstNodeID)) = pair.Value
                                Matrix(GetRatingIndexByID(RS, pair.FirstNodeID), GetRatingIndexByID(RS, pair.SecondNodeID)) = 1 / pair.Value
                            End If
                        End If
                    End If
                Next

                If HasSpanningSet(Matrix, outcomes.Count) Then
                    child.PWOutcomesJudgments.EigenCalcs.SetMatrix(Matrix, outcomes.Count)
                    child.PWOutcomesJudgments.EigenCalcs.Calculate()

                    Dim childValue As Single = 0
                    Dim sumPrty As Single = 0
                    For i As Integer = 0 To outcomes.Count - 1
                        Dim prty As Double = child.PWOutcomesJudgments.EigenCalcs.EigenVector(i)

                        childW.Add(outcomes(i).ID, New clsWeight(outcomes(i).ID, prty))
                        childWUnnorm.Add(outcomes(i).ID, New clsWeight(outcomes(i).ID, prty))
                        sumPrty += prty

                        childValue += prty * outcomes(i).Value
                    Next

                    If sumPrty <> 0 Then
                        For Each weight As clsWeight In childW.Values
                            weight.Value /= sumPrty
                        Next
                    End If

                    W.Add(child.NodeID, New clsWeight(child.NodeID, childValue))
                    WUnnorm.Add(child.NodeID, New clsWeight(child.NodeID, childValue))

                    sum += childValue
                Else
                    W.Add(child.NodeID, New clsWeight(child.NodeID, 0))
                    WUnnorm.Add(child.NodeID, New clsWeight(child.NodeID, 0))
                End If
            Next

            If sum <> 0 Then
                For Each weight As clsWeight In W.Values
                    weight.Value /= sum
                Next
            End If

            UW.Timestamp = Now

            Return True
        End Function

        Public Overrides Function CalculateWeights(ByVal CalculationTarget As clsCalculationTarget, Optional RS As clsRatingScale = Nothing) As Boolean
            If Node.MeasureType = ECMeasureType.mtPWOutcomes Then
                Return CalculatePWOutcomes(CalculationTarget, If(Node.IsAlternative, If(RS IsNot Nothing, RS, Node.MeasurementScale), Node.MeasurementScale))
            End If

            Dim userID As Integer = CalculationTarget.GetUserID
            If Not IsCombinedUserID(userID) AndAlso Node.Hierarchy.ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes Then
                Dim UseCombined As Boolean = False

                If Not Node.IsTerminalNode Then
                    If Not IsCombinedUserID(userID) OrElse (IsCombinedUserID(userID) AndAlso (userID <> COMBINED_USER_ID)) Then
                        If Not Node.IsAllowed(userID) Then
                            UseCombined = True
                        Else
                            If IsCombinedUserID(userID) Then
                                UseCombined = True
                                For Each u As clsUser In CType(CalculationTarget.Target, clsCombinedGroup).UsersList
                                    If Node.IsAllowed(u.UserID) Then
                                        UseCombined = False
                                    End If
                                Next
                            End If
                        End If
                    End If
                Else
                    If GetNodesBelow(userID).Count <> GetNodesBelow(UNDEFINED_USER_ID).Count Then
                        UseCombined = True
                    End If
                End If

                If UseCombined Then
                    Return CalculateWeightsUsingCombinedForRestrictedNodes(CalculationTarget)
                End If
            End If

            If Not IsCombinedUserID(userID) And Not Node.IsAllowed(userID) Then
                Weights.ClearUserWeights(userID)
                Return True
            End If

            Dim nodesbelow As List(Of clsNode) = GetNodesBelow(userID)
            Dim n As Integer = nodesbelow.Count

            If n = 0 Then Return True

            Weights.ClearUserWeights(userID)

            Dim UW As clsUserWeights = Weights.GetUserWeights(userID, ECSynthesisMode.smNone, False)
            Dim W As Dictionary(Of Integer, clsWeight) = UW.Values
            Dim WUnnorm As Dictionary(Of Integer, clsWeight) = UW.UnnormalizedValues
            Dim WNorm As Dictionary(Of Integer, clsWeight) = UW.NormalizedValues

            Dim s As Single = 0
            Dim sUnnorm As Single = 0
            Dim sum As Single = 0

            If n = 1 Then
                W.Add(nodesbelow(0).NodeID, New clsWeight(nodesbelow(0).NodeID, 1))
                WUnnorm.Add(nodesbelow(0).NodeID, New clsWeight(nodesbelow(0).NodeID, 1))
                WNorm.Add(nodesbelow(0).NodeID, New clsWeight(nodesbelow(0).NodeID, 1))
                s = 1
                sUnnorm = 1
                sum = 1
            End If

            If n > 1 Then
                ReDim mMatrix(n - 1, n - 1)
                InitMatrix(n)

                Dim filledSomething As Boolean = FillMatrixWithPairwiseData(userID)

                'If IsEmptyMatrix(n) Or Not HasSpanningSet(userID) Then 'C0960
                If Not filledSomething OrElse Not HasSpanningSet(userID) Then
                    For i As Integer = 0 To n - 1
                        W.Add(nodesbelow(i).NodeID, New clsWeight(nodesbelow(i).NodeID, 0))
                        WUnnorm.Add(nodesbelow(i).NodeID, New clsWeight(nodesbelow(i).NodeID, 0))
                        WNorm.Add(nodesbelow(i).NodeID, New clsWeight(nodesbelow(i).NodeID, 0))
                    Next
                Else
                    EigenCalcs.SetMatrix(mMatrix, n)
                    EigenCalcs.Calculate()

                    For i As Integer = 0 To n - 1
                        W.Add(nodesbelow(i).NodeID, New clsWeight(nodesbelow(i).NodeID, EigenCalcs.EigenVector(i)))
                        WUnnorm.Add(nodesbelow(i).NodeID, New clsWeight(nodesbelow(i).NodeID, EigenCalcs.EigenVector(i)))
                        WNorm.Add(nodesbelow(i).NodeID, New clsWeight(nodesbelow(i).NodeID, EigenCalcs.EigenVector(i)))

                        s += EigenCalcs.EigenVector(i)
                    Next
                    sUnnorm = s
                    sum = s
                End If
            End If

            If Node.MeasureType = ECMeasureType.mtPWAnalogous Then
                Dim attr As clsAttribute = ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_KNOWN_VALUE_ID)
                If attr IsNot Nothing Then
                    Dim KnownValue As Double = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE
                    Dim childID As Integer = -1
                    Dim i As Integer = 0
                    While (childID = -1) And (i < nodesbelow.Count)
                        Dim value As Double = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_KNOWN_VALUE_ID, nodesbelow(i).NodeGuidID, Node.NodeGuidID)
                        If value <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE And value <> 0 Then
                            KnownValue = value
                            childID = nodesbelow(i).NodeID
                        End If
                        i += 1
                    End While
                    If KnownValue <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                        Dim MasterPriority As Double = W(childID).Value
                        Dim uMasterPriority As Double = WUnnorm(childID).Value

                        s = 0
                        sum = 0
                        For Each chID As Integer In W.Keys
                            Dim Weight As clsWeight = W(chID)
                            Dim uWeight As clsWeight = WUnnorm(chID)
                            Dim normWeight As clsWeight = WNorm(chID)
                            If MasterPriority <> 0 Then
                                Weight.Value = KnownValue * Weight.Value / MasterPriority
                                normWeight.Value = KnownValue * normWeight.Value / MasterPriority
                            End If
                            s += Weight.Value
                            If uMasterPriority <> 0 Then
                                uWeight.Value = KnownValue * uWeight.Value / uMasterPriority
                            End If
                            sum += uWeight.Value
                        Next
                    End If
                End If
            End If

            If ProjectManager.IsRiskProject AndAlso Node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                If Node.ParentNode Is Nothing Then
                    Dim UncontributedAlts As List(Of clsNode) = Node.Hierarchy.GetUncontributedAlternatives
                    If UncontributedAlts.Count > 0 Then
                        For Each alt As clsNode In UncontributedAlts
                            Dim J As clsNonPairwiseMeasureData = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, Node.Hierarchy.Nodes(0).NodeID, CalculationTarget.GetUserID)
                            Dim v As Single = 0
                            If J IsNot Nothing Then v = J.SingleValue
                            s += v
                            sUnnorm += v
                        Next

                        For Each alt As clsNode In UncontributedAlts
                            Dim J As clsNonPairwiseMeasureData = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, Node.Hierarchy.Nodes(0).NodeID, CalculationTarget.GetUserID)
                            Dim v As Single = 0
                            If J IsNot Nothing Then
                                v = J.SingleValue
                                If sUnnorm > 1 Then
                                    J.AltNormalizedValue = v / sUnnorm
                                Else
                                    J.AltNormalizedValue = v
                                End If
                            End If
                        Next
                    End If
                End If

                If Not Node.IsTerminalNode AndAlso Not Node.MeasureType = ECMeasureType.mtPWAnalogous Then
                    For Each kvp As KeyValuePair(Of Integer, clsWeight) In W
                        If s > 1 Then kvp.Value.Value /= s
                        If WNorm.ContainsKey(kvp.Key) Then
                            WNorm(kvp.Key).Value /= sum
                        End If
                    Next

                    'For i As Integer = 0 To n - 1
                    '    Dim Weight As clsWeight = W(i)
                    '    If s > 1 Then Weight.Value /= s
                    '    Weight = WNorm(i)
                    '    Weight.Value /= sum
                    'Next
                End If

                For Each child As clsNode In Node.GetNodesBelow(CalculationTarget.GetUserID)
                    If child.RiskNodeType = RiskNodeType.ntCategory Then
                        If W.ContainsKey(child.NodeID) Then
                            W(child.NodeID).Value = 1
                            W(child.NodeID).UnnormValue = 1
                        Else
                            W.Add(child.NodeID, New clsWeight(child.NodeID, 1))
                        End If
                        If WUnnorm.ContainsKey(child.NodeID) Then
                            WUnnorm(child.NodeID).Value = 1
                            WUnnorm(child.NodeID).UnnormValue = 1
                        Else
                            WUnnorm.Add(child.NodeID, New clsWeight(child.NodeID, 1))
                        End If
                        If WNorm.ContainsKey(child.NodeID) Then
                            WNorm(child.NodeID).Value = 1
                            WNorm(child.NodeID).UnnormValue = 1
                        Else
                            WNorm.Add(child.NodeID, New clsWeight(child.NodeID, 1))
                        End If
                    End If
                Next
            End If

            UW.Timestamp = Now

            Return True
        End Function

        Public Function GetNthMostInconsistentJudgment(CalcTarget As clsCalculationTarget, N As Integer) As clsPairwiseMeasureData
            Dim Row As Integer
            Dim Column As Integer
            If Not HasSpanningSet(CalcTarget.GetUserID) Then
                Return Nothing
            Else
                Dim nodes As List(Of clsNode) = GetNodesBelow(CalcTarget.GetUserID)
                Dim MatrixSize As Integer = nodes.Count
                CalculateWeights(CalcTarget)

                ReDim mMatrix(MatrixSize - 1, MatrixSize - 1)
                InitMatrix(MatrixSize)
                FillMatrixWithPairwiseData(CalcTarget.GetUserID)

                Dim M As Double(,)
                ReDim M(MatrixSize - 1, MatrixSize - 1)
                For i As Integer = 0 To MatrixSize - 1
                    For j As Integer = 0 To MatrixSize - 1
                        M(i, j) = mMatrix(i, j)
                    Next
                Next

                ' we don't really need to call CalcPrelim, because in VB6 code it just fills elements under diagonal with reciprocals
                ' this is not needed here since we already have a full matrix at this point

                EigenCalcs.SetMatrix(M, MatrixSize)
                EigenCalcs.Calculate()


                Dim ratios As Single(,)
                ReDim ratios(MatrixSize - 1, MatrixSize - 1)

                For i As Integer = 0 To MatrixSize - 2
                    For j As Integer = i + 1 To MatrixSize - 1
                        Dim aX As Single = mMatrix(i, j)
                        If aX <> 0 Then
                            ' I don't think we need this check, because aX is an element of the matrix and cannot be negative by definition
                            If aX < 0 Then aX = -1 / aX

                            ratios!(i, j) = aX! / (EigenCalcs.EigenVector(i) / EigenCalcs.EigenVector(j))
                            If ratios!(i, j) < 1 Then
                                ratios!(i, j) = 1 / ratios!(i, j)
                            End If

                        Else
                            ratios(i, j) = 0
                        End If
                    Next
                Next
                'AS1572a==

                For k As Integer = 1 To N
                    Dim t1 As Single = 0
                    For i As Integer = 0 To MatrixSize - 2
                        For j As Integer = i + 1 To MatrixSize - 1
                            'If Math.Abs(ratios(i, j)) > t1 > 0 Then 'AS1572b
                            If Math.Abs(ratios(i, j)) - t1 > 0.0001 Then 'AS1572b
                                t1 = Math.Abs(ratios(i, j))
                                Row = i
                                Column = j
                            End If
                        Next
                    Next

                    If k <> N Then
                        ratios(Row, Column) = 0
                    End If
                Next

                If (Row >= 0 And Row < nodes.Count) And (Column >= 0 And Column < nodes.Count) Then
                    Dim res As clsPairwiseMeasureData = PairwiseJudgment(nodes(Row).NodeID, nodes(Column).NodeID, CalcTarget.GetUserID)
                    Return res
                Else
                    Return Nothing
                End If
            End If
        End Function

        Public Function GetNthMostInconsistentJudgmentOutcomes(CalcTarget As clsCalculationTarget, N As Integer, RS As clsRatingScale) As clsPairwiseMeasureData
            Dim Row As Integer
            Dim Column As Integer
            If Not HasSpanningSet(CalcTarget.GetUserID) Then
                Return Nothing
            Else
                Dim ratings As List(Of clsRating) = RS.RatingSet
                Dim MatrixSize As Integer = ratings.Count
                'CalculateWeights(CalcTarget)
                'Dim numMissing As Integer

                ReDim mMatrix(MatrixSize - 1, MatrixSize - 1)
                InitMatrix(MatrixSize)
                FillMatrixWithPairwiseDataOutcomes(CalcTarget.GetUserID, mMatrix, RS)

                Dim M As Double(,)
                ReDim M(MatrixSize - 1, MatrixSize - 1)
                For i As Integer = 0 To MatrixSize - 1
                    For j As Integer = 0 To MatrixSize - 1
                        M(i, j) = mMatrix(i, j)
                    Next
                Next

                ' we don't really need to call CalcPrelim, because in VB6 code it just fills elements under diagonal with reciprocals
                ' this is not needed here since we already have a full matrix at this point

                EigenCalcs.SetMatrix(M, MatrixSize)
                EigenCalcs.Calculate()


                Dim ratios As Single(,)
                ReDim ratios(MatrixSize - 1, MatrixSize - 1)

                For i As Integer = 0 To MatrixSize - 2
                    For j As Integer = i + 1 To MatrixSize - 1
                        Dim aX As Single = mMatrix(i, j)
                        If aX <> 0 Then
                            ' I don't think we need this check, because aX is an element of the matrix and cannot be negative by definition
                            If aX < 0 Then aX = -1 / aX

                            ratios!(i, j) = aX! / (EigenCalcs.EigenVector(i) / EigenCalcs.EigenVector(j))
                            If ratios!(i, j) < 1 Then
                                ratios!(i, j) = 1 / ratios!(i, j)
                            End If

                        Else
                            ratios(i, j) = 0
                        End If
                    Next
                Next
                'AS1572a==

                For k As Integer = 1 To N
                    Dim t1 As Single = 0
                    For i As Integer = 0 To MatrixSize - 2
                        For j As Integer = i + 1 To MatrixSize - 1
                            'If Math.Abs(ratios(i, j)) > t1 > 0 Then 'AS1572b
                            If Math.Abs(ratios(i, j)) - t1 > 0.0001 Then 'AS1572b
                                t1 = Math.Abs(ratios(i, j))
                                Row = i
                                Column = j
                            End If
                        Next
                    Next

                    If k <> N Then
                        ratios(Row, Column) = 0
                    End If
                Next

                If (Row >= 0 And Row < ratings.Count) And (Column >= 0 And Column < ratings.Count) Then
                    Dim res As clsPairwiseMeasureData = PairwiseJudgment(CType(ratings(Row), clsRating).ID, CType(ratings(Column), clsRating).ID, CalcTarget.GetUserID)
                    Return res
                Else
                    Return Nothing
                End If
            End If
        End Function

        Public Function GetBestFitJudgment(CalcTarget As clsCalculationTarget, FirstNodeID As Integer, SecondNodeID As Integer) As clsPairwiseMeasureData
            If Not HasSpanningSet(CalcTarget.GetUserID) Then
                Return Nothing
            Else
                Dim nodes As List(Of clsNode) = GetNodesBelow(CalcTarget.GetUserID)

                Dim Row As Integer = GetNodeIndexByID(nodes, FirstNodeID)
                Dim Column As Integer = GetNodeIndexByID(nodes, SecondNodeID)

                If Row = -1 Or Column = -1 Then
                    Return Nothing
                End If

                Dim MatrixSize As Integer = nodes.Count

                ReDim mMatrix(MatrixSize - 1, MatrixSize - 1)
                InitMatrix(MatrixSize)
                FillMatrixWithPairwiseData(CalcTarget.GetUserID)

                Dim M As Double(,)
                ReDim M(MatrixSize - 1, MatrixSize - 1)

                For i As Integer = 0 To MatrixSize - 1
                    For j As Integer = 0 To MatrixSize - 1
                        If ((i = Row) And (Column = j)) Or ((j = Row) And (Column = i)) Then
                            M(i, j) = 0
                            M(j, i) = 0
                        Else
                            M(i, j) = mMatrix(i, j)
                        End If
                    Next
                Next

                EigenCalcs.SetMatrix(M, MatrixSize)
                EigenCalcs.Calculate()

                Dim resValue As Single
                If EigenCalcs.EigenVector(Column) <> 0 Then
                    resValue = EigenCalcs.EigenVector(Row) / EigenCalcs.EigenVector(Column)
                Else
                    resValue = 0
                End If

                Dim adv As Integer = 0
                If resValue <> 0 Then
                    If resValue < 1 Then
                        resValue = 1 / resValue
                        adv = -1
                    Else
                        adv = 1
                    End If
                End If

                Dim res As New clsPairwiseMeasureData(FirstNodeID, SecondNodeID, adv, resValue, Node.NodeID, CalcTarget.GetUserID)
                Return res
            End If
        End Function

        Public Function GetBestFitJudgmentOutcomes(CalcTarget As clsCalculationTarget, FirstNodeID As Integer, SecondNodeID As Integer, RS As clsRatingScale) As clsPairwiseMeasureData
            If Not HasSpanningSet(CalcTarget.GetUserID) Then
                Return Nothing
            Else
                Dim ratings As List(Of clsRating) = RS.RatingSet

                Dim Row As Integer = RS.RatingSet.IndexOf(RS.GetRatingByID(FirstNodeID))
                Dim Column As Integer = RS.RatingSet.IndexOf(RS.GetRatingByID(SecondNodeID))

                If Row = -1 Or Column = -1 Then
                    Return Nothing
                End If

                Dim MatrixSize As Integer = RS.RatingSet.Count

                ReDim mMatrix(MatrixSize - 1, MatrixSize - 1)
                InitMatrix(MatrixSize)
                FillMatrixWithPairwiseDataOutcomes(CalcTarget.GetUserID, mMatrix, RS)

                Dim M As Double(,)
                ReDim M(MatrixSize - 1, MatrixSize - 1)

                For i As Integer = 0 To MatrixSize - 1
                    For j As Integer = 0 To MatrixSize - 1
                        If ((i = Row) And (Column = j)) Or ((j = Row) And (Column = i)) Then
                            M(i, j) = 0
                            M(j, i) = 0
                        Else
                            M(i, j) = mMatrix(i, j)
                        End If
                    Next
                Next

                EigenCalcs.SetMatrix(M, MatrixSize)
                EigenCalcs.Calculate()

                Dim resValue As Single
                If EigenCalcs.EigenVector(Column) <> 0 Then
                    resValue = EigenCalcs.EigenVector(Row) / EigenCalcs.EigenVector(Column)
                Else
                    resValue = 0
                End If

                Dim adv As Integer = 0
                If resValue <> 0 Then
                    If resValue < 1 Then
                        resValue = 1 / resValue
                        adv = -1
                    Else
                        adv = 1
                    End If
                End If

                Dim res As New clsPairwiseMeasureData(FirstNodeID, SecondNodeID, adv, resValue, Node.NodeID, CalcTarget.GetUserID)
                Return res
            End If
        End Function

        Public Function PairwiseJudgment(ByVal FirstNodeID As Integer, ByVal SecondNodeID As Integer, ByVal UserID As Integer, Optional ParentNodeID As Integer = Integer.MinValue, Optional outcomesNodeID As Integer = Integer.MinValue) As clsPairwiseMeasureData
            If ParentNodeID = Integer.MinValue Then ParentNodeID = Node.NodeID
            For Each tmpPair As clsPairwiseMeasureData In UsersJudgments(UserID)
                If (tmpPair.ParentNodeID = ParentNodeID) And ((tmpPair.FirstNodeID = FirstNodeID) And (tmpPair.SecondNodeID = SecondNodeID) Or
                    (tmpPair.SecondNodeID = FirstNodeID) And (tmpPair.FirstNodeID = SecondNodeID)) Then
                    If (outcomesNodeID = Integer.MinValue) Or ((outcomesNodeID <> Integer.MinValue) And (outcomesNodeID = tmpPair.OutcomesNodeID)) Then
                        Return tmpPair
                    End If
                End If
            Next
            Return Nothing
        End Function

        Public Function PairwiseOutcomesJudgment(ParentNodeID As Integer, ByVal FirstNodeID As Integer, ByVal SecondNodeID As Integer, ByVal UserID As Integer) As clsPairwiseMeasureData
            If Not GetNodesBelow(UserID).Exists(Function(child) (child.NodeID = ParentNodeID)) Then Return Nothing

            For Each tmpPair As clsPairwiseMeasureData In UsersJudgments(UserID)
                If (tmpPair.ParentNodeID = ParentNodeID) And ((tmpPair.FirstNodeID = FirstNodeID) And (tmpPair.SecondNodeID = SecondNodeID) Or
                    (tmpPair.SecondNodeID = FirstNodeID) And (tmpPair.FirstNodeID = SecondNodeID)) Then
                    Return tmpPair
                End If
            Next
            Return Nothing
        End Function

        Public Overrides Function GetWeight(ByVal UserID As Integer, ByVal ChildNodeID As Integer, Optional NodesBelow As HashSet(Of Integer) = Nothing) As Single
            Dim UW As clsUserWeights = Weights.GetUserWeights(UserID, ECSynthesisMode.smDistributive, False)
            Return UW.GetWeightValueByNodeID(ChildNodeID)
        End Function

        Public Overrides Sub AddMeasureData(ByVal data As clsCustomMeasureData, Optional ByVal ForceAdd As Boolean = False) 'C0313
            If ForceAdd Then
                UsersJudgments(data.UserID).Add(data)
                Exit Sub
            End If

            Dim pwData As clsPairwiseMeasureData = CType(data, clsPairwiseMeasureData)
            Dim index As Integer = GetPairIndex(data)

            If index <> -1 Then
                Dim pw As clsPairwiseMeasureData = CType(JudgmentsFromUser(data.UserID).Item(index), clsPairwiseMeasureData)
                pw.IsUndefined = pwData.IsUndefined
                pw.Value = pwData.Value
                pw.Advantage = pwData.Advantage
                pw.ModifyDate = pwData.ModifyDate
                pw.Comment = pwData.Comment
            Else
                JudgmentsFromUser(data.UserID).Add(data)
            End If
        End Sub


        Public Overloads Function HasSpanningSet(ByVal Matrix As Double(,), n As Integer) As Boolean
            If n < 2 Then
                Return True
            End If

            Dim M As Double(,)
            ReDim M(n, n)

            Dim i As Integer, j As Integer

            M(0, 0) = 1
            For i = 0 To n - 1
                For j = 0 To n - 1
                    M(i + 1, j + 1) = Matrix(i, j)
                Next
            Next

            Dim edge As Double(,)
            ReDim edge(n, n)
            For i = 0 To n
                For j = 0 To n
                    edge(i, j) = 0
                Next
            Next


            Dim pred As Double()
            ReDim pred(n)
            For i = 0 To n
                pred(i) = 0
            Next

            Dim Nxx As Double()
            ReDim Nxx(n)
            For i = 0 To n
                Nxx(i) = 0
            Next

            Dim d As Double()
            ReDim d(n)
            For i = 0 To n
                d(i) = 0
            Next

            For i = 1 To n
                For j = i + 1 To n
                    If M(i, j) <> 0 Then
                        d(i) += 1
                        edge(i, d(i)) = j

                        d(j) += 1
                        edge(j, d(j)) = i
                    End If
                Next
            Next

            pred(1) = 1
            i = 1

            Dim NumReached As Integer = 1
            While (Nxx(i) <> d(i)) Or (i <> 1)
                If Nxx(i) = d(i) Then
                    i = pred(i)
                Else
                    Nxx(i) += 1
                    j = edge(i, Nxx(i))
                    If pred(j) = 0 Then
                        pred(j) = i
                        i = j
                        NumReached += 1
                    End If
                End If
            End While

            Dim res As Boolean
            res = (NumReached = n)

            Return res
        End Function

        Public Overloads Function HasSpanningSet(ByVal UserID As Integer) As Boolean
            Dim nodesBelow As List(Of clsNode) = GetNodesBelow(UserID)
            Dim n As Integer = nodesBelow.Count

            If n < 2 Then Return True

            Dim M As Single(,)
            ReDim M(n, n)

            Dim UseCombined As Boolean = False

            If Node.Hierarchy.ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes AndAlso (UserID >= 0 OrElse UserID <= COMBINED_GROUPS_USERS_START_ID) Then
                UseCombined = False
                If Not Node.IsAllowed(UserID) Then
                    UseCombined = True
                Else
                    If GetNodesBelow(UserID).Count <> GetNodesBelow(UNDEFINED_USER_ID).Count Then
                        UseCombined = True
                    End If
                End If
            End If

            Dim i As Integer, j As Integer

            For i = 0 To n
                For j = 0 To n
                    M(i, j) = If(i = j, 1, 0)
                Next
            Next

            Dim pairNode1 As clsNode
            Dim pairNode2 As clsNode

            For Each pair As clsPairwiseMeasureData In JudgmentsFromUser(UserID)
                If Not pair.IsUndefined AndAlso pair.Value <> 0 Then
                    If Node.IsTerminalNode Then
                        pairNode1 = Node.Hierarchy.ProjectManager.AltsHierarchy(Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(pair.FirstNodeID)
                        pairNode2 = Node.Hierarchy.ProjectManager.AltsHierarchy(Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(pair.SecondNodeID)
                    Else
                        pairNode1 = Node.Hierarchy.GetNodeByID(pair.FirstNodeID)
                        pairNode2 = Node.Hierarchy.GetNodeByID(pair.SecondNodeID)
                    End If

                    If nodesBelow.Contains(pairNode1) And nodesBelow.Contains(pairNode2) Then 'C0450
                        If pair.Advantage > 0 Then
                            M(Node.GetChildIndexByID(pair.FirstNodeID, nodesBelow, UserID) + 1, Node.GetChildIndexByID(pair.SecondNodeID, nodesBelow, UserID) + 1) = pair.Value
                            M(Node.GetChildIndexByID(pair.SecondNodeID, nodesBelow, UserID) + 1, Node.GetChildIndexByID(pair.FirstNodeID, nodesBelow, UserID) + 1) = 1 / pair.Value
                        Else
                            M(Node.GetChildIndexByID(pair.SecondNodeID, nodesBelow, UserID) + 1, Node.GetChildIndexByID(pair.FirstNodeID, nodesBelow, UserID) + 1) = pair.Value
                            M(Node.GetChildIndexByID(pair.FirstNodeID, nodesBelow, UserID) + 1, Node.GetChildIndexByID(pair.SecondNodeID, nodesBelow, UserID) + 1) = 1 / pair.Value
                        End If
                    End If
                End If
            Next

            If UseCombined Then
                For Each pair As clsPairwiseMeasureData In JudgmentsFromUser(COMBINED_USER_ID)
                    If Not pair.IsUndefined AndAlso pair.Value <> 0 Then
                        If Node.IsTerminalNode Then
                            pairNode1 = Node.Hierarchy.ProjectManager.AltsHierarchy(Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(pair.FirstNodeID)
                            pairNode2 = Node.Hierarchy.ProjectManager.AltsHierarchy(Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(pair.SecondNodeID)
                        Else
                            pairNode1 = Node.Hierarchy.GetNodeByID(pair.FirstNodeID)
                            pairNode2 = Node.Hierarchy.GetNodeByID(pair.SecondNodeID)
                        End If

                        If nodesBelow.Contains(pairNode1) And nodesBelow.Contains(pairNode2) Then
                            i = Node.GetChildIndexByID(pair.FirstNodeID, nodesBelow, UserID) + 1
                            j = Node.GetChildIndexByID(pair.SecondNodeID, nodesBelow, UserID) + 1
                            If M(i, j) = 0 Then
                                If pair.Advantage > 0 Then
                                    M(i, j) = pair.Value
                                    M(j, i) = 1 / pair.Value
                                Else
                                    M(j, i) = pair.Value
                                    M(i, j) = 1 / pair.Value
                                End If
                            End If
                        End If
                    End If
                Next
            End If

            Dim edge As Single(,)
            ReDim edge(n, n)
            For i = 0 To n
                For j = 0 To n
                    edge(i, j) = 0
                Next
            Next


            Dim pred As Single()
            ReDim pred(n)
            For i = 0 To n
                pred(i) = 0
            Next

            Dim Nxx As Single()
            ReDim Nxx(n)
            For i = 0 To n
                Nxx(i) = 0
            Next

            Dim d As Single()
            ReDim d(n)
            For i = 0 To n
                d(i) = 0
            Next

            For i = 1 To n
                For j = i + 1 To n
                    If M(i, j) <> 0 Then
                        d(i) += 1
                        edge(i, d(i)) = j

                        d(j) += 1
                        edge(j, d(j)) = i
                    End If
                Next
            Next

            pred(1) = 1
            i = 1

            Dim NumReached As Integer = 1
            While (Nxx(i) <> d(i)) Or (i <> 1)
                If Nxx(i) = d(i) Then
                    i = pred(i)
                Else
                    Nxx(i) += 1
                    j = edge(i, Nxx(i))
                    If pred(j) = 0 Then
                        pred(j) = i
                        i = j
                        NumReached += 1
                    End If
                End If
            End While

            Dim res As Boolean
            res = (NumReached = n)

            Return res
        End Function

        Public Sub New(ByVal node As clsNode, Optional ProjectManager As clsProjectManager = Nothing)
            MyBase.New(node, ProjectManager)
        End Sub
    End Class

    <Serializable()> Public Class clsNonPairwiseJudgments
        Inherits clsCustomJudgments

        Public Overrides Sub AddMeasureData(ByVal data As clsCustomMeasureData, Optional ByVal ForceAdd As Boolean = False)
            If data Is Nothing Then Exit Sub

            If ForceAdd Then
                UsersJudgments(data.UserID).Add(data)
                Exit Sub
            End If

            Dim NonPWData As clsNonPairwiseMeasureData = CType(data, clsNonPairwiseMeasureData)

            For Each currMD As clsNonPairwiseMeasureData In UsersJudgments(data.UserID)
                If (currMD.NodeID = NonPWData.NodeID) And (currMD.ParentNodeID = NonPWData.ParentNodeID) Then
                    currMD.ObjectValue = NonPWData.ObjectValue
                    currMD.Comment = NonPWData.Comment
                    currMD.IsUndefined = NonPWData.IsUndefined
                    currMD.ModifyDate = NonPWData.ModifyDate
                    Exit Sub
                End If
            Next

            UsersJudgments(data.UserID).Add(NonPWData)
        End Sub

        Private Function GetWeightUsingCombinedForRestrictedNodes(ByVal UserID As Integer, ByVal CombinedUserID As Integer, ByVal ChildNodeID As Integer, ByVal AllNodes As List(Of clsNode), ByVal UserNodes As List(Of clsNode), Optional ByVal ForceNotUserIdealAlt As Boolean = False) As Single 'C0933
            Dim IncludeIdeal As Boolean = If(ForceNotUserIdealAlt, False, Node.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative And Node.IsTerminalNode)
            Dim IdealMaxValue As Single = 1

            If (ChildNodeID = IDEAL_ALTERNATIVE_ID) AndAlso IncludeIdeal Then Return IdealMaxValue

            If AllNodes.Exists(Function(n) (n.NodeID = ChildNodeID)) Then
                If UserNodes.Exists(Function(n) (n.NodeID = ChildNodeID)) Then
                    If Not IsCombinedUserID(UserID) Then
                        Return GetSingleValue(ChildNodeID, Node.NodeID, UserID)
                    Else
                        Dim useCombined As Boolean = True
                        For Each u As clsUser In ProjectManager.CombinedGroups.GetCombinedGroupByUserID(UserID).UsersList
                            If ProjectManager.UsersRoles.IsAllowedAlternative(Node.NodeGuidID, ProjectManager.ActiveAlternatives.GetNodeByID(ChildNodeID).NodeGuidID, u.UserID) Then
                                useCombined = False
                                Exit For
                            End If
                        Next
                        Return If(useCombined, GetSingleValue(ChildNodeID, Node.NodeID, CombinedUserID), GetSingleValue(ChildNodeID, Node.NodeID, UserID))
                    End If

                Else
                    Return GetSingleValue(ChildNodeID, Node.NodeID, CombinedUserID)
                End If
            Else
                Return 0
            End If
        End Function

        Protected Overrides Function CalculateWeightsUsingCombinedForRestrictedNodes(ByVal CalculationTarget As ECTypes.clsCalculationTarget) As Boolean 'C0785
            Dim userID As Integer = CalculationTarget.GetUserID
            Dim combinedUserID As Integer = Node.Hierarchy.ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID

            Dim IncludeIdeal As Boolean = Node.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative And Node.IsTerminalNode

            Dim UserNodesbelow As List(Of clsNode) = If(Node.IsTerminalNode, GetNodesBelow(userID), New List(Of clsNode))
            Dim AllNodesbelow As List(Of clsNode) = GetNodesBelow(UNDEFINED_USER_ID)

            Weights.ClearUserWeights(userID)

            Dim UW As clsUserWeights = Weights.GetUserWeights(userID, ECSynthesisMode.smNone, False)

            Dim W As Dictionary(Of Integer, clsWeight) = UW.Values
            Dim WUnnorm As Dictionary(Of Integer, clsWeight) = UW.UnnormalizedValues

            Dim sum As Single = 0

            Dim value As Single
            For Each child As clsNode In AllNodesbelow
                value = GetWeightUsingCombinedForRestrictedNodes(userID, combinedUserID, child.NodeID, AllNodesbelow, UserNodesbelow, True)
                W.Add(child.NodeID, New clsWeight(child.NodeID, value))
                WUnnorm.Add(child.NodeID, New clsWeight(child.NodeID, value))
                sum += value
            Next

            If Not Node.Hierarchy.ProjectManager.IsRiskProject AndAlso sum <> 0 AndAlso Not Node.IsTerminalNode Then
                For Each weight As clsWeight In W.Values
                    weight.Value /= sum
                    weight.UnnormValue /= sum
                Next
                For Each weight As clsWeight In WUnnorm.Values
                    weight.Value /= sum
                Next
            End If

            UW.Timestamp = Now

            Return True
        End Function

        Public Overrides Function CalculateWeights(ByVal CalculationTarget As clsCalculationTarget, Optional RS As clsRatingScale = Nothing) As Boolean
            Dim userID As Integer = CalculationTarget.GetUserID
            Dim nodesbelow As HashSet(Of Integer) = Node.GetNodesBelowHS(userID, , False)

            If Node.Hierarchy.ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes Then
                Dim UseCombined As Boolean = False

                If Not Node.IsTerminalNode Then
                    If Not IsCombinedUserID(userID) OrElse (IsCombinedUserID(userID) AndAlso (userID <> COMBINED_USER_ID)) Then
                        If Not Node.IsAllowed(userID) Then
                            UseCombined = True
                        Else
                            If IsCombinedUserID(userID) Then
                                UseCombined = True
                                For Each u As clsUser In CType(CalculationTarget.Target, clsCombinedGroup).UsersList
                                    If Node.IsAllowed(u.UserID) Then
                                        UseCombined = False
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    End If
                Else
                    If Not IsCombinedUserID(userID) Then
                        If nodesbelow.Count <> GetNodesBelow(UNDEFINED_USER_ID).Count Then
                            UseCombined = True
                        End If
                    Else
                        For Each u As clsUser In CType(CalculationTarget.Target, clsCombinedGroup).UsersList
                            If nodesbelow.Count <> GetNodesBelow(UNDEFINED_USER_ID).Count Then
                                UseCombined = True
                                Exit For
                            End If
                        Next
                    End If
                End If

                If UseCombined Then
                    Return CalculateWeightsUsingCombinedForRestrictedNodes(CalculationTarget)
                End If
            End If

            If Not IsCombinedUserID(userID) And Not Node.IsAllowed(userID) Then
                Weights.ClearUserWeights(userID)
                Return True
            End If

            Dim IncludeIdeal As Boolean = Node.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative AndAlso Node.IsTerminalNode

            Dim n As Integer = nodesbelow.Count

            Weights.ClearUserWeights(userID)

            Dim UW As clsUserWeights = Weights.GetUserWeights(userID, ECSynthesisMode.smNone, False)

            Dim W As Dictionary(Of Integer, clsWeight) = UW.Values
            Dim WUnnorm As Dictionary(Of Integer, clsWeight) = UW.UnnormalizedValues
            Dim WNorm As Dictionary(Of Integer, clsWeight) = UW.NormalizedValues

            Dim s As Single = 0
            Dim sUnnorm As Single = 0

            Dim value As Single
            Dim sum As Single = 0

            'For Each child As clsNode In nodesbelow
            '    value = GetWeight(userID, child.NodeID)
            '    W.Add(child.NodeID, New clsWeight(child.NodeID, value))
            '    WUnnorm.Add(child.NodeID, New clsWeight(child.NodeID, value))
            '    WNorm.Add(child.NodeID, New clsWeight(child.NodeID, value))
            '    s += value
            '    sUnnorm += value
            '    sum += value
            'Next

            For Each childID As Integer In nodesbelow
                value = GetWeight(userID, childID, nodesbelow)
                W.Add(childID, New clsWeight(childID, value))
                WUnnorm.Add(childID, New clsWeight(childID, value))
                WNorm.Add(childID, New clsWeight(childID, value))
                s += value
                sUnnorm += value
                sum += value
            Next

            If Not Node.Hierarchy.ProjectManager.IsRiskProject AndAlso sum <> 0 AndAlso Not Node.IsTerminalNode Then
                If sum <> 0 Then
                    For Each weight As clsWeight In W.Values
                        weight.Value /= sum
                    Next
                    For Each weight As clsWeight In WUnnorm.Values
                        weight.Value = weight.Value / sum
                    Next
                End If
            End If

            If ProjectManager.IsRiskProject AndAlso Node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                If Node.ParentNode Is Nothing Then
                    Dim uID As Integer = userID
                    Dim UncontributedAlts As List(Of clsNode) = Node.Hierarchy.GetUncontributedAlternatives
                    If UncontributedAlts.Count > 0 Then
                        For Each alt As clsNode In UncontributedAlts
                            Dim combinedV As Single = 0
                            Dim cJ As clsNonPairwiseMeasureData = Nothing
                            Dim useCIS As Boolean = Not IsCombinedUserID(uID) AndAlso Node.Hierarchy.ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes AndAlso Not ProjectManager.UsersRoles.IsAllowedAlternative(Node.NodeGuidID, alt.NodeGuidID, uID)
                            If useCIS Then
                                cJ = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, Node.Hierarchy.Nodes(0).NodeID, ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID)
                                If cJ IsNot Nothing Then
                                    combinedV = cJ.SingleValue
                                End If
                            End If

                            Dim J As clsNonPairwiseMeasureData = If(alt.DirectJudgmentsForNoCause IsNot Nothing, alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, Node.Hierarchy.Nodes(0).NodeID, CalculationTarget.GetUserID), Nothing)
                            Dim v As Single = 0

                            If useCIS Then
                                If cJ IsNot Nothing Then
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtRatings
                                            J = New clsRatingMeasureData(alt.NodeID, Node.NodeID, CalculationTarget.GetUserID, cJ.ObjectValue, alt.MeasurementScale, cJ.IsUndefined)
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            J = New clsUtilityCurveMeasureData(alt.NodeID, Node.NodeID, CalculationTarget.GetUserID, cJ.ObjectValue, alt.MeasurementScale, cJ.IsUndefined)
                                        Case ECMeasureType.mtStep
                                            J = New clsStepMeasureData(alt.NodeID, Node.NodeID, CalculationTarget.GetUserID, cJ.ObjectValue, alt.MeasurementScale, cJ.IsUndefined)
                                        Case ECMeasureType.mtDirect
                                            J = New clsDirectMeasureData(alt.NodeID, Node.NodeID, CalculationTarget.GetUserID, cJ.ObjectValue, cJ.IsUndefined)
                                    End Select
                                    If J IsNot Nothing Then
                                        alt.DirectJudgmentsForNoCause.AddMeasureData(J, True)
                                    End If
                                End If
                            End If

                            If J IsNot Nothing Then
                                v = J.SingleValue
                            End If

                            s += v
                            sUnnorm += v
                        Next

                        For Each alt As clsNode In UncontributedAlts
                            Dim J As clsNonPairwiseMeasureData = If(alt.DirectJudgmentsForNoCause IsNot Nothing, alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, Node.Hierarchy.Nodes(0).NodeID, CalculationTarget.GetUserID), Nothing)
                            Dim v As Single = 0
                            If J IsNot Nothing Then
                                v = J.SingleValue
                                If sUnnorm > 1 Then
                                    J.AltNormalizedValue = v / sUnnorm
                                Else
                                    J.AltNormalizedValue = v
                                End If
                            End If
                        Next
                    End If
                End If

                'If Not Node.IsTerminalNode Then
                '    For i As Integer = 0 To n - 1
                '        Dim Weight As clsWeight = W(i)
                '        If s > 1 Then
                '            Weight.Value /= s
                '        End If
                '        Weight = WNorm(i)
                '        If sum > 1 Then
                '            Weight.Value /= s
                '        End If
                '    Next
                'End If

                For Each child As clsNode In Node.GetNodesBelow(CalculationTarget.GetUserID)
                    If child.RiskNodeType = RiskNodeType.ntCategory Then
                        If W.ContainsKey(child.NodeID) Then
                            W(child.NodeID).Value = 1
                            W(child.NodeID).UnnormValue = 1
                        Else
                            W.Add(child.NodeID, New clsWeight(child.NodeID, 1))
                        End If
                        If WUnnorm.ContainsKey(child.NodeID) Then
                            WUnnorm(child.NodeID).Value = 1
                            WUnnorm(child.NodeID).UnnormValue = 1
                        Else
                            WUnnorm.Add(child.NodeID, New clsWeight(child.NodeID, 1))
                        End If
                        If WNorm.ContainsKey(child.NodeID) Then
                            WNorm(child.NodeID).Value = 1
                            WNorm(child.NodeID).UnnormValue = 1
                        Else
                            WNorm.Add(child.NodeID, New clsWeight(child.NodeID, 1))
                        End If
                    End If
                Next
            End If

            UW.Timestamp = Now

            Return True
        End Function

        Public Overrides Function GetWeight(ByVal UserID As Integer, ByVal ChildNodeID As Integer, Optional NodesBelow As HashSet(Of Integer) = Nothing) As Single
            Dim IdealMaxValue As Single = 1
            If (ChildNodeID = IDEAL_ALTERNATIVE_ID) Then Return IdealMaxValue

            If NodesBelow Is Nothing Then NodesBelow = Node.GetNodesBelowHS(UserID)

            Return If(NodesBelow.Contains(ChildNodeID), GetSingleValue(ChildNodeID, Node.NodeID, UserID), 0)
            'If GetNodesBelow(UserID).Exists(Function(n) (n.NodeID = ChildNodeID)) Then
            '    Return GetSingleValue(ChildNodeID, Node.NodeID, UserID)
            'Else
            '    Return 0
            'End If
        End Function

        Public Function GetSingleValue(ByVal NodeID As Integer, ByVal ParentNodeID As Integer, ByVal UserID As Integer) As Single
            For Each NonPWMD As clsNonPairwiseMeasureData In UsersJudgments(UserID)
                If (NonPWMD.NodeID = NodeID) And (NonPWMD.ParentNodeID = ParentNodeID) Then
                    Return If(Single.IsNaN(NonPWMD.SingleValue), 0, NonPWMD.SingleValue)
                End If
            Next
            Return 0
        End Function

        Public Function GetObjectValue(ByVal NodeID As Integer, ByVal ParentNodeID As Integer, ByVal UserID As Integer) As Object
            For Each NonPWMD As clsNonPairwiseMeasureData In UsersJudgments(UserID)
                If (NonPWMD.NodeID = NodeID) And (NonPWMD.ParentNodeID = ParentNodeID) Then
                    Return NonPWMD.ObjectValue
                End If
            Next
            Return Nothing
        End Function

        Public Function GetJudgement(ByVal NodeID As Integer, ByVal ParentNodeID As Integer, ByVal UserID As Integer) As clsNonPairwiseMeasureData
            For Each NonPWMD As clsNonPairwiseMeasureData In UsersJudgments(UserID)
                If (NonPWMD.NodeID = NodeID) Then
                    Return NonPWMD
                End If
            Next
            Return Nothing
        End Function

        Public Function GetJudgement(ByVal ObjectiveID As Guid, ByVal EventID As Guid, ByVal UserID As Integer) As clsNonPairwiseMeasureData
            For Each NonPWMD As clsNonPairwiseMeasureData In UsersJudgments(UserID)
                If NonPWMD.CtrlObjectiveID.Equals(ObjectiveID) AndAlso NonPWMD.CtrlEventID.Equals(EventID) Then
                    Return NonPWMD
                End If
            Next
            Return Nothing
        End Function

        Public Function GetJudgement(ByVal ObjectiveID As Guid, ByVal UserID As Integer) As clsNonPairwiseMeasureData
            For Each NonPWMD As clsNonPairwiseMeasureData In UsersJudgments(UserID)
                If NonPWMD.CtrlObjectiveID.Equals(ObjectiveID) Then
                    Return NonPWMD
                End If
            Next
            Return Nothing
        End Function

        Public Sub New(ByVal node As clsNode, Optional AProjectManager As clsProjectManager = Nothing)
            MyBase.New(node, AProjectManager)
        End Sub
    End Class
End Namespace