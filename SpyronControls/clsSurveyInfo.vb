Imports SpyronControls.Spyron.Data
Imports SpyronControls.Spyron.Core

Namespace Spyron.Core
    ' D1039 ===
    Public Class clsSurveyInfoComparer
        Implements IComparer

        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
            Return String.Compare(CType(x, clsSurveyInfo).Title, CType(y, clsSurveyInfo).Title, True)
        End Function

    End Class
    ' D1039 ==

    <Serializable()> Public Class clsComparionUser
        Private fID As Integer = -1
        Private fUserName As String = ""
        Public Property ID As Integer
            Get
                Return fID
            End Get
            Set(value As Integer)
                fID = value
            End Set
        End Property
        Public Property UserName As String
            Get
                Return fUserName
            End Get
            Set(value As String)
                fUserName = value
            End Set
        End Property
    End Class

    ''' <summary>
    ''' Represent Survey information and main methods to operate with Survey object
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class clsSurveyInfo
        Private fID As Integer = -1
        Private fGUID As Guid = Guid.NewGuid()
        Private fWorkgroupID As Integer = -1 ' D0236
        Private fOwnerID As Integer = -1 'L0020

        Private fConnectionString As String = ""
        Private fProviderType As DBProviderType
        Private fStorageType As SurveyStorageType = SurveyStorageType.sstDatabaseStream     ' D0377

        Private fTitle As String = ""
        Private fComments As String = ""
        Private fHideIndexNumbers As Boolean = False

        Private fSurveyDataProvider As clsSurveyDataProvider = Nothing

        Private fProjectID As Integer = -1
        Private fSurveyType As SurveyType
        Private fComparionUsersList As New Dictionary(Of String, clsComparionUser)
        Private fDBVersion As New Version("1.2")
        Private fErrorMessage As String = ""

        Public Property ProjectID As Integer
            Get
                Return fProjectID
            End Get
            Set(value As Integer)
                fProjectID = value
            End Set
        End Property

        Public Property SurveyType As SurveyType
            Get
                Return fSurveyType
            End Get
            Set(value As SurveyType)
                fSurveyType = value
            End Set
        End Property

        Public Property ComparionUsersList As Dictionary(Of String, clsComparionUser)
            Get
                Return fComparionUsersList
            End Get
            Set(value As Dictionary(Of String, clsComparionUser))
                fComparionUsersList = value
            End Set
        End Property

        Public Property DBVersion As Version
            Get
                Return fDBVersion
            End Get
            Set(value As Version)
                fDBVersion = value
            End Set
        End Property

        Public Property ErrorMessage As String
            Get
                Return fErrorMessage
            End Get
            Set(value As String)
                fErrorMessage = value
            End Set
        End Property

        Public Property SurveyDataProvider(ByVal ParticipantEmail As String) As clsSurveyDataProvider
            Get
                If fSurveyDataProvider Is Nothing Then
                    fSurveyDataProvider = New clsSurveyDataProvider(Me)
                    If Not fSurveyDataProvider.OpenSurvey(ParticipantEmail) Then fSurveyDataProvider.Survey = Nothing ' D0381
                End If
                Return fSurveyDataProvider
            End Get
            Set(ByVal value As clsSurveyDataProvider)
                fSurveyDataProvider = value
            End Set
        End Property

        Public Property ID() As Integer
            Get
                Return fID
            End Get
            Set(ByVal value As Integer)
                fID = value
            End Set
        End Property

        Public Property AGuid() As Guid
            Get
                Return fGUID
            End Get
            Set(ByVal value As Guid)
                fGUID = value
            End Set
        End Property

        ' D0236 ===
        Public Property WorkgroupID() As Integer
            Get
                Return fWorkgroupID
            End Get
            Set(ByVal value As Integer)
                fWorkgroupID = value
            End Set
        End Property
        ' D0236 ==

        'L0020 ===
        Public Property OwnerID() As Integer
            Get
                Return fOwnerID
            End Get
            Set(ByVal value As Integer)
                fOwnerID = value
            End Set
        End Property

        Public Property ConnectionString() As String
            Get
                Return fConnectionString
            End Get
            Set(ByVal value As String)
                ' D0380 ===
                If fConnectionString <> value Then
                    'fDBName = ""
                    fConnectionString = value
                End If
                ' D0380 ==
            End Set
        End Property

        Public Property ProviderType() As DBProviderType
            Get
                Return fProviderType
            End Get
            Set(ByVal value As DBProviderType)
                fProviderType = value
            End Set
        End Property

        ' D0377 ===
        Public Property StorageType() As SurveyStorageType
            Get
                Return fStorageType
            End Get
            Set(ByVal value As SurveyStorageType)
                fStorageType = value
            End Set
        End Property
        ' D0377 ==
        'L0020 ==

        Public Property Title() As String
            Get
                Return fTitle
            End Get
            Set(ByVal value As String)
                fTitle = value
            End Set
        End Property

        Public Property Comments() As String
            Get
                Return fComments
            End Get
            Set(ByVal value As String)
                fComments = value
            End Set
        End Property

        Public Property HideIndexNumbers() As Boolean 
            Get
                Return fHideIndexNumbers
            End Get
            Set(ByVal value As Boolean)
                fHideIndexNumbers = value
            End Set
        End Property

        Public ReadOnly Property Survey(ByVal ParticipantEmail As String) As clsSurvey 'L0442
            Get
                Return SurveyDataProvider(ParticipantEmail).Survey 'L0442
            End Get
        End Property

        Public Function SaveSurvey(ByVal ForceSaveRespondentData As Boolean)  'L0441
            Dim SurveyEMail As String
            If ForceSaveRespondentData Then
                SurveyEMail = ""
            Else
                SurveyEMail = "-"
            End If
            Return SurveyDataProvider(SurveyEMail).SaveSurvey(ForceSaveRespondentData)  ' D0380 'L0441 'L0442
        End Function

        ' D0380 ===
        Public Sub InitNewSurvey(Optional ByVal sPageName As String = "Page 1", Optional ByVal sDefaultGroupname As String = "Default Group") 'L0061
            Dim ASurvey As New clsSurvey

            'Create Default Page
            Dim APage As New clsSurveyPage(ASurvey)
            APage.Title = sPageName
            ASurvey.Pages.Add(APage)

            'Create Default RespondentGroup
            'Dim ARespondentGroup As New clsRespondentGroup
            'ARespondentGroup.GroupName = sDefaultGroupname
            'ASurvey.RespondentGroups.Add(ARespondentGroup)

            SurveyDataProvider("-").Survey = ASurvey 'L0442
        End Sub
        ' D0380 ==

    End Class

End Namespace