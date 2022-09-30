Imports ECCore
Imports System.IO
Imports System.Data.Common
Imports ECSecurity.ECSecurity 'C0380
Imports System.Text

<Serializable()> Public MustInherit Class clsStreamModelWriter 'C0345
    <NonSerialized()> Protected mStream As Stream     ' D0360
    <NonSerialized()> Protected mBW As BinaryWriter   ' D0360

    Protected mProjectManager As clsProjectManager
    Protected mWriteStatus As Boolean = False

    Public Property BinaryStream() As Stream
        Get
            Return mStream
        End Get
        Set(ByVal value As Stream)
            mStream = value
        End Set
    End Property

    Public Property ProjectManager() As clsProjectManager
        Get
            Return mProjectManager
        End Get
        Set(ByVal value As clsProjectManager)
            mProjectManager = value
        End Set
    End Property

    Protected MustOverride Function ProcessUsersGroupsChunk() As Boolean
    Protected MustOverride Function ProcessUsersListChunk() As Boolean
    Protected MustOverride Function ProcessCombinedGroupsUsersChunk() As Boolean
    Protected MustOverride Function ProcessDataInstancesChunk() As Boolean
    Protected MustOverride Function ProcessMeasurementScalesChunk() As Boolean
    Protected MustOverride Function ProcessNodeChunk(ByVal node As clsNode) As Boolean 'C0642
    Protected MustOverride Function ProcessHierarchiesChunk() As Boolean
    Protected MustOverride Function ProcessAlternativesContributionChunk() As Boolean
    Protected MustOverride Function ProcessCustomAttributesChunk() As Boolean 'C1020

    Public MustOverride Function WriteModelStructure() As Boolean
    Public MustOverride Function WriteDataMapping() As Boolean
    Public MustOverride Function WriteEventsGroups() As Boolean
    Public MustOverride Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
    Public MustOverride Function WriteUserJudgmentsControls(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean
    Public MustOverride Function WriteUserPermissions(ByVal User As clsUser) As Boolean
    Public MustOverride Function WriteCombinedGroupPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
    Public MustOverride Function WritePermissions(ByVal UserID As Integer) As Boolean
    Public MustOverride Function WriteControlsPermissions(ByVal UserID As Integer) As Boolean
    Public MustOverride Function WriteUserDisabledNodes(ByVal User As clsUser) As Boolean
    Public MustOverride Function WriteInfoDocs() As Boolean
    Public MustOverride Function WriteAdvancedInfoDocs() As Boolean 'C0920

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_7
    Inherits clsStreamModelWriter

    Public Overrides Function WriteDataMapping() As Boolean
        Return True
    End Function

    Public Overrides Function WriteEventsGroups() As Boolean
        Return True
    End Function

    Public Overrides Function WriteUserJudgmentsControls(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean
        Return True
    End Function

    Public Overrides Function WriteControlsPermissions(ByVal UserID As Integer) As Boolean
        Return True
    End Function

    Public Overrides Function WriteCombinedGroupPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
        Return True
    End Function

    Public Overrides Function WritePermissions(ByVal UserID As Integer) As Boolean
        Return True
    End Function

    Protected Overrides Function ProcessNodeChunk(ByVal node As clsNode) As Boolean 'C0642
    End Function

    Protected Overrides Function ProcessUsersGroupsChunk() As Boolean
        mBW.Write(CHUNK_USERS_GROUPS)

        Dim UsersGroupsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(CInt(ProjectManager.EvaluationGroups.GroupsList.Count + ProjectManager.CombinedGroups.GroupsList.Count))

        Dim UserGroupChunkSizePosition As Integer 'C0262
        Dim UserGroupChunkSize As Integer 'C0262

        For Each evalGroup As clsGroup In ProjectManager.EvaluationGroups.GroupsList
            mBW.Write(CHUNK_USER_GROUP)

            UserGroupChunkSizePosition = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(evalGroup.ID)
            'mBW.Write(evalGroup.GuidID.ToByteArray) 'C0261
            mBW.Write(evalGroup.Name)
            mBW.Write(evalGroup.Rule)
            mBW.Write(CInt(ECGroupType.gtEvaluation))
            mBW.Write(Integer.MaxValue) ' dummy value for CombinedUserID for Combined Groups

            'C0262===
            UserGroupChunkSize = mBW.BaseStream.Position - UserGroupChunkSizePosition 'C0262
            mBW.Seek(-UserGroupChunkSize, SeekOrigin.Current)
            mBW.Write(UserGroupChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        For Each combinedGroup As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
            mBW.Write(CHUNK_USER_GROUP)

            UserGroupChunkSizePosition = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(combinedGroup.ID)
            'mBW.Write(combinedGroup.CombinedUserGuidID.ToByteArray) 'C0261
            mBW.Write(combinedGroup.Name)
            mBW.Write(combinedGroup.Rule)
            mBW.Write(CInt(ECGroupType.gtCombined))
            mBW.Write(combinedGroup.CombinedUserID)

            'C0262===
            UserGroupChunkSize = mBW.BaseStream.Position - UserGroupChunkSizePosition 'C0262
            mBW.Seek(-UserGroupChunkSize, SeekOrigin.Current)
            mBW.Write(UserGroupChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim UsersGroupsChunkSize As Integer = mBW.BaseStream.Position - UsersGroupsChunkSizePosition
        mBW.Seek(-UsersGroupsChunkSize, SeekOrigin.Current)
        mBW.Write(UsersGroupsChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessUsersGroups completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessUsersListChunk() As Boolean
        mBW.Write(CHUNK_USERS_LIST)

        Dim UsersListChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(CInt(ProjectManager.UsersList.Count + ProjectManager.DataInstanceUsers.Count))

        Dim users As New ArrayList
        For Each user As clsUser In ProjectManager.UsersList
            'If user.UserID <> COMBINED_USER_ID Then 'C0555
            If Not IsCombinedUserID(user.UserID) Then 'C0555
                users.Add(user)
            End If
        Next
        For Each user As clsUser In ProjectManager.DataInstanceUsers
            users.Add(user)
        Next

        Dim UserChunkSizePosition As Integer 'C0262
        Dim UserChunkSize As Integer 'C0262

        For Each user As clsUser In users
            mBW.Write(CHUNK_USER)

            UserChunkSizePosition = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(user.UserID)
            mBW.Write(user.UserGuidID.ToByteArray) 'C0261
            mBW.Write(user.UserName)
            mBW.Write(user.UserEMail)
            mBW.Write(user.Active)
            mBW.Write(user.VotingBoxID)
            mBW.Write(user.IncludedInSynchronous)
            mBW.Write(user.DataInstanceID)
            If user.EvaluationGroup IsNot Nothing Then
                mBW.Write(user.EvaluationGroup.ID)
            Else
                mBW.Write(CInt(-1))
            End If

            'C0343===
            If user.AHPUserData Is Nothing Then
                mBW.Write(CInt(0))
            Else
                mBW.Write(user.AHPUserData.ToStream.ToArray.Length)
                mBW.Write(user.AHPUserData.ToStream.ToArray)
            End If
            'C0343==

            'C0262===
            UserChunkSize = mBW.BaseStream.Position - UserChunkSizePosition
            mBW.Seek(-UserChunkSize, SeekOrigin.Current)
            mBW.Write(UserChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim UsersListChunkSize As Integer = mBW.BaseStream.Position - UsersListChunkSizePosition
        mBW.Seek(-UsersListChunkSize, SeekOrigin.Current)
        mBW.Write(UsersListChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessUsersList completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessCombinedGroupsUsersChunk() As Boolean
        Dim count As Integer = 0
        For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
            count += CG.UsersList.Count
        Next

        mBW.Write(CHUNK_COMBINED_GROUPS_USERS)

        Dim CombinedGroupsUsersChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(count)

        For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
            For Each user As clsUser In CG.UsersList
                mBW.Write(CG.ID)
                mBW.Write(user.UserID)
                'mBW.Write(CG.CombinedUserGuidID.ToByteArray) 'C0261
                'mBW.Write(user.UserGuidID.ToByteArray) 'C0261
            Next
        Next

        'C0262===
        Dim CombinedGroupsUsersChunkSize As Integer = mBW.BaseStream.Position - CombinedGroupsUsersChunkSizePosition
        mBW.Seek(-CombinedGroupsUsersChunkSize, SeekOrigin.Current)
        mBW.Write(CombinedGroupsUsersChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessCombinedGroupsUsers completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessDataInstancesChunk() As Boolean
        mBW.Write(CHUNK_DATA_INSTANCES)

        Dim DataInstancesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.DataInstances.Count)

        Dim DIChunkSizePosition As Integer 'C0262
        Dim DIChunkSize As Integer 'C0262

        For Each DI As clsDataInstance In ProjectManager.DataInstances
            mBW.Write(CHUNK_DATA_INSTANCE)

            DIChunkSizePosition = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(DI.ID)
            'mBW.Write(DI.GuidID.ToByteArray) 'C0261
            mBW.Write(DI.Name)
            mBW.Write(DI.User.UserID)
            'mBW.Write(DI.User.UserGuidID.ToByteArray) 'C0261
            mBW.Write(DI.Comment)

            'C0262===
            DIChunkSize = mBW.BaseStream.Position - DIChunkSizePosition
            mBW.Seek(-DIChunkSize, SeekOrigin.Current)
            mBW.Write(DIChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim DataInstancesChunkSize As Integer = mBW.BaseStream.Position - DataInstancesChunkSizePosition
        mBW.Seek(-DataInstancesChunkSize, SeekOrigin.Current)
        mBW.Write(DataInstancesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessDataInstances completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessMeasurementScalesChunk() As Boolean
        mBW.Write(CHUNK_MEASUREMENT_SCALES)

        Dim MeasurementScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.MeasureScales.AllScales.Count)

        If ProjectManager.MeasureScales.RatingsScales.Count <> 0 Then
            mBW.Write(CHUNK_RATINGS_SCALES)

            Dim RatingScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RatingsScales.Count)
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                mBW.Write(CHUNK_RATING_SCALE)

                Dim RSChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.ID)
                mBW.Write(RS.GuidID.ToByteArray) 'C0283
                mBW.Write(RS.Name)
                mBW.Write(RS.Comment)

                mBW.Write(CHUNK_RATING_INTENSITIES)

                Dim RIntensitiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.RatingSet.Count)

                For Each R As clsRating In RS.RatingSet
                    If R.Value < 0 Then
                        R.Value = 0
                    End If

                    If R.Value > 1 Then
                        R.Value = 1
                    End If

                    mBW.Write(CHUNK_RATING_INTENSITY)

                    Dim RIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(R.ID)
                    mBW.Write(R.GuidID.ToByteArray) 'C0261
                    mBW.Write(R.Name)
                    mBW.Write(R.Value)
                    mBW.Write(R.Comment)

                    'C0262===
                    Dim RIChunkSize As Integer = mBW.BaseStream.Position - RIChunkSizePosition
                    mBW.Seek(-RIChunkSize, SeekOrigin.Current)
                    mBW.Write(RIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim RIntensitiesChunkSize As Integer = mBW.BaseStream.Position - RIntensitiesChunkSizePosition
                mBW.Seek(-RIntensitiesChunkSize, SeekOrigin.Current)
                mBW.Write(RIntensitiesChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim RSChunkSize As Integer = mBW.BaseStream.Position - RSChunkSizePosition
                mBW.Seek(-RSChunkSize, SeekOrigin.Current)
                mBW.Write(RSChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

            Next

            'C0262===
            Dim RatingScalesChunkSize As Integer = mBW.BaseStream.Position - RatingScalesChunkSizePosition
            mBW.Seek(-RatingScalesChunkSize, SeekOrigin.Current)
            mBW.Write(RatingScalesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.RegularUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_REGULAR_UTILITY_CURVES)

            Dim RUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RegularUtilityCurves.Count)
            For Each RUC As clsRegularUtilityCurve In ProjectManager.MeasureScales.RegularUtilityCurves
                mBW.Write(CHUNK_REGULAR_UTILITY_CURVE)

                Dim RUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RUC.ID)
                mBW.Write(RUC.GuidID.ToByteArray) 'C0283
                mBW.Write(RUC.Name)
                mBW.Write(RUC.Low)
                mBW.Write(RUC.High)
                mBW.Write(RUC.Curvature)
                mBW.Write(RUC.IsIncreasing)
                mBW.Write(RUC.Comment)

                'C0262===
                Dim RUCChunkSize As Integer = mBW.BaseStream.Position - RUCChunkSizePosition
                mBW.Seek(-RUCChunkSize, SeekOrigin.Current)
                mBW.Write(RUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim RUCsChunkSize As Integer = mBW.BaseStream.Position - RUCsChunkSizePosition
            mBW.Seek(-RUCsChunkSize, SeekOrigin.Current)
            mBW.Write(RUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.AdvancedUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_ADVANCED_UTILITY_CURVES)

            Dim AUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.AdvancedUtilityCurves.Count)
            For Each AUC As clsAdvancedUtilityCurve In ProjectManager.MeasureScales.AdvancedUtilityCurves
                mBW.Write(CHUNK_ADVANCED_UTILITY_CURVE)

                Dim AUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.ID)
                mBW.Write(AUC.GuidID.ToByteArray) 'C0283
                mBW.Write(AUC.Name)
                mBW.Write(CInt(AUC.InterpolationMethod))
                mBW.Write(AUC.Comment)

                mBW.Write(CHUNK_AUC_POINTS)

                Dim AUCPointsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.Points.Count)

                For Each point As clsUCPoint In AUC.Points
                    mBW.Write(point.X)
                    mBW.Write(point.Y)
                Next

                'C0262===
                Dim AUCPointsChunkSize As Integer = mBW.BaseStream.Position - AUCPointsChunkSizePosition
                mBW.Seek(-AUCPointsChunkSize, SeekOrigin.Current)
                mBW.Write(AUCPointsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim AUCChunkSize As Integer = mBW.BaseStream.Position - AUCChunkSizePosition
                mBW.Seek(-AUCChunkSize, SeekOrigin.Current)
                mBW.Write(AUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim AUCsChunkSize As Integer = mBW.BaseStream.Position - AUCsChunkSizePosition
            mBW.Seek(-AUCsChunkSize, SeekOrigin.Current)
            mBW.Write(AUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.StepFunctions.Count <> 0 Then
            mBW.Write(CHUNK_STEP_FUNCTIONS)

            Dim SFsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.StepFunctions.Count)
            For Each SF As clsStepFunction In ProjectManager.MeasureScales.StepFunctions
                mBW.Write(CHUNK_STEP_FUNCTION)

                Dim SFChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.ID)
                mBW.Write(SF.GuidID.ToByteArray) 'C0283
                mBW.Write(SF.Name)
                mBW.Write(SF.IsPiecewiseLinear) 'C0329
                mBW.Write(SF.Comment)

                mBW.Write(CHUNK_STEP_INTERVALS)

                Dim SIsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.Intervals.Count)
                For Each interval As clsStepInterval In SF.Intervals
                    mBW.Write(CHUNK_STEP_INTERVAL)

                    Dim SIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(interval.ID)
                    'mBW.Write(interval.GuidID.ToByteArray)
                    mBW.Write(interval.Name)
                    mBW.Write(interval.Low)
                    mBW.Write(interval.High)
                    mBW.Write(interval.Value)
                    mBW.Write(interval.Comment)

                    'C0262===
                    Dim SIChunkSize As Integer = mBW.BaseStream.Position - SIChunkSizePosition
                    mBW.Seek(-SIChunkSize, SeekOrigin.Current)
                    mBW.Write(SIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim SIsChunkSize As Integer = mBW.BaseStream.Position - SIsChunkSizePosition
                mBW.Seek(-SIsChunkSize, SeekOrigin.Current)
                mBW.Write(SIsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim SFChunkSize As Integer = mBW.BaseStream.Position - SFChunkSizePosition
                mBW.Seek(-SFChunkSize, SeekOrigin.Current)
                mBW.Write(SFChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim SFsChunkSize As Integer = mBW.BaseStream.Position - SFsChunkSizePosition
            mBW.Seek(-SFsChunkSize, SeekOrigin.Current)
            mBW.Write(SFsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        'C0262===
        Dim MeasurementScalesChunkSize As Integer = mBW.BaseStream.Position - MeasurementScalesChunkSizePosition
        mBW.Seek(-MeasurementScalesChunkSize, SeekOrigin.Current)
        mBW.Write(MeasurementScalesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessMeasurementScales completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean
        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.Nodes.Clear()
            hierarchy.ResetNodesDictionaries()
        Next

        mBW.Write(CHUNK_HIERARCHIES)

        Dim HierarchiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.GetAllHierarchies.Count)

        For Each H As clsHierarchy In ProjectManager.GetAllHierarchies
            mBW.Write(CHUNK_HIERARCHY)

            Dim HierarchyChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(H.HierarchyID)
            mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
            mBW.Write(CInt(H.HierarchyType))
            mBW.Write(H.HierarchyName)
            mBW.Write(H.Comment)

            mBW.Write(CHUNK_HIERARCHY_NODES)

            Dim HNodesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(H.Nodes.Count)
            For Each node As clsNode In H.Nodes
                mBW.Write(CHUNK_HIERARCHY_NODE)

                Dim NodeChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(node.NodeID)
                mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
                mBW.Write(node.NodeName)
                mBW.Write(node.ParentNodeID)
                mBW.Write(CInt(node.MeasureType))
                mBW.Write(node.MeasurementScaleID)
                mBW.Write(CInt(node.MeasureMode))
                mBW.Write(node.Enabled)
                If node.DefaultDataInstance IsNot Nothing Then
                    mBW.Write(node.DefaultDataInstance.ID)
                Else
                    mBW.Write(CInt(-1))
                End If
                mBW.Write(node.Comment)

                'C0343===
                Select Case H.HierarchyType
                    Case ECHierarchyType.htModel
                        If node.AHPNodeData Is Nothing Then
                            mBW.Write(CInt(0))
                        Else
                            mBW.Write(node.AHPNodeData.ToStream.ToArray.Length)
                            mBW.Write(node.AHPNodeData.ToStream.ToArray)
                        End If
                    Case ECHierarchyType.htAlternative
                        If node.AHPAltData Is Nothing Then
                            mBW.Write(CInt(0))
                        Else
                            mBW.Write(node.AHPAltData.ToStream.ToArray.Length)
                            mBW.Write(node.AHPAltData.ToStream.ToArray)
                        End If
                    Case Else
                        mBW.Write(CInt(0))
                End Select

                Dim NodeChunkSize As Integer = mBW.BaseStream.Position - NodeChunkSizePosition
                mBW.Seek(-NodeChunkSize, SeekOrigin.Current)
                mBW.Write(NodeChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim HNodesChunkSize As Integer = mBW.BaseStream.Position - HNodesChunkSizePosition
            mBW.Seek(-HNodesChunkSize, SeekOrigin.Current)
            mBW.Write(HNodesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==

            'C0262===
            Dim HierarchyChunkSize As Integer = mBW.BaseStream.Position - HierarchyChunkSizePosition
            mBW.Seek(-HierarchyChunkSize, SeekOrigin.Current)
            mBW.Write(HierarchyChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim HierarchiesChunkSize As Integer = mBW.BaseStream.Position - HierarchiesChunkSizePosition
        mBW.Seek(-HierarchiesChunkSize, SeekOrigin.Current)
        mBW.Write(HierarchiesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessHierarchies completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessAlternativesContributionChunk() As Boolean
        mBW.Write(CHUNK_ALTERNATIVES_CONTRIBUTION)

        Dim AltsContributionChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.Hierarchies.Count * ProjectManager.AltsHierarchies.Count)

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_ALTERNATIVES_CONTRIBUTION_SET)

                Dim ACSetChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyID)
                mBW.Write(AH.HierarchyID)

                Dim terminalNodes As List(Of clsNode) = H.TerminalNodes 'C0384

                Dim count As Integer = 0
                'For Each node As clsNode In H.TerminalNodes 'C0265
                For Each node As clsNode In terminalNodes 'C0265
                    count += node.ChildrenAlts.Count
                Next

                mBW.Write(count)

                'For Each node As clsNode In H.TerminalNodes 'C0265
                For Each node As clsNode In terminalNodes 'C0265
                    For Each altID As Integer In node.ChildrenAlts
                        mBW.Write(node.NodeID)
                        mBW.Write(altID)
                    Next
                Next

                'C0262===
                Dim ACSetChunkSize As Integer = mBW.BaseStream.Position - ACSetChunkSizePosition
                mBW.Seek(-ACSetChunkSize, SeekOrigin.Current)
                mBW.Write(ACSetChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        'C0262===
        Dim AltsContributionChunkSize As Integer = mBW.BaseStream.Position - AltsContributionChunkSizePosition
        mBW.Seek(-AltsContributionChunkSize, SeekOrigin.Current)
        mBW.Write(AltsContributionChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessAlternativesContribution completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessCustomAttributesChunk() As Boolean 'C1020
        mBW.Write(CHUNK_CUSTOM_ATTRIBUTES)

        Dim CustomAttributesChunkSizePosition As Integer = mBW.BaseStream.Position
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

        Dim CustomAttributesChunkSize As Integer = mBW.BaseStream.Position - CustomAttributesChunkSizePosition
        mBW.Seek(-CustomAttributesChunkSize, SeekOrigin.Current)
        mBW.Write(CustomAttributesChunkSize)
        mBW.Seek(0, SeekOrigin.End)

        If mWriteStatus Then
            'Debug.Print("ProcessCustomAttributes completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Public Overrides Function WriteModelStructure() As Boolean
        If mWriteStatus Then
            'Debug.Print("Write Model Structure start " + Now.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        ProcessUsersGroupsChunk()
        ProcessUsersListChunk()
        ProcessCombinedGroupsUsersChunk()
        ProcessDataInstancesChunk()
        ProcessMeasurementScalesChunk()
        ProcessHierarchiesChunk()
        ProcessAlternativesContributionChunk()

        mBW.Close()
        If mWriteStatus Then
            'Debug.Print("Write Model Structure end " + Now.ToString)
        End If

        Return True
    End Function

    'Public Overrides Function WriteUserJudgments(ByVal User As clsUser) As Boolean 'C0369
    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt(ProjectManager.Hierarchies.Count * ProjectManager.AltsHierarchies.Count))

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                'mBW.Write(H.HierarchyID) 'C0261
                'mBW.Write(AH.HierarchyID)'C0261
                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    'mBW.Write(node.NodeID) 'C0261
                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                        For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                            If Not J.IsUndefined Then
                                count += 1
                            End If
                        Next
                        For Each J As clsCustomMeasureData In node.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                            If Not J.IsUndefined Then
                                count += 1
                            End If
                        Next
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                        If node.MeasureType = ECMeasureType.mtPairwise Then
                            For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not pwData.IsUndefined Then 'C0272
                                    'mBW.Write(pwData.FirstNodeID) 'C0261
                                    'mBW.Write(pwData.SecondNodeID)'C0261

                                    'mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                    'mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275

                                    'C0275===
                                    If node.IsTerminalNode Then
                                        mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                        mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                    Else
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                    End If
                                    'C0275==

                                    mBW.Write(pwData.Advantage)
                                    mBW.Write(pwData.Value)
                                    mBW.Write(pwData.Comment)
                                    'C0369===
                                    If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                        pwData.ModifyDate = JudgmentsTime
                                    End If
                                    'C0369==
                                    mBW.Write(pwData.ModifyDate.ToBinary)
                                End If
                            Next
                        Else
                            For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not nonpwData.IsUndefined Then 'C0272
                                    'mBW.Write(nonpwData.NodeID) 'C0261
                                    'mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray) 'C0261 'C0275

                                    'C0275===
                                    If node.IsTerminalNode Then
                                        mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                    Else
                                        mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                    End If
                                    'C0275==

                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtRatings
                                            'mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.ID) 'C0261
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                        Case ECMeasureType.mtStep
                                            mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                        Case ECMeasureType.mtDirect
                                            mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                    End Select
                                    mBW.Write(nonpwData.Comment)
                                    'C0369===
                                    If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                        nonpwData.ModifyDate = JudgmentsTime
                                    End If
                                    'C0369==
                                    mBW.Write(nonpwData.ModifyDate.ToBinary)
                                End If
                            Next
                        End If
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function

    Public Overrides Function WriteUserPermissions(ByVal User As clsUser) As Boolean
        mBW = New BinaryWriter(mStream)

        If mWriteStatus Then
            'Debug.Print("ProcessUserPermissions started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim isAllowed As Boolean = False ' for now only

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            mBW.Write(CHUNK_NODES_PERMISSIONS)

            Dim NodesPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(isAllowed) ' for now we will store restricted nodes
            'mBW.Write(H.HierarchyID) 'C0261
            mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
            Dim count As Integer = 0
            For Each node As clsNode In H.Nodes
                'If node.UserPermissions(User.UserID).Write = isAllowed Then 'C0901
                If node.IsAllowed(User.UserID) = isAllowed Then 'C0901
                    count += 1
                End If
            Next

            mBW.Write(count)

            For Each node As clsNode In H.Nodes
                'If node.UserPermissions(User.UserID).Write = isAllowed Then 'C0901
                If node.IsAllowed(User.UserID) = isAllowed Then 'C0901
                    'mBW.Write(node.NodeID) 'C0261
                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
                End If
            Next

            'C0262===
            Dim NodesPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodesPermissionsChunkSizePosition
            mBW.Seek(-NodesPermissionsChunkSize, SeekOrigin.Current)
            mBW.Write(NodesPermissionsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_ALTERNATIVES_PERMISSIONS)

                Dim AltsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                'mBW.Write(H.HierarchyID) 'C0261
                'mBW.Write(AH.HierarchyID) 'C0261
                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.TerminalNodes.Count)

                'For Each node As clsNode In H.TerminalNodes.Clone 'C0385
                For Each node As clsNode In H.TerminalNodes 'C0385
                    mBW.Write(CHUNK_NODE_ALTERNATIVES_PERMISSIONS)

                    Dim NodeAltsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    'mBW.Write(node.NodeID) 'C0261
                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
                    mBW.Write(isAllowed)
                    'mBW.Write(CType(node.UserPermissions(User.UserID).SpecialPermissions, clsNodePermissions).RestrictedNodesBelow.Count) 'C0901
                    mBW.Write(ProjectManager.UsersRoles.GetRestrictedAlternatives(User.UserID, node.NodeGuidID).Count) 'C0901

                    'Dim altsList As List(Of clsNode) = If(isAllowed, CType(node.UserPermissions(User.UserID).SpecialPermissions, clsNodePermissions).AllowedNodesBelow, _
                    '                                CType(node.UserPermissions(User.UserID).SpecialPermissions, clsNodePermissions).RestrictedNodesBelow) 'C0903

                    Dim altsList As List(Of clsNode) = If(isAllowed, node.AllowedNodesBelow(User.UserID), node.RestrictedNodesBelow(User.UserID)) 'C0903

                    For Each alt As clsNode In altsList ' for now only
                        'mBW.Write(alt.NodeID) 'C0261
                        mBW.Write(alt.NodeGuidID.ToByteArray) 'C0261
                    Next

                    'C0262===
                    Dim NodeAltsPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodeAltsPermissionsChunkSizePosition
                    mBW.Seek(-NodeAltsPermissionsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeAltsPermissionsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim AltsPermissionsChunkSize As Integer = mBW.BaseStream.Position - AltsPermissionsChunkSizePosition
                mBW.Seek(-AltsPermissionsChunkSize, SeekOrigin.Current)
                mBW.Write(AltsPermissionsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserPermissions completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If
        mBW.Close()
    End Function

    Public Overrides Function WriteUserDisabledNodes(ByVal User As clsUser) As Boolean 'C0330
        mBW = New BinaryWriter(mStream)

        If mWriteStatus Then
            'Debug.Print("ProcessUserDisabledNodes started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        For Each H As clsHierarchy In ProjectManager.GetAllHierarchies
            mBW.Write(CHUNK_HIERARCHY_DISABLED_NODES)

            Dim NodesPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            mBW.Write(H.HierarchyGuidID.ToByteArray)

            Dim count As Integer = 0
            For Each node As clsNode In H.Nodes
                If node.DisabledForUser(User.UserID) Then
                    count += 1
                End If
            Next

            mBW.Write(count)

            For Each node As clsNode In H.Nodes
                If node.DisabledForUser(User.UserID) Then
                    mBW.Write(node.NodeGuidID.ToByteArray)
                End If
            Next

            Dim NodesPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodesPermissionsChunkSizePosition
            mBW.Seek(-NodesPermissionsChunkSize, SeekOrigin.Current)
            mBW.Write(NodesPermissionsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserDisabledNodes completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If
        mBW.Close()
    End Function

    Public Overrides Function WriteInfoDocs() As Boolean 'C0276
        mBW = New BinaryWriter(mStream)

        If mWriteStatus Then
            'Debug.Print("WriteInfoDocs started " + Now.ToString)
        End If

        mBW.Write(ProjectManager.GetAllHierarchies.Count)

        For Each H As clsHierarchy In ProjectManager.GetAllHierarchies
            mBW.Write(CHUNK_HIERARCHY_INFODOCS)

            Dim HierarchyInfoDocsChunkSizePosition As Integer = mBW.BaseStream.Position
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            mBW.Write(H.HierarchyGuidID.ToByteArray)

            Dim count As Integer = 0
            For Each node As clsNode In H.Nodes
                If node.InfoDoc <> "" Then
                    count += 1
                End If
            Next

            mBW.Write(count)

            For Each node As clsNode In H.Nodes
                If node.InfoDoc <> "" Then
                    mBW.Write(node.NodeGuidID.ToByteArray)
                    mBW.Write(node.InfoDoc)
                End If
            Next

            Dim HierarchyInfoDocsChunkSize As Integer = mBW.BaseStream.Position - HierarchyInfoDocsChunkSizePosition
            mBW.Seek(-HierarchyInfoDocsChunkSize, SeekOrigin.Current)
            mBW.Write(HierarchyInfoDocsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
        Next

        If mWriteStatus Then
            'Debug.Print("Write InfoDocs completed " + Now.ToString)
        End If
        mBW.Close()
    End Function

    Public Overrides Function WriteAdvancedInfoDocs() As Boolean 'C0920
        mBW = New BinaryWriter(mStream)

        If mWriteStatus Then
            'Debug.Print("WriteAdvancedInfoDocs started " + Now.ToString)
        End If

        mBW.Write(ProjectManager.InfoDocs.InfoDocs.Count)

        For Each iDoc As clsInfoDoc In ProjectManager.InfoDocs.InfoDocs
            mBW.Write(CHUNK_INFODOC)

            mBW.Write(iDoc.DocumentType)
            mBW.Write(iDoc.TargetID.ToByteArray)

            Select Case iDoc.DocumentType
                Case InfoDocType.idtNodeWRTParent, InfoDocType.idtUserWRTGroup
                    mBW.Write(iDoc.AdditionalID.ToByteArray)
            End Select

            mBW.Write(iDoc.InfoDoc)
        Next

        If mWriteStatus Then
            'Debug.Print("WriteAdvancedInfoDocs completed " + Now.ToString)
        End If
        mBW.Close()
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_8 'C0350
    Inherits clsStreamModelWriter_v_1_1_7

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt(ProjectManager.Hierarchies.Count * ProjectManager.AltsHierarchies.Count))

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                            If Not J.IsUndefined Then
                                count += 1
                            End If
                        Next
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType = ECMeasureType.mtPairwise Then
                            For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not pwData.IsUndefined Then 'C0272
                                    'C0275===
                                    If node.IsTerminalNode Then
                                        mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                        mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                    Else
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                    End If
                                    'C0275==

                                    mBW.Write(pwData.Advantage)
                                    mBW.Write(pwData.Value)
                                    mBW.Write(pwData.Comment)
                                    'C0369===
                                    If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                        pwData.ModifyDate = JudgmentsTime
                                    End If
                                    'C0369==
                                    mBW.Write(pwData.ModifyDate.ToBinary)
                                End If
                            Next
                        Else
                            For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not nonpwData.IsUndefined Then 'C0272
                                    'C0275===
                                    If node.IsTerminalNode Then
                                        mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                    Else
                                        mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                    End If
                                    'C0275==

                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtRatings
                                            'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                            'C0396===
                                            ' write a flag to determine whether we have a rating of direct value
                                            mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                            If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                            Else
                                                mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                            End If
                                            'C0396==
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                        Case ECMeasureType.mtStep
                                            mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                        Case ECMeasureType.mtDirect
                                            mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                    End Select
                                    mBW.Write(nonpwData.Comment)
                                    'C0369===
                                    If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                        nonpwData.ModifyDate = JudgmentsTime
                                    End If
                                    'C0369==
                                    mBW.Write(nonpwData.ModifyDate.ToBinary)
                                End If
                            Next
                        End If
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_9 'C0425
    Inherits clsStreamModelWriter_v_1_1_8

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean
        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.Nodes.Clear()
            hierarchy.ResetNodesDictionaries()
        Next

        mBW.Write(CHUNK_HIERARCHIES)

        Dim HierarchiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.GetAllHierarchies.Count)

        For Each H As clsHierarchy In ProjectManager.GetAllHierarchies
            mBW.Write(CHUNK_HIERARCHY)

            Dim HierarchyChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(H.HierarchyID)
            mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
            mBW.Write(CInt(H.HierarchyType))
            mBW.Write(H.HierarchyName)
            mBW.Write(H.Comment)

            mBW.Write(CHUNK_HIERARCHY_NODES)

            Dim HNodesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(H.Nodes.Count)
            For Each node As clsNode In H.Nodes
                mBW.Write(CHUNK_HIERARCHY_NODE)

                Dim NodeChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(node.NodeID)
                mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
                mBW.Write(node.NodeName)
                mBW.Write(node.ParentNodeID)
                mBW.Write(CInt(node.MeasureType))
                mBW.Write(node.MeasurementScaleID)
                mBW.Write(CInt(node.MeasureMode))
                mBW.Write(node.Enabled)
                If node.DefaultDataInstance IsNot Nothing Then
                    mBW.Write(node.DefaultDataInstance.ID)
                Else
                    mBW.Write(CInt(-1))
                End If
                mBW.Write(node.Comment)

                'C0425===
                If node.Tag IsNot Nothing And (H.HierarchyType = ECHierarchyType.htAlternative) Then
                    mBW.Write(CSng(node.Tag))
                Else
                    mBW.Write(Single.MinValue)
                End If
                'C0425==

                'C0343===
                Select Case H.HierarchyType
                    Case ECHierarchyType.htModel
                        If node.AHPNodeData Is Nothing Then
                            mBW.Write(CInt(0))
                        Else
                            mBW.Write(node.AHPNodeData.ToStream.ToArray.Length)
                            mBW.Write(node.AHPNodeData.ToStream.ToArray)
                        End If
                    Case ECHierarchyType.htAlternative
                        If node.AHPAltData Is Nothing Then
                            mBW.Write(CInt(0))
                        Else
                            mBW.Write(node.AHPAltData.ToStream.ToArray.Length)
                            mBW.Write(node.AHPAltData.ToStream.ToArray)
                        End If
                    Case Else
                        mBW.Write(CInt(0))
                End Select
                'C0343==


                'C0262===
                Dim NodeChunkSize As Integer = mBW.BaseStream.Position - NodeChunkSizePosition
                mBW.Seek(-NodeChunkSize, SeekOrigin.Current)
                mBW.Write(NodeChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim HNodesChunkSize As Integer = mBW.BaseStream.Position - HNodesChunkSizePosition
            mBW.Seek(-HNodesChunkSize, SeekOrigin.Current)
            mBW.Write(HNodesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==

            'C0262===
            Dim HierarchyChunkSize As Integer = mBW.BaseStream.Position - HierarchyChunkSizePosition
            mBW.Seek(-HierarchyChunkSize, SeekOrigin.Current)
            mBW.Write(HierarchyChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim HierarchiesChunkSize As Integer = mBW.BaseStream.Position - HierarchiesChunkSizePosition
        mBW.Seek(-HierarchiesChunkSize, SeekOrigin.Current)
        mBW.Write(HierarchiesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessHierarchies completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_11 'C0625
    Inherits clsStreamModelWriter_v_1_1_9

    Protected Overrides Function ProcessUsersListChunk() As Boolean
        mBW.Write(CHUNK_USERS_LIST)

        Dim UsersListChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(CInt(ProjectManager.UsersList.Count + ProjectManager.DataInstanceUsers.Count))

        Dim users As New ArrayList
        For Each user As clsUser In ProjectManager.UsersList
            'If user.UserID <> COMBINED_USER_ID Then 'C0555
            If Not IsCombinedUserID(user.UserID) Then 'C0555
                users.Add(user)
            End If
        Next
        For Each user As clsUser In ProjectManager.DataInstanceUsers
            users.Add(user)
        Next

        Dim UserChunkSizePosition As Integer 'C0262
        Dim UserChunkSize As Integer 'C0262

        For Each user As clsUser In users
            mBW.Write(CHUNK_USER)

            UserChunkSizePosition = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(user.UserID)
            mBW.Write(user.UserGuidID.ToByteArray) 'C0261
            mBW.Write(user.UserName)
            mBW.Write(user.UserEMail)
            mBW.Write(user.Active)
            mBW.Write(user.VotingBoxID)
            mBW.Write(user.IncludedInSynchronous)
            mBW.Write(user.DataInstanceID)
            mBW.Write(user.SyncEvaluationMode) 'C0625
            If user.EvaluationGroup IsNot Nothing Then
                mBW.Write(user.EvaluationGroup.ID)
            Else
                mBW.Write(CInt(-1))
            End If

            'C0343===
            If user.AHPUserData Is Nothing Then
                mBW.Write(CInt(0))
            Else
                mBW.Write(user.AHPUserData.ToStream.ToArray.Length)
                mBW.Write(user.AHPUserData.ToStream.ToArray)
            End If
            'C0343==

            'C0262===
            UserChunkSize = mBW.BaseStream.Position - UserChunkSizePosition
            mBW.Seek(-UserChunkSize, SeekOrigin.Current)
            mBW.Write(UserChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim UsersListChunkSize As Integer = mBW.BaseStream.Position - UsersListChunkSizePosition
        mBW.Seek(-UsersListChunkSize, SeekOrigin.Current)
        mBW.Write(UsersListChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessUsersList completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean
        mBW.Write(CHUNK_HIERARCHIES)

        Dim HierarchiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.GetAllHierarchies.Count)

        For Each H As clsHierarchy In ProjectManager.GetAllHierarchies
            mBW.Write(CHUNK_HIERARCHY)

            Dim HierarchyChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(H.HierarchyID)
            mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
            mBW.Write(CInt(H.HierarchyType))
            mBW.Write(H.HierarchyName)
            mBW.Write(H.Comment)

            mBW.Write(CHUNK_HIERARCHY_NODES)

            Dim HNodesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(H.Nodes.Count)

            'C0642===

            'C0731===
            'For Each rootNode As clsNode In H.RootNodes
            '    ProcessNodeChunk(rootNode)
            'Next
            'C0731==

            For Each node As clsNode In H.Nodes
                node.SavedToStream = False
            Next

            'C0731===
            Select Case H.HierarchyType
                Case ECHierarchyType.htModel, ECHierarchyType.htMeasure
                    If H.Nodes.Count > 0 Then 'C0739
                        If H.RootNodes.Count > 0 Then 'C0739
                            ProcessNodeChunk(H.RootNodes(0))
                        End If
                    End If
                Case ECHierarchyType.htAlternative
                    For Each rootNode As clsNode In H.RootNodes
                        ProcessNodeChunk(rootNode)
                    Next
            End Select
            'C0731==

            'C0642==

            'C0642===
            'For Each node As clsNode In H.Nodes
            '    mBW.Write(CHUNK_HIERARCHY_NODE)

            '    Dim NodeChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            '    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            '    mBW.Write(node.NodeID)
            '    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
            '    mBW.Write(node.NodeName)
            '    mBW.Write(node.ParentNodeID)
            '    mBW.Write(CInt(node.MeasureType))
            '    mBW.Write(node.MeasurementScaleID)
            '    mBW.Write(CInt(node.MeasureMode))
            '    mBW.Write(node.Enabled)
            '    If node.DefaultDataInstance IsNot Nothing Then
            '        mBW.Write(node.DefaultDataInstance.ID)
            '    Else
            '        mBW.Write(CInt(-1))
            '    End If
            '    mBW.Write(node.Comment)

            '    'C0425===
            '    If node.Tag IsNot Nothing And (H.HierarchyType = ECHierarchyType.htAlternative) Then
            '        'mBW.Write(CSng(node.Tag)) 'C0626
            '        mBW.Write(node.Tag.ToString) 'C0626
            '    Else
            '        'mBW.Write(Single.MinValue) 'C0626
            '        mBW.Write(UNDEFINED_COST_VALUE) 'C0626
            '    End If
            '    'C0425==

            '    'C0343===
            '    Select Case H.HierarchyType
            '        Case ECHierarchyType.htModel
            '            If node.AHPNodeData Is Nothing Then
            '                mBW.Write(CInt(0))
            '            Else
            '                mBW.Write(node.AHPNodeData.ToStream.ToArray.Length)
            '                mBW.Write(node.AHPNodeData.ToStream.ToArray)
            '            End If
            '        Case ECHierarchyType.htAlternative
            '            If node.AHPAltData Is Nothing Then
            '                mBW.Write(CInt(0))
            '            Else
            '                mBW.Write(node.AHPAltData.ToStream.ToArray.Length)
            '                mBW.Write(node.AHPAltData.ToStream.ToArray)
            '            End If
            '        Case Else
            '            mBW.Write(CInt(0))
            '    End Select
            '    'C0343==
            'C0642==

            '    'C0262===
            '    Dim NodeChunkSize As Integer = mBW.BaseStream.Position - NodeChunkSizePosition
            '    mBW.Seek(-NodeChunkSize, SeekOrigin.Current)
            '    mBW.Write(NodeChunkSize)
            '    mBW.Seek(0, SeekOrigin.End)
            '    'C0262==
            'Next

            'C0262===
            Dim HNodesChunkSize As Integer = mBW.BaseStream.Position - HNodesChunkSizePosition
            mBW.Seek(-HNodesChunkSize, SeekOrigin.Current)
            mBW.Write(HNodesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==

            'C0262===
            Dim HierarchyChunkSize As Integer = mBW.BaseStream.Position - HierarchyChunkSizePosition
            mBW.Seek(-HierarchyChunkSize, SeekOrigin.Current)
            mBW.Write(HierarchyChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim HierarchiesChunkSize As Integer = mBW.BaseStream.Position - HierarchiesChunkSizePosition
        mBW.Seek(-HierarchiesChunkSize, SeekOrigin.Current)
        mBW.Write(HierarchiesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessHierarchies completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessNodeChunk(ByVal node As clsNode) As Boolean 'C0642
        mBW.Write(CHUNK_HIERARCHY_NODE)

        Dim NodeChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(node.NodeID)
        mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
        mBW.Write(node.NodeName)
        mBW.Write(node.ParentNodeID)
        mBW.Write(CInt(node.MeasureType))
        mBW.Write(node.MeasurementScaleID)
        mBW.Write(CInt(node.MeasureMode))
        mBW.Write(node.Enabled)
        If node.DefaultDataInstance IsNot Nothing Then
            mBW.Write(node.DefaultDataInstance.ID)
        Else
            mBW.Write(CInt(-1))
        End If
        mBW.Write(node.Comment)

        'C0425===
        If node.Tag IsNot Nothing And (node.Hierarchy.HierarchyType = ECHierarchyType.htAlternative) Then
            'mBW.Write(CSng(node.Tag)) 'C0626
            mBW.Write(node.Tag.ToString) 'C0626
        Else
            'mBW.Write(Single.MinValue) 'C0626
            mBW.Write(UNDEFINED_COST_VALUE) 'C0626
        End If
        'C0425==

        'C0343===
        Select Case node.Hierarchy.HierarchyType
            Case ECHierarchyType.htModel
                If node.AHPNodeData Is Nothing Then
                    mBW.Write(CInt(0))
                Else
                    mBW.Write(node.AHPNodeData.ToStream.ToArray.Length)
                    mBW.Write(node.AHPNodeData.ToStream.ToArray)
                End If
            Case ECHierarchyType.htAlternative
                If node.AHPAltData Is Nothing Then
                    mBW.Write(CInt(0))
                Else
                    mBW.Write(node.AHPAltData.ToStream.ToArray.Length)
                    mBW.Write(node.AHPAltData.ToStream.ToArray)
                End If
            Case Else
                mBW.Write(CInt(0))
        End Select
        'C0343==

        'C0262===
        Dim NodeChunkSize As Integer = mBW.BaseStream.Position - NodeChunkSizePosition
        mBW.Seek(-NodeChunkSize, SeekOrigin.Current)
        mBW.Write(NodeChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        For Each child As clsNode In node.Children
            ProcessNodeChunk(child)
        Next
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_12 'C0646
    Inherits clsStreamModelWriter_v_1_1_11

    Protected Overrides Function ProcessDataInstancesChunk() As Boolean
        mBW.Write(CHUNK_DATA_INSTANCES)

        Dim DataInstancesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.DataInstances.Count)

        Dim DIChunkSizePosition As Integer 'C0262
        Dim DIChunkSize As Integer 'C0262

        For Each DI As clsDataInstance In ProjectManager.DataInstances
            mBW.Write(CHUNK_DATA_INSTANCE)

            DIChunkSizePosition = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(DI.ID)
            'mBW.Write(DI.GuidID.ToByteArray) 'C0261
            mBW.Write(DI.Name)
            mBW.Write(DI.User.UserID)

            'C0646===
            If DI.EvaluatorUser IsNot Nothing Then
                mBW.Write(DI.EvaluatorUser.UserID)
            Else
                mBW.Write(UNDEFINED_USER_ID)
            End If
            'C0646==

            'mBW.Write(DI.User.UserGuidID.ToByteArray) 'C0261
            mBW.Write(DI.Comment)

            'C0262===
            DIChunkSize = mBW.BaseStream.Position - DIChunkSizePosition
            mBW.Seek(-DIChunkSize, SeekOrigin.Current)
            mBW.Write(DIChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim DataInstancesChunkSize As Integer = mBW.BaseStream.Position - DataInstancesChunkSizePosition
        mBW.Seek(-DataInstancesChunkSize, SeekOrigin.Current)
        mBW.Write(DataInstancesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessDataInstances completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_13 'C0745
    Inherits clsStreamModelWriter_v_1_1_12

    Protected Overrides Function ProcessUsersListChunk() As Boolean
        mBW.Write(CHUNK_USERS_LIST)

        Dim UsersListChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(CInt(ProjectManager.UsersList.Count + ProjectManager.DataInstanceUsers.Count))

        Dim users As New ArrayList
        For Each user As clsUser In ProjectManager.UsersList
            If Not IsCombinedUserID(user.UserID) Then 'C0555
                users.Add(user)
            End If
        Next
        For Each user As clsUser In ProjectManager.DataInstanceUsers
            users.Add(user)
        Next

        Dim UserChunkSizePosition As Integer 'C0262
        Dim UserChunkSize As Integer 'C0262

        For Each user As clsUser In users
            mBW.Write(CHUNK_USER)

            UserChunkSizePosition = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(user.UserID)
            mBW.Write(user.UserGuidID.ToByteArray) 'C0261
            mBW.Write(user.UserName)
            mBW.Write(user.UserEMail)
            mBW.Write(user.Active)
            mBW.Write(user.VotingBoxID)
            mBW.Write(user.IncludedInSynchronous)
            mBW.Write(user.DataInstanceID)
            mBW.Write(user.SyncEvaluationMode) 'C0625
            mBW.Write(user.Weight) 'C0745
            If user.EvaluationGroup IsNot Nothing Then
                mBW.Write(user.EvaluationGroup.ID)
            Else
                mBW.Write(CInt(-1))
            End If

            'C0343===
            If user.AHPUserData Is Nothing Then
                mBW.Write(CInt(0))
            Else
                mBW.Write(user.AHPUserData.ToStream.ToArray.Length)
                mBW.Write(user.AHPUserData.ToStream.ToArray)
            End If
            'C0343==

            'C0262===
            UserChunkSize = mBW.BaseStream.Position - UserChunkSizePosition
            mBW.Seek(-UserChunkSize, SeekOrigin.Current)
            mBW.Write(UserChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        Next

        'C0262===
        Dim UsersListChunkSize As Integer = mBW.BaseStream.Position - UsersListChunkSizePosition
        mBW.Seek(-UsersListChunkSize, SeekOrigin.Current)
        mBW.Write(UsersListChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessUsersList completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_14 'C0900
    Inherits clsStreamModelWriter_v_1_1_13

    Public Overrides Function WriteUserPermissions(ByVal User As clsUser) As Boolean
        mBW = New BinaryWriter(mStream)

        If mWriteStatus Then
            'Debug.Print("ProcessUserPermissions started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim isAllowed As Boolean = False

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            mBW.Write(CHUNK_NODES_PERMISSIONS)

            Dim NodesPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            mBW.Write(isAllowed) ' for now we will store restricted nodes
            mBW.Write(H.HierarchyGuidID.ToByteArray)

            Dim restrictedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetRestrictedObjectives(User.UserID)
            mBW.Write(restrictedObjectives.Count)

            For Each ObjID As Guid In restrictedObjectives
                mBW.Write(ObjID.ToByteArray)
            Next

            Dim NodesPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodesPermissionsChunkSizePosition
            mBW.Seek(-NodesPermissionsChunkSize, SeekOrigin.Current)
            mBW.Write(NodesPermissionsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
        Next


        Dim CurHierarchy As Integer = ProjectManager.ActiveHierarchy
        Dim CurAltsHierarchy As Integer = ProjectManager.ActiveAltsHierarchy

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            ProjectManager.ActiveHierarchy = H.HierarchyID
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                ProjectManager.ActiveAltsHierarchy = AH.HierarchyID
                mBW.Write(CHUNK_ALTERNATIVES_PERMISSIONS)

                Dim AltsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                mBW.Write(H.HierarchyGuidID.ToByteArray)
                mBW.Write(AH.HierarchyGuidID.ToByteArray)
                mBW.Write(H.TerminalNodes.Count)

                For Each node As clsNode In H.TerminalNodes
                    mBW.Write(CHUNK_NODE_ALTERNATIVES_PERMISSIONS)

                    Dim NodeAltsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(node.NodeGuidID.ToByteArray)
                    mBW.Write(isAllowed)

                    Dim restrictedAlts As HashSet(Of Guid) = ProjectManager.UsersRoles.GetRestrictedAlternatives(User.UserID, node.NodeGuidID)

                    mBW.Write(restrictedAlts.Count)

                    For Each AltID As Guid In restrictedAlts
                        mBW.Write(AltID.ToByteArray)
                    Next

                    'C0262===
                    Dim NodeAltsPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodeAltsPermissionsChunkSizePosition
                    mBW.Seek(-NodeAltsPermissionsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeAltsPermissionsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim AltsPermissionsChunkSize As Integer = mBW.BaseStream.Position - AltsPermissionsChunkSizePosition
                mBW.Seek(-AltsPermissionsChunkSize, SeekOrigin.Current)
                mBW.Write(AltsPermissionsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        ProjectManager.ActiveHierarchy = CurHierarchy
        ProjectManager.ActiveAltsHierarchy = CurAltsHierarchy

        If mWriteStatus Then
            'Debug.Print("ProcessUserPermissions completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If
        mBW.Close()
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_15 'C1030
    Inherits clsStreamModelWriter_v_1_1_14

    Public Overrides Function WritePermissions(ByVal UserID As Integer) As Boolean
        mBW = New BinaryWriter(mStream)

        If mWriteStatus Then
            'Debug.Print("ProcessUserPermissions started " + Now.ToString + " User: " + UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim isAllowed As Boolean = False

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            mBW.Write(CHUNK_NODES_PERMISSIONS)

            Dim NodesPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            'mBW.Write(isAllowed) ' for now we will store restricted nodes
            mBW.Write(H.HierarchyGuidID.ToByteArray)

            Dim restrictedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetRestrictedObjectives(UserID)
            mBW.Write(restrictedObjectives.Count)

            For Each ObjID As Guid In restrictedObjectives
                mBW.Write(ObjID.ToByteArray)
            Next

            Dim allowedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetAllowedObjectives(UserID)
            mBW.Write(allowedObjectives.Count)

            For Each ObjID As Guid In allowedObjectives
                mBW.Write(ObjID.ToByteArray)
            Next

            Dim NodesPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodesPermissionsChunkSizePosition
            mBW.Seek(-NodesPermissionsChunkSize, SeekOrigin.Current)
            mBW.Write(NodesPermissionsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
        Next

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_ALTERNATIVES_PERMISSIONS)

                Dim AltsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                mBW.Write(H.HierarchyGuidID.ToByteArray)
                mBW.Write(AH.HierarchyGuidID.ToByteArray)
                mBW.Write(H.TerminalNodes.Count)

                For Each node As clsNode In H.TerminalNodes
                    mBW.Write(CHUNK_NODE_ALTERNATIVES_PERMISSIONS)

                    Dim NodeAltsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(node.NodeGuidID.ToByteArray)
                    'mBW.Write(isAllowed)

                    ' write restricted alternatives
                    Dim restrictedAlts As HashSet(Of Guid) = ProjectManager.UsersRoles.GetRestrictedAlternatives(UserID, node.NodeGuidID)

                    mBW.Write(restrictedAlts.Count)

                    For Each AltID As Guid In restrictedAlts
                        mBW.Write(AltID.ToByteArray)
                    Next

                    ' write allowed alternatives
                    Dim allowedAlts As HashSet(Of Guid) = ProjectManager.UsersRoles.GetAllowedAlternatives(UserID, node.NodeGuidID)

                    mBW.Write(allowedAlts.Count)

                    For Each AltID As Guid In allowedAlts
                        mBW.Write(AltID.ToByteArray)
                    Next

                    'C0262===
                    Dim NodeAltsPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodeAltsPermissionsChunkSizePosition
                    mBW.Seek(-NodeAltsPermissionsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeAltsPermissionsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim AltsPermissionsChunkSize As Integer = mBW.BaseStream.Position - AltsPermissionsChunkSizePosition
                mBW.Seek(-AltsPermissionsChunkSize, SeekOrigin.Current)
                mBW.Write(AltsPermissionsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserPermissions completed " + Now.ToString + " User: " + UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If
        mBW.Close()
    End Function

    Public Overrides Function WriteUserPermissions(ByVal User As clsUser) As Boolean
        Return WritePermissions(User.UserID)
    End Function

    Public Overrides Function WriteCombinedGroupPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
        Return WritePermissions(CombinedGroup.CombinedUserID)
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_16
    Inherits clsStreamModelWriter_v_1_1_15

    Public Overrides Function WritePermissions(ByVal UserID As Integer) As Boolean
        mBW = New BinaryWriter(mStream)

        If mWriteStatus Then
            'Debug.Print("ProcessUserPermissions started " + Now.ToString + " User: " + UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim isAllowed As Boolean = False

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            mBW.Write(CHUNK_NODES_PERMISSIONS)

            Dim NodesPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            'mBW.Write(isAllowed) ' for now we will store restricted nodes
            mBW.Write(H.HierarchyGuidID.ToByteArray)

            Dim restrictedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetRestrictedObjectives(UserID)
            mBW.Write(restrictedObjectives.Count)

            For Each ObjID As Guid In restrictedObjectives
                mBW.Write(ObjID.ToByteArray)
            Next

            Dim allowedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetAllowedObjectives(UserID)
            mBW.Write(allowedObjectives.Count)

            For Each ObjID As Guid In allowedObjectives
                mBW.Write(ObjID.ToByteArray)
            Next

            If UserID <> COMBINED_USER_ID Then
                mBW.Write(CInt(0))
            Else
                Dim undefinedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetUndefinedObjectivesForDefaultGroup()
                mBW.Write(undefinedObjectives.Count)
                For Each ObjID As Guid In undefinedObjectives
                    mBW.Write(ObjID.ToByteArray)
                Next
            End If

            Dim NodesPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodesPermissionsChunkSizePosition
            mBW.Seek(-NodesPermissionsChunkSize, SeekOrigin.Current)
            mBW.Write(NodesPermissionsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
        Next

        Dim CurHierarchy As Integer = ProjectManager.ActiveHierarchy
        Dim CurAltsHierarchy As Integer = ProjectManager.ActiveAltsHierarchy

        For Each H As clsHierarchy In ProjectManager.Hierarchies
            ProjectManager.ActiveHierarchy = H.HierarchyID
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                ProjectManager.ActiveAltsHierarchy = AH.HierarchyID

                mBW.Write(CHUNK_ALTERNATIVES_PERMISSIONS)

                Dim AltsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                mBW.Write(H.HierarchyGuidID.ToByteArray)
                mBW.Write(AH.HierarchyGuidID.ToByteArray)

                Dim nodes As New List(Of clsNode)
                For Each node As clsNode In H.TerminalNodes
                    nodes.Add(node)
                Next
                If H.GetUncontributedAlternatives.Count > 0 Then
                    nodes.Add(H.Nodes(0))
                End If

                mBW.Write(nodes.Count)

                For Each node As clsNode In nodes
                    mBW.Write(CHUNK_NODE_ALTERNATIVES_PERMISSIONS)

                    Dim NodeAltsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(node.NodeGuidID.ToByteArray)
                    'mBW.Write(isAllowed)

                    ' write restricted alternatives
                    Dim restrictedAlts As HashSet(Of Guid) = ProjectManager.UsersRoles.GetRestrictedAlternatives(UserID, node.NodeGuidID)

                    mBW.Write(restrictedAlts.Count)

                    For Each AltID As Guid In restrictedAlts
                        mBW.Write(AltID.ToByteArray)
                    Next

                    ' write allowed alternatives
                    Dim allowedAlts As HashSet(Of Guid) = ProjectManager.UsersRoles.GetAllowedAlternatives(UserID, node.NodeGuidID)

                    mBW.Write(allowedAlts.Count)

                    For Each AltID As Guid In allowedAlts
                        mBW.Write(AltID.ToByteArray)
                    Next

                    If UserID <> COMBINED_USER_ID Then
                        mBW.Write(CInt(0))
                    Else
                        Dim undefinedAlts As HashSet(Of Guid) = ProjectManager.UsersRoles.GetUndefinedAlternativesForDefaultGroup(node.NodeGuidID)
                        mBW.Write(undefinedAlts.Count)
                        For Each AltID As Guid In undefinedAlts
                            mBW.Write(AltID.ToByteArray)
                        Next
                    End If

                    'C0262===
                    Dim NodeAltsPermissionsChunkSize As Integer = mBW.BaseStream.Position - NodeAltsPermissionsChunkSizePosition
                    mBW.Seek(-NodeAltsPermissionsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeAltsPermissionsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim AltsPermissionsChunkSize As Integer = mBW.BaseStream.Position - AltsPermissionsChunkSizePosition
                mBW.Seek(-AltsPermissionsChunkSize, SeekOrigin.Current)
                mBW.Write(AltsPermissionsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next


        ProjectManager.ActiveHierarchy = CurHierarchy
        ProjectManager.ActiveAltsHierarchy = CurAltsHierarchy

        If mWriteStatus Then
            'Debug.Print("ProcessUserPermissions completed " + Now.ToString + " User: " + UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If
        mBW.Close()
    End Function

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt((ProjectManager.Hierarchies.Count + ProjectManager.MeasureHierarchies.Count) * ProjectManager.AltsHierarchies.Count))

        Dim Hierarchies As New List(Of clsHierarchy)
        For Each H As clsHierarchy In ProjectManager.Hierarchies
            Hierarchies.Add(H)
        Next
        For Each H As clsHierarchy In ProjectManager.MeasureHierarchies
            Hierarchies.Add(H)
        Next


        For Each H As clsHierarchy In Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                            If Not J.IsUndefined Then
                                count += 1
                            End If
                        Next
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType = ECMeasureType.mtPairwise Then
                            For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not pwData.IsUndefined Then 'C0272
                                    'C0275===
                                    If node.IsTerminalNode Then
                                        mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                        mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                    Else
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray) 'C0261 'C0275
                                    End If
                                    'C0275==

                                    mBW.Write(pwData.Advantage)
                                    mBW.Write(pwData.Value)
                                    mBW.Write(pwData.Comment)
                                    'C0369===
                                    If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                        pwData.ModifyDate = JudgmentsTime
                                    End If
                                    'C0369==
                                    mBW.Write(pwData.ModifyDate.ToBinary)
                                End If
                            Next
                        Else
                            For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not nonpwData.IsUndefined Then 'C0272
                                    'C0275===
                                    If node.IsTerminalNode Then
                                        mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                    Else
                                        mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                    End If
                                    'C0275==

                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtRatings
                                            'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                            'C0396===
                                            ' write a flag to determine whether we have a rating of direct value
                                            mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                            If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                            Else
                                                mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                            End If
                                            'C0396==
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                        Case ECMeasureType.mtStep
                                            mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                        Case ECMeasureType.mtDirect
                                            mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                    End Select
                                    mBW.Write(nonpwData.Comment)
                                    'C0369===
                                    If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                        nonpwData.ModifyDate = JudgmentsTime
                                    End If
                                    'C0369==
                                    mBW.Write(nonpwData.ModifyDate.ToBinary)
                                End If
                            Next
                        End If
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_18
    Inherits clsStreamModelWriter_v_1_1_16

    Protected Overrides Function ProcessMeasurementScalesChunk() As Boolean
        mBW.Write(CHUNK_MEASUREMENT_SCALES)

        Dim MeasurementScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.MeasureScales.AllScales.Count)

        If ProjectManager.MeasureScales.RatingsScales.Count <> 0 Then
            mBW.Write(CHUNK_RATINGS_SCALES)

            Dim RatingScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RatingsScales.Count)
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                mBW.Write(CHUNK_RATING_SCALE)

                Dim RSChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.ID)
                mBW.Write(RS.GuidID.ToByteArray) 'C0283
                mBW.Write(RS.Name)
                mBW.Write(RS.Comment)
                mBW.Write(RS.IsOutcomes)

                mBW.Write(CHUNK_RATING_INTENSITIES)

                Dim RIntensitiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.RatingSet.Count)

                For Each R As clsRating In RS.RatingSet
                    If R.Value < 0 Then
                        R.Value = 0
                    End If

                    If R.Value > 1 Then
                        R.Value = 1
                    End If

                    mBW.Write(CHUNK_RATING_INTENSITY)

                    Dim RIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(R.ID)
                    mBW.Write(R.GuidID.ToByteArray) 'C0261
                    mBW.Write(R.Name)
                    mBW.Write(R.Value)
                    mBW.Write(R.Comment)

                    'C0262===
                    Dim RIChunkSize As Integer = mBW.BaseStream.Position - RIChunkSizePosition
                    mBW.Seek(-RIChunkSize, SeekOrigin.Current)
                    mBW.Write(RIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim RIntensitiesChunkSize As Integer = mBW.BaseStream.Position - RIntensitiesChunkSizePosition
                mBW.Seek(-RIntensitiesChunkSize, SeekOrigin.Current)
                mBW.Write(RIntensitiesChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim RSChunkSize As Integer = mBW.BaseStream.Position - RSChunkSizePosition
                mBW.Seek(-RSChunkSize, SeekOrigin.Current)
                mBW.Write(RSChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

            Next

            'C0262===
            Dim RatingScalesChunkSize As Integer = mBW.BaseStream.Position - RatingScalesChunkSizePosition
            mBW.Seek(-RatingScalesChunkSize, SeekOrigin.Current)
            mBW.Write(RatingScalesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.RegularUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_REGULAR_UTILITY_CURVES)

            Dim RUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RegularUtilityCurves.Count)
            For Each RUC As clsRegularUtilityCurve In ProjectManager.MeasureScales.RegularUtilityCurves
                mBW.Write(CHUNK_REGULAR_UTILITY_CURVE)

                Dim RUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RUC.ID)
                mBW.Write(RUC.GuidID.ToByteArray) 'C0283
                mBW.Write(RUC.Name)
                mBW.Write(RUC.Low)
                mBW.Write(RUC.High)
                mBW.Write(RUC.Curvature)
                mBW.Write(RUC.IsIncreasing)
                mBW.Write(RUC.Comment)

                'C0262===
                Dim RUCChunkSize As Integer = mBW.BaseStream.Position - RUCChunkSizePosition
                mBW.Seek(-RUCChunkSize, SeekOrigin.Current)
                mBW.Write(RUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim RUCsChunkSize As Integer = mBW.BaseStream.Position - RUCsChunkSizePosition
            mBW.Seek(-RUCsChunkSize, SeekOrigin.Current)
            mBW.Write(RUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.AdvancedUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_ADVANCED_UTILITY_CURVES)

            Dim AUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.AdvancedUtilityCurves.Count)
            For Each AUC As clsAdvancedUtilityCurve In ProjectManager.MeasureScales.AdvancedUtilityCurves
                mBW.Write(CHUNK_ADVANCED_UTILITY_CURVE)

                Dim AUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.ID)
                mBW.Write(AUC.GuidID.ToByteArray) 'C0283
                mBW.Write(AUC.Name)
                mBW.Write(CInt(AUC.InterpolationMethod))
                mBW.Write(AUC.Comment)

                mBW.Write(CHUNK_AUC_POINTS)

                Dim AUCPointsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.Points.Count)

                For Each point As clsUCPoint In AUC.Points
                    mBW.Write(point.X)
                    mBW.Write(point.Y)
                Next

                'C0262===
                Dim AUCPointsChunkSize As Integer = mBW.BaseStream.Position - AUCPointsChunkSizePosition
                mBW.Seek(-AUCPointsChunkSize, SeekOrigin.Current)
                mBW.Write(AUCPointsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim AUCChunkSize As Integer = mBW.BaseStream.Position - AUCChunkSizePosition
                mBW.Seek(-AUCChunkSize, SeekOrigin.Current)
                mBW.Write(AUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim AUCsChunkSize As Integer = mBW.BaseStream.Position - AUCsChunkSizePosition
            mBW.Seek(-AUCsChunkSize, SeekOrigin.Current)
            mBW.Write(AUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.StepFunctions.Count <> 0 Then
            mBW.Write(CHUNK_STEP_FUNCTIONS)

            Dim SFsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.StepFunctions.Count)
            For Each SF As clsStepFunction In ProjectManager.MeasureScales.StepFunctions
                mBW.Write(CHUNK_STEP_FUNCTION)

                Dim SFChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.ID)
                mBW.Write(SF.GuidID.ToByteArray) 'C0283
                mBW.Write(SF.Name)
                mBW.Write(SF.IsPiecewiseLinear) 'C0329
                mBW.Write(SF.Comment)

                mBW.Write(CHUNK_STEP_INTERVALS)

                Dim SIsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.Intervals.Count)
                For Each interval As clsStepInterval In SF.Intervals
                    mBW.Write(CHUNK_STEP_INTERVAL)

                    Dim SIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(interval.ID)
                    'mBW.Write(interval.GuidID.ToByteArray)
                    mBW.Write(interval.Name)
                    mBW.Write(interval.Low)
                    mBW.Write(interval.High)
                    mBW.Write(interval.Value)
                    mBW.Write(interval.Comment)

                    'C0262===
                    Dim SIChunkSize As Integer = mBW.BaseStream.Position - SIChunkSizePosition
                    mBW.Seek(-SIChunkSize, SeekOrigin.Current)
                    mBW.Write(SIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim SIsChunkSize As Integer = mBW.BaseStream.Position - SIsChunkSizePosition
                mBW.Seek(-SIsChunkSize, SeekOrigin.Current)
                mBW.Write(SIsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim SFChunkSize As Integer = mBW.BaseStream.Position - SFChunkSizePosition
                mBW.Seek(-SFChunkSize, SeekOrigin.Current)
                mBW.Write(SFChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim SFsChunkSize As Integer = mBW.BaseStream.Position - SFsChunkSizePosition
            mBW.Seek(-SFsChunkSize, SeekOrigin.Current)
            mBW.Write(SFsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        'C0262===
        Dim MeasurementScalesChunkSize As Integer = mBW.BaseStream.Position - MeasurementScalesChunkSizePosition
        mBW.Seek(-MeasurementScalesChunkSize, SeekOrigin.Current)
        mBW.Write(MeasurementScalesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessMeasurementScales completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt((ProjectManager.Hierarchies.Count + ProjectManager.MeasureHierarchies.Count) * ProjectManager.AltsHierarchies.Count))

        Dim Hierarchies As New List(Of clsHierarchy)
        For Each H As clsHierarchy In ProjectManager.Hierarchies
            Hierarchies.Add(H)
        Next
        For Each H As clsHierarchy In ProjectManager.MeasureHierarchies
            Hierarchies.Add(H)
        Next

        For Each H As clsHierarchy In Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                            For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not J.IsUndefined Then
                                    count += 1
                                End If
                            Next
                        Else
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                For Each J As clsCustomMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                                    If Not J.IsUndefined Then
                                        count += 1
                                    End If
                                Next
                            Next
                        End If
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not pwData.IsUndefined Then
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                        If node.MeasureType = ECMeasureType.mtPairwise Then
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            End If
                                        Else
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)
                                        End If

                                        mBW.Write(pwData.Advantage)
                                        mBW.Write(pwData.Value)
                                        mBW.Write(pwData.Comment)

                                        If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                            pwData.ModifyDate = JudgmentsTime
                                        End If

                                        mBW.Write(pwData.ModifyDate.ToBinary)
                                    End If
                                Next
                            Case ECMeasureType.mtPWOutcomes
                                For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                    For Each pwData As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                                        If Not pwData.IsUndefined Then
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(H.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            End If
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                            mBW.Write(pwData.Advantage)
                                            mBW.Write(pwData.Value)
                                            mBW.Write(pwData.Comment)

                                            If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                                pwData.ModifyDate = JudgmentsTime
                                            End If

                                            mBW.Write(pwData.ModifyDate.ToBinary)
                                        End If
                                    Next
                                Next
                            Case Else
                                For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not nonpwData.IsUndefined Then 'C0272
                                        If node.IsTerminalNode Then
                                            mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        Else
                                            mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        End If

                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                                'C0396===
                                                ' write a flag to determine whether we have a rating of direct value
                                                mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                                If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                    mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                                Else
                                                    mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                                End If
                                                'C0396==
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                            Case ECMeasureType.mtStep
                                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                            Case ECMeasureType.mtDirect
                                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                        End Select
                                        mBW.Write(nonpwData.Comment)
                                        'C0369===
                                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                            nonpwData.ModifyDate = JudgmentsTime
                                        End If
                                        'C0369==
                                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                                    End If
                                Next
                        End Select
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next


                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_19
    Inherits clsStreamModelWriter_v_1_1_18

    Protected Overrides Function ProcessMeasurementScalesChunk() As Boolean
        mBW.Write(CHUNK_MEASUREMENT_SCALES)

        Dim MeasurementScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.MeasureScales.AllScales.Count)

        If ProjectManager.MeasureScales.RatingsScales.Count <> 0 Then
            mBW.Write(CHUNK_RATINGS_SCALES)

            Dim RatingScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RatingsScales.Count)
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                mBW.Write(CHUNK_RATING_SCALE)

                Dim RSChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.ID)
                mBW.Write(RS.GuidID.ToByteArray) 'C0283
                mBW.Write(RS.Name)
                mBW.Write(RS.Comment)
                mBW.Write(RS.IsOutcomes)

                mBW.Write(CHUNK_RATING_INTENSITIES)

                Dim RIntensitiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.RatingSet.Count)

                For Each R As clsRating In RS.RatingSet
                    If R.Value < 0 Then
                        R.Value = 0
                    End If

                    If R.Value > 1 Then
                        R.Value = 1
                    End If

                    mBW.Write(CHUNK_RATING_INTENSITY)

                    Dim RIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(R.ID)
                    mBW.Write(R.GuidID.ToByteArray) 'C0261
                    mBW.Write(R.Name)
                    mBW.Write(R.Value)
                    mBW.Write(R.Comment)
                    mBW.Write(R.AvailableForPW)

                    'C0262===
                    Dim RIChunkSize As Integer = mBW.BaseStream.Position - RIChunkSizePosition
                    mBW.Seek(-RIChunkSize, SeekOrigin.Current)
                    mBW.Write(RIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim RIntensitiesChunkSize As Integer = mBW.BaseStream.Position - RIntensitiesChunkSizePosition
                mBW.Seek(-RIntensitiesChunkSize, SeekOrigin.Current)
                mBW.Write(RIntensitiesChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim RSChunkSize As Integer = mBW.BaseStream.Position - RSChunkSizePosition
                mBW.Seek(-RSChunkSize, SeekOrigin.Current)
                mBW.Write(RSChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

            Next

            'C0262===
            Dim RatingScalesChunkSize As Integer = mBW.BaseStream.Position - RatingScalesChunkSizePosition
            mBW.Seek(-RatingScalesChunkSize, SeekOrigin.Current)
            mBW.Write(RatingScalesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.RegularUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_REGULAR_UTILITY_CURVES)

            Dim RUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RegularUtilityCurves.Count)
            For Each RUC As clsRegularUtilityCurve In ProjectManager.MeasureScales.RegularUtilityCurves
                mBW.Write(CHUNK_REGULAR_UTILITY_CURVE)

                Dim RUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RUC.ID)
                mBW.Write(RUC.GuidID.ToByteArray) 'C0283
                mBW.Write(RUC.Name)
                mBW.Write(RUC.Low)
                mBW.Write(RUC.High)
                mBW.Write(RUC.Curvature)
                mBW.Write(RUC.IsIncreasing)
                mBW.Write(RUC.Comment)

                'C0262===
                Dim RUCChunkSize As Integer = mBW.BaseStream.Position - RUCChunkSizePosition
                mBW.Seek(-RUCChunkSize, SeekOrigin.Current)
                mBW.Write(RUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim RUCsChunkSize As Integer = mBW.BaseStream.Position - RUCsChunkSizePosition
            mBW.Seek(-RUCsChunkSize, SeekOrigin.Current)
            mBW.Write(RUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.AdvancedUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_ADVANCED_UTILITY_CURVES)

            Dim AUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.AdvancedUtilityCurves.Count)
            For Each AUC As clsAdvancedUtilityCurve In ProjectManager.MeasureScales.AdvancedUtilityCurves
                mBW.Write(CHUNK_ADVANCED_UTILITY_CURVE)

                Dim AUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.ID)
                mBW.Write(AUC.GuidID.ToByteArray) 'C0283
                mBW.Write(AUC.Name)
                mBW.Write(CInt(AUC.InterpolationMethod))
                mBW.Write(AUC.Comment)

                mBW.Write(CHUNK_AUC_POINTS)

                Dim AUCPointsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.Points.Count)

                For Each point As clsUCPoint In AUC.Points
                    mBW.Write(point.X)
                    mBW.Write(point.Y)
                Next

                'C0262===
                Dim AUCPointsChunkSize As Integer = mBW.BaseStream.Position - AUCPointsChunkSizePosition
                mBW.Seek(-AUCPointsChunkSize, SeekOrigin.Current)
                mBW.Write(AUCPointsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim AUCChunkSize As Integer = mBW.BaseStream.Position - AUCChunkSizePosition
                mBW.Seek(-AUCChunkSize, SeekOrigin.Current)
                mBW.Write(AUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim AUCsChunkSize As Integer = mBW.BaseStream.Position - AUCsChunkSizePosition
            mBW.Seek(-AUCsChunkSize, SeekOrigin.Current)
            mBW.Write(AUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.StepFunctions.Count <> 0 Then
            mBW.Write(CHUNK_STEP_FUNCTIONS)

            Dim SFsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.StepFunctions.Count)
            For Each SF As clsStepFunction In ProjectManager.MeasureScales.StepFunctions
                mBW.Write(CHUNK_STEP_FUNCTION)

                Dim SFChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.ID)
                mBW.Write(SF.GuidID.ToByteArray) 'C0283
                mBW.Write(SF.Name)
                mBW.Write(SF.IsPiecewiseLinear) 'C0329
                mBW.Write(SF.Comment)

                mBW.Write(CHUNK_STEP_INTERVALS)

                Dim SIsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.Intervals.Count)
                For Each interval As clsStepInterval In SF.Intervals
                    mBW.Write(CHUNK_STEP_INTERVAL)

                    Dim SIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(interval.ID)
                    'mBW.Write(interval.GuidID.ToByteArray)
                    mBW.Write(interval.Name)
                    mBW.Write(interval.Low)
                    mBW.Write(interval.High)
                    mBW.Write(interval.Value)
                    mBW.Write(interval.Comment)

                    'C0262===
                    Dim SIChunkSize As Integer = mBW.BaseStream.Position - SIChunkSizePosition
                    mBW.Seek(-SIChunkSize, SeekOrigin.Current)
                    mBW.Write(SIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim SIsChunkSize As Integer = mBW.BaseStream.Position - SIsChunkSizePosition
                mBW.Seek(-SIsChunkSize, SeekOrigin.Current)
                mBW.Write(SIsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim SFChunkSize As Integer = mBW.BaseStream.Position - SFChunkSizePosition
                mBW.Seek(-SFChunkSize, SeekOrigin.Current)
                mBW.Write(SFChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim SFsChunkSize As Integer = mBW.BaseStream.Position - SFsChunkSizePosition
            mBW.Seek(-SFsChunkSize, SeekOrigin.Current)
            mBW.Write(SFsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        'C0262===
        Dim MeasurementScalesChunkSize As Integer = mBW.BaseStream.Position - MeasurementScalesChunkSizePosition
        mBW.Seek(-MeasurementScalesChunkSize, SeekOrigin.Current)
        mBW.Write(MeasurementScalesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessMeasurementScales completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt((ProjectManager.Hierarchies.Count + ProjectManager.MeasureHierarchies.Count) * ProjectManager.AltsHierarchies.Count))

        Dim Hierarchies As New List(Of clsHierarchy)
        For Each H As clsHierarchy In ProjectManager.Hierarchies
            Hierarchies.Add(H)
        Next
        For Each H As clsHierarchy In ProjectManager.MeasureHierarchies
            Hierarchies.Add(H)
        Next

        For Each H As clsHierarchy In Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                            For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                    count += 1
                                End If
                            Next
                        Else
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                For Each J As clsCustomMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                                    If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                        count += 1
                                    End If
                                Next
                            Next
                        End If
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                        mBW.Write(pwData.IsUndefined)
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                        If node.MeasureType = ECMeasureType.mtPairwise Then
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            End If
                                        Else
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)
                                        End If

                                        mBW.Write(pwData.Advantage)
                                        mBW.Write(pwData.Value)
                                        mBW.Write(pwData.Comment)

                                        If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                            pwData.ModifyDate = JudgmentsTime
                                        End If

                                        mBW.Write(pwData.ModifyDate.ToBinary)
                                    End If
                                Next
                            Case ECMeasureType.mtPWOutcomes
                                For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                    For Each pwData As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                                        If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                            mBW.Write(pwData.IsUndefined)
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(H.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            End If
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                            mBW.Write(pwData.Advantage)
                                            mBW.Write(pwData.Value)
                                            mBW.Write(pwData.Comment)

                                            If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                                pwData.ModifyDate = JudgmentsTime
                                            End If

                                            mBW.Write(pwData.ModifyDate.ToBinary)
                                        End If
                                    Next
                                Next
                            Case Else
                                For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then 'C0272
                                        mBW.Write(nonpwData.IsUndefined)
                                        If node.IsTerminalNode Then
                                            mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        Else
                                            mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        End If

                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                                'C0396===
                                                ' write a flag to determine whether we have a rating of direct value
                                                If nonpwData.IsUndefined Then
                                                    mBW.Write(True)
                                                    mBW.Write(CSng(-1))
                                                Else
                                                    mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                                    If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                        mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                                    Else
                                                        mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                                    End If
                                                End If
                                                'C0396==
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                            Case ECMeasureType.mtStep
                                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                            Case ECMeasureType.mtDirect
                                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                        End Select
                                        mBW.Write(nonpwData.Comment)
                                        'C0369===
                                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                            nonpwData.ModifyDate = JudgmentsTime
                                        End If
                                        'C0369==
                                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                                    End If
                                Next
                        End Select
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next


                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_21
    Inherits clsStreamModelWriter_v_1_1_19

    Protected Overrides Function ProcessMeasurementScalesChunk() As Boolean
        mBW.Write(CHUNK_MEASUREMENT_SCALES)

        Dim MeasurementScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.MeasureScales.AllScales.Count)

        If ProjectManager.MeasureScales.RatingsScales.Count <> 0 Then
            mBW.Write(CHUNK_RATINGS_SCALES)

            Dim RatingScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RatingsScales.Count)
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                mBW.Write(CHUNK_RATING_SCALE)

                Dim RSChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.ID)
                mBW.Write(RS.GuidID.ToByteArray) 'C0283
                mBW.Write(RS.Name)
                mBW.Write(RS.Comment)
                mBW.Write(RS.IsOutcomes)
                mBW.Write(RS.IsPWofPercentages)

                mBW.Write(CHUNK_RATING_INTENSITIES)

                Dim RIntensitiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.RatingSet.Count)

                For Each R As clsRating In RS.RatingSet
                    If R.Value < 0 Then
                        R.Value = 0
                    End If

                    If R.Value > 1 Then
                        R.Value = 1
                    End If

                    mBW.Write(CHUNK_RATING_INTENSITY)

                    Dim RIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(R.ID)
                    mBW.Write(R.GuidID.ToByteArray) 'C0261
                    mBW.Write(R.Name)
                    mBW.Write(R.Value)
                    mBW.Write(R.Comment)
                    mBW.Write(R.AvailableForPW)

                    'C0262===
                    Dim RIChunkSize As Integer = mBW.BaseStream.Position - RIChunkSizePosition
                    mBW.Seek(-RIChunkSize, SeekOrigin.Current)
                    mBW.Write(RIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim RIntensitiesChunkSize As Integer = mBW.BaseStream.Position - RIntensitiesChunkSizePosition
                mBW.Seek(-RIntensitiesChunkSize, SeekOrigin.Current)
                mBW.Write(RIntensitiesChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim RSChunkSize As Integer = mBW.BaseStream.Position - RSChunkSizePosition
                mBW.Seek(-RSChunkSize, SeekOrigin.Current)
                mBW.Write(RSChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

            Next

            'C0262===
            Dim RatingScalesChunkSize As Integer = mBW.BaseStream.Position - RatingScalesChunkSizePosition
            mBW.Seek(-RatingScalesChunkSize, SeekOrigin.Current)
            mBW.Write(RatingScalesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.RegularUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_REGULAR_UTILITY_CURVES)

            Dim RUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RegularUtilityCurves.Count)
            For Each RUC As clsRegularUtilityCurve In ProjectManager.MeasureScales.RegularUtilityCurves
                mBW.Write(CHUNK_REGULAR_UTILITY_CURVE)

                Dim RUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RUC.ID)
                mBW.Write(RUC.GuidID.ToByteArray) 'C0283
                mBW.Write(RUC.Name)
                mBW.Write(RUC.Low)
                mBW.Write(RUC.High)
                mBW.Write(RUC.Curvature)
                mBW.Write(RUC.IsIncreasing)
                mBW.Write(RUC.Comment)

                'C0262===
                Dim RUCChunkSize As Integer = mBW.BaseStream.Position - RUCChunkSizePosition
                mBW.Seek(-RUCChunkSize, SeekOrigin.Current)
                mBW.Write(RUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim RUCsChunkSize As Integer = mBW.BaseStream.Position - RUCsChunkSizePosition
            mBW.Seek(-RUCsChunkSize, SeekOrigin.Current)
            mBW.Write(RUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.AdvancedUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_ADVANCED_UTILITY_CURVES)

            Dim AUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.AdvancedUtilityCurves.Count)
            For Each AUC As clsAdvancedUtilityCurve In ProjectManager.MeasureScales.AdvancedUtilityCurves
                mBW.Write(CHUNK_ADVANCED_UTILITY_CURVE)

                Dim AUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.ID)
                mBW.Write(AUC.GuidID.ToByteArray) 'C0283
                mBW.Write(AUC.Name)
                mBW.Write(CInt(AUC.InterpolationMethod))
                mBW.Write(AUC.Comment)

                mBW.Write(CHUNK_AUC_POINTS)

                Dim AUCPointsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.Points.Count)

                For Each point As clsUCPoint In AUC.Points
                    mBW.Write(point.X)
                    mBW.Write(point.Y)
                Next

                'C0262===
                Dim AUCPointsChunkSize As Integer = mBW.BaseStream.Position - AUCPointsChunkSizePosition
                mBW.Seek(-AUCPointsChunkSize, SeekOrigin.Current)
                mBW.Write(AUCPointsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim AUCChunkSize As Integer = mBW.BaseStream.Position - AUCChunkSizePosition
                mBW.Seek(-AUCChunkSize, SeekOrigin.Current)
                mBW.Write(AUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim AUCsChunkSize As Integer = mBW.BaseStream.Position - AUCsChunkSizePosition
            mBW.Seek(-AUCsChunkSize, SeekOrigin.Current)
            mBW.Write(AUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.StepFunctions.Count <> 0 Then
            mBW.Write(CHUNK_STEP_FUNCTIONS)

            Dim SFsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.StepFunctions.Count)
            For Each SF As clsStepFunction In ProjectManager.MeasureScales.StepFunctions
                mBW.Write(CHUNK_STEP_FUNCTION)

                Dim SFChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.ID)
                mBW.Write(SF.GuidID.ToByteArray) 'C0283
                mBW.Write(SF.Name)
                mBW.Write(SF.IsPiecewiseLinear) 'C0329
                mBW.Write(SF.Comment)

                mBW.Write(CHUNK_STEP_INTERVALS)

                Dim SIsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.Intervals.Count)
                For Each interval As clsStepInterval In SF.Intervals
                    mBW.Write(CHUNK_STEP_INTERVAL)

                    Dim SIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(interval.ID)
                    'mBW.Write(interval.GuidID.ToByteArray)
                    mBW.Write(interval.Name)
                    mBW.Write(interval.Low)
                    mBW.Write(interval.High)
                    mBW.Write(interval.Value)
                    mBW.Write(interval.Comment)

                    'C0262===
                    Dim SIChunkSize As Integer = mBW.BaseStream.Position - SIChunkSizePosition
                    mBW.Seek(-SIChunkSize, SeekOrigin.Current)
                    mBW.Write(SIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim SIsChunkSize As Integer = mBW.BaseStream.Position - SIsChunkSizePosition
                mBW.Seek(-SIsChunkSize, SeekOrigin.Current)
                mBW.Write(SIsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim SFChunkSize As Integer = mBW.BaseStream.Position - SFChunkSizePosition
                mBW.Seek(-SFChunkSize, SeekOrigin.Current)
                mBW.Write(SFChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim SFsChunkSize As Integer = mBW.BaseStream.Position - SFsChunkSizePosition
            mBW.Seek(-SFsChunkSize, SeekOrigin.Current)
            mBW.Write(SFsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        'C0262===
        Dim MeasurementScalesChunkSize As Integer = mBW.BaseStream.Position - MeasurementScalesChunkSizePosition
        mBW.Seek(-MeasurementScalesChunkSize, SeekOrigin.Current)
        mBW.Write(MeasurementScalesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessMeasurementScales completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_22
    Inherits clsStreamModelWriter_v_1_1_21

    Protected Overrides Function ProcessMeasurementScalesChunk() As Boolean
        mBW.Write(CHUNK_MEASUREMENT_SCALES)

        Dim MeasurementScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.MeasureScales.AllScales.Count)

        If ProjectManager.MeasureScales.RatingsScales.Count <> 0 Then
            mBW.Write(CHUNK_RATINGS_SCALES)

            Dim RatingScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RatingsScales.Count)
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                mBW.Write(CHUNK_RATING_SCALE)

                Dim RSChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.ID)
                mBW.Write(RS.GuidID.ToByteArray) 'C0283
                mBW.Write(RS.Name)
                mBW.Write(RS.Comment)
                mBW.Write(RS.IsOutcomes)
                mBW.Write(RS.IsPWofPercentages)
                mBW.Write(RS.IsExpectedValues)

                mBW.Write(CHUNK_RATING_INTENSITIES)

                Dim RIntensitiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.RatingSet.Count)

                For Each R As clsRating In RS.RatingSet
                    If R.Value < 0 Then
                        R.Value = 0
                    End If

                    If R.Value > 1 Then
                        R.Value = 1
                    End If

                    mBW.Write(CHUNK_RATING_INTENSITY)

                    Dim RIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(R.ID)
                    mBW.Write(R.GuidID.ToByteArray) 'C0261
                    mBW.Write(R.Name)
                    mBW.Write(R.Value)
                    mBW.Write(R.Value2)
                    mBW.Write(R.Comment)
                    mBW.Write(R.AvailableForPW)

                    'C0262===
                    Dim RIChunkSize As Integer = mBW.BaseStream.Position - RIChunkSizePosition
                    mBW.Seek(-RIChunkSize, SeekOrigin.Current)
                    mBW.Write(RIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim RIntensitiesChunkSize As Integer = mBW.BaseStream.Position - RIntensitiesChunkSizePosition
                mBW.Seek(-RIntensitiesChunkSize, SeekOrigin.Current)
                mBW.Write(RIntensitiesChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim RSChunkSize As Integer = mBW.BaseStream.Position - RSChunkSizePosition
                mBW.Seek(-RSChunkSize, SeekOrigin.Current)
                mBW.Write(RSChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

            Next

            'C0262===
            Dim RatingScalesChunkSize As Integer = mBW.BaseStream.Position - RatingScalesChunkSizePosition
            mBW.Seek(-RatingScalesChunkSize, SeekOrigin.Current)
            mBW.Write(RatingScalesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.RegularUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_REGULAR_UTILITY_CURVES)

            Dim RUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RegularUtilityCurves.Count)
            For Each RUC As clsRegularUtilityCurve In ProjectManager.MeasureScales.RegularUtilityCurves
                mBW.Write(CHUNK_REGULAR_UTILITY_CURVE)

                Dim RUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RUC.ID)
                mBW.Write(RUC.GuidID.ToByteArray) 'C0283
                mBW.Write(RUC.Name)
                mBW.Write(RUC.Low)
                mBW.Write(RUC.High)
                mBW.Write(RUC.Curvature)
                mBW.Write(RUC.IsIncreasing)
                mBW.Write(RUC.Comment)

                'C0262===
                Dim RUCChunkSize As Integer = mBW.BaseStream.Position - RUCChunkSizePosition
                mBW.Seek(-RUCChunkSize, SeekOrigin.Current)
                mBW.Write(RUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim RUCsChunkSize As Integer = mBW.BaseStream.Position - RUCsChunkSizePosition
            mBW.Seek(-RUCsChunkSize, SeekOrigin.Current)
            mBW.Write(RUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.AdvancedUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_ADVANCED_UTILITY_CURVES)

            Dim AUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.AdvancedUtilityCurves.Count)
            For Each AUC As clsAdvancedUtilityCurve In ProjectManager.MeasureScales.AdvancedUtilityCurves
                mBW.Write(CHUNK_ADVANCED_UTILITY_CURVE)

                Dim AUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.ID)
                mBW.Write(AUC.GuidID.ToByteArray) 'C0283
                mBW.Write(AUC.Name)
                mBW.Write(CInt(AUC.InterpolationMethod))
                mBW.Write(AUC.Comment)

                mBW.Write(CHUNK_AUC_POINTS)

                Dim AUCPointsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.Points.Count)

                For Each point As clsUCPoint In AUC.Points
                    mBW.Write(point.X)
                    mBW.Write(point.Y)
                Next

                'C0262===
                Dim AUCPointsChunkSize As Integer = mBW.BaseStream.Position - AUCPointsChunkSizePosition
                mBW.Seek(-AUCPointsChunkSize, SeekOrigin.Current)
                mBW.Write(AUCPointsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim AUCChunkSize As Integer = mBW.BaseStream.Position - AUCChunkSizePosition
                mBW.Seek(-AUCChunkSize, SeekOrigin.Current)
                mBW.Write(AUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim AUCsChunkSize As Integer = mBW.BaseStream.Position - AUCsChunkSizePosition
            mBW.Seek(-AUCsChunkSize, SeekOrigin.Current)
            mBW.Write(AUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.StepFunctions.Count <> 0 Then
            mBW.Write(CHUNK_STEP_FUNCTIONS)

            Dim SFsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.StepFunctions.Count)
            For Each SF As clsStepFunction In ProjectManager.MeasureScales.StepFunctions
                mBW.Write(CHUNK_STEP_FUNCTION)

                Dim SFChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.ID)
                mBW.Write(SF.GuidID.ToByteArray) 'C0283
                mBW.Write(SF.Name)
                mBW.Write(SF.IsPiecewiseLinear) 'C0329
                mBW.Write(SF.Comment)

                mBW.Write(CHUNK_STEP_INTERVALS)

                Dim SIsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.Intervals.Count)
                For Each interval As clsStepInterval In SF.Intervals
                    mBW.Write(CHUNK_STEP_INTERVAL)

                    Dim SIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(interval.ID)
                    'mBW.Write(interval.GuidID.ToByteArray)
                    mBW.Write(interval.Name)
                    mBW.Write(interval.Low)
                    mBW.Write(interval.High)
                    mBW.Write(interval.Value)
                    mBW.Write(interval.Comment)

                    'C0262===
                    Dim SIChunkSize As Integer = mBW.BaseStream.Position - SIChunkSizePosition
                    mBW.Seek(-SIChunkSize, SeekOrigin.Current)
                    mBW.Write(SIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim SIsChunkSize As Integer = mBW.BaseStream.Position - SIsChunkSizePosition
                mBW.Seek(-SIsChunkSize, SeekOrigin.Current)
                mBW.Write(SIsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim SFChunkSize As Integer = mBW.BaseStream.Position - SFChunkSizePosition
                mBW.Seek(-SFChunkSize, SeekOrigin.Current)
                mBW.Write(SFChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim SFsChunkSize As Integer = mBW.BaseStream.Position - SFsChunkSizePosition
            mBW.Seek(-SFsChunkSize, SeekOrigin.Current)
            mBW.Write(SFsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        'C0262===
        Dim MeasurementScalesChunkSize As Integer = mBW.BaseStream.Position - MeasurementScalesChunkSizePosition
        mBW.Seek(-MeasurementScalesChunkSize, SeekOrigin.Current)
        mBW.Write(MeasurementScalesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessMeasurementScales completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_24
    Inherits clsStreamModelWriter_v_1_1_22

    Protected Overrides Function ProcessMeasurementScalesChunk() As Boolean
        mBW.Write(CHUNK_MEASUREMENT_SCALES)

        Dim MeasurementScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.MeasureScales.AllScales.Count)

        If ProjectManager.MeasureScales.RatingsScales.Count <> 0 Then
            mBW.Write(CHUNK_RATINGS_SCALES)

            Dim RatingScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RatingsScales.Count)
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                mBW.Write(CHUNK_RATING_SCALE)

                Dim RSChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.ID)
                mBW.Write(RS.GuidID.ToByteArray) 'C0283
                mBW.Write(RS.Name)
                mBW.Write(RS.Comment)
                mBW.Write(RS.Type)
                mBW.Write(RS.IsOutcomes)
                mBW.Write(RS.IsPWofPercentages)
                mBW.Write(RS.IsExpectedValues)

                mBW.Write(CHUNK_RATING_INTENSITIES)

                Dim RIntensitiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.RatingSet.Count)

                For Each R As clsRating In RS.RatingSet
                    If R.Value < 0 Then
                        R.Value = 0
                    End If

                    If R.Value > 1 Then
                        R.Value = 1
                    End If

                    mBW.Write(CHUNK_RATING_INTENSITY)

                    Dim RIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(R.ID)
                    mBW.Write(R.GuidID.ToByteArray) 'C0261
                    mBW.Write(R.Name)
                    mBW.Write(R.Value)
                    mBW.Write(R.Value2)
                    mBW.Write(R.Comment)
                    mBW.Write(R.AvailableForPW)

                    'C0262===
                    Dim RIChunkSize As Integer = mBW.BaseStream.Position - RIChunkSizePosition
                    mBW.Seek(-RIChunkSize, SeekOrigin.Current)
                    mBW.Write(RIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim RIntensitiesChunkSize As Integer = mBW.BaseStream.Position - RIntensitiesChunkSizePosition
                mBW.Seek(-RIntensitiesChunkSize, SeekOrigin.Current)
                mBW.Write(RIntensitiesChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim RSChunkSize As Integer = mBW.BaseStream.Position - RSChunkSizePosition
                mBW.Seek(-RSChunkSize, SeekOrigin.Current)
                mBW.Write(RSChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

            Next

            'C0262===
            Dim RatingScalesChunkSize As Integer = mBW.BaseStream.Position - RatingScalesChunkSizePosition
            mBW.Seek(-RatingScalesChunkSize, SeekOrigin.Current)
            mBW.Write(RatingScalesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.RegularUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_REGULAR_UTILITY_CURVES)

            Dim RUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RegularUtilityCurves.Count)
            For Each RUC As clsRegularUtilityCurve In ProjectManager.MeasureScales.RegularUtilityCurves
                mBW.Write(CHUNK_REGULAR_UTILITY_CURVE)

                Dim RUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RUC.ID)
                mBW.Write(RUC.GuidID.ToByteArray) 'C0283
                mBW.Write(RUC.Name)
                mBW.Write(RUC.Low)
                mBW.Write(RUC.High)
                mBW.Write(RUC.Curvature)
                mBW.Write(RUC.IsIncreasing)
                mBW.Write(RUC.Comment)
                mBW.Write(RUC.Type)

                'C0262===
                Dim RUCChunkSize As Integer = mBW.BaseStream.Position - RUCChunkSizePosition
                mBW.Seek(-RUCChunkSize, SeekOrigin.Current)
                mBW.Write(RUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim RUCsChunkSize As Integer = mBW.BaseStream.Position - RUCsChunkSizePosition
            mBW.Seek(-RUCsChunkSize, SeekOrigin.Current)
            mBW.Write(RUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.AdvancedUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_ADVANCED_UTILITY_CURVES)

            Dim AUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.AdvancedUtilityCurves.Count)
            For Each AUC As clsAdvancedUtilityCurve In ProjectManager.MeasureScales.AdvancedUtilityCurves
                mBW.Write(CHUNK_ADVANCED_UTILITY_CURVE)

                Dim AUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.ID)
                mBW.Write(AUC.GuidID.ToByteArray) 'C0283
                mBW.Write(AUC.Name)
                mBW.Write(CInt(AUC.InterpolationMethod))
                mBW.Write(AUC.Comment)
                mBW.Write(AUC.Type)

                mBW.Write(CHUNK_AUC_POINTS)

                Dim AUCPointsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.Points.Count)

                For Each point As clsUCPoint In AUC.Points
                    mBW.Write(point.X)
                    mBW.Write(point.Y)
                Next

                'C0262===
                Dim AUCPointsChunkSize As Integer = mBW.BaseStream.Position - AUCPointsChunkSizePosition
                mBW.Seek(-AUCPointsChunkSize, SeekOrigin.Current)
                mBW.Write(AUCPointsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim AUCChunkSize As Integer = mBW.BaseStream.Position - AUCChunkSizePosition
                mBW.Seek(-AUCChunkSize, SeekOrigin.Current)
                mBW.Write(AUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim AUCsChunkSize As Integer = mBW.BaseStream.Position - AUCsChunkSizePosition
            mBW.Seek(-AUCsChunkSize, SeekOrigin.Current)
            mBW.Write(AUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.StepFunctions.Count <> 0 Then
            mBW.Write(CHUNK_STEP_FUNCTIONS)

            Dim SFsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.StepFunctions.Count)
            For Each SF As clsStepFunction In ProjectManager.MeasureScales.StepFunctions
                mBW.Write(CHUNK_STEP_FUNCTION)

                Dim SFChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.ID)
                mBW.Write(SF.GuidID.ToByteArray) 'C0283
                mBW.Write(SF.Name)
                mBW.Write(SF.IsPiecewiseLinear) 'C0329
                mBW.Write(SF.Comment)
                mBW.Write(SF.Type)

                mBW.Write(CHUNK_STEP_INTERVALS)

                Dim SIsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.Intervals.Count)
                For Each interval As clsStepInterval In SF.Intervals
                    mBW.Write(CHUNK_STEP_INTERVAL)

                    Dim SIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(interval.ID)
                    'mBW.Write(interval.GuidID.ToByteArray)
                    mBW.Write(interval.Name)
                    mBW.Write(interval.Low)
                    mBW.Write(interval.High)
                    mBW.Write(interval.Value)
                    mBW.Write(interval.Comment)

                    'C0262===
                    Dim SIChunkSize As Integer = mBW.BaseStream.Position - SIChunkSizePosition
                    mBW.Seek(-SIChunkSize, SeekOrigin.Current)
                    mBW.Write(SIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim SIsChunkSize As Integer = mBW.BaseStream.Position - SIsChunkSizePosition
                mBW.Seek(-SIsChunkSize, SeekOrigin.Current)
                mBW.Write(SIsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim SFChunkSize As Integer = mBW.BaseStream.Position - SFChunkSizePosition
                mBW.Seek(-SFChunkSize, SeekOrigin.Current)
                mBW.Write(SFChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim SFsChunkSize As Integer = mBW.BaseStream.Position - SFsChunkSizePosition
            mBW.Seek(-SFsChunkSize, SeekOrigin.Current)
            mBW.Write(SFsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        'C0262===
        Dim MeasurementScalesChunkSize As Integer = mBW.BaseStream.Position - MeasurementScalesChunkSizePosition
        mBW.Seek(-MeasurementScalesChunkSize, SeekOrigin.Current)
        mBW.Write(MeasurementScalesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessMeasurementScales completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_25
    Inherits clsStreamModelWriter_v_1_1_24

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt((ProjectManager.Hierarchies.Count + ProjectManager.MeasureHierarchies.Count) * ProjectManager.AltsHierarchies.Count))

        Dim Hierarchies As New List(Of clsHierarchy)
        For Each H As clsHierarchy In ProjectManager.Hierarchies
            Hierarchies.Add(H)
        Next
        For Each H As clsHierarchy In ProjectManager.MeasureHierarchies
            Hierarchies.Add(H)
        Next

        For Each H As clsHierarchy In Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                            For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                    count += 1
                                End If
                            Next
                        Else
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                For Each J As clsCustomMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                                    If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                        count += 1
                                    End If
                                Next
                            Next
                        End If
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                        mBW.Write(pwData.IsUndefined)
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            End If
                                        Else
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)
                                        End If

                                        mBW.Write(pwData.Advantage)
                                        mBW.Write(pwData.Value)
                                        mBW.Write(pwData.Comment)

                                        If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                            pwData.ModifyDate = JudgmentsTime
                                        End If

                                        mBW.Write(pwData.ModifyDate.ToBinary)
                                    End If
                                Next
                            Case ECMeasureType.mtPWOutcomes
                                For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                    For Each pwData As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                                        If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                            mBW.Write(pwData.IsUndefined)
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(H.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            End If
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                            mBW.Write(pwData.Advantage)
                                            mBW.Write(pwData.Value)
                                            mBW.Write(pwData.Comment)

                                            If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                                pwData.ModifyDate = JudgmentsTime
                                            End If

                                            mBW.Write(pwData.ModifyDate.ToBinary)
                                        End If
                                    Next
                                Next
                            Case Else
                                For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then 'C0272
                                        mBW.Write(nonpwData.IsUndefined)
                                        If node.IsTerminalNode Then
                                            mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        Else
                                            mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        End If

                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                                'C0396===
                                                ' write a flag to determine whether we have a rating of direct value
                                                If nonpwData.IsUndefined Then
                                                    mBW.Write(True)
                                                    mBW.Write(CSng(-1))
                                                Else
                                                    mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                                    If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                        mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                                    Else
                                                        mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                                    End If
                                                End If
                                                'C0396==
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                            Case ECMeasureType.mtStep
                                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                            Case ECMeasureType.mtDirect
                                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                        End Select
                                        mBW.Write(nonpwData.Comment)
                                        'C0369===
                                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                            nonpwData.ModifyDate = JudgmentsTime
                                        End If
                                        'C0369==
                                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                                    End If
                                Next
                        End Select
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                Dim UncontributedAlts As List(Of clsNode) = H.GetUncontributedAlternatives
                Dim NoContributionCount As Integer = UncontributedAlts.Count
                mBW.Write(NoContributionCount)

                If NoContributionCount > 0 Then
                    bHasAtLeastOneJudgment = True
                End If

                For Each alt As clsNode In UncontributedAlts
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(alt.NodeGuidID.ToByteArray)
                    If alt.MeasurementScale IsNot Nothing Then
                        mBW.Write(alt.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If

                    Dim rCount As Integer = 0
                    For Each RatingsData As clsRatingMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                        If Not RatingsData.IsUndefined OrElse (RatingsData.Comment <> "") Then
                            rCount += 1
                        End If
                    Next

                    mBW.Write(rCount)

                    For Each RatingsData As clsRatingMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                        If Not RatingsData.IsUndefined OrElse (RatingsData.Comment <> "") Then
                            mBW.Write(RatingsData.IsUndefined)

                            If RatingsData.IsUndefined Then
                                mBW.Write(True)
                                mBW.Write(CSng(-1))
                            Else
                                mBW.Write(CBool(RatingsData.Rating.ID = -1))
                                If RatingsData.Rating.ID <> -1 Then
                                    mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(CType(RatingsData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                Else
                                    mBW.Write(RatingsData.Rating.Value)
                                End If
                            End If
                            mBW.Write(RatingsData.Comment)
                            If (RatingsData.ModifyDate = VERY_OLD_DATE) Or (RatingsData.ModifyDate = UNDEFINED_DATE) Then
                                RatingsData.ModifyDate = JudgmentsTime
                            End If
                            mBW.Write(RatingsData.ModifyDate.ToBinary)
                        End If
                    Next

                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                Next


                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_27
    Inherits clsStreamModelWriter_v_1_1_25

    Public Overrides Function WriteUserJudgmentsControls(User As ECCore.ECTypes.clsUser, JudgmentsTime As Date) As Boolean
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgmentsForControls (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt(ProjectManager.Controls.Controls.Count))

        For Each control As clsControl In ProjectManager.Controls.Controls
            mBW.Write(CHUNK_CONTROL_JUDGMENTS)

            Dim ControlJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            mBW.Write(control.ID.ToByteArray)
            mBW.Write(CInt(control.Assignments.Count))

            For Each assignment As clsControlAssignment In control.Assignments
                mBW.Write(CHUNK_CONTROL_ASSIGNMENT_JUDGMENTS)

                Dim AssignmentJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                mBW.Write(assignment.ID.ToByteArray)
                mBW.Write(assignment.MeasurementType)
                mBW.Write(assignment.MeasurementScaleGuid.ToByteArray)

                Dim count As Integer = 0
                For Each J As clsNonPairwiseMeasureData In assignment.Judgments.JudgmentsFromUser(User.UserID)
                    If Not J.IsUndefined Or J.Comment <> "" Then count += 1
                Next
                mBW.Write(count)
                If count > 0 Then bHasAtLeastOneJudgment = True

                For Each nonpwData As clsNonPairwiseMeasureData In assignment.Judgments.JudgmentsFromUser(User.UserID)
                    If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                        mBW.Write(nonpwData.IsUndefined)

                        mBW.Write(nonpwData.CtrlObjectiveID.ToByteArray)
                        mBW.Write(nonpwData.CtrlEventID.ToByteArray)

                        Select Case assignment.MeasurementType
                            Case ECMeasureType.mtRatings
                                If nonpwData.IsUndefined Then
                                    mBW.Write(True)
                                    mBW.Write(CSng(-1))
                                Else
                                    mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1))
                                    If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                        mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.GuidID.ToByteArray)
                                    Else
                                        mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                    End If
                                End If
                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                            Case ECMeasureType.mtStep
                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                            Case ECMeasureType.mtDirect
                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                        End Select
                        mBW.Write(nonpwData.Comment)

                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                            nonpwData.ModifyDate = JudgmentsTime
                        End If

                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                    End If
                Next

                Dim AssignmentJudgmentsChunkSize As Integer = mBW.BaseStream.Position - AssignmentJudgmentsChunkSizePosition
                mBW.Seek(-AssignmentJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(AssignmentJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
            Next

            Dim ControlJudgmentsChunkSize As Integer = mBW.BaseStream.Position - ControlJudgmentsChunkSizePosition
            mBW.Seek(-ControlJudgmentsChunkSize, SeekOrigin.Current)
            mBW.Write(ControlJudgmentsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        Return bHasAtLeastOneJudgment
    End Function

    Public Overrides Function WriteControlsPermissions(ByVal UserID As Integer) As Boolean
        mBW = New BinaryWriter(mStream)

        mBW.Write(CHUNK_CONTROLS_PERMISSIONS)

        Dim ControlsPermissionsChunkSizePosition As Integer = mBW.BaseStream.Position
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

        Dim restrictedObjectives As List(Of Guid) = ProjectManager.ControlsRoles.GetRestrictedObjectives(UserID)
        mBW.Write(restrictedObjectives.Count)

        For Each ObjID As Guid In restrictedObjectives
            mBW.Write(ObjID.ToByteArray)
        Next

        Dim allowedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetAllowedObjectives(UserID)
        mBW.Write(allowedObjectives.Count)

        For Each ObjID As Guid In allowedObjectives
            mBW.Write(ObjID.ToByteArray)
        Next

        Dim ControlsPermissionsChunkSize As Integer = mBW.BaseStream.Position - ControlsPermissionsChunkSizePosition
        mBW.Seek(-ControlsPermissionsChunkSize, SeekOrigin.Current)
        mBW.Write(ControlsPermissionsChunkSize)
        mBW.Seek(0, SeekOrigin.End)

        mBW.Close()
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_28
    Inherits clsStreamModelWriter_v_1_1_27

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt((ProjectManager.Hierarchies.Count + ProjectManager.MeasureHierarchies.Count) * ProjectManager.AltsHierarchies.Count))

        Dim Hierarchies As New List(Of clsHierarchy)
        For Each H As clsHierarchy In ProjectManager.Hierarchies
            Hierarchies.Add(H)
        Next
        For Each H As clsHierarchy In ProjectManager.MeasureHierarchies
            Hierarchies.Add(H)
        Next

        For Each H As clsHierarchy In Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                            For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                    count += 1
                                End If
                            Next
                        Else
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                For Each J As clsCustomMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                                    If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                        count += 1
                                    End If
                                Next
                            Next
                        End If
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                        mBW.Write(pwData.IsUndefined)
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            End If
                                        Else
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)
                                        End If

                                        mBW.Write(pwData.Advantage)
                                        mBW.Write(pwData.Value)
                                        mBW.Write(pwData.Comment)

                                        If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                            pwData.ModifyDate = JudgmentsTime
                                        End If

                                        mBW.Write(pwData.ModifyDate.ToBinary)
                                    End If
                                Next
                            Case ECMeasureType.mtPWOutcomes
                                For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                    For Each pwData As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID)
                                        If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                            mBW.Write(pwData.IsUndefined)
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(H.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            End If
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                            mBW.Write(pwData.Advantage)
                                            mBW.Write(pwData.Value)
                                            mBW.Write(pwData.Comment)

                                            If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                                pwData.ModifyDate = JudgmentsTime
                                            End If

                                            mBW.Write(pwData.ModifyDate.ToBinary)
                                        End If
                                    Next
                                Next
                            Case Else
                                For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then 'C0272
                                        mBW.Write(nonpwData.IsUndefined)
                                        If node.IsTerminalNode Then
                                            mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        Else
                                            mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        End If

                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                                'C0396===
                                                ' write a flag to determine whether we have a rating of direct value
                                                If nonpwData.IsUndefined Then
                                                    mBW.Write(True)
                                                    mBW.Write(CSng(-1))
                                                Else
                                                    mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                                    If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                        mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                                    Else
                                                        mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                                    End If
                                                End If
                                                'C0396==
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                            Case ECMeasureType.mtStep
                                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                            Case ECMeasureType.mtDirect
                                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                        End Select
                                        mBW.Write(nonpwData.Comment)
                                        'C0369===
                                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                            nonpwData.ModifyDate = JudgmentsTime
                                        End If
                                        'C0369==
                                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                                    End If
                                Next
                        End Select
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                Dim UncontributedAlts As List(Of clsNode) = H.GetUncontributedAlternatives
                Dim NoContributionCount As Integer = UncontributedAlts.Count
                mBW.Write(NoContributionCount)

                If NoContributionCount > 0 Then
                    bHasAtLeastOneJudgment = True
                End If

                For Each alt As clsNode In UncontributedAlts
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(alt.NodeGuidID.ToByteArray)
                    mBW.Write(alt.MeasureType)
                    If alt.MeasurementScale IsNot Nothing Then
                        mBW.Write(alt.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If

                    Dim jCount As Integer = 0
                    For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                        If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                            jCount += 1
                        End If
                    Next

                    mBW.Write(jCount)

                    For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                        If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                            mBW.Write(nonpwData.IsUndefined)

                            Select Case alt.MeasureType
                                Case ECMeasureType.mtRatings
                                    Dim RatingsData As clsRatingMeasureData = CType(nonpwData, clsRatingMeasureData)
                                    If RatingsData.IsUndefined Then
                                        mBW.Write(True)
                                        mBW.Write(CSng(-1))
                                    Else
                                        mBW.Write(CBool(RatingsData.Rating.ID = -1))
                                        If RatingsData.Rating.ID <> -1 Then
                                            mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(CType(RatingsData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                        Else
                                            mBW.Write(RatingsData.Rating.Value)
                                        End If
                                    End If
                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                    mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                Case ECMeasureType.mtStep
                                    mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                Case ECMeasureType.mtDirect
                                    mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                            End Select

                            mBW.Write(nonpwData.Comment)
                            If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                nonpwData.ModifyDate = JudgmentsTime
                            End If
                            mBW.Write(nonpwData.ModifyDate.ToBinary)
                        End If
                    Next

                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                Next


                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_29
    Inherits clsStreamModelWriter_v_1_1_28

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt((ProjectManager.Hierarchies.Count + ProjectManager.MeasureHierarchies.Count) * ProjectManager.AltsHierarchies.Count))

        Dim Hierarchies As New List(Of clsHierarchy)
        For Each H As clsHierarchy In ProjectManager.Hierarchies
            Hierarchies.Add(H)
        Next
        For Each H As clsHierarchy In ProjectManager.MeasureHierarchies
            Hierarchies.Add(H)
        Next

        For Each H As clsHierarchy In Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                            For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                    count += 1
                                End If
                            Next
                        Else
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                For Each J As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, node.NodeID)
                                    If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                        count += 1
                                    End If
                                Next
                            Next
                        End If
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                        mBW.Write(node.NodeGuidID.ToByteArray)
                                        mBW.Write(pwData.IsUndefined)
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            End If
                                        Else
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)
                                        End If

                                        mBW.Write(pwData.Advantage)
                                        mBW.Write(pwData.Value)
                                        mBW.Write(pwData.Comment)

                                        If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                            pwData.ModifyDate = JudgmentsTime
                                        End If

                                        mBW.Write(pwData.ModifyDate.ToBinary)
                                    End If
                                Next
                            Case ECMeasureType.mtPWOutcomes
                                For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                    For Each pwData As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, node.NodeID)
                                        If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                            mBW.Write(node.NodeGuidID.ToByteArray)
                                            mBW.Write(pwData.IsUndefined)
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(H.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            End If
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                            mBW.Write(pwData.Advantage)
                                            mBW.Write(pwData.Value)
                                            mBW.Write(pwData.Comment)

                                            If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                                pwData.ModifyDate = JudgmentsTime
                                            End If

                                            mBW.Write(pwData.ModifyDate.ToBinary)
                                        End If
                                    Next
                                Next
                            Case Else
                                For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then 'C0272
                                        mBW.Write(node.NodeGuidID.ToByteArray)
                                        mBW.Write(nonpwData.IsUndefined)
                                        If node.IsTerminalNode Then
                                            mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        Else
                                            mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        End If

                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                                'C0396===
                                                ' write a flag to determine whether we have a rating of direct value
                                                If nonpwData.IsUndefined Then
                                                    mBW.Write(True)
                                                    mBW.Write(CSng(-1))
                                                Else
                                                    mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                                    If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                        mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                                    Else
                                                        mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                                    End If
                                                End If
                                                'C0396==
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                            Case ECMeasureType.mtStep
                                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                            Case ECMeasureType.mtDirect
                                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                        End Select
                                        mBW.Write(nonpwData.Comment)
                                        'C0369===
                                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                            nonpwData.ModifyDate = JudgmentsTime
                                        End If
                                        'C0369==
                                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                                    End If
                                Next
                        End Select
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                Dim UncontributedAlts As List(Of clsNode) = H.GetUncontributedAlternatives
                Dim NoContributionCount As Integer = UncontributedAlts.Count
                mBW.Write(NoContributionCount)

                If NoContributionCount > 0 Then
                    bHasAtLeastOneJudgment = True
                End If

                For Each alt As clsNode In UncontributedAlts
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(alt.NodeGuidID.ToByteArray)
                    mBW.Write(alt.MeasureType)
                    If alt.MeasurementScale IsNot Nothing Then
                        mBW.Write(alt.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If

                    Dim jCount As Integer = 0
                    For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                        If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                            jCount += 1
                        End If
                    Next

                    mBW.Write(jCount)

                    For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                        If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                            mBW.Write(nonpwData.IsUndefined)

                            Select Case alt.MeasureType
                                Case ECMeasureType.mtRatings
                                    Dim RatingsData As clsRatingMeasureData = CType(nonpwData, clsRatingMeasureData)
                                    If RatingsData.IsUndefined Then
                                        mBW.Write(True)
                                        mBW.Write(CSng(-1))
                                    Else
                                        mBW.Write(CBool(RatingsData.Rating.ID = -1))
                                        If RatingsData.Rating.ID <> -1 Then
                                            mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(CType(RatingsData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                        Else
                                            mBW.Write(RatingsData.Rating.Value)
                                        End If
                                    End If
                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                    mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                Case ECMeasureType.mtStep
                                    mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                Case ECMeasureType.mtDirect
                                    mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                            End Select

                            mBW.Write(nonpwData.Comment)
                            If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                nonpwData.ModifyDate = JudgmentsTime
                            End If
                            mBW.Write(nonpwData.ModifyDate.ToBinary)
                        End If
                    Next

                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                Next


                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_30
    Inherits clsStreamModelWriter_v_1_1_29

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt((ProjectManager.Hierarchies.Count + ProjectManager.MeasureHierarchies.Count) * ProjectManager.AltsHierarchies.Count))

        Dim Hierarchies As New List(Of clsHierarchy)
        For Each H As clsHierarchy In ProjectManager.Hierarchies
            Hierarchies.Add(H)
        Next
        For Each H As clsHierarchy In ProjectManager.MeasureHierarchies
            Hierarchies.Add(H)
        Next

        For Each H As clsHierarchy In Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                            For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                    count += 1
                                End If
                            Next
                        Else
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                For Each J As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, node.NodeID)
                                    If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                        count += 1
                                    End If
                                Next
                            Next
                        End If
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                        mBW.Write(node.NodeGuidID.ToByteArray)
                                        mBW.Write(pwData.IsUndefined)
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            End If
                                        Else
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)
                                        End If

                                        mBW.Write(pwData.Advantage)
                                        mBW.Write(pwData.Value)
                                        mBW.Write(pwData.Comment)

                                        If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                            pwData.ModifyDate = JudgmentsTime
                                        End If

                                        mBW.Write(pwData.ModifyDate.ToBinary)
                                    End If
                                Next
                            Case ECMeasureType.mtPWOutcomes
                                For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                    For Each pwData As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, node.NodeID)
                                        If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                            mBW.Write(node.NodeGuidID.ToByteArray)
                                            mBW.Write(pwData.IsUndefined)
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(H.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            End If
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                            mBW.Write(pwData.Advantage)
                                            mBW.Write(pwData.Value)
                                            mBW.Write(pwData.Comment)

                                            If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                                pwData.ModifyDate = JudgmentsTime
                                            End If

                                            mBW.Write(pwData.ModifyDate.ToBinary)
                                        End If
                                    Next
                                Next
                            Case Else
                                For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then 'C0272
                                        mBW.Write(node.NodeGuidID.ToByteArray)
                                        mBW.Write(nonpwData.IsUndefined)
                                        If node.IsTerminalNode Then
                                            mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        Else
                                            mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        End If

                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                                'C0396===
                                                ' write a flag to determine whether we have a rating of direct value
                                                If nonpwData.IsUndefined Then
                                                    mBW.Write(True)
                                                    mBW.Write(CSng(-1))
                                                Else
                                                    mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                                    If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                        mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                                    Else
                                                        mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                                    End If
                                                End If
                                                'C0396==
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                            Case ECMeasureType.mtStep
                                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                            Case ECMeasureType.mtDirect
                                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                        End Select
                                        mBW.Write(nonpwData.Comment)
                                        'C0369===
                                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                            nonpwData.ModifyDate = JudgmentsTime
                                        End If
                                        'C0369==
                                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                                    End If
                                Next
                        End Select
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                Dim UncontributedAlts As List(Of clsNode) = H.GetUncontributedAlternatives

                If UncontributedAlts.Count = 0 Then
                    For Each alt As clsNode In AH.TerminalNodes
                        If alt.DirectJudgmentsForNoCause IsNot Nothing AndAlso alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID).Count > 0 Then  ' D6768
                            UncontributedAlts.Add(alt)
                        End If
                    Next
                End If

                Dim NoContributionCount As Integer = UncontributedAlts.Count

                mBW.Write(NoContributionCount)

                If NoContributionCount > 0 Then
                    bHasAtLeastOneJudgment = True
                End If

                For Each alt As clsNode In UncontributedAlts
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(alt.NodeGuidID.ToByteArray)
                    mBW.Write(alt.MeasureType)
                    If alt.MeasurementScale IsNot Nothing Then
                        mBW.Write(alt.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If

                    Dim jCount As Integer = 0

                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                        For Each J As clsPairwiseMeasureData In alt.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, alt.NodeID)
                            If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                jCount += 1
                            End If
                        Next
                    Else
                        For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                            If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                                jCount += 1
                            End If
                        Next
                    End If

                    mBW.Write(jCount)

                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                        For Each pwData As clsPairwiseMeasureData In alt.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, alt.NodeID)
                            If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                mBW.Write(alt.NodeGuidID.ToByteArray)
                                mBW.Write(pwData.IsUndefined)
                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)

                                mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                mBW.Write(pwData.Advantage)
                                mBW.Write(pwData.Value)
                                mBW.Write(pwData.Comment)

                                If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                    pwData.ModifyDate = JudgmentsTime
                                End If

                                mBW.Write(pwData.ModifyDate.ToBinary)
                            End If
                        Next
                    Else
                        For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                            If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                                mBW.Write(nonpwData.IsUndefined)

                                Select Case alt.MeasureType
                                    Case ECMeasureType.mtRatings
                                        Dim RatingsData As clsRatingMeasureData = CType(nonpwData, clsRatingMeasureData)
                                        If RatingsData.IsUndefined Then
                                            mBW.Write(True)
                                            mBW.Write(CSng(-1))
                                        Else
                                            mBW.Write(CBool(RatingsData.Rating.ID = -1))
                                            If RatingsData.Rating.ID <> -1 Then
                                                mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(CType(RatingsData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                            Else
                                                mBW.Write(RatingsData.Rating.Value)
                                            End If
                                        End If
                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                        mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                    Case ECMeasureType.mtStep
                                        mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                    Case ECMeasureType.mtDirect
                                        mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                End Select

                                mBW.Write(nonpwData.Comment)
                                If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                    nonpwData.ModifyDate = JudgmentsTime
                                End If
                                mBW.Write(nonpwData.ModifyDate.ToBinary)
                            End If
                        Next
                    End If

                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                Next


                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_32
    Inherits clsStreamModelWriter_v_1_1_30

    Protected Overrides Function ProcessMeasurementScalesChunk() As Boolean
        mBW.Write(CHUNK_MEASUREMENT_SCALES)

        Dim MeasurementScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(ProjectManager.MeasureScales.AllScales.Count)

        If ProjectManager.MeasureScales.RatingsScales.Count <> 0 Then
            mBW.Write(CHUNK_RATINGS_SCALES)

            Dim RatingScalesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RatingsScales.Count)
            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                mBW.Write(CHUNK_RATING_SCALE)

                Dim RSChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.ID)
                mBW.Write(RS.GuidID.ToByteArray) 'C0283
                mBW.Write(RS.Name)
                mBW.Write(RS.Comment)
                mBW.Write(RS.Type)
                mBW.Write(RS.IsOutcomes)
                mBW.Write(RS.IsPWofPercentages)
                mBW.Write(RS.IsExpectedValues)
                mBW.Write(RS.IsDefault)

                mBW.Write(CHUNK_RATING_INTENSITIES)

                Dim RIntensitiesChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RS.RatingSet.Count)

                For Each R As clsRating In RS.RatingSet
                    If R.Value < 0 Then
                        R.Value = 0
                    End If

                    If R.Value > 1 Then
                        R.Value = 1
                    End If

                    mBW.Write(CHUNK_RATING_INTENSITY)

                    Dim RIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(R.ID)
                    mBW.Write(R.GuidID.ToByteArray) 'C0261
                    mBW.Write(R.Name)
                    mBW.Write(R.Value)
                    mBW.Write(R.Value2)
                    mBW.Write(R.Comment)
                    mBW.Write(R.AvailableForPW)

                    'C0262===
                    Dim RIChunkSize As Integer = mBW.BaseStream.Position - RIChunkSizePosition
                    mBW.Seek(-RIChunkSize, SeekOrigin.Current)
                    mBW.Write(RIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim RIntensitiesChunkSize As Integer = mBW.BaseStream.Position - RIntensitiesChunkSizePosition
                mBW.Seek(-RIntensitiesChunkSize, SeekOrigin.Current)
                mBW.Write(RIntensitiesChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim RSChunkSize As Integer = mBW.BaseStream.Position - RSChunkSizePosition
                mBW.Seek(-RSChunkSize, SeekOrigin.Current)
                mBW.Write(RSChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

            Next

            'C0262===
            Dim RatingScalesChunkSize As Integer = mBW.BaseStream.Position - RatingScalesChunkSizePosition
            mBW.Seek(-RatingScalesChunkSize, SeekOrigin.Current)
            mBW.Write(RatingScalesChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.RegularUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_REGULAR_UTILITY_CURVES)

            Dim RUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.RegularUtilityCurves.Count)
            For Each RUC As clsRegularUtilityCurve In ProjectManager.MeasureScales.RegularUtilityCurves
                mBW.Write(CHUNK_REGULAR_UTILITY_CURVE)

                Dim RUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(RUC.ID)
                mBW.Write(RUC.GuidID.ToByteArray) 'C0283
                mBW.Write(RUC.Name)
                mBW.Write(RUC.Low)
                mBW.Write(RUC.High)
                mBW.Write(RUC.Curvature)
                mBW.Write(RUC.IsIncreasing)
                mBW.Write(RUC.Comment)
                mBW.Write(RUC.Type)
                mBW.Write(RUC.IsDefault)

                'C0262===
                Dim RUCChunkSize As Integer = mBW.BaseStream.Position - RUCChunkSizePosition
                mBW.Seek(-RUCChunkSize, SeekOrigin.Current)
                mBW.Write(RUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim RUCsChunkSize As Integer = mBW.BaseStream.Position - RUCsChunkSizePosition
            mBW.Seek(-RUCsChunkSize, SeekOrigin.Current)
            mBW.Write(RUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.AdvancedUtilityCurves.Count <> 0 Then
            mBW.Write(CHUNK_ADVANCED_UTILITY_CURVES)

            Dim AUCsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.AdvancedUtilityCurves.Count)
            For Each AUC As clsAdvancedUtilityCurve In ProjectManager.MeasureScales.AdvancedUtilityCurves
                mBW.Write(CHUNK_ADVANCED_UTILITY_CURVE)

                Dim AUCChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.ID)
                mBW.Write(AUC.GuidID.ToByteArray) 'C0283
                mBW.Write(AUC.Name)
                mBW.Write(CInt(AUC.InterpolationMethod))
                mBW.Write(AUC.Comment)
                mBW.Write(AUC.Type)
                mBW.Write(AUC.IsDefault)

                mBW.Write(CHUNK_AUC_POINTS)

                Dim AUCPointsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(AUC.Points.Count)

                For Each point As clsUCPoint In AUC.Points
                    mBW.Write(point.X)
                    mBW.Write(point.Y)
                Next

                'C0262===
                Dim AUCPointsChunkSize As Integer = mBW.BaseStream.Position - AUCPointsChunkSizePosition
                mBW.Seek(-AUCPointsChunkSize, SeekOrigin.Current)
                mBW.Write(AUCPointsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim AUCChunkSize As Integer = mBW.BaseStream.Position - AUCChunkSizePosition
                mBW.Seek(-AUCChunkSize, SeekOrigin.Current)
                mBW.Write(AUCChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim AUCsChunkSize As Integer = mBW.BaseStream.Position - AUCsChunkSizePosition
            mBW.Seek(-AUCsChunkSize, SeekOrigin.Current)
            mBW.Write(AUCsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        If ProjectManager.MeasureScales.StepFunctions.Count <> 0 Then
            mBW.Write(CHUNK_STEP_FUNCTIONS)

            Dim SFsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
            mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

            mBW.Write(ProjectManager.MeasureScales.StepFunctions.Count)
            For Each SF As clsStepFunction In ProjectManager.MeasureScales.StepFunctions
                mBW.Write(CHUNK_STEP_FUNCTION)

                Dim SFChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.ID)
                mBW.Write(SF.GuidID.ToByteArray) 'C0283
                mBW.Write(SF.Name)
                mBW.Write(SF.IsPiecewiseLinear) 'C0329
                mBW.Write(SF.Comment)
                mBW.Write(SF.Type)
                mBW.Write(SF.IsDefault)

                mBW.Write(CHUNK_STEP_INTERVALS)

                Dim SIsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(SF.Intervals.Count)
                For Each interval As clsStepInterval In SF.Intervals
                    mBW.Write(CHUNK_STEP_INTERVAL)

                    Dim SIChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(interval.ID)
                    'mBW.Write(interval.GuidID.ToByteArray)
                    mBW.Write(interval.Name)
                    mBW.Write(interval.Low)
                    mBW.Write(interval.High)
                    mBW.Write(interval.Value)
                    mBW.Write(interval.Comment)

                    'C0262===
                    Dim SIChunkSize As Integer = mBW.BaseStream.Position - SIChunkSizePosition
                    mBW.Seek(-SIChunkSize, SeekOrigin.Current)
                    mBW.Write(SIChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                'C0262===
                Dim SIsChunkSize As Integer = mBW.BaseStream.Position - SIsChunkSizePosition
                mBW.Seek(-SIsChunkSize, SeekOrigin.Current)
                mBW.Write(SIsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==

                'C0262===
                Dim SFChunkSize As Integer = mBW.BaseStream.Position - SFChunkSizePosition
                mBW.Seek(-SFChunkSize, SeekOrigin.Current)
                mBW.Write(SFChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next

            'C0262===
            Dim SFsChunkSize As Integer = mBW.BaseStream.Position - SFsChunkSizePosition
            mBW.Seek(-SFsChunkSize, SeekOrigin.Current)
            mBW.Write(SFsChunkSize)
            mBW.Seek(0, SeekOrigin.End)
            'C0262==
        End If

        'C0262===
        Dim MeasurementScalesChunkSize As Integer = mBW.BaseStream.Position - MeasurementScalesChunkSizePosition
        mBW.Seek(-MeasurementScalesChunkSize, SeekOrigin.Current)
        mBW.Write(MeasurementScalesChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        If mWriteStatus Then
            'Debug.Print("ProcessMeasurementScales completed " + Now.ToString + "  StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_33
    Inherits clsStreamModelWriter_v_1_1_32

    Protected Overrides Function ProcessNodeChunk(ByVal node As clsNode) As Boolean 'C0642
        If node.SavedToStream Then Exit Function

        mBW.Write(CHUNK_HIERARCHY_NODE)

        Dim NodeChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(node.NodeID)
        mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
        mBW.Write(node.NodeName)
        mBW.Write(node.ParentNodeID)

        'A1000 ===
        Dim parentNodesGuidsCount As Integer = 0
        If node.ParentNodesGuids IsNot Nothing Then parentNodesGuidsCount = node.ParentNodesGuids.Count
        mBW.Write(parentNodesGuidsCount)

        If node.ParentNodesGuids IsNot Nothing Then
            For Each NodeGuid As Guid In node.ParentNodesGuids
                mBW.Write(NodeGuid.ToByteArray)
            Next
        End If
        'A1000 ==

        mBW.Write(CInt(node.MeasureType))
        mBW.Write(node.MeasurementScaleID)
        mBW.Write(CInt(node.MeasureMode))
        mBW.Write(node.Enabled)
        If node.DefaultDataInstance IsNot Nothing Then
            mBW.Write(node.DefaultDataInstance.ID)
        Else
            mBW.Write(CInt(-1))
        End If
        mBW.Write(node.Comment)

        'C0425===
        If node.Tag IsNot Nothing And (node.Hierarchy.HierarchyType = ECHierarchyType.htAlternative) Then
            'mBW.Write(CSng(node.Tag)) 'C0626
            mBW.Write(node.Tag.ToString) 'C0626
        Else
            'mBW.Write(Single.MinValue) 'C0626
            mBW.Write(UNDEFINED_COST_VALUE) 'C0626
        End If
        'C0425==

        'C0343===
        Select Case node.Hierarchy.HierarchyType
            Case ECHierarchyType.htModel
                If node.AHPNodeData Is Nothing Then
                    mBW.Write(CInt(0))
                Else
                    mBW.Write(node.AHPNodeData.ToStream.ToArray.Length)
                    mBW.Write(node.AHPNodeData.ToStream.ToArray)
                End If
            Case ECHierarchyType.htAlternative
                If node.AHPAltData Is Nothing Then
                    mBW.Write(CInt(0))
                Else
                    mBW.Write(node.AHPAltData.ToStream.ToArray.Length)
                    mBW.Write(node.AHPAltData.ToStream.ToArray)
                End If
            Case Else
                mBW.Write(CInt(0))
        End Select
        'C0343==

        'C0262===
        Dim NodeChunkSize As Integer = mBW.BaseStream.Position - NodeChunkSizePosition
        mBW.Seek(-NodeChunkSize, SeekOrigin.Current)
        mBW.Write(NodeChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        node.SavedToStream = True

        For Each child As clsNode In node.Children
            ProcessNodeChunk(child)
        Next
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_34
    Inherits clsStreamModelWriter_v_1_1_33

    Protected Overrides Function ProcessNodeChunk(ByVal node As clsNode) As Boolean 'C0642
        If node.SavedToStream Then Exit Function

        mBW.Write(CHUNK_HIERARCHY_NODE)

        Dim NodeChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(node.NodeID)
        mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
        mBW.Write(node.NodeName)
        mBW.Write(node.ParentNodeID)

        'A1000 ===
        Dim parentNodesGuidsCount As Integer = 0
        If node.ParentNodesGuids IsNot Nothing Then parentNodesGuidsCount = node.ParentNodesGuids.Count
        mBW.Write(parentNodesGuidsCount)

        If node.ParentNodesGuids IsNot Nothing Then
            For Each NodeGuid As Guid In node.ParentNodesGuids
                mBW.Write(NodeGuid.ToByteArray)
            Next
        End If
        'A1000 ==

        mBW.Write(CInt(node.MeasureType))
        mBW.Write(node.MeasurementScaleID)

        mBW.Write(CInt(node.FeedbackMeasureType))
        mBW.Write(node.FeedbackMeasurementScaleID)

        mBW.Write(CInt(node.MeasureMode))
        mBW.Write(node.Enabled)
        If node.DefaultDataInstance IsNot Nothing Then
            mBW.Write(node.DefaultDataInstance.ID)
        Else
            mBW.Write(CInt(-1))
        End If
        mBW.Write(node.Comment)

        'C0425===
        If node.Tag IsNot Nothing And (node.Hierarchy.HierarchyType = ECHierarchyType.htAlternative) Then
            'mBW.Write(CSng(node.Tag)) 'C0626
            mBW.Write(node.Tag.ToString) 'C0626
        Else
            'mBW.Write(Single.MinValue) 'C0626
            mBW.Write(UNDEFINED_COST_VALUE) 'C0626
        End If
        'C0425==

        'C0343===
        Select Case node.Hierarchy.HierarchyType
            Case ECHierarchyType.htModel
                If node.AHPNodeData Is Nothing Then
                    mBW.Write(CInt(0))
                Else
                    mBW.Write(node.AHPNodeData.ToStream.ToArray.Length)
                    mBW.Write(node.AHPNodeData.ToStream.ToArray)
                End If
            Case ECHierarchyType.htAlternative
                If node.AHPAltData Is Nothing Then
                    mBW.Write(CInt(0))
                Else
                    mBW.Write(node.AHPAltData.ToStream.ToArray.Length)
                    mBW.Write(node.AHPAltData.ToStream.ToArray)
                End If
            Case Else
                mBW.Write(CInt(0))
        End Select
        'C0343==

        'C0262===
        Dim NodeChunkSize As Integer = mBW.BaseStream.Position - NodeChunkSizePosition
        mBW.Seek(-NodeChunkSize, SeekOrigin.Current)
        mBW.Write(NodeChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        node.SavedToStream = True

        For Each child As clsNode In node.Children
            ProcessNodeChunk(child)
        Next
    End Function

    Public Overrides Function WriteUserJudgments(ByVal User As clsUser, ByVal JudgmentsTime As DateTime) As Boolean 'C0369
        If User IsNot Nothing Then
            'Debug.Print("WriteUserJudgments (streams)" + "(UserEmail: " + User.UserEMail + " | UserName: " + User.UserName + " | UserID: " + User.UserID.ToString)
        End If

        mBW = New BinaryWriter(mStream)

        Dim bHasAtLeastOneJudgment = False 'C0412

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Write(CInt((ProjectManager.Hierarchies.Count + ProjectManager.MeasureHierarchies.Count) * ProjectManager.AltsHierarchies.Count))

        Dim Hierarchies As New List(Of clsHierarchy)
        For Each H As clsHierarchy In ProjectManager.Hierarchies
            Hierarchies.Add(H)
        Next
        For Each H As clsHierarchy In ProjectManager.MeasureHierarchies
            Hierarchies.Add(H)
        Next

        For Each H As clsHierarchy In Hierarchies
            For Each AH As clsHierarchy In ProjectManager.AltsHierarchies
                mBW.Write(CHUNK_HIERARCHY_JUDGMENTS)

                Dim HJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                mBW.Write(H.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(AH.HierarchyGuidID.ToByteArray) 'C0261
                mBW.Write(H.Nodes.Count)

                ''Debug.Print("Show corrupted nodes from ahp file: ")
                For Each node As clsNode In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        ''Debug.Print(node.NodeID.ToString + ": " + node.NodeName + " (" + node.MeasureType.ToString + ")")
                    End If
                Next

                For Each node As clsNode In H.Nodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

                    mBW.Write(node.NodeGuidID.ToByteArray) 'C0261

                    'C0283===
                    mBW.Write(node.MeasureType)
                    If node.MeasurementScale IsNot Nothing Then
                        mBW.Write(node.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If
                    'C0283==

                    Dim count As Integer = 0

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                            For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                    count += 1
                                End If
                            Next
                        Else
                            For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                For Each J As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, node.NodeID)
                                    If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                        count += 1
                                    End If
                                Next
                            Next
                        End If
                    End If

                    mBW.Write(count)

                    'C0412===
                    If count > 0 Then
                        bHasAtLeastOneJudgment = True
                    End If
                    'C0412==

                    'If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Then 'C0342 'C0648
                    If Not (((node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing)) And (User.UserID >= 0)) Or
                        ((User.UserID >= 0) AndAlso (node.DefaultDataInstance IsNot Nothing) AndAlso (node.DefaultDataInstance.User IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser IsNot Nothing) AndAlso (node.DefaultDataInstance.EvaluatorUser.UserID = User.UserID)) Then 'C0648

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                        mBW.Write(node.NodeGuidID.ToByteArray)
                                        mBW.Write(pwData.IsUndefined)
                                        mBW.Write(node.Hierarchy.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(AH.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.FirstNodeID).NodeGuidID.ToByteArray)
                                                mBW.Write(node.Hierarchy.GetNodeByID(pwData.SecondNodeID).NodeGuidID.ToByteArray)
                                            End If
                                        Else
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)
                                        End If

                                        mBW.Write(pwData.Advantage)
                                        mBW.Write(pwData.Value)
                                        mBW.Write(pwData.Comment)

                                        If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                            pwData.ModifyDate = JudgmentsTime
                                        End If

                                        mBW.Write(pwData.ModifyDate.ToBinary)
                                    End If
                                Next
                            Case ECMeasureType.mtPWOutcomes
                                For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                                    For Each pwData As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, node.NodeID)
                                        If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                            mBW.Write(node.NodeGuidID.ToByteArray)
                                            mBW.Write(pwData.IsUndefined)
                                            If node.IsTerminalNode Then
                                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            Else
                                                mBW.Write(H.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)
                                            End If
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                            mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                            mBW.Write(pwData.Advantage)
                                            mBW.Write(pwData.Value)
                                            mBW.Write(pwData.Comment)

                                            If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                                pwData.ModifyDate = JudgmentsTime
                                            End If

                                            mBW.Write(pwData.ModifyDate.ToBinary)
                                        End If
                                    Next
                                Next
                            Case Else
                                For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(User.UserID)
                                    If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then 'C0272
                                        mBW.Write(node.NodeGuidID.ToByteArray)
                                        mBW.Write(nonpwData.IsUndefined)
                                        If node.IsTerminalNode Then
                                            mBW.Write(AH.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        Else
                                            mBW.Write(node.Hierarchy.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                        End If

                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                'mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray) 'C0261 'C0396
                                                'C0396===
                                                ' write a flag to determine whether we have a rating of direct value
                                                If nonpwData.IsUndefined Then
                                                    mBW.Write(True)
                                                    mBW.Write(CSng(-1))
                                                Else
                                                    mBW.Write(CBool(CType(nonpwData, clsRatingMeasureData).Rating.ID = -1)) 'C0397
                                                    If CType(nonpwData, clsRatingMeasureData).Rating.ID <> -1 Then
                                                        mBW.Write(CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CType(nonpwData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                                    Else
                                                        mBW.Write(CType(nonpwData, clsRatingMeasureData).Rating.Value)
                                                    End If
                                                End If
                                                'C0396==
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                            Case ECMeasureType.mtStep
                                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                            Case ECMeasureType.mtDirect
                                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                        End Select
                                        mBW.Write(nonpwData.Comment)
                                        'C0369===
                                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                            nonpwData.ModifyDate = JudgmentsTime
                                        End If
                                        'C0369==
                                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                                    End If
                                Next
                        End Select
                    End If

                    'C0262===
                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                    'C0262==
                Next

                Dim UncontributedAlts As List(Of clsNode) = H.GetUncontributedAlternatives

                If UncontributedAlts.Count = 0 Then
                    For Each alt As clsNode In AH.TerminalNodes
                        If alt.DirectJudgmentsForNoCause IsNot Nothing AndAlso alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID).Count > 0 Then  ' D6768
                            UncontributedAlts.Add(alt)
                        End If
                    Next
                End If

                Dim NoContributionCount As Integer = UncontributedAlts.Count

                mBW.Write(NoContributionCount)

                If NoContributionCount > 0 Then
                    bHasAtLeastOneJudgment = True
                End If

                For Each alt As clsNode In UncontributedAlts
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(alt.NodeGuidID.ToByteArray)
                    mBW.Write(alt.MeasureType)
                    If alt.MeasurementScale IsNot Nothing Then
                        mBW.Write(alt.MeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If

                    Dim jCount As Integer = 0

                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                        For Each J As clsPairwiseMeasureData In alt.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, alt.NodeID)
                            If Not J.IsUndefined OrElse (J.Comment <> "") Then
                                jCount += 1
                            End If
                        Next
                    Else
                        For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                            If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                                jCount += 1
                            End If
                        Next
                    End If

                    mBW.Write(jCount)

                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                        For Each pwData As clsPairwiseMeasureData In alt.PWOutcomesJudgments.JudgmentsFromUser(User.UserID, alt.NodeID)
                            If Not pwData.IsUndefined OrElse (pwData.Comment <> "") Then
                                mBW.Write(alt.NodeGuidID.ToByteArray)
                                mBW.Write(pwData.IsUndefined)
                                mBW.Write(AH.GetNodeByID(pwData.ParentNodeID).NodeGuidID.ToByteArray)

                                mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID).GuidID.ToByteArray)
                                mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID).GuidID.ToByteArray)

                                mBW.Write(pwData.Advantage)
                                mBW.Write(pwData.Value)
                                mBW.Write(pwData.Comment)

                                If (pwData.ModifyDate = VERY_OLD_DATE) Or (pwData.ModifyDate = UNDEFINED_DATE) Then
                                    pwData.ModifyDate = JudgmentsTime
                                End If

                                mBW.Write(pwData.ModifyDate.ToBinary)
                            End If
                        Next
                    Else
                        For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(User.UserID)
                            If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                                mBW.Write(nonpwData.IsUndefined)

                                Select Case alt.MeasureType
                                    Case ECMeasureType.mtRatings
                                        Dim RatingsData As clsRatingMeasureData = CType(nonpwData, clsRatingMeasureData)
                                        If RatingsData.IsUndefined Then
                                            mBW.Write(True)
                                            mBW.Write(CSng(-1))
                                        Else
                                            mBW.Write(CBool(RatingsData.Rating.ID = -1))
                                            If RatingsData.Rating.ID <> -1 Then
                                                mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(CType(RatingsData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                            Else
                                                mBW.Write(RatingsData.Rating.Value)
                                            End If
                                        End If
                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                        mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                    Case ECMeasureType.mtStep
                                        mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                    Case ECMeasureType.mtDirect
                                        mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                End Select

                                mBW.Write(nonpwData.Comment)
                                If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                    nonpwData.ModifyDate = JudgmentsTime
                                End If
                                mBW.Write(nonpwData.ModifyDate.ToBinary)
                            End If
                        Next
                    End If

                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                Next

                ' feedback judgments
                Dim fjCount As Integer = 0
                For Each alt As clsNode In AH.TerminalNodes
                    For Each J As clsCustomMeasureData In alt.FeedbackJudgments.JudgmentsFromUser(User.UserID)
                        If Not J.IsUndefined OrElse (J.Comment <> "") Then fjCount += 1
                    Next
                Next

                If fjCount > 0 Then
                    bHasAtLeastOneJudgment = True
                End If

                mBW.Write(AH.TerminalNodes.Count)

                For Each alt As clsNode In AH.TerminalNodes
                    mBW.Write(CHUNK_NODE_JUDGMENTS)

                    Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
                    mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                    mBW.Write(alt.NodeGuidID.ToByteArray)
                    mBW.Write(alt.FeedbackMeasureType)
                    If alt.FeedbackMeasurementScale IsNot Nothing Then
                        mBW.Write(alt.FeedbackMeasurementScale.GuidID.ToByteArray)
                    Else
                        mBW.Write(Guid.NewGuid.ToByteArray)
                    End If

                    Dim jCount As Integer = 0

                    For Each J As clsCustomMeasureData In alt.FeedbackJudgments.JudgmentsFromUser(User.UserID)
                        If Not J.IsUndefined OrElse (J.Comment <> "") Then jCount += 1
                    Next

                    mBW.Write(jCount)

                    If Not IsPWMeasurementType(alt.FeedbackMeasureType) Then
                        For Each nonpwData As clsNonPairwiseMeasureData In CType(alt.FeedbackJudgments, clsNonPairwiseJudgments).JudgmentsFromUser(User.UserID)
                            If Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                                mBW.Write(H.GetNodeByID(nonpwData.NodeID).NodeGuidID.ToByteArray)
                                mBW.Write(nonpwData.IsUndefined)

                                Select Case alt.FeedbackMeasureType
                                    Case ECMeasureType.mtRatings
                                        Dim RatingsData As clsRatingMeasureData = CType(nonpwData, clsRatingMeasureData)
                                        If RatingsData.IsUndefined Then
                                            mBW.Write(True)
                                            mBW.Write(CSng(-1))
                                        Else
                                            mBW.Write(CBool(RatingsData.Rating.ID = -1))
                                            If RatingsData.Rating.ID <> -1 Then
                                                mBW.Write(CType(alt.FeedbackMeasurementScale, clsRatingScale).GetRatingByID(CType(RatingsData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                            Else
                                                mBW.Write(RatingsData.Rating.Value)
                                            End If
                                        End If
                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                        mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                                    Case ECMeasureType.mtStep
                                        mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                                    Case ECMeasureType.mtDirect
                                        mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                                End Select

                                mBW.Write(nonpwData.Comment)
                                If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                                    nonpwData.ModifyDate = JudgmentsTime
                                End If
                                mBW.Write(nonpwData.ModifyDate.ToBinary)
                            End If
                        Next
                    End If

                    Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                    mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                    mBW.Write(NodeJudgmentsChunkSize)
                    mBW.Seek(0, SeekOrigin.End)
                Next

                'C0262===
                Dim HJudgmentsChunkSize As Integer = mBW.BaseStream.Position - HJudgmentsChunkSizePosition
                mBW.Seek(-HJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(HJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
                'C0262==
            Next
        Next

        If True Then
            ' write events link judgments
            mBW.Write(ProjectManager.ActiveAlternatives.TerminalNodes.Count)
            For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                mBW.Write(CHUNK_NODE_JUDGMENTS)

                Dim NodeJudgmentsChunkSizePosition As Integer = mBW.BaseStream.Position
                mBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

                mBW.Write(alt.NodeGuidID.ToByteArray)

                Dim jCount As Integer = 0
                For Each nonpwData As clsNonPairwiseMeasureData In alt.EventsJudgments.JudgmentsFromUser(User.UserID)
                    Dim child As clsNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)

                    If child IsNot Nothing AndAlso Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                        jCount += 1
                    End If
                Next
                mBW.Write(jCount)
                For Each nonpwData As clsNonPairwiseMeasureData In alt.EventsJudgments.JudgmentsFromUser(User.UserID)
                    Dim child As clsNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)
                    If child IsNot Nothing AndAlso Not nonpwData.IsUndefined OrElse (nonpwData.Comment <> "") Then
                        mBW.Write(nonpwData.IsUndefined)
                        mBW.Write(child.NodeGuidID.ToByteArray)

                        Dim mt As ECMeasureType = ECMeasureType.mtDirect
                        Select Case mt
                            Case ECMeasureType.mtRatings
                                Dim RatingsData As clsRatingMeasureData = CType(nonpwData, clsRatingMeasureData)
                                If RatingsData.IsUndefined Then
                                    mBW.Write(True)
                                    mBW.Write(CSng(-1))
                                Else
                                    mBW.Write(CBool(RatingsData.Rating.ID = -1))
                                    If RatingsData.Rating.ID <> -1 Then
                                        mBW.Write(CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(CType(RatingsData, clsRatingMeasureData).Rating.ID).GuidID.ToByteArray)
                                    Else
                                        mBW.Write(RatingsData.Rating.Value)
                                    End If
                                End If
                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                mBW.Write(CType(nonpwData, clsUtilityCurveMeasureData).Data)
                            Case ECMeasureType.mtStep
                                mBW.Write(CType(nonpwData, clsStepMeasureData).Value)
                            Case ECMeasureType.mtDirect
                                mBW.Write(CType(nonpwData, clsDirectMeasureData).DirectData)
                        End Select

                        mBW.Write(nonpwData.Comment)
                        If (nonpwData.ModifyDate = VERY_OLD_DATE) Or (nonpwData.ModifyDate = UNDEFINED_DATE) Then
                            nonpwData.ModifyDate = JudgmentsTime
                        End If
                        mBW.Write(nonpwData.ModifyDate.ToBinary)
                    End If
                Next

                Dim NodeJudgmentsChunkSize As Integer = mBW.BaseStream.Position - NodeJudgmentsChunkSizePosition
                mBW.Seek(-NodeJudgmentsChunkSize, SeekOrigin.Current)
                mBW.Write(NodeJudgmentsChunkSize)
                mBW.Seek(0, SeekOrigin.End)
            Next
        End If

        If mWriteStatus Then
            'Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBW.Close()
        'Return True 'C0412
        Return bHasAtLeastOneJudgment 'C0412
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_43
    Inherits clsStreamModelWriter_v_1_1_34

    Public Overrides Function WriteDataMapping() As Boolean
        mBW = New BinaryWriter(mStream)

        If mWriteStatus Then
            'Debug.Print("WriteDataMapping started " + Now.ToString)
        End If

        mBW.Write(ProjectManager.DataMappings.Count)

        For Each dm As clsDataMapping In ProjectManager.DataMappings
            mBW.Write(dm.DataMappingGUID.ToByteArray)
            mBW.Write(dm.eccMappedColID.ToByteArray)
            mBW.Write(CInt(dm.eccMappedColType))
            mBW.Write(dm.externalColName)
            mBW.Write(dm.externalDBconnString)
            mBW.Write(dm.externalDBname)
            mBW.Write(CInt(dm.externalDBType))
            mBW.Write(dm.externalMapkeyColName)
            mBW.Write(dm.externalTblName)
        Next

        If mWriteStatus Then
            'Debug.Print("WriteDataMapping completed " + Now.ToString)
        End If
        mBW.Close()
    End Function
End Class

<Serializable()> Public Class clsStreamModelWriter_v_1_1_45
    Inherits clsStreamModelWriter_v_1_1_43
    Public Overrides Function WriteEventsGroups() As Boolean
        mBW = New BinaryWriter(mStream)

        mBW.Write(ProjectManager.EventsGroups.Groups.Count)

        For Each group As EventsGroup In ProjectManager.EventsGroups.Groups
            mBW.Write(group.ID.ToByteArray)
            mBW.Write(group.Name)
            mBW.Write(group.Events.Count)
            For Each eData As EventGroupData In group.Events
                mBW.Write(eData.EventGuidID.ToByteArray)
                mBW.Write(eData.EventIntID)
                mBW.Write(eData.Precedence)
            Next
        Next

        mBW.Close()
    End Function
End Class


<Serializable()> Public Class clsStreamModelWriter_v_1_1_46
    Inherits clsStreamModelWriter_v_1_1_45

    Protected Overrides Function ProcessNodeChunk(ByVal node As clsNode) As Boolean 'C0642
        If node.SavedToStream Then Exit Function

        mBW.Write(CHUNK_HIERARCHY_NODE)

        Dim NodeChunkSizePosition As Integer = mBW.BaseStream.Position 'C0262
        mBW.Write(DUMMY_SIZE_OF_THE_CHUNK) 'C0262

        mBW.Write(node.NodeID)
        mBW.Write(node.NodeGuidID.ToByteArray) 'C0261
        mBW.Write(node.NodeName)
        mBW.Write(node.ParentNodeID)

        'A1000 ===
        Dim parentNodesGuidsCount As Integer = 0
        If node.ParentNodesGuids IsNot Nothing Then parentNodesGuidsCount = node.ParentNodesGuids.Count
        mBW.Write(parentNodesGuidsCount)

        If node.ParentNodesGuids IsNot Nothing Then
            For Each NodeGuid As Guid In node.ParentNodesGuids
                mBW.Write(NodeGuid.ToByteArray)
            Next
        End If
        'A1000 ==

        mBW.Write(CInt(node.MeasureType))
        mBW.Write(node.MeasurementScaleID)

        mBW.Write(CInt(node.FeedbackMeasureType))
        mBW.Write(node.FeedbackMeasurementScaleID)

        mBW.Write(CInt(node.MeasureMode))
        mBW.Write(node.Enabled)
        If node.DefaultDataInstance IsNot Nothing Then
            mBW.Write(node.DefaultDataInstance.ID)
        Else
            mBW.Write(CInt(-1))
        End If
        mBW.Write(node.Comment)

        ' new to 1.1.46
        mBW.Write(node.DataMappingGUID.ToByteArray)
        mBW.Write(node.NodeMappedID)
        mBW.Write(CInt(node.EventType))

        'C0425===
        If node.Tag IsNot Nothing And (node.Hierarchy.HierarchyType = ECHierarchyType.htAlternative) Then
            'mBW.Write(CSng(node.Tag)) 'C0626
            mBW.Write(node.Tag.ToString) 'C0626
        Else
            'mBW.Write(Single.MinValue) 'C0626
            mBW.Write(UNDEFINED_COST_VALUE) 'C0626
        End If
        'C0425==

        'C0343===
        Select Case node.Hierarchy.HierarchyType
            Case ECHierarchyType.htModel
                If node.AHPNodeData Is Nothing Then
                    mBW.Write(CInt(0))
                Else
                    mBW.Write(node.AHPNodeData.ToStream.ToArray.Length)
                    mBW.Write(node.AHPNodeData.ToStream.ToArray)
                End If
            Case ECHierarchyType.htAlternative
                If node.AHPAltData Is Nothing Then
                    mBW.Write(CInt(0))
                Else
                    mBW.Write(node.AHPAltData.ToStream.ToArray.Length)
                    mBW.Write(node.AHPAltData.ToStream.ToArray)
                End If
            Case Else
                mBW.Write(CInt(0))
        End Select
        'C0343==

        'C0262===
        Dim NodeChunkSize As Integer = mBW.BaseStream.Position - NodeChunkSizePosition
        mBW.Seek(-NodeChunkSize, SeekOrigin.Current)
        mBW.Write(NodeChunkSize)
        mBW.Seek(0, SeekOrigin.End)
        'C0262==

        node.SavedToStream = True

        For Each child As clsNode In node.Children
            ProcessNodeChunk(child)
        Next
    End Function

    Public Overrides Function WriteEventsGroups() As Boolean
        mBW = New BinaryWriter(mStream)

        Dim groups As List(Of EventsGroup) = ProjectManager.EventsGroups.Groups.Concat(ProjectManager.SourceGroups.Groups).ToList

        mBW.Write(groups.Count)

        For Each group As EventsGroup In groups
            mBW.Write(ProjectManager.EventsGroups.Groups.Contains(group))
            mBW.Write(group.ID.ToByteArray)
            mBW.Write(group.Name)
            mBW.Write(group.Enabled)
            mBW.Write(group.Behaviour)
            mBW.Write(group.Rule)
            mBW.Write(group.RuleParameterValue)
            mBW.Write(group.Events.Count)
            For Each eData As EventGroupData In group.Events
                mBW.Write(eData.EventGuidID.ToByteArray)
                mBW.Write(eData.EventIntID)
                mBW.Write(eData.Precedence)
            Next
        Next

        mBW.Close()
    End Function
End Class
