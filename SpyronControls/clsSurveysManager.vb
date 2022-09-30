Imports System.Xml 'L0052
Imports System.IO 'L0052
Imports SpyronControls.Spyron.Data

Namespace Spyron.Core

    <Serializable()> Public Enum SurveyStorageType
        sstDatabaseStream = 1
        sstFileStream = 2
        sstECCDatabaseStream_v1 = 3
        sstFileStream_v1 = 4
    End Enum

    <Serializable()> Public Enum SurveyType
        stWelcomeSurvey = 1
        stThankyouSurvey = 2
        stImpactWelcomeSurvey = 3
        stImpactThankyouSurvey = 4
    End Enum

    <Serializable()> Public Class clsSurveysManager
        Private fConnectionString As String
        Private fProviderType As DBProviderType
        Private fSurveyStorageType As SurveyStorageType = SurveyStorageType.sstDatabaseStream
        Private fActiveWorkgroupID As Integer = -1      ' D1127
        Private fActiveUserEmail As String = "-"


        Public Property ConnectionString() As String
            Get
                Return fConnectionString
            End Get
            Set(ByVal value As String)
                fConnectionString = value
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

        Public Property SurveyStorageType() As SurveyStorageType
            Get
                Return fSurveyStorageType
            End Get
            Set(ByVal value As SurveyStorageType)
                fSurveyStorageType = value
            End Set
        End Property

        ' D1127 ===
        Public Property ActiveWorkgroupID() As Integer
            Get
                Return fActiveWorkgroupID
            End Get
            Set(ByVal value As Integer)
                fActiveWorkgroupID = value
            End Set
        End Property
        ' D1127 ==

        Public Property ActiveUserEmail As String
            Get
                Return fActiveUserEmail
            End Get
            Set(value As String)
                fActiveUserEmail = value
            End Set
        End Property
        ''' <summary>
        ''' Load available, depends on ActiveWorkgroupID, array of clsSurveyInfo's
        ''' </summary>
        ''' <param name="fActiveWorkgroupID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadSurveyList(ByVal fActiveWorkgroupID As Integer) As ArrayList
            Dim AResult As New ArrayList
            Using DBConn As DbConnection = GetDBConnection(ProviderType, ConnectionString)  ' D2232
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                Dim dr As DbDataReader
                Dim SurveyInfo As clsSurveyInfo
                Try ' D0237
                    DBConn.Open()
                    oCommand.Connection = DBConn
                    oCommand.CommandText = "SELECT * FROM Surveys"
                    dr = DBExecuteReader(ProviderType, oCommand)
                    While (Not dr Is Nothing) And (dr.Read)
                        SurveyInfo = New clsSurveyInfo
                        SurveyInfo.ID = CInt(dr("ID").ToString)
                        ' D0236 ===
                        SurveyInfo.WorkgroupID = CInt(dr("WorkgroupID").ToString)
                        If Not TypeOf (dr("Title")) Is DBNull Then SurveyInfo.Title = dr("Title").ToString
                        ' D0380 ===
                        ''L0458
                        SurveyInfo.ConnectionString = ConnectionString
                        If Not TypeOf (dr("Comments")) Is DBNull Then SurveyInfo.Comments = dr("Comments").ToString
                        If Not TypeOf (dr("HideIndexNumbers")) Is DBNull Then SurveyInfo.HideIndexNumbers = dr("HideIndexNumbers")
                        If Not TypeOf (dr("GUID")) Is DBNull Then SurveyInfo.AGuid = New System.Guid(dr("GUID").ToString.ToUpper) ' D0249
                        If dr("OwnerID") IsNot Nothing Then
                            SurveyInfo.OwnerID = CInt(dr("OwnerID").ToString)
                        End If
                        'SurveyInfo.State = CInt(dr("State").ToString)
                        ' D0236 ==
                        If fActiveWorkgroupID = -1 Or fActiveWorkgroupID = SurveyInfo.WorkgroupID Then AResult.Add(SurveyInfo) ' D0236 + D1127
                    End While
                    dr.Close()
                    DBConn.Close()
                Catch ex As Exception   ' D0237
                End Try
            End Using

            Return AResult
        End Function

        ''' <summary>
        ''' Get SurveyInfo from provided SurveysList, based on Survey Info ID
        ''' </summary>
        ''' <param name="SurveysList"></param>
        ''' <param name="ID"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property GetSurveyInfo(ByVal SurveysList As ArrayList, ByVal ID As Integer) As clsSurveyInfo
            Get
                If Not SurveysList Is Nothing Then
                    For Each tSurvey As clsSurveyInfo In SurveysList
                        If tSurvey.ID = ID Then Return tSurvey
                    Next
                End If
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Get SurveyInfo from provided SurveysList, based on Survey GUID
        ''' </summary>
        ''' <param name="SurveysList"></param>
        ''' <param name="tGUID"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property GetSurveyInfo(ByVal SurveysList As ArrayList, ByVal tGUID As Guid) As clsSurveyInfo
            Get
                If Not SurveysList Is Nothing Then
                    For Each tSurvey As clsSurveyInfo In SurveysList
                        If tSurvey.AGuid = tGUID Then Return tSurvey
                    Next
                End If
                Return Nothing
            End Get
        End Property

        ' D0380 ===
        ''' <summary>
        ''' Get SurveyInfo from provided SurveysList, based on Survey GUID string
        ''' </summary>
        ''' <param name="SurveysList"></param>
        ''' <param name="sGUID"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property GetSurveyInfo(ByVal SurveysList As ArrayList, ByVal sGUID As String) As clsSurveyInfo
            Get
                If Not SurveysList Is Nothing Then
                    For Each tSurvey As clsSurveyInfo In SurveysList
                        If tSurvey.AGuid.ToString = sGUID Then Return tSurvey
                    Next
                End If
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property GetSurveyInfoByProjectID(ByVal ProjectID As Integer, ByVal ASurveyType As SurveyType, ByVal AUsersList As Dictionary(Of String, clsComparionUser)) As clsSurveyInfo
            Get
                Dim ASurveyInfo As New clsSurveyInfo()
                ASurveyInfo.ProjectID = ProjectID
                ASurveyInfo.StorageType = Core.SurveyStorageType.sstECCDatabaseStream_v1
                ASurveyInfo.SurveyType = ASurveyType
                ASurveyInfo.ConnectionString = ConnectionString
                ASurveyInfo.ProviderType = ProviderType
                ASurveyInfo.ComparionUsersList = AUsersList
                'Dim ASurvey As clsSurvey = ASurveyInfo.Survey(LoadedUserEmail)
                'If ASurvey IsNot Nothing AndAlso ASurvey.Pages.Count > 0 Then
                '    Return ASurveyInfo
                'Else
                '    Return Nothing
                'End If
                Return ASurveyInfo
            End Get
        End Property
        ' D0380 ==

        'L0043 ===
        Public Function GetSurveyStepsCountByGUID(ByVal ProjectID As Integer, ByVal ASurveyType As Integer, ByRef StepsCount As Integer, ByRef HasAltsSelect As Boolean) As Boolean   ' D6671
            Dim AUsersList As New Dictionary(Of String, clsComparionUser)
            Dim ASurveyInfo As clsSurveyInfo = GetSurveyInfoByProjectID(ProjectID, ASurveyType, AUsersList)
            If ASurveyInfo IsNot Nothing Then
                Dim ASurvey As clsSurvey = ASurveyInfo.Survey(ActiveUserEmail)
                If ASurvey IsNot Nothing Then
                    StepsCount = ASurvey.StepCount() 'L0442
                    ' D6671 ===
                    For p As Integer = 0 To ASurvey.Pages.Count - 1
                        With CType(ASurvey.Pages(p), clsSurveyPage).Questions
                            For i As Integer = 0 To .Count - 1
                                If CType(.Item(i), clsQuestion).Type = QuestionType.qtAlternativesSelect Then
                                    HasAltsSelect = True
                                    Exit For
                                End If
                            Next
                        End With
                        If HasAltsSelect Then Exit For
                    Next
                    ' D6671 ==
                    Return True
                End If
            End If
            Return False
        End Function
        'L0043 ==

    End Class

End Namespace