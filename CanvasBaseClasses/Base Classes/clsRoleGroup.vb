Imports ExpertChoice.Service    ' D0990

Namespace ExpertChoice.Data


    ' D0045 ===
    Public Enum ecRoleGroupType
        gtCustomGroup = 0
        gtUser = 1
        ' D087 ===
        gtProjectOrganizer = 2              ' Project Creator
        gtWorkgroupManager = 3  ' D2780     ' Account Manager
        gtECAccountManager = 4
        gtTechSupport = 8      ' -D2780 + D3288
        ' D0087 ==
        gtAdministrator = 9
        gtEvaluator = 10
        gtViewer = 11           ' D2239
        gtEvaluatorAndViewer = 12   ' D2857
        gtProjectManager = 19   ' D0087 +  D2780    ' Project Owner
    End Enum

    Public Enum ecRoleGroupStatus
        gsDisabled = -1
        gsEnabled = 0
        gsEvaluatorDefault = 1          ' D0052
        gsProjectManagerDefault = 2     ' D0087 + D2780
        gsUserDefault = 3               ' D0052
        gsViewerDefault = 4             ' D2239
        gsProjectOrganizerDefault = 5   ' D0087 + D2780
        gsWorkgroupManagerDefault = 6   ' D0068 + D0087
        gsECAccountManagerDefault = 7   ' D0087
        gsTechSupportDefault = 8        ' D0087    ' - D2780 + D3288
        gsAdministratorDefault = 9      ' D0068 + D0087
        gsEvaluatorAndViewerDefault = 10    ' D2857
    End Enum
    ' D0045 ==

    <Serializable()> Public Class clsRoleGroup

        Private _mID As Integer
        'Private _mPID As Integer
        Private _mWorkgroupID As Integer        ' D0045
        Private _mGroupType As ecRoleGroupType  ' D0045
        Private _mRoleLevel As ecRoleLevel
        Private _mStatus As ecRoleGroupStatus
        Private _mName As String
        Private _mComment As String
        Private _mActions As List(Of clsRoleAction) ' D0464
        Private _Created As Nullable(Of DateTime)   ' D0494
        Private _LastModify As Nullable(Of DateTime)    ' D0494

        Public Property ID() As Integer
            Get
                Return _mID
            End Get
            Set(ByVal value As Integer)
                _mID = value
            End Set
        End Property

        'Public Property ParentID() As Integer
        '    Get
        '        Return _mPID
        '    End Get
        '    Set(ByVal value As Integer)
        '        _mPID = value
        '    End Set
        'End Property

        ' D0045 ===
        Public Property WorkgroupID() As Integer
            Get
                Return _mWorkgroupID
            End Get
            Set(ByVal value As Integer)
                _mWorkgroupID = value
            End Set
        End Property

        Public Property GroupType() As ecRoleGroupType
            Get
                Return _mGroupType
            End Get
            Set(ByVal value As ecRoleGroupType)
                _mGroupType = value
            End Set
        End Property

        Public Property Status() As ecRoleGroupStatus
            Get
                Return _mStatus
            End Get
            Set(ByVal value As ecRoleGroupStatus)
                _mStatus = value
            End Set
        End Property
        ' D0045 ==

        Public Property RoleLevel() As ecRoleLevel
            Get
                Return _mRoleLevel
            End Get
            Set(ByVal value As ecRoleLevel)
                _mRoleLevel = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return _mName
            End Get
            Set(ByVal value As String)
                _mName = substring(value.Trim, 250)    ' D0990
            End Set
        End Property

        Public Property Comment() As String
            Get
                Return _mComment
            End Get
            Set(ByVal value As String)
                _mComment = value
            End Set
        End Property

        Public Property Actions() As List(Of clsRoleAction) ' D0464
            Get
                Return _mActions
            End Get
            Set(ByVal value As List(Of clsRoleAction)) ' D0464
                _mActions = value
            End Set
        End Property

        Public Overloads ReadOnly Property Action(ByVal ActionID As Integer) As clsRoleAction
            Get
                Return clsRoleGroup.ActionByID(ActionID, Actions)   ' D0727
            End Get
        End Property

        Public Overloads ReadOnly Property Action(ByVal ActionType As ecActionType) As clsRoleAction
            Get
                Return clsRoleGroup.ActionByType(ActionType, Actions)   ' D0727
            End Get
        End Property

        ' D0727 ===
        Shared ReadOnly Property ActionByID(ByVal ActionID As Integer, ByVal ActionsList As List(Of clsRoleAction)) As clsRoleAction
            Get
                If Not ActionsList Is Nothing Then
                    For Each tAction As clsRoleAction In ActionsList
                        If tAction IsNot Nothing AndAlso tAction.ID = ActionID Then Return tAction
                    Next
                End If
                Return Nothing
            End Get
        End Property

        Shared ReadOnly Property ActionByType(ByVal ActionType As ecActionType, ByVal ActionsList As List(Of clsRoleAction)) As clsRoleAction
            Get
                If Not ActionsList Is Nothing Then
                    For Each tAction As clsRoleAction In ActionsList
                        If tAction IsNot Nothing AndAlso tAction.ActionType = ActionType Then Return tAction
                    Next
                End If
                Return Nothing
            End Get
        End Property
        ' D0727 ==

        Public Overloads ReadOnly Property ActionStatus(ByVal ActionType As ecActionType) As ecActionStatus
            Get
                Dim curAction As clsRoleAction = Action(ActionType)
                If curAction Is Nothing Then Return ecActionStatus.asUnspecified Else Return curAction.Status ' D0049
            End Get
        End Property

        ' D0494 ===
        Public Property Created() As Nullable(Of DateTime)
            Get
                Return _Created
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _Created = value
            End Set
        End Property

        Public Property LastModify() As Nullable(Of DateTime)
            Get
                Return _LastModify
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _LastModify = value
            End Set
        End Property
        ' D0494 ==

        Public Function Clone() As clsRoleGroup
            Dim newGroup As New clsRoleGroup()
            newGroup.ID = Me.ID
            'newGroup.ParentID = Me.ParentID
            newGroup.WorkgroupID = Me.WorkgroupID   ' D0045
            newGroup.Name = Me.Name
            newGroup.GroupType = Me.GroupType   ' D0045
            newGroup.RoleLevel = Me.RoleLevel   ' D0045
            newGroup.Status = Me.Status         ' D0045
            newGroup.Comment = Me.Comment
            newGroup.Created = Me.Created   ' D0494
            newGroup.LastModify = Me.LastModify     ' D0494
            For Each tAction As clsRoleAction In Me.Actions
                newGroup.Actions.Add(tAction.Clone)
            Next
            Return newGroup
        End Function

        Public Sub New()
            _mID = 0
            '_mPID = -1
            ' D0045 ===
            _mWorkgroupID = -1
            _mGroupType = ecRoleGroupType.gtCustomGroup
            _mRoleLevel = ecRoleLevel.rlApplicationLevel
            _mStatus = ecRoleGroupStatus.gsEnabled
            ' D0045 ==
            _mName = ""
            _mComment = ""
            _Created = Nothing  ' D0494
            _LastModify = Nothing   ' D0494
            _mActions = New List(Of clsRoleAction) ' D0464
        End Sub

    End Class

End Namespace
