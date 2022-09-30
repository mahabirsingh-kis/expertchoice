Imports ECCore
Imports Canvas
Imports System.Data
Imports System.Data.odbc

Namespace Canvas

    Public Enum CanvasReportType
        crtAllJudgments = 0
        'crtHierarchyJudgments = 1
        'crtAltsPairwiseJudgments = 2
        'crtAltsRatingsJudgments = 3
        crtHierarchyObjectives = 4
        'crtHierarchyAlternatives = 5
        crtHierarchyObjectivesAndAlternatives = 6
        crtObjectivesPriorities = 7
        crtObjectivesAndAlternativesPriorities = 8
        crtOverallAlternativesPriorities = 9
        'crtPivotPriorities = 10 'C0024
        crtUserPermissions = 11 'C0127
        crtSilverlightReport = 12 'C0210
        'crtSilverlightReport2 = 13 'C0223
        crtSilverlightReportInitialData = 14 'C0229
        crtHierarchyObjectivesAndAlternatives2 = 15 'C0250
        crtMaxOutPriorities = 16 'C0389
        crtLocalPrioritiesForSilverlight = 17 'C0573
        crtConsensusView = 18 'C0631
        crtUsersObjectivesPriorities = 19 'C0759
        crtEvaluationProgress = 20 'C0766
        'crtOverallResults1 = 21 'C0816
        'crtOverallResults2 = 22 'C0816
        crtPivotAlternativesPriorities = 23 'C0822
        crtInconsistencies = 24 'C1004
        crtDataGrid = 25
        crtAllJudgmentsInOne = 26
        crtDataGridRisk = 27
        crtHierarchyObjectives2 = 28
        crtDataGridRiskWithControls = 29
        crtConsensusViewOnly = 30 'A1041
        crtJudgmentsObjs = 31   ' D3703
        crtJudgmentsAlts = 32   ' D3730
    End Enum

    Public Enum GroupReportType
        grtGroupOnly = 0
        grtUsersOnly = 1
        grtBoth = 2
    End Enum

    Public Enum ReportCommentType 'C0580
        rctComment = 0
        rctInfoDoc = 1
        rctTag = 2
    End Enum

    <Serializable()> Public Class clsProjectDataProvider

        Public Const TotalCol As Integer = -1
        Public Const CostCol As Integer = -2
        Public Const RiskCol As Integer = -3

        Private Const TABLE_NAME_HIERARCHY_STRUCTURE As String = "HierarchyStructure"
        Private Const TABLE_NAME_HIERARCHY_STRUCTURE_2 As String = "HierarchyStructure2"
        Private Const TABLE_NAME_HIERARCHY_JUDGMENTS As String = "HierarchyJudgments"
        Private Const TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS As String = "AltsPairwiseJudgments"
        Private Const TABLE_NAME_ALTS_RATINGS_JUDGMENTS As String = "AltsRatingsJudgments"
        Private Const TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS As String = "AltsNonRatingsJudgments" 'C0188
        Private Const TABLE_NAME_ALL_PAIRWISE_JUDGMENTS As String = "AllPairwiseJudgments"
        Private Const TABLE_NAME_STRUCTURE_WITH_PRIORITIES As String = "StructureWithPriorities"
        Private Const TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES As String = "OverallAlternativesPriorities"
        Private Const TABLE_NAME_PIVOT_PRIORITIES As String = "PivotPriorities" 'C0024
        Private Const TABLE_NAME_USER_PERMISSIONS As String = "UserPermissions" 'C0127
        Private Const TABLE_NAME_OBJECTIVES_WITH_PRIORITIES As String = "ObjectivesWithPriorities" 'C0573
        Private Const TABLE_NAME_OBJECTIVES_LIST As String = "ObjectivesList" 'C0223
        Private Const TABLE_NAME_ALTERNATIVES_LIST As String = "AlternativesList" 'C0223
        Private Const TABLE_NAME_USERS_LIST As String = "UsersList" 'C0223
        Private Const TABLE_NAME_USER_OBJECTIVES_PRIORITIES As String = "UserObjectivesPriorities" 'C0223
        Private Const TABLE_NAME_USER_ALTERNATIVES_PRIORITIES As String = "UserAlternativesPriorities" 'C0223
        Private Const TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS As String = "UserAltsJudgments" 'C0223
        Private Const TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES As String = "AlternativesLocalPriorities" 'C0573
        Private Const TABLE_NAME_MAXOUT_NODES_PRIORITIES As String = "MaxOutNodesPriorities" 'C0389
        Private Const TABLE_NAME_MAXOUT_ALTERNATIVES_PRIORITIES As String = "MaxOutAlternativesPriorities" 'C0389
        Private Const TABLE_NAME_CONSENSUS_VIEW As String = "ConsensusView" 'C0631
        Private Const TABLE_NAME_USERS_OBJECTIVES_PRIORITIES As String = "UsersObjectivesPriorities" 'C0759
        Private Const TABLE_NAME_EVALUATION_PROGRESS As String = "EvaluationProgress" 'C0766
        Private Const TABLE_NAME_OVERALL_RESULTS_2 As String = "OverallResults2" 'C0816
        Private Const TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES As String = "PivotAlternativesPriorities" 'C0822
        Private Const TABLE_NAME_INCONSISTENCIES As String = "Inconsistencies" 'C1004
        Private Const TABLE_NAME_DATA_GRID As String = "DataGrid"
        Private Const TABLE_NAME_DATA_GRID_RISK As String = "DataGridRisk"
        Private Const TABLE_NAME_ALL_JUDGMENTS As String = "AllJudgments"

        Private Const NODE_PATH_DELIMITER As String = " | "

        Private Const MEASUREMENT_TYPE_PAIRWISE_NAME As String = "Pairwise"
        Private Const MEASUREMENT_TYPE_PAIRWISE_OUTCOMES_NAME As String = "Pairwise of Probabilities"
        Private Const MEASUREMENT_TYPE_PAIRWISE_ANALOGOUS_NAME As String = "Pairwise Analogous"
        Private Const MEASUREMENT_TYPE_RATINGS_NAME As String = "Ratings"
        'Private Const MEASUREMENT_TYPE_UTILITY_CURVE_NAME As String = "Utility Curve" 'C0188
        Private Const MEASUREMENT_TYPE_REGULAR_UTILITY_CURVE_NAME As String = "Regular Utility Curve" 'C0188
        Private Const MEASUREMENT_TYPE_ADVANCED_UTILITY_CURVE_NAME As String = "Advanced Utility Curve" 'C0188
        Private Const MEASUREMENT_TYPE_STEP_FUNCTION_NAME As String = "Step Function" 'C0026
        Private Const MEASUREMENT_TYPE_DIRECT_NAME As String = "Direct" 'C0026

        Private mPrjManager As clsProjectManager
        Private mReportType As CanvasReportType
        Private mDataSet As DataSet

        Private mInDepth As Boolean = True
        Private mFullAltPath As Boolean = True

        Private mIncludeIdealAlt As Boolean = False
        Private mShowIdelAlt As Boolean = False

        Private mWRTNodeID As Integer

        Private mIncludeCombinedInPivot As Boolean

        Private mReportCommentType As ReportCommentType

        Private mGroupType As GroupReportType

        <NonSerializedAttribute()> Private mWorker As ComponentModel.BackgroundWorker = Nothing 'C0771

        Public Event UserPrioritiesProgress(ByVal CurrentUser As Integer, ByVal TotalUsers As Integer, ByVal CurrentUserName As String) 'C0771

        'Private mAltsIDs As List(Of Integer) 'C0816
        'Private mWRTNodeID As Integer 'C0816

        'Public Property AlternativeIDs() As List(Of Integer) 'C0816
        '    Get
        '        Return mAltsIDs
        '    End Get
        '    Set(ByVal value As List(Of Integer))
        '        mAltsIDs = value
        '    End Set
        'End Property

        Public Property WRTNodeID() As Integer 'C0816
            Get
                Return mWRTNodeID
            End Get
            Set(ByVal value As Integer)
                mWRTNodeID = value
            End Set
        End Property

        Private Function GetUserIDsList(User As clsUser, Group As clsCombinedGroup) As ArrayList
            Dim res As New ArrayList
            If User IsNot Nothing Then
                res.Add(User.UserID)
            Else
                If Group IsNot Nothing Then
                    For Each u As clsUser In Group.UsersList
                        res.Add(u.UserID)
                    Next
                End If
            End If
            Return res
        End Function

        Private Function GetCalulationTarget(User As clsUser, Group As clsCombinedGroup) As clsCalculationTarget
            If User IsNot Nothing Then
                Return New clsCalculationTarget(CalculationTargetType.cttUser, User)
            Else
                If Group IsNot Nothing Then
                    Return New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, Group)
                End If
            End If

            Return Nothing
        End Function

        Private Sub LoadJudgments(User As clsUser, Group As clsCombinedGroup)
            If User IsNot Nothing Then
                ProjectManager.StorageManager.Reader.LoadUserJudgments(User)
            Else
                If Group IsNot Nothing Then
                    For Each u As clsUser In Group.UsersList
                        ProjectManager.StorageManager.Reader.LoadUserJudgments(u)
                    Next
                End If
            End If
        End Sub

        Private Sub LoadData(User As clsUser, Group As clsCombinedGroup)
            If User IsNot Nothing Then
                ProjectManager.StorageManager.Reader.LoadUserData(User)
            Else
                If Group IsNot Nothing Then
                    For Each u As clsUser In Group.UsersList
                        ProjectManager.StorageManager.Reader.LoadUserData(u)
                    Next
                End If
            End If
        End Sub

        Private Sub LoadPermissions(User As clsUser, Group As clsCombinedGroup)
            If User IsNot Nothing Then
                ProjectManager.StorageManager.Reader.LoadUserPermissions(User)
            Else
                If Group IsNot Nothing Then
                    For Each u As clsUser In Group.UsersList
                        ProjectManager.StorageManager.Reader.LoadUserPermissions(u)
                    Next
                End If
            End If
        End Sub

        Public Property CommentType() As ReportCommentType 'C0580
            Get
                Return mReportCommentType
            End Get
            Set(ByVal value As ReportCommentType)
                mReportCommentType = value
            End Set
        End Property

        Public Property ProjectManager() As clsProjectManager
            Get
                Return mPrjManager
            End Get
            Set(ByVal value As clsProjectManager)
                mPrjManager = value
            End Set
        End Property

        Public Property InDepth() As Boolean
            Get
                Return mInDepth
            End Get
            Set(ByVal value As Boolean)
                mInDepth = value
            End Set
        End Property

        Public Property FullAlternativePath() As Boolean
            Get
                Return mFullAltPath
            End Get
            Set(ByVal value As Boolean)
                mFullAltPath = value
            End Set
        End Property

        Public Property IncludeIdealAlternative() As Boolean
            Get
                Return mPrjManager.CalculationsManager.IncludeIdealAlternative
            End Get
            Set(ByVal value As Boolean)
                mIncludeIdealAlt = value
            End Set
        End Property

        Public Property ShowIdealAlternative() As Boolean
            Get
                Return mPrjManager.CalculationsManager.ShowIdealAlternative
            End Get
            Set(ByVal value As Boolean)
                mShowIdelAlt = value
            End Set
        End Property

        Public Property IncludeCombinedInPivot() As Boolean 'C0024
            Get
                Return mIncludeCombinedInPivot
            End Get
            Set(ByVal value As Boolean)
                mIncludeCombinedInPivot = value
            End Set
        End Property

        Private Sub GetFullNodePath(ByVal node As clsNode, ByRef NodePath As String, Optional ByVal NodePathDelimiter As String = NODE_PATH_DELIMITER) 'C0389
            If node Is Nothing Then
                Exit Sub
            End If

            If node.ParentNode Is Nothing Then
                NodePath = node.NodeName + NodePath
            Else
                NodePath = NodePathDelimiter + node.NodeName + NodePath 'C0389
                GetFullNodePath(node.ParentNode, NodePath)
            End If
        End Sub

        Private Sub GetNodePath(OriginalNode As clsNode, ByVal node As clsNode, ByRef NodePath As String, Optional ByVal NodePathDelimiter As String = NODE_PATH_DELIMITER) 'C0389
            If node Is Nothing Then
                Exit Sub
            End If

            If node.ParentNode Is Nothing Then
                NodePath = node.NodeName + NodePath
            Else
                If node IsNot OriginalNode Then
                    NodePath = NodePathDelimiter + node.NodeName + NodePath 'C0389
                End If
                GetNodePath(OriginalNode, node.ParentNode, NodePath)
            End If
        End Sub

        Protected Sub AddNodeToStructure(ByVal node As clsNode)
            If node Is Nothing Then
                Exit Sub
            End If

            Dim NodePath As String = ""
            GetFullNodePath(node, NodePath)

            Dim Row As DataRow

            Row = mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).NewRow()
            Row("NodeID") = node.NodeID
            Row("NodePath") = NodePath
            Row("NodeName") = node.NodeName 'C0210
            Row("ParentNodeID") = node.ParentNodeID 'C0249
            'C0580===
            Select Case CommentType
                Case ReportCommentType.rctComment
                    Row("Comment") = node.Comment
                Case ReportCommentType.rctInfoDoc
                    Row("Comment") = node.InfoDoc
                Case ReportCommentType.rctTag
                    Row("Comment") = If(node.Tag Is Nothing, "", CType(node.Tag, String))
            End Select
            'C0580==
            Select Case node.MeasureType
                Case ECMeasureType.mtPairwise
                    Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_NAME
                Case ECMeasureType.mtPWAnalogous
                    Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_ANALOGOUS_NAME
                Case ECMeasureType.mtPWOutcomes
                    Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_OUTCOMES_NAME
                Case ECMeasureType.mtRatings
                    Row("MeasurementType") = MEASUREMENT_TYPE_RATINGS_NAME
                    'Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve 'C0026 'C0188
                    '    Row("MeasurementType") = MEASUREMENT_TYPE_UTILITY_CURVE_NAME
                Case ECMeasureType.mtRegularUtilityCurve 'C0188
                    Row("MeasurementType") = MEASUREMENT_TYPE_REGULAR_UTILITY_CURVE_NAME
                Case ECMeasureType.mtAdvancedUtilityCurve 'C0188
                    Row("MeasurementType") = MEASUREMENT_TYPE_ADVANCED_UTILITY_CURVE_NAME
                Case ECMeasureType.mtStep 'C0026
                    Row("MeasurementType") = MEASUREMENT_TYPE_STEP_FUNCTION_NAME
                Case ECMeasureType.mtDirect 'C0026
                    Row("MeasurementType") = MEASUREMENT_TYPE_DIRECT_NAME
            End Select
            Row("IsAlternative") = False
            Row("IsCoveringObjective") = node.IsTerminalNode 'C0815
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Rows.Add(Row)
        End Sub

        Protected Sub AddAlternativeToStructure(ByVal alt As clsNode, ByVal CoveringObjective As clsNode)
            If alt Is Nothing Or CoveringObjective Is Nothing Then
                Exit Sub
            End If

            Dim NodePath As String = ""
            If mFullAltPath Then
                GetFullNodePath(CoveringObjective, NodePath)
                NodePath += NODE_PATH_DELIMITER + alt.NodeName
            Else
                NodePath = alt.NodeName
            End If

            Dim Row As DataRow

            Row = mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).NewRow()
            Row("NodeID") = alt.NodeID
            Row("NodePath") = NodePath
            Row("NodeName") = alt.NodeName 'C0210 'C0249
            Row("ParentNodeID") = CoveringObjective.NodeID 'C0249
            'C0580===
            Select Case CommentType
                Case ReportCommentType.rctComment
                    Row("Comment") = alt.Comment
                Case ReportCommentType.rctInfoDoc
                    Row("Comment") = alt.InfoDoc
                Case ReportCommentType.rctTag
                    Row("Comment") = If(alt.Tag Is Nothing, "", CType(alt.Tag, String))
            End Select
            'C0580==
            Row("MeasurementType") = ""
            Row("IsAlternative") = True
            Row("IsCoveringObjective") = False 'C0815
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Rows.Add(Row)
        End Sub

        Protected Sub AddHierarchyStructure(ByVal node As clsNode, Optional ByVal IncludeAlternatives As Boolean = True)
            If node Is Nothing Then
                Exit Sub
            End If

            If Not InDepth Then
                If node.ParentNode Is Nothing Then
                    AddNodeToStructure(node)
                End If
            Else
                AddNodeToStructure(node)
            End If

            If Not node.IsTerminalNode Then
                If Not InDepth Then
                    For Each child As clsNode In node.Children
                        AddNodeToStructure(child)
                    Next
                End If

                For Each child As clsNode In node.Children
                    AddHierarchyStructure(child, IncludeAlternatives)
                Next
            Else
                If IncludeAlternatives Then
                    Dim alt As clsNode
                    For Each alt In node.GetNodesBelow(UNDEFINED_USER_ID)
                        If Not alt Is Nothing Then
                            AddAlternativeToStructure(alt, node)
                        End If
                    Next
                End If
            End If
        End Sub

        Protected Sub AddHierarchyStructureTable(Optional ByVal IncludeAlternatives As Boolean = True)
            mDataSet.Tables.Add(TABLE_NAME_HIERARCHY_STRUCTURE)
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("NodePath", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("NodeName", System.Type.GetType("System.String")) 'C0210
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("ParentNodeID", System.Type.GetType("System.Int32")) 'C0249
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("MeasurementType", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("IsAlternative", System.Type.GetType("System.Boolean"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("IsCoveringObjective", System.Type.GetType("System.Boolean")) 'C0815

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                AddHierarchyStructure(mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), IncludeAlternatives)
            End If
        End Sub

        Private Sub AddNodesToHierarchyTable2(node As clsNode)
            Dim Row As DataRow

            Row = mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE_2).NewRow()

            Dim level As Integer = node.Level
            Row("Level" + level.ToString) = node.NodeName
            Dim nd As clsNode = node.ParentNode
            level -= 1
            While nd IsNot Nothing
                Row("Level" + level.ToString) = nd.NodeName
                nd = nd.ParentNode
                level -= 1
            End While

            mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE_2).Rows.Add(Row) 'SL

            For Each child As clsNode In node.Children
                AddNodesToHierarchyTable2(child)
            Next
        End Sub

        Protected Sub AddHierarchyStructure2Table()
            mDataSet.Tables.Add(TABLE_NAME_HIERARCHY_STRUCTURE_2)
            Dim H As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
            Dim MaxLevel As Integer = H.GetMaxLevel
            For i As Integer = 0 To MaxLevel
                mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE_2).Columns.Add("Level" + i.ToString, System.Type.GetType("System.String"))
            Next

            AddNodesToHierarchyTable2(H.Nodes(0))
        End Sub

        Protected Sub AddAllPairwiseJudgmentsTable(User As clsUser, Group As clsCombinedGroup, ReportType As CanvasReportType) ' D3703
            mDataSet.Tables.Add(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS)
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("UserEmail", System.Type.GetType("System.String")) 'C0964
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("NodeName", System.Type.GetType("System.String")) 'C0966 'L0464
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("Child1Name", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("Child2Name", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("Value", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Columns.Add("MeasurementType", System.Type.GetType("System.String"))

            Dim Row As DataRow

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                If ReportType = CanvasReportType.crtAllJudgments OrElse ((node.IsTerminalNode AndAlso ReportType = CanvasReportType.crtJudgmentsAlts) OrElse (Not node.IsTerminalNode AndAlso ReportType = CanvasReportType.crtJudgmentsObjs)) Then ' D3703 + D3709
                    If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                        For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                            For Each pwJ As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromAllUsers
                                If Not pwJ.IsUndefined Then
                                    If pwJ.UserID >= 0 Then 'C0170
                                        Row = mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).NewRow()
                                        Row("UserName") = ProjectManager.GetUserByID(pwJ.UserID).UserName
                                        Row("UserEmail") = ProjectManager.GetUserByID(pwJ.UserID).UserEMail
                                        Row("NodeID") = child.NodeID
                                        Row("Comment") = pwJ.Comment
                                        Row("Child1Name") = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwJ.FirstNodeID).Name
                                        Row("Child2Name") = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwJ.SecondNodeID).Name
                                        Row("Value") = pwJ.Value * pwJ.Advantage
                                        Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_NAME

                                        mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Rows.Add(Row)
                                    End If
                                End If
                            Next
                        Next
                    Else
                        For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromAllUsers
                            If Not J.IsUndefined Then
                                If J.UserID >= 0 Then
                                    Row = mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).NewRow()
                                    Row("UserName") = ProjectManager.GetUserByID(J.UserID).UserName
                                    Row("UserEmail") = ProjectManager.GetUserByID(J.UserID).UserEMail
                                    Row("NodeID") = node.NodeID
                                    Row("Comment") = J.Comment
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            Dim pwData As clsPairwiseMeasureData = CType(J, clsPairwiseMeasureData)
                                            If node.IsTerminalNode Then
                                                Row("Child1Name") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(pwData.FirstNodeID).NodeName
                                                Row("Child2Name") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(pwData.SecondNodeID).NodeName
                                            Else
                                                Row("Child1Name") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(pwData.FirstNodeID).NodeName
                                                Row("Child2Name") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(pwData.SecondNodeID).NodeName
                                            End If
                                            Row("Value") = pwData.Value * pwData.Advantage
                                            Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_NAME

                                            mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Rows.Add(Row)

                                        Case Else
                                            Dim nonpwData As clsNonPairwiseMeasureData = CType(J, clsNonPairwiseMeasureData)
                                            If Not node.IsTerminalNode Then
                                                Row("Child1Name") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(nonpwData.NodeID).NodeName 'C0978
                                                Row("Value") = nonpwData.SingleValue
                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtDirect
                                                        Row("MeasurementType") = MEASUREMENT_TYPE_DIRECT_NAME
                                                    Case ECMeasureType.mtRatings
                                                        Row("MeasurementType") = MEASUREMENT_TYPE_RATINGS_NAME
                                                    Case ECMeasureType.mtRegularUtilityCurve
                                                        Row("MeasurementType") = MEASUREMENT_TYPE_REGULAR_UTILITY_CURVE_NAME
                                                    Case ECMeasureType.mtAdvancedUtilityCurve
                                                        Row("MeasurementType") = MEASUREMENT_TYPE_ADVANCED_UTILITY_CURVE_NAME
                                                    Case ECMeasureType.mtStep
                                                        Row("MeasurementType") = MEASUREMENT_TYPE_STEP_FUNCTION_NAME
                                                End Select

                                                mDataSet.Tables(TABLE_NAME_ALL_PAIRWISE_JUDGMENTS).Rows.Add(Row)
                                            End If
                                    End Select
                                End If
                            End If
                        Next
                    End If
                End If
            Next
        End Sub

        Protected Sub AddAllJudgmentsTable(User As clsUser, Group As clsCombinedGroup)
            mDataSet.Tables.Add(TABLE_NAME_ALL_JUDGMENTS)
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("UserEmail", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("NodeName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("Child1Name", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("Child2Name", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("Value", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("MeasurementType", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Columns.Add("IsTerminal", System.Type.GetType("System.Boolean"))

            Dim Row As DataRow

            Dim Judgments As List(Of clsCustomMeasureData)

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                For Each J As clsCustomMeasureData In Judgments
                    If Not J.IsUndefined Then
                        If J.UserID >= 0 Then
                            Row = mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).NewRow()
                            Row("UserName") = ProjectManager.GetUserByID(J.UserID).UserName
                            Row("UserEmail") = ProjectManager.GetUserByID(J.UserID).UserEMail
                            Row("NodeName") = node.NodeName
                            Row("Comment") = J.Comment
                            Row("IsTerminal") = node.IsTerminalNode
                            If IsPWMeasurementType(node.MeasureType) Then
                                Dim pwData As clsPairwiseMeasureData = CType(J, clsPairwiseMeasureData)
                                If node.IsTerminalNode Then
                                    Row("Child1Name") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(pwData.FirstNodeID).NodeName
                                    Row("Child2Name") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(pwData.SecondNodeID).NodeName
                                Else
                                    Row("Child1Name") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(pwData.FirstNodeID).NodeName
                                    Row("Child2Name") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(pwData.SecondNodeID).NodeName
                                End If
                                Row("Value") = CStr(pwData.Value * pwData.Advantage)
                                If pwData.Advantage = 0 Then
                                    Row("Value") = "1/1"
                                End If
                                Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_NAME

                                mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Rows.Add(Row)
                                Else
                                    Dim nonpwData As clsNonPairwiseMeasureData = CType(J, clsNonPairwiseMeasureData)
                                If node.IsTerminalNode Then
                                    Row("Child1Name") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(nonpwData.NodeID).NodeName
                                Else
                                    Row("Child1Name") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(nonpwData.NodeID).NodeName
                                End If
                                Select Case node.MeasureType
                                    Case ECMeasureType.mtDirect
                                        Row("MeasurementType") = MEASUREMENT_TYPE_DIRECT_NAME
                                        Row("Value") = nonpwData.SingleValue.ToString
                                    Case ECMeasureType.mtRatings
                                        Row("MeasurementType") = MEASUREMENT_TYPE_RATINGS_NAME
                                        If CType(nonpwData, clsRatingMeasureData).Rating.ID = -1 Then
                                            Row("Value") = CStr(nonpwData.SingleValue)
                                        Else
                                            Row("Value") = CType(nonpwData, clsRatingMeasureData).Rating.Name + " (Value: " + nonpwData.SingleValue.ToString + ")"
                                        End If
                                    Case ECMeasureType.mtRegularUtilityCurve
                                        Row("MeasurementType") = MEASUREMENT_TYPE_REGULAR_UTILITY_CURVE_NAME
                                        Row("Value") = nonpwData.SingleValue.ToString
                                    Case ECMeasureType.mtAdvancedUtilityCurve
                                        Row("MeasurementType") = MEASUREMENT_TYPE_ADVANCED_UTILITY_CURVE_NAME
                                        Row("Value") = nonpwData.SingleValue.ToString
                                    Case ECMeasureType.mtStep
                                        Row("MeasurementType") = MEASUREMENT_TYPE_STEP_FUNCTION_NAME
                                        Row("Value") = nonpwData.SingleValue.ToString
                                End Select

                                mDataSet.Tables(TABLE_NAME_ALL_JUDGMENTS).Rows.Add(Row)
                            End If
                        End If
                    End If
                Next
            Next
        End Sub

        Protected Sub AddHierarchyJudgmentsTable(User As clsUser, Group As clsCombinedGroup)
            mDataSet.Tables.Add(TABLE_NAME_HIERARCHY_JUDGMENTS)
            mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("id", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("UserName", System.Type.GetType("System.String"))
            'mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("NodeName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("Child1Name", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("Child2Name", System.Type.GetType("System.String"))
            'mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("Advantage", System.Type.GetType("System.Int32"))
            'mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("Value", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("Value", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Columns.Add("MeasurementType", System.Type.GetType("System.String"))

            Dim Row As DataRow
            Dim i As Integer = 1

            Dim Judgments As List(Of clsCustomMeasureData)

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                If Not node.IsTerminalNode Then
                    If node.MeasureType = ECMeasureType.mtPairwise Then
                        Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                        For Each J As clsPairwiseMeasureData In Judgments
                            If Not J.IsUndefined Then
                                If J.UserID >= 0 Then 'C0170
                                    Row = mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).NewRow()
                                    Row("id") = i
                                    Row("UserName") = ProjectManager.GetUserByID(J.UserID).UserName
                                    'Row("NodeName") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeName
                                    Row("NodeID") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeID
                                    Row("Child1Name") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.FirstNodeID).NodeName
                                    Row("Child2Name") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.SecondNodeID).NodeName
                                    'Row("Advantage") = J.Advantage
                                    'Row("Value") = J.Value
                                    Row("Value") = J.Value * J.Advantage
                                    Row("Comment") = J.Comment
                                    Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_NAME

                                    mDataSet.Tables(TABLE_NAME_HIERARCHY_JUDGMENTS).Rows.Add(Row)

                                    i += 1
                                End If
                            End If
                        Next
                    End If
                End If
            Next
        End Sub

        Protected Sub AddAltsPairwiseJudgmentsTable(User As clsUser, Group As clsCombinedGroup)
            mDataSet.Tables.Add(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS)
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("id", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("NodeName", System.Type.GetType("System.String")) 'C0219
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("Child1Name", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("Child2Name", System.Type.GetType("System.String"))
            'mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("Advantage", System.Type.GetType("System.Int32"))
            'mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("Value", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("Value", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Columns.Add("MeasurementType", System.Type.GetType("System.String"))

            Dim Row As DataRow
            Dim i As Integer = 1

            Dim Judgments As List(Of clsCustomMeasureData)

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                If node.IsTerminalNode Then
                    If node.MeasureType = ECMeasureType.mtPairwise Then
                        Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                        For Each J As clsPairwiseMeasureData In Judgments
                            If J.UserID >= 0 Then 'C0170
                                If Not J.IsUndefined Then
                                    Row = mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).NewRow()
                                    Row("id") = i
                                    Row("UserName") = ProjectManager.GetUserByID(J.UserID).UserName
                                    Row("NodeName") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeName
                                    Row("Child1Name") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(J.FirstNodeID).NodeName
                                    Row("Child2Name") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(J.SecondNodeID).NodeName
                                    'Row("Advantage") = J.Advantage
                                    'Row("Value") = J.Value
                                    Row("Value") = J.Value * J.Advantage
                                    Row("Comment") = J.Comment
                                    Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_NAME

                                    mDataSet.Tables(TABLE_NAME_ALTS_PAIRWISE_JUDGMENTS).Rows.Add(Row)

                                    i += 1
                                End If
                            End If
                        Next
                    End If
                End If
            Next
        End Sub

        Protected Sub AddAltsRatingsJudgmentsTable(User As clsUser, Group As clsCombinedGroup, ReportType As CanvasReportType) ' D3703
            mDataSet.Tables.Add(TABLE_NAME_ALTS_RATINGS_JUDGMENTS)
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("id", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("UserEmail", System.Type.GetType("System.String")) 'C0964
            'mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("CovObjName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("CovObjID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("AltName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("RatingName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("Value", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Columns.Add("MeasurementType", System.Type.GetType("System.String"))

            Dim Row As DataRow
            Dim i As Integer = 1

            Dim Judgments As List(Of clsCustomMeasureData)

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                If ReportType = CanvasReportType.crtAllJudgments OrElse ((node.IsTerminalNode AndAlso ReportType = CanvasReportType.crtJudgmentsAlts) OrElse (Not node.IsTerminalNode AndAlso ReportType = CanvasReportType.crtJudgmentsObjs)) Then ' D3703 + D3709
                    'If node.IsTerminalNode Then 'C0813
                    If node.MeasureType = ECMeasureType.mtRatings Then
                        Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                        For Each J As clsRatingMeasureData In Judgments
                            If Not J.IsUndefined Then
                                'If J.UserID <> COMBINED_USER_ID Then 'C0170
                                If J.UserID >= 0 Then 'C0170
                                    Row = mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).NewRow()
                                    Row("id") = i
                                    Row("UserName") = ProjectManager.GetUserByID(J.UserID).UserName
                                    Row("UserEmail") = ProjectManager.GetUserByID(J.UserID).UserEMail 'C0964
                                    'Row("CovObjName") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.CoveringObjectiveID).NodeName
                                    Row("CovObjID") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeID
                                    'Row("AltName") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(J.NodeID).NodeName 'C0813
                                    'C0813===
                                    If node.IsTerminalNode Then
                                        Row("AltName") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(J.NodeID).NodeName
                                    Else
                                        Row("AltName") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName
                                    End If
                                    'C0813==
                                    Row("RatingName") = J.Rating.Name
                                    Row("Value") = J.Rating.Value
                                    Row("Comment") = J.Comment
                                    Row("MeasurementType") = MEASUREMENT_TYPE_RATINGS_NAME

                                    mDataSet.Tables(TABLE_NAME_ALTS_RATINGS_JUDGMENTS).Rows.Add(Row)
                                    i += 1
                                End If

                            End If
                        Next
                    End If
                End If
                'End If 'C0813
            Next
        End Sub

        Protected Sub AddAltsNonRatingsJudgmentsTable(User As clsUser, Group As clsCombinedGroup, ReportType As CanvasReportType)   'C0188 + D3703
            mDataSet.Tables.Add(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS)
            mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Columns.Add("id", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Columns.Add("UserEmail", System.Type.GetType("System.String")) 'C0964
            mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Columns.Add("CovObjID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Columns.Add("AltName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Columns.Add("Value", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Columns.Add("MeasurementType", System.Type.GetType("System.String"))

            Dim Row As DataRow
            Dim i As Integer = 1

            Dim Judgments As List(Of clsCustomMeasureData)

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                'If node.IsTerminalNode Then 'C0813
                If (node.MeasureType <> ECMeasureType.mtRatings) And (node.MeasureType <> ECMeasureType.mtPWOutcomes) And Not IsPWMeasurementType(node.MeasureType) Then
                    If ReportType = CanvasReportType.crtAllJudgments OrElse ((node.IsTerminalNode AndAlso ReportType = CanvasReportType.crtJudgmentsAlts) OrElse (Not node.IsTerminalNode AndAlso ReportType = CanvasReportType.crtJudgmentsObjs)) Then ' D3703 + D3709
                        Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                        For Each J As clsNonPairwiseMeasureData In Judgments
                            If Not J.IsUndefined Then
                                'If J.UserID <> COMBINED_USER_ID Then 'C0170
                                If J.UserID >= 0 Then 'C0170
                                    Row = mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).NewRow()
                                    Row("id") = i
                                    Row("UserName") = ProjectManager.GetUserByID(J.UserID).UserName
                                    Row("UserEmail") = ProjectManager.GetUserByID(J.UserID).UserEMail 'C0964
                                    Row("CovObjID") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.ParentNodeID).NodeID
                                    'Row("AltName") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(J.NodeID).NodeName 'C0813
                                    'C0813===
                                    If node.IsTerminalNode Then
                                        Row("AltName") = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(J.NodeID).NodeName
                                    Else
                                        Row("AltName") = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName
                                    End If
                                    'C0813==
                                    Row("Value") = CSng(J.ObjectValue)
                                    Row("Comment") = J.Comment

                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtAdvancedUtilityCurve
                                            Row("MeasurementType") = MEASUREMENT_TYPE_ADVANCED_UTILITY_CURVE_NAME
                                        Case ECMeasureType.mtRegularUtilityCurve
                                            Row("MeasurementType") = MEASUREMENT_TYPE_REGULAR_UTILITY_CURVE_NAME
                                        Case ECMeasureType.mtStep
                                            Row("MeasurementType") = MEASUREMENT_TYPE_STEP_FUNCTION_NAME
                                        Case ECMeasureType.mtDirect
                                            Row("MeasurementType") = MEASUREMENT_TYPE_DIRECT_NAME
                                    End Select

                                    mDataSet.Tables(TABLE_NAME_ALTS_NON_RATINGS_JUDGMENTS).Rows.Add(Row)
                                    i += 1
                                End If

                            End If
                        Next
                    End If
                End If
                'End If 'C0813
            Next
        End Sub

        'Protected Sub AddNodeWithPriorities(ByVal node As clsNode) 'C0159
        'Protected Sub AddNodeWithPriorities(ByVal UserID As Integer, ByVal node As clsNode) 'C0159 'C0556
        Protected Sub AddNodeWithPriorities(ByVal CalcTarget As clsCalculationTarget, ByVal node As clsNode) 'C0556
            If node Is Nothing Or CalcTarget Is Nothing Then 'C0556
                Exit Sub
            End If

            Dim NodePath As String = ""
            GetFullNodePath(node, NodePath)

            Dim Row As DataRow

            Row = mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).NewRow()
            Row("NodeID") = node.NodeID
            Row("NodePath") = NodePath
            Row("NodeName") = node.NodeName 'C0210
            Row("ParentNodeID") = node.ParentNodeID 'C0210
            'Row("Comment") = node.Comment 'C0577
            'Row("Comment") = node.InfoDoc 'C0577 'C0580
            'C0580===
            Select Case CommentType
                Case ReportCommentType.rctComment
                    Row("Comment") = node.Comment
                Case ReportCommentType.rctInfoDoc
                    Row("Comment") = node.InfoDoc
                Case ReportCommentType.rctTag
                    Row("Comment") = If(node.Tag Is Nothing, "", CType(node.Tag, String))
            End Select
            'C0580==
            'Row("LocalPriority") = node.LocalPriority 'C0159
            'Row("LocalPriority") = node.LocalPriority(UserID) 'C0159 'C0551
            'Row("LocalPriority") = node.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(UserID))) 'C0551 'C0556
            Row("LocalPriority") = node.LocalPriority(CalcTarget) 'C0556
            Row("GlobalPriority") = node.WRTGlobalPriority
            Row("IsAlternative") = False
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Rows.Add(Row)
        End Sub

        'Protected Sub AddAlternativeWithPriorities(ByVal alt As clsNode, ByVal CoveringObjective As clsNode) 'C0159
        'Protected Sub AddAlternativeWithPriorities(ByVal UserID As Integer, ByVal alt As clsNode, ByVal CoveringObjective As clsNode) 'C0159 'C0556
        Protected Sub AddAlternativeWithPriorities(ByVal CalcTarget As clsCalculationTarget, ByVal alt As clsNode, ByVal CoveringObjective As clsNode) 'C0556
            If alt Is Nothing Or CoveringObjective Is Nothing Then
                Exit Sub
            End If

            Dim NodePath As String = ""
            If mFullAltPath Then
                GetFullNodePath(CoveringObjective, NodePath)
                NodePath += NODE_PATH_DELIMITER + alt.NodeName
            Else
                NodePath = alt.NodeName
            End If

            Dim Row As DataRow

            Row = mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).NewRow()
            Row("NodeID") = alt.NodeID
            Row("NodePath") = NodePath
            Row("NodeName") = alt.NodeName 'C0210
            Row("ParentNodeID") = CoveringObjective.NodeID 'C0210
            'Row("Comment") = alt.Comment 'C0577
            'Row("Comment") = alt.InfoDoc 'C0577 'C0580
            'C0580===
            Select Case CommentType
                Case ReportCommentType.rctComment
                    Row("Comment") = alt.Comment
                Case ReportCommentType.rctInfoDoc
                    Row("Comment") = alt.InfoDoc
                Case ReportCommentType.rctTag
                    Row("Comment") = If(alt.Tag Is Nothing, "", CType(alt.Tag, String))
            End Select
            'C0580==
            Row("LocalPriority") = CoveringObjective.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID) 'C0556

            Row("GlobalPriority") = 0.0
            Row("IsAlternative") = True
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Rows.Add(Row)
        End Sub

        Protected Sub AddStructureWithPriorities(ByVal CalcTarget As clsCalculationTarget, ByVal node As clsNode, Optional ByVal IncludeAlternatives As Boolean = True) 'C0556
            If node Is Nothing Then
                Exit Sub
            End If

            If Not InDepth Then
                If node.ParentNode Is Nothing Then
                    AddNodeWithPriorities(CalcTarget, node) 'C0556
                End If
            Else
                AddNodeWithPriorities(CalcTarget, node) 'C0556
            End If

            If Not node.IsTerminalNode Then
                If Not InDepth Then
                    For Each child As clsNode In node.Children
                        AddNodeWithPriorities(CalcTarget, child) 'C0556
                    Next
                End If

                For Each child As clsNode In node.Children
                    AddStructureWithPriorities(CalcTarget, child, IncludeAlternatives) 'C0556
                Next
            Else
                If IncludeAlternatives Then
                    Dim alt As clsNode
                    For Each alt In node.GetVisibleNodesBelow(CalcTarget.GetUserID) 'C0786
                        If Not alt Is Nothing Then
                            AddAlternativeWithPriorities(CalcTarget, alt, node) 'C0556
                        End If
                    Next
                End If
            End If
        End Sub

        Protected Sub AddStructureWithPrioritiesTable(User As clsUser, Group As clsCombinedGroup, Optional ByVal IncludeAlternatives As Boolean = True) 'C0159
            mDataSet.Tables.Add(TABLE_NAME_STRUCTURE_WITH_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Columns.Add("NodePath", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Columns.Add("NodeName", System.Type.GetType("System.String")) 'C0210
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Columns.Add("ParentNodeID", System.Type.GetType("System.Int32")) 'C0210
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Columns.Add("LocalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Columns.Add("GlobalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_STRUCTURE_WITH_PRIORITIES).Columns.Add("IsAlternative", System.Type.GetType("System.Boolean"))

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
                AddStructureWithPriorities(CT, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), IncludeAlternatives) 'C0556
            End If
        End Sub

        Protected Sub AddAltWithOverallPriority(ByVal node As clsNode)
            If node Is Nothing Then
                Exit Sub
            End If

            Dim NodePath As String = ""
            GetFullNodePath(node, NodePath)

            Dim Row As DataRow

            Row = mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).NewRow()
            Row("NodeID") = node.NodeID
            Row("NodePath") = NodePath
            'Row("Comment") = node.Comment 'C0577
            'Row("Comment") = node.InfoDoc 'C0577 'C0580
            'C0580===
            Select Case CommentType
                Case ReportCommentType.rctComment
                    Row("Comment") = node.Comment
                Case ReportCommentType.rctInfoDoc
                    Row("Comment") = node.InfoDoc
                Case ReportCommentType.rctTag
                    Row("Comment") = If(node.Tag Is Nothing, "", CType(node.Tag, String))
            End Select
            'C0580==

            If node.IsTerminalNode Then
                If IncludeIdealAlternative Then
                    Dim sum As Single = 0
                    For Each alt As clsNode In node.Hierarchy.TerminalNodes
                        sum += alt.WRTGlobalPriority
                    Next
                    If ShowIdealAlternative Then
                        sum += node.Hierarchy.ProjectManager.CalculationsManager.GetIdealGlobalPriority
                    End If

                    'Row("GlobalPriority") = node.WRTGlobalPriority / sum 'C0657
                    'C0657===
                    If sum = 0 Then
                        Row("GlobalPriority") = node.WRTGlobalPriority
                    Else
                        Row("GlobalPriority") = node.WRTGlobalPriority / sum
                    End If
                    'C0657==
                Else
                    Row("GlobalPriority") = node.WRTGlobalPriority
                End If
            Else
                Row("GlobalPriority") = 0
            End If

            Row("IsTerminalNode") = node.IsTerminalNode
            mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).Rows.Add(Row)
        End Sub

        Protected Sub AddAltsHierarchyWithPriorities(ByVal node As clsNode)
            If node Is Nothing Then
                Exit Sub
            End If

            If Not InDepth Then
                If node.ParentNode Is Nothing Then
                    AddAltWithOverallPriority(node)
                End If
            Else
                AddAltWithOverallPriority(node)
            End If

            If Not node.IsTerminalNode Then
                If Not InDepth Then
                    For Each child As clsNode In node.Children
                        AddAltWithOverallPriority(child)
                    Next
                End If

                For Each child As clsNode In node.Children
                    AddAltsHierarchyWithPriorities(child)
                Next
            End If
        End Sub

        Protected Sub AddOverallAlternativesPriorities()
            mDataSet.Tables.Add(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).Columns.Add("NodePath", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).Columns.Add("Comment", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).Columns.Add("GlobalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).Columns.Add("IsTerminalNode", System.Type.GetType("System.Boolean"))

            If mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).Nodes.Count > 0 Then
                For Each alt As clsNode In mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).Nodes
                    If alt.ParentNode Is Nothing Then
                        AddAltsHierarchyWithPriorities(alt)
                    End If
                Next

                If IncludeIdealAlternative And ShowIdealAlternative Then
                    Dim Row As DataRow

                    Row = mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).NewRow()
                    Row("NodeID") = IDEAL_ALTERNATIVE_ID
                    Row("NodePath") = IDEAL_ALTERNATIVE_NAME
                    Row("Comment") = ""

                    Dim sum As Single = 0
                    For Each alt As clsNode In mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).TerminalNodes
                        sum += alt.WRTGlobalPriority
                    Next
                    sum += ProjectManager.CalculationsManager.GetIdealGlobalPriority

                    If sum = 0 Then
                        Row("GlobalPriority") = ProjectManager.CalculationsManager.GetIdealGlobalPriority
                    Else
                        Row("GlobalPriority") = ProjectManager.CalculationsManager.GetIdealGlobalPriority / sum
                    End If

                    Row("IsTerminalNode") = True
                    mDataSet.Tables(TABLE_NAME_OVERALL_ALTERNATIVES_PRIORITIES).Rows.Add(Row)
                End If
            End If
        End Sub

        ' D3703 ===
        Protected Sub AddAllJudgmentsTables(User As clsUser, Group As clsCombinedGroup, ReportType As CanvasReportType)
            AddAllPairwiseJudgmentsTable(User, Group, ReportType)
            AddAltsRatingsJudgmentsTable(User, Group, ReportType)
            AddAltsNonRatingsJudgmentsTable(User, Group, ReportType)
            ' D3703 ==
        End Sub

        'Protected Sub AddPivotPriorities() 'C0024 'C0159
        Protected Sub AddPivotPriorities(ByVal UserID As Integer) 'C0159
            mDataSet.Tables.Add(TABLE_NAME_PIVOT_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Columns.Add("NodeName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Columns.Add("NodePath", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Columns.Add("NodeLocalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Columns.Add("NodeGlobalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Columns.Add("AltName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Columns.Add("AltLocalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Columns.Add("AltGlobalPriority", System.Type.GetType("System.Single"))

            If mPrjManager Is Nothing Then
                Exit Sub
            End If

            'ProjectManager.StorageManager.Reader.LoadUserJudgments(Nothing, False, -1) 'C0194 'C0773

            If (mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0) Then
                Dim NodePath As String
                Dim oldUserID As Integer
                Dim oldSynthMode As ECSynthesisMode 'C0027
                Dim Row As DataRow
                Dim idealOn As Boolean = ProjectManager.CalculationsManager.IncludeIdealAlternative 'C0027
                ProjectManager.CalculationsManager.IncludeIdealAlternative = False 'C0027

                Dim i As Integer = 0 'C0773

                'mPrjManager.AddCombinedUser() 'C0552
                For Each user As clsUser In mPrjManager.UsersList

                    'C0773===
                    If Worker IsNot Nothing Then
                        Worker.ReportProgress(i / ProjectManager.UsersList.Count * 100, "Processing user: " + user.UserName + " (" + user.UserEMail + ")")
                    End If
                    i += 1

                    Dim count As Integer
                    Select Case ProjectManager.StorageManager.StorageType
                        Case ECModelStorageType.mstCanvasStreamDatabase
                            count = ProjectManager.StorageManager.Reader.LoadUserJudgments(user)
                    End Select
                    'C0773==


                    oldUserID = mPrjManager.UserID
                    oldSynthMode = mPrjManager.CalculationsManager.SynthesisMode 'C0027
                    mPrjManager.UserID = user.UserID
                    mPrjManager.CalculationsManager.SynthesisMode = ECSynthesisMode.smDistributive 'C0027
                    'mPrjManager.CalculationsManager.Calculate(mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), False, Nothing, user.UserID = COMBINED_USER_ID, True) 'C0159
                    'mPrjManager.CalculationsManager.Calculate(UserID, mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), False, Nothing, user.UserID = COMBINED_USER_ID, True) 'C0159 'C0551
                    'C0551===
                    'If user.UserID = COMBINED_USER_ID Then 'C0555
                    If IsCombinedUserID(user.UserID) Then 'C0555
                        'mPrjManager.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup), mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy) 'C0664
                        mPrjManager.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetCombinedGroupByUserID(user.UserID)), mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy) 'C0664
                    Else
                        mPrjManager.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(user.UserID)), mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)
                    End If
                    'C0551==
                    'mPrjManager.UserID = oldUserID 'C0027

                    'For Each CoveringObjective As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes.Clone 'C0385
                    For Each CoveringObjective As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes 'C0385
                        'For Each alt As clsNode In CoveringObjective.GetNodesBelow.Clone 'C0385
                        'For Each alt As clsNode In CoveringObjective.GetNodesBelow 'C0385 'C0450
                        'For Each alt As clsNode In CoveringObjective.GetNodesBelow(UserID) 'C0450 'C0773

                        'For Each alt As clsNode In CoveringObjective.GetNodesBelow(user.UserID) 'C0773 'C0786
                        For Each alt As clsNode In CoveringObjective.GetVisibleNodesBelow(user.UserID) 'C0786
                            NodePath = CoveringObjective.NodeName
                            GetFullNodePath(CoveringObjective, NodePath)

                            Row = mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).NewRow()
                            Row("UserName") = user.UserName
                            Row("NodeName") = CoveringObjective.NodeName
                            Row("NodePath") = NodePath
                            'Row("NodeLocalPriority") = CoveringObjective.LocalPriority 'C0159
                            'Row("NodeLocalPriority") = CoveringObjective.LocalPriority(UserID) 'C0159 'C0551
                            'C0551===
                            'If user.UserID = COMBINED_USER_ID Then 'C0555
                            If IsCombinedUserID(user.UserID) Then 'C0555
                                'Row("NodeLocalPriority") = CoveringObjective.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)) 'C0551 'C0664
                                Row("NodeLocalPriority") = CoveringObjective.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetCombinedGroupByUserID(user.UserID))) 'C0664
                            Else
                                Row("NodeLocalPriority") = CoveringObjective.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(user.UserID))) 'C0551
                            End If
                            'C0551==
                            Row("NodeGlobalPriority") = CoveringObjective.WRTGlobalPriority
                            Row("AltName") = alt.NodeName
                            'Row("AltLocalPriority") = CoveringObjective.Judgments.Weights(CoveringObjective.GetChildIndexByID(alt.NodeID)) 'C0128
                            'Row("AltLocalPriority") = CoveringObjective.Judgments.Weights(alt.NodeID) 'C0128 'C0159

                            'Row("AltLocalPriority") = CoveringObjective.Judgments.Weights.GetUserWeights(UserID).GetWeightValueByNodeID(alt.NodeID) 'C0159 'C0177
                            'Row("AltLocalPriority") = CoveringObjective.Judgments.Weights.GetUserWeights(UserID, ProjectManager.CalculationsManager.SynthesisMode).GetWeightValueByNodeID(alt.NodeID) 'C0177 'C0338
                            'Row("AltLocalPriority") = CoveringObjective.Judgments.Weights.GetUserWeights(UserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID) 'C0338 'C0773
                            Row("AltLocalPriority") = CoveringObjective.Judgments.Weights.GetUserWeights(UserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID) 'C0773

                            Row("AltGlobalPriority") = alt.WRTGlobalPriority
                            mDataSet.Tables(TABLE_NAME_PIVOT_PRIORITIES).Rows.Add(Row)
                        Next
                    Next

                    'C0773===
                    If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> user.UserID) Then
                        ProjectManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, user.UserID, , True)
                    End If
                    'C0773==

                    mPrjManager.UserID = oldUserID 'C0027
                    mPrjManager.CalculationsManager.SynthesisMode = oldSynthMode 'C0027
                Next

                'C0773===
                If Worker IsNot Nothing Then
                    Worker.ReportProgress(100, "Done!")
                End If
                'C0773==

                ProjectManager.CalculationsManager.IncludeIdealAlternative = idealOn 'C0027
            End If
        End Sub

        Protected Sub AddUserPermissions(User As clsUser, Group As clsGroup) 'C0127
            mDataSet.Tables.Add(TABLE_NAME_USER_PERMISSIONS)
            mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).Columns.Add("UserID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).Columns.Add("NodeName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).Columns.Add("NodePath", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).Columns.Add("AltName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).Columns.Add("Value", System.Type.GetType("System.Boolean"))

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)

            If (mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).Nodes.Count > 0) And
                (mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0) Then

                Dim path As String = ""

                For Each UserID As Integer In UserIDs
                    Dim u As clsUser = ProjectManager.GetUserByID(UserID)
                    If u IsNot Nothing Then
                        For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes
                            For Each alt As clsNode In node.AllowedNodesBelow(UserID)
                                Dim Row As DataRow

                                Row = mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).NewRow()
                                Row("UserID") = u.UserID
                                Row("UserName") = u.UserName
                                Row("NodeID") = node.NodeID
                                Row("NodeName") = node.NodeName
                                path = node.NodeName
                                GetFullNodePath(node, path)
                                Row("NodePath") = path
                                Row("AltName") = alt.NodeName
                                Row("Value") = True
                                mDataSet.Tables(TABLE_NAME_USER_PERMISSIONS).Rows.Add(Row)
                            Next
                        Next
                    End If
                Next
            End If
        End Sub

        Protected Sub AddObjectivesListTable() 'C0223
            mDataSet.Tables.Add(TABLE_NAME_OBJECTIVES_LIST)
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_LIST).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_LIST).Columns.Add("NodeName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_LIST).Columns.Add("ParentNodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_LIST).Columns.Add("HasInfoDoc", System.Type.GetType("System.Boolean"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_LIST).Columns.Add("MeasurementType", System.Type.GetType("System.String"))

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes
                    Dim Row As DataRow
                    Row = mDataSet.Tables(TABLE_NAME_OBJECTIVES_LIST).NewRow()
                    Row("NodeID") = node.NodeID
                    Row("NodeName") = node.NodeName
                    Row("ParentNodeID") = node.ParentNodeID
                    Row("HasInfoDoc") = node.InfoDoc <> ""
                    Select Case node.MeasureType
                        Case ECMeasureType.mtPairwise
                            Row("MeasurementType") = MEASUREMENT_TYPE_PAIRWISE_NAME
                        Case ECMeasureType.mtRatings
                            Row("MeasurementType") = MEASUREMENT_TYPE_RATINGS_NAME
                        Case ECMeasureType.mtAdvancedUtilityCurve
                            Row("MeasurementType") = MEASUREMENT_TYPE_ADVANCED_UTILITY_CURVE_NAME
                        Case ECMeasureType.mtRegularUtilityCurve
                            Row("MeasurementType") = MEASUREMENT_TYPE_REGULAR_UTILITY_CURVE_NAME
                        Case ECMeasureType.mtStep
                            Row("MeasurementType") = MEASUREMENT_TYPE_STEP_FUNCTION_NAME
                        Case ECMeasureType.mtDirect
                            Row("MeasurementType") = MEASUREMENT_TYPE_DIRECT_NAME
                    End Select
                    mDataSet.Tables(TABLE_NAME_OBJECTIVES_LIST).Rows.Add(Row)
                Next
            End If
        End Sub

        Protected Sub AddAlternativesListTable() 'C0223
            mDataSet.Tables.Add(TABLE_NAME_ALTERNATIVES_LIST)
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).Columns.Add("AltID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).Columns.Add("AltName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).Columns.Add("HasInfoDoc", System.Type.GetType("System.Boolean"))
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).Columns.Add("Comment", System.Type.GetType("System.String")) 'L0193

            If mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).TerminalNodes.Count > 0 Then
                For Each alt As clsNode In mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).TerminalNodes
                    Dim Row As DataRow
                    Row = mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).NewRow()
                    Row("AltID") = alt.NodeID
                    Row("AltName") = alt.NodeName
                    Row("HasInfoDoc") = alt.InfoDoc <> ""
                    'L0193 ==
                    Select Case CommentType
                        Case ReportCommentType.rctComment
                            Row("Comment") = alt.Comment
                        Case ReportCommentType.rctInfoDoc
                            Row("Comment") = alt.InfoDoc
                        Case ReportCommentType.rctTag
                            Row("Comment") = If(alt.Tag Is Nothing, "", CType(alt.Tag, String))
                    End Select
                    'L0193 ===
                    'Row("Comment") = alt.InfoDoc 'L0193
                    mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).Rows.Add(Row)
                Next
            End If
        End Sub

        Protected Sub AddShortAlternativesListTable() 'C0573
            mDataSet.Tables.Add(TABLE_NAME_ALTERNATIVES_LIST)
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).Columns.Add("AltID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).Columns.Add("AltName", System.Type.GetType("System.String"))

            If mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).TerminalNodes.Count > 0 Then
                For Each alt As clsNode In mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).TerminalNodes
                    Dim Row As DataRow
                    Row = mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).NewRow()
                    Row("AltID") = alt.NodeID
                    Row("AltName") = alt.NodeName
                    mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LIST).Rows.Add(Row)
                Next
            End If
        End Sub

        Protected Sub AddObjectiveWithPriorities(ByVal CalcTarget As clsCalculationTarget, ByVal node As clsNode) 'C0573
            If node Is Nothing Or CalcTarget Is Nothing Then
                Exit Sub
            End If

            Dim Row As DataRow
            Row = mDataSet.Tables(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES).NewRow()
            Row("NodeID") = node.NodeID
            Row("NodeName") = node.NodeName
            Row("ParentNodeID") = node.ParentNodeID
            Row("LocalPriority") = node.LocalPriority(CalcTarget)
            Row("UserID") = CalcTarget.GetUserID

            'C0583===
            Dim JA As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ECSynthesisMode.smDistributive, mPrjManager)
            Row("CanShowGlobalWRTResults") = JA.CanShowIndividualResults(CalcTarget.GetUserID, node)
            'C0583==

            mDataSet.Tables(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES).Rows.Add(Row)
        End Sub

        Protected Sub AddObjectivesWithPriorities(ByVal CalcTarget As clsCalculationTarget, ByVal node As clsNode) 'C0573
            If node Is Nothing Then
                Exit Sub
            End If

            If Not InDepth Then
                If node.ParentNode Is Nothing Then
                    AddObjectiveWithPriorities(CalcTarget, node)
                End If
            Else
                AddObjectiveWithPriorities(CalcTarget, node)
            End If

            If Not node.IsTerminalNode Then
                If Not InDepth Then
                    For Each child As clsNode In node.Children
                        AddObjectiveWithPriorities(CalcTarget, child)
                    Next
                End If

                For Each child As clsNode In node.Children
                    AddObjectivesWithPriorities(CalcTarget, child)
                Next
            End If
        End Sub

        Protected Sub AddObjectivesWithPrioritiesTable(User As clsUser, Group As clsCombinedGroup)
            mDataSet.Tables.Add(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES).Columns.Add("NodeName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES).Columns.Add("ParentNodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES).Columns.Add("LocalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES).Columns.Add("UserID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_OBJECTIVES_WITH_PRIORITIES).Columns.Add("CanShowGlobalWRTResults", System.Type.GetType("System.Boolean")) 'C0583

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                Dim CalcTarget As clsCalculationTarget = GetCalulationTarget(User, Group)

                AddObjectivesWithPriorities(CalcTarget, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0))
            End If
        End Sub

        Protected Sub AddAlternativesLocalPrioritiesTable(User As clsUser, Group As clsCombinedGroup)
            mDataSet.Tables.Add(TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES).Columns.Add("UserID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES).Columns.Add("CovObjID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES).Columns.Add("AltID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES).Columns.Add("AltLocalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES).Columns.Add("AltLocalPriorityIdeal", System.Type.GetType("System.Single")) 'C0724

            Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
            Dim UserID As Integer = CT.GetUserID

            Dim Alts As List(Of clsNode)
            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes
                    Alts = node.GetVisibleNodesBelow(UserID) 'C0786
                    For Each alt As clsNode In Alts
                        Dim Row As DataRow
                        Row = mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES).NewRow()
                        Row("UserID") = UserID
                        Row("CovObjID") = node.NodeID
                        Row("AltID") = alt.NodeID
                        Row("AltLocalPriority") = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smDistributive, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID)
                        Row("AltLocalPriorityIdeal") = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID)
                        mDataSet.Tables(TABLE_NAME_ALTERNATIVES_LOCAL_PRIORITIES).Rows.Add(Row)
                    Next
                Next
            End If
        End Sub

        Protected Sub AddUsersListTable() 'C0223
            mDataSet.Tables.Add(TABLE_NAME_USERS_LIST)
            mDataSet.Tables(TABLE_NAME_USERS_LIST).Columns.Add("UserID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USERS_LIST).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USERS_LIST).Columns.Add("UserEMail", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USERS_LIST).Columns.Add("UserType", System.Type.GetType("System.Int32"))

            For Each U As clsUser In mPrjManager.UsersList
                Dim Row As DataRow
                Row = mDataSet.Tables(TABLE_NAME_USERS_LIST).NewRow()
                Row("UserID") = U.UserID
                Row("UserName") = U.UserName
                Row("UserEmail") = U.UserEMail
                If U.UserID >= 0 Then
                    Row("UserType") = ECUserType.utNormal
                Else
                    'If (U.UserID = COMBINED_USER_ID) Or (U.UserID <= COMBINED_GROUPS_USERS_START_ID) Then 'C0555
                    If IsCombinedUserID(U.UserID) Then 'C0555
                        Row("UserType") = ECUserType.utCombined
                    Else
                        Row("UserType") = ECUserType.utDataInstance
                    End If
                End If
                mDataSet.Tables(TABLE_NAME_USERS_LIST).Rows.Add(Row)
            Next
        End Sub

        Protected Sub AddUserObjectivesPrioritiesTable(ByVal AUserID As Integer) 'C0223
            mDataSet.Tables.Add(TABLE_NAME_USER_OBJECTIVES_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_USER_OBJECTIVES_PRIORITIES).Columns.Add("UserID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_OBJECTIVES_PRIORITIES).Columns.Add("ObjectiveID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_OBJECTIVES_PRIORITIES).Columns.Add("ObjectiveLocalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_USER_OBJECTIVES_PRIORITIES).Columns.Add("ObjectiveGlobalPriority", System.Type.GetType("System.Single"))

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes
                    Dim Row As DataRow
                    Row = mDataSet.Tables(TABLE_NAME_USER_OBJECTIVES_PRIORITIES).NewRow()
                    Row("UserID") = AUserID 'C0225
                    Row("ObjectiveID") = node.NodeID
                    'Row("ObjectiveLocalPriority") = node.LocalPriority(AUserID) 'C0551
                    Row("ObjectiveLocalPriority") = node.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(AUserID))) 'C0551
                    Row("ObjectiveGlobalPriority") = node.WRTGlobalPriority 'C0225
                    mDataSet.Tables(TABLE_NAME_USER_OBJECTIVES_PRIORITIES).Rows.Add(Row)
                Next
            End If
        End Sub

        Protected Sub AddUserAlternativesPrioritiesTable(ByVal AUserID As Integer) 'C0223
            mDataSet.Tables.Add(TABLE_NAME_USER_ALTERNATIVES_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_PRIORITIES).Columns.Add("UserID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_PRIORITIES).Columns.Add("CoveringObjectiveID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_PRIORITIES).Columns.Add("AlternativeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_PRIORITIES).Columns.Add("AlternativeLocalPriority", System.Type.GetType("System.Single"))

            Dim allowedAlts As List(Of clsNode) 'C0384
            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                'For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes.Clone 'C0385
                For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes 'C0385
                    'allowedAlts = CType(node.UserPermissions(AUserID).SpecialPermissions, clsNodePermissions).AllowedNodesBelow 'C0903
                    allowedAlts = node.AllowedNodesBelow(AUserID) 'C0903
                    For Each alt As clsNode In mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).TerminalNodes
                        Dim Row As DataRow
                        Row = mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_PRIORITIES).NewRow()
                        Row("UserID") = AUserID
                        Row("CoveringObjectiveID") = node.NodeID
                        Row("AlternativeID") = alt.NodeID
                        If allowedAlts.Contains(alt) Then
                            'Row("AlternativeLocalPriority") = node.Judgments.Weights.GetUserWeights(AUserID, ProjectManager.CalculationsManager.SynthesisMode).GetWeightValueByNodeID(alt.NodeID) 'C0338
                            Row("AlternativeLocalPriority") = node.Judgments.Weights.GetUserWeights(AUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID) 'C0338
                        Else
                            Row("AlternativeLocalPriority") = -1
                        End If
                        mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_PRIORITIES).Rows.Add(Row)
                    Next
                Next
            End If
        End Sub

        Protected Sub AddUserAltsJudgmentsTable(ByVal AUserID As Integer) 'C0223
            mDataSet.Tables.Add(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS)
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Columns.Add("UserID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Columns.Add("WRTNodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Columns.Add("Advantage", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Columns.Add("FirstNodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Columns.Add("SecondNodeID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Columns.Add("VerbalValue", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Columns.Add("NumericalValue", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Columns.Add("Comment", System.Type.GetType("System.String"))

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes
                    For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(AUserID)
                        Dim Row As DataRow
                        Row = mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).NewRow()
                        Row("UserID") = AUserID
                        Row("WRTNodeID") = node.NodeID
                        If node.MeasureType = ECMeasureType.mtPairwise Then
                            Row("Advantage") = CType(J, clsPairwiseMeasureData).Advantage
                            Row("FirstNodeID") = CType(J, clsPairwiseMeasureData).FirstNodeID
                            Row("SecondNodeID") = CType(J, clsPairwiseMeasureData).SecondNodeID
                        Else
                            Row("Advantage") = 0
                            Row("FirstNodeID") = CType(J, clsNonPairwiseMeasureData).NodeID
                            Row("SecondNodeID") = -1
                        End If

                        If J.IsUndefined Then
                            Row("VerbalValue") = "undefined"
                            Row("NumericalValue") = -1
                        Else
                            Select Case node.MeasureType
                                Case ECMeasureType.mtPairwise
                                    Row("VerbalValue") = CType(J, clsPairwiseMeasureData).Value.ToString
                                    Row("NumericalValue") = CType(J, clsPairwiseMeasureData).Value
                                Case ECMeasureType.mtRatings
                                    Row("VerbalValue") = CType(J, clsRatingMeasureData).Rating.Name
                                    Row("NumericalValue") = CType(J, clsRatingMeasureData).Rating.Value
                                Case Else
                                    Row("NumericalValue") = CSng(CType(J, clsNonPairwiseMeasureData).ObjectValue).ToString
                                    Row("VerbalValue") = CSng(CType(J, clsNonPairwiseMeasureData).SingleValue)
                            End Select
                        End If

                        Row("Comment") = J.Comment
                        mDataSet.Tables(TABLE_NAME_USER_ALTERNATIVES_JUDGMENTS).Rows.Add(Row)
                    Next
                Next
            End If
        End Sub

        Protected Sub AddMaxOutPrioritiesTable() 'C0389
            ' ADD MaxOutNodesPriorities table
            mDataSet.Tables.Add(TABLE_NAME_MAXOUT_NODES_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_MAXOUT_NODES_PRIORITIES).Columns.Add("UserEMail", System.Type.GetType("System.String")) 'C0391
            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                mDataSet.Tables(TABLE_NAME_MAXOUT_NODES_PRIORITIES).Columns.Add(node.NodeID.ToString + " - " + node.NodeName, System.Type.GetType("System.Single"))
            Next


            ' ADD MaxOutAlternativesPriorities table
            mDataSet.Tables.Add(TABLE_NAME_MAXOUT_ALTERNATIVES_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_MAXOUT_ALTERNATIVES_PRIORITIES).Columns.Add("UserEMail", System.Type.GetType("System.String")) 'C0391
            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).Nodes
                mDataSet.Tables(TABLE_NAME_MAXOUT_ALTERNATIVES_PRIORITIES).Columns.Add(alt.NodeName, System.Type.GetType("System.Single"))
            Next

            For Each user As clsUser In ProjectManager.UsersList
                If user.UserID >= 0 Then
                    'ProjectManager.CalculationsManager.Calculate(user.UserID, ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)) 'C0551
                    ProjectManager.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttUser, user), ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0), ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy) 'C0551

                    ' add nodes priorities
                    Dim Row As DataRow
                    Row = mDataSet.Tables(TABLE_NAME_MAXOUT_NODES_PRIORITIES).NewRow()
                    Row("UserEMail") = user.UserEMail 'C0391

                    For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                        'Row(node.NodeID.ToString + " - " + node.NodeName) = node.LocalPriority(user.UserID) 'C0551
                        Row(node.NodeID.ToString + " - " + node.NodeName) = node.LocalPriority(New clsCalculationTarget(CalculationTargetType.cttUser, user)) 'C0551
                    Next

                    mDataSet.Tables(TABLE_NAME_MAXOUT_NODES_PRIORITIES).Rows.Add(Row)

                    ' add alternatives priorities
                    Dim Row1 As DataRow
                    Row1 = mDataSet.Tables(TABLE_NAME_MAXOUT_ALTERNATIVES_PRIORITIES).NewRow()
                    Row1("UserEMail") = user.UserEMail 'C0391

                    For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).Nodes
                        Row1(alt.NodeName) = alt.WRTGlobalPriority()
                    Next

                    mDataSet.Tables(TABLE_NAME_MAXOUT_ALTERNATIVES_PRIORITIES).Rows.Add(Row1)
                End If
            Next
        End Sub

        ' D4300 ===
        Private Function GetTeamTimeStep(ObjID As Integer, AltID As Integer) As Integer
            Dim tStep As Integer = TeamTime.GetTeamTimeStep(ObjID, AltID)
            If tStep >= 0 Then tStep += 1
            Return tStep
        End Function

        ' D1269 ===
        Private _TeamTime_Pipe As clsTeamTimePipe = Nothing
        Friend Property TeamTime() As clsTeamTimePipe
            Get
                If _TeamTime_Pipe Is Nothing Then
                    _TeamTime_Pipe = New clsTeamTimePipe(mPrjManager, mPrjManager.User)
                    _TeamTime_Pipe.Override_ResultsMode = True
                    _TeamTime_Pipe.ResultsViewMode = ResultsView.rvBoth
                    _TeamTime_Pipe.VerifyUsers(TeamTimeUsersList)
                    _TeamTime_Pipe.CreatePipe()
                End If
                Return _TeamTime_Pipe
            End Get
            Set(value As clsTeamTimePipe)
                _TeamTime_Pipe = value
            End Set
        End Property

        Private Function TeamTimeUsersList() As List(Of clsUser)
            Dim tLst As New List(Of clsUser)
            For Each tUser As clsUser In mPrjManager.UsersList
                If tUser.UserID = mPrjManager.UserID OrElse tUser.SyncEvaluationMode <> SynchronousEvaluationMode.semNone OrElse tUser.IncludedInSynchronous OrElse tUser.Active Then   ' D7045
                    tLst.Add(tUser)
                Else
                    mPrjManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, tUser.UserID, , True)
                End If
            Next
            mPrjManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, COMBINED_USER_ID, , True)
            Return tLst
        End Function

        ' D4300 ==

        Protected Sub AddConsensusViewTable() 'C0631
            mDataSet.Tables.Add(TABLE_NAME_CONSENSUS_VIEW)
            mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Columns.Add("ParentID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Columns.Add("ChildID", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Columns.Add("Variance", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Columns.Add("StdDeviation", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Columns.Add("TTStepID", System.Type.GetType("System.Int32")) 'C0636 'L0197
            mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Columns.Add("IsAlternative", System.Type.GetType("System.Boolean")) 'C0970
            mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Columns.Add("IsTTStepAvailable", System.Type.GetType("System.Boolean"))

            Dim pipeStepsCount As Integer = TeamTime.Pipe.Count ' need this to address and create pipe

            ProjectManager.StorageManager.Reader.LoadUserData()
            'TeamTime = Nothing  ' D4300

            Dim variance As Single
            Dim stdDeviation As Single

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes 'C0970
                    Dim ListOfValues As New List(Of Double) 'C0821
                    Dim value As Single 'C0821

                    Select Case node.MeasureType
                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                            For Each user As clsUser In mPrjManager.UsersList
                                node.CalculateLocal(user.UserID)
                            Next
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                ListOfValues.Clear()
                                For Each user As clsUser In mPrjManager.UsersList
                                    value = node.Judgments.Weights.GetUserWeights(user.UserID, ECSynthesisMode.smDistributive, False).GetWeightValueByNodeID(child.NodeID)
                                    If value <> 0 Then 'C0981
                                        ListOfValues.Add(value)
                                    End If
                                Next
                                If ListOfValues.Count > 1 Then 'C0981
                                    Dim Row As DataRow
                                    Row = mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).NewRow()
                                    Row("ParentID") = node.NodeID
                                    Row("ChildID") = child.NodeID
                                    If ECCore.MathFuncs.CalculateVarianceAndStandardDeviation(ListOfValues, variance, stdDeviation, MathFuncs.StandardDeviationMode.sdmBiased) Then 'C0633 'C0635
                                        Row("Variance") = variance
                                        Row("StdDeviation") = stdDeviation
                                    Else
                                        Row("Variance") = 2 'dummy value to indicate that there's no value in this cell
                                        Row("StdDeviation") = 2 'dummy value to indicate that there's no value in this cell
                                    End If
                                    Row("TTStepID") = GetTeamTimeStep(node.NodeID, child.NodeID)    ' D4300
                                    Row("IsAlternative") = node.IsTerminalNode 'C0970
                                    'Row("IsTTStepAvailable") = node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtRatings Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes
                                    Row("IsTTStepAvailable") = True
                                    mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Rows.Add(Row)
                                End If
                            Next
                        Case ECMeasureType.mtRatings, ECMeasureType.mtDirect, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID) 'C0804
                                Dim J As clsNonPairwiseMeasureData
                                ListOfValues.Clear()
                                For Each user As clsUser In mPrjManager.UsersList
                                    Dim IsAvailable As Boolean
                                    If child.IsAlternative Then
                                        IsAvailable = ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, child.NodeGuidID, user.UserID)
                                    Else
                                        IsAvailable = child.IsAllowed(user.UserID)
                                    End If

                                    If IsAvailable Then
                                        J = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(child.NodeID, node.NodeID, user.UserID)
                                        If J IsNot Nothing Then
                                            If Not J.IsUndefined Then
                                                value = J.SingleValue
                                                If value <> 0 Then 'C0974
                                                    ListOfValues.Add(value)
                                                End If
                                            End If
                                        End If
                                    End If
                                Next
                                If ListOfValues.Count > 1 Then 'C0981
                                    Dim Row As DataRow
                                    Row = mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).NewRow()
                                    Row("ParentID") = node.NodeID
                                    Row("ChildID") = child.NodeID
                                    If ECCore.MathFuncs.CalculateVarianceAndStandardDeviation(ListOfValues, variance, stdDeviation, MathFuncs.StandardDeviationMode.sdmBiased) Then 'C0633 'C0635
                                        Row("Variance") = variance
                                        Row("StdDeviation") = stdDeviation
                                    Else
                                        Row("Variance") = 2 'dummy value to indicate that there's no value in this cell
                                        Row("StdDeviation") = 2 'dummy value to indicate that there's no value in this cell
                                    End If
                                    Row("TTStepID") = GetTeamTimeStep(node.NodeID, child.NodeID)    ' D4300
                                    Row("IsAlternative") = node.IsTerminalNode 'C0970
                                    'Row("IsTTStepAvailable") = node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtRatings
                                    Row("IsTTStepAvailable") = True
                                    mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Rows.Add(Row)
                                End If
                            Next
                    End Select
                Next
            End If

            If ProjectManager.IsRiskProject And (mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).HierarchyID = ECHierarchyID.hidLikelihood) Then
                For Each alt As ECCore.clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).GetUncontributedAlternatives
                    Dim ListOfValues As New List(Of Double)
                    Dim value As Single
                    For Each user As clsUser In mPrjManager.UsersList
                        Dim IsAvailable As Boolean
                        IsAvailable = ProjectManager.UsersRoles.IsAllowedAlternative(mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0).NodeGuidID, alt.NodeGuidID, user.UserID)

                        If IsAvailable Then
                            If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                alt.CalculateLocal(user.UserID)
                                value = alt.Judgments.Weights.GetUserWeights(user.UserID, alt.Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, alt.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                            Else
                                Dim J As clsNonPairwiseMeasureData = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0).NodeID, user.UserID)
                                If J IsNot Nothing AndAlso Not J.IsUndefined Then
                                    value = J.SingleValue
                                    If value <> 0 Then
                                        ListOfValues.Add(value)
                                    End If
                                End If
                            End If

                        End If
                    Next
                    If ListOfValues.Count > 1 Then 'C0981
                        Dim Row As DataRow
                        Row = mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).NewRow()
                        Row("ParentID") = -2 'dummy value to indicate no sources item
                        Row("ChildID") = alt.NodeID
                        If ECCore.MathFuncs.CalculateVarianceAndStandardDeviation(ListOfValues, variance, stdDeviation, MathFuncs.StandardDeviationMode.sdmBiased) Then 'C0633 'C0635
                            Row("Variance") = variance
                            Row("StdDeviation") = stdDeviation
                        Else
                            Row("Variance") = 2 'dummy value to indicate that there's no value in this cell
                            Row("StdDeviation") = 2 'dummy value to indicate that there's no value in this cell
                        End If
                        Row("TTStepID") = GetTeamTimeStep(-2, alt.NodeID)    ' D4300
                        Row("IsAlternative") = True
                        'Row("IsTTStepAvailable") = node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtRatings
                        Row("IsTTStepAvailable") = True
                        mDataSet.Tables(TABLE_NAME_CONSENSUS_VIEW).Rows.Add(Row)
                    End If
                Next
            End If
        End Sub

        Protected Sub AddUsersObjectivesPrioritiesTable(User As clsUser, Group As clsGroup)
            'Debug.Print("Started generating UsersObjectivesPrioritiesTable: " + Now.ToString)

            'Debug.Print("Started creating table: " + Now.ToString)
            mDataSet.Tables.Add(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Columns.Add("N", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Columns.Add("UserEmail", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Columns.Add("UserName", System.Type.GetType("System.String")) 'C0964
            mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Columns.Add("NodeName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Columns.Add("NodePath", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Columns.Add("LocalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Columns.Add("GlobalPriority", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Columns.Add("Inconsistency", System.Type.GetType("System.Single")) 'C0963

            'Debug.Print("Users count: " + mPrjManager.UsersList.Count.ToString)
            Dim i As Integer = 1
            Dim j As Integer = 0 'C0771 'C0774 - changed to 0

            Dim UsersWithNoJudgmentsCount As Integer = 0

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                For Each UserID As Integer In UserIDs
                    Dim u As clsUser = mPrjManager.GetUserByID(UserID)
                    If Worker IsNot Nothing Then
                        Worker.ReportProgress(j / ProjectManager.UsersList.Count * 100, "Processing user: " + u.UserName + " (" + u.UserEMail + ")")
                    End If

                    'RaiseEvent UserPrioritiesProgress(j, mPrjManager.UsersList.Count, user.UserName + " (" + user.UserEMail + ")") 'C0771

                    'C0771===
                    Dim count As Integer
                    Select Case ProjectManager.StorageManager.StorageType
                        Case ECModelStorageType.mstCanvasStreamDatabase
                            count = ProjectManager.StorageManager.Reader.LoadUserJudgments(u)
                    End Select
                    'C0771==

                    ProjectManager.StorageManager.Reader.LoadUserPermissions(u)

                    'Debug.Print("Judgments loaded: " + count.ToString)

                    If count < 1 Then
                        UsersWithNoJudgmentsCount += 1
                    Else
                        If u.UserID >= 0 Then
                            'Debug.Print("Calculating for user " + i.ToString + ": " + u.UserEMail)

                            Dim CT As New clsCalculationTarget(CalculationTargetType.cttUser, u)
                            mPrjManager.CalculationsManager.Calculate(CT, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)

                            For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes
                                If node.IsAllowed(u.UserID) Then
                                    Dim NodePath As String = ""
                                    GetFullNodePath(node, NodePath)

                                    Dim Row As DataRow
                                    Row = mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).NewRow()
                                    Row("N") = i
                                    Row("UserEmail") = u.UserEMail
                                    Row("UserName") = u.UserName 'C0964
                                    Row("NodeName") = node.NodeName
                                    Row("NodePath") = NodePath
                                    Row("LocalPriority") = node.LocalPriority(CT)
                                    Row("GlobalPriority") = node.WRTGlobalPriority
                                    'C0963===
                                    If node.MeasureType = ECMeasureType.mtPairwise Then
                                        Row("Inconsistency") = CType(node.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                                    Else
                                        Row("Inconsistency") = 0
                                    End If
                                    'Debug.Print("Inconsistency for node " + node.NodeName + "(" + node.MeasureType.ToString + ") = " + Row("Inconsistency").ToString)
                                    'C0963==
                                    mDataSet.Tables(TABLE_NAME_USERS_OBJECTIVES_PRIORITIES).Rows.Add(Row)

                                    i += 1
                                End If
                            Next
                        End If
                    End If

                    If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> u.UserID) Then 'C0771
                        ProjectManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, u.UserID, , True)
                    End If

                    j += 1 'C0771
                Next
            End If

            'C0774===
            If Worker IsNot Nothing Then
                Worker.ReportProgress(100, "Done!")
            End If
            'C0774==

            'Debug.Print("UsersWithNoJudgmentsCount: " + UsersWithNoJudgmentsCount.ToString)
            'Debug.Print("Finished creating table: " + Now.ToString)
        End Sub

        Protected Sub AddEvaluationProgressTable() 'C0766
            'Debug.Print("Started generating EvaluationProgress table: " + Now.ToString)

            'Debug.Print("Started creating table: " + Now.ToString)
            mDataSet.Tables.Add(TABLE_NAME_EVALUATION_PROGRESS)
            mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("N", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("UserEmail", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("JudgmentsMade", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("JudgmentsTotal", System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("Percentage", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("JudgmentTime", System.Type.GetType("System.String"))

            Dim defTotal As Integer = ProjectManager.GetDefaultTotalJudgmentCount(ProjectManager.ActiveHierarchy)
            Dim startTime As DateTime = Now

            Dim user As clsUser
            Dim uCount As Integer = ProjectManager.UsersList.Count
            Dim StartDT As Date = Now

            Dim madeAllCount As Integer = 0
            Dim totalAllCount As Integer = 0
            Dim evalProgress As Dictionary(Of String, UserEvaluationProgressData) = ProjectManager.StorageManager.Reader.GetEvaluationProgress(ProjectManager.UsersList, ProjectManager.ActiveHierarchy, madeAllCount, totalAllCount)

            For i As Integer = 0 To uCount - 1
                user = ProjectManager.UsersList(i)
                If Worker Is Nothing OrElse (Worker IsNot Nothing AndAlso Not Worker.CancellationPending) Then
                    If user IsNot Nothing Then
                        Dim userLastJudgmentTime As Nullable(Of DateTime) = user.LastJudgmentTime
                        If userLastJudgmentTime Is Nothing Then
                            userLastJudgmentTime = VERY_OLD_DATE
                        End If

                        Dim totalCount As Integer = 0
                        Dim madeCount As Integer = 0

                        If evalProgress.ContainsKey(user.UserEMail.ToLower) Then
                            With evalProgress(user.UserEMail.ToLower)
                                madeCount = .EvaluatedCount
                                totalAllCount = .TotalCount
                                If .LastJudgmentTime.HasValue Then
                                    userLastJudgmentTime = .LastJudgmentTime
                                End If
                            End With
                        End If

                        If madeCount = 0 Then userLastJudgmentTime = Nothing

                        Dim Row As DataRow
                        Row = mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).NewRow()
                        Row("N") = i
                        Row("UserName") = user.UserName
                        Row("UserEmail") = user.UserEMail
                        Row("JudgmentsMade") = madeCount
                        Row("JudgmentsTotal") = totalCount
                        Row("Percentage") = If(totalCount <> 0, (madeCount / totalCount * 100), 0).ToString("F2") + "%"
                        If userLastJudgmentTime IsNot Nothing Then
                            Row("JudgmentTime") = userLastJudgmentTime.ToString
                        Else
                            Row("JudgmentTime") = ""
                        End If
                        mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Rows.Add(Row)

                    End If
                Else
                    Exit For
                End If
            Next
        End Sub

        'Protected Sub AddEvaluationProgressTable() 'C0766
        '    'Debug.Print("Started generating EvaluationProgress table: " + Now.ToString)

        '    'Debug.Print("Started creating table: " + Now.ToString)
        '    mDataSet.Tables.Add(TABLE_NAME_EVALUATION_PROGRESS)
        '    mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("N", System.Type.GetType("System.Int32"))
        '    mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("UserName", System.Type.GetType("System.String"))
        '    mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("UserEmail", System.Type.GetType("System.String"))
        '    mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("JudgmentsMade", System.Type.GetType("System.Int32"))
        '    mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("JudgmentsTotal", System.Type.GetType("System.Int32"))
        '    mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Columns.Add("Percentage", System.Type.GetType("System.String"))

        '    'Debug.Print("Total users count: " + mPrjManager.UsersList.Count.ToString)
        '    Dim i As Integer = 1
        '    Dim j As Integer = 0 'C0771 'C0774 - changed to 0
        '    Dim UsersWithNoJudgmentsCount As Integer = 0

        '    Dim made As Integer
        '    Dim total As Integer

        '    For Each user As clsUser In mPrjManager.UsersList
        '        'ProjectManager.StorageManager.Reader.LoadUserJudgments(user, False, -1) 'C0765
        '        'Debug.Print("Loading judgments for user " + i.ToString + ": " + user.UserEMail)

        '        'C0774===
        '        If Worker IsNot Nothing Then
        '            Worker.ReportProgress(j / ProjectManager.UsersList.Count * 100, "Processing user: " + user.UserName + " (" + user.UserEMail + ")")
        '        End If
        '        'C0774==

        '        If user.UserID >= 0 Then
        '            'Debug.Print("Loading permissions for user " + i.ToString + ": " + user.UserEMail)
        '            'ProjectManager.StorageManager.Reader.LoadUserPermissions(user)
        '            'Debug.Print("Permissions loaded")

        '            made = ProjectManager.GetMadeJudgmentCount(ProjectManager.ActiveHierarchy, user.UserID)
        '            total = ProjectManager.GetTotalJudgmentCount(ProjectManager.ActiveHierarchy, user.UserID)

        '            Dim Row As DataRow
        '            Row = mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).NewRow()
        '            Row("N") = i
        '            Row("UserName") = user.UserName
        '            Row("UserEmail") = user.UserEMail
        '            Row("JudgmentsMade") = made
        '            Row("JudgmentsTotal") = total
        '            Row("Percentage") = (made / total * 100).ToString("F2") + "%"
        '            mDataSet.Tables(TABLE_NAME_EVALUATION_PROGRESS).Rows.Add(Row)

        '            i += 1
        '        End If
        '        'End If

        '        j += 1 'C0771

        '        'If ProjectManager.User.UserID <> user.UserID Then 'C0771

        '        'C0781===
        '        'If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> user.UserID) Then 'C0771
        '        '    ProjectManager.CleanUpUserDataFromMemory(user.UserID)
        '        'End If
        '        'C0781==
        '    Next

        '    'C0774===
        '    If Worker IsNot Nothing Then
        '        Worker.ReportProgress(100, "Done!")
        '    End If
        '    'C0774==

        '    'Debug.Print("UsersWithNoJudgmentsCount: " + UsersWithNoJudgmentsCount.ToString)
        '    'Debug.Print("Finished creating EvaluationProgress table: " + Now.ToString)
        'End Sub

        Protected Sub AddOverallResults2Table(ByVal UsersList As List(Of clsUser), ByVal GroupsList As List(Of clsCombinedGroup), ByVal Alternatives As List(Of clsNode), ByVal WRTNode As clsNode) 'C0816
            'Debug.Print("Started generating OverallResults2 table: " + Now.ToString)

            'Debug.Print("Started creating table: " + Now.ToString)
            mDataSet.Tables.Add(TABLE_NAME_OVERALL_RESULTS_2)

            mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).Columns.Add("AlternativeName", System.Type.GetType("System.String"))
            For Each user As clsUser In UsersList
                mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).Columns.Add("User" + user.UserID, System.Type.GetType("System.String"))
            Next
            For Each group As clsCombinedGroup In GroupsList
                mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).Columns.Add("CombinedGroup" + group.CombinedUserID.ToString, System.Type.GetType("System.String"))
            Next

            'Debug.Print("Finished creating table structure")

            Dim totalUsers As Integer = UsersList.Count + GroupsList.Count

            Dim j As Integer = 0

            Dim UsersWithNoJudgmentsCount As Integer = 0

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                For Each alt As clsNode In Alternatives
                    Dim Row As DataRow
                    Row = mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).NewRow()
                    Row("AlternativeName") = alt.NodeName
                    mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).Rows.Add(Row)
                Next

                'Debug.Print("Adding users' priorities")
                For l As Integer = 0 To UsersList.Count - 1
                    Dim user As clsUser = UsersList(l)
                    'Debug.Print("Loading judgments for user: " + user.UserEMail)

                    If Worker IsNot Nothing Then
                        Worker.ReportProgress(j / totalUsers * 100, "Processing user: " + user.UserName + " (" + user.UserEMail + ")")
                    End If

                    Dim count As Integer
                    Select Case ProjectManager.StorageManager.StorageType
                        Case ECModelStorageType.mstCanvasStreamDatabase
                            count = ProjectManager.StorageManager.Reader.LoadUserJudgments(user)
                    End Select

                    'Debug.Print("Judgments loaded: " + count.ToString)

                    If count < 1 Then
                        UsersWithNoJudgmentsCount += 1

                        Dim email As String = user.UserEMail
                        For k As Integer = 0 To Alternatives.Count - 1
                            mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).Rows(k).Item(j + 1) = -1
                        Next
                    Else
                        'Debug.Print("Calculating for user: " + user.UserEMail)

                        Dim CT As New clsCalculationTarget(CalculationTargetType.cttUser, user)
                        mPrjManager.CalculationsManager.Calculate(CT, WRTNode, mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)

                        For k As Integer = 0 To Alternatives.Count - 1
                            mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).Rows(k).Item(j + 1) = Alternatives(k).WRTGlobalPriority
                        Next
                    End If

                    If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> user.UserID) Then
                        ProjectManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, user.UserID, , True)
                    End If

                    j += 1
                Next

                'Debug.Print("Adding groups' priorities")
                For l As Integer = 0 To GroupsList.Count - 1
                    Dim CG As clsCombinedGroup = GroupsList(l)

                    Dim count As Integer
                    'Debug.Print("Loading judgments for combined group: " + CG.Name)
                    For Each user As clsUser In CG.UsersList
                        'Debug.Print("Loading judgments for user: " + user.UserEMail)
                        Select Case ProjectManager.StorageManager.StorageType
                            Case ECModelStorageType.mstCanvasStreamDatabase
                                count += ProjectManager.StorageManager.Reader.LoadUserJudgments(user)
                        End Select
                        'Debug.Print("Judgments loaded: " + count.ToString)
                    Next

                    If Worker IsNot Nothing Then
                        Worker.ReportProgress(j / totalUsers * 100, "Processing group: " + CG.Name)
                    End If

                    If count < 1 Then
                        For Each user As clsUser In CG.UsersList
                            Dim email As String = user.UserEMail
                            For k As Integer = 0 To Alternatives.Count - 1
                                mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).Rows(k).Item(j + 1 + UsersList.Count) = -1
                            Next
                        Next
                    Else
                        'Debug.Print("Calculating for group: " + CG.Name)

                        Dim CT As New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                        mPrjManager.CalculationsManager.Calculate(CT, WRTNode, mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)

                        For k As Integer = 0 To Alternatives.Count - 1
                            mDataSet.Tables(TABLE_NAME_OVERALL_RESULTS_2).Rows(k).Item(j + 1) = Alternatives(k).WRTGlobalPriority
                        Next
                    End If

                    For Each user As clsUser In CG.UsersList
                        If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> user.UserID) Then
                            ProjectManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, user.UserID, , True)
                        End If
                    Next
                    j += 1
                Next
            End If

            'C0774===
            If Worker IsNot Nothing Then
                Worker.ReportProgress(100, "Done!")
            End If
            'C0774==
            'Debug.Print("Finished creating table: " + Now.ToString)
        End Sub

        Protected Sub AddPivotAlternativesPrioritiesTable(User As clsUser, Group As clsCombinedGroup)
            'Debug.Print("Started generating PivotAlternativesPriorities table: " + Now.ToString)

            mDataSet.Tables.Add(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES)
            mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Columns.Add("AltName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Columns.Add("AltGlobalPriority", System.Type.GetType("System.Single"))

            Dim UserIDs As ArrayList = GetUserIDsList(User, Group)
            LoadData(User, Group)

            'Dim totalUsers As Integer = UsersList.Count + GroupsList.Count
            Dim totalUsers As Integer = UserIDs.Count

            Dim WRTNode As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)

            Dim j As Integer = 0

            Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                If User IsNot Nothing Then
                    'Debug.Print("Loading judgments for user: " + User.UserEMail)

                    If Worker IsNot Nothing Then
                        Worker.ReportProgress(j / totalUsers * 100, "Processing user: " + User.UserName + " (" + User.UserEMail + ")")
                    End If

                    mPrjManager.CalculationsManager.Calculate(CT, WRTNode, mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)

                    Dim sum As Single = 0
                    If ProjectManager.CalculationsManager.IncludeIdealAlternative Then
                        For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                            sum += alt.WRTGlobalPriority
                        Next
                    End If

                    For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                        Dim Row As DataRow
                        Row = mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).NewRow()
                        Row("UserName") = User.UserName + " (" + User.UserEMail + ")"
                        Row("AltName") = alt.NodeName
                        If sum = 0 Then
                            Row("AltGlobalPriority") = alt.WRTGlobalPriority
                        Else
                            Row("AltGlobalPriority") = alt.WRTGlobalPriority / sum
                        End If
                        mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Rows.Add(Row)
                    Next

                    If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> User.UserID) Then
                        ProjectManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, User.UserID, , True)
                    End If

                    j += 1
                End If

                If Group IsNot Nothing Then
                    If Worker IsNot Nothing Then
                        Worker.ReportProgress(j / totalUsers * 100, "Processing group: " + Group.Name)
                    End If

                    If mGroupType = GroupReportType.grtGroupOnly Or mGroupType = GroupReportType.grtBoth Then
                        mPrjManager.CalculationsManager.Calculate(CT, WRTNode, mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)

                        'C0983===
                        Dim sum As Single = 0
                        If ProjectManager.CalculationsManager.IncludeIdealAlternative Then
                            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                                sum += alt.WRTGlobalPriority
                            Next
                        End If
                        'C0983==

                        For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                            Dim Row As DataRow
                            Row = mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).NewRow()
                            Row("UserName") = Group.Name
                            Row("AltName") = alt.NodeName
                            If sum = 0 Then
                                Row("AltGlobalPriority") = alt.WRTGlobalPriority
                            Else
                                Row("AltGlobalPriority") = alt.WRTGlobalPriority / sum
                            End If
                            mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Rows.Add(Row)
                        Next
                    End If

                    If mGroupType = GroupReportType.grtUsersOnly Or mGroupType = GroupReportType.grtBoth Then
                        For Each u As clsUser In Group.UsersList.ToArray
                            Dim uCT As clsCalculationTarget = GetCalulationTarget(u, Nothing)

                            mPrjManager.CalculationsManager.Calculate(uCT, WRTNode, mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)

                            Dim sum As Single = 0
                            If ProjectManager.CalculationsManager.IncludeIdealAlternative Then
                                For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                                    sum += alt.WRTGlobalPriority
                                Next
                            End If

                            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                                Dim Row As DataRow
                                Row = mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).NewRow()
                                Row("UserName") = u.UserName + " (" + u.UserEMail + ")"
                                Row("AltName") = alt.NodeName
                                If sum = 0 Then
                                    Row("AltGlobalPriority") = alt.WRTGlobalPriority
                                Else
                                    Row("AltGlobalPriority") = alt.WRTGlobalPriority / sum
                                End If
                                mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Rows.Add(Row)
                            Next

                            If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> u.UserID) Then
                                'ProjectManager.CleanUpUserDataFromMemory(u.UserID, , True)
                            End If
                        Next
                    End If

                    j += 1
                End If
            End If

            If Worker IsNot Nothing Then
                Worker.ReportProgress(100, "Done!")
            End If
            'Debug.Print("Finished creating table: " + Now.ToString)
        End Sub

        'Protected Sub AddPivotAlternativesPrioritiesTable(ByVal UsersList As List(Of clsUser), ByVal GroupsList As List(Of clsCombinedGroup), ByVal Alternatives As List(Of clsNode), ByVal WRTNode As clsNode) 'C0822
        '    'Debug.Print("Started generating PivotAlternativesPriorities table: " + Now.ToString)

        '    mDataSet.Tables.Add(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES)
        '    mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Columns.Add("UserName", System.Type.GetType("System.String"))
        '    mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Columns.Add("AltName", System.Type.GetType("System.String"))
        '    mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Columns.Add("AltGlobalPriority", System.Type.GetType("System.Single"))

        '    Dim totalUsers As Integer = UsersList.Count + GroupsList.Count

        '    Dim j As Integer = 0

        '    Dim UsersWithNoJudgmentsCount As Integer = 0

        '    If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
        '        For Each user As clsUser In UsersList
        '            'Debug.Print("Loading judgments for user: " + user.UserEMail)

        '            If Worker IsNot Nothing Then
        '                Worker.ReportProgress(j / totalUsers * 100, "Processing user: " + user.UserName + " (" + user.UserEMail + ")")
        '            End If

        '            Dim count As Integer
        '            Select Case ProjectManager.StorageManager.StorageType
        '                Case ECModelStorageType.mstCanvasStreamDatabase
        '                    count = ProjectManager.StorageManager.Reader.LoadUserJudgmentsFromCanvasStreamDatabase(user)

        '                Case ECModelStorageType.mstAHPSFile
        '                    count = ProjectManager.StorageManager.Reader.LoadUserJudgmentsFromAHPSFile(user)
        '                Case ECModelStorageType.mstCanvasDatabase, ECModelStorageType.mstAHPDatabase 'C0772
        '                    ProjectManager.StorageManager.Reader.LoadUserJudgments(user, False)
        '                    count = 1
        '            End Select
        '            ProjectManager.StorageManager.Reader.LoadUserPermissions(user) 'C0984

        '            'Debug.Print("Judgments loaded: " + count.ToString)

        '            If count >= 1 Then
        '                Dim CT As New clsCalculationTarget(CalculationTargetType.cttUser, user)
        '                mPrjManager.CalculationsManager.Calculate(CT, WRTNode, mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)
        '            End If

        '            'C0983===
        '            Dim sum As Single = 0
        '            If ProjectManager.CalculationsManager.IncludeIdealAlternative Then
        '                For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
        '                    sum += alt.WRTGlobalPriority
        '                Next
        '            End If
        '            'C0983==

        '            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
        '                Dim Row As DataRow
        '                Row = mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).NewRow()
        '                'Row("UserName") = user.UserName 'C0970
        '                Row("UserName") = user.UserName + " (" + user.UserEMail + ")" 'C0970
        '                Row("AltName") = alt.NodeName
        '                If count < 1 Then
        '                    Row("AltGlobalPriority") = -1
        '                Else
        '                    'Row("AltGlobalPriority") = alt.WRTGlobalPriority 'C0983
        '                    'C0983===
        '                    If sum = 0 Then
        '                        Row("AltGlobalPriority") = alt.WRTGlobalPriority
        '                    Else
        '                        Row("AltGlobalPriority") = alt.WRTGlobalPriority / sum
        '                    End If
        '                    'C0983==
        '                End If
        '                mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Rows.Add(Row)
        '            Next

        '            If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> user.UserID) Then
        '                ProjectManager.CleanUpUserDataFromMemory(user.UserID)
        '            End If

        '            j += 1
        '        Next

        '        'Debug.Print("Adding groups' priorities")
        '        For Each CG As clsCombinedGroup In GroupsList
        '            Dim count As Integer
        '            'Debug.Print("Loading judgments for combined group: " + CG.Name)
        '            For Each user As clsUser In CG.UsersList
        '                'Debug.Print("Loading judgments for user: " + user.UserEMail)
        '                Select Case ProjectManager.StorageManager.StorageType
        '                    Case ECModelStorageType.mstCanvasStreamDatabase
        '                        count += ProjectManager.StorageManager.Reader.LoadUserJudgmentsFromCanvasStreamDatabase(user)
        '                    Case ECModelStorageType.mstAHPSFile
        '                        count += ProjectManager.StorageManager.Reader.LoadUserJudgmentsFromAHPSFile(user)
        '                    Case ECModelStorageType.mstCanvasDatabase, ECModelStorageType.mstAHPDatabase
        '                        ProjectManager.StorageManager.Reader.LoadUserJudgments(user, False)
        '                        count += 1
        '                End Select
        '                'Debug.Print("Judgments loaded: " + count.ToString)
        '            Next

        '            If Worker IsNot Nothing Then
        '                Worker.ReportProgress(j / totalUsers * 100, "Processing group: " + CG.Name)
        '            End If

        '            If count >= 1 Then
        '                'Debug.Print("Calculating for group: " + CG.Name)

        '                Dim CT As New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
        '                mPrjManager.CalculationsManager.Calculate(CT, WRTNode, mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)
        '            End If

        '            'C0983===
        '            Dim sum As Single = 0
        '            If ProjectManager.CalculationsManager.IncludeIdealAlternative Then
        '                For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
        '                    sum += alt.WRTGlobalPriority
        '                Next
        '            End If
        '            'C0983==

        '            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
        '                Dim Row As DataRow
        '                Row = mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).NewRow()
        '                'Row("UserName") = CG.Name 'C0970
        '                Row("UserName") = CG.Name + " (" + CG.CombinedUserID.ToString + ")" 'C0970
        '                Row("AltName") = alt.NodeName
        '                If count < 1 Then
        '                    Row("AltGlobalPriority") = -1
        '                Else
        '                    'Row("AltGlobalPriority") = alt.WRTGlobalPriority 'C0983

        '                    'C0983===
        '                    If sum = 0 Then
        '                        Row("AltGlobalPriority") = alt.WRTGlobalPriority
        '                    Else
        '                        Row("AltGlobalPriority") = alt.WRTGlobalPriority / sum
        '                    End If
        '                    'C0983==
        '                End If
        '                mDataSet.Tables(TABLE_NAME_PIVOT_ALTERNATIVES_PRIORITIES).Rows.Add(Row)
        '            Next

        '            For Each user As clsUser In CG.UsersList
        '                If (ProjectManager.User Is Nothing) OrElse (ProjectManager.User.UserID <> user.UserID) Then
        '                    ProjectManager.CleanUpUserDataFromMemory(user.UserID)
        '                End If
        '            Next
        '            j += 1
        '        Next
        '    End If

        '    'C0774===
        '    If Worker IsNot Nothing Then
        '        Worker.ReportProgress(100, "Done!")
        '    End If
        '    'C0774==
        '    'Debug.Print("Finished creating table: " + Now.ToString)
        'End Sub

        Private Function GetMeasureTypeName(MT As ECMeasureType) As String
            Select Case MT
                Case ECMeasureType.mtPairwise
                    Return "Pairwise"
                Case ECMeasureType.mtRatings
                    Return "Ratings"
                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                    Return "Utility Curve"
                Case ECMeasureType.mtDirect
                    Return "Direct"
                Case ECMeasureType.mtStep
                    Return "Step Function"
            End Select
            Return ""
        End Function

        Public Shared ReadOnly dgColNum As New Guid("{c76351fa-ec55-4b9a-8ffc-52e106b5fe1b}")
        Public Shared ReadOnly dgColAltName As New Guid("{b6ff0096-989a-457c-9ea7-4e72c58f65da}")
        Public Shared ReadOnly dgColTotal As New Guid("{e6f7a118-30fd-4556-b430-ee408337b550}")
        Public Shared ReadOnly dgColTotalLikelihood As New Guid("{3CCA09D6-E683-4B04-B5F8-2062BB9DD14B}")
        Public Shared ReadOnly dgColTotalImpact As New Guid("{110A3943-7579-4226-8804-B48565045256}")
        Public Shared ReadOnly dgColRisk As New Guid("{D70A547F-E17E-4878-AEB0-454612B4A7D8}")
        Public Shared ReadOnly dgColEarliest As New Guid("{667DBD44-0A9F-45C7-BEDA-9B49C12CE61B}")
        Public Shared ReadOnly dgColLatest As New Guid("{CE833D07-4B5F-4C65-AE83-24F19798D57B}")
        Public Shared ReadOnly dgColDuration As New Guid("{704734A9-CAE4-47BA-90CC-B96E1757ED7E}")

        Protected Sub AddDataGridTable(User As clsUser, Group As clsGroup)
            Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
            Dim UserID As Integer = CT.GetUserID

            mDataSet.Tables.Add(TABLE_NAME_DATA_GRID)
            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(dgColNum.ToString(), System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(dgColAltName.ToString(), System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(dgColTotal.ToString(), System.Type.GetType("System.Single"))
            For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes
                mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(node.NodeGuidID.ToString(), System.Type.GetType("System.String"))
            Next
            For Each altAttr As clsAttribute In mPrjManager.Attributes.GetAlternativesAttributes
                If altAttr.ID <> ECCore.ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID And altAttr.ID <> ECCore.ATTRIBUTE_RA_ALT_SORT_ID And (altAttr.ID <> ECCore.ATTRIBUTE_KNOWN_VALUE_ID Or ProjectManager.IsRiskProject) And altAttr.ID <> ECCore.ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID And altAttr.ID <> ECCore.ATTRIBUTE_RESULTS_ALTERNATIVE_IS_SELECTED_ID Then 'A1410
                    Select Case altAttr.ValueType
                        Case AttributeValueTypes.avtBoolean
                            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.String"))
                        Case AttributeValueTypes.avtDouble
                            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.Double"))
                        Case AttributeValueTypes.avtLong
                            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.Int64"))
                        Case AttributeValueTypes.avtString
                            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.String"))
                        Case AttributeValueTypes.avtEnumeration
                            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.String"))
                        Case AttributeValueTypes.avtEnumerationMulti
                            mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.String"))
                    End Select
                End If
            Next
            Dim RA As ResourceAligner = ProjectManager.ResourceAligner
            If RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0 Then
                mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(dgColEarliest.ToString(), System.Type.GetType("System.Int32"))
                mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(dgColLatest.ToString(), System.Type.GetType("System.Int32"))
                mDataSet.Tables(TABLE_NAME_DATA_GRID).Columns.Add(dgColDuration.ToString(), System.Type.GetType("System.Int32"))
            End If

            Dim i As Integer = 0
            Dim count As Integer = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Count

            Dim unnormSum As Single = 0
            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                unnormSum += alt.UnnormalizedPriority
            Next
            If unnormSum <> 0 And ProjectManager.CalculationsManager.SynthesisMode = ECSynthesisMode.smDistributive Then
                For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                    'alt.UnnormalizedPriority /= unnormSum
                Next
            End If

            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                Dim Row As DataRow
                i += 1
                'Total
                Row = mDataSet.Tables(TABLE_NAME_DATA_GRID).NewRow()
                Row(dgColNum.ToString) = i
                Row(dgColAltName.ToString) = alt.NodeName
                Row(dgColTotal.ToString) = alt.UnnormalizedPriority

                For Each node As clsNode In mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).TerminalNodes
                    Select Case node.MeasureType
                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
                            Row(node.NodeGuidID.ToString()) = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                        Case ECMeasureType.mtRatings
                            Dim rData As clsRatingMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserID)
                            If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                                Row(node.NodeGuidID.ToString()) = If(rData.Rating.ID <> -1, rData.Rating.Name, rData.Rating.Value)
                            Else
                                Row(node.NodeGuidID.ToString()) = ""
                            End If
                        Case Else
                            Dim nonpwData As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserID)
                            If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                Row(node.NodeGuidID.ToString()) = CSng(nonpwData.ObjectValue)
                            Else
                                Row(node.NodeGuidID.ToString()) = ""
                            End If
                    End Select

                Next

                For Each altAttr As clsAttribute In mPrjManager.Attributes.GetAlternativesAttributes
                    If altAttr.ID <> ECCore.ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID And altAttr.ID <> ECCore.ATTRIBUTE_RA_ALT_SORT_ID And (altAttr.ID <> ECCore.ATTRIBUTE_KNOWN_VALUE_ID Or ProjectManager.IsRiskProject) And altAttr.ID <> ECCore.ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID And altAttr.ID <> ECCore.ATTRIBUTE_RESULTS_ALTERNATIVE_IS_SELECTED_ID Then 'A1410
                        Dim Val As String = ""
                        Try
                            Select Case altAttr.ID
                                Case ATTRIBUTE_COST_ID
                                    Val = ProjectManager.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Cost
                                Case ATTRIBUTE_RISK_ID
                                    Val = ProjectManager.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).RiskOriginal
                                Case Else
                                    Val = mPrjManager.Attributes.GetAttributeValue(altAttr.ID, alt.NodeGuidID).ToString
                            End Select
                        Catch ex As Exception
                            Val = ""
                        End Try
                        Select Case altAttr.ValueType
                            Case AttributeValueTypes.avtBoolean
                                If Val <> "" Then
                                    If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) Then
                                        Row(altAttr.ID.ToString()) = CBool(Val)
                                    End If
                                End If
                            Case AttributeValueTypes.avtDouble
                                If Val <> "" Then
                                    If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE OrElse Val = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE) Then
                                        Row(altAttr.ID.ToString()) = CDbl(Val)
                                    End If
                                End If
                            Case AttributeValueTypes.avtLong
                                If Val <> "" Then
                                    If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE OrElse Val = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE) Then
                                        Row(altAttr.ID.ToString()) = CLng(Val)
                                    End If
                                End If
                            Case AttributeValueTypes.avtString
                                If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) Then
                                    Row(altAttr.ID.ToString()) = CStr(Val)
                                End If
                            Case AttributeValueTypes.avtEnumeration
                                If (Val <> UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) And (Val <> "") Then
                                    Dim sValue As String = ""
                                    Dim e As clsAttributeEnumeration = ProjectManager.Attributes.GetEnumByID(altAttr.EnumID)
                                    If e IsNot Nothing Then
                                        Dim ei As clsAttributeEnumerationItem = e.GetItemByID(New Guid(Val))
                                        If ei IsNot Nothing Then
                                            sValue = ei.Value
                                        End If
                                    End If

                                    Row(altAttr.ID.ToString()) = sValue
                                End If
                            Case AttributeValueTypes.avtEnumerationMulti
                                If (Val <> UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) And (Val <> "") Then
                                    Dim sValue As String = "["
                                    Dim e As clsAttributeEnumeration = ProjectManager.Attributes.GetEnumByID(altAttr.EnumID)
                                    If e IsNot Nothing Then
                                        Dim isFirstVal As Boolean = True
                                        For Each sVal As String In Val.Split(";")
                                            Dim ei As clsAttributeEnumerationItem = e.GetItemByID(New Guid(sVal))
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
                                    sValue += "]"
                                    Row(altAttr.ID.ToString()) = sValue
                                End If
                        End Select
                    End If
                Next
                Dim AltTPData As AlternativePeriodsData = RA.Scenarios.ActiveScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.NodeGuidID.ToString)
                If RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0 Then
                    Row(dgColEarliest.ToString) = CInt(AltTPData.MinPeriod + 1)
                    Row(dgColLatest.ToString) = CInt(AltTPData.MaxPeriod + 1)
                    Row(dgColDuration.ToString) = CInt(AltTPData.Duration)
                End If

                mDataSet.Tables(TABLE_NAME_DATA_GRID).Rows.Add(Row)
            Next

            If ProjectManager.User IsNot Nothing AndAlso ProjectManager.User.UserID <> UserID Then
                ProjectManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, UserID, , True)
            End If

            If Worker IsNot Nothing Then
                Worker.ReportProgress(100, "Done!")
            End If
        End Sub

        'A1221 ===
        Private ReadOnly Property ShowCostOfGoal As Boolean
            Get
                Dim retVal As Boolean = False
                If CostOfGoal <> UNDEFINED_INTEGER_VALUE Then
                    retVal = CBool(mPrjManager.Attributes.GetAttributeValue(ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
                End If
                Return retVal
            End Get
        End Property

        Private ReadOnly Property CostOfGoal As Double
            Get
                Return CDbl(mPrjManager.Attributes.GetAttributeValue(ATTRIBUTE_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
            End Get
        End Property

        Friend ReadOnly Property CostOfGoalTarget As String
            Get
                Dim retVal As String = CStr(mPrjManager.Attributes.GetAttributeValue(ATTRIBUTE_DOLLAR_VALUE_TARGET_ID, UNDEFINED_USER_ID))
                If Not String.IsNullOrEmpty(retVal) AndAlso retVal.Length >= 36 Then
                    Dim targetGuid As Guid = New Guid(retVal)

                    If Not targetGuid.Equals(Guid.Empty) Then
                        Dim Alt As clsNode = mPrjManager.AltsHierarchy(mPrjManager.ActiveAltsHierarchy).GetNodeByID(targetGuid)
                        If Alt IsNot Nothing Then
                            Return retVal
                        End If
                    End If
                End If
                Return ""
            End Get
        End Property
        'A1221 ==

        Protected Sub AddDataGridRiskTable(User As clsUser, Group As clsGroup)
            Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
            Dim UserID As Integer = CT.GetUserID

            'A1221 ===
            Dim tShowCostInDollars As Boolean = ShowCostOfGoal
            Dim tCostInDollars As Double = CostOfGoal
            Dim tCostOfGoalTarget As String = CostOfGoalTarget

            With mPrjManager
                .Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                '.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
            End With
            'A1221 ==

            mDataSet.Tables.Add(TABLE_NAME_DATA_GRID_RISK)
            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(dgColNum.ToString(), System.Type.GetType("System.Int32"))
            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(dgColAltName.ToString(), System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(dgColTotalLikelihood.ToString(), System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(dgColTotalImpact.ToString(), System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(dgColRisk.ToString(), System.Type.GetType("System.Single"))

            For Each H As clsHierarchy In ProjectManager.Hierarchies
                For Each node As clsNode In H.TerminalNodes
                    mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(node.NodeGuidID.ToString(), System.Type.GetType("System.String"))
                Next
            Next

            For Each altAttr As clsAttribute In mPrjManager.Attributes.GetAlternativesAttributes
                If mPrjManager.Attributes.IsAltAttrVisibleInReport(altAttr.ID, mPrjManager) Then 'A1224
                    Select Case altAttr.ValueType
                        Case AttributeValueTypes.avtBoolean
                            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.String"))
                        Case AttributeValueTypes.avtDouble
                            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.Double"))
                        Case AttributeValueTypes.avtLong
                            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.Int64"))
                        Case AttributeValueTypes.avtString
                            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.String"))
                        Case AttributeValueTypes.avtEnumeration
                            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.String"))
                        Case AttributeValueTypes.avtEnumerationMulti
                            mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Columns.Add(altAttr.ID.ToString(), System.Type.GetType("System.String"))
                    End Select
                End If
            Next

            Dim i As Integer = 0
            'Dim count As Integer = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Count
            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                Dim Row As DataRow
                i += 1
                'Total
                Row = mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).NewRow()
                Row(dgColNum.ToString) = i
                Row(dgColAltName.ToString) = alt.NodeName

                For Each H As clsHierarchy In ProjectManager.Hierarchies
                    ProjectManager.CalculationsManager.Calculate(CT, H.Nodes(0), H.HierarchyID, ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).HierarchyID)
                    If H.HierarchyID = ECHierarchyID.hidLikelihood Then
                        Row(dgColTotalLikelihood.ToString) = alt.UnnormalizedPriority
                    Else
                        Row(dgColTotalImpact.ToString) = alt.UnnormalizedPriority
                    End If
                    For Each node As clsNode In H.TerminalNodes
                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                Row(node.NodeGuidID.ToString()) = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                            Case ECMeasureType.mtRatings
                                Dim rData As clsRatingMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserID)
                                If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                                    Row(node.NodeGuidID.ToString()) = If(rData.Rating.ID <> -1, rData.Rating.Name, rData.Rating.Value)
                                Else
                                    Row(node.NodeGuidID.ToString()) = ""
                                End If
                            Case Else
                                Dim nonpwData As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserID)
                                If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                    Row(node.NodeGuidID.ToString()) = CSng(nonpwData.ObjectValue)
                                Else
                                    Row(node.NodeGuidID.ToString()) = ""
                                End If
                        End Select
                    Next
                Next

                'A1221 + A1224 ===
                Dim tRiskValue As Single = CSng(Row(dgColTotalLikelihood.ToString)) * CSng(Row(dgColTotalImpact.ToString))
                If tShowCostInDollars Then
                    If tCostOfGoalTarget <> "" Then
                        Dim tTarget As Guid = New Guid(tCostOfGoalTarget)
                        Dim targetAlt As clsNode = mPrjManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(tTarget)
                        Dim ImpactValueWC As Double = 1
                        If targetAlt IsNot Nothing Then
                            If ProjectManager.CalculationsManager.UseReductions Then
                                ImpactValueWC = targetAlt.UnnormalizedPriorityWithoutControls
                            Else
                                ImpactValueWC = targetAlt.UnnormalizedPriority
                            End If
                        End If
                        If ImpactValueWC = 0 Then ImpactValueWC = 1
                        Dim IMP As Double = alt.UnnormalizedPriority * tCostInDollars / ImpactValueWC
                        Row(dgColTotalImpact.ToString) = IMP
                        Row(dgColRisk.ToString) = CSng(Row(dgColTotalLikelihood.ToString)) * IMP
                    Else
                        Row(dgColRisk.ToString) = tRiskValue * tCostInDollars
                        Row(dgColTotalImpact.ToString) = alt.UnnormalizedPriority * tCostInDollars
                    End If
                Else
                    Row(dgColRisk.ToString) = tRiskValue
                End If
                'A1221 ==

                For Each altAttr As clsAttribute In mPrjManager.Attributes.GetAlternativesAttributes
                    If altAttr.ID <> ECCore.ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID And altAttr.ID <> ECCore.ATTRIBUTE_ALTERNAITVE_FUNDED_IN_CURRENT_SCENARIO_ID And altAttr.ID <> ECCore.ATTRIBUTE_RESULTS_ALTERNATIVE_IS_SELECTED_ID And altAttr.ID <> ECCore.ATTRIBUTE_RA_ALT_SORT_ID Then
                        Dim Val As String = ""
                        Try
                            Val = mPrjManager.Attributes.GetAttributeValue(altAttr.ID, alt.NodeGuidID).ToString
                        Catch ex As Exception
                            Val = ""
                        End Try
                        Select Case altAttr.ValueType
                            Case AttributeValueTypes.avtBoolean
                                If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) Then
                                    Row(altAttr.ID.ToString()) = CBool(Val)
                                End If
                            Case AttributeValueTypes.avtDouble
                                If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE OrElse Val = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE) Then
                                    Row(altAttr.ID.ToString()) = CDbl(Val)
                                End If
                            Case AttributeValueTypes.avtLong
                                If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE OrElse Val = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE) Then
                                    Row(altAttr.ID.ToString()) = CLng(Val)
                                End If
                            Case AttributeValueTypes.avtString
                                If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) Then
                                    Row(altAttr.ID.ToString()) = CStr(Val)
                                End If
                            Case AttributeValueTypes.avtEnumeration
                                If Not (Val = UNDEFINED_ATTRIBUTE_DEFAULT_VALUE) Then
                                    Dim sValue As String = ""
                                    Dim e As clsAttributeEnumeration = ProjectManager.Attributes.GetEnumByID(altAttr.EnumID)
                                    If e IsNot Nothing AndAlso Not String.IsNullOrEmpty(Val) Then 'A1131
                                        Dim ei As clsAttributeEnumerationItem = e.GetItemByID(New Guid(Val))
                                        If ei IsNot Nothing Then
                                            sValue = ei.Value
                                        End If
                                    End If

                                    Row(altAttr.ID.ToString()) = sValue
                                End If
                        End Select
                    End If
                Next

                mDataSet.Tables(TABLE_NAME_DATA_GRID_RISK).Rows.Add(Row)
            Next

            If ProjectManager.User IsNot Nothing AndAlso ProjectManager.User.UserID <> UserID Then
                ProjectManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, UserID, , True)
            End If

            If Worker IsNot Nothing Then
                Worker.ReportProgress(100, "Done!")
            End If
        End Sub

        Protected Sub AddInconsistencyTable(User As clsUser, Group As clsGroup)
            mDataSet.Tables.Add(TABLE_NAME_INCONSISTENCIES)
            mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).Columns.Add("UserName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).Columns.Add("UserEmail", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).Columns.Add("NodeGUID", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).Columns.Add("NodeName", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).Columns.Add("NodePath", System.Type.GetType("System.String"))
            mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).Columns.Add("Inconsistency", System.Type.GetType("System.Single"))
            mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).Columns.Add("NumberOfChildren", System.Type.GetType("System.Int32"))

            Dim oldUseCIS As Boolean = ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes
            ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes = False

            If mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes.Count > 0 Then
                Dim UserIDs As ArrayList = GetUserIDsList(User, Group)
                For Each UserID As Integer In UserIDs
                    Dim u As clsUser = ProjectManager.GetUserByID(UserID)
                    ProjectManager.StorageManager.Reader.LoadUserData(u)
                    For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                        If node.MeasureType = ECMeasureType.mtPairwise AndAlso node.IsAllowed(u.UserID) Then
                            If CType(node.Judgments, clsPairwiseJudgments).HasSpanningSet(u.UserID) Then
                                node.CalculateLocal(u.UserID)

                                Dim NodePath As String = "" 'L0493
                                GetFullNodePath(node, NodePath) 'L0493

                                Dim Row As DataRow
                                Row = mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).NewRow()
                                Row("UserName") = u.UserName 'L0493
                                Row("UserEmail") = u.UserEMail 'L0493
                                Row("NodeGUID") = node.NodeGuidID.ToString  ' D2823
                                Row("NodeName") = node.NodeName 'L0493
                                Row("NodePath") = NodePath 'L0493
                                Row("Inconsistency") = CType(node.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                                Row("NumberOfChildren") = node.GetNodesBelow(u.UserID).Count 'L0493
                                mDataSet.Tables(TABLE_NAME_INCONSISTENCIES).Rows.Add(Row)
                            End If
                        End If
                    Next
                Next
            End If
            ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes = oldUseCIS
        End Sub

        Protected Function GetReport(User As clsUser, Group As clsCombinedGroup, Optional GroupType As GroupReportType = GroupReportType.grtGroupOnly) As DataSet
            mDataSet = Nothing
            mDataSet = New DataSet

            mGroupType = GroupType

            If TypeOf ProjectManager Is clsProjectManager Then
                Dim cpm As clsProjectManager = ProjectManager
                cpm.LoadPipeParameters(PipeStorageType.pstStreamsDatabase, cpm.StorageManager.ModelID)
            End If

            Select Case mReportType
                Case CanvasReportType.crtAllJudgments, CanvasReportType.crtJudgmentsAlts, CanvasReportType.crtJudgmentsObjs ' D3703
                    AddHierarchyStructureTable(False)
                    LoadJudgments(User, Group)
                    AddAllJudgmentsTables(User, Group, mReportType) ' D3703
                Case CanvasReportType.crtAllJudgmentsInOne
                    LoadJudgments(User, Group)
                    AddAllJudgmentsTable(User, Group)
                    'Case CanvasReportType.crtHierarchyJudgments
                    '    LoadJudgments(User, Group)
                    '    AddHierarchyStructureTable(False)
                    '    AddHierarchyJudgmentsTable(User, Group)
                    'Case CanvasReportType.crtAltsPairwiseJudgments
                    '    LoadJudgments(User, Group)
                    '    AddHierarchyStructureTable(False)
                    '    AddAltsPairwiseJudgmentsTable(User, Group)
                    'Case CanvasReportType.crtAltsRatingsJudgments
                    '    LoadJudgments(User, Group)
                    '    AddHierarchyStructureTable(False)
                    '    AddAltsRatingsJudgmentsTable(User, Group)
                Case CanvasReportType.crtHierarchyObjectives
                    AddHierarchyStructureTable(False)
                Case CanvasReportType.crtHierarchyObjectives2
                    AddHierarchyStructure2Table()
                Case CanvasReportType.crtHierarchyObjectivesAndAlternatives
                    AddHierarchyStructureTable(True)
                Case CanvasReportType.crtObjectivesPriorities
                    LoadJudgments(User, Group)
                    Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
                    mPrjManager.CalculationsManager.Calculate(CT, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)
                    AddStructureWithPrioritiesTable(User, Group, False)
                Case CanvasReportType.crtObjectivesAndAlternativesPriorities
                    LoadData(User, Group)
                    Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
                    mPrjManager.CalculationsManager.Calculate(CT, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)
                    AddStructureWithPrioritiesTable(User, Group, True)
                Case CanvasReportType.crtOverallAlternativesPriorities
                    LoadData(User, Group)
                    Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
                    mPrjManager.CalculationsManager.Calculate(CT, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), mPrjManager.ActiveHierarchy, mPrjManager.ActiveAltsHierarchy)
                    AddOverallAlternativesPriorities()
                    'Case CanvasReportType.crtPivotPriorities
                    '    AddPivotPriorities(mPrjManager.UserID)
                Case CanvasReportType.crtUserPermissions
                    LoadPermissions(User, Group)
                    'TODO: handle new roles
                    AddUserPermissions(User, Group)
                    'Case CanvasReportType.crtSilverlightReport2
                    '    AddStructureWithPrioritiesTable(User, Group, True)
                    '    AddAltsPairwiseJudgmentsTable(User, Group)
                    '    AddAltsRatingsJudgmentsTable(User, Group)
                    '    AddAltsNonRatingsJudgmentsTable(User, Group)
                Case CanvasReportType.crtSilverlightReport
                    LoadData(User, Group)
                    AddUserAltsJudgmentsTable(mPrjManager.UserID)
                    Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
                    mPrjManager.CalculationsManager.Calculate(CT, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0), ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy)
                    AddUserObjectivesPrioritiesTable(mPrjManager.UserID)
                    AddUserAlternativesPrioritiesTable(mPrjManager.UserID)
                Case CanvasReportType.crtSilverlightReportInitialData
                    AddObjectivesListTable()
                    AddAlternativesListTable()
                    AddUsersListTable()
                Case CanvasReportType.crtHierarchyObjectivesAndAlternatives2 'C0250
                    AddHierarchyStructureTable(False)
                    AddAlternativesListTable()
                Case CanvasReportType.crtMaxOutPriorities
                    LoadData(User, Group)
                    ProjectManager.StorageManager.Reader.LoadUserJudgments(Nothing)
                    AddMaxOutPrioritiesTable()
                Case CanvasReportType.crtLocalPrioritiesForSilverlight
                    AddShortAlternativesListTable()

                    LoadData(User, Group)
                    Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
                    AddObjectivesWithPrioritiesTable(User, Group)
                    AddAlternativesLocalPrioritiesTable(User, Group)
                Case CanvasReportType.crtConsensusView
                    AddObjectivesListTable()
                    AddShortAlternativesListTable()
                    AddConsensusViewTable()
                Case CanvasReportType.crtConsensusViewOnly
                    AddConsensusViewTable()
                Case CanvasReportType.crtUsersObjectivesPriorities
                    AddUsersObjectivesPrioritiesTable(User, Group)
                Case CanvasReportType.crtEvaluationProgress
                    AddEvaluationProgressTable()
                    'Case CanvasReportType.crtOverallResults1, CanvasReportType.crtOverallResults2, CanvasReportType.crtPivotAlternativesPriorities
                Case CanvasReportType.crtPivotAlternativesPriorities
                    AddPivotAlternativesPrioritiesTable(User, Group)
                Case CanvasReportType.crtDataGrid
                    LoadData(User, Group)
                    Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)

                    'Dim IdealAlt As Boolean = ProjectManager.CalculationsManager.IncludeIdealAlternative
                    'ProjectManager.CalculationsManager.IncludeIdealAlternative = False
                    ProjectManager.CalculationsManager.Calculate(CT, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))
                    'ProjectManager.CalculationsManager.IncludeIdealAlternative = IdealAlt

                    AddDataGridTable(User, Group)

                Case CanvasReportType.crtDataGridRisk
                    Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
                    ProjectManager.CalculationsManager.UseReductions = False 'A0807
                    AddDataGridRiskTable(User, Group)
                Case CanvasReportType.crtDataGridRiskWithControls
                    Dim CT As clsCalculationTarget = GetCalulationTarget(User, Group)
                    ProjectManager.CalculationsManager.UseReductions = True 'A0807
                    AddDataGridRiskTable(User, Group)
                Case CanvasReportType.crtInconsistencies 'C1004
                    AddHierarchyStructureTable(False)
                    AddInconsistencyTable(User, Group)
            End Select

            Return mDataSet
        End Function

        Public Property ReportType() As CanvasReportType
            Get
                Return mReportType
            End Get
            Set(ByVal value As CanvasReportType)
                mReportType = value
            End Set
        End Property

        Public Overloads ReadOnly Property ReportDataSet(ByVal ReportType As CanvasReportType, Optional ByVal UsersIDList As ArrayList = Nothing, Optional ByVal AUserID As Integer = Integer.MinValue) As DataSet
            Get
                mReportType = ReportType

                Dim CG As clsCombinedGroup
                If AUserID <> Integer.MinValue Then
                    If AUserID >= 0 Then
                        Dim User As clsUser = ProjectManager.GetUserByID(AUserID)
                        If User IsNot Nothing Then
                            Return ReportDataSet(ReportType, User)
                        End If
                    Else
                        If IsCombinedUserID(AUserID) Then
                            CG = ProjectManager.CombinedGroups.GetCombinedGroupByUserID(AUserID)
                            If CG IsNot Nothing Then
                                Return ReportDataSet(ReportType, CG)
                            End If
                        End If
                    End If

                    CG = ProjectManager.CombinedGroups.GetDefaultCombinedGroup
                    Return ReportDataSet(ReportType, CG)
                Else
                    CG = ProjectManager.CombinedGroups.GetDefaultCombinedGroup
                    Return ReportDataSet(ReportType, CG)
                End If
            End Get
        End Property

        Public Property Worker() As System.ComponentModel.BackgroundWorker 'C0771
            Get
                Return mWorker
            End Get
            Set(ByVal value As System.ComponentModel.BackgroundWorker)
                mWorker = value
            End Set
        End Property

        Public Overloads ReadOnly Property ReportDataSet(ByVal ReportType As CanvasReportType, User As clsUser) As DataSet
            Get
                mReportType = ReportType
                Return GetReport(User, Nothing)
            End Get
        End Property

        Public Overloads ReadOnly Property ReportDataSet(ByVal ReportType As CanvasReportType, Group As clsCombinedGroup, Optional GroupType As GroupReportType = GroupReportType.grtGroupOnly) As DataSet
            Get
                mReportType = ReportType
                Return GetReport(Nothing, Group, GroupType)
            End Get
        End Property

        Public Sub New(ByVal ProjectManager As clsProjectManager)
            mReportCommentType = ReportCommentType.rctInfoDoc 'C0580
            mPrjManager = ProjectManager
            mReportType = CanvasReportType.crtAllJudgments
            'mWRTNodeID = Integer.MinValue 'C0816
        End Sub
    End Class
End Namespace
