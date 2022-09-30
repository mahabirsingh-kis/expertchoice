Namespace ExpertChoice.Data

    ' D0041 ===
    ''' <summary>
    ''' Constants for ExtraInfo logging, could be used for grouping actions
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum dbObjectType
        'einfSession = 1
        einfUser = 10
        einfProject = 20
        einfProjectReport = 21  ' D2236
        einfSurvey = 30
        einfRoleAction = 50
        einfRoleGroup = 51
        einfUserWorkgroup = 52
        einfWorkgroup = 53
        einfWorkspace = 54
        einfUserTemplate = 55   ' D0795
        einfWorkgroupParameters = 56    ' D2212
        einfFile = 60       ' D0496
        einfDatabase = 61   ' D0496
        einfSnapshot = 62   ' D3509
        einfPage = 70       ' D3183
        einfWebService = 80 ' D0348
        einfAppSetting = 95     ' D3821
        einfConfigSetting = 96  ' D3821
        einfMessage = 97    ' D0184
        einfRTE = 98        ' D0184
        einfOther = 99
    End Enum
    ' D0041 ==

    Public Enum dbActionType
        actLogon = 1
        actLogout = 2
        ' D0558 ==
        actStartMeeting = 3
        actJoinMeeting = 4
        actTokenizedURLLogon = 5
        actCredentialsLogon = 6
        actAcceptEULA = 7   ' D4306
        actSSOLogin = 8     ' D6532
        ' D0558 ==

        actCreate = 10
        actModify = 11
        actRestore = 12 ' D3511
        actOpen = 15    ' D2236
        actClose = 16   ' D2236
        actSelect = 18
        actDelete = 19

        actMakeJudgment = 20
        actDatagridUpload = 22
        actReportStart = 23 ' D2236
        actReportEnd = 24   ' D2236
        actLock = 25    ' D0499
        actUnLock = 26  ' D0499
        actDownload = 29

        actRASolveModel = 70   ' D3894

        actExtractArchive = 80
        actCreateArchive = 81
        actSendEmail = 85
        actEditLicense = 86
        actRedirect = 88
        'actCreateReport = 89    ' pls don't use it: use ReportStart, ReportEnd

        actApplyPatch = 90
        actUpgrade = 91

        actLicenseMessage = 94  ' D2685

        actShellError = 95  ' D2169
        actServiceRTE = 96  ' D1631
        actShowMessage = 97
        actSendRTE = 98
        actShowRTE = 99
    End Enum

    <Serializable()> Public Class clsExtraLoad

        Private _LoadedDT As Nullable(Of DateTime)
        'Private _CheckedDT As Nullable(Of DateTime)
        Private _ExtraType As dbObjectType

        Public Sub New()
            _ExtraType = dbObjectType.einfOther
        End Sub

        Public Property LoadedDateTime() As Nullable(Of DateTime)
            Get
                Return _LoadedDT
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _LoadedDT = value
            End Set
        End Property

        'Public Property CheckedDateTime() As Nullable(Of DateTime)
        '    Get
        '        Return _CheckedDT
        '    End Get
        '    Set(ByVal value As Nullable(Of DateTime))
        '        _CheckedDT = value
        '    End Set
        'End Property

        Public Property ExtraType() As dbObjectType
            Get
                Return _ExtraType
            End Get
            Set(ByVal value As dbObjectType)
                _ExtraType = value
            End Set
        End Property

    End Class

End Namespace
