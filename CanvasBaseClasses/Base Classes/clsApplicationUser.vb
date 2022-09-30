Imports System.Web.UI.WebControls
Imports ECCore.ECTypes
Imports ExpertChoice.Service

Namespace ExpertChoice.Data

    Public Enum ecUserStatus
        usUnspecified = -2    ' D0162
        usDisabled = -1
        usEnabled = 0
    End Enum

    <Serializable()> Public Class clsApplicationUser
        Inherits clsUser

        Public Const _mask_CantBeDelete As Integer = 64    ' D0104 + D0456

        Private _mOwnerID As Integer    ' D0086
        Private _mDefaultWorkgroupID As Integer ' D0094 + D0100
        Private _mStatus As ecUserStatus
        Private _mCantBeDeleted As Boolean
        Private _mPassword As String    ' D0062
        Private _mPasswordStatus As Integer  ' D2213
        Private _mComment As String
        ' D0494 ===
        Private _Created As Nullable(Of DateTime)
        Private _LastModify As Nullable(Of DateTime)
        Public LastVisited As Nullable(Of DateTime) ' D6062
        Private _isOnline As Boolean
        Private _Session As clsOnlineUserSession
        ' D0494 ==

        ' D0086 ===
        Public Property OwnerID() As Integer
            Get
                Return _mOwnerID
            End Get
            Set(ByVal value As Integer)
                _mOwnerID = value
            End Set
        End Property
        ' D0086 ==

        Public Overloads Property UserEmail() As String
            Get
                Return MyBase.UserEMail
            End Get
            Set(ByVal value As String)
                If MyBase.UserEMail <> value Then MyBase.UserEMail = value.Trim.Replace("<", "").Replace(">", "") ' D0916
            End Set
        End Property

        ' D0046 ===
        Public Overloads Property UserName() As String
            Get
                Return MyBase.UserName
            End Get
            Set(ByVal value As String)
                If MyBase.UserName <> value Then MyBase.UserName = value.Trim.Replace("<", "").Replace(">", "") ' D0916
            End Set
        End Property
        ' D0046 ==

        ' D0094 + D0100 ===
        Public Property DefaultWorkgroupID() As Integer
            Get
                Return _mDefaultWorkgroupID
            End Get
            Set(ByVal value As Integer)
                _mDefaultWorkgroupID = value
            End Set
        End Property
        ' D0094 + D0100 ==

        Public Property Status() As ecUserStatus
            Get
                Return _mStatus
            End Get
            Set(ByVal value As ecUserStatus)
                _mStatus = value
            End Set
        End Property

        ' D0104 ===
        Public Property DBStatus() As Integer
            Get
                Return _mask_CantBeDelete * CInt(IIf(CannotBeDeleted, 1, 0)) + CInt(Status)
            End Get
            Set(ByVal value As Integer)
                CannotBeDeleted = (value \ _mask_CantBeDelete) <> 0
                Status = CType(value Mod _mask_CantBeDelete, ecUserStatus)
            End Set
        End Property
        ' D0104 ==

        ' D0063 ===
        Public Property UserPassword() As String
            Get
                ' D0412 ===
                If isEncodedString(_mPassword) Then
                    _mPassword = DecodeString(_mPassword, Nothing, True)    ' D0826
                End If
                Return _mPassword
                ' D0412 ==
            End Get
            Set(ByVal value As String)
                _mPassword = value
            End Set
        End Property
        ' D0063 ==

        ' D0838 ===
        Public ReadOnly Property HasPassword() As Boolean
            Get
                Return _mPassword <> "" AndAlso _mPassword <> _DEF_ENCODED_BLANK_PSW
            End Get
        End Property
        ' D0838 ==

        ' D0095 ===
        Public Property CannotBeDeleted() As Boolean
            Get
                Return _mCantBeDeleted
            End Get
            Set(ByVal value As Boolean)
                _mCantBeDeleted = value
            End Set
        End Property
        ' D0095 ==

        ' D2213 ===
        Public Property PasswordStatus As Integer
            Get
                Return _mPasswordStatus
            End Get
            Set(value As Integer)
                If CannotBeDeleted Then value = If(value = -9, -1, 0) ' D2413 + D6446
                _mPasswordStatus = value
            End Set
        End Property
        ' D2213 ==

        Public Property Comment() As String
            Get
                Return _mComment
            End Get
            Set(ByVal value As String)
                _mComment = value
            End Set
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

        Public Property isOnline() As Boolean
            Get
                Return _isOnline
            End Get
            Set(ByVal value As Boolean)
                _isOnline = value
            End Set
        End Property

        Public Property Session() As clsOnlineUserSession
            Get
                Return _Session
            End Get
            Set(ByVal value As clsOnlineUserSession)
                _Session = value
            End Set
        End Property
        ' D0494 ==

        ' D0478 ===
        Shared Function UserByUserID(ByVal UserID As Integer, ByVal tUsersList As List(Of clsApplicationUser)) As clsApplicationUser
            If Not tUsersList Is Nothing Then
                For Each tUser As clsApplicationUser In tUsersList
                    If Not tUser Is Nothing AndAlso tUser.UserID = UserID Then Return tUser
                Next
            End If
            Return Nothing
        End Function

        Shared Function UserByUserEmail(ByVal UserEMail As String, ByVal tUsersList As List(Of clsApplicationUser)) As clsApplicationUser
            If Not tUsersList Is Nothing Then
                UserEMail = UserEMail.ToLower.Trim
                For Each tUser As clsApplicationUser In tUsersList
                    If Not tUser Is Nothing AndAlso tUser.UserEmail.ToLower.Trim = UserEMail Then Return tUser
                Next
            End If
            Return Nothing
        End Function

        Shared Function AHPUserByUserID(ByVal UserID As Integer, ByVal tUsersList As List(Of clsUser)) As clsUser
            If Not tUsersList Is Nothing Then
                For Each tUser As clsUser In tUsersList
                    If Not tUser Is Nothing AndAlso tUser.UserID = UserID Then Return tUser
                Next
            End If
            Return Nothing
        End Function

        Shared Function AHPUserByUserEmail(ByVal UserEMail As String, ByVal tUsersList As List(Of clsUser)) As clsUser
            If Not tUsersList Is Nothing Then
                UserEMail = UserEMail.ToLower.Trim
                For Each tUser As clsUser In tUsersList
                    If Not tUser Is Nothing AndAlso tUser.UserEMail.ToLower.Trim = UserEMail Then Return tUser
                Next
            End If
            Return Nothing
        End Function
        ' D0478 ==

        ''A1330 ===
        'Public Function DisplayName() As String 
        '    Return CStr(IIf(UserName = "", UserEmail, UserName))
        'End Function 'A1330 ==

        Public Overloads Function Clone() As clsApplicationUser
            Dim newUser As New clsApplicationUser
            newUser.UserID = Me.UserID
            newUser.UserEmail = Me.UserEmail
            newUser.UserPassword = Me.UserPassword
            newUser.PasswordStatus = Me.PasswordStatus  ' D2213
            newUser.UserName = Me.UserName
            newUser.Active = Me.Active
            newUser.OwnerID = Me.OwnerID    ' D0086
            newUser.DefaultWorkgroupID = Me.DefaultWorkgroupID ' D0094 + D0100
            newUser.CannotBeDeleted = Me.CannotBeDeleted    ' D0095
            newUser.Status = Me.Status
            newUser.Comment = Me.Comment
            ' D0494 ===
            newUser.Created = Me.Created
            newUser.LastModify = Me.LastModify
            newUser.isOnline = Me.isOnline
            If Not Me.Session Is Nothing Then newUser.Session = Me.Session.Clone
            ' D0494 ==
            Return newUser
        End Function

        ' D3657 ===
        Public Overloads Function Clone_clsUser() As clsUser
            Dim newUser As New clsUser
            newUser.UserID = Me.UserID
            newUser.UserEMail = Me.UserEmail
            newUser.UserName = Me.UserName
            newUser.Active = Me.Active
            Return newUser
        End Function
        ' D3657 ==

        Public Sub New()
            MyBase.New()
            _mOwnerID = 0   ' D0086
            _mDefaultWorkgroupID = -1   ' D0094 + D0100
            _mStatus = ecUserStatus.usEnabled
            _mPassword = ""     ' D0063
            _mPasswordStatus = 0 ' D2213
            _mComment = ""
            ' D0494 ===
            _Created = Nothing
            _LastModify = Nothing
            _isOnline = False
            _Session = Nothing
            ' D0494 ==
        End Sub

    End Class

End Namespace
