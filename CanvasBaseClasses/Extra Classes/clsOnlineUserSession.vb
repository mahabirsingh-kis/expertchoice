Namespace ExpertChoice.Data

    <Serializable()> Public Class clsOnlineUserSession  ' D0501 (was clsSession)

        Private _UserID As Integer  ' D0494
        Private _UserEmail As String
        Private _LastAccess As Nullable(Of DateTime)
        Private _WorkgroupID As Integer
        Private _ProjectID As Integer
        Private _RoleGroupID As Integer ' D4818
        Private _PageID As Integer
        Private _URL As String
        Private _SessionID As String    ' D0494

        ' D0494 ===
        Public Property UserID() As Integer
            Get
                Return _UserID
            End Get
            Set(ByVal value As Integer)
                _UserID = value
            End Set
        End Property
        ' D0494 ==

        Public Property UserEmail() As String
            Get
                Return _UserEmail
            End Get
            Set(ByVal value As String)
                _UserEmail = value
            End Set
        End Property

        Public Property LastAccess() As Nullable(Of DateTime)
            Get
                Return _LastAccess
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _LastAccess = value
            End Set
        End Property

        Public Property WorkgroupID() As Integer
            Get
                Return _WorkgroupID
            End Get
            Set(ByVal value As Integer)
                _WorkgroupID = value
            End Set
        End Property

        Public Property ProjectID() As Integer
            Get
                Return _ProjectID
            End Get
            Set(ByVal value As Integer)
                _ProjectID = value
            End Set
        End Property

        Public Property PageID() As Integer
            Get
                Return _PageID
            End Get
            Set(ByVal value As Integer)
                _PageID = value
            End Set
        End Property

        Public Property URL() As String
            Get
                Return _URL
            End Get
            Set(ByVal value As String)
                _URL = value
            End Set
        End Property

        ' D4818 ===
        Public Property RoleGroupID As Integer
            Get
                Return _RoleGroupID
            End Get
            Set(value As Integer)
                _RoleGroupID = value
            End Set
        End Property
        ' D4818 ==

        ' D0494 ===
        Public Property SessionID() As String
            Get
                Return _SessionID
            End Get
            Set(ByVal value As String)
                _SessionID = value
            End Set
        End Property

        Public Function Clone() As clsOnlineUserSession
            Dim NewSession As New clsOnlineUserSession
            NewSession.UserID = Me.UserID
            NewSession.UserEmail = Me.UserEmail
            NewSession.LastAccess = Me.LastAccess
            NewSession.WorkgroupID = Me.WorkgroupID
            NewSession.PageID = Me.PageID
            NewSession.RoleGroupID = Me.RoleGroupID    ' D4818
            NewSession.URL = Me.URL
            NewSession.SessionID = Me.SessionID
            Return NewSession
        End Function
        ' D0494 ==

        ' D0501 ===
        Shared Function OnlineSessionByEmail(ByVal sEmail As String, ByVal SessionsList As List(Of clsOnlineUserSession)) As clsOnlineUserSession
            If sEmail <> "" AndAlso Not SessionsList Is Nothing Then
                For Each tSess As clsOnlineUserSession In SessionsList
                    If tSess.UserEmail = sEmail Then Return tSess
                Next
            End If
            Return Nothing
        End Function

        Shared Function OnlineSessionByUserID(ByVal tUserID As Integer, ByVal SessionsList As List(Of clsOnlineUserSession)) As clsOnlineUserSession
            If Not SessionsList Is Nothing Then
                For Each tSess As clsOnlineUserSession In SessionsList
                    If tSess.UserID = tUserID Then Return tSess
                Next
            End If
            Return Nothing
        End Function

        Shared Function OnlineSessionByUserIDProjectID(ByVal tUserID As Integer, ByVal tProjectID As Integer, ByVal SessionsList As List(Of clsOnlineUserSession)) As clsOnlineUserSession
            If Not SessionsList Is Nothing Then
                For Each tSess As clsOnlineUserSession In SessionsList
                    If tSess.UserID = tUserID AndAlso tSess.ProjectID = tProjectID Then Return tSess
                Next
            End If
            Return Nothing
        End Function

        Shared Function OnlineSessionsByProjectID(ByVal tProjectID As Integer, ByVal SessionsList As List(Of clsOnlineUserSession)) As List(Of clsOnlineUserSession)
            Dim Lst As New List(Of clsOnlineUserSession)
            If Not SessionsList Is Nothing Then
                For Each tSess As clsOnlineUserSession In SessionsList
                    If tSess.ProjectID = tProjectID Then Lst.Add(tSess)
                Next
            End If
            Return Lst
        End Function
        ' D0501 ==

        ' D1345 ===
        Shared Function OnlineSessionsByWorkgroupID(ByVal tWorkgroupID As Integer, ByVal SessionsList As List(Of clsOnlineUserSession)) As List(Of clsOnlineUserSession)
            Dim Lst As New List(Of clsOnlineUserSession)
            If Not SessionsList Is Nothing Then
                For Each tSess As clsOnlineUserSession In SessionsList
                    If tSess.WorkgroupID = tWorkgroupID Then Lst.Add(tSess)
                Next
            End If
            Return Lst
        End Function
        ' D1345 ==

        Public Sub New()
            _UserID = -1    ' D0494
            _UserEmail = ""
            _LastAccess = Nothing
            _WorkgroupID = -1
            _ProjectID = -1
            _RoleGroupID = -1   ' D4818
            _PageID = -1    '_PGID_UNKNOWN 'D0457
            _URL = ""
            _SessionID = ""     ' D0494
        End Sub

    End Class

End Namespace