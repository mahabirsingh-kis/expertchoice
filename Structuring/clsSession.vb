Namespace ExpertChoice.Structuring

    <Serializable> Public Class clsSession

        Public Sub New(ByVal UserInfo As UserToken)
            mUserToken = UserInfo
        End Sub

        Private mUserToken As UserToken
        Public Property UserToken() As UserToken
            Get
                Return mUserToken
            End Get
            Set(value As UserToken)
                mUserToken = value
            End Set
        End Property

        Public Sub OperationResult(ByVal e As AntiguaOperationEventArgs)
            Exit Sub ' do nothing
        End Sub

        Public Sub LogMessage(ByVal Text As String)
            Exit Sub
            'TODO
        End Sub

    End Class

    <KnownType(GetType(ClientType))>
    <Serializable> Public Class UserToken

        Private mTokenID As Integer

        Public Property TokenID() As Integer
            Get
                Return mTokenID
            End Get
            Set(ByVal value As Integer)
                mTokenID = value
            End Set
        End Property

        Private mUserName As String

        Public Property UserName() As String
            Get
                Return mUserName
            End Get
            Set(ByVal value As String)
                mUserName = value
            End Set
        End Property

        Private mEmail As String

        Public Property Email() As String
            Get
                Return mEmail
            End Get
            Set(ByVal value As String)
                mEmail = value
            End Set
        End Property

        Private mClientType As ClientType

        Public Property ClientType() As ClientType
            Get
                Return mClientType
            End Get
            Set(ByVal value As ClientType)
                mClientType = value
            End Set
        End Property

        Private mMeetingID As Integer

        Public Property MeetingID() As Integer
            Get
                Return mMeetingID
            End Get
            Set(ByVal value As Integer)
                mMeetingID = value
            End Set
        End Property

        Private mProjectName As String = "" 'A0214 'A0217

        Public Property ProjectName() As String 'A0214
            Get
                Return mProjectName
            End Get
            Set(ByVal value As String)
                mProjectName = value
            End Set
        End Property

        Private mProjectGuid As Guid

        Public Property ProjectGuid() As Guid
            Get
                Return mProjectGuid
            End Get
            Set(ByVal value As Guid)
                mProjectGuid = value
            End Set
        End Property

        Private _TreeLocked As Boolean = False 'A0093

        Public Property TreeLocked() As Boolean 'A0093
            Get
                Return _TreeLocked
            End Get
            Set(ByVal value As Boolean)
                _TreeLocked = value
            End Set
        End Property

        Private _BoardLocked As Boolean = False 'A0093

        Public Property BoardLocked() As Boolean 'A0093
            Get
                Return _BoardLocked
            End Get
            Set(ByVal value As Boolean)
                _BoardLocked = value
            End Set
        End Property

        Private _RecycleLocked As Boolean = False 'A0093

        Public Property RecycleLocked() As Boolean 'A0093
            Get
                Return _RecycleLocked
            End Get
            Set(ByVal value As Boolean)
                _RecycleLocked = value
            End Set
        End Property

        Private _UserGuid As Guid

        Public Property UserGuid() As Guid
            Get
                Return _UserGuid
            End Get
            Set(ByVal value As Guid)
                _UserGuid = value
            End Set
        End Property

    End Class

    Public Enum ClientType

        Regular = 0

        Owner = 1
    End Enum

    Public Enum MeetingState

        InActive = 0

        Active = 1

        Paused = 2

        OwnerDisconnected = 4

        Stopped = 8

        Booted = 16
    End Enum


    Public Enum ConnectErrorCode 'A0095

        None = 0

        WrongMeetingID = 1

        WrongPassword = 2

        WrongCredentials = 4

        SessionError = 8
    End Enum

    Public Enum StructuringMode

        Collaborative = 0

        SingleUser = 1
    End Enum

    Public Enum MeetingMode 'A0122
        Alternatives = 0
        Sources = 1
        Impacts = 2
        'Objectives = 0 ' both sides sources (likelihood)
        'Alternatives = 1 ' both sides alternatives
        'ProsCons = 2 ' objectives on the left, alternatives on the whiteboard
        'Consequences = 3 ' both sides objectives (impact)
        'SourcesContribution = 4 ' sources on the left, alternatives on the whiteboard
        'ConsequencesContribution = 5 ' objectives on the left, alternatives on the whiteboard
        'AltsSources = 6 ' objectives on the left, alternatives on the whiteboard
        'AltsConsequences = 7 ' objectives on the left, alternatives on the whiteboard
    End Enum

End Namespace