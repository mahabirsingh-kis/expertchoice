Imports System.IO
Imports ExpertChoice.Service
Imports System.Linq

Namespace ExpertChoice.Data

    Public Enum ecWorkgroupStatus
        wsDisabled = -1
        wsEnabled = 0
        wsSystem = 128  ' D0087
    End Enum

    <Serializable()> Public Class clsWorkgroup

        Private _ID As Integer
        Private _OwnerID As Integer ' D0086
        Private _ECAMID As Integer ' D0091
        Private _Name As String
        Private _Status As ecWorkgroupStatus
        Private _RoleGroups As List(Of clsRoleGroup)   ' D0464
        Private _Comment As String
        Private _License As clsLicense      ' D0261
        Private _Created As Nullable(Of DateTime)   ' D0494
        Private _LastModify As Nullable(Of DateTime)    ' D0494
        Private _LastVisited As Nullable(Of DateTime)   ' D0494
        Private _EULAFile As String     ' D0922
        Private _LifeTimeProjects As Integer    ' D0922
        Private _WordingTemplates As Dictionary(Of String, String)  ' D2384
        Private _StarredProjectsIDsByUserEmail As New Dictionary(Of String, List(Of Integer)) 'A1715
        Private _OpportunityID As String    ' D3333

        Public _WKG_OLD_BEFORE_DATE As New Date(2015, 3, 24) ' D2772 + D2821 + D2894

        Public Property ID() As Integer
            Get
                Return _ID
            End Get
            Set(ByVal value As Integer)
                _ID = value
            End Set
        End Property

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

        ' D0091 ===
        Public Property ECAMID() As Integer
            Get
                Return _ECAMID
            End Get
            Set(ByVal value As Integer)
                _ECAMID = value
            End Set
        End Property
        ' D0091 ==

        Public Property Name() As String
            Get
                Return _Name
            End Get
            Set(ByVal value As String)
                _Name = SubString(value.Trim, 250)  ' D0916 + D0990
            End Set
        End Property

        Public Property Status() As ecWorkgroupStatus
            Get
                Return _Status
            End Get
            Set(ByVal value As ecWorkgroupStatus)
                _Status = value
            End Set
        End Property

        ' D3333 ===
        Public Property OpportunityID() As String
            Get
                Return _OpportunityID
            End Get
            Set(ByVal value As String)
                _OpportunityID = SubString(value.Trim, 250)
            End Set
        End Property
        ' D3333 ==

        Public ReadOnly Property GetDefaultRoleGroupID(ByVal tRoleLevel As ecRoleLevel, ByVal tGroupType As ecRoleGroupType) As Integer  ' D0046 + D0052
            Get
                ' D0052 ===
                Dim tStatus As ecRoleGroupStatus
                Select Case tGroupType
                    Case ecRoleGroupType.gtEvaluator
                        tStatus = ecRoleGroupStatus.gsEvaluatorDefault
                    Case ecRoleGroupType.gtViewer   ' D2239
                        tStatus = ecRoleGroupStatus.gsViewerDefault ' D2239
                    Case ecRoleGroupType.gtEvaluatorAndViewer  ' D2857
                        tStatus = ecRoleGroupStatus.gsEvaluatorAndViewerDefault  ' D2857
                        ' D2780 ===
                    Case ecRoleGroupType.gtProjectManager
                        tStatus = ecRoleGroupStatus.gsProjectManagerDefault
                    Case ecRoleGroupType.gtProjectOrganizer
                        tStatus = ecRoleGroupStatus.gsProjectOrganizerDefault
                    Case ecRoleGroupType.gtUser
                        tStatus = ecRoleGroupStatus.gsUserDefault
                        ' D0087 ===
                    Case ecRoleGroupType.gtWorkgroupManager
                        tStatus = ecRoleGroupStatus.gsWorkgroupManagerDefault
                    Case ecRoleGroupType.gtECAccountManager
                        tStatus = ecRoleGroupStatus.gsECAccountManagerDefault
                        'Case ecRoleGroupType.gtTechSupport
                        '    tStatus = ecRoleGroupStatus.gsTechSupportDefault
                        ' D2780 ==
                    Case ecRoleGroupType.gtAdministrator
                        tStatus = ecRoleGroupStatus.gsAdministratorDefault
                        ' D0087 ==
                End Select
                For Each tG As clsRoleGroup In RoleGroups
                    If tG IsNot Nothing AndAlso tG.RoleLevel = tRoleLevel AndAlso tG.Status = tStatus Then Return tG.ID ' D0604
                Next
                Return RoleGroupID(tGroupType)
            End Get
        End Property

        Public Property RoleGroups() As List(Of clsRoleGroup) ' D0464
            Get
                Return _RoleGroups
            End Get
            Set(ByVal value As List(Of clsRoleGroup)) ' D0464
                _RoleGroups = value
            End Set
        End Property

        Public Overloads ReadOnly Property RoleGroup(ByVal ID As Integer, Optional ByVal GroupsList As List(Of clsRoleGroup) = Nothing) As clsRoleGroup ' D0464
            Get
                If GroupsList Is Nothing Then GroupsList = RoleGroups
                Return GroupsList?.FirstOrDefault(Function(tGroup) (tGroup IsNot Nothing AndAlso tGroup.ID = ID))
                'If Not GroupsList Is Nothing Then
                '    For Each tGroup As clsRoleGroup In GroupsList
                '        If tGroup IsNot Nothing AndAlso tGroup.ID = ID Then ' D0604
                '            Return tGroup
                '        End If
                '    Next
                'End If
                'Return Nothing
            End Get
        End Property

        Public Overloads ReadOnly Property RoleGroup(ByVal GroupType As ecRoleGroupType, Optional ByVal GroupsList As List(Of clsRoleGroup) = Nothing) As clsRoleGroup  ' D0464
            Get
                If GroupsList Is Nothing Then GroupsList = RoleGroups
                If Not GroupsList Is Nothing Then
                    For Each tGroup As clsRoleGroup In GroupsList
                        If tGroup IsNot Nothing AndAlso tGroup.GroupType = GroupType Then   ' D0604
                            Return tGroup
                        End If
                    Next
                End If
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property RoleGroupID(ByVal GroupType As ecRoleGroupType, Optional ByVal GroupsList As List(Of clsRoleGroup) = Nothing) As Integer   ' D0464
            Get
                Dim RG As clsRoleGroup = RoleGroup(GroupType, GroupsList)
                If RG Is Nothing Then Return -1 Else Return RG.ID
            End Get
        End Property

        Public Property Comment() As String
            Get
                Return _Comment
            End Get
            Set(ByVal value As String)
                _Comment = value
            End Set
        End Property


        ' D0922 ===
        Public Property EULAFile() As String
            Get
                Return _EULAFile
            End Get
            Set(ByVal value As String)
                _EULAFile = SubString(value.Trim, 200) ' D0990
            End Set
        End Property

        Public Property LifeTimeProjects() As Integer
            Get
                Return _LifeTimeProjects
            End Get
            Set(ByVal value As Integer)
                _LifeTimeProjects = value
            End Set
        End Property
        ' D0922 ==

        'Public Sub InheriteGroupActions()
        '    If Not RoleGroups Is Nothing Then
        '        For Each tGrp As clsRoleGroup In RoleGroups
        '            If tGrp.ParentID <= 0 Then InheriteRoleGroup(tGrp)
        '        Next
        '    End If
        'End Sub

        'Private Sub InheriteRoleGroup(ByVal tParentGrp As clsRoleGroup)
        '    If Not tParentGrp Is Nothing Then
        '        For Each tGrp As clsRoleGroup In RoleGroups
        '            If tGrp.ParentID = tParentGrp.ID Then
        '                InheriteRoleGroup(tGrp)
        '                For Each tAction As clsRoleAction In tParentGrp.Actions
        '                    If tGrp.Action(tAction.ActionType) Is Nothing Then
        '                        Dim newAction As clsRoleAction = tAction.Clone
        '                        newAction.isInherited = True
        '                        tGrp.Actions.Add(newAction)
        '                    End If
        '                Next
        '            End If
        '        Next
        '    End If
        'End Sub

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

        ' D2384 ===
        Public Property WordingTemplates As Dictionary(Of String, String)
            Get
                Return _WordingTemplates
            End Get
            Set(value As Dictionary(Of String, String))
                _WordingTemplates = value
            End Set
        End Property
        ' D2384 ==

        ' -D5006
        ''A1715 ===
        'Public Property StarredProjectsIDsByUserEmail As Dictionary(Of String, List(Of Integer))
        '    Get
        '        Return _StarredProjectsIDsByUserEmail
        '    End Get
        '    Set(value As Dictionary(Of String, List(Of Integer)))
        '        _StarredProjectsIDsByUserEmail = value
        '    End Set
        'End Property
        ''A1715 ==

        ' D2772 ===
        Public Function IsOldWorkgroup() As Boolean
            Return Not Created.HasValue OrElse Created.Value < _WKG_OLD_BEFORE_DATE ' D2894
        End Function
        ' D2772 ==

        Public Function Clone() As clsWorkgroup
            Dim newWG As New clsWorkgroup
            newWG.ID = Me.ID
            newWG.OwnerID = Me.OwnerID  ' D0086
            newWG.ECAMID = Me.ECAMID    ' D0091
            newWG.Name = Me.Name
            newWG.Status = Me.Status

            ' UNDONE clone License for Workgroup
            'newWG.LicenseKey = Me.LicenseKey
            newWG.Comment = Me.Comment

            newWG.Created = Me.Created          ' D0494
            newWG.LastModify = Me.LastModify    ' D0494
            newWG.LastVisited = Me.LastVisited  ' D0494

            newWG.EULAFile = Me.EULAFile        ' D0922
            newWG.LifeTimeProjects = Me.LifeTimeProjects    ' D0922

            newWG.OpportunityID = Me.OpportunityID  ' D3333

            ' D2384 ===
            newWG.WordingTemplates.Clear()
            For Each sKey In Me.WordingTemplates.Keys
                newWG.WordingTemplates.Add(sKey, Me.WordingTemplates(sKey))
            Next
            ' D2384 ==

            If Not RoleGroups Is Nothing Then
                For Each tRole As clsRoleGroup In RoleGroups
                    newWG.RoleGroups.Add(tRole.Clone)
                Next
            End If
            Return newWG
        End Function

        ' D0261 ===
        ReadOnly Property License() As clsLicense
            Get
                If _License Is Nothing Then
                    _License = New clsLicense()
                    _License.Enabled = False
                End If
                Return _License
            End Get
        End Property

        Public Sub CreateLicense(ByVal tLicenseContent As MemoryStream, ByVal sLicenseKey As String)    ' D0264
            _License = New clsLicense(tLicenseContent, sLicenseKey)     ' D0264
            _License.Enabled = True
        End Sub
        ' D0261 ==

        Public Sub New()
            _ID = 0
            _OwnerID = 0    ' D0086
            _ECAMID = 0     ' D0091
            _Name = ""
            _Status = ecWorkgroupStatus.wsEnabled
            _RoleGroups = New List(Of clsRoleGroup) ' D0464
            _License = Nothing  ' D0261
            _Comment = ""
            _Created = Nothing      ' D0494
            _LastModify = Nothing   ' D0494
            _LastVisited = Nothing  ' D0494
            _EULAFile = ""          ' D0922
            _LifeTimeProjects = 0   ' D0922
            _WordingTemplates = New Dictionary(Of String, String)  ' D2384
            _OpportunityID = ""     ' D3333
        End Sub

        ' D0465 ===
        Public Shared Function WorkgroupByID(ByVal tID As Integer, ByVal tWorkgroupsList As List(Of clsWorkgroup)) As clsWorkgroup
            If Not tWorkgroupsList Is Nothing Then
                For Each tWkg As clsWorkgroup In tWorkgroupsList
                    If tWkg IsNot Nothing AndAlso tWkg.ID = tID Then Return tWkg ' D0604
                Next
            End If
            Return Nothing
        End Function
        ' D0465 ==

        ' D0466 ===
        Public Shared Function WorkgroupNameByID(ByVal tID As Integer, ByVal tWorkgroupsList As List(Of clsWorkgroup)) As String
            If Not tWorkgroupsList Is Nothing Then
                For Each tWkg As clsWorkgroup In tWorkgroupsList
                    If tWkg IsNot Nothing AndAlso tWkg.ID = tID Then Return tWkg.Name ' D0604
                Next
            End If
            Return ""
        End Function
        ' D0466 ==

    End Class

End Namespace