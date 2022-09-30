Imports System.Web.UI.WebControls
Imports System.Collections.Specialized
Imports ExpertChoice.Data
Imports ExpertChoice.Service
Imports ECCore

Namespace ExpertChoice.Data

#Region "Project Comparer"

    ' D0233 ===
    Public Enum ecProjectSort
        srtNoSort = -1
        srtProjectName = 0
        srtProjectPasscode = 1
        srtProjectStatus = 3
        'srtProjectParticipating = 4 ' D0372 -D0748
        srtProjectPublic = 4        ' D0748
        srtProjectVersion = 5       ' D0372
        srtProjectWorkgroup = 6     ' D0372
        srtProjectDateTime = 7      ' D0405
        srtMeetingID = 8            ' D0420
        srtProjectOnline = 9        ' D0748
        srtProjectDeleted = 10      ' D0789
    End Enum
    ' D0233 ==

    ' D0233 ===
    Public Class clsProjectComparer
        Implements IComparer(Of clsProject) ' D0483 + D0512

        Private _SortField As ecProjectSort = ecProjectSort.srtProjectName
        Private _Direction As SortDirection = SortDirection.Descending
        Private _Workgroups As List(Of clsWorkgroup) = Nothing  ' D0372 + D0466
        'Private _DateTimes As NameValueCollection   ' D0405 -D0512

        ' D0372 ===
        Public Property Workgroups() As List(Of clsWorkgroup)   ' D0466
            Get
                Return _Workgroups
            End Get
            Set(ByVal value As List(Of clsWorkgroup))   ' D0466
                _Workgroups = value
            End Set
        End Property

        '' D0405 ===
        ' -D0512
        'Public Property DateTimes() As NameValueCollection
        '    Get
        '        Return _DateTimes
        '    End Get
        '    Set(ByVal value As NameValueCollection)
        '        _DateTimes = value
        '    End Set
        'End Property
        '
        'Private Function ProjectDateTime(ByVal ProjectID As Integer) As Long
        '    Dim DT As Long = -1
        '    If Not DateTimes Is Nothing Then
        '        Dim sVal As String = DateTimes(ProjectID.ToString)
        '        If Not Long.TryParse(sVal, DT) Then DT = 0
        '    End If
        '    Return DT
        'End Function
        '' D0405 ==

        ' D0372 ==

        'Public Sub New(ByVal tSortField As ecProjectSort, ByVal tDirection As SortDirection, Optional ByVal tWorkgroupsList As List(Of clsWorkgroup) = Nothing, Optional ByVal tDateTimes As NameValueCollection = Nothing)   ' D0372 + D0405 + D0466
        Public Sub New(ByVal tSortField As ecProjectSort, ByVal tDirection As SortDirection, ByVal tWorkgroupsList As List(Of clsWorkgroup))   ' D0372 + D0405 + D0466 + D0512
            _SortField = tSortField
            _Direction = tDirection
            _Workgroups = tWorkgroupsList   ' D0372
            '_DateTimes = tDateTimes     ' D0405 -D0512
        End Sub

        Public Function Compare(ByVal A As clsProject, ByVal B As clsProject) As Integer Implements IComparer(Of clsProject).Compare    ' D0483 + D0512
            Dim Res As Integer = 0
            Select Case _SortField
                Case ecProjectSort.srtNoSort
                    Res = CInt(IIf(A.ID = B.ID, 0, IIf(A.ID > B.ID, 1, -1)))
                Case ecProjectSort.srtProjectName
                    Res = String.Compare(A.ProjectName, B.ProjectName, True)
                Case ecProjectSort.srtProjectPasscode
                    Res = String.Compare(A.Passcode, B.Passcode, True)
                Case ecProjectSort.srtProjectStatus
                    Res = CInt(IIf(A.ProjectStatus = B.ProjectStatus, 0, IIf(CInt(A.ProjectStatus) > CInt(B.ProjectStatus), 1, -1)))    ' D0372
                    ' D0372 ===
                    ' -D0748
                    'Case ecProjectSort.srtProjectParticipating
                    '    Res = CInt(IIf(A.ProjectParticipating = B.ProjectParticipating, 0, IIf(CInt(A.ProjectParticipating) > CInt(B.ProjectParticipating), 1, -1)))
                    ' D0748 ===
                Case ecProjectSort.srtProjectPublic
                    Res = CInt(IIf(A.isPublic = B.isPublic, 0, IIf(CInt(A.isPublic) > CInt(B.isPublic), 1, -1)))
                Case ecProjectSort.srtProjectOnline
                    Res = CInt(IIf(A.isOnline = B.isOnline, 0, IIf(CInt(A.isOnline) > CInt(B.isOnline), 1, -1)))
                    ' D0748 ==
                Case ecProjectSort.srtProjectVersion
                    Dim AVer As String = A.DBVersion.GetVersionString
                    Dim BVer As String = B.DBVersion.GetVersionString   ' D0402
                    Res = String.Compare(AVer, BVer, True)   ' D0402
                Case ecProjectSort.srtProjectWorkgroup
                    Dim AW As String = clsWorkgroup.WorkgroupNameByID(A.WorkgroupID, Workgroups)    ' D0466
                    Dim BW As String = clsWorkgroup.WorkgroupNameByID(B.WorkgroupID, Workgroups)    ' D0466
                    If AW <> "" And BW <> "" Then Return String.Compare(AW, BW, True)
                    Res = CInt(IIf(A.WorkgroupID = B.WorkgroupID, 0, IIf(A.WorkgroupID > B.WorkgroupID, 1, -1)))
                    ' D0372 ==
                    ' D0405 ===
                Case ecProjectSort.srtProjectDateTime
                    ' D0512 ===
                    If Not A.LastModify.HasValue AndAlso Not B.LastModify.HasValue Then Return 0
                    If Not A.LastModify.HasValue AndAlso B.LastModify.HasValue Then Return 1
                    If A.LastModify.HasValue AndAlso Not B.LastModify.HasValue Then Return -1
                    Res = DateTime.Compare(A.LastModify.Value, B.LastModify.Value)
                    ' D0405 + D0512 ==
                    ' D0420 ===
                Case ecProjectSort.srtMeetingID
                    Res = CInt(IIf(A.MeetingID = B.MeetingID, 0, IIf(A.MeetingID > B.MeetingID, 1, -1)))
                    ' D0420 ==
                    ' D0789 ===
                Case ecProjectSort.srtProjectDeleted
                    Res = CInt(IIf(A.isMarkedAsDeleted = B.isMarkedAsDeleted, 0, IIf(A.isMarkedAsDeleted, 1, -1)))
                    ' D0789 ==
            End Select
            If _Direction = SortDirection.Descending And Res <> 0 Then Return -Res Else Return Res
        End Function
    End Class
    ' D0233 ==

#End Region

#Region "Users comparer"

    ' D1561 ===
    Public Enum ecUserSort
        usNoSort = -1
        usEmail = 0
        usName = 1
        usActive = 4
        usKeyPad = 6
    End Enum

    Public Class clsUserComparer
        Implements IComparer(Of clsUser)

        Private _SortField As ecUserSort = ecUserSort.usEmail
        Private _Direction As SortDirection = SortDirection.Descending

        Public Sub New(ByVal tSortField As ecUserSort, ByVal tDirection As SortDirection)
            _SortField = tSortField
            _Direction = tDirection
        End Sub

        Public Function Compare(ByVal A As clsUser, ByVal B As clsUser) As Integer Implements IComparer(Of clsUser).Compare    ' D0483
            Dim Res As Integer = 0
            Select Case _SortField
                Case ecUserSort.usNoSort
                    Res = CInt(IIf(A.UserID = B.UserID, 0, IIf(A.UserID > B.UserID, 1, -1)))
                Case ecUserSort.usEmail
                    Res = String.Compare(A.UserEMail, B.UserEMail, True)
                Case ecUserSort.usName
                    If A.UserName = "" Then A.UserName = A.UserEMail
                    If B.UserName = "" Then B.UserName = B.UserEMail
                    Res = String.Compare(A.UserName, B.UserName, True)
                Case ecUserSort.usActive
                    Res = CInt(IIf(A.Active = B.Active, 0, IIf(A.Active, -1, 1)))
                Case ecUserSort.usKeyPad
                    If A.SyncEvaluationMode = B.SyncEvaluationMode AndAlso A.SyncEvaluationMode = SynchronousEvaluationMode.semVotingBox Then
                        Res = CInt(IIf(A.VotingBoxID = B.VotingBoxID, 0, IIf(A.VotingBoxID > B.VotingBoxID, 1, -1)))
                    Else
                        If A.SyncEvaluationMode = SynchronousEvaluationMode.semVotingBox Then Res = 1
                        If B.SyncEvaluationMode = SynchronousEvaluationMode.semVotingBox Then Res = 1
                        If Res = 0 Then Res = String.Compare(A.UserEMail, B.UserEMail)
                    End If
            End Select
            If _Direction = SortDirection.Descending And Res <> 0 Then Return -Res Else Return Res
        End Function
    End Class
    ' D1561 ==

#End Region

#Region "ApplicationUsers comparer"

    Public Enum ecApplicationUserSort
        usNoSort = -1
        usEmail = 0
        usName = 1
        usStatus = 2
        usRoleGroup = 3
        usActive = 4
        usProjectRole = 5       ' D0451
        usKeyPad = 6            ' D1561
        usExpirationDate = 8    ' D0419
        'usHasData = 9           ' D0313
    End Enum

    ' D0162 ===
    Public Class clsApplicationUserComparer
        Implements IComparer(Of clsApplicationUser) ' D0483

        Private _SortField As ecApplicationUserSort = ecApplicationUserSort.usEmail
        Private _Direction As SortDirection = SortDirection.Descending
        Private _Workgroup As clsWorkgroup = Nothing    ' D0466
        Private _Project4Sort As clsProject = Nothing   ' D0313
        Private _ProjectWorkspaces As List(Of clsWorkspace) = Nothing   ' D0451 + D0466
        Private _UserWorkgroups As List(Of clsUserWorkgroup) = Nothing  ' D0466

        Public Sub New(ByVal tSortField As ecApplicationUserSort, ByVal tDirection As SortDirection)
            _SortField = tSortField
            _Direction = tDirection
        End Sub

        ' D0466 ===
        Public Property Workgroup() As clsWorkgroup
            Get
                Return _Workgroup
            End Get
            Set(ByVal value As clsWorkgroup)
                _Workgroup = value
            End Set
        End Property

        Public Property UserWorkgroups() As List(Of clsUserWorkgroup)
            Get
                Return _UserWorkgroups
            End Get
            Set(ByVal value As List(Of clsUserWorkgroup))
                _UserWorkgroups = value
            End Set
        End Property
        ' D0466 ==

        Public Property ProjectForSort() As clsProject
            Get
                Return _Project4Sort
            End Get
            Set(ByVal value As clsProject)
                _Project4Sort = value
            End Set
        End Property

        ' D0451 ===
        Public Property ProjectWorkspaces() As List(Of clsWorkspace)    ' D0466
            Get
                Return _ProjectWorkspaces
            End Get
            Set(ByVal value As List(Of clsWorkspace))  ' D0466
                _ProjectWorkspaces = value
            End Set
        End Property
        ' D0451 ==

        Public Function Compare(ByVal A As clsApplicationUser, ByVal B As clsApplicationUser) As Integer Implements IComparer(Of clsApplicationUser).Compare    ' D0483
            Dim Res As Integer = 0
            Select Case _SortField
                Case ecApplicationUserSort.usNoSort
                    Res = CInt(IIf(A.UserID = B.UserID, 0, IIf(A.UserID > B.UserID, 1, -1)))
                Case ecApplicationUserSort.usEmail
                    Res = String.Compare(A.UserEmail, B.UserEmail, True)
                Case ecApplicationUserSort.usName
                    If A.UserName = "" Then A.UserName = A.UserEmail ' D1561
                    If B.UserName = "" Then B.UserName = B.UserEmail ' D1561
                    Res = String.Compare(A.UserName, B.UserName, True)
                Case ecApplicationUserSort.usActive
                    Res = CInt(IIf(A.Active = B.Active, 0, IIf(A.Active, -1, 1)))
                    ' D1561 ===
                Case ecApplicationUserSort.usKeyPad
                    If A.SyncEvaluationMode = B.SyncEvaluationMode AndAlso A.SyncEvaluationMode = SynchronousEvaluationMode.semVotingBox Then
                        Res = CInt(IIf(A.VotingBoxID = B.VotingBoxID, 0, IIf(A.VotingBoxID > B.VotingBoxID, 1, -1)))
                    Else
                        If A.SyncEvaluationMode = SynchronousEvaluationMode.semVotingBox Then Res = 1
                        If B.SyncEvaluationMode = SynchronousEvaluationMode.semVotingBox Then Res = 1
                        If Res = 0 Then Res = String.Compare(A.UserEmail, B.UserEmail)
                    End If
                    ' D1561 ==
                Case ecApplicationUserSort.usRoleGroup
                    If Not Workgroup Is Nothing Then
                        Dim UWA As Data.clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(A.UserID, Workgroup.ID, UserWorkgroups) ' D0466
                        Dim UWB As Data.clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(B.UserID, Workgroup.ID, UserWorkgroups) ' D0466
                        If Not UWA Is Nothing And Not UWB Is Nothing Then
                            If UWA.RoleGroupID = UWB.RoleGroupID Then Return 0
                            Dim RGA As clsRoleGroup = Workgroup.RoleGroup(UWA.RoleGroupID)  ' D0466
                            Dim RGB As clsRoleGroup = Workgroup.RoleGroup(UWB.RoleGroupID)  ' D0466
                            If Not RGA Is Nothing And Not RGB Is Nothing Then
                                Res = String.Compare(RGA.Name.ToLower, RGB.Name.ToLower)
                            Else
                                If Not RGA Is Nothing Then Res = 1
                                If Not RGB Is Nothing Then Res = -1
                            End If
                        Else
                            If Not UWA Is Nothing Then Res = 1
                            If Not UWB Is Nothing Then Res = -1
                        End If
                    End If
                    ' D0451 ===
                Case ecApplicationUserSort.usProjectRole
                    If Not ProjectWorkspaces Is Nothing AndAlso Not ProjectForSort Is Nothing Then
                        Dim AWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(A.UserID, ProjectForSort.ID, ProjectWorkspaces)    ' D0466
                        Dim BWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(B.UserID, ProjectForSort.ID, ProjectWorkspaces)   ' D0466
                        Res = CInt(IIf(AWS.GroupID = BWS.GroupID, 0, IIf(AWS.GroupID > BWS.GroupID, 1, -1)))
                    End If
                    Return 0
                    ' D0451 ==
                Case ecApplicationUserSort.usStatus
                    Dim SA As Integer = CInt(A.Status)
                    Dim SB As Integer = CInt(B.Status)
                    Res = CInt(IIf(SA = SB, 0, IIf(SA > SB, 1, -1)))
                    ' D0419 ===
                Case ecApplicationUserSort.usExpirationDate
                    Res = 0
                    If Not Workgroup Is Nothing Then
                        Dim UWA As Data.clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(A.UserID, Workgroup.ID, UserWorkgroups) ' D0466
                        Dim UWB As Data.clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(B.UserID, Workgroup.ID, UserWorkgroups) ' D0466
                        If Not UWA Is Nothing And Not UWB Is Nothing Then
                            Dim AExp As ULong = 0
                            Dim BExp As ULong = 0
                            If UWA.ExpirationDate.HasValue Then AExp = Date2ULong(UWA.ExpirationDate.Value)
                            If UWB.ExpirationDate.HasValue Then BExp = Date2ULong(UWB.ExpirationDate.Value)
                            Res = CInt(IIf(AExp = BExp, 0, IIf(AExp < BExp, 1, -1)))
                        End If
                    End If
            End Select
            If _Direction = SortDirection.Descending And Res <> 0 Then Return -Res Else Return Res
        End Function
    End Class
    ' D0162 ==

#End Region

#Region "Workgroup Comparer"
    ' D0176 ===
    Public Class clsWorkgroupComparer
        Implements IComparer(Of clsWorkgroup)   ' D0471

        ' D0590 ===
        Private _sStartupWkgName As String

        Public Property StartupWorkgroupName() As String
            Get
                Return _sStartupWkgname
            End Get
            Set(ByVal value As String)
                _sStartupWkgname = value
            End Set
        End Property
        ' D0590 ==

        Public Function Compare(ByVal W1 As clsWorkgroup, ByVal W2 As clsWorkgroup) As Integer Implements IComparer(Of clsWorkgroup).Compare    ' D0471
            If W1.Status <> W2.Status Then
                If W1.Status = ecWorkgroupStatus.wsSystem Then Return -1
                If W2.Status = ecWorkgroupStatus.wsSystem Then Return 1
            End If
            If W1.Name = W2.Name Then Return 0 ' D1175
            ' D0590 ===
            If StartupWorkgroupName <> "" Then
                If String.Compare(W1.Name, StartupWorkgroupName) = 0 Then Return -1
                If String.Compare(W2.Name, StartupWorkgroupName) = 0 Then Return 1
            End If
            ' D0590 ==
            Return String.Compare(W1.Name, W2.Name, True)
        End Function

    End Class
    ' D0176 ==
#End Region

#Region "clsIntensityComparer"
    ' D0160 ===
    Public Class clsIntensityComparer
        Implements IComparer(Of clsRating) 'A1507

        Private _Direction As SortDirection = SortDirection.Descending

        Public Sub New(ByVal tDirection As SortDirection)
            _Direction = tDirection
        End Sub

        Public Function Compare(ByVal A As clsRating, ByVal B As clsRating) As Integer Implements IComparer(Of clsRating).Compare 'A1507
            Dim Res As Integer = 0
            Res = CInt(IIf(A.Value = B.Value, IIf(A.ID = B.ID, 0, IIf(A.ID < B.ID, 1, -1)), IIf(A.Value > B.Value, 1, -1)))
            If _Direction = 1 And Res <> 0 Then Return -Res Else Return Res
        End Function

    End Class
    ' D0160 ==
#End Region

#Region "clsIntervalsComparer"

    ' D0269 ===
    Public Class clsIntervalsComparer
        Implements IComparer(Of clsStepInterval) 'A1507

        Public Enum IntervalField
            Value
            Low
            High
            Name
        End Enum

        Private _Direction As SortDirection = SortDirection.Descending
        Private _Field As IntervalField = IntervalField.Value

        Public Sub New(ByVal tDirection As SortDirection, ByVal tField As IntervalField)    ' D0332
            _Direction = tDirection
            _Field = tField     ' D0332
        End Sub

        Public Function Compare(ByVal A As clsStepInterval, ByVal B As clsStepInterval) As Integer Implements IComparer(Of clsStepInterval).Compare 'A1507
            Dim Res As Integer = 0
            ' D0332 =
            Dim AA As Single
            Dim BB As Single
            Select Case _Field
                Case IntervalField.Low
                    AA = A.Low
                    BB = B.Low
                Case IntervalField.High
                    AA = A.High
                    BB = B.High
                Case IntervalField.Value
                    AA = A.Value
                    BB = B.Value
            End Select
            If _Field = IntervalField.Name Then
                Res = String.Compare(A.Name, B.Name, True)
            Else
                Res = CInt(IIf(AA = BB, IIf(A.Low = B.Low, 0, IIf(A.Low > B.Low, 1, -1)), IIf(AA > BB, 1, -1))) ' D0333
            End If
            ' D0332 ==
            If _Direction = 1 And Res <> 0 Then Return -Res Else Return Res
        End Function

    End Class
    ' D0269 ==
#End Region

#Region "UserWorkgroups Comparer"
    ' D2582 ===

    Public Enum ecUserWorkgroupSort
        uwsNoSort = -1
        uwsStatus = 0
        uwsExpiration = 1
        uwsCreated = 2
    End Enum

    Public Class clsUserWorkgroupComparer
        Implements IComparer(Of clsUserWorkgroup)

        Private _SortField As ecUserWorkgroupSort = ecUserWorkgroupSort.uwsCreated
        Private _Direction As SortDirection = SortDirection.Descending

        Public Sub New(ByVal tSortField As ecUserWorkgroupSort, ByVal tDirection As SortDirection)
            _SortField = tSortField
            _Direction = tDirection
        End Sub

        Public Function Compare(ByVal A As clsUserWorkgroup, ByVal B As clsUserWorkgroup) As Integer Implements IComparer(Of clsUserWorkgroup).Compare
            Dim Res As Integer = 0
            Select Case _SortField
                Case ecUserWorkgroupSort.uwsNoSort
                    Res = CInt(IIf(A.ID = B.ID, 0, IIf(A.ID > B.ID, 1, -1)))
                Case ecUserWorkgroupSort.uwsCreated
                    If Not A.Created.HasValue AndAlso Not B.Created.HasValue Then Return 0
                    If Not A.Created.HasValue Then Return -1
                    If Not B.Created.HasValue Then Return 1
                    Res = Date.Compare(A.Created.Value, B.Created.Value)
                Case ecUserWorkgroupSort.uwsExpiration
                    If Not A.ExpirationDate.HasValue AndAlso Not B.ExpirationDate.HasValue Then Return 0
                    If Not A.ExpirationDate.HasValue Then Return -1
                    If Not B.ExpirationDate.HasValue Then Return 1
                    Res = Date.Compare(A.ExpirationDate.Value, B.ExpirationDate.Value)
                Case ecUserWorkgroupSort.uwsStatus
                    Res = CInt(IIf(A.Status = B.Status, 0, IIf(A.Status > B.Status, 1, -1)))
            End Select
            If _Direction = SortDirection.Descending And Res <> 0 Then Return -Res Else Return Res
        End Function
    End Class
    ' D0162 ==

#End Region


    '#Region "Evaluation progress Comparer"

    '    ' D1407 ===
    '    Public Enum ecEvaluationProgressSort
    '        srtUserEmail = 0
    '        srtTotalSteps = 2
    '        srtEvaluatedSteps = 3
    '    End Enum

    '    Public Class clsEvaluationProgressComparer
    '        Implements IComparer(Of clsUserEvaluationData)

    '        Private _SortField As ecEvaluationProgressSort = ecEvaluationProgressSort.srtUserEmail
    '        Private _Direction As SortDirection = SortDirection.Ascending

    '        Public Sub New(ByVal tSortField As ecEvaluationProgressSort, ByVal tDirection As SortDirection)
    '            _SortField = tSortField
    '            _Direction = tDirection
    '        End Sub

    '        Public Function Compare(ByVal A As clsUserEvaluationData, ByVal B As clsUserEvaluationData) As Integer Implements IComparer(Of clsUserEvaluationData).Compare
    '            Dim Res As Integer = 0
    '            Select Case _SortField
    '                Case ecEvaluationProgressSort.srtUserEmail
    '                    Res = String.Compare(A.UserEmail, B.UserEmail, True)
    '                Case ecEvaluationProgressSort.srtTotalSteps
    '                    Res = CInt(IIf(A.EvaluationTotalSteps = B.EvaluationTotalSteps, 0, IIf(A.EvaluationTotalSteps < B.EvaluationTotalSteps, -1, 1)))
    '                Case ecEvaluationProgressSort.srtEvaluatedSteps
    '                    Res = CInt(IIf(A.EvaluationProgressSteps = B.EvaluationProgressSteps, 0, IIf(A.EvaluationProgressSteps < B.EvaluationProgressSteps, -1, 1)))
    '            End Select
    '            If _Direction = SortDirection.Descending And Res <> 0 Then Return -Res Else Return Res
    '        End Function
    '    End Class
    '    ' D1407 ==

    '#End Region


End Namespace
