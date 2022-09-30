Imports System.Collections.Generic

Namespace Expertchoice.TeamTime

    <ServiceContract()> _
    Public Interface IKeypadPrivilleged

        <OperationContract()> _
        Function AddToken(ByVal MeetingID As String, ByVal Client As ClientType) As Integer

        <OperationContract()> _
        Sub DeleteToken(ByVal MeetingID As String, ByVal AuthToken As Integer)

        <OperationContract()> _
        Function GetTokens(ByVal MeetingID As String) As List(Of TokenInfo)

        <OperationContract()> _
        Sub ClearTokens(ByVal MeetingID As String)

        <OperationContract()> _
        Sub EndMeeting(ByVal MeetingID As String)

        <OperationContract()> _
        Function ServiceCheck() As Boolean

    End Interface


    <ServiceContract()> _
    Public Interface IKeypad

        <OperationContract()> _
        Function Register(ByVal MeetingID As String, ByVal AuthorizationToken As Integer, ByVal email As String) As Boolean

        <OperationContract()> _
        Function GetUsers() As KeypadData

        <OperationContract()> _
        Function MaxKeypadNum() As Integer

        <OperationContract()> _
        Function SubmitKeyPadData(ByVal Data As KeypadData) As Boolean

        <OperationContract()> _
        Function GetMeetingStatus() As MeetingStatus

        <OperationContract()> _
        Sub Disconnect()

    End Interface

#Region "HelperClasses and Types"

    Public Class UserConnection

        'could add new parameters indicating client-type here
        Public Sub New(ByVal Context As System.ServiceModel.OperationContext, ByVal MeetingID As String)
            mContext = Context
            mMeetingID = MeetingID
        End Sub

        Private mAuthToken As TokenInfo

        Public Property Token() As TokenInfo
            Get
                Return mAuthToken
            End Get
            Set(ByVal value As TokenInfo)
                mAuthToken = value
            End Set
        End Property

        Private mContext As System.ServiceModel.OperationContext
        Public ReadOnly Property Context() As System.ServiceModel.OperationContext
            Get
                Return mContext
            End Get
        End Property

        Public ReadOnly Property SessionID() As String
            Get
                Return mContext.SessionId
            End Get
        End Property

        Private mMeetingID As String
        Public ReadOnly Property MeetingID() As String
            Get
                Return mMeetingID
            End Get
        End Property

    End Class

    <DataContract()> _
    Public Class TokenInfo
        Public mAuthToken As Integer
        Public mClient As ClientType
        Public mMeetingID As String

        <DataMember()> _
        Public Property AuthToken() As Integer
            Get
                Return mAuthToken
            End Get
            Set(ByVal value As Integer)
                mAuthToken = value
            End Set
        End Property

        <DataMember()> _
        Public Property Client() As ClientType
            Get
                Return mClient
            End Get
            Set(ByVal value As ClientType)
                mClient = value
            End Set
        End Property

        <DataMember()> _
        Public Property MeetingID() As String
            Get
                Return mMeetingID
            End Get
            Set(ByVal value As String)
                mMeetingID = value
            End Set
        End Property

        'Public Sub New(ByVal MeetingID As String, ByVal Token As Integer, ByVal Client As ClientType)
        '    mToken = Token
        '    mClient = Client
        '    mMeetingID = MeetingID
        'End Sub

    End Class

    <DataContract()> _
    Public Class KeyPadInfo
        Private mKeypadNumber As Integer
        <DataMember()> _
        Public Property KeypadNumber() As Integer
            Get
                Return mKeypadNumber
            End Get
            Set(ByVal value As Integer)
                mKeypadNumber = value
            End Set
        End Property

        Private mUserEmail As String
        <DataMember()> _
        Public Property UserEmail() As String
            Get
                Return mUserEmail
            End Get
            Set(ByVal value As String)
                mUserEmail = value
            End Set
        End Property

        Private mUserName As String
        <DataMember()> _
        Public Property UserName() As String
            Get
                Return mUserName
            End Get
            Set(ByVal value As String)
                mUserName = value
            End Set
        End Property

        Private mValue As Char
        <DataMember()> _
        Public Property Value() As Char
            Get
                Return mValue
            End Get
            Set(ByVal Value As Char)
                mValue = Value
            End Set
        End Property
    End Class

    <DataContract()> _
    Public Class KeypadData
        Private mKeypads As New List(Of KeyPadInfo)
        <DataMember()> _
        Public Property Keypads() As List(Of KeyPadInfo)
            Get
                Return mKeypads
            End Get
            Set(ByVal value As List(Of KeyPadInfo))
                mKeypads = value
            End Set
        End Property
    End Class

    <DataContract()> _
    Public Enum MeetingStatus
        <EnumMember()> _
        Unknown = Nothing
        <EnumMember()> _
        PollingOn = 1
        <EnumMember()> _
        PollingOff = 2
        <EnumMember()> _
        Initiailize = 3
        <EnumMember()> _
        MeetingEnded = 4
    End Enum

    <DataContract()> _
    Public Enum ClientType
        <EnumMember()> _
        Regular = 1
        <EnumMember()> _
        Super = 2
    End Enum

#End Region

End Namespace
