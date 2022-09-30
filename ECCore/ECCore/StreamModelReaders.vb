Imports ECCore
Imports System.IO
Imports System.Data.Common
Imports ECSecurity.ECSecurity 'C0380
Imports ECCore.MiscFuncs
Imports Canvas
Imports System.ComponentModel

<Serializable()> Public MustInherit Class clsStreamModelReader 'C0345
    <NonSerialized()> Protected mStream As Stream     ' D0360
    <NonSerialized()> Protected mBR As BinaryReader   ' D0360

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
    Protected MustOverride Function ProcessRatingScalesChunk(ByRef RatingScalesCount As Integer) As Boolean 'C0271
    Protected MustOverride Function ProcessRegularUtilityCurvesChunk(ByRef RegularUtilityCurvesCount As Integer) As Boolean 'C0271
    Protected MustOverride Function ProcessAdvancedUtilityCurvesChunk(ByRef AdvancedUtilityCurvesCount As Integer) As Boolean 'C0271
    Protected MustOverride Function ProcessStepFunctionsChunk(ByRef StepFunctionsCount As Integer) As Boolean 'C0271
    Protected MustOverride Function ProcessMeasurementScalesChunk() As Boolean
    Protected MustOverride Function ProcessHierarchiesChunk() As Boolean
    Protected MustOverride Function ProcessAlternativeContributionChunk() As Boolean
    Protected MustOverride Function ProcessChunk(ByVal chunk As Int32) As Boolean
    Protected MustOverride Function ProcessHierarchyDisabledNodes(ByVal User As clsUser) As Boolean 'C0330
    Protected MustOverride Function ProcessNodesPermissions(ByVal User As clsUser) As Boolean
    Protected MustOverride Function ProcessControlsPermissions(ByVal UserID As Integer) As Boolean
    Protected MustOverride Function ProcessAlternativesPermissions(ByVal User As clsUser) As Boolean
    Protected MustOverride Function ProcessNodesPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
    Protected MustOverride Function ProcessAlternativesPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
    Protected MustOverride Function ProcessNodesPermissions(ByVal UserID As Integer) As Boolean
    Protected MustOverride Function ProcessAlternativesPermissions(ByVal userid As Integer) As Boolean

    Public MustOverride Function AddJudgmentsToCombined(ByVal User As clsUser, Group As clsCombinedGroup, Optional Hierarchy As clsHierarchy = Nothing) As Integer

    Public MustOverride Function ReadModelStructure() As Boolean

    Public MustOverride Function ReadDataMapping() As Boolean
    Public MustOverride Function ReadEventsGroups() As Boolean

    Public MustOverride Function GetUsersList() As List(Of clsUser)
    'Public MustOverride Function ReadUserJudgments(ByVal User As clsUser) As Boolean 'C0765
    Public MustOverride Function ReadUserJudgments(ByVal User As clsUser) As Integer 'C0765
    Public MustOverride Function ReadUserJudgmentsControls(ByVal User As clsUser) As Integer
    Public MustOverride Function ReadUserPermissions(ByVal User As clsUser) As Boolean
    Public MustOverride Function ReadControlsPermissions(UserID As Integer) As Boolean
    Public MustOverride Function ReadGroupPermissions(ByVal Group As clsCombinedGroup) As Boolean 'C1030
    Public MustOverride Function ReadUserDisabledNodes(ByVal User As clsUser) As Boolean 'C0330
    Public MustOverride Function ReadInfoDocs() As Boolean 'C0276
    Public MustOverride Function ReadAdvancedInfoDocs() As Boolean 'C0920
    Public MustOverride Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
    Public MustOverride Function JudgmentsExists(User As clsUser, Optional HierarchyID As Integer = -1) As Boolean
    Public Function GetNodeIndexByID(ByVal NodesList As List(Of clsNode), ByVal NodeID As Guid) As Integer
        If NodesList Is Nothing Then Return -1
        Return NodesList.FindIndex(Function(n) (n.NodeGuidID.Equals(NodeID)))
    End Function
End Class


<Serializable()> Public Class clsStreamModelReader_v_1_1_7 'C0259
    Inherits clsStreamModelReader 'C0345

    Public Overrides Function JudgmentsExists(User As clsUser, Optional HierarchyID As Integer = -1) As Boolean
        Return False
    End Function

    Public Overrides Function ReadDataMapping() As Boolean
        Return False
    End Function

    Public Overrides Function ReadEventsGroups() As Boolean
        Return False
    End Function

    Protected Overrides Function ProcessControlsPermissions(ByVal UserID As Integer) As Boolean
        Return False
    End Function

    Public Overrides Function ReadControlsPermissions(UserID As Integer) As Boolean
        Return False
    End Function

    Public Overrides Function ReadUserJudgmentsControls(ByVal User As clsUser) As Integer
        Return -1
    End Function

    Public Overrides Function AddJudgmentsToCombined(ByVal User As clsUser, Group As clsCombinedGroup, Optional Hierarchy As clsHierarchy = Nothing) As Integer
        Return -1
    End Function

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Return -1
    End Function

    Protected Overrides Function ProcessNodesPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
        Return True
    End Function

    Protected Overrides Function ProcessAlternativesPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
        Return True
    End Function

    Protected Overrides Function ProcessNodesPermissions(ByVal UserID As Integer) As Boolean
        Return True
    End Function

    Protected Overrides Function ProcessAlternativesPermissions(ByVal userid As Integer) As Boolean
        Return True
    End Function

    Protected Overrides Function ProcessUsersGroupsChunk() As Boolean
        ' read users groups

        ProjectManager.EvaluationGroups.GroupsList.Clear()
        ProjectManager.CombinedGroups.GroupsList.Clear()

        ' read the number of groups first
        Dim groupsCount As Int32 = mBR.ReadInt32

        Dim i As Integer = 0

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> groupsCount)
            subChunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_USER_GROUP Then

                Dim groupID As Int32 = mBR.ReadInt32
                Dim groupName As String = mBR.ReadString
                Dim groupRule As String = mBR.ReadString
                Dim groupType As Int32 = mBR.ReadInt32
                Dim groupCombinedUserID As Int32 = mBR.ReadInt32

                Select Case CType(groupType, ECGroupType)
                    Case ECGroupType.gtEvaluation
                        Dim evalGroup As clsGroup = New clsGroup
                        evalGroup.ID = groupID
                        evalGroup.Name = groupName
                        evalGroup.Rule = groupRule
                        ProjectManager.EvaluationGroups.AddGroup(evalGroup)

                    Case ECGroupType.gtCombined
                        Dim combinedGroup As clsCombinedGroup = New clsCombinedGroup(ProjectManager)
                        combinedGroup.ID = groupID
                        combinedGroup.Name = groupName
                        combinedGroup.Rule = groupRule
                        combinedGroup.CombinedUserID = groupCombinedUserID
                        ProjectManager.CombinedGroups.AddGroup(combinedGroup)
                End Select
            Else
                Return False
            End If
            i += 1
        End While

        If Not ProjectManager.CombinedGroups.CombinedGroupUserIDExists(COMBINED_USER_ID) Then
            Dim CG As clsCombinedGroup = ProjectManager.CombinedGroups.AddCombinedGroup(DEFAULT_COMBINED_GROUP_NAME)
            CG.CombinedUserID = COMBINED_USER_ID
            ProjectManager.CombinedGroups.AddGroup(CG)
        End If

        'C0672===
        'C0721===
        'If Not ProjectManager.CombinedGroupUserIDExists(COMBINED_GROUP_ALL_USERS_ID) Then
        '    Dim CG As clsCombinedGroup = ProjectManager.CombinedGroups.AddCombinedGroup(ALL_USERS_COMBINED_GROUP_NAME)
        '    CG.CombinedUserID = COMBINED_GROUP_ALL_USERS_ID
        '    ProjectManager.CombinedGroups.AddGroup(CG)
        'End If
        'C0721==
        'C0672==

        'C0721===
        If ProjectManager.CombinedGroups.CombinedGroupUserIDExists(COMBINED_GROUP_ALL_USERS_ID) Then
            ProjectManager.CombinedGroups.DeleteGroup(ProjectManager.CombinedGroups.GetCombinedGroupByUserID(COMBINED_GROUP_ALL_USERS_ID))
        End If
        'C0721==

        If mWriteStatus Then
            Debug.Print("ProcessUsersGroup " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessUsersListChunk() As Boolean
        ' read users list

        ProjectManager.UsersList.Clear()
        ProjectManager.DataInstanceUsers.Clear()

        ProjectManager.UserID = 0

        ' read the number of users first
        Dim usersCount As Int32 = mBR.ReadInt32

        Dim i As Integer = 0

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> usersCount)
            subChunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_USER Then
                Dim user As clsUser = New clsUser
                user.UserID = mBR.ReadInt32
                user.UserGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                user.UserName = mBR.ReadString
                user.UserEMail = mBR.ReadString
                user.Active = mBR.ReadBoolean
                user.VotingBoxID = mBR.ReadInt32
                user.IncludedInSynchronous = mBR.ReadBoolean
                user.DataInstanceID = mBR.ReadInt32
                Dim evalGroupID As Int32 = mBR.ReadInt32
                user.EvaluationGroup = ProjectManager.EvaluationGroups.GetGroupByID(evalGroupID)

                'C0343===
                Dim count As Integer = mBR.ReadInt32
                If count <> 0 Then
                    Dim bytes As Byte() = mBR.ReadBytes(count)
                    Dim stream As New MemoryStream(bytes)
                    user.AHPUserData = New clsAHPUserData
                    user.AHPUserData.FromStream(stream)
                End If
                'C0343==

                If user.UserID >= 0 Then
                    ProjectManager.UsersList.Add(user)
                Else
                    ProjectManager.DataInstanceUsers.Add(user)
                End If
            Else
                Return False
            End If
            i += 1
        End While

        'ProjectManager.AddCombinedUser() 'C0552

        If mWriteStatus Then
            Debug.Print("ProcessUsersList " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessCombinedGroupsUsersChunk() As Boolean
        ' read pairs CombinedGroupID / UserID

        ' read the number of pairs first
        Dim pairsCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim group As clsCombinedGroup
        Dim user As clsUser

        Dim groupID As Int32
        Dim userID As Int32

        'C0663===
        For Each group In ProjectManager.CombinedGroups.GroupsList
            group.UsersList.Clear()
        Next
        'C0663==

        While (mStream.Position < mStream.Length - 1) And (i <> pairsCount)
            groupID = mBR.ReadInt32
            userID = mBR.ReadInt32

            group = ProjectManager.CombinedGroups.GetGroupByID(groupID)
            user = ProjectManager.GetUserByID(userID)

            If (group IsNot Nothing) And (user IsNot Nothing) Then
                'If Not group.ContainsUser(user) Then 'C0589
                'If group.ContainsUser(user) Then 'C0663
                If Not group.ContainsUser(user) Then 'C0663
                    group.UsersList.Add(user)
                End If
            End If

            i += 1
        End While

        For Each group In ProjectManager.CombinedGroups.GroupsList
            group.ApplyRules()
        Next

        If mWriteStatus Then
            Debug.Print("ProcessCombinedGroupsUsers " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessDataInstancesChunk() As Boolean
        ' read data instances definitions

        ProjectManager.DataInstances.Clear()

        ' read the number of data instances first
        Dim diCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim DI As clsDataInstance

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> diCount)
            subChunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_DATA_INSTANCE Then
                DI = New clsDataInstance
                DI.ID = mBR.ReadInt32
                DI.Name = mBR.ReadString
                DI.User = ProjectManager.GetDataInstanceUserByID(mBR.ReadInt32)
                DI.Comment = mBR.ReadString

                'ProjectManager.DataInstances.Add(DI) 'C0952
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessDataInstances " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    'Protected Function ProcessRatingScalesChunk() As Boolean 'C0271
    Protected Overrides Function ProcessRatingScalesChunk(ByRef RatingScalesCount As Integer) As Boolean 'C0271
        ' read rating scales

        ProjectManager.MeasureScales.RatingsScales.Clear()

        ' read the number of rating scales first
        Dim rsCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RS As clsRatingScale

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rsCount)
            subChunk = mBR.ReadInt32
            Dim RSChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_RATING_SCALE Then
                Dim rsID As Int32 = mBR.ReadInt32
                RS = New clsRatingScale(rsID)
                RS.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                RS.Name = mBR.ReadString
                RS.Comment = mBR.ReadString

                ProjectManager.MeasureScales.RatingsScales.Add(RS)

                ' read rating scale intensities
                subChunk = mBR.ReadInt32
                Dim RIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_RATING_INTENSITIES Then
                    Dim iCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim id As Int32
                    Dim GuidID As Guid 'C0261
                    Dim name As String
                    Dim value As Single
                    Dim comment As String

                    Dim R As clsRating

                    While (mStream.Position < mStream.Length - 1) And (j <> iCount)
                        subChunk = mBR.ReadInt32
                        Dim RIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_RATING_INTENSITY Then
                            id = mBR.ReadInt32
                            GuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            name = mBR.ReadString
                            value = mBR.ReadSingle
                            comment = mBR.ReadString

                            If value < 0 Then
                                value = 0
                            End If

                            If value > 1 Then
                                value = 1
                            End If

                            R = New clsRating(id, name, value, RS, comment)
                            R.GuidID = GuidID 'C0261

                            RS.RatingSet.Add(R)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRatingScales " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RatingScalesCount = i 'C0271
        Return True
    End Function

    'Protected Function ProcessRegularUtilityCurvesChunk() As Boolean 'C0271
    Protected Overrides Function ProcessRegularUtilityCurvesChunk(ByRef RegularUtilityCurvesCount As Integer) As Boolean 'C0271
        ' read regular utility curves definitions

        ProjectManager.MeasureScales.RegularUtilityCurves.Clear()

        ' read the number of regular utility curves first
        Dim rucCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RUC As clsRegularUtilityCurve

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rucCount)
            subChunk = mBR.ReadInt32
            Dim RUCChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_REGULAR_UTILITY_CURVE Then
                Dim id As Int32 = mBR.ReadInt32
                Dim guidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0283
                Dim name As String = mBR.ReadString
                Dim low As Single = mBR.ReadSingle
                Dim high As Single = mBR.ReadSingle
                Dim curvature As Single = mBR.ReadSingle
                Dim isIncreasing As Boolean = mBR.ReadBoolean
                Dim comment As String = mBR.ReadString

                RUC = New clsRegularUtilityCurve(id, low, high, curvature, curvature = 0, isIncreasing)

                RUC.GuidID = guidID 'C0283
                RUC.Name = name
                RUC.Comment = comment

                ProjectManager.MeasureScales.RegularUtilityCurves.Add(RUC)
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRegularUtilityCurves " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RegularUtilityCurvesCount = i 'C0271
        Return True
    End Function

    'Protected Function ProcessAdvancedUtilityCurvesChunk() As Boolean 'C0271
    Protected Overrides Function ProcessAdvancedUtilityCurvesChunk(ByRef AdvancedUtilityCurvesCount As Integer) As Boolean 'C0271
        ' read advanced utility curves definitions

        ProjectManager.MeasureScales.AdvancedUtilityCurves.Clear()

        ' read the number of advanced utility curves first
        Dim aucCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim AUC As clsAdvancedUtilityCurve

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> aucCount)
            subChunk = mBR.ReadInt32
            Dim AUCChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_ADVANCED_UTILITY_CURVE Then
                Dim id As Int32 = mBR.ReadInt32
                AUC = New clsAdvancedUtilityCurve(id)
                AUC.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                AUC.Name = mBR.ReadString
                AUC.InterpolationMethod = CType(mBR.ReadInt32, ECInterpolationMethod)
                AUC.Comment = mBR.ReadString
                ProjectManager.MeasureScales.AdvancedUtilityCurves.Add(AUC)

                AUC.Points.Clear()

                ' read advanced utility curve points
                subChunk = mBR.ReadInt32
                Dim AUCPointsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_AUC_POINTS Then
                    Dim pointsCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> pointsCount)
                        Dim x As Single = mBR.ReadSingle
                        Dim y As Single = mBR.ReadSingle
                        AUC.AddUCPoint(x, y)
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessAdvancedUtilityCurves " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        AdvancedUtilityCurvesCount = i 'C0271
        Return True
    End Function

    'Protected Function ProcessStepFunctionsChunk() As Boolean 'C0271
    Protected Overrides Function ProcessStepFunctionsChunk(ByRef StepFunctionsCount As Integer) As Boolean 'C0271
        ' read step functions

        ProjectManager.MeasureScales.StepFunctions.Clear()

        ' read the number of step functions first
        Dim sfCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim SF As clsStepFunction

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> sfCount)
            subChunk = mBR.ReadInt32
            Dim SFChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_STEP_FUNCTION Then
                Dim sfID As Int32 = mBR.ReadInt32
                SF = New clsStepFunction(sfID)
                SF.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                SF.Name = mBR.ReadString
                SF.IsPiecewiseLinear = mBR.ReadBoolean 'C0329
                SF.Comment = mBR.ReadString

                ProjectManager.MeasureScales.StepFunctions.Add(SF)

                subChunk = mBR.ReadInt32
                Dim SIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_STEP_INTERVALS Then
                    Dim siCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    Dim SI As clsStepInterval
                    While (mStream.Position < mStream.Length - 1) And (j <> siCount)
                        subChunk = mBR.ReadInt32
                        Dim SIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_STEP_INTERVAL Then
                            Dim id As Int32 = mBR.ReadInt32
                            Dim name As String = mBR.ReadString
                            Dim low As Single = mBR.ReadSingle
                            Dim high As Single = mBR.ReadSingle
                            Dim value As Single = mBR.ReadSingle
                            Dim comment As String = mBR.ReadString
                            SI = New clsStepInterval(id, name, low, high, value, SF, comment)
                            SF.Intervals.Add(SI)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each SF In ProjectManager.MeasureScales.StepFunctions
            'SF.SortByInterval()
            Dim tmpVal As Single = SF.GetValue(0) ' D6333 // trick with sort by interval and fix "high" values since some scales always have high=0 that cause issues with a render scales
        Next

        If mWriteStatus Then
            Debug.Print("ProcessStepFunctions " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        StepFunctionsCount = i 'C0271
        Return True
    End Function

    Protected Overrides Function ProcessMeasurementScalesChunk() As Boolean
        Dim msCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim chunk As Int32

        Dim count As Integer = 0 'C0271

        Dim res As Boolean = True
        While mStream.Position < mStream.Length - 1 And (i <> msCount) And res
            chunk = mBR.ReadInt32()
            Dim MSChunkSize As Integer = mBR.ReadInt32 'C0263

            Select Case chunk
                Case CHUNK_RATINGS_SCALES
                    'res = ProcessRatingScalesChunk() 'C0271
                    res = ProcessRatingScalesChunk(count) 'C0271
                Case CHUNK_REGULAR_UTILITY_CURVES
                    'res = ProcessRegularUtilityCurvesChunk() 'C0271
                    res = ProcessRegularUtilityCurvesChunk(count) 'C0271
                Case CHUNK_ADVANCED_UTILITY_CURVES
                    'res = ProcessAdvancedUtilityCurvesChunk() 'C0271
                    res = ProcessAdvancedUtilityCurvesChunk(count) 'C0271
                Case CHUNK_STEP_FUNCTIONS
                    'res = ProcessStepFunctionsChunk() 'C0271
                    res = ProcessStepFunctionsChunk(count) 'C0271
                Case Else
                    res = False
            End Select

            'i += 1 'C0271
            i += count 'C0271
        End While

        If mWriteStatus Then
            Debug.Print("ProcessMeasurementScales " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return res
    End Function

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean
        ' read hierarchies with nodes

        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            chunk = mBR.ReadInt32
            Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                End Select

                H.Nodes.Clear()
                H.ResetNodesDictionaries()

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32
                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, node.ParentNodeID, True)

                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32
                            'C0952===
                            'If defDI <> UNDEFINED_DATA_INSTANCE_ID Then
                            '    node.DefaultDataInstance = ProjectManager.GetDataInstanceByID(defDI)
                            'End If
                            'C0952==

                            node.Comment = mBR.ReadString

                            'C0343===
                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
        Next

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessAlternativeContributionChunk() As Boolean
        ' read alternatives contributions

        ' read the number of contribution sets HierarchyID / AltHierarchyID
        Dim setsCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32

        While (mStream.Position < mStream.Length - 1) And (j <> setsCount)
            chunk = mBR.ReadInt32
            Dim ACSetChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_ALTERNATIVES_CONTRIBUTION_SET Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim ahID As Int32 = mBR.ReadInt32

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hID)
                Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(ahID)

                ' read the number of pairs covering objective / alternative first
                Dim pairsCount As Int32 = mBR.ReadInt32
                Dim i As Integer = 0

                Dim node As clsNode
                Dim alt As clsNode

                Dim nodeID As Int32
                Dim altID As Int32

                While (mStream.Position < mStream.Length - 1) And (i <> pairsCount)

                    If H IsNot Nothing Then
                        nodeID = mBR.ReadInt32
                        node = H.GetNodeByID(nodeID)
                    Else
                        mBR.ReadInt32()
                        node = Nothing
                    End If

                    If AH IsNot Nothing Then
                        altID = mBR.ReadInt32
                        alt = AH.GetNodeByID(altID)
                    Else
                        mBR.ReadInt32()
                        alt = Nothing
                    End If

                    If (node IsNot Nothing) And (alt IsNot Nothing) Then
                        node.ChildrenAlts.Add(alt.NodeID)
                    End If

                    i += 1
                End While

            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessAlternativesContribution " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessChunk(ByVal chunk As Int32) As Boolean
        Select Case chunk
            Case CHUNK_USERS_GROUPS
                Return ProcessUsersGroupsChunk()
            Case CHUNK_USERS_LIST
                Return ProcessUsersListChunk()
            Case CHUNK_COMBINED_GROUPS_USERS
                Return ProcessCombinedGroupsUsersChunk()
            Case CHUNK_DATA_INSTANCES
                Return ProcessDataInstancesChunk()
            Case CHUNK_MEASUREMENT_SCALES
                ProjectManager.MeasureScales.ClearAllScales()
                Return ProcessMeasurementScalesChunk()
            Case CHUNK_HIERARCHIES
                Return ProcessHierarchiesChunk()
            Case CHUNK_ALTERNATIVES_CONTRIBUTION
                Return ProcessAlternativeContributionChunk()
            Case Else
                Return False
        End Select

        Return True
    End Function

    Public Overrides Function ReadModelStructure() As Boolean
        mBR = New BinaryReader(mStream)

        If mWriteStatus Then
            Debug.Print("ReadModelStructure start " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim chunk As Int32

        Dim res As Boolean = True
        While (mStream.Position < mStream.Length - 1) And res
            chunk = mBR.ReadInt32()
            Dim curPos As Integer = mBR.BaseStream.Position 'C0263

            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            res = ProcessChunk(chunk)

            'Debug.Print("NewPos - oldPos = " + (mBR.BaseStream.Position - curPos).ToString + "; ChunkSize = " + ChunkSize.ToString + " (Read Model Structure)") 'C0263
        End While

        If mWriteStatus Then
            'Debug.Print("ReadModelStructure completed " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        Return res
    End Function

    Public Overrides Function GetUsersList() As List(Of clsUser)
        mBR = New BinaryReader(mStream)

        mBR.BaseStream.Seek(0, SeekOrigin.Begin)

        Dim UsersList As New List(Of clsUser)

        Dim res As Boolean = True
        While (mStream.Position < mStream.Length - 1) And res
            Dim chunk As Int32 = mBR.ReadInt32()
            Dim ChunkSize As Integer = mBR.ReadInt32

            If chunk = CHUNK_USERS_LIST Then
                ' read users list

                ' read the number of users first
                Dim usersCount As Int32 = mBR.ReadInt32

                Dim i As Integer = 0

                Dim subChunk As Int32
                While (mStream.Position < mStream.Length - 1) And (i <> usersCount)
                    subChunk = mBR.ReadInt32
                    Dim chSize As Integer = mBR.ReadInt32

                    If subChunk = CHUNK_USER Then
                        Dim user As clsUser = New clsUser
                        user.UserID = mBR.ReadInt32
                        user.UserGuidID = New Guid(mBR.ReadBytes(16))
                        user.UserName = mBR.ReadString
                        user.UserEMail = mBR.ReadString
                        user.Active = mBR.ReadBoolean
                        user.VotingBoxID = mBR.ReadInt32
                        user.IncludedInSynchronous = mBR.ReadBoolean
                        user.DataInstanceID = mBR.ReadInt32
                        Dim evalGroupID As Int32 = mBR.ReadInt32
                        'user.EvaluationGroup = ProjectManager.EvaluationGroups.GetGroupByID(evalGroupID)

                        'C0356===
                        Dim count As Integer = mBR.ReadInt32
                        If count <> 0 Then
                            Dim bytes As Byte() = mBR.ReadBytes(count)
                            Dim stream As New MemoryStream(bytes)
                            user.AHPUserData = New clsAHPUserData
                            user.AHPUserData.FromStream(stream)
                        End If
                        'C0356==

                        If user.UserID >= 0 Then
                            UsersList.Add(user)
                        End If
                    End If
                    i += 1
                End While

                res = False
            Else
                mBR.BaseStream.Seek(ChunkSize - 4, SeekOrigin.Current)
            End If
        End While

        mBR.Close()
        Return UsersList
    End Function

    'Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Boolean 'C0765
    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer 'C0765
        mBR = New BinaryReader(mStream)

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim res As Integer = 0 'C0765

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim nodesCount As Int32 = mBR.ReadInt32

                Dim k As Integer = 0
                While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                    Dim subChunk As Int32 = mBR.ReadInt32
                    Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                    Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                    If subChunk = CHUNK_NODE_JUDGMENTS Then
                        'Dim NodeID As Int32 = mBR.ReadInt32 'C0261
                        Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                        Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                        Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                        If node IsNot Nothing Then 'C0264

                            'C0283===
                            Dim MT As ECMeasureType = mBR.ReadInt32
                            Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                            Dim validMeasurementTypeAndScale As Boolean
                            If MT <> node.MeasureType Then
                                validMeasurementTypeAndScale = False
                            Else
                                Select Case node.MeasureType
                                    Case ECMeasureType.mtPairwise, ECMeasureType.mtDirect
                                        validMeasurementTypeAndScale = True
                                    Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                        'validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid) 'C0431
                                        'C0431===
                                        If node.MeasurementScale IsNot Nothing Then
                                            validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                        Else
                                            validMeasurementTypeAndScale = False
                                        End If
                                        'C0431==
                                End Select
                            End If
                            'C0283==

                            If validMeasurementTypeAndScale Then 'C0283
                                Dim jCount As Int32 = mBR.ReadInt32
                                Dim i As Integer = 0

                                'Dim childNodeID As Int32 'C0261
                                Dim childNodeGuidID As Guid 'C0261
                                Dim MD As clsCustomMeasureData

                                While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                    MD = Nothing

                                    If node.MeasureType = ECMeasureType.mtPairwise Then
                                        'Dim firstNodeID As Int32 = mBR.ReadInt32 'C0261
                                        'Dim secondNodeID As Int32 = mBR.ReadInt32 'C0261
                                        Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                        Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                        'C0275===
                                        'Dim firstNode As clsNode = node.Hierarchy.GetNodeByID(firstNodeGuidID) 'C0261
                                        'Dim secondNode As clsNode = node.Hierarchy.GetNodeByID(secondNodeGuidID) 'C0261
                                        'C0275==

                                        'C0275===
                                        Dim firstNode As clsNode
                                        Dim secondNode As clsNode

                                        If node.IsTerminalNode Then
                                            firstNode = AH.GetNodeByID(firstNodeGuidID)
                                            secondNode = AH.GetNodeByID(secondNodeGuidID)
                                        Else
                                            firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                            secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                        End If
                                        'C0275===

                                        Dim advantage As Int32 = mBR.ReadInt32
                                        Dim value As Single = mBR.ReadDouble

                                        'MD = New clsPairwiseMeasureData(firstNodeID, secondNodeID, advantage, value, node.NodeID, User.UserID, value = 0) 'C0261

                                        If firstNode IsNot Nothing And secondNode IsNot Nothing Then 'C0264
                                            MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, value = 0) 'C0261
                                        End If
                                    Else
                                        'childNodeID = mBR.ReadInt32 'C0261
                                        childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                        'Dim childNode As clsNode = node.Hierarchy.GetNodeByID(childNodeGuidID) 'C0261 'C0275
                                        'C0275===
                                        Dim childNode As clsNode
                                        If node.IsTerminalNode Then
                                            childNode = AH.GetNodeByID(childNodeGuidID)
                                        Else
                                            childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                        End If
                                        'C0275==

                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                Dim ratingGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                If childNode IsNot Nothing Then 'C0264
                                                    Dim rating As clsRating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                    MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, rating Is Nothing) 'C0261
                                                End If
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                Dim ucDataValue As Single = mBR.ReadSingle
                                                'MD = New clsUtilityCurveMeasureData(childNodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, Single.IsNaN(ucDataValue)) 'C0261
                                                If childNode IsNot Nothing Then 'C0264
                                                    MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, Single.IsNaN(ucDataValue)) 'C0261
                                                End If
                                            Case ECMeasureType.mtStep
                                                Dim value As Single = mBR.ReadSingle
                                                'MD = New clsStepMeasureData(childNodeID, node.NodeID, User.UserID, value, node.MeasurementScale, Single.IsNaN(value)) 'C0261
                                                If childNode IsNot Nothing Then 'C0264
                                                    MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, Single.IsNaN(value)) 'C0261
                                                End If
                                            Case ECMeasureType.mtDirect
                                                Dim directValue As Single = mBR.ReadSingle
                                                'MD = New clsDirectMeasureData(childNodeID, node.NodeID, User.UserID, directValue, Single.IsNaN(directValue)) 'C0261
                                                If childNode IsNot Nothing Then 'C0264
                                                    MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, Single.IsNaN(directValue)) 'C0261
                                                End If
                                        End Select
                                    End If

                                    If MD IsNot Nothing Then
                                        MD.Comment = mBR.ReadString
                                        MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                        'C0369===
                                        If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                            MD.ModifyDate = Now
                                        End If
                                        'C0369==

                                        'node.Judgments.AddMeasureData(MD, True) 'C0264 'C0327
                                        node.Judgments.AddMeasureData(MD) 'C0327

                                        res += 1 'C0765
                                    Else
                                        mBR.ReadString()
                                        mBR.ReadInt64()
                                    End If

                                    'node.Judgments.AddMeasureData(MD, True) 'C0260 'C0264

                                    i += 1
                                End While
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                            End If
                        Else
                            mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                        End If
                    End If
                    k += 1
                End While

            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Protected Overrides Function ProcessNodesPermissions(ByVal User As clsUser) As Boolean
        Dim IsAllowed As Boolean = mBR.ReadBoolean
        'Dim HierarchyID As Int32 = mBR.ReadInt32 'C0261
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

        If mWriteStatus Then
            Debug.Print("ProcessUserPermissions started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        'Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyID) 'C0261
        Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyGuidID) 'C0261

        If H IsNot Nothing Then
            For Each nd As clsNode In H.Nodes
                'nd.UserPermissions(User.UserID).Write = Not IsAllowed 'C0901
            Next
        End If

        Dim nodesCount As Integer = mBR.ReadInt32
        Dim i As Integer = 0

        'Dim NodeID As Int32 'C0261
        Dim NodeGuidID As Guid 'C0261
        Dim node As clsNode

        While (mStream.Position < mStream.Length - 1) And (i <> nodesCount)
            'NodeID = mBR.ReadInt32 'C0261
            NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261

            If H IsNot Nothing Then
                'node = H.GetNodeByID(NodeID) 'C0261
                node = H.GetNodeByID(NodeGuidID) 'C0261
                If node IsNot Nothing Then
                    'node.UserPermissions(User.UserID).Write = IsAllowed 'C0901
                    ProjectManager.UsersRoles.SetObjectivesRoles(User.UserID, node.NodeGuidID, If(IsAllowed, RolesValueType.rvtAllowed, RolesValueType.rvtRestricted)) 'C0901 'C1061
                End If
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserPermissions completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessHierarchyDisabledNodes(ByVal User As clsUser) As Boolean 'C0330
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16))

        If mWriteStatus Then
            Debug.Print("ProcessHierarchyDisabledNodes started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim H As clsHierarchy = ProjectManager.GetAnyHierarchyByID(HierarchyGuidID) 'C0332

        Dim nodesCount As Integer = mBR.ReadInt32
        Dim i As Integer = 0

        Dim NodeGuidID As Guid
        Dim node As clsNode

        While (mStream.Position < mStream.Length - 1) And (i <> nodesCount)
            NodeGuidID = New Guid(mBR.ReadBytes(16))

            If H IsNot Nothing Then
                node = H.GetNodeByID(NodeGuidID)
                If node IsNot Nothing Then
                    node.DisabledForUser(User.UserID) = True
                End If
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessHierarchyDisabledNodes completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessAlternativesPermissions(ByVal User As clsUser) As Boolean
        'Dim HierarchyID As Integer = mBR.ReadInt32 'C0261
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
        'Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyID) 'C0261
        Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyGuidID) 'C0261

        'Dim AltsHierarchyID As Integer = mBR.ReadInt32
        Dim AltsHierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
        'Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyID)
        Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyGuidID) 'C0261

        If H IsNot Nothing And AH IsNot Nothing Then
            Dim coCount As Int32 = mBR.ReadInt32 ' covering objectives count
            Dim i As Integer = 0

            'Dim NodeID As Integer 'C0261
            Dim NodeGuidID As Guid 'C0261
            Dim node As clsNode

            Dim IsAllowed As Boolean

            Dim chunk As Int32
            While (mStream.Position < mStream.Length - 1) And (i <> coCount)
                chunk = mBR.ReadInt32
                Dim NodeAltsChunkSize As Integer = mBR.ReadInt32 'C0263

                If chunk = CHUNK_NODE_ALTERNATIVES_PERMISSIONS Then
                    'NodeID = mBR.ReadInt32 'C0261
                    NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                    IsAllowed = mBR.ReadBoolean

                    'node = H.GetNodeByID(NodeID) 'C0261
                    node = H.GetNodeByID(NodeGuidID) 'C0261

                    'C0901===
                    'If node IsNot Nothing Then
                    '    CType(node.UserPermissions(User.UserID).SpecialPermissions, clsNodePermissions).SetAllPermissions(Not IsAllowed)

                    '    If node.TmpNodesBelow Is Nothing Then
                    '        'node.TmpNodesBelow = node.GetNodesBelow.Clone 'C0385
                    '        'node.TmpNodesBelow = node.GetNodesBelow 'C0385 'C0450
                    '        node.TmpNodesBelow = node.GetNodesBelow(UNDEFINED_USER_ID) 'C0450
                    '    End If
                    'End If
                    'C0901==

                    Dim altCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    'Dim altID As Int32 'C0261
                    Dim altGuidID As Guid 'C0261
                    Dim alt As clsNode

                    While (mStream.Position < mStream.Length - 1) And (j <> altCount)
                        'altID = mBR.ReadInt32 'C0261
                        altGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                        'alt = AH.GetNodeByID(altID) 'C0261
                        alt = AH.GetNodeByID(altGuidID) 'C0261

                        If alt IsNot Nothing And node IsNot Nothing Then
                            'C0901===
                            'Dim UP As clsPermissions = node.UserPermissions(User.UserID)

                            'If IsAllowed Then
                            '    CType(UP.SpecialPermissions, clsNodePermissions).AddAllowedNodeBelow(alt, node.TmpNodesBelow)
                            'Else
                            '    CType(UP.SpecialPermissions, clsNodePermissions).AddRestrictedNodeBelow(alt, node.TmpNodesBelow)
                            'End If
                            'C0901==

                            ProjectManager.UsersRoles.SetAlternativesRoles(User.UserID, node.NodeGuidID, alt.NodeGuidID, If(IsAllowed, RolesValueType.rvtAllowed, RolesValueType.rvtRestricted)) 'C0901 'C1061
                        End If

                        j += 1
                    End While

                    If (node IsNot Nothing) AndAlso (node.TmpNodesBelow IsNot Nothing) Then
                        node.TmpNodesBelow = Nothing
                    End If

                End If
                i += 1
            End While
        End If

        Return True
    End Function

    Public Overrides Function ReadUserPermissions(ByVal User As clsUser) As Boolean
        If User IsNot Nothing AndAlso User.UserID <> DUMMY_PERMISSIONS_USER_ID Then
            ProjectManager.UsersRoles.CleanUpUserRoles(User.UserID)
        End If

        mBR = New BinaryReader(mStream)

        Dim chunk As Int32
        Dim res As Boolean = True

        While (mStream.Position < mStream.Length - 1) And res
            chunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            Dim curPos As Integer = mBR.BaseStream.Position 'C0263

            Select Case chunk
                Case CHUNK_NODES_PERMISSIONS
                    res = ProcessNodesPermissions(User)
                Case CHUNK_ALTERNATIVES_PERMISSIONS
                    res = ProcessAlternativesPermissions(User)
                    'C0700===
                    'For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes
                    '    node.ClearCalculatedNodesBelow(User.UserID)
                    '    CType(node.UserPermissions(User.UserID).SpecialPermissions, clsNodePermissions).VerifyPermissions(True)
                    'Next
                    'C0700==
            End Select


            'Debug.Print("NewPos - oldPos = " + (mBR.BaseStream.Position - curPos).ToString + "; ChunkSize = " + ChunkSize.ToString + " (Read User Permissions) " + chunk.ToString + " UserID: " + User.UserID.ToString) 'C0263
        End While

        mBR.Close()
        Return True
    End Function

    Public Overrides Function ReadGroupPermissions(ByVal Group As clsCombinedGroup) As Boolean
        mBR = New BinaryReader(mStream)

        Dim chunk As Int32
        Dim res As Boolean = True

        While (mStream.Position < mStream.Length - 1) And res
            chunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32

            Dim curPos As Integer = mBR.BaseStream.Position

            Select Case chunk
                Case CHUNK_NODES_PERMISSIONS
                    res = ProcessNodesPermissions(Group)
                Case CHUNK_ALTERNATIVES_PERMISSIONS
                    res = ProcessAlternativesPermissions(Group)
            End Select
        End While

        mBR.Close()
        Return True
    End Function

    Public Overrides Function ReadUserDisabledNodes(ByVal User As clsUser) As Boolean 'C0330
        mBR = New BinaryReader(mStream)

        Dim chunk As Int32
        Dim res As Boolean = True

        While (mStream.Position < mStream.Length - 1) And res
            chunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            Dim curPos As Integer = mBR.BaseStream.Position 'C0263

            Select Case chunk
                Case CHUNK_HIERARCHY_DISABLED_NODES
                    res = ProcessHierarchyDisabledNodes(User)
            End Select

            'Debug.Print("NewPos - oldPos = " + (mBR.BaseStream.Position - curPos).ToString + "; ChunkSize = " + ChunkSize.ToString + " (Read User Disabled Nodes)") 'C0263
        End While

        mBR.Close()
        Return True
    End Function

    Public Overrides Function ReadInfoDocs() As Boolean 'C0276
        mBR = New BinaryReader(mStream)

        If mWriteStatus Then
            Debug.Print("ReadInfoDocs started " + Now.ToString)
        End If

        Dim hCount As Int32 = mBR.ReadInt32 'hierarchies count
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hCount)
            chunk = mBR.ReadInt32
            Dim HInfoDocsChunkSize As Integer = mBR.ReadInt32

            If chunk = CHUNK_HIERARCHY_INFODOCS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16))
                Dim InfoDocsCount As Int32 = mBR.ReadInt32

                Dim k As Integer = 0
                While (mStream.Position < mStream.Length - 1) And (k <> InfoDocsCount)
                    Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                    'Dim node As clsNode = ProjectManager.GetAnyHierarchyByID(hGuidID).GetNodeByID(NodeGuidID) 'C0659
                    'C0659===
                    Dim node As clsNode = Nothing
                    Dim H As clsHierarchy = ProjectManager.GetAnyHierarchyByID(hGuidID)
                    If H IsNot Nothing Then
                        node = ProjectManager.GetAnyHierarchyByID(hGuidID).GetNodeByID(NodeGuidID)
                    End If
                    'C0659==

                    If node IsNot Nothing Then
                        node.InfoDoc = mBR.ReadString
                    Else
                        Dim s As String = mBR.ReadString()
                    End If
                    k += 1
                End While

            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ReadInfoDocs completed " + Now.ToString)
        End If

        mBR.Close()
        Return True
    End Function

    Public Overrides Function ReadAdvancedInfoDocs() As Boolean 'C0920
        mBR = New BinaryReader(mStream)

        If mWriteStatus Then
            Debug.Print("ReadAdvancedInfoDocs started " + Now.ToString)
        End If

        ProjectManager.InfoDocs.InfoDocs.Clear()

        Dim DocsCount As Int32 = mBR.ReadInt32 ' infodocs count
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> DocsCount)
            chunk = mBR.ReadInt32

            Select Case chunk
                Case CHUNK_INFODOC
                    Dim DocType As InfoDocType = mBR.ReadInt32
                    Dim TargetGuid As Guid = New Guid(mBR.ReadBytes(16))
                    Dim AdditionalGuid As Guid = Guid.Empty
                    Select Case DocType
                        Case InfoDocType.idtNodeWRTParent, InfoDocType.idtUserWRTGroup
                            AdditionalGuid = New Guid(mBR.ReadBytes(16))
                    End Select
                    Dim InfoDoc As String = mBR.ReadString

                    'Dim NewInfoDoc As New clsInfoDoc(DocType, TargetGuid, AdditionalGuid, InfoDoc)
                    'ProjectManager.InfoDocs.InfoDocs.Add(NewInfoDoc)
                    Select Case DocType
                        Case InfoDocType.idtNode
                            ProjectManager.InfoDocs.SetNodeInfoDoc(TargetGuid, InfoDoc)
                        Case InfoDocType.idtNodeWRTParent
                            ProjectManager.InfoDocs.SetNodeWRTInfoDoc(TargetGuid, AdditionalGuid, InfoDoc)
                        Case InfoDocType.idtProjectDescription
                            ProjectManager.InfoDocs.SetProjectDescription(InfoDoc)
                        Case Else
                            Dim NewInfoDoc As New clsInfoDoc(DocType, TargetGuid, AdditionalGuid, InfoDoc)  ' D4345
                            ProjectManager.InfoDocs.InfoDocs.Add(NewInfoDoc)    ' D4345
                    End Select
            End Select

            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ReadAdvancedInfoDocs completed " + Now.ToString)
        End If

        mBR.Close()
        Return True
    End Function
End Class


<Serializable()> Public Class clsStreamModelReader_v_1_1_8 'C0350
    Inherits clsStreamModelReader_v_1_1_7

    'Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Boolean 'C0765
    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer 'C0765
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261


                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            'Dim NodeID As Int32 = mBR.ReadInt32 'C0261
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then 'C0264

                                'C0283===
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            'validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid) 'C0431
                                            'C0431===
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                            'C0431==
                                    End Select
                                End If
                                'C0283==

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    'Dim childNodeID As Int32 'C0261
                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        If node.MeasureType = ECMeasureType.mtPairwise Then
                                            'Dim firstNodeID As Int32 = mBR.ReadInt32 'C0261
                                            'Dim secondNodeID As Int32 = mBR.ReadInt32 'C0261
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            'C0275===
                                            'Dim firstNode As clsNode = node.Hierarchy.GetNodeByID(firstNodeGuidID) 'C0261
                                            'Dim secondNode As clsNode = node.Hierarchy.GetNodeByID(secondNodeGuidID) 'C0261
                                            'C0275==

                                            'C0275===
                                            Dim firstNode As clsNode
                                            Dim secondNode As clsNode

                                            If node.IsTerminalNode Then
                                                firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                secondNode = AH.GetNodeByID(secondNodeGuidID)
                                            Else
                                                firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                            End If
                                            'C0275===

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            'MD = New clsPairwiseMeasureData(firstNodeID, secondNodeID, advantage, value, node.NodeID, User.UserID, value = 0) 'C0261

                                            If firstNode IsNot Nothing And secondNode IsNot Nothing Then 'C0264
                                                MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, value = 0) 'C0261
                                            End If
                                        Else
                                            'childNodeID = mBR.ReadInt32 'C0261
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            'Dim childNode As clsNode = node.Hierarchy.GetNodeByID(childNodeGuidID) 'C0261 'C0275
                                            'C0275===
                                            Dim childNode As clsNode
                                            If node.IsTerminalNode Then
                                                childNode = AH.GetNodeByID(childNodeGuidID)
                                            Else
                                                childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                            End If
                                            'C0275==

                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    'C0396===
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                    Dim rating As clsRating
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle

                                                        rating = New clsRating
                                                        rating.ID = -1
                                                        rating.Name = "Direct Entry from EC11.5"
                                                        rating.Value = directValue
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                        rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                    End If

                                                    'C0396==

                                                    'Dim ratingGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261 'C0396

                                                    If childNode IsNot Nothing Then 'C0264
                                                        'Dim rating As clsRating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID) 'C0396
                                                        MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, rating Is Nothing) 'C0261
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    'MD = New clsUtilityCurveMeasureData(childNodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, Single.IsNaN(ucDataValue)) 'C0261
                                                    If childNode IsNot Nothing Then 'C0264
                                                        MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, Single.IsNaN(ucDataValue)) 'C0261
                                                    End If
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    'MD = New clsStepMeasureData(childNodeID, node.NodeID, User.UserID, value, node.MeasurementScale, Single.IsNaN(value)) 'C0261
                                                    If childNode IsNot Nothing Then 'C0264
                                                        MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, Single.IsNaN(value)) 'C0261
                                                    End If
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    'MD = New clsDirectMeasureData(childNodeID, node.NodeID, User.UserID, directValue, Single.IsNaN(directValue)) 'C0261
                                                    If childNode IsNot Nothing Then 'C0264
                                                        MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, Single.IsNaN(directValue)) 'C0261
                                                    End If
                                            End Select
                                        End If

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            'C0369===
                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If
                                            'C0369==

                                            'node.Judgments.AddMeasureData(MD, True) 'C0264 'C0327
                                            node.Judgments.AddMeasureData(MD) 'C0327

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        'node.Judgments.AddMeasureData(MD, True) 'C0260 'C0264

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If


            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_9 'C0425
    Inherits clsStreamModelReader_v_1_1_8

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean
        ' read hierarchies with nodes

        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            chunk = mBR.ReadInt32
            Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                End Select

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32
                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, node.ParentNodeID, True) 'C0383

                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32

                            'C0952===
                            'If defDI <> UNDEFINED_DATA_INSTANCE_ID Then
                            '    node.DefaultDataInstance = ProjectManager.GetDataInstanceByID(defDI)
                            'End If
                            'C0952==

                            node.Comment = mBR.ReadString

                            'C0425===
                            Dim cost As Single = mBR.ReadSingle
                            If (cost <> Single.MinValue) And (node.IsAlternative) Then
                                node.Tag = cost
                            End If
                            'C0425==

                            'C0343===
                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
        Next

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_11 'C0625
    Inherits clsStreamModelReader_v_1_1_9

    Protected Overrides Function ProcessUsersListChunk() As Boolean
        ' read users list

        ProjectManager.UsersList.Clear()
        ProjectManager.DataInstanceUsers.Clear()

        ProjectManager.UserID = 0

        ' read the number of users first
        Dim usersCount As Int32 = mBR.ReadInt32

        Dim i As Integer = 0

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> usersCount)
            subChunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_USER Then
                Dim user As clsUser = New clsUser
                user.UserID = mBR.ReadInt32
                user.UserGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                user.UserName = mBR.ReadString
                user.UserEMail = mBR.ReadString
                user.Active = mBR.ReadBoolean
                user.VotingBoxID = mBR.ReadInt32
                user.IncludedInSynchronous = mBR.ReadBoolean
                user.DataInstanceID = mBR.ReadInt32
                user.SyncEvaluationMode = CType(mBR.ReadInt32, SynchronousEvaluationMode) 'C0625
                Dim evalGroupID As Int32 = mBR.ReadInt32
                user.EvaluationGroup = ProjectManager.EvaluationGroups.GetGroupByID(evalGroupID)

                'C0343===
                Dim count As Integer = mBR.ReadInt32
                If count <> 0 Then
                    Dim bytes As Byte() = mBR.ReadBytes(count)
                    Dim stream As New MemoryStream(bytes)
                    user.AHPUserData = New clsAHPUserData
                    user.AHPUserData.FromStream(stream)
                End If
                'C0343==

                If user.UserID >= 0 Then
                    ProjectManager.UsersList.Add(user)
                Else
                    ProjectManager.DataInstanceUsers.Add(user)
                End If
            Else
                Return False
            End If
            i += 1
        End While

        'ProjectManager.AddCombinedUser() 'C0552

        If mWriteStatus Then
            Debug.Print("ProcessUsersList " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean 'C0626
        ' read hierarchies with nodes

        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim bReadHiearchyChunk As Boolean = True 'C0738

        Dim HierarchyChunkSize As Integer 'C0738

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            If bReadHiearchyChunk Then 'C0738
                chunk = mBR.ReadInt32
                HierarchyChunkSize = mBR.ReadInt32 'C0738
            End If
            bReadHiearchyChunk = True 'C0738 - reset reading chunk (this can be set to False later when finding out that the model is corrupted - getting hierarchy chunk instead of node chunk)

            'Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263 'C0738

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                End Select

                H.Nodes.Clear()
                H.ResetNodesDictionaries()

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then 'C0738
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32
                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            If node.IsAlternative Then
                                node.ParentNodeID = -1
                            End If

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If (H.HierarchyType <> ECHierarchyType.htModel) Or
                               ((H.HierarchyType = ECHierarchyType.htModel) And ((node.ParentNodeID <> -1) Or ((node.ParentNodeID = -1) AndAlso (H.Nodes.Count = 0)))) Then 'C0731
                                If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, node.ParentNodeID, True) 'C0383
                            End If


                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            Debug.Print(node.NodeName + ": " + node.MeasureType.ToString)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRatingScale).Name)
                                Case ECMeasureType.mtRegularUtilityCurve
                                    Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRegularUtilityCurve).Name + " (" + CType(node.MeasurementScale, clsRegularUtilityCurve).IsIncreasing.ToString + ")")
                            End Select

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32

                            'C0952===
                            'If defDI <> UNDEFINED_DATA_INSTANCE_ID Then
                            '    node.DefaultDataInstance = ProjectManager.GetDataInstanceByID(defDI)
                            'End If
                            'C0952==

                            node.Comment = mBR.ReadString

                            'C0425===
                            'Dim cost As Single = mBR.ReadSingle
                            'If (cost <> Single.MinValue) And (node.IsAlternative) Then
                            '    node.Tag = cost
                            'End If
                            'C0425==

                            'C0626===
                            Dim cost As String = mBR.ReadString
                            If (cost <> UNDEFINED_COST_VALUE) And (node.IsAlternative) Then
                                node.Tag = cost
                            End If
                            'C0626==

                            'C0343===
                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            'C0738===
                            If subChunk = CHUNK_HIERARCHY Then
                                bReadHiearchyChunk = False
                                chunk = CHUNK_HIERARCHY
                                HierarchyChunkSize = NodeChunkSize
                                j = nodesCount - 1 'to skip nodes after increment below
                            Else
                                Return False
                            End If
                            'C0738===

                            'Return False 'C0738
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
        Next

        ProjectManager.VerifyObjectivesHierarchies()

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Public Overrides Function GetUsersList() As List(Of clsUser)
        mBR = New BinaryReader(mStream)

        mBR.BaseStream.Seek(0, SeekOrigin.Begin)

        Dim UsersList As New List(Of clsUser)

        Dim res As Boolean = True
        While (mStream.Position < mStream.Length - 1) And res
            Dim chunk As Int32 = mBR.ReadInt32()
            Dim ChunkSize As Integer = mBR.ReadInt32

            If chunk = CHUNK_USERS_LIST Then
                ' read users list

                ' read the number of users first
                Dim usersCount As Int32 = mBR.ReadInt32

                Dim i As Integer = 0

                Dim subChunk As Int32
                While (mStream.Position < mStream.Length - 1) And (i <> usersCount)
                    subChunk = mBR.ReadInt32
                    Dim chSize As Integer = mBR.ReadInt32

                    If subChunk = CHUNK_USER Then
                        Dim user As clsUser = New clsUser
                        user.UserID = mBR.ReadInt32
                        user.UserGuidID = New Guid(mBR.ReadBytes(16))
                        user.UserName = mBR.ReadString
                        user.UserEMail = mBR.ReadString
                        user.Active = mBR.ReadBoolean
                        user.VotingBoxID = mBR.ReadInt32
                        user.IncludedInSynchronous = mBR.ReadBoolean
                        user.DataInstanceID = mBR.ReadInt32
                        user.SyncEvaluationMode = CType(mBR.ReadInt32, SynchronousEvaluationMode) 'C0628
                        Dim evalGroupID As Int32 = mBR.ReadInt32
                        'user.EvaluationGroup = ProjectManager.EvaluationGroups.GetGroupByID(evalGroupID)

                        'C0356===
                        Dim count As Integer = mBR.ReadInt32
                        If count <> 0 Then
                            Dim bytes As Byte() = mBR.ReadBytes(count)
                            Dim stream As New MemoryStream(bytes)
                            user.AHPUserData = New clsAHPUserData
                            user.AHPUserData.FromStream(stream)
                        End If
                        'C0356==

                        If user.UserID >= 0 Then
                            UsersList.Add(user)
                        End If
                    End If
                    i += 1
                End While

                res = False
            Else
                mBR.BaseStream.Seek(ChunkSize - 4, SeekOrigin.Current)
            End If
        End While

        mBR.Close()
        Return UsersList
    End Function

End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_12 'C0646
    Inherits clsStreamModelReader_v_1_1_11

    Protected Overrides Function ProcessDataInstancesChunk() As Boolean
        ' read data instances definitions

        ProjectManager.DataInstances.Clear()

        ' read the number of data instances first
        Dim diCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim DI As clsDataInstance

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> diCount)
            subChunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_DATA_INSTANCE Then
                DI = New clsDataInstance
                DI.ID = mBR.ReadInt32
                DI.Name = mBR.ReadString
                DI.User = ProjectManager.GetDataInstanceUserByID(mBR.ReadInt32)
                DI.EvaluatorUser = ProjectManager.GetUserByID(mBR.ReadInt32) 'C0646
                DI.Comment = mBR.ReadString

                'ProjectManager.DataInstances.Add(DI) 'C0952
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessDataInstances " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_13 'C0745
    Inherits clsStreamModelReader_v_1_1_12

    Protected Overrides Function ProcessUsersListChunk() As Boolean
        ' read users list

        ProjectManager.UsersList.Clear()
        ProjectManager.DataInstanceUsers.Clear()

        ProjectManager.UserID = 0

        ' read the number of users first
        Dim usersCount As Int32 = mBR.ReadInt32

        Dim i As Integer = 0

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> usersCount)
            subChunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_USER Then
                Dim user As clsUser = New clsUser
                user.UserID = mBR.ReadInt32
                user.UserGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                user.UserName = mBR.ReadString
                user.UserEMail = mBR.ReadString
                user.Active = mBR.ReadBoolean
                user.VotingBoxID = mBR.ReadInt32
                user.IncludedInSynchronous = mBR.ReadBoolean
                user.DataInstanceID = mBR.ReadInt32
                user.SyncEvaluationMode = CType(mBR.ReadInt32, SynchronousEvaluationMode) 'C0625
                user.Weight = mBR.ReadSingle 'C0745
                Dim evalGroupID As Int32 = mBR.ReadInt32
                user.EvaluationGroup = ProjectManager.EvaluationGroups.GetGroupByID(evalGroupID)

                'C0343===
                Dim count As Integer = mBR.ReadInt32
                If count <> 0 Then
                    Dim bytes As Byte() = mBR.ReadBytes(count)
                    Dim stream As New MemoryStream(bytes)
                    user.AHPUserData = New clsAHPUserData
                    user.AHPUserData.FromStream(stream)
                End If
                'C0343==

                If user.UserID >= 0 Then
                    ProjectManager.UsersList.Add(user)
                Else
                    ProjectManager.DataInstanceUsers.Add(user)
                End If
            Else
                Return False
            End If
            i += 1
        End While

        'ProjectManager.AddCombinedUser() 'C0552

        If mWriteStatus Then
            Debug.Print("ProcessUsersList " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Public Overrides Function GetUsersList() As List(Of clsUser)
        mBR = New BinaryReader(mStream)

        mBR.BaseStream.Seek(0, SeekOrigin.Begin)

        Dim UsersList As New List(Of clsUser)

        Dim res As Boolean = True
        While (mStream.Position < mStream.Length - 1) And res
            Dim chunk As Int32 = mBR.ReadInt32()
            Dim ChunkSize As Integer = mBR.ReadInt32

            If chunk = CHUNK_USERS_LIST Then
                ' read users list

                ' read the number of users first
                Dim usersCount As Int32 = mBR.ReadInt32

                Dim i As Integer = 0

                Dim subChunk As Int32
                While (mStream.Position < mStream.Length - 1) And (i <> usersCount)
                    subChunk = mBR.ReadInt32
                    Dim chSize As Integer = mBR.ReadInt32

                    If subChunk = CHUNK_USER Then
                        Dim user As clsUser = New clsUser
                        user.UserID = mBR.ReadInt32
                        user.UserGuidID = New Guid(mBR.ReadBytes(16))
                        user.UserName = mBR.ReadString
                        user.UserEMail = mBR.ReadString
                        user.Active = mBR.ReadBoolean
                        user.VotingBoxID = mBR.ReadInt32
                        user.IncludedInSynchronous = mBR.ReadBoolean
                        user.DataInstanceID = mBR.ReadInt32
                        user.SyncEvaluationMode = CType(mBR.ReadInt32, SynchronousEvaluationMode) 'C0628
                        user.Weight = mBR.ReadSingle 'C0745
                        Dim evalGroupID As Int32 = mBR.ReadInt32
                        'user.EvaluationGroup = ProjectManager.EvaluationGroups.GetGroupByID(evalGroupID)

                        'C0356===
                        Dim count As Integer = mBR.ReadInt32
                        If count <> 0 Then
                            Dim bytes As Byte() = mBR.ReadBytes(count)
                            Dim stream As New MemoryStream(bytes)
                            user.AHPUserData = New clsAHPUserData
                            user.AHPUserData.FromStream(stream)
                        End If
                        'C0356==

                        If user.UserID >= 0 Then
                            UsersList.Add(user)
                        End If
                    End If
                    i += 1
                End While

                res = False
            Else
                mBR.BaseStream.Seek(ChunkSize - 4, SeekOrigin.Current)
            End If
        End While

        mBR.Close()
        Return UsersList
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_14 'C0900
    Inherits clsStreamModelReader_v_1_1_13

    Protected Overrides Function ProcessNodesPermissions(ByVal User As clsUser) As Boolean
        Dim IsAllowed As Boolean = mBR.ReadBoolean
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

        If mWriteStatus Then
            Debug.Print("ProcessUserPermissions started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyGuidID) 'C0261

        Dim nodesCount As Integer = mBR.ReadInt32
        Dim i As Integer = 0

        Dim NodeGuidID As Guid
        Dim node As clsNode

        Dim nodesList As New List(Of Guid)

        While (mStream.Position < mStream.Length - 1) And (i <> nodesCount)
            NodeGuidID = New Guid(mBR.ReadBytes(16))

            If H IsNot Nothing Then
                node = H.GetNodeByID(NodeGuidID) 'C0261
                If node IsNot Nothing Then
                    nodesList.Add(NodeGuidID)
                End If
            End If
            i += 1
        End While

        If nodesList.Count > 0 Then
            ProjectManager.UsersRoles.SetObjectivesRoles(User.UserID, nodesList, If(IsAllowed, RolesValueType.rvtAllowed, RolesValueType.rvtRestricted))
        End If

        If mWriteStatus Then
            Debug.Print("ProcessUserPermissions completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessAlternativesPermissions(ByVal User As clsUser) As Boolean
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16))
        Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyGuidID)

        Dim AltsHierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16))
        Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyGuidID)

        If H IsNot Nothing And AH IsNot Nothing Then
            Dim coCount As Int32 = mBR.ReadInt32 ' covering objectives count
            Dim i As Integer = 0

            Dim NodeGuidID As Guid
            Dim node As clsNode

            Dim IsAllowed As Boolean

            Dim chunk As Int32
            While (mStream.Position < mStream.Length - 1) And (i <> coCount)
                chunk = mBR.ReadInt32
                Dim NodeAltsChunkSize As Integer = mBR.ReadInt32

                If chunk = CHUNK_NODE_ALTERNATIVES_PERMISSIONS Then
                    NodeGuidID = New Guid(mBR.ReadBytes(16))
                    IsAllowed = mBR.ReadBoolean

                    node = H.GetNodeByID(NodeGuidID)

                    Dim altCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim altGuidID As Guid
                    Dim alt As clsNode

                    Dim altsList As New HashSet(Of Guid)

                    While (mStream.Position < mStream.Length - 1) And (j <> altCount)
                        altGuidID = New Guid(mBR.ReadBytes(16))
                        alt = AH.GetNodeByID(altGuidID)

                        If alt IsNot Nothing And node IsNot Nothing Then
                            altsList.Add(altGuidID)
                        End If

                        j += 1
                    End While

                    If altsList.Count > 0 Then
                        Dim nodesList As New List(Of Guid)
                        nodesList.Add(NodeGuidID)

                        ProjectManager.UsersRoles.SetAlternativesRoles(User.UserID, nodesList, altsList, If(IsAllowed, RolesValueType.rvtAllowed, RolesValueType.rvtRestricted)) 'C1061
                    End If
                End If
                i += 1
            End While
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_15 'C1030
    Inherits clsStreamModelReader_v_1_1_14

    Protected Overrides Function ProcessNodesPermissions(ByVal UserID As Integer) As Boolean
        'Dim IsAllowed As Boolean = mBR.ReadBoolean
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

        If mWriteStatus Then
            Debug.Print("ProcessUserPermissions started " + Now.ToString + " User: " + UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyGuidID) 'C0261

        ' reading restricted nodes
        Dim nodesCount As Integer = mBR.ReadInt32
        Dim i As Integer = 0

        Dim NodeGuidID As Guid
        Dim node As clsNode

        Dim RestrictedNodesList As New List(Of Guid)

        While (mStream.Position < mStream.Length - 1) And (i <> nodesCount)
            NodeGuidID = New Guid(mBR.ReadBytes(16))

            If H IsNot Nothing Then
                node = H.GetNodeByID(NodeGuidID) 'C0261
                If node IsNot Nothing Then
                    RestrictedNodesList.Add(NodeGuidID)
                End If
            End If
            i += 1
        End While

        If RestrictedNodesList.Count > 0 Then
            ProjectManager.UsersRoles.SetObjectivesRoles(UserID, RestrictedNodesList, RolesValueType.rvtRestricted) 'C1061
        End If

        ' reading allowed nodes
        nodesCount = mBR.ReadInt32

        Dim AllowedNodesList As New List(Of Guid)
        i = 0 'C1031

        While (mStream.Position < mStream.Length - 1) And (i <> nodesCount)
            NodeGuidID = New Guid(mBR.ReadBytes(16))

            If H IsNot Nothing Then
                node = H.GetNodeByID(NodeGuidID) 'C0261
                If node IsNot Nothing Then
                    AllowedNodesList.Add(NodeGuidID)
                End If
            End If
            i += 1
        End While

        If AllowedNodesList.Count > 0 Then
            ProjectManager.UsersRoles.SetObjectivesRoles(UserID, AllowedNodesList, RolesValueType.rvtAllowed) 'C1061
        End If

        If mWriteStatus Then
            Debug.Print("ProcessUserPermissions completed " + Now.ToString + " User: " + UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessAlternativesPermissions(ByVal UserID As Integer) As Boolean
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16))
        Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyGuidID)

        Dim AltsHierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16))
        Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyGuidID)

        If H IsNot Nothing And AH IsNot Nothing Then
            Dim coCount As Int32 = mBR.ReadInt32 ' covering objectives count
            Dim i As Integer = 0

            Dim NodeGuidID As Guid
            Dim node As clsNode

            'Dim IsAllowed As Boolean

            Dim chunk As Int32
            While (mStream.Position < mStream.Length - 1) And (i <> coCount)
                chunk = mBR.ReadInt32
                Dim NodeAltsChunkSize As Integer = mBR.ReadInt32

                If chunk = CHUNK_NODE_ALTERNATIVES_PERMISSIONS Then
                    NodeGuidID = New Guid(mBR.ReadBytes(16))
                    'IsAllowed = mBR.ReadBoolean

                    node = H.GetNodeByID(NodeGuidID)

                    ' read restricted alternatives
                    Dim altCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim altGuidID As Guid
                    Dim alt As clsNode

                    Dim RestrictedAltsList As New HashSet(Of Guid)

                    While (mStream.Position < mStream.Length - 1) And (j <> altCount)
                        altGuidID = New Guid(mBR.ReadBytes(16))
                        alt = AH.GetNodeByID(altGuidID)

                        If alt IsNot Nothing And node IsNot Nothing Then
                            RestrictedAltsList.Add(altGuidID)
                        End If

                        j += 1
                    End While

                    If RestrictedAltsList.Count > 0 Then
                        Dim nodesList As New List(Of Guid)
                        nodesList.Add(NodeGuidID)

                        ProjectManager.UsersRoles.SetAlternativesRoles(UserID, nodesList, RestrictedAltsList, RolesValueType.rvtRestricted) 'C1061
                    End If

                    ' read allowed alternatives
                    altCount = mBR.ReadInt32
                    j = 0

                    Dim AllowedAltsList As New HashSet(Of Guid)

                    While (mStream.Position < mStream.Length - 1) And (j <> altCount)
                        altGuidID = New Guid(mBR.ReadBytes(16))
                        alt = AH.GetNodeByID(altGuidID)

                        If alt IsNot Nothing And node IsNot Nothing Then
                            AllowedAltsList.Add(altGuidID)
                        End If

                        j += 1
                    End While

                    If AllowedAltsList.Count > 0 Then
                        Dim nodesList As New List(Of Guid)
                        nodesList.Add(NodeGuidID)

                        ProjectManager.UsersRoles.SetAlternativesRoles(UserID, nodesList, AllowedAltsList, RolesValueType.rvtAllowed) 'C1061
                    End If

                End If
                i += 1
            End While
        End If

        Return True
    End Function

    Protected Overrides Function ProcessNodesPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
        Return ProcessNodesPermissions(CombinedGroup.CombinedUserID)
    End Function

    Protected Overrides Function ProcessAlternativesPermissions(ByVal CombinedGroup As clsCombinedGroup) As Boolean
        Return ProcessAlternativesPermissions(CombinedGroup.CombinedUserID)
    End Function

    Protected Overrides Function ProcessNodesPermissions(ByVal User As clsUser) As Boolean
        Return ProcessNodesPermissions(User.UserID)
    End Function

    Protected Overrides Function ProcessAlternativesPermissions(ByVal User As clsUser) As Boolean
        Return ProcessAlternativesPermissions(User.UserID)
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_16
    Inherits clsStreamModelReader_v_1_1_15

    Protected Overrides Function ProcessNodesPermissions(ByVal UserID As Integer) As Boolean
        'Dim IsAllowed As Boolean = mBR.ReadBoolean
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

        If mWriteStatus Then
            Debug.Print("ProcessUserPermissions started " + Now.ToString + " User: " + UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyGuidID) 'C0261

        ' reading restricted nodes
        Dim nodesCount As Integer = mBR.ReadInt32
        Dim i As Integer = 0

        Dim NodeGuidID As Guid
        Dim node As clsNode

        Dim RestrictedNodesList As New List(Of Guid)
        RestrictedNodesList.Capacity = nodesCount

        While (mStream.Position < mStream.Length - 1) And (i <> nodesCount)
            NodeGuidID = New Guid(mBR.ReadBytes(16))

            If H IsNot Nothing Then
                node = H.GetNodeByID(NodeGuidID) 'C0261
                If node IsNot Nothing Then
                    RestrictedNodesList.Add(NodeGuidID)
                End If
            End If
            i += 1
        End While

        If RestrictedNodesList.Count > 0 Then
            ProjectManager.UsersRoles.SetObjectivesRoles(UserID, RestrictedNodesList, RolesValueType.rvtRestricted, Not ProjectManager.IsRiskProject)
        End If

        ' reading allowed nodes
        nodesCount = mBR.ReadInt32

        Dim AllowedNodesList As New List(Of Guid)
        AllowedNodesList.Capacity = nodesCount
        i = 0 'C1031

        While (mStream.Position < mStream.Length - 1) And (i <> nodesCount)
            NodeGuidID = New Guid(mBR.ReadBytes(16))

            If H IsNot Nothing Then
                node = H.GetNodeByID(NodeGuidID) 'C0261
                If node IsNot Nothing Then
                    AllowedNodesList.Add(NodeGuidID)
                End If
            End If
            i += 1
        End While

        If AllowedNodesList.Count > 0 Then
            ProjectManager.UsersRoles.SetObjectivesRoles(UserID, AllowedNodesList, RolesValueType.rvtAllowed, Not ProjectManager.IsRiskProject)
        End If

        ' reading undefined nodes
        nodesCount = mBR.ReadInt32

        Dim UndefinedNodesList As New List(Of Guid)
        UndefinedNodesList.Capacity = nodesCount
        i = 0

        While (mStream.Position < mStream.Length - 1) And (i <> nodesCount)
            NodeGuidID = New Guid(mBR.ReadBytes(16))

            If H IsNot Nothing Then
                node = H.GetNodeByID(NodeGuidID) 'C0261
                If node IsNot Nothing Then
                    UndefinedNodesList.Add(NodeGuidID)
                End If
            End If
            i += 1
        End While

        If (UndefinedNodesList.Count > 0) And (UserID = COMBINED_USER_ID) Then
            ProjectManager.UsersRoles.SetObjectivesRoles(UserID, UndefinedNodesList, RolesValueType.rvtUndefined, Not ProjectManager.IsRiskProject)
        End If

        If mWriteStatus Then
            Debug.Print("ProcessUserPermissions completed " + Now.ToString + " User: " + UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessAlternativesPermissions(ByVal UserID As Integer) As Boolean
        Dim HierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16))
        Dim H As clsHierarchy = ProjectManager.Hierarchy(HierarchyGuidID)

        Dim AltsHierarchyGuidID As Guid = New Guid(mBR.ReadBytes(16))
        Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyGuidID)

        'If H IsNot Nothing And AH IsNot Nothing Then
        Dim coCount As Int32 = mBR.ReadInt32 ' covering objectives count
        Dim i As Integer = 0

        Dim NodeGuidID As Guid
        Dim node As clsNode

        'Dim IsAllowed As Boolean

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> coCount)
            chunk = mBR.ReadInt32
            Dim NodeAltsChunkSize As Integer = mBR.ReadInt32

            If H IsNot Nothing And AH IsNot Nothing Then
                If chunk = CHUNK_NODE_ALTERNATIVES_PERMISSIONS Then
                    NodeGuidID = New Guid(mBR.ReadBytes(16))
                    'IsAllowed = mBR.ReadBoolean

                    node = H.GetNodeByID(NodeGuidID)

                    ' read restricted alternatives
                    Dim altCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim altGuidID As Guid
                    Dim alt As clsNode

                    Dim RestrictedAltsList As New HashSet(Of Guid)
                    'RestrictedAltsList.Capacity = altCount

                    While (mStream.Position < mStream.Length - 1) And (j <> altCount)
                        altGuidID = New Guid(mBR.ReadBytes(16))
                        alt = AH.GetNodeByID(altGuidID)

                        If alt IsNot Nothing And node IsNot Nothing Then
                            RestrictedAltsList.Add(altGuidID)
                        End If

                        j += 1
                    End While

                    If RestrictedAltsList.Count > 0 Then
                        Dim nodesList As New List(Of Guid)
                        nodesList.Add(NodeGuidID)

                        ProjectManager.UsersRoles.SetAlternativesRoles(UserID, nodesList, RestrictedAltsList, RolesValueType.rvtRestricted, Not ProjectManager.IsRiskProject)
                    End If

                    ' read allowed alternatives
                    altCount = mBR.ReadInt32
                    j = 0

                    Dim AllowedAltsList As New HashSet(Of Guid)
                    'AllowedAltsList.Capacity = altCount

                    While (mStream.Position < mStream.Length - 1) And (j <> altCount)
                        altGuidID = New Guid(mBR.ReadBytes(16))
                        alt = AH.GetNodeByID(altGuidID)

                        If alt IsNot Nothing And node IsNot Nothing Then
                            AllowedAltsList.Add(altGuidID)
                        End If

                        j += 1
                    End While

                    If AllowedAltsList.Count > 0 Then
                        Dim nodesList As New List(Of Guid)
                        nodesList.Add(NodeGuidID)

                        ProjectManager.UsersRoles.SetAlternativesRoles(UserID, nodesList, AllowedAltsList, RolesValueType.rvtAllowed, Not ProjectManager.IsRiskProject)
                    End If

                    ' read undefined alternatives
                    altCount = mBR.ReadInt32
                    j = 0

                    Dim UndefinedAltsList As New HashSet(Of Guid)
                    'UndefinedAltsList.Capacity = altCount

                    While (mStream.Position < mStream.Length - 1) And (j <> altCount)
                        altGuidID = New Guid(mBR.ReadBytes(16))
                        alt = AH.GetNodeByID(altGuidID)

                        If alt IsNot Nothing And node IsNot Nothing Then
                            UndefinedAltsList.Add(altGuidID)
                        End If

                        j += 1
                    End While

                    If (UndefinedAltsList.Count > 0) And (UserID = COMBINED_USER_ID) Then
                        Dim nodesList As New List(Of Guid)
                        nodesList.Add(NodeGuidID)

                        ProjectManager.UsersRoles.SetAlternativesRoles(UserID, nodesList, UndefinedAltsList, RolesValueType.rvtUndefined, Not ProjectManager.IsRiskProject)
                    End If

                End If
            Else
                mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + NodeAltsChunkSize, SeekOrigin.Begin)
            End If
            i += 1
        End While
        'End If

        Return True
    End Function

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean 'C0626
        ' read hierarchies with nodes

        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()
        ProjectManager.MeasureHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim bReadHiearchyChunk As Boolean = True 'C0738

        Dim HierarchyChunkSize As Integer 'C0738

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            If bReadHiearchyChunk Then 'C0738
                chunk = mBR.ReadInt32
                HierarchyChunkSize = mBR.ReadInt32 'C0738
            End If
            bReadHiearchyChunk = True 'C0738 - reset reading chunk (this can be set to False later when finding out that the model is corrupted - getting hierarchy chunk instead of node chunk)

            'Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263 'C0738

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                    Case ECHierarchyType.htMeasure
                        H = ProjectManager.AddMeasureHierarchy
                End Select

                H.Nodes.Clear()
                H.ResetNodesDictionaries()

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then 'C0738
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32
                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If (H.HierarchyType <> ECHierarchyType.htModel) Or
                               ((H.HierarchyType = ECHierarchyType.htModel) And ((node.ParentNodeID <> -1) Or ((node.ParentNodeID = -1) AndAlso (H.Nodes.Count = 0)))) Then 'C0731
                                If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, node.ParentNodeID, True) 'C0383
                            End If


                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            Debug.Print(node.NodeName + ": " + node.MeasureType.ToString)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRatingScale).Name)
                                Case ECMeasureType.mtRegularUtilityCurve
                                    Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRegularUtilityCurve).Name + " (" + CType(node.MeasurementScale, clsRegularUtilityCurve).IsIncreasing.ToString + ")")
                            End Select

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32

                            'C0952===
                            'If defDI <> UNDEFINED_DATA_INSTANCE_ID Then
                            '    node.DefaultDataInstance = ProjectManager.GetDataInstanceByID(defDI)
                            'End If
                            'C0952==

                            node.Comment = mBR.ReadString

                            'C0425===
                            'Dim cost As Single = mBR.ReadSingle
                            'If (cost <> Single.MinValue) And (node.IsAlternative) Then
                            '    node.Tag = cost
                            'End If
                            'C0425==

                            'C0626===
                            Dim cost As String = mBR.ReadString
                            If (cost <> UNDEFINED_COST_VALUE) And (node.IsAlternative) Then
                                node.Tag = cost
                            End If
                            'C0626==

                            'C0343===
                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            'C0738===
                            If subChunk = CHUNK_HIERARCHY Then
                                bReadHiearchyChunk = False
                                chunk = CHUNK_HIERARCHY
                                HierarchyChunkSize = NodeChunkSize
                                j = nodesCount - 1 'to skip nodes after increment below
                            Else
                                Return False
                            End If
                            'C0738===

                            'Return False 'C0738
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
        Next

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer 'C0765
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261


                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            'Dim NodeID As Int32 = mBR.ReadInt32 'C0261
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then 'C0264

                                'C0283===
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            'validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid) 'C0431
                                            'C0431===
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                            'C0431==
                                    End Select
                                End If
                                'C0283==

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    'Dim childNodeID As Int32 'C0261
                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        If node.MeasureType = ECMeasureType.mtPairwise Then
                                            'Dim firstNodeID As Int32 = mBR.ReadInt32 'C0261
                                            'Dim secondNodeID As Int32 = mBR.ReadInt32 'C0261
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            'C0275===
                                            'Dim firstNode As clsNode = node.Hierarchy.GetNodeByID(firstNodeGuidID) 'C0261
                                            'Dim secondNode As clsNode = node.Hierarchy.GetNodeByID(secondNodeGuidID) 'C0261
                                            'C0275==

                                            'C0275===
                                            Dim firstNode As clsNode
                                            Dim secondNode As clsNode

                                            If node.IsTerminalNode Then
                                                firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                secondNode = AH.GetNodeByID(secondNodeGuidID)
                                            Else
                                                firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                            End If
                                            'C0275===

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            'MD = New clsPairwiseMeasureData(firstNodeID, secondNodeID, advantage, value, node.NodeID, User.UserID, value = 0) 'C0261

                                            If firstNode IsNot Nothing And secondNode IsNot Nothing Then 'C0264
                                                MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, value = 0) 'C0261
                                            End If
                                        Else
                                            'childNodeID = mBR.ReadInt32 'C0261
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            'Dim childNode As clsNode = node.Hierarchy.GetNodeByID(childNodeGuidID) 'C0261 'C0275
                                            'C0275===
                                            Dim childNode As clsNode
                                            If node.IsTerminalNode Then
                                                childNode = AH.GetNodeByID(childNodeGuidID)
                                            Else
                                                childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                            End If
                                            'C0275==

                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    'C0396===
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                    Dim rating As clsRating
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle

                                                        rating = New clsRating
                                                        rating.ID = -1
                                                        rating.Name = "Direct Entry from EC11.5"
                                                        rating.Value = directValue
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                        rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                    End If

                                                    'C0396==

                                                    'Dim ratingGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261 'C0396

                                                    If childNode IsNot Nothing Then 'C0264
                                                        'Dim rating As clsRating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID) 'C0396
                                                        MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, rating Is Nothing) 'C0261
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    'MD = New clsUtilityCurveMeasureData(childNodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, Single.IsNaN(ucDataValue)) 'C0261
                                                    If childNode IsNot Nothing Then 'C0264
                                                        MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, Single.IsNaN(ucDataValue)) 'C0261
                                                    End If
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    'MD = New clsStepMeasureData(childNodeID, node.NodeID, User.UserID, value, node.MeasurementScale, Single.IsNaN(value)) 'C0261
                                                    If childNode IsNot Nothing Then 'C0264
                                                        MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, Single.IsNaN(value)) 'C0261
                                                    End If
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    'MD = New clsDirectMeasureData(childNodeID, node.NodeID, User.UserID, directValue, Single.IsNaN(directValue)) 'C0261
                                                    If childNode IsNot Nothing Then 'C0264
                                                        MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, Single.IsNaN(directValue)) 'C0261
                                                    End If
                                            End Select
                                        End If

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            'C0369===
                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If
                                            'C0369==

                                            'node.Judgments.AddMeasureData(MD, True) 'C0264 'C0327
                                            node.Judgments.AddMeasureData(MD) 'C0327

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        'node.Judgments.AddMeasureData(MD, True) 'C0260 'C0264

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If


            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
                                Dim evalDiagonals As DiagonalsEvaluation = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        If node.MeasureType = ECMeasureType.mtPairwise Then
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                            Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                            If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                    If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                        madeCount += 1
                                                    Else
                                                        Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                        If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                            (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                            madeCount += 1
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)

                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select
                                            If ind <> -1 Then
                                                If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                    madeCount += 1
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_18
    Inherits clsStreamModelReader_v_1_1_16

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean 'C0626
        ' read hierarchies with nodes

        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()
        ProjectManager.MeasureHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim bReadHiearchyChunk As Boolean = True 'C0738

        Dim HierarchyChunkSize As Integer 'C0738

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            If bReadHiearchyChunk Then 'C0738
                chunk = mBR.ReadInt32
                HierarchyChunkSize = mBR.ReadInt32 'C0738
            End If
            bReadHiearchyChunk = True 'C0738 - reset reading chunk (this can be set to False later when finding out that the model is corrupted - getting hierarchy chunk instead of node chunk)

            'Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263 'C0738

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                    Case ECHierarchyType.htMeasure
                        H = ProjectManager.AddMeasureHierarchy
                End Select

                H.Nodes.Clear()
                H.ResetNodesDictionaries()

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then 'C0738
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32
                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If (H.HierarchyType <> ECHierarchyType.htModel) Or
                               ((H.HierarchyType = ECHierarchyType.htModel) And ((node.ParentNodeID <> -1) Or ((node.ParentNodeID = -1) AndAlso (H.Nodes.Count = 0)))) Then 'C0731
                                If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, node.ParentNodeID, True) 'C0383
                            End If


                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            Debug.Print(node.NodeName + ": " + node.MeasureType.ToString)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRatingScale).Name)
                                Case ECMeasureType.mtRegularUtilityCurve
                                    Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRegularUtilityCurve).Name + " (" + CType(node.MeasurementScale, clsRegularUtilityCurve).IsIncreasing.ToString + ")")
                            End Select

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32

                            'C0952===
                            'If defDI <> UNDEFINED_DATA_INSTANCE_ID Then
                            '    node.DefaultDataInstance = ProjectManager.GetDataInstanceByID(defDI)
                            'End If
                            'C0952==

                            node.Comment = mBR.ReadString

                            'C0425===
                            'Dim cost As Single = mBR.ReadSingle
                            'If (cost <> Single.MinValue) And (node.IsAlternative) Then
                            '    node.Tag = cost
                            'End If
                            'C0425==

                            'C0626===
                            Dim cost As String = mBR.ReadString
                            If (cost <> UNDEFINED_COST_VALUE) And (node.IsAlternative) Then
                                node.Tag = cost
                            End If
                            'C0626==

                            'C0343===
                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            'C0738===
                            If subChunk = CHUNK_HIERARCHY Then
                                bReadHiearchyChunk = False
                                chunk = CHUNK_HIERARCHY
                                HierarchyChunkSize = NodeChunkSize
                                j = nodesCount - 1 'to skip nodes after increment below
                            Else
                                Return False
                            End If
                            'C0738===

                            'Return False 'C0738
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
        Next

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessRatingScalesChunk(ByRef RatingScalesCount As Integer) As Boolean 'C0271
        ' read rating scales

        ProjectManager.MeasureScales.RatingsScales.Clear()

        ' read the number of rating scales first
        Dim rsCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RS As clsRatingScale

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rsCount)
            subChunk = mBR.ReadInt32
            Dim RSChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_RATING_SCALE Then
                Dim rsID As Int32 = mBR.ReadInt32
                RS = New clsRatingScale(rsID)
                RS.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                RS.Name = mBR.ReadString
                RS.Comment = mBR.ReadString
                RS.IsOutcomes = mBR.ReadBoolean

                ProjectManager.MeasureScales.RatingsScales.Add(RS)

                ' read rating scale intensities
                subChunk = mBR.ReadInt32
                Dim RIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_RATING_INTENSITIES Then
                    Dim iCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim id As Int32
                    Dim GuidID As Guid 'C0261
                    Dim name As String
                    Dim value As Single
                    'Dim priority As Single
                    Dim comment As String

                    Dim R As clsRating

                    While (mStream.Position < mStream.Length - 1) And (j <> iCount)
                        subChunk = mBR.ReadInt32
                        Dim RIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_RATING_INTENSITY Then
                            id = mBR.ReadInt32
                            GuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            name = mBR.ReadString
                            value = mBR.ReadSingle
                            comment = mBR.ReadString

                            If value < 0 Then
                                value = 0
                            End If

                            If value > 1 Then
                                value = 1
                            End If

                            R = New clsRating(id, name, value, RS, comment)
                            R.GuidID = GuidID 'C0261

                            RS.RatingSet.Add(R)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRatingScales " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RatingScalesCount = i 'C0271
        Return True
    End Function

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer 'C0765
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261


                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            'Dim NodeID As Int32 = mBR.ReadInt32 'C0261
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then 'C0264

                                'C0283===
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            'validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid) 'C0431
                                            'C0431===
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                            'C0431==
                                    End Select
                                End If
                                'C0283==

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    'Dim childNodeID As Int32 'C0261
                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                    R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, value = 0)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, value = 0)
                                                    End If
                                                End If
                                            Case Else
                                                'childNodeID = mBR.ReadInt32 'C0261
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                'Dim childNode As clsNode = node.Hierarchy.GetNodeByID(childNodeGuidID) 'C0261 'C0275
                                                'C0275===
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If
                                                'C0275==

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        'C0396===
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If

                                                        'C0396==

                                                        'Dim ratingGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261 'C0396

                                                        If childNode IsNot Nothing Then 'C0264
                                                            'Dim rating As clsRating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID) 'C0396
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, rating Is Nothing) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        'MD = New clsUtilityCurveMeasureData(childNodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, Single.IsNaN(ucDataValue)) 'C0261
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, Single.IsNaN(ucDataValue)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        'MD = New clsStepMeasureData(childNodeID, node.NodeID, User.UserID, value, node.MeasurementScale, Single.IsNaN(value)) 'C0261
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, Single.IsNaN(value)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        'MD = New clsDirectMeasureData(childNodeID, node.NodeID, User.UserID, directValue, Single.IsNaN(directValue)) 'C0261
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, Single.IsNaN(directValue)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            'C0369===
                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If
                                            'C0369==

                                            'node.Judgments.AddMeasureData(MD, True) 'C0264 'C0327
                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            Else
                                                node.Judgments.AddMeasureData(MD)
                                            End If


                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        'node.Judgments.AddMeasureData(MD, True) 'C0260 'C0264

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If


            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                'If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                If H IsNot Nothing AndAlso (H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
                                Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                Dim parentNode As clsNode = H.GetNodeByID(parentNodeGuidID)
                                                If parentNode IsNot Nothing Then
                                                    Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet

                                                    evalDiagonals = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                                    If node.IsTerminalNode Then
                                                        If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                                            evalDiagonals = DiagonalsEvaluation.deAll
                                                        End If
                                                    Else
                                                        If ForceObjectives And (outcomes.Count < ObjectivesForceLimit) Then
                                                            evalDiagonals = DiagonalsEvaluation.deAll
                                                        End If
                                                    End If

                                                    NodesList.Clear()
                                                    For Each R As clsRating In outcomes
                                                        Dim dummyNode As New clsNode
                                                        dummyNode.NodeGuidID = R.GuidID
                                                        NodesList.Add(dummyNode)
                                                    Next
                                                End If
                                            End If

                                            Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                            Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                            If value <> 0 Then
                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                        If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                            madeCount += 1
                                                        Else
                                                            Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                            If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                madeCount += 1
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)

                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select
                                            If ind <> -1 Then
                                                If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                    madeCount += 1
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function

    'Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
    '    Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
    '    Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
    '    Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
    '    Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
    '    Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
    '    Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
    '    Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

    '    LastJudgmentTime = VERY_OLD_DATE

    '    mBR = New BinaryReader(mStream)

    '    Dim res As Integer = 0 'C0765

    '    If mWriteStatus Then
    '        Debug.Print("Evaluation progress from streams started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
    '    End If

    '    Dim hjCount As Int32 = mBR.ReadInt32
    '    Dim j As Integer = 0

    '    Dim madeCount As Integer = 0

    '    Dim chunk As Int32
    '    While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
    '        chunk = mBR.ReadInt32
    '        Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

    '        If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
    '            Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
    '            Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

    '            Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

    '            If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
    '                Dim nodesCount As Int32 = mBR.ReadInt32

    '                Dim k As Integer = 0
    '                While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
    '                    Dim subChunk As Int32 = mBR.ReadInt32
    '                    Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

    '                    Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

    '                    If subChunk = CHUNK_NODE_JUDGMENTS Then
    '                        Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
    '                        Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

    '                        Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

    '                        If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
    '                            Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
    '                            Dim evalDiagonals As DiagonalsEvaluation = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
    '                            If node.IsTerminalNode Then
    '                                If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
    '                                    evalDiagonals = DiagonalsEvaluation.deAll
    '                                End If
    '                            Else
    '                                If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
    '                                    evalDiagonals = DiagonalsEvaluation.deAll
    '                                End If
    '                            End If

    '                            Dim MT As ECMeasureType = mBR.ReadInt32
    '                            Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

    '                            Dim validMeasurementTypeAndScale As Boolean
    '                            If MT <> node.MeasureType Then
    '                                validMeasurementTypeAndScale = False
    '                            Else
    '                                Select Case node.MeasureType
    '                                    Case ECMeasureType.mtPairwise, ECMeasureType.mtDirect
    '                                        validMeasurementTypeAndScale = True
    '                                    Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
    '                                        If node.MeasurementScale IsNot Nothing Then
    '                                            validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
    '                                        Else
    '                                            validMeasurementTypeAndScale = False
    '                                        End If
    '                                End Select
    '                            End If

    '                            If validMeasurementTypeAndScale Then 'C0283
    '                                Dim jCount As Int32 = mBR.ReadInt32
    '                                Dim i As Integer = 0

    '                                Dim childNodeGuidID As Guid 'C0261
    '                                Dim MD As clsCustomMeasureData

    '                                While (mStream.Position < mStream.Length - 1) And (i <> jCount)
    '                                    MD = Nothing

    '                                    If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
    '                                        Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
    '                                        Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
    '                                        Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

    '                                        Dim advantage As Int32 = mBR.ReadInt32
    '                                        Dim value As Single = mBR.ReadDouble

    '                                        Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
    '                                        Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

    '                                        If (ind1 <> -1) AndAlso (ind2 <> -1) Then
    '                                            If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
    '                                                If evalDiagonals = DiagonalsEvaluation.deAll Then
    '                                                    madeCount += 1
    '                                                Else
    '                                                    Dim diff As Integer = Math.Abs(ind1 - ind2)
    '                                                    If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or _
    '                                                        (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
    '                                                        madeCount += 1
    '                                                    End If
    '                                                End If
    '                                            End If
    '                                        End If
    '                                    Else
    '                                        childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
    '                                        Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)

    '                                        Select Case node.MeasureType
    '                                            Case ECMeasureType.mtRatings
    '                                                Dim isDirectValue As Boolean = mBR.ReadBoolean
    '                                                Dim directValue As Single
    '                                                Dim ratingGuidID As Guid

    '                                                If isDirectValue Then
    '                                                    directValue = mBR.ReadSingle
    '                                                Else
    '                                                    ratingGuidID = New Guid(mBR.ReadBytes(16))
    '                                                End If
    '                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
    '                                                Dim ucDataValue As Single = mBR.ReadSingle
    '                                            Case ECMeasureType.mtStep
    '                                                Dim value As Single = mBR.ReadSingle
    '                                            Case ECMeasureType.mtDirect
    '                                                Dim directValue As Single = mBR.ReadSingle
    '                                        End Select
    '                                        If ind <> -1 Then
    '                                            If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
    '                                                madeCount += 1
    '                                            End If
    '                                        End If
    '                                    End If

    '                                    mBR.ReadString()
    '                                    Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
    '                                    If time > LastJudgmentTime Then
    '                                        LastJudgmentTime = time
    '                                    End If
    '                                    i += 1
    '                                End While
    '                            Else
    '                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
    '                            End If
    '                        Else
    '                            mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
    '                        End If
    '                    End If
    '                    k += 1
    '                End While
    '            Else
    '                mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
    '            End If
    '        End If
    '        j += 1
    '    End While

    '    If mWriteStatus Then
    '        Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
    '    End If

    '    mBR.Close()

    '    If LastJudgmentTime = VERY_OLD_DATE Then
    '        LastJudgmentTime = Nothing
    '    End If
    '    Return madeCount
    'End Function

End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_19
    Inherits clsStreamModelReader_v_1_1_18

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean 'C0626
        ' read hierarchies with nodes

        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()
        ProjectManager.MeasureHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim bReadHiearchyChunk As Boolean = True 'C0738

        Dim HierarchyChunkSize As Integer 'C0738

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            If bReadHiearchyChunk Then 'C0738
                chunk = mBR.ReadInt32
                HierarchyChunkSize = mBR.ReadInt32 'C0738
            End If
            bReadHiearchyChunk = True 'C0738 - reset reading chunk (this can be set to False later when finding out that the model is corrupted - getting hierarchy chunk instead of node chunk)

            'Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263 'C0738

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                    Case ECHierarchyType.htMeasure
                        H = ProjectManager.AddMeasureHierarchy
                End Select

                H.Nodes.Clear()
                H.ResetNodesDictionaries()

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    H.Nodes.Capacity = nodesCount
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then 'C0738
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32
                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If (H.HierarchyType <> ECHierarchyType.htModel) Or
                               ((H.HierarchyType = ECHierarchyType.htModel) And ((node.ParentNodeID <> -1) Or ((node.ParentNodeID = -1) AndAlso (H.Nodes.Count = 0)))) Then 'C0731
                                If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, node.ParentNodeID, True) 'C0383
                            End If

                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            'If node.IsAlternative And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                            If node.IsAlternative And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                                node.MeasureType = ECMeasureType.mtRatings
                                node.RatingScaleID = -1
                            End If

                            Debug.Print(node.NodeName + ": " + node.MeasureType.ToString)

                            If node.MeasurementScale IsNot Nothing Then
                                Select Case node.MeasureType
                                    Case ECMeasureType.mtRatings
                                        Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRatingScale).Name)
                                    Case ECMeasureType.mtRegularUtilityCurve
                                        Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRegularUtilityCurve).Name + " (" + CType(node.MeasurementScale, clsRegularUtilityCurve).IsIncreasing.ToString + ")")
                                End Select
                            End If

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32

                            node.Comment = mBR.ReadString

                            Dim cost As String = mBR.ReadString
                            If (cost <> UNDEFINED_COST_VALUE) And (node.IsAlternative) Then
                                node.Tag = cost
                            End If

                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            'C0738===
                            If subChunk = CHUNK_HIERARCHY Then
                                bReadHiearchyChunk = False
                                chunk = CHUNK_HIERARCHY
                                HierarchyChunkSize = NodeChunkSize
                                j = nodesCount - 1 'to skip nodes after increment below
                            Else
                                Return False
                            End If
                            'C0738===

                            'Return False 'C0738
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
        Next

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Protected Overrides Function ProcessRatingScalesChunk(ByRef RatingScalesCount As Integer) As Boolean 'C0271
        ' read rating scales

        ProjectManager.MeasureScales.RatingsScales.Clear()

        ' read the number of rating scales first
        Dim rsCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RS As clsRatingScale

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rsCount)
            subChunk = mBR.ReadInt32
            Dim RSChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_RATING_SCALE Then
                Dim rsID As Int32 = mBR.ReadInt32
                RS = New clsRatingScale(rsID)
                RS.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                RS.Name = mBR.ReadString
                RS.Comment = mBR.ReadString
                RS.IsOutcomes = mBR.ReadBoolean

                ProjectManager.MeasureScales.RatingsScales.Add(RS)

                ' read rating scale intensities
                subChunk = mBR.ReadInt32
                Dim RIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_RATING_INTENSITIES Then
                    Dim iCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim id As Int32
                    Dim GuidID As Guid 'C0261
                    Dim name As String
                    Dim value As Single
                    'Dim priority As Single
                    Dim comment As String
                    Dim AvaiForPW As Boolean

                    Dim R As clsRating

                    While (mStream.Position < mStream.Length - 1) And (j <> iCount)
                        subChunk = mBR.ReadInt32
                        Dim RIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_RATING_INTENSITY Then
                            id = mBR.ReadInt32
                            GuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            name = mBR.ReadString
                            value = mBR.ReadSingle
                            comment = mBR.ReadString
                            AvaiForPW = mBR.ReadBoolean

                            If value < 0 Then
                                value = 0
                            End If

                            If value > 1 Then
                                value = 1
                            End If

                            R = New clsRating(id, name, value, RS, comment)
                            R.GuidID = GuidID 'C0261
                            R.AvailableForPW = AvaiForPW

                            RS.RatingSet.Add(R)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRatingScales " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RatingScalesCount = i 'C0271
        Return True
    End Function

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer 'C0765
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                    R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            Else
                                                node.Judgments.AddMeasureData(MD, True)
                                            End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263
            Dim nextHierarchyPos As Integer = mBR.BaseStream.Position - 4 + HJudgmentsChunkSize
            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                'If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                If H IsNot Nothing AndAlso (H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
                                Dim evalDiagonals As DiagonalsEvaluation = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If Not isUndef Then
                                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                    Dim parentNode As clsNode = Nothing
                                                    If node.IsTerminalNode Then
                                                        parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                    Else
                                                        parentNode = H.GetNodeByID(parentNodeGuidID)
                                                    End If
                                                    If parentNode IsNot Nothing Then
                                                        Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet

                                                        evalDiagonals = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                                        If node.IsTerminalNode Then
                                                            If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                                                evalDiagonals = DiagonalsEvaluation.deAll
                                                            End If
                                                        Else
                                                            If ForceObjectives And (outcomes.Count < ObjectivesForceLimit) Then
                                                                evalDiagonals = DiagonalsEvaluation.deAll
                                                            End If
                                                        End If

                                                        NodesList.Clear()
                                                        For Each R As clsRating In outcomes
                                                            Dim dummyNode As New clsNode
                                                            dummyNode.NodeGuidID = R.GuidID
                                                            NodesList.Add(dummyNode)
                                                        Next
                                                    End If
                                                End If

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                        If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                            madeCount += 1
                                                        Else
                                                            Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                            If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                madeCount += 1
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select

                                            If Not isUndef Then
                                                Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)
                                                If (ind <> -1) Then
                                                    If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                        madeCount += 1
                                                    End If
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While
                Else
                    mBR.BaseStream.Seek(nextHierarchyPos, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_20
    Inherits clsStreamModelReader_v_1_1_19
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_21
    Inherits clsStreamModelReader_v_1_1_20

    Protected Overrides Function ProcessRatingScalesChunk(ByRef RatingScalesCount As Integer) As Boolean 'C0271
        ' read rating scales

        ProjectManager.MeasureScales.RatingsScales.Clear()

        ' read the number of rating scales first
        Dim rsCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RS As clsRatingScale

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rsCount)
            subChunk = mBR.ReadInt32
            Dim RSChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_RATING_SCALE Then
                Dim rsID As Int32 = mBR.ReadInt32
                RS = New clsRatingScale(rsID)
                RS.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                RS.Name = mBR.ReadString
                RS.Comment = mBR.ReadString
                RS.IsOutcomes = mBR.ReadBoolean
                RS.IsPWofPercentages = mBR.ReadBoolean

                ProjectManager.MeasureScales.RatingsScales.Add(RS)

                ' read rating scale intensities
                subChunk = mBR.ReadInt32
                Dim RIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_RATING_INTENSITIES Then
                    Dim iCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim id As Int32
                    Dim GuidID As Guid 'C0261
                    Dim name As String
                    Dim value As Single
                    'Dim priority As Single
                    Dim comment As String
                    Dim AvaiForPW As Boolean

                    Dim R As clsRating

                    While (mStream.Position < mStream.Length - 1) And (j <> iCount)
                        subChunk = mBR.ReadInt32
                        Dim RIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_RATING_INTENSITY Then
                            id = mBR.ReadInt32
                            GuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            name = mBR.ReadString
                            value = mBR.ReadSingle
                            comment = mBR.ReadString
                            AvaiForPW = mBR.ReadBoolean

                            If value < 0 Then
                                value = 0
                            End If

                            If value > 1 Then
                                value = 1
                            End If

                            R = New clsRating(id, name, value, RS, comment)
                            R.GuidID = GuidID 'C0261
                            R.AvailableForPW = AvaiForPW

                            RS.RatingSet.Add(R)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRatingScales " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RatingScalesCount = i 'C0271
        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_22
    Inherits clsStreamModelReader_v_1_1_21

    Protected Overrides Function ProcessRatingScalesChunk(ByRef RatingScalesCount As Integer) As Boolean 'C0271
        ' read rating scales

        ProjectManager.MeasureScales.RatingsScales.Clear()

        ' read the number of rating scales first
        Dim rsCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RS As clsRatingScale

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rsCount)
            subChunk = mBR.ReadInt32
            Dim RSChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_RATING_SCALE Then
                Dim rsID As Int32 = mBR.ReadInt32
                RS = New clsRatingScale(rsID)
                RS.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                RS.Name = mBR.ReadString
                RS.Comment = mBR.ReadString
                RS.IsOutcomes = mBR.ReadBoolean
                RS.IsPWofPercentages = mBR.ReadBoolean
                RS.IsExpectedValues = mBR.ReadBoolean

                ProjectManager.MeasureScales.RatingsScales.Add(RS)

                ' read rating scale intensities
                subChunk = mBR.ReadInt32
                Dim RIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_RATING_INTENSITIES Then
                    Dim iCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim id As Int32
                    Dim GuidID As Guid 'C0261
                    Dim name As String
                    Dim value As Single
                    Dim value2 As Single
                    'Dim priority As Single
                    Dim comment As String
                    Dim AvaiForPW As Boolean

                    Dim R As clsRating

                    While (mStream.Position < mStream.Length - 1) And (j <> iCount)
                        subChunk = mBR.ReadInt32
                        Dim RIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_RATING_INTENSITY Then
                            id = mBR.ReadInt32
                            GuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            name = mBR.ReadString
                            value = mBR.ReadSingle
                            value2 = mBR.ReadSingle
                            comment = mBR.ReadString
                            AvaiForPW = mBR.ReadBoolean

                            If value < 0 Then
                                value = 0
                            End If

                            If value > 1 Then
                                value = 1
                            End If

                            R = New clsRating(id, name, value, RS, comment)
                            R.GuidID = GuidID 'C0261
                            R.AvailableForPW = AvaiForPW
                            R.Value2 = value2

                            RS.RatingSet.Add(R)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRatingScales " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RatingScalesCount = i 'C0271
        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_24
    Inherits clsStreamModelReader_v_1_1_22

    Protected Overrides Function ProcessRatingScalesChunk(ByRef RatingScalesCount As Integer) As Boolean 'C0271
        ' read rating scales

        ProjectManager.MeasureScales.RatingsScales.Clear()

        ' read the number of rating scales first
        Dim rsCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RS As clsRatingScale

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rsCount)
            subChunk = mBR.ReadInt32
            Dim RSChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_RATING_SCALE Then
                Dim rsID As Int32 = mBR.ReadInt32
                RS = New clsRatingScale(rsID)
                RS.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                RS.Name = mBR.ReadString
                RS.Comment = mBR.ReadString
                RS.Type = mBR.ReadInt32
                RS.IsOutcomes = mBR.ReadBoolean
                RS.IsPWofPercentages = mBR.ReadBoolean
                RS.IsExpectedValues = mBR.ReadBoolean

                ProjectManager.MeasureScales.RatingsScales.Add(RS)

                ' read rating scale intensities
                subChunk = mBR.ReadInt32
                Dim RIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_RATING_INTENSITIES Then
                    Dim iCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim id As Int32
                    Dim GuidID As Guid 'C0261
                    Dim name As String
                    Dim value As Single
                    Dim value2 As Single
                    'Dim priority As Single
                    Dim comment As String
                    Dim AvaiForPW As Boolean

                    Dim R As clsRating

                    While (mStream.Position < mStream.Length - 1) And (j <> iCount)
                        subChunk = mBR.ReadInt32
                        Dim RIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_RATING_INTENSITY Then
                            id = mBR.ReadInt32
                            GuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            name = mBR.ReadString
                            value = mBR.ReadSingle
                            value2 = mBR.ReadSingle
                            comment = mBR.ReadString
                            AvaiForPW = mBR.ReadBoolean

                            If value < 0 Then
                                value = 0
                            End If

                            If value > 1 Then
                                value = 1
                            End If

                            R = New clsRating(id, name, value, RS, comment)
                            R.GuidID = GuidID 'C0261
                            R.AvailableForPW = AvaiForPW
                            R.Value2 = value2

                            RS.RatingSet.Add(R)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRatingScales " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RatingScalesCount = i 'C0271
        Return True
    End Function

    Protected Overrides Function ProcessRegularUtilityCurvesChunk(ByRef RegularUtilityCurvesCount As Integer) As Boolean 'C0271
        ' read regular utility curves definitions

        ProjectManager.MeasureScales.RegularUtilityCurves.Clear()

        ' read the number of regular utility curves first
        Dim rucCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RUC As clsRegularUtilityCurve

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rucCount)
            subChunk = mBR.ReadInt32
            Dim RUCChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_REGULAR_UTILITY_CURVE Then
                Dim id As Int32 = mBR.ReadInt32
                Dim guidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0283
                Dim name As String = mBR.ReadString
                Dim low As Single = mBR.ReadSingle
                Dim high As Single = mBR.ReadSingle
                Dim curvature As Single = mBR.ReadSingle
                Dim isIncreasing As Boolean = mBR.ReadBoolean
                Dim comment As String = mBR.ReadString
                Dim type As ScaleType = mBR.ReadInt32

                RUC = New clsRegularUtilityCurve(id, low, high, curvature, curvature = 0, isIncreasing)

                RUC.GuidID = guidID 'C0283
                RUC.Name = name
                RUC.Comment = comment
                RUC.Type = type

                ProjectManager.MeasureScales.RegularUtilityCurves.Add(RUC)
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRegularUtilityCurves " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RegularUtilityCurvesCount = i 'C0271
        Return True
    End Function

    'Protected Function ProcessAdvancedUtilityCurvesChunk() As Boolean 'C0271
    Protected Overrides Function ProcessAdvancedUtilityCurvesChunk(ByRef AdvancedUtilityCurvesCount As Integer) As Boolean 'C0271
        ' read advanced utility curves definitions

        ProjectManager.MeasureScales.AdvancedUtilityCurves.Clear()

        ' read the number of advanced utility curves first
        Dim aucCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim AUC As clsAdvancedUtilityCurve

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> aucCount)
            subChunk = mBR.ReadInt32
            Dim AUCChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_ADVANCED_UTILITY_CURVE Then
                Dim id As Int32 = mBR.ReadInt32
                AUC = New clsAdvancedUtilityCurve(id)
                AUC.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                AUC.Name = mBR.ReadString
                AUC.InterpolationMethod = CType(mBR.ReadInt32, ECInterpolationMethod)
                AUC.Comment = mBR.ReadString
                AUC.Type = mBR.ReadInt32
                ProjectManager.MeasureScales.AdvancedUtilityCurves.Add(AUC)

                AUC.Points.Clear()

                ' read advanced utility curve points
                subChunk = mBR.ReadInt32
                Dim AUCPointsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_AUC_POINTS Then
                    Dim pointsCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> pointsCount)
                        Dim x As Single = mBR.ReadSingle
                        Dim y As Single = mBR.ReadSingle
                        AUC.AddUCPoint(x, y)
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessAdvancedUtilityCurves " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        AdvancedUtilityCurvesCount = i 'C0271
        Return True
    End Function

    'Protected Function ProcessStepFunctionsChunk() As Boolean 'C0271
    Protected Overrides Function ProcessStepFunctionsChunk(ByRef StepFunctionsCount As Integer) As Boolean 'C0271
        ' read step functions

        ProjectManager.MeasureScales.StepFunctions.Clear()

        ' read the number of step functions first
        Dim sfCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim SF As clsStepFunction

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> sfCount)
            subChunk = mBR.ReadInt32
            Dim SFChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_STEP_FUNCTION Then
                Dim sfID As Int32 = mBR.ReadInt32
                SF = New clsStepFunction(sfID)
                SF.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                SF.Name = mBR.ReadString
                SF.IsPiecewiseLinear = mBR.ReadBoolean 'C0329
                SF.Comment = mBR.ReadString
                SF.Type = mBR.ReadInt32

                ProjectManager.MeasureScales.StepFunctions.Add(SF)

                subChunk = mBR.ReadInt32
                Dim SIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_STEP_INTERVALS Then
                    Dim siCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    Dim SI As clsStepInterval
                    While (mStream.Position < mStream.Length - 1) And (j <> siCount)
                        subChunk = mBR.ReadInt32
                        Dim SIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_STEP_INTERVAL Then
                            Dim id As Int32 = mBR.ReadInt32
                            Dim name As String = mBR.ReadString
                            Dim low As Single = mBR.ReadSingle
                            Dim high As Single = mBR.ReadSingle
                            Dim value As Single = mBR.ReadSingle
                            Dim comment As String = mBR.ReadString
                            SI = New clsStepInterval(id, name, low, high, value, SF, comment)
                            SF.Intervals.Add(SI)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each SF In ProjectManager.MeasureScales.StepFunctions
            SF.SortByInterval()
        Next

        If mWriteStatus Then
            Debug.Print("ProcessStepFunctions " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        StepFunctionsCount = i 'C0271
        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_25
    Inherits clsStreamModelReader_v_1_1_24

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263
            Dim nextHierarchyPos As Integer = mBR.BaseStream.Position - 4 + HJudgmentsChunkSize
            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                'If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                If H IsNot Nothing AndAlso (H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
                                'Dim evalDiagonals As DiagonalsEvaluation = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If Not isUndef Then
                                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                    Dim parentNode As clsNode = Nothing
                                                    If node.IsTerminalNode Then
                                                        parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                    Else
                                                        parentNode = H.GetNodeByID(parentNodeGuidID)
                                                    End If
                                                    If parentNode IsNot Nothing Then
                                                        Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet
                                                        If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                                            'evalDiagonals = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                                            evalDiagonals = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                                        Else
                                                            evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                                        End If

                                                        If node.IsTerminalNode Then
                                                            If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                                                evalDiagonals = DiagonalsEvaluation.deAll
                                                            End If
                                                        Else
                                                            If ForceObjectives And (outcomes.Count < ObjectivesForceLimit) Then
                                                                evalDiagonals = DiagonalsEvaluation.deAll
                                                            End If
                                                        End If

                                                        NodesList.Clear()
                                                        For Each R As clsRating In outcomes
                                                            Dim dummyNode As New clsNode
                                                            dummyNode.NodeGuidID = R.GuidID
                                                            NodesList.Add(dummyNode)
                                                        Next
                                                    End If
                                                End If

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind1).RiskNodeType <> RiskNodeType.ntCategory AndAlso NodesList(ind2).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                                madeCount += 1
                                                            Else
                                                                Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                                If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                    (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select

                                            If Not isUndef Then
                                                Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)
                                                If (ind <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            madeCount += 1
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If alt.MeasurementScale IsNot Nothing Then
                                    validMeasurementTypeAndScale = alt.MeasurementScale.GuidID.Equals(MSGuid)
                                Else
                                    validMeasurementTypeAndScale = False
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim MD As clsRatingMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                        Dim directValue As Single
                                        Dim ratingGuidID As Guid

                                        If isDirectValue Then
                                            directValue = mBR.ReadSingle
                                        Else
                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                        End If

                                        mBR.ReadString()
                                        mBR.ReadInt64()

                                        If ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                            madeCount += 1
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(nextHierarchyPos, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer 'C0765
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                    R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            Else
                                                node.Judgments.AddMeasureData(MD, False)
                                            End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean = True
                                'If alt.MeasurementScale IsNot Nothing Then
                                '    validMeasurementTypeAndScale = alt.MeasurementScale.GuidID.Equals(MSGuid)
                                'Else
                                '    validMeasurementTypeAndScale = False
                                'End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim MD As clsRatingMeasureData = Nothing

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean

                                        If isUndef Then
                                            mBR.ReadBoolean()
                                            mBR.ReadSingle()
                                        Else
                                            Dim isDirectValue As Boolean = mBR.ReadBoolean

                                            Dim rating As clsRating
                                            Dim directValue As Single
                                            Dim ratingGuidID As Guid

                                            If isDirectValue Then
                                                directValue = mBR.ReadSingle

                                                rating = New clsRating
                                                rating.ID = -1
                                                rating.Name = "Direct Entry"
                                                rating.Value = directValue
                                            Else
                                                ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                            End If
                                            MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                        End If

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                            res += 1
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_27
    Inherits clsStreamModelReader_v_1_1_25

    Public Overrides Function ReadUserJudgmentsControls(User As ECCore.ECTypes.clsUser) As Integer
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim ControlsCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> ControlsCount)
            chunk = mBR.ReadInt32
            Dim ControlJudgmentsChunkSize As Integer = mBR.ReadInt32

            If chunk = CHUNK_CONTROL_JUDGMENTS Then
                Dim cGuidID As Guid = New Guid(mBR.ReadBytes(16))
                Dim control As clsControl = ProjectManager.Controls.GetControlByID(cGuidID)

                If control IsNot Nothing Then
                    Dim assignmentsCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> assignmentsCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim AssignmentJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextAssignmentJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + AssignmentJudgmentsChunkSize

                        If subChunk = CHUNK_CONTROL_ASSIGNMENT_JUDGMENTS Then
                            Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim assignment As clsControlAssignment = control.GetAssignmentByID(aGuidID)

                            If assignment IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32

                                If MT = assignment.MeasurementType Then
                                    Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                    Dim jCount As Int32 = mBR.ReadInt32

                                    Dim i As Integer = 0

                                    Dim MD As clsNonPairwiseMeasureData
                                    assignment.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                    assignment.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean

                                        Dim CtrlObjectiveID As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim CtrlEventID As Guid = New Guid(mBR.ReadBytes(16))

                                        Select Case assignment.MeasurementType
                                            Case ECMeasureType.mtRatings
                                                Dim RS As clsRatingScale = ProjectManager.MeasureScales.GetRatingScaleByID(MSGuid)

                                                If isUndef Then
                                                    mBR.ReadBoolean()
                                                    mBR.ReadSingle()
                                                Else
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                    Dim rating As clsRating
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle

                                                        rating = New clsRating
                                                        rating.ID = -1
                                                        rating.Name = "Direct Entry from EC11.5"
                                                        rating.Value = directValue
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                        rating = RS.GetRatingByID(ratingGuidID)
                                                    End If
                                                    MD = New clsRatingMeasureData(-1, -1, User.UserID, rating, RS, If(Not isUndef, rating Is Nothing, isUndef))
                                                End If
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                Dim UC As clsRegularUtilityCurve = ProjectManager.MeasureScales.GetRegularUtilityCurveByID(MSGuid)
                                                Dim ucDataValue As Single = mBR.ReadSingle
                                                MD = New clsUtilityCurveMeasureData(-1, -1, User.UserID, ucDataValue, UC, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                            Case ECMeasureType.mtStep
                                                Dim SF As clsStepFunction = ProjectManager.MeasureScales.GetStepFunctionByID(MSGuid)
                                                Dim value As Single = mBR.ReadSingle
                                                MD = New clsStepMeasureData(-1, -1, User.UserID, value, SF, If(Not isUndef, Single.IsNaN(value), isUndef))
                                            Case ECMeasureType.mtDirect
                                                Dim directValue As Single = mBR.ReadSingle
                                                MD = New clsDirectMeasureData(-1, -1, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                        End Select
                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            MD.CtrlObjectiveID = CtrlObjectiveID
                                            MD.CtrlEventID = CtrlEventID

                                            assignment.Judgments.AddMeasureData(MD)

                                            res += 1
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextAssignmentJudgmentsChunkPosition, SeekOrigin.Begin)
                                End If
                            Else
                                mBR.BaseStream.Seek(NextAssignmentJudgmentsChunkPosition, SeekOrigin.Begin)
                            End If
                        End If
                        k += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 - 16 + ControlJudgmentsChunkSize, SeekOrigin.Begin)
                End If
            Else
                mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + ControlJudgmentsChunkSize, SeekOrigin.Begin)
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        Return res
    End Function

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer 'C0765
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                    R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = node.NodeID
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            Else
                                                node.Judgments.AddMeasureData(MD, False)
                                            End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean = True
                                'If alt.MeasurementScale IsNot Nothing Then
                                '    validMeasurementTypeAndScale = alt.MeasurementScale.GuidID.Equals(MSGuid)
                                'Else
                                '    validMeasurementTypeAndScale = False
                                'End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim MD As clsRatingMeasureData = Nothing

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean

                                        If isUndef Then
                                            mBR.ReadBoolean()
                                            mBR.ReadSingle()
                                        Else
                                            Dim isDirectValue As Boolean = mBR.ReadBoolean

                                            Dim rating As clsRating
                                            Dim directValue As Single
                                            Dim ratingGuidID As Guid

                                            If isDirectValue Then
                                                directValue = mBR.ReadSingle

                                                rating = New clsRating
                                                rating.ID = -1
                                                rating.Name = "Direct Entry"
                                                rating.Value = directValue
                                            Else
                                                ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                            End If
                                            MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                        End If

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                            res += 1
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Public Overrides Function ReadControlsPermissions(UserID As Integer) As Boolean
        If UserID <> DUMMY_PERMISSIONS_USER_ID Then
            ProjectManager.ControlsRoles.CleanUpUserRoles(UserID)
        End If
        mStream.Seek(0, SeekOrigin.Begin)
        mBR = New BinaryReader(mStream)

        Dim chunk As Int32
        Dim res As Boolean = True

        While (mStream.Position < mStream.Length - 1) And res
            chunk = mBR.ReadInt32
            Dim ChunkSize As Integer = mBR.ReadInt32

            Dim curPos As Integer = mBR.BaseStream.Position

            Select Case chunk
                Case CHUNK_CONTROLS_PERMISSIONS
                    res = ProcessControlsPermissions(UserID)
            End Select
        End While

        mBR.Close()
        Return True
    End Function

    Protected Overrides Function ProcessControlsPermissions(ByVal UserID As Integer) As Boolean
        Dim controlsCount As Integer = mBR.ReadInt32
        Dim i As Integer = 0

        Dim ControlGuidID As Guid
        Dim control As clsControl

        Dim RestrictedControlsList As New List(Of Guid)

        While (mStream.Position < mStream.Length - 1) And (i <> controlsCount)
            ControlGuidID = New Guid(mBR.ReadBytes(16))

            control = ProjectManager.Controls.GetControlByID(ControlGuidID)
            If control IsNot Nothing Then
                RestrictedControlsList.Add(ControlGuidID)
            End If
            i += 1
        End While

        If RestrictedControlsList.Count > 0 Then
            ProjectManager.ControlsRoles.SetObjectivesRoles(UserID, RestrictedControlsList, RolesValueType.rvtRestricted)
        End If

        ' reading allowed nodes
        controlsCount = mBR.ReadInt32

        Dim AllowedControlsList As New List(Of Guid)
        i = 0

        While (mStream.Position < mStream.Length - 1) And (i <> controlsCount)
            ControlGuidID = New Guid(mBR.ReadBytes(16))

            control = ProjectManager.Controls.GetControlByID(ControlGuidID)
            If control IsNot Nothing Then
                AllowedControlsList.Add(ControlGuidID)
            End If
            i += 1
        End While

        If AllowedControlsList.Count > 0 Then
            ProjectManager.ControlsRoles.SetObjectivesRoles(UserID, AllowedControlsList, RolesValueType.rvtAllowed)
        End If

        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_28
    Inherits clsStreamModelReader_v_1_1_27

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                    R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = node.NodeID
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            Else
                                                node.Judgments.AddMeasureData(MD, False)
                                            End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim MD As clsNonPairwiseMeasureData = Nothing
                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean

                                        Select Case alt.MeasureType
                                            Case ECMeasureType.mtRatings
                                                If isUndef Then
                                                    mBR.ReadBoolean()
                                                    mBR.ReadSingle()
                                                Else
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                    Dim rating As clsRating
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle

                                                        rating = New clsRating
                                                        rating.ID = -1
                                                        rating.Name = "Direct Entry"
                                                        rating.Value = directValue
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                        rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                    End If
                                                    MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                End If
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                Dim ucDataValue As Single = mBR.ReadSingle
                                                MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                            Case ECMeasureType.mtStep
                                                Dim value As Single = mBR.ReadSingle
                                                MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                            Case ECMeasureType.mtDirect
                                                Dim directValue As Single = mBR.ReadSingle
                                                MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                            res += 1
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263
            Dim nextHierarchyPos As Integer = mBR.BaseStream.Position - 4 + HJudgmentsChunkSize
            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                'If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                If H IsNot Nothing AndAlso (H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
                                'Dim evalDiagonals As DiagonalsEvaluation = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If Not isUndef Then
                                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                    Dim parentNode As clsNode = Nothing
                                                    If node.IsTerminalNode Then
                                                        parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                    Else
                                                        parentNode = H.GetNodeByID(parentNodeGuidID)
                                                    End If
                                                    If parentNode IsNot Nothing Then
                                                        Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet
                                                        If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                                            'evalDiagonals = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                                            evalDiagonals = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                                        Else
                                                            evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                                        End If

                                                        If node.IsTerminalNode Then
                                                            If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                                                evalDiagonals = DiagonalsEvaluation.deAll
                                                            End If
                                                        Else
                                                            If ForceObjectives And (outcomes.Count < ObjectivesForceLimit) Then
                                                                evalDiagonals = DiagonalsEvaluation.deAll
                                                            End If
                                                        End If

                                                        NodesList.Clear()
                                                        For Each R As clsRating In outcomes
                                                            Dim dummyNode As New clsNode
                                                            dummyNode.NodeGuidID = R.GuidID
                                                            NodesList.Add(dummyNode)
                                                        Next
                                                    End If
                                                End If

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind1).RiskNodeType <> RiskNodeType.ntCategory AndAlso NodesList(ind2).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                                madeCount += 1
                                                            Else
                                                                Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                                If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                    (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select

                                            If Not isUndef Then
                                                Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)
                                                If (ind <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            madeCount += 1
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim MD As clsRatingMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean

                                        Select Case alt.MeasureType
                                            Case ECMeasureType.mtRatings
                                                Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                Dim directValue As Single
                                                Dim ratingGuidID As Guid

                                                If isDirectValue Then
                                                    directValue = mBR.ReadSingle
                                                Else
                                                    ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                End If
                                            Case Else
                                                Dim value As Single = mBR.ReadSingle
                                        End Select

                                        mBR.ReadString()
                                        mBR.ReadInt64()

                                        If Not isUndef And ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                            madeCount += 1
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(nextHierarchyPos, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_29
    Inherits clsStreamModelReader_v_1_1_28

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim outcomesNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    outcomesNode = node.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                                    If node.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                        R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                        R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                    End If
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            Else
                                                node.Judgments.AddMeasureData(MD, False)
                                            End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim MD As clsNonPairwiseMeasureData = Nothing
                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean

                                        Select Case alt.MeasureType
                                            Case ECMeasureType.mtRatings
                                                If isUndef Then
                                                    mBR.ReadBoolean()
                                                    mBR.ReadSingle()
                                                Else
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                    Dim rating As clsRating
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle

                                                        rating = New clsRating
                                                        rating.ID = -1
                                                        rating.Name = "Direct Entry"
                                                        rating.Value = directValue
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                        rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                    End If
                                                    MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                End If
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                Dim ucDataValue As Single = mBR.ReadSingle
                                                MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                            Case ECMeasureType.mtStep
                                                Dim value As Single = mBR.ReadSingle
                                                MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                            Case ECMeasureType.mtDirect
                                                Dim directValue As Single = mBR.ReadSingle
                                                MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                            res += 1
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263
            Dim nextHierarchyPos As Integer = mBR.BaseStream.Position - 4 + HJudgmentsChunkSize
            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                'If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                If H IsNot Nothing AndAlso (H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
                                'Dim evalDiagonals As DiagonalsEvaluation = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        Dim wrtNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If Not isUndef Then
                                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                    NodesList.Clear()
                                                    If wrtNodeGuid.Equals(node.NodeGuidID) Then
                                                        Dim parentNode As clsNode = Nothing
                                                        If node.IsTerminalNode Then
                                                            parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                        Else
                                                            parentNode = H.GetNodeByID(parentNodeGuidID)
                                                        End If
                                                        If parentNode IsNot Nothing Then
                                                            Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet
                                                            If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                                                'evalDiagonals = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                                                evalDiagonals = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                                            Else
                                                                evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                                            End If

                                                            If node.IsTerminalNode Then
                                                                If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                                                    evalDiagonals = DiagonalsEvaluation.deAll
                                                                End If
                                                            Else
                                                                If ForceObjectives And (outcomes.Count < ObjectivesForceLimit) Then
                                                                    evalDiagonals = DiagonalsEvaluation.deAll
                                                                End If
                                                            End If

                                                            For Each R As clsRating In outcomes
                                                                Dim dummyNode As New clsNode
                                                                dummyNode.NodeGuidID = R.GuidID
                                                                NodesList.Add(dummyNode)
                                                            Next
                                                        End If
                                                    End If
                                                End If

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind1).RiskNodeType <> RiskNodeType.ntCategory AndAlso NodesList(ind2).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                                madeCount += 1
                                                            Else
                                                                Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                                If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                    (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select

                                            If Not isUndef Then
                                                Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)
                                                If (ind <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            madeCount += 1
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim MD As clsRatingMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim isUndef As Boolean = mBR.ReadBoolean

                                        Select Case alt.MeasureType
                                            Case ECMeasureType.mtRatings
                                                Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                Dim directValue As Single
                                                Dim ratingGuidID As Guid

                                                If isDirectValue Then
                                                    directValue = mBR.ReadSingle
                                                Else
                                                    ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                End If
                                            Case Else
                                                Dim value As Single = mBR.ReadSingle
                                        End Select

                                        mBR.ReadString()
                                        mBR.ReadInt64()

                                        If Not isUndef And ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                            madeCount += 1
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(nextHierarchyPos, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function

End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_30
    Inherits clsStreamModelReader_v_1_1_29

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim outcomesNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    outcomesNode = node.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                                    If node.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                        R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                        R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                    End If
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            Else
                                                node.Judgments.AddMeasureData(MD, False)
                                            End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0 'C0765

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263
            Dim nextHierarchyPos As Integer = mBR.BaseStream.Position - 4 + HJudgmentsChunkSize
            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                'If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                If H IsNot Nothing AndAlso (H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
                                'Dim evalDiagonals As DiagonalsEvaluation = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        Dim wrtNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If Not isUndef Then
                                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                    NodesList.Clear()
                                                    If wrtNodeGuid.Equals(node.NodeGuidID) Then
                                                        Dim parentNode As clsNode = Nothing
                                                        If node.IsTerminalNode Then
                                                            parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                        Else
                                                            parentNode = H.GetNodeByID(parentNodeGuidID)
                                                        End If
                                                        If parentNode IsNot Nothing Then
                                                            Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet
                                                            If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                                                'evalDiagonals = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                                                evalDiagonals = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                                            Else
                                                                evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                                            End If

                                                            If node.IsTerminalNode Then
                                                                If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                                                    evalDiagonals = DiagonalsEvaluation.deAll
                                                                End If
                                                            Else
                                                                If ForceObjectives And (outcomes.Count < ObjectivesForceLimit) Then
                                                                    evalDiagonals = DiagonalsEvaluation.deAll
                                                                End If
                                                            End If

                                                            For Each R As clsRating In outcomes
                                                                Dim dummyNode As New clsNode
                                                                dummyNode.NodeGuidID = R.GuidID
                                                                NodesList.Add(dummyNode)
                                                            Next
                                                        End If
                                                    End If
                                                End If

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind1).RiskNodeType <> RiskNodeType.ntCategory AndAlso NodesList(ind2).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                                madeCount += 1
                                                            Else
                                                                Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                                If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                    (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select

                                            If Not isUndef Then
                                                Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)
                                                If (ind <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            madeCount += 1
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        Dim outcomes As List(Of clsRating) = CType(alt.MeasurementScale, clsRatingScale).RatingSet
                                        Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, alt.NodeGuidID)
                                        If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                            evalDiagonals = DiagonalsEvaluation.deAll
                                        End If
                                        Dim NodesList As New List(Of clsNode)
                                        For Each R As clsRating In outcomes
                                            Dim dummyNode As New clsNode
                                            dummyNode.NodeGuidID = R.GuidID
                                            NodesList.Add(dummyNode)
                                        Next

                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                'parentNode.PWOutcomesJudgments.AddMeasureData(MD)

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If alt.Enabled AndAlso Not alt.DisabledForUser(User.UserID) AndAlso ProjectManager.UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, User.UserID) Then
                                                        If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                            madeCount += 1
                                                        Else
                                                            Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                            If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                If Not isUndef And ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If


                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                'alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                                If Not isUndef And ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                                    madeCount += 1
                                                End If

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If

                                'If validMeasurementTypeAndScale Then
                                '    Dim jCount As Int32 = mBR.ReadInt32
                                '    Dim i As Integer = 0

                                '    Dim MD As clsRatingMeasureData

                                '    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                '        MD = Nothing
                                '        Dim isUndef As Boolean = mBR.ReadBoolean

                                '        Select Case alt.MeasureType
                                '            Case ECMeasureType.mtRatings
                                '                Dim isDirectValue As Boolean = mBR.ReadBoolean

                                '                Dim directValue As Single
                                '                Dim ratingGuidID As Guid

                                '                If isDirectValue Then
                                '                    directValue = mBR.ReadSingle
                                '                Else
                                '                    ratingGuidID = New Guid(mBR.ReadBytes(16))
                                '                End If
                                '            Case Else
                                '                Dim value As Single = mBR.ReadSingle
                                '        End Select

                                '        mBR.ReadString()
                                '        mBR.ReadInt64()

                                '        If Not isUndef And ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                '            madeCount += 1
                                '        End If

                                '        i += 1
                                '    End While
                                'Else
                                '    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                'End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(nextHierarchyPos, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function

    Public Overrides Function AddJudgmentsToCombined(User As ECCore.ECTypes.clsUser, Group As ECCore.Groups.clsCombinedGroup, Optional Hierarchy As clsHierarchy = Nothing) As Integer
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        'node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        'node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim CMD As clsCustomMeasureData = Nothing
                                        Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim outcomesNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    outcomesNode = node.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                                    If node.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                        R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                        R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                    End If
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                        If MD IsNot Nothing Then
                                                            CMD = CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(firstNode.NodeID, secondNode.NodeID, Group.CombinedUserID)
                                                        End If
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                        If MD IsNot Nothing Then
                                                            CMD = CType(parentNode.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(R1.ID, R2.ID, Group.CombinedUserID)
                                                            CType(CMD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                        End If
                                                    End If
                                                End If

                                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                                    If value <> 0 Then
                                                        Dim bIncludeJudgment As Boolean = True
                                                        If Not node.IsAllowed(User.UserID) Then
                                                            bIncludeJudgment = False
                                                        Else
                                                            If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                                                If node.IsTerminalNode And node.MeasureType = ECMeasureType.mtPairwise Then
                                                                    If Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, firstNodeGuidID, User.UserID) OrElse Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, secondNodeGuidID, User.UserID) Then
                                                                        bIncludeJudgment = False
                                                                    End If
                                                                End If
                                                            End If
                                                        End If

                                                        If bIncludeJudgment Then
                                                            If advantage = -1 Then
                                                                value = 1 / value
                                                            End If
                                                            Dim mpwData As clsPairwiseMeasureData = CType(MD, clsPairwiseMeasureData)
                                                            Dim cpwData As clsPairwiseMeasureData = CType(CMD, clsPairwiseMeasureData)
                                                            If cpwData.FirstNodeID <> mpwData.FirstNodeID Then
                                                                value = 1 / value
                                                            End If
                                                            cpwData.AggregatedValue *= value
                                                            cpwData.UsersCount += 1
                                                        End If
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                End Select

                                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                                    Dim bIncludeJudgment As Boolean = True
                                                    If Not node.IsAllowed(User.UserID) Then
                                                        bIncludeJudgment = False
                                                    Else
                                                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                                            If node.IsTerminalNode Then
                                                                If Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, childNodeGuidID, User.UserID) Then
                                                                    bIncludeJudgment = False
                                                                End If
                                                            End If
                                                        End If
                                                    End If

                                                    If bIncludeJudgment Then
                                                        Dim userValue As Single
                                                        Select Case node.MeasureType
                                                            Case ECMeasureType.mtDirect, ECMeasureType.mtRatings
                                                                userValue = CType(MD, clsNonPairwiseMeasureData).SingleValue
                                                            Case Else
                                                                userValue = CType(CType(MD, clsNonPairwiseMeasureData).ObjectValue, Single)
                                                        End Select

                                                        CMD.AggregatedValue += userValue
                                                        CMD.UsersCount += 1
                                                    End If
                                                End If
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            'If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            '    parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            'Else
                                            '    node.Judgments.AddMeasureData(MD, False)
                                            'End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_32
    Inherits clsStreamModelReader_v_1_1_30

    Protected Overrides Function ProcessRatingScalesChunk(ByRef RatingScalesCount As Integer) As Boolean 'C0271
        ' read rating scales

        ProjectManager.MeasureScales.RatingsScales.Clear()

        ' read the number of rating scales first
        Dim rsCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RS As clsRatingScale

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rsCount)
            subChunk = mBR.ReadInt32
            Dim RSChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_RATING_SCALE Then
                Dim rsID As Int32 = mBR.ReadInt32
                RS = New clsRatingScale(rsID)
                RS.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                RS.Name = mBR.ReadString
                RS.Comment = mBR.ReadString
                RS.Type = mBR.ReadInt32
                RS.IsOutcomes = mBR.ReadBoolean
                RS.IsPWofPercentages = mBR.ReadBoolean
                RS.IsExpectedValues = mBR.ReadBoolean
                RS.IsDefault = mBR.ReadBoolean

                ProjectManager.MeasureScales.RatingsScales.Add(RS)

                ' read rating scale intensities
                subChunk = mBR.ReadInt32
                Dim RIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_RATING_INTENSITIES Then
                    Dim iCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0

                    Dim id As Int32
                    Dim GuidID As Guid 'C0261
                    Dim name As String
                    Dim value As Single
                    Dim value2 As Single
                    'Dim priority As Single
                    Dim comment As String
                    Dim AvaiForPW As Boolean

                    Dim R As clsRating

                    While (mStream.Position < mStream.Length - 1) And (j <> iCount)
                        subChunk = mBR.ReadInt32
                        Dim RIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_RATING_INTENSITY Then
                            id = mBR.ReadInt32
                            GuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            name = mBR.ReadString
                            value = mBR.ReadSingle
                            value2 = mBR.ReadSingle
                            comment = mBR.ReadString
                            AvaiForPW = mBR.ReadBoolean

                            If value < 0 Then
                                value = 0
                            End If

                            If value > 1 Then
                                value = 1
                            End If

                            R = New clsRating(id, name, value, RS, comment)
                            R.GuidID = GuidID 'C0261
                            R.AvailableForPW = AvaiForPW
                            R.Value2 = value2

                            RS.RatingSet.Add(R)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRatingScales " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RatingScalesCount = i 'C0271
        Return True
    End Function

    Protected Overrides Function ProcessRegularUtilityCurvesChunk(ByRef RegularUtilityCurvesCount As Integer) As Boolean 'C0271
        ' read regular utility curves definitions

        ProjectManager.MeasureScales.RegularUtilityCurves.Clear()

        ' read the number of regular utility curves first
        Dim rucCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim RUC As clsRegularUtilityCurve

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> rucCount)
            subChunk = mBR.ReadInt32
            Dim RUCChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_REGULAR_UTILITY_CURVE Then
                Dim id As Int32 = mBR.ReadInt32
                Dim guidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0283
                Dim name As String = mBR.ReadString
                Dim low As Single = mBR.ReadSingle
                Dim high As Single = mBR.ReadSingle
                Dim curvature As Single = mBR.ReadSingle
                Dim isIncreasing As Boolean = mBR.ReadBoolean
                Dim comment As String = mBR.ReadString
                Dim type As ScaleType = mBR.ReadInt32
                Dim IsDefault As Boolean = mBR.ReadBoolean

                RUC = New clsRegularUtilityCurve(id, low, high, curvature, curvature = 0, isIncreasing)

                RUC.GuidID = guidID 'C0283
                RUC.Name = name
                RUC.Comment = comment
                RUC.Type = type
                RUC.IsDefault = IsDefault

                ProjectManager.MeasureScales.RegularUtilityCurves.Add(RUC)
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessRegularUtilityCurves " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        RegularUtilityCurvesCount = i 'C0271
        Return True
    End Function

    'Protected Function ProcessAdvancedUtilityCurvesChunk() As Boolean 'C0271
    Protected Overrides Function ProcessAdvancedUtilityCurvesChunk(ByRef AdvancedUtilityCurvesCount As Integer) As Boolean 'C0271
        ' read advanced utility curves definitions

        ProjectManager.MeasureScales.AdvancedUtilityCurves.Clear()

        ' read the number of advanced utility curves first
        Dim aucCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim AUC As clsAdvancedUtilityCurve

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> aucCount)
            subChunk = mBR.ReadInt32
            Dim AUCChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_ADVANCED_UTILITY_CURVE Then
                Dim id As Int32 = mBR.ReadInt32
                AUC = New clsAdvancedUtilityCurve(id)
                AUC.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                AUC.Name = mBR.ReadString
                AUC.InterpolationMethod = CType(mBR.ReadInt32, ECInterpolationMethod)
                AUC.Comment = mBR.ReadString
                AUC.Type = mBR.ReadInt32
                AUC.IsDefault = mBR.ReadBoolean

                ProjectManager.MeasureScales.AdvancedUtilityCurves.Add(AUC)

                AUC.Points.Clear()

                ' read advanced utility curve points
                subChunk = mBR.ReadInt32
                Dim AUCPointsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_AUC_POINTS Then
                    Dim pointsCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> pointsCount)
                        Dim x As Single = mBR.ReadSingle
                        Dim y As Single = mBR.ReadSingle
                        AUC.AddUCPoint(x, y)
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessAdvancedUtilityCurves " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        AdvancedUtilityCurvesCount = i 'C0271
        Return True
    End Function

    'Protected Function ProcessStepFunctionsChunk() As Boolean 'C0271
    Protected Overrides Function ProcessStepFunctionsChunk(ByRef StepFunctionsCount As Integer) As Boolean 'C0271
        ' read step functions

        ProjectManager.MeasureScales.StepFunctions.Clear()

        ' read the number of step functions first
        Dim sfCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim SF As clsStepFunction

        Dim subChunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> sfCount)
            subChunk = mBR.ReadInt32
            Dim SFChunkSize As Integer = mBR.ReadInt32 'C0263

            If subChunk = CHUNK_STEP_FUNCTION Then
                Dim sfID As Int32 = mBR.ReadInt32
                SF = New clsStepFunction(sfID)
                SF.GuidID = New Guid(mBR.ReadBytes(16)) 'C0283
                SF.Name = mBR.ReadString
                SF.IsPiecewiseLinear = mBR.ReadBoolean 'C0329
                SF.Comment = mBR.ReadString
                SF.Type = mBR.ReadInt32
                SF.IsDefault = mBR.ReadBoolean

                ProjectManager.MeasureScales.StepFunctions.Add(SF)

                subChunk = mBR.ReadInt32
                Dim SIsChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_STEP_INTERVALS Then
                    Dim siCount As Int32 = mBR.ReadInt32
                    Dim j As Integer = 0
                    Dim SI As clsStepInterval
                    While (mStream.Position < mStream.Length - 1) And (j <> siCount)
                        subChunk = mBR.ReadInt32
                        Dim SIChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_STEP_INTERVAL Then
                            Dim id As Int32 = mBR.ReadInt32
                            Dim name As String = mBR.ReadString
                            Dim low As Single = mBR.ReadSingle
                            Dim high As Single = mBR.ReadSingle
                            Dim value As Single = mBR.ReadSingle
                            Dim comment As String = mBR.ReadString
                            SI = New clsStepInterval(id, name, low, high, value, SF, comment)
                            SF.Intervals.Add(SI)
                        Else
                            Return False
                        End If
                        j += 1
                    End While
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each SF In ProjectManager.MeasureScales.StepFunctions
            SF.SortByInterval()
        Next

        If mWriteStatus Then
            Debug.Print("ProcessStepFunctions " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        StepFunctionsCount = i 'C0271
        Return True
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_33
    Inherits clsStreamModelReader_v_1_1_32

    Public Overrides Function AddJudgmentsToCombined(User As ECCore.ECTypes.clsUser, Group As ECCore.Groups.clsCombinedGroup, Optional Hierarchy As clsHierarchy = Nothing) As Integer
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing And ((Hierarchy Is Nothing) Or (Hierarchy IsNot Nothing And Hierarchy Is H)) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        'node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        'node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim CMD As clsCustomMeasureData = Nothing
                                        Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim outcomesNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    outcomesNode = node.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                                    If node.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                        R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                        R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                    End If
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                        If MD IsNot Nothing Then
                                                            CMD = CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(firstNode.NodeID, secondNode.NodeID, Group.CombinedUserID)
                                                        End If
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                        If MD IsNot Nothing Then
                                                            CMD = CType(parentNode.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(R1.ID, R2.ID, Group.CombinedUserID)
                                                            CType(CMD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                        End If
                                                    End If
                                                End If

                                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                                    If value <> 0 Then
                                                        Dim bIncludeJudgment As Boolean = True
                                                        If Not node.IsAllowed(User.UserID) Then
                                                            bIncludeJudgment = False
                                                        Else
                                                            If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                                                If node.IsTerminalNode And node.MeasureType = ECMeasureType.mtPairwise Then
                                                                    If Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, firstNodeGuidID, User.UserID) OrElse Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, secondNodeGuidID, User.UserID) Then
                                                                        bIncludeJudgment = False
                                                                    End If
                                                                End If
                                                            End If
                                                        End If

                                                        If bIncludeJudgment Then
                                                            If advantage = -1 Then
                                                                value = 1 / value
                                                            End If
                                                            Dim mpwData As clsPairwiseMeasureData = CType(MD, clsPairwiseMeasureData)
                                                            Dim cpwData As clsPairwiseMeasureData = CType(CMD, clsPairwiseMeasureData)
                                                            If cpwData.FirstNodeID <> mpwData.FirstNodeID Then
                                                                value = 1 / value
                                                            End If
                                                            cpwData.AggregatedValue *= value
                                                            cpwData.UsersCount += 1
                                                        End If
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                End Select

                                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                                    Dim bIncludeJudgment As Boolean = True
                                                    If Not node.IsAllowed(User.UserID) Then
                                                        bIncludeJudgment = False
                                                    Else
                                                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                                            If node.IsTerminalNode Then
                                                                If Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, childNodeGuidID, User.UserID) Then
                                                                    bIncludeJudgment = False
                                                                End If
                                                            End If
                                                        End If
                                                    End If

                                                    If bIncludeJudgment Then
                                                        Dim userValue As Single
                                                        Select Case node.MeasureType
                                                            Case ECMeasureType.mtDirect, ECMeasureType.mtRatings
                                                                userValue = CType(MD, clsNonPairwiseMeasureData).SingleValue
                                                            Case Else
                                                                userValue = CType(CType(MD, clsNonPairwiseMeasureData).ObjectValue, Single)
                                                        End Select

                                                        CMD.AggregatedValue += userValue
                                                        CMD.UsersCount += 1
                                                    End If
                                                End If
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            'If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            '    parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            'Else
                                            '    node.Judgments.AddMeasureData(MD, False)
                                            'End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean 'C0626
        ' read hierarchies with nodes

        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()
        ProjectManager.MeasureHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim bReadHiearchyChunk As Boolean = True 'C0738

        Dim HierarchyChunkSize As Integer 'C0738

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            If bReadHiearchyChunk Then 'C0738
                chunk = mBR.ReadInt32
                HierarchyChunkSize = mBR.ReadInt32 'C0738
            End If
            bReadHiearchyChunk = True 'C0738 - reset reading chunk (this can be set to False later when finding out that the model is corrupted - getting hierarchy chunk instead of node chunk)

            'Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263 'C0738

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                    Case ECHierarchyType.htMeasure
                        H = ProjectManager.AddMeasureHierarchy
                End Select

                H.Nodes.Clear()
                H.ResetNodesDictionaries()

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    H.Nodes.Capacity = nodesCount
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then 'C0738
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32

                            Dim ParentsCount As Integer = mBR.ReadInt32
                            If ParentsCount > 0 Then node.ParentNodesGuids = New List(Of Guid) 'A1000
                            For k As Integer = 1 To ParentsCount
                                Dim pGuid As Guid = New Guid(mBR.ReadBytes(16))
                                node.ParentNodesGuids.Add(pGuid) 'A1000
                            Next

                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If (H.HierarchyType <> ECHierarchyType.htModel) Or
                               ((H.HierarchyType = ECHierarchyType.htModel) And ((node.ParentNodeID <> -1) Or ((node.ParentNodeID = -1) AndAlso (H.Nodes.Count = 0)))) Then 'C0731
                                If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, node.ParentNodeID, True) 'C0383
                            End If

                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            'If node.IsAlternative And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                            If node.IsAlternative And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                                node.MeasureType = ECMeasureType.mtRatings
                                node.RatingScaleID = -1
                            End If

                            Debug.Print(node.NodeName + ": " + node.MeasureType.ToString)

                            If node.MeasurementScale IsNot Nothing Then
                                Select Case node.MeasureType
                                    Case ECMeasureType.mtRatings
                                        Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRatingScale).Name)
                                    Case ECMeasureType.mtRegularUtilityCurve
                                        Debug.Print(node.NodeName + ": " + CType(node.MeasurementScale, clsRegularUtilityCurve).Name + " (" + CType(node.MeasurementScale, clsRegularUtilityCurve).IsIncreasing.ToString + ")")
                                End Select
                            End If

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32

                            node.Comment = mBR.ReadString

                            Dim cost As String = mBR.ReadString
                            If (cost <> UNDEFINED_COST_VALUE) And (node.IsAlternative) Then
                                node.Tag = cost
                            End If

                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            'C0738===
                            If subChunk = CHUNK_HIERARCHY Then
                                bReadHiearchyChunk = False
                                chunk = CHUNK_HIERARCHY
                                HierarchyChunkSize = NodeChunkSize
                                j = nodesCount - 1 'to skip nodes after increment below
                            Else
                                Return False
                            End If
                            'C0738===

                            'Return False 'C0738
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
            H.FixChildrenLinks()
        Next

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263
            Dim nextHierarchyPos As Integer = mBR.BaseStream.Position - 4 + HJudgmentsChunkSize
            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                'If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                If H IsNot Nothing AndAlso (H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If (node IsNot Nothing) AndAlso ((Not node.IsTerminalNode And PP.EvaluateObjectives) Or (node.IsTerminalNode And PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)
                                'Dim evalDiagonals As DiagonalsEvaluation = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        Dim wrtNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If Not isUndef Then
                                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                    NodesList.Clear()
                                                    If wrtNodeGuid.Equals(node.NodeGuidID) Then
                                                        Dim parentNode As clsNode = Nothing
                                                        If node.IsTerminalNode Then
                                                            parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                        Else
                                                            parentNode = H.GetNodeByID(parentNodeGuidID)
                                                        End If
                                                        If parentNode IsNot Nothing Then
                                                            Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet
                                                            If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                                                'evalDiagonals = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                                                evalDiagonals = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                                            Else
                                                                evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                                            End If

                                                            If node.IsTerminalNode Then
                                                                If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                                                    evalDiagonals = DiagonalsEvaluation.deAll
                                                                End If
                                                            Else
                                                                If ForceObjectives And (outcomes.Count < ObjectivesForceLimit) Then
                                                                    evalDiagonals = DiagonalsEvaluation.deAll
                                                                End If
                                                            End If

                                                            For Each R As clsRating In outcomes
                                                                Dim dummyNode As New clsNode
                                                                dummyNode.NodeGuidID = R.GuidID
                                                                NodesList.Add(dummyNode)
                                                            Next
                                                        End If
                                                    End If
                                                End If

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind1).RiskNodeType <> RiskNodeType.ntCategory AndAlso NodesList(ind2).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                                madeCount += 1
                                                            Else
                                                                Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                                If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                    (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select

                                            If Not isUndef Then
                                                Dim ind As Integer = GetNodeIndexByID(NodesList, childNodeGuidID)
                                                If (ind <> -1) Then
                                                    If node.IsTerminalNode Or (Not node.IsTerminalNode AndAlso NodesList(ind).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not node.DisabledForUser(User.UserID) AndAlso node.IsAllowed(User.UserID) Then
                                                            madeCount += 1
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        Dim outcomes As List(Of clsRating) = CType(alt.MeasurementScale, clsRatingScale).RatingSet
                                        Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, alt.NodeGuidID)
                                        If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                            evalDiagonals = DiagonalsEvaluation.deAll
                                        End If
                                        Dim NodesList As New List(Of clsNode)
                                        For Each R As clsRating In outcomes
                                            Dim dummyNode As New clsNode
                                            dummyNode.NodeGuidID = R.GuidID
                                            NodesList.Add(dummyNode)
                                        Next

                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                'parentNode.PWOutcomesJudgments.AddMeasureData(MD)

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If alt.Enabled AndAlso Not alt.DisabledForUser(User.UserID) AndAlso ProjectManager.UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, User.UserID) Then
                                                        If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                            madeCount += 1
                                                        Else
                                                            Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                            If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                If Not isUndef And ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If


                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                'alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                                If Not isUndef And ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                                    madeCount += 1
                                                End If

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(nextHierarchyPos, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_34
    Inherits clsStreamModelReader_v_1_1_33

    Public Overrides Function ReadMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
        Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
        Dim evalDiagonalsObjectives As DiagonalsEvaluation = PP.EvaluateDiagonals
        Dim evalDiagonalsAlternatives As DiagonalsEvaluation = PP.EvaluateDiagonalsAlternatives
        Dim ForceObjectives As Boolean = PP.ForceAllDiagonals
        Dim ForceAlternatives As Boolean = PP.ForceAllDiagonalsForAlternatives
        Dim ObjectivesForceLimit As Integer = PP.ForceAllDiagonalsLimit
        Dim AlternativesForceLimit As Integer = PP.ForceAllDiagonalsLimitForAlternatives

        'LastJudgmentTime = VERY_OLD_DATE

        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim madeCount As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263
            Dim nextHierarchyPos As Integer = mBR.BaseStream.Position - 4 + HJudgmentsChunkSize
            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660
                Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                'If H IsNot Nothing AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                'If H IsNot Nothing AndAlso (H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (H.HierarchyType <> ECHierarchyType.htMeasure) AndAlso ((HierarchyID = -1) Or ((HierarchyID <> -1) And (HierarchyID = H.HierarchyID))) Then
                If H IsNot Nothing AndAlso (((H.HierarchyID = ProjectManager.ActiveHierarchy) AndAlso (HierarchyID = -1)) OrElse ((HierarchyID <> -1) AndAlso (HierarchyID = H.HierarchyID))) Then  ' D7386
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = H.GetNodeByID(NodeGuidID)

                            Dim isTerminalNode As Boolean = If(node Is Nothing, False, node.IsTerminalNode)
                            Dim isAllowed As Boolean = If(node Is Nothing, False, node.IsAllowed(User.UserID))
                            Dim isDisabled As Boolean = If(node Is Nothing, False, node.DisabledForUser(User.UserID))

                            If (node IsNot Nothing) AndAlso ((Not isTerminalNode AndAlso PP.EvaluateObjectives) OrElse (isTerminalNode AndAlso PP.EvaluateAlternatives)) Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(User.UserID)

                                Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                If node.IsTerminalNode Then
                                    If ForceAlternatives And (NodesList.Count < AlternativesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If ForceObjectives And (NodesList.Count < ObjectivesForceLimit) Then
                                        evalDiagonals = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim nodesDict As New Dictionary(Of Guid, clsNode)
                                    For Each nd As clsNode In NodesList
                                        nodesDict.Add(nd.NodeGuidID, nd)
                                    Next

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing

                                        Dim wrtNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            value = Math.Abs(value)
                                            If value = 0 Then isUndef = True

                                            If Not isUndef Then
                                                If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                    NodesList.Clear()
                                                    If wrtNodeGuid.Equals(node.NodeGuidID) Then
                                                        Dim parentNode As clsNode = Nothing
                                                        If isTerminalNode Then
                                                            parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                        Else
                                                            parentNode = H.GetNodeByID(parentNodeGuidID)
                                                        End If
                                                        If parentNode IsNot Nothing Then
                                                            Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet
                                                            If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                                                'evalDiagonals = If(node.IsTerminalNode, evalDiagonalsAlternatives, evalDiagonalsObjectives)
                                                                evalDiagonals = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                                            Else
                                                                evalDiagonals = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                                            End If

                                                            If isTerminalNode Then
                                                                If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                                                    evalDiagonals = DiagonalsEvaluation.deAll
                                                                End If
                                                            Else
                                                                If ForceObjectives And (outcomes.Count < ObjectivesForceLimit) Then
                                                                    evalDiagonals = DiagonalsEvaluation.deAll
                                                                End If
                                                            End If

                                                            For Each R As clsRating In outcomes
                                                                Dim dummyNode As New clsNode
                                                                dummyNode.NodeGuidID = R.GuidID
                                                                NodesList.Add(dummyNode)
                                                            Next
                                                        End If
                                                    End If
                                                End If

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If isTerminalNode OrElse (Not isTerminalNode AndAlso NodesList(ind1).RiskNodeType <> RiskNodeType.ntCategory AndAlso NodesList(ind2).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not isDisabled AndAlso isAllowed Then
                                                            'If Not node.IsTerminalNode OrElse node.IsTerminalNode AndAlso ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, firstNodeGuidID, User.UserID) AndAlso ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, secondNodeGuidID, User.UserID) Then
                                                            If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                                madeCount += 1
                                                            Else
                                                                Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                                If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                        (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                            'End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Select Case node.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    Dim isDirectValue As Boolean = mBR.ReadBoolean
                                                    Dim directValue As Single
                                                    Dim ratingGuidID As Guid

                                                    If isDirectValue Then
                                                        directValue = mBR.ReadSingle
                                                    Else
                                                        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                            End Select

                                            If Not isUndef Then
                                                If nodesDict.ContainsKey(childNodeGuidID) Then
                                                    If isTerminalNode OrElse (Not isTerminalNode AndAlso nodesDict(childNodeGuidID).RiskNodeType <> RiskNodeType.ntCategory) Then
                                                        If node.Enabled AndAlso Not isDisabled AndAlso isAllowed Then
                                                            If Not isTerminalNode OrElse isTerminalNode AndAlso ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, childNodeGuidID, User.UserID) Then
                                                                madeCount += 1
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If

                                        mBR.ReadString()
                                        Dim time As DateTime = DateTime.FromBinary(mBR.ReadInt64)
                                        If time > LastJudgmentTime Then
                                            LastJudgmentTime = time
                                        End If
                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            AH = ProjectManager.AltsHierarchy(aGuidID)

                            Dim uncontributedAlts As List(Of clsNode) = H.GetUncontributedAlternatives()

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing AndAlso uncontributedAlts.Contains(alt) Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        Dim outcomes As List(Of clsRating) = CType(alt.MeasurementScale, clsRatingScale).RatingSet
                                        Dim evalDiagonals As DiagonalsEvaluation = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, alt.NodeGuidID)
                                        If ForceAlternatives And (outcomes.Count < AlternativesForceLimit) Then
                                            evalDiagonals = DiagonalsEvaluation.deAll
                                        End If
                                        Dim NodesList As New List(Of clsNode)
                                        For Each R As clsRating In outcomes
                                            Dim dummyNode As New clsNode
                                            dummyNode.NodeGuidID = R.GuidID
                                            NodesList.Add(dummyNode)
                                        Next

                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                'parentNode.PWOutcomesJudgments.AddMeasureData(MD)

                                                Dim ind1 As Integer = GetNodeIndexByID(NodesList, firstNodeGuidID)
                                                Dim ind2 As Integer = GetNodeIndexByID(NodesList, secondNodeGuidID)

                                                If (ind1 <> -1) AndAlso (ind2 <> -1) Then
                                                    If alt.Enabled AndAlso Not alt.DisabledForUser(User.UserID) AndAlso ProjectManager.UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, User.UserID) Then
                                                        If evalDiagonals = DiagonalsEvaluation.deAll Then
                                                            madeCount += 1
                                                        Else
                                                            Dim diff As Integer = Math.Abs(ind1 - ind2)
                                                            If (evalDiagonals = DiagonalsEvaluation.deFirst) And (diff = 1) Or
                                                                (evalDiagonals = DiagonalsEvaluation.deFirstAndSecond) And ((diff = 1) Or (diff = 2)) Then
                                                                If Not isUndef And ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                                                    madeCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If


                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                'alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                                If Not isUndef AndAlso ProjectManager.IsRiskProject AndAlso (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                                                    madeCount += 1
                                                End If

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While

                    Dim AltsCount As Integer = mBR.ReadInt32
                    t = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> AltsCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            AH = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.FeedbackMeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.FeedbackMeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.FeedbackMeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If Not IsPWMeasurementType(alt.FeedbackMeasureType) Then
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim nodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim node As clsNode = H.GetNodeByID(nodeGuidID)
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.FeedbackMeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.FeedbackMeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(node.NodeID, alt.NodeID, User.UserID, rating, alt.FeedbackMeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(node.NodeID, alt.NodeID, User.UserID, ucDataValue, alt.FeedbackMeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(node.NodeID, alt.NodeID, User.UserID, value, alt.FeedbackMeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(node.NodeID, alt.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                If Not isUndef And ProjectManager.FeedbackOn Then
                                                    madeCount += 1
                                                End If

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(nextHierarchyPos, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        ' read events links judgments
        If mBR.BaseStream.Position < mBR.BaseStream.Length - 1 Then
            Dim eCount As Integer = mBR.ReadInt32
            Dim t As Integer = 0
            While (mStream.Position < mStream.Length - 1) And (t <> eCount)
                Dim subChunk As Int32 = mBR.ReadInt32
                Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                If subChunk = CHUNK_NODE_JUDGMENTS Then
                    Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                    Dim AH As clsHierarchy = ProjectManager.ActiveAlternatives
                    Dim alt As clsNode = AH.GetNodeByID(AltGuidID)

                    If alt IsNot Nothing Then
                        Dim validMeasurementTypeAndScale As Boolean = True

                        If validMeasurementTypeAndScale Then
                            Dim jCount As Int32 = mBR.ReadInt32
                            Dim i As Integer = 0

                            Dim MD As clsNonPairwiseMeasureData = Nothing
                            While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                MD = Nothing
                                Dim isUndef As Boolean = mBR.ReadBoolean
                                Dim ChildGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                Dim child As clsNode = AH.GetNodeByID(ChildGuidID)

                                Dim mt As ECMeasureType = ECMeasureType.mtDirect
                                Select Case mt
                                    'Case ECMeasureType.mtRatings
                                    '    If isUndef Then
                                    '        mBR.ReadBoolean()
                                    '        mBR.ReadSingle()
                                    '    Else
                                    '        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                    '        Dim rating As clsRating
                                    '        Dim directValue As Single
                                    '        Dim ratingGuidID As Guid

                                    '        If isDirectValue Then
                                    '            directValue = mBR.ReadSingle

                                    '            rating = New clsRating
                                    '            rating.ID = -1
                                    '            rating.Name = "Direct Entry"
                                    '            rating.Value = directValue
                                    '        Else
                                    '            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                    '            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                    '        End If
                                    '        'MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                    '    End If
                                    'Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                    '    Dim ucDataValue As Single = mBR.ReadSingle
                                    '    'MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                    'Case ECMeasureType.mtStep
                                    '    Dim value As Single = mBR.ReadSingle
                                    '    'MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                    Case ECMeasureType.mtDirect
                                        Dim directValue As Single = mBR.ReadSingle
                                        If child IsNot Nothing Then
                                            MD = New clsDirectMeasureData(child.NodeID, alt.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                        End If
                                End Select

                                If MD IsNot Nothing Then
                                    MD.Comment = mBR.ReadString
                                    MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                    'If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                    '    MD.ModifyDate = Now
                                    'End If

                                    'alt.EventsJudgments.AddMeasureData(MD, False)

                                    madeCount += 1
                                Else
                                    mBR.ReadString()
                                    mBR.ReadInt64()
                                End If

                                i += 1
                            End While
                        Else
                            mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                        End If
                    Else
                        mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                    End If
                End If
                t += 1
            End While
        End If

        If mWriteStatus Then
            Debug.Print("Evaluation progress from streams completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        If LastJudgmentTime = VERY_OLD_DATE Then
            LastJudgmentTime = Nothing
        End If
        Return madeCount
    End Function

    Public Overrides Function AddJudgmentsToCombined(User As ECCore.ECTypes.clsUser, Group As ECCore.Groups.clsCombinedGroup, Optional Hierarchy As clsHierarchy = Nothing) As Integer
        mBR = New BinaryReader(mStream)

        Dim WeightsSum As Double = Group.GetWeightsSum

        Dim res As Integer = 0

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing And ((Hierarchy Is Nothing) Or (Hierarchy IsNot Nothing And Hierarchy Is H)) Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        'node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        'node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim CMD As clsCustomMeasureData = Nothing
                                        Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim outcomesNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    outcomesNode = node.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                                    If node.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                        R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                        R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                    End If
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble
                                                value = Math.Abs(value)
                                                If value = 0 Then isUndef = True

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                        If MD IsNot Nothing Then
                                                            CMD = CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(firstNode.NodeID, secondNode.NodeID, Group.CombinedUserID)
                                                            If CMD Is Nothing Then
                                                                CMD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, 0, 0, node.NodeID, Group.CombinedUserID, True)
                                                                node.Judgments.AddMeasureData(CMD)
                                                            End If
                                                        End If
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                        If MD IsNot Nothing Then
                                                            CMD = CType(parentNode.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(R1.ID, R2.ID, Group.CombinedUserID, , outcomesNode.NodeID)
                                                            If CMD Is Nothing Then
                                                                CMD = New clsPairwiseMeasureData(R1.ID, R2.ID, 0, 0, parentNode.NodeID, Group.CombinedUserID, True)
                                                                parentNode.PWOutcomesJudgments.AddMeasureData(CMD)
                                                            End If
                                                            CType(CMD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                        End If
                                                    End If
                                                End If

                                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                                    If value <> 0 Then
                                                        Dim bIncludeJudgment As Boolean = True
                                                        If Not node.IsAllowed(User.UserID) Then
                                                            bIncludeJudgment = False
                                                        Else
                                                            If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                                                If node.IsTerminalNode And node.MeasureType = ECMeasureType.mtPairwise Then
                                                                    If Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, firstNodeGuidID, User.UserID) OrElse Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, secondNodeGuidID, User.UserID) Then
                                                                        bIncludeJudgment = False
                                                                    End If
                                                                End If
                                                            End If
                                                        End If

                                                        If bIncludeJudgment Then
                                                            If advantage = -1 Then
                                                                value = 1 / value
                                                            End If
                                                            Dim mpwData As clsPairwiseMeasureData = CType(MD, clsPairwiseMeasureData)
                                                            Dim cpwData As clsPairwiseMeasureData = CType(CMD, clsPairwiseMeasureData)
                                                            If cpwData.FirstNodeID <> mpwData.FirstNodeID Then
                                                                value = 1 / value
                                                            End If
                                                            If ProjectManager.CalculationsManager.UseUserWeights AndAlso WeightsSum > 0 Then
                                                                'cpwData.AggregatedValue *= Math.Pow(value, User.Weight / WeightsSum)
                                                                cpwData.AggregatedValues.Add(New AggregatedData(value, User.Weight))
                                                            Else
                                                                cpwData.AggregatedValue *= value
                                                                cpwData.AggregatedValues2.Add(value)
                                                            End If
                                                            cpwData.UsersCount += 1
                                                        End If
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                            If MD IsNot Nothing Then
                                                                CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                                            End If
                                                        End If
                                                End Select

                                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                                    Dim bIncludeJudgment As Boolean = True
                                                    If Not node.IsAllowed(User.UserID) Then
                                                        bIncludeJudgment = False
                                                    Else
                                                        If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                                            If node.IsTerminalNode Then
                                                                If Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, childNodeGuidID, User.UserID) Then
                                                                    bIncludeJudgment = False
                                                                End If
                                                            End If
                                                        End If
                                                    End If

                                                    If bIncludeJudgment Then
                                                        Dim userValue As Single
                                                        Select Case node.MeasureType
                                                            Case ECMeasureType.mtDirect, ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                                                userValue = CType(MD, clsNonPairwiseMeasureData).SingleValue
                                                            Case Else
                                                                userValue = CType(CType(MD, clsNonPairwiseMeasureData).ObjectValue, Single)
                                                        End Select

                                                        If ProjectManager.CalculationsManager.UseUserWeights AndAlso WeightsSum > 0 Then
                                                            'CMD.AggregatedValue += userValue * User.Weight / WeightsSum
                                                            CMD.AggregatedValues.Add(New AggregatedData(userValue, User.Weight))
                                                        Else
                                                            CMD.AggregatedValue += userValue
                                                        End If
                                                        CMD.UsersCount += 1
                                                    End If
                                                End If
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            'If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                            '    parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            'Else
                                            '    node.Judgments.AddMeasureData(MD, False)
                                            'End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim CMD As clsCustomMeasureData = Nothing

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                CMD = CType(parentNode.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(R1.ID, R2.ID, Group.CombinedUserID)
                                                If CMD Is Nothing Then
                                                    CMD = New clsPairwiseMeasureData(R1.ID, R2.ID, 0, 0, parentNode.NodeID, Group.CombinedUserID, True)
                                                    parentNode.PWOutcomesJudgments.AddMeasureData(CMD)
                                                End If
                                                CType(CMD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID

                                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                                    If value <> 0 Then
                                                        Dim bIncludeJudgment As Boolean = True
                                                        If Not alt.IsAllowed(User.UserID) Then
                                                            bIncludeJudgment = False
                                                        End If

                                                        If bIncludeJudgment Then
                                                            If advantage = -1 Then
                                                                value = 1 / value
                                                            End If
                                                            Dim mpwData As clsPairwiseMeasureData = CType(MD, clsPairwiseMeasureData)
                                                            Dim cpwData As clsPairwiseMeasureData = CType(CMD, clsPairwiseMeasureData)
                                                            If cpwData.FirstNodeID <> mpwData.FirstNodeID Then
                                                                value = 1 / value
                                                            End If
                                                            If ProjectManager.CalculationsManager.UseUserWeights AndAlso WeightsSum > 0 Then
                                                                'cpwData.AggregatedValue *= Math.Pow(value, User.Weight / WeightsSum)
                                                                cpwData.AggregatedValues.Add(New AggregatedData(value, User.Weight))
                                                            Else
                                                                cpwData.AggregatedValue *= value
                                                                cpwData.AggregatedValues2.Add(value)
                                                            End If
                                                            cpwData.UsersCount += 1
                                                        End If
                                                    End If
                                                End If

                                                'parentNode.PWOutcomesJudgments.AddMeasureData(MD)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                        If MD IsNot Nothing Then
                                                            CMD = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, H.Nodes(0).NodeID, Group.CombinedUserID)
                                                        End If
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                    If MD IsNot Nothing Then
                                                        CMD = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, H.Nodes(0).NodeID, Group.CombinedUserID)
                                                    End If
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                    If MD IsNot Nothing Then
                                                        CMD = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, H.Nodes(0).NodeID, Group.CombinedUserID)
                                                    End If
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                                    If MD IsNot Nothing Then
                                                        CMD = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, H.Nodes(0).NodeID, Group.CombinedUserID)
                                                    End If
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                                    Dim bIncludeJudgment As Boolean = True
                                                    If Not ProjectManager.UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, User.UserID) Then
                                                        bIncludeJudgment = False
                                                    End If

                                                    If bIncludeJudgment Then
                                                        Dim userValue As Single
                                                        Select Case alt.MeasureType
                                                            Case ECMeasureType.mtDirect, ECMeasureType.mtRatings
                                                                userValue = CType(MD, clsNonPairwiseMeasureData).SingleValue
                                                            Case Else
                                                                userValue = CType(CType(MD, clsNonPairwiseMeasureData).ObjectValue, Single)
                                                        End Select

                                                        If ProjectManager.CalculationsManager.UseUserWeights AndAlso WeightsSum > 0 Then
                                                            'CMD.AggregatedValue += userValue * User.Weight / WeightsSum
                                                            CMD.AggregatedValues.Add(New AggregatedData(userValue, User.Weight))
                                                        Else
                                                            CMD.AggregatedValue += userValue
                                                        End If
                                                        CMD.UsersCount += 1
                                                    End If
                                                End If

                                                'alt.DirectJudgmentsForNoCause.AddMeasureData(CMD, False)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While

                    Dim AltsCount As Integer = mBR.ReadInt32
                    t = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> AltsCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.FeedbackMeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.FeedbackMeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.FeedbackMeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If Not IsPWMeasurementType(alt.FeedbackMeasureType) Then
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim nodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim node As clsNode = H.GetNodeByID(nodeGuidID)
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.FeedbackMeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.FeedbackMeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(node.NodeID, alt.NodeID, User.UserID, rating, alt.FeedbackMeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(node.NodeID, alt.NodeID, User.UserID, ucDataValue, alt.FeedbackMeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(node.NodeID, alt.NodeID, User.UserID, value, alt.FeedbackMeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(node.NodeID, alt.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                If Not isUndef And ProjectManager.FeedbackOn Then
                                                    'madeCount += 1
                                                End If

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mBR.BaseStream.Position < mBR.BaseStream.Length - 1 Then
            Dim eCount As Integer = mBR.ReadInt32
            Dim t As Integer = 0
            While (mStream.Position < mStream.Length - 1) And (t <> eCount)
                Dim subChunk As Int32 = mBR.ReadInt32
                Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                If subChunk = CHUNK_NODE_JUDGMENTS Then
                    Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                    Dim AH As clsHierarchy = ProjectManager.ActiveAlternatives
                    Dim alt As clsNode = AH.GetNodeByID(AltGuidID)

                    If alt IsNot Nothing Then
                        Dim validMeasurementTypeAndScale As Boolean = True

                        If validMeasurementTypeAndScale Then
                            Dim jCount As Int32 = mBR.ReadInt32
                            Dim i As Integer = 0

                            Dim MD As clsNonPairwiseMeasureData = Nothing
                            While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                MD = Nothing
                                Dim CMD As clsNonPairwiseMeasureData = Nothing
                                Dim isUndef As Boolean = mBR.ReadBoolean
                                Dim parentNode As clsNode = Nothing
                                Dim childNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                Dim childNode As clsNode
                                childNode = AH.GetNodeByID(childNodeGuidID)

                                Dim mt As ECMeasureType = ECMeasureType.mtDirect
                                Select Case mt
                                    'Case ECMeasureType.mtRatings
                                    '    Dim isDirectValue As Boolean = mBR.ReadBoolean

                                    '    Dim rating As clsRating
                                    '    Dim directValue As Single
                                    '    Dim ratingGuidID As Guid

                                    '    If isDirectValue Then
                                    '        directValue = mBR.ReadSingle

                                    '        rating = New clsRating
                                    '        rating.ID = -1
                                    '        rating.Name = "Direct Entry from EC11.5"
                                    '        rating.Value = directValue
                                    '    Else
                                    '        ratingGuidID = New Guid(mBR.ReadBytes(16))
                                    '        rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                    '    End If
                                    '    If childNode IsNot Nothing Then 'C0264
                                    '        MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                    '        If MD IsNot Nothing Then
                                    '            CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                    '        End If
                                    '    End If
                                    'Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                    '    Dim ucDataValue As Single = mBR.ReadSingle
                                    '    If childNode IsNot Nothing Then 'C0264
                                    '        MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                    '        If MD IsNot Nothing Then
                                    '            CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                    '        End If
                                    '    End If
                                    'Case ECMeasureType.mtStep
                                    '    Dim value As Single = mBR.ReadSingle
                                    '    If childNode IsNot Nothing Then 'C0264
                                    '        MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                    '        If MD IsNot Nothing Then
                                    '            CMD = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, node.NodeID, Group.CombinedUserID)
                                    '        End If
                                    '    End If
                                    Case ECMeasureType.mtDirect
                                        Dim directValue As Single = mBR.ReadSingle
                                        If childNode IsNot Nothing Then
                                            MD = New clsDirectMeasureData(childNode.NodeID, alt.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            If MD IsNot Nothing Then
                                                CMD = CType(alt.EventsJudgments, clsNonPairwiseJudgments).GetJudgement(childNode.NodeID, alt.NodeID, Group.CombinedUserID)
                                            End If
                                        End If
                                End Select

                                If MD IsNot Nothing AndAlso CMD IsNot Nothing Then
                                    Dim bIncludeJudgment As Boolean = True
                                    'TODO: Nexteer - handle roles
                                    'If Not node.IsAllowed(User.UserID) Then
                                    '    bIncludeJudgment = False
                                    'Else
                                    '    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                    '        If node.IsTerminalNode Then
                                    '            If Not ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, childNodeGuidID, User.UserID) Then
                                    '                bIncludeJudgment = False
                                    '            End If
                                    '        End If
                                    '    End If
                                    'End If

                                    If bIncludeJudgment Then
                                        Dim userValue As Single
                                        ' TODO: Nexteer - handle measurement types
                                        Select Case mt
                                            Case ECMeasureType.mtDirect, ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                                userValue = CType(MD, clsNonPairwiseMeasureData).SingleValue
                                            Case Else
                                                userValue = CType(CType(MD, clsNonPairwiseMeasureData).ObjectValue, Single)
                                        End Select

                                        If ProjectManager.CalculationsManager.UseUserWeights AndAlso WeightsSum > 0 Then
                                            'CMD.AggregatedValue += userValue * User.Weight / WeightsSum
                                            CMD.AggregatedValues.Add(New AggregatedData(userValue, User.Weight))
                                        Else
                                            CMD.AggregatedValue += userValue
                                        End If
                                        CMD.UsersCount += 1
                                    End If
                                End If

                                If MD IsNot Nothing Then
                                    MD.Comment = mBR.ReadString
                                    MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                    If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                        MD.ModifyDate = Now
                                    End If

                                    res += 1
                                Else
                                    mBR.ReadString()
                                    mBR.ReadInt64()
                                End If

                                i += 1
                            End While
                        Else
                            mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                        End If
                    Else
                        mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                    End If
                End If
                t += 1
            End While
        End If

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean 'C0626
        ' read hierarchies with nodes

        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()
        ProjectManager.MeasureHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim bReadHiearchyChunk As Boolean = True 'C0738

        Dim HierarchyChunkSize As Integer 'C0738

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            If bReadHiearchyChunk Then 'C0738
                chunk = mBR.ReadInt32
                HierarchyChunkSize = mBR.ReadInt32 'C0738
            End If
            bReadHiearchyChunk = True 'C0738 - reset reading chunk (this can be set to False later when finding out that the model is corrupted - getting hierarchy chunk instead of node chunk)

            'Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263 'C0738

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                    Case ECHierarchyType.htMeasure
                        H = ProjectManager.AddMeasureHierarchy
                End Select

                H.Nodes.Clear()
                H.ResetNodesDictionaries()

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    H.Nodes.Capacity = nodesCount
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then 'C0738
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32

                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            Dim ParentsCount As Integer = mBR.ReadInt32
                            If ParentsCount > 0 Then node.ParentNodesGuids = New List(Of Guid) 'A1000
                            For k As Integer = 1 To ParentsCount
                                Dim pGuid As Guid = New Guid(mBR.ReadBytes(16))
                                If Not node.IsAlternative Then node.ParentNodesGuids.Add(pGuid) 'A1000
                            Next

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If (H.HierarchyType <> ECHierarchyType.htModel) Or
                               ((H.HierarchyType = ECHierarchyType.htModel) And ((node.ParentNodeID <> -1) Or ((node.ParentNodeID = -1) AndAlso (H.Nodes.Count = 0)))) Then 'C0731
                                If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, If(node.IsAlternative, -1, node.ParentNodeID), True) 'C0383
                            End If

                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            node.FeedbackMeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.FeedbackMeasureType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                                    node.FeedbackRatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.FeedbackRegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.FeedbackAdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.FeedbackStepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            'If node.IsAlternative And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                            If node.IsAlternative And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                                node.MeasureType = ECMeasureType.mtRatings
                                node.RatingScaleID = -1
                            End If

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32

                            node.Comment = mBR.ReadString

                            Dim cost As String = mBR.ReadString
                            If (cost <> UNDEFINED_COST_VALUE) And (node.IsAlternative) Then
                                node.Tag = cost
                            End If

                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            'C0738===
                            If subChunk = CHUNK_HIERARCHY Then
                                bReadHiearchyChunk = False
                                chunk = CHUNK_HIERARCHY
                                HierarchyChunkSize = NodeChunkSize
                                j = nodesCount - 1 'to skip nodes after increment below
                            Else
                                Return False
                            End If
                            'C0738===

                            'Return False 'C0738
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
            H.FixChildrenLinks()
        Next

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function

    Public Overrides Function ReadUserJudgments(ByVal User As clsUser) As Integer
        mBR = New BinaryReader(mStream)

        Dim res As Integer = 0

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments started " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then
                'Dim hID As Int32 = mBR.ReadInt32 'C0261
                'Dim ahID As Int32 = mBR.ReadInt32 'C0261

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660
                Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                If H IsNot Nothing Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            If node IsNot Nothing Then
                                Dim isTerminalNode As Boolean = node.IsTerminalNode
                                Dim isAllowed As Boolean = node.IsAllowed(User.UserID)
                                Dim isDisabled As Boolean = node.DisabledForUser(User.UserID)
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    If node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                        node.Judgments.JudgmentsFromUser(User.UserID).Clear()
                                        node.Judgments.JudgmentsFromUser(User.UserID).Capacity = jCount
                                    End If

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim outcomesNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    outcomesNode = node.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                                    If node.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                        R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                        R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                    End If
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble
                                                value = Math.Abs(value)
                                                If value = 0 Then isUndef = True

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            MD.Comment = mBR.ReadString
                                            MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                            If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                MD.ModifyDate = Now
                                            End If

                                            If node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)
                                            Else
                                                node.Judgments.AddMeasureData(MD, False)
                                            End If

                                            res += 1 'C0765
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            AH = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                parentNode.PWOutcomesJudgments.AddMeasureData(MD)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                alt.DirectJudgmentsForNoCause.AddMeasureData(MD, False)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While

                    ' feedback
                    Dim altsCount As Integer = mBR.ReadInt32
                    t = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> altsCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            AH = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.FeedbackMeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.FeedbackMeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.FeedbackMeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.FeedbackMeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If Not IsPWMeasurementType(alt.FeedbackMeasureType) Then
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim node As clsNode = H.GetNodeByID(NodeGuidID)
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.FeedbackMeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.FeedbackMeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If node IsNot Nothing Then
                                                            MD = New clsRatingMeasureData(node.NodeID, alt.NodeID, User.UserID, rating, alt.FeedbackMeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                        End If
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    If node IsNot Nothing Then
                                                        MD = New clsUtilityCurveMeasureData(node.NodeID, alt.NodeID, User.UserID, ucDataValue, alt.FeedbackMeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                    End If
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    If node IsNot Nothing Then
                                                        MD = New clsStepMeasureData(node.NodeID, alt.NodeID, User.UserID, value, alt.FeedbackMeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                    End If
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    If node IsNot Nothing Then
                                                        MD = New clsDirectMeasureData(node.NodeID, alt.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                                    End If
                                            End Select

                                            If MD IsNot Nothing Then
                                                MD.Comment = mBR.ReadString
                                                MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                                If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                                    MD.ModifyDate = Now
                                                End If

                                                alt.FeedbackJudgments.AddMeasureData(MD, False)

                                                res += 1
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        ' read events links judgments
        If mBR.BaseStream.Position < mBR.BaseStream.Length - 1 Then
            Dim eCount As Integer = mBR.ReadInt32
            Dim t As Integer = 0
            While (mStream.Position < mStream.Length - 1) And (t <> eCount)
                Dim subChunk As Int32 = mBR.ReadInt32
                Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                If subChunk = CHUNK_NODE_JUDGMENTS Then
                    Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                    Dim AH As clsHierarchy = ProjectManager.ActiveAlternatives
                    Dim alt As clsNode = AH.GetNodeByID(AltGuidID)

                    If alt IsNot Nothing Then
                        Dim validMeasurementTypeAndScale As Boolean = True

                        If validMeasurementTypeAndScale Then
                            Dim jCount As Int32 = mBR.ReadInt32
                            Dim i As Integer = 0

                            Dim MD As clsNonPairwiseMeasureData = Nothing
                            While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                MD = Nothing
                                Dim isUndef As Boolean = mBR.ReadBoolean
                                Dim ChildGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                Dim child As clsNode = AH.GetNodeByID(ChildGuidID)

                                Dim mt As ECMeasureType = ECMeasureType.mtDirect
                                Select Case mt
                                    'Case ECMeasureType.mtRatings
                                    '    If isUndef Then
                                    '        mBR.ReadBoolean()
                                    '        mBR.ReadSingle()
                                    '    Else
                                    '        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                    '        Dim rating As clsRating
                                    '        Dim directValue As Single
                                    '        Dim ratingGuidID As Guid

                                    '        If isDirectValue Then
                                    '            directValue = mBR.ReadSingle

                                    '            rating = New clsRating
                                    '            rating.ID = -1
                                    '            rating.Name = "Direct Entry"
                                    '            rating.Value = directValue
                                    '        Else
                                    '            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                    '            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                    '        End If
                                    '        'MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                    '    End If
                                    'Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                    '    Dim ucDataValue As Single = mBR.ReadSingle
                                    '    'MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                    'Case ECMeasureType.mtStep
                                    '    Dim value As Single = mBR.ReadSingle
                                    '    'MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                    Case ECMeasureType.mtDirect
                                        Dim directValue As Single = mBR.ReadSingle
                                        If child IsNot Nothing Then
                                            MD = New clsDirectMeasureData(child.NodeID, alt.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                        End If
                                End Select

                                If MD IsNot Nothing Then
                                    MD.Comment = mBR.ReadString
                                    MD.ModifyDate = DateTime.FromBinary(mBR.ReadInt64)

                                    If (MD.ModifyDate = VERY_OLD_DATE) Or (MD.ModifyDate = UNDEFINED_DATE) Then
                                        MD.ModifyDate = Now
                                    End If

                                    alt.EventsJudgments.AddMeasureData(MD, False)

                                    res += 1
                                Else
                                    mBR.ReadString()
                                    mBR.ReadInt64()
                                End If

                                i += 1
                            End While
                        Else
                            mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                        End If
                    Else
                        mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                    End If
                End If
                t += 1
            End While
        End If

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()
        'Return True 'C0765
        Return res 'C0765
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_43
    Inherits clsStreamModelReader_v_1_1_34

    Public Overrides Function ReadDataMapping() As Boolean
        mBR = New BinaryReader(mStream)

        If mWriteStatus Then
            Debug.Print("ReadDataMapping started " + Now.ToString)
        End If

        ProjectManager.DataMappings.Clear()

        Dim dmCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        While (mStream.Position < mStream.Length - 1) And (j <> dmCount)
            Dim dm As New clsDataMapping

            dm.DataMappingGUID = New Guid(mBR.ReadBytes(16))
            dm.eccMappedColID = New Guid(mBR.ReadBytes(16))
            dm.eccMappedColType = mBR.ReadInt32
            dm.externalColName = mBR.ReadString
            dm.externalDBconnString = mBR.ReadString
            dm.externalDBname = mBR.ReadString
            dm.externalDBType = mBR.ReadInt32
            dm.externalMapkeyColName = mBR.ReadString
            dm.externalTblName = mBR.ReadString

            ProjectManager.DataMappings.Add(dm) 'AS/12323xq
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ReadDataMapping completed " + Now.ToString)
        End If

        mBR.Close()
        Return True
    End Function

    Public Overrides Function JudgmentsExists(User As clsUser, Optional HierarchyID As Integer = -1) As Boolean
        mBR = New BinaryReader(mStream)

        Dim hID As Integer = If(HierarchyID = -1, ProjectManager.ActiveHierarchy, HierarchyID)

        Dim hjCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (j <> hjCount)
            chunk = mBR.ReadInt32
            Dim HJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

            If chunk = CHUNK_HIERARCHY_JUDGMENTS Then

                Dim hGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim aGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                Dim H As clsHierarchy = ProjectManager.Hierarchy(hGuidID) 'C0660

                If H IsNot Nothing AndAlso H.HierarchyID = hID Then
                    Dim nodesCount As Int32 = mBR.ReadInt32

                    Dim k As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (k <> nodesCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32 'C0263

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize  'C0264

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                            Dim node As clsNode = ProjectManager.Hierarchy(hGuidID).GetNodeByID(NodeGuidID)

                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            If node IsNot Nothing Then
                                Dim MT As ECMeasureType = mBR.ReadInt32
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> node.MeasureType Then
                                    If MT = ECMeasureType.mtPairwise And node.MeasureType = ECMeasureType.mtPWAnalogous Or
                                        MT = ECMeasureType.mtPWAnalogous And node.MeasureType = ECMeasureType.mtPairwise Then
                                        validMeasurementTypeAndScale = True
                                    Else
                                        validMeasurementTypeAndScale = False
                                    End If
                                Else
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If node.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (node.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then 'C0283
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    Dim childNodeGuidID As Guid 'C0261
                                    Dim MD As clsCustomMeasureData

                                    While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                        MD = Nothing
                                        Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                        Dim isUndef As Boolean = mBR.ReadBoolean
                                        Dim parentNode As clsNode = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                                Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))

                                                If node.IsTerminalNode Then
                                                    parentNode = AH.GetNodeByID(parentNodeGuidID)
                                                Else
                                                    parentNode = H.GetNodeByID(parentNodeGuidID)
                                                End If

                                                Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                                Dim firstNode As clsNode = Nothing
                                                Dim secondNode As clsNode = Nothing

                                                Dim outcomesNode As clsNode = Nothing

                                                Dim R1 As clsRating = Nothing
                                                Dim R2 As clsRating = Nothing

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If node.IsTerminalNode Then
                                                        firstNode = AH.GetNodeByID(firstNodeGuidID)
                                                        secondNode = AH.GetNodeByID(secondNodeGuidID)
                                                    Else
                                                        firstNode = node.Hierarchy.GetNodeByID(firstNodeGuidID)
                                                        secondNode = node.Hierarchy.GetNodeByID(secondNodeGuidID)
                                                    End If
                                                Else
                                                    outcomesNode = node.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                                    If node.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                        R1 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                        R2 = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                                    End If
                                                End If

                                                Dim advantage As Int32 = mBR.ReadInt32
                                                Dim value As Single = mBR.ReadDouble
                                                value = Math.Abs(value)

                                                If node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Then
                                                    If firstNode IsNot Nothing And secondNode IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(firstNode.NodeID, secondNode.NodeID, advantage, value, node.NodeID, User.UserID, isUndef)
                                                    End If
                                                Else
                                                    If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                        MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                        CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                                    End If
                                                End If
                                            Case Else
                                                childNodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                                                Dim childNode As clsNode
                                                If node.IsTerminalNode Then
                                                    childNode = AH.GetNodeByID(childNodeGuidID)
                                                Else
                                                    childNode = node.Hierarchy.GetNodeByID(childNodeGuidID)
                                                End If

                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry from EC11.5"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(node.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsRatingMeasureData(childNode.NodeID, node.NodeID, User.UserID, rating, node.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        Dim ucDataValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsUtilityCurveMeasureData(childNode.NodeID, node.NodeID, User.UserID, ucDataValue, node.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtStep
                                                        Dim value As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsStepMeasureData(childNode.NodeID, node.NodeID, User.UserID, value, node.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef)) 'C0261
                                                        End If
                                                    Case ECMeasureType.mtDirect
                                                        Dim directValue As Single = mBR.ReadSingle
                                                        If childNode IsNot Nothing Then 'C0264
                                                            MD = New clsDirectMeasureData(childNode.NodeID, node.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef)) 'C0261
                                                        End If
                                                End Select
                                        End Select

                                        If MD IsNot Nothing Then
                                            mBR.Close()
                                            Return True
                                        Else
                                            mBR.ReadString()
                                            mBR.ReadInt64()
                                        End If

                                        i += 1
                                    End While
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        k += 1
                    End While

                    Dim NoContributionCount As Integer = mBR.ReadInt32
                    Dim t As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> NoContributionCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.MeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtDirect, ECMeasureType.mtPWOutcomes
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.MeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.MeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            Dim MD As clsPairwiseMeasureData = Nothing
                                            Dim outcomesNodeGuid As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim isUndef As Boolean = mBR.ReadBoolean
                                            Dim parentNode As clsNode = Nothing
                                            Dim parentNodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            parentNode = AH.GetNodeByID(parentNodeGuidID)

                                            Dim firstNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                                            Dim secondNodeGuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261

                                            Dim firstNode As clsNode = Nothing
                                            Dim secondNode As clsNode = Nothing

                                            Dim outcomesNode As clsNode = Nothing

                                            Dim R1 As clsRating = Nothing
                                            Dim R2 As clsRating = Nothing

                                            outcomesNode = alt.Hierarchy.GetNodeByID(outcomesNodeGuid)

                                            If alt.NodeGuidID.Equals(outcomesNode.NodeGuidID) Then
                                                R1 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(firstNodeGuidID)
                                                R2 = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(secondNodeGuidID)
                                            End If

                                            Dim advantage As Int32 = mBR.ReadInt32
                                            Dim value As Single = mBR.ReadDouble

                                            If parentNode IsNot Nothing And R1 IsNot Nothing And R2 IsNot Nothing Then
                                                MD = New clsPairwiseMeasureData(R1.ID, R2.ID, advantage, value, parentNode.NodeID, User.UserID, isUndef)
                                                CType(MD, clsPairwiseMeasureData).OutcomesNodeID = outcomesNode.NodeID
                                            End If

                                            If MD IsNot Nothing Then
                                                mBR.Close()
                                                Return True
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    Else
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.MeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.MeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        MD = New clsRatingMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, rating, alt.MeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    MD = New clsUtilityCurveMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, ucDataValue, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    MD = New clsStepMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, value, alt.MeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    MD = New clsDirectMeasureData(alt.NodeID, H.Nodes(0).NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                            End Select

                                            If MD IsNot Nothing Then
                                                mBR.Close()
                                                Return True
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While

                    ' feedback
                    Dim altsCount As Integer = mBR.ReadInt32
                    t = 0
                    While (mStream.Position < mStream.Length - 1) And (t <> altsCount)
                        Dim subChunk As Int32 = mBR.ReadInt32
                        Dim NodeJudgmentsChunkSize As Integer = mBR.ReadInt32

                        Dim NextNodeJudgmentsChunkPosition As Integer = mBR.BaseStream.Position - 4 + NodeJudgmentsChunkSize

                        If subChunk = CHUNK_NODE_JUDGMENTS Then
                            Dim AltGuidID As Guid = New Guid(mBR.ReadBytes(16))
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(aGuidID).GetNodeByID(AltGuidID)
                            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(aGuidID)

                            Dim MT As ECMeasureType = CType(mBR.ReadInt32, ECMeasureType)

                            If alt IsNot Nothing Then
                                Dim MSGuid As Guid = New Guid(mBR.ReadBytes(16))

                                Dim validMeasurementTypeAndScale As Boolean
                                If MT <> alt.FeedbackMeasureType Then
                                    validMeasurementTypeAndScale = False
                                Else
                                    Select Case alt.FeedbackMeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtDirect
                                            validMeasurementTypeAndScale = True
                                        Case ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
                                            validMeasurementTypeAndScale = False
                                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtStep
                                            If alt.FeedbackMeasurementScale IsNot Nothing Then
                                                validMeasurementTypeAndScale = (alt.FeedbackMeasurementScale.GuidID = MSGuid)
                                            Else
                                                validMeasurementTypeAndScale = False
                                            End If
                                    End Select
                                End If

                                If validMeasurementTypeAndScale Then
                                    Dim jCount As Int32 = mBR.ReadInt32
                                    Dim i As Integer = 0

                                    If Not IsPWMeasurementType(alt.FeedbackMeasureType) Then
                                        Dim MD As clsNonPairwiseMeasureData = Nothing
                                        While (mStream.Position < mStream.Length - 1) And (i <> jCount)
                                            MD = Nothing
                                            Dim NodeGuidID As Guid = New Guid(mBR.ReadBytes(16))
                                            Dim node As clsNode = H.GetNodeByID(NodeGuidID)
                                            Dim isUndef As Boolean = mBR.ReadBoolean

                                            Select Case alt.FeedbackMeasureType
                                                Case ECMeasureType.mtRatings
                                                    If isUndef Then
                                                        mBR.ReadBoolean()
                                                        mBR.ReadSingle()
                                                    Else
                                                        Dim isDirectValue As Boolean = mBR.ReadBoolean

                                                        Dim rating As clsRating
                                                        Dim directValue As Single
                                                        Dim ratingGuidID As Guid

                                                        If isDirectValue Then
                                                            directValue = mBR.ReadSingle

                                                            rating = New clsRating
                                                            rating.ID = -1
                                                            rating.Name = "Direct Entry"
                                                            rating.Value = directValue
                                                        Else
                                                            ratingGuidID = New Guid(mBR.ReadBytes(16))
                                                            rating = CType(alt.FeedbackMeasurementScale, clsRatingScale).GetRatingByID(ratingGuidID)
                                                        End If
                                                        If node IsNot Nothing Then
                                                            MD = New clsRatingMeasureData(node.NodeID, alt.NodeID, User.UserID, rating, alt.FeedbackMeasurementScale, If(Not isUndef, rating Is Nothing, isUndef))
                                                        End If
                                                    End If
                                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                    Dim ucDataValue As Single = mBR.ReadSingle
                                                    If node IsNot Nothing Then
                                                        MD = New clsUtilityCurveMeasureData(node.NodeID, alt.NodeID, User.UserID, ucDataValue, alt.FeedbackMeasurementScale, If(Not isUndef, Single.IsNaN(ucDataValue), isUndef))
                                                    End If
                                                Case ECMeasureType.mtStep
                                                    Dim value As Single = mBR.ReadSingle
                                                    If node IsNot Nothing Then
                                                        MD = New clsStepMeasureData(node.NodeID, alt.NodeID, User.UserID, value, alt.FeedbackMeasurementScale, If(Not isUndef, Single.IsNaN(value), isUndef))
                                                    End If
                                                Case ECMeasureType.mtDirect
                                                    Dim directValue As Single = mBR.ReadSingle
                                                    If node IsNot Nothing Then
                                                        MD = New clsDirectMeasureData(node.NodeID, alt.NodeID, User.UserID, directValue, If(Not isUndef, Single.IsNaN(directValue), isUndef))
                                                    End If
                                            End Select

                                            If MD IsNot Nothing Then
                                                mBR.Close()
                                                Return True
                                            Else
                                                mBR.ReadString()
                                                mBR.ReadInt64()
                                            End If

                                            i += 1
                                        End While
                                    End If
                                Else
                                    mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0283
                                End If
                            Else
                                mBR.BaseStream.Seek(NextNodeJudgmentsChunkPosition, SeekOrigin.Begin) 'C0264
                            End If
                        End If
                        t += 1
                    End While
                Else
                    mBR.BaseStream.Seek(mBR.BaseStream.Position - 4 + HJudgmentsChunkSize, SeekOrigin.Begin) 'C0660
                End If
            End If
            j += 1
        End While

        If mWriteStatus Then
            Debug.Print("ProcessUserJudgments completed " + Now.ToString + " User: " + User.UserID.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        mBR.Close()

        Return False
    End Function
End Class

<Serializable()> Public Class clsStreamModelReader_v_1_1_45
    Inherits clsStreamModelReader_v_1_1_43
    Public Overrides Function ReadEventsGroups() As Boolean
        mBR = New BinaryReader(mStream)

        ProjectManager.EventsGroups.Groups.Clear()

        Dim gCount As Int32 = mBR.ReadInt32
        Dim j As Integer = 0

        While (mStream.Position < mStream.Length - 1) And (j <> gCount)
            Dim group As New EventsGroup
            group.ID = New Guid(mBR.ReadBytes(16))
            group.Name = mBR.ReadString
            Dim eCount As Integer = mBR.ReadInt32
            Dim i As Integer = 0
            While (mStream.Position < mStream.Length - 1) And (i <> eCount)
                Dim eData As New EventGroupData
                eData.EventGuidID = New Guid(mBR.ReadBytes(16))
                eData.EventIntID = mBR.ReadInt32
                eData.Precedence = mBR.ReadInt32
                group.Events.Add(eData)
                i += 1
            End While
            ProjectManager.EventsGroups.Groups.Add(group)
            j += 1
        End While

        mBR.Close()
        Return True
    End Function
End Class


<Serializable()> Public Class clsStreamModelReader_v_1_1_46
    Inherits clsStreamModelReader_v_1_1_45

    Protected Overrides Function ProcessHierarchiesChunk() As Boolean 'C0626
        ' read hierarchies with nodes
        ProjectManager.Hierarchies.Clear()
        ProjectManager.AltsHierarchies.Clear()
        ProjectManager.MeasureHierarchies.Clear()

        For Each hierarchy As clsHierarchy In ProjectManager.GetAllHierarchies
            hierarchy.ResetNodesDictionaries()
        Next

        ' read the number of hierarchies first
        Dim hCount As Int32 = mBR.ReadInt32
        Dim i As Integer = 0

        Dim H As clsHierarchy = Nothing

        Dim bReadHiearchyChunk As Boolean = True 'C0738

        Dim HierarchyChunkSize As Integer 'C0738

        Dim chunk As Int32
        While (mStream.Position < mStream.Length - 1) And (i <> hCount)
            If bReadHiearchyChunk Then 'C0738
                chunk = mBR.ReadInt32
                HierarchyChunkSize = mBR.ReadInt32 'C0738
            End If
            bReadHiearchyChunk = True 'C0738 - reset reading chunk (this can be set to False later when finding out that the model is corrupted - getting hierarchy chunk instead of node chunk)

            'Dim HierarchyChunkSize As Integer = mBR.ReadInt32 'C0263 'C0738

            If chunk = CHUNK_HIERARCHY Then
                Dim hID As Int32 = mBR.ReadInt32
                Dim GuidID As Guid = New Guid(mBR.ReadBytes(16)) 'C0261
                Dim hType As ECHierarchyType = CType(mBR.ReadInt32, ECHierarchyType)

                Select Case hType
                    Case ECHierarchyType.htModel
                        H = ProjectManager.AddHierarchy
                    Case ECHierarchyType.htAlternative
                        H = ProjectManager.AddAltsHierarchy
                    Case ECHierarchyType.htMeasure
                        H = ProjectManager.AddMeasureHierarchy
                End Select

                H.Nodes.Clear()
                H.ResetNodesDictionaries()

                H.HierarchyID = hID
                H.HierarchyGuidID = GuidID 'C0261
                H.HierarchyName = mBR.ReadString
                H.Comment = mBR.ReadString

                Dim subChunk As Int32 = mBR.ReadInt32
                Dim HNodesChunkSize As Integer = mBR.ReadInt32 'C0263

                If subChunk = CHUNK_HIERARCHY_NODES Then
                    Dim nodesCount As Int32 = mBR.ReadInt32
                    H.Nodes.Capacity = nodesCount
                    Dim j As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (j <> nodesCount)
                        subChunk = mBR.ReadInt32
                        Dim NodeChunkSize As Integer = mBR.ReadInt32 'C0263

                        If subChunk = CHUNK_HIERARCHY_NODE Then 'C0738
                            Dim node As clsNode = New clsNode(H)
                            node.NodeID = mBR.ReadInt32
                            node.NodeGuidID = New Guid(mBR.ReadBytes(16)) 'C0261
                            node.NodeName = mBR.ReadString
                            node.ParentNodeID = mBR.ReadInt32

                            node.IsAlternative = H.HierarchyType = ECHierarchyType.htAlternative

                            Dim ParentsCount As Integer = mBR.ReadInt32

                            'H.AddNode(node, node.ParentNodeID) 'C0383
                            If (H.HierarchyType <> ECHierarchyType.htModel) Or
                               ((H.HierarchyType = ECHierarchyType.htModel) And ((node.ParentNodeID <> -1) Or ((node.ParentNodeID = -1) AndAlso (H.Nodes.Count = 0)))) Then 'C0731
                                If H.GetNodeByID(node.NodeGuidID) Is Nothing Then H.AddNode(node, If(node.IsAlternative, -1, node.ParentNodeID), True)
                            End If

                            If ParentsCount > 0 Then node.ParentNodesGuids = New List(Of Guid) 'A1000
                            For k As Integer = 1 To ParentsCount
                                Dim pGuid As Guid = New Guid(mBR.ReadBytes(16))
                                If Not node.IsAlternative Then node.ParentNodesGuids.Add(pGuid) 'A1000
                            Next

                            node.MeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                                    node.RatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.RegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.AdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.StepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            node.FeedbackMeasureType(False) = CType(mBR.ReadInt32, ECMeasureType)
                            Select Case node.FeedbackMeasureType
                                Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                                    node.FeedbackRatingScaleID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtRegularUtilityCurve
                                    node.FeedbackRegularUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtAdvancedUtilityCurve
                                    node.FeedbackAdvancedUtilityCurveID(False) = mBR.ReadInt32
                                Case ECMeasureType.mtStep
                                    node.FeedbackStepFunctionID(False) = mBR.ReadInt32
                                Case Else
                                    mBR.ReadInt32()
                            End Select

                            'If node.IsAlternative And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                            If node.IsAlternative And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                                node.MeasureType = ECMeasureType.mtRatings
                                node.RatingScaleID = -1
                            End If

                            node.MeasureMode = CType(mBR.ReadInt32, ECMeasureMode)
                            node.Enabled = mBR.ReadBoolean

                            Dim defDI As Int32 = mBR.ReadInt32

                            node.Comment = mBR.ReadString

                            ' new to 1.1.46
                            node.DataMappingGUID = New Guid(mBR.ReadBytes(16))
                            node.NodeMappedID = mBR.ReadInt32
                            node.EventType = CType(mBR.ReadInt32, EventType)

                            Dim cost As String = mBR.ReadString
                            If (cost <> UNDEFINED_COST_VALUE) And (node.IsAlternative) Then
                                node.Tag = cost
                            End If

                            Dim count As Integer = mBR.ReadInt32
                            If count <> 0 Then
                                Dim bytes As Byte() = mBR.ReadBytes(count)
                                Dim stream As New MemoryStream(bytes)
                                Select Case H.HierarchyType
                                    Case ECHierarchyType.htModel
                                        node.AHPNodeData = New clsAHPNodeData
                                        node.AHPNodeData.FromStream(stream)
                                    Case ECHierarchyType.htAlternative
                                        node.AHPAltData = New clsAHPAltData
                                        node.AHPAltData.FromStream(stream)
                                End Select
                            End If
                            'C0343==
                        Else
                            'C0738===
                            If subChunk = CHUNK_HIERARCHY Then
                                bReadHiearchyChunk = False
                                chunk = CHUNK_HIERARCHY
                                HierarchyChunkSize = NodeChunkSize
                                j = nodesCount - 1 'to skip nodes after increment below
                            Else
                                Return False
                            End If
                            'C0738===

                            'Return False 'C0738
                        End If
                        j += 1
                    End While
                Else
                    Return False
                End If
            Else
                Return False
            End If
            i += 1
        End While

        For Each H In ProjectManager.GetAllHierarchies
            ProjectManager.CreateHierarchyLevelValues(H)
            H.FixChildrenLinks()
        Next

        If mWriteStatus Then
            Debug.Print("ProcessHierarchies " + Now.ToString + " StreamSize: " + mStream.Length.ToString)
        End If

        Return True
    End Function


    Public Overrides Function ReadEventsGroups() As Boolean
        Dim success As Boolean = True
        Try
            mBR = New BinaryReader(mStream)

            ProjectManager.EventsGroups.Groups.Clear()

            Dim gCount As Int32 = mBR.ReadInt32
            Dim j As Integer = 0

            While (mStream.Position < mStream.Length - 1) And (j <> gCount)
                Dim group As New EventsGroup
                Dim isEventsGroup As Boolean = mBR.ReadBoolean
                group.ID = New Guid(mBR.ReadBytes(16))
                group.Name = mBR.ReadString
                group.Enabled = mBR.ReadBoolean
                group.Behaviour = mBR.ReadInt32
                group.Rule = mBR.ReadInt32
                group.RuleParameterValue = mBR.ReadInt32
                Dim eCount As Integer = mBR.ReadInt32
                Dim i As Integer = 0
                While (mStream.Position < mStream.Length - 1) And (i <> eCount)
                    Dim eData As New EventGroupData
                    eData.EventGuidID = New Guid(mBR.ReadBytes(16))
                    eData.EventIntID = mBR.ReadInt32
                    eData.Precedence = mBR.ReadInt32
                    group.Events.Add(eData)
                    i += 1
                End While
                If isEventsGroup Then
                    ProjectManager.EventsGroups.Groups.Add(group)
                Else
                    ProjectManager.SourceGroups.Groups.Add(group)
                End If
                j += 1
            End While

        Catch ex As Exception
            success = False
        Finally
            'mBR.Close()
        End Try

        If success Then
            mBR.Close()
        End If

        ' There was an issue with old models when project version was 1.1.46, but really in stream is was written as 1.1.45, so it was failing on upload
        ' This is why if it failed to load 1.1.46, we try to load as 11.45
        ' See case 17170

        If Not success Then
            Try
                success = True
                mBR = New BinaryReader(mStream)
                mBR.BaseStream.Position = 0

                ProjectManager.EventsGroups.Groups.Clear()

                Dim gCount As Int32 = mBR.ReadInt32
                Dim j As Integer = 0

                While (mStream.Position < mStream.Length - 1) And (j <> gCount)
                    Dim group As New EventsGroup
                    group.ID = New Guid(mBR.ReadBytes(16))
                    group.Name = mBR.ReadString
                    Dim eCount As Integer = mBR.ReadInt32
                    Dim i As Integer = 0
                    While (mStream.Position < mStream.Length - 1) And (i <> eCount)
                        Dim eData As New EventGroupData
                        eData.EventGuidID = New Guid(mBR.ReadBytes(16))
                        eData.EventIntID = mBR.ReadInt32
                        eData.Precedence = mBR.ReadInt32
                        group.Events.Add(eData)
                        i += 1
                    End While
                    ProjectManager.EventsGroups.Groups.Add(group)
                    j += 1
                End While

            Catch ex As Exception
                success = False
            Finally
                mBR.Close()
            End Try
        End If

        Return success
    End Function
End Class
