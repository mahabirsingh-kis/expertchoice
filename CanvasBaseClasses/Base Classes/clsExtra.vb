Namespace ExpertChoice.Data

    Public Enum ecExtraType
        Unspecified = -1
        Common = 1
        Project = 2
        TeamTime = 3
        SilverLight = 4     ' D0529
        Workgroup = 5       ' D2657
        User = 6            ' D6657
    End Enum

    Public Enum ecExtraProperty
        Unspecified = -1
        Version = 1
        ShowJudgments = 2
        ' D0507 ===
        ShowResultsNode = 3
        ShowResultsSortField = 4
        ShowResultsSortOrder = 5
        ' D0507 ==
        SensitivityAnalysisNode = 6     ' D0555
        ShowIntroductionPage = 7        ' D0734
        'AnonymousMode = 8               ' D0778 - D0806
        DatabaseID = 9                  ' D0557
        RefreshTimeout = 10             ' D0750
        Message = 11                    ' D0796
        WipeoutProjectsTimeout = 12     ' D0944
        ShowInstruction = 13            ' D1164
        TeamTimeSessionData = 14        ' D1251
        'TeamTimeSessionDateTime = 15    ' D1254 -D1275
        TeamTimeJudgment = 16           ' D1275
        TeamTimeStepData = 17           ' D1348
        ReviewAccountEnabled = 18       ' D1381
        TeamTimeSessionPasscode = 19    ' D1734
        RTEonOpen = 20                  ' D2602
        CheckMasterProjectsDate = 21               ' D2657
        DetailsMode = 22                ' D3087
        'EmailSubject = 23               ' D3580    ' -D3589
        'EmailBody = 24                  ' D3580    ' -D3589
        OldPMProjects = 25              ' D3684
        WebConfigSetting = 26           ' D3821
        AppSetting = 27                 ' D3821
        TeamTimeSessionAppID = 29       ' D4274
        LifetimeProjects = 30           ' D4564
        CheckExpiredSnapshotsDT = 31    ' D4700
        StructuringJsonData = 32        ' A1564
        IncreasingBudgetsJsonData = 33  ' A1601
        UserPswHashesJsonData = 34      ' D6657
        UserSessionTerminate = 35       ' D7356
    End Enum

    <Serializable()> Public Class clsExtra

        Private _ID As Integer
        Private _ExtraType As ecExtraType
        Private _ObjectID As Integer
        Private _ExtraProperty As ecExtraProperty
        Private _Value As Object
        'Private _Changed As Nullable(Of DateTime)   ' D2214 - D2259

        Public Property ID() As Integer
            Get
                Return _ID
            End Get
            Set(ByVal value As Integer)
                _ID = value
            End Set
        End Property

        Public Property ExtraType() As ecExtraType
            Get
                Return _ExtraType
            End Get
            Set(ByVal value As ecExtraType)
                _ExtraType = value
            End Set
        End Property

        Public Property ExtraProperty() As ecExtraProperty
            Get
                Return _ExtraProperty
            End Get
            Set(ByVal value As ecExtraProperty)
                _ExtraProperty = value
            End Set
        End Property

        Public Property ObjectID() As Integer
            Get
                Return _ObjectID
            End Get
            Set(ByVal value As Integer)
                _ObjectID = value
            End Set
        End Property

        Public Property Value() As Object
            Get
                Return _Value
            End Get
            Set(ByVal value As Object)
                _Value = value
            End Set
        End Property

        ' -D2259
        '' D2214 ===
        'Public Property LastChanged As Nullable(Of DateTime)
        '    Get
        '        Return _Changed
        '    End Get
        '    Set(value As Nullable(Of DateTime))
        '        _Changed = value
        '    End Set
        'End Property
        '' D2214 ==

        ' D0507 ===
        Public Shared Function Params2Extra(ByVal tObjectID As Integer, ByVal tExtraType As ecExtraType, ByVal tExtraProperty As ecExtraProperty, Optional ByVal tValue As Object = Nothing) As clsExtra
            Dim tExtra As New clsExtra
            tExtra.ObjectID = tObjectID
            tExtra.ExtraType = tExtraType
            tExtra.ExtraProperty = tExtraProperty
            If tValue IsNot Nothing Then tExtra.Value = tValue
            Return tExtra
        End Function
        ' D0507 ==

        Public Function Clone() As clsExtra
            Dim tNew As New clsExtra
            tNew.ID = Me.ID
            tNew.ExtraType = Me.ExtraType
            tNew.ExtraProperty = Me.ExtraProperty
            tNew.ObjectID = Me.ObjectID
            tNew.Value = Me.Value
            'tNew.LastChanged = Me.LastChanged   ' D2214 -D2259
            Return tNew
        End Function

        Public Sub New()
            _ID = -1
            _ExtraType = ecExtraType.Unspecified
            _ExtraProperty = ecExtraProperty.Unspecified
            _ObjectID = -1
            _Value = Nothing
            '_Changed = Nothing  ' D2214 -D2259
        End Sub

    End Class

End Namespace
