Namespace ExpertChoice.Data

    Public Enum ecActionType
        atUnspecified = -1

        at_alCreateNewModel = 1
        at_alUploadModel = 2
        ' D0087 ===
        at_alDeleteAnyModel = 4
        at_alManageAnyModel = 5

        at_alViewAllModels = 10 ' D2780
        at_alCanBePM = 11       ' D2780

        ' -D2780
        'at_alManageSurveys = 30 ' D0190
        'at_alStartTTAssistance = 35 ' D0258

        at_alManageWorkgroupRights = 40 ' D0091
        at_alManageWorkgroupUsers = 41  ' D0727
        at_alHideWorkgroupMembers = 49  ' D1014


        at_slCreateWorkgroup = 50
        at_slManageAnyWorkgroup = 51
        at_slManageOwnWorkgroup = 52
        at_slDeleteAnyWorkgroup = 55
        at_slDeleteOwnWorkgroup = 56

        at_slManageAllUsers = 61        ' D0091

        at_slViewLicenseAnyWorkgroup = 70
        at_slViewLicenseOwnWorkgroup = 71
        at_slSetLicenseAnyWorkgroup = 75
        at_slSetLicenseOwnWorkgroup = 76

        at_slViewAnyWorkgroupReports = 80
        at_slViewOwnWorkgroupReports = 81

        at_slViewAnyWorkgroupLogs = 90
        at_slViewOwnWorkgroupLogs = 91
        ' D0087 ==

        at_mlModifyModelHierarchy = 100
        at_mlModifyAlternativeHierarchy = 101
        at_mlModifyAltsContributesTo = 110
        at_mlModifyMeasurementScales = 111
        at_mlSetSpecificRolesViewing = 120
        at_mlSetSpecificRolesEvaluation = 121
        at_mlDownloadModel = 130
        at_mlDeleteModel = 145  ' D0052
        at_mlEvaluateModel = 150
        at_mlPerformSensitivityAnalysis = 151
        at_mlViewOverallResults = 165
        at_mlViewModel = 170
        at_mlUsePredefinedReports = 181
        at_mlHideWorkgroupMembers = 189     ' D0967
        at_mlManageModelUsers = 190
        at_mlManageProjectOptions = 191

    End Enum

    Public Enum ecActionStatus
        asUnspecified = -2
        asRestricted = -1
        asGranted = 0
    End Enum

    ' D0089 + D0093 ===
    Public Enum ecPageAccessType
        paUnspecified = -1
        paEveryone = 0
        paOnlySystem = 1
        paOnlyProjects = 2
    End Enum
    ' D0089 + D0093 ==

    ' D2256 ===
    Public Enum ecPageRiskAccessType
        raAlways = 0
        raNoRisk = 1
        raRiskOnly = 2
    End Enum
    ' D2256 ==

    <Serializable()> Public Class clsRoleAction

        Private _mID As Integer
        Private _mType As ecActionType
        Private _mStatus As ecActionStatus
        Private _mInherited As Boolean
        Private _mRoleGroupID As Integer    ' D0045
        Private _mComment As String

        Public Property ID() As Integer
            Get
                Return _mID
            End Get
            Set(ByVal value As Integer)
                _mID = value
            End Set
        End Property

        Public Property ActionType() As ecActionType
            Get
                Return _mType
            End Get
            Set(ByVal value As ecActionType)
                _mType = value
            End Set
        End Property

        Public Property Status() As ecActionStatus
            Get
                Return _mStatus
            End Get
            Set(ByVal value As ecActionStatus)
                _mStatus = value
            End Set
        End Property

        ' D0045 ===
        Public Property RoleGroupID() As Integer
            Get
                Return _mRoleGroupID
            End Get
            Set(ByVal value As Integer)
                _mRoleGroupID = value
            End Set
        End Property
        ' D0045 ==

        Public Property isInherited() As Boolean
            Get
                Return _mInherited
            End Get
            Set(ByVal value As Boolean)
                _mInherited = value
            End Set
        End Property

        Public Property Comment() As String
            Get
                Return _mComment
            End Get
            Set(ByVal value As String)
                _mComment = value
            End Set
        End Property

        Public Function Clone() As clsRoleAction
            Dim newRoleAction As New clsRoleAction
            newRoleAction.ID = Me.ID
            newRoleAction.ActionType = Me.ActionType
            newRoleAction.Status = Me.Status
            newRoleAction.RoleGroupID = Me.RoleGroupID  ' D0045
            newRoleAction.isInherited = Me.isInherited
            newRoleAction.Comment = Me.Comment
            Return newRoleAction
        End Function

        Public Sub New()
            _mID = 0
            _mType = ecActionType.atUnspecified
            _mStatus = ecActionStatus.asUnspecified
            _mRoleGroupID = -1  ' D0045
            _mInherited = False
            _mComment = ""
        End Sub

    End Class

End Namespace