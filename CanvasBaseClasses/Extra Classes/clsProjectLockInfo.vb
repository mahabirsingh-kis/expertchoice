Namespace ExpertChoice.Data

    ' D0589 ===
    Public Enum ECLockStatus
        lsUnLocked = 0
        lsLockForModify = 1
        lsLockForTeamTime = 2
        lsLockForAntigua = 3
        lsLockForSystem = 9
    End Enum
    ' D0589 ==

    <Serializable()> Public Class clsProjectLockInfo


        Private _LockerUserID As Integer    ' D0494
        'Private _LockerUser As clsApplicationUser  ' -D0494
        Private _LockExpiration As Nullable(Of DateTime)
        Private _ProjectID As Integer   ' D0483
        Private _LockStatus As ECLockStatus ' D0589

        ' D0494 ==
        Public Property LockerUserID() As Integer
            Get
                Return _LockerUserID
            End Get
            Set(ByVal value As Integer)
                _LockerUserID = value
            End Set
        End Property

        ' -D0494
        'Public ReadOnly Property LockerUserID() As Integer
        '    Get
        '        If LockerUser Is Nothing Then Return -1 Else Return LockerUser.UserID
        '    End Get
        'End Property

        ' -D0494 
        'Public ReadOnly Property LockerEmail() As String
        '    Get
        '        If LockerUser Is Nothing Then Return "" Else Return LockerUser.UserEmail
        '    End Get
        'End Property

        ' -D0494
        'Public Property LockerUser() As clsApplicationUser  ' D0483
        '    Get
        '        Return _LockerUser
        '    End Get
        '    Set(ByVal value As clsApplicationUser)
        '        _LockerUser = value
        '    End Set
        'End Property

        Public Property LockExpiration() As Nullable(Of DateTime)
            Get
                Return _LockExpiration
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _LockExpiration = value
            End Set
        End Property

        Public ReadOnly Property isExpired() As Boolean
            Get
                If Not LockExpiration.HasValue Then Return True
                Return LockExpiration.Value <= Now
            End Get
        End Property

        ' D0589 ===
        Public Property LockStatus() As ECLockStatus
            Get
                If LockerUserID <= 0 Or isExpired Then _LockStatus = ECLockStatus.lsUnLocked
                Return _LockStatus
            End Get
            Set(ByVal value As ECLockStatus)
                _LockStatus = value
            End Set
        End Property

        Public Function isLockAvailable(ByVal tUser As clsApplicationUser) As Boolean
            Return LockStatus = ECLockStatus.lsUnLocked Or isLockedByUser(tUser)
        End Function

        Public Function isLockAvailable(ByVal tUserID As Integer) As Boolean
            Return LockStatus = ECLockStatus.lsUnLocked Or isLockedByUser(tUserID)
        End Function

        Public Function isLockedByUser(ByVal tUser As clsApplicationUser) As Boolean
            Return tUser IsNot Nothing AndAlso isLockedByUser(tUser.UserID)
        End Function

        Public Function isLockedByUser(ByVal tUserID As Integer) As Boolean
            Return LockStatus <> ECLockStatus.lsUnLocked AndAlso LockerUserID = tUserID
        End Function
        ' D0589 ==

        'Public Property LockStatus() As ECLockStatus
        '    Get
        '        Return _LockStatus
        '    End Get
        '    Set(ByVal value As ECLockStatus)
        '        _LockStatus = value
        '    End Set
        'End Property


        'Public ReadOnly Property isLocked(ByVal tIgnoreUser As clsApplicationUser) As ECLockStatus ' D0474 + D0589
        '    Get
        '        ' D0589 ===
        '        If LockerUserID > 0 AndAlso Not isExpired AndAlso isLockedByUser(tIgnoreUser.UserID) <> ECLockStatus.lsUnLocked Then   ' D0494
        '            Return _LockStatus
        '        Else
        '            Return ECLockStatus.lsUnLocked
        '        End If
        '        ' D0589 ==
        '        'Return Not LockerUser Is Nothing AndAlso Not isExpired AndAlso Not isLockedByUser(tIgnoreUser)    ' D0474 '-D0494
        '    End Get
        'End Property

        '' D0494 ===
        'Public Overloads ReadOnly Property isLockedByUser(ByVal tUserID As Integer) As ECLockStatus ' D0589
        '    Get
        '        If LockerUserID <= 0 Or isExpired Then Return ECLockStatus.lsUnLocked ' D0589
        '        If LockerUserID = tUserID Then Return _LockStatus Else Return ECLockStatus.lsUnLocked ' D0589
        '    End Get
        'End Property
        '' D0494 ==

        ' -D0494
        'Public ReadOnly Property isLockedByUser(ByVal tUser As clsApplicationUser) As Boolean
        '    Get
        '        If tUser Is Nothing Or LockerUser Is Nothing Or isExpired Then Return False ' D0474
        '        Return LockerUser.UserID = tUser.UserID   ' D0474
        '    End Get
        'End Property

        ' D0483 ===
        Public Property ProjectID() As Integer
            Get
                Return _ProjectID
            End Get
            Set(ByVal value As Integer)
                _ProjectID = value
            End Set
        End Property

        Public Function Clone() As clsProjectLockInfo
            Dim newLock As New clsProjectLockInfo
            'newLock.LockerUser = Me.LockerUser ' -D0494
            newLock.LockerUserID = Me.LockerUserID  ' D0494
            newLock.LockExpiration = Me.LockExpiration
            newLock.ProjectID = Me.ProjectID
            newLock.LockStatus = Me.LockStatus  ' D0589
            Return newLock
        End Function
        ' D0483 ==

        Public Sub New()
            '_LockerUser = Nothing  ' -D0494
            _LockerUserID = -1      ' D0494
            _LockExpiration = Nothing
            _ProjectID = -1 ' D0483
            _LockStatus = ECLockStatus.lsUnLocked   ' D0589
        End Sub

    End Class

End Namespace
