Imports GenericDBAccess.ECGenericDatabaseAccess
Imports ECCore
Imports ECCore.ECTypes
Imports Canvas
Imports Canvas.PipeParameters
Imports ExpertChoice.Service
Imports ExpertChoice.Database

Namespace ExpertChoice.Data

#Region "Types and Enumerations"

    ' D0300 ===
    'Public Enum ecProjectMode
    '    pmAnyAction = 0
    '    pmStructuring = 1
    '    pmDataInput = 2
    '    pmAnalysis = 3
    'End Enum

    ' D3433 ===
    Public Enum ProjectExtraProperty    ' Must be like a bit mask
        epNone = 0
        epInfodocs = 2
        epAHPTables = 4
        epAntigua = 8
        epSurvey = 16
        epResourceAligner = 32
        epCompleteHierarchy = 64    ' D3439
    End Enum
    ' D3433 ==

    Public Enum ecProjectStatus
        psActive = 0
        psArchived = 1
        psTemplate = 2
        psMasterProject = 3 ' D2479
    End Enum

    ' -D0748
    'Public Enum ecProjectParticipating
    '    ppOffline = 0
    '    ppAccessCodeEnabled = 1
    '    ppOnwerCanAdd = 2
    'End Enum
    ' D0300 ==

    ' D0209 ===
    ''' <summary>
    ''' Enum type for support controls with Synchronous
    ''' </summary>
    ''' <remarks>When Synchronous is active, it should be not equal ssNoSynchronous</remarks>
    Public Enum ECTeamTimeStatus
        tsNoTeamTimeSession = 0
        tsTeamTimeSessionUser = 1
        tsTeamTimeSessionOwner = 2
    End Enum
    ' D0209 ==

    ' D4575 ===
    <Serializable> Public Structure ProjectWordingTemplate
        Public Name As String
        Public Value As String
        Public InUse As Boolean
    End Structure
    ' D4575 ==

#End Region

    ''' <summary>
    ''' Class for processing Project (Decision)
    ''' </summary>
    ''' <remarks>Dependencies: clsEvaluation</remarks>
    <Serializable()> Public Class clsProject

        Public Const StorageType As ECModelStorageType = ECModelStorageType.mstCanvasStreamDatabase   ' D0466
        Public Const PipeStorageType As PipeStorageType = PipeStorageType.pstStreamsDatabase          ' D0466

        ' D0300 ===
        Public Const mask_ProjectStatus As Integer = &H38         ' 0000 0000 00xx x000   56
        Public Const bits_ProjectStatus As Integer = 3
        ' D0748 ===
        Public Const mask_AccessCode As Integer = &HC0            ' 0000 0000 0x00 0000   128
        Public Const bits_AccessCode As Integer = 6
        ' D0748 ==
        Public Const mask_isTeamTime As Integer = &H200           ' 0000 00x0 0000 0000   512
        Public Const bits_isTeamTime As Integer = 9
        ' D0748 ===
        Public Const mask_OnlineStatus As Integer = &H400         ' 0000 0x00 0000 0000   1024
        Public Const bits_OnlineStatus As Integer = 10
        ' D0748 ==
        ' D0789 ===
        Public Const mask_Deleted As Integer = &H800              ' 0000 x000 0000 0000   2048
        Public Const bits_Deleted As Integer = 11
        ' D0789 ==
        Public Const mask_EncodingVersion As Integer = &HE000     ' xxx0 0000 0000 0000   57344 ' D0789
        Public Const bits_EncodingVersion As Integer = 13

        Public Const EncodingVersion As Integer = 2     ' D0748
        ' D0300 ==

        ' D3571 ===
        Delegate Sub onProjectEvent(tProject As clsProject, sMessage As String, isCriticalChange As Boolean, sComment As String)    ' D3572 + D3731
        Delegate Sub onProjectDateTimeEvent(tProject As clsProject)     ' D4535
        Public Property onProjectSaving As onProjectEvent = Nothing
        Public Property onProjectSaved As onProjectEvent = Nothing
        Public Property onProjectUpdateLastModify As onProjectDateTimeEvent = Nothing   ' D4535
        ' D3571 ==

        Public Property UseDataMapping As Boolean = False   ' D4465

        ' ProjectID
        Private _ID As Integer
        ' GUID for project (based on PipeParameters.ProjectGUID)
        Private _GUID As Guid           ' D0891
        ' UserID for Owner store
        Private _OwnerID As Integer     ' D0086
        ' Passcode
        Private _PasscodeImpact As String       ' D1709
        Private _PasscodeLikelihood As String   ' D1709
        ' ID of project, which is replaced this one
        Private _ReplacedID As Integer ' D0893
        ' Project Full name
        Private _ProjectName As String

        ' D0329 ===
        Private _DBConnectionString As String
        Private _DBProviderType As DBProviderType
        ' D0329 ==

        ' Status or current project
        Private _ProjectStatusLikelihood As Integer     ' D0045 + D0063 + D1944
        ' Status or current project
        Private _ProjectStatusImpact As Integer     ' D1944
        ' Comment for project
        Private _Comment As String  ' D0010
        ' ID for owner Workgroup
        Private _WorkgroupID As Integer     ' D0045
        ' Reference to ProjectManager
        Private _ProjectManager As clsProjectManager  ' D0045 + D0053 + D0457

        Private _IsRisk As Boolean = False  ' D2255
        ''' <summary>
        ''' Reference to the PipeParameters (could be loaded independent when Project manager isn't loaded yet)
        ''' </summary>
        ''' <remarks>Auto-linked to the loaded ProjectManager when it's available</remarks>
        Private _PipeParameters As clsPipeParamaters        ' D0174
        Private _isDataChecked As Boolean   ' D0063
        Private _HasData As Boolean         ' D0063
        Private _DBversion As ECCanvasDatabaseVersion   ' D0149
        Private _DBUpdated As Boolean       ' D0622
        Private _isLoadOnDemand As Boolean  ' D0183
        Private _isForceAllowedAlts As Boolean  ' D0245
        Private _UserEmail As String        ' D0183

        Private _MeetingIDLikelihood As Long    ' D0420 + D1709
        Private _MeetingIDImpact As Long        ' D1709
        'Private _MeetingOwnerID As Integer  ' D0487
        Private _MeetingOwner As clsApplicationUser ' D0487

        'Private _UserEvaluationCache As List(Of clsUserEvaluationData)   ' D0305 + D0308 + D0464

        Private _LockInfo As clsProjectLockInfo     ' D0483
        Private _Created As Nullable(Of DateTime)   ' D0494
        Private _LastModify As Nullable(Of DateTime)    ' D0494
        Private _LastVisited As Nullable(Of DateTime)   ' D0494

        Private _LastHierarchyID As Integer = -1        ' D1411

        Private _WordingTpls As Dictionary(Of String, ProjectWordingTemplate) = Nothing      ' D4413 + D4575

        ''' <summary>
        ''' Project ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ID() As Integer
            Get
                Return _ID
            End Get
            Set(ByVal value As Integer)
                _ID = value
            End Set
        End Property

        ' D0891 ===
        Public Property ProjectGUID() As Guid
            Get
                Return _GUID
            End Get
            Set(ByVal value As Guid)
                _GUID = value
            End Set
        End Property
        ' D0891 ==

        ' D0086 ===
        Public Property OwnerID() As Integer
            Get
                Return _OwnerID
            End Get
            Set(ByVal value As Integer)
                _OwnerID = value
            End Set
        End Property
        ' D0086 ==

        ' D0009 ===
        ''' <summary>
        ''' Project name (full)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProjectName() As String
            Get
                Return _ProjectName
            End Get
            Set(ByVal value As String)
                _ProjectName = SubString(value.TrimEnd, 250)    ' D0916 + D0990
            End Set
        End Property

        ' D1944 ===
        Public ReadOnly Property isImpact As Boolean
            Get
                If Not IsRisk Then Return False
                If IsProjectLoaded AndAlso _ProjectManager IsNot Nothing AndAlso ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood Then Return False
                Return True
            End Get
        End Property
        ' D1944 ==

        ' D1709 + D1944 ===
        ''' <summary>
        ''' Project Passcode (short name)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Best solution: each project must be have unique passcode.</remarks>
        Public Property Passcode(fIsImpact As Boolean) As String
            Get
                'If IsProjectLoaded AndAlso _ProjectManager IsNot Nothing AndAlso ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood Then Return PasscodeLikelihood ' D1929
                If IsRisk AndAlso IsProjectLoaded AndAlso _ProjectManager IsNot Nothing AndAlso fIsImpact Then Return PasscodeImpact ' D1929 + D2017 + D2898 + D7374
                Return PasscodeLikelihood       ' D1929 + D2017
            End Get
            Set(value As String)
                If fIsImpact AndAlso IsRisk Then PasscodeImpact = value Else PasscodeLikelihood = value ' D2898
            End Set
        End Property
        ' D1944 ==

        Public Property Passcode() As String    ' D1944
            Get
                Return Passcode(isImpact)   ' D1944
            End Get
            Set(value As String)
                Passcode(isImpact) = value  ' D1944
            End Set
        End Property

        Public Property PasscodeLikelihood As String
            Get
                Return _PasscodeLikelihood
            End Get
            Set(value As String)
                _PasscodeLikelihood = value.ToLower ' D2674
            End Set
        End Property

        Public Property PasscodeImpact As String
            Get
                If Not String.IsNullOrEmpty(_PasscodeImpact) Then Return _PasscodeImpact Else Return _PasscodeLikelihood
            End Get
            Set(value As String)
                _PasscodeImpact = value.ToLower ' D2674
            End Set
        End Property
        ' D1709 ==

        ' D0893 ===
        Public Property ReplacedID() As Integer
            Get
                Return _ReplacedID
            End Get
            Set(ByVal value As Integer)
                _ReplacedID = value
            End Set
        End Property
        ' D0893 ==

        ' D0329 ===
        Public Property ConnectionString() As String
            Get
                Return _DBConnectionString
            End Get
            Set(ByVal value As String)
                If _DBConnectionString <> value Then
                    _DBConnectionString = value
                End If
            End Set
        End Property

        Public Property ProviderType() As DBProviderType
            Get
                Return _DBProviderType
            End Get
            Set(ByVal value As DBProviderType)
                _DBProviderType = value
            End Set
        End Property
        ' D0329 ==

        ' D0300 ===
        Private Function GetValueByMask(ByVal tStatusValue As Integer, ByVal tMask As Integer, ByVal tShiftBits As Byte) As Integer
            Return (tStatusValue And tMask) >> tShiftBits
        End Function

        Private Function SetValueByMask(ByVal tStatusValue As Integer, ByVal tNewValue As Integer, ByVal tMask As Integer, ByVal tShiftBits As Byte) As Integer
            Return (tStatusValue And (Not tMask)) Or (tNewValue << tShiftBits)
        End Function

        ' D2063 ===
        Public Function TeamTimeCheckStatus() As Boolean
            Dim fChanged As Boolean = False
            Dim HID As Integer = _LastHierarchyID
            If IsProjectLoaded Then HID = ProjectManager.ActiveHierarchy
            If isTeamTimeImpact AndAlso HID <> ECHierarchyID.hidImpact Then
                If IsProjectLoaded Then ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact Else _LastHierarchyID = ECHierarchyID.hidImpact
                fChanged = True
            End If
            If isTeamTimeLikelihood AndAlso HID <> ECHierarchyID.hidLikelihood Then
                If IsProjectLoaded Then ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood Else _LastHierarchyID = ECHierarchyID.hidLikelihood
                fChanged = True
            End If
            Return fChanged
        End Function
        ' D2063 ==

        ' D1944 ===
        Public Property isTeamTimeImpact As Boolean
            Get
                Return CType(GetValueByMask(StatusDataImpact, mask_isTeamTime, bits_isTeamTime), Boolean)    ' D1944
            End Get
            Set(ByVal value As Boolean)
                StatusDataImpact = SetValueByMask(StatusDataImpact, CInt(IIf(value, 1, 0)), mask_isTeamTime, bits_isTeamTime) ' D0311 + D1944
                StatusDataLikelihood = SetValueByMask(StatusDataLikelihood, 0, mask_isTeamTime, bits_isTeamTime) ' D1953
                TeamTimeCheckStatus()   ' D2062
            End Set
        End Property

        Public Property isTeamTimeLikelihood As Boolean
            Get
                Return CType(GetValueByMask(StatusDataLikelihood, mask_isTeamTime, bits_isTeamTime), Boolean)    ' D1944
            End Get
            Set(ByVal value As Boolean)
                StatusDataLikelihood = SetValueByMask(StatusDataLikelihood, CInt(IIf(value, 1, 0)), mask_isTeamTime, bits_isTeamTime) ' D0311 + D1944
                StatusDataImpact = SetValueByMask(StatusDataImpact, 0, mask_isTeamTime, bits_isTeamTime) ' D1953
                TeamTimeCheckStatus()   ' D2062
            End Set
        End Property
        ' D1944 ==

        Public Property isTeamTime() As Boolean
            Get
                If isImpact Then Return isTeamTimeImpact Else Return isTeamTimeLikelihood ' D1944
            End Get
            Set(ByVal value As Boolean)
                If isImpact Then isTeamTimeImpact = value Else isTeamTimeLikelihood = value ' D1944
            End Set
        End Property

        Public Property ProjectStatus() As ecProjectStatus
            Get
                Return CType(GetValueByMask(StatusDataLikelihood, mask_ProjectStatus, bits_ProjectStatus), ecProjectStatus) ' D1944
            End Get
            Set(ByVal value As ecProjectStatus)
                StatusDataLikelihood = SetValueByMask(StatusDataLikelihood, CInt(value), mask_ProjectStatus, bits_ProjectStatus)    ' D1944
                StatusDataImpact = SetValueByMask(StatusDataImpact, CInt(value), mask_ProjectStatus, bits_ProjectStatus)    ' D1944
            End Set
        End Property

        ' D0748 ===
        Public Property isPublic() As Boolean
            Get
                Return GetValueByMask(StatusDataLikelihood, mask_AccessCode, bits_AccessCode) = 1   ' D1944
            End Get
            Set(ByVal value As Boolean)
                StatusDataLikelihood = SetValueByMask(StatusDataLikelihood, CInt(IIf(value, 1, 0)), mask_AccessCode, bits_AccessCode)   ' D1944
                StatusDataImpact = SetValueByMask(StatusDataImpact, CInt(IIf(value, 1, 0)), mask_AccessCode, bits_AccessCode)   ' D1944
            End Set
        End Property

        Public Property isOnline() As Boolean
            Get
                Return GetValueByMask(StatusDataLikelihood, mask_OnlineStatus, bits_OnlineStatus) = 1   ' D1944
            End Get
            Set(ByVal value As Boolean)
                StatusDataLikelihood = SetValueByMask(StatusDataLikelihood, CInt(IIf(value, 1, 0)), mask_OnlineStatus, bits_OnlineStatus)   ' D1944
                StatusDataImpact = SetValueByMask(StatusDataImpact, CInt(IIf(value, 1, 0)), mask_OnlineStatus, bits_OnlineStatus)   ' D1944
            End Set
        End Property
        ' D0748 ==

        ' D0789 ===
        Public Property isMarkedAsDeleted() As Boolean
            Get
                Return GetValueByMask(StatusDataLikelihood, mask_Deleted, bits_Deleted) = 1 ' D1944
            End Get
            Set(ByVal value As Boolean)
                StatusDataLikelihood = SetValueByMask(StatusDataLikelihood, CInt(IIf(value, 1, 0)), mask_Deleted, bits_Deleted) ' D1944
                StatusDataImpact = SetValueByMask(StatusDataImpact, CInt(IIf(value, 1, 0)), mask_Deleted, bits_Deleted) ' D1944
            End Set
        End Property
        ' D0789 + D0300 ==

        Public ReadOnly Property HasUsersData() As Boolean
            Get
                If Not _isDataChecked Then  ' D0466
                    ' D0069 ===
                    Try
                        DebugInfo("Check DataExistsInProject()...")
                        _HasData = MiscFuncs.DataExistsInProject(StorageType, ID, ConnectionString, ProviderType) 'C0274 + D0369 + D0376 + D0466
                    Catch ex As Exception
                        DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                        _HasData = False
                    End Try
                    _isDataChecked = True
                    ' D0069 ==
                End If
                Return _HasData
            End Get
        End Property


        ' D1944 ===
        Private Function ParseStatusData(value As Integer) As Integer
            Dim _ProjectStatus As Integer = 0
            If value < 0 Then value = Math.Abs(value)
            ' D0300 ===
            Select Case GetValueByMask(value, mask_EncodingVersion, bits_EncodingVersion)   ' D0748
                Case 0  ' D0748
                    ' D0063 ===
                    Const mask_v0_StatusPublic As Integer = 1
                    Const mask_v0_StatusArchived As Integer = 2
                    Const mask_v0_StatusTemplate As Integer = 4 ' D0206
                    Const mask_v0_StatusMaster As Integer = 5   ' D2479
                    Const mask_v0_StatusDisabled As Integer = 128
                    Const mask_v0_StatusSynchronous As Integer = 64   ' D0192
                    ' D0063 ==

                    'Dim pm As ecProjectMode = ecProjectMode.pmAnyAction    ' -D0789
                    Dim ps As ecProjectStatus = ecProjectStatus.psActive
                    'Dim pp As ecProjectParticipating = ecProjectParticipating.ppOnwerCanAdd ' -D0748
                    Dim ac As Boolean = False   ' D0748
                    Dim ol As Boolean = False   ' D0748
                    Dim sy As Boolean = False

                    If (value And mask_v0_StatusArchived) <> 0 Then ps = ecProjectStatus.psArchived
                    If (value And mask_v0_StatusTemplate) <> 0 Then ps = ecProjectStatus.psTemplate
                    If (value And mask_v0_StatusMaster) = mask_v0_StatusMaster Then ps = ecProjectStatus.psMasterProject ' D2479
                    ac = (value And mask_v0_StatusPublic) <> 0      ' D0748
                    ol = (value And mask_v0_StatusDisabled) = 0     ' D0748
                    sy = (value And mask_v0_StatusSynchronous) <> 0

                    _ProjectStatus = SetValueByMask(_ProjectStatus, ps, mask_ProjectStatus, bits_ProjectStatus)
                    _ProjectStatus = SetValueByMask(_ProjectStatus, CInt(IIf(ac, 1, 0)), mask_AccessCode, bits_AccessCode)      ' D0748
                    _ProjectStatus = SetValueByMask(_ProjectStatus, CInt(IIf(ol, 1, 0)), mask_OnlineStatus, bits_OnlineStatus)  ' D0748
                    _ProjectStatus = SetValueByMask(_ProjectStatus, CInt(IIf(sy, 1, 0)), mask_isTeamTime, bits_isTeamTime)    ' D0311
                    _ProjectStatus = SetValueByMask(_ProjectStatus, EncodingVersion, mask_EncodingVersion, bits_EncodingVersion)

                    ' D0748 ===
                Case 1
                    Const mask_v1_Participating As Integer = 448
                    Const bits_v1_Participating As Integer = 6
                    Dim pp As Integer = GetValueByMask(value, mask_v1_Participating, bits_v1_Participating)
                    Dim ac As Boolean = (pp = 1)
                    Dim ol As Boolean = (pp <> 0)
                    _ProjectStatus = SetValueByMask(value, 0, mask_v1_Participating, bits_v1_Participating)
                    _ProjectStatus = SetValueByMask(_ProjectStatus, CInt(IIf(ac, 1, 0)), mask_AccessCode, bits_AccessCode)
                    _ProjectStatus = SetValueByMask(_ProjectStatus, CInt(IIf(ol, 1, 0)), mask_OnlineStatus, bits_OnlineStatus)
                    _ProjectStatus = SetValueByMask(_ProjectStatus, EncodingVersion, mask_EncodingVersion, bits_EncodingVersion)
                    ' D0748 ==

                Case Else
                    _ProjectStatus = value
            End Select
            ' D0300 ==
            Return _ProjectStatus
        End Function

        Private Property StatusData(fIsImpact As Boolean) As Integer    ' D1944
            Get
                If fIsImpact Then Return StatusDataImpact Else Return StatusDataLikelihood ' D1944
            End Get
            Set(ByVal value As Integer)
                If fIsImpact Then StatusDataImpact = value Else StatusDataLikelihood = value ' D1944
            End Set
        End Property

        ' D1944 ===
        Public Property StatusDataLikelihood() As Integer   ' D1944
            Get
                Return _ProjectStatusLikelihood
            End Get
            Set(ByVal value As Integer)
                _ProjectStatusLikelihood = ParseStatusData(value)   ' D1944
                TeamTimeCheckStatus()   ' D2062
            End Set
        End Property

        Public Property StatusDataImpact() As Integer
            Get
                Return _ProjectStatusImpact
            End Get
            Set(ByVal value As Integer)
                _ProjectStatusImpact = ParseStatusData(value)
                TeamTimeCheckStatus()   ' D2062
            End Set
        End Property
        ' D0062 + D1944 ==

        ' D0097 ===
        ReadOnly Property IsProjectEmpty() As Boolean
            Get
                Dim fIsEmpty As Boolean = True
                If ProjectManager.GetAllHierarchies.Count > 0 Then
                    If Not HierarchyObjectives Is Nothing Then
                        If HierarchyObjectives.Nodes.Count > 0 Then fIsEmpty = False
                    End If
                    ' D0121 ===
                    If Not fIsEmpty And Not HierarchyAlternatives Is Nothing Then
                        If HierarchyAlternatives.Nodes.Count > 0 Then fIsEmpty = False
                    End If
                    ' D0121 ==
                End If
                Return fIsEmpty
            End Get
        End Property
        ' D0097 ==

        ' D0282 ===
        Public ReadOnly Property IsProjectLoaded() As Boolean
            Get
                Return Not _ProjectManager Is Nothing
            End Get
        End Property
        ' D0282 ==

        ' D0134 ===
        ReadOnly Property CanShowGroupResults(Optional ByVal WRTNode As clsNode = Nothing) As Boolean 'C0565
            Get
                Dim PrjAnalyzer As New clsJudgmentsAnalyzer(PipeParameters.SynthesisMode, ProjectManager)
                Return PrjAnalyzer.CanShowGroupResults(WRTNode) 'C0565
            End Get
        End Property

        ' D0400 ===
        ReadOnly Property CanShowGlobalIndividualResults(ByVal ProjectUserID As Integer, Optional ByVal WRTNode As clsNode = Nothing) As Boolean 'C0565
            Get
                Dim PrjAnalyzer As New clsJudgmentsAnalyzer(PipeParameters.SynthesisMode, ProjectManager)
                Return PrjAnalyzer.CanShowIndividualResults(ProjectUserID, WRTNode) 'C0565
            End Get
        End Property
        ' D0400 ==

        ReadOnly Property CanShowGlobalIndividualResults(ByVal UserEmail As String, Optional ByVal WRTNode As clsNode = Nothing) As Boolean 'C0565
            Get
                Dim PrjUser As clsUser = ProjectManager.GetUserByEMail(UserEmail)
                If PrjUser Is Nothing Then Return False Else Return CanShowGlobalIndividualResults(PrjUser.UserID, WRTNode) 'C0565
            End Get
        End Property
        ' D0134 ==

        Public Property WorkgroupID() As Integer
            Get
                Return _WorkgroupID
            End Get
            Set(ByVal value As Integer)
                _WorkgroupID = value
            End Set
        End Property
        ' D0045 ==

        Public Property Comment() As String
            Get
                Return _Comment
            End Get
            Set(ByVal value As String)
                _Comment = SubString(value.TrimEnd, 900)    ' D0916 + D1119
            End Set
        End Property

        ' D0420 + D1709===
        Public ReadOnly Property MeetingID(Optional ByVal fAllowEmpty As Boolean = False) As Long
            Get
                If isImpact Then Return MeetingIDImpact Else Return MeetingIDLikelihood ' D1944
            End Get
        End Property

        Public Property MeetingIDLikelihood(Optional ByVal fAllowEmpty As Boolean = False) As Long
            Get
                If _MeetingIDLikelihood < 0 AndAlso Not fAllowEmpty Then _MeetingIDLikelihood = clsMeetingID.ReNew
                Return _MeetingIDLikelihood
            End Get
            Set(ByVal value As Long)
                _MeetingIDLikelihood = value
            End Set
        End Property

        Public Property MeetingIDImpact(Optional ByVal fAllowEmpty As Boolean = False) As Long
            Get
                If _MeetingIDImpact < 0 AndAlso Not fAllowEmpty Then _MeetingIDImpact = clsMeetingID.ReNew
                Return _MeetingIDImpact
            End Get
            Set(ByVal value As Long)
                _MeetingIDImpact = value
            End Set
        End Property
        ' D0420 + D1709 ==

        Public Property MeetingOwner() As clsApplicationUser
            Get
                Return _MeetingOwner
            End Get
            Set(ByVal value As clsApplicationUser)
                _MeetingOwner = value
            End Set
        End Property

        Public ReadOnly Property MeetingOwnerID() As Integer
            Get
                If MeetingOwner Is Nothing Then Return -1 Else Return MeetingOwner.UserID
            End Get
        End Property

        Public ReadOnly Property MeetingStatus(ByVal tUser As clsApplicationUser) As ECTeamTimeStatus
            Get
                If Not isTeamTimeImpact AndAlso Not isTeamTimeLikelihood Then Return ECTeamTimeStatus.tsNoTeamTimeSession ' D1953
                If Not tUser Is Nothing AndAlso tUser.UserID = MeetingOwnerID Then Return ECTeamTimeStatus.tsTeamTimeSessionOwner Else Return ECTeamTimeStatus.tsTeamTimeSessionUser
            End Get
        End Property
        ' D0487 ==

        ' D2255 ===
        Public Property IsRisk As Boolean
            Get
                Return _IsRisk
            End Get
            ' D2646 ===
            Set(value As Boolean)
                If _IsRisk <> value Then
                    _IsRisk = value
                    If IsProjectLoaded Then ResetProject()
                End If
            End Set
            ' D2646 ==
        End Property
        ' D2255 ==

        ' D3438 ===
        Public ReadOnly Property isOpportunityModel As Boolean  ' D4978
            Get
                Return (ProjectTypeData And CanvasTypes.ProjectType.ptOpportunities) <> 0
            End Get
        End Property

        Public Property isRiskAssociatedModel As Boolean
            Get
                Return (ProjectTypeData And CanvasTypes.ProjectType.ptRiskAssociated) <> 0
            End Get
            Set(value As Boolean)
                If value Then ProjectTypeData = (ProjectTypeData And Not CanvasTypes.ProjectType.ptRiskAssociated) Or If(value, ProjectType.ptRiskAssociated, 0)    ' D4978
            End Set
        End Property

        ' D6798 ===
        Public ReadOnly Property isMixedModel As Boolean
            Get
                Return (ProjectTypeData And CanvasTypes.ProjectType.ptMixed) <> 0
            End Get
        End Property

        Public ReadOnly Property isMyRiskRewardModel As Boolean
            Get
                Return (ProjectTypeData And CanvasTypes.ProjectType.ptMyRiskReward) <> 0
            End Get
        End Property
        ' D6798 ==

        ' D4978 ===
        Public Property RiskionProjectType As ProjectType
            Get
                Return CType(ProjectTypeData And Not CanvasTypes.ProjectType.ptRiskAssociated, ProjectType)
            End Get
            Set(value As ProjectType)
                ProjectTypeData = value Or If(isRiskAssociatedModel, ProjectType.ptRiskAssociated, 0)
            End Set
        End Property

        Public Property ProjectTypeData As Integer
        ' D3438 + D4978 ==

        Public Property ProjectManager() As clsProjectManager     ' D0053
            Get
                If _ProjectManager Is Nothing Then
                    DebugInfo("Create Project Manager")
                    _ProjectManager = New clsProjectManager(isLoadOnDemand, isTeamTime, IsRisk) 'C0238 + D2255
                    _ProjectManager.UseDataMapping = UseDataMapping ' D4465
                    _ProjectManager.StorageManager.ProviderType = ProviderType  ' D0329
                    _ProjectManager.StorageManager.ProjectLocation = ConnectionString   'C0039 + D0327
                    _ProjectManager.StorageManager.ModelID = ID 'C0271
                    _ProjectManager.StorageManager.StorageType = StorageType 'C0271 + D0369 + D0376 + D0466
                    _ProjectManager.StorageManager.ReadDBVersion()
                    _ProjectManager.LoadProject(ConnectionString, ProviderType, StorageType, ID)      'C0271 + D0369 + D0376 + D0466
                    If IsRisk AndAlso isMyRiskRewardModel Then
                        'TODO: Init scenario there (AC)
                    End If
                    Dim _prjUser As clsUser = _ProjectManager.GetUserByEMail(_UserEmail)   ' D0183
                    ' -D6725 '' remove since it cause issues with user names on upload
                    '' D6588 ===
                    'If _prjUser Is Nothing AndAlso Not String.IsNullOrEmpty(_UserEmail) Then
                    '    _ProjectManager.AddUser(_UserEmail)
                    '    _prjUser = _ProjectManager.GetUserByEMail(_UserEmail)
                    'End If
                    '' D6588 ==
                    If Not _prjUser Is Nothing Then _ProjectManager.User = _prjUser ' D0183
                    DebugInfo("Project data was loaded")
                    ' D0010 ===
                    If _ProjectManager.Hierarchies.Count = 0 Then
                        _ProjectManager.AddHierarchy()
                        DebugInfo("Empty hierarchy was added")
                    End If
                    If _ProjectManager.AltsHierarchies.Count = 0 Then
                        _ProjectManager.AddAltsHierarchy()
                        DebugInfo("Empty alts hierarchy was added")
                    End If
                    ' D0010 ==
                    ' D0442 ===
                    Dim PSID As Integer = -1
                    If Not _PipeParameters Is Nothing Then
                        PSID = _PipeParameters.CurrentParameterSet.ID
                    End If
                    _PipeParameters = _ProjectManager.PipeParameters
                    If PSID >= 0 Then _PipeParameters.CurrentParameterSet = _PipeParameters.GetParameterSetByID(PSID)
                    ' D0442 ==
                    If ProjectGUID = Guid.Empty AndAlso _PipeParameters.ProjectGuid <> Guid.Empty Then ProjectGUID = _PipeParameters.ProjectGuid ' D0891
                    If _LastHierarchyID >= 0 Then _ProjectManager.ActiveHierarchy = _LastHierarchyID ' D1411
                    _ProjectManager.BaronSolverCallback = AddressOf RunBaronSolver  ' D4071
                    _ProjectManager.BaronSolverCallback2 = AddressOf RunBaronSolver2  ' D4071
                End If
                Return _ProjectManager
            End Get
            Set(ByVal value As clsProjectManager)
                _ProjectManager = value
            End Set
        End Property

        Public ReadOnly Property PipeParameters() As clsPipeParamaters
            Get
                ' D0174 ===
                If _PipeParameters Is Nothing Then
                    If _ProjectManager IsNot Nothing AndAlso IsProjectLoaded Then
                        _PipeParameters = _ProjectManager.PipeParameters
                    Else
                        DebugInfo("Create Pipe Parameters")
                        _PipeParameters = New clsPipeParamaters
                        If ConnectionString <> "" Then 'C0353
                            _PipeParameters.Read(PipeStorageType, ConnectionString, ProviderType, ID) 'C0353 + D0376
                            _PipeParameters.PipeMessages.Load(PipeStorageType, ConnectionString, ProviderType, ID)   ' D0327 + D0329 + D0376 'C0420
                            If ProjectGUID = Guid.Empty AndAlso _PipeParameters.ProjectGuid <> Guid.Empty Then ProjectGUID = _PipeParameters.ProjectGuid ' D0891
                        End If
                    End If
                End If
                Return _PipeParameters
                ' D0174 ==
            End Get
            'Set(ByVal value As clsPipeParamaters)
            '    _PipeParameters = value     ' D0174
            'End Set
        End Property

        ' D0053 ===
        Public ReadOnly Property Pipe() As List(Of clsAction)
            Get
                Return ProjectManager.Pipe
            End Get
        End Property
        ' D0053 ==

        ' D0010 ===
        Public ReadOnly Property HierarchyObjectives() As clsHierarchy
            Get
                If ProjectManager Is Nothing Then
                    Return Nothing
                Else
                    Return ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
                End If
            End Get
        End Property

        Public ReadOnly Property HierarchyAlternatives() As clsHierarchy
            Get
                If ProjectManager Is Nothing Then
                    Return Nothing
                Else
                    Return ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
                End If
            End Get
        End Property

        ' D0149 ===
        Public ReadOnly Property DBVersion() As ECCanvasDatabaseVersion
            Get
                'C0346===
                If _DBversion Is Nothing Then _DBversion = GetDBVersion(StorageType, ConnectionString, ProviderType, ID) ' D0327 + D0329 + D0376 + D0466
                'C0346==
                Return _DBversion
            End Get
        End Property
        ' D0149 ==

        ' D1429 ===
        Public Sub DBVersionReset()
            _DBversion = Nothing
        End Sub
        ' D1429 ==

        ' D0622 ===
        Public Property IgnoreDBVersion() As Boolean    ' D5032
            Get
                Return _DBUpdated
            End Get
            Set(ByVal value As Boolean)
                _DBUpdated = value
            End Set
        End Property
        ' D0622 ==

        ' D0144 ===
        Public ReadOnly Property isValidDBVersion() As Boolean
            Get
                Return IsEqualCanvasDBVersions(DBVersion, GetCurrentDBVersion)  ' D0149
            End Get
        End Property
        ' D0144 ==

        ' D1429 ===
        Public ReadOnly Property isDBVersionCanBeUpdated() As Boolean
            Get
                Return DBVersion IsNot Nothing AndAlso DBVersion.MajorVersion * 10000 + DBVersion.MinorVersion <= GetCurrentDBVersion.MajorVersion * 10000 + GetCurrentDBVersion.MinorVersion
            End Get
        End Property
        ' D1429 ==

        ' D0183 ===
        Public Property isLoadOnDemand() As Boolean
            Get
                Return _isLoadOnDemand
            End Get
            Set(ByVal value As Boolean)
                _isLoadOnDemand = value
            End Set
        End Property
        ' D0183 ==

        ' D0245 ===
        Public Property isForceAllowedAlts() As Boolean
            Get
                Return _isForceAllowedAlts
            End Get
            Set(ByVal value As Boolean)
                _isForceAllowedAlts = value
            End Set
        End Property
        ' D0245 ==

        ' D4413 ===
        Private Function WordingTemplatesEncode(sLst As Dictionary(Of String, ProjectWordingTemplate)) As String    ' D4575
            Dim sData As String = ""
            If sLst IsNot Nothing AndAlso sLst.Count > 0 Then
                For Each sKey As String In sLst.Keys
                    sData += String.Format("{0}{1}{2}{1}{3}{4}", sKey, vbTab, sLst(sKey).Value.Replace(vbTab, " "), If(sLst(sKey).InUse, 1, 0), vbNewLine)  ' D4575
                Next
            End If
            Return sData
        End Function

        Private Function WordingTemplatesDecode(sData As String) As Dictionary(Of String, ProjectWordingTemplate)   ' D4575
            Dim sLst As New Dictionary(Of String, ProjectWordingTemplate)   ' D4575
            If Not String.IsNullOrEmpty(sData) Then
                Dim sLines As String() = sData.Split(CChar(vbNewLine))
                For Each sLine As String In sLines
                    Dim sRow As String() = sLine.Trim.Split(CChar(vbTab))
                    If sRow.Length > 1 Then
                        ' D4575 ===
                        Dim WTpl As New ProjectWordingTemplate()
                        Dim sKey As String = sRow(0).Trim.ToLower
                        WTpl.Name = sKey
                        WTpl.Value = sRow(1).Trim
                        If sRow.Length > 2 Then WTpl.InUse = sRow(1) <> "0"
                        If Not sLst.ContainsKey(sKey) Then sLst.Add(sKey, WTpl)
                        ' D4575 ==
                    End If
                Next
            End If
            Return sLst
        End Function

        Public Property WordingTemplates As Dictionary(Of String, ProjectWordingTemplate)   ' D4575
            Get
                'If _WordingTpls Is Nothing OrElse _WordingTpls.Count = 0 AndAlso isValidDBVersion Then
                If _WordingTpls Is Nothing AndAlso isValidDBVersion Then    ' D7406
                    _WordingTpls = WordingTemplatesDecode(ProjectManager.Parameters.WordingTemplatesData)
                End If
                Return _WordingTpls
            End Get
            Set(value As Dictionary(Of String, ProjectWordingTemplate))
                _WordingTpls = value
            End Set
        End Property

        Public Function SaveWordingTemplates(Optional fSaveSnapshot As Boolean = True, Optional sSnapshotComment As String = "") As Boolean
            Dim fResult As Boolean = False
            If isValidDBVersion Then
                Dim sData As String = WordingTemplatesEncode(WordingTemplates)
                If ProjectManager.Parameters.WordingTemplatesData <> sData Then
                    ProjectManager.Parameters.WordingTemplatesData = sData
                    fResult = ProjectManager.Parameters.Save()
                    If fResult AndAlso fSaveSnapshot Then If onProjectSaved IsNot Nothing Then onProjectSaved.Invoke(Me, "Update wording template", False, sSnapshotComment)
                    If fResult AndAlso onProjectUpdateLastModify IsNot Nothing Then onProjectUpdateLastModify.Invoke(Me) ' D4535
                Else
                    fResult = True
                End If
            End If
            Return fResult
        End Function
        ' D4413 ==

        Public Sub MakeSnapshot(sLogMsg As String, sSnapshotComment As String)
            If Not String.IsNullOrEmpty(sLogMsg) Then Me.onProjectSaved.Invoke(Me, sLogMsg, False, sSnapshotComment)
        End Sub

        Public Function SaveStructure(Optional sMessage As String = "", Optional fIsCritical As Boolean = False, Optional fSaveSnapshot As Boolean = True, Optional sSnapshotComment As String = "") As Boolean ' D3572 + D3611 + D3731
            DebugInfo("Start saving project structure...")
            If sMessage = "" Then sMessage = "Save Project Changes" ' D3571
            If fSaveSnapshot AndAlso onProjectSaving IsNot Nothing Then onProjectSaving.Invoke(Me, sMessage, fIsCritical, sSnapshotComment) ' D3571 + D3572 + D3611 + D3731

            ' -D3576 == remove due to auto-upgrade
            'ProjectManager.StorageManager.ProviderType = ProviderType   ' D0329
            'ProjectManager.StorageManager.ProjectLocation = ConnectionString    'C0028 + D0327 + D0329
            'ProjectManager.StorageManager.StorageType = StorageType     'C0271 + D0369 + D0376
            'ProjectManager.StorageManager.CanvasDBVersion = ProjectManager.StorageManager.CurrentCanvasDBVersion 'C0028
            'ProjectManager.StorageManager.ModelID = ID 'C0271
            ' -D3576 ==

            ProjectManager.PipeBuilder.PipeCreated = False  ' D0274
            'ResetUserEvaluations()  ' D0308
            Dim fResult As Boolean = ProjectManager.StorageManager.Writer.SaveProject(True) ' D0069 'C0028 'C0085
            If fResult AndAlso fSaveSnapshot Then If onProjectSaved IsNot Nothing Then onProjectSaved.Invoke(Me, sMessage, fIsCritical, sSnapshotComment) ' D3571 + D3572 + D3611 + D3731
            If fResult AndAlso onProjectUpdateLastModify IsNot Nothing Then onProjectUpdateLastModify.Invoke(Me) ' D4535
            DebugInfo(String.Format("Project structure saved ({0})", fResult))  ' D0466
            Return fResult
        End Function
        ' D0010 ==

        ' D0127 ===
        Public Sub SaveProjectOptions(Optional sMessage As String = "", Optional fIsCritical As Boolean = False, Optional fSaveSnapshot As Boolean = True, Optional sSnapshotComment As String = "")    ' D3572 + D3611 + D3731
            DebugInfo("Save PipeParameters...")
            If sMessage = "" Then sMessage = "Save Project Options" ' D3571
            If fSaveSnapshot AndAlso onProjectSaving IsNot Nothing Then onProjectSaving.Invoke(Me, sMessage, fIsCritical, sSnapshotComment) ' D3571 + D3572 + D3611 + D3731
            ' D0174 ===
            PipeParameters.ProjectName = ProjectName
            PipeParameters.ProjectPurpose = Comment
            ' D0174 ==
            If ProjectGUID <> Guid.Empty AndAlso _PipeParameters.ProjectGuid = Guid.Empty Then PipeParameters.ProjectGuid = ProjectGUID ' D0891 + D0892
            ' D0183 ===
            Dim fResult As Boolean = False
            If ID <> 0 Then ' D4757
                If _ProjectManager Is Nothing Then
                    fResult = PipeParameters.Write(PipeStorageType, ConnectionString, ProviderType, ID) 'C0271 + D0376 + D0466
                Else
                    ProjectManager.Parameters.Save()    ' D3786
                    ProjectManager.SavePipeParameters(PipeStorageType, ID) 'C0271 + D0376
                    ProjectManager.PipeBuilder.PipeCreated = False  ' D0274
                    fResult = True
                End If
            End If
            ' D0183 ==
            If fResult AndAlso fSaveSnapshot Then If onProjectSaved IsNot Nothing Then onProjectSaved.Invoke(Me, sMessage, fIsCritical, sSnapshotComment) ' D3571 + D3572 + D3611 + D3731
            If fResult AndAlso onProjectUpdateLastModify IsNot Nothing Then onProjectUpdateLastModify.Invoke(Me) ' D4535
            DebugInfo(String.Format("PipeParameters saved ({0})", fResult))  ' D0466
        End Sub
        ' D0127 ==

        ' D3572 ===
        Public Function SaveUsersInfo(Optional tUser As clsUser = Nothing, Optional sMessage As String = "", Optional fIsCritical As Boolean = False, Optional fSaveSnapshot As Boolean = True, Optional sSnapshotComment As String = "") As Boolean  ' D3611 + D3731
            DebugInfo("Start saving users info...")
            If sMessage = "" Then sMessage = "Update Users Info"
            If fSaveSnapshot AndAlso onProjectSaving IsNot Nothing Then onProjectSaving.Invoke(Me, sMessage, fIsCritical, sSnapshotComment) ' D3611 + D3731
            Dim fResult As Boolean = ProjectManager.StorageManager.Writer.SaveModelStructure()
            If fResult Then
                ProjectManager.PipeBuilder.PipeCreated = False
                'ResetUserEvaluations()
                If fSaveSnapshot AndAlso onProjectSaved IsNot Nothing Then onProjectSaved.Invoke(Me, sMessage, fIsCritical, sSnapshotComment) ' D3611 + D3731
                If onProjectUpdateLastModify IsNot Nothing Then onProjectUpdateLastModify.Invoke(Me) ' D4535
            End If
            DebugInfo(String.Format("Users Info saved ({0})", fResult))
            Return fResult
        End Function

        'Public Function SaveInfodocs(Optional sMessage As String = "") As Boolean
        '    DebugInfo("Start saving users info...")
        '    If sMessage = "" Then sMessage = "Update Users Info"
        '    If onProjectSaving IsNot Nothing Then onProjectSaving.Invoke(Me, sMessage)
        '    Dim fResult As Boolean = ProjectManager.StorageManager.Writer.SaveInfoDocs()
        '    If fResult Then If onProjectSaved IsNot Nothing Then onProjectSaved.Invoke(Me, sMessage)
        '    DebugInfo(String.Format("Users Info saved ({0})", fResult))
        '    Return fResult
        'End Function
        ' D3572 ==

        ' D3578 ===
        Public Function SaveRA(Optional sMessage As String = "", Optional fIsCritical As Boolean = False, Optional fSaveSnapshot As Boolean = True, Optional sSnapshotComment As String = "") As Boolean  ' D3611 + D3731
            DebugInfo("Start saving RA data...")
            If sMessage = "" Then sMessage = "Save Resource Aligner data"
            If fSaveSnapshot AndAlso onProjectSaving IsNot Nothing Then onProjectSaving.Invoke(Me, sMessage, fIsCritical, sSnapshotComment) ' D3611 + D3731
            Dim fResult As Boolean = ProjectManager.ResourceAligner.Save()
            If fResult AndAlso fSaveSnapshot Then If onProjectSaved IsNot Nothing Then onProjectSaved.Invoke(Me, sMessage, fIsCritical, sSnapshotComment) ' D3611 + D3731
            If fResult AndAlso onProjectUpdateLastModify IsNot Nothing Then onProjectUpdateLastModify.Invoke(Me) ' D4535
            DebugInfo(String.Format("RA data saved ({0})", fResult))
            Return fResult
        End Function
        ' D3578 ==

        ' D0011 ===
        Public Sub ResetProject(Optional ByVal fOnlyProjectManager As Boolean = False)  ' D0021 + D0209
            If Not _ProjectManager Is Nothing Then
                DebugInfo("Close Project Manager")
                _LastHierarchyID = _ProjectManager.ActiveHierarchy  ' D1411
                '_ProjectManager.CloseProject() ' D0183
            End If
            _ProjectManager = Nothing   ' D0053

            'ResetUserEvaluations()  ' D0307
            If Not fOnlyProjectManager Then ' D0209
                _PipeParameters = Nothing   ' D0174
                _isDataChecked = False      ' D0063
                _HasData = False            ' D0063
                _DBversion = Nothing        ' D0249
            End If
            _WordingTpls = Nothing          ' D4413
            DebugInfo("Project Reset")
        End Sub
        ' D0011 ==

        ' D0892 ===
        Public Function CheckGUID() As Boolean
            Dim fUpdated As Boolean = False
            If ProjectGUID = Guid.Empty Then
                ProjectGUID = Guid.NewGuid
                fUpdated = True
            End If
            If IsProjectLoaded OrElse isValidDBVersion Then ' D2192
                If PipeParameters.ProjectGuid = Guid.Empty Or ProjectGUID <> PipeParameters.ProjectGuid Then    ' D0988
                    DebugInfo("Init ProjectGUID", _TRACE_INFO)
                    PipeParameters.ProjectGuid = ProjectGUID
                    SaveProjectOptions(, , False)   ' D3611
                    fUpdated = True
                End If
            End If
            Return fUpdated
        End Function
        ' D0892 ==

        ' D0308 ===
        'Public Property UserEvaluations() As List(Of clsUserEvaluationData) ' D0464
        '    Get
        '        If _UserEvaluationCache Is Nothing Then _UserEvaluationCache = New List(Of clsUserEvaluationData) ' D0464
        '        Return _UserEvaluationCache
        '    End Get
        '    Set(ByVal value As List(Of clsUserEvaluationData))  ' D0464
        '        _UserEvaluationCache = value
        '    End Set
        'End Property
        '' D0308 ==

        '' D0307 ===
        'Public Sub ResetUserEvaluations()
        '    _UserEvaluationCache = Nothing  ' D0308
        'End Sub
        ' D0307 ==

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

        Public Property LastVisited() As Nullable(Of DateTime)
            Get
                Return _LastVisited
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                _LastVisited = value
            End Set
        End Property
        ' D0494 ==

        ' D0043 ===
        Public Function Clone() As clsProject
            Dim newProject As New clsProject(Me.isLoadOnDemand, Me.isForceAllowedAlts, Me._UserEmail, Me.IsRisk, Me.onProjectSaving, Me.onProjectSaved, Me.UseDataMapping) ' D0183 + D0245 + D2255 + D3571 + D4465
            newProject.ID = Me.ID
            newProject.OwnerID = Me.OwnerID     ' D0086
            newProject.WorkgroupID = Me.WorkgroupID     ' D0045
            newProject.PasscodeLikelihood = Me.PasscodeLikelihood   ' D1709
            newProject._PasscodeImpact = Me._PasscodeImpact         ' D1709
            ' D0329 ===
            newProject.ProviderType = Me.ProviderType
            newProject.ConnectionString = Me.ConnectionString
            ' D0329 ==
            newProject.ProjectName = Me.ProjectName
            'newProject.FileName = Me.FileName  ' -D1193
            newProject.StatusDataImpact = Me.StatusDataImpact ' D0062 + D1944
            newProject.StatusDataLikelihood = Me.StatusDataLikelihood  ' D1944
            newProject.Comment = Me.Comment
            newProject.MeetingIDLikelihood = Me.MeetingIDLikelihood(True)   ' D0420 + D1709
            newProject.MeetingIDImpact = Me.MeetingIDImpact(True)           ' D1709
            newProject.IgnoreDBVersion = Me.IgnoreDBVersion ' D0622
            newProject.ProjectGUID = Me.ProjectGUID ' D0891
            newProject.ReplacedID = Me.ReplacedID   ' D0893
            'newProject.MeetingOwnerID = Me.MeetingOwnerID   ' D0487
            If Not Me.MeetingOwner Is Nothing Then newProject.MeetingOwner = Me.MeetingOwner.Clone ' D0487 + D0496
            If Not Me.LockInfo Is Nothing Then newProject.LockInfo = Me.LockInfo.Clone ' D0483
            newProject.Created = Me.Created         ' D0494
            newProject.LastModify = Me.LastModify   ' D0494
            newProject.LastVisited = Me.LastVisited ' D0494
            newProject._IsRisk = Me._IsRisk         ' D2646
            newProject._LastHierarchyID = Me._LastHierarchyID   ' D1411
            newProject.ProjectTypeData = Me.ProjectTypeData     ' D3438 + D4978
            Return newProject
        End Function
        ' D0043 ==

        ' D0305 ===
        'Public Function GetUserEvaluationByEmail(ByVal sUserEmail As String) As clsUserEvaluationData
        '    Dim sEmail As String = sUserEmail.ToLower
        '    For Each tEval As clsUserEvaluationData In UserEvaluations  ' D0308
        '        If tEval.UserEmail.ToLower = sEmail Then Return tEval
        '    Next
        '    Return Nothing
        'End Function

        'Public Function LoadUserEvaluationData(ByVal sUserEmail As String) As clsUserEvaluationData ' D0324 + D0400 + D0524
        '    Dim OldEvalData As clsUserEvaluationData = GetUserEvaluationByEmail(sUserEmail)
        '    If Not OldEvalData Is Nothing Then UserEvaluations.Remove(OldEvalData) ' D0308
        '    Dim tUserEval As New clsUserEvaluationData(Me, sUserEmail)
        '    UserEvaluations.Add(tUserEval)  ' D0308
        '    Return tUserEval
        'End Function
        '' D0305 ==

        ' D0483 ===
        Public Property LockInfo() As clsProjectLockInfo
            Get
                Return _LockInfo
            End Get
            Set(ByVal value As clsProjectLockInfo)
                _LockInfo = value
            End Set
        End Property
        ' D0483 ==

        ''' <summary>
        ''' Create object for new Project.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(ByVal fLoadOnDemand As Boolean, ByVal fForceAllowedAlts As Boolean, ByVal sUserEmail As String, fIsRisk As Boolean, Optional onProjectSavingEvent As onProjectEvent = Nothing, Optional onProjectSavedEvent As onProjectEvent = Nothing, Optional fUseDataMapping As Boolean = False, Optional onProjectLastModifyEvent As onProjectDateTimeEvent = Nothing)    ' D0245 + D2254 + D3571 + D4465 + D4535
            _ID = 0
            _IsRisk = fIsRisk       ' D2255
            _GUID = Guid.Empty      ' D0891
            _OwnerID = 0            ' D0086
            _WorkgroupID = -1       ' D0045
            _PasscodeLikelihood = ""    ' D1709
            _PasscodeImpact = ""        ' D1709
            _ReplacedID = -1        ' D0893
            _ProjectName = ""
            '_Filename = ""         ' -D1193
            _isLoadOnDemand = fLoadOnDemand ' D0183
            _isForceAllowedAlts = fForceAllowedAlts ' D0245
            _UserEmail = sUserEmail     ' D0183
            ' D0329 ===
            _DBConnectionString = ""
            _DBProviderType = DBProviderType.dbptSQLClient
            ' D0329 ==
            ' D0045 + D0964 ===
            _ProjectStatusImpact = SetValueByMask(0, EncodingVersion, mask_EncodingVersion, bits_EncodingVersion)   ' D1944
            _ProjectStatusLikelihood = _ProjectStatusImpact ' D1944
            isOnline = False
            isPublic = True ' D3021
            isTeamTime = False
            isMarkedAsDeleted = False
            ProjectStatus = ecProjectStatus.psActive
            ProjectTypeData = ProjectType.ptRegular    ' D3438 + D4978
            ' D0045 + D0964 ==
            _Comment = ""               ' D0010
            _MeetingIDLikelihood = -1   ' D0420 + D1709
            _MeetingIDImpact = -1       ' D1709
            '_MeetingOwnerID = -1        ' D0487
            _MeetingOwner = Nothing     ' D0487
            _ProjectManager = Nothing   ' D0045
            _isDataChecked = False      ' D0063
            _HasData = False            ' D0063
            _DBversion = Nothing        ' D0149
            _DBUpdated = False          ' D0622
            _LockInfo = New clsProjectLockInfo  ' D0483
            '_UserEvaluationCache = Nothing  ' D0305 + D0308
            _Created = Nothing      ' D0494
            _LastModify = Nothing   ' D0494
            _LastVisited = Nothing  ' D0494
            UseDataMapping = fUseDataMapping    ' D4465
            If onProjectSavingEvent IsNot Nothing Then onProjectSaving = onProjectSavingEvent ' D3571
            If onProjectSavedEvent IsNot Nothing Then onProjectSaved = onProjectSavedEvent ' D3571
            If onProjectLastModifyEvent IsNot Nothing Then onProjectUpdateLastModify = onProjectLastModifyEvent ' D4535
        End Sub

        ' D0465 ===
        Public Shared Function ProjectByID(ByVal tProjectID As Integer, ByVal tProjectsList As List(Of clsProject)) As clsProject
            If Not tProjectsList Is Nothing Then
                For Each tPrj As clsProject In tProjectsList
                    If tPrj IsNot Nothing AndAlso tPrj.ID = tProjectID Then Return tPrj ' D0604
                Next
            End If
            Return Nothing
        End Function

        ' D0891 ===
        Public Shared Function ProjectsByGUID(ByVal tGUID As Guid, ByVal tProjectsList As List(Of clsProject), Optional ByVal fCheckMarkedAsDeleted As Boolean = False) As List(Of clsProject)   ' D0892
            Dim tList As New List(Of clsProject)    ' D0892
            If Not tProjectsList Is Nothing Then
                For Each tPrj As clsProject In tProjectsList
                    If tPrj IsNot Nothing AndAlso tPrj.ProjectGUID = tGUID AndAlso (Not fCheckMarkedAsDeleted Or Not tPrj.isMarkedAsDeleted) Then tList.Add(tPrj) ' D0892
                Next
            End If
            Return tList        ' D0892
        End Function
        ' D0891 ==

        Public Shared Function ProjectByPasscode(ByVal sPasscode As String, ByVal tProjectsList As List(Of clsProject)) As clsProject
            If Not tProjectsList Is Nothing Then
                Dim sCode As String = sPasscode.Trim.ToLower
                For Each tPrj As clsProject In tProjectsList
                    If tPrj IsNot Nothing AndAlso (tPrj.PasscodeLikelihood.ToLower = sPasscode OrElse tPrj.PasscodeImpact.ToLower = sPasscode) Then Return tPrj ' D0604 + D1724
                Next
            End If
            Return Nothing
        End Function

        Public Overloads Shared Function ProjectByMeetingID(ByVal tMeetingID As Long, ByVal tProjectsList As List(Of clsProject)) As clsProject ' D0466
            If Not tProjectsList Is Nothing Then
                For Each tPrj As clsProject In tProjectsList
                    If tPrj IsNot Nothing AndAlso (tPrj.MeetingIDImpact = tMeetingID OrElse tPrj.MeetingIDLikelihood = tMeetingID) Then Return tPrj ' D0604 + D1944
                Next
            End If
            Return Nothing
        End Function

        Public Overloads Shared Function ProjectByMeetingID(ByVal sMeetingID As String, ByVal tProjectsList As List(Of clsProject)) As clsProject
            Dim tMeetingID As Long = -1 ' D0466
            If clsMeetingID.TryParse(sMeetingID, tMeetingID) Then Return ProjectByMeetingID(tMeetingID, tProjectsList) ' D0466
            Return Nothing
        End Function
        ' D0465 ==

    End Class

End Namespace
