Namespace ExpertChoice.Data

    Public Enum ecUserWorkgroupStatus
        uwDisabled = -1
        uwEnabled = 0
    End Enum

    <Serializable()> Public Class clsUserWorkgroup

        Private _mID As Integer
        Private _mUserID As Integer
        Private _mWorkgroupID As Integer
        Private _mRoleGroupID As Integer
        Private _mStatus As ecUserWorkgroupStatus
        Private _mComment As String
        Private _mExpirationDate As Nullable(Of DateTime)   ' D0419
        Private _Created As Nullable(Of DateTime)   ' D0494
        'Private _LastModify As Nullable(Of DateTime)    ' D0494
        Private _LastVisited As Nullable(Of DateTime)    ' D0494 + D4622
        Private _LastProjectID As Integer   ' D0494

        Public Property ID() As Integer
            Get
                Return _mID
            End Get
            Set(ByVal value As Integer)
                _mID = value
            End Set
        End Property

        Public Property UserID() As Integer
            Get
                Return _mUserID
            End Get
            Set(ByVal value As Integer)
                _mUserID = value
            End Set
        End Property

        Public Property WorkgroupID() As Integer
            Get
                Return _mWorkgroupID
            End Get
            Set(ByVal value As Integer)
                _mWorkgroupID = value
            End Set
        End Property

        Public Property RoleGroupID() As Integer
            Get
                Return _mRoleGroupID
            End Get
            Set(ByVal value As Integer)
                _mRoleGroupID = value
            End Set
        End Property

        Public Property Status() As ecUserWorkgroupStatus
            Get
                Return _mStatus
            End Get
            Set(ByVal value As ecUserWorkgroupStatus)
                _mStatus = value
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

        ' D0419 ===
        Public Property ExpirationDate() As Nullable(Of DateTime)
            Get
                Return _mExpirationDate
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _mExpirationDate = value
            End Set
        End Property

        Public ReadOnly Property isExpired() As Boolean
            Get
                If Not ExpirationDate.HasValue Then Return False Else Return ExpirationDate.Value < Now
            End Get
        End Property
        ' D0419 ==

        ' D0494 ===
        Public Property Created() As Nullable(Of DateTime)
            Get
                Return _Created
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _Created = value
            End Set
        End Property

        'Public Property LastModify() As Nullable(Of DateTime)
        '    Get
        '        Return _LastModify
        '    End Get
        '    Set(ByVal value As Nullable(Of DateTime))
        '        _LastModify = value
        '    End Set
        'End Property

        Public Property LastVisited() As Nullable(Of DateTime)
            Get
                Return _LastVisited
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _LastVisited = value
            End Set
        End Property

        Public Property LastProjectID() As Integer
            Get
                Return _LastProjectID
            End Get
            Set(ByVal value As Integer)
                _LastProjectID = value
            End Set
        End Property

        ' D0494 ==

        Public Overloads Function Clone() As clsUserWorkgroup
            Dim newUW As New clsUserWorkgroup
            newUW.ID = Me.ID
            newUW.UserID = Me.UserID
            newUW.WorkgroupID = Me.WorkgroupID
            newUW.RoleGroupID = Me.RoleGroupID
            newUW.Status = Me.Status
            newUW.Comment = Me.Comment
            newUW.ExpirationDate = Me.ExpirationDate    ' D0419
            newUW.Created = Me.Created  ' D0494
            'newUW.LastModify = Me.LastModify ' D0494
            newUW.LastVisited = Me.LastVisited ' D0494 + D4622
            newUW.LastProjectID = Me.LastProjectID  ' D0494
            Return newUW
        End Function

        Public Sub New()
            MyBase.New()
            _mID = 0
            _mUserID = -1
            _mWorkgroupID = -1
            _mRoleGroupID = -1
            _mStatus = ecUserWorkgroupStatus.uwEnabled
            _mComment = ""
            _mExpirationDate = Nothing  ' D0419
            _Created = Nothing  ' D0494
            '_LastModify = Nothing   ' D0494
            _LastVisited = Nothing   ' D0494 + D4622
            _LastProjectID = -1     ' D0494
        End Sub

        ' D0465 ===
        Public Shared Function UserWorkgroupByID(ByVal tID As Integer, ByVal tUserWorkgroupsList As List(Of clsUserWorkgroup)) As clsUserWorkgroup
            If Not tUserWorkgroupsList Is Nothing Then
                For Each tUW As clsUserWorkgroup In tUserWorkgroupsList
                    If tUW IsNot Nothing AndAlso tUW.ID = tID Then Return tUW ' D0604
                Next
            End If
            Return Nothing
        End Function

        Public Shared Function UserWorkgroupsByWorkgroupID(ByVal tWorkgroupID As Integer, ByVal tUserWorkgroupsList As List(Of clsUserWorkgroup)) As List(Of clsUserWorkgroup)
            Dim tList As New List(Of clsUserWorkgroup)
            If Not tUserWorkgroupsList Is Nothing Then
                For Each tUW As clsUserWorkgroup In tUserWorkgroupsList
                    If tUW IsNot Nothing AndAlso tUW.WorkgroupID = tWorkgroupID Then tList.Add(tUW) ' D0604
                Next
            End If
            Return tList
        End Function

        Public Shared Function UserWorkgroupsByUserID(ByVal tUserID As Integer, ByVal tUserWorkgroupsList As List(Of clsUserWorkgroup)) As List(Of clsUserWorkgroup)
            Dim tList As New List(Of clsUserWorkgroup)
            If Not tUserWorkgroupsList Is Nothing Then
                For Each tUW As clsUserWorkgroup In tUserWorkgroupsList
                    If tUW IsNot Nothing AndAlso tUW.UserID = tUserID Then tList.Add(tUW) ' D0604
                Next
            End If
            Return tList
        End Function

        Public Shared Function UserWorkgroupByUserIDAndWorkgroupID(ByVal tUserID As Integer, ByVal tWorkgroupID As Integer, ByVal tUserWorkgroupsList As List(Of clsUserWorkgroup)) As clsUserWorkgroup
            If Not tUserWorkgroupsList Is Nothing Then
                For Each tUW As clsUserWorkgroup In tUserWorkgroupsList
                    If tUW IsNot Nothing AndAlso tUW.WorkgroupID = tWorkgroupID AndAlso tUW.UserID = tUserID Then Return tUW ' D0604
                Next
            End If
            Return Nothing
        End Function
        ' D0465 ==

    End Class

End Namespace