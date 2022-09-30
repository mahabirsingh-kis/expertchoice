Namespace ExpertChoice.Data

    <Serializable()> Public Class clsComparionCoreOptions

        Public ProjectLoadOnDemand As Boolean = True
        Public ProjectForceAllowedAlts As Boolean = False
        Public ProjectLockTimeout As Integer = _DEF_LOCK_TIMEOUT    ' D0851
        Public ProjectUseDataMapping As Boolean = False             ' D4465

        Public ProjectArchiveAccessTimeoutDays As Integer = 365     ' D1568 // 1 year
        Public DoAutoArchiveProjects As Boolean = False             ' D2933
        Public RestoreAutoArchivedProjects As Boolean = False       ' D2933

        Public CheckEULA As Boolean = True
        Public CheckSilverLight As Boolean = False  ' D0529 + D0563

        Public AutoUpdateOutdatedDecisions As Boolean = True   ' D0622

        Public CheckLicense As Boolean = True
        Public _Backdoor As String = "" ' D0466
        Public SynchronousMaxUsers As Integer = 200  ' D1966 + D1972 + D2622
        ' D0315 ==

        Public CanvasMasterDBName As String = ""
        Public CanvasProjectsDBName As String = ""
        'Public SpyronMasterDBName As String = ""       ' -D6423
        'Public SpyronProjectsDBName As String = ""     ' -D6423

        Public DefParamSet As String = ""           ' D2256
        Public DefParamSetRisk As String = ""       ' D2256

        Public ShowAppNavigation As Boolean = True  ' D0468

        Private _SingleModeProjectPasscode As String = "" ' D0304 + D0461 + D0465
        Public isLoggedInWithMeetingID As Boolean = False    ' D0422 + D0468
        Public isLoggedInWithPasscode As Boolean = False     ' D1355
        Public ignoreOffline As Boolean = False             ' D6619

        Private _OnlyTTSession As Boolean = False   ' D0655

        Public RestoreLastVisitedProject As Boolean = False     ' D0491 + D2051

        Private _SLShellOnLogin As Boolean = False  ' D0729

        Private _RoleGroupID As Integer = -1    ' D1936
        Private _WkgRoleGroupID As Integer = -1 ' D2287

        Public UseTinyURL As Boolean = True     ' D0896

        Public Property JoinAsPMonAttachProject As Boolean = False  ' D4332

        Public Property RiskionRiskRewardMode As Boolean = False    ' D6813

        ' D3939 ===
        Private _EvalSiteURL As String = ""     ' D3308
        Public Property EvalSiteURL As String
            Get
                Return _EvalSiteURL
            End Get
            Set(value As String)
                _EvalSiteURL = value
            End Set
        End Property
        ' D3939 ==

        Public EvalURLPostfix As String = ""    ' D6364

        Public Property EvalURL4TeamTime As Boolean = False  ' D3494 + D3561 + D6016
        Public Property EvalURL4TeamTime_Riskion As Boolean = False  ' D6020
        Public Property EvalURL4Anytime As Boolean = True    ' D6016
        Public Property EvalURL4Anytime_Riskion As Boolean = False  ' D6020
        Public Property EvalSiteOnly As Boolean = False     ' D6359

        Public SessionID As String = ""         ' D2289

        Public UseUserTemplates As Boolean = False  ' D2485

        Public KeypadsAvailable As Boolean = True   ' D6429

        Public ReadOnly Property isSingleModeEvaluation() As Boolean
            Get
                Return _SingleModeProjectPasscode <> ""
            End Get
        End Property

        Public Property SingleModeProjectPasscode() As String
            Get
                Return _SingleModeProjectPasscode
            End Get
            Set(ByVal value As String)
                _SingleModeProjectPasscode = value
            End Set
        End Property

        ' D0466 ===
        Public Property BackDoor() As String
            Get
                Return _Backdoor
            End Get
            Set(ByVal value As String)
                If _Backdoor <> value Then _Backdoor = value.Trim.ToLower
            End Set
        End Property

        Public ReadOnly Property isActiveBackdoor() As Boolean
            Get
                Return _Backdoor <> ""
            End Get
        End Property
        ' D0466 ==

        ' D0655 ===
        Public Property OnlyTeamTimeEvaluation() As Boolean
            Get
                Return _OnlyTTSession
            End Get
            Set(ByVal value As Boolean)
                _OnlyTTSession = value
            End Set
        End Property
        ' D0655 ==

        '' D0729 ===
        'Public Property ShowSilverlightShellOnLogon() As Boolean
        '    Get
        '        Return _SLShellOnLogin
        '    End Get
        '    Set(ByVal value As Boolean)
        '        _SLShellOnLogin = value
        '    End Set
        'End Property
        '' D0729 ==

        ' D1936 ===
        Public Property UserRoleGroupID As Integer
            Get
                Return _RoleGroupID
            End Get
            Set(value As Integer)
                _RoleGroupID = value
            End Set
        End Property
        ' D1936 ==

        ' D2287 ===
        Public Property WorkgroupRoleGroupID As Integer
            Get
                Return _WkgRoleGroupID
            End Get
            Set(value As Integer)
                _WkgRoleGroupID = value
            End Set
        End Property
        ' D2287 ==

    End Class

End Namespace
