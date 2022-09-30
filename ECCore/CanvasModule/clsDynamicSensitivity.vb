Imports Microsoft.VisualBasic

Imports ECCore
Imports ECCore.MathFuncs
Imports ECCore.MiscFuncs 'C0361
Imports Canvas

Namespace Canvas
    <Serializable()> Public Class clsDynamicSensitivity
        Private mPrjMan As clsProjectManager
        Private mNode As clsNode

        Private mCalcWRTGoal As Boolean = True
        Private mCalcForCombined As Boolean = False

        Private mNodesList As ArrayList

        Private mSuperMatrixSize As Integer
        Private mInitialSuperMatrix(,) As Single
        Private mSuperMatrix(,) As Single

        Private mMaxAltValue As Single 'C0106

        Private mAlternatives As List(Of clsNode) 'C0331 'C0384

        Private mSAUserID As Integer 'C0567

        Public IsInitialized As Boolean = False ' D2499

        Public Property SAUserID() As Integer 'C0567
            Get
                Return mPrjMan.UserID
                'Return mSAUserID
            End Get
            Set(ByVal value As Integer)
                mSAUserID = value
            End Set
        End Property

        Public Property ProjectManager() As clsProjectManager
            Get
                Return mPrjMan
            End Get
            Set(ByVal value As clsProjectManager)
                mPrjMan = value
            End Set
        End Property

        Public Sub New(ByVal ProjectManager As clsProjectManager)
            mPrjMan = ProjectManager
            If Not mPrjMan Is Nothing Then
                If mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes.Count <> 0 Then
                    mNode = mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0)
                End If
            End If
            mMaxAltValue = -2 'C0106
            mAlternatives = Nothing 'C0331
            CalculateForCombined = True
        End Sub

        Private Sub AddNodeToNodesList(ByVal node As clsNode)
            If node.IsTerminalNode Then Exit Sub

            For Each nd As clsNode In node.GetNodesBelow(IIf(CalculateForCombined Or ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes, UNDEFINED_USER_ID, SAUserID), , , ProjectManager.UserID)
                mNodesList.Add(nd)
            Next
            For Each nd As clsNode In node.GetNodesBelow(IIf(CalculateForCombined Or ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes, UNDEFINED_USER_ID, SAUserID), , , ProjectManager.UserID)
                AddNodeToNodesList(nd)
            Next
        End Sub

        Private Sub CreateNodesList()
            mNodesList = New ArrayList

            ' add objective nodes 'TODO: remove this
            'Node = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)

            If Node Is Nothing Then
                Exit Sub
            End If

            Dim rootNode As clsNode = IIf(mCalcWRTGoal, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0), Node) 'C0005
            'Dim rootNode As clsNode = Node 'C0005

            'C0005===
            'mNodesList.Add(Node)
            'AddNodeToNodesList(Node)
            mNodesList.Add(rootNode)
            AddNodeToNodesList(rootNode)
            'C0005==

            ' add alternative nodes
            'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0331
            For Each alt As clsNode In Alternatives 'C0331
                mNodesList.Add(alt)
            Next
        End Sub

        Private Sub CreateInitialSuperMatrix()
            CreateNodesList()

            mSuperMatrixSize = mNodesList.Count
            'mSuperMatrixSize = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes.Count + _
            '    ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Count
            ReDim mInitialSuperMatrix(mSuperMatrixSize - 1, mSuperMatrixSize - 1)

            Dim node1 As clsNode
            Dim node2 As clsNode
            Dim altIndex As Integer

            Dim calcTarget As clsCalculationTarget 'C0560

            'Dim oldUserID As Integer 'C0567
            If CalculateForCombined Then
                'oldUserID = ProjectManager.UserID 'C0560 'C0567
                'ProjectManager.UserID = COMBINED_USER_ID 'C0560 'C0567
                calcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup) 'C0560
            Else
                'calcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.User) 'C0560 'C0567
                calcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(SAUserID)) 'C0567
            End If

            For i As Integer = 0 To mNodesList.Count - 1
                node1 = mNodesList(i)
                For j As Integer = 0 To mNodesList.Count - 1
                    node2 = mNodesList(j)

                    If Not node1.IsAlternative Then
                        'mInitialSuperMatrix(i, j) = IIf(node1.ParentNode Is node2, node1.LocalPriority, 0) 'C0159
                        'mInitialSuperMatrix(i, j) = IIf(node1.ParentNode Is node2, node1.LocalPriority(ProjectManager.UserID), 0) 'C0159 'C0551
                        'mInitialSuperMatrix(i, j) = IIf(node1.ParentNode Is node2, node1.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.User)), 0) 'C0551 'C0560
                        mInitialSuperMatrix(i, j) = IIf(node1.ParentNode Is node2, node1.LocalPriority(calcTarget), 0) 'C0560
                    Else
                        ' rows for alternatives
                        If node2.IsAlternative Then
                            mInitialSuperMatrix(i, j) = IIf(node1 Is node2, 1, 0)
                        Else
                            If node2.IsTerminalNode Then
                                'altIndex = node2.ChildrenAlts.IndexOf(node1.NodeID)
                                'altIndex = node2.GetNodesBelow.IndexOf(node1) 'C0450
                                'altIndex = node2.GetNodesBelow(ProjectManager.UserID).IndexOf(node1) 'C0450 'C0567
                                altIndex = node2.GetNodesBelow(SAUserID).IndexOf(node1) 'C0567
                                'C0128===
                                'mInitialSuperMatrix(i, j) = IIf(altIndex <> -1, _
                                '    node2.Judgments.Weights(altIndex), 0)

                                'C0159===
                                'mInitialSuperMatrix(i, j) = IIf(altIndex <> -1, _
                                '    node2.Judgments.Weights(node1.NodeID), 0)

                                'C0177===
                                'mInitialSuperMatrix(i, j) = IIf(altIndex <> -1, _
                                '    node2.Judgments.Weights.GetUserWeights(ProjectManager.UserID).GetWeightValueByNodeID(node1.NodeID), 0)

                                'mInitialSuperMatrix(i, j) = IIf(altIndex <> -1, _
                                '    node2.Judgments.Weights.GetUserWeights(ProjectManager.UserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(node1.NodeID), 0) 'C0338 'C0567

                                mInitialSuperMatrix(i, j) = IIf(altIndex <> -1, _
                                    node2.Judgments.Weights.GetUserWeights(SAUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(node1.NodeID), 0) 'C0567

                                'node2.Judgments.Weights.GetUserWeights(ProjectManager.UserID, ProjectManager.CalculationsManager.SynthesisMode).GetWeightValueByNodeID(node1.NodeID), 0) 'C0338
                                'C0177==

                                'C0159==
                                'C0128==
                            Else
                                mInitialSuperMatrix(i, j) = 0
                            End If
                        End If
                    End If
                Next
            Next

            'C0567===
            'If CalculateForCombined Then
            '    ProjectManager.UserID = oldUserID
            'End If
            'C0567==
        End Sub

        Private Sub DebugPrintMatrix(ByVal text As String, ByVal M As Single(,))
            Debug.Print("Matrix: " + text)
            Dim s As String
            For i As Integer = 0 To mSuperMatrixSize - 1
                s = "(" + i.ToString + ")"
                For j As Integer = 0 To mSuperMatrixSize - 1
                    s += " " + M(i, j).ToString("G4")
                Next
                Debug.Print(s)
            Next
        End Sub

        Private Sub NormalizeAltsInSupermatrix()
            'Dim altsCount As Integer = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Count 'C0331
            Dim altsCount As Integer = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Count 'C0331
            Dim sum As Single

            For j As Integer = 0 To (mSuperMatrixSize - 1) - altsCount
                sum = 0
                For i As Integer = (mSuperMatrixSize - 1) - altsCount To mSuperMatrixSize - 1
                    sum += mSuperMatrix(i, j)
                Next
                ' normalize
                For i As Integer = (mSuperMatrixSize - 1) - altsCount To mSuperMatrixSize - 1
                    If sum <> 0 Then 'C0061
                        mSuperMatrix(i, j) = mSuperMatrix(i, j) / sum
                    End If
                Next
            Next
        End Sub

        Private Sub CreateSuperMatrix()
            'ProjectManager.CalculationsManager.CreateCombinedJudgments()
            'ProjectManager.CalculationsManager.Calculate(ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy, _
            'ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0), False, Nothing, True, True)


            'ProjectManager.CalculationsManager.Calculate(ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy, _
            '    ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))


            CreateInitialSuperMatrix()
            DebugPrintMatrix("initial", mInitialSuperMatrix)

            Dim n As Integer = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetMaxLevel

            ReDim mSuperMatrix(mSuperMatrixSize - 1, mSuperMatrixSize - 1)

            Dim B(,) As Single
            ReDim B(mSuperMatrixSize - 1, mSuperMatrixSize - 1)

            CopySquareMatrix(mInitialSuperMatrix, B, mSuperMatrixSize) 'C0300
            'B = ECMathFuncs.CopySquareMatrix(mInitialSuperMatrix, mSuperMatrixSize) 'C0300
            For i As Integer = 1 To n
                MultSquareMatrix(B, mInitialSuperMatrix, mSuperMatrix, mSuperMatrixSize) 'C0300
                'mSuperMatrix = ECMathFuncs.MultSquareMatrix(B, mInitialSuperMatrix, mSuperMatrixSize) 'C0300
                If i <> n Then
                    CopySquareMatrix(mSuperMatrix, B, mSuperMatrixSize) 'C0300
                    'B = ECMathFuncs.CopySquareMatrix(mSuperMatrix, mSuperMatrixSize) 'C0300
                End If
            Next

            DebugPrintMatrix("final", mSuperMatrix)
            If ProjectManager.CalculationsManager.SynthesisMode = ECSynthesisMode.smIdeal Then
                NormalizeAltsInSupermatrix()
                DebugPrintMatrix("final normalized", mSuperMatrix)
            End If
        End Sub

        'Public Function GetMaxAltValueInSA() As Single 'C0159
        'Public Function GetMaxAltValueInSA(ByVal UserID As Integer) As Single 'C0159 'C0567
        Public Function GetMaxAltValueInSA() As Single 'C0567
            'C0106===
            If mMaxAltValue <> -2 Then
                Return mMaxAltValue
            End If
            'C0106==

            If Node Is Nothing Then
                Return -1
            End If

            If Node.Children.Count < 1 Then
                Return -1
            End If

            Dim max As Single = -1


            'If mCalcWRTGoal And (Not Node Is ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)) Then
            If True Then
                Debug.Print("GetMaxAltValueInSA start: " + Now.ToString)
                'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0331
                For Each alt As clsNode In Alternatives 'C0331
                    If alt.WRTGlobalPriority > max Then
                        max = alt.WRTGlobalPriority
                    End If
                Next


                For Each child As clsNode In Node.Children
                    For Each child1 As clsNode In Node.Children
                        child1.SALocalPriority = IIf(child1 Is child, 1, 0)
                    Next

                    'mPrjMan.CalculationsManager.Calculate(mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mNode, True, mNode, mCalcForCombined, False)

                    'Calculate() 'C0159
                    'Calculate(UserID) 'C0159 'C0567
                    Calculate() 'C0567

                    'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0331
                    For Each alt As clsNode In Alternatives 'C0331
                        If alt.SAGlobalPriority > max Then
                            max = alt.SAGlobalPriority
                        End If
                    Next
                Next

                Debug.Print("GetMaxAltValueInSA end: " + Now.ToString)

                Return max
                'Return Node.WRTGlobalPriority
            End If

            Dim i As Integer, j As Integer

            For Each nd As clsNode In Node.Children
                'j = mNodesList.IndexOf(nd.NodeID)
                j = mNodesList.IndexOf(nd)
                If j <> -1 Then
                    'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0331
                    For Each alt As clsNode In Alternatives 'C0331
                        'i = mNodesList.IndexOf(alt.NodeID)
                        i = mNodesList.IndexOf(alt)
                        If i <> -1 Then
                            If mSuperMatrix(i, j) > max Then
                                max = mSuperMatrix(i, j)
                            End If
                        End If
                    Next
                End If
            Next

            'C0005===
            'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0331
            For Each alt As clsNode In Alternatives 'C0331
                i = mNodesList.IndexOf(alt)
                If i <> -1 Then
                    If mSuperMatrix(i, 0) > max Then
                        max = mSuperMatrix(i, 0)
                    End If
                End If
            Next
            'C0005==

            'Return max
            Return IIf(max = -1, 1, max)
        End Function

        Public Function GetAllAltsValuesForNode(ByVal node As clsNode) As ArrayList
            Dim res As New ArrayList

            If node Is Nothing Then
                Return Nothing
            End If

            'Dim j As Integer = mNodesList.IndexOf(node.NodeID)
            Dim j As Integer = mNodesList.IndexOf(node)
            Dim i As Integer

            If j <> -1 Then
                'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0331
                For Each alt As clsNode In Alternatives 'C0331
                    'i = mNodesList.IndexOf(alt.NodeID)
                    i = mNodesList.IndexOf(alt)
                    If i <> -1 Then
                        res.Add(mSuperMatrix(i, j))
                    End If
                Next
            Else
                Return Nothing
            End If

            Return res
        End Function

        'Public Function GetAltValueForChild(ByVal childNode As clsNode, ByVal alternative As clsNode) As Single 'C0159
        'Public Function GetAltValueForChild(ByVal UserID As Integer, ByVal childNode As clsNode, ByVal alternative As clsNode) As Single 'C0159 'C0567
        Public Function GetAltValueForChild(ByVal childNode As clsNode, ByVal alternative As clsNode) As Single 'C0567
            If Node Is Nothing Or alternative Is Nothing Or childNode Is Nothing Then
                Return -1
            End If

            If childNode.ParentNode IsNot Node Then
                Return -1
            End If

            'If mCalcWRTGoal And (Node IsNot ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)) Then 'C0256
            If True Then 'C0256
                For Each child1 As clsNode In Node.Children
                    child1.SALocalPriority = IIf(child1 Is childNode, 1, 0)
                Next

                'Calculate() 'C0159
                'Calculate(UserID) 'C0159 'C0567
                Calculate() 'C0567

                Return alternative.SAGlobalPriority
            End If

            Dim i As Integer, j As Integer
            j = mNodesList.IndexOf(childNode)
            If j <> -1 Then
                i = mNodesList.IndexOf(alternative)
                If i <> -1 Then
                    'If mSuperMatrix(i, j) Then 'C0018
                    Return mSuperMatrix(i, j)
                    'End If 'C0018
                End If
            End If

            Return -1
        End Function

        'Public Sub GetGradientAltValuesForChild(ByVal childNode As clsNode, ByVal alternative As clsNode, _
        '    ByRef ValueInZero As Single, ByRef ValueInOne As Single) 'C0009 'C0159

        'Public Sub GetGradientAltValuesForChild(ByVal UserID As Integer, ByVal childNode As clsNode, ByVal alternative As clsNode, _
        '    ByRef ValueInZero As Single, ByRef ValueInOne As Single) 'C0159 'C0567
        Public Sub GetGradientAltValuesForChild(ByVal childNode As clsNode, ByVal alternative As clsNode, _
            ByRef ValueInZero As Single, ByRef ValueInOne As Single) 'C0567

            If Node Is Nothing Or alternative Is Nothing Or childNode Is Nothing Then
                Return
            End If

            'C0010===
            'If childNode.ParentNode IsNot Node Then
            '    Return
            'End If

            Node = childNode.ParentNode

            If Node Is Nothing Then
                Return
            End If
            'C0010==

            'C0571===
            Dim calcTarget As clsCalculationTarget
            If IsCombinedUserID(SAUserID) Then 'C0555
                calcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)
            Else
                If SAUserID >= 0 Then
                    calcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(SAUserID))
                Else
                    calcTarget = New clsCalculationTarget(CalculationTargetType.cttDataInstance, ProjectManager.GetDataInstanceByUserID(SAUserID))
                End If
                'C0576==
            End If
            'C0571==

            'C0010===
            For Each child1 As clsNode In Node.Children
                'child1.SALocalPriority = child1.WRTGlobalPriority 'C0010
                'child1.SALocalPriority = child1.LocalPriority 'C0010 'C0159
                'child1.SALocalPriority = child1.LocalPriority(UserID) 'C0159 'C0551
                'child1.SALocalPriority = child1.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(UserID))) 'C0551 'C0567
                'child1.SALocalPriority = child1.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(SAUserID))) 'C0567 'C0571
                child1.SALocalPriority = child1.LocalPriority(calcTarget) 'C0571
            Next

            'Calculate() 'C0159
            'Calculate(UserID) 'C0159 'C0567
            Calculate() 'C0567
            'C0010==

            ' calculate in 1
            For Each child1 As clsNode In Node.Children
                child1.SALocalPriority = IIf(child1 Is childNode, 1, 0)
            Next
            'Calculate() 'C0159
            'Calculate(UserID) 'C0159 'C0567
            Calculate() 'C0567
            ValueInOne = alternative.SAGlobalPriority

            ' calculate in 0
            For Each child1 As clsNode In Node.Children
                'child1.SALocalPriority = child1.WRTGlobalPriority 'C0010
                'child1.SALocalPriority = child1.LocalPriority 'C0010 'C0159
                'child1.SALocalPriority = child1.LocalPriority(UserID) 'C0159 'C0551
                'child1.SALocalPriority = child1.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(UserID))) 'C0551 'C0567
                'child1.SALocalPriority = child1.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(SAUserID))) 'C0567 'C0571
                child1.SALocalPriority = child1.LocalPriority(calcTarget) 'C0571
            Next
            'Calculate() 'C0159
            'Calculate(UserID) 'C0159 'C0567
            Calculate() 'C0567

            Dim sum As Single = 0
            For Each child1 As clsNode In Node.Children
                If child1 IsNot childNode Then
                    sum += child1.SALocalPriority
                End If
            Next

            If sum = 0 Then
                Return
            End If

            Dim k As Single = (1 - 0) / sum
            For Each child1 As clsNode In Node.Children
                If child1 IsNot childNode Then
                    child1.SALocalPriority *= k
                Else
                    'child1.SAGlobalPriority = 0 '0029
                    child1.SALocalPriority = 0 '0029
                End If
            Next
            'Calculate() 'C0159
            'Calculate(UserID) 'C0159 'C0567
            Calculate() 'C0567

            ValueInZero = alternative.SAGlobalPriority
        End Sub

        Public Property Node() As clsNode
            Get
                Return mNode
            End Get
            Set(ByVal value As clsNode)
                mNode = value
            End Set
        End Property

        Public ReadOnly Property Objectives() As List(Of clsNode) 'C0384
            Get
                Dim res As New List(Of clsNode)
                Dim tList As List(Of clsNode) = Nothing
                If mNode IsNot Nothing Then tList = mNode.GetNodesBelow(IIf(CalculateForCombined Or ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes, UNDEFINED_USER_ID, SAUserID), , , ProjectManager.UserID)
                If tList IsNot Nothing Then
                    For Each nd As clsNode In tList
                        If nd.RiskNodeType <> RiskNodeType.ntCategory Then
                            res.Add(nd)
                        End If
                    Next
                End If
                Return res
            End Get
        End Property

        Public ReadOnly Property Alternatives() As List(Of clsNode) 'C0384
            Get
                'Return mPrjMan.AltsHierarchy(mPrjMan.ActiveAltsHierarchy).TerminalNodes.Clone 'C0010 'C0093
                'C0093===

                'Return mPrjMan.GetAllowedAlternatives(mPrjMan.UserID) 'C0331
                'C0331===
                If mAlternatives Is Nothing Then
                    'Return mPrjMan.GetAllowedAlternatives(mPrjMan.UserID) 'C031
                    'C0361===
                    'C0450===
                    'If CalculateForCombined Then
                    '    'Dim CovObjs = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes.Clone 'C0385
                    '    Dim CovObjs As List(Of clsNode) = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes 'C0384
                    '    Return GetContributedAlternatives(CovObjs)
                    'Else
                    '    Return mPrjMan.GetAllowedAlternatives(mPrjMan.UserID) 'C0367
                    'End If
                    'C0450==

                    'C0450===
                    Dim CovObjs As List(Of clsNode) = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes
                    'Return GetAvailableAlternatives(CovObjs, IIf(CalculateForCombined, UNDEFINED_USER_ID, mPrjMan.UserID)) 'C0567
                    Return GetAvailableAlternatives(CovObjs, IIf(CalculateForCombined Or ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes, UNDEFINED_USER_ID, SAUserID)) 'C0567
                    'C0450==

                    'C0361==
                Else
                    Return mAlternatives
                End If
                'C0331==

                'If mCalcForCombined Then
                '    Return mPrjMan.GetAllowedAlternatives(COMBINED_USER_ID)
                'Else
                '    Return mPrjMan.GetAllowedAlternatives(mPrjMan.UserID)
                'End If
                'C0093==
            End Get
        End Property

        Public Property CalculateWRTGoal() As Boolean
            Get
                Return mCalcWRTGoal
            End Get
            Set(ByVal value As Boolean)
                mCalcWRTGoal = value
            End Set
        End Property

        Public Property CalculateForCombined() As Boolean
            Get
                'Return False
                Return mCalcForCombined
            End Get
            Set(ByVal value As Boolean)
                mCalcForCombined = value
            End Set
        End Property

        'Public Sub Init() 'C0159
        'Public Sub Init(ByVal UserID As Integer) 'C0159 'C0567
        Public Sub Init() 'C0567
            mAlternatives = Nothing 'C0331

            'C0095===
            Dim bIncludeIdeal As Boolean = mPrjMan.CalculationsManager.IncludeIdealAlternative
            Dim bShowIdeal As Boolean = mPrjMan.CalculationsManager.ShowIdealAlternative

            mPrjMan.CalculationsManager.IncludeIdealAlternative = False
            mPrjMan.CalculationsManager.ShowIdealAlternative = False
            'C0095==

            'mPrjMan.CalculationsManager.Calculate(mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), False, Nothing, mCalcForCombined) 'C0159
            'mPrjMan.CalculationsManager.Calculate(UserID, mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), False, Nothing, mCalcForCombined) 'C0159 'C0551
            'C0551===
            Dim calcTarget As clsCalculationTarget
            If CalculateForCombined Then
                calcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, mPrjMan.CombinedGroups.GetDefaultCombinedGroup)
            Else
                'calcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, mPrjMan.GetUserByID(UserID)) 'C0567
                calcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, mPrjMan.GetUserByID(SAUserID)) 'C0567
            End If

            mPrjMan.CalculationsManager.Calculate(calcTarget, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, False, Node, True)

            'If CalculateForCombined Then
            '    mPrjMan.CalculationsManager.Calculate(calcTarget, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, False, Node, True)
            'Else
            '    mPrjMan.CalculationsManager.Calculate(calcTarget, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0))
            'End If
            'C0551==

            ' -D3459
            'For Each child As clsNode In Objectives
            '    'child.SALocalPriority = child.LocalPriority 'C0159
            '    'child.SALocalPriority = child.LocalPriority(UserID) 'C0159 'C0551
            '    child.SALocalPriority = child.LocalPriority(calcTarget) 'C0551
            '    'child.SAGlobalPriority = child.GlobalPriority 'C0450
            'Next

            If mCalcWRTGoal Then
                'mPrjMan.CalculationsManager.Calculate(mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), True, mNode, mCalcForCombined) 'C0159
                'mPrjMan.CalculationsManager.Calculate(UserID, mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), True, mNode, mCalcForCombined) 'C0159 'C0551
                mPrjMan.CalculationsManager.Calculate(calcTarget, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, True, mNode) 'C0551
            Else
                'mPrjMan.CalculationsManager.Calculate(mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), True, mNode, mCalcForCombined)
                'mPrjMan.CalculationsManager.Calculate(mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mNode, True, mNode, mCalcForCombined) 'C0159
                mPrjMan.CalculationsManager.Calculate(calcTarget, mNode, mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, True, mNode) 'C0159
            End If
            'Calculate()

            ' D3459 === // moved here
            For Each child As clsNode In Objectives
                child.SALocalPriority = child.LocalPriority(calcTarget) 'C0551
            Next

            'For Each alt As clsNode In Alternatives 'C0093
            'For Each alt As clsNode In mPrjMan.AltsHierarchy(mPrjMan.ActiveAltsHierarchy).TerminalNodes.Clone 'C0331
            For Each alt As clsNode In Alternatives 'C0331
                'alt.SAGlobalPriority = alt.WRTGlobalPriority
            Next
            ' D3459 ==

            CreateSuperMatrix()

            'C0095===
            mPrjMan.CalculationsManager.IncludeIdealAlternative = bIncludeIdeal
            mPrjMan.CalculationsManager.ShowIdealAlternative = bShowIdeal
            'C0095==

            IsInitialized = True    ' D2499
        End Sub

        'Public Sub Calculate() 'C0159
        'Public Sub Calculate(ByVal UserID As Integer) 'C0159 'C0567
        Public Sub Calculate() 'C0567
            'C0090===
            Dim bIncludeIdeal As Boolean = mPrjMan.CalculationsManager.IncludeIdealAlternative
            Dim bShowIdeal As Boolean = mPrjMan.CalculationsManager.ShowIdealAlternative

            'mPrjMan.CalculationsManager.IncludeIdealAlternative = False 'C0254
            mPrjMan.CalculationsManager.ShowIdealAlternative = False
            'C0090==

            'C0551===
            Dim calcTarget As clsCalculationTarget
            If CalculateForCombined Then
                calcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, mPrjMan.CombinedGroups.GetDefaultCombinedGroup)
            Else
                'calcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, mPrjMan.GetUserByID(UserID)) 'C0567
                calcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, mPrjMan.GetUserByID(SAUserID)) 'C0567
            End If
            'C0551==

            mCalcWRTGoal = Node Is mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0)

            If mCalcWRTGoal Then
                'mPrjMan.CalculationsManager.Calculate(mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), True, mNode, mCalcForCombined, False) 'C0159
                'mPrjMan.CalculationsManager.Calculate(UserID, mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), True, mNode, mCalcForCombined, False) 'C0159 'C0551
                mPrjMan.CalculationsManager.Calculate(calcTarget, mPrjMan.Hierarchy(mPrjMan.ActiveHierarchy).Nodes(0), mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, True, mNode, False) 'C0551
            Else
                'mPrjMan.CalculationsManager.Calculate(mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mNode, True, mNode, mCalcForCombined, False) 'C0159
                'mPrjMan.CalculationsManager.Calculate(UserID, mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, mNode, True, mNode, mCalcForCombined, False) 'C0159 'C0551
                mPrjMan.CalculationsManager.Calculate(calcTarget, mNode, mPrjMan.ActiveHierarchy, mPrjMan.ActiveAltsHierarchy, True, mNode, False) 'C0551
            End If

            'C0090===
            mPrjMan.CalculationsManager.IncludeIdealAlternative = bIncludeIdeal
            mPrjMan.CalculationsManager.ShowIdealAlternative = bShowIdeal
            'C0090==
        End Sub

        Public Property MaxAltValue() As Single 'C0106
            Get
                Return mMaxAltValue
            End Get
            Set(ByVal value As Single)
                mMaxAltValue = value
            End Set
        End Property
    End Class
End Namespace