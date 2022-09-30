Namespace ExpertChoice.Data

    Public Enum ecPagePermission
        ppUnspecified = -1
        ppEveryone = 0
        ppUnAuthorized = 1
        ppAuthorized = 2
        ppProject = 3
        ppProjectWithLock = 4   ' D0134
        ' -D4940 ===
        'ppProjectNotEmpty = 5    ' D0097 + D0589
        'ppProjectNotEmptyWithLock = 6    ' D0134 + D0589
        'ppProjectWithStructuringLock = 7    ' D0589
        'ppNoAvailableProjects = 9   ' D0060 + D0097
        ' -D4940 ==
    End Enum

    Public Enum ecRoleLevel ' D0457
        rlUndefined = -1    ' D0064
        rlApplicationLevel = 0
        rlModelLevel = 1
        rlSystemLevel = 9   ' D0087
    End Enum

    ' D0064 ===
    Public Module Actions

        Public Function ActionRoleLevel(ByVal tActionType As ecActionType) As ecRoleLevel
            Select Case tActionType
                Case ecActionType.atUnspecified
                    Return ecRoleLevel.rlUndefined
                Case ecActionType.at_alCreateNewModel, _
                     ecActionType.at_alDeleteAnyModel, _
                     ecActionType.at_alManageAnyModel, _
                     ecActionType.at_alCanBePM, _
                     ecActionType.at_alManageWorkgroupRights, _
                     ecActionType.at_alManageWorkgroupUsers, _
                     ecActionType.at_alHideWorkgroupMembers, _
                     ecActionType.at_alUploadModel  ' D0077 + D0087 + D0091 + D0190 + D0258 + D0727 + D1014 +  D2780 + D4460
                    Return ecRoleLevel.rlApplicationLevel

                    'ecActionType.at_alManageSurveys, _
                    'ecActionType.at_alStartTTAssistance   ' D0077 + D0087 + D0091 + D0190 + D0258 + D0727 + D1014

                    ' D0087 ===
                Case ecActionType.at_slCreateWorkgroup, _
                     ecActionType.at_slDeleteAnyWorkgroup, _
                     ecActionType.at_slDeleteOwnWorkgroup, _
                     ecActionType.at_slManageAllUsers, _
                     ecActionType.at_slManageAnyWorkgroup, _
                     ecActionType.at_slManageOwnWorkgroup, _
                     ecActionType.at_slSetLicenseAnyWorkgroup, _
                     ecActionType.at_slSetLicenseOwnWorkgroup, _
                     ecActionType.at_slViewAnyWorkgroupReports, _
                     ecActionType.at_slViewOwnWorkgroupReports, _
                     ecActionType.at_slViewAnyWorkgroupLogs, _
                     ecActionType.at_slViewOwnWorkgroupLogs, _
                     ecActionType.at_slViewLicenseOwnWorkgroup, _
                     ecActionType.at_slViewLicenseAnyWorkgroup
                    Return ecRoleLevel.rlSystemLevel    ' D0091
                    ' D0087 ==
                Case Else
                    Return ecRoleLevel.rlModelLevel
            End Select
        End Function

    End Module
    ' D0064 ==


    <Serializable()> Public Class clsPageAction

        Private _mID As Integer
        Private _mName As String
        Private _mURL As String
        Private _mPermission As ecPagePermission
        Private _mType As ecActionType
        Private _mPageType As ecPageAccessType          ' D0089 + D0093
        Private _mRiskAccess As ecPageRiskAccessType    ' D2256

        Public Property ID() As Integer
            Get
                Return _mID
            End Get
            Set(ByVal value As Integer)
                _mID = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return _mName
            End Get
            Set(ByVal value As String)
                _mName = value
            End Set
        End Property


        Public Property URL() As String
            Get
                Return _mURL
            End Get
            Set(ByVal value As String)
                _mURL = value
            End Set
        End Property

        Public Property Permission() As ecPagePermission
            Get
                Return _mPermission
            End Get
            Set(ByVal value As ecPagePermission)
                _mPermission = value
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

        ' D0052 ===
        Public ReadOnly Property RoleLevel() As ecRoleLevel
            Get
                Return ActionRoleLevel(ActionType)  ' D0064
            End Get
        End Property
        ' D0052 ==

        ' D0089 ===
        Public Property PageType() As ecPageAccessType
            Get
                Return _mPageType
            End Get
            Set(ByVal value As ecPageAccessType)
                _mPageType = value
            End Set
        End Property
        ' D0089 ==

        ' D2256 ===
        Public Property RiskAccess() As ecPageRiskAccessType
            Get
                Return _mRiskAccess
            End Get
            Set(ByVal value As ecPageRiskAccessType)
                _mRiskAccess = value
            End Set
        End Property
        ' D2256 ==

        Public Function Clone() As clsPageAction
            Dim newAction As New clsPageAction
            newAction.ID = Me.ID
            newAction.Name = Me.Name
            newAction.URL = Me.URL
            newAction.Permission = Me.Permission
            newAction.ActionType = Me.ActionType
            newAction.PageType = Me.PageType        ' D0089
            newAction.RiskAccess = Me.RiskAccess    ' D2256
            Return newAction
        End Function

        Public Sub New(Optional ByVal tID As Integer = -1, Optional ByVal sName As String = "", Optional ByVal sURL As String = "", Optional ByVal tPagePermission As ecPagePermission = ecPagePermission.ppUnspecified, Optional ByVal tActionType As ecActionType = ecActionType.atUnspecified, Optional ByVal tPageType As ecPageAccessType = ecPageAccessType.paUnspecified, Optional tRiskAccess As ecPageRiskAccessType = ecPageRiskAccessType.raAlways) ' D0089 + D0093 + D2256
            _mID = tID
            _mName = sName
            _mURL = sURL
            _mPermission = tPagePermission
            _mType = tActionType
            _mPageType = tPageType   ' D0089
            _mRiskAccess = tRiskAccess  ' D2256
        End Sub

    End Class

End Namespace
